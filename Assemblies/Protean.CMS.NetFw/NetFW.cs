using Protean.Adapters.AspNetFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;
using static Protean.Env;
using static System.Net.Mime.MediaTypeNames;


namespace Protean
{

    public class NetFW : Cms, IDisposable
    {
        private bool disposedValue = false;

        // Adapters for Http abstractions
        public IHttpContext HttpContextAdapter { get; private set; }
        public IHttpRequest HttpRequestAdapter { get; private set; }
        public IHttpResponse HttpResponseAdapter { get; private set; }
        public Protean.Env.IHttpSessionState SessionAdapter { get; private set; }
        public Protean.Env.IWebConfigurationManager ConfigAdapter { get; private set; }

        public NetFW()
        {
            var currentContext = HttpContext.Current;
            if (currentContext == null)
            {
                throw new InvalidOperationException("HttpContext.Current is not available.");
            }

            // Initialise adapters
            moCtx = new AspNetFrameworkHttpContextAdapter(currentContext);
            moRequest = new AspNetFrameworkHttpRequestAdapter(currentContext.Request);
            moResponse = new AspNetFrameworkHttpResponseAdapter(currentContext.Response);
            moSession = new AspNetFrameworkSessionAdapter(currentContext.Session);
            moConfigMng = new AspNetFrameworkWebConfigurationManagerAdapter();
            moServer = new AspNetFrameworkHttpServerUtilityAdapter(currentContext.Server);
            goCache = new AspNetFrameworkCacheAdapter();
        }

        /// <summary>
        /// Dispose pattern implementation.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    HttpContextAdapter = null;
                    HttpRequestAdapter = null;
                    HttpResponseAdapter = null;
                    SessionAdapter = null;
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Public Dispose method for IDisposable.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizer in case Dispose is not called.
        /// </summary>
        ~NetFW()
        {
            Dispose(false);
        }
    }
}



namespace Protean.Adapters.AspNetFramework
{

    public class AspNetFrameworkHttpContextAdapter : Protean.Env.IHttpContext
    {
        private readonly HttpContext _context;

        public AspNetFrameworkHttpContextAdapter(HttpContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Gets the request abstraction.
        /// </summary>
        public Protean.Env.IHttpRequest Request => new AspNetFrameworkHttpRequestAdapter(_context.Request);

        /// <summary>
        /// Gets the response abstraction.
        /// </summary>
        public IHttpResponse Response => new AspNetFrameworkHttpResponseAdapter(_context.Response);

        /// <summary>
        /// Gets the session abstraction.
        /// </summary>
        public Protean.Env.IHttpSessionState Session => new AspNetFrameworkSessionAdapter(_context.Session);

        /// <summary>
        /// Gets the current user identity name.
        /// </summary>
        public string UserIdentity => _context.User?.Identity?.Name;

        /// <summary>
        /// Gets the current URL.
        /// </summary>
        public Uri Url => _context.Request.Url;

        /// <summary>
        /// Gets the current application path.
        /// </summary>
        public string ApplicationPath => _context.Request.ApplicationPath;

        /// <summary>
        /// Gets the current items collection (per-request storage).
        /// </summary>
        public System.Collections.IDictionary Items => _context.Items;

        public IHttpServerUtility Server => new AspNetFrameworkHttpServerUtilityAdapter(_context.Server);


        public Env.IHttpApplicationState Application => new AspNetFrameworkApplicationAdapter(_context.Application);

        public object ApplicationInstance => _context.ApplicationInstance;

        public IWebConfigurationManager Config => new AspNetFrameworkWebConfigurationManagerAdapter();

        public ICache Cache => new AspNetFrameworkCacheAdapter();

        // In AspNetFrameworkHttpContextProvider
        public IHttpContext Current
        {
            get
            {
                var context = System.Web.HttpContext.Current;
                if (context == null)
                    throw new InvalidOperationException("HttpContext.Current is not available.");
                return new AspNetFrameworkHttpContextAdapter(context);
            }
        }


        public void CompleteRequest()
        {
            if (_context?.ApplicationInstance is HttpApplication app)
            {
                app.CompleteRequest();
            }
            else
            {
                throw new InvalidOperationException("ApplicationInstance is not available.");
            }
        }

    }


