using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Command.UserInfo;

public struct UpdateRegistrationOnEventsCommand : IRequest<(bool Success, XEventUser? registrationOnEvents)>
{
    public XEventUser newRegistrationOnEvents { get; set; }
}

public class UpdateRegistrationOnEventsCommandHandler : DbContextInjection, IRequestHandler<UpdateRegistrationOnEventsCommand, (bool Success, XEventUser? registrationOnEvents)>
{
    public UpdateRegistrationOnEventsCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, XEventUser? registrationOnEvents)> Handle(UpdateRegistrationOnEventsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            db.XEventUser.Update(request.newRegistrationOnEvents);
            await db.SaveChangesAsync();

            return (true, request.newRegistrationOnEvents);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}
