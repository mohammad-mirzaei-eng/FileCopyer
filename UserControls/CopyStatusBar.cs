using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.UserInterface
{
    public partial class CopyStatusBar : UserControl
    {
        public CopyStatusBar()
        {
            InitializeComponent();
        }

        private int _progressbarValue;

        public int ProgressBarValue
        {
            get
            {
                return _progressbarValue;
            }
            set
            {
                _progressbarValue = value;
                if (progressBar1.InvokeRequired)
                {
                    progressBar1.BeginInvoke((MethodInvoker)(() =>
                        {
                            progressBar1.Value = value;
                        }));
                }
                else
                {
                    progressBar1.Value = value;
                }
            }
        }

        private int _progressbarMinValue;

        public int ProgressBarMinValue
        {
            get
            {
                return _progressbarMinValue;
            }
            set
            {
                _progressbarMinValue = value;
                progressBar1.Minimum = value;
            }
        }

        private int _progressbarMaxValue;

        public int ProgressBarMaxValue
        {
            get
            {
                return _progressbarMaxValue;
            }
            set
            {
                _progressbarMaxValue = value;
                progressBar1.Maximum = value;
            }
        }


        private string _textLable;

        public string LableText
        {
            get
            {
                return _textLable;
            }
            set
            {
                _textLable = value;
                label1.Text = value;
            }
        }


        private string _nameprogressbar;

        public string ProgressBarName
        {
            get
            {
                return _nameprogressbar;
            }
            set
            {
                _nameprogressbar = value;
                progressBar1.Name = value;
            }
        }

        private void CopyStatusBar_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
            if (this.Tag != null &&
               Directory.Exists(this.Tag.ToString()))
            {
                Process.Start(this.Tag.ToString());
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {
            if (this.Tag != null &&
                Directory.Exists(this.Tag.ToString()))
            {
                Process.Start(this.Tag.ToString());
            }
        }
    }
}
