using AngleSharp.Io;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using static Protean.Env;

namespace Protean
{
    public class Env
    {


        public interface IHttpContext
        {
            IHttpRequest Request { get; }
            IHttpResponse Response { get; }
            IHttpSessionState Session { get; }
            IHttpApplicationState Application { get; }
            IHttpServerUtility Server { get; }
            IWebConfigurationManager Config { get; }
            IHttpContext Current { get; }

            ICache Cache { get; }

            void CompleteRequest();

            // Legacy support for HttpApplication instance
            object ApplicationInstance { get; }

        }




        public interface IWebConfigurationManager
        {
            string GetAppSetting(string key);
            IDictionary<string, string> GetAllAppSettings();
            string GetConnectionString(string name);
            IDictionary<string, string> GetAllConnectionStrings();
            Configuration OpenWebConfiguration(string path);
            Configuration OpenMachineConfiguration();
           // Configuration OpenMappedWebConfiguration(IWebConfigurationFileMap fileMap, string path); // Requires System.Web.Configuration
            object GetSection(string sectionName);
           // ConfigurationSectionGroup GetSectionGroup(string sectionGroupName);
            object GetWebApplicationSection(string sectionName);
        }


        public interface IGetSectionGroup
        {
            /// <summary>
            /// Retrieves a configuration section group by name.
            /// </summary>
            /// <param name="sectionGroupName">The name of the section group.</param>
            /// <returns>The configuration section group object, or null if not found.</returns>
            object GetSectionGroup(string sectionGroupName);
        }


        public interface IWebConfigurationFileMap
        {
            string MachineConfigFilename { get; set; }

            void AddVirtualDirectory(string virtualDirectory, string physicalDirectory);

            IDictionary<string, string> VirtualDirectories { get; }
        }



        public interface IHttpContextProvider
        {
            IHttpContext Current { get; }
        }






        public interface IHttpRequest
        {
            // Core URL and Path Properties
            Uri Url { get; }
            Uri UrlReferrer { get; }
            string RawUrl { get; }
            string Path { get; }
            string PathInfo { get; }
            string FilePath { get; }
            string CurrentExecutionFilePath { get; }
            string CurrentExecutionFilePathExtension { get; }
            string AppRelativeCurrentExecutionFilePath { get; }
            string ApplicationPath { get; }
            string PhysicalPath { get; }
            string PhysicalApplicationPath { get; }
            //string PathTranslated { get; }

            // HTTP Metadata
            string HttpMethod { get; }
            string RequestType { get; }
            string Scheme { get; }
            string Host { get; }
            string Protocol { get; }
            bool IsSecureConnection { get; }
            bool IsLocal { get; }
            bool IsAuthenticated { get; }
            string UserIdentity { get; }
            string UserHostAddress { get; }
            string UserAgent { get; }
            IEnumerable<string> AcceptTypes { get; }
            IEnumerable<string> UserLanguages { get; }
            Version HttpVersion { get; }

            // Browser Capabilities
            string BrowserName { get; }
            string BrowserVersion { get; }
            string BrowserPlatform { get; }
            bool BrowserSupportsJavaScript { get; }

            // Collections
           // NameValueCollection Query { get; }
            IDictionary<string, string[]> QueryMulti { get; }
            NameValueCollection Form { get; }
            IDictionary<string, string[]> FormMulti { get; }
            NameValueCollection Headers { get; }
            IHttpCookieCollection Cookies { get; }
            NameValueCollection ServerVariables { get; }

            // Raw Collections
            NameValueCollection QueryString { get; }
            NameValueCollection FormCollection { get; }
           // NameValueCollection RawHeaders { get; }
            IEnumerable<KeyValuePair<string, string>> RawCookies { get; }
            IEnumerable<KeyValuePair<string, IHttpPostedFile>> RawFiles { get; }

            // Content and Encoding
            string ContentType { get; }
            Encoding ContentEncoding { get; }
            Encoding ContentEncodingObject { get; }
            long? ContentLength { get; }
            int TotalBytes { get; }
            bool HasEntityBody { get; }

