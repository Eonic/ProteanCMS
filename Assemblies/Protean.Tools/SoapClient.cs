using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic

namespace Protean.Tools
{
    public class SoapClient
    {
        #region Declarations
        public SoapResponse oResults = new SoapResponse();
        private string cServiceUrl;
        private string cServiceNamespace; // "http://www.eonic.co.uk/ewcommon/Services/"
        private string cActionName;
        private bool bRemoveReturnEnvelope = false;
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);
        private const string mcModuleName = "Protean.Tools.SoapClient";
        private bool bErrorReturned = false;
        public System.Web.SessionState.HttpSessionState moSession;
        public Dictionary<string, string> Headers = new Dictionary<string, string>();


        #endregion

        #region Properties
        public string Url
        {
            get
            {
                return cServiceUrl;
            }
            set
            {
                cServiceUrl = value;
            }
        }

        public string Namespace
        {
            get
            {
                return cServiceNamespace;
            }
            set
            {
                if (!(value.Substring(value.Length - 1, 1) == "/"))
                    value += "/";
                cServiceNamespace = value;
            }
        }

        public string Action
        {
            get
            {
                return cActionName;
            }
            set
            {
                cActionName = value;
            }
        }

        public bool RemoveReturnSoapEnvelope
        {
            get
            {
                return bRemoveReturnEnvelope;
            }
            set
            {
                bRemoveReturnEnvelope = value;
            }
        }

        public bool ErrorReturned
        {
            get
            {
                return bErrorReturned;
            }
        }

        #endregion

        #region Private Procedures


        private void SendSoapRequest(string soapBody)
        {

            System.Net.WebRequest soapRequest;
            string cProcessInfo = "sendSoapRequest";

            try
            {
                // reset the results so we don't return the same in a loop.
                oResults = null;
                oResults = new SoapResponse();

                // If cServiceUrl.EndsWith(cActionName) Then
                soapRequest = System.Net.WebRequest.Create(cServiceUrl);
                // Else
                // soapRequest = WebRequest.Create(cServiceUrl & "/" & cActionName)
                // End If

                soapRequest.Timeout = System.Threading.Timeout.Infinite;

                string namespacestring = Namespace + cActionName;

                soapRequest.Headers.Add("SOAPAction", namespacestring);

                // Moved into OpenQuote not needed here
                // If Not moSession Is Nothing Then
                // If moSession("JSESSIONID") <> "" Then
                // soapRequest.Headers.Add("Cookie", "JSESSIONID=" & moSession("JSESSIONID"))
                // End If
                // End If

                HttpWebRequest serviceRequest = (HttpWebRequest)soapRequest;

                serviceRequest.ContentType = "text/Xml";
                foreach (var pair in Headers)
                    serviceRequest.Headers.Add(pair.Key, pair.Value);

                addSoap(serviceRequest, soapBody);

                ProduceReturn(ReturnSoapResponse(serviceRequest));
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SendSoapRequest", ex, cProcessInfo));
            }
        }

        private void addSoap(HttpWebRequest serviceRequest, string soapBody)
        {
            string cProcessInfo = "addSoap";

            try
            {

                serviceRequest.Method = "POST";
                var encoding = new UTF8Encoding();
                byte[] bodybytes = encoding.GetBytes(soapBody);
                serviceRequest.ContentLength = bodybytes.Length;
                Stream bodyStream = serviceRequest.GetRequestStream();
                bodyStream.Write(bodybytes, 0, bodybytes.Length);
                bodyStream.Close();
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addSoap", ex, cProcessInfo));
            }

        }

        private string ReturnSoapResponse(HttpWebRequest serviceRequest)
        {

            StreamReader serviceResponseStream = null;
            string serviceResponseBody;
            string cProcessInfo = "ReturnSoapResponse";
            try
            {
                WebResponse servResponse;
                serviceRequest.Timeout = System.Threading.Timeout.Infinite;

                servResponse = serviceRequest.GetResponse();


                HttpWebResponse serviceResponse = (HttpWebResponse)servResponse;
                serviceResponseStream = new StreamReader(serviceResponse.GetResponseStream(), Encoding.UTF8);

                if (moSession != null)
                {
                    GetCookieHeaders(serviceResponse);
                }

                GetErrorHeaders(serviceResponse);
            }

            catch (Exception ex)
            {

                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ReturnSoapRequest", ex, cProcessInfo));

            }

            if (serviceResponseStream is null)
            {
                serviceResponseBody = null; // "<error>Response Stream Error</error>" 'Nothing is required for Openquote please don't change
            }
            else
            {
                serviceResponseBody = serviceResponseStream.ReadToEnd();
                serviceResponseStream.Close();
            }
            return serviceResponseBody;

        }

