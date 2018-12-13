using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using NostreetsExtensions.Extend.Basic;
using NostreetsExtensions.Utilities;

using RestSharp;
using RestSharp.Authenticators;

namespace NostreetsExtensions.Extend.Web
{
    public static partial class Web
    {
        /// <summary>
        /// Gets the local ip addresses.
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress()
        {
            string result = null;

            if (HttpContext.Current != null)
                HttpContext.Current.GetIP4Address();

            else {
                string hostName = Dns.GetHostName();
                IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
                result = ipEntry.AddressList[ipEntry.AddressList.Length - 1].ToString();
            }

            return result;

        }

        public static void UpdateWebConfig(string key, string value)
        {
            // Get the configuration.
            Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
            bool doesKeyExist = false;

            foreach (KeyValueConfigurationElement item in config.AppSettings.Settings)
                if (item.Key == key)
                    doesKeyExist = true;

            if (!doesKeyExist)
                config.AppSettings.Settings.Add(key, value);
            else
                config.AppSettings.Settings[key].Value = value;

            // Save to the file,
            config.Save(ConfigurationSaveMode.Minimal);
        }

        public static HttpResponseMessage CreateHttpResponseMessage(object content, string contentType, HttpStatusCode httpStatusCode)
        {
            throw new NotImplementedException();

            string serializedContent = content as string ?? JsonConvert.SerializeObject(content);
            HttpResponseMessage result = new HttpResponseMessage(httpStatusCode);
            result.Content = new StringContent((string)content, Encoding.UTF8, contentType);
        }
    }

    public static partial class Web
    {
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
        /// Decodes the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static string DecodeUrl(this string url)
        {
            return HttpUtility.UrlDecode(url);
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
        /// Hits the endpoint.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="method">The method.</param>
        /// <param name="data">The data.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="headers">The headers.</param>
        /// <returns></returns>
        /// <exception cref="Exception">url to has to be valid url string to be able to HitEndpoint...</exception>
        public static object HitEndpoint(this string url, string method = "GET", object data = null, string contentType = "application/json", Dictionary<string, string> headers = null, Type responseType = null)
        {
            if (!url.IsValidUrl())
                throw new Exception("url to has to be valid url string to be able to HitEndpoint...");

            HttpWebRequest requestStream = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse responseStream = null;
            string responseString = null,
                   requestString = null;

            requestStream.ContentType = contentType;
            requestStream.Method = method;

            if (headers != null)
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

                    if (responseString.IsJson())
                        responseData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseString);

                    else if (responseString.IsXml())
                    {

                        object deserializedJson;
                        string json;


                        if (responseString.IsHtml())
                        {
                            XDocument doc = XDocument.Parse(responseString);
                            json = JsonConvert.SerializeXNode(doc);
                            deserializedJson = JsonConvert.DeserializeObject(json);
                        }
                        else
                        {
                            //XmlSerializer serial = new XmlSerializer(data.GetType());
                            //StringReader reader = new StringReader(responseString);
                            //responseData = serial.Deserialize(reader);
                            XmlDocument doc = new XmlDocument();
                            doc.Load(responseString);
                            json = JsonConvert.SerializeXmlNode(doc);
                            deserializedJson = JsonConvert.DeserializeObject(json);
                        }


                        if (responseType != null && deserializedJson.TryCast(responseType, out object obj))
                            responseData = obj;
                        else
                            responseData = deserializedJson;

                    }
                    else
                        responseData = null;

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
                        //XmlSerializer serial = new XmlSerializer(data.GetType());
                        //StringReader reader = new StringReader(responseString);
                        //responseData = serial.Deserialize(reader);

                        XmlDocument doc = new XmlDocument();
                        doc.Load(responseString);

                        string json = JsonConvert.SerializeXmlNode(doc);
                        object deserializedJson = JsonConvert.DeserializeObject(json);

                        if (deserializedJson.TryCast(out T obj))
                            responseData = obj;
                        else
                            throw new InvalidCastException("Unable to cast response data to type of " + typeof(T).Name, new Exception(responseString));


                    }

                    if (responseString.ToLower().Contains("<error>"))
                        throw new Exception(responseString);

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
            IRestResponse<T> result = null;
            RestClient rest = null;
            if (url != null)
            {
                rest = new RestClient(url);
            }
            else { throw new Exception("URL is not defined!"); }

            #endregion Client

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

            #endregion Request

            result = rest.Execute<T>(request);
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
        /// Valids the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="uri">The URI.</param>
        /// <returns></returns>
        public static bool ValidUrl(this string url, out Uri uri)
        {
            if (!Regex.IsMatch(url, @"^https?:\/\/", RegexOptions.IgnoreCase))
                url = "http://" + url;

            url.IsValidUri(out uri, a => a.Scheme == Uri.UriSchemeHttp || a.Scheme == Uri.UriSchemeHttps);

            return false;
        }

        /// <summary>
        /// Registers the route.
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <param name="namespace">The namespace.</param>
        /// <param name="path">The path.</param>
        public static void RegisterRoute(this RouteCollection routes, string @namespace, string path = "{controller}/{action}/{id}")
        {
            Route externalBlogRoute = new Route(path, new MvcRouteHandler())
            {
                DataTokens = new RouteValueDictionary(
               new
               {
                   namespaces = new[] { @namespace }
               })
            };

            routes.Add(Guid.NewGuid().ToString() + "Route", externalBlogRoute);
        }

        public static bool IsHtml(this string input)
        {
            return input != HttpUtility.HtmlEncode(input) && input.Contains("DOCTYPE html");
        }
    }
}