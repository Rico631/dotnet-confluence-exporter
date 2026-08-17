using WebApp4.Application.Common;
using WebApp4.Application.DomainEvents;

namespace WebApp4.Application.Entities;

public class UserDocument : Entity
{

    public UserDocument()
    {

    }

    public UserDocument(User user, string name, byte[] content)
    {
        User = user;
        Name = name;
        Content = content;

        Raise(new UserDocumentAdded
        {
            DocumentId = Id,
            UserId = User.Id
        });
    }

    public required virtual User User { get; init; }

    public required string Name { get; init; }
    public required byte[] Content { get; init; }
}

