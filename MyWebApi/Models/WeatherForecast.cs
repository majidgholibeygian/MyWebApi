using System;
using System.Collections.Generic;
using System.Text;

namespace mywebapi.Models
{
    public class WeatherForecast
    {
        public DateTime Date { get; internal set; }
        public int TemperatureC { get; internal set; }
        public string Summary { get; internal set; }
    }
}
