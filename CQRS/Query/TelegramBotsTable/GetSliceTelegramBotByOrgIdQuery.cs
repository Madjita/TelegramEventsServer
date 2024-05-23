using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;


namespace CQRS;

public struct GetSliceTelegramBotByOrgIdQuery : IRequest<(bool Success, List<TelegramBots> TelegramBots, int TotalRecords)>
{
    public int OrgId { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class GetSliceTelegramBotByOrgIdQueryHandler : DbContextInjection, IRequestHandler<GetSliceTelegramBotByOrgIdQuery, (bool Success, List<TelegramBots> TelegramBots, int TotalRecords)>
{
    public GetSliceTelegramBotByOrgIdQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, List<TelegramBots> TelegramBots, int TotalRecords)> Handle(GetSliceTelegramBotByOrgIdQuery request, CancellationToken cancellationToken)
    {
        int totalRecords = db.TelegramBots.Count(_ => _.OrgId == request.OrgId);

        List<TelegramBots> telegramBots = new();
        if(request.Take >= totalRecords)
        {
            telegramBots = await db.TelegramBots.OrderBy(_ => _.Id).Where(_ => _.OrgId == request.OrgId).ToListAsync(cancellationToken);
        }
        else if (request.Skip > 0)
        {
            telegramBots = await db.TelegramBots.OrderBy(_ => _.Id).Where(_ => _.OrgId == request.OrgId).Skip(request.Skip).Take(request.Take).ToListAsync(cancellationToken);
        }
        else if(request.Take > 0)
        {
            telegramBots = await db.TelegramBots.OrderBy(_ => _.Id).Where(_ => _.OrgId == request.OrgId).Take(request.Take).ToListAsync(cancellationToken);
        }
        else
        {
            telegramBots = await db.TelegramBots.OrderBy(_ => _.Id).Where(_ => _.OrgId == request.OrgId).ToListAsync(cancellationToken);
        }
                
        return (telegramBots.Any(), telegramBots, totalRecords);
    }
}