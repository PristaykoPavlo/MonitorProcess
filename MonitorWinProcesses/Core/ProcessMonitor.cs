using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorProcess.Core
{
    public class ProcessMonitor
    {
        #region Properties

        public readonly string _processName;
        private readonly int _maxLifetimeMinutes;
        private readonly int _monitoringFrequencyMinutes;
        private readonly string _logFilePath = "process_kill_log.txt";
        private readonly IProcessService _processService;
        private readonly IFileService _fileService;
        private readonly IKeyboardService _keyboardService;

        #endregion

        #region Contructor

        public ProcessMonitor(string[] args, IProcessService processService, IFileService fileService, IKeyboardService keyboardService)
        {
            PrintUsage();


            if (args.Length != 3)
            {
                throw new ArgumentException("Please pass correct number of arguments: MonitorProcess.exe <process_name> <max_lifetime_minutes> <frequency_minutes>");
            }


            if (!int.TryParse(args[1], out int maxLifetimeMinutes) || maxLifetimeMinutes <= 0 ||
                    !int.TryParse(args[2], out int monitoringFrequencyMinutes) || monitoringFrequencyMinutes <= 0)
            {
                throw new ArgumentException("Please pass arguments in a format: MonitorProcess.exe <string> <int> <int>");
            }
            else
            {
                _processName = args[0];
                _maxLifetimeMinutes = maxLifetimeMinutes;
                _monitoringFrequencyMinutes = monitoringFrequencyMinutes;
                _fileService = fileService;
                _processService = processService;
                _keyboardService = keyboardService;
                PrintArgs();
            }
        }

        #endregion

        #region Public Methods
        public void Start()
        {
            while (true)
            {
                if (_keyboardService.IsKeyAvailable() && _keyboardService.ReadKey().Key == ConsoleKey.Q)
                {
                    Console.WriteLine("Exiting...");
                    break;
                }

                MonitorAndKillProcesses();

                for (int i = 0; i < _monitoringFrequencyMinutes * 60; i++)
                {
                    if (_keyboardService.IsKeyAvailable() && _keyboardService.ReadKey().Key == ConsoleKey.Q)
                    {
                        Console.WriteLine("Exiting...");
                        return;
                    }
                    Thread.Sleep(1000);
                }
            }
        }
        #endregion

        #region Private Methods
        private void PrintUsage()
        {
            Console.WriteLine(" | Usage: MonitorProcess.exe <process_name> <max_lifetime_minutes> <frequency_minutes>");
            Console.WriteLine(" | Example: MonitorProcess.exe notepad 5 1");
            Console.WriteLine(" ------------------------------------------ ");
        }

        private void PrintArgs()
        {
            Console.WriteLine($" | Process Name:      [{_processName.PadRight(15)}] |");
            Console.WriteLine($" | Max Lifetime(min): [{_maxLifetimeMinutes.ToString().PadRight(15)}] |");
            Console.WriteLine($" | Frequency(min):    [{_monitoringFrequencyMinutes.ToString().PadRight(15)}] |");
        }

        private void MonitorAndKillProcesses()
        {
            var processes = _processService.GetProcessesByName(_processName);
            var currentTime = DateTime.Now;

            foreach (var process in processes)
            {
                var runtime = currentTime - process.StartTime;
                if (runtime.TotalMinutes > _maxLifetimeMinutes)
                {
                    KillProcess(process, runtime);
                }
                else
                {
                    Console.WriteLine($"Runtime of process '{process.ProcessName}' is smaller than expected: {runtime.Seconds} secods < {_maxLifetimeMinutes} minutes ");
                }
            }
        }

        private void KillProcess(IProcess process, TimeSpan runtime)
        {
            try
            {
                _processService.KillProcess(process);
                LogProcessKill(process, runtime);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to kill process : {ex.Message}");
            }
        }

        private void LogProcessKill(IProcess process, TimeSpan runtime)
        {
            var logMessage = $"{DateTime.Now}: Killed process '{process.ProcessName}' after running for {runtime.TotalMinutes:F2} minutes.";
            Console.WriteLine(logMessage);

            try
            {
                _fileService.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }
        #endregion
    }
}
