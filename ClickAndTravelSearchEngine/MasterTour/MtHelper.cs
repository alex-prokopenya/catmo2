using ClickAndTravelMiddleOffice.Containers.Excursions;
using ClickAndTravelMiddleOffice.Containers.Hotels;
using ClickAndTravelMiddleOffice.Containers.Transfers;
using ClickAndTravelMiddleOffice.Containers;
using ClickAndTravelMiddleOffice.Exceptions;
using ClickAndTravelMiddleOffice.Helpers;
using ClickAndTravelMiddleOffice.ParamsContainers;
using ClickAndTravelMiddleOffice.Responses;
using ClickAndTravelMiddleOffice.Store;
using ClickAndTravelMiddleOffice.DB;

using Jayrock.Json.Conversion;

using Megatec.Common.BusinessRules.Base;
using Megatec.MasterTour.BusinessRules;
using Megatec.MasterTour.DataAccess;

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


namespace ClickAndTravelMiddleOffice.MasterTour
{
   
    public class MtHelper
    {
        private static int serviceAnnulateStatusKey = 13; //ключ статуса аннулированной услуги
        private static int dogovorAnnulateStatusKey = 25; //ключ статуса аннулированной путевки
        private static string saltKey = "laypistrubezkoi"; //secretkey для подписи ссылки на документ

        private static int SERVICE_EXCURSION    = 1141;
        private static int SERVICE_TRANSFER     = 1142;
        private static int SERVICE_FLIGHT       = 1144;
        private static int SERVICE_HOTEL        = 1143;

        private static int PARTNER_WEATLAS      = 10923;
        private static int PARTNER_AWAD         = 7965;
        private static int PARTNER_TICKETS      = 10922;
        private static int PARTNER_PORTBILET    = 10921;
        private static int PARTNER_VIZIT        = 7993;
        private static int PARTNER_IWAY         = 9894;
        private static int PARTNER_HOTELBOOK    = 9560;
        private static int PARTNER_OSTROVOK     = 10920;
        private static int PARTNER_GUTA         = 3680;

        private static int COUNTRY_CLICK        = 3332;
        private static int CITY_CLICK           = 1010;
        private static int TOUR_CLICK           = 5328;

        private static int DOGOVOR_CREATOR = 100130;

        public static void SaveServiceDetailsJson(ServiceJson service)// bookId, string serviceType, string jsonCode)
        {
            SaveServicesDetailsJson(new List<ServiceJson>() { service });
        }

        public static void SaveServicesDetailsJson(List<ServiceJson> services)// bookId, string serviceType, string jsonCode)
        {
            SqlConnection con = new SqlConnection(Manager.ConnectionString);
            con.Open();

            var comandText = "";
            var findBookIdSubquery = "(select service_id from CATSE_book_id where book_id = @bookid)";

            foreach (ServiceJson sj in services)
            {
                switch (sj.type)
                {
                    case "flight":
                        comandText = "update CATSE_Flights set ft_service_details = @json where ft_id = " + findBookIdSubquery;
                        break;

                    case "hotel":
                        comandText = "update CATSE_hotels set ht_service_details = @json where ht_id = " + findBookIdSubquery;
                        break;

                    case "excursion":
                        comandText = "update CATSE_excursions set ex_service_details = @json where ex_id = " + findBookIdSubquery;
                        break;

                    case "transfer":
                        comandText = "update CATSE_transfers set tr_service_details = @json where tr_id = " + findBookIdSubquery;
                        break;

                    default: continue;
                }

                SqlCommand com = new SqlCommand(comandText, con);
                com.Parameters.Add(new SqlParameter("bookid", sj.id));
                com.Parameters.Add(new SqlParameter("json", sj.json));
                com.ExecuteNonQuery();
            }
            con.Close();
        }

        public static string PrepareLogin(string input)
        {
            return input;//.Replace("@agent.click", "");
        }

