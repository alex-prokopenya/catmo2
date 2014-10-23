using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelMiddleOffice.Containers.Flights
{
    public class FlightTicket
    {
        public FlightTicket()
        { }

      
        //идентификатор билета
        private int _ticketId;

        [JsonMemberName("id")]
        public int TicketId
        {
            get { return _ticketId; }
            set { _ticketId = value; }
        }

        //полная стоимость билетов
        //private int _price;

        //public int Price
        //{
        //    get { return _price; }
        //    set { _price = value; }
        //}

        private KeyValuePair<string, decimal>[] _prices;

        [JsonMemberName("price")]
        public JsonObject Price
        {
            get 
            {
                JsonObject pr = new JsonObject();

                foreach(KeyValuePair<string, decimal> val in _prices)
                    pr.Add(val.Key, val.Value);

                return pr; 
            }
            set {  }
        }

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }
             

        //класс обслуживания E (econom) или B (business)
        private string _serviceClass;

        [JsonMemberName("service_class")]
        public string ServiceClass
        {
            get { return _serviceClass; }
            set { _serviceClass = value; }
        }

        //код авиакомпании
        private string _airlineCode;

        [JsonMemberName("airline_code")]
        public string AirlineCode
        {
            get { return _airlineCode; }
            set { _airlineCode = value; }
        }

        //время на выписку билета
        private DateTime _timeLimit;

        [JsonMemberName("time_limit")]
        public string TimeLimitString
        {
            get { return _timeLimit.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { }
        }

        [JsonIgnore]
        public DateTime TimeLimit
        {
            get { return _timeLimit; }
            set { _timeLimit = value; }
        }

        //массив участков маршрута
        private RouteItem[] _routeItems;

        [JsonMemberName("route_items")]
        public RouteItem[] RouteItems
        {
            get { return _routeItems; }
            set { _routeItems = value; }
        }
    }
}