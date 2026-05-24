using System;

namespace StudentConsultationApp
{
    public class ConsultationLog
    {
        public string RecordId { get; set; } = string.Empty;

        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public DateTime ConsultationDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string Checksum { get; set; } = string.Empty;
    }
}