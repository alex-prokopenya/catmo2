using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class Document
    {
        private string _title;
        [JsonMemberName("title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }


        private string _url;
        [JsonMemberName("url")]
        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        
        private DateTime updated;
        [JsonIgnore]
        public DateTime Updated
        {
            get { return updated; }
            set { updated = value; }
        }


        [JsonMemberName("up_date")]
        public string UpdatedString
        {
            get { return this.updated.ToString("yyyy-MM-dd HH:mm:ss"); }
            set { }
        }
    }
}