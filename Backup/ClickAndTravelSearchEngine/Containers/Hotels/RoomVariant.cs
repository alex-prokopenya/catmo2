using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json.Conversion;
using Jayrock.Json;

namespace ClickAndTravelMiddleOffice.Containers.Hotels
{
    public class RoomVariant
    {
        private int _variantId;
        [JsonMemberName("variant_id")]
        public int VariantId
        {
            get { return _variantId; }
            set { _variantId = value; }
        }

        private string _pansionTitle;
        [JsonMemberName("pansion_title")]
        public string PansionTitle
        {
            get { return _pansionTitle; }
            set { _pansionTitle = value; }
        }

        private int _pansionGroupId;
        [JsonMemberName("pansion_group_id")]
        public int PansionGroupId
        {
            get { return _pansionGroupId; }
            set { _pansionGroupId = value; }
        }

        private string _roomTitle;
        [JsonMemberName("room_title")]
        public string RoomTitle
        {
            get { return _roomTitle; }
            set { _roomTitle = value; }
        }

        private string roomCategory;
        [JsonMemberName("room_category")]
        public string RoomCategory
        {
            get { return roomCategory; }
            set { roomCategory = value; }
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