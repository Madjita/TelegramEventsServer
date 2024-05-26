using MediatR;
using DataBase.Entities.Entities_DBContext;
using DataBase.Contexts.DBContext;

namespace CQRS.Query;

public record UpdateOpenWeatherMapCommand : IRequest<(bool Success, OpenWeatherMap? OpenWeatherMapDB)>
{
    public OpenWeatherMap OpenWeatherMapDB { get; set; }
}

public class UpdateOpenWeatherMapCommandHandler : DbContextInjection, IRequestHandler<UpdateOpenWeatherMapCommand, (bool Success, OpenWeatherMap? OpenWeatherMapDB)>
{
    public UpdateOpenWeatherMapCommandHandler(IDBContext dbContext) : base(dbContext) { }

    public async Task<(bool Success, OpenWeatherMap? OpenWeatherMapDB)> Handle(UpdateOpenWeatherMapCommand request, CancellationToken cancellationToken)
    {
        try
        {
            db.OpenWeatherMap.Update(request.OpenWeatherMapDB);
            await db.SaveChangesAsync();

            return (true, request.OpenWeatherMapDB);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return (false, null);
    }
}

