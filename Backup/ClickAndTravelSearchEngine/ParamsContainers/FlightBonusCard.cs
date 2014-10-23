using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;

namespace ClickAndTravelMiddleOffice.ParamsContainers
{
    public class FlightBonusCard
    {

        public FlightBonusCard()
        { }

        public FlightBonusCard(JsonObject inp)
        {
            try
            {
                _airlineCode = inp["airline_code"].ToString();
                _cardNumber = inp["card_number"].ToString();
            }
            catch (Exception)
            { }
        }

        //код авиакомпании
        private string _airlineCode;

        public string AirlineCode
        {
            get { return _airlineCode; }
            set { _airlineCode = value; }
        }

        //номер карты
        private string _cardNumber;

        public string CardNumber
        {
            get { return _cardNumber; }
            set { _cardNumber = value; }
        }
    }
}