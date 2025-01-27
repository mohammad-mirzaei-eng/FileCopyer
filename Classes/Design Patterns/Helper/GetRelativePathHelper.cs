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
        // متد دریافت مسیر نسبی
        /// <summary>
        /// Gets the relative path from a source path to a destination path
        /// </summary>
        /// <param name="fromPath">Source path</param>
        /// <param name="toPath">Destination path</param>
        /// <param name="createParentPath">Boolean indicating whether to include the parent path</param>
        /// <returns>Relative path as a string</returns>
        public static string GetRelativePath(string fromPath, string toPath, bool createParentPath)
        {
            // بررسی خالی بودن مسیر مبدا
            if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            // بررسی خالی بودن مسیر مقصد
            if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            // ایجاد URI از مسیر مبدا
            Uri fromUri = new Uri(fromPath);
            // ایجاد URI از مسیر مقصد
            Uri toUri = new Uri(toPath);

            // بررسی تفاوت درایوها
            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // Paths from different drives

            // ایجاد URI نسبی
            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            // تبدیل URI نسبی به رشته
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            // تبدیل / به \
            relativePath = relativePath.Replace('/', '\\');

            // حذف .\ از ابتدای مسیر
            if (relativePath.StartsWith(@".\"))
                relativePath = relativePath.Substring(2);
            // بررسی نیاز به ایجاد مسیر والد
            if (createParentPath)
                return relativePath;
            else
            {
                // حذف پوشه والد از مسیر نسبی
                int index = relativePath.IndexOf('\\');
                if (index > 0)
                    return relativePath.Substring(index + 1);
                return relativePath;
            }
        }
    }
}
