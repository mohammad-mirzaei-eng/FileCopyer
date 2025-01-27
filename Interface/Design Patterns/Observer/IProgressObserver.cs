using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Interface.Design_Patterns.Observer
{
    public interface IProgressObserver
    {
        // رویداد کپی شدن فایل
        /// <summary>
        /// Event triggered when a file is copied
        /// </summary>
        /// <param name="copiedFiles">Number of copied files</param>
        /// <param name="totalFiles">Total number of files</param>
        /// <param name="errorFiles">Number of error files</param>
        void OnFileCopied(int copiedFiles, int totalFiles, int errorFiles);

        // رویداد اتمام کپی
        /// <summary>
        /// Event triggered when the copy process is completed
        /// </summary>
        void OnCopyCompleted();
    }
}
