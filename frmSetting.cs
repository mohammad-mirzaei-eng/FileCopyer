using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FileCopyer
{
    public partial class frmSetting : Form
    {
        public frmSetting()
        {
            InitializeComponent();
        }

        private FileModel fileModel = new FileModel();
        private SettingsModel settings = new SettingsModel();
        private List<FileModel> fileModels = new List<FileModel>();

        private void LoadFileModels(bool fromfile)
        {
            if (fromfile)
            {
                fileModels = BinarySerializationHelper.LoadFileModels();
                settings=BinarySerializationHelper.LoadFileSettingsModels();
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
                numMaxThread.Value = settings.maxThreads;
                ChkDeepCheck.Checked = settings.CheckFileDeep;
            }
        }

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

        private void button3_Click(object sender, EventArgs e)
        {
            using (CustomFolderBrowserForm folderForm = new CustomFolderBrowserForm())
            {
                folderForm.Text = "مسیر فایل مبدا را انتخاب کنید";
                if (folderForm.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = folderForm.SelectedPath;
                    fileModel.Source = folderForm.SelectedPath;
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (CustomFolderBrowserForm folderForm = new CustomFolderBrowserForm())
            {
                folderForm.Text = "مسیر فایل مقصد را انتخاب کنید";
                if (folderForm.ShowDialog() == DialogResult.OK)
                {
                    textBox2.Text = folderForm.SelectedPath;
                    fileModel.Destination = folderForm.SelectedPath;
                }
            }
        }

        private void frmSetting_Load(object sender, EventArgs e)
        {
            LoadFileModels(true);
        }

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

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DeletSourceFile();
        }

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

        private void toolStripDell_Click(object sender, EventArgs e)
        {
            DeletSourceFile();
        }

        private void numMaxThread_ValueChanged(object sender, EventArgs e)
        {
            settings.maxThreads = (int)numMaxThread.Value;
        }

        private void ChkDeepCheck_CheckedChanged(object sender, EventArgs e)
        {
            settings.CheckFileDeep=ChkDeepCheck.Checked;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
