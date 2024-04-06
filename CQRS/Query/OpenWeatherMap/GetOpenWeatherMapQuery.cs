using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;
using System.Linq;

namespace CQRS.Query;

public struct GetOpenWeatherMapQuery : IRequest<(bool Success, OpenWeatherMap? OpenWeatherMapDB)>
{
    public string City { get; set; }
}

public class GetOpenWeatherMapQueryHandler : DbContextInjection, IRequestHandler<GetOpenWeatherMapQuery, (bool Success, OpenWeatherMap? OpenWeatherMapDB)>
{
    public GetOpenWeatherMapQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, OpenWeatherMap? OpenWeatherMapDB)> Handle(GetOpenWeatherMapQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var weather = db.OpenWeatherMap.Where(_ => _.City == request.City).OrderBy(_=> _.Date).Take(1).FirstOrDefault();
            await db.SaveChangesAsync();

            return (true, weather);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}

