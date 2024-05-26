using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;
using System.Linq;
using System.Collections.Generic;

namespace CQRS.Query.CustomerCompany
{
    public record CheckExistWhoCreatedOrgCommand : IRequest<(bool Success, List<Org> customerCompany, int totalRecords)>
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
                var query = db.Org.Where(company => company.WhoRegisterdUsserId == request.WhoRegisterdUsserId && company.IsDeleted == false);
                var totalRecords = await query.CountAsync(cancellationToken);
                query = query.OrderBy(_ => _.Id);
                if (request.Skip > 0) 
                {
                   query = query.Skip(request.Skip); 
                } 
                if (request.Take > 0 && request.Take < totalRecords) 
                {
                   query = query.Take(request.Take); 
                } 
                var customerCompany = await query.ToListAsync(cancellationToken); 
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
