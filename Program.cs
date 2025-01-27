using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using FileCopyer.Classes.Design_Patterns.Strategy;
using FileCopyer.Classes.Observer;
using FileCopyer.Forms;
using FileCopyer.Models;

namespace FileCopyer
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length >= 2)
            {
                string source = args[0];
                string destination = args[1];

                var settings = new SettingsModel
                {
                    MaxThreads = args.Contains("--maxThreads") ? int.Parse(args[Array.IndexOf(args, "--maxThreads") + 1]) : 2,
                    CreateParentPath = args.Contains("--createParentPath"),
                    ShowProgressBar = args.Contains("--showProgressBar"),
                    CheckFileDeep = args.Contains("--checkFileDeep"),
                    MaxBufferSize = args.Contains("--maxBufferSize") ? int.Parse(args[Array.IndexOf(args, "--maxBufferSize") + 1]) : 4
                };

                var notifier = new CopyProgressNotifier();
                var strategy = new DefaultCopyStrategy(settings, notifier);

                var fileModels = new List<FileModel>
                        {
                            new FileModel { Source = source, Destination = destination }
                        };

                var flowLayoutPanel = new FlowLayoutPanel();
                var cancellationToken = new CancellationToken();

                strategy.CopyFile(fileModels, flowLayoutPanel, cancellationToken).Wait();
            }
            else
            {
                ShowHelp();
            }
        }

        static void ShowHelp()
        {
            Console.WriteLine("Usage: FileCopyer.exe <source> <destination> [options]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --maxThreads <number>       Set the maximum number of threads (default: 2)");
            Console.WriteLine("  --createParentPath          Create parent path in the destination");
            Console.WriteLine("  --showProgressBar           Show progress bar during copy");
            Console.WriteLine("  --checkFileDeep             Perform a deep check of the files");
            Console.WriteLine("  --maxBufferSize <number>    Set the maximum buffer size in MB (default: 4)");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  FileCopyer.exe C:\\SourceFolder C:\\DestinationFolder --maxThreads 4 --showProgressBar");
        }
    }
}
