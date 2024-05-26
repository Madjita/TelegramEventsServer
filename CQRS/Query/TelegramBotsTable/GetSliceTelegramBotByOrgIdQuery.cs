using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;


namespace CQRS;

public record GetSliceTelegramBotByOrgIdQuery : IRequest<(bool Success, List<TelegramBots> TelegramBots, int TotalRecords)>
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
        try
        {
            var query = db.TelegramBots.Where(_ => _.OrgId == request.OrgId);
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
            var telegramBots = await query.ToListAsync(cancellationToken); 
            return (telegramBots.Any(), telegramBots, totalRecords); 
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return (false, new List<TelegramBots>(), 0);
    }
}