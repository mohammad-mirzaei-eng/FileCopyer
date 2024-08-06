using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Task.Run(() => DisplayStatus()); // اجرای تابع نمایش وضعیت به صورت غیرهمزمان
            //numericMaxThreads.Value = maxThreads; // مقدار پیش‌فرض
            semaphore = new SemaphoreSlim(maxThreads);
        }
        /// <summary>
        /// TODO
        /// </summary>
        private bool CheckFileDeep=false;

        private CancellationTokenSource cts;

        private CancellationToken cancellationToken;

        // متغیرهای سراسری
        private string tempPath = string.Empty;

        private string tempFilePath = string.Empty;

        private string oldFilePath = string.Empty;

        /// <summary>
        /// TODO
        /// </summary>
        private int maxThreads = 15; // مقدار پیش‌فرض

        private SemaphoreSlim semaphore; // سمفور برای مدیریت تعداد وظایف همزمان
        /// <summary>
        /// TODO
        /// </summary>
        private bool tempFile = false; // اگر کاربر بخواهد از فایل موقت استفاده کند

        /// <summary>
        /// 
        /// </summary>
        private bool runApp = false; // پرچم برای مدیریت اجرای برنامه

        /// <summary>
        /// 
        /// </summary>
        private List<FileModel> fileModels = new List<FileModel>(); // لیست مدل‌های فایل

        /// <summary>
        /// 
        /// </summary>
        private SettingsModel settingsModel = new SettingsModel();

        /// <summary>
        /// 
        /// </summary>
        private int totalFiles = 0; // تعداد کل فایل‌ها

        /// <summary>
        /// 
        /// </summary>
        private int copiedFiles = 0; // تعداد فایل‌های کپی شده

        /// <summary>
        /// 
        /// </summary>
        private object lockObj = new object(); // شیء برای قفل کردن عملیات‌ها

        /// <summary>
        /// 
        /// </summary>
        private List<string> errorList = new List<string>(); // لیست خطاها
        /// <summary>
        /// 
        /// </summary>
        private ConcurrentDictionary<string, bool> copyingFiles = new ConcurrentDictionary<string, bool>(); // دیکشنری برای مدیریت فایل‌های در حال کپی

        /// <summary>
        /// 
        /// </summary>
        private ConcurrentDictionary<string, bool> filesCopied = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 
        /// </summary>
        private bool isExiting = false; // پرچم برای خروج برنامه

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isCopied"></param>
        private void UpdateFileCounts(bool isCopied)
        {
            lock (lockObj)
            {
                if (isCopied)
                {
                    copiedFiles++;
                }
                else
                {
                    totalFiles++;
                }
            }
        }

        // رویداد کلیک دکمه شروع/توقف
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnstart_Click(object sender, EventArgs e)
        {
            if (fileModels.Count > 0)
            {
                if (!runApp)
                {
                    (sender as Button).Text = "متوقف کردن";
                    runApp = true;
                }
                else
                {
                    (sender as Button).Text = "اجرا";
                    runApp = false;
                    cts?.Cancel(); // لغو عملیات‌های در حال اجرا
                    totalFiles = 0;
                    copiedFiles = 0;
                }

                cts = new CancellationTokenSource(); // ایجاد `CancellationTokenSource` جدید
                cancellationToken = cts.Token;
                Task.Run(() => StartCopyProcess(runApp)); // شروع فرآیند کپی
            }
            else
            {
                MessageBox.Show("مسیر کپی فایل مشخص نشده است", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }




        // رویداد کلیک منوی خروج
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitappcms_Click(object sender, EventArgs e)
        {
            this.Close(); // بستن فرم
        }

        // رویداد بستن فرم
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            closeForm(e); // اجرای فرآیند بستن فرم
        }

        // متد بستن فرم
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private async void closeForm(FormClosingEventArgs e)
        {
            if (copyingFiles.Count > 0) // چک کردن وجود فایل‌های در حال کپی
            {
                DialogResult result = MessageBox.Show("مطمئن هستید میخواهید خروج کنید؟", "اعلان", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                isExiting = true; // تنظیم پرچم خروج
                lblstatus.Text = "درحال خروج، لطفا کمی صبر کنید تا فایل بدرستی کپی شود";

                cts?.Cancel(); // لغو تمام عملیات‌های کپی

                while (copyingFiles.Count > 0) // انتظار برای اتمام کپی
                {
                    await Task.Delay(100); // استفاده از تاخیر غیرمسدود کننده
                }
            }

            await GenerateReport(); // تولید گزارش
        }


        // متد شروع فرآیند کپی
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        private async void StartCopyProcess(bool start)
        {
            if (start)
            {
                while (runApp && !isExiting)
                {
                    var copyTasks = new List<Task>();
                    foreach (FileModel model in fileModels)
                    {
                        copyTasks.Add(Task.Run(async () =>
                        {
                            await semaphore.WaitAsync();

                            try
                            {
                                if (isExiting || cancellationToken.IsCancellationRequested || !runApp)
                                {
                                    copyingFiles.Clear();
                                    return;
                                }
                                await CopyDirectoryWithThreads(model.Source, model.Destination);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        }, cancellationToken));
                    }
                    await Task.WhenAll(copyTasks);
                    // بررسی وضعیت هر 1 ثانیه
                    await Task.Delay(1000);
                }
            }
            else
            {
                cts?.Cancel(); // لغو عملیات‌ها
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private HashSet<string> GetFileNames(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath);
                return new HashSet<string>(files.Select(Path.GetFileName));
            }
            return new HashSet<string>();
        }

        // متد کپی دایرکتوری با استفاده از تردها
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destDir"></param>
        /// <returns></returns>
        private async Task CopyDirectoryWithThreads(string sourceDir, string destDir)
        {
            if (isExiting || cancellationToken.IsCancellationRequested || !runApp)
            {
                copyingFiles.Clear();
                return;
            }

            // دریافت نام فایل‌های مبدا و مقصد برای مقایسه
            HashSet<string> sourceFileNames = GetFileNames(sourceDir);
            HashSet<string> destFileNames = GetFileNames(destDir);
            // شناسایی فایل‌های جدید یا تغییر یافته
            var newOrChangedFiles = sourceFileNames.Except(destFileNames).ToList();
            if (!sourceFileNames.SetEquals(destFileNames))
            {
                lock (lockObj)
                {
                    errorList.Add($"نام فایل‌های مبدا و مقصد در دایرکتوری {sourceDir} برابر نیستند.");
                }
                copyingFiles.TryRemove(sourceDir, out _);
            }

            if (newOrChangedFiles.Count > 0)
            {
                lock (lockObj)
                {
                    totalFiles += newOrChangedFiles.Count; // تنها فایل‌های جدید را به totalFiles اضافه کنید
                }
            }

            if (!copyingFiles.TryAdd(sourceDir, true))
            {
                return;
            }

            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            FileInfo[] files = dir.GetFiles();

            List<Task> fileCopyTasks = new List<Task>();

            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDir, file.Name);
                string tempFilePath = tempPath + ".tmp"; // مسیر فایل موقت
                // اگر فایل قبلاً کپی شده، از کپی دوباره آن صرف‌نظر کنید
                if (filesCopied.ContainsKey(file.FullName))
                {
                    if (File.Exists(tempPath))
                    {
                        continue;
                    }
                }

                fileCopyTasks.Add(Task.Run(async () =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        if (isExiting || cancellationToken.IsCancellationRequested || !runApp)
                        {
                            copyingFiles.Clear();
                            return;
                        }

                        if (!copyingFiles.TryAdd(file.FullName, true))
                        {
                            return;
                        }

                        if (File.Exists(tempPath))
                        {
                            if (CheckFileDeep)
                            {
                                ChangeFileNameWhenFileDestinationChanged(file, tempPath);
                            }
                            else
                            {
                                await CopyFileWithStream(file.FullName, tempPath, tempFile);
                            }
                        }
                        else
                        {
                            lock (lockObj)
                            {
                                copiedFiles++;
                            }
                            filesCopied.TryAdd(file.FullName, true);
                        }

                        await CopyFileWithStream(file.FullName, tempPath, tempFile);

                        if (VerifyFileCopy(file.FullName, tempPath))
                        {
                            lock (lockObj)
                            {
                                UpdateFileCounts(true);
                            }
                            filesCopied.TryAdd(file.FullName, true);
                        }
                        else
                        {
                            lock (lockObj)
                            {
                                errorList.Add($"خطا برای کپی فایل :  {file.FullName}");
                            }
                        }
                    }
                    finally
                    {
                        semaphore.Release();
                        copyingFiles.TryRemove(file.FullName, out _);
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(fileCopyTasks);

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDir, subdir.Name);
                await CopyDirectoryWithThreads(subdir.FullName, tempPath);
            }
        }

        #region Check file For Chang by virus or ....
        private void ChangeFileNameWhenFileDestinationChanged(FileInfo file, string tempPath)
        {
            using (FileStream sourceStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            using (FileStream destStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
            {
                string sourceHash = GetFileHash(sourceStream);
                string destHash = GetFileHash(destStream);

                if (sourceHash != destHash)
                {
                    string oldFilePath = tempPath + ".old";
                    try
                    {
                        int count = 1;
                        while (File.Exists(oldFilePath))
                        {
                            oldFilePath = tempPath + $".old{count}";
                            count++;
                        }
                        File.Move(tempPath, oldFilePath);
                        lock (lockObj)
                        {
                            errorList.Add($"فایل تغییر یافته و به مسیر قدیمی منتقل شد: {file.FullName} به {oldFilePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (lockObj)
                        {
                            errorList.Add($"خطا در جابه‌جایی فایل: {file.FullName} به {oldFilePath}. خطا: {ex.Message}");
                        }
                    }
                }
                else
                {
                    lock (lockObj)
                    {
                        UpdateFileCounts(true);
                    }
                }
            }
        }
        #endregion

        // متد کپی فایل با استفاده از Stream
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <param name="useTempFile"></param>
        /// <returns></returns>
        private async Task CopyFileWithStream(string sourceFilePath, string destFilePath, bool useTempFile)
        {
            string tempFilePath = useTempFile ? destFilePath + ".temp" : destFilePath;

            try
            {
                using (FileStream sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
                using (FileStream destStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    const int bufferSize = 1024 * 1024;
                    byte[] buffer = new byte[bufferSize];
                    int bytesRead;
                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await destStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                if (useTempFile)
                {
                    if (File.Exists(destFilePath))
                    {
                        File.Delete(destFilePath);
                    }
                    File.Move(tempFilePath, destFilePath);
                }

                lock (lockObj)
                {
                    copiedFiles++;
                }
                filesCopied.TryAdd(sourceFilePath, true);
            }
            catch (Exception ex)
            {
                lock (lockObj)
                {
                    errorList.Add($"خطا در کپی کردن فایل: {sourceFilePath} به {destFilePath}. خطا: {ex.Message}");
                }
            }
        }

        // متد بررسی کپی موفق فایل
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <param name="destFile"></param>
        /// <returns></returns>
        private bool VerifyFileCopy(string sourceFile, string destFile)
        {
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read))
            using (FileStream destStream = new FileStream(destFile, FileMode.Open, FileAccess.Read))
            {
                string sourceHash = GetFileHash(sourceStream);
                string destHash = GetFileHash(destStream);

                return sourceHash == destHash; // بررسی یکسان بودن هش فایل‌های مبدا و مقصد
            }
        }

        // متد محاسبه هش فایل
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private string GetFileHash(FileStream stream)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(); // تبدیل هش به رشته
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Task GenerateReport()
        {
            string reportBasePath = "Report_";
            string date = DateTime.Now.ToString("yyyyMMdd");

            string reportPath = $"{reportBasePath}{date}.txt";

            reportPath = GetNewFilePath(reportPath);

            using (StreamWriter writer = new StreamWriter(reportPath, true))
            {
                writer.WriteLine($"تاریخ: {DateTime.Now}");
                writer.WriteLine($"مقدار کلی فایلها: {totalFiles}");
                writer.WriteLine($"مقدار کپی شده: {copiedFiles}");
                writer.WriteLine($"کپی شده (درصد): {(double)copiedFiles / totalFiles * 100}%");
                writer.WriteLine();
            }

            if (errorList.Count > 0)
            {
                string errorReportBasePath = "ErrorReport_";
                string errorReportPath = $"{errorReportBasePath}{date}.txt";
                errorReportPath = GetNewFilePath(errorReportPath);
                using (StreamWriter errorWriter = new StreamWriter(errorReportPath, true))
                {
                    foreach (var error in errorList)
                    {
                        errorWriter.WriteLine(error);
                    }
                }
            }

            lock (lockObj)
            {
                totalFiles = 0;
                copiedFiles = 0;
                errorList.Clear();
            }
            return Task.CompletedTask;
        }

        // متد دریافت مسیر فایل جدید
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseFilePath"></param>
        /// <returns></returns>
        private string GetNewFilePath(string baseFilePath)
        {
            string directory = Path.GetDirectoryName(baseFilePath);
            string fileName = Path.GetFileNameWithoutExtension(baseFilePath);
            string extension = Path.GetExtension(baseFilePath);
            int partNumber = 1;

            string newFilePath = baseFilePath;

            while (File.Exists(newFilePath) && new FileInfo(newFilePath).Length > (1 * 1024 * 1024)) // بررسی وجود فایل و اندازه آن
            {
                newFilePath = Path.Combine(directory, $"{fileName}_part{partNumber}{extension}");
                partNumber++;
            }

            return newFilePath;
        }

        // متد نمایش وضعیت
        /// <summary>
        /// 
        /// </summary>
        private void DisplayStatus()
        {
            while (!isExiting || !cancellationToken.IsCancellationRequested)
            {
                lock (lockObj)
                {
                    if (runApp)
                    {
                        if (totalFiles > 0)
                        {
                            Invoke(new Action(() =>
                            {
                                totalbar.Maximum = totalFiles;
                                totalbar.Value = copiedFiles;
                            }));
                        }
                        Invoke(new Action(() =>
                        {
                            toolStripstatus.BackColor = lblstatus.BackColor = Color.SpringGreen;
                        }));
                    }
                    else
                    {
                        Invoke(new Action(() =>
                        {
                            toolStripstatus.BackColor = lblstatus.BackColor = SystemColors.Control;
                        }));
                    }
                    Invoke(new Action(() =>
                    {
                        toolStripstatus.Text = lblstatus.Text = ($"در حال اجرا: {runApp}");
                        lbltotalcopied.Text = totalFiles.ToString();
                        lblcopeid.Text = copiedFiles.ToString();
                    }));
                }
                Thread.Sleep(1000);
            }
        }

        // متد بارگذاری مدل‌های فایل
        /// <summary>
        /// 
        /// </summary>
        private void LoadFileModels()
        {
            fileModels = BinarySerializationHelper.LoadFileModels();
            settingsModel = BinarySerializationHelper.LoadFileSettingsModels();
            // نمایش لیست فایل‌ها در فرم یا کنترل‌های مناسب

            var query = from o in fileModels select o.GetResourceName;
            if (query != null && query.Count() > 0)
            {
                listBox1.DataSource = query.ToList();
            }
            else
            {
                listBox1.DataSource = null;
            }
            if (settingsModel!=null)
            {
                maxThreads = settingsModel.maxThreads;
                CheckFileDeep = settingsModel.CheckFileDeep;
            }
        }

        // رویداد بارگذاری فرم
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFileModels(); // بارگذاری مدل‌های فایل
        }

        // رویداد کلیک دکمه نمایش فرم تنظیمات
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            using (frmSetting frmSetting = new frmSetting())
            {
                frmSetting.ShowDialog();
                LoadFileModels();
            }
        }

        // رویداد تغییر سایز فرم
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIcon1.Visible = true;
                this.Hide();
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                notifyIcon1.Visible = false;
                this.Show();
            }
        }

        // رویداد کلیک نمایش برنامه از نوتیفیکیشن
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showappcms_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            this.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon1.Visible = false;
            this.WindowState = FormWindowState.Normal;
            this.Focus();
            this.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox1_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            DeletSourceFile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripDell_Click(object sender, EventArgs e)
        {
            DeletSourceFile();
        }

        /// <summary>
        /// 
        /// </summary>
        private void DeletSourceFile()
        {
            if (listBox1.SelectedItems != null && listBox1.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("آیا از حذف مسیر انتخابی مطمئن هستید؟", "اخطار", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    foreach (var listselect in listBox1.SelectedItems)
                    {
                        var item = from o in fileModels
                                   where o.GetResourceName == listselect.ToString()
                                   select o;
                        fileModels.Remove(item.FirstOrDefault());
                    }
                    BinarySerializationHelper.SaveFileModels(fileModels);
                    LoadFileModels();
                }
            }
        }
    }
}