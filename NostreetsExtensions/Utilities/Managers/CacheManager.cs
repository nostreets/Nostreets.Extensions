using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
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
                _hasRedisCache = true;
                _reconnectLock = new object();
                _redisConfig = GetRedisConfigurationOptions();
                _multiplexer = CreateMultiplexer();
            }

        }

        private static MemoryCache _instance = null;
        private static ConfigurationOptions _redisConfig = null;
        private static bool _hasRedisCache = false;


        private static void Contains(string key, out bool instanceContains, out bool redisContains)
        {
            instanceContains = _instance.Contains(key);
            redisContains = RedisContains(key);


            if (key != null)
                if (instanceContains)
                {
                    object data = _instance.Get(key);

                    if (_hasRedisCache && !redisContains)
                    {
                        RedisSet(key, data, TimeSpan.FromMinutes(180));
                        redisContains = true;
                    }
                }
                else if (_hasRedisCache && redisContains)
                {
                    object data = RedisGet(key);
                    _instance.Add(key, data, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(180) });
                    instanceContains = true;
                }

        }

        public static T Get<T>(string key)
        {
            T result = default(T);
            Contains(key, out bool instanceContains, out bool redisContains);

            if (instanceContains || redisContains)
            {
                if (instanceContains)
                {
                    object obj = _instance.Get(key);

                    if (obj.GetType() == typeof(JObject))
                        result = ((JObject)obj).ToObject<T>();

                    else if (obj.GetType() == typeof(T))
                        result = (T)obj;


                    if (_hasRedisCache && !redisContains)
                        RedisSet(key, result, TimeSpan.FromMinutes(180));
                }

                if (_hasRedisCache && redisContains && result.Equals(default(T)))
                {
                    result = RedisGet<T>(key);
                    if (!instanceContains)
                        _instance.Add(key, result, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(180) });
                }
            }

            return result;
        }

        public static void Set(string key, object data, int minsTillExp = 180)
        {
            Contains(key, out bool instanceContains, out bool redisContains);


            if (_hasRedisCache)
                RedisSet(key, data, TimeSpan.FromMinutes(minsTillExp));


            if (!instanceContains)
                _instance.Add(key, data, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minsTillExp) });

            else
                _instance.Set(key, data, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(minsTillExp) });
        }

        public static void Remove(string key)
        {
            Contains(key, out bool instanceContains, out bool redisContains);


            if (instanceContains)
                _instance.Remove(key);

            if (_hasRedisCache && redisContains)
                RedisRemove(key);
        }

        public static bool Contains(string key)
        {
            Contains(key, out bool instanceContains, out bool redisContains);

            return instanceContains || redisContains;
        }


        #region Redis Logic

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return GetRedisConnection();
            }
        }

        #region Redis Fields
        private static Lazy<ConnectionMultiplexer> _multiplexer = null;
        private static DateTimeOffset _lastReconnectTime = DateTimeOffset.MinValue,
                                      _firstError = DateTimeOffset.MinValue,
                                      _previousError = DateTimeOffset.MinValue;
        private static TimeSpan _reconnectMinFrequency = TimeSpan.FromSeconds(60),
                                _reconnectErrorThreshold = TimeSpan.FromSeconds(30);
        private static object _reconnectLock = null;
        #endregion

        #region Redis Methods
        private static T RedisGet<T>(string key)
        {
            T result = default(T);

            if (_hasRedisCache)
            {
                IDatabase db = Connection.GetDatabase(-1);
                RedisValue value = db.StringGet(key);

                if (value.HasValue)
                    result = ((string)value).JsonDeserialize<T>();
            }

            return result;
        }

        private static object RedisGet(string key)
        {
            object result = null;

            if (_hasRedisCache)
            {
                IDatabase db = Connection.GetDatabase(-1);
                RedisValue value = db.StringGet(key);

                if (value.HasValue)
                    result = ((string)value).JsonDeserialize();
            }

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

            if (_hasRedisCache)
            {
                IDatabase db = Connection.GetDatabase(-1);
                db.StringSet(key, data.JsonSerialize(), cacheTime);
            }
        }

        private static bool RedisContains(string key)
        {
            bool result = false;
            if (_hasRedisCache)
            {
                IDatabase db = Connection.GetDatabase(-1);
                result = db.KeyExists(key);
            }

            return result;
        }

        private static void RedisRemove(string key)
        {
            if (_hasRedisCache)
            {
                IDatabase db = Connection.GetDatabase(-1);
                db.KeyDelete(key);
            }
        }

        private static void RedisRemoveByPattern(string pattern)
        {
            if (int.TryParse(WebConfigurationManager.AppSettings["Redis.Port"], out int redisPort))
                throw new ArgumentException("Redis.Port needs to equal an int to be able to RemoveByPatternRedis()");

            if (_hasRedisCache)
            {
                IServer server = Connection.GetServer(_redisConfig.SslHost, redisPort);
                var keysToRemove = server.Keys(pattern: "*" + pattern + "*");
                foreach (var key in keysToRemove)
                    Remove(key);
            }
        }

        private static void RedisClear()
        {
            if (int.TryParse(WebConfigurationManager.AppSettings["Redis.Port"], out int redisPort))
                throw new ArgumentException("Redis.Port needs to equal an int to be able to ClearRedis()");

            if (_hasRedisCache)
            {
                IServer server = Connection.GetServer(_redisConfig.SslHost, redisPort);
                var keysToRemove = server.Keys();
                foreach (var key in keysToRemove)
                    RedisRemove(key);
            }
        }

        private static ConfigurationOptions GetRedisConfigurationOptions()
        {
            if (!int.TryParse(WebConfigurationManager.AppSettings["Redis.Port"], out int redisPort))
                throw new ArgumentException("Redis.Port needs to equal an int to be able to GetRedisConfigurationOptions()");


            ConfigurationOptions options = new ConfigurationOptions();

            string redisHost = WebConfigurationManager.AppSettings["Redis.Host"];
            string redisPassword = WebConfigurationManager.AppSettings["Redis.Password"];

            options.EndPoints.Add(redisHost, redisPort);
            options.Password = redisPassword;
            options.AllowAdmin = true;
            options.Ssl = true;
            options.AbortOnConnectFail = false;
            options.SyncTimeout = int.MaxValue;
            options.ConnectRetry = 10;
            options.WriteBuffer = 10000000;
            options.ConnectTimeout = 30000;
            options.KeepAlive = 180;

            return options;
        }

        private static ConnectionMultiplexer GetRedisConnection()
        {
            return _multiplexer.Value; 
        }

        private static Lazy<ConnectionMultiplexer> CreateMultiplexer()
        {
            return new Lazy<ConnectionMultiplexer>(
                () =>
                {
                    return ConnectionMultiplexer.ConnectAsync(_redisConfig).Complete();
                }
            );
        }
        #endregion

        #endregion

    }

}
