using WebApp4.Application.Common;

namespace WebApp4.Application.DomainEvents;

public record UserDocumentAdded : IDomainEvent
{
    public Guid UserId { get; set; }
    public Guid DocumentId { get; set; }
}

