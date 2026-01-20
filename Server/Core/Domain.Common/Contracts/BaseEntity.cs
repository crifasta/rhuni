using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Common.Contracts;


public abstract class BaseEntity<TKey>  : IEntity<TKey>
{
    [Key]
    public TKey Id { get; set; } = default!;

    [NotMapped]
    public List<DomainEvent> DomainEvents { get; } = new();
    public string EstadoRegistro { get; set; } = EstadoLogicoRegistro.A.ToString();
}


public abstract class BaseEntity : BaseEntity<int>;