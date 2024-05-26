using MediatR;
using DataBase.Contexts.DBContext;
using DataBase.Entities.Entities_DBContext;

namespace CQRS.Query;

public record UpdateXOrgUserCommand : IRequest<bool>
{
    public XOrgUser newOrgUser { get; set; }
}

public class UpdateXOrgUserCommandHandler : DbContextInjection, IRequestHandler<UpdateXOrgUserCommand, bool>
{
    public UpdateXOrgUserCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<bool> Handle(UpdateXOrgUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            db.XOrgUser.Update(request.newOrgUser);
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
