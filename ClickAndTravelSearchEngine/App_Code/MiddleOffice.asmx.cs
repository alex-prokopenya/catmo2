using ClickAndTravelMiddleOffice.Exceptions;
using ClickAndTravelMiddleOffice.Helpers;
using ClickAndTravelMiddleOffice.MasterTour;
using ClickAndTravelMiddleOffice.ParamsContainers;
using ClickAndTravelMiddleOffice.Responses;

using Megatec.Common.BusinessRules.Base;
using Megatec.MasterTour.BusinessRules;
using Megatec.MasterTour.DataAccess;
using Megatec.MasterTour.Web.Security;
using Megatec.MasterWeb.Logic;
using Megatec.Cryptography;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.ServiceModel;
using System.Web.Services;

namespace ClickAndTravelMiddleOffice
{
    [ServiceContractAttribute(Namespace = "http://schemas.myservice.com")]


    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://clickandtravel.ru/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]

    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class MiddleOffice : System.Web.Services.WebService
    {
        public MiddleOffice()
            : base()
        {
            Manager.ConnectionString = ConfigurationManager.AppSettings["MasterTourConnectionString"];
        }

        #region PrivateFields rate_codes, rate_courses

        private string[] rate_codes = new string[] { "RUB", "USD", "EUR", "BYR", "UAH", "KZT" };

        private KeyValuePair<string, decimal>[] rate_courses;

        #endregion
        public static object CheckAgentLogin(string login, string passwordhash) {

            var salt = ConfigurationManager.AppSettings["AuthSalt"];
            //проверить пароль
            DupUsers dus = new DupUsers(new Megatec.Common.BusinessRules.Base.DataContainer());
            dus.RowFilter = "US_ID = '" + InjectionProtector.ValidString(MtHelper.PrepareLogin(login)) + "' ";

            dus.Fill();

            if (dus.Count > 0)
            {
                Logger.WriteToLog(dus[0].Password);
                var password = dus[0].Password;

                try {
                    password =  Megatec.MasterTour.Web.Security.Authentication.GetPassword(dus[0].Password, true);
                }
                catch (Exception ex)
                {
                    Logger.WriteToLog(ex.Message + ex.StackTrace);
                }

                var hash = MtHelper.CalculateMD5Hash(password + salt);

                if (hash.ToUpper() == passwordhash.ToUpper())
                    return new { result = "success", token = MtHelper.CalculateMD5Hash(login + salt + DateTime.Now.AddHours(-1).ToString("yyMMddHH")) };
                else
                    return new { result = "fail", error = "wrong password" };
            }

            return new { result = "fail", error = "login not found" };

        }

        public static Dogovor GetDogovor(string code)
        {
            Dogovors dogs = new Dogovors(new DataContainer());
            dogs.RowFilter = "dg_code='" + code + "'";
            dogs.Fill();

            if (dogs.Count == 0)
                throw new CatmoException("Не найдена путевка с номером " + code.Trim() + "!", ErrorCodes.AnotherException);

            Dogovor dog = dogs[0];

            if (dog.TurDate < System.Convert.ToDateTime("1900-01-01"))
                throw new CatmoException("Путевка с номером " + code.Trim() + " аннулирована!", ErrorCodes.AnotherException);

            if (dog.TurDate < DateTime.Today)
                throw new CatmoException("По путевке " + code.Trim() + " уже наступила дата заезда!", ErrorCodes.AnotherException);

            if ((dog.Price - dog.Payed) <= 0)
                throw new CatmoException("Путевка с номером " + code.Trim() + " уже оплачена!", ErrorCodes.AnotherException);

            if ((dog.OrderStatusKey == 2) || (dog.OrderStatusKey == 4))
                throw new CatmoException("Статус путевки " + dog.Code + " запрещает оплату", ErrorCodes.AnotherException);

            return dog;
        }

        public static bool TryBonusPayment(string dogovorCode, long transactionId, int summ)
        {
            Dogovor dog = GetDogovor(dogovorCode);

            if (!IsPidUsed(dog, transactionId.ToString()))
                return CreatePay_UPayment(dog, transactionId.ToString(), summ);
            else
                throw new CatmoException("Trans_id already used", ErrorCodes.AnotherException);
        }


        public static bool IsPidUsed(Dogovor dogovor, string pId)
        {
            SqlConnection myConnection = new SqlConnection(Manager.ConnectionString);
            myConnection.Open();

            try
            {
                string findingNum = "bs_" + pId + "" + dogovor.Key;
                SqlCommand com = new SqlCommand();
                com.Connection = myConnection;
                com.CommandText = "select count(*) from fin_documents where dc_outnum like '%" + findingNum + "%'";
                int num = Convert.ToInt32(com.ExecuteScalar());
                return num > 0;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Result - Ошибка при выполнении поиска документа " + pId + "\n" + ex.Message + "\n" + ex.StackTrace);
            }
            finally
            {
                myConnection.Close();
            }
            return false;
        }

        //проведение оплаты бонусами
        private static bool CreatePay_UPayment(Dogovor dogovor, string pId, double summ)
        {
            SqlConnection myConnection = new SqlConnection(ConfigurationManager.AppSettings["MasterTourConnectionString"]);
            myConnection.Open();
            try
            {
                int error = 0;
                SqlCommand com = new SqlCommand("FIN_VZT_ADD_INCPAYMENT", myConnection);

                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.Add(new SqlParameter("@p_dtDate", DateTime.Today.ToString("yyyy-MM-dd")));
                com.Parameters.Add(new SqlParameter("@p_nFirm", 8637));     //Клик
                com.Parameters.Add(new SqlParameter("@p_nBank", 134));      //Сбербанк
                com.Parameters.Add(new SqlParameter("@p_nSum", summ)); //сумма в валюте ПС
                com.Parameters.Add(new SqlParameter("@p_sRate", "рб")); //код валюты ПС
                com.Parameters.Add(new SqlParameter("@p_nTurSum", summ));//сумма в евро
                com.Parameters.Add(new SqlParameter("@p_sTurRate", dogovor.RateCode));   //евро
                com.Parameters.Add(new SqlParameter("@p_sReceipt", "bs_" + pId + "" + dogovor.Key));
                com.Parameters.Add(new SqlParameter("@p_sDogovor", dogovor.Code));
                com.Parameters.Add(new SqlParameter("@p_sComment", "Проводка по оплате бонусами. " + pId));
                com.Parameters.Add(new SqlParameter("@p_nError", error));
                com.CommandTimeout = 3000;
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Result - ошибка при создании проводок: " + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
            finally
            {
                myConnection.Close();
            }
            return true;
        }


        //применяет пачку курсов к цене
        private KeyValuePair<string, decimal>[] ApplyCourses(decimal price, KeyValuePair<string, decimal>[] courses)
        {
            KeyValuePair<string, decimal>[] res = new KeyValuePair<string, decimal>[courses.Length];

            for (int i = 0; i < courses.Length; i++)
                res[i] = new KeyValuePair<string, decimal>(courses[i].Key, Math.Round(price / courses[i].Value));

            return res;
        }

        [WebMethod]
        public NewDogovorResponse CreateNewDogovor(int[] bookIds, UserInfo info)
        {
            Dogovor dog = MtHelper.SaveNewDogovor(bookIds, info);
            KeyValuePair<string, decimal>[] _courses = MtHelper.GetCourses(rate_codes, dog.Rate.ISOCode, DateTime.Today);

            return new NewDogovorResponse()
            {
                DogovorCode = dog.Code,
                PayDate = dog.PaymentDate,
                Prices = ApplyCourses(Convert.ToDecimal(dog.Price), _courses),
            };
        }

        [WebMethod]  //annulate by user
        public int AnnulateDogovor(string dogovorCode, int[] services)
        {
            dogovorCode = MtHelper.GetDogovor(dogovorCode).Code; //для тестов подставляем определенный номер

            try
            {
                if (services.Length > 0)
                    foreach (int book_id in services)
                        MtHelper.AnnulateService(dogovorCode, book_id);
                else
                    MtHelper.AnnulateDogovor(dogovorCode);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("AnnulateDogovor exc: " + ex.Message + ex.StackTrace);
                throw ex;
            }

            return 1;
        }

        [WebMethod]  //dogovors by user
        public DogovorHeader[] GetDogovorsByUser(string email, int user_id)
        {
            Dogovors dogs = MtHelper.GetUserDogovors(email, user_id);

            DogovorHeader[] res = new DogovorHeader[dogs.Count];

            DateTime minDate = new DateTime(2016, 3, 1);

            for (int i = 0; i < dogs.Count; i++)
            {
                Dogovor dog = dogs[i];
                //получить курсы
                KeyValuePair<string, decimal>[] _courses = MtHelper.GetCourses(rate_codes, dog.Rate.ISOCode, dog.CreateDate);

                res[i] = new DogovorHeader()
                {
                    DogovorCode = dog.Code,
                    DogovorStatus = dog.OrderStatusKey,
                    PaidSumm = ApplyCourses(Convert.ToDecimal(dog.Payed), _courses),
                    TotalPrices = ApplyCourses(Convert.ToDecimal(dog.Price), _courses),
                    TourDate = dog.TurDate,
                    UpDate = dog.ConfirmedDate,
                    Services = GetDogovorServices(dog),
                    DocumentsCount = MtHelper.GetDogovorDocuments(dog.Code).Length,
                    CommentsCount = GetDogovorMessages(dog.Code, minDate).Length
                };
            }

            return res;
        }

        private ServiceSimpleInfo[] GetDogovorServices(Dogovor dog)
        {
            var tempDict = new Dictionary<int, ServiceSimpleInfo>();

            dog.DogovorLists.Fill(); //загрузить услуги

            for (int j = 0; j < dog.DogovorLists.Count; j++)//DogovorList dl in dog.DogovorLists)
            {
                DogovorList dl = dog.DogovorLists[j];

                int bookId = 0;
                int tempKey = j;
                try
                {
                    bookId = Convert.ToInt32(dl.Comment);
                    tempKey = bookId;
                }
                catch (Exception) { }

                if (tempDict.ContainsKey(tempKey))
                {
                    tempDict[tempKey].Title += " + " + dl.Name;
                }
                else
                    tempDict[tempKey] = new ServiceSimpleInfo()
                    {
                        BookId = bookId,
                        Status = dl.ControlKey,
                        Title = dl.Name
                    };
            }

            return tempDict.Values.ToArray();
        }

        [WebMethod] //dogovor info
        public DogovorInfo GetDogovorInfo(string dogovorCode)
        {
            try
            {
                //получаем заказ по номеру
                Dogovor dog = MtHelper.GetDogovor(dogovorCode);

                dog.DogovorLists.Fill();

                //узнаем курсы валют
                KeyValuePair<string, decimal>[] _courses = MtHelper.GetCourses(rate_codes, dog.Rate.ISOCode, dog.CreateDate);

                //создаем массив по количеству услуг
                var tempServicesDict = new Dictionary<int, ServiceInfo>();
                var tempPrices = new Dictionary<int, double>();

                int cnt = 0;
                for (int j = 0; j < dog.DogovorLists.Count; j++) //заполняем массив услуг
                {
                    DogovorList dl = dog.DogovorLists[j];

                    int bookId = 0;
                    int tempKey = j;
                    try
                    {
                        bookId = Convert.ToInt32(dl.Comment);
                        tempKey = bookId;
                    }
                    catch (Exception) { }

                    if (tempPrices.ContainsKey(tempKey))
                        tempPrices[tempKey] += dl.Brutto;
                    else
                        tempPrices[tempKey] = dl.Brutto;


                    tempServicesDict[tempKey] = new ServiceInfo()
                    {
                        BookId = bookId,
                        Day = dl.Day,
                        Ndays = dl.NDays,
                        Prices = ApplyCourses(Convert.ToDecimal(tempPrices[tempKey]), _courses),
                        ServiceClass = dl.ServiceKey,
                        Status = dl.ControlKey,
                        Title = dl.Name
                    };
                }

                //получаем информацию о туристах
                dog.Turists.Fill();
                TuristContainer[] turists = new TuristContainer[dog.Turists.Count];

                cnt = 0;

                foreach (Turist ts in dog.Turists) //заполняем массив услуг
                {
                    int tsId = cnt;

                    try
                    {
                        tsId = Convert.ToInt32(ts.PostIndex);
                    }
                    catch (Exception) { }

                    turists[cnt++] = new TuristContainer()
                    {
                        Id = tsId,
                        Name = ts.NameLat,
                        FirstName = ts.FNameLat,
                        BirthDate = ts.Birthday,
                        Citizenship = ts.Citizen,
                        PassportDate = ts.PasportDateEnd,
                        PassportNum = ts.PasportType + ts.PasportNum,
                        Sex = ts.RealSex + 1
                    };
                }

                return new DogovorInfo()
                {
                    Turists = turists,

                    DogovorStatus = dog.OrderStatusKey,

                    PaidSumm = ApplyCourses(Convert.ToDecimal(dog.Payed), _courses),

                    TotalPrices = ApplyCourses(Convert.ToDecimal(dog.Price), _courses),

                    TourDate = dog.TurDate,

                    PayDate = dog.PaymentDate,

                    UpDate = dog.ConfirmedDate,

                    Messages = GetDogovorMessages(dog.Code, Convert.ToDateTime("2013-10-10")),

                    Services = tempServicesDict.Values.ToArray(),

                    Documents = MtHelper.GetDogovorDocuments(dog.Code)
                };
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.Source + " " + ex.StackTrace);
                throw ex;

            }
        }

        [WebMethod]
        public Responses.DogovorMessage[] GetDogovorMessages(string dogovorCode, DateTime minDate)
        {
            Dogovor dog = MtHelper.GetDogovor(dogovorCode);

            return MtHelper.GetDogovorMessages(dog.Key, minDate);
        }

        [WebMethod]
        public int SendMessage(string dogovorCode, string text)
        {
            Dogovor dog = MtHelper.GetDogovor(dogovorCode);

            return MtHelper.SaveDogovorMessage(dog.Code, text);
        }

        [WebMethod]
        public int AddService(string dogovorCode, int bookId)
        {
            try
            {
                //получаем заказ по номеру
                Dogovor dog = MtHelper.GetDogovor(dogovorCode);
                return MtHelper.AddServiceToDogovor(dog, bookId);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog(ex.Message + " " + ex.Source + " " + ex.StackTrace);
                throw ex;
            }
        }
    }
}