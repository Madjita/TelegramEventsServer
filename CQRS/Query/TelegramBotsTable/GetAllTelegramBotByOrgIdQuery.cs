using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;


namespace CQRS;

public struct GetAllTelegramBotByOrgIdQuery : IRequest<IEnumerable<TelegramBots>>
{
    public int OrgId { get; set; }
}

public struct CheckExistTelegramBotByTokenQuery : IRequest<TelegramBots?>
{
    public string TelegramBotToken { get; set; }
}

public class GetAllTelegramBotByOrgIdQueryHandler : DbContextInjection, IRequestHandler<GetAllTelegramBotByOrgIdQuery, IEnumerable<TelegramBots>> , IRequestHandler<CheckExistTelegramBotByTokenQuery, TelegramBots?>
{
    public GetAllTelegramBotByOrgIdQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<IEnumerable<TelegramBots>> Handle(GetAllTelegramBotByOrgIdQuery request, CancellationToken cancellationToken)
    {
        return await db.TelegramBots.Where(_ => _.OrgId == request.OrgId).ToListAsync();
    }

    public async Task<TelegramBots?> Handle(CheckExistTelegramBotByTokenQuery request, CancellationToken cancellationToken)
    {
        return await db.TelegramBots.FirstOrDefaultAsync(_ => _.TelegramBotToken == request.TelegramBotToken);
    }
}
