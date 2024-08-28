using FileCopyer.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Classes
{
    public class BufferedCopyStrategy : IFileCopyStrategy
    {
        private readonly int bufferSize;

        public BufferedCopyStrategy(int bufferSize)
        {
            this.bufferSize = bufferSize;
        }

        public void CopyFile(string sourceFilePath, string destFilePath, FlowLayoutPanel flowLayoutPanel, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void CopyFileWithStream(string sourceFile, string destFile, ProgressBar progressBar, Label label, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

}
