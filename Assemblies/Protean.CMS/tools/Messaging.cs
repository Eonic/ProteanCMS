using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using PreMailer.Net;
using Protean.Providers.Messaging;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{

    public class Messaging
    {

        public bool mbEncrypt = false;
        public string msGnuDirectory = @"C:\GNU\GNUPG";
        public string msGnuOriginator = "";
        public string msGnuPassphrase = "";
        public bool mbIsBodyHtml = true;
        public static string msException = "";

        public System.Web.HttpContext moCtx = System.Web.HttpContext.Current;

        public System.Web.HttpApplicationState goApp;

        public System.Web.HttpRequest goRequest;
        public System.Web.HttpResponse goResponse;
        public System.Web.SessionState.HttpSessionState goSession;
        public System.Web.HttpServerUtility goServer;
        public System.Collections.Specialized.NameValueCollection goConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

        public string mcModuleName = "Protean.Messaging";
        public bool sendAsync = false;

        private string msAttachmentPath = "";
        private Collection Attachments;
        private static bool mailSent = false;

        private string _language = "";

        public Messaging()
        {
            goApp = (System.Web.HttpApplicationState)Interaction.IIf(moCtx == null, null, moCtx.Application);
            goRequest = moCtx.Request;
            goResponse = moCtx.Response;
            goSession = moCtx.Session;
            goServer = moCtx.Server;

        }
        public Messaging(ref string sException)
        {
            goApp = (System.Web.HttpApplicationState)Interaction.IIf(moCtx is null, null, moCtx.Application);
            goRequest = moCtx.Request;
            goResponse = moCtx.Response;
            goSession = moCtx.Session;
            goServer = moCtx.Server;
            msException = sException;
        }

        public string Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        public bool HasLanguage
        {
            get
            {
                return !string.IsNullOrEmpty(_language);
            }
        }

        public void addAttachment(Stream contentStream, string name)
        {
            string cProcessInfo = "emailCart";
            try
            {

                var oAtt = new Attachment(contentStream, name);
                if (Attachments is null)
                {
                    Attachments = new Collection();
                }
                Attachments.Add(oAtt);
            }

            catch (Exception ex)
            {
                if (gbDebug)
                {
                    returnException(ref msException, mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug);
                }
            }
        }
        public void addAttachmentAsPdf(Stream contentStream, string name)
        {
            string cProcessInfo = "emailCart";
            try
            {
                contentStream.Position = 0L;

                var oAtt = new Attachment(contentStream, name, "application/pdf");
                if (Attachments is null)
                {
                    Attachments = new Collection();
                }
                Attachments.Add(oAtt);
            }

            catch (Exception ex)
            {
                if (gbDebug)
                {
                    returnException(ref msException, mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug);
                }
            }
        }

        public void addAttachment(string fileLocation, bool deleteAfterAttach = false)
        {
            string cProcessInfo = "emailCart";
            try
            {
                if (!string.IsNullOrEmpty(fileLocation))
                {
                    // check if filesystem path allready supplied
                    if (!fileLocation.Contains(":/"))
                    {
                        fileLocation = goServer.MapPath("/") + fileLocation;
                    }

                    var oAtt = new Attachment(fileLocation);
                    // rewrite the name
                    oAtt.Name = Path.GetFileName(fileLocation);

                    if (Attachments is null)
                    {
                        Attachments = new Collection();
                    }
                    Attachments.Add(oAtt);

                    // NB : 01-09-2009 Commented, See deleteAttachment
                    // As here only causes "File in Use" exceptions in the fsHelper
                    // If deleteAfterAttach Then
                    // Dim fsh As Protean.fsHelper = New fsHelper
                    // fsh.DeleteFile(goServer.MapPath("") & fileLocation)
                    // End If

                }
            }

            catch (Exception ex)
            {
                if (gbDebug)
                {
                    returnException(ref msException, mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug);
                }
            }
        }

        public void addAttachment(byte[] bytes, string name, string contenttype)
        {
            try
            {
                Stream contentStream;
                contentStream = new MemoryStream(bytes);
                var oAtt = new Attachment(contentStream, name, contenttype);
                if (Attachments is null)
                {
                    Attachments = new Collection();
                }
                Attachments.Add(oAtt);
            }
            catch (Exception ex)
            {
                if (gbDebug)
                {
                    returnException(ref msException, mcModuleName, "addAttachment", ex, "", "", gbDebug);
                }
            }
        }


        public void AddAttachmentsByIds(ref XmlElement RootElmt, string ids, string xPath)
        {
            try
            {
                string sSql;
                DataSet oDs;
                string strFilePath;
                string strError = "";

                string dbAuth;
                if (!string.IsNullOrEmpty(goConfig["DatabasePassword"]))
                {
                    dbAuth = "user id=" + goConfig["DatabaseUsername"] + "; password=" + goConfig["DatabasePassword"];
                }
                else if (!string.IsNullOrEmpty(goConfig["DatabaseAuth"]))
                {
                    dbAuth = goConfig["DatabaseAuth"];
                }
                else
                {
                    dbAuth = "Integrated Security=SSPI;";
                }

                string dbConn = "Data Source=" + goConfig["DatabaseServer"] + "; " + "Initial Catalog=" + goConfig["DatabaseName"] + "; " + dbAuth;

                var moDbhelper = new Cms.dbHelper(dbConn, 1);
                moDbhelper.moPageXml = RootElmt.OwnerDocument;
                var AttachmentsElmt = RootElmt.OwnerDocument.CreateElement("Attachments");
                RootElmt.ParentNode.AppendChild(AttachmentsElmt);
                if (!string.IsNullOrEmpty(ids))
                {
                    sSql = "select * from tblContent where nContentKey in (" + ids + ")";
                    oDs = moDbhelper.GetDataSet(sSql, "Item");
                    foreach (DataRow dsRow in oDs.Tables["Item"].Rows)
                    {
                        strFilePath = moDbhelper.getContentFilePath(dsRow, xPath);

                        var AttachmentElmt = RootElmt.OwnerDocument.CreateElement("Attachment");
                        AttachmentsElmt.AppendChild(AttachmentElmt);
                        AttachmentElmt.AppendChild(moDbhelper.GetContentDetailXml(Convert.ToInt32(dsRow["nContentKey"])));
                        AttachmentElmt.SetAttribute("file", strFilePath);

                        try
                        {
                            var oAtt = new Attachment(strFilePath);
                            if (Attachments is null)
                            {
                                Attachments = new Collection();
                            }
                            Attachments.Add(oAtt);
                        }
                        catch (Exception)
                        {
                            strError += "Missing file: " + strFilePath + "<br/>";
                            RootElmt.SetAttribute("error", "Missing file:" + strFilePath);
                        }
                    }
                    oDs = null;
                }
            }

            catch (Exception ex)
            {
                if (gbDebug)
                {
                    returnException(ref msException, mcModuleName, "addAttachment", ex, "", "", gbDebug);
                }
            }
        }

        public void clearAttachments()
        {
            string cProcessInfo = "emailCart";
            try
            {

                if (Attachments != null)
                {
                    Attachments.Clear();
                }
            }

            catch (Exception ex)
            {
                if (gbDebug)
                {
                    returnException(ref msException, mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug);
                }
            }
        }

        public void deleteAttachment(string fileLocation)
        {
            string cProcessInfo = "emailCart";
            try
            {
                if (!string.IsNullOrEmpty(fileLocation))
                {

                    if (Attachments != null)
                    {
                        Attachments.Clear();
                    }

                    Protean.fsHelper fsh = new fsHelper();
                    fsh.DeleteFile(goServer.MapPath("/") + fileLocation);

                }
            }

            catch (Exception ex)
            {
                if (gbDebug)
                {
                    returnException(ref msException, mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug);
                }
            }
        }

        // deleting physical files given full path
        public void deleteAttachmentPDF(string fileLocation)
        {
            string cProcessInfo = "emailCart";
            try
            {
                if (!string.IsNullOrEmpty(fileLocation))
                {

                    if (Attachments != null)
                    {
                        Attachments.Clear();
                    }

                    Protean.fsHelper fsh = new fsHelper();
                    fsh.DeleteFile(fileLocation);

                }
            }

            catch (Exception ex)
            {
                if (gbDebug)
                {
                    returnException(ref msException, mcModuleName, "deleteAttachmentPDF", ex, "", cProcessInfo, gbDebug);
                }
            }
        }

        public virtual void AddToLists(string StepName, string Name = "", string Email = "", System.Collections.Generic.Dictionary<string, string> valDict = null)
        {

            string cProcessInfo = "";
            try
            {
                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                if (moMailConfig != null)
                {

                    string sMessagingProvider = "";

                    if (moMailConfig != null)
                    {
                        sMessagingProvider = moMailConfig["MessagingProvider"];
                    }

                    if (!string.IsNullOrEmpty(sMessagingProvider))
                    {
                        var myWeb = new Cms(moCtx);
                        Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                        IMessagingProvider moMessaging = RetProv.Get(ref myWeb, sMessagingProvider);

                        if (valDict is null)
                            valDict = new System.Collections.Generic.Dictionary<string, string>();

                        string ListId = "";
                        ListId = StepName;

                        if (!string.IsNullOrEmpty(ListId))
                        {
                            moMessaging.Activities.AddToList(ListId, Name, Email, valDict);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "purchaseActions", ex, "", cProcessInfo, gbDebug);
            }

        }
        public object emailer(XmlElement oBodyXML, string xsltPath, string fromName, string fromEmail, string recipientEmail, string SubjectLine, string successMessage = "Message Sent", string failureMessage = "Message Failed", string recipientName = "", string ccRecipient = "", string bccRecipient = "", string cSeperator = "") {
            Cms.dbHelper mydbhelper = null;
            return emailer(oBodyXML,xsltPath,fromName, fromEmail, recipientEmail, SubjectLine, ref mydbhelper, successMessage,failureMessage, recipientName, ccRecipient,bccRecipient, cSeperator, "","");
            }

        public object emailer(XmlElement oBodyXML, string xsltPath, string fromName, string fromEmail, string recipientEmail, string SubjectLine, ref Protean.Cms.dbHelper odbHelper, string successMessage = "Message Sent", string failureMessage = "Message Failed", string recipientName = "", string ccRecipient = "", string bccRecipient = "", string cSeperator = "", string cPickupHost = "", string cPickupLocation = "")
        {
           
            // PerfMon.Log("Messaging", "emailer")
            if (cSeperator is null)
            {
                cSeperator = ",";
            }
            else if (string.IsNullOrEmpty(cSeperator))
            {
                cSeperator = ",";
            }
            string styleFile;
            TextWriter sWriter = new StringWriter();
            var oXml = new XmlDocument();
            string cProcessInfo = "Emailer";
            var oTransform = new Protean.XmlHelper.Transform();

            // User counts
            int nTotalAddressesAttempted = 0;
            int nTotalAddressesSkipped = 0;
            string cAddressesSkipped = "";

            try
            {
                // Dim nsmgr As XmlNamespaceManager = New XmlNamespaceManager(oBodyXML.OwnerDocument.NameTable)
                // Dim cNsURI As String = oBodyXML.NamespaceURI
                // nsmgr.AddNamespace("ews", cNsURI)
                // ' sResponse = oSoapElmt.SelectSingleNode("ews:" & sActionName & "Result", nsmgr).InnerText

                string XmlString = oBodyXML.OuterXml;
                XmlString = XmlString.Replace("xmlns=\"http://www.eonic.co.uk/ewcommon/Services\"", "");

                oXml.LoadXml(XmlString);

                oXml.DocumentElement.SetAttribute("subjectLine", SubjectLine);

                oXml.DocumentElement.SetAttribute("sessionReferrer", goRequest.Headers["SessionReferer"]);
                oXml.DocumentElement.SetAttribute("websiteURL", goRequest.ServerVariables["HTTP_HOST"]);

                XmlElement oAttIdsElmt = (XmlElement)oXml.DocumentElement.SelectSingleNode("AttachmentIds");
                if (oAttIdsElmt != null)
                {
                    AddAttachmentsByIds(ref oAttIdsElmt, oAttIdsElmt.GetAttribute("ids"), oAttIdsElmt.GetAttribute("xpath"));
                }
                // added to test plain text emails - remove on build
                // xsltPath = "/xsl/Email/vMessagingSMS.xsl"

                // Load the XSL from the passed filename string styleFileName
                cProcessInfo = xsltPath;

                if (string.IsNullOrEmpty(xsltPath))
                {
                    styleFile = goServer.MapPath(goConfig["ProjectPath"] + "/xsl/emailer.xsl");
                }
                else
                {
                    styleFile = goServer.MapPath(xsltPath);
                }
                if (HasLanguage & oXml.DocumentElement != null)
                {
                    oXml.DocumentElement.SetAttribute("translang", Language);
                }

                // We run the transformation twice
                // Once for HTML, and once for Plain Text
                string messageHtml = "";
                string messagePlainText = "";
                oTransform.XSLFile = styleFile;
                oTransform.Compiled = false;

                // Transform for HTML
                oXml.DocumentElement.SetAttribute("output", "html");
                oTransform.Process(oXml, ref sWriter);
                if (oTransform.HasError)
                {
                    throw new Exception("There was an error transforming the email (Output: HTML).");
                }
                messageHtml = sWriter.ToString();
                sWriter.Close();
                // Call to Pre-Mailer to move css to inline style attributes.
                string hostUrl = goRequest.Url.Host;
                string urlScheme = "http://";
                if (goRequest.IsSecureConnection)
                {
                    urlScheme = "https://";
                }
                if (!hostUrl.StartsWith(urlScheme, StringComparison.OrdinalIgnoreCase))
                {
                    hostUrl = urlScheme + hostUrl;
                }
                var preMailerResult = PreMailer.Net.PreMailer.MoveCssInline(new Uri(hostUrl), messageHtml);
                messageHtml = preMailerResult.Html;


                // Transform for Plain Text
                sWriter = new StringWriter();
                oXml.DocumentElement.SetAttribute("output", "plaintext");
                oTransform.Process(oXml, ref sWriter);
                if (oTransform.HasError)
                    throw new Exception("There was an error transforming the email (Output: Plain Text).");
                messagePlainText = sWriter.ToString();

                sWriter.Close();
                sWriter = null;

                // is there's no HTML, set is as plain text
                int nHtmlPos = Strings.InStr(Strings.LCase(messagePlainText), "<html");
                if (nHtmlPos <= 0)
                {
                    mbIsBodyHtml = false;
                }

                // lets get the subjectline form the html title
                var oEmailXmlDoc = htmlToXmlDoc(messageHtml);
                if (oEmailXmlDoc != null)
                {
                    // override the subject line from the template.
                    if (oEmailXmlDoc.SelectSingleNode("html/head/title") != null)
                    {
                        XmlElement oElmt2 = (XmlElement)oEmailXmlDoc.SelectSingleNode("html/head/title");
                        if (!string.IsNullOrEmpty(oElmt2.InnerText))
                        {
                            SubjectLine = Strings.Trim(oElmt2.InnerText);
                        }
                    }
                }


                // Check that we have a valid from address
                if (!Tools.Text.IsEmail(fromEmail))
                {
                    fromEmail = goConfig["SiteAdminEmail"];
                    fromName = goConfig["SiteAdminName"];
                }
                if (!Tools.Text.IsEmail(fromEmail))
                {
                    fromEmail = "webmaster@genericcms.com";
                }
                if (string.IsNullOrEmpty(fromName))
                    fromName = "Webmaster";

                var adFrom = new MailAddress(fromEmail, fromName);
                var oMailn = new MailMessage();
                oMailn.From = adFrom;
                oMailn.BodyEncoding = System.Text.Encoding.GetEncoding("utf-8");
                oMailn.IsBodyHtml = mbIsBodyHtml;


                // Implement the site sender
                if (goConfig["DisableServerSenderEmail"] != "true" & goConfig["DisableServerSenderEmail"] != "on")
                {
                    string serverSenderEmail = goConfig["ServerSenderEmail"] + "";
                    string serverSenderEmailName = goConfig["ServerSenderEmailName"] + "";
                    if (!Tools.Text.IsEmail(serverSenderEmail))
                    {
                        serverSenderEmail = "emailsender@protean.site";
                    }
                    if (string.IsNullOrEmpty(serverSenderEmailName))
                    {
                        serverSenderEmailName = "ProteanCMS Email Sender";
                    }

                    var mailSender = new MailAddress(serverSenderEmail, serverSenderEmailName);

                    if (Strings.LCase(goConfig["overrideFromEmail"]) == "on")
                    {
                        oMailn.From = mailSender;
                    }
                    // Don't add the sender if it's the same address as the from
                    else if (!Equals(mailSender, adFrom))
                    {
                        if (Strings.LCase(goConfig["EnableReplyTo"]) == "on")
                        {
                            oMailn.ReplyToList.Add(adFrom);
                            oMailn.From = mailSender;
                        }
                        else
                        {
                            oMailn.Sender = mailSender;
                        }

                    }
                }

                // All throught the process check if the e-mail address is actually valid.
                string cEmailOverride;
                cEmailOverride = goConfig["EmailOverride"];
                if (!string.IsNullOrEmpty(cEmailOverride))
                {
                    // If override email has been specified in the global config then send all mails to the override e-amil address
                    // Useful for testing.
                    nTotalAddressesAttempted = 1;
                    if (Tools.Text.IsEmail(cEmailOverride))
                    {
                        oMailn.To.Add(new MailAddress(goConfig["EmailOverride"], recipientName));
                    }
                    else
                    {
                        failureMessage += "";
                        nTotalAddressesSkipped = 1;
                        cAddressesSkipped = cEmailOverride;
                    }
                }
                else
                {

                    if (string.IsNullOrEmpty(recipientEmail))
                    {
                        recipientEmail = goConfig["SiteAdminEmail"];
                    }

                    // using multiple addresses here
                    if (recipientEmail.Contains(cSeperator))
                    {
                        string[] oTos = Strings.Split(recipientEmail, cSeperator);
                        int i;
                        var loopTo = oTos.Length - 1;
                        for (i = 0; i <= loopTo; i++)
                        {
                            nTotalAddressesAttempted += 1;
                            if (Tools.Text.IsEmail(oTos[i]))
                            {
                                oMailn.To.Add(oTos[i]);
                            }
                            else
                            {
                                nTotalAddressesSkipped += 1;
                                cAddressesSkipped += ", " + oTos[i];
                            }
                        }
                    }
                    else
                    {
                        nTotalAddressesAttempted += 1;
                        if (Tools.Text.IsEmail(recipientEmail))
                        {
                            oMailn.To.Add(new MailAddress(recipientEmail, recipientName));
                        }
                        else
                        {
                            nTotalAddressesSkipped += 1;
                            cAddressesSkipped += ", " + recipientEmail;
                        }
                    }
                    // cc
                    if (ccRecipient != null)
                    {
                        if (ccRecipient.Contains(cSeperator))
                        {
                            string[] oTos = Strings.Split(ccRecipient, cSeperator);
                            int i;

                            var loopTo1 = oTos.Length - 1;
                            for (i = 0; i <= loopTo1; i++)
                            {
                                nTotalAddressesAttempted += 1;
                                if (Tools.Text.IsEmail(oTos[i]))
                                {
                                    oMailn.CC.Add(oTos[i]);
                                }
                                else
                                {
                                    nTotalAddressesSkipped += 1;
                                    cAddressesSkipped += ", " + oTos[i];
                                }
                            }
                        }
                        else if (!string.IsNullOrEmpty(ccRecipient))
                        {
                            nTotalAddressesAttempted += 1;
                            if (Tools.Text.IsEmail(ccRecipient))
                            {
                                oMailn.CC.Add(ccRecipient);
                            }
                            else
                            {
                                nTotalAddressesSkipped += 1;
                                cAddressesSkipped += ", " + ccRecipient;
                            }
                        }
                    }
                    // bcc
                    if (bccRecipient != null)
                    {
                        if (bccRecipient.Contains(cSeperator))
                        {
                            string[] oTos = Strings.Split(bccRecipient, cSeperator);
                            int i;
                            var loopTo2 = oTos.Length - 1;
                            for (i = 0; i <= loopTo2; i++)
                            {
                                nTotalAddressesAttempted += 1;
                                if (Tools.Text.IsEmail(oTos[i]))
                                {
                                    oMailn.Bcc.Add(oTos[i]);
                                }
                                else
                                {
                                    nTotalAddressesSkipped += 1;
                                    cAddressesSkipped += ", " + oTos[i];
                                }

                            }
                        }
                        else if (!string.IsNullOrEmpty(bccRecipient))
                        {
                            nTotalAddressesAttempted += 1;
                            if (Tools.Text.IsEmail(bccRecipient))
                            {
                                oMailn.Bcc.Add(bccRecipient);
                            }
                            else
                            {
                                nTotalAddressesSkipped += 1;
                                cAddressesSkipped += ", " + bccRecipient;
                            }
                        }
                    }
                }

                // Check if we need to send the e-mail
                if (string.IsNullOrEmpty(goConfig["MailServer"]))
                {
                    return "Mailserver Not Specified.";
                }
                else if (nTotalAddressesAttempted == nTotalAddressesSkipped)
                {
                    // If all the e-mails failed validation then don't bother sending them
                    switch (nTotalAddressesAttempted)
                    {
                        case 0:
                            {
                                return failureMessage + ": No e-mail addresses were provided";
                            }
                        case 1:
                            {
                                return failureMessage + ": E-mail address provided is invalid";
                            }

                        default:
                            {
                                return failureMessage + ": E-mail addresses provided are invalid";
                            }
                    }
                }
                else
                {

                    // Override the HTML setting if BodyHtml is explicitly being set
                    if (oMailn.IsBodyHtml == false | mbEncrypt)
                    {

                        if (string.IsNullOrEmpty(messagePlainText))
                        {
                            messagePlainText = messageHtml;
                        }
                        // messageHtml = ""

                    }

                    cProcessInfo = "Encrypt the Email";
                    if (mbEncrypt == true)
                    {
                        // encrypt the email using GNU

                        var oGnuPg = new GnuPG.GnuPGWrapper();

                        oGnuPg.homedirectory = msGnuDirectory;
                        oGnuPg.originator = msGnuOriginator;
                        oGnuPg.passphrase = msGnuPassphrase;
                        oGnuPg.recipient = recipientEmail;
                        oGnuPg.command = GnuPG.Commands.SignAndEncrypt;
                        oGnuPg.ExecuteCommand(messagePlainText, messagePlainText);
                    }

                    // Transform and output to page
                    cProcessInfo = "create ewMailer Component";

                    // lets just tidy up the subject line
                    if (string.IsNullOrEmpty(SubjectLine))
                        SubjectLine = "";
                    SubjectLine = Regex.Replace(SubjectLine, @"[\r\f\n\t\f]", "").Trim();
                    cProcessInfo = "Subject:" + SubjectLine;
                    oMailn.Subject = SubjectLine;

                    // Set the message body
                    oMailn.IsBodyHtml = mbIsBodyHtml;

                    // mbIsBodyHtml = False ' added for testing. remove for deployment.

                    if (mbIsBodyHtml == false)
                    {
                        // Because we're processing the same XSLs for both plain text and html we can't tell
                        // the xsl:output element to be conditional and do things like output text and omit docTypes
                        // so a little manual tidy up is needed
                        messagePlainText = Regex.Replace(messagePlainText, "<!DOCTYPE[^>]+?>", "", RegexOptions.IgnoreCase);
                        oMailn.Body = messagePlainText.Replace("%0D%0A", Environment.NewLine);
                        oMailn.Headers.Set("Content-Type", "text/plain");
                        // moCtx.Response.ContentType = "text/plain"

                        if (Strings.InStr(Strings.LCase(messageHtml), "<html") > 0)
                        {
                            var htmlView = AlternateView.CreateAlternateViewFromString(messageHtml, new System.Net.Mime.ContentType("text/html; charset=UTF-8"));
                            oMailn.AlternateViews.Add(htmlView);
                        }
                    }

                    else
                    {
                        oMailn.Body = messagePlainText.Replace("%0D%0A", Environment.NewLine);
                        var htmlView = AlternateView.CreateAlternateViewFromString(messageHtml, new System.Net.Mime.ContentType("text/html; charset=UTF-8"));
                        oMailn.AlternateViews.Add(htmlView);

                    }

                    // ok lets add an attachment if we need too
                    if (Attachments != null)
                    {
                        foreach (Attachment oAtt in Attachments)
                            // reset to begining if allready used
                            oMailn.Attachments.Add(oAtt);

                    }

                    var oSmtpn = new SmtpClient();

                    cProcessInfo = "Mailserver Failed:" + goConfig["MailServer"];


                    // Work out whether to send this to a queued server
                    if (!string.IsNullOrEmpty(cPickupHost) & !string.IsNullOrEmpty(cPickupLocation))
                    {
                        // Send queued
                        if (SendQueuedMail(oMailn, cPickupHost, cPickupLocation) == "Error")
                        {
                            return failureMessage + " - Queuing message failed";
                        }
                    }
                    else
                    {
                        // Send direct
                        try
                        {
                            oSmtpn.Host = goConfig["MailServer"];
                            if (!string.IsNullOrEmpty(goConfig["MailServerPort"]))
                            {
                                oSmtpn.Port = Conversions.ToInteger(goConfig["MailServerPort"]);
                            }
                            if (!string.IsNullOrEmpty(goConfig["MailServerUsername"]))
                            {
                                oSmtpn.UseDefaultCredentials = false;
                                oSmtpn.Credentials = new System.Net.NetworkCredential(goConfig["MailServerUsername"], goConfig["MailServerPassword"].Replace("&lt;", "<").Replace("&gt;", "<"), goConfig["MailServerUsernameDomain"]);
                                // oSmtpn.Credentials = New System.Net.NetworkCredential(goConfig("MailServerUsername"), goConfig("MailServerPassword"))
                            }
                            if (Strings.LCase(goConfig["MailServerSSL"]) == "on")
                            {
                                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                                oSmtpn.EnableSsl = true;
                                oSmtpn.DeliveryMethod = SmtpDeliveryMethod.Network;
                            }
                            if (Strings.LCase(goConfig["MailServerSSL"]) == "off")
                            {
                                oSmtpn.EnableSsl = false;
                            }
                            if (sendAsync)
                            {
                                // Set the method that is called back when the send operation ends. 
                                oSmtpn.SendCompleted += SendCompletedCallback;
                                // The userState can be any object that allows your callback  
                                // method to identify this send operation. 
                                // For this example, the userToken is a string constant. 
                                string userState = SubjectLine;
                                oSmtpn.SendAsync(oMailn, userState);
                            }

                            else
                            {
                                try
                                {
                                    oSmtpn.Send(oMailn);
                                }
                                catch (Exception ex)
                                {
                                    if (string.IsNullOrEmpty(goConfig["MailServer2"]))
                                    {
                                        if (gbDebug)
                                        {
                                            returnException(ref msException, mcModuleName, "emailer", ex, "", cProcessInfo, gbDebug);
                                            return "ex: " + ex.ToString();
                                        }
                                        else
                                        {
                                            return failureMessage + " - Error1: " + ex.Message + " - " + cProcessInfo + " - " + ex.StackTrace;
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            oSmtpn.Host = goConfig["MailServer2"];
                                            oSmtpn.Send(oMailn);
                                        }
                                        catch (Exception ex3)
                                        {
                                            if (gbDebug)
                                            {
                                                returnException(ref msException, mcModuleName, "emailer", ex3, "", cProcessInfo, gbDebug);
                                                return "ex3: " + ex3.ToString();
                                            }
                                            else
                                            {
                                                return failureMessage + " - Error1: " + ex3.Message + " - " + cProcessInfo + " - " + ex.StackTrace;
                                            }

                                        }
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                oSmtpn.Host.Insert(0, goConfig["MailServer"]);
                                try
                                {
                                    oSmtpn.Send(oMailn);
                                }
                                catch (Exception)
                                {
                                    if (!string.IsNullOrEmpty(goConfig["MailServer2"]))
                                    {
                                        oSmtpn.Host.Insert(0, goConfig["MailServer2"]);
                                        oSmtpn.Send(oMailn);
                                    }
                                }
                            }
                            catch (Exception ex2)
                            {
                                if (gbDebug)
                                {
                                    returnException(ref msException, mcModuleName, "emailer", ex2, "", cProcessInfo, gbDebug);
                                    return "ex2: " + ex2.ToString();
                                }
                                else
                                {
                                    return failureMessage + " - Error1: " + ex2.Message + " - " + cProcessInfo + " - " + ex.StackTrace;
                                }
                            }
                        }
                    }

                    if (Strings.LCase(goConfig["LogEmail"]) == "on")
                    {
                        try
                        {
                            string cActivityDetail = "";
                            try
                            {
                                XmlElement oBodyElmt = (XmlElement)oEmailXmlDoc.SelectSingleNode("html/body");
                                if (!string.IsNullOrEmpty(oBodyElmt.InnerText))
                                {
                                    cActivityDetail = oBodyElmt.InnerText;
                                }
                                else
                                {
                                    cActivityDetail = oMailn.Body;
                                }
                            }
                            catch (Exception)
                            {
                                cActivityDetail = oMailn.Body;
                            }
                            // Trim out multiple whitespace - saves space
                            var oRE = new Regex(@"(\s)\s+"); // Pattern looks for a whitespace character followed by one or more whitespace chars
                            cActivityDetail = Strings.Trim(oRE.Replace(cActivityDetail, " ")); // Replaces the whole lot with a space

                            int mnUserId = 0;
                            if (goSession != null)
                                mnUserId = Conversions.ToInteger(goSession["mnUserId"]);
                            if (odbHelper is null)
                            {
                                string cCon = "Data Source=" + goConfig["DatabaseServer"] + "; Initial Catalog=" + goConfig["DatabaseName"] + ";";
                                if (!string.IsNullOrEmpty(goConfig["DatabasePassword"]))
                                {
                                    cCon += "user id=" + goConfig["DatabaseUsername"] + "; password=" + goConfig["DatabasePassword"];
                                }
                                else
                                {
                                    cCon += goConfig["DatabaseAuth"] + "; ";
                                }
                                odbHelper = new Cms.dbHelper(cCon, mnUserId);
                            }

                            string SessionId = null;
                            if (goSession != null)
                            {
                                SessionId = goSession.SessionID;
                            }

                            if (odbHelper.checkTableColumnExists("tblEmailActivityLog", "cActivityXml") & Strings.LCase(goConfig["LogEmailXml"]) != "off")
                            {
                                string activitySchema = "Default";
                                if (string.IsNullOrEmpty(oBodyXML.GetAttribute("id")))
                                {
                                    activitySchema = oBodyXML.Name;
                                }
                                else
                                {
                                    activitySchema = oBodyXML.GetAttribute("id");
                                }
                                long logId = odbHelper.emailActivity((Int16)mnUserId, cActivityDetail, oMailn.To.ToString(), oMailn.From.ToString(), oXml.OuterXml);
                                odbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Email, mnUserId, SessionId, DateTime.Now, (Int16)logId, 0, activitySchema);
                            }

                            else
                            {
                                odbHelper.emailActivity((Int16)mnUserId, cActivityDetail, oMailn.To.ToString(), oMailn.From.ToString());
                                odbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Email, mnUserId, SessionId, DateTime.Now, 0, 0, "");
                            }
                        }

                        catch (Exception)
                        {
                            // Dont do anything
                        }
                    }

                    // Check if any e-mails were skipped.
                    if (nTotalAddressesSkipped > 0)
                    {
                        successMessage += " (" + nTotalAddressesSkipped + " out of " + nTotalAddressesAttempted + " e-mails were not sent, because the e-mail addresses were invalid. ";
                        if (gbDebug)
                            successMessage += "Skipped: " + cAddressesSkipped.TrimStart(',');
                        successMessage += ")";
                    }
                    if (!sendAsync)
                    {
                        oMailn.Dispose();
                    }

                    // handle opt-in behaviour
                    if (oXml.SelectSingleNode("descendant-or-self::optIn[node()='true']") != null)
                    {

                        
                        System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                        string sMessagingProvider = "";
                        if (moMailConfig != null)
                        {
                            sMessagingProvider = moMailConfig["MessagingProvider"];
                            var myWeb = new Cms(moCtx);
                            Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                            IMessagingProvider moMessaging = RetProv.Get(ref myWeb, sMessagingProvider);
                            try
                            {

                                string email = oXml.SelectSingleNode("descendant-or-self::Email").InnerText;
                                string name = oXml.SelectSingleNode("descendant-or-self::Name").InnerText;
                                var values = new System.Collections.Generic.Dictionary<string, string>();
                                foreach (XmlElement oElmt in oXml.SelectNodes("/*/*"))
                                {
                                    if (!(oElmt.Name == "Name" | oElmt.Name == "Email" | oElmt.Name == "ListId"))
                                    {
                                        values.Add(oElmt.Name, oElmt.InnerText);
                                    }
                                }
                                moMessaging.Activities.AddToList(moMailConfig["OptInList"], name, email, values);
                            }

                            catch (Exception ex)
                            {
                                cProcessInfo = ex.StackTrace;
                            }
                        }

                    }

                    return successMessage;

                }
            }

            catch (Exception ex)
            {
                if (gbDebug)
                {
                    returnException(ref msException, mcModuleName, "emailer", ex, "", cProcessInfo, gbDebug);
                    return ex.ToString();
                }
                else
                {
                    return failureMessage + " - Error2: " + ex.Message + " - " + cProcessInfo + " - " + ex.StackTrace;
                }
            }

        }


        private static void SendCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation. 
            string token = Conversions.ToString(e.UserState);

            if (e.Cancelled)
            {
                // Console.WriteLine("[{0}] Send canceled.", token)
            }
            if (e.Error != null)
            {
                returnException(ref msException, "messaging", "SendCompletedCallback", e.Error, "", "[{0}] Send Error.", gbDebug);
            }
            // Console.WriteLine("[{0}] {1}", token, e.Error.ToString())
            else
            {
                //sender.Dispose();
                // Console.WriteLine("Message sent.")
            }
            mailSent = true;
        }


        // Public Function emailerPlainText( _
        // ByVal oBodyXML As XmlElement, _
        // ByVal xsltPath As String, _
        // ByVal fromName As String, _
        // ByVal fromEmail As String, _
        // ByVal recipientEmail As String, _
        // ByVal SubjectLine As String, _
        // Optional ByVal successMessage As String = "Message Sent", _
        // Optional ByVal failureMessage As String = "Message Failed", _
        // Optional ByVal recipientName As String = "", _
        // Optional ByVal ccRecipient As String = "", _
        // Optional ByVal bccRecipient As String = "", _
        // Optional ByVal cSeperator As String = "", _
        // Optional ByRef odbHelper As Protean.Cms.dbHelper = Nothing, _
        // Optional ByVal cPickupHost As String = "", _
        // Optional ByVal cPickupLocation As String = "" _
        // ) As Object

        // Dim cProcessInfo As String = "emailerPlainText"

        // Try

        // mbIsBodyHtml = False
        // Dim cMessage As Object = emailerPlainText(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, successMessage, failureMessage, recipientName, ccRecipient, bccRecipient, cSeperator, odbHelper, cPickupHost, cPickupLocation)

        // mbIsBodyHtml = True

        // Return cMessage


        // Catch ex As Exception
        // If gbDebug Then
        // returnException(msException, mcModuleName, "emailer", ex, "", cProcessInfo, gbDebug)
        // Return ex.ToString
        // Else
        // Return failureMessage & " - Error: " & ex.Message & " - " & cProcessInfo
        // End If
        // End Try


        // End Function

        public object emailerMultiUsers(XmlElement oBodyXML, string xsltPath, string fromName, string fromEmail, string recipientIds, string SubjectLine, string successMessage = "Message Sent", string failureMessage = "Message Failed", string cPickupHost = "", string cPickupLocation = "")
        {

            // PerfMon.Log("Messaging", "emailerMultiUsers")

            string cProcessInfo = "emailerMultiUsers Initialise";
            string cRecipientName = "";
            string cRecipientEmail = "";

            string cResult = "";

            try
            {

                // First get Host and Location from Web.config, only if not passed in already
                if (string.IsNullOrEmpty(cPickupHost))
                {
                    cPickupHost = goConfig["SMPTHost"]; // Typo - included for backwards compatibility
                    if (string.IsNullOrEmpty(cPickupHost))
                        cPickupHost = goConfig["SMTPHost"];
                    if (string.IsNullOrEmpty(cPickupHost))
                        cPickupHost = goConfig["PickupHost"];
                }
                if (string.IsNullOrEmpty(cPickupLocation))
                {
                    cPickupLocation = goConfig["SMPTLocation"]; // Typo - included for backwards compatibility
                    if (string.IsNullOrEmpty(cPickupLocation))
                        cPickupLocation = goConfig["SMTPLocation"];
                    if (string.IsNullOrEmpty(cPickupLocation))
                        cPickupLocation = goConfig["PickupLocation"];
                }

                string[] recipientIdsSplit = Strings.Split(recipientIds, ",");

                // Are these both needed?
                if (recipientIdsSplit != null)
                {
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(recipientIdsSplit.GetValue(0), "", false))) // Empty Check
                    {


                        cProcessInfo = "IDs Detected, Initialising DBHelper";

                        // Get a Database Helper
                        Protean.Cms.dbHelper odbHelper = default;
                        int mnUserId = 0;

                        if (goSession != null)
                            mnUserId = Conversions.ToInteger(goSession["mnUserId"]);
                        if (odbHelper is null)
                        {
                            string cCon = "Data Source=" + goConfig["DatabaseServer"] + "; Initial Catalog=" + goConfig["DatabaseName"] + ";";
                            if (!string.IsNullOrEmpty(goConfig["DatabasePassword"]))
                            {
                                cCon += "user id=" + goConfig["DatabaseUsername"] + "; password=" + goConfig["DatabasePassword"];
                            }
                            else
                            {
                                cCon += goConfig["DatabaseAuth"] + "; ";
                            }
                            odbHelper = new Cms.dbHelper(cCon, mnUserId);
                            cProcessInfo = "DBHelper Started";
                        }

                        int iCounter = 0;
                        int iSuccess = 0;
                        // Results populating by given Year(s)
                        foreach (var cRecipientId in recipientIdsSplit)
                        {

                            iCounter = iCounter + 1;
                            cProcessInfo = "Processing ID to Email for " + cRecipientId.ToString();

                            // Dim oDr As SqlClient.SqlDataReader
                            string cSQL = "";

                            cSQL = "SELECT * FROM tblDirectory INNER JOIN tblContentStructure ON tblDirectory.cDirName = tblContentStructure.cStructForiegnRef WHERE tblContentStructure.cStructName = 'Student_" + cRecipientId + "'";

                            using (SqlDataReader oDr = odbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                            {

                                while (oDr.Read())
                                {
                                    var oResultsDoc = new XmlDocument();
                                    var oResults = oResultsDoc.CreateElement("Results");
                                    oResults.InnerXml = Conversions.ToString(oDr["cDirXml"]);

                                    if (oResults != null)
                                    {
                                        cRecipientName = oResults.SelectSingleNode("User/FirstName").InnerText.ToString() + " " + oResults.SelectSingleNode("User/LastName").InnerText.ToString();
                                        cRecipientEmail = oResults.SelectSingleNode("User/Email").InnerText.ToString();
                                    }
                                }
                            }

                            string cEmailResult = "";

                            if (!string.IsNullOrEmpty(cRecipientEmail))
                            {
                                cProcessInfo = "Sending Email for #" + iCounter.ToString();
                                Cms.dbHelper odbHelperElmt = null;
                                cEmailResult = emailer(oBodyXML, xsltPath, fromName, fromEmail, cRecipientEmail, SubjectLine, ref odbHelperElmt, recipientName: cRecipientName, cPickupHost: cPickupHost, cPickupLocation: cPickupLocation).ToString();
                            }
                            else
                            {
                                cEmailResult = failureMessage + ": E-mail address provided is invalid";
                            }

                            // Append to Final Message
                            cResult = cResult + "<email id=" + '"' + cRecipientId.ToString() + '"' + ">" + cEmailResult + "</email>";

                            if (cEmailResult.Equals(successMessage))
                            {
                                iSuccess = iSuccess + 1;
                            }

                            // reset
                            cRecipientName = "";
                            cRecipientEmail = "";
                            cSQL = "";
                        }
                        int iFail = iCounter - iSuccess;
                        cResult = "<emails count=" + '"' + iCounter + '"' + " success=" + '"' + iSuccess + '"' + " fail=" + '"' + iFail + '"' + " >" + cResult + "</emails>";
                    }
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "emailerMultiUsers", ex, "", cProcessInfo);
                return ex.ToString();
            }

            if (string.IsNullOrEmpty(cResult))
            {
                cResult = "No Emails Processed";
            }
            return cResult;

        }
        public object emailerWithXmlAttachment(XmlElement oBodyXML, string xsltPath, string fromName, string fromEmail, string recipientEmail, string SubjectLine, string XSLPath, string XSLType, string emailAttachName, ref Protean.Cms.dbHelper odbHelper, string successMessage = "Message Sent", string failureMessage = "Message Failed", string recipientName = "", string ccRecipient = "", string bccRecipient = "", string cSeperator = "", string cPickupHost = "", string cPickupLocation = "")
        {

            // PerfMon.Log("Messaging", "emailerWithXmlAttachment")
            string cProcessInfo = "emailerWithXmlAttachment Initialise";

            var oXml = new XmlDocument();
            string cResults = "";

            // 22-Apr-09 Added to handle specific email attachment names
            string attachName = emailAttachName;
            if (string.IsNullOrEmpty(attachName))
            {
                attachName = "attached";
            }
            // 

            try
            {
                // PerfMon.Log("Messaging", "emailerWithXmlAttachment - Get Xml")
                oXml.LoadXml(oBodyXML.OuterXml);

                if (oXml != null)
                {
                    if (XSLType.ToLower() == "xml")
                    {

                        // PerfMon.Log("Messaging", "emailerWithXmlAttachment - Create Save Path")

                        string cSourceXmlPath = goConfig["XmlAttachmentPath"];
                        string cXmlPath = "";

                        if (cSourceXmlPath is null)
                        {
                            cXmlPath = goServer.MapPath("/") + @"..\imports\" + attachName + ".xml";
                        }
                        else
                        {
                            cXmlPath = goServer.MapPath("/") + cSourceXmlPath + attachName + ".xml";
                        }

                        // ''''Dim cXmlPath As String = goServer.MapPath("") & "..\..\imports\" & attachName & ".xml"

                        // Dim cXmlPath As String = goServer.MapPath("../") & "..\imports\Email.xml"

                        if (!string.IsNullOrEmpty(cXmlPath))
                        {
                            // PerfMon.Log("Messaging", "emailerWithXmlAttachment - Save Xml File")

                            if (!string.IsNullOrEmpty(XSLPath))
                            {
                                TextWriter sWriter = new StringWriter();
                                string sMessage;
                                var oTransform = new Protean.XmlHelper.Transform();

                                cProcessInfo = xsltPath;

                                oTransform.XSLFile = goServer.MapPath(XSLPath);
                                oTransform.Compiled = false;
                                oTransform.Process(oXml, ref sWriter);
                                if (oTransform.HasError)
                                    throw new Exception("There was an error transforming the email.");

                                sMessage = sWriter.ToString();

                                sWriter.Close();
                                sWriter = null;

                                oXml = htmlToXmlDoc(sMessage);
                            }

                            var oXmlRemoveHeader = oXml.FirstChild;
                            oXml.RemoveChild(oXmlRemoveHeader);

                            XmlDeclaration oXmlDec;
                            oXmlDec = oXml.CreateXmlDeclaration("1.0", null, null);
                            oXmlDec.Encoding = "UTF-8";

                            var oXmlRoot = oXml.DocumentElement;
                            oXml.InsertBefore(oXmlDec, oXmlRoot);

                            oXml.Save(cXmlPath);

                            // PerfMon.Log("Messaging", "emailerWithXmlAttachment - Add Attachment")
                            var oAtt = new Attachment(cXmlPath);
                            if (Attachments is null)
                            {
                                Attachments = new Collection();
                            }
                            Attachments.Add(oAtt);
                            // PerfMon.Log("Messaging", "emailerWithXmlAttachment - Delete Xml File")
                            Protean.fsHelper fsh = new fsHelper();
                            fsh.DeleteFile(cXmlPath);
                        }

                        // PerfMon.Log("Messaging", "emailerWithXmlAttachment - Call Emailer")
                        cResults = emailer(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, ref odbHelper, successMessage, failureMessage, recipientName, ccRecipient, bccRecipient, cSeperator, cPickupHost, cPickupLocation).ToString();
                    }
                }
                else
                {
                    cResults = "Mailform failed to load";
                }


                return cResults;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "emailerWithXmlAttachment", ex, "", cProcessInfo);
                return "Error: " + ex.ToString();
            }

        }
        public object emailerPostToHTMLForm(XmlElement oFormXML)
        {
            // Name?!!?

            string cProcessInfo = "PostToHTMLForm Initialise";
            string cResponse = "";

            try
            {
                if (oFormXML.Attributes.GetNamedItem("action") != null)
                {

                    string cPostTo = oFormXML.Attributes.GetNamedItem("action").Value.ToString();
                    string cPostString = "";

                    foreach (XmlNode oPostNode in oFormXML.GetElementsByTagName("Post"))
                    {
                        if (oPostNode.Attributes.GetNamedItem("name").Value != null)
                        {
                            cPostString = cPostString + System.Web.HttpUtility.UrlEncode("" + oPostNode.Attributes.GetNamedItem("name").Value.ToString()) + "=" + System.Web.HttpUtility.UrlEncode("" + oPostNode.InnerText) + "&";
                        }
                        // what happens to IDs?
                    }

                    if (cPostString.Length > 1)
                    {
                        cPostString = cPostString.Substring(0, cPostString.Length - 1);
                        // cPostString = System.Web.HttpUtility.UrlEncode(cPostString)
                    }

                    var httpRequest = new Tools.Http.WebRequest("application/x-www-form-urlencoded");
                    httpRequest.IncludeResponse = false;
                    cResponse = httpRequest.Send(cPostTo, cPostString);
                }
                else
                {
                    cResponse = "Error: No Post To Path Defined";
                }

                cResponse = "Message Sent";
                return cResponse;
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "PostToHTMLForm", ex, "", cProcessInfo);
                return "Error: " + ex.ToString();
            }
        }


        public string SendQueuedMail(MailMessage oMailn, string cHost, string cPickupLocation)
        {
            // PerfMon.Log("Messaging", "SendQueuedMail")
            try
            {
                if (oMailn is null)
                    return "No Email Supplied";
                var oSmtpn = new SmtpClient();
                oSmtpn.Host = cHost; // "127.0.0.1"
                oSmtpn.PickupDirectoryLocation = cPickupLocation; // "C:\Inetpub\mailroot\Pickup"
                oSmtpn.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                // =#=#=#=#=#=#=#=#=#=#=#=#=#==#=#=#=
                // Exit Function
                // =#=#=#=#=#=#=#=#=#=#=#=#=#==#=#=#=
                oSmtpn.Send(oMailn);
                return "Sent";
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "SendQueuedMail", ex, "", "", gbDebug);
                return "Error";
            }
        }

        public bool SendMailToList_Queued(int nPageId, string cEmailXSL, string cGroups, string cFromEmail, string cFromName, string cSubject)
        {
            // PerfMon.Log("Messaging", "SendMailToList_Queued")

            string cProcessInfo = "";

            try
            {
                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                var oAddressDic = GetGroupEmails(cGroups);
                // max number of bcc


                var oWeb = new Cms();
                oWeb.InitializeVariables();
                oWeb.Open();
                oWeb.mnPageId = nPageId;
                oWeb.mbAdminMode = false;

                oWeb.mcEwSiteXsl = cEmailXSL;
                // get body
                oWeb.mnMailMenuId = Convert.ToInt64(moMailConfig["RootPageId"]);
                string oEmailBody = oWeb.ReturnPageHTML(oWeb.mnPageId);

                var i2 = default(int);

                MailMessage oEmail = null;

                cFromEmail = cFromEmail.Trim();

                if (Tools.Text.IsEmail(cFromEmail))
                {
                    foreach (string cRepientMail in oAddressDic.Keys)
                    {
                        // create the message
                        if (oEmail is null)
                        {
                            oEmail = new MailMessage();
                            oEmail.IsBodyHtml = true;
                            cProcessInfo = "Sending from: " + cFromEmail;
                            oEmail.From = new MailAddress(cFromEmail, cFromName);
                            oEmail.Body = oEmailBody;
                            oEmail.Subject = cSubject;
                        }
                        // if we are not at the bcc limit then we add the addres
                        if (i2 < Conversions.ToInteger(moMailConfig["BCCLimit"]))
                        {
                            if (Tools.Text.IsEmail(cRepientMail.Trim()))
                            {
                                cProcessInfo = "Sending to: " + cRepientMail.Trim();
                                oEmail.Bcc.Add(new MailAddress(cRepientMail.Trim()));
                                i2 += 1;
                            }
                        }
                        else
                        {
                            // otherwise we send it
                            cProcessInfo = "Sending queued mail";
                            SendQueuedMail(oEmail, moMailConfig["PickupHost"], moMailConfig["PickupLocation"]);
                            // and reset the counter
                            i2 = 0;
                            oEmail = null;
                        }
                    }
                    // try a send after in case we havent reached the last send
                    if (i2 < Conversions.ToInteger(moMailConfig["BCCLimit"]))
                    {
                        cProcessInfo = "Sending queued mail (last)";
                        SendQueuedMail(oEmail, moMailConfig["PickupHost"], moMailConfig["PickupLocation"]);
                        // and reset the counter
                        i2 = 0;
                        oEmail = null;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "SendMailToList_Queued", ex, "", cProcessInfo, gbDebug);
                return false;
            }
        }

        public bool SendSingleMail_Queued(int nPageId, string cEmailXSL, string cRepientMail, string cFromEmail, string cFromName, string cSubject)
        {
            // PerfMon.Log("Messaging", "SendSingleMail_Queued")
            try
            {
                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");


                if (Tools.Text.IsEmail(cFromEmail.Trim()) & Tools.Text.IsEmail(cRepientMail.Trim()))
                {
                    Hashtable emailStructure;

                    emailStructure = SetEmailBodyAndSubject(nPageId, cEmailXSL, cRepientMail, cFromEmail, cFromName, cSubject);

                    MailMessage oEmail;

                    oEmail = new MailMessage();
                    oEmail.IsBodyHtml = true;
                    oEmail.From = new MailAddress(cFromEmail.Trim(), cFromName);
                    oEmail.Body = emailStructure["EmailBody"].ToString();

                    oEmail.To.Add(new MailAddress(cRepientMail.Trim()));
                    oEmail.Subject = emailStructure["Subject"].ToString();


                    // otherwise we send it
                    SendQueuedMail(oEmail, moMailConfig["PickupHost"], moMailConfig["PickupLocation"]);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "SendSingleMail_Queued", ex, "", "", gbDebug);
                return false;
            }
        }


        public bool SendSingleMail_Direct(int nPageId, string cEmailXSL, string cRepientMail, string cFromEmail, string cFromName, string cSubject)
        {
            // PerfMon.Log("Messaging", "SendSingleMail_Queued")
            try
            {
                if (Tools.Text.IsEmail(cFromEmail.Trim()) & Tools.Text.IsEmail(cRepientMail.Trim()))
                {
                    Hashtable emailStructure;

                    emailStructure = SetEmailBodyAndSubject(nPageId, cEmailXSL, cRepientMail, cFromEmail, cFromName, cSubject);

                    var oSmtpn = new SmtpClient();

                    oSmtpn.Host = goConfig["MailServer"];
                    if (!string.IsNullOrEmpty(goConfig["MailServerPort"]))
                    {
                        oSmtpn.Port = Conversions.ToInteger(goConfig["MailServerPort"]);
                    }

                    if (!string.IsNullOrEmpty(goConfig["MailServerUsername"]))
                    {
                        oSmtpn.UseDefaultCredentials = false;
                        oSmtpn.Credentials = new System.Net.NetworkCredential(goConfig["MailServerUsername"], goConfig["MailServerPassword"].Replace("&lt;", "<").Replace("&gt;", "<"), goConfig["MailServerUsernameDomain"]);
                    }
                    if (Strings.LCase(goConfig["MailServerSSL"]) == "on")
                    {
                        System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                        oSmtpn.EnableSsl = true;
                        oSmtpn.DeliveryMethod = SmtpDeliveryMethod.Network;
                    }

                    MailMessage oEmail;

                    oEmail = new MailMessage();
                    oEmail.IsBodyHtml = true;

                    if (Strings.LCase(goConfig["overrideFromEmail"]) == "on")
                    {
                        oEmail.From = new MailAddress(goConfig["ServerSenderEmail"], cFromName);
                    }
                    else
                    {
                        oEmail.From = new MailAddress(cFromEmail.Trim(), cFromName);
                    }

                    // PREMailer Code
                    string hostUrl = goRequest.Url.Host;
                    string urlScheme = "http://";
                    if (goRequest.IsSecureConnection)
                    {
                        urlScheme = "https://";
                    }
                    if (!hostUrl.StartsWith(urlScheme, StringComparison.OrdinalIgnoreCase))
                    {
                        hostUrl = urlScheme + hostUrl;
                    }
                    InlineResult preMailerResult = PreMailer.Net.PreMailer.MoveCssInline(Convert.ToString(new Uri(hostUrl)), Convert.ToBoolean(emailStructure["EmailBody"]));
                    string sEmailBody = preMailerResult.Html;


                    oEmail.Body = sEmailBody;
                    oEmail.To.Add(new MailAddress(cRepientMail.Trim()));
                    oEmail.Subject = Conversions.ToString(emailStructure["Subject"]);

                    oSmtpn.Send(oEmail);

                    // otherwise we send it
                    // SendQueuedMail(oEmail, moMailConfig("PickupHost"), moMailConfig("PickupLocation"))
                    return true;
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "SendSingleMail_Queued", ex, "", "", gbDebug);
                return false;
            }
        }

        public string SetInlineCss(string sEmailBody)
        {
            try
            {
                string hostUrl = goRequest.Url.Host;
                string urlScheme = "http://";
                if (goRequest.IsSecureConnection)
                {
                    urlScheme = "https://";
                }
                if (!hostUrl.StartsWith(urlScheme, StringComparison.OrdinalIgnoreCase))
                {
                    hostUrl = urlScheme + hostUrl;
                }
                var preMailerResult = PreMailer.Net.PreMailer.MoveCssInline(new Uri(hostUrl), sEmailBody);
                sEmailBody = preMailerResult.Html;
                return sEmailBody;
            }
            catch
            {
                return null;
            }


        }
        public Hashtable SetEmailBodyAndSubject(int nPageId, string cEmailXSL, string cRepientMail, string cFromEmail, string cFromName, string cSubject)
        {
            try
            {
                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");

                var oWeb = new Cms();
                oWeb.InitializeVariables();
                oWeb.Open();
                oWeb.mnPageId = nPageId;
                oWeb.mbAdminMode = false;

                oWeb.mcEwSiteXsl = cEmailXSL;
                // get body
                oWeb.mnMailMenuId = Convert.ToInt64(moMailConfig["RootPageId"]);
                string sEmailBody;
                var emailStructure = new Hashtable();


                sEmailBody = oWeb.ReturnPageHTML(oWeb.mnPageId);
                emailStructure.Add("EmailBody", sEmailBody);
                // Lets get the title and override the one provided
                var oXml = new XmlDocument();

                oXml = htmlToXmlDoc(sEmailBody);

                if (oXml != null)
                {
                    // override the subject line from the template.
                    if (oXml.SelectSingleNode("html/head/title") != null)
                    {
                        XmlElement oElmt2 = (XmlElement)oXml.SelectSingleNode("html/head/title");
                        if (!string.IsNullOrEmpty(oElmt2.InnerText))
                        {
                            cSubject = Strings.Trim(oElmt2.InnerText);
                            emailStructure.Add("Subject", cSubject);
                        }
                    }
                }
                oXml = null;

                return emailStructure;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public UserEmailDictionary GetGroupEmails(string groupIds)
        {
            // PerfMon.Log("Messaging", "GetGroupEmails")
            // Retrieves a list of email addresses from the groups
            // checks them against opted out adress
            // adds them to a master list and checks duplicates
            // outputs as a dictionary
            try
            {
                // dictionary object
                var oDic = new UserEmailDictionary();
                var oDBH = new Cms.dbHelper("Data Source=" + goConfig["DatabaseServer"] + "; " + "Initial Catalog=" + goConfig["DatabaseName"] + "; " + goConfig["DatabaseAuth"], 1);
                string cSQL = "SELECT nDirKey, cDirXml" + " FROM tblDirectory" + " WHERE (((SELECT TOP 1 tblDirectoryRelation.nDirChildId" + " FROM tblDirectoryRelation INNER JOIN" + " tblDirectory Groups ON tblDirectoryRelation.nDirParentId = Groups.nDirKey" + " WHERE (Groups.nDirKey IN (" + groupIds + ")) AND (tblDirectoryRelation.nDirChildId = tblDirectory.nDirKey)" + " GROUP BY tblDirectoryRelation.nDirChildId)) IS NOT NULL)";

                DataSet oDS = oDBH.GetDataSet(cSQL, "Users", "Addresses");
                // set the table ready to be xml
                if (oDS.Tables["Users"].Rows.Count > 0)
                {
                    oDS.Tables["Users"].Columns["nDirKey"].ColumnMapping = MappingType.Attribute;
                    oDS.Tables["Users"].Columns["cDirXml"].ColumnMapping = MappingType.Element;
                }
                // get the opt out addresses
                cSQL = "SELECT EmailAddress FROM tblOptOutAddresses";
                oDBH.addTableToDataSet(ref oDS, cSQL, "OptOut");
                // set the table ready to be xml
                if (oDS.Tables["OptOut"].Rows.Count > 0)
                {
                    oDS.Tables["OptOut"].Columns["EmailAddress"].ColumnMapping = MappingType.Attribute;
                }
                // lower case the xml so that we can compare properely
                var oXML = new XmlDocument();
                oXML.InnerXml = Strings.LCase(Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<"));


                // now cycle through the users
                foreach (XmlElement oElmt in oXML.DocumentElement.SelectNodes("users"))
                {
                    string cEmail = "";
                    int nID = 0;
                    // get the user id
                    // we will need this for personalised to get the user xml
                    nID = Conversions.ToInteger(oElmt.GetAttribute("ndirkey"));
                    // get the email address

                    XmlElement oEmailElmt = (XmlElement)oElmt.SelectSingleNode("descendant-or-self::user/email");
                    if (oEmailElmt != null)
                    {
                        cEmail = oEmailElmt.InnerText;
                    }

                    XmlElement oCheckElmt = (XmlElement)oXML.DocumentElement.SelectSingleNode("optout[@emailaddress='" + cEmail + "']");

                    if (oCheckElmt is null)
                    {
                        oDic.Add(cEmail, nID);
                    }

                }
                return oDic;
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "emailer", ex, "GetGroupEmails", "", gbDebug);
                return null;
            }

        }



        // 'Public Sub courseReminder(ByRef oResultsXml As XmlElement, ByVal cUserName As String, ByVal cUserEmail As String, ByVal subject As String, ByVal sEmailTemplate As String, ByRef fForm As Collections.Specialized.NameValueCollection) 'As String
        // '    'PerfMon.Log("Messaging", "courseReminder")
        // '    ' This takes the candidate course results Report xml and sends email to all the users highlighted in the form.

        // '    Dim cProcessInfo As String = ""
        // '    Dim aUserId() As String
        // '    Dim sReturn As String
        // '    Dim oUserElmt As XmlElement
        // '    'Dim sProjectPath As String = ConfigurationSettings.AppSettings("ewProjectPath")
        // '    Dim sProjectPath As String = System.Configuration.ConfigurationManager.AppSettings("ewProjectPath")
        // '    Dim cRecipientEmail As String
        // '    Dim cCourseName As String

        // '    Try
        // '        cCourseName = oResultsXml.GetAttribute("CourseName")


        // '        'step through selected users
        // '        aUserId = Split(fForm("userId"), ",")

        // '        Dim i As Long

        // '        For i = 0 To UBound(aUserId)

        // '            'extract the users XML
        // '            oUserElmt = oResultsXml.SelectSingleNode("candidate[@id='" & aUserId(i) & "']")
        // '            cRecipientEmail = oUserElmt.SelectSingleNode("Email").InnerText
        // '            oUserElmt.SetAttribute("courseName", cCourseName)
        // '            Try
        // '                sReturn = emailer(oUserElmt, sProjectPath & "/xsl/email/" & sEmailTemplate & ".xsl" _
        // '                                    , cUserName _
        // '                                    , cUserEmail _
        // '                                    , cRecipientEmail _
        // '                                    , subject _
        // '                                    , "Sent")
        // '            Catch ex As Exception

        // '                sReturn = "Failed"

        // '            End Try
        // '            oUserElmt.SetAttribute("message", sReturn)

        // '        Next

        // '    Catch ex As Exception
        // '        returnException(msException, mcModuleName, "courseReminder", ex, "", cProcessInfo, gbDebug)
        // '    End Try
        // 'End Sub



        public class UserEmailDictionary : DictionaryBase
        {

            public int this[string key]
            {
                get
                {
                    return Conversions.ToInteger(Dictionary[key]);
                }
                set
                {
                    Dictionary[key] = value;
                }
            }


            public ICollection Keys
            {
                get
                {
                    return Dictionary.Keys;
                }
            }

            public ICollection Values
            {
                get
                {
                    return Dictionary.Values;
                }
            }

            public void Add(string key, int value)
            {
                if (!Contains(key))
                    Dictionary.Add(key, value);
            } // Add

            public bool Contains(string key)
            {
                return Dictionary.Contains(key);
            } // Contains

            public void Remove(string key)
            {
                Dictionary.Remove(key);
            } // Remove

            protected override void OnInsert(object key, object value)
            {
                if (!typeof(string).IsAssignableFrom(key.GetType()))
                {
                    throw new ArgumentException("key must be of type Recipient.", "key");
                }
            } // OnInsert

            protected override void OnRemove(object key, object value)
            {
                if (!typeof(string).IsAssignableFrom(key.GetType()))
                {
                    throw new ArgumentException("key must be of type Recipient.", "key");
                }
            } // OnRemove

            protected override void OnSet(object key, object oldValue, object newValue)
            {
                if (!typeof(string).IsAssignableFrom(key.GetType()))
                {
                    throw new ArgumentException("key must be of type Recipient.", "key");
                }
            } // OnSet

            protected override void OnValidate(object key, object value)
            {
                if (!typeof(string).IsAssignableFrom(key.GetType()))
                {
                    throw new ArgumentException("key must be of type Recipient.", "key");
                }
            } // OnValidate 


        }


    }

    public class POP3
    {

        public System.Net.Sockets.TcpClient Server;
        public System.Net.Sockets.NetworkStream NetStrm;
        public StreamReader RdStrm;
        public string Data;
        public byte[] szData;
        public string CRLF = Constants.vbCrLf; // "\r\n"

        public void ReadMail(string cServer, string cUser, string cPassword)
        {
            // PerfMon.Log("POP3", "ReadMail")
            try
            {
                int nNoEmails = Conversions.ToInteger(Connect(cServer, cUser, cPassword));

                int i;
                var oXML = new XMLEmail[nNoEmails];

                var loopTo = nNoEmails;
                for (i = 1; i <= loopTo; i++)
                {
                    string Message = Retrieve(i);
                    var oMsgXML = CreateMailXML(Message, i);
                    oXML[i - 1] = ConvertToMail(oMsgXML);
                    var oElmt = oXML[i - 1].Full;
                    Debug.WriteLine(Information.UBound(oXML));
                }
                Disconnect();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

        }


        private string Connect(string cServer, string cUser, string cPassword)
        {
            // PerfMon.Log("POP3", "Connect")

            // create server POP3 with port 110
            Server = new System.Net.Sockets.TcpClient(cServer, 110);
            try
            {

                // initialization
                NetStrm = Server.GetStream();
                RdStrm = new StreamReader(Server.GetStream());
                Debug.WriteLine(RdStrm.ReadLine());

                // Login Process
                Data = "USER " + cUser + CRLF;
                szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray());
                NetStrm.Write(szData, 0, szData.Length);
                Debug.WriteLine(RdStrm.ReadLine());

                Data = "PASS " + cPassword + CRLF;
                szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray());
                NetStrm.Write(szData, 0, szData.Length);
                Debug.WriteLine(RdStrm.ReadLine());

                // Send STAT command to get information ie: number of mail and size
                Data = "STAT " + CRLF;
                szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray());
                NetStrm.Write(szData, 0, szData.Length);

                string[] tmpArray;
                tmpArray = Strings.Split(RdStrm.ReadLine(), " ");
                string numMess = tmpArray[1];
                return Conversions.ToInteger(numMess).ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return 0.ToString();
            }
        }

        private string Retrieve(int nMessageNo)
        {
            // PerfMon.Log("POP3", "Retrieve")

            string szTemp;
            try
            {
                string Message = "";
                // retrieve mail with number mail parameter
                Data = "RETR " + nMessageNo + CRLF;
                szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray());
                NetStrm.Write(szData, 0, szData.Length);

                szTemp = RdStrm.ReadLine();
                if (!(Strings.Left(szTemp, 0) == "-"))
                {
                    while (!(szTemp == "."))
                    {
                        if (!(Strings.Left(szTemp, 0) == "+"))
                            Message += szTemp + CRLF;
                        szTemp = RdStrm.ReadLine();
                    }
                }
                else
                {

                }

                return Message;
            }
            catch (Exception)
            {
                // Status.Items.Add("Error: " + Err.ToString())
                return "";
            }
        }

        private bool Disconnect()
        {
            // PerfMon.Log("POP3", "Disconnect")
            // Send QUIT command to close session from POP server
            try
            {
                Data = "QUIT " + CRLF;
                szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray());
                NetStrm.Write(szData, 0, szData.Length);
                // close connection
                NetStrm.Close();
                RdStrm.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public XmlDocument CreateMailXML(string cMessage, int nId)
        {
            // PerfMon.Log("POP3", "CreateMailXML")
            string cCurrent = string.Empty;
            try
            {
                cMessage = Strings.Replace(cMessage, "+OK", "");
                cMessage = Strings.Replace(cMessage, ";" + Constants.vbCrLf, ";");
                cMessage = Strings.Replace(cMessage, Constants.vbTab, "");
                cMessage = Strings.Replace(cMessage, ">," + Constants.vbCrLf + "<", ">,<");
                string cSplitStr = "";
                bool bMessageStarted = false;
               // bool bMessageFinished = false;
                var oXML = new XmlDocument();
                oXML.AppendChild(oXML.CreateElement("MailMessage"));
                var oMessageElement = oXML.CreateElement("Message");

                string[] oItems = Strings.Split(cMessage, Constants.vbCrLf);
                int i;
                var loopTo = Information.UBound(oItems);
                for (i = 0; i <= loopTo; i++)
                {
                    string cCleanLine = Strings.Trim(oItems[i]);
                    if (cCleanLine.Contains(": "))
                    {
                        string[] oItemSplit = Strings.Split(oItems[i], ": ");
                        if (Information.UBound(oItemSplit) == 1)
                        {
                            if (!AddMailElement(oXML.DocumentElement, oItemSplit[0], oItemSplit[1]))
                            {
                                AddMailElement(oXML.DocumentElement, "Item", cCleanLine);
                            }
                        }
                        else
                        {
                            AddMailElement(oXML.DocumentElement, "Item", cCleanLine);
                        }
                    }
                    else
                    {
                        AddMailElement(oXML.DocumentElement, "Item", cCleanLine);
                    }
                    // check if we are in the messaqge
                    if (oXML.DocumentElement.LastChild.InnerText.Contains(cSplitStr) & !bMessageStarted & !string.IsNullOrEmpty(cSplitStr))
                    {
                        bMessageStarted = true;
                        // ElseIf oXML.DocumentElement.LastChild.InnerText.Contains(cSplitStr) And bMessageStarted And Not bMessageFinished And Not cSplitStr = "" Then
                        // bMessageFinished = True
                    }

                    if (oXML.DocumentElement.LastChild.Name == "Content-Type")
                    {
                        if (oXML.DocumentElement.LastChild.InnerText.Contains(";boundary="))
                        {
                            int nStart = Strings.InStr(oXML.DocumentElement.LastChild.InnerText, ";boundary=") + 10;
                            cSplitStr = Strings.Right(oXML.DocumentElement.LastChild.InnerText, oXML.DocumentElement.LastChild.InnerText.Length - nStart);
                            cSplitStr = Strings.Left(cSplitStr, cSplitStr.Length - 1);
                        }
                    }



                    if (oXML.DocumentElement.LastChild.Name == "Item" & bMessageStarted & !oXML.DocumentElement.LastChild.InnerText.Contains(cSplitStr)) // If oXML.DocumentElement.LastChild.Name = "Item" And bMessageStarted And Not bMessageFinished And Not oXML.DocumentElement.LastChild.InnerText.Contains(cSplitStr) Then
                    {
                        oMessageElement.InnerText += oXML.DocumentElement.LastChild.InnerText + CRLF;
                    }
                }
                oXML.DocumentElement.AppendChild(oMessageElement);
                return oXML;
            }
            catch (Exception)
            {
                // returnException("POP3", "EmailListTransform", ex, "", "EmailListTransform", )
                return null;
            }


        }

        public bool AddMailElement(XmlElement oParent, string oItemName, string oItemValue)
        {
            // PerfMon.Log("POP3", "AddMailElement")
            try
            {
                if (string.IsNullOrEmpty(oItemValue) | oItemValue == ".")
                    return true;
                var oElmt = oParent.OwnerDocument.CreateElement(Strings.Trim(oItemName));
                oElmt.InnerText = Strings.Trim(oItemValue);
                oParent.AppendChild(oElmt);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public XMLEmail ConvertToMail(XmlDocument RawXML)
        {
            // PerfMon.Log("POP3", "ConvertToMail")
            try
            {
                XmlElement oElmt;
                oElmt = (XmlElement)RawXML.DocumentElement.SelectSingleNode("To");
                if (oElmt is null)
                    return null;
                oElmt = (XmlElement)RawXML.DocumentElement.SelectSingleNode("From");
                if (oElmt is null)
                    return null;
                oElmt = (XmlElement)RawXML.DocumentElement.SelectSingleNode("Thread-Topic");
                if (oElmt is null)
                    return null;
                oElmt = (XmlElement)RawXML.DocumentElement.SelectSingleNode("Message");
                if (oElmt is null)
                    return null;

                var oEmail = new XMLEmail();
                oEmail.Subject = RawXML.DocumentElement.SelectSingleNode("Thread-Topic").InnerText;
                oEmail.Body = RawXML.DocumentElement.SelectSingleNode("Message").InnerText;
                oEmail.AddAddress(XMLEmail.ToType.From, oEmail.ConvertTextAddress(RawXML.DocumentElement.SelectSingleNode("From").InnerText));
                int i;
                string[] oAdds = Strings.Split(RawXML.DocumentElement.SelectSingleNode("To").InnerText, ",");
                var loopTo = Information.UBound(oAdds);
                for (i = 0; i <= loopTo; i++)
                    oEmail.AddAddress(XMLEmail.ToType.To, oEmail.ConvertTextAddress(oAdds[i]));
                if (RawXML.DocumentElement.SelectSingleNode("Cc") != null)
                {
                    oAdds = Strings.Split(RawXML.DocumentElement.SelectSingleNode("Cc").InnerText, ",");
                    var loopTo1 = Information.UBound(oAdds);
                    for (i = 0; i <= loopTo1; i++)
                        oEmail.AddAddress(XMLEmail.ToType.CC, oEmail.ConvertTextAddress(oAdds[i]));
                }
                if (RawXML.DocumentElement.SelectSingleNode("Bcc") != null)
                {
                    oAdds = Strings.Split(RawXML.DocumentElement.SelectSingleNode("Bcc").InnerText, ",");
                    var loopTo2 = Information.UBound(oAdds);
                    for (i = 0; i <= loopTo2; i++)
                        oEmail.AddAddress(XMLEmail.ToType.BCC, oEmail.ConvertTextAddress(oAdds[i]));
                }
                return oEmail;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }


        public POP3()
        {

        }
    }

    public class XMLEmail
    {
        private XmlDocument oEmailXML;

        public XMLEmail()
        {
            // PerfMon.Log("XMLEmail", "New")
            try
            {
                oEmailXML = new XmlDocument();
                var oElmt = oEmailXML.CreateElement("Email");
                oEmailXML.AppendChild(oElmt);
                XmlElement oElmt1;
                oElmt1 = oEmailXML.CreateElement("From");
                oElmt.AppendChild(oElmt1);
                oElmt1 = oEmailXML.CreateElement("To");
                oElmt.AppendChild(oElmt1);
                oElmt1 = oEmailXML.CreateElement("CC");
                oElmt.AppendChild(oElmt1);
                oElmt1 = oEmailXML.CreateElement("BCC");
                oElmt.AppendChild(oElmt1);
                oElmt1 = oEmailXML.CreateElement("Subject");
                oElmt.AppendChild(oElmt1);
                oElmt1 = oEmailXML.CreateElement("Body");
                oElmt.AppendChild(oElmt1);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public bool AddAddress(ToType AddType, string cEmailAddress, string cDisplayName = "")
        {
            // PerfMon.Log("XMLEmail", "AddAddress")
            try
            {
                var oAddElmt = oEmailXML.CreateElement("EmailAddress");
                var oEmailElmt = oEmailXML.CreateElement("Address");
                oEmailElmt.InnerText = cEmailAddress;
                var oNameElmt = oEmailXML.CreateElement("DisplayName");
                oNameElmt.InnerText = cDisplayName;
                oAddElmt.AppendChild(oEmailElmt);
                oAddElmt.AppendChild(oNameElmt);
                oEmailXML.DocumentElement.SelectSingleNode(Enum.GetName(typeof(ToType), AddType)).AppendChild(oAddElmt);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddAddress(ToType AddType, MailAddress oAddress)
        {
            // PerfMon.Log("XMLEmail", "AddAddress")
            try
            {
                var oAddElmt = oEmailXML.CreateElement("EmailAddress");
                var oEmailElmt = oEmailXML.CreateElement("Address");
                oEmailElmt.InnerText = oAddress.Address;
                var oNameElmt = oEmailXML.CreateElement("DisplayName");
                oNameElmt.InnerText = oAddress.DisplayName;
                oAddElmt.AppendChild(oEmailElmt);
                oAddElmt.AppendChild(oNameElmt);
                oEmailXML.DocumentElement.SelectSingleNode(Enum.GetName(typeof(ToType), AddType)).AppendChild(oAddElmt);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool RemoveAddress(ToType AddType, string cEmailAddress)
        {
            // PerfMon.Log("XMLEmail", "RemoveAddress")
            try
            {
                XmlElement oMainElmt = (XmlElement)oEmailXML.DocumentElement.SelectSingleNode(Enum.GetName(typeof(ToType), AddType));
                foreach (XmlElement oElmt in oMainElmt.SelectNodes("EmailAddress[Address='" + cEmailAddress + "']"))
                    oElmt.ParentNode.RemoveChild(oElmt);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string Body
        {
            get
            {
                return oEmailXML.DocumentElement.SelectSingleNode("Body").InnerText;
            }
            set
            {
                oEmailXML.DocumentElement.SelectSingleNode("Body").InnerText = value;
            }
        }

        public string Subject
        {
            get
            {
                return oEmailXML.DocumentElement.SelectSingleNode("Subject").InnerText;
            }
            set
            {
                oEmailXML.DocumentElement.SelectSingleNode("Subject").InnerText = value;
            }
        }

        public XmlElement Full
        {
            get
            {
                return oEmailXML.DocumentElement;
            }
        }

        public MailAddress[] get_ToList(ToType AddType)
        {
            var oAddArr = new MailAddress[1];
            int i = 0;
            foreach (XmlElement oElmt in oEmailXML.DocumentElement.SelectSingleNode(Enum.GetName(typeof(ToType), AddType)).ChildNodes)
            {
                var oAdd = new MailAddress(oElmt.SelectSingleNode("Address").InnerText, oElmt.SelectSingleNode("DisplayName").InnerText);
                Array.Resize(ref oAddArr, i + 1 + 1);
                oAddArr[i] = oAdd;
            }
            return oAddArr;
        }

        public MailAddress ConvertTextAddress(string cTextAddress)
        {
            // PerfMon.Log("XMLEmail", "ConvertTextAddress")
            try
            {
                // "Barry Rushton" <barryr@eonic.co.uk>
                string cName = "";
                if (cTextAddress.Contains(Conversions.ToString('"')))
                {
                    cName = Strings.Right(cTextAddress, cTextAddress.Length - Strings.InStr(cTextAddress, Conversions.ToString('"')));
                    if (!string.IsNullOrEmpty(cName))
                        cName = Strings.Left(cName, Strings.InStr(cName, Conversions.ToString('"')) - 1);
                }
                string cAddress;
                cAddress = Strings.Right(cTextAddress, cTextAddress.Length - Strings.InStr(cTextAddress, "<"));
                if (!string.IsNullOrEmpty(cAddress))
                    cAddress = Strings.Left(cAddress, Strings.InStr(cAddress, ">") - 1);

                return new MailAddress(Strings.Trim(cAddress), Strings.Trim(cName));
            }
            catch (Exception)
            {
                return null;
            }
        }



        public enum ToType
        {
            To = 0,
            CC = 1,
            BCC = 2,
            From = 3
        }

    }
}