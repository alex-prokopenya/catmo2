using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class ServiceInfo : ServiceSimpleInfo
    {
        /*
            service_class - тип услуги (авиабилет, отель, экскурсия и т.д.)
            title
            prices
            status
            day - порядковый день в рамках заказа.
            ndays - продолжительность действия услуги, для таких услуг как страховка, виза, проживание в отеле.
            book_id
        */

        private int _serviceClass;
        [JsonMemberName("service_class")]
        public int ServiceClass
        {
            get { return _serviceClass; }
            set { _serviceClass = value; }
        }


        private KeyValuePair<string, decimal>[] _prices;
        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }

        [JsonMemberName("price")]
        public JsonObject PriceJson
        {
            get
            {
                JsonObject ret = new JsonObject();

                foreach (KeyValuePair<string, decimal> pair in this._prices)
                    ret.Add(pair.Key, pair.Value);

                return ret;
            }
            set { }
        }

      

        private int _day;
        [JsonMemberName("day")]
        public int Day
        {
            get { return _day; }
            set { _day = value; }
        }

        private int _ndays;
        [JsonMemberName("days_long")]
        public int Ndays
        {
            get { return _ndays; }
            set { _ndays = value; }
        }

      
    }
}