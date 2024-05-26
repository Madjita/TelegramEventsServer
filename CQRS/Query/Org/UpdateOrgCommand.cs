using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query;

public record UpdateOrgCommand : IRequest<(bool Success, Org? customerCompany)>
{
    public Org newCustomerCompany { get; set; }
}

public class UpdateOrgCommandHandler : DbContextInjection, IRequestHandler<UpdateOrgCommand, (bool Success, Org? customerCompany)>
{
    public UpdateOrgCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, Org? customerCompany)> Handle(UpdateOrgCommand request, CancellationToken cancellationToken)
    {
        try
        {
            db.Org.Update(request.newCustomerCompany);
            await db.SaveChangesAsync();

            return (true, request.newCustomerCompany);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}
