using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;
using System.Linq;
using DataBase.Entities.QrCodeEntities;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Query;

public record GetCheckDataByProcessedQuery : IRequest<(bool Success, List<CheckData> CheckDatas)>
{
    public CheckProcessed Processed { get; set; }
}

public class GetCheckDataByProcessedQueryHandler : DbContextInjection, IRequestHandler<GetCheckDataByProcessedQuery, (bool Success, List<CheckData> CheckDatas)>
{
    public GetCheckDataByProcessedQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, List<CheckData> CheckDatas)> Handle(GetCheckDataByProcessedQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var checks = await db.CheckData
                .Where(_ => _.Processed == request.Processed)
                .OrderBy(_=> _.Date)
                .ToListAsync(cancellationToken);
            
            return (true, checks);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, new List<CheckData>());
    }
}

