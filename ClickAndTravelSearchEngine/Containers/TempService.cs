using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickAndTravelMiddleOffice.Containers
{
    public class TempService
    {
        private int id;

        public int Id //book_id для связи с фронтом
        {
            get { return id; }
            set { id = value; }
        }

        private int serviceClass;

        public int ServiceClass // тип услуги (отель, билет....)
        {
            get { return serviceClass; }
            set { serviceClass = value; }
        }

        private int price;

        public int Price //стоимость ...
        {
            get { return price; }
            set { price = value; }
        }
        private string name;

        public string Name //название...
        {
            get { return name; }
            set { name = value; }
        }

        private string[] turists;

        public string[] Turists //список туристов...
        {
            get { return turists; }
            set { turists = value; }
        }

        private DateTime date;

        public DateTime Date //дата начала ...
        {
            get { return date; }
            set { date = value; }
        }

        private int nDays;

        public int NDays//продолжительность ...
        {
            get { return nDays; }
            set { nDays = value; }
        }

        private int partnerKey;

        public int PartnerKey //ссылка на партнера
        {
            get { return partnerKey; }
            set { partnerKey = value; }
        }
    }
}