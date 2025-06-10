// ***********************************************************************
// $Library:     Protean.Providers.membership.base
// $Revision:    3.1  
// $Date:        2012-07-21
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
// ***********************************************************************


using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.AdminProxy;
using Protean.Providers.Authentication;
using Protean.Providers.CDN;
using Protean.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Protean.Cms;
using static Protean.stdTools;
using static Protean.Tools.Xml;


namespace Protean.Providers
{
    namespace Membership
    {
        public interface IMembershipProvider
        {
            IMembershipProvider Initiate(ref Cms myWeb);
            //  void Initiate(ref Base myWeb);

            IMembershipAdminXforms AdminXforms { get; set; }
            IMembershipAdminProcess AdminProcess { get; set; }
            IMembershipActivities Activities { get; set; }

            void Dispose();
        }
        public interface IMembershipAdminXforms
        {
            XmlElement xFrmUserLogon(string FormName = "UserLogon", string cmdPrefix = "");
            XmlElement xFrmPasswordReminder();
            XmlElement xFrmActivateAccount();
            XmlElement xFrmResetPassword(long mnUserId);
            XmlElement xFrmEditDirectoryItem(long id = 0L, string cDirectorySchemaName = "User", long parId = 0L, string cXformName = "", string FormXML = "");
            XmlElement xFrmEditDirectoryItem(ref XmlElement IntanceAppend, long id = 0L, string cDirectorySchemaName = "User", long parId = 0L, string cXformName = "", string FormXML = "");

            XmlElement xFrmResetAccount(long userId = 0L);
            XmlElement xFrmConfirmPassword(string AccountHash);
            XmlElement xFrmConfirmPassword(long nUserId);

            XmlElement xFrmActivationCode(long nUserId, string cXformName = "ActivationCode", string cFormXml = "");

            XmlElement xFrmEditDirectoryContact(long id = 0L, int nUID = 0, string xFormPath = "/xforms/directory/UserContact.xml");

            XmlElement xFrmUserIntegrations(long userid, string cmd);

            void open(XmlDocument oPageXml);

            Boolean valid { get; set; }
            XmlElement Instance { get; set; }
            XmlElement moXformElmt { get; set; }
            XmlElement addGroup(ref XmlElement oContextNode, string sRef, string sClass = "", string sLabel = "", XmlElement oInsertBeforeNode = null);

            void addNote(string sRef, xForm.noteTypes nTypes, string sMessage, bool bInsertFirst = false, string sClass = "");
            void addNote(ref XmlNode oNode, xForm.noteTypes nTypes, string sMessage, bool bInsertFirst = false, string sClass = "");

        }

        public interface IMembershipAdminProcess
        {

        }
        public interface IMembershipActivities
        {
            long GetUserSessionId(ref Cms myWeb);
            string GetUserId(ref Cms myWeb);
            void SetUserId(ref Cms myWeb);
            string MembershipProcess(ref Cms myWeb);
            bool AlternativeAuthentication(ref Cms myWeb);

            XmlElement GetUserXML(ref Cms myWeb, long nUserId = 0L);

            void sendRegistrationAlert(ref Cms myWeb, long mnUserId, Boolean clearUserId, string cmdPrefix = "");

            void LogSingleUserSession();
            void LogSingleUserSession(ref Cms myWeb);

            string ResetUserAcct(ref Cms myWeb, int nUserId);

        }

        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.Membership.ReturnProvider";
            protected XmlNode moPaymentCfg;
            public event OnErrorEventHandler OnError;
            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            public event OnErrorWithWebEventHandler OnErrorWithWeb;
            public delegate void OnErrorWithWebEventHandler(ref Cms myweb, object sender, Tools.Errors.ErrorEventArgs e);

