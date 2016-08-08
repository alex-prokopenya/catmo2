using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;
using ClickAndTravelMiddleOffice.MasterTour;


namespace ClickAndTravelMiddleOffice.ParamsContainers
{
    public class UserInfo
    {
        public UserInfo()
        { }

        public UserInfo(JsonObject inp)
        {
            try
            {
                this._email = inp["email"].ToString();
                this._phone = inp["phone"].ToString();

                if (inp.Contains("agent_login"))
                    this._userLogin = MtHelper.PrepareLogin(inp["agent_login"].ToString());
                else
                    this._userLogin = "";

                this._userId = Convert.ToInt32(inp["id"]);

            }
            catch (Exception ex)
            {
                throw new Exception( "Cannot convert " + inp.ToString() + " to UserInfo object", ex);
            }
        }

        private string _phone;

        public string Phone
        {
            get { return _phone; }
            set { _phone = value; }
        }

        private string _email;

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        private int _userId;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        private string _userLogin;

        public string UserLogin
        {
            get { return _userLogin; }
            set { _userLogin = value; }
        }
    }
}