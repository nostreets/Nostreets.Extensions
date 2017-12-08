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
using Microsoft.Practices.Unity;
using Newtonsoft.Json.Serialization;
using NostreetsExtensions.Helpers;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using NostreetsExtensions.Interfaces;

namespace NostreetsExtensions
{
    public static class Extend
    {
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

        public static DateTime ToDateTime(this string obj, string format = null)
        {
            if (format != null) { return DateTime.ParseExact(obj, format, CultureInfo.InvariantCulture); }
            else { return Convert.ToDateTime(obj); }
        }

        public static DateTime StartOfWeek(this DateTime dt)
        {
            DayOfWeek firstDay = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            DateTime firstDayInWeek = dt.Date;
            while (firstDayInWeek.DayOfWeek != firstDay)
                firstDayInWeek = firstDayInWeek.AddDays(-1);

            return firstDayInWeek;
        }

        public static DateTime EndOfWeek(this DateTime dt)
        {
            DateTime start = StartOfWeek(dt);
            return start.AddDays(6);
        }

        public static object HitEndpoint(this SqlService obj, string url, string method = "GET", object data = null, string contentType = "application/json", Dictionary<string, string> headers = null)
        {
            HttpWebRequest requestStream = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse responseStream = null;
            string responseString, requestString;

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
                return ex;
            }
        }

