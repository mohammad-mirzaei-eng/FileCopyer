using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Interface
{
    public interface IFileCopyStrategy
    {
        void AddObserver(IProgressObserver observer);
       
        void CopyFile(string sourceFilePath, string destFilePath, FlowLayoutPanel flowLayoutPanel, CancellationToken cancellationToken);
    }

}
