using System;
using System.IO;
namespace FileCopyer.Models
{
    [Serializable] // مشخص می‌کند که کلاس می‌تواند سریال‌سازی شود.
    public class FileModel
    {
        // مسیر مبدا
        /// <summary>
        /// مسیر مبدا را Get و Set میکند بصورت String باشد
        /// </summary>
        public string Source { get; set; }

        // مسیر مقصد
        /// <summary>
        /// مسیر مقصد را Get و Set میکند بصورت String باشد
        /// </summary>
        public string Destination { get; set; }

        // متدی که مسیر مبدا و مقصد را برمی‌گرداند
        /// <summary>
        /// وقتی مبدا و مقصد مشخص شده باشند بصورت String کنار همدیگر نمایش داده میشوند
        /// </summary>
        public string GetResourceName
        {
            get
            {
                // اگر هر دو مسیر مبدا و مقصد تعیین شده باشند، آنها را برمی‌گرداند
                if (!string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(Destination))
                {
                    return $"مبدا: {Source} ، مقصد: {Destination}";
                }
                else
                {
                    return "مبدا یا مقصد تعیین نشده است."; // در غیر اینصورت پیغام خطا می‌دهد
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string GetName
        {
            get
            {
                // اگر هر دو مسیر مبدا و مقصد تعیین شده باشند، آنها را برمی‌گرداند
                if (!string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(Destination))
                {
                    return $"{Path.GetFileName(Source)}";
                }
                else
                {
                    return "";
                }
            }
        }
    }
}