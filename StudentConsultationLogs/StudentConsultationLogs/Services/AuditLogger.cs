using System;
using System.IO;

namespace StudentConsultationApp
{
    public class AuditLogger
    {
        private readonly string _logFilePath;

        public AuditLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Log(string action, string details)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action.ToUpper()}: {details}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical Error writing to audit log: {ex.Message}");
            }
        }
    }
}