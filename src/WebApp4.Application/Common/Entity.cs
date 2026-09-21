
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp4.Application.Common;

public interface IEntity
{

}

public abstract class Entity : IAggregateRoot, IEntity
{
    public Guid Id { get; set; }

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    [NotMapped]
    private HashSet<IDomainEvent> _domainEvents = [];

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}

