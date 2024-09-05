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
        private List<IProgressObserver> observers = new List<IProgressObserver>();

        public void AddObserver(IProgressObserver observer)
        {
            observers.Add(observer);
        }

        public void RemoveObserver(IProgressObserver observer)
        {
            observers.Remove(observer);
        }

        public void NotifyFileCopied(int copiedFiles, int totalFiles, int errorFiles)
        {
            foreach (var observer in observers)
            {
                observer.OnFileCopied(copiedFiles, totalFiles,errorFiles);
            }
        }

        public void NotifyCopyCompleted()
        {
            foreach (var observer in observers)
            {
                observer.OnCopyCompleted();
            }
        }
    }
}
