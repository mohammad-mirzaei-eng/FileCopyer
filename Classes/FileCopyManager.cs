using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;
using FileCopyer.Interface;
using System.Threading.Tasks;

namespace FileCopyer.Classes
{
    public class FileCopyManager
    {
        private static readonly Lazy<FileCopyManager> _instance = new Lazy<FileCopyManager>(() => new FileCopyManager());
        private readonly List<IFileOperation> _operations = new List<IFileOperation>();
        private IFileCopyStrategy _copyStrategy;
        private CancellationTokenSource _cancellationTokenSource;
        private ConcurrentDictionary<string, bool> _copyingFiles = new ConcurrentDictionary<string, bool>();

        private FileCopyManager() { }

        public static FileCopyManager Instance => _instance.Value;

        // Add an operation to the list
        public void AddOperation(IFileOperation operation)
        {
            _operations.Add(operation);
        }

        // Get all operations
        public IEnumerable<IFileOperation> GetOperations()
        {
            return _operations;
        }

        // Execute a single operation
        public void ExecuteOperation(IFileOperation operation)
        {
            operation.Execute();
        }

        // Set the copy strategy
        public void SetCopyStrategy(IFileCopyStrategy strategy)
        {
            _copyStrategy = strategy;
        }

        // Start copying files
        public void StartCopy(string sourcePath, string destinationPath, FlowLayoutPanel flowLayoutPanel,ProgressBar pgbtotal)
        {
            if (_copyingFiles.Count > 0)
            {
                MessageBox.Show("فایلی در حال کپی شدن است لطفا کمی صبر کنید تا فایل کپی شود");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                _copyStrategy.CopyFile(sourcePath, destinationPath, flowLayoutPanel, _cancellationTokenSource.Token);
            });
        }

        // Cancel the copy operation
        public void CancelCopy()
        {
            _cancellationTokenSource.Cancel();
        }

        // Check if a file is being copied
        public bool IsCopyInProgress(string filePath)
        {
            return _copyingFiles.ContainsKey(filePath);
        }

        // Add a file to the copying list
        public void AddFileToCopy(string filePath)
        {
            _copyingFiles.TryAdd(filePath, true);
        }

        // Remove a file from the copying list
        public void RemoveFileFromCopy(string filePath)
        {
            _copyingFiles.TryRemove(filePath, out _);
        }

        public bool IsCopyingInProgress()
        {
            return _copyingFiles.Count > 0;
        }

        public async Task WaitForCopyCompletion()
        {
            while (_copyingFiles.Count > 0)
            {
                await Task.Delay(500);
            }
        }


        // Handle the FormClosing event
        public void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_copyingFiles.Count > 0)
            {
                DialogResult result = MessageBox.Show("مطمئن هستید میخواهید خروج کنید؟", "اعلان", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    e.Cancel = true;
                    Task.Run(() =>
                    {
                        while (_copyingFiles.Count > 0)
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
    }
}
