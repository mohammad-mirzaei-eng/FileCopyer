using FileCopyer.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes
{
    public class MoveFileOperation : IFileOperation
    {
        private readonly string sourceFilePath;
        private readonly string destFilePath;

        public MoveFileOperation(string sourceFilePath, string destFilePath)
        {
            this.sourceFilePath = sourceFilePath;
            this.destFilePath = destFilePath;
        }

        public void Execute()
        {
            if (File.Exists(sourceFilePath))
            {
                File.Move(sourceFilePath, destFilePath);
            }
        }
    }

}
