using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query;

public struct UpdateWeatherSubscribersQuery : IRequest<(bool Success, WeatherSubscribers? WeatherSubscribers)>
{
    public WeatherSubscribers WeatherSubscribers { get; set; }
}

public class UpdateWeatherSubscribersQueryHandler : DbContextInjection, IRequestHandler<UpdateWeatherSubscribersQuery, (bool Success, WeatherSubscribers? WeatherSubscribers)>
{
    public UpdateWeatherSubscribersQueryHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, WeatherSubscribers? WeatherSubscribers)> Handle(UpdateWeatherSubscribersQuery request, CancellationToken cancellationToken)
    {
        try
        {

            var currentWeather = db.WeatherSubscribers.FirstOrDefault(_ =>
                _.UserId == request.WeatherSubscribers.UserId && _.city_id == request.WeatherSubscribers.city_id);

            if (currentWeather is not null)
            {
                currentWeather.DateSubscribe = request.WeatherSubscribers.DateSubscribe;
                currentWeather.TimeSendWeather = request.WeatherSubscribers.TimeSendWeather;
                db.WeatherSubscribers.Update(currentWeather);
            }
            else
            {
                db.WeatherSubscribers.Update(request.WeatherSubscribers);
            }
            
            await db.SaveChangesAsync();

            return (true, request.WeatherSubscribers);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}


