using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Configuration;
using System.Web.Services;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Lucene.Net.Documents.Field;
using static Protean.stdTools;
using static QRCoder.PayloadGenerator;

namespace Protean
{

    [WebService(Namespace = "http://www.eonic.co.uk/ewcommon/Services")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [DesignerGenerated()]
    public class Services : WebService
    {
        #region Declarations
        public HttpContext moCtx = HttpContext.Current;
        public HttpApplicationState moApp;
        public HttpRequest moRequest;
        public HttpResponse moResponse;
        public System.Web.SessionState.HttpSessionState moSession;
        public HttpServerUtility moServer;
        private Tools.FTPClient _oFTP;

        private Tools.FTPClient oFTP
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _oFTP;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_oFTP != null)
                {
                    _oFTP.OnError -= OnError;
                }

                _oFTP = value;
                if (_oFTP != null)
                {
                    _oFTP.OnError += OnError;
                }
            }
        }
        private XmlDocument oRXML;
        private bool bResult = true;
        private XmlElement oResponseElmt;
        private string mcModuleName = "Eonic.services";
        private Cms myWeb;

        // custom class for key/value (inbuilt struct KeyValuePair does not get correctly translated with WSDL).
        public class KeyValPair
        {
            public string Key;
            public string Value;
        }

        public Services()
        {
            moApp = moCtx.Application;
            moRequest = moCtx.Request;
            moResponse = moCtx.Response;
            moSession = moCtx.Session;
            moServer = moCtx.Server;
        }
        #endregion
        #region Non Web Methods

        public void CreateResponse()
        {
            try
            {
                oRXML = new XmlDocument();
                oResponseElmt = oRXML.CreateElement("Response");
                oRXML.AppendChild(oResponseElmt);
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
            }
        }

        public void AddResponse(string cMessage)
        {
            try
            {
                var oElmt = oRXML.CreateElement("ResponseMessage");
                if (cMessage.StartsWith("<"))
                {
                    oElmt.InnerXml = cMessage;
                }
                else
                {

                    oElmt.InnerText = cMessage;
                }
                oResponseElmt.AppendChild(oElmt);
            }
            catch (Exception)
            {
                bResult = false;
                // Do nothing
            }
        }

        public bool CheckUserIP()
        {
            try
            {
                System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                string SoapIps = moConfig["SoapIps"];

                if (Strings.LCase(moConfig["Debug"]) == "on")
                {
                    SoapIps = SoapIps + ",127.0.0.1,::1,";
                }

                string cIP = GetIpAddress(moRequest);
                if (SoapIps.Contains(cIP + ","))
                {
                    return true;
                }
                else
                {
                    AddResponse("Invalid Host: " + cIP);
                    return false;
                }
            }
            catch (Exception ex)
            {
                AddResponse(ex.ToString());
                return false;
            }
        }

        public string GetIpAddress(HttpRequest request)
        {
            try
            {
                if (request.Headers["CF-CONNECTING-IP"] != null)
                    return request.Headers["CF-CONNECTING-IP"];
                string ipAddress = request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (!string.IsNullOrEmpty(ipAddress))
                {
                    string[] addresses = ipAddress.Split(',');
                    if (addresses.Length != 0)
                    {
                        // strip port
                        string thisIp = addresses[0];
                        if (thisIp.Contains(":"))
                        {
                            thisIp = thisIp.Substring(0, thisIp.IndexOf(":"));
                        }
                        return thisIp;
                    }
                }
                else
                {
                    return request.UserHostAddress;
                }
            }
            catch (Exception ex)
            {
                AddResponse(ex.ToString());
                return Conversions.ToString(false);
            }

            return default;
        }


        private void OnError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            AddResponse(e.ToString());

            // Try to generate an Eonic Error
            returnException(ref myWeb.msException, e.ModuleName, e.ProcedureName, e.Exception, vstrFurtherInfo: e.AddtionalInformation, bDebug: gbDebug, cSubjectLinePrefix: "Services: ");
        }


        #endregion
        #region Web Methods

        [WebMethod(Description = "Sends Email From Website xForm")]
        public object emailer(ref XmlElement oBodyXML, ref string xsltPath, ref string fromName, ref string fromEmail, ref string recipientEmail, ref string SubjectLine, string ccRecipient, string bccRecipient, string cSeperator)
        {

            string sMessage = "";
            //string cProcessInfo = "emailer";
            try
            {
                if (CheckUserIP())
                {
                    var myWeb = new Cms(moCtx);
                    var oMsg = new Messaging(ref myWeb.msException);
                    Cms.dbHelper odbhelper = null;
                    sMessage = oMsg.emailer(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, ref odbhelper, "", "", "", ccRecipient, bccRecipient, cSeperator).ToString();
                }
                else
                {
                    sMessage = "<div class=\"error\">Please check SOAP IP Settings for " + GetIpAddress(moRequest) + "</div>";
                }

                return sMessage;
            }

            catch (Exception ex)
            {
                return ex.Message + " - " + ex.GetBaseException().Message + " - " + ex.StackTrace;
            }

        }


        [WebMethod(Description = "Sends Email From Website xForm")]
        public object emailerMultiSend(ref XmlElement oBodyXML, ref string xsltPath, ref string fromName, ref string fromEmail, ref string recipientEmail, ref string SubjectLine, string ccRecipient, string bccRecipient, string cSeperator, string Mode)
        {

            string sMessage = "";
            //string cProcessInfo = "emailer";
            try
            {
                if (CheckUserIP())
                {
                    var myWeb = new Cms(moCtx);
                    var oMsg = new Messaging(ref myWeb.msException);
                    Cms.dbHelper odbhelper = null;

                    // using multiple addresses here
                    if (recipientEmail.Contains(cSeperator))
                    {
                        string[] oTos = Strings.Split(recipientEmail, cSeperator);
                        string[] oModes = Strings.Split(Mode, cSeperator);
                        int i;
                        var loopTo = oTos.Length - 1;
                        for (i = 0; i <= loopTo; i++)
                        {
                            oBodyXML.SetAttribute("mode", oModes[i]);
                            sMessage = oMsg.emailer(oBodyXML, xsltPath, fromName, fromEmail, oTos[i], SubjectLine, ref odbhelper, "", "", "", ccRecipient, bccRecipient, cSeperator).ToString();
                        }
                    }
                    else {
                        sMessage = oMsg.emailer(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, ref odbhelper, "", "", "", ccRecipient, bccRecipient, cSeperator).ToString();
                    }
                    if (fromEmail != "") {
                        //send confirmation to the customer
                        oBodyXML.SetAttribute("mode", "customer");
                        sMessage = oMsg.emailer(oBodyXML, xsltPath, fromName, fromEmail, fromEmail, SubjectLine, ref odbhelper, "", "", "", ccRecipient, bccRecipient, cSeperator).ToString();
                    }

                }
                else
                {
                    sMessage = "<div class=\"error\">Please check SOAP IP Settings for " + GetIpAddress(moRequest) + "</div>";
                }

                return sMessage;
            }

            catch (Exception ex)
            {
                return ex.Message + " - " + ex.GetBaseException().Message + " - " + ex.StackTrace;
            }

        }

        [WebMethod(Description = "Sends Email From Website xForm")]
        public object emailerXMLAttach(ref XmlElement oBodyXML, ref string xsltPath, ref string fromName, ref string fromEmail, ref string recipientEmail, ref string SubjectLine, string ccRecipient, string bccRecipient, string cSeperator, string attachmentFromXSLPath, string attachmentFromXSLType, string attachmentName)
        {
            string sMessage;
            //string cProcessInfo = "emailerXMLAttach";
            try
            {
                var myWeb = new Cms(moCtx);
                var oMsg = new Messaging(ref myWeb.msException);
                Cms.dbHelper odbhelper = null;
                sMessage = Conversions.ToString(oMsg.emailerWithXmlAttachment(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, attachmentFromXSLPath, attachmentFromXSLType, attachmentName, ref odbhelper, "Message Sent", "Message Failed", recipientEmail, ccRecipient, bccRecipient, cSeperator));
                return sMessage;
            }
            catch (Exception ex)
            {
                return ex.Message + " - " + ex.GetBaseException().Message;
            }
        }

        [WebMethod(Description = "Sends Email To Multiple Recipients")]
        public object multiEmailer(ref XmlElement oBodyXML, ref string xsltPath, ref string fromName, ref string fromEmail, ref string recipientIds, ref string SubjectLine)
        {
            string sMessage;
            //string cProcessInfo = "multiEmailer";
            try
            {

                var myWeb = new Cms(moCtx);
                var oMsg = new Messaging(ref myWeb.msException);

                sMessage = Conversions.ToString(oMsg.emailerMultiUsers(oBodyXML, xsltPath, fromName, fromEmail, recipientIds, SubjectLine));

                return sMessage;
            }

            catch (Exception ex)
            {
                return ex.Message + " - " + ex.GetBaseException().Message;
            }

        }

        [WebMethod(Description = "Sends Email From Website xForm with Attachement As Stream in base 64 string format")]
        public object emailerWithAttachmentStreams(ref XmlElement oBodyXML, ref string xsltPath, ref string fromName, ref string fromEmail, ref string recipientEmail, ref string SubjectLine, string ccRecipient, string bccRecipient, string cSeperator, KeyValPair[] attachmentsBase64List)
        {
            string sMessage;
            // string cProcessInfo = "emailerWithAttachmentStreams";
            try
            {
                var myWeb = new Cms(moCtx);
                var oMsg = new Messaging(ref myWeb.msException);

                if (!(attachmentsBase64List == null))
                {
                    foreach (KeyValPair attachment in attachmentsBase64List)
                    {
                        string attachmentBase64 = attachment.Value;
                        byte[] byteArray = Convert.FromBase64String(attachmentBase64);
                        var contentStream = new MemoryStream(byteArray);

                        if (!(contentStream == null))
                        {
                            oMsg.addAttachment(contentStream, attachment.Key);
                        }
                    }
                }
                Cms.dbHelper odbhelper = null;
                sMessage = Conversions.ToString(oMsg.emailer(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, ref odbhelper, "Message Sent", "Message Failed", "", ccRecipient, bccRecipient, cSeperator));
                return sMessage;
            }

            catch (Exception ex)
            {
                return ex.Message + " - " + ex.GetBaseException().Message;
            }

        }

        [WebMethod(Description = "Sends Email From Website xForm with Attachment (from physical file)")]
        public object emailerWithAttachment(ref XmlElement oBodyXML, ref string xsltPath, ref string fromName, ref string fromEmail, ref string recipientEmail, ref string SubjectLine, string ccRecipient, string bccRecipient, string cSeperator, string cAttachmentFilePath, bool bDeleteAfterSend)
        {
            string sMessage;
            //string cProcessInfo = "emailerWithAttachment";
            try
            {
                var myWeb = new Cms(moCtx);
                var oMsg = new Messaging(ref myWeb.msException);

                oMsg.addAttachment(cAttachmentFilePath, bDeleteAfterSend);
                Cms.dbHelper odbhelper = null;
                sMessage = Conversions.ToString(oMsg.emailer(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, ref odbhelper, "Message Sent", "Message Failed", "", ccRecipient, bccRecipient, cSeperator));
                oMsg.deleteAttachment(cAttachmentFilePath);
                return sMessage;
            }

            catch (Exception ex)
            {
                return ex.Message + " - " + ex.GetBaseException().Message;
            }

        }

        [WebMethod(Description = "Sends Email From Website xForm with Attachment and delete file (from physical file)")]
        public object emailerWithAttachmentPDF(ref XmlElement oBodyXML, ref string xsltPath, ref string fromName, ref string fromEmail, ref string recipientEmail, ref string SubjectLine, string ccRecipient, string bccRecipient, string cSeperator, string cAttachmentFilePath, bool bDeleteAfterSend)
        {
            string sMessage;
            //string cProcessInfo = "emailerWithAttachment";
            try
            {
                var myWeb = new Cms(moCtx);
                var oMsg = new Messaging(ref myWeb.msException);
                string arrayItem;
                string[] strFilePath = cAttachmentFilePath.Split(',');

                foreach (var currentArrayItem in strFilePath)
                {
                    arrayItem = currentArrayItem;
                    oMsg.addAttachment(arrayItem, bDeleteAfterSend);
                }
                Cms.dbHelper odbhelper = null;
                sMessage = Conversions.ToString(oMsg.emailer(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, ref odbhelper, "Message Sent", "Message Failed", "", ccRecipient, bccRecipient, cSeperator));
                // deleting physical files given full path
                foreach (var currentArrayItem1 in strFilePath)
                {
                    arrayItem = currentArrayItem1;
                    oMsg.deleteAttachmentPDF(arrayItem);
                }

                return sMessage;
            }

            catch (Exception ex)
            {
                return ex.Message + " - " + ex.GetBaseException().Message;
            }

        }

        [WebMethod(Description = "Sends Email From Website xForm with Attachment (from physical file)")]
        public object emailerWithFTPAttachment(ref XmlElement oBodyXML, ref string xsltPath, ref string fromName, ref string fromEmail, ref string recipientEmail, ref string SubjectLine, string ccRecipient, string bccRecipient, string cSeperator, string cAttachmentFilePath, bool bDeleteAfterSend, string FTPServer, string FTPUsername, string FTPPassword, string FTPFolder)
        {
            string sMessage;
            //string cProcessInfo = "emailerWithAttachment";
            try
            {
                var oMsg = new Messaging(ref myWeb.msException);
                // oMsg.addAttachment(cAttachmentFilePath, bDeleteAfterSend)
                // oMsg.deleteAttachment(cAttachmentFilePath)
                string FileName = cAttachmentFilePath.Substring(cAttachmentFilePath.LastIndexOf("/") + 1);
                FTPFolder = FTPFolder.Trim('/') + "/" + Strings.Replace(fromName, " ", "-");

                // Dim miUri As String = "ftp://" & FTPServer & "/" & FTPFolder & "/" & Replace(fromName, " ", "-") & "/" & FileName
                // Dim miRequest As Net.FtpWebRequest = Net.WebRequest.Create(miUri)
                // miRequest.Credentials = New Net.NetworkCredential(FTPUserName, FTPPassword)
                // miRequest.Method = Net.WebRequestMethods.Ftp.UploadFile
                try
                {
                    // Dim bFile() As Byte = System.IO.File.ReadAllBytes(cAttachmentFilePath)
                    // Dim miStream As System.IO.Stream = miRequest.GetRequestStream()
                    // miStream.Write(bFile, 0, bFile.Length)
                    // miStream.Close()
                    // miStream.Dispose()

                    var ftp = new FTPHelper(FTPServer, FTPUsername, FTPPassword);
                    var UploadClient = new WebClient();
                    var DownloadClient = new WebClient();

                    ftp.Connect();
                    ftp.CreateDirectory("/" + FTPFolder, true);
                    ftp.UploadFile(ref UploadClient, cAttachmentFilePath, FTPFolder + "/" + FileName, false);

                    if (bDeleteAfterSend)
                    {
                        var fsh = new fsHelper();
                        fsh.DeleteFile(cAttachmentFilePath);
                    }
                }

                catch (Exception ex)
                {
                    // Throw New Exception(ex.Message & "FTP Failed")
                    oBodyXML.SetAttribute("error", "FTP Failed: " + ex.Message);
                }
                Cms.dbHelper odbhelper = null;
                sMessage = Conversions.ToString(oMsg.emailer(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, ref odbhelper, "Message Sent", "Message Failed", "", ccRecipient, bccRecipient, cSeperator));

                return sMessage;
            }

            catch (Exception ex)
            {
                return ex.Message + " - " + ex.GetBaseException().Message;
            }

        }

        [WebMethod()]
        public XmlDocument LuceneIndex()
        {
            try
            {
                CreateResponse();
                if (CheckUserIP())
                {

                    // Dim myThread As System.Threading.Thread
                    // myThread = New System.Threading.Thread(AddressOf Services.LuceneIndexAsync)
                    // myThread.Start(HttpContext.Current)
                    string sResult = LuceneIndexAsync(HttpContext.Current);

                    Tools.Xml.addElement(ref oResponseElmt, "Message", sResult);
                    bResult = true;

                    // Dim oIndexer As New Protean.Indexer(New Protean.Cms)
                    // oIndexer.DoIndex(0, bResult)
                    // Dim cSubResponse As String = oIndexer.cExError
                    // If cSubResponse = "" Then
                    // bResult = True
                    // cSubResponse = "Completed Successfully"
                    // Else
                    // bResult = False
                    // End If
                    // AddResponse(cSubResponse)
                    // AddResponse(vbCrLf & "Pages: " & oIndexer.nPagesIndexed)
                    // AddResponse(vbCrLf & "Documents: " & oIndexer.nDocumentsIndexed)
                    // AddResponse(vbCrLf & "Contents: " & oIndexer.nContentsIndexed)

                }
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }
            return oRXML;
        }

        public static string LuceneIndexAsync(HttpContext oCtx)
        {
            var bResult = default(bool);
            string sResult = "";
            var myWeb = new Cms(oCtx);
            myWeb.Open();
            try
            {

                var oIndexer = new IndexerAsync(ref myWeb);

                bool argbResult = Conversions.ToBoolean(0);
                sResult = oIndexer.DoIndex(ref argbResult, Conversions.ToInteger(bResult));

                string cSubResponse = oIndexer.cExError;
                if (string.IsNullOrEmpty(cSubResponse))
                {
                    bResult = true;
                }
                else
                {
                    sResult = "Indexer Error" + cSubResponse;
                    bResult = false;
                }

                // sResult = sResult & " Pages: " & oIndexer.nPagesIndexed
                // sResult = sResult & " Documents: " & oIndexer.nDocumentsIndexed
                // sResult = sResult & " Contents: " & oIndexer.nContentsIndexed

                myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.Search, 0L, 0L, 0L, sResult);
                return sResult;
            }

            catch (Exception ex)
            {
                bResult = false;
                myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.Search, 0L, 0L, 0L, ex.ToString());
                return null;
            }
            finally
            {

                myWeb.Close();
                myWeb = null;
            }

        }

        [WebMethod()]
        public XmlDocument DatabaseUpgrade()
        {
            try
            {
                CreateResponse();
                if (CheckUserIP())
                {
                    var oSetup = new Setup();
                    bResult = oSetup.UpdateDatabase();
                    string cSubResponse = oSetup.oResponse.OuterXml;
                    AddResponse(cSubResponse);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }
            return oRXML;
        }

        [WebMethod()]
        public XmlDocument Feed(string cURL, string cXSLPath, long nPageId, int nSaveMode, string cItemNodeName)
        {
            try
            {
                CreateResponse();
                if (CheckUserIP())
                {
                    var oResElmt = oResponseElmt.OwnerDocument.CreateElement("Response");
                    var oFeeder = new FeedHandler(cURL, cXSLPath, nPageId, nSaveMode, ref oResElmt, cItemNodeName);
                    bResult = oFeeder.ProcessFeeds();
                    string cSubResponse = oResElmt.InnerXml;
                    AddResponse(cSubResponse);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }

            HttpContext.Current.ApplicationInstance.CompleteRequest();
            return oRXML;
            // moResponse.Flush()
            // moResponse.OutputStream.Close()
            // moResponse.End()

        }

        [WebMethod()]
        public XmlDocument SubscriptionProcess()
        {
            var myWeb = new Cms(HttpContext.Current);
            Cms.gbCart = false;
            myWeb.Open();
            try
            {
                CreateResponse();
                if (CheckUserIP())
                {
                    var oResElmt = oResponseElmt.OwnerDocument.CreateElement("Response");
                    var oSubscriptions = new Cms.Cart.Subscriptions(ref myWeb);
                    oResElmt.InnerXml = oSubscriptions.SubcriptionReminders().OuterXml;
                    if (bResult)
                        AddResponse("Reminders Complete");
                    // bResult = oSubscriptions.CheckExpiringSubscriptions()
                    // If bResult Then AddResponse("Expiring Subscriptions Complete")
                    string cSubResponse = oResElmt.InnerXml;
                    AddResponse(cSubResponse);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }
            return oRXML;
        }
        [WebMethod()]
        public XmlDocument FTPUploadPage(string SourceURL, string FTPServer, string FTPUserName, string FTPPassword, string FTPFilePath, string FTPFileName)
        {
            try
            {
                CreateResponse();
                if (CheckUserIP())
                {
                    string cPageSourceText = "";
                    HttpWebRequest oWebRequest;
                    HttpWebResponse oWebResponse = null;
                    StreamReader oSReader;
                    // request the page
                    oWebRequest = (HttpWebRequest)WebRequest.Create(SourceURL);
                    oWebRequest.KeepAlive = false;
                    oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
                    oSReader = new StreamReader(oWebResponse.GetResponseStream());
                    cPageSourceText = oSReader.ReadToEnd();
                    oWebRequest.Abort();
                    oWebResponse.Close();
                    oWebRequest = null;
                    oWebResponse = null;

                    oFTP = new Tools.FTPClient(FTPServer, FTPUserName, FTPPassword);

                    bResult = oFTP.UploadText(cPageSourceText, FTPFilePath, FTPFileName);
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }
            return oRXML;
        }

        [WebMethod(Description = "ScheduleMonitorXml")]
        public XmlDocument UserAlerts()
        {
            try
            {
                myWeb = new Cms();
                myWeb.Open();
                CreateResponse();
                if (CheckUserIP())
                {
                    var oAlerts = new Cms.Membership.Alerts(myWeb);
                    oAlerts.OnError += OnError;

                    oResponseElmt.AppendChild(oResponseElmt.OwnerDocument.ImportNode(oAlerts.CurrentAlerts(ref bResult), true));
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
                returnException(ref myWeb.msException, mcModuleName, "UserAlerts", ex, bDebug: gbDebug);
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }
            return oRXML;
        }


        [WebMethod(Description = "ScheduleMonitorXml")]
        public XmlDocument ScheduleMonitorXml()
        {
            var oDb = new Tools.Database();
            try
            {
                myWeb = new Cms();
                myWeb.Open();

                CreateResponse();
                if (CheckUserIP())
                {

                    var oMonitor = new Monitor(ref myWeb);
                    oMonitor.OnError += OnError;
                    oResponseElmt.AppendChild(oResponseElmt.OwnerDocument.ImportNode(oMonitor.GetMonitorSchedulerXml(), true));
                    bResult = true;

                }
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
                returnException(ref myWeb.msException, mcModuleName, "Monitor", ex, bDebug: gbDebug);
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }
            return oRXML;
        }

        [WebMethod()]
        public XmlDocument ScheduleMonitor()
        {
            var oDb = new Tools.Database();
            try
            {
                myWeb = new Cms();
                myWeb.Open();
                CreateResponse();
                if (CheckUserIP())
                {

                    var oMonitor = new Monitor(ref myWeb);
                    oMonitor.OnError += OnError;
                    Tools.Xml.addElement(ref oResponseElmt, "Message", oMonitor.EmailMonitorScheduler());
                    bResult = true;

                }
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
                returnException(ref myWeb.msException, mcModuleName, "Monitor", ex, bDebug: gbDebug);
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }
            return oRXML;
        }


        [WebMethod(Description = "GetPendingContent")]
        public XmlDocument GetPendingContent()
        {
            var oDb = new Tools.Database();
            System.Collections.Specialized.NameValueCollection oVConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/versioncontrol");
            try
            {
                myWeb = new Cms();
                myWeb.Open();
                CreateResponse();
                if (CheckUserIP())
                {

                    if (myWeb.gbVersionControl)
                    {

                        var oResponse = myWeb.moDbHelper.getPendingContent(true);
                        if (oResponse is null)
                        {
                            AddResponse("There is no content currently awaiting approval");
                        }
                        else
                        {
                            // Email the response
                            var oMsg = new Messaging(ref myWeb.msException);

                            string cEmail = oVConfig["notificationEmail"];
                            string cXSLPath = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty("" + oVConfig["notificationXsl"]), "/ewcommon/xsl/Email/pendingcontentNotification.xsl", oVConfig["notificationXsl"]));
                            string cWebmasterEmail = oVConfig["notificationEmailSender"];
                            if (string.IsNullOrEmpty(cWebmasterEmail))
                                cWebmasterEmail = myWeb.moConfig["SiteAdminEmail"];
                            string SenderName = myWeb.moConfig["SiteName"] + " Notification";

                            Cms.dbHelper odbhelper = null;
                            string cMessage = Conversions.ToString(oMsg.emailer(oResponse, cXSLPath, SenderName, cWebmasterEmail, cEmail, "", ref odbhelper, "Message Sent", "Message Failed", "", "", "", ""));
                            AddResponse(cMessage);
                        }
                        bResult = true;
                    }

                    else
                    {
                        bResult = false;
                        AddResponse("Version Control is not enabled for this site.  Please check the config, and ewcommon application setup");
                    }

                }
            }
            catch (Exception ex)
            {
                bResult = false;
                AddResponse(ex.ToString());
                returnException(ref myWeb.msException, mcModuleName, "GetPendingContent", ex, bDebug: gbDebug);
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
                myWeb.Close();
                myWeb = null;
            }
            return oRXML;
        }

        [WebMethod(Description = "Syndicate content from the website to distributors")]
        public XmlDocument Syndicate(string distributors, string contentTypes, int page, bool iterate)
        {
            try
            {
                myWeb = new Cms();
                myWeb.Open();
                CreateResponse();
                if (CheckUserIP())
                {
                    var oSyndication = new Syndication(ref myWeb, distributors, contentTypes, page, iterate);
                    oSyndication.OnError += OnError;

                    oSyndication.Syndicate();

                    bResult = oSyndication.IsCompleted;

                    if (!oSyndication.IsContentsNodePopulated)
                    {
                        AddResponse("There was no content found to syndicate");
                    }
                    else
                    {
                        AddResponse("Total content found for syndication: " + oSyndication.ContentCount);
                    }

                    if (!bResult)
                    {
                        AddResponse(oSyndication.Diagnostics);
                    }
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                // returnException(mcModuleName, "Syndicate", ex, , , gbDebug)
                AddResponse(ex.ToString());
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
                myWeb.Close();
                myWeb = null;
            }

            return oRXML;

        }

        [WebMethod(Description = "Syndicate content from the website to distributors with distributor specific settings")]
        public XmlDocument SyndicateExtended(string distributors, string contentTypes, int page, bool iterate, XmlElement extendedSettings)
        {
            try
            {
                myWeb = new Cms();
                myWeb.Open();
                CreateResponse();
                if (CheckUserIP())
                {
                    var oSyndication = new Syndication(ref myWeb, distributors, contentTypes, page, iterate, extendedSettings);
                    oSyndication.OnError += OnError;

                    oSyndication.Syndicate();

                    bResult = oSyndication.IsCompleted;

                    if (!oSyndication.IsContentsNodePopulated)
                    {
                        AddResponse("There was no content found to syndicate");
                    }
                    else
                    {
                        AddResponse("Total content found for syndication: " + oSyndication.ContentCount);
                    }

                    if (!bResult)
                    {
                        AddResponse(oSyndication.Diagnostics);
                    }
                }
            }
            catch (Exception ex)
            {
                bResult = false;
                // returnException(mcModuleName, "SyndicateExtended", ex, , , gbDebug)
                AddResponse(ex.ToString());
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
                myWeb.Close();
                myWeb = null;
            }
            return oRXML;
        }

        [WebMethod(Description = "GetStatus")]
        public XmlDocument GetStatus()
        {
            var oDb = new Tools.Database();
            System.Collections.Specialized.NameValueCollection oVConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/versioncontrol");
            try
            {
                myWeb = new Cms();
                myWeb.Open();
                CreateResponse();
                System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                string sSql;
                var oElmt = oRXML.CreateElement("Debug");
                oElmt.InnerText = moConfig["debug"];
                oResponseElmt.AppendChild(oElmt);

                // 'Start URI
                // oElmt = oRXML.CreateElement("AbsoluteURI")
                // oElmt.InnerText = System.Web.HttpContext.Current.Request.Url.AbsoluteUri
                // oResponseElmt.AppendChild(oElmt)
                // 'End URI

                oElmt = oRXML.CreateElement("DBVersion");
                try
                {
                    // Dim oDr As System.Data.SqlClient.SqlDataReader
                    string sResult = "";
                    sSql = "select * from tblSchemaVersion";
                    using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            while (oDr.Read())
                                sResult = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oDr[1], "."), oDr[2]), "."), oDr[3]), "."), oDr[4]));
                        }
                        else
                        {
                            sResult = "nodata";
                        }
                    }
                    oElmt.InnerText = sResult;
                }
                catch (Exception)
                {
                    if (string.IsNullOrEmpty(moConfig["VersionNumber"]))
                    {
                        oElmt.InnerText = "Pre V4 or no Eonicweb";
                    }
                    else
                    {
                        oElmt.InnerText = moConfig["VersionNumber"];
                    }

                }

                oResponseElmt.AppendChild(oElmt);


                // Start DotNetversion
                oElmt = oRXML.CreateElement("DotNetVersion");
                oElmt.InnerText = Environment.Version.ToString();
                oResponseElmt.AppendChild(oElmt);
                // End DotNetversion

                // Start LocalOrCommonBin
                oElmt = oRXML.CreateElement("LocalOrCommonBin");
                if (!string.IsNullOrEmpty(FileSystem.Dir(Server.MapPath("../default.ashx"))))
                {
                    oElmt.InnerText = "Local";
                }
                else if (!string.IsNullOrEmpty(FileSystem.Dir(Server.MapPath("../ewcommon/default.ashx"))))
                {
                    oElmt.InnerText = "Common";
                }
                else
                {
                    oElmt.InnerText = "Error";
                }
                oResponseElmt.AppendChild(oElmt);
                // End LocalOrCommonBin


                // Start DLL Version

                oElmt = oRXML.CreateElement("DLLVersion");

                // Dim sCodeBase As String = myWeb.moRequest.ServerVariables("GENERATOR")
                // oElmt.InnerText = sCodeBase

                // oElmt.InnerText = myWeb.Generator().FullName()

                oElmt.InnerText = Cms.gcGenerator.ToString();

                // Dim CodeGenerator As Assembly = Me.Generator()
                // gcGenerator = CodeGenerator.FullName()

                oResponseElmt.AppendChild(oElmt);

                // End DLL Version


                // Start LatestDBVersion
                oElmt = oRXML.CreateElement("LatestDBVersion");
                XmlTextReader LatestDBVersion;
                LatestDBVersion = new XmlTextReader(Server.MapPath("../ewcommon/sqlUpdate/DatabaseUpgrade.xml"));
                LatestDBVersion.WhitespaceHandling = WhitespaceHandling.None;
                // Disable whitespace so that it doesn't have to read over whitespaces
                LatestDBVersion.Read();
                LatestDBVersion.Read();
                string LatestDBversionAttribute = LatestDBVersion.GetAttribute("LatestVersion");
                oElmt.InnerText = LatestDBversionAttribute;
                oResponseElmt.AppendChild(oElmt);
                // End LatestDBVersion


                // Start EnabledFeatures

                oElmt = oRXML.CreateElement("Membership");
                oElmt.InnerText = moConfig["Membership"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Cart");
                oElmt.InnerText = moConfig["Cart"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Quote");
                oElmt.InnerText = moConfig["Quote"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Report");
                oElmt.InnerText = moConfig["Report"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Subscriptions");
                oElmt.InnerText = moConfig["Subscriptions"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("MailingList");
                oElmt.InnerText = moConfig["MailingList"];
                oResponseElmt.AppendChild(oElmt);

                oElmt = oRXML.CreateElement("Search");
                oElmt.InnerText = moConfig["Search"];
                oResponseElmt.AppendChild(oElmt);
            }

            // End Enabledfeatures


            catch (Exception ex)
            {
                bResult = false;
                // returnException(mcModuleName, "GetPendingContent", ex, , , gbDebug)
                AddResponse(ex.ToString());
            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
                myWeb.Close();
                myWeb = null;
            }
            return oRXML;
        }

        [WebMethod(Description = "get content detail xml")]
        public XmlElement GetContentDetailXml(int nContentKey)
        {
            XmlElement xmlEle;

            // We should be checking users permissions here.

            xmlEle = myWeb.GetContentDetailXml(null, nContentKey, false);


            return xmlEle;
        }


        [WebMethod(Description = "get content url")]
        public string GetContentUrl(int contentId)
        {
            myWeb = new Cms();
            myWeb.Open();
            string url;
            url = myWeb.GetContentUrl(contentId);
            myWeb.Close();
            return url;

        }

        [WebMethod(Description = "get standard filter for sql content")]
        public object GetStandardFilterSQLForContent()
        {
            myWeb = new Cms();
            myWeb.Open();

            myWeb.GetStandardFilterSQLForContent();
            myWeb.Close();
            return null;

        }

        [WebMethod(Description = "check page permission")]
        public int checkPagePermission(int parId)
        {
            myWeb = new Cms();
            myWeb.Open();

            int data = Convert.ToInt32(myWeb.moDbHelper.checkPagePermission(parId));
            myWeb.Close();
            return data;

        }

        [WebMethod(Description = "return admin mode")]
        public bool returnIsAdminMode()
        {
            myWeb = new Cms();
            myWeb.Open();

            bool data = myWeb.mbAdminMode;
            myWeb.Close();
            return data;
        }

        [WebMethod(Description = "create Element")]
        public XmlElement createElement(XmlElement oRoot)
        {
            myWeb = new Cms();
            myWeb.Open();
            oRoot = myWeb.moPageXml.CreateElement("Contents");
            myWeb.moPageXml.DocumentElement.AppendChild(oRoot);
            myWeb.Close();
            return oRoot;

        }

        [WebMethod(Description = "update content index")]

        public void RunContentFilterIndex()
        {
            try
            {
                CreateResponse();
                if (CheckUserIP())
                {
                    myWeb = new Cms();
                    myWeb.Open();

                    string sSql = "spScheduleToUpdateIndexTable";
                    var arrParms = new Hashtable();
                    arrParms.Add("IndexId", "");
                    myWeb.moDbHelper.ExeProcessSql(sSql, CommandType.StoredProcedure, arrParms);

                }
            }
            catch (Exception ex)
            {
                bResult = false;

            }
            finally
            {
                oResponseElmt.SetAttribute("bResult", Conversions.ToString(bResult));
            }

            HttpContext.Current.ApplicationInstance.CompleteRequest();

        }
       
        #endregion

    }
}