using StackExchange.Redis;
using System;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Configuration;

namespace NostreetsExtensions.Utilities
{
    public static class CacheManager
    {
        static CacheManager()
        {
            _instance = MemoryCache.Default;

            if (WebConfigurationManager.AppSettings["Redis.Host"] != null)
            {
                _redisConfig = GetRedisConfigurationOptions();
                _hasRedisCache = true;
            }

        }

        static MemoryCache _instance = null;
        static ConfigurationOptions _redisConfig = null;
        static bool _hasRedisCache = false;


        public static bool Contains(string key)
        {
            return (key != null && (_instance.Contains(key) || (_hasRedisCache && RedisContains(key)))) ? true : false;
        }

        public static T Get<T>(string key)
        {
            T result = default(T);

            if (Contains(key))
                result = (T)_instance.Get(key);

            /*else*/
            if (RedisContains(key))
            {
                result = RedisGet<T>(key);
                //_instance.Add(key, result, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(180) });
            }

            return result;

        }

        public static void Set(string key, object data, int minsTillExp = 180)
        {
            if (_hasRedisCache)
                RedisSet(key, data, TimeSpan.FromMinutes(minsTillExp));

            if (!Contains(key))
                _instance.Add(key, data, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minsTillExp) });

            else
                _instance.Set(key, data, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minsTillExp) });



        }

        public static void Remove(string key)
        {
            if (Contains(key))
                _instance.Remove(key);

            if (RedisContains(key))
                RedisRemove(key);
        }


        #region Redis Methods
        private static T RedisGet<T>(string key)
        {
            T result;
            var db = Connection().GetDatabase(-1);
            var value = db.StringGet(key);
            result = value.HasValue ? ((string)value).JsonDeserialize<T>() : default(T);

            return result;
        }

        private static void RedisSet(string key, object data, TimeSpan cacheTime)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (cacheTime == default(TimeSpan))
                throw new ArgumentNullException(nameof(cacheTime));

            if (data == null)
                return;

            IDatabase db = Connection().GetDatabase();
            db.StringSet(key, data.JsonSerialize(), cacheTime);
        }

        private static bool RedisContains(string key)
        {
            IDatabase db = Connection().GetDatabase();
            return db.KeyExists(key);
        }

        private static void RedisRemove(string key)
        {
            IDatabase db = Connection().GetDatabase(-1);
            db.KeyDelete(key);
        }

        private static void RedisRemoveByPattern(string pattern)
        {
            if (int.TryParse(WebConfigurationManager.AppSettings["Redis.Port"], out int redisPort))
                throw new ArgumentException("Redis.Port needs to equal an int to be able to RemoveByPatternRedis()");

            IServer server = Connection().GetServer(_redisConfig.SslHost, redisPort);
            var keysToRemove = server.Keys(pattern: "*" + pattern + "*");
            foreach (var key in keysToRemove)
                Remove(key);
        }

        private static void RedisClear()
        {
            if (int.TryParse(WebConfigurationManager.AppSettings["Redis.Port"], out int redisPort))
                throw new ArgumentException("Redis.Port needs to equal an int to be able to ClearRedis()");

            IServer server = Connection().GetServer(_redisConfig.SslHost, redisPort);
            var keysToRemove = server.Keys();
            foreach (var key in keysToRemove)
                RedisRemove(key);
        }

        private static ConnectionMultiplexer Connection()
        {
            return ConnectionMultiplexer.Connect(_redisConfig);

        }

        private static ConfigurationOptions GetRedisConfigurationOptions()
        {
            if (!int.TryParse(WebConfigurationManager.AppSettings["Redis.Port"], out int redisPort))
                throw new ArgumentException("Redis.Port needs to equal an int to be able to GetRedisConfigurationOptions()");


            ConfigurationOptions options = new ConfigurationOptions();

            string redisHost = WebConfigurationManager.AppSettings["Redis.Host"];
            string redisPassword = WebConfigurationManager.AppSettings["Redis.Password"];

            options = new ConfigurationOptions();
            options.EndPoints.Add(redisHost, redisPort);
            options.Ssl = true;
            options.Password = redisPassword;
            options.AbortOnConnectFail = false;
            options.SyncTimeout = int.MaxValue;
            options.WriteBuffer = 10000000;
            options.KeepAlive = 180;
            options.ConnectRetry = 5;
            options.ConnectTimeout = 10000;

            return options;
        }
        #endregion

    }

}
