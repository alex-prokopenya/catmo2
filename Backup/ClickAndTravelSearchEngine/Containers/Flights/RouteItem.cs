using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Containers.Flights
{
    //класс для описания участка маршрута с учетом пересадок и стыковок
    public class RouteItem
    {
        public RouteItem()
        { }


        //продолжительность перелетов с учетом ожидания
        private int _timeLong;

        [JsonMemberName("time_long")]
        public int TimeLong
        {
            get { return _timeLong; }
            set { _timeLong = value; }
        }

        //все перелеты при движении по участку маршрута
        private Leg[] _legs;

        [JsonMemberName("legs")]
        public Leg[] Legs
        {
            get { return _legs; }
            set { _legs = value; }
        }
    }
}