        public static KeyValuePair<string, decimal>[] GetCourses(string[] iso_codes, string base_rate, DateTime date)
        {
            string key_for_redis = "courses_" + base_rate + "b" + iso_codes.Aggregate((a, b) => a + "," + b) + "d" + date.ToString();

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
                        return kvps;
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
            //конвертируем массив в строку
            var str = res.Select(kvp => String.Format("{0}={1}", kvp.Key, kvp.Value));
            string value_for_redis = string.Join(";", str);
            //save_to redis
            RedisHelper.SetString(key_for_redis, value_for_redis);

            return res;
        }

        private static decimal getCourse(string rate1, string rate2, DateTime date)
        {
            RealCourses rcs = new RealCourses(new DataContainer());

            string filter = String.Format("RC_RCOD2 = (select top 1  ra_code from click2009.dbo.Rates where RA_ISOCode = '{0}') and " +
                                          "RC_RCOD1 = (select top 1  ra_code from click2009.dbo.Rates where RA_ISOCode = '{1}') and " +
                                          "RC_DATEBEG <='{2}' and RC_DATEBEG > '{3}'", rate1, rate2, date.ToString("yyyy-MM-dd"), date.AddDays(-14).ToString("yyyy-MM-dd"));
            rcs.RowFilter = filter;

            Logger.WriteToLog(filter);
            rcs.Sort = "RC_DATEBEG desc";

            rcs.Fill();

            if (rcs.Count > 0)
                return Convert.ToDecimal(rcs[0].Course);

            else
            {
                rcs = new RealCourses(new DataContainer());

                string filter2 = String.Format("RC_RCOD1 = (select top 1  ra_code from click2009.dbo.Rates where RA_ISOCode = '{0}') and " +
                                                    "RC_RCOD2 = (select top 1  ra_code from click2009.dbo.Rates where RA_ISOCode = '{1}') and " +
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
            Dogovors dogs = new Dogovors(new DataContainer());
            Dogovor dog = dogs.NewRow();
            try
            {
                dog.CountryKey = COUNTRY_CLICK;
                dog.CityKey = CITY_CLICK;
                dog.TurDate = DateTime.Today.AddDays(380);
                dog.NDays = 1;
                dog.MainMenEMail = userInfo.Email;
                dog.MainMenPhone = userInfo.Phone;
                dog.TourKey = TOUR_CLICK;

                if (userInfo.UserLogin !="")
                {
                    DupUsers dups = new DupUsers(new Megatec.Common.BusinessRules.Base.DataContainer());
                    dups.RowFilter = "us_id='" + AntiInject(userInfo.UserLogin) + "'";
                    dups.Fill();

                    if (dups.Count>0)
                    {
                        dog.PartnerKey = dups[0].PartnerKey;  //!!!!покупатель
                        dog.DupUserKey = dups[0].Key;  //!!!!
                    }
                }
                else
                {
                    dog.PartnerKey = 0;
                    dog.DupUserKey = userInfo.UserId;
                }

                dog.CreatorKey = DOGOVOR_CREATOR;
                dog.OwnerKey = dog.CreatorKey;
                dog.RateCode = "рб";
                dog.PaymentDate = DateTime.Now.AddMinutes(45);
                dogs.Add(dog);

                dogs.DataContainer.Update();

                List<TempService> tempServices = GetBookedSrvices(bookIds);

                var serviceNames = MySqlDataProvider.GetServicesDetails(bookIds.ToList(), true);

                for (int i = 0; i < tempServices.Count; i++)
                {
                    if (serviceNames.ContainsKey(tempServices[i].Id))
                        tempServices[i].Name = serviceNames[tempServices[i].Id];
                    else
                        tempServices[i].Name = "not found details";
                }

                Logger.WriteToLog("founded services "+tempServices.Count);
                

                List<string> tempTuristsIds = new List<string>();

                foreach (TempService tfl in tempServices)
                    tempTuristsIds.AddRange(tfl.Turists);

                TempTurist[] tempTurists = GetTurists(tempTuristsIds.ToArray());

                Dictionary<int, List<string>> serviceToTuristLink = SaveNewServices(dog.DogovorLists, tempServices); //возвращает ссылки на туристов
                SaveNewTurists(dog, tempTurists, serviceToTuristLink);

                dog.CalculateCost();
                MyCalculateCost(dog);

                dog.NMen = (short)dog.Turists.Count;
                dog.DataContainer.Update();

                SqlConnection conn = new SqlConnection(Manager.ConnectionString);
                conn.Open();
                SqlCommand com = conn.CreateCommand();
                com.CommandText = "update tbl_dogovor set dg_creator=" + DOGOVOR_CREATOR + ", dg_owner=" + DOGOVOR_CREATOR + ", dg_filialkey = (select top 1 us_prkey from userlist where us_key = " + DOGOVOR_CREATOR + ") where dg_code='" + dog.Code + "'";
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

        private static List<TempService> GetBookedSrvices(int[] bookIds)
        {
            //получить услуги по бук айди
            List<TempService> tempServices = GetFlights(bookIds);
            tempServices.AddRange(GetHotels(bookIds));
            tempServices.AddRange(GetExcursions(bookIds));
            tempServices.AddRange(GetTransfers(bookIds));
            return tempServices;
        }

        public static void SaveNewTurists(Dogovor dogovor, TempTurist[] tsts, Dictionary<int, List<string>> serviceToTurist)
        {
            TuristServices tServices = new TuristServices(new DataContainer());   //берем объект TuristServices
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
                tst.CreatorKey = DOGOVOR_CREATOR;                        //создатель - Он-лайн

                tst.PasportDateEnd = iTst.PasspDate;

                tst.PasportNum = iTst.PasspNum.Substring(2);      //номер и ...
                tst.PasportType = iTst.PasspNum.Substring(0, 2);     //... серия паспорта
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
                dogovor.Turists.DataContainer.Update();                    //Сохраняем изменения

                foreach (DogovorList dl in dogovor.DogovorLists)       //Просматриваем услуги в путевке
                {
                   if((serviceToTurist.ContainsKey(dl.Key)) && (serviceToTurist[dl.Key].Contains(iTst.Id)))
                   {
                            dl.NMen += 1;                               //увеличиваем кол-во туристов на услуге
                            TuristService ts = tServices.NewRow();      //садим туриста на услугу
                            ts.Turist = tst;
                            ts.DogovorList = dl;
                            tServices.Add(ts);
                            tServices.DataContainer.Update();          //сохраняем изменения
                   }
                }
                dogovor.DogovorLists.DataContainer.Update();                //сохраняем изменения в услугах
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
                                    tServices.DataContainer.Update();
                                    dl.DataContainer.Update();
                                }
                            }
                }
            }
        }

