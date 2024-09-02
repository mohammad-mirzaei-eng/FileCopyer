using FileCopyer.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Classes
{
    public class DefaultCopyStrategy : IFileCopyStrategy
    {
        private SemaphoreSlim semaphore = new SemaphoreSlim(15); // محدود کردن تعداد تردها
        private ConcurrentDictionary<string, bool> copyingFiles = new ConcurrentDictionary<string, bool>();
        private List<IProgressObserver> observers = new List<IProgressObserver>();
        int totalFiles = 0;

        int copiedFiles = 0;

        public void AddObserver(IProgressObserver observer)
        {
            observers.Add(observer);
        }

        private void NotifyFileCopied()
        {                           
            Interlocked.Increment(ref copiedFiles);
            foreach (var observer in observers)
            {
                observer.OnFileCopied(copiedFiles, totalFiles);
            }
        }

        private void NotifyCopyCompleted()
        {
            foreach (var observer in observers)
            {
                observer.OnCopyCompleted();
            }
        }


        public async void CopyFile(string sourceFilePath, string destFilePath, FlowLayoutPanel flowLayoutPanel, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // لیست تمام فایل‌ها از پوشه‌ها و زیرپوشه‌ها
            List<string> filesToCopy = Directory.GetFiles(sourceFilePath, "*.*", SearchOption.AllDirectories).ToList();
            totalFiles = filesToCopy.Count;

            copiedFiles = 0;

            foreach (var dirPath in Directory.GetDirectories(sourceFilePath, "*", SearchOption.AllDirectories))
            {
                string newDirPath = dirPath.Replace(sourceFilePath, destFilePath);
                if (!Directory.Exists(newDirPath))
                {
                    Directory.CreateDirectory(newDirPath);
                }
            }
            List<Control> controls = new List<Control>();
            var progressBarDict = new Dictionary<string, ProgressBar>();
            var labelDict = new Dictionary<string, Label>();

            foreach (var file in filesToCopy)
            {
                string newDirPath = file.Replace(sourceFilePath, destFilePath);
                string relativePath = file.Substring(sourceFilePath.Length + 1);
                string destFile = Path.Combine(destFilePath, relativePath);

                var progressBar = new ProgressBar
                {
                    Name = file + "_ProgressBar",
                    Minimum = 0,
                    Value = 0,
                    Dock = DockStyle.Bottom,
                    Width = flowLayoutPanel.Width - 30
                };

                var label = new Label
                {
                    Name = file + "_Label",
                    AutoEllipsis = true,
                    AutoSize = false,
                    Text = $"Preparing to copy {Path.GetFileName(file)}...",
                    Dock = DockStyle.Top,
                    Width = flowLayoutPanel.Width - 30
                };

                progressBarDict[file] = progressBar;
                labelDict[file] = label;

                controls.Add(label);
                controls.Add(progressBar);
            }

            // افزودن کنترل‌ها به FlowLayoutPanel به صورت دسته‌ای
            flowLayoutPanel.Invoke((MethodInvoker)(() =>
            {
                flowLayoutPanel.Controls.AddRange(controls.ToArray());
            }));

            // کپی فایل‌ها
            List<Task> tasks = new List<Task>();
            foreach (var file in filesToCopy)
            {
                string relativePath = file.Substring(sourceFilePath.Length + 1);
                string destFile = Path.Combine(destFilePath, relativePath);

                if (!File.Exists(destFile))
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            if (!copyingFiles.TryAdd(file, true))
                            {
                                return;
                            }

                            var progressBar = progressBarDict[file];
                            var label = labelDict[file];

                            await CopyFileWithStream(file, destFile, progressBar, label, cancellationToken);

                            NotifyCopyCompleted();
                            flowLayoutPanel.Invoke((MethodInvoker)(() =>
                            {
                                label.Text = $"Copied {copiedFiles}/{totalFiles} files.";
                            }));
                        }
                        finally
                        {
                            semaphore.Release();
                            copyingFiles.TryRemove(file, out _);
                        }
                    }, cancellationToken));
                }
            }

            await Task.WhenAll(tasks);
        }

        private static void InitializeComponent(int width, string filename, out ProgressBar progressBar, out Label label)
        {
            progressBar = new ProgressBar
            {
                Name = filename + "_ProgressBar",
                Minimum = 0,
                Value = 0,
                Dock = DockStyle.Bottom,
                Width = width
            };
            label = new Label
            {
                Name = filename + "_Label",
                AutoEllipsis = true,
                AutoSize = false,
                Text = $"Preparing to copy {filename}...",
                Dock = DockStyle.Top,
                Width = width
            };
            label.Click += Label_Click;
        }

        private static void Label_Click(object sender, EventArgs e)
        {
            Clipboard.SetText((sender as Label).Text.ToString());
            MessageBox.Show("Test");
        }

        public async Task CopyFileWithStream(string sourceFile, string destFile, ProgressBar progressBar, Label label, CancellationToken cancellationToken)
        {
            try
            {

                const int bufferSize = 1024 * 1024; // 1MB buffer size
                using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
                using (FileStream destStream = new FileStream(destFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, bufferSize, true))
                {
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead;
                    long totalBytesRead = 0;

                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        await destStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        totalBytesRead += bytesRead;

                        // بروزرسانی پروگرس بار و لیبل
                        Update_progressBar(progressBar, (int)totalBytesRead, (int)sourceStream.Length);
                        Update_Lable(label, $"Copying {Path.GetFileName(sourceFile)} ({totalBytesRead / 1024} KB of {sourceStream.Length / 1024} KB)");
                    }
                    NotifyFileCopied();
                    Update_Lable(label, $"Copying {Path.GetFileName(sourceFile)} completed.");
                }
            }
            catch (OperationCanceledException ex)
            {
                Update_Lable(label, $"Copying {Path.GetFileName(sourceFile)} {ex.Message}");
            }
            catch (Exception ex)
            {
                Update_Lable(label, $"Error copying {Path.GetFileName(sourceFile)}: {ex.Message}");
            }
        }

        private static void Update_progressBar(ProgressBar progressBar, int value = 0, int Maximum = 100)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.BeginInvoke((MethodInvoker)(() =>
                {
                    progressBar.Maximum = Maximum;
                    progressBar.Value = value;
                }));
            }
            else
            {
                progressBar.Maximum = Maximum;
                progressBar.Value = value;
            }
        }

        private static void Update_Lable(Label label, string txt)
        {
            if (label.InvokeRequired)
            {
                label.BeginInvoke((MethodInvoker)(() =>
                {
                    label.Text = txt;
                }));
            }
            else
            {
                label.Text = txt;
            }
        }
    }
}
