using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileCopyer.Classes;
using FileCopyer.Interface;
using FileCopyer.Models;

namespace FileCopyer.Forms
{
    public partial class Form1 : Form,IProgressObserver
    {
        //private List<FileModel> files;
        private CancellationTokenSource cancellationTokenSource;
        SettingsModel settingsModel;
        /// <summary>
        /// 
        /// </summary>
        private List<FileModel> fileModels = new List<FileModel>(); // لیست مدل‌های فایل

        public Form1()
        {
            InitializeComponent();
            settingsModel = new SettingsModel();
            settingsModel.MaxThreads = 15;
            settingsModel.CheckFileDeep = false;
        }

        public void OnFileCopied(int copiedFiles, int totalFiles)
        {
            Invoke(new Action(() =>
            {
                // به‌روزرسانی UI با تعداد فایل‌های کپی‌شده
                lblstatus.Text = $"Copied {copiedFiles}/{totalFiles} files.";
                totalbar.Maximum = totalFiles;
                totalbar.Value = copiedFiles;
            }));
        }

        public void OnCopyCompleted()
        {
            Invoke(new Action(() =>
            {
                // به‌روزرسانی UI در صورت اتمام کپی
                //lblstatus.Text = "Copy completed!";
                lblstatus.BackColor = Color.LimeGreen;
            }));
        }

        private void CopyFilesButton_Click(object sender, EventArgs e)
        {
            IFileCopyStrategy strategy = new DefaultCopyStrategy(settingsModel);
            cancellationTokenSource = new CancellationTokenSource();
            strategy.AddObserver(this);
            // Load all file models and create copy operations
            foreach (var fileModel in fileModels)
            {
                // ایجاد و افزودن عملیات کپی به لیست
                IFileOperation copyOperation = new CopyFileOperation(fileModel.Source, fileModel.Destination, strategy, flowLayoutPanel1, cancellationTokenSource.Token);
                FileCopyManager.Instance.AddOperation(copyOperation);
            }

            // Get the list of operations from FileCopyManager and execute them
            var operations = FileCopyManager.Instance.GetOperations();
            foreach (var operation in operations)
            {
                FileCopyManager.Instance.ExecuteOperation(operation);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            //fileModels = new List<FileModel>();
            //FileModel fileModel = new FileModel();
            //fileModel.Source = @"D:\Telegram Desktop";
            //fileModel.Destination = @"D:\test";
            //fileModel.Source = @"\\192.168.110.22\Fileserver\IT\driver";
            //fileModel.Destination = @"D:\tmp";
            //fileModels.Add(fileModel);
            //var query = from o in fileModels select o.GetResourceName;
            //listBox1.DataSource = query.ToList();
            LoadFileModels(); // بارگذاری مدل‌های فایل
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var manager = FileCopyManager.Instance;

            if (manager.IsCopyingInProgress())
            {
                var result = MessageBox.Show("مطمئن هستید می‌خواهید خروج کنید؟", "تایید خروج", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                e.Cancel = true; // جلوگیری از بستن فوری برنامه
                Task.Run(async () =>
                {
                    await manager.WaitForCopyCompletion();
                    this.Invoke((MethodInvoker)(() => this.Close()));
                });
            }
        }

        private void lbltotalcopied_Click(object sender, EventArgs e)
        {
            lbltotalcopied.Text = $"Copied {totalbar.Value.ToString()}/{totalbar.Maximum.ToString()} files.";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (frmSetting frmSetting = new frmSetting())
            {
                frmSetting.ShowDialog();
                LoadFileModels();
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
        }
    }

}