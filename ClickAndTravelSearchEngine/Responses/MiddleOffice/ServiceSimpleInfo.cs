using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class ServiceSimpleInfo
    {
        private string _title;
        [JsonMemberName("title")]
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        

        private int _status;
        [JsonMemberName("status")]
        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }

      

        private int _bookId;
        [JsonMemberName("book_id")]
        public int BookId
        {
            get { return _bookId; }
            set { _bookId = value; }
        }

    }
}