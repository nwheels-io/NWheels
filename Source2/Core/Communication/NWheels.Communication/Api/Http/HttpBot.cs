using System;
using System.Collections.Specialized;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace NWheels.Communication.Api.Http
{
    public class HttpBot
    {
        private readonly IHttpBotLogger _logger;
        private Uri _baseUri;
        private CookieContainer _cookies;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public HttpBot(string baseUrl = null, IHttpBotLogger logger = null)
        {
            if (baseUrl != null)
            {
                this.BaseUrl = baseUrl;
            }

            _logger = logger;
            _cookies = new CookieContainer();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IResponse Get(string relativeUrl, object query = null)
        {
            var requestUri = new UriBuilder(new Uri(_baseUri, relativeUrl));
            var webClient = new WebClientEx(_cookies);

            CopyObjectPropertiesToNamedValues(query, webClient.QueryString);

            _logger?.SendingHttpGet(url: requestUri.Uri);

            return new WebClientBasedResponse(
                this,
                requestUri.Uri,
                webClient,
                w => w.DownloadData(requestUri.ToString()),
                newCookies => _cookies = newCookies);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IResponse Post(string relativeUrl, object query = null, object form = null, FileUpload[] filesToUpload = null)
        {
            var requestUri = new UriBuilder(new Uri(_baseUri, relativeUrl));

            if (form != null && (filesToUpload == null || filesToUpload.Length == 0))
            {
                var webClient = new WebClientEx(_cookies);
                CopyObjectPropertiesToNamedValues(query, webClient.QueryString);

                var formData = new NameValueCollection();
                CopyObjectPropertiesToNamedValues(form, formData);

                _logger?.SendingHttpPost(url: requestUri.Uri, formData: DumpNameValueCollection(formData), uploadFiles: "none");

                return new WebClientBasedResponse(
                    this,
                    requestUri.Uri,
                    webClient,
                    w => w.UploadValues(requestUri.Uri, formData),
                    newCookies => _cookies = newCookies);
            }
            else
            {
                var webRequest = CreateMultiPartDataRequest(requestUri, form, filesToUpload);
                return new WebRequestBasedResponse(this, webRequest, newCookies => _cookies = newCookies);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void LogResponseStatus(HttpStatusCode statusCode, string responseUrl)
        {
            if ((int)statusCode < 400)
            {
                _logger?.ResponseSuccess(string.Format("{0}: {1}", (int)statusCode, statusCode.ToString()), responseUrl);
            }
            else
            {
                _logger?.ResponseFailure(string.Format("{0}: {1}", (int)statusCode, statusCode.ToString()), responseUrl);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IHttpBotLogger Logger
        {
            get { return _logger; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private HttpWebRequest CreateMultiPartDataRequest(UriBuilder requestUri, object form, FileUpload[] filesToUpload)
        {
            var request = (HttpWebRequest)WebRequest.Create(requestUri.ToString());
            request.Method = "POST";
            request.CookieContainer = _cookies;

            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            boundary = "--" + boundary;

            using (var requestStream = request.GetRequestStream())
            {
                var formData = AppendFormData(form, boundary, requestStream);
                AppendFilesToUpload(filesToUpload, boundary, requestStream);

                var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);

                _logger?.SendingHttpPost(url: requestUri.Uri, formData: DumpNameValueCollection(formData), uploadFiles: DumpFileUploadCollection(filesToUpload));
            }

            return request;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string BaseUrl
        {
            get
            {
                return (_baseUri != null ? _baseUri.ToString() : null);
            }
            set
            {
                _baseUri = new Uri(value, UriKind.Absolute);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string DumpFileUploadCollection(FileUpload[] collection)
        {
            if (collection == null)
            {
                return "(null)";
            }

            var result = new StringBuilder();
            int index = 0;

            foreach (var file in collection)
            {
                result.AppendFormat("[{0}] type=[{1}] name=[{2}] path=[{3}]", index++, file.ContentType, file.Name, file.Filename);
                result.AppendLine();
            }

            return result.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private string DumpNameValueCollection(NameValueCollection collection)
        {
            if (collection == null)
            {
                return "(null)";
            }

            var result = new StringBuilder();
            int index = 0;

            result.AppendLine();

            foreach (var key in collection.AllKeys)
            {
                result.AppendFormat("[{0}] ", index++);
                result.Append(key);
                result.Append("=");
                result.Append(collection[key] ?? "(null)");
                result.AppendLine();
            }

            return result.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void CopyObjectPropertiesToNamedValues(object query, NameValueCollection nameValueCollection)
        {
            if (query != null)
            {
                foreach (var property in query.GetType().GetProperties())
                {
                    var value = property.GetValue(query);

                    if (value != null)
                    {
                        nameValueCollection.Add(property.Name, value.ToString());
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void AppendFilesToUpload(FileUpload[] filesToUpload, string boundary, Stream requestStream)
        {
            if (filesToUpload != null)
            {
                foreach (var file in filesToUpload)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer =
                        Encoding.UTF8.GetBytes(
                            string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", file.Name, file.Filename, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", file.ContentType, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    file.Stream.CopyTo(requestStream);
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private NameValueCollection AppendFormData(object form, string boundary, Stream requestStream)
        {
            if (form != null)
            {
                var formData = new NameValueCollection();
                CopyObjectPropertiesToNamedValues(form, formData);

                foreach (string name in formData.Keys)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", name, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(formData[name] + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                }

                return formData;
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static XElement GetResponseAsXml(MemoryStream responseStream)
        {
            using (var reader = XmlReader.Create(responseStream))
            {
                return XElement.Load(reader);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string GetResponseAsString(MemoryStream responseStream)
        {
            return Encoding.UTF8.GetString(responseStream.ToArray());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string JavaScriptStringDecodeIf(string s, bool condition)
        {
            if (!condition || s == null || !s.StartsWith("\"") || !s.EndsWith("\"") || s.Length < 3)
            {
                return s;
            }

            return s.Substring(1, s.Length - 2).Replace("\\\\", "\\").Replace("\\\"", "\"");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IResponse : IDisposable
        {
            dynamic ResponseBodyAsJsonDynamic(bool unescape = false);
            T ResponseBodyAsJson<T>(bool unescape = false) where T : class;
            string ResponseBodyAsString();
            XElement ResponseBodyAsXml();
            byte[] ResponseBodyAsByteArray();
            HttpStatusCode StatusCode { get; }
            Exception Exception { get; }
            string Url { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class FileUpload
        {
            public FileUpload()
            {
                ContentType = "application/octet-stream";
            }
            public string Name { get; set; }
            public string Filename { get; set; }
            public string ContentType { get; set; }
            public Stream Stream { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class WebRequestBasedResponse : IResponse, IDisposable
        {
            private readonly HttpWebRequest _webRequest;
            private readonly Action<CookieContainer> _onUpdateCookies;
            private HttpWebResponse _webResponse;
            private readonly HttpBot _bot;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WebRequestBasedResponse(HttpBot bot, HttpWebRequest webRequest, Action<CookieContainer> onUpdateCookies)
            {
                _bot = bot;
                _webRequest = webRequest;
                _onUpdateCookies = onUpdateCookies;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _webResponse.Dispose();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public dynamic ResponseBodyAsJsonDynamic(bool unescape = false)
            {
                dynamic result = null;

                ExecuteRequest(responseBuffer => {
                    _bot.LogResponseStatus(this.StatusCode, this.Url);

                    var json = JavaScriptStringDecodeIf(GetResponseAsString(responseBuffer), unescape);

                    _bot.Logger?.ResponsePayload(format: "JSON", contents: json);

                    var converter = new ExpandoObjectConverter();
                    result = JsonConvert.DeserializeObject<ExpandoObject>(json, converter);
                });

                Dispose();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T ResponseBodyAsJson<T>(bool unescape = false)
                where T : class
            {
                T result = null;
                ExecuteRequest(responseBuffer => {
                    _bot.LogResponseStatus(this.StatusCode, this.Url);
                    var json = JavaScriptStringDecodeIf(GetResponseAsString(responseBuffer), unescape);
                    _bot.Logger?.ResponsePayload(format: "JSON", contents: json);
                    result = JsonConvert.DeserializeObject<T>(json);
                });

                Dispose();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string ResponseBodyAsString()
            {
                string result = null;
                ExecuteRequest(responseBuffer => result = GetResponseAsString(responseBuffer));

                Dispose();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public XElement ResponseBodyAsXml()
            {
                XElement result = null;
                ExecuteRequest(responseBuffer => result = GetResponseAsXml(responseBuffer));

                Dispose();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public byte[] ResponseBodyAsByteArray()
            {
                byte[] result = null;
                ExecuteRequest(responseBuffer => result = responseBuffer.ToArray());

                Dispose();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpStatusCode StatusCode { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Exception Exception { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Url { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ExecuteRequest(Action<MemoryStream> responseHandler)
            {
                try
                {
                    using (_webResponse = (HttpWebResponse)_webRequest.GetResponse())
                    {

                        this.StatusCode = _webResponse.StatusCode;
                        this.Url = _webResponse.ResponseUri.ToString();

                        using (var responseStream = _webResponse.GetResponseStream())
                        {
                            using (var buffer = new MemoryStream())
                            {
                                if (responseStream != null)
                                {
                                    responseStream.CopyTo(buffer);
                                    buffer.Position = 0;
                                }

                                responseHandler(buffer);
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    if (_webResponse == null)
                    {
                        _webResponse = (HttpWebResponse) e.Response;
                    }
                    this.StatusCode = _webResponse.StatusCode;
                    this.Exception = e;
                    throw;
                }
                catch (Exception e)
                {
                    this.Exception = e;
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class WebClientBasedResponse : IResponse, IDisposable
        {
            private readonly Uri _requestUri;
            private readonly WebClientEx _webClient;
            private readonly Func<WebClientEx, byte[]> _onExecuteRequest;
            private readonly Action<CookieContainer> _onUpdateCookies;
            private readonly HttpBot _bot;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WebClientBasedResponse(
                HttpBot bot,
                Uri requestUri,
                WebClientEx webClient,
                Func<WebClientEx, byte[]> onExecuteRequest,
                Action<CookieContainer> onUpdateCookies)
            {
                _bot = bot;
                _onExecuteRequest = onExecuteRequest;
                _requestUri = requestUri;
                _webClient = webClient;
                _onUpdateCookies = onUpdateCookies;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Dispose()
            {
                _webClient.Dispose();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public dynamic ResponseBodyAsJsonDynamic(bool unescape = false)
            {
                var responseBytes = ExecuteRequest();

                _bot.LogResponseStatus(this.StatusCode, this.Url);

                var json = JavaScriptStringDecodeIf(GetResponseAsString(new MemoryStream(responseBytes)), unescape);

                _bot.Logger?.ResponsePayload(format: "JSON", contents: json);

                dynamic result = JObject.Parse(json);

                Dispose();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public T ResponseBodyAsJson<T>(bool unescape = false) where T : class
            {
                var responseBytes = ExecuteRequest();

                _bot.LogResponseStatus(this.StatusCode, this.Url);

                var json = JavaScriptStringDecodeIf(GetResponseAsString(new MemoryStream(responseBytes)), unescape);

                _bot.Logger?.ResponsePayload(format: "JSON", contents: json);

                var result = JsonConvert.DeserializeObject<T>(json);

                Dispose();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string ResponseBodyAsString()
            {
                var responseBytes = ExecuteRequest();

                _bot.LogResponseStatus(this.StatusCode, this.Url);

                var result = GetResponseAsString(new MemoryStream(responseBytes));

                _bot.Logger?.ResponsePayload(format: "TEXT", contents: result);

                Dispose();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public XElement ResponseBodyAsXml()
            {
                var responseBytes = ExecuteRequest();

                _bot.LogResponseStatus(this.StatusCode, this.Url);

                var result = GetResponseAsXml(new MemoryStream(responseBytes));

                _bot.Logger?.ResponsePayload(format: "XML", contents: result.ToString());

                Dispose();
                return result;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public byte[] ResponseBodyAsByteArray()
            {
                var responseBytes = ExecuteRequest();

                _bot.LogResponseStatus(this.StatusCode, this.Url);
                _bot.Logger?.ResponsePayload(format: "byte[]", contents: string.Format("{0} byte(s)", responseBytes != null ? responseBytes.Length : 0));

                Dispose();
                return responseBytes;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpStatusCode StatusCode { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Exception Exception { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Url { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private byte[] ExecuteRequest()
            {
                try
                {
                    var responseBytes = _onExecuteRequest(_webClient);

                    this.StatusCode = _webClient.StatusCode;
                    this.Url = _webClient.ResponseUri.ToString();

                    return responseBytes;
                }
                catch (WebException e)
                {
                    this.StatusCode = ((HttpWebResponse)e.Response).StatusCode;
                    this.Exception = e;
                    throw;
                }
                catch (Exception e)
                {
                    this.Exception = e;
                    throw;
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class WebClientEx : WebClient
        {
            private readonly CookieContainer _cookies;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public WebClientEx(CookieContainer cookies)
            {
                _cookies = cookies ?? new CookieContainer();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CookieContainer Cookies
            {
                get { return _cookies; }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public HttpStatusCode StatusCode { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Uri ResponseUri { get; private set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest r = base.GetWebRequest(address);
                var request = r as HttpWebRequest;

                if (request != null)
                {
                    request.CookieContainer = _cookies;
                }

                return r;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
            {
                WebResponse response = base.GetWebResponse(request, result);
                ReadHttpResponse(response);
                return response;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            protected override WebResponse GetWebResponse(WebRequest request)
            {
                WebResponse response = base.GetWebResponse(request);
                ReadHttpResponse(response);
                return response;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void ReadHttpResponse(WebResponse r)
            {
                var response = r as HttpWebResponse;

                if (response != null)
                {
                    this.StatusCode = response.StatusCode;
                    this.ResponseUri = response.ResponseUri;

                    CookieCollection cookies = response.Cookies;
                    _cookies.Add(cookies);
                }
            }
        }
    }
}
