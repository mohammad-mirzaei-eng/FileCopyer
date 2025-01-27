using FileCopyer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileCopyer.Classes.Design_Patterns.Helper
{
    internal static class BinarySerializationHelper
    {
        // متد دریافت مسیر فایل
        /// <summary>
        /// Gets the file path for storing serialized data
        /// </summary>
        /// <param name="model">Boolean indicating whether to get the path for file models or settings</param>
        /// <returns>File path as a string</returns>
        private static string GetFilePath(bool model)
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileCopyer");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return Path.Combine(directory, model ? "fileModels.bin" : "settings.bin");
        }

        // متد ذخیره مدل‌های فایل
        /// <summary>
        /// Saves the list of file models to a binary file
        /// </summary>
        /// <param name="fileModels">List of FileModel objects to be saved</param>
        public static void SaveFileModels(List<FileModel> fileModels)
        {
            string filePath = GetFilePath(true);
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
        public static void SaveSetting(SettingsModel settings)
        {
            string filePath = GetFilePath(false);
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
            try
            {
                string filePath = GetFilePath(true);
                if (!File.Exists(filePath))
                {
                    return new List<FileModel>();
                }
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (List<FileModel>)formatter.Deserialize(fs);
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("خطا در نمایش مسیر فایلها ، لطفا دوباره کانفیگ کنید", "خطا", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return new List<FileModel>();
            }
        }

        // متد بارگذاری مدل‌های فایل از مسیر مشخص
        /// <summary>
        /// Loads the list of file models from a specified binary file
        /// </summary>
        /// <param name="file">Path to the binary file</param>
        /// <returns>List of FileModel objects</returns>
        public static List<FileModel> LoadFileModels(string file)
        {
            if (!File.Exists(file))
            {
                return new List<FileModel>();
            }
            using (FileStream fs = new FileStream(file, FileMode.Open))
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
        public static SettingsModel LoadFileSettingsModels()
        {
            try
            {
                string filePath = GetFilePath(false);
                if (!File.Exists(filePath))
                {
                    return new SettingsModel();
                }
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (SettingsModel)formatter.Deserialize(fs);
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("خطا در دریافت تنظیمات ، لطفا دوباره کانفیگ کنید", "خطا", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return new SettingsModel();
            }
        }
    }
}