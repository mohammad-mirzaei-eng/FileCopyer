using FileCopyer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes.Design_Patterns.Helper
{
    public class FindFileModelHelper
    {
        public static FileModel FindFileModel(List<FileModel> fileModels, string file)
        {
            // تبدیل مسیر مبدا و فایل به مسیر نسبی
            var relativePath =GetRelativePathHelper.GetRelativePath(fileModels.First().Source, file);

            return fileModels.FirstOrDefault(f =>
                Path.Combine(f.Source, relativePath).Equals(file, StringComparison.OrdinalIgnoreCase)
            );
        }

    }
}