            public IMembershipProvider Get(ref Cms myWeb, string ProviderName)
            {
                try
                {
                    Type calledType;
                    if (string.IsNullOrEmpty(ProviderName))
                    {
                        ProviderName = "Protean.Providers.Membership.DefaultProvider";
                        calledType = Type.GetType(ProviderName, true);
                    }
                    else
                    {
                        var castObject = WebConfigurationManager.GetWebApplicationSection("protean/membershipProviders");
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                        var ourProvider = moPrvConfig.Providers[ProviderName];
                        Assembly assemblyInstance;
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(ourProvider.Parameters["path"], "", false)))
                        {
                            assemblyInstance = Assembly.LoadFrom(goServer.MapPath(Conversions.ToString(ourProvider.Parameters["path"])));
                        }
                        else
                        {
                            assemblyInstance = Assembly.Load(ourProvider.Type);
                        }
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(ourProvider.Parameters["rootClass"], "", false)))
                        {
                            calledType = assemblyInstance.GetType("Protean.Providers.Membership." + ProviderName, true);
                        }
                        else
                        {
                            calledType = assemblyInstance.GetType(ourProvider.Parameters["rootClass"] + ".Providers.Membership." + ProviderName, true);
                        }
                    }

                    var o = Activator.CreateInstance(calledType);

                    var args = new object[1];
                    args[0] = myWeb;

                    return (IMembershipProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args);

                }

                catch (Exception ex)
                {

                    string argsException = Conversions.ToString(myWeb.msException);
                    returnException(ref argsException, mcModuleName, "New", ex, "", ProviderName + " Could Not be Loaded", gbDebug);
                    myWeb.msException = argsException;
                    return null;
                }

            }

        }

        public class DefaultProvider : IMembershipProvider
        {
            //IMembershipProvider obj1 = new DefaultProvider();
            private IMembershipAdminXforms _AdminXforms;
            private IMembershipAdminProcess _AdminProcess;
            private IMembershipActivities _Activities;
            IMembershipAdminXforms IMembershipProvider.AdminXforms
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
            IMembershipAdminProcess IMembershipProvider.AdminProcess
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
            IMembershipActivities IMembershipProvider.Activities
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


            public DefaultProvider()
            {
                // do nothing
            }
            public IMembershipProvider Initiate(ref Cms myWeb)
            {
                _AdminXforms = new AdminXForms(ref myWeb);
                _AdminProcess = new AdminProcess(ref myWeb);
                // MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                _Activities = new Activities();
                return this;
            }

            public void Dispose()
            {
                _AdminXforms = null;
                _AdminProcess = null;
                _Activities = null;
            }

            //public void Initiate(ref Protean.Base myWeb)
            //{
            //    //IMembershipAdminXforms AdminXforms = new AdminXForms(ref myWeb);
            //    // MemProvider.AdminProcess = New AdminProcess(myWeb)
            //    // MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
            //    IMembershipActivities Activities = new Activities();
            //}
            public class AdminXForms : Admin.AdminXforms, IMembershipAdminXforms
            {
                private const string mcModuleName = "Protean.Providers.Membership.Default.AdminXForms";
                public bool maintainMembershipsOnAdd = true;

                XmlElement IMembershipAdminXforms.moXformElmt
                {
                    set
                    {
                        base.moXformElmt = value;
                    }
                    get
                    {
                        return base.moXformElmt;
                    }
                }

                public AdminXForms(ref Cms aWeb) : base(ref aWeb)
                {
                }

                public XmlElement xFrmUserLogon(string FormName = "UserLogon",string cmdPrefix = "") // Just replace Overridable to Overrides
                {

                    // Called to get XML for the User Logon.
                    string btnClass = "btn btn-default btn-block icon-right";
                    string btnIcon = "fa fa-sign-in";
                    string grpClass = "d-grid gap-2";
                    if (myWeb.bs5) {
                        btnClass = "btn btn-primary";
                        btnIcon = "fa-solid fa-right-to-bracket";
                        grpClass = "";
                    }

                    XmlElement oFrmElmt = null;
                    XmlElement oSelElmt;
                    string sValidResponse;
                    string cProcessInfo = "";
                    bool bRememberMe = false;


                    try
                    {
                        //this need to be optional based on auth provider config
                        Protean.Providers.Authentication.ReturnProvider oAuthProv = new Protean.Providers.Authentication.ReturnProvider();
                        IEnumerable<IauthenticaitonProvider> oAuthProviders = oAuthProv.Get(ref myWeb);


                        base.NewFrm("UserLogon");

                        if (mbAdminMode & myWeb.mnUserId == 0)
                            goto BuildForm;
                        if (myWeb.moConfig["RememberMeMode"] == "KeepCookieAfterLogoff" | myWeb.moConfig["RememberMeMode"] == "ClearCookieAfterLogoff")
                            bRememberMe = true;
                        string formPath = "/xforms/directory/" + FormName + ".xml";
                        if (myWeb.moConfig["cssFramework"] == "bs5")
                        {
                            formPath = "/features/membership/" + FormName + ".xml";
                        }

                        // maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        if (!base.load(formPath, myWeb.maCommonFolders))
                        {
                            // If this does not load manually then build a form to do it.
                           goto BuildForm;
                        }
                        else
                        {
                            goto Check;
                        }

                    BuildForm:
                        ;

                        base.submission("UserLogon", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "UserDetails", grpClass, "Sign in to ProteanCMS");

                        XmlElement userIpt = base.addInput(ref oFrmElmt, "cUserName", true, "");
                        userIpt.SetAttribute("placeholder", "Email");
                        base.addClientSideValidation(ref userIpt, true, "Please enter Email");
                        XmlElement oBindParent = null;
                        base.addBind("cUserName", "user/username", ref oBindParent, "true()");
                        string cClass = "";
                        XmlElement pwdIpt = base.addSecret(ref oFrmElmt, "cPassword", true, "", ref cClass);
                        pwdIpt.SetAttribute("placeholder", "Password");
                        base.addClientSideValidation(ref pwdIpt, true, "Please enter Password");
                        base.addBind("cPassword", "user/password", ref oBindParent, "true()");
                        base.addDiv(ref oFrmElmt, "", "password-reminder");

                        base.addSubmit(ref oFrmElmt, "UserLogon", "Sign In", "UserLogon", btnClass, btnIcon);



                     
                        if (oAuthProviders != null){
                            if (oAuthProviders.Count() > 0)
                            {
                                base.addDiv(ref oFrmElmt, "OR", "separator");
                                foreach (IauthenticaitonProvider authProvider in oAuthProviders) {
                                    Boolean bUse = false;
                                    if (FormName == "AdminLogon" && authProvider.config["scope"].ToString() == "admin") {
                                        bUse = true;
                                    }
                                    if (bUse) {
                                        string provName = authProvider.name;
                                        XmlElement thisBtn = base.addSubmit(ref oFrmElmt, "AuthProvider", "Sign In With " + provName, "AuthProvider", btnClass + " btn-"+ provName.ToLower(), btnIcon, provName.ToLower());
                                        thisBtn.SetAttribute("icon-left", "fab fa-" + provName.ToLower());
                                    }
                                }
                            }
                        }
                        //END auth provider

                        base.addDiv(ref oFrmElmt, "", "footer-override");
                        base.Instance.InnerXml = "<user rememberMe=\"\"><username/><password/></user>";

                    Check:
                        ;
                        XmlElement xmlGroupElmt = (XmlElement)base.moXformElmt.SelectSingleNode("group");
                        // Set the action URL
                        // Is the membership email address secure.
                        if (myWeb.moConfig["SecureMembershipAddress"] != "" & myWeb.moConfig["SecureMembershipAddress"] != null)
                        {
                            XmlElement oSubElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant::submission");
                            oSubElmt.SetAttribute("action", myWeb.moConfig["SecureMembershipAddress"] + myWeb.moConfig["ProjectPath"] + "/" + myWeb.mcPagePath);
                        }

                        // Set the remember me value, username from cookie.
                        if (bRememberMe)
                        {

                            // Add elements to the form if not present
                            if (Xml.NodeState(ref base.model, "bind[@id='cRemember']") == XmlNodeState.NotInstantiated)
                            {
                                
                                oSelElmt = base.addSelect(ref xmlGroupElmt, "cRemember", true, "&#160;", "", ApperanceTypes.Full);
                                base.addOption(ref oSelElmt, "Remember me", "true");
                                XmlElement oBindParent1 = null;
                                base.addBind("cRemember", "user/@rememberMe", ref oBindParent1, "false()");
                            }

                            // Retrieve values from the cookie
                            if (goRequest.Cookies["RememberMeUserName"] != null)
                            {
                                string cRememberedUsername = goRequest.Cookies["RememberMeUserName"].Value;
                                bool bRemembered = false;
                                XmlElement oElmt = null;

                                if (!string.IsNullOrEmpty(cRememberedUsername))
                                    bRemembered = true;
                                XmlElement baseInstanceElmt = (XmlElement)base.Instance;
                                if (Xml.NodeState(ref baseInstanceElmt, "user", "", "", XmlNodeState.NotInstantiated, oElmt) != XmlNodeState.NotInstantiated & !base.isSubmitted())
                                {

                                    oElmt.SetAttribute("rememberMe", Strings.LCase(Conversions.ToString(bRemembered)));
                                    Xml.NodeState(ref baseInstanceElmt, "user/username", cRememberedUsername);

                                }
                            }
                        }

                        base.updateInstanceFromRequest();

                        if (!string.IsNullOrEmpty(myWeb.moRequest["LoginResendActivation"]))
                        {
                            if (mnUserId == 0)
                            {
                                mnUserId = Convert.ToInt64(moDbHelper.ExeProcessSqlScalar("select nDirKey from tblDirectory where cDirName like '" + base.Instance.SelectSingleNode("user/username").InnerText + "'"));
                            }
                            ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                            IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);
                            oMembershipProv.Activities.sendRegistrationAlert(ref myWeb, mnUserId, false, cmdPrefix);

                            base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, "sValidResponse", true);

                            base.NewFrm("ActivateAccount");
                            XmlElement oFrmGrp2 = (XmlElement)base.addGroup(ref base.moXformElmt, "ActivateAccount");
                            var oMembership = new Cms.Membership(ref myWeb);                      
                            addNote(ref oFrmGrp2, noteTypes.Hint, "<span class=\"msg-1036\">Your Activation Code has been resent</span>", true, "msg-1036");                         

                            return base.moXformElmt;
                        }
                        else
                        {

                            if (base.isSubmitted())
                            {

                                //check for 
                                //Add code to redirect SAML Auth using Google                             
                                if (!string.IsNullOrEmpty(myWeb.moRequest["AuthProvider"]))
                                {
                                    string selectedProvider = myWeb.moRequest["AuthProvider"];
                                    foreach (IauthenticaitonProvider authProvider in oAuthProviders)
                                    {
                                        string redirectUrl = authProvider.GetAuthenticationURL();

                                        if (!string.IsNullOrEmpty(redirectUrl))
                                        {
                                            myWeb.moResponse.Redirect(redirectUrl); // Redirects browser to SAML login                                            
                                        }
                                    }
                                }                               

                                base.validate();
                                if (base.valid)
                                {

                                    // changed to get from instance rather than direct from querysting / form.
                                    string username = base.Instance.SelectSingleNode("user/username").InnerText;
                                    string password = base.Instance.SelectSingleNode("user/password").InnerText;

                                    sValidResponse = moDbHelper.validateUser(username, password);

                                    if (Information.IsNumeric(sValidResponse))
                                    {
                                        myWeb.mnUserId = Convert.ToInt32(sValidResponse);
                                        moDbHelper.mnUserId = Conversions.ToLong(sValidResponse);
                                        if (goSession != null)
                                        {
                                            goSession["nUserId"] = myWeb.mnUserId;

                                            XmlElement UserXml = myWeb.GetUserXML();
                                            if (!string.IsNullOrEmpty(UserXml.GetAttribute("defaultCurrency")))
                                            {
                                                goSession["cCurrency"] = UserXml.GetAttribute("defaultCurrency");
                                            }
                                        }
                                        // Set the remember me cookie
                                        if (bRememberMe)
                                        {
                                            if (goRequest["cRemember"] == "true")
                                            {
                                                System.Web.HttpCookie oCookie;
                                                if (myWeb.moRequest.Cookies["RememberMeUserName"] != null)
                                                    goResponse.Cookies.Remove("RememberMeUserName");
                                                oCookie = new System.Web.HttpCookie("RememberMeUserName");
                                                oCookie.Value = myWeb.moRequest["cUserName"];
                                                oCookie.Expires = DateAndTime.DateAdd(DateInterval.Day, 60d, DateTime.Now);
                                                goResponse.Cookies.Add(oCookie);

                                                if (myWeb.moRequest.Cookies["RememberMeUserId"] != null)
                                                    goResponse.Cookies.Remove("RememberMeUserId");
                                                oCookie = new System.Web.HttpCookie("RememberMeUserId");
                                                oCookie.Value = Convert.ToString(myWeb.mnUserId);
                                                oCookie.Expires = DateAndTime.DateAdd(DateInterval.Day, 60d, DateTime.Now);
                                                goResponse.Cookies.Add(oCookie);
                                            }
                                            else
                                            {
                                                goResponse.Cookies["RememberMeUserName"].Expires = DateTime.Now.AddDays(-1);
                                                goResponse.Cookies["RememberMeUserId"].Expires = DateTime.Now.AddDays(-1);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        base.valid = false;
                                        base.addNote(ref xmlGroupElmt, Protean.xForm.noteTypes.Alert, sValidResponse, true);

                                        if (sValidResponse.Contains("msg-1021"))
                                        {
                                            //remove password
                                            XmlNode passwordNode = moXformElmt.SelectSingleNode("descendant-or-self::secret");
                                            oFrmElmt = (XmlElement)passwordNode.ParentNode;
                                            oFrmElmt.RemoveChild(passwordNode);
                                            //remove submit mode
                                            XmlNode SubmitNode = moXformElmt.SelectSingleNode("descendant-or-self::submit");
                                            oFrmElmt = (XmlElement)SubmitNode.ParentNode;
                                            oFrmElmt.RemoveChild(SubmitNode);
                                            base.addSubmit(ref oFrmElmt, "ewSubmit", "Resend Activation Code", "LoginResendActivation", default, "fa-solid fa-right-to-bracket");
                                        }
                                    }
                                }
                                else
                                {
                                    valid = false;
                                }
                                if (valid == false)
                                {
                                    myWeb.mnUserId = 0;
                                }
                            }


                            base.addValues();
                            return base.moXformElmt;
                        }
                      
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmUserLogon", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                XmlElement IMembershipAdminXforms.xFrmPasswordReminder()  // Just replace Overridable to Overrides
                {
                    XmlElement oFrmElmt;
                    string sValidResponse;
                    string cProcessInfo = "";
                    //bool getRecordByEmail;
                    try
                    {

                        // Does the configuration setting indicate that email addresses are allowed.
                        if (myWeb.moConfig["EmailUsernames"] != null)
                        {
                            if ((myWeb.moConfig["EmailUsernames"].ToLower()) == "on")
                            {
                                //getRecordByEmail = true;
                            }
                            else
                            {
                                //getRecordByEmail = false;
                            }
                        }
                        else
                        {
                            //getRecordByEmail = false;
                        }

                        base.NewFrm("PasswordReminder");

                        base.submission("PasswordReminder", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "PasswordReminder");
                        base.addDiv(ref oFrmElmt, "Please enter your email address and we will email you with a reset link..", "term-1038");
                        base.addInput(ref oFrmElmt, "cEmail", true, "Email Address");
                        XmlElement oBindParent = null;
                        base.addBind("cEmail", "user/email", ref oBindParent, "true()", "email");

                        base.addSubmit(ref oFrmElmt, "", "Reset Password", "ewSubmitReminder");

                        if (!base.load("/xforms/passwordreminder.xml", myWeb.maCommonFolders))
                        {
                            base.NewFrm("PasswordReminder");

                            base.submission("PasswordReminder", "", "post", "form_check(this)");

                            oFrmElmt = base.addGroup(ref base.moXformElmt, "PasswordReminder");

                            base.addDiv(ref oFrmElmt, "Please enter your email address and we will email you with a reset link.", "term-1038");

                            base.addInput(ref oFrmElmt, "cEmail", true, "Email Address");
                            XmlElement oBindParent2 = null;
                            base.addBind("cEmail", "user/email", ref oBindParent2, "true()", "email");

                            base.addSubmit(ref oFrmElmt, "", "Reset Password", "ewSubmitReminder");
                            base.Instance.InnerXml = "<user><email/></user>";
                        }
                        else
                        {
                            oFrmElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::group[1]");
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // Get the data using either the email address or username dependant upon whether the application is
                                // congigured to store usernames as email addresses.
                                sValidResponse = moDbHelper.passwordReminder(goRequest["cEmail"]);

                                if (sValidResponse == "Your reset link has been emailed to you")
                                {
                                    // remove old form
                                    valid = true;
                                    foreach (XmlElement oElmt in oFrmElmt.SelectNodes("*"))
                                        oFrmElmt.RemoveChild(oElmt);
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, sValidResponse, true, "msg-1037");
                                }
                                else
                                {
                                    valid = false;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, sValidResponse, true, "msg-1037");
                                }
                            }
                            else
                            {
                                valid = false;
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

                XmlElement IMembershipAdminXforms.xFrmActivateAccount()
                {
                    XmlElement oFrmElmt;

                    // Dim sValidResponse As String
                    string cProcessInfo = "";
                    try
                    {
                        // Find a user account with the right activation code.

                        // Change the account status and delete the activation code.

                        // AddHandler oMembership.OnError, AddressOf OnComponentError


                        base.NewFrm("ActivateAccount");
                        XmlElement oFrmGrp2 = (XmlElement)base.addGroup(ref base.moXformElmt, "ActivateAccount");

                        var oMembership = new Cms.Membership(ref myWeb);
                        if (oMembership.ActivateAccount(moRequest["key"]))
                        {
                            addNote(ref oFrmGrp2, noteTypes.Hint, "<span class=\"msg-1036\">Your account is now activated please logon</span>", true, "msg-1036");
                        }
                        else {
                            addNote(ref oFrmGrp2, noteTypes.Hint, "<span class=\"msg-1037\">This activation code has allready been used or is invalid</span>", true, "msg-10376");
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmActivateAccount", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public override XmlElement xFrmResetAccount(long userId = 0L)
                {

                    XmlElement oFrmElmt;
                    string cSQL;
                    string cResponse;
                    string cProcessInfo = "";
                    bool areEmailAddressesAllowed;
                    string cEmailAddress = "";
                    DataSet dsUsers;
                    int nNumberOfUsers;
                    DataRow oUserDetails;
                    string cUsername;
                    var isValidEmailAddress = default(bool);
                    string formTitle;
                    try
                    {

                        // Does the configuration setting indicate that email addresses are allowed.
                        // Does the configuration setting indicate that email addresses are allowed.
                        if (myWeb.moConfig["EmailUsernames"] != null)
                        {
                            if ((myWeb.moConfig["EmailUsernames"].ToLower()) == "on")
                            {
                                areEmailAddressesAllowed = true;
                            }
                            else { areEmailAddressesAllowed = false; }

                        }
                        else
                        {
                            areEmailAddressesAllowed = false;
                        }

                        if (userId > 0L)
                        {
                            // populate the user id in the form

                            cSQL = "SELECT cDirName, cDirEmail FROM tblDirectory WHERE cDirSchema = 'User' and nDirKey = '" + userId + "'";
                            dsUsers = myWeb.moDbHelper.GetDataSet(cSQL, "tblTemp");

                            oUserDetails = dsUsers.Tables[0].Rows[0];
                            cEmailAddress = Conversions.ToString(oUserDetails["cDirName"]);
                            isValidEmailAddress = EmailAddressCheck(cEmailAddress);
                            formTitle = "<span class=\"msg-1030\">Send account reset message to user.</span>";
                        }
                        else
                        {
                            formTitle = "<span class=\"msg-028\">Please enter your email address and we will email you a reset link.</span>";
                        }


                        string FormName = "ResetAccount";

                        // maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        if (!base.load("/xforms/directory/" + FormName + ".xml", myWeb.maCommonFolders))
                        {
                            // If this does not load manually then build a form to do it.
                            goto BuildForm;
                        }
                        else
                        {
                            oFrmElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::group[position()=1]");
                            goto Check;
                        }

                    BuildForm:
                        ;


                        base.NewFrm("ResetAccount");

                        base.submission("ResetAccount", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "ResetAccount", "", formTitle);

                        if (userId > 0L)
                        {
                            if (isValidEmailAddress)
                            {
                                base.addInput(ref oFrmElmt, "cEmail", true, "Email address", "readonly");
                            }
                            else
                            {
                                base.addInput(ref oFrmElmt, "cEmail", true, "Username", "readonly");
                            }
                        }
                        else
                        {
                            base.addInput(ref oFrmElmt, "cEmail", true, "Email address / Username");
                        }
                        // check for legal chars in either email or username
                        XmlElement xmlObind = null;
                        base.addBind("cEmail", "user/email", ref xmlObind, "true()", "format:^[a-zA-Z0-9._%+-@ ]*$");

                        base.addSubmit(ref oFrmElmt, "", "Reset Password", "ewAccountReset");

                    Check:
                        ;


                        base.Instance.InnerXml = "<user><email>" + cEmailAddress + "</email></user>";
                        base.addValues();

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                cUsername = Instance.SelectSingleNode("user/email").InnerText;
                                // Returns true if cUsername is a valid email address.
                                isValidEmailAddress = EmailAddressCheck(cUsername);
                                if (isValidEmailAddress)
                                {
                                    if (areEmailAddressesAllowed == true)
                                    {
                                        cSQL = "SELECT nDirKey FROM tblDirectory WHERE cDirSchema = 'User' and cDirEmail = '" + Strings.LCase(cUsername) + "'";
                                    }
                                    else
                                    {
                                        cSQL = "SELECT nDirKey FROM tblDirectory WHERE cDirSchema = 'User' and cDirXml like '%<Email>" + Strings.LCase(cUsername) + "</Email>%'";
                                    }
                                }
                                else if (areEmailAddressesAllowed == true)
                                {
                                    cSQL = "SELECT nDirKey, cDirEmail FROM tblDirectory WHERE cDirSchema = 'User' and cDirName = '" + Strings.LCase(cUsername) + "'";
                                }
                                else
                                {
                                    cSQL = "SELECT nDirKey FROM tblDirectory WHERE cDirSchema = 'User' and cDirName = '" + Strings.LCase(cUsername) + "'";

                                }

                                dsUsers = myWeb.moDbHelper.GetDataSet(cSQL, "tblTemp");
                                nNumberOfUsers = dsUsers.Tables[0].Rows.Count;

                                if (nNumberOfUsers == 0)
                                {
                                    cResponse = "<span class=\"msg-1026\">There was a problem resetting this account, the account was not found.</span>";
                                }
                                else if (nNumberOfUsers > 1 & areEmailAddressesAllowed == true)
                                {
                                    cResponse = "<span class=\"msg-1027\">There was a problem resetting this account, email address is ambiguous. Please contact the website administrator</span>";
                                }
                                else
                                {
                                    oUserDetails = dsUsers.Tables[0].Rows[0];
                                    int nAcc = Conversions.ToInteger(oUserDetails["nDirKey"]);
                                    ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                    IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);
                                    //Providers.Membership.ReturnProvider oMembershipProv = new Providers.Membership.ReturnProvider.Get(myWeb, myWeb.moConfig["MembershipProvider"]);

                                    cResponse = Conversions.ToString(oMembershipProv.Activities.ResetUserAcct(ref myWeb, nAcc));

                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, cResponse, true);


                                }
                                if (!string.IsNullOrEmpty(cResponse))
                                {
                                    oFrmElmt.ParentNode.RemoveChild(oFrmElmt);
                                    oFrmElmt = base.addGroup(ref base.moXformElmt, "ResetAccount", "", "");
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, cResponse, true);
                                }
                            }
                            else
                            {
                                cResponse = "<span class=\"msg-1028\">There was a problem resetting this account. Please contact the website administrator</span>";
                            } // endif MyBase.valid

                        } // endif MyBase.isSubmitted

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmResetAccount", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }

                }


                public XmlElement xFrmResetPassword(long nAccount)
                {
                    try
                    {

                        XmlElement moPolicy;
                        string passwordClass = "required";
                        string passwordValidation = null;
                        XmlElement oPI1;
                        XmlElement oPI2;
                        XmlElement oSB;
                        XmlElement oGrp;
                        moPolicy = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy");

                        if (moPolicy != null)
                        {
                            passwordClass = "required strongPassword";
                            passwordValidation = "strongPassword";
                        }

                        string FormName = "ResetPassword";

                        // maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        if (!base.load("/xforms/directory/" + FormName + ".xml", myWeb.maCommonFolders))
                        {
                            // If this does not load manually then build a form to do it.
                            goto BuildForm;
                        }
                        else
                        {
                            oGrp = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::group[position()=1]");
                            oPI1 = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::secret[@bind='cDirPassword']");
                            oPI2 = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::secret[@bind='cDirPassword2']");
                            oSB = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::submit[@submission='SetPassword']");
                            goto Check;
                        }

                    BuildForm:
                        ;
                        base.NewFrm("ResetPassword");

                        base.Instance.InnerXml = "<Password><cDirPassword/><cDirPassword2/></Password>";

                        oGrp = base.addGroup(ref base.moXformElmt, "Password", default, "Reset Password");
                        base.submission("SetPassword", "", "POST");
                        oPI1 = base.addSecret(ref oGrp, "cDirPassword", true, "Password", ref passwordClass);
                        XmlElement oxmlBind = null;
                        base.addBind("cDirPassword", "Password/cDirPassword", ref oxmlBind, "true()", passwordValidation);
                        string strrequired = "required";
                        oPI2 = base.addSecret(ref oGrp, "cDirPassword2", true, "Confirm Password", ref strrequired);
                        base.addBind("cDirPassword2", "Password/cDirPassword2", ref oxmlBind, "true()");
                        oSB = (XmlElement)base.addSubmit(ref oGrp, "SetPassword", "Set Password");

                    Check:
                        ;


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            // any additonal validation goes here
                            // Passwords match?
                            if ((goRequest["cDirPassword2"]).Length > 0)
                            {
                                if (goRequest["cDirPassword"] != goRequest["cDirPassword2"])
                                {
                                    base.valid = false;
                                    base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must match ");
                                }
                            }
                            if (moPolicy is null)
                            {
                                // Password policy?
                                if ((base.Instance.SelectSingleNode("Password/cDirPassword").InnerXml).Length < 4)
                                {
                                    base.valid = false;
                                    base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must be 4 characters long ");
                                }
                            }
                            if (base.valid)
                            {
                                var oMembership = new Cms.Membership(ref myWeb);

                                if (!oMembership.ReactivateAccount((Int16)nAccount, goRequest["cDirPassword"]))
                                {
                                    base.addNote("cDirPassword2", Protean.xForm.noteTypes.Alert, "There was a problem changing the password");
                                    base.valid = false;
                                }
                                else
                                {
                                    base.addNote(ref oGrp, Protean.xForm.noteTypes.Alert, "The password has been updated.");
                                    oPI1.ParentNode.RemoveChild(oPI1);
                                    oPI2.ParentNode.RemoveChild(oPI2);
                                    oSB.ParentNode.RemoveChild(oSB);
                                }
                            }
                        }
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmResetPassword", ex, "", "", gbDebug);
                        return null;
                    }
                }

                public override XmlElement xFrmConfirmPassword(string AccountHash)
                {
                    try
                    {
                        var oMembership = new Cms.Membership(ref myWeb);
                        int SubmittedUserId = Convert.ToInt32("0" + goRequest["id"]);

                        int nUserId = oMembership.DecryptResetLink(SubmittedUserId, AccountHash);

                        return xFrmConfirmPassword(nUserId);
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "addInput", ex, "", "", gbDebug);
                        return null;
                    }
                }

                public override XmlElement xFrmConfirmPassword(long nUserId)
                {
                    try
                    {
                        XmlElement moPolicy;
                        string passwordClass = "required";
                        string passwordValidation = null;
                        XmlElement oPI1;
                        XmlElement oPI2;
                        XmlElement oSB;
                        XmlElement oGrp;

                        moPolicy = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy");

                        if (moPolicy != null)
                        {
                            passwordClass = "required strongPassword";
                            passwordValidation = "strongPassword";
                        }

                        string FormName = "ConfirmPassword";

                        // maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        if (!base.load("/xforms/directory/" + FormName + ".xml", myWeb.maCommonFolders))
                        {
                            // If this does not load manually then build a form to do it.
                            goto BuildForm;
                        }
                        else
                        {
                            oGrp = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::group[position()=1]");
                            oPI1 = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::secret[@bind='cDirPassword']");
                            oPI2 = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::secret[@bind='cDirPassword2']");
                            oSB = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::submit[@submission='SetPassword']");
                            goto Check;
                        }

                    BuildForm:

                        base.NewFrm("ConfirmPassword");
                        XmlElement xmlObindparent = null;
                        base.Instance.InnerXml = "<Password><cDirPassword/><cDirPassword2/></Password>";
                        string formTitle = "<span class=\"trans-2031\">Enter new password</span>";
                        oGrp = base.addGroup(ref base.moXformElmt, "Password", default, formTitle);
                        base.submission("SetPassword", "", "POST");
                        oPI1 = base.addSecret(ref oGrp, "cDirPassword", true, "Password", ref passwordClass);
                        base.addBind("cDirPassword", "Password/cDirPassword", ref xmlObindparent, "true()", passwordValidation);
                        string strReqSecr = "required secret";
                        oPI2 = base.addSecret(ref oGrp, "cDirPassword2", true, "Confirm Password", ref strReqSecr);
                        base.addBind("cDirPassword2", "Password/cDirPassword2", ref xmlObindparent, "true()");
                        oSB = (XmlElement)base.addSubmit(ref oGrp, "SetPassword", "Set Password", "", "principle", "", "");

                        int nAccount = (int)nUserId;
                        if (nAccount == 0)
                        {
                            oGrp.InnerXml = "";
                            //XmlNode GrpNode = (XmlNode)oGrp;
                            base.addNote(ref oGrp, Protean.xForm.noteTypes.Alert, "This reset link has already been used");
                            base.valid = false;
                        }

                    Check:
                        ;

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            // any additonal validation goes here
                            // Passwords match?
                            if ((goRequest["cDirPassword2"]).Length > 0)
                            {
                                if (goRequest["cDirPassword"] != goRequest["cDirPassword2"])
                                {
                                    base.valid = false;
                                    base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must match ");
                                }
                            }

                            // Password policy?
                            if ((base.Instance.SelectSingleNode("Password/cDirPassword").InnerXml).Length < 4)
                            {
                                base.valid = false;
                                base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must be 4 characters long ");
                            }

                            var oMembership = new Cms.Membership(ref myWeb);

                            if (moPolicy != null)
                            {
                                // Password History?
                                if (!oMembership.CheckPasswordHistory(Convert.ToInt32(nUserId), goRequest["cDirPassword"]))
                                {
                                    base.valid = false;
                                    base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "<span class=\"msg-1020\">You cannot use a password you have used recently.</span>");
                                }
                            }

                            if (base.valid)
                            {

                                nAccount = (int)nUserId;
                                if (!oMembership.ReactivateAccount(nAccount, goRequest["cDirPassword"]))
                                {
                                    oGrp.InnerXml = "";
                                    base.addNote(ref oGrp, Protean.xForm.noteTypes.Alert, "<span class=\"msg-1022\">There was a problem activating your account</span>");
                                    base.valid = false;
                                }
                                else
                                {
                                    base.addNote(ref oGrp, Protean.xForm.noteTypes.Alert, "Your password has been reset <a href=\"/" + myWeb.moConfig["LogonRedirectPath"] + "\">click here</a> to sign in");
                                    oPI1.ParentNode.RemoveChild(oPI1);
                                    oPI2.ParentNode.RemoveChild(oPI2);
                                    oSB.ParentNode.RemoveChild(oSB);
                                    // delete failed logon attempts record
                                    string sSql = $"delete from tblActivityLog where nActivityType = {Convert.ToString((int)dbHelper.ActivityType.LogonInvalidPassword)} and nUserDirId={nAccount}";
                                    myWeb.moDbHelper.ExeProcessSql(sSql);

                                    if (myWeb.mnUserId == 0)
                                    {
                                        myWeb.mnUserId = nAccount;
                                    }
                                    myWeb.msRedirectOnEnd = myWeb.moConfig["LogonRedirectPath"].ToString();
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "addInput", ex, "", "", gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmEditDirectoryItem(ref XmlElement IntanceAppend, long id = 0L, string cDirectorySchemaName = "User", long parId = 0L, string cXformName = "", string FormXML = "")
                {

                    XmlElement oGrpElmt;
                    string cProcessInfo = "";
                    string cCurrentPassword = "";
                    string cCodeUsed = "";
                    bool addNewitemToParId = false;
                    XmlElement moPolicy;

                    try
                    {

                        moPolicy = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy");

                        bool formFound = true;

                        if (string.IsNullOrEmpty(cXformName))
                            cXformName = cDirectorySchemaName;

                        // ok lets load in an xform from the file location.
                        if (string.IsNullOrEmpty(FormXML))
                        {
                            string formPath = "/xforms/directory/" + cXformName + ".xml";
                            if (myWeb.moConfig["cssFramework"] == "bs5")
                            {
                                formPath = "/features/membership/" + cXformName + ".xml";
                            }
                            if (IntanceAppend != null)
                            {
                                base.bProcessRepeats = false;
                            }
                            if (!base.load(formPath, myWeb.maCommonFolders))
                            {
                                // load a default content xform if no alternative.
                                base.NewFrm(cXformName);

                                formFound = false;
                            }
                        }
                        else
                        {
                            base.NewFrm(cXformName);
                            base.loadtext(FormXML);

                        }
                        if (formFound)
                        {
                            if (id > 0L)
                            {
                                base.Instance.InnerXml = moDbHelper.getObjectInstance(Protean.Cms.dbHelper.objectTypes.Directory, id);
                                cCurrentPassword = Instance.SelectSingleNode("*/cDirPassword").InnerText;
                            }

                            if (IntanceAppend != null)
                            {
                                if (goSession[$"tempInstance-{FormName}"] != null)
                                {
                                    base.Instance = (XmlElement)goSession[$"tempInstance-{FormName}"];
                                    base.bProcessRepeats = true;
                                    base.LoadInstance(base.Instance, true);
                                    goSession[$"tempInstance-{FormName}"] = base.Instance;
                                }
                                else
                                {
                                    // this enables an overload to add additional Xml for updating.
                                    var importedNode = Instance.OwnerDocument.ImportNode(IntanceAppend, true);
                                    base.Instance.AppendChild(importedNode);
                                    base.bProcessRepeats = true;
                                    base.LoadInstance(base.Instance, true);
                                    goSession[$"tempInstance-{FormName}"] = base.Instance;
                                }
                            }

                            if (base.Instance.SelectSingleNode("tblDirectory/cDirSchema") != null)
                            {
                                cDirectorySchemaName = base.Instance.SelectSingleNode("tblDirectory/cDirSchema").InnerText;
                            }
                            else
                            {
                                base.addNote(ref moXformElmt, Protean.xForm.noteTypes.Alert, "xForm does not specify Schema Name");
                            }



                            // lets add the groups to the instance
                            oGrpElmt = moDbHelper.getGroupsInstance(id, parId);
                            base.Instance.InsertAfter(oGrpElmt, base.Instance.LastChild);

                            if (cDirectorySchemaName == "User")
                            {

                                if (goConfig["Subscriptions"] == "on")
                                {
                                    var oSub = new Cart.Subscriptions(ref myWeb);
                                    XmlElement xmlXformInstance = base.Instance;
                                    oSub.AddSubscriptionToUserXML(ref xmlXformInstance, Convert.ToInt32(id));
                                }

                                // now lets check our security, and if we are encrypted lets not show the password on edit.
                                if (id > 0L)
                                {
                                    // RJP 7 Nov 2012. Added LCase as a precaution against people entering string in Protean.Cms.Config lowercase, i.e. md5.
                                    if (myWeb.moConfig["MembershipEncryption"] != null)
                                    {
                                        if ((myWeb.moConfig["MembershipEncryption"].ToLower()).StartsWith("md5") | (myWeb.moConfig["MembershipEncryption"].ToLower()).StartsWith("sha"))
                                        {
                                            // Remove password (and confirm password) fields
                                            foreach (XmlElement oPwdNode in base.moXformElmt.SelectNodes("/group/descendant-or-self::*[contains(@bind,'cDirPassword')]"))
                                                oPwdNode.ParentNode.RemoveChild(oPwdNode);
                                        }
                                    }

                                }

                                // Is the membership email address secure.
                                if ((myWeb.moConfig["SecureMembershipAddress"] != "" & myWeb.moConfig["SecureMembershipAddress"] != null) & myWeb.mbAdminMode == false)
                                {
                                    XmlElement oSubElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant::submission");
                                    if (myWeb.mcPagePath is null)
                                    {
                                        oSubElmt.SetAttribute("action", myWeb.moConfig["SecureMembershipAddress"] + myWeb.mcOriginalURL);
                                    }
                                    else
                                    {
                                        oSubElmt.SetAttribute("action", myWeb.moConfig["SecureMembershipAddress"] + myWeb.moConfig["ProjectPath"] + "/" + myWeb.mcPagePath);
                                    }
                                }
                            }

                            if (base.isSubmitted())
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                // any additonal validation goes here
                                switch (cDirectorySchemaName ?? "")
                                {
                                    case "User":
                                    case "UserMyAccount":
                                        {
                                            if (base.valid == false)
                                            {
                                                base.addNote("cDirName", Protean.xForm.noteTypes.Alert, base.validationError);
                                            }


                                            // Username exists?
                                            if (base.Instance.SelectSingleNode("*/cDirName").InnerXml != base.Instance.SelectSingleNode("*/cDirXml/User/Email").InnerXml)
                                            {
                                                if (!moDbHelper.checkUserUnique(base.Instance.SelectSingleNode("*/cDirName").InnerXml, id))
                                                {
                                                    base.valid = false;
                                                    base.addNote("cDirName", Protean.xForm.noteTypes.Alert, "This username already exists please select another");
                                                }
                                            }

                                            // Email Exists?
                                            if (!moDbHelper.checkEmailUnique(base.Instance.SelectSingleNode("*/cDirXml/User/Email").InnerXml, id))
                                            {
                                                base.valid = false;
                                                if (base.Instance.SelectSingleNode("*/cDirName").InnerXml == base.Instance.SelectSingleNode("*/cDirXml/User/Email").InnerXml)
                                                {
                                                    base.addNote("cEmail", Protean.xForm.noteTypes.Alert, "<span class=\"msg-1031\">This email address already has an account, please use password reminder facility.</span>");
                                                    base.addNote("cDirName", Protean.xForm.noteTypes.Alert, "<span class=\"msg-1031\">This email address already has an account, please use password reminder facility.</span>");
                                                }
                                                else
                                                {
                                                    base.addNote("cEmail", Protean.xForm.noteTypes.Alert, "<span class=\"msg-1031\">This email address already has an account, please use password reminder facility.</span>");
                                                }

                                            }

                                            // Only validate passwords if form contains the fields.
                                            if (base.moXformElmt.SelectSingleNode("/group/descendant-or-self::*[@bind='cDirPassword']") != null)
                                            {
                                                // Passwords match?
                                                if (goRequest["cDirPassword2"] != null)
                                                {
                                                    if ((goRequest["cDirPassword2"]).Length > 0)
                                                    {
                                                        if (goRequest["cDirPassword"] != goRequest["cDirPassword2"])
                                                        {
                                                            base.valid = false;
                                                            base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must match ");
                                                        }
                                                    }
                                                }

                                                if (moPolicy != null)
                                                {
                                                    // Password policy?
                                                    if ((base.Instance.SelectSingleNode("*/cDirPassword").InnerXml).Length < 4)
                                                    {
                                                        base.valid = false;
                                                        base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must be 4 characters long ");
                                                    }
                                                }
                                            }

                                            // Email exists...?

                                            // Membership codes
                                            cCodeUsed = validateMemberCode("*/RegistrationCode", "RegistrationCode");
                                            break;
                                        }

                                }
                                if (base.valid)
                                {

                                    string cPassword = Instance.SelectSingleNode("*/cDirPassword").InnerText;
                                    string cClearPassword = cPassword;
                                    // RJP 7 Nov 2012. Added LCase to MembershipEncryption. Note leave the value below for md5Password hard coded as MD5.
                                    if ((myWeb.moConfig["MembershipEncryption"].ToLower()) == "md5salt")
                                    {
                                        string cSalt = Encryption.generateSalt();
                                        string inputPassword = string.Concat(cSalt, cPassword); // Take the users password and add the salt at the front
                                        string md5Password = Encryption.HashString(inputPassword, "md5", true); // Md5 the marged string of the password and salt
                                        string resultPassword = string.Concat(md5Password, ":", cSalt); // Adds the salt to the end of the hashed password
                                        cPassword = resultPassword; // Store the resultant password with salt in the database
                                    }
                                    else
                                    {
                                        cPassword = Encryption.HashString(cPassword, (myWeb.moConfig["MembershipEncryption"].ToLower()), true);
                                    } // plain - md5 - sha1
                                    if (!((cPassword ?? "") == (cCurrentPassword ?? "")) & !((cClearPassword ?? "") == (cCurrentPassword ?? "")))
                                    {
                                        Instance.SelectSingleNode("*/cDirPassword").InnerText = cPassword;
                                    }

                                    if (id > 0L)
                                    {

                                        moDbHelper.setObjectInstance(dbHelper.objectTypes.Directory, base.Instance, id);
                                        if (moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='EditContent' or @bind='EditContent']") != null)
                                        {
                                            base.addNote("EditContent", Protean.xForm.noteTypes.Alert, "<span class=\"msg-1010\">Your details have been updated.</span>", true);
                                        }
                                        else
                                        {
                                            XmlElement oSubElmt = (XmlElement)moXformElmt.SelectSingleNode("descendant-or-self::group[parent::Content][1]");
                                            if (oSubElmt != null)
                                            {
                                                base.addNote(ref oSubElmt, Protean.xForm.noteTypes.Alert, "<span class=\"msg-1010\">Your details have been updated.</span>", true);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // add new
                                        id = Convert.ToInt64(moDbHelper.setObjectInstance(dbHelper.objectTypes.Directory, base.Instance));

                                        // update the instance with the id
                                        base.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText = id.ToString();
                                        addNewitemToParId = parId > 0L;
                                        base.addNote("EditContent", Protean.xForm.noteTypes.Alert, "This user has been added.", true);

                                        // add addresses
                                        if (base.Instance.SelectSingleNode("tblCartContact") != null)
                                        {
                                            foreach (XmlElement oCartContact in base.Instance.SelectNodes("tblCartContact"))
                                            {
                                                var TempInstance = new XmlDocument();
                                                TempInstance.LoadXml("<instance/>");
                                                TempInstance.DocumentElement.InnerXml = oCartContact.OuterXml;
                                                TempInstance.DocumentElement.SelectSingleNode("tblCartContact/nContactDirId").InnerText = id.ToString();
                                                moDbHelper.setObjectInstance(dbHelper.objectTypes.CartContact, TempInstance.DocumentElement);
                                            }
                                        }

                                        // Save the member code, if applicable
                                        useMemberCode(cCodeUsed, id);

                                        // If member codes were being applied then reconstruct the Group Instance.
                                        if (gbMemberCodes & !string.IsNullOrEmpty(cCodeUsed))
                                        {
                                            oGrpElmt = moDbHelper.getGroupsInstance(id, parId);
                                            base.Instance.ReplaceChild(oGrpElmt, base.Instance.LastChild);
                                        }

                                    }

                                    // lets add the user to any groups
                                    if ((cDirectorySchemaName == "User" | cDirectorySchemaName == "Company") & maintainMembershipsOnAdd)
                                    {
                                        maintainMembershipsFromXForm((int)id);

                                        // we want to ad the user to a specified group from a pick list of groups.
                                        XmlElement GroupsElmt = (XmlElement)base.Instance.SelectSingleNode("groups");
                                        if (GroupsElmt != null)
                                        {
                                            if (!string.IsNullOrEmpty(GroupsElmt.GetAttribute("addIds")))
                                            {
                                                foreach (var i in Strings.Split(GroupsElmt.GetAttribute("addIds"), ","))
                                                    moDbHelper.maintainDirectoryRelation(Conversions.ToLong(i), id, false);
                                            }
                                        }
                                        // code added by sonali for pure360
                                        if (cDirectorySchemaName == "User")
                                        {
                                            System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");

                                            Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                                            Protean.Providers.Messaging.IMessagingProvider oMessagingProv = RetProv.Get(ref myWeb, moMailConfig["messagingprovider"]);

                                            string ListId = moMailConfig["AllUsersList"];
                                            if (ListId != null)
                                            {
                                                Dictionary<string, string> valDict = new Dictionary<string, string>();

                                                valDict.Add("Email", Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/Email").InnerText);
                                                valDict.Add("FirstName", Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/FirstName").InnerText);
                                                valDict.Add("LastName", Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/LastName").InnerText);
                                                if (Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/Mobile") != null)
                                                {
                                                    valDict.Add("Mobile", Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/Mobile").InnerText);
                                                }
                                                string Name = Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/FirstName").InnerText;
                                                string Email = Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/Email").InnerText;

                                                oMessagingProv.Activities.AddToList(ListId, Name, Email, valDict);
                                            }

                                        }
                                    }

                                    if (addNewitemToParId)
                                    {
                                        moDbHelper.maintainDirectoryRelation(parId, id, false);
                                    }

                                }
                            }

                            base.addValues();
                            return base.moXformElmt;
                        }
                        else
                        {
                            return base.moXformElmt;
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmEditDirectoryItem", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public virtual XmlElement xFrmEditDirectoryItem(long id = 0L, string cDirectorySchemaName = "User", long parId = 0L, string cXformName = "", string FormXML = "")
                {
                    XmlElement argIntanceAppend = null;
                    return xFrmEditDirectoryItem(ref argIntanceAppend, id, cDirectorySchemaName, parId, cXformName, FormXML);
                }

                /// <summary>
                ///     Maintains membership between userid and groups as found in xForm
                /// </summary>
                /// <param name="nUserId">The user id to be associated with</param>
                /// <param name="cGroupNodeListXPath">The XPath from the xform instance to the group nodes.</param>
                /// <remarks>Group nodes membership is indicated by a boolean attribute "isMember"</remarks>
                public void maintainMembershipsFromXForm(int nUserId, string cGroupNodeListXPath = "groups/group", string Email = null, bool addOnly = false)
                {
                    myWeb.PerfMon.Log(mcModuleName, "maintainMembershipsFromXForm", "start");
                    string sSql = "";
                    // Dim oDr As SqlDataReader
                    var userMembershipIds = new List<int>();

                    try
                    {
                        // get the users current memberships
                        sSql = "select * from tblDirectoryRelation where nDirChildId  = " + nUserId;
                        using (SqlDataReader oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read())
                                userMembershipIds.Add(Conversions.ToInteger(oDr["nDirParentId"]));
                        }
                        foreach (XmlElement oElmt in base.Instance.SelectNodes(cGroupNodeListXPath))
                        {
                            // TS isLast forces an update everytime this loops not possible to tell if this will be the last time
                            bool bIsLast = true;
                            // If oElmt.NextSibling Is Nothing Then bIsLast = True
                            if (Strings.LCase(oElmt.GetAttribute("isMember")) == "true" | Strings.LCase(oElmt.GetAttribute("isMember")) == "yes")
                            {
                                // if user not in group
                                if (!userMembershipIds.Contains(Conversions.ToInteger(oElmt.GetAttribute("id"))))
                                {
                                    moDbHelper.maintainDirectoryRelation(Convert.ToInt64(oElmt.GetAttribute("id")), nUserId, false, default, default, Email, oElmt.GetAttribute("name"), bIsLast);
                                }
                            }

                            // if user is in group
                            else if (userMembershipIds.Contains(Conversions.ToInteger(oElmt.GetAttribute("id"))))
                            {
                                if (addOnly == false)
                                {
                                    moDbHelper.maintainDirectoryRelation(Convert.ToInt64(oElmt.GetAttribute("id")), nUserId, true, default, default, Email, oElmt.GetAttribute("name"), bIsLast);
                                }
                            }
                        }


                        myWeb.PerfMon.Log(mcModuleName, "maintainMembershipsFromXForm", "end");
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "maintainMembershipsFromXForm", ex, "", "", gbDebug);
                    }

                }

                public virtual XmlElement xFrmActivationCode(long nUserId, string cXformName = "ActivationCode", string cFormXml = "")
                {


                    string cProcessInfo = "";
                    string cCodeUsed = "";
                    try
                    {


                        // Load the form
                        if (gbMemberCodes)
                        {


                            if (string.IsNullOrEmpty(cFormXml))
                            {
                                if (!base.load("/xforms/directory/" + cXformName + ".xml", myWeb.maCommonFolders))
                                {


                                    // No form.

                                }
                            }
                            else
                            {
                                base.NewFrm(cXformName);
                                base.loadtext(cFormXml);
                            }

                            // Check if submitted
                            if (base.isSubmitted())
                            {
                                base.updateInstanceFromRequest();
                                base.validate();

                                string cCodeNode = "RegistrationCode";

                                // Check if we're limiting this to a codeset
                                string zcCodeSet = "";
                                XmlElement xmlXformInstance = base.Instance;
                                Xml.NodeState(ref xmlXformInstance, "//CodeSet", "", "", XmlNodeState.IsEmpty, null, "", zcCodeSet, bCheckTrimmedInnerText: false);

                                // Validate the code
                                cCodeUsed = validateMemberCode("//" + cCodeNode, cCodeNode, zcCodeSet);

                                // Invalidate if the user id and code are bad
                                if (base.valid & (!(nUserId > 0L) | string.IsNullOrEmpty(cCodeUsed)))
                                {

                                    base.valid = false;
                                    base.addNote(cCodeNode, noteTypes.Alert, "There was a problem using this code.  Please try another code, or contact the website team.");

                                }

                                // Prcess the valid form
                                if (base.valid)
                                {

                                    // Save the member code, if applicable
                                    useMemberCode(cCodeUsed, nUserId);

                                    // Add an indication that the form succeeded.

                                    XmlNode argoParent = base.model.SelectSingleNode("instance");
                                    XmlElement xmlargoParentElmt = (XmlElement)argoParent;
                                    Xml.addElement(ref xmlargoParentElmt, "formState", "success");

                                    // Clear out the form
                                    // Dim oGroup As XmlElement = Nothing
                                    // If NodeState(MyBase.moXformElmt, "group", , , , oGroup) <> XmlNodeState.NotInstantiated Then
                                    // oGroup.InnerXml = "<label>Activation Code successful</label><div>The activation code was applied successfully.</div>"
                                    // oGroup.SetAttribute("class", "activationcodesuccess")
                                    // End If

                                }

                            }

                            // Add in values
                            base.addValues();
                            return base.moXformElmt;
                        }

                        else
                        {
                            return null;
                        }
                    }



                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmActivationCode", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                /// <summary>
                /// Validates a member code in an xform.
                /// </summary>
                /// <param name="cXPathToCode">The xpath to the code in the instance</param>
                /// <param name="cBindOrReftoCode">The bind or ref of the node in the form</param>
                /// <returns>String - returns the code if a code was entered and it is not a valid code</returns>
                /// <remarks>
                /// <para>If a code was entered and it's not valid, then the xform will be invalidated.</para>
                /// <para>Note that if the xpath can't be found or a code has not been entered then the form will not be invalidated.</para>
                /// <para>This requires MemberCodes to be turned on in web.config</para>
                /// </remarks>
                private string validateMemberCode(string cXPathToCode, string cBindOrReftoCode, string cCodeSet = "")
                {
                    string cCode = "";
                    string cReturnCode = "";
                    try
                    {
                        // Check:
                        // - is member codes on
                        // - have we got an xpath
                        // - has a code been entered?
                        XmlElement xmlXformInstance = base.Instance;
                        if (gbMemberCodes && !string.IsNullOrEmpty(cXPathToCode) && Xml.NodeState(ref xmlXformInstance, cXPathToCode, "", "", XmlNodeState.IsEmpty, null, "", cCode, bCheckTrimmedInnerText: false) == XmlNodeState.HasContents)


                        {

                            if (myWeb.moDbHelper.CheckCode(cCode, cCodeSet))
                            {
                                cReturnCode = cCode;
                            }
                            else
                            {
                                // Invalidate the form
                                base.valid = false;
                                base.addNote(cBindOrReftoCode, Protean.xForm.noteTypes.Alert, "Activation Code Incorrect");
                            }

                        }

                        return cReturnCode;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "validateMemberCode", ex, "", "", gbDebug);
                        return "";
                    }
                }

                /// <summary>
                /// Registers a user against a member code, and adds the user to the code memberships
                /// </summary>
                /// <param name="cCode">The code to use</param>
                /// <param name="nUserId">The user to register the code against</param>
                private void useMemberCode(string cCode, long nUserId)
                {
                    try
                    {

                        if (gbMemberCodes & !string.IsNullOrEmpty(cCode))
                        {

                            // Use the code
                            myWeb.moDbHelper.UseCode(cCode, Convert.ToInt32(nUserId));

                            // Get the CSV list of directory items for membership
                            string cCodeCSVList = Convert.ToString(myWeb.moDbHelper.GetDataValue("SELECT tblCodes.cCodeGroups FROM tblCodes INNER JOIN tblCodes Child ON tblCodes.nCodeKey = Child.nCodeParentId WHERE (Child.cCode = '" + cCode + "')", default, default, ""));

                            // Process the List
                            foreach (string cDirId in cCodeCSVList.Split(','))
                            {
                                if (Information.IsNumeric(cDirId))
                                    moDbHelper.maintainDirectoryRelation(Convert.ToInt64(cDirId), nUserId, false, default, default);
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "useMemberCode", ex, "", "", gbDebug);
                    }
                }
                public new XmlElement xFrmEditDirectoryContact(long id = 0L, int nUID = 0, string xFormPath = "/xforms/directory/UserContact.xml")
                {
                    string cProcessInfo = "";
                    try
                    {


                        base.NewFrm("EditContact");
                        base.load(xFormPath, myWeb.maCommonFolders);

                        if (id > 0L)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, id);

                        }

                        // Add the countries list to the form
                        if (base.moXformElmt.SelectSingleNode("//select1[@bind='cContactCountry']") != null)
                        {
                            var oEc = new Cms.Cart(ref myWeb);
                            Cms.xForm argoXform = (Cms.xForm)this;
                            XmlElement argoCountriesDropDown = (XmlElement)base.moXformElmt.SelectSingleNode("//select1[@bind='cContactCountry']");
                            oEc.populateCountriesDropDown(ref argoXform, ref argoCountriesDropDown, "Billing Address");
                            oEc.close();
                            oEc = (Cms.Cart)null;
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            // MyBase.instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = myweb.mnUserId
                            base.addValues();
                            if (nUID > 0)
                            {
                                // if not supplied we do not want to overwrite it.
                                base.Instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = nUID.ToString();
                            }

                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, base.Instance, Conversions.ToLong(Interaction.IIf(id > 0L, id, -1)));
                                try
                                {
                                    var argoNode = base.moXformElmt.SelectSingleNode("group/group(1)");
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Successfully Updated", true);
                                }
                                catch (Exception)
                                {
                                    var argoNode1 = base.moXformElmt.SelectSingleNode("group");
                                    base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "Successfully Updated", true);
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmEditDirectoryContact", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }
            }

            public class AdminProcess : Admin, IMembershipAdminProcess
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                public event OnErrorWithWebEventHandler OnErrorWithWeb;

                public delegate void OnErrorWithWebEventHandler(ref Cms myweb, object sender, Tools.Errors.ErrorEventArgs e);


                private Payment.DefaultProvider.AdminXForms _oAdXfm;

                public object oAdXfm
                {
                    set
                    {
                        _oAdXfm = (Payment.DefaultProvider.AdminXForms)value;
                    }
                    get
                    {
                        return _oAdXfm;
                    }
                }

                public AdminProcess(ref Cms aWeb) : base(ref aWeb)
                {
                }

                public void maintainUserInGroup(long nUserId, long nGroupId, bool remove, string cUserEmail = null, string cGroupName = null)
                {
                    myWeb.PerfMon.Log("Messaging", "maintainUserInGroup");
                    try
                    {
                    }

                    // do nothing this is a placeholder

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "maintainUserInGroup", ex, ""));
                    }
                }

            }


            public class Activities : IMembershipActivities
            {
                private const string mcModuleName = "Protean.Providers.Membership.Default.Activities";

                #region ErrorHandling

                public Activities()
                {
                    //empty constructor
                }

                // for anything controlling web
                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

                protected virtual void OnComponentError(ref Cms myWeb, object sender, Tools.Errors.ErrorEventArgs e) // Handles moDBHelper.OnErrorwithweb, oSync.OnError, moCalendar.OnError
                {
                    // deals with the error
                    Protean.Cms.dbHelper moDbHelper = myWeb.moDbHelper;

                    stdTools.returnException(ref myWeb.msException, e.ModuleName, e.ProcedureName, e.Exception, myWeb.mcEwSiteXsl, e.AddtionalInformation, gbDebug);
                    // close connection pooling
                    if (moDbHelper != null)
                    {
                        try
                        {
                            moDbHelper.CloseConnection();
                        }
                        catch (Exception)
                        {

                        }
                    }
                    // then raises a public event
                    OnError?.Invoke(sender, e);
                }

                #endregion

                public virtual long GetUserSessionId(ref Protean.Cms myWeb)
                {

                    string sProcessInfo = "";
                    System.Web.SessionState.HttpSessionState moSession = myWeb.moSession;
                    int mnUserId = myWeb.mnUserId;

                    try
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(moSession["nUserId"], 0, false)))
                        {
                            myWeb.mnUserId = Convert.ToInt32(moSession["nUserId"]);
                        }
                        else
                        {
                            myWeb.mnUserId = 0;
                        }

                        return myWeb.mnUserId;
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserSessionId", ex, sProcessInfo));
                        return default;
                    }
                }

                public string GetUserId(ref Protean.Cms myWeb)
                {
                    myWeb.PerfMon.Log(mcModuleName, "getUserId");
                    string sProcessInfo = "";
                    string sReturnValue = string.Empty;
                    string cLogonCmd = string.Empty;
                    int mnUserId = myWeb.mnUserId;
                    System.Web.SessionState.HttpSessionState moSession = myWeb.moSession;
                    System.Web.HttpRequest moRequest = myWeb.moRequest;
                    System.Web.HttpResponse moResponse = myWeb.moResponse;
                    System.Collections.Specialized.NameValueCollection moConfig = myWeb.moConfig;
                    Protean.Cms.dbHelper moDbHelper = myWeb.moDbHelper;
                    string sDomain = myWeb.moRequest.ServerVariables["HTTP_HOST"];

                    try
                    {
                        sProcessInfo = Conversions.ToString(Operators.ConcatenateObject(Interaction.IIf(myWeb.moRequest.ServerVariables["HTTPS"] == "on", "https://", "http://"), sDomain));
                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(moSession["nUserId"], null, false), mnUserId == 0)))
                        {

                            // first lets check for a remember me cookie
                            string rememberMeMode = moConfig["RememberMeMode"];
                            if (moRequest.Cookies["RememberMeUserId"] != null & rememberMeMode != "KeepCookieAfterLogoff" & !string.IsNullOrEmpty(rememberMeMode))
                            {
                                if (Information.IsNumeric(moRequest.Cookies["RememberMeUserId"].Value))
                                {
                                    // AG - MAJOR SECURITY FUBAR!!! Commenting out for now.
                                    // mnUserId = moRequest.Cookies("RememberMeUserId").Value
                                }
                            }


                            // If single user login is set we need to check if a cookie exists and reconnect if needs be
                            var oldSession = moRequest.Cookies["ewslock"];
                            if (gbSingleLoginSessionPerUser && oldSession != null)
                            {


                                // Find out what the last thing the user did (login-wise) was
                                if (!string.IsNullOrEmpty(oldSession.Value))
                                {

                                    // We need to find a few things:
                                    // 1. Find the last activity for the session id (within the timeout period)
                                    // 2. Work out if the user id for that session id has had a more recent session
                                    // 3. If not, then we can assume that the session is still alive and we need to update for this session.
                                    string lastSeenQuery = "SELECT nUserDirId FROM (SELECT TOP 1 nUserDirId,nACtivityType,cSessionId, (SELECT TOP 1 l2.cSessionId As sessionId FROM tblActivityLog l2 WHERE l2.nUserDirId = l.nUserDirId ORDER BY dDateTime DESC) As lastSessionForUser FROM tblActivityLog l " + "WHERE cSessionId = " + Protean.Tools.Database.SqlString(oldSession.Value) + " " + "AND DATEDIFF(s,l.dDateTime,GETDATE()) < " + gnSingleLoginSessionTimeout + " " + "ORDER BY dDateTime DESC) s WHERE s.cSessionId = s.lastSessionForUser AND s.nACtivityType <> " + dbHelper.ActivityType.Logoff;



                                    int lastSeenUser = Convert.ToInt32(moDbHelper.GetDataValue(lastSeenQuery, default, default, 0));
                                    if (lastSeenUser > 0)
                                    {

                                        // Reconnect with the new session ID
                                        mnUserId = lastSeenUser;
                                        moResponse.Cookies["ewslock"].Value = moSession.SessionID;
                                        moDbHelper.logActivity(dbHelper.ActivityType.SessionReconnectFromCookie, mnUserId, 0, 0, oldSession.Value);

                                    }
                                }
                            }
                        }
                        else if (Conversions.ToBoolean(Operators.OrObject(Operators.ConditionalCompareObjectEqual(moSession["nUserId"], null, false), Operators.ConditionalCompareObjectEqual(moSession["nUserId"], 0, false))))
                        {
                            // this will get set on close
                            if (Information.IsNumeric(moSession["PreviewUser"]))
                            {
                                mnUserId = Conversions.ToInteger(moSession["PreviewUser"]);
                                myWeb.mbPreview = true;
                            }
                        }
                        // If there is a user set, we need to check if we are transferring over to a secure site,
                        // to see if we should actually be logged off (which we can tell by looking at the cart order
                        // based on the session id).
                        else if (gbCart && !string.IsNullOrEmpty(moRequest["refSessionId"]))
                        {

                            long nCartUserId = 0L;

                            if (moDbHelper != null)
                            {
                                nCartUserId = Convert.ToInt64(moDbHelper.GetDataValue(Convert.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT nCartUserDirId FROM tblCartOrder o where o.cCartSchemaName='Order' and o.cCartSessionId = '", SqlFmt(moRequest["refSessionId"])), "'")), default, default, 0));
                            }

                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(nCartUserId, moSession["nUserId"], false)))
                            {
                                mnUserId = 0;
                            }
                            else
                            {
                                mnUserId = Conversions.ToInteger(moSession["nUserId"]);
                            }
                        }
                        else
                        {

                            // feature to turn on preview mode with supplied user ID if token provided then this is happening in alternativeauthentication
                            if (myWeb.moRequest["ewCmd"] == "PreviewOn" & Information.IsNumeric(myWeb.moRequest["PreviewUser"]))
                            {
                                myWeb.moSession["PreviewUser"] = myWeb.moRequest["PreviewUser"];
                            }


                            // lets finally set the user Id from the session
                            if (Information.IsNumeric(moSession["PreviewUser"]))
                            {
                                if (myWeb.moRequest["ewCmd"] == "Normal" | myWeb.moRequest["ewCmd"] == "ExitPreview" | myWeb.moRequest["ewCmd"] == "EditContent" | myWeb.moRequest["ewCmd"] == "PublishContent")
                                {
                                    // jump out of admin mode...
                                    myWeb.moSession["PreviewUser"] = (object)null;
                                    myWeb.mbPreview = false;
                                }
                                else
                                {
                                    mnUserId = Conversions.ToInteger(moSession["PreviewUser"]);
                                    myWeb.mbPreview = true;
                                }
                            }
                            else
                            {
                                mnUserId = Conversions.ToInteger(moSession["nUserId"]);
                            }


                        }
                        myWeb.PerfMon.Log(mcModuleName, "getUserId-end");
                        return mnUserId.ToString();
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserId", ex, sProcessInfo));
                        return null;
                    }
                }

                public virtual void SetUserId(ref Cms myWeb)
                {
                    myWeb.PerfMon.Log("Web", "getUserId");
                    string sProcessInfo = "";
                    string sReturnValue = string.Empty;
                    string cLogonCmd = string.Empty;
                    int mnUserId = myWeb.mnUserId;
                    System.Web.SessionState.HttpSessionState moSession = myWeb.moSession;

                    try
                    {
                        if (moSession["nUserId"] != null)
                        {
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(moSession["nUserId"], 0, false)))
                            {
                                moSession["nUserId"] = mnUserId;
                            }
                            else if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectNotEqual(moSession["nUserId"], mnUserId, false), string.IsNullOrEmpty(Conversions.ToString(moSession["PreviewUser"])))))
                            {
                                // reset to a different value
                                moSession["nUserId"] = mnUserId;
                                // have a user id so dont remove it
                            }
                        }
                        else
                        {
                            moSession["nUserId"] = mnUserId;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SetUserId", ex, sProcessInfo));
                    }
                }

                public virtual XmlElement GetUserXML(ref Cms myWeb, long nUserId = 0L)
                {
                    myWeb.PerfMon.Log("Web", "GetUserXML");
                    string sProcessInfo = "";
                    int mnUserId = myWeb.mnUserId;
                    Protean.Cms.dbHelper moDbHelper = myWeb.moDbHelper;
                    try
                    {
                        if (nUserId == 0L)
                            nUserId = mnUserId;

                        return moDbHelper.GetUserXML(nUserId);
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserXml", ex, sProcessInfo));
                        return null;
                    }

                }

                public virtual string MembershipProcess(ref Cms myWeb)
                {
                    myWeb.PerfMon.Log("Web", "MembershipProcess");
                    string sProcessInfo = "";
                    string sReturnValue = null;
                    string cLogonCmd = "";
                    int mnUserId = myWeb.mnUserId;
                    System.Web.SessionState.HttpSessionState moSession = myWeb.moSession;
                    System.Web.HttpRequest moRequest = myWeb.moRequest;
                    System.Collections.Specialized.NameValueCollection moConfig = myWeb.moConfig;
                    Protean.Cms.dbHelper moDbHelper = myWeb.moDbHelper;

                    try
                    {

                        IMembershipAdminXforms adXfm = myWeb.moMemProv.AdminXforms;

                        adXfm.open(myWeb.moPageXml);

                        // logoff handler
                        if (Convert.ToString(myWeb.moRequest["ewCmd"]) == "logoff" & mnUserId != 0)
                        {

                            if (Convert.ToString(myWeb.moSession["ewCmd"]) != "PreviewOn")
                            {
                                LogOffProcess(ref myWeb);
                            }

                            // we are logging off so lets redirect
                            if (myWeb.moConfig["BaseUrl"] != null & !(myWeb.moConfig["BaseUrl"] == ""))
                            {
                                myWeb.msRedirectOnEnd = myWeb.moConfig["BaseUrl"];
                            }
                            else if (myWeb.moConfig["RootPageId"] != null & !(myWeb.moConfig["RootPageId"] == ""))
                            {
                                myWeb.msRedirectOnEnd = myWeb.moConfig["ProjectPath"] + "/";
                            }
                            else
                            {
                                myWeb.msRedirectOnEnd = myWeb.mcOriginalURL;
                            }
                            var oMembership = new Cms.Membership(ref myWeb);
                            oMembership.ProviderActions(ref myWeb, "logoffAction");
                            // BaseUrl
                            sReturnValue = "LogOff";
                        }
                        else if (myWeb.moRequest["ewCmd"] == "CancelSubscription")
                        {

                            var oAdx = new Admin.AdminXforms(ref myWeb);
                            oAdx.moPageXML = myWeb.moPageXml;

                            XmlElement oCSFrm = oAdx.xFrmConfirmCancelSubscription(myWeb.moRequest["nUserId"], myWeb.moRequest["nSubscriptionId"], mnUserId, myWeb.mbAdminMode);
                            if (oCSFrm is null)
                            {
                                myWeb.moPageXml.SelectSingleNode("/Page/@layout").InnerText = "My_Account";
                            }
                            else
                            {
                                myWeb.moPageXml.SelectSingleNode("/Page/@layout").InnerText = "CancelSubscription";
                                myWeb.AddContentXml(ref oCSFrm);
                            }
                        }
                        else if (myWeb.moRequest["ewCmd"] == "AR") // AccountReset
                        {
                            string cAccountHash = myWeb.moRequest["AI"];
                            if (!string.IsNullOrEmpty(cAccountHash))
                            {
                                XmlElement oXfmElmt = (XmlElement)adXfm.xFrmConfirmPassword(cAccountHash);
                                myWeb.AddContentXml(ref oXfmElmt);
                                myWeb.moPageXml.DocumentElement.SetAttribute("layout", "Account_Reset");

                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(adXfm.valid, true, false)))
                                {

                                    sReturnValue = "LogOn";

                                    if (gbSingleLoginSessionPerUser)
                                    {
                                        myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Logon, mnUserId, 0);
                                    }
                                    else
                                    {
                                        myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logon, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "");
                                    }

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

                                    if (!string.IsNullOrEmpty(cLogonCmd))
                                    {
                                        myWeb.logonRedirect(cLogonCmd);
                                    }

                                }
                            }

                        }
                        if (!(myWeb.moConfig["SecureMembershipAddress"] == ""))
                        {

                            var oMembership = new Cms.Membership(ref myWeb);
                            oMembership.OnErrorWithWeb += OnComponentError;
                            oMembership.SecureMembershipProcess(sReturnValue);

                        }

                        // display logon form for all pages if user is not logged on.
                        if (mnUserId == 0 & (myWeb.moRequest["ewCmd"] != "passwordReminder" & myWeb.moRequest["ewCmd"] != "ResendActivation" & myWeb.moRequest["ewCmd"] != "ActivateAccount" & myWeb.moRequest["ewCmd"] != "AR"))
                        {

                            XmlElement oXfmElmt = (XmlElement)adXfm.xFrmUserLogon();
                            bool bAdditionalChecks = false;

                            if (Conversions.ToBoolean(!adXfm.valid))
                            {
                                // Call in additional authentication checks
                                if ((myWeb.moConfig["AlternativeAuthentication"]) == "on")
                                {
                                    bAdditionalChecks = AlternativeAuthentication(ref myWeb);
                                }

                            }

                            if (Conversions.ToBoolean(Operators.OrObject(adXfm.valid, bAdditionalChecks)))
                            {
                                myWeb.moContentDetail = null;
                                mnUserId = myWeb.mnUserId;
                                if (myWeb.moSession != null)
                                    myWeb.moSession["nUserId"] = mnUserId;
                                if (myWeb.moRequest["cRemember"] == "true")
                                {
                                    var oCookie = new System.Web.HttpCookie("RememberMe");
                                    oCookie.Value = mnUserId.ToString();
                                    oCookie.Expires = DateAndTime.DateAdd(DateInterval.Day, 60d, DateTime.Now);
                                    myWeb.moResponse.Cookies.Add(oCookie);
                                }
                                sReturnValue = "LogOn";

                                if (gbSingleLoginSessionPerUser)
                                {
                                    myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Logon, mnUserId, 0);
                                }
                                else
                                {
                                    myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logon, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "");
                                }

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

                                // LogonProviderOptions

                                // ProviderActions(myWeb, "logonAction")

                                // do not cache
                                myWeb.bPageCache = false;
                                myWeb.logonRedirect(cLogonCmd);
                            }
                            else
                            {
                                myWeb.AddContentXml(ref oXfmElmt);
                                // mnUserId = adXfm.mnUserId
                            }
                        }
                        else if (moRequest["ewCmd"] == "passwordReminder")
                        {

                            XmlElement oXfmElmt;
                            switch (Strings.LCase(moConfig["MembershipEncryption"]) ?? "")
                            {
                                case "md5salt":
                                case "md5":
                                case "sha1":
                                case "sha256":
                                case "SHA2_512_SALT":
                                    {
                                        oXfmElmt = (XmlElement)adXfm.xFrmResetAccount();
                                        break;
                                    }

                                default:
                                    {
                                        oXfmElmt = (XmlElement)adXfm.xFrmPasswordReminder();
                                        break;
                                    }
                            }
                            myWeb.AddContentXml(ref oXfmElmt);
                        }
                        else if (moRequest["ewCmd"] == "ResendActivation")

                        {
                            if (mnUserId == 0) {
                                mnUserId = Convert.ToInt16(myWeb.moRequest["userId"]);
                            }
                            sendRegistrationAlert(ref myWeb, mnUserId, false);
                        }
                        else if (moRequest["ewCmd"] == "ActivateAccount")
                        {
                            //moved to register module
                            //var oMembership = new Cms.Membership(ref myWeb);
                            //oMembership.OnErrorWithWeb += OnComponentError;
                            //XmlElement oXfmElmt;
                            //oXfmElmt = (XmlElement)adXfm.xFrmActivateAccount();
                            //myWeb.AddContentXml(ref oXfmElmt);
                        }

                        string sharedKey = moConfig["SharedKey"];
                        if (!string.IsNullOrEmpty(sharedKey))
                        {
                            string loginKey = moRequest["userkey"];
                            if (!string.IsNullOrEmpty(loginKey))
                            {

                                // decrpyts using shared key using 

                                // UserEmail / Timestamp / Sharekey / User-Preview-admin / redirect path

                                // timestamp with 60 mins.

                                // only if the sharedkey is in config.
                                string ewCmd = string.Empty;
                                if (moRequest["ewCmd"] != null)
                                {
                                    ewCmd = moRequest["ewCmd"];
                                }

                                var duration = default(long);
                                int AuthenticationDuration = 60;
                                if (!string.IsNullOrEmpty(moConfig["AuthenticationDuration"]))
                                {
                                    AuthenticationDuration = Convert.ToInt32(moConfig["AuthenticationDuration"]);
                                }
                                string decryptedString;
                                string timestamp;
                                string userEmail;
                                string userMode = "user"; // other values preview and admin
                                string[] userDetails;

                                if (sharedKey != null & loginKey != null)
                                {
                                    decryptedString = Encryption.RC4.Decrypt(loginKey, sharedKey);
                                    userDetails = decryptedString.Split('-');
                                    userEmail = userDetails[0].ToString();
                                    timestamp = userDetails[1].ToString();
                                    if (userDetails.Length == 3)
                                    {
                                        userMode = userDetails[2].ToString();
                                    }

                                    // myWeb.moResponse.Write(DateTime.Parse(timestamp.ToString("dd/MM/yyyy HH:MM")))
                                    // myWeb.moResponse.Write(DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy HH:MM")))
                                    try
                                    {
                                        duration = DateAndTime.DateDiff(DateInterval.Minute, DateTime.Parse(timestamp), DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy HH:MM")));
                                    }
                                    // myWeb.moResponse.Write(duration)
                                    // duration = DateDiff(DateInterval.Minute, DateTime.Parse(timestamp), DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy HH:MM")))
                                    catch (Exception)
                                    {
                                        // myWeb.moResponse.Write(ex.Message)
                                    }

                                    if (duration < AuthenticationDuration) // ' greater than 60
                                    {

                                        mnUserId = myWeb.moDbHelper.GetUserIDFromEmail(userEmail);
                                        myWeb.mnUserId = mnUserId;
                                        switch (userMode ?? "")
                                        {
                                            case "preview":
                                                {
                                                    myWeb.moSession["nUserId"] = myWeb.mnUserId;
                                                    myWeb.moSession["adminMode"] = "true";
                                                    myWeb.mbAdminMode = true;
                                                    if (string.IsNullOrEmpty(ewCmd))
                                                    {
                                                        myWeb.moSession["ewCmd"] = "PreviewOn";
                                                        myWeb.moSession["PreviewDate"] = DateTime.Now.Date;
                                                        myWeb.moSession["PreviewUser"] = 0;
                                                        myWeb.mbPreview = true;
                                                        if (string.IsNullOrEmpty(myWeb.mcPagePath) == false)
                                                        {
                                                            myWeb.msRedirectOnEnd = myWeb.mcPagePath;
                                                        }
                                                        else
                                                        {
                                                            myWeb.msRedirectOnEnd = "/";
                                                        }
                                                    }

                                                    else
                                                    {
                                                        myWeb.moSession["ewCmd"] = (object)null;
                                                        myWeb.moSession["PreviewDate"] = (object)null;
                                                        myWeb.moSession["PreviewUser"] = (object)null;
                                                        myWeb.msRedirectOnEnd = "/?ewCmd=" + ewCmd;
                                                    }

                                                    break;
                                                }
                                            case "admin":
                                                {
                                                    myWeb.moSession["nUserId"] = myWeb.mnUserId;
                                                    myWeb.moSession["adminMode"] = "true";
                                                    myWeb.mbAdminMode = true;
                                                    if (string.IsNullOrEmpty(ewCmd))
                                                    {
                                                        myWeb.moSession["PreviewDate"] = (object)null;
                                                        myWeb.moSession["PreviewUser"] = (object)null;
                                                        myWeb.msRedirectOnEnd = "/admin";
                                                    }
                                                    else
                                                    {
                                                        string param = myWeb.mcOriginalURL.Substring(myWeb.mcOriginalURL.IndexOf("ewCmd=")).Replace("ewCmd=", "");

                                                        myWeb.msRedirectOnEnd = "/?ewCmd=" + param;
                                                    }

                                                    break;
                                                }
                                            case "user":
                                                {
                                                    myWeb.moSession["nUserId"] = myWeb.mnUserId;
                                                    break;
                                                }
                                        }
                                    }
                                }
                            }
                        }

                        switch (moRequest["ewCmd"] ?? "")
                        {

                            case "UserIntegrations":
                                {
                                    XmlElement xmloContentElement = adXfm.xFrmUserIntegrations(mnUserId, moRequest["ewCmd2"]);
                                    myWeb.AddContentXml(ref xmloContentElement);
                                    // moContentDetail.AppendChild(adXfm.xFrmUserIntegrations(mnUserId, moRequest("ewCmd2")))
                                    if (Conversions.ToBoolean(adXfm.valid))
                                    {
                                        // moContentDetail.RemoveAll()
                                        // clear the listDirectory cache
                                        myWeb.moDbHelper.clearDirectoryCache();
                                        // return to process flow
                                    }

                                    break;
                                }
                        }



                        // add the user logon details to the page xml.
                        if (mnUserId != 0)
                        {
                            myWeb.RefreshUserXML();
                            myWeb.GetUserXML(mnUserId);
                            // moPageXml.DocumentElement.AppendChild(moPageXml.ImportNode(GetUserXML().CloneNode(True), True))
                        }

                        // Site Redirection Process
                        if (!string.IsNullOrEmpty(moConfig["SiteGroupRedirection"]) & mnUserId != 0)
                        {
                            myWeb.SiteRedirection();
                        }

                        if (!myWeb.bs5 && !myWeb.bs3)
                        {
                            // behaviour based on layout page (not required in V5 sites, this behaviour uses modules instead)
                            MembershipV4LayoutProcess(ref myWeb, adXfm);
                        }

                        LogSingleUserSession(ref myWeb);

                        if (!string.IsNullOrEmpty(moRequest["ewEdit"]))
                        {
                            UserEditProcess(ref myWeb);
                        }

                        return sReturnValue;
                    }

                    catch (Exception ex)
                    {
                        // returnException(myWeb.msException, mcModuleName, "MembershipLogon", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "MembershipProcess", ex, sProcessInfo));
                        return null;
                    }
                }

                public virtual string MembershipV4LayoutProcess(ref Cms myWeb, IMembershipAdminXforms adXfm)
                {
                    myWeb.PerfMon.Log("Web", "MembershipProcess");
                    string sProcessInfo = "";
                    string sReturnValue = null;
                    string cLogonCmd = "";
                    int mnUserId = myWeb.mnUserId;
                    System.Web.SessionState.HttpSessionState moSession = myWeb.moSession;
                    System.Web.HttpRequest moRequest = myWeb.moRequest;
                    System.Collections.Specialized.NameValueCollection moConfig = myWeb.moConfig;
                    Protean.Cms.dbHelper moDbHelper = myWeb.moDbHelper;
                    bool clearUserId = false;

                    try
                    {

                        // behaviour based on layout page
                        switch (myWeb.moPageXml.SelectSingleNode("/Page/@layout").Value)
                        {
                            case "Logon_Register":
                            case "My_Account":
                            case "Register":
                                {
                                    XmlElement oXfmElmt;
                                    // If not in admin mode then base our choice on whether the user is logged in. 
                                    // If in Admin Mode, then present it as WYSIWYG
                                    if (!myWeb.mbAdminMode & mnUserId > 0 | myWeb.mbAdminMode & myWeb.moPageXml.SelectSingleNode("/Page/@layout").Value == "My_Account")
                                    {
                                        XmlElement oContentForm = (XmlElement)myWeb.moPageXml.SelectSingleNode("descendant-or-self::Content[@type='xform' and @name='UserMyAccount']");
                                        if (oContentForm is null)
                                        {
                                            XmlElement instanceAppend = null;
                                            oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(ref instanceAppend, mnUserId, "User", default, "UserMyAccount");
                                        }
                                        else
                                        {
                                            XmlElement instanceAppend = null;
                                            oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(ref instanceAppend, mnUserId, "User", default, "UserMyAccount", oContentForm.OuterXml);
                                            if (!myWeb.mbAdminMode)
                                                oContentForm.ParentNode.RemoveChild(oContentForm);
                                        }
                                        if (Conversions.ToBoolean(adXfm.valid))
                                        {
                                            if (string.IsNullOrEmpty(sReturnValue))
                                                sReturnValue = "updateUser";
                                        }


                                        myWeb.AddContentXml(ref oXfmElmt);
                                    }
                                    else
                                    {

                                        XmlElement oContentForm = (XmlElement)myWeb.moPageXml.SelectSingleNode("descendant-or-self::Content[@type='xform' and @name='UserRegister']");
                                        if (oContentForm is null)
                                        {
                                            XmlElement instanceAppend = null;
                                            oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(ref instanceAppend, mnUserId, "User", default, "UserRegister");
                                        }
                                        else
                                        {
                                            XmlElement instanceAppend = null;
                                            oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(ref instanceAppend, mnUserId, "User", default, "UserRegister", oContentForm.OuterXml);
                                            if (!myWeb.mbAdminMode)
                                                oContentForm.ParentNode.RemoveChild(oContentForm);
                                        }

                                        // ok if the user is valid we then need to handle what happens next.
                                        if (Conversions.ToBoolean(adXfm.valid))
                                        {
                                            bool bRedirect = true;
                                            switch (moConfig["RegisterBehaviour"] ?? "")
                                            {

                                                case "validateByEmail":
                                                    {
                                                        // don't redirect because we want to reuse this form
                                                        bRedirect = false;
                                                        // say thanks for registering and update the form
                                                        adXfm.addNote("EditContent", Protean.xForm.noteTypes.Hint, "Thanks for registering you have been sent an email with a link you must click to activate your account", true);

                                                        // lets get the new userid from the instance
                                                        mnUserId = Conversions.ToInteger(adXfm.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText);

                                                        // first we set the user account to be pending
                                                        myWeb.moDbHelper.setObjectStatus(dbHelper.objectTypes.Directory, dbHelper.Status.Pending, mnUserId);

                                                        var oMembership = new Cms.Membership(ref myWeb);
                                                        oMembership.OnErrorWithWeb += OnComponentError;
                                                        oMembership.AccountActivateLink(mnUserId);
                                                        clearUserId = true;
                                                        myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Register, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "Send Activation"); // Auto logon
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        mnUserId = Conversions.ToInteger(adXfm.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText);
                                                        if (moSession != null)
                                                        {
                                                            myWeb.mnUserId = mnUserId;
                                                            moSession["nUserId"] = mnUserId;
                                                        }

                                                        myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Register, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "First Logon");

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
                                                        myWeb.logonRedirect(cLogonCmd);
                                                        bRedirect = false;
                                                        break;
                                                    }

                                            }

                                            sendRegistrationAlert(ref myWeb, mnUserId, clearUserId);

                                            // redirect to this page or alternative page.
                                            if (bRedirect)
                                            {
                                                myWeb.msRedirectOnEnd = myWeb.mcOriginalURL;
                                            }

                                            sReturnValue = "newUser";
                                        }
                                        else
                                        {
                                            myWeb.AddContentXml(ref oXfmElmt);
                                        }
                                    }

                                    break;
                                }

                            case "Password_Reminder":
                                {

                                    XmlElement oXfmElmt;
                                    if (string.IsNullOrEmpty(moConfig["MembershipEncryption"]))
                                    {

                                        oXfmElmt = (XmlElement)adXfm.xFrmPasswordReminder();
                                    }

                                    else
                                    {
                                        ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                        IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);
                                        var oAdXfm = oMembershipProv.AdminXforms;
                                        oXfmElmt = (XmlElement)oAdXfm.xFrmResetAccount();

                                    }
                                    myWeb.AddContentXml(ref oXfmElmt);
                                    break;
                                }

                            case "Password_Change":
                                {

                                    ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                    IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, myWeb.moConfig["MembershipProvider"]);

                                    var oAdXfm = oMembershipProv.AdminXforms;

                                    XmlElement oXfmElmt;
                                    oXfmElmt = (XmlElement)oAdXfm.xFrmResetPassword(mnUserId);
                                    myWeb.AddContentXml(ref oXfmElmt);
                                    break;
                                }

                            case "Activation_Code":
                                {

                                    if (!myWeb.mbAdminMode)
                                    {
                                        XmlElement oXfmElmt = null;
                                        string cExistingFormXml = "";
                                        // For Each ocNode In moPageXml.SelectNodes("/Page/Contents/Content[@type='xform' and model/submission/@SOAPAction!='']")

                                        // Look for activation code xforms
                                        XmlElement xmlPageElmt = (XmlElement)myWeb.moPageXml.DocumentElement;
                                        if (Xml.NodeState(ref xmlPageElmt, "Contents/Content[@type='xform' and @name='ActivationCode']", "", "", XmlNodeState.IsEmpty, oXfmElmt, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                        {
                                            oXfmElmt.ParentNode.RemoveChild(oXfmElmt);
                                            cExistingFormXml = oXfmElmt.OuterXml;
                                        }

                                        oXfmElmt = (XmlElement)adXfm.xFrmActivationCode(mnUserId, default, cExistingFormXml);
                                        if (oXfmElmt != null)
                                        {
                                            myWeb.AddContentXml(ref oXfmElmt);

                                            // If the form has been successful, then check for a redirect
                                            // If the redirectpage is a number then continue
                                            if (oXfmElmt.SelectSingleNode("//instance/formState[node()='success']") != null)
                                            {

                                                // Activation was succesful, let's prepare the redirect

                                                // Clear the cache.
                                                string cSql = Conversions.ToString(Operators.ConcatenateObject("DELETE dbo.tblXmlCache " + " WHERE cCacheSessionID = '" + moSession.SessionID + "' " + "         AND nCacheDirId = ", SqlFmt(mnUserId.ToString())));

                                                myWeb.moDbHelper.ExeProcessSqlorIgnore(cSql);

                                                // Check if the redirect is another page or just redirect to the current url
                                                if (oXfmElmt.SelectSingleNode("//instance/RedirectPage[number(.)=number(.)]") != null)
                                                {
                                                    myWeb.msRedirectOnEnd = moConfig["ProjectPath"] + "/?pgid=" + adXfm.Instance.SelectSingleNode("RedirectPage").InnerText;
                                                }
                                                else
                                                {
                                                    myWeb.msRedirectOnEnd = myWeb.mcOriginalURL;
                                                }
                                                myWeb.bRedirectStarted = true;
                                                moSession["RedirectReason"] = "activation";
                                            }
                                        }
                                    }

                                    break;
                                }



                            case "User_Contact":
                                {
                                    if (mnUserId > 0)
                                    {
                                        switch (moRequest["ewCmd"] ?? "")
                                        {
                                            case "addContact":
                                            case "editContact":
                                                {
                                                    XmlElement oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryContact(Convert.ToInt64(moRequest["id"]), mnUserId);
                                                    if (Conversions.ToBoolean(!adXfm.valid))
                                                    {
                                                        myWeb.AddContentXml(ref oXfmElmt);
                                                    }
                                                    else
                                                    {
                                                        myWeb.RefreshUserXML();
                                                    }

                                                    break;
                                                }
                                            case "delContact":
                                                {
                                                    foreach (XmlElement oContactElmt in myWeb.GetUserXML(mnUserId).SelectNodes("descendant-or-self::Contact"))
                                                    {
                                                        XmlElement oId = (XmlElement)oContactElmt.SelectSingleNode("nContactKey");
                                                        if (oId != null)
                                                        {
                                                            if ((oId.InnerText ?? "") == (moRequest["id"] ?? ""))
                                                            {
                                                                moDbHelper.DeleteObject(dbHelper.objectTypes.CartContact, Convert.ToInt64(moRequest["id"]));
                                                                myWeb.RefreshUserXML();
                                                            }
                                                        }
                                                    }

                                                    break;
                                                }
                                        }
                                    }

                                    break;
                                }

                        }
                    }
                    catch (Exception ex)
                    {
                        // returnException(myWeb.msException, mcModuleName, "MembershipLogon", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "MembershipV4LayoutProcess", ex, sProcessInfo));
                        return null;
                    }

                    return default;
                }

                /// <summary>
                /// If in Single Session per Session mode this will log an activity indicating that the user is still in session.
                /// </summary>
                /// <remarks>It is called in EonicWeb but has been extracted so that it may be called by lightweight EonicWeb calls (e.g. ajax calls)</remarks>
                /// 

                public void sendRegistrationAlert(ref Cms myWeb,long mnUserId, Boolean clearUserId, string cmdPrefix = "") {
                    System.Web.HttpRequest moRequest = myWeb.moRequest;
                    System.Collections.Specialized.NameValueCollection moConfig = myWeb.moConfig;

                    string sProcessInfo = "";
                    
                    try { 

                    // send registration confirmation
                        string xsltPath = "/xsl/email/registration.xsl";
                        if (myWeb.bs5) {
                            xsltPath = "/features/membership/email/registration.xsl";
                        }
                        if (File.Exists(goServer.MapPath(xsltPath)))
                        {
                            XmlElement oUserElmt = myWeb.moDbHelper.GetUserXML(mnUserId);
                            if (clearUserId)
                                mnUserId = 0; // clear user Id so we don't stay logged on
                            XmlElement oElmtPwd = myWeb.moPageXml.CreateElement("Password");
                            oElmtPwd.InnerText = moRequest["cDirPassword"];
                            oUserElmt.AppendChild(oElmtPwd);

                            XmlElement oUserEmail = (XmlElement)oUserElmt.SelectSingleNode("Email");
                            string fromName = moConfig["SiteAdminName"];
                            string fromEmail = moConfig["SiteAdminEmail"];
                            string recipientEmail = "";
                            if (oUserEmail != null)
                                recipientEmail = oUserEmail.InnerText;
                            string SubjectLine = "Your Registration Details";
                            var oMsg = new Protean.Messaging(ref myWeb.msException);

                            oUserElmt.SetAttribute("Url", myWeb.mcOriginalURL);
                            oUserElmt.SetAttribute("activateCmd", cmdPrefix + "ActivateAccount");

                            // send an email to the new registrant
                            if (!string.IsNullOrEmpty(recipientEmail))
                            {
                                Protean.Cms.dbHelper argodbHelper = null;
                                sProcessInfo = Conversions.ToString(oMsg.emailer(oUserElmt, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, odbHelper: ref argodbHelper, "Message Sent", "Message Failed"));
                            }
                            // send an email to the webadmin                            

                            string registrationAlertPath = "/xsl/email/registrationAlert.xsl";
                            if (myWeb.bs5)
                            {
                                registrationAlertPath = "/features/membership/email/registration-alert.xsl";
                            }
                            if (File.Exists(goServer.MapPath(moConfig["ProjectPath"] + registrationAlertPath)))
                            {
                                recipientEmail = moConfig["SiteAdminEmail"];
                                Protean.Cms.dbHelper nulldbhelper = null;
                                sProcessInfo = Conversions.ToString(oMsg.emailer(oUserElmt, moConfig["ProjectPath"] + registrationAlertPath, "New User", recipientEmail, fromEmail, SubjectLine, odbHelper: ref nulldbhelper, "Message Sent", "Message Failed"));
                            }
                            oMsg = (Protean.Messaging)null;
                        }
                    }
                    catch (Exception ex)
                    {
                        // returnException(myWeb.msException, mcModuleName, "MembershipLogon", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "MembershipV4LayoutProcess", ex, sProcessInfo));
                  
                    }
                 }


                public void LogSingleUserSession()
                {
                    // Do Nothing
                }



                public void LogSingleUserSession(ref Cms myWeb)
                {

                    int mnUserId = myWeb.mnUserId;
                    System.Web.SessionState.HttpSessionState moSession = myWeb.moSession;
                    System.Web.HttpRequest moRequest = myWeb.moRequest;
                    System.Collections.Specialized.NameValueCollection moConfig = myWeb.moConfig;
                    Protean.Cms.dbHelper moDbHelper = myWeb.moDbHelper;
                    System.Web.HttpResponse moResponse = myWeb.moResponse;

                    try
                    {
                        // If logged on and in single login per user mode, log a session continuation flag.
                        if (gbSingleLoginSessionPerUser & mnUserId > 0 & moSession != null)
                        {
                            // Log the session
                            moDbHelper.logActivity(dbHelper.ActivityType.SessionContinuation, mnUserId, 0, 0, 0, "", true);

                            // Add a cookie / update it.
                            var cookieLock = moRequest.Cookies["ewslock"];
                            if (cookieLock is null)
                            {
                                cookieLock = new System.Web.HttpCookie("ewslock");
                            }
                            cookieLock.Expires = DateTime.Now.AddSeconds(gnSingleLoginSessionTimeout);
                            cookieLock.Value = moSession.SessionID;
                            moResponse.Cookies.Add(cookieLock);
                        }
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LogSingleUserSession", ex, ""));
                    }
                }

                public virtual bool AlternativeAuthentication(ref Cms myWeb)
                {

                    myWeb.PerfMon.Log("Web", "AlternativeAuthentication");


                    string cProcessInfo = "";
                    bool bCheck = false;
                    string cToken = "";
                    string cKey = "";
                    string cDecrypted = "";
                    int nReturnId;

                    int mnUserId = myWeb.mnUserId;
                    System.Web.SessionState.HttpSessionState moSession = myWeb.moSession;
                    System.Web.HttpRequest moRequest = myWeb.moRequest;
                    System.Collections.Specialized.NameValueCollection moConfig = myWeb.moConfig;
                    Protean.Cms.dbHelper moDbHelper = myWeb.moDbHelper;


                    try
                    {

                        // Look for the RC4 token
                        if (!string.IsNullOrEmpty(moRequest["token"]) & !string.IsNullOrEmpty(moConfig["AlternativeAuthenticationKey"]))
                        {

                            cProcessInfo = "IP Address Checking";

                            string cIPList = moConfig["AlternativeAuthenticationIPList"];

                            if (string.IsNullOrEmpty(cIPList) || Text.IsIPAddressInList(moRequest.UserHostAddress, cIPList))
                            {

                                cProcessInfo = "Decrypting token";
                                // Dim oEnc As New Protean.Tools.Encryption.RC4()

                                cToken = moRequest["token"];
                                cKey = moConfig["AlternativeAuthenticationKey"];

                                // There are two accepted formats to receive:
                                // 1. Email address
                                // 2. User ID

                                cDecrypted = Strings.Trim(Encryption.RC4.Decrypt(cToken, cKey));

                                if (Text.IsEmail(cDecrypted))
                                {

                                    // Authentication is by way of email address
                                    cProcessInfo = "Email authenctication: Retrieving user for email: " + cDecrypted;
                                    // Get the user id based on the email address
                                    nReturnId = moDbHelper.GetUserIDFromEmail(cDecrypted);

                                    if (nReturnId > 0)
                                    {
                                        bCheck = true;
                                        mnUserId = nReturnId;
                                        myWeb.mnUserId = mnUserId;
                                        if (moRequest["ewCmd"] == "PreviewOn")
                                        {
                                            moSession["adminMode"] = "true";
                                            moSession["ewCmd"] = "PreviewOn";
                                            myWeb.mbAdminMode = true;
                                            myWeb.mbPreview = true;
                                            myWeb.moSession["PreviewUser"] = "0";
                                        }

                                    }
                                }

                                else if (Information.IsNumeric(cDecrypted) && Conversions.ToInteger(cDecrypted) > 0)
                                {

                                    // Authentication is by way of user ID
                                    cProcessInfo = "User ID Authentication: " + cDecrypted;
                                    // Get the user id based on the email address
                                    bCheck = moDbHelper.IsValidUser(Conversions.ToInteger(cDecrypted));
                                    if (bCheck)
                                        mnUserId = Conversions.ToInteger(cDecrypted);

                                }

                            }

                        }

                        return bCheck;
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AlternativeAuthentication", ex, cProcessInfo));
                        return false;
                    }

                }

                public virtual void LogOffProcess(ref Cms myWeb)
                {
                    myWeb.PerfMon.Log("Web", "LogOffProcess");
                    string cProcessInfo = "";

                    int mnUserId = myWeb.mnUserId;
                    System.Web.SessionState.HttpSessionState moSession = myWeb.moSession;
                    System.Web.HttpRequest moRequest = myWeb.moRequest;
                    System.Collections.Specialized.NameValueCollection moConfig = myWeb.moConfig;
                    Protean.Cms.dbHelper moDbHelper = myWeb.moDbHelper;
                    System.Web.HttpResponse moResponse = myWeb.moResponse;

                    try
                    {

                        cProcessInfo = "Commit to Log";
                        if (gbSingleLoginSessionPerUser)
                        {
                            moDbHelper.logActivity(dbHelper.ActivityType.Logoff, mnUserId, 0);
                            if (moRequest.Cookies["ewslock"] != null)
                            {
                                moResponse.Cookies["ewslock"].Expires = DateTime.Now.AddDays(-1);
                            }
                        }
                        else
                        {
                            moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logoff, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "");
                        }


                        // Call this BEFORE clearing the user ID.
                        cProcessInfo = "Clear Site Stucture";
                        moDbHelper.clearStructureCacheUser();

                        // Clear the user ID.
                        mnUserId = 0;
                        myWeb.mnUserId = 0;

                        // Clear the cart
                        if (moSession != null && gbCart)
                        {
                            string cSql = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where (cCartSessionId = '" + moSession.SessionID + "' and cCartSessionId <> '')";
                            moDbHelper.ExeProcessSql(cSql);
                        }

                        if (moSession != null)
                        {
                            cProcessInfo = "Abandon Session";
                            // AJG : Question - why does this not clear the Session ID?
                            moSession["nEwUserId"] = null;
                            moSession["nUserId"] = null;
                            moSession.Abandon();
                        }

                        if (moConfig["RememberMeMode"] != "KeepCookieAfterLogoff")
                        {
                            cProcessInfo = "Clear Cookies";
                            moResponse.Cookies["RememberMeUserName"].Expires = DateTime.Now.AddDays(-1);
                            moResponse.Cookies["RememberMeUserId"].Expires = DateTime.Now.AddDays(-1);
                        }
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LogOffProcess", ex, cProcessInfo));
                    }

                }

                public virtual string UserEditProcess(ref Cms myWeb)
                {
                    myWeb.PerfMon.Log("Web", "UserEditProcess");
                    string sProcessInfo = "";
                    string sReturnValue = string.Empty;
                    try
                    {


                        return null;
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UserEditProcess", ex, sProcessInfo));
                        return null;
                    }
                }

                public virtual string ResetUserAcct(ref Cms myWeb, int nUserId)
                {
                    myWeb.PerfMon.Log("Web", "ResetUserAcct");
                    string sProcessInfo = "";
                    string sReturnValue = null;
                    try
                    {

                        // Get the user XML
                        XmlElement oUserXml = myWeb.moDbHelper.GetUserXML(nUserId, false);

                        if (oUserXml is null)
                        {
                            sReturnValue = "<span class=\"msg-1028\">There was a problem resetting this account. Please contact the website administrator</span>";
                        }
                        else
                        {
                            // Check the xsl
                            string userEmail;
                            // If areEmailAddressesAllowed = True And Not (isValidEmailAddress) Then
                            // userEmail = oUserDetails("cDirEmail")
                            // Else
                            userEmail = oUserXml.SelectSingleNode("Email").InnerText;
                            // End If

                            var oMembership = new Cms.Membership(ref myWeb);
                            var oEmailDoc = new XmlDocument();
                            oEmailDoc.AppendChild(oEmailDoc.CreateElement("AccountReset"));
                            oEmailDoc.DocumentElement.AppendChild(oEmailDoc.ImportNode(oUserXml, true));
                            oEmailDoc.DocumentElement.SetAttribute("Link", oMembership.AccountResetLink(nUserId));
                            oEmailDoc.DocumentElement.SetAttribute("Url", myWeb.mcOriginalURL);
                            //  oEmailDoc.DocumentElement.SetAttribute("logonRedirect", myWeb.moSession["LogonRedirectId"].ToString());
                            oEmailDoc.DocumentElement.SetAttribute("lang", myWeb.mcPageLanguage);
                            oEmailDoc.DocumentElement.SetAttribute("translang", myWeb.mcPreferredLanguage);

                            var oMessage = new Protean.Messaging(ref myWeb.msException);

                            var fs = new Protean.fsHelper();
                            string path = "";
                            if (myWeb.moConfig["cssFramework"] == "bs5")
                            {
                                path = fs.FindFilePathInCommonFolders("/features/membership/email/password-reset.xsl", myWeb.maCommonFolders);
                            }
                            else {
                                path = fs.FindFilePathInCommonFolders("/xsl/Email/passwordReset.xsl", myWeb.maCommonFolders);
                             
                            }
                            Protean.Cms.dbHelper argodbHelper = null;
                            sReturnValue = Conversions.ToString(oMessage.emailer(oEmailDoc.DocumentElement, path, myWeb.moConfig["SiteAdminName"], myWeb.moConfig["SiteAdminEmail"], userEmail, "Account Reset ", odbHelper: ref argodbHelper));

                            //sReturnValue = Conversions.ToString(Interaction.IIf(sReturnValue == "Message Sent", "<span class=\"msg-1035\">Reset code sent to </span>" + userEmail, ""));
                            if (sReturnValue == "Message Sent") {
                                 sReturnValue = "If we have the user account supplied we will have emailed you a reset code";
                            }
                        } // endif oUserXml Is Nothing


                        return sReturnValue;
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ResetUserAcct", ex, sProcessInfo));
                        return null;
                    }
                }



            }
        }

    }
}