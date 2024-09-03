using FileCopyer.Interface.Design_Patterns.Observer;
using FileCopyer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Interface.Design_Patterns.Strategy
{
    public interface IFileCopyStrategy
    {       
        void CopyFile(List<FileModel> fileModels, FlowLayoutPanel flowLayoutPanel, CancellationToken cancellationToken);
    }

}
