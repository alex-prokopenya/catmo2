using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using YamlDotNet.Serialization;
using System.IO;
using System.Text;
using Jayrock.Json.Conversion;
using Jayrock.Json;
using ClickAndTravelMiddleOffice.DB;

namespace ClickAndTravelMiddleOffice.Helpers
{
    public class ServiceNameTranslator
    {
        /// <summary>
        /// из json объекта service формируем красивую строку названия услуги
        /// </summary>
        /// <param name="serviceType">тип услуги (flight|hotel|excursion|transfer)</param>
        /// <param name="serviceYaml">yaml строка с деталями услуги</param>
        /// <returns>строка названия услуги</returns>
        public static string GetServiceTitle(string serviceType, string searchId, string serviceYaml)
        {
            var jsonObject = GetJsonObjectFromYaml(serviceYaml);

            switch (serviceType)
            {
                case "flight":
                    return GetTitleForFlight(jsonObject);

                case "hotel":
                    return GetTitleForHotel(searchId, jsonObject);

                case "excursion":
                    return GetTitleForExcursion(searchId, jsonObject);

                case "transfer":
                    return GetTitleForTransfer(searchId, jsonObject);

                default: return "unknown service_type";
            }
        }

        /// <summary>
        /// конвертируем yaml в json код
        /// </summary>
        /// <param name="serviceYaml"></param>
        /// <returns>json строка</returns>
        public static string GetJsonCodeFromYaml(string serviceYaml)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            var deserializer = new Deserializer();

            ///remove all aliases
            int i = 1;
            while(serviceYaml.Contains(": &"+ i))
            {
                serviceYaml = serviceYaml.Replace("&"+i, "").Replace("*"+i, "");
                i++;
            }

            var yamlObject = deserializer.Deserialize(new StringReader(serviceYaml));
            
            yamlObject = deserializer.Deserialize(new StringReader(serviceYaml));
            var serializer = new Serializer(SerializationOptions.JsonCompatible);
            serializer.Serialize(sw, yamlObject);

            return sb.ToString();
        }


        /// <summary>
        /// конвертируем yaml в json объект
        /// </summary>
        /// <param name="serviceYaml">yaml код</param>
        /// <returns>json объект</returns>
        public static JsonObject GetJsonObjectFromYaml(string serviceYaml)
        {
            var text = GetJsonCodeFromYaml(serviceYaml);
            return JsonConvert.Import<JsonObject>(text);
        }

        private static string GetTitleForHotel(string searchId, JsonObject service)
        {
            var cityId = Convert.ToInt32(searchId.Split('-')[0].Substring(8));

            var title = MySqlDataProvider.GetCityNameById(cityId) + ": " + service["title"] + " | ";

            var variant = ((((service["rooms"] as JsonArray)[0] as JsonObject)["variants"] as JsonArray)[0] as JsonObject);

            title += variant["pansion_title"] + " - " + variant["room_title"] + " " + variant["room_category"];

            return title;
        }

        private static string GetTitleForTransfer(string searchId, JsonObject service)
        {
            var cityId = Convert.ToInt32(searchId.Split('-')[1]);

            var title = MySqlDataProvider.GetCityNameByPointId(cityId) + ": " + service["start_point_title"] + " - " + service["end_point_title"];

            if (!string.IsNullOrEmpty(service["departure_date"].ToString()))
                title += " - " + service["start_point_title"];

            title += " | " + service["title"];

            return title;
        }

        private static string GetTitleForExcursion(string searchId, JsonObject service)
        {
            var cityId = Convert.ToInt32(searchId.Split('-')[1]);

            var title = MySqlDataProvider.GetCityNameById(cityId) + ": " + service["title"];

            if (service["groupe"].ToString() == "true")
                title += " | Групповая | ";
            else
                title += " | Индивидуально | ";

            title += service["excursion_time"];

            return title;
        }

        private static string GetTitleForFlight(JsonObject service)
        {
            var airline = service["airline_code"].ToString();

            var routeItems = service["route_items"] as JsonArray;

            var title = (((routeItems[0] as JsonObject)["legs"] as JsonArray)[0] as JsonObject)["departure_city"].ToString();

            foreach (JsonObject routeItem in routeItems)
                title += GetFlightChanges(routeItem) + ((routeItem["legs"] as JsonArray).Last() as JsonObject)["arrival_city"];

            return title + " | " + airline;
        }

        private static string GetFlightChanges(JsonObject routeItem)
        {
            var result = "";

            if ((routeItem["legs"] as JsonArray).Length > 1)
            {
                foreach (JsonObject leg in routeItem["legs"] as JsonArray)
                    result += leg["departure_code"].ToString() + "|";

                return "(" + result.Substring(4) + ")";
            }
            else
                return " - ";
        }
    }
}