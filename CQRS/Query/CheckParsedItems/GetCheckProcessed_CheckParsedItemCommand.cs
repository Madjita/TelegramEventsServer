using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;
using DataBase.Entities.QrCodeEntities;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Query;

public record GetCheckProcessed_CheckParsedItemCommand : IRequest<(bool Success, List<CheckParsedItems> ListCheckParsedItems)>
{
    public CheckData CheckData { get; set; }
}

public class GetCheckProcessed_CheckParsedItemCommandHandler : DbContextInjection, IRequestHandler<GetCheckProcessed_CheckParsedItemCommand, (bool Success, List<CheckParsedItems> ListCheckParsedItems)>
{
    public GetCheckProcessed_CheckParsedItemCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, List<CheckParsedItems> ListCheckParsedItems)> Handle(GetCheckProcessed_CheckParsedItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            List<CheckParsedItems> resultList;

            var query = db.CheckParsedItems
                .Include(p => p.CheckData)  // Предварительная загрузка CheckData
                .Include(p => p.CheckCompany)  // Предварительная загрузка CheckCompany
                .Where(p => p.CheckData != null &&
                            p.CheckData.T == request.CheckData.T &&
                            p.CheckData.S == request.CheckData.S &&
                            p.CheckData.Fn == request.CheckData.Fn &&
                            p.CheckData.I == request.CheckData.I &&
                            p.CheckData.Fp == request.CheckData.Fp &&
                            p.CheckData.N == request.CheckData.N);

            resultList = await query.ToListAsync(cancellationToken: cancellationToken);
            
            return (resultList is not null, resultList);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        
        return (false, null);
    }
}
