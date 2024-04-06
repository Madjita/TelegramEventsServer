using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("weather_subscribers")]
    public class WeatherSubscribers : EntityBase
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("city_id")]
        public int city_id { get; set; }

        [Column("date_subscribe")]
        public DateTime? DateSubscribe { get; set; }

        [Column("time_send_weather")]
        public string? TimeSendWeather { get; set; }

        public ICollection<User> Users { get; set; } = new HashSet<User>();
    }
}