    public class AspNetFrameworkWebConfigurationManagerAdapter : IWebConfigurationManager
    {
        public string GetAppSetting(string key) => WebConfigurationManager.AppSettings[key];

        public IDictionary<string, string> GetAllAppSettings() =>
            WebConfigurationManager.AppSettings.AllKeys.ToDictionary(k => k, k => WebConfigurationManager.AppSettings[k]);

        public string GetConnectionString(string name) =>
            WebConfigurationManager.ConnectionStrings[name]?.ConnectionString;

        public IDictionary<string, string> GetAllConnectionStrings() =>
            WebConfigurationManager.ConnectionStrings.Cast<ConnectionStringSettings>()
                .ToDictionary(cs => cs.Name, cs => cs.ConnectionString);

        public Configuration OpenWebConfiguration(string path) =>
            WebConfigurationManager.OpenWebConfiguration(path);

        public Configuration OpenMachineConfiguration() =>
            WebConfigurationManager.OpenMachineConfiguration();

        public Configuration OpenMappedWebConfiguration(WebConfigurationFileMap fileMap, string path) =>
            WebConfigurationManager.OpenMappedWebConfiguration(fileMap, path);

        public object GetSection(string sectionName) =>
            WebConfigurationManager.GetSection(sectionName);

      //  public ConfigurationSectionGroup GetSectionGroup(string sectionGroupName) =>
       //     WebConfigurationManager.GetSectionGroup(sectionGroupName);


        public object GetWebApplicationSection(string sectionName) =>
            System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection(sectionName);

    }



    public class AspNetFrameworkSectionGroupAdapter : Protean.Env.IGetSectionGroup
    {
        public object GetSectionGroup(string sectionGroupName)
        {
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            return config.GetSectionGroup(sectionGroupName);
        }
    }


    /// <summary>
    /// Adapter for System.Web.HttpRequest implementing IHttpRequest.
    /// </summary>

    /// <summary>
    /// Adapter for System.Web.HttpRequest implementing IHttpRequest.
    /// </summary>

    public class AspNetFrameworkHttpRequestAdapter : Protean.Env.IHttpRequest
    {
        private readonly HttpRequest _request;

