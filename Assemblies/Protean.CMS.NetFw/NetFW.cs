using Protean.Adapters.AspNetFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using static Protean.Env;


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

        public NetFW()
        {
            var currentContext = HttpContext.Current;
            if (currentContext == null)
            {
                throw new InvalidOperationException("HttpContext.Current is not available.");
            }

            // Initialise adapters
            HttpContextAdapter = new AspNetFrameworkHttpContextAdapter(currentContext);
            HttpRequestAdapter = new AspNetFrameworkHttpRequestAdapter(currentContext.Request);
            HttpResponseAdapter = new AspNetFrameworkHttpResponseAdapter(currentContext.Response);
            SessionAdapter = new AspNetFrameworkSessionAdapter(currentContext.Session);
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
        }



        /// <summary>
        /// Adapter for System.Web.HttpRequest implementing IHttpRequest.
        /// </summary>

        /// <summary>
        /// Adapter for System.Web.HttpRequest implementing IHttpRequest.
        /// </summary>
        public class AspNetFrameworkHttpRequestAdapter : IHttpRequest
        {
            private readonly System.Web.HttpRequest _request;

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
            public string PathTranslated => _request.PathTranslated;

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

            // Browser Capabilities
            public string BrowserName => _request.Browser?.Browser;
            public string BrowserVersion => _request.Browser?.Version;
            public string BrowserPlatform => _request.Browser?.Platform;
            public bool BrowserSupportsJavaScript => _request.Browser?.EcmaScriptVersion.Major > 0;

            // Collections
            public IDictionary<string, string> Query =>
                _request.QueryString.AllKeys.Where(k => k != null).ToDictionary(k => k, k => _request.QueryString[k]);

            public IDictionary<string, string[]> QueryMulti =>
                _request.QueryString.AllKeys.Where(k => k != null).ToDictionary(k => k, k => _request.QueryString.GetValues(k));

            public IDictionary<string, string> Form =>
                _request.Form.AllKeys.Where(k => k != null).ToDictionary(k => k, k => _request.Form[k]);

            public IDictionary<string, string[]> FormMulti =>
                _request.Form.AllKeys.Where(k => k != null).ToDictionary(k => k, k => _request.Form.GetValues(k));

            public IDictionary<string, string> Headers =>
                _request.Headers.AllKeys.Where(k => k != null).ToDictionary(k => k, k => _request.Headers[k]);

            public IDictionary<string, string> Cookies =>
                _request.Cookies.AllKeys.Where(k => k != null).ToDictionary(k => k, k => _request.Cookies[k]?.Value);

            public IDictionary<string, string> ServerVariables =>
                _request.ServerVariables.AllKeys.Where(k => k != null).ToDictionary(k => k, k => _request.ServerVariables[k]);

            // Content and Encoding
            public string ContentType => _request.ContentType;
            public string ContentEncoding => _request.ContentEncoding?.WebName;
            public Encoding ContentEncodingObject => _request.ContentEncoding;
            public long? ContentLength => _request.ContentLength;
            public int TotalBytes => _request.TotalBytes;

            // Streams and Body
            public Stream Body => _request.InputStream;
            public Stream InputStream => _request.InputStream;
            public Task<Stream> BodyAsync() => Task.FromResult(_request.InputStream);
            public byte[] BinaryRead(int count) => _request.BinaryRead(count);

            // Files
            public IEnumerable<IHttpPostedFile> Files =>
                _request.Files.AllKeys.Select(k => new AspNetFrameworkHttpPostedFileAdapter(_request.Files[k]));

            public int FileCount => _request.Files.Count;
            public bool HasFiles => _request.Files.Count > 0;
            public IHttpPostedFile GetFile(string key) =>
                _request.Files[key] != null ? new AspNetFrameworkHttpPostedFileAdapter(_request.Files[key]) : null;

            public IHttpPostedFile GetFile(int index) =>
                _request.Files[index] != null ? new AspNetFrameworkHttpPostedFileAdapter(_request.Files[index]) : null;

            public IEnumerable<string> FileKeys => _request.Files.AllKeys;
            public IEnumerable<string> AllFileKeys => _request.Files.AllKeys;

            // Request Validation
            public bool ValidateRequest => _request.ValidateRequest();
            public void ValidateInput() => _request.ValidateInput();

            // Utility Methods
            public string MapPath(string virtualPath) => _request.MapPath(virtualPath);
            public bool IsAjaxRequest() => _request.Headers["X-Requested-With"] == "XMLHttpRequest";
            public void Abort() => _request.Abort();


        public string this[string key]
        {
            get
            {
                if (_request.QueryString[key] != null) return _request.QueryString[key];
                if (_request.Form[key] != null) return _request.Form[key];
                if (_request.Cookies[key] != null) return _request.Cookies[key].Value;
                return null;
            }
        }

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

            /// <summary>
            /// Indicates whether the file is empty.
            /// </summary>
            public bool IsEmpty => _file.ContentLength == 0;

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

            /// <summary>
            /// Gets or sets the HTTP status code for the response.
            /// </summary>
            public int StatusCode
            {
                get => _response.StatusCode;
                set => _response.StatusCode = value;
            }

            /// <summary>
            /// Gets or sets the content type of the response.
            /// </summary>
            public string ContentType
            {
                get => _response.ContentType;
                set => _response.ContentType = value;
            }

            /// <summary>
            /// Adds a header to the response.
            /// </summary>
            public void AddHeader(string name, string value)
            {
                _response.AddHeader(name, value);
            }

            /// <summary>
            /// Writes a string to the response body.
            /// </summary>
            public void Write(string content)
            {
                _response.Write(content);
            }

            /// <summary>
            /// Writes a byte array to the response body.
            /// </summary>
            public void Write(byte[] buffer)
            {
                _response.BinaryWrite(buffer);
            }

            /// <summary>
            /// Writes a stream to the response body asynchronously.
            /// </summary>
            public async Task WriteAsync(Stream stream)
            {
                if (stream == null) throw new ArgumentNullException(nameof(stream));

                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    _response.BinaryWrite(memoryStream.ToArray());
                }
            }

            /// <summary>
            /// Redirects the client to the specified URL.
            /// </summary>
            public void Redirect(string url)
            {
                _response.Redirect(url, endResponse: false);
            }

            /// <summary>
            /// Ends the response (flushes and closes).
            /// </summary>
            public void End()
            {
                _response.End();
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

            public HttpCookieMode CookieMode => _session.CookieMode;

            public SessionStateMode Mode => _session.Mode;

            public bool IsCookieless => _session.IsCookieless;

            public bool IsSynchronized => _session.IsSynchronized;

            public object SyncRoot => _session.SyncRoot;

            public object Clone() => ((ICloneable)_session).Clone();

            public Task CommitAsync()
            {
                // ASP.NET Framework does not require explicit commit.
                return Task.CompletedTask;
            }

            public IEnumerator GetEnumerator() => _session.GetEnumerator();



        }
    }
