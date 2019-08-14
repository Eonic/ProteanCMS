using System;
using System.Collections.Generic;
using System.Xml;
using System.Web.Configuration;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Threading;
using static Protean.Cms;
using Microsoft.VisualBasic;
//using Microsoft.SqlServer.Management.Smo;


namespace Protean.Providers.Messaging
{
    public class Provider
    {

        public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, Protean.Providers.Messaging.BaseProvider MemProvider, ref Protean.Cms myWeb)
        {
            MemProvider.AdminXforms = new AdminXForms(myWeb);
            MemProvider.AdminProcess = new AdminProcess(ref myWeb);
            //MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms;
            MemProvider.Activities = new Activities();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public class AdminXForms : Protean.Providers.Messaging.EonicProvider.AdminXForms
        {
            private const string mcModuleName = "Providers.Messaging.Generic.AdminXForms";
            private System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");


            public AdminXForms(Cms aWeb) : base(ref aWeb)
            {
            }

            public XmlElement xFrmPreviewNewsLetter(int nPageId, XmlElement oPageDetail)
            {
                XmlElement oFrmElmt;

                string cProcessInfo = "";
                try
                {
                    base.NewFrm("SendNewsLetter");

                    base.submission("SendNewsLetter", "", "post", "");
                    xForm xform = new xForm();
                    //ss
                    //string Unpersonalised = "Unpersonalised";
                    //string sendUn = "Send Unpersonalised";
                    //oFrmElmt = xform.addGroup(moXformElmt, ref Unpersonalised, "", ref sendUn);

                    XmlElement oElmt;
                    string Long = "long";
                    //ss
                    //oElmt = addInput(ref oFrmElmt, "cEmail", true, "Email address to send to",ref Long);
                    //base.addBind("cEmail", "cEmail");
                    //oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"));
                    //base.addSubmit(ref oFrmElmt, "SendUnpersonalised", "Send Unpersonalised");

                    base.Instance.InnerXml = "<cEmail/><cUsers/>";

                    if (base.isSubmitted())
                    {
                        base.updateInstanceFromRequest();
                        base.validate();
                        XmlNode oEmailElmt = base.Instance.SelectSingleNode("cEmail");
                        if (!Protean.Tools.Text.IsEmail(oEmailElmt.InnerText))
                        {//ss
                         // base.addNote(oElmt.ToString(), xForm.noteTypes.Alert, "Incorrect Email Address Supplied");
                            base.valid = false;
                        }
                        if (base.valid)
                        {
                            System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                            string cEmail = base.Instance.SelectSingleNode("cEmail").InnerText;
                            // first we will only deal with unpersonalised
                            Protean.Messaging oMessager = new Protean.Messaging();
                            // get the subject
                            string cSubject = "";
                            Activities oMessaging = new Activities();
                            string cMailingXsl = moMailConfig["MailingXsl"];
                            if (cMailingXsl == "")
                                cMailingXsl = "/xsl/mailer/mailerStandard.xsl";
                            Protean.fsHelper ofs = new Protean.fsHelper(myWeb.moCtx);
                            cMailingXsl = ofs.checkCommonFilePath(cMailingXsl);
                            if (oMessaging.SendSingleMail_Queued(nPageId, cMailingXsl, cEmail, moMailConfig["SenderEmail"], moMailConfig["SenderName"], cSubject))
                            {
                                // add mssage and return to form so they can sen another

                                XmlElement oMsgElmt = oPageDetail.OwnerDocument.CreateElement("Content");

                                oMsgElmt.SetAttribute("type", "Message");
                                oMsgElmt.InnerText = "Messages Sent";
                                oPageDetail.AppendChild(oMsgElmt);
                            }
                        }
                    }

                    base.addValues();
                    return base.moXformElmt;
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
            }

            public XmlElement xFrmSendNewsLetter(int nPageId, string cPageName, string cDefaultEmail, string cDefaultEmailName, ref XmlElement oPageDetail, string cReplyEmail)
            {
                XmlElement oFrmElmt;
                XmlElement oCol1;
                XmlElement oCol2;

                string cProcessInfo = "";
                try
                {
                    base.NewFrm("SendNewsLetter");

                    base.submission("SendNewsLetter", "", "post", "");
                    //ss
                    //oFrmElmt = base.addGroup(base.moXformElmt, "Groups", "2col", "Please select a group(s) to send to.");

                    cDefaultEmail = (cDefaultEmail.Trim());

                    //oCol1 = base.addGroup(oFrmElmt, "", "col1", "");
                    //oCol2 = base.addGroup(oFrmElmt, "", "col2", "");

                    XmlElement oElmt;
                    string req = "required long";
                    //oElmt = base.addInput(ref oCol1, "cDefaultEmail", true, "Email address to send from", ref req);
                    //base.addBind("cDefaultEmail", "cDefaultEmail", "true()");
                    //oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"));

                    //XmlElement oElmt2;
                    //oElmt2 = base.addInput(ref oCol1, "cDefaultEmailName", true, "Name to send from", "required long");
                    //base.addBind("cDefaultEmailName", "cDefaultEmailName", "true()");
                    //oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"));

                    //oElmt2 = base.addInput(ref oCol1, "cReplyEmail", true, "Email to Reply to", "required long");
                    //base.addBind("cReplyEmail", "cReplyEmail", "true()");
                    //oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"));


                    //oElmt2 = base.addInput(ref oCol1, "cCampaignName", true, "Campaign Name", "required long");
                    //base.addBind("cCampaignName", "cCampaignName", "true()");
                    //oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"));

                    //oElmt2 = base.addInput(ref oCol1, "cSubject", true, "Subject", "required long");
                    //base.addBind("cSubject", "cSubject", "true()");
                    //oElmt2.AppendChild(ref oElmt.OwnerDocument.CreateElement("value"));

                    //oElmt2 = base.addRange(oCol1, "nSendInHrs", true, "Send in x Hrs", "0", "24", "1");
                    //base.addBind("nSendInHrs", "nSendInHrs", "true()");
                    //oElmt2.AppendChild(ref oElmt.OwnerDocument.CreateElement("value"));

                    //oElmt2 = base.addRange(ref oCol1, "nSendInMins", true, "Send in x Mins", "5", "60", "1");
                    //base.addBind("nSendInMins", "nSendInMins", "true()");
                    //oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"));

                    // XmlElement oSyncElmt = base.addSelect1(ref oCol1, "SyncMode", true, "Sync List Options", "required multiline", ref ApperanceTypes.Full);
                    // base.addOption(ref oSyncElmt, "Sync All Groups", "sync");
                    // base.addOption(ref oSyncElmt, "Sync Selected Groups", "syncSelected");
                    // base.addOption(ref oSyncElmt, "Send without Syncing Groups to Campaign Monitor", "noSync");
                    //ss
                    //base.addBind("SyncMode", "SyncMode", "true()");


                    // XmlElement oSelElmt = base.addSelect( ref oCol2, "cGroups", true, "Select Groups to send to", "required multiline", ref ApperanceTypes.Full);
                    //GetListsAsOptions(ref oSelElmt);
                    //ss
                    //base.addBind("cGroups", "cGroups", "true()");

                    // oFrmElmt =base.addGroup(base.moXformElmt, "Send", "", "");

                    //base.addSubmit(ref oFrmElmt, "Send", "Send Via CampaignMonitor");

                    base.Instance.InnerXml = "<cGroups/><cDefaultEmail>" + cDefaultEmail + "</cDefaultEmail><cDefaultEmailName>" + cDefaultEmailName + "</cDefaultEmailName><cReplyEmail>" + cDefaultEmail + "</cReplyEmail><cCampaignName>" + cPageName + "</cCampaignName><cSubject>" + cPageName + "</cSubject><nSendInHrs>0</nSendInHrs><nSendInMins>5</nSendInMins><SyncMode>syncSelected</SyncMode>";

                    if (base.isSubmitted())
                    {
                        base.updateInstanceFromRequest();
                        base.validate();
                        XmlNode oEmailElmt = base.Instance.SelectSingleNode("cDefaultEmail");

                        if (!Tools.Text.IsEmail(oEmailElmt.InnerText.Trim()))
                        {//ss
                            //base.addNote(oElmt.ToString(), xForm.noteTypes.Alert, "Incorrect Email Address Supplied");
                            base.valid = false;
                        }

                        if (base.valid)
                        {

                            // Sync the Groups
                            //ss
                            //Protean.Providers.Messaging.CampaignMonitor.AdminProcess AdminProcess = myWeb;
                            string innerXml = base.Instance.SelectSingleNode("cGroups").InnerText.ToString();
                            string[] SubscriberListIds = innerXml.Split(new char[] { ',' });
                            List<string> ListIds = new List<string>();
                            List<string> SegmentIds = new List<string>();
                            foreach (string id in SubscriberListIds)
                            {
                                if (id.StartsWith("SEGMENT"))
                                    SegmentIds.Add(id.Replace("SEGMENT", ""));
                                else
                                    ListIds.Add(id);
                            }


                            //string listId1;
                            switch (base.Instance.SelectSingleNode("SyncMode").InnerText)
                            {
                                case "sync":
                                    {
                                        //ss
                                        //AdminProcess.SyncLists();
                                        break;
                                    }

                                case "syncSelected":
                                    {
                                        //ss
                                        //foreach (var listId in SubscriberListIds)

                                        // AdminProcess.SyncLists(0, listId);
                                        break;
                                    }

                                case "noSync":
                                    {
                                        break;
                                    }
                            }


                            // Setup the Campaign
                            // Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()

                            createsend_dotnet.ApiKeyAuthenticationDetails cmAuth = new createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig["ApiKey"]);

                            object campaignId; // CampaignMonitorAPIWrapper.CampaignMonitorAPI.Campaign

                            string CampaignName = base.Instance.SelectSingleNode("cCampaignName").InnerText;
                            string CampaignSubject = base.Instance.SelectSingleNode("cSubject").InnerText;
                            string FromEmail = base.Instance.SelectSingleNode("cDefaultEmail").InnerText;
                            string FromName = base.Instance.SelectSingleNode("cDefaultEmailName").InnerText;
                            string ReplyEmail = base.Instance.SelectSingleNode("cReplyEmail").InnerText;
                            int SendHours = Convert.ToInt32(base.Instance.SelectSingleNode("nSendInHrs").InnerText);
                            int SendMins = Convert.ToInt32(base.Instance.SelectSingleNode("nSendInMins").InnerText);

                            string DirectURL = moMailConfig["DirectURL"];
                            if (DirectURL == "")
                                DirectURL = myWeb.moRequest.ServerVariables["SERVER_NAME"];

                            if (!DirectURL.StartsWith("http"))
                                DirectURL = "http://" + DirectURL;

                            string htmlUrl = DirectURL + "/?contentType=email&pgid=" + myWeb.mnPageId;
                            string textUrl = DirectURL + "/?contentType=email&pgid=" + myWeb.mnPageId + "&textonly=true";

                            // Dim SubscriberListIds() As String = MyBase.Instance.SelectSingleNode("cGroups").InnerText.Split(",")
                            object oListSegments = null;
                            try
                            {
                                campaignId = createsend_dotnet.Campaign.Create(cmAuth, moMailConfig["ClientID"], CampaignSubject, CampaignName, FromName, FromEmail, ReplyEmail, htmlUrl, textUrl, ListIds, SegmentIds);
                                createsend_dotnet.Campaign thisCampaign = new createsend_dotnet.Campaign(cmAuth, campaignId.ToString());

                                if (campaignId is CampaignMonitorAPIWrapper.CampaignMonitorAPI.Result)
                                {
                                    //ss 
                                    //base.addNote(oFrmElmt, xForm.noteTypes.Alert, campaignId.Message, true);
                                    base.valid = false;
                                }
                                else
                                {
                                    // add the campaign Id to the instance.
                                    XmlElement oElmt4 = moPageXML.CreateElement("CampaignId");
                                    oElmt4.InnerText = campaignId.ToString();
                                    base.Instance.AppendChild(oElmt4);
                                    DateTime sendDateTime = DateTime.Now;
                                    sendDateTime = sendDateTime.AddHours(SendHours);
                                    sendDateTime = sendDateTime.AddMinutes(SendMins);

                                    // Send the Email
                                    try
                                    {
                                        thisCampaign.Send(FromEmail, sendDateTime);
                                        moDbHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, nPageId, 0, base.Instance.OuterXml);
                                        moDbHelper.CommitLogToDB(dbHelper.ActivityType.NewsLetterSent, myWeb.mnUserId, myWeb.moSession.SessionID, DateTime.Now, myWeb.mnPageId, 0, "", true);
                                        // base.addNote(oFrmElmt.ToString(), xForm.noteTypes.Alert, "Message Sent", true);
                                    }
                                    catch (Exception ex)
                                    {//ss
                                        //base.addNote(oFrmElmt.ToString(), xForm.noteTypes.Alert, ex.Message, true);
                                        base.valid = false;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {//ss
                                //base.addNote(oFrmElmt.ToString(), xForm.noteTypes.Alert, ex.Message, true);
                                base.valid = false;
                            }
                        }
                    }

                    base.addValues();
                    return base.moXformElmt;
                }
                catch (Exception ex)
                {
                    // returnException(mcModuleName, "xFrmSendNewsLetter", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
            }

            public XmlElement GetListsAsOptions(ref XmlElement oSelElmt)
            {
                try
                {
                    // Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                    object Lists;
                    object Segments;
                    createsend_dotnet.BasicList List;
                    createsend_dotnet.BasicSegment Segment;

                    createsend_dotnet.ApiKeyAuthenticationDetails cmAuth = new createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig["ApiKey"]);
                    createsend_dotnet.Client CMclient = new createsend_dotnet.Client(cmAuth, moMailConfig["ClientID"]);


                    // gets the lists for the client
                    Hashtable hLists = new Hashtable();
                    Lists = CMclient.Lists();
                    Segments = CMclient.Segments();
                    //ss
                    //for (int i = 0; i <=Microsoft.VisualBasic.Information.UBound(Lists as object[]); i++)
                    //{

                    //List =Lists[i];
                    //base.addOption(ref oSelElmt, List.Name, List.ListID);
                    //for (var j = 0; j <= Information.UBound(Segments as object[]); j++)
                    //{
                    //    Segment = Segments[j];
                    //    if (Segment.ListID == List.ListID)
                    //        base.addOption(ref oSelElmt, "--- " + Segment.Title, "SEGMENT" + Segment.SegmentID);
                    //}
                //}

                    return oSelElmt;
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug);
                    return null;
                }
            }
        }

        public class AdminProcess : Cms.Admin
        {
            private string IntegratorId = "d2d03c3dd847620c";

            AdminXForms _oAdXfm;
            private System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");

            private int CMSupressionListId;
            private int CMDeletedListId;
            private int CMUnSubscribedListId;
            private int CMBouncedListId;
            private int CMUnconfirmedListId;
            private List<ListSubscribe> BulkSubscribes = new List<ListSubscribe>();

            public AdminXForms oAdXfm
            {
                set
                {

                    _oAdXfm = value;
                }
                get
                {
                    return _oAdXfm;
                }
            }

            public AdminProcess(ref Cms aWeb) : base(ref aWeb)
            {
            }

            public void GetLocalListIDs()
            {
                try
                {
                    CMSupressionListId = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("CMSupressionListId", "group", "CM Supression List", "", "<Group><Name>CM Supression List</Name><Notes></Notes></Group>", 1, false));
                    CMDeletedListId = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("CMDeletedListId", "group", "CM Deleted List", "", "<Group><Name>CM Deleted List</Name><Notes></Notes></Group>", 1, false));
                    CMUnSubscribedListId = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("CMUnSubscribedListId", "group", "CM Unsubscribed List", "", "<Group><Name>CM Unsubscribed List</Name><Notes></Notes></Group>", 1, false));
                    CMBouncedListId = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("CMBouncedListId", "group", "CM Bounced List", "", "<Group><Name>CM Supression List</Name><Notes></Notes></Group>", 1, false));
                    CMUnconfirmedListId = Convert.ToInt32(myWeb.moDbHelper.insertDirectory("CMUnconfirmedListId", "group", "CM Unconfirmed List", "", "<Group><Name>CM Unconfirmed List</Name><Notes></Notes></Group>", 1, false));
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "GetLocalListIDs", ex, "", "", gbDebug);
                }
            }

            public string MailingListProcess(XmlElement oPageDetail, Cms oWeb, string sAdminLayout = "", string cCmd = "", bool bLoadStructure = false, string sEditContext = "", bool bClearEditContext = false)
            {
                string cRetVal = "";
                string cSQL = "";

                try
                {
                    System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                    if (moMailConfig == null)
                        return "";



                    long nMailMenuRoot = System.Convert.ToInt64("0" + moMailConfig["RootPageId"]);
                    // if we hit this we want to default to the mail root Id
                    if (myWeb.mnPageId == gnTopLevel & cCmd != "NewMail" & cCmd != "NormalMail" & cCmd != "AdvancedMail" & cCmd != "SyncMailList" & cCmd != "ListMailLists")
                    {
                        myWeb.mnPageId = Convert.ToInt32(nMailMenuRoot);

                        if (cCmd != "")
                            // skip image for submissions
                            cCmd = "MailingList";
                    }

                    string cMailingXsl = moMailConfig["MailingXsl"];
                    if (cMailingXsl == "")
                        cMailingXsl = "/xsl/mailer/mailerStandard.xsl";
                    Protean.fsHelper ofs = new Protean.fsHelper(myWeb.moCtx);
                    cMailingXsl = ofs.checkCommonFilePath(cMailingXsl);

                    // If cCmd = "IsMailingList" Then
                    // If myWeb.mnPageId = 0 Or Not myweb.moConfig("MailingList") = "on" Then Return False
                    // If myWeb.mnPageId = moMailConfig("RootPageId") Then
                    // Return True
                    // Else
                    // cSQL = "SELECT nStructParId FROM tblContentStructure WHERE nStructKey = " & myWeb.mnPageId
                    // If myWeb.moDBHelper.exeProcessSQLScalar(cSQL) = moMailConfig("RootPageId") Then
                    // Return True
                    // Else
                    // Return False
                    // End If
                    // End If
                    // End If
                    if (cCmd == "OverrideAdminMode")
                    {
                        if (oPageDetail.OwnerDocument.DocumentElement.GetAttribute("adminMode") == "true")
                            oPageDetail.OwnerDocument.DocumentElement.SetAttribute("adminMode", "false");
                    }

                ProcessFlow:
                    ;
                    switch (cCmd)
                    {
                        case "MailingList":
                            {
                                sAdminLayout = "MailingList";
                                myWeb.mcEwSiteXsl = cMailingXsl;
                                bLoadStructure = false;
                                if (sEditContext == "Normal")
                                    EditContext = "NormalMail";
                                int nNewsletterRoot = System.Convert.ToInt32("0" + moMailConfig["RootPageId"]);
                                if (!myWeb.moDbHelper.checkPageExist(nNewsletterRoot))
                                    nNewsletterRoot = 0;

                                if (nNewsletterRoot == 0)
                                {
                                    string defaultPageXml = "<DisplayName title=\"\" linkType=\"internal\" exclude=\"false\" noindex=\"false\"/><Images><img class=\"icon\" /><img class=\"thumbnail\" /><img class=\"detail\" /></Images><Description/>";
                                    nNewsletterRoot = Convert.ToInt32(myWeb.moDbHelper.insertStructure(0, "", "Newsletters", defaultPageXml, "NewsletterRoot"));
                                    Protean.Config.UpdateConfigValue(ref myWeb, "protean/mailinglist", "RootPageId", System.Convert.ToString(nNewsletterRoot));
                                }

                                // we want to return here after editing
                                myWeb.moSession["lastPage"] = myWeb.mcOriginalURL;

                                // sAdminLayout = "MailingList"
                                // Web.gcEwSiteXsl = moMailConfig("MailingXsl")
                                // bLoadStructure = False
                                // we want to return here after editing
                                if (!myWeb.mbSuppressLastPageOverrides)
                                    myWeb.moSession["lastPage"] = myWeb.mcOriginalURL;
                                break;
                            }

                        case "NormalMail":
                            {
                                sAdminLayout = "";
                                cCmd = "NormalMail";
                                EditContext = "NormalMail";

                                myWeb.mcEwSiteXsl = cMailingXsl;
                                // we want to return here after editing
                                if (!myWeb.mbPopupMode)
                                {
                                    if (!myWeb.mbSuppressLastPageOverrides)
                                        myWeb.moSession["lastPage"] = "?ewCmd=NormalMail&pgid=" + myWeb.mnPageId; // myWeb.mcOriginalURL 'not this if being redirected after editing layout for instance.
                                }

                                ListCampaigns(ref oPageDetail);
                                break;
                            }

                        case "MailPreviewOn":
                            {
                                myWeb.mcEwSiteXsl = cMailingXsl;
                                sAdminLayout = "Preview";

                                myWeb.moSession["PreviewDate"] = DateTime.Now.Date;
                                myWeb.moSession["PreviewUser"] = oWeb.mnUserId;
                                break;
                            }

                        case "NewMail":
                        case "EditMail":
                            {
                                sAdminLayout = "NewMail";
                                int nPage;
                                if (myWeb.moRequest["pgid"] == "")
                                    nPage = 0;
                                else
                                    nPage = Convert.ToInt32(myWeb.moRequest["pgid"]);

                                oPageDetail.AppendChild(oAdXfm.xFrmEditPage(nPage, myWeb.moRequest["name"], "Mail"));
                                if (oAdXfm.valid)
                                {
                                    if (cCmd == "NewMail")
                                        cCmd = "MailingList";
                                    else
                                        cCmd = "NormalMail";
                                    oPageDetail.RemoveAll();
                                    goto ProcessFlow;
                                }

                                break;
                            }

                        case "AdvancedMail":
                            {
                                sAdminLayout = "Advanced";
                                EditContext = "AdvancedMail";

                                XmlDocument oCommonContentTypes = new XmlDocument();
                                if (System.IO.File.Exists(myWeb.goServer.MapPath("/ewcommon/xsl/mailer/layoutmanifest.xml")))
                                    oCommonContentTypes.Load(myWeb.goServer.MapPath("/ewcommon/xsl/pagelayouts/layoutmanifest.xml"));
                                if (System.IO.File.Exists(myWeb.goServer.MapPath(moConfig["ProjectPath"] + "/xsl/mailer/layoutmanifest.xml")))
                                {
                                    XmlDocument oLocalContentTypes = new XmlDocument();
                                    oLocalContentTypes.Load(myWeb.goServer.MapPath(moConfig["ProjectPath"] + "/xsl/mailer/layoutmanifest.xml"));
                                    XmlNode oLocals = oLocalContentTypes.SelectSingleNode("/PageLayouts/ContentTypes");
                                    if (oLocals != null)
                                    {

                                        foreach (XmlNode oGrp in oLocals.SelectNodes("ContentTypeGroup"))
                                        {
                                            XmlElement oComGrp = (XmlElement)oCommonContentTypes.SelectSingleNode("/PageLayouts/ContentTypes/ContentTypeGroup[@name='" + oGrp.SelectNodes("name") + "']");
                                            if (oComGrp != null)
                                            {
                                                //XmlElement oTypeElmt;
                                                foreach (XmlElement oTypeElmt in oGrp.SelectNodes("ContentType"))
                                                {
                                                    if (oComGrp.SelectSingleNode("ContentType[@type='" + oTypeElmt.GetAttribute("type") + "']") != null)
                                                        oComGrp.SelectSingleNode("ContentType[@type='" + oTypeElmt.GetAttribute("type") + "']").InnerText = oTypeElmt.InnerText;
                                                    else
                                                        oComGrp.InnerXml += oTypeElmt.OuterXml;
                                                }
                                            }
                                            else
                                                oCommonContentTypes.DocumentElement.SelectSingleNode("ContentTypes").InnerXml += oGrp.OuterXml;
                                        }
                                    }
                                }
                                // now to add it to the pagexml
                                oPageDetail.AppendChild(moPageXML.ImportNode(oCommonContentTypes.SelectSingleNode("/PageLayouts/ContentTypes"), true));


                                if (myWeb.moRequest["pgid"] != "")
                                    // lets save the page we are editing to the session
                                    myWeb.moSession["pgid"] = myWeb.moRequest["pgid"];
                                // we want to return here after editing
                                myWeb.moSession["lastPage"] = myWeb.mcOriginalURL;
                                break;
                            }

                        case "MailHistory":
                            {
                                EditContext = "";
                                myWeb.moDbHelper.ViewMailHistory(ref oPageDetail);
                                break;
                            }

                        case "EditMailLayout":
                            {
                                EditContext = "";
                                moAdXfm.goServer = myWeb.goServer;

                                if (myWeb.moRequest["pgid"] != null)
                                {
                                    oPageDetail.AppendChild(oAdXfm.xFrmEditMailLayout(Convert.ToInt32(myWeb.moRequest["pgid"])));

                                    if (oAdXfm.valid)
                                    {
                                        cCmd = "NormalMail";
                                        sAdminLayout = "NormalMail";
                                        oPageDetail.RemoveAll();
                                        myWeb.msRedirectOnEnd = myWeb.moSession["lastPage"].ToString();
                                    }
                                    else
                                        sAdminLayout = "AdminXForm";
                                }
                                break;
                            }

                        case "AddMailModule":
                            {
                                EditContext = "";
                                bLoadStructure = true;
                                nAdditionId = 0;

                                if (myWeb.moRequest["pgid"] != null && myWeb.moRequest["position"] != null)
                                {
                                    oPageDetail.AppendChild(oAdXfm.xFrmAddModule(Convert.ToInt32(myWeb.moRequest["pgid"]), myWeb.moRequest["position"]));
                                    if (oAdXfm.valid)
                                    {
                                        if (myWeb.moRequest["nStatus"] != "")
                                        {
                                            oPageDetail.RemoveAll();
                                            if (myWeb.moSession["lastPage"].ToString() != "")
                                            {
                                                myWeb.msRedirectOnEnd = myWeb.moSession["lastPage"].ToString();
                                                myWeb.moSession["lastPage"] = "";
                                            }
                                            else
                                            {
                                                cCmd = "NormalMail";
                                                oPageDetail.RemoveAll();
                                                moAdXfm.valid = false;
                                                goto ProcessFlow;
                                            }
                                        }
                                        else
                                        {
                                            cCmd = "AddMailModule";
                                            sAdminLayout = "AdminXForm";
                                        }
                                    }
                                    else
                                        sAdminLayout = "AdminXForm";
                                }
                                break;
                            }

                        case "EditMailContent":
                            {

                                // Get a version Id if it's passed through.
                                string cVersionKey = myWeb.moRequest["verId"] + "";
                                bClearEditContext = false;
                                bLoadStructure = true;
                                //ss
                                //if (!(Information.IsNumeric(cVersionKey)))
                                //    cVersionKey = "0";
                                int nContentId;
                                nContentId = 0;
                                //ss
                                //oPageDetail.AppendChild((moAdXfm.xFrmEditContent(myWeb.moRequest["id"]), "", System.Convert.ToInt64(myWeb.moRequest["pgid"]), "", false, nContentId, "" , "", long.Parse(cVersionKey)));

                                if (moAdXfm.valid)
                                {
                                    // bAdminMode = False
                                    sAdminLayout = "";
                                    mcEwCmd = (myWeb.moSession["ewCmd"]).ToString();

                                    // if we have a parent releationship lets add it
                                    if (myWeb.moRequest["contentParId"] != "" && double.TryParse(myWeb.moRequest["contentParId"], out double partid)) ;
                                    myWeb.moDbHelper.insertContentRelation(Convert.ToInt32(myWeb.moRequest["contentParId"]), nContentId.ToString());
                                    if (myWeb.moRequest["EditXForm"] != "")
                                    {
                                        // bAdminMode = True
                                        sAdminLayout = "AdminXForm";
                                        mcEwCmd = "EditXForm";
                                        oPageDetail = oWeb.GetContentDetailXml(null/* Conversion error: Set to default value for this argument */, long.Parse(myWeb.moRequest["id"]));
                                    }
                                    else
                                    {
                                        myWeb.mnArtId = 0;
                                        oPageDetail.RemoveAll();

                                        // Check for an optional command to redireect to
                                        if (!(string.IsNullOrEmpty("" + myWeb.moRequest["ewRedirCmd"])))
                                            myWeb.msRedirectOnEnd = moConfig["ProjectPath"] + "/?ewCmd=" + myWeb.moRequest["ewRedirCmd"];
                                        else if (myWeb.moSession["lastPage"].ToString() != "")
                                        {
                                            myWeb.msRedirectOnEnd = myWeb.moSession["lastPage"].ToString();
                                            myWeb.moSession["lastPage"] = "";
                                        }
                                        else
                                        {
                                            oPageDetail.RemoveAll();
                                            moAdXfm.valid = false;
                                            goto ProcessFlow;
                                        }
                                    }
                                }
                                else
                                    sAdminLayout = "AdminXForm";
                                break;
                            }

                        case "PreviewMail":
                            {
                                string cSubject = "";
                                //ss
                                //XmlElement oLocElmt = (XmlElement)(oWeb.GetPageXML("descendant-or-self::MenuItem[@id=" + myWeb.mnPageId + "]").SelectSingleNode);
                                //if (oLocElmt != null)
                                //    cSubject = oLocElmt.GetAttribute("name");
                                //oPageDetail.AppendChild(oAdXfm.xFrmPreviewNewsLetter(myWeb.mnPageId,ref oPageDetail, cSubject));

                                //sAdminLayout = "PreviewMail";
                                break;
                            }

                        case "SendMail":
                            {
                                sAdminLayout = "SendMail";
                                // get subject
                                string cSubject = "";
                                oWeb.mnMailMenuId = long.Parse(moMailConfig["RootPageId"]);
                                //ss
                                //XmlElement oLocElmt = oWeb.GetPageXML.SelectSingleNode("descendant-or-self::MenuItem[@id=" + myWeb.mnPageId + "]");
                                //if (oLocElmt != null)
                                //    cSubject = oLocElmt.GetAttribute("name");

                                //oPageDetail.AppendChild(oAdXfm.xFrmSendNewsLetter(myWeb.mnPageId, cSubject, moMailConfig["SenderEmail"], moMailConfig["SenderName"], oPageDetail, moMailConfig["ReplyEmail"]));
                                oPageDetail.AppendChild(getCampaignReports(myWeb.mnPageId));
                                break;
                            }

                        case "MailOptOut":
                            {
                                sAdminLayout = "OptOut";
                                //ss
                                //oPageDetail.AppendChild(oAdXfm.xFrmAdminOptOut);
                                break;
                            }

                        case "ProcessMailbox":
                            {
                                break;
                            }

                        case "DeletePageMail":
                            {
                                bLoadStructure = true;
                                //ss
                                //oPageDetail.AppendChild(oAdXfm.xFrmDeletePage(myWeb.moRequest["pgid"]));
                                //if (oAdXfm.valid)
                                //    myWeb.msRedirectOnEnd = "/?ewCmd=MailingList";
                                //else
                                //    sAdminLayout = "AdminXForm";
                                break;
                            }

                        case "SearchMailContent":
                            {
                                break;
                            }

                        case "ListMailLists":
                            {
                                //  XmlElement oGrp;

                                oPageDetail.AppendChild(myWeb.moDbHelper.listDirectory("Group", System.Convert.ToInt32("0")));

                                // remove the global groups
                                string GroupsXpath = "directory/group";
                                if (myWeb.moConfig["NonAuthenticatedUsersGroupId"] != "")
                                    GroupsXpath = GroupsXpath + "[@id = '" + myWeb.moConfig["NonAuthenticatedUsersGroupId"] + "' and @id = '" + myWeb.moConfig["AuthenticatedUsersGroupId"] + "']";

                                foreach (XmlNode oGrp in oPageDetail.SelectNodes(GroupsXpath))
                                    oGrp.ParentNode.RemoveChild(oGrp);


                                sAdminLayout = "ListMailLists"; // "ListGroups"
                                                                // myWeb.moSession("ewCmd") = mcEwCmd
                                GetListsAsXml(ref oPageDetail);
                                break;
                            }

                        case "SyncMailList":
                            {
                                SyncLists(System.Convert.ToInt32(myWeb.moRequest["parId"]));
                                cCmd = "ListMailLists";
                                goto ProcessFlow;
                                break;
                            }

                        case "MailingList.CMSession":
                            {
                                // Get a session URL
                                createsend_dotnet.ApiKeyAuthenticationDetails AuthDetails = new createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig["ApiKey"]);

                                createsend_dotnet.ExternalSessionOptions ExtSessionOptions = new createsend_dotnet.ExternalSessionOptions();

                                ExtSessionOptions.Chrome = "Tabs";
                                ExtSessionOptions.ClientID = moMailConfig["ClientID"];
                                ExtSessionOptions.Email = moMailConfig["CMUsername"];
                                ExtSessionOptions.IntegratorID = IntegratorId;
                                ExtSessionOptions.Url = "/";

                                createsend_dotnet.General CMGen = new createsend_dotnet.General(AuthDetails);

                                string sessionURL = CMGen.ExternalSessionUrl(ExtSessionOptions);

                                XmlElement oElmt = moPageXML.CreateElement("SessionPage");
                                oElmt.SetAttribute("CMUrl", sessionURL);

                                oPageDetail.AppendChild(oElmt);
                                break;
                            }
                    }

                    sEditContext = EditContext;
                    bClearEditContext = clearEditContext;

                    myWeb.moSession["ewCmd"] = cCmd;
                    mcEwCmd = cCmd;

                    // Return cRetVal
                    return "";
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "NewsLetterProcess", ex, "", "", gbDebug);
                    return "";
                }
            }


            public void MailingListAdminMenu(ref XmlElement oAdminMenu)
            {
            }

            public void SyncUser(ref int nUserId)
            {
                try
                {
                    string cProcessInfo;
                    // This is just a placeholder for overloading
                    XmlElement oUserXml = myWeb.moDbHelper.GetUserXML(nUserId);

                    SyncMember Tasks = new SyncMember();
                    Int32 workerThreads;
                    Int32 compPortThreads;
                    System.Threading.ThreadPool.GetMaxThreads(out workerThreads, out compPortThreads);
                    System.Threading.ThreadPool.SetMaxThreads(workerThreads, compPortThreads);
                    CountdownEvent finished = new CountdownEvent(1);

                    foreach (XmlElement oGroup in oUserXml.SelectNodes("Group"))
                    {
                        bool unSubBehavoir = false;
                        // get unsubBehaviour
                        XmlElement oGroupXml = myWeb.moPageXml.CreateElement("instance");
                        oGroupXml.InnerXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, long.Parse(oGroup.GetAttribute("id")));
                        XmlNode groupDetail = oGroupXml.SelectSingleNode("tblDirectory/cDirXml/Group");
                        if (groupDetail.Attributes["unsubscribebehaviour"].InnerText == "1")
                            unSubBehavoir = true;

                        // GetCMListID
                        SyncMember.MemberObj stateObj = new SyncMember.MemberObj();
                        stateObj.oUserElmt = oUserXml;
                        stateObj.myWeb = myWeb;
                        stateObj.CmListID = getListId(oGroup.GetAttribute("name"));
                        stateObj.CMBouncedListId = CMBouncedListId;
                        stateObj.CMDeletedListId = CMDeletedListId;
                        stateObj.CMSupressionListId = CMSupressionListId;
                        stateObj.CMUnconfirmedListId = CMUnconfirmedListId;
                        stateObj.CMUnSubscribedListId = CMUnSubscribedListId;
                        stateObj.EwGroupID = oGroup.GetAttribute("id");
                        stateObj.UnSubRemove = unSubBehavoir;
                        //ss
                        // System.Threading.ThreadPool.QueueUserWorkItem(New System.Threading.WaitCallback(AddressOf Tasks.SyncSingleMember), stateObj)
                        finished.AddCount();
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            try
                            {
                                Tasks.SyncSingleMember(stateObj);
                            }
                            finally
                            {
                                // Signal that the work item is complete.
                                finished.Signal();
                            }
                        }, stateObj);
                        stateObj = null;
                    }

                    finished.Signal();
                    finished.Wait(1000000); // THIS IS A Timeout
                    if (finished.CurrentCount != 0)
                        cProcessInfo = "Timed Out " + finished.CurrentCount + " left";
                    finished.Dispose();
                    finished = null;
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug);
                }
            }

            private void ListCampaigns(ref XmlElement oPageDetail)
            {
                try
                {
                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.api _api = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.api();
                    object Campaigns;
                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.Campaign Campaign;
                    XmlElement CampElmt;
                    XmlElement CampElmt2;
                    int i;
                    Campaigns = (_api.GetClientCampaigns(moMailConfig["ApiKey"], moMailConfig["ClientID"]));
                    CampElmt = base.moPageXML.CreateElement("Campaigns");
                    //ss
                    //for (i = 0; i <= Information.UBound(Campaigns as object[]); i++)
                    //{
                        
                    //    //Campaign = Campaigns(i);
                    //    //CampElmt2 = base.moPageXML.CreateElement("Campaign");
                    //    //CampElmt2.SetAttribute("id", Campaign.CampaignID);
                    //    //CampElmt2.SetAttribute("name", Campaign.Name);
                    //    //CampElmt2.SetAttribute("subject", Campaign.Subject);
                    //    //CampElmt2.SetAttribute("recipients", (Campaign.TotalRecipients).ToString());
                    //    //CampElmt.AppendChild(CampElmt2);
                    //}
                    oPageDetail.AppendChild(CampElmt);
                }
                catch (Exception ex)
                {
                    // returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug);
                }
            }



            public void SyncLists(int ListId = 0, string CMListId = "")
            {
                string cProcessInfo = "";
                try
                {
                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.api _api = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.api();
                    object Lists;
                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.List List;
                    // gets the membership groups for the website.

                    string cSQL = "SELECT nDirKey, cDirName  FROM tblDirectory WHERE (cDirSchema = 'Group') ORDER BY cDirName";
                    SqlDataReader oDr = myWeb.moDbHelper.getDataReader(cSQL);
                    Hashtable hDirGroups = new Hashtable();
                    while (oDr.Read())
                        hDirGroups.Add(oDr["cDirName"], oDr["nDirKey"]);

                    // gets the lists for the client
                    Hashtable hLists = new Hashtable();
                    Lists = _api.GetClientLists(moMailConfig["ApiKey"], moMailConfig["ClientID"]);
                    //ss
                    //for (var i = 0; i <= Information.UBound(Lists as string[]); i++)
                    //{
                    //    // ss
                    //    //List = Lists(i);
                    //    //hLists.Add(List.Name, List.ListID);
                    //    //hDirGroups.Remove(List.Name);
                    //}
                    //// DictionaryEntry key;
                    foreach (DictionaryEntry key in hDirGroups)
                    {
                        object newList = _api.CreateList(moMailConfig["ApiKey"], moMailConfig["ClientID"], (key.Key).ToString(), "", false, "");
                        cProcessInfo = newList.ToString();
                        hLists.Add(key.Key, newList);
                    }

                    oDr = myWeb.moDbHelper.getDataReader(cSQL);

                    while (oDr.Read())
                    {
                        if (oDr["nDirKey"] != myWeb.moConfig["NonAuthenticatedUsersGroupId"] && oDr["nDirKey"] != myWeb.moConfig["AuthenticatedUsersGroupId"])
                        {
                            if (ListId == 0 || ListId == Convert.ToInt32(oDr["nDirKey"]))
                            {
                                if (CMListId == "" || CMListId == (hLists[oDr["cDirName"]]).ToString())
                                {
                                    if (hLists[oDr["cDirName"]] == null)
                                    {
                                        cProcessInfo += oDr["cDirName"] + "[" + SyncListMembers(hLists[oDr["cDirName"]].ToString(), Convert.ToInt64(oDr["nDirKey"])) + "]";
                                    }

                                    else
                                    {
                                        cProcessInfo += oDr["cDirName"] + "[" + SyncListMembers(hLists[oDr["cDirName"]].ToString(), Convert.ToInt64(oDr["nDirKey"])) + "]";
                                    }

                                }
                            }
                        }
                    }

                    // If a list doesn't exist for a group create it.

                    // Save the List ID in the foriegn Ref.
                    cProcessInfo = cProcessInfo + "";

                }
                // Returns the lists as XML format

                catch (Exception ex)
                {
                    //returnException(mcModuleName, "ListCampaigns", ex, "", cProcessInfo, gbDebug);
                }
            }

            public XmlElement GetListsAsXml(ref XmlElement rootElmt)
            {
                string cProcessInfo = "";
                try
                {
                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.api _api = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.api();
                    object Lists;
                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.List List;
                    XmlElement listsElmt = rootElmt.OwnerDocument.CreateElement("ProviderLists");

                    // gets the lists for the client
                    Hashtable hLists = new Hashtable();
                    Lists = _api.GetClientLists(moMailConfig["ApiKey"], moMailConfig["ClientID"]);
                    //ss
                    //for (var i = 0; i <= Information.UBound(Lists as string[]); i++)
                    //{
                    //    //ss
                    //    //List = Lists(i);
                    //    //XmlElement listElmt = rootElmt.OwnerDocument.CreateElement("List");
                    //    //listElmt.SetAttribute("name", List.Name);
                    //    //listElmt.SetAttribute("id", List.ListID);
                    //    //listsElmt.AppendChild(listElmt);
                    //}

                    AddListStats Tasks = new AddListStats();
                    System.Threading.ThreadPool.SetMaxThreads(20, 20);
                    int count = 0;
                    int j = 0;
                    CountdownEvent finished = new CountdownEvent(1);
                    //ss
                    //for (var i = 0; i <= Information.UBound(Lists as string[]); i++)
                    //{
                    //    //ss
                    //    //List = Lists(i);
                    //    AddListStats.ListObj stateObj = new AddListStats.ListObj();
                    //    //stateObj.oMasterList = listsElmt;
                    //    //stateObj.CmListID = List.ListID;
                    //    //stateObj.API = _api;

                    //    finished.AddCount();
                    //    ThreadPool.QueueUserWorkItem(state =>
                    //    {
                    //        try
                    //        {
                    //            Tasks.UpdateListStats(stateObj);
                    //        }
                    //        finally
                    //        {
                    //            // Signal that the work item is complete.
                    //            if (finished != null)
                    //                finished.Signal();
                    //        }
                    //    }, stateObj);
                    //    stateObj = null;
                    //}
                    finished.Signal();
                    finished.Wait(100000);
                    finished.Dispose();
                    finished = null;

                    rootElmt.AppendChild(listsElmt);
                    return listsElmt;
                }
                catch (Exception ex)
                {
                    // returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug);
                    return null;
                }
            }

            private long SyncListMembers(string CmListID, long EwGroupID)
            {
                string cProcessInfo = "";
                long i = 0;
                bool unSubBehavoir = false;
                try
                {

                    // Get existing Unsubs
                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.api _api = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.api();
                    // Dim UnSubList As List(Of CampaignMonitorAPIWrapper.CampaignMonitorAPI.Subscriber)
                    object UnSubList;
                    UnSubList = _api.GetUnsubscribed(moMailConfig["ApiKey"], CmListID, null);

                    XmlElement oUsersXml;

                    GetLocalListIDs();
                    // gets the users in a group
                    oUsersXml = myWeb.moDbHelper.listDirectory("User", EwGroupID);

                    XmlElement oGroupXml = myWeb.moPageXml.CreateElement("instance");
                    oGroupXml.InnerXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, EwGroupID);
                    XmlNode groupDetail = oGroupXml.SelectSingleNode("tblDirectory/cDirXml/Group");
                    if (groupDetail.SelectNodes("unsubscribebehaviour").ToString() == "1")
                        unSubBehavoir = true;

                    long totalInstances = oUsersXml.SelectNodes("user").Count;

                    SyncMember Tasks = new SyncMember();
                    Int32 workerThreads;
                    Int32 compPortThreads;
                    System.Threading.ThreadPool.GetMaxThreads(out workerThreads, out compPortThreads);
                    System.Threading.ThreadPool.SetMaxThreads(workerThreads, compPortThreads);
                    CountdownEvent finished = new CountdownEvent(1);

                    //XmlElement oElmt;
                    foreach (XmlElement oElmt in oUsersXml.SelectNodes("user"))
                    {
                        bool bSyncUser = true;
                        foreach (object unSub in UnSubList as object[])
                        {
                            //ss
                            //if (unSub.EmailAddress == oElmt.SelectSingleNode("User/Email").InnerText)
                            bSyncUser = false;
                        }
                        if (bSyncUser)
                        {
                            SyncMember.MemberObj stateObj = new SyncMember.MemberObj();
                            stateObj.oUserElmt = oElmt;
                            stateObj.myWeb = myWeb;
                            stateObj.CmListID = CmListID;
                            stateObj.CMBouncedListId = CMBouncedListId;
                            stateObj.CMDeletedListId = CMDeletedListId;
                            stateObj.CMSupressionListId = CMSupressionListId;
                            stateObj.CMUnconfirmedListId = CMUnconfirmedListId;
                            stateObj.CMUnSubscribedListId = CMUnSubscribedListId;
                            stateObj.EwGroupID = EwGroupID.ToString();
                            stateObj.UnSubRemove = unSubBehavoir;
                            // System.Threading.ThreadPool.QueueUserWorkItem(New System.Threading.WaitCallback(AddressOf Tasks.SyncSingleMember), stateObj)
                            finished.AddCount();
                            ThreadPool.QueueUserWorkItem(state =>
                            {
                                try
                                {
                                    Tasks.SyncSingleMember(stateObj);
                                }
                                finally
                                {
                                    // Signal that the work item is complete.
                                    finished.Signal();
                                }
                            }, stateObj);
                            stateObj = null;
                        }
                        i = i + 1;
                    }

                    finished.Signal();
                    finished.Wait(1000000); // THIS IS A Timeout
                    if (finished.CurrentCount != 0)
                        cProcessInfo = "Timed Out " + finished.CurrentCount + " left";
                    finished.Dispose();
                    finished = null;

                    return i;
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "ListCampaigns", ex, "", "", gbDebug);
                }
                return i;
            }

            private XmlElement getCampaignReports(int PageId)
            {
                string cProcessInfo = "";
                XmlElement ReportElmt;
                string sSql = "select * from  tblActivityLog where nStructId = " + PageId + " and nActivityType = 3";
                DataSet oDs;
                // XmlNode oElmt;
                try
                {
                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.api _api = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.api();

                    ReportElmt = moPageXML.CreateElement("Report");
                    oDs = myWeb.moDbHelper.GetDataSet(sSql, "Campaign", "Report");

                    {
                        var withBlock = oDs.Tables[0];
                        withBlock.Columns["nActivityKey"].ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns["nUserDirId"].ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns["nStructId"].ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns["nArtId"].ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns["dDateTime"].ColumnMapping = System.Data.MappingType.Attribute;
                        withBlock.Columns["nActivityType"].ColumnMapping = System.Data.MappingType.Attribute;
                        // .Columns("cActivityDetail").ColumnMapping = Data.MappingType.SimpleContent
                        withBlock.Columns["cSessionId"].ColumnMapping = System.Data.MappingType.Attribute;
                        // .Columns("cIPAddress").ColumnMapping = Data.MappingType.Attribute
                        withBlock.Columns["nOtherId"].ColumnMapping = System.Data.MappingType.Attribute;
                    }

                    ReportElmt.InnerXml = oDs.GetXml();
                    ReportElmt.InnerXml = ReportElmt.FirstChild.InnerXml;
                    object result;
                    // CampaignMonitorAPIWrapper.CampaignMonitorAPI.Campaign campaign;
                    result = _api.GetClientCampaigns(moMailConfig["ApiKey"], moMailConfig["ClientId"]);

                    foreach (XmlElement oElmt in ReportElmt.SelectNodes("Campaign"))
                    {
                        oElmt.InnerXml = oElmt.SelectSingleNode("cActivityDetail").InnerText;
                        oElmt.InnerXml = oElmt.FirstChild.InnerXml;
                        if (oElmt.SelectSingleNode("CampaignId") != null)
                        {
                            string CampaignId = oElmt.SelectSingleNode("CampaignId").InnerText;
                            foreach (CampaignMonitorAPIWrapper.CampaignMonitorAPI.Campaign campaign in result as CampaignMonitorAPIWrapper.CampaignMonitorAPI.Campaign[])
                            {
                                if (campaign.CampaignID == CampaignId)
                                {
                                    oElmt.SetAttribute("name", campaign.Name);
                                    oElmt.SetAttribute("sentDate", campaign.SentDate);
                                    oElmt.SetAttribute("recipients", (campaign.TotalRecipients).ToString());
                                }
                            }
                            if (oElmt.GetAttribute("name") == "")
                            {
                                // we can't find it lets try for a summary
                                object campSummary;
                                campSummary = _api.GetCampaignSummary(moMailConfig["ApiKey"], CampaignId);
                                if (campSummary is CampaignMonitorAPIWrapper.CampaignMonitorAPI.CampaignSummary)
                                {
                                    oElmt.SetAttribute("bounced", null);//campSummary.Bounced
                                }
                                else
                                {
                                    oElmt.SetAttribute("name", null);//campSummary.Message
                                }

                            }
                        }
                    }

                    return ReportElmt;
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "getCampaignReports", ex, "", "", gbDebug);
                    return null;
                }
            }

            private string getListId(string ListName)
            {
                stdTools.PerfMon.Log("Messaging", "getListId");

                CampaignMonitorAPIWrapper.CampaignMonitorAPI.api _api = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.api();
                object Lists;
                CampaignMonitorAPIWrapper.CampaignMonitorAPI.List List;
                Hashtable hLists = new Hashtable();


                try
                {
                    Lists = _api.GetClientLists(moMailConfig["ApiKey"], moMailConfig["ClientID"]);
                    //ss
                    //for (var i = 0; i <= Information.UBound(Lists as object[]); i++)
                    //{
                    //    //List = Lists(i);ss
                    //    //if (!hLists.ContainsKey(List.Name)) ss
                    //    //    hLists.Add(List.Name, List.ListID); ss
                    //}

                    if (hLists[ListName] == null)
                    {
                        _api.CreateList(moMailConfig["ApiKey"], moMailConfig["ClientID"], ListName, "", false, "");

                        hLists = new Hashtable();
                        Lists = _api.GetClientLists(moMailConfig["ApiKey"], moMailConfig["ClientID"]);
                        //ss
                        //for (var i = 0; i <= Information.UBound(Lists as object[]); i++)
                        //{
                        //    //List = Lists(i);ss
                        //    //if (!hLists.ContainsKey(List.Name))ss
                        //    //    hLists.Add(List.Name, List.ListID); ss
                        //}
                    }

                    return hLists[ListName].ToString();
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "getListId", ex, "", "", gbDebug);
                    return null;
                }
            }

            public void maintainUserInGroup(long nUserId, long nGroupId, bool remove, string cUserEmail = null, string cGroupName = null, bool isLast = true)
            {
                stdTools.PerfMon.Log("Messaging", "maintainUserInGroup");

                XmlDocument oUserXml = new XmlDocument();
                XmlDocument oGroupXml = new XmlDocument();
                string fullName;
                string email = null;

                try
                {

                    // Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                    if (BulkSubscribes.Count == 0)
                    {
                        string sUserXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, nUserId);
                        if (sUserXml != "")
                        {
                            oUserXml.LoadXml(sUserXml);
                            if (oUserXml.DocumentElement.SelectSingleNode("/tblDirectory/cDirXml/User") != null)
                            {
                                fullName = oUserXml.DocumentElement.SelectSingleNode("/tblDirectory/cDirXml/User/FirstName").InnerText + " " + oUserXml.DocumentElement.SelectSingleNode("/tblDirectory/cDirXml/User/LastName").InnerText;
                                email = oUserXml.DocumentElement.SelectSingleNode("/tblDirectory/cDirXml/User/Email").InnerText;
                            }
                        }
                    }
                    else
                    {
                        fullName = BulkSubscribes[0].FullName;
                        email = BulkSubscribes[0].Email;
                    }

                    if (email != null)
                    {
                        string groupName = myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, nGroupId);
                        // test that is group

                        if (!groupName.StartsWith("CM"))
                        {
                            // If the list doesn't exist create it
                            string CmListID = getListId(groupName);
                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(oUserXml.DocumentElement.SelectSingleNode("/tblDirectory/cDirXml/User/FirstName").InnerText);
                            if (!remove)
                                BulkSubscribes.Add(new ListSubscribe() { Add = true, CmListID = CmListID, oUser = doc.DocumentElement });
                            else
                                BulkSubscribes.Add(new ListSubscribe() { Add = false, CmListID = CmListID, oUser = doc.DocumentElement });
                        }
                    }

                    if (isLast)
                    {
                        CampaignMonitorAPIWrapper.CampaignMonitorAPI.api _api = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.api();
                        Int32 totalInstances = BulkSubscribes.Count;

                        SyncMember Tasks = new SyncMember();
                        Int32 workerThreads;
                        Int32 compPortThreads;
                        System.Threading.ThreadPool.GetMaxThreads(out workerThreads, out compPortThreads);
                        System.Threading.ThreadPool.SetMaxThreads(workerThreads, compPortThreads);
                        System.Threading.ManualResetEvent[] doneEvents = new System.Threading.ManualResetEvent[totalInstances + 1];
                        Int32 i = 0;

                        foreach (var subscribe in BulkSubscribes)
                        {
                            if (subscribe.Add)
                            {
                                SyncMember.MemberObj stateObj = new SyncMember.MemberObj();

                                stateObj.oUserElmt = subscribe.oUser;
                                stateObj.myWeb = myWeb;
                                stateObj.CmListID = subscribe.CmListID;
                                stateObj.CMBouncedListId = CMBouncedListId;
                                stateObj.CMDeletedListId = CMDeletedListId;
                                stateObj.CMSupressionListId = CMSupressionListId;
                                stateObj.CMUnconfirmedListId = CMUnconfirmedListId;
                                stateObj.CMUnSubscribedListId = CMUnSubscribedListId;
                                //ss System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(Tasks.SyncSingleMember), stateObj);
                                stateObj = null;
                                i = i + 1;
                            }
                            else
                                _api.Unsubscribe(moMailConfig["ApiKey"], subscribe.CmListID, subscribe.oUser.SelectSingleNode("cDirXml/User/Email").InnerText);
                        }

                        BulkSubscribes.Clear();
                    }
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "maintainUserInGroup", ex, "", "", gbDebug);
                }
            }
        }

        public class ListSubscribe
        {
            private bool _add;
            private string _CmListID;
            private string _email;
            private string _fullname;
            private XmlElement _userXml;

            public bool Add
            {
                get
                {
                    return _add;
                }
                set
                {
                    _add = value;
                }
            }

            public string CmListID
            {
                get
                {
                    return _CmListID;
                }
                set
                {
                    _CmListID = value;
                }
            }

            public string Email
            {
                get
                {
                    return _email;
                }
                set
                {
                    _email = value;
                }
            }

            public string FullName
            {
                get
                {
                    return _fullname;
                }
                set
                {
                    _fullname = value;
                }
            }

            public XmlElement oUser
            {
                get
                {
                    return _userXml;
                }
                set
                {
                    _userXml = value;
                }
            }
        }

        public class Activities : Protean.Messaging
        {
            public new bool SendMailToList_Queued(int nPageId, string cEmailXSL, string cGroups, string cFromEmail, string cFromName, string cSubject)
            {
                stdTools.PerfMon.Log("Messaging", "SendMailToList_Queued");

                string cProcessInfo = "";

                try
                {
                    System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                    UserEmailDictionary oAddressDic = GetGroupEmails(cGroups);
                    // max number of bcc

                    Cms oWeb = new Cms();
                    oWeb.InitializeVariables();
                    oWeb.mnPageId = nPageId;
                    oWeb.mbAdminMode = false;

                    oWeb.mcEwSiteXsl = cEmailXSL;
                    // get body
                    oWeb.mnMailMenuId = long.Parse(moMailConfig["RootPageId"]);
                    string oEmailBody = oWeb.ReturnPageHTML(oWeb.mnPageId);

                    int i2 = 0;

                    System.Net.Mail.MailMessage oEmail = null;
                    //string cRepientMail;

                    cFromEmail = cFromEmail.Trim();

                    if (Protean.Tools.Text.IsEmail(cFromEmail))
                    {
                        foreach (string cRepientMail in oAddressDic.Keys)
                        {
                            // create the message
                            if (oEmail == null)
                            {
                                oEmail = new System.Net.Mail.MailMessage();
                                oEmail.IsBodyHtml = true;
                                cProcessInfo = "Sending from: " + cFromEmail;
                                oEmail.From = new System.Net.Mail.MailAddress(cFromEmail, cFromName);
                                oEmail.Body = oEmailBody;
                                oEmail.Subject = cSubject;
                            }
                            // if we are not at the bcc limit then we add the addres
                            if (i2 < System.Convert.ToInt32(moMailConfig["BCCLimit"]))
                            {
                                if (Protean.Tools.Text.IsEmail(cRepientMail.Trim()))
                                {
                                    cProcessInfo = "Sending to: " + cRepientMail.Trim();
                                    oEmail.Bcc.Add(new System.Net.Mail.MailAddress(cRepientMail.Trim()));
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
                        if (i2 < System.Convert.ToInt32(moMailConfig["BCCLimit"]))
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
                        return false;
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "SendMailToList_Queued", ex, "", cProcessInfo, gbDebug);
                    return false;
                }
            }

            public new string SendQueuedMail(System.Net.Mail.MailMessage oMailn, string cHost, string cPickupLocation)
            {
                stdTools.PerfMon.Log("Messaging", "SendQueuedMail");
                try
                {
                    if (oMailn == null)
                        return "No Email Supplied";
                    SmtpClient oSmtpn = new SmtpClient();
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
                    //returnException(mcModuleName, "SendQueuedMail", ex, "", "", gbDebug);
                    return "Error";
                }
            }

            public bool AddToList(string ListId, string name, string email, System.Collections.Generic.Dictionary<string, string> values)
            {
                stdTools.PerfMon.Log("Activities", "AddToList");
                string cProcessInfo = "";
                int customCount = 45;
                try
                {
                    System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");

                    // do nothing this is a placeholder
                    // We have an Xform within this content we need to process.
                    string apiKey = moMailConfig["ApiKey"];
                    createsend_dotnet.ApiKeyAuthenticationDetails cmAuth = new createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig["ApiKey"]);
                    createsend_dotnet.Subscriber subscriber = new createsend_dotnet.Subscriber(cmAuth, ListId);
                    List<createsend_dotnet.SubscriberCustomField> customFields = new List<createsend_dotnet.SubscriberCustomField>();

                    createsend_dotnet.List list = new createsend_dotnet.List(cmAuth, ListId);

                    foreach (var oElmt in values)
                    {
                        if (!(oElmt.Key == "Name" | oElmt.Key == "Email"))
                        {
                            // scan the custom feilds to check exists
                            if (customCount > 0)
                            {
                                bool cfExists = false;
                                foreach (var cf in list.CustomFields())
                                {
                                    if (cf.FieldName == oElmt.Key)
                                        cfExists = true;
                                }
                                // if not exists create it
                                if (!cfExists)
                                {
                                    System.Collections.Generic.List<string> opts = new System.Collections.Generic.List<string>();
                                    createsend_dotnet.CustomFieldDataType dt = createsend_dotnet.CustomFieldDataType.Text;
                                    if ((oElmt.Key).ToLower().Contains("date"))
                                        dt = createsend_dotnet.CustomFieldDataType.Date;
                                    try
                                    {
                                        list.CreateCustomField(oElmt.Key, dt, opts, true);
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }

                                createsend_dotnet.SubscriberCustomField customField = new createsend_dotnet.SubscriberCustomField();
                                customField.Key = oElmt.Key;
                                customField.Value = oElmt.Value;
                                customFields.Add(customField);
                            }
                            customCount = customCount - 1;
                        }
                    }

                    try
                    {
                        subscriber.Add(email, name, customFields, true, true);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("205"))
                            subscriber.Update(email, email, name, customFields, true, true);
                        cProcessInfo = ex.Message;
                    }


                    return true;
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "AddToList", ex, "", "", gbDebug);
                    return false;
                }
            }

            public bool RemoveFromList(string ListId, string Email)
            {

                stdTools.PerfMon.Log("Activities", "RemoveFromList");
                try
                {
                    System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");

                    string apiKey = moMailConfig["ApiKey"];
                    createsend_dotnet.ApiKeyAuthenticationDetails cmAuth = new createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig["ApiKey"]);
                    createsend_dotnet.Subscriber subscriber = new createsend_dotnet.Subscriber(cmAuth, ListId);
                    // do nothing this is a placeholder

                    subscriber.Delete(Email);

                    return true;
                }
                catch (Exception ex)
                {
                    // returnException(mcModuleName, "RemoveFromList", ex, "", "", gbDebug)
                    return false;
                }
            }
        }

        public class SyncMember
        {
            private const string mcModuleName = "Providers.Messaging.Generic.AdminXForms";
            private System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");

            public class MemberObj
            {
                public XmlElement oUserElmt;
                public string CmListID;
                public long CMSupressionListId;
                public long CMDeletedListId;
                public long CMUnSubscribedListId;
                public long CMBouncedListId;
                public long CMUnconfirmedListId;
                public Protean.Cms myWeb;
                public string EwGroupID;
                public bool UnSubRemove = false;
            }

            public void SyncSingleMember(MemberObj Member)
            {
                string cTableName = "";
                string cTableKey = "";
                string cTableFRef = "";
                string cProcessInfo = "";
                string fullName = "";
                string Email = "";
                long nUserId = 0;
                try
                {
                    dbHelper localDbHelper = new dbHelper(ref Member.myWeb);
                    Int16 memberStatus = 1;

                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.api _api = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.api();
                    if (Member.oUserElmt != null)
                    {
                        if (Member.oUserElmt.SelectSingleNode("cDirXml") != null)
                        {
                            fullName = Member.oUserElmt.SelectSingleNode("cDirXml/User/FirstName").InnerText + " " + Member.oUserElmt.SelectSingleNode("cDirXml/User/LastName").InnerText;
                            nUserId = System.Convert.ToInt64(Member.oUserElmt.SelectSingleNode("nDirKey").InnerText);
                            Email = Member.oUserElmt.SelectSingleNode("cDirXml/User/Email").InnerText;
                            if (Member.oUserElmt.SelectSingleNode("cDirXml/User/Status") == null)
                                memberStatus = 1;
                            else
                                memberStatus = System.Convert.ToInt16(Member.oUserElmt.SelectSingleNode("cDirXml/User/Status").InnerText);
                        }
                        else
                        {
                            fullName = Member.oUserElmt.SelectSingleNode("descendant-or-self::FirstName").InnerText + " " + Member.oUserElmt.SelectSingleNode("descendant-or-self::LastName").InnerText;
                            nUserId = System.Convert.ToInt64(Member.oUserElmt.GetAttribute("id"));
                            Email = Member.oUserElmt.SelectSingleNode("descendant-or-self::Email").InnerText;
                            if (Member.oUserElmt.SelectSingleNode("descendant-or-self::Status") == null)
                                memberStatus = 1;
                            else
                                memberStatus = System.Convert.ToInt16(Member.oUserElmt.SelectSingleNode("descendant-or-self::Status").InnerText);
                        }
                    }

                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.Result result = new CampaignMonitorAPIWrapper.CampaignMonitorAPI.Result();

                    switch (memberStatus)
                    {
                        case 1:
                        case -1:
                            {
                                result = _api.AddSubscriber(moMailConfig["ApiKey"], Member.CmListID, Email, fullName);
                                break;
                            }

                        case 0:
                            {
                                result = _api.Unsubscribe(moMailConfig["ApiKey"], Member.CmListID, Email);
                                break;
                            }
                    }

                    switch (result.Code)
                    {
                        case 0 // Success
                       :
                            {
                                break;
                            }

                        case 1 // Invalid email address
               :
                            {
                                break;
                            }

                        case 100 // Invalid API Key
               :
                            {
                                break;
                            }

                        case 101 // Invalid(ListID)
               :
                            {
                                break;
                            }

                        case 204 // In Suppression List
               :
                            {
                                _api.AddAndResubscribe(moMailConfig["ApiKey"], Member.CmListID, Email, fullName);
                                localDbHelper.maintainDirectoryRelation(Member.CMSupressionListId, nUserId, false/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, true, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, false);
                                break;
                            }

                        case 205 // Is Deleted
                 :
                            {
                                localDbHelper.maintainDirectoryRelation(Member.CMDeletedListId, nUserId, false/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, true, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, false);
                                break;
                            }

                        case 206 // Is Unsubscribed
                 :
                            {
                                if (Member.UnSubRemove)
                                {
                                    localDbHelper.maintainDirectoryRelation(long.Parse(Member.EwGroupID), nUserId, true);
                                    localDbHelper.maintainDirectoryRelation(Member.CMUnSubscribedListId, nUserId, false/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, true, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, false);
                                }
                                else
                                {
                                    _api.AddAndResubscribe(moMailConfig["ApiKey"], Member.CmListID, Email, fullName);
                                    localDbHelper.maintainDirectoryRelation(Member.CMUnSubscribedListId, nUserId, false/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, true, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, false);
                                }

                                break;
                            }

                        case 207 // Is Bounced
                 :
                            {
                                localDbHelper.maintainDirectoryRelation(Member.CMBouncedListId, nUserId, false/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, true, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, false);
                                break;
                            }

                        case 208 // Is Unconfirmed
                 :
                            {
                                localDbHelper.maintainDirectoryRelation(Member.CMUnconfirmedListId, nUserId, false/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, true, null/* Conversion error: Set to default value for this argument */, null/* Conversion error: Set to default value for this argument */, false);
                                break;
                            }
                    }
                    localDbHelper.CloseConnection();
                    localDbHelper = null/* TODO Change to default(_) if this is not a reference type */;

                    cProcessInfo = result.GetType().ToString();
                }

                catch (Exception ex)
                {
                    //returnException(mcModuleName, "SyncSingleMember", ex, "", cProcessInfo, gbDebug);
                }
            }
        }

        public class AddListStats
        {
            private const string mcModuleName = "Providers.Messaging.Generic.AdminXForms";
            private System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");

            public class ListObj
            {
                public XmlElement oMasterList;
                public string CmListID;
                // Public finished As CountdownEvent
                public CampaignMonitorAPIWrapper.CampaignMonitorAPI.api API;
            }

            public void UpdateListStats(ListObj MyList)
            {
                string cProcessInfo = "Updating stats for ListID: " + MyList.CmListID;
                try
                {
                    CampaignMonitorAPIWrapper.CampaignMonitorAPI.ListStatistics ls = (CampaignMonitorAPIWrapper.CampaignMonitorAPI.ListStatistics)MyList.API.GetListStats(moMailConfig["ApiKey"], MyList.CmListID);
                    XmlElement listElmt = (XmlElement)MyList.oMasterList.SelectSingleNode(("List[@id='" + MyList.CmListID + "']"));
                    listElmt.SetAttribute("subscribers", (ls.TotalActiveSubscribers).ToString());
                    listElmt.SetAttribute("unsubscribes", (ls.TotalUnsubscribes).ToString());
                    listElmt.SetAttribute("deleted", (ls.TotalDeleted).ToString());
                    listElmt.SetAttribute("bounces", (ls.TotalBounces).ToString());
                }
                catch (Exception ex)
                {
                    //returnException(mcModuleName, "UpdateListStats", ex, "", cProcessInfo, gbDebug);
                }
                finally
                {
                }
            }
        }
    }
}

