using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.Membership;
using static Protean.stdTools;

namespace Protean
{


    public partial class Cms
    {
        public partial class Membership
        {
            #region Module Behaviour

            public class Modules
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Protean.Cms.Membership.Modules";

                public Modules()
                {

                    // do nowt

                }

                // sReturnValue = "LogOn"
                // moDbHelper.logActivity(dbHelper.ActivityType.Logon, mnUserId, mnPageId, mnArtId)
                // myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logon, myWeb.mnUserId, myWeb.moSession.SessionID, Now, 0, 0, "")

                public void Logon(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {

                        Admin.AdminXforms oAdXfm = myWeb.getAdminXform();

                        XmlElement oXfmElmt;
                        string sReturnValue = string.Empty;
                        string cLogonCmd = "";

                        if (myWeb.mnUserId == 0 & (myWeb.moRequest["ewCmd"] != "passwordReminder" & myWeb.moRequest["ewCmd"] != "ActivateAccount"))
                        {

                            oXfmElmt = (XmlElement)oAdXfm.GetProviderXFrmUserLogon();
                            bool bAdditionalChecks = false;
                            if (Conversions.ToBoolean(!oAdXfm.valid))
                            {
                                // Call in additional authentication checks
                                if (myWeb.moConfig["AlternativeAuthentication"] == "On")
                                {
                                    bAdditionalChecks = myWeb.AlternativeAuthentication();
                                }
                            }

                            if (Conversions.ToBoolean(Operators.OrObject(oAdXfm.valid, bAdditionalChecks)))
                            {
                                myWeb.moContentDetail = (XmlElement)null;
                                // mnUserId = adXfm.mnUserId
                                if (myWeb.moSession != null)
                                    myWeb.moSession["nUserId"] = (object)myWeb.mnUserId;
                                if (myWeb.moRequest["cRemember"] == "true")
                                {
                                    var oCookie = new System.Web.HttpCookie("RememberMe");
                                    oCookie.Value = myWeb.mnUserId.ToString();
                                    oCookie.Expires = DateAndTime.DateAdd(DateInterval.Day, 60d, DateTime.Now);
                                    myWeb.moResponse.Cookies.Add(oCookie);
                                }
                                // Now we want to reload as permissions have changed
                                if (myWeb.moSession != null)
                                {
                                    if (myWeb.moSession["cLogonCmd"] != null)
                                    {
                                        cLogonCmd = Strings.Split(Conversions.ToString(myWeb.moSession["cLogonCmd"]), "=")[0];
                                        if (myWeb.mcOriginalURL.Contains(cLogonCmd + "="))
                                        {
                                            cLogonCmd = "";
                                        }
                                        else if (myWeb.mcOriginalURL.Contains("="))
                                        {
                                            cLogonCmd = Conversions.ToString(Operators.ConcatenateObject("&", myWeb.moSession["cLogonCmd"]));
                                        }
                                        else
                                        {
                                            cLogonCmd = Conversions.ToString(Operators.ConcatenateObject("?", myWeb.moSession["cLogonCmd"]));
                                        }
                                    }
                                }
                                myWeb.moSession["RedirectReason"] = "logonSuccessful";

                                var oMembership = new Membership(ref myWeb);
                                oMembership.ProviderActions(ref myWeb, "logonAction");

                                myWeb.logonRedirect(cLogonCmd);
                            }
                            else
                            {
                                if (oXfmElmt != null)
                                {
                                    oContentNode.InnerXml = oXfmElmt.InnerXml;
                                }
                            }
                        }

                        else if (myWeb.moRequest["ewCmd"] == "passwordReminder")
                        {
                            // RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Protean.Cms.Config.
                            switch (Strings.LCase(myWeb.moConfig["MembershipEncryption"]) ?? "")
                            {
                                case "md5":
                                case "sha1":
                                case "sha256":
                                    {
                                        oXfmElmt = (XmlElement)oAdXfm.xFrmResetAccount();
                                        break;
                                    }

                                default:
                                    {
                                        oXfmElmt = (XmlElement)oAdXfm.xFrmPasswordReminder();
                                        break;
                                    }
                            }
                            oContentNode.InnerXml = oXfmElmt.InnerXml;
                        }

                        else if (myWeb.moRequest["ewCmd"] == "ActivateAccount")
                        {

                            oXfmElmt = (XmlElement)oAdXfm.xFrmActivateAccount();
                            oContentNode.InnerXml = oXfmElmt.InnerXml;

                        }
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                    finally
                    {

                    }
                }

                public void Register(ref Cms myWeb, ref XmlElement oContentNode)
                {

                    try
                    {
                        myWeb.bPageCache = false;
                        long mnUserId = (long)myWeb.mnUserId;
                        bool mbAdminMode = myWeb.mbAdminMode;
                        var moPageXml = myWeb.moPageXml;
                        var moDbHelper = myWeb.moDbHelper;
                        var moSession = myWeb.moSession;
                        var moConfig = myWeb.moConfig;
                        var moRequest = myWeb.moRequest;

                        var goServer = myWeb.goServer;

                        string cLogonCmd = "";
                        // bool bRedirectStarted = false;
                        XmlElement oXfmElmt;
                        string sProcessInfo;

                        string AccountCreateForm = "UserRegister";
                        string AccountUpdateForm = "UserMyAccount";

                        if (!string.IsNullOrEmpty(oContentNode.GetAttribute("accountCreateFormName")))
                            AccountCreateForm = oContentNode.GetAttribute("accountCreateFormName");
                        if (!string.IsNullOrEmpty(oContentNode.GetAttribute("accountUpdateFormName")))
                            AccountUpdateForm = oContentNode.GetAttribute("accountUpdateFormName");

                        bool bLogon = false;

                        Admin.AdminXforms oAdXfm = myWeb.getAdminXform();

                        bool bRedirect = true;

                        if (moRequest["ewCmd"] == "ResendActivation")
                        {
                            Membership oMembership = new Cms.Membership(ref myWeb);
                            //oMembership.OnErrorWithWeb += OnComponentError;
                            //oMembership.AccountActivateLink(Convert.ToInt16(mnUserId));

                            ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                            IMembershipProvider moMemProv = RetProv.Get(ref myWeb, moConfig["MembershipProvider"]);

                            moMemProv.Activities.sendRegistrationAlert(ref myWeb, myWeb.mnUserId, false);

                            oContentNode.SetAttribute("activationMsg", "Activation Link Sent");

                            //string xsltPath = "/xsl/email/registration.xsl";
                            //if (myWeb.moConfig["cssFramework"] == "bs5")
                            //{
                            //    xsltPath = "/features/membership/email/registration.xsl";
                            //}
                            //if (!string.IsNullOrEmpty(xsltPath))
                            //{
                            //    var oUserElmt = myWeb.moDbHelper.GetUserXML(myWeb.mnUserId);
                            //    oUserElmt.SetAttribute("Url", myWeb.mcOriginalURL);
                            //    XmlElement oUserEmail = (XmlElement)oUserElmt.SelectSingleNode("Email");
                            //    string fromName = myWeb.moConfig["SiteAdminName"];
                            //    string fromEmail = myWeb.moConfig["SiteAdminEmail"];
                            //    string recipientEmail = "";
                            //    if (oUserEmail != null)
                            //        recipientEmail = oUserEmail.InnerText;
                            //    string SubjectLine = "Your Account Activation Link";
                            //    var oMsg = new Protean.Messaging(ref myWeb.msException);
                            //    // send an email to the new registrant
                            //    if (!string.IsNullOrEmpty(recipientEmail))
                            //    {
                            //        Cms.dbHelper argodbHelper = null;
                            //        String cProcessInfo = Conversions.ToString(oMsg.emailer(oUserElmt, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, odbHelper: ref argodbHelper, "Message Sent", "Message Failed"));
                            //    }
                            //    oMsg = (Protean.Messaging)null;
                            //    oContentNode.SetAttribute("activationMsg", "Activation Link Sent");
                            //}

                        }


                        if (moRequest["ewCmd"] == "ActivateAccount")
                        {

                        }

                        // We should move activate account features here. Currently in membership process.

                        else
                        {



                            // OAuth Functionality

                            if (!!string.IsNullOrEmpty(moRequest["oAuthResp"]))
                            {
                                if (!string.IsNullOrEmpty(moRequest["oAuthReg"]) & string.IsNullOrEmpty(myWeb.msRedirectOnEnd))
                                {
                                    object sRedirectPath = "";
                                    object appId = "";
                                    object redirectURI = "https://" + moRequest.ServerVariables["SERVER_NAME"] + myWeb.mcPageURL + "?oAuthResp=" + moRequest["oAuthReg"];
                                    switch (moRequest["oAuthReg"] ?? "")
                                    {
                                        case "facebook":
                                            {
                                                sRedirectPath = "https://www.facebook.com/v2.8/dialog/oauth?";
                                                appId = moConfig["OauthFacebookId"];
                                                sRedirectPath = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(sRedirectPath, "client_id="), appId), "&redirect_uri="), redirectURI);
                                                break;
                                            }
                                        case "twitter":
                                            {
                                                var twApi = new Integration.Directory.Twitter(ref myWeb);
                                                twApi.twitterConsumerKey = moConfig["OauthTwitterId"];
                                                twApi.twitterConsumerSecret = moConfig["OauthTwitterKey"];
                                                sRedirectPath = twApi.GetRequestToken();
                                                break;
                                            }
                                    }
                                    myWeb.msRedirectOnEnd = Conversions.ToString(sRedirectPath);
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                switch (moRequest["oAuthResp"] ?? "")
                                {
                                    case "facebook":
                                        {
                                            object redirectURI = "http://" + moRequest.ServerVariables["SERVER_NAME"] + myWeb.mcPageURL + "?oAuthResp=facebook";
                                            sProcessInfo = "Facebook Response";
                                            var fbClient = new Integration.Directory.Facebook(ref myWeb, moConfig["OauthFacebookId"], moConfig["OauthFacebookKey"]);
                                            List<Integration.Directory.Facebook.User> fbUsers;
                                            fbUsers = fbClient.GetFacebookUserData(moRequest["code"], Conversions.ToString(redirectURI));
                                            sProcessInfo = fbUsers[0].first_name + " " + fbUsers[0].last_name;

                                            var tmp = fbUsers;
                                            var argfbUser = tmp[0];
                                            mnUserId = fbClient.CreateUser(ref argfbUser);
                                            tmp[0] = argfbUser;

                                            if (moSession != null)
                                                moSession["nUserId"] = (object)mnUserId;
                                            moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Register, (int)mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "First Logon");

                                            bLogon = true;
                                            break;
                                        }

                                    case "twitter":
                                        {
                                            sProcessInfo = "Twitter Response";
                                            var twApi = new Integration.Directory.Twitter(ref myWeb);
                                            twApi.twitterConsumerKey = moConfig["OauthTwitterId"];
                                            twApi.twitterConsumerSecret = moConfig["OauthTwitterKey"];
                                            break;
                                        }
                                        // Get twitter user
                                        // Dim twUsers As List(Of Protean.Integration.Directory.Twitter.User)   'Never used


                                }
                                if (mnUserId > 0L)
                                {



                                }


                            }

                            // If not in admin mode then base our choice on whether the user is logged in. 
                            // If in Admin Mode, then present it as WYSIWYG
                            if (!myWeb.mbAdminMode & myWeb.mnUserId > 0)
                            {
                                XmlElement InstanceAppend = null;
                                XmlElement oContentForm = (XmlElement)myWeb.moPageXml.SelectSingleNode("descendant-or-self::Content[@type='xform' and @name='UserMyAccount']");
                                if (oContentForm is null)
                                {
                                    oXfmElmt = (XmlElement)oAdXfm.xFrmEditDirectoryItem(ref InstanceAppend, myWeb.mnUserId, "User", default, AccountUpdateForm);
                                }
                                else
                                {
                                    oXfmElmt = (XmlElement)oAdXfm.xFrmEditDirectoryItem(ref InstanceAppend, myWeb.mnUserId, "User", default, AccountUpdateForm, oContentForm.OuterXml);
                                    if (!myWeb.mbAdminMode)
                                        oContentForm.ParentNode.RemoveChild(oContentForm);
                                }
                                oContentNode.InnerXml = oXfmElmt.InnerXml;
                            }

                            else
                            {
                                XmlElement InstanceAppend = null;
                                XmlElement oContentForm = (XmlElement)moPageXml.SelectSingleNode("descendant-or-self::Content[@type='xform' and @name='UserRegister']");
                                if (oContentForm is null)
                                {
                                    oXfmElmt = (XmlElement)oAdXfm.xFrmEditDirectoryItem(ref InstanceAppend, mnUserId, "User", default, AccountCreateForm);
                                }
                                else
                                {
                                    oXfmElmt = (XmlElement)oAdXfm.xFrmEditDirectoryItem(ref InstanceAppend, mnUserId, "User", default, AccountCreateForm, oContentForm.OuterXml);
                                    if (!mbAdminMode)
                                        oContentForm.ParentNode.RemoveChild(oContentForm);
                                }

                                if (!string.IsNullOrEmpty(myWeb.moConfig["SecureMembershipAddress"]) & myWeb.mbAdminMode == false)
                                {
                                    XmlElement oSubElmt = (XmlElement)oAdXfm.moXformElmt.SelectSingleNode("descendant::submission");
                                    oSubElmt.SetAttribute("action", myWeb.moConfig["SecureMembershipAddress"] + myWeb.mcPagePath);
                                }

                                // ok if the user is valid we then need to handle what happens next.
                                if (Conversions.ToBoolean(oAdXfm.valid))
                                {
                                    myWeb.mnUserId = Conversions.ToInteger(oAdXfm.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText);
                                    var oMembership = new Membership(ref myWeb);
                                    oMembership.RegistrationActions();
                                    switch (myWeb.moConfig["RegisterBehaviour"] ?? "")
                                    {
                                        case "validateByEmail":
                                            {
                                                // don't redirect because we want to reuse this form
                                                bRedirect = false;
                                                // say thanks for registering and update the form
                                                // hide the current form
                                                XmlElement oFrmGrp = (XmlElement)oAdXfm.moXformElmt.SelectSingleNode("group");
                                                oFrmGrp.SetAttribute("class", "hidden");
                                                // create a new note
                                                XmlElement frmElmt = oAdXfm.moXformElmt;
                                                XmlElement oFrmGrp2 = (XmlElement)oAdXfm.addGroup(ref frmElmt, "validateByEmail");
                                                //XmlNode oFrmGrp2Node = (XmlNode)oFrmGrp2;
                                                oAdXfm.addNote(ref oFrmGrp2, Protean.xForm.noteTypes.Hint, "<span class=\"msg-1029\">Thanks for registering you have been sent an email with a link you must click to activate your account</span>", true);
                                                break;
                                            }

                                        default:
                                            {
                                                bLogon = true;
                                                break;
                                            }


                                    }

                                    // redirect to this page or alternative page.
                                    if (bRedirect)
                                    {
                                        myWeb.msRedirectOnEnd = myWeb.mcOriginalURL;
                                    }
                                    else
                                    {
                                        oContentNode.InnerXml = oXfmElmt.InnerXml;
                                    }
                                }
                                else
                                {
                                    oContentNode.InnerXml = oXfmElmt.InnerXml;
                                }
                            }

                            if (bLogon)
                            {
                                // Now we want to reload as permissions have changed

                                if (moSession != null)
                                {
                                    if (moSession["cLogonCmd"] != null)
                                    {
                                        cLogonCmd = Strings.Split(Conversions.ToString(moSession["cLogonCmd"]), "=")[0];
                                        if (myWeb.mcOriginalURL.Contains(cLogonCmd + "="))
                                        {
                                            cLogonCmd = "";
                                        }
                                        else if (myWeb.mcOriginalURL.Contains("="))
                                        {
                                            cLogonCmd = Conversions.ToString(Operators.ConcatenateObject("&", moSession["cLogonCmd"]));
                                        }
                                        else
                                        {
                                            cLogonCmd = Conversions.ToString(Operators.ConcatenateObject("?", moSession["cLogonCmd"]));
                                        }
                                    }
                                }

                                moSession["RedirectReason"] = "registration";
                                myWeb.bRedirectStarted = true; // This acts as a local suppressant allowing for the sessio to pass through to the redirected page

                                string redirectId = myWeb.moConfig["RegisterRedirectPageId"];

                                if (!string.IsNullOrEmpty(oContentNode.GetAttribute("redirectPathId")))
                                {
                                    redirectId = oContentNode.GetAttribute("redirectPathId");
                                }

                                if (!string.IsNullOrEmpty(redirectId))
                                {
                                    // refresh the site strucutre with new userId
                                    myWeb.mnUserId = (int)mnUserId;
                                    myWeb.GetStructureXML("Site");
                                    XmlElement oElmt = (XmlElement)myWeb.moPageXml.SelectSingleNode("/Page/Menu/descendant-or-self::MenuItem[@id = '" + redirectId + "']");
                                    string redirectPath = myWeb.mcOriginalURL;
                                    if (oElmt is null)
                                    {
                                        myWeb.msRedirectOnEnd = redirectPath;
                                        bRedirect = true;
                                    }
                                    else
                                    {
                                        redirectPath = oElmt.GetAttribute("url");
                                        myWeb.msRedirectOnEnd = redirectPath;
                                        bRedirect = false;
                                    }
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                    finally
                    {
                    }
                }

                public void PasswordReminder(ref Cms myWeb, ref XmlElement oContentNode)
                {

                    var moConfig = myWeb.moConfig;
                    try
                    {

                        Admin.AdminXforms oAdXfm = myWeb.getAdminXform();

                        XmlElement oXfmElmt;
                        if (myWeb.mnUserId == 0)
                        {
                            switch (Strings.LCase(moConfig["MembershipEncryption"]) ?? "")
                            {
                                case "md5salt":
                                case "md5":
                                case "sha1":
                                case "sha256":
                                    {
                                        if (myWeb.moRequest["ewCmd"] == "AR-MOD")
                                        {
                                            string cAccountHash = myWeb.moRequest["AI"];
                                            // Validate to prevent SQL injection
                                            // strip out any spaces to prevent SQL injection
                                            if (cAccountHash.Contains(" "))
                                            {
                                                cAccountHash = Strings.Left(cAccountHash, Strings.InStr(cAccountHash, " "));
                                            }
                                            if (cAccountHash.Contains("%20"))
                                            {
                                                cAccountHash = Strings.Left(cAccountHash, Strings.InStr(cAccountHash, "%20"));
                                            }
                                            var regex = new System.Text.RegularExpressions.Regex(@"[\d,]");

                                            if (!regex.Match(cAccountHash).Success)
                                            {
                                                cAccountHash = "";
                                            }

                                            if (!string.IsNullOrEmpty(cAccountHash))
                                            {
                                                oXfmElmt = (XmlElement)oAdXfm.xFrmConfirmPassword(cAccountHash);
                                            }
                                            else
                                            {
                                                oXfmElmt = (XmlElement)oAdXfm.xFrmResetAccount();
                                            }
                                        }
                                        else
                                        {
                                            oXfmElmt = (XmlElement)oAdXfm.xFrmResetAccount();
                                        }

                                        break;
                                    }

                                default:
                                    {
                                        oXfmElmt = (XmlElement)oAdXfm.xFrmPasswordReminder();
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            oXfmElmt = (XmlElement)oAdXfm.xFrmConfirmPassword(myWeb.mnUserId);
                        }


                        oContentNode.InnerXml = oXfmElmt.InnerXml;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                }

                public void GetUserContent(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    string ContentType = oContentNode.GetAttribute("contentType");
                    string UserId = myWeb.mnUserId.ToString();
                    string WhereSql = " cContentSchemaName = '" + ContentType + "' and nInsertDirId = " + UserId;

                    try
                    {
                        if (Conversions.ToDouble(UserId) != 0d)
                        {
                            myWeb.mbAdminMode = true;
                            XmlElement argoPageDetail = null;
                            int ncount = 0;
                            myWeb.GetPageContentFromSelect(WhereSql, ref ncount, oContentsNode: ref oContentNode, oPageDetail: ref argoPageDetail, false, cOrderBy: "", distinct: true);
                            myWeb.mbAdminMode = false;
                        }
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserContent", ex, ""));
                    }
                }


                public void GetUsersByGroup(ref Cms myWeb, ref XmlElement contentNode)
                {

                    try
                    {

                        string cSchemaName = contentNode.Attributes["parentSchemaType"].InnerText;
                        string sArrDirKeys = contentNode.Attributes["parentIds"].InnerText; // comma separated int array
                        int nStatus = Conversions.ToInteger(contentNode.Attributes["parentStatus"].InnerText);
                        string nParDirId = contentNode.Attributes["parentParId"].InnerText;

                        if (sArrDirKeys.Length > 0)
                        {

                            // make SQL statement
                            var strSql = new System.Text.StringBuilder();
                            strSql.Append("CREATE TABLE #tmpTblUsers ");
                            strSql.Append("(");
                            strSql.Append("[id] INT, ");
                            strSql.Append("[Status] INT, ");
                            strSql.Append("[Username] NVARCHAR(100), ");
                            strSql.Append("[UserXml] NVARCHAR(max), ");
                            strSql.Append("[Companies] NVARCHAR(100) ");
                            strSql.Append(") ");

                            // gets all users using standard stored proc
                            strSql.Append("INSERT INTO #tmpTblUsers ");
                            strSql.Append("EXEC spGetUsers " + nParDirId.ToString() + ", " + nStatus.ToString() + " ");

                            // narrows scope of users
                            strSql.Append("SELECT ");
                            strSql.Append("#tmpTblUsers.[id] ");
                            strSql.Append("FROM tblDirectory AS tblGroups ");
                            strSql.Append("INNER JOIN tblDirectoryRelation ON tblGroups.nDirKey = tblDirectoryRelation.nDirParentId ");
                            strSql.Append("INNER JOIN #tmpTblUsers ON tblDirectoryRelation.nDirChildId = #tmpTblUsers.id ");
                            strSql.Append("WHERE (tblGroups.cDirSchema = '" + cSchemaName + "') ");
                            strSql.Append(" AND (tblGroups.nDirKey in (" + sArrDirKeys + ")) ");

                            // add each result to content node xml
                            var oDsUsers = myWeb.moDbHelper.GetDataSet(strSql.ToString(), "User", "UserGroups");
                            foreach (DataRow oDrUser in oDsUsers.Tables[0].Rows)
                                contentNode.AppendChild(myWeb.GetUserXML(Conversions.ToLong(oDrUser["id"])).Clone());

                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetUsersByGroup", ex, "", "", gbDebug);
                        // Return Nothing
                    }

                }


                public void UserContact(ref Cms myWeb, ref XmlElement contentNode)
                {

                    try
                    {

                        if (myWeb.mnUserId > 0)
                        {

                            var adXfm = myWeb.getAdminXform();
                            adXfm.open(myWeb.moPageXml);
                            string memCmd = myWeb.moRequest["memCmd"];
                            if (string.IsNullOrEmpty(memCmd) & !string.IsNullOrEmpty(myWeb.moRequest["ewCmd"]))
                            {
                                memCmd = myWeb.moRequest["ewCmd"];
                            }

                            switch (memCmd ?? "")
                            {
                                case "addContact":
                                case "editContact":
                                    {
                                        XmlElement oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryContact(Convert.ToInt64(myWeb.moRequest["id"]), myWeb.mnUserId);
                                        if (Conversions.ToBoolean(!adXfm.valid))
                                        {
                                            contentNode.AppendChild(oXfmElmt);
                                        }
                                        else
                                        {
                                            myWeb.RefreshUserXML();
                                        }

                                        break;
                                    }
                                case "delContact":
                                    {
                                        foreach (XmlElement oContactElmt in myWeb.GetUserXML((long)myWeb.mnUserId).SelectNodes("descendant-or-self::Contact"))
                                        {
                                            XmlElement oId = (XmlElement)oContactElmt.SelectSingleNode("nContactKey");
                                            if (oId != null)
                                            {
                                                if ((oId.InnerText ?? "") == (myWeb.moRequest["id"] ?? ""))
                                                {
                                                    myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, Conversions.ToLong(myWeb.moRequest["id"]));
                                                    myWeb.RefreshUserXML();
                                                }
                                            }
                                        }

                                        break;
                                    }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "UserContacts", ex, "", "", gbDebug);
                        // Return Nothing
                    }

                }

                public void CompanyContact(ref Cms myWeb, ref XmlElement contentNode)
                {
                    long CompanyId = Conversions.ToLong(myWeb.moRequest["ParentDirId"]);
                    bool bUserValid = true;
                    try
                    {

                        // If myWeb.mnUserId > 0 And myWeb.moPageXml.SelectSingleNode("User/Company[@id='" & CompanyId & "']") IsNot Nothing Then

                        if (myWeb.mnUserId > 0)
                        {

                            var adXfm = myWeb.getAdminXform();
                            adXfm.open(myWeb.moPageXml);

                            if (Conversions.ToInteger("0" + myWeb.moRequest["id"]) > 0 & myWeb.moPageXml.SelectSingleNode("User/Company/Contacts/Contact/nContactKey[text()='" + myWeb.moRequest["id"] + "']") != null)
                            {
                                bUserValid = false;
                            }

                            if (bUserValid)
                            {
                                string memCmd = myWeb.moRequest["memCmd"];
                                if (string.IsNullOrEmpty(memCmd) & !string.IsNullOrEmpty(myWeb.moRequest["ewCmd"]))
                                {
                                    memCmd = myWeb.moRequest["ewCmd"];
                                }

                                switch (memCmd ?? "")
                                {
                                    case "addContact":
                                    case "editContact":
                                        {
                                            XmlElement oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryContact(Convert.ToInt64(myWeb.moRequest["id"]), Convert.ToInt16(CompanyId), "/xforms/directory/CompanyContact.xml");
                                            if (Conversions.ToBoolean(!adXfm.valid))
                                            {
                                                contentNode.AppendChild(oXfmElmt);
                                            }
                                            else
                                            {
                                                myWeb.RefreshUserXML();
                                            }

                                            break;
                                        }
                                    case "delContact":
                                        {
                                            foreach (XmlElement oContactElmt in myWeb.GetUserXML((long)myWeb.mnUserId).SelectNodes("descendant-or-self::Contact"))
                                            {
                                                XmlElement oId = (XmlElement)oContactElmt.SelectSingleNode("nContactKey");
                                                if (oId != null)
                                                {
                                                    if ((oId.InnerText ?? "") == (myWeb.moRequest["id"] ?? ""))
                                                    {
                                                        myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, Conversions.ToLong(myWeb.moRequest["id"]));
                                                        myWeb.RefreshUserXML();
                                                    }
                                                }
                                            }

                                            break;
                                        }
                                }
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CompanyContact", ex, "", "", gbDebug);
                        // Return Nothing
                    }

                }


                public void AddUserToGroup(ref Cms myWeb, ref XmlElement contentNode)
                {
                    try
                    {

                        if (myWeb.mnUserId > 0 & contentNode.SelectSingleNode("ancestor::Order") != null)
                        {

                            foreach (XmlElement groupNode in contentNode.SelectNodes("descendant-or-self::UserGroups/Group"))
                            {
                                string groupId = groupNode.GetAttribute("id");
                                if (Information.IsNumeric(groupId))
                                {
                                    // Dim cUserEmail As String = myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, myWeb.mnUserId)
                                    // Dim cGroupName As String = myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, CLng(groupId))

                                    myWeb.moDbHelper.maintainDirectoryRelation(Conversions.ToLong(groupId), (long)myWeb.mnUserId, false);
                                }
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "AddUserToGroup", ex, "", "", gbDebug);
                        // Return Nothing
                    }

                }

                public void JobApplication(ref Cms myWeb, ref XmlElement oContentNode)
                {

                    string cProcessInfo = string.Empty;
                    var moConfig = myWeb.moConfig;
                    XmlElement oElmt;
                    long JobId = Conversions.ToLong("0" + myWeb.moRequest["JobId"]);
                    long FormId = Conversions.ToLong("0" + myWeb.moRequest["FormId"]);
                    try
                    {
                        if (myWeb.mnUserId == 0)
                        {
                            Register(ref myWeb, ref oContentNode);

                            var logonContentNode = myWeb.moPageXml.CreateElement("Content");
                            oContentNode.ParentNode.AppendChild(logonContentNode);

                            this.Logon(ref myWeb, ref logonContentNode);
                        }

                        else if (string.IsNullOrEmpty(myWeb.moRequest["FormId"]))
                        {
                            // List User Applications
                            var strSql = new System.Text.StringBuilder();
                            strSql.Append("SELECT * FROM [tblEmailActivityLog] eal ");
                            strSql.Append("INNER JOIN tblActivityLog al on al.nOtherId = eal.nEmailActivityKey ");
                            strSql.Append("where al.nUserDirId = " + myWeb.mnUserId);
                            var oDsJobs = myWeb.moDbHelper.GetDataSet(strSql.ToString(), "Application", "JobApplications");
                            oContentNode.InnerXml = Strings.Replace(oDsJobs.GetXml(), "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                            string sContent;
                            foreach (XmlElement oElmt2 in oContentNode.SelectNodes("descendant-or-self::cActivityXml | descendant-or-self::cActivityDetail"))
                            {
                                sContent = oElmt2.InnerText;
                                if (!string.IsNullOrEmpty(sContent))
                                {
                                    try
                                    {
                                        oElmt2.InnerXml = sContent;
                                    }
                                    catch
                                    {
                                        oElmt2.InnerXml = stdTools.tidyXhtmlFrag(sContent);
                                    }
                                }
                            }
                        }


                        else if (oContentNode.ParentNode.Name == "ContentDetail")
                        {
                            // remove application forms not required
                            foreach (XmlElement currentOElmt in oContentNode.SelectNodes("Content[@id!='" + myWeb.moRequest["FormId"] + "']"))
                            {
                                oElmt = currentOElmt;
                                oElmt.ParentNode.RemoveChild(oElmt);
                            }

                            foreach (XmlElement currentOElmt1 in oContentNode.SelectNodes("Content[@id='" + myWeb.moRequest["FormId"] + "']"))
                            {
                                oElmt = currentOElmt1;

                                // Form Handling
                                Protean.xForm oXform = (Protean.xForm)myWeb.getXform();

                                oXform.moPageXML = myWeb.moPageXml;
                                oXform.load(ref oElmt, true);

                                if (string.IsNullOrEmpty(myWeb.moRequest["stageButton"]) & !string.IsNullOrEmpty(myWeb.moRequest["stage"]))
                                {
                                    XmlElement passportNode2 = (XmlElement)oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport");
                                    passportNode2.SetAttribute("stage", myWeb.moRequest["stage"]);
                                    myWeb.moSession["tempInstance"] = oXform.Instance;
                                }

                                var strSql = new System.Text.StringBuilder();
                                strSql.Append("SELECT eal.nEmailActivityKey FROM [tblEmailActivityLog] eal ");
                                strSql.Append("INNER JOIN tblActivityLog al on al.nOtherId = eal.nEmailActivityKey ");
                                strSql.Append("where al.nUserDirId = " + myWeb.mnUserId + " and al.nArtId = " + JobId);
                                long EmailActivityId = Conversions.ToLong(Operators.ConcatenateObject("0", myWeb.moDbHelper.GetDataValue(strSql.ToString())));
                                if (EmailActivityId > 0L)
                                {
                                    // load in a saved instance
                                    var strSql2 = new System.Text.StringBuilder();
                                    strSql2.Append("SELECT cActivityXml FROM [tblEmailActivityLog] eal ");
                                    strSql2.Append("INNER JOIN tblActivityLog al on al.nOtherId = eal.nEmailActivityKey ");
                                    strSql2.Append("where al.nUserDirId = " + myWeb.mnUserId + " and al.nArtId = " + JobId);

                                    string loadedInstance = Conversions.ToString(myWeb.moDbHelper.GetDataValue(strSql2.ToString()));
                                    if (!string.IsNullOrEmpty(loadedInstance) & myWeb.moSession["tempInstance"] is null)
                                    {
                                        var oLoadedInstance = myWeb.moPageXml.CreateElement("instance");
                                        oLoadedInstance.InnerXml = loadedInstance;
                                        // tidy up the stage editor

                                        oXform.Instance = (XmlElement)oLoadedInstance.FirstChild;
                                    }
                                }
                                else
                                {
                                    // set the job title
                                    XmlElement JobTitleNode = (XmlElement)oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport/LearnerInfo/Headline/Type/Label");
                                    JobTitleNode.InnerText = myWeb.moRequest["JobApply"];
                                    XmlElement JobCodeNode = (XmlElement)oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport/LearnerInfo/Headline/Type/Code");
                                    JobCodeNode.InnerText = myWeb.moRequest["JobId"];
                                }

                                if (oXform.isSubmitted())
                                {

                                    oXform.updateInstanceFromRequest();

                                    // tidy up the stage editor
                                    if (string.IsNullOrEmpty(myWeb.moRequest["stageButton"]))
                                    {
                                        XmlElement passportNode = (XmlElement)oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport");
                                        passportNode.SetAttribute("stage", myWeb.moRequest["stage"]);
                                    }

                                    // save instace & progress
                                    if (EmailActivityId > 0L)
                                    {
                                        var strSql3 = new System.Text.StringBuilder();
                                        strSql3.Append(Operators.ConcatenateObject(Operators.ConcatenateObject("update tblEmailActivityLog set cActivityXml = '", stdTools.SqlFmt(oXform.Instance.OuterXml)), "'"));
                                        strSql3.Append("where nEmailActivityKey = " + EmailActivityId);
                                        myWeb.moDbHelper.ExeProcessSql(strSql3.ToString());
                                    }
                                    else
                                    {
                                        EmailActivityId = myWeb.moDbHelper.emailActivity((short)myWeb.mnUserId, cActivityXml: oXform.Instance.OuterXml);
                                        myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.JobApplication, (long)myWeb.mnUserId, 0L, JobId, EmailActivityId, "");
                                    }

                                    if (!string.IsNullOrEmpty(myWeb.moRequest["SaveExit"]))
                                    {

                                        myWeb.msRedirectOnEnd = myWeb.mcPagePath;

                                    }

                                    if (myWeb.moRequest["stageButton"] == "Validate")
                                    {

                                        // We validate the form

                                        XmlElement oSkillsPassport = (XmlElement)oXform.Instance.SelectSingleNode("emailer/oBodyXML/SkillsPassport");
                                        oSkillsPassport.SetAttribute("stage", "Validate");
                                        oXform.validate();

                                        if (oXform.valid)
                                        {
                                            string sMessage;

                                            var oMsg = new Protean.Messaging(ref myWeb.msException);

                                            Cms.dbHelper argodbHelper = null;
                                            sMessage = Conversions.ToString(oMsg.emailer((XmlElement)oXform.Instance.SelectSingleNode("emailer/oBodyXML"), oXform.Instance.SelectSingleNode("emailer/xsltPath").InnerText, oXform.Instance.SelectSingleNode("emailer/fromName").InnerText, oXform.Instance.SelectSingleNode("emailer/fromEmail").InnerText, oXform.Instance.SelectSingleNode("emailer/recipientEmail").InnerText, oXform.Instance.SelectSingleNode("emailer/SubjectLine").InnerText, ccRecipient: oXform.Instance.SelectSingleNode("emailer/ccRecipient").InnerText, bccRecipient: oXform.Instance.SelectSingleNode("emailer/bccRecipient").InnerText, cSeperator: "", odbHelper: ref argodbHelper));
                                            // Return sMessage
                                            XmlElement oEmailer = (XmlElement)oXform.Instance.SelectSingleNode("emailer");
                                            oEmailer.SetAttribute("SubmitMessage", sMessage);

                                            oSkillsPassport.SetAttribute("stage", "Complete");

                                        }

                                        var strSql3 = new System.Text.StringBuilder();
                                        strSql3.Append(Operators.ConcatenateObject(Operators.ConcatenateObject("update tblEmailActivityLog set cActivityXml = '", stdTools.SqlFmt(oXform.Instance.OuterXml)), "'"));
                                        strSql3.Append("where nEmailActivityKey = " + EmailActivityId);
                                        myWeb.moDbHelper.ExeProcessSql(strSql3.ToString());

                                    }
                                }

                                oXform.addValues();

                                oElmt.InnerXml = oXform.moXformElmt.InnerXml;
                                oXform = (Protean.xForm)null;

                            }
                        }
                        // Load Application Form
                        else
                        {
                            cProcessInfo = "only run on content detail";
                            // do nothing
                        }
                    }


                    // Dim adXfm As Object = myWeb.getAdminXform()
                    // Dim oXfmElmt As XmlElement

                    // oContentNode.InnerXml = oXfmElmt.InnerXml

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                }

            }

            #endregion
        }
    }
}
