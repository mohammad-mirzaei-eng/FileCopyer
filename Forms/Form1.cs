using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileCopyer.Classes;
using FileCopyer.Classes.Design_Patterns.Helper;
using FileCopyer.Classes.Design_Patterns.Singleton;
using FileCopyer.Classes.Design_Patterns.Strategy;
using FileCopyer.Interface;
using FileCopyer.Interface.Design_Patterns.Observer;
using FileCopyer.Interface.Design_Patterns.Strategy;
using FileCopyer.Models;

namespace FileCopyer.Forms
{
    public partial class Form1 : Form, IProgressObserver
    {
        private CancellationTokenSource cancellationTokenSource;
        private SettingsModel settingsModel;
        private List<FileModel> fileModels = new List<FileModel>(); // لیست مدل‌های فایل

        public Form1()
        {
            InitializeComponent();
            settingsModel = new SettingsModel();
        }

        public void OnFileCopied(int copiedFiles, int totalFiles)
        {
            //Invoke(new Action(() =>
            //{
            //    lbltotalcopied.Text = $"Copied {copiedFiles}/{totalFiles} files.";
            //    totalbar.Maximum = totalFiles;
            //    totalbar.Value = copiedFiles;
            //}));

            FileCopyManager.Instance.UpdateFileCopyStatus(
                fileModels.FirstOrDefault()?.Source,
                true
            );
            Invoke(new Action(() =>
            {
                lbltotalcopied.Text = $"Copied {copiedFiles}/{totalFiles} files.";
                totalbar.Maximum = totalFiles;
                totalbar.Value = copiedFiles;
            }));
        }

        public void OnCopyCompleted()
        {
            Invoke(new Action(() =>
            {
                lblstatus.BackColor = Color.LimeGreen;
                toolStripstatus.BackColor = Color.LimeGreen;
            }));
        }

        private void CopyFilesButton_Click(object sender, EventArgs e)
        {
            // استفاده از استراتژی جدید با تنظیمات مدل
            //IFileCopyStrategy strategy = new DefaultCopyStrategy(settingsModel);
            //strategy.AddObserver(this);

            //// ایجاد و افزودن عملیات کپی به لیست
            //foreach (var fileModel in fileModels)
            //{
            //    IFileOperation copyOperation = new CopyFileOperation(fileModel.Source, fileModel.Destination, strategy, flowLayoutPanel1, cancellationTokenSource.Token);
            //    FileCopyManager.Instance.AddOperation(copyOperation);
            //    FileCopyManager.Instance.UpdateFileCopyStatus(fileModel.Source, true); // اضافه کردن فایل به لیست در حال کپی
            //}

            //// شروع عملیات کپی
            //cancellationTokenSource = new CancellationTokenSource();
            //FileCopyManager.Instance.StartCopy(fileModels.FirstOrDefault()?.Source, fileModels.FirstOrDefault()?.Destination, flowLayoutPanel1, totalbar, settingsModel);

            var manager = FileCopyManager.Instance;
            manager.RegisterObserver(this);
            
                FileCopyManager.Instance.StartCopy(
                fileModels,
                flowLayoutPanel1,
                totalbar
            );
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFileModels(); // بارگذاری مدل‌های فایل
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var manager = FileCopyManager.Instance;
            manager.Form_FormClosing(sender, e);
        }

        private void lbltotalcopied_Click(object sender, EventArgs e)
        {
            lbltotalcopied.Text = $"Copied {totalbar.Value}/{totalbar.Maximum} files.";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (frmSetting frmSetting = new frmSetting())
            {
                frmSetting.ShowDialog();
                LoadFileModels();
            }
        }

        private void LoadFileModels()
        {
            fileModels = BinarySerializationHelper.LoadFileModels();
            settingsModel = BinarySerializationHelper.LoadFileSettingsModels();

            var query = from o in fileModels select o.GetResourceName;
            if (query != null && query.Any())
            {
                listBox1.DataSource = query.ToList();
            }
            else
            {
                listBox1.DataSource = null;
            }
        }

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

        private void showappcms_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            this.Show();
        }

        private void notifycontext_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon1.Visible = false;
            this.WindowState = FormWindowState.Normal;
            this.Focus();
            this.Show();
        }

        private void exitappcms_Click(object sender, EventArgs e)
        {
            this.Close(); // بستن فرم
        }

        private void toolStripDell_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems != null && listBox1.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("آیا از حذف مسیر انتخابی مطمئن هستید؟", "اخطار", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    foreach (var listselect in listBox1.SelectedItems)
                    {
                        var item = fileModels.FirstOrDefault(o => o.GetResourceName == listselect.ToString());
                        if (item != null)
                        {
                            fileModels.Remove(item);
                        }
                    }
                    BinarySerializationHelper.SaveFileModels(fileModels);
                    LoadFileModels();
                }
            }
        }
    }
}
