using MediatR;
using DataBase.Contexts.DBContext;

namespace CQRS.Query.TelegramBotInChat;

public struct DeleteTelegramBotInChatCommand : IRequest<bool>
{
    public long TelegramChatId { get; set; }
    public long TelegramBotId { get; set; }
}

public class DeleteTelegramBotInChatCommandHandler : DbContextInjection, IRequestHandler<DeleteTelegramBotInChatCommand, bool>
{
    public DeleteTelegramBotInChatCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<bool> Handle(DeleteTelegramBotInChatCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var findRemoveItem = db.TelegramBotInChats.FirstOrDefault(_ => _.TelegramChatId == request.TelegramChatId && _.TelegramBotId == request.TelegramBotId);
            if (findRemoveItem != null)
            {
                db.TelegramBotInChats.Remove(findRemoveItem);
                await db.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return false;
    }
}
