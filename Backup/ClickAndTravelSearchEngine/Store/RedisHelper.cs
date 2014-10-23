using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Sider;
using System.Threading.Tasks;


namespace ClickAndTravelMiddleOffice.Store
{
    public class RedisHelper
    {
        private static string host = ConfigurationManager.AppSettings["RedisHost"];
        private static RedisClient redis_clinet = null;

        public static void SetString(string key, string value, bool wait = false)
        {
            redis_clinet = new RedisClient(host);
            redis_clinet.SetEX(key, new TimeSpan(24, 0, 0), value);
        }

        public static string GetString(string key)
        {
            redis_clinet = new RedisClient(host);
            return redis_clinet.Get(key);
        }
    }
}