namespace Domain;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

using System.Collections.Generic;
using Domain.Common.Contracts;

// -----------------------------------------------------------------------------
// RRHH Universal - Modelo EF Core (Entidades + DbContext)
// Target: .NET 8/9/10 + EF Core 8/9
// Notes:
// - Multi-tenant: todas las entidades operativas implementan ITenantEntity
// - Auditable: Created/Updated + soft delete opcional
// - Unique constraints críticos definidos en Fluent API
// -----------------------------------------------------------------------------




// -----------------------------------------------------------------------------
// Enums (pueden migrarse a tablas )
// -----------------------------------------------------------------------------


public enum RolUsuarioTenant
{
    Admin = 1,
    RRHH = 2,
    Empleado = 3,
    Supervisor = 4,
    Auditor = 5
}

public enum EstadoLaboral
{
    Activo = 1,
    Suspendido = 2,
    Licencia = 3,
    Baja = 4
}

public enum TipoDatoCampoDinamico
{
    Texto = 1,
    Numero = 2,
    Fecha = 3,
    Booleano = 4,
    Lista = 5,
    Json = 6
}

public enum TipoLiquidacion
{
    Mensual = 1,
    Final = 2,
    SAC = 3
}

public enum EstadoLiquidacion
{
    Borrador = 1,
    Cerrada = 2,
    Exportada = 3,
    Anulada = 4
}

public enum TipoConcepto
{
    Haberes = 1,
    Descuentos = 2,
    Aportes = 3
}

public enum OrigenRegistroHorario
{
    Web = 1,
    App = 2,
    Biometrico = 3
}

public enum EstadoSolicitud
{
    Pendiente = 1,
    Aprobada = 2,
    Rechazada = 3,
    Cerrada = 4
}

public enum MetodoFirma
{
    AceptacionSimple = 1,
    OTP = 2,
    FirmaDigital = 3,
    ProveedorExterno = 4
}

// -----------------------------------------------------------------------------
// Catálogos base multi-país (mínimos; podés expandir)
// -----------------------------------------------------------------------------
public sealed class Pais : BaseEntity<short>
{
    [Required, StringLength(2)]
    public string Iso2 { get; set; } = null!;

    [Required, StringLength(100)]
    public string Nombre { get; set; } = null!;
}

public sealed class Moneda : BaseEntity<short>
{
    [Required, StringLength(3)]
    public string Iso3 { get; set; } = null!;

    [Required, StringLength(100)]
    public string Nombre { get; set; } = null!;
}

// -----------------------------------------------------------------------------
// Identity Core
// -----------------------------------------------------------------------------
public sealed class UsuarioGlobal : BaseEntity<Guid>
{
    // Identidad global por país + tipo + número
    [Required, StringLength(30)]
    public string TipoIdentificacion { get; set; } = null!; // CUIT, DNI, SSN, etc.

    [Required, StringLength(50)]
    public string NumeroIdentificacion { get; set; } = null!;

    public short PaisId { get; set; }
    public Pais? Pais { get; set; }

    [EmailAddress, StringLength(254)]
    public string? EmailPrincipal { get; set; }

    [StringLength(30)]
    public string? Telefono { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    // Navegación
    public List<UsuarioTenant> Tenants { get; set; } = new();
    public List<Legajo> Legajos { get; set; } = new();
}

public sealed class Tenant : BaseEntity<Guid>
{
    [Required, StringLength(200)]
    public string RazonSocial { get; set; } = null!;

    [Required, StringLength(50)]
    public string IdentificacionFiscal { get; set; } = null!; // CUIT o equivalente

    public short PaisId { get; set; }
    public Pais? Pais { get; set; }

    public short MonedaBaseId { get; set; }
    public Moneda? MonedaBase { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;

    // Navegación
    public List<UsuarioTenant> Usuarios { get; set; } = new();
    public List<Legajo> Legajos { get; set; } = new();
}

public sealed class UsuarioTenant : TenantEntityBase<Guid>
{
    public Guid UsuarioGlobalId { get; set; }
    public UsuarioGlobal? UsuarioGlobal { get; set; }

