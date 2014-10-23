using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelMiddleOffice.Containers.CarRent;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class CarRentExtrasResult: _Response
    {
        private CarRentExtra[] _extras;
        [JsonMemberName("extras")]
        public CarRentExtra[] Extras
        {
          get { return _extras; }
          set { _extras = value; }
        }
    }
}