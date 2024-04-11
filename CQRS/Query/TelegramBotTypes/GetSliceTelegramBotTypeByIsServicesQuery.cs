using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;


namespace CQRS;

public struct GetSliceTelegramBotTypeByIsServicesQuery : IRequest<(bool Success, List<TelegramBotTypes> TelegramBotTypes, int TotalRecords)>
{
    public bool IsSystem { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class GetSliceTelegramBotTypeByIsServicesQueryHandler : DbContextInjection, IRequestHandler<GetSliceTelegramBotTypeByIsServicesQuery, (bool Success, List<TelegramBotTypes> TelegramBotTypes, int TotalRecords)>
{
    public GetSliceTelegramBotTypeByIsServicesQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, List<TelegramBotTypes> TelegramBotTypes, int TotalRecords)> Handle(GetSliceTelegramBotTypeByIsServicesQuery request, CancellationToken cancellationToken)
    {
        int totalRecords = db.TelegramBotTypes.Where(_ => _.TelegramBotTypeIsSystem == request.IsSystem).Count();

        List<TelegramBotTypes> telegramBotTypes = new();
        if(request.Take >= totalRecords)
        {
            telegramBotTypes = await db.TelegramBotTypes.OrderBy(_ => _.Id).Where(_ => _.TelegramBotTypeIsSystem == request.IsSystem).ToListAsync(cancellationToken);
        }
        else if (request.Skip > 0)
        {
            telegramBotTypes = await db.TelegramBotTypes.OrderBy(_ => _.Id).Where(_ => _.TelegramBotTypeIsSystem == request.IsSystem).Skip(request.Skip).Take(request.Take).ToListAsync(cancellationToken);
        }
        else if(request.Take > 0)
        {
            telegramBotTypes = await db.TelegramBotTypes.OrderBy(_ => _.Id).Where(_ => _.TelegramBotTypeIsSystem == request.IsSystem).Take(request.Take).ToListAsync(cancellationToken);
        }
        else
        {
            telegramBotTypes = await db.TelegramBotTypes.OrderBy(_ => _.Id).Where(_ => _.TelegramBotTypeIsSystem == request.IsSystem).ToListAsync(cancellationToken);
        }
                
        return (telegramBotTypes.Any(), telegramBotTypes, totalRecords);
    }
}