    public RolUsuarioTenant Rol { get; set; } = RolUsuarioTenant.Empleado;

    public DateTime FechaAltaUtc { get; set; } = DateTime.UtcNow;
    public DateTime? FechaBajaUtc { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
}

// -----------------------------------------------------------------------------
// RRHH Core - Legajo
// -----------------------------------------------------------------------------
public sealed class Legajo : TenantEntityBase<Guid>
{
    public Guid UsuarioGlobalId { get; set; }
    public UsuarioGlobal? UsuarioGlobal { get; set; }

    [Required, StringLength(30)]
    public string NumeroLegajo { get; set; } = null!;

    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaEgreso { get; set; }

    public EstadoLaboral EstadoLaboral { get; set; } = EstadoLaboral.Activo;

    // One-to-one
    public DatosPersonalesLegajo? DatosPersonales { get; set; }

    // Navegación
    public List<ContratoLaboral> Contratos { get; set; } = new();
    public List<LegajoPuesto> PuestosHistorico { get; set; } = new();
    public List<RegistroHorario> RegistrosHorario { get; set; } = new();
    public List<Ausencia> Ausencias { get; set; } = new();
    public List<LiquidacionLegajo> Liquidaciones { get; set; } = new();
    public List<SolicitudMedica> SolicitudesMedicas { get; set; } = new();
    public List<ExamenMedico> ExamenesMedicos { get; set; } = new();
    public List<Documento> Documentos { get; set; } = new();
    public List<CampoDinamicoValor> CamposDinamicos { get; set; } = new();
}

public sealed class DatosPersonalesLegajo : TenantEntityBase<Guid>
{
    // Id = LegajoId para 1:1 “shared primary key”
    [ForeignKey(nameof(Legajo))]
    public override Guid Id
    {
        get => base.Id;
        set => base.Id = value;
    }

    public Legajo? Legajo { get; set; }

    [Required, StringLength(120)]
    public string Nombre { get; set; } = null!;

    [Required, StringLength(120)]
    public string Apellido { get; set; } = null!;

    public DateTime? FechaNacimiento { get; set; }

    [StringLength(50)]
    public string? EstadoCivil { get; set; }

    [StringLength(250)]
    public string? Direccion { get; set; }

    [StringLength(120)]
    public string? Localidad { get; set; }

    [StringLength(120)]
    public string? Provincia { get; set; }

    public short? PaisId { get; set; }
    public Pais? Pais { get; set; }
}

// -----------------------------------------------------------------------------
// Campos dinámicos (por tenant) aplicados al legajo
// -----------------------------------------------------------------------------
public sealed class CampoDinamicoDef : TenantEntityBase<Guid>
{
    [Required, StringLength(80)]
    public string Nombre { get; set; } = null!;

    [StringLength(250)]
    public string? Descripcion { get; set; }

    public TipoDatoCampoDinamico TipoDato { get; set; }

    public bool Obligatorio { get; set; }

    // Para listas/valores: opcional
    public string? ConfigJson { get; set; } // e.g. { "options":[...] }

    public List<CampoDinamicoValor> Valores { get; set; } = new();
}

public sealed class CampoDinamicoValor : TenantEntityBase<Guid>
{
    public Guid CampoDinamicoDefId { get; set; }
    public CampoDinamicoDef? CampoDinamicoDef { get; set; }

    public Guid LegajoId { get; set; }
    public Legajo? Legajo { get; set; }

    // Guardado flexible (si preferís tipado, agregamos columnas ValueX por tipo)
    public string? Valor { get; set; }
}

// -----------------------------------------------------------------------------
// Estructura organizacional / puesto
// -----------------------------------------------------------------------------
public sealed class Area : TenantEntityBase<Guid>
{
    [Required, StringLength(120)]
    public string Nombre { get; set; } = null!;

    public Guid? AreaPadreId { get; set; }
    public Area? AreaPadre { get; set; }

    public List<Area> Hijos { get; set; } = new();
}

public sealed class Puesto : TenantEntityBase<Guid>
{
    [Required, StringLength(120)]
    public string Nombre { get; set; } = null!;

