using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using MonitorProcess.Core;

class Program
{
    static void Main(string[] args)
    {
        ProcessService processService = new ProcessService();   
        FileService fileService = new FileService();
        KeyboardService keyboardService = new KeyboardService();

        ProcessMonitor monitor = new ProcessMonitor(args, processService, fileService, keyboardService);
        monitor.Start();
    }
}
