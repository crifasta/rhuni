using System.ComponentModel.DataAnnotations;

namespace Domain.Common.Contracts;

 
public enum EstadoLogicoRegistro
{
    A = 1,
    B = 0
}


public interface IEntity
{
    List<DomainEvent> DomainEvents { get; }

 
}

public interface IEntity<TId> : IEntity
{
    TId Id { get; }

    string EstadoRegistro { get; set; } 
}

