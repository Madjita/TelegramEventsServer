using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Entities.Entities_DBContext
{
    [Table("open_weather_map")]
    public class OpenWeatherMap : EntityBase
    {
        public DateTime Date { get; set; }

        [Column("City")]
        public string? City { get; set; }

        [Column("Temprature")]
        public double Temprature { get; set; }

        [Column("FeelsLike")]
        public double FeelsLike { get; set; }

        [Column("TempratureMin")]
        public double TempratureMin { get; set; }

        [Column("TempratureMax")]
        public double TempratureMax { get; set; }

        [Column("Pressure")]
        public long Pressure { get; set; }

        [Column("Humidity")]
        public long Humidity { get; set; }

        [Column("SeaLevel")]
        public long SeaLevel { get; set; }

        [Column("Type_from_received_date")]
        public string TypeFromReceivedDate { get; set; }
    }
}