        private static double CalcNetto(double brutto, int serviceKey, int partnerKey)
        {
            if ((serviceKey == SERVICE_EXCURSION) && (partnerKey == PARTNER_WEATLAS))
                return Math.Round(brutto * 0.9);

            return Math.Round(brutto * 0.97);
        }

        public static Dictionary<int, List<string>> SaveNewServices(DogovorLists dls, List<TempService> services)
        {
            Dictionary<int, List<string>> result = new Dictionary<int, List<string>>();

            foreach (TempService srvc in services)    //По одной создаем услуги
            {
                DogovorList dl = dls.NewRow();        //создаем объект

                DateTime date = srvc.Date;

                if (dl.Dogovor.TurDate > date)		//корректируем даты тура в путевке
                {
                    dl.Dogovor.TurDate = date;
                    dl.Dogovor.DataContainer.Update();
                }

                dl.NMen = 0;                                      //обнуляем кол-во туристов
                dl.ServiceKey = srvc.ServiceClass;                             //ставим тип услуги

                dl.SubCode1 = 0;                                  //..привязываем к ислуге в справочнике
                dl.SubCode2 = 0;                                  //..
                dl.TurDate = dl.Dogovor.TurDate;//копируем дату тура
                dl.TourKey = dl.Dogovor.TourKey;                              //ключ тура
                dl.PacketKey = dl.Dogovor.TourKey;                            //пакет
                dl.CreatorKey = dl.Dogovor.CreatorKey;                        //копируем ключ создателя
                dl.OwnerKey = dl.Dogovor.OwnerKey;                            //копируем ключ создателя
                dl.Name = srvc.Name;
                dl.Code = AddServiceToServiceList(srvc);
                dl.Comment = srvc.Id.ToString();
                dl.Brutto = srvc.Price;                     //ставим брутто

		        dl.FormulaBrutto = (("" + dl.Brutto).Contains(".") ||("" + dl.Brutto).Contains(",")  )? ("" + dl.Brutto).Replace(".",","): ("" + dl.Brutto) + ",00";  //копируем брутто в "formula"
				
                dl.CountryKey = dl.Dogovor.CountryKey;      //копируем страну
                dl.CityKey = dl.Dogovor.CityKey;            //копируем город
                    
                double netto = CalcNetto(dl.Brutto, dl.ServiceKey, srvc.PartnerKey); //расчет нетто //
                dl.Netto = netto;
                dl.FormulaNetto = "" + netto + ",00";
				
                dl.BuildName();

                #region rebuildname
                var parts = dl.Name.Split('/');
                parts[1] = srvc.Name;

                var newName = string.Join("/", parts);

                if (newName.Length > 255)
                    newName = newName.Substring(255);

                dl.Name = newName;
                #endregion

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
                    dl.Dogovor.DataContainer.Update();
                }

                dl.Day = (short)((date - dl.Dogovor.TurDate).Days + 1);	    //порядковый день
                dl.DataContainer.Update();                                 	    //сохраняем изменения
                dls.Add(dl);                                           	    //добавляем в набор услуг
                dls.DataContainer.Update();

                List<string> tList = new List<string>();
                tList.AddRange(srvc.Turists);

                result.Add(dl.Key, tList);
            }

