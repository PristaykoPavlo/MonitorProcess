using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorProcess.Core
{
    public interface IProcessService
    {
        IProcess[] GetProcessesByName(string processName);
        void KillProcess(IProcess process);
    }

    public interface IFileService
    {
        void AppendAllText(string path, string contents);
    }
    public interface IProcess
    {
        string ProcessName { get; }
        DateTime StartTime { get; }
        void Kill();
    }

    public class ProcessWrapper : IProcess
    {
        private readonly Process _process;

        public ProcessWrapper(Process process)
        {
            _process = process;
        }

        public string ProcessName => _process.ProcessName;
        public DateTime StartTime => _process.StartTime;
        public void Kill() => _process.Kill();
    }

    public interface IKeyboardService
    {
        bool IsKeyAvailable();
        ConsoleKeyInfo ReadKey();
    }

    public class KeyboardService : IKeyboardService
    {
        public bool IsKeyAvailable() => Console.KeyAvailable;
        public ConsoleKeyInfo ReadKey() => Console.ReadKey(true);
    }

    public class ProcessService : IProcessService
    {
        public IProcess[] GetProcessesByName(string processName)
        {
            return Process.GetProcessesByName(processName).Select(p => new ProcessWrapper(p)).ToArray();
        }

        public void KillProcess(IProcess process)
        {
            process.Kill();
        }
    }

    public class FileService : IFileService
    {
        public void AppendAllText(string path, string contents)
        {
            File.AppendAllText(path, contents);
        }
    }
}
