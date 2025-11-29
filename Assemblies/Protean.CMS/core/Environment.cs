using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
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
            string PathTranslated { get; }

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

            // Browser Capabilities
            string BrowserName { get; }
            string BrowserVersion { get; }
            string BrowserPlatform { get; }
            bool BrowserSupportsJavaScript { get; }

            // Collections
            IDictionary<string, string> Query { get; }
            IDictionary<string, string[]> QueryMulti { get; }
            IDictionary<string, string> Form { get; }
            IDictionary<string, string[]> FormMulti { get; }
            IDictionary<string, string> Headers { get; }
            IDictionary<string, string> Cookies { get; }
            IDictionary<string, string> ServerVariables { get; }

            // Content and Encoding
            string ContentType { get; }
            string ContentEncoding { get; }
            Encoding ContentEncodingObject { get; }
            long? ContentLength { get; }
            int TotalBytes { get; }

            // Streams and Body
            Stream Body { get; }
            Stream InputStream { get; }
            Task<Stream> BodyAsync();
            byte[] BinaryRead(int count);

            // Files
            IEnumerable<IHttpPostedFile> Files { get; }
            int FileCount { get; }
            bool HasFiles { get; }
            IHttpPostedFile GetFile(string key);
            IHttpPostedFile GetFile(int index);
            IEnumerable<string> FileKeys { get; }
            IEnumerable<string> AllFileKeys { get; }

            // Request Validation
            bool ValidateRequest { get; }
            void ValidateInput();

            // Utility Methods
            string MapPath(string virtualPath);
            bool IsAjaxRequest();
            void Abort();

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




        public interface IHttpResponse
        {
            /// <summary>
            /// Gets or sets the HTTP status code for the response.
            /// </summary>
            int StatusCode { get; set; }

            /// <summary>
            /// Gets or sets the content type of the response.
            /// </summary>
            string ContentType { get; set; }

            /// <summary>
            /// Adds a header to the response.
            /// </summary>
            /// <param name="name">The name of the header.</param>
            /// <param name="value">The value of the header.</param>
            void AddHeader(string name, string value);

            /// <summary>
            /// Writes a string to the response body.
            /// </summary>
            /// <param name="content">The content to write.</param>
            void Write(string content);

            /// <summary>
            /// Writes a byte array to the response body.
            /// </summary>
            /// <param name="buffer">The byte array to write.</param>
            void Write(byte[] buffer);

            /// <summary>
            /// Writes a stream to the response body asynchronously.
            /// </summary>
            /// <param name="stream">The stream to write.</param>
            /// <returns>A task representing the asynchronous operation.</returns>
            Task WriteAsync(Stream stream);

            /// <summary>
            /// Redirects the client to the specified URL.
            /// </summary>
            /// <param name="url">The URL to redirect to.</param>
            void Redirect(string url);

            /// <summary>
            /// Ends the response (flushes and closes).
            /// </summary>
            void End();
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


            HttpCookieMode CookieMode { get; }       // Indicates how cookies are used for session state.
            SessionStateMode Mode { get; }          // Indicates the session state mode (InProc, StateServer, SQLServer, Custom).
            bool IsCookieless { get; }              // Indicates whether the session is using cookieless mode.

            bool IsSynchronized { get; }          // Indicates whether access to the session is thread-safe.
            object SyncRoot { get; }              // An object that can be used to synchronise access to the session.
            void CopyTo(Array array, int index);  // Copies session values to an array starting at a specific index.



            void Abandon();


            object this[int index] { get; }        // Indexed access
            void Add(string name, object value);   // Add by name
            void RemoveAt(int index);              // Remove by index
            IEnumerator GetEnumerator();           // For iteration
            object Clone();                        // For cloning


            /// <summary>
            /// Persists the session asynchronously (for environments that require it).
            /// </summary>
            /// <returns>A task representing the asynchronous operation.</returns>
            Task CommitAsync();
        }

    }
}
