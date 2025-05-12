using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LoL_eSport_Team_Mangager
{
    public static class Logger
    {
        private static readonly string logFilePath = "application_log.txt";

        public static void Log(string message, string level = "INFO", string source = "General") 
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] [{source}] {message}";
                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch 
            {
                
            }
        }
    }
}
