using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Windows;
using System.Xml;
//using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Ajax.Utilities;
using Protean.Tools;

namespace Protean
{
    public static partial class stdTools
    {

        public static bool mbException;
        // Public oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")

        public static System.Web.HttpServerUtility goServer = System.Web.HttpContext.Current.Server;
        public static System.Web.HttpApplicationState goApp = System.Web.HttpContext.Current.Application;

        // Public msException As String = "" 'TODO !-!IMPORTANT!-! WHEN ERROR EVENTS ARE ESTABLISHED THIS SHOULD BE MOVED INSIDE WEB!!!!!
        public static bool mbDBError = false;

        public static bool gbDebug = false; // make sure this is False when doing a release

        public enum SortDirection
        {

            Ascending = 1,
            Descending = 0

        }

        public enum PollBlockReason
        {
            None = 0,
            RegisteredUsersOnly = 1,
            CookieFound = 2,
            LogFound = 3,
            JustVoted = 4,
            Excluded = 5,
            PollNotAvailable = 6
        }

        public enum ResponseType
        {
            Undefined = 0,
            Alert = 1,
            Hint = 2,
            Help = 3,
            Redirect = 4
        }

        public static string[] SortDirectionVal = new string[] { "descending", "ascending" };


        /// <summary>
        /// Modern overload that accepts IHttpContext abstraction for .NET Core compatibility
        /// </summary>
        public static void returnException(
            ref string sException, 
            string vstrModuleName, 
            string vstrRoutineName, 
            Exception oException,
            System.Web.HttpContext httpContext,
            string xsltTemplatePath = "/ewcommon/xsl/standard.xsl", 
            string vstrFurtherInfo = "", 
            bool bDebug = false, 
            string cSubjectLinePrefix = "")
        {
            ReturnExceptionCore(
                ref sException, 
                vstrModuleName, 
                vstrRoutineName, 
                oException, 
                httpContext,
                xsltTemplatePath, 
                vstrFurtherInfo, 
                bDebug, 
                cSubjectLinePrefix);
        }

        /// <summary>
        /// Legacy overload for backward compatibility - auto-resolves current HttpContext
        /// </summary>
        [Obsolete("Use overload with IHttpContext parameter for .NET Core compatibility")]
        public static void returnException(
            ref string sException, 
            string vstrModuleName, 
            string vstrRoutineName, 
            Exception oException, 
            string xsltTemplatePath = "/ewcommon/xsl/standard.xsl", 
            string vstrFurtherInfo = "", 
            bool bDebug = false, 
            string cSubjectLinePrefix = "")
        {
            // Resolve current context (Framework-specific)
            System.Web.HttpContext httpContext = null;
            if (System.Web.HttpContext.Current != null)
            {
                // TODO: Wrap System.Web.HttpContext.Current in IHttpContext adapter
                // For now, pass null and let core method handle it
                httpContext = null;
            }

            ReturnExceptionCore(
                ref sException, 
                vstrModuleName, 
                vstrRoutineName, 
                oException, 
                httpContext,
                xsltTemplatePath, 
                vstrFurtherInfo, 
                bDebug, 
                cSubjectLinePrefix);
        }

