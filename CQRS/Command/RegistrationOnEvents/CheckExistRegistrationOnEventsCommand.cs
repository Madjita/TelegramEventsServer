using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Contexts.DBContext;
using DataBase.Entities.Entities_DBContext;

namespace CQRS.Command.UserInfo;

public struct CheckExistRegistrationOnEventsCommand : IRequest<(bool Success, XEventUser? xEventUser)>
{
  public int WhoRegistratedUserId { get; set; }
  public int EventId { get; set; }
  public int UserId { get; set; }
  public string StateRegistrationOnEvent { get; set; }
}

public class CheckExistRegistrationOnEventsCommandHandler : DbContextInjection, IRequestHandler<CheckExistRegistrationOnEventsCommand, (bool Success, XEventUser? registrationOnEvents)>
{
    public CheckExistRegistrationOnEventsCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, XEventUser? registrationOnEvents)> Handle(CheckExistRegistrationOnEventsCommand request, CancellationToken cancellationToken)
    {

        try
        {
            var registrationOnEvent = await db.XEventUser.FirstOrDefaultAsync(_ => (_.UserId == request.UserId && _.EventId == request.EventId), cancellationToken);
            return (registrationOnEvent is not null, registrationOnEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return (false, null);
    }
}
