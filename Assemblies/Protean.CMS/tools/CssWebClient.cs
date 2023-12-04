
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;

namespace Protean
{

    public class CssWebClient
    {

        public XmlDocument results = new XmlDocument();
        public object query;
        public string ServiceNamespace;

        public System.Web.HttpContext moCtx;
        public System.Web.HttpRequest goRequest;
        private string msException;

        private List<string> cssSplit = new List<string>();
        public List<string> CssSplits
        {
            get
            {
                return cssSplit;
            }
        }

        private List<string> serviceUrls = new List<string>();
        public List<string> ServiceUrlsList
        {
            get
            {
                return serviceUrls;
            }
            set
            {
                serviceUrls = value;
            }
        }

        private string fullCss;
        public string FullCssFile
        {
            get
            {
                return fullCss;
            }
        }

        public new string mcModuleName = "Eonic.CssWebClient";

        public CssWebClient(System.Web.HttpContext context, ref string sException)
        {
            moCtx = context;
            goRequest = context.Request;
        }


        public void SendCssHttpHandlerRequest()
        {
            WebRequest httpHandlerRequest;
            string cProcessInfo = "sendHttpHandlerRequest";
            System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

            try
            {
                foreach (string Serviceurl in ServiceUrlsList)
                {
                    // check for fullpath
                    string origServiceUrl = Serviceurl;

                    // PerfMon.Log("CssWebClient", "SendCssHttpHandlerRequest", "start-" & Serviceurl)

                    if (goRequest.ServerVariables["SERVER_NAME"] == "localhost" & goRequest.ServerVariables["SERVER_PORT"] != "80")
                    {
                        origServiceUrl = ":" + goRequest.ServerVariables["SERVER_PORT"] + Serviceurl;
                    }

                    if (!(Strings.InStr(Serviceurl, "http") == 1))
                    {
                        if (Strings.LCase(goRequest.ServerVariables["HTTPS"]) == "on")
                        {
                            origServiceUrl = "https://" + goRequest.ServerVariables["SERVER_NAME"] + Serviceurl;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            ServicePointManager.ServerCertificateValidationCallback +=
                                (sender, cert, chain, error) =>
                                {
                                    if (cert.GetCertHashString() == "xxxxxxxxxxxxxxxx")
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return error == SslPolicyErrors.None;
                                    }
                                };
                            // ServicePointManager.ServerCertificateValidationCallback = new Func<object, System.Security.Cryptography.X509Certificates.X509Certificate, System.Security.Cryptography.X509Certificates.X509Chain, SslPolicyErrors, bool>((se, cert, chain, sslerror) => true);
                        }
                        else
                        {
                            origServiceUrl = "http://" + goRequest.ServerVariables["SERVER_NAME"] + Serviceurl;
                        }
                    }
                   
                    cProcessInfo = origServiceUrl;

                    httpHandlerRequest = WebRequest.Create(Serviceurl);

                    HttpWebRequest serviceRequest = (HttpWebRequest)httpHandlerRequest;

                    HttpWebResponse response = (HttpWebResponse)serviceRequest.GetResponse();
                    // PerfMon.Log("CssWebClient", "SendCssHttpHandlerRequest", "end-" & Serviceurl)
                    string strResponse;
                    using (var receiveStream = response.GetResponseStream())
                    {
                        using (var readStream = new StreamReader(receiveStream, Encoding.UTF8))
                        {
                            strResponse = readStream.ReadToEnd();
                        }
                    }
                    strResponse = strResponse.Replace(Constants.vbLf, "");
                    fullCss = strResponse;

                    ClearApplicationCache(origServiceUrl);
                    ServicePointManager.ServerCertificateValidationCallback = null;
                }
                int cssSplit = Conversions.ToInteger(Interaction.IIf(string.IsNullOrEmpty(moConfig["cssSplit"]), 2000, moConfig["cssSplit"]));
                ComputeCSS(fullCss, cssSplit);
            }
            catch (Exception ex)
            {
                cProcessInfo = ex.Message;
                // WE DO NOT !!!!! want this to return an html exception becuase it will throw a loop.
                // returnException(msException, mcModuleName, "SendHttpHandlerRequest", ex, "", cProcessInfo, gbDebug)
            }
        }

        public void ClearApplicationCache(string Serviceurl)
        {

            var keys = new List<string>();

            // retrieve application Cache enumerator
            var enumerator = moCtx.Cache.GetEnumerator();

            // copy all keys that currently exist in Cache
            while (enumerator.MoveNext())
                keys.Add(enumerator.Key.ToString());

            // delete every key from cache
            for (int i = 0, loopTo = keys.Count - 1; i <= loopTo; i++)
            {
                if (keys[i].Contains(Serviceurl.ToLower()))
                {
                    moCtx.Cache.Remove(keys[i]);
                }
            }
        }

        /// <summary>
        /// This function is called to get around the IE9 CSS selector limit, max CSS selectors is 4096 so we need to split the file
        /// </summary>
        /// <param name="css">The aggregated css string</param>
        /// <param name="maxRules">the max number of rules to contain within a css document</param>
        /// <remarks>Currently should not risk setting the maxRules to the limit, as the regex may not find all selectors according to specification</remarks>
        private void ComputeCSS(string css, int maxRules)
        {
            var matches = Regex.Matches(css, @"\}\n\.|\}\.|,\.|,\n\."); // this regex expression will search for ',.' OR '}.' OR '}[linefeed].'
            string cProcessInfo = "ComputeCSS";
            try
            {
                if (matches.Count > maxRules)
                {
                    if (!(matches[maxRules].Value == "}."))
                    {
                        ComputeCSS(css, maxRules + 1); // call this function recursively until we find a value that we can safely split on ("}.")
                    }
                    else
                    {
                        cssSplit.Add(css.Substring(0, matches[maxRules].Index + 1));
                        // we have more matches than our maximum number of rules so call recursively with the remaining css
                        ComputeCSS(css.Substring(matches[maxRules].Index + 1, css.Length - 1 - matches[maxRules].Index), maxRules);
                    }
                }

                else
                {
                    // add the final piece of css to the List and exit
                    cssSplit.Add(css.Substring(0, css.Length));
                    return;
                }
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "ComputeCSS", ex, "", cProcessInfo, gbDebug);
            }
        }

    }
}