using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudentConsultationApp
{
    public class ReportGenerator
    {
        private readonly string _reportsDirectory = "Reports";
        private readonly AuditLogger _logger;

        public ReportGenerator(AuditLogger logger)
        {
            _logger = logger;
            if (!Directory.Exists(_reportsDirectory))
            {
                Directory.CreateDirectory(_reportsDirectory);
            }
        }

        public void GenerateTopicSummary(List<ConsultationLog> records)
        {
            try
            {
                var activeRecords = records.Where(r => r.IsActive).ToList();
                var topicCounts = activeRecords
                    .GroupBy(r => r.Topic)
                    .Select(g => new { Topic = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count);

                string reportPath = Path.Combine(_reportsDirectory, $"TopicSummary_{DateTime.Now:yyyyMMddHHmmss}.txt");

                using (StreamWriter writer = new StreamWriter(reportPath))
                {
                    writer.WriteLine("--- STUDENT CONSULTATION TOPIC SUMMARY ---");
                    writer.WriteLine($"Generated At: {DateTime.Now}");
                    writer.WriteLine($"Total Active Consultations: {activeRecords.Count}");
                    writer.WriteLine("------------------------------------------");
                    foreach (var item in topicCounts)
                    {
                        writer.WriteLine($"- {item.Topic}: {item.Count} session(s)");
                    }
                }

                Console.WriteLine($"\nReport successfully generated at: {reportPath}");
                _logger.Log("Report", $"Generated Topic Summary Report: {reportPath}");
            }
            catch (Exception ex)
            {
                _logger.Log("Error", $"Report generation failed: {ex.Message}");
                Console.WriteLine("Failed to generate report.");
            }
        }
    }
}