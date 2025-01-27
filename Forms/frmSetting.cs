using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FileCopyer.Classes;
using FileCopyer.Classes.Design_Patterns.Helper;
using FileCopyer.Models;

namespace FileCopyer.Forms
{
    public partial class frmSetting : Form
    {
        // سازنده فرم تنظیمات
        /// <summary>
        /// Initializes a new instance of the frmSetting class
        /// </summary>
        public frmSetting()
        {
            InitializeComponent();
        }

        private FileModel fileModel = new FileModel();
        private SettingsModel settings = new SettingsModel();
        private List<FileModel> fileModels = new List<FileModel>();

        // بارگذاری مدل‌های فایل
        /// <summary>
        /// Loads the file models and settings from binary files
        /// </summary>
        /// <param name="fromfile">Boolean indicating whether to load from file</param>
        /// <param name="file">Optional file path to load from</param>
        private void LoadFileModels(bool fromfile, string file = "")
        {
            if (fromfile)
            {
                if (string.IsNullOrEmpty(file) && file.Length <= 0)
                {
                    fileModels = BinarySerializationHelper.LoadFileModels();
                }
                else
                {
                    fileModels = BinarySerializationHelper.LoadFileModels(file);
                }
                settings = BinarySerializationHelper.LoadFileSettingsModels();
            }
            // نمایش لیست فایل‌ها در فرم یا کنترل‌های مناسب
            var queryFileModel = from o in fileModels select o.GetResourceName;
            if (queryFileModel != null && queryFileModel.Count() > 0)
            {
                listBox1.DataSource = queryFileModel.ToList();
            }
            else
            {
                listBox1.DataSource = null;
            }

            if (settings != null)
            {
                numMaxThread.Value = settings.MaxThreads;
                ChkDeepCheck.Checked = settings.CheckFileDeep;
                maxBufferSize.Value = settings.MaxBufferSize;
                chkCreateParent.Checked = settings.CreateParentPath;
                chkShowProgressBar.Checked = settings.ShowProgressBar;
            }
        }

        // رویداد کلیک دکمه ذخیره تنظیمات
        /// <summary>
        /// Handles the Click event of the Save button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void button2_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrEmpty(textBox2.Text))
                button1_Click(sender, e);

            BinarySerializationHelper.SaveFileModels(fileModels);
            BinarySerializationHelper.SaveSetting(settings);

            textBox1.Text = String.Empty;
            textBox2.Text = String.Empty;
            this.Close();
        }

        // رویداد کلیک دکمه انتخاب مسیر مبدا
        /// <summary>
        /// Handles the Click event of the Browse Source button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void button3_Click(object sender, EventArgs e)
        {
            using (CustomFolderBrowserForm folderForm = new CustomFolderBrowserForm())
            {
                folderForm.Text = "مسیر فایل مبدا را انتخاب کنید";
                if (Directory.Exists(textBox1.Text))
                {
                    folderForm.SelectedPath = textBox1.Text;
                }
                if (folderForm.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = folderForm.SelectedPath;
                    fileModel.Source = folderForm.SelectedPath;
                }
            }
        }

        // رویداد کلیک دکمه انتخاب مسیر مقصد
        /// <summary>
        /// Handles the Click event of the Browse Destination button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void button4_Click(object sender, EventArgs e)
        {
            using (CustomFolderBrowserForm folderForm = new CustomFolderBrowserForm())
            {
                if (Directory.Exists(textBox2.Text))
                {
                    folderForm.SelectedPath = textBox2.Text;
                }
                folderForm.Text = "مسیر فایل مقصد را انتخاب کنید";
                if (folderForm.ShowDialog() == DialogResult.OK)
                {
                    textBox2.Text = folderForm.SelectedPath;
                    fileModel.Destination = folderForm.SelectedPath;
                }
            }
        }

        // رویداد بارگذاری فرم
        /// <summary>
        /// Handles the Load event of the form
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void frmSetting_Load(object sender, EventArgs e)
        {
            LoadFileModels(true);
        }

        // رویداد کلیک دکمه افزودن مسیر
        /// <summary>
        /// Handles the Click event of the Add Path button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox1.Text))
            {
                MessageBox.Show("مسیر فایل  مبدا انتخابی شما پیدا نشد!", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Directory.Exists(textBox2.Text))
            {
                MessageBox.Show("مسیر فایل انتخابی شما پیدا نشد!", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            fileModels.Add(fileModel);
            LoadFileModels(false);
            textBox1.Text = String.Empty;
            textBox2.Text = String.Empty;
            fileModel = new FileModel();
        }

        // رویداد دوبار کلیک بر روی لیست مسیرها
        /// <summary>
        /// Handles the MouseDoubleClick event of the listBox1
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">MouseEventArgs containing event data</param>
        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DeletSourceFile();
        }

        // متد حذف مسیر انتخابی
        /// <summary>
        /// Deletes the selected source file path from the list
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
                    BinarySerializationHelper.LoadFileModels();
                }
            }
        }

        // رویداد کلیک دکمه حذف
        /// <summary>
        /// Handles the Click event of the Delete button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void toolStripDell_Click(object sender, EventArgs e)
        {
            DeletSourceFile();
        }

        // رویداد تغییر مقدار حداکثر تعداد تردها
        /// <summary>
        /// Handles the ValueChanged event of the numMaxThread control
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void numMaxThread_ValueChanged(object sender, EventArgs e)
        {
            settings.MaxThreads = (int)numMaxThread.Value;
        }

        // رویداد تغییر وضعیت چک باکس بررسی عمیق
        /// <summary>
        /// Handles the CheckedChanged event of the ChkDeepCheck control
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void ChkDeepCheck_CheckedChanged(object sender, EventArgs e)
        {
            settings.CheckFileDeep = ChkDeepCheck.Checked;
        }

        // رویداد تغییر انتخاب در لیست مسیرها
        /// <summary>
        /// Handles the SelectedIndexChanged event of the listBox1
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // رویداد کلیک منوی باز کردن فایل
        /// <summary>
        /// Handles the Click event of the openFileToolStripMenuItem
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "bin file|fileModels.bin";
                ofd.Title = "فایل مسیرها";
                ofd.Multiselect = false;
                ofd.SupportMultiDottedExtensions = false;
                ofd.ValidateNames = true;
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    LoadFileModels(true, ofd.FileName);
                }
            }
        }

        // رویداد تغییر وضعیت چک باکس ایجاد مسیر والد
        /// <summary>
        /// Handles the CheckedChanged event of the chkCreateParent control
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void chkCreateParent_CheckedChanged(object sender, EventArgs e)
        {
            settings.CreateParentPath = chkCreateParent.Checked;
        }

        // رویداد تغییر وضعیت چک باکس نمایش نوار پیشرفت
        /// <summary>
        /// Handles the CheckedChanged event of the chkShowProgressBar control
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void chkShowProgressBar_CheckedChanged(object sender, EventArgs e)
        {
            settings.ShowProgressBar = chkCreateParent.Checked;
        }
    }
}
