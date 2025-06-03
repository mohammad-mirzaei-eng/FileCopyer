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
        private class SourceFileToCopy
        {
            public string SourcePath { get; set; } // Full path to the source file
            public string RelativePath { get; set; } // Relative path used for comparison and destination construction
            public FileModel AssociatedFileModel { get; set; } // The FileModel this source file belongs to
        }

        // Event declarations
        public event IFileCopyStrategy.FileCopyStartedHandler OnFileStarted;
        public event IFileCopyStrategy.FileCopyProgressHandler OnFileProgress;
        public event IFileCopyStrategy.FileCopyCompletedHandler OnFileCompleted;

        // محدود کردن تعداد تردها
        private SemaphoreSlim semaphore;
        ParallelOptions parallelOptions;

        private CopyProgressNotifier progressNotifier;

        SettingsModel settingsModel = null;
        int copiedFiles = 0;

        // سازنده کلاس که تنظیمات و نوتیفایر را دریافت می‌کند
        /// <summary>
        /// Constructor for DefaultCopyStrategy class
        /// </summary>
        /// <param name="settings">SettingsModel object containing settings</param>
        /// <param name="notifier">CopyProgressNotifier object for notifying progress</param>
        public DefaultCopyStrategy(SettingsModel settings, CopyProgressNotifier notifier)
        {
            this.settingsModel = settings;
            this.progressNotifier = notifier;
            settingsModel.ShowProgressBar = true;

            // مقداردهی اولیه SemaphoreSlim با حداکثر تعداد تردها
            semaphore = new SemaphoreSlim(settings.MaxThreads);
            parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = settings.MaxThreads;
        }

        // متد کپی فایل‌ها
        /// <summary>
        /// Copies files from source to destination
        /// </summary>
        /// <param name="_fileModels">List of FileModel objects containing source and destination paths</param>
        /// <param name="cancellationToken">CancellationToken for cancelling the operation</param>
        /// <returns></returns>
        public async Task CopyFile(List<FileModel> initialFileModels, CancellationToken cancellationToken)
        {
            copiedFiles = 0; // Reset overall progress counter

            try
            {
                if (cancellationToken.IsCancellationRequested) return;

                // 1. Adjust destination paths if CreateParentPath is enabled
                List<FileModel> adjustedFileModels = AdjustDestinationPaths(initialFileModels);
                if (cancellationToken.IsCancellationRequested) return;

                // 2. Get all source files and directories
                List<string> allSourceFiles = GetAllSourceFiles(adjustedFileModels);
                if (cancellationToken.IsCancellationRequested) return;

                List<string> allSourceDirectories = GetAllSourceDirectories(adjustedFileModels);
                if (cancellationToken.IsCancellationRequested) return;

                // 3. Create destination directory structure
                // Configure parallelOptions with the cancellationToken for this operation
                parallelOptions.CancellationToken = cancellationToken;
                await CreateDestinationDirectories(allSourceDirectories, adjustedFileModels, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;

                // 4. Identify missing files that need to be copied
                // The old logic for populating UI elements for missing files is removed.
                // That responsibility is now on the UI consuming the events.
                List<SourceFileToCopy> filesToProcess = IdentifyMissingFiles(allSourceFiles, adjustedFileModels);
                if (cancellationToken.IsCancellationRequested) return;

                // 5. Execute copy tasks for the identified files
                if (filesToProcess.Any())
                {
                    await ExecuteCopyTasks(filesToProcess, cancellationToken);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                     FileCopyManager.Instance.AddError("عملیات کپی توسط کاربر لغو شد."); // Copy operation was cancelled by the user.
                }
            }
            catch (OperationCanceledException)
            {
                FileCopyManager.Instance.AddError("عملیات کپی لغو شد."); // Copy operation was cancelled.
            }
            catch (Exception ex)
            {
                FileCopyManager.Instance.AddError($"خطای کلی در عملیات کپی: {ex.Message}"); // General error in copy operation
            }
            finally
            {
                // Notify completion regardless of how it ended (success, error, cancelled)
                // The FileCopyManager might have its own logic for this, but the strategy signals its part is done.
                progressNotifier.NotifyCopyCompleted();
            }
        }

        private List<FileModel> AdjustDestinationPaths(List<FileModel> initialFileModels)
        {
            List<FileModel> adjustedFileModels = initialFileModels.Select(fm => new FileModel { Source = fm.Source, Destination = fm.Destination }).ToList(); // Create a new list to avoid modifying the input list directly if it's from elsewhere
            if (settingsModel.CreateParentPath)
            {
                for (int i = 0; i < adjustedFileModels.Count; i++)
                {
                    string sourceFolderName = new DirectoryInfo(adjustedFileModels[i].Source).Name;
                    string destinationFolder = new DirectoryInfo(adjustedFileModels[i].Destination).Name;
                    if (sourceFolderName != destinationFolder)
                    {
                        string destinationPath = Path.Combine(adjustedFileModels[i].Destination, sourceFolderName);
                        if (!Directory.Exists(destinationPath))
                        {
                            Directory.CreateDirectory(destinationPath);
                        }
                        adjustedFileModels[i].Destination = destinationPath;
                    }
                }
            }
            return adjustedFileModels;
        }

        private List<string> GetAllSourceFiles(List<FileModel> fileModels)
        {
            return fileModels.AsParallel()
                .SelectMany(item => Directory.GetFiles(item.Source, "*.*", SearchOption.AllDirectories))
                .ToList();
        }

        private List<string> GetAllSourceDirectories(List<FileModel> fileModels)
        {
            return fileModels.AsParallel()
                .SelectMany(item => Directory.GetDirectories(item.Source, "*", SearchOption.AllDirectories))
                .ToList();
        }

        private async Task CreateDestinationDirectories(List<string> sourceDirectories, List<FileModel> fileModels, CancellationToken cancellationToken)
        {
            // Ensure parallelOptions is configured with the cancellationToken for this specific operation scope if needed
            // For now, assuming parallelOptions is a class member correctly set up in constructor.
            // If cancellationToken passed here should be used by Parallel.ForEach, parallelOptions should be local or reconfigured.
            var localParallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = settingsModel.MaxThreads, // Assuming settingsModel is accessible
                CancellationToken = cancellationToken
            };

            await Task.Run(() =>
            {
                Parallel.ForEach(sourceDirectories, localParallelOptions, dirPath =>
                {
                    cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation
                    foreach (var fileModel in fileModels)
                    {
                        if (!dirPath.StartsWith(fileModel.Source))
                            continue;

                        string relativePath = dirPath.Length > fileModel.Source.Length
                            ? dirPath.Substring(fileModel.Source.Length + 1)
                            : string.Empty;

                        string newDirPath = Path.Combine(fileModel.Destination, relativePath);
                        Directory.CreateDirectory(newDirPath);
                    }
                });
            }, cancellationToken);
        }

        private List<SourceFileToCopy> IdentifyMissingFiles(List<string> allSourceFiles, List<FileModel> fileModels)
        {
            var fileToRelativePathMap = new Dictionary<string, string>();
            foreach (var sourceFilePath in allSourceFiles)
            {
                foreach (var fileModel in fileModels) // fileModels here are the ones with potentially adjusted destination paths
                {
                    var relativePath = GetRelativePathHelper.GetRelativePath(fileModel.Source, sourceFilePath, settingsModel.CreateParentPath);
                    if (!string.IsNullOrEmpty(relativePath))
                    {
                        fileToRelativePathMap[relativePath] = sourceFilePath;
                        break;
                    }
                }
            }

            var destinationFilePathsSet = new HashSet<string>();
            // Optimization: Get unique physical destination base paths
            var uniqueDestinationBasePaths = fileModels.Select(fm => fm.Destination).ToHashSet();

            foreach (var uniqueDestPath in uniqueDestinationBasePaths)
            {
                if (Directory.Exists(uniqueDestPath))
                {
                    var destinationFiles = Directory.GetFiles(uniqueDestPath, "*.*", SearchOption.AllDirectories);
                    foreach (var destFileInUniquePath in destinationFiles)
                    {
                        // Relative path from the unique destination base path
                        var relativePath = GetRelativePathHelper.GetRelativePath(uniqueDestPath, destFileInUniquePath, settingsModel.CreateParentPath);
                        if (!string.IsNullOrEmpty(relativePath))
                        {
                            destinationFilePathsSet.Add(relativePath);
                        }
                    }
                }
            }

            var missingFiles = new List<SourceFileToCopy>();
            foreach(var kvp in fileToRelativePathMap)
            {
                if (!destinationFilePathsSet.Contains(kvp.Key))
                {
                    var associatedFileModel = fileModels.FirstOrDefault(fm => kvp.Value.StartsWith(fm.Source));
                    if (associatedFileModel != null) // Should always find one if logic is correct
                    {
                        missingFiles.Add(new SourceFileToCopy
                        {
                            SourcePath = kvp.Value, // Full source path
                            RelativePath = kvp.Key,   // Relative path
                            AssociatedFileModel = associatedFileModel
                        });
                    }
                }
            }
            return missingFiles;
        }

        private async Task ExecuteCopyTasks(List<SourceFileToCopy> filesToProcess, CancellationToken cancellationToken)
        {
            if (!filesToProcess.Any()) return;

            int totalFilesForOverallProgress = filesToProcess.Count; // All files passed here are missing and need processing
            var countdown = new CountdownEvent(totalFilesForOverallProgress);

            var tasks = filesToProcess.AsParallel()
                .WithDegreeOfParallelism(settingsModel.MaxThreads) // Use settingsModel for MaxDegreeOfParallelism
                .WithCancellation(cancellationToken) // Pass cancellationToken to PLINQ query
                .Select(fileToCopy => // fileToCopy is of type SourceFileToCopy
                {
                    // originalFilePath is fileToCopy.SourcePath
                    // fileModel is fileToCopy.AssociatedFileModel
                    // file.RelativePath is fileToCopy.RelativePath

                    string destFile = Path.Combine(fileToCopy.AssociatedFileModel.Destination, fileToCopy.RelativePath);

                    if (File.Exists(destFile) && !settingsModel.OverwriteFiles)
                    {
                        FileCopyManager.Instance.AddError($"فایل {fileToCopy.SourcePath} در مقصد وجود دارد و تنظیم بازنویسی غیرفعال است. از کپی صرف نظر شد.");
                        countdown.Signal(); // Signal even if skipped, to not hang countdown.Wait()
                        return null;
                    }

                    return Task.Run(async () =>
                    {
                        try
                        {
                            await semaphore.WaitAsync(cancellationToken); // Pass cancellationToken to semaphore
                            if (cancellationToken.IsCancellationRequested) return;

                            if (!FileCopyManager.Instance.IsFileBeingCopied(fileToCopy.SourcePath))
                            {
                                FileCopyManager.Instance.UpdateFileCopyStatus(fileToCopy.SourcePath, true);
                                await CopyFileWithStream(fileToCopy.SourcePath, destFile, fileToCopy.RelativePath, totalFilesForOverallProgress, cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                FileCopyManager.Instance.AddError($"فایل {fileToCopy.SourcePath} در صف کپی هست");
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            // Handle cancellation specific to this task if needed, though semaphore and PLINQ should handle it.
                            FileCopyManager.Instance.AddError($"کپی فایل {fileToCopy.SourcePath} لغو شد.");
                        }
                        catch (Exception ex)
                        {
                            FileCopyManager.Instance.AddError($"خطا در کپی فایل {fileToCopy.SourcePath}: {ex.Message}");
                            FileCopyManager.Instance.UpdateFileCopyStatus(destFile, false); // Ensure status is updated on error
                        }
                        finally
                        {
                            semaphore.Release();
                            countdown.Signal();
                        }
                    }, cancellationToken);

                }).Where(task => task != null)
                .ToList();

            if (tasks.Any())
            {
                await Task.WhenAll(tasks); // Wait for all copy tasks to complete or be cancelled
            }
            else if (totalFilesForOverallProgress > 0 && !filesToProcess.Any(f => !File.Exists(Path.Combine(f.AssociatedFileModel.Destination, f.RelativePath)) || settingsModel.OverwriteFiles))
            {
                // This case handles if all files were skipped by the File.Exists check
                // and tasks list is empty, but countdown was initialized.
                // However, countdown.Signal() is called for skipped files, so Wait should not hang.
                // If tasks list is empty because all files were skipped, countdown should be 0.
            }
             if(countdown.CurrentCount > 0 && !cancellationToken.IsCancellationRequested)
             {
                // This might indicate an issue if countdown isn't 0 and not cancelled.
                // For safety, especially if some paths lead to tasks not being created.
                // However, the .Signal() for skipped files should prevent hangs.
             }
            countdown.Wait(cancellationToken); // Wait for all signals, with cancellation support
        }


        // متد کپی فایل با استفاده از Stream و نمایش ProgressBar
        // Signature changed: removed totalFiles, copyStatusBar. Added fileKeyForEvents.
        /// <summary>
        /// Copies a file using streams and updates the progress bar
        /// </summary>
        /// <param name="sourceFile">Source file path</param>
        /// <param name="destFile">Destination file path</param>
        /// <param name="fileKeyForEvents">A key (like relativePath) for associating events with the file</param>
        /// <param name="totalFilesForOverallProgress">Total number of files for overall progress reporting</param>
        /// <param name="cancellationToken">CancellationToken for cancelling the operation</param>
        /// <returns></returns>
        private async Task CopyFileWithStream(string sourceFile, string destFile, string fileKeyForEvents, int totalFilesForOverallProgress, CancellationToken cancellationToken)
        {
            string tempDestFile = destFile + ".temp";
            OnFileStarted?.Invoke(destFile, fileKeyForEvents); // Invoke OnFileStarted
            bool success = false;
            string errorMessage = null;

            try
            {
                int bufferSize = settingsModel.MaxBufferSize * (1024 * 1024); // MB buffer size

                using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
                using (FileStream destStream = new FileStream(tempDestFile, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, bufferSize, true))
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
                        double percentage = (double)totalBytesRead / sourceStream.Length * 100;
                        OnFileProgress?.Invoke(destFile, totalBytesRead, sourceStream.Length, percentage, fileKeyForEvents);
                    }
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    errorMessage = "Copying cancelled by user.";
                    if (File.Exists(tempDestFile))
                        File.Delete(tempDestFile);
                    // success remains false
                }
                else
                {
                    bool check_deep_result = VerifyFileCopy(sourceFile, tempDestFile, true);
                    if (check_deep_result)
                    {
                        RenameFile(tempDestFile, destFile);
                        FileCopyManager.Instance.UpdateFileCopyStatus(sourceFile, false); // به‌روزرسانی وضعیت کپی شده
                        Interlocked.Increment(ref copiedFiles); // Increment for overall progress
                        progressNotifier.NotifyFileCopied(copiedFiles, totalFilesForOverallProgress, FileCopyManager.Instance.GetError()); // Notify overall progress
                        success = true;
                    }
                    else
                    {
                        errorMessage = "File verification failed.";
                        FileCopyManager.Instance.AddError($"File copy failed for {sourceFile}: {errorMessage}");
                        if (File.Exists(tempDestFile))
                            File.Delete(tempDestFile);
                        // success remains false
                    }
                }
            }
            catch (IOException ioEx)
            {
                errorMessage = $"IO Error: {ioEx.Message}";
                FileCopyManager.Instance.AddError($"Error copying {Path.GetFileName(sourceFile)}: {errorMessage}");
                if (File.Exists(tempDestFile))
                    File.Delete(tempDestFile);
                // success remains false
            }
            catch (OperationCanceledException opEx)
            {
                errorMessage = $"Copying cancelled: {opEx.Message}";
                FileCopyManager.Instance.AddError($"Copying {Path.GetFileName(sourceFile)} {errorMessage}");
                if (File.Exists(tempDestFile))
                    File.Delete(tempDestFile);
                // success remains false
            }
            catch (Exception ex)
            {
                errorMessage = $"Generic error: {ex.Message}";
                FileCopyManager.Instance.AddError($"Error copying {Path.GetFileName(sourceFile)}: {errorMessage}");
                if (File.Exists(tempDestFile))
                    File.Delete(tempDestFile);
                // success remains false
            }
            finally
            {
                OnFileCompleted?.Invoke(destFile, success, errorMessage, fileKeyForEvents);
                // Ensure temp file is deleted if cancellation was requested during the operation and not handled above
                if (cancellationToken.IsCancellationRequested && File.Exists(tempDestFile) && success == false)
                {
                    File.Delete(tempDestFile);
                }
            }
        }

        private void RenameFile(string tempFilePath, string destFilePath)
        {
            if (File.Exists(destFilePath))
            {
                File.Delete(destFilePath);
            }
            File.Move(tempFilePath, destFilePath);
        }
        // متد کپی فایل با استفاده از Stream بدون نمایش ProgressBar
        /// <summary>
        /// Copies a file using streams without updating the progress bar
        /// </summary>
        /// <param name="sourceFile">Source file path</param>
        /// <param name="destFile">Destination file path</param>
        /// <param name="fileKeyForEvents">A key (like relativePath) for associating events with the file</param>
        /// <param name="cancellationToken">CancellationToken for cancelling the operation</param>
        /// <returns></returns>
        // This overload is now removed as it's redundant.
        // private async Task CopyFileWithStream(string sourceFile, string destFile, string fileKeyForEvents, CancellationToken cancellationToken)
        // {
        // ... (content of the second overload was here) ...
        // }

        // متد بررسی کپی موفق فایل
        /// <summary>
        /// Get the hash of the source and destination files and compare them
        /// </summary>
        /// <param name="sourceFile">address source file type string</param>
        /// <param name="destFile">address destination file type string</param>
        /// <param name="size">if true hash file size , if false read all byte file in stream and return hash</param>
        /// <returns>Boolean</returns>
        private bool VerifyFileCopy(string sourceFile, string destFile, bool size)
        {
            if (size)
            {
                long source_size = (new FileInfo(sourceFile).Length);
                long destFile_size = (new FileInfo(destFile).Length);
                return source_size == destFile_size; // بررسی یکسان بودن اندازه فایل‌های مبدا و مقصد
            }
            else
            {
                using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
                using (FileStream destStream = new FileStream(destFile, FileMode.Open, FileAccess.Read))
                {
                    HashFileHelper hashFileHelper = new HashFileHelper();
                    string sourceHash = hashFileHelper.GetFileHash(sourceStream);
                    string destHash = hashFileHelper.GetFileHash(destStream);

                    return sourceHash == destHash; // بررسی یکسان بودن هش فایل‌های مبدا و مقصد
                }
            }
        }
    }
}
