using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;


namespace CQRS;

public struct GetAllTelegramBotsQuery : IRequest<IEnumerable<TelegramBots>>
{
    public GetAllTelegramBotsQuery()
    {

    }
}


public class GetAllTelegramBotsQueryHandler : DbContextInjection, IRequestHandler<GetAllTelegramBotsQuery, IEnumerable<TelegramBots>>
{
    public GetAllTelegramBotsQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<IEnumerable<TelegramBots>> Handle(GetAllTelegramBotsQuery request, CancellationToken cancellationToken)
    {
        return await db.TelegramBots.ToListAsync();
    }
}