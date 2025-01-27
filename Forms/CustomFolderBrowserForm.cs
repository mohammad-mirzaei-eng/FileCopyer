using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Forms
{
    public partial class CustomFolderBrowserForm : Form
    {
        // مسیر انتخاب شده توسط کاربر
        public string SelectedPath { get; set; }

        // سازنده فرم CustomFolderBrowserForm
        /// <summary>
        /// Initializes a new instance of the CustomFolderBrowserForm class
        /// </summary>
        public CustomFolderBrowserForm()
        {
            InitializeComponent();
        }

        // رویداد کلیک دکمه Browse
        /// <summary>
        /// Handles the Click event of the Browse button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath = textBoxPath.Text;
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        // رویداد کلیک دکمه OK
        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            SelectedPath = textBoxPath.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // رویداد کلیک دکمه Cancel
        /// <summary>
        /// Handles the Click event of the Cancel button
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">Event data</param>
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
