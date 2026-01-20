namespace Domain.Common.Contracts;

public abstract class TenantEntityBase<TKey> : BaseEntity<TKey>, ITenantEntity
{
    public Guid TenantId { get; set; }
}


 