using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;


namespace ClickAndTravelMiddleOffice.Responses
{
    public class TuristContainer
    {
        public TuristContainer()
        { }

        public TuristContainer(JsonObject inp)
        {
            try
            {
                //  JsonArray  turistBd = inp["birth_date"] as JsonArray;    //дата рождения
                //  JsonArray  turistPd = inp["passport_date"] as JsonArray; //дата действия паспорта

                _birthDate = DateTime.ParseExact(inp["birth_date"].ToString(), "yyyy-MM-dd", null);
                _citizenship = inp["citizenship"].ToString();
                _firstName = inp["first_name"].ToString();
                _name = inp["last_name"].ToString();
                _passportNum = inp["passport_num"].ToString();
                _passportDate = DateTime.ParseExact(inp["passport_date"].ToString(), "yyyy-MM-dd", null);// new DateTime((int)turistPd[0], (int)turistPd[1], (int)turistPd[2]);

                try
                {
                    this._id = Convert.ToInt32(inp["id"]);
                    this._sex = Convert.ToInt32(inp["sex"]);
                }
                catch (Exception)
                { }
            }
            catch (Exception ex)
            {
                throw new Exception("Cann't convert " + inp + " to Turist object", ex);
            }
        }

        private int _id;
        [JsonMemberName("id")]
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _sex;
        [JsonMemberName("gender")]
        public int Sex
        {
            get { return _sex; }
            set { _sex = value; }
        }

        //имя туриста
        private string _name;
        [JsonMemberName("last_name")]
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        //фамилия туриста
        private string _firstName;
        [JsonMemberName("first_name")]
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        //дата рождения туриста
        private DateTime _birthDate;
        [JsonIgnore]
        public DateTime BirthDate
        {
            get { return _birthDate; }
            set { _birthDate = value; }
        }

        [JsonMemberName("birth_date")]
        public string BirthDateStr
        {
            get { return _birthDate.ToString("yyyy-MM-dd"); }
            set {  }
        }

        //код гражданства
        private string _citizenship;
        [JsonMemberName("citizenship")]
        public string Citizenship
        {
            get { return _citizenship; }
            set { _citizenship = value; }
        }

        //код паспорта
        private string _passportNum;
        [JsonMemberName("passport_num")]
        public string PassportNum
        {
            get { return _passportNum; }
            set { _passportNum = value; }
        }

        //дата действия паспорта
        private DateTime _passportDate;
        [JsonIgnore]
        public DateTime PassportDate
        {
            get { return _passportDate; }
            set { _passportDate = value; }
        }

        [JsonMemberName("passport_date")]
        public string PassportDateStr
        {
            get { return _passportDate.ToString("yyyy-MM-dd"); }
            set { }
        }
    }
}