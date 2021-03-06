﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelMiddleOffice.Responses
{
    public class HotelVerifyResult : _Response
    {
        private int _variandId;
        [JsonMemberName("variant_id")]
        public int VariantId
        {
            get { return _variandId; }
            set { _variandId = value; }
        }

        private bool _isAvailable;
        [JsonMemberName("is_available")]
        public bool IsAvailable
        {
            get { return _isAvailable; }
            set { _isAvailable = value; }
        }

        private KeyValuePair<string, decimal>[] _prices;

        [JsonMemberName("price")]
        public JsonObject Price
        {
            get
            {
                JsonObject pr = new JsonObject();

                foreach (KeyValuePair<string, decimal> val in _prices)
                    pr.Add(val.Key, val.Value);

                return pr;
            }
            set { }
        }

        [JsonIgnore]
        public KeyValuePair<string, decimal>[] Prices
        {
            get { return _prices; }
            set { _prices = value; }
        }
    }
}