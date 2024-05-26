using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query.CustomerCompany
{
    public record CheckExistOrgCommand : IRequest<(bool Success, Org? customerCompany)>
    {
        public string Name { get; set; }
    }

    public class CheckExistOrgCommandHandler : DbContextInjection, IRequestHandler<CheckExistOrgCommand, (bool Success, Org? customerCompany)>
    {
        public CheckExistOrgCommandHandler(IDBContext dbContext) : base(dbContext) { }

        public async Task<(bool Success, Org? customerCompany)> Handle(CheckExistOrgCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var customerCompany = await db.Org.FirstOrDefaultAsync(customerCompany => customerCompany.Name == request.Name, cancellationToken);
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
