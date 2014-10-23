<%@ WebHandler Language="C#" Class="ClickAndTravelMiddleOffice.json_handler" %>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Jayrock.Json;
using Jayrock.JsonRpc;
using Jayrock.JsonRpc.Web;
using Jayrock.Json.Conversion;
using System.Collections;
using Jayrock.Services;

using ClickAndTravelMiddleOffice.Responses;
using ClickAndTravelMiddleOffice.ParamsContainers;
using ClickAndTravelMiddleOffice.Helpers;
using ClickAndTravelMiddleOffice.Exceptions;
using ClickAndTravelMiddleOffice.Containers.CarRent;

namespace ClickAndTravelMiddleOffice
{
    /// <summary>
    /// Summary description for json_handler
    /// </summary>
    public class json_handler : JsonRpcHandler
    {
        private MiddleOffice middle_office = new MiddleOffice();


        public json_handler()
        {

            JsonRpcDispatcherFactory.Current = s => new ClickAndTravelMiddleOffice.JsonRpcDispatcher(s);
        }
        
        //++++
        //создаем новый заказ в мастер-туре по полученным айдишникам от поставщиков услуг
        [JsonRpcMethod("get_server_time")]
        [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"get_server_time\",\"params\":[], \"id\":0}")]
        public object get_server_time()
        {
            //выводим текущие время и дату в виде массива
            return new int[] { DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second};
        }

      
        [JsonRpcMethod("make_bonus_payment")]
        [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"make_bonus_payment\",\"params\":[\"TPA12332125\", 1010, 100],\"id\":0}")]
        public object make_bonus_payment(string dogovor_code, long trans_id, int summ)
        {
            //проверяем номер путевки на валидность
            if (!Validator.CheckDogovorCode(dogovor_code)) throw new CatmoException("Invalid dogovor_code", ErrorCodes.InvalidDogovorCode);

            if (trans_id < 0) throw new CatmoException("Invalid transaction id", ErrorCodes.InvalidParams);

            if (summ < 0) throw new CatmoException("Invalid summ", ErrorCodes.InvalidParams);

            if (MiddleOffice.TryBonusPayment(dogovor_code, trans_id, summ))
                return 1;
            else
                return 0;
        }
        
        //сохраняем новый заказ в МТ на основании сделаных в backoffice броней
        [JsonRpcMethod("create_new_dogovor")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"create_new_dogovor\",\"params\":[[1,2,3,4], {\"email\":\"user@test.com\",\"phone\":\"+375 29 111 22 33\"}],\"id\":0}")]
        public  object create_new_dogovor(JsonArray book_ids, JsonObject user_info)
        {
            UserInfo userInfo = null;
            try
            {
                //читаем информацию о покупателе
                userInfo = new UserInfo(user_info);
            }
            catch (Exception)
            {
                throw new CatmoException("cann't parse user_info", ErrorCodes.CanntParseUserInfo);
            }

            //забираем массив айдишников забронированных услуг
            int[] services_ = JsonArrayToIntArray(book_ids);

            //проверяем их количество
            if ((services_.Length == 0) || (services_.Length > 40))
                throw new CatmoException("Invalid book_ids length", ErrorCodes.InvalidBookIdsLength);
            
            //проверяем айдишники на валидность
            foreach (int key in services_)
                if (key <= 0) throw new CatmoException("Invalid book_id '" + key + "'", ErrorCodes.InvalidBookId);

            //создаем заказ
            return middle_office.CreateNewDogovor(services_, userInfo);
        }

        //++++++
        //аннулируем заказ, либо набор услуг из заказа
        [JsonRpcMethod("annulate_dogovor")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"annulate_dogovor\",\"params\":['TR4534',[]],\"id\":0}")]
        public object annulate_dogovor(string dogovor_code, params object[] args)
        {
            //проверяем номер путевки на валидность
            if (!Validator.CheckDogovorCode(dogovor_code)) throw new CatmoException("Invalid dogovor_code", ErrorCodes.InvalidDogovorCode);

            int[] service_keys = new int[0];

            //читаем список услуг
            if (args.Length > 0)
                service_keys = ObjectArrayToIntArray(args);

            //проверяем айдишники услуг на валидность
            foreach (int key in service_keys)
                if (key <= 0) throw new CatmoException("Invalid book_id '" + key + "'", ErrorCodes.InvalidBookId);

            //пробуем аннулировать услуги или заказ
            return middle_office.AnnulateDogovor(dogovor_code, service_keys);
        }

        //получаем список путевок привязаных к пользователю по email или user id
        [JsonRpcMethod("get_dogovors_by_user")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"get_dogovors_by_user\",\"params\":[\"tt@rr.tt\", 1],\"id\":0}")]
        public object get_dogovors_by_user(string user_mail, int user_id)
        {
            if (!Validator.CheckUserMail(user_mail)) throw new CatmoException("Invalid user_mail ", ErrorCodes.InvalidUserMail);

            return middle_office.GetDogovorsByUser(user_mail, user_id);
        }

        [JsonRpcMethod("get_dogovor_info")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"get_dogovor_info\",\"params\":[\"TR12423\"],\"id\":0}")]
        public object get_dogovor_info(string dogovor_code)
	    {
            if (!Validator.CheckDogovorCode(dogovor_code)) throw new CatmoException("Invalid dogovor_code", ErrorCodes.InvalidDogovorCode);

            return middle_office.GetDogovorInfo(dogovor_code);
        }

        [JsonRpcMethod("get_dogovor_messages")]
        [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"get_dogovor_messages\",\"params\":[\"TR12423\", \"2013-05-10 15:12:08\"],\"id\":0}")]
        public object get_dogovor_messages(string dogovor_code, string min_date_time)
        {
            if (!Validator.CheckDogovorCode(dogovor_code)) throw new CatmoException("Invalid dogovor_code", ErrorCodes.InvalidDogovorCode);

            DateTime minDate = DateTime.MinValue;

            try
            {
                minDate = DateTime.ParseExact(min_date_time, "yyyy-MM-dd HH:mm:ss", null); 
            }
            catch (Exception)
            {
                throw new CatmoException("Invalid date", ErrorCodes.InvalidDate);
            }

            return middle_office.GetDogovorMessages(dogovor_code, minDate);
        }
        
        
        //отправляем сообщение менеджеру от пользователя 
        [JsonRpcMethod("send_message")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"send_message\",\"params\":[\"TR34325\",\"testtt\"],\"id\":0}")]
        public object send_message(string dogovor_code, string text)
        {
            if (!Validator.CheckDogovorCode(dogovor_code)) throw new CatmoException("Invalid dogovor_code", ErrorCodes.InvalidDogovorCode);

            if ((text.Length > 1000) || (text.Length == 0)) throw new CatmoException("Too long message text", ErrorCodes.InvalidMessageSize);


            return middle_office.SendMessage(dogovor_code, text);
        }

        //добавляем услугу к уже созданному заказу
        [JsonRpcMethod("add_service")]
	    [JsonRpcHelp("{\"jsonrpc\":\"2.0\",\"method\":\"add_service\",\"params\":['TR4534',1],\"id\":0}")]	
        public object add_service(string dogovor_code, int book_id)
        {
            Logger.WriteToLog("test");
            if (!Validator.CheckDogovorCode(dogovor_code)) throw new CatmoException("Invalid dogovor_code", ErrorCodes.InvalidDogovorCode);

            if (book_id <= 0) throw new CatmoException("invalid book id", ErrorCodes.InvalidBookId);

            return middle_office.AddService(dogovor_code, book_id);
        }

        //конвертируем object массив в массив int
        private int[] ObjectArrayToIntArray(object[] inp)
        {
            try
            {
                int[] res = new int[inp.Length];

                for (int i = 0; i < inp.Length; i++)
                    res[i] = Convert.ToInt32(inp[i]);

                return res;
            }
            catch (Exception ex)
            {
                throw new CatmoException("cann't parse int array " + ex.Message + "\n" + ex.StackTrace, ErrorCodes.CanntParseArrayOfInteger);
            }
        }

        //конвертируем json массив в массив int
        private int[] JsonArrayToIntArray(JsonArray inp)
        {
            try
            {
                int[] res = new int[inp.Length];

                for (int i = 0; i < inp.Length; i++)
                    res[i] = Convert.ToInt32(inp[i]);

                return res;
            }
            catch (Exception ex)
            {
                throw new CatmoException("cann't parse int array " + ex.Message + "\n" + ex.StackTrace, ErrorCodes.CanntParseArrayOfInteger);
            }
        }
    }
}