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
        // متد دریافت مسیر جدید فایل
        /// <summary>
        /// Generates a new file path to avoid conflicts with existing files
        /// </summary>
        /// <param name="baseFilePath">The base file path to check for conflicts</param>
        /// <param name="checkFileSize">Optional parameter to check file size (default is false)</param>
        /// <returns>New file path as a string</returns>
        public string GetNewFilePath(string baseFilePath, bool checkFileSize = false)
        {
            // دریافت دایرکتوری از مسیر فایل پایه
            string directory = Path.GetDirectoryName(baseFilePath);
            // دریافت نام فایل بدون پسوند
            string fileName = Path.GetFileNameWithoutExtension(baseFilePath);
            // دریافت پسوند فایل
            string extension = Path.GetExtension(baseFilePath);
            // شماره بخش فایل
            int partNumber = 1;

            // مسیر جدید فایل
            string newFilePath = baseFilePath;

            // بررسی وجود فایل و ایجاد مسیر جدید در صورت نیاز
            while (File.Exists(newFilePath))
            {
                // بررسی اندازه فایل در صورت نیاز
                if (checkFileSize && new FileInfo(newFilePath).Length > (1 * 1024 * 1024)) // بررسی وجود فایل و اندازه آن
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
