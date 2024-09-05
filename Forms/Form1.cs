using FileCopyer.Classes.Design_Patterns.Helper;
using FileCopyer.Classes.Design_Patterns.Singleton;
using FileCopyer.Interface.Design_Patterns.Observer;
using FileCopyer.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FileCopyer.Forms
{
    public partial class Form1 : Form, IProgressObserver
    {
        private SettingsModel settingsModel = new SettingsModel();
        private List<FileModel> fileModels = new List<FileModel>(); // لیست مدل‌های فایل

        /// <summary>
        /// 
        /// </summary>
        private bool runApp = false; // پرچم برای مدیریت اجرای برنامه

        public Form1()
        {
            InitializeComponent();
            var manager = FileCopyManager.Instance;
            manager.RegisterObserver(this);
        }

        public void OnFileCopied(int copiedFiles, int totalFiles, int errorFiles)
        {
            Invoke(new Action(() =>
            {
                if (runApp)
                {
                    toolStripstatus.BackColor = lblstatus.BackColor = Color.SpringGreen;
                }
                else
                {
                    toolStripstatus.BackColor = lblstatus.BackColor = SystemColors.Control;
                }
                this.Text = errorFiles > 0 ? $"{errorFiles} خطا در کپی کردن" : $"";
                toolStripstatus.Text = lblstatus.Text = ($"در حال اجرا: {runApp}");
                lblcopeid.Text = copiedFiles.ToString();

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
            try
            {
                if (fileModels.Count > 0)
                {
                    if (!runApp)
                    {
                        (sender as Button).Text = "متوقف کردن";
                        runApp = true;
                        lblstatus.Text = "درحال آماده سازی مقدمات کپی کردن فایلها";
                        lblstatus.BackColor = Color.LightGoldenrodYellow;

                        FileCopyManager.Instance.StartCopy(
                            fileModels,
                            flowLayoutPanel1,
                            totalbar,
                            settingsModel);
                    }
                    else
                    {
                        (sender as Button).Text = "اجرا";
                        runApp = false;
                        FileCopyManager.Instance.CancelCopy();
                    }
                }
                else
                {
                    MessageBox.Show("مسیر کپی فایل مشخص نشده است", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
