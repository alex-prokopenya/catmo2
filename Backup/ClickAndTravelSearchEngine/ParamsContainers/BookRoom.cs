using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;

namespace ClickAndTravelMiddleOffice.ParamsContainers
{
    public class BookRoom
    {
        public BookRoom() { }

        public BookRoom(JsonObject inp)
        {
            try
            {
                this._variantId = Convert.ToInt32( inp["variant_id"]);//.ToString();

                JsonArray arrTurists = inp["turists"] as JsonArray;

                this._turists = new Turist_del[arrTurists.Length];

                for (int i = 0; i < arrTurists.Length; i++)
                {
                    this._turists[i] = new Turist_del(arrTurists[i] as JsonObject);
                }
            }
            catch (Exception)
            { 
                throw new Exception("cann't parse bookRoom from " + inp.ToString());
            }
        }

        private int _variantId;

        public int VariantId
        {
            get { return _variantId; }
            set { _variantId = value; }
        }

        private Turist_del[] _turists;

        public Turist_del[] Turists
        {
            get { return _turists; }
            set { _turists = value; }
        }
    }
}