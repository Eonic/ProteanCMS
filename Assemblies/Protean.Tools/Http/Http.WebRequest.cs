
using System;
using System.IO;
using System.Net;
using System.Text;
using static System.Text.Encoding;

namespace Protean.Tools.Http
{
    /// <summary>
    /// Simplified WebRequest class.
    /// </summary>
    /// <remarks>TODO: Needs a GET method</remarks>
    public partial class WebRequest
    {

        #region Declarations

        private string _moduleName = "Protean.Tools.WebRequest";

        private string _contentType = "text/html";
        private long _contentLength = 0L;
        private string _userAgent = "";
        private ASCIIEncoding _requestEncoding = new ASCIIEncoding();
        private string _requestBody;
        private byte[] _requestBytes;
        private string _method = "POST";
        private bool _includeResponse = true;

        #endregion

        #region Events

        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        #endregion

        #region Constructor

        public WebRequest()
        {

        }


        public WebRequest(string contentType)
        {
            ContentType = contentType;
        }

        public WebRequest(string contentType, string method)
        {
            ContentType = contentType;
            Method = method;
        }

        public WebRequest(string contentType, string method, string userAgent)
        {
            ContentType = contentType;
            Method = method;
            UserAgent = userAgent;
        }

        #endregion

        #region Private Properties

        #endregion

        #region Public Properties

        public string ContentType
        {
            get
            {
                return _contentType;
            }
            set
            {
                _contentType = value;
            }
        }

        public string Method
        {
            get
            {
                return _method;
            }
            set
            {
                _method = value;
            }
        }

        public string UserAgent
        {
            get
            {
                return _userAgent;
            }
            set
            {
                _userAgent = value;
            }
        }

        public bool IncludeResponse
        {
            get
            {
                return _includeResponse;
            }
            set
            {
                _includeResponse = value;
            }
        }


        public string RequestBody
        {
            get
            {
                return _requestBody;
            }
            set
            {
                _requestBody = value;
                _requestBytes = _requestEncoding.GetBytes(_requestBody);
                _contentLength = _requestBytes.Length;
            }
        }

        public int ContentLength
        {
            get
            {
                return (int)_contentLength;
            }
        }
        #endregion

        #region Private Members

        #endregion

        #region Public Members

        /// <summary>
        /// Instigate a synchronous request and response
        /// </summary>
        /// <param name="url">The URI to send the request to</param>
        /// <param name="request">The request string to send</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Send(string url, string request = "")
        {

            try
            {
                if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                {
                    return Send(new Uri(url), request);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(_moduleName, "Send(String)", ex, ""));
                return "";
            }

        }

        /// <summary>
        /// Instigate a synchronous request and response
        /// </summary>
        /// <param name="url">The URI to send the request to</param>
        /// <param name="request">The request string to send</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public string Send(Uri url, string request = "")
        {
            try
            {

                // Create the response object
                HttpWebRequest newRequest = default;
                HttpWebResponse newResponse = default;
                Stream requestStream = null;
                StreamReader responseStream = null;
                string responseOutput = "";

                if (!string.IsNullOrEmpty(request))
                    RequestBody = request;

                if (!(ContentLength == 0 & Method == "POST"))
                {
                    // Create the request object
                    newRequest = (HttpWebRequest)HttpWebRequest.Create(url);

                    // Set the standard headers
                    newRequest.ContentType = ContentType;
                    newRequest.ContentLength = ContentLength;
                    newRequest.Method = Method;
                    // newRequest.MaximumAutomaticRedirections = 4
                    // newRequest.MaximumResponseHeadersLength = 4

                    // Set the optional headers
                    if (!string.IsNullOrEmpty(UserAgent))
                        newRequest.Headers["User-Agent"] = UserAgent;

                    // POST the request body
                    if (Method == "POST")
                    {
                        requestStream = newRequest.GetRequestStream();
                        requestStream.Write(_requestBytes, 0, ContentLength);
                        requestStream.Close();
                    }


                    if (IncludeResponse)
                    {
                        // Get the response
                        newResponse = (HttpWebResponse)newRequest.GetResponse();
                        requestStream = newResponse.GetResponseStream();
                        responseStream = new StreamReader(requestStream, UTF8);
                        responseOutput = responseStream.ReadToEnd();

                        requestStream.Close();
                        responseStream.Close();
                        newResponse.Close();
                    }

                }

                return responseOutput;
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(_moduleName, "Send(Uri)", ex, ""));
                return "";
            }
        }

        #endregion

    }

}
