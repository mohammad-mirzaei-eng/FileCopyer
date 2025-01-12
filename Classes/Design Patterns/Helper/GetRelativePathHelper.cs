using FileCopyer.Models;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromPath">Source File</param>
        /// <param name="file">File Name</param>
        /// <param name="topath">Destination File</param>
        /// <param name="settingsModel">Settings Model</param>
        /// <returns></returns>
        public static string GetRelativePath(string fromPath, string toPath, bool createParentPath)
        {
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // Paths from different drives

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // تبدیل / به \
            relativePath = relativePath.Replace('/', '\\');

            if (relativePath.StartsWith(@".\"))
                relativePath = relativePath.Substring(2);
            if (createParentPath)
                return relativePath;
            else
            {
                int index = relativePath.IndexOf('\\');
                if (index > 0)
                    return relativePath.Substring(index + 1);
                return relativePath;
            }
        }
    }
}
