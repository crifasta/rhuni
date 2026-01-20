using System.Security.Claims;
namespace Application;

public static class TenantConstants
{
    // Headers/Claims recomendados (ajustá según tu auth)
    public const string TenantHeader = "X-Tenant-Id";
    public const string TenantClaimType = "tenant_id"; // o "tid"
    public const string SuperAdminClaimType = "is_superadmin"; // "true"
}

public sealed class TenantNotResolvedException : Exception
{
    public TenantNotResolvedException(string message) : base(message) { }
}

public sealed class CrossTenantAccessDeniedException : Exception
{
    public CrossTenantAccessDeniedException(string message) : base(message) { }
}

public interface ITenantProvider
{
    Guid? TenantId { get; }
    bool IsSuperAdmin { get; }

    /// <summary>
    /// Si IsSuperAdmin = true, permite operar sin filtro o con tenant explícito.
    /// </summary>
    Guid GetRequiredTenantId();
}
