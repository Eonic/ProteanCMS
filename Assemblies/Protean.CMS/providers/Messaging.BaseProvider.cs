// ***********************************************************************
// $Library:     Protean.Providers.messaging.base
// $Revision:    3.1  
// $Date:        2010-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
// ***********************************************************************

using System;
using System.Collections;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Configuration;

using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.Cms;
using static Protean.stdTools;
using Protean.Tools;
using System.Dynamic;

namespace Protean.Providers
{
    namespace Messaging
    {

        public class BaseProvider: DynamicObject
        {
            private Admin.AdminXforms _AdminXforms;
            private object _AdminProcess;
            private object _Activities;
            private const string mcModuleName = "Protean.Providers.Messaging";


            public object AdminXforms
            {
                set
                {
                    _AdminXforms = value;
                }
                get
                {
                    return _AdminXforms;
                }
            }

            public object AdminProcess
            {
                set
                {
                    _AdminProcess = value;
                }
                get
                {
                    return _AdminProcess;
                }
            }

            public object Activities
            {
                set
                {
                    _Activities = value;
                }
                get
                {
                    return _Activities;
                }
            }

            public BaseProvider(ref Cms myWeb, string ProviderName)
            {
                string cProgressInfo = "";
                try
                {
                    Type calledType;
                    if (string.IsNullOrEmpty(ProviderName))
                    {
                        ProviderName = "Protean.Providers.Messaging.EonicProvider";
                        calledType = Type.GetType(ProviderName, true);
                    }
                    else
                    {
                        var castObject = WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders");
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                        System.Configuration.ProviderSettings ourProvider = moPrvConfig.Providers[ProviderName];
                        Assembly assemblyInstance;
                        // = [Assembly].Load(moPrvConfig.Providers(ProviderName).Type)

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(ourProvider.Parameters["path"], "", false)))
                        {
                            cProgressInfo = goServer.MapPath(Conversions.ToString(ourProvider.Parameters["path"]));
                            assemblyInstance = Assembly.LoadFrom(goServer.MapPath(Conversions.ToString(ourProvider.Parameters["path"])));
                        }
                        else
                        {
                            assemblyInstance = Assembly.Load(ourProvider.Type);
                        }

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(ourProvider.Parameters["className"], "", false)))
                        {
                            ProviderName = Conversions.ToString(ourProvider.Parameters["className"]);
                        }

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(ourProvider.Parameters["rootClass"], "", false)))
                        {
                            calledType = assemblyInstance.GetType("Protean.Providers.Messaging." + ProviderName, true);
                        }
                        else
                        {
                            // calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & ".Providers.Messaging", True)
                            calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(ourProvider.Parameters["rootClass"], ".Providers.Messaging."), ProviderName)), true);
                        }
                    }

                    var o = Activator.CreateInstance(calledType);

                    var args = new object[5];
                    args[0] = _AdminXforms;
                    args[1] = _AdminProcess;
                    args[2] = _Activities;
                    args[3] = this;
                    args[4] = myWeb;

                    calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, vstrFurtherInfo: cProgressInfo + " - " + ProviderName + " Could Not be Loaded", bDebug: gbDebug);
                }

            }
        }

        public class EonicProvider
        {

            public EonicProvider()
            {
                // do nothing
            }

            public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, ref object MemProvider, ref Cms myWeb)
            {

                MemProvider.AdminXforms = new AdminXForms(ref myWeb);
                MemProvider.AdminProcess = new AdminProcess(ref myWeb);
                MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms;
                // MemProvider.Activities = New Activities()


            }

            public class AdminXForms : Admin.AdminXforms
            {
                private const string mcModuleName = "Providers.Messaging.Generic.AdminXForms";

                public AdminXForms(ref Cms aWeb) : base(ref aWeb)
                {
                }

                public XmlElement xFrmPreviewNewsLetter(int nPageId, ref XmlElement oPageDetail, string cSubject = "")
                {
                    XmlElement oFrmElmt;

                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("SendNewsLetter");

                        base.submission("SendNewsLetter", "", "post", "");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Unpersonalised", "", "Send Unpersonalised");

                        XmlElement oElmt;
                        XmlElement oxmlBindelmt = null;
                        oElmt = base.addInput(ref oFrmElmt, "cEmail", true, "Email address to send to", "long");
                        base.addBind("cEmail", "cEmail", ref oxmlBindelmt);
                        oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"));
                        base.addSubmit(ref oFrmElmt, "SendUnpersonalised", "Send Unpersonalised");

                        // Uncomment for personalised
                        // ''oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Personalised", "", "Send Personalised")
                        // ''Dim cSQL As String = "SELECT tblDirectory.nDirKey, tblDirectory.cDirName" & _
                        // ''" FROM tblDirectory INNER JOIN" & _
                        // ''" tblDirectoryRelation ON tblDirectory.nDirKey = tblDirectoryRelation.nDirChildId INNER JOIN" & _
                        // ''" tblDirectory Role ON tblDirectoryRelation.nDirParentId = Role.nDirKey" & _
                        // ''" WHERE (tblDirectory.cDirSchema = N'User') AND (Role.cDirSchema = N'Role') AND (Role.cDirName = N'Administrator')" & _
                        // ''" ORDER BY tblDirectory.cDirName"
                        // ''Dim oDre As SqlDataReader = moDbhelper.getDataReader(cSQL)
                        // ''Dim oSelElmt As XmlElement = MyBase.addSelect1(oFrmElmt, "cUsers", True, "Select admin user to send to", "short", ApperanceTypes.Minimal)
                        // ''Do While oDre.Read
                        // ''    MyBase.addOption(oSelElmt, oDre(1), oDre(0))
                        // ''Loop
                        // ''MyBase.addBind("cUsers", "cUsers")
                        // ''MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

                        base.Instance.InnerXml = "<cEmail/><cUsers/>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            XmlElement oEmailElmt = (XmlElement)base.Instance.SelectSingleNode("cEmail");
                            bool localis_valid_email() { string argstr_Renamed = oEmailElmt.InnerText; var ret = Protean.Tools.Text.IsEmail(argstr_Renamed); oEmailElmt.InnerText = argstr_Renamed; return ret; }

                            if (!localis_valid_email())
                            {
                                base.addNote(oElmt.ToString(), Protean.xForm.noteTypes.Alert, "Incorrect Email Address Supplied");
                                base.valid = false;
                            }
                            if (base.valid)
                            {
                                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                                string cEmail = base.Instance.SelectSingleNode("cEmail").InnerText;
                                // first we will only deal with unpersonalised
                                var oMessager = new Protean.Messaging(ref myWeb.msException);
                                // get the subject
                                var oMessaging = new Activities(ref myWeb.msException);

                                string cMailingXsl = moMailConfig["MailingXsl"];
                                if (string.IsNullOrEmpty(cMailingXsl))
                                    cMailingXsl = "/xsl/mailer/mailerStandard.xsl";
                                if (myWeb.moConfig["cssFramework"] == "bs5")
                                    cMailingXsl = "/features/mailer/mailer-core.xsl";
                                var ofs = new Protean.fsHelper(myWeb.moCtx);
                                cMailingXsl = ofs.checkCommonFilePath(cMailingXsl);



                                if (oMessaging.SendSingleMail_Direct(nPageId, cMailingXsl, cEmail, moMailConfig["SenderEmail"], moMailConfig["SenderName"], cSubject))
                                {
                                    // add mssage and return to form so they can sen another

                                    var oMsgElmt = oPageDetail.OwnerDocument.CreateElement("Content");
                                    oMsgElmt.SetAttribute("type", "Message");
                                    oMsgElmt.InnerText = "Messages Sent";
                                    oPageDetail.AppendChild(oMsgElmt);
                                }
                                else
                                {
                                    var oMsgElmt = oPageDetail.OwnerDocument.CreateElement("Content");
                                    oMsgElmt.SetAttribute("type", "Message");
                                    oMsgElmt.InnerText = "Message Failed Check Config";
                                    oPageDetail.AppendChild(oMsgElmt);
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmSendNewsLetter(int nPageId, string cPageName, string cDefaultEmail, string cDefaultEmailName, ref XmlElement oPageDetail)
                {
                    XmlElement oFrmElmt;
                    XmlElement oCol1;
                    XmlElement oCol2;

                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("SendNewsLetter");

                        base.submission("SendNewsLetter", "", "post", "");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Groups", "2col", "Please select a group(s) to send to.");

                        cDefaultEmail = Strings.Trim(cDefaultEmail);

                        oCol1 = base.addGroup(ref oFrmElmt, "", "col1", "");
                        oCol2 = base.addGroup(ref oFrmElmt, "", "col2", "");

                        XmlElement oElmt;
                        XmlElement oBindEmlt = null;
                        oElmt = base.addInput(ref oCol1, "cDefaultEmail", true, "Email address to send from", "required long");
                        base.addBind("cDefaultEmail", "cDefaultEmail", ref oBindEmlt, "true()");
                        oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"));

                        XmlElement oElmt2;
                        oElmt2 = base.addInput(ref oCol1, "cDefaultEmailName", true, "Name to send from", "required long");
                        base.addBind("cDefaultEmailName", "cDefaultEmailName", ref oBindEmlt, "true()");
                        oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"));

                        oElmt2 = base.addInput(ref oCol1, "cSubject", true, "Subject", "required long");
                        base.addBind("cSubject", "cSubject", ref oBindEmlt, "true()");
                        oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"));


                        string cSQL = "SELECT nDirKey, cDirName  FROM tblDirectory WHERE (cDirSchema = 'Group') ORDER BY cDirName";
                        using (SqlDataReader oDre = moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                        {
                            XmlElement oSelElmt = base.addSelect(ref oCol2, "cGroups", true, "Select Groups to send to", "required multiline", ApperanceTypes.Full);
                            while (oDre.Read())
                                base.addOption(ref oSelElmt, oDre[1].ToString(), oDre[0].ToString());
                        }
                        base.addBind("cGroups", "cGroups", ref oBindEmlt, "true()");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Send", "", "");

                        base.addSubmit(ref oFrmElmt, "SendUnpersonalised", "Send Unpersonalised");
                        // Uncomment for personalised
                        // MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

                        base.Instance.InnerXml = "<cGroups/><cDefaultEmail>" + cDefaultEmail + "</cDefaultEmail><cDefaultEmailName>" + cDefaultEmailName + "</cDefaultEmailName><cSubject>" + cPageName + "</cSubject>";

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            XmlElement oEmailElmt = (XmlElement)base.Instance.SelectSingleNode("cDefaultEmail");
                            if (!Text.IsEmail(oEmailElmt.InnerText.Trim()))
                            {
                                base.addNote(oElmt.ToString(), Protean.xForm.noteTypes.Alert, "Incorrect Email Address Supplied");
                                base.valid = false;
                            }
                            if (base.valid)
                            {
                                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                                // get the individual elements
                                var oMessaging = new Protean.Messaging(ref myWeb.msException);
                                // First we need to get the groups we are sending to
                                XmlElement oGroupElmt = (XmlElement)base.Instance.SelectSingleNode("cGroups");
                                XmlElement oFromEmailElmt = (XmlElement)base.Instance.SelectSingleNode("cDefaultEmail");
                                XmlElement oFromNameElmt = (XmlElement)base.Instance.SelectSingleNode("cDefaultEmailName");
                                XmlElement oSubjectElmt = (XmlElement)base.Instance.SelectSingleNode("cSubject");
                                // get the email addresses for these groups
                                string cMailingXsl = moMailConfig["MailingXsl"];
                                if (string.IsNullOrEmpty(cMailingXsl))
                                    cMailingXsl = "/xsl/mailer/mailerStandard.xsl";
                                if (myWeb.moConfig["cssFramework"] == "bs5")
                                    cMailingXsl = "/features/mailer/mailer-core.xsl";
                                var ofs = new Protean.fsHelper(myWeb.moCtx);
                                cMailingXsl = ofs.checkCommonFilePath(cMailingXsl);

                                bool bResult = oMessaging.SendMailToList_Queued(nPageId, cMailingXsl, oGroupElmt.InnerText, oFromEmailElmt.InnerText, oFromNameElmt.InnerText, oSubjectElmt.InnerText);


                                // Log the result
                                if (bResult)
                                {
                                    // moDbHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, nPageId, , oGroupElmt.InnerText)
                                    moDbHelper.CommitLogToDB(dbHelper.ActivityType.NewsLetterSent, myWeb.mnUserId, myWeb.moSession.SessionID, DateTime.Now, myWeb.mnPageId, 0, "", true);
                                    string cGroupStr = "<Groups><Group>" + Strings.Replace(oGroupElmt.InnerText, ",", "</Group><Group>") + "</Group></Groups>";
                                    // add mssage and return to form so they can sen another
                                    var oMsgElmt = oPageDetail.OwnerDocument.CreateElement("Content");
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
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmAddModule(long pgid, string position)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string sImgPath = "";

                    string cProcessInfo = "";
                    var oXformDoc = new XmlDocument();
                    try
                    {

                        if (moRequest["cModuleBox"] != "")
                        {

                            xFrmEditContent(0, "Module/" + moRequest["cModuleType"], pgid, moRequest["cPosition"]);
                            return base.moXformElmt;
                        }

                        else
                        {
                            XmlElement oBindParentElmt = null;
                            base.NewFrm("EditPageLayout");
                            base.submission("AddModule", "", "post", "form_check(this)");
                            base.Instance.InnerXml = "<Module position=\"" + position + "\"></Module>";
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Add Module", "", "Select Module Type");
                            base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                            base.addInput(ref oFrmElmt, "cPosition", true, "Position", "hidden");
                            base.addBind("cPosition", "Module/@position", ref oBindParentElmt, "true()");
                            base.addNote(oFrmElmt.ToString(), Protean.xForm.noteTypes.Hint, "Click the image to select Module Type");

                            oSelElmt = base.addSelect1(ref oFrmElmt, "cModuleType", true, "", "PickByImage", Protean.xForm.ApperanceTypes.Full);

                            EnumberateManifestOptions(ref oSelElmt, "/xsl/Mailer", "ModuleTypes/ModuleGroup", "Module", true);
                            EnumberateManifestOptions(ref oSelElmt, "/ewcommon/xsl/Mailer", "ModuleTypes/ModuleGroup", "Module", false);

                            if (base.isSubmitted() | goRequest.Form["ewsubmit.x"] != "" | goRequest.Form["cModuleType"] != "")
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                if (base.valid)
                                {
                                    // Do nothing
                                    // or redirect to content form
                                    xFrmEditContent(0, "Module/" + moRequest["cModuleType"], pgid, moRequest["cPosition"]);
                                }
                            }
                            base.addValues();
                            return base.moXformElmt;
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

            }

            public class AdminProcess : Admin
            {

                private AdminXForms _oAdXfm;

                public object oAdXfm
                {
                    set
                    {
                        _oAdXfm = (AdminXForms)value;
                    }
                    get
                    {
                        return _oAdXfm;
                    }
                }

                public AdminProcess(ref Cms aWeb) : base(ref aWeb)
                {
                }

                public string MailingListProcess(ref XmlElement oPageDetail, ref Cms oWeb, [Optional, DefaultParameterValue("")] ref string sAdminLayout, [Optional, DefaultParameterValue("")] ref string cCmd, [Optional, DefaultParameterValue(false)] ref bool bLoadStructure, [Optional, DefaultParameterValue("")] ref string sEditContext, bool bClearEditContext = false)
                {
                    string cRetVal = "";
                    string cSQL = "";

                    try
                    {
                        System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                        if (moMailConfig is null)
                            return "";

                        long nMailMenuRoot = Conversions.ToLong(moMailConfig["RootPageId"]);
                        // if we hit this we want to default to the mail root Id
                        if (myWeb.mnPageId == gnTopLevel & cCmd != "NewMail" & cCmd != "NormalMail" & cCmd != "AdvancedMail")
                        {
                            myWeb.mnPageId = Convert.ToInt32(nMailMenuRoot);
                            if (!string.IsNullOrEmpty(cCmd))
                            {
                                // skip image for submissions
                                cCmd = "MailingList";
                            }
                        }

                        string cMailingXsl = moMailConfig["MailingXsl"];
                        if (string.IsNullOrEmpty(cMailingXsl))
                            cMailingXsl = "/xsl/mailer/mailerStandard.xsl";
                        if (myWeb.moConfig["cssFramework"] == "bs5")
                            cMailingXsl = "/features/mailer/mailer-core.xsl";
                        var ofs = new Protean.fsHelper(myWeb.moCtx);
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
                            {
                                oPageDetail.OwnerDocument.DocumentElement.SetAttribute("adminMode", "false");
                            }
                        }

                    ProcessFlow:
                        ;

                        switch (cCmd ?? "")
                        {
                            case "MailingList":
                                {
                                    sAdminLayout = "MailingList";
                                    myWeb.mcEwSiteXsl = cMailingXsl;
                                    bLoadStructure = false;

                                    int nNewsletterRoot = Conversions.ToInteger("0" + moMailConfig["RootPageId"]);
                                    if (!myWeb.moDbHelper.checkPageExist(nNewsletterRoot))
                                        nNewsletterRoot = 0;

                                    if (nNewsletterRoot == 0)
                                    {
                                        string defaultPageXml = "<DisplayName title=\"\" linkType=\"internal\" exclude=\"false\" noindex=\"false\"/><Images><img class=\"icon\" /><img class=\"thumbnail\" /><img class=\"detail\" /></Images><Description/>";
                                        nNewsletterRoot = Convert.ToInt32(myWeb.moDbHelper.insertStructure(0, "", "Newsletters", defaultPageXml, "NewsletterRoot"));
                                        Protean.Config.UpdateConfigValue(ref myWeb, "protean/mailinglist", "RootPageId", nNewsletterRoot.ToString());

                                    }

                                    // we want to return here after editing
                                    myWeb.moSession["lastPage"] = myWeb.mcOriginalURL;
                                    break;
                                }

                            case "NormalMail":
                                {
                                    sAdminLayout = "";
                                    cCmd = "NormalMail";

                                    myWeb.mcEwSiteXsl = cMailingXsl;
                                    // we want to return here after editing
                                    if (!myWeb.mbPopupMode)
                                    {
                                        if (!myWeb.mbSuppressLastPageOverrides)
                                            myWeb.moSession["lastPage"] = "?ewCmd=NormalMail&pgid=" + myWeb.mnPageId; // myWeb.mcOriginalURL 'not this if being redirected after editing layout for instance.
                                    }

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
                                    {
                                        nPage = 0;
                                    }
                                    else
                                    {
                                        nPage =Convert.ToInt32(myWeb.moRequest["pgid"]);
                                    }
                                    oPageDetail.AppendChild(oAdXfm.xFrmEditPage(nPage, myWeb.moRequest["name"], "Mail"));
                                    if (Conversions.ToBoolean(oAdXfm.valid))
                                    {
                                        if (cCmd == "NewMail")
                                        {
                                            cCmd = "MailingList";
                                        }
                                        else
                                        {
                                            cCmd = "NormalMail";
                                        }
                                        oPageDetail.RemoveAll();
                                        goto ProcessFlow;
                                    }

                                    break;
                                }

                            case "AdvancedMail":
                                {
                                    sAdminLayout = "Advanced";

                                    var oCommonContentTypes = new XmlDocument();
                                    if (System.IO.File.Exists(myWeb.goServer.MapPath("/ewcommon/xsl/mailer/layoutmanifest.xml")))
                                        oCommonContentTypes.Load(myWeb.goServer.MapPath("/ewcommon/xsl/pagelayouts/layoutmanifest.xml"));
                                    if (System.IO.File.Exists(myWeb.goServer.MapPath(moConfig["ProjectPath"] + "/xsl/mailer/layoutmanifest.xml")))
                                    {
                                        var oLocalContentTypes = new XmlDocument();
                                        oLocalContentTypes.Load(myWeb.goServer.MapPath(moConfig["ProjectPath"] + "/xsl/mailer/layoutmanifest.xml"));
                                        XmlElement oLocals = (XmlElement)oLocalContentTypes.SelectSingleNode("/PageLayouts/ContentTypes");
                                        if (oLocals != null)
                                        {
                                            foreach (XmlElement oGrp in oLocals.SelectNodes("ContentTypeGroup"))
                                            {
                                                XmlElement oComGrp = (XmlElement)oCommonContentTypes.SelectSingleNode("/PageLayouts/ContentTypes/ContentTypeGroup[@name='" + oGrp.GetAttribute("name") + "']");
                                                if (oComGrp != null)
                                                {
                                                    foreach (XmlElement oTypeElmt in oGrp.SelectNodes("ContentType"))
                                                    {
                                                        if (oComGrp.SelectSingleNode("ContentType[@type='" + oTypeElmt.GetAttribute("type") + "']") != null)
                                                        {
                                                            oComGrp.SelectSingleNode("ContentType[@type='" + oTypeElmt.GetAttribute("type") + "']").InnerText = oTypeElmt.InnerText;
                                                        }
                                                        else
                                                        {
                                                            oComGrp.InnerXml += oTypeElmt.OuterXml;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    oCommonContentTypes.DocumentElement.SelectSingleNode("ContentTypes").InnerXml += oGrp.OuterXml;
                                                }
                                            }
                                        }
                                    }
                                    // now to add it to the pagexml
                                    oPageDetail.AppendChild(moPageXML.ImportNode(oCommonContentTypes.SelectSingleNode("/PageLayouts/ContentTypes"), true));


                                    if (myWeb.moRequest["pgid"] != "")
                                    {
                                        // lets save the page we are editing to the session
                                        myWeb.moSession["pgid"] = myWeb.moRequest["pgid"];
                                    }
                                    // we want to return here after editing
                                    myWeb.moSession["lastPage"] = myWeb.mcOriginalURL;
                                    break;
                                }

                            case "MailHistory":
                                {
                                    myWeb.moDbHelper.ViewMailHistory(ref oPageDetail);
                                    break;
                                }
                            case "EditMailLayout":
                                {
                                    moAdXfm.goServer = myWeb.goServer;
                                    oPageDetail.AppendChild(oAdXfm.xFrmEditMailLayout(myWeb.moRequest["pgid"]));
                                    if (Conversions.ToBoolean(oAdXfm.valid))
                                    {
                                        cCmd = "NormalMail";
                                        sAdminLayout = "NormalMail";
                                        oPageDetail.RemoveAll();
                                        goto ProcessFlow;
                                    }
                                    else
                                    {
                                        sAdminLayout = "AdminXForm";
                                    }

                                    break;
                                }
                            case "AddMailModule":
                                {
                                    bLoadStructure = true;
                                    nAdditionId = 0;
                                    oPageDetail.AppendChild(oAdXfm.xFrmAddModule(myWeb.moRequest["pgid"], myWeb.moRequest["position"]));
                                    if (Conversions.ToBoolean(oAdXfm.valid))
                                    {
                                        if (myWeb.moRequest["nStatus"] != "")
                                        {
                                            oPageDetail.RemoveAll();
                                            if (myWeb.moSession["lastPage"] != "")
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
                                    {
                                        sAdminLayout = "AdminXForm";
                                    }

                                    break;
                                }
                            case "EditMailContent":
                                {

                                    // Get a version Id if it's passed through.
                                    string cVersionKey = myWeb.moRequest["verId"]+ "";
                                    bClearEditContext = false;
                                    bLoadStructure = true;
                                    if (!Information.IsNumeric(cVersionKey))
                                        cVersionKey = "0";
                                    long nContentId;
                                    nContentId = 0L;
                                    oPageDetail.AppendChild(moAdXfm.xFrmEditContent(Convert.ToInt64(myWeb.moRequest["id"]), "", Convert.ToInt64(myWeb.moRequest["pgid"]), default, default, ref nContentId, default, default, Conversions.ToLong(cVersionKey)));

                                    if (moAdXfm.valid)
                                    {
                                        // bAdminMode = False
                                        sAdminLayout = "";
                                        mcEwCmd = myWeb.moSession["ewCmd"].ToString();

                                        // if we have a parent releationship lets add it
                                        if (myWeb.moRequest["contentParId"] != "" && Information.IsNumeric(myWeb.moRequest["contentParId"]))
                                        {
                                            myWeb.moDbHelper.insertContentRelation(Convert.ToInt32(myWeb.moRequest["contentParId"]), nContentId.ToString());
                                        }
                                        if (myWeb.moRequest["EditXForm"] != "")
                                        {
                                            // bAdminMode = True
                                            sAdminLayout = "AdminXForm";
                                            mcEwCmd = "EditXForm";
                                            oPageDetail = oWeb.GetContentDetailXml(default, Convert.ToInt64(myWeb.moRequest["id"]));
                                        }
                                        else
                                        {
                                            myWeb.mnArtId = 0;
                                            oPageDetail.RemoveAll();

                                            // Check for an optional command to redireect to
                                            if (!string.IsNullOrEmpty("" + myWeb.moRequest["ewRedirCmd"]))
                                            {

                                                myWeb.msRedirectOnEnd = moConfig["ProjectPath"] + "/?ewCmd=" + myWeb.moRequest["ewRedirCmd"];
                                            }

                                            else if (myWeb.moSession["lastPage"] != "")
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
                                    {
                                        sAdminLayout = "AdminXForm";
                                    }

                                    break;
                                }
                            case "PreviewMail":
                                {

                                    string cSubject = "";
                                    XmlElement oLocElmt = (XmlElement)oWeb.GetPageXML().SelectSingleNode("descendant-or-self::MenuItem[@id=" + myWeb.mnPageId + "]");
                                    if (oLocElmt != null)
                                        cSubject = oLocElmt.GetAttribute("name");

                                    oPageDetail.AppendChild(oAdXfm.xFrmPreviewNewsLetter(myWeb.mnPageId, oPageDetail, cSubject));

                                    sAdminLayout = "PreviewMail";
                                    break;
                                }

                            // If moAdXfm.valid Then
                            // Dim cEmail As String = moAdXfm.instance.SelectSingleNode("cEmail").InnerText
                            // 'first we will only deal with unpersonalised
                            // Dim oMessager As New Messaging
                            // 'get the subject
                            // Dim cSubject As String = ""
                            // Dim oMessaging As New Protean.Messaging
                            // If oMessaging.SendSingleMail_Queued(myWeb.mnPageId, moMailConfig("MailingXsl"), cEmail, moMailConfig("SenderEmail"), moMailConfig("SenderName"), cSubject) Then
                            // 'add mssage and return to form so they can sen another

                            // Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")

                            // oMsgElmt.SetAttribute("type", "Message")
                            // oMsgElmt.InnerText = "Messages Sent"
                            // oPageDetail.AppendChild(oMsgElmt)
                            // End If
                            // End If

                            case "SendMail":
                                {
                                    sAdminLayout = "PreviewMail";
                                    // get subject
                                    string cSubject = "";
                                    oWeb.mnMailMenuId = Convert.ToInt64(moMailConfig["RootPageId"]);
                                    XmlElement oLocElmt = (XmlElement)oWeb.GetPageXML().SelectSingleNode("descendant-or-self::MenuItem[@id=" + myWeb.mnPageId + "]");
                                    if (oLocElmt != null)
                                        cSubject = oLocElmt.GetAttribute("name");

                                    oPageDetail.AppendChild(oAdXfm.xFrmSendNewsLetter(myWeb.mnPageId, cSubject, moMailConfig["SenderEmail"], moMailConfig["SenderName"], oPageDetail));
                                    break;
                                }

                            // If moAdXfm.valid Then

                            // 'get the individual elements
                            // Dim oMessaging As New Protean.Messaging
                            // 'First we need to get the groups we are sending to
                            // Dim oGroupElmt As XmlElement = moAdXfm.instance.SelectSingleNode("cGroups")
                            // Dim oFromEmailElmt As XmlElement = moAdXfm.instance.SelectSingleNode("cDefaultEmail")
                            // Dim oFromNameElmt As XmlElement = moAdXfm.instance.SelectSingleNode("cDefaultEmailName")
                            // Dim oSubjectElmt As XmlElement = moAdXfm.instance.SelectSingleNode("cSubject")
                            // 'get the email addresses for these groups

                            // If oMessaging.SendMailToList_Queued(myWeb.mnPageId, moMailConfig("MailingXsl"), oGroupElmt.InnerText, oFromEmailElmt.InnerText, oFromNameElmt.InnerText, oSubjectElmt.InnerText) Then
                            // Dim cGroupStr As String = "<Groups><Group>" & Replace(oGroupElmt.InnerText, ",", "</Group><Group>") & "</Group></Groups>"
                            // myWeb.moDBHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, myWeb.mnPageId, , oGroupElmt.InnerText)
                            // 'add mssage and return to form so they can sen another
                            // Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")
                            // oMsgElmt.SetAttribute("type", "Message")
                            // oMsgElmt.InnerText = "Messages Sent"
                            // oPageDetail.AppendChild(oMsgElmt)
                            // End If
                            // End If

                            case "MailOptOut":
                                {
                                    sAdminLayout = "OptOut";
                                    oPageDetail.AppendChild((XmlNode)oAdXfm.xFrmAdminOptOut);
                                    break;
                                }
                            case "ProcessMailbox":
                                {
                                    break;
                                }
                            // Dim oPop3 As New POP3
                            // oPop3.ReadMail(moMailConfig("Pop3Server"), moMailConfig("Pop3Acct"), moMailConfig("Pop3Pwd"))
                            case "DeletePageMail":
                                {
                                    bLoadStructure = true;
                                    oPageDetail.AppendChild(oAdXfm.xFrmDeletePage(myWeb.moRequest["pgid"]));
                                    if (Conversions.ToBoolean(oAdXfm.valid))
                                    {
                                        myWeb.msRedirectOnEnd = "/?ewCmd=MailingList";
                                    }

                                    // cCmd = "MailingList"
                                    // GoTo ProcessFlow
                                    else
                                    {
                                        sAdminLayout = "AdminXForm";
                                    }

                                    break;
                                }
                            case "SearchMailContent":
                                {
                                    break;
                                }
                                // for searching for content to add
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
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "NewsLetterProcess", ex, "", "", gbDebug);
                        return "";
                    }
                }

                public void MailingListAdminMenu(ref XmlElement oAdminMenu)
                {

                    // This is just a placeholder for overloading

                }

                public void SyncUser(ref int nUserId)
                {

                    // This is just a placeholder for overloading

                }



                public void maintainUserInGroup(long nUserId, long nGroupId, bool remove, string cUserEmail = null, string cGroupName = null, bool isLast = false)
                {
                    // PerfMon.Log("Messaging", "maintainUserInGroup")
                    try
                    {
                    }
                    // do nothing this is a placeholder

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "maintainUserInGroup", ex, "", "", gbDebug);
                    }
                }

            }

            public class Activities : Protean.Messaging
            {

                public Activities(ref string sException) : base(ref sException)
                {
                }

                public new bool SendMailToList_Queued(int nPageId, string cEmailXSL, string cGroups, string cFromEmail, string cFromName, string cSubject)
                {
                    // PerfMon.Log("Messaging", "SendMailToList_Queued")

                    string cProcessInfo = "";

                    try
                    {
                        System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                        var oAddressDic = this.GetGroupEmails(cGroups);
                        // max number of bcc

                        var oWeb = new Cms();
                        oWeb.InitializeVariables();
                        oWeb.Open();
                        oWeb.mnPageId = nPageId;
                        oWeb.mbAdminMode = false;

                        oWeb.mcEwSiteXsl = cEmailXSL;
                        // get body
                        oWeb.mnMailMenuId =Convert.ToInt64(moMailConfig["RootPageId"]);
                        string oEmailBody = oWeb.ReturnPageHTML(oWeb.mnPageId);

                        var i2 = default(int);

                        MailMessage oEmail = null;

                        cFromEmail = cFromEmail.Trim();

                        if (Text.IsEmail(cFromEmail))
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
                                    if (Text.IsEmail(cRepientMail.Trim()))
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

                        stdTools.returnException(ref Protean.Messaging.msException, this.mcModuleName, "SendMailToList_Queued", ex, "", cProcessInfo, gbDebug);
                        return false;
                    }
                }

                public new string SendQueuedMail(MailMessage oMailn, string cHost, string cPickupLocation)
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
                        stdTools.returnException(ref Protean.Messaging.msException, this.mcModuleName, "SendQueuedMail", ex, "", "", gbDebug);
                        return "Error";
                    }
                }

                public bool AddToList(string ListId, string Name, string Email, IDictionary values)
                {
                    // PerfMon.Log("Activities", "AddToList")
                    try
                    {
                        // do nothing this is a placeholder
                        return default;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref Protean.Messaging.msException, this.mcModuleName, "AddToList", ex, "", "", gbDebug);
                        return default;
                    }
                }

                public bool RemoveFromList(string ListId, string Email)
                {
                    // PerfMon.Log("Activities", "RemoveFromList")
                    try
                    {
                        // do nothing this is a placeholder
                        return default;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref Protean.Messaging.msException, this.mcModuleName, "RemoveFromList", ex, "", "", gbDebug);
                        return default;
                    }
                }


            }
        }
    }
}