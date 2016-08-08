using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Sider;
using System.Threading.Tasks;
using System.Threading;
using ClickAndTravelMiddleOffice.Helpers;

namespace ClickAndTravelMiddleOffice.Store
{
    public class RedisHelper
    {

        private static string host = ConfigurationManager.AppSettings["RedisHost"];

        public static void SetString(string key, string value)
        {

            RedisClient redis_clinet;

            int max = 10;
            while (max-- > 0)
            {
                try
                {
                    redis_clinet = new RedisClient(host);
                    redis_clinet.Set(key, value);

                    break;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(100);

                }
            }
        }

        public static void SetString(string key, string value, TimeSpan lifetime)
        {
            RedisClient redis_clinet;

            int max = 10;
            while (max-- > 0)
            {
                try
                {
                    redis_clinet = new RedisClient(host);
                    redis_clinet.SetEX(key, lifetime, value);
                }
                catch (Exception ex)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public static string GetString(string key)
        {
            RedisClient redis_clinet;

            int max = 10;
            while (max-- > 0)
            {
                try
                {
                    redis_clinet = new RedisClient(host);
                    string value = redis_clinet.Get(key);

                    return value;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(100);
                }
            }

            throw new Exception("redis get string exception");
        }
    }
}