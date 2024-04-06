using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;
using System.Linq;
using System.Collections.Generic;

namespace CQRS.Query.CustomerCompany
{
    public struct CheckExistWhoCreatedOrgCommand : IRequest<(bool Success, List<Org> customerCompany, int totalRecords)>
    {
        public int WhoRegisterdUsserId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
    }

    public class CheckExistWhoCreatedOrgCommandHandler : DbContextInjection, IRequestHandler<CheckExistWhoCreatedOrgCommand, (bool Success, List<Org> customerCompany, int totalRecords)>
    {
        public CheckExistWhoCreatedOrgCommandHandler(IDBContext dbContext) : base(dbContext) { }

        public async Task<(bool Success, List<Org> customerCompany, int totalRecords)> Handle(CheckExistWhoCreatedOrgCommand request, CancellationToken cancellationToken)
        {

            try
            {
                int totalRecords = db.Org.Where(company => company.WhoRegisterdUsserId == request.WhoRegisterdUsserId && company.IsDeleted == false).Count();

                List<Org> customerCompany = new();
                if(request.Take >= totalRecords)
                {
                    customerCompany = await db.Org.OrderBy(company => company.Id).Where(company => company.WhoRegisterdUsserId == request.WhoRegisterdUsserId && company.IsDeleted == false).ToListAsync(cancellationToken);
                }
                else if (request.Skip > 0)
                {
                    customerCompany = await db.Org.OrderBy(company => company.Id).Where(company => company.WhoRegisterdUsserId == request.WhoRegisterdUsserId && company.IsDeleted == false).Skip(request.Skip).Take(request.Take).ToListAsync(cancellationToken);
                }
                else if(request.Take > 0)
                {
                    customerCompany = await db.Org.OrderBy(company => company.Id).Where(company => company.WhoRegisterdUsserId == request.WhoRegisterdUsserId && company.IsDeleted == false).Take(request.Take).ToListAsync(cancellationToken);
                }
                else
                {
                    customerCompany = await db.Org.OrderBy(company => company.Id).Where(company => company.WhoRegisterdUsserId == request.WhoRegisterdUsserId && company.IsDeleted == false).ToListAsync(cancellationToken);
                }
                
                return (customerCompany.Any(), customerCompany, totalRecords);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return (false, new List<Org>(), 0);
        }
    }
}
