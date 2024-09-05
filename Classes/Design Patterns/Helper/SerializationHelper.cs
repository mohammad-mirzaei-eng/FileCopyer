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
        private static string GetFilePath(bool isFileModel)
        {
            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileCopyer");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return Path.Combine(directory, isFileModel ? "fileModels.bin" : "settings.bin");
        }

        public static void SaveFileModels(List<FileModel> fileModels)
        {
            string filePath = GetFilePath(true);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, fileModels);
            }
        }

        public static void SaveSettings(SettingsModel settings)
        {
            string filePath = GetFilePath(false);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fs, settings);
            }
        }

        public static List<FileModel> LoadFileModels()
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

        public static SettingsModel LoadSettings()
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
    }

}
