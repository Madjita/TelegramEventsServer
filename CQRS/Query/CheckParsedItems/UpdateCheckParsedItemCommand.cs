using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;
using DataBase.Entities.QrCodeEntities;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Query;

public record UpdateCheckParsedItemCommand : IRequest<(bool Success, CheckParsedItems? checkParsedItem)>
{
    public CheckParsedItems newCheckParsedItem { get; set; }
}

public class UpdateCheckParsedItemCommandHandler : DbContextInjection, IRequestHandler<UpdateCheckParsedItemCommand, (bool Success, CheckParsedItems? checkParsedItem)>
{
    public UpdateCheckParsedItemCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, CheckParsedItems? checkParsedItem)> Handle(UpdateCheckParsedItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            
            // Поиск существующей записи в базе данных
            var existingData = await db.CheckParsedItems
                .Where(c => c.Name == request.newCheckParsedItem.Name &&
                            c.Sum == request.newCheckParsedItem.Sum &&
                            c.Price == request.newCheckParsedItem.Price &&
                            c.CheckDataId == request.newCheckParsedItem.CheckDataId )
                .FirstOrDefaultAsync();

            if (existingData != null)
            {
                db.CheckParsedItems.Update(existingData);
            }
            else
            {
                // Если запись не существует, добавляем новую запись
                await db.CheckParsedItems.AddAsync(request.newCheckParsedItem, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
            
            return (true, request.newCheckParsedItem);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}
