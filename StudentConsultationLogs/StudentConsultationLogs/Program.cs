using System;
using System.Linq;

namespace StudentConsultationApp
{
    class Program
    {
        static void Main(string[] args)
        {
            AuditLogger logger = new AuditLogger("AuditLog.txt");
            logger.Log("System", "Application Started");

            FileRepository repo = new FileRepository("consultations.json", logger);
            Validator validator = new Validator();
            ReportGenerator reportGen = new ReportGenerator(logger);

            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("=== STUDENT CONSULTATION LOGS ===");
                Console.WriteLine("1. Add Consultation Record");
                Console.WriteLine("2. View Active Records");
                Console.WriteLine("3. Update Record");
                Console.WriteLine("4. Delete Record (Soft Delete)");
                Console.WriteLine("5. Hard Delete Record");
                Console.WriteLine("6. Generate Topic Report");
                Console.WriteLine("7. Exit");
                Console.Write("Choose an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": AddRecord(repo, validator, logger); break;
                    case "2": ViewRecords(repo, validator, logger); break;
                    case "3": UpdateRecord(repo, validator, logger); break;
                    case "4": SoftDeleteRecord(repo, logger); break;
                    case "5": HardDeleteRecord(repo, logger); break;
                    case "6": reportGen.GenerateTopicSummary(repo.LoadRecords()); Pause(); break;
                    case "7":
                        running = false;
                        logger.Log("System", "Application Exited");
                        break;
                    default:
                        Console.WriteLine("Invalid option. Try again.");
                        Pause();
                        break;
                }
            }
        }

        static void AddRecord(FileRepository repo, Validator validator, AuditLogger logger)
        {
            Console.Clear();
            Console.WriteLine("--- ADD CONSULTATION ---");

            var log = new ConsultationLog
            {
                RecordId = Guid.NewGuid().ToString(),
                StudentId = validator.PromptAndValidateString("Student ID: "),
                StudentName = validator.PromptAndValidateString("Student Name: "),
                Topic = validator.PromptAndValidateString("Topic: "),
                ConsultationDate = validator.PromptAndValidateDate("Consultation Date (YYYY-MM-DD): "),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsActive = true
            };

            log.Checksum = validator.GenerateChecksum(log);

            var records = repo.LoadRecords();
            records.Add(log);
            repo.SaveRecords(records);

            logger.Log("Add", $"Added record {log.RecordId} for Student {log.StudentId}");
            Console.WriteLine("\nRecord added successfully!");
            Pause();
        }

        static void ViewRecords(FileRepository repo, Validator validator, AuditLogger logger)
        {
            Console.Clear();
            Console.WriteLine("--- VIEW CONSULTATIONS ---");

            var records = repo.LoadRecords();
            var activeRecords = records.Where(r => r.IsActive).ToList();

            if (!activeRecords.Any())
            {
                Console.WriteLine("No active records found.");
                Pause();
                return;
            }

            Console.Write("Filter by Student ID? (Leave blank for all): ");
            string filter = Console.ReadLine()?.Trim() ?? "";

            if (!string.IsNullOrEmpty(filter))
            {
                activeRecords = activeRecords.Where(r => r.StudentId.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            logger.Log("Read", $"Viewed records. Filter: {(string.IsNullOrEmpty(filter) ? "None" : filter)}");

            Console.WriteLine($"\n{"ID",-38} | {"Student ID",-10} | {"Name",-20} | {"Topic",-20} | {"Date",-10} | Valid");
            Console.WriteLine(new string('-', 115));

            foreach (var r in activeRecords)
            {
                bool isValid = validator.ValidateRecordIntegrity(r);
                string validStr = isValid ? "Yes" : "NO (Corrupted)";
                Console.WriteLine($"{r.RecordId,-38} | {r.StudentId,-10} | {r.StudentName,-20} | {r.Topic,-20} | {r.ConsultationDate:yyyy-MM-dd} | {validStr}");
            }

            Pause();
        }

        static void UpdateRecord(FileRepository repo, Validator validator, AuditLogger logger)
        {
            Console.Clear();
            Console.WriteLine("--- UPDATE CONSULTATION ---");

            var records = repo.LoadRecords();
            Console.Write("Enter Record ID to update: ");
            string targetId = Console.ReadLine()?.Trim() ?? "";

            var record = records.FirstOrDefault(r => r.RecordId == targetId && r.IsActive);

            if (record == null)
            {
                Console.WriteLine("Active record not found.");
                Pause();
                return;
            }

            Console.WriteLine("Enter new values (leave blank to keep current):");

            Console.Write($"Student ID [{record.StudentId}]: ");
            string sid = Console.ReadLine()?.Trim() ?? "";
            if (!string.IsNullOrEmpty(sid)) record.StudentId = sid;

            Console.Write($"Student Name [{record.StudentName}]: ");
            string sname = Console.ReadLine()?.Trim() ?? "";
            if (!string.IsNullOrEmpty(sname)) record.StudentName = sname;

            Console.Write($"Topic [{record.Topic}]: ");
            string topic = Console.ReadLine()?.Trim() ?? "";
            if (!string.IsNullOrEmpty(topic)) record.Topic = topic;

            record.UpdatedAt = DateTime.Now;
            record.Checksum = validator.GenerateChecksum(record); 

            repo.SaveRecords(records);
            logger.Log("Update", $"Updated record {record.RecordId}");

            Console.WriteLine("\nRecord updated successfully!");
            Pause();
        }

        static void SoftDeleteRecord(FileRepository repo, AuditLogger logger)
        {
            Console.Clear();
            Console.WriteLine("--- DELETE CONSULTATION (SOFT) ---");

            var records = repo.LoadRecords();
            Console.Write("Enter Record ID to delete: ");
            string targetId = Console.ReadLine()?.Trim() ?? "";

            var record = records.FirstOrDefault(r => r.RecordId == targetId && r.IsActive);
            if (record == null)
            {
                Console.WriteLine("Active record not found.");
                Pause();
                return;
            }

            record.IsActive = false;
            record.UpdatedAt = DateTime.Now;
            repo.SaveRecords(records);

            logger.Log("Delete (Soft)", $"Marked record {record.RecordId} as inactive");
            Console.WriteLine("\nRecord deleted (soft) successfully!");
            Pause();
        }

        static void HardDeleteRecord(FileRepository repo, AuditLogger logger)
        {
            Console.Clear();
            Console.WriteLine("--- HARD DELETE CONSULTATION ---");
            Console.WriteLine("WARNING: This will permanently remove the record from the file.");

            var records = repo.LoadRecords();
            Console.Write("Enter Record ID to permanently delete: ");
            string targetId = Console.ReadLine()?.Trim() ?? "";

            var record = records.FirstOrDefault(r => r.RecordId == targetId);
            if (record == null)
            {
                Console.WriteLine("Record not found.");
                Pause();
                return;
            }

            records.Remove(record);
            repo.SaveRecords(records);

            logger.Log("Delete (Hard)", $"Permanently deleted record {targetId}");
            Console.WriteLine("\nRecord permanently deleted!");
            Pause();
        }

        static void Pause()
        {
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
        }
    }
}