            // Streams and Body
            Stream Body { get; }
            Stream InputStream { get; }
            Task<Stream> BodyAsync();
            byte[] BinaryRead(int count);

            // Files
            IHttpFileCollection Files { get; } // Instead of IEnumerable<IHttpPostedFile>
            int FileCount { get; }
            bool HasFiles { get; }
            IHttpPostedFile GetFile(string key);
            IHttpPostedFile GetFile(int index);
            IEnumerable<string> FileKeys { get; }
            IEnumerable<string> AllFileKeys { get; }

            // Request Validation
           // bool ValidateRequest { get; }
            void ValidateInput();

            // Utility Methods
            string MapPath(string virtualPath);
            bool IsAjaxRequest();
            void Abort();
            void SaveAs(string filename, bool includeHeaders);

            // Client Certificate
            byte[] ClientCertificate { get; }

            // Indexer for convenience
            string this[string key] { get; }



        }





        public interface IHttpPostedFile
        {
            /// <summary>
            /// Gets the name of the form field that the file was uploaded with.
            /// </summary>
            string Name { get; }

            /// <summary>
            /// Gets the original file name of the uploaded file.
            /// </summary>
            string FileName { get; }

            /// <summary>
            /// Gets the MIME content type of the uploaded file.
            /// </summary>
            string ContentType { get; }

            /// <summary>
            /// Gets the length of the uploaded file in bytes.
            /// </summary>
            long Length { get; }

            /// <summary>
            /// Indicates whether the file is empty.
            /// </summary>
            bool IsEmpty { get; }
            int ContentLength { get; }

            Stream InputStream { get; } // ✅ NEW
            /// <summary>
            /// Opens a read-only stream for the uploaded file.
            /// </summary>
            Stream OpenReadStream();

            /// <summary>
            /// Saves the uploaded file to the specified path.
            /// </summary>
            void SaveAs(string path);

            /// <summary>
            /// Saves the uploaded file asynchronously to the specified path.
            /// </summary>
            Task SaveAsAsync(string path);

            /// <summary>
            /// Copies the uploaded file to a stream synchronously.
            /// </summary>
            void CopyTo(Stream target);

            /// <summary>
            /// Copies the uploaded file to a stream asynchronously.
            /// </summary>
            Task CopyToAsync(Stream target);
        }


        public interface IHttpFileCollection
        {
            int Count { get; }
            string[] AllKeys { get; }
            IHttpPostedFile this[int index] { get; }
            IHttpPostedFile this[string key] { get; }
        }




        public interface IHttpResponse
        {
            // Core Response Properties
            int StatusCode { get; set; }
            string StatusDescription { get; set; }
            string ContentType { get; set; }
            Encoding ContentEncoding { get; set; }
            long? ContentLength { get; set; }
            bool Buffer { get; set; }
            string CacheControl { get; set; }
            bool IsClientConnected { get; }
            bool SuppressContent { get; set; }
            bool TrySkipIisCustomErrors { get; set; }

            // Headers
            IDictionary<string, string> Headers { get; }
            void AddHeader(string name, string value);
            void AppendHeader(string name, string value);
            void RemoveHeader(string name);

            // Cookies
            IHttpCookieCollection Cookies { get; }

           // IHttpCookie GetCookie(string name);
          //  void AddCookie(IHttpCookie cookie);
          //  void RemoveCookie(string name);

            // Output Writing
            void Write(string content);
            void Write(char ch);
            void Write(char[] buffer, int index, int count);
            void Write(byte[] buffer);
            Task WriteAsync(Stream stream);
            void BinaryWrite(byte[] buffer);
            void OutputStreamWrite(Stream stream);
            TextWriter OutputWriter { get; }

            Stream OutputStream { get; }

            // Lifecycle Control
            void Flush();
            void Clear();
            void ClearHeaders();
            void ClearContent();
            void End();
            void Close();

