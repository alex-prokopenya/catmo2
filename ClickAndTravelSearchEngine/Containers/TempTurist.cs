using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelMiddleOffice.Containers
{
    public class TempTurist
    {
        private int gender;

        public int Gender
        {
            get { return gender; }
            set { gender = value; }
        }

        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string fName;

        public string FName
        {
            get { return fName; }
            set { fName = value; }
        }
        private string citizen;

        public string Citizen
        {
            get { return citizen; }
            set { citizen = value; }
        }
        private DateTime birthDate;

        public DateTime BirthDate
        {
            get { return birthDate; }
            set { birthDate = value; }
        }
        private DateTime passpDate;

        public DateTime PasspDate
        {
            get { return passpDate; }
            set { passpDate = value; }
        }
        private string passpNum;

        public string PasspNum
        {
            get { return passpNum; }
            set { passpNum = value; }
        }
    }

}