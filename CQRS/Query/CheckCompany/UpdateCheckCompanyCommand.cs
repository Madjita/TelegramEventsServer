using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;
using DataBase.Entities.QrCodeEntities;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Query;

public record UpdateCheckCompanyCommand : IRequest<(bool Success, CheckCompany? CheckCompany)>
{
    public CheckCompany newCheckCompany { get; set; }
    public bool Update { get; set; }
}

public class UpdateCheckCompanyCommandHandler : DbContextInjection, IRequestHandler<UpdateCheckCompanyCommand, (bool Success, CheckCompany? CheckCompany)>
{
    public UpdateCheckCompanyCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, CheckCompany? CheckCompany)> Handle(UpdateCheckCompanyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            
            // Поиск существующей записи в базе данных
            var existingData = await db.CheckCompany
                .Where(c => c.Name.ToUpper() == request.newCheckCompany.Name.ToUpper() &&
                            c.Inn == request.newCheckCompany.Inn)
                .FirstOrDefaultAsync();


            if (existingData is null)
            {
                // Если запись не существует, добавляем новую запись
                await db.CheckCompany.AddAsync(request.newCheckCompany, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            else
            {

                if (request.Update)
                {
                    db.CheckCompany.Update(existingData);
                    await db.SaveChangesAsync(cancellationToken);
                }
                return (true, existingData);
            }
            
            return (true, request.newCheckCompany);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}