    [StringLength(50)]
    public string? Nivel { get; set; }

    public Guid? AreaId { get; set; }
    public Area? Area { get; set; }

    public List<LegajoPuesto> LegajosHistorico { get; set; } = new();
}

public sealed class LegajoPuesto : TenantEntityBase<Guid>
{
    public Guid LegajoId { get; set; }
    public Legajo? Legajo { get; set; }

    public Guid PuestoId { get; set; }
    public Puesto? Puesto { get; set; }

    public DateTime FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

// -----------------------------------------------------------------------------
// Contrato laboral (parametrizable por país/convenio)
// -----------------------------------------------------------------------------
public sealed class Convenio : TenantEntityBase<Guid>
{
    [Required, StringLength(50)]
    public string Codigo { get; set; } = null!;

    [Required, StringLength(200)]
    public string Nombre { get; set; } = null!;
}

public sealed class Categoria : TenantEntityBase<Guid>
{
    [Required, StringLength(50)]
    public string Codigo { get; set; } = null!;

    [Required, StringLength(200)]
    public string Nombre { get; set; } = null!;
}

public sealed class ContratoLaboral : TenantEntityBase<Guid>
{
    public Guid LegajoId { get; set; }
    public Legajo? Legajo { get; set; }

    [Required, StringLength(50)]
    public string TipoContrato { get; set; } = null!; // Indeterminado, Plazo fijo, etc.

    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    [StringLength(50)]
    public string? JornadaLaboral { get; set; } // 8hs, part-time, turnos, etc.

    public Guid? ConvenioId { get; set; }
    public Convenio? Convenio { get; set; }

    public Guid? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public EstadoRegistro Estado { get; set; } = EstadoRegistro.Activo;
}

// -----------------------------------------------------------------------------
// Asistencia y tiempo
// -----------------------------------------------------------------------------
public sealed class RegistroHorario : TenantEntityBase<Guid>
{
    public Guid LegajoId { get; set; }
    public Legajo? Legajo { get; set; }

    public DateOnly Fecha { get; set; }

    public TimeOnly? HoraEntrada { get; set; }
    public TimeOnly? HoraSalida { get; set; }

    public OrigenRegistroHorario Origen { get; set; } = OrigenRegistroHorario.Web;

    [StringLength(250)]
    public string? Observaciones { get; set; }
}

public sealed class TipoAusencia : TenantEntityBase<Guid>
{
    // Si preferís catálogo por país, podés quitar TenantId y usar PaisId (o ambos).
    public short? PaisId { get; set; }
    public Pais? Pais { get; set; }

    [Required, StringLength(120)]
    public string Nombre { get; set; } = null!;

    public bool DescuentaSueldo { get; set; }

    [StringLength(250)]
    public string? ReglaCodigo { get; set; } // hook al motor de reglas
}

public sealed class Ausencia : TenantEntityBase<Guid>
{
    public Guid LegajoId { get; set; }
    public Legajo? Legajo { get; set; }

    public Guid TipoAusenciaId { get; set; }
    public TipoAusencia? TipoAusencia { get; set; }

    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }

    public Guid? AprobadoPorUsuarioGlobalId { get; set; }
    public UsuarioGlobal? AprobadoPor { get; set; }

    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    [StringLength(500)]
    public string? Motivo { get; set; }
}

// -----------------------------------------------------------------------------
// Nómina / Liquidaciones
// -----------------------------------------------------------------------------
public sealed class Liquidacion : TenantEntityBase<Guid>
{
    // Periodo "YYYYMM" por simplicidad; alternativamente Year+Month.
    [Required, StringLength(6)]
    public string Periodo { get; set; } = null!;

    public TipoLiquidacion Tipo { get; set; } = TipoLiquidacion.Mensual;
    public EstadoLiquidacion Estado { get; set; } = EstadoLiquidacion.Borrador;

    public List<LiquidacionLegajo> Items { get; set; } = new();
}

public sealed class LiquidacionLegajo : TenantEntityBase<Guid>
{
    public Guid LiquidacionId { get; set; }
    public Liquidacion? Liquidacion { get; set; }

