using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelMiddleOffice.Containers.CarRent;
using Jayrock.Json;

using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class CarRentGetLocationsResult: _Response
    {
        private CarRentLocation[] _Locations;
        [JsonMemberName("locations")]
        public CarRentLocation[] Locations
        {
            get { return _Locations; }
            set { _Locations = value; }
        }
    }
}