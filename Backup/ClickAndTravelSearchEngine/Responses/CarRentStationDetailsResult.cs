using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelMiddleOffice.Containers.CarRent;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class CarRentStationDetailsResult:_Response
    {
        private CarRentStationDetails _stationDetails;

        public CarRentStationDetails StationDetails
        {
            get { return _stationDetails; }
            set { _stationDetails = value; }
        }
    }
}