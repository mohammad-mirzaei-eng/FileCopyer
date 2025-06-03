using FileCopyer.Interface.Design_Patterns.Observer;
using FileCopyer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Interface.Design_Patterns.Strategy
{
    // Delegates for file copy progress reporting
    public delegate void FileCopyStartedHandler(string filePath, object userState);
    public delegate void FileCopyProgressHandler(string filePath, long copiedBytes, long totalBytes, double percentage, object userState);
    public delegate void FileCopyCompletedHandler(string filePath, bool success, string errorMessage, object userState);

    public interface IFileCopyStrategy
    {
        // Events for file copy operations
        /// <summary>
        /// Event triggered when a single file copy operation starts.
        /// </summary>
        event FileCopyStartedHandler OnFileStarted;

        /// <summary>
        /// Event triggered periodically during a single file copy operation with progress updates.
        /// </summary>
        event FileCopyProgressHandler OnFileProgress;

        /// <summary>
        /// Event triggered when a single file copy operation completes (either successfully or with an error).
        /// </summary>
        event FileCopyCompletedHandler OnFileCompleted;

        // متد کپی فایل‌ها
        /// <summary>
        /// Copies files from source to destination
        /// </summary>
        /// <param name="fileModels">List of FileModel objects containing source and destination paths</param>
        /// <param name="cancellationToken">CancellationToken for cancelling the operation</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task CopyFile(List<FileModel> fileModels, CancellationToken cancellationToken);
    }

}
