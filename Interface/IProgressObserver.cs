using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Interface
{
    public interface IProgressObserver
    {
        void OnFileCopied(int copiedFiles, int totalFiles);
        void OnCopyCompleted();
    }
}
