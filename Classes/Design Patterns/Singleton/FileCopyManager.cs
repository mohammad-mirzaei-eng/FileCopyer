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

namespace FileCopyer.Classes.Design_Patterns.Singleton
{
    public class FileCopyManager
    {
        private static readonly Lazy<FileCopyManager> _instance = new Lazy<FileCopyManager>(() => new FileCopyManager());
        private readonly List<IFileOperation> _operations = new List<IFileOperation>();
        private IFileCopyStrategy _copyStrategy;
        private CancellationTokenSource _cancellationTokenSource;
        private ConcurrentDictionary<string, bool> _copyingFiles = new ConcurrentDictionary<string, bool>();

        public static FileCopyManager Instance => _instance.Value;

        private ConcurrentDictionary<List<FileModel>, IFileCopyStrategy> activeCopyStrategies = new ConcurrentDictionary<List<FileModel>, IFileCopyStrategy>();
       
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
                activeCopyStrategies.TryAdd(fileModels, strategy);
                _copyStrategy = strategy;

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                _copyStrategy?.CopyFile(fileModels, flowLayoutPanel, _cancellationTokenSource.Token);
            });
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
            }
        }

        public bool IsCopyInProgress(string filePath)
        {
            return _copyingFiles.ContainsKey(filePath);
        }

        public bool IsCopyingInProgress()
        {
            return _copyingFiles.Count > 0;
        }

        public async Task WaitForCopyCompletion()
        {
            while (IsCopyingInProgress())
            {
                await Task.Delay(500);
            }
        }

        // مدیریت رویداد بستن فرم
        public void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsCopyingInProgress())
            {
                DialogResult result = MessageBox.Show("مطمئن هستید میخواهید خروج کنید؟", "اعلان", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    e.Cancel = true;
                    Task.Run(() =>
                    {
                        while (IsCopyingInProgress())
                        {
                            Thread.Sleep(500);
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
            _cancellationTokenSource?.Cancel();
        }
    }
}
