namespace FileCopyer.Forms
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnstart = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.listboxcontext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripDell = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifycontext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripstatus = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.showappcms = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitappcms = new System.Windows.Forms.ToolStripMenuItem();
            this.totalbar = new System.Windows.Forms.ProgressBar();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblcopeid = new System.Windows.Forms.Label();
            this.lbltotalcopied = new System.Windows.Forms.Label();
            this.lblstatus = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.listboxcontext.SuspendLayout();
            this.notifycontext.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnstart
            // 
            this.btnstart.Location = new System.Drawing.Point(410, 422);
            this.btnstart.Name = "btnstart";
            this.btnstart.Size = new System.Drawing.Size(75, 23);
            this.btnstart.TabIndex = 0;
            this.btnstart.Text = "شروع";
            this.btnstart.UseVisualStyleBackColor = true;
            this.btnstart.Click += new System.EventHandler(this.CopyFilesButton_Click);
            // 
            // listBox1
            // 
            this.listBox1.ContextMenuStrip = this.listboxcontext;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 25);
            this.listBox1.Name = "listBox1";
            this.listBox1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.listBox1.Size = new System.Drawing.Size(297, 420);
            this.listBox1.TabIndex = 1;
            // 
            // listboxcontext
            // 
            this.listboxcontext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDell});
            this.listboxcontext.Name = "listboxcontext";
            this.listboxcontext.Size = new System.Drawing.Size(167, 26);
            // 
            // toolStripDell
            // 
            this.toolStripDell.Name = "toolStripDell";
            this.toolStripDell.Size = new System.Drawing.Size(166, 22);
            this.toolStripDell.Text = "حذف مسیر انتخابی";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "مسیر فایلها";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(621, 422);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "تنظیمات";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.notifycontext;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            // 
            // notifycontext
            // 
            this.notifycontext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripstatus,
            this.toolStripSeparator2,
            this.showappcms,
            this.toolStripSeparator1,
            this.exitappcms});
            this.notifycontext.Name = "contextMenuStrip1";
            this.notifycontext.Size = new System.Drawing.Size(150, 82);
            // 
            // toolStripstatus
            // 
            this.toolStripstatus.Name = "toolStripstatus";
            this.toolStripstatus.Size = new System.Drawing.Size(149, 22);
            this.toolStripstatus.Text = "وضعیت : False";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(146, 6);
            // 
            // showappcms
            // 
            this.showappcms.Name = "showappcms";
            this.showappcms.Size = new System.Drawing.Size(149, 22);
            this.showappcms.Text = "نمایش برنامه";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(146, 6);
            // 
            // exitappcms
            // 
            this.exitappcms.Name = "exitappcms";
            this.exitappcms.Size = new System.Drawing.Size(149, 22);
            this.exitappcms.Text = "خروج";
            // 
            // totalbar
            // 
            this.totalbar.Location = new System.Drawing.Point(315, 103);
            this.totalbar.Name = "totalbar";
            this.totalbar.Size = new System.Drawing.Size(473, 23);
            this.totalbar.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(669, 54);
            this.label3.Name = "label3";
            this.label3.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label3.Size = new System.Drawing.Size(107, 23);
            this.label3.TabIndex = 7;
            this.label3.Text = "مقدار فایل کپی شده:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(676, 77);
            this.label4.Name = "label4";
            this.label4.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 8;
            this.label4.Text = "تعداد کل فایلها:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblcopeid
            // 
            this.lblcopeid.AutoEllipsis = true;
            this.lblcopeid.Location = new System.Drawing.Point(315, 54);
            this.lblcopeid.Name = "lblcopeid";
            this.lblcopeid.Size = new System.Drawing.Size(348, 23);
            this.lblcopeid.TabIndex = 9;
            this.lblcopeid.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbltotalcopied
            // 
            this.lbltotalcopied.AutoEllipsis = true;
            this.lbltotalcopied.Location = new System.Drawing.Point(315, 77);
            this.lbltotalcopied.Name = "lbltotalcopied";
            this.lbltotalcopied.Size = new System.Drawing.Size(355, 23);
            this.lbltotalcopied.TabIndex = 10;
            this.lbltotalcopied.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbltotalcopied.Click += new System.EventHandler(this.lbltotalcopied_Click);
            // 
            // lblstatus
            // 
            this.lblstatus.AutoEllipsis = true;
            this.lblstatus.Location = new System.Drawing.Point(315, 25);
            this.lblstatus.Name = "lblstatus";
            this.lblstatus.Size = new System.Drawing.Size(473, 29);
            this.lblstatus.TabIndex = 11;
            this.lblstatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(318, 132);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(470, 284);
            this.flowLayoutPanel1.TabIndex = 13;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.lblstatus);
            this.Controls.Add(this.lbltotalcopied);
            this.Controls.Add(this.lblcopeid);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.totalbar);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.btnstart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.listboxcontext.ResumeLayout(false);
            this.notifycontext.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnstart;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip notifycontext;
        private System.Windows.Forms.ToolStripMenuItem showappcms;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitappcms;
        private System.Windows.Forms.ProgressBar totalbar;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblcopeid;
        private System.Windows.Forms.Label lbltotalcopied;
        private System.Windows.Forms.Label lblstatus;
        private System.Windows.Forms.ContextMenuStrip listboxcontext;
        private System.Windows.Forms.ToolStripMenuItem toolStripDell;
        private System.Windows.Forms.ToolStripMenuItem toolStripstatus;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
    }
}

