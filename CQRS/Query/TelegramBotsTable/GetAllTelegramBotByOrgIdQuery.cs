using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;


namespace CQRS;

public record GetAllTelegramBotByOrgIdQuery : IRequest<IEnumerable<TelegramBots>>
{
    public int OrgId { get; set; }
}

public record CheckExistTelegramBotByTokenQuery : IRequest<TelegramBots?>
{
    public string TelegramBotToken { get; set; }
}

public record GetTelegramBotByIdQuery : IRequest<TelegramBots?>
{
    public int Id { get; set; }
}

public class GetAllTelegramBotByOrgIdQueryHandler : 
    DbContextInjection, 
    IRequestHandler<GetAllTelegramBotByOrgIdQuery, 
    IEnumerable<TelegramBots>> , 
    IRequestHandler<CheckExistTelegramBotByTokenQuery, TelegramBots?>, 
    IRequestHandler<GetTelegramBotByIdQuery, TelegramBots?>
{
    public GetAllTelegramBotByOrgIdQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<IEnumerable<TelegramBots>> Handle(GetAllTelegramBotByOrgIdQuery request, CancellationToken cancellationToken)
    {
        return await db.TelegramBots.Where(_ => _.OrgId == request.OrgId).ToListAsync(cancellationToken);
    }

    public async Task<TelegramBots?> Handle(CheckExistTelegramBotByTokenQuery request, CancellationToken cancellationToken)
    {
        return await db.TelegramBots.FirstOrDefaultAsync(_ => _.TelegramBotToken == request.TelegramBotToken, cancellationToken);
    }
    
    public async Task<TelegramBots?> Handle(GetTelegramBotByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.TelegramBots.FirstOrDefaultAsync(_ => _.Id == request.Id, cancellationToken);
    }
}
