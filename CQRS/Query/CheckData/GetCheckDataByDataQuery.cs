using DataBase.Contexts.DBContext;
using DataBase.Entities.QrCodeEntities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Query;

public class GetCheckDataByDataQuery : IRequest<(bool Success, CheckData? CheckData)>
{
    public GetCheckDataByDataQuery(CheckData dataRow)
    {
        DataRow = dataRow;
    }

    public CheckData DataRow { get; set; }
}

public class GetCheckDataByDataQueryHandler : DbContextInjection, IRequestHandler<GetCheckDataByDataQuery, (bool Success, CheckData? CheckDatas)>
{
    public GetCheckDataByDataQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, CheckData? CheckDatas)> Handle(GetCheckDataByDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var check = await db.CheckData.FirstOrDefaultAsync(_ => _.I == request.DataRow.I, cancellationToken: cancellationToken);
            
            return (true, check);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false,null);
    }
}

