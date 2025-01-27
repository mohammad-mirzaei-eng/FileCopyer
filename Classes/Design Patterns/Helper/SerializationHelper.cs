using FileCopyer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes.Design_Patterns.Helper
{
    internal static class SerializationHelper
    {
        // متد دریافت مسیر فایل
        /// <summary>
        /// Gets the file path for storing serialized data
        /// </summary>
        /// <param name="isFileModel">Boolean indicating whether to get the path for file models or settings</param>
        /// <returns>File path as a string</returns>
        private static string GetFilePath(bool isFileModel)
        {
            // دریافت مسیر دایرکتوری در پوشه ApplicationData
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileCopyer");
            // بررسی وجود دایرکتوری و ایجاد آن در صورت عدم وجود
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            // بازگرداندن مسیر فایل بر اساس نوع داده
            return Path.Combine(directory, isFileModel ? "fileModels.bin" : "settings.bin");
        }

        // متد ذخیره مدل‌های فایل
        /// <summary>
        /// Saves the list of file models to a binary file
        /// </summary>
        /// <param name="fileModels">List of FileModel objects to be saved</param>
        public static void SaveFileModels(List<FileModel> fileModels)
        {
            // دریافت مسیر فایل
            string filePath = GetFilePath(true);
            // ایجاد فایل و سریال‌سازی داده‌ها
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, fileModels);
            }
        }

        // متد ذخیره تنظیمات
        /// <summary>
        /// Saves the settings model to a binary file
        /// </summary>
        /// <param name="settings">SettingsModel object to be saved</param>
        public static void SaveSettings(SettingsModel settings)
        {
            // دریافت مسیر فایل
            string filePath = GetFilePath(false);
            // ایجاد فایل و سریال‌سازی داده‌ها
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, settings);
            }
        }

        // متد بارگذاری مدل‌های فایل
        /// <summary>
        /// Loads the list of file models from a binary file
        /// </summary>
        /// <returns>List of FileModel objects</returns>
        public static List<FileModel> LoadFileModels()
        {
            // دریافت مسیر فایل
            string filePath = GetFilePath(true);
            // بررسی وجود فایل
            if (!File.Exists(filePath))
            {
                return new List<FileModel>();
            }
            // باز کردن فایل و دی‌سریال‌سازی داده‌ها
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (List<FileModel>)formatter.Deserialize(fs);
            }
        }

        // متد بارگذاری تنظیمات
        /// <summary>
        /// Loads the settings model from a binary file
        /// </summary>
        /// <returns>SettingsModel object</returns>
        public static SettingsModel LoadSettings()
        {
            // دریافت مسیر فایل
            string filePath = GetFilePath(false);
            // بررسی وجود فایل
            if (!File.Exists(filePath))
            {
                return new SettingsModel();
            }
            // باز کردن فایل و دی‌سریال‌سازی داده‌ها
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (SettingsModel)formatter.Deserialize(fs);
            }
        }
    }

}
