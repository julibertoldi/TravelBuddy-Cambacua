using System;

namespace TravelBuddy.Cities
{
    public class CityDetailDto : CityDto
    {
        public string Region { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Population { get; set; }
    }
}