    public Guid LegajoId { get; set; }
    public Legajo? Legajo { get; set; }

    [Precision(18, 2)]
    public decimal Bruto { get; set; }

    [Precision(18, 2)]
    public decimal Neto { get; set; }

    public List<LiquidacionConcepto> Conceptos { get; set; } = new();
}

public sealed class Concepto : TenantEntityBase<Guid>
{
    // Si el concepto es “global por país”, podés también colgarlo de PaisId.
    public short? PaisId { get; set; }
    public Pais? Pais { get; set; }

    [Required, StringLength(30)]
    public string Codigo { get; set; } = null!;

    [Required, StringLength(200)]
    public string Nombre { get; set; } = null!;

    public TipoConcepto Tipo { get; set; }

    // Fórmula/expresión (motor de reglas)
    [StringLength(2000)]
    public string? Formula { get; set; }

    public List<LiquidacionConcepto> Aplicaciones { get; set; } = new();
}

public sealed class LiquidacionConcepto : TenantEntityBase<Guid>
{
    public Guid LiquidacionLegajoId { get; set; }
    public LiquidacionLegajo? LiquidacionLegajo { get; set; }

    public Guid ConceptoId { get; set; }
    public Concepto? Concepto { get; set; }

    [Precision(18, 2)]
    public decimal Importe { get; set; }
}

// -----------------------------------------------------------------------------
// Salud / medicina laboral
// -----------------------------------------------------------------------------
public sealed class SolicitudMedica : TenantEntityBase<Guid>
{
    public Guid LegajoId { get; set; }
    public Legajo? Legajo { get; set; }

    [Required, StringLength(50)]
    public string Tipo { get; set; } = null!; // Visita, Certificado, Reintegro, etc.

    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;

    [StringLength(1000)]
    public string? Detalle { get; set; }
}

public sealed class ExamenMedico : TenantEntityBase<Guid>
{
    public Guid LegajoId { get; set; }
    public Legajo? Legajo { get; set; }

    [Required, StringLength(80)]
    public string Tipo { get; set; } = null!; // Pre-ocupacional, Periódico, etc.

    public DateTime Fecha { get; set; }

    [StringLength(2000)]
    public string? Resultado { get; set; }
}

// -----------------------------------------------------------------------------
// Documentación y firma
// -----------------------------------------------------------------------------
public sealed class Documento : TenantEntityBase<Guid>, ISoftDelete
{
    public Guid LegajoId { get; set; }
    public Legajo? Legajo { get; set; }

    [Required, StringLength(80)]
    public string TipoDocumento { get; set; } = null!;

    [Required, StringLength(500)]
    public string RutaArchivo { get; set; } = null!; // blob key / path

    public DateTime FechaCargaUtc { get; set; } = DateTime.UtcNow;

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserGlobalId { get; set; }

    public List<FirmaDocumento> Firmas { get; set; } = new();
}

public sealed class FirmaDocumento : TenantEntityBase<Guid>
{
    public Guid DocumentoId { get; set; }
    public Documento? Documento { get; set; }

    public Guid UsuarioGlobalId { get; set; }
    public UsuarioGlobal? UsuarioGlobal { get; set; }

    public DateTime FechaFirmaUtc { get; set; } = DateTime.UtcNow;
    public MetodoFirma Metodo { get; set; }

    [StringLength(2000)]
    public string? EvidenciaJson { get; set; } // hash, IP, provider payload, etc.
}

// -----------------------------------------------------------------------------
// Auditoría
// -----------------------------------------------------------------------------
public sealed class AuditoriaEvento : TenantEntityBase<long>
{
    public Guid? UsuarioGlobalId { get; set; }
    public UsuarioGlobal? UsuarioGlobal { get; set; }

    [Required, StringLength(120)]
    public string Entidad { get; set; } = null!;

    [Required, StringLength(80)]
    public string Accion { get; set; } = null!; // CREATE/UPDATE/DELETE/LOGIN, etc.

    public DateTime FechaUtc { get; set; } = DateTime.UtcNow;

    public string? DatosJson { get; set; } // snapshot/diff contextual
}
