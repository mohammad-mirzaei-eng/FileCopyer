using FileCopyer.Classes.Design_Patterns.Helper;
using FileCopyer.Classes.Design_Patterns.Singleton;
using FileCopyer.Classes.Observer;
using FileCopyer.Interface.Design_Patterns.Strategy;
using FileCopyer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Classes.Design_Patterns.Strategy
{
    public class DefaultCopyStrategy : IFileCopyStrategy
    {
        private SemaphoreSlim semaphore; // محدود کردن تعداد تردها
        ParallelOptions parallelOptions;

        private CopyProgressNotifier progressNotifier;

        /// <summary>
        /// 
        /// </summary>
        private List<string> _errorList = new List<string>(); // لیست خطاها

        int totalFiles = 0;
        SettingsModel settingsModel = null;
        int copiedFiles = 0;


        public DefaultCopyStrategy(SettingsModel settings, CopyProgressNotifier notifier)
        {
            this.settingsModel = settings;
            semaphore = new SemaphoreSlim(settings.MaxThreads);
            parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = settings.MaxThreads;
            progressNotifier = notifier;
        }

        public async Task CopyFile(List<FileModel> _fileModels, FlowLayoutPanel flowLayoutPanel, CancellationToken cancellationToken)
        {
            try
            {

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                List<FileModel> fileModels = _fileModels.Select(file => new FileModel
                {
                    Source = file.Source,
                    Destination = file.Destination
                }).ToList();

                if (settingsModel.CreateParentPath)
                {
                    for (int i = 0; i < fileModels.Count; i++)
                    {
                        string sourceFolderName = new DirectoryInfo(fileModels[i].Source).Name;
                        string destinationPath = Path.Combine(fileModels[i].Destination, sourceFolderName);
                        if (!Directory.Exists(destinationPath))
                        {
                            Directory.CreateDirectory(destinationPath);
                        }
                        fileModels[i].Destination = destinationPath;
                    }
                }

                // لیست تمام فایل‌ها از پوشه‌ها و زیرپوشه‌ها
                List<string> filesToCopy = fileModels
                    .SelectMany(item => Directory.GetFiles(item.Source, "*.*", SearchOption.AllDirectories))
                    .ToList();

                List<string> Destinationfiles = fileModels
                    .SelectMany(item => Directory.GetFiles(item.Destination, "*.*", SearchOption.AllDirectories))
                    .ToList();

                totalFiles = filesToCopy.Count;

                parallelOptions.CancellationToken = cancellationToken;
                List<string> directories = fileModels
                    .SelectMany(item => Directory.GetDirectories(item.Source, "*", SearchOption.AllDirectories))
                    .ToList();

                copiedFiles = 0;
                await Task.Run(() =>
                {
                    Parallel.ForEach(directories, parallelOptions, dirPath =>
                    {
                        foreach (var fileModel in fileModels)
                        {
                            if (dirPath.StartsWith(fileModel.Source))
                            {
                                string relativePath = dirPath != fileModel.Source ? dirPath.Substring(fileModel.Source.Length + 1) : dirPath;
                                string newDirPath = Path.Combine(fileModel.Destination, relativePath);

                                if (!Directory.Exists(newDirPath))
                                {
                                    Directory.CreateDirectory(newDirPath);
                                }
                            }
                        }
                    });
                });


                List<Control> controls = new List<Control>();
                var progressBarDict = new Dictionary<string, ProgressBar>();
                var labelDict = new Dictionary<string, Label>();
                flowLayoutPanel.Invoke((MethodInvoker)(() =>
                {
                    flowLayoutPanel.SuspendLayout();
                }));

                var filesToCopyPaths = fileModels.SelectMany(item =>
                filesToCopy.Select(file => GetRelativePathHelper.GetRelativePath(item.Source, file))
                ).ToList();

                var destinationFilePaths = fileModels.SelectMany(item =>
                    Destinationfiles.Select(file => GetRelativePathHelper.GetRelativePath(item.Destination, file))
                ).ToList();


                // مقایسه و پیدا کردن فایل‌هایی که فقط در مبدا وجود دارند
                var files = filesToCopyPaths.Except(destinationFilePaths).ToList();

                foreach (var file in files)
                {
                    var fileModel = fileModels.FirstOrDefault(f => file.Contains(f.GetName));
                    if (fileModel != null)
                    {
                        string relativePath = file.Substring(fileModel.Source.Length + 1);
                        string destFile = Path.Combine(fileModel.Destination, relativePath);
                        ProgressBar progressBar;
                        Label label;
                        InitializeComponent(flowLayoutPanel, destFile, out progressBar, out label);

                        progressBarDict[file] = progressBar;
                        labelDict[file] = label;

                        controls.Add(label);
                        controls.Add(progressBar);
                    }
                }

                // افزودن کنترل‌ها به FlowLayoutPanel به صورت دسته‌ای
                flowLayoutPanel.Invoke((MethodInvoker)(() =>
                {
                    flowLayoutPanel.Controls.AddRange(controls.ToArray());
                    flowLayoutPanel.ResumeLayout();
                }));

                // کپی فایل‌ها
                List<Task> tasks = new List<Task>();
                foreach (var file in files)
                {
                    if (!FileCopyManager.Instance.IsFileCopied(file))
                    {
                        var fileModel = fileModels.FirstOrDefault(f => file.Contains(f.GetName)); ;
                        if (fileModel != null)
                        {
                            string relativePath = file.Substring(fileModel.Source.Length + 1);
                            string destFile = Path.Combine(fileModel.Destination, relativePath);

                            if (!File.Exists(destFile))
                            {
                                tasks.Add(Task.Run(async () =>
                                {
                                    await semaphore.WaitAsync();
                                    try
                                    {
                                        if (!FileCopyManager.Instance.IsFileBeingCopied(file))
                                        {
                                            FileCopyManager.Instance.UpdateFileCopyStatus(file, true);
                                            if (progressBarDict.ContainsKey(file) && labelDict.ContainsKey(file))
                                            {
                                                var progressBar = progressBarDict[file];
                                                var label = labelDict[file];
                                                await CopyFileWithStream(file, destFile, progressBar, label, cancellationToken);

                                                FileCopyManager.Instance.UpdateFileCopyStatus(file, false);
                                            }
                                        }
                                        else
                                        {
                                            _errorList.Add($"فایل {file} در صف کپی هست");
                                        }
                                    }
                                    finally
                                    {
                                        semaphore.Release();
                                    }
                                }, cancellationToken));
                            }
                        }
                    }
                }

                await Task.WhenAll(tasks);
                FileCopyManager.Instance.AddErrors(_errorList);
                progressNotifier.NotifyCopyCompleted();
            }
            catch (Exception ex)
            {
                _errorList.Add(ex.Message);
            }
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
            if ((sender as ProgressBar).Tag != null &&
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

                int bufferSize = settingsModel.MaxBufferSize * (1024 * 1024); // MB buffer size

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
                    Interlocked.Increment(ref copiedFiles);
                    progressNotifier.NotifyFileCopied(copiedFiles, totalFiles, _errorList.Count);
                    Update_Lable(label, $"Copying {Path.GetFileName(sourceFile)} completed. {copiedFiles}/{totalFiles} files.");
                }
            }
            catch (IOException ioEx)
            {
                _errorList.Add($"Error copying {Path.GetFileName(sourceFile)}: {ioEx.Message}");
                Update_Lable(label, $"Error copying {Path.GetFileName(sourceFile)}: {ioEx.Message}");
            }
            catch (OperationCanceledException ex)
            {
                _errorList.Add($"Copying {Path.GetFileName(sourceFile)} {ex.Message}");
                Update_Lable(label, $"Copying {Path.GetFileName(sourceFile)} {ex.Message}");
            }
            catch (Exception ex)
            {
                _errorList.Add($"Error copying {Path.GetFileName(sourceFile)}: {ex.Message}");
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
