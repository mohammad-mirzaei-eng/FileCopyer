using FileCopyer.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Classes
{
    public class DefaultCopyStrategy : IFileCopyStrategy
    {
        private SemaphoreSlim semaphore = new SemaphoreSlim(15);
        private ConcurrentDictionary<string, bool> copyingFiles = new ConcurrentDictionary<string, bool>();

        public void CopyFile(string sourceFilePath, string destFilePath, FlowLayoutPanel flowLayoutPanel, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            List<Task> fileCopyTasks = new List<Task>();

            DirectoryInfo dir = new DirectoryInfo(sourceFilePath);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destFilePath))
            {
                Directory.CreateDirectory(destFilePath);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destFilePath, file.Name);

                if (!File.Exists(tempPath))
                {
                    ProgressBar progressBar;
                    Label label;
                    InitializeComponent(flowLayoutPanel.Width-30, tempPath, out progressBar, out label);

                    fileCopyTasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        try
                        {
                            if (!copyingFiles.TryAdd(file.FullName, true))
                            {
                                return;
                            }

                            CopyFileWithStream(file.FullName, tempPath, progressBar, label, cancellationToken);
                        }
                        finally
                        {
                            semaphore.Release();
                            copyingFiles.TryRemove(file.FullName, out _);
                        }
                    }, cancellationToken));

                    // Add controls to the UI
                    flowLayoutPanel.Controls.Add(progressBar);
                    flowLayoutPanel.Controls.Add(label);
                }
            }

            Task.WaitAll(fileCopyTasks.ToArray());

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destFilePath, subdir.Name);
                CopyFile(subdir.FullName, tempPath, flowLayoutPanel, cancellationToken);
            }
        }

        private static void InitializeComponent(int Width, string filename, out ProgressBar progressBar, out Label label)
        {
            progressBar = new ProgressBar
            {
                Name = filename + "_ProgressBar",
                Minimum = 0,
                Value = 0,
                Dock = DockStyle.Top,
                Width = Width
            };
            label = new Label
            {
                Name = filename + "_Label",
                AutoEllipsis = true,
                AutoSize = false,
                Text = $"Preparing to copy {filename}...",
                Dock = DockStyle.Top,
                Width = Width
            };
        }

        public async void CopyFileWithStream(string sourceFile, string destFile, ProgressBar progressBar, Label label, CancellationToken cancellationToken)
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
                        await destStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        totalBytesRead += bytesRead;

                        // Update ProgressBar and Label
                        Update_progressBar(progressBar, (int)totalBytesRead, (int)sourceStream.Length);

                        Update_Lable(label, $"Copying {Path.GetFileName(sourceFile)} ({totalBytesRead / 1024} KB of {sourceStream.Length / 1024} KB)");

                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
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
