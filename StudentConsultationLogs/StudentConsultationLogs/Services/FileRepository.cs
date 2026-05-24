using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace StudentConsultationApp
{
    public class FileRepository
    {
        private readonly string _dataDirectory = "Data";
        private readonly string _filePath;
        private readonly AuditLogger _logger;

        public FileRepository(string fileName, AuditLogger logger)
        {
            _filePath = Path.Combine(_dataDirectory, fileName);
            _logger = logger;
            InitializeStorage();
        }

        private void InitializeStorage()
        {
            try
            {
                if (!Directory.Exists(_dataDirectory))
                {
                    Directory.CreateDirectory(_dataDirectory);
                    _logger.Log("Initialize", $"Created directory: {_dataDirectory}");
                }

                if (!File.Exists(_filePath))
                {
                    File.WriteAllText(_filePath, "[]");
                    _logger.Log("Initialize", $"Created empty data file: {_filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.Log("Error", $"Storage initialization failed: {ex.Message}");
                Console.WriteLine("Error initializing storage. Check logs.");
            }
        }

        public List<ConsultationLog> LoadRecords()
        {
            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<ConsultationLog>>(json) ?? new List<ConsultationLog>();
            }
            catch (JsonException ex)
            {
                _logger.Log("Error", $"Data corruption detected (JSON parsing): {ex.Message}");
                Console.WriteLine("Warning: Data file might be corrupted.");
                return new List<ConsultationLog>();
            }
            catch (Exception ex)
            {
                _logger.Log("Error", $"Failed to load records: {ex.Message}");
                return new List<ConsultationLog>();
            }
        }

        public void SaveRecords(List<ConsultationLog> records)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(records, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                _logger.Log("Error", $"Failed to save records: {ex.Message}");
                Console.WriteLine("Error saving data. Check logs.");
            }
        }
    }
}