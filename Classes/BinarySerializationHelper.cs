using FileCopyer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileCopyer.Classes
{
    internal static class BinarySerializationHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static string GetFilePath(bool model)
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileCopyer");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return Path.Combine(directory, model ? "fileModels.bin" : "settings.bin");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileModels"></param>
        public static void SaveFileModels(List<FileModel> fileModels)
        {
            string filePath = GetFilePath(true);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, fileModels);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public static void SaveSetting(SettingsModel settings)
        {
            string filePath = GetFilePath(false);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, settings);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("خطا در نمایش مسیر فایلها ، لطفا دوباره کانفیگ کنید","خطا",System.Windows.Forms.MessageBoxButtons.OK,System.Windows.Forms.MessageBoxIcon.Error);
                return new List<FileModel>();
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("خطا در دریافت تنظیمات ، لطفا دوباره کانفیگ کنید", "خطا", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return new SettingsModel();
            }
            
        }
    }
}