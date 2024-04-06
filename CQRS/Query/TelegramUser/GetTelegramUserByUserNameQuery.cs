using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS
{
    public struct GetUserByUserNameQuery : IRequest<User?>
    {
        public string UserName
        {
            get;
            set;
        }
    }

    public class GetUserByUserNameQueryHandler : DbContextInjection, IRequestHandler<GetUserByUserNameQuery, User?>
    {
        public GetUserByUserNameQueryHandler(IDBContext dbContext) : base(dbContext) { }

        public async Task<User?> Handle(GetUserByUserNameQuery request, CancellationToken cancellationToken)
        {
            return await db.User.FirstOrDefaultAsync(telegramUser => telegramUser.UserName == request.UserName, cancellationToken);
        }
    }
}
