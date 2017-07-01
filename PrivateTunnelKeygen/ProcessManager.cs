using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static PrivateTunnelKeygen.ConsoleHelper;

namespace PrivateTunnelKeygen
{
    static class ProcessManager
    {
        private static IEnumerable<Process> GetProcessByPath(string fileName)
        {
            return Process.GetProcesses().Where(i =>
            {
                try
                {
                    return i.MainModule.FileName == fileName;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        public static void RestartClient()
        {
            string pePath = FileManager.ExecutablePath;

            WriteLine("Killing running instances...");
            foreach (Process process in GetProcessByPath(pePath))
                process.Kill();

            WriteLine("Starting new instance...");
            Process.Start(new ProcessStartInfo(pePath)
            {
                WorkingDirectory = Directory.GetDirectoryRoot(pePath),
                UseShellExecute = false,
                CreateNoWindow = true
            });
        }
    }
}
