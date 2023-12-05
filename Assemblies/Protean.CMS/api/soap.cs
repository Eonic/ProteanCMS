
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using Microsoft.VisualBasic;

namespace Protean
{
    public class SoapClient
    {

        public XmlDocument results = new XmlDocument();
        public object query;
        public string ServiceUrl;
        public string ServiceNamespace;

        public System.Web.HttpContext moCtx = System.Web.HttpContext.Current;
        public System.Web.HttpRequest goRequest;

        public new string mcModuleName = "Eonic.SoapClient";

        public SoapClient()
        {
            goRequest = moCtx.Request;
        }

        public void SendSoapRequest(string actionName, string soapBody)
        {

            WebRequest soapRequest;
            string cProcessInfo = "sendSoapRequest";

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // check for fullpath
                if (!(Strings.InStr(ServiceUrl, "http") == 1))
                {
                    if (Strings.LCase(goRequest.ServerVariables["HTTPS"]) == "on")
                    {
                        ServiceUrl = "https://" + goRequest.ServerVariables["SERVER_NAME"] + ServiceUrl;
                    }
                    else
                    {
                        ServiceUrl = "http://" + goRequest.ServerVariables["SERVER_NAME"] + ServiceUrl;
                    }
                }

                soapRequest = WebRequest.Create(ServiceUrl);

                soapRequest.Headers.Add("SOAPAction", "http://www.eonic.co.uk/ewcommon/Services/" + actionName);
                soapRequest.ContentType = "text/xml; charset=UTF-8";

                // soapRequest.Method = "POST"

                HttpWebRequest serviceRequest = (HttpWebRequest)soapRequest;

                soapBody = Strings.Replace(soapBody, "xmlns=\"\"", "");
                serviceRequest.ContentType = "text/xml; charset=UTF-8";
                serviceRequest.Accept = "text/xml";
                serviceRequest.Method = "POST";
                serviceRequest.Headers["SessionReferer"] = moCtx.Request.UrlReferrer.OriginalString; // moCtx.Session("Referrer")
                addSoap(serviceRequest, soapBody);

                cProcessInfo = "soapBody:" + soapBody;

                string soapResponse = ReturnSoapResponse(serviceRequest);
                cProcessInfo = ServiceUrl + "<br/><br/>";
                cProcessInfo = cProcessInfo + actionName + "<br/><br/>";
                cProcessInfo = cProcessInfo + soapResponse;
                results.LoadXml(soapResponse);
            }

            catch (Exception ex)
            {
                results.LoadXml("<error>" + ex.Message + "</error>");
                // returnException(mcModuleName, "sendSoapRequest", ex, "", cProcessInfo, gbDebug)
            }
        }

        private void addSoap(HttpWebRequest serviceRequest, string soapBody)
        {
            string cProcessInfo = "addSoap";

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var encoding = new UTF8Encoding();
                byte[] bodybytes = encoding.GetBytes(soapBody);
                serviceRequest.ContentLength = bodybytes.Length;
                var bodyStream = serviceRequest.GetRequestStream();
                bodyStream.Write(bodybytes, 0, bodybytes.Length);
                bodyStream.Close();
            }

            catch (Exception)
            {
                // returnException(mcModuleName, "addSoap", ex, "", cProcessInfo, gbDebug)
            }

        }

        private string ReturnSoapResponse(HttpWebRequest serviceRequest)
        {

            StreamReader serviceResponseStream = null;
            string serviceResponseBody;
            // Dim cProcessInfo As String '= "ReturnSoapResponse: " & serviceRequest.Address.ToString & " - " & serviceRequest.Method & " https:" & goRequest.ServerVariables("HTTPS")
            try
            {
                WebResponse servResponse;
                servResponse = serviceRequest.GetResponse();
                HttpWebResponse serviceResponse = (HttpWebResponse)servResponse;
                // serviceResponseStream = New StreamReader(serviceResponse.GetResponseStream, System.Text.Encoding.ASCII)
                serviceResponseStream = new StreamReader(serviceResponse.GetResponseStream(), Encoding.UTF8);
            }

            catch (WebException)
            {
            }

            // Dim serviceResponse As HttpWebResponse = ex.Response
            // serviceResponseStream = New StreamReader(serviceResponse.GetResponseStream, System.Text.Encoding.ASCII)
            // returnException(mcModuleName, "ReturnSoapResponse", ex, "", cProcessInfo, gbDebug)

            catch (Exception)
            {

                // Return ex.Message.tostring
                // returnException(mcModuleName, "ReturnSoapResponse", ex, "", cProcessInfo, gbDebug)

            }
            serviceResponseBody = serviceResponseStream.ReadToEnd();
            serviceResponseStream.Close();
            return serviceResponseBody;

        }

        public XmlDocument getSoapEnvelope(XmlElement oBodyElmt)
        {

            XmlDocument oXml;
            string sEnvelope;
            string cProcessInfo = string.Empty;

            try
            {

                sEnvelope = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><soap:Body/></soap:Envelope>";

                oXml = new XmlDocument();
                oXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                oXml.LoadXml(sEnvelope);
                oXml.SelectSingleNode("soap:Envelope/soap:Body").AppendChild(oBodyElmt);

                return oXml;
            }

            catch (Exception )
            {
                // returnException(mcModuleName, "getSoapEnvelope", ex, "", cProcessInfo, gbDebug)
                return null;
            }
        }

        public XmlDocument getSoapEnvelope(string sBodyElmt)
        {

            XmlDocument oXml;
            string sEnvelope;
            string cProcessInfo = string.Empty;

            try
            {
                sEnvelope = "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><soap:Body/></soap:Envelope>";

                oXml = new XmlDocument();
                oXml.CreateXmlDeclaration("1.0", "UTF-8", "yes");
                oXml.LoadXml(sEnvelope);

                // add the soap namespace to the nametable of the xmlDocument to allow xpath to query the namespace
                var nsmgr = new XmlNamespaceManager(oXml.NameTable);
                nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");

                oXml.SelectSingleNode("/soap:Envelope/soap:Body", nsmgr).InnerXml = sBodyElmt;

                return oXml;
            }

            catch (Exception)
            {
                // returnException(mcModuleName, "getSoapEnvelope", ex, "", cProcessInfo, gbDebug)
                return null;
            }
        }



    }
}