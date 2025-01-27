using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes.Design_Patterns.Helper
{
    public class HashFileHelper
    {
        /// <summary>
        /// Convert FileStream to Hash
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>MD5 Hash type string</returns>
        public string GetFileHash(FileStream stream)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(); // تبدیل هش به رشته
            }
        }

        /// <summary>
        /// Convert long to Hash
        /// </summary>
        /// <param name="value"></param>
        /// <returns>MD5 Hash type string</returns>
        public string GetFileHash(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(); // تبدیل هش به رشته
            }
        }
    }
}
