using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class DogovorHeader
    {
        /*
            dogovor_code - код договора
            dogovor_status - 
            tour_date - дата начала тура
            total_prices - общая стоимость путешествия в виде хэша цен в различных валютах
            paid_summ - сколько денег уже оплачено. Может отличаться от total_prices потому что могут быть добавлены дополнительные услуги
            up_date - дата последнего пересчета бонусов. Будет использоваться в качестве маркера того что нужно обновить бонусы у нас в системе.
        */

        private string _dogovorCode;
        [JsonMemberName("dogovor_code")]
        public string DogovorCode
        {
            get { return _dogovorCode; }
            set { _dogovorCode = value; }
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
            get {
                JsonObject ret = new JsonObject();

                foreach (KeyValuePair<string, decimal> pair in this._totalPrices)
                    ret.Add(pair.Key, pair.Value);

                return ret;
            }
            set {  }
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
            set {  }
        }

        private ServiceSimpleInfo[] _services;

        [JsonMemberName("services")]
        public ServiceSimpleInfo[] Services
        {
            get{return this._services;}
            set{this._services = value;}
        }

        private int _docs_cnt;
        [JsonMemberName("documents_count")]
        public int DocumentsCount
        {
            get { return this._docs_cnt; }
            set { this._docs_cnt = value; }
        }

        private int _comments_cnt;
        [JsonMemberName("comments_cnt")]
        public int CommentsCount
        {
            get { return this._comments_cnt; }
            set { this._comments_cnt = value; }
        }
    }
}