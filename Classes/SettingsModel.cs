using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes
{
    [Serializable] // مشخص می‌کند که کلاس می‌تواند سریال‌سازی شود.
    public class SettingsModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int maxThreads { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool CheckFileDeep { get; set; }
    }
}
