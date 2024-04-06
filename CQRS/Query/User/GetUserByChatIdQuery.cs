using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query
{
    public struct GetUserByChatIdQuery : IRequest<User?>
    {
        public long TelegramChatId { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class GetUserByChatIdQueryHandler : DbContextInjection, IRequestHandler<GetUserByChatIdQuery, User?>
    {
        public GetUserByChatIdQueryHandler(IDBContext dbContext) : base(dbContext) { }

        public async Task<User?> Handle(GetUserByChatIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.IsDeleted)
                {
                    return await db.User.FirstOrDefaultAsync(telegramUser => telegramUser.TelegramChatId == request.TelegramChatId && telegramUser.IsDeleted == request.IsDeleted, cancellationToken);
                }
                else
                {
                    return await db.User.FirstOrDefaultAsync(telegramUser => telegramUser.TelegramChatId == request.TelegramChatId, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }
    }
}
