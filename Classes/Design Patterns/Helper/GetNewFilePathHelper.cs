using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes.Design_Patterns.Helper
{
    public class GetNewFilePathHelper
    {
        public string GetNewFilePath(string baseFilePath, bool checkFileSize=false)
        {
            string directory = Path.GetDirectoryName(baseFilePath);
            string fileName = Path.GetFileNameWithoutExtension(baseFilePath);
            string extension = Path.GetExtension(baseFilePath);
            int partNumber = 1;

            string newFilePath = baseFilePath;

            while (File.Exists(newFilePath))
            {
                if (checkFileSize && new FileInfo(newFilePath).Length > (1 * 1024 * 1024)) // بررسی وجود فایل و اندازه آن)
                {
                    newFilePath = Path.Combine(directory, $"{fileName}_part{partNumber}{extension}");
                }
                else
                {
                    newFilePath = Path.Combine(directory, $"{fileName}_part{partNumber}{extension}");
                }
                partNumber++;
            }

            return newFilePath;
        }
    }
}
