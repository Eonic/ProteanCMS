
// ***********************************************************************
// $Library:     eonic.adminXforms
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2006 Eonic Ltd.
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Tools;
using static Protean.Tools.Text;
using static Protean.Tools.Xml;

namespace Protean
{
    public partial class Cms
    {
        public partial class Admin
        {
            public class AdminXforms : Cms.xForm
            {
                private const string mcModuleName = "ewcommon.AdminXForms";
                // Private Const gbDebug As Boolean = True
                public Cms.dbHelper moDbHelper;
                public NameValueCollection goConfig; // = WebConfigurationManager.GetWebApplicationSection("protean/web")
                public bool mbAdminMode = false;
                public System.Web.HttpRequest moRequest;

                // Error Handling hasn't been formally set up for AdminXforms so this is just for method invocation found in xfrmEditContent
                public new event OnErrorEventHandler OnError;

                public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

                private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
                {
                    Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, e.ProcedureName, e.Exception, "", e.AddtionalInformation, Protean.stdTools.gbDebug);
                }

                // Public myWeb As Protean.Cms

                public AdminXforms(ref Cms aWeb) : base(ref aWeb)
                {
                    OnError += _OnError;
                    Protean.stdTools.PerfMon.Log("AdminXforms", "New");
                    try
                    {
                        this.myWeb = aWeb;
                        goConfig = this.myWeb.moConfig;
                        moDbHelper = this.myWeb.moDbHelper;
                        moRequest = this.myWeb.moRequest;
                        base.cLanguage = this.myWeb.mcPageLanguage;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "New", ex, "", "", Protean.stdTools.gbDebug);
                    }
                }

                public AdminXforms(ref string sException) : base(sException)
                {
                    OnError += _OnError;
                }

                public new void open(XmlDocument oPageXml)
                {
                    string cProcessInfo = "";
                    try
                    {
                        this.moPageXML = oPageXml;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "Open", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                    }
                }

                public XmlElement xFrmGenericObject(int nObjectId, string FormTitle, string xFormPath, Cms.dbHelper.objectTypes ptnObjectType)
                {
                    string cProcessInfo = "";
                    try
                    {
                        // This is a generic function for a framework for all protean object.
                        // This is not intended for use but rather as an example of how xforms are processed

                        // The instance of the form needs to be saved in the session to allow repeating elements to be edited prior to saving in the database.
                        object InstanceSessionName = "tempInstance_" + ptnObjectType.ToString() + "_" + nObjectId.ToString();
                        base.NewFrm(FormTitle);
                        base.bProcessRepeats = false;

                        // We load the xform from a file, it may be in local or in common folders.
                        base.load(xFormPath, this.myWeb.maCommonFolders);

                        // We get the instance
                        if (nObjectId > 0)
                        {
                            base.bProcessRepeats = true;
                            if (this.myWeb.moSession(InstanceSessionName) is null)
                            {
                                var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                existingInstance.InnerXml = moDbHelper.getObjectInstance(ptnObjectType, nObjectId).Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "").Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                                base.LoadInstance(existingInstance);
                                this.myWeb.moSession(InstanceSessionName) = base.Instance;
                            }
                            else
                            {
                                base.LoadInstance(this.myWeb.moSession["tempInstance"]);
                            }
                        }

                        this.moXformElmt.SelectSingleNode("descendant-or-self::instance").InnerXml = base.Instance.InnerXml;
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                int nCId = Conversions.ToInteger(moDbHelper.setObjectInstance(ptnObjectType, base.Instance, (long)nObjectId));
                                this.myWeb.moSession["tempInstance"] = null;
                            }
                        }
                        else if (base.isTriggered)
                        {
                            // we have clicked a trigger so we must update the instance
                            base.updateInstanceFromRequest();
                            // lets save the instance
                            this.goSession(InstanceSessionName) = base.Instance;
                        }
                        else
                        {
                            this.goSession(InstanceSessionName) = base.Instance;
                        }

                        // we populate the values onto the form.
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditUserSubscription", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmUserLogon(string FormName = "UserLogon")
                {
                    string cProcessInfo = "";
                    try
                    {
                        object argmyWeb = (object)this.myWeb;
                        var oMembershipProv = new Protean.Providers.Membership.BaseProvider(ref argmyWeb, this.myWeb.moConfig["MembershipProvider"]);
                        var oAdXfm = oMembershipProv.AdminXforms;
                        oAdXfm.xFrmUserLogon(FormName);
                        this.valid = Conversions.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmUserLogon", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }


                // Public Overridable Function xFrmUserLogon(Optional ByVal FormName As String = "UserLogon") As XmlElement
                // Dim oFrmElmt As XmlElement
                // Dim oSelElmt As XmlElement
                // Dim sValidResponse As String
                // Dim cProcessInfo As String = ""
                // Dim bRememberMe As Boolean = False
                // Try
                // MyBase.NewFrm("UserLogon")

                // If mbAdminMode And myWeb.mnUserId = 0 Then GoTo BuildForm
                // If myWeb.moConfig("RememberMeMode") = "KeepCookieAfterLogoff" Or myWeb.moConfig("RememberMeMode") = "ClearCookieAfterLogoff" Then bRememberMe = True

                // If Not MyBase.load("/xforms/directory/" & FormName & ".xml", myWeb.maCommonFolders) Then
                // GoTo BuildForm
                // Else
                // GoTo Check
                // End If
                // BuildForm:
                // MyBase.submission("UserLogon", "", "post", "form_check(this)")
                // oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "UserDetails", "", "Please fill in your login details below")
                // MyBase.addInput(oFrmElmt, "cUserName", True, "Username")
                // MyBase.addBind("cUserName", "user/username", "true()")
                // MyBase.addSecret(oFrmElmt, "cPassword", True, "Password")
                // MyBase.addBind("cPassword", "user/password", "true()")

                // MyBase.addSubmit(oFrmElmt, "ewSubmit", "Login")
                // MyBase.Instance.InnerXml = "<user rememberMe=""""><username/><password/></user>"
                // Check:


                // ' Set the remember me value
                // If bRememberMe Then

                // ' Add elements to the form if not present
                // If Tools.Xml.NodeState(MyBase.model, "bind[@id='cRemember']") = Tools.Xml.XmlNodeState.NotInstantiated Then
                // oSelElmt = MyBase.addSelect(MyBase.moXformElmt.SelectSingleNode("group"), "cRemember", True, "&#160;", "", ApperanceTypes.Full)
                // MyBase.addOption(oSelElmt, "Remember me", "true")
                // MyBase.addBind("cRemember", "user/@rememberMe", "false()")
                // End If

                // ' Retrieve values from the cookie
                // If Not goRequest.Cookies("RememberMeUserName") Is Nothing Then
                // Dim cRememberedUsername As String = goRequest.Cookies("RememberMeUserName").Value
                // Dim bRemembered As Boolean = False
                // Dim oElmt As XmlElement = Nothing

                // If cRememberedUsername <> "" Then bRemembered = True

                // If Tools.Xml.NodeState(MyBase.Instance, "user", , , , oElmt) <> Tools.Xml.XmlNodeState.NotInstantiated And Not (MyBase.isSubmitted) Then

                // oElmt.SetAttribute("rememberMe", LCase(CStr(bRemembered)))
                // Tools.Xml.NodeState(MyBase.Instance, "user/username", cRememberedUsername)

                // End If
                // End If
                // End If



                // If MyBase.isSubmitted Then
                // MyBase.updateInstanceFromRequest()
                // MyBase.validate()
                // If MyBase.valid Then

                // sValidResponse = moDbHelper.validateUser(goRequest("cUserName"), goRequest("cPassWord"))

                // If IsNumeric(sValidResponse) Then
                // myWeb.mnUserId = CLng(sValidResponse)
                // moDbHelper.mnUserId = CLng(sValidResponse)
                // If Not goSession Is Nothing Then
                // goSession("nUserId") = myWeb.mnUserId
                // End If

                // ' Set the remember me cookie
                // If bRememberMe Then
                // If goRequest("cRemember") = "true" Then
                // Dim oCookie As System.Web.HttpCookie
                // If Not (myWeb.moRequest.Cookies("RememberMeUserName") Is Nothing) Then goResponse.Cookies.Remove("RememberMeUserName")
                // oCookie = New System.Web.HttpCookie("RememberMeUserName")
                // oCookie.Value = myWeb.moRequest("cUserName")
                // oCookie.Expires = DateAdd(DateInterval.Day, 60, Now())
                // goResponse.Cookies.Add(oCookie)

                // If Not (myWeb.moRequest.Cookies("RememberMeUserId") Is Nothing) Then goResponse.Cookies.Remove("RememberMeUserId")
                // oCookie = New System.Web.HttpCookie("RememberMeUserId")
                // oCookie.Value = myWeb.mnUserId
                // oCookie.Expires = DateAdd(DateInterval.Day, 60, Now())
                // goResponse.Cookies.Add(oCookie)
                // Else
                // goResponse.Cookies("RememberMeUserName").Expires = DateTime.Now.AddDays(-1)
                // goResponse.Cookies("RememberMeUserId").Expires = DateTime.Now.AddDays(-1)
                // End If
                // End If
                // Else
                // valid = False
                // MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
                // End If
                // Else
                // valid = False
                // End If
                // If valid = False Then
                // myWeb.mnUserId = 0
                // End If
                // End If

                // MyBase.addValues()
                // Return MyBase.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "xFrmUserLogon", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function


                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmPasswordReminder()
                {
                    string cProcessInfo = "";
                    try
                    {
                        object argmyWeb = (object)this.myWeb;
                        var oMembershipProv = new Protean.Providers.Membership.BaseProvider(ref argmyWeb, this.myWeb.moConfig["MembershipProvider"]);
                        var oAdXfm = oMembershipProv.AdminXforms;
                        oAdXfm.xFrmPasswordReminder();
                        this.valid = Conversions.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmPasswordReminder", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmActivateAccount()
                {
                    string cProcessInfo = "";
                    try
                    {
                        object argmyWeb = (object)this.myWeb;
                        var oMembershipProv = new Protean.Providers.Membership.BaseProvider(ref argmyWeb, this.myWeb.moConfig["MembershipProvider"]);
                        var oAdXfm = oMembershipProv.AdminXforms;
                        oAdXfm.xFrmActivateAccount();
                        this.valid = Conversions.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmActivateAccount", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmResetAccount()
                {
                    string cProcessInfo = "";
                    try
                    {
                        object argmyWeb = (object)this.myWeb;
                        var oMembershipProv = new Protean.Providers.Membership.BaseProvider(ref argmyWeb, this.myWeb.moConfig["MembershipProvider"]);
                        var oAdXfm = oMembershipProv.AdminXforms;
                        oAdXfm.xFrmResetAccount();
                        this.valid = Conversions.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmResetAccount", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmConfirmPassword(string AccountHash)
                {
                    string cProcessInfo = "";
                    try
                    {
                        object argmyWeb = (object)this.myWeb;
                        var oMembershipProv = new Protean.Providers.Membership.BaseProvider(ref argmyWeb, this.myWeb.moConfig["MembershipProvider"]);
                        var oAdXfm = oMembershipProv.AdminXforms;
                        oAdXfm.xFrmConfirmPassword(AccountHash);
                        this.valid = Conversions.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmConfirmPassword", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmConfirmPassword(long nUserId)
                {
                    string cProcessInfo = "";
                    try
                    {
                        object argmyWeb = (object)this.myWeb;
                        var oMembershipProv = new Protean.Providers.Membership.BaseProvider(ref argmyWeb, this.myWeb.moConfig["MembershipProvider"]);
                        var oAdXfm = oMembershipProv.AdminXforms;
                        oAdXfm.xFrmConfirmPassword(nUserId);
                        this.valid = Conversions.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmConfirmPassword", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmUserIntegrations(long userId, string secondaryCommand)
                {
                    string processInfo = "";
                    string provider = "";
                    string formName = "UserIntegrations";
                    XmlElement userInstance = null;
                    XmlElement credentials = null;
                    XmlElement permissions = null;
                    try
                    {

                        // Handle the direct integration commands
                        this.myWeb.CommonActions();
                        provider = this.myWeb.moRequest["provider"];

                        // Create the form
                        if (!string.IsNullOrEmpty(secondaryCommand))
                            formName += "." + secondaryCommand;
                        base.NewFrm(formName);

                        // Add form parameters
                        if (!string.IsNullOrEmpty(provider))
                        {
                            var integrationsFormParameters = new string[] { provider };
                            this.FormParameters = integrationsFormParameters;
                        }

                        // Load the form
                        if (base.load("/xforms/directory/" + formName + ".xml", this.myWeb.maCommonFolders))
                        {
                            if (userId > 0L)
                            {

                                // Load the user instance
                                userInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                userInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Directory, userId);

                                // If this is permissions then we need to extract the permissions first, or create new ones.
                                if (secondaryCommand == "Permissions")
                                {
                                    credentials = (XmlElement)userInstance.SelectSingleNode("//Credentials[@provider='" + provider + "']");
                                    permissions = (XmlElement)credentials.SelectSingleNode("Permissions");
                                    if (permissions is object)
                                        base.Instance.InnerXml = permissions.OuterXml;
                                }
                                else
                                {
                                    // By default add the user instance
                                    base.Instance.InnerXml = userInstance.InnerXml;
                                }


                                // Handle the submits
                                if (base.isSubmitted())
                                {
                                    base.updateInstanceFromRequest();
                                    base.validate();
                                    if (base.valid)
                                    {

                                        // Permissions
                                        if (secondaryCommand == "Permissions")
                                        {

                                            // Need to put the permissions node back into the user instance
                                            if (credentials is object)
                                            {
                                                if (permissions is object)
                                                {
                                                    XmlElement localfirstElement() { var argoElement = base.Instance; var ret = Xml.firstElement(ref argoElement); base.Instance = argoElement; return ret; }

                                                    credentials.ReplaceChild(credentials.OwnerDocument.ImportNode(localfirstElement(), true), permissions);
                                                }
                                                else
                                                {
                                                    XmlElement localfirstElement1() { var argoElement = base.Instance; var ret = Xml.firstElement(ref argoElement); base.Instance = argoElement; return ret; }

                                                    credentials.AppendChild(credentials.OwnerDocument.ImportNode(localfirstElement1(), true));
                                                }

                                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, userInstance, userId);
                                            }
                                            else
                                            {
                                                // No credentials to save something against
                                            }
                                        }



                                        // Go back to UserIntegrations
                                        var newCmd = new NameValueCollection(1);
                                        newCmd.Add("ewCmd", "UserIntegrations");
                                        this.myWeb.msRedirectOnEnd = Tools.Http.Utils.BuildURIFromRequest(this.myWeb.moRequest, newCmd, "integration,provider").ToString();
                                    }
                                }
                            }
                            else
                            {
                                // No user found - bad times.

                            }

                            // Because we have handled the integrations, we need to follow up any responses
                            foreach (XmlElement response in this.myWeb.PageXMLResponses)
                            {
                                switch (response.GetAttribute("type") ?? "")
                                {
                                    case "Redirect":
                                        {
                                            this.myWeb.msRedirectOnEnd = response.InnerText;
                                            break;
                                        }

                                    case "Alert":
                                        {
                                            var argoNode = base.moXformElmt.SelectSingleNode("group");
                                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, response.InnerText);
                                            break;
                                        }

                                    case "Hint":
                                        {
                                            var argoNode1 = base.moXformElmt.SelectSingleNode("group");
                                            base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, response.InnerText);
                                            break;
                                        }

                                    case "Help":
                                        {
                                            var argoNode2 = base.moXformElmt.SelectSingleNode("group");
                                            base.addNote(ref argoNode2, Protean.xForm.noteTypes.Help, response.InnerText);
                                            break;
                                        }
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmUserIntegrations", ex, "", processInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmWebSettings()
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;
                    try
                    {
                        oFsh = new Protean.fsHelper();
                        oFsh.open(this.moPageXML);
                        base.NewFrm("WebSettings");
                        base.submission("WebSettings", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "WebSettings", "", "Web Settings");

                        // oSelElmt = MyBase.addSelect1(oFrmElmt, "ewDatabaseType", True, "DB Type", "", ApperanceTypes.Full)
                        // MyBase.addOption(oSelElmt, "MS SQL", "SQL")
                        // MyBase.addOption(oSelElmt, "MS Access", "Access")
                        // MyBase.addBind("ewDatabaseType", "web/add[@key='DatabaseType']/@value", "true()")

                        // MyBase.addInput(oFrmElmt, "ewDatabaseServer", True, "DB Server")
                        // MyBase.addBind("ewDatabaseServer", "web/add[@key='DatabaseServer']/@value", "true()")

                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Any Changes you make to this form risk making this site completely non-functional. Please be sure you know what you are doing before making any changes, or call your web developer for support");
                        base.addInput(ref oFrmElmt, "ewDatabaseName", true, "DB Name");
                        XmlElement argoBindParent = null;
                        base.addBind("ewDatabaseName", "web/add[@key='DatabaseName']/@value", "true()", oBindParent: ref argoBindParent);

                        // MyBase.addInput(oFrmElmt, "ewDatabaseAuth", True, "DB Auth")
                        // MyBase.addBind("ewDatabaseAuth", "web/add[@key='DatabaseAuth']/@value", "false()")

                        base.addInput(ref oFrmElmt, "ewDatabaseUsername", true, "DB Username");
                        XmlElement argoBindParent1 = null;
                        base.addBind("ewDatabaseUsername", "web/add[@key='DatabaseUsername']/@value", "false()", oBindParent: ref argoBindParent1);
                        base.addInput(ref oFrmElmt, "ewDatabasePassword", true, "DB Passowrd");
                        XmlElement argoBindParent2 = null;
                        base.addBind("ewDatabasePassword", "web/add[@key='DatabasePassword']/@value", "false()", oBindParent: ref argoBindParent2);

                        // oSelElmt = MyBase.addSelect1(oFrmElmt, "ewSiteXsl", True, "Site Scheme", "", ApperanceTypes.Minimal)
                        // ' MyBase.addOptionsFilesFromDirectory(oSelElmt, "/ewcommon/xsl/scheme", ".xsl")
                        // MyBase.addOption(oSelElmt, "standard.xsl [bespoke]", "/xsl/standard.xsl")
                        // MyBase.addBind("ewSiteXsl", "web/add[@key='SiteXsl']/@value", "false()")

                        base.addInput(ref oFrmElmt, "ewRootPageId", true, "Root Page Id");
                        XmlElement argoBindParent3 = null;
                        base.addBind("ewRootPageId", "web/add[@key='RootPageId']/@value", "true()", oBindParent: ref argoBindParent3);
                        base.addInput(ref oFrmElmt, "ewBaseUrl", true, "Base URL");
                        XmlElement argoBindParent4 = null;
                        base.addBind("ewBaseUrl", "web/add[@key='BaseUrl']/@value", "false()", oBindParent: ref argoBindParent4);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewDebug", true, "Debug", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent5 = null;
                        base.addBind("ewDebug", "web/add[@key='Debug']/@value", "true()", oBindParent: ref argoBindParent5);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewCompiledTransform", true, "Compiled Transform", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent6 = null;
                        base.addBind("ewCompiledTransform", "web/add[@key='CompiledTransform']/@value", "true()", oBindParent: ref argoBindParent6);
                        base.addInput(ref oFrmElmt, "ewImageRootPath", true, "Images Directory");
                        XmlElement argoBindParent7 = null;
                        base.addBind("ewImageRootPath", "web/add[@key='ImageRootPath']/@value", "true()", oBindParent: ref argoBindParent7);
                        base.addInput(ref oFrmElmt, "ewDocRootPath", true, "Docs Directory");
                        XmlElement argoBindParent8 = null;
                        base.addBind("ewDocRootPath", "web/add[@key='DocRootPath']/@value", "true()", oBindParent: ref argoBindParent8);
                        base.addInput(ref oFrmElmt, "ewMediaRootPath", true, "Media Directory");
                        XmlElement argoBindParent9 = null;
                        base.addBind("ewMediaRootPath", "web/add[@key='MediaRootPath']/@value", "true()", oBindParent: ref argoBindParent9);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewCart", true, "Shopping Cart", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent10 = null;
                        base.addBind("ewCart", "web/add[@key='Cart']/@value", "true()", oBindParent: ref argoBindParent10);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewMembership", true, "Membership", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent11 = null;
                        base.addBind("ewMembership", "web/add[@key='Membership']/@value", "true()", oBindParent: ref argoBindParent11);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewMailingList", true, "Mailing List", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent12 = null;
                        base.addBind("ewMailingList", "web/add[@key='MailingList']/@value", "true()", oBindParent: ref argoBindParent12);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewSubscriptions", true, "Subscriptions", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent13 = null;
                        base.addBind("ewSubscriptions", "web/add[@key='Subscriptions']/@value", "true()", oBindParent: ref argoBindParent13);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewActivityLogging", true, "Activity Logging", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent14 = null;
                        base.addBind("ewActivityLogging", "web/add[@key='ActivityLogging']/@value", "true()", oBindParent: ref argoBindParent14);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewIPLogging", true, "IP Tracking", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent15 = null;
                        base.addBind("ewIPLogging", "web/add[@key='IPLogging']/@value", "true()", oBindParent: ref argoBindParent15);
                        base.addInput(ref oFrmElmt, "ewMailserver", true, "Mailserver");
                        XmlElement argoBindParent16 = null;
                        base.addBind("ewMailserver", "web/add[@key='MailServer']/@value", "false()", oBindParent: ref argoBindParent16);
                        base.addInput(ref oFrmElmt, "ewSiteAdminEmail", true, "Webmaster Email");
                        XmlElement argoBindParent17 = null;
                        base.addBind("ewSiteAdminEmail", "web/add[@key='SiteAdminEmail']/@value", "true()", oBindParent: ref argoBindParent17);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewSearch", true, "Content Search", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent18 = null;
                        base.addBind("ewSearch", "web/add[@key='ContentSearch']/@value", "true()", oBindParent: ref argoBindParent18);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewSiteSearch", true, "Index Search", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent19 = null;
                        base.addBind("ewSiteSearch", "web/add[@key='SiteSearch']/@value", "false()", oBindParent: ref argoBindParent19);
                        base.addInput(ref oFrmElmt, "ewSiteSearchPath", true, "Index Path");
                        XmlElement argoBindParent20 = null;
                        base.addBind("ewSiteSearchPath", "web/add[@key='SiteSearchPath']/@value", "false()", oBindParent: ref argoBindParent20);
                        base.addInput(ref oFrmElmt, "ewGoogleContentTypes", true, "Google Sitemap Content Types");
                        XmlElement argoBindParent21 = null;
                        base.addBind("ewGoogleContentTypes", "web/add[@key='GoogleContentTypes']/@value", "false()", oBindParent: ref argoBindParent21);
                        base.addInput(ref oFrmElmt, "ewShowRelatedBriefContentTypes", true, "Include Related Content for these Content Types");
                        XmlElement argoBindParent22 = null;
                        base.addBind("ewShowRelatedBriefContentTypes", "web/add[@key='ShowRelatedBriefContentTypes']/@value", "false()", oBindParent: ref argoBindParent22);
                        base.addInput(ref oFrmElmt, "ewShowRelatedBriefDepth", true, "Depth to get related content for brief content.");
                        XmlElement argoBindParent23 = null;
                        base.addBind("ewShowRelatedBriefDepth", "web/add[@key='ShowRelatedBriefDepth']/@value", "false()", "number", oBindParent: ref argoBindParent23);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewVersionControl", true, "Version Control", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent24 = null;
                        base.addBind("ewVersionControl", "web/add[@key='VersionControl']/@value", "false()", oBindParent: ref argoBindParent24);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewLegacyRedirect", true, "Legacy URL Forwarding", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent25 = null;
                        base.addBind("ewLegacyRedirect", "web/add[@key='LegacyRedirect']/@value", "true()", oBindParent: ref argoBindParent25);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewPageURLFormat", true, "Page URL Format for Spaces", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Use Hyphens", "hyphens");
                        base.addOption(ref oSelElmt, "No preference", "off");
                        XmlElement argoBindParent26 = null;
                        base.addBind("ewPageURLFormat", "web/add[@key='PageURLFormat']/@value", "true()", oBindParent: ref argoBindParent26);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewAllowContentDetailAccess", true, "Always allow access to content detail regardless of location and permissions", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent27 = null;
                        base.addBind("ewAllowContentDetailAccess", "web/add[@key='AllowContentDetailAccess']/@value", "true()", oBindParent: ref argoBindParent27);
                        base.addSubmit(ref oFrmElmt, "", "Save Settings");
                        var oCfg = WebConfigurationManager.OpenWebConfiguration("/" + this.myWeb.moConfig["ProjectPath"]);
                        DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection("protean/web");
                        var oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]))
                        {
                            base.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml();

                            // code here to replace any missing nodes
                            // all of the required config settings
                            var aSettingValues = Strings.Split("DatabaseType,DatabaseName,DatabaseAuth,DatabaseUsername,DatabasePassword,MailServer,RootPageId,BaseUrl,SiteXsl,ImageRootPath,DocRootPath,MediaRootPath,Membership,MailingList,NonAuthenticatedUsersGroupId,AuthenticatedUsersGroupId,RegisterBehaviour,RegisterRedirectPageId,Cart,Quote,Debug,CompiledTransform,SiteAdminName,SiteAdminEmail,ContentSearch,SiteSearch,SiteSearchPath,Subscriptions,ActivityLogging,IPLogging,GoogleContentTypes,ShowRelatedBriefContentTypes,ShowRelatedBriefDepth,VersionControl,LegacyRedirect,PageURLFormat,AllowContentDetailAccess", ",");
                            long i;
                            XmlElement oElmt;
                            XmlElement oElmtAft = null;
                            var loopTo = (long)Information.UBound(aSettingValues);
                            for (i = 0L; i <= loopTo; i++)
                            {
                                oElmt = (XmlElement)base.Instance.SelectSingleNode("web/add[@key='" + aSettingValues[(int)i] + "']");
                                if (oElmt is null)
                                {
                                    oElmt = this.moPageXML.CreateElement("add");
                                    oElmt.SetAttribute("key", aSettingValues[(int)i]);
                                    oElmt.SetAttribute("value", "");
                                    if (oElmtAft is null)
                                    {
                                        base.Instance.FirstChild.InsertBefore(oElmt, base.Instance.FirstChild.FirstChild);
                                    }
                                    else
                                    {
                                        base.Instance.FirstChild.InsertAfter(oElmt, oElmtAft);
                                    }
                                }

                                oElmtAft = oElmt;
                            }

                            if (base.isSubmitted())
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                if (base.valid)
                                {
                                    oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                    oCgfSect.SectionInformation.SetRawXml(base.Instance.InnerXml);
                                    oCfg.Save();
                                }
                            }

                            oImp.UndoImpersonation();
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmWebSettings", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmWebConfig(string ConfigType)
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;
                    string xFormPath = "/xforms/config/" + ConfigType + ".xml";
                    try
                    {
                        if (this.myWeb.mcEWCommonFolder == "/ptn")
                        {
                            xFormPath = "/admin/xforms/config/" + ConfigType + ".xml";
                        }

                        oFsh = new Protean.fsHelper();
                        oFsh.open(this.moPageXML);
                        base.NewFrm("WebSettings");
                        if (!base.load(xFormPath, this.myWeb.maCommonFolders))
                        {
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Config", "", "ConfigSettings");
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, xFormPath + " could not be found. - ");
                        }
                        else
                        {
                            var oCfg = WebConfigurationManager.OpenWebConfiguration("/" + this.myWeb.moConfig["ProjectPath"]);
                            var oImp = new Tools.Security.Impersonate();
                            if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]))
                            {

                                // code here to replace any missing nodes
                                // all of the required config settings

                                var oTemplateInstance = this.moPageXML.CreateElement("Instance");
                                oTemplateInstance.InnerXml = base.Instance.InnerXml;
                                string oCgfSectName = oTemplateInstance.FirstChild.Name;
                                string oCgfSectPath = "protean/" + oCgfSectName;
                                DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection(oCgfSectPath);
                                cProcessInfo = "Getting Section Name:" + oCgfSectPath;
                                bool sectionMissing = false;

                                // Get the current settings
                                if (!string.IsNullOrEmpty(oCgfSect.SectionInformation.GetRawXml()))
                                {
                                    base.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml();
                                }
                                else
                                {
                                    // no current settings create them
                                    sectionMissing = true;
                                    oFrmElmt = base.moXformElmt;
                                    XmlNode argoNode1 = oFrmElmt;
                                    base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "This config section has not yet been setup, saving will implement these settings for the first time and then log you off the admin system.");
                                }

                                XmlElement oElmt;
                                string Key;
                                string ConfigSectionName;
                                NameValueCollection ConfigSection;
                                foreach (XmlElement oTemplateElmt in oTemplateInstance.SelectNodes("*/add"))
                                {
                                    ConfigSectionName = oTemplateElmt.ParentNode.Name;
                                    ConfigSection = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/" + ConfigSectionName);
                                    Key = oTemplateElmt.GetAttribute("key");
                                    oElmt = (XmlElement)base.Instance.SelectSingleNode(ConfigSectionName + "/add[@key='" + Key + "']");
                                    // lets not write an empty value if inherited from machine level web.config
                                    if (!(!string.IsNullOrEmpty(ConfigSection[Key]) & string.IsNullOrEmpty(oTemplateElmt.GetAttribute("value"))))
                                    {
                                        if (oElmt is null)
                                        {
                                            oElmt = this.moPageXML.CreateElement("add");
                                            oElmt.SetAttribute("key", Key);
                                            oElmt.SetAttribute("value", oTemplateElmt.GetAttribute("value"));
                                            base.Instance.SelectSingleNode(ConfigSectionName).AppendChild(oElmt);
                                        }
                                    }
                                }

                                if (base.isSubmitted())
                                {
                                    base.updateInstanceFromRequest();
                                    base.validate();
                                    if (base.valid)
                                    {
                                        if (sectionMissing)
                                        {
                                            // update config based on form submission
                                            var oNewCfgXml = new XmlDocument();
                                            oNewCfgXml.LoadXml(base.Instance.InnerXml);
                                            // save as web.config in the root
                                            oNewCfgXml.Save(this.goServer.MapPath(@"\" + Strings.Replace(oCgfSectPath, "/", ".") + ".config"));
                                            var oMainCfgXml = new XmlDocument();
                                            // update the the web.config to include new file
                                            cProcessInfo = "loading file:" + this.goServer.MapPath("/web.config");
                                            oMainCfgXml.Load(this.goServer.MapPath("/web.config"));
                                            XmlElement oElmtEonic = (XmlElement)oMainCfgXml.SelectSingleNode("configuration/protean");
                                            var oNewElmt = oMainCfgXml.CreateElement(oCgfSectName);
                                            oNewElmt.SetAttribute("configSource", Strings.Replace(oCgfSectPath, "/", ".") + ".config");
                                            oElmtEonic.AppendChild(oNewElmt);
                                            oMainCfgXml.Save(this.goServer.MapPath("/web.config"));
                                            this.myWeb.msRedirectOnEnd = "/";
                                        }
                                        else
                                        {
                                            // check not read only
                                            var oFileInfo = new FileInfo(this.goServer.MapPath(@"\" + Strings.Replace(oCgfSectPath, "/", ".") + ".config"));
                                            oFileInfo.IsReadOnly = false;
                                            oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                            oCgfSect.SectionInformation.SetRawXml(base.Instance.InnerXml);
                                            oCfg.Save();
                                            XmlNode argoNode2 = (XmlNode)this.moXformElmt;
                                            base.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, "Settings Saved");
                                        }
                                    }
                                }

                                oImp.UndoImpersonation();
                            }

                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmWebConfig", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmRewriteMaps(string ConfigType)
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;
                    string xFormPath = "/xforms/config/" + ConfigType + ".xml";
                    try
                    {
                        oFsh = new Protean.fsHelper();
                        oFsh.open(this.moPageXML);
                        base.NewFrm("WebSettings");
                        base.bProcessRepeats = false;
                        if (!base.load(xFormPath, this.myWeb.maCommonFolders))
                        {
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Config", "", "ConfigSettings");
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, xFormPath + " could not be found. - ");
                        }
                        else
                        {
                            var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                            var oImp = new Tools.Security.Impersonate();
                            if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]))
                            {

                                // code here to replace any missing nodes
                                // all of the required config settings

                                var rewriteXml = new XmlDocument();
                                rewriteXml.Load(this.goServer.MapPath("/rewriteMaps.config"));
                                var oTemplateInstance = this.moPageXML.CreateElement("Instance");
                                oTemplateInstance.InnerXml = base.Instance.InnerXml;
                                string oCgfSectName = "system.webServer";
                                string oCgfSectPath = "rewriteMaps/rewriteMap[@name='" + ConfigType + "']";
                                // Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection(oCgfSectName)
                                cProcessInfo = "Getting Section Name:" + oCgfSectPath;
                                bool sectionMissing = false;

                                // Get the current settings
                                if (rewriteXml.SelectSingleNode(oCgfSectPath) is object)
                                {
                                    base.bProcessRepeats = true;
                                    if (this.goSession["oTempInstance"] is null)
                                    {
                                        int PerPageCount = 50;
                                        if (this.goSession["totalCountTobeLoad"] is object)
                                        {
                                            PerPageCount = Conversions.ToInteger(this.goSession["totalCountTobeLoad"]);
                                        }

                                        var props = rewriteXml.SelectSingleNode(oCgfSectPath);
                                        int TotalCount = props.ChildNodes.Count;
                                        if (props.ChildNodes.Count >= PerPageCount)
                                        {
                                            string xmlstring = "<rewriteMap name='" + ConfigType + "'>";
                                            string xmlstringend = "</rewriteMap>";
                                            int count = 0;
                                            for (int i = 0, loopTo = PerPageCount - 1; i <= loopTo; i++)
                                                xmlstring = xmlstring + props.ChildNodes[i].OuterXml;
                                            base.LoadInstanceFromInnerXml(xmlstring + xmlstringend);
                                        }
                                        else
                                        {
                                            base.LoadInstanceFromInnerXml(rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml);
                                        }

                                        this.bProcessRepeats = false;
                                    }
                                    else
                                    {
                                        var oTempInstance = this.moPageXML.CreateElement("instance");
                                        oTempInstance = (XmlElement)this.goSession["oTempInstance"];
                                        base.updateInstance(oTempInstance);
                                    }
                                }
                                else
                                {
                                    // no current settings create them
                                    sectionMissing = true;
                                    oFrmElmt = base.moXformElmt;
                                    XmlNode argoNode1 = oFrmElmt;
                                    base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "This config section has not yet been setup, saving will implement these settings for the first time and then log you off the admin system.");
                                }

                                XmlElement oElmt;
                                if (base.isSubmitted())
                                {
                                    base.updateInstanceFromRequest();
                                    base.validate();
                                    // Check for loop
                                    foreach (XmlElement currentOElmt in base.Instance.FirstChild.SelectNodes("descendant-or-self::add"))
                                    {
                                        oElmt = currentOElmt;
                                        object newURL = oElmt.GetAttribute("value");
                                        if (base.Instance.FirstChild.SelectSingleNode(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("descendant-or-self::add[@key='", newURL), "']"))) is object)
                                        {
                                            base.valid = false;
                                            XmlElement argoContextNode = (XmlElement)this.moXformElmt.SelectSingleNode("group[1]");
                                            var alertGrp = base.addGroup(ref argoContextNode, "alert", oInsertBeforeNode: (XmlElement)this.moXformElmt.SelectSingleNode("group[1]/group[1]"));
                                            XmlNode argoNode2 = (XmlNode)alertGrp;
                                            base.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("<strong>", newURL), "</strong> cannot match an old URL")));
                                        }
                                    }

                                    if (base.valid)
                                    {
                                        XmlElement replacerNode = (XmlElement)rewriteXml.ImportNode(base.Instance.FirstChild, true);
                                        var folderRules = new ArrayList();
                                        if (ConfigType == "301Redirect")
                                        {
                                            // step through and create rules to deal with paths
                                            var rulesXml = new XmlDocument();
                                            rulesXml.Load(this.myWeb.goServer.MapPath("/RewriteRules.config"));
                                            XmlElement insertAfterElment = (XmlElement)rulesXml.SelectSingleNode("descendant-or-self::rule[@name='EW: 301 Redirects']");
                                            XmlElement oRule;
                                            foreach (XmlElement currentORule in replacerNode.SelectNodes("add"))
                                            {
                                                oRule = currentORule;
                                                XmlElement CurrentRule = (XmlElement)rulesXml.SelectSingleNode("descendant-or-self::rule[@name='Folder: " + oRule.GetAttribute("key") + "']");
                                                var newRule = rulesXml.CreateElement("newRule");
                                                string matchString = oRule.GetAttribute("key");
                                                if (matchString.StartsWith("/"))
                                                {
                                                    matchString = matchString.TrimStart('/');
                                                }

                                                folderRules.Add("Folder: " + oRule.GetAttribute("key"));
                                                newRule.InnerXml = "<rule name=\"Folder: " + oRule.GetAttribute("key") + "\"><match url=\"^" + matchString + "(.*)\"/><action type=\"Redirect\" url=\"" + oRule.GetAttribute("value") + "{R:1}\" /></rule>";
                                                if (CurrentRule is null)
                                                {
                                                    insertAfterElment.ParentNode.InsertAfter(newRule.FirstChild, insertAfterElment);
                                                }
                                                else
                                                {
                                                    CurrentRule.ParentNode.ReplaceChild(newRule.FirstChild, CurrentRule);
                                                }
                                            }

                                            foreach (XmlElement currentORule1 in rulesXml.SelectNodes("descendant-or-self::rule[starts-with(@name,'Folder: ')]"))
                                            {
                                                oRule = currentORule1;
                                                if (!folderRules.Contains(oRule.GetAttribute("name")))
                                                {
                                                    oRule.ParentNode.RemoveChild(oRule);
                                                }
                                            }

                                            rulesXml.Save(this.goServer.MapPath("/RewriteRules.config"));
                                            this.myWeb.bRestartApp = true;
                                        }

                                        // Dim replacingNode As XmlElement = rewriteXml.SelectSingleNode(oCgfSectPath)
                                        // If replacingNode Is Nothing Then
                                        // rewriteXml.FirstChild.AppendChild(replacerNode)
                                        // Else
                                        // rewriteXml.FirstChild.ReplaceChild(replacerNode, replacingNode)
                                        // End If
                                        // rewriteXml.Save(goServer.MapPath("/rewriteMaps.config"))

                                        // 'Check we do not have a redirect for the OLD URL allready. Remove if exists
                                        // Dim addValue As XmlElement


                                        foreach (XmlElement currentOElmt1 in base.Instance.FirstChild.SelectNodes("descendant-or-self::add"))
                                        {
                                            oElmt = currentOElmt1;
                                            object oldUrl = oElmt.GetAttribute("key");

                                            // If Not MyBase.Instance.FirstChild.SelectSingleNode("descendant-or-self::add[@key='" & newURL & "']") Is Nothing Then
                                            var existingRedirects = rewriteXml.SelectNodes(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("rewriteMaps/rewriteMap[@name='" + ConfigType + "']/add[@key='", oldUrl), "']")));
                                            if (existingRedirects is object)
                                            {
                                                foreach (XmlNode existingNode in existingRedirects)
                                                {
                                                    existingNode.ParentNode.RemoveChild(existingNode);
                                                    // existingNode.RemoveAll()
                                                    rewriteXml.Save(this.myWeb.goServer.MapPath("/rewriteMaps.config"));
                                                }
                                            }
                                        }

                                        // Add redirect
                                        string oCgfSectPathobj = "rewriteMaps/rewriteMap[@name='" + ConfigType + "']";
                                        var redirectSectionXmlNode = rewriteXml.SelectSingleNode(oCgfSectPathobj);
                                        if (redirectSectionXmlNode is object)
                                        {
                                            foreach (XmlElement currentOElmt2 in base.Instance.FirstChild.SelectNodes("descendant-or-self::add"))
                                            {
                                                oElmt = currentOElmt2;
                                                var replacingElement = rewriteXml.CreateElement("RedirectInfo");
                                                replacingElement.InnerXml = oElmt.OuterXml;

                                                // rewriteXml.SelectSingleNode(oCgfSectPath).FirstChild.AppendChild(replacingElement.FirstChild)
                                                rewriteXml.SelectSingleNode(oCgfSectPathobj).AppendChild(replacingElement.FirstChild);
                                                rewriteXml.Save(this.myWeb.goServer.MapPath("/rewriteMaps.config"));
                                            }
                                        }

                                        XmlElement argoContextNode1 = (XmlElement)this.moXformElmt.SelectSingleNode("group[1]");
                                        var alertGrp = base.addGroup(ref argoContextNode1, "alert", oInsertBeforeNode: (XmlElement)this.moXformElmt.SelectSingleNode("group[1]/group[1]"));
                                        XmlNode argoNode3 = (XmlNode)alertGrp;
                                        base.addNote(ref argoNode3, Protean.xForm.noteTypes.Alert, "Settings Saved");
                                        this.goSession["oTempInstance"] = null;
                                    }
                                }
                                else if (base.isTriggered)
                                {
                                    // we have clicked a trigger so we must update the instance
                                    base.updateInstanceFromRequest();
                                    // lets save the instance
                                    this.goSession["oTempInstance"] = base.Instance;
                                }
                                else
                                {
                                    // clear this if we are loading the first form
                                    this.goSession["oTempInstance"] = null;
                                }

                                oImp.UndoImpersonation();
                            }

                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmWebConfig", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmThemeSettings(string ConfigType)
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;
                    string xFormPath = "/xforms/config/" + ConfigType + ".xml";
                    try
                    {
                        oFsh = new Protean.fsHelper();
                        oFsh.open(this.moPageXML);
                        NameValueCollection moThemeConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/theme");
                        string currentTheme = moThemeConfig["CurrentTheme"];
                        base.NewFrm("WebSettings");
                        if (!base.load(xFormPath, this.myWeb.maCommonFolders))
                        {
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Config", "", "ConfigSettings");
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, xFormPath + " could not be found. - ");
                        }
                        else
                        {
                            var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                            var oImp = new Tools.Security.Impersonate();
                            if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]))
                            {

                                // code here to replace any missing nodes
                                // all of the required config settings

                                var oTemplateInstance = this.moPageXML.CreateElement("Instance");
                                oTemplateInstance.InnerXml = base.Instance.InnerXml;
                                string oCgfSectName = "protean/" + oTemplateInstance.FirstChild.Name;
                                DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection(oCgfSectName);
                                cProcessInfo = "Getting Section Name:" + oCgfSectName;
                                // Get the current settings
                                base.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml();
                                string currentPresetName = "";
                                XmlElement presetSetting = (XmlElement)base.Instance.SelectSingleNode("theme/add[@key='" + currentTheme + ".ThemePreset']");
                                if (presetSetting is object)
                                {
                                    currentPresetName = presetSetting.GetAttribute("value");
                                }

                                if ((this.myWeb.moRequest["ThemePreset"] ?? "") != (currentPresetName ?? "") | string.IsNullOrEmpty(currentPresetName))
                                {
                                    // replace Instance Elements WITH VALUES IN NAMED THEME PRESET FILE.

                                    if (File.Exists(this.goServer.MapPath("/ewthemes/" + currentTheme + "/themeManifest.xml")))
                                    {
                                        var newXml = new XmlDocument();
                                        newXml.PreserveWhitespace = true;
                                        newXml.Load(this.goServer.MapPath("/ewthemes/" + currentTheme + "/themeManifest.xml"));
                                        foreach (XmlElement oElmt2 in newXml.SelectNodes("/Theme/Presets/Preset[@name='" + this.myWeb.moRequest["ThemePreset"] + "']/add"))
                                        {
                                            // <add key="Bootswatch.Layout" value="TopNavSideSub"/>
                                            XmlElement changeElmt = (XmlElement)base.Instance.SelectSingleNode("descendant-or-self::add[@key='" + oElmt2.GetAttribute("key") + "']");
                                            if (changeElmt is object)
                                            {
                                                changeElmt.SetAttribute("value", oElmt2.GetAttribute("value"));
                                            }
                                        }
                                    }
                                }

                                XmlElement oElmt;
                                string Key;
                                string ConfigSectionName;
                                NameValueCollection ConfigSection;
                                foreach (XmlElement oTemplateElmt in oTemplateInstance.SelectNodes("*/add"))
                                {
                                    ConfigSectionName = oTemplateElmt.ParentNode.Name;
                                    ConfigSection = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/" + ConfigSectionName);
                                    Key = oTemplateElmt.GetAttribute("key");
                                    oElmt = (XmlElement)base.Instance.SelectSingleNode(ConfigSectionName + "/add[@key='" + Key + "']");
                                    // lets not write an empty value if inherited from machine level web.config
                                    if (!(!string.IsNullOrEmpty(ConfigSection[Key]) & string.IsNullOrEmpty(oTemplateElmt.GetAttribute("value"))))
                                    {
                                        if (oElmt is null)
                                        {
                                            oElmt = this.moPageXML.CreateElement("add");
                                            oElmt.SetAttribute("key", Key);
                                            oElmt.SetAttribute("value", oTemplateElmt.GetAttribute("value"));
                                            base.Instance.SelectSingleNode(ConfigSectionName).AppendChild(oElmt);
                                        }
                                    }
                                }

                                if (base.isSubmitted())
                                {

                                    // If myWeb.moRequest("ThemePreset") <> currentPresetName And Not (myWeb.moSession("presetView") = "true") Then
                                    // MyBase.valid = False
                                    // myWeb.moSession("presetView") = "true"
                                    // Else
                                    // myWeb.moSession("presetView") = Nothing
                                    // MyBase.updateInstanceFromRequest()
                                    // MyBase.validate()
                                    // End If

                                    base.updateInstanceFromRequest();
                                    base.validate();
                                    if (base.valid)
                                    {

                                        // check not read only
                                        var oFileInfo = new FileInfo(this.goServer.MapPath("/protean.theme.config"));
                                        oFileInfo.IsReadOnly = false;
                                        oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                        oCgfSect.SectionInformation.SetRawXml(base.Instance.InnerXml);
                                        oCfg.Save();
                                        if (!string.IsNullOrEmpty(this.myWeb.moRequest["newPresetName"]))
                                        {
                                            if (File.Exists(this.goServer.MapPath("/ewthemes/" + currentTheme + "/themeManifest.xml")))
                                            {
                                                var newXml = new XmlDocument();
                                                newXml.PreserveWhitespace = true;
                                                newXml.Load(this.goServer.MapPath("/ewthemes/" + currentTheme + "/themeManifest.xml"));
                                                bool addNew = true;
                                                // update existing
                                                foreach (XmlElement oElmt2 in newXml.SelectNodes("/Theme/Presets/Preset[@name='" + this.myWeb.moRequest["newPresetName"] + "']"))
                                                {
                                                    oElmt2.InnerXml = this.Instance.InnerXml;
                                                    addNew = false;
                                                }

                                                if (addNew)
                                                {
                                                    XmlElement PresetsNode = (XmlElement)newXml.SelectSingleNode("/Theme/Presets");
                                                    var NewPreset = PresetsNode.OwnerDocument.CreateElement("Preset");
                                                    NewPreset.SetAttribute("name", this.myWeb.moRequest["newPresetName"]);
                                                    foreach (XmlElement matchingElmt in this.Instance.SelectNodes("descendant-or-self::add[starts-with(@key,'" + currentTheme + ".')]"))
                                                    {
                                                        if ((matchingElmt.GetAttribute("key") ?? "") == (currentTheme + ".ThemePreset" ?? ""))
                                                        {
                                                            matchingElmt.SetAttribute("value", this.myWeb.moRequest["newPresetName"]);
                                                        }

                                                        AddExistingNode(ref NewPreset, matchingElmt);
                                                    }

                                                    PresetsNode.AppendChild(NewPreset);
                                                }
                                                // check not read only
                                                var oFileInfo2 = new FileInfo(this.goServer.MapPath("/ewthemes/" + currentTheme + "/themeManifest.xml"));
                                                oFileInfo2.IsReadOnly = false;
                                                newXml.Save(this.goServer.MapPath("/ewthemes/" + currentTheme + "/themeManifest.xml"));
                                            }

                                            XmlNode argoNode1 = (XmlNode)this.moXformElmt;
                                            base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "New Preset Saved");
                                        }
                                        else
                                        {
                                            XmlNode argoNode2 = (XmlNode)this.moXformElmt;
                                            base.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, "Settings Saved");
                                        }
                                    }
                                    else
                                    {
                                        XmlNode argoNode3 = (XmlNode)this.moXformElmt;
                                        base.addNote(ref argoNode3, Protean.xForm.noteTypes.Alert, "Form Invalid:" + base.validationError);
                                    }
                                }

                                oImp.UndoImpersonation();
                            }

                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmWebConfig", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmSelectTheme()
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;
                    try
                    {
                        oFsh = new Protean.fsHelper();
                        oFsh.open(this.moPageXML);
                        base.NewFrm("WebSettings");
                        // MyBase.Instance.InnerXml = "<web><add key=""SiteXsl"" value="""" /></web><theme><add key=""CurrentTheme"" value="""" /></theme>"
                        base.submission("WebSettings", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "WebSettings", "", "Select Theme");
                        var rootdir = new DirectoryInfo(this.goServer.MapPath("/ewThemes"));
                        if (!rootdir.Exists)
                        {
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "This site is not configured to allow new themes to be selected.");
                        }
                        else
                        {
                            XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "Any Changes you make to this form risk making this site non-functional. Please be sure you know what you are doing before making any changes.");
                            oSelElmt = base.addSelect1(ref oFrmElmt, "ewSiteTheme", true, "Site Theme", "PickByImage", Protean.xForm.ApperanceTypes.Full);
                            EnumberateThemeOptions(ref oSelElmt, "/ewThemes", ".xsl", "", true);
                            // MyBase.addOption(oSelElmt, "standard.xsl [bespoke]", "/xsl/standard.xsl")
                            XmlElement argoBindParent = null;
                            base.addBind("ewSiteTheme", "theme/add[@key='CurrentTheme']/@value", "false()", oBindParent: ref argoBindParent);

                            // MyBase.addSubmit(oFrmElmt, "", "Save Settings")

                            var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                            DefaultSection oWebCgfSect = (DefaultSection)oCfg.GetSection("protean/web");
                            DefaultSection oThemeCgfSect = (DefaultSection)oCfg.GetSection("protean/theme");
                            var oImp = new Tools.Security.Impersonate();
                            if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]))
                            {
                                base.Instance.InnerXml = oWebCgfSect.SectionInformation.GetRawXml() + oThemeCgfSect.SectionInformation.GetRawXml();
                                var oTemplateInstance = this.moPageXML.CreateElement("Instance");
                                oTemplateInstance.InnerXml = base.Instance.InnerXml;
                                if (base.isSubmitted() | !string.IsNullOrEmpty(this.goRequest.Form["ewsubmit.x"]) | !string.IsNullOrEmpty(this.goRequest.Form["ewSiteTheme"]))
                                {
                                    XmlElement oElmt;
                                    string Key;
                                    string ConfigSectionName;
                                    NameValueCollection ConfigSection;
                                    foreach (XmlElement oTemplateElmt in oTemplateInstance.SelectNodes("*/add"))
                                    {
                                        ConfigSectionName = oTemplateElmt.ParentNode.Name;
                                        ConfigSection = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/" + ConfigSectionName);
                                        Key = oTemplateElmt.GetAttribute("key");
                                        oElmt = (XmlElement)base.Instance.SelectSingleNode(ConfigSectionName + "/add[@key='" + Key + "']");
                                        // lets not write an empty value if inherited from machine level web.config
                                        if (!(!string.IsNullOrEmpty(ConfigSection[Key]) & string.IsNullOrEmpty(oTemplateElmt.GetAttribute("value"))))
                                        {
                                            if (oElmt is null)
                                            {
                                                oElmt = this.moPageXML.CreateElement("add");
                                                oElmt.SetAttribute("key", Key);
                                                oElmt.SetAttribute("value", oTemplateElmt.GetAttribute("value"));
                                                base.Instance.SelectSingleNode(ConfigSectionName).AppendChild(oElmt);
                                            }
                                        }
                                    }

                                    base.updateInstanceFromRequest();
                                    base.validate();
                                    if (base.valid)
                                    {
                                        XmlElement updElmt = (XmlElement)base.Instance.SelectSingleNode("web/add[@key='SiteXsl']");
                                        updElmt.SetAttribute("value", "/ewthemes/" + moRequest["ewSiteTheme"] + "/standard.xsl");
                                        string cssFramework = "";
                                        if (oSelElmt.SelectSingleNode("descendant-or-self::Theme[@name='" + moRequest["ewSiteTheme"] + "' and @cssFramework!='']") is object)
                                        {
                                            XmlElement themeElmt = (XmlElement)oSelElmt.SelectSingleNode("descendant-or-self::Theme[@name='" + moRequest["ewSiteTheme"] + "' and @cssFramework!='']");
                                            cssFramework = themeElmt.GetAttribute("cssFramework");
                                        }

                                        XmlElement cssElmt = (XmlElement)base.Instance.SelectSingleNode("web/add[@key='cssFramework']");
                                        if (cssElmt is null)
                                        {
                                            oElmt = this.moPageXML.CreateElement("add");
                                            oElmt.SetAttribute("key", "cssFramework");
                                            oElmt.SetAttribute("value", cssFramework);
                                            base.Instance.SelectSingleNode("web").AppendChild(oElmt);
                                        }
                                        else
                                        {
                                            cssElmt.SetAttribute("value", cssFramework);
                                        }

                                        oWebCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                        oWebCgfSect.SectionInformation.SetRawXml(base.Instance.SelectSingleNode("web").OuterXml);
                                        oThemeCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                        oThemeCgfSect.SectionInformation.SetRawXml(base.Instance.SelectSingleNode("theme").OuterXml);
                                        oCfg.Save();
                                        XmlNode argoNode2 = (XmlNode)this.moXformElmt;
                                        base.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, "Settings Saved");
                                    }
                                }

                                oImp.UndoImpersonation();
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmSelectTheme", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                private void EnumberateThemeOptions(ref XmlElement oSelectElmt, string filepath, string groupName, string optionName, bool bIgnoreIfNotFound)
                {
                    var oXformDoc = new XmlDocument();
                    string cProcessInfo = "";
                    string sImgPath = "";
                    XmlElement oOptElmt;
                    try
                    {
                        if (string.IsNullOrEmpty(filepath))
                            filepath = "/";

                        // for each folder found in ewskins

                        var rootdir = new DirectoryInfo(this.goServer.MapPath(filepath));
                        var dir = rootdir.GetDirectories();
                        FileInfo[] files;
                        var oChoicesElmt = base.addChoices(ref oSelectElmt, "Installed Themes");
                        foreach (var di in dir)
                        {
                            files = di.GetFiles("themeManifest.xml");
                            foreach (var fi in files)
                            {
                                cProcessInfo = "loading File:" + this.goServer.MapPath(filepath) + @"\" + di.Name + @"\" + fi.Name;
                                oXformDoc.Load(this.goServer.MapPath(filepath) + @"\" + di.Name + @"\" + fi.Name);
                                foreach (XmlElement oChoices in oXformDoc.SelectNodes("/Theme"))
                                {
                                    XmlElement RootXsltElmt = (XmlElement)oChoices.SelectSingleNode("/Theme/RootXslt");
                                    if (RootXsltElmt is object)
                                    {
                                        oOptElmt = base.addOption(ref oChoicesElmt, oChoices.GetAttribute("name"), RootXsltElmt.GetAttribute("src"));
                                        oOptElmt.FirstChild.InnerXml = oChoices.OuterXml;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "EnumberateThemeOptions", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                    }
                }

                public XmlElement xFrmEditPage(long pgid = 0L, string cName = "", string cFormName = "Page", string cParId = "")
                {
                    string cXmlFilePath;
                    XmlElement oFrmElmt = null;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    var oObjType = new Cms.dbHelper.objectTypes();
                    var oTblName = default(Cms.dbHelper.TableNames);
                    int nRContentId = 0;
                    try
                    {
                        cXmlFilePath = "/xforms/page/" + cFormName + ".xml";
                        if (goConfig["cssFramework"] == "bs5")
                        {
                            cXmlFilePath = "/admin" + cXmlFilePath;
                        }

                        if (!base.load(cXmlFilePath, this.myWeb.maCommonFolders))
                        {
                            // If not a custom page is loaded, pull in the standard elements
                            base.NewFrm("EditPage");
                            // MyBase.submission("EditEage", "admin.aspx?ewCmd=EditPage&pgid=46&xml=x", "post")

                            base.submission("EditEage", "", "post", "form_check(this)");
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPage", "", "Edit Page");
                            base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                            XmlElement argoBindParent = null;
                            base.addBind("nStructParId", "tblContentStructure/nStructParId", "true()", oBindParent: ref argoBindParent);
                            base.addInput(ref oFrmElmt, "cStructName", true, "Page Name", Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(cName), "", "readonly")));
                            XmlElement argoBindParent1 = null;
                            base.addBind("cStructName", "tblContentStructure/cStructName", "true()", oBindParent: ref argoBindParent1);
                            base.addInput(ref oFrmElmt, "cDisplayName", true, "Display Name");
                            XmlElement argoBindParent2 = null;
                            base.addBind("cDisplayName", "tblContentStructure/cStructDescription/DisplayName", "false()", oBindParent: ref argoBindParent2);
                            string argsClass = "xhtml";
                            int argnRows = 10;
                            int argnCols = 0;
                            base.addTextArea(ref oFrmElmt, "cStructDescription", true, "Description", ref argsClass, ref argnRows, nCols: ref argnCols);
                            XmlElement argoBindParent3 = null;
                            base.addBind("cStructDescription", "tblContentStructure/cStructDescription/Description", "false()", oBindParent: ref argoBindParent3);
                            if (Strings.LCase(this.myWeb.moConfig["ShowStructForiegnRef"]) == "yes" | Strings.LCase(this.myWeb.moConfig["ShowStructForiegnRef"]) == "on")
                            {
                                base.addInput(ref oFrmElmt, "cStructForiegnRef", true, "Foriegn Reference");
                                XmlElement argoBindParent4 = null;
                                base.addBind("cStructForiegnRef", "tblContentStructure/cStructForiegnRef", "false()", oBindParent: ref argoBindParent4);
                            }

                            if (this.myWeb.goLangConfig is object)
                            {
                                oSelElmt = base.addSelect1(ref oFrmElmt, "cLang", true, "Language", "", Protean.xForm.ApperanceTypes.Full);
                                base.addOption(ref oSelElmt, this.myWeb.goLangConfig.GetAttribute("default"), this.myWeb.goLangConfig.GetAttribute("code"));
                                foreach (XmlElement langNode in this.myWeb.goLangConfig.SelectNodes("Language"))
                                    base.addOption(ref oSelElmt, langNode.GetAttribute("systemName"), langNode.GetAttribute("code"));
                                XmlElement argoBindParent5 = null;
                                base.addBind("cLang", "tblContentStructure/cVersionLang", "tblContentStructure/nVersionType='3'", oBindParent: ref argoBindParent5);
                            }

                            base.addInput(ref oFrmElmt, "thumbnail", true, "Thumbnail Image", "short pickImage");
                            XmlElement argoBindParent6 = null;
                            base.addBind("thumbnail", "tblContentStructure/cStructDescription/Images/img[@class='thumbnail']", "false()", "xml-replace", oBindParent: ref argoBindParent6);
                            base.addInput(ref oFrmElmt, "cUrl", true, "URL");
                            XmlElement argoBindParent7 = null;
                            base.addBind("cUrl", "tblContentStructure/cUrl", "false()", oBindParent: ref argoBindParent7);
                            base.addInput(ref oFrmElmt, "dPublishDate", true, "Publish Date", "calendar");
                            XmlElement argoBindParent8 = null;
                            base.addBind("dPublishDate", "tblContentStructure/dPublishDate", "false()", oBindParent: ref argoBindParent8);
                            base.addInput(ref oFrmElmt, "dExpireDate", true, "Expire Date", "calendar");
                            XmlElement argoBindParent9 = null;
                            base.addBind("dExpireDate", "tblContentStructure/dExpireDate", "false()", oBindParent: ref argoBindParent9);
                            oSelElmt = base.addSelect1(ref oFrmElmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt, "Live", 1.ToString());
                            base.addOption(ref oSelElmt, "Hidden", 0.ToString());
                            XmlElement argoBindParent10 = null;
                            base.addBind("nStatus", "tblContentStructure/nStatus", "true()", oBindParent: ref argoBindParent10);
                            base.addInput(ref oFrmElmt, "cDescription", true, "Change Notes");
                            XmlElement argoBindParent11 = null;
                            base.addBind("cDescription", "tblContentStructure/cDescription", "false()", oBindParent: ref argoBindParent11);
                            base.addSubmit(ref oFrmElmt, "", "Save Page");
                        }

                        // As this is only needed for Related Content Pages
                        XmlNodeState localNodeState() { var argoNode = base.Instance; var ret = Xml.NodeState(ref argoNode, "tblContentStructure/RelatedContent"); base.Instance = argoNode; return ret; }

                        if (localNodeState() == XmlNodeState.HasContents)
                        {
                            foreach (Cms.dbHelper.TableNames currentOTblName in Enum.GetValues(typeof(Cms.dbHelper.objectTypes)))
                            {
                                oTblName = currentOTblName;
                                if (oTblName.ToString() == "tblContent")
                                    break;
                            }

                            oObjType = (Cms.dbHelper.objectTypes)oTblName;
                        }

                        if (pgid > 0L)
                        {
                            XmlNodeState localNodeState1() { var argoNode = base.Instance; var ret = Xml.NodeState(ref argoNode, "tblContentStructure/RelatedContent"); base.Instance = argoNode; return ret; }

                            if (localNodeState1() == XmlNodeState.HasContents)
                            {
                                XmlNode oContent;
                                SqlDataReader oDr;

                                // For Each oContent In MyBase.Instance.SelectNodes("tblContentStructure/RelatedContent/tblContent")
                                string sSql = "Select nContentKey from tblContent c Inner Join tblContentLocation cl on c.nContentKey = cl.nContentId Where cl.nStructId = '" + pgid + "' AND c.cContentName = '" + cFormName + "_RelatedContent'";
                                oDr = moDbHelper.getDataReader(sSql);

                                // nRContentId = 0
                                while (oDr.Read())
                                    nRContentId = Conversions.ToInteger(oDr[0]);
                                oDr.Close();
                                if (nRContentId > 0)
                                {
                                    oContent = base.Instance.SelectSingleNode("tblContentStructure/RelatedContent");
                                    oContent.InnerXml = moDbHelper.getObjectInstance(oObjType, nRContentId);
                                }
                            }
                        }
                        else
                        {
                            // Set the default language

                        }

                        if (pgid > 0L)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, pgid);

                            // Set the default language if empty
                            if (this.myWeb.goLangConfig is object)
                            {
                                if (base.Instance.SelectSingleNode("tblContentStructure/cVersionLang") is object)
                                {
                                    if (string.IsNullOrEmpty(base.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText))
                                    {
                                        base.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = this.myWeb.goLangConfig.GetAttribute("code");
                                    }
                                }
                            }
                        }
                        else if (string.IsNullOrEmpty(base.Instance.InnerXml))
                        {
                            base.Instance.InnerXml = "<tblContentStructure><nStructKey/><nStructParId/><cStructForiegnRef/><cStructName/><cStructDescription><DisplayName/><Images><img class=\"thumbnail\"/></Images><Description/></cStructDescription><cUrl/><nStructOrder/><cStructLayout>1_Column</cStructLayout><cVersionLang/><nAuditId/>" + "<nAuditKey/><dPublishDate/><dExpireDate/><dInsertDate/><nInsertDirId/><dUpdateDate/><nUpdateDirId/><nStatus>0</nStatus><cDescription></cDescription></tblContentStructure>";
                        }
                        else if (this.myWeb.goLangConfig is object)
                        {
                            base.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = this.myWeb.goLangConfig.GetAttribute("code");
                        }

                        // Add the page name if passed through
                        if (!string.IsNullOrEmpty(cName))
                        {
                            cProcessInfo = base.Instance.InnerXml;
                            base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText = cName;
                        }

                        // delete the status if we are editing the home page

                        if ((double)pgid == Conversions.ToDouble(this.myWeb.moConfig["RootPageId"]))
                        {
                            XmlElement oStatusElmt;
                            oStatusElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::*[@bind='nStatus' or @ref='nStatus']");
                            if (oStatusElmt is object)
                            {
                                oStatusElmt.ParentNode.RemoveChild(oStatusElmt);
                            }
                        }

                        // If the Par Id is empty then populate it
                        if (string.IsNullOrEmpty(cParId))
                        {
                            var argoNode = base.Instance;
                            Xml.NodeState(ref argoNode, "tblContentStructure/nStructParId", Conversions.ToString(Interaction.IIf(base.goRequest["parId"] is null, "", base.goRequest["parId"])));
                            base.Instance = argoNode;
                        }
                        else
                        {
                            var argoNode1 = base.Instance;
                            Xml.NodeState(ref argoNode1, "tblContentStructure/nStructParId", cParId);
                            base.Instance = argoNode1;
                        }


                        // Account for the clone node 
                        if (Cms.gbClone)
                        {

                            // Check for the instance of Clone
                            XmlNodeState localNodeState2() { var argoNode = base.Instance; var ret = Xml.NodeState(ref argoNode, "tblContentStructure/nCloneStructId"); base.Instance = argoNode; return ret; }

                            if (localNodeState2() == XmlNodeState.NotInstantiated)
                            {
                                var argoParent = base.Instance.SelectSingleNode("tblContentStructure");
                                XmlNode argoNodeFromXPath = null;
                                Protean.xmlTools.addElement(ref argoParent, "nCloneStructId", oNodeFromXPath: ref argoNodeFromXPath);
                            }

                            // Check for the binding of clone
                            if (Xml.NodeState(ref base.model, "//bind[contains(@nodeset,'nCloneStructId'])") == XmlNodeState.NotInstantiated)
                            {
                                XmlElement oGroup = (XmlElement)base.moXformElmt.SelectSingleNode("group");
                                base.addInput(ref oGroup, "nCloneStructId", true, "Clone Page", "clonepage");
                                XmlElement argoBindParent12 = null;
                                base.addBind("nCloneStructId", "tblContentStructure/nCloneStructId", "false()", oBindParent: ref argoBindParent12);
                            }
                        }

                        cName = base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText;
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                PageValidation();
                            }

                            if (base.valid)
                            {

                                // TS reset system application page values
                                this.goApp["PageNotFoundId"] = null;
                                this.goApp["PageAccessDeniedId"] = null;
                                this.goApp["PageLoginRequiredId"] = null;
                                this.goApp["PageLoginRequiredId"] = null;

                                // NB Notes: Extract RelatedContent Nodes here - is this old now?

                                if (pgid > 0L)
                                {
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, base.Instance);
                                }
                                else
                                {
                                    pgid = moDbHelper.insertStructure(base.Instance);
                                    moDbHelper.ReorderNode(Cms.dbHelper.objectTypes.ContentStructure, pgid, "MoveBottom");

                                    // If the site wants to, by default, restrict new pages to a given group or directory item, then
                                    // read this in from the config and set the permission.
                                    if (Information.IsNumeric(goConfig["DefaultPagePermissionGroupId"]) & Conversions.ToDouble(goConfig["DefaultPagePermissionGroupId"]) > 0d)
                                    {
                                        long nDefaultPagePermDirId = Conversions.ToLong(goConfig["DefaultPagePermissionGroupId"]);
                                        moDbHelper.maintainPermission(pgid, nDefaultPagePermDirId, ((int)Cms.dbHelper.PermissionLevel.View).ToString());
                                    }

                                    // We need to return the page id somehow, so we could update the instance
                                    var argoNode2 = base.Instance;
                                    Xml.NodeState(ref argoNode2, "//nStructKey", pgid.ToString(), populateState: XmlNodeState.IsEmpty);
                                    base.Instance = argoNode2;
                                }


                                // Clear the cache
                                if (Cms.gbSiteCacheMode)
                                {
                                    moDbHelper.ExeProcessSqlScalar("DELETE FROM dbo.tblXmlCache");
                                }


                                // NB Notes: Get PgId above then process Related Content
                                XmlNodeState localNodeState3() { var argoNode = base.Instance; var ret = Xml.NodeState(ref argoNode, "tblContentStructure/RelatedContent"); base.Instance = argoNode; return ret; }

                                if (localNodeState3() == XmlNodeState.HasContents)
                                {
                                    if (pgid > 0L)
                                    {
                                        XmlNode oContent;
                                        SqlDataReader oDr;
                                        oContent = base.Instance.SelectSingleNode("tblContentStructure/RelatedContent/tblContent");
                                        string sSql = "Select nContentKey from tblContent c Inner Join tblContentLocation cl on c.nContentKey = cl.nContentId Where cl.nStructId = '" + pgid + "' AND c.cContentName = '" + cFormName + "_RelatedContent'";
                                        oDr = moDbHelper.getDataReader(sSql);
                                        var oInstance = new XmlDocument();
                                        oInstance.AppendChild(oInstance.CreateElement("Instance"));
                                        oInstance.FirstChild.AppendChild(oInstance.ImportNode(oContent, true));
                                        nRContentId = 0;
                                        while (oDr.Read())
                                            nRContentId = Conversions.ToInteger(oDr[0]);
                                        oDr.Close();
                                        if (nRContentId > 0)
                                        {
                                            nRContentId = Conversions.ToInteger(moDbHelper.setObjectInstance(oObjType, (XmlElement)oInstance.FirstChild, (long)nRContentId));
                                            moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentEdited, this.myWeb.mnUserId, this.myWeb.moSession.SessionID, DateAndTime.Now, nRContentId, (int)pgid, "");
                                            moDbHelper.setContentLocation(pgid, nRContentId);
                                        }
                                        else
                                        {
                                            nRContentId = Conversions.ToInteger(moDbHelper.setObjectInstance(oObjType, (XmlElement)oInstance.FirstChild));
                                            moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentAdded, this.myWeb.mnUserId, this.myWeb.moSession.SessionID, DateAndTime.Now, nRContentId, (int)pgid, "");
                                            moDbHelper.setContentLocation(pgid, nRContentId);
                                        }
                                    }
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditPage", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                /// <summary>
            /// Page xform validation:
            /// <list>
            ///   <item>Checks for reserved words</item>
            ///   <item>Checks for illegal characters</item>
            /// </list>
            /// </summary>
            /// <remarks>This can be overridden, but should be called by the overriding method.</remarks>
                protected virtual void PageValidation()
                {
                    string cProcessInfo = "";
                    try
                    {
                        cProcessInfo = "Check for reserved words";
                        var aReservedDirs = Strings.Split("ewcommon,images,docs,media,css,bin,js,xforms,xsl", ",");
                        int i;
                        var loopTo = Information.UBound(aReservedDirs);
                        for (i = 0; i <= loopTo; i++)
                        {
                            if ((this.goRequest["cStructName"] ?? "") == (aReservedDirs[i] ?? ""))
                            {
                                base.valid = false;
                                XmlNode argoNode = (XmlNode)base.RootGroup;
                                base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "<strong>" + aReservedDirs[i] + "</strong> is a reserved directory name, please use another.");
                                base.RootGroup = (XmlElement)argoNode;
                            }
                        }

                        // check for illegal charactors within the page name.
                        // update 21-May-09 : taken out - and +  as these can confuse our URLs
                        cProcessInfo = "Check for illegal characters";
                        var oUrlExp = new Regex(@"^[\w\u0020]+$");
                        if (!oUrlExp.IsMatch(this.goRequest["cStructName"]))
                        {
                            base.valid = false;
                            base.addNote("cStructName", Protean.xForm.noteTypes.Alert, "Page names are used for the URL and only contain alphanumberic characters, underscores and spaces.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "PageValidation", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                    }
                }

                public XmlElement xFrmCopyPage(long pgid)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    long nNewPgid;
                    XmlElement oElmt;
                    try
                    {
                        base.NewFrm("CopyPage");
                        // MyBase.submission("EditEage", "admin.aspx?ewCmd=EditPage&pgid=46&xml=x", "post")

                        base.submission("EditEage", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "CopyPage", "", "Copy Page");
                        base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nStructParId", "tblContentStructure/nStructParId", "true()", oBindParent: ref argoBindParent);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "nCopyType", true, "Copy", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "This page and all its decendants", 1.ToString());
                        base.addOption(ref oSelElmt, "This page only", 0.ToString());
                        XmlElement argoBindParent1 = null;
                        base.addBind("nCopyType", "tblContentStructure/@nCopyType", "true()", oBindParent: ref argoBindParent1);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "nCopyContent", true, "Page Content", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Create empty pages", 0.ToString());
                        base.addOption(ref oSelElmt, "Same content with multiple locations", 2.ToString());
                        base.addOption(ref oSelElmt, "Same content with multiple primary locations", 3.ToString());
                        base.addOption(ref oSelElmt, "Create copies of the content", 1.ToString());
                        XmlElement argoBindParent2 = null;
                        base.addBind("nCopyContent", "tblContentStructure/@nCopyContent", "true()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oFrmElmt, "cStructName", true, "Page Name");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cStructName", "tblContentStructure/cStructName", "true()", oBindParent: ref argoBindParent3);
                        base.addInput(ref oFrmElmt, "cDisplayName", true, "Display Name");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cDisplayName", "tblContentStructure/cStructDescription/DisplayName", "false()", oBindParent: ref argoBindParent4);
                        string argsClass = "xhtml";
                        int argnRows = 10;
                        int argnCols = 0;
                        base.addTextArea(ref oFrmElmt, "cStructDescription", true, "Description", ref argsClass, ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent5 = null;
                        base.addBind("cStructDescription", "tblContentStructure/cStructDescription/Description", "false()", oBindParent: ref argoBindParent5);
                        base.addInput(ref oFrmElmt, "cUrl", true, "URL");
                        XmlElement argoBindParent6 = null;
                        base.addBind("cUrl", "tblContentStructure/cUrl", "false()", oBindParent: ref argoBindParent6);
                        base.addInput(ref oFrmElmt, "dPublishDate", true, "Publish Date", "calendar");
                        XmlElement argoBindParent7 = null;
                        base.addBind("dPublishDate", "tblContentStructure/dPublishDate", "false()", oBindParent: ref argoBindParent7);
                        base.addInput(ref oFrmElmt, "dExpireDate", true, "Expire Date", "calendar");
                        XmlElement argoBindParent8 = null;
                        base.addBind("dExpireDate", "tblContentStructure/dExpireDate", "false()", oBindParent: ref argoBindParent8);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Live", 1.ToString());
                        base.addOption(ref oSelElmt, "Hidden", 0.ToString());
                        XmlElement argoBindParent9 = null;
                        base.addBind("nStatus", "tblContentStructure/nStatus", "true()", oBindParent: ref argoBindParent9);
                        base.addInput(ref oFrmElmt, "cDescription", true, "Change Notes");
                        XmlElement argoBindParent10 = null;
                        base.addBind("cDescription", "tblContentStructure/cDescription", "false()", oBindParent: ref argoBindParent10);
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "This page will be copied without any permissions and will inherit the permissions from the new locations ancestors");
                        base.addSubmit(ref oFrmElmt, "", "Save Page");
                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, pgid);

                        // delete old page id so new page is added not updated
                        base.Instance.SelectSingleNode("tblContentStructure/nStructKey").InnerText = "";
                        base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText = "Copy of " + base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText;
                        oElmt = (XmlElement)base.Instance.SelectSingleNode("tblContentStructure");
                        oElmt.SetAttribute("copyPgid", pgid.ToString());
                        oElmt.SetAttribute("nCopyType", "0");
                        oElmt.SetAttribute("nCopyContent", "0");
                        if (base.Instance.SelectSingleNode("tblContentStructure/cStructDescription/DisplayName") is null)
                        {
                            string sDescText = base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml;
                            // make sure the description contains our xml  

                            oElmt = this.moPageXML.CreateElement("DisplayName");
                            base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt);
                            var oElmt2 = this.moPageXML.CreateElement("Description");
                            oElmt2.InnerXml = sDescText;
                            base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt2);
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                var aReservedDirs = Strings.Split("ewcommon,images,docs,media,css,bin,js,xforms,xsl", ",");
                                int i;
                                var loopTo = Information.UBound(aReservedDirs);
                                for (i = 0; i <= loopTo; i++)
                                {
                                    if ((this.goRequest["cStructName"] ?? "") == (aReservedDirs[i] ?? ""))
                                    {
                                        base.valid = false;
                                        XmlNode argoNode1 = oFrmElmt;
                                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "<strong>" + aReservedDirs[i] + "</strong> is a reserved directory name, please use another.");
                                    }
                                }

                                // check for illegal charactors within the page name.
                                var oUrlExp = new Regex(@"^[\w\-\u0020\+]+$");
                                if (!oUrlExp.IsMatch(this.goRequest["cStructName"]))
                                {
                                    base.valid = false;
                                    base.addNote("cStructName", Protean.xForm.noteTypes.Alert, "Page names are used for the URL and only contain Alphanumberic, underscores, hyphens and spaces.");
                                }
                            }

                            if (base.valid)
                            {
                                // add new
                                var dPublishDate = DateTime.Parse("0001-01-01");
                                if (!string.IsNullOrEmpty(base.goRequest["dPublishDate"]))
                                    dPublishDate = Conversions.ToDate(base.goRequest["dPublishDate"]);
                                var dExpireDate = DateTime.Parse("0001-01-01");
                                if (!string.IsNullOrEmpty(base.goRequest["dExpireDate"]))
                                    dExpireDate = Conversions.ToDate(base.goRequest["dExpireDate"]);
                                nNewPgid = moDbHelper.insertStructure(Conversions.ToLong(base.goRequest["nStructParId"]), base.goRequest["nStructForeignRef"], base.goRequest["cStructName"], base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml, base.Instance.SelectSingleNode("tblContentStructure/cStructLayout").InnerXml, Conversions.ToLong(base.goRequest["nStatus"]), dPublishDate, dExpireDate, base.goRequest["cDescription"]);
                                moDbHelper.ReorderNode(Cms.dbHelper.objectTypes.ContentStructure, nNewPgid, "MoveBottom");

                                // Copy content and children
                                // Dim nCopyType As Boolean = False
                                // If Not IsNumeric(goRequest("nCopyType")) Then
                                // nCopyType = False
                                // ElseIf goRequest("nCopyType") = 1 Then
                                // nCopyType = True
                                // Else
                                // nCopyType = False
                                // End If
                                moDbHelper.copyPageContent(pgid, nNewPgid, Conversions.ToBoolean(this.goRequest["nCopyType"]), (Cms.dbHelper.CopyContentType)Conversions.ToInteger(this.goRequest["nCopyContent"]));
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditPage", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCopyPageVersion(long contentPageId, long parentPageId)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    long nNewPgid;
                    XmlElement oElmt;
                    try
                    {
                        base.NewFrm("NewPageVersion");
                        base.submission("EditEage", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "PageVersion", "2col", "New Page Version");
                        // MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                        // MyBase.addBind("nStructParId", "tblContentStructure/nStructParId", "true()")

                        var oGrp1 = base.addGroup(ref oFrmElmt, "Group1", "", "");
                        var oGrp2 = base.addGroup(ref oGrp1, "PageSettings", "", "Page Settings");
                        base.addInput(ref oGrp2, "dPublishDate", true, "Publish Date", "calendar");
                        XmlElement argoBindParent = null;
                        base.addBind("dPublishDate", "tblContentStructure/dPublishDate", "false()", oBindParent: ref argoBindParent);
                        base.addInput(ref oGrp2, "dExpireDate", true, "Expire Date", "calendar");
                        XmlElement argoBindParent1 = null;
                        base.addBind("dExpireDate", "tblContentStructure/dExpireDate", "false()", oBindParent: ref argoBindParent1);
                        oSelElmt = base.addSelect1(ref oGrp2, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Live", 1.ToString());
                        base.addOption(ref oSelElmt, "Hidden", 0.ToString());
                        XmlElement argoBindParent2 = null;
                        base.addBind("nStatus", "tblContentStructure/nStatus", "true()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oGrp2, "cDescription", true, "Change Notes");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cDescription", "tblContentStructure/cDescription", "false()", oBindParent: ref argoBindParent3);
                        var oGrp3 = base.addGroup(ref oFrmElmt, "Group3", "", "");
                        var oGrp4 = base.addGroup(ref oGrp3, "CopySettings", "", "Copy Settings");
                        base.addInput(ref oGrp4, "cVersionDescription", true, "Version Description", "long");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cVersionDescription", "tblContentStructure/cVersionDescription", "true()", oBindParent: ref argoBindParent4);
                        oSelElmt = base.addSelect1(ref oGrp4, "nVersionType", true, "Type", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Personalisation", 1.ToString());
                        // MyBase.addOption(oSelElmt, "Working Copy", 2)
                        if (this.myWeb.goLangConfig is object)
                        {
                            base.addOption(ref oSelElmt, "Language Version", 3.ToString());
                        }
                        // MyBase.addOption(oSelElmt, "Split Test", 4)
                        XmlElement argoBindParent5 = null;
                        base.addBind("nVersionType", "tblContentStructure/nVersionType", "true()", oBindParent: ref argoBindParent5);
                        if (this.myWeb.goLangConfig is object)
                        {
                            oSelElmt = base.addSelect1(ref oGrp4, "cVersionLang", true, "Language", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt, this.myWeb.goLangConfig.GetAttribute("default"), this.myWeb.goLangConfig.GetAttribute("code"));
                            foreach (XmlElement langNode in this.myWeb.goLangConfig.SelectNodes("Language"))
                                base.addOption(ref oSelElmt, langNode.GetAttribute("systemName"), langNode.GetAttribute("code"));
                            XmlElement argoBindParent6 = null;
                            base.addBind("cVersionLang", "tblContentStructure/cVersionLang", "tblContentStructure/nVersionType='3'", oBindParent: ref argoBindParent6);
                        }

                        oSelElmt = base.addSelect1(ref oGrp4, "nCopyContent", true, "Page Content", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Create empty pages", 0.ToString());
                        base.addOption(ref oSelElmt, "Same content with multiple locations", 2.ToString());
                        // MyBase.addOption(oSelElmt, "Same content with multiple primary locations", 3)
                        base.addOption(ref oSelElmt, "Create copies of the content", 1.ToString());
                        XmlElement argoBindParent7 = null;
                        base.addBind("nCopyContent", "tblContentStructure/@nCopyContent", "true()", oBindParent: ref argoBindParent7);
                        var oGrp6 = base.addGroup(ref oGrp3, "PageDetails", "", "Page Details");
                        base.addInput(ref oGrp6, "cStructName", true, "Page Name", "long");
                        XmlElement argoBindParent8 = null;
                        base.addBind("cStructName", "tblContentStructure/cStructName", "true()", oBindParent: ref argoBindParent8);
                        base.addInput(ref oGrp6, "cDisplayName", true, "Display Name", "long");
                        XmlElement argoBindParent9 = null;
                        base.addBind("cDisplayName", "tblContentStructure/cStructDescription/DisplayName", "false()", oBindParent: ref argoBindParent9);
                        string argsClass = "xhtml";
                        int argnRows = 10;
                        int argnCols = 0;
                        base.addTextArea(ref oGrp6, "cStructDescription", true, "Description", ref argsClass, ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent10 = null;
                        base.addBind("cStructDescription", "tblContentStructure/cStructDescription/Description", "false()", oBindParent: ref argoBindParent10);
                        base.addInput(ref oGrp6, "cUrl", true, "URL");
                        XmlElement argoBindParent11 = null;
                        base.addBind("cUrl", "tblContentStructure/cUrl", "false()", oBindParent: ref argoBindParent11);
                        var oGrp5 = base.addGroup(ref base.moXformElmt, "", "", "");
                        XmlNode argoNode = (XmlNode)oGrp5;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "This page will be copied without any permissions and will inherit the permissions from the new locations ancestors");
                        base.addSubmit(ref oGrp5, "", "Save Page");
                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, contentPageId);

                        // delete old page id so new page is added not updated
                        base.Instance.SelectSingleNode("tblContentStructure/nStructKey").InnerText = "";
                        base.Instance.SelectSingleNode("tblContentStructure/cVersionDescription").InnerText = base.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText + " New Version";

                        // set a default for version type
                        base.Instance.SelectSingleNode("tblContentStructure/nVersionType").InnerText = "1";

                        // set a default for lang type
                        if (this.myWeb.goLangConfig is object)
                        {
                            base.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = this.myWeb.goLangConfig.GetAttribute("code");
                        }

                        oElmt = (XmlElement)base.Instance.SelectSingleNode("tblContentStructure");
                        oElmt.SetAttribute("copyPgid", contentPageId.ToString());
                        oElmt.SetAttribute("nCopyType", "0");
                        oElmt.SetAttribute("nCopyContent", "0");
                        if (base.Instance.SelectSingleNode("tblContentStructure/cStructDescription/DisplayName") is null)
                        {
                            string sDescText = base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml;
                            // make sure the description contains our xml  

                            oElmt = this.moPageXML.CreateElement("DisplayName");
                            base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt);
                            var oElmt2 = this.moPageXML.CreateElement("Description");
                            oElmt2.InnerXml = sDescText;
                            base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt2);
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                var aReservedDirs = Strings.Split("ewcommon,images,docs,media,css,bin,js,xforms,xsl", ",");
                                int i;
                                var loopTo = Information.UBound(aReservedDirs);
                                for (i = 0; i <= loopTo; i++)
                                {
                                    if ((this.goRequest["cStructName"] ?? "") == (aReservedDirs[i] ?? ""))
                                    {
                                        base.valid = false;
                                        XmlNode argoNode1 = oFrmElmt;
                                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "<strong>" + aReservedDirs[i] + "</strong> is a reserved directory name, please use another.");
                                    }
                                }

                                // check for illegal charactors within the page name.
                                var oUrlExp = new Regex(@"^[\w\-\u0020\+]+$");
                                if (!oUrlExp.IsMatch(this.goRequest["cStructName"]))
                                {
                                    base.valid = false;
                                    base.addNote("cStructName", Protean.xForm.noteTypes.Alert, "Page names are used for the URL and only contain Alphanumberic, underscores, hyphens and spaces.");
                                }
                            }

                            if (base.valid)
                            {
                                // add new
                                var dPublishDate = DateTime.Parse("0001-01-01");
                                if (!string.IsNullOrEmpty(base.goRequest["dPublishDate"]))
                                    dPublishDate = Conversions.ToDate(base.goRequest["dPublishDate"]);
                                var dExpireDate = DateTime.Parse("0001-01-01");
                                if (!string.IsNullOrEmpty(base.goRequest["dExpireDate"]))
                                    dExpireDate = Conversions.ToDate(base.goRequest["dExpireDate"]);
                                nNewPgid = moDbHelper.insertPageVersion(Conversions.ToLong(base.goRequest["nStructParId"]), base.goRequest["nStructForeignRef"], base.goRequest["cStructName"], base.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml, base.Instance.SelectSingleNode("tblContentStructure/cStructLayout").InnerXml, Conversions.ToLong(base.goRequest["nStatus"]), dPublishDate, dExpireDate, base.goRequest["cDescription"], nVersionParId: Conversions.ToLong(base.goRequest["vParId"]), cVersionLang: base.goRequest["cVersionLang"], cVersionDescription: base.goRequest["cVersionDescription"], nVersionType: (Cms.dbHelper.PageVersionType)Conversions.ToInteger(base.goRequest["nVersionType"]));
                                moDbHelper.ReorderNode(Cms.dbHelper.objectTypes.ContentStructure, nNewPgid, "MoveBottom");

                                // Copy content and children
                                // Dim nCopyType As Boolean = False
                                // If Not IsNumeric(goRequest("nCopyType")) Then
                                // nCopyType = False
                                // ElseIf goRequest("nCopyType") = 1 Then
                                // nCopyType = True
                                // Else
                                // nCopyType = False
                                // End If
                                moDbHelper.copyPageContent(contentPageId, nNewPgid, Conversions.ToBoolean(0), (Cms.dbHelper.CopyContentType)Conversions.ToInteger(this.goRequest["nCopyContent"]));
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmCopyPageVersion", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmEditPageLayout(long pgid = 0L)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oChoices;
                    // Dim oChoicesElmt As XmlElement
                    XmlElement oItem;
                    XmlElement oOptElmt;
                    XmlElement oDescElmt;
                    string sImgPath = "";
                    string cProcessInfo = "";
                    var oXformDoc = new XmlDocument();
                    try
                    {
                        base.NewFrm("EditPageLayout");
                        base.submission("EditEage", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPage", "", "Select Page Layout");
                        base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nStructParId", "tblContentStructure/nStructParId", "true()", oBindParent: ref argoBindParent);

                        // MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select page layout")

                        oSelElmt = base.addSelect1(ref oFrmElmt, "cStructLayout", true, "", "PickByImage", Protean.xForm.ApperanceTypes.Full);
                        XmlElement argoBindParent1 = null;
                        base.addBind("cStructLayout", "tblContentStructure/cStructLayout", "true()", oBindParent: ref argoBindParent1);
                        try
                        {
                            // if this file exists then add the bespoke templates
                            oXformDoc.Load(this.goServer.MapPath(this.myWeb.moConfig["ProjectPath"] + "/xsl") + "/LayoutManifest.xml");
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                            foreach (XmlElement currentOChoices in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                            {
                                oChoices = currentOChoices;
                                var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                                foreach (XmlElement currentOItem in oChoices.SelectNodes("Layout"))
                                {
                                    oItem = currentOItem;
                                    oOptElmt = base.addOption(ref oChoicesElmt, Strings.Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"));
                                    // lets add an image tag
                                    oDescElmt = this.moPageXML.CreateElement("img");
                                    oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                    oOptElmt.AppendChild(oDescElmt);

                                    // lets insert a description html tag
                                    if (!string.IsNullOrEmpty(oItem.InnerXml))
                                    {
                                        oDescElmt = this.moPageXML.CreateElement("div");
                                        oDescElmt.SetAttribute("class", "description");
                                        oDescElmt.InnerXml = oItem.InnerXml;
                                        oOptElmt.AppendChild(oDescElmt);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // do nothing
                        }

                        // Lets load in the available common templates from XML file
                        try
                        {
                            oXformDoc.Load(this.goServer.MapPath("/" + Cms.gcProjectPath + "ewcommon/xsl/pageLayouts") + "/LayoutManifest.xml");
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                        }
                        catch (Exception ex)
                        {
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "/" + Cms.gcProjectPath + "ewcommon/xsl/pageLayouts/LayoutManifest.xml could not be found. - " + ex.Message);
                        }

                        foreach (XmlElement currentOChoices1 in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                        {
                            oChoices = currentOChoices1;
                            var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                            foreach (XmlElement currentOItem1 in oChoices.SelectNodes("Layout"))
                            {
                                oItem = currentOItem1;
                                oOptElmt = base.addOption(ref oChoicesElmt, Strings.Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"));

                                // lets add an image tag
                                oDescElmt = this.moPageXML.CreateElement("img");
                                oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                oOptElmt.AppendChild(oDescElmt);

                                // lets insert a description html tag
                                if (!string.IsNullOrEmpty(oItem.InnerXml))
                                {
                                    oDescElmt = this.moPageXML.CreateElement("div");
                                    oDescElmt.SetAttribute("class", "description");
                                    oDescElmt.InnerXml = oItem.InnerXml;
                                    oOptElmt.AppendChild(oDescElmt);
                                }
                            }
                        }

                        NameValueCollection oConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                        if (oConfig["cart"] == "on")
                        {
                            try
                            {
                                oXformDoc.Load(this.goServer.MapPath("/" + Cms.gcProjectPath + "ewcommon/xsl/cart") + "/LayoutManifest.xml");
                                sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                            }
                            catch (Exception ex)
                            {
                                XmlNode argoNode = oFrmElmt;
                                base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "/" + Cms.gcProjectPath + "ewcommon/xsl/cart/LayoutManifest.xml could not be found. - " + ex.Message);
                            }

                            foreach (XmlElement currentOChoices2 in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                            {
                                oChoices = currentOChoices2;
                                var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                                foreach (XmlElement currentOItem2 in oChoices.SelectNodes("Layout"))
                                {
                                    oItem = currentOItem2;
                                    oOptElmt = base.addOption(ref oChoicesElmt, Strings.Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"));

                                    // lets add an image tag
                                    oDescElmt = this.moPageXML.CreateElement("img");
                                    oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                    oOptElmt.AppendChild(oDescElmt);

                                    // lets insert a description html tag
                                    if (!string.IsNullOrEmpty(oItem.InnerXml))
                                    {
                                        oDescElmt = this.moPageXML.CreateElement("div");
                                        oDescElmt.SetAttribute("class", "description");
                                        oDescElmt.InnerXml = oItem.InnerXml;
                                        oOptElmt.AppendChild(oDescElmt);
                                    }
                                }
                            }
                        }

                        if (oConfig["membership"] == "on")
                        {
                            try
                            {
                                oXformDoc.Load(this.goServer.MapPath("/" + Cms.gcProjectPath + "ewcommon/xsl/membership") + "/LayoutManifest.xml");
                                sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                            }
                            catch (Exception ex)
                            {
                                XmlNode argoNode = oFrmElmt;
                                base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "/" + Cms.gcProjectPath + "ewcommon/xsl/membership/LayoutManifest.xml could not be found. - " + ex.Message);
                            }

                            foreach (XmlElement currentOChoices3 in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                            {
                                oChoices = currentOChoices3;
                                var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                                foreach (XmlElement currentOItem3 in oChoices.SelectNodes("Layout"))
                                {
                                    oItem = currentOItem3;
                                    oOptElmt = base.addOption(ref oChoicesElmt, Strings.Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"));

                                    // lets add an image tag
                                    oDescElmt = this.moPageXML.CreateElement("img");
                                    oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                    oOptElmt.AppendChild(oDescElmt);

                                    // lets insert a description html tag
                                    if (!string.IsNullOrEmpty(oItem.InnerXml))
                                    {
                                        oDescElmt = this.moPageXML.CreateElement("div");
                                        oDescElmt.SetAttribute("class", "description");
                                        oDescElmt.InnerXml = oItem.InnerXml;
                                        oOptElmt.AppendChild(oDescElmt);
                                    }
                                }
                            }
                        }

                        if (pgid > 0L)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, pgid);
                        }
                        else
                        {
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "No page identified");
                        }

                        if (base.isSubmitted() | !string.IsNullOrEmpty(this.goRequest.Form["ewsubmit.x"]) | !string.IsNullOrEmpty(this.goRequest.Form["cStructLayout"]))
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                if (pgid > 0L)
                                {
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, base.Instance);
                                }
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

                public XmlElement xFrmEditMailLayout(long pgid = 0L)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oChoices;
                    // Dim oChoicesElmt As XmlElement
                    XmlElement oItem;
                    XmlElement oOptElmt;
                    XmlElement oDescElmt;
                    string sImgPath = "";
                    string cProcessInfo = "";
                    var oXformDoc = new XmlDocument();
                    try
                    {
                        base.NewFrm("EditPageLayout");
                        base.submission("EditEage", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPage", "", "Select Page Layout");
                        base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nStructParId", "tblContentStructure/nStructParId", "true()", oBindParent: ref argoBindParent);

                        // MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select page layout")

                        oSelElmt = base.addSelect1(ref oFrmElmt, "cStructLayout", true, "", "PickByImage", Protean.xForm.ApperanceTypes.Full);
                        XmlElement argoBindParent1 = null;
                        base.addBind("cStructLayout", "tblContentStructure/cStructLayout", "true()", oBindParent: ref argoBindParent1);
                        try
                        {
                            // if this file exists then add the bespoke templates
                            oXformDoc.Load(this.goServer.MapPath(goConfig["ProjectPath"] + "/xsl/Mailer/") + "/LayoutManifest.xml");
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                            foreach (XmlElement currentOChoices in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                            {
                                oChoices = currentOChoices;
                                var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                                foreach (XmlElement currentOItem in oChoices.SelectNodes("Layout"))
                                {
                                    oItem = currentOItem;
                                    oOptElmt = base.addOption(ref oChoicesElmt, Strings.Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"));
                                    // lets add an image tag
                                    oDescElmt = this.moPageXML.CreateElement("img");
                                    oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                    oOptElmt.AppendChild(oDescElmt);

                                    // lets insert a description html tag
                                    if (!string.IsNullOrEmpty(oItem.InnerXml))
                                    {
                                        oDescElmt = this.moPageXML.CreateElement("div");
                                        oDescElmt.SetAttribute("class", "description");
                                        oDescElmt.InnerXml = oItem.InnerXml;
                                        oOptElmt.AppendChild(oDescElmt);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // do nothing
                        }



                        // Lets load in the available common templates from XML file
                        try
                        {
                            oXformDoc.Load(this.goServer.MapPath("/" + Cms.gcProjectPath + "ewcommon/xsl/mailer") + "/LayoutManifest.xml");
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                        }
                        catch (Exception ex)
                        {
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "/" + Cms.gcProjectPath + "ewcommon/xsl/mailer/LayoutManifest.xml could not be found. - " + ex.Message);
                        }

                        foreach (XmlElement currentOChoices1 in oXformDoc.SelectNodes("/PageLayouts/LayoutGroup"))
                        {
                            oChoices = currentOChoices1;
                            var oChoicesElmt = base.addChoices(ref oSelElmt, oChoices.GetAttribute("name"));
                            foreach (XmlElement currentOItem1 in oChoices.SelectNodes("Layout"))
                            {
                                oItem = currentOItem1;
                                oOptElmt = base.addOption(ref oChoicesElmt, Strings.Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"));

                                // lets add an image tag
                                oDescElmt = this.moPageXML.CreateElement("img");
                                oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                oOptElmt.AppendChild(oDescElmt);

                                // lets insert a description html tag
                                if (!string.IsNullOrEmpty(oItem.InnerXml))
                                {
                                    oDescElmt = this.moPageXML.CreateElement("div");
                                    oDescElmt.SetAttribute("class", "description");
                                    oDescElmt.InnerXml = oItem.InnerXml;
                                    oOptElmt.AppendChild(oDescElmt);
                                }
                            }
                        }


                        // MyBase.addSubmit(oFrmElmt, "", "Save Page")

                        if (pgid > 0L)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, pgid);
                        }
                        else
                        {
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "No page identified");
                        }

                        if (base.isSubmitted() | !string.IsNullOrEmpty(this.goRequest.Form["ewsubmit.x"]) | !string.IsNullOrEmpty(this.goRequest.Form["cStructLayout"]))
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                if (pgid > 0L)
                                {
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, base.Instance);
                                }
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

                public XmlElement xFrmAddModule(long pgid, string position)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string sImgPath = "";
                    string cProcessInfo = "";
                    var oXformDoc = new XmlDocument();
                    try
                    {
                        if (!string.IsNullOrEmpty(moRequest["cModuleBox"]))
                        {
                            if (goConfig["cssFramework"] == "bs5")
                            {
                                int argnReturnId = 0;
                                string argzcReturnSchema = "";
                                string argAlternateFormName = "";
                                xFrmEditContent(0L, moRequest["cModuleType"], pgid, moRequest["cPosition"], nReturnId: ref argnReturnId, zcReturnSchema: ref argzcReturnSchema, AlternateFormName: ref argAlternateFormName);
                            }
                            else
                            {
                                int argnReturnId1 = 0;
                                string argzcReturnSchema1 = "";
                                string argAlternateFormName1 = "";
                                xFrmEditContent(0L, "Module/" + moRequest["cModuleType"], pgid, moRequest["cPosition"], nReturnId: ref argnReturnId1, zcReturnSchema: ref argzcReturnSchema1, AlternateFormName: ref argAlternateFormName1);
                            }

                            return base.moXformElmt;
                        }
                        else
                        {
                            base.NewFrm("EditPageLayout");
                            base.submission("AddModule", "", "post", "form_check(this)");
                            base.Instance.InnerXml = "<Module position=\"" + position + "\"></Module>";
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Add Module", "", "Select Module Type");
                            base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                            base.addInput(ref oFrmElmt, "cPosition", true, "Position", "hidden");
                            XmlElement argoBindParent = null;
                            base.addBind("cPosition", "Module/@position", "true()", oBindParent: ref argoBindParent);
                            oSelElmt = base.addSelect1(ref oFrmElmt, "cModuleType", true, "", "PickByImage", Protean.xForm.ApperanceTypes.Full);
                            GetModuleOptions(ref oSelElmt);
                            if (base.isSubmitted() | !string.IsNullOrEmpty(this.goRequest.Form["ewsubmit.x"]) | !string.IsNullOrEmpty(this.goRequest.Form["cModuleType"]))
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                if (base.valid)
                                {
                                    // Do nothing
                                    // or redirect to content form
                                    // 
                                    if (goConfig["cssFramework"] == "bs5")
                                    {
                                        int argnReturnId2 = 0;
                                        string argzcReturnSchema2 = "";
                                        string argAlternateFormName2 = "";
                                        xFrmEditContent(0L, moRequest["cModuleType"], pgid, moRequest["cPosition"], nReturnId: ref argnReturnId2, zcReturnSchema: ref argzcReturnSchema2, AlternateFormName: ref argAlternateFormName2);
                                    }
                                    else
                                    {
                                        int argnReturnId3 = 0;
                                        string argzcReturnSchema3 = "";
                                        string argAlternateFormName3 = "";
                                        xFrmEditContent(0L, "Module/" + moRequest["cModuleType"], pgid, moRequest["cPosition"], nReturnId: ref argnReturnId3, zcReturnSchema: ref argzcReturnSchema3, AlternateFormName: ref argAlternateFormName3);
                                    }
                                }
                            }

                            base.addValues();
                            return base.moXformElmt;
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                protected string GetContentFormPath(string SchemaName)
                {
                    string cProcessInfo = "";
                    try
                    {
                        var ModuleList = this.moPageXML.CreateElement("Select");
                        this.GetContentOptions(ref ModuleList);
                        XmlElement thisModule = (XmlElement)ModuleList.SelectSingleNode("descendant-or-self::item[@type='" + SchemaName + "']");
                        if (thisModule is null)
                        {
                            return SchemaName;
                        }
                        else
                        {
                            return thisModule.SelectSingleNode("value").InnerText;
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "GetContentFormPath", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                protected string GetModuleFormPath(string SchemaName)
                {
                    string cProcessInfo = "";
                    try
                    {
                        var ModuleList = this.moPageXML.CreateElement("Select");
                        this.GetModuleOptions(ref ModuleList);
                        XmlElement thisModule = (XmlElement)ModuleList.SelectSingleNode("descendant-or-self::item[@type='" + SchemaName + "']");
                        if (thisModule is null)
                        {
                            return SchemaName;
                        }
                        else
                        {
                            return thisModule.SelectSingleNode("value").InnerText;
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "GetContentFormPath", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                protected void GetContentOptions(ref XmlElement oSelElmt)
                {
                    string cProcessInfo = "";
                    try
                    {
                        object PathPrefix = "ewcommon/xsl/";
                        if (goConfig["cssFramework"] == "bs5")
                        {
                            PathPrefix = @"ptn\";
                            EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"core\modules")), "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            var rootFolder = new DirectoryInfo(this.goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "modules"))));
                            foreach (var fld in rootFolder.GetDirectories())
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"modules\"), fld.Name)), "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            if (!string.IsNullOrEmpty(this.myWeb.moConfig["ClientCommonFolder"]))
                            {
                                this.EnumberateManifestOptions(ref oSelElmt, this.myWeb.moConfig["ClientCommonFolder"] + @"\xsl", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }

                            EnumberateManifestOptions(ref oSelElmt, "/xsl", "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            if (this.myWeb.moConfig["Search"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\search")), "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["Membership"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\membership")), "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["Cart"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\cart")), "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["Quote"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\quote")), "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["MailingList"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\mailer")), "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["Subscriptions"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\subscriptions")), "ContentTypes/ContentTypeGroup", "ContentType", false, "manifest.xml");
                            }
                        }
                        else
                        {
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "PageLayouts", "ModuleTypes/ModuleGroup", "Module", False)

                            // MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select Module Type")

                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "PageLayouts", "ModuleTypes/ModuleGroup", "Module", False)

                            // If myWeb.moConfig("ClientCommonFolder") <> "" Then
                            // EnumberateManifestOptions(oSelElmt, myWeb.moConfig("ClientCommonFolder") & "/xsl", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // EnumberateManifestOptions(oSelElmt, "/xsl", "ModuleTypes/ModuleGroup", "Module", True)
                            // If myWeb.moConfig("Search") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Search", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("Membership") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Membership", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("Cart") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Cart", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("Quote") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Quote", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("MailingList") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Mailer", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                            // If myWeb.moConfig("Subscriptions") = "on" Then
                            // EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & PathPrefix & "Subscriptions", "ModuleTypes/ModuleGroup", "Module", False)
                            // End If
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                    }
                }

                protected void GetModuleOptions(ref XmlElement oSelElmt)
                {
                    string cProcessInfo = "";
                    try
                    {
                        object PathPrefix = "ewcommon/xsl/";
                        if (goConfig["cssFramework"] == "bs5")
                        {
                            PathPrefix = @"ptn\";
                            EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"core\modules")), "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            var rootFolder = new DirectoryInfo(this.goServer.MapPath(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "modules"))));
                            foreach (var fld in rootFolder.GetDirectories())
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"modules\"), fld.Name)), "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            if (!string.IsNullOrEmpty(this.myWeb.moConfig["ClientCommonFolder"]))
                            {
                                this.EnumberateManifestOptions(ref oSelElmt, this.myWeb.moConfig["ClientCommonFolder"] + @"\xsl", "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }

                            EnumberateManifestOptions(ref oSelElmt, "/xsl", "ModuleTypes/ModuleGroup", "Module", true, "manifest.xml");
                            if (this.myWeb.moConfig["Search"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\search")), "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["Membership"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\membership")), "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["Cart"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\cart")), "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["Quote"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\quote")), "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["MailingList"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\mailer")), "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }

                            if (this.myWeb.moConfig["Subscriptions"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), @"features\subscriptions")), "ModuleTypes/ModuleGroup", "Module", false, "manifest.xml");
                            }
                        }
                        else
                        {
                            EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "PageLayouts")), "ModuleTypes/ModuleGroup", "Module", false);

                            // MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select Module Type")

                            EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "PageLayouts")), "ModuleTypes/ModuleGroup", "Module", false);
                            if (!string.IsNullOrEmpty(this.myWeb.moConfig["ClientCommonFolder"]))
                            {
                                this.EnumberateManifestOptions(ref oSelElmt, this.myWeb.moConfig["ClientCommonFolder"] + "/xsl", "ModuleTypes/ModuleGroup", "Module", false);
                            }

                            EnumberateManifestOptions(ref oSelElmt, "/xsl", "ModuleTypes/ModuleGroup", "Module", true);
                            if (this.myWeb.moConfig["Search"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "Search")), "ModuleTypes/ModuleGroup", "Module", false);
                            }

                            if (this.myWeb.moConfig["Membership"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "Membership")), "ModuleTypes/ModuleGroup", "Module", false);
                            }

                            if (this.myWeb.moConfig["Cart"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "Cart")), "ModuleTypes/ModuleGroup", "Module", false);
                            }

                            if (this.myWeb.moConfig["Quote"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "Quote")), "ModuleTypes/ModuleGroup", "Module", false);
                            }

                            if (this.myWeb.moConfig["MailingList"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "Mailer")), "ModuleTypes/ModuleGroup", "Module", false);
                            }

                            if (this.myWeb.moConfig["Subscriptions"] == "on")
                            {
                                EnumberateManifestOptions(ref oSelElmt, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("/" + Cms.gcProjectPath, PathPrefix), "Subscriptions")), "ModuleTypes/ModuleGroup", "Module", false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                    }
                }

                protected void EnumberateManifestOptions(ref XmlElement oSelectElmt, string filepath, string groupName, string optionName, bool bIgnoreIfNotFound, string manifestFilename = "LayoutManifest.xml")
                {
                    var oXformDoc = new XmlDocument();
                    string cProcessInfo = "";
                    string sImgPath = "";
                    XmlElement oOptElmt;
                    XmlElement oDescElmt;
                    try
                    {
                        if (string.IsNullOrEmpty(filepath))
                            filepath = "/";
                        try
                        {
                            // if this file exists then add the bespoke templates
                            oXformDoc.Load(this.goServer.MapPath(filepath) + "/" + manifestFilename);
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath");
                            foreach (XmlElement oChoices in oXformDoc.SelectNodes("/PageLayouts/" + groupName))
                            {
                                if (string.IsNullOrEmpty(oChoices.GetAttribute("targetCssFramework")) | this.myWeb.moConfig["cssFramework"] is object & oChoices.GetAttribute("targetCssFramework").Contains("" + this.myWeb.moConfig["cssFramework"]))
                                {
                                    // do we have a choices element?
                                    XmlElement oChoicesElmt = (XmlElement)oSelectElmt.SelectSingleNode("choices[label/node()='" + oChoices.GetAttribute("name") + "']");
                                    if (oChoicesElmt is null)
                                    {
                                        oChoicesElmt = base.addChoices(ref oSelectElmt, oChoices.GetAttribute("name"));
                                    }

                                    if (!string.IsNullOrEmpty(oChoices.GetAttribute("icon")))
                                    {
                                        XmlElement labelElmt = (XmlElement)oChoicesElmt.SelectSingleNode("label");
                                        labelElmt.SetAttribute("icon", oChoices.GetAttribute("icon"));
                                    }

                                    foreach (XmlElement oItem in oChoices.SelectNodes(optionName))
                                    {
                                        if (string.IsNullOrEmpty(oItem.GetAttribute("targetCssFramework")) | this.myWeb.moConfig["cssFramework"] is object & oItem.GetAttribute("targetCssFramework").Contains("" + this.myWeb.moConfig["cssFramework"]))
                                        {
                                            string FormPath = oItem.GetAttribute("type");
                                            if (!string.IsNullOrEmpty(oItem.GetAttribute("formPath")))
                                            {
                                                FormPath = oItem.GetAttribute("formPath");
                                            }

                                            oOptElmt = base.addOption(ref oChoicesElmt, Strings.Replace(oItem.GetAttribute("name"), "_", " "), FormPath);
                                            // lets add an image tag
                                            oOptElmt.SetAttribute("type", oItem.GetAttribute("type"));
                                            oDescElmt = this.moPageXML.CreateElement("img");
                                            oDescElmt.SetAttribute("src", sImgPath + "/" + oItem.GetAttribute("name") + ".gif");
                                            if (!string.IsNullOrEmpty(oItem.GetAttribute("icon")))
                                            {
                                                oDescElmt.SetAttribute("icon", oItem.GetAttribute("icon"));
                                            }

                                            oOptElmt.AppendChild(oDescElmt);
                                            // lets insert a description html tag
                                            if (!string.IsNullOrEmpty(oItem.InnerXml))
                                            {
                                                oDescElmt = this.moPageXML.CreateElement("div");
                                                oDescElmt.SetAttribute("class", "description");
                                                oDescElmt.InnerXml = oItem.InnerXml;
                                                oOptElmt.AppendChild(oDescElmt);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!bIgnoreIfNotFound)
                            {
                                var argoNode = oSelectElmt.ParentNode;
                                base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, filepath + " could not be found. - " + ex.Message);
                                oSelectElmt.ParentNode = argoNode;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "EnumberateManifestOptions", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                    }
                }
                // Public Overridable Function xFrmEditContent(Optional ByVal id As Long = 0, Optional ByVal cContentSchemaName As String = "", Optional ByVal pgid As Long = 0, Optional ByVal cContentName As String = "", Optional ByVal bCopy As Boolean = False, Optional ByRef nReturnId As Integer = 0, Optional ByVal nVersionId As Long = 0) As XmlElement
                // xFrmEditContent(id, cContentSchemaName, pgid, cContentName, bCopy, nReturnId, "", "", Optional ByVal nVersionId As Long = 0)
                // End Function

                public virtual XmlElement xFrmEditContent(long id = 0L, string cContentSchemaName = "", long pgid = 0L, string cContentName = "", bool bCopy = false, [Optional, DefaultParameterValue(0)] ref int nReturnId, [Optional, DefaultParameterValue("")] ref string zcReturnSchema, [Optional, DefaultParameterValue("")] ref string AlternateFormName, long nVersionId = 0L)
                {
                    XmlElement oFrmElmt;
                    XmlElement oGrp1Elmt;
                    XmlElement oGrp2Elmt;
                    XmlElement oSelElmt;
                    var oTempInstance = this.moPageXML.CreateElement("instance");
                    bool bCascade = false;
                    string cProcessInfo = "";
                    XmlElement oCRNode;
                    string cModuleType = "";

                    // Location specific scopes
                    XmlNodeList oLocationSelects = null;
                    XmlNodeList oMenuItemsFromSelect = null;
                    xFormContentLocations oContentLocations;
                    try
                    {
                        var integrationHelper = new Protean.Integration.Directory.Helper(ref this.myWeb);
                        if (id > 0L)
                        {
                            // we may be halfway through a trigger so lets rescue the instance from the session
                            if (this.goSession["oContentInstance"] is null)
                            {
                                if (nVersionId > 0L)
                                {
                                    oTempInstance = moDbHelper.GetVersionInstance(id, nVersionId);
                                    // Only Update the status if the cmd is ewcmd is RollbackContent
                                    if (this.myWeb.moRequest["ewCmd"] == "RollbackContent")
                                    {
                                        oTempInstance.SelectSingleNode("tblContent/nStatus").InnerText = ((int)Cms.dbHelper.Status.Live).ToString();
                                    }
                                }
                                else
                                {
                                    oTempInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, id);
                                }

                                // turn off process repeats when loading from file
                                this.bProcessRepeats = false;
                            }
                            else
                            {
                                oTempInstance = (XmlElement)this.goSession["oContentInstance"];
                            }

                            if (string.IsNullOrEmpty(cContentSchemaName))
                            {
                                cContentSchemaName = oTempInstance.SelectSingleNode("tblContent/cContentSchemaName").InnerText;
                                if (cContentSchemaName == "Module")
                                {
                                    if (oTempInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content/@moduleType") is object)
                                    {
                                        cModuleType = oTempInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content/@moduleType").Value;
                                    }
                                }

                                if (!string.IsNullOrEmpty(moRequest["type"]))
                                    cContentSchemaName = moRequest["type"];
                            }

                            XmlElement argContentNode = (XmlElement)oTempInstance.FirstChild;
                            moDbHelper.getLocationsByContentId(id, ref argContentNode);
                            oTempInstance.FirstChild = argContentNode;

                            // Add ProductCategories
                            string sProductTypes = "Product,SKU";
                            if (!string.IsNullOrEmpty(this.myWeb.moConfig["ProductTypes"]))
                            {
                                sProductTypes = this.myWeb.moConfig["ProductTypes"];
                            }

                            sProductTypes = sProductTypes.Trim().TrimEnd(',') + ",";
                            if (sProductTypes.Contains(cContentSchemaName + ",") & id > 0L)
                            {
                                if (moDbHelper.checkDBObjectExists("sp_GetProductGroups"))
                                {
                                    var prodCatElmt = oTempInstance.OwnerDocument.CreateElement("ProductGroups");
                                    string sSQL = "execute sp_GetProductGroups " + id;
                                    string Ids = Conversions.ToString(moDbHelper.GetDataValue(sSQL));
                                    Ids.TrimEnd(',');
                                    prodCatElmt.SetAttribute("ids", Ids);
                                    oTempInstance.AppendChild(prodCatElmt);
                                }
                            }
                        }

                        if (this.goSession["oContentInstance"] is object)
                        {
                            // turn off process repeats when loading from file if we are going to load the instance later.
                            this.bProcessRepeats = false;
                        }

                        // Set the return parameter
                        zcReturnSchema = cContentSchemaName;

                        // ok lets load in an xform from the file location.
                        // If cContentSchemaName = "Subscription" Then
                        // Return xFrmEditSubscription(id, pgid)
                        // End If

                        // '''''' if contentSchemeaName starts with "filter|" then modify the path...

                        string cXformPath = cContentSchemaName;
                        if (!string.IsNullOrEmpty(AlternateFormName))
                            cXformPath = AlternateFormName;
                        if (!string.IsNullOrEmpty(cModuleType))
                        {
                            cXformPath = cXformPath + "/" + cModuleType;
                            if (goConfig["cssFramework"] == "bs5")
                            {
                                cXformPath = GetModuleFormPath(cModuleType);
                            }
                        }
                        else if (goConfig["cssFramework"] == "bs5")
                        {
                            cXformPath = GetContentFormPath(cContentSchemaName);
                        }

                        if (goConfig["cssFramework"] == "bs5")
                        {
                            cXformPath = "/modules/" + cXformPath;
                        }
                        else
                        {
                            cXformPath = "/xforms/content/" + cXformPath;
                        }

                        if (!base.load(cXformPath + ".xml", this.myWeb.maCommonFolders))
                        {
                            // load a default content xform if no alternative.
                            cProcessInfo = cXformPath + ".xml - Not Found";
                            base.NewFrm("EditContent");
                            base.submission("EditContent", "", "post", "form_check(this)");
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "EditContent", "2Col", "Edit Content");
                            base.addNote("EditContent", Protean.xForm.noteTypes.Alert, "We do not have an XForm for this type of content - this is the default form");
                        }

                        if (id > 0L)
                        {
                            // here it would be really useful to merge nodes!
                            base.bProcessRepeats = true;
                            foreach (XmlElement NonTableInstanceElements in base.Instance)
                            {
                                if (NonTableInstanceElements.Name != "tblContent")
                                {
                                    // <Relation type="" direction="child" relatedContentId=""/>
                                    if (NonTableInstanceElements.Name == "Relation")
                                    {
                                        string sSql;
                                        if (Strings.LCase(NonTableInstanceElements.GetAttribute("direction")) == "child")
                                        {
                                            sSql = "Select nContentParentId from tblContentRelation where nContentChildId = " + id + " And cRelationType = '" + NonTableInstanceElements.GetAttribute("type") + "'";
                                        }
                                        else
                                        {
                                            sSql = "Select nContentChildId from tblContentRelation where nContentParentId = " + id + " AND cRelationType = '" + NonTableInstanceElements.GetAttribute("type") + "'";
                                        }

                                        var oRead = moDbHelper.getDataReader(sSql);
                                        string CSV = "";
                                        while (oRead.Read())
                                        {
                                            if (!string.IsNullOrEmpty(CSV))
                                            {
                                                CSV = CSV + ",";
                                            }

                                            CSV = CSV + oRead.GetInt32(0).ToString();
                                        }

                                        NonTableInstanceElements.SetAttribute("relatedContentId", CSV);
                                    }

                                    var newNode = oTempInstance.OwnerDocument.ImportNode(NonTableInstanceElements, true);
                                    oTempInstance.AppendChild(newNode);
                                }
                            }

                            base.updateInstance(oTempInstance);

                            // Add related content to the instance
                            oCRNode = this.moPageXML.CreateElement("ContentRelations");
                            moDbHelper.addRelatedContent(ref oCRNode, (int)id, true);
                            base.Instance.AppendChild(oCRNode);
                            if (bCopy)
                            {
                                oCRNode.SetAttribute("copyRelations", "true");

                                // select all nodes where content = cContentName
                                cContentName = base.Instance.SelectSingleNode("tblContent/cContentName").InnerText;
                                foreach (XmlElement oElmt in base.Instance.SelectNodes("descendant::*[.='" + cContentName + "']"))
                                {
                                    if ((oElmt.InnerText ?? "") == (cContentName ?? "") & oElmt.Name == "cContentName")
                                    {
                                        oElmt.InnerText = "Copy of " + cContentName;
                                    }
                                }
                                // set the id to zero
                                id = 0L;
                                base.Instance.SelectSingleNode("tblContent/nContentKey").InnerText = 0.ToString();
                                // remove any audit info, but keep the publish start and expire dates.
                                base.Instance.SelectSingleNode("tblContent/nAuditId").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/nAuditKey").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/dInsertDate").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/nInsertDirId").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/dUpdateDate").InnerText = "";
                                base.Instance.SelectSingleNode("tblContent/nUpdateDirId").InnerText = "";
                            }

                            var argoNode = base.Instance.SelectSingleNode("tblContent");
                            Protean.xmlTools.addNewTextNode("bCascade", ref argoNode, Strings.LCase(Conversions.ToString(moDbHelper.isCascade(id))), true, false);
                            if (pgid == 0L)
                            {
                                // lets go get a parId to set the pgid so we can update the cascade position
                                pgid = moDbHelper.getPrimaryLocationByArtId(id);
                            }
                        }
                        else
                        {
                            if (this.goSession["oContentInstance"] is object)
                            {
                                this.bProcessRepeats = true;
                                base.Instance = (XmlElement)this.goSession["oContentInstance"];
                            }

                            if (!string.IsNullOrEmpty(cContentName))
                            {
                                base.Instance.SelectSingleNode("tblContent/cContentName").InnerText = cContentName;
                                base.Instance.SelectSingleNode("tblContent/dPublishDate").InnerText = XmlDate(DateAndTime.Now);
                            }
                            // we are adding orphan content so cannot cascade
                            // remove any cascade radio on form
                            if (pgid == 0L)
                            {
                                foreach (XmlElement oElmt in base.moXformElmt.SelectNodes("descendant-or-self::*[@bind='bCascade']"))
                                    oElmt.ParentNode.RemoveChild(oElmt);
                            }
                            else
                            {
                                var argoNode1 = base.Instance.SelectSingleNode("tblContent");
                                Protean.xmlTools.addNewTextNode("bCascade", ref argoNode1, "", true, false);
                            }
                        }

                        // Check for adding the integration checkbox
                        var argform = this;
                        integrationHelper.PostContentCheckboxes(ref argform, cContentSchemaName, id > 0L);

                        // Process any location selects
                        Cms.xForm argForm = (Cms.xForm)this;
                        oContentLocations = new xFormContentLocations(id, ref argForm);
                        oContentLocations.ProcessSelects();

                        // Version Control: if on, copy the status node for use after submission
                        if (this.myWeb.gbVersionControl)
                        {
                            string nCurrentStatus = "";
                            XmlNodeState localNodeState() { var argoNode = base.Instance; var ret = Xml.NodeState(ref argoNode, "//nStatus", returnAsText: nCurrentStatus); base.Instance = argoNode; return ret; }

                            if (localNodeState() == XmlNodeState.HasContents)
                            {
                                XmlNode argoNode2 = (XmlNode)base.Instance;
                                Protean.xmlTools.addNewTextNode("currentStatus", ref argoNode2, nCurrentStatus);
                                base.Instance = (XmlElement)argoNode2;
                            }
                        }

                        // Additional Processing : Post Build
                        xFrmEditContentPostBuildProcessing(cContentSchemaName);
                        if (base.isSubmitted())
                        {

                            // Additional Processing : Pre Submission 
                            xFrmEditContentSubmissionPreProcessing();
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                bool bPreviewRedirect = false;
                                if (!string.IsNullOrEmpty(this.goRequest["ptn-preview"]))
                                {
                                    if (this.myWeb.gbVersionControl)
                                    {
                                        // Leave the current version unchanged and live

                                        // create a new version of the content as pending
                                        base.Instance.SelectSingleNode("tblContent/nStatus").InnerText = ((int)Cms.dbHelper.Status.InProgress).ToString();

                                        // redirect to preview version in preview mode
                                        bPreviewRedirect = true;
                                    }
                                }

                                Cms.dbHelper.ActivityType editResult = default;

                                // we don't need this now.
                                this.goSession["oContentInstance"] = null;

                                // trim the contentName to no longer than 255 chars
                                base.Instance.SelectSingleNode("*/cContentName").InnerXml = Strings.Left(base.Instance.SelectSingleNode("*/cContentName").InnerXml, 255);

                                // remove any invalid charactors from the contentName
                                var oUrlExp = new Regex(@"[^\w\-\u0020\+]");
                                base.Instance.SelectSingleNode("*/cContentName").InnerXml = oUrlExp.Replace(base.Instance.SelectSingleNode("*/cContentName").InnerXml, "");
                                oUrlExp = null;
                                if (base.Instance.SelectSingleNode("*/bCascade") is object)
                                {
                                    if (base.Instance.SelectSingleNode("*/bCascade").InnerXml == "true")
                                    {
                                        bCascade = true;
                                    }
                                }

                                if (id > 0L)
                                {
                                    object updatedVersionId = moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance);
                                    moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentEdited, this.myWeb.mnUserId, this.myWeb.moSession.SessionID, DateAndTime.Now, (int)id, (int)pgid, "");
                                    // Redirection 
                                    string redirectType = "";
                                    string newUrl = "";
                                    string strOldurl = "";
                                    if (moRequest["redirectType"] is object)
                                    {
                                        redirectType = moRequest["redirectType"].ToString();
                                    }

                                    if (moRequest["productNewUrl"] is object)
                                    {
                                        newUrl = moRequest["productNewUrl"].ToString();
                                    }

                                    if (moRequest["productOldUrl"] is object)
                                    {
                                        strOldurl = moRequest["productOldUrl"].ToString();
                                    }









                                    // Individual content location set
                                    // Don't set a location if a contentparid has been passed (still process content locations as tickboexs on the form, if they've been set)
                                    if (!(this.myWeb.moRequest["contentParId"] is object & !string.IsNullOrEmpty(this.myWeb.moRequest["contentParId"])))
                                    {

                                        // TS 28-11-2017 we only want to update the cascade information if the content is on this page.
                                        // If not on this page i.e. being edited via search results or related content on a page we should ignore this.
                                        if (Conversions.ToDouble(moDbHelper.ExeProcessSqlScalar("select count(nContentLocationKey) from tblContentLocation where nContentId=" + id + " and nStructId = " + pgid)) > 0d)
                                        {
                                            moDbHelper.setContentLocation(pgid, id, bCascade: bCascade, cPosition: "");
                                        }
                                    }

                                    // TS 10-01-2014 fix for cascade on saved items... To Be tested
                                    if (bCascade & pgid > 0L)
                                    {
                                        moDbHelper.setContentLocation(pgid, id, true, bCascade);
                                    }

                                    editResult = Cms.dbHelper.ActivityType.ContentEdited;
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(updatedVersionId, id, false)))
                                    {
                                        nReturnId = Conversions.ToInteger(updatedVersionId);
                                    }
                                    else
                                    {
                                        nReturnId = (int)id;
                                    }
                                }
                                else
                                {
                                    long nContentId;
                                    nContentId = Conversions.ToLong(moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance));
                                    moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentAdded, this.myWeb.mnUserId, this.myWeb.moSession.SessionID, DateAndTime.Now, (int)nContentId, (int)pgid, "");


                                    // If we have an action here we need to relate the item
                                    if (Conversions.ToBoolean(Operators.OrObject(Operators.OrObject(Operators.ConditionalCompareObjectEqual(this.goSession["mcRelAction"], "Add", false), Operators.ConditionalCompareObjectEqual(this.goSession["mcRelAction"], "Find", false)), Operators.ConditionalCompareObjectEqual(this.goSession["mcRelAction"], "Edit", false))))
                                    {
                                        bool b2Way = Conversions.ToBoolean(Interaction.IIf(moRequest["RelType"] == "2way" | moRequest["direction"] == "2Way", true, false));
                                        string sRelType = moRequest["relationType"];
                                        moDbHelper.insertContentRelation(Conversions.ToInteger(this.goSession["mcRelParent"]), nContentId.ToString(), b2Way, sRelType);
                                    }
                                    // TS - Change 26/04/2016 We do not want added to the page if it is related.

                                    // Individual content location set
                                    // Don't set a location if a contentparid has been passed (still process content locations as tickboexs on the form, if they've been set)
                                    else if (!(this.myWeb.moRequest["contentParId"] is object & !string.IsNullOrEmpty(this.myWeb.moRequest["contentParId"])))
                                    {
                                        moDbHelper.setContentLocation(pgid, nContentId, true, bCascade, cPosition: moRequest["cPosition"]);
                                    }

                                    editResult = Cms.dbHelper.ActivityType.ContentAdded;

                                    // If this is a new element then we need to add the related content to the instance to be handled in processInstanceExtras
                                    nReturnId = (int)nContentId;
                                    foreach (var item in moRequest.Form)
                                    {
                                        if (Conversions.ToString(item).StartsWith("Relate_"))
                                        {
                                            var arr = Conversions.ToString(item).Split('_');
                                            var relateElmt = this.moPageXML.CreateElement("Relation");
                                            relateElmt.SetAttribute("relatedContentId", moRequest.Form[Conversions.ToString(item)]);
                                            relateElmt.SetAttribute("type", arr[1]);
                                            relateElmt.SetAttribute("direction", arr[2]);
                                            base.Instance.AppendChild(relateElmt);
                                        }
                                    }
                                }

                                // TS Added 24-11-2014 to allow content forms to add related content from dropdowns.
                                moDbHelper.processInstanceExtras(nReturnId, base.Instance, false, false);


                                // Check for related content redirection
                                string mcRelRedirectString = Conversions.ToString(this.goSession["mcRelRedirectString"]);
                                if (base.valid && !string.IsNullOrEmpty(mcRelRedirectString))
                                {
                                    string cQueryString = this.goRequest.QueryString.ToString();
                                    if (cQueryString.IndexOf("ewCmd=") > 0)
                                    {
                                        // here we fail because the ? is an & >=[
                                        if (cQueryString.IndexOf("&") > 0)
                                        {
                                            // So we have the index of the first &, how to replace?!
                                        }
                                    }
                                    else
                                    {
                                        cQueryString = "?" + Strings.Replace(this.goRequest.QueryString.ToString(), "path=", "");
                                    }

                                    if (cQueryString.IndexOf("ewCmd=") != -1)
                                    {
                                        cQueryString = cQueryString.Substring(cQueryString.IndexOf("ewCmd="));
                                    }

                                    if (cQueryString.IndexOf("ajaxCmd=") != -1)
                                    {
                                        cQueryString = cQueryString.Substring(cQueryString.IndexOf("ajaxCmd="));
                                    }

                                    mcRelRedirectString = mcRelRedirectString.Substring(mcRelRedirectString.IndexOf("ewCmd="));
                                    if ((mcRelRedirectString.ToLower() ?? "") == (cQueryString.ToLower() ?? ""))
                                    {
                                        // Suppress last page being reset anywhere else
                                        this.myWeb.mbSuppressLastPageOverrides = true;
                                        this.myWeb.moSession.Remove("lastPage");
                                        this.myWeb.msRedirectOnEnd = Conversions.ToString(this.goSession["mnContentRelationParent"]);
                                    }
                                }

                                this.goSession["mnContentRelationParent"] = null;
                                this.goSession["mcRelRedirectString"] = null;
                                this.goSession["mcRelAction"] = null;
                                this.goSession["mcRelParent"] = null;
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(this.goSession["EwCmd"], "", false)))
                                {
                                    this.goSession["EwCmd"] = "Normal";
                                }

                                // Submitted and valid - should have a content id let's process the relationships
                                oContentLocations.ProcessRequest(nReturnId);

                                // User Integrations check
                                if (integrationHelper.Enabled)
                                {
                                    integrationHelper.PostContent(nReturnId);


                                    // Module content edit action handler
                                }

                                foreach (XmlElement contentEditActionHandler in base.Instance.SelectNodes("//Content[@editAction]"))
                                {
                                    string contentEditAction = contentEditActionHandler.GetAttribute("editAction");
                                    if (!string.IsNullOrEmpty(contentEditAction))
                                    {
                                        string assemblyName = contentEditActionHandler.GetAttribute("assembly");
                                        string assemblyType = contentEditActionHandler.GetAttribute("assemblyType");
                                        string providerName = contentEditActionHandler.GetAttribute("providerName");
                                        string providerType = contentEditActionHandler.GetAttribute("providerType");
                                        if (string.IsNullOrEmpty(providerType))
                                            providerType = "messaging";
                                        string methodName = contentEditAction;
                                        string classPath = "";
                                        if (methodName.Contains("."))
                                        {
                                            methodName = Strings.Right(contentEditAction, Strings.Len(contentEditAction) - contentEditAction.LastIndexOf(".") - 1);
                                            classPath = Strings.Left(contentEditAction, contentEditAction.LastIndexOf("."));
                                        }

                                        // Dim providerSection As String = Coalesce(contentEditActionHandler.GetAttribute("providerSection"), "eonic/" & providerType & "Providers")

                                        // Edit Action method constructor follows the following format:
                                        // 1 - Protean.Cms
                                        // 2 - Content XML
                                        // 3 - Content ID being editted
                                        // 4 - Content action 

                                        try
                                        {
                                            Type calledType;
                                            if (!string.IsNullOrEmpty(assemblyName))
                                            {
                                                contentEditAction = contentEditAction + ", " + assemblyName;
                                            }
                                            // Dim oModules As New Protean.Cms.Membership.Modules

                                            if (!string.IsNullOrEmpty(providerName))
                                            {
                                                // case for external Providers
                                                Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/" + providerType + "Providers");
                                                Assembly assemblyInstance;
                                                if (moPrvConfig.Providers[providerName + "Local"] is object)
                                                {
                                                    if (!string.IsNullOrEmpty(moPrvConfig.Providers[providerName + "Local"].Parameters["path"]))
                                                    {
                                                        assemblyInstance = Assembly.LoadFrom(this.goServer.MapPath(moPrvConfig.Providers[providerName + "Local"].Parameters["path"]));
                                                        calledType = assemblyInstance.GetType(contentEditAction, true);
                                                    }
                                                    else
                                                    {
                                                        assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName + "Local"].Type);
                                                        calledType = assemblyInstance.GetType(contentEditAction, true);
                                                    }
                                                }
                                                else
                                                {
                                                    switch (moPrvConfig.Providers[providerName].Parameters["path"] ?? "")
                                                    {
                                                        case var @case when @case == "":
                                                            {
                                                                assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName].Type);
                                                                calledType = assemblyInstance.GetType(contentEditAction, true);
                                                                break;
                                                            }

                                                        case "builtin":
                                                            {
                                                                string prepProviderName; // = Replace(moPrvConfig.Providers(providerName).Type, ".", "+")
                                                                                         // prepProviderName = (New Regex("\+")).Replace(prepProviderName, ".", 1)
                                                                prepProviderName = moPrvConfig.Providers[providerName].Type;
                                                                calledType = Type.GetType(prepProviderName + "+" + classPath, true);
                                                                break;
                                                            }

                                                        default:
                                                            {
                                                                assemblyInstance = Assembly.LoadFrom(this.goServer.MapPath(moPrvConfig.Providers[providerName].Parameters["path"]));
                                                                classPath = moPrvConfig.Providers[providerName].Parameters["classPrefix"] + classPath;
                                                                calledType = assemblyInstance.GetType(classPath, true);
                                                                break;
                                                            }
                                                    }
                                                }
                                            }
                                            else if (!string.IsNullOrEmpty(assemblyType))
                                            {
                                                // case for external DLL's
                                                var assemblyInstance = Assembly.Load(assemblyType);
                                                calledType = assemblyInstance.GetType(contentEditAction, true);
                                            }
                                            else
                                            {
                                                // case for methods within EonicWeb Core DLL
                                                calledType = Type.GetType(contentEditAction, true);
                                            }

                                            var o = Activator.CreateInstance(calledType);
                                            var args = new object[4];
                                            args[0] = this.myWeb;
                                            args[1] = contentEditActionHandler;
                                            args[2] = nReturnId;
                                            args[3] = editResult;
                                            calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);

                                            // Error Handling ?
                                            // Object Clearup ?

                                            calledType = null;

                                            // Update again ?
                                            base.Instance.SelectSingleNode("*/nContentPrimaryId").InnerText = 0.ToString();
                                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance, nReturnId);
                                        }
                                        catch (Exception ex)
                                        {
                                            // OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo))
                                            cProcessInfo = assemblyName + "." + contentEditAction + " not found";
                                        }
                                    }
                                }

                                if (bPreviewRedirect)
                                {
                                    long VerId = 0L;
                                    this.myWeb.msRedirectOnEnd = "/?ewCmd=PreviewOn&pgid=" + pgid + "&artid=" + id + "&verId=" + nReturnId;
                                }
                            }
                        }
                        else if (isSubmittedOther((int)pgid)) // has another specific submit button been pressed?
                        {
                            // This should really be taken over using  xForms Triggers
                            base.updateInstanceFromRequest();
                            if (Conversions.ToBoolean(Operators.OrObject(this.goSession["mcRelRedirectString"] is object, !Operators.ConditionalCompareObjectEqual(this.goSession["mcRelRedirectString"], "", false))))
                            {
                                base.validate();
                                if (base.valid)
                                {
                                    this.myWeb.msRedirectOnEnd = Conversions.ToString(this.goSession["mcRelRedirectString"]);
                                    base.valid = false;
                                }
                            }
                            else
                            {
                                // we are re-ordering so we don't want a valid form
                                base.valid = false;
                            }

                            this.goSession["oContentInstance"] = null;
                        }
                        else if (base.isTriggered)
                        {
                            // we have clicked a trigger so we must update the instance
                            base.updateInstanceFromRequest();
                            // lets save the instance
                            this.goSession["oContentInstance"] = base.Instance;
                        }
                        else
                        {
                            // clear this if we are loading the first form
                            this.goSession["oContentInstance"] = null;
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditContent", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public virtual void xFrmEditContentPostBuildProcessing(string cContentSchemaName)
                {
                    // Holding function for adding proecssing after building the form.
                }

                public virtual void xFrmEditContentSubmissionPreProcessing()
                {
                    // Holding function for adding pre proecssing on submitting this form.
                }

                // Check if a specific button has been pressed
                public bool isSubmittedOther(int pgid = 0)
                {
                    int nRelId;
                    var nParId = default(int);
                    // Dim oDbh As New dbHelper(myWeb)
                    bool bResult = false;
                    try
                    {
                        XmlElement oTmpNode = (XmlElement)this.moXformElmt.SelectSingleNode("model/instance/tblContent/nContentKey");
                        if (oTmpNode is object)
                        {
                            if (Information.IsNumeric(oTmpNode.InnerText))
                                nParId = Conversions.ToInteger(oTmpNode.InnerText);
                            foreach (var myItem in this.goRequest.Form.Keys)
                            {
                                // ok, we need to check through all the things that would require a save first, 
                                // save, then do the action
                                // ###############################-SAVE IF NEEDED-########################
                                if (Conversions.ToBoolean(Operators.OrObject(myItem.startswith("Relate"), myItem.startswith("ewSubmitClone_Relate"))))
                                {
                                    // if it has no id then its a new piece of content
                                    // we need to check and save
                                    // If nParId = 0 Then
                                    var bCascade = default(bool);
                                    base.updateInstanceFromRequest();
                                    base.validate();
                                    if (base.valid)
                                    {
                                        // trim the contentName to no longer than 255 chars
                                        base.Instance.SelectSingleNode("*/cContentName").InnerXml = Strings.Left(base.Instance.SelectSingleNode("*/cContentName").InnerXml, 255);
                                        if (base.Instance.SelectSingleNode("*/bCascade").InnerXml == "true")
                                        {
                                            bCascade = true;
                                        }

                                        if (nParId > 0)
                                        {
                                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance);
                                        }
                                        else
                                        {
                                            nParId = Conversions.ToInteger(moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance));
                                            moDbHelper.setContentLocation(pgid, nParId, true, bCascade);
                                            if (Conversions.ToBoolean(Operators.OrObject(Operators.ConditionalCompareObjectEqual(this.goSession["mcRelAction"], "Add", false), Operators.ConditionalCompareObjectEqual(this.goSession["mcRelAction"], "Find", false))))
                                            {
                                                moDbHelper.insertContentRelation(Conversions.ToInteger(this.goSession["mcRelParent"]), nParId.ToString());
                                            }
                                        }
                                    }
                                    // End If
                                    // now there should be an id if all is well
                                    if (nParId == 0)
                                        return false;

                                    // remove ewSubmitClone because it gets added by js disablebutton
                                    var relateCmdArr = Strings.Split(Strings.Replace(Conversions.ToString(myItem), "ewSubmitClone_", ""), "_");

                                    // ###############################-REORDER-########################
                                    this.goSession["mnContentRelationParent"] = null;
                                    this.goSession["mcRelRedirectString"] = null;
                                    this.goSession["mcRelAction"] = null;
                                    this.goSession["mcRelParent"] = null;
                                    this.goSession["mcRelType"] = null;
                                    string pgidQueryString = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(this.goRequest.QueryString["pgid"]), "", "&pgid=" + this.goRequest.QueryString["pgid"]));
                                    if (Conversions.ToBoolean(myItem.contains("RelateUp")))
                                    {
                                        nRelId = Conversions.ToInteger(relateCmdArr[1]);
                                        this.myWeb.moDbHelper.ReorderContent(nParId, nRelId, "MoveUp", true);
                                        bResult = true;
                                    }
                                    else if (Conversions.ToBoolean(myItem.contains("RelateDown")))
                                    {
                                        nRelId = Conversions.ToInteger(relateCmdArr[1]);
                                        this.myWeb.moDbHelper.ReorderContent(nParId, nRelId, "MoveDown", true);
                                        bResult = true;
                                    }
                                    else if (Conversions.ToBoolean(myItem.contains("RelateTop")))
                                    {
                                        nRelId = Conversions.ToInteger(relateCmdArr[1]);
                                        this.myWeb.moDbHelper.ReorderContent(nParId, nRelId, "MoveTop", true);
                                        bResult = true;
                                    }
                                    else if (Conversions.ToBoolean(myItem.contains("RelateBottom")))
                                    {
                                        nRelId = Conversions.ToInteger(relateCmdArr[1]);
                                        this.myWeb.moDbHelper.ReorderContent(nParId, nRelId, "MoveBottom", true);
                                        bResult = true;
                                    }
                                    // ###############################-ACTIONS-########################
                                    else if (Conversions.ToBoolean(myItem.contains("RelateEdit")))
                                    {
                                        nRelId = Conversions.ToInteger(relateCmdArr[1]);
                                        this.goSession["mnContentRelationParent"] = Operators.ConcatenateObject("/" + this.myWeb.moConfig["ProjectPath"] + this.goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nParId, Interaction.IIf(string.IsNullOrEmpty(this.goRequest.QueryString["pgid"]), "", "&pgid=" + this.goRequest.QueryString["pgid"]));
                                        this.goSession["mcRelRedirectString"] = "/" + this.myWeb.moConfig["ProjectPath"] + this.goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nRelId;
                                        bResult = true;
                                        break;
                                    }
                                    else if (Conversions.ToBoolean(myItem.contains("RelateRemove")))
                                    {
                                        nRelId = Conversions.ToInteger(relateCmdArr[1]);
                                        this.myWeb.moDbHelper.RemoveContentRelation(nParId, nRelId);
                                        bResult = true;
                                        break;
                                    }
                                    else if (Conversions.ToBoolean(myItem.contains("RelateAdd")))
                                    {
                                        this.goSession["mnContentRelationParent"] = "/" + this.myWeb.moConfig["ProjectPath"] + this.goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nParId + pgidQueryString;
                                        string cContentType = relateCmdArr[1];
                                        if (relateCmdArr.Length > 3)
                                        {
                                            this.goSession["mcRelRedirectString"] = "/" + this.myWeb.moConfig["ProjectPath"] + this.goRequest.QueryString["Path"] + "?ewCmd=AddContent&type=" + cContentType + "&name=New+" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + "&relationType=" + relateCmdArr[3] + pgidQueryString;
                                        }
                                        else
                                        {
                                            this.goSession["mcRelRedirectString"] = "/" + this.myWeb.moConfig["ProjectPath"] + this.goRequest.QueryString["Path"] + "?ewCmd=AddContent&type=" + cContentType + "&name=New+" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + pgidQueryString;
                                        }

                                        this.goSession["mcRelAction"] = "Add";
                                        this.goSession["mcRelParent"] = nParId;
                                        bResult = true;
                                        break;
                                    }
                                    else if (Conversions.ToBoolean(myItem.contains("RelateFind")))
                                    {
                                        this.goSession["mnContentRelationParent"] = "/" + this.myWeb.moConfig["ProjectPath"] + this.goRequest.QueryString["Path"] + "?ewCmd=EditContent&id=" + nParId + pgidQueryString;
                                        string cContentType = relateCmdArr[1];
                                        if (relateCmdArr.Length > 3)
                                        {
                                            this.goSession["mcRelRedirectString"] = "/" + this.myWeb.moConfig["ProjectPath"] + this.goRequest.QueryString["Path"] + "?ewCmd=RelateSearch&type=" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + "&relationType=" + relateCmdArr[3] + pgidQueryString;
                                        }
                                        else
                                        {
                                            this.goSession["mcRelRedirectString"] = "/" + this.myWeb.moConfig["ProjectPath"] + this.goRequest.QueryString["Path"] + "?ewCmd=RelateSearch&type=" + cContentType + "&direction=" + relateCmdArr[2] + "&RelType=" + relateCmdArr[2] + pgidQueryString;
                                        }

                                        this.goSession["mcRelAction"] = "Find";
                                        this.goSession["mcRelParent"] = nParId;
                                        bResult = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (bResult)
                        {
                            // Reload Related Content
                            if (base.Instance.SelectSingleNode("ContentRelations") is object)
                            {
                                base.Instance.RemoveChild(base.Instance.SelectSingleNode("ContentRelations"));
                            }

                            XmlElement oCRNode;
                            oCRNode = this.moPageXML.CreateElement("ContentRelations");
                            moDbHelper.addRelatedContent(ref oCRNode, nParId, true);
                            base.Instance.AppendChild(oCRNode);
                        }

                        return bResult;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "addInput", ex, "", "", Protean.stdTools.gbDebug);
                        return default;
                    }
                }

                public XmlElement xFrmDeleteContent(long artid)
                {
                    XmlElement oFrmElmt;
                    string sContentName;
                    string sContentSchemaName;
                    string cProcessInfo = "";
                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = this.moPageXML;
                        sContentName = moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Content, artid);
                        sContentSchemaName = moDbHelper.getContentType((int)artid);
                        base.NewFrm("DeleteContent");
                        base.submission("DeleteContent", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteItem", "", "Delete Content");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this item - \"" + encodeAllHTML(sContentName) + "\"", sClass: "alert-danger");
                        if (sContentSchemaName == "xFormQuiz")
                        {
                            XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "By deleting the Exam you will also delete all the user results from the database \"ARE YOU SURE\" !", sClass: "alert-danger");
                        }

                        base.addSubmit(ref oFrmElmt, "", "Delete " + sContentSchemaName, sClass: "principle btn-danger", sIcon: "fa-trash-o");
                        base.Instance.InnerXml = "<delete/>";
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // remove the relevent content information
                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Content, artid);
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeleteBulkContent(params string[] artid)
                {
                    XmlElement oFrmElmt;
                    string sContentName;
                    string sContentSchemaName;
                    string cProcessInfo = "";
                    string bulkContentName = "";
                    string bulkContentSchemaName = "";
                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = this.moPageXML;
                        base.NewFrm("DeleteContent");
                        base.submission("DeleteContent", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteItem", "", "Delete Content");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete below items ?", sClass: "alert-error");
                        for (int i = 0, loopTo = Information.UBound(artid); i <= loopTo; i++)
                        {
                            sContentName = moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Content, Conversions.ToLong(artid[i]));
                            sContentSchemaName = moDbHelper.getContentType(Conversions.ToInteger(artid[i]));
                            bulkContentName = encodeAllHTML(sContentName);
                            XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, bulkContentName, sClass: "alert-danger");
                            if (sContentSchemaName == "xFormQuiz")
                            {
                                XmlNode argoNode2 = oFrmElmt;
                                base.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, "By deleting the Exam you will also delete all the user results from the database \"ARE YOU SURE\" !", sClass: "alert-danger");
                            }

                            bulkContentSchemaName = encodeAllHTML(sContentSchemaName) + " , ";
                        }

                        bulkContentSchemaName = bulkContentSchemaName.Trim(' ').Trim(',').Trim(' ');
                        base.addSubmit(ref oFrmElmt, "", "Delete Products", sClass: "principle btn-danger", sIcon: "fa-trash-o");
                        base.Instance.InnerXml = "<delete/>";
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // remove the relevent content information
                                for (int i = 0, loopTo1 = Information.UBound(artid); i <= loopTo1; i++)
                                {
                                    sContentName = moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Content, Conversions.ToLong(artid[i]));
                                    sContentSchemaName = moDbHelper.getContentType(Conversions.ToInteger(artid[i]));
                                    moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Content, Conversions.ToLong(artid[i]));
                                }
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeleteFolder(ref string cPath, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse;
                    string cProcessInfo = "";
                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = this.moPageXML;
                        base.NewFrm("DeleteFolder");
                        base.submission("DeleteFolder", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Delete Content");
                        if (string.IsNullOrEmpty(cPath) | cPath == @"\" | cPath == "/")
                        {
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "You cannot delete the root folder", sClass: "alert-danger");
                        }
                        else
                        {
                            XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this folder? - \"" + cPath + "\"", sClass: "alert-danger");
                            base.addSubmit(ref oFrmElmt, "", "Delete folder");
                        }

                        base.Instance.InnerXml = "<delete/>";
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (string.IsNullOrEmpty(this.goRequest["cFolderName"]) | this.goRequest["cFolderName"] == @"\" | this.goRequest["cFolderName"] == "/")
                            {
                                base.valid = false;
                            }

                            if (base.valid)
                            {
                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);
                                sValidResponse = oFs.DeleteFolder(this.goRequest["cFolderName"], cPath);

                                // fsh.DeleteFolder()
                                // cPath = Left(cPath, InStrRev(cPath, "\") - 1)
                                if (sValidResponse != "1")
                                {
                                    base.valid = false;
                                    XmlNode argoNode2 = oFrmElmt;
                                    base.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, sValidResponse);
                                    base.addValues();
                                }
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmDeleteFolder", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeleteFile(string cPath, string cName, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse;
                    string cProcessInfo = "";
                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = this.moPageXML;
                        base.NewFrm("DeleteFile");
                        base.submission("DeleteFile", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Delete File");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "This file is used in these content Items");
                        // search for file in content and pages
                        var oFsh = new Protean.fsHelper();
                        oFsh.initialiseVariables(nType);
                        string fileToFind = "/" + oFsh.mcRoot + cPath.Replace(@"\", "/") + "/" + cName;
                        string sSQL = "select * from tblContent where cContentXmlBrief like '%" + fileToFind + "%' or cContentXmlDetail like '%" + fileToFind + "%'";
                        var odr = moDbHelper.getDataReader(sSQL);
                        if (odr.HasRows)
                        {
                            string contentFound = "<p>This file is used in these content Items</p><ul>";
                            while (odr.Read())
                                contentFound = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(contentFound + "<li><a href=\"?artid=", odr["nContentKey"]), "\" target=\"_new\">"), odr["cContentSchemaName"]), " - "), odr["cContentName"]), "</a></li>"));
                            XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, contentFound + "</ul>");
                        }
                        else
                        {
                            XmlNode argoNode2 = oFrmElmt;
                            base.addNote(ref argoNode2, Protean.xForm.noteTypes.Hint, "This cannot be found referenced in any content but it may be used in a template or stylesheet");
                        }

                        odr = null;
                        XmlNode argoNode3 = oFrmElmt;
                        base.addNote(ref argoNode3, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this file? - \"" + cPath + @"\" + cName + "\"", sClass: "alert-danger");
                        base.addSubmit(ref oFrmElmt, "", "Delete file");
                        base.Instance.InnerXml = "<delete/>";
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);
                                sValidResponse = oFs.DeleteFile(cPath, cName);
                                if (sValidResponse != "1")
                                {
                                    base.valid = false;
                                    XmlNode argoNode4 = oFrmElmt;
                                    base.addNote(ref argoNode4, Protean.xForm.noteTypes.Alert, sValidResponse);
                                    base.addValues();
                                }
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmMoveFile(string cPath, string cName, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse;
                    string cProcessInfo = "";
                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = this.moPageXML;
                        base.NewFrm("MoveFile");
                        base.submission("MoveFile", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Move File");

                        // search for file in content and pages
                        var oFsh = new Protean.fsHelper();
                        oFsh.initialiseVariables(nType);
                        string fileToFind = "/" + oFsh.mcRoot + cPath.Replace(@"\", "/").Replace("//", "/");
                        if (fileToFind.EndsWith("/"))
                        {
                            fileToFind = fileToFind + cName;
                        }
                        else
                        {
                            fileToFind = fileToFind + "/" + cName;
                        }

                        string sSQL = "select * from tblContent where cContentXmlBrief like '%" + fileToFind + "%' or cContentXmlDetail like '%" + fileToFind + "%'";
                        var odr = moDbHelper.getDataReader(sSQL);
                        if (odr is null)
                        {
                            XmlNode argoNode = oFrmElmt;
                            base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "This cannot be found referenced in any content but it may be used in a template or stylesheet");
                        }
                        else if (odr.HasRows)
                        {
                            string contentFound = "<p>This file is used in these content Items</p><ul>";
                            string artIds = "";
                            while (odr.Read())
                            {
                                contentFound = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(contentFound + "<li><a href=\"?artid=", odr["nContentKey"]), "\" target=\"_new\">"), odr["cContentSchemaName"]), " - "), odr["cContentName"]), "</a></li>"));
                                artIds = Conversions.ToString(Operators.ConcatenateObject(odr["nContentKey"], ","));
                            }

                            XmlNode argoNode1 = oFrmElmt;
                            base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, contentFound + "</ul>");
                            var oSelUpd = base.addSelect1(ref oFrmElmt, "UpdatePaths", false, "Update Paths", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelUpd, "Yes", artIds.TrimEnd(','));
                            base.addOption(ref oSelUpd, "No", "0");
                        }
                        else
                        {
                            XmlNode argoNode2 = oFrmElmt;
                            base.addNote(ref argoNode2, Protean.xForm.noteTypes.Hint, "This cannot be found referenced in any content but it may be used in a template or stylesheet");
                        }

                        odr = null;
                        var oSelElmt = base.addSelect1(ref oFrmElmt, "destPath", false, "Move To");
                        base.addOptionsFoldersFromDirectory(ref oSelElmt, "/" + oFsh.mcRoot);
                        XmlNode argoNode3 = oFrmElmt;
                        base.addNote(ref argoNode3, Protean.xForm.noteTypes.Alert, "Are you sure you want to move this file? - \"" + cPath + @"\" + cName + "\"", sClass: "alert-danger");
                        base.addSubmit(ref oFrmElmt, "", "Move file");
                        base.Instance.InnerXml = "<delete/>";
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);
                                string cDestPath = this.myWeb.moRequest["destPath"].Replace(oFs.mcRoot, "").Replace("//", "/");
                                if (oFs.MoveFile(cName, cPath, cDestPath))
                                {
                                    if (this.myWeb.moRequest["UpdatePaths"] != "0" & !string.IsNullOrEmpty(this.myWeb.moRequest["UpdatePaths"]))
                                    {
                                        string fileToReplace = "/" + oFs.mcRoot + cDestPath.Replace(@"\", "/") + "/" + cName.Replace(" ", "-");
                                        string sSQLUpd = "Update tblContent set cContentXmlBrief = REPLACE(CAST(cContentXmlBrief AS NVARCHAR(MAX)),'" + fileToFind + "','" + fileToReplace + "'), cContentXmlDetail = REPLACE(CAST(cContentXmlDetail AS NVARCHAR(MAX)),'" + fileToFind + "','" + fileToReplace + "') where nContentKey IN (" + this.myWeb.moRequest["UpdatePaths"] + ")";
                                        moDbHelper.ExeProcessSql(sSQLUpd);
                                    }
                                }
                                else
                                {
                                    base.valid = false;
                                    XmlNode argoNode4 = oFrmElmt;
                                    base.addNote(ref argoNode4, Protean.xForm.noteTypes.Alert, "File move error");
                                    base.addValues();
                                }
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeletePage(long pgid)
                {
                    XmlElement oFrmElmt;
                    string sContentName;
                    string sContentSchemaName = "";
                    string cProcessInfo = "";
                    try
                    {
                        // load the xform to be edited
                        moDbHelper.moPageXml = this.moPageXML;
                        sContentName = moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.ContentStructure, pgid);
                        base.NewFrm("DeletePage");
                        base.submission("DeleteContent", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeletePg", "", "Delete Page");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "<h4>Are you sure you want to delete this page - \"" + encodeAllHTML(sContentName) + "\"</h4><br/><br/>By deleting this page you will also delete <strong>ALL</strong> the child pages beneath <strong>ARE YOU SURE</strong> !", sClass: "alert-danger");
                        base.addSubmit(ref oFrmElmt, "", "Delete Page" + sContentSchemaName, sClass: "btn-danger principle", sIcon: "fa-trash");
                        base.Instance.InnerXml = "<delete/>";
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // remove the relevent content information
                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.ContentStructure, pgid);
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmAddFolder(ref string cPath, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse = "";
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("AddFolder");
                        base.submission("AddFolder", "/?ewcmd=" + this.myWeb.moRequest["ewcmd"] + "&ewCmd2=" + this.myWeb.moRequest["ewCmd2"] + "&pathonly=" + this.myWeb.moRequest["pathonly"] + "&targetForm=" + this.myWeb.moRequest["targetForm"] + "&targetField=" + this.myWeb.moRequest["targetField"], "post", "");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "New Folder", "", "Please enter the folder name");
                        base.addInput(ref oFrmElmt, "fld", true, "Path", "readonly");
                        XmlElement argoBindParent = null;
                        base.addBind("fld", "folder/@path", "false() ", oBindParent: ref argoBindParent);
                        base.addInput(ref oFrmElmt, "cFolderName", true, "Folder Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cFolderName", "folder/@name", "true()", oBindParent: ref argoBindParent1);
                        base.addSubmit(ref oFrmElmt, "AddFolder", "Create Folder", "ewSubmit");
                        base.Instance.InnerXml = "<folder path=\"" + cPath + "\" name=\"\"/>";
                        if (base.isSubmitted() | !string.IsNullOrEmpty(moRequest["cFolderName"]))
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);
                                sValidResponse = oFs.CreateFolder(System.Web.HttpUtility.HtmlDecode(this.goRequest["cFolderName"]), cPath);
                                if (Information.IsNumeric(sValidResponse))
                                {
                                    this.valid = true;
                                    cPath += @"\" + this.goRequest["cFolderName"];
                                }
                                else
                                {
                                    this.valid = false;
                                    XmlNode argoNode = (XmlNode)this.moXformElmt;
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, sValidResponse);
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
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmAddFolder", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmUpload(string cPath, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse = "";
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("UploadFile");
                        base.submission("Upload File", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "New File", "", "Please select the file to upload");
                        base.addInput(ref oFrmElmt, "fld", true, "Upload Path", "readonly");
                        XmlElement argoBindParent = null;
                        base.addBind("fld", "file/@path", "true()", oBindParent: ref argoBindParent);
                        string argsClass = "";
                        base.addUpload(ref oFrmElmt, "uploadFile", true, "image/*", "Pick File", sClass: ref argsClass);
                        XmlElement argoBindParent1 = null;
                        base.addBind("uploadFile", "file", "true()", oBindParent: ref argoBindParent1);
                        base.addSubmit(ref oFrmElmt, "", "Upload", "ewSubmit");
                        base.Instance.InnerXml = "<file path=\"" + cPath + "\" filename=\"\" mediatype=\"\"/>";
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();

                            // lets do some hacking 
                            System.Web.HttpPostedFile fUpld;
                            fUpld = this.goRequest.Files["uploadFile"];
                            if (fUpld is object)
                            {
                                base.valid = true;
                            }

                            if (base.valid)
                            {
                                var oFs = new Protean.fsHelper();
                                oFs.initialiseVariables(nType);
                                sValidResponse = oFs.SaveFile(ref fUpld, cPath);
                                if ((sValidResponse ?? "") == (fUpld.FileName ?? ""))
                                {
                                    this.valid = true;
                                }
                                // MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse & " - File Saved")
                                else
                                {
                                    this.valid = false;
                                    XmlNode argoNode = (XmlNode)this.moXformElmt;
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, sValidResponse);
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
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmUpload", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmMultiUpload(string cPath, Protean.fsHelper.LibraryType nType)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse = "";
                    string cProcessInfo = "";
                    string rootDir = @"\";
                    try
                    {
                        switch (nType)
                        {
                            case Protean.fsHelper.LibraryType.Image:
                                {
                                    rootDir = this.myWeb.moConfig["ImageRootPath"];
                                    break;
                                }

                            case Protean.fsHelper.LibraryType.Documents:
                                {
                                    rootDir = this.myWeb.moConfig["DocRootPath"];
                                    break;
                                }

                            case Protean.fsHelper.LibraryType.Media:
                                {
                                    rootDir = this.myWeb.moConfig["MediaRootPath"];
                                    break;
                                }
                        }

                        this.myWeb.moSession["allowUpload"] = "True";
                        base.NewFrm("UploadFile");
                        base.submission("Upload File", "", "post", "");
                        cPath = cPath.Replace(@"\", "/");
                        if (cPath.StartsWith("/"))
                        {
                            cPath = cPath.Substring(1);
                        }

                        string SavePath = rootDir + cPath;
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "New File", "", "Please select files to upload to " + SavePath);
                        base.addInput(ref oFrmElmt, "fld", true, "Upload Path", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("fld", "file/@path", "true()", oBindParent: ref argoBindParent);
                        string argsClass = "MultiPowUpload";
                        base.addUpload(ref oFrmElmt, "uploadFile", true, "image/*", "", ref argsClass);
                        XmlElement argoBindParent1 = null;
                        base.addBind("uploadFile", "file", "true()", oBindParent: ref argoBindParent1);
                        base.addSubmit(ref oFrmElmt, "", "Finish", "ewSubmit");
                        base.Instance.InnerXml = "<file path=\"" + SavePath + "\" filename=\"\" mediatype=\"\"/>";
                        if (base.isSubmitted())
                        {

                            // do nothing
                            this.valid = true;
                            this.myWeb.moSession["allowUpload"] = null;
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmMultiUpload", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmPickImage(string cPathName, string cTargetForm, string cTargetFeild, string cClassName = "")
                {
                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    string sValidResponse = "0";
                    string cProcessInfo = "";
                    try
                    {
                        if (string.IsNullOrEmpty(cTargetForm))
                            cTargetForm = "ContentForm";
                        base.NewFrm("AddFolder");
                        base.submission("imageDetailsForm", "", "post", "form_check(this);passImgToForm('" + cTargetForm + "','" + cTargetFeild + "');return(false);");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Image Details", "", "Please enter image description");
                        base.addInput(ref oFrmElmt, "cName", true, "Class", "readonly");
                        XmlElement argoBindParent = null;
                        base.addBind("cName", "img/@class", "true()", oBindParent: ref argoBindParent);
                        base.addInput(ref oFrmElmt, "cPathName", true, "Path Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cPathName", "img/@src", "true()", oBindParent: ref argoBindParent1);
                        base.addInput(ref oFrmElmt, "nWidth", true, "Width");
                        XmlElement argoBindParent2 = null;
                        base.addBind("nWidth", "img/@width", "true()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oFrmElmt, "nHeight", true, "Height");
                        XmlElement argoBindParent3 = null;
                        base.addBind("nHeight", "img/@height", "true()", oBindParent: ref argoBindParent3);
                        base.addInput(ref oFrmElmt, "cDesc", true, "Alt Description");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cDesc", "img/@alt", "false()", oBindParent: ref argoBindParent4);
                        base.addSubmit(ref oFrmElmt, "", "Add Image", "ewSubmit");
                        var oFs = new Protean.fsHelper();
                        oFs.initialiseVariables(Protean.fsHelper.LibraryType.Image);
                        base.Instance.InnerXml = oFs.getImageXhtml(cPathName);
                        if (!string.IsNullOrEmpty(cClassName))
                        {
                            oElmt = (XmlElement)base.Instance.FirstChild;
                            oElmt.SetAttribute("class", cClassName);
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                if (Information.IsNumeric(sValidResponse))
                                {
                                    this.valid = true;
                                }
                                // MyBase.jsOnLoad()
                                else
                                {
                                    this.valid = false;
                                    XmlNode argoNode = (XmlNode)this.moXformElmt;
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, sValidResponse);
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
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmPickImage", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                // Public Function xFrmPickDocument(ByVal cPathName As String, ByVal cTargetFeild As String, Optional ByVal cClassName As String = "") As XmlElement
                // Dim oFrmElmt As XmlElement
                // Dim oElmt As XmlElement
                // Dim sValidResponse As String = ""
                // Dim cProcessInfo As String = ""
                // Try
                // MyBase.NewFrm("AddFolder")

                // MyBase.submission("documentDetailsForm", "", "post", "form_check(this);passDocToForm('" & cTargetFeild & "');return(false);")

                // oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Document Details", "", "Please enter image description")

                // MyBase.addInput(oFrmElmt, "cPathName", True, "Path Name")
                // MyBase.addBind("cPathName", "a/@href", "true()")

                // MyBase.addSubmit(oFrmElmt, "", "Add Document ", "ewSubmit", "ewSubmit")


                // MyBase.instance.InnerXml = "<a href=""" & Replace(cPathName, "\", "/") & """/>"


                // If cClassName <> "" Then
                // oElmt = MyBase.instance.FirstChild()
                // oElmt.SetAttribute("class", cClassName)
                // End If

                // 'auto submit-
                // MyBase.submit()


                // '------------

                // If MyBase.isSubmitted Then
                // MyBase.updateInstanceFromRequest()
                // MyBase.validate()
                // If MyBase.valid Then

                // If IsNumeric(sValidResponse) Then
                // valid = True
                // 'MyBase.jsOnLoad()
                // Else
                // valid = False
                // MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
                // End If
                // Else
                // valid = False
                // End If
                // End If

                // MyBase.addValues()
                // Return MyBase.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                public XmlElement xFrmEditImage(string cImgHtml, string cTargetForm, string cTargetFeild, string cClassName = "")
                {
                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    string sValidResponse = "";
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("EditImage");
                        base.Instance.InnerXml = cImgHtml.Replace("\">", "\"/>").Replace("&", "&amp;");
                        if (string.IsNullOrEmpty(cClassName))
                        {
                            oElmt = (XmlElement)base.Instance.FirstChild;
                            cClassName = oElmt.GetAttribute("class");
                        }

                        base.submission("imageDetailsForm", "", "post", "form_check(this);passImgToForm('" + cTargetForm + "','" + cTargetFeild + "');return(false);");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Image Details", "", "Please enter image description");
                        base.addInput(ref oFrmElmt, "cName", true, "Class", "readonly");
                        XmlElement argoBindParent = null;
                        base.addBind("cName", "img/@class", "true()", oBindParent: ref argoBindParent);
                        base.addInput(ref oFrmElmt, "cPathName", true, "Path Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cPathName", "img/@src", "true()", oBindParent: ref argoBindParent1);
                        base.addInput(ref oFrmElmt, "nWidth", true, "Width");
                        XmlElement argoBindParent2 = null;
                        base.addBind("nWidth", "img/@width", "true()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oFrmElmt, "nHeight", true, "Height");
                        XmlElement argoBindParent3 = null;
                        base.addBind("nHeight", "img/@height", "true()", oBindParent: ref argoBindParent3);
                        base.addInput(ref oFrmElmt, "cDesc", true, "Alt Description");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cDesc", "img/@alt", "false()", oBindParent: ref argoBindParent4);
                        base.addDiv(ref oFrmElmt, "<a href=\"?contentType=popup&amp;ewCmd=ImageLib&amp;targetField=" + cTargetFeild + "&amp;targetClass=" + cClassName + "\" class=\"btn btn-primary pull-right\"><i class=\"fa fa-picture-o\"> </i> Pick New Image</a>", "");
                        base.addSubmit(ref oFrmElmt, "", "Update Image", "ewSubmit", "ewSubmit");
                        if (!string.IsNullOrEmpty(cClassName))
                        {
                            oElmt = (XmlElement)base.Instance.FirstChild;
                            oElmt.SetAttribute("class", cClassName);
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                if (Information.IsNumeric(sValidResponse))
                                {
                                    this.valid = true;
                                }
                                // MyBase.jsOnLoad()
                                else
                                {
                                    this.valid = false;
                                    XmlNode argoNode = (XmlNode)this.moXformElmt;
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, sValidResponse);
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
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditImage", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }


                /// <summary>
            /// this routine now calls the Membership provider for this function as it has moved. It should only be being used by legacy overides.
            /// </summary>
            /// <param name="id"></param>
            /// <param name="cDirectorySchemaName"></param>
            /// <param name="parId"></param>
            /// <param name="cXformName"></param>
            /// <param name="FormXML"></param>
            /// <returns></returns>
            /// <remarks></remarks>

                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmEditDirectoryItem(long id = 0L, string cDirectorySchemaName = "User", long parId = 0L, string cXformName = "", string FormXML = "")
                {
                    string cProcessInfo = "";
                    try
                    {
                        object argmyWeb = (object)this.myWeb;
                        var oMembershipProv = new Protean.Providers.Membership.BaseProvider(ref argmyWeb, this.myWeb.moConfig["MembershipProvider"]);
                        var oAdXfm = oMembershipProv.AdminXforms;
                        oAdXfm.xFrmEditDirectoryItem(id, cDirectorySchemaName, parId, cXformName, FormXML);
                        this.valid = Conversions.ToBoolean(oAdXfm.valid);
                        this.moXformElmt = (XmlElement)oAdXfm.moXformElmt;
                        this.updateInstance((XmlElement)oAdXfm.Instance);
                        return this.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditDirectoryItem", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public virtual XmlElement xFrmCopyGroupMembers(long dirId)
                {
                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    XmlElement oElmt2;
                    XmlElement oElmt3;
                    string sSql;
                    SqlDataReader oDr;
                    string cProcessInfo = "";
                    string sType = "Group";
                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = this.moPageXML;
                        base.NewFrm("CopyGroupMembers");

                        // Lets get the object
                        oElmt = this.moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", dirId.ToString());
                        if (dirId != 0L)
                        {
                            oDr = moDbHelper.getDataReader("SELECT * FROM tblDirectory where nDirKey = " + dirId);
                            while (oDr.Read())
                            {
                                oElmt.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDr["cDirXml"], "", false)))
                                {
                                    oElmt.InnerXml = Conversions.ToString(oDr["cDirXml"]);
                                }
                                else
                                {
                                    oElmt.InnerXml = this.Instance.SelectSingleNode("*").InnerXml;
                                }
                            }

                            oDr.Close();
                            oDr = null;

                            // get item parents
                            sSql = "SELECT d.* FROM tblDirectory d " + "inner join tblDirectoryRelation r on r.nDirParentId = d.nDirKey " + "where r.nDirChildId = " + dirId;
                            oDr = moDbHelper.getDataReader(sSql);
                            while (oDr.Read())
                            {
                                oElmt2 = this.moPageXML.CreateElement(Conversions.ToString(oDr["cDirSchema"]));
                                oElmt2.SetAttribute("id", Conversions.ToString(oDr["nDirKey"]));
                                oElmt2.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                oElmt2.SetAttribute("relType", "child");
                                base.Instance.AppendChild(oElmt2);
                            }

                            oDr.Close();
                            oDr = null;

                            // get item Children
                            sSql = "SELECT d.* FROM tblDirectory d " + "inner join tblDirectoryRelation r on r.nDirChildId = d.nDirKey " + "where r.nDirParentId = " + dirId;
                            oDr = moDbHelper.getDataReader(sSql);
                            while (oDr.Read())
                            {
                                oElmt3 = this.moPageXML.CreateElement(Conversions.ToString(oDr["cDirSchema"]));
                                oElmt3.SetAttribute("id", Conversions.ToString(oDr["nDirKey"]));
                                oElmt3.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                oElmt3.SetAttribute("relType", "parent");
                                base.Instance.AppendChild(oElmt3);
                            }

                            oDr.Close();
                            oDr = null;
                        }

                        base.Instance.AppendChild(oElmt);
                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "MoveDirMembers", "", "Copy " + sType + " Members");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Are you sure you want to copy this " + sType + " Members " + encodeAllHTML(oElmt.GetAttribute("name")), sClass: "alert-danger");

                        // Lets get all other groups
                        oElmt3 = base.addSelect1(ref oFrmElmt, sType + "CopyTo", false, "Copy " + sType + " Members To", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='" + sType + "' and d.nDirKey<>" + dirId + " order by cDirName";
                        var argoDr = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt3, ref argoDr, "name", "value");
                        base.addSubmit(ref oFrmElmt, "", "Copy " + sType);
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // select all child relations so child objects don't get deleted
                                sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " + dirId;
                                oDr = moDbHelper.getDataReader(sSql);
                                // Loop through 1 behind so we can trigger sync on last one.
                                long previousId = 0L;
                                while (oDr.Read())
                                {
                                    if (!(previousId == 0L))
                                    {
                                        moDbHelper.maintainDirectoryRelation(Conversions.ToLong(moRequest["GroupCopyTo"]), previousId, false, bIfExistsDontUpdate: true, isLast: false);
                                    }

                                    previousId = Conversions.ToLong(oDr[1]);
                                }

                                moDbHelper.maintainDirectoryRelation(Conversions.ToLong(moRequest["GroupCopyTo"]), previousId, false, bIfExistsDontUpdate: true, isLast: true);
                                oDr.Close();
                                oDr = null;
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmCopyGroupMembers", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public virtual XmlElement xFrmEditRole(long id)
                {
                    string cProcessInfo = "";
                    string cCurrentPassword = "";
                    string cCodeUsed = "";
                    bool addNewitemToParId = false;
                    string cDirectorySchemaName = "role";
                    string cXformName = "";
                    XmlElement oElmt;
                    try
                    {
                        if (id > 0L)
                        {
                            if (string.IsNullOrEmpty(cXformName))
                                cXformName = cDirectorySchemaName;

                            // ok lets load in an xform from the file location.

                            if (!base.load("/xforms/directory/" + cXformName + ".xml", this.myWeb.maCommonFolders))
                            {
                                // load a default content xform if no alternative.

                            }

                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Directory, id);
                            XmlElement oRoleRights = (XmlElement)base.Instance.SelectSingleNode("tblDirectory/cDirXml/Role/AdminRights");
                            XmlElement siteRights = (XmlElement)this.moPageXML.SelectSingleNode("/Page/AdminMenu");
                            if (oRoleRights is null)
                            {
                                oRoleRights = this.moPageXML.CreateElement("AdminRights");
                                oRoleRights.InnerXml = siteRights.InnerXml;
                                foreach (XmlElement currentOElmt in oRoleRights.SelectNodes("descendant-or-self::MenuItem"))
                                {
                                    oElmt = currentOElmt;
                                    oElmt.SetAttribute("adminRight", "");
                                }

                                base.Instance.SelectSingleNode("tblDirectory/cDirXml/Role").AppendChild(oRoleRights);
                            }
                            else
                            {
                                XmlElement oRoleRights2;
                                oRoleRights2 = this.moPageXML.CreateElement("AdminRights");
                                oRoleRights2.InnerXml = siteRights.InnerXml;
                                foreach (XmlElement currentOElmt1 in oRoleRights2.SelectNodes("descendant-or-self::MenuItem"))
                                {
                                    oElmt = currentOElmt1;
                                    if (oRoleRights.SelectSingleNode("descendant-or-self::MenuItem[@cmd='" + oElmt.GetAttribute("cmd") + "' and @adminRight='true']") is object)
                                    {
                                        oElmt.SetAttribute("adminRight", "true");
                                    }
                                    else
                                    {
                                        oElmt.SetAttribute("adminRight", "");
                                    }
                                }
                                // remove the old admin rights
                                foreach (XmlElement currentOElmt2 in this.Instance.SelectNodes("tblDirectory/cDirXml/Role/AdminRights"))
                                {
                                    oElmt = currentOElmt2;
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                }
                                // paste in new and updated
                                base.Instance.SelectSingleNode("tblDirectory/cDirXml/Role").AppendChild(oRoleRights2);
                            }

                            oRoleRights = (XmlElement)base.Instance.SelectSingleNode("tblDirectory/cDirXml/Role/AdminRights");
                            XmlElement oSelElmt;
                            XmlElement oGrpRoot = (XmlElement)this.moXformElmt.SelectSingleNode("group[@class='2Col']/group[2]");
                            if (oGrpRoot is null)
                                oGrpRoot = this.moXformElmt;
                            var oGrp = base.addGroup(ref oGrpRoot, "adminRights", "adminRights", "Admin Rights");
                            XmlElement oGrp2;
                            XmlElement oGrp3;
                            XmlElement oGrp4;
                            XmlElement oGrp5;
                            long nCount = 1L;
                            string sRef;
                            XmlElement oBind;
                            XmlElement oBind1;
                            XmlElement oBind2;
                            XmlElement oBind3;
                            XmlElement oBind4;
                            foreach (XmlElement currentOElmt3 in oRoleRights.SelectNodes("MenuItem"))
                            {
                                oElmt = currentOElmt3;
                                sRef = "adminRight" + nCount;
                                oSelElmt = base.addSelect(ref oGrp, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                base.addOption(ref oSelElmt, oElmt.GetAttribute("name"), "true");
                                XmlElement argoBindParent = null;
                                oBind = base.addBind("", "tblDirectory/cDirXml/Role/AdminRights/MenuItem[@cmd='" + oElmt.GetAttribute("cmd") + "']", oBindParent: ref argoBindParent);
                                base.addBind(sRef, "@adminRight", oBindParent: ref oBind);
                                nCount = nCount + 1L;
                                if (oElmt.SelectNodes("MenuItem").Count > 0)
                                {
                                    oGrp2 = base.addGroup(ref oGrp, oElmt.GetAttribute("name"), "grp" + nCount);
                                    foreach (XmlElement oElmt2 in oElmt.SelectNodes("MenuItem"))
                                    {
                                        sRef = "adminRight" + nCount;
                                        oSelElmt = base.addSelect(ref oGrp2, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                        base.addOption(ref oSelElmt, oElmt2.GetAttribute("name"), "true");
                                        oBind1 = base.addBind("", "MenuItem[@cmd='" + oElmt2.GetAttribute("cmd") + "']", oBindParent: ref oBind);
                                        base.addBind(sRef, "@adminRight", oBindParent: ref oBind1);
                                        nCount = nCount + 1L;
                                        if (oElmt2.SelectNodes("MenuItem").Count > 0)
                                        {
                                            oGrp3 = base.addGroup(ref oGrp2, oElmt2.GetAttribute("name"), "grp" + nCount);
                                            foreach (XmlElement oElmt3 in oElmt2.SelectNodes("MenuItem"))
                                            {
                                                sRef = "adminRight" + nCount;
                                                oSelElmt = base.addSelect(ref oGrp3, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                                base.addOption(ref oSelElmt, oElmt3.GetAttribute("name"), "true");
                                                oBind2 = base.addBind("", "MenuItem[@cmd='" + oElmt3.GetAttribute("cmd") + "']", oBindParent: ref oBind1);
                                                base.addBind(sRef, "@adminRight", oBindParent: ref oBind2);
                                                nCount = nCount + 1L;
                                                if (oElmt3.SelectNodes("MenuItem").Count > 0)
                                                {
                                                    oGrp4 = base.addGroup(ref oGrp3, oElmt3.GetAttribute("name"), "grp" + nCount);
                                                    foreach (XmlElement oElmt4 in oElmt3.SelectNodes("MenuItem"))
                                                    {
                                                        sRef = "adminRight" + nCount;
                                                        oSelElmt = base.addSelect(ref oGrp4, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                                        base.addOption(ref oSelElmt, oElmt4.GetAttribute("name"), "true");
                                                        oBind3 = base.addBind("", "MenuItem[@cmd='" + oElmt4.GetAttribute("cmd") + "']", oBindParent: ref oBind2);
                                                        base.addBind(sRef, "@adminRight", oBindParent: ref oBind3);
                                                        nCount = nCount + 1L;
                                                        if (oElmt4.SelectNodes("MenuItem").Count > 0)
                                                        {
                                                            oGrp5 = base.addGroup(ref oGrp4, oElmt4.GetAttribute("name"), "grp" + nCount);
                                                            foreach (XmlElement oElmt5 in oElmt4.SelectNodes("MenuItem"))
                                                            {
                                                                sRef = "adminRight" + nCount;
                                                                oSelElmt = base.addSelect(ref oGrp5, sRef, true, "", nAppearance: Protean.xForm.ApperanceTypes.Full);
                                                                base.addOption(ref oSelElmt, oElmt5.GetAttribute("name"), "true");
                                                                oBind4 = base.addBind("", "MenuItem[@cmd='" + oElmt5.GetAttribute("cmd") + "']", oBindParent: ref oBind3);
                                                                base.addBind(sRef, "@adminRight", oBindParent: ref oBind4);
                                                                nCount = nCount + 1L;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            cDirectorySchemaName = base.Instance.SelectSingleNode("tblDirectory/cDirSchema").InnerText;
                            if (base.isSubmitted())
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                // any additonal validation goes here

                                if (base.valid)
                                {
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, base.Instance, id);
                                    base.addNote("EditContent", Protean.xForm.noteTypes.Alert, "<span class=\"msg-1010\">Your details have been updated.</span>", true);
                                }
                            }

                            base.addValues();
                            return base.moXformElmt;
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditRole", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }

                    return default;
                }

                public virtual XmlElement xFrmDeleteDirectoryItem(long dirId, string sType)
                {
                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    XmlElement oElmt2;
                    XmlElement oElmt3;
                    string sSql;
                    SqlDataReader oDr;
                    string cProcessInfo = "";
                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = this.moPageXML;
                        base.NewFrm("EditSelect");

                        // Lets get the object
                        oElmt = this.moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", dirId.ToString());
                        if (dirId != 0L)
                        {
                            oDr = moDbHelper.getDataReader("SELECT * FROM tblDirectory where nDirKey = " + dirId);
                            while (oDr.Read())
                            {
                                oElmt.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDr["cDirXml"], "", false)))
                                {
                                    oElmt.InnerXml = Conversions.ToString(oDr["cDirXml"]);
                                }
                                else
                                {
                                    oElmt.InnerXml = this.Instance.SelectSingleNode("*").InnerXml;
                                }
                            }

                            oDr.Close();
                            oDr = null;

                            // get item parents
                            sSql = "SELECT d.* FROM tblDirectory d " + "inner join tblDirectoryRelation r on r.nDirParentId = d.nDirKey " + "where r.nDirChildId = " + dirId;
                            oDr = moDbHelper.getDataReader(sSql);
                            while (oDr.Read())
                            {
                                oElmt2 = this.moPageXML.CreateElement(Conversions.ToString(oDr["cDirSchema"]));
                                oElmt2.SetAttribute("id", Conversions.ToString(oDr["nDirKey"]));
                                oElmt2.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                oElmt2.SetAttribute("relType", "child");
                                base.Instance.AppendChild(oElmt2);
                            }

                            oDr.Close();
                            oDr = null;

                            // get item Children
                            sSql = "SELECT d.* FROM tblDirectory d " + "inner join tblDirectoryRelation r on r.nDirChildId = d.nDirKey " + "where r.nDirParentId = " + dirId;
                            oDr = moDbHelper.getDataReader(sSql);
                            while (oDr.Read())
                            {
                                oElmt3 = this.moPageXML.CreateElement(Conversions.ToString(oDr["cDirSchema"]));
                                oElmt3.SetAttribute("id", Conversions.ToString(oDr["nDirKey"]));
                                oElmt3.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                oElmt3.SetAttribute("relType", "parent");
                                base.Instance.AppendChild(oElmt3);
                            }

                            oDr.Close();
                            oDr = null;
                        }

                        base.Instance.AppendChild(oElmt);
                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteDir", "", "Delete " + sType);
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this " + sType + " " + encodeAllHTML(oElmt.GetAttribute("name")), sClass: "alert-danger");
                        switch (sType ?? "")
                        {
                            case "User":
                                {
                                    break;
                                }
                            // MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "What do you want to do with this " & sType & "s exam results?")

                            // oElmt2 = MyBase.addSelect1(oFrmElmt, "Options", False, "Exam Results", "", xForm.ApperanceTypes.Full)
                            // MyBase.addOption(oElmt2, "Delete All", "Delete")
                            // MyBase.addOption(oElmt2, "Transfer to another user in the same company", "Transfer")

                            // oElmt3 = MyBase.addSelect1(oFrmElmt, "TransUserId", False, "Select User", "scroll_10", xForm.ApperanceTypes.Minimal)

                            // 'Lets get all the users in the same company

                            // sSql = "select d.nDirKey as value, d.cDirName as name " & _
                            // "FROM (((tblDirectory d " & _
                            // "inner join tblAudit a on nAuditId = a.nAuditKey " & _
                            // "INNER JOIN tblDirectoryRelation user2company1 " & _
                            // "ON d.nDirKey = user2company1.nDirChildId) " & _
                            // "INNER JOIN tblDirectory company " & _
                            // "ON company.nDirKey = user2company1.nDirParentId) " & _
                            // "INNER JOIN tblDirectoryRelation user2company2 " & _
                            // "ON company.nDirKey = user2company2.nDirParentId ) " & _
                            // "INNER JOIN tblDirectory users " & _
                            // "ON users.nDirKey = user2company2.nDirChildId " & _
                            // "left outer join tblDirectoryRelation dr on d.nDirKey = dr.nDirParentId and dr.nDirChildId = " & dirId & _
                            // "WHERE d.cDirSchema = 'User' AND company.cDirSchema = 'Company' AND users.cDirSchema = 'User' " & _
                            // "AND users.nDirKey = " & dirId & " and d.nDirKey<>" & dirId & " order by d.cDirName"

                            // MyBase.addOptionsFromSqlDataReader(oElmt3, oDt.getDataReader(sSql), "name", "value")

                            case "Department":
                                {
                                    XmlNode argoNode1 = oFrmElmt;
                                    base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "What do you want to do with this " + sType + "s users?", sClass: "alert-danger");
                                    oElmt2 = base.addSelect1(ref oFrmElmt, "Options", false, "Users", "", Protean.xForm.ApperanceTypes.Full);
                                    base.addOption(ref oElmt2, "Just remove the Department relationship from the Users", "Remove");
                                    base.addOption(ref oElmt2, "Transfer Users to another Department in the same Company", "Transfer");
                                    oElmt3 = base.addSelect1(ref oFrmElmt, "TransDeptId", false, "Select User", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                                    // Lets get all the departments in the same company

                                    sSql = "select d.nDirKey as value, d.cDirName as name " + "FROM (((tblDirectory d " + "inner join tblAudit a on nAuditId = a.nAuditKey " + "INNER JOIN tblDirectoryRelation dept2company1 " + "ON d.nDirKey = dept2company1.nDirChildId) " + "INNER JOIN tblDirectory company " + "ON company.nDirKey = dept2company1.nDirParentId) " + "INNER JOIN tblDirectoryRelation user2company2 " + "ON company.nDirKey = user2company2.nDirParentId ) " + "INNER JOIN tblDirectory dept " + "ON dept.nDirKey = user2company2.nDirChildId " + "left outer join tblDirectoryRelation dr on d.nDirKey = dr.nDirParentId and dr.nDirChildId = " + dirId + "WHERE d.cDirSchema = 'Department' AND company.cDirSchema = 'Company' AND dept.cDirSchema = 'Department' " + "AND dept.nDirKey = " + dirId + " and d.nDirKey<>" + dirId + " order by d.cDirName";
                                    var argoDr = moDbHelper.getDataReader(sSql);
                                    base.addOptionsFromSqlDataReader(ref oElmt3, ref argoDr, "name", "value");
                                    break;
                                }

                            case "Company":
                                {
                                    XmlNode argoNode2 = oFrmElmt;
                                    base.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, "What do you want to do with this " + sType + "s users?", sClass: "alert-danger");
                                    oElmt2 = base.addSelect1(ref oFrmElmt, "Options", false, "Users / Departments", "", Protean.xForm.ApperanceTypes.Full);
                                    base.addOption(ref oElmt2, "Delete All Users/Departments", "Delete");
                                    base.addOption(ref oElmt2, "Just remove the Company relationship from the Users and delete Departments", "RemoveDept");
                                    base.addOption(ref oElmt2, "Transfer Users/Departments to another Company", "Transfer");

                                    // Lets get all other companies
                                    oElmt3 = base.addSelect1(ref oFrmElmt, "TransCompanyId", false, "Select Departments", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                                    sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='Company' and d.nDirKey<>" + dirId + " order by cDirName";
                                    var argoDr1 = moDbHelper.getDataReader(sSql);
                                    base.addOptionsFromSqlDataReader(ref oElmt3, ref argoDr1, "name", "value"); // "Group", "Role"
                                    break;
                                }

                            default:
                                {
                                    oElmt2 = base.addSelect1(ref oFrmElmt, "Options", false, "What do you want to do with this " + sType + "s members?", "", Protean.xForm.ApperanceTypes.Full);
                                    base.addOption(ref oElmt2, "Just remove the " + sType + " relationship from the members", "Remove");
                                    base.addOption(ref oElmt2, "Transfer members to another " + sType, "Transfer");

                                    // Lets get all other groups
                                    oElmt3 = base.addSelect1(ref oFrmElmt, sType + "s", false, "Select Alternative " + sType, "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                                    sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='" + sType + "' and d.nDirKey<>" + dirId + " order by cDirName";
                                    var argoDr2 = moDbHelper.getDataReader(sSql);
                                    base.addOptionsFromSqlDataReader(ref oElmt3, ref argoDr2, "name", "value");
                                    break;
                                }

                                // Case "Role"

                                // oElmt2 = MyBase.addSelect1(oFrmElmt, "Options", False, "What do you want to do with this " & sType & "s users?", "", xForm.ApperanceTypes.Full)
                                // MyBase.addOption(oElmt2, "Just remove the Role relationship from the Users", "Remove")
                                // MyBase.addOption(oElmt2, "Transfer Users to another Role", "Transfer")

                                // 'Lets get all other roles
                                // oElmt3 = MyBase.addSelect1(oFrmElmt, "Roles", False, "Select Roles", "scroll_10", xForm.ApperanceTypes.Minimal)
                                // sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='Role' and d.nDirKey<>" & dirId & " order by cDirName"
                                // MyBase.addOptionsFromSqlDataReader(oElmt3, moDbHelper.getDataReader(sSql), "name", "value")

                        }

                        base.addSubmit(ref oFrmElmt, "", "Delete " + sType);
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // remove the relevent answer information
                                switch (sType ?? "")
                                {
                                    case "User":
                                        {
                                            // Select Case goRequest("Options")
                                            // Case "Transfer"
                                            // sSql = "select nQResultsKey from tblQuestionaireResult where nDirId=" & dirId
                                            // oDr = oDt.getDataReader(sSql)
                                            // While oDr.Read
                                            // sSql = "update tblQuestionaireResult set nDirId = " & goRequest("TransUserId") & " where nDirId=" & dirId
                                            // oDt.exeProcessSQL(sSql)
                                            // End While
                                            // oDr.Close()
                                            // oDr = Nothing
                                            // moDbhelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)
                                            // Case "Delete"
                                            moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                            break;
                                        }
                                    // Case Else
                                    // 'do nothing
                                    // End Select

                                    case "Company":
                                        {
                                            // Delete Departments 
                                            // Delete Departments Directory Relations
                                            // Delete Company Permissions
                                            // Delete Company Users / or move to another Company / or leave oprhaned?
                                            // Move to another department / or leave oprhaned?
                                            switch (this.goRequest["Options"] ?? "")
                                            {
                                                case "RemoveDept":
                                                    {
                                                        sSql = "select r.nRelKey from tblDirectoryRelation r where r.nDirParentId = " + dirId + " inner join tblDirectory d on r.nDirChildId = d.nDirKey " + " where d.cDirSchema = 'User' ";
                                                        oDr = moDbHelper.getDataReader(sSql);
                                                        while (oDr.Read())
                                                            moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                                                        oDr.Close();
                                                        oDr = null;

                                                        // Delete Company Directory Relations, Company Permissions and Company
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                case "Transfer":
                                                    {
                                                        sSql = "update tblDirectoryRelation set nDirParentId = " + this.goRequest["TransCompanyId"] + " where nDirParentId=" + dirId;
                                                        moDbHelper.ExeProcessSql(sSql);

                                                        // Delete Company Directory Relations, Company Permissions and Company
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                case "Delete":
                                                    {
                                                        // Delete Company Directory Relations, Company Permissions and Company
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }
                                            }

                                            break;
                                        }

                                    case "Department":
                                        {

                                            // Move to another department / or leave oprhaned?
                                            switch (this.goRequest["Options"] ?? "")
                                            {
                                                case "Transfer":
                                                    {
                                                        sSql = "update tblDirectoryRelation set nDirParentId = " + this.goRequest["TransDeptId"] + " where nDirParentId=" + dirId;
                                                        moDbHelper.ExeProcessSql(sSql);

                                                        // Delete Department Directory Relations, Department Permissions and Department
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                case "Remove":
                                                    {
                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        oDr = moDbHelper.getDataReader(sSql);
                                                        while (oDr.Read())
                                                            moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                                                        oDr.Close();
                                                        oDr = null;

                                                        // Delete Department Directory Relations, Department Permissions and Department
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        break;
                                                    }
                                                    // do nothing
                                            }

                                            break;
                                        }

                                    case "Group":
                                        {
                                            switch (this.goRequest["Options"] ?? "")
                                            {
                                                case "Transfer":
                                                    {
                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        oDr = moDbHelper.getDataReader(sSql);
                                                        while (oDr.Read())
                                                        {
                                                            moDbHelper.saveDirectoryRelations(Conversions.ToLong(oDr[1]), this.goRequest["Groups"]);
                                                            moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                                                        }

                                                        oDr.Close();
                                                        oDr = null;
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                case "Remove":
                                                    {
                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        oDr = moDbHelper.getDataReader(sSql);
                                                        while (oDr.Read())
                                                            moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                                                        oDr.Close();
                                                        oDr = null;
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        break;
                                                    }
                                                    // do nothing
                                            }

                                            break;
                                        }

                                    case "Role":
                                        {
                                            switch (this.goRequest["Options"] ?? "")
                                            {
                                                case "Transfer":
                                                    {

                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        oDr = moDbHelper.getDataReader(sSql);
                                                        while (oDr.Read())
                                                        {
                                                            moDbHelper.saveDirectoryRelations(Conversions.ToLong(oDr[1]), this.goRequest["Roles"]);
                                                            moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                                                        }

                                                        oDr.Close();
                                                        oDr = null;
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                case "Remove":
                                                    {
                                                        // remove all child relations so child objects don't get deleted
                                                        sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " + dirId;
                                                        oDr = moDbHelper.getDataReader(sSql);
                                                        while (oDr.Read())
                                                            moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.DirectoryRelation, Conversions.ToLong(oDr[0]));
                                                        oDr.Close();
                                                        oDr = null;
                                                        moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Directory, dirId);
                                                        break;
                                                    }

                                                default:
                                                    {
                                                        break;
                                                    }
                                                    // do nothing
                                            }

                                            break;
                                        }
                                }
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeleteDeliveryMethod(long id)
                {
                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    SqlDataReader oDr;
                    string cProcessInfo = "";
                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = this.moPageXML;
                        base.NewFrm("EditDeliveryMethod");

                        // Lets get the object
                        oElmt = this.moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", id.ToString());
                        if (id != 0L)
                        {
                            oDr = moDbHelper.getDataReader("SELECT tblCartShippingMethods.* FROM tblCartShippingMethods WHERE nShipOptKey = " + id);
                            while (oDr.Read())
                                oElmt.SetAttribute("name", Conversions.ToString(oDr["cShipOptName"]));
                            oDr.Close();
                            oDr = null;
                        }

                        base.Instance.AppendChild(oElmt);
                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteDM", "", "Delete Delivery Method");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this Delivery Method: " + oElmt.GetAttribute("name"));
                        base.addSubmit(ref oFrmElmt, "", "Delete Delivery Method");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartShippingMethod, id);
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmDeleteDeliveryMethod", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeleteCarrier(long id)
                {
                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    SqlDataReader oDr;
                    string cProcessInfo = "";
                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = this.moPageXML;
                        base.NewFrm("DeleteCarrier");

                        // Lets get the object
                        oElmt = this.moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", id.ToString());
                        if (id != 0L)
                        {
                            oDr = moDbHelper.getDataReader("SELECT tblCartCarrier.* FROM tblCartCarrier WHERE nCarrierKey = " + id);
                            while (oDr.Read())
                                oElmt.SetAttribute("name", Conversions.ToString(oDr["cCarrierName"]));
                            oDr.Close();
                            oDr = null;
                        }

                        base.Instance.AppendChild(oElmt);
                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteDM", "", "Delete Delivery Method");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this Carrier: " + oElmt.GetAttribute("name"));
                        base.addSubmit(ref oFrmElmt, "", "Delete Carrier");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartCarrier, id);
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmDeleteCarrier", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeleteShippingLocation(long id)
                {
                    XmlElement oFrmElmt;
                    XmlElement oElmt;
                    SqlDataReader oDr;
                    string cProcessInfo = "";
                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = this.moPageXML;
                        base.NewFrm("DeleteShippingLocation");

                        // Lets get the object
                        oElmt = this.moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", id.ToString());
                        if (id != 0L)
                        {
                            oDr = moDbHelper.getDataReader("SELECT cLocationNameFull FROM tblCartShippingLocations WHERE nLocationKey = " + id);
                            while (oDr.Read())
                                oElmt.SetAttribute("name", Conversions.ToString(oDr[0]));
                            oDr.Close();
                            oDr = null;
                        }

                        base.Instance.AppendChild(oElmt);
                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteDM", "", "Delete Shipping Location");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this Shipping Location: " + oElmt.GetAttribute("name") + " and all of its children?");
                        base.addSubmit(ref oFrmElmt, "", "Delete Shipping Location");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartShippingLocation, id);
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmDeleteShippingLocation", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public virtual XmlElement xFrmPagePermissions(long id)
                {
                    string cDirectorySchemas = "Group,Role";
                    string[] aDirectorySchemas;
                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    string sSql;
                    string cProcessInfo = "";
                    try
                    {

                        // Check for schema overloads
                        if (!string.IsNullOrEmpty("" + goConfig["AdminPagePermissionSchemas"]))
                        {
                            cDirectorySchemas = goConfig["AdminPagePermissionSchemas"];
                        }

                        // Split the schema into an array
                        cProcessInfo = "Schemas: " + cDirectorySchemas;
                        aDirectorySchemas = cDirectorySchemas.Split(',');

                        // load the xform to be edited
                        base.NewFrm("EditPagePermissions");
                        base.submission("EditInputPagePermissions", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPermissions", "3col", "Permissions for Page - " + moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.ContentStructure, id));
                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "System User Groups &amp; Roles");

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditPermissions", "PermissionButtons", "Set Selected Group Permissions");
                        // MyBase.addSubmit(oFrmGrp2, "AddAll", "Add All >", "", "PermissionButtons")
                        base.addSubmit(ref oFrmGrp2, "AllowSelected", "Allow Selected", "", "PermissionButton icon-right", "fa-arrow-right");
                        base.addSubmit(ref oFrmGrp2, "DenySelected", "Deny Selected", "", "PermissionButton icon-right", "fa-arrow-right");
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "Remove Selected", "", "PermissionButton btn-primary", "fa-arrow-left");
                        base.addSubmit(ref oFrmGrp2, "RemoveAll", "Remove All - Open Access", "", "PermissionButton btn-danger", "fa-times");
                        XmlNode argoNode = oFrmGrp2;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "Allowing one group impicitly denies all others, only use deny permissions to further filter members of allowed groups");

                        // Process any submissions
                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddAll":
                                {
                                    break;
                                }

                            case "AllowSelected":
                                {
                                    foreach (string cSchema in aDirectorySchemas)
                                        moDbHelper.savePermissions(Conversions.ToLong(this.goRequest["pgid"]), this.goRequest[cSchema], Cms.dbHelper.PermissionLevel.View);
                                    break;
                                }

                            case "DenySelected":
                                {
                                    foreach (string cSchema in aDirectorySchemas)
                                        moDbHelper.savePermissions(Conversions.ToLong(this.goRequest["pgid"]), this.goRequest[cSchema], Cms.dbHelper.PermissionLevel.Denied);
                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    moDbHelper.savePermissions(Conversions.ToLong(this.goRequest["pgid"]), this.goRequest["Items"], Cms.dbHelper.PermissionLevel.Open);
                                    break;
                                }

                            case "RemoveAll":
                                {
                                    moDbHelper.savePermissions(Conversions.ToLong(this.goRequest["pgid"]), "", Cms.dbHelper.PermissionLevel.Open);
                                    break;
                                }
                        }


                        // Populate the left hand selects, grouped by Directory schema.
                        foreach (string cSchema in aDirectorySchemas)
                        {
                            oElmt2 = base.addSelect(ref oFrmGrp1, cSchema, false, cSchema, Conversions.ToString(Interaction.IIf(cSchema == "User", "scroll_30", "scroll_10")), Protean.xForm.ApperanceTypes.Minimal);
                            sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d " + "left outer join tblDirectoryPermission p on p.nDirId = d.nDirKey and p.nStructId = " + id + " " + "where d.cDirSchema='", Protean.stdTools.SqlFmt(cSchema)), "' and p.nPermKey is null order by d.cDirName"));
                            var argoDr = moDbHelper.getDataReader(sSql);
                            base.addOptionsFromSqlDataReader(ref oElmt2, ref argoDr, "name", "value");
                        }

                        XmlNode argoNode1 = oFrmGrp1;
                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");


                        // Populate the allow and denied boxes.
                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "PermittedObjects", "", "Assigned Permissions");
                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Allowed", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT p.nDirId as value, '['+ d.cDirSchema + '] ' + d.cDirName as name from tblDirectoryPermission p " + "inner join tblDirectory d on d.nDirKey = p.nDirId " + "where p.nStructId=" + id + " and p.nAccessLevel = 2" + " order by d.cDirSchema";
                        var argoDr1 = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt4, ref argoDr1, "name", "value");
                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Denied", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT p.nDirId as value, '['+ d.cDirSchema + '] ' + d.cDirName as name from tblDirectoryPermission p " + "inner join tblDirectory d on d.nDirKey = p.nDirId " + "where p.nStructId=" + id + " and p.nAccessLevel = 0" + " order by d.cDirSchema";
                        XmlNode argoNode2 = oFrmGrp3;
                        base.addNote(ref argoNode2, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        var argoDr2 = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt4, ref argoDr2, "name", "value");
                        base.Instance.InnerXml = "<permissions/>";


                        // Rights Alert - to give a user an idea that Rights exists on this page, we'll highlight
                        // this on the Rights page in an alert
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(moDbHelper.GetDataValue("SELECT COUNT(*) As pCount FROM tblDirectoryPermission WHERE nAccessLevel > 2 AND nStructId=" + id, nullreturnvalue: (object)0), 0, false)))
                        {
                            XmlNode argoNode3 = oFrmElmt;
                            base.addNote(ref argoNode3, Protean.xForm.noteTypes.Alert, "Note: there are also Rights being applied to this page.  You can view these by clicking the Rights button above.");
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmPageRights(long id)
                {
                    string cDirectorySchemas = "Group,Role,User";
                    string[] aDirectorySchemas;
                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    string sSql;
                    bool bRightsByUser = true;
                    string cProcessInfo = "";
                    try
                    {

                        // Check for schema overloads
                        if (!string.IsNullOrEmpty("" + goConfig["AdminPageRightSchemas"]))
                        {
                            cDirectorySchemas = goConfig["AdminPageRightSchemas"];
                        }

                        // Split the schema into an array
                        cProcessInfo = "Schemas: " + cDirectorySchemas;
                        aDirectorySchemas = cDirectorySchemas.Split(',');


                        // Load the xform to be edited
                        base.NewFrm("EditPagePermissions");
                        base.submission("EditInputPageRights", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditPermissions", "3col", "Rights for Page - " + moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.ContentStructure, id));
                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the items you want to have access to this page");
                        XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CTRL while clicking the names");


                        // Add the buttons and radios
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditPermissions", "PermissionButtons", "Buttons");
                        oElmt2 = base.addSelect1(ref oFrmGrp2, "Level", false, "Access Level", "multiline", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oElmt2, "View", "2");
                        base.addOption(ref oElmt2, "Add", "3");
                        base.addOption(ref oElmt2, "Add and Update Own", "4");
                        base.addOption(ref oElmt2, "Update All", "5");
                        base.addOption(ref oElmt2, "Approve All", "6");
                        base.addOption(ref oElmt2, "Add, Update and Publish Own ", "7");
                        base.addOption(ref oElmt2, "Publish All", "8");
                        base.addOption(ref oElmt2, "Full", "9");

                        // Add All does nothing, probably is not good for this screen either.
                        // MyBase.addSubmit(oFrmGrp2, "AddAll", "Add All >", "", "PermissionButtons")
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Add Selected >", "", "PermissionButtons");
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons");
                        base.addSubmit(ref oFrmGrp2, "RemoveAll", "< Remove All", "", "PermissionButtons");


                        // Save the permissions on submission
                        switch (base.getSubmitted() ?? "")
                        {

                            // Case "AddAll"

                            case "AddSelected":
                                {
                                    foreach (string cSchema in aDirectorySchemas)
                                    {
                                        if ((cSchema != "User" | bRightsByUser) & !string.IsNullOrEmpty("" + this.goRequest[cSchema]))
                                            moDbHelper.savePermissions(Conversions.ToLong(this.goRequest["pgid"]), this.goRequest[cSchema], (Cms.dbHelper.PermissionLevel)Conversions.ToInteger(this.goRequest["Level"]));
                                    }

                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    foreach (string cSchema in aDirectorySchemas)
                                    {
                                        if ((cSchema != "User" | bRightsByUser) & !string.IsNullOrEmpty("" + this.goRequest["Items" + cSchema]))
                                            moDbHelper.savePermissions(Conversions.ToLong(this.goRequest["pgid"]), this.goRequest["Items" + cSchema], Cms.dbHelper.PermissionLevel.Open);
                                    }

                                    break;
                                }

                            case "RemoveAll":
                                {
                                    moDbHelper.savePermissions(Conversions.ToLong(this.goRequest["pgid"]), "", Cms.dbHelper.PermissionLevel.Open);
                                    break;
                                }
                        }


                        // Add the left hand selection boxes.
                        foreach (string cSchema in aDirectorySchemas)
                        {
                            // Run this for all schemas except users, unless bRightsByUser is True
                            if (cSchema != "User" | bRightsByUser)
                            {
                                oElmt = base.addSelect(ref oFrmGrp1, cSchema, false, cSchema, Conversions.ToString(Interaction.IIf(cSchema == "User", "scroll_30", "scroll_10")), Protean.xForm.ApperanceTypes.Minimal);
                                sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d " + "left outer join tblDirectoryPermission p on p.nDirId = d.nDirKey and p.nStructId = " + id + " " + "where d.cDirSchema='", Protean.stdTools.SqlFmt(cSchema)), "' and p.nPermKey is null order by d.cDirName"));
                                var argoDr = moDbHelper.getDataReader(sSql);
                                base.addOptionsFromSqlDataReader(ref oElmt, ref argoDr, "name", "value");
                            }
                        }


                        // Add the right hand selected lists.
                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "PermittedObjects", "", "All items with permissions to access page");
                        XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        foreach (string cSchema in aDirectorySchemas)
                        {
                            // Run this for all schemas except users, unless bRightsByUser is True
                            if (cSchema != "User" | bRightsByUser)
                            {
                                oElmt4 = base.addSelect(ref oFrmGrp3, "Items" + cSchema, false, "Allowed " + cSchema, Conversions.ToString(Interaction.IIf(cSchema == "User", "scroll_30", "scroll_10")), Protean.xForm.ApperanceTypes.Minimal);
                                sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT p.nDirId as value, '['+ str(p.nAccessLevel) + '] ' + d.cDirName as name from tblDirectoryPermission p " + "inner join tblDirectory d on d.nDirKey = p.nDirId " + "where p.nStructId=" + id + " and d.cDirSchema = '", Protean.stdTools.SqlFmt(cSchema)), "' "), "order by d.cDirSchema"));
                                var argoDr1 = moDbHelper.getDataReader(sSql);
                                base.addOptionsFromSqlDataReader(ref oElmt4, ref argoDr1, "name", "value");
                            }
                        }

                        base.Instance.InnerXml = "<permissions/>";

                        // Permissions Alert - to give a user an idea that permissions exists on this page, we'll highlight
                        // this on the rights page in an alert
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(moDbHelper.GetDataValue("SELECT COUNT(*) As pCount FROM tblDirectoryPermission WHERE nAccessLevel IN (0,2) AND nStructId=" + id, nullreturnvalue: (object)0), 0, false)))
                        {
                            XmlNode argoNode2 = oFrmElmt;
                            base.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, "Note: there are also Permissions being applied to this page.  You can view these by clicking the Permissions button above.");
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmPageRights", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmUserMemberships(long UserId)
                {
                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt1;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    string sSql;
                    string cProcessInfo = "";
                    try
                    {
                        // load the xform to be edited

                        base.NewFrm("EditUserMemberships");
                        base.submission("EditUserMemberships", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditUserMemberships", "3col", "Memberships for User - ");
                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the groups you want this user to belong too");
                        XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditPermissions", "PermissionButtons", "Buttons");
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Add Selected >", "", "PermissionButtons");
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons");
                        base.addSubmit(ref oFrmGrp2, "Finish", "Finish Editing", "", "PermissionButtons");
                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    moDbHelper.saveDirectoryRelations(Conversions.ToLong(this.goRequest["id"]), this.goRequest["Departments"]);
                                    moDbHelper.saveDirectoryRelations(Conversions.ToLong(this.goRequest["id"]), this.goRequest["Groups"]);
                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    moDbHelper.saveDirectoryRelations(Conversions.ToLong(this.goRequest["id"]), this.goRequest["Items"], true);
                                    break;
                                }

                            case "Finish":
                                {
                                    base.valid = true;
                                    break;
                                }
                        }

                        oElmt1 = base.addSelect(ref oFrmGrp1, "Departments", false, "Departments", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                        // sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d " & _
                        // "left outer join tblDirectoryRelation dr on dr.nDirParentId = d.nDirKey and dr.nDirChildId = " & UserId & " " & _
                        // "where d.cDirSchema='Department' and dr.nRelKey is null order by d.cDirName"

                        sSql = "execute getUsersCompanyDepartments @userId=" + UserId + ", @adminUserId=" + this.myWeb.mnUserId;
                        var argoDr = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt1, ref argoDr, "name", "value");
                        oElmt2 = base.addSelect(ref oFrmGrp1, "Groups", false, "Groups", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "execute getUsersCompanyGroups @userId=" + UserId + ", @adminUserId=" + this.myWeb.mnUserId;
                        var argoDr1 = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt2, ref argoDr1, "name", "value");
                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "PermittedObjects", "", "User is Member of...");
                        XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Allowed", "scroll_30", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT d.nDirKey as value, '['+ d.cDirSchema + '] ' + d.cDirName as name from tblDirectory d " + "inner join tblDirectoryRelation dr on dr.nDirParentId = d.nDirKey and dr.nDirChildId = " + UserId + " " + "where d.cDirSchema='Group' or d.cDirSchema='Department'  order by d.cDirName";
                        var argoDr2 = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt4, ref argoDr2, "name", "value");
                        base.Instance.InnerXml = "<memberships/>";
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmUserMemberships", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public virtual XmlElement xFrmDirMemberships(string rootType, long DirId, long DirParId, string ChildTypes)
                {
                    XmlElement oFrmElmt;
                    XmlElement oFrmAll;
                    XmlElement oFrmChosen;
                    XmlElement oFrmButtons;
                    XmlElement oElmt1;
                    XmlElement oElmt4;
                    string sSql;
                    var aChildTypes = Strings.Split(ChildTypes, ",");
                    string aChildDesc = "";
                    int i;
                    string cProcessInfo = "";
                    try
                    {
                        // Enumerate the childtypes
                        var loopTo = Information.UBound(aChildTypes);
                        for (i = 0; i <= loopTo; i++)
                        {
                            if (i == Information.UBound(aChildTypes))
                            {
                                aChildDesc = aChildDesc + " and " + aChildTypes[i] + "s";
                            }
                            else
                            {
                                aChildDesc = aChildDesc + ", " + aChildTypes[i] + "s";
                            }
                        }

                        // Get the description
                        if (Information.UBound(aChildTypes) == 0)
                        {
                            aChildDesc = Strings.LCase(Strings.Right(aChildDesc, Strings.Len(aChildDesc) - 5));
                        }
                        else
                        {
                            aChildDesc = Strings.LCase(Strings.Right(aChildDesc, Strings.Len(aChildDesc) - 2));
                        }

                        // Create the form
                        base.NewFrm("EditMemberships");
                        base.submission("EditMemberships", "", "post");


                        // Create the groups
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditUserMemberships", "3col", "Memberships for " + rootType + " - " + moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Directory, DirId));
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CTRL whilst clicking the names");

                        // Create a group for all directory items
                        oFrmAll = base.addGroup(ref oFrmElmt, "AllObjects", "", "Add " + aChildDesc + " to " + rootType);
                        XmlNode argoNode1 = oFrmAll;
                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, "Select the " + aChildDesc + " you would like to belong to this " + Strings.LCase(rootType));

                        // Create a middle column
                        oFrmButtons = base.addGroup(ref oFrmElmt, "SubmissionButtons", "PermissionButtons", "Actions");

                        // Create a group for chosen directory items
                        oFrmChosen = base.addGroup(ref oFrmElmt, "PermittedObjects", "", rootType + " Contains");
                        // MyBase.addNote(oFrmGrp2, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")


                        // Add submit buttons (group specified by issue 1362)
                        base.addSubmit(ref oFrmButtons, "AddSelected", "Add Selected >", "", "PermissionButtons");
                        base.addSubmit(ref oFrmButtons, "RemoveSelected", "< Remove Selected", "", "PermissionButtons");
                        base.addSubmit(ref oFrmButtons, "Finish", "Finish Editing", "", "principle PermissionButtons");

                        // lets add / remove before we populate
                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    var loopTo1 = Information.UBound(aChildTypes);
                                    for (i = 0; i <= loopTo1; i++)
                                        moDbHelper.saveDirectoryRelations(Conversions.ToLong(this.goRequest["id"]), this.goRequest[aChildTypes[i] + "s"], false, Cms.dbHelper.RelationType.Child);
                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    moDbHelper.saveDirectoryRelations(Conversions.ToLong(this.goRequest["id"]), this.goRequest["Items"], true, Cms.dbHelper.RelationType.Child);
                                    break;
                                }
                        }

                        // populate the add boxes

                        var loopTo2 = Information.UBound(aChildTypes);
                        for (i = 0; i <= loopTo2; i++)
                        {
                            switch (aChildTypes[i] ?? "")
                            {
                                case "User":
                                    {
                                        oElmt1 = base.addSelect(ref oFrmAll, aChildTypes[i] + "s", false, aChildTypes[i] + "s", "scroll_15 alphasort", Protean.xForm.ApperanceTypes.Minimal);
                                        break;
                                    }

                                default:
                                    {
                                        oElmt1 = base.addSelect(ref oFrmAll, aChildTypes[i] + "s", false, aChildTypes[i] + "s", "scroll_10 alphasort", Protean.xForm.ApperanceTypes.Minimal);
                                        break;
                                    }
                            }

                            sSql = "";
                            if (DirParId != 0L)
                            {
                                if (Information.IsNumeric(DirParId) && Information.IsNumeric(DirId))
                                {
                                    sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT d.nDirKey as value, d.cDirName as name, d.cDirXml as detail " + "FROM tblDirectory d " + "     INNER JOIN tblAudit a on nAuditId = a.nAuditKey " + "     INNER JOIN tblDirectoryRelation dr on d.nDirKey = dr.nDirChildId " + "     LEFT JOIN tblDirectoryRelation dr2 on dr2.nDirChildId = d.nDirKey and dr2.nDirParentId =  ", Protean.stdTools.SqlFmt(DirId.ToString())), " "), "WHERE d.cDirSchema = "), Database.SqlString(Strings.Trim(aChildTypes[i]))), "  "), "     AND dr.nDirParentId =  "), Protean.stdTools.SqlFmt(DirParId.ToString())), " "), "     AND dr2.nRelKey is null "), "     AND (a.nStatus =1 or a.nStatus = -1) "), "ORDER BY d.cDirName "));








                                }
                            }
                            else if (Information.IsNumeric(DirId))
                            {
                                sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT d.nDirKey as value, d.cDirName as name, d.cDirXml as detail " + "FROM tblDirectory d " + "     INNER JOIN tblAudit a on nAuditId = a.nAuditKey " + "     LEFT JOIN tblDirectoryRelation dr2 on dr2.nDirChildId = d.nDirKey and dr2.nDirParentId =  ", Protean.stdTools.SqlFmt(DirId.ToString())), " "), "WHERE d.cDirSchema = "), Database.SqlString(Strings.Trim(aChildTypes[i]))), "  "), "     AND dr2.nRelKey is null "), "     AND (a.nStatus =1 or a.nStatus = -1) "), "ORDER BY d.cDirName "));






                            }

                            if (!string.IsNullOrEmpty(sSql))
                            {
                                if (aChildTypes[i] == "User")
                                {
                                    var argoDr = moDbHelper.getDataReader(sSql);
                                    this.addUserOptionsFromSqlDataReader(ref oElmt1, ref argoDr, "name", "value");
                                }
                                else
                                {
                                    var argoDr1 = moDbHelper.getDataReader(sSql);
                                    this.addOptionsFromSqlDataReader(ref oElmt1, ref argoDr1, "name", "value");
                                }
                            }
                        }

                        // populate the allowed boxes
                        oElmt4 = base.addSelect(ref oFrmChosen, "Items", false, "Allowed", "scroll_30 alphasort", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT d.nDirKey as value, '['+ d.cDirSchema + '] ' + d.cDirName as name, d.cDirXml as detail from tblDirectory d " + "inner join tblAudit a on nAuditId = a.nAuditKey inner join tblDirectoryRelation dr on dr.nDirChildId = d.nDirKey and dr.nDirParentId = " + DirId + " " + "where ";
                        var loopTo3 = Information.UBound(aChildTypes);
                        for (i = 0; i <= loopTo3; i++)
                        {
                            if (i == 0)
                            {
                                sSql = sSql + "d.cDirSchema='" + Strings.Trim(aChildTypes[i]) + "'";
                            }
                            else
                            {
                                sSql = sSql + " or d.cDirSchema='" + Strings.Trim(aChildTypes[i]) + "'";
                            }
                        }

                        sSql = sSql + "and (a.nStatus =1 or a.nStatus = -1) order by d.cDirName";
                        var argoDr2 = moDbHelper.getDataReader(sSql);
                        this.addUserOptionsFromSqlDataReader(ref oElmt4, ref argoDr2, "name", "value");
                        if (base.getSubmitted() == "Finish")
                            base.valid = true;
                        base.Instance.InnerXml = "<memberships/>";
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmDirMemberships", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                // +++++++++++++++++++++++++++ Ecommerce Forms ++++++++++++++++++++++++++++++++'

                public XmlElement xFrmEditShippingLocation(long id = -1, long parId = -1)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    try
                    {
                        if (id == 0L)
                            id = -1;
                        base.NewFrm("EditShippingLocation");
                        base.submission("EditShippingLocation", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditShippingLocation", "", "Edit Shipping Location");
                        base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nStructParId", "tblCartShippingLocations/nLocationParId", "true()", oBindParent: ref argoBindParent);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "nLocationType", true, "Type", "required", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Global", 0.ToString());
                        base.addOption(ref oSelElmt, "Continental", 1.ToString());
                        base.addOption(ref oSelElmt, "Country", 2.ToString());
                        base.addOption(ref oSelElmt, "Region", 3.ToString());
                        base.addOption(ref oSelElmt, "County", 4.ToString());
                        base.addOption(ref oSelElmt, "Post Town", 5.ToString());
                        base.addOption(ref oSelElmt, "Postal Code", 5.ToString());
                        XmlElement argoBindParent1 = null;
                        base.addBind("nLocationType", "tblCartShippingLocations/nLocationType", "true()", oBindParent: ref argoBindParent1);
                        base.addInput(ref oFrmElmt, "cNameFull", true, "Full Name", "required");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cNameFull", "tblCartShippingLocations/cLocationNameFull", "true()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oFrmElmt, "cNameShort", true, "Short Name", "required");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cNameShort", "tblCartShippingLocations/cLocationNameShort", "true()", oBindParent: ref argoBindParent3);
                        base.addInput(ref oFrmElmt, "cISOnum", true, "ISO Num");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cISOnum", "tblCartShippingLocations/cLocationISOnum", "false()", oBindParent: ref argoBindParent4);
                        base.addInput(ref oFrmElmt, "cISOa2", true, "ISOa2");
                        XmlElement argoBindParent5 = null;
                        base.addBind("cISOa2", "tblCartShippingLocations/cLocationISOa2", "false()", oBindParent: ref argoBindParent5);
                        base.addInput(ref oFrmElmt, "cISOa3", true, "ISOa3");
                        XmlElement argoBindParent6 = null;
                        base.addBind("cISOa3", "tblCartShippingLocations/cLocationISOa3", "false()", oBindParent: ref argoBindParent6);
                        base.addInput(ref oFrmElmt, "cCode", true, "Code");
                        XmlElement argoBindParent7 = null;
                        base.addBind("cCode", "tblCartShippingLocations/cLocationCode", "false()", oBindParent: ref argoBindParent7);
                        base.addInput(ref oFrmElmt, "cTaxRate", true, "TaxRate");
                        XmlElement argoBindParent8 = null;
                        base.addBind("cTaxRate", "tblCartShippingLocations/nLocationTaxRate", "false()", oBindParent: ref argoBindParent8);
                        base.addSubmit(ref oFrmElmt, "ewSubmit", "Save Page");
                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartShippingLocation, id);
                        // set the parId for a new record
                        if (id < 0L)
                        {
                            this.Instance.SelectSingleNode("tblCartShippingLocations/nLocationParId").InnerText = parId.ToString();
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartShippingLocation, base.Instance);
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditShippingLocation", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmEditDeliveryMethod(long id = -1, long parId = -1)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oGrp1Elmt;
                    XmlElement oGrp2Elmt;
                    string cProcessInfo = "";
                    try
                    {
                        if (id == 0L)
                            id = -1;
                        base.NewFrm("EditShippingMethod");
                        base.submission("EditShippingMethod", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditDeliveryMethod", "2Col", "Edit Delivery Method");
                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Settings", "", "Content Settings");
                        oGrp2Elmt = base.addGroup(ref oFrmElmt, "Content", "", "Terms and Conditions");
                        XmlNode moPaymentCfg;
                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        // check we have differenct currencies
                        if (moPaymentCfg is object)
                        {
                            if (moPaymentCfg.SelectSingleNode("currencies/Currency") is object)
                            {
                                XmlElement oCurElmt;
                                oCurElmt = base.addSelect1(ref oGrp1Elmt, "cCurrency", true, "Currency Code");
                                XmlElement argoBindParent = null;
                                base.addBind("cCurrency", "tblCartShippingMethods/cCurrency", oBindParent: ref argoBindParent);
                                base.addOption(ref oCurElmt, "All", "");
                                foreach (XmlElement ocSetElmt in moPaymentCfg.SelectNodes("currencies/Currency"))
                                    base.addOption(ref oCurElmt, ocSetElmt.SelectSingleNode("name").InnerText, ocSetElmt.GetAttribute("ref"));
                            }
                        }

                        base.addInput(ref oGrp1Elmt, "cShipOptName", true, "Service Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cShipOptName", "tblCartShippingMethods/cShipOptName", "true()", oBindParent: ref argoBindParent1);
                        base.addInput(ref oGrp1Elmt, "cShipOptCarrier", true, "Carrier");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cShipOptCarrier", "tblCartShippingMethods/cShipOptCarrier", "false()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oGrp1Elmt, "cShipOptTime", true, "Delivery Period");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cShipOptTime", "tblCartShippingMethods/cShipOptTime", "false()", oBindParent: ref argoBindParent3);
                        base.addInput(ref oGrp1Elmt, "nShipOptCost", true, "Cost", "short");
                        XmlElement argoBindParent4 = null;
                        base.addBind("nShipOptCost", "tblCartShippingMethods/nShipOptCost", "false()", oBindParent: ref argoBindParent4);
                        base.addInput(ref oGrp1Elmt, "nShipOptPercentage", true, "Percentage", "short");
                        XmlElement argoBindParent5 = null;
                        base.addBind("nShipOptPercentage", "tblCartShippingMethods/nShipOptPercentage", "false()", oBindParent: ref argoBindParent5);
                        base.addInput(ref oGrp1Elmt, "nShipOptQuantMin", true, "Minimum Quantity", "short");
                        XmlElement argoBindParent6 = null;
                        base.addBind("nShipOptQuantMin", "tblCartShippingMethods/nShipOptQuantMin", "false()", oBindParent: ref argoBindParent6);
                        base.addInput(ref oGrp1Elmt, "nShipOptQuantMax", true, "Maximum Quantity", "short");
                        XmlElement argoBindParent7 = null;
                        base.addBind("nShipOptQuantMax", "tblCartShippingMethods/nShipOptQuantMax", "false()", oBindParent: ref argoBindParent7);
                        base.addInput(ref oGrp1Elmt, "nShipOptWeightMin", true, "Minimum Weight", "short");
                        XmlElement argoBindParent8 = null;
                        base.addBind("nShipOptWeightMin", "tblCartShippingMethods/nShipOptWeightMin", "false()", oBindParent: ref argoBindParent8);
                        base.addInput(ref oGrp1Elmt, "nShipOptWeightMax", true, "Maximum Weight", "short");
                        XmlElement argoBindParent9 = null;
                        base.addBind("nShipOptWeightMax", "tblCartShippingMethods/nShipOptWeightMax", "false()", oBindParent: ref argoBindParent9);
                        base.addInput(ref oGrp1Elmt, "nShipOptPriceMin", true, "Minimum Price", "short");
                        XmlElement argoBindParent10 = null;
                        base.addBind("nShipOptPriceMin", "tblCartShippingMethods/nShipOptPriceMin", "false()", oBindParent: ref argoBindParent10);
                        base.addInput(ref oGrp1Elmt, "nShipOptPriceMax", true, "Maximum Price", "short");
                        XmlElement argoBindParent11 = null;
                        base.addBind("nShipOptPriceMax", "tblCartShippingMethods/nShipOptPriceMax", "false()", oBindParent: ref argoBindParent11);
                        base.addInput(ref oGrp1Elmt, "nShipOptHandlingPercentage", true, "Handling Percent", "short");
                        XmlElement argoBindParent12 = null;
                        base.addBind("nShipOptHandlingPercentage", "tblCartShippingMethods/nShipOptHandlingPercentage", "false()", oBindParent: ref argoBindParent12);
                        base.addInput(ref oGrp1Elmt, "nShipOptHandlingFixedCost", true, "Handling Fixed Cost", "short");
                        XmlElement argoBindParent13 = null;
                        base.addBind("nShipOptHandlingFixedCost", "tblCartShippingMethods/nShipOptHandlingFixedCost", "false()", oBindParent: ref argoBindParent13);
                        base.addInput(ref oGrp1Elmt, "nShipOptTaxRate", true, "Tax Rate", "short");
                        XmlElement argoBindParent14 = null;
                        base.addBind("nShipOptTaxRate", "tblCartShippingMethods/nShipOptTaxRate", "false()", oBindParent: ref argoBindParent14);
                        base.addInput(ref oGrp1Elmt, "nDisplayPriority", true, "Display Priority", "short");
                        XmlElement argoBindParent15 = null;
                        base.addBind("nDisplayPriority", "tblCartShippingMethods/nDisplayPriority", "false()", oBindParent: ref argoBindParent15);
                        string argsClass = "xhtml";
                        int argnRows = 0;
                        int argnCols = 0;
                        base.addTextArea(ref oGrp2Elmt, "cTerms", true, "Special Terms", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent16 = null;
                        base.addBind("cTerms", "tblCartShippingMethods/cShipOptTandC", "false()", oBindParent: ref argoBindParent16);
                        base.addInput(ref oGrp2Elmt, "dPublishDate", true, "Start Date", "calendar short");
                        XmlElement argoBindParent17 = null;
                        base.addBind("dPublishDate", "tblCartShippingMethods/dPublishDate", "false()", oBindParent: ref argoBindParent17);
                        base.addInput(ref oGrp2Elmt, "dExpireDate", true, "Expire Date", "calendar short");
                        XmlElement argoBindParent18 = null;
                        base.addBind("dExpireDate", "tblCartShippingMethods/dExpireDate", "false()", oBindParent: ref argoBindParent18);
                        if (moDbHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                        {
                            oSelElmt = base.addSelect(ref oGrp2Elmt, "bCollection", true, "Collection Option", "multiline", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt, "Collection", "True");
                            XmlElement argoBindParent19 = null;
                            base.addBind("bCollection", "tblCartShippingMethods/bCollection", "false()", oBindParent: ref argoBindParent19);
                        }

                        oSelElmt = base.addSelect1(ref oGrp2Elmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Minimal);
                        base.addOption(ref oSelElmt, "Active", 1.ToString());
                        base.addOption(ref oSelElmt, "In-Active", 0.ToString());
                        XmlElement argoBindParent20 = null;
                        base.addBind("nStatus", "tblCartShippingMethods/nStatus", "true()", oBindParent: ref argoBindParent20);
                        base.addSubmit(ref oGrp2Elmt, "ewSubmit", "Save Method");
                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartShippingMethod, id);
                        if (id == -1)
                        {
                            // set some default values in the instance.
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptPercentage").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptQuantMin").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptQuantMax").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptWeightMin").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptWeightMax").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptPriceMin").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptPriceMax").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptHandlingPercentage").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptHandlingFixedCost").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nShipOptTaxRate").InnerText = "0";
                            this.Instance.SelectSingleNode("tblCartShippingMethods/nDisplayPriority").InnerText = "0";
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartShippingMethod, base.Instance);
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditDeliveryMethod", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmEditCarrier(long id = -1, long parId = -1)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oGrp1Elmt;
                    XmlElement oGrp2Elmt;
                    string cProcessInfo = "";
                    try
                    {
                        if (id == 0L)
                            id = -1;
                        base.NewFrm("EditCarrier");
                        base.submission("EditCarrier", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditCarrier", "2Col", "Edit Carrier");
                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Settings", "", "Settings");
                        oGrp2Elmt = base.addGroup(ref oFrmElmt, "Content", "", "Carrier");
                        base.addInput(ref oGrp1Elmt, "dPublishDate", true, "Start Date", "calendar short");
                        XmlElement argoBindParent = null;
                        base.addBind("dPublishDate", "tblCartCarrier/dPublishDate", "false()", oBindParent: ref argoBindParent);
                        base.addInput(ref oGrp1Elmt, "dExpireDate", true, "Expire Date", "calendar short");
                        XmlElement argoBindParent1 = null;
                        base.addBind("dExpireDate", "tblCartCarrier/dExpireDate", "false()", oBindParent: ref argoBindParent1);
                        oSelElmt = base.addSelect1(ref oGrp1Elmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Active", 1.ToString());
                        base.addOption(ref oSelElmt, "In-Active", 0.ToString());
                        XmlElement argoBindParent2 = null;
                        base.addBind("nStatus", "tblCartCarrier/nStatus", "true()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oGrp2Elmt, "cCarrierName", true, "Name");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cCarrierName", "tblCartCarrier/cCarrierName", "true()", oBindParent: ref argoBindParent3);
                        string argsClass = "xhtml";
                        int argnRows = 0;
                        int argnCols = 0;
                        base.addTextArea(ref oGrp2Elmt, "cCarrierTrackingInstructions", true, "Tracking Instructions", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent4 = null;
                        base.addBind("cCarrierTrackingInstructions", "tblCartCarrier/cCarrierTrackingInstructions", "true()", oBindParent: ref argoBindParent4);
                        XmlNode argoNode = oGrp2Elmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Help, "{@code} will be replaced with the code entered at the time of sending");
                        base.addSubmit(ref oGrp2Elmt, "ewSubmit", "Save Method");
                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartCarrier, id);
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartCarrier, base.Instance);
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditCarrier", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmPaymentProvider(string cProviderType)
                {
                    string cProcessInfo = "";
                    XmlElement oPaymentCfg;
                    try
                    {
                        var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                        DefaultSection oCfgSect = (DefaultSection)oCfg.GetSection("protean/payment");
                        oPaymentCfg = this.moPageXML.CreateElement("Config");
                        oPaymentCfg.InnerXml = oCfgSect.SectionInformation.GetRawXml();

                        // Replace Spaces with hypens
                        cProviderType = Strings.Replace(cProviderType, " ", "-");
                        if (!base.load("/xforms/PaymentProvider/" + cProviderType + ".xml", this.myWeb.maCommonFolders))
                        {
                        }
                        // show xform load error message

                        else
                        {
                            // remove hyphens
                            cProviderType = Strings.Replace(cProviderType, "-", "");
                            var oImp = new Tools.Security.Impersonate();
                            if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]))
                            {

                                // replace the instance if it exists in the web.config
                                if (oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']") is object)
                                {
                                    base.Instance.InnerXml = oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']").OuterXml;
                                }

                                if (base.isSubmitted())
                                {
                                    base.updateInstanceFromRequest();
                                    base.validate();
                                    if (base.valid)
                                    {
                                        // here we update the web.config
                                        if (oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']") is null)
                                        {
                                            oPaymentCfg.SelectSingleNode("payment").AppendChild(base.Instance.FirstChild);
                                        }
                                        else
                                        {
                                            oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']").InnerXml = this.Instance.FirstChild.InnerXml;
                                        }

                                        oCfgSect.SectionInformation.RestartOnExternalChanges = false;
                                        oCfgSect.SectionInformation.SetRawXml(oPaymentCfg.InnerXml);
                                        oCfg.Save();

                                        // Copy file to secure if secure directory exists
                                        if (File.Exists(this.goServer.MapPath("protean.payment.config")))
                                        {
                                            var fsHelper = new Protean.fsHelper();
                                            fsHelper.CopyFile("protean.payment.config", "", @"\..\secure", true);
                                            fsHelper = null;
                                        }
                                        // Copy file to secure if secure directory exists
                                        if (File.Exists(this.goServer.MapPath("Protean.Config")))
                                        {
                                            var fsHelper = new Protean.fsHelper();
                                            fsHelper.CopyFile("Protean.Config", "", @"\..\secure", true);
                                            fsHelper = null;
                                        }
                                    }
                                }

                                oImp.UndoImpersonation();
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditPaymentProvider", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeletePaymentProvider(string cProviderType)
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    XmlElement oPaymentCfg;
                    try
                    {
                        var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                        DefaultSection oCfgSect = (DefaultSection)oCfg.GetSection("protean/payment");
                        oPaymentCfg = this.moPageXML.CreateElement("Config");
                        oPaymentCfg.InnerXml = oCfgSect.SectionInformation.GetRawXml();
                        base.NewFrm("DeleteProvider");
                        base.submission("DeleteProvider", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Delete Payment Provider");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this payment provider? - \"" + encodeAllHTML(cProviderType) + "\"");
                        base.addSubmit(ref oFrmElmt, "", "Delete Provider");
                        base.Instance.InnerXml = "<delete/>";

                        // remove hyphens
                        cProviderType = Strings.Replace(cProviderType, "-", "");
                        var oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]))
                        {
                            if (base.isSubmitted())
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                if (base.valid)
                                {
                                    cProviderType = Strings.Replace(cProviderType, " ", "");
                                    // here we update the web.config
                                    if (oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']") is null)
                                    {
                                    }
                                    // can find do nothing
                                    else
                                    {
                                        oPaymentCfg.SelectSingleNode("payment").RemoveChild(oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']"));
                                    }

                                    oCfgSect.SectionInformation.RestartOnExternalChanges = false;
                                    oCfgSect.SectionInformation.SetRawXml(oPaymentCfg.InnerXml);
                                    oCfg.Save();

                                    // Copy file to secure if secure directory exists
                                    var fsHelper = new Protean.fsHelper();
                                    fsHelper.CopyFile("Protean.Config", "", @"\..\secure", true);
                                    fsHelper = null;
                                }
                            }

                            oImp.UndoImpersonation();
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditPaymentProvider", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmUpdateOrder(long nOrderId, string cSchemaName)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oGrp1Elmt;
                    XmlElement oGrp2Elmt;
                    string cProcessInfo = "";
                    long nStatus;
                    XmlElement tempElement;
                    NameValueCollection moCartConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
                    try
                    {
                        base.NewFrm("Update" + cSchemaName);
                        base.submission("Update" + cSchemaName, "", "post", "form_check(this)");
                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartOrder, nOrderId);
                        nStatus = Conversions.ToLong(base.Instance.SelectSingleNode("tblCartOrder/nCartStatus").InnerText);

                        // Add a note for shipped status goRequest("nStatus")
                        string shippedStatus = "Shipped";
                        string customerShippedTemplate = "";
                        bool sendEmailOnShipped = false;
                        if (moCartConfig is object)
                            customerShippedTemplate = moCartConfig["CustomerEmailShippedTemplatePath"];
                        if (!string.IsNullOrEmpty(customerShippedTemplate) && nStatus != 9L && File.Exists(this.goServer.MapPath(customerShippedTemplate)))

                        {
                            sendEmailOnShipped = true;
                            if (this.goRequest["nStatus"] != "9")
                                shippedStatus += " (Confirmation e-mail will be sent to customer)";
                        }

                        string completedMsg = "";
                        if (moCartConfig["SendRecieptsFromAdmin"] != "off")
                        {
                            completedMsg = " - Payment Recieved (resends receipt)";
                        }

                        // update the status if we have submitted it allready
                        if (!string.IsNullOrEmpty(this.goRequest["nStatus"]))
                            nStatus = Conversions.ToInteger(this.goRequest["nStatus"]);
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Update" + cSchemaName, "", "");
                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Status", "", cSchemaName + " Status");
                        oSelElmt = base.addSelect1(ref oGrp1Elmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                        switch (nStatus)
                        {
                            case 0L:
                            case 1L:
                            case 2L:
                            case 3L:
                            case 4L:
                            case 5L: // new
                                {
                                    base.addOption(ref oSelElmt, "Abandoned", 11.ToString());
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString());
                                    break;
                                }

                            case 6L: // Completed
                                {
                                    base.addOption(ref oSelElmt, "Awaiting Payment", 13.ToString(), false, "Awaiting_Payment");
                                    base.addOption(ref oSelElmt, "Completed", 6.ToString(), false, "Completed");
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString(), false, "Refunded");
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString(), false, "Shipped");
                                    break;
                                }

                            case 7L: // Refunded
                                {
                                    base.addOption(ref oSelElmt, "Completed" + completedMsg, 6.ToString());
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString());
                                    break;
                                }

                            case 8L: // Failed
                                {
                                    base.addOption(ref oSelElmt, "Abandoned", 11.ToString());
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString());
                                    break;
                                }

                            case 9L: // Shipped
                                {
                                    base.addOption(ref oSelElmt, "Completed" + completedMsg, 6.ToString());
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString());
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString());
                                    break;
                                }

                            case 10L: // Deposit Paid
                                {
                                    base.addOption(ref oSelElmt, "Deposit Paid", 10.ToString());
                                    base.addOption(ref oSelElmt, "Completed" + completedMsg, 6.ToString());
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString());
                                    break;
                                }

                            case 13L: // Awaiting Payment
                                {
                                    base.addOption(ref oSelElmt, "Awaiting Payment", 13.ToString());
                                    base.addOption(ref oSelElmt, "Completed" + completedMsg, 6.ToString());
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString());
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString());
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString());
                                    break;
                                }

                            case 17L: // In Progress
                                {
                                    base.addOption(ref oSelElmt, "Awaiting Payment", 13.ToString(), false, "Awaiting_Payment");
                                    base.addOption(ref oSelElmt, "Completed", 6.ToString(), false, "Completed");
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString(), false, "Refunded");
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString(), false, "Shipped");
                                    break;
                                }
                        }

                        XmlElement argoBindParent = null;
                        base.addBind("nStatus", "tblCartOrder/nCartStatus", "true()", oBindParent: ref argoBindParent);
                        if (nStatus == 6L | this.myWeb.moRequest["nStatus"] == "9" | nStatus == 17L)
                        {
                            // Add carrier information
                            XmlElement argoInsertBeforeNode = null;
                            var oSwitch = base.addSwitch(ref oGrp1Elmt, "", oInsertBeforeNode: ref argoInsertBeforeNode);
                            var oCase = base.addCase(ref oSwitch, "Awaiting_Payment");
                            var oCase1 = base.addCase(ref oSwitch, "Completed");
                            var oCase2 = base.addCase(ref oSwitch, "Refunded");
                            var oCase3 = base.addCase(ref oSwitch, "Shipped");

                            // Turn of validation when switching back to completed
                            string validationOn = "true()";
                            if (nStatus == 6L & this.myWeb.moRequest["nStatus"] == "6")
                            {
                                validationOn = "false()";
                            }

                            if (Strings.LCase(moCartConfig["ShippedValidation"]) == "off")
                            {
                                validationOn = "false()";
                            }

                            if (moDbHelper.checkDBObjectExists("tblCartCarrier"))
                            {
                                var oCarrierElmt = base.addGroup(ref oCase3, "Carrier", "inline", cSchemaName + " Carrier");
                                var CarrierSelect = base.addSelect1(ref oCarrierElmt, "nCarrierId", true, "Carrier");
                                var oDr = moDbHelper.getDataReader("select cCarrierName as name, nCarrierKey as value from tblCartCarrier");
                                base.addOptionsFromSqlDataReader(ref CarrierSelect, ref oDr);
                                XmlElement argoBindParent1 = null;
                                base.addBind("nCarrierId", "tblCartOrderDelivery/nCarrierId", validationOn, oBindParent: ref argoBindParent1);
                                base.addInput(ref oCarrierElmt, "cCarrierRef", true, "Carrier Reference");
                                XmlElement argoBindParent2 = null;
                                base.addBind("cCarrierRef", "tblCartOrderDelivery/cCarrierRef", "false()", oBindParent: ref argoBindParent2);
                                base.addInput(ref oCarrierElmt, "cCarrierNotes", true, "Carrier Notes", "long");
                                XmlElement argoBindParent3 = null;
                                base.addBind("cCarrierNotes", "tblCartOrderDelivery/cCarrierNotes", "false()", oBindParent: ref argoBindParent3);
                                string validClass = "";
                                if (validationOn == "true()")
                                {
                                    validClass = " required";
                                }

                                base.addInput(ref oCarrierElmt, "dExpectedDeliveryDate", true, "Target Delivery Date", "calendar" + validClass);
                                XmlElement argoBindParent4 = null;
                                base.addBind("dExpectedDeliveryDate", "tblCartOrderDelivery/dExpectedDeliveryDate", validationOn, oBindParent: ref argoBindParent4);
                                base.addInput(ref oCarrierElmt, "dCollectionDate", true, "Collection Date", "calendar" + validClass);
                                XmlElement argoBindParent5 = null;
                                base.addBind("dCollectionDate", "tblCartOrderDelivery/dCollectionDate", validationOn, oBindParent: ref argoBindParent5);
                                var deliveryInstance = this.moPageXML.CreateElement("instance");
                                deliveryInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartDelivery);
                                deliveryInstance.SelectSingleNode("tblCartOrderDelivery/dCollectionDate").InnerText = Protean.stdTools.xmlDate(DateAndTime.Now);
                                base.Instance.AppendChild(deliveryInstance.FirstChild);

                                // set the order id
                                base.Instance.SelectSingleNode("tblCartOrderDelivery/nOrderId").InnerText = nOrderId.ToString();
                            }
                        }

                        oGrp2Elmt = base.addGroup(ref oFrmElmt, "Notes", "", "Notes");

                        // Get the seller notes
                        string sellerNotes = this.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText;
                        tempElement = (XmlElement)base.addDiv(ref oGrp2Elmt, sellerNotes, "orderNotes", false);
                        var aSellerNotes = Strings.Split(sellerNotes, "/n");
                        string cSellerNotesHtml = "<ul>";
                        for (int snCount = 0, loopTo = Information.UBound(aSellerNotes); snCount <= loopTo; snCount++)
                            cSellerNotesHtml = cSellerNotesHtml + "<li>" + convertEntitiesToCodes(aSellerNotes[snCount]) + "</li>";
                        tempElement.InnerXml = cSellerNotesHtml + "</ul>";
                        string argsClass = "";
                        int argnRows = Conversions.ToInteger("5");
                        int argnCols = 0;
                        base.addTextArea(ref oGrp2Elmt, "cNotesAmend", false, "Update Seller Notes", ref argsClass, ref argnRows, nCols: ref argnCols);
                        base.addSubmit(ref oGrp2Elmt, "ewUpdate" + cSchemaName, "Update Order");
                        if (base.isSubmitted())
                        {
                            // MyBase.updateInstanceFromRequest()
                            base.Instance.SelectSingleNode("tblCartOrder/nCartStatus").InnerText = this.goRequest["nStatus"];
                            string sStatusDesc;
                            switch (this.goRequest["nStatus"] ?? "")
                            {
                                case "0":
                                    {
                                        sStatusDesc = "New Cart";
                                        break;
                                    }

                                case "1":
                                    {
                                        sStatusDesc = "Items Added";
                                        break;
                                    }

                                case "2":
                                    {
                                        sStatusDesc = "Billing Address Added";
                                        break;
                                    }

                                case "3":
                                    {
                                        sStatusDesc = "Delivery Address Added";
                                        break;
                                    }

                                case "4":
                                    {
                                        sStatusDesc = "Confirmed";
                                        break;
                                    }

                                case "5":
                                    {
                                        sStatusDesc = "Pass for Payment";
                                        break;
                                    }

                                case "6":
                                    {
                                        sStatusDesc = "Completed";
                                        break;
                                    }

                                case "7":
                                    {
                                        sStatusDesc = "Refunded";
                                        break;
                                    }

                                case "8":
                                    {
                                        sStatusDesc = "Failed";
                                        break;
                                    }

                                case "9":
                                    {
                                        sStatusDesc = "Shipped";
                                        break;
                                    }

                                case "10":
                                    {
                                        sStatusDesc = "Deposit Paid";
                                        break;
                                    }

                                case "11":
                                    {
                                        sStatusDesc = "Abandoned";
                                        break;
                                    }

                                case "12":
                                    {
                                        sStatusDesc = "Deleted";
                                        break;
                                    }

                                case "13":
                                    {
                                        sStatusDesc = "Awaiting Payment";
                                        break;
                                    }

                                default:
                                    {
                                        sStatusDesc = "No Change";
                                        break;
                                    }
                            }

                            string updateNotes = this.goRequest["cNotesAmend"];
                            // If Not String.IsNullOrEmpty(updateNotes) Then updateNotes = ControlChars.CrLf & Now.ToString() & ":" & updateNotes

                            string notes = base.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText + "/n" + Conversions.ToString(DateAndTime.Now) + ": changed to: (" + this.goRequest["nStatus"] + ") " + sStatusDesc + " - " + updateNotes;
                            string AdminUserName = this.myWeb.moPageXml.SelectSingleNode("Page/User/@name").InnerText;
                            notes += "by " + AdminUserName;
                            base.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText = notes;
                            moDbHelper.logActivity(Cms.dbHelper.ActivityType.OrderStatusChange, (long)this.myWeb.mnUserId, 0L, 0L, notes);
                            aSellerNotes = Strings.Split(base.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText, "/n");
                            cSellerNotesHtml = "<ul>";
                            for (int snCount = 0, loopTo1 = Information.UBound(aSellerNotes); snCount <= loopTo1; snCount++)
                                cSellerNotesHtml = cSellerNotesHtml + "<li>" + convertEntitiesToCodes(aSellerNotes[snCount]) + "</li>";
                            tempElement.InnerXml = cSellerNotesHtml + "</ul>";
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartOrder, base.Instance);
                                if (Conversions.ToDouble(this.goRequest["nStatus"]) == 9d)
                                {

                                    // Get the carrier name from the ID
                                    if (moDbHelper.checkDBObjectExists("tblCartCarrier") & moDbHelper.checkDBObjectExists("tblCartOrderDelivery"))
                                    {
                                        string CarrierName;
                                        CarrierName = Conversions.ToString(moDbHelper.GetDataValue("select cCarrierName from tblCartCarrier where nCarrierKey = " + this.myWeb.moRequest["nCarrierId"]));
                                        this.Instance.SelectSingleNode("tblCartOrderDelivery/cCarrierName").InnerText = CarrierName;
                                        moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartDelivery, base.Instance);
                                    }

                                    if (sendEmailOnShipped)
                                    {
                                        string cSubject = moCartConfig["OrderEmailSubject"];
                                        if (string.IsNullOrEmpty(cSubject))
                                            cSubject = "Order Shipped";
                                        // send to customer
                                        var oMsg = new Protean.Messaging(ref this.myWeb.msException);
                                        var cartXml = new XmlDocument();
                                        var cartElement = cartXml.CreateElement("Cart");
                                        cartElement.InnerXml = base.Instance.SelectSingleNode("tblCartOrder/cCartXml").InnerXml;
                                        cartElement.SetAttribute("InvoiceRef", moCartConfig["OrderNoPrefix"] + base.Instance.SelectSingleNode("tblCartOrder/nCartOrderKey").InnerXml);
                                        cartElement.SetAttribute("InvoiceDate", Strings.Left(base.Instance.SelectSingleNode("tblCartOrder/dInsertDate").InnerXml, 10));
                                        cartElement.SetAttribute("AccountId", base.Instance.SelectSingleNode("tblCartOrder/nCartUserDirId").InnerXml);
                                        if (base.Instance.SelectSingleNode("tblCartOrderDelivery") is object)
                                        {
                                            var delElmt = cartXml.CreateElement("Delivery");
                                            delElmt.InnerXml = base.Instance.SelectSingleNode("tblCartOrderDelivery").InnerXml;
                                            cartElement.AppendChild(delElmt);
                                            long carrierId = Conversions.ToInteger(delElmt.SelectSingleNode("nCarrierId").InnerText);
                                            var carrierElmt = cartXml.CreateElement("Carrier");
                                            carrierElmt.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartCarrier, carrierId).Replace("{@code}", delElmt.SelectSingleNode("cCarrierRef").InnerText);
                                            cartElement.AppendChild(carrierElmt);
                                        }

                                        string CustomerEmailShippedTemplatePath = Conversions.ToString(Interaction.IIf(!string.IsNullOrEmpty(moCartConfig["CustomerEmailShippedTemplatePath"]), moCartConfig["CustomerEmailShippedTemplatePath"], "/xsl/Cart/mailOrderCustomerDelivery.xsl"));
                                        Cms.dbHelper argodbHelper = null;
                                        cProcessInfo = Conversions.ToString(oMsg.emailer(cartElement, CustomerEmailShippedTemplatePath, moCartConfig["MerchantName"], moCartConfig["MerchantEmail"], cartElement.SelectSingleNode("//Contact[@type='Billing Address']/Email").InnerText, "Order Shipped", odbHelper: ref argodbHelper));
                                        oMsg = null;
                                    }
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmUpdateOrder", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmRefundOrder(long nOrderId, string providerName, string providerPaymentReference)
                {
                    string cProcessInfo = "";
                    NameValueCollection moCartConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
                    var oCfg = WebConfigurationManager.OpenWebConfiguration("/" + this.myWeb.moConfig["ProjectPath"]);
                    DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection("protean/web");
                    try
                    {
                        string IsRefund = "";
                        var oCart = new Cms.Cart(ref this.myWeb);
                        base.NewFrm("Refund");
                        base.submission("Refund", "", "post", "form_check(this)");
                        decimal refundAmount;
                        string cResponse = "";   // check this
                        var xdoc = new XmlDocument();
                        string amount = "";
                        if (nOrderId > 0L)
                        {
                            string cartXmlSql = "select cCartXml from tblCartOrder where nCartOrderKey = " + nOrderId;
                            if (!string.IsNullOrEmpty(cartXmlSql))
                            {
                                string orderXml = Convert.ToString(this.myWeb.moDbHelper.GetDataValue(cartXmlSql));
                                xdoc.LoadXml(orderXml);
                            }

                            if (!string.IsNullOrEmpty(xdoc.InnerXml))
                            {
                                var xn = xdoc.SelectSingleNode("/Order/PaymentDetails/instance/Response");
                                var xnInstance = xdoc.SelectSingleNode("/Order/PaymentDetails/instance");
                                if (xn is object & xnInstance is object)
                                {
                                    amount = xnInstance.Attributes["AmountPaid"].InnerText;
                                }
                            }
                        }

                        refundAmount = (decimal)Convert.ToDouble(amount);
                        base.Instance.InnerXml = "<Refund><RefundAmount> " + refundAmount + " </RefundAmount><ProviderName>" + providerName + "</ProviderName> <ProviderReference>" + providerPaymentReference + " </ProviderReference><OrderId>" + nOrderId + "</OrderId></Refund>";
                        XmlElement oFrmElmt;
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Refund " + providerName, "", "");
                        base.addInput(ref oFrmElmt, "RefundAmount", true, "Refund Amount");
                        XmlElement argoBindParent = null;
                        base.addBind("RefundAmount", "Refund/RefundAmount", "true()", oBindParent: ref argoBindParent);
                        base.addInput(ref oFrmElmt, "ProviderName", true, "Provider Name", "readonly");
                        XmlElement argoBindParent1 = null;
                        base.addBind("ProviderName", "Refund/ProviderName", "true()", oBindParent: ref argoBindParent1);
                        base.addInput(ref oFrmElmt, "ProviderReference", true, "Provider Reference", "readonly");
                        XmlElement argoBindParent2 = null;
                        base.addBind("ProviderReference", "Refund/ProviderReference", "true()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oFrmElmt, "id", true, "Order Id", "readonly");
                        XmlElement argoBindParent3 = null;
                        base.addBind("id", "Refund/OrderId", "true()", oBindParent: ref argoBindParent3);
                        base.addSubmit(ref oFrmElmt, "Refund", "Refund", "ewSubmit");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (Conversions.ToDouble(amount) >= (double)refundAmount)
                            {
                                if (base.valid)
                                {
                                    // oCgfSect.SectionInformation.RestartOnExternalChanges = False    'check this
                                    // oCgfSect.SectionInformation.SetRawXml(MyBase.Instance.InnerXml)
                                    // oCfg.Save()

                                    var oPayProv = new Protean.Providers.Payment.BaseProvider(ref this.myWeb, providerName);
                                    IsRefund = Conversions.ToString(oPayProv.Activities.RefundPayment(providerPaymentReference, refundAmount));
                                    if (IsRefund is null)
                                    {
                                        base.addNote("Refund", Protean.xForm.noteTypes.Alert, "Refund Failed");
                                        this.myWeb.msRedirectOnEnd = (Conversions.ToDouble("/?ewCmd=Orders&ewCmd2=Display&id=") + nOrderId).ToString();
                                    }
                                    // Update Seller Notes:
                                    string sSql = "select * from tblCartOrder where nCartOrderKey = " + nOrderId;
                                    DataSet oDs;
                                    oDs = this.myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                                    {
                                        if (IsRefund is object)
                                        {
                                            oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), DateAndTime.Today), " "), DateAndTime.TimeOfDay), ": changed to: (Refund Payment Successful) "), Constants.vbLf), "comment: "), "Refund amount:"), refundAmount), Constants.vbLf), "Full Response:' Refunded Amount is "), refundAmount), " And ReceiptId is: "), IsRefund), "'");
                                        }
                                        else
                                        {
                                            oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), DateAndTime.Today), " "), DateAndTime.TimeOfDay), ": changed to: (Refund Payment Failed) "), Constants.vbLf), "comment: "), "Refund amount:"), refundAmount), Constants.vbLf), "Full Response:' Refunded Amount is "), refundAmount), " And ReceiptId is: "), IsRefund), "'");
                                        }
                                    }

                                    this.myWeb.moDbHelper.updateDataset(ref oDs, "Order");
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmRefundOrder", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmFindRelated(string nParentID, string cContentType, ref XmlElement oPageDetail, string nParId, bool bIgnoreParID, string cTableName, string cSelectField, string cFilterField, string redirect = "")
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt1;
                    XmlElement oSelElmt2;
                    var oTempInstance = this.moPageXML.CreateElement("instance");
                    bool bCascade = false;
                    string cProcessInfo = "";
                    try
                    {
                        string cParentContentName = Protean.xmlTools.convertEntitiesToCodes(moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Content, Conversions.ToLong(nParentID)));
                        base.NewFrm("FindRelatedContent");
                        base.Instance.InnerXml = "<nParentContentId>" + nParentID + "</nParentContentId>" + "<cSchemaName>" + cContentType + "</cSchemaName>" + "<cSection/><nSearchChildren/><nIncludeRelated/><cParentContentName>" + cParentContentName + "</cParentContentName><redirect>" + redirect + "</redirect><cSearch/>";

                        // MyBase.submission("AddRelated", "?ewCmd=RelateSearch&Type=Document&xml=x", "post", "form_check(this)")
                        base.submission("AddRelated", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "SearchRelated", sLabel: "Search For Related " + cContentType);

                        // Definitions
                        if (!string.IsNullOrEmpty(redirect))
                        {
                            base.addInput(ref oFrmElmt, "redirect", true, "redirect", "hidden");
                            XmlElement argoBindParent = null;
                            base.addBind("redirect", "redirect", oBindParent: ref argoBindParent);
                        }

                        base.addInput(ref oFrmElmt, "nParentContentId", true, "nParentContentId", "hidden");
                        XmlElement argoBindParent1 = null;
                        base.addBind("nParentContentId", "nParentContentId", oBindParent: ref argoBindParent1);
                        base.addInput(ref oFrmElmt, "cSchemaName", true, "cSchemaName", "hidden");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cSchemaName", "cSchemaName", oBindParent: ref argoBindParent2);

                        // What we are searching for
                        base.addInput(ref oFrmElmt, "cSearch", true, "Search Text");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cSearch", "cSearch", "false()", oBindParent: ref argoBindParent3);

                        // Pages
                        oSelElmt1 = base.addSelect1(ref oFrmElmt, "cSection", false, "Page", nAppearance: Protean.xForm.ApperanceTypes.Minimal);
                        base.addOption(ref oSelElmt1, "All", 0.ToString());
                        base.addOption(ref oSelElmt1, "All Orphan " + cContentType + "s", (-1).ToString());
                        string cSQL;
                        cSQL = "SELECT tblContentStructure.* FROM tblContentStructure ORDER BY nStructOrder";
                        var oDS = new DataSet();
                        oDS = moDbHelper.GetDataSet(cSQL, "Menu", "Struct");
                        oDS.Relations.Add("RelMenu", oDS.Tables["Menu"].Columns["nStructKey"], oDS.Tables["Menu"].Columns["nStructParID"], false);
                        oDS.Relations["RelMenu"].Nested = true;
                        var oMenuXml = new XmlDocument();
                        oMenuXml.InnerXml = oDS.GetXml();
                        foreach (XmlElement oMenuElmt in oMenuXml.SelectNodes("descendant-or-self::Menu"))
                        {
                            var oTmpNode = oMenuElmt;
                            string cNameString = "";
                            while (oTmpNode.ParentNode.Name != "Struct")
                            {
                                cNameString += "-";
                                oTmpNode = (XmlElement)oTmpNode.ParentNode;
                            }

                            cNameString += oMenuElmt.SelectSingleNode("cStructName").InnerText;
                            base.addOption(ref oSelElmt1, cNameString, oMenuElmt.SelectSingleNode("nStructKey").InnerText);
                        }

                        XmlElement argoBindParent4 = null;
                        base.addBind("cSection", "cSection", "true()", oBindParent: ref argoBindParent4);
                        // Search sub pages
                        oSelElmt2 = base.addSelect(ref oFrmElmt, "nSearchChildren", true, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt2, "Search all sub-pages", 1.ToString());
                        XmlElement argoBindParent5 = null;
                        base.addBind("nSearchChildren", "nSearchChildren", "false()", oBindParent: ref argoBindParent5);
                        if (cContentType.Contains("Product") & cContentType.Contains("SKU"))
                        {
                            oSelElmt2 = base.addSelect(ref oFrmElmt, "nIncludeRelated", true, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt2, "Include Related Sku's", 1.ToString());
                            XmlElement argoBindParent6 = null;
                            base.addBind("nIncludeRelated", "nIncludeRelated", "false()", oBindParent: ref argoBindParent6);
                        }

                        // search button
                        base.addSubmit(ref oFrmElmt, "Search", "Search", "ewSubmit");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                // Dim nPar As Integer = goRequest.QueryString("GroupId")
                                if (!Information.IsNumeric(nParId))
                                {
                                    XmlElement oParElmt = (XmlElement)base.Instance.SelectSingleNode(nParId);
                                    if (oParElmt is object)
                                        nParId = oParElmt.InnerText;
                                }

                                int nRoot = Conversions.ToInteger(base.Instance.SelectSingleNode("cSection").InnerText);
                                bool bChilds = Conversions.ToBoolean(Interaction.IIf(base.Instance.SelectSingleNode("nSearchChildren").InnerText == "1", true, false));
                                string cExpression = base.Instance.SelectSingleNode("cSearch").InnerText;
                                bool bIncRelated = Conversions.ToBoolean(Interaction.IIf(base.Instance.SelectSingleNode("nIncludeRelated").InnerText == "1", true, false));
                                string sSQL = "Select " + cSelectField + " From " + cTableName + " WHERE " + cFilterField + " = " + nParId;
                                var oDre = moDbHelper.getDataReader(sSQL);
                                string cTmp = "";
                                while (oDre.Read())
                                    cTmp = Conversions.ToString(cTmp + Operators.ConcatenateObject(oDre[0], ","));
                                oDre.Close();
                                if (!string.IsNullOrEmpty(cTmp))
                                    cTmp = Strings.Left(cTmp, Strings.Len(cTmp) - 1);
                                oPageDetail.AppendChild(moDbHelper.RelatedContentSearch(nRoot, cContentType, bChilds, cExpression, Conversions.ToInteger(nParId), Conversions.ToInteger(Interaction.IIf(bIgnoreParID, (object)0, nParId)), cTmp.Split(','), bIncRelated));
                            }
                        }
                        else
                        {
                            base.Instance.InnerXml = "<nParentContentId>" + nParentID + "</nParentContentId>" + "<cSchemaName>" + cContentType + "</cSchemaName>" + "<cSection>0</cSection>" + "<nSearchChildren>1</nSearchChildren>" + "<cParentContentName>" + cParentContentName + "</cParentContentName>" + "<redirect>" + redirect + "</redirect><cSearch/>";
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmFindRelated", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmProductGroup(int nGroupId, string SchemaName = "Discount")
                {
                    XmlElement oFrmElmt;
                    XmlElement oGrp1Elmt;
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("EditProductGroup");
                        base.Instance.InnerXml = "<tblCartProductCategories><nCatKey/><cCatSchemaName>" + SchemaName + "</cCatSchemaName><cCatForeignRef/><nCatParentId/><cCatName/><cCatDescription/><nAuditId/></tblCartProductCategories>";
                        if (nGroupId > 0)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartProductCategories, nGroupId);
                        }

                        base.submission("EditProductGroup", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "ProductGroup");
                        base.addNote("pgheader", Protean.xForm.noteTypes.Help, Conversions.ToString(Operators.ConcatenateObject(Interaction.IIf(nGroupId > 0, "Edit ", "Add "), "Product Group")));
                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Details", "1col", "Details");

                        // Definitions
                        base.addInput(ref oGrp1Elmt, "nCatKey", true, "nCatKey", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nCatKey", "tblCartProductCategories/nCatKey", oBindParent: ref argoBindParent);
                        // MyBase.addInput(oGrp1Elmt, "cCatSchemaName", True, "Schema")
                        // MyBase.addBind("cCatSchemaName", "tblCartProductCategories/cCatSchemaName", "true()")
                        // MyBase.addInput(oGrp1Elmt, "cCatForeignRef", True, "Foreign Ref")
                        // MyBase.addBind("cCatForeignRef", "tblCartProductCategories/cCatForeignRef")
                        // MyBase.addInput(oGrp1Elmt, "nCatParentId", True, "nCatParentId", "hidden")
                        // MyBase.addBind("nCatParentId", "tblCartProductCategories/nCatParentId")
                        base.addInput(ref oGrp1Elmt, "cCatName", true, "Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cCatName", "tblCartProductCategories/cCatName", "true()", oBindParent: ref argoBindParent1);
                        var oSchemaSelect = base.addSelect1(ref oGrp1Elmt, "cCatSchemaName", true, "Group Type", nAppearance: Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSchemaSelect, SchemaName, SchemaName);
                        string[] aOptions = null;
                        if (this.myWeb.moCart.moCartConfig["ProductCategoryTypes"] is object)
                        {
                            aOptions = this.myWeb.moCart.moCartConfig["ProductCategoryTypes"].Split(',');
                            if (aOptions.Length > 0)
                            {
                                for (int i = 0, loopTo = aOptions.Length - 1; i <= loopTo; i++)
                                    base.addOption(ref oSchemaSelect, aOptions[i], aOptions[i]);
                            }
                        }

                        XmlElement argoBindParent2 = null;
                        base.addBind("cCatSchemaName", "tblCartProductCategories/cCatSchemaName", oBindParent: ref argoBindParent2);
                        int argnRows = 15;
                        int argnCols = 50;
                        base.addTextArea(ref oGrp1Elmt, "cCatDescription", true, "Description", nRows: ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent3 = null;
                        base.addBind("cCatDescription", "tblCartProductCategories/cCatDescription", oBindParent: ref argoBindParent3);
                        base.addInput(ref oGrp1Elmt, "nAuditId", true, "nAuditId", "hidden");
                        XmlElement argoBindParent4 = null;
                        base.addBind("nAuditId", "tblCartProductCategories/nAuditId", oBindParent: ref argoBindParent4);

                        // search button

                        base.addSubmit(ref oFrmElmt, "EditProductGroup", "Save Product Group", "SaveProductGroup");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartProductCategories, base.Instance, Conversions.ToLong(Interaction.IIf(nGroupId > 0, nGroupId, -1)));
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmProductGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDiscountRule(int nDiscountId, int nDiscountType = 0)
                {
                    string cProcessInfo = "";
                    string cTypePath;
                    Protean.stdTools.DiscountCategory eDiscountType;
                    string sSQL;
                    try
                    {
                        if (nDiscountType > 0)
                            eDiscountType = (Protean.stdTools.DiscountCategory)nDiscountType;
                        if (nDiscountId > 0)
                        {
                            sSQL = "Select nDiscountCat From tblCartDiscountRules WHERE nDiscountKey = " + nDiscountId;
                            nDiscountType = Conversions.ToInteger(moDbHelper.ExeProcessSqlScalar(sSQL));
                        }

                        if (nDiscountType > 0)
                        {
                            eDiscountType = (Protean.stdTools.DiscountCategory)nDiscountType;
                            cTypePath = "DiscountRule_" + eDiscountType.ToString() + ".xml";
                        }
                        else
                        {
                            cTypePath = "DiscountRule.xml";
                        }

                        base.NewFrm("EditDiscountRules");
                        if (!base.load("/xforms/discounts/" + cTypePath, this.myWeb.maCommonFolders))
                        {
                            // not allot we can do really except try defaults
                            if (!base.load("/xforms/discounts/DiscountRule.xml", this.myWeb.maCommonFolders))
                            {
                                // not allot we can do really 
                            }
                        }

                        var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                        if (nDiscountId > 0)
                        {
                            existingInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartDiscountRules, nDiscountId);
                            this.LoadInstanceFromInnerXml(existingInstance.InnerXml);
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartDiscountRules, base.Instance, Conversions.ToLong(Interaction.IIf(nDiscountId > 0, nDiscountId, -1)));
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmDiscountRule", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDiscountProductRelations(long id, string dname)
                {
                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    string sSql;
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("EditDiscountProductRelations");
                        base.submission("EditDiscountRelations", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditRelations", "3col", "Product Group Relations for Discount");
                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the product groups you want to have access to this discount");
                        XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditRelations", "RelationButtons", "Buttons");
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Add Selected >", "", "PermissionButtons");
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons");
                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    moDbHelper.saveDiscountProdGroupRelation((int)id, this.goRequest["Groups"]);
                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    moDbHelper.saveDiscountProdGroupRelation((int)id, this.goRequest["Items"], false);
                                    break;
                                }
                        }

                        oElmt2 = base.addSelect(ref oFrmGrp1, "Groups", false, "Product Groups", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT nCatKey AS value, cCatName AS name" + " FROM tblCartProductCategories" + " WHERE (cCatSchemaName = N'Discount') AND" + " (((SELECT nDiscountProdCatRelationKey" + " FROM tblCartDiscountProdCatRelations" + " WHERE (nProductCatId = tblCartProductCategories.nCatKey) AND (nDiscountId = " + id + "))) IS NULL)" + " ORDER BY cCatName";
                        var argoDr = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt2, ref argoDr, "name", "value");
                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "RelatedObjects", "", "All items with permissions to access page");
                        XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Related", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT tblCartDiscountProdCatRelations.nDiscountProdCatRelationKey as value, tblCartProductCategories.cCatName as name" + " FROM tblCartDiscountProdCatRelations INNER JOIN tblCartProductCategories ON tblCartDiscountProdCatRelations.nProductCatId = tblCartProductCategories.nCatKey" + " WHERE (tblCartDiscountProdCatRelations.nDiscountId = " + id + ") ORDER BY tblCartProductCategories.cCatName";
                        var argoDr1 = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt4, ref argoDr1, "name", "value");
                        base.Instance.InnerXml = "<relations/>";
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDiscountDirRelations(long id, string dname)
                {
                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    XmlElement oElmt5;
                    string sSql;
                    string cProcessInfo = "";
                    var bDeny = default(bool);
                    string cDenyFilter = "";
                    try
                    {
                        base.NewFrm("EditDiscountProductDirs");
                        if (moDbHelper.checkTableColumnExists("tblCartDiscountDirRelations", "nPermLevel"))
                        {
                            bDeny = true;
                            cDenyFilter = " and nPermLevel <> 0";
                        }

                        base.submission("EditDiscountDirs", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditDirs", "3col", "User Group Relations for Discount " + dname);
                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the user groups you want to have access to this discount");
                        XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditDirs", "DirButtons", "Buttons");
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Add Selected >", "", "PermissionButtons");
                        if (bDeny)
                        {
                            base.addSubmit(ref oFrmGrp2, "DenySelected", "Deny Selected >", "", "PermissionButtons");
                        }

                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons");
                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    moDbHelper.saveDiscountDirRelation((int)id, this.goRequest["Groups"]);
                                    break;
                                }

                            case "DenySelected":
                                {
                                    if (bDeny)
                                    {
                                        moDbHelper.saveDiscountDirRelation((int)id, this.goRequest["Groups"], true, Cms.dbHelper.PermissionLevel.Denied);
                                    }

                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    moDbHelper.saveDiscountDirRelation((int)id, this.goRequest["Items"], false);
                                    break;
                                }
                        }

                        oElmt2 = base.addSelect(ref oFrmGrp1, "Groups", false, "User Groups", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        // Dim nxxx As Integer = moDbhelper.exeProcessSQLScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " & id & ") AND (nDirId = 0)")
                        if (!(Conversions.ToDouble(moDbHelper.ExeProcessSqlScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " + id + ") AND (nDirId = 0)")) > 0d))
                        {
                            base.addOption(ref oElmt2, "<<All Users>>", 0.ToString());
                        }

                        sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Group') AND" + " (((SELECT nDiscountDirRelationKey" + " FROM tblCartDiscountDirRelations" + " WHERE (nDiscountId = " + id + ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" + "ORDER BY cDirName";
                        var argoDr = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt2, ref argoDr, "name", "value");
                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "RelatedObjects", "", "All items with permissions to access page");
                        XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Related", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT tblCartDiscountDirRelations.nDiscountDirRelationKey AS value, " + " CASE WHEN tblCartDiscountDirRelations.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" + " FROM tblCartDiscountDirRelations LEFT OUTER JOIN" + "  tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey" + " WHERE (tblCartDiscountDirRelations.ndiscountid = " + id + ")" + cDenyFilter + " ORDER BY cDirName";
                        var argoDr1 = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt4, ref argoDr1, "name", "value");
                        if (bDeny)
                        {
                            oElmt5 = base.addSelect(ref oFrmGrp3, "Items", false, "Denied", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                            sSql = "SELECT tblCartDiscountDirRelations.nDiscountDirRelationKey AS value, " + " CASE WHEN tblCartDiscountDirRelations.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" + " FROM tblCartDiscountDirRelations LEFT OUTER JOIN" + "  tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey" + " WHERE (tblCartDiscountDirRelations.ndiscountid = " + id + ") and nPermLevel = 0 ORDER BY cDirName";
                            var argoDr2 = moDbHelper.getDataReader(sSql);
                            base.addOptionsFromSqlDataReader(ref oElmt5, ref argoDr2, "name", "value");
                        }

                        base.Instance.InnerXml = "<Dirs/>";
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmDiscountDirRelations", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmShippingDirRelations(long id, string dname)
                {
                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    XmlElement oElmt5;
                    string sSql;
                    var bDeny = default(bool);
                    string cProcessInfo = "";
                    string cDenyFilter = "";
                    try
                    {
                        base.NewFrm("EditShippingDirRelations");
                        if (moDbHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel"))
                        {
                            bDeny = true;
                            cDenyFilter = " and nPermLevel <> 0";
                        }

                        base.submission("EditInputPageRights", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditDirs", "3col", "User Group Relations for Shipping Method " + dname);
                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the user groups you want to have access to this shipping method");
                        XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditDirs", "DirButtons", "Buttons");
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Allow Selected >", "", "PermissionButtons");
                        if (bDeny)
                        {
                            base.addSubmit(ref oFrmGrp2, "DenySelected", "Deny Selected >", "", "PermissionButtons");
                        }

                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons");
                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    if (!string.IsNullOrEmpty(this.goRequest["Groups"]))
                                    {
                                        moDbHelper.saveShippingDirRelation((int)id, this.goRequest["Groups"]);
                                    }

                                    if (!string.IsNullOrEmpty(this.goRequest["Roles"]))
                                    {
                                        moDbHelper.saveShippingDirRelation((int)id, this.goRequest["Roles"]);
                                    }

                                    break;
                                }

                            case "DenySelected":
                                {
                                    if (bDeny)
                                    {
                                        if (!string.IsNullOrEmpty(this.goRequest["Groups"]))
                                        {
                                            moDbHelper.saveShippingDirRelation((int)id, this.goRequest["Groups"], true, Cms.dbHelper.PermissionLevel.Denied);
                                        }

                                        if (!string.IsNullOrEmpty(this.goRequest["Roles"]))
                                        {
                                            moDbHelper.saveShippingDirRelation((int)id, this.goRequest["Roles"], true, Cms.dbHelper.PermissionLevel.Denied);
                                        }
                                    }

                                    break;
                                }

                            case "RemoveSelected":
                                {
                                    moDbHelper.saveShippingDirRelation((int)id, this.goRequest["Items"], false);
                                    break;
                                }
                        }

                        oElmt2 = base.addSelect(ref oFrmGrp1, "Groups", false, "User Groups", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        // Dim nxxx As Integer = moDbhelper.exeProcessSQLScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " & id & ") AND (nDirId = 0)")
                        if (!(Conversions.ToDouble(moDbHelper.ExeProcessSqlScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " + id + ") AND (nDirId = 0)")) > 0d))
                        {
                            base.addOption(ref oElmt2, "<<All Users>>", 0.ToString());
                        }

                        sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Group') AND" + " (((SELECT nCartShippingPermissionKey" + " FROM tblCartShippingPermission" + " WHERE (nShippingMethodId = " + id + ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" + "ORDER BY cDirName";
                        var argoDr = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt2, ref argoDr, "name", "value");
                        oElmt2 = base.addSelect(ref oFrmGrp1, "Roles", false, "User Roles", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Role') AND" + " (((SELECT nCartShippingPermissionKey" + " FROM tblCartShippingPermission" + " WHERE (nShippingMethodId = " + id + ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" + "ORDER BY cDirName";
                        var argoDr1 = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt2, ref argoDr1, "name", "value");
                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "RelatedObjects", "", "All Groups with permissions for Shipping Method");
                        XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Allowed", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT tblCartShippingPermission.nCartShippingPermissionKey AS value, " + " CASE WHEN tblCartShippingPermission.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" + " FROM tblCartShippingPermission LEFT OUTER JOIN" + " tblDirectory ON tblCartShippingPermission.nDirId = tblDirectory.nDirKey" + " WHERE (tblCartShippingPermission.nShippingMethodId = " + id + ")" + cDenyFilter + " ORDER BY cDirName";
                        var argoDr2 = moDbHelper.getDataReader(sSql);
                        base.addOptionsFromSqlDataReader(ref oElmt4, ref argoDr2, "name", "value");
                        if (bDeny)
                        {
                            oElmt5 = base.addSelect(ref oFrmGrp3, "Items", false, "Denied", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                            sSql = "SELECT tblCartShippingPermission.nCartShippingPermissionKey AS value, " + " CASE WHEN tblCartShippingPermission.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" + " FROM tblCartShippingPermission LEFT OUTER JOIN" + "  tblDirectory ON tblCartShippingPermission.nDirId = tblDirectory.nDirKey" + " WHERE (tblCartShippingPermission.nShippingMethodId = " + id + ") and nPermLevel = 0 ORDER BY cDirName";
                            var argoDr3 = moDbHelper.getDataReader(sSql);
                            base.addOptionsFromSqlDataReader(ref oElmt5, ref argoDr3, "name", "value");
                        }

                        base.Instance.InnerXml = "<Dirs/>";
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmShippingDirRelations", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmEditDirectoryContact(long id = 0L, int nUID = 0, string xFormPath = "/xforms/directory/UserContact.xml")
                {
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("EditContact");
                        base.load(xFormPath, this.myWeb.maCommonFolders);
                        if (id > 0L)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, id);
                        }

                        // Add the countries list to the form
                        if (base.moXformElmt.SelectSingleNode("//select1[@bind='cContactCountry']") is object)
                        {
                            var oEc = new Cms.Cart(ref this.myWeb);
                            Cms.xForm argoXform = (Cms.xForm)this;
                            XmlElement argoCountriesDropDown = (XmlElement)base.moXformElmt.SelectSingleNode("//select1[@bind='cContactCountry']");
                            oEc.populateCountriesDropDown(ref argoXform, ref argoCountriesDropDown, "Billing Address");
                            oEc.close();
                            oEc = null;
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
                                catch (Exception exp)
                                {
                                    var argoNode = base.moXformElmt.SelectSingleNode("group");
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Successfully Updated", true);
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmDiscountRule", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmContentLocationDetail(int nPageId, int nContentId)
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("EditContent");
                        base.submission("EditContent", "", "post", "");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "RelationshipType", "", "Please select a relationship type.");
                        var oSelElmt = base.addSelect1(ref oFrmElmt, "nRelType", true, "Type", "required", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Primary", 1.ToString());
                        base.addOption(ref oSelElmt, "Link", 0.ToString());
                        XmlElement argoBindParent = null;
                        base.addBind("nRelType", "nRelType", "true()", "number", oBindParent: ref argoBindParent);
                        base.addSubmit(ref oFrmElmt, "Save", "Save Changes");
                        int nPrimary = 0;
                        string cRes = Strings.LCase(moDbHelper.ExeProcessSqlScalar("Select bPrimary from tblContentLocation where nStructId = " + nPageId + " and nContentId = " + nContentId));
                        if (cRes == "true")
                        {
                            nPrimary = 1;
                        }

                        base.Instance.InnerXml = "<nRelType>" + nPrimary + "</nRelType>";
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                bool bResult = Conversions.ToBoolean(Interaction.IIf(Conversions.ToDouble(base.Instance.SelectSingleNode("nRelType").InnerText) == 1d, true, false));
                                bResult = moDbHelper.updateLocationsDetail(nContentId, nPageId, bResult);
                                this.valid = true;
                                if (!bResult)
                                {
                                    XmlNode argoNode = oFrmElmt;
                                    base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "Cannot remove the only Primary Relationship", true);
                                    this.valid = false;
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

                // Public Function xFrmPreviewNewsLetter(ByVal nPageId As Integer, ByRef oPageDetail As XmlElement) As XmlElement
                // Dim oFrmElmt As XmlElement


                // Dim cProcessInfo As String = ""
                // Try
                // MyBase.NewFrm("SendNewsLetter")

                // MyBase.submission("SendNewsLetter", "", "post", "")

                // oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Unpersonalised", "", "Send Unpersonalised")

                // Dim oElmt As XmlElement
                // oElmt = MyBase.addInput(oFrmElmt, "cEmail", True, "Email address to send to", "long")
                // MyBase.addBind("cEmail", "cEmail")
                // oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"))
                // MyBase.addSubmit(oFrmElmt, "SendUnpersonalised", "Send Unpersonalised")

                // ' Uncomment for personalised
                // ' ''oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Personalised", "", "Send Personalised")
                // ' ''Dim cSQL As String = "SELECT tblDirectory.nDirKey, tblDirectory.cDirName" & _
                // ' ''" FROM tblDirectory INNER JOIN" & _
                // ' ''" tblDirectoryRelation ON tblDirectory.nDirKey = tblDirectoryRelation.nDirChildId INNER JOIN" & _
                // ' ''" tblDirectory Role ON tblDirectoryRelation.nDirParentId = Role.nDirKey" & _
                // ' ''" WHERE (tblDirectory.cDirSchema = N'User') AND (Role.cDirSchema = N'Role') AND (Role.cDirName = N'Administrator')" & _
                // ' ''" ORDER BY tblDirectory.cDirName"
                // ' ''Dim oDre As SqlDataReader = moDbhelper.getDataReader(cSQL)
                // ' ''Dim oSelElmt As XmlElement = MyBase.addSelect1(oFrmElmt, "cUsers", True, "Select admin user to send to", "short", ApperanceTypes.Minimal)
                // ' ''Do While oDre.Read
                // ' ''    MyBase.addOption(oSelElmt, oDre(1), oDre(0))
                // ' ''Loop
                // ' ''MyBase.addBind("cUsers", "cUsers")
                // ' ''MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

                // MyBase.Instance.InnerXml = "<cEmail/><cUsers/>"

                // If MyBase.isSubmitted Then
                // MyBase.updateInstanceFromRequest()
                // MyBase.validate()
                // Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cEmail")
                // If Not is_valid_email(oEmailElmt.InnerText) Then
                // MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
                // MyBase.valid = False
                // End If
                // If MyBase.valid Then
                // Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                // Dim cEmail As String = MyBase.Instance.SelectSingleNode("cEmail").InnerText
                // 'first we will only deal with unpersonalised
                // Dim oMessager As New Messaging
                // 'get the subject
                // Dim cSubject As String = ""
                // Dim oMessaging As New Protean.Messaging
                // If oMessaging.SendSingleMail_Queued(nPageId, moMailConfig("MailingXsl"), cEmail, moMailConfig("SenderEmail"), moMailConfig("SenderName"), cSubject) Then
                // 'add mssage and return to form so they can sen another

                // Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")

                // oMsgElmt.SetAttribute("type", "Message")
                // oMsgElmt.InnerText = "Messages Sent"
                // oPageDetail.AppendChild(oMsgElmt)
                // End If
                // End If
                // End If

                // MyBase.addValues()
                // Return MyBase.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                // Public Function xFrmSendNewsLetter(ByVal nPageId As Integer, ByVal cPageName As String, ByVal cDefaultEmail As String, ByVal cDefaultEmailName As String, ByRef oPageDetail As XmlElement) As XmlElement
                // Dim oFrmElmt As XmlElement
                // Dim oCol1 As XmlElement
                // Dim oCol2 As XmlElement

                // Dim cProcessInfo As String = ""
                // Try
                // MyBase.NewFrm("SendNewsLetter")

                // MyBase.submission("SendNewsLetter", "", "post", "")

                // oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Groups", "2col", "Please select a group(s) to send to.")

                // cDefaultEmail = Trim(cDefaultEmail)

                // oCol1 = MyBase.addGroup(oFrmElmt, "", "col1", "")
                // oCol2 = MyBase.addGroup(oFrmElmt, "", "col2", "")

                // Dim oElmt As XmlElement
                // oElmt = MyBase.addInput(oCol1, "cDefaultEmail", True, "Email address to send from", "required long")
                // MyBase.addBind("cDefaultEmail", "cDefaultEmail", "true()")
                // oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                // Dim oElmt2 As XmlElement
                // oElmt2 = MyBase.addInput(oCol1, "cDefaultEmailName", True, "Name to send from", "required long")
                // MyBase.addBind("cDefaultEmailName", "cDefaultEmailName", "true()")
                // oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

                // oElmt2 = MyBase.addInput(oCol1, "cSubject", True, "Subject", "required long")
                // MyBase.addBind("cSubject", "cSubject", "true()")
                // oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))


                // Dim cSQL As String = "SELECT nDirKey, cDirName  FROM tblDirectory WHERE (cDirSchema = 'Group') ORDER BY cDirName"
                // Dim oDre As SqlDataReader = moDbHelper.getDataReader(cSQL)
                // Dim oSelElmt As XmlElement = MyBase.addSelect(oCol2, "cGroups", True, "Select Groups to send to", "required multiline", ApperanceTypes.Full)
                // Do While oDre.Read
                // MyBase.addOption(oSelElmt, oDre(1), oDre(0))
                // Loop
                // MyBase.addBind("cGroups", "cGroups", "true()")

                // oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Send", "", "")

                // MyBase.addSubmit(oFrmElmt, "SendUnpersonalised", "Send Unpersonalised")
                // ' Uncomment for personalised
                // 'MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

                // MyBase.Instance.InnerXml = "<cGroups/><cDefaultEmail>" & cDefaultEmail & "</cDefaultEmail><cDefaultEmailName>" & cDefaultEmailName & "</cDefaultEmailName><cSubject>" & cPageName & "</cSubject>"

                // If MyBase.isSubmitted Then
                // MyBase.updateInstanceFromRequest()
                // MyBase.validate()
                // Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmail")
                // If Not Tools.Text.IsEmail(oEmailElmt.InnerText.Trim()) Then
                // MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
                // MyBase.valid = False
                // End If
                // If MyBase.valid Then
                // Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                // 'get the individual elements
                // Dim oMessaging As New Protean.Messaging
                // 'First we need to get the groups we are sending to
                // Dim oGroupElmt As XmlElement = MyBase.Instance.SelectSingleNode("cGroups")
                // Dim oFromEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmail")
                // Dim oFromNameElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmailName")
                // Dim oSubjectElmt As XmlElement = MyBase.Instance.SelectSingleNode("cSubject")
                // 'get the email addresses for these groups

                // Dim bResult As Boolean = oMessaging.SendMailToList_Queued(nPageId, moMailConfig("MailingXsl"), oGroupElmt.InnerText, oFromEmailElmt.InnerText, oFromNameElmt.InnerText, oSubjectElmt.InnerText)


                // ' Log the result
                // If bResult Then
                // 'moDbHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, nPageId, , oGroupElmt.InnerText)
                // moDbHelper.CommitLogToDB(dbHelper.ActivityType.NewsLetterSent, myWeb.mnUserId, myWeb.moSession.SessionID, Now, myWeb.mnPageId, 0, "", True)
                // Dim cGroupStr As String = "<Groups><Group>" & Replace(oGroupElmt.InnerText, ",", "</Group><Group>") & "</Group></Groups>"
                // 'add mssage and return to form so they can sen another
                // Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")
                // oMsgElmt.SetAttribute("type", "Message")
                // oMsgElmt.InnerText = "Messages Sent"
                // oPageDetail.AppendChild(oMsgElmt)
                // End If
                // End If
                // End If

                // MyBase.addValues()
                // Return MyBase.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                public XmlElement xFrmAdminOptOut()
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("OptOut");
                        base.submission("OptOut", "", "post", "return form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Existing", "", "Add Opt-Out Address");
                        XmlElement oElmt;
                        oElmt = base.addInput(ref oFrmElmt, "cEmail", true, "Add Address", "long");
                        oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"));
                        XmlElement argoBindParent = null;
                        base.addBind("cEmail", "cEmail", oBindParent: ref argoBindParent);
                        base.addSubmit(ref oFrmElmt, "AddOptOut", "Add to List");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Existing", "", "Existing Opt-Out Addresses");
                        var oSelElmt = base.addSelect(ref oFrmElmt, "OptIn", true, "Addresses", "block scroll", Protean.xForm.ApperanceTypes.Full);
                        string cSQL = "SELECT EmailAddress FROM tblOptOutAddresses ORDER BY EmailAddress";
                        var oDre = moDbHelper.getDataReader(cSQL);
                        while (oDre.Read())
                            base.addOption(ref oSelElmt, Conversions.ToString(oDre[0]), Conversions.ToString(oDre[0]));
                        XmlElement argoBindParent1 = null;
                        base.addBind("OptIn", "OptIn", oBindParent: ref argoBindParent1);
                        base.addSubmit(ref oFrmElmt, "RemoveOptOut", "Remove from List");
                        base.Instance.InnerXml = "<cEmail/><OptIn/>";
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            XmlElement oEmailElmt = (XmlElement)base.Instance.SelectSingleNode("cEmail");
                            if (base.valid)
                            {
                                if (!string.IsNullOrEmpty(oEmailElmt.InnerText))
                                {
                                    if (!IsEmail(oEmailElmt.InnerText))
                                    {
                                        XmlNode argoNode = oElmt;
                                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Incorrect Email Address Supplied");
                                    }
                                    else if (moDbHelper.AddInvalidEmail(oEmailElmt.InnerText))
                                    {
                                        XmlNode argoNode1 = oElmt;
                                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Hint, oEmailElmt.InnerText + " Added");
                                        base.addOption(ref oSelElmt, oEmailElmt.InnerText, oEmailElmt.InnerText);
                                        oEmailElmt.InnerText = "";
                                    }
                                    else
                                    {
                                        XmlNode argoNode2 = oElmt;
                                        base.addNote(ref argoNode2, Protean.xForm.noteTypes.Hint, oEmailElmt.InnerText + "Already Exists");
                                        oEmailElmt.InnerText = "";
                                    }
                                }

                                XmlElement oRemoveElmt = (XmlElement)base.Instance.SelectSingleNode("OptIn");
                                if (!string.IsNullOrEmpty(oRemoveElmt.InnerText))
                                {
                                    moDbHelper.RemoveInvalidEmail(oRemoveElmt.InnerText);
                                    XmlNode argoNode3 = (XmlNode)oSelElmt;
                                    base.addNote(ref argoNode3, Protean.xForm.noteTypes.Hint, oRemoveElmt.InnerText + " Removed");
                                    oRemoveElmt.InnerText = "";
                                }
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

                public XmlElement xFrmSchedulerItem(string cActionType, int nSiteId, string sSchedCon, int nID = 0)
                {
                    string cProcessInfo = "";
                    try
                    {
                        var dbh = new Cms.dbHelper(ref this.myWeb);
                        dbh.ResetConnection(sSchedCon);
                        base.NewFrm("EditScheduleItem");
                        base.load("/xforms/ScheduledItems/" + cActionType + ".xml", this.myWeb.maCommonFolders);
                        if (nID > 0)
                        {
                            base.Instance.InnerXml = dbh.getObjectInstance(Cms.dbHelper.objectTypes.ScheduledItem, nID);
                        }

                        // get menu
                        XmlElement oPageSelect = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::select1[@bind='nPageId']");
                        if (oPageSelect is object)
                        {
                            MenuSelect(ref oPageSelect);
                        }
                        // get files
                        XmlElement oXSLSelect = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::select1[@bind='cXSLPath']");
                        if (oXSLSelect is object)
                        {
                            FileList("/xsl/feeds/", ref oXSLSelect, ".xsl");
                        }
                        // set siteid
                        XmlElement oSiteIDElmt = (XmlElement)base.Instance.SelectSingleNode("descendant-or-self::nWebsite");
                        oSiteIDElmt.InnerText = nSiteId.ToString();
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            // check the min interval

                            NameValueCollection oSchedulerConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/scheduler");
                            XmlElement oMainFrequency = (XmlElement)base.Instance.SelectSingleNode("tblActions/nFrequency");
                            if (oMainFrequency is object)
                            {
                                if (!(Conversions.ToInteger(oMainFrequency.InnerText) >= Conversions.ToInteger(oSchedulerConfig["MinimumInterval"])))
                                {
                                    base.valid = false;
                                }
                            }

                            if (base.valid)
                            {
                                // now we need to save it 
                                dbh.setObjectInstance(Cms.dbHelper.objectTypes.ScheduledItem, base.Instance);
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmSchedulerItem", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }
                #region Temp subs for Scheduled Items
                public void MenuSelect(ref XmlElement oSelect)
                {
                    try
                    {
                        var oWeb = new Cms();
                        oWeb.Open();
                        var oMenuElmt = this.myWeb.GetStructureXML((long)this.myWeb.mnUserId, 0L, 0L, "Site", false, false, false, true, false, "MenuItem", "Menu");
                        foreach (XmlElement oMenuItem in oMenuElmt.SelectNodes("MenuItem"))
                            MenuReiterate(oMenuItem, ref oSelect, 0);
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "MenuSelect", ex, "", "", Protean.stdTools.gbDebug);
                    }
                }

                public void MenuReiterate(XmlElement oMenuItem, ref XmlElement oSelect, int nDepth)
                {
                    try
                    {
                        string cNameString = "";
                        int i;
                        var loopTo = nDepth;
                        for (i = 0; i <= loopTo; i++)
                            cNameString += "-";
                        base.addOption(ref oSelect, cNameString + oMenuItem.GetAttribute("name"), oMenuItem.GetAttribute("id"));
                        foreach (XmlElement oSubelmt in oMenuItem.SelectNodes("MenuItem"))
                            MenuReiterate(oSubelmt, ref oSelect, nDepth + 1);
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "MenuReiterate", ex, "", "", Protean.stdTools.gbDebug);
                    }
                }

                public void FileList(string cInitialFolder, ref XmlElement oSelect, string cFileExt)
                {
                    try
                    {
                        string oNameStr = "";
                        string cBasePath = this.goServer.MapPath("/" + cInitialFolder);
                        string cCommonPath = this.goServer.MapPath("/ewcommon" + cInitialFolder);
                        var dir = new DirectoryInfo(cBasePath);
                        if (!dir.Exists)
                        {
                            dir = new DirectoryInfo(cCommonPath);
                        }

                        var files = dir.GetFiles();
                        FileInfo fi;
                        foreach (var currentFi in files)
                        {
                            fi = currentFi;
                            if ((fi.Extension ?? "") == (cFileExt ?? ""))
                            {
                                if (!oNameStr.Contains(fi.Name + ","))
                                {
                                    base.addOption(ref oSelect, Strings.Replace(fi.Name, cFileExt, ""), fi.FullName);
                                    oNameStr += fi.Name + ",";
                                }
                            }
                        }

                        dir = new DirectoryInfo(cCommonPath);
                        files = dir.GetFiles();
                        foreach (var currentFi1 in files)
                        {
                            fi = currentFi1;
                            if ((fi.Extension ?? "") == (cFileExt ?? ""))
                            {
                                if (!oNameStr.Contains(fi.Name + ","))
                                {
                                    base.addOption(ref oSelect, Strings.Replace(fi.Name, cFileExt, ""), fi.FullName);
                                    oNameStr += fi.Name + ",";
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "cInitialFolder", ex, "", "", Protean.stdTools.gbDebug);
                    }
                }
                #endregion

                public XmlElement xFrmFeedItem(int nContentId = 0, XmlElement oInstanceElmt = null, int nPageId = 0, string cURL = "")
                {
                    string cProcessInfo = "";
                    bool existingIsDifferent = true;
                    try
                    {
                        base.NewFrm("EditFeedItem");
                        base.load("/xforms/content/feeditem.xml", this.myWeb.maCommonFolders);
                        var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                        if (nContentId > 0)
                        {
                            existingInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, nContentId);
                            base.Instance.InnerXml = existingInstance.InnerXml;
                        }

                        if (oInstanceElmt is object)
                        {
                            base.Instance.InnerXml = oInstanceElmt.InnerXml;
                        }

                        if (!string.IsNullOrEmpty(cURL))
                        {
                            XmlElement oURLElmt = (XmlElement)base.Instance.SelectSingleNode("descendant-or-self::cContentXmlBrief/Content/url");
                            if (oURLElmt is object)
                            {
                                oURLElmt.InnerText = cURL;
                            }
                        }
                        // going to override som stuff here since we will be supplying the instance
                        if (base.isSubmitted() | oInstanceElmt is object)
                        {
                            if (oInstanceElmt is null)
                                base.updateInstanceFromRequest();
                            if (oInstanceElmt is null)
                                base.validate();
                            if (nContentId > 0 && oInstanceElmt is object && !string.IsNullOrEmpty(existingInstance.InnerXml) && oInstanceElmt.SelectSingleNode("//cContentXmlBrief") is object && existingInstance.SelectSingleNode("//cContentXmlBrief") is object && oInstanceElmt.SelectSingleNode("//cContentXmlDetail") is object && existingInstance.SelectSingleNode("//cContentXmlDetail") is object && (oInstanceElmt.SelectSingleNode("//cContentXmlBrief").InnerXml ?? "") == (existingInstance.SelectSingleNode("//cContentXmlBrief").InnerXml ?? "") && (oInstanceElmt.SelectSingleNode("//cContentXmlDetail").InnerXml ?? "") == (existingInstance.SelectSingleNode("//cContentXmlDetail").InnerXml ?? ""))






                            {
                                // Do nothing - don't update it.
                                existingIsDifferent = false;
                            }

                            if (base.valid | oInstanceElmt is object & existingIsDifferent)
                            {
                                // now we need to save it 
                                int id;
                                if (nContentId > 0)
                                {
                                    id = nContentId;
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance, id);
                                    base.moXformElmt.SetAttribute("itemupdated", "true");
                                }
                                else
                                {
                                    id = Conversions.ToInteger(moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, base.Instance));
                                }

                                if (!(id == 0) & !(nPageId == 0))
                                {
                                    moDbHelper.setContentLocation(nPageId, id, true);
                                }
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmFeedItem", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                // Moved to edit content instead

                public XmlElement xFrmEditUserSubscription(int nSubId)
                {
                    string cProcessInfo = "";
                    try
                    {
                        if (Strings.LCase(moRequest["reset"]) == "true")
                        {
                            this.myWeb.moSession["tempInstance"] = null;
                        }

                        base.NewFrm("EditUserSubscription");
                        base.bProcessRepeats = false;
                        base.load("/xforms/Subscription/EditSubscription.xml", this.myWeb.maCommonFolders);
                        if (nSubId > 0)
                        {
                            base.bProcessRepeats = true;
                            if (this.myWeb.moSession["tempInstance"] is null)
                            {
                                var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                existingInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Subscription, nSubId).Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "").Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                                base.LoadInstance(existingInstance);
                                this.myWeb.moSession["tempInstance"] = base.Instance;
                            }
                            else
                            {
                                base.LoadInstance(this.myWeb.moSession["tempInstance"]);
                            }
                        }

                        this.moXformElmt.SelectSingleNode("descendant-or-self::instance").InnerXml = base.Instance.InnerXml;
                        int i = 1;
                        bool bDone = false;
                        string cItems = "";
                        long initialSubContentId = Conversions.ToLong("0" + base.Instance.SelectSingleNode("tblSubscription/nSubContentId").InnerText);
                        if (base.isSubmitted() | base.isTriggered)
                        {
                            base.updateInstanceFromRequest();
                            long ContentId = Conversions.ToLong(base.Instance.SelectSingleNode("tblSubscription/nSubContentId").InnerText);
                            var ContentXml = this.myWeb.moPageXml.CreateElement("Content");
                            ContentXml.InnerXml = moDbHelper.getContentBrief((int)ContentId);
                            if (initialSubContentId != ContentId)
                            {
                                // Now we populate the instance
                                base.Instance.SelectSingleNode("tblSubscription/cSubName").InnerText = ContentXml.SelectSingleNode("Content/Name").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/cSubXml").InnerXml = ContentXml.InnerXml;
                                // dStartDate Populated by form
                                base.Instance.SelectSingleNode("tblSubscription/nPeriod").InnerText = ContentXml.SelectSingleNode("Content/Duration/Length").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/cPeriodUnit").InnerText = ContentXml.SelectSingleNode("Content/Duration/Unit").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/nMinimumTerm").InnerText = ContentXml.SelectSingleNode("Content/Duration/MinimumTerm").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/nRenewalTerm").InnerText = ContentXml.SelectSingleNode("Content/Duration/RenewalTerm").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/nValueNet").InnerText = ContentXml.SelectSingleNode("Content/SubscriptionPrices/Price[@type='sale']").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/cRenewalStatus").InnerText = ContentXml.SelectSingleNode("Content/Type").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/dPublishDate").InnerText = base.Instance.SelectSingleNode("tblSubscription/dStartDate").InnerText;
                            }

                            if (nSubId == 0)
                            {
                                // we are creating a new subscription
                                // first we get the subscription content XML
                                base.Instance.SelectSingleNode("tblSubscription/nDirId").InnerText = this.myWeb.moRequest["userId"];
                                base.Instance.SelectSingleNode("tblSubscription/nDirType").InnerText = "user";
                                base.Instance.SelectSingleNode("tblSubscription/dPublishDate").InnerText = base.Instance.SelectSingleNode("tblSubscription/dStartDate").InnerText;

                                // Calculate Renewal Date
                                var oSub = new Cms.Cart.Subscriptions();
                                var dSubEndDate = oSub.SubscriptionEndDate(Conversions.ToDate(base.Instance.SelectSingleNode("tblSubscription/dStartDate").InnerText), (XmlElement)ContentXml.SelectSingleNode("Content"));
                                base.Instance.SelectSingleNode("tblSubscription/dExpireDate").InnerText = Protean.stdTools.xmlDate((object)dSubEndDate);
                            }
                            // updating an existing subscription
                            else if (base.Instance.SelectSingleNode("tblSubscription/cRenewalStatus").InnerText != "Cancelled")
                            {
                                base.Instance.SelectSingleNode("tblSubscription/nStatus").InnerText = "1";
                            }

                            if (base.isSubmitted())
                            {
                                base.validate();
                            }

                            if (base.valid)
                            {
                                int nCId = Conversions.ToInteger(moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Subscription, base.Instance, (long)nSubId));
                                if (base.Instance.SelectSingleNode("tblSubscription/cRenewalStatus").InnerText != "Cancelled")
                                {
                                    if (nSubId > 0)
                                    {
                                        foreach (XmlElement oElmt in base.Instance.SelectNodes("tblSubscription/cSubXml/Content/UserGroups/Group[@id!='']"))
                                        {
                                            int nGrpID = Conversions.ToInteger(oElmt.Attributes["id"].Value);
                                            this.myWeb.moDbHelper.saveDirectoryRelations(Conversions.ToInteger(base.Instance.SelectSingleNode("tblSubscription/nDirId").InnerText), nGrpID.ToString());
                                        }
                                    }
                                }

                                this.myWeb.moSession["tempInstance"] = null;
                            }
                            else if (base.isTriggered)
                            {
                                // we have clicked a trigger so we must update the instance
                                base.updateInstanceFromRequest();
                                // lets save the instance
                                this.goSession["tempInstance"] = base.Instance;
                            }
                            else
                            {
                                this.goSession["tempInstance"] = base.Instance;
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditUserSubscription", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmRenewSubscription(string nSubscriptionId)
                {
                    string cProcessInfo = "";
                    try
                    {
                        var oSub = new Cms.Cart.Subscriptions(ref this.myWeb);
                        base.NewFrm("RenewSubscription");
                        base.submission("RenewSubscription", "", "post");
                        XmlElement oFrmElmt;
                        var argoParentElmt = base.Instance;
                        oSub.GetSubscriptionDetail(ref argoParentElmt, Conversions.ToInteger(nSubscriptionId));
                        base.Instance = argoParentElmt;
                        object SubXml = base.Instance.FirstChild;
                        // calculate new expiry date

                        var renewInterval = DateInterval.Day;
                        switch (SubXml.GetAttribute("periodUnit"))
                        {
                            case "Week":
                                {
                                    renewInterval = DateInterval.WeekOfYear;
                                    break;
                                }

                            case "Year":
                                {
                                    renewInterval = DateInterval.Year;
                                    break;
                                }
                        }

                        long SubId = Conversions.ToLong(SubXml.GetAttribute("id"));
                        var dNewStart = DateAndTime.DateAdd(DateInterval.Day, 1d, Conversions.ToDate(SubXml.GetAttribute("expireDate")));
                        var dNewEnd = DateAndTime.DateAdd(renewInterval, Conversions.ToInteger(SubXml.GetAttribute("period")), Conversions.ToDate(SubXml.GetAttribute("expireDate")));
                        double RenewalCost = Conversions.ToDouble(SubXml.GetAttribute("value"));
                        SubXml.setAttribute("newStart", Protean.stdTools.xmlDate(dNewStart));
                        SubXml.setAttribute("newExpire", Protean.stdTools.xmlDate(dNewEnd));
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "RenewSubscription");
                        base.addInput(ref oFrmElmt, "nUserID", false, "UserId", "hidden");
                        base.addInput(ref oFrmElmt, "nSubscriptionId", false, "SubscriptionId", "hidden");
                        var oSelElmt = base.addSelect(ref oFrmElmt, "emailClient", true, "", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Email Renewal Invoice", "yes");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "Renew Subscription", true, "renew-sub");
                        base.addSubmit(ref oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left");
                        base.addSubmit(ref oFrmElmt, "Confirm", "Confirm Renewal", "Confirm", "btn-success principle", "fa-repeat");
                        if (this.isSubmitted())
                        {
                            if (base.getSubmitted() == "Back")
                            {
                                return base.moXformElmt;
                                this.myWeb.msRedirectOnEnd = "/?ewCmd=RenewSubscription";
                            }
                            else if (base.getSubmitted() == "Confirm")
                            {
                                bool bEmailClient = false;
                                if (this.myWeb.moRequest["emailClient"] == "yes")
                                    bEmailClient = true;
                                oSub.RenewSubscription(Conversions.ToLong(nSubscriptionId), bEmailClient);
                                base.valid = true;
                                return base.moXformElmt;
                            }
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmSchedulerItem", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmConfirmCancelSubscription(string nUserId, string nSubscriptionId, int nCurrentUser, bool bAdminMode)
                {
                    try
                    {
                        if (!Information.IsNumeric(nUserId))
                            nUserId = 0.ToString();
                        if (!Information.IsNumeric(nSubscriptionId))
                            nSubscriptionId = 0.ToString();
                        base.NewFrm("CancelSubscription");
                        base.submission("CancelSubscription", "", "post");
                        XmlElement oFrmElmt;
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "CancelSubscription");
                        base.addInput(ref oFrmElmt, "nUserID", false, "UserId", "hidden");
                        base.addInput(ref oFrmElmt, "nSubscriptionId", false, "SubscriptionId", "hidden");
                        base.addInput(ref oFrmElmt, "cStatedReason", false, "Reason for cancelation");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "Are you sure you wish to cancel this subscription", true);
                        base.addSubmit(ref oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left");
                        base.addSubmit(ref oFrmElmt, "Cancel", "Cancel Subscription", "Cancel", "btn-warning principle", "fa-stop");
                        if (this.isSubmitted())
                        {
                            if (base.getSubmitted() == "Back")
                            {
                                return base.moXformElmt;
                            }
                            else if (base.getSubmitted() == "Cancel")
                            {
                                var oSub = new Cms.Cart.Subscriptions(ref this.myWeb);
                                oSub.CancelSubscription(Conversions.ToInteger(nSubscriptionId), this.myWeb.moRequest["cStatedReason"]);
                                base.valid = true;
                                return base.moXformElmt;
                            }
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmConfirmCancelSubscription", ex, "", bDebug: Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmConfirmExpireSubscription(string nUserId, string nSubscriptionId, int nCurrentUser, bool bAdminMode)
                {
                    try
                    {
                        if (!Information.IsNumeric(nUserId))
                            nUserId = 0.ToString();
                        if (!Information.IsNumeric(nSubscriptionId))
                            nSubscriptionId = 0.ToString();
                        base.NewFrm("CancelSubscription");
                        base.submission("CancelSubscription", "", "post");
                        XmlElement oFrmElmt;
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "ExpireSubscription");
                        base.addInput(ref oFrmElmt, "nUserID", false, "UserId", "hidden");
                        base.addInput(ref oFrmElmt, "nSubscriptionId", false, "SubscriptionId", "hidden");
                        base.addInput(ref oFrmElmt, "cStatedReason", false, "Reason for expiry");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Hint, "Are you sure you wish this subscription to expire", true);
                        base.addSubmit(ref oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left");
                        base.addSubmit(ref oFrmElmt, "Expire", "Expire Subscription", "Expire", "btn-warning principle", "fa-stop");
                        if (this.isSubmitted())
                        {
                            if (base.getSubmitted() == "Back")
                            {
                                return base.moXformElmt;
                            }
                            else if (base.getSubmitted() == "Expire")
                            {
                                var oSub = new Cms.Cart.Subscriptions(ref this.myWeb);
                                oSub.ExpireSubscription(Conversions.ToInteger(nSubscriptionId), this.myWeb.moRequest["cStatedReason"]);
                                base.valid = true;
                                return base.moXformElmt;
                            }
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmConfirmCancelSubscription", ex, "", bDebug: Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCartSettings()
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    Protean.fsHelper oFsh;
                    try
                    {
                        oFsh = new Protean.fsHelper();
                        oFsh.open(this.moPageXML);
                        base.NewFrm("WebSettings");
                        base.submission("WebSettings", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "CartSettings", "", "Cart Settings");
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Any Changes you make to this form risk making this site completely non-functional. Please be sure you know what you are doing before making any changes, or call your web developer for support.");
                        base.addInput(ref oFrmElmt, "ewSiteURL", true, "Site URL");
                        XmlElement argoBindParent = null;
                        base.addBind("ewSiteURL", "cart/add[@key='SiteURL']/@value", "true()", oBindParent: ref argoBindParent);
                        base.addInput(ref oFrmElmt, "ewSecureURL", true, "Secure URL");
                        XmlElement argoBindParent1 = null;
                        base.addBind("ewSecureURL", "cart/add[@key='SecureURL']/@value", "true()", oBindParent: ref argoBindParent1);
                        base.addInput(ref oFrmElmt, "ewTaxRate", true, "Tax Rate");
                        XmlElement argoBindParent2 = null;
                        base.addBind("ewTaxRate", "cart/add[@key='TaxRate']/@value", "true()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oFrmElmt, "ewMerchantName", true, "Merchant Name");
                        XmlElement argoBindParent3 = null;
                        base.addBind("ewMerchantName", "cart/add[@key='MerchantName']/@value", "true()", oBindParent: ref argoBindParent3);
                        base.addInput(ref oFrmElmt, "ewMerchantEmail", true, "Merchant Email");
                        XmlElement argoBindParent4 = null;
                        base.addBind("ewMerchantEmail", "cart/add[@key='MerchantEmail']/@value", "true()", oBindParent: ref argoBindParent4);
                        base.addInput(ref oFrmElmt, "ewOrderEmailSubject", true, "Order Email Subject");
                        XmlElement argoBindParent5 = null;
                        base.addBind("ewOrderEmailSubject", "cart/add[@key='OrderEmailSubject']/@value", "true()", oBindParent: ref argoBindParent5);
                        base.addInput(ref oFrmElmt, "ewOrderNoPrefix", true, "Order No. Prefix");
                        XmlElement argoBindParent6 = null;
                        base.addBind("ewOrderNoPrefix", "cart/add[@key='OrderNoPrefix']/@value", "true()", oBindParent: ref argoBindParent6);
                        base.addInput(ref oFrmElmt, "ewCurrencySymbol", true, "Currency Symbol");
                        XmlElement argoBindParent7 = null;
                        base.addBind("ewCurrencySymbol", "cart/add[@key='CurrencySymbol']/@value", "false()", oBindParent: ref argoBindParent7);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewStockControl", true, "Stock Control", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent8 = null;
                        base.addBind("ewStockControl", "cart/add[@key='StockControl']/@value", "true()", oBindParent: ref argoBindParent8);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewDeposit", true, "Deposit", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "1");
                        base.addOption(ref oSelElmt, "Off", "0");
                        XmlElement argoBindParent9 = null;
                        base.addBind("ewDeposit", "cart/add[@key='Deposit']/@value", "true()", oBindParent: ref argoBindParent9);
                        base.addInput(ref oFrmElmt, "ewDepositAmount", true, "DepositAmount");
                        XmlElement argoBindParent10 = null;
                        base.addBind("ewDepositAmount", "cart/add[@key='DepositAmount']/@value", "false()", oBindParent: ref argoBindParent10);
                        base.addInput(ref oFrmElmt, "ewNotesXForm", true, "Notes Xform");
                        XmlElement argoBindParent11 = null;
                        base.addBind("ewNotesXForm", "cart/add[@key='NotesXForm']/@value", "false()", oBindParent: ref argoBindParent11);
                        base.addInput(ref oFrmElmt, "ewBillingAddressXForm", true, "Billing Address Xform");
                        XmlElement argoBindParent12 = null;
                        base.addBind("ewBillingAddressXForm", "cart/add[@key='BillingAddressXForm']/@value", "false()", oBindParent: ref argoBindParent12);
                        base.addInput(ref oFrmElmt, "ewDeliveryAddressXForm", true, "DeliveryAddress Xform");
                        XmlElement argoBindParent13 = null;
                        base.addBind("ewDeliveryAddressXForm", "cart/add[@key='DeliveryAddressXForm']/@value", "false()", oBindParent: ref argoBindParent13);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewNoDeliveryAddress", true, "Disable Delivery Address", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Yes", "on");
                        base.addOption(ref oSelElmt, "No", "off");
                        XmlElement argoBindParent14 = null;
                        base.addBind("ewNoDeliveryAddress", "cart/add[@key='NoDeliveryAddress']/@value", "true()", oBindParent: ref argoBindParent14);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewEmailReceipts", true, "Email Receipts", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Yes", "on");
                        base.addOption(ref oSelElmt, "No", "off");
                        XmlElement argoBindParent15 = null;
                        base.addBind("ewEmailReceipts", "cart/add[@key='EmailReceipts']/@value", "true()", oBindParent: ref argoBindParent15);
                        base.addInput(ref oFrmElmt, "ewMerchantEmailTemplatePath", true, "Merchant Email Template Path");
                        XmlElement argoBindParent16 = null;
                        base.addBind("ewMerchantEmailTemplatePath", "cart/add[@key='MerchantEmailTemplatePath']/@value", "false()", oBindParent: ref argoBindParent16);
                        base.addInput(ref oFrmElmt, "ewCustomerEmailTemplatePath", true, "Customer Email Template Path");
                        XmlElement argoBindParent17 = null;
                        base.addBind("ewCustomerEmailTemplatePath", "cart/add[@key='CustomerEmailTemplatePath']/@value", "false()", oBindParent: ref argoBindParent17);
                        base.addInput(ref oFrmElmt, "ewPriorityCountries", true, "Priority Countries");
                        XmlElement argoBindParent18 = null;
                        base.addBind("ewPriorityCountries", "cart/add[@key='PriorityCountries']/@value", "true()", oBindParent: ref argoBindParent18);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewSavePayments", true, "SavePayments", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent19 = null;
                        base.addBind("ewSavePayments", "cart/add[@key='SavePayments']/@value", "true()", oBindParent: ref argoBindParent19);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewVatAtUnit", true, "Vat At Unit", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Yes", "yes");
                        base.addOption(ref oSelElmt, "No", "no");
                        XmlElement argoBindParent20 = null;
                        base.addBind("ewVatAtUnit", "cart/add[@key='VatAtUnit']/@value", "true()", oBindParent: ref argoBindParent20);
                        oSelElmt = base.addSelect1(ref oFrmElmt, "ewDiscounts", true, "Discounts", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "On", "on");
                        base.addOption(ref oSelElmt, "Off", "off");
                        XmlElement argoBindParent21 = null;
                        base.addBind("ewDiscounts", "cart/add[@key='Discounts']/@value", "true()", oBindParent: ref argoBindParent21);
                        base.addInput(ref oFrmElmt, "ewPriceModOrder", true, "Price Mod Order");
                        XmlElement argoBindParent22 = null;
                        base.addBind("ewPriceModOrder", "cart/add[@key='PriceModOrder']/@value", "false()", oBindParent: ref argoBindParent22);
                        base.addSubmit(ref oFrmElmt, "", "Save Settings");
                        var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                        DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection("protean/cart");
                        var oImp = new Tools.Security.Impersonate();
                        if (oImp.ImpersonateValidUser(goConfig["AdminAcct"], goConfig["AdminDomain"], goConfig["AdminPassword"], cInGroup: goConfig["AdminGroup"]))
                        {
                            base.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml();

                            // code here to replace any missing nodes
                            // all of the required config settings
                            var aSettingValues = Strings.Split("SiteURL,SecureURL,TaxRate,MerchantName,MerchantEmail,OrderEmailSubject,OrderNoPrefix,CurrencySymbol,StockControl,Deposit,DepositAmount,NotesXForm,BillingAddressXForm,DeliveryAddressXForm,NoDeliveryAddress,MerchantEmailTemplatePath,PriorityCountries,SavePayments,VatAtUnit,Discounts,PriceModOrder", ",");
                            long i;
                            XmlElement oElmt;
                            XmlElement oElmtAft = null;
                            var loopTo = (long)Information.UBound(aSettingValues);
                            for (i = 0L; i <= loopTo; i++)
                            {
                                oElmt = (XmlElement)base.Instance.SelectSingleNode("cart/add[@key='" + aSettingValues[(int)i] + "']");
                                if (oElmt is null)
                                {
                                    oElmt = this.moPageXML.CreateElement("add");
                                    oElmt.SetAttribute("key", aSettingValues[(int)i]);
                                    oElmt.SetAttribute("value", "");
                                    if (oElmtAft is null)
                                    {
                                        base.Instance.FirstChild.InsertBefore(oElmt, base.Instance.FirstChild.FirstChild);
                                    }
                                    else
                                    {
                                        base.Instance.FirstChild.InsertAfter(oElmt, oElmtAft);
                                    }
                                }

                                oElmtAft = oElmt;
                            }

                            if (base.isSubmitted())
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                if (base.valid)
                                {
                                    oCgfSect.SectionInformation.RestartOnExternalChanges = false;
                                    oCgfSect.SectionInformation.SetRawXml(base.Instance.InnerXml);
                                    oCfg.Save();
                                }
                            }

                            oImp.UndoImpersonation();
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmWebSettings", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmFindContentToLocate(string nNewLocationPage, string nFromPage, string bIncludeChildren, string cContentType, string cSearchTerm, ref XmlElement oDetailElement)
                {
                    if (string.IsNullOrEmpty(nFromPage))
                        nFromPage = 0.ToString();
                    if (bIncludeChildren is null)
                        bIncludeChildren = "0";
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt1;
                    XmlElement oSelElmt2;
                    var oTempInstance = this.moPageXML.CreateElement("instance");
                    bool bCascade = false;
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("FindContentToRelate");
                        // nNewLocationPage 
                        var oElement = base.Instance.OwnerDocument.CreateElement("nNewLocationPage");
                        oElement.InnerText = nNewLocationPage;
                        base.Instance.AppendChild(oElement);
                        // nFromPage
                        oElement = base.Instance.OwnerDocument.CreateElement("nFromPage");
                        oElement.InnerText = nFromPage;
                        base.Instance.AppendChild(oElement);
                        // bIncludeChildren
                        oElement = base.Instance.OwnerDocument.CreateElement("bIncludeChildren");
                        oElement.InnerText = bIncludeChildren;
                        base.Instance.AppendChild(oElement);
                        // cContentType 
                        oElement = base.Instance.OwnerDocument.CreateElement("cContentType");
                        oElement.InnerText = cContentType;
                        base.Instance.AppendChild(oElement);
                        // cSearchTerm
                        oElement = base.Instance.OwnerDocument.CreateElement("cSearchTerm");
                        oElement.InnerText = cSearchTerm;
                        base.Instance.AppendChild(oElement);
                        base.submission("AddLocation", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "SearchContent");
                        // oGrp1Elmt = MyBase.addGroup(oFrmElmt, "Criteria", "", "")

                        // Definitions

                        // Hidden
                        base.addInput(ref oFrmElmt, "nNewLocationPage", true, "nNewLocationPage", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nNewLocationPage", "nNewLocationPage", oBindParent: ref argoBindParent);
                        base.addInput(ref oFrmElmt, "type", true, "cContentType", "hidden");
                        XmlElement argoBindParent1 = null;
                        base.addBind("type", "cContentType", oBindParent: ref argoBindParent1);
                        // Textbox
                        base.addInput(ref oFrmElmt, "cSearchTerm", true, "Search Expression");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cSearchTerm", "cSearchTerm", "false()", oBindParent: ref argoBindParent2);

                        // Select
                        // Pages
                        oSelElmt1 = base.addSelect1(ref oFrmElmt, "nFromPage", false, "Page", "siteTree", Protean.xForm.ApperanceTypes.Minimal);
                        XmlElement argoBindParent3 = null;
                        base.addBind("nFromPage", "nFromPage", "true()", oBindParent: ref argoBindParent3);

                        // Checkbox
                        oSelElmt2 = base.addSelect(ref oFrmElmt, "bIncludeChildren", true, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt2, "Search Children", 1.ToString());
                        XmlElement argoBindParent4 = null;
                        base.addBind("bIncludeChildren", "bIncludeChildren", "false()", oBindParent: ref argoBindParent4);

                        // search button
                        base.addSubmit(ref oFrmElmt, "Search", "Search", "Search");
                        base.addValues();
                        oDetailElement.AppendChild(base.moXformElmt);
                        if (base.isSubmitted() | moRequest["cSearched"] == "1")
                        {
                            oDetailElement.AppendChild(xFrmLocateContent(Conversions.ToInteger(nNewLocationPage), Conversions.ToInteger(nFromPage), bIncludeChildren, cContentType, cSearchTerm));
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmFindRelated", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmLocateContent(int nNewLocationPage, int nFromPage, string bIncludeChildren, string cContentType, string cSearchTerm)
                {
                    // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                    XmlElement oFrmElmt;
                    XmlElement oGrp1Elmt;
                    XmlElement oSelElmt2;
                    var oTempInstance = this.moPageXML.CreateElement("instance");
                    bool bCascade = false;
                    string cProcessInfo = "";
                    try
                    {
                        base.NewFrm("SelectContentToLocate");
                        base.submission("SelectLocation", "", "post", "");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content");
                        var oGrp0Elmt = base.addGroup(ref oFrmElmt, "ResultsHeader", sLabel: "&#160;");
                        base.addSubmit(ref oFrmElmt, "Locate", "Add To Page", "Locate");
                        if (base.isSubmitted())
                        {
                            var oItems = Strings.Split(this.myWeb.moRequest["Results"], ",");
                            string cPosition = this.myWeb.moRequest["Position"];
                            int i = 0;
                            var loopTo = oItems.Length - 1;
                            for (i = 0; i <= loopTo; i++)
                                this.myWeb.moDbHelper.setContentLocation(nNewLocationPage, Conversions.ToLong(oItems[i]), false, false, false);
                            base.valid = true;
                        }
                        else
                        {
                            var oResults = moDbHelper.LocateContentSearch(nFromPage, cContentType, bIncludeChildren, cSearchTerm, nNewLocationPage);
                            int nCount = 0;
                            if (oResults is object)
                                nCount = oResults.Count;
                            oGrp0Elmt.SelectSingleNode("label").InnerText = "Results (" + nCount + ")";
                            oGrp1Elmt = base.addGroup(ref oGrp0Elmt, "Results", "horizontal ", "");
                            // nNewLocation
                            var oElement = base.Instance.OwnerDocument.CreateElement("nNewLocationPage");
                            oElement.InnerText = nNewLocationPage.ToString();
                            base.Instance.AppendChild(oElement);

                            // nFromPage
                            oElement = base.Instance.OwnerDocument.CreateElement("nFromPage");
                            oElement.InnerText = nFromPage.ToString();
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "nNewLocationPage", true, "nNewLocationPage", "hidden").SetAttribute("value", nNewLocationPage.ToString());
                            XmlElement argoBindParent = null;
                            base.addBind("nNewLocationPage", "nNewLocationPage", oBindParent: ref argoBindParent);

                            // bIncludeChildren
                            oElement = base.Instance.OwnerDocument.CreateElement("bIncludeChildren");
                            oElement.InnerText = bIncludeChildren;
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "type", true, "type", "hidden").SetAttribute("value", cContentType);
                            XmlElement argoBindParent1 = null;
                            base.addBind("type", "type", oBindParent: ref argoBindParent1);

                            // cContentType 
                            oElement = base.Instance.OwnerDocument.CreateElement("cContentType");
                            oElement.InnerText = cContentType;
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "nFromPage", true, "nFromPage", "hidden").SetAttribute("value", nFromPage.ToString());
                            XmlElement argoBindParent2 = null;
                            base.addBind("nFromPage", "nFromPage", oBindParent: ref argoBindParent2);
                            base.addInput(ref oGrp1Elmt, "bIncludeChildren", true, "bIncludeChildren", "hidden").SetAttribute("value", bIncludeChildren);
                            XmlElement argoBindParent3 = null;
                            base.addBind("bIncludeChildren", "bIncludeChildren", oBindParent: ref argoBindParent3);

                            // cSearchTerm
                            oElement = base.Instance.OwnerDocument.CreateElement("cSearchTerm");
                            oElement.InnerText = cSearchTerm;
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "cSearchTerm", true, "cSearchTerm", "hidden").SetAttribute("value", cSearchTerm);
                            XmlElement argoBindParent4 = null;
                            base.addBind("cSearchTerm", "cSearchTerm", oBindParent: ref argoBindParent4);

                            // cSearched
                            oElement = base.Instance.OwnerDocument.CreateElement("cSearched");
                            oElement.InnerText = "1";
                            base.Instance.AppendChild(oElement);
                            base.addInput(ref oGrp1Elmt, "cSearched", true, "cSearched", "hidden").SetAttribute("value", "1");
                            XmlElement argoBindParent5 = null;
                            base.addBind("cSearched", "cSearched", oBindParent: ref argoBindParent5);
                            base.addSubmit(ref oGrp1Elmt, "Locate", "Add To Page", "Locate");
                            oSelElmt2 = base.addSelect(ref oGrp1Elmt, "Results", true, "", "radiocheckbox multiline selectAll content", Protean.xForm.ApperanceTypes.Full);
                            if (oResults is object)
                            {
                                foreach (XmlElement oResult in oResults)
                                {
                                    if (oSelElmt2.SelectSingleNode("item[value/node()='" + oResult.GetAttribute("id") + "']") is null)
                                    {
                                        base.addOption(ref oSelElmt2, oResult.OuterXml, oResult.GetAttribute("id"), true);
                                    }
                                }
                            }

                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmLocateContent", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCartOrderDownloads()
                {
                    try
                    {
                        // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                        XmlElement oFrmElmt;
                        var oTempInstance = this.moPageXML.CreateElement("instance");
                        bool bCascade = false;
                        string cProcessInfo = "";
                        base.NewFrm("CartActivity");
                        base.submission("SeeReport", "/ewcommon/tools/export.ashx?ewCmd=CartDownload", "get", "");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");
                        var oContElmt = base.addGroup(ref oFrmElmt, "1", "xFormContainer");
                        var oGrp0Elmt = base.addGroup(ref oContElmt, "Criteria", "xFormContainer", "Criteria");
                        base.Instance.InnerXml = "<Criteria ewCmd=\"CartDownload\" output=\"csv\"><dBegin>" + XmlDate(DateAndTime.Now.AddMonths(-1), false) + "</dBegin><dEnd>" + XmlDate(DateAndTime.Now.AddDays(1d), false) + "</dEnd>" + "<cCurrencySymbol/><cOrderType>Order</cOrderType><cOrderStage>6</cOrderStage>" + "</Criteria>";
                        XmlElement argoBindParent = null;
                        base.addBind("dBegin", "Criteria/dBegin", "true()", oBindParent: ref argoBindParent);
                        XmlElement argoBindParent1 = null;
                        base.addBind("dEnd", "Criteria/dEnd", "true()", oBindParent: ref argoBindParent1);
                        XmlElement argoBindParent2 = null;
                        base.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", sType: "string", oBindParent: ref argoBindParent2);
                        XmlElement argoBindParent3 = null;
                        base.addBind("cOrderType", "Criteria/cOrderType", "true()", "string", oBindParent: ref argoBindParent3);
                        XmlElement argoBindParent4 = null;
                        base.addBind("cOrderStage", "Criteria/cOrderStage", "true()", "string", oBindParent: ref argoBindParent4);
                        XmlElement argoBindParent5 = null;
                        base.addBind("format", "Criteria/@output", "false()", "string", oBindParent: ref argoBindParent5);
                        XmlElement argoBindParent6 = null;
                        base.addBind("ewCmd", "Criteria/@ewCmd", "false()", "string", oBindParent: ref argoBindParent6);
                        base.addInput(ref oGrp0Elmt, "ewCmd", true, "ewCmd", "hidden");
                        base.addInput(ref oGrp0Elmt, "dBegin", true, "From", "calendarTime");
                        base.addInput(ref oGrp0Elmt, "dEnd", true, "To", "calendarTime");
                        var oSel1 = base.addSelect1(ref oGrp0Elmt, "cCurrencySymbol", true, "Currency");
                        if (this.myWeb.moConfig["Quote"] != "on")
                        {
                            oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderType", true, "Cart Type");
                            base.addOption(ref oSel1, "Order", "Order");
                            base.addOption(ref oSel1, "Quote", "Quote");
                        }

                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderStage", true, "Cart Type");
                        int i = 0;
                        foreach (Cms.Cart.cartProcess nProcess in Enum.GetValues(typeof(Cms.Cart.cartProcess)))
                        {
                            base.addOption(ref oSel1, nProcess.ToString(), i.ToString());
                            i = i + 1;
                        }

                        oSel1 = base.addSelect1(ref oGrp0Elmt, "format", true, "Output");
                        base.addOption(ref oSel1, "Excel", "excel");
                        base.addOption(ref oSel1, "CSV", "csv");
                        base.addOption(ref oSel1, "XML", "xml");
                        if (this.myWeb.moConfig["Debug"] == "on")
                        {
                            base.addOption(ref oSel1, "Raw XML", "rawxml");
                        }

                        base.addSubmit(ref oGrp0Elmt, "Results", "Download Spreadsheet", "Results");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmCartActivity", ex, "", "", Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCartActivity()
                {
                    try
                    {
                        // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                        XmlElement oFrmElmt;
                        var oTempInstance = this.moPageXML.CreateElement("instance");
                        bool bCascade = false;
                        string cProcessInfo = "";
                        base.NewFrm("CartActivity");
                        base.submission("SeeReport", "", "post", "");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");
                        var oContElmt = base.addGroup(ref oFrmElmt, "1", "xFormContainer");
                        var oGrp0Elmt = base.addGroup(ref oContElmt, "Criteria", "xFormContainer", "Criteria");
                        base.Instance.InnerXml = "<Criteria><dBegin>" + XmlDate(DateAndTime.Now.AddMonths(-1), false) + "</dBegin><dEnd>" + XmlDate(DateAndTime.Now.AddDays(1d), false) + "</dEnd><bSplit>0</bSplit>" + "<cProductType/><nProductId>0</nProductId><cCurrencySymbol/>" + "<nOrderStatus>6,9,17</nOrderStatus><cOrderType>Order</cOrderType>" + "</Criteria>";
                        base.addInput(ref oGrp0Elmt, "dBegin", true, "From", "calendar");
                        base.addInput(ref oGrp0Elmt, "dEnd", true, "To", "calendar");
                        var oSel1 = base.addSelect1(ref oGrp0Elmt, "cCurrencySymbol", true, "Currency");
                        base.addOption(ref oSel1, "All", "");
                        base.addOption(ref oSel1, "GBP", "GBP");
                        base.addInput(ref oGrp0Elmt, "nOrderStatus", true, "nOrderStatus", "hidden");
                        if (this.myWeb.moConfig["Quote"] != "on")
                        {
                            oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderType", true, "Cart Type");
                            base.addOption(ref oSel1, "Order", "Order");
                            base.addOption(ref oSel1, "Quote", "Quote");
                        }

                        // Lets get the content types if we have more than 1

                        SqlDataReader oDR;
                        string cSQL = "SELECT tblContent.cContentSchemaName" + " FROM tblCartItem LEFT OUTER JOIN" + " tblContent ON tblCartItem.nItemId = tblContent.nContentKey" + " GROUP BY tblContent.cContentSchemaName" + " ORDER BY tblContent.cContentSchemaName";
                        oDR = this.myWeb.moDbHelper.getDataReader(cSQL);
                        if (oDR.VisibleFieldCount > 1)
                        {
                            oSel1 = base.addSelect1(ref oGrp0Elmt, "bSplit", true, "Group By Product Type");
                            base.addOption(ref oSel1, "Yes", "1");
                            base.addOption(ref oSel1, "No", "0");
                            oSel1 = base.addSelect1(ref oGrp0Elmt, "cProductType", true, "Select Product Type");
                            base.addOption(ref oSel1, "All", "");
                            base.addOptionsFromSqlDataReader(ref oSel1, ref oDR, "cContentSchemaName", "cContentSchemaName");
                        }

                        // Gets full list of products

                        cSQL = " SELECT tblContent.cContentName, tblContent.nContentKey" + " FROM tblCartItem LEFT OUTER JOIN" + " tblContent ON tblCartItem.nItemId = tblContent.nContentKey" + " GROUP BY tblContent.cContentName, tblContent.nContentKey" + " ORDER BY tblContent.cContentName";
                        oDR = this.myWeb.moDbHelper.getDataReader(cSQL);
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nProductId", true, "Single Product");
                        base.addOption(ref oSel1, "All", "0");
                        base.addOptionsFromSqlDataReader(ref oSel1, ref oDR, "cContentName", "nContentKey");
                        base.addSubmit(ref oGrp0Elmt, "Results", "See Results", "Results");
                        XmlElement argoBindParent = null;
                        base.addBind("dBegin", "Criteria/dBegin", "true()", oBindParent: ref argoBindParent);
                        XmlElement argoBindParent1 = null;
                        base.addBind("dEnd", "Criteria/dEnd", "true()", oBindParent: ref argoBindParent1);
                        XmlElement argoBindParent2 = null;
                        base.addBind("bSplit", "Criteria/bSplit", sType: "number", oBindParent: ref argoBindParent2);
                        XmlElement argoBindParent3 = null;
                        base.addBind("cProductType", "Criteria/cProductType", sType: "string", oBindParent: ref argoBindParent3);
                        XmlElement argoBindParent4 = null;
                        base.addBind("nProductId", "Criteria/nProductId", sType: "number", oBindParent: ref argoBindParent4);
                        XmlElement argoBindParent5 = null;
                        base.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", sType: "string", oBindParent: ref argoBindParent5);
                        XmlElement argoBindParent6 = null;
                        base.addBind("nOrderStatus", "Criteria/nOrderStatus", sType: "string", oBindParent: ref argoBindParent6);
                        XmlElement argoBindParent7 = null;
                        base.addBind("cOrderType", "Criteria/cOrderType", "true()", "string", oBindParent: ref argoBindParent7);
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmCartActivity", ex, "", "", Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCartActivityDrillDown()
                {
                    try
                    {
                        // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                        XmlElement oFrmElmt;
                        var oTempInstance = this.moPageXML.CreateElement("instance");
                        bool bCascade = false;
                        string cProcessInfo = "";
                        base.NewFrm("CartActivityDrilldown");
                        base.submission("SeeReport", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");
                        var oGrp0Elmt = base.addGroup(ref oFrmElmt, "Criteria", sLabel: "Criteria");
                        base.Instance.InnerXml = "<Criteria>" + "<nYear>" + DateAndTime.Now.Year + "</nYear>" + "<nMonth>" + DateAndTime.Now.Month + "</nMonth>" + "<nDay>0</nDay>" + "<cGrouping>Page</cGrouping><cCurrencySymbol/>" + "<nOrderStatus>6,9,17</nOrderStatus>" + "<cOrderType>Order</cOrderType>" + "</Criteria>";
                        // Year
                        var oSel1 = base.addSelect1(ref oGrp0Elmt, "nYear", true, "Year", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 2000, loopTo = DateAndTime.Now.Year; i <= loopTo; i++)
                            base.addOption(ref oSel1, i.ToString(), i.ToString());
                        // Month
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nMonth", true, "Month", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 1; i <= 12; i++)
                            base.addOption(ref oSel1, DateAndTime.MonthName(i), i.ToString());
                        // Day
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nDay", true, "Day", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 1; i <= 31; i++)
                            base.addOption(ref oSel1, i.ToString(), i.ToString());
                        // Grouping
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cGrouping", true, "Grouping", "required");
                        base.addOption(ref oSel1, "By Page", "Page");
                        base.addOption(ref oSel1, "By Group", "Group");
                        // Currency
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cCurrencySymbol", true, "Currency");
                        base.addOption(ref oSel1, "All/None", "");
                        base.addOption(ref oSel1, "GBP", "£");
                        // OrderStatus
                        base.addInput(ref oGrp0Elmt, "nOrderStatus", true, "nOrderStatus", "hidden");
                        // CartType
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderType", true, "Cart Type", "required");
                        base.addOption(ref oSel1, "Order", "Order");
                        base.addOption(ref oSel1, "Quote", "Quote");
                        base.addSubmit(ref oGrp0Elmt, "Results", "See Results", "Results");
                        XmlElement argoBindParent = null;
                        base.addBind("nYear", "Criteria/nYear", oBindParent: ref argoBindParent);
                        XmlElement argoBindParent1 = null;
                        base.addBind("nMonth", "Criteria/nMonth", oBindParent: ref argoBindParent1);
                        XmlElement argoBindParent2 = null;
                        base.addBind("nDay", "Criteria/nDay", oBindParent: ref argoBindParent2);
                        XmlElement argoBindParent3 = null;
                        base.addBind("cGrouping", "Criteria/cGrouping", sType: "string", oBindParent: ref argoBindParent3);
                        XmlElement argoBindParent4 = null;
                        base.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", sType: "string", oBindParent: ref argoBindParent4);
                        XmlElement argoBindParent5 = null;
                        base.addBind("nOrderStatus", "Criteria/nOrderStatus", sType: "string", oBindParent: ref argoBindParent5);
                        XmlElement argoBindParent6 = null;
                        base.addBind("cOrderType", "Criteria/cOrderType", "true()", "string", oBindParent: ref argoBindParent6);
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmCartActivityDrillDown", ex, "", "", Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCartActivityPeriod()
                {
                    try
                    {
                        // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                        XmlElement oFrmElmt;
                        var oTempInstance = this.moPageXML.CreateElement("instance");
                        bool bCascade = false;
                        string cProcessInfo = "";
                        base.NewFrm("CartActivityDrilldown");
                        base.submission("SeeReport", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");
                        var oGrp0Elmt = base.addGroup(ref oFrmElmt, "Criteria", sLabel: "Criteria");
                        base.Instance.InnerXml = "<Criteria>" + "<nYear>" + DateAndTime.Now.Year + "</nYear>" + "<nMonth>0</nMonth>" + "<nWeek>0</nWeek>" + "<cGroup>Month</cGroup><cCurrencySymbol/>" + "<nOrderStatus>6,9,17</nOrderStatus>" + "<cOrderType>Order</cOrderType>" + "</Criteria>";
                        // Year
                        var oSel1 = base.addSelect1(ref oGrp0Elmt, "nYear", true, "Year", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 2000, loopTo = DateAndTime.Now.Year; i <= loopTo; i++)
                            base.addOption(ref oSel1, i.ToString(), i.ToString());
                        // Month
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nMonth", true, "Month", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 1; i <= 12; i++)
                            base.addOption(ref oSel1, DateAndTime.MonthName(i), i.ToString());
                        // Day
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nWeek", true, "Week", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 1; i <= 52; i++)
                            base.addOption(ref oSel1, i.ToString(), i.ToString());
                        // Grouping
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cGroup", true, "Grouping", "required");
                        base.addOption(ref oSel1, "By Month", "Month");
                        base.addOption(ref oSel1, "By Week", "Week");
                        base.addOption(ref oSel1, "By Day", "Day");
                        // Currency
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cCurrencySymbol", true, "Currency");
                        base.addOption(ref oSel1, "All/None", "");
                        base.addOption(ref oSel1, "GBP", "£");
                        // OrderStatus
                        base.addInput(ref oGrp0Elmt, "nOrderStatus", true, "nOrderStatus", "hidden");
                        // CartType
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderType", true, "Cart Type", "required");
                        base.addOption(ref oSel1, "Order", "Order");
                        base.addOption(ref oSel1, "Quote", "Quote");
                        base.addSubmit(ref oGrp0Elmt, "Results", "See Results", "Results");
                        XmlElement argoBindParent = null;
                        base.addBind("nYear", "Criteria/nYear", oBindParent: ref argoBindParent);
                        XmlElement argoBindParent1 = null;
                        base.addBind("nMonth", "Criteria/nMonth", oBindParent: ref argoBindParent1);
                        XmlElement argoBindParent2 = null;
                        base.addBind("nWeek", "Criteria/nWeek", oBindParent: ref argoBindParent2);
                        XmlElement argoBindParent3 = null;
                        base.addBind("cGroup", "Criteria/cGroup", sType: "string", oBindParent: ref argoBindParent3);
                        XmlElement argoBindParent4 = null;
                        base.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", sType: "string", oBindParent: ref argoBindParent4);
                        XmlElement argoBindParent5 = null;
                        base.addBind("nOrderStatus", "Criteria/nOrderStatus", sType: "string", oBindParent: ref argoBindParent5);
                        XmlElement argoBindParent6 = null;
                        base.addBind("cOrderType", "Criteria/cOrderType", "true()", "string", oBindParent: ref argoBindParent6);
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmCartActivityPeriod", ex, "", "", Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmMemberVisits()
                {
                    try
                    {
                        XmlElement oFrmElmt;
                        // Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")


                        base.NewFrm("MemberVisits");
                        base.Instance.InnerXml = "<Criteria><dFrom>" + XmlDate(DateAndTime.Now.AddDays(-31), false) + "</dFrom><dTo>" + XmlDate(DateAndTime.Now.AddDays(1d), false) + "</dTo><cGroups>0</cGroups></Criteria>";
                        base.submission("SeeReport", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");
                        var oGrp0Elmt = base.addGroup(ref oFrmElmt, "Criteria", sLabel: "Search Visits");
                        base.addInput(ref oGrp0Elmt, "dFrom", true, "From", "calendar");
                        base.addInput(ref oGrp0Elmt, "dTo", true, "To", "calendar");
                        var oSel = base.addSelect(ref oGrp0Elmt, "cGroups", true, "Filter by Group", nAppearance: Protean.xForm.ApperanceTypes.Minimal);
                        base.addSubmit(ref oGrp0Elmt, "Results", "See Results", "Results");
                        string cSQL = "SELECT cDirName, nDirKey FROM tblDirectory ";
                        cSQL += " WHERE (NOT (cDirSchema = 'User')) AND (NOT (cDirSchema = N'Role'))";
                        cSQL += " ORDER BY cDirName";
                        var oDR = this.myWeb.moDbHelper.getDataReader(cSQL);
                        base.addOptionsFromSqlDataReader(ref oSel, ref oDR, "cDirName", "nDirKey");
                        XmlElement argoBindParent = null;
                        base.addBind("dFrom", "Criteria/dFrom", "true()", oBindParent: ref argoBindParent);
                        XmlElement argoBindParent1 = null;
                        base.addBind("dTo", "Criteria/dTo", "true()", oBindParent: ref argoBindParent1);
                        XmlElement argoBindParent2 = null;
                        base.addBind("cGroups", "Criteria/cGroups", oBindParent: ref argoBindParent2);
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmMemberVisits", ex, "", "", Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                /// <summary>
            /// This adds or edits a codeset, or the groups associated with the code set
            /// </summary>
            /// <param name="nCodesetKey">The code set ID</param>
            /// <param name="cFormName">The type of form we're dealing with - Codes or CodeGroups</param>
            /// <returns></returns>
            /// <remarks></remarks>
                public XmlElement xFrmMemberCodeset(int nCodesetKey, string cFormName = "Codes")
                {
                    try
                    {
                        XmlElement oElmt = null;
                        string cCodeGroups = "";
                        string cSQL = "";

                        // Build the form
                        base.NewFrm("MemberCodes");
                        base.load("/xforms/directory/" + cFormName + ".xml", this.myWeb.maCommonFolders);

                        // Load the instance.
                        if (nCodesetKey > 0)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Codes, nCodesetKey);
                        }

                        // Pre-population
                        // ==============

                        // Code Type
                        base.Instance.SelectSingleNode("tblCodes/nCodeType").InnerText = ((int)Cms.dbHelper.CodeType.Membership).ToString();

                        // Groups
                        if (Xml.NodeState(ref base.moXformElmt, "//select[@bind='cCodeGroups']") != XmlNodeState.NotInstantiated)
                        {
                            oElmt = (XmlElement)base.moXformElmt.SelectSingleNode("//select[@bind='cCodeGroups']");
                            cSQL = "SELECT cDirName + ' [' + cDirSchema + ': ' + CAST(nDirKey As nvarchar) + ']' AS DirName, nDirKey FROM tblDirectory ";
                            cSQL += " WHERE NOT(cDirSchema IN ('Role','User'))";
                            cSQL += " ORDER BY cDirSchema, cDirName";
                            var oDR = this.myWeb.moDbHelper.getDataReader(cSQL);
                            base.addOptionsFromSqlDataReader(ref oElmt, ref oDR, "DirName", "nDirKey");
                        }

                        // Handle Submission
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                // Create a unique ID for new code sets
                                if (nCodesetKey == 0)
                                {
                                    base.Instance.SelectSingleNode("tblCodes/cCode").InnerText = Guid.NewGuid().ToString();
                                }

                                // Save the code set
                                this.myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Codes, base.Instance, nCodesetKey);

                                // Update any sub-codes
                                if (nCodesetKey > 0)
                                {
                                    cSQL = "UPDATE tblAudit SET dPublishDate = " + Database.SqlDate(base.Instance.SelectSingleNode("tblCodes/dPublishDate").InnerText, true);
                                    cSQL += ", dExpireDate  = " + Database.SqlDate(base.Instance.SelectSingleNode("tblCodes/dExpireDate").InnerText, true);
                                    cSQL += ", nStatus  = " + base.Instance.SelectSingleNode("tblCodes/nStatus").InnerText;
                                    cSQL += " FROM tblAudit a INNER JOIN tblCodes c ON a.nauditkey = c.nauditid AND (c.nCodeParentId = " + nCodesetKey + " OR c.nCodeKey = " + nCodesetKey + ")";
                                    this.myWeb.moDbHelper.ExeProcessSql(cSQL);
                                }
                            }
                        }

                        // Manually populate the groups in the dropdown
                        XmlNodeState localNodeState() { var argoNode = base.Instance; var ret = Xml.NodeState(ref argoNode, "tblCodes/cCodeGroups", returnAsText: cCodeGroups); base.Instance = argoNode; return ret; }

                        if (localNodeState() == XmlNodeState.HasContents)
                        {
                            var oGroups = Strings.Split(cCodeGroups, ",");
                            for (int i = 0, loopTo = oGroups.Length - 1; i <= loopTo; i++)
                            {
                                if (Xml.NodeState(ref base.moXformElmt, "descendant-or-self::*[@bind='cCodeGroups']/item[value='" + oGroups[i] + "']") != XmlNodeState.NotInstantiated)
                                {
                                    oElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::*[@bind='cCodeGroups']/item[value='" + oGroups[i] + "']");
                                    oElmt.SetAttribute("selected", "selected");
                                }
                            }
                        }

                        // Tidy Up
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmMemberCodeset", ex, "", "", Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                /// <summary>
            ///   Xform for generating codes for codes sets.
            /// </summary>
            /// <param name="nParentCodeKey">The parent code set id</param>
            /// <param name="cFormName">Optional, probably not needed - the type of form we're dealing with - CodeGenerator</param>
            /// <returns></returns>
            /// <remarks></remarks>
                public XmlElement xFrmMemberCodeGenerator(int nParentCodeKey, string cFormName = "CodeGenerator")
                {
                    XmlElement oElmt = null;
                    XmlElement oParentInstance = null;
                    XmlElement oInstanceRoot = null;
                    string cCodeGroups = "";
                    string cCodeXForm = "";
                    try
                    {

                        // Build the form
                        base.NewFrm("MemberCodes");
                        base.load("/xforms/directory/" + cFormName + ".xml", this.myWeb.maCommonFolders);
                        base.Instance.SelectSingleNode("tblCodes/nCodeType").InnerText = ((int)Cms.dbHelper.CodeType.Membership).ToString();

                        // Get the parent code instance
                        oParentInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                        oParentInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Codes, nParentCodeKey);

                        // Update the form with relevant values from the parent
                        foreach (XmlElement currentOElmt in oParentInstance.SelectNodes("tblCodes/dPublishDate|tblCodes/dExpireDate|tblCodes/nStatus"))
                        {
                            oElmt = currentOElmt;
                            // Populate the local instance with parent code information
                            if (base.Instance.SelectSingleNode("tblCodes/" + oElmt.Name) is object)
                            {
                                base.Instance.SelectSingleNode("tblCodes/" + oElmt.Name).InnerText = oElmt.InnerText;
                            }
                        }

                        // Add the parent code id
                        base.Instance.SelectSingleNode("tblCodes/nCodeParentId").InnerText = nParentCodeKey.ToString();

                        // Update the label
                        if (Xml.NodeState(ref base.moXformElmt, "group/label", returnElement: oElmt) != XmlNodeState.NotInstantiated)
                        {
                            oElmt.InnerText += " for " + oParentInstance.SelectSingleNode("tblCodes/cCodeName").InnerText;
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                // Generate the Codes
                                oInstanceRoot = (XmlElement)base.Instance.SelectSingleNode("tblCodes");
                                int nNoCodes = Conversions.ToInteger(oInstanceRoot.SelectSingleNode("nNumberOfCodes").InnerText);
                                var oCodes = new string[nNoCodes];
                                XmlNode argoParent1 = oInstanceRoot;
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(getNodeValueByType(ref argoParent1, "bRND", vDefaultValue: "0"), "0", false)))
                                {

                                    // Generate non-random codes
                                    oCodes = CodeGen(oInstanceRoot.SelectSingleNode("cPreceedingText").InnerText, Conversions.ToInteger(oInstanceRoot.SelectSingleNode("nStartNumber").InnerText), nNoCodes, Conversions.ToBoolean(oInstanceRoot.SelectSingleNode("bKeepProceedingZeros").InnerText), Conversions.ToBoolean(oInstanceRoot.SelectSingleNode("bMD5Results").InnerText));
                                }
                                else
                                {

                                    // Generate random codes
                                    var random = new Number.Random();

                                    // Set the options

                                    // Option: Case
                                    var options = TextOptions.UpperCase;

                                    // Option: Unambiguous Letters
                                    XmlNode argoParent = oInstanceRoot;
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(getNodeValueByType(ref argoParent, "bRNDVague", vDefaultValue: "0"), "1", false)))
                                        options = options | TextOptions.UnambiguousCharacters;

                                    // Option: Character Sets
                                    object localgetNodeValueByType() { XmlNode argoParent = oInstanceRoot; var ret = getNodeValueByType(ref argoParent, "cRNDAlpha", vDefaultValue: "Letters,Numbers"); return ret; }

                                    string cCharDefs = localgetNodeValueByType().ToString();
                                    if (cCharDefs.Contains("Letters"))
                                        options = options | TextOptions.UseAlpha;
                                    if (cCharDefs.Contains("Numbers"))
                                        options = options | TextOptions.UseNumeric;
                                    if (cCharDefs.Contains("Symbols"))
                                        options = options | TextOptions.UseSymbols;

                                    // Generate the codes
                                    for (int i = 0, loopTo = nNoCodes - 1; i <= loopTo; i++)
                                    {
                                        // Generate a random password
                                        object localgetNodeValueByType1() { XmlNode argoParent = oInstanceRoot; var ret = getNodeValueByType(ref argoParent, "nRNDLength", vDefaultValue: "8"); return ret; }

                                        object localgetNodeValueByType2() { XmlNode argoParent = oInstanceRoot; var ret = getNodeValueByType(ref argoParent, "nRNDLength", vDefaultValue: "8"); return ret; }

                                        string cC = RandomPassword(Conversions.ToInteger(localgetNodeValueByType2()), options: options, oRandomObject: random);

                                        // Check for duplicates
                                        while (!(Array.LastIndexOf(oCodes, cC) == -1 | string.IsNullOrEmpty(cC)))
                                        {
                                            object localgetNodeValueByType3() { XmlNode argoParent = oInstanceRoot; var ret = getNodeValueByType(ref argoParent, "nRNDLength", vDefaultValue: "8"); return ret; }

                                            object localgetNodeValueByType4() { XmlNode argoParent = oInstanceRoot; var ret = getNodeValueByType(ref argoParent, "nRNDLength", vDefaultValue: "8"); return ret; }

                                            cC = RandomPassword(Conversions.ToInteger(localgetNodeValueByType4()), options: options);
                                        }

                                        oCodes[i] = cC;
                                    }
                                }

                                // Add the codes to the database
                                int nAdded = 0;
                                int nSkipped = 0;
                                for (int i = 0, loopTo1 = oCodes.Length - 1; i <= loopTo1; i++)
                                {
                                    if (!string.IsNullOrEmpty(oCodes[i]))
                                    {
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(this.myWeb.moDbHelper.GetDataValue("SELECT nCodeKey FROM tblCodes WHERE cCode ='" + oCodes[i] + "'", nullreturnvalue: (object)0), 0, false)))
                                        {
                                            nSkipped += 1;
                                        }
                                        else
                                        {
                                            base.Instance.SelectSingleNode("tblCodes/cCode").InnerText = oCodes[i];
                                            int nSubId = Conversions.ToInteger(this.myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Codes, base.Instance));
                                            nAdded += 1;
                                        }
                                    }
                                }

                                var argoNode = base.moXformElmt.SelectSingleNode("group");
                                base.addNote(ref argoNode, Protean.xForm.noteTypes.Help, nAdded + " Codes Added, " + nSkipped + " Codes Skipped (Duplicates)", true);
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmMemberCodeGenerator", ex, "", "", Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmVoucherCode(int nCodeId)
                {
                    string cProcessInfo = "";
                    string cTypePath = "";
                    try
                    {
                        base.NewFrm("EditVoucherCode");
                        if (!base.load("/xforms/codes/" + cTypePath, this.myWeb.maCommonFolders))
                        {
                            // not allot we can do really except try defaults
                            if (!base.load("/xforms/code/Voucher.xml", this.myWeb.maCommonFolders))
                            {
                                // not allot we can do really 
                            }
                        }

                        if (nCodeId > 0)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Codes, nCodeId);
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Codes, base.Instance, Conversions.ToLong(Interaction.IIf(nCodeId > 0, nCodeId, -1)));
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmVoucherCode", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmImportFile(string cPath)
                {
                    XmlElement oFrmElmt;
                    string sValidResponse = "";
                    string cProcessInfo = "";
                    var oImportManifestXml = new XmlDocument();
                    try
                    {
                        try
                        {
                            oImportManifestXml.Load(this.goServer.MapPath(this.myWeb.moConfig["ProjectPath"] + "/xsl/import") + "/ImportManifest.xml");
                        }
                        catch
                        {
                            // do nothing
                        }

                        if (oImportManifestXml is object)
                        {
                            base.NewFrm("ImportFile");
                            base.submission("Inport File", "", "post", "form_check(this)");
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Import file into ProteanCMS", "", "Please select the file to import");
                            XmlElement oSelectElmt;
                            oSelectElmt = base.addSelect1(ref oFrmElmt, "importXslt", true, "Import Type");
                            string sDefaultXslt = "";
                            foreach (XmlElement oChoices in oImportManifestXml.SelectNodes("/Imports/ImportGroup/Import"))
                            {
                                // Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelectElmt, oChoices.GetAttribute("name"))
                                // For Each oItem In oChoices.SelectNodes("Import")
                                base.addOption(ref oSelectElmt, Strings.Replace(oChoices.GetAttribute("name"), "_", " "), oChoices.GetAttribute("xslFile"));
                                if (string.IsNullOrEmpty(sDefaultXslt))
                                    sDefaultXslt = oChoices.GetAttribute("xslFile");
                                // Next
                            }

                            base.addNote("importXslt", Protean.xForm.noteTypes.Hint, "This defines the layout and columns of the import file. Each import file must be in the pre-agreed format. To create additional import filters contact your web developer.");
                            XmlElement argoBindParent = null;
                            base.addBind("importXslt", "file/@importXslt", "true()", oBindParent: ref argoBindParent);
                            base.addInput(ref oFrmElmt, "fld", true, "Upload Path", "readonly");
                            XmlElement argoBindParent1 = null;
                            base.addBind("fld", "file/@path", "true()", oBindParent: ref argoBindParent1);
                            string argsClass = "";
                            base.addUpload(ref oFrmElmt, "uploadFile", true, "image/*", "Upload File", sClass: ref argsClass);
                            XmlElement argoBindParent2 = null;
                            base.addBind("uploadFile", "file", "true()", oBindParent: ref argoBindParent2);
                            XmlElement oSelectElmt2;
                            oSelectElmt2 = base.addSelect1(ref oFrmElmt, "opperationMode", true, "Opperation Mode", nAppearance: Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelectElmt2, "Test", "test");
                            base.addOption(ref oSelectElmt2, "Full Import", "import");
                            XmlElement argoBindParent3 = null;
                            base.addBind("opperationMode", "file/@opsMode", "true()", oBindParent: ref argoBindParent3);

                            // If myWeb.moConfig("debug") = "on" Then
                            XmlElement oSelectElmt3;
                            oSelectElmt3 = base.addSelect1(ref oFrmElmt, "contentType", true, "Response Xml", nAppearance: Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelectElmt3, "on", "xml");
                            base.addOption(ref oSelectElmt3, "off", "");
                            // End If
                            XmlElement argoBindParent4 = null;
                            base.addBind("xml", "file/@xml", "false()", oBindParent: ref argoBindParent4);
                            base.addSubmit(ref oFrmElmt, "", "Upload", "ewSubmit");
                            base.Instance.InnerXml = "<file path=\"" + cPath + "\" filename=\"\" mediatype=\"\" opsMode=\"test\" importXslt=\"" + sDefaultXslt + "\" xml=\"\"/>";
                            if (base.isSubmitted())
                            {
                                base.updateInstanceFromRequest();
                                base.validate();

                                // lets do some hacking 
                                System.Web.HttpPostedFile fUpld;
                                fUpld = this.goRequest.Files["uploadFile"];
                                if (fUpld is object)
                                {
                                    base.valid = true;
                                }

                                if (base.valid)
                                {
                                    var oFs = new Protean.fsHelper();
                                    oFs.initialiseVariables(Protean.fsHelper.LibraryType.Documents);
                                    oFs.mcStartFolder = this.goServer.MapPath("/") + cPath;
                                    sValidResponse = oFs.SaveFile(ref fUpld, "");
                                    XmlElement oElmt = (XmlElement)base.Instance.FirstChild;
                                    string cFilename = oFs.mcStartFolder + Strings.Right(fUpld.FileName, Strings.Len(fUpld.FileName) - fUpld.FileName.LastIndexOf(@"\"));
                                    cFilename = cFilename.Replace(" ", "-");
                                    oElmt.SetAttribute("filename", cFilename);
                                    if ((sValidResponse ?? "") == (fUpld.FileName ?? ""))
                                    {
                                        this.valid = true;
                                        XmlNode argoNode = (XmlNode)this.moXformElmt;
                                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, sValidResponse + " - File Imported");
                                    }
                                    else
                                    {
                                        this.valid = false;
                                        XmlNode argoNode1 = (XmlNode)this.moXformElmt;
                                        base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, sValidResponse);
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
                        else
                        {
                            base.NewFrm("ImportFile");
                            base.submission("Import File Error", "", "post", "form_check(this)");
                            oFrmElmt = base.addGroup(ref base.moXformElmt, "Import File", "", "Error");
                            XmlNode argoNode2 = oFrmElmt;
                            base.addNote(ref argoNode2, Protean.xForm.noteTypes.Alert, "There are no imports configured for this site.");
                            return base.moXformElmt;
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmStartIndex()
                {
                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    try
                    {

                        // load the xform to be edited
                        moDbHelper.moPageXml = this.moPageXML;
                        var idx = new Protean.Indexer(ref this.myWeb);
                        base.NewFrm("StartIndex");
                        base.submission("DeleteFile", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Start Index");
                        base.Instance.InnerXml = idx.GetIndexInfo();
                        XmlNode argoNode = oFrmElmt;
                        base.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Starting off the indexing process can take up to an hour for larger sites");
                        base.addSubmit(ref oFrmElmt, "", "Start Index", sClass: "principle pleaseWait");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                bool bResult = true;
                                idx.DoIndex(0, ref bResult);
                                string cSubResponse = idx.cExError;
                                if (string.IsNullOrEmpty(cSubResponse))
                                {
                                    bResult = true;
                                    cSubResponse = "Completed Successfully";
                                }
                                else
                                {
                                    bResult = false;
                                }

                                cSubResponse += Constants.vbCrLf + "Pages: " + idx.nPagesIndexed;
                                cSubResponse += Constants.vbCrLf + "Documents: " + idx.nDocumentsIndexed;
                                cSubResponse += Constants.vbCrLf + "Contents: " + idx.nContentsIndexed;
                                XmlNode argoNode1 = oFrmElmt;
                                base.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, cSubResponse);
                            }

                            // fire this off in its own thread.
                            // Dim t As Thread
                            // t = New Thread(AddressOf idx.DoIndex)
                            // t.Start()

                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmStartIndex", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmGetReport(string cReportName)
                {
                    string cProcessInfo = "";
                    try
                    {

                        // Replace Spaces with hypens
                        cReportName = Strings.Replace(cReportName, " ", "-");
                        if (!base.load("/xforms/Reports/" + cReportName + ".xml", this.myWeb.maCommonFolders))
                        {
                            // show xform load error message
                        }

                        XmlElement queryNode = (XmlElement)this.moXformElmt.SelectSingleNode("descendant-or-self::Query");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }
                        else if (Strings.LCase(queryNode.GetAttribute("autoSubmit")) == "true")
                        {
                            base.validate();
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmGetReport", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmLookup(int nLookupId, string Category = "", long ParentId = 0L)
                {
                    XmlElement oFrmElmt;
                    XmlElement oGrp1Elmt;
                    string cProcessInfo = "";
                    try
                    {
                        string parentOptions = "" + this.myWeb.moConfig["LookupParentOptions"];
                        var oDict = new Dictionary<string, string>();
                        if (!string.IsNullOrEmpty(parentOptions))
                        {
                            foreach (var s in Strings.Split(parentOptions, ";"))
                            {
                                var arr = Strings.Split(s, ":");
                                oDict.Add(arr[0], arr[1]);
                            }
                        }

                        base.NewFrm("EditProductGroup");
                        base.Instance.InnerXml = "<tblLookup><nLkpID/><cLkpKey/><cLkpValue/><cLkpCategory>" + Category + "</cLkpCategory><nLkpParent>" + ParentId + "</nLkpParent><nAuditId/></tblLookup>";
                        if (nLookupId > 0)
                        {
                            // MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Lookup, nLookupId)
                            Category = base.Instance.SelectSingleNode("tblLookup/cLkpCategory").InnerText;
                        }

                        base.submission("EditLookup", "", "post", "form_check(this)");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Lookup");
                        base.addNote("pgheader", Protean.xForm.noteTypes.Help, Conversions.ToString(Operators.ConcatenateObject(Interaction.IIf(nLookupId > 0, "Edit ", "Add "), "Lookup")));
                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Lookup", "1col", "Details");

                        // Definitions
                        base.addInput(ref oGrp1Elmt, "nLkpID", true, "nLkpID", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nLkpID", "tblLookup/nLkpID", oBindParent: ref argoBindParent);
                        if (!string.IsNullOrEmpty(parentOptions))
                        {
                            if (oDict.ContainsKey(Category))
                            {
                                var SelectElmt = base.addSelect1(ref oGrp1Elmt, "nLkpParent", true, oDict[Category], ((int)Protean.xForm.ApperanceTypes.Minimal).ToString());
                                XmlElement argoBindParent1 = null;
                                base.addBind("nLkpParent", "tblLookup/nLkpParent", oBindParent: ref argoBindParent1);
                                string sSql = "select nLkpId as value, cLkpKey as name from tblLookup where cLkpCategory like '" + oDict[Category] + "'";
                                var oDr = this.myWeb.moDbHelper.getDataReader(sSql);
                                base.addOptionsFromSqlDataReader(ref SelectElmt, ref oDr);
                            }
                        }

                        base.addInput(ref oGrp1Elmt, "cLkpKey", true, "Name");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cLkpKey", "tblLookup/cLkpKey", "true()", oBindParent: ref argoBindParent2);
                        base.addInput(ref oGrp1Elmt, "cLkpValue", true, "Value");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cLkpValue", "tblLookup/cLkpValue", "true()", oBindParent: ref argoBindParent3);
                        base.addInput(ref oGrp1Elmt, "cLkpCategory", true, "Category", "readonly");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cLkpCategory", "tblLookup/cLkpCategory", "true()", oBindParent: ref argoBindParent4);
                        base.addInput(ref oGrp1Elmt, "nAuditId", true, "nAuditId", "hidden");
                        XmlElement argoBindParent5 = null;
                        base.addBind("nAuditId", "tblLookup/nAuditId", oBindParent: ref argoBindParent5);

                        // search button

                        base.addSubmit(ref oFrmElmt, "EditLookup", "Save Lookup", "SaveLookup");
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Lookup, base.Instance, Conversions.ToLong(Interaction.IIf(nLookupId > 0, nLookupId, -1)));
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmLookup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmEditTemplate()
                {
                    string cProcessInfo = "";
                    string xslFilename = "";
                    XmlElement oFrmElmt;
                    try
                    {
                        base.NewFrm("EditTemplate");
                        switch (this.myWeb.moRequest["ewCmd2"] ?? "")
                        {
                            case "RenewalAlerts":
                                {
                                    xslFilename = "/xsl/email/subscriptionReminder.xsl";
                                    break;
                                }
                        }

                        base.Instance.InnerXml = "<Template name=\"\"><TemplateContent/></Template>";
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "SelectTemplate");
                        var XslDocument = new XmlDocument();
                        XslDocument.Load(this.goServer.MapPath(xslFilename));
                        base.submission("EditTemplate", "", "post", "form_check(this)");
                        object xmlnsManager = new XmlNamespaceManager(XslDocument.NameTable);
                        xmlnsManager.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform");
                        var SelectElmt = base.addSelect1(ref oFrmElmt, "Template", true, "Template", ((int)Protean.xForm.ApperanceTypes.Minimal).ToString());
                        short i = 1;
                        foreach (XmlElement oTmpt in XslDocument.DocumentElement.SelectNodes("xsl:template", (XmlNamespaceManager)xmlnsManager))
                        {
                            this.addOption(ref SelectElmt, oTmpt.GetAttribute("mode") + " - " + oTmpt.GetAttribute("match"), i.ToString());
                            if (Conversions.ToInteger(this.myWeb.moRequest["Template"]) == i)
                            {
                                this.addInput(ref oFrmElmt, "tplt-mode", true, "Mode");
                                XmlElement argoBindParent = null;
                                base.addBind("tplt-mode", "Template/TemplateContent/*/@mode", "true()", oBindParent: ref argoBindParent);
                                this.addInput(ref oFrmElmt, "tplt-match", true, "Match");
                                XmlElement argoBindParent1 = null;
                                base.addBind("tplt-match", "Template/TemplateContent/*/@match", "true()", oBindParent: ref argoBindParent1);
                                string argsClass = "xsl";
                                int argnRows = 0;
                                int argnCols = 0;
                                base.addTextArea(ref oFrmElmt, "TemplateContent", true, "Template Content", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                                XmlElement argoBindParent2 = null;
                                base.addBind("TemplateContent", "Template/TemplateContent", "true()", oBindParent: ref argoBindParent2);
                                XmlElement oElmt = (XmlElement)base.Instance.SelectSingleNode("Template/TemplateContent");
                                oElmt.InnerXml = oTmpt.OuterXml;
                            }

                            i = (short)(i + 1);
                        }

                        var xmlElmt = base.addSelect1(ref oFrmElmt, "contentType", true, "contentType", ((int)Protean.xForm.ApperanceTypes.Minimal).ToString());
                        this.addOption(ref xmlElmt, "xml", "xml");
                        if (Conversions.ToInteger(this.myWeb.moRequest["Template"]) > 0)
                        {
                            base.addSubmit(ref oFrmElmt, "EditTemplate", "Save Template", "SaveTemplate");
                        }
                        else
                        {
                            base.addSubmit(ref oFrmElmt, "EditTemplate", "Edit Template", "EditTemplate");
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmLookup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                /// <summary>
            /// 
            /// </summary>
            /// <remarks></remarks>
                private class xFormContentLocations
                {

                    #region  Declarations
                    private new string mcModuleName = "xFormContentLocations";


                    // Declarations
                    private AdminXforms _form;
                    private XmlElement _locations;
                    private int _locationCount = 1;
                    private long _contentId;
                    private XmlElement _structureXml = null;
                    private Hashtable _currentLocations;
                    private Hashtable _locationsScope;
                    private XmlNodeList _selects = null;

                    // Constants
                    private const string _selectsXPath = "//node()[contains(name(),'select') and contains(@class,'contentLocations')]";
                    private const string _locationInstanceNodeName = "locatomatic";
                    #endregion
                    #region  Initialisation
                    public xFormContentLocations(long ContentId, ref Cms.xForm Form)
                    {
                        Protean.stdTools.PerfMon.Log(mcModuleName, "New");
                        try
                        {
                            // Set variables
                            _contentId = ContentId;
                            _form = (AdminXforms)Form;
                            _selects = _form.RootGroup.SelectNodes(_selectsXPath);
                        }
                        catch (Exception ex)
                        {
                            Protean.stdTools.returnException(ref Form.myWeb.msException, mcModuleName, "New", ex, "", "", Protean.stdTools.gbDebug);
                        }
                    }
                    #endregion
                    #region  Public Methods
                    public void Refresh()
                    {
                        Protean.stdTools.PerfMon.Log(mcModuleName, "Refresh");
                        try
                        {
                            _selects = _form.RootGroup.SelectNodes(_selectsXPath);
                        }
                        catch (Exception ex)
                        {
                            Protean.stdTools.returnException(ref _form.myWeb.msException, mcModuleName, "Refresh", ex, "", "", Protean.stdTools.gbDebug);
                        }
                    }

                    public bool IsActive()
                    {
                        Protean.stdTools.PerfMon.Log(mcModuleName, "IsActive");
                        try
                        {
                            return _selects.Count > 0;
                        }
                        catch (Exception ex)
                        {
                            Protean.stdTools.returnException(ref _form.myWeb.msException, mcModuleName, "IsActive", ex, "", "", Protean.stdTools.gbDebug);
                            return false;
                        }
                    }

                    public void ProcessSelects()
                    {
                        Protean.stdTools.PerfMon.Log(mcModuleName, "ProcessSelects");
                        long menuId;
                        XmlElement bind;
                        XmlElement locations;
                        XmlNodeList menuItems;
                        xFormContentLocationsSelect selectItem;
                        string value;
                        XmlElement location;
                        string locationid;
                        string cXPath;
                        string cXPathModifier;
                        string menuName = "";
                        try
                        {
                            if (IsActive())
                            {

                                // Get the site structure
                                _structureXml = _form.myWeb.GetStructureXML();

                                // Get the current locations
                                _currentLocations = new Hashtable();
                                _locationsScope = new Hashtable();
                                if (_contentId > 0L)
                                {
                                    string argsValueField = "Location";
                                    _currentLocations = _form.moDbHelper.getHashTable("SELECT nStructId,1 As Location FROM tblContentLocation WHERE nContentId=" + _contentId, "nStructId", ref argsValueField);
                                }

                                // Create a bind element
                                XmlNode argoNode = (XmlNode)_form.model;
                                bind = addNewTextNode("bind", ref argoNode);
                                bind.SetAttribute("nodeset", _locationInstanceNodeName);
                                var argoNode1 = _form.moXformElmt.SelectSingleNode("//instance");
                                locations = Xml.addNewTextNode(_locationInstanceNodeName, ref argoNode1);


                                // Iterate through each select
                                foreach (XmlElement _selectItem in _selects)
                                {

                                    // Get the rootid - look for class root-id
                                    selectItem = new xFormContentLocationsSelect(ref _selectItem);

                                    // Get the menuItems - construct an xpath
                                    // The rootmode is the path modifier
                                    cXPath = "//MenuItem[@id=" + selectItem.Root.ToString() + " and ((not(@cloneparent) or @cloneparent=0) and (not(@clone) or @clone=0))]";
                                    if (!string.IsNullOrEmpty(_selectItem.GetAttribute("locationsXpath")))
                                    {
                                        cXPath = cXPath + _selectItem.GetAttribute("locationsXpath");
                                    }
                                    else
                                    {
                                        cXPathModifier = "";
                                        switch (selectItem.RootMode)
                                        {
                                            case xFormContentLocationsSelect.RootModes.Exclude:
                                                {
                                                    cXPath = cXPath + "/descendant::MenuItem";
                                                    break;
                                                }

                                            case xFormContentLocationsSelect.RootModes.Include:
                                                {
                                                    cXPath = cXPath + "/descendant-or-self::MenuItem";
                                                    break;
                                                }

                                            case xFormContentLocationsSelect.RootModes.ChildrenOnly:
                                                {
                                                    cXPath = cXPath + "/MenuItem";
                                                    break;
                                                }
                                        }
                                    }

                                    menuItems = _structureXml.SelectNodes(cXPath);

                                    // Add the instance node - assume that there could be more then one select here.
                                    XmlNode argoNode2 = locations;
                                    location = addNewTextNode("location", ref argoNode2);
                                    locationid = "loc_idx_" + _locationCount.ToString();
                                    _locationCount = _locationCount + 1;
                                    location.SetAttribute("id", locationid);


                                    // location the bind
                                    _form.addBind(selectItem.Id, "location[@id='" + locationid + "']", oBindParent: ref bind);
                                    var proceedingParent = default(XmlElement);
                                    var oChoices = default(XmlElement);
                                    // Process the menu items
                                    // For each menuitem, check if it's already in scope.
                                    // If not add the option to the select.
                                    foreach (XmlElement menuItem in menuItems)
                                    {
                                        menuId = Conversions.ToLong(menuItem.GetAttribute("id"));

                                        // Check if we've added it already
                                        if (!_locationsScope.ContainsKey(menuId))
                                        {
                                            if (_currentLocations.ContainsKey(menuId.ToString()))
                                            {
                                                value = "true";
                                                if (!string.IsNullOrEmpty(location.InnerText))
                                                    location.InnerText += ",";
                                                location.InnerText += menuId.ToString();
                                            }
                                            else
                                            {
                                                value = "false";
                                            }

                                            // Add to in-scope location hashtable
                                            _locationsScope.Add(menuId, value);

                                            // Determine the name - NodeState effectively sets menuName as
                                            // the DisplayName node if it's populated, if not is sets it to be the
                                            // name attribute.

                                            if (NodeState(ref menuItem, "DisplayName", returnAsText: menuName, bCheckTrimmedInnerText: true) != XmlNodeState.HasContents)
                                            {
                                                menuName = menuItem.GetAttribute("name");
                                            }
                                            else
                                            {
                                                menuName = menuItem.SelectSingleNode("DisplayName").InnerText;
                                            }

                                            XmlElement oParentNode = (XmlElement)menuItem.ParentNode;
                                            XmlElement oParentParentNode = (XmlElement)oParentNode.ParentNode;

                                            // _form.addOption(_selectItem, menuName, menuId)

                                            // if we are only 2 levels from the root then we use choices
                                            if (oParentParentNode is object)
                                            {
                                                if ((oParentParentNode.GetAttribute("id") ?? "") == (selectItem.Root.ToString() ?? "") & Strings.LCase(_selectItem.GetAttribute("showAllLevels")) != "true")
                                                {
                                                    if (proceedingParent is null)
                                                    {
                                                        oChoices = _form.addChoices(ref _selectItem, oParentNode.GetAttribute("name"));
                                                    }
                                                    else if ((proceedingParent.GetAttribute("id") ?? "") != (oParentNode.GetAttribute("id") ?? ""))
                                                    {
                                                        oChoices = _form.addChoices(ref _selectItem, oParentNode.GetAttribute("name"));
                                                    }
                                                    // Add the checkbox
                                                    _form.addOption(ref oChoices, menuName, menuId.ToString());
                                                }
                                                else if ((oParentNode.GetAttribute("id") ?? "") != (_form.myWeb.moConfig["RootPageId"] ?? ""))
                                                {
                                                    while ((oParentNode.GetAttribute("id") ?? "") != (selectItem.Root.ToString() ?? ""))
                                                    {
                                                        menuName = oParentNode.GetAttribute("name") + " / " + menuName;
                                                        oParentNode = (XmlElement)oParentNode.ParentNode;
                                                    }
                                                }
                                                // Add the checkbox
                                                _form.addOption(ref _selectItem, menuName, menuId.ToString());
                                            }
                                            else
                                            {
                                                if ((menuItem.GetAttribute("id") ?? "") != (_form.myWeb.moConfig["RootPageId"] ?? ""))
                                                {
                                                    while ((menuItem.GetAttribute("id") ?? "") != (selectItem.Root.ToString() ?? ""))
                                                    {
                                                        menuName = menuItem.GetAttribute("name") + " / " + menuName;
                                                        oParentNode = (XmlElement)menuItem.ParentNode;
                                                    }
                                                }

                                                // Add the checkbox
                                                _form.addOption(ref _selectItem, menuName, menuId.ToString());
                                            }

                                            proceedingParent = oParentNode;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Protean.stdTools.returnException(ref _form.myWeb.msException, mcModuleName, "ProcessSelects", ex, "", "", Protean.stdTools.gbDebug);
                        }
                    }

                    public void ProcessRequest(long ContentId)
                    {
                        Protean.stdTools.PerfMon.Log(mcModuleName, "ProcessRequest");
                        string InclusionList = "";
                        string ScopeList = "";
                        try
                        {
                            if (IsActive())
                            {

                                // Go through the location binds and deal with them.
                                // _locationInstanceNodeName

                                foreach (XmlElement location in _form.moXformElmt.SelectNodes("//instance/" + _locationInstanceNodeName + "/location"))
                                {

                                    // The inner text will be a comma separated list, we need to add this to the inclusion list.
                                    if (!string.IsNullOrEmpty(location.InnerText))
                                    {
                                        if (!string.IsNullOrEmpty(InclusionList))
                                        {
                                            InclusionList += ",";
                                        }

                                        InclusionList += location.InnerText;
                                    }
                                }

                                // Convert the Scope to a CSV
                                ScopeList = Dictionary.hashtableToCSV(ref _locationsScope, Dictionary.Dimension.Key);

                                // manage the locations
                                _form.moDbHelper.updateLocationsWithScope(ContentId, InclusionList, ScopeList);
                            }
                        }
                        catch (Exception ex)
                        {
                            Protean.stdTools.returnException(ref _form.myWeb.msException, mcModuleName, "ProcessRequest", ex, "", "", Protean.stdTools.gbDebug);
                        }
                    }


                    #endregion
                    #region  Private Class: xFormContentLocationsSelect
                    private class xFormContentLocationsSelect
                    {

                        #region  Declarations
                        private new string mcModuleName = "xFormContentLocationsSelect";
                        private XmlElement _selectItem;
                        private long _rootId;
                        private RootModes _rootMode;

                        public enum RootModes
                        {
                            Include,
                            Exclude,
                            RootOnly,
                            ChildrenOnly
                        }
                        #endregion
                        #region  Initialisation
                        public xFormContentLocationsSelect(ref XmlElement selectItem)
                        {
                            Protean.stdTools.PerfMon.Log(mcModuleName, "New");
                            try
                            {
                                _rootMode = RootModes.Exclude;
                                Item = selectItem;
                            }
                            catch (Exception ex)
                            {
                                // returnException(Form.myWeb.msException, mcModuleName, "New", ex, "", "", gbDebug)
                            }
                        }

                        #endregion
                        #region  Private Properties
                        private string ClassName
                        {
                            get
                            {
                                return _selectItem.GetAttribute("class");
                            }
                        }

                        #endregion
                        #region  Public Properties
                        public XmlElement Item
                        {
                            get
                            {
                                return _selectItem;
                            }

                            set
                            {

                                // Set the Item
                                _selectItem = value;

                                // Determine its Root Id
                                string argpropertyName = "root";
                                string rootId = getPropertyFromClass(ref argpropertyName);
                                _rootId = Conversions.ToLong(Interaction.IIf(!string.IsNullOrEmpty(rootId) & Information.IsNumeric(rootId), Conversions.ToLong(rootId), 0));

                                // Determine the root mode
                                string argpropertyName1 = "rootMode";
                                string rootModeParameter = "" + getPropertyFromClass(ref argpropertyName1);
                                switch (rootModeParameter.ToLower() ?? "")
                                {
                                    case "exclude":
                                        {
                                            _rootMode = RootModes.Exclude;
                                            break;
                                        }

                                    case "include":
                                        {
                                            _rootMode = RootModes.Include;
                                            break;
                                        }

                                    case "rootonly":
                                        {
                                            _rootMode = RootModes.RootOnly;
                                            break;
                                        }

                                    case "childrenonly":
                                        {
                                            _rootMode = RootModes.ChildrenOnly;
                                            break;
                                        }
                                }
                            }
                        }

                        public long Root
                        {
                            get
                            {
                                return _rootId;
                            }
                        }

                        public RootModes RootMode
                        {
                            get
                            {
                                return _rootMode;
                            }
                        }

                        public string Id
                        {
                            get
                            {
                                if (!string.IsNullOrEmpty(_selectItem.GetAttribute("bind")))
                                {
                                    return _selectItem.GetAttribute("bind");
                                }
                                else if (!string.IsNullOrEmpty(_selectItem.GetAttribute("ref")))
                                {
                                    return _selectItem.GetAttribute("ref");
                                }
                                else
                                {
                                    return "";
                                }
                            }
                        }
                        #endregion
                        #region  Private Methods
                        private string getPropertyFromClass(ref string propertyName)
                        {
                            Protean.stdTools.PerfMon.Log(mcModuleName, "getPropertyFromClass");
                            try
                            {
                                string pattern = @"^.*\s" + propertyName + @"-([\S]*)\s.*$";
                                return "" + Protean.stdTools.SimpleRegexFind(" " + ClassName + " ", pattern, 1);
                            }
                            catch (Exception ex)
                            {
                                // returnException(myWeb.msException, mcModuleName, "getPropertyFromClass", ex, "", "", gbDebug)
                                return "";
                            }
                        }
                        #endregion

                    }

                    #endregion


                }

                public XmlElement xFrmRegradeUser(int nUserId, long existingGroupId, string newGroupId, string FormTitle, string xFormPath, string messageId)
                {
                    string cProcessInfo = "";
                    object InstanceSessionName = "tempInstance_regrade" + nUserId.ToString();
                    try
                    {
                        // This is a generic function for a framework for all protean object.
                        // This is not intended for use but rather as an example of how xforms are processed

                        // The instance of the form needs to be saved in the session to allow repeating elements to be edited prior to saving in the database.

                        this.myWeb.moSession(InstanceSessionName) = null;
                        base.NewFrm(FormTitle);
                        base.bProcessRepeats = false;

                        // We load the xform from a file, it may be in local or in common folders.
                        base.load(xFormPath, this.myWeb.maCommonFolders);

                        // We get the instance
                        if (nUserId > 0)
                        {
                            string sNewGroupNames = "";
                            if (newGroupId.Contains(","))
                            {
                                foreach (var i in Strings.Split(newGroupId, ","))
                                    sNewGroupNames = sNewGroupNames + this.myWeb.moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Directory, Conversions.ToLong(i.ToString())) + ", ";
                                sNewGroupNames.TrimEnd();
                                sNewGroupNames.TrimEnd(',');
                            }
                            else
                            {
                                sNewGroupNames = this.myWeb.moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Directory, Conversions.ToLong(newGroupId));
                            }

                            base.bProcessRepeats = true;
                            if (this.myWeb.moSession(InstanceSessionName) is null)
                            {
                                var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                XmlElement regradeUser = (XmlElement)existingInstance.AppendChild(base.moXformElmt.OwnerDocument.CreateElement("RegradeUser"));
                                regradeUser.SetAttribute("existingGroupId", existingGroupId.ToString());
                                regradeUser.SetAttribute("existingGroupName", this.myWeb.moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Directory, existingGroupId));
                                regradeUser.SetAttribute("newGroupId", newGroupId.ToString());
                                regradeUser.SetAttribute("newGroupName", sNewGroupNames);
                                regradeUser.SetAttribute("sendEmail", "1");
                                // Remove Messages that don't match the messageId
                                regradeUser.InnerXml = this.myWeb.GetUserXML(nUserId).OuterXml;
                                foreach (XmlElement msgNode in base.Instance.SelectNodes("RegradeUser/emailer/oBodyXML/Items/Message"))
                                {
                                    if ((msgNode.GetAttribute("id") ?? "") == (messageId ?? ""))
                                    {
                                    }
                                    // do nothing we want to keep
                                    else
                                    {
                                        msgNode.ParentNode.RemoveChild(msgNode);
                                    }
                                }

                                regradeUser.AppendChild(base.Instance.SelectSingleNode("RegradeUser/emailer"));
                                base.LoadInstance(existingInstance);
                                this.myWeb.moSession(InstanceSessionName) = base.Instance;
                            }
                            else
                            {
                                base.LoadInstance(this.myWeb.moSession["tempInstance"]);
                            }
                        }

                        this.moXformElmt.SelectSingleNode("descendant-or-self::instance").InnerXml = base.Instance.InnerXml;
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                // Change User Group
                                moDbHelper.saveDirectoryRelations(nUserId, existingGroupId.ToString(), true);
                                if (newGroupId.Contains(","))
                                {
                                    foreach (var i in Strings.Split(newGroupId, ","))
                                        moDbHelper.saveDirectoryRelations(nUserId, Conversions.ToLong(i).ToString(), false);
                                }
                                else
                                {
                                    moDbHelper.saveDirectoryRelations(nUserId, newGroupId, false);
                                }

                                // Send Email
                                var oMsg = new Protean.Messaging();
                                Cms.dbHelper argodbHelper = null;
                                oMsg.emailer((XmlElement)base.Instance.SelectSingleNode("RegradeUser"), base.Instance.SelectSingleNode("RegradeUser/emailer/xsltPath").InnerText, base.Instance.SelectSingleNode("RegradeUser/emailer/fromName").InnerText, base.Instance.SelectSingleNode("RegradeUser/emailer/fromEmail").InnerText, base.Instance.SelectSingleNode("RegradeUser/User/Email").InnerText, base.Instance.SelectSingleNode("RegradeUser/emailer/SubjectLine").InnerText, odbHelper: ref argodbHelper);
                                this.myWeb.moSession(InstanceSessionName) = null;
                            }
                        }
                        else if (base.isTriggered)
                        {
                            // we have clicked a trigger so we must update the instance
                            base.updateInstanceFromRequest();
                            // lets save the instance
                            this.goSession(InstanceSessionName) = base.Instance;
                        }
                        else
                        {
                            this.goSession(InstanceSessionName) = base.Instance;
                        }

                        // we populate the values onto the form.
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        this.myWeb.moSession(InstanceSessionName) = null;
                        Protean.stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmEditUserSubscription", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }
            }






            /// <summary>
        /// <para>XFormEditor facilitates the editting of xforms within an xform.</para>
        /// <para>
        /// It creates a MasterXForm, which is the Content node under ContentDetail of the content being worked on.
        /// It maintains a MasterInstance which is the object instance of the content as a whole (from GetObjectInstance)
        /// </para>
        /// <para>
        /// MasterInstance and MasterXform are different, with MasterXform being a subcomponent of MasterInstance
        /// </para>
        /// <para>
        /// It is instantiaated with a content id.  It is then invoked with methods specific to each xform control (e.g. group, input etc).
        /// </para>
        /// </summary> 
        /// <remarks>
        /// <para>
        /// This is overridden by EonicLMS, which uses it for editting questionnaires.
        /// This has been written in a way that should work without EonicLMS (i.e. for generic form editting),
        /// but needs the following areas addressed for this generic implementation
        /// </para>
        /// <list>
        /// <item>The xform xml files for each control do not exist.  Currently only found in wellardsCommon</item>
        /// <item>xFrmEditXFormInput - New control items will not have binds created. </item>
        /// <item>I also think instantiating the object without a content id will not work.  I'm not sure if it actually should be possible.</item>
        /// </list>
        /// </remarks>
            public class XFormEditor : Protean.xForm
            {
                private const string mcModuleName = "Protean.Cms.Admin.XFormEditor";
                public const string FORMPATH = "/xforms/content";
                private XmlElement _masterInstance;
                protected Protean.xForm _masterXform;
                private System.Web.HttpRequest _request;
                protected string _schema = "generic";
                public Cms myWeb;

                public XFormEditor(ref Cms aWeb, long contentId = 0L) : base(ref aWeb.msException)
                {

                    // Set the Web context variables
                    myWeb = aWeb;
                    _request = myWeb.moRequest;
                    this.moPageXML = myWeb.moPageXml;

                    // Create the form
                    CreateMasterForm(contentId);
                }

                public XmlElement MasterInstance
                {
                    get
                    {
                        return _masterInstance;
                    }
                }

                public bool Ready
                {
                    get
                    {
                        try
                        {
                            return _masterXform is object;
                        }
                        catch (Exception ex)
                        {
                            return false;
                        }
                    }
                }

                public string ContentSchema
                {
                    get
                    {
                        return _schema;
                    }
                }

                protected virtual string[] schemaPreferenceList
                {
                    get
                    {
                        var schemaList = new string[3];
                        schemaList[0] = _schema;
                        schemaList[1] = "generic";
                        return schemaList;
                    }
                }

                protected void CreateMasterForm(long contentId = 0L)
                {
                    try
                    {
                        _masterXform = new Protean.xForm(ref myWeb.msException);
                        _masterXform.moPageXML = this.moPageXML;
                        _masterInstance = this.moPageXML.CreateElement("instance");

                        // If content id has been set, then get the instance
                        if (contentId > 0L)
                        {
                            _masterInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, contentId);
                            var argoNode = _masterInstance.SelectSingleNode("descendant-or-self::cContentXmlDetail/Content");
                            _masterXform.load(ref argoNode);
                            _schema = _masterInstance.SelectSingleNode("descendant-or-self::cContentSchemaName").InnerText;
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref myWeb.msException, mcModuleName, "CreateMasterForm", ex, "", "", Protean.stdTools.gbDebug);
                    }
                }

                protected bool loadControlForm(string controlType)
                {
                    bool success = false;
                    try
                    {
                        foreach (string schemaType in schemaPreferenceList)
                        {
                            if (this.load(FORMPATH + "/" + schemaType + "." + controlType + ".xml", myWeb.maCommonFolders))
                            {
                                success = true;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref myWeb.msException, mcModuleName, "loadForm", ex, "", "", Protean.stdTools.gbDebug);
                        return false;
                    }

                    return default;
                }


                /// <summary>
            /// This function returns an xForm to update the form being edited
            /// </summary>
            /// <param name="cRef">The Ref or Bind of the form element to be edited</param>
            /// <param name="cParRef">The Ref or Bind of the form element under which the new element will be inserted</param>
            /// <returns></returns>
            /// <remarks></remarks>

                public virtual XmlElement xFrmEditXFormGroup(string cRef, string cParRef = "")
                {
                    XmlElement oElmt;
                    string cProcessInfo = "cRef = " + cRef + ", ParRef=" + cParRef;
                    string cMode;
                    string newRef;
                    try
                    {
                        // Set the update mode
                        cMode = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(cRef), "Add", "Edit"));

                        // Create the form that we're going to populate for updating this xform control
                        this.NewFrm("EditGroup");

                        // Load in the form from a file
                        loadControlForm("group");

                        // load a default content xform if no alternative.
                        if (!string.IsNullOrEmpty(cRef))
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                            this.LoadInstanceFromInnerXml(oElmt.OuterXml);
                        }

                        if (this.isSubmitted())
                        {
                            this.updateInstanceFromRequest();
                            this.validate();
                            if (this.valid)
                            {
                                if (!string.IsNullOrEmpty(cRef))
                                {
                                    // drop the instance back into the full xform
                                    var oNode = _masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                                    oNode.ParentNode.ReplaceChild(this.Instance.FirstChild, oNode);
                                }
                                else
                                {
                                    // add new
                                    var oNode = _masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cParRef + "' or @bind='" + cParRef + "']");
                                    newRef = _masterXform.getNewRef(this.goRequest["cRef"]);
                                    oElmt = (XmlElement)this.Instance.FirstChild;
                                    oElmt.SetAttribute("ref", newRef);
                                    oNode.AppendChild(this.Instance.FirstChild);
                                }
                            }
                            else
                            {
                                this.addValues();
                            }
                        }
                        else
                        {
                            this.addValues();
                        }

                        return this.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public virtual XmlElement xFrmDeleteElement(string cRef, string cPosIndex)
                {
                    XmlElement oFrmElmt;
                    XmlNode oNode;
                    string cProcessInfo = "cRef = " + cRef + ", cPosIndex=" + cPosIndex;
                    try
                    {

                        // Generate the Delete form within the component

                        // Indentify the node by ref and position, if given.
                        if (!string.IsNullOrEmpty(cPosIndex))
                        {
                            oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item[" + cPosIndex + "]");
                        }
                        else
                        {
                            oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                        }

                        // Create the form
                        this.NewFrm("EditSelect");
                        this.submission("EditInput", "", "post");
                        oFrmElmt = this.addGroup(ref this.moXformElmt, "EditGroup", "", "Delete Element");
                        XmlNode argoNode = oFrmElmt;
                        this.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this element - \"" + oNode.SelectSingleNode("label").InnerText + "\"");
                        this.addSubmit(ref oFrmElmt, "", "Delete Element");
                        this.LoadInstanceFromInnerXml("<delete/>");

                        // Handle the submission
                        if (this.isSubmitted())
                        {
                            this.updateInstanceFromRequest();
                            this.validate();
                            if (this.valid)
                            {

                                // Delete the node and/or bind
                                deleteElementAction(ref oNode, cRef, cPosIndex);
                            }
                            else
                            {
                                this.addValues();
                            }
                        }
                        else
                        {
                            this.addValues();
                        }

                        return this.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmDeleteElement", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                /// <summary>
            /// Deletes nodes that relate to the element / item.
            /// Problem we face is that we don't know what to delete from the instance, 
            /// so we will assume that the instance node we're loooking for has a matching @ref attribute.
            /// If it's an item we will also assume that the @ref node has children that have a child node of value which
            /// matches the value of the item being deleted.
            /// </summary>
            /// <param name="ref"></param>
            /// <param name="position"></param>
            /// <remarks></remarks>
                protected virtual void deleteElementAction(ref XmlNode node, string @ref, string position = "")
                {
                    string processInfo = "";
                    string value = "";
                    XmlNode node2;
                    try
                    {

                        // If we're deleting an item, then we don't need to remove the bind, but we do need to remove the item
                        if (!string.IsNullOrEmpty(position))
                        {
                            // remove related node's item
                            value = node.SelectSingleNode("value").InnerText;
                            node2 = _masterXform.Instance.SelectSingleNode("//*[@ref='" + @ref + "']");
                            if (node2.SelectSingleNode("*[value/node()='" + value + "']") is object)
                            {
                                // remove existing node
                                node2.RemoveChild(node2.SelectSingleNode("*[value/node()='" + value + "']"));
                            }
                        }
                        else
                        {
                            // remove related node
                            node2 = _masterXform.Instance.SelectSingleNode("//*[@ref='" + @ref + "']");
                            if (node2 is object)
                            {
                                node2.ParentNode.RemoveChild(node2);
                            }

                            // remove bind
                            if (_masterXform.model.SelectSingleNode("descendant-or-self::bind[@id='" + @ref + "']") is object)
                            {
                                node2 = _masterXform.model.SelectSingleNode("descendant-or-self::bind[@id='" + @ref + "']");
                                node2.ParentNode.RemoveChild(node2);
                            }
                        }

                        // remove the element itself
                        node.ParentNode.RemoveChild(node);
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref myWeb.msException, mcModuleName, "deleteElementAction", ex, "", processInfo, Protean.stdTools.gbDebug);
                    }
                }

                public void moveElement(long nContentId, string cRef, string ewCmd, long nItemIndex = 0L)
                {
                    XmlElement oElmt;
                    string cProcessInfo = "nContentId = " + nContentId + ",cRef = " + cRef + ",ewCmd = " + ewCmd + ", nItemIndex=" + nItemIndex;
                    try
                    {
                        if (nItemIndex == 0L)
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                        }
                        else
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item[" + nItemIndex + "]");
                        }

                        switch (ewCmd ?? "")
                        {
                            case "MoveTop":
                            case "MoveItemTop":
                                {
                                    oElmt.ParentNode.InsertBefore(oElmt.CloneNode(true), oElmt.ParentNode.FirstChild);
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }

                            case "MoveUp":
                            case "MoveItemUp":
                                {
                                    oElmt.ParentNode.InsertBefore(oElmt.CloneNode(true), oElmt.PreviousSibling);
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }

                            case "MoveDown":
                            case "MoveItemDown":
                                {
                                    oElmt.ParentNode.InsertAfter(oElmt.CloneNode(true), oElmt.NextSibling);
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }

                            case "MoveBottom":
                            case "MoveItemBottom":
                                {
                                    oElmt.ParentNode.AppendChild(oElmt.CloneNode(true));
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }

                            case "DeleteItem":
                                {
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref myWeb.msException, mcModuleName, "moveElement", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                    }
                }

                public virtual XmlElement xFrmEditXFormItem(string cRef, long nItemIndex)
                {
                    XmlElement oElmt = null;
                    XmlNode oNode;
                    string sValue;
                    var nCount = default(long);
                    string cProcessInfo = "cRef = " + cRef + ", nItemIndex=" + nItemIndex;
                    try
                    {
                        // load the xform to be edited

                        if (nItemIndex != 0L)
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item[" + nItemIndex + "]");
                        }

                        // Create the form that we're going to populate for updating this xform control
                        this.NewFrm("EditSelect");

                        // Load in the form from a file
                        loadControlForm("item");

                        // set the instance from the item loaded from the xform
                        if (nItemIndex != 0L)
                        {
                            this.Instance.AppendChild(oElmt.CloneNode(true));

                            // add weighting and correct flag to the item node from answer
                            oElmt = (XmlElement)this.Instance.FirstChild;
                            sValue = oElmt.SelectSingleNode("value").InnerText;
                        }
                        else
                        {

                            // Multi choice, auto indexing
                            foreach (XmlNode currentONode in _masterXform.moXformElmt.SelectNodes("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item"))
                            {
                                oNode = currentONode;
                                if (Conversions.ToInteger(oNode.SelectSingleNode("value").InnerText) > nCount)
                                {
                                    nCount = Conversions.ToInteger(oNode.SelectSingleNode("value").InnerText);
                                }
                            }

                            this.LoadInstanceFromInnerXml("<item><label/><value>" + (nCount + 1L) + "</value></item>");
                        }

                        if (this.isSubmitted())
                        {
                            this.updateInstanceFromRequest();
                            this.validate();
                            if (this.valid)
                            {
                                sValue = this.Instance.SelectSingleNode("item/value").InnerText;
                                if (nItemIndex != 0L)
                                {
                                    // drop the instance back into the full xform
                                    oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item[" + nItemIndex + "]");
                                    oNode.ParentNode.ReplaceChild(this.Instance.FirstChild, oNode);
                                }
                                else
                                {
                                    // add new
                                    oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                                    oNode.AppendChild(this.Instance.FirstChild);
                                }

                                oNode = null;
                            }
                            else
                            {
                                this.addValues();
                            }
                        }
                        else
                        {
                            this.addValues();
                        }

                        return this.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmEditXFormItem", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }

                public virtual XmlElement xFrmEditXFormInput(string cRef, string cParRef = "", string cElementType = "")
                {
                    XmlElement oElmt;
                    string cBind = "";
                    XmlNode oNode;
                    string newRef;
                    string cProcessInfo = "";
                    try
                    {

                        // Determine the node we're looking at
                        if (!string.IsNullOrEmpty(cRef))
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                            cElementType = oElmt.Name;
                        }

                        // Load in the form
                        switch (cElementType ?? "")
                        {
                            case "select1":
                                {

                                    // Create a new form
                                    this.NewFrm("EditSelect");

                                    // Load in the form from a file
                                    loadControlForm("select1");
                                    break;
                                }

                            case "select":
                                {

                                    // Create a new form
                                    this.NewFrm("EditSelect");

                                    // Load in the form from a file
                                    loadControlForm("select");
                                    break;
                                }

                            case "input":
                                {

                                    // Create a new form
                                    this.NewFrm("EditSelect");

                                    // Load in the form from a file
                                    loadControlForm("input");
                                    break;
                                }

                            case "textarea":
                                {

                                    // Create a new form
                                    this.NewFrm("EditSelect");

                                    // Load in the form from a file
                                    loadControlForm("textarea");
                                    break;
                                }
                        }

                        // Load existing data
                        if (!string.IsNullOrEmpty(cRef))
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                            this.LoadInstanceFromInnerXml(oElmt.OuterXml);
                        }

                        if (this.isSubmitted())
                        {
                            this.updateInstanceFromRequest();
                            this.validate();
                            if (this.valid)
                            {
                                if (string.IsNullOrEmpty(cRef))
                                {
                                    newRef = _masterXform.getNewRef("control");
                                }
                                else
                                {
                                    newRef = cRef;
                                }

                                oElmt = null;

                                // remove anything unnessesary before save
                                switch (cElementType ?? "")
                                {
                                    case "select1":
                                    case "select":
                                        {
                                            // remove any empty item nodes
                                            foreach (XmlNode currentONode in this.Instance.FirstChild.SelectNodes("item"))
                                            {
                                                oNode = currentONode;
                                                if (oNode.FirstChild is null)
                                                {
                                                    oNode.ParentNode.RemoveChild(oNode);
                                                }
                                                else if (string.IsNullOrEmpty(oNode.FirstChild.InnerText))
                                                {
                                                    oNode.ParentNode.RemoveChild(oNode);
                                                }
                                            }

                                            break;
                                        }
                                }

                                // drop the instance back into the full xform
                                if (!string.IsNullOrEmpty(cRef))
                                {
                                    // replace existing
                                    oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                                    oNode.ParentNode.ReplaceChild(this.Instance.FirstChild, oNode);
                                }
                                else
                                {
                                    // add new
                                    oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cParRef + "' or @bind='" + cParRef + "']");
                                    oElmt = (XmlElement)this.Instance.FirstChild;

                                    // TODO: XFormEditor (Generic) how do we identify the node xpath to create the bind.
                                    // This is just for the EonicWeb Generic xformeditor function
                                    // not for anything overridden like EonicLMS

                                    // cBind = "whatgoeshere[@ref='" & newRef & "']"
                                    // oElmt.SetAttribute("bind", newRef)
                                    // _masterXform.addBind(newRef, cBind)

                                    oNode.AppendChild(this.Instance.FirstChild);
                                }
                            }
                            else
                            {
                                this.addValues();
                            }
                        }
                        else
                        {
                            this.addValues();
                        }

                        return this.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        Protean.stdTools.returnException(ref myWeb.msException, mcModuleName, "xFrmEditXFormInput", ex, "", cProcessInfo, Protean.stdTools.gbDebug);
                        return null;
                    }
                }








                // Public Overridable Function xFrmEditXFormInput(ByVal cRef As String, Optional ByVal cParRef As String = "", Optional ByVal cElementType As String = "") As XmlElement
                // Dim oFrmElmt As XmlElement
                // Dim oRpt1Elmt As XmlElement
                // Dim oSelElmt As XmlElement

                // Dim oElmt As XmlElement
                // Dim oElmt1 As XmlElement
                // Dim oElmt2 As XmlElement
                // Dim oElmt3 As XmlElement
                // Dim oElmt4 As XmlElement
                // Dim nCount As Long
                // Dim sValidAnswers As String = ""

                // Dim nAnswerCount As Long
                // Dim cReqd As String
                // Dim cBind As String = ""

                // Dim oNode As XmlNode

                // Dim i As Integer

                // Dim newRef As String

                // Dim cProcessInfo As String = ""

                // Try

                // If cRef <> "" Then
                // oElmt = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                // cElementType = oElmt.Name
                // nAnswerCount = oElmt.SelectNodes("item").Count + 1
                // Else
                // nAnswerCount = 4
                // End If

                // Select Case cElementType
                // Case "select1"

                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")

                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditSelect1", "", "Edit Select1")

                // Me.addInput(oFrmElmt, "ref", True, "Question Ref", "hidden")
                // Me.addBind("ref", "select1/@ref", "true()")

                // Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
                // Me.addBind("cName", "select1/label", "true()")

                // Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
                // Me.addBind("cDesc", "select1/div[@class='description']", "false()")

                // Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
                // Me.addBind("cRecRead", "select1/div[@class='recRead']", "false()")

                // oSelElmt = Me.addSelect1(oFrmElmt, "cAppearance", True, "Appearance", "", ApperanceTypes.Minimal)
                // Me.addOption(oSelElmt, "Radio Buttons", "full")
                // Me.addOption(oSelElmt, "Dropdown Selector", "minimal")
                // Me.addBind("cAppearance", "select1/@appearance", "true()")

                // For i = 1 To nAnswerCount
                // oRpt1Elmt = Me.addGroup(oFrmElmt, "Answer " & i, "horizontal", "Answer " & i)
                // Me.addTextArea(oRpt1Elmt, "A" & i, True, "Answer", "xhtml answerEditor")
                // If i = nAnswerCount Then
                // cReqd = "false()"
                // Else
                // cReqd = "true()"
                // End If
                // Me.addBind("A" & i, "select1/item[" & i & "]/label", cReqd)

                // oSelElmt = Me.addSelect1(oRpt1Elmt, "ATick", True, "Correct ", "", ApperanceTypes.Full)
                // Me.addOption(oSelElmt, "", i)
                // Next
                // Me.addBind("ATick", "select1/@correctIndex", "false()")

                // Me.addRange(oFrmElmt, "AWeighting", True, "Weighting", "0", "100", "10", "short")

                // Me.addBind("AWeighting", "select1/@weighting", "true()")

                // Me.addSubmit(oFrmElmt, "", "Save Group")

                // Case "select"

                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")

                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditSelect", "", "Edit Select")

                // Me.addInput(oFrmElmt, "ref", True, "Question Ref", "hidden")
                // Me.addBind("ref", "select/@ref", "true()")

                // Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
                // Me.addBind("cName", "select/label", "true()")

                // Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
                // Me.addBind("cDesc", "select/div[@class='description']", "false()")

                // Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
                // Me.addBind("cRecRead", "select/div[@class='recRead']", "false()")

                // oSelElmt = Me.addSelect1(oFrmElmt, "cAppearance", True, "Appearance", "", ApperanceTypes.Minimal)
                // Me.addOption(oSelElmt, "CheckBoxes", "full")
                // Me.addOption(oSelElmt, "Dropdown Selector", "minimal")
                // Me.addBind("cAppearance", "select/@appearance", "true()")

                // For i = 1 To nAnswerCount
                // oRpt1Elmt = Me.addGroup(oFrmElmt, "Answer " & i, "horizontal", "Answer " & i)
                // Me.addTextArea(oRpt1Elmt, "A" & i, True, "Answer", "xhtml answerEditor")
                // Me.addBind("A" & i, "select/item[" & i & "]/label", "false()")

                // oSelElmt = Me.addSelect(oRpt1Elmt, "A" & i & "Tick", True, "Correct ", "", ApperanceTypes.Full)
                // Me.addOption(oSelElmt, "", "true")
                // Me.addBind("A" & i & "Tick", "select/item[" & i & "]/@correct", "false()")

                // Me.addRange(oRpt1Elmt, "A" & i & "Weighting", True, "Weighting", "0", "100", "10", "short")

                // Me.addBind("A" & i & "Weighting", "select/item[" & i & "]/@weighting", "false()")
                // Next

                // Me.addSubmit(oFrmElmt, "", "Save Group")

                // Case "input"

                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")

                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditInput", "", "Edit Input")

                // '  Me.addInput(oFrmElmt, "ref", True, "Question Ref", "hidden")
                // ' Me.addBind("ref", "input/@ref", "true()")

                // Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
                // Me.addBind("cName", "input/label", "true()")

                // Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
                // Me.addBind("cDesc", "input/div[@class='description']", "false()")

                // Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
                // Me.addBind("cRecRead", "input/div[@class='recRead']", "false()")

                // Me.addInput(oFrmElmt, "cAnswers", True, "Valid Answers")
                // Me.addBind("cAnswers", "input/@validAnswers", "false()")
                // Me.addNote("cAnswers", xForm.noteTypes.Hint, "Comma separated list of valid answers")

                // Me.addRange(oFrmElmt, "cWeighting", True, "Weighting", "0", "100", "10")
                // Me.addBind("cWeighting", "input/@weighting", "true()")

                // Me.addSubmit(oFrmElmt, "", "Save Group")

                // Case "textarea"

                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")

                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditInput", "", "Edit Comprehension Question")

                // Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
                // Me.addBind("cName", "textarea/label", "true()")

                // Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
                // Me.addBind("cDesc", "textarea/div[@class='description']", "false()")

                // Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
                // Me.addBind("cRecRead", "textarea/div[@class='recRead']", "false()")

                // Me.addSubmit(oFrmElmt, "", "Save Group")

                // End Select

                // ' load a default content xform if no alternative.

                // If cRef <> "" Then
                // oElmt = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                // Select Case cElementType
                // Case "select1"
                // 'step through the answers in the question were editing
                // i = 1
                // Dim bResult As Boolean = False
                // For Each oElmt2 In oElmt.SelectNodes("item")

                // 'get the answer value
                // Dim sVal As String = oElmt2.SelectSingleNode("value").InnerText
                // 'see if that value is in score of the answers fot that question
                // If Not moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']") Is Nothing Then
                // oElmt.SetAttribute("correctIndex", i)
                // oElmt3 = moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']")
                // oElmt.SetAttribute("weighting", oElmt3.GetAttribute("weighting"))
                // bResult = True
                // End If
                // i = i + 1

                // Next
                // If Not bResult Then ' add in the empties
                // oElmt.SetAttribute("correctIndex", "")
                // oElmt.SetAttribute("weighting", "")
                // End If
                // Case "select"
                // 'step through the answers in the question were editing
                // For Each oElmt2 In oElmt.SelectNodes("item")
                // 'get the answer value
                // Dim sVal As String = oElmt2.SelectSingleNode("value").InnerText
                // 'see if that value is in score of the answers fot that question
                // If Not moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']") Is Nothing Then
                // oElmt2.SetAttribute("correct", "true")
                // oElmt3 = moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']")
                // oElmt2.SetAttribute("weighting", oElmt3.GetAttribute("weighting"))
                // Else
                // oElmt2.SetAttribute("correct", "false")
                // oElmt2.SetAttribute("weighting", "")
                // End If
                // Next
                // Case "input"
                // For Each oElmt2 In moXForm2Edit.Instance.SelectNodes("results/answers/answer[@ref='" & cRef & "']/score/value")
                // If sValidAnswers = "" Then
                // sValidAnswers = oElmt2.InnerText
                // Else
                // sValidAnswers = sValidAnswers & ", " & oElmt2.InnerText
                // End If
                // Next
                // oElmt.SetAttribute("validAnswers", sValidAnswers)
                // oElmt.SetAttribute("weighting", moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score/@weighting").Value)
                // End Select

                // Me.Instance.AppendChild(oElmt.CloneNode(True))

                // Else
                // Select Case cElementType
                // Case "select1"
                // Me.Instance.InnerXml = "<select1 bind="""" appearance="""" correctIndex="""" weighting=""""><label/><div class=""description""/><div class=""recRead""/></select1>"
                // Case "select"
                // Me.Instance.InnerXml = "<select bind="""" appearance=""""><label/><div class=""description""/><div class=""recRead""/></select>"
                // Case "input"
                // Me.Instance.InnerXml = "<input bind="""" validAnswers="""" weighting=""""><label/><div class=""description""/><div class=""recRead""/></input>"
                // Case "textarea"
                // Me.Instance.InnerXml = "<textarea bind="""" rows=""20"" class=""textarea_compquiz""><label/><div class=""description""/><div class=""recRead""/></textarea>"

                // End Select
                // End If

                // Select Case cElementType
                // Case "select1", "select"
                // 'ensure we have the right number of blank item nodes to update.
                // For i = 1 To nAnswerCount
                // If Me.Instance.FirstChild.SelectSingleNode("item[" & i & "]") Is Nothing Then
                // 'populate new nodes with values
                // For Each oNode In Me.Instance.FirstChild.SelectNodes("item")
                // If CInt(oNode.SelectSingleNode("value").InnerText) > nCount Then
                // nCount = CInt(oNode.SelectSingleNode("value").InnerText)
                // End If
                // Next
                // oElmt1 = Me.addOption(Me.Instance.FirstChild, "", nCount + 1)
                // If cElementType = "select" Then
                // oElmt1.SetAttribute("correct", "")
                // oElmt1.SetAttribute("weighting", "")
                // End If
                // End If
                // Next

                // End Select

                // If Me.isSubmitted Then
                // Me.updateInstanceFromRequest()
                // Me.validate()

                // If Me.valid Then

                // If cRef = "" Then
                // newRef = moXForm2Edit.getNewRef("Q")
                // Else
                // newRef = cRef
                // End If

                // oElmt = Nothing

                // 'Update the Answer Node
                // Select Case cElementType
                // Case "select1"
                // 'Code to update single answer node
                // 'get the correctIndex and find the value
                // oElmt2 = Me.Instance.SelectSingleNode("select1")
                // If oElmt2.GetAttribute("correctIndex") <> "" Then
                // Dim nCorIdx As Integer = oElmt2.GetAttribute("correctIndex")

                // If Not Me.Instance.SelectSingleNode("select1/item[" & nCorIdx & "]") Is Nothing Then
                // oElmt3 = Me.Instance.SelectSingleNode("select1/item[" & nCorIdx & "]")
                // oElmt4 = oElmt3.SelectSingleNode("value")
                // oElmt = addAnswerElmt(moXForm2Edit, newRef, oElmt2.GetAttribute("weighting"), oElmt4.InnerText, True)
                // End If
                // End If

                // Case "select"
                // 'Code to update any answer nodes required
                // 'remove any existing answernodes
                // clearAnswerScores(moXForm2Edit, newRef)
                // 'step through the answers in the select instance to find valid correct ones
                // For Each oElmt2 In Me.Instance.SelectNodes("select/item")
                // If oElmt2.GetAttribute("correct") = "true" Then
                // oElmt3 = oElmt2.SelectSingleNode("value")
                // oElmt = addAnswerElmt(moXForm2Edit, newRef, oElmt2.GetAttribute("weighting"), oElmt3.InnerText)
                // End If
                // Next
                // Case "input"
                // 'Code to update answer node
                // oElmt = addAnswerElmt(moXForm2Edit, newRef, moRequest("cWeighting"), "", True)
                // If moRequest("cAnswers") <> "" Then
                // Dim aCorrect() As String = Split(moRequest("cAnswers"), ",")
                // For i = 0 To UBound(aCorrect)
                // addCorrectAnswer(oElmt, Trim(aCorrect(i)))
                // Next
                // End If
                // Case "textarea"
                // 'Code to update answer node
                // oElmt = addAnswerElmt(moXForm2Edit, newRef, moRequest("cWeighting"), "", True, cElementType)
                // End Select


                // 'remove anything unnessesary before save
                // Select Case cElementType
                // Case "select1", "select"
                // 'remove any empty item nodes
                // For Each oNode In Me.Instance.FirstChild.SelectNodes("item")
                // If oNode.FirstChild Is Nothing Then
                // oNode.ParentNode.RemoveChild(oNode)
                // Else
                // If oNode.FirstChild.InnerText = "" Then
                // oNode.ParentNode.RemoveChild(oNode)
                // End If
                // End If
                // If cElementType = "select" Then
                // oElmt2 = oNode
                // oElmt2.RemoveAttribute("correct")
                // oElmt2.RemoveAttribute("weighting")
                // oElmt2 = Nothing
                // End If
                // Next
                // If cElementType = "select" Then
                // oElmt2 = Me.Instance.FirstChild
                // oElmt2.RemoveAttribute("correctIndex")
                // oElmt2.RemoveAttribute("weighting")
                // oElmt2 = Nothing
                // End If

                // Case "input"
                // 'remove validAnswers Attrib
                // oElmt = Me.Instance.FirstChild
                // oElmt.RemoveAttribute("validAnswers")

                // End Select

                // 'drop the instance back into the full xform
                // If cRef <> "" Then
                // 'replace existing
                // oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                // oNode.ParentNode.ReplaceChild(Me.Instance.FirstChild, oNode)
                // Else
                // 'add new
                // oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cParRef & "' or @bind='" & cParRef & "']")
                // oElmt = Me.Instance.FirstChild()

                // 'everything is bound to a single answer node
                // Select Case cElementType
                // Case "textarea"
                // cBind = "results/answers/answer[@ref='" & newRef & "']/given"
                // Case Else
                // cBind = "results/answers/answer[@ref='" & newRef & "']/@given"
                // End Select
                // oElmt.SetAttribute("bind", newRef)
                // moXForm2Edit.addBind(newRef, cBind)

                // oNode.AppendChild(Me.Instance.FirstChild())
                // End If

                // Else
                // Me.addValues()
                // End If
                // Else
                // Me.addValues()
                // End If

                // Return Me.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function


                // Public Overridable Function xFrmEditXFormItem(ByVal cRef As String, ByVal nItemIndex As Long) As XmlElement
                // Dim oFrmElmt As XmlElement
                // Dim oSelElmt As XmlElement

                // Dim oElmt As XmlElement = Nothing

                // Dim oNode As XmlNode

                // Dim sQuestionType As String
                // Dim sValue As String

                // Dim nCount As Long

                // Dim cProcessInfo As String = ""

                // Try
                // 'load the xform to be edited

                // If nItemIndex <> 0 Then
                // oElmt = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & nItemIndex & "]")
                // End If

                // 'What Q type are we editing...?
                // sQuestionType = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']").Name


                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")
                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditGroup", "", "Edit Answer")

                // Me.addTextArea(oFrmElmt, "cName", True, "Answer", "xhtml answerEditor")
                // Me.addBind("cName", "item/label", "true()")

                // oSelElmt = Me.addSelect(oFrmElmt, "bCorrect", True, "Correct Answer", "", ApperanceTypes.Full)
                // Me.addOption(oSelElmt, "Correct", "true")
                // Me.addBind("bCorrect", "item/@correct", "false()")

                // Me.addRange(oFrmElmt, "cWeighting", True, "Weighting", "0", "100", "5")
                // Me.addBind("cWeighting", "item/@weighting", "false()")

                // Me.addSubmit(oFrmElmt, "", "Save Answer")

                // ' set the instance from the item loaded from the xform
                // If nItemIndex <> 0 Then
                // Me.Instance.AppendChild(oElmt.CloneNode(True))
                // ' add weighting and correct flag to the item node from answer
                // oElmt = Me.Instance.FirstChild

                // sValue = oElmt.SelectSingleNode("value").InnerText

                // If moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sValue & "']") Is Nothing Then
                // oElmt.SetAttribute("correct", "false")
                // oElmt.SetAttribute("weighting", "")
                // Else
                // oElmt.SetAttribute("correct", "true")
                // oElmt.SetAttribute("weighting", moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sValue & "']/@weighting").Value)
                // End If

                // Else
                // For Each oNode In moXForm2Edit.moXformElmt.SelectNodes("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item")
                // If CInt(oNode.SelectSingleNode("value").InnerText) > nCount Then
                // nCount = CInt(oNode.SelectSingleNode("value").InnerText)
                // End If
                // Next
                // Me.Instance.InnerXml = "<item><label/><value>" & nCount + 1 & "</value></item>"
                // End If

                // If Me.isSubmitted Then
                // Me.updateInstanceFromRequest()
                // Me.validate()
                // If Me.valid Then
                // 'remove the weighting and correct attribs
                // Me.Instance.RemoveAttribute("correct")
                // Me.Instance.RemoveAttribute("weighting")

                // sValue = Me.Instance.SelectSingleNode("item/value").InnerText

                // If nItemIndex <> 0 Then
                // 'drop the instance back into the full xform
                // oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & nItemIndex & "]")
                // oNode.ParentNode.ReplaceChild(Me.Instance.FirstChild, oNode)
                // Else
                // 'add new
                // oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                // oNode.AppendChild(Me.Instance.FirstChild)
                // End If
                // oNode = Nothing

                // If moRequest("bCorrect") = "true" Then
                // Select Case sQuestionType
                // Case "select1"
                // oElmt = addAnswerElmt(moXForm2Edit, cRef, moRequest("cWeighting"), sValue, True)
                // Case Else
                // oElmt = addAnswerElmt(moXForm2Edit, cRef, moRequest("cWeighting"), sValue, False)
                // End Select

                // Else
                // 'if we are a select (multi choice) we need to remove the score node if exists.
                // oNode = moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']")
                // Select Case sQuestionType
                // Case "select"
                // If Not oNode.SelectSingleNode("score[value/node()='" & sValue & "']") Is Nothing Then
                // 'remove existing node
                // oNode.RemoveChild(oNode.SelectSingleNode("score[value/node()='" & sValue & "']"))

                // End If
                // End Select
                // End If
                // Else
                // Me.addValues()
                // End If
                // Else
                // Me.addValues()
                // End If

                // Return Me.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                // Private Function addAnswerElmt(ByRef oxFrm As Protean.xForm, ByVal cRef As String, ByVal cWeighting As String, Optional ByVal cValue As String = "", Optional ByVal bSingleAnswer As Boolean = False, Optional ByVal cElementType As String = "") As XmlElement

                // Dim oElmt As XmlElement
                // Dim oElmt2 As XmlElement
                // Dim oElmt3 As XmlElement

                // Dim oNode As XmlNode


                // Dim cProcessInfo As String = ""
                // Try
                // If oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']") Is Nothing Then

                // oElmt = oxFrm.moPageXML.CreateElement("answer")
                // oElmt.SetAttribute("ref", cRef)
                // Select Case cElementType
                // Case "textarea"
                // addNewTextNode("given", oElmt)
                // Case Else
                // oElmt.SetAttribute("given", "")
                // End Select
                // oElmt.SetAttribute("mark", "")
                // Else
                // oElmt = oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']").CloneNode(True)
                // If bSingleAnswer Then
                // 'remove existing score
                // For Each oNode In oElmt.SelectNodes("score")
                // oNode.ParentNode.RemoveChild(oNode)
                // Next
                // End If
                // End If
                // If Not oElmt.SelectSingleNode("score[value/node()='" & cValue & "']") Is Nothing Then
                // 'score exists update the weighting
                // oElmt2 = oElmt.SelectSingleNode("score[value/node()='" & cValue & "']")
                // oElmt2.SetAttribute("weighting", cWeighting)
                // Else
                // oElmt2 = oxFrm.moPageXML.CreateElement("score")
                // oElmt2.SetAttribute("weighting", cWeighting)
                // If cValue <> "" Then
                // oElmt3 = oxFrm.moPageXML.CreateElement("value")
                // oElmt3.InnerText = cValue
                // oElmt2.AppendChild(oElmt3)
                // End If
                // oElmt.AppendChild(oElmt2)
                // End If

                // If oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']") Is Nothing Then
                // 'Create a new one
                // oNode = oxFrm.Instance.SelectSingleNode("results/answers")
                // oNode.AppendChild(oElmt)
                // Else
                // 'replace existing
                // oNode = oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']")
                // 'replace the whole lot
                // oNode.ParentNode.ReplaceChild(oElmt, oNode)
                // End If

                // Return oElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "addAnswerNode", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                // Private Sub clearAnswerScores(ByRef oxFrm As Protean.xForm, ByVal cRef As String)

                // Dim oElmt As XmlElement

                // Dim oNode As XmlNode


                // Dim cProcessInfo As String = ""
                // Try
                // If Not oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']") Is Nothing Then
                // oElmt = oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']")
                // For Each oNode In oElmt.SelectNodes("score")
                // oNode.ParentNode.RemoveChild(oNode)
                // Next
                // End If

                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "clearAnswerScores", ex, "", cProcessInfo, gbDebug)
                // End Try
                // End Sub

                // Private Function addCorrectAnswer(ByRef oElmt As XmlElement, ByVal cValue As String) As XmlElement

                // Dim oElmt3 As XmlElement

                // Dim cProcessInfo As String = ""
                // Try

                // oElmt3 = oElmt.OwnerDocument.CreateElement("value")
                // oElmt3.InnerText = cValue
                // oElmt.FirstChild.AppendChild(oElmt3)

                // Return oElmt
                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "addCorrectAnswer", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function


            }

            ~Admin()
            {
            }
        }
    }
}