using System;
using System.Security.Cryptography;
using System.Text;

namespace StudentConsultationApp
{
    public class Validator
    {
        public string GenerateChecksum(ConsultationLog log)
        {
            string rawData = $"{log.RecordId}|{log.StudentId}|{log.StudentName}|{log.Topic}|{log.ConsultationDate:yyyyMMddHHmm}";
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToBase64String(bytes);
        }

        public bool ValidateRecordIntegrity(ConsultationLog log)
        {
            string expectedChecksum = GenerateChecksum(log);
            return log.Checksum == expectedChecksum;
        }

        public string PromptAndValidateString(string prompt, bool required = true)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Trim();

                if (required && string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Error: This field cannot be empty.");
                    continue;
                }
                return input ?? string.Empty;
            }
        }

        public DateTime PromptAndValidateDate(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string? input = Console.ReadLine()?.Trim();
                if (DateTime.TryParse(input, out DateTime date))
                {
                    return date;
                }
                Console.WriteLine("Error: Invalid date format. Please use YYYY-MM-DD.");
            }
        }
    }
}