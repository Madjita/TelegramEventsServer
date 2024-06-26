﻿using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query;

public record UpdateTelegramBotInChatCommand : IRequest<(bool Success, TelegramBotInChats? newTelegramBotInChat)>
{
    public TelegramBotInChats newTelegramBotInChat { get; set; }
}

public class UpdateTelegramBotInChatCommandHandler : DbContextInjection, IRequestHandler<UpdateTelegramBotInChatCommand, (bool Success, TelegramBotInChats? newTelegramBotInChat)>
{
    public UpdateTelegramBotInChatCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, TelegramBotInChats? newTelegramBotInChat)> Handle(UpdateTelegramBotInChatCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var findItem = db.TelegramBotInChats.FirstOrDefault(_ => _.TelegramChatId == request.newTelegramBotInChat.TelegramChatId && _.TelegramBotId == request.newTelegramBotInChat.TelegramBotId);
            if (findItem != null)
            {
                return (true, findItem);
            }
            
            db.TelegramBotInChats.Update(request.newTelegramBotInChat);
            await db.SaveChangesAsync();

            return (true, request.newTelegramBotInChat);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}
