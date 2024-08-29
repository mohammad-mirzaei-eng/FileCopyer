using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCopyer.Classes
{
    public class GenerateReportHelper
    {
        public Task GenerateReport(string reportPath, List<string> errorList)
        {
            string date = DateTime.Now.ToString("yyyyMMdd");

            if (errorList.Count > 0)
            {
                string errorReportBasePath = "ErrorReport_";
                string errorReportPath = $"{errorReportBasePath}{date}.txt";
                errorReportPath =new GetNewFilePathHelper().GetNewFilePath(errorReportPath);
                using (StreamWriter errorWriter = new StreamWriter(errorReportPath, true))
                {
                    foreach (var error in errorList)
                    {
                        errorWriter.WriteLine(error);
                    }
                }
            }
            errorList.Clear();

            return Task.CompletedTask;
        }
    }
}
