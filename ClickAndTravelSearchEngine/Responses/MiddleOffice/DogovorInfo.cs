using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class DogovorInfo
    {
        /*
            Массив услуг входящих в договор:
            
            dogovor_status
            total_prices
            paid_summ -  сколько денег уже оплачено. Может отличаться от total_prices потому что могут быть добавлены дополнительные услуги
            tour_date - дата начала путешествия
            up_date -  дата последнего пересчета бонусов. Будет использоваться в качестве маркера того что нужно обновить бонусы у нас в системе.
            messages:[{in_out, text},{},{}]
        */

        private TuristContainer[] turists;
        [JsonMemberName("turists")]
        public TuristContainer[] Turists
        {
            get { return turists; }
            set { turists = value; }
        }


        private Document[] documents;
        [JsonMemberName("documents")]
        public Document[] Documents
        {
            get { return documents; }
            set { documents = value; }
        }

        private ServiceInfo[] services;
        [JsonMemberName("services")]
        public ServiceInfo[] Services
        {
            get { return services; }
            set { services = value; }
        }

        private int _dogovorStatus;
        [JsonMemberName("dogovor_status")]
        public int DogovorStatus
        {
            get { return _dogovorStatus; }
            set { _dogovorStatus = value; }
        }

        private DateTime _tourDate;
        [JsonIgnore]
        public DateTime TourDate
        {
            get { return _tourDate; }
            set { _tourDate = value; }
        }

        [JsonMemberName("tour_date")]
        public string TourDateStr
        {
            get { return this._tourDate.ToString("yyyy-MM-dd"); }
            set { }
        }

        private DateTime _payDate;
        [JsonIgnore]
        public DateTime PayDate
        {
            get { return _payDate; }
            set { _payDate = value; }
        }

        [JsonMemberName("pay_date")]
        public string PayDateStr
        {
            get { return this._payDate.ToString("yyyy-MM-dd HH:mm:ss zzz"); }
            set { }
        }

        private KeyValuePair<string, decimal>[] _totalPrices;
        [JsonIgnore]
        public KeyValuePair<string, decimal>[] TotalPrices
        {
            get { return _totalPrices; }
            set { _totalPrices = value; }
        }

        [JsonMemberName("total_price")]
        public JsonObject TotalPriceJson
        {
            get
            {
                JsonObject ret = new JsonObject();

                foreach (KeyValuePair<string, decimal> pair in this._totalPrices)
                    ret.Add(pair.Key, pair.Value);

                return ret;
            }
            set { }
        }

        private KeyValuePair<string, decimal>[] _paidSumm;
        [JsonIgnore]
        public KeyValuePair<string, decimal>[] PaidSumm
        {
            get { return _paidSumm; }
            set { _paidSumm = value; }
        }

        [JsonMemberName("paid_sum")]
        public JsonObject PaidSumJson
        {
            get
            {
                JsonObject ret = new JsonObject();

                foreach (KeyValuePair<string, decimal> pair in this._paidSumm)
                    ret.Add(pair.Key, pair.Value);

                return ret;
            }
            set { }
        }

        private DateTime _upDate;
        [JsonIgnore]
        public DateTime UpDate
        {
            get { return _upDate; }
            set { _upDate = value; }
        }

        [JsonMemberName("up_date")]
        public string UpDateJson
        {
            get { return this._upDate.ToString("yyyy-MM-dd HH:mm:ss zzz"); }
            set { }
        }

        private DogovorMessage[] _messages;
        [JsonMemberName("messages")]
        public DogovorMessage[] Messages
        {
            get { return _messages; }
            set { _messages = value; }
        }
    }
}