using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileCopyer.Classes.Design_Patterns.Strategy;
using FileCopyer.Classes.Observer;
using FileCopyer.Classes.Design_Patterns.Helper;
using FileCopyer.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileCopyer.Interface.Design_Patterns.Observer;

namespace FileCopyer.Tests
{
    [TestClass]
    public class DefaultCopyStrategyTests
    {
        [TestMethod]
        public void DefaultCopyStrategy_Constructor_ShouldInitializeCorrectly()
        {
            // Arrange
            var settings = new SettingsModel();
            var notifier = new CopyProgressNotifier();

            // Act
            var strategy = new DefaultCopyStrategy(settings, notifier);

            // Assert
            Assert.IsNotNull(strategy);
        }

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

    [TestClass]
    public class CopyProgressNotifierTests
    {
        [TestMethod]
        public void AddObserver_ShouldAddObserverToList()
        {
            // Arrange
            var notifier = new CopyProgressNotifier();
            var observer = new MockProgressObserver();

            // Act
            notifier.AddObserver((Interface.Design_Patterns.Observer.IProgressObserver)observer);

            // Assert
            // Verify that the observer was added to the list
        }

        [TestMethod]
        public void RemoveObserver_ShouldRemoveObserverFromList()
        {
            // Arrange
            var notifier = new CopyProgressNotifier();
            var observer = new MockProgressObserver();
            notifier.AddObserver((Interface.Design_Patterns.Observer.IProgressObserver)observer);

            // Act
            notifier.RemoveObserver((Interface.Design_Patterns.Observer.IProgressObserver)observer);

            // Assert
            // Verify that the observer was removed from the list
        }

        [TestMethod]
        public void NotifyFileCopied_ShouldNotifyAllObservers()
        {
            // Arrange
            var notifier = new CopyProgressNotifier();
            var observer = new MockProgressObserver();
            notifier.AddObserver((Interface.Design_Patterns.Observer.IProgressObserver)observer);

            // Act
            notifier.NotifyFileCopied(1, 2, 0);

            // Assert
            // Verify that the observer was notified
        }

        [TestMethod]
        public void NotifyCopyCompleted_ShouldNotifyAllObservers()
        {
            // Arrange
            var notifier = new CopyProgressNotifier();
            var observer = new MockProgressObserver();
            notifier.AddObserver((Interface.Design_Patterns.Observer.IProgressObserver)observer);

            // Act
            notifier.NotifyCopyCompleted();

            // Assert
            // Verify that the observer was notified
        }
    }

    [TestClass]
    public class GenerateReportHelperTests
    {
        [TestMethod]
        public async Task GenerateReport_ShouldGenerateReportSuccessfully()
        {
            // Arrange
            var helper = new GenerateReportHelper();
            var errorList = new List<string> { "Error 1", "Error 2" };
            var reportPath = "C:\\Reports";

            // Act
            await helper.GenerateReport(reportPath, errorList);

            // Assert
            // Verify that the report was generated successfully
        }
    }

    public class MockProgressObserver : IProgressObserver
    {
        public void OnFileCopied(int copiedFiles, int totalFiles, int errorFiles)
        {
            // Mock implementation
        }

        public void OnCopyCompleted()
        {
            // Mock implementation
        }
    }
}
