using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;


namespace ClickAndTravelMiddleOffice.Responses
{
    public class NewDogovorResponse
    {
        private string _dogovorCode;
        [JsonMemberName("dogovor_code")]
        public string DogovorCode
        {
            get { return _dogovorCode; }
            set { _dogovorCode = value; }
        }

        private DateTime _payDate;
        [JsonIgnore]
        public DateTime PayDate
        {
            get { return _payDate; }
            set { _payDate = value; }
        }

        [JsonMemberName("pay_date")]
        public string TimelimitJson
        {
            set { }
            get {
                return this.PayDate.ToString("yyyy-MM-dd HH:mm:ss zzz");
            }
        }

        private KeyValuePair<string, decimal>[] _prices;

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }

        [JsonMemberName("price")]
        public JsonObject Price
        {
            set { }

            get {
                JsonObject ret = new JsonObject();
                
                foreach(KeyValuePair<string,decimal> pair in this._prices)
                    ret.Add(pair.Key, pair.Value);

                return ret;
            }
        }
    }
}