using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

namespace NostreetsExtensions.Utilities
{
    public class SessionManager
    {
        public static HttpSessionState Sessions => HttpContext.Current.Session;

        public static bool DoesKeyExist(SessionState key)
        {
            if (!HasAnySessions())
            {
                return false;
            }
            bool exist = false;
            for (int i = 0; i < Sessions.Count; i++)
            {
                exist = Sessions.Keys[i] == key.ToString();
                if (exist)
                {
                    break;
                }
            }
            return exist;
        }

        public static bool IsNull(SessionState key)
        {
            return Sessions[key.ToString()] == null;
        }

        public static void SetNull(SessionState key)
        {
            Sessions[key.ToString()] = null;
        }

        public static TSource Get<TSource>(SessionState key)
        {
            if (IsNull(key))
            {
                throw new Exception(String.Format("The session with key '{0}' is null", key.ToString()));
            }
            return (TSource)Sessions[key.ToString()];
        }

        public static object Get(SessionState key)
        {
            if (IsNull(key))
            {
                throw new Exception(String.Format("The session with key '{0}' is null", key.ToString()));
            }
            return Sessions[key.ToString()];
        }

        public static void Add<TSource>(TSource model, SessionState key)
        {
            if (DoesKeyExist(key))
            {
                throw new Exception(String.Format("The session key '{0}' is already been used, try using another key",
                    key.ToString()));
            }
            Sessions.Add(key.ToString(), model);
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
                Sessions.Add(item.Key.ToString(), item.Value);
            }
        }

        public static void Replace<TSource>(TSource model, SessionState key)
        {
            if (!DoesKeyExist(key))
            {
                throw new Exception(String.Format("The session key '{0}' is not been used yet", key.ToString()));
            }

            if (!IsNull(key) && (model.GetType() != Sessions[key.ToString()].GetType()))
            {
                throw new Exception(
                    String.Format("The old data type of session key '{0}' is not matching with the new data type",
                        key.ToString()));
            }
            Sessions[key.ToString()] = model;
        }

        public static void Remove(SessionState key)
        {
            if (!DoesKeyExist(key))
            {
                throw new Exception(
                    String.Format("The session with the key '{0}' is already been removed, or not used yet", key.ToString()));
            }
            Sessions.Remove(key.ToString());
        }

        public static string GetSessionId()
        {
            return Sessions.SessionID;
        }

        public static bool HasAnySessions()
        {
            return Sessions.Count > 0;
        }

        public static void RemoveAll()
        {
            Sessions.RemoveAll();
        }

        public static void AbandonSessions()
        {
            Sessions.Abandon();
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
