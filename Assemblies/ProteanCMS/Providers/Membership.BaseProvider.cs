// ***********************************************************************
// $Library:     Protean.Providers.membership.base
// $Revision:    3.1  
// $Date:        2012-07-21
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
// ***********************************************************************


using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Tools;
using static Protean.Tools.Xml;

namespace Protean.Providers
{
    namespace Membership
    {

        public class BaseProvider
        {


            private const string mcModuleName = "Protean.Providers.Membership.BaseProvider";

            private object _AdminXforms;
            private object _AdminProcess;
            private object _Activities;

            protected XmlNode moPaymentCfg;

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            public event OnErrorWithWebEventHandler OnErrorWithWeb;

            public delegate void OnErrorWithWebEventHandler(ref Protean.Cms myweb, object sender, Tools.Errors.ErrorEventArgs e);

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

            public BaseProvider(ref object myWeb, string ProviderName)
            {
                try
                {

                    Type calledType;
                    if (string.IsNullOrEmpty(ProviderName))
                    {
                        ProviderName = "Protean.Providers.Membership.EonicProvider";
                        calledType = Type.GetType(ProviderName, true);
                    }
                    else
                    {
                        var castObject = WebConfigurationManager.GetWebApplicationSection("protean/membershipProviders");
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                        object ourProvider = moPrvConfig.Providers[ProviderName];
                        Assembly assemblyInstance;
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(ourProvider.parameters("path"), "", false)))
                        {
                            assemblyInstance = Assembly.LoadFrom(Protean.stdTools.goServer.MapPath(Conversions.ToString(ourProvider.parameters("path"))));
                        }
                        else
                        {
                            assemblyInstance = Assembly.Load(ourProvider.Type);
                        }
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(ourProvider.parameters("rootClass"), "", false)))
                        {
                            calledType = assemblyInstance.GetType("Protean.Providers.Membership." + ProviderName, true);
                        }
                        else
                        {
                            // calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & ".Providers.Messaging", True)
                            calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(ourProvider.parameters("rootClass"), ".Providers.Membership."), ProviderName)), true);
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
                    string argsException = Conversions.ToString(myWeb.msException);
                    Protean.stdTools.returnException(ref argsException, mcModuleName, "New", ex, "", ProviderName + " Could Not be Loaded", Protean.stdTools.gbDebug);
                    myWeb.msException = argsException;
                }

            }

        }

        public class EonicProvider
        {

            public EonicProvider()
            {
                // do nothing
            }

            public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, ref object MemProvider, ref Protean.Cms myWeb)
            {

                MemProvider.AdminXforms = new AdminXForms(ref myWeb);
                // MemProvider.AdminProcess = New AdminProcess(myWeb)
                // MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                MemProvider.Activities = new Activities();

            }

            public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, ref object MemProvider, ref Protean.Base myWeb)
            {

                // MemProvider.AdminXforms = New AdminXForms(myWeb)
                // MemProvider.AdminProcess = New AdminProcess(myWeb)
                // MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                MemProvider.Activities = new Activities();

            }

            public class AdminXForms : Protean.Cms.Admin.AdminXforms
            {
                private const string mcModuleName = "Providers.Membership.Eonic.AdminXForms";
                public bool maintainMembershipsOnAdd = true;

                public AdminXForms(ref Protean.Cms aWeb) : base(ref aWeb)
                {
                }

                public override XmlElement xFrmUserLogon(string FormName = "UserLogon") // Just replace Overridable to Overrides
                {

                    // Called to get XML for the User Logon.

                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string sValidResponse;
                    string cProcessInfo = "";
                    bool bRememberMe = false;
                    try
                    {
                        base.NewFrm("UserLogon");

                        if (this.mbAdminMode & this.myWeb.mnUserId == 0)
                            goto BuildForm;
                        if (this.myWeb.moConfig["RememberMeMode"] == "KeepCookieAfterLogoff" | this.myWeb.moConfig["RememberMeMode"] == "ClearCookieAfterLogoff")
                            bRememberMe = true;
                        string formPath = "/xforms/directory/" + FormName + ".xml";
                        if (this.myWeb.moConfig["cssFramework"] == "bs5")
                        {
                            formPath = "/features/membership/" + FormName + ".xml";
                        }

                        // maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        if (!base.load(formPath, this.myWeb.maCommonFolders))
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

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "UserDetails", "", "Login to ProteanCMS");

                        var userIpt = base.addInput(ref oFrmElmt, "cUserName", true, "Email");
                        base.addClientSideValidation(ref userIpt, true, "Please enter Email");
                        XmlElement argoBindParent = null;
                        base.addBind("cUserName", "user/username", "true()", oBindParent: ref argoBindParent);
                        string argsClass = "";
                        var pwdIpt = base.addSecret(ref oFrmElmt, "cPassword", true, "Password", sClass: ref argsClass);
                        base.addClientSideValidation(ref pwdIpt, true, "Please enter Password");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cPassword", "user/password", "true()", oBindParent: ref argoBindParent1);

                        base.addSubmit(ref oFrmElmt, "ewSubmit", "Login", sIcon: "fa-solid fa-right-to-bracket");

                        base.Instance.InnerXml = "<user rememberMe=\"\"><username/><password/></user>";
                    Check:
                        ;

                        // Set the action URL

                        // Is the membership email address secure.
                        if (!string.IsNullOrEmpty(this.myWeb.moConfig["SecureMembershipAddress"]))
                        {
                            XmlElement oSubElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant::submission");
                            oSubElmt.SetAttribute("action", this.myWeb.moConfig["SecureMembershipAddress"] + this.myWeb.moConfig["ProjectPath"] + "/" + this.myWeb.mcPagePath);
                        }

                        // Set the remember me value, username from cookie.
                        if (bRememberMe)
                        {

                            // Add elements to the form if not present
                            if (Xml.NodeState(ref base.model, "bind[@id='cRemember']") == XmlNodeState.NotInstantiated)
                            {
                                XmlElement argoContextNode = (XmlElement)base.moXformElmt.SelectSingleNode("group");
                                oSelElmt = base.addSelect(ref argoContextNode, "cRemember", true, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                                base.addOption(ref oSelElmt, "Remember me", "true");
                                XmlElement argoBindParent2 = null;
                                base.addBind("cRemember", "user/@rememberMe", "false()", oBindParent: ref argoBindParent2);
                            }

                            // Retrieve values from the cookie
                            if (this.goRequest.Cookies["RememberMeUserName"] is not null)
                            {
                                string cRememberedUsername = this.goRequest.Cookies["RememberMeUserName"].Value;
                                bool bRemembered = false;
                                XmlElement oElmt = null;

                                if (!string.IsNullOrEmpty(cRememberedUsername))
                                    bRemembered = true;

                                XmlNodeState localNodeState() { var argoNode = base.Instance; var ret = Xml.NodeState(ref argoNode, "user", "", "", 1, oElmt, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false); base.Instance = argoNode; return ret; }

                                if (localNodeState() != XmlNodeState.NotInstantiated & !base.isSubmitted())
                                {

                                    oElmt.SetAttribute("rememberMe", Strings.LCase(Conversions.ToString(bRemembered)));
                                    var argoNode = base.Instance;
                                    Xml.NodeState(ref argoNode, "user/username", cRememberedUsername);
                                    base.Instance = argoNode;

                                }
                            }
                        }

                        base.updateInstanceFromRequest();

                        if (base.isSubmitted())
                        {
                            base.validate();
                            if (base.valid)
                            {

                                // changed to get from instance rather than direct from querysting / form.
                                string username = base.Instance.SelectSingleNode("user/username").InnerText;
                                string password = base.Instance.SelectSingleNode("user/password").InnerText;

                                sValidResponse = this.moDbHelper.validateUser(username, password);

                                if (Information.IsNumeric(sValidResponse))
                                {
                                    this.myWeb.mnUserId = (int)Conversions.ToLong(sValidResponse);
                                    this.moDbHelper.mnUserId = Conversions.ToLong(sValidResponse);
                                    if (this.goSession is not null)
                                    {
                                        this.goSession["nUserId"] = (object)this.myWeb.mnUserId;

                                        var UserXml = this.myWeb.GetUserXML();
                                        if (!string.IsNullOrEmpty(UserXml.GetAttribute("defaultCurrency")))
                                        {
                                            this.goSession["cCurrency"] = UserXml.GetAttribute("defaultCurrency");
                                        }

                                    }

                                    // Set the remember me cookie
                                    if (bRememberMe)
                                    {
                                        if (this.goRequest["cRemember"] == "true")
                                        {
                                            System.Web.HttpCookie oCookie;
                                            if (this.myWeb.moRequest.Cookies["RememberMeUserName"] is not null)
                                                this.goResponse.Cookies.Remove("RememberMeUserName");
                                            oCookie = new System.Web.HttpCookie("RememberMeUserName");
                                            oCookie.Value = this.myWeb.moRequest["cUserName"];
                                            oCookie.Expires = DateAndTime.DateAdd(DateInterval.Day, 60d, DateTime.Now);
                                            this.goResponse.Cookies.Add(oCookie);

                                            if (this.myWeb.moRequest.Cookies["RememberMeUserId"] is not null)
                                                this.goResponse.Cookies.Remove("RememberMeUserId");
                                            oCookie = new System.Web.HttpCookie("RememberMeUserId");
                                            oCookie.Value = this.myWeb.mnUserId.ToString();
                                            oCookie.Expires = DateAndTime.DateAdd(DateInterval.Day, 60d, DateTime.Now);
                                            this.goResponse.Cookies.Add(oCookie);
                                        }
                                        else
                                        {
                                            this.goResponse.Cookies["RememberMeUserName"].Expires = DateTime.Now.AddDays(-1);
                                            this.goResponse.Cookies["RememberMeUserId"].Expires = DateTime.Now.AddDays(-1);
                                        }
                                    }
                                }
                                else
                                {
                                    this.valid = false;
                                    XmlNode argoNode1 = (XmlNode)this.moXformElmt;
                                    base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, sValidResponse);
                                    this.moXformElmt = (XmlElement)argoNode1;
                                }
                            }
                            else
                            {
                                this.valid = false;
                            }
                            if (this.valid == false)
                            {
                                this.myWeb.mnUserId = 0;
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmUserLogon", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public override XmlElement xFrmPasswordReminder()  // Just replace Overridable to Overrides
                {
                    XmlElement oFrmElmt;
                    string sValidResponse;
                    string cProcessInfo = "";
                    bool getRecordByEmail;


                    try
                    {

                        // Does the configuration setting indicate that email addresses are allowed.
                        if (Strings.LCase(this.myWeb.moConfig["EmailUsernames"]) == "on")
                        {
                            getRecordByEmail = true;
                        }
                        else
                        {
                            getRecordByEmail = false;
                        }

                        base.NewFrm("PasswordReminder");

                        base.submission("PasswordReminder", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "PasswordReminder");
                        base.addDiv(ref oFrmElmt, "Please enter your email address and we will email you with your password.");
                        base.addInput(ref oFrmElmt, "cEmail", true, "Email Address");
                        XmlElement argoBindParent = null;
                        base.addBind("cEmail", "user/email", "true()", "email", oBindParent: ref argoBindParent);

                        base.addSubmit(ref oFrmElmt, "", "Send Password", "ewSubmitReminder");

                        if (!base.load("/xforms/passwordreminder.xml", this.myWeb.maCommonFolders))
                        {
                            base.NewFrm("PasswordReminder");

                            base.submission("PasswordReminder", "", "post", "form_check(this)");

                            oFrmElmt = base.addGroup(ref base.moXformElmt, "PasswordReminder");

                            base.addDiv(ref oFrmElmt, "Please enter your email address and we will email you with your password.");

                            base.addInput(ref oFrmElmt, "cEmail", true, "Email Address");
                            XmlElement argoBindParent1 = null;
                            base.addBind("cEmail", "user/email", "true()", "email", oBindParent: ref argoBindParent1);

                            base.addSubmit(ref oFrmElmt, "", "Send Password", "ewSubmitReminder");
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
                                sValidResponse = this.moDbHelper.passwordReminder(this.goRequest["cEmail"]);

                                if (sValidResponse == "Your password has been emailed to you")
                                {
                                    // remove old form
                                    this.valid = true;
                                    foreach (XmlElement oElmt in oFrmElmt.SelectNodes("*"))
                                        oFrmElmt.RemoveChild(oElmt);
                                    XmlNode argoNode = oFrmElmt;
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, sValidResponse, true);
                                    oFrmElmt = (XmlElement)argoNode;
                                }
                                else
                                {
                                    this.valid = false;
                                    XmlNode argoNode1 = oFrmElmt;
                                    base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, sValidResponse, true);
                                    oFrmElmt = (XmlElement)argoNode1;
                                }
                            }
                            else
                            {
                                this.valid = false;
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public override XmlElement xFrmActivateAccount()
                {
                    XmlElement oFrmElmt;

                    // Dim sValidResponse As String
                    string cProcessInfo = "";
                    try
                    {
                        // Find a user account with the right activation code.

                        // Change the account status and delete the activation code.
                        var oMembership = new Protean.Cms.Membership(ref this.myWeb);
                        // AddHandler oMembership.OnError, AddressOf OnComponentError

                        oMembership.ActivateAccount(this.moRequest["key"]);

                        oFrmElmt = xFrmUserLogon();

                        this.addNote("UserDetails", Protean.xForm.noteTypes.Hint, "Your account is now activated please logon", true);

                        // Update the user Xform to say "Thank you for activating your account please logon, Pre-populating the username"


                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmActivateAccount", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmResetAccount(long userId = 0L)
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
                        if (Strings.LCase(this.myWeb.moConfig["EmailUsernames"]) == "on")
                        {
                            areEmailAddressesAllowed = true;
                        }
                        else
                        {
                            areEmailAddressesAllowed = false;
                        }

                        if (userId > 0L)
                        {
                            // populate the user id in the form

                            cSQL = "SELECT cDirName, cDirEmail FROM tblDirectory WHERE cDirSchema = 'User' and nDirKey = '" + userId + "'";
                            dsUsers = this.myWeb.moDbHelper.GetDataSet(cSQL, "tblTemp");

                            oUserDetails = dsUsers.Tables[0].Rows[0];
                            cEmailAddress = Conversions.ToString(oUserDetails["cDirName"]);
                            isValidEmailAddress = EmailAddressCheck(cEmailAddress);
                            formTitle = "<span class=\"msg-1030\">Send account reset message to user.</span>";
                        }
                        else
                        {
                            formTitle = "<span class=\"msg-028\">Please enter your email address and we will email you with your password.</span>";
                        }


                        string FormName = "ResetAccount";

                        // maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        if (!base.load("/xforms/directory/" + FormName + ".xml", this.myWeb.maCommonFolders))
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
                        XmlElement argoBindParent = null;
                        base.addBind("cEmail", "user/email", "true()", "format:^[a-zA-Z0-9._%+-@ ]*$", oBindParent: ref argoBindParent);

                        base.addSubmit(ref oFrmElmt, "", "Send Password", "ewAccountReset");

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

                                cUsername = this.Instance.SelectSingleNode("user/email").InnerText;
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

                                dsUsers = this.myWeb.moDbHelper.GetDataSet(cSQL, "tblTemp");
                                nNumberOfUsers = dsUsers.Tables[0].Rows.Count;

                                if (nNumberOfUsers == 0)
                                {
                                    cResponse = "<span class=\"msg-1026\">There was a problem resetting this account, the username was not found. Please contact the website administrator</span>";
                                }
                                else if (nNumberOfUsers > 1 & areEmailAddressesAllowed == true)
                                {
                                    cResponse = "<span class=\"msg-1027\">There was a problem resetting this account, email address is ambiguous. Please contact the website administrator</span>";
                                }
                                else
                                {
                                    oUserDetails = dsUsers.Tables[0].Rows[0];
                                    int nAcc = Conversions.ToInteger(oUserDetails["nDirKey"]);

                                    object argmyWeb = (object)this.myWeb;
                                    var oMembershipProv = new BaseProvider(ref argmyWeb, this.myWeb.moConfig["MembershipProvider"]);
                                    this.myWeb = (Protean.Cms)argmyWeb;

                                    cResponse = Conversions.ToString(oMembershipProv.Activities.ResetUserAcct(this.myWeb, nAcc));

                                }

                                if (!string.IsNullOrEmpty(cResponse))
                                {
                                    XmlNode argoNode = oFrmElmt;
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, cResponse, true);
                                    oFrmElmt = (XmlElement)argoNode;
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
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmResetAccount", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
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

                        if (moPolicy is not null)
                        {
                            passwordClass = "required strongPassword";
                            passwordValidation = "strongPassword";
                        }

                        string FormName = "ResetPassword";

                        // maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        if (!base.load("/xforms/directory/" + FormName + ".xml", this.myWeb.maCommonFolders))
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

                        oGrp = base.addGroup(ref base.moXformElmt, "Password", sLabel: "Reset Password");
                        base.submission("SetPassword", "", "POST");
                        oPI1 = base.addSecret(ref oGrp, "cDirPassword", true, "Password", ref passwordClass);
                        XmlElement argoBindParent = null;
                        base.addBind("cDirPassword", "Password/cDirPassword", "true()", passwordValidation, oBindParent: ref argoBindParent);
                        string argsClass = "required";
                        oPI2 = base.addSecret(ref oGrp, "cDirPassword2", true, "Confirm Password", ref argsClass);
                        XmlElement argoBindParent1 = null;
                        base.addBind("cDirPassword2", "Password/cDirPassword2", "true()", oBindParent: ref argoBindParent1);
                        oSB = (XmlElement)base.addSubmit(ref oGrp, "SetPassword", "Set Password");

                    Check:
                        ;


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            // any additonal validation goes here
                            // Passwords match?
                            if (Strings.Len(this.goRequest["cDirPassword2"]) > 0)
                            {
                                if ((this.goRequest["cDirPassword"] ?? "") != (this.goRequest["cDirPassword2"] ?? ""))
                                {
                                    base.valid = false;
                                    base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must match ");
                                }
                            }

                            if (moPolicy is null)
                            {
                                // Password policy?
                                if (Strings.Len(base.Instance.SelectSingleNode("Password/cDirPassword").InnerXml) < 4)
                                {
                                    base.valid = false;
                                    base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must be 4 characters long ");
                                }
                            }

                            if (base.valid)
                            {
                                var oMembership = new Protean.Cms.Membership(ref this.myWeb);

                                if (!oMembership.ReactivateAccount((int)nAccount, this.goRequest["cDirPassword"]))
                                {
                                    base.addNote("cDirPassword2", Protean.xForm.noteTypes.Alert, "There was a problem changing the password");
                                    base.valid = false;
                                }
                                else
                                {
                                    XmlNode argoNode = oGrp;
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "The password has been updated.");
                                    oGrp = (XmlElement)argoNode;
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
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmResetPassword", ex, "", "", Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public override XmlElement xFrmConfirmPassword(string AccountHash)
                {
                    try
                    {
                        var oMembership = new Protean.Cms.Membership(ref this.myWeb);
                        int SubmittedUserId = Conversions.ToInteger("0" + this.goRequest["id"]);

                        int nUserId = oMembership.DecryptResetLink(SubmittedUserId, AccountHash);

                        return xFrmConfirmPassword(nUserId);
                    }

                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "addInput", ex, "", "", Protean.stdTools.gbDebug);
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

                        if (moPolicy is not null)
                        {
                            passwordClass = "required strongPassword";
                            passwordValidation = "strongPassword";
                        }

                        string FormName = "ConfirmPassword";

                        // maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        if (!base.load("/xforms/directory/" + FormName + ".xml", this.myWeb.maCommonFolders))
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


                        base.NewFrm("ConfirmPassword");

                        base.Instance.InnerXml = "<Password><cDirPassword/><cDirPassword2/></Password>";
                        string formTitle = "<span class=\"trans-2031\">Enter new password</span>";
                        oGrp = base.addGroup(ref base.moXformElmt, "Password", sLabel: formTitle);
                        base.submission("SetPassword", "", "POST");
                        oPI1 = base.addSecret(ref oGrp, "cDirPassword", true, "Password", ref passwordClass);
                        XmlElement argoBindParent = null;
                        base.addBind("cDirPassword", "Password/cDirPassword", "true()", passwordValidation, oBindParent: ref argoBindParent);
                        string argsClass = "required secret";
                        oPI2 = base.addSecret(ref oGrp, "cDirPassword2", true, "Confirm Password", ref argsClass);
                        XmlElement argoBindParent1 = null;
                        base.addBind("cDirPassword2", "Password/cDirPassword2", "true()", oBindParent: ref argoBindParent1);
                        oSB = (XmlElement)base.addSubmit(ref oGrp, "SetPassword", "Set Password");

                    Check:
                        ;

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            // any additonal validation goes here
                            // Passwords match?
                            if (Strings.Len(this.goRequest["cDirPassword2"]) > 0)
                            {
                                if ((this.goRequest["cDirPassword"] ?? "") != (this.goRequest["cDirPassword2"] ?? ""))
                                {
                                    base.valid = false;
                                    base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must match ");
                                }
                            }

                            // Password policy?
                            if (Strings.Len(base.Instance.SelectSingleNode("Password/cDirPassword").InnerXml) < 4)
                            {
                                base.valid = false;
                                base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must be 4 characters long ");
                            }

                            var oMembership = new Protean.Cms.Membership(ref this.myWeb);

                            if (moPolicy is not null)
                            {
                                // Password History?
                                if (!oMembership.CheckPasswordHistory((int)nUserId, this.goRequest["cDirPassword"]))
                                {
                                    base.valid = false;
                                    base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "<span class=\"msg-1020\">You cannot use a password you have used recently.</span>");
                                }
                            }

                            if (base.valid)
                            {

                                int nAccount = (int)nUserId;
                                if (nAccount == 0)
                                {
                                    base.addNote("cDirPassword2", Protean.xForm.noteTypes.Alert, "This reset link has already been used");
                                    base.valid = false;
                                }
                                else if (!oMembership.ReactivateAccount(nAccount, this.goRequest["cDirPassword"]))
                                {
                                    base.addNote("cDirPassword2", Protean.xForm.noteTypes.Alert, "There was an problem updating your account");
                                    base.valid = false;
                                }
                                else
                                {
                                    XmlNode argoNode = oGrp;
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Your password has been reset <a href=\"/" + this.myWeb.moConfig["LogonRedirectPath"] + "\">click here</a> to login");
                                    oGrp = (XmlElement)argoNode;
                                    oPI1.ParentNode.RemoveChild(oPI1);
                                    oPI2.ParentNode.RemoveChild(oPI2);
                                    oSB.ParentNode.RemoveChild(oSB);
                                    // delete failed logon attempts record
                                    string sSql = "delete from tblActivityLog where nActivityType = " + ((int)Protean.Cms.dbHelper.ActivityType.LogonInvalidPassword).ToString() + " and nUserDirId=" + nAccount;
                                    this.myWeb.moDbHelper.ExeProcessSql(sSql);

                                    if (this.myWeb.mnUserId == 0)
                                    {
                                        this.myWeb.mnUserId = nAccount;
                                    }
                                    // myWeb.msRedirectOnEnd = myWeb.moConfig("LogonRedirectPath")
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "addInput", ex, "", "", Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                // Public Overrides Function xFrmEditDirectoryItem(Optional ByVal id As Long = 0, Optional ByVal cDirectorySchemaName As String = "User", Optional ByVal parId As Long = 0, Optional ByVal cXformName As String = "") As XmlElement
                // Return xFrmEditDirectoryItem(id, cDirectorySchemaName, parId, cXformName, "", Nothing)
                // End Function
                // Public Overrides Function xFrmEditDirectoryItem(Optional ByVal id As Long = 0, Optional ByVal cDirectorySchemaName As String = "User", Optional ByVal parId As Long = 0, Optional ByVal cXformName As String = "", Optional ByVal FormXML As String = "") As XmlElement
                // Return xFrmEditDirectoryItem(id, cDirectorySchemaName, parId, cXformName, FormXML, Nothing)
                // End Function

                public override XmlElement xFrmEditDirectoryItem([Optional, DefaultParameterValue(0L)] long id, [Optional, DefaultParameterValue("User")] string cDirectorySchemaName, [Optional, DefaultParameterValue(0L)] long parId, [Optional, DefaultParameterValue("")] string cXformName, [Optional, DefaultParameterValue("")] string FormXML, [Optional] ref XmlElement IntanceAppend)
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

                        if (string.IsNullOrEmpty(cXformName))
                            cXformName = cDirectorySchemaName;

                        // ok lets load in an xform from the file location.
                        if (string.IsNullOrEmpty(FormXML))
                        {
                            string formPath = "/xforms/directory/" + cXformName + ".xml";
                            if (this.myWeb.moConfig["cssFramework"] == "bs5")
                            {
                                formPath = "/features/membership/" + cXformName + ".xml";
                            }
                            if (IntanceAppend is not null)
                            {
                                base.bProcessRepeats = false;
                            }
                            if (!base.load(formPath, this.myWeb.maCommonFolders))
                            {
                                // load a default content xform if no alternative.
                            }
                        }
                        else
                        {
                            base.NewFrm(cXformName);
                            base.loadtext(FormXML);

                        }

                        if (id > 0L)
                        {
                            base.Instance.InnerXml = this.moDbHelper.getObjectInstance(Protean.Cms.dbHelper.objectTypes.Directory, id);
                            cCurrentPassword = this.Instance.SelectSingleNode("*/cDirPassword").InnerText;
                        }

                        if (IntanceAppend is not null)
                        {
                            if (this.goSession["tempInstance"] is not null)
                            {
                                base.Instance = (XmlElement)this.goSession["tempInstance"];
                                base.bProcessRepeats = true;
                                base.LoadInstance(base.Instance, true);
                                this.goSession["tempInstance"] = base.Instance;
                            }
                            else
                            {
                                // this enables an overload to add additional Xml for updating.
                                var importedNode = this.Instance.OwnerDocument.ImportNode(IntanceAppend, true);
                                base.Instance.AppendChild(importedNode);
                                base.bProcessRepeats = true;
                                base.LoadInstance(base.Instance, true);
                                this.goSession["tempInstance"] = base.Instance;
                            }
                        }

                        cDirectorySchemaName = base.Instance.SelectSingleNode("tblDirectory/cDirSchema").InnerText;

                        // lets add the groups to the instance
                        oGrpElmt = this.moDbHelper.getGroupsInstance(id, parId);
                        base.Instance.InsertAfter(oGrpElmt, base.Instance.LastChild);

                        if (cDirectorySchemaName == "User")
                        {

                            if (this.goConfig["Subscriptions"] == "on")
                            {
                                var oSub = new Protean.Cms.Cart.Subscriptions(ref this.myWeb);
                                var argoElmt = base.Instance;
                                oSub.AddSubscriptionToUserXML(ref argoElmt, (int)id);
                                base.Instance = argoElmt;
                            }

                            // now lets check our security, and if we are encrypted lets not show the password on edit.
                            if (id > 0L)
                            {
                                // RJP 7 Nov 2012. Added LCase as a precaution against people entering string in Protean.Cms.Config lowercase, i.e. md5.
                                if (this.myWeb.moConfig["MembershipEncryption"] is not null)
                                {
                                    if (Strings.LCase(this.myWeb.moConfig["MembershipEncryption"]).StartsWith("md5") | Strings.LCase(this.myWeb.moConfig["MembershipEncryption"]).StartsWith("sha"))
                                    {
                                        // Remove password (and confirm password) fields
                                        foreach (XmlElement oPwdNode in base.moXformElmt.SelectNodes("/group/descendant-or-self::*[contains(@bind,'cDirPassword')]"))
                                            oPwdNode.ParentNode.RemoveChild(oPwdNode);
                                    }
                                }

                            }

                            // Is the membership email address secure.
                            if (!string.IsNullOrEmpty(this.myWeb.moConfig["SecureMembershipAddress"]) & this.myWeb.mbAdminMode == false)
                            {
                                XmlElement oSubElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant::submission");
                                if (this.myWeb.mcPagePath is null)
                                {
                                    oSubElmt.SetAttribute("action", this.myWeb.moConfig["SecureMembershipAddress"] + this.myWeb.mcOriginalURL);
                                }
                                else
                                {
                                    oSubElmt.SetAttribute("action", this.myWeb.moConfig["SecureMembershipAddress"] + this.myWeb.moConfig["ProjectPath"] + "/" + this.myWeb.mcPagePath);
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
                                        if ((base.Instance.SelectSingleNode("*/cDirName").InnerXml ?? "") != (base.Instance.SelectSingleNode("*/cDirXml/User/Email").InnerXml ?? ""))
                                        {
                                            if (!this.moDbHelper.checkUserUnique(base.Instance.SelectSingleNode("*/cDirName").InnerXml, id))
                                            {
                                                base.valid = false;
                                                base.addNote("cDirName", Protean.xForm.noteTypes.Alert, "This username already exists please select another");
                                            }
                                        }

                                        // Email Exists?
                                        if (!this.moDbHelper.checkEmailUnique(base.Instance.SelectSingleNode("*/cDirXml/User/Email").InnerXml, id))
                                        {
                                            base.valid = false;
                                            if ((base.Instance.SelectSingleNode("*/cDirName").InnerXml ?? "") == (base.Instance.SelectSingleNode("*/cDirXml/User/Email").InnerXml ?? ""))
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
                                        if (base.moXformElmt.SelectSingleNode("/group/descendant-or-self::*[@bind='cDirPassword']") is not null)
                                        {
                                            // Passwords match?
                                            if (Strings.Len(this.goRequest["cDirPassword2"]) > 0)
                                            {
                                                if ((this.goRequest["cDirPassword"] ?? "") != (this.goRequest["cDirPassword2"] ?? ""))
                                                {
                                                    base.valid = false;
                                                    base.addNote("cDirPassword", Protean.xForm.noteTypes.Alert, "Passwords must match ");
                                                }
                                            }

                                            if (moPolicy is not null)
                                            {
                                                // Password policy?
                                                if (Strings.Len(base.Instance.SelectSingleNode("*/cDirPassword").InnerXml) < 4)
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

                                string cPassword = this.Instance.SelectSingleNode("*/cDirPassword").InnerText;
                                string cClearPassword = cPassword;
                                // RJP 7 Nov 2012. Added LCase to MembershipEncryption. Note leave the value below for md5Password hard coded as MD5.
                                if (Strings.LCase(this.myWeb.moConfig["MembershipEncryption"]) == "md5salt")
                                {
                                    string cSalt = Encryption.generateSalt();
                                    string inputPassword = string.Concat(cSalt, cPassword); // Take the users password and add the salt at the front
                                    string md5Password = Encryption.HashString(inputPassword, "md5", true); // Md5 the marged string of the password and salt
                                    string resultPassword = string.Concat(md5Password, ":", cSalt); // Adds the salt to the end of the hashed password
                                    cPassword = resultPassword; // Store the resultant password with salt in the database
                                }
                                else
                                {
                                    cPassword = Encryption.HashString(cPassword, Strings.LCase(this.myWeb.moConfig["MembershipEncryption"]), true);
                                } // plain - md5 - sha1
                                if (!((cPassword ?? "") == (cCurrentPassword ?? "")) & !((cClearPassword ?? "") == (cCurrentPassword ?? "")))
                                {
                                    this.Instance.SelectSingleNode("*/cDirPassword").InnerText = cPassword;
                                }

                                if (id > 0L)
                                {

                                    this.moDbHelper.setObjectInstance(Protean.Cms.dbHelper.objectTypes.Directory, base.Instance, id);
                                    if (this.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='EditContent' or @bind='EditContent']") is not null)
                                    {
                                        base.addNote("EditContent", Protean.xForm.noteTypes.Alert, "<span class=\"msg-1010\">Your details have been updated.</span>", true);
                                    }
                                    else
                                    {
                                        XmlElement oSubElmt = (XmlElement)this.moXformElmt.SelectSingleNode("descendant-or-self::group[parent::Content][1]");
                                        if (oSubElmt is not null)
                                        {
                                            XmlNode argoNode = oSubElmt;
                                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "<span class=\"msg-1010\">Your details have been updated.</span>", true);
                                            oSubElmt = (XmlElement)argoNode;
                                        }
                                    }
                                }
                                else
                                {
                                    // add new
                                    id = Conversions.ToLong(this.moDbHelper.setObjectInstance(Protean.Cms.dbHelper.objectTypes.Directory, base.Instance));

                                    // update the instance with the id
                                    base.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText = id.ToString();
                                    addNewitemToParId = parId > 0L;
                                    base.addNote("EditContent", Protean.xForm.noteTypes.Alert, "This user has been added.", true);

                                    // add addresses
                                    if (base.Instance.SelectSingleNode("tblCartContact") is not null)
                                    {
                                        foreach (XmlElement oCartContact in base.Instance.SelectNodes("tblCartContact"))
                                        {
                                            var TempInstance = new XmlDocument();
                                            TempInstance.LoadXml("<instance/>");
                                            TempInstance.DocumentElement.InnerXml = oCartContact.OuterXml;
                                            TempInstance.DocumentElement.SelectSingleNode("tblCartContact/nContactDirId").InnerText = id.ToString();
                                            this.moDbHelper.setObjectInstance(Protean.Cms.dbHelper.objectTypes.CartContact, TempInstance.DocumentElement);
                                        }
                                    }

                                    // Save the member code, if applicable
                                    useMemberCode(cCodeUsed, id);

                                    // If member codes were being applied then reconstruct the Group Instance.
                                    if (Protean.Cms.gbMemberCodes & !string.IsNullOrEmpty(cCodeUsed))
                                    {
                                        oGrpElmt = this.moDbHelper.getGroupsInstance(id, parId);
                                        base.Instance.ReplaceChild(oGrpElmt, base.Instance.LastChild);
                                    }

                                }

                                // lets add the user to any groups
                                if ((cDirectorySchemaName == "User" | cDirectorySchemaName == "Company") & maintainMembershipsOnAdd)
                                {
                                    maintainMembershipsFromXForm((int)id);

                                    // we want to ad the user to a specified group from a pick list of groups.
                                    XmlElement GroupsElmt = (XmlElement)base.Instance.SelectSingleNode("groups");
                                    if (GroupsElmt is not null)
                                    {
                                        if (!string.IsNullOrEmpty(GroupsElmt.GetAttribute("addIds")))
                                        {
                                            foreach (var i in Strings.Split(GroupsElmt.GetAttribute("addIds"), ","))
                                                this.moDbHelper.maintainDirectoryRelation(Conversions.ToLong(i), id, false);
                                        }
                                    }
                                    // code added by sonali for pure360
                                    if (cDirectorySchemaName == "User")
                                    {
                                        System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                                        string sMessagingProvider = moMailConfig["messagingprovider"];
                                        var oMessaging = new Providers.Messaging.BaseProvider(ref this.myWeb, sMessagingProvider);
                                        string ListId = moMailConfig["AllUsersList"];
                                        if (ListId is not null)
                                        {
                                            object valDict = new Dictionary<string, string>();

                                            valDict.Add("Email", this.Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/Email").InnerText);
                                            valDict.Add("FirstName", this.Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/FirstName").InnerText);
                                            valDict.Add("LastName", this.Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/LastName").InnerText);
                                            if (this.Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/Mobile") is not null)
                                            {
                                                valDict.Add("Mobile", this.Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/Mobile").InnerText);
                                            }
                                            string Name = this.Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/FirstName").InnerText;
                                            string Email = this.Instance.SelectSingleNode("descendant-or-self::*/cDirXml/User/Email").InnerText;

                                            oMessaging.Activities.addToList(ListId, Name, Email, valDict);
                                        }

                                    }
                                }

                                if (addNewitemToParId)
                                {
                                    this.moDbHelper.maintainDirectoryRelation(parId, id, false);
                                }

                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditDirectoryItem", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                /// <summary>
                ///     Maintains membership between userid and groups as found in xForm
                /// </summary>
                /// <param name="nUserId">The user id to be associated with</param>
                /// <param name="cGroupNodeListXPath">The XPath from the xform instance to the group nodes.</param>
                /// <remarks>Group nodes membership is indicated by a boolean attribute "isMember"</remarks>
                public void maintainMembershipsFromXForm(int nUserId, string cGroupNodeListXPath = "groups/group", string Email = null, bool addOnly = false)
                {
                    this.myWeb.PerfMon.Log(mcModuleName, "maintainMembershipsFromXForm", "start");
                    string sSql = "";
                    // Dim oDr As SqlDataReader
                    var userMembershipIds = new List<int>();

                    try
                    {
                        // get the users current memberships
                        sSql = "select * from tblDirectoryRelation where nDirChildId  = " + nUserId;
                        using (var oDr = this.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
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
                                    this.moDbHelper.maintainDirectoryRelation(Conversions.ToLong(oElmt.GetAttribute("id")), (long)nUserId, false, cEmail: Email, cGroup: oElmt.GetAttribute("name"), isLast: bIsLast);
                                }
                            }

                            // if user is in group
                            else if (userMembershipIds.Contains(Conversions.ToInteger(oElmt.GetAttribute("id"))))
                            {
                                if (addOnly == false)
                                {
                                    this.moDbHelper.maintainDirectoryRelation(Conversions.ToLong(oElmt.GetAttribute("id")), (long)nUserId, true, cEmail: Email, cGroup: oElmt.GetAttribute("name"), isLast: bIsLast);
                                }
                            }
                        }


                        this.myWeb.PerfMon.Log(mcModuleName, "maintainMembershipsFromXForm", "end");
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "maintainMembershipsFromXForm", ex, "", "", Protean.stdTools.gbDebug);
                    }

                }

                public virtual XmlElement xFrmActivationCode(long nUserId, string cXformName = "ActivationCode", string cFormXml = "")
                {


                    string cProcessInfo = "";
                    string cCodeUsed = "";
                    try
                    {


                        // Load the form
                        if (Protean.Cms.gbMemberCodes)
                        {


                            if (string.IsNullOrEmpty(cFormXml))
                            {
                                if (!base.load("/xforms/directory/" + cXformName + ".xml", this.myWeb.maCommonFolders))
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
                                var argoNode = base.Instance;
                                Xml.NodeState(ref argoNode, "//CodeSet", "", "", 1, null, "", zcCodeSet, bCheckTrimmedInnerText: false);
                                base.Instance = argoNode;

                                // Validate the code
                                cCodeUsed = validateMemberCode("//" + cCodeNode, cCodeNode, zcCodeSet);

                                // Invalidate if the user id and code are bad
                                if (base.valid & (!(nUserId > 0L) | string.IsNullOrEmpty(cCodeUsed)))
                                {

                                    base.valid = false;
                                    base.addNote(cCodeNode, Protean.xForm.noteTypes.Alert, "There was a problem using this code.  Please try another code, or contact the website team.");

                                }

                                // Prcess the valid form
                                if (base.valid)
                                {

                                    // Save the member code, if applicable
                                    useMemberCode(cCodeUsed, nUserId);

                                    // Add an indication that the form succeeded.

                                    var argoParent = base.model.SelectSingleNode("instance");
                                    XmlNode argoNodeFromXPath = null;
                                    Protean.xmlTools.addElement(ref argoParent, "formState", "success", oNodeFromXPath: ref argoNodeFromXPath);

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
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmActivationCode", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
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
                        XmlNodeState localNodeState() { var argoNode1 = base.Instance; var ret = Xml.NodeState(ref argoNode1, cXPathToCode, "", "", 1, null, "", cCode, bCheckTrimmedInnerText: false); base.Instance = argoNode1; return ret; }

                        if (Protean.Cms.gbMemberCodes && !string.IsNullOrEmpty(cXPathToCode) && localNodeState() == XmlNodeState.HasContents)


                        {

                            if (this.myWeb.moDbHelper.CheckCode(cCode, cCodeSet))
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
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "validateMemberCode", ex, "", "", Protean.stdTools.gbDebug);
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

                        if (Protean.Cms.gbMemberCodes & !string.IsNullOrEmpty(cCode))
                        {

                            // Use the code
                            this.myWeb.moDbHelper.UseCode(cCode, (int)nUserId);

                            // Get the CSV list of directory items for membership
                            string cCodeCSVList = Conversions.ToString(this.myWeb.moDbHelper.GetDataValue("SELECT tblCodes.cCodeGroups FROM tblCodes INNER JOIN tblCodes Child ON tblCodes.nCodeKey = Child.nCodeParentId WHERE (Child.cCode = '" + cCode + "')", 1, null, ""));

                            // Process the List
                            foreach (string cDirId in cCodeCSVList.Split(','))
                            {
                                if (Information.IsNumeric(cDirId))
                                    this.moDbHelper.maintainDirectoryRelation(Conversions.ToLong(cDirId), nUserId, false);
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "useMemberCode", ex, "", "", Protean.stdTools.gbDebug);
                    }
                }

            }

            public class AdminProcess : Protean.Cms.Admin
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                public event OnErrorWithWebEventHandler OnErrorWithWeb;

                public delegate void OnErrorWithWebEventHandler(ref Protean.Cms myweb, object sender, Tools.Errors.ErrorEventArgs e);


                private Providers.Payment.DefaultProvider.AdminXForms _oAdXfm;

                public object oAdXfm
                {
                    set
                    {
                        _oAdXfm = (Providers.Payment.DefaultProvider.AdminXForms)value;
                    }
                    get
                    {
                        return _oAdXfm;
                    }
                }

                public AdminProcess(ref Protean.Cms aWeb) : base(ref aWeb)
                {
                }

                public void maintainUserInGroup(long nUserId, long nGroupId, bool remove, string cUserEmail = null, string cGroupName = null)
                {
                    this.myWeb.PerfMon.Log("Messaging", "maintainUserInGroup");
                    try
                    {
                    }

                    // do nothing this is a placeholder

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(Protean.Cms.Admin.mcModuleName, "maintainUserInGroup", ex, ""));
                    }
                }

            }


            public class Activities
            {
                private const string mcModuleName = "Providers.Membership.Eonic.Activities";

                #region ErrorHandling

                // for anything controlling web
                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

                protected virtual void OnComponentError(ref Protean.Cms myWeb, object sender, Tools.Errors.ErrorEventArgs e) // Handles moDBHelper.OnErrorwithweb, oSync.OnError, moCalendar.OnError
                {
                    // deals with the error
                    var moDbHelper = myWeb.moDbHelper;

                    Protean.stdTools.returnException(ref myWeb.msException, e.ModuleName, e.ProcedureName, e.Exception, myWeb.mcEwSiteXsl, e.AddtionalInformation, Protean.stdTools.gbDebug);
                    // close connection pooling
                    if (moDbHelper is not null)
                    {
                        try
                        {
                            moDbHelper.CloseConnection();
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    // then raises a public event
                    OnError?.Invoke(sender, e);
                }

                #endregion

                public virtual long GetUserSessionId(ref Protean.Base myWeb)
                {

                    string sProcessInfo = "";
                    var moSession = myWeb.moSession;
                    int mnUserId = myWeb.mnUserId;

                    try
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(moSession["nUserId"], 0, false)))
                        {
                            myWeb.mnUserId = Conversions.ToInteger(moSession["nUserId"]);
                        }
                        else
                        {
                            myWeb.mnUserId = 0;
                        }

                        return (long)myWeb.mnUserId;
                    }

                    catch (Exception ex)
                    {
                        Protean.Cms argmyWeb = (Protean.Cms)myWeb;
                        OnComponentError(ref argmyWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserSessionId", ex, sProcessInfo));
                        myWeb = argmyWeb;
                        return default;
                    }
                }


                public virtual string GetUserId(ref Protean.Base myWeb)
                {
                    myWeb.PerfMon.Log(mcModuleName, "getUserId");
                    string sProcessInfo = "";
                    string sReturnValue = null;
                    string cLogonCmd = "";
                    int mnUserId = myWeb.mnUserId;
                    var moSession = myWeb.moSession;
                    var moRequest = myWeb.moRequest;
                    var moResponse = myWeb.moResponse;
                    var moConfig = myWeb.moConfig;
                    var moDbHelper = myWeb.moDbHelper;
                    string sDomain = myWeb.moRequest.ServerVariables["HTTP_HOST"];

                    try
                    {
                        sProcessInfo = Conversions.ToString(Operators.ConcatenateObject(Interaction.IIf(myWeb.moRequest.ServerVariables["HTTPS"] == "on", "https://", "http://"), sDomain));
                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(moSession["nUserId"], null, false), mnUserId == 0)))
                        {

                            // first lets check for a remember me cookie
                            string rememberMeMode = moConfig["RememberMeMode"];
                            if (moRequest.Cookies["RememberMeUserId"] is not null & rememberMeMode != "KeepCookieAfterLogoff" & !string.IsNullOrEmpty(rememberMeMode))
                            {
                                if (Information.IsNumeric(moRequest.Cookies["RememberMeUserId"].Value))
                                {
                                    // AG - MAJOR SECURITY FUBAR!!! Commenting out for now.
                                    // mnUserId = moRequest.Cookies("RememberMeUserId").Value
                                }
                            }


                            // If single user login is set we need to check if a cookie exists and reconnect if needs be
                            var oldSession = moRequest.Cookies["ewslock"];
                            if (Protean.Cms.gbSingleLoginSessionPerUser && oldSession is not null)
                            {


                                // Find out what the last thing the user did (login-wise) was
                                if (!string.IsNullOrEmpty(oldSession.Value))
                                {

                                    // We need to find a few things:
                                    // 1. Find the last activity for the session id (within the timeout period)
                                    // 2. Work out if the user id for that session id has had a more recent session
                                    // 3. If not, then we can assume that the session is still alive and we need to update for this session.
                                    string lastSeenQuery = "SELECT nUserDirId FROM (SELECT TOP 1 nUserDirId,nACtivityType,cSessionId, (SELECT TOP 1 l2.cSessionId As sessionId FROM tblActivityLog l2 WHERE l2.nUserDirId = l.nUserDirId ORDER BY dDateTime DESC) As lastSessionForUser FROM tblActivityLog l " + "WHERE cSessionId = " + Database.SqlString(oldSession.Value) + " " + "AND DATEDIFF(s,l.dDateTime,GETDATE()) < " + Protean.Cms.gnSingleLoginSessionTimeout + " " + "ORDER BY dDateTime DESC) s WHERE s.cSessionId = s.lastSessionForUser AND s.nACtivityType <> " + ((int)Protean.Cms.dbHelper.ActivityType.Logoff).ToString();



                                    int lastSeenUser = Conversions.ToInteger(moDbHelper.GetDataValue(lastSeenQuery, 1, null, (object)0));
                                    if (lastSeenUser > 0)
                                    {

                                        // Reconnect with the new session ID
                                        mnUserId = lastSeenUser;
                                        moResponse.Cookies["ewslock"].Value = moSession.SessionID;
                                        moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.SessionReconnectFromCookie, (long)mnUserId, 0L, 0L, oldSession.Value);

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
                        else if (Protean.Cms.gbCart && !string.IsNullOrEmpty(moRequest["refSessionId"]))
                        {

                            long nCartUserId = 0L;

                            if (moDbHelper is not null)
                            {
                                nCartUserId = Conversions.ToLong(moDbHelper.GetDataValue(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT nCartUserDirId FROM tblCartOrder o where o.cCartSchemaName='Order' and o.cCartSessionId = '", Protean.stdTools.SqlFmt(moRequest["refSessionId"])), "'")), 1, null, (object)0));
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
                        Protean.Cms argmyWeb = (Protean.Cms)myWeb;
                        OnComponentError(ref argmyWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserId", ex, sProcessInfo));
                        myWeb = argmyWeb;
                        return null;
                    }
                }

                public virtual void SetUserId(ref Protean.Cms myWeb)
                {
                    myWeb.PerfMon.Log("Web", "getUserId");
                    string sProcessInfo = "";
                    string sReturnValue = null;
                    string cLogonCmd = "";
                    int mnUserId = myWeb.mnUserId;
                    var moSession = myWeb.moSession;

                    try
                    {
                        if (moSession["nUserId"] is not null)
                        {
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(moSession["nUserId"], 0, false)))
                            {
                                moSession["nUserId"] = (object)mnUserId;
                            }
                            else if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectNotEqual(moSession["nUserId"], mnUserId, false), string.IsNullOrEmpty(Conversions.ToString(moSession["PreviewUser"])))))
                            {
                                // reset to a different value
                                moSession["nUserId"] = (object)mnUserId;
                                // have a user id so dont remove it
                            }
                        }
                        else
                        {
                            moSession["nUserId"] = (object)mnUserId;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SetUserId", ex, sProcessInfo));
                    }
                }

                public virtual XmlElement GetUserXML(ref Protean.Cms myWeb, long nUserId = 0L)
                {
                    myWeb.PerfMon.Log("Web", "GetUserXML");
                    string sProcessInfo = "";
                    int mnUserId = myWeb.mnUserId;
                    var moDbHelper = myWeb.moDbHelper;
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

                public virtual string MembershipProcess(ref Protean.Cms myWeb)
                {
                    myWeb.PerfMon.Log("Web", "MembershipProcess");
                    string sProcessInfo = "";
                    string sReturnValue = null;
                    string cLogonCmd = "";
                    int mnUserId = myWeb.mnUserId;
                    var moSession = myWeb.moSession;
                    var moRequest = myWeb.moRequest;
                    var moConfig = myWeb.moConfig;
                    var moDbHelper = myWeb.moDbHelper;

                    try
                    {

                        // Dim adXfm As EonicProvider.AdminXForms = New EonicProvider.AdminXForms(myWeb)
                        var adXfm = myWeb.getAdminXform();

                        adXfm.open(myWeb.moPageXml);

                        // logoff handler
                        if (Strings.LCase(myWeb.moRequest["ewCmd"]) == "logoff" & mnUserId != 0)
                        {

                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["ewCmd"], "PreviewOn", false)))
                            {
                                LogOffProcess(ref myWeb);
                            }

                            // we are logging off so lets redirect
                            if (myWeb.moConfig["BaseUrl"] is not null & !string.IsNullOrEmpty(myWeb.moConfig["BaseUrl"]))
                            {
                                myWeb.msRedirectOnEnd = myWeb.moConfig["BaseUrl"];
                            }
                            else if (myWeb.moConfig["RootPageId"] is not null & !string.IsNullOrEmpty(myWeb.moConfig["RootPageId"]))
                            {
                                myWeb.msRedirectOnEnd = myWeb.moConfig["ProjectPath"] + "/";
                            }
                            else
                            {
                                myWeb.msRedirectOnEnd = myWeb.mcOriginalURL;
                            }
                            var oMembership = new Protean.Cms.Membership(ref myWeb);
                            oMembership.ProviderActions(ref myWeb, "logoffAction");
                            // BaseUrl
                            sReturnValue = "LogOff";
                        }

                        else if (myWeb.moRequest["ewCmd"] == "CancelSubscription")
                        {

                            var oAdx = new Protean.Cms.Admin.AdminXforms(ref myWeb);
                            oAdx.moPageXML = myWeb.moPageXml;

                            var oCSFrm = oAdx.xFrmConfirmCancelSubscription(myWeb.moRequest["nUserId"], myWeb.moRequest["nSubscriptionId"], mnUserId, myWeb.mbAdminMode);
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

                                    if (Protean.Cms.gbSingleLoginSessionPerUser)
                                    {
                                        myWeb.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Logon, (long)mnUserId, 0L);
                                    }
                                    else
                                    {
                                        myWeb.moDbHelper.CommitLogToDB(Protean.Cms.dbHelper.ActivityType.Logon, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "");
                                    }

                                    // Now we want to reload as permissions have changed
                                    if (moSession is not null)
                                    {
                                        if (moSession["cLogonCmd"] is not null)
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

                        if (!string.IsNullOrEmpty(myWeb.moConfig["SecureMembershipAddress"]))
                        {

                            var oMembership = new Protean.Cms.Membership(ref myWeb);
                            oMembership.OnErrorWithWeb += this.OnComponentError;
                            oMembership.SecureMembershipProcess(sReturnValue);

                        }

                        // display logon form for all pages if user is not logged on.
                        if (mnUserId == 0 & (myWeb.moRequest["ewCmd"] != "passwordReminder" & myWeb.moRequest["ewCmd"] != "ActivateAccount" & myWeb.moRequest["ewCmd"] != "AR"))
                        {

                            XmlElement oXfmElmt = (XmlElement)adXfm.xFrmUserLogon();
                            bool bAdditionalChecks = false;

                            if (Conversions.ToBoolean(!adXfm.valid))
                            {
                                // Call in additional authentication checks
                                if (Strings.LCase(myWeb.moConfig["AlternativeAuthentication"]) == "on")
                                {
                                    bAdditionalChecks = AlternativeAuthentication(ref myWeb);
                                }

                            }

                            if (Conversions.ToBoolean(Operators.OrObject(adXfm.valid, bAdditionalChecks)))
                            {
                                myWeb.moContentDetail = (XmlElement)null;
                                mnUserId = myWeb.mnUserId;
                                if (myWeb.moSession is not null)
                                    myWeb.moSession["nUserId"] = (object)mnUserId;
                                if (myWeb.moRequest["cRemember"] == "true")
                                {
                                    var oCookie = new System.Web.HttpCookie("RememberMe");
                                    oCookie.Value = mnUserId.ToString();
                                    oCookie.Expires = DateAndTime.DateAdd(DateInterval.Day, 60d, DateTime.Now);
                                    myWeb.moResponse.Cookies.Add(oCookie);
                                }
                                sReturnValue = "LogOn";

                                if (Protean.Cms.gbSingleLoginSessionPerUser)
                                {
                                    myWeb.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Logon, (long)mnUserId, 0L);
                                }
                                else
                                {
                                    myWeb.moDbHelper.CommitLogToDB(Protean.Cms.dbHelper.ActivityType.Logon, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "");
                                }

                                // Now we want to reload as permissions have changed
                                if (moSession is not null)
                                {
                                    if (moSession["cLogonCmd"] is not null)
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

                        else if (moRequest["ewCmd"] == "ActivateAccount")
                        {

                            var oMembership = new Protean.Cms.Membership(ref myWeb);
                            oMembership.OnErrorWithWeb += this.OnComponentError;

                            XmlElement oXfmElmt;
                            oXfmElmt = (XmlElement)adXfm.xFrmActivateAccount();

                            myWeb.AddContentXml(ref oXfmElmt);

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
                                if (moRequest["ewCmd"] is not null)
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

                                if (sharedKey is not null & loginKey is not null)
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
                                    catch (Exception ex)
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
                                                    myWeb.moSession["nUserId"] = (object)myWeb.mnUserId;
                                                    myWeb.moSession["adminMode"] = "true";
                                                    myWeb.mbAdminMode = true;
                                                    if (string.IsNullOrEmpty(ewCmd))
                                                    {
                                                        myWeb.moSession["ewCmd"] = "PreviewOn";
                                                        myWeb.moSession["PreviewDate"] = (object)DateTime.Now.Date;
                                                        myWeb.moSession["PreviewUser"] = (object)0;
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
                                                    myWeb.moSession["nUserId"] = (object)myWeb.mnUserId;
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
                                                    myWeb.moSession["nUserId"] = (object)myWeb.mnUserId;
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
                                    XmlElement argoContentElmt = (XmlElement)adXfm.xFrmUserIntegrations(mnUserId, moRequest["ewCmd2"]);
                                    myWeb.AddContentXml(ref argoContentElmt);
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

                            myWeb.GetUserXML((long)mnUserId);

                            // moPageXml.DocumentElement.AppendChild(moPageXml.ImportNode(GetUserXML().CloneNode(True), True))
                        }

                        // Site Redirection Process
                        if (!string.IsNullOrEmpty(moConfig["SiteGroupRedirection"]) & mnUserId != 0)
                        {
                            myWeb.SiteRedirection();
                        }


                        // behaviour based on layout page (not required in V5 sites, this behaviour uses modules instead)
                        this.MembershipV4LayoutProcess(ref myWeb, adXfm);

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

                public virtual string MembershipV4LayoutProcess(ref Protean.Cms myWeb, object adXfm)
                {
                    myWeb.PerfMon.Log("Web", "MembershipProcess");
                    string sProcessInfo = "";
                    string sReturnValue = null;
                    string cLogonCmd = "";
                    int mnUserId = myWeb.mnUserId;
                    var moSession = myWeb.moSession;
                    var moRequest = myWeb.moRequest;
                    var moConfig = myWeb.moConfig;
                    var moDbHelper = myWeb.moDbHelper;
                    bool clearUserId = false;

                    try
                    {

                        // behaviour based on layout page
                        switch (myWeb.moPageXml.SelectSingleNode("/Page/@layout").Value ?? "")
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
                                            oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(mnUserId, "User", default, "UserMyAccount");
                                        }
                                        else
                                        {
                                            oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(mnUserId, "User", default, "UserMyAccount", oContentForm.OuterXml);
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
                                            oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(mnUserId, "User", default, "UserRegister");
                                        }
                                        else
                                        {
                                            oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryItem(mnUserId, "User", default, "UserRegister", oContentForm.OuterXml);
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
                                                        adXfm.addNote("EditContent", Protean.xForm.noteTypes.Hint, "Thanks for registering you have been sent an email with a link you must click to activate your account", (object)true);

                                                        // lets get the new userid from the instance
                                                        mnUserId = Conversions.ToInteger(adXfm.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText);

                                                        // first we set the user account to be pending
                                                        myWeb.moDbHelper.setObjectStatus(Protean.Cms.dbHelper.objectTypes.Directory, Protean.Cms.dbHelper.Status.Pending, (long)mnUserId);

                                                        var oMembership = new Protean.Cms.Membership(ref myWeb);
                                                        oMembership.OnErrorWithWeb += this.OnComponentError;
                                                        oMembership.AccountActivateLink(mnUserId);
                                                        clearUserId = true;
                                                        myWeb.moDbHelper.CommitLogToDB(Protean.Cms.dbHelper.ActivityType.Register, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "Send Activation"); // Auto logon
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        mnUserId = Conversions.ToInteger(adXfm.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText);
                                                        if (moSession is not null)
                                                        {
                                                            myWeb.mnUserId = mnUserId;
                                                            moSession["nUserId"] = (object)mnUserId;
                                                        }

                                                        myWeb.moDbHelper.CommitLogToDB(Protean.Cms.dbHelper.ActivityType.Register, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "First Logon");

                                                        // Now we want to reload as permissions have changed

                                                        if (moSession is not null)
                                                        {
                                                            if (moSession["cLogonCmd"] is not null)
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

                                            // send registration confirmation
                                            string xsltPath = "/xsl/email/registration.xsl";

                                            if (File.Exists(Protean.stdTools.goServer.MapPath(xsltPath)))
                                            {
                                                var oUserElmt = myWeb.moDbHelper.GetUserXML((long)mnUserId);
                                                if (clearUserId)
                                                    mnUserId = 0; // clear user Id so we don't stay logged on
                                                var oElmtPwd = myWeb.moPageXml.CreateElement("Password");
                                                oElmtPwd.InnerText = moRequest["cDirPassword"];
                                                oUserElmt.AppendChild(oElmtPwd);

                                                XmlElement oUserEmail = (XmlElement)oUserElmt.SelectSingleNode("Email");
                                                string fromName = moConfig["SiteAdminName"];
                                                string fromEmail = moConfig["SiteAdminEmail"];
                                                string recipientEmail = "";
                                                if (oUserEmail is not null)
                                                    recipientEmail = oUserEmail.InnerText;
                                                string SubjectLine = "Your Registration Details";
                                                var oMsg = new Protean.Messaging(ref myWeb.msException);
                                                // send an email to the new registrant
                                                if (!string.IsNullOrEmpty(recipientEmail))
                                                {
                                                    Protean.Cms.dbHelper argodbHelper = null;
                                                    sProcessInfo = Conversions.ToString(oMsg.emailer(oUserElmt, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, "Message Sent", "Message Failed", odbHelper: ref argodbHelper));
                                                }
                                                // send an email to the webadmin
                                                recipientEmail = moConfig["SiteAdminEmail"];
                                                if (File.Exists(Protean.stdTools.goServer.MapPath(moConfig["ProjectPath"] + "/xsl/email/registrationAlert.xsl")))
                                                {
                                                    Protean.Cms.dbHelper argodbHelper1 = null;
                                                    sProcessInfo = Conversions.ToString(oMsg.emailer(oUserElmt, moConfig["ProjectPath"] + "/xsl/email/registrationAlert.xsl", "New User", recipientEmail, fromEmail, SubjectLine, "Message Sent", "Message Failed", odbHelper: ref argodbHelper1));
                                                }
                                                oMsg = (Protean.Messaging)null;
                                            }

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
                                        object argmyWeb = (object)myWeb;
                                        var oMembershipProv = new BaseProvider(ref argmyWeb, myWeb.moConfig["MembershipProvider"]);
                                        myWeb = (Protean.Cms)argmyWeb;
                                        var oAdXfm = oMembershipProv.AdminXforms;
                                        oXfmElmt = (XmlElement)oAdXfm.xFrmResetAccount();

                                    }
                                    myWeb.AddContentXml(ref oXfmElmt);
                                    break;
                                }

                            case "Password_Change":
                                {

                                    object argmyWeb1 = (object)myWeb;
                                    var oMembershipProv = new BaseProvider(ref argmyWeb1, myWeb.moConfig["MembershipProvider"]);
                                    myWeb = (Protean.Cms)argmyWeb1;
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
                                        var argoNode = myWeb.moPageXml.DocumentElement;
                                        if (Xml.NodeState(ref argoNode, "Contents/Content[@type='xform' and @name='ActivationCode']", "", "", 1, oXfmElmt, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                        {
                                            oXfmElmt.ParentNode.RemoveChild(oXfmElmt);
                                            cExistingFormXml = oXfmElmt.OuterXml;
                                        }

                                        oXfmElmt = (XmlElement)adXfm.xFrmActivationCode(mnUserId, default, cExistingFormXml);
                                        if (oXfmElmt is not null)
                                        {
                                            myWeb.AddContentXml(ref oXfmElmt);

                                            // If the form has been successful, then check for a redirect
                                            // If the redirectpage is a number then continue
                                            if (oXfmElmt.SelectSingleNode("//instance/formState[node()='success']") is not null)
                                            {

                                                // Activation was succesful, let's prepare the redirect

                                                // Clear the cache.
                                                string cSql = Conversions.ToString(Operators.ConcatenateObject("DELETE dbo.tblXmlCache " + " WHERE cCacheSessionID = '" + moSession.SessionID + "' " + "         AND nCacheDirId = ", Protean.stdTools.SqlFmt(mnUserId.ToString())));

                                                myWeb.moDbHelper.ExeProcessSqlorIgnore(cSql);

                                                // Check if the redirect is another page or just redirect to the current url
                                                if (oXfmElmt.SelectSingleNode("//instance/RedirectPage[number(.)=number(.)]") is not null)
                                                {
                                                    myWeb.msRedirectOnEnd = Conversions.ToString(Operators.ConcatenateObject(moConfig["ProjectPath"] + "/?pgid=", adXfm.Instance.SelectSingleNode("RedirectPage").InnerText));
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
                                                    XmlElement oXfmElmt = (XmlElement)adXfm.xFrmEditDirectoryContact(moRequest["id"], mnUserId);
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
                                                    foreach (XmlElement oContactElmt in myWeb.GetUserXML((long)mnUserId).SelectNodes("descendant-or-self::Contact"))
                                                    {
                                                        XmlElement oId = (XmlElement)oContactElmt.SelectSingleNode("nContactKey");
                                                        if (oId is not null)
                                                        {
                                                            if ((oId.InnerText ?? "") == (moRequest["id"] ?? ""))
                                                            {
                                                                moDbHelper.DeleteObject(Protean.Cms.dbHelper.objectTypes.CartContact, Conversions.ToLong(moRequest["id"]));
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

                public void LogSingleUserSession()
                {
                    // Do Nothing
                }



                public void LogSingleUserSession(ref Protean.Cms myWeb)
                {

                    int mnUserId = myWeb.mnUserId;
                    var moSession = myWeb.moSession;
                    var moRequest = myWeb.moRequest;
                    var moConfig = myWeb.moConfig;
                    var moDbHelper = myWeb.moDbHelper;
                    var moResponse = myWeb.moResponse;

                    try
                    {
                        // If logged on and in single login per user mode, log a session continuation flag.
                        if (Protean.Cms.gbSingleLoginSessionPerUser & mnUserId > 0 & moSession is not null)
                        {
                            // Log the session
                            moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.SessionContinuation, (long)mnUserId, 0L, 0L, 0L, "", true);

                            // Add a cookie / update it.
                            var cookieLock = moRequest.Cookies["ewslock"];
                            if (cookieLock is null)
                            {
                                cookieLock = new System.Web.HttpCookie("ewslock");
                            }
                            cookieLock.Expires = DateTime.Now.AddSeconds((double)Protean.Cms.gnSingleLoginSessionTimeout);
                            cookieLock.Value = moSession.SessionID;
                            moResponse.Cookies.Add(cookieLock);
                        }
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LogSingleUserSession", ex, ""));
                    }
                }

                public virtual bool AlternativeAuthentication(ref Protean.Cms myWeb)
                {

                    myWeb.PerfMon.Log("Web", "AlternativeAuthentication");


                    string cProcessInfo = "";
                    bool bCheck = false;
                    string cToken = "";
                    string cKey = "";
                    string cDecrypted = "";
                    int nReturnId;

                    int mnUserId = myWeb.mnUserId;
                    var moSession = myWeb.moSession;
                    var moRequest = myWeb.moRequest;
                    var moConfig = myWeb.moConfig;
                    var moDbHelper = myWeb.moDbHelper;


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

                                    // Authentication is by way of e-mail address
                                    cProcessInfo = "Email authenctication: Retrieving user for email: " + cDecrypted;
                                    // Get the user id based on the e-mail address
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
                                    // Get the user id based on the e-mail address
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

                public virtual void LogOffProcess(ref Protean.Cms myWeb)
                {
                    myWeb.PerfMon.Log("Web", "LogOffProcess");
                    string cProcessInfo = "";

                    int mnUserId = myWeb.mnUserId;
                    var moSession = myWeb.moSession;
                    var moRequest = myWeb.moRequest;
                    var moConfig = myWeb.moConfig;
                    var moDbHelper = myWeb.moDbHelper;
                    var moResponse = myWeb.moResponse;

                    try
                    {

                        cProcessInfo = "Commit to Log";
                        if (Protean.Cms.gbSingleLoginSessionPerUser)
                        {
                            moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Logoff, (long)mnUserId, 0L);
                            if (moRequest.Cookies["ewslock"] is not null)
                            {
                                moResponse.Cookies["ewslock"].Expires = DateTime.Now.AddDays(-1);
                            }
                        }
                        else
                        {
                            moDbHelper.CommitLogToDB(Protean.Cms.dbHelper.ActivityType.Logoff, mnUserId, moSession.SessionID, DateTime.Now, 0, 0, "");
                        }


                        // Call this BEFORE clearing the user ID.
                        cProcessInfo = "Clear Site Stucture";
                        moDbHelper.clearStructureCacheUser();

                        // Clear the user ID.
                        mnUserId = 0;
                        myWeb.mnUserId = 0;

                        // Clear the cart
                        if (moSession is not null && Protean.Cms.gbCart)
                        {
                            string cSql = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where (cCartSessionId = '" + moSession.SessionID + "' and cCartSessionId <> '')";
                            moDbHelper.ExeProcessSql(cSql);
                        }

                        if (moSession is not null)
                        {
                            cProcessInfo = "Abandon Session";
                            // AJG : Question - why does this not clear the Session ID?
                            moSession["nEwUserId"] = (object)null;
                            moSession["nUserId"] = (object)null;
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

                public virtual string UserEditProcess(ref Protean.Cms myWeb)
                {
                    myWeb.PerfMon.Log("Web", "UserEditProcess");
                    string sProcessInfo = "";
                    string sReturnValue = null;
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

                public virtual string ResetUserAcct(ref Protean.Cms myWeb, int nUserId)
                {
                    myWeb.PerfMon.Log("Web", "ResetUserAcct");
                    string sProcessInfo = "";
                    string sReturnValue = null;
                    try
                    {

                        // Get the user XML
                        var oUserXml = myWeb.moDbHelper.GetUserXML((long)nUserId, false);

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

                            var oMembership = new Protean.Cms.Membership(ref myWeb);
                            var oEmailDoc = new XmlDocument();
                            oEmailDoc.AppendChild(oEmailDoc.CreateElement("AccountReset"));
                            oEmailDoc.DocumentElement.AppendChild(oEmailDoc.ImportNode(oUserXml, true));
                            oEmailDoc.DocumentElement.SetAttribute("Link", oMembership.AccountResetLink(nUserId));
                            oEmailDoc.DocumentElement.SetAttribute("Url", myWeb.mcOriginalURL);
                            oEmailDoc.DocumentElement.SetAttribute("logonRedirect", Conversions.ToString(myWeb.moSession["LogonRedirectId"]));
                            oEmailDoc.DocumentElement.SetAttribute("lang", myWeb.mcPageLanguage);
                            oEmailDoc.DocumentElement.SetAttribute("translang", myWeb.mcPreferredLanguage);

                            var oMessage = new Protean.Messaging(ref myWeb.msException);

                            var fs = new Protean.fsHelper();
                            string path = fs.FindFilePathInCommonFolders("/xsl/Email/passwordReset.xsl", myWeb.maCommonFolders);
                            if (myWeb.moConfig["cssFramework"] == "bs5")
                            {
                                path = "/email/passwordReset.xsl";
                            }
                            Protean.Cms.dbHelper argodbHelper = null;
                            sReturnValue = Conversions.ToString(oMessage.emailer(oEmailDoc.DocumentElement, path, myWeb.moConfig["SiteAdminName"], myWeb.moConfig["SiteAdminEmail"], userEmail, "Account Reset ", odbHelper: ref argodbHelper));
                            sReturnValue = Conversions.ToString(Interaction.IIf(sReturnValue == "Message Sent", "<span class=\"msg-1035\">" + sReturnValue + " to </span>" + userEmail, ""));

                        } // endif oUserXml Is Nothing


                        return sReturnValue;
                    }

                    catch (Exception ex)
                    {
                        OnComponentError(ref myWeb, this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ResetUserAcct", ex, sProcessInfo));
                        return null;
                    }
                }



                public Activities()
                {

                }
            }
        }

    }
}