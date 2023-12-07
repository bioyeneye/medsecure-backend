using System.Text.Json.Serialization;
using MedSecureSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace MedSecureSystem.Domain.Extensions;

public static class EntityExtensions
{
    public static AuditLog GetAuditTrail(this BaseEntity entity, EntityState state)
    {
        var auditLog = new AuditLog
        {
            TableName = entity.GetType().Name,
            RecordId = entity.Id,
        };


        switch (state)
        {
            case EntityState.Deleted:
                auditLog.Action = nameof(EntityState.Deleted);
                auditLog.OldValues = SerializeObject(entity);
                break;
            case EntityState.Modified:
                auditLog.Action = nameof(EntityState.Modified);
                auditLog.OldValues = SerializeObject(entity);
                break;
            case EntityState.Added:
                auditLog.Action = nameof(EntityState.Added);
                break;
            default:
                break;
        }

        return auditLog;
    }

    private static string SerializeObject(object obj)
    {
        try
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            });
        }
        catch (JsonSerializationException)
        {
            // Handle serialization exceptions as needed
            return "Error during serialization";
        }
    }

    // 
}
