using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query;

public struct UpdateEventsCommand : IRequest<(bool Success, Event? events)>
{
    public Event newEvent { get; set; }
}

public class UpdateEventsCommandHandler : DbContextInjection, IRequestHandler<UpdateEventsCommand, (bool Success, Event? newEvent)>
{
    public UpdateEventsCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, Event? newEvent)> Handle(UpdateEventsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            db.Event.Update(request.newEvent);
            await db.SaveChangesAsync();

            return (true, request.newEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}
