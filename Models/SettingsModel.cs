using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Models
{
    [Serializable] // مشخص می‌کند که کلاس می‌تواند سریال‌سازی شود.
    public class SettingsModel
    {
        /// <summary>
        /// 
        /// </summary>
        public int MaxThreads { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool CheckFileDeep { get; set; }
    }
}