            foreach (DogovorList dl in dls)
            {
                dl.DateBegin = dl.Dogovor.TurDate.AddDays(dl.Day - 1);
                dl.DataContainer.Update();
            }

            return result;
        }

        private static void MyCalculateCost(Dogovor dog)                             //Расчитываем стоимость
        {
            MyCalculateCost(dog, "");
        }

        private static void MyCalculateCost(Dogovor dog, string promo)                             //Расчитываем стоимость
        {
            bool have_ins = false;

            int promo_disc = 0;

            dog.DogovorLists.Fill();
            foreach (DogovorList dl in dog.DogovorLists)                      //По всем услугам в путевке
            {
                try
                {
                    have_ins = have_ins || ((dl.ServiceKey == Service.Insurance) && (dl.PartnerKey == PARTNER_GUTA));

                    if ((dl.FormulaBrutto != "") && (dl.FormulaBrutto.IndexOf(",") > 0))                                 //если брутто услуги 0
                    {
                        dl.Brutto = Math.Round(System.Convert.ToDouble(dl.FormulaBrutto) * (100 - promo_disc)) / 100;    //проставляем брутто из поля "Formula"

                        dog.Price += dl.Brutto;
                        dl.DataContainer.Update();                                    //сохраняем изменения
                    }

                    if ((dl.FormulaNetto != "") && (dl.FormulaNetto.IndexOf(",") > 0))                                 //если брутто услуги 0
                    {
                        //dog.Price -= dl.Brutto;                                 //корректируем общую стоимость
                        dl.Netto = System.Convert.ToDouble(dl.FormulaNetto);      //проставляем брутто из поля "Formula"
                        //dog.Price += dl.Brutto; 
                        dl.DataContainer.Update();                                    //сохраняем изменения
                    }
                    dog.DataContainer.Update();

                    have_ins = have_ins || ((dl.ServiceKey == SERVICE_FLIGHT) && (dl.Brutto != Math.Round(dl.Brutto)));
                }
                catch (Exception ex)
                {
                    Logger.WriteToLog(ex.Message + "\n" + ex.StackTrace);
                }
            }

            if (!have_ins)
                dog.Price = Math.Round(dog.Price);

            //ерунда какая-то если одна услуга экскурсия не проставляет ее стоимость
            if ((dog.Price == 0) && (dog.DogovorLists.Count == 1))
            {
                dog.Price = dog.DogovorLists[0].Brutto;
                dog.DataContainer.Update();
            }
        }

