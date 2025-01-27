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
    public partial class main : Form, IProgressObserver
    {
        // مدل تنظیمات
        private SettingsModel settingsModel = new SettingsModel();
        // لیست مدل‌های فایل
        private List<FileModel> fileModels = new List<FileModel>();

        // پرچم برای مدیریت اجرای برنامه
        /// <summary>
        /// Flag to manage the running state of the application
        /// </summary>
        private bool runApp = false;

        // سازنده فرم اصلی
        /// <summary>
        /// Initializes a new instance of the main form
        /// </summary>
        public main()
        {
            InitializeComponent();
            var manager = FileCopyManager.Instance;
            manager.RegisterObserver(this);
        }

        // رویداد کپی شدن فایل
        /// <summary>
        /// Event handler for when a file is copied
        /// </summary>
        /// <param name="copiedFiles">Number of copied files</param>
        /// <param name="totalFiles">Total number of files</param>
        /// <param name="errorFiles">Number of error files</param>
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

        // رویداد اتمام کپی
        /// <summary>
        /// Event handler for when the copy process is completed
        /// </summary>
        public void OnCopyCompleted()
        {
            Invoke(new Action(() =>
            {
                lblstatus.BackColor = Color.LimeGreen;
                toolStripstatus.BackColor = Color.LimeGreen;
            }));
        }

        // رویداد کلیک دکمه کپی فایل‌ها
        /// <summary>
        /// Handles the Click event of the CopyFiles button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
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

        // رویداد بارگذاری فرم
        /// <summary>
        /// Handles the Load event of the form
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFileModels(); // بارگذاری مدل‌های فایل
        }

        // رویداد بستن فرم
        /// <summary>
        /// Handles the FormClosing event of the form
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">FormClosingEventArgs containing event data</param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var manager = FileCopyManager.Instance;
            manager.Form_FormClosing(sender, e);
        }

        // رویداد کلیک بر روی برچسب تعداد فایل‌های کپی شده
        /// <summary>
        /// Handles the Click event of the lbltotalcopied label
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void lbltotalcopied_Click(object sender, EventArgs e)
        {
            lbltotalcopied.Text = $"Copied {totalbar.Value}/{totalbar.Maximum} files.";
        }

        // رویداد کلیک دکمه تنظیمات
        /// <summary>
        /// Handles the Click event of the settings button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void button1_Click(object sender, EventArgs e)
        {
            using (frmSetting frmSetting = new frmSetting())
            {
                frmSetting.ShowDialog();
                LoadFileModels();
            }
        }

        // بارگذاری مدل‌های فایل
        /// <summary>
        /// Loads the file models and settings from binary files
        /// </summary>
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

        // رویداد تغییر اندازه فرم
        /// <summary>
        /// Handles the SizeChanged event of the form
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
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

        // رویداد کلیک بر روی گزینه نمایش برنامه
        /// <summary>
        /// Handles the Click event of the showappcms context menu item
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void showappcms_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            this.Show();
        }

        // رویداد دوبار کلیک بر روی آیکون نوتیفیکیشن
        /// <summary>
        /// Handles the MouseDoubleClick event of the notifycontext
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">MouseEventArgs containing event data</param>
        private void notifycontext_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon1.Visible = false;
            this.WindowState = FormWindowState.Normal;
            this.Focus();
            this.Show();
        }

        // رویداد کلیک بر روی گزینه خروج از برنامه
        /// <summary>
        /// Handles the Click event of the exitappcms context menu item
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void exitappcms_Click(object sender, EventArgs e)
        {
            this.Close(); // بستن فرم
        }

        // رویداد کلیک بر روی گزینه حذف
        /// <summary>
        /// Handles the Click event of the toolStripDell tool strip item
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
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
