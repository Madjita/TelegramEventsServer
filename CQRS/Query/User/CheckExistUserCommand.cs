using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Contexts.DBContext;
using DataBase.Entities.Entities_DBContext;

namespace CQRS.Query;

public record CheckExistUserCommand : IRequest<(bool Success, User? userInfo)>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
}

public class CheckExistUserCommandHandler : DbContextInjection, IRequestHandler<CheckExistUserCommand, (bool Success, User? userInfo)>
{
    public CheckExistUserCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, User? userInfo)> Handle(CheckExistUserCommand request, CancellationToken cancellationToken)
    {

        try
        {
            var userInfo = await db.User.FirstOrDefaultAsync(_ => _.FirstName == request.FirstName 
                                                                  && _.LastName == request.LastName 
                                                                  && _.MiddleName == request.MiddleName 
                                                                  && _.IsDeleted == false, cancellationToken);
            return (userInfo is not null, userInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return (false, null);
    }
}
