namespace MedSecureSystem.Domain.Entities
{
    public class AuditLog
    {
        public long Id { get; set; }
        public string TableName { get; set; }
        public long RecordId { get; set; }
        public string Action { get; set; }
        public string NewValues { get; set; }
        public string OldValues { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
