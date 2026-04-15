// ***********************************************************************
// $Library:     protean.cms.adminXforms
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.com)
// &Website:     eonic.com
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2022 Eonic Digital LLP.
// ***********************************************************************


using Protean.Providers.Membership;
using System;
using System.Xml;
using static Protean.stdTools;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin
        {
            public partial class AdminXforms : xForm, IDisposable
            {

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
                // returnException(myWeb.msException, _moduleName, "xFrmUserLogon", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function


                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmPasswordReminder()
                {
                    string cProcessInfo = "";

                    try
                    {

                        Cms argmyWeb = myWeb;

                        IMembershipAdminXforms oAdXfm = myWeb.moMemProv.AdminXforms;
                        myWeb = (Cms)argmyWeb;

                        oAdXfm.xFrmPasswordReminder();
                        valid = Convert.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmPasswordReminder", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmActivateAccount()
                {
                    string cProcessInfo = "";

                    try
                    {


                        IMembershipAdminXforms oAdXfm = myWeb.moMemProv.AdminXforms;

                        oAdXfm.xFrmActivateAccount();
                        valid = Convert.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmActivateAccount", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmResetAccount(long userId = 0L)
                {
                    string cProcessInfo = "";

                    try
                    {


                        IMembershipAdminXforms oAdXfm = myWeb.moMemProv.AdminXforms;

                        oAdXfm.xFrmResetAccount(userId);
                        valid = Convert.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmResetAccount", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }



                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmConfirmPassword(string AccountHash)
                {
                    string cProcessInfo = "";

                    try
                    {

                        IMembershipAdminXforms oAdXfm = myWeb.moMemProv.AdminXforms;

                        oAdXfm.xFrmConfirmPassword(AccountHash);
                        valid = Convert.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmConfirmPassword", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }



                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmConfirmPassword(long nUserId)
                {
                    string cProcessInfo = "";

                    try
                    {


                        IMembershipAdminXforms oAdXfm = myWeb.moMemProv.AdminXforms;

                        oAdXfm.xFrmConfirmPassword(nUserId);
                        valid = Convert.ToBoolean(oAdXfm.valid);
                        return (XmlElement)oAdXfm.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmConfirmPassword", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                [Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", false)]
                public virtual XmlElement xFrmEditDirectoryItem(ref XmlElement InstanceAppend, long id = 0L, string cDirectorySchemaName = "User", long parId = 0L, string cXformName = "", string FormXML = "")
                {
                    string cProcessInfo = "";

                    try
                    {

                        var oAdXfm = myWeb.moMemProv.AdminXforms;

                        oAdXfm.xFrmEditDirectoryItem(ref InstanceAppend, id, cDirectorySchemaName, parId, cXformName, FormXML);

                        valid = Convert.ToBoolean(oAdXfm.valid);
                        moXformElmt = (XmlElement)oAdXfm.moXformElmt;
                        updateInstance((XmlElement)oAdXfm.Instance);
                        return moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditDirectoryItem", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


            }
        }
    }
}