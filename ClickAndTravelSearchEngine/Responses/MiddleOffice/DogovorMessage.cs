using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;


namespace ClickAndTravelMiddleOffice.Responses
{
    public class DogovorMessage
    {
        private int _inOut;
        [JsonMemberName("in_out")]
        public int InOut
        {
            get { return _inOut; }
            set { _inOut = value; }
        }

        private DateTime _date;
        [JsonIgnore]
        public DateTime Date
        {
            get { return _date; }
            set { _date = value; }
        }

        [JsonMemberName("date")]
        public string DateJson
        {
            get { return _date.ToString("yyyy-MM-dd HH:mm:ss zzz"); }
            set { }
        }

        private string _text;
        [JsonMemberName("text")]
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

    }
}