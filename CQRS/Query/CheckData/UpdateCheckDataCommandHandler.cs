using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;
using DataBase.Entities.QrCodeEntities;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Query;

public record UpdateCheckDataCommand : IRequest<(bool Success, CheckData? checkData)>
{
    public CheckData newCheckData { get; set; }
    public bool Update { get; set; }
}

public class UpdateCheckDataCommandHandler : DbContextInjection, IRequestHandler<UpdateCheckDataCommand, (bool Success, CheckData? checkData)>
{
    public UpdateCheckDataCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, CheckData? checkData)> Handle(UpdateCheckDataCommand request, CancellationToken cancellationToken)
    {
        try
        {
            
            // Поиск существующей записи в базе данных
            var existingData = await db.CheckData
                .Where(c => c.T == request.newCheckData.T &&
                            c.S == request.newCheckData.S &&
                            c.Fn == request.newCheckData.Fn &&
                            c.I == request.newCheckData.I &&
                            c.Fp == request.newCheckData.Fp &&
                            c.N == request.newCheckData.N)
                .FirstOrDefaultAsync();

            if (existingData is null)
            {
                // Если запись не существует, добавляем новую запись
                await db.CheckData.AddAsync(request.newCheckData, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
                return (true, request.newCheckData);
            }
            else if (request.Update)
            {
                existingData.Date = request.newCheckData.Date.ToUniversalTime();
                existingData.Processed = request.newCheckData.Processed;
                db.CheckData.Update(existingData);
                await db.SaveChangesAsync(cancellationToken);
                return (true, existingData);
            }
            
            return (false, existingData);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}