            // Redirection
            void Redirect(string url, bool permanent = false);
            void RedirectPermanent(string url);

            void RedirectPermanent(string url, bool endResponse);

            void RedirectToRoute(string routeName);
            void RedirectToRoutePermanent(string routeName);

            // File Transmission
            void TransmitFile(string filename);
            void WriteFile(string filename);
            void WriteFile(string filename, bool readIntoMemory);
            void WriteFile(string filename, long offset, long length);

            // Charset and Encoding
            Encoding CharsetEncoding { get; set; }
            string Charset { get; set; }

            // Cache Control
            void SetCacheability(string cacheability);
            void SetNoStore();

            // Client Disconnect Handling
            bool ClientDisconnectedToken { get; }

            // Compression (if supported)
            void ApplyAppPathModifier(string virtualPath);

            
            // NEW cache-related properties
            int Expires { get; set; } // Minutes until expiration
            DateTime ExpiresAbsolute { get; set; } // Absolute expiration date/time

            Encoding HeaderEncoding { get; set; }
            ICachePolicy Cache { get; }

        }


        public interface ICachePolicy
        {
            void SetNoStore();
            void AppendCacheExtension(string extension);
            void SetCacheability(string cacheability);
            void SetExpires(DateTime date);
            void SetValidUntilExpires(bool validUntilExpires);
        }


        public interface ICache
        {
            object this[string key] { get; set; } // Add indexer
            void Insert(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
            object Get(string key);
            void Remove(string key);
            bool Contains(string key);
            void Clear();
        }


        public interface IHttpSessionState
        {
            /// <summary>
            /// Gets or sets a session value by key.
            /// </summary>
            /// <param name="key">The session key.</param>
            /// <returns>The session value.</returns>
            object this[string key] { get; set; }

            /// <summary>
            /// Checks if the session contains a specific key.
            /// </summary>
            /// <param name="key">The key to check.</param>
            /// <returns>True if the key exists; otherwise, false.</returns>
            bool ContainsKey(string key);

            /// <summary>
            /// Removes a value from the session by key.
            /// </summary>
            /// <param name="key">The key to remove.</param>
            void Remove(string key);

            /// <summary>
            /// Clears all session values.
            /// </summary>
            void Clear();

            /// <summary>
            /// Gets all keys in the session.
            /// </summary>
            /// <returns>An enumerable of all session keys.</returns>
            IEnumerable<string> Keys { get; }


            int Count { get; }
            string SessionID { get; }
            bool IsNewSession { get; }
            bool IsReadOnly { get; }
            int Timeout { get; set; }


            IHttpCookieMode CookieMode { get; }       // Indicates how cookies are used for session state.
            ISessionStateMode Mode { get; }          // Indicates the session state mode (InProc, StateServer, SQLServer, Custom).
            bool IsCookieless { get; }              // Indicates whether the session is using cookieless mode.

            bool IsSynchronized { get; }          // Indicates whether access to the session is thread-safe.
            object SyncRoot { get; }              // An object that can be used to synchronise access to the session.
            void CopyTo(Array array, int index);  // Copies session values to an array starting at a specific index.



            void Abandon();


            object this[int index] { get; }        // Indexed access
            void Add(string name, object value);   // Add by name
            void RemoveAt(int index);



            void RemoveAll();
            
            // Remove by index
            IEnumerator GetEnumerator();           // For iteration
            object Clone();                        // For cloning


            /// <summary>
            /// Persists the session asynchronously (for environments that require it).
            /// </summary>
            /// <returns>A task representing the asynchronous operation.</returns>
            Task CommitAsync();
        }


        public enum IHttpCookieMode
        {
            /// <summary>
            /// Cookies are always used to store session identifiers.
            /// </summary>
            UseCookies,

            /// <summary>
            /// Session identifiers are stored in the URL (cookieless).
            /// </summary>
            UseUri,

