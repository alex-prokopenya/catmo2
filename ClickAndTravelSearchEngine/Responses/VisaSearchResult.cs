using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelMiddleOffice.Containers.Visa;
using Jayrock.Json.Conversion;
namespace ClickAndTravelMiddleOffice.Responses
{
    public class VisaSearchResult
    {
        private string _searchId;

        [JsonMemberName("search_id")]
        public string SearchId
        {
            get { return _searchId; }
            set { _searchId = value; }
        }

        private VisaDetails _visaDetails;

        [JsonMemberName("visa_details")]
        public VisaDetails VisaDetails
        {
            get { return _visaDetails; }
            set { _visaDetails = value; }
        }
    }
}