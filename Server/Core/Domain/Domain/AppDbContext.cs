namespace Domain;

// -----------------------------------------------------------------------------
// DbContext
// -----------------------------------------------------------------------------
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Catálogos
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Moneda> Monedas => Set<Moneda>();

    // Identity
    public DbSet<UsuarioGlobal> UsuariosGlobales => Set<UsuarioGlobal>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UsuarioTenant> UsuariosTenant => Set<UsuarioTenant>();

    // RRHH
    public DbSet<Legajo> Legajos => Set<Legajo>();
    public DbSet<DatosPersonalesLegajo> DatosPersonalesLegajos => Set<DatosPersonalesLegajo>();

    public DbSet<CampoDinamicoDef> CamposDinamicosDef => Set<CampoDinamicoDef>();
    public DbSet<CampoDinamicoValor> CamposDinamicosValor => Set<CampoDinamicoValor>();

    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Puesto> Puestos => Set<Puesto>();
    public DbSet<LegajoPuesto> LegajoPuestos => Set<LegajoPuesto>();

    public DbSet<Convenio> Convenios => Set<Convenio>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<ContratoLaboral> ContratosLaborales => Set<ContratoLaboral>();

    public DbSet<RegistroHorario> RegistrosHorario => Set<RegistroHorario>();
    public DbSet<TipoAusencia> TiposAusencia => Set<TipoAusencia>();
    public DbSet<Ausencia> Ausencias => Set<Ausencia>();

    public DbSet<Liquidacion> Liquidaciones => Set<Liquidacion>();
    public DbSet<LiquidacionLegajo> LiquidacionesLegajo => Set<LiquidacionLegajo>();
    public DbSet<Concepto> Conceptos => Set<Concepto>();
    public DbSet<LiquidacionConcepto> LiquidacionesConcepto => Set<LiquidacionConcepto>();

    public DbSet<SolicitudMedica> SolicitudesMedicas => Set<SolicitudMedica>();
    public DbSet<ExamenMedico> ExamenesMedicos => Set<ExamenMedico>();

    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<FirmaDocumento> FirmasDocumento => Set<FirmaDocumento>();

    public DbSet<AuditoriaEvento> AuditoriaEventos => Set<AuditoriaEvento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // -----------------------------
        // Unique / indexes críticos
        // -----------------------------
        modelBuilder.Entity<UsuarioGlobal>()
            .HasIndex(x => new { x.PaisId, x.TipoIdentificacion, x.NumeroIdentificacion })
            .IsUnique();

        modelBuilder.Entity<Tenant>()
            .HasIndex(x => new { x.PaisId, x.IdentificacionFiscal })
            .IsUnique();

        modelBuilder.Entity<UsuarioTenant>()
            .HasIndex(x => new { x.TenantId, x.UsuarioGlobalId })
            .IsUnique();

        modelBuilder.Entity<Legajo>()
            .HasIndex(x => new { x.TenantId, x.NumeroLegajo })
            .IsUnique();

        // Campos dinámicos: nombre único por tenant
        modelBuilder.Entity<CampoDinamicoDef>()
            .HasIndex(x => new { x.TenantId, x.Nombre })
            .IsUnique();

        // Concepto: código único por tenant (y opcionalmente por país si aplica)
        modelBuilder.Entity<Concepto>()
            .HasIndex(x => new { x.TenantId, x.Codigo })
            .IsUnique();

        // Liquidación: periodo+tipo único por tenant (si lo querés así)
        modelBuilder.Entity<Liquidacion>()
            .HasIndex(x => new { x.TenantId, x.Periodo, x.Tipo })
            .IsUnique();

        // -----------------------------
        // Relaciones 1:1 Legajo <-> DatosPersonales (shared PK)
        // -----------------------------
        modelBuilder.Entity<Legajo>()
            .HasOne(x => x.DatosPersonales)
            .WithOne(x => x.Legajo)
            .HasForeignKey<DatosPersonalesLegajo>(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // -----------------------------
        // Area self reference
        // -----------------------------
        modelBuilder.Entity<Area>()
            .HasOne(x => x.AreaPadre)
            .WithMany(x => x.Hijos)
            .HasForeignKey(x => x.AreaPadreId)
            .OnDelete(DeleteBehavior.Restrict);

        // -----------------------------
        // Soft delete global filter (Documentos)
        // (Repetible para otras entidades si aplica)
        // -----------------------------
        modelBuilder.Entity<Documento>()
            .HasQueryFilter(x => !x.IsDeleted);

        // -----------------------------
        // DateOnly/TimeOnly mapping (EF Core 8+ ok nativo)
        // Si tu provider requiere, agregar conversiones aquí.
        // -----------------------------
    }
}
