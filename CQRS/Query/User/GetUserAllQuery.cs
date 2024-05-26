using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query;
public record GetUserAllQuery : IRequest<List<User>>
{
}

public class GetUserAllQueryHandler : DbContextInjection, IRequestHandler<GetUserAllQuery, List<User>>
{
    public GetUserAllQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<List<User>> Handle(GetUserAllQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await db.User.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return new List<User>();
    }
}