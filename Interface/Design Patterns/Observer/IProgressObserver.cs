using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Interface.Design_Patterns.Observer
{
    public interface IProgressObserver
    {
        void OnFileCopied(int copiedFiles, int totalFiles,int errorFiles);
        void OnCopyCompleted();
    }
}
