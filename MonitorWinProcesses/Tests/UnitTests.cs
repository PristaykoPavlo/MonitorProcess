using NUnit.Framework;
using Moq;
using static MonitorProcess.Tests.IntegrationTests;
using MonitorProcess.Core;

namespace MonitorProcess.Tests
{
    [TestFixture]
    public class UnitTests
    {
        #region Private Properties
        private Mock<IProcessService> _mockProcessService;
        private Mock<IFileService> _mockFileService;
        private ProcessMonitor _processMonitor;
        private MockKeyboardService _keyboardService;
        #endregion

        #region Setup
        [SetUp]
        public void SetUp()
        {
            _mockProcessService = new Mock<IProcessService>();
            _mockFileService = new Mock<IFileService>();
            _keyboardService = new MockKeyboardService();
        }
        #endregion

        #region Tests
        [Test]
        public async Task MonitorAndKillProcesses_ValidProcess_KillsProcess()
        {
            // Arrange
            string[] args = new[] { "testProcess", "5", "1" };

            IProcess process = CreateMockProcess("testProcess", DateTime.Now.AddMinutes(-6));
            _mockProcessService.Setup(p => p.GetProcessesByName("testProcess")).Returns(new[] { process });

            _processMonitor = new ProcessMonitor(args, _mockProcessService.Object, _mockFileService.Object, _keyboardService);

            // Act
            var monitoringTask = Task.Run(() => _processMonitor.Start());
            await Task.Delay(1000);
            _keyboardService.SimulateKeyPress(ConsoleKey.Q); // Stop the monitor

            await monitoringTask;


            // Assert
            _mockProcessService.Verify(p => p.KillProcess(It.IsAny<IProcess>()), Times.Once, $"Failed to execute KillProcess()");
        }

        [Test]
        public async Task MonitorAndKillProcesses_ProcessWithInvalidLifetime_DoesNotKillProcess()
        {
            // Arrange
            string[] args = new string[] { "testProcess", "7", "1" };

            IProcess process = CreateMockProcess("testProcess", DateTime.Now.AddMinutes(-6));
            _mockProcessService.Setup(p => p.GetProcessesByName("testProcess")).Returns(new[] { process });

            _processMonitor = new ProcessMonitor(args, _mockProcessService.Object, _mockFileService.Object, _keyboardService);

            // Act
            var monitoringTask = Task.Run(() => _processMonitor.Start());
            await Task.Delay(1000);
            _keyboardService.SimulateKeyPress(ConsoleKey.Q); // Stop the monitor

            await monitoringTask;


            // Assert
            _mockProcessService.Verify(p => p.KillProcess(It.IsAny<IProcess>()), Times.Never);
            _mockFileService.Verify(f => f.AppendAllText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Constructor_InvalidArgumentsType_ThrowsArgumentException()
        {
            // Arrange
            string[] args = new string[] { "notepad", "invalid", "5" };

            // Act & Assert
            Exception exception = Assert.Throws<ArgumentException>(() => new ProcessMonitor(args, _mockProcessService.Object, _mockFileService.Object, _keyboardService));
            Assert.That("Please pass arguments in a format: MonitorProcess.exe <string> <int> <int>", Is.EqualTo(exception.Message));
        }

        [Test]
        public void Constructor_InvalidNumberOfArguments_ThrowsArgumentException()
        {
            // Arrange
            string[] args = new string[] { "notepad", "1", "1", "5"};

            // Act & Assert
            Exception exception = Assert.Throws<ArgumentException>(() => new ProcessMonitor(args, _mockProcessService.Object, _mockFileService.Object, _keyboardService));
            Assert.That("Please pass correct number of arguments: MonitorProcess.exe <process_name> <max_lifetime_minutes> <frequency_minutes>", Is.EqualTo(exception.Message));
        }
        #endregion

        #region Test Helpers
        private IProcess CreateMockProcess(string processName, DateTime startTime)
        {
            var mockProcess = new Mock<IProcess>();
            mockProcess.Setup(p => p.ProcessName).Returns(processName);
            mockProcess.Setup(p => p.StartTime).Returns(startTime);
            return mockProcess.Object;
        }
        #endregion
    }
}
