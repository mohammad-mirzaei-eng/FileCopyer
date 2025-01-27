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
        // ایجاد یک نمونه Singleton از FileCopyManager
        private static readonly Lazy<FileCopyManager> _instance = new Lazy<FileCopyManager>(() => new FileCopyManager());
        private IFileCopyStrategy _copyStrategy;
        private CancellationTokenSource _cancellationTokenSource;

        // دیکشنری‌های همزمان برای نگهداری وضعیت فایل‌های در حال کپی و کپی شده
        private ConcurrentDictionary<string, bool> _copyingFiles = new ConcurrentDictionary<string, bool>();
        private ConcurrentDictionary<string, bool> _filesCopied = new ConcurrentDictionary<string, bool>();

        // لیست خطاها
        private List<string> _errorList = new List<string>();

        // دسترسی به نمونه Singleton
        public static FileCopyManager Instance => _instance.Value;

        private CopyProgressNotifier notifier = new CopyProgressNotifier();

        // سازنده خصوصی برای جلوگیری از ایجاد نمونه‌های جدید
        private FileCopyManager() { }

        // متد ثبت ناظر جدید
        /// <summary>
        /// Registers a new observer to receive copy progress notifications
        /// </summary>
        /// <param name="observer">The observer to be registered</param>
        public void RegisterObserver(IProgressObserver observer)
        {
            notifier.AddObserver(observer);
        }

        // متد لغو ثبت ناظر
        /// <summary>
        /// Unregisters an observer from receiving copy progress notifications
        /// </summary>
        /// <param name="observer">The observer to be unregistered</param>
        public void UnregisterObserver(IProgressObserver observer)
        {
            notifier.RemoveObserver(observer);
        }

        // متد عمومی برای شروع عملیات کپی
        /// <summary>
        /// Starts the file copy operation
        /// </summary>
        /// <param name="fileModels">List of FileModel objects to be copied</param>
        /// <param name="flowLayoutPanel">FlowLayoutPanel for displaying progress bars</param>
        /// <param name="pgbtotal">ProgressBar for overall progress</param>
        /// <param name="settings">Optional SettingsModel object for copy settings</param>
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
                        await GenerateErrorReport();
                        if (cancellationToken.IsCancellationRequested)
                        {
                            // عملیات کپی متوقف شده است.
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
        /// <summary>
        /// Generates an error report if there are any errors
        /// </summary>
        /// <returns></returns>
        private async Task GenerateErrorReport()
        {
            if (_errorList.Count > 0)
            {
                string reportBasePath = "Report_";
                string date = DateTime.Now.ToString("yyyyMMdd");

                string reportPath = $"{reportBasePath}{date}.txt"; // مسیر فایل گزارش
                await new GenerateReportHelper().GenerateReport(reportPath, _errorList);
                _errorList.Clear();
            }
        }

        // متد عمومی برای مدیریت وضعیت کپی
        /// <summary>
        /// Updates the copy status of a file
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <param name="isCopying">Boolean indicating whether the file is being copied</param>
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
        /// <summary>
        /// Checks if a file is being copied
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <returns>Boolean indicating whether the file is being copied</returns>
        public bool IsFileBeingCopied(string filePath)
        {
            return _copyingFiles.ContainsKey(filePath);
        }

        // متد عمومی برای بررسی اینکه آیا فایل کپی شده است
        /// <summary>
        /// Checks if a file has been copied
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <returns>Boolean indicating whether the file has been copied</returns>
        public bool IsFileCopied(string filePath)
        {
            return _filesCopied.ContainsKey(filePath);
        }

        /// <summary>
        /// Checks if a file copy is in progress
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <returns>Boolean indicating whether the file copy is in progress</returns>
        public bool IsCopyInProgress(string filePath)
        {
            return _copyingFiles.ContainsKey(filePath);
        }

        /// <summary>
        /// Checks if any file copy is in progress
        /// </summary>
        /// <returns>Boolean indicating whether any file copy is in progress</returns>
        public bool IsCopyingInProgress()
        {
            return _copyingFiles.Count > 0;
        }

        // متد انتظار برای تکمیل کپی
        /// <summary>
        /// Waits for all file copy operations to complete
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Clears the list of files being copied
        /// </summary>
        public void ClearCopyingFiles()
        {
            _copyingFiles.Clear();
        }

        /// <summary>
        /// Clears the list of copied files
        /// </summary>
        public void ClearCopyedFiles()
        {
            _filesCopied.Clear();
        }

        // مدیریت رویداد بستن فرم
        /// <summary>
        /// Handles the form closing event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">FormClosingEventArgs containing event data</param>
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
        /// <summary>
        /// Cancels the file copy operation
        /// </summary>
        public void CancelCopy()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Adds a list of errors to the error list
        /// </summary>
        /// <param name="errors">List of error messages</param>
        public void AddErrors(List<string> errors)
        {
            _errorList.AddRange(errors);
        }

        /// <summary>
        /// Adds a single error to the error list
        /// </summary>
        /// <param name="errors">Error message</param>
        public void AddError(string errors)
        {
            _errorList.Add(errors);
        }

        /// <summary>
        /// Gets the number of errors in the error list
        /// </summary>
        /// <returns>Number of errors</returns>
        public int GetError()
        {
            return _errorList.Count;
        }
    }
}
