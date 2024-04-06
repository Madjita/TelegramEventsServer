using System.ComponentModel.DataAnnotations.Schema;

namespace DataBase.Entities.Entities_DBContext;

[Table("weather_city")]
public class WeatherCity : EntityBase
{
    [Column("city")]
    public string City { get; set; }
    
    [Column("latitude")]
    public string Latitude { get; set; }

    [Column("longitude")]
    public string Longitude { get; set; }
}