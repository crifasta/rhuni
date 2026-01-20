using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Common.Contracts;

public abstract class AuditableEntity<T> : BaseEntity<T>, IAuditableEntity
{
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
    public Guid? UpdatedByUserlId { get; set; }
    protected AuditableEntity()
    {
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;

        //CreatedAtUtc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
        //LastModifiedOn = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

    }
}
 
public abstract class AuditableEntity : AuditableEntity<Guid>;