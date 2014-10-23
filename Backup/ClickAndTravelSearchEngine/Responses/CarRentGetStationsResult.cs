using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelMiddleOffice.Containers.CarRent;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class CarRentGetStationsResult: _Response
    {
        private CarRentStation[] _stations;
        [JsonMemberName("stations")]
        public CarRentStation[] Stations
        {
            get { return _stations; }
            set { _stations = value; }
        }
    }
}