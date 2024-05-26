using DataBase.Contexts.DBContext;
using DataBase.Entities.Entities_DBContext;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CQRS.Query;

public record GetWeatherCityQuery : IRequest<WeatherCity?>
{
    public string City { get; set; }
}

public record GetByIdWeatherCityQuery : IRequest<WeatherCity?>
{
    public int Id { get; set; }
}

public class GetWeatherCityQueryHandler : DbContextInjection, IRequestHandler<GetWeatherCityQuery, WeatherCity?>, IRequestHandler<GetByIdWeatherCityQuery, WeatherCity?>
{
    public GetWeatherCityQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<WeatherCity?> Handle(GetWeatherCityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await db.WeatherCity.FirstOrDefaultAsync(weatherCity => weatherCity.City == request.City.ToLower(), cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return null;
    }
    
    public async Task<WeatherCity?> Handle(GetByIdWeatherCityQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await db.WeatherCity.FirstOrDefaultAsync(weatherCity => weatherCity.Id== request.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        return null;
    }
}