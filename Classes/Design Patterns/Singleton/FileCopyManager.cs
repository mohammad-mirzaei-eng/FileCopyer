using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;
using FileCopyer.Interface;
using System.Threading.Tasks;
using FileCopyer.Interface.Design_Patterns.Strategy;
using FileCopyer.Classes.Design_Patterns.Strategy;
using FileCopyer.Classes.Observer;
using FileCopyer.Interface.Design_Patterns.Observer;
using FileCopyer.Models;
using FileCopyer.Classes.Design_Patterns.Helper;
using System.IO;

namespace FileCopyer.Classes.Design_Patterns.Singleton
{
    public class FileCopyManager
    {
        private static readonly Lazy<FileCopyManager> _instance = new Lazy<FileCopyManager>(() => new FileCopyManager());
        private IFileCopyStrategy _copyStrategy;
        private CancellationTokenSource _cancellationTokenSource;

        private ConcurrentDictionary<string, bool> _copyingFiles = new ConcurrentDictionary<string, bool>();
        private ConcurrentDictionary<string, bool> _filesCopied = new ConcurrentDictionary<string, bool>();

        private List<string> _errorList = new List<string>(); // لیست خطاها

        public static FileCopyManager Instance => _instance.Value;

        //private ConcurrentDictionary<List<FileModel>, IFileCopyStrategy> activeCopyStrategies = new ConcurrentDictionary<List<FileModel>, IFileCopyStrategy>();

        private CopyProgressNotifier notifier = new CopyProgressNotifier();

        private FileCopyManager() { }

        public void RegisterObserver(IProgressObserver observer)
        {
            notifier.AddObserver(observer);
        }

        public void UnregisterObserver(IProgressObserver observer)
        {
            notifier.RemoveObserver(observer);
        }

        // متد عمومی برای شروع عملیات کپی
        public void StartCopy(List<FileModel> fileModels, FlowLayoutPanel flowLayoutPanel, ProgressBar pgbtotal, SettingsModel settings = null)
        {
            if (IsCopyingInProgress())
            {
                MessageBox.Show("فایلی در حال کپی شدن است لطفا کمی صبر کنید تا فایل کپی شود");
                return;
            }

            if (settings == null)
            {
                settings = new SettingsModel();
            }

            var strategy = new DefaultCopyStrategy(settings, notifier);
            //activeCopyStrategies.TryAdd(fileModels, strategy);
            _copyStrategy = strategy;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            Task.Run(async () =>
            {
            try
            {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await _copyStrategy?.CopyFile(fileModels, flowLayoutPanel, _cancellationTokenSource.Token);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            // عملیات کپی متوقف شده است.
                            // شما می‌توانید وضعیت متوقف شده را مدیریت کنید
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // اگر عملیات متوقف شده است، اینجا می‌توانید وضعیت را مدیریت کنید
                }
                catch (Exception ex)
                {
                    // مدیریت خطاها
                    MessageBox.Show($"خطا در عملیات کپی: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // تولید گزارش خطا بعد از پایان عملیات
                    await GenerateErrorReport();
                }
            }, cancellationToken);
        }

        // متد تولید گزارش خطا
        private async Task GenerateErrorReport()
        {
            if (_errorList.Count > 0)
            {
                string reportBasePath = "Report_";
                string date = DateTime.Now.ToString("yyyyMMdd");

                string reportPath = $"{reportBasePath}{date}.txt"; // مسیر فایل گزارش
                await new GenerateReportHelper().GenerateReport(reportPath, _errorList);
            }
        }


        // متد عمومی برای مدیریت وضعیت کپی
        public void UpdateFileCopyStatus(string filePath, bool isCopying)
        {
            if (isCopying)
            {
                _copyingFiles.TryAdd(filePath, true);
            }
            else
            {
                _copyingFiles.TryRemove(filePath, out _);
                _filesCopied.TryAdd(filePath, true); // فایل‌هایی که به پایان رسیدند
            }
        }
        // متد عمومی برای بررسی وضعیت کپی
        public bool IsFileBeingCopied(string filePath)
        {
            return _copyingFiles.ContainsKey(filePath);
        }

        // متد عمومی برای بررسی اینکه آیا فایل کپی شده است
        public bool IsFileCopied(string filePath)
        {
            return _filesCopied.ContainsKey(filePath);
        }

        public bool IsCopyInProgress(string filePath)
        {
            return _copyingFiles.ContainsKey(filePath);
        }

        public bool IsCopyingInProgress()
        {
            return _copyingFiles.Count > 0;
        }

        private async Task WaitForCopyCompletion()
        {
            while (IsCopyingInProgress())
            {
                await Task.Delay(500);
            }
            await Task.Run(async () =>
            {
                // بعد از پایان عملیات کپی، گزارش خطا تولید شود
                await GenerateErrorReport();
            });
        }

        public void ClearCopyingFiles()
        {
            _copyingFiles.Clear();
        }

        // مدیریت رویداد بستن فرم
        public void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsCopyingInProgress())
            {
                DialogResult result = MessageBox.Show("برنامه درحال کپی است می خواهید بعد از اتمام کپی برنامه بسته شود؟", "اعلان", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    e.Cancel = true;
                    Task.Run(async () =>
                    {
                        while (IsCopyingInProgress())
                        {
                           await WaitForCopyCompletion();
                        }

                        Application.Exit();
                    });
                }
                else
                {
                    CancelCopy();
                }
            }
        }

        // لغو عملیات کپی
        public void CancelCopy()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }
        
        public void AddErrors(List<string> errors)
        {
            _errorList.AddRange(errors);
        }
    }
}
