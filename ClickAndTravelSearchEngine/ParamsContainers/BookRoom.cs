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

                this._turists = new TuristCATMO[arrTurists.Length];

                for (int i = 0; i < arrTurists.Length; i++)
                {
                    this._turists[i] = new TuristCATMO(arrTurists[i] as JsonObject);
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

        private TuristCATMO[] _turists;

        public TuristCATMO[] Turists
        {
            get { return _turists; }
            set { _turists = value; }
        }
    }
}