        public AspNetFrameworkHttpRequestAdapter(HttpRequest request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        // Core URL and Path Properties
        public Uri Url => _request.Url;
        public Uri UrlReferrer => _request.UrlReferrer;
        public string RawUrl => _request.RawUrl;
        public string Path => _request.Path;
        public string PathInfo => _request.PathInfo;
        public string FilePath => _request.FilePath;
        public string CurrentExecutionFilePath => _request.CurrentExecutionFilePath;
        public string CurrentExecutionFilePathExtension => _request.CurrentExecutionFilePathExtension;
        public string AppRelativeCurrentExecutionFilePath => _request.AppRelativeCurrentExecutionFilePath;
        public string ApplicationPath => _request.ApplicationPath;
        public string PhysicalPath => _request.PhysicalPath;
        public string PhysicalApplicationPath => _request.PhysicalApplicationPath;
        // public string PathTranslated => _request.PathTranslated;

        // HTTP Metadata
        public string HttpMethod => _request.HttpMethod;
        public string RequestType => _request.RequestType;
        public string Scheme => _request.Url.Scheme;
        public string Host => _request.Url.Host;
        public string Protocol => _request.ServerVariables["SERVER_PROTOCOL"];
        public bool IsSecureConnection => _request.IsSecureConnection;
        public bool IsLocal => _request.IsLocal;
        public bool IsAuthenticated => _request.IsAuthenticated;
        public string UserIdentity => _request.LogonUserIdentity?.Name;
        public string UserHostAddress => _request.UserHostAddress;
        public string UserAgent => _request.UserAgent;
        public IEnumerable<string> AcceptTypes => _request.AcceptTypes ?? Enumerable.Empty<string>();
        public IEnumerable<string> UserLanguages => _request.UserLanguages ?? Enumerable.Empty<string>();
        public Version HttpVersion => new Version(_request.ServerVariables["SERVER_PROTOCOL"]?.Replace("HTTP/", "") ?? "1.1");

        // Browser Capabilities
        public string BrowserName => _request.Browser?.Browser;
        public string BrowserVersion => _request.Browser?.Version;
        public string BrowserPlatform => _request.Browser?.Platform;
        public bool BrowserSupportsJavaScript => _request.Browser?.EcmaScriptVersion.Major > 0;

        // Collections
        public IDictionary<string, string[]> QueryMulti =>
            _request.QueryString.AllKeys.Where(k => k != null).ToDictionary(k => k, k => _request.QueryString.GetValues(k));
        public NameValueCollection Form => _request.Form;
        public IDictionary<string, string[]> FormMulti =>
            _request.Form.AllKeys.Where(k => k != null).ToDictionary(k => k, k => _request.Form.GetValues(k));

        public NameValueCollection Headers => _request.Headers;

        public IHttpCookieCollection Cookies => new AspNetFrameworkHttpCookieCollectionAdapter(_request.Cookies);

        public NameValueCollection ServerVariables => _request.ServerVariables;

        // Raw Collections
        public NameValueCollection QueryString => _request.QueryString;
        public NameValueCollection FormCollection => _request.Form;
        public IEnumerable<KeyValuePair<string, string>> RawCookies =>
            _request.Cookies.AllKeys.Select(k => new KeyValuePair<string, string>(k, _request.Cookies[k]?.Value));
        public IEnumerable<KeyValuePair<string, IHttpPostedFile>> RawFiles =>
            _request.Files.AllKeys.Select(k => new KeyValuePair<string, IHttpPostedFile>(k, new AspNetFrameworkHttpPostedFileAdapter(_request.Files[k])));

        // Content and Encoding
        public string ContentType => _request.ContentType;
        public Encoding ContentEncoding => _request.ContentEncoding;
        public Encoding ContentEncodingObject => _request.ContentEncoding;
        public long? ContentLength => _request.ContentLength;
        public int TotalBytes => _request.TotalBytes;
        public bool HasEntityBody => _request.ContentLength > 0;

        // Streams and Body
        public Stream Body => _request.InputStream;
        public Stream InputStream => _request.InputStream;
        public Task<Stream> BodyAsync() => Task.FromResult(_request.InputStream);
        public byte[] BinaryRead(int count) => _request.BinaryRead(count);

        // Files

        public IHttpFileCollection Files => new AspNetFrameworkHttpFileCollectionAdapter(_request.Files);

        public int FileCount => _request.Files.Count;

        public bool HasFiles => _request.Files.Count > 0;

        public IHttpPostedFile GetFile(string key) =>
            _request.Files[key] != null ? new AspNetFrameworkHttpPostedFileAdapter(_request.Files[key]) : null;

        public IHttpPostedFile GetFile(int index) =>
            _request.Files[index] != null ? new AspNetFrameworkHttpPostedFileAdapter(_request.Files[index]) : null;

        public IEnumerable<string> AllFileKeys => _request.Files.AllKeys;

        public IEnumerable<string> FileKeys => _request.Files.AllKeys.Where(k => !string.IsNullOrEmpty(k));

        // Request Validation
        //  public bool ValidateRequest => _request.v
        public void ValidateInput() => _request.ValidateInput();

        // Utility Methods
        public string MapPath(string virtualPath) => _request.MapPath(virtualPath);
        public bool IsAjaxRequest() => _request.Headers["X-Requested-With"] == "XMLHttpRequest";
        public void Abort() => _request.Abort();
        public void SaveAs(string filename, bool includeHeaders) => _request.SaveAs(filename, includeHeaders);

        // Client Certificate
        public byte[] ClientCertificate => _request.ClientCertificate?.Certificate;

        // Indexer
        public string this[string key] =>
            _request[key]; // Mimics HttpRequest indexer behaviour


    }


