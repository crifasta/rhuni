namespace Domain.Common.Contracts;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    Guid? CreatedByUserId { get; set; }

    DateTime? UpdatedAtUtc { get; set; }
    Guid? UpdatedByUserlId { get; set; }
}


 