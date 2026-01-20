namespace Domain.Common.Contracts;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAtUtc { get; set; }
    Guid? DeletedByUserGlobalId { get; set; }
}


 