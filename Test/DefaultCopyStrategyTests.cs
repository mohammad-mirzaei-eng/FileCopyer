using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileCopyer.Classes.Design_Patterns.Strategy;
using FileCopyer.Models;
using FileCopyer.Classes.Observer;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileCopyer.Tests
{
    [TestClass]
    public class DefaultCopyStrategyTests
    {
        [TestMethod]
        public async Task CopyFile_ShouldCopyFilesSuccessfully()
        {
            // Arrange
            var settings = new SettingsModel
            {
                MaxThreads = 2,
                CreateParentPath = true,
                ShowProgressBar = false
            };
            var notifier = new CopyProgressNotifier();
            var strategy = new DefaultCopyStrategy(settings, notifier);

            var fileModels = new List<FileModel>
            {
                new FileModel { Source = "C:\\SourceFolder", Destination = "C:\\DestinationFolder" }
            };

            var flowLayoutPanel = new FlowLayoutPanel();
            var cancellationToken = new CancellationToken();

            // Act
            await strategy.CopyFile(fileModels, flowLayoutPanel, cancellationToken);

            // Assert
            // Add assertions to verify the expected behavior
            // For example, check if the files were copied to the destination folder
        }
    }
}
