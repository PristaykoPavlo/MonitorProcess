using MonitorProcess.Core;
using NUnit.Framework;
using System.Diagnostics;

namespace MonitorProcess.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        #region Private Properties
        private const string ProcessName = "notepad";
        private const int MaxLifetimeMinutes = 1;
        private const int MonitoringFrequencyMinutes = 1;
        private const string LogFilePath = "process_kill_log.txt";
        private ProcessMonitor _processMonitor;
        private IProcessService _processService;
        private IFileService _fileService;
        private MockKeyboardService _keyboardService;
        #endregion

        #region Setup
        [SetUp]
        public void SetUp()
        {
            _processService = new ProcessService();
            _fileService = new FileService();
            _keyboardService = new MockKeyboardService();
        }
        #endregion

        #region Teardown
        [TearDown]
        public void TearDown()
        {
            CleanupProcesses();
            if (File.Exists(LogFilePath))
            {
                File.Delete(LogFilePath);
            }
        }
        #endregion

        #region Tests
        [Test]
        public async Task MonitorAndKillProcesses_KillsLongRunningNotepadProcess()
        {
            // Arrange
            string[] args = { ProcessName, MaxLifetimeMinutes.ToString(), MonitoringFrequencyMinutes.ToString() };
            Process process = Process.Start("notepad.exe");

            _processMonitor = new ProcessMonitor(args, _processService, _fileService, _keyboardService);

            // Act
            var monitoringTask = Task.Run(() => _processMonitor.Start());
            await Task.Delay(80000); // Run the monitor for 80 seconds
            _keyboardService.SimulateKeyPress(ConsoleKey.Q); // Stop the monitor

            // Assert
            Assert.That(IsProcessKilled(ProcessName), Is.True);
            Assert.That(File.ReadAllText(LogFilePath).Contains("Killed process"), Is.True);
        }

        [Test]
        public async Task MonitorAndKillProcesses_KillsMultipleProcesses()
        {
            // Arrange
            Process process1 = Process.Start("notepad.exe");
            Process process2 = Process.Start("notepad.exe");
            string[] args = { ProcessName, MaxLifetimeMinutes.ToString(), MonitoringFrequencyMinutes.ToString() };

            _processMonitor = new ProcessMonitor(args, _processService, _fileService, _keyboardService);

            // Act
            var monitoringTask = Task.Run(() => _processMonitor.Start());
            await Task.Delay(80000); // Run the monitor for 80 seconds
            _keyboardService.SimulateKeyPress(ConsoleKey.Q); // Stop the monitor

            await monitoringTask;

            // Assert
            Assert.That(IsProcessKilled(ProcessName), Is.True);
            Assert.That(File.ReadAllText(LogFilePath).Contains("Killed process"), Is.True);
        }

        [Test]
        public async Task MonitorAndKillProcesses_ProcessWithInvalidName_DoesNotKillProcess()
        {
            // Arrange
            string[] args = { "notepadeeeee", MaxLifetimeMinutes.ToString(), MonitoringFrequencyMinutes.ToString() };
            Process process = Process.Start($"{ProcessName}.exe");
            _keyboardService = new MockKeyboardService();

            _processMonitor = new ProcessMonitor(args, _processService, _fileService, _keyboardService);

            // Act
            var monitoringTask = Task.Run(() => _processMonitor.Start());
            await Task.Delay(80000); // Run the monitor for 80 seconds
            _keyboardService.SimulateKeyPress(ConsoleKey.Q); // Stop the monitor

            await monitoringTask;


            // Assert
            Assert.That(IsProcessKilled(ProcessName), Is.False);
        }
        #endregion

        #region Test Helpers

        private bool IsProcessKilled(string process)
        {
            IProcess[] processes = _processService.GetProcessesByName(process);
            return processes.Length == 0;

        }

        private void CleanupProcesses()
        {
            var processes = Process.GetProcessesByName(ProcessName);
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Cleanup Failed with message: {e.Message}");
                }
            }
        }

        public class MockKeyboardService : IKeyboardService
        {
            private volatile bool _keyPressed;

            public bool IsKeyAvailable() => _keyPressed;

            public ConsoleKeyInfo ReadKey() => new ConsoleKeyInfo('q', ConsoleKey.Q, false, false, false);

            public void SimulateKeyPress(ConsoleKey key)
            {
                _keyPressed = true;
            }
        }

        #endregion
    }
}
