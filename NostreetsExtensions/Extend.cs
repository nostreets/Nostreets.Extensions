using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Serialization;
using System.Net.Http.Headers;
using System.Linq.Expressions;
using System.Collections.Specialized;
using System.Web;
using NostreetsExtensions.Utilities;
using System.Text;
using Newtonsoft.Json.Serialization;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using NostreetsExtensions.Interfaces;
using System.Diagnostics;
using RestSharp;
using RestSharp.Authenticators;
using Unity;
using Unity.Resolution;
using Castle.Windsor;
using NostreetsExtensions.Helpers;
using System.Threading.Tasks;
using System.Xml;
using System.Web.Optimization;

namespace NostreetsExtensions
{

    /// <summary>
    /// 
    /// </summary>
    public static class Extend
    {
        #region Extension Methods

        /// <summary>
        /// To the data table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="iList">The i list.</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this List<T> iList)
        {
            DataTable dataTable = new DataTable();
            List<PropertyDescriptor> propertyDescriptorCollection = TypeDescriptor.GetProperties(typeof(T)).Cast<PropertyDescriptor>().ToList();

            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];


                Type type = propertyDescriptor.PropertyType ?? typeof(int);

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);

                dataTable.Columns.Add(propertyDescriptor.Name);
                dataTable.Columns[i].AllowDBNull = true;
            }

            int id = 0;
            foreach (object iListItem in iList)
            {
                ArrayList values = new ArrayList();
                for (int i = 0; i < propertyDescriptorCollection.Count; i++)
                {
                    values.Add(
                        propertyDescriptorCollection[i].GetValue(iListItem) == null && propertyDescriptorCollection[i].PropertyType == typeof(string)
                        ? String.Empty
                        : (i == 0 && propertyDescriptorCollection[i].Name.Contains("Id") && propertyDescriptorCollection[i].PropertyType == typeof(int))
                        ? id += 1
                        : propertyDescriptorCollection[i].GetValue(iListItem) == null
                        ? DBNull.Value
                        : propertyDescriptorCollection[i].GetValue(iListItem));

                }
                dataTable.Rows.Add(values.ToArray());

                values = null;
            }

            return dataTable;
        }

        /// <summary>
        /// To the data table.
        /// </summary>
        /// <param name="iList">The i list.</param>
        /// <param name="objType">Type of the object.</param>
        /// <returns></returns>
        public static DataTable ToDataTable(this List<object> iList, Type objType)
        {
            DataTable dataTable = new DataTable();
            List<PropertyDescriptor> propertyDescriptorCollection = TypeDescriptor.GetProperties(objType).Cast<PropertyDescriptor>().ToList();

            for (int i = 0; i < propertyDescriptorCollection.Count; i++)
            {
                PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i];


                Type type = propertyDescriptor.PropertyType ?? typeof(int);

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = Nullable.GetUnderlyingType(type);

                dataTable.Columns.Add(propertyDescriptor.Name);
                dataTable.Columns[i].AllowDBNull = true;
            }

            int id = 0;
            foreach (object iListItem in iList)
            {
                ArrayList values = new ArrayList();
                for (int i = 0; i < propertyDescriptorCollection.Count; i++)
                {
                    values.Add(
                        propertyDescriptorCollection[i].GetValue(iListItem) == null && propertyDescriptorCollection[i].PropertyType == typeof(string)
                        ? String.Empty
                        : (i == 0 && propertyDescriptorCollection[i].Name.Contains("Id") && propertyDescriptorCollection[i].PropertyType == typeof(int))
                        ? id += 1
                        : propertyDescriptorCollection[i].GetValue(iListItem) == null
                        ? DBNull.Value
                        : propertyDescriptorCollection[i].GetValue(iListItem));

                }
                dataTable.Rows.Add(values.ToArray());

                values = null;
            }

            return dataTable;
        }

        /// <summary>
        /// To the date time.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static DateTime ToDateTime(this string obj, string format = null)
        {
            if (format != null) { return DateTime.ParseExact(obj, format, CultureInfo.InvariantCulture); }
            else { return Convert.ToDateTime(obj); }
        }

        /// <summary>
        /// Starts the of week.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static DateTime StartOfWeek(this DateTime dt)
        {
            DayOfWeek firstDay = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            DateTime firstDayInWeek = dt.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);

            return firstDayInWeek;
        }

        /// <summary>
        /// Ends the of week.
        /// </summary>
        /// <param name="dt">The dt.</param>
        /// <returns></returns>
        public static DateTime EndOfWeek(this DateTime dt)
        {
            DateTime start = StartOfWeek(dt);
            return start.AddDays(6);
        }

        /// <summary>
        /// Hits the endpoint.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        /// <param name="data">The data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="headers">The headers.</param>
        /// <returns></returns>
        /// <exception cref="Exception">url to has to be valid url string to be able to HitEndpoint...</exception>
        public static object HitEndpoint(this string url, string method = "GET", object data = null, string contentType = "application/json", Dictionary<string, string> headers = null)
        {
            if (!url.IsValidUrl())
                throw new Exception("url to has to be valid url string to be able to HitEndpoint...");

            HttpWebRequest requestStream = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse responseStream = null;
            string responseString = null,
                   requestString = null;

            requestStream.ContentType = contentType;
            requestStream.Method = method;

            foreach (KeyValuePair<string, string> head in headers)
            {
                requestStream.Headers.Add(head.Key, head.Value);
            }

            try
            {
                if (data != null)
                {
                    if (method == "POST" || method == "PUT" || method == "PATCH")
                    {
                        if (contentType == "application/json")
                        {
                            requestString = JsonConvert.SerializeObject(data);


                        }
                        else
                        {
                            XmlSerializer serial = new XmlSerializer(data.GetType());
                            StringWriter writer = new StringWriter();
                            serial.Serialize(writer, data);
                            requestString = writer.ToString();
                            writer.Close();
                        }

                        using (Stream stream = requestStream.GetRequestStream())
                        {
                            StreamWriter writer = new StreamWriter(stream);
                            writer.Write(requestString);
                            writer.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex;
            }

            try
            {
                using (responseStream = (HttpWebResponse)requestStream.GetResponse())
                {
                    using (Stream stream = responseStream.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseString = reader.ReadToEnd();
                    }

                    object responseData;

                    if (contentType == "application/json")
                    {
                        responseData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseString);
                    }
                    else
                    {
                        XmlSerializer serial = new XmlSerializer(data.GetType());
                        StringReader reader = new StringReader(responseString);
                        responseData = serial.Deserialize(reader);
                    }
                    return responseData;
                }
            }
            catch (Exception ex)
            {
                if (FileManager.LatestInstance != null)
                    FileManager.LatestInstance.WriteToFile("\n----------------------   Error Details    ---------------------\nRequest: \n{0}\n\n Response: \n {1}\n", requestString, responseString);

                return ex;
            }
        }

        /// <summary>
        /// Hits the endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        /// <param name="data">The data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="headers">The headers.</param>
        /// <returns></returns>
        /// <exception cref="Exception">url to has to be valid url string to be able to HitEndpoint...
        /// or</exception>
        public static T HitEndpoint<T>(this string url, string method = "GET", object data = null, string contentType = "application/json", Dictionary<string, string> headers = null)
        {
            if (!url.IsValidUrl())
                throw new Exception("url to has to be valid url string to be able to HitEndpoint...");

            HttpWebRequest requestStream = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse responseStream = null;
            string responseString = null,
                   requestString = null;
            byte[] bytes = null;

            if (headers == null) { headers = new Dictionary<string, string>(); }

            requestStream.ContentType = contentType;
            requestStream.Method = method;

            foreach (KeyValuePair<string, string> head in headers)
            {
                requestStream.Headers.Add(head.Key, head.Value);
            }

            try
            {
                if (data != null)
                {
                    if (method == "POST" || method == "PUT" || method == "PATCH")
                    {
                        if (contentType == "application/json")
                        {
                            requestString = JsonConvert.SerializeObject(data);
                        }
                        else if (contentType == "text/xml; encoding='utf-8'")
                        {
                            XmlSerializer serial = new XmlSerializer(data.GetType());
                            StringWriter writer = new StringWriter();
                            serial.Serialize(writer, data);
                            requestString = "XML=" + writer.ToString();
                            writer.Close();
                        }
                    }

                    using (Stream stream = requestStream.GetRequestStream())
                    {
                        StreamWriter writer = new StreamWriter(stream);
                        if (requestString != null) { writer.Write(requestString); }
                        else if (bytes != null) { stream.Write(bytes, 0, bytes.Length); }
                        writer.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            try
            {
                using (responseStream = (HttpWebResponse)requestStream.GetResponse())
                {
                    using (Stream stream = responseStream.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseString = reader.ReadToEnd();
                    }

                    T responseData;

                    if (contentType == "application/json")
                    {
                        responseData = JsonConvert.DeserializeObject<T>(responseString);
                    }
                    else
                    {
                        XmlSerializer serial = new XmlSerializer(typeof(T));
                        StringReader reader = new StringReader(responseString); //XmlReader.Create(responseString);
                        responseData = (T)serial.Deserialize(reader);
                    }

                    if (responseString.ToLower().Contains("<error>"))
                    { throw new Exception(responseString); }

                    return responseData;
                }
            }
            catch (Exception ex)
            {
                if (FileManager.LatestInstance != null)
                    FileManager.LatestInstance.WriteToFile("\n----------------------   Error Details    ---------------------\nRequest: \n{0}\n\n Response: \n {1}\n", requestString, responseString);

                throw ex;
            }

        }

        /// <summary>
        /// Hits the endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        /// <param name="data">The data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="headers">The headers.</param>
        /// <returns></returns>
        /// <exception cref="Exception">url to has to be valid url string to be able to HitEndpoint...
        /// or</exception>
        public static IRestResponse<T> RestSharpEndpoint<T>(this string url, string method = "GET", object data = null, string contentType = "application/json", Dictionary<string, string> headers = null) where T : new()
        {
            #region Client

            RestClient rest = null;
            if (url != null)
            {
                rest = new RestClient(url);
            }
            else { throw new Exception("URL is not defined!"); }

            #endregion


            #region Request

            RestRequest request = new RestRequest();
            switch (method)
            {
                case "GET":
                    request.Method = Method.GET;
                    break;

                case "POST":
                    request.Method = Method.POST;
                    break;

                case "PATCH":
                    request.Method = Method.PATCH;
                    break;

                case "PUT":
                    request.Method = Method.PUT;
                    break;

                case "DELETE":
                    request.Method = Method.DELETE;
                    break;
                default:
                    request.Method = Method.GET;
                    break;
            };
            request.JsonSerializer = CustomSerializer.CamelCaseIngoreDictionaryKeys;
            request.RequestFormat = DataFormat.Json;
            request.AddBody(data);
            if (headers != null)
            {
                foreach (var item in headers)
                {
                    if (item.Key.Contains("auth"))
                    {
                        rest.Authenticator = new HttpBasicAuthenticator("username", item.Value);
                    }
                    else if (item.Key == "contentType")
                    {
                        request.AddParameter(new Parameter { ContentType = item.Value });
                    }
                    else
                    {
                        request.AddParameter(new Parameter { Name = item.Key, Value = item.Value });
                    }
                }
            }
            #endregion


            return rest.Execute<T>(request);

        }

        /// <summary>
        /// Gets the week of month.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public static int GetWeekOfMonth(this DateTime time)
        {
            DateTime first = new DateTime(time.Year, time.Month, 1);
            return time.GetWeekOfYear() - first.GetWeekOfYear() + 1;
        }

        /// <summary>
        /// Gets the week of year.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public static int GetWeekOfYear(this DateTime time)
        {
            GregorianCalendar _gc = new GregorianCalendar();
            return _gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

        /// <summary>
        /// Determines whether [is week of month] [the specified pay day].
        /// </summary>
        /// <param name="week">The week.</param>
        /// <param name="payDay">The pay day.</param>
        /// <returns>
        ///   <c>true</c> if [is week of month] [the specified pay day]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsWeekOfMonth(this int week, DateTime payDay)
        {
            bool result = false;
            int weekOfPay = GetWeekOfMonth(payDay);

            while (week >= weekOfPay)
            {
                week -= 4;
                if (week == weekOfPay) { result = true; }
            }

            return result;

        }

        /// <summary>
        /// Runs the power shell command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="parameters">The parameters.</param>
        public static void RunPowerShellCommand(this string command, params string[] parameters)
        {
            string script = "Set-ExecutionPolicy -Scope Process -ExecutionPolicy Unrestricted; Get-ExecutionPolicy"; // the second command to know the ExecutionPolicy level

            using (PowerShell powershell = PowerShell.Create())
            {
                powershell.AddScript(script);
                var someResult = powershell.Invoke();


                powershell.AddCommand(command);
                powershell.AddParameters(parameters);
                var results = powershell.Invoke();
            }
        }

        /// <summary>
        /// Adds the attribute.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="addToBaseObj">if set to <c>true</c> [affect base object].</param>
        /// <param name="attributeParams">The attribute parameters.</param>
        /// <param name="affectedFields">The affected fields.</param>
        public static void AddAttribute<T>(this object obj, Dictionary<Type, object> attributeParams, bool addToBaseObj = true, FieldInfo[] affectedFields = null) where T : Attribute
        {
            Type type = obj.GetType();

            AssemblyName aName = new AssemblyName("SomeNamespace");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(aName.Name);
            TypeBuilder affectedType = moduleBuilder.DefineType(type.Name + Guid.NewGuid().ToString(), TypeAttributes.Public, type);


            Type[] attrParams = attributeParams.Keys.ToArray();
            ConstructorInfo attrConstructor = typeof(T).GetConstructor(attrParams);
            CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(attrConstructor, attributeParams.Values.ToArray());


            if (addToBaseObj)
                affectedType.SetCustomAttribute(attrBuilder);

            if (affectedFields != null && affectedFields.Length > 1)
            {
                foreach (FieldInfo field in affectedFields)
                {
                    FieldBuilder firstNameField = affectedType.DefineField(field.Name, field.FieldType, (field.IsPrivate) ? FieldAttributes.Private : FieldAttributes.Public);
                    firstNameField.SetCustomAttribute(attrBuilder);
                }
            }


            Type newType = affectedType.CreateType();
            object instance = Activator.CreateInstance(newType);


            obj = instance;

        }

        public static object AddAttribute(this object obj, Tuple<string, Type, object[]>[] attributeParams, bool addToBaseObj = true)
        {
            object result = null;
            if (attributeParams.Any(a => !a.Item2.IsSubclassOf(typeof(Attribute))))
                throw new InvalidDataException("attributeParams must only have Type's of Attributes...");

            Type type = obj.GetType();
            PropertyInfo[] props = type.GetProperties();

            result = ClassBuilder.CreateObject(
                                type.Name
                                , props.Select(a => a.Name).ToArray()
                                , props.Select(a => a.PropertyType).ToArray()
                                , attributeParams
                            );

            foreach (PropertyInfo prop in props)
                result.SetPropertyValue(prop.Name, obj.GetPropertyValue(prop.Name));

            return result;
        }

        /// <summary>
        /// Determines whether this instance is odd.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is odd; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsOdd(this int value)
        {
            return value % 2 != 0;
        }

        /// <summary>
        /// Determines whether this instance is even.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is even; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEven(this int value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// To the dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Type must be an enum</exception>
        public static Dictionary<int, string> ToDictionary<T>(this Type enumType) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentNullException("Type must be an enum");

            return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(t => (int)(object)t, t => t.ToString());
        }

        /// <summary>
        /// To the dictionary.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Type must be an enum</exception>
        public static Dictionary<int, string> ToDictionary(this Type enumType)
        {
            Dictionary<int, string> result = null;
            if (!enumType.IsEnum)
                throw new ArgumentNullException("Type must be an enum");

            string[] arr = Enum.GetNames(enumType);

            foreach (string enumName in arr)
            {
                if (result == null)
                    result = new Dictionary<int, string>();

                result.Add(enumType.GetEnumValue(enumName), enumName);
            }

            return result;
        }

        /// <summary>
        /// To the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="coll">The coll.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> coll)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();

            if (coll != null && coll.ToArray().Length > 0)
                foreach (KeyValuePair<TKey, TValue> pair in coll.ToArray())
                    result.Add(pair.Key, pair.Value);

            return result;
        }

        /// <summary>
        /// Gets the enum value.
        /// </summary>
        /// <param name="enumType">Type of the enum.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static int GetEnumValue(this Type enumType, string name)
        {
            return (int)Enum.Parse(enumType, name);
        }

        /// <summary>
        /// Gets the enum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static int GetEnumValue<T>(this string name)
        {
            return (int)Enum.Parse(typeof(T), name);
        }

        /// <summary>
        /// Gets the query strings.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetQueryStrings(this HttpRequestMessage request)
        {
            return request.GetQueryNameValuePairs()
                          .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetQueryString(this HttpRequestMessage request, string key)
        {
            // IEnumerable<KeyValuePair<string,string>> - right!
            var queryStrings = request.GetQueryNameValuePairs();
            if (queryStrings == null)
                return null;

            var match = queryStrings.FirstOrDefault(kv => string.Compare(kv.Key, key, true) == 0);
            if (string.IsNullOrEmpty(match.Value))
                return null;

            return match.Value;
        }

        /// <summary>
        /// Gets the header.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static string GetHeader(this HttpRequestMessage request, string key)
        {
            IEnumerable<string> keys = null;
            if (!request.Headers.TryGetValues(key, out keys))
                return null;

            return keys.First();
        }

        /// <summary>
        /// Gets the cookie.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <returns></returns>
        public static string GetCookie(this HttpRequestMessage request, string cookieName)
        {
            CookieHeaderValue cookie = request.Headers.GetCookies(cookieName).FirstOrDefault() ?? default(CookieHeaderValue);

            return cookie[cookieName].Value;
        }

        /// <summary>
        /// Gets the cookie.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <returns></returns>
        public static string GetCookie(this HttpRequest request, string cookieName)
        {
            string result = null;
            System.Web.HttpCookie cookie = request.Cookies[cookieName] ?? default(System.Web.HttpCookie);
            if (cookie != null)
            {
                result = cookie.Value;
            }

            return result;
        }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="value">The value.</param>
        /// <param name="expires">The expires.</param>
        public static void SetCookie(this HttpContext context, string cookieName, string value, DateTime? expires = null)
        {
            try
            {
                System.Web.HttpCookie cookie = new System.Web.HttpCookie(cookieName, value);
                cookie.Expires = (expires != null) ? expires.Value : default(DateTime);

                context.Response.Cookies.Add(cookie);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Sets the cookie.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="values">The values.</param>
        /// <param name="expires">The expires.</param>
        public static void SetCookie(this HttpContext context, string cookieName, Dictionary<string, string> values, DateTime? expires = null)
        {
            try
            {
                string value = null;
                if (values != null && values.Count > 0)
                {
                    int i = 0;
                    foreach (KeyValuePair<string, string> val in values)
                    {
                        value += String.Format((i == values.Count - 1) ? "{0}={1}" : "{0}={1}, ", val.Key, val.Value); i++;
                    }
                }

                System.Web.HttpCookie cookie = new System.Web.HttpCookie(cookieName, value);
                cookie.Expires = (expires != null) ? expires.Value : default(DateTime);


                context.Response.Cookies.Add(cookie);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// To the delegate.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static Delegate ToDelegate(this MethodInfo obj, object target = null)
        {

            Type delegateType;

            var typeArgs = obj.GetParameters()
                .Select(p => p.ParameterType)
                .ToList();

            // builds a delegate type
            if (obj.ReturnType == typeof(void))
            {
                delegateType = Expression.GetActionType(typeArgs.ToArray());

            }
            else
            {
                typeArgs.Add(obj.ReturnType);
                delegateType = Expression.GetFuncType(typeArgs.ToArray());
            }

            // creates a binded delegate if target is supplied
            var result = (target == null)
                ? Delegate.CreateDelegate(delegateType, obj)
                : Delegate.CreateDelegate(delegateType, target, obj);

            return result;
        }

        /// <summary>
        /// To the name value collection.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in dict)
            {
                string value = null;
                if (kvp.Value != null)
                    value = kvp.Value.ToString();

                nameValueCollection.Add(kvp.Key.ToString(), value);
            }

            return nameValueCollection;
        }

        /// <summary>
        /// Determines whether the specified property name has duplicates.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>
        ///   <c>true</c> if the specified property name has duplicates; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasDuplicates<T>(this IEnumerable<T> enumerable, string propertyName) where T : class
        {

            List<object> dict = new List<object>();
            foreach (var item in enumerable)
            {
                object key = item.GetPropertyValue(propertyName);

                if (!dict.Contains(key))
                    dict.Add(key);

                else
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the types with.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="app">The application.</param>
        /// <param name="searchDervied">if set to <c>true</c> [search dervied].</param>
        /// <returns></returns>
        public static List<Type> GetTypesWith<TAttribute>(this AppDomain app, bool searchDervied) where TAttribute : Attribute
        {
            //return from a in AppDomain.CurrentDomain.GetAssemblies()
            //       from t in a.GetTypes()
            //       where t.IsDefined(typeof(TAttribute), searchDervied)
            //       select t;

            List<Type> result = new List<Type>();
            var assemblies = app.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var item in types)
                {
                    if (Attribute.GetCustomAttribute(item, typeof(TAttribute)) != null) { result.Add(item); }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the duplicates.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static Dictionary<object, T[]> GetDuplicates<T>(this IEnumerable<T> enumerable, string propertyName) where T : class
        {

            Dictionary<object, List<T>> dict = new Dictionary<object, List<T>>();

            foreach (T item in enumerable)
            {
                object key = item.GetPropertyValue(propertyName);
                if (!dict.ContainsKey(key))
                {
                    dict.Add(key, new List<T> { item });
                }
                else
                {
                    dict[item.GetPropertyValue(propertyName)].Add(item);
                }
            }

            Dictionary<object, T[]> duplicates = new Dictionary<object, T[]>();

            foreach (var value in dict)
            {
                if (value.Value.Count > 1)
                {
                    duplicates.Add(value.Key, value.Value.ToArray());
                }
            }

            return duplicates;
        }

        /// <summary>
        /// Determines whether [is action delegate].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns>
        ///   <c>true</c> if [is action delegate] [the specified source]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsActionDelegate<T>(this T source)
        {
            return typeof(T).FullName.StartsWith("System.Action");
        }

        /// <summary>
        /// Determines whether the specified output is type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="output">The output.</param>
        /// <returns>
        ///   <c>true</c> if the specified output is type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsType<T>(this object obj, out T output) where T : class
        {
            bool result = false;
            output = null;
            if (typeof(T) == obj.GetType())
            {
                result = true;
                output = (T)obj;
            }

            return result;
        }

        /// <summary>
        /// Determines whether the specified type is type.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type is type; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsType(this object obj, Type type)
        {
            bool result = false;
            if (type == obj.GetType())
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Gets the method information.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Expression is not a method - expression</exception>
        public static MethodInfo GetMethodInfo<T, T2>(this Expression<Func<T, T2>> expression)
        {
            var member = expression.Body as MethodCallExpression;

            if (member != null)
                return member.Method;

            throw new ArgumentNullException("Expression is not a method", "expression");
        }

        /// <summary>
        /// Gets the method information.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Expression is not a method - expression</exception>
        public static MethodInfo GetMethodInfo<T>(this Expression<Action<T>> expression)
        {
            var member = expression.Body as MethodCallExpression;

            if (member != null)
                return member.Method;

            throw new ArgumentNullException("Expression is not a method", "expression");
        }

        /// <summary>
        /// Gets the method information.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="searchSettings">The search settings.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(this object obj, string methodName, BindingFlags searchSettings = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
        {
            return obj.GetType().GetMethod(methodName, searchSettings);
        }

        /// <summary>
        /// Gets the method information.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="searchSettings">The search settings.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(this Type type, string methodName, BindingFlags searchSettings = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)
        {
            return type.GetMethod(methodName, searchSettings);
        }

        /// <summary>
        /// Gets the method information.
        /// </summary>
        /// <param name="fullMethodName">Full name of the method.</param>
        /// <returns></returns>
        public static MethodInfo GetMethodInfo(this string fullMethodName)
        {
            return (MethodInfo)fullMethodName.ScanAssembliesForObject();
        }

        public static List<Tuple<TAttribute, object, Assembly>> GetObjectsWithAttribute<TAttribute>(this Assembly assembly, ClassTypes section) where TAttribute : Attribute
        {
            List<Tuple<TAttribute, object, Assembly>> result = new List<Tuple<TAttribute, object, Assembly>>();
            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
                foreach (var item in scanner.ScanForAttributes(assembly, section))
                    result.Add(new Tuple<TAttribute, object, Assembly>(item.Item1, item.Item2, item.Item4));

            return result;
        }

        /// <summary>
        /// Gets the objects by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="section">The section.</param>
        /// <param name="type">The type.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<object> GetObjectsByAttribute<TAttribute>(this Assembly assembly, ClassTypes section, Type type = null) where TAttribute : Attribute
        {
            List<object> result = new List<object>();
            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
            {
                foreach (var item in scanner.ScanForAttributes(assembly, section, type))
                    result.Add(item.Item2);
            }

            return result;
        }

        /// <summary>
        /// Gets the methods by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<MethodInfo> GetMethodsByAttribute<TAttribute>(this Assembly assembly, Type type = null) where TAttribute : Attribute
        {

            List<MethodInfo> result = new List<MethodInfo>();
            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
            {
                foreach (var item in scanner.ScanForAttributes(assembly, ClassTypes.Methods, type))
                    result.Add((MethodInfo)item.Item2);
            }

            return result;
        }

        public static List<MethodInfo> GetMethodsByAttribute<TAttribute>(this Assembly assembly, IEnumerable<Type> types = null) where TAttribute : Attribute
        {
            List<MethodInfo> result = new List<MethodInfo>();
            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
            {
                if (types != null)
                {
                    foreach (Type type in types)
                        foreach (var item in scanner.ScanForAttributes(assembly, ClassTypes.Methods))
                            result.Add((MethodInfo)item.Item2);
                }
                else
                    foreach (var item in scanner.ScanForAttributes(Assembly.GetCallingAssembly(), ClassTypes.Methods))
                        result.Add((MethodInfo)item.Item2);
            }

            return result;
        }

        public static List<MethodInfo> GetMethodsByAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            List<MethodInfo> result = new List<MethodInfo>();
            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
            {
                foreach (var item in scanner.ScanForAttributes(Assembly.GetCallingAssembly(), ClassTypes.Methods, type))
                    result.Add((MethodInfo)item.Item2);
            }

            return result;
        }

        public static List<MethodInfo> GetMethodsByAttribute<TAttribute>(this IEnumerable<Type> types) where TAttribute : Attribute
        {
            List<MethodInfo> result = new List<MethodInfo>();
            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
            {
                foreach (Type type in types)
                    foreach (var item in scanner.ScanForAttributes(Assembly.GetCallingAssembly(), ClassTypes.Methods, type))
                        result.Add((MethodInfo)item.Item2);
            }

            return result;
        }

        /// <summary>
        /// Gets the types by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<Type> GetTypesByAttribute<TAttribute>(this Assembly assembly, Type type = null) where TAttribute : Attribute
        {
            List<Type> result = new List<Type>();

            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
            {
                foreach (var item in scanner.ScanForAttributes(assembly, ClassTypes.Type, type))
                    result.Add((Type)item.Item2);
            }

            return result;
        }

        /// <summary>
        /// Gets the properties by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<PropertyInfo> GetPropertiesByAttribute<TAttribute>(this Assembly assembly, Type type = null) where TAttribute : Attribute
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
            {
                foreach (var item in scanner.ScanForAttributes(assembly, ClassTypes.Properties, type))
                    result.Add((PropertyInfo)item.Item2);
            }

            return result;
        }

        /// <summary>
        /// Gets the properties by attribute.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="type">The type.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <returns></returns>
        public static List<PropertyInfo> GetPropertiesByAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            List<PropertyInfo> result = null;

            using (AttributeScanner<TAttribute> scanner = new AttributeScanner<TAttribute>())
            {
                foreach (var item in scanner.ScanForAttributes(Assembly.GetCallingAssembly(), ClassTypes.Properties, type))
                {
                    if (result == null)
                        result = new List<PropertyInfo>();

                    result.Add((PropertyInfo)item.Item2);
                }
            }

            return result;
        }

        /// <summary>
        /// Instantiates the specified parameters.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object Instantiate(this Type type, params object[] parameters)
        {
            try
            {
                if (type == typeof(string))
                    return Activator.CreateInstance(typeof(string), Char.MinValue, 0);
                else
                        if (parameters.Length > 0 && parameters[0] != null)
                    return Activator.CreateInstance(type, parameters);

                else
                    return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Instantiates the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static T Instantiate<T>(this T type)
        {
            return (T)typeof(T).Instantiate();
        }

        public static void GetConstructorParams(this Type type, object[] param = null)
        {
            if (param == null)
                param = new object[0];

            ConstructorInfo result = null;
            ConstructorInfo[] constructors = type.GetConstructors();
            foreach (ConstructorInfo con in constructors)
            {
                ParameterInfo[] parameters = con.GetParameters();
                if (parameters.Length == param.Length)
                {
                    for (int i = 0; i < param.Length; i++)
                    {
                        if (param[i].GetType() != parameters[i].ParameterType)
                            break;

                        if (i == param.Length - 1)
                            result = con;
                    }
                }
            }

        }

        public static object SafeResolve(this Type type, IUnityContainer containter)
        {
            bool canResovle = false;
            ParameterInfo[] parameters = type.GetConstructors()[0].GetParameters();
            foreach (var par in parameters)
            {
                bool matched = false;
                foreach (var reg in containter.Registrations)
                    if (!matched && par.ParameterType == reg.RegisteredType)
                        matched = true;


                if (!matched)
                    break;
                else if (parameters[parameters.Length - 1] == par)
                    canResovle = true;
            }

            return (canResovle || parameters.Length == 0) ? containter.Resolve(type) : type.Instantiate();
        }

        public static object WindsorResolve(this Type type, IWindsorContainer containter)
        {
            return containter.Resolve(type);
        }

        public static object WindsorResolve(this object obj, IWindsorContainer containter)
        {
            bool resolved = obj.GetType().TryWindsorResolve(containter, out object instance);
            return resolved ? instance : null;
        }

        public static T WindsorResolve<T>(this T obj, IWindsorContainer containter)
        {
            bool resolved = typeof(T).TryWindsorResolve(containter, out object instance);
            return resolved ? (T)instance : default(T);
        }

        public static T WindsorResolve<T>(this T obj, Assembly assembly)
        {
            IWindsorContainer containter = assembly.GetWindsorContainer();
            bool resolved = typeof(T).TryWindsorResolve(containter, out object instance);
            return resolved ? (T)instance : default(T);
        }

        public static object WindsorResolve(this Type type, Assembly assembly)
        {
            IWindsorContainer containter = assembly.GetWindsorContainer();
            return containter.Resolve(type);
        }

        public static object WindsorResolve(this object obj, Assembly assembly)
        {
            IWindsorContainer containter = assembly.GetWindsorContainer();
            bool resolved = obj.GetType().TryWindsorResolve(containter, out object instance);
            return resolved ? instance : null;
        }

        public static T WindsorResolve<T>(this T obj, string assemblyName)
        {
            IWindsorContainer containter = assemblyName.GetWindsorContainer();
            bool resolved = typeof(T).TryWindsorResolve(containter, out object instance);
            return resolved ? (T)instance : default(T);
        }

        public static object WindsorResolve(this Type type, string assemblyName)
        {
            IWindsorContainer containter = assemblyName.GetWindsorContainer();
            return containter.Resolve(type);
        }

        public static object WindsorResolve(this object obj, string assemblyName)
        {
            IWindsorContainer containter = assemblyName.GetWindsorContainer();
            bool resolved = obj.GetType().TryWindsorResolve(containter, out object instance);
            return resolved ? instance : null;
        }

        public static bool TryWindsorResolve(this Type type, IWindsorContainer containter, out object instance)
        {
            bool result = false;
            try
            {
                instance = containter.Resolve(type);
                result = true;
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // Variable is declared but never used
            {
                instance = null;
            }
            return result;
        }

        public static bool TryUnityResolve(this Type type, IUnityContainer containter, out object instance)
        {
            bool result = false;
            try
            {
                instance = containter.Resolve(type);
                result = true;
            }
            catch (Exception)
            {
                instance = null;
            }
            return result;
        }

        public static object UnityResolve(this Type type, IUnityContainer containter)
        {
            return containter.Resolve(type);
        }

        public static object UntityResolve(this Type type, IUnityContainer container, object overrideObject = null)
        {
            PropertyInfo[] properties = overrideObject?.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            ResolverOverride[] overridesArray = properties?.Select(p => new ParameterOverride(p.Name, p.GetValue(overrideObject, null))).Cast<ResolverOverride>().ToArray();

            return (overrideObject != null)
                    ? container.Resolve(type, overridesArray)
                    : container.Resolve(type);
        }

        /// <summary>
        /// Scans the assemblies for object.
        /// </summary>
        /// <param name="nameToCheckFor">The name to check for.</param>
        /// <param name="assemblyToLookFor">The assembly to look for.</param>
        /// <returns></returns>
        public static object ScanAssembliesForObject(this string nameToCheckFor, string assemblyToLookFor = null, ClassTypes classType = ClassTypes.Any)
        {
            object result = null;
            using (AssemblyScanner scanner = new AssemblyScanner())
                result = scanner.ScanAssembliesForObject(nameToCheckFor, (assemblyToLookFor == null) ? new[] { assemblyToLookFor } : null, null, classType);
            return result;
        }

        /// <summary>
        /// Scans the assemblies for object.
        /// </summary>
        /// <param name="nameToCheckFor">The name to check for.</param>
        /// <param name="assembliesToLookFor">The assemblies to look for.</param>
        /// <returns></returns>
        public static object ScanAssembliesForObject(this string nameToCheckFor, string[] assembliesToLookFor, ClassTypes classType = ClassTypes.Any)
        {
            object result = null;
            using (AssemblyScanner scanner = new AssemblyScanner())
                result = scanner.ScanAssembliesForObject(nameToCheckFor, assembliesToLookFor, null, classType);
            return result;
        }

        /// <summary>
        /// Scans the assemblies for object.
        /// </summary>
        /// <param name="nameToCheckFor">The name to check for.</param>
        /// <param name="assembliesToSkip">The assemblies to skip.</param>
        /// <param name="assembliesToLookFor">The assemblies to look for.</param>
        /// <returns></returns>
        public static object ScanAssembliesForObject(this string nameToCheckFor, string[] assembliesToSkip, string[] assembliesToLookFor, ClassTypes classType = ClassTypes.Any)
        {
            object result = null;
            using (AssemblyScanner scanner = new AssemblyScanner())
                result = scanner.ScanAssembliesForObject(nameToCheckFor, assembliesToLookFor, assembliesToSkip, classType);
            return result;
        }

        /// <summary>
        /// Extends the path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        public static string ExtendPath(this string path, string extension)
        {
            return Path.GetFullPath(Path.Combine(path, extension));
        }

        public static string[] ScanForFilePaths(this string dirPath, string fileExtension, params string[] fileNames)
        {
            if (fileNames == null && fileExtension == null)
                if (fileExtension == null)
                    throw new ArgumentNullException(nameof(fileExtension));

                else
                    throw new ArgumentNullException(nameof(fileNames));

            List<string> result = new List<string>();
            FileInfo[] files = ScanForFiles(dirPath, fileExtension, fileNames);

            foreach (FileInfo f in files)
                result.Add(f.FullName);

            return result.ToArray();

        }

        public static FileInfo[] ScanForFiles(this string dirPath, string fileExtension, params string[] fileNames)
        {
            if (fileNames == null && fileExtension == null)
                if (fileExtension == null)
                    throw new ArgumentNullException(nameof(fileExtension));

                else
                    throw new ArgumentNullException(nameof(fileNames));


            FileInfo[] result = null;
            using (DirectoryScanner scanner = new DirectoryScanner())
                result = scanner.SearchForFiles(dirPath, fileExtension, fileNames);


            return result;
        }

        public static FileInfo ScanForFile(this string dirPath, string fileName, string fileExtension)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            FileInfo result = null;

            using (DirectoryScanner scanner = new DirectoryScanner())
                result = scanner.SearchForFile(fileName, dirPath, fileExtension);


            return result;
        }

        public static string ScanForFilePath(this string dirPath, string fileName, string fileExtension)
        {
            if (fileName == null && fileExtension == null)
                if (fileExtension == null)
                    throw new ArgumentNullException(nameof(fileExtension));
                else
                    throw new ArgumentNullException(nameof(fileName));

            FileInfo result = null;

            using (DirectoryScanner scanner = new DirectoryScanner())
                result = scanner.SearchForFile(fileName, dirPath, fileExtension);

            return result.FullName;
        }

        public static string StepIntoDirectory(this string path, string targetFile, bool recursively = false)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));


            string result = Path.GetDirectoryName(path);

            do
            {
                string[] subDirectories = Directory.GetDirectories(path),
                         filesInFolder = Directory.GetFiles(path);


                foreach (string file in filesInFolder)
                    if (file.Contains(targetFile))
                        return file;

                foreach (string dir in subDirectories)
                    if (dir == targetFile)
                        return dir;

                if (recursively && subDirectories.Length > 0)
                {
                    foreach (string dir in subDirectories)
                    {
                        dir.StepIntoDirectory(targetFile, true);
                    }
                }
                else
                {
                    recursively = false;
                }
            }
            while (recursively);

            return result;
        }

        /// <summary>
        /// Steps the out of directory.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="foldersBack">The folders back.</param>
        /// <returns></returns>
        public static string StepOutOfDirectory(this string path, int foldersBack = 1)
        {
            Uri uri = null;
            string p = path;

            if (path.Substring(0, 8) == "file:///")
                path = path.Substring(8);


            for (var i = 0; i < foldersBack; i++)
                p = Directory.GetParent(p.IsUri(out uri) ? uri.LocalPath : p).FullName;


            return p;
        }

        public static bool IsUri(this string path, out Uri uri)
        {
            bool result = false;
            if (Uri.TryCreate(path, UriKind.Absolute, out uri))
                result = true;

            return result;
        }

        public static string FileExtention(this string path)
        {
            string[] split = path.Split('.');
            return split[split.Length - 1];
        }

        /// <summary>
        /// Catches the reflection type load exception.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        public static string CatchReflectionTypeLoadException(this ReflectionTypeLoadException ex)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Exception exSub in ex.LoaderExceptions)
            {
                sb.AppendLine(exSub.Message);
                FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                if (exFileNotFound != null)
                {
                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exFileNotFound.FusionLog);
                    }
                }
                sb.AppendLine();
            }

            string errorMessage = sb.ToString();
            return errorMessage;
        }

        /// <summary>
        /// Determines whether [contains] [the specified values].
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="values">The values.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified values]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(this IEnumerable<string> list, params string[] values)
        {
            bool result = false;
            foreach (string item in list)
            {
                result = (values.Any(a => a == item)) ? true : false;
            }

            return result;
        }

        /// <summary>
        /// Gets the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns></returns>
        public static Assembly GetAssembly(this AppDomain assembly, string assemblyName)
        {
            Assembly result = null;

            foreach (Assembly assemble in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assemble.FullName.Contains(assemblyName) || assemble.GetName().Name == assemblyName) { result = assemble; break; }
            }

            return result;
        }

        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="statusCode">The status code.</param>
        /// <param name="obj">The object.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="resolver">The resolver.</param>
        /// <param name="encoding">The encoding.</param>
        public static void CreateResponse(this HttpApplication app
                                            , HttpStatusCode statusCode
                                            , object obj
                                            , string contentType = "application/json"
                                            , IContractResolver resolver = null
                                            , Encoding encoding = null)
        {
            if (encoding == null) { encoding = Encoding.UTF8; }
            if (resolver == null) { resolver = new CamelCasePropertyNamesContractResolver(); }

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                ContractResolver = resolver
            };

            app.Response.TrySkipIisCustomErrors = true;


            string jsonObj = JsonConvert.SerializeObject(obj, settings);
            HttpResponseMessage response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(jsonObj, encoding, contentType)
            };

            app.Response.Clear();

            app.Response.ContentType = contentType;
            app.Response.StatusCode = (int)statusCode;
            app.Response.Write(jsonObj);

            app.Response.End();

        }

        /// <summary>
        /// Directories the exists.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static bool DirectoryExists(this string path)
        {
            if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1) { return false; }

            DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetFullPath(path));
            if (!directoryInfo.Exists) { return false; }


            return true;
        }

        /// <summary>
        /// Timestamps the specified time.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public static string Timestamp(this DateTime time, string format = null)
        {
            return time.ToString(format ?? "hh.mm.ss.tt MMM/dd/yy");
        }

        /// <summary>
        /// Formats the string.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="txt">The text.</param>
        /// <returns></returns>
        public static string FormatString(this string template, params string[] txt)
        {
            try { return String.Format(template, txt); }
            catch (Exception)
            {
                for (int i = 0; i < txt.Length; i++)
                    template.Replace("{" + i + "}", txt[i]);

                return template;
            }
        }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            if (obj.GetType() == typeof(Type).GetType())
                throw new Exception("obj cannot be a Type its self to be able to GetPropertyValue...");

            return obj.GetType().GetProperties().Single(pi => pi.Name == propertyName).GetValue(obj);
        }

        public static object GetPropertyValue(this object obj, int ordinal)
        {
            if (obj.GetType() == typeof(Type).GetType())
                throw new Exception("obj cannot be a Type its self to be able to GetPropertyValue...");

            return obj.GetType().GetProperties().Where((a, b) => b == ordinal).First().GetValue(obj);
        }

        /// <summary>
        /// Sets the property value.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        public static void SetPropertyValue(this object obj, int ordinal, object value)
        {
            if (obj.GetType().GetProperties().Where((a, b) => b == ordinal).Single().GetSetMethod() != null)
                obj.GetType().GetProperties().Where((a, b) => b == ordinal).Single().SetValue(obj, value);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            if (obj.GetType().GetProperties().Single(pi => pi.Name == propertyName).GetSetMethod() != null)
                obj.GetType().GetProperties().Single(pi => pi.Name == propertyName).SetValue(obj, value);
        }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static List<string> GetColumns(this DbContext dbContext, Type type)
        {
            string statment = String.Format("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME like N'{0}s'", type.Name);
            DbRawSqlQuery<string> result = dbContext.Database.SqlQuery<string>(statment);
            return result.ToList();
        }

        /// <summary>
        /// Adds the specified values.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static IEnumerable AddValues(this IEnumerable collection, params object[] values)
        {
            IEnumerable result = null;
            if (values.Length > 0)
            {
                List<object> list = collection.OfType<object>().ToList();
                list.AddRange(values);

                if (collection.GetType().IsArray)
                    result = list.ToArray();
                else
                    result = list;
            }

            return result;
        }

        /// <summary>
        /// Prepends the specified values.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static IEnumerable Prepend(this IEnumerable collection, params object[] values)
        {
            return values.Concat(collection.OfType<object>());
        }

        /// <summary>
        /// Prepends the specified values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> collection, params T[] values)
        {
            return values.Concat(collection);
        }

        /// <summary>
        /// Prepends the specified item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="item">The item.</param>
        public static void Prepend<T>(this IList<T> collection, T item)
        {
            collection.Insert(0, item);
        }

        /// <summary>
        /// Prepends the specified key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dic">The dic.</param>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> Prepend<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue item)
        {
            List<KeyValuePair<TKey, TValue>> list = dic.ToList();
            list.Insert(0, new KeyValuePair<TKey, TValue>(key, item));
            return list.ToDictionary();
        }

        /// <summary>
        /// Gets the schema.
        /// </summary>
        /// <param name="srv">The SRV.</param>
        /// <param name="dataSouce">The data souce.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        /// <exception cref="Exception">dataSouce param must not be null or return null...
        /// or
        /// dataSouce param must not be null or return null...</exception>
        public static KeyValuePair<string, Type>[] GetSchema(this ISqlExecutor srv, Func<SqlConnection> dataSouce, string tableName)
        {
            SqlDataReader reader = null;
            SqlCommand cmd = null;
            SqlConnection conn = null;
            KeyValuePair<string, Type>[] result = null;

            try
            {
                if (dataSouce == null)
                    throw new Exception("dataSouce param must not be null or return null...");

                using (conn = dataSouce())
                {
                    if (conn == null)
                        throw new Exception("dataSouce param must not be null or return null...");


                    if (conn.State != ConnectionState.Open)
                        conn.Open();


                    string query = "SELECT * FROM {0}".FormatString(tableName);
                    cmd = srv.GetCommand(conn, query);
                    cmd.CommandType = CommandType.Text;

                    if (cmd != null)
                    {
                        reader = cmd.ExecuteReader();


                        result = reader.GetSchemaTable().Rows.Cast<DataRow>().Select(
                                    c => new KeyValuePair<string, Type>(c["ColumnName"].ToString(), (Type)c["DataType"]))
                                .ToArray();

                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (conn != null && conn.State != ConnectionState.Closed)
                    conn.Close();
            }


            return result;

        }

        public static string[] GetColumnNames(this ISqlExecutor reader, Func<SqlConnection> dataSouce, string tableName)
        {
            KeyValuePair<string, Type>[] result = GetSchema(reader, dataSouce, tableName);
            return result.Select(a => a.Key).ToArray();
        }

        public static Type[] GetColumnTypes(this ISqlExecutor reader, Func<SqlConnection> dataSouce, string tableName)
        {
            KeyValuePair<string, Type>[] result = GetSchema(reader, dataSouce, tableName);
            return result.Select(a => a.Value).ToArray();
        }

        public static string[] GetColumnNames(this IDataReader reader)
        {
            return reader.GetSchemaTable().Rows.Cast<DataRow>().Select(c => c["ColumnName"].ToString()).ToArray();
        }

        public static Type[] GetColumnTypes(this IDataReader reader)
        {
            List<Type> result = new List<Type>();
            string[] columns = reader.GetColumnNames();
            for (int i = 0; i < columns.Length; i++)
            {
                result.Add(reader.GetValue(i).GetType());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="objType">Type of the object.</param>
        /// <param name="propType">Type of the property.</param>
        /// <param name="propName">Name of the property.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static Type AddProperty(this Type objType, Type propType, string propName, int index = 0)
        {
            List<string> propNames = new List<string>();
            List<Type> propTypes = new List<Type>();
            PropertyInfo[] props = objType.GetProperties().Where(a => a.Name != propName).ToArray();
            int i = 0;

            foreach (PropertyInfo prop in props)
            {
                if (i == index)
                {
                    propNames.Add(propName);
                    propTypes.Add(propType);
                }

                propNames.Add(prop.Name);//s[(addedProp) ? i - 1 : i].Name);
                propTypes.Add(prop.PropertyType);//s[(addedProp) ? i - 1 : i].PropertyType);

                i++;
            }


            return ClassBuilder.CreateType(objType.Name, propNames.ToArray(), propTypes.ToArray());

        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propType">Type of the property.</param>
        /// <param name="propName">Name of the property.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static Type AddProperty(this object obj, Type propType, string propName, int index = 0)
        {
            Type objType = obj.GetType();
            return objType.AddProperty(propType, propName, index);
        }

        /// <summary>
        /// Determines whether this instance has interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the specified type has interface; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="Exception">T has to be an interface</exception>
        public static bool HasInterface<T>(this Type type)
        {
            if (!typeof(T).IsInterface)
                throw new Exception("T has to be an interface");

            if (type.IsInterface)
                return false;

            if (typeof(T).IsAssignableFrom(type))
                return true;

            if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(T)))
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether the specified inter has interface.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="inter">The inter.</param>
        /// <returns>
        ///   <c>true</c> if the specified inter has interface; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="Exception">T has to be an interface</exception>
        public static bool HasInterface(this Type type, Type inter)
        {
            if (!inter.IsInterface)
                throw new Exception("T has to be an interface");

            if (type.IsInterface)
                return false;

            if (inter.IsAssignableFrom(type))
                return true;

            if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == inter))
                return true;

            return false;
        }

        /// <summary>
        /// Gets the type of t.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">type</exception>
        /// <exception cref="InvalidDataException">type does not implements IEnumerable</exception>
        public static Type GetTypeOfT(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (!type.HasInterface<IEnumerable>() || !type.HasInterface<ICollection>() || !type.HasInterface<IList>())
                throw new InvalidDataException("type does not implements IEnumerable");

            return type.GetGenericArguments()[0];
        }

        /// <summary>
        /// Gets the type of t.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">obj</exception>
        /// <exception cref="InvalidDataException">obj's Type does not implements IEnumerable</exception>
        public static Type GetTypeOfT(this object obj)
        {

            if (obj == null)
                throw new ArgumentNullException("obj");

            Type type = obj.GetType();

            if (!type.HasInterface<IEnumerable>() || !type.HasInterface<ICollection>() || !type.HasInterface<IList>())
                throw new InvalidDataException("obj's Type does not implements IEnumerable");

            return type.GetGenericArguments()[0];
        }

        /// <summary>
        /// Determines whether the specified exclude strings is collection.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="excludeStrings">if set to <c>true</c> [exclude strings].</param>
        /// <returns>
        ///   <c>true</c> if the specified exclude strings is collection; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCollection(this Type item, bool excludeStrings = true)
        {
            return (!excludeStrings)
                        ? (item.HasInterface<IEnumerable>() || item.HasInterface<ICollection>() || item.HasInterface<IList>())
                        : (item == typeof(string))
                        ? false
                        : (item.HasInterface<IEnumerable>() || item.HasInterface<ICollection>() || item.HasInterface<IList>());
        }

        /// <summary>
        /// Determines whether [is system type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is system type] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSystemType(this Type type)
        {
            return type.Assembly == typeof(object).Assembly;
        }

        /// <summary>
        /// Determines whether this instance is plural.
        /// </summary>
        /// <param name="txt">The text.</param>
        /// <returns>
        ///   <c>true</c> if the specified text is plural; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsPlural(this string txt)
        {
            return ((txt[txt.Length - 1] == 's') ? true : false);
        }

        /// <summary>
        /// Distincts the by.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Gets the request ip address.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static string GetIPAddress(this HttpContext context)
        {
            string result = null;
            if (context != null)
            {
                string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length != 0)
                    {
                        return addresses[0];
                    }
                }

                result = context.Request.ServerVariables["REMOTE_ADDR"];
            }

            return result;
        }

        public static string GetIPAddress(this HttpContextBase context)
        {
            string result = null;
            if (context != null)
            {
                string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length != 0)
                    {
                        return addresses[0];
                    }
                }

                result = context.Request.ServerVariables["REMOTE_ADDR"];
            }

            return result;
        }


        /// <summary>
        /// Parses the int.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static int ParseInt(this string obj)
        {
            return int.Parse(obj);
        }

        /// <summary>
        /// Parses the u int.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static uint ParseUInt(this string obj)
        {
            return uint.Parse(obj);
        }

        /// <summary>
        /// Parses the short.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static short ParseShort(this string obj)
        {
            return short.Parse(obj);
        }

        /// <summary>
        /// Parses the u short.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static ushort ParseUShort(this string obj)
        {
            return ushort.Parse(obj);
        }

        /// <summary>
        /// Parses the long.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static long ParseLong(this string obj)
        {
            return long.Parse(obj);
        }

        /// <summary>
        /// Parses the u long.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static ulong ParseULong(this string obj)
        {
            return ulong.Parse(obj);
        }

        /// <summary>
        /// Removes the character.
        /// </summary>
        /// <param name="txt">The text.</param>
        /// <param name="charsToRemove">The chars to remove.</param>
        /// <returns></returns>
        public static string RemoveChar(this string txt, params char[] charsToRemove)
        {
            foreach (char c in charsToRemove)
            {
                txt = txt.Replace("" + c, "");
            }

            return txt;
        }

        /// <summary>
        /// Removes the specified strings to remove.
        /// </summary>
        /// <param name="txt">The text.</param>
        /// <param name="stringsToRemove">The strings to remove.</param>
        /// <returns></returns>
        public static string Remove(this string txt, params string[] stringsToRemove)
        {
            foreach (string s in stringsToRemove)
            {
                txt = txt.Replace(s, "");
            }

            return txt;
        }

        /// <summary>
        /// To the expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="method">The method.</param>
        /// <returns></returns>
        public static Expression<Func<T, TResult>> ToExpression<T, TResult>(this Func<T, TResult> method)
        {
            return x => method(x);
        }

        public static Expression<Func<T, TResult>> ToExpression<T, TResult>(this object method)
        {
            if (method is Delegate)
                return x => ((Func<T, TResult>)method)(x);
            else
                throw new Exception("obj is not an method...");
        }

        /// <summary>
        /// Logs the specified text.
        /// </summary>
        /// <param name="txt">The text.</param>
        public static void Log(this string txt)
        {
            Debug.Write(txt);
        }

        /// <summary>
        /// Casts the specified value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static object Cast(this Type type, object value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            object obj = null;
            obj = Convert.ChangeType(value, type);
            return obj;
        }

        /// <summary>
        /// Casts the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="Exception">type cannot be null to be able to Cast
        /// or
        /// All entities Type in the collection have to match to type to be able to Cast</exception>
        public static IEnumerable Cast<T>(this IEnumerable<T> collection, Type type)
        {
            IEnumerable result = null;
            if (type == null)
                throw new Exception("type cannot be null to be able to Cast");


            if (collection != null)
            {
                bool entitiesMatch = collection.All(a => a.GetType() == type);
                if (type != typeof(object) && !entitiesMatch)
                    throw new Exception("All entities Type in the collection have to match to type to be able to Cast");


                dynamic genericList = typeof(List<>).MakeGenericType(type).Instantiate();
                Type genericListType = (Type)genericList.GetType();

                if (genericList != null)
                {
                    if (result == null)
                        result = new List<object>();

                    foreach (var item in collection)
                        ((object)genericList).IntoMethod(genericListType, "Add", false, item);

                    result = genericList;
                }
            }

            return result;
        }

        /// <summary>
        /// Casts the specified type.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// type cannot be null to be able to Cast
        /// or
        /// All entities Type in the collection have to match to type to be able to Cast
        /// </exception>
        public static IEnumerable Cast(this IEnumerable collection, Type type)
        {
            IEnumerable result = null;
            if (type == null)
                throw new Exception("type cannot be null to be able to Cast");



            if (collection != null)
            {
                bool entitiesMatch = true;
                foreach (var item in collection)
                    entitiesMatch = (item.GetType() == type) ? true : false;

                if (!entitiesMatch)
                    throw new Exception("All entities Type in the collection have to match to type to be able to Cast");


                dynamic genericList = typeof(List<>).MakeGenericType(type).Instantiate();
                Type genericListType = (Type)genericList.GetType();

                if (genericList != null)
                {
                    if (result == null)
                        result = new List<object>();

                    foreach (var item in collection)
                        ((object)genericList).IntoMethod(genericListType, "Add", false, item);

                    result = genericList;
                }
            }

            return result;
        }

        /// <summary>
        /// Intoes the method.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="methodHolder">The method holder.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="isExtension">if set to <c>true</c> [is extension].</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object IntoMethod(this object obj, Type methodHolder, string methodName, bool isExtension = false, params object[] parameters)
        {
            object result = null;
            bool isStatic = false;

            if (isExtension)
                parameters = (object[])parameters.AddValues(obj);

            if (methodHolder.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(a => a.Name == methodName))
                isStatic = true;

            MethodInfo method = methodHolder.GetMethodInfo(methodName);
            result = method?.Invoke((isStatic) ? null : obj, parameters);
            return result;
        }

        public static object IntoMethod(this object obj, string methodName, params object[] parameters)
        {
            object result = null;
            bool isStatic = false;

            if (obj.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public).Any(a => a.Name == methodName))
                isStatic = true;

            MethodInfo method = obj.GetType().GetMethodInfo(methodName);
            result = method?.Invoke((isStatic) ? null : obj, parameters);
            return result;
        }

        public static object IntoMethod(this Type methodHolder, string methodName, params object[] parameters)
        {
            object result = null;
            bool isStatic = false;

            if (methodHolder.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(a => a.Name == methodName))
                isStatic = true;

            MethodInfo method = methodHolder.GetMethodInfo(methodName);
            result = method?.Invoke((isStatic) ? null : methodHolder.Instantiate(), parameters);
            return result;
        }

        /// <summary>
        /// Intoes the generic method.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="methodHolder">The method holder.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="generics">The type.</param>
        /// <param name="isExtension">if set to <c>true</c> [is extension].</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public static object IntoGenericMethod(this object obj, Type methodHolder, string methodName, Type[] generics, bool isExtension = false, params object[] parameters)
        {
            object result = null;
            bool isStatic = false;

            if (isExtension)
                parameters = (object[])parameters.AddValues(obj);

            if (methodHolder.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(a => a.Name == methodName))
                isStatic = true;

            MethodInfo method = methodHolder.GetMethodInfo(methodName)?.MakeGenericMethod(generics);
            result = method?.Invoke((isStatic) ? null : obj, parameters);
            return result;
        }

        public static object IntoGenericMethod(this object obj, string methodName, Type generic, params object[] parameters)
        {
            object result = null;
            bool isStatic = false;

            if (obj.GetType().GetMethods(BindingFlags.Static | BindingFlags.Public).Any(a => a.Name == methodName))
                isStatic = true;

            MethodInfo method = obj.GetType().GetMethodInfo(methodName)?.MakeGenericMethod(new Type[] { generic });
            result = method?.Invoke((isStatic) ? null : obj, parameters);
            return result;
        }

        public static object IntoGenericMethod(this Type methodHolder, string methodName, Type generic, params object[] parameters)
        {
            object result = null;
            bool isStatic = false;

            if (methodHolder.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(a => a.Name == methodName))
                isStatic = true;

            MethodInfo method = methodHolder.GetMethodInfo(methodName)?.MakeGenericMethod(new Type[] { generic });

            result = method?.Invoke((isStatic) ? null : methodHolder.Instantiate(), parameters);
            return result;
        }

        public static object IntoGenericMethod(this Type methodHolder, string methodName, Type[] generics, params object[] parameters)
        {
            object result = null;
            bool isStatic = false;

            if (methodHolder.GetMethods(BindingFlags.Static | BindingFlags.Public).Any(a => a.Name == methodName))
                isStatic = true;

            MethodInfo method = methodHolder.GetMethodInfo(methodName)?.MakeGenericMethod(generics);

            result = method?.Invoke((isStatic) ? null : methodHolder.Instantiate(), parameters);
            return result;
        }

        /// <summary>
        /// To the array.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static Array ToArray(this IEnumerable source, Type type)
        {
            var param = Expression.Parameter(typeof(IEnumerable), "source");
            var cast = Expression.Call(typeof(Enumerable), "Cast", new[] { type }, param);
            var toArray = Expression.Call(typeof(Enumerable), "ToArray", new[] { type }, cast);
            var lambda = Expression.Lambda<Func<IEnumerable, Array>>(toArray, param).Compile();

            return lambda(source);
        }

        /// <summary>
        /// Determines whether [is valid URL].
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>
        ///   <c>true</c> if [is valid URL] [the specified URL]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValidUrl(this string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        /// <summary>
        /// Encodes the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string EncodeUrl(this string url)
        {
            return (url.IsValidUrl()) ? HttpUtility.UrlEncode(url) : null;
        }

        /// <summary>
        /// Decodes the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string DecodeUrl(this string url)
        {
            return HttpUtility.UrlDecode(url);
        }

        /// <summary>
        /// Safes the name.
        /// </summary>
        /// <param name="txt">The text.</param>
        /// <returns></returns>
        public static string SafeName(this string txt)
        {
            return txt?.Remove("`1")?.RemoveChar('<', '>', '@', '.', '{', '}', '[', ']', '_');
        }

        /// <summary>
        /// Splits by the specified seperator.
        /// </summary>
        /// <param name="txt">The text.</param>
        /// <param name="seperator">The seperator.</param>
        /// <returns></returns>
        public static string[] Split(this string txt, string seperator)
        {
            return txt.Split(new string[] { seperator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string ReadFile(this string path)
        {
            string result = null;
            using (StreamReader reader = new StreamReader(path))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        public static IUnityContainer GetUnityContainer(this string assemblyName)
        {
            MethodInfo methodInfo = (MethodInfo)"UnityConfig.GetContainer".ScanAssembliesForObject(assemblyName);
            object unityConfig = "UnityConfig".ScanAssembliesForObject().Instantiate();
            UnityContainer result = (UnityContainer)methodInfo?.Invoke(unityConfig, null);
            return result;
        }

        public static IUnityContainer GetUnityContainer(this Assembly assembly)
        {
            MethodInfo methodInfo = (MethodInfo)"UnityConfig.GetContainer".ScanAssembliesForObject(assembly.GetName().Name);
            object unityConfig = "UnityConfig".ScanAssembliesForObject().Instantiate();
            UnityContainer result = (UnityContainer)methodInfo?.Invoke(unityConfig, null);
            return result;
        }

        public static IWindsorContainer GetWindsorContainer(this string assemblyName)
        {
            MethodInfo methodInfo = (MethodInfo)"WindsorConfig.GetContainer".ScanAssembliesForObject(assemblyName);
            object windsorConfig = "WindsorConfig".ScanAssembliesForObject().Instantiate();
            WindsorContainer result = (WindsorContainer)methodInfo?.Invoke(windsorConfig, null);
            return result;
        }

        public static IWindsorContainer GetWindsorContainer(this Assembly assembly)
        {
            MethodInfo methodInfo = (MethodInfo)"WindsorConfig.GetContainer".ScanAssembliesForObject(assembly.GetName().Name);
            object windsorConfig = "WindsorConfig".ScanAssembliesForObject().Instantiate();
            WindsorContainer result = (WindsorContainer)methodInfo?.Invoke(windsorConfig, null);
            return result;
        }

        public static SqlDbType GetSqlDbType(this Type type)
        {
            return SqlHelper.GetDbType(type);
        }

        public static string JsonSerialize(this object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }

        public static object JsonDeserialize(this string obj)
        {
            return JsonConvert.DeserializeObject(obj);
        }

        public static T JsonDeserialize<T>(this string obj)
        {
            return JsonConvert.DeserializeObject<T>(obj);
        }

        public static string XmlSerialize(this object obj)
        {
            if (obj == null)
                return string.Empty;

            try
            {
                var xmlserializer = new XmlSerializer(obj.GetType());
                var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    xmlserializer.Serialize(writer, obj);
                    return stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred on XmlSerialize(this object obj)...", ex);
            }
        }

        public static object XmlDeserialize(this string obj)
        {
            object result = null;
            string path = "cars.xml";

            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            StreamReader reader = new StreamReader(path);
            result = serializer.Deserialize(reader);
            reader.Close();
            return result;
        }

        public static T XmlDeserialize<T>(this string obj)
        {
            return (T)XmlDeserialize(obj);
        }

        public static T SyncTask<T>(this Task<T> task)
        {
            task.RunSynchronously();
            return task.Result;
        }

        public static char ToChar(this int num)
        {
            return Convert.ToChar(num);
        }

        public static EventInfo GetEvent(this object obj, string eventName)
        {
            return obj.GetType().GetEvent(eventName);
        }

        public static object IntoGenericAsT(this Type type, Type genericType, params object[] parms)
        {
            return type.IntoGenericAsT(genericType).Instantiate(parms);
        }

        public static Type IntoGenericAsT(this Type type, Type genericType)
        {
            return genericType.MakeGenericType(type);
        }

        public static Type InsertGenericTypes(this Type genericType, Type[] types, params object[] parms)
        {
            return genericType.MakeGenericType(types);
        }

        public static Type CreateClass(string[] propNames, Type[] propTypes)
        {
            return CreateClass(propNames, propTypes);
        }

        public static Type CreateClass(string name, string[] propNames, Type[] propTypes)
        {
            return ClassBuilder.CreateType(name ?? "Class" + Guid.NewGuid().ToString(), propNames, propTypes);
        }

        public static bool IsNumeric(this Type type)
        {
            Type nnType = Nullable.GetUnderlyingType(type) ?? type;
            switch (Type.GetTypeCode(nnType))
            {
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsNumeric(this object obj)
        {
            return (obj == null) ? false : obj.GetType().IsNumeric();
        }

        public static bool HasProperty(this Type type, PropertyInfo prop)
        {
            return type.GetProperties().FirstOrDefault(a => a == prop) == null ? false : true;
        }

        public static bool HasMethod(this Type type, MethodInfo method)
        {
            return type.GetMethods().FirstOrDefault(a => a == method) == null ? false : true;
        }

        public static bool HasField(this Type type, FieldInfo field)
        {
            return type.GetFields().FirstOrDefault(a => a == field) == null ? false : true;
        }

        public static string RandomString(this Random ran, int length, bool includeNumbers = true)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0" + (includeNumbers ? "123456789" : "");
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[ran.Next(s.Length)]).ToArray());
        }

        public static int RandomNumber(this Random ran, int min, int max)
        {

            return ran.Next(min, max);
        }

        public static string Encrypt(this string data, string key, Encoding encoding = null)
        {
            if (key == null)
                throw new Exception("key cannot be null to be able to Encrypt...");

            if (data == null)
                throw new Exception("this cannot be null to be able to Encrypt...");

            if (encoding == null)
                encoding = Encoding.Unicode;

            return Encryption.Encrypt(data, key);
        }

        public static string Decrypt(this string data, string key, Encoding encoding = null)
        {
            if (key == null)
                throw new Exception("key cannot be null to be able to Decrypt...");

            if (data == null)
                throw new Exception("this cannot be null to be able to Decrypt...");

            if (encoding == null)
                encoding = Encoding.Unicode;

            return Encryption.Decrypt(data, key);
        }

        public static bool In<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }

        public static int Index<T>(this IEnumerable<T> coll, Func<T, bool> predicate)
        {
            for (int i = 0; i < coll.Count(); i++)
                if (predicate(coll.ElementAt(i)))
                    return i;

            return -1;
        }

        public static DateTime SemiMonthDate(this DateTime date)
        {
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

            DateTime halfTimeDate = new DateTime(date.Year, date.Month, daysInMonth / 2),
                     endDate = new DateTime(date.Year, date.Month, daysInMonth);

            while (!halfTimeDate.IsWeekDay() || !endDate.IsWeekDay())
            {
                if (!halfTimeDate.IsWeekDay())
                    halfTimeDate = halfTimeDate.AddDays(-1);

                if (!endDate.IsWeekDay())
                    endDate = endDate.AddDays(-1);
            }

            return (date.Day <= halfTimeDate.Day)
                   ? halfTimeDate
                   : endDate;

        }

        public static bool IsWeekDay(this DateTime date)
        {
            return ((date.DayOfWeek == DayOfWeek.Saturday) || (date.DayOfWeek == DayOfWeek.Sunday)) ? false : true;
        }

        public static int NumberOfDaysInMonth(this DateTime date, DayOfWeek dayOfWeek)
        {
            DateTime start = new DateTime(date.Year, date.Month, 1),
                     end = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

            TimeSpan ts = end - start;

            int totalDays = (int)Math.Floor(ts.TotalDays / 7);
            int remainder = (int)ts.TotalDays % 7;
            int sinceLastDay = end.DayOfWeek - dayOfWeek;

            if (sinceLastDay < 0)
                sinceLastDay += 7;

            if (remainder >= sinceLastDay)
                totalDays++;

            return totalDays;
        }

        public static T Complete<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }

        public static T[] Complete<T>(this IEnumerable<Task<T>> tasks)
        {
            List<T> list = new List<T>();
            Task.WaitAll();

            foreach (Task<T> task in tasks)
                list.Add(task.Result);

            return list.ToArray();
        }

        /// <summary>
        /// Applies the CssRewriteUrlTransform to every path in the array.
        /// </summary>      
        public static Bundle IncludeWithCssRewriteUrlTransform(this StyleBundle bundle, params string[] virtualPaths)
        {
            //Ensure we add CssRewriteUrlTransform to turn relative paths (to images, etc.) in the CSS files into absolute paths.
            //Otherwise, you end up with 404s as the bundle paths will cause the relative paths to be off and not reach the static files.

            if ((virtualPaths != null) && (virtualPaths.Any()))
            {
                virtualPaths.ToList().ForEach(path =>
                {
                    bundle.Include(path, new CssRewriteUrlTransform());
                });
            }

            return bundle;
        }

        public static string GetIP4Address(this HttpContext context)
        {
            string result = String.Empty;

            foreach (IPAddress IPA in Dns.GetHostAddresses(context.Request.UserHostAddress))
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    result = IPA.ToString();
                    break;
                }

            if (result != String.Empty)
                return result;

            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    result = IPA.ToString();
                    break;
                }

            return result;
        }

        public static string GetIP4Address(this HttpContextBase context)
        {
            string result = String.Empty;

            foreach (IPAddress IPA in Dns.GetHostAddresses(context.Request.UserHostAddress))
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    result = IPA.ToString();
                    break;
                }

            if (result != String.Empty)
                return result;

            foreach (IPAddress IPA in Dns.GetHostAddresses(Dns.GetHostName()))
                if (IPA.AddressFamily.ToString() == "InterNetwork")
                {
                    result = IPA.ToString();
                    break;
                }

            return result;
        }


        #endregion
    }
}