            /// <summary>
            /// The application automatically detects whether to use cookies or cookieless mode.
            /// </summary>
            AutoDetect,

            /// <summary>
            /// The application uses device profile settings to determine cookie usage.
            /// </summary>
            UseDeviceProfile
        }


        public enum ISessionStateMode
        {
            /// <summary>
            /// Session state is disabled.
            /// </summary>
            Off,

            /// <summary>
            /// Session state is stored in-process.
            /// </summary>
            InProc,

            /// <summary>
            /// Session state is stored in a state server.
            /// </summary>
            StateServer,

            /// <summary>
            /// Session state is stored in a SQL Server database.
            /// </summary>
            SQLServer,

            /// <summary>
            /// Session state is stored using a custom provider.
            /// </summary>
            Custom
        }


        public interface IHttpApplicationState
        {
            /// <summary>
            /// Gets or sets a value by key.
            /// </summary>
            object this[string key] { get; set; }
            object Get(string key);
            /// <summary>
            /// Adds a new item to the application state.
            /// </summary>
            void Add(string key, object value);

            /// <summary>
            /// Removes an item from the application state.
            /// </summary>
            void Remove(string key);

            /// <summary>
            /// Clears all items from the application state.
            /// </summary>
            void Clear();

            /// <summary>
            /// Gets all keys in the application state.
            /// </summary>
            IEnumerable<string> Keys { get; }

            /// <summary>
            /// Gets the number of items in the application state.
            /// </summary>
            int Count { get; }

            /// <summary>
            /// Locks the application state for exclusive access.
            /// </summary>
            void Lock();

            /// <summary>
            /// Unlocks the application state.
            /// </summary>
            void UnLock();
        }


        public interface IHttpCookie
        {
            string Name { get; }
            string Value { get; set; }
            DateTime? Expires { get; set; }
            string Path { get; set; }
            string Domain { get; set; } // Add this
            bool HttpOnly { get; set; }
            bool Secure { get; set; }
        }


        public interface IHttpCookieCollection
        {
            int Count { get; }
            string[] AllKeys { get; }
            IHttpCookie this[int index] { get; }
            IHttpCookie this[string name] { get; }
            void Add(IHttpCookie cookie);
            void Remove(string name);
        }

        public interface IHttpServerUtility
        {
            /// <summary>
            /// Maps a virtual path to a physical path on the server.
            /// </summary>
            string MapPath(string virtualPath);

            /// <summary>
            /// Encodes a string for use in a URL.
            /// </summary>
            string UrlEncode(string value);

            /// <summary>
            /// Decodes a URL-encoded string.
            /// </summary>
            string UrlDecode(string value);

            /// <summary>
            /// Encodes a string for HTML output.
            /// </summary>
            string HtmlEncode(string value);

            /// <summary>
            /// Decodes an HTML-encoded string.
            /// </summary>
            string HtmlDecode(string value);

            /// <summary>
            /// Executes a script on the server.
            /// </summary>
            void Execute(string path);

            /// <summary>
            /// Transfers execution to another page.
            /// </summary>
            void Transfer(string path);

            /// <summary>
            /// Clears the error collection.
            /// </summary>
            void ClearError();

            string MachineName { get; }

            /// <summary>
            /// Gets the last error that occurred on the server.
            /// </summary>
            Exception GetLastError();

            int ScriptTimeout { get; set; }
        }


        public class HttpCookie : IHttpCookie
        {
            public string Name { get; }
            public string Value { get; set; }
            public string Path { get; set; }
            public string Domain { get; set; }
            public DateTime? Expires { get; set; }
            public bool HttpOnly { get; set; }
            public bool Secure { get; set; }
            public string SameSite { get; set; }
            public NameValueCollection Values { get; } = new NameValueCollection();

            public HttpCookie(string name, string value = null)
            {
                Name = name ?? throw new ArgumentNullException(nameof(name));
                Value = value;
            }

            public override string ToString()
            {
                return $"{Name}={Value}";
            }
        }


    }
}