        /// <summary>
        /// Core implementation of exception handling - uses IHttpContext abstraction
        /// </summary>
        private static void ReturnExceptionCore(
            ref string sException, 
            string vstrModuleName, 
            string vstrRoutineName, 
            Exception oException,
            System.Web.HttpContext httpContext,
            string xsltTemplatePath, 
            string vstrFurtherInfo, 
            bool bDebug, 
            string cSubjectLinePrefix)
        {
            // Author:        Trevor Spink
            // Copyright:     Eonic Ltd 2005
            // Date:          2005-07-08

            string sProcessInfo = "Beginning";
            string strErrorHtml;
            string strMessageHtml;
            var oExceptionXml = new XmlDocument();
            XmlElement oElmt;

            string styleFile;
            var oStyle = new System.Xml.Xsl.XslTransform();
            var sWriter = new System.IO.StringWriter();
            string sReturnHtml = "";
            string cHost = "";
            System.Collections.Specialized.NameValueCollection oConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
            
            // Use abstracted context instead of direct HttpContext.Current
            System.Web.HttpRequest moRequest = httpContext?.Request;
            System.Web.HttpResponse moResponse = httpContext?.Response;
            System.Web.HttpServerUtility moServer = httpContext?.Server;

            sProcessInfo = "Getting Host";

            // Extract host from abstracted request
            if (moRequest != null)
            {
                cHost = moRequest.ServerVariables["HTTP_HOST"];
            }
            
            if (string.IsNullOrEmpty(sException))
            {
                if (string.IsNullOrEmpty(xsltTemplatePath))
                {
                    xsltTemplatePath = oConfig["SiteXsl"];
                }

                string exMessage;
                if (string.IsNullOrEmpty(oException.Message))
                {
                    exMessage = "";
                }
                else
                {
                    exMessage = oException.Message.ToString();
                }

                // want to skip this as is the result of a response redirect so not a real error.
                if (oException.GetType().ToString() != "System.Threading.ThreadAbortException" & !exMessage.Contains("The remote host closed the connection"))
                {
                    // Test expanded from 1 to 2 NOT's to avoid Web Crawler based errors

                    oExceptionXml.LoadXml("<Page layout=\"Error\"><Contents/></Page>");
                    
                    // Determine protocol from abstracted request
                    bool mbIsUsingHTTPS = moRequest?.IsSecureConnection ?? false;
                    string protocol = mbIsUsingHTTPS ? "https://" : "http://";
                    oExceptionXml.DocumentElement.SetAttribute("baseUrl", protocol + cHost);
                    oElmt = oExceptionXml.CreateElement("Content");
                    oElmt.SetAttribute("type", "Formatted Text");
                    oElmt.SetAttribute("name", "column1");

                    strErrorHtml = exceptionReport(oException, vstrModuleName + "." + vstrRoutineName, vstrFurtherInfo, httpContext);
                    strMessageHtml = "<div style=\"font-family:Verdana,Tahoma,Arial\"><h2>Unfortunately this site has experienced an error.</h2>" + "<h3>We take all errors very seriously.</h3>" + "<p>" + "This error has been recorded and details sent to <a href=\"http://eonic.com\">Eonic</a> who provide technical support for this website." + "</p>" + "<p>" + "Eonic welcome any feedback that helps us improve our service and that of our clients, please email any supporting information you might have as to how this error arose to <a href=\"mailto:support@eonic.co.uk\">support@eonic.co.uk</a> or alternatively you are welcome call us on +44 (0)1892 534044 between 9.30am and 5.00pm GMT." + "</p>" + "<p>Please contact the owner of this website for any enquiries specific to the products and services outlined within this site.</p>" + "<a href=\"javascript:history.back();\">Click Here to return to the previous page.</a></div>";
                 
                    try
                    {
                        // bDebug = True

                        if (bDebug && strErrorHtml != null)
                        {
                            sProcessInfo = "In Debug";
                            oElmt.InnerXml = strErrorHtml;
                            oExceptionXml.SelectSingleNode("/Page/Contents").AppendChild(oElmt);
                        }

                        else
                        {
                            // Next If statement to remove 404 errors sending two emails to error (just use the one in web.vb)
                            if (!vstrFurtherInfo.Contains("File Not Found"))
                            {
                                sProcessInfo = "Creating Email";
                                // oElmt.InnerXml = strMessageHtml
                                // send an email to the site admin and V3error@eonic.co.uk
                                string errorEmail = "error@protean.site";
                                if (!string.IsNullOrEmpty(oConfig["errorEmail"]))
                                {
                                    errorEmail = oConfig["errorEmail"];
                                }
                                var adFrom = new MailAddress(errorEmail, "ProteanCMS");
                                var adTo = new MailAddress("error@protean.site", "ErrorLog");
                                sProcessInfo = "Creating Email - New MailMessage Object";
                                var oMail = new System.Net.Mail.MailMessage(adFrom, adTo);
                                oMail.IsBodyHtml = true;
                                // oMail.Subject = moRequest.ServerVariables("HTTP_HOST") & " has generated an Error"
                                oMail.Subject = cSubjectLinePrefix + cHost + " has generated an Error";
                                sProcessInfo = "Creating Email - Adding Body";
                                try
                                {
                                    oMail.Body = strErrorHtml;
                                }
                                catch (Exception ex2)
                                {
                                    oMail.Body = "Error sending error html:" + ex2.Message;
                                }


                                // Send the email
                                var oSmtp = new SmtpClient();
                                try
                                {
                                    sProcessInfo = "Trying Email 1 -" + oConfig["MailServer"];
                                    oSmtp.Host = oConfig["MailServer"];
                                    if (!string.IsNullOrEmpty(oConfig["MailServerPort"]))
                                    {
                                        oSmtp.Port = Convert.ToInt32(oConfig["MailServerPort"]);
                                    }
                                    if (!string.IsNullOrEmpty(oConfig["MailServerUsername"]))
                                    {
                                        oSmtp.Credentials = new System.Net.NetworkCredential(oConfig["MailServerUsername"], oConfig["MailServerPassword"]);
                                    }
                                    if (oConfig["MailServerSSL"].ToLower() == "on")
                                    {
                                        oSmtp.EnableSsl = true;
                                    }
                                    if (oConfig["MailServerSSL"].ToLower() == "off")
                                    {
                                        oSmtp.EnableSsl = false;
                                    }
                                    oSmtp.Send(oMail);
                                }
                                catch (Exception ex)
                                {
                                    try
                                    {
                                        sProcessInfo = "Trying Email 2 -" + oConfig["MailServer"] + ex.Message;
                                        oSmtp.Host.Insert(0, oConfig["MailServer"]);
                                        oSmtp.Send(oMail);
                                    }
                                    catch (Exception exp)
                                    {
                                        AddExceptionToEventLog(exp, sProcessInfo, oException, vstrFurtherInfo, httpContext);
                                    }
                                }
                            }
                            // ############################
                            // Load the content from the Error Page, if it exists.
                            // ############################

                            try
                            {
                                if (!mbDBError)
                                {

                                    sProcessInfo = "Loading Error Page Settings - ConStr";
                                    string dbAuth;

                                    if (!string.IsNullOrEmpty(oConfig["DatabasePassword"]))
                                    {
                                        dbAuth = "user id=" + oConfig["DatabaseUsername"] + "; password=" + oConfig["DatabasePassword"];
                                    }
                                    else
                                    {
                                        dbAuth = oConfig["DatabaseAuth"];
                                    }

                                    string cEwConnStr = "Data Source=" + oConfig["DatabaseServer"] + "; " + "Initial Catalog=" + oConfig["DatabaseName"] + "; " + dbAuth;

                                    sProcessInfo = "Loading Error Page Settings - Conn Open";
                                    string cSQL = "SELECT nStructKey, cStructLayout FROM tblContentStructure WHERE cStructName = 'Protean Error' or cStructName = 'Eonic Error'";
                                    var oCN = new System.Data.SqlClient.SqlConnection(cEwConnStr);
                                    var oCMD = new System.Data.SqlClient.SqlCommand(cSQL, oCN);
                                    oCMD.Connection.Open();

                                    sProcessInfo = "Loading Error Page Settings - Exec Reader";
                                    var oDR = oCMD.ExecuteReader();

                                    int nErrorPageId = 0;
                                    string cLayout = "";

                                    sProcessInfo = "Data Reader";
                                    while (oDR.Read())
                                    {
                                        nErrorPageId = Convert.ToInt32(oDR[0]);
                                        cLayout = Convert.ToString(oDR[1]);
                                        break;
                                    }
                                    oDR.Close();
                                    if (oCMD.Connection.State != ConnectionState.Closed)
                                        oCMD.Connection.Close();
                                    if (nErrorPageId > 0)
                                    {
                                        sProcessInfo = "Loading Error Page";
                                        oExceptionXml.DocumentElement.SetAttribute("layout", cLayout);
                                        cSQL = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey where( CL.nStructId = " + nErrorPageId;
                                        cSQL += ") order by type, cl.nDisplayOrder";
                                        var oDataAdpt = new System.Data.SqlClient.SqlDataAdapter(cSQL, oCN);
                                        oCN.Open();
                                        var oDs = new DataSet();
                                        oDs.DataSetName = "Contents";
                                        oDataAdpt.Fill(oDs, "Content");
                                        oCN.Close();
                                        oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;
                                        if (oDs.Tables[0].Columns.Contains("parID"))
                                        {
                                            oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                                        }
                                        oDs.Tables[0].Columns["ref"].ColumnMapping = MappingType.Attribute;
                                        oDs.Tables[0].Columns["name"].ColumnMapping = MappingType.Attribute;
                                        oDs.Tables[0].Columns["type"].ColumnMapping = MappingType.Attribute;
                                        oDs.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                                        oDs.Tables[0].Columns["publish"].ColumnMapping = MappingType.Attribute;
                                        oDs.Tables[0].Columns["expire"].ColumnMapping = MappingType.Attribute;
                                        oDs.Tables[0].Columns["content"].ColumnMapping = MappingType.SimpleContent;
                                        oDs.EnforceConstraints = false;
                                        // convert to Xml Dom
                                        // oXml = new XmlDataDocument(oDs);
                                        XmlDocument oXml = new XmlDocument();
                                        if (oDs.Tables[0].Rows.Count>0)
                                        {
                                            oXml.LoadXml(oDs.GetXml());
                                        }                                       
                                        oXml.PreserveWhitespace = false;
                                        if (oXml.DocumentElement != null)
                                        {
                                            oExceptionXml.SelectSingleNode("/Page/Contents").InnerXml = oXml.DocumentElement.InnerXml.Replace("&gt;", ">").Replace("&lt;", "<");

                                            // Move the content up a level.
                                            foreach (XmlNode oContent in oExceptionXml.SelectNodes("/Page/Contents/Content")) {
                                                //foreach (XmlNode oAttr in oContent.SelectSingleNode("Content/*")) {
                                                //    XmlElement oContentElmt = (XmlElement)oContent;
                                                //    oContentElmt.SetAttribute(oAttr.Name, oAttr.Value);
                                                //}
                                                // oContent.InnerXml = oContent.SelectSingleNode("Content").InnerXml;

                                                XmlNode innerContent = oContent.SelectSingleNode("Content");
                                                foreach (XmlAttribute attr in innerContent.Attributes)
                                                {
                                                    ((XmlElement)oContent).SetAttribute(attr.Name, attr.Value);
                                                }
                                                oContent.InnerXml = innerContent.InnerXml;
                                               
                                            }

                                        }
                                    }

                                }
                            }
                            catch (Exception ex)
                            {

                                mbDBError = true;
                                // sProcessInfo = "Found Error"
                                AddExceptionToEventLog(ex, sProcessInfo, oException, vstrFurtherInfo, httpContext);
                                oElmt.InnerXml = strMessageHtml;
                                oExceptionXml.SelectSingleNode("/Page/Contents").AppendChild(oElmt);
                            }
                            // ###########################
                        }

                        sProcessInfo = "Loading XSLT";

                        if (xsltTemplatePath.ToLower().StartsWith(@"c:\") | xsltTemplatePath.ToLower().StartsWith(@"d:\"))
                        {
                            styleFile = xsltTemplatePath;
                        }
                        else
                        {
                            // Use abstracted server utility or fallback to static goServer
                            styleFile = moServer?.MapPath(xsltTemplatePath) ?? goServer.MapPath(xsltTemplatePath);
                        }

                        oStyle.Load(styleFile);

                        // add Eonic Bespoke Functions
                        var xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                        Protean.Cms errWeb;
                        
                        // Use abstracted context if available, otherwise fall back to legacy
                        if (httpContext != null)
                        {
                            // TODO: When Cms constructor accepts IHttpContext, use:
                            // errWeb = new Protean.Cms(httpContext);
                            // For now, check if we can get legacy context
                            if (System.Web.HttpContext.Current != null && oException.Message != "Database connection validation failed")
                            {
                                errWeb = new Protean.Cms(System.Web.HttpContext.Current);
                                errWeb.InitializeVariables();
                                var ewXsltExt = new Protean.xmlTools.xsltExtensions(ref errWeb);
                                xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);
                            }
                            else
                            {
                                var ewXsltExt = new Protean.xmlTools.xsltExtensions();
                                xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);
                            }
                        }
                        else
                        {
                            var ewXsltExt = new Protean.xmlTools.xsltExtensions();
                            xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);
                        }


                        sProcessInfo = "Transform";
                        // Would be useful to have some indicator that this is an error, even if loading from another page.
                        oElmt = (XmlElement)oExceptionXml.SelectSingleNode("Page");
                        oElmt.SetAttribute("error", "true");
                        oStyle.Transform(oExceptionXml, xsltArgs, sWriter, null);
                        sProcessInfo = "Setting Return";
                        sReturnHtml = sWriter.ToString();
                      //  errWeb = (Protean.Cms)null;
                    }

                    catch (Exception ex)
                    {
                        AddExceptionToEventLog(ex, sProcessInfo, oException, vstrFurtherInfo, httpContext);
                        if (!gbDebug & !string.IsNullOrEmpty(strMessageHtml))
                        {
                            strErrorHtml = strMessageHtml;
                        }
                        sReturnHtml = "<html><title>Error Message</title><body>" + strErrorHtml + "</body></html>";
                    }
                    finally
                    {
                        sException = sReturnHtml;
                        
                        // Write to response if available
                        if (!string.IsNullOrEmpty(sReturnHtml) && moResponse != null)
                        {
                            try
                            {
                                moResponse.ContentType = "text/html";
                                moResponse.StatusCode = 500;
                                moResponse.Write(sReturnHtml);
                                moResponse.Flush();
                                moResponse.End();
                            }
                            catch
                            {
                                // Response may already be committed
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reports exception via email without rendering HTML response
        /// Modern overload with IHttpContext abstraction
        /// </summary>
        public static void reportException(
            ref string sException, 
            string vstrModuleName, 
            string vstrRoutineName, 
            Exception oException,
            System.Web.HttpContext httpContext,
            string xsltTemplatePath = "/ewcommon/xsl/standard.xsl", 
            string vstrFurtherInfo = "", 
            bool bDebug = false, 
            string cSubjectLinePrefix = "")
        {
            ReportExceptionCore(
                ref sException,
                vstrModuleName,
                vstrRoutineName,
                oException,
                httpContext,
                xsltTemplatePath,
                vstrFurtherInfo,
                bDebug,
                cSubjectLinePrefix);
        }

        /// <summary>
        /// Legacy overload for backward compatibility
        /// </summary>
        [Obsolete("Use overload with IHttpContext parameter for .NET Core compatibility")]
        public static void reportException(
            ref string sException, 
            string vstrModuleName, 
            string vstrRoutineName, 
            Exception oException, 
            string xsltTemplatePath = "/ewcommon/xsl/standard.xsl", 
            string vstrFurtherInfo = "", 
            bool bDebug = false, 
            string cSubjectLinePrefix = "")
        {
            System.Web.HttpContext httpContext = System.Web.HttpContext.Current;
            // TODO: Wrap System.Web.HttpContext.Current when available
            
            ReportExceptionCore(
                ref sException,
                vstrModuleName,
                vstrRoutineName,
                oException,
                httpContext,
                xsltTemplatePath,
                vstrFurtherInfo,
                bDebug,
                cSubjectLinePrefix);
        }

        private static void ReportExceptionCore(
            ref string sException, 
            string vstrModuleName, 
            string vstrRoutineName, 
            Exception oException,
            System.Web.HttpContext httpContext,
            string xsltTemplatePath, 
            string vstrFurtherInfo, 
            bool bDebug, 
            string cSubjectLinePrefix)
        {

            // Author:        Trevor Spink
            // Copyright:     Eonic Ltd 2005
            // Date:          2005-07-08

            string sProcessInfo = "Beginning";
            string strErrorHtml;
            string strMessageHtml= string.Empty;
            var oExceptionXml = new XmlDocument();
            XmlElement oElmt;

            var oStyle = new System.Xml.Xsl.XslTransform();
            var sWriter = new System.IO.StringWriter();
            string sReturnHtml = string.Empty;
            string cHost = "";
            System.Collections.Specialized.NameValueCollection oConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

            // Use abstracted request
            System.Web.HttpRequest moRequest = httpContext?.Request;

            sProcessInfo = "Getting Host";
            if (moRequest != null)
            {
                cHost = moRequest.ServerVariables["HTTP_HOST"];
            }
            if (string.IsNullOrEmpty(sException))
            {
                if (string.IsNullOrEmpty(xsltTemplatePath))
                {
                    xsltTemplatePath = oConfig["SiteXsl"];
                }

                string exMessage;
                if (string.IsNullOrEmpty(oException.Message))
                {
                    exMessage = "";
                }
                else
                {
                    exMessage = oException.Message.ToString();
                }

                // want to skip this as is the result of a response redirect so not a real error.
                if (oException.GetType().ToString() != "System.Threading.ThreadAbortException" & !exMessage.Contains("The remote host closed the connection"))
                {
                    // Test expanded from 1 to 2 NOT's to avoid Web Crawler based errors

                    oExceptionXml.LoadXml("<Page layout=\"Error\"><Contents/></Page>");
                    // oExceptionXml.DocumentElement.SetAttribute("baseUrl", "http://" & moRequest.ServerVariables("HTTP_HOST"))
                    oExceptionXml.DocumentElement.SetAttribute("baseUrl", "http://" + cHost);
                    oElmt = oExceptionXml.CreateElement("Content");
                    oElmt.SetAttribute("type", "Formatted Text");
                    oElmt.SetAttribute("name", "column1");

                    strErrorHtml = exceptionReport(oException, vstrModuleName + "." + vstrRoutineName, vstrFurtherInfo, httpContext);
                    strMessageHtml = "<div style=\"font-family:Verdana,Tahoma,Arial\"><h2>Unfortunately this site has experienced an error.</h2>" + "<h3>We take all errors very seriously.</h3>" + "<p>" + "This error has been recorded and details sent to <a href=\"http://www.eonic.co.uk\">Eonic</a> who provide technical support for this website." + "</p>" + "<p>" + "Eonic welcome any feedback that helps us improve our service and that of our clients, please email any supporting information you might have as to how this error arose to <a href=\"mailto:support@eonic.co.uk\">support@eonic.co.uk</a> or alternatively you are welcome call us on +44 (0)1892 534044 between 9.30am and 5.00pm GMT." + "</p>" + "<p>Please contact the owner of this website for any enquiries specific to the products and services outlined within this site.</p>" + "<a href=\"javascript:history.back();\">Click Here to return to the previous page.</a></div>";

                    try
                    {

                        if (!vstrFurtherInfo.Contains("File Not Found"))
                        {
                            sProcessInfo = "Creating Email";
                            // send an email to the site admin and error@eonic.co.uk
                            string errorEmail = "error@protean.site";
                            if (!string.IsNullOrEmpty(oConfig["errorEmail"]))
                            {
                                errorEmail = oConfig["errorEmail"];
                            }
                            var adFrom = new MailAddress(errorEmail, "ProteanCMS");
                            var adTo = new MailAddress("error@protean.site", "ErrorLog");
                            sProcessInfo = "Creating Email - New MailMessage Object";
                            var oMail = new System.Net.Mail.MailMessage(adFrom, adTo);
                            oMail.IsBodyHtml = true;
                            oMail.Subject = cSubjectLinePrefix + cHost + " has generated an Error";
                            sProcessInfo = "Creating Email - Adding Body";
                            try
                            {
                                oMail.Body = strErrorHtml;
                            }
                            catch (Exception ex2)
                            {
                                oMail.Body = "Error sending error html:" + ex2.Message;
                            }

                            // Send the email
                            var oSmtp = new SmtpClient();
                            try
                            {
                                sProcessInfo = "Trying Email 1 -" + oConfig["MailServer"];
                                oSmtp.Host = oConfig["MailServer"];
                                if (!string.IsNullOrEmpty(oConfig["MailServerPort"]))
                                {
                                    oSmtp.Port = Convert.ToInt32(oConfig["MailServerPort"]);
                                }
                                if (!string.IsNullOrEmpty(oConfig["MailServerUsername"]))
                                {
                                    oSmtp.Credentials = new System.Net.NetworkCredential(oConfig["MailServerUsername"], oConfig["MailServerPassword"]);
                                }
                                if (oConfig["MailServerSSL"].ToLower() == "on")
                                {
                                    oSmtp.EnableSsl = true;
                                }
                                if (oConfig["MailServerSSL"].ToLower() == "off")
                                {
                                    oSmtp.EnableSsl = false;
                                }
                                oSmtp.Send(oMail);
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    sProcessInfo = "Trying Email 2 -" + oConfig["MailServer"];
                                    oSmtp.Host.Insert(0, oConfig["MailServer"]);
                                    oSmtp.Send(oMail);
                                }
                                catch (Exception exp)
                                {
                                    AddExceptionToEventLog(exp, sProcessInfo, oException, vstrFurtherInfo, httpContext);
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        AddExceptionToEventLog(ex, sProcessInfo, oException, vstrFurtherInfo, httpContext);
                    }
                }
            }
        }


        private static void TransformErrorHandle(string cModuleName, string cRoutineName, Exception oException, string cFurtherInfo)
        {
            AddExceptionToEventLog(oException, cFurtherInfo);
        }

        /// <summary>
        /// Adds exception to Windows Event Log with IHttpContext support
        /// </summary>
        public static void AddExceptionToEventLog(Exception oCurrentException, string cCurrentInfo, Exception oOriginalError = null, string cOriginalInfo = "", System.Web.HttpContext httpContext = null)
        {
            // writes an event to the even log under the heading "EonicWebV4.1"
            string thisError;
            string LogName = "ProteanCMS";
            string cSource = "ProteanCMS Site";
            string cMessage = "Site: unknown\r\n\r\n";
            try
            {
                EventLog oEventLog = null;
                EventLog[] oELs = EventLog.GetEventLogs();
                int i = 0;
                var loopTo = oELs.Length - 1;
                for (i = 0; i <= loopTo; i++)
                {
                    if ((oELs[i].Log ?? "") == (LogName ?? ""))
                    {
                        oEventLog = oELs[i];
                        break;
                    }
                }

                // Try abstracted context first, then fall back to legacy
                if (httpContext?.Request != null)
                {
                    cSource = httpContext.Request.ServerVariables["HTTP_HOST"];
                    cMessage = "Site: " + httpContext.Request.ServerVariables["HTTP_HOST"] + "\r\n";
                }
                else if (System.Web.HttpContext.Current != null)
                {
                    cSource = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
                    cMessage = "Site: " + System.Web.HttpContext.Current.Request.ServerVariables["HTTP_HOST"] + "\r\n";
                }

                if (oEventLog is null)
                {
                    oEventLog = new EventLog(LogName, Environment.MachineName, cSource);
                }

                // The Current Error
                if (oCurrentException != null)
                {
                    cMessage += "\r\n Current Error:\r\n ";
                    cMessage += $"Info:{cCurrentInfo}\r\n ";
                    cMessage += $"Exception Type:{oCurrentException.GetType().ToString() }\r\n";
                    cMessage += $"Message:{oCurrentException.Message}\r\n";
                    cMessage += $"Source:{oCurrentException.Source}\r\n";
                    cMessage += $"Stack:{oCurrentException.StackTrace}\r\n";
                    cMessage += $"Full Exception:{oCurrentException.ToString()}\r\n" ;
                }
                // We might be coming from an error handling procedure so lets get the orignal error that sent us there too
                if (oOriginalError != null)
                {
                    cMessage += "\r\nOriginal Error: \r\n" ;
                    cMessage += $"Info:{cOriginalInfo}\r\n" ;
                    cMessage += $"Exception Type:{oOriginalError.GetType().ToString()}\r\n";
                    cMessage += $"Message:{oOriginalError.Message}\r\n";
                    cMessage += $"Source:{oOriginalError.Source}\r\n";
                    cMessage += $"Stack:{oOriginalError.StackTrace}\r\n";
                    cMessage += $"Full Exception:{oOriginalError.ToString()}\r\n";
                }

                if (!EventLog.SourceExists(cSource))
                {
                    EventLog.CreateEventSource(LogName, cSource);
                }
                oEventLog.Source = cSource;
                oEventLog.WriteEntry(cMessage, EventLogEntryType.Error);
                oEventLog = null;
            }

            catch (Exception ex)
            {
                // cant do diddly but cry 
                try
                {
                    System.IO.File.WriteAllText(@"D:\HostingSpaces\ProteanError.txt", cMessage);
                }
                catch (Exception ex2)
                {
                    thisError = ex2.Message;
                }
                thisError = ex.Message;
            }
        }


        /// <summary>
        /// Generates detailed exception report HTML with IHttpContext support
        /// </summary>
        public static string exceptionReport(Exception oException, string sComponent, string sInfo, System.Web.HttpContext httpContext = null)
        {
            string cReport;
            //string cSV;
            string cAssembly = "";
            
            // Use abstracted context
            System.Web.HttpRequest moRequest = httpContext?.Request;
            System.Web.SessionState.HttpSessionState moSession = httpContext?.Session;
            System.Web.HttpServerUtility moServer = httpContext?.Server;

            cReport = "<div style=\"font: normal .75em/1.5 Verdana, Tahoma, sans-serif;\"><h2>ProteanCMS has returned the following Error</h2>" + "<table cellpadding=\"1\" cellspacing=\"0\" border=\"0\">";

            // Report Information
            addExceptionHeader(ref cReport, "Report Info");
            addExceptionLine(ref cReport, "Date + Time", DateTime.Now.ToString("dd MMMM yyyy"));
            addExceptionLine(ref cReport, "Webserver:", Environment.MachineName);
            
            // Try to get site name from hosting environment or abstracted server
            try
            {
                string siteName = System.Web.Hosting.HostingEnvironment.ApplicationHost?.GetSiteName() ?? "Unknown";
                addExceptionLine(ref cReport, "SiteName:", siteName);
            }
            catch
            {
                addExceptionLine(ref cReport, "SiteName:", "N/A");
            }

            var a = Assembly.GetExecutingAssembly();
            addExceptionLine(ref cReport, "Assembly", a.FullName);

            // Referenced Assemblies
            addExceptionLine(ref cReport, "Codebase", a.CodeBase);
            foreach (var an in a.GetReferencedAssemblies())
                cAssembly = cAssembly + an.Name + " (" + an.Version.ToString() + "); ";
            addExceptionLine(ref cReport, "Referenced Assemblies", cAssembly);

            // Exception Details
            addExceptionHeader(ref cReport, "Exception Details");
            addExceptionLine(ref cReport, "Component:", sComponent);
            addExceptionLine(ref cReport, "Info:", sInfo);
            addExceptionLine(ref cReport, "Exception Type:", oException.GetType().ToString());
            addExceptionLine(ref cReport, "Message:", oException.Message);
            addExceptionLine(ref cReport, "Source:", oException.Source);
            addExceptionLine(ref cReport, "Stack:", oException.StackTrace);
            addExceptionLine(ref cReport, "Full Exception:", oException.ToString());

            // Session Variables
            if (moSession != null)
            {
                try
                {
                    if (moSession.Count > 0)
                    {
                        addExceptionHeader(ref cReport, "Session Variables");
                        foreach (string cSV2 in moSession.Keys)
                        {
                            try
                            {
                                addExceptionLine(ref cReport, cSV2, Convert.ToString(moSession[cSV2]));
                            }
                            catch
                            {
                                addExceptionLine(ref cReport, cSV2, "object cannot be converted to string");
                            }
                        }
                    }
                    else
                    {
                        addExceptionHeader(ref cReport, "No Session variables found");
                    }
                }
                catch
                {
                    addExceptionHeader(ref cReport, "Session unavailable");
                }
            }

            // Querystring Variables
            if (moRequest != null)
            {
                addExceptionHeader(ref cReport, "Server Variables");
                addExceptionLine(ref cReport, "AppPath", moRequest.ApplicationPath ?? "");
                addExceptionLine(ref cReport, "Path", moRequest.Path ?? "");
                // ++++++++++++ Only re-enable this temporarily if needed for debugging +++++ Can cause credit card numbers to show in error messages.... bad karma man!

                // If moRequest.QueryString.Count > 0 Then
                // addExceptionHeader(cReport, "Querystring Variables")
                // For Each cSV In moRequest.QueryString
                // addExceptionLine(cReport, cSV, moRequest.QueryString(cSV))
                // Next
                // Else
                // addExceptionHeader(cReport, "No Querystring variables found")
                // End If

                // ' Form Variables
                // If moRequest.Form.Count > 0 Then
                // addExceptionHeader(cReport, "Form Variables")
                // For Each cSV In moRequest.Form
                // If TypeName(moRequest.Form(cSV)) = "String" Then addExceptionLine(cReport, cSV, moRequest.Form(cSV))
                // Next
                // Else
                // addExceptionHeader(cReport, "No Form variables found")
                // End If

                // ' Server Variables
                addExceptionHeader(ref cReport, "Server Variables");
                foreach (string cSV3 in moRequest.ServerVariables)
                {
                    // If Left(cSV, 5) = "HTTP_" Then addExceptionLine(cReport, cSV, moRequest.ServerVariables(cSV))
                    addExceptionLine(ref cReport, cSV3, moRequest.ServerVariables[cSV3]);
                }
            }

            cReport = cReport + "</table></div>";
            return Xml.convertEntitiesToCodes(cReport);

        }

        public static void addExceptionHeader(ref string cReport, string cHeader)
        {

            cReport = cReport + "<tr><th colspan=\"2\" valign=\"top\" align=\"left\" style=\"padding-top:2em\"><h3>" + cHeader + "</h3></th></tr>";

        }
        public static void addExceptionLine(ref string cReport, string cHeader, string cValue)
        {

            cValue = cValue.Replace("<", "&lt;");
            cValue = cValue.Replace(">", "&gt;");
            cValue = cValue.Replace(Convert.ToString('\n'), "<br/>");
            cReport = cReport + "<tr><th valign=\"top\" align=\"left\">" + cHeader + "</th><td valign=\"top\">" + cValue + "</td></tr>";

        }


        public static object SqlFmt(string sText)
        {
            if (sText != null)
            {
                object SqlFmtRet = default;
                // 'PerfMon.Log("stdTools", "SqlFmt")
                SqlFmtRet = sText.Replace("'", "''");
                return SqlFmtRet;
            }
            else {
                return "";
            }
        }


        public static object xPathEscapeQuote(string sText)
        {
            if (sText.Contains("'"))
            {

                return "concat('" + sText.Replace("'", "', \"'\", '") + "')";
            }
            else
            {
                return "'" + sText + "'";
            }

        }

        public static string niceDate(object dDate)
        {
            string niceDateRet = default;
            // 'PerfMon
            string sdate;

            if (Tools.Text.IsDate(dDate))
            {
                sdate = Convert.ToString(Convert.ToDateTime(dDate));
                DateTime parsedDate = Convert.ToDateTime(sdate);
                var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
                niceDateRet = culture.Calendar.GetDayOfMonth(parsedDate) + " " +
                              culture.DateTimeFormat.GetAbbreviatedMonthName(culture.Calendar.GetMonth(parsedDate)) + " " +
                              culture.Calendar.GetYear(parsedDate);

            }
            else if (dDate.ToString() != "00:00:00")
            {
                niceDateRet = "";
            }
            else
            {
                niceDateRet = "";
            }

            return niceDateRet;

        }

        #region SQL DATE TIME FUNCTIONS
        // Public Function sqlDate(ByVal dDate As Object) As String
        // ''PerfMon.Log("stdTools", "sqlDate")

        // If IsDate(dDate) Then
        // sqlDate = "'" & Format(dDate, "dd MMMM yyyy") & "'"
        // Else
        // If dDate <> "00:00:00" Then
        // sqlDate = "null"
        // Else
        // sqlDate = "null"
        // End If
        // End If
        // If dDate = #12:00:00 AM# Then sqlDate = "null"
        // If dDate = #12:00:00 PM# Then sqlDate = "null"
        // End Function

        // Public Function sqlDateTime(ByVal dDateTime As Object) As String
        // If IsDate(dDateTime) Then
        // sqlDateTime = "'" & Format(dDateTime, "dd-MMM-yy HH:mm:ss") & "'"
        // Else
        // sqlDateTime = "null"
        // End If
        // End Function

        // Public Function sqlDateTime(ByVal dDate As Object, ByVal stime As Object) As String
        // 'PerfMon.Log("stdTools", "sqlDateTime")

        // If IsDate(dDate) Then
        // sqlDateTime = "'" & Format(dDate, "dd MMMM yyyy") & " " & stime & "'"
        // Else
        // If dDate <> "00:00:00" Then
        // sqlDateTime = "null"
        // Else
        // sqlDateTime = "null"
        // End If
        // End If

        // End Function

        // Left here for external applications to use
        public static string sqlDate(object dDate)
        {
            return Database.SqlDate(dDate, false);
        }

        public static string sqlDateTime(object dDateTime)
        {
            return Database.SqlDate(dDateTime, true);
        }

        public static string sqlDateTime(object dDate, object stime)
        {
            return Database.SqlDate((Convert.ToDateTime(dDate).ToString("dd MMMM yyyy") + " ", stime), true);
        }



        #endregion
        #region XML DATE TIME FUNCTIONS

        // Public Function xmlDate(ByVal dDate As Object) As String
        // 'PerfMon.Log("stdTools", "xmlDate")
        // Dim sDay As String
        // Dim sMonth As String

        // If IsDBNull(dDate) Then
        // xmlDate = ""
        // Else
        // sDay = CStr(VB.Day(dDate))
        // If Len(sDay) = 1 Then sDay = "0" & sDay

        // sMonth = CStr(Month(dDate))
        // If Len(sMonth) = 1 Then sMonth = "0" & sMonth

        // xmlDate = Year(dDate) & "-" & sMonth & "-" & sDay

        // End If

        // End Function

        // Public Function xmlDateTime(ByVal dDate As Object) As String
        // 'PerfMon.Log("stdTools", "xmlDateTime")
        // Dim sDay As String
        // Dim sMonth As String

        // If IsDBNull(dDate) Then
        // xmlDateTime = ""
        // Else
        // sDay = CStr(VB.Day(dDate))
        // If Len(sDay) = 1 Then sDay = "0" & sDay

        // sMonth = CStr(Month(dDate))
        // If Len(sMonth) = 1 Then sMonth = "0" & sMonth

        // xmlDateTime = Trim(Year(dDate) & "-" & sMonth & "-" & sDay & "T" & TimeValue(dDate))

        // End If

        // End Function

        //public static string xmlDate(object dDate)
        //{
        //    return Xml.XmlDate(dDate);
        //}

        //public static string xmlDateTime(object dDate)
        //{
        //    return Xml.XmlDate(dDate, true);
        //}
        #endregion




        public static bool ButtonSubmitted(ref System.Web.HttpRequest mroRequest, string cButtonName)
        {
            bool ButtonSubmittedRet = default;
            // 'PerfMon.Log("stdTools", "ButtonSubmitted")
            bool bSubmitted = false;

            string cNameX = cButtonName + ".x";
            string cNameY = cButtonName + ".y";

            if (mroRequest[cButtonName] != null | mroRequest[cNameX] != null | mroRequest[cNameY] != null)
                bSubmitted = true;
            ButtonSubmittedRet = bSubmitted;
            return ButtonSubmittedRet;

        }


        public static Hashtable UrlResponseToHashTable(string sResponse, string sFieldSeperator = "&", string sValueSeperator = "=", bool bURLDecodeValue = true)
        {
            // PerfMon.Log("stdTools", "UrlResponseToHashTable")
            try
            {

                string[] aResponse = sResponse.Split(Convert.ToChar(sFieldSeperator));
                string cKey;
                string cValue;
                int nPos;
                var oResponseDict = new Hashtable();
                int i;
                var loopTo = aResponse.Length;
                for (i = 0; i <= loopTo; i++)
                {

                    // It's not a guarantee that the value separator won't also appear in the value!
                    // e.g. ACSURL=http://someurl/?key=foo
                    // PaReq=isdfjeariogj/vw==
                    // 
                    // So rather than use Split, we should look for the first instance of the value separator.
                    nPos = aResponse[i].IndexOf(sValueSeperator);

                    // Ignore blank keys (0) and separators that are not found (-1)
                    if (nPos > 0)
                    {
                        cKey = aResponse[i].Substring(0, nPos);
                        if (nPos == aResponse[i].Length - 1)
                        {
                            cValue = "";
                        }
                        else
                        {
                            cValue = aResponse[i].Substring(nPos + 1);
                        }
                        if (bURLDecodeValue)
                            cValue = goServer.UrlDecode(cValue);
                        oResponseDict.Add(cKey, cValue);
                    }

                }

                return oResponseDict;
            }
            catch (Exception)
            {
                // returnException("stdTools", "UrlResponseToHashTable", ex, "", "", gbDebug)
                return null;
            }
        }


        public static string GenerateMD5Hash(string SourceText)
        {
            // PerfMon.Log("stdTools", "GenerateMD5Hash")
            // This generates a PHP compatible MD5 Hex string for the source value.

            var md5 = MD5.Create();
            byte[] dataMd5 = md5.ComputeHash(Encoding.Default.GetBytes(SourceText));
            var sb = new StringBuilder();
            int i = 0;
            while (i < dataMd5.Length)
            {
                sb.AppendFormat("{0:x2}", dataMd5[i]);
                Math.Min(System.Threading.Interlocked.Increment(ref i), i - 1);
            }
            return sb.ToString();
        }

        public static string EncryptString(string SourceText, bool bUseAsymmetric = true, string cSalt = "")
        {

            Encryption.EncData encryptedData;
            try
            {


                if (bUseAsymmetric)
                {

                    var asym = new Encryption.Asymmetric();
                    var pubkey = new Encryption.Asymmetric.PublicKey();
                    var privkey = new Encryption.Asymmetric.PrivateKey();

                    if (string.IsNullOrEmpty(WebConfigurationManager.AppSettings["PublicKey.Modulus"]))
                    {
                        // at the moment this writes a new file that needs to be included in web.config
                        asym.GenerateNewKeySet(ref pubkey, ref privkey);
                        pubkey.ExportToConfigFile(goServer.MapPath("encrypt.config"));
                        privkey.ExportToConfigFile(goServer.MapPath("encrypt.config"));
                    }
                    else
                    {
                        pubkey.LoadFromConfig();
                        privkey.LoadFromConfig();
                    }

                    // End Try


                    encryptedData = asym.Encrypt(new Encryption.EncData(SourceText), pubkey);
                }

                else
                {

                    // This may seem unnecessary, but symmetric encryption is relatively weak (often because of the choice of key),
                    // so why not encrypt the symmetric key asymettrically to end up with a bastard-hard sym-key to guess.
                    // UPDATE For some reason the assym encryption wouldn't consistently betwenn ENC and DEC so I've commented it out
                    // for now - rely on salt.
                    // UPDATE Get the right key size for Rijndael - try using the PrivateKey.D
                    var sym = new Encryption.Symmetric(Encryption.Symmetric.Provider.Rijndael);
                    sym.InitializationVector = new Encryption.EncData(WebConfigurationManager.AppSettings["SymmetricIV"]);
                    sym.Key = new Encryption.EncData(WebConfigurationManager.AppSettings["PrivateKey.D"]);
                    encryptedData = sym.Encrypt(new Encryption.EncData(SourceText));

                }

                return encryptedData.ToHex();
            }

            catch (Exception)
            {
                return "";
            }


        }

        public static string DecryptString(string encryptedText, bool bUseAsymmetric = true, string cSalt = "")
        {
            try
            {

                var decryptedData = new Encryption.EncData();

                var encryptedData = new Encryption.EncData();
                encryptedData.Hex = encryptedText;

                if (bUseAsymmetric)
                {
                    var asym = new Encryption.Asymmetric();
                    var pubkey = new Encryption.Asymmetric.PublicKey();
                    var privkey = new Encryption.Asymmetric.PrivateKey();
                    pubkey.LoadFromConfig();
                    privkey.LoadFromConfig();

                    var asym2 = new Encryption.Asymmetric();
                    decryptedData = asym2.Decrypt(encryptedData, privkey);
                }

                else
                {
                    // UPDATE For some reason the assym encryption wouldn't consistently betwenn ENC and DEC so I've commented it out
                    // for now - rely on salt.
                    var sym = new Encryption.Symmetric(Encryption.Symmetric.Provider.Rijndael);
                    sym.InitializationVector = new Encryption.EncData(WebConfigurationManager.AppSettings["SymmetricIV"]);
                    sym.Key = new Encryption.EncData(WebConfigurationManager.AppSettings["PrivateKey.D"]);
                    decryptedData = sym.Decrypt(encryptedData);
                }

                return decryptedData.ToString();
            }

            catch (Exception)
            {
                return "";
            }

        }

        public static string EncryptStringOLD(string SourceText)
        {
            // PerfMon.Log("stdTools", "EncryptString")
            var asym = new Encryption.Asymmetric();
            var pubkey = new Encryption.Asymmetric.PublicKey();
            var privkey = new Encryption.Asymmetric.PrivateKey();

            if (string.IsNullOrEmpty(WebConfigurationManager.AppSettings["PublicKey.Modulus"]))
            {
                // at the moment this writes a new file that needs to be included in web.config
                asym.GenerateNewKeySet(ref pubkey, ref privkey);
                pubkey.ExportToConfigFile(goServer.MapPath("encrypt.config"));
                privkey.ExportToConfigFile(goServer.MapPath("encrypt.config"));
            }
            else
            {
                pubkey.LoadFromConfig();
                privkey.LoadFromConfig();
            }

            // End Try

            Encryption.EncData encryptedData;
            encryptedData = asym.Encrypt(new Encryption.EncData(SourceText), pubkey);

            return encryptedData.ToHex();

        }

        public static string DecryptStringOLD(string encryptedText)
        {
            // PerfMon.Log("stdTools", "DecryptString")
            try
            {
                var asym = new Encryption.Asymmetric();
                var pubkey = new Encryption.Asymmetric.PublicKey();
                var privkey = new Encryption.Asymmetric.PrivateKey();
                pubkey.LoadFromConfig();
                privkey.LoadFromConfig();

                var encryptedData = new Encryption.EncData();
                encryptedData.Hex = encryptedText;

                var decryptedData = new Encryption.EncData();
                var asym2 = new Encryption.Asymmetric();
                decryptedData = asym2.Decrypt(encryptedData, privkey);

                return decryptedData.ToString();
            }
            catch
            {
                return "";
            }

        }

        public enum DiscountCategory
        {

            // describes the type of discount and how its worked out
            Basic = 1, // Percent or Value Off = Product Specific
            BreakProduct = 2, // Quantity or Value Breaks = Product Specific
            X4PriceY = 3, // Buy X for the price of Y (Buy X Get Y Free) = Product Specific
            CheapestFree = 4, // Cheapest Item Free = Group specific
            BreakGroup = 5 // Cheapest Item Free = Group specific
        }

        public static decimal Round(object nNumber, int nDecimalPlaces = 2, int nSplitNo = 5, bool bForceRoundup = true, bool bForceRoundDown = false)
        {
            // 'PerfMon.Log("stdTools", "RoundUp")
            try
            {
                decimal RetVal;
                if (bForceRoundup)
                {
                    RetVal = RoundUp(nNumber, nDecimalPlaces, nSplitNo);
                }
                else if (bForceRoundDown)
                {
                    double adjustment = Math.Pow(10d, nDecimalPlaces);
                    // RetVal = Math.Floor(nNumber, adjustment)/adjustment;
                    RetVal = Math.Round(Convert.ToDecimal(nNumber), nDecimalPlaces, MidpointRounding.ToEven);
                }
                // RetVal = Math.Round(nNumber, nDecimalPlaces, MidpointRounding.ToEven)
                else
                {
                    RetVal = Math.Round(Convert.ToDecimal(nNumber), nDecimalPlaces);
                }
                return RetVal;
            }
            catch (Exception)
            {
                return 0m;
            }
        }

        public static decimal RoundUp(object nNumber, int nDecimalPlaces = 2, int nSplitNo = 5)
        {
            // PerfMon.Log("stdTools", "RoundUp")
            try
            {
                // get the dross over with
                if (!Tools.Number.IsNumeric(nNumber))
                    return 0m;
                // no decimal places to deal with
                var numberString = Convert.ToString(nNumber);
                if (!numberString.Contains("."))
                    return Convert.ToDecimal(nNumber);
                // has correct number of decimal places
                var parts = numberString.Split('.');
                var fractional = parts.Length > 1 ? parts[1] : string.Empty;
                if (fractional.Length <= nDecimalPlaces)
                    return Convert.ToDecimal(nNumber);

                // now the fun
                int nWholeNo = Convert.ToInt32(parts[0]); // the whole number before decimal point
                int nTotalLength = fractional.Length; // the total number of decimal places

                int nI; // a counter

                int nCarry = 0; // number to carry to next number

                // loop through until we reach the correct number of decimal places
                var loopTo = nTotalLength - nDecimalPlaces;
                for (nI = 0; nI <= loopTo; nI++)
                {
                    // compute index of the digit equivalent to:
                    // Strings.Right(Strings.Left(fractional, nTotalLength - nI), 1)
                    int takeLen = nTotalLength - nI;
                    int idx = takeLen - 1; // zero-based index of the digit we need
                    char digitChar = (idx >= 0 && idx < fractional.Length) ? fractional[idx] : '0';
                    int nCurrent = (int)char.GetNumericValue(digitChar);
                    if (nCurrent < 0) nCurrent = 0; // be defensive
                    nCurrent += nCarry; // add the carry
                    if (nCurrent >= nSplitNo)
                        nCarry = 1;
                    else
                        nCarry = 0; // make a new carry dependant on where we are
                }

                int nDecimal = Convert.ToInt32(fractional.Substring(0, nDecimalPlaces)); // the decimal value
                nDecimal += nCarry; // add last carry
                if (nDecimal.ToString().Length > nDecimalPlaces) // if we have now gone over the number of decimal places then need to sort it
                {
                    nCarry = 1;
                    var nDecimalStr = nDecimal.ToString();
                    nDecimal = Convert.ToInt32(nDecimalStr.Substring(nDecimalStr.Length - nDecimalPlaces));
                }
                else
                {
                    nCarry = 0;
                }
                nWholeNo += nCarry;
                return Convert.ToDecimal(nWholeNo + "." + nDecimal);
            }
            catch (Exception)
            {
                return 0m;
                // returnException("stdTools", "Round Up", ex, "", "", gbDebug)
            }
        }



        public static string MaskString(string cInitialString, string cMaskchar = "*", bool bKeepSpaces = false, int nNoCharsToLeave = 4)
        {
            string cNewString = "";
            try
            {
                if (!bKeepSpaces)
                    cInitialString = cInitialString.Replace(" ", "");

                int i;
                var loopTo = cInitialString.Length - (nNoCharsToLeave + 1);
                for (i = 0; i <= loopTo; i++)
                {
                    if (!(cInitialString.Substring(i, 1) == " "))
                    {
                        cNewString += cMaskchar;
                    }
                    else
                    {
                        cNewString += " ";
                    }
                }

                // Append the rightmost nNoCharsToLeave characters (safe for short strings)
                if (nNoCharsToLeave <= 0)
                {
                    // nothing to append
                }
                else if (nNoCharsToLeave >= cInitialString.Length)
                {
                    cNewString += cInitialString;
                }
                else
                {
                    cNewString += cInitialString.Substring(cInitialString.Length - nNoCharsToLeave);
                }

                return cNewString;
            }
            catch (Exception)
            {
                // returnException("stdTools", "MaskString", ex, "", "", gbDebug)
                return cNewString;
            }
        }



        public static long objectToNumeric(object oRequestItem, long bDefaultValue = 0L, bool bAllowNegatives = false)
        {

            long nRequest;

            if (Tools.Number.IsNumeric(oRequestItem))
            {
                if (Convert.ToBoolean(bAllowNegatives))
                {
                    nRequest = Convert.ToInt32(oRequestItem);
                }
                else if (Convert.ToInt32(oRequestItem) > 0)
                {
                    nRequest = Convert.ToInt32(oRequestItem);
                }
                else
                {
                    nRequest = bDefaultValue;
                }
            }
            else
            {
                nRequest = bDefaultValue;
            }

            return nRequest;

        }

        public static void HTTPRedirect(ref System.Web.HttpContext oCtx, string cURL, [Optional, DefaultParameterValue(302)] ref int nStatusCode)
        {

            // Response.Redirect always results in 302 (unless you use the .NET 3.0 Web.Extensions)
            // so you have to go through this rigmorale.

            try
            {

                // Look for valid codes
                switch (nStatusCode)
                {
                    // Do nothing
                    case 301:
                    case 302:
                    case 303:
                    case 304:
                    case 307:
                        {
                            break;
                        }

                    default:
                        {
                            nStatusCode = 302;
                            break;
                        }
                }

                oCtx.Response.StatusCode = nStatusCode;

                // Set the description
                switch (nStatusCode)
                {

                    case 301:
                        {
                            oCtx.Response.StatusDescription = "Moved Permanently";
                            break;
                        }

                    case 302:
                        {
                            oCtx.Response.StatusDescription = "Found";
                            break;
                        }

                    case 303:
                        {
                            oCtx.Response.StatusDescription = "See Other";
                            break;
                        }

                    case 304:
                        {
                            oCtx.Response.StatusDescription = "Not Modified";
                            break;
                        }

                    case 307:
                        {
                            oCtx.Response.StatusDescription = "Temporary Redirect";
                            break;
                        }

                }

                oCtx.Response.RedirectLocation = cURL;
                oCtx.ApplicationInstance.CompleteRequest();
                oCtx.Response.End();
            }

            catch (Exception)
            {

            }

        }


        public static bool strongPassword(string password)
        {
            string pwdRegEx = "";

            XmlElement moPolicy;

            moPolicy = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy");

            pwdRegEx = "(?=^.{" + moPolicy.FirstChild.SelectSingleNode("minLength").InnerText + "," + moPolicy.FirstChild.SelectSingleNode("maxLength").InnerText + "}$)" + @"(?=(?:.*?\d){" + moPolicy.FirstChild.SelectSingleNode("numsLength").InnerText + "})" + "(?=.*[a-z])" + "(?=(?:.*?[A-Z]){" + moPolicy.FirstChild.SelectSingleNode("upperLength").InnerText + "})" + "(?=(?:.*?[" + moPolicy.FirstChild.SelectSingleNode("specialChars").InnerText + "]){" + moPolicy.FirstChild.SelectSingleNode("specialLength").InnerText + "})" + @"(?!.*\s)[0-9a-zA-Z" + moPolicy.FirstChild.SelectSingleNode("specialChars").InnerText + "]*$";

            // Validate the e-mail address
            return new Regex(pwdRegEx, RegexOptions.IgnoreCase).IsMatch(password + "");


        }

        // Sub SetDefaultSortColumn(ByRef moPageXml As XmlDocument, ByVal nSortColumn As Long, Optional ByVal nSortDirection As SortDirection = SortDirection.Ascending)
        // 'PerfMon.Log("stdTools", "SetDefaultSortColumn")
        // Try
        // Dim oElmt As XmlElement
        // If moPageXml.SelectSingleNode("/Page/Request/QueryString/Item[@name='sortCol']") Is Nothing Then
        // ' Add a default column sort
        // oElmt = addNewTextNode("Item", moPageXml.SelectSingleNode("/Page/Request/QueryString"), nSortColumn, , False)
        // oElmt.SetAttribute("name", "sortCol")
        // oElmt = addNewTextNode("Item", moPageXml.SelectSingleNode("/Page/Request/QueryString"), SortDirectionVal(nSortDirection), , False)
        // oElmt.SetAttribute("name", "sortDir")
        // End If
        // Catch ex As Exception
        // returnException("stdTools", "SetDefaultSortColumn", ex, "", "", gbDebug)
        // End Try
        // End Sub

        public class StringWriterWithEncoding : System.IO.StringWriter
        {

            private Encoding _encoding;

            public StringWriterWithEncoding(Encoding encoding) : base()
            {
                _encoding = encoding;
            }

            public StringWriterWithEncoding(Encoding encoding, IFormatProvider formatProvider) : base(formatProvider)
            {
                _encoding = encoding;
            }

            public StringWriterWithEncoding(Encoding encoding, StringBuilder sb) : base(sb)
            {
                _encoding = encoding;
            }

            public StringWriterWithEncoding(Encoding encoding, StringBuilder sb, IFormatProvider formatProvider) : base(sb, formatProvider)
            {
                _encoding = encoding;
            }

            public override Encoding Encoding
            {
                get
                {
                    return _encoding;
                }
            }

        }

        public static string tidyXhtmlFrag(string shtml, bool bReturnNumbericEntities = false, bool bEncloseText = true, string removeTags = "")
        {

            // PerfMon.Log("Web", "tidyXhtmlFrag")
            // string sProcessInfo = "tidyXhtmlFrag";
            string sTidyXhtml = "";
            int crResult = 0;
            bool bRetryWithEscapedTags = false;

            if (!(removeTags == ""))
                shtml = removeTagFromXml(shtml, removeTags);

            string originalHtml = shtml; // Keep original for retry
            TidyManaged.Document oTdyManaged;

            try
            {
                // clear some nasties I haven't allready captured.
                shtml = shtml.Replace("&amp;nbsp;", "&#160;");
                shtml = Regex.Replace(shtml, "<\\?xml.*\\?>", "", RegexOptions.IgnoreCase);

                //temp fix for dirty VMH data
                shtml = shtml.Replace( ":=", "=");

            RetryWithEscaping:

                oTdyManaged = TidyManaged.Document.FromString(shtml);
                oTdyManaged.OutputBodyOnly = TidyManaged.AutoBool.Yes;
                oTdyManaged.MakeClean = true;
                oTdyManaged.DropFontTags = true;
                //oTdyManaged.ErrorBuffer = true;
                oTdyManaged.ShowWarnings = true;
                oTdyManaged.OutputXhtml = true;
                oTdyManaged.MakeBare = true;//removed word tags
                oTdyManaged.CleanWord2000 = true;//removed word tags

               // oTdyManaged.InputCharacterEncoding = TidyManaged.EncodingType.Latin1;

                oTdyManaged.CharacterEncoding = TidyManaged.EncodingType.Utf8;



                if (bReturnNumbericEntities)
                {
                    oTdyManaged.OutputNumericEntities = true;
                }

                // CleanAndRepair returns status: <0 = error, 0 = no warnings/errors, >0 = warnings
                crResult = oTdyManaged.CleanAndRepair();

                try
                {
                    // Only call Save if CleanAndRepair succeeded (result >= 0)
                    if (crResult >= 0)
                    {
                        // Some versions of TidyManaged have issues with the internal state
                        try
                        {
                            sTidyXhtml = oTdyManaged.Save();
                        }
                        catch (InvalidOperationException ioEx)
                        {
                            // TidyManaged internal state issue - try to extract content using the underlying buffer
                            // This happens when CleanAndRepair succeeds but Save() still thinks it hasn't been called
                            try
                            {
                                // Try to write to a memory stream instead
                                using (var memStream = new System.IO.MemoryStream())
                                {
                                    oTdyManaged.Save(memStream);
                                    memStream.Position = 0;
                                    using (var reader = new System.IO.StreamReader(memStream, System.Text.Encoding.UTF8))
                                    {
                                        sTidyXhtml = reader.ReadToEnd();
                                    }
                                }
                            }
                            catch
                            {
                                // If we haven't tried escaping unknown tags yet, do that now
                                if (!bRetryWithEscapedTags)
                                {
                                    bRetryWithEscapedTags = true;
                                    oTdyManaged.Dispose();
                                    // Escape unknown/non-standard HTML tags that might be causing issues
                                    shtml = EscapeUnknownHtmlTags(originalHtml);
                                    goto RetryWithEscaping;
                                }

                                // If even that fails, return an error comment
                                sTidyXhtml = "<!-- HTML Tidy Error: Unable to save cleaned document despite successful repair (code: " + crResult + "). Error: " + ioEx.Message.Replace("<", "&lt;").Replace(">", "&gt;") + " -->";
                            }
                        }
                    }
                    else
                    {
                        // CleanAndRepair failed - try with escaped tags if we haven't already
                        if (!bRetryWithEscapedTags)
                        {
                            bRetryWithEscapedTags = true;
                            oTdyManaged.Dispose();
                            shtml = EscapeUnknownHtmlTags(originalHtml);
                            goto RetryWithEscaping;
                        }

                        sTidyXhtml = "<!-- HTML Tidy Error: CleanAndRepair failed with code " + crResult + " -->";
                    }
                }
                catch (Exception ex)
                {
                    // Try with escaped tags if we haven't already
                    if (!bRetryWithEscapedTags)
                    {
                        bRetryWithEscapedTags = true;
                        oTdyManaged.Dispose();
                        shtml = EscapeUnknownHtmlTags(originalHtml);
                        goto RetryWithEscaping;
                    }

                    sTidyXhtml = "<!-- HTML Tidy Error: " + ex.Message.Replace("<", "&lt;").Replace(">", "&gt;") + " (Result code: " + crResult + ") -->";
                }

                oTdyManaged.Dispose();
                oTdyManaged = null/* TODO Change to default(_) if this is not a reference type */;
                // End Using

                return sTidyXhtml;
            }
            catch (Exception ex)
            {
                // It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.            Return Nothing
                return crResult + " - " + ex.Message + ex.StackTrace;
            }
            // Return Nothing
            finally
            {
                sTidyXhtml = null;
            }
        }

        /// <summary>
        /// Escapes unknown/non-standard HTML tags that might cause TidyManaged to fail
        /// </summary>
        private static string EscapeUnknownHtmlTags(string html)
        {
            // Define standard HTML tags that should NOT be escaped
            var standardTags = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "a", "abbr", "address", "area", "article", "aside", "audio", "b", "base", "bdi", "bdo", "blockquote",
                "body", "br", "button", "canvas", "caption", "cite", "code", "col", "colgroup", "data", "datalist",
                "dd", "del", "details", "dfn", "dialog", "div", "dl", "dt", "em", "embed", "fieldset", "figcaption",
                "figure", "footer", "form", "h1", "h2", "h3", "h4", "h5", "h6", "head", "header", "hr", "html", "i",
                "iframe", "img", "input", "ins", "kbd", "label", "legend", "li", "link", "main", "map", "mark", "meta",
                "meter", "nav", "noscript", "object", "ol", "optgroup", "option", "output", "p", "param", "picture",
                "pre", "progress", "q", "rp", "rt", "ruby", "s", "samp", "script", "section", "select", "small",
                "source", "span", "strong", "style", "sub", "summary", "sup", "table", "tbody", "td", "template",
                "textarea", "tfoot", "th", "thead", "time", "title", "tr", "track", "u", "ul", "var", "video", "wbr"
            };

            // Regex to find all tags (opening and closing)
            return Regex.Replace(html, @"<(/?)([a-zA-Z][a-zA-Z0-9]*)((?:\s+[^>]*)?)>", match =>
            {
                string slash = match.Groups[1].Value;
                string tagName = match.Groups[2].Value;
                string attributes = match.Groups[3].Value;

                // If it's a standard HTML tag, leave it alone
                if (standardTags.Contains(tagName))
                {
                    return match.Value;
                }

                // Escape non-standard tags
                return "&lt;" + slash + tagName + attributes + "&gt;";
            });
        }

        #region Deprecated

        [Obsolete("This method is deprecated, please use Protean.Tools.Text.IsEmail instead")]
        public static bool is_valid_email(ref string str_Renamed)
        {
            return Text.IsEmail(str_Renamed);
        }

        // <Obsolete("This method is deprecated, use Protean.Tools.Text.SimpleRegexFind instead")> _
        public static string SimpleRegexFind(string cSearchString, string cRegexPattern, int nReturnGroup = 0, RegexOptions oRegexOptions = RegexOptions.None)
        {
            return Text.SimpleRegexFind(cSearchString, cRegexPattern, nReturnGroup, oRegexOptions);
        }

        [Obsolete("This method is deprecated, use Protean.Tools.FileHelper.GetMIMEType instead")]
        public static string GetMIMEType(string Extension)
        {
            return FileHelper.GetMIMEType(Extension);
        }



        #endregion


    }
}