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
    public interface IFileCopyStrategy
    {
        // متد کپی فایل‌ها
        /// <summary>
        /// Copies files from source to destination
        /// </summary>
        /// <param name="fileModels">List of FileModel objects containing source and destination paths</param>
        /// <param name="flowLayoutPanel">FlowLayoutPanel for displaying progress bars</param>
        /// <param name="cancellationToken">CancellationToken for cancelling the operation</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task CopyFile(List<FileModel> fileModels, FlowLayoutPanel flowLayoutPanel, CancellationToken cancellationToken);
    }

}
