using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;


namespace CQRS;

public record GetSliceTelegramBotTypeByIsServicesQuery : IRequest<(bool Success, List<TelegramBotTypes> TelegramBotTypes, int TotalRecords)>
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
        try
        {
            var query = db.TelegramBotTypes.Where(_ => _.TelegramBotTypeIsSystem == request.IsSystem);
            var totalRecords = await query.CountAsync(cancellationToken);
            query = query.OrderBy(_ => _.Id);
            if (request.Skip > 0) 
            {
                query = query.Skip(request.Skip); 
            } 
            if (request.Take > 0 && request.Take < totalRecords) 
            {
                query = query.Take(request.Take); 
            } 
            var telegramBotTypes = await query.ToListAsync(cancellationToken); 
            return (telegramBotTypes.Any(), telegramBotTypes, totalRecords); 
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return (false, new List<TelegramBotTypes>(), 0);
    }
}