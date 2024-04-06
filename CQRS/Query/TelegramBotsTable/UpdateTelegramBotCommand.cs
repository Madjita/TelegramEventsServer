using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;


namespace CQRS;

public struct UpdateTelegramBotCommand : IRequest<(bool Success, TelegramBots? TelegramBot)>
{
    public TelegramBots newBot { get; set; }
}

public class UpdateTelegramBotCommandHandler : DbContextInjection, IRequestHandler<UpdateTelegramBotCommand, (bool Success, TelegramBots? TelegramBot)>
{
    public UpdateTelegramBotCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, TelegramBots? TelegramBot)> Handle(UpdateTelegramBotCommand request, CancellationToken cancellationToken)
    {
        try
        {
            db.TelegramBots.Update(request.newBot);
            await db.SaveChangesAsync();

            return (true, request.newBot);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}