using MyLoggerNamespace;
using System;
using System.Diagnostics;

namespace Utils
{
    public class MyDiagnostics
    {

        public MyDiagnostics() 
        { 

        }

        public static void ShutdownServer()
        {
            Process.Start("shutdown", "/s /f /t 0");
        }

        public static void RestartServer()
        {
            Process.Start("shutdown", "/r /f /t 0");
        }

        public static void ShutdownGame(IMyLogger logger)
        {
            // Список имен процессов игр
            string[] gameProcessNames = { "witcher3", "cascadeur" };

            Process[] processes = Process.GetProcesses();

            // Сначала сортируем процессы по использованию памяти
            var sortedProcesses = processes.OrderByDescending(process => process.WorkingSet64);

            foreach (Process process in processes)
            {
                //logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, $"Программа: [{process.ProcessName}] и tolowwer [{process.ProcessName?.ToLower()}]");
                if (Array.Exists(gameProcessNames, name => name.Equals(process.ProcessName, StringComparison.OrdinalIgnoreCase)))
                {
                    logger.WriteLine(MyLoggerNamespace.Enums.MessageType.Info, $"Закрытие игры: {process.ProcessName}");
                    process.Kill();
                }
            }
        }
    }
}