using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClickAndTravelMiddleOffice.Containers.Tours;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class TourRoutesResult:_Response
    {
        private TourSearchState _searchState;
        [JsonMemberName("search_state")]
        public TourSearchState SearchState
        {
            get { return _searchState; }
            set { _searchState = value; }
        }

        private TourRoute[] _routes;
        [JsonMemberName("routes")]
        public TourRoute[] Routes
        {
            get { return _routes; }
            set { _routes = value; }
        }
    }
}