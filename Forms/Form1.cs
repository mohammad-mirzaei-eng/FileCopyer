﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using FileCopyer.Classes;
using FileCopyer.Interface;
using FileCopyer.Models;

namespace FileCopyer.Forms
{
    public partial class Form1 : Form
    {
        private List<FileModel> files;
        private CancellationTokenSource cancellationTokenSource;

        public Form1()
        {
            InitializeComponent();
        }

        private void CopyFilesButton_Click(object sender, EventArgs e)
        {
            IFileCopyStrategy strategy = new DefaultCopyStrategy();
            cancellationTokenSource = new CancellationTokenSource();
            // Load all file models and create copy operations
            foreach (var fileModel in files)
            {
                // ایجاد و افزودن عملیات کپی به لیست
                IFileOperation copyOperation = new CopyFileOperation(fileModel.Source, fileModel.Destination, strategy,flowLayoutPanel1, cancellationTokenSource.Token);
                FileCopyManager.Instance.AddOperation(copyOperation);
            }

            // Get the list of operations from FileCopyManager and execute them
            var operations = FileCopyManager.Instance.GetOperations();
            foreach (var operation in operations)
            {
                FileCopyManager.Instance.ExecuteOperation(operation);
            }

            if (cancellationTokenSource != null)
            {
               // cancellationTokenSource.Cancel();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cancellationTokenSource = new CancellationTokenSource();
            files = new List<FileModel>();
            FileModel fileModel = new FileModel();
            fileModel.Source = @"\\192.168.110.22\Fileserver\IT\110.14\driver\acer 5750\download";
            fileModel.Destination = @"D:\tmp";
            files.Add(fileModel);
            var query=from o in files select o.GetResourceName;
            listBox1.DataSource=query.ToList();
        }
    }

}