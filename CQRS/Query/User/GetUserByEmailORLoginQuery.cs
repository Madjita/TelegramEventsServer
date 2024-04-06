using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Contexts.DBContext;
using User = DataBase.Entities.Entities_DBContext.User;

namespace CQRS.Query;
public struct GetUserByEmailORLoginQuery : IRequest<User?>
{
    public string Email { get; set; }
    public string HPassword { get; set; }
}

public class GetUserByEmailORLoginQueryHandler : DbContextInjection, IRequestHandler<GetUserByEmailORLoginQuery, User?>
{
    public GetUserByEmailORLoginQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<User?> Handle(GetUserByEmailORLoginQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.HPassword))
        {
            return await db.User.FirstOrDefaultAsync(_ => _.Email == request.Email || _.UserName == request.Email && _.IsDeleted == false);
        }
        else
        {
            return await db.User.FirstOrDefaultAsync(_ => (_.Email == request.Email || _.UserName == request.Email) 
                                                          && _.HPassword == request.HPassword
                                                          && _.IsDeleted == false);
        }
    }
}
