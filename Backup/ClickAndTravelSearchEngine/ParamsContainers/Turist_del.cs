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
    public class Turist_del
    {
        public Turist_del()
        { }

        public Turist_del(JsonObject inp)
        { 
            try
            {
              //  JsonArray  turistBd = inp["birth_date"] as JsonArray;    //дата рождения
              //  JsonArray  turistPd = inp["passport_date"] as JsonArray; //дата действия паспорта

                JsonObject bonusCard = inp["bonus_card"] as JsonObject;

                _birthDate = DateTime.ParseExact(inp["birth_date"].ToString(), "yyyy-MM-dd", null);
                _bonusCard = new FlightBonusCard(bonusCard);
                _citizenship = inp["citizenship"].ToString();
                _firstName = inp["first_name"].ToString();
                _name = inp["last_name"].ToString();
                _passportNum = inp["passport_num"].ToString();
                _passportDate = DateTime.ParseExact(inp["passport_date"].ToString(), "yyyy-MM-dd", null);// new DateTime((int)turistPd[0], (int)turistPd[1], (int)turistPd[2]);
                
            }
            catch(Exception ex)
            {
                throw new Exception("Cann't convert " + inp + " to Turist object", ex);
            }
        }

        //имя туриста
        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        //фамилия туриста
        private string _firstName;

        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        //дата рождения туриста
        private DateTime _birthDate;

        public DateTime BirthDate
        {
            get { return _birthDate; }
            set { _birthDate = value; }
        }

        //код гражданства
        private string _citizenship;

        public string Citizenship
        {
            get { return _citizenship; }
            set { _citizenship = value; }
        }

        //код паспорта
        private string _passportNum;

        public string PassportNum
        {
            get { return _passportNum; }
            set { _passportNum = value; }
        }

        //дата действия паспорта
        private DateTime _passportDate;

        public DateTime PassportDate
        {
            get { return _passportDate; }
            set { _passportDate = value; }
        }

        //карта бонусных миль по а/к
        private FlightBonusCard _bonusCard;

        public FlightBonusCard BonusCard
        {
            get { return _bonusCard; }
            set { _bonusCard = value; }
        }
    }
}