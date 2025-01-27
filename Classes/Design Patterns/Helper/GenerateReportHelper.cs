using FileCopyer.Interface.Design_Patterns.Observer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes.Design_Patterns.Helper
{
    public class GenerateReportHelper
    {
        // متد تولید گزارش
        /// <summary>
        /// Generates a report based on the provided error list and saves it to the specified path
        /// </summary>
        /// <param name="reportPath">The path where the report will be saved</param>
        /// <param name="errorList">List of errors to be included in the report</param>
        /// <returns></returns>
        public async Task GenerateReport(string reportPath, List<string> errorList)
        {
            // دریافت تاریخ فعلی به فرمت yyyyMMdd
            string date = DateTime.Now.ToString("yyyyMMdd");

            // بررسی وجود خطاها در لیست خطاها
            if (errorList.Count > 0)
            {
                // مسیر پایه گزارش خطا
                string errorReportBasePath = "ErrorReport_";
                // ایجاد مسیر کامل گزارش خطا با تاریخ
                string errorReportPath = $"{errorReportBasePath}{date}.txt";
                // دریافت مسیر جدید فایل برای جلوگیری از تداخل نام فایل‌ها
                errorReportPath = new GetNewFilePathHelper().GetNewFilePath(errorReportPath);
                // نوشتن خطاها در فایل گزارش
                using (StreamWriter errorWriter = new StreamWriter(errorReportPath, true, Encoding.UTF8))
                {
                    foreach (var error in errorList)
                    {
                        await errorWriter.WriteLineAsync(error);
                    }
                }
            }
            // پاک کردن لیست خطاها
            errorList.Clear();
        }
    }
}