        private void GetErrorHeaders(HttpWebResponse serviceResponse)
        {
            try
            {
                int i;
                bErrorReturned = false;
                var loopTo = serviceResponse.Headers.Count - 1;
                for (i = 0; i <= loopTo; i++)
                {
                    switch (serviceResponse.Headers.Keys[i])
                    {
                        case "Error":
                            {
                                bErrorReturned = true;
                                return;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetErrorHeaders", ex, ""));
            }
        }

        private void GetCookieHeaders(HttpWebResponse serviceResponse)
        {
            try
            {
                int i;
                bErrorReturned = false;
                var loopTo = serviceResponse.Headers.Count - 1;
                for (i = 0; i <= loopTo; i++)
                {
                    switch (serviceResponse.Headers.Keys[i])
                    {
                        case "Set-Cookie":
                            {
                                string[] aCookies = serviceResponse.Headers.GetValues("Set-Cookie");
                                long j;
                                long k;

                                var loopTo1 = (long)Information.UBound(aCookies);
                                for (j = 0L; j <= loopTo1; j++)
                                {
                                    string[] aCookies2 = aCookies[(int)j].Split(";");
                                    var loopTo2 = (long)Information.UBound(aCookies2);
                                    for (k = 0L; k <= loopTo2; k++)
                                    {
                                        string[] aCookies3 = Strings.Split(aCookies2[(int)k], "=");
                                        moSession[aCookies3[0]] = aCookies3[1];
                                    }
                                }
                                return;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetErrorHeaders", ex, ""));
            }
        }

        public XmlDocument getSoapEnvelope(string sBodyElmt)
        {

            XmlDocument oXml;
            string sEnvelope;
            string cProcessInfo = "getSoapEnvelope";

            try
            {
                sEnvelope = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XmlSchema\" xmlns:xsi=\"http://www.w3.org/2001/XmlSchema-instance\"><soap:Body/></soap:Envelope>";

                oXml = new XmlDocument();
                oXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                oXml.LoadXml(sEnvelope);

                // add the soap namespace to the nametable of the XmlDocument to allow xpath to query the namespace
                var nsmgr = new XmlNamespaceManager(oXml.NameTable);
                nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");

                oXml.SelectSingleNode("/soap:Envelope/soap:Body", nsmgr).InnerXml = sBodyElmt;
                oXml.InnerXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + oXml.InnerXml;
                return oXml;
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getSoapEnvelope", ex, cProcessInfo));
                return null;
            }
        }

        private void ProduceReturn(string cReturn)
        {
            try
            {
                if (RemoveReturnSoapEnvelope)
                {
                    cReturn = cReturn.Replace("soap:Envelope", "soapEnvelope");
                    cReturn = cReturn.Replace("soap:Body", "soapBody");

                    // cReturn = Replace(cReturn, "xmlns", "exemelnamespace")
                    // cReturn = Replace(cReturn, ":soap", "")
                    var oNewXml = new XmlDocument();
                    oNewXml.LoadXml(cReturn);
                    oResults.LoadXml(oNewXml.SelectSingleNode("soapEnvelope/soapBody").InnerXml);
                    oResults.InnerXml = oResults.InnerXml.Replace("xmlns", "exemelnamespace");
                }
                else
                {
                    // cReturn = Replace(cReturn, "env:", "")
                    // cReturn = Replace(cReturn, "ns1:", "")
                    oResults.LoadXml(cReturn);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SendRequest", ex, ""));
            }
        }



        #endregion

        public string SendRequest(string oSoapBody)
        {
            try
            {
                oSoapBody = Strings.Replace(oSoapBody, "exemelnamespace", "xmlns");
                oSoapBody = getSoapEnvelope(oSoapBody).OuterXml;
                SendSoapRequest(oSoapBody);
                return oResults.OuterXml;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SendRequest", ex, ""));
                return "";
            }
        }

        public async Task<string> SendRequestAsync(string oSoapBody)
        {
            try
            {
                oSoapBody = Strings.Replace(oSoapBody, "exemelnamespace", "xmlns");
                oSoapBody = getSoapEnvelope(oSoapBody).OuterXml;
                SendSoapRequest(oSoapBody);
                return oResults.OuterXml;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SendRequest", ex, ""));
                return "";
            }
        }


        public partial class SoapResponse : XmlDocument
        {

            public XmlElement getBody
            {
                get
                {
                    return (XmlElement)SelectSingleNode("Body");
                }
            }


        }

        // Public Class headers
        // Inherits dic
        // End Class
    }
}
