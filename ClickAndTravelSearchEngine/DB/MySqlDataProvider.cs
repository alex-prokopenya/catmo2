using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Configuration;
using ClickAndTravelMiddleOffice.Helpers;
using ClickAndTravelMiddleOffice.MasterTour;

namespace ClickAndTravelMiddleOffice.DB
{
    public class ServiceJson
    {

        public int id;
        public string type;
        public string json;
    }
    public class MySqlDataProvider
    {
        /// <summary>
        /// Для book_id формируем название услуги
        /// </summary>
        /// <param name="serviceId">id услуги, для которой формируем название</param>
        /// <param name="loadDeailsToMaster">сохранять ли детали в мастер-туре</param>
        /// <returns>Название услуги для Мастер-Тура</returns>
        public static string GetServiceTitle(int serviceId, bool loadDeailsToMaster = false)
        {
            var selectQuery = "select search_id, detail, service_type from services where book_id = " + serviceId;

            //коннектимся к базе и выполняем запрос
            var connection = new MySqlConnection(ConfigurationManager.AppSettings["MySqlConnectionString"]);
            var command = new MySqlCommand(selectQuery, connection);
            connection.Open();
            var reader = command.ExecuteReader();

            var resultTitle = "unknown book_id";

            if (reader.Read())
            {
                string type = reader["service_type"].ToString();
                string searchId = reader["search_id"].ToString();

                resultTitle = ServiceNameTranslator.GetServiceTitle(type, searchId, reader["detail"].ToString());

                if (loadDeailsToMaster)
                    MtHelper.SaveServiceDetailsJson(
                            new ServiceJson
                            {
                                id = serviceId,
                                type = type,
                                json = ServiceNameTranslator.GetJsonCodeFromYaml(reader["detail"].ToString())
                            });
            }

            connection.Close();

            return resultTitle;
        }

        /// <summary>
        /// Для списка услуг формируем названия услуг
        /// </summary>
        /// <param name="serviceIds">Список id услуг</param>
        /// <returns>Словарь id услуги => название для Мастер-Тура</returns>
        public static Dictionary<int, string> GetServicesDetails(List<int> serviceIds, bool loadDeailsToMaster = false)
        {
            var selectQuery = "select search_id, book_id, detail, service_type from services where book_id in (" + string.Join(",", serviceIds) + ")";

            var connection = new MySqlConnection(ConfigurationManager.AppSettings["MySqlConnectionString"]);
            var command = new MySqlCommand(selectQuery, connection);
            connection.Open();

            var reader = command.ExecuteReader();

            var result = new Dictionary<int, string>(); //словарь для ответа функции
            var serviceList = new List<ServiceJson>();  //список услуг для добавления деталей в Мастер-Тур

            while (reader.Read())
            {
                string type = reader["service_type"].ToString();
                string searchId = reader["search_id"].ToString();

                int bookId = Convert.ToInt32(reader["book_id"]);

                string title = ServiceNameTranslator.GetServiceTitle(type, searchId, reader["detail"].ToString());

                serviceList.Add(
                               new ServiceJson()
                               {
                                   id = bookId,
                                   type = type,
                                   json = ServiceNameTranslator.GetJsonCodeFromYaml(reader["detail"].ToString())
                               });

                result.Add(bookId, title);
            }

            connection.Close();

            //если нужно сохранить детали в мастер, дергаем функцию
            if (loadDeailsToMaster)
                MtHelper.SaveServicesDetailsJson(serviceList);

            return result;
        }

        /// <summary>
        /// По id города ищем его название в справочнике клика
        /// </summary>
        /// <param name="cityId"> id города </param>
        /// <returns>Название города на сайте</returns>
        public static string GetCityNameById(int cityId)
        {
            var selectQuery = "select title from cities where id = " + cityId;

            //коннектимся к базе и выполняем запрос
            var connection = new MySqlConnection(ConfigurationManager.AppSettings["MySqlConnectionString"]);
            var command = new MySqlCommand(selectQuery, connection);
            connection.Open();

            var title = command.ExecuteScalar().ToString();

            connection.Close();

            return title;
        }


        public static string GetCityNameByPointId(int pointId)
        {
            var selectQuery = "select c.title from cities c, transfer_points t where t.city_id = c.id  and  t.id = " + pointId;

            //коннектимся к базе и выполняем запрос
            var connection = new MySqlConnection(ConfigurationManager.AppSettings["MySqlConnectionString"]);
            var command = new MySqlCommand(selectQuery, connection);
            connection.Open();

            var title = command.ExecuteScalar().ToString();

            connection.Close();

            return title;
        }
    }
}