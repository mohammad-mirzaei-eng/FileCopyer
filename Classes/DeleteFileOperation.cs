using FileCopyer.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes
{
    public class DeleteFileOperation : IFileOperation
    {
        private readonly string filePath;

        public DeleteFileOperation(string filePath)
        {
            this.filePath = filePath;
        }

        public void Execute()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

}
