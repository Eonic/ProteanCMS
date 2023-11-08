using System;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using static System.Web.HttpUtility;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
using Microsoft.VisualBasic.CompilerServices; // Install-Package Microsoft.VisualBasic

// ' ---------------------------------------------------------
// ' Class       :   Protean.Tools.Http.WebRequest
// ' Author      :   Ali Granger
// ' Website     :   www.eonic.co.uk
// ' Description :   HTTP and Web related classes and functions.
// '
// ' (c) Copyright 2010 Eonic Ltd.
// ' ---------------------------------------------------------


namespace Protean.Tools.Http
{

    public enum URLShorteningService
    {

        BitLy,
        Isgd,
        TinyUrl

    }

    /// <summary>
    /// Protean.Tools.Http.Utils
    /// Collection of shared Http/Web utils
    /// </summary>
    /// <remarks></remarks>
    public class Utils
    {

        public static Uri BuildURIFromRequest(HttpRequest currentRequest, NameValueCollection parameters = null, string parametersToExclude = "")
        {

            var uriBuilder = new StringBuilder();
            NameValueCollection queryString = null;
            string[] exclusions = parametersToExclude.Split(',');
            try
            {

                Uri requestUri = currentRequest.Url;

                // Build the Querystring.
                queryString = HttpUtility.ParseQueryString(currentRequest.QueryString.ToString());

                // Add the parameters, if specified.
                // Parameters override existing ones
                if (parameters != null)
                {

                    foreach (string key in parameters.AllKeys)
                    {
                        queryString.Remove(key);
                        queryString.Add(key, parameters[key]);
                    }

                }

                // Check the exclusions
                foreach (string exclusion in exclusions)
                {
                    if (queryString[exclusion] != null)
                    {
                        queryString.Remove(exclusion);
                    }
                }

                // Calculate the Absolute path from the original path (not the rewritten one)
                string originalPath = currentRequest.RawUrl;
                originalPath = originalPath.Substring(0, Conversions.ToInteger(Interaction.IIf(originalPath.Contains("?"), originalPath.IndexOf("?"), originalPath.Length)));

                // Build the URI
                return BuildURI(requestUri.Host, requestUri.Scheme + "://", originalPath, queryString.ToString());
            }


            catch (Exception ex)
            {
                // If an error is encountered return Nothing
                return null;
            }

        }

        public static Uri BuildURI(string host, string protocol = "http://", string path = "/", string querystring = "")
        {

            var uriBuilder = new StringBuilder();

            try
            {
                // Build the string

                if (string.IsNullOrEmpty(host))
                {
                    // Empty host
                    return null;
                }
                else
                {

                    uriBuilder.Append(protocol.Trim());
                    uriBuilder.Append(host.Trim().Trim('/'));
                    uriBuilder.Append("/");
                    uriBuilder.Append(path.TrimStart('/'));
                    if (!string.IsNullOrEmpty(querystring))
                    {
                        uriBuilder.Append("?");
                        uriBuilder.Append(querystring.TrimStart('?'));
                    }

                    if (Uri.IsWellFormedUriString(uriBuilder.ToString(), UriKind.Absolute))
                    {
                        return new Uri(uriBuilder.ToString());
                    }
                    else
                    {
                        return null;
                    }

                }
            }

            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlToShorten"></param>
        /// <param name="shorteningService"></param>
        /// <returns></returns>
        /// <remarks>This is derived from TwitterVB, as we wanted to add other services.</remarks>
        public static string ShortenURL(string urlToShorten, URLShorteningService shorteningService = URLShorteningService.TinyUrl, string bitlyUser = "", string bitlyKey = "")
        {

            var shorteningRequest = new Tools.Http.WebRequest("text/html", "GET");

            string shortenedURL = string.Empty;

            switch (shorteningService)
            {
                case URLShorteningService.Isgd:
                    {
                        shortenedURL = shorteningRequest.Send(string.Format("http://is.gd/api.php?longurl={0}", UrlEncode(urlToShorten)));
                        break;
                    }

                case URLShorteningService.TinyUrl:
                    {
                        shortenedURL = shorteningRequest.Send(string.Format("http://tinyurl.com/api-create.php?url={0}", UrlEncode(urlToShorten)));
                        break;
                    }

                case URLShorteningService.BitLy:
                    {
                        shortenedURL = shorteningRequest.Send(string.Format("http://api.bit.ly/v3/shorten?login={0}&apiKey={1}&longUrl={2}&format=txt", bitlyUser, bitlyKey, urlToShorten));
                        break;
                    }

            }

            return shortenedURL;

        }
    }



}
