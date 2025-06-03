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
        // سازنده کلاس SettingsModel
        /// <summary>
        /// Initializes a new instance of the SettingsModel class with default values
        /// </summary>
        public SettingsModel()
        {
            MaxBufferSize = 1;
            CheckFileDeep = false;
            MaxThreads = 5;
            CreateParentPath = true;
            ShowProgressBar = true;
            OverwriteFiles = false; // Default to not overwriting files
        }

        // حداکثر تعداد تردها
        /// <summary>
        /// Gets or sets the maximum number of threads
        /// </summary>
        public int MaxThreads { get; set; }

        // بررسی عمیق فایل
        /// <summary>
        /// Gets or sets a value indicating whether to perform a deep check of the files
        /// </summary>
        public bool CheckFileDeep { get; set; }

        // حداکثر اندازه بافر
        /// <summary>
        /// Gets or sets the maximum buffer size in MB
        /// </summary>
        public int MaxBufferSize { get; set; }

        // ایجاد مسیر والد
        /// <summary>
        /// Gets or sets a value indicating whether to create the parent path
        /// </summary>
        public bool CreateParentPath { get; set; }

        // نمایش نوار پیشرفت
        /// <summary>
        /// Gets or sets a value indicating whether to show the progress bar
        /// </summary>
        public bool ShowProgressBar { get; set; }

        // Overwrite existing files
        /// <summary>
        /// Gets or sets a value indicating whether to overwrite existing files at the destination.
        /// </summary>
        public bool OverwriteFiles { get; set; }
    }
}
