using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Contexts.DBContext;
using DataBase.Entities.Entities_DBContext;

namespace CQRS.Query.Events;

public record CheckExistEventsCommand : IRequest<(bool Success, Event? newEvent)>
{
    public string Name { get; set; }
    public int OrgId { get; set; }
}

public class CheckExistEventsCommandHandler : DbContextInjection, IRequestHandler<CheckExistEventsCommand, (bool Success, Event? newEvent)>
{
    public CheckExistEventsCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, Event? newEvent)> Handle(CheckExistEventsCommand request, CancellationToken cancellationToken)
    {

        try
        {
            var userInfo = await db.Event.FirstOrDefaultAsync(_ => _.Name == request.Name, cancellationToken);
            return (userInfo is not null, userInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return (false, null);
    }
}
