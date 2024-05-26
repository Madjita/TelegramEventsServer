using DataBase.Entities.Entities_DBContext;
using MediatR;
using Microsoft.EntityFrameworkCore;
using DataBase.Contexts.DBContext;


namespace CQRS.Query;

public record GetSliceUsersInOrganizationByOrganizationIdWithRoleIdQuery : IRequest<(bool Success, List<XOrgUser> UsersInOrganization, int TotalRecords)>
{
    public int OrganizationId { get; set; }
    public int RoleId { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class GetSliceUsersInOrganizationByOrganizationIdWithRoleIdQueryHandler : DbContextInjection, IRequestHandler<GetSliceUsersInOrganizationByOrganizationIdWithRoleIdQuery, (bool Success, List<XOrgUser> UsersInOrganization, int TotalRecords)>
{
    public GetSliceUsersInOrganizationByOrganizationIdWithRoleIdQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, List<XOrgUser> UsersInOrganization, int TotalRecords)> Handle(GetSliceUsersInOrganizationByOrganizationIdWithRoleIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = db.XOrgUser.Where(_ => _.OrgId == request.OrganizationId && _.RoleId == request.RoleId);
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
            
            // Выполняем запрос и загружаем связанные данные
            var usersInOrganization = await query
                .Include(_ => _.User)         // Явная загрузка связанных данных User
                .Include(_ => _.Organization) // Явная загрузка связанных данных Organization
                .ToListAsync(cancellationToken);
            
            return (usersInOrganization.Any(), usersInOrganization, totalRecords);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return (false, new List<XOrgUser>(), 0);
    }
}