using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer
{
    public partial class CustomFolderBrowserForm : Form
    {
        public string SelectedPath { get; private set; }

        public CustomFolderBrowserForm()
        {
            InitializeComponent();
        }       

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.SelectedPath=textBoxPath.Text;
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxPath.Text = folderDialog.SelectedPath;
                }
            }
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            SelectedPath = textBoxPath.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
