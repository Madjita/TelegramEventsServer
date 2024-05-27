using DataBase.Contexts.DBContext;
using DataBase.Entities.Entities_DBContext;
using MediatR;

namespace CQRS.Query.TelegramBotInChat;

public record GetTelegramBotInChatCommand : IRequest<TelegramBotInChats>
{
    public long TelegramBotId { get; set; }
}

public class GetTelegramBotInChatCommandHandler : DbContextInjection, IRequestHandler<GetTelegramBotInChatCommand, TelegramBotInChats>
{
    public GetTelegramBotInChatCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<TelegramBotInChats> Handle(GetTelegramBotInChatCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return db.TelegramBotInChats.FirstOrDefault(_ => _.TelegramBotId == request.TelegramBotId);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return null;
    }
}
