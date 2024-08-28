using FileCopyer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Classes
{
    public class CopyFileOperation : IFileOperation
    {
        
        /// <summary>
        /// 
        /// </summary>
        private readonly string _sourceFilePath;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly string _destinationFilePath;
       
        /// <summary>
        /// 
        /// </summary>
        private readonly IFileCopyStrategy _copyStrategy;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly FlowLayoutPanel _flowLayout;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly Label _label;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly CancellationToken cancellationToken;
       
        
        public CopyFileOperation(string sourceFilePath, string destFilePath, IFileCopyStrategy strategy, FlowLayoutPanel flowLayoutPanel, CancellationToken cancellationToken)
        {
            this._sourceFilePath = sourceFilePath;
            this._destinationFilePath = destFilePath;
            this._copyStrategy = strategy;
            this._flowLayout=flowLayoutPanel;
            this.cancellationToken = cancellationToken;
        }

        public void Execute()
        {
            _copyStrategy.CopyFile(_sourceFilePath, _destinationFilePath, _flowLayout, cancellationToken);
        }
    }

}