        public static T HitEndpoint<T>(this SqlService obj, string url, string method = "GET", object data = null, string contentType = "application/json", Dictionary<string, string> headers = null)
        {
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
                throw ex;
            }

        }

        public static int GetWeekOfMonth(this DateTime time)
        {
            DateTime first = new DateTime(time.Year, time.Month, 1);
            return time.GetWeekOfYear() - first.GetWeekOfYear() + 1;
        }

        public static int GetWeekOfYear(this DateTime time)
        {
            GregorianCalendar _gc = new GregorianCalendar();
            return _gc.GetWeekOfYear(time, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
        }

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

        public static void AddAttribute<T>(this object obj, bool affectBaseObj = true, Dictionary<object, Type> attributeParams = null, FieldInfo[] affectedFields = null) where T : Attribute
        {
            Type type = obj.GetType();

            AssemblyName aName = new AssemblyName("SomeNamespace");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(aName.Name);
            TypeBuilder affectedType = moduleBuilder.DefineType(type.Name + "Proxy", TypeAttributes.Public, type);


            Type[] attrParams = attributeParams.Values.ToArray();
            ConstructorInfo attrConstructor = typeof(T).GetConstructor(attrParams);
            CustomAttributeBuilder attrBuilder = new CustomAttributeBuilder(attrConstructor, attributeParams.Keys.ToArray());


            if (affectBaseObj)
            {
                affectedType.SetCustomAttribute(attrBuilder);
            }
            else if (affectedFields != null && affectedFields.Length > 1)
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

        public static bool IsOdd(this int value)
        {
            return value % 2 != 0;
        }

        public static bool IsEven(this int value)
        {
            return value % 2 == 0;
        }

        public static Dictionary<int, string> ToDictionary<T>(this Type enumType) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentException("Type must be an enum");

            return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(t => (int)(object)t, t => t.ToString());
        }

        public static Dictionary<int, string> ToDictionary(this Type enumType)
        {
            Dictionary<int, string> result = null;
            if (!enumType.IsEnum)
                throw new ArgumentException("Type must be an enum");

            string[] arr = Enum.GetNames(enumType);

            foreach (string enumName in arr)
            {
                if (result == null)
                    result = new Dictionary<int, string>();

                result.Add(enumType.GetEnumValue(enumName), enumName);
            }

            return result;
        }

        public static int GetEnumValue(this Type enumType, string name)
        {
            return (int)Enum.Parse(enumType, name);
        }

        public static int GetEnumValue<T>(this string name)
        {
            return (int)Enum.Parse(typeof(T), name);
        }

        public static Dictionary<string, string> GetQueryStrings(this HttpRequestMessage request)
        {
            return request.GetQueryNameValuePairs()
                          .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

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

        public static string GetHeader(this HttpRequestMessage request, string key)
        {
            IEnumerable<string> keys = null;
            if (!request.Headers.TryGetValues(key, out keys))
                return null;

            return keys.First();
        }

        public static string GetCookie(this HttpRequestMessage request, string cookieName)
        {
            CookieHeaderValue cookie = request.Headers.GetCookies(cookieName).FirstOrDefault() ?? default(CookieHeaderValue);

            return cookie[cookieName].Value;
        }

        public static string GetCookie(this HttpRequest request, string cookieName)
        {
            string result = null;
            HttpCookie cookie = request.Cookies[cookieName] ?? default(HttpCookie);
            if (cookie != null)
            {
                result = cookie.Value;
            }

            return result;
        }

        public static void SetCookie(this HttpContext context, string cookieName, string value, DateTime? expires = null)
        {
            try
            {
                HttpCookie cookie = new HttpCookie(cookieName, value);
                cookie.Expires = (expires != null) ? expires.Value : default(DateTime);

                context.Response.Cookies.Add(cookie);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

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

                HttpCookie cookie = new HttpCookie(cookieName, value);
                cookie.Expires = (expires != null) ? expires.Value : default(DateTime);


                context.Response.Cookies.Add(cookie);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

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

        public static bool HasDuplicates<T>(this IEnumerable<T> enumerable, string propertyName) where T : class
        {

            var dict = new Dictionary<string, int>();
            foreach (var item in enumerable)
            {
                if (!dict.ContainsKey((string)typeof(T).GetProperty(propertyName).GetValue(item)))
                {
                    dict.Add((string)typeof(T).GetProperty(propertyName).GetValue(item), 0);
                }
                if (dict[(string)typeof(T).GetProperty(propertyName).GetValue(item)] > 0)
                {
                    dict = null;
                    return true;
                }
                dict[(string)typeof(T).GetProperty(propertyName).GetValue(item)]++;
            }
            dict = null;
            return false;
        }

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

        public static List<string> GetDuplicates<T>(this IEnumerable<T> enumerable, string propertyName) where T : class
        {

            var dict = new Dictionary<string, int>();
            foreach (var item in enumerable)
            {
                if (!dict.ContainsKey((string)typeof(T).GetProperty(propertyName).GetValue(item)))
                {
                    dict.Add((string)typeof(T).GetProperty(propertyName).GetValue(item), 0);
                }
                else
                {
                    dict[(string)typeof(T).GetProperty(propertyName).GetValue(item)]++;
                }
            }
            var duplicates = new List<string>();
            foreach (var value in dict)
            {
                if (value.Value > 0)
                {
                    duplicates.Add(value.Key);
                }
            }
            return duplicates;
        }

        public static bool IsActionDelegate<T>(this T source)
        {
            return typeof(T).FullName.StartsWith("System.Action");
        }

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

        public static bool IsType(this object obj, Type type)
        {
            bool result = false;
            if (type == obj.GetType())
            {
                result = true;
            }

            return result;
        }

        public static MethodInfo GetMethodInfo<T, T2>(this Expression<Func<T, T2>> expression)
        {
            var member = expression.Body as MethodCallExpression;

            if (member != null)
                return member.Method;

            throw new ArgumentException("Expression is not a method", "expression");
        }

        public static MethodInfo GetMethodInfo<T>(this Expression<Action<T>> expression)
        {
            var member = expression.Body as MethodCallExpression;

            if (member != null)
                return member.Method;

            throw new ArgumentException("Expression is not a method", "expression");
        }

        public static MethodInfo GetMethodInfo<T>(this T obj, string methodName, BindingFlags searchSettings = BindingFlags.NonPublic | BindingFlags.Instance) where T : new()
        {
            return typeof(T).GetMethod(methodName, searchSettings);
        }

        public static MethodInfo GetMethodInfo(this Type type, string methodName, BindingFlags searchSettings = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            return type.GetMethod(methodName, searchSettings);
        }

        public static MethodInfo GetMethodInfo(this string fullMethodName)
        {
            return (MethodInfo)fullMethodName.ScanAssembliesForObject();
        }

        public static List<Tuple<TAttribute, object>> GetObjectsWithAttribute<TAttribute>(this IList<Tuple<TAttribute, object>> obj, ClassTypes types, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            return AttributeScanner<TAttribute>.ScanAssembliesForAttributes(types, null, assembliesToSkip);
        }

        public static List<object> GetObjectsByAttribute<TAttribute>(this IList<TAttribute> obj, ClassTypes section, Type type = null, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<object> result = new List<object>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes(section, type, assembliesToSkip)) { result.Add(item.Item2); }

            return result;
        }

        public static List<object> GetObjectsByAttribute<TAttribute>(this IList<object> obj, ClassTypes section, Type type = null, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<object> result = new List<object>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes(section, type, assembliesToSkip)) { result.Add(item.Item2); }

            return result;
        }

        public static List<MethodInfo> GetMethodsByAttribute<TAttribute>(this IList<TAttribute> obj, Type type = null, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<MethodInfo> result = new List<MethodInfo>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes(ClassTypes.Methods, type, assembliesToSkip)) { result.Add((MethodInfo)item.Item2); }

            return result;
        }

        public static List<MethodInfo> GetMethodsByAttribute<TAttribute>(this IList<MethodInfo> obj, Type type = null, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<MethodInfo> result = new List<MethodInfo>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes(ClassTypes.Methods, type, assembliesToSkip)) { result.Add((MethodInfo)item.Item2); }

            return result;
        }

        public static List<Type> GetTypesByAttribute<TAttribute>(this IList<TAttribute> obj, Type type = null, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<Type> result = new List<Type>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes(ClassTypes.Type, type, assembliesToSkip)) { result.Add((Type)item.Item2); }

            return result;
        }

        public static List<Type> GetTypesByAttribute<TAttribute>(this IList<Type> obj, Type type = null, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<Type> result = new List<Type>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes(ClassTypes.Type, type, assembliesToSkip)) { result.Add((Type)item.Item2); }

            return result;
        }

        public static List<Assembly> GetAssembliesByAttribute<TAttribute>(this IList<TAttribute> obj, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<Assembly> result = new List<Assembly>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes(ClassTypes.Assembly, null, assembliesToSkip)) { result.Add((Assembly)item.Item2); }

            return result;
        }

        public static List<Assembly> GetAssembliesByAttribute<TAttribute>(this IList<Assembly> obj, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<Assembly> result = new List<Assembly>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes(ClassTypes.Assembly, null, assembliesToSkip)) { result.Add((Assembly)item.Item2); }

            return result;
        }

        public static List<PropertyInfo> GetPropertiesByAttribute<TAttribute>(this IList<TAttribute> obj, Type type = null, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            foreach (var item in AttributeScanner<TAttribute>.ScanAssembliesForAttributes(ClassTypes.Properties, type, assembliesToSkip)) { result.Add((PropertyInfo)item.Item2); }

            return result;
        }

        public static List<PropertyInfo> GetPropertiesByAttribute<TAttribute>(this IList<PropertyInfo> obj, Type type = null, Func<Assembly, bool> assembliesToSkip = null) where TAttribute : Attribute
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            List<Tuple<TAttribute, object>> list = AttributeScanner<TAttribute>.ScanAssembliesForAttributes(ClassTypes.Properties, type, assembliesToSkip);

            if (list != null)
            {
                foreach (var item in list) { result.Add((PropertyInfo)item.Item2); }
            }

            return result;
        }

        public static object Instantiate(this Type type)
        {
            if (type == typeof(string))
                return Activator.CreateInstance(typeof(string), Char.MinValue, 0);

            else
                return Activator.CreateInstance(type);

        }

        public static T Instantiate<T>(this T type)
        {

            if (typeof(T) == typeof(string))
                return (T)Activator.CreateInstance(typeof(string), Char.MinValue, 0);

            else
                return Activator.CreateInstance<T>();
        }

        public static T UnityInstantiate<T>(this T type, UnityContainer containter)
        {
            return containter.Resolve<T>();
        }

        public static object UnityInstantiate(this Type type, UnityContainer containter)
        {
            return containter.Resolve(type);
        }

        public static object ScanAssembliesForObject(this string nameToCheckFor, string assemblyToLookFor)
        {
            object result = AssemblyScanner.ScanAssembliesForObject(nameToCheckFor, new[] { assemblyToLookFor });
            return result;
        }

        public static object ScanAssembliesForObject(this string nameToCheckFor, params string[] assembliesToLookFor)
        {
            object result = AssemblyScanner.ScanAssembliesForObject(nameToCheckFor, assembliesToLookFor);
            return result;
        }

        public static object ScanAssembliesForObject(this string nameToCheckFor, string[] assembliesToSkip, string[] assembliesToLookFor)
        {
            object result = AssemblyScanner.ScanAssembliesForObject(nameToCheckFor, assembliesToLookFor, assembliesToSkip);
            return result;
        }

        public static string ExtendPath(this string path, string extension)
        {
            return Path.GetFullPath(Path.Combine(path, extension));
        }

        public static string StepIntoDirectory(this string path, string pathExt, bool recursively = false)
        {

            do
            {
                string[] subDirectories = Directory.GetDirectories(path);
                foreach (string dir in subDirectories)
                {
                    if (dir.Contains(pathExt)) { return dir; }
                }

                if (recursively && subDirectories.Length > 0)
                {
                    foreach (string dir in subDirectories)
                    {
                        dir.StepIntoDirectory(pathExt, true);
                    }
                }
                else
                {
                    recursively = false;
                }
            }
            while (recursively);

            return null;
        }

        public static string StepOutOfDirectory(this string path, int foldersBack = 1)
        {
            string modifiedPath = path;

            for (var i = 0; i < foldersBack; i++)
            {
                modifiedPath = Directory.GetParent(modifiedPath).FullName;
            }

            return modifiedPath;
        }

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

        public static bool Contains(this IEnumerable<string> list, params string[] values)
        {
            bool result = false;
            foreach (string item in list)
            {
                result = (values.Any(a => a == item)) ? true : false;
            }

            return result;
        }

        public static Assembly GetAssembly(this AppDomain assembly, string assemblyName)
        {
            Assembly result = null;

            foreach (Assembly assemble in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assemble.FullName.Contains(assemblyName) || assemble.GetName().Name == assemblyName) { result = assemble; break; }
            }

            return result;
        }

        public static void CreateResponse(this HttpApplication app, HttpStatusCode statusCode, object obj, string contentType = "application/json", IContractResolver resolver = null, Encoding encoding = null)
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

        public static bool DirectoryExists(this string path)
        {
            if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1) { return false; }

            DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetFullPath(path));
            if (!directoryInfo.Exists) { return false; }


            return true;
        }

        public static string Timestamp(this DateTime time)
        {
            return time.ToString("hh.mm.ss.tt_MMM-yy");
        }

        public static string FormatString(this string template, params string[] txt)
        {
            return String.Format(template, txt);
        }

        public static object GetPropertyValue(this object obj, string propertyName)
        {
            return obj.GetType().GetProperties().Single(pi => pi.Name == propertyName).GetValue(obj, null);
        }

        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            obj.GetType().GetProperties().Single(pi => pi.Name == propertyName).SetValue(obj, value);
        }

        public static List<string> GetColumns(this DbContext dbContext, Type type)
        {
            string statment = String.Format("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME like N'{0}s'", type.Name);
            DbRawSqlQuery<string> result = dbContext.Database.SqlQuery<string>(statment);
            return result.ToList();
        }

        public static List<ContainerRegistration> GetRegistrations(this IUnityContainer theContainer)
        {
            List<ContainerRegistration> result = null;

            foreach (ContainerRegistration item in theContainer.Registrations)
            {
                if (result == null)
                    result = new List<ContainerRegistration>();

                result.Add(item);
            }

            return result;
        }

        public static void Prepend<T>(this IList<T> list, T item)
        {
            list.Insert(0, item);
        }

        public static List<string> GetSchema(this ISqlDao srv, Func<SqlConnection> dataSouce, string tableName)
        {
            SqlDataReader reader = null;
            SqlCommand cmd = null;
            SqlConnection conn = null;
            List<string> result = null;

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

                        result = reader.GetSchemaTable().Rows.Cast<DataRow>().Select(c => c["ColumnName"].ToString()).ToList();

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

        public static string[] GetSchema(this IDataReader reader)
        {
            return reader.GetSchemaTable().Rows.Cast<DataRow>().Select(c => c["ColumnName"].ToString()).ToArray();
        }

        public static Type[] GetSchemaTypes(this IDataReader reader)
        {
            List<Type> result = new List<Type>();
            string[] columns = reader.GetSchema();
            for (int i = 0; i < columns.Length; i++)
            {
                result.Add(reader.GetValue(i).GetType());
            }

            return result.ToArray();
        }

        public static Type AddProperty(this Type objType, Type propType, string propName, int index = 0)
        {
            List<string> propNames = new List<string>();
            List<Type> propTypes = new List<Type>();
            PropertyInfo[] props = objType.GetProperties();
            bool addedProp = false;

            for (int i = 0; i < props.Length; i++)
            {
                if (i == index)
                {
                    propNames.Add(propName);
                    propTypes.Add(propType);
                    addedProp = true;
                }
                else
                {
                    propNames.Add(props[(addedProp) ? i - 1 : i].Name);
                    propTypes.Add(props[(addedProp) ? i - 1 : i].PropertyType);
                }
            }


            ClassBuilder builder = new ClassBuilder(objType.Name);
            return builder.CreateType(propNames.ToArray(), propTypes.ToArray());

        }

        public static Type AddProperty(this object obj, Type propType, string propName, int index = 0)
        {
            Type objType = obj.GetType();
            return objType.AddProperty(propType, propName, index);
        }

    }
}
