using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query;

public record GetUserByIdQuery : IRequest<User?>
{
    public long Id
    {
        get;
        set;
    }
}

public class GetUserQueryHandler : DbContextInjection, IRequestHandler<GetUserByIdQuery, User?>
{
    public GetUserQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.User.FirstOrDefaultAsync(user => user.Id == request.Id && user.IsDeleted == false, cancellationToken);
    }
}
