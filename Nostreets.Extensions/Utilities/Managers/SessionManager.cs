using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

namespace NostreetsExtensions.Utilities.Managers
{
    public class SessionManager
    {
        static SessionManager()
        {
            _session = HttpContext.Current.Session;
        }

        public SessionManager(HttpSessionState session)
        {
            _session = session;
        }

        public static HttpSessionState Session
        {
            get
            {
                if (_session == null)
                    throw new Exception("Sessions are null in this web project...");
                else
                    return _session;
            }
        }

        private static HttpSessionState _session = null;

        public static bool DoesKeyExist(SessionState key)
        {
            if (!HasAnySessions())
                return false;

            bool exist = false;
            for (int i = 0; i < Session.Count; i++)
            {
                exist = Session.Keys[i] == key.ToString();
                if (exist)
                    break;
            }

            return exist;
        }

        public static bool IsNull(SessionState key)
        {
            return Session[key.ToString()] == null;
        }

        public static void SetNull(SessionState key)
        {
            Session[key.ToString()] = null;
        }

        public static TSource Get<TSource>(SessionState key)
        {
            if (IsNull(key))
            {
                throw new Exception(String.Format("The session with key '{0}' is null", key.ToString()));
            }
            return (TSource)Session[key.ToString()];
        }

        public static object Get(SessionState key)
        {
            if (IsNull(key))
                throw new Exception(String.Format("The session with key '{0}' is null", key.ToString()));

            return Session[key.ToString()];
        }

        public static void Add<TSource>(TSource model, SessionState key)
        {
            if (DoesKeyExist(key))
                throw new Exception(String.Format("The session key '{0}' is already been used, try using another key",  key.ToString()));

            Session.Add(key.ToString(), model);
        }

        public static void Add(Dictionary<SessionState, object> models)
        {
            foreach (KeyValuePair<SessionState, object> item in models)
            {
                if (DoesKeyExist(item.Key))
                    throw new Exception(String.Format("The session key '{0}' is already been used, try using another key", item.Key.ToString()));

                Session.Add(item.Key.ToString(), item.Value);
            }
        }

        public static void Replace<TSource>(TSource model, SessionState key)
        {
            if (!DoesKeyExist(key))
            {
                throw new Exception(String.Format("The session key '{0}' is not been used yet", key.ToString()));
            }

            if (!IsNull(key) && (model.GetType() != Session[key.ToString()].GetType()))
            {
                throw new Exception(
                    String.Format("The old data type of session key '{0}' is not matching with the new data type",
                        key.ToString()));
            }
            Session[key.ToString()] = model;
        }

        public static void Remove(SessionState key)
        {
            if (!DoesKeyExist(key))
            {
                throw new Exception(
                    String.Format("The session with the key '{0}' is already been removed, or not used yet", key.ToString()));
            }
            Session.Remove(key.ToString());
        }

        public static string GetSessionId()
        {
            return Session.SessionID;
        }

        public static bool HasAnySessions()
        {
            return Session.Count > 0;
        }

        public static void RemoveAll()
        {
            Session.RemoveAll();
        }

        public static void AbandonSessions()
        {
            Session.Abandon();
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
