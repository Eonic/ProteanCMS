using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web;
using static Protean.Env;

namespace Protean.Framework.Adapters
{
    internal class FrameworkHttpRequestAdapter : IHttpRequest
    {
        private readonly HttpRequestBase _request;

        public FrameworkHttpRequestAdapter(HttpRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            _request = new HttpRequestWrapper(request);
        }

        public FrameworkHttpRequestAdapter(HttpRequestBase request)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        private NameValueCollection _queryString;

        public NameValueCollection QueryString
        {
            // HttpRequest.QueryString is read-only, so expose a writable copy
            get { return _queryString ?? (_queryString = new NameValueCollection(_request.QueryString)); }
            set { _queryString = value; }
        }

        // Dummy members - to be populated later

        // Core URL and Path Properties
        public Uri Url => null;
        public Uri UrlReferrer => null;
        public string RawUrl => null;
        public string Path => null;
        public string PathInfo => null;
        public string FilePath => null;
        public string CurrentExecutionFilePath => null;
        public string CurrentExecutionFilePathExtension => null;
        public string AppRelativeCurrentExecutionFilePath => null;
        public string ApplicationPath => null;
        public string PhysicalPath => null;
        public string PhysicalApplicationPath => null;

        // HTTP Metadata
        public string HttpMethod => null;
        public string RequestType => null;
        public string Scheme => null;
        public string Host => null;
        public string Protocol => null;
        public bool IsSecureConnection => false;
        public bool IsLocal => false;
        public bool IsAuthenticated => false;
        public string UserIdentity => null;
        public string UserHostAddress => null;
        public string UserAgent => null;
        public IEnumerable<string> AcceptTypes => null;
        public IEnumerable<string> UserLanguages => null;
        public Version HttpVersion => null;

        // Browser Capabilities
        public string BrowserName => null;
        public string BrowserVersion => null;
        public string BrowserPlatform => null;
        public bool BrowserSupportsJavaScript => false;

        // Collections
        public IDictionary<string, string[]> QueryMulti => null;
        public NameValueCollection Form => null;
        public IDictionary<string, string[]> FormMulti => null;
        public NameValueCollection Headers => null;
        public IHttpCookieCollection Cookies => null;
        public NameValueCollection ServerVariables => null;

        // Raw Collections
        public NameValueCollection FormCollection => null;
        public IEnumerable<KeyValuePair<string, string>> RawCookies => null;
        public IEnumerable<KeyValuePair<string, IHttpPostedFile>> RawFiles => null;

        // Content and Encoding
        public string ContentType => null;
        public Encoding ContentEncoding => null;
        public Encoding ContentEncodingObject => null;
        public long? ContentLength => null;
        public int TotalBytes => 0;
        public bool HasEntityBody => false;

        // Streams and Body
        public Stream Body => null;
        public Stream InputStream => null;
        public Task<Stream> BodyAsync() { return Task.FromResult<Stream>(null); }
        public byte[] BinaryRead(int count) { return null; }

        // Files
        public IHttpFileCollection Files => null;
        public int FileCount => 0;
        public bool HasFiles => false;
        public IHttpPostedFile GetFile(string key) { return null; }
        public IHttpPostedFile GetFile(int index) { return null; }
        public IEnumerable<string> FileKeys => null;
        public IEnumerable<string> AllFileKeys => null;

        // Request Validation
        public void ValidateInput() { }

        // Utility Methods
        public string MapPath(string virtualPath) { return null; }
        public bool IsAjaxRequest() { return false; }
        public void Abort() { }
        public void SaveAs(string filename, bool includeHeaders) { }

        // Client Certificate
        public byte[] ClientCertificate => null;

        public string this[string key] => null;
    }
}
