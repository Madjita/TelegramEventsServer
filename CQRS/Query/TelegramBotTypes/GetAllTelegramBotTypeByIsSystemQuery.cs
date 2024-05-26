using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS;

public record GetAllTelegramBotTypeByIsSystemQuery : IRequest<IEnumerable<TelegramBotTypes>>
{
    public bool IsSystem { get; set; }
}


public class GetAllTelegramBotTypeByServicesQueryHandler : DbContextInjection, IRequestHandler<GetAllTelegramBotTypeByIsSystemQuery, IEnumerable<TelegramBotTypes>>
{
    public GetAllTelegramBotTypeByServicesQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<IEnumerable<TelegramBotTypes>> Handle(GetAllTelegramBotTypeByIsSystemQuery request, CancellationToken cancellationToken)
    {
        return await db.TelegramBotTypes.Where(_ => _.TelegramBotTypeIsSystem == request.IsSystem).ToListAsync();
    }
}
