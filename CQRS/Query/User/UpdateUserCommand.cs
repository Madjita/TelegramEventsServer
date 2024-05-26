using MediatR;
using DataBase.Contexts.DBContext;
using DataBase.Entities.Entities_DBContext;

namespace CQRS.Query;

public record UpdateUserCommand : IRequest<bool>
{
    public User newUser { get; set; }
}

public class UpdateUserCommandHandler : DbContextInjection, IRequestHandler<UpdateUserCommand, bool>
{
    public UpdateUserCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            db.User.Update(request.newUser);
            await db.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return false;
    }
}
