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
        void CopyFile(string sourceFilePath, string destFilePath, FlowLayoutPanel flowLayoutPanel, ProgressBar pgbtotal, CancellationToken cancellationToken);
        Task CopyFileWithStream(string sourceFile, string destFile, ProgressBar progressBar, Label label, CancellationToken cancellationToken);
    }

}
