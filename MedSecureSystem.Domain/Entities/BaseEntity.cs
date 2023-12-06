namespace MedSecureSystem.Domain.Entities;

public abstract class BaseEntity
{
    public long Id { get; set; }  // Assuming an integer ID for simplicity
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
}