    /// <summary>
    /// Adapter for System.Web.HttpPostedFile implementing IHttpPostedFile.
    /// </summary>
    public class AspNetFrameworkHttpPostedFileAdapter : IHttpPostedFile
    {
        private readonly HttpPostedFile _file;

        public AspNetFrameworkHttpPostedFileAdapter(HttpPostedFile file)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));
        }

        /// <summary>
        /// Gets the name of the form field that the file was uploaded with.
        /// </summary>
        public string Name => _file.FileName; // Note: HttpPostedFile does not expose form field name directly.

        /// <summary>
        /// Gets the original file name of the uploaded file.
        /// </summary>
        public string FileName => Path.GetFileName(_file.FileName);

        /// <summary>
        /// Gets the MIME content type of the uploaded file.
        /// </summary>
        public string ContentType => _file.ContentType;

        /// <summary>
        /// Gets the length of the uploaded file in bytes.
        /// </summary>
        public long Length => _file.ContentLength;
        public int ContentLength => _file.ContentLength; // ✅ NEW
        /// <summary>
        /// Indicates whether the file is empty.
        /// </summary>
        public bool IsEmpty => _file.ContentLength == 0;
        public Stream InputStream => _file.InputStream; // ✅ NEW
        /// <summary>
        /// Opens a read-only stream for the uploaded file.
        /// </summary>
        public Stream OpenReadStream() => _file.InputStream;

        /// <summary>
        /// Saves the uploaded file to the specified path.
        /// </summary>
        public void SaveAs(string path) => _file.SaveAs(path);

        /// <summary>
        /// Saves the uploaded file asynchronously to the specified path.
        /// </summary>
        public async Task SaveAsAsync(string path)
        {
            using (var targetStream = File.Create(path))
            {
                await _file.InputStream.CopyToAsync(targetStream);
            }
        }

        /// <summary>
        /// Copies the uploaded file to a stream synchronously.
        /// </summary>
        public void CopyTo(Stream target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            _file.InputStream.CopyTo(target);
        }

