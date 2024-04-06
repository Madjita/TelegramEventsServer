using DataBase.Contexts.DBContext;
using DataBase.Entities.Entities_DBContext;
using MediatR;

namespace CQRS.Query;

public struct UpdateWeatherCityQuery : IRequest<(bool Success, WeatherCity? WeatherCity)>
{
    public WeatherCity WeatherCity { get; set; }
}

public class UpdateWeatherCityQueryHandler : DbContextInjection, IRequestHandler<UpdateWeatherCityQuery, (bool Success, WeatherCity? WeatherCity)>
{
    public UpdateWeatherCityQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, WeatherCity? WeatherCity)> Handle(UpdateWeatherCityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            db.WeatherCity.Update(request.WeatherCity);
            await db.SaveChangesAsync();

            return (true, request.WeatherCity);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}
