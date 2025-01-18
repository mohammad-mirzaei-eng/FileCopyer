using FileCopyer.Classes.Design_Patterns.Helper;
using FileCopyer.Classes.Design_Patterns.Singleton;
using FileCopyer.Classes.Observer;
using FileCopyer.Interface.Design_Patterns.Strategy;
using FileCopyer.Models;
using FileCopyer.UserInterface;
using System;
using System.Collections.Concurrent;
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

        SettingsModel settingsModel = null;
        int copiedFiles = 0;



        public DefaultCopyStrategy(SettingsModel settings, CopyProgressNotifier notifier)
        {
            this.settingsModel = settings;
            this.progressNotifier = notifier;
            settingsModel.ShowProgressBar = true;

            semaphore = new SemaphoreSlim(settings.MaxThreads);
            parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = settings.MaxThreads;
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
                        string destinationFolder = new DirectoryInfo(fileModels[i].Destination).Name;
                        if (sourceFolderName != destinationFolder)
                        {
                            string destinationPath = Path.Combine(fileModels[i].Destination, sourceFolderName);
                            if (!Directory.Exists(destinationPath))
                            {
                                Directory.CreateDirectory(destinationPath);
                            }
                            fileModels[i].Destination = destinationPath;
                        }
                    }
                }

                // لیست تمام فایل‌ها از پوشه‌ها و زیرپوشه‌ها
                List<string> filesToCopy = fileModels.AsParallel()
                    .SelectMany(item => Directory.GetFiles(item.Source, "*.*", SearchOption.AllDirectories))
                    .ToList();

                List<string> directories = fileModels.AsParallel()
                    .SelectMany(item => Directory.GetDirectories(item.Source, "*", SearchOption.AllDirectories))
                    .ToList();

                parallelOptions.CancellationToken = cancellationToken;
                copiedFiles = 0;

                await Task.Run(() =>
                {
                    Parallel.ForEach(directories, parallelOptions, dirPath =>
                    {
                        foreach (var fileModel in fileModels)
                        {
                            if (!dirPath.StartsWith(fileModel.Source))
                                continue;

                            // محاسبه مسیر نسبی
                            string relativePath = dirPath.Length > fileModel.Source.Length
                                ? dirPath.Substring(fileModel.Source.Length + 1)
                                : string.Empty;

                            // ایجاد مسیر جدید در مقصد
                            string newDirPath = Path.Combine(fileModel.Destination, relativePath);

                            // ساخت دایرکتوری در صورت نیاز (ایجاد فقط اگر وجود ندارد)
                            Directory.CreateDirectory(newDirPath);
                        }
                    });
                });


                List<Control> controls = new List<Control>();
                var copyStatusBar = new Dictionary<string, CopyStatusBar>();

                if (settingsModel.ShowProgressBar)
                    flowLayoutPanel.Invoke((MethodInvoker)(() =>
                    {
                        flowLayoutPanel.SuspendLayout();
                    }));

                // ساخت دیکشنری برای نگهداری مسیر نسبی فایل‌های مبدا

                var fileToRelativePathMap = new Dictionary<string, string>();

                foreach (var file in filesToCopy)
                {
                    foreach (var fileModel in fileModels)
                    {
                        // تلاش برای یافتن مسیر نسبی
                        var relativePath = GetRelativePathHelper.GetRelativePath(fileModel.Source, file, settingsModel.CreateParentPath);

                        if (!string.IsNullOrEmpty(relativePath))
                        {
                            fileToRelativePathMap[relativePath] = file;
                            break; // خروج از حلقه پس از یافتن اولین مسیر نسبی معتبر
                        }
                    }
                }

                var destinationFilePathsSet = new HashSet<string>();

                foreach (var item in fileModels)
                {
                    if (Directory.Exists(item.Destination)) // بررسی وجود مسیر مقصد
                    {
                        var destinationFiles = Directory.GetFiles(item.Destination, "*.*", SearchOption.AllDirectories);

                        foreach (var destFile in destinationFiles)
                        {
                            var relativePath = GetRelativePathHelper.GetRelativePath(item.Destination, destFile, settingsModel.CreateParentPath);

                            if (!string.IsNullOrEmpty(relativePath)) // جلوگیری از اضافه کردن مسیرهای خالی
                            {
                                destinationFilePathsSet.Add(relativePath);
                            }
                        }
                    }
                }

                // پیدا کردن فایل‌هایی که فقط در مبدا وجود دارند
                var missingFilesInDestination = fileToRelativePathMap.AsParallel()
                    .Where(kvp => !destinationFilePathsSet.Contains(kvp.Key)) // بررسی عدم وجود در مقصد
                    .Select(kvp => new
                    {
                        SourcePath = kvp.Value, // مسیر کامل فایل در مبدا
                        RelativePath = kvp.Key  // مسیر نسبی فایل
                    });



                // برای هر فایل که در مقصد وجود ندارد
                foreach (var kvp in missingFilesInDestination)
                {
                    // پیدا کردن مدل فایل مرتبط
                    var fileModel = fileModels.FirstOrDefault(fm => kvp.SourcePath.StartsWith(fm.Source));
                    if (fileModel == null)
                        continue; // اگر مدل مرتبط پیدا نشد، از این فایل صرف نظر کنید

                    // محاسبه مسیر نسبی بدون پوشه والد
                    string relativePath = GetRelativePathHelper.GetRelativePath(fileModel.Source, fileModel.Destination, settingsModel.CreateParentPath);

                    // حذف پوشه والد از مسیر نسبی (اگر وجود داشته باشد)
                    string sourceFolderName = new DirectoryInfo(fileModel.Source).Name;
                    if (relativePath.StartsWith(sourceFolderName + Path.DirectorySeparatorChar))
                    {
                        relativePath = relativePath.Substring(sourceFolderName.Length + 1);
                    }

                    // مسیر نهایی فایل در مقصد
                    string destFile = Path.Combine(fileModel.Destination, relativePath);
                    // ایجاد ProgressBar و Label فقط در صورت فعال بودن تنظیمات
                    if (settingsModel.ShowProgressBar)
                    {
                        CopyStatusBar copyStatus;
                        InitializeComponent(flowLayoutPanel, destFile, out copyStatus);

                        // ذخیره ProgressBar و Label در دیکشنری‌ها
                        copyStatusBar[kvp.RelativePath] = copyStatus;


                        // افزودن به لیست کنترل‌ها
                        controls.Add(copyStatus);
                    }
                }

                // افزودن کنترل‌ها به FlowLayoutPanel به صورت دسته‌ای
                if (settingsModel.ShowProgressBar && controls.Count > 0)
                {
                    flowLayoutPanel.Invoke((MethodInvoker)(() =>
                    {
                        flowLayoutPanel.Controls.AddRange(controls.ToArray());
                        flowLayoutPanel.ResumeLayout();
                    }));
                }

                var countdown = new CountdownEvent(missingFilesInDestination.Count(file =>
    !FileCopyManager.Instance.IsFileCopied(file.SourcePath)));
                // به‌روزرسانی کد برای محاسبه درست مسیرهای نسبی
                var tasks = missingFilesInDestination.AsParallel()
                    .Where(file => !FileCopyManager.Instance.IsFileCopied(file.SourcePath)) // فقط فایل‌هایی که کپی نشده‌اند
                    .Select(file =>
                    {
                        var originalFilePath = filesToCopy.FirstOrDefault(f => Path.GetFullPath(f) == Path.GetFullPath(file.SourcePath));
                        if (originalFilePath == null) return null; // اگر فایل مبدا پیدا نشد، از کپی صرف نظر کن

                        var fileModel = fileModels.FirstOrDefault(fm => originalFilePath.StartsWith(fm.Source));
                        if (fileModel == null) return null; // اگر مدل فایل پیدا نشد، از کپی صرف نظر کن

                        // محاسبه مسیر نسبی بدون پوشه والد
                        string relativePath = GetRelativePathHelper.GetRelativePath(fileModel.Source, originalFilePath, settingsModel.CreateParentPath);

                        // حذف پوشه والد اضافی
                        string sourceFolderName = new DirectoryInfo(fileModel.Source).Name;
                        if (relativePath.StartsWith(sourceFolderName))
                        {
                            relativePath = relativePath.Substring(sourceFolderName.Length + 1);
                        }

                        // اصلاح مسیر مقصد
                        string destFile = Path.Combine(fileModel.Destination, relativePath);

                        // اگر فایل در مقصد وجود داشته باشد، هیچ کاری انجام نمی‌دهیم
                        if (File.Exists(destFile))
                        {
                            FileCopyManager.Instance.AddError($"فایل {file.SourcePath} در مقصد وجود دارد");
                            return null; // اگر فایل در مقصد وجود دارد، از کپی صرف نظر کن
                        }

                        // ایجاد Task برای کپی فایل
                        return Task.Run(async () =>
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                if (!FileCopyManager.Instance.IsFileBeingCopied(file.SourcePath))
                                {
                                    FileCopyManager.Instance.UpdateFileCopyStatus(file.SourcePath, true); // به‌روزرسانی وضعیت کپی شدن فایل
                                    if (settingsModel.ShowProgressBar && copyStatusBar.ContainsKey(file.RelativePath))
                                    {
                                        await CopyFileWithStream(originalFilePath, destFile, missingFilesInDestination.AsParallel().Count(), copyStatusBar[file.RelativePath], cancellationToken).ConfigureAwait(false); // کپی فایل
                                        FileCopyManager.Instance.UpdateFileCopyStatus(file.SourcePath, false); // به‌روزرسانی وضعیت کپی شده
                                    }
                                    else if (settingsModel.ShowProgressBar == false)
                                    {
                                        await CopyFileWithStream(originalFilePath, destFile, missingFilesInDestination.AsParallel().Count(), cancellationToken).ConfigureAwait(false); // کپی فایل
                                        FileCopyManager.Instance.UpdateFileCopyStatus(file.SourcePath, false); // به‌روزرسانی وضعیت کپی شده
                                    }
                                }
                                else
                                {
                                    FileCopyManager.Instance.AddError($"فایل {file.SourcePath} در صف کپی هست");
                                }
                            }
                            catch (Exception ex)
                            {
                                FileCopyManager.Instance.AddError($"خطا در کپی فایل {file.SourcePath}: {ex.Message}");
                                FileCopyManager.Instance.UpdateFileCopyStatus(destFile, false);
                            }
                            finally
                            {
                                semaphore.Release();
                                countdown.Signal();
                            }
                        }, cancellationToken);

                    }).Where(task => task != null)
                    .ToList(); // حذف تسک‌های خالی
                if (tasks.Any())
                {
                   // await Task.WhenAll(tasks);
                }
                countdown.Wait();
                progressNotifier.NotifyCopyCompleted();
            }
            catch (Exception ex)
            {
                FileCopyManager.Instance.AddError(ex.Message);
            }
        }

        private void InitializeComponent(FlowLayoutPanel flowLayoutPanel, string relativePath, out CopyStatusBar copyStatusBar)
        {
            copyStatusBar = new CopyStatusBar
            {
                ProgressBarName = relativePath + "_ProgressBar",
                ProgressBarMinValue = 0,
                ProgressBarValue = 0,
                Tag = relativePath,
                LableText = $"Preparing to copy {Path.GetFileName(relativePath)}...",
                Width = flowLayoutPanel.ClientRectangle.Width - 20
            };
        }

        private async Task CopyFileWithStream(string sourceFile, string destFile, int totalFiles, CopyStatusBar copyStatusBar, CancellationToken cancellationToken)
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
                        copyStatusBar.Invoke((MethodInvoker)(() =>
                        {
                            copyStatusBar.ProgressBarMaxValue = (int)sourceStream.Length;
                            copyStatusBar.ProgressBarValue = (int)totalBytesRead;
                            copyStatusBar.LableText = $"Copying {Path.GetFileName(sourceFile)} ({totalBytesRead / 1024} KB of {sourceStream.Length / 1024} KB)";
                        }));
                    }
                    Interlocked.Increment(ref copiedFiles);
                    progressNotifier.NotifyFileCopied(copiedFiles, totalFiles, FileCopyManager.Instance.GetError());
                    copyStatusBar.Invoke((MethodInvoker)(() =>
                    {
                        copyStatusBar.LableText = $"Copying {Path.GetFileName(sourceFile)} completed. {copiedFiles}/{totalFiles} files.";
                    }));
                }
            }
            catch (IOException ioEx)
            {
                FileCopyManager.Instance.AddError($"Error copying {Path.GetFileName(sourceFile)}: {ioEx.Message}");
                copyStatusBar.Invoke((MethodInvoker)(() =>
                {
                    copyStatusBar.LableText = $"Error copying {Path.GetFileName(sourceFile)}: {ioEx.Message}";
                }));
            }
            catch (OperationCanceledException ex)
            {
                FileCopyManager.Instance.AddError($"Copying {Path.GetFileName(sourceFile)} {ex.Message}");
                copyStatusBar.Invoke((MethodInvoker)(() =>
                {
                    copyStatusBar.LableText = $"Copying {Path.GetFileName(sourceFile)} {ex.Message}";
                }));
            }
            catch (Exception ex)
            {
                FileCopyManager.Instance.AddError($"Error copying {Path.GetFileName(sourceFile)}: {ex.Message}");
                copyStatusBar.Invoke((MethodInvoker)(() =>
                {
                    copyStatusBar.LableText = $"Error copying {Path.GetFileName(sourceFile)}: {ex.Message}";
                }));
            }
        }

        private async Task CopyFileWithStream(string sourceFile, string destFile, int totalFiles, CancellationToken cancellationToken)
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
                    }
                    Interlocked.Increment(ref copiedFiles);
                    progressNotifier.NotifyFileCopied(copiedFiles, totalFiles, FileCopyManager.Instance.GetError());
                }
            }
            catch (IOException ioEx)
            {
                FileCopyManager.Instance.AddError($"Error copying {Path.GetFileName(sourceFile)}: {ioEx.Message}");
            }
            catch (OperationCanceledException ex)
            {
                FileCopyManager.Instance.AddError($"Copying {Path.GetFileName(sourceFile)} {ex.Message}");
            }
            catch (Exception ex)
            {
                FileCopyManager.Instance.AddError($"Error copying {Path.GetFileName(sourceFile)}: {ex.Message}");
            }
        }
    }
}
