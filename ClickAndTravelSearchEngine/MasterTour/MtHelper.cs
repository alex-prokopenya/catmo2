using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Megatec.MasterTour.BusinessRules;
using Megatec.MasterTour.DataAccess;

using System.Data.Sql;
using System.Data.SqlClient;
using ClickAndTravelMiddleOffice.Exceptions;
using ClickAndTravelMiddleOffice.Responses;
using ClickAndTravelMiddleOffice.Store;
using ClickAndTravelMiddleOffice.Helpers;
using ClickAndTravelMiddleOffice.ParamsContainers;
using ClickAndTravelMiddleOffice.Containers.Hotels;

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Jayrock.Json.Conversion;
using Jayrock.Json;


namespace ClickAndTravelMiddleOffice.MasterTour
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


    public class MtHelper
    {
        private static int serviceAnnulateStatusKey = 13; //ключ статуса аннулированной услуги
        private static int dogovorAnnulateStatusKey = 25; //ключ статуса аннулированной путевки
        private static string salt_key = "laypistrubezkoi"; //secretkey для подписи ссылки на документ

        //private static Dictionary<string, int> prefixToKey = new Dictionary<string, int>();


        public static KeyValuePair<string, decimal>[] GetCourses(string[] iso_codes, string base_rate, DateTime date)
        {
            //check redis cache
            string key_for_redis = "courses_" + base_rate + "b" + iso_codes.Aggregate((a, b) => a + "," + b) + "d" + date.ToString();

            //RedisHelper.SetString(key_for_redis,"");
            //return null;

            KeyValuePair<string, decimal>[] res = new KeyValuePair<string, decimal>[iso_codes.Length];

            string cache = RedisHelper.GetString(key_for_redis);

            if ((cache != null) && (cache.Length > 0))
            {
                try
                {
                    var pairs = cache.Split(';');

                    var kvps = pairs.Select<string, KeyValuePair<string, decimal>>(x =>
                    {
                        string[] arr = x.Split('=');
                        return new KeyValuePair<string, decimal>(arr[0], Convert.ToDecimal(arr[1]));
                    }).ToArray();

                    if (kvps.Length == res.Length)
                    {
                        Logger.WriteToLog("from redis");
                        return kvps;
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteToLog(ex.Message + ex.StackTrace);
                }
            }

            //get data from MasterTour
            int cnt = 0;
            foreach (string code in iso_codes)
            {
                if (code != base_rate)
                    res[cnt++] = new KeyValuePair<string, decimal>(code, getCourse(code, base_rate, date));
                else
                    res[cnt++] = new KeyValuePair<string, decimal>(code, 1M);
            }
            Logger.WriteToLog("from mt");
            //конвертируем массив в строку
            var str = res.Select(kvp => String.Format("{0}={1}", kvp.Key, kvp.Value));
            string value_for_redis = string.Join(";", str);
            //save_to redis
            RedisHelper.SetString(key_for_redis, value_for_redis);

            return res;
        }

        private static decimal getCourse(string rate1, string rate2, DateTime date)
        {
            RealCourses rcs = new RealCourses(new DataCache());

            string filter = String.Format("RC_RCOD2 = (select top 1  ra_code from click1.dbo.Rates where RA_ISOCode = '{0}') and " +
                                          "RC_RCOD1 = (select top 1  ra_code from click1.dbo.Rates where RA_ISOCode = '{1}') and " +
                                          "RC_DATEBEG <='{2}' and RC_DATEBEG > '{3}'", rate1, rate2, date.ToString("yyyy-MM-dd"), date.AddDays(-14).ToString("yyyy-MM-dd"));
            rcs.RowFilter = filter;

            Logger.WriteToLog(filter);
            rcs.Sort = "RC_DATEBEG desc";

            rcs.Fill();

            if (rcs.Count > 0)
                return Convert.ToDecimal(rcs[0].Course);

            else
            {
                rcs = new RealCourses(new DataCache());

                string filter2 = String.Format("RC_RCOD1 = (select top 1  ra_code from click1.dbo.Rates where RA_ISOCode = '{0}') and " +
                                                    "RC_RCOD2 = (select top 1  ra_code from click1.dbo.Rates where RA_ISOCode = '{1}') and " +
                                                    "RC_DATEBEG <='{2}' and RC_DATEBEG > '{3}'", rate1, rate2, date.ToString("yyyy-MM-dd"), date.AddDays(-14).ToString("yyyy-MM-dd"));
                rcs.RowFilter = filter2;
                Logger.WriteToLog(filter2);
                rcs.Sort = "RC_DATEBEG desc";

                rcs.Fill();
                if (rcs.Count > 0)
                    return  1 / Convert.ToDecimal(rcs[0].Course);

            }

            throw new CatmoException("Course not founded for date and rates "+ rate1+ " " + rate2 + " " + date.ToString());
        }

        public static Dogovor SaveNewDogovor(int[] bookIds, UserInfo userInfo)
        {
            Dogovors dogs = new Dogovors(new DataCache());
            Dogovor dog = dogs.NewRow();
            try
            {
                dog.CountryKey = 3325;
                dog.CityKey = 884;
                dog.TurDate = DateTime.Today.AddDays(380);
                dog.NDays = 1;
                dog.MainMenEMail = userInfo.Email;
                dog.MainMenPhone = userInfo.Phone;
                dog.TourKey = 3889;
                dog.PartnerKey = 0;
                dog.DupUserKey = userInfo.UserId;
                
                dog.CreatorKey = 100130;
                dog.OwnerKey = dog.CreatorKey;
                dog.RateCode = "рб";

                dogs.Add(dog);

                dogs.DataCache.Update();

                //получить услуги по бук айди
                List<TempService> tempServices = GetFlights(bookIds);
                tempServices.AddRange(GetHotels(bookIds));

                Logger.WriteToLog("founded services "+tempServices.Count);

              //  SaveNewServices(dog.DogovorLists, tempServices);
                    
                List<string> tempTuristsIds = new List<string>();

                foreach (TempService tfl in tempServices)
                    tempTuristsIds.AddRange(tfl.Turists);

                TempTurist[] tempTurists = GetTurists(tempTuristsIds.ToArray());

                Dictionary<int, List<string>> serviceToTuristLink = SaveNewServices(dog.DogovorLists, tempServices); //возвращает ссылки на туристов
                SaveNewTurists(dog, tempTurists, serviceToTuristLink);

                dog.CalculateCost();
                MyCalculateCost(dog);

                dog.NMen = (short)dog.Turists.Count;
                dog.DataCache.Update();

                SqlConnection conn = new SqlConnection(Manager.ConnectionString);
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                com.CommandText = "update tbl_dogovor set dg_creator=" + 100130 + ", dg_owner=" + 100130 + ", dg_filialkey = (select top 1 us_prkey from userlist where us_key = " + 100130 + ") where dg_code='" + dog.Code + "'";
                com.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                throw ex;
            }

            return dog;
		}

        public static void SaveNewTurists(Dogovor dogovor, TempTurist[] tsts, Dictionary<int, List<string>> serviceToTurist)
        {
            TuristServices tServices = new TuristServices(new DataCache());   //берем объект TuristServices
            foreach (TempTurist iTst in tsts) //если добавляются новые туристы
            {
                Turist tst = dogovor.Turists.NewRow();          // создаем новый объект "турист"
                tst.NameRus = iTst.Name;                        // проставляем имя
                tst.NameLat = iTst.Name;
                tst.FNameRus = iTst.FName;                      // проставляем Фамилию
                tst.FNameLat = iTst.FName;
                tst.SNameRus = "";                              // проставляем отчество
                tst.SNameLat = "";
                tst.Birthday = iTst.BirthDate;                  //дату рождения
                tst.CreatorKey = 100130;                        //создатель - Он-лайн

                tst.PasportDateEnd = iTst.PasspDate;

                tst.PasportNum = iTst.PasspNum.Substring(2);      //номер и ...
                tst.PasportType = iTst.PasspNum.Substring(0,2);     //... серия паспорта
                tst.DogovorCode = dogovor.Code;              //код путевки
                tst.DogovorKey = dogovor.Key;                //ключ путевки
                tst.PostIndex = iTst.Id;

                if (iTst.Citizen.Length > 2)
                    tst.Citizen = iTst.Citizen.Substring(2, 2);          //код гражданства туриста
                else
                    tst.Citizen = iTst.Citizen;

                if (iTst.Gender == 1)                  //пол туриста
                {
                    tst.RealSex = Turist.RealSex_Female;
                    if (tst.Age > 14)                        //ребенок или взрослый в зависимости от возраста
                        tst.Sex = Turist.Sex_Female;
                    else
                        tst.Sex = Turist.Sex_Child;
                }
                else
                {
                    tst.RealSex = Turist.RealSex_Male;
                    if (tst.Age > 14)
                        tst.Sex = Turist.Sex_Male;
                    else
                        tst.Sex = Turist.Sex_Child;
                }
                dogovor.Turists.Add(tst);                              //Добавляем к туристам в путевке 
                dogovor.Turists.DataCache.Update();                    //Сохраняем изменения

                foreach (DogovorList dl in dogovor.DogovorLists)       //Просматриваем услуги в путевке
                {
                   if((serviceToTurist.ContainsKey(dl.Key)) && (serviceToTurist[dl.Key].Contains(iTst.Id)))
                   {
                            dl.NMen += 1;                               //увеличиваем кол-во туристов на услуге
                            TuristService ts = tServices.NewRow();      //садим туриста на услугу
                            ts.Turist = tst;
                            ts.DogovorList = dl;
                            tServices.Add(ts);
                            tServices.DataCache.Update();          //сохраняем изменения
                   }
                }
                dogovor.DogovorLists.DataCache.Update();                //сохраняем изменения в услугах
            }

            if (tsts.Length == 0) // если просто нужно привязать старых туристов к новой услуге
            {
                dogovor.Turists.Fill();
                dogovor.DogovorLists.Fill();

                foreach (DogovorList dl in dogovor.DogovorLists)
                { 
                        if(serviceToTurist.ContainsKey(dl.Key))
                            foreach (Turist tst in dogovor.Turists)
                            {
                                if (serviceToTurist[dl.Key].Contains(tst.PostIndex))
                                {
                                    dl.NMen += 1;                               //увеличиваем кол-во туристов на услуге
                                    TuristService ts = tServices.NewRow();      //садим туриста на услугу
                                    ts.Turist = tst;
                                    ts.DogovorList = dl;
                                    tServices.Add(ts);
                                    tServices.DataCache.Update();
                                    dl.DataCache.Update();
                                }
                            }
                }
            }
        }

        public static Dictionary<int, List<string>> SaveNewServices(DogovorLists dls, List<TempService> services)
        {
            Dictionary<int, List<string>> result = new Dictionary<int, List<string>>();

            foreach (TempService srvc in services)                      //По одной создаем услуги
            {
                DogovorList dl = dls.NewRow();                    //создаем объект

                DateTime date = srvc.Date;
                if (dl.Dogovor.TurDate > date)		//корректируем даты тура в путевке
                {
                    dl.Dogovor.TurDate = date;
                    dl.Dogovor.DataCache.Update();
                }

                dl.NMen = 0;                                      //обнуляем кол-во туристов
               // dl.CountryKey = 0;
               // dl.CityKey = 0;
                dl.ServiceKey = srvc.ServiceClass;                             //ставим тип услуги

                dl.SubCode1 = 0;                                  //..привязываем к ислуге в справочнике
                dl.SubCode2 = 0;                                  //..
                dl.TurDate = dl.Dogovor.TurDate;//копируем дату тура
                dl.TourKey = dl.Dogovor.TourKey;                              //ключ тура
                dl.PacketKey = dl.Dogovor.TourKey;                            //пакет
                dl.CreatorKey = dl.Dogovor.CreatorKey;                        //копируем ключ создателя
                dl.OwnerKey = dl.Dogovor.OwnerKey;                            //копируем ключ создателя
                //dl.DateBegin = System.Convert.ToDateTime(srvc.date);     //ставим дату начала услуги

                dl.Name = srvc.Name;

                dl.Code = AddServiceToServiceList(srvc);

                dl.Comment = srvc.Id.ToString();

                dl.Brutto = srvc.Price;                     //ставим брутто

		        dl.FormulaBrutto = (("" + dl.Brutto).Contains(".") ||("" + dl.Brutto).Contains(",")  )? ("" + dl.Brutto).Replace(".",","): ("" + dl.Brutto) + ",00";  //копируем брутто в "formula"
				
                dl.CountryKey = dl.Dogovor.CountryKey;      //копируем страну
                dl.CityKey = dl.Dogovor.CityKey;            //копируем город
                    
                double netto = Math.Round(srvc.Price * 0.97); //расчет нетто
                dl.Netto = netto;
                dl.FormulaNetto = "" + netto + ",00";
				
                dl.BuildName();

                
                dl.PartnerKey = srvc.PartnerKey;                    //ставим поставщика услуги

                if (srvc.NDays > 0)
                {
                    dl.DateEnd = date.AddDays(srvc.NDays);
                    dl.NDays = Convert.ToInt16(srvc.NDays+1);
                    dl.Name += ", " + dl.NDays + " дней";
                }
                else
                    dl.DateEnd = date; //проставляем дату окончания услуги


                if (dl.DateEnd > dl.Dogovor.DateEnd)	//корректируем дату окончания тура в путевке
                {
                    dl.Dogovor.NDays += (short)(dl.DateEnd - dl.Dogovor.DateEnd).Days;
                    dl.Dogovor.DataCache.Update();
                }

                dl.Day = (short)((date - dl.Dogovor.TurDate).Days + 1);	    //порядковый день
                dl.DataCache.Update();                                 	    //сохраняем изменения
                dls.Add(dl);                                           	    //добавляем в набор услуг
                dls.DataCache.Update();


                List<string> tList = new List<string>();
                tList.AddRange(srvc.Turists);

                result.Add(dl.Key, tList);
            }

            foreach (DogovorList dl in dls)
            {
                dl.DateBegin = dl.Dogovor.TurDate.AddDays(dl.Day - 1);
                dl.DataCache.Update();
            }

            return result;
        }

        private static void MyCalculateCost(Dogovor dog)                             //Расчитываем стоимость
        {
            MyCalculateCost(dog, "");
        }

        private static void MyCalculateCost(Dogovor dog, string promo)                             //Расчитываем стоимость
        {
            int GUTA_PARTNER_KEY = 3680;
            bool have_ins = false;

            int promo_disc = 0;

            dog.DogovorLists.Fill();
            foreach (DogovorList dl in dog.DogovorLists)                      //По всем услугам в путевке
            {
                try
                {
                    have_ins = have_ins || ((dl.ServiceKey == Service.Insurance) && (dl.PartnerKey == GUTA_PARTNER_KEY));

                    if ((dl.FormulaBrutto != "") && (dl.FormulaBrutto.IndexOf(",") > 0))                                 //если брутто услуги 0
                    {
                        dl.Brutto = Math.Round(System.Convert.ToDouble(dl.FormulaBrutto) * (100 - promo_disc)) / 100;    //проставляем брутто из поля "Formula"

                       // dl.FormulaBrutto = dl.Brutto.ToString().Replace(".", ",");
                        dog.Price += dl.Brutto;
                        dl.DataCache.Update();                                    //сохраняем изменения
                    }

                    if ((dl.FormulaNetto != "") && (dl.FormulaNetto.IndexOf(",") > 0))                                 //если брутто услуги 0
                    {
                        //dog.Price -= dl.Brutto;                                 //корректируем общую стоимость
                        dl.Netto = System.Convert.ToDouble(dl.FormulaNetto);      //проставляем брутто из поля "Formula"
                        //dog.Price += dl.Brutto; 
                        dl.DataCache.Update();                                    //сохраняем изменения
                    }
                    dog.DataCache.Update();

                    have_ins = have_ins || ((dl.ServiceKey == 1118) && (dl.Brutto != Math.Round(dl.Brutto)));
                }
                catch (Exception ex)
                {
                    //throw new Exception(ex.Message);
                }
            }

            if (!have_ins)
                dog.Price = Math.Round(dog.Price);

            //ерунда какая-то если одна услуга экскурсия не проставляет ее стоимость
            if ((dog.Price == 0) && (dog.DogovorLists.Count == 1))
            {
                dog.Price = dog.DogovorLists[0].Brutto;
                dog.DataCache.Update();
            }
        }

        private static int AddServiceToServiceList(TempService srvc)
        {
            if (srvc.Name.Length > 50)
                srvc.Name = srvc.Name.Substring(0, 50);

            ServiceLists svs = new ServiceLists(new DataCache());
            ServiceList sv = svs.NewRow();
            sv.Name = srvc.Name;
            sv.NameLat = srvc.Id.ToString();
            sv.ServiceKey = srvc.ServiceClass;
            svs.Add(sv);
            svs.DataCache.Update();

            return sv.Key;
        }

        public static List<TempService> GetFlights(int[] bookIds)
        {
            try
            {
                var partnerKeys = new Dictionary<string,int>();
                partnerKeys.Add("aw_", 7965);
                partnerKeys.Add("pb_", 8797);
                partnerKeys.Add("vt_", 7993);

                string ids = string.Join(",", bookIds);

                //получить список сообщений
                SqlConnection con = new SqlConnection(Manager.ConnectionString);
                con.Open();

                SqlCommand com = new SqlCommand(String.Format("select book_id,[ft_id],[ft_ticketid],[ft_route],[ft_date],[ft_price],[ft_turists] from [CATSE_Flights], [CATSE_book_id] where [ft_id] = service_id and book_id in(" + ids + ") and service_type='CATSE_Flights'"), con);

                SqlDataReader reader = com.ExecuteReader();

                List<TempService> tempList = new List<TempService>();

                while (reader.Read())
                {
                    string bookTurists = reader["ft_turists"].ToString();

                    string route = Convert.ToString(reader["ft_route"]).Trim();

                    string ticketId = reader["ft_ticketid"].ToString().Trim();

                    if (ticketId.Contains("@@@"))
                        ticketId = ticketId.Substring(0, ticketId.IndexOf("@@@"));

                    tempList.Add(new TempService()
                    {
                        Date = Convert.ToDateTime(reader["ft_date"]),
                        Id = Convert.ToInt32(reader["book_id"]),
                        Name = ticketId + "/" + route,
                        Price = Convert.ToInt32(reader["ft_price"]),
                        Turists = bookTurists.Split(','),
                        NDays = 0,
                        PartnerKey = partnerKeys[reader["ft_ticketid"].ToString().Substring(0,3)], // подставить партнера
                        ServiceClass = 1118
                    });
                }
                reader.Close();
                con.Close();

                return tempList;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
        }

        public static List<TempService> GetHotels(int[] bookIds) //выгружаем отели
        {
            var prefixToKey = new Dictionary<string, int>();

            prefixToKey.Add("hb_", 8542 ); //указатели на поставщика
            prefixToKey.Add("ve_", 7993 );


            try
            {
                Logger.WriteToLog("in get hotels");

                string ids = string.Join(",", bookIds);

                //получить список сообщений
                SqlConnection con = new SqlConnection(Manager.ConnectionString);
                con.Open();

                SqlCommand com = new SqlCommand(String.Format("select book_id, ht_hash from [CATSE_hotels], [CATSE_book_id] where [ht_id] = service_id and book_id in(" + ids + ") and service_type='CATSE_hotels'"), con);

                SqlDataReader reader = com.ExecuteReader();

                List<TempService> tempList = new List<TempService>();


                while (reader.Read())
                {
                    string hash = reader["ht_hash"].ToString();
                    HotelBooking[] htlBooks = JsonConvert.Import<HotelBooking[]>(hash);

                    int bookId = Convert.ToInt32(reader["book_id"]);

                    foreach (HotelBooking hb in htlBooks)
                    {
                        Logger.WriteToLog( "бронь отеля " + hb.PartnerPrefix + hb.PartnerBookId);

                        tempList.Add(new TempService()
                        {
                            Date = Convert.ToDateTime(hb.DateBegin),
                            Name = hb.PartnerPrefix + hb.PartnerBookId + "/" + hb.SearchId,
                            Id = bookId,
                            NDays = hb.NightsCnt,
                            PartnerKey = prefixToKey[hb.PartnerPrefix],
                            Price = Convert.ToInt32( hb.Price["RUB"]),
                            ServiceClass = 1113,
                            Turists = string.Join(",", hb.Turists).Split(',')
                        });
                    }
                }

                return tempList;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
        }

        public static TempTurist[] GetTurists(string[] turistsIds)
        {
            try
            {
                string ids = "-1";
                foreach (string ft_id in turistsIds) ids += "," + ft_id;
                //получить список сообщений
                SqlConnection con = new SqlConnection(Manager.ConnectionString);
                con.Open();

                SqlCommand com = new SqlCommand(String.Format("select [ts_name] ,[ts_fname] ,[ts_gender] ,[ts_id] ,[ts_passport] ,[ts_passportdate] ,[ts_birthdate] ,[ts_citizenship] from [CATSE_Turists] where [ts_id] in ({0})", ids), con);

                SqlDataReader reader = com.ExecuteReader();

                List<TempTurist> tempList = new List<TempTurist>();

                while (reader.Read())
                {
                    tempList.Add(new TempTurist()
                    {
                        BirthDate = Convert.ToDateTime( reader["ts_birthdate"]),
                        Citizen = Convert.ToString(reader["ts_citizenship"]).Trim(),
                        FName = Convert.ToString(reader["ts_fname"]).Trim(),
                        Name = Convert.ToString(reader["ts_name"]).Trim(),
                        PasspDate = Convert.ToDateTime(reader["ts_passportdate"]),
                        PasspNum = Convert.ToString(reader["ts_passport"]).Trim(),
                        Id = Convert.ToString(reader["ts_id"]).Trim(),
                        Gender = Convert.ToInt32(reader["ts_gender"]),
                    });
                }
                reader.Close();
                con.Close();

                return tempList.ToArray();
             }
            catch (Exception ex)
            {

                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
        }

        public static int AddServiceToDogovor(Dogovor dogovor, int bookId)
        {
            try
            {
                dogovor.DogovorLists.Fill();
                dogovor.Turists.Fill();

               //проверяем, есть ли уже услуга в заказе
                if (ContainsBookId(dogovor.DogovorLists, bookId)) return 0;

                //получить услуги по бук айди
                List<TempService> tempServices = GetFlights(new int[] { bookId });  //пробуем найти авиабилет

                tempServices.AddRange( GetHotels(new int[] { bookId }));            //пробуем найти отель

                
                //собираем список с id новых туристов
                List<string> tempTuristsIds = new List<string>();

                foreach (TempService tfl in tempServices)
                {
                    for (int i = 0; i < tfl.Turists.Length; i++)                    //есть ли новый для брони турист 
                        if (!ContainsTuristId(dogovor.Turists, tfl.Turists[i])) tempTuristsIds.Add(tfl.Turists[i]);// = "-1"; 

                    //tempTuristsIds.AddRange(tfl.Turists);
                }

                TempTurist[] tempTurists = GetTurists(tempTuristsIds.ToArray());

                Dictionary<int, List<string>> serviceToTurist =  SaveNewServices(dogovor.DogovorLists, tempServices);

                SaveNewTurists(dogovor, tempTurists, serviceToTurist);

                dogovor.CalculateCost();
                MyCalculateCost(dogovor);

                dogovor.NMen = (short)dogovor.Turists.Count;
                dogovor.DataCache.Update();
                return 1;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
            }

            return 0;
        }

        private static bool ContainsBookId(DogovorLists dls, int bookId)
        {
            foreach (DogovorList dl in dls)
            { 
                try
                {
                    if (Convert.ToInt32(dl.Comment) == bookId)  return true;
                }
                catch(Exception){}

            }
            return false;
        }

        private static bool ContainsTuristId(Turists tsts, string turistId)
        {
            foreach (Turist tst in tsts)
            {
                try
                {
                    if ((tst.PostIndex) == turistId) return true;
                }
                catch (Exception) { }

            }
            return false;
        }

        //изменяет статус услуги на "запрос на аннуляцию"
        public static void AnnulateService(string dogovorCode, int bookId)
        {
            DogovorLists dls = new DogovorLists(new DataCache());
            dls.RowFilter = "dl_dgcod='" + dogovorCode + "' and DL_comment='" + bookId+"'";

            dls.Fill();

            if (dls.Count > 0)
            {
                dls[0].SetStatus(serviceAnnulateStatusKey);
                dls[0].DataCache.Update();
            }
            else
                throw new CatmoException("service not found", ErrorCodes.ServiceNotFound);
        }

        //изменяет статус путевки на "запрос на аннуляцию"
        public static void AnnulateDogovor(string dogovorCode)
        {
            Dogovor dog = GetDogovor(dogovorCode);
            dog.SetStatus(dogovorAnnulateStatusKey);
            dog.DataCache.Update();
        }

        //забирает путевки созданные пользователем
        public static Dogovors GetUserDogovors(string mail, int userkey)
        {
            Dogovors dogs = new Dogovors(new DataCache());

            string filter = ""; //создаем пустой фильтр

            if (mail.Length > 0) //если пришел и-мэйл, добавим его к фильтру
                filter = " (DG_MAINMENEMAIL like '" + mail + "') ";

            if (userkey > 0) //если пришел юзер-айди, добавим его к фильтру
            {
                filter = filter.Length > 0 ? filter + " or " : ""; // если в фильтре уже есть условие добавим "или"

                filter += "(dg_dupuserkey = " + userkey + ")";
            }

            dogs.RowFilter = filter;   //применим фильтр
            dogs.Fill();               //комит

            return dogs;    //отдаем найденные путевки
        }

        //получить путевку
        public static Dogovor GetDogovor(string dg_code)
        {
          //  dg_code = "TPA3103012";

            Dogovors dogs = new Dogovors(new DataCache()); 
            dogs.RowFilter = "dg_code = '"+dg_code+"'"; //ищем закакз по номеру

            dogs.Fill(); //комит

            if (dogs.Count > 0) //если заказ нашелся, отдаем его
                return dogs[0];
            else                //иначе
                throw new CatmoException("dogovor not found", ErrorCodes.DogovorNotFound); //генерируем эксепшн
        }

        //получить документы прикрепленные к путевке
        public static Document[] GetDogovorDocuments(string dg_code)
        {
            Dogovor dog = GetDogovor(dg_code);

            string url_prefix = "http://online.clickandtravel.ru/click/extra.aspx?dogovor={0}&k={1}&h={2}";
            //получить список документов
            SqlConnection con = new SqlConnection(Manager.ConnectionString);
            con.Open();
            SqlCommand com = new SqlCommand(String.Format("select doc_key, doc_title, isnull(doc_updated, '2000-01-01') as datetime from [dogovor_documents] where doc_dogovorkey = '{0}'", dog.Key), con);

            SqlDataReader reader = com.ExecuteReader();

            List<Document> tempList = new List<Document>();

            string hash = CalculateMD5Hash( dog.Code +  "extra" + salt_key );

            while (reader.Read())
            {
                tempList.Add(new Document() { Title = reader["doc_title"].ToString(),
                                              Updated = Convert.ToDateTime(reader["datetime"]),
                                              Url = string.Format(url_prefix, dog.Code, reader["doc_key"].ToString(), hash),
                });
            }
            reader.Close();
            con.Close();

            return tempList.ToArray();
        }


        //получить сообщения прикрепленные к путевке
        public static Responses.DogovorMessage[] GetDogovorMessages(string dg_code, DateTime minDate)
        {
            //получить список сообщений
            SqlConnection con = new SqlConnection(Manager.ConnectionString);
            con.Open();

            SqlCommand com = new SqlCommand(String.Format("select [DM_Type], [DM_Date], [DM_Text] from [DogovorMessages] where [DM_DGCODE] = '{0}' and DM_Date > '{1}'", dg_code, minDate.ToString("yyyy-MM-dd HH:mm:ss")), con);

            SqlDataReader reader = com.ExecuteReader();

            List<Responses.DogovorMessage> tempList = new List<Responses.DogovorMessage>();

            while (reader.Read())
            {
                tempList.Add(new Responses.DogovorMessage()
                {
                    Date = Convert.ToDateTime(reader["DM_Date"]), 
                    InOut = Convert.ToInt32(reader["DM_Type"]),
                    Text = reader["DM_Text"].ToString()
                });
            }
            reader.Close();
            con.Close();

            return tempList.ToArray();
        }

        public static int SaveDogovorMessage(string code, string text)
        {
            Dogovor dog = GetDogovor(code);

            SqlConnection con = new SqlConnection(Manager.ConnectionString);
            con.Open();

            SqlCommand com = new SqlCommand(String.Format("insert into [DogovorMessages](DM_Type, [DM_DGKey], [DM_DGCODE], DM_Text) values ({0},{1},'{2}','{3}')", 1, dog.Key, dog.Code, AntiInject(text)), con);

            try
            {
                com.ExecuteNonQuery();
            }
            catch (Exception)
            {
                con.Close();
                return 0;
            }

            con.Close();
            return 1;
        }

        private static string AntiInject(string inp)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            pairs.Add("'", "`");
            pairs.Add("--", "- -");
            pairs.Add(" drop ", " dr op ");
            pairs.Add("insert", "inse rt");
            pairs.Add("select", "sel ect");
            pairs.Add("delete", "dele te");
            pairs.Add("update", "up date");

            foreach (string key in pairs.Keys)
                inp = Regex.Replace(inp, key, pairs[key], RegexOptions.IgnoreCase);

            return inp;
        }

        private static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }
    }
}