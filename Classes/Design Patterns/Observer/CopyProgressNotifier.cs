using FileCopyer.Classes.Design_Patterns.Singleton;
using FileCopyer.Interface;
using FileCopyer.Interface.Design_Patterns.Observer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes.Observer
{
    public class CopyProgressNotifier
    {
        // لیستی از ناظران (Observers) که به‌روزرسانی‌ها را دریافت می‌کنند
        private List<IProgressObserver> observers = new List<IProgressObserver>();

        // متد افزودن ناظر جدید به لیست ناظران
        /// <summary>
        /// Adds a new observer to the list of observers
        /// </summary>
        /// <param name="observer">The observer to be added</param>
        public void AddObserver(IProgressObserver observer)
        {
            observers.Add(observer);
        }

        /// <summary>
        /// Removes an observer from the list of observers
        /// </summary>
        /// <param name="observer"></param>
        public void RemoveObserver(IProgressObserver observer)
        {
            observers.Remove(observer);
        }

        // متد اطلاع‌رسانی به ناظران در مورد کپی شدن فایل
        /// <summary>
        /// Notifies all observers that a file has been copied
        /// </summary>
        /// <param name="copiedFiles">Number of files copied</param>
        /// <param name="totalFiles">Total number of files</param>
        /// <param name="errorFiles">Number of files with errors</param>
        public void NotifyFileCopied(int copiedFiles, int totalFiles, int errorFiles)
        {
            // اطلاع‌رسانی به هر ناظر در لیست ناظران
            foreach (var observer in observers)
            {
                observer.OnFileCopied(copiedFiles, totalFiles, errorFiles);
            }
        }

        // متد اطلاع‌رسانی به ناظران در مورد اتمام کپی
        /// <summary>
        /// Notifies all observers that the copy process is completed
        /// </summary>
        public void NotifyCopyCompleted()
        {
            // اطلاع‌رسانی به هر ناظر در لیست ناظران
            foreach (var observer in observers)
            {
                observer.OnCopyCompleted();
            }

            // پاک کردن لیست فایل‌های کپی شده و در حال کپی
            FileCopyManager.Instance.ClearCopyedFiles();
            FileCopyManager.Instance.ClearCopyingFiles();
        }
    }
}
