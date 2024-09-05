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
        public async Task GenerateReport(string reportPath, List<string> errorList)
        {
            string date = DateTime.Now.ToString("yyyyMMdd");

            if (errorList.Count > 0)
            {
                string errorReportBasePath = "ErrorReport_";
                string errorReportPath = $"{errorReportBasePath}{date}.txt";
                errorReportPath =new GetNewFilePathHelper().GetNewFilePath(errorReportPath);
                using (StreamWriter errorWriter = new StreamWriter(errorReportPath, true,Encoding.UTF8))
                {
                    foreach (var error in errorList)
                    {
                       await errorWriter.WriteLineAsync(error);
                    }
                }
            }
            errorList.Clear();
        }
    }
}