        private static int AddServiceToServiceList(TempService srvc)
        {
            var name = srvc.Name;

            if (name.Length > 60)
                name = srvc.Name.Substring(0, 60);

            ServiceLists svs = new ServiceLists(new DataContainer());
            ServiceList sv = svs.NewRow();
            sv.Name = name;
            sv.NameLat = srvc.Id.ToString();
            sv.ServiceKey = srvc.ServiceClass;
            svs.Add(sv);
            svs.DataContainer.Update();

            return sv.Key;
        }

        public static List<TempService> GetFlights(int[] bookIds)
        {
            try
            {
                var partnerKeys = new Dictionary<string,int>();
                partnerKeys.Add("aw_", PARTNER_AWAD);
                partnerKeys.Add("tk_", PARTNER_TICKETS);
                partnerKeys.Add("pb_", PARTNER_PORTBILET);
                partnerKeys.Add("vt_", PARTNER_VIZIT);

                string ids = string.Join(",", bookIds);

                //получить список сообщений
                SqlConnection con = new SqlConnection(Manager.ConnectionString);
                con.Open();

                SqlCommand com = new SqlCommand(String.Format("select book_id,[ft_id],[ft_ticketid],[ft_route],[ft_date],[ft_price],[ft_turists],[ft_lastdate] "+
                                                            " from [CATSE_Flights], [CATSE_book_id] "+
                                                            " where [ft_id] = service_id and book_id in(" + ids + ") and service_type='CATSE_Flights'"), con);

                SqlDataReader reader = com.ExecuteReader();

                List<TempService> tempList = new List<TempService>();

                while (reader.Read())
                {
                    string bookTurists = reader["ft_turists"].ToString();

                    string route = Convert.ToString(reader["ft_route"]).Trim();

                    string ticketId = reader["ft_ticketid"].ToString().Trim();

                    if (ticketId.Contains("@@@"))
                        ticketId = ticketId.Substring(0, ticketId.IndexOf("@@@"));


                    var firstdate = Convert.ToDateTime(reader["ft_date"]);
                    var lastdate = Convert.ToDateTime(reader["ft_lastdate"]);

                    tempList.Add(new TempService()
                    {
                        Date = firstdate,
                        Id = Convert.ToInt32(reader["book_id"]),
                        Name = ticketId + "/" + route,
                        Price = Convert.ToInt32(reader["ft_price"]),
                        Turists = bookTurists.Split(','),
                        NDays = (lastdate - firstdate).Days,//продолжительность перелетов в ночах
                        PartnerKey = partnerKeys[reader["ft_ticketid"].ToString().Substring(0,3)], // подставить партнера
                        ServiceClass = SERVICE_FLIGHT
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

            prefixToKey.Add("hb_", PARTNER_HOTELBOOK ); //указатели на поставщика
            prefixToKey.Add("ve_", PARTNER_VIZIT );
            prefixToKey.Add("os_", PARTNER_OSTROVOK);

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
                            ServiceClass = SERVICE_HOTEL,
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

        private static int GetPartnerByExcursion(int excId)
        {
            //по id экскурсии определяем партнера
            if (excId > 10000000) //у экскурсий c таким id партнер WeAtlas
                return PARTNER_WEATLAS;
            else
                return PARTNER_VIZIT;
        }

        public static List<TempService> GetExcursions(int[] bookIds) //выгружаем отели
        {
            var prefixToKey = new Dictionary<string, int>();

            try
            {
                Logger.WriteToLog("in get exc");

                string ids = string.Join(",", bookIds);

                //получить список сообщений
                SqlConnection con = new SqlConnection(Manager.ConnectionString);
                con.Open();

                SqlCommand com = new SqlCommand(String.Format("select book_id, ex_hash from [CATSE_excursions], [CATSE_book_id] where [ex_id] = service_id and book_id in(" + ids + ") and service_type='CATSE_excursions'"), con);

                SqlDataReader reader = com.ExecuteReader();

                List<TempService> tempList = new List<TempService>();

                while (reader.Read())
                {
                    string hash = reader["ex_hash"].ToString();
                    ExcursionBooking exBook = JsonConvert.Import<ExcursionBooking>(hash);

                    int partnerKey = GetPartnerByExcursion(exBook.ExcVariant.Id);

                    int bookId = Convert.ToInt32(reader["book_id"]);

                    Logger.WriteToLog("бронь экскурсии " + exBook.ExcVariant.Id);

                    tempList.Add(new TempService()
                    {
                        Date = Convert.ToDateTime(exBook.SelectedDate),
                        Name = "Экскурсия/" + exBook.SearchId + "/" + exBook.ExcVariant.Id,
                        Id = bookId,
                        NDays = 0,
                        PartnerKey = partnerKey,
                        Price = Convert.ToInt32(exBook.ExcVariant.Price["RUB"]),
                        ServiceClass = SERVICE_EXCURSION,
                        Turists = string.Join(",", exBook.Turists).Split(',')
                    });
                }

                return tempList;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.StackTrace);
                throw ex;
            }
        }

        public static List<TempService> GetTransfers(int[] bookIds) //выгружаем отели
        {
            var prefixToKey = new Dictionary<string, int>();

            try
            {
                Logger.WriteToLog("in get trf");

                string ids = string.Join(",", bookIds);

                //получить список сообщений
                SqlConnection con = new SqlConnection(Manager.ConnectionString);
                con.Open();

                SqlCommand com = new SqlCommand(String.Format("select book_id, tr_hash from [CATSE_transfers], [CATSE_book_id] where [tr_id] = service_id and book_id in(" + ids + ") and service_type='CATSE_transfers'"), con);

                SqlDataReader reader = com.ExecuteReader();

                List<TempService> tempList = new List<TempService>();

                while (reader.Read())
                {
                    string hash = reader["tr_hash"].ToString();
                    var trBook = JsonConvert.Import<TransferBooking>(hash);

                    int bookId = Convert.ToInt32(reader["book_id"]);

                    Logger.WriteToLog("бронь трансфера " + trBook.TransactionId);

                    tempList.Add(new TempService()
                    {
                        Date = Convert.ToDateTime(trBook.StartDate),
                        Name = "Трансфер/" + trBook.SearchId + "/" + trBook.TransactionId,
                        Id = bookId,
                        NDays = (Convert.ToDateTime(trBook.EndDate) - Convert.ToDateTime( trBook.StartDate )).Days,
                        PartnerKey = PARTNER_IWAY, //проставить IWAY
                        Price = Convert.ToInt32(trBook.TransferVariant.Price["RUB"]),
                        ServiceClass = SERVICE_TRANSFER, //проставить ТРАНСФЕР
                        Turists = string.Join(",", trBook.Turists).Split(',')
                    });
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

                //получаем список услуг
                List<TempService> tempServices = GetBookedSrvices(new int[]{bookId});

                //собираем список с id новых туристов
                List<string> tempTuristsIds = new List<string>();

                foreach (TempService tfl in tempServices)
                {
                    for (int i = 0; i < tfl.Turists.Length; i++)                    //есть ли новый для брони турист 
                        if (!ContainsTuristId(dogovor.Turists, tfl.Turists[i])) tempTuristsIds.Add(tfl.Turists[i]);// = "-1"; 
                }

                TempTurist[] tempTurists = GetTurists(tempTuristsIds.ToArray());

                Dictionary<int, List<string>> serviceToTurist =  SaveNewServices(dogovor.DogovorLists, tempServices);

                SaveNewTurists(dogovor, tempTurists, serviceToTurist);

                dogovor.CalculateCost();
                MyCalculateCost(dogovor);

                dogovor.NMen = (short)dogovor.Turists.Count;
                dogovor.DataContainer.Update();
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
            DogovorLists dls = new DogovorLists(new DataContainer());
            dls.RowFilter = "dl_dgcod='" + dogovorCode + "' and DL_comment='" + bookId+"'";

            dls.Fill();

            if (dls.Count > 0)
            {
                dls[0].SetStatus(serviceAnnulateStatusKey);

                if (dls[0].Dogovor.Payed == 0)
                {
                    Dogovor dog = dls[0].Dogovor;

                    DeleteService(dls[0].Key);

                    dog.CalculateCost();

                    MyCalculateCost(dog);
                }
                else
                    dls[0].DataContainer.Update();
            }
            else
                throw new CatmoException("service not found", ErrorCodes.ServiceNotFound);
        }

        private static void DeleteService(int serviceKey)
        {
            SqlConnection con = new SqlConnection(Manager.ConnectionString);
            con.Open();
            SqlCommand com = new SqlCommand(String.Format("delete from tbl_dogovorlist where dl_key={0}", serviceKey), con);

            com.ExecuteNonQuery();

            con.Close();
        }

        //изменяет статус путевки на "запрос на аннуляцию"
        public static void AnnulateDogovor(string dogovorCode)
        {
            Dogovor dog = GetDogovor(dogovorCode);
            dog.SetStatus(dogovorAnnulateStatusKey);
            dog.DataContainer.Update();
        }

        //забирает путевки созданные пользователем
        public static Dogovors GetUserDogovors(string mail, int userkey)
        {
            Dogovors dogs = new Dogovors(new DataContainer());

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

        //получить путевку по ключу
        public static Dogovor GetDogovorByKey(int dg_key)
        {
            Dogovors dogs = new Dogovors(new DataContainer());
            dogs.RowFilter = "dg_key = " + dg_key ; //ищем закакз по номеру

            dogs.Fill(); //комит

            if (dogs.Count > 0) //если заказ нашелся, отдаем его
                return dogs[0];
            else                //иначе
                throw new CatmoException("dogovor not found", ErrorCodes.DogovorNotFound); //генерируем эксепшн
        }

        //получить путевку
        public static Dogovor GetDogovor(string dg_code)
        {
            Dogovors dogs = new Dogovors(new DataContainer()); 
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

            string hash = CalculateMD5Hash( dog.Code +  "extra" + saltKey );

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
        public static Responses.DogovorMessage[] GetDogovorMessages(int dgKey, DateTime minDate)
        {
            //получить список сообщений
            SqlConnection con = new SqlConnection(Manager.ConnectionString);
            con.Open();

            List<Responses.DogovorMessage> tempList = new List<Responses.DogovorMessage>();

            SqlCommand com = new SqlCommand(String.Format("select [DM_IsOutgoing], [DM_CreateDate], [DM_Text] from [DogovorMessages] where [DM_DGkey] = {0} and DM_CreateDate > '{1}'", dgKey, minDate.ToString("yyyy-MM-dd HH:mm:ss")), con);

            SqlDataReader reader = com.ExecuteReader();

            while (reader.Read())
            {
                tempList.Add(new Responses.DogovorMessage()
                {
                    Date = Convert.ToDateTime(reader["DM_CreateDate"]),
                    InOut = Convert.ToInt32(reader["DM_IsOutgoing"]),
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

            SqlCommand com = new SqlCommand(String.Format("insert into[DogovorMessages](DM_TypeCode, [DM_DGKey], DM_Text, [DM_IsOutgoing], DM_Remark) values({0},{1},'{2}',{3}, '')", 0, dog.Key, AntiInject(text), 0), con);

            try
            {
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        public static string CalculateMD5Hash(string input)
        {
            try
            {
                // step 1, calculate MD5 hash from input
                MD5 md5 = MD5.Create();
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hash = md5.ComputeHash(inputBytes);

                // step 2, convert byte array to hex string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " "+ ex.StackTrace);
                throw ex;
            }
        }
    }
}