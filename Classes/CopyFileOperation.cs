using FileCopyer.Interface;
using FileCopyer.Interface.Design_Patterns.Strategy;
using FileCopyer.Models;
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
        private readonly IFileCopyStrategy _copyStrategy;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly FlowLayoutPanel _flowLayout;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// 
        /// </summary>
        private readonly List<FileModel> _fileModels;



        public CopyFileOperation(List<FileModel> fileModels, IFileCopyStrategy strategy, FlowLayoutPanel flowLayoutPanel, CancellationToken cancellationToken)
        {
            this._fileModels = fileModels;
            this._copyStrategy = strategy;
            this._flowLayout=flowLayoutPanel;
            this._cancellationToken = cancellationToken;
        }

        public void Execute()
        {
            _copyStrategy.CopyFile(_fileModels, _flowLayout, _cancellationToken);
        }
    }

}
