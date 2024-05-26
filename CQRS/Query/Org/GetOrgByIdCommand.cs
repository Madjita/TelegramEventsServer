using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query.CustomerCompany
{
    public record GetOrgByIdCommand : IRequest<(bool Success, Org? customerCompany)>
    {
        public int Id { get; set; }
    }

    public class GetOrgByIdCommandHandler : DbContextInjection, IRequestHandler<GetOrgByIdCommand, (bool Success, Org? customerCompany)>
    {
        public GetOrgByIdCommandHandler(IDBContext dbContext) : base(dbContext) { }

        public async Task<(bool Success, Org? customerCompany)> Handle(GetOrgByIdCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var customerCompany = await db.Org.FirstOrDefaultAsync(customerCompany => customerCompany.Id == request.Id, cancellationToken);
                return (customerCompany is not null, customerCompany);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return (false, null);
        }
    }
}

