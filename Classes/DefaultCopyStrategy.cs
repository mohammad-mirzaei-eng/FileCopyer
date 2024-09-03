using FileCopyer.Interface;
using FileCopyer.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Classes
{
    public class DefaultCopyStrategy : IFileCopyStrategy
    {
        private SemaphoreSlim semaphore; // محدود کردن تعداد تردها
        ParallelOptions parallelOptions;
        private ConcurrentDictionary<string, bool> copyingFiles = new ConcurrentDictionary<string, bool>();
        private List<IProgressObserver> observers = new List<IProgressObserver>();
        int totalFiles = 0;

        int copiedFiles = 0;
        public DefaultCopyStrategy(SettingsModel settings)
        {
            semaphore = new SemaphoreSlim(settings.MaxThreads);
            parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = settings.MaxThreads;
        }
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
            parallelOptions.CancellationToken = cancellationToken;
           
            copiedFiles = 0;
            await Task.Run(() =>
            {
                var directories = Directory.GetDirectories(sourceFilePath, "*", SearchOption.AllDirectories);
                Parallel.ForEach(directories,parallelOptions, dirPath =>
                {
                    string newDirPath = dirPath.Replace(sourceFilePath, destFilePath);
                    if (!Directory.Exists(newDirPath))
                    {
                        Directory.CreateDirectory(newDirPath);
                    }
                });
            });

            List<Control> controls = new List<Control>();
            var progressBarDict = new Dictionary<string, ProgressBar>();
            var labelDict = new Dictionary<string, Label>();

            foreach (var file in filesToCopy)
            {
                string newDirPath = file.Replace(sourceFilePath, destFilePath);
                string relativePath = file.Substring(sourceFilePath.Length + 1);
                string destFile = Path.Combine(destFilePath, relativePath);
                ProgressBar progressBar;
                Label label;
                InitializeComponent(flowLayoutPanel, destFile, out progressBar, out label);

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

        private void InitializeComponent(FlowLayoutPanel flowLayoutPanel, string relativePath, out ProgressBar progressBar, out Label label)
        {
            progressBar = new ProgressBar
            {
                Name = relativePath + "_ProgressBar",
                Minimum = 0,
                Value = 0,
                Tag = Path.GetDirectoryName(relativePath),
                Dock = DockStyle.Bottom,
                Width = flowLayoutPanel.Width - 30
            };
            label = new Label
            {
                Name = relativePath + "_Label",
                AutoEllipsis = true,
                AutoSize = false,
                Text = $"Preparing to copy {Path.GetFileName(relativePath)}...",
                Tag = Path.GetDirectoryName(relativePath),
                Dock = DockStyle.Top,
                Width = flowLayoutPanel.Width - 30
            };
            label.Click += Label_Clicked;
            progressBar.Click += ProgressBar_Clicked;
        }

        private void ProgressBar_Clicked(object sender, EventArgs e)
        {
            if ((sender as ProgressBar).Tag!=null &&
                Directory.Exists((sender as ProgressBar).Tag.ToString()))
            {
                Process.Start((sender as ProgressBar).Tag.ToString());
            }
        }

        private void Label_Clicked(object sender, EventArgs e)
        {
            if ((sender as Label).Tag != null &&
               Directory.Exists((sender as Label).Tag.ToString()))
            {
                Process.Start((sender as Label).Tag.ToString());
            }
        }

        private async Task CopyFileWithStream(string sourceFile, string destFile, ProgressBar progressBar, Label label, CancellationToken cancellationToken)
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
                    Update_Lable(label, $"Copying {Path.GetFileName(sourceFile)} completed. {copiedFiles}/{totalFiles} files.");
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