        /// <summary>
        /// Copies the uploaded file to a stream asynchronously.
        /// </summary>
        public async Task CopyToAsync(Stream target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            await _file.InputStream.CopyToAsync(target);
        }
    }


    public class AspNetFrameworkHttpFileCollectionAdapter : IHttpFileCollection
    {
        private readonly HttpFileCollection _files;

        public AspNetFrameworkHttpFileCollectionAdapter(HttpFileCollection files)
        {
            _files = files ?? throw new ArgumentNullException(nameof(files));
        }

        public int Count => _files.Count;
        public string[] AllKeys => _files.AllKeys;

        public IHttpPostedFile this[int index] =>
            _files[index] != null ? new AspNetFrameworkHttpPostedFileAdapter(_files[index]) : null;

        public IHttpPostedFile this[string key] =>
            _files[key] != null ? new AspNetFrameworkHttpPostedFileAdapter(_files[key]) : null;
    }

    /// <summary>
    /// Adapter for System.Web.HttpResponse implementing IHttpResponse.
    /// </summary>

    /// <summary>
    /// Adapter for System.Web.HttpResponse implementing IHttpResponse.
    /// </summary>
    public class AspNetFrameworkHttpResponseAdapter : Protean.Env.IHttpResponse
    {
        private readonly HttpResponse _response;

        public AspNetFrameworkHttpResponseAdapter(HttpResponse response)
        {
            _response = response ?? throw new ArgumentNullException(nameof(response));
        }

        // Core Properties
        public int StatusCode
        {
            get => _response.StatusCode;
            set => _response.StatusCode = value;
        }

        public string StatusDescription
        {
            get => _response.StatusDescription;
            set => _response.StatusDescription = value;
        }

        public string ContentType
        {
            get => _response.ContentType;
            set => _response.ContentType = value;
        }

        public Encoding ContentEncoding
        {
            get => _response.ContentEncoding;
            set => _response.ContentEncoding = value;
        }

        public long? ContentLength
        {
            get => _response.OutputStream.CanSeek ? (long?)_response.OutputStream.Length : default(long?);
            set { /* HttpResponse does not allow setting ContentLength directly */ }
        }

        public bool Buffer
        {
            get => _response.Buffer;
            set => _response.Buffer = value;
        }

        public string CacheControl
        {
            get => _response.CacheControl;
            set => _response.CacheControl = value;
        }

        public bool IsClientConnected => _response.IsClientConnected;

        public bool SuppressContent
        {
            get => _response.SuppressContent;
            set => _response.SuppressContent = value;
        }

        public bool TrySkipIisCustomErrors
        {
            get => _response.TrySkipIisCustomErrors;
            set => _response.TrySkipIisCustomErrors = value;
        }


        public int Expires
        {
            get => _response.Expires;
            set => _response.Expires = value;
        }

        public DateTime ExpiresAbsolute
        {
            get => _response.ExpiresAbsolute;
            set => _response.ExpiresAbsolute = value;
        }


        public Encoding HeaderEncoding
        {
            get => _response.HeaderEncoding;
            set => _response.HeaderEncoding = value;
        }

        // Headers
        public IDictionary<string, string> Headers =>
            _response.Headers.AllKeys.Where(k => k != null)
                .ToDictionary(k => k, k => _response.Headers[k]);

        public void AddHeader(string name, string value) => _response.AddHeader(name, value);
        public void AppendHeader(string name, string value) => _response.AppendHeader(name, value);
        public void RemoveHeader(string name) => _response.Headers.Remove(name);

        // Cookies

        public IHttpCookieCollection Cookies => new AspNetFrameworkHttpCookieCollectionAdapter(_response.Cookies);

        // Output Writing
        public void Write(string content) => _response.Write(content);
        public void Write(char ch) => _response.Write(ch);
        public void Write(char[] buffer, int index, int count) => _response.Write(new string(buffer, index, count));
        public void Write(byte[] buffer) => _response.BinaryWrite(buffer);
        public async Task WriteAsync(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                _response.BinaryWrite(ms.ToArray());
            }
        }
        public void BinaryWrite(byte[] buffer) => _response.BinaryWrite(buffer);
        public void OutputStreamWrite(Stream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            stream.CopyTo(_response.OutputStream);
        }
        public TextWriter OutputWriter => _response.Output;

        public Stream OutputStream => _response.OutputStream;

        // Lifecycle Control
        public void Flush() => _response.Flush();
        public void Clear() => _response.Clear();
        public void ClearHeaders() => _response.ClearHeaders();
        public void ClearContent() => _response.ClearContent();
        public void End() => _response.End();
        public void Close() => _response.Close();

        // Redirection
        public void Redirect(string url, bool permanent = false)
        {
            if (permanent)
            {
                _response.RedirectPermanent(url, endResponse: false);
            }
            else
            {
                _response.Redirect(url, endResponse: false);
            }
        }
        public void RedirectPermanent(string url) => _response.RedirectPermanent(url, endResponse: false);


        public void RedirectPermanent(string url, bool endResponse)
        {
            // ASP.NET Framework does not have an overload, so emulate behaviour:
            _response.RedirectPermanent(url);
            if (!endResponse)
            {
                // Do not call Response.End(); allow further processing
            }
            else
            {
                _response.End();
            }
        }

        public void RedirectToRoute(string routeName) => _response.RedirectToRoute(routeName);
        public void RedirectToRoutePermanent(string routeName) => _response.RedirectToRoutePermanent(routeName);

        // File Transmission
        public void TransmitFile(string filename) => _response.TransmitFile(filename);
        public void WriteFile(string filename) => _response.WriteFile(filename);
        public void WriteFile(string filename, bool readIntoMemory) => _response.WriteFile(filename, readIntoMemory);
        public void WriteFile(string filename, long offset, long length) => _response.WriteFile(filename, offset, length);

        // Charset and Encoding
        public Encoding CharsetEncoding
        {
            get => _response.ContentEncoding;
            set => _response.ContentEncoding = value;
        }
        public string Charset
        {
            get => _response.Charset;
            set => _response.Charset = value;
        }

        // Cache Control
        public void SetCacheability(string cacheability) => _response.Cache.SetCacheability((HttpCacheability)Enum.Parse(typeof(HttpCacheability), cacheability));
        public void SetNoStore() => _response.Cache.SetNoStore();

        // Client Disconnect Handling
        public bool ClientDisconnectedToken => !_response.IsClientConnected; // Or use 

        // Path Modifier
        public void ApplyAppPathModifier(string virtualPath) => _response.ApplyAppPathModifier(virtualPath);

        public ICachePolicy Cache => new AspNetFrameworkCachePolicyAdapter(_response.Cache);

    }


    public class AspNetFrameworkCachePolicyAdapter : ICachePolicy
    {
        private readonly HttpCachePolicy _cache;

        public AspNetFrameworkCachePolicyAdapter(HttpCachePolicy cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public void SetNoStore() => _cache.SetNoStore();
        public void AppendCacheExtension(string extension) => _cache.AppendCacheExtension(extension);
        public void SetCacheability(string cacheability) =>
            _cache.SetCacheability((HttpCacheability)Enum.Parse(typeof(HttpCacheability), cacheability));
        public void SetExpires(DateTime date) => _cache.SetExpires(date);
        public void SetValidUntilExpires(bool validUntilExpires) => _cache.SetValidUntilExpires(validUntilExpires);
    }



    public class AspNetFrameworkCacheAdapter : ICache
    {
        private readonly Cache _cache;


        public AspNetFrameworkCacheAdapter()
        {
            _cache = HttpRuntime.Cache;
        }


        public object this[string key]
        {
            get => _cache[key];
            set => _cache.Insert(key, value);
        }


        public void Insert(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            if (absoluteExpiration.HasValue)
            {
                _cache.Insert(key, value, null, absoluteExpiration.Value, Cache.NoSlidingExpiration);
            }
            else if (slidingExpiration.HasValue)
            {
                _cache.Insert(key, value, null, Cache.NoAbsoluteExpiration, slidingExpiration.Value);
            }
            else
            {
                _cache.Insert(key, value);
            }
        }

        public object Get(string key) => _cache[key];

        public void Remove(string key) => _cache.Remove(key);

        public bool Contains(string key) => _cache[key] != null;

        public void Clear()
        {
            var enumerator = _cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                _cache.Remove(enumerator.Key.ToString());
            }
        }
    }


    public class AspNetFrameworkHttpCookieAdapter : IHttpCookie
    {
        private readonly System.Web.HttpCookie _cookie;

        public AspNetFrameworkHttpCookieAdapter(System.Web.HttpCookie cookie)
        {
            _cookie = cookie ?? throw new ArgumentNullException(nameof(cookie));
        }

        public string Name => _cookie.Name;
        public string Value
        {
            get => _cookie.Value;
            set => _cookie.Value = value;
        }
        public DateTime? Expires
        {
            get => _cookie.Expires == DateTime.MinValue ? default(DateTime?) : _cookie.Expires;
            set => _cookie.Expires = value ?? DateTime.MinValue;
        }

        public string Domain
        {
            get => _cookie.Domain;
            set => _cookie.Domain = value;
        }

        public string Path
        {
            get => _cookie.Path;
            set => _cookie.Path = value;
        }
        public bool HttpOnly
        {
            get => _cookie.HttpOnly;
            set => _cookie.HttpOnly = value;
        }
        public bool Secure
        {
            get => _cookie.Secure;
            set => _cookie.Secure = value;
        }
    }


    public class AspNetFrameworkHttpCookieCollectionAdapter : IHttpCookieCollection
    {
        private readonly HttpCookieCollection _cookies;

        public AspNetFrameworkHttpCookieCollectionAdapter(HttpCookieCollection cookies)
        {
            _cookies = cookies ?? throw new ArgumentNullException(nameof(cookies));
        }

        public int Count => _cookies.Count;
        public string[] AllKeys => _cookies.AllKeys;

        public IHttpCookie this[int index] =>
            _cookies[index] != null ? new AspNetFrameworkHttpCookieAdapter(_cookies[index]) : null;

        public IHttpCookie this[string name] =>
            _cookies[name] != null ? new AspNetFrameworkHttpCookieAdapter(_cookies[name]) : null;

        public void Add(IHttpCookie cookie)
        {
            var httpCookie = new System.Web.HttpCookie(cookie.Name, cookie.Value)
            {
                Expires = cookie.Expires ?? DateTime.MinValue,
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure
            };
            _cookies.Add(httpCookie);
        }

        public void Remove(string name)
        {
            if (_cookies[name] != null)
            {
                var expiredCookie = new System.Web.HttpCookie(name) { Expires = DateTime.UtcNow.AddDays(-1) };
                _cookies.Add(expiredCookie);
            }
        }
    }

    /// <summary>
    /// Adapter for System.Web.SessionState.HttpSessionState implementing ISession.
    /// </summary>

    /// <summary>
    /// Adapter for System.Web.SessionState.HttpSessionState implementing IHttpSessionState.
    /// </summary>
    public class AspNetFrameworkSessionAdapter : Protean.Env.IHttpSessionState
    {
        private readonly HttpSessionState _session;

        public AspNetFrameworkSessionAdapter(HttpSessionState session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public object this[string key]
        {
            get => _session[key];
            set => _session[key] = value;
        }

        public object this[int index] => _session[index];

        public bool ContainsKey(string key) => _session[key] != null;

        public void Add(string name, object value) => _session.Add(name, value);

        public void Remove(string key) => _session.Remove(key);

        public void RemoveAt(int index) => _session.RemoveAt(index);

        public void RemoveAll() => _session.RemoveAll();

        public void Clear() => _session.Clear();

        public void CopyTo(Array array, int index) => _session.CopyTo(array, index);

        public IEnumerable<string> Keys
        {
            get
            {
                foreach (string key in _session.Keys)
                {
                    yield return key;
                }
            }
        }

        public int Count => _session.Count;

        public string SessionID => _session.SessionID;

        public bool IsNewSession => _session.IsNewSession;

        public bool IsReadOnly => _session.IsReadOnly;

        public int Timeout
        {
            get => _session.Timeout;
            set => _session.Timeout = value;
        }

        public void Abandon() => _session.Abandon();


        public bool IsCookieless => _session.IsCookieless;

        public bool IsSynchronized => _session.IsSynchronized;

        public object SyncRoot => _session.SyncRoot;


        public object Clone()
        {
            var cloneDictionary = new Dictionary<string, object>();
            foreach (string key in _session.Keys)
            {
                cloneDictionary[key] = _session[key];
            }
            return cloneDictionary;
        }


        public Task CommitAsync()
        {
            // ASP.NET Framework does not require explicit commit.
            return Task.CompletedTask;
        }

        public IEnumerator GetEnumerator() => _session.GetEnumerator();




        public IHttpCookieMode CookieMode
        {
            get
            {
                // ASP.NET Framework does not expose CookieMode; assume cookies are used.
                return IHttpCookieMode.UseCookies;
            }
        }


        public ISessionStateMode Mode
        {
            get
            {
                switch (_session.Mode)
                {
                    case System.Web.SessionState.SessionStateMode.Off:
                        return ISessionStateMode.Off;
                    case System.Web.SessionState.SessionStateMode.InProc:
                        return ISessionStateMode.InProc;
                    case System.Web.SessionState.SessionStateMode.StateServer:
                        return ISessionStateMode.StateServer;
                    case System.Web.SessionState.SessionStateMode.SQLServer:
                        return ISessionStateMode.SQLServer;
                    case System.Web.SessionState.SessionStateMode.Custom:
                        return ISessionStateMode.Custom;
                    default:
                        return ISessionStateMode.InProc;
                }
            }
        }

    }





    public class AspNetFrameworkApplicationAdapter : Protean.Env.IHttpApplicationState
    {
        private readonly HttpApplicationState _application;

        public AspNetFrameworkApplicationAdapter(HttpApplicationState application)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
        }

        public object this[string key]
        {
            get => _application[key];
            set => _application[key] = value;
        }
        public object Get(string key) => _application[key];
        public void Add(string key, object value) => _application.Add(key, value);
        public void Remove(string key) => _application.Remove(key);
        public void Clear() => _application.Clear();
        public IEnumerable<string> Keys
        {
            get
            {
                foreach (string key in _application.Keys)
                {
                    yield return key;
                }
            }
        }
        public int Count => _application.Count;
        public void Lock() => _application.Lock();
        public void UnLock() => _application.UnLock();


        public void CompleteRequest()
        {
            var context = HttpContext.Current;
            if (context?.ApplicationInstance is HttpApplication app)
            {
                app.CompleteRequest();
            }
            else
            {
                throw new InvalidOperationException("HttpContext.Current or ApplicationInstance is not available.");
            }
        }


    }


    public class AspNetFrameworkHttpContextProvider : IHttpContextProvider
    {
        public IHttpContext Current
        {
            get
            {
                var context = System.Web.HttpContext.Current;
                if (context == null)
                    throw new InvalidOperationException("HttpContext.Current is not available.");
                return new AspNetFrameworkHttpContextAdapter(context);
            }
        }
    }


    public class AspNetFrameworkHttpServerUtilityAdapter : IHttpServerUtility
    {
        private readonly HttpServerUtility _server;

        public AspNetFrameworkHttpServerUtilityAdapter(HttpServerUtility server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public string MapPath(string virtualPath) => _server.MapPath(virtualPath);

        public string UrlEncode(string value) => _server.UrlEncode(value);

        public string UrlDecode(string value) => _server.UrlDecode(value);

        public string HtmlEncode(string value) => _server.HtmlEncode(value);

        public string HtmlDecode(string value) => _server.HtmlDecode(value);

        public void Execute(string path) => _server.Execute(path);

        public void Transfer(string path) => _server.Transfer(path);

        public string MachineName => _server.MachineName;

        public void ClearError() => _server.ClearError();

        public Exception GetLastError() => _server.GetLastError();


        public int ScriptTimeout
        {
            get => _server.ScriptTimeout;
            set => _server.ScriptTimeout = value;
        }

    }


    public class AspNetFrameworkWebConfigurationFileMapAdapter : Protean.Env.IWebConfigurationFileMap
    {
        private readonly WebConfigurationFileMap _fileMap;

        public AspNetFrameworkWebConfigurationFileMapAdapter()
        {
            _fileMap = new WebConfigurationFileMap();
        }

        public AspNetFrameworkWebConfigurationFileMapAdapter(WebConfigurationFileMap fileMap)
        {
            _fileMap = fileMap ?? throw new ArgumentNullException(nameof(fileMap));
        }

        public string MachineConfigFilename
        {
            get => _fileMap.MachineConfigFilename;
            set => _fileMap.MachineConfigFilename = value;
        }

        public void AddVirtualDirectory(string virtualDirectory, string physicalDirectory)
        {
            if (string.IsNullOrEmpty(virtualDirectory))
                throw new ArgumentException("Virtual directory name cannot be null or empty.", nameof(virtualDirectory));

            if (string.IsNullOrEmpty(physicalDirectory))
                throw new ArgumentException("Physical directory path cannot be null or empty.", nameof(physicalDirectory));

            var mapping = new VirtualDirectoryMapping(physicalDirectory, true);
            _fileMap.VirtualDirectories.Add(virtualDirectory, mapping);
        }


        public IDictionary<string, string> VirtualDirectories
        {
            get
            {
                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string key in _fileMap.VirtualDirectories.Keys)
                {
                    var mapping = _fileMap.VirtualDirectories[key] as VirtualDirectoryMapping;
                    if (mapping != null)
                    {
                        result[key] = mapping.PhysicalDirectory;
                    }
                }
                return result;
            }
        }


        public WebConfigurationFileMap InnerFileMap => _fileMap;
    }

}



