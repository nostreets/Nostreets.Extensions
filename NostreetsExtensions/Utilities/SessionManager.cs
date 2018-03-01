using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

namespace NostreetsExtensions.Utilities
{
    public class SessionManager
    {
        private static HttpSessionState _session = HttpContext.Current.Session;

        static SessionManager()
        { }

        public static bool DoesKeyExist(SessionState key)
        {
            if (!HasAnySessions())
            {
                return false;
            }
            bool exist = false;
            for (int i = 0; i < _session.Count; i++)
            {
                exist = _session.Keys[i] == key.ToString();
                if (exist)
                {
                    break;
                }
            }
            return exist;
        }

        public static bool IsNull(SessionState key)
        {
            return _session[key.ToString()] == null;
        }

        public static void SetNull(SessionState key)
        {
            _session[key.ToString()] = null;
        }

        public static TSource Get<TSource>(SessionState key)
        {
            if (IsNull(key))
            {
                throw new Exception(String.Format("The session with key '{0}' is null", key.ToString()));
            }
            return (TSource)_session[key.ToString()];
        }

        public static object Get(SessionState key)
        {
            if (IsNull(key))
            {
                throw new Exception(String.Format("The session with key '{0}' is null", key.ToString()));
            }
            return _session[key.ToString()];
        }

        public static void Add<TSource>(TSource model, SessionState key)
        {
            if (DoesKeyExist(key))
            {
                throw new Exception(String.Format("The session key '{0}' is already been used, try using another key",
                    key.ToString()));
            }
            _session.Add(key.ToString(), model);
        }

        public static void Add(Dictionary<SessionState, object> models)
        {
            foreach (KeyValuePair<SessionState, object> item in models)
            {
                if (DoesKeyExist(item.Key))
                {
                    throw new Exception(String.Format("The session key '{0}' is already been used, try using another key",
                        item.Key.ToString()));
                }
                _session.Add(item.Key.ToString(), item.Value);
            }
        }

        public static void Replace<TSource>(TSource model, SessionState key)
        {
            if (!DoesKeyExist(key))
            {
                throw new Exception(String.Format("The session key '{0}' is not been used yet", key.ToString()));
            }

            if (!IsNull(key) && (model.GetType() != _session[key.ToString()].GetType()))
            {
                throw new Exception(
                    String.Format("The old data type of session key '{0}' is not matching with the new data type",
                        key.ToString()));
            }
            _session[key.ToString()] = model;
        }

        public static void Remove(SessionState key)
        {
            if (!DoesKeyExist(key))
            {
                throw new Exception(
                    String.Format("The session with the key '{0}' is already been removed, or not used yet", key.ToString()));
            }
            _session.Remove(key.ToString());
        }

        public static string GetSessionId()
        {
            return _session.SessionID;
        }

        public static bool HasAnySessions()
        {
            return _session.Count > 0;
        }

        public static void RemoveAll()
        {
            _session.RemoveAll();
        }

        public static void AbandonSessions()
        {
            _session.Abandon();
        }
    }

    public enum SessionState
    {
        IsUser,
        IsLoggedOn,
        UserId,
        User,
        LogInTime,
        LogOffTime
    }
}
