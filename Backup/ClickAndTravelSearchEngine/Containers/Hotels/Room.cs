using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.Containers.Hotels
{
    public class Room: ParamsContainers.RequestRoom
    {
        private RoomVariant[] _variants;
        [JsonMemberName("variants")]
        public RoomVariant[] Variants
        {
            get { return _variants; }
            set { _variants = value; }
        }
    }
}