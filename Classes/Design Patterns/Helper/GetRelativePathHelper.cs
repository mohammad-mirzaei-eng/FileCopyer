using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Classes.Design_Patterns.Helper
{
    public class GetRelativePathHelper
    {
        public static string GetRelativePath(string fromPath, string toPath)
        {
            int lastIndexfromPath = fromPath.LastIndexOf('\\');
            int lastIndextoPath = toPath.LastIndexOf('\\');
            string relativePathfromPath = fromPath.Substring(lastIndexfromPath + 1); // از اولین کاراکتر بعد از '\\'
            string relativePathtoPath = toPath.Substring(lastIndextoPath + 1); // از اولین کاراکتر بعد از '\\'
            string relativePath = $@"{relativePathfromPath}\{relativePathtoPath}";
            return relativePath;
        }


    }
}
