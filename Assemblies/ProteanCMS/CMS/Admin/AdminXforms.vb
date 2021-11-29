
'***********************************************************************
' $Library:     eonic.adminXforms
' $Revision:    3.1  
' $Date:        2006-03-02
' $Author:      Trevor Spink (trevor@eonic.co.uk)
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2006 Eonic Ltd.
'***********************************************************************

Option Strict Off
Option Explicit On

Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Collections.Specialized
Imports Protean.Tools
Imports Protean.Tools.Xml
Imports Protean.Tools.Text
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports System.Reflection

Partial Public Class Cms
    Partial Public Class Admin
        Public Class AdminXforms
            Inherits xForm

            Private Const mcModuleName As String = "ewcommon.AdminXForms"
            'Private Const gbDebug As Boolean = True
            Public moDbHelper As dbHelper
            Public goConfig As System.Collections.Specialized.NameValueCollection ' = WebConfigurationManager.GetWebApplicationSection("protean/web")
            Public mbAdminMode As Boolean = False
            Public moRequest As System.Web.HttpRequest

            ' Error Handling hasn't been formally set up for AdminXforms so this is just for method invocation found in xfrmEditContent
            Shadows Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

            Private Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs) Handles Me.OnError
                returnException(myWeb.msException, mcModuleName, e.ProcedureName, e.Exception, "", e.AddtionalInformation, gbDebug)
            End Sub

            'Public myWeb As Protean.Cms

            Public Sub New(ByRef aWeb As Protean.Cms)
                MyBase.New(aWeb)

                PerfMon.Log("AdminXforms", "New")
                Try
                    myWeb = aWeb
                    goConfig = myWeb.moConfig
                    moDbHelper = myWeb.moDbHelper
                    moRequest = myWeb.moRequest

                    MyBase.cLanguage = myWeb.mcPageLanguage

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "New", ex, "", "", gbDebug)
                End Try
            End Sub

            Public Sub New(ByRef sException As String)
                MyBase.New(sException)
            End Sub

            Public Shadows Sub open(ByVal oPageXml As XmlDocument)
                Dim cProcessInfo As String = ""
                Try
                    moPageXML = oPageXml

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "Open", ex, "", cProcessInfo, gbDebug)
                End Try
            End Sub

            Public Function xFrmGenericObject(ByVal nObjectId As Integer, ByVal FormTitle As String, ByVal xFormPath As String, ByVal ptnObjectType As dbHelper.objectTypes) As XmlElement
                Dim cProcessInfo As String = ""
                Try
                    ' This is a generic function for a framework for all protean object.
                    'This is not intended for use but rather as an example of how xforms are processed

                    'The instance of the form needs to be saved in the session to allow repeating elements to be edited prior to saving in the database.
                    Dim InstanceSessionName = "tempInstance_" & ptnObjectType.ToString() & "_" & nObjectId.ToString()

                    MyBase.NewFrm(FormTitle)
                    MyBase.bProcessRepeats = False

                    'We load the xform from a file, it may be in local or in common folders.
                    MyBase.load(xFormPath, myWeb.maCommonFolders)

                    'We get the instance
                    If nObjectId > 0 Then
                        MyBase.bProcessRepeats = True
                        If myWeb.moSession(InstanceSessionName) Is Nothing Then
                            Dim existingInstance As XmlElement = MyBase.moXformElmt.OwnerDocument.CreateElement("instance")
                            existingInstance.InnerXml = moDbHelper.getObjectInstance(ptnObjectType, nObjectId).Replace("xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "").Replace("xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", "")
                            MyBase.LoadInstance(existingInstance)
                            myWeb.moSession(InstanceSessionName) = MyBase.Instance
                        Else
                            MyBase.LoadInstance(myWeb.moSession("tempInstance"))
                        End If
                    End If

                    moXformElmt.SelectSingleNode("descendant-or-self::instance").InnerXml = MyBase.Instance.InnerXml

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            Dim nCId As Integer = moDbHelper.setObjectInstance(ptnObjectType, MyBase.Instance, nObjectId)
                            myWeb.moSession("tempInstance") = Nothing
                        End If
                    ElseIf MyBase.isTriggered Then
                        'we have clicked a trigger so we must update the instance
                        MyBase.updateInstanceFromRequest()
                        'lets save the instance
                        goSession(InstanceSessionName) = MyBase.Instance
                    Else
                        goSession(InstanceSessionName) = MyBase.Instance
                    End If

                    'we populate the values onto the form.
                    MyBase.addValues()

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditUserSubscription", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            <Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", False)>
            Public Overridable Function xFrmUserLogon(Optional ByVal FormName As String = "UserLogon") As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                    Dim oAdXfm As Object = oMembershipProv.AdminXforms

                    oAdXfm.xFrmUserLogon(FormName)
                    valid = oAdXfm.valid
                    Return oAdXfm.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmUserLogon", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            '            Public Overridable Function xFrmUserLogon(Optional ByVal FormName As String = "UserLogon") As XmlElement
            '                Dim oFrmElmt As XmlElement
            '                Dim oSelElmt As XmlElement
            '                Dim sValidResponse As String
            '                Dim cProcessInfo As String = ""
            '                Dim bRememberMe As Boolean = False
            '                Try
            '                    MyBase.NewFrm("UserLogon")

            '                    If mbAdminMode And myWeb.mnUserId = 0 Then GoTo BuildForm
            '                    If myWeb.moConfig("RememberMeMode") = "KeepCookieAfterLogoff" Or myWeb.moConfig("RememberMeMode") = "ClearCookieAfterLogoff" Then bRememberMe = True

            '                    If Not MyBase.load("/xforms/directory/" & FormName & ".xml", myWeb.maCommonFolders) Then
            '                        GoTo BuildForm
            '                    Else
            '                        GoTo Check
            '                    End If
            'BuildForm:
            '                    MyBase.submission("UserLogon", "", "post", "form_check(this)")
            '                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "UserDetails", "", "Please fill in your login details below")
            '                    MyBase.addInput(oFrmElmt, "cUserName", True, "Username")
            '                    MyBase.addBind("cUserName", "user/username", "true()")
            '                    MyBase.addSecret(oFrmElmt, "cPassword", True, "Password")
            '                    MyBase.addBind("cPassword", "user/password", "true()")

            '                    MyBase.addSubmit(oFrmElmt, "ewSubmit", "Login")
            '                    MyBase.Instance.InnerXml = "<user rememberMe=""""><username/><password/></user>"
            'Check:


            '                    ' Set the remember me value
            '                    If bRememberMe Then

            '                        ' Add elements to the form if not present
            '                        If Tools.Xml.NodeState(MyBase.model, "bind[@id='cRemember']") = Tools.Xml.XmlNodeState.NotInstantiated Then
            '                            oSelElmt = MyBase.addSelect(MyBase.moXformElmt.SelectSingleNode("group"), "cRemember", True, "&#160;", "", ApperanceTypes.Full)
            '                            MyBase.addOption(oSelElmt, "Remember me", "true")
            '                            MyBase.addBind("cRemember", "user/@rememberMe", "false()")
            '                        End If

            '                        ' Retrieve values from the cookie
            '                        If Not goRequest.Cookies("RememberMeUserName") Is Nothing Then
            '                            Dim cRememberedUsername As String = goRequest.Cookies("RememberMeUserName").Value
            '                            Dim bRemembered As Boolean = False
            '                            Dim oElmt As XmlElement = Nothing

            '                            If cRememberedUsername <> "" Then bRemembered = True

            '                            If Tools.Xml.NodeState(MyBase.Instance, "user", , , , oElmt) <> Tools.Xml.XmlNodeState.NotInstantiated And Not (MyBase.isSubmitted) Then

            '                                oElmt.SetAttribute("rememberMe", LCase(CStr(bRemembered)))
            '                                Tools.Xml.NodeState(MyBase.Instance, "user/username", cRememberedUsername)

            '                            End If
            '                        End If
            '                    End If



            '                    If MyBase.isSubmitted Then
            '                        MyBase.updateInstanceFromRequest()
            '                        MyBase.validate()
            '                        If MyBase.valid Then

            '                            sValidResponse = moDbHelper.validateUser(goRequest("cUserName"), goRequest("cPassWord"))

            '                            If IsNumeric(sValidResponse) Then
            '                                myWeb.mnUserId = CLng(sValidResponse)
            '                                moDbHelper.mnUserId = CLng(sValidResponse)
            '                                If Not goSession Is Nothing Then
            '                                    goSession("nUserId") = myWeb.mnUserId
            '                                End If

            '                                ' Set the remember me cookie
            '                                If bRememberMe Then
            '                                    If goRequest("cRemember") = "true" Then
            '                                        Dim oCookie As System.Web.HttpCookie
            '                                        If Not (myWeb.moRequest.Cookies("RememberMeUserName") Is Nothing) Then goResponse.Cookies.Remove("RememberMeUserName")
            '                                        oCookie = New System.Web.HttpCookie("RememberMeUserName")
            '                                        oCookie.Value = myWeb.moRequest("cUserName")
            '                                        oCookie.Expires = DateAdd(DateInterval.Day, 60, Now())
            '                                        goResponse.Cookies.Add(oCookie)

            '                                        If Not (myWeb.moRequest.Cookies("RememberMeUserId") Is Nothing) Then goResponse.Cookies.Remove("RememberMeUserId")
            '                                        oCookie = New System.Web.HttpCookie("RememberMeUserId")
            '                                        oCookie.Value = myWeb.mnUserId
            '                                        oCookie.Expires = DateAdd(DateInterval.Day, 60, Now())
            '                                        goResponse.Cookies.Add(oCookie)
            '                                    Else
            '                                        goResponse.Cookies("RememberMeUserName").Expires = DateTime.Now.AddDays(-1)
            '                                        goResponse.Cookies("RememberMeUserId").Expires = DateTime.Now.AddDays(-1)
            '                                    End If
            '                                End If
            '                            Else
            '                                valid = False
            '                                MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
            '                            End If
            '                        Else
            '                            valid = False
            '                        End If
            '                        If valid = False Then
            '                            myWeb.mnUserId = 0
            '                        End If
            '                    End If

            '                    MyBase.addValues()
            '                    Return MyBase.moXformElmt

            '                Catch ex As Exception
            '                    returnException(myWeb.msException, mcModuleName, "xFrmUserLogon", ex, "", cProcessInfo, gbDebug)
            '                    Return Nothing
            '                End Try
            '            End Function


            <Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", False)>
            Public Overridable Function xFrmPasswordReminder() As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                    Dim oAdXfm As Object = oMembershipProv.AdminXforms

                    oAdXfm.xFrmPasswordReminder()
                    valid = oAdXfm.valid
                    Return oAdXfm.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmPasswordReminder", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            <Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", False)>
            Public Overridable Function xFrmActivateAccount() As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                    Dim oAdXfm As Object = oMembershipProv.AdminXforms

                    oAdXfm.xFrmActivateAccount()
                    valid = oAdXfm.valid
                    Return oAdXfm.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmActivateAccount", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            <Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", False)>
            Public Overridable Function xFrmResetAccount() As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                    Dim oAdXfm As Object = oMembershipProv.AdminXforms

                    oAdXfm.xFrmResetAccount()
                    valid = oAdXfm.valid
                    Return oAdXfm.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmResetAccount", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function



            <Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", False)>
            Public Overridable Function xFrmConfirmPassword(ByVal AccountHash As String) As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                    Dim oAdXfm As Object = oMembershipProv.AdminXforms

                    oAdXfm.xFrmConfirmPassword(AccountHash)
                    valid = oAdXfm.valid
                    Return oAdXfm.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmConfirmPassword", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function



            <Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", False)>
            Public Overridable Function xFrmConfirmPassword(ByVal nUserId As Long) As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                    Dim oAdXfm As Object = oMembershipProv.AdminXforms

                    oAdXfm.xFrmConfirmPassword(nUserId)
                    valid = oAdXfm.valid
                    Return oAdXfm.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmConfirmPassword", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmUserIntegrations(ByVal userId As Long, ByVal secondaryCommand As String) As XmlElement

                Dim processInfo As String = ""
                Dim provider As String = ""
                Dim formName As String = "UserIntegrations"
                Dim userInstance As XmlElement = Nothing
                Dim credentials As XmlElement = Nothing
                Dim permissions As XmlElement = Nothing
                Try

                    ' Handle the direct integration commands
                    myWeb.CommonActions()
                    provider = myWeb.moRequest("provider")

                    ' Create the form
                    If Not String.IsNullOrEmpty(secondaryCommand) Then formName &= "." & secondaryCommand
                    MyBase.NewFrm(formName)

                    ' Add form parameters
                    If Not String.IsNullOrEmpty(provider) Then
                        Dim integrationsFormParameters() As String = {provider}
                        Me.FormParameters = integrationsFormParameters
                    End If

                    ' Load the form
                    If MyBase.load("/xforms/directory/" & formName & ".xml", myWeb.maCommonFolders) Then



                        If userId > 0 Then

                            ' Load the user instance
                            userInstance = MyBase.moXformElmt.OwnerDocument.CreateElement("instance")
                            userInstance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, userId)

                            ' If this is permissions then we need to extract the permissions first, or create new ones.
                            If secondaryCommand = "Permissions" Then
                                credentials = userInstance.SelectSingleNode("//Credentials[@provider='" & provider & "']")
                                permissions = credentials.SelectSingleNode("Permissions")
                                If permissions IsNot Nothing Then MyBase.Instance.InnerXml = permissions.OuterXml
                            Else
                                ' By default add the user instance
                                MyBase.Instance.InnerXml = userInstance.InnerXml
                            End If


                            ' Handle the submits
                            If MyBase.isSubmitted Then
                                MyBase.updateInstanceFromRequest()
                                MyBase.validate()
                                If MyBase.valid Then

                                    ' Permissions
                                    If secondaryCommand = "Permissions" Then

                                        ' Need to put the permissions node back into the user instance
                                        If credentials IsNot Nothing Then

                                            If permissions IsNot Nothing Then
                                                credentials.ReplaceChild(credentials.OwnerDocument.ImportNode(firstElement(MyBase.Instance), True), permissions)
                                            Else
                                                credentials.AppendChild(credentials.OwnerDocument.ImportNode(firstElement(MyBase.Instance), True))
                                            End If

                                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, userInstance, userId)

                                        Else
                                            ' No credentials to save something against
                                        End If

                                    End If



                                    ' Go back to UserIntegrations
                                    Dim newCmd As New NameValueCollection(1)
                                    newCmd.Add("ewCmd", "UserIntegrations")
                                    myWeb.msRedirectOnEnd = Tools.Http.Utils.BuildURIFromRequest(myWeb.moRequest, newCmd, "integration,provider").ToString()
                                End If
                            End If

                        Else
                            ' No user found - bad times.

                        End If

                        ' Because we have handled the integrations, we need to follow up any responses
                        For Each response As XmlElement In myWeb.PageXMLResponses

                            Select Case response.GetAttribute("type")

                                Case "Redirect"
                                    myWeb.msRedirectOnEnd = response.InnerText
                                Case "Alert"
                                    MyBase.addNote(MyBase.moXformElmt.SelectSingleNode("group"), noteTypes.Alert, response.InnerText)
                                Case "Hint"
                                    MyBase.addNote(MyBase.moXformElmt.SelectSingleNode("group"), noteTypes.Hint, response.InnerText)
                                Case "Help"
                                    MyBase.addNote(MyBase.moXformElmt.SelectSingleNode("group"), noteTypes.Help, response.InnerText)
                            End Select

                        Next
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmUserIntegrations", ex, "", processInfo, gbDebug)
                    Return Nothing
                End Try

            End Function

            Public Function xFrmWebSettings() As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim oFsh As fsHelper

                Try
                    oFsh = New fsHelper
                    oFsh.open(moPageXML)

                    MyBase.NewFrm("WebSettings")

                    MyBase.submission("WebSettings", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "WebSettings", "", "Web Settings")

                    'oSelElmt = MyBase.addSelect1(oFrmElmt, "ewDatabaseType", True, "DB Type", "", ApperanceTypes.Full)
                    'MyBase.addOption(oSelElmt, "MS SQL", "SQL")
                    'MyBase.addOption(oSelElmt, "MS Access", "Access")
                    'MyBase.addBind("ewDatabaseType", "web/add[@key='DatabaseType']/@value", "true()")

                    'MyBase.addInput(oFrmElmt, "ewDatabaseServer", True, "DB Server")
                    'MyBase.addBind("ewDatabaseServer", "web/add[@key='DatabaseServer']/@value", "true()")

                    MyBase.addNote(oFrmElmt, noteTypes.Alert, "Any Changes you make to this form risk making this site completely non-functional. Please be sure you know what you are doing before making any changes, or call your web developer for support")

                    MyBase.addInput(oFrmElmt, "ewDatabaseName", True, "DB Name")
                    MyBase.addBind("ewDatabaseName", "web/add[@key='DatabaseName']/@value", "true()")

                    'MyBase.addInput(oFrmElmt, "ewDatabaseAuth", True, "DB Auth")
                    'MyBase.addBind("ewDatabaseAuth", "web/add[@key='DatabaseAuth']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewDatabaseUsername", True, "DB Username")
                    MyBase.addBind("ewDatabaseUsername", "web/add[@key='DatabaseUsername']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewDatabasePassword", True, "DB Passowrd")
                    MyBase.addBind("ewDatabasePassword", "web/add[@key='DatabasePassword']/@value", "false()")

                    'oSelElmt = MyBase.addSelect1(oFrmElmt, "ewSiteXsl", True, "Site Scheme", "", ApperanceTypes.Minimal)
                    '' MyBase.addOptionsFilesFromDirectory(oSelElmt, "/ewcommon/xsl/scheme", ".xsl")
                    'MyBase.addOption(oSelElmt, "standard.xsl [bespoke]", "/xsl/standard.xsl")
                    'MyBase.addBind("ewSiteXsl", "web/add[@key='SiteXsl']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewRootPageId", True, "Root Page Id")
                    MyBase.addBind("ewRootPageId", "web/add[@key='RootPageId']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewBaseUrl", True, "Base URL")
                    MyBase.addBind("ewBaseUrl", "web/add[@key='BaseUrl']/@value", "false()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewDebug", True, "Debug", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewDebug", "web/add[@key='Debug']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewCompiledTransform", True, "Compiled Transform", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewCompiledTransform", "web/add[@key='CompiledTransform']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewImageRootPath", True, "Images Directory")
                    MyBase.addBind("ewImageRootPath", "web/add[@key='ImageRootPath']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewDocRootPath", True, "Docs Directory")
                    MyBase.addBind("ewDocRootPath", "web/add[@key='DocRootPath']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewMediaRootPath", True, "Media Directory")
                    MyBase.addBind("ewMediaRootPath", "web/add[@key='MediaRootPath']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewCart", True, "Shopping Cart", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewCart", "web/add[@key='Cart']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewMembership", True, "Membership", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewMembership", "web/add[@key='Membership']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewMailingList", True, "Mailing List", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewMailingList", "web/add[@key='MailingList']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewSubscriptions", True, "Subscriptions", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewSubscriptions", "web/add[@key='Subscriptions']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewActivityLogging", True, "Activity Logging", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewActivityLogging", "web/add[@key='ActivityLogging']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewIPLogging", True, "IP Tracking", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewIPLogging", "web/add[@key='IPLogging']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewMailserver", True, "Mailserver")
                    MyBase.addBind("ewMailserver", "web/add[@key='MailServer']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewSiteAdminEmail", True, "Webmaster Email")
                    MyBase.addBind("ewSiteAdminEmail", "web/add[@key='SiteAdminEmail']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewSearch", True, "Content Search", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewSearch", "web/add[@key='ContentSearch']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewSiteSearch", True, "Index Search", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewSiteSearch", "web/add[@key='SiteSearch']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewSiteSearchPath", True, "Index Path")
                    MyBase.addBind("ewSiteSearchPath", "web/add[@key='SiteSearchPath']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewGoogleContentTypes", True, "Google Sitemap Content Types")
                    MyBase.addBind("ewGoogleContentTypes", "web/add[@key='GoogleContentTypes']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewShowRelatedBriefContentTypes", True, "Include Related Content for these Content Types")
                    MyBase.addBind("ewShowRelatedBriefContentTypes", "web/add[@key='ShowRelatedBriefContentTypes']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewShowRelatedBriefDepth", True, "Depth to get related content for brief content.")
                    MyBase.addBind("ewShowRelatedBriefDepth", "web/add[@key='ShowRelatedBriefDepth']/@value", "false()", "number")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewVersionControl", True, "Version Control", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewVersionControl", "web/add[@key='VersionControl']/@value", "false()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewLegacyRedirect", True, "Legacy URL Forwarding", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewLegacyRedirect", "web/add[@key='LegacyRedirect']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewPageURLFormat", True, "Page URL Format for Spaces", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Use Hyphens", "hyphens")
                    MyBase.addOption(oSelElmt, "No preference", "off")
                    MyBase.addBind("ewPageURLFormat", "web/add[@key='PageURLFormat']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewAllowContentDetailAccess", True, "Always allow access to content detail regardless of location and permissions", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewAllowContentDetailAccess", "web/add[@key='AllowContentDetailAccess']/@value", "true()")


                    MyBase.addSubmit(oFrmElmt, "", "Save Settings")

                    Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/" & myWeb.moConfig("ProjectPath"))
                    Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("protean/web")
                    Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                    If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then

                        MyBase.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml

                        'code here to replace any missing nodes
                        'all of the required config settings
                        Dim aSettingValues() As String = Split("DatabaseType,DatabaseName,DatabaseAuth,DatabaseUsername,DatabasePassword,MailServer,RootPageId,BaseUrl,SiteXsl,ImageRootPath,DocRootPath,MediaRootPath,Membership,MailingList,NonAuthenticatedUsersGroupId,AuthenticatedUsersGroupId,RegisterBehaviour,RegisterRedirectPageId,Cart,Quote,Debug,CompiledTransform,SiteAdminName,SiteAdminEmail,ContentSearch,SiteSearch,SiteSearchPath,Subscriptions,ActivityLogging,IPLogging,GoogleContentTypes,ShowRelatedBriefContentTypes,ShowRelatedBriefDepth,VersionControl,LegacyRedirect,PageURLFormat,AllowContentDetailAccess", ",")

                        Dim i As Long
                        Dim oElmt As XmlElement
                        Dim oElmtAft As XmlElement = Nothing

                        For i = 0 To UBound(aSettingValues)
                            oElmt = MyBase.Instance.SelectSingleNode("web/add[@key='" & aSettingValues(i) & "']")
                            If oElmt Is Nothing Then
                                oElmt = moPageXML.CreateElement("add")
                                oElmt.SetAttribute("key", aSettingValues(i))
                                oElmt.SetAttribute("value", "")
                                If oElmtAft Is Nothing Then
                                    MyBase.Instance.FirstChild.InsertBefore(oElmt, MyBase.Instance.FirstChild.FirstChild)
                                Else
                                    MyBase.Instance.FirstChild.InsertAfter(oElmt, oElmtAft)
                                End If
                            End If
                            oElmtAft = oElmt
                        Next

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            If MyBase.valid Then
                                oCgfSect.SectionInformation.RestartOnExternalChanges = False
                                oCgfSect.SectionInformation.SetRawXml(MyBase.Instance.InnerXml)
                                oCfg.Save()
                            End If
                        End If

                        oImp.UndoImpersonation()
                    End If
                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmWebSettings", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmWebConfig(ByVal ConfigType As String) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim oFsh As fsHelper
                Dim xFormPath As String = "/xforms/config/" & ConfigType & ".xml"
                Try
                    If myWeb.mcEWCommonFolder = "/ptn" Then
                        xFormPath = "/admin/xforms/config/" & ConfigType & ".xml"
                    End If

                    oFsh = New fsHelper
                    oFsh.open(moPageXML)

                    MyBase.NewFrm("WebSettings")

                    If Not MyBase.load(xFormPath, myWeb.maCommonFolders) Then

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Config", "", "ConfigSettings")
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, xFormPath & " could not be found. - ")

                    Else

                        Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/" & myWeb.moConfig("ProjectPath"))

                        Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                        If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then

                            'code here to replace any missing nodes
                            'all of the required config settings

                            Dim oTemplateInstance As XmlElement = moPageXML.CreateElement("Instance")
                            oTemplateInstance.InnerXml = MyBase.Instance.InnerXml
                            Dim oCgfSectName As String = oTemplateInstance.FirstChild.Name
                            Dim oCgfSectPath As String = "protean/" & oCgfSectName
                            Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection(oCgfSectPath)
                            cProcessInfo = "Getting Section Name:" & oCgfSectPath
                            Dim sectionMissing As Boolean = False

                            'Get the current settings
                            If oCgfSect.SectionInformation.GetRawXml <> "" Then
                                MyBase.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml
                            Else
                                'no current settings create them
                                sectionMissing = True
                                oFrmElmt = MyBase.moXformElmt
                                MyBase.addNote(oFrmElmt, noteTypes.Alert, "This config section has not yet been setup, saving will implement these settings for the first time and then log you off the admin system.")
                            End If

                            Dim oTemplateElmt As XmlElement
                            Dim oElmt As XmlElement
                            Dim Key As String
                            Dim ConfigSectionName As String
                            Dim ConfigSection As System.Collections.Specialized.NameValueCollection

                            For Each oTemplateElmt In oTemplateInstance.SelectNodes("*/add")
                                ConfigSectionName = oTemplateElmt.ParentNode.Name
                                ConfigSection = WebConfigurationManager.GetWebApplicationSection("protean/" & ConfigSectionName)
                                Key = oTemplateElmt.GetAttribute("key")

                                oElmt = MyBase.Instance.SelectSingleNode(ConfigSectionName & "/add[@key='" & Key & "']")
                                'lets not write an empty value if inherited from machine level web.config
                                If Not (ConfigSection(Key) <> "" And oTemplateElmt.GetAttribute("value") = "") Then
                                    If oElmt Is Nothing Then
                                        oElmt = moPageXML.CreateElement("add")
                                        oElmt.SetAttribute("key", Key)
                                        oElmt.SetAttribute("value", oTemplateElmt.GetAttribute("value"))
                                        MyBase.Instance.SelectSingleNode(ConfigSectionName).AppendChild(oElmt)
                                    End If
                                End If
                            Next

                            If MyBase.isSubmitted Then

                                MyBase.updateInstanceFromRequest()
                                MyBase.validate()
                                If MyBase.valid Then
                                    If sectionMissing Then
                                        'update config based on form submission
                                        Dim oNewCfgXml As New XmlDocument
                                        oNewCfgXml.LoadXml(MyBase.Instance.InnerXml)
                                        'save as web.config in the root
                                        oNewCfgXml.Save(goServer.MapPath("\" & Replace(oCgfSectPath, "/", ".") & ".config"))
                                        Dim oMainCfgXml As New XmlDocument
                                        'update the the web.config to include new file
                                        cProcessInfo = "loading file:" & goServer.MapPath("/web.config")
                                        oMainCfgXml.Load(goServer.MapPath("/web.config"))
                                        Dim oElmtEonic As XmlElement = oMainCfgXml.SelectSingleNode("configuration/protean")
                                        Dim oNewElmt As XmlElement = oMainCfgXml.CreateElement(oCgfSectName)
                                        oNewElmt.SetAttribute("configSource", Replace(oCgfSectPath, "/", ".") & ".config")
                                        oElmtEonic.AppendChild(oNewElmt)
                                        oMainCfgXml.Save(goServer.MapPath("/web.config"))
                                        myWeb.msRedirectOnEnd = "/"
                                    Else
                                        'check not read only
                                        Dim oFileInfo As IO.FileInfo = New IO.FileInfo(goServer.MapPath("\" & Replace(oCgfSectPath, "/", ".") & ".config"))
                                        oFileInfo.IsReadOnly = False

                                        oCgfSect.SectionInformation.RestartOnExternalChanges = False
                                        oCgfSect.SectionInformation.SetRawXml(MyBase.Instance.InnerXml)
                                        oCfg.Save()
                                        MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, "Settings Saved")
                                    End If

                                End If
                            End If

                            oImp.UndoImpersonation()
                        End If
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmWebConfig", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function



            Public Function xFrmRewriteMaps(ByVal ConfigType As String) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim oFsh As fsHelper
                Dim xFormPath As String = "/xforms/config/" & ConfigType & ".xml"
                Try
                    oFsh = New fsHelper
                    oFsh.open(moPageXML)

                    MyBase.NewFrm("WebSettings")
                    MyBase.bProcessRepeats = False

                    If Not MyBase.load(xFormPath, myWeb.maCommonFolders) Then

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Config", "", "ConfigSettings")
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, xFormPath & " could not be found. - ")

                    Else

                        Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")

                        Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                        If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then

                            'code here to replace any missing nodes
                            'all of the required config settings

                            Dim rewriteXml As New XmlDocument

                            rewriteXml.Load(goServer.MapPath("/rewriteMaps.config"))


                            Dim oTemplateInstance As XmlElement = moPageXML.CreateElement("Instance")
                            oTemplateInstance.InnerXml = MyBase.Instance.InnerXml
                            Dim oCgfSectName As String = "system.webServer"
                            Dim oCgfSectPath As String = "rewriteMaps/rewriteMap[@name='" & ConfigType & "']"
                            ' Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection(oCgfSectName)
                            cProcessInfo = "Getting Section Name:" & oCgfSectPath
                            Dim sectionMissing As Boolean = False

                            'Get the current settings
                            If Not rewriteXml.SelectSingleNode(oCgfSectPath) Is Nothing Then
                                MyBase.bProcessRepeats = True
                                If goSession("oTempInstance") Is Nothing Then


                                    Dim PerPageCount As Integer = 50
                                    If goSession("totalCountTobeLoad") IsNot Nothing Then
                                        PerPageCount = goSession("totalCountTobeLoad")
                                    End If
                                    Dim props As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPath)
                                    Dim TotalCount As Integer = props.ChildNodes.Count

                                    If props.ChildNodes.Count >= PerPageCount Then
                                        Dim xmlstring As String = "<rewriteMap name='" & ConfigType & "'>"
                                        Dim xmlstringend As String = "</rewriteMap>"
                                        Dim count As Integer = 0

                                        For i As Integer = 0 To (PerPageCount) - 1
                                            xmlstring = xmlstring & props.ChildNodes(i).OuterXml
                                        Next

                                        MyBase.LoadInstanceFromInnerXml(xmlstring & xmlstringend)
                                    Else
                                        MyBase.LoadInstanceFromInnerXml(rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml)
                                    End If

                                    Me.bProcessRepeats = False
                                Else
                                    Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")
                                    oTempInstance = goSession("oTempInstance")
                                    MyBase.updateInstance(oTempInstance)
                                End If
                            Else
                                'no current settings create them
                                sectionMissing = True
                                oFrmElmt = MyBase.moXformElmt
                                MyBase.addNote(oFrmElmt, noteTypes.Alert, "This config section has not yet been setup, saving will implement these settings for the first time and then log you off the admin system.")
                            End If


                            Dim oElmt As XmlElement


                            If MyBase.isSubmitted Then

                                MyBase.updateInstanceFromRequest()
                                MyBase.validate()
                                'Check for loop
                                For Each oElmt In MyBase.Instance.FirstChild.SelectNodes("descendant-or-self::add")
                                    Dim newURL = oElmt.GetAttribute("value")
                                    If Not MyBase.Instance.FirstChild.SelectSingleNode("descendant-or-self::add[@key='" & newURL & "']") Is Nothing Then
                                        MyBase.valid = False
                                        Dim alertGrp As XmlElement = MyBase.addGroup(moXformElmt.SelectSingleNode("group[1]"), "alert",,, moXformElmt.SelectSingleNode("group[1]/group[1]"))
                                        MyBase.addNote(alertGrp, xForm.noteTypes.Alert, "<strong>" & newURL & "</strong> cannot match an old URL")
                                    End If
                                Next


                                If MyBase.valid Then
                                    Dim replacerNode As XmlElement = rewriteXml.ImportNode(MyBase.Instance.FirstChild, True)
                                    Dim folderRules As New ArrayList
                                    If ConfigType = "301Redirect" Then
                                        'step through and create rules to deal with paths
                                        Dim rulesXml As New XmlDocument
                                        rulesXml.Load(myWeb.goServer.MapPath("/RewriteRules.config"))
                                        Dim insertAfterElment As XmlElement = rulesXml.SelectSingleNode("descendant-or-self::rule[@name='EW: 301 Redirects']")
                                        Dim oRule As XmlElement
                                        For Each oRule In replacerNode.SelectNodes("add")
                                            Dim CurrentRule As XmlElement = rulesXml.SelectSingleNode("descendant-or-self::rule[@name='Folder: " & oRule.GetAttribute("key") & "']")
                                            Dim newRule As XmlElement = rulesXml.CreateElement("newRule")
                                            Dim matchString As String = oRule.GetAttribute("key")
                                            If matchString.StartsWith("/") Then
                                                matchString = matchString.TrimStart("/")
                                            End If
                                            folderRules.Add("Folder: " & oRule.GetAttribute("key"))
                                            newRule.InnerXml = "<rule name=""Folder: " & oRule.GetAttribute("key") & """><match url=""^" & matchString & "(.*)""/><action type=""Redirect"" url=""" & oRule.GetAttribute("value") & "{R:1}"" /></rule>"
                                            If CurrentRule Is Nothing Then
                                                insertAfterElment.ParentNode.InsertAfter(newRule.FirstChild, insertAfterElment)
                                            Else
                                                CurrentRule.ParentNode.ReplaceChild(newRule.FirstChild, CurrentRule)
                                            End If
                                        Next

                                        For Each oRule In rulesXml.SelectNodes("descendant-or-self::rule[starts-with(@name,'Folder: ')]")
                                            If Not folderRules.Contains(oRule.GetAttribute("name")) Then
                                                oRule.ParentNode.RemoveChild(oRule)
                                            End If
                                        Next

                                        rulesXml.Save(goServer.MapPath("/RewriteRules.config"))
                                        myWeb.bRestartApp = True

                                    End If

                                    'Dim replacingNode As XmlElement = rewriteXml.SelectSingleNode(oCgfSectPath)
                                    'If replacingNode Is Nothing Then
                                    '    rewriteXml.FirstChild.AppendChild(replacerNode)
                                    'Else
                                    '    rewriteXml.FirstChild.ReplaceChild(replacerNode, replacingNode)
                                    'End If
                                    'rewriteXml.Save(goServer.MapPath("/rewriteMaps.config"))

                                    ''Check we do not have a redirect for the OLD URL allready. Remove if exists
                                    ' Dim addValue As XmlElement


                                    For Each oElmt In MyBase.Instance.FirstChild.SelectNodes("descendant-or-self::add")
                                        Dim oldUrl = oElmt.GetAttribute("key")

                                        'If Not MyBase.Instance.FirstChild.SelectSingleNode("descendant-or-self::add[@key='" & newURL & "']") Is Nothing Then
                                        Dim existingRedirects As XmlNodeList = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & ConfigType & "']/add[@key='" & oldUrl & "']")
                                        If Not existingRedirects Is Nothing Then

                                            For Each existingNode As XmlNode In existingRedirects
                                                existingNode.ParentNode.RemoveChild(existingNode)
                                                'existingNode.RemoveAll()
                                                rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                                            Next
                                        End If
                                    Next

                                    'Add redirect
                                    Dim oCgfSectPathobj As String = "rewriteMaps/rewriteMap[@name='" & ConfigType & "']"
                                    Dim redirectSectionXmlNode As XmlNode = rewriteXml.SelectSingleNode(oCgfSectPathobj)
                                    If Not redirectSectionXmlNode Is Nothing Then
                                        For Each oElmt In MyBase.Instance.FirstChild.SelectNodes("descendant-or-self::add")
                                            Dim replacingElement As XmlElement = rewriteXml.CreateElement("RedirectInfo")
                                            replacingElement.InnerXml = oElmt.OuterXml

                                            ' rewriteXml.SelectSingleNode(oCgfSectPath).FirstChild.AppendChild(replacingElement.FirstChild)
                                            rewriteXml.SelectSingleNode(oCgfSectPathobj).AppendChild(replacingElement.FirstChild)

                                            rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"))
                                        Next
                                    End If






                                    Dim alertGrp As XmlElement = MyBase.addGroup(moXformElmt.SelectSingleNode("group[1]"), "alert",,, moXformElmt.SelectSingleNode("group[1]/group[1]"))
                                    MyBase.addNote(alertGrp, xForm.noteTypes.Alert, "Settings Saved")
                                    goSession("oTempInstance") = Nothing
                                End If
                            ElseIf MyBase.isTriggered Then
                                'we have clicked a trigger so we must update the instance
                                MyBase.updateInstanceFromRequest()
                                'lets save the instance
                                goSession("oTempInstance") = MyBase.Instance
                            Else
                                'clear this if we are loading the first form
                                goSession("oTempInstance") = Nothing
                            End If


                            oImp.UndoImpersonation()
                        End If
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmWebConfig", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmThemeSettings(ByVal ConfigType As String) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim oFsh As fsHelper
                Dim xFormPath As String = "/xforms/config/" & ConfigType & ".xml"
                Try
                    oFsh = New fsHelper
                    oFsh.open(moPageXML)

                    Dim moThemeConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/theme")
                    Dim currentTheme As String = moThemeConfig("CurrentTheme")

                    MyBase.NewFrm("WebSettings")

                    If Not MyBase.load(xFormPath, myWeb.maCommonFolders) Then

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Config", "", "ConfigSettings")
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, xFormPath & " could not be found. - ")

                    Else

                        Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")

                        Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                        If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then

                            'code here to replace any missing nodes
                            'all of the required config settings

                            Dim oTemplateInstance As XmlElement = moPageXML.CreateElement("Instance")
                            oTemplateInstance.InnerXml = MyBase.Instance.InnerXml
                            Dim oCgfSectName As String = "protean/" & oTemplateInstance.FirstChild.Name
                            Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection(oCgfSectName)
                            cProcessInfo = "Getting Section Name:" & oCgfSectName
                            'Get the current settings
                            MyBase.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml



                            Dim currentPresetName As String = ""
                            Dim presetSetting As XmlElement = MyBase.Instance.SelectSingleNode("theme/add[@key='" & currentTheme & ".ThemePreset']")
                            If Not presetSetting Is Nothing Then
                                currentPresetName = presetSetting.GetAttribute("value")
                            End If

                            If myWeb.moRequest("ThemePreset") <> currentPresetName Or currentPresetName = "" Then
                                'replace Instance Elements WITH VALUES IN NAMED THEME PRESET FILE.

                                If IO.File.Exists(goServer.MapPath("/ewthemes/" & currentTheme & "/themeManifest.xml")) Then



                                    Dim newXml As New XmlDocument
                                    newXml.PreserveWhitespace = True
                                    newXml.Load(goServer.MapPath("/ewthemes/" & currentTheme & "/themeManifest.xml"))
                                    Dim oElmt2 As XmlElement
                                    For Each oElmt2 In newXml.SelectNodes("/Theme/Presets/Preset[@name='" & myWeb.moRequest("ThemePreset") & "']/add")
                                        '<add key="Bootswatch.Layout" value="TopNavSideSub"/>
                                        Dim changeElmt As XmlElement = MyBase.Instance.SelectSingleNode("descendant-or-self::add[@key='" & oElmt2.GetAttribute("key") & "']")
                                        If Not changeElmt Is Nothing Then
                                            changeElmt.SetAttribute("value", oElmt2.GetAttribute("value"))
                                        End If
                                    Next
                                End If
                            End If

                            Dim oTemplateElmt As XmlElement
                            Dim oElmt As XmlElement
                            Dim Key As String
                            Dim ConfigSectionName As String
                            Dim ConfigSection As System.Collections.Specialized.NameValueCollection

                            For Each oTemplateElmt In oTemplateInstance.SelectNodes("*/add")
                                ConfigSectionName = oTemplateElmt.ParentNode.Name
                                ConfigSection = WebConfigurationManager.GetWebApplicationSection("protean/" & ConfigSectionName)
                                Key = oTemplateElmt.GetAttribute("key")

                                oElmt = MyBase.Instance.SelectSingleNode(ConfigSectionName & "/add[@key='" & Key & "']")
                                'lets not write an empty value if inherited from machine level web.config
                                If Not (ConfigSection(Key) <> "" And oTemplateElmt.GetAttribute("value") = "") Then
                                    If oElmt Is Nothing Then
                                        oElmt = moPageXML.CreateElement("add")
                                        oElmt.SetAttribute("key", Key)
                                        oElmt.SetAttribute("value", oTemplateElmt.GetAttribute("value"))
                                        MyBase.Instance.SelectSingleNode(ConfigSectionName).AppendChild(oElmt)
                                    End If
                                End If
                            Next

                            If MyBase.isSubmitted Then

                                'If myWeb.moRequest("ThemePreset") <> currentPresetName And Not (myWeb.moSession("presetView") = "true") Then
                                '    MyBase.valid = False
                                '    myWeb.moSession("presetView") = "true"
                                'Else
                                '    myWeb.moSession("presetView") = Nothing
                                '    MyBase.updateInstanceFromRequest()
                                '    MyBase.validate()
                                'End If

                                MyBase.updateInstanceFromRequest()
                                MyBase.validate()


                                If MyBase.valid Then

                                    'check not read only
                                    Dim oFileInfo As IO.FileInfo = New IO.FileInfo(goServer.MapPath("/protean.theme.config"))
                                    oFileInfo.IsReadOnly = False

                                    oCgfSect.SectionInformation.RestartOnExternalChanges = False
                                    oCgfSect.SectionInformation.SetRawXml(MyBase.Instance.InnerXml)
                                    oCfg.Save()

                                    If myWeb.moRequest("newPresetName") <> "" Then

                                        If IO.File.Exists(goServer.MapPath("/ewthemes/" & currentTheme & "/themeManifest.xml")) Then
                                            Dim newXml As New XmlDocument
                                            newXml.PreserveWhitespace = True
                                            newXml.Load(goServer.MapPath("/ewthemes/" & currentTheme & "/themeManifest.xml"))
                                            Dim oElmt2 As XmlElement
                                            Dim addNew As Boolean = True
                                            'update existing
                                            For Each oElmt2 In newXml.SelectNodes("/Theme/Presets/Preset[@name='" & myWeb.moRequest("newPresetName") & "']")
                                                oElmt2.InnerXml = Instance.InnerXml
                                                addNew = False
                                            Next
                                            If addNew Then
                                                Dim PresetsNode As XmlElement = newXml.SelectSingleNode("/Theme/Presets")
                                                Dim NewPreset As XmlElement = PresetsNode.OwnerDocument.CreateElement("Preset")
                                                NewPreset.SetAttribute("name", myWeb.moRequest("newPresetName"))
                                                Dim matchingElmt As XmlElement
                                                For Each matchingElmt In Instance.SelectNodes("descendant-or-self::add[starts-with(@key,'" & currentTheme & ".')]")
                                                    If matchingElmt.GetAttribute("key") = currentTheme & ".ThemePreset" Then
                                                        matchingElmt.SetAttribute("value", myWeb.moRequest("newPresetName"))
                                                    End If
                                                    Protean.Tools.Xml.AddExistingNode(NewPreset, matchingElmt)
                                                Next
                                                PresetsNode.AppendChild(NewPreset)
                                            End If
                                            'check not read only
                                            Dim oFileInfo2 As IO.FileInfo = New IO.FileInfo(goServer.MapPath("/ewthemes/" & currentTheme & "/themeManifest.xml"))
                                            oFileInfo2.IsReadOnly = False
                                            newXml.Save(goServer.MapPath("/ewthemes/" & currentTheme & "/themeManifest.xml"))
                                        End If

                                        MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, "New Preset Saved")
                                    Else
                                        MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, "Settings Saved")
                                    End If
                                Else
                                    MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, "Form Invalid:" & MyBase.validationError)

                                End If
                            End If

                            oImp.UndoImpersonation()
                        End If
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmWebConfig", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmSelectTheme() As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim oFsh As fsHelper

                Try
                    oFsh = New fsHelper
                    oFsh.open(moPageXML)

                    MyBase.NewFrm("WebSettings")
                    'MyBase.Instance.InnerXml = "<web><add key=""SiteXsl"" value="""" /></web><theme><add key=""CurrentTheme"" value="""" /></theme>"
                    MyBase.submission("WebSettings", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "WebSettings", "", "Select Theme")

                    Dim rootdir As New DirectoryInfo(goServer.MapPath("/ewThemes"))
                    If Not rootdir.Exists Then
                        MyBase.addNote(oFrmElmt, noteTypes.Alert, "This site is not configured to allow new themes to be selected.")
                    Else
                        MyBase.addNote(oFrmElmt, noteTypes.Alert, "Any Changes you make to this form risk making this site non-functional. Please be sure you know what you are doing before making any changes.")

                        oSelElmt = MyBase.addSelect1(oFrmElmt, "ewSiteTheme", True, "Site Theme", "PickByImage", ApperanceTypes.Full)
                        EnumberateThemeOptions(oSelElmt, "/ewThemes", ".xsl", "", True)
                        ' MyBase.addOption(oSelElmt, "standard.xsl [bespoke]", "/xsl/standard.xsl")
                        MyBase.addBind("ewSiteTheme", "theme/add[@key='CurrentTheme']/@value", "false()")

                        '    MyBase.addSubmit(oFrmElmt, "", "Save Settings")

                        Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")

                        Dim oWebCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("protean/web")
                        Dim oThemeCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("protean/theme")

                        Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                        If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then

                            MyBase.Instance.InnerXml = oWebCgfSect.SectionInformation.GetRawXml & oThemeCgfSect.SectionInformation.GetRawXml

                            Dim oTemplateInstance As XmlElement = moPageXML.CreateElement("Instance")
                            oTemplateInstance.InnerXml = MyBase.Instance.InnerXml

                            If MyBase.isSubmitted Or goRequest.Form("ewsubmit.x") <> "" Or goRequest.Form("ewSiteTheme") <> "" Then

                                Dim oTemplateElmt As XmlElement
                                Dim oElmt As XmlElement
                                Dim Key As String
                                Dim ConfigSectionName As String
                                Dim ConfigSection As System.Collections.Specialized.NameValueCollection

                                For Each oTemplateElmt In oTemplateInstance.SelectNodes("*/add")
                                    ConfigSectionName = oTemplateElmt.ParentNode.Name
                                    ConfigSection = WebConfigurationManager.GetWebApplicationSection("protean/" & ConfigSectionName)
                                    Key = oTemplateElmt.GetAttribute("key")

                                    oElmt = MyBase.Instance.SelectSingleNode(ConfigSectionName & "/add[@key='" & Key & "']")
                                    'lets not write an empty value if inherited from machine level web.config
                                    If Not (ConfigSection(Key) <> "" And oTemplateElmt.GetAttribute("value") = "") Then
                                        If oElmt Is Nothing Then
                                            oElmt = moPageXML.CreateElement("add")
                                            oElmt.SetAttribute("key", Key)
                                            oElmt.SetAttribute("value", oTemplateElmt.GetAttribute("value"))
                                            MyBase.Instance.SelectSingleNode(ConfigSectionName).AppendChild(oElmt)
                                        End If
                                    End If
                                Next

                                MyBase.updateInstanceFromRequest()
                                MyBase.validate()
                                If MyBase.valid Then

                                    Dim updElmt As XmlElement = MyBase.Instance.SelectSingleNode("web/add[@key='SiteXsl']")
                                    updElmt.SetAttribute("value", "/ewthemes/" & moRequest("ewSiteTheme") & "/standard.xsl")
                                    Dim cssFramework As String = ""
                                    If Not oSelElmt.SelectSingleNode("descendant-or-self::Theme[@name='" & moRequest("ewSiteTheme") & "' and @cssFramework!='']") Is Nothing Then
                                        Dim themeElmt As XmlElement = oSelElmt.SelectSingleNode("descendant-or-self::Theme[@name='" & moRequest("ewSiteTheme") & "' and @cssFramework!='']")
                                        cssFramework = themeElmt.GetAttribute("cssFramework")
                                    End If
                                    Dim cssElmt As XmlElement = MyBase.Instance.SelectSingleNode("web/add[@key='cssFramework']")
                                    If cssElmt Is Nothing Then
                                        oElmt = moPageXML.CreateElement("add")
                                        oElmt.SetAttribute("key", "cssFramework")
                                        oElmt.SetAttribute("value", cssFramework)
                                        MyBase.Instance.SelectSingleNode("web").AppendChild(oElmt)
                                    Else
                                        cssElmt.SetAttribute("value", cssFramework)
                                    End If
                                    oWebCgfSect.SectionInformation.RestartOnExternalChanges = False
                                    oWebCgfSect.SectionInformation.SetRawXml(MyBase.Instance.SelectSingleNode("web").OuterXml)
                                    oThemeCgfSect.SectionInformation.RestartOnExternalChanges = False
                                    oThemeCgfSect.SectionInformation.SetRawXml(MyBase.Instance.SelectSingleNode("theme").OuterXml)

                                    oCfg.Save()
                                    MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, "Settings Saved")
                                End If
                            End If

                            oImp.UndoImpersonation()
                        End If
                    End If


                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmSelectTheme", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Private Sub EnumberateThemeOptions(ByRef oSelectElmt As XmlElement, ByVal filepath As String, ByVal groupName As String, ByVal optionName As String, ByVal bIgnoreIfNotFound As Boolean)

                Dim oXformDoc As XmlDocument = New XmlDocument
                Dim cProcessInfo As String = ""
                Dim sImgPath As String = ""
                Dim oChoices As XmlElement
                Dim oOptElmt As XmlElement

                Try
                    If filepath = "" Then filepath = "/"

                    'for each folder found in ewskins

                    Dim rootdir As New DirectoryInfo(goServer.MapPath(filepath))
                    Dim dir As DirectoryInfo() = rootdir.GetDirectories
                    Dim di As DirectoryInfo

                    Dim files As FileInfo()
                    Dim fi As FileInfo
                    Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelectElmt, "Installed Themes")

                    For Each di In dir
                        files = di.GetFiles("themeManifest.xml")
                        For Each fi In files
                            cProcessInfo = "loading File:" & goServer.MapPath(filepath) & "\" & di.Name & "\" & fi.Name
                            oXformDoc.Load(goServer.MapPath(filepath) & "\" & di.Name & "\" & fi.Name)
                            For Each oChoices In oXformDoc.SelectNodes("/Theme")
                                Dim RootXsltElmt As XmlElement = oChoices.SelectSingleNode("/Theme/RootXslt")
                                If Not RootXsltElmt Is Nothing Then
                                    oOptElmt = MyBase.addOption(oChoicesElmt, oChoices.GetAttribute("name"), RootXsltElmt.GetAttribute("src"))
                                    oOptElmt.FirstChild.InnerXml = oChoices.OuterXml
                                End If
                            Next
                        Next
                    Next

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "EnumberateThemeOptions", ex, "", cProcessInfo, gbDebug)
                End Try

            End Sub

            Public Function xFrmEditPage(Optional ByVal pgid As Long = 0, Optional ByVal cName As String = "", Optional ByVal cFormName As String = "Page", Optional ByVal cParId As String = "") As XmlElement
                Dim cXmlFilePath As String
                Dim oFrmElmt As XmlElement = Nothing
                Dim oSelElmt As XmlElement
                Dim cProcessInfo As String = ""

                Dim oObjType As New dbHelper.objectTypes
                Dim oTblName As dbHelper.TableNames
                Dim nRContentId As Integer = 0

                Try

                    cXmlFilePath = "/xforms/page/" & cFormName & ".xml"
                    If goConfig("cssFramework") = "bs5" Then
                        cXmlFilePath = "/admin" & cXmlFilePath
                    End If
                    If Not MyBase.load(cXmlFilePath, myWeb.maCommonFolders) Then
                        'If not a custom page is loaded, pull in the standard elements
                        MyBase.NewFrm("EditPage")
                        ' MyBase.submission("EditEage", "admin.aspx?ewCmd=EditPage&pgid=46&xml=x", "post")

                        MyBase.submission("EditEage", "", "post", "form_check(this)")

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditPage", "", "Edit Page")
                        MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                        MyBase.addBind("nStructParId", "tblContentStructure/nStructParId", "true()")

                        MyBase.addInput(oFrmElmt, "cStructName", True, "Page Name", IIf(cName = "", "", "readonly"))
                        MyBase.addBind("cStructName", "tblContentStructure/cStructName", "true()")

                        MyBase.addInput(oFrmElmt, "cDisplayName", True, "Display Name")
                        MyBase.addBind("cDisplayName", "tblContentStructure/cStructDescription/DisplayName", "false()")

                        MyBase.addTextArea(oFrmElmt, "cStructDescription", True, "Description", "xhtml", 10)
                        MyBase.addBind("cStructDescription", "tblContentStructure/cStructDescription/Description", "false()")

                        If LCase(myWeb.moConfig("ShowStructForiegnRef")) = "yes" Or LCase(myWeb.moConfig("ShowStructForiegnRef")) = "on" Then
                            MyBase.addInput(oFrmElmt, "cStructForiegnRef", True, "Foriegn Reference")
                            MyBase.addBind("cStructForiegnRef", "tblContentStructure/cStructForiegnRef", "false()")
                        End If

                        If Not myWeb.goLangConfig Is Nothing Then
                            oSelElmt = MyBase.addSelect1(oFrmElmt, "cLang", True, "Language", "", ApperanceTypes.Full)
                            MyBase.addOption(oSelElmt, myWeb.goLangConfig.GetAttribute("default"), myWeb.goLangConfig.GetAttribute("code"))
                            Dim langNode As XmlElement
                            For Each langNode In myWeb.goLangConfig.SelectNodes("Language")
                                MyBase.addOption(oSelElmt, langNode.GetAttribute("systemName"), langNode.GetAttribute("code"))
                            Next
                            MyBase.addBind("cLang", "tblContentStructure/cVersionLang", "tblContentStructure/nVersionType='3'")
                        End If

                        MyBase.addInput(oFrmElmt, "thumbnail", True, "Thumbnail Image", "short pickImage")
                        MyBase.addBind("thumbnail", "tblContentStructure/cStructDescription/Images/img[@class='thumbnail']", "false()", "xml-replace")

                        MyBase.addInput(oFrmElmt, "cUrl", True, "URL")
                        MyBase.addBind("cUrl", "tblContentStructure/cUrl", "false()")

                        MyBase.addInput(oFrmElmt, "dPublishDate", True, "Publish Date", "calendar")
                        MyBase.addBind("dPublishDate", "tblContentStructure/dPublishDate", "false()")

                        MyBase.addInput(oFrmElmt, "dExpireDate", True, "Expire Date", "calendar")
                        MyBase.addBind("dExpireDate", "tblContentStructure/dExpireDate", "false()")

                        oSelElmt = MyBase.addSelect1(oFrmElmt, "nStatus", True, "Status", "", ApperanceTypes.Full)
                        MyBase.addOption(oSelElmt, "Live", 1)
                        MyBase.addOption(oSelElmt, "Hidden", 0)
                        MyBase.addBind("nStatus", "tblContentStructure/nStatus", "true()")

                        MyBase.addInput(oFrmElmt, "cDescription", True, "Change Notes")
                        MyBase.addBind("cDescription", "tblContentStructure/cDescription", "false()")

                        MyBase.addSubmit(oFrmElmt, "", "Save Page")

                    End If

                    'As this is only needed for Related Content Pages
                    If Tools.Xml.NodeState(MyBase.Instance, "tblContentStructure/RelatedContent") = XmlNodeState.HasContents Then
                        For Each oTblName In [Enum].GetValues(GetType(dbHelper.objectTypes))
                            If oTblName.ToString = "tblContent" Then Exit For
                        Next
                        oObjType = oTblName
                    End If

                    If pgid > 0 Then
                        If Tools.Xml.NodeState(MyBase.Instance, "tblContentStructure/RelatedContent") = XmlNodeState.HasContents Then
                            Dim oContent As XmlNode
                            Dim oDr As SqlDataReader

                            'For Each oContent In MyBase.Instance.SelectNodes("tblContentStructure/RelatedContent/tblContent")
                            Dim sSql As String = "Select nContentKey from tblContent c Inner Join tblContentLocation cl on c.nContentKey = cl.nContentId Where cl.nStructId = '" & pgid & "' AND c.cContentName = '" & cFormName & "_RelatedContent'"
                            oDr = moDbHelper.getDataReader(sSql)

                            'nRContentId = 0
                            While oDr.Read
                                nRContentId = oDr(0)
                            End While
                            oDr.Close()

                            If nRContentId > 0 Then
                                oContent = MyBase.Instance.SelectSingleNode("tblContentStructure/RelatedContent")
                                oContent.InnerXml = moDbHelper.getObjectInstance(oObjType, nRContentId)
                            End If
                        End If

                    Else
                        'Set the default language

                    End If


                    If pgid > 0 Then

                        MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.ContentStructure, pgid)

                        'Set the default language if empty
                        If Not myWeb.goLangConfig Is Nothing Then
                            If Not MyBase.Instance.SelectSingleNode("tblContentStructure/cVersionLang") Is Nothing Then
                                If MyBase.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = "" Then
                                    MyBase.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = myWeb.goLangConfig.GetAttribute("code")
                                End If
                            End If
                        End If

                    ElseIf String.IsNullOrEmpty(MyBase.Instance.InnerXml) Then
                        MyBase.Instance.InnerXml = "<tblContentStructure><nStructKey/><nStructParId/><cStructForiegnRef/><cStructName/><cStructDescription><DisplayName/><Images><img class=""thumbnail""/></Images><Description/></cStructDescription><cUrl/><nStructOrder/><cStructLayout>1_Column</cStructLayout><cVersionLang/><nAuditId/>" &
                        "<nAuditKey/><dPublishDate/><dExpireDate/><dInsertDate/><nInsertDirId/><dUpdateDate/><nUpdateDirId/><nStatus>0</nStatus><cDescription></cDescription></tblContentStructure>"
                    Else
                        If Not myWeb.goLangConfig Is Nothing Then
                            MyBase.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = myWeb.goLangConfig.GetAttribute("code")
                        End If
                    End If

                    'Add the page name if passed through
                    If cName <> "" Then
                        cProcessInfo = MyBase.Instance.InnerXml
                        MyBase.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText = cName
                    End If

                    ' delete the status if we are editing the home page

                    If pgid = myWeb.moConfig("RootPageId") Then
                        Dim oStatusElmt As XmlElement
                        oStatusElmt = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::*[@bind='nStatus' or @ref='nStatus']")
                        If Not oStatusElmt Is Nothing Then
                            oStatusElmt.ParentNode.RemoveChild(oStatusElmt)
                        End If
                    End If

                    ' If the Par Id is empty then populate it
                    If cParId = "" Then
                        Tools.Xml.NodeState(MyBase.Instance, "tblContentStructure/nStructParId", IIf(MyBase.goRequest("parId") Is Nothing, "", CStr(MyBase.goRequest("parId"))))
                    Else
                        Tools.Xml.NodeState(MyBase.Instance, "tblContentStructure/nStructParId", cParId)
                    End If


                    ' Account for the clone node 
                    If gbClone Then

                        ' Check for the instance of Clone
                        If Tools.Xml.NodeState(MyBase.Instance, "tblContentStructure/nCloneStructId") = Tools.Xml.XmlNodeState.NotInstantiated Then
                            addElement(MyBase.Instance.SelectSingleNode("tblContentStructure"), "nCloneStructId")
                        End If

                        ' Check for the binding of clone
                        If Tools.Xml.NodeState(MyBase.model, "//bind[contains(@nodeset,'nCloneStructId'])") = Tools.Xml.XmlNodeState.NotInstantiated Then
                            Dim oGroup As XmlElement = MyBase.moXformElmt.SelectSingleNode("group")
                            MyBase.addInput(oGroup, "nCloneStructId", True, "Clone Page", "clonepage")
                            MyBase.addBind("nCloneStructId", "tblContentStructure/nCloneStructId", "false()")
                        End If
                    End If

                    cName = MyBase.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText
                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            PageValidation()
                        End If
                        If MyBase.valid Then

                            'TS reset system application page values
                            goApp("PageNotFoundId") = Nothing
                            goApp("PageAccessDeniedId") = Nothing
                            goApp("PageLoginRequiredId") = Nothing
                            goApp("PageLoginRequiredId") = Nothing

                            'NB Notes: Extract RelatedContent Nodes here - is this old now?

                            If pgid > 0 Then
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, MyBase.Instance)


                            Else

                                pgid = moDbHelper.insertStructure(MyBase.Instance)
                                moDbHelper.ReorderNode(dbHelper.objectTypes.ContentStructure, pgid, "MoveBottom")

                                ' If the site wants to, by default, restrict new pages to a given group or directory item, then
                                ' read this in from the config and set the permission.
                                If IsNumeric(goConfig("DefaultPagePermissionGroupId")) And goConfig("DefaultPagePermissionGroupId") > 0 Then
                                    Dim nDefaultPagePermDirId As Long = CLng(goConfig("DefaultPagePermissionGroupId"))
                                    moDbHelper.maintainPermission(pgid, nDefaultPagePermDirId, dbHelper.PermissionLevel.View)
                                End If

                                ' We need to return the page id somehow, so we could update the instance
                                Tools.Xml.NodeState(MyBase.Instance, "//nStructKey", pgid, , Tools.Xml.XmlNodeState.IsEmpty)

                            End If


                            ' Clear the cache
                            If gbSiteCacheMode Then
                                moDbHelper.ExeProcessSqlScalar("DELETE FROM dbo.tblXmlCache")
                            End If


                            'NB Notes: Get PgId above then process Related Content
                            If Tools.Xml.NodeState(MyBase.Instance, "tblContentStructure/RelatedContent") = XmlNodeState.HasContents Then
                                If pgid > 0 Then
                                    Dim oContent As XmlNode
                                    Dim oDr As SqlDataReader

                                    oContent = MyBase.Instance.SelectSingleNode("tblContentStructure/RelatedContent/tblContent")
                                    Dim sSql As String = "Select nContentKey from tblContent c Inner Join tblContentLocation cl on c.nContentKey = cl.nContentId Where cl.nStructId = '" & pgid & "' AND c.cContentName = '" & cFormName & "_RelatedContent'"
                                    oDr = moDbHelper.getDataReader(sSql)


                                    Dim oInstance As XmlDocument = New XmlDocument
                                    oInstance.AppendChild(oInstance.CreateElement("Instance"))
                                    oInstance.FirstChild.AppendChild(oInstance.ImportNode(oContent, True))

                                    nRContentId = 0
                                    While oDr.Read
                                        nRContentId = oDr(0)
                                    End While
                                    oDr.Close()


                                    If nRContentId > 0 Then
                                        nRContentId = moDbHelper.setObjectInstance(oObjType, oInstance.FirstChild, nRContentId)
                                        moDbHelper.CommitLogToDB(dbHelper.ActivityType.ContentEdited, myWeb.mnUserId, myWeb.moSession.SessionID, Now, nRContentId, pgid, "")
                                        moDbHelper.setContentLocation(pgid, nRContentId)
                                    Else
                                        nRContentId = moDbHelper.setObjectInstance(oObjType, oInstance.FirstChild)
                                        moDbHelper.CommitLogToDB(dbHelper.ActivityType.ContentAdded, myWeb.mnUserId, myWeb.moSession.SessionID, Now, nRContentId, pgid, "")
                                        moDbHelper.setContentLocation(pgid, nRContentId)
                                    End If

                                End If
                            End If
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditPage", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            ''' <summary>
            ''' Page xform validation:
            ''' <list>
            '''   <item>Checks for reserved words</item>
            '''   <item>Checks for illegal characters</item>
            ''' </list>
            ''' </summary>
            ''' <remarks>This can be overridden, but should be called by the overriding method.</remarks>
            Protected Overridable Sub PageValidation()
                Dim cProcessInfo As String = ""
                Try

                    cProcessInfo = "Check for reserved words"
                    Dim aReservedDirs As String() = Split("ewcommon,images,docs,media,css,bin,js,xforms,xsl", ",")
                    Dim i As Integer
                    For i = 0 To UBound(aReservedDirs)
                        If goRequest("cStructName") = aReservedDirs(i) Then
                            MyBase.valid = False
                            MyBase.addNote(MyBase.RootGroup, noteTypes.Alert, "<strong>" & aReservedDirs(i) & "</strong> is a reserved directory name, please use another.")
                        End If
                    Next

                    'check for illegal charactors within the page name.
                    ' update 21-May-09 : taken out - and +  as these can confuse our URLs
                    cProcessInfo = "Check for illegal characters"
                    Dim oUrlExp As Regex = New Regex("^[\w\u0020]+$")

                    If Not oUrlExp.IsMatch(goRequest("cStructName")) Then
                        MyBase.valid = False
                        MyBase.addNote("cStructName", noteTypes.Alert, "Page names are used for the URL and only contain alphanumberic characters, underscores and spaces.")
                    End If
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "PageValidation", ex, "", cProcessInfo, gbDebug)
                End Try
            End Sub

            Public Function xFrmCopyPage(ByVal pgid As Long) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim nNewPgid As Long
                Dim oElmt As XmlElement

                Try
                    MyBase.NewFrm("CopyPage")
                    ' MyBase.submission("EditEage", "admin.aspx?ewCmd=EditPage&pgid=46&xml=x", "post")

                    MyBase.submission("EditEage", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "CopyPage", "", "Copy Page")
                    MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                    MyBase.addBind("nStructParId", "tblContentStructure/nStructParId", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "nCopyType", True, "Copy", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "This page and all its decendants", 1)
                    MyBase.addOption(oSelElmt, "This page only", 0)
                    MyBase.addBind("nCopyType", "tblContentStructure/@nCopyType", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "nCopyContent", True, "Page Content", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Create empty pages", 0)
                    MyBase.addOption(oSelElmt, "Same content with multiple locations", 2)
                    MyBase.addOption(oSelElmt, "Same content with multiple primary locations", 3)
                    MyBase.addOption(oSelElmt, "Create copies of the content", 1)
                    MyBase.addBind("nCopyContent", "tblContentStructure/@nCopyContent", "true()")

                    MyBase.addInput(oFrmElmt, "cStructName", True, "Page Name")
                    MyBase.addBind("cStructName", "tblContentStructure/cStructName", "true()")

                    MyBase.addInput(oFrmElmt, "cDisplayName", True, "Display Name")
                    MyBase.addBind("cDisplayName", "tblContentStructure/cStructDescription/DisplayName", "false()")

                    MyBase.addTextArea(oFrmElmt, "cStructDescription", True, "Description", "xhtml", 10)
                    MyBase.addBind("cStructDescription", "tblContentStructure/cStructDescription/Description", "false()")

                    MyBase.addInput(oFrmElmt, "cUrl", True, "URL")
                    MyBase.addBind("cUrl", "tblContentStructure/cUrl", "false()")

                    MyBase.addInput(oFrmElmt, "dPublishDate", True, "Publish Date", "calendar")
                    MyBase.addBind("dPublishDate", "tblContentStructure/dPublishDate", "false()")

                    MyBase.addInput(oFrmElmt, "dExpireDate", True, "Expire Date", "calendar")
                    MyBase.addBind("dExpireDate", "tblContentStructure/dExpireDate", "false()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "nStatus", True, "Status", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Live", 1)
                    MyBase.addOption(oSelElmt, "Hidden", 0)
                    MyBase.addBind("nStatus", "tblContentStructure/nStatus", "true()")

                    MyBase.addInput(oFrmElmt, "cDescription", True, "Change Notes")
                    MyBase.addBind("cDescription", "tblContentStructure/cDescription", "false()")

                    MyBase.addNote(oFrmElmt, noteTypes.Hint, "This page will be copied without any permissions and will inherit the permissions from the new locations ancestors")
                    MyBase.addSubmit(oFrmElmt, "", "Save Page")


                    MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.ContentStructure, pgid)

                    'delete old page id so new page is added not updated
                    MyBase.Instance.SelectSingleNode("tblContentStructure/nStructKey").InnerText = ""
                    MyBase.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText = "Copy of " & MyBase.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText

                    oElmt = MyBase.Instance.SelectSingleNode("tblContentStructure")
                    oElmt.SetAttribute("copyPgid", pgid)
                    oElmt.SetAttribute("nCopyType", "0")
                    oElmt.SetAttribute("nCopyContent", "0")

                    If MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription/DisplayName") Is Nothing Then
                        Dim sDescText As String = MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml
                        'make sure the description contains our xml  

                        oElmt = moPageXML.CreateElement("DisplayName")
                        MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt)

                        Dim oElmt2 As XmlElement = moPageXML.CreateElement("Description")
                        oElmt2.InnerXml = sDescText
                        MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt2)

                    End If

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            Dim aReservedDirs As String() = Split("ewcommon,images,docs,media,css,bin,js,xforms,xsl", ",")
                            Dim i As Integer
                            For i = 0 To UBound(aReservedDirs)
                                If goRequest("cStructName") = aReservedDirs(i) Then
                                    MyBase.valid = False
                                    MyBase.addNote(oFrmElmt, noteTypes.Alert, "<strong>" & aReservedDirs(i) & "</strong> is a reserved directory name, please use another.")
                                End If
                            Next

                            'check for illegal charactors within the page name.
                            Dim oUrlExp As Regex = New Regex("^[\w\-\u0020\+]+$")

                            If Not oUrlExp.IsMatch(goRequest("cStructName")) Then
                                MyBase.valid = False
                                MyBase.addNote("cStructName", noteTypes.Alert, "Page names are used for the URL and only contain Alphanumberic, underscores, hyphens and spaces.")
                            End If
                        End If

                        If MyBase.valid Then
                            'add new
                            Dim dPublishDate As DateTime = #12:00:00 AM#
                            If MyBase.goRequest("dPublishDate") <> "" Then dPublishDate = CDate(MyBase.goRequest("dPublishDate"))
                            Dim dExpireDate As DateTime = #12:00:00 AM#
                            If MyBase.goRequest("dExpireDate") <> "" Then dExpireDate = CDate(MyBase.goRequest("dExpireDate"))

                            nNewPgid = moDbHelper.insertStructure(MyBase.goRequest("nStructParId"), MyBase.goRequest("nStructForeignRef"), MyBase.goRequest("cStructName"), MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml, MyBase.Instance.SelectSingleNode("tblContentStructure/cStructLayout").InnerXml, CLng(MyBase.goRequest("nStatus")), dPublishDate, dExpireDate, MyBase.goRequest("cDescription"))

                            moDbHelper.ReorderNode(dbHelper.objectTypes.ContentStructure, nNewPgid, "MoveBottom")

                            'Copy content and children
                            'Dim nCopyType As Boolean = False
                            'If Not IsNumeric(goRequest("nCopyType")) Then
                            '    nCopyType = False
                            'ElseIf goRequest("nCopyType") = 1 Then
                            '    nCopyType = True
                            'Else
                            '    nCopyType = False
                            'End If
                            moDbHelper.copyPageContent(pgid, nNewPgid, goRequest("nCopyType"), goRequest("nCopyContent"))
                        End If

                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditPage", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmCopyPageVersion(ByVal contentPageId As Long, ByVal parentPageId As Long) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim nNewPgid As Long
                Dim oElmt As XmlElement

                Try
                    MyBase.NewFrm("NewPageVersion")

                    MyBase.submission("EditEage", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "PageVersion", "2col", "New Page Version")
                    'MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                    'MyBase.addBind("nStructParId", "tblContentStructure/nStructParId", "true()")

                    Dim oGrp1 As XmlElement = MyBase.addGroup(oFrmElmt, "Group1", "", "")
                    Dim oGrp2 As XmlElement = MyBase.addGroup(oGrp1, "PageSettings", "", "Page Settings")

                    MyBase.addInput(oGrp2, "dPublishDate", True, "Publish Date", "calendar")
                    MyBase.addBind("dPublishDate", "tblContentStructure/dPublishDate", "false()")

                    MyBase.addInput(oGrp2, "dExpireDate", True, "Expire Date", "calendar")
                    MyBase.addBind("dExpireDate", "tblContentStructure/dExpireDate", "false()")

                    oSelElmt = MyBase.addSelect1(oGrp2, "nStatus", True, "Status", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Live", 1)
                    MyBase.addOption(oSelElmt, "Hidden", 0)
                    MyBase.addBind("nStatus", "tblContentStructure/nStatus", "true()")

                    MyBase.addInput(oGrp2, "cDescription", True, "Change Notes")
                    MyBase.addBind("cDescription", "tblContentStructure/cDescription", "false()")

                    Dim oGrp3 As XmlElement = MyBase.addGroup(oFrmElmt, "Group3", "", "")
                    Dim oGrp4 As XmlElement = MyBase.addGroup(oGrp3, "CopySettings", "", "Copy Settings")

                    MyBase.addInput(oGrp4, "cVersionDescription", True, "Version Description", "long")
                    MyBase.addBind("cVersionDescription", "tblContentStructure/cVersionDescription", "true()")

                    oSelElmt = MyBase.addSelect1(oGrp4, "nVersionType", True, "Type", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Personalisation", 1)
                    'MyBase.addOption(oSelElmt, "Working Copy", 2)
                    If Not myWeb.goLangConfig Is Nothing Then
                        MyBase.addOption(oSelElmt, "Language Version", 3)
                    End If
                    'MyBase.addOption(oSelElmt, "Split Test", 4)
                    MyBase.addBind("nVersionType", "tblContentStructure/nVersionType", "true()")

                    If Not myWeb.goLangConfig Is Nothing Then
                        oSelElmt = MyBase.addSelect1(oGrp4, "cVersionLang", True, "Language", "", ApperanceTypes.Full)
                        MyBase.addOption(oSelElmt, myWeb.goLangConfig.GetAttribute("default"), myWeb.goLangConfig.GetAttribute("code"))
                        Dim langNode As XmlElement
                        For Each langNode In myWeb.goLangConfig.SelectNodes("Language")
                            MyBase.addOption(oSelElmt, langNode.GetAttribute("systemName"), langNode.GetAttribute("code"))
                        Next
                        MyBase.addBind("cVersionLang", "tblContentStructure/cVersionLang", "tblContentStructure/nVersionType='3'")
                    End If

                    oSelElmt = MyBase.addSelect1(oGrp4, "nCopyContent", True, "Page Content", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Create empty pages", 0)
                    MyBase.addOption(oSelElmt, "Same content with multiple locations", 2)
                    ' MyBase.addOption(oSelElmt, "Same content with multiple primary locations", 3)
                    MyBase.addOption(oSelElmt, "Create copies of the content", 1)
                    MyBase.addBind("nCopyContent", "tblContentStructure/@nCopyContent", "true()")

                    Dim oGrp6 As XmlElement = MyBase.addGroup(oGrp3, "PageDetails", "", "Page Details")

                    MyBase.addInput(oGrp6, "cStructName", True, "Page Name", "long")
                    MyBase.addBind("cStructName", "tblContentStructure/cStructName", "true()")

                    MyBase.addInput(oGrp6, "cDisplayName", True, "Display Name", "long")
                    MyBase.addBind("cDisplayName", "tblContentStructure/cStructDescription/DisplayName", "false()")

                    MyBase.addTextArea(oGrp6, "cStructDescription", True, "Description", "xhtml", 10)
                    MyBase.addBind("cStructDescription", "tblContentStructure/cStructDescription/Description", "false()")

                    MyBase.addInput(oGrp6, "cUrl", True, "URL")
                    MyBase.addBind("cUrl", "tblContentStructure/cUrl", "false()")

                    Dim oGrp5 As XmlElement = MyBase.addGroup(MyBase.moXformElmt, "", "", "")

                    MyBase.addNote(oGrp5, noteTypes.Hint, "This page will be copied without any permissions and will inherit the permissions from the new locations ancestors")
                    MyBase.addSubmit(oGrp5, "", "Save Page")


                    MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.ContentStructure, contentPageId)

                    'delete old page id so new page is added not updated
                    MyBase.Instance.SelectSingleNode("tblContentStructure/nStructKey").InnerText = ""
                    MyBase.Instance.SelectSingleNode("tblContentStructure/cVersionDescription").InnerText = MyBase.Instance.SelectSingleNode("tblContentStructure/cStructName").InnerText & " New Version"

                    'set a default for version type
                    MyBase.Instance.SelectSingleNode("tblContentStructure/nVersionType").InnerText = "1"

                    'set a default for lang type
                    If Not myWeb.goLangConfig Is Nothing Then
                        MyBase.Instance.SelectSingleNode("tblContentStructure/cVersionLang").InnerText = myWeb.goLangConfig.GetAttribute("code")
                    End If

                    oElmt = MyBase.Instance.SelectSingleNode("tblContentStructure")
                    oElmt.SetAttribute("copyPgid", contentPageId)
                    oElmt.SetAttribute("nCopyType", "0")
                    oElmt.SetAttribute("nCopyContent", "0")

                    If MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription/DisplayName") Is Nothing Then
                        Dim sDescText As String = MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml
                        'make sure the description contains our xml  

                        oElmt = moPageXML.CreateElement("DisplayName")
                        MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt)

                        Dim oElmt2 As XmlElement = moPageXML.CreateElement("Description")
                        oElmt2.InnerXml = sDescText
                        MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription").AppendChild(oElmt2)

                    End If

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            Dim aReservedDirs As String() = Split("ewcommon,images,docs,media,css,bin,js,xforms,xsl", ",")
                            Dim i As Integer
                            For i = 0 To UBound(aReservedDirs)
                                If goRequest("cStructName") = aReservedDirs(i) Then
                                    MyBase.valid = False
                                    MyBase.addNote(oFrmElmt, noteTypes.Alert, "<strong>" & aReservedDirs(i) & "</strong> is a reserved directory name, please use another.")
                                End If
                            Next

                            'check for illegal charactors within the page name.
                            Dim oUrlExp As Regex = New Regex("^[\w\-\u0020\+]+$")

                            If Not oUrlExp.IsMatch(goRequest("cStructName")) Then
                                MyBase.valid = False
                                MyBase.addNote("cStructName", noteTypes.Alert, "Page names are used for the URL and only contain Alphanumberic, underscores, hyphens and spaces.")
                            End If
                        End If

                        If MyBase.valid Then
                            'add new
                            Dim dPublishDate As DateTime = #12:00:00 AM#
                            If MyBase.goRequest("dPublishDate") <> "" Then dPublishDate = CDate(MyBase.goRequest("dPublishDate"))
                            Dim dExpireDate As DateTime = #12:00:00 AM#
                            If MyBase.goRequest("dExpireDate") <> "" Then dExpireDate = CDate(MyBase.goRequest("dExpireDate"))

                            nNewPgid = moDbHelper.insertPageVersion(MyBase.goRequest("nStructParId"), MyBase.goRequest("nStructForeignRef"), MyBase.goRequest("cStructName"), MyBase.Instance.SelectSingleNode("tblContentStructure/cStructDescription").InnerXml, MyBase.Instance.SelectSingleNode("tblContentStructure/cStructLayout").InnerXml, CLng(MyBase.goRequest("nStatus")), dPublishDate, dExpireDate, MyBase.goRequest("cDescription"), , MyBase.goRequest("vParId"), MyBase.goRequest("cVersionLang"), MyBase.goRequest("cVersionDescription"), MyBase.goRequest("nVersionType"))

                            moDbHelper.ReorderNode(dbHelper.objectTypes.ContentStructure, nNewPgid, "MoveBottom")

                            'Copy content and children
                            'Dim nCopyType As Boolean = False
                            'If Not IsNumeric(goRequest("nCopyType")) Then
                            '    nCopyType = False
                            'ElseIf goRequest("nCopyType") = 1 Then
                            '    nCopyType = True
                            'Else
                            '    nCopyType = False
                            'End If
                            moDbHelper.copyPageContent(contentPageId, nNewPgid, 0, goRequest("nCopyContent"))
                        End If

                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmCopyPageVersion", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmEditPageLayout(Optional ByVal pgid As Long = 0) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim oChoices As XmlElement
                'Dim oChoicesElmt As XmlElement
                Dim oItem As XmlElement
                Dim oOptElmt As XmlElement
                Dim oDescElmt As XmlElement
                Dim sImgPath As String = ""

                Dim cProcessInfo As String = ""
                Dim oXformDoc As XmlDocument = New XmlDocument
                Try

                    MyBase.NewFrm("EditPageLayout")
                    MyBase.submission("EditEage", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditPage", "", "Select Page Layout")
                    MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                    MyBase.addBind("nStructParId", "tblContentStructure/nStructParId", "true()")

                    'MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select page layout")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "cStructLayout", True, "", "PickByImage", xForm.ApperanceTypes.Full)
                    MyBase.addBind("cStructLayout", "tblContentStructure/cStructLayout", "true()")

                    Try
                        'if this file exists then add the bespoke templates
                        oXformDoc.Load(goServer.MapPath(myWeb.moConfig("ProjectPath") & "/xsl") & "/LayoutManifest.xml")
                        sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath")

                        For Each oChoices In oXformDoc.SelectNodes("/PageLayouts/LayoutGroup")
                            Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelElmt, oChoices.GetAttribute("name"))
                            For Each oItem In oChoices.SelectNodes("Layout")
                                oOptElmt = MyBase.addOption(oChoicesElmt, Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"))
                                'lets add an image tag
                                oDescElmt = moPageXML.CreateElement("img")
                                oDescElmt.SetAttribute("src", sImgPath & "/" & oItem.GetAttribute("name") & ".gif")
                                oOptElmt.AppendChild(oDescElmt)

                                'lets insert a description html tag
                                If oItem.InnerXml <> "" Then
                                    oDescElmt = moPageXML.CreateElement("div")
                                    oDescElmt.SetAttribute("class", "description")
                                    oDescElmt.InnerXml = oItem.InnerXml
                                    oOptElmt.AppendChild(oDescElmt)
                                End If
                            Next
                        Next
                    Catch
                        'do nothing
                    End Try

                    'Lets load in the available common templates from XML file
                    Try
                        oXformDoc.Load(goServer.MapPath("/" & gcProjectPath & "ewcommon/xsl/pageLayouts") & "/LayoutManifest.xml")
                        sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath")
                    Catch ex As Exception
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "/" & gcProjectPath & "ewcommon/xsl/pageLayouts/LayoutManifest.xml could not be found. - " & ex.Message)
                    End Try


                    For Each oChoices In oXformDoc.SelectNodes("/PageLayouts/LayoutGroup")
                        Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelElmt, oChoices.GetAttribute("name"))
                        For Each oItem In oChoices.SelectNodes("Layout")
                            oOptElmt = MyBase.addOption(oChoicesElmt, Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"))

                            'lets add an image tag
                            oDescElmt = moPageXML.CreateElement("img")
                            oDescElmt.SetAttribute("src", sImgPath & "/" & oItem.GetAttribute("name") & ".gif")
                            oOptElmt.AppendChild(oDescElmt)

                            'lets insert a description html tag
                            If oItem.InnerXml <> "" Then
                                oDescElmt = moPageXML.CreateElement("div")
                                oDescElmt.SetAttribute("class", "description")
                                oDescElmt.InnerXml = oItem.InnerXml
                                oOptElmt.AppendChild(oDescElmt)
                            End If
                        Next
                    Next

                    Dim oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")
                    If oConfig("cart") = "on" Then
                        Try
                            oXformDoc.Load(goServer.MapPath("/" & gcProjectPath & "ewcommon/xsl/cart") & "/LayoutManifest.xml")
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath")
                        Catch ex As Exception
                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "/" & gcProjectPath & "ewcommon/xsl/cart/LayoutManifest.xml could not be found. - " & ex.Message)
                        End Try

                        For Each oChoices In oXformDoc.SelectNodes("/PageLayouts/LayoutGroup")
                            Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelElmt, oChoices.GetAttribute("name"))
                            For Each oItem In oChoices.SelectNodes("Layout")
                                oOptElmt = MyBase.addOption(oChoicesElmt, Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"))

                                'lets add an image tag
                                oDescElmt = moPageXML.CreateElement("img")
                                oDescElmt.SetAttribute("src", sImgPath & "/" & oItem.GetAttribute("name") & ".gif")
                                oOptElmt.AppendChild(oDescElmt)

                                'lets insert a description html tag
                                If oItem.InnerXml <> "" Then
                                    oDescElmt = moPageXML.CreateElement("div")
                                    oDescElmt.SetAttribute("class", "description")
                                    oDescElmt.InnerXml = oItem.InnerXml
                                    oOptElmt.AppendChild(oDescElmt)
                                End If
                            Next
                        Next
                    End If

                    If oConfig("membership") = "on" Then
                        Try
                            oXformDoc.Load(goServer.MapPath("/" & gcProjectPath & "ewcommon/xsl/membership") & "/LayoutManifest.xml")
                            sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath")
                        Catch ex As Exception
                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "/" & gcProjectPath & "ewcommon/xsl/membership/LayoutManifest.xml could not be found. - " & ex.Message)
                        End Try

                        For Each oChoices In oXformDoc.SelectNodes("/PageLayouts/LayoutGroup")
                            Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelElmt, oChoices.GetAttribute("name"))
                            For Each oItem In oChoices.SelectNodes("Layout")
                                oOptElmt = MyBase.addOption(oChoicesElmt, Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"))

                                'lets add an image tag
                                oDescElmt = moPageXML.CreateElement("img")
                                oDescElmt.SetAttribute("src", sImgPath & "/" & oItem.GetAttribute("name") & ".gif")
                                oOptElmt.AppendChild(oDescElmt)

                                'lets insert a description html tag
                                If oItem.InnerXml <> "" Then
                                    oDescElmt = moPageXML.CreateElement("div")
                                    oDescElmt.SetAttribute("class", "description")
                                    oDescElmt.InnerXml = oItem.InnerXml
                                    oOptElmt.AppendChild(oDescElmt)
                                End If
                            Next
                        Next
                    End If

                    If pgid > 0 Then
                        MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.ContentStructure, pgid)
                    Else
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "No page identified")
                    End If

                    If MyBase.isSubmitted Or goRequest.Form("ewsubmit.x") <> "" Or goRequest.Form("cStructLayout") <> "" Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            If pgid > 0 Then
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, MyBase.Instance)
                            End If
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmEditMailLayout(Optional ByVal pgid As Long = 0) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim oChoices As XmlElement
                'Dim oChoicesElmt As XmlElement
                Dim oItem As XmlElement
                Dim oOptElmt As XmlElement
                Dim oDescElmt As XmlElement
                Dim sImgPath As String = ""

                Dim cProcessInfo As String = ""
                Dim oXformDoc As XmlDocument = New XmlDocument
                Try

                    MyBase.NewFrm("EditPageLayout")
                    MyBase.submission("EditEage", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditPage", "", "Select Page Layout")
                    MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                    MyBase.addBind("nStructParId", "tblContentStructure/nStructParId", "true()")

                    ' MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select page layout")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "cStructLayout", True, "", "PickByImage", xForm.ApperanceTypes.Full)
                    MyBase.addBind("cStructLayout", "tblContentStructure/cStructLayout", "true()")

                    Try
                        'if this file exists then add the bespoke templates
                        oXformDoc.Load(goServer.MapPath(goConfig("ProjectPath") & "/xsl/Mailer/") & "/LayoutManifest.xml")
                        sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath")

                        For Each oChoices In oXformDoc.SelectNodes("/PageLayouts/LayoutGroup")
                            Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelElmt, oChoices.GetAttribute("name"))
                            For Each oItem In oChoices.SelectNodes("Layout")
                                oOptElmt = MyBase.addOption(oChoicesElmt, Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"))
                                'lets add an image tag
                                oDescElmt = moPageXML.CreateElement("img")
                                oDescElmt.SetAttribute("src", sImgPath & "/" & oItem.GetAttribute("name") & ".gif")
                                oOptElmt.AppendChild(oDescElmt)

                                'lets insert a description html tag
                                If oItem.InnerXml <> "" Then
                                    oDescElmt = moPageXML.CreateElement("div")
                                    oDescElmt.SetAttribute("class", "description")
                                    oDescElmt.InnerXml = oItem.InnerXml
                                    oOptElmt.AppendChild(oDescElmt)
                                End If
                            Next
                        Next
                    Catch
                        'do nothing
                    End Try



                    'Lets load in the available common templates from XML file
                    Try
                        oXformDoc.Load(goServer.MapPath("/" & gcProjectPath & "ewcommon/xsl/mailer") & "/LayoutManifest.xml")
                        sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath")
                    Catch ex As Exception
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "/" & gcProjectPath & "ewcommon/xsl/mailer/LayoutManifest.xml could not be found. - " & ex.Message)
                    End Try

                    For Each oChoices In oXformDoc.SelectNodes("/PageLayouts/LayoutGroup")
                        Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelElmt, oChoices.GetAttribute("name"))
                        For Each oItem In oChoices.SelectNodes("Layout")
                            oOptElmt = MyBase.addOption(oChoicesElmt, Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("name"))

                            'lets add an image tag
                            oDescElmt = moPageXML.CreateElement("img")
                            oDescElmt.SetAttribute("src", sImgPath & "/" & oItem.GetAttribute("name") & ".gif")
                            oOptElmt.AppendChild(oDescElmt)

                            'lets insert a description html tag
                            If oItem.InnerXml <> "" Then
                                oDescElmt = moPageXML.CreateElement("div")
                                oDescElmt.SetAttribute("class", "description")
                                oDescElmt.InnerXml = oItem.InnerXml
                                oOptElmt.AppendChild(oDescElmt)
                            End If
                        Next
                    Next


                    '  MyBase.addSubmit(oFrmElmt, "", "Save Page")

                    If pgid > 0 Then
                        MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.ContentStructure, pgid)
                    Else
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "No page identified")
                    End If

                    If MyBase.isSubmitted Or goRequest.Form("ewsubmit.x") <> "" Or goRequest.Form("cStructLayout") <> "" Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            If pgid > 0 Then
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.ContentStructure, MyBase.Instance)
                            End If
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmAddModule(ByVal pgid As Long, ByVal position As String) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim sImgPath As String = ""

                Dim cProcessInfo As String = ""
                Dim oXformDoc As XmlDocument = New XmlDocument
                Try

                    If moRequest("cModuleBox") <> "" Then

                        xFrmEditContent(0, "Module/" & moRequest("cModuleType"), pgid, moRequest("cPosition"))

                        Return MyBase.moXformElmt
                    Else
                        MyBase.NewFrm("EditPageLayout")
                        MyBase.submission("AddModule", "", "post", "form_check(this)")
                        MyBase.Instance.InnerXml = "<Module position=""" & position & """></Module>"

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Add Module", "", "Select Module Type")
                        MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                        MyBase.addInput(oFrmElmt, "cPosition", True, "Position", "hidden")
                        MyBase.addBind("cPosition", "Module/@position", "true()")


                        '  MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Click the image to select Module Type")

                        oSelElmt = MyBase.addSelect1(oFrmElmt, "cModuleType", True, "", "PickByImage", xForm.ApperanceTypes.Full)

                        EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & "ewcommon/xsl/PageLayouts", "ModuleTypes/ModuleGroup", "Module", False)
                        If myWeb.moConfig("ClientCommonFolder") <> "" Then
                            EnumberateManifestOptions(oSelElmt, myWeb.moConfig("ClientCommonFolder") & "/xsl", "ModuleTypes/ModuleGroup", "Module", False)
                        End If
                        EnumberateManifestOptions(oSelElmt, "/xsl", "ModuleTypes/ModuleGroup", "Module", True)
                        If myWeb.moConfig("Search") = "on" Then
                            EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & "ewcommon/xsl/Search", "ModuleTypes/ModuleGroup", "Module", False)
                        End If
                        If myWeb.moConfig("Membership") = "on" Then
                            EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & "ewcommon/xsl/Membership", "ModuleTypes/ModuleGroup", "Module", False)
                        End If
                        If myWeb.moConfig("Cart") = "on" Then
                            EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & "ewcommon/xsl/Cart", "ModuleTypes/ModuleGroup", "Module", False)
                        End If
                        If myWeb.moConfig("Quote") = "on" Then
                            EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & "ewcommon/xsl/Quote", "ModuleTypes/ModuleGroup", "Module", False)
                        End If
                        If myWeb.moConfig("MailingList") = "on" Then
                            EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & "ewcommon/xsl/Mailer", "ModuleTypes/ModuleGroup", "Module", False)
                        End If
                        If myWeb.moConfig("Subscriptions") = "on" Then
                            EnumberateManifestOptions(oSelElmt, "/" & gcProjectPath & "ewcommon/xsl/Subscriptions", "ModuleTypes/ModuleGroup", "Module", False)
                        End If


                        If MyBase.isSubmitted Or goRequest.Form("ewsubmit.x") <> "" Or goRequest.Form("cModuleType") <> "" Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            If MyBase.valid Then
                                'Do nothing
                                'or redirect to content form
                                xFrmEditContent(0, "Module/" & moRequest("cModuleType"), pgid, moRequest("cPosition"))
                            End If
                        End If

                        MyBase.addValues()
                        Return MyBase.moXformElmt
                    End If

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Protected Sub EnumberateManifestOptions(ByRef oSelectElmt As XmlElement, ByVal filepath As String, ByVal groupName As String, ByVal optionName As String, ByVal bIgnoreIfNotFound As Boolean)

                Dim oXformDoc As XmlDocument = New XmlDocument
                Dim cProcessInfo As String = ""
                Dim sImgPath As String = ""
                Dim oChoices As XmlElement
                Dim oItem As XmlElement
                Dim oOptElmt As XmlElement
                Dim oDescElmt As XmlElement

                Try
                    If filepath = "" Then filepath = "/"

                    Try
                        'if this file exists then add the bespoke templates
                        oXformDoc.Load(goServer.MapPath(filepath) & "/LayoutManifest.xml")
                        sImgPath = oXformDoc.DocumentElement.GetAttribute("imgPath")
                        For Each oChoices In oXformDoc.SelectNodes("/PageLayouts/" & groupName)
                            If oChoices.GetAttribute("targetCssFramework") = "" Or (Not (myWeb.moConfig("cssFramework") Is Nothing) And oChoices.GetAttribute("targetCssFramework").Contains("" & myWeb.moConfig("cssFramework"))) Then
                                Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelectElmt, oChoices.GetAttribute("name"))
                                If oChoices.GetAttribute("icon") <> "" Then
                                    Dim labelElmt As XmlElement = oChoicesElmt.SelectSingleNode("label")
                                    labelElmt.SetAttribute("icon", oChoices.GetAttribute("icon"))
                                End If
                                For Each oItem In oChoices.SelectNodes(optionName)
                                    If oItem.GetAttribute("targetCssFramework") = "" Or (Not (myWeb.moConfig("cssFramework") Is Nothing) And oItem.GetAttribute("targetCssFramework").Contains("" & myWeb.moConfig("cssFramework"))) Then
                                        oOptElmt = MyBase.addOption(oChoicesElmt, Replace(oItem.GetAttribute("name"), "_", " "), oItem.GetAttribute("type"))
                                        'lets add an image tag
                                        oDescElmt = moPageXML.CreateElement("img")
                                        oDescElmt.SetAttribute("src", sImgPath & "/" & oItem.GetAttribute("name") & ".gif")
                                        oOptElmt.AppendChild(oDescElmt)
                                        'lets insert a description html tag
                                        If oItem.InnerXml <> "" Then
                                            oDescElmt = moPageXML.CreateElement("div")
                                            oDescElmt.SetAttribute("class", "description")
                                            oDescElmt.InnerXml = oItem.InnerXml
                                            oOptElmt.AppendChild(oDescElmt)
                                        End If
                                    End If
                                Next
                            End If

                        Next
                    Catch ex As Exception
                        If Not bIgnoreIfNotFound Then
                            MyBase.addNote(oSelectElmt.ParentNode, xForm.noteTypes.Alert, filepath & " could not be found. - " & ex.Message)
                        End If
                    End Try
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "EnumberateManifestOptions", ex, "", cProcessInfo, gbDebug)
                End Try

            End Sub
            '    Public Overridable Function xFrmEditContent(Optional ByVal id As Long = 0, Optional ByVal cContentSchemaName As String = "", Optional ByVal pgid As Long = 0, Optional ByVal cContentName As String = "", Optional ByVal bCopy As Boolean = False, Optional ByRef nReturnId As Integer = 0, Optional ByVal nVersionId As Long = 0) As XmlElement
            '        xFrmEditContent(id, cContentSchemaName, pgid, cContentName, bCopy, nReturnId, "", "", Optional ByVal nVersionId As Long = 0)
            '    End Function

            Public Overridable Function xFrmEditContent(Optional ByVal id As Long = 0, Optional ByVal cContentSchemaName As String = "", Optional ByVal pgid As Long = 0, Optional ByVal cContentName As String = "", Optional ByVal bCopy As Boolean = False, Optional ByRef nReturnId As Integer = 0, Optional ByRef zcReturnSchema As String = "", Optional ByRef AlternateFormName As String = "", Optional ByVal nVersionId As Long = 0) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oGrp1Elmt As XmlElement
                Dim oGrp2Elmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")
                Dim bCascade As Boolean = False
                Dim cProcessInfo As String = ""
                Dim oCRNode As XmlElement
                Dim cModuleType As String = ""

                ' Location specific scopes
                Dim oLocationSelects As XmlNodeList = Nothing
                Dim oMenuItemsFromSelect As XmlNodeList = Nothing

                Dim oContentLocations As xFormContentLocations


                Try

                    Dim integrationHelper As New Integration.Directory.Helper(myWeb)

                    If id > 0 Then
                        'we may be halfway through a trigger so lets rescue the instance from the session
                        If goSession("oContentInstance") Is Nothing Then
                            If nVersionId > 0 Then

                                oTempInstance = moDbHelper.GetVersionInstance(id, nVersionId)
                                ' Only Update the status if the cmd is ewcmd is RollbackContent
                                If Me.myWeb.moRequest("ewCmd") = "RollbackContent" Then
                                    oTempInstance.SelectSingleNode("tblContent/nStatus").InnerText = dbHelper.Status.Live
                                End If

                            Else
                                oTempInstance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Content, id)
                            End If

                            'turn off process repeats when loading from file
                            Me.bProcessRepeats = False
                        Else
                            oTempInstance = goSession("oContentInstance")
                        End If

                        If cContentSchemaName = "" Then
                            cContentSchemaName = oTempInstance.SelectSingleNode("tblContent/cContentSchemaName").InnerText
                            If cContentSchemaName = "Module" Then
                                If Not oTempInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content/@moduleType") Is Nothing Then
                                    cModuleType = oTempInstance.SelectSingleNode("tblContent/cContentXmlBrief/Content/@moduleType").Value
                                End If
                            End If
                            If moRequest("type") <> "" Then cContentSchemaName = moRequest("type")
                        End If

                        moDbHelper.getLocationsByContentId(id, oTempInstance.FirstChild)

                        'Add ProductCategories
                        Dim sProductTypes As String = "Product,SKU"
                        If myWeb.moConfig("ProductTypes") <> "" Then
                            sProductTypes = myWeb.moConfig("ProductTypes")
                        End If
                        sProductTypes = sProductTypes.Trim().TrimEnd(",") & ","
                        If sProductTypes.Contains(cContentSchemaName & ",") And id > 0 Then
                            If moDbHelper.checkDBObjectExists("sp_GetProductGroups") Then
                                Dim prodCatElmt As XmlElement = oTempInstance.OwnerDocument.CreateElement("ProductGroups")
                                Dim sSQL As String = "execute sp_GetProductGroups " & id
                                Dim Ids As String = moDbHelper.GetDataValue(sSQL)
                                Ids.TrimEnd(",")
                                prodCatElmt.SetAttribute("ids", Ids)
                                oTempInstance.AppendChild(prodCatElmt)
                            End If
                        End If

                    End If

                    If Not goSession("oContentInstance") Is Nothing Then
                        'turn off process repeats when loading from file if we are going to load the instance later.
                        Me.bProcessRepeats = False
                    End If

                    ' Set the return parameter
                    zcReturnSchema = cContentSchemaName

                    ' ok lets load in an xform from the file location.
                    ' If cContentSchemaName = "Subscription" Then
                    ' Return xFrmEditSubscription(id, pgid)
                    ' End If

                    ''''''' if contentSchemeaName starts with "filter|" then modify the path...

                    Dim cXformName As String = cContentSchemaName
                    If AlternateFormName <> "" Then cXformName = AlternateFormName
                    If cModuleType <> "" Then cXformName = cXformName & "/" & cModuleType

                    If Not MyBase.load("/xforms/content/" & cXformName & ".xml", myWeb.maCommonFolders) Then
                        ' load a default content xform if no alternative.
                        cProcessInfo = "/xforms/content/" & cXformName & ".xml - Not Found"

                        MyBase.NewFrm("EditContent")

                        MyBase.submission("EditContent", "", "post", "form_check(this)")

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditContent", "2Col", "Edit Content")
                        MyBase.addNote("EditContent", xForm.noteTypes.Alert, "We do not have an XForm for this type of content - this is the default form")

                        MyBase.addInput(oFrmElmt, "nContentKey", True, "ContentId", "hidden")
                        MyBase.addBind("nStructParId", "tblContent/nContentKey", "true()")

                        MyBase.addInput(oFrmElmt, "cContentSchemaName", True, "cContentSchemaName", "hidden")
                        MyBase.addBind("cContentSchemaName", "tblContent/cContentSchemaName", "true()")

                        oGrp1Elmt = MyBase.addGroup(oFrmElmt, "Settings", "", "Content Settings")
                        oGrp2Elmt = MyBase.addGroup(oFrmElmt, "Content", "", "Full XML")

                        MyBase.addInput(oGrp1Elmt, "cContentName", True, "Page Name")
                        MyBase.addBind("cContentName", "tblContent/cContentName", "true()")

                        MyBase.addTextArea(oGrp2Elmt, "cContentXmlBrief", True, "Content Brief", "TextAreaBrief")
                        MyBase.addBind("cContentXmlBrief", "tblContent/cContentXmlBrief", "false()")

                        MyBase.addTextArea(oGrp2Elmt, "cContentXmlDetail", True, "Content Detail", "TextAreaDetail")
                        MyBase.addBind("cContentXmlDetail", "tblContent/cContentXmlDetail", "false()")

                        MyBase.addInput(oGrp1Elmt, "dPublishDate", True, "Publish Date", "calendar short")
                        MyBase.addBind("dPublishDate", "tblContentStructure/dPublishDate", "false()")

                        MyBase.addInput(oGrp1Elmt, "dExpireDate", True, "Expire Date", "calendar short")
                        MyBase.addBind("dExpireDate", "tblContentStructure/dExpireDate", "false()")

                        oSelElmt = MyBase.addSelect1(oGrp1Elmt, "nStatus", True, "Status", "", ApperanceTypes.Minimal)
                        MyBase.addOption(oSelElmt, "Live", 1)
                        MyBase.addOption(oSelElmt, "Hidden", 0)
                        MyBase.addBind("nStatus", "tblContentStructure/nStatus", "true()")

                        MyBase.addInput(oGrp1Elmt, "cDescription", True, "Change Notes")
                        MyBase.addBind("cDescription", "tblContentStructure/cDescription", "false()")

                        MyBase.addSubmit(oFrmElmt, "", "Save Content")
                    End If

                    If id > 0 Then
                        'here it would be really useful to merge nodes!
                        MyBase.bProcessRepeats = True
                        Dim NonTableInstanceElements As XmlElement
                        For Each NonTableInstanceElements In MyBase.Instance
                            If NonTableInstanceElements.Name <> "tblContent" Then
                                '<Relation type="" direction="child" relatedContentId=""/>
                                If NonTableInstanceElements.Name = "Relation" Then
                                    Dim sSql As String
                                    If LCase(NonTableInstanceElements.GetAttribute("direction")) = "child" Then
                                        sSql = "Select nContentParentId from tblContentRelation where nContentChildId = " & id & " And cRelationType = '" & NonTableInstanceElements.GetAttribute("type") & "'"
                                    Else
                                        sSql = "Select nContentChildId from tblContentRelation where nContentParentId = " & id & " AND cRelationType = '" & NonTableInstanceElements.GetAttribute("type") & "'"
                                    End If
                                    Dim oRead As SqlDataReader = moDbHelper.getDataReader(sSql)
                                    Dim CSV As String = ""
                                    While oRead.Read()
                                        If CSV <> "" Then
                                            CSV = CSV & ","
                                        End If
                                        CSV = CSV & CStr(oRead.GetInt32(0))
                                    End While
                                    NonTableInstanceElements.SetAttribute("relatedContentId", CSV)

                                End If
                                Dim newNode As XmlNode = oTempInstance.OwnerDocument.ImportNode(NonTableInstanceElements, True)
                                oTempInstance.AppendChild(newNode)
                            End If
                        Next

                        MyBase.updateInstance(oTempInstance)

                        'Add related content to the instance
                        oCRNode = moPageXML.CreateElement("ContentRelations")
                        moDbHelper.addRelatedContent(oCRNode, id, True)
                        MyBase.Instance.AppendChild(oCRNode)

                        If bCopy Then
                            oCRNode.SetAttribute("copyRelations", "true")

                            'select all nodes where content = cContentName
                            cContentName = MyBase.Instance.SelectSingleNode("tblContent/cContentName").InnerText()
                            Dim oElmt As XmlElement
                            For Each oElmt In MyBase.Instance.SelectNodes("descendant::*[.='" & cContentName & "']")
                                If oElmt.InnerText = cContentName And oElmt.Name = "cContentName" Then
                                    oElmt.InnerText = "Copy of " & cContentName
                                End If
                            Next
                            'set the id to zero
                            id = 0
                            MyBase.Instance.SelectSingleNode("tblContent/nContentKey").InnerText() = 0
                            'remove any audit info, but keep the publish start and expire dates.
                            MyBase.Instance.SelectSingleNode("tblContent/nAuditId").InnerText() = ""
                            MyBase.Instance.SelectSingleNode("tblContent/nAuditKey").InnerText() = ""
                            MyBase.Instance.SelectSingleNode("tblContent/dInsertDate").InnerText() = ""
                            MyBase.Instance.SelectSingleNode("tblContent/nInsertDirId").InnerText() = ""
                            MyBase.Instance.SelectSingleNode("tblContent/dUpdateDate").InnerText() = ""
                            MyBase.Instance.SelectSingleNode("tblContent/nUpdateDirId").InnerText() = ""
                        End If



                        addNewTextNode("bCascade", MyBase.Instance.SelectSingleNode("tblContent"), LCase(CStr(moDbHelper.isCascade(id))), True, False)
                        If pgid = 0 Then
                            'lets go get a parId to set the pgid so we can update the cascade position
                            pgid = moDbHelper.getPrimaryLocationByArtId(id)
                        End If
                    Else

                        If Not goSession("oContentInstance") Is Nothing Then
                            Me.bProcessRepeats = True
                            MyBase.Instance = goSession("oContentInstance")
                        End If

                        If cContentName <> "" Then
                            MyBase.Instance.SelectSingleNode("tblContent/cContentName").InnerText() = cContentName
                            MyBase.Instance.SelectSingleNode("tblContent/dPublishDate").InnerText() = Protean.Tools.Xml.XmlDate(Now())
                        End If

                        If pgid = 0 Then
                            'we are adding orphan content so cannot cascade
                            'remove any cascade radio on form
                            Dim oElmt As XmlElement
                            For Each oElmt In MyBase.moXformElmt.SelectNodes("descendant-or-self::*[@bind='bCascade']")
                                oElmt.ParentNode.RemoveChild(oElmt)
                            Next
                        Else
                            addNewTextNode("bCascade", MyBase.Instance.SelectSingleNode("tblContent"), "", True, False)
                        End If

                    End If

                    ' Check for adding the integration checkbox
                    integrationHelper.PostContentCheckboxes(Me, cContentSchemaName, (id > 0))

                    ' Process any location selects
                    oContentLocations = New xFormContentLocations(id, Me)
                    oContentLocations.ProcessSelects()

                    ' Version Control: if on, copy the status node for use after submission
                    If myWeb.gbVersionControl Then
                        Dim nCurrentStatus As String = ""
                        If Xml.NodeState(MyBase.Instance, "//nStatus", , , , , , nCurrentStatus) = XmlNodeState.HasContents Then
                            addNewTextNode("currentStatus", MyBase.Instance, nCurrentStatus)
                        End If
                    End If

                    ' Additional Processing : Post Build
                    Me.xFrmEditContentPostBuildProcessing(cContentSchemaName)

                    If MyBase.isSubmitted Then

                        ' Additional Processing : Pre Submission 
                        xFrmEditContentSubmissionPreProcessing()

                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()





                        If MyBase.valid Then

                            Dim bPreviewRedirect As Boolean = False

                            If goRequest("ptn-preview") <> "" Then
                                If myWeb.gbVersionControl Then
                                    'Leave the current version unchanged and live

                                    'create a new version of the content as pending
                                    MyBase.Instance.SelectSingleNode("tblContent/nStatus").InnerText() = dbHelper.Status.InProgress

                                    'redirect to preview version in preview mode
                                    bPreviewRedirect = True
                                End If
                            End If

                            Dim editResult As dbHelper.ActivityType = Nothing

                            'we don't need this now.
                            goSession("oContentInstance") = Nothing

                            'trim the contentName to no longer than 255 chars
                            MyBase.Instance.SelectSingleNode("*/cContentName").InnerXml() = Left(MyBase.Instance.SelectSingleNode("*/cContentName").InnerXml, 255)

                            'remove any invalid charactors from the contentName
                            Dim oUrlExp As Regex = New Regex("[^\w\-\u0020\+]")
                            MyBase.Instance.SelectSingleNode("*/cContentName").InnerXml() = oUrlExp.Replace(MyBase.Instance.SelectSingleNode("*/cContentName").InnerXml(), "")
                            oUrlExp = Nothing
                            If Not MyBase.Instance.SelectSingleNode("*/bCascade") Is Nothing Then
                                If MyBase.Instance.SelectSingleNode("*/bCascade").InnerXml = "true" Then
                                    bCascade = True
                                End If
                            End If

                            If id > 0 Then

                                Dim updatedVersionId = moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, MyBase.Instance)

                                moDbHelper.CommitLogToDB(dbHelper.ActivityType.ContentEdited, myWeb.mnUserId, myWeb.moSession.SessionID, Now, id, pgid, "")
                                'Redirection 
                                Dim redirectType As String = ""
                                Dim newUrl As String = ""
                                Dim strOldurl As String = ""
                                If moRequest("redirectType") IsNot Nothing Then
                                    redirectType = moRequest("redirectType").ToString()
                                End If

                                If moRequest("productNewUrl") IsNot Nothing Then
                                    newUrl = moRequest("productNewUrl").ToString()
                                End If
                                If moRequest("productOldUrl") IsNot Nothing Then
                                    strOldurl = moRequest("productOldUrl").ToString()
                                End If









                                ' Individual content location set
                                ' Don't set a location if a contentparid has been passed (still process content locations as tickboexs on the form, if they've been set)
                                If Not (myWeb.moRequest("contentParId") IsNot Nothing And myWeb.moRequest("contentParId") <> "") Then

                                    'TS 28-11-2017 we only want to update the cascade information if the content is on this page.
                                    'If not on this page i.e. being edited via search results or related content on a page we should ignore this.
                                    If moDbHelper.ExeProcessSqlScalar("select count(nContentLocationKey) from tblContentLocation where nContentId=" & id & " and nStructId = " & pgid) > 0 Then
                                        moDbHelper.setContentLocation(pgid, id, , bCascade, , "")
                                    End If
                                End If

                                'TS 10-01-2014 fix for cascade on saved items... To Be tested
                                If bCascade And pgid > 0 Then
                                    moDbHelper.setContentLocation(pgid, id, True, bCascade, )
                                End If


                                editResult = dbHelper.ActivityType.ContentEdited

                                If updatedVersionId <> id Then
                                    nReturnId = updatedVersionId
                                Else
                                    nReturnId = id
                                End If

                            Else
                                Dim nContentId As Long
                                nContentId = moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, MyBase.Instance)
                                moDbHelper.CommitLogToDB(dbHelper.ActivityType.ContentAdded, myWeb.mnUserId, myWeb.moSession.SessionID, Now, nContentId, pgid, "")


                                'If we have an action here we need to relate the item
                                If goSession("mcRelAction") = "Add" Or goSession("mcRelAction") = "Find" Or goSession("mcRelAction") = "Edit" Then
                                    Dim b2Way As Boolean = IIf(moRequest("RelType") = "2way" Or moRequest("direction") = "2Way", True, False)
                                    Dim sRelType As String = moRequest("relationType")
                                    moDbHelper.insertContentRelation(goSession("mcRelParent"), nContentId, b2Way, sRelType)
                                Else
                                    'TS - Change 26/04/2016 We do not want added to the page if it is related.

                                    ' Individual content location set
                                    ' Don't set a location if a contentparid has been passed (still process content locations as tickboexs on the form, if they've been set)
                                    If Not (myWeb.moRequest("contentParId") IsNot Nothing And myWeb.moRequest("contentParId") <> "") Then
                                        moDbHelper.setContentLocation(pgid, nContentId, True, bCascade, , moRequest("cPosition"))
                                    End If
                                End If


                                editResult = dbHelper.ActivityType.ContentAdded
                                nReturnId = nContentId

                                'If this is a new element then we need to add the related content to the instance to be handled in processInstanceExtras
                                Dim item As Object
                                For Each item In moRequest.Form
                                    If CStr(item).StartsWith("Relate_") Then
                                        Dim arr() As String = CStr(item).Split("_")
                                        Dim relateElmt As XmlElement = moPageXML.CreateElement("Relation")
                                        relateElmt.SetAttribute("relatedContentId", moRequest.Form(CStr(item)))
                                        relateElmt.SetAttribute("type", arr(1))
                                        relateElmt.SetAttribute("direction", arr(2))
                                        MyBase.Instance.AppendChild(relateElmt)
                                    End If
                                Next

                            End If

                            'TS Added 24-11-2014 to allow content forms to add related content from dropdowns.
                            moDbHelper.processInstanceExtras(nReturnId, MyBase.Instance, False, False)


                            ' Check for related content redirection
                            Dim mcRelRedirectString As String = goSession("mcRelRedirectString")
                            If MyBase.valid AndAlso Not String.IsNullOrEmpty(mcRelRedirectString) Then

                                Dim cQueryString As String = goRequest.QueryString.ToString
                                If cQueryString.IndexOf("ewCmd=") > 0 Then
                                    'here we fail because the ? is an & >=[
                                    If cQueryString.IndexOf("&") > 0 Then
                                        'So we have the index of the first &, how to replace?!
                                    End If
                                Else
                                    cQueryString = "?" & Replace(goRequest.QueryString.ToString, "path=", "")
                                End If
                                If cQueryString.IndexOf("ewCmd=") <> -1 Then
                                    cQueryString = cQueryString.Substring(cQueryString.IndexOf("ewCmd="))
                                End If
                                If cQueryString.IndexOf("ajaxCmd=") <> -1 Then
                                    cQueryString = cQueryString.Substring(cQueryString.IndexOf("ajaxCmd="))
                                End If
                                mcRelRedirectString = mcRelRedirectString.Substring(mcRelRedirectString.IndexOf("ewCmd="))

                                If mcRelRedirectString.ToLower() = cQueryString.ToLower() Then
                                    ' Suppress last page being reset anywhere else
                                    myWeb.mbSuppressLastPageOverrides = True
                                    myWeb.moSession.Remove("lastPage")
                                    myWeb.msRedirectOnEnd = goSession("mnContentRelationParent")
                                End If

                            End If



                            goSession("mnContentRelationParent") = Nothing
                            goSession("mcRelRedirectString") = Nothing
                            goSession("mcRelAction") = Nothing
                            goSession("mcRelParent") = Nothing

                            If goSession("EwCmd") = "" Then
                                goSession("EwCmd") = "Normal"
                            End If

                            ' Submitted and valid - should have a content id let's process the relationships
                            oContentLocations.ProcessRequest(nReturnId)

                            ' User Integrations check
                            If integrationHelper.Enabled Then
                                integrationHelper.PostContent(nReturnId)
                            End If


                            ' Module content edit action handler
                            Dim contentEditActionHandler As XmlElement

                            For Each contentEditActionHandler In MyBase.Instance.SelectNodes("//Content[@editAction]")
                                Dim contentEditAction As String = contentEditActionHandler.GetAttribute("editAction")
                                If Not String.IsNullOrEmpty(contentEditAction) Then

                                    Dim assemblyName As String = contentEditActionHandler.GetAttribute("assembly")
                                    Dim assemblyType As String = contentEditActionHandler.GetAttribute("assemblyType")
                                    Dim providerName As String = contentEditActionHandler.GetAttribute("providerName")
                                    Dim providerType As String = contentEditActionHandler.GetAttribute("providerType")
                                    If providerType = "" Then providerType = "messaging"


                                    Dim methodName As String = contentEditAction
                                    Dim classPath As String = ""

                                    If methodName.Contains(".") Then
                                        methodName = Right(contentEditAction, Len(contentEditAction) - contentEditAction.LastIndexOf(".") - 1)
                                        classPath = Left(contentEditAction, contentEditAction.LastIndexOf("."))
                                    End If

                                    '     Dim providerSection As String = Coalesce(contentEditActionHandler.GetAttribute("providerSection"), "eonic/" & providerType & "Providers")

                                    ' Edit Action method constructor follows the following format:
                                    ' 1 - Protean.Cms
                                    ' 2 - Content XML
                                    ' 3 - Content ID being editted
                                    ' 4 - Content action 

                                    Try
                                        Dim calledType As Type

                                        If assemblyName <> "" Then
                                            contentEditAction = contentEditAction & ", " & assemblyName
                                        End If
                                        'Dim oModules As New Protean.Cms.Membership.Modules

                                        If providerName <> "" Then
                                            'case for external Providers
                                            Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/" & providerType & "Providers")
                                            Dim assemblyInstance As [Assembly]

                                            If Not moPrvConfig.Providers(providerName & "Local") Is Nothing Then
                                                If moPrvConfig.Providers(providerName & "Local").Parameters("path") <> "" Then
                                                    assemblyInstance = [Assembly].LoadFrom(goServer.MapPath(moPrvConfig.Providers(providerName & "Local").Parameters("path")))
                                                    calledType = assemblyInstance.GetType(contentEditAction, True)
                                                Else
                                                    assemblyInstance = [Assembly].Load(moPrvConfig.Providers(providerName & "Local").Type)
                                                    calledType = assemblyInstance.GetType(contentEditAction, True)
                                                End If
                                            Else
                                                Select Case moPrvConfig.Providers(providerName).Parameters("path")
                                                    Case ""
                                                        assemblyInstance = [Assembly].Load(moPrvConfig.Providers(providerName).Type)
                                                        calledType = assemblyInstance.GetType(contentEditAction, True)
                                                    Case "builtin"
                                                        Dim prepProviderName As String ' = Replace(moPrvConfig.Providers(providerName).Type, ".", "+")
                                                        'prepProviderName = (New Regex("\+")).Replace(prepProviderName, ".", 1)
                                                        prepProviderName = moPrvConfig.Providers(providerName).Type
                                                        calledType = System.Type.GetType(prepProviderName & "+" & classPath, True)
                                                    Case Else
                                                        assemblyInstance = [Assembly].LoadFrom(goServer.MapPath(moPrvConfig.Providers(providerName).Parameters("path")))

                                                        classPath = moPrvConfig.Providers(providerName).Parameters("classPrefix") & classPath
                                                        calledType = assemblyInstance.GetType(classPath, True)
                                                End Select

                                            End If
                                        ElseIf assemblyType <> "" Then
                                            'case for external DLL's
                                            Dim assemblyInstance As [Assembly] = [Assembly].Load(assemblyType)
                                            calledType = assemblyInstance.GetType(contentEditAction, True)
                                        Else
                                            'case for methods within EonicWeb Core DLL
                                            calledType = System.Type.GetType(contentEditAction, True)
                                        End If

                                        Dim o As Object = Activator.CreateInstance(calledType)

                                        Dim args(3) As Object
                                        args(0) = myWeb
                                        args(1) = contentEditActionHandler
                                        args(2) = nReturnId
                                        args(3) = editResult

                                        calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, Nothing, o, args)

                                        'Error Handling ?
                                        'Object Clearup ?

                                        calledType = Nothing

                                        'Update again ?
                                        MyBase.Instance.SelectSingleNode("*/nContentPrimaryId").InnerText = 0
                                        moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, MyBase.Instance, nReturnId)

                                    Catch ex As Exception
                                        '  OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo))
                                        cProcessInfo = assemblyName & "." & contentEditAction & " not found"

                                    End Try
                                End If
                            Next

                            If bPreviewRedirect Then
                                Dim VerId As Long = 0
                                myWeb.msRedirectOnEnd = "/?ewCmd=PreviewOn&pgid=" & pgid & "&artid=" & id & "&verId=" & nReturnId
                            End If

                        End If




                    ElseIf isSubmittedOther(pgid) Then ' has another specific submit button been pressed?
                        'This should really be taken over using  xForms Triggers
                        MyBase.updateInstanceFromRequest()
                        If Not goSession("mcRelRedirectString") Is Nothing Or Not goSession("mcRelRedirectString") = "" Then
                            MyBase.validate()
                            If MyBase.valid Then
                                myWeb.msRedirectOnEnd = goSession("mcRelRedirectString")
                                MyBase.valid = False
                            End If
                        Else
                            'we are re-ordering so we don't want a valid form
                            MyBase.valid = False
                        End If
                        goSession("oContentInstance") = Nothing

                    ElseIf MyBase.isTriggered Then
                        'we have clicked a trigger so we must update the instance
                        MyBase.updateInstanceFromRequest()
                        'lets save the instance
                        goSession("oContentInstance") = MyBase.Instance
                    Else
                        'clear this if we are loading the first form
                        goSession("oContentInstance") = Nothing
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditContent", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Overridable Sub xFrmEditContentPostBuildProcessing(ByVal cContentSchemaName As String)
                ' Holding function for adding proecssing after building the form.
            End Sub

            Public Overridable Sub xFrmEditContentSubmissionPreProcessing()
                ' Holding function for adding pre proecssing on submitting this form.
            End Sub

            Public Function isSubmittedOther(Optional ByVal pgid As Integer = 0) As Boolean

                'Check if a specific button has been pressed
                Dim myItem As Object
                Dim nRelId As Integer
                Dim nParId As Integer
                'Dim oDbh As New dbHelper(myWeb)
                Dim bResult As Boolean = False

                Try
                    Dim oTmpNode As XmlElement = moXformElmt.SelectSingleNode("model/instance/tblContent/nContentKey")
                    If Not oTmpNode Is Nothing Then
                        If IsNumeric(oTmpNode.InnerText) Then nParId = CInt(oTmpNode.InnerText)
                        For Each myItem In goRequest.Form.Keys
                            'ok, we need to check through all the things that would require a save first, 
                            'save, then do the action
                            '###############################-SAVE IF NEEDED-########################
                            If myItem.startswith("Relate") Or myItem.startswith("ewSubmitClone_Relate") Then
                                'if it has no id then its a new piece of content
                                'we need to check and save
                                ' If nParId = 0 Then
                                Dim bCascade As Boolean
                                MyBase.updateInstanceFromRequest()
                                MyBase.validate()
                                If MyBase.valid Then
                                    'trim the contentName to no longer than 255 chars
                                    MyBase.Instance.SelectSingleNode("*/cContentName").InnerXml() = Left(MyBase.Instance.SelectSingleNode("*/cContentName").InnerXml, 255)
                                    If MyBase.Instance.SelectSingleNode("*/bCascade").InnerXml = "true" Then
                                        bCascade = True
                                    End If
                                    If nParId > 0 Then
                                        moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, MyBase.Instance)
                                    Else
                                        nParId = moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, MyBase.Instance)
                                        moDbHelper.setContentLocation(pgid, nParId, True, bCascade)
                                        If goSession("mcRelAction") = "Add" Or goSession("mcRelAction") = "Find" Then
                                            moDbHelper.insertContentRelation(goSession("mcRelParent"), nParId)
                                        End If
                                    End If
                                End If
                                'End If
                                'now there should be an id if all is well
                                If nParId = 0 Then Return False

                                'remove ewSubmitClone because it gets added by js disablebutton
                                Dim relateCmdArr() As String = Split(Replace(myItem, "ewSubmitClone_", ""), "_")

                                '###############################-REORDER-########################
                                goSession("mnContentRelationParent") = Nothing
                                goSession("mcRelRedirectString") = Nothing
                                goSession("mcRelAction") = Nothing
                                goSession("mcRelParent") = Nothing
                                goSession("mcRelType") = Nothing


                                Dim pgidQueryString As String = IIf(goRequest.QueryString("pgid") = "", "", "&pgid=" & goRequest.QueryString("pgid"))

                                If myItem.contains("RelateUp") Then
                                    nRelId = relateCmdArr(1)
                                    myWeb.moDbHelper.ReorderContent(nParId, nRelId, "MoveUp", True)
                                    bResult = True
                                ElseIf myItem.contains("RelateDown") Then
                                    nRelId = relateCmdArr(1)
                                    myWeb.moDbHelper.ReorderContent(nParId, nRelId, "MoveDown", True)
                                    bResult = True
                                ElseIf myItem.contains("RelateTop") Then
                                    nRelId = relateCmdArr(1)
                                    myWeb.moDbHelper.ReorderContent(nParId, nRelId, "MoveTop", True)
                                    bResult = True
                                ElseIf myItem.contains("RelateBottom") Then
                                    nRelId = relateCmdArr(1)
                                    myWeb.moDbHelper.ReorderContent(nParId, nRelId, "MoveBottom", True)
                                    bResult = True
                                    '###############################-ACTIONS-########################
                                ElseIf myItem.contains("RelateEdit") Then
                                    nRelId = relateCmdArr(1)
                                    goSession("mnContentRelationParent") = "/" & myWeb.moConfig("ProjectPath") & goRequest.QueryString("Path") & "?ewCmd=EditContent&id=" & nParId & IIf(goRequest.QueryString("pgid") = "", "", "&pgid=" & goRequest.QueryString("pgid"))
                                    goSession("mcRelRedirectString") = "/" & myWeb.moConfig("ProjectPath") & goRequest.QueryString("Path") & "?ewCmd=EditContent&id=" & nRelId
                                    bResult = True
                                    Exit For
                                ElseIf myItem.contains("RelateRemove") Then
                                    nRelId = relateCmdArr(1)
                                    myWeb.moDbHelper.RemoveContentRelation(nParId, nRelId)
                                    bResult = True
                                    Exit For
                                ElseIf myItem.contains("RelateAdd") Then

                                    goSession("mnContentRelationParent") = "/" & myWeb.moConfig("ProjectPath") & goRequest.QueryString("Path") & "?ewCmd=EditContent&id=" & nParId & pgidQueryString

                                    Dim cContentType As String = relateCmdArr(1)
                                    If (relateCmdArr.Length > 3) Then
                                        goSession("mcRelRedirectString") = "/" & myWeb.moConfig("ProjectPath") & goRequest.QueryString("Path") & "?ewCmd=AddContent&type=" & cContentType & "&name=New+" & cContentType & "&direction=" & relateCmdArr(2) & "&RelType=" & relateCmdArr(2) & "&relationType=" & relateCmdArr(3) & pgidQueryString
                                    Else
                                        goSession("mcRelRedirectString") = "/" & myWeb.moConfig("ProjectPath") & goRequest.QueryString("Path") & "?ewCmd=AddContent&type=" & cContentType & "&name=New+" & cContentType & "&direction=" & relateCmdArr(2) & "&RelType=" & relateCmdArr(2) & pgidQueryString
                                    End If
                                    goSession("mcRelAction") = "Add"
                                    goSession("mcRelParent") = nParId
                                    bResult = True
                                    Exit For
                                ElseIf myItem.contains("RelateFind") Then
                                    goSession("mnContentRelationParent") = "/" & myWeb.moConfig("ProjectPath") & goRequest.QueryString("Path") & "?ewCmd=EditContent&id=" & nParId & pgidQueryString
                                    Dim cContentType As String = relateCmdArr(1)
                                    If (relateCmdArr.Length > 3) Then
                                        goSession("mcRelRedirectString") = "/" & myWeb.moConfig("ProjectPath") & goRequest.QueryString("Path") & "?ewCmd=RelateSearch&type=" & cContentType & "&direction=" & relateCmdArr(2) & "&RelType=" & relateCmdArr(2) & "&relationType=" & relateCmdArr(3) & pgidQueryString
                                    Else
                                        goSession("mcRelRedirectString") = "/" & myWeb.moConfig("ProjectPath") & goRequest.QueryString("Path") & "?ewCmd=RelateSearch&type=" & cContentType & "&direction=" & relateCmdArr(2) & "&RelType=" & relateCmdArr(2) & pgidQueryString
                                    End If
                                    goSession("mcRelAction") = "Find"
                                    goSession("mcRelParent") = nParId
                                    bResult = True
                                    Exit For
                                End If
                            End If
                        Next
                    End If

                    If bResult Then
                        'Reload Related Content
                        If Not MyBase.Instance.SelectSingleNode("ContentRelations") Is Nothing Then
                            MyBase.Instance.RemoveChild(MyBase.Instance.SelectSingleNode("ContentRelations"))
                        End If

                        Dim oCRNode As XmlElement
                        oCRNode = moPageXML.CreateElement("ContentRelations")
                        moDbHelper.addRelatedContent(oCRNode, nParId, True)
                        MyBase.Instance.AppendChild(oCRNode)
                    End If

                    Return bResult
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "addInput", ex, "", "", gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDeleteContent(ByVal artid As Long) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim sContentName As String
                Dim sContentSchemaName As String

                Dim cProcessInfo As String = ""

                Try
                    'load the xform to be edited
                    moDbHelper.moPageXml = moPageXML

                    sContentName = moDbHelper.getNameByKey(dbHelper.objectTypes.Content, artid)
                    sContentSchemaName = moDbHelper.getContentType(artid)

                    MyBase.NewFrm("DeleteContent")

                    MyBase.submission("DeleteContent", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "DeleteItem", "", "Delete Content")

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to delete this item - """ & Tools.Xml.encodeAllHTML(sContentName) & """", , "alert-danger")
                    If sContentSchemaName = "xFormQuiz" Then
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "By deleting the Exam you will also delete all the user results from the database ""ARE YOU SURE"" !", , "alert-danger")

                    End If
                    MyBase.addSubmit(oFrmElmt, "", "Delete " & sContentSchemaName, , "principle btn-danger", "fa-trash-o")

                    MyBase.Instance.InnerXml = "<delete/>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            'remove the relevent content information
                            moDbHelper.DeleteObject(dbHelper.objectTypes.Content, artid)

                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDeleteBulkContent(ByVal ParamArray artid() As String) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim sContentName As String
                Dim sContentSchemaName As String

                Dim cProcessInfo As String = ""
                Dim bulkContentName As String = ""
                Dim bulkContentSchemaName As String = ""

                Try
                    'load the xform to be edited
                    moDbHelper.moPageXml = moPageXML

                    MyBase.NewFrm("DeleteContent")

                    MyBase.submission("DeleteContent", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "DeleteItem", "", "Delete Content")
                    MyBase.addNote(oFrmElmt, noteTypes.Alert, "Are you sure you want to delete below items ?", , "alert-error")
                    For i As Integer = 0 To UBound(artid)
                        sContentName = moDbHelper.getNameByKey(dbHelper.objectTypes.Content, artid(i))
                        sContentSchemaName = moDbHelper.getContentType(artid(i))
                        bulkContentName = Tools.Xml.encodeAllHTML(sContentName)
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, bulkContentName, , "alert-danger")
                        If sContentSchemaName = "xFormQuiz" Then
                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "By deleting the Exam you will also delete all the user results from the database ""ARE YOU SURE"" !", , "alert-danger")
                        End If
                        bulkContentSchemaName = Tools.Xml.encodeAllHTML(sContentSchemaName) & " , "
                    Next i
                    bulkContentSchemaName = bulkContentSchemaName.Trim(" ").Trim(",").Trim(" ")
                    MyBase.addSubmit(oFrmElmt, "", "Delete Products", , "principle btn-danger", "fa-trash-o")

                    MyBase.Instance.InnerXml = "<delete/>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            'remove the relevent content information
                            For i As Integer = 0 To UBound(artid)
                                sContentName = moDbHelper.getNameByKey(dbHelper.objectTypes.Content, artid(i))
                                sContentSchemaName = moDbHelper.getContentType(artid(i))
                                moDbHelper.DeleteObject(dbHelper.objectTypes.Content, artid(i))
                            Next i



                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDeleteFolder(ByRef cPath As String, ByVal nType As fsHelper.LibraryType) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim sValidResponse As String
                Dim cProcessInfo As String = ""

                Try
                    'load the xform to be edited
                    moDbHelper.moPageXml = moPageXML


                    MyBase.NewFrm("DeleteFolder")

                    MyBase.submission("DeleteFolder", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "folderItem", "", "Delete Content")

                    If cPath = "" Or cPath = "\" Or cPath = "/" Then
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "You cannot delete the root folder", , "alert-danger")
                    Else
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to delete this folder? - """ & cPath & """", , "alert-danger")
                        MyBase.addSubmit(oFrmElmt, "", "Delete folder")
                    End If

                    MyBase.Instance.InnerXml = "<delete/>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If goRequest("cFolderName") = "" Or goRequest("cFolderName") = "\" Or goRequest("cFolderName") = "/" Then
                            MyBase.valid = False
                        End If
                        If MyBase.valid Then

                            Dim oFs As fsHelper = New fsHelper
                            oFs.initialiseVariables(nType)
                            sValidResponse = oFs.DeleteFolder(goRequest("cFolderName"), cPath)

                            ' fsh.DeleteFolder()
                            ' cPath = Left(cPath, InStrRev(cPath, "\") - 1)
                            If sValidResponse <> "1" Then
                                MyBase.valid = False
                                MyBase.addNote(oFrmElmt, noteTypes.Alert, sValidResponse)
                                MyBase.addValues()
                            End If
                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmDeleteFolder", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function
            Public Function xFrmDeleteFile(ByVal cPath As String, ByVal cName As String, ByVal nType As fsHelper.LibraryType) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim sValidResponse As String
                Dim cProcessInfo As String = ""

                Try
                    'load the xform to be edited
                    moDbHelper.moPageXml = moPageXML


                    MyBase.NewFrm("DeleteFile")

                    MyBase.submission("DeleteFile", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "folderItem", "", "Delete File")

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "This file is used in these content Items")
                    'search for file in content and pages
                    Dim oFsh As fsHelper = New fsHelper
                    oFsh.initialiseVariables(nType)
                    Dim fileToFind As String = "/" & oFsh.mcRoot & cPath.Replace("\", "/") & "/" & cName
                    Dim sSQL As String = "select * from tblContent where cContentXmlBrief like '%" & fileToFind & "%' or cContentXmlDetail like '%" & fileToFind & "%'"
                    Dim odr As SqlDataReader = moDbHelper.getDataReader(sSQL)
                    If odr.HasRows Then
                        Dim contentFound As String = "<p>This file is used in these content Items</p><ul>"
                        Do While odr.Read
                            contentFound = contentFound + "<li><a href=""?artid=" & odr("nContentKey") & """ target=""_new"">" & odr("cContentSchemaName") & " - " & odr("cContentName") & "</a></li>"
                        Loop
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, contentFound & "</ul>")

                    Else
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "This cannot be found referenced in any content but it may be used in a template or stylesheet")
                    End If
                    odr = Nothing

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to delete this file? - """ & cPath & "\" & cName & """", , "alert-danger")

                    MyBase.addSubmit(oFrmElmt, "", "Delete file")

                    MyBase.Instance.InnerXml = "<delete/>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            Dim oFs As fsHelper = New fsHelper
                            oFs.initialiseVariables(nType)

                            sValidResponse = oFs.DeleteFile(cPath, cName)
                            If sValidResponse <> "1" Then
                                MyBase.valid = False
                                MyBase.addNote(oFrmElmt, noteTypes.Alert, sValidResponse)
                                MyBase.addValues()
                            End If

                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmMoveFile(ByVal cPath As String, ByVal cName As String, ByVal nType As fsHelper.LibraryType) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim sValidResponse As String
                Dim cProcessInfo As String = ""
                Try
                    'load the xform to be edited
                    moDbHelper.moPageXml = moPageXML
                    MyBase.NewFrm("MoveFile")
                    MyBase.submission("MoveFile", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "folderItem", "", "Move File")

                    'search for file in content and pages
                    Dim oFsh As fsHelper = New fsHelper
                    oFsh.initialiseVariables(nType)

                    Dim fileToFind As String = "/" & oFsh.mcRoot & cPath.Replace("\", "/").Replace("//", "/")

                    If fileToFind.EndsWith("/") Then
                        fileToFind = fileToFind & cName
                    Else
                        fileToFind = fileToFind & "/" & cName
                    End If

                    Dim sSQL As String = "select * from tblContent where cContentXmlBrief like '%" & fileToFind & "%' or cContentXmlDetail like '%" & fileToFind & "%'"
                    Dim odr As SqlDataReader = moDbHelper.getDataReader(sSQL)
                    If odr Is Nothing Then
                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "This cannot be found referenced in any content but it may be used in a template or stylesheet")
                    Else

                        If odr.HasRows Then
                            Dim contentFound As String = "<p>This file is used in these content Items</p><ul>"
                            Dim artIds As String = ""
                            Do While odr.Read
                                contentFound = contentFound + "<li><a href=""?artid=" & odr("nContentKey") & """ target=""_new"">" & odr("cContentSchemaName") & " - " & odr("cContentName") & "</a></li>"
                                artIds = odr("nContentKey") & ","
                            Loop
                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, contentFound & "</ul>")

                            Dim oSelUpd As XmlElement = MyBase.addSelect1(oFrmElmt, "UpdatePaths", False, "Update Paths", "", xForm.ApperanceTypes.Full)
                            MyBase.addOption(oSelUpd, "Yes", artIds.TrimEnd(","))
                            MyBase.addOption(oSelUpd, "No", "0")
                        Else
                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "This cannot be found referenced in any content but it may be used in a template or stylesheet")
                        End If
                    End If
                    odr = Nothing

                    Dim oSelElmt As XmlElement = MyBase.addSelect1(oFrmElmt, "destPath", False, "Move To")
                    MyBase.addOptionsFoldersFromDirectory(oSelElmt, "/" & oFsh.mcRoot)

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to move this file? - """ & cPath & "\" & cName & """", , "alert-danger")



                    MyBase.addSubmit(oFrmElmt, "", "Move file")

                    MyBase.Instance.InnerXml = "<delete/>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            Dim oFs As fsHelper = New fsHelper
                            oFs.initialiseVariables(nType)
                            Dim cDestPath As String = myWeb.moRequest("destPath").Replace(oFs.mcRoot, "").Replace("//", "/")

                            If oFs.MoveFile(cName, cPath, cDestPath) Then

                                If myWeb.moRequest("UpdatePaths") <> "0" And myWeb.moRequest("UpdatePaths") <> "" Then
                                    Dim fileToReplace As String = "/" & oFs.mcRoot & cDestPath.Replace("\", "/") & "/" & cName.Replace(" ", "-")
                                    Dim sSQLUpd As String = "Update tblContent set cContentXmlBrief = REPLACE(CAST(cContentXmlBrief AS NVARCHAR(MAX)),'" & fileToFind & "','" & fileToReplace & "'), cContentXmlDetail = REPLACE(CAST(cContentXmlDetail AS NVARCHAR(MAX)),'" & fileToFind & "','" & fileToReplace & "') where nContentKey IN (" & myWeb.moRequest("UpdatePaths") & ")"
                                    moDbHelper.ExeProcessSql(sSQLUpd)
                                End If

                            Else
                                MyBase.valid = False
                                MyBase.addNote(oFrmElmt, noteTypes.Alert, "File move error")
                                MyBase.addValues()

                            End If


                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDeletePage(ByVal pgid As Long) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim sContentName As String
                Dim sContentSchemaName As String = ""

                Dim cProcessInfo As String = ""

                Try
                    'load the xform to be edited
                    moDbHelper.moPageXml = moPageXML

                    sContentName = moDbHelper.getNameByKey(dbHelper.objectTypes.ContentStructure, pgid)

                    MyBase.NewFrm("DeletePage")

                    MyBase.submission("DeleteContent", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "DeletePg", "", "Delete Page")

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "<h4>Are you sure you want to delete this page - """ & Tools.Xml.encodeAllHTML(sContentName) & """</h4><br/><br/>By deleting this page you will also delete <strong>ALL</strong> the child pages beneath <strong>ARE YOU SURE</strong> !", , "alert-danger")

                    MyBase.addSubmit(oFrmElmt, "", "Delete Page" & sContentSchemaName, , "btn-danger principle", "fa-trash")

                    MyBase.Instance.InnerXml = "<delete/>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            'remove the relevent content information
                            moDbHelper.DeleteObject(dbHelper.objectTypes.ContentStructure, pgid)

                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmAddFolder(ByRef cPath As String, ByVal nType As fsHelper.LibraryType) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim sValidResponse As String = ""
                Dim cProcessInfo As String = ""
                Try
                    MyBase.NewFrm("AddFolder")



                    MyBase.submission("AddFolder", "/?ewcmd=" & myWeb.moRequest("ewcmd") & "&ewCmd2=" & myWeb.moRequest("ewCmd2") & "&pathonly=" & myWeb.moRequest("pathonly") & "&targetForm=" & myWeb.moRequest("targetForm") & "&targetField=" & myWeb.moRequest("targetField"), "post", "")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "New Folder", "", "Please enter the folder name")
                    MyBase.addInput(oFrmElmt, "fld", True, "Path", "readonly")
                    MyBase.addBind("fld", "folder/@path", "false() ")

                    MyBase.addInput(oFrmElmt, "cFolderName", True, "Folder Name")
                    MyBase.addBind("cFolderName", "folder/@name", "true()")

                    MyBase.addSubmit(oFrmElmt, "AddFolder", "Create Folder", "ewSubmit")

                    MyBase.Instance.InnerXml = "<folder path=""" & cPath & """ name=""""/>"

                    If MyBase.isSubmitted Or moRequest("cFolderName") <> "" Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            Dim oFs As fsHelper = New fsHelper
                            oFs.initialiseVariables(nType)
                            sValidResponse = oFs.CreateFolder(HtmlDecode(goRequest("cFolderName")), cPath)

                            If IsNumeric(sValidResponse) Then
                                valid = True
                                cPath &= "\" & goRequest("cFolderName")
                            Else
                                valid = False
                                MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
                            End If
                        Else
                            valid = False
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmAddFolder", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmUpload(ByVal cPath As String, ByVal nType As fsHelper.LibraryType) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim sValidResponse As String = ""
                Dim cProcessInfo As String = ""
                Try
                    MyBase.NewFrm("UploadFile")

                    MyBase.submission("Upload File", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "New File", "", "Please select the file to upload")
                    MyBase.addInput(oFrmElmt, "fld", True, "Upload Path", "readonly")
                    MyBase.addBind("fld", "file/@path", "true()")

                    MyBase.addUpload(oFrmElmt, "uploadFile", True, "image/*", "Pick File")
                    MyBase.addBind("uploadFile", "file", "true()")

                    MyBase.addSubmit(oFrmElmt, "", "Upload", "ewSubmit")

                    MyBase.Instance.InnerXml = "<file path=""" & cPath & """ filename="""" mediatype=""""/>"

                    If MyBase.isSubmitted Then

                        MyBase.updateInstanceFromRequest()

                        'lets do some hacking 
                        Dim fUpld As System.Web.HttpPostedFile
                        fUpld = goRequest.Files("uploadFile")

                        If Not fUpld Is Nothing Then
                            MyBase.valid = True
                        End If

                        If MyBase.valid Then

                            Dim oFs As fsHelper = New fsHelper
                            oFs.initialiseVariables(nType)
                            sValidResponse = oFs.SaveFile(fUpld, cPath)

                            If sValidResponse = fUpld.FileName Then
                                valid = True
                                ' MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse & " - File Saved")
                            Else
                                valid = False
                                MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
                            End If
                        Else
                            valid = False
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmUpload", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmMultiUpload(ByVal cPath As String, ByVal nType As fsHelper.LibraryType) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim sValidResponse As String = ""
                Dim cProcessInfo As String = ""
                Dim rootDir As String = "\"
                Try

                    Select Case nType
                        Case fsHelper.LibraryType.Image
                            rootDir = myWeb.moConfig("ImageRootPath")
                        Case fsHelper.LibraryType.Documents
                            rootDir = myWeb.moConfig("DocRootPath")
                        Case fsHelper.LibraryType.Media
                            rootDir = myWeb.moConfig("MediaRootPath")
                    End Select

                    myWeb.moSession("allowUpload") = "True"

                    MyBase.NewFrm("UploadFile")

                    MyBase.submission("Upload File", "", "post", "")

                    cPath = cPath.Replace("\", "/")

                    If cPath.StartsWith("/") Then
                        cPath = cPath.Substring(1)
                    End If

                    Dim SavePath As String = rootDir & cPath

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "New File", "", "Please select files to upload to " & SavePath)
                    MyBase.addInput(oFrmElmt, "fld", True, "Upload Path", "hidden")
                    MyBase.addBind("fld", "file/@path", "true()")

                    MyBase.addUpload(oFrmElmt, "uploadFile", True, "image/*", "", "MultiPowUpload")
                    MyBase.addBind("uploadFile", "file", "true()")

                    MyBase.addSubmit(oFrmElmt, "", "Finish", "ewSubmit")

                    MyBase.Instance.InnerXml = "<file path=""" & SavePath & """ filename="""" mediatype=""""/>"

                    If MyBase.isSubmitted Then

                        'do nothing
                        valid = True
                        myWeb.moSession("allowUpload") = Nothing

                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmMultiUpload", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmPickImage(ByVal cPathName As String, ByVal cTargetForm As String, ByVal cTargetFeild As String, Optional ByVal cClassName As String = "") As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oElmt As XmlElement
                Dim sValidResponse As String = "0"
                Dim cProcessInfo As String = ""
                Try
                    If cTargetForm = "" Then cTargetForm = "ContentForm"

                    MyBase.NewFrm("AddFolder")

                    MyBase.submission("imageDetailsForm", "", "post", "form_check(this);passImgToForm('" & cTargetForm & "','" & cTargetFeild & "');return(false);")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Image Details", "", "Please enter image description")

                    MyBase.addInput(oFrmElmt, "cName", True, "Class", "readonly")
                    MyBase.addBind("cName", "img/@class", "true()")

                    MyBase.addInput(oFrmElmt, "cPathName", True, "Path Name")
                    MyBase.addBind("cPathName", "img/@src", "true()")

                    MyBase.addInput(oFrmElmt, "nWidth", True, "Width")
                    MyBase.addBind("nWidth", "img/@width", "true()")

                    MyBase.addInput(oFrmElmt, "nHeight", True, "Height")
                    MyBase.addBind("nHeight", "img/@height", "true()")

                    MyBase.addInput(oFrmElmt, "cDesc", True, "Alt Description")
                    MyBase.addBind("cDesc", "img/@alt", "false()")


                    MyBase.addSubmit(oFrmElmt, "", "Add Image", "ewSubmit")

                    Dim oFs As fsHelper = New fsHelper
                    oFs.initialiseVariables(fsHelper.LibraryType.Image)
                    MyBase.Instance.InnerXml = oFs.getImageXhtml(cPathName)


                    If cClassName <> "" Then
                        oElmt = MyBase.Instance.FirstChild()
                        oElmt.SetAttribute("class", cClassName)
                    End If

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            If IsNumeric(sValidResponse) Then
                                valid = True
                                'MyBase.jsOnLoad()
                            Else
                                valid = False
                                MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
                            End If
                        Else
                            valid = False
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmPickImage", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            'Public Function xFrmPickDocument(ByVal cPathName As String, ByVal cTargetFeild As String, Optional ByVal cClassName As String = "") As XmlElement
            '    Dim oFrmElmt As XmlElement
            '    Dim oElmt As XmlElement
            '    Dim sValidResponse As String = ""
            '    Dim cProcessInfo As String = ""
            '    Try
            '        MyBase.NewFrm("AddFolder")

            '        MyBase.submission("documentDetailsForm", "", "post", "form_check(this);passDocToForm('" & cTargetFeild & "');return(false);")

            '        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Document Details", "", "Please enter image description")

            '        MyBase.addInput(oFrmElmt, "cPathName", True, "Path Name")
            '        MyBase.addBind("cPathName", "a/@href", "true()")

            '        MyBase.addSubmit(oFrmElmt, "", "Add Document ", "ewSubmit", "ewSubmit")


            '        MyBase.instance.InnerXml = "<a href=""" & Replace(cPathName, "\", "/") & """/>"


            '        If cClassName <> "" Then
            '            oElmt = MyBase.instance.FirstChild()
            '            oElmt.SetAttribute("class", cClassName)
            '        End If

            '        'auto submit-
            '        MyBase.submit()


            '        '------------

            '        If MyBase.isSubmitted Then
            '            MyBase.updateInstanceFromRequest()
            '            MyBase.validate()
            '            If MyBase.valid Then

            '                If IsNumeric(sValidResponse) Then
            '                    valid = True
            '                    'MyBase.jsOnLoad()
            '                Else
            '                    valid = False
            '                    MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
            '                End If
            '            Else
            '                valid = False
            '            End If
            '        End If

            '        MyBase.addValues()
            '        Return MyBase.moXformElmt

            '    Catch ex As Exception
            '        returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
            '        Return Nothing
            '    End Try
            'End Function

            Public Function xFrmEditImage(ByVal cImgHtml As String, ByVal cTargetForm As String, ByVal cTargetFeild As String, Optional ByVal cClassName As String = "") As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oElmt As XmlElement
                Dim sValidResponse As String = ""
                Dim cProcessInfo As String = ""
                Try
                    MyBase.NewFrm("EditImage")

                    MyBase.Instance.InnerXml = cImgHtml.Replace(""">", """/>").Replace("&", "&amp;")
                    If cClassName = "" Then
                        oElmt = MyBase.Instance.FirstChild
                        cClassName = oElmt.GetAttribute("class")
                    End If

                    MyBase.submission("imageDetailsForm", "", "post", "form_check(this);passImgToForm('" & cTargetForm & "','" & cTargetFeild & "');return(false);")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Image Details", "", "Please enter image description")

                    MyBase.addInput(oFrmElmt, "cName", True, "Class", "readonly")
                    MyBase.addBind("cName", "img/@class", "true()")

                    MyBase.addInput(oFrmElmt, "cPathName", True, "Path Name")
                    MyBase.addBind("cPathName", "img/@src", "true()")

                    MyBase.addInput(oFrmElmt, "nWidth", True, "Width")
                    MyBase.addBind("nWidth", "img/@width", "true()")

                    MyBase.addInput(oFrmElmt, "nHeight", True, "Height")
                    MyBase.addBind("nHeight", "img/@height", "true()")

                    MyBase.addInput(oFrmElmt, "cDesc", True, "Alt Description")
                    MyBase.addBind("cDesc", "img/@alt", "false()")
                    MyBase.addDiv(oFrmElmt, "<a href=""?contentType=popup&amp;ewCmd=ImageLib&amp;targetField=" & cTargetFeild & "&amp;targetClass=" & cClassName & """ class=""btn btn-primary pull-right""><i class=""fa fa-picture-o""> </i> Pick New Image</a>", "")
                    MyBase.addSubmit(oFrmElmt, "", "Update Image", "ewSubmit", "ewSubmit")

                    If cClassName <> "" Then
                        oElmt = MyBase.Instance.FirstChild()
                        oElmt.SetAttribute("class", cClassName)
                    End If

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            If IsNumeric(sValidResponse) Then
                                valid = True
                                'MyBase.jsOnLoad()
                            Else
                                valid = False
                                MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
                            End If
                        Else
                            valid = False
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditImage", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            ''' <summary>
            ''' this routine now calls the Membership provider for this function as it has moved. It should only be being used by legacy overides.
            ''' </summary>
            ''' <param name="id"></param>
            ''' <param name="cDirectorySchemaName"></param>
            ''' <param name="parId"></param>
            ''' <param name="cXformName"></param>
            ''' <param name="FormXML"></param>
            ''' <returns></returns>
            ''' <remarks></remarks>

            <Obsolete("Don't use this routine any more. Use the new one in Membership Provider ", False)>
            Public Overridable Function xFrmEditDirectoryItem(Optional ByVal id As Long = 0, Optional ByVal cDirectorySchemaName As String = "User", Optional ByVal parId As Long = 0, Optional ByVal cXformName As String = "", Optional ByVal FormXML As String = "") As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                    Dim oAdXfm As Object = oMembershipProv.AdminXforms

                    oAdXfm.xFrmEditDirectoryItem(id, cDirectorySchemaName, parId, cXformName, FormXML)

                    Me.valid = oAdXfm.valid
                    Me.moXformElmt = oAdXfm.moXformElmt
                    Me.updateInstance(oAdXfm.Instance)
                    Return Me.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditDirectoryItem", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Overridable Function xFrmCopyGroupMembers(ByVal dirId As Long) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oElmt As XmlElement
                Dim oElmt2 As XmlElement
                Dim oElmt3 As XmlElement
                Dim sSql As String
                Dim oDr As SqlDataReader
                Dim cProcessInfo As String = ""
                Dim sType As String = "Group"

                Try
                    'load the directory item to be deleted
                    moDbHelper.moPageXml = moPageXML
                    MyBase.NewFrm("CopyGroupMembers")

                    'Lets get the object
                    oElmt = moPageXML.CreateElement("sType")
                    oElmt.SetAttribute("id", dirId)
                    If dirId <> 0 Then
                        oDr = moDbHelper.getDataReader("SELECT * FROM tblDirectory where nDirKey = " & dirId)
                        While oDr.Read
                            oElmt.SetAttribute("name", oDr("cDirName"))
                            If oDr("cDirXml") <> "" Then
                                oElmt.InnerXml = oDr("cDirXml")
                            Else
                                oElmt.InnerXml = Instance.SelectSingleNode("*").InnerXml
                            End If
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'get item parents
                        sSql = "SELECT d.* FROM tblDirectory d " &
                        "inner join tblDirectoryRelation r on r.nDirParentId = d.nDirKey " &
                        "where r.nDirChildId = " & dirId

                        oDr = moDbHelper.getDataReader(sSql)
                        While oDr.Read
                            oElmt2 = moPageXML.CreateElement(oDr("cDirSchema"))
                            oElmt2.SetAttribute("id", oDr("nDirKey"))
                            oElmt2.SetAttribute("name", oDr("cDirName"))
                            oElmt2.SetAttribute("relType", "child")
                            MyBase.Instance.AppendChild(oElmt2)
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'get item Children
                        sSql = "SELECT d.* FROM tblDirectory d " &
                        "inner join tblDirectoryRelation r on r.nDirChildId = d.nDirKey " &
                        "where r.nDirParentId = " & dirId

                        oDr = moDbHelper.getDataReader(sSql)
                        While oDr.Read
                            oElmt3 = moPageXML.CreateElement(oDr("cDirSchema"))
                            oElmt3.SetAttribute("id", oDr("nDirKey"))
                            oElmt3.SetAttribute("name", oDr("cDirName"))
                            oElmt3.SetAttribute("relType", "parent")
                            MyBase.Instance.AppendChild(oElmt3)
                        End While
                        oDr.Close()
                        oDr = Nothing

                    End If

                    MyBase.Instance.AppendChild(oElmt)

                    MyBase.submission("EditInput", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "MoveDirMembers", "", "Copy " & sType & " Members")

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to copy this " & sType & " Members " & Tools.Xml.encodeAllHTML(oElmt.GetAttribute("name")), , "alert-danger")

                    'Lets get all other groups
                    oElmt3 = MyBase.addSelect1(oFrmElmt, sType & "CopyTo", False, "Copy " & sType & " Members To", "scroll_10", xForm.ApperanceTypes.Minimal)
                    sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='" & sType & "' and d.nDirKey<>" & dirId & " order by cDirName"
                    MyBase.addOptionsFromSqlDataReader(oElmt3, moDbHelper.getDataReader(sSql), "name", "value")

                    MyBase.addSubmit(oFrmElmt, "", "Copy " & sType)

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            'select all child relations so child objects don't get deleted
                            sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " & dirId
                            oDr = moDbHelper.getDataReader(sSql)
                            'Loop through 1 behind so we can trigger sync on last one.
                            Dim previousId As Long = 0
                            While oDr.Read
                                If Not previousId = 0 Then
                                    moDbHelper.maintainDirectoryRelation(moRequest("GroupCopyTo"), previousId, False,, True,,, False)
                                End If
                                previousId = oDr(1)
                            End While
                            moDbHelper.maintainDirectoryRelation(moRequest("GroupCopyTo"), previousId, False,, True,,, True)

                            oDr.Close()
                            oDr = Nothing

                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmCopyGroupMembers", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Overridable Function xFrmEditRole(ByVal id As Long) As XmlElement


                Dim cProcessInfo As String = ""
                Dim cCurrentPassword As String = ""
                Dim cCodeUsed As String = ""
                Dim addNewitemToParId As Boolean = False
                Dim cDirectorySchemaName As String = "role"
                Dim cXformName As String = ""
                Dim oElmt As XmlElement

                Try

                    If id > 0 Then
                        If cXformName = "" Then cXformName = cDirectorySchemaName

                        ' ok lets load in an xform from the file location.

                        If Not MyBase.load("/xforms/directory/" & cXformName & ".xml", myWeb.maCommonFolders) Then
                            ' load a default content xform if no alternative.

                        End If

                        MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, id)

                        Dim oRoleRights As XmlElement = MyBase.Instance.SelectSingleNode("tblDirectory/cDirXml/Role/AdminRights")

                        Dim siteRights As XmlElement = moPageXML.SelectSingleNode("/Page/AdminMenu")

                        If oRoleRights Is Nothing Then
                            oRoleRights = moPageXML.CreateElement("AdminRights")
                            oRoleRights.InnerXml = siteRights.InnerXml
                            For Each oElmt In oRoleRights.SelectNodes("descendant-or-self::MenuItem")
                                oElmt.SetAttribute("adminRight", "")
                            Next
                            MyBase.Instance.SelectSingleNode("tblDirectory/cDirXml/Role").AppendChild(oRoleRights)
                        Else
                            Dim oRoleRights2 As XmlElement
                            oRoleRights2 = moPageXML.CreateElement("AdminRights")
                            oRoleRights2.InnerXml = siteRights.InnerXml
                            For Each oElmt In oRoleRights2.SelectNodes("descendant-or-self::MenuItem")
                                If Not oRoleRights.SelectSingleNode("descendant-or-self::MenuItem[@cmd='" & oElmt.GetAttribute("cmd") & "' and @adminRight='true']") Is Nothing Then
                                    oElmt.SetAttribute("adminRight", "true")
                                Else
                                    oElmt.SetAttribute("adminRight", "")
                                End If

                            Next
                            'remove the old admin rights
                            For Each oElmt In Instance.SelectNodes("tblDirectory/cDirXml/Role/AdminRights")
                                oElmt.ParentNode.RemoveChild(oElmt)
                            Next
                            'paste in new and updated
                            MyBase.Instance.SelectSingleNode("tblDirectory/cDirXml/Role").AppendChild(oRoleRights2)
                        End If

                        oRoleRights = MyBase.Instance.SelectSingleNode("tblDirectory/cDirXml/Role/AdminRights")

                        Dim oElmt2 As XmlElement
                        Dim oElmt3 As XmlElement
                        Dim oElmt4 As XmlElement
                        Dim oElmt5 As XmlElement
                        Dim oSelElmt As XmlElement
                        Dim oGrpRoot As XmlElement = moXformElmt.SelectSingleNode("group[@class='2Col']/group[2]")
                        If oGrpRoot Is Nothing Then oGrpRoot = moXformElmt
                        Dim oGrp As XmlElement = MyBase.addGroup(oGrpRoot, "adminRights", "adminRights", "Admin Rights")
                        Dim oGrp2 As XmlElement
                        Dim oGrp3 As XmlElement
                        Dim oGrp4 As XmlElement
                        Dim oGrp5 As XmlElement
                        Dim nCount As Long = 1
                        Dim sRef As String
                        Dim oBind As XmlElement
                        Dim oBind1 As XmlElement
                        Dim oBind2 As XmlElement
                        Dim oBind3 As XmlElement
                        Dim oBind4 As XmlElement

                        For Each oElmt In oRoleRights.SelectNodes("MenuItem")
                            sRef = "adminRight" & nCount
                            oSelElmt = MyBase.addSelect(oGrp, sRef, True, "", , Protean.xForm.ApperanceTypes.Full)
                            MyBase.addOption(oSelElmt, oElmt.GetAttribute("name"), "true")
                            oBind = MyBase.addBind("", "tblDirectory/cDirXml/Role/AdminRights/MenuItem[@cmd='" & oElmt.GetAttribute("cmd") & "']")
                            MyBase.addBind(sRef, "@adminRight", , , oBind)
                            nCount = nCount + 1
                            If oElmt.SelectNodes("MenuItem").Count > 0 Then
                                oGrp2 = MyBase.addGroup(oGrp, oElmt.GetAttribute("name"), "grp" & nCount)
                                For Each oElmt2 In oElmt.SelectNodes("MenuItem")
                                    sRef = "adminRight" & nCount
                                    oSelElmt = MyBase.addSelect(oGrp2, sRef, True, "", , Protean.xForm.ApperanceTypes.Full)
                                    MyBase.addOption(oSelElmt, oElmt2.GetAttribute("name"), "true")
                                    oBind1 = MyBase.addBind("", "MenuItem[@cmd='" & oElmt2.GetAttribute("cmd") & "']", , , oBind)
                                    MyBase.addBind(sRef, "@adminRight", , , oBind1)
                                    nCount = nCount + 1
                                    If oElmt2.SelectNodes("MenuItem").Count > 0 Then
                                        oGrp3 = MyBase.addGroup(oGrp2, oElmt2.GetAttribute("name"), "grp" & nCount)
                                        For Each oElmt3 In oElmt2.SelectNodes("MenuItem")
                                            sRef = "adminRight" & nCount
                                            oSelElmt = MyBase.addSelect(oGrp3, sRef, True, "", , Protean.xForm.ApperanceTypes.Full)
                                            MyBase.addOption(oSelElmt, oElmt3.GetAttribute("name"), "true")
                                            oBind2 = MyBase.addBind("", "MenuItem[@cmd='" & oElmt3.GetAttribute("cmd") & "']", , , oBind1)
                                            MyBase.addBind(sRef, "@adminRight", , , oBind2)
                                            nCount = nCount + 1
                                            If oElmt3.SelectNodes("MenuItem").Count > 0 Then
                                                oGrp4 = MyBase.addGroup(oGrp3, oElmt3.GetAttribute("name"), "grp" & nCount)
                                                For Each oElmt4 In oElmt3.SelectNodes("MenuItem")
                                                    sRef = "adminRight" & nCount
                                                    oSelElmt = MyBase.addSelect(oGrp4, sRef, True, "", , Protean.xForm.ApperanceTypes.Full)
                                                    MyBase.addOption(oSelElmt, oElmt4.GetAttribute("name"), "true")
                                                    oBind3 = MyBase.addBind("", "MenuItem[@cmd='" & oElmt4.GetAttribute("cmd") & "']", , , oBind2)
                                                    MyBase.addBind(sRef, "@adminRight", , , oBind3)
                                                    nCount = nCount + 1
                                                    If oElmt4.SelectNodes("MenuItem").Count > 0 Then
                                                        oGrp5 = MyBase.addGroup(oGrp4, oElmt4.GetAttribute("name"), "grp" & nCount)
                                                        For Each oElmt5 In oElmt4.SelectNodes("MenuItem")
                                                            sRef = "adminRight" & nCount
                                                            oSelElmt = MyBase.addSelect(oGrp5, sRef, True, "", , Protean.xForm.ApperanceTypes.Full)
                                                            MyBase.addOption(oSelElmt, oElmt5.GetAttribute("name"), "true")
                                                            oBind4 = MyBase.addBind("", "MenuItem[@cmd='" & oElmt5.GetAttribute("cmd") & "']", , , oBind3)
                                                            MyBase.addBind(sRef, "@adminRight", , , oBind4)
                                                            nCount = nCount + 1
                                                        Next
                                                    End If
                                                Next
                                            End If
                                        Next
                                    End If
                                Next
                            End If
                        Next

                        cDirectorySchemaName = MyBase.Instance.SelectSingleNode("tblDirectory/cDirSchema").InnerText

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            'any additonal validation goes here

                            If MyBase.valid Then

                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, MyBase.Instance, id)

                                MyBase.addNote("EditContent", xForm.noteTypes.Alert, "<span class=""msg-1010"">Your details have been updated.</span>", True)

                            End If
                        End If

                        MyBase.addValues()
                        Return MyBase.moXformElmt
                    End If
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditRole", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function



            Public Overridable Function xFrmDeleteDirectoryItem(ByVal dirId As Long, ByVal sType As String) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oElmt As XmlElement
                Dim oElmt2 As XmlElement
                Dim oElmt3 As XmlElement
                Dim sSql As String
                Dim oDr As SqlDataReader
                Dim cProcessInfo As String = ""


                Try
                    'load the directory item to be deleted
                    moDbHelper.moPageXml = moPageXML

                    MyBase.NewFrm("EditSelect")

                    'Lets get the object
                    oElmt = moPageXML.CreateElement("sType")
                    oElmt.SetAttribute("id", dirId)
                    If dirId <> 0 Then
                        oDr = moDbHelper.getDataReader("SELECT * FROM tblDirectory where nDirKey = " & dirId)
                        While oDr.Read
                            oElmt.SetAttribute("name", oDr("cDirName"))
                            If oDr("cDirXml") <> "" Then
                                oElmt.InnerXml = oDr("cDirXml")
                            Else
                                oElmt.InnerXml = Instance.SelectSingleNode("*").InnerXml
                            End If
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'get item parents
                        sSql = "SELECT d.* FROM tblDirectory d " &
                        "inner join tblDirectoryRelation r on r.nDirParentId = d.nDirKey " &
                        "where r.nDirChildId = " & dirId

                        oDr = moDbHelper.getDataReader(sSql)
                        While oDr.Read
                            oElmt2 = moPageXML.CreateElement(oDr("cDirSchema"))
                            oElmt2.SetAttribute("id", oDr("nDirKey"))
                            oElmt2.SetAttribute("name", oDr("cDirName"))
                            oElmt2.SetAttribute("relType", "child")
                            MyBase.Instance.AppendChild(oElmt2)
                        End While
                        oDr.Close()
                        oDr = Nothing

                        'get item Children
                        sSql = "SELECT d.* FROM tblDirectory d " &
                        "inner join tblDirectoryRelation r on r.nDirChildId = d.nDirKey " &
                        "where r.nDirParentId = " & dirId

                        oDr = moDbHelper.getDataReader(sSql)
                        While oDr.Read
                            oElmt3 = moPageXML.CreateElement(oDr("cDirSchema"))
                            oElmt3.SetAttribute("id", oDr("nDirKey"))
                            oElmt3.SetAttribute("name", oDr("cDirName"))
                            oElmt3.SetAttribute("relType", "parent")
                            MyBase.Instance.AppendChild(oElmt3)
                        End While
                        oDr.Close()
                        oDr = Nothing

                    End If

                    MyBase.Instance.AppendChild(oElmt)




                    MyBase.submission("EditInput", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "DeleteDir", "", "Delete " & sType)

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to delete this " & sType & " " & Tools.Xml.encodeAllHTML(oElmt.GetAttribute("name")), , "alert-danger")

                    Select Case sType
                        Case "User"
                            ' MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "What do you want to do with this " & sType & "s exam results?")

                            'oElmt2 = MyBase.addSelect1(oFrmElmt, "Options", False, "Exam Results", "", xForm.ApperanceTypes.Full)
                            'MyBase.addOption(oElmt2, "Delete All", "Delete")
                            'MyBase.addOption(oElmt2, "Transfer to another user in the same company", "Transfer")

                            'oElmt3 = MyBase.addSelect1(oFrmElmt, "TransUserId", False, "Select User", "scroll_10", xForm.ApperanceTypes.Minimal)

                            ''Lets get all the users in the same company

                            'sSql = "select d.nDirKey as value, d.cDirName as name " & _
                            '"FROM (((tblDirectory d " & _
                            '"inner join tblAudit a on nAuditId = a.nAuditKey " & _
                            '"INNER JOIN tblDirectoryRelation user2company1 " & _
                            '"ON d.nDirKey = user2company1.nDirChildId) " & _
                            '"INNER JOIN tblDirectory company " & _
                            '"ON company.nDirKey = user2company1.nDirParentId) " & _
                            '"INNER JOIN tblDirectoryRelation user2company2 " & _
                            '"ON company.nDirKey = user2company2.nDirParentId ) " & _
                            '"INNER JOIN tblDirectory users " & _
                            '"ON users.nDirKey = user2company2.nDirChildId " & _
                            '"left outer join tblDirectoryRelation dr on d.nDirKey = dr.nDirParentId and dr.nDirChildId = " & dirId & _
                            '"WHERE d.cDirSchema = 'User' AND company.cDirSchema = 'Company' AND users.cDirSchema = 'User' " & _
                            '"AND users.nDirKey = " & dirId & " and d.nDirKey<>" & dirId & " order by d.cDirName"

                            'MyBase.addOptionsFromSqlDataReader(oElmt3, oDt.getDataReader(sSql), "name", "value")

                        Case "Department"
                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "What do you want to do with this " & sType & "s users?", , "alert-danger")

                            oElmt2 = MyBase.addSelect1(oFrmElmt, "Options", False, "Users", "", xForm.ApperanceTypes.Full)
                            MyBase.addOption(oElmt2, "Just remove the Department relationship from the Users", "Remove")
                            MyBase.addOption(oElmt2, "Transfer Users to another Department in the same Company", "Transfer")

                            oElmt3 = MyBase.addSelect1(oFrmElmt, "TransDeptId", False, "Select User", "scroll_10", xForm.ApperanceTypes.Minimal)

                            'Lets get all the departments in the same company

                            sSql = "select d.nDirKey as value, d.cDirName as name " &
                            "FROM (((tblDirectory d " &
                            "inner join tblAudit a on nAuditId = a.nAuditKey " &
                            "INNER JOIN tblDirectoryRelation dept2company1 " &
                            "ON d.nDirKey = dept2company1.nDirChildId) " &
                            "INNER JOIN tblDirectory company " &
                            "ON company.nDirKey = dept2company1.nDirParentId) " &
                            "INNER JOIN tblDirectoryRelation user2company2 " &
                            "ON company.nDirKey = user2company2.nDirParentId ) " &
                            "INNER JOIN tblDirectory dept " &
                            "ON dept.nDirKey = user2company2.nDirChildId " &
                            "left outer join tblDirectoryRelation dr on d.nDirKey = dr.nDirParentId and dr.nDirChildId = " & dirId &
                            "WHERE d.cDirSchema = 'Department' AND company.cDirSchema = 'Company' AND dept.cDirSchema = 'Department' " &
                            "AND dept.nDirKey = " & dirId & " and d.nDirKey<>" & dirId & " order by d.cDirName"

                            MyBase.addOptionsFromSqlDataReader(oElmt3, moDbHelper.getDataReader(sSql), "name", "value")

                        Case "Company"

                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "What do you want to do with this " & sType & "s users?", , "alert-danger")

                            oElmt2 = MyBase.addSelect1(oFrmElmt, "Options", False, "Users / Departments", "", xForm.ApperanceTypes.Full)
                            MyBase.addOption(oElmt2, "Delete All Users/Departments", "Delete")
                            MyBase.addOption(oElmt2, "Just remove the Company relationship from the Users and delete Departments", "RemoveDept")
                            MyBase.addOption(oElmt2, "Transfer Users/Departments to another Company", "Transfer")

                            'Lets get all other companies
                            oElmt3 = MyBase.addSelect1(oFrmElmt, "TransCompanyId", False, "Select Departments", "scroll_10", xForm.ApperanceTypes.Minimal)
                            sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='Company' and d.nDirKey<>" & dirId & " order by cDirName"
                            MyBase.addOptionsFromSqlDataReader(oElmt3, moDbHelper.getDataReader(sSql), "name", "value")

                        Case Else '"Group", "Role"

                            oElmt2 = MyBase.addSelect1(oFrmElmt, "Options", False, "What do you want to do with this " & sType & "s members?", "", xForm.ApperanceTypes.Full)
                            MyBase.addOption(oElmt2, "Just remove the " & sType & " relationship from the members", "Remove")
                            MyBase.addOption(oElmt2, "Transfer members to another " & sType, "Transfer")

                            'Lets get all other groups
                            oElmt3 = MyBase.addSelect1(oFrmElmt, sType & "s", False, "Select Alternative " & sType, "scroll_10", xForm.ApperanceTypes.Minimal)
                            sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='" & sType & "' and d.nDirKey<>" & dirId & " order by cDirName"
                            MyBase.addOptionsFromSqlDataReader(oElmt3, moDbHelper.getDataReader(sSql), "name", "value")

                            'Case "Role"

                            '    oElmt2 = MyBase.addSelect1(oFrmElmt, "Options", False, "What do you want to do with this " & sType & "s users?", "", xForm.ApperanceTypes.Full)
                            '    MyBase.addOption(oElmt2, "Just remove the Role relationship from the Users", "Remove")
                            '    MyBase.addOption(oElmt2, "Transfer Users to another Role", "Transfer")

                            '    'Lets get all other roles
                            '    oElmt3 = MyBase.addSelect1(oFrmElmt, "Roles", False, "Select Roles", "scroll_10", xForm.ApperanceTypes.Minimal)
                            '    sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d where d.cDirSchema='Role' and d.nDirKey<>" & dirId & " order by cDirName"
                            '    MyBase.addOptionsFromSqlDataReader(oElmt3, moDbHelper.getDataReader(sSql), "name", "value")

                    End Select

                    MyBase.addSubmit(oFrmElmt, "", "Delete " & sType)

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            'remove the relevent answer information
                            Select Case sType
                                Case "User"
                                    'Select Case goRequest("Options")
                                    '    Case "Transfer"
                                    '        sSql = "select nQResultsKey from tblQuestionaireResult where nDirId=" & dirId
                                    '        oDr = oDt.getDataReader(sSql)
                                    '        While oDr.Read
                                    '            sSql = "update tblQuestionaireResult set nDirId = " & goRequest("TransUserId") & " where nDirId=" & dirId
                                    '            oDt.exeProcessSQL(sSql)
                                    '        End While
                                    '        oDr.Close()
                                    '        oDr = Nothing
                                    '        moDbhelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)
                                    '    Case "Delete"
                                    moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)
                                    '    Case Else
                                    '        'do nothing
                                    'End Select

                                Case "Company"
                                    'Delete Departments 
                                    'Delete Departments Directory Relations
                                    'Delete Company Permissions
                                    'Delete Company Users / or move to another Company / or leave oprhaned?
                                    'Move to another department / or leave oprhaned?
                                    Select Case goRequest("Options")
                                        Case "RemoveDept"
                                            sSql = "select r.nRelKey from tblDirectoryRelation r where r.nDirParentId = " & dirId &
                                            " inner join tblDirectory d on r.nDirChildId = d.nDirKey " &
                                            " where d.cDirSchema = 'User' "
                                            oDr = moDbHelper.getDataReader(sSql)
                                            While oDr.Read
                                                moDbHelper.DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr(0))
                                            End While
                                            oDr.Close()
                                            oDr = Nothing

                                            'Delete Company Directory Relations, Company Permissions and Company
                                            moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)

                                        Case "Transfer"

                                            sSql = "update tblDirectoryRelation set nDirParentId = " & goRequest("TransCompanyId") & " where nDirParentId=" & dirId
                                            moDbHelper.ExeProcessSql(sSql)

                                            'Delete Company Directory Relations, Company Permissions and Company
                                            moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)

                                        Case "Delete"
                                            'Delete Company Directory Relations, Company Permissions and Company
                                            moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)

                                    End Select

                                Case "Department"

                                    'Move to another department / or leave oprhaned?
                                    Select Case goRequest("Options")
                                        Case "Transfer"

                                            sSql = "update tblDirectoryRelation set nDirParentId = " & goRequest("TransDeptId") & " where nDirParentId=" & dirId
                                            moDbHelper.ExeProcessSql(sSql)

                                            'Delete Department Directory Relations, Department Permissions and Department
                                            moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)
                                        Case "Remove"
                                            'remove all child relations so child objects don't get deleted
                                            sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " & dirId
                                            oDr = moDbHelper.getDataReader(sSql)
                                            While oDr.Read
                                                moDbHelper.DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr(0))
                                            End While
                                            oDr.Close()
                                            oDr = Nothing

                                            'Delete Department Directory Relations, Department Permissions and Department
                                            moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)
                                        Case Else
                                            'do nothing
                                    End Select


                                Case "Group"
                                    Select Case goRequest("Options")
                                        Case "Transfer"
                                            'remove all child relations so child objects don't get deleted
                                            sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " & dirId
                                            oDr = moDbHelper.getDataReader(sSql)
                                            While oDr.Read
                                                moDbHelper.saveDirectoryRelations(oDr(1), goRequest("Groups"))
                                                moDbHelper.DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr(0))
                                            End While
                                            oDr.Close()
                                            oDr = Nothing

                                            moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)

                                        Case "Remove"
                                            'remove all child relations so child objects don't get deleted
                                            sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " & dirId
                                            oDr = moDbHelper.getDataReader(sSql)
                                            While oDr.Read
                                                moDbHelper.DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr(0))
                                            End While
                                            oDr.Close()
                                            oDr = Nothing

                                            moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)
                                        Case Else
                                            'do nothing
                                    End Select
                                Case "Role"
                                    Select Case goRequest("Options")
                                        Case "Transfer"

                                            'remove all child relations so child objects don't get deleted
                                            sSql = "select nRelKey, nDirChildId from tblDirectoryRelation where nDirParentId = " & dirId
                                            oDr = moDbHelper.getDataReader(sSql)
                                            While oDr.Read
                                                moDbHelper.saveDirectoryRelations(oDr(1), goRequest("Roles"))
                                                moDbHelper.DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr(0))
                                            End While
                                            oDr.Close()
                                            oDr = Nothing

                                            moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)

                                        Case "Remove"
                                            'remove all child relations so child objects don't get deleted
                                            sSql = "select nRelKey from tblDirectoryRelation where nDirParentId = " & dirId
                                            oDr = moDbHelper.getDataReader(sSql)
                                            While oDr.Read
                                                moDbHelper.DeleteObject(dbHelper.objectTypes.DirectoryRelation, oDr(0))
                                            End While
                                            oDr.Close()
                                            oDr = Nothing

                                            moDbHelper.DeleteObject(dbHelper.objectTypes.Directory, dirId)
                                        Case Else
                                            'do nothing
                                    End Select
                            End Select


                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDeleteDeliveryMethod(ByVal id As Long) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oElmt As XmlElement

                Dim oDr As SqlDataReader
                Dim cProcessInfo As String = ""


                Try
                    'load the directory item to be deleted
                    moDbHelper.moPageXml = moPageXML

                    MyBase.NewFrm("EditDeliveryMethod")

                    'Lets get the object
                    oElmt = moPageXML.CreateElement("sType")
                    oElmt.SetAttribute("id", id)
                    If id <> 0 Then
                        oDr = moDbHelper.getDataReader("SELECT tblCartShippingMethods.* FROM tblCartShippingMethods WHERE nShipOptKey = " & id)
                        While oDr.Read
                            oElmt.SetAttribute("name", oDr("cShipOptName"))
                        End While
                        oDr.Close()
                        oDr = Nothing
                    End If

                    MyBase.Instance.AppendChild(oElmt)


                    MyBase.submission("EditInput", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "DeleteDM", "", "Delete Delivery Method")

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to delete this Delivery Method: " & oElmt.GetAttribute("name"))


                    MyBase.addSubmit(oFrmElmt, "", "Delete Delivery Method")

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            moDbHelper.DeleteObject(dbHelper.objectTypes.CartShippingMethod, id)

                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmDeleteDeliveryMethod", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDeleteCarrier(ByVal id As Long) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oElmt As XmlElement

                Dim oDr As SqlDataReader
                Dim cProcessInfo As String = ""


                Try
                    'load the directory item to be deleted
                    moDbHelper.moPageXml = moPageXML

                    MyBase.NewFrm("DeleteCarrier")

                    'Lets get the object
                    oElmt = moPageXML.CreateElement("sType")
                    oElmt.SetAttribute("id", id)
                    If id <> 0 Then
                        oDr = moDbHelper.getDataReader("SELECT tblCartCarrier.* FROM tblCartCarrier WHERE nCarrierKey = " & id)
                        While oDr.Read
                            oElmt.SetAttribute("name", oDr("cCarrierName"))
                        End While
                        oDr.Close()
                        oDr = Nothing
                    End If

                    MyBase.Instance.AppendChild(oElmt)


                    MyBase.submission("EditInput", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "DeleteDM", "", "Delete Delivery Method")

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to delete this Carrier: " & oElmt.GetAttribute("name"))


                    MyBase.addSubmit(oFrmElmt, "", "Delete Carrier")

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            moDbHelper.DeleteObject(dbHelper.objectTypes.CartCarrier, id)

                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmDeleteCarrier", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDeleteShippingLocation(ByVal id As Long) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oElmt As XmlElement

                Dim oDr As SqlDataReader
                Dim cProcessInfo As String = ""


                Try
                    'load the directory item to be deleted
                    moDbHelper.moPageXml = moPageXML

                    MyBase.NewFrm("DeleteShippingLocation")

                    'Lets get the object
                    oElmt = moPageXML.CreateElement("sType")
                    oElmt.SetAttribute("id", id)
                    If id <> 0 Then
                        oDr = moDbHelper.getDataReader("SELECT cLocationNameFull FROM tblCartShippingLocations WHERE nLocationKey = " & id)
                        While oDr.Read
                            oElmt.SetAttribute("name", oDr(0))
                        End While
                        oDr.Close()
                        oDr = Nothing
                    End If

                    MyBase.Instance.AppendChild(oElmt)


                    MyBase.submission("EditInput", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "DeleteDM", "", "Delete Shipping Location")

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to delete this Shipping Location: " & oElmt.GetAttribute("name") & " and all of its children?")

                    MyBase.addSubmit(oFrmElmt, "", "Delete Shipping Location")

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then

                            moDbHelper.DeleteObject(dbHelper.objectTypes.CartShippingLocation, id)

                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmDeleteShippingLocation", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Overridable Function xFrmPagePermissions(ByVal id As Long) As XmlElement

                Dim cDirectorySchemas As String = "Group,Role"
                Dim aDirectorySchemas() As String

                Dim oFrmElmt As XmlElement
                Dim oFrmGrp1 As XmlElement
                Dim oFrmGrp2 As XmlElement
                Dim oFrmGrp3 As XmlElement


                Dim oElmt2 As XmlElement

                Dim oElmt4 As XmlElement

                Dim sSql As String



                Dim cProcessInfo As String = ""

                Try

                    ' Check for schema overloads
                    If Not (String.IsNullOrEmpty("" & goConfig("AdminPagePermissionSchemas"))) Then

                        cDirectorySchemas = goConfig("AdminPagePermissionSchemas")

                    End If

                    ' Split the schema into an array
                    cProcessInfo = "Schemas: " & cDirectorySchemas
                    aDirectorySchemas = cDirectorySchemas.Split(",")

                    'load the xform to be edited
                    MyBase.NewFrm("EditPagePermissions")

                    MyBase.submission("EditInputPagePermissions", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditPermissions", "3col", "Permissions for Page - " & moDbHelper.getNameByKey(dbHelper.objectTypes.ContentStructure, id))

                    oFrmGrp1 = MyBase.addGroup(oFrmElmt, "AllObjects", "", "System User Groups &amp; Roles")

                    'add the buttons so we can test for submission
                    oFrmGrp2 = MyBase.addGroup(oFrmElmt, "EditPermissions", "PermissionButtons", "Set Selected Group Permissions")
                    'MyBase.addSubmit(oFrmGrp2, "AddAll", "Add All >", "", "PermissionButtons")
                    MyBase.addSubmit(oFrmGrp2, "AllowSelected", "Allow Selected", "", "PermissionButton icon-right", "fa-arrow-right")
                    MyBase.addSubmit(oFrmGrp2, "DenySelected", "Deny Selected", "", "PermissionButton icon-right", "fa-arrow-right")
                    MyBase.addSubmit(oFrmGrp2, "RemoveSelected", "Remove Selected", "", "PermissionButton btn-primary", "fa-arrow-left")
                    MyBase.addSubmit(oFrmGrp2, "RemoveAll", "Remove All - Open Access", "", "PermissionButton btn-danger", "fa-times")

                    MyBase.addNote(oFrmGrp2, xForm.noteTypes.Hint, "Allowing one group impicitly denies all others, only use deny permissions to further filter members of allowed groups")

                    ' Process any submissions
                    Select Case MyBase.getSubmitted

                        Case "AddAll"

                        Case "AllowSelected"
                            For Each cSchema As String In aDirectorySchemas
                                moDbHelper.savePermissions(goRequest("pgid"), goRequest(cSchema), dbHelper.PermissionLevel.View)
                            Next


                        Case "DenySelected"
                            For Each cSchema As String In aDirectorySchemas
                                moDbHelper.savePermissions(goRequest("pgid"), goRequest(cSchema), dbHelper.PermissionLevel.Denied)
                            Next

                        Case "RemoveSelected"
                            moDbHelper.savePermissions(goRequest("pgid"), goRequest("Items"), dbHelper.PermissionLevel.Open)

                        Case "RemoveAll"
                            moDbHelper.savePermissions(goRequest("pgid"), "", dbHelper.PermissionLevel.Open)

                    End Select


                    ' Populate the left hand selects, grouped by Directory schema.
                    For Each cSchema As String In aDirectorySchemas
                        oElmt2 = MyBase.addSelect(oFrmGrp1, cSchema, False, cSchema, IIf(cSchema = "User", "scroll_30", "scroll_10"), xForm.ApperanceTypes.Minimal)
                        sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d " &
                        "left outer join tblDirectoryPermission p on p.nDirId = d.nDirKey and p.nStructId = " & id & " " &
                        "where d.cDirSchema='" & SqlFmt(cSchema) & "' and p.nPermKey is null order by d.cDirName"
                        MyBase.addOptionsFromSqlDataReader(oElmt2, moDbHelper.getDataReader(sSql), "name", "value")
                    Next


                    MyBase.addNote(oFrmGrp1, xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names")


                    ' Populate the allow and denied boxes.
                    oFrmGrp3 = MyBase.addGroup(oFrmElmt, "PermittedObjects", "", "Assigned Permissions")

                    oElmt4 = MyBase.addSelect(oFrmGrp3, "Items", False, "Allowed", "scroll_10", xForm.ApperanceTypes.Minimal)
                    sSql = "SELECT p.nDirId as value, '['+ d.cDirSchema + '] ' + d.cDirName as name from tblDirectoryPermission p " &
                    "inner join tblDirectory d on d.nDirKey = p.nDirId " &
                    "where p.nStructId=" & id & " and p.nAccessLevel = 2" &
                    " order by d.cDirSchema"

                    MyBase.addOptionsFromSqlDataReader(oElmt4, moDbHelper.getDataReader(sSql), "name", "value")

                    oElmt4 = MyBase.addSelect(oFrmGrp3, "Items", False, "Denied", "scroll_10", xForm.ApperanceTypes.Minimal)
                    sSql = "SELECT p.nDirId as value, '['+ d.cDirSchema + '] ' + d.cDirName as name from tblDirectoryPermission p " &
                    "inner join tblDirectory d on d.nDirKey = p.nDirId " &
                    "where p.nStructId=" & id & " and p.nAccessLevel = 0" &
                    " order by d.cDirSchema"

                    MyBase.addNote(oFrmGrp3, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")

                    MyBase.addOptionsFromSqlDataReader(oElmt4, moDbHelper.getDataReader(sSql), "name", "value")

                    MyBase.Instance.InnerXml = "<permissions/>"


                    ' Rights Alert - to give a user an idea that Rights exists on this page, we'll highlight
                    ' this on the Rights page in an alert
                    If moDbHelper.GetDataValue("SELECT COUNT(*) As pCount FROM tblDirectoryPermission WHERE nAccessLevel > 2 AND nStructId=" & id, , , 0) > 0 Then

                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Note: there are also Rights being applied to this page.  You can view these by clicking the Rights button above.")

                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmPageRights(ByVal id As Long) As XmlElement

                Dim cDirectorySchemas As String = "Group,Role,User"
                Dim aDirectorySchemas() As String


                Dim oFrmElmt As XmlElement
                Dim oFrmGrp1 As XmlElement
                Dim oFrmGrp2 As XmlElement
                Dim oFrmGrp3 As XmlElement

                Dim oElmt As XmlElement
                Dim oElmt2 As XmlElement
                Dim oElmt4 As XmlElement
                Dim sSql As String
                Dim bRightsByUser As Boolean = True

                Dim cProcessInfo As String = ""

                Try

                    ' Check for schema overloads
                    If Not (String.IsNullOrEmpty("" & goConfig("AdminPageRightSchemas"))) Then
                        cDirectorySchemas = goConfig("AdminPageRightSchemas")
                    End If

                    ' Split the schema into an array
                    cProcessInfo = "Schemas: " & cDirectorySchemas
                    aDirectorySchemas = cDirectorySchemas.Split(",")


                    ' Load the xform to be edited
                    MyBase.NewFrm("EditPagePermissions")
                    MyBase.submission("EditInputPageRights", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditPermissions", "3col", "Rights for Page - " & moDbHelper.getNameByKey(dbHelper.objectTypes.ContentStructure, id))
                    oFrmGrp1 = MyBase.addGroup(oFrmElmt, "AllObjects", "", "Select the items you want to have access to this page")
                    MyBase.addNote(oFrmGrp1, xForm.noteTypes.Hint, "You can select multiple items by holding down CTRL while clicking the names")


                    ' Add the buttons and radios
                    oFrmGrp2 = MyBase.addGroup(oFrmElmt, "EditPermissions", "PermissionButtons", "Buttons")

                    oElmt2 = MyBase.addSelect1(oFrmGrp2, "Level", False, "Access Level", "multiline", xForm.ApperanceTypes.Full)
                    MyBase.addOption(oElmt2, "View", "2")
                    MyBase.addOption(oElmt2, "Add", "3")
                    MyBase.addOption(oElmt2, "Add and Update Own", "4")
                    MyBase.addOption(oElmt2, "Update All", "5")
                    MyBase.addOption(oElmt2, "Approve All", "6")
                    MyBase.addOption(oElmt2, "Add, Update and Publish Own ", "7")
                    MyBase.addOption(oElmt2, "Publish All", "8")
                    MyBase.addOption(oElmt2, "Full", "9")

                    ' Add All does nothing, probably is not good for this screen either.
                    'MyBase.addSubmit(oFrmGrp2, "AddAll", "Add All >", "", "PermissionButtons")
                    MyBase.addSubmit(oFrmGrp2, "AddSelected", "Add Selected >", "", "PermissionButtons")
                    MyBase.addSubmit(oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons")
                    MyBase.addSubmit(oFrmGrp2, "RemoveAll", "< Remove All", "", "PermissionButtons")


                    ' Save the permissions on submission
                    Select Case MyBase.getSubmitted

                        'Case "AddAll"

                        Case "AddSelected"
                            For Each cSchema As String In aDirectorySchemas
                                If (cSchema <> "User" Or bRightsByUser) And Not (String.IsNullOrEmpty("" & goRequest(cSchema))) Then moDbHelper.savePermissions(goRequest("pgid"), goRequest(cSchema), CInt(goRequest("Level")))
                            Next

                        Case "RemoveSelected"
                            For Each cSchema As String In aDirectorySchemas
                                If (cSchema <> "User" Or bRightsByUser) And Not (String.IsNullOrEmpty("" & goRequest("Items" & cSchema))) Then moDbHelper.savePermissions(goRequest("pgid"), goRequest("Items" & cSchema), dbHelper.PermissionLevel.Open)
                            Next

                        Case "RemoveAll"
                            moDbHelper.savePermissions(goRequest("pgid"), "", dbHelper.PermissionLevel.Open)

                    End Select


                    ' Add the left hand selection boxes.
                    For Each cSchema As String In aDirectorySchemas
                        ' Run this for all schemas except users, unless bRightsByUser is True
                        If cSchema <> "User" Or bRightsByUser Then
                            oElmt = MyBase.addSelect(oFrmGrp1, cSchema, False, cSchema, IIf(cSchema = "User", "scroll_30", "scroll_10"), xForm.ApperanceTypes.Minimal)
                            sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d " &
                            "left outer join tblDirectoryPermission p on p.nDirId = d.nDirKey and p.nStructId = " & id & " " &
                            "where d.cDirSchema='" & SqlFmt(cSchema) & "' and p.nPermKey is null order by d.cDirName"
                            MyBase.addOptionsFromSqlDataReader(oElmt, moDbHelper.getDataReader(sSql), "name", "value")
                        End If
                    Next


                    ' Add the right hand selected lists.
                    oFrmGrp3 = MyBase.addGroup(oFrmElmt, "PermittedObjects", "", "All items with permissions to access page")
                    MyBase.addNote(oFrmGrp3, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")


                    For Each cSchema As String In aDirectorySchemas
                        ' Run this for all schemas except users, unless bRightsByUser is True
                        If cSchema <> "User" Or bRightsByUser Then

                            oElmt4 = MyBase.addSelect(oFrmGrp3, "Items" & cSchema, False, "Allowed " & cSchema, IIf(cSchema = "User", "scroll_30", "scroll_10"), xForm.ApperanceTypes.Minimal)
                            sSql = "SELECT p.nDirId as value, '['+ str(p.nAccessLevel) + '] ' + d.cDirName as name from tblDirectoryPermission p " &
                            "inner join tblDirectory d on d.nDirKey = p.nDirId " &
                            "where p.nStructId=" & id & " and d.cDirSchema = '" & SqlFmt(cSchema) & "' " &
                            "order by d.cDirSchema"
                            MyBase.addOptionsFromSqlDataReader(oElmt4, moDbHelper.getDataReader(sSql), "name", "value")

                        End If
                    Next

                    MyBase.Instance.InnerXml = "<permissions/>"

                    ' Permissions Alert - to give a user an idea that permissions exists on this page, we'll highlight
                    ' this on the rights page in an alert
                    If moDbHelper.GetDataValue("SELECT COUNT(*) As pCount FROM tblDirectoryPermission WHERE nAccessLevel IN (0,2) AND nStructId=" & id, , , 0) > 0 Then

                        MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Note: there are also Permissions being applied to this page.  You can view these by clicking the Permissions button above.")

                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmPageRights", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmUserMemberships(ByVal UserId As Long) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oFrmGrp1 As XmlElement
                Dim oFrmGrp2 As XmlElement
                Dim oFrmGrp3 As XmlElement

                Dim oElmt1 As XmlElement
                Dim oElmt2 As XmlElement

                Dim oElmt4 As XmlElement
                Dim sSql As String

                Dim cProcessInfo As String = ""

                Try
                    'load the xform to be edited

                    MyBase.NewFrm("EditUserMemberships")

                    MyBase.submission("EditUserMemberships", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditUserMemberships", "3col", "Memberships for User - ")

                    oFrmGrp1 = MyBase.addGroup(oFrmElmt, "AllObjects", "", "Select the groups you want this user to belong too")
                    MyBase.addNote(oFrmGrp1, xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names")

                    'add the buttons so we can test for submission
                    oFrmGrp2 = MyBase.addGroup(oFrmElmt, "EditPermissions", "PermissionButtons", "Buttons")

                    MyBase.addSubmit(oFrmGrp2, "AddSelected", "Add Selected >", "", "PermissionButtons")
                    MyBase.addSubmit(oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons")
                    MyBase.addSubmit(oFrmGrp2, "Finish", "Finish Editing", "", "PermissionButtons")

                    Select Case MyBase.getSubmitted

                        Case "AddSelected"
                            moDbHelper.saveDirectoryRelations(goRequest("id"), goRequest("Departments"))
                            moDbHelper.saveDirectoryRelations(goRequest("id"), goRequest("Groups"))

                        Case "RemoveSelected"
                            moDbHelper.saveDirectoryRelations(goRequest("id"), goRequest("Items"), True)
                        Case "Finish"
                            MyBase.valid = True

                    End Select

                    oElmt1 = MyBase.addSelect(oFrmGrp1, "Departments", False, "Departments", "scroll_10", xForm.ApperanceTypes.Minimal)

                    '     sSql = "SELECT d.nDirKey as value, d.cDirName as name from tblDirectory d " & _
                    '"left outer join tblDirectoryRelation dr on dr.nDirParentId = d.nDirKey and dr.nDirChildId = " & UserId & " " & _
                    '"where d.cDirSchema='Department' and dr.nRelKey is null order by d.cDirName"

                    sSql = "execute getUsersCompanyDepartments @userId=" & UserId & ", @adminUserId=" & myWeb.mnUserId


                    MyBase.addOptionsFromSqlDataReader(oElmt1, moDbHelper.getDataReader(sSql), "name", "value")

                    oElmt2 = MyBase.addSelect(oFrmGrp1, "Groups", False, "Groups", "scroll_10", xForm.ApperanceTypes.Minimal)

                    sSql = "execute getUsersCompanyGroups @userId=" & UserId & ", @adminUserId=" & myWeb.mnUserId


                    MyBase.addOptionsFromSqlDataReader(oElmt2, moDbHelper.getDataReader(sSql), "name", "value")

                    oFrmGrp3 = MyBase.addGroup(oFrmElmt, "PermittedObjects", "", "User is Member of...")
                    MyBase.addNote(oFrmGrp3, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")

                    oElmt4 = MyBase.addSelect(oFrmGrp3, "Items", False, "Allowed", "scroll_30", xForm.ApperanceTypes.Minimal)

                    sSql = "SELECT d.nDirKey as value, '['+ d.cDirSchema + '] ' + d.cDirName as name from tblDirectory d " &
                     "inner join tblDirectoryRelation dr on dr.nDirParentId = d.nDirKey and dr.nDirChildId = " & UserId & " " &
                     "where d.cDirSchema='Group' or d.cDirSchema='Department'  order by d.cDirName"

                    MyBase.addOptionsFromSqlDataReader(oElmt4, moDbHelper.getDataReader(sSql), "name", "value")

                    MyBase.Instance.InnerXml = "<memberships/>"

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmUserMemberships", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Overridable Function xFrmDirMemberships(ByVal rootType As String, ByVal DirId As Long, ByVal DirParId As Long, ByVal ChildTypes As String) As XmlElement
                Dim oFrmElmt As XmlElement

                Dim oFrmAll As XmlElement
                Dim oFrmChosen As XmlElement

                Dim oFrmButtons As XmlElement



                Dim oElmt1 As XmlElement

                Dim oElmt4 As XmlElement

                Dim sSql As String
                Dim aChildTypes() As String = Split(ChildTypes, ",")
                Dim aChildDesc As String = ""
                Dim i As Integer

                Dim cProcessInfo As String = ""

                Try
                    ' Enumerate the childtypes
                    For i = 0 To UBound(aChildTypes)
                        If i = UBound(aChildTypes) Then
                            aChildDesc = aChildDesc & " and " & aChildTypes(i) & "s"
                        Else
                            aChildDesc = aChildDesc & ", " & aChildTypes(i) & "s"
                        End If
                    Next

                    ' Get the description
                    If UBound(aChildTypes) = 0 Then
                        aChildDesc = LCase(Right(aChildDesc, Len(aChildDesc) - 5))
                    Else
                        aChildDesc = LCase(Right(aChildDesc, Len(aChildDesc) - 2))
                    End If

                    ' Create the form
                    MyBase.NewFrm("EditMemberships")
                    MyBase.submission("EditMemberships", "", "post")


                    ' Create the groups
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditUserMemberships", "3col", "Memberships for " & rootType & " - " & moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, DirId))
                    MyBase.addNote(oFrmElmt, noteTypes.Hint, "You can select multiple items by holding down CTRL whilst clicking the names")

                    ' Create a group for all directory items
                    oFrmAll = MyBase.addGroup(oFrmElmt, "AllObjects", "", "Add " & aChildDesc & " to " & rootType)
                    MyBase.addNote(oFrmAll, noteTypes.Hint, "Select the " & aChildDesc & " you would like to belong to this " & LCase(rootType))

                    ' Create a middle column
                    oFrmButtons = MyBase.addGroup(oFrmElmt, "SubmissionButtons", "PermissionButtons", "Actions")

                    ' Create a group for chosen directory items
                    oFrmChosen = MyBase.addGroup(oFrmElmt, "PermittedObjects", "", rootType & " Contains")
                    ' MyBase.addNote(oFrmGrp2, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")


                    ' Add submit buttons (group specified by issue 1362)
                    MyBase.addSubmit(oFrmButtons, "AddSelected", "Add Selected >", "", "PermissionButtons")
                    MyBase.addSubmit(oFrmButtons, "RemoveSelected", "< Remove Selected", "", "PermissionButtons")
                    MyBase.addSubmit(oFrmButtons, "Finish", "Finish Editing", "", "principle PermissionButtons")

                    'lets add / remove before we populate
                    Select Case MyBase.getSubmitted
                        Case "AddSelected"
                            For i = 0 To UBound(aChildTypes)
                                moDbHelper.saveDirectoryRelations(goRequest("id"), goRequest(aChildTypes(i) & "s"), False, dbHelper.RelationType.Child)
                            Next
                        Case "RemoveSelected"
                            moDbHelper.saveDirectoryRelations(goRequest("id"), goRequest("Items"), True, dbHelper.RelationType.Child)
                    End Select

                    'populate the add boxes

                    For i = 0 To UBound(aChildTypes)
                        Select Case aChildTypes(i)
                            Case "User"
                                oElmt1 = MyBase.addSelect(oFrmAll, aChildTypes(i) & "s", False, aChildTypes(i) & "s", "scroll_15 alphasort", ApperanceTypes.Minimal)
                            Case Else
                                oElmt1 = MyBase.addSelect(oFrmAll, aChildTypes(i) & "s", False, aChildTypes(i) & "s", "scroll_10 alphasort", ApperanceTypes.Minimal)
                        End Select

                        sSql = ""

                        If DirParId <> 0 Then
                            If IsNumeric(DirParId) AndAlso IsNumeric(DirId) Then
                                sSql = "SELECT d.nDirKey as value, d.cDirName as name, d.cDirXml as detail " _
                                        & "FROM tblDirectory d " _
                                        & "     INNER JOIN tblAudit a on nAuditId = a.nAuditKey " _
                                        & "     INNER JOIN tblDirectoryRelation dr on d.nDirKey = dr.nDirChildId " _
                                        & "     LEFT JOIN tblDirectoryRelation dr2 on dr2.nDirChildId = d.nDirKey and dr2.nDirParentId =  " & SqlFmt(DirId.ToString) & " " _
                                        & "WHERE d.cDirSchema = " & Tools.Database.SqlString(Trim(aChildTypes(i))) & "  " _
                                        & "     AND dr.nDirParentId =  " & SqlFmt(DirParId.ToString) & " " _
                                        & "     AND dr2.nRelKey is null " _
                                        & "     AND (a.nStatus =1 or a.nStatus = -1) " _
                                        & "ORDER BY d.cDirName "
                            End If
                        Else
                            If IsNumeric(DirId) Then
                                sSql = "SELECT d.nDirKey as value, d.cDirName as name, d.cDirXml as detail " _
                                        & "FROM tblDirectory d " _
                                        & "     INNER JOIN tblAudit a on nAuditId = a.nAuditKey " _
                                        & "     LEFT JOIN tblDirectoryRelation dr2 on dr2.nDirChildId = d.nDirKey and dr2.nDirParentId =  " & SqlFmt(DirId.ToString) & " " _
                                        & "WHERE d.cDirSchema = " & Tools.Database.SqlString(Trim(aChildTypes(i))) & "  " _
                                        & "     AND dr2.nRelKey is null " _
                                        & "     AND (a.nStatus =1 or a.nStatus = -1) " _
                                        & "ORDER BY d.cDirName "
                            End If
                        End If
                        If Not String.IsNullOrEmpty(sSql) Then
                            If aChildTypes(i) = "User" Then
                                addUserOptionsFromSqlDataReader(oElmt1, moDbHelper.getDataReader(sSql), "name", "value")
                            Else
                                addOptionsFromSqlDataReader(oElmt1, moDbHelper.getDataReader(sSql), "name", "value")
                            End If
                        End If

                    Next

                    'populate the allowed boxes
                    oElmt4 = MyBase.addSelect(oFrmChosen, "Items", False, "Allowed", "scroll_30 alphasort", ApperanceTypes.Minimal)

                    sSql = "SELECT d.nDirKey as value, '['+ d.cDirSchema + '] ' + d.cDirName as name, d.cDirXml as detail from tblDirectory d " &
                     "inner join tblAudit a on nAuditId = a.nAuditKey inner join tblDirectoryRelation dr on dr.nDirChildId = d.nDirKey and dr.nDirParentId = " & DirId & " " &
                     "where "
                    For i = 0 To UBound(aChildTypes)
                        If i = 0 Then
                            sSql = sSql & "d.cDirSchema='" & Trim(aChildTypes(i)) & "'"
                        Else
                            sSql = sSql & " or d.cDirSchema='" & Trim(aChildTypes(i)) & "'"
                        End If
                    Next
                    sSql = sSql & "and (a.nStatus =1 or a.nStatus = -1) order by d.cDirName"

                    addUserOptionsFromSqlDataReader(oElmt4, moDbHelper.getDataReader(sSql), "name", "value")

                    If MyBase.getSubmitted = "Finish" Then MyBase.valid = True

                    MyBase.Instance.InnerXml = "<memberships/>"

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    Protean.returnException(myWeb.msException, mcModuleName, "xFrmDirMemberships", ex, "", cProcessInfo, Protean.gbDebug)
                    Return Nothing
                End Try
            End Function

            '+++++++++++++++++++++++++++ Ecommerce Forms ++++++++++++++++++++++++++++++++'

            Public Function xFrmEditShippingLocation(Optional ByVal id As Long = -1, Optional ByVal parId As Long = -1) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim cProcessInfo As String = ""
                Try
                    If id = 0 Then id = -1

                    MyBase.NewFrm("EditShippingLocation")

                    MyBase.submission("EditShippingLocation", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditShippingLocation", "", "Edit Shipping Location")
                    MyBase.addInput(oFrmElmt, "nStructParId", True, "ParId", "hidden")
                    MyBase.addBind("nStructParId", "tblCartShippingLocations/nLocationParId", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "nLocationType", True, "Type", "required", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Global", 0)
                    MyBase.addOption(oSelElmt, "Continental", 1)
                    MyBase.addOption(oSelElmt, "Country", 2)
                    MyBase.addOption(oSelElmt, "Region", 3)
                    MyBase.addOption(oSelElmt, "County", 4)
                    MyBase.addOption(oSelElmt, "Post Town", 5)
                    MyBase.addOption(oSelElmt, "Postal Code", 5)
                    MyBase.addBind("nLocationType", "tblCartShippingLocations/nLocationType", "true()")

                    MyBase.addInput(oFrmElmt, "cNameFull", True, "Full Name", "required")
                    MyBase.addBind("cNameFull", "tblCartShippingLocations/cLocationNameFull", "true()")

                    MyBase.addInput(oFrmElmt, "cNameShort", True, "Short Name", "required")
                    MyBase.addBind("cNameShort", "tblCartShippingLocations/cLocationNameShort", "true()")

                    MyBase.addInput(oFrmElmt, "cISOnum", True, "ISO Num")
                    MyBase.addBind("cISOnum", "tblCartShippingLocations/cLocationISOnum", "false()")

                    MyBase.addInput(oFrmElmt, "cISOa2", True, "ISOa2")
                    MyBase.addBind("cISOa2", "tblCartShippingLocations/cLocationISOa2", "false()")

                    MyBase.addInput(oFrmElmt, "cISOa3", True, "ISOa3")
                    MyBase.addBind("cISOa3", "tblCartShippingLocations/cLocationISOa3", "false()")

                    MyBase.addInput(oFrmElmt, "cCode", True, "Code")
                    MyBase.addBind("cCode", "tblCartShippingLocations/cLocationCode", "false()")

                    MyBase.addInput(oFrmElmt, "cTaxRate", True, "TaxRate")
                    MyBase.addBind("cTaxRate", "tblCartShippingLocations/nLocationTaxRate", "false()")

                    MyBase.addSubmit(oFrmElmt, "ewSubmit", "Save Page")

                    MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.CartShippingLocation, id)
                    'set the parId for a new record
                    If id < 0 Then
                        Instance.SelectSingleNode("tblCartShippingLocations/nLocationParId").InnerText = parId
                    End If
                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartShippingLocation, MyBase.Instance)
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditShippingLocation", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmEditDeliveryMethod(Optional ByVal id As Long = -1, Optional ByVal parId As Long = -1) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim oGrp1Elmt As XmlElement
                Dim oGrp2Elmt As XmlElement
                Dim cProcessInfo As String = ""
                Try
                    If id = 0 Then id = -1

                    MyBase.NewFrm("EditShippingMethod")

                    MyBase.submission("EditShippingMethod", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditDeliveryMethod", "2Col", "Edit Delivery Method")

                    oGrp1Elmt = MyBase.addGroup(oFrmElmt, "Settings", "", "Content Settings")
                    oGrp2Elmt = MyBase.addGroup(oFrmElmt, "Content", "", "Terms and Conditions")

                    Dim moPaymentCfg As XmlNode
                    moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                    'check we have differenct currencies
                    If Not moPaymentCfg Is Nothing Then
                        If Not moPaymentCfg.SelectSingleNode("currencies/Currency") Is Nothing Then
                            Dim oCurElmt As XmlElement
                            oCurElmt = MyBase.addSelect1(oGrp1Elmt, "cCurrency", True, "Currency Code")
                            MyBase.addBind("cCurrency", "tblCartShippingMethods/cCurrency")
                            MyBase.addOption(oCurElmt, "All", "")
                            Dim ocSetElmt As XmlElement
                            For Each ocSetElmt In moPaymentCfg.SelectNodes("currencies/Currency")
                                MyBase.addOption(oCurElmt, ocSetElmt.SelectSingleNode("name").InnerText, ocSetElmt.GetAttribute("ref"))
                            Next
                        End If
                    End If



                    MyBase.addInput(oGrp1Elmt, "cShipOptName", True, "Service Name")
                    MyBase.addBind("cShipOptName", "tblCartShippingMethods/cShipOptName", "true()")

                    MyBase.addInput(oGrp1Elmt, "cShipOptCarrier", True, "Carrier")
                    MyBase.addBind("cShipOptCarrier", "tblCartShippingMethods/cShipOptCarrier", "false()")

                    MyBase.addInput(oGrp1Elmt, "cShipOptTime", True, "Delivery Period")
                    MyBase.addBind("cShipOptTime", "tblCartShippingMethods/cShipOptTime", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptCost", True, "Cost", "short")
                    MyBase.addBind("nShipOptCost", "tblCartShippingMethods/nShipOptCost", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptPercentage", True, "Percentage", "short")
                    MyBase.addBind("nShipOptPercentage", "tblCartShippingMethods/nShipOptPercentage", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptQuantMin", True, "Minimum Quantity", "short")
                    MyBase.addBind("nShipOptQuantMin", "tblCartShippingMethods/nShipOptQuantMin", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptQuantMax", True, "Maximum Quantity", "short")
                    MyBase.addBind("nShipOptQuantMax", "tblCartShippingMethods/nShipOptQuantMax", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptWeightMin", True, "Minimum Weight", "short")
                    MyBase.addBind("nShipOptWeightMin", "tblCartShippingMethods/nShipOptWeightMin", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptWeightMax", True, "Maximum Weight", "short")
                    MyBase.addBind("nShipOptWeightMax", "tblCartShippingMethods/nShipOptWeightMax", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptPriceMin", True, "Minimum Price", "short")
                    MyBase.addBind("nShipOptPriceMin", "tblCartShippingMethods/nShipOptPriceMin", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptPriceMax", True, "Maximum Price", "short")
                    MyBase.addBind("nShipOptPriceMax", "tblCartShippingMethods/nShipOptPriceMax", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptHandlingPercentage", True, "Handling Percent", "short")
                    MyBase.addBind("nShipOptHandlingPercentage", "tblCartShippingMethods/nShipOptHandlingPercentage", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptHandlingFixedCost", True, "Handling Fixed Cost", "short")
                    MyBase.addBind("nShipOptHandlingFixedCost", "tblCartShippingMethods/nShipOptHandlingFixedCost", "false()")

                    MyBase.addInput(oGrp1Elmt, "nShipOptTaxRate", True, "Tax Rate", "short")
                    MyBase.addBind("nShipOptTaxRate", "tblCartShippingMethods/nShipOptTaxRate", "false()")

                    MyBase.addInput(oGrp1Elmt, "nDisplayPriority", True, "Display Priority", "short")
                    MyBase.addBind("nDisplayPriority", "tblCartShippingMethods/nDisplayPriority", "false()")

                    MyBase.addTextArea(oGrp2Elmt, "cTerms", True, "Special Terms", "xhtml")
                    MyBase.addBind("cTerms", "tblCartShippingMethods/cShipOptTandC", "false()")

                    MyBase.addInput(oGrp2Elmt, "dPublishDate", True, "Start Date", "calendar short")
                    MyBase.addBind("dPublishDate", "tblCartShippingMethods/dPublishDate", "false()")

                    MyBase.addInput(oGrp2Elmt, "dExpireDate", True, "Expire Date", "calendar short")
                    MyBase.addBind("dExpireDate", "tblCartShippingMethods/dExpireDate", "false()")

                    If moDbHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection") Then
                        oSelElmt = MyBase.addSelect(oGrp2Elmt, "bCollection", True, "Collection Option", "multiline", ApperanceTypes.Full)
                        MyBase.addOption(oSelElmt, "Collection", "True")
                        MyBase.addBind("bCollection", "tblCartShippingMethods/bCollection", "false()")
                    End If

                    oSelElmt = MyBase.addSelect1(oGrp2Elmt, "nStatus", True, "Status", "", ApperanceTypes.Minimal)
                    MyBase.addOption(oSelElmt, "Active", 1)
                    MyBase.addOption(oSelElmt, "In-Active", 0)
                    MyBase.addBind("nStatus", "tblCartShippingMethods/nStatus", "true()")

                    MyBase.addSubmit(oGrp2Elmt, "ewSubmit", "Save Method")

                    MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.CartShippingMethod, id)

                    If id = -1 Then
                        'set some default values in the instance.
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptPercentage").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptQuantMin").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptQuantMax").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptWeightMin").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptWeightMax").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptPriceMin").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptPriceMax").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptHandlingPercentage").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptHandlingFixedCost").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nShipOptTaxRate").InnerText = "0"
                        Instance.SelectSingleNode("tblCartShippingMethods/nDisplayPriority").InnerText = "0"
                    End If

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartShippingMethod, MyBase.Instance)
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditDeliveryMethod", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmEditCarrier(Optional ByVal id As Long = -1, Optional ByVal parId As Long = -1) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim oGrp1Elmt As XmlElement
                Dim oGrp2Elmt As XmlElement
                Dim cProcessInfo As String = ""
                Try
                    If id = 0 Then id = -1

                    MyBase.NewFrm("EditCarrier")

                    MyBase.submission("EditCarrier", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditCarrier", "2Col", "Edit Carrier")

                    oGrp1Elmt = MyBase.addGroup(oFrmElmt, "Settings", "", "Settings")
                    oGrp2Elmt = MyBase.addGroup(oFrmElmt, "Content", "", "Carrier")


                    MyBase.addInput(oGrp1Elmt, "dPublishDate", True, "Start Date", "calendar short")
                    MyBase.addBind("dPublishDate", "tblCartCarrier/dPublishDate", "false()")

                    MyBase.addInput(oGrp1Elmt, "dExpireDate", True, "Expire Date", "calendar short")
                    MyBase.addBind("dExpireDate", "tblCartCarrier/dExpireDate", "false()")


                    oSelElmt = MyBase.addSelect1(oGrp1Elmt, "nStatus", True, "Status", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Active", 1)
                    MyBase.addOption(oSelElmt, "In-Active", 0)
                    MyBase.addBind("nStatus", "tblCartCarrier/nStatus", "true()")


                    MyBase.addInput(oGrp2Elmt, "cCarrierName", True, "Name")
                    MyBase.addBind("cCarrierName", "tblCartCarrier/cCarrierName", "true()")

                    MyBase.addTextArea(oGrp2Elmt, "cCarrierTrackingInstructions", True, "Tracking Instructions", "xhtml")
                    MyBase.addBind("cCarrierTrackingInstructions", "tblCartCarrier/cCarrierTrackingInstructions", "true()")
                    MyBase.addNote(oGrp2Elmt, noteTypes.Help, "{@code} will be replaced with the code entered at the time of sending")

                    MyBase.addSubmit(oGrp2Elmt, "ewSubmit", "Save Method")

                    MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.CartCarrier, id)

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartCarrier, MyBase.Instance)
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditCarrier", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmPaymentProvider(ByVal cProviderType As String) As XmlElement

                Dim cProcessInfo As String = ""
                Dim oPaymentCfg As XmlElement

                Try

                    Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")
                    Dim oCfgSect As System.Configuration.DefaultSection = oCfg.GetSection("protean/payment")
                    oPaymentCfg = moPageXML.CreateElement("Config")
                    oPaymentCfg.InnerXml = oCfgSect.SectionInformation.GetRawXml

                    'Replace Spaces with hypens
                    cProviderType = Replace(cProviderType, " ", "-")

                    If Not MyBase.load("/xforms/PaymentProvider/" & cProviderType & ".xml", myWeb.maCommonFolders) Then
                        'show xform load error message

                    Else
                        'remove hyphens
                        cProviderType = Replace(cProviderType, "-", "")
                        Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                        If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then

                            'replace the instance if it exists in the web.config
                            If Not oPaymentCfg.SelectSingleNode("payment/provider[@name='" & cProviderType & "']") Is Nothing Then
                                MyBase.Instance.InnerXml = oPaymentCfg.SelectSingleNode("payment/provider[@name='" & cProviderType & "']").OuterXml
                            End If

                            If MyBase.isSubmitted Then
                                MyBase.updateInstanceFromRequest()
                                MyBase.validate()
                                If MyBase.valid Then
                                    'here we update the web.config
                                    If oPaymentCfg.SelectSingleNode("payment/provider[@name='" & cProviderType & "']") Is Nothing Then
                                        oPaymentCfg.SelectSingleNode("payment").AppendChild(MyBase.Instance.FirstChild)
                                    Else
                                        oPaymentCfg.SelectSingleNode("payment/provider[@name='" & cProviderType & "']").InnerXml = Instance.FirstChild.InnerXml
                                    End If

                                    oCfgSect.SectionInformation.RestartOnExternalChanges = False
                                    oCfgSect.SectionInformation.SetRawXml(oPaymentCfg.InnerXml)
                                    oCfg.Save()

                                    'Copy file to secure if secure directory exists
                                    If IO.File.Exists(goServer.MapPath("protean.payment.config")) Then
                                        Dim fsHelper As New Protean.fsHelper
                                        fsHelper.CopyFile("protean.payment.config", "", "\..\secure", True)
                                        fsHelper = Nothing
                                    End If
                                    'Copy file to secure if secure directory exists
                                    If IO.File.Exists(goServer.MapPath("Protean.Config")) Then
                                        Dim fsHelper As New Protean.fsHelper
                                        fsHelper.CopyFile("Protean.Config", "", "\..\secure", True)
                                        fsHelper = Nothing
                                    End If
                                End If
                            End If
                            oImp.UndoImpersonation()
                        End If
                    End If
                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditPaymentProvider", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDeletePaymentProvider(ByVal cProviderType As String) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim oPaymentCfg As XmlElement

                Try

                    Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")
                    Dim oCfgSect As System.Configuration.DefaultSection = oCfg.GetSection("protean/payment")
                    oPaymentCfg = moPageXML.CreateElement("Config")
                    oPaymentCfg.InnerXml = oCfgSect.SectionInformation.GetRawXml

                    MyBase.NewFrm("DeleteProvider")

                    MyBase.submission("DeleteProvider", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "folderItem", "", "Delete Payment Provider")

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to delete this payment provider? - """ & Tools.Xml.encodeAllHTML(cProviderType) & """")

                    MyBase.addSubmit(oFrmElmt, "", "Delete Provider")

                    MyBase.Instance.InnerXml = "<delete/>"

                    'remove hyphens
                    cProviderType = Replace(cProviderType, "-", "")
                    Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                    If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            If MyBase.valid Then
                                cProviderType = Replace(cProviderType, " ", "")
                                'here we update the web.config
                                If oPaymentCfg.SelectSingleNode("payment/provider[@name='" & cProviderType & "']") Is Nothing Then
                                    'can find do nothing
                                Else
                                    oPaymentCfg.SelectSingleNode("payment").RemoveChild(oPaymentCfg.SelectSingleNode("payment/provider[@name='" & cProviderType & "']"))
                                End If

                                oCfgSect.SectionInformation.RestartOnExternalChanges = False
                                oCfgSect.SectionInformation.SetRawXml(oPaymentCfg.InnerXml)
                                oCfg.Save()

                                'Copy file to secure if secure directory exists
                                Dim fsHelper As New Protean.fsHelper
                                fsHelper.CopyFile("Protean.Config", "", "\..\secure", True)
                                fsHelper = Nothing

                            End If
                        End If
                        oImp.UndoImpersonation()

                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditPaymentProvider", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmUpdateOrder(ByVal nOrderId As Long, ByVal cSchemaName As String) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim oGrp1Elmt As XmlElement
                Dim oGrp2Elmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim nStatus As Long

                Dim tempElement As XmlElement

                Dim moCartConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/cart")


                Try

                    MyBase.NewFrm("Update" & cSchemaName)

                    MyBase.submission("Update" & cSchemaName, "", "post", "form_check(this)")

                    MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.CartOrder, nOrderId)
                    nStatus = MyBase.Instance.SelectSingleNode("tblCartOrder/nCartStatus").InnerText

                    ' Add a note for shipped status goRequest("nStatus")
                    Dim shippedStatus As String = "Shipped"
                    Dim customerShippedTemplate As String = ""
                    Dim sendEmailOnShipped As Boolean = False
                    If moCartConfig IsNot Nothing Then customerShippedTemplate = moCartConfig("CustomerEmailShippedTemplatePath")
                    If Not String.IsNullOrEmpty(customerShippedTemplate) _
                        AndAlso nStatus <> 9 _
                        AndAlso File.Exists(goServer.MapPath(customerShippedTemplate)) Then
                        sendEmailOnShipped = True
                        If goRequest("nStatus") <> "9" Then shippedStatus &= " (Confirmation e-mail will be sent to customer)"
                    End If

                    Dim completedMsg As String = ""
                    If moCartConfig("SendRecieptsFromAdmin") <> "off" Then
                        completedMsg = " - Payment Recieved (resends receipt)"
                    End If

                    'update the status if we have submitted it allready
                    If goRequest("nStatus") <> "" Then nStatus = CInt(goRequest("nStatus"))
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Update" & cSchemaName, "", "")
                    oGrp1Elmt = MyBase.addGroup(oFrmElmt, "Status", "", cSchemaName & " Status")
                    oSelElmt = MyBase.addSelect1(oGrp1Elmt, "nStatus", True, "Status", "", ApperanceTypes.Full)
                    Select Case nStatus
                        Case 0, 1, 2, 3, 4, 5 ' new
                            MyBase.addOption(oSelElmt, "Abandoned", 11)
                            MyBase.addOption(oSelElmt, "Delete", 12)
                        Case 6 ' Completed
                            MyBase.addOption(oSelElmt, "Awaiting Payment", 13, False, "Awaiting_Payment")
                            MyBase.addOption(oSelElmt, "Completed", 6, False, "Completed")
                            MyBase.addOption(oSelElmt, "Refunded", 7, False, "Refunded")
                            MyBase.addOption(oSelElmt, shippedStatus, 9, False, "Shipped")
                        Case 7 ' Refunded
                            MyBase.addOption(oSelElmt, "Completed" & completedMsg, 6)
                            MyBase.addOption(oSelElmt, "Refunded", 7)
                        Case 8 ' Failed
                            MyBase.addOption(oSelElmt, "Abandoned", 11)
                            MyBase.addOption(oSelElmt, "Delete", 12)
                        Case 9 ' Shipped
                            MyBase.addOption(oSelElmt, "Completed" & completedMsg, 6)
                            MyBase.addOption(oSelElmt, "Refunded", 7)
                            MyBase.addOption(oSelElmt, shippedStatus, 9)
                        Case 10 'Deposit Paid
                            MyBase.addOption(oSelElmt, "Deposit Paid", 10)
                            MyBase.addOption(oSelElmt, "Completed" & completedMsg, 6)
                            MyBase.addOption(oSelElmt, shippedStatus, 9)
                        Case 13 'Awaiting Payment
                            MyBase.addOption(oSelElmt, "Awaiting Payment", 13)
                            MyBase.addOption(oSelElmt, "Completed" & completedMsg, 6)
                            MyBase.addOption(oSelElmt, "Refunded", 7)
                            MyBase.addOption(oSelElmt, shippedStatus, 9)
                            MyBase.addOption(oSelElmt, "Delete", 12)
                        Case 17 ' In Progress
                            MyBase.addOption(oSelElmt, "Awaiting Payment", 13, False, "Awaiting_Payment")
                            MyBase.addOption(oSelElmt, "Completed", 6, False, "Completed")
                            MyBase.addOption(oSelElmt, "Refunded", 7, False, "Refunded")
                            MyBase.addOption(oSelElmt, shippedStatus, 9, False, "Shipped")

                    End Select
                    MyBase.addBind("nStatus", "tblCartOrder/nCartStatus", "true()")

                    If nStatus = 6 Or myWeb.moRequest("nStatus") = "9" Or nStatus = 17 Then
                        'Add carrier information
                        Dim oSwitch As XmlElement = MyBase.addSwitch(oGrp1Elmt, "")
                        Dim oCase As XmlElement = MyBase.addCase(oSwitch, "Awaiting_Payment")
                        Dim oCase1 As XmlElement = MyBase.addCase(oSwitch, "Completed")
                        Dim oCase2 As XmlElement = MyBase.addCase(oSwitch, "Refunded")
                        Dim oCase3 As XmlElement = MyBase.addCase(oSwitch, "Shipped")

                        'Turn of validation when switching back to completed
                        Dim validationOn As String = "true()"
                        If nStatus = 6 And myWeb.moRequest("nStatus") = "6" Then
                            validationOn = "false()"
                        End If

                        If LCase(moCartConfig("ShippedValidation")) = "off" Then
                            validationOn = "false()"
                        End If

                        If moDbHelper.checkDBObjectExists("tblCartCarrier") Then

                            Dim oCarrierElmt As XmlElement = MyBase.addGroup(oCase3, "Carrier", "inline", cSchemaName & " Carrier")

                            Dim CarrierSelect As XmlElement = MyBase.addSelect1(oCarrierElmt, "nCarrierId", True, "Carrier")
                            Dim oDr As SqlDataReader = moDbHelper.getDataReader("select cCarrierName as name, nCarrierKey as value from tblCartCarrier")
                            MyBase.addOptionsFromSqlDataReader(CarrierSelect, oDr)
                            MyBase.addBind("nCarrierId", "tblCartOrderDelivery/nCarrierId", validationOn)

                            MyBase.addInput(oCarrierElmt, "cCarrierRef", True, "Carrier Reference")
                            MyBase.addBind("cCarrierRef", "tblCartOrderDelivery/cCarrierRef", "false()")


                            MyBase.addInput(oCarrierElmt, "cCarrierNotes", True, "Carrier Notes", "long")
                            MyBase.addBind("cCarrierNotes", "tblCartOrderDelivery/cCarrierNotes", "false()")

                            Dim validClass As String = ""
                            If validationOn = "true()" Then
                                validClass = " required"
                            End If

                            MyBase.addInput(oCarrierElmt, "dExpectedDeliveryDate", True, "Target Delivery Date", "calendar" & validClass)
                            MyBase.addBind("dExpectedDeliveryDate", "tblCartOrderDelivery/dExpectedDeliveryDate", validationOn)

                            MyBase.addInput(oCarrierElmt, "dCollectionDate", True, "Collection Date", "calendar" & validClass)
                            MyBase.addBind("dCollectionDate", "tblCartOrderDelivery/dCollectionDate", validationOn)

                            Dim deliveryInstance As XmlElement = moPageXML.CreateElement("instance")
                            deliveryInstance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.CartDelivery)

                            deliveryInstance.SelectSingleNode("tblCartOrderDelivery/dCollectionDate").InnerText = xmlDate(Now())

                            MyBase.Instance.AppendChild(deliveryInstance.FirstChild)

                            'set the order id
                            MyBase.Instance.SelectSingleNode("tblCartOrderDelivery/nOrderId").InnerText = nOrderId
                        End If
                    End If

                    oGrp2Elmt = MyBase.addGroup(oFrmElmt, "Notes", "", "Notes")

                    ' Get the seller notes
                    Dim sellerNotes As String = Me.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText

                    tempElement = MyBase.addDiv(oGrp2Elmt, sellerNotes, "orderNotes", False)

                    Dim aSellerNotes As String() = Split(sellerNotes, "/n")
                    Dim cSellerNotesHtml As String = "<ul>"
                    For snCount As Integer = 0 To UBound(aSellerNotes)
                        cSellerNotesHtml = cSellerNotesHtml + "<li>" + Protean.Tools.Xml.convertEntitiesToCodes(aSellerNotes(snCount)) + "</li>"
                    Next
                    tempElement.InnerXml = cSellerNotesHtml + "</ul>"

                    MyBase.addTextArea(oGrp2Elmt, "cNotesAmend", False, "Update Seller Notes", "", "5")

                    MyBase.addSubmit(oGrp2Elmt, "ewUpdate" & cSchemaName, "Update Order")

                    If MyBase.isSubmitted Then
                        'MyBase.updateInstanceFromRequest()
                        MyBase.Instance.SelectSingleNode("tblCartOrder/nCartStatus").InnerText = goRequest("nStatus")
                        Dim sStatusDesc As String
                        Select Case goRequest("nStatus")
                            Case "0"
                                sStatusDesc = "New Cart"
                            Case "1"
                                sStatusDesc = "Items Added"
                            Case "2"
                                sStatusDesc = "Billing Address Added"
                            Case "3"
                                sStatusDesc = "Delivery Address Added"
                            Case "4"
                                sStatusDesc = "Confirmed"
                            Case "5"
                                sStatusDesc = "Pass for Payment"
                            Case "6"
                                sStatusDesc = "Completed"
                            Case "7"
                                sStatusDesc = "Refunded"
                            Case "8"
                                sStatusDesc = "Failed"
                            Case "9"
                                sStatusDesc = "Shipped"
                            Case "10"
                                sStatusDesc = "Deposit Paid"
                            Case "11"
                                sStatusDesc = "Abandoned"
                            Case "12"
                                sStatusDesc = "Deleted"
                            Case "13"
                                sStatusDesc = "Awaiting Payment"
                            Case Else
                                sStatusDesc = "No Change"
                        End Select

                        Dim updateNotes As String = goRequest("cNotesAmend")
                        '  If Not String.IsNullOrEmpty(updateNotes) Then updateNotes = ControlChars.CrLf & Now.ToString() & ":" & updateNotes

                        Dim notes As String = MyBase.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText & "/n" & Now() & ": changed to: (" & goRequest("nStatus") & ") " & sStatusDesc & " - " & updateNotes
                        Dim AdminUserName As String = myWeb.moPageXml.SelectSingleNode("Page/User/@name").InnerText
                        notes += "by " + AdminUserName

                        MyBase.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText = notes
                        moDbHelper.logActivity(dbHelper.ActivityType.OrderStatusChange, myWeb.mnUserId, 0, 0, notes)

                        aSellerNotes = Split(MyBase.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText, "/n")
                        cSellerNotesHtml = "<ul>"
                        For snCount As Integer = 0 To UBound(aSellerNotes)
                            cSellerNotesHtml = cSellerNotesHtml + "<li>" + Protean.Tools.Xml.convertEntitiesToCodes(aSellerNotes(snCount)) + "</li>"
                        Next
                        tempElement.InnerXml = cSellerNotesHtml + "</ul>"

                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartOrder, MyBase.Instance)

                            If goRequest("nStatus") = 9 Then

                                'Get the carrier name from the ID
                                If moDbHelper.checkDBObjectExists("tblCartCarrier") And moDbHelper.checkDBObjectExists("tblCartOrderDelivery") Then
                                    Dim CarrierName As String
                                    CarrierName = moDbHelper.GetDataValue("select cCarrierName from tblCartCarrier where nCarrierKey = " & myWeb.moRequest("nCarrierId"))
                                    Me.Instance.SelectSingleNode("tblCartOrderDelivery/cCarrierName").InnerText = CarrierName
                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartDelivery, MyBase.Instance)
                                End If

                                If sendEmailOnShipped Then
                                    Dim cSubject As String = moCartConfig("OrderEmailSubject")
                                    If String.IsNullOrEmpty(cSubject) Then cSubject = "Order Shipped"
                                    'send to customer
                                    Dim oMsg As Messaging = New Messaging(myWeb.msException)
                                    Dim cartXml As New XmlDocument
                                    Dim cartElement As XmlElement = cartXml.CreateElement("Cart")
                                    cartElement.InnerXml = MyBase.Instance.SelectSingleNode("tblCartOrder/cCartXml").InnerXml
                                    cartElement.SetAttribute("InvoiceRef", moCartConfig("OrderNoPrefix") & MyBase.Instance.SelectSingleNode("tblCartOrder/nCartOrderKey").InnerXml)
                                    cartElement.SetAttribute("InvoiceDate", Left(MyBase.Instance.SelectSingleNode("tblCartOrder/dInsertDate").InnerXml, 10))
                                    cartElement.SetAttribute("AccountId", MyBase.Instance.SelectSingleNode("tblCartOrder/nCartUserDirId").InnerXml)
                                    If Not MyBase.Instance.SelectSingleNode("tblCartOrderDelivery") Is Nothing Then
                                        Dim delElmt As XmlElement = cartXml.CreateElement("Delivery")
                                        delElmt.InnerXml = MyBase.Instance.SelectSingleNode("tblCartOrderDelivery").InnerXml
                                        cartElement.AppendChild(delElmt)

                                        Dim carrierId As Long = CInt(delElmt.SelectSingleNode("nCarrierId").InnerText)
                                        Dim carrierElmt As XmlElement = cartXml.CreateElement("Carrier")
                                        carrierElmt.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.CartCarrier, carrierId).Replace("{@code}", delElmt.SelectSingleNode("cCarrierRef").InnerText)
                                        cartElement.AppendChild(carrierElmt)
                                    End If
                                    Dim CustomerEmailShippedTemplatePath As String = IIf(moCartConfig("CustomerEmailShippedTemplatePath") <> "", moCartConfig("CustomerEmailShippedTemplatePath"), "/xsl/Cart/mailOrderCustomerDelivery.xsl")
                                    cProcessInfo = oMsg.emailer(cartElement, CustomerEmailShippedTemplatePath, moCartConfig("MerchantName"), moCartConfig("MerchantEmail"), (cartElement.SelectSingleNode("//Contact[@type='Billing Address']/Email").InnerText), "Order Shipped")

                                    oMsg = Nothing
                                End If
                            End If
                        End If
                    End If

                    MyBase.addValues()

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmUpdateOrder", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmRefundOrder(ByVal nOrderId As Long, ByVal providerName As String, ByVal providerPaymentReference As String) As XmlElement

                Dim cProcessInfo As String = ""
                Dim moCartConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/cart")
                Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/" & myWeb.moConfig("ProjectPath"))
                Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("protean/web")
                Try
                    Dim IsRefund As String = ""
                    Dim oCart As Protean.Cms.Cart = New Cart(myWeb)
                    MyBase.NewFrm("Refund")
                    MyBase.submission("Refund", "", "post", "form_check(this)")
                    Dim refundAmount As Decimal
                    Dim cResponse As String = ""   'check this
                    Dim xdoc As New XmlDocument()
                    Dim amount As String = ""
                    If (nOrderId > 0) Then
                        Dim cartXmlSql As String = "select cCartXml from tblCartOrder where nCartOrderKey = " & nOrderId
                        If (cartXmlSql <> "") Then
                            Dim orderXml As String = Convert.ToString(myWeb.moDbHelper.GetDataValue(cartXmlSql))
                            xdoc.LoadXml(orderXml)
                        End If
                        If (xdoc.InnerXml <> "") Then

                            Dim xn As XmlNode = xdoc.SelectSingleNode("/Order/PaymentDetails/instance/Response")
                            Dim xnInstance As XmlNode = xdoc.SelectSingleNode("/Order/PaymentDetails/instance")
                            If (xn IsNot Nothing And xnInstance IsNot Nothing) Then
                                amount = xnInstance.Attributes("AmountPaid").InnerText
                            End If
                        End If

                    End If

                    refundAmount = Convert.ToDouble(amount)

                    MyBase.Instance.InnerXml = "<Refund><RefundAmount> " & refundAmount & " </RefundAmount><ProviderName>" & providerName & "</ProviderName> <ProviderReference>" & providerPaymentReference & " </ProviderReference><OrderId>" & nOrderId & "</OrderId></Refund>"
                    Dim oFrmElmt As XmlElement
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Refund " & providerName, "", "")
                    MyBase.addInput(oFrmElmt, "RefundAmount", True, "Refund Amount")
                    MyBase.addBind("RefundAmount", "Refund/RefundAmount", "true()")

                    MyBase.addInput(oFrmElmt, "ProviderName", True, "Provider Name", "readonly")
                    MyBase.addBind("ProviderName", "Refund/ProviderName", "true()")

                    MyBase.addInput(oFrmElmt, "ProviderReference", True, "Provider Reference", "readonly")
                    MyBase.addBind("ProviderReference", "Refund/ProviderReference", "true()")

                    MyBase.addInput(oFrmElmt, "id", True, "Order Id", "readonly")
                    MyBase.addBind("id", "Refund/OrderId", "true()")

                    MyBase.addSubmit(oFrmElmt, "Refund", "Refund", "ewSubmit")

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If (amount >= refundAmount) Then
                            If MyBase.valid Then
                                'oCgfSect.SectionInformation.RestartOnExternalChanges = False    'check this
                                'oCgfSect.SectionInformation.SetRawXml(MyBase.Instance.InnerXml)
                                'oCfg.Save()

                                Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, providerName)
                                IsRefund = oPayProv.Activities.RefundPayment(providerPaymentReference, refundAmount)
                                If (IsRefund Is Nothing) Then
                                    MyBase.addNote("Refund", noteTypes.Alert, "Refund Failed")
                                    myWeb.msRedirectOnEnd = "/?ewCmd=Orders&ewCmd2=Display&id=" + nOrderId
                                End If
                                'Update Seller Notes:
                                Dim sSql As String = "select * from tblCartOrder where nCartOrderKey = " & nOrderId
                                Dim oDs As DataSet
                                Dim oRow As DataRow
                                oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                                For Each oRow In oDs.Tables("Order").Rows
                                    If (IsRefund IsNot Nothing) Then
                                        oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today & " " & TimeOfDay & ": changed to: (Refund Payment Successful) " & vbLf & "comment: " & "Refund amount:" & refundAmount & vbLf & "Full Response:' Refunded Amount is " & refundAmount & " And ReceiptId is: " & IsRefund & "'"
                                    Else
                                        oRow("cSellerNotes") = oRow("cSellerNotes") & vbLf & Today & " " & TimeOfDay & ": changed to: (Refund Payment Failed) " & vbLf & "comment: " & "Refund amount:" & refundAmount & vbLf & "Full Response:' Refunded Amount is " & refundAmount & " And ReceiptId is: " & IsRefund & "'"
                                    End If
                                Next
                                myWeb.moDbHelper.updateDataset(oDs, "Order")

                            End If
                        End If

                    End If
                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmRefundOrder", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmFindRelated(ByVal nParentID As String, ByVal cContentType As String, ByRef oPageDetail As XmlElement, ByVal nParId As String, ByVal bIgnoreParID As Boolean, ByVal cTableName As String, ByVal cSelectField As String, ByVal cFilterField As String, Optional ByVal redirect As String = "") As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt1 As XmlElement
                Dim oSelElmt2 As XmlElement
                Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")
                Dim bCascade As Boolean = False
                Dim cProcessInfo As String = ""

                Try
                    Dim cParentContentName As String = convertEntitiesToCodes(moDbHelper.getNameByKey(dbHelper.objectTypes.Content, nParentID))

                    MyBase.NewFrm("FindRelatedContent")
                    MyBase.Instance.InnerXml = "<nParentContentId>" & nParentID & "</nParentContentId>" &
                          "<cSchemaName>" & cContentType & "</cSchemaName>" &
                        "<cSection/><nSearchChildren/><nIncludeRelated/><cParentContentName>" & cParentContentName & "</cParentContentName><redirect>" & redirect & "</redirect><cSearch/>"

                    'MyBase.submission("AddRelated", "?ewCmd=RelateSearch&Type=Document&xml=x", "post", "form_check(this)")
                    MyBase.submission("AddRelated", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "SearchRelated", , "Search For Related " & cContentType)

                    'Definitions
                    If redirect <> "" Then
                        MyBase.addInput(oFrmElmt, "redirect", True, "redirect", "hidden")
                        MyBase.addBind("redirect", "redirect")
                    End If
                    MyBase.addInput(oFrmElmt, "nParentContentId", True, "nParentContentId", "hidden")
                    MyBase.addBind("nParentContentId", "nParentContentId")

                    MyBase.addInput(oFrmElmt, "cSchemaName", True, "cSchemaName", "hidden")
                    MyBase.addBind("cSchemaName", "cSchemaName")

                    'What we are searching for
                    MyBase.addInput(oFrmElmt, "cSearch", True, "Search Text")
                    MyBase.addBind("cSearch", "cSearch", "false()")

                    'Pages
                    oSelElmt1 = MyBase.addSelect1(oFrmElmt, "cSection", False, "Page", , ApperanceTypes.Minimal)
                    MyBase.addOption(oSelElmt1, "All", 0)
                    MyBase.addOption(oSelElmt1, "All Orphan " & cContentType & "s", -1)
                    Dim cSQL As String
                    cSQL = "SELECT tblContentStructure.* FROM tblContentStructure ORDER BY nStructOrder"
                    Dim oDS As New DataSet
                    oDS = moDbHelper.GetDataSet(cSQL, "Menu", "Struct")
                    oDS.Relations.Add("RelMenu", oDS.Tables("Menu").Columns("nStructKey"), oDS.Tables("Menu").Columns("nStructParID"), False)
                    oDS.Relations("RelMenu").Nested = True
                    Dim oMenuXml As New XmlDocument
                    oMenuXml.InnerXml = oDS.GetXml
                    Dim oMenuElmt As XmlElement
                    For Each oMenuElmt In oMenuXml.SelectNodes("descendant-or-self::Menu")
                        Dim oTmpNode As XmlElement = oMenuElmt
                        Dim cNameString As String = ""
                        Do Until oTmpNode.ParentNode.Name = "Struct"
                            cNameString &= "-"
                            oTmpNode = oTmpNode.ParentNode
                        Loop
                        cNameString &= oMenuElmt.SelectSingleNode("cStructName").InnerText
                        MyBase.addOption(oSelElmt1, cNameString, oMenuElmt.SelectSingleNode("nStructKey").InnerText)
                    Next
                    MyBase.addBind("cSection", "cSection", "true()")
                    'Search sub pages
                    oSelElmt2 = MyBase.addSelect(oFrmElmt, "nSearchChildren", True, "&#160;", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt2, "Search all sub-pages", 1)
                    MyBase.addBind("nSearchChildren", "nSearchChildren", "false()")

                    If cContentType.Contains("Product") And cContentType.Contains("SKU") Then
                        oSelElmt2 = MyBase.addSelect(oFrmElmt, "nIncludeRelated", True, "&#160;", "", ApperanceTypes.Full)
                        MyBase.addOption(oSelElmt2, "Include Related Sku's", 1)
                        MyBase.addBind("nIncludeRelated", "nIncludeRelated", "false()")
                    End If

                    'search button
                    MyBase.addSubmit(oFrmElmt, "Search", "Search", "ewSubmit")

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.addValues()
                        MyBase.validate()
                        If MyBase.valid Then
                            'Dim nPar As Integer = goRequest.QueryString("GroupId")
                            If Not IsNumeric(nParId) Then
                                Dim oParElmt As XmlElement = MyBase.Instance.SelectSingleNode(nParId)
                                If Not oParElmt Is Nothing Then nParId = oParElmt.InnerText
                            End If
                            Dim nRoot As Integer = MyBase.Instance.SelectSingleNode("cSection").InnerText
                            Dim bChilds As Boolean = IIf(MyBase.Instance.SelectSingleNode("nSearchChildren").InnerText = "1", True, False)
                            Dim cExpression As String = MyBase.Instance.SelectSingleNode("cSearch").InnerText
                            Dim bIncRelated As Boolean = IIf(MyBase.Instance.SelectSingleNode("nIncludeRelated").InnerText = "1", True, False)

                            Dim sSQL As String = "Select " & cSelectField & " From " & cTableName & " WHERE " & cFilterField & " = " & nParId
                            Dim oDre As SqlDataReader = moDbHelper.getDataReader(sSQL)
                            Dim cTmp As String = ""
                            Do While oDre.Read
                                cTmp &= oDre(0) & ","
                            Loop
                            oDre.Close()
                            If Not cTmp = "" Then cTmp = Left(cTmp, Len(cTmp) - 1)

                            oPageDetail.AppendChild(moDbHelper.RelatedContentSearch(nRoot, cContentType, bChilds, cExpression, nParId, IIf(bIgnoreParID, 0, nParId), cTmp.Split(","), bIncRelated))

                        End If

                    Else
                        MyBase.Instance.InnerXml = "<nParentContentId>" & nParentID & "</nParentContentId>" &
                          "<cSchemaName>" & cContentType & "</cSchemaName>" &
                          "<cSection>0</cSection>" &
                          "<nSearchChildren>1</nSearchChildren>" &
                          "<cParentContentName>" & cParentContentName & "</cParentContentName>" &
                          "<redirect>" & redirect & "</redirect><cSearch/>"
                        MyBase.addValues()
                    End If
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmFindRelated", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmProductGroup(ByVal nGroupId As Integer, Optional ByVal SchemaName As String = "Discount") As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oGrp1Elmt As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    MyBase.NewFrm("EditProductGroup")
                    MyBase.Instance.InnerXml = "<tblCartProductCategories><nCatKey/><cCatSchemaName>" & SchemaName & "</cCatSchemaName><cCatForeignRef/><nCatParentId/><cCatName/><cCatDescription/><nAuditId/></tblCartProductCategories>"
                    If nGroupId > 0 Then
                        MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.CartProductCategories, nGroupId)
                    End If
                    MyBase.submission("EditProductGroup", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "ProductGroup")
                    MyBase.addNote("pgheader", noteTypes.Help, IIf(nGroupId > 0, "Edit ", "Add ") & "Product Group")
                    oGrp1Elmt = MyBase.addGroup(oFrmElmt, "Details", "1col", "Details")

                    'Definitions
                    MyBase.addInput(oGrp1Elmt, "nCatKey", True, "nCatKey", "hidden")
                    MyBase.addBind("nCatKey", "tblCartProductCategories/nCatKey")
                    'MyBase.addInput(oGrp1Elmt, "cCatSchemaName", True, "Schema")
                    'MyBase.addBind("cCatSchemaName", "tblCartProductCategories/cCatSchemaName", "true()")
                    'MyBase.addInput(oGrp1Elmt, "cCatForeignRef", True, "Foreign Ref")
                    'MyBase.addBind("cCatForeignRef", "tblCartProductCategories/cCatForeignRef")
                    'MyBase.addInput(oGrp1Elmt, "nCatParentId", True, "nCatParentId", "hidden")
                    'MyBase.addBind("nCatParentId", "tblCartProductCategories/nCatParentId")
                    MyBase.addInput(oGrp1Elmt, "cCatName", True, "Name")
                    MyBase.addBind("cCatName", "tblCartProductCategories/cCatName", "true()")

                    Dim oSchemaSelect As XmlElement = MyBase.addSelect1(oGrp1Elmt, "cCatSchemaName", True, "Group Type",, ApperanceTypes.Full)
                    MyBase.addOption(oSchemaSelect, SchemaName, SchemaName)
                    Dim aOptions() As String = Nothing
                    If Not myWeb.moCart.moCartConfig("ProductCategoryTypes") Is Nothing Then
                        aOptions = myWeb.moCart.moCartConfig("ProductCategoryTypes").Split(",")
                        If aOptions.Length > 0 Then
                            For i As Integer = 0 To aOptions.Length - 1
                                MyBase.addOption(oSchemaSelect, aOptions(i), aOptions(i))
                            Next
                        End If
                    End If
                    MyBase.addBind("cCatSchemaName", "tblCartProductCategories/cCatSchemaName")


                    MyBase.addTextArea(oGrp1Elmt, "cCatDescription", True, "Description", , 15, 50)
                    MyBase.addBind("cCatDescription", "tblCartProductCategories/cCatDescription")

                    MyBase.addInput(oGrp1Elmt, "nAuditId", True, "nAuditId", "hidden")
                    MyBase.addBind("nAuditId", "tblCartProductCategories/nAuditId")

                    'search button

                    MyBase.addSubmit(oFrmElmt, "EditProductGroup", "Save Product Group", "SaveProductGroup")


                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.addValues()
                        MyBase.validate()
                        If MyBase.valid Then
                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartProductCategories, MyBase.Instance, IIf(nGroupId > 0, nGroupId, -1))
                        End If
                    End If
                    MyBase.addValues()
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmProductGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDiscountRule(ByVal nDiscountId As Integer, Optional ByVal nDiscountType As Integer = 0) As XmlElement

                Dim cProcessInfo As String = ""

                Dim cTypePath As String
                Dim eDiscountType As stdTools.DiscountCategory
                Dim sSQL As String
                Try
                    If nDiscountType > 0 Then eDiscountType = nDiscountType
                    If nDiscountId > 0 Then
                        sSQL = "Select nDiscountCat From tblCartDiscountRules WHERE nDiscountKey = " & nDiscountId
                        nDiscountType = moDbHelper.ExeProcessSqlScalar(sSQL)
                    End If

                    If nDiscountType > 0 Then
                        eDiscountType = nDiscountType
                        cTypePath = "DiscountRule_" & eDiscountType.ToString & ".xml"
                    Else
                        cTypePath = "DiscountRule.xml"
                    End If

                    MyBase.NewFrm("EditDiscountRules")
                    If Not MyBase.load("/xforms/discounts/" & cTypePath, myWeb.maCommonFolders) Then
                        'not allot we can do really except try defaults
                        If Not MyBase.load("/xforms/discounts/DiscountRule.xml", myWeb.maCommonFolders) Then
                            'not allot we can do really 
                        End If
                    End If

                    Dim existingInstance As XmlElement = MyBase.moXformElmt.OwnerDocument.CreateElement("instance")


                    If nDiscountId > 0 Then
                        existingInstance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.CartDiscountRules, nDiscountId)
                        LoadInstanceFromInnerXml(existingInstance.InnerXml)
                    End If
                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.addValues()
                        MyBase.validate()
                        If MyBase.valid Then
                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartDiscountRules, MyBase.Instance, IIf(nDiscountId > 0, nDiscountId, -1))
                        End If
                    End If
                    MyBase.addValues()
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmDiscountRule", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDiscountProductRelations(ByVal id As Long, ByVal dname As String) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oFrmGrp1 As XmlElement
                Dim oFrmGrp2 As XmlElement
                Dim oFrmGrp3 As XmlElement
                Dim oElmt2 As XmlElement
                Dim oElmt4 As XmlElement
                Dim sSql As String

                Dim cProcessInfo As String = ""

                Try

                    MyBase.NewFrm("EditDiscountProductRelations")

                    MyBase.submission("EditDiscountRelations", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditRelations", "3col", "Product Group Relations for Discount")

                    oFrmGrp1 = MyBase.addGroup(oFrmElmt, "AllObjects", "", "Select the product groups you want to have access to this discount")
                    MyBase.addNote(oFrmGrp1, xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names")

                    'add the buttons so we can test for submission
                    oFrmGrp2 = MyBase.addGroup(oFrmElmt, "EditRelations", "RelationButtons", "Buttons")
                    MyBase.addSubmit(oFrmGrp2, "AddSelected", "Add Selected >", "", "PermissionButtons")
                    MyBase.addSubmit(oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons")

                    Select Case MyBase.getSubmitted
                        Case "AddSelected"
                            moDbHelper.saveDiscountProdGroupRelation(id, goRequest("Groups"))
                        Case "RemoveSelected"
                            moDbHelper.saveDiscountProdGroupRelation(id, goRequest("Items"), False)
                    End Select

                    oElmt2 = MyBase.addSelect(oFrmGrp1, "Groups", False, "Product Groups", "scroll_10", xForm.ApperanceTypes.Minimal)
                    sSql = "SELECT nCatKey AS value, cCatName AS name" &
                    " FROM tblCartProductCategories" &
                    " WHERE (cCatSchemaName = N'Discount') AND" &
                    " (((SELECT nDiscountProdCatRelationKey" &
                    " FROM tblCartDiscountProdCatRelations" &
                    " WHERE (nProductCatId = tblCartProductCategories.nCatKey) AND (nDiscountId = " & id & "))) IS NULL)" &
                    " ORDER BY cCatName"
                    MyBase.addOptionsFromSqlDataReader(oElmt2, moDbHelper.getDataReader(sSql), "name", "value")


                    oFrmGrp3 = MyBase.addGroup(oFrmElmt, "RelatedObjects", "", "All items with permissions to access page")
                    MyBase.addNote(oFrmGrp3, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")

                    oElmt4 = MyBase.addSelect(oFrmGrp3, "Items", False, "Related", "scroll_10", xForm.ApperanceTypes.Minimal)

                    sSql = "SELECT tblCartDiscountProdCatRelations.nDiscountProdCatRelationKey as value, tblCartProductCategories.cCatName as name" &
                    " FROM tblCartDiscountProdCatRelations INNER JOIN tblCartProductCategories ON tblCartDiscountProdCatRelations.nProductCatId = tblCartProductCategories.nCatKey" &
                    " WHERE (tblCartDiscountProdCatRelations.nDiscountId = " & id & ") ORDER BY tblCartProductCategories.cCatName"

                    MyBase.addOptionsFromSqlDataReader(oElmt4, moDbHelper.getDataReader(sSql), "name", "value")

                    MyBase.Instance.InnerXml = "<relations/>"

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmDiscountDirRelations(ByVal id As Long, ByVal dname As String) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oFrmGrp1 As XmlElement
                Dim oFrmGrp2 As XmlElement
                Dim oFrmGrp3 As XmlElement
                Dim oElmt2 As XmlElement
                Dim oElmt4 As XmlElement
                Dim oElmt5 As XmlElement
                Dim sSql As String

                Dim cProcessInfo As String = ""
                Dim bDeny As Boolean
                Dim cDenyFilter As String = ""
                Try

                    MyBase.NewFrm("EditDiscountProductDirs")

                    If moDbHelper.checkTableColumnExists("tblCartDiscountDirRelations", "nPermLevel") Then
                        bDeny = True
                        cDenyFilter = " and nPermLevel <> 0"
                    End If

                    MyBase.submission("EditDiscountDirs", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditDirs", "3col", "User Group Relations for Discount " & dname)

                    oFrmGrp1 = MyBase.addGroup(oFrmElmt, "AllObjects", "", "Select the user groups you want to have access to this discount")
                    MyBase.addNote(oFrmGrp1, xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names")

                    'add the buttons so we can test for submission
                    oFrmGrp2 = MyBase.addGroup(oFrmElmt, "EditDirs", "DirButtons", "Buttons")
                    MyBase.addSubmit(oFrmGrp2, "AddSelected", "Add Selected >", "", "PermissionButtons")
                    If bDeny Then
                        MyBase.addSubmit(oFrmGrp2, "DenySelected", "Deny Selected >", "", "PermissionButtons")
                    End If
                    MyBase.addSubmit(oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons")

                    Select Case MyBase.getSubmitted
                        Case "AddSelected"
                            moDbHelper.saveDiscountDirRelation(id, goRequest("Groups"))
                        Case "DenySelected"
                            If bDeny Then
                                moDbHelper.saveDiscountDirRelation(id, goRequest("Groups"), True, dbHelper.PermissionLevel.Denied)
                            End If
                        Case "RemoveSelected"
                            moDbHelper.saveDiscountDirRelation(id, goRequest("Items"), False)
                    End Select

                    oElmt2 = MyBase.addSelect(oFrmGrp1, "Groups", False, "User Groups", "scroll_10", xForm.ApperanceTypes.Minimal)
                    'Dim nxxx As Integer = moDbhelper.exeProcessSQLScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " & id & ") AND (nDirId = 0)")
                    If Not moDbHelper.ExeProcessSqlScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " & id & ") AND (nDirId = 0)") > 0 Then
                        MyBase.addOption(oElmt2, "<<All Users>>", 0)
                    End If
                    sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Group') AND" &
                    " (((SELECT nDiscountDirRelationKey" &
                    " FROM tblCartDiscountDirRelations" &
                    " WHERE (nDiscountId = " & id & ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" &
                    "ORDER BY cDirName"
                    MyBase.addOptionsFromSqlDataReader(oElmt2, moDbHelper.getDataReader(sSql), "name", "value")

                    oFrmGrp3 = MyBase.addGroup(oFrmElmt, "RelatedObjects", "", "All items with permissions to access page")
                    MyBase.addNote(oFrmGrp3, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")

                    oElmt4 = MyBase.addSelect(oFrmGrp3, "Items", False, "Related", "scroll_10", xForm.ApperanceTypes.Minimal)

                    sSql = "SELECT tblCartDiscountDirRelations.nDiscountDirRelationKey AS value, " &
                    " CASE WHEN tblCartDiscountDirRelations.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" &
                    " FROM tblCartDiscountDirRelations LEFT OUTER JOIN" &
                    "  tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey" &
                    " WHERE (tblCartDiscountDirRelations.ndiscountid = " & id & ")" & cDenyFilter & " ORDER BY cDirName"

                    MyBase.addOptionsFromSqlDataReader(oElmt4, moDbHelper.getDataReader(sSql), "name", "value")

                    If bDeny Then

                        oElmt5 = MyBase.addSelect(oFrmGrp3, "Items", False, "Denied", "scroll_10", xForm.ApperanceTypes.Minimal)

                        sSql = "SELECT tblCartDiscountDirRelations.nDiscountDirRelationKey AS value, " &
                    " CASE WHEN tblCartDiscountDirRelations.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" &
                    " FROM tblCartDiscountDirRelations LEFT OUTER JOIN" &
                    "  tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey" &
                    " WHERE (tblCartDiscountDirRelations.ndiscountid = " & id & ") and nPermLevel = 0 ORDER BY cDirName"

                        MyBase.addOptionsFromSqlDataReader(oElmt5, moDbHelper.getDataReader(sSql), "name", "value")
                    End If

                    MyBase.Instance.InnerXml = "<Dirs/>"

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmDiscountDirRelations", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmShippingDirRelations(ByVal id As Long, ByVal dname As String) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oFrmGrp1 As XmlElement
                Dim oFrmGrp2 As XmlElement
                Dim oFrmGrp3 As XmlElement
                Dim oElmt2 As XmlElement
                Dim oElmt4 As XmlElement
                Dim oElmt5 As XmlElement
                Dim sSql As String
                Dim bDeny As Boolean
                Dim cProcessInfo As String = ""
                Dim cDenyFilter As String = ""
                Try

                    MyBase.NewFrm("EditShippingDirRelations")

                    If moDbHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel") Then
                        bDeny = True
                        cDenyFilter = " and nPermLevel <> 0"
                    End If


                    MyBase.submission("EditInputPageRights", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "EditDirs", "3col", "User Group Relations for Shipping Method " & dname)

                    oFrmGrp1 = MyBase.addGroup(oFrmElmt, "AllObjects", "", "Select the user groups you want to have access to this shipping method")
                    MyBase.addNote(oFrmGrp1, xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names")

                    'add the buttons so we can test for submission
                    oFrmGrp2 = MyBase.addGroup(oFrmElmt, "EditDirs", "DirButtons", "Buttons")
                    MyBase.addSubmit(oFrmGrp2, "AddSelected", "Allow Selected >", "", "PermissionButtons")
                    If bDeny Then
                        MyBase.addSubmit(oFrmGrp2, "DenySelected", "Deny Selected >", "", "PermissionButtons")
                    End If
                    MyBase.addSubmit(oFrmGrp2, "RemoveSelected", "< Remove Selected", "", "PermissionButtons")

                    Select Case MyBase.getSubmitted
                        Case "AddSelected"
                            If goRequest("Groups") <> "" Then
                                moDbHelper.saveShippingDirRelation(id, goRequest("Groups"))
                            End If
                            If goRequest("Roles") <> "" Then
                                moDbHelper.saveShippingDirRelation(id, goRequest("Roles"))
                            End If
                        Case "DenySelected"
                            If bDeny Then
                                If goRequest("Groups") <> "" Then
                                    moDbHelper.saveShippingDirRelation(id, goRequest("Groups"), True, dbHelper.PermissionLevel.Denied)
                                End If
                                If goRequest("Roles") <> "" Then
                                    moDbHelper.saveShippingDirRelation(id, goRequest("Roles"), True, dbHelper.PermissionLevel.Denied)
                                End If
                            End If
                        Case "RemoveSelected"
                            moDbHelper.saveShippingDirRelation(id, goRequest("Items"), False)
                    End Select

                    oElmt2 = MyBase.addSelect(oFrmGrp1, "Groups", False, "User Groups", "scroll_10", xForm.ApperanceTypes.Minimal)
                    'Dim nxxx As Integer = moDbhelper.exeProcessSQLScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " & id & ") AND (nDirId = 0)")
                    If Not moDbHelper.ExeProcessSqlScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " & id & ") AND (nDirId = 0)") > 0 Then
                        MyBase.addOption(oElmt2, "<<All Users>>", 0)
                    End If
                    sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Group') AND" &
                    " (((SELECT nCartShippingPermissionKey" &
                    " FROM tblCartShippingPermission" &
                    " WHERE (nShippingMethodId = " & id & ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" &
                    "ORDER BY cDirName"
                    MyBase.addOptionsFromSqlDataReader(oElmt2, moDbHelper.getDataReader(sSql), "name", "value")

                    oElmt2 = MyBase.addSelect(oFrmGrp1, "Roles", False, "User Roles", "scroll_10", xForm.ApperanceTypes.Minimal)

                    sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Role') AND" &
                    " (((SELECT nCartShippingPermissionKey" &
                    " FROM tblCartShippingPermission" &
                    " WHERE (nShippingMethodId = " & id & ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" &
                    "ORDER BY cDirName"
                    MyBase.addOptionsFromSqlDataReader(oElmt2, moDbHelper.getDataReader(sSql), "name", "value")



                    oFrmGrp3 = MyBase.addGroup(oFrmElmt, "RelatedObjects", "", "All Groups with permissions for Shipping Method")
                    MyBase.addNote(oFrmGrp3, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")

                    oElmt4 = MyBase.addSelect(oFrmGrp3, "Items", False, "Allowed", "scroll_10", xForm.ApperanceTypes.Minimal)


                    sSql = "SELECT tblCartShippingPermission.nCartShippingPermissionKey AS value, " &
                    " CASE WHEN tblCartShippingPermission.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" &
                    " FROM tblCartShippingPermission LEFT OUTER JOIN" &
                    " tblDirectory ON tblCartShippingPermission.nDirId = tblDirectory.nDirKey" &
                    " WHERE (tblCartShippingPermission.nShippingMethodId = " & id & ")" & cDenyFilter & " ORDER BY cDirName"

                    MyBase.addOptionsFromSqlDataReader(oElmt4, moDbHelper.getDataReader(sSql), "name", "value")

                    If bDeny Then

                        oElmt5 = MyBase.addSelect(oFrmGrp3, "Items", False, "Denied", "scroll_10", xForm.ApperanceTypes.Minimal)

                        sSql = "SELECT tblCartShippingPermission.nCartShippingPermissionKey AS value, " &
                        " CASE WHEN tblCartShippingPermission.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" &
                        " FROM tblCartShippingPermission LEFT OUTER JOIN" &
                        "  tblDirectory ON tblCartShippingPermission.nDirId = tblDirectory.nDirKey" &
                        " WHERE (tblCartShippingPermission.nShippingMethodId = " & id & ") and nPermLevel = 0 ORDER BY cDirName"

                        MyBase.addOptionsFromSqlDataReader(oElmt5, moDbHelper.getDataReader(sSql), "name", "value")
                    End If

                    MyBase.Instance.InnerXml = "<Dirs/>"

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmShippingDirRelations", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmEditDirectoryContact(Optional ByVal id As Long = 0, Optional ByVal nUID As Integer = 0, Optional xFormPath As String = "/xforms/directory/UserContact.xml") As XmlElement
                Dim cProcessInfo As String = ""
                Try


                    MyBase.NewFrm("EditContact")
                    MyBase.load(xFormPath, myWeb.maCommonFolders)

                    If id > 0 Then
                        MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.CartContact, id)
                    End If

                    ' Add the countries list to the form
                    If Not MyBase.moXformElmt.SelectSingleNode("//select1[@bind='cContactCountry']") Is Nothing Then
                        Dim oEc As Protean.Cms.Cart = New Cart(myWeb)
                        oEc.populateCountriesDropDown(Me, MyBase.moXformElmt.SelectSingleNode("//select1[@bind='cContactCountry']"), "Billing Address")
                        oEc.close()
                        oEc = Nothing
                    End If

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        'MyBase.instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = myweb.mnUserId
                        MyBase.addValues()
                        If nUID > 0 Then
                            'if not supplied we do not want to overwrite it.
                            MyBase.Instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = nUID
                        End If

                        MyBase.validate()
                        If MyBase.valid Then
                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, MyBase.Instance, IIf(id > 0, id, -1))
                            Try
                                MyBase.addNote(MyBase.moXformElmt.SelectSingleNode("group/group(1)"), xForm.noteTypes.Alert, "Successfully Updated", True)
                            Catch exp As Exception
                                MyBase.addNote(MyBase.moXformElmt.SelectSingleNode("group"), xForm.noteTypes.Alert, "Successfully Updated", True)
                            End Try
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmDiscountRule", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmContentLocationDetail(ByVal nPageId As Integer, ByVal nContentId As Integer) As XmlElement
                Dim oFrmElmt As XmlElement


                Dim cProcessInfo As String = ""
                Try
                    MyBase.NewFrm("EditContent")

                    MyBase.submission("EditContent", "", "post", "")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "RelationshipType", "", "Please select a relationship type.")
                    Dim oSelElmt As XmlElement = MyBase.addSelect1(oFrmElmt, "nRelType", True, "Type", "required", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Primary", 1)
                    MyBase.addOption(oSelElmt, "Link", 0)
                    MyBase.addBind("nRelType", "nRelType", "true()", "number")

                    MyBase.addSubmit(oFrmElmt, "Save", "Save Changes")
                    Dim nPrimary As Integer = 0
                    Dim cRes As String = LCase(moDbHelper.ExeProcessSqlScalar("Select bPrimary from tblContentLocation where nStructId = " & nPageId & " and nContentId = " & nContentId))
                    If cRes = "true" Then
                        nPrimary = 1
                    End If
                    MyBase.Instance.InnerXml = "<nRelType>" & nPrimary & "</nRelType>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            Dim bResult As Boolean = IIf(MyBase.Instance.SelectSingleNode("nRelType").InnerText = 1, True, False)
                            bResult = moDbHelper.updateLocationsDetail(nContentId, nPageId, bResult)
                            valid = True
                            If Not bResult Then
                                MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, "Cannot remove the only Primary Relationship", True)
                                valid = False
                            End If
                        Else
                            valid = False
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            'Public Function xFrmPreviewNewsLetter(ByVal nPageId As Integer, ByRef oPageDetail As XmlElement) As XmlElement
            '    Dim oFrmElmt As XmlElement


            '    Dim cProcessInfo As String = ""
            '    Try
            '        MyBase.NewFrm("SendNewsLetter")

            '        MyBase.submission("SendNewsLetter", "", "post", "")

            '        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Unpersonalised", "", "Send Unpersonalised")

            '        Dim oElmt As XmlElement
            '        oElmt = MyBase.addInput(oFrmElmt, "cEmail", True, "Email address to send to", "long")
            '        MyBase.addBind("cEmail", "cEmail")
            '        oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"))
            '        MyBase.addSubmit(oFrmElmt, "SendUnpersonalised", "Send Unpersonalised")

            '        ' Uncomment for personalised
            '        ' ''oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Personalised", "", "Send Personalised")
            '        ' ''Dim cSQL As String = "SELECT tblDirectory.nDirKey, tblDirectory.cDirName" & _
            '        ' ''" FROM tblDirectory INNER JOIN" & _
            '        ' ''" tblDirectoryRelation ON tblDirectory.nDirKey = tblDirectoryRelation.nDirChildId INNER JOIN" & _
            '        ' ''" tblDirectory Role ON tblDirectoryRelation.nDirParentId = Role.nDirKey" & _
            '        ' ''" WHERE (tblDirectory.cDirSchema = N'User') AND (Role.cDirSchema = N'Role') AND (Role.cDirName = N'Administrator')" & _
            '        ' ''" ORDER BY tblDirectory.cDirName"
            '        ' ''Dim oDre As SqlDataReader = moDbhelper.getDataReader(cSQL)
            '        ' ''Dim oSelElmt As XmlElement = MyBase.addSelect1(oFrmElmt, "cUsers", True, "Select admin user to send to", "short", ApperanceTypes.Minimal)
            '        ' ''Do While oDre.Read
            '        ' ''    MyBase.addOption(oSelElmt, oDre(1), oDre(0))
            '        ' ''Loop
            '        ' ''MyBase.addBind("cUsers", "cUsers")
            '        ' ''MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

            '        MyBase.Instance.InnerXml = "<cEmail/><cUsers/>"

            '        If MyBase.isSubmitted Then
            '            MyBase.updateInstanceFromRequest()
            '            MyBase.validate()
            '            Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cEmail")
            '            If Not is_valid_email(oEmailElmt.InnerText) Then
            '                MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
            '                MyBase.valid = False
            '            End If
            '            If MyBase.valid Then
            '                Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
            '                Dim cEmail As String = MyBase.Instance.SelectSingleNode("cEmail").InnerText
            '                'first we will only deal with unpersonalised
            '                Dim oMessager As New Messaging
            '                'get the subject
            '                Dim cSubject As String = ""
            '                Dim oMessaging As New Protean.Messaging
            '                If oMessaging.SendSingleMail_Queued(nPageId, moMailConfig("MailingXsl"), cEmail, moMailConfig("SenderEmail"), moMailConfig("SenderName"), cSubject) Then
            '                    'add mssage and return to form so they can sen another

            '                    Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")

            '                    oMsgElmt.SetAttribute("type", "Message")
            '                    oMsgElmt.InnerText = "Messages Sent"
            '                    oPageDetail.AppendChild(oMsgElmt)
            '                End If
            '            End If
            '        End If

            '        MyBase.addValues()
            '        Return MyBase.moXformElmt

            '    Catch ex As Exception
            '        returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
            '        Return Nothing
            '    End Try
            'End Function

            'Public Function xFrmSendNewsLetter(ByVal nPageId As Integer, ByVal cPageName As String, ByVal cDefaultEmail As String, ByVal cDefaultEmailName As String, ByRef oPageDetail As XmlElement) As XmlElement
            '    Dim oFrmElmt As XmlElement
            '    Dim oCol1 As XmlElement
            '    Dim oCol2 As XmlElement

            '    Dim cProcessInfo As String = ""
            '    Try
            '        MyBase.NewFrm("SendNewsLetter")

            '        MyBase.submission("SendNewsLetter", "", "post", "")

            '        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Groups", "2col", "Please select a group(s) to send to.")

            '        cDefaultEmail = Trim(cDefaultEmail)

            '        oCol1 = MyBase.addGroup(oFrmElmt, "", "col1", "")
            '        oCol2 = MyBase.addGroup(oFrmElmt, "", "col2", "")

            '        Dim oElmt As XmlElement
            '        oElmt = MyBase.addInput(oCol1, "cDefaultEmail", True, "Email address to send from", "required long")
            '        MyBase.addBind("cDefaultEmail", "cDefaultEmail", "true()")
            '        oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

            '        Dim oElmt2 As XmlElement
            '        oElmt2 = MyBase.addInput(oCol1, "cDefaultEmailName", True, "Name to send from", "required long")
            '        MyBase.addBind("cDefaultEmailName", "cDefaultEmailName", "true()")
            '        oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))

            '        oElmt2 = MyBase.addInput(oCol1, "cSubject", True, "Subject", "required long")
            '        MyBase.addBind("cSubject", "cSubject", "true()")
            '        oElmt2.AppendChild(oElmt.OwnerDocument.CreateElement("value"))


            '        Dim cSQL As String = "SELECT nDirKey, cDirName  FROM tblDirectory WHERE (cDirSchema = 'Group') ORDER BY cDirName"
            '        Dim oDre As SqlDataReader = moDbHelper.getDataReader(cSQL)
            '        Dim oSelElmt As XmlElement = MyBase.addSelect(oCol2, "cGroups", True, "Select Groups to send to", "required multiline", ApperanceTypes.Full)
            '        Do While oDre.Read
            '            MyBase.addOption(oSelElmt, oDre(1), oDre(0))
            '        Loop
            '        MyBase.addBind("cGroups", "cGroups", "true()")

            '        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Send", "", "")

            '        MyBase.addSubmit(oFrmElmt, "SendUnpersonalised", "Send Unpersonalised")
            '        ' Uncomment for personalised
            '        'MyBase.addSubmit(oFrmElmt, "SendPersonalised", "Send Personalised")

            '        MyBase.Instance.InnerXml = "<cGroups/><cDefaultEmail>" & cDefaultEmail & "</cDefaultEmail><cDefaultEmailName>" & cDefaultEmailName & "</cDefaultEmailName><cSubject>" & cPageName & "</cSubject>"

            '        If MyBase.isSubmitted Then
            '            MyBase.updateInstanceFromRequest()
            '            MyBase.validate()
            '            Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmail")
            '            If Not Tools.Text.IsEmail(oEmailElmt.InnerText.Trim()) Then
            '                MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
            '                MyBase.valid = False
            '            End If
            '            If MyBase.valid Then
            '                Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
            '                'get the individual elements
            '                Dim oMessaging As New Protean.Messaging
            '                'First we need to get the groups we are sending to
            '                Dim oGroupElmt As XmlElement = MyBase.Instance.SelectSingleNode("cGroups")
            '                Dim oFromEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmail")
            '                Dim oFromNameElmt As XmlElement = MyBase.Instance.SelectSingleNode("cDefaultEmailName")
            '                Dim oSubjectElmt As XmlElement = MyBase.Instance.SelectSingleNode("cSubject")
            '                'get the email addresses for these groups

            '                Dim bResult As Boolean = oMessaging.SendMailToList_Queued(nPageId, moMailConfig("MailingXsl"), oGroupElmt.InnerText, oFromEmailElmt.InnerText, oFromNameElmt.InnerText, oSubjectElmt.InnerText)


            '                ' Log the result
            '                If bResult Then
            '                    'moDbHelper.logActivity(dbHelper.ActivityType.Email, myWeb.mnUserId, nPageId, , oGroupElmt.InnerText)
            '                    moDbHelper.CommitLogToDB(dbHelper.ActivityType.NewsLetterSent, myWeb.mnUserId, myWeb.moSession.SessionID, Now, myWeb.mnPageId, 0, "", True)
            '                    Dim cGroupStr As String = "<Groups><Group>" & Replace(oGroupElmt.InnerText, ",", "</Group><Group>") & "</Group></Groups>"
            '                    'add mssage and return to form so they can sen another
            '                    Dim oMsgElmt As XmlElement = oPageDetail.OwnerDocument.CreateElement("Content")
            '                    oMsgElmt.SetAttribute("type", "Message")
            '                    oMsgElmt.InnerText = "Messages Sent"
            '                    oPageDetail.AppendChild(oMsgElmt)
            '                End If
            '            End If
            '        End If

            '        MyBase.addValues()
            '        Return MyBase.moXformElmt

            '    Catch ex As Exception
            '        returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
            '        Return Nothing
            '    End Try
            'End Function

            Public Function xFrmAdminOptOut() As XmlElement
                Dim oFrmElmt As XmlElement


                Dim cProcessInfo As String = ""
                Try
                    MyBase.NewFrm("OptOut")

                    MyBase.submission("OptOut", "", "post", "return form_check(this)")




                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Existing", "", "Add Opt-Out Address")
                    Dim oElmt As XmlElement
                    oElmt = MyBase.addInput(oFrmElmt, "cEmail", True, "Add Address", "long")
                    oElmt.AppendChild(oElmt.OwnerDocument.CreateElement("value"))
                    MyBase.addBind("cEmail", "cEmail")
                    MyBase.addSubmit(oFrmElmt, "AddOptOut", "Add to List")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Existing", "", "Existing Opt-Out Addresses")

                    Dim oSelElmt As XmlElement = MyBase.addSelect(oFrmElmt, "OptIn", True, "Addresses", "block scroll", ApperanceTypes.Full)
                    Dim cSQL As String = "SELECT EmailAddress FROM tblOptOutAddresses ORDER BY EmailAddress"
                    Dim oDre As SqlDataReader = moDbHelper.getDataReader(cSQL)
                    Do While oDre.Read
                        MyBase.addOption(oSelElmt, oDre(0), oDre(0))
                    Loop
                    MyBase.addBind("OptIn", "OptIn")
                    MyBase.addSubmit(oFrmElmt, "RemoveOptOut", "Remove from List")

                    MyBase.Instance.InnerXml = "<cEmail/><OptIn/>"

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        Dim oEmailElmt As XmlElement = MyBase.Instance.SelectSingleNode("cEmail")

                        If MyBase.valid Then
                            If Not oEmailElmt.InnerText = "" Then
                                If Not Tools.Text.IsEmail(oEmailElmt.InnerText) Then
                                    MyBase.addNote(oElmt, xForm.noteTypes.Alert, "Incorrect Email Address Supplied")
                                Else
                                    If moDbHelper.AddInvalidEmail(oEmailElmt.InnerText) Then
                                        MyBase.addNote(oElmt, xForm.noteTypes.Hint, oEmailElmt.InnerText & " Added")
                                        MyBase.addOption(oSelElmt, oEmailElmt.InnerText, oEmailElmt.InnerText)
                                        oEmailElmt.InnerText = ""
                                    Else
                                        MyBase.addNote(oElmt, xForm.noteTypes.Hint, oEmailElmt.InnerText & "Already Exists")
                                        oEmailElmt.InnerText = ""
                                    End If
                                End If
                            End If
                            Dim oRemoveElmt As XmlElement = MyBase.Instance.SelectSingleNode("OptIn")
                            If Not oRemoveElmt.InnerText = "" Then
                                moDbHelper.RemoveInvalidEmail(oRemoveElmt.InnerText)
                                MyBase.addNote(oSelElmt, xForm.noteTypes.Hint, oRemoveElmt.InnerText & " Removed")
                                oRemoveElmt.InnerText = ""
                            End If

                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmSchedulerItem(ByVal cActionType As String, ByVal nSiteId As Integer, ByVal sSchedCon As String, Optional ByVal nID As Integer = 0) As XmlElement
                Dim cProcessInfo As String = ""
                Try
                    Dim dbh As New dbHelper(myWeb)
                    dbh.ResetConnection(sSchedCon)

                    MyBase.NewFrm("EditScheduleItem")

                    MyBase.load("/xforms/ScheduledItems/" & cActionType & ".xml", myWeb.maCommonFolders)

                    If nID > 0 Then
                        MyBase.Instance.InnerXml = dbh.getObjectInstance(dbHelper.objectTypes.ScheduledItem, nID)
                    End If

                    'get menu
                    Dim oPageSelect As XmlElement = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::select1[@bind='nPageId']")
                    If Not oPageSelect Is Nothing Then
                        MenuSelect(oPageSelect)
                    End If
                    'get files
                    Dim oXSLSelect As XmlElement = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::select1[@bind='cXSLPath']")
                    If Not oXSLSelect Is Nothing Then
                        FileList("/xsl/feeds/", oXSLSelect, ".xsl")
                    End If
                    'set siteid
                    Dim oSiteIDElmt As XmlElement = MyBase.Instance.SelectSingleNode("descendant-or-self::nWebsite")
                    oSiteIDElmt.InnerText = nSiteId

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        'check the min interval

                        Dim oSchedulerConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/scheduler")

                        Dim oMainFrequency As XmlElement = MyBase.Instance.SelectSingleNode("tblActions/nFrequency")
                        If Not oMainFrequency Is Nothing Then
                            If Not CInt(oMainFrequency.InnerText) >= CInt(oSchedulerConfig("MinimumInterval")) Then
                                MyBase.valid = False
                            End If
                        End If

                        If MyBase.valid Then
                            'now we need to save it 
                            dbh.setObjectInstance(Cms.dbHelper.objectTypes.ScheduledItem, MyBase.Instance)
                        End If
                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmSchedulerItem", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function
#Region "Temp subs for Scheduled Items"
            Sub MenuSelect(ByRef oSelect As XmlElement)
                Try
                    Dim oWeb As New Cms
                    oWeb.Open()
                    Dim oMenuElmt As XmlElement = myWeb.GetStructureXML(myWeb.mnUserId, 0, 0, "Site", False, False, False, True, False, "MenuItem", "Menu")
                    Dim oMenuItem As XmlElement
                    For Each oMenuItem In oMenuElmt.SelectNodes("MenuItem")
                        MenuReiterate(oMenuItem, oSelect, 0)
                    Next
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "MenuSelect", ex, "", "", gbDebug)
                End Try
            End Sub
            Sub MenuReiterate(ByVal oMenuItem As XmlElement, ByRef oSelect As XmlElement, ByVal nDepth As Integer)
                Try
                    Dim cNameString As String = ""
                    Dim i As Integer
                    For i = 0 To nDepth
                        cNameString &= "-"
                    Next
                    MyBase.addOption(oSelect, cNameString & oMenuItem.GetAttribute("name"), oMenuItem.GetAttribute("id"))
                    Dim oSubelmt As XmlElement
                    For Each oSubelmt In oMenuItem.SelectNodes("MenuItem")
                        MenuReiterate(oSubelmt, oSelect, nDepth + 1)
                    Next
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "MenuReiterate", ex, "", "", gbDebug)
                End Try
            End Sub

            Sub FileList(ByVal cInitialFolder As String, ByRef oSelect As XmlElement, ByVal cFileExt As String)
                Try

                    Dim oNameStr As String = ""

                    Dim cBasePath As String = goServer.MapPath("/" & cInitialFolder)
                    Dim cCommonPath As String = goServer.MapPath("/ewcommon" & cInitialFolder)
                    Dim dir As New DirectoryInfo(cBasePath)

                    If Not dir.Exists Then
                        dir = New DirectoryInfo(cCommonPath)
                    End If

                    Dim files As FileInfo() = dir.GetFiles()
                    Dim fi As FileInfo
                    For Each fi In files
                        If fi.Extension = cFileExt Then
                            If Not oNameStr.Contains(fi.Name & ",") Then
                                MyBase.addOption(oSelect, Replace(fi.Name, cFileExt, ""), fi.FullName)
                                oNameStr &= fi.Name & ","
                            End If
                        End If
                    Next

                    dir = New DirectoryInfo(cCommonPath)
                    files = dir.GetFiles()
                    For Each fi In files
                        If fi.Extension = cFileExt Then
                            If Not oNameStr.Contains(fi.Name & ",") Then
                                MyBase.addOption(oSelect, Replace(fi.Name, cFileExt, ""), fi.FullName)
                                oNameStr &= fi.Name & ","
                            End If
                        End If
                    Next
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "cInitialFolder", ex, "", "", gbDebug)
                End Try
            End Sub
#End Region

            Public Function xFrmFeedItem(Optional ByVal nContentId As Integer = 0, Optional ByVal oInstanceElmt As XmlElement = Nothing, Optional ByVal nPageId As Integer = 0, Optional ByVal cURL As String = "") As XmlElement
                Dim cProcessInfo As String = ""
                Dim existingIsDifferent As Boolean = True
                Try


                    MyBase.NewFrm("EditFeedItem")



                    MyBase.load("/xforms/content/feeditem.xml", myWeb.maCommonFolders)

                    Dim existingInstance As XmlElement = MyBase.moXformElmt.OwnerDocument.CreateElement("instance")

                    If nContentId > 0 Then
                        existingInstance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Content, nContentId)
                        MyBase.Instance.InnerXml = existingInstance.InnerXml
                    End If
                    If Not oInstanceElmt Is Nothing Then
                        MyBase.Instance.InnerXml = oInstanceElmt.InnerXml
                    End If
                    If Not cURL = "" Then
                        Dim oURLElmt As XmlElement = MyBase.Instance.SelectSingleNode("descendant-or-self::cContentXmlBrief/Content/url")
                        If Not oURLElmt Is Nothing Then
                            oURLElmt.InnerText = cURL
                        End If
                    End If
                    'going to override som stuff here since we will be supplying the instance
                    If MyBase.isSubmitted Or Not oInstanceElmt Is Nothing Then
                        If oInstanceElmt Is Nothing Then MyBase.updateInstanceFromRequest()
                        If oInstanceElmt Is Nothing Then MyBase.validate()

                        If nContentId > 0 AndAlso oInstanceElmt IsNot Nothing _
                                    AndAlso Not String.IsNullOrEmpty(existingInstance.InnerXml) _
                                    AndAlso oInstanceElmt.SelectSingleNode("//cContentXmlBrief") IsNot Nothing _
                                    AndAlso existingInstance.SelectSingleNode("//cContentXmlBrief") IsNot Nothing _
                                    AndAlso oInstanceElmt.SelectSingleNode("//cContentXmlDetail") IsNot Nothing _
                                    AndAlso existingInstance.SelectSingleNode("//cContentXmlDetail") IsNot Nothing _
                                    AndAlso oInstanceElmt.SelectSingleNode("//cContentXmlBrief").InnerXml = existingInstance.SelectSingleNode("//cContentXmlBrief").InnerXml _
                                    AndAlso oInstanceElmt.SelectSingleNode("//cContentXmlDetail").InnerXml = existingInstance.SelectSingleNode("//cContentXmlDetail").InnerXml Then
                            ' Do nothing - don't update it.
                            existingIsDifferent = False
                        End If

                        If MyBase.valid Or (oInstanceElmt IsNot Nothing And existingIsDifferent) Then
                            'now we need to save it 
                            Dim id As Integer
                            If nContentId > 0 Then
                                id = nContentId
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, MyBase.Instance, id)
                                MyBase.moXformElmt.SetAttribute("itemupdated", "true")
                            Else
                                id = moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Content, MyBase.Instance)
                            End If

                            If Not id = 0 And Not nPageId = 0 Then
                                moDbHelper.setContentLocation(nPageId, id, True)
                            End If
                        End If



                    End If

                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmFeedItem", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            'Moved to edit content instead

            Public Function xFrmEditUserSubscription(ByVal nSubId As Integer) As XmlElement
                Dim cProcessInfo As String = ""
                Try

                    If LCase(moRequest("reset")) = "true" Then
                        myWeb.moSession("tempInstance") = Nothing
                    End If

                    MyBase.NewFrm("EditUserSubscription")
                    MyBase.bProcessRepeats = False
                    MyBase.load("/xforms/Subscription/EditSubscription.xml", myWeb.maCommonFolders)

                    If nSubId > 0 Then
                        MyBase.bProcessRepeats = True
                        If myWeb.moSession("tempInstance") Is Nothing Then
                            Dim existingInstance As XmlElement = MyBase.moXformElmt.OwnerDocument.CreateElement("instance")
                            existingInstance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Subscription, nSubId).Replace("xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "").Replace("xmlns:xsd=""http://www.w3.org/2001/XMLSchema""", "")
                            MyBase.LoadInstance(existingInstance)
                            myWeb.moSession("tempInstance") = MyBase.Instance
                        Else
                            MyBase.LoadInstance(myWeb.moSession("tempInstance"))
                        End If
                    End If

                        moXformElmt.SelectSingleNode("descendant-or-self::instance").InnerXml = MyBase.Instance.InnerXml
                    Dim i As Integer = 1
                    Dim bDone As Boolean = False
                    Dim cItems As String = ""
                    Dim initialSubContentId As Long = CLng("0" & MyBase.Instance.SelectSingleNode("tblSubscription/nSubContentId").InnerText)



                    If MyBase.isSubmitted Or MyBase.isTriggered Then
                        MyBase.updateInstanceFromRequest()

                        Dim ContentId As Long = CLng(MyBase.Instance.SelectSingleNode("tblSubscription/nSubContentId").InnerText)
                        Dim ContentXml As XmlElement = myWeb.moPageXml.CreateElement("Content")
                        ContentXml.InnerXml = moDbHelper.getContentBrief(ContentId)

                        If initialSubContentId <> ContentId Then
                            'Now we populate the instance
                            MyBase.Instance.SelectSingleNode("tblSubscription/cSubName").InnerText = ContentXml.SelectSingleNode("Content/Name").InnerText
                            MyBase.Instance.SelectSingleNode("tblSubscription/cSubXml").InnerXml = ContentXml.InnerXml
                            'dStartDate Populated by form
                            MyBase.Instance.SelectSingleNode("tblSubscription/nPeriod").InnerText = ContentXml.SelectSingleNode("Content/Duration/Length").InnerText
                            MyBase.Instance.SelectSingleNode("tblSubscription/cPeriodUnit").InnerText = ContentXml.SelectSingleNode("Content/Duration/Unit").InnerText
                            MyBase.Instance.SelectSingleNode("tblSubscription/nMinimumTerm").InnerText = ContentXml.SelectSingleNode("Content/Duration/MinimumTerm").InnerText
                            MyBase.Instance.SelectSingleNode("tblSubscription/nRenewalTerm").InnerText = ContentXml.SelectSingleNode("Content/Duration/RenewalTerm").InnerText
                            MyBase.Instance.SelectSingleNode("tblSubscription/nValueNet").InnerText = ContentXml.SelectSingleNode("Content/SubscriptionPrices/Price[@type='sale']").InnerText
                            MyBase.Instance.SelectSingleNode("tblSubscription/cRenewalStatus").InnerText = ContentXml.SelectSingleNode("Content/Type").InnerText
                            MyBase.Instance.SelectSingleNode("tblSubscription/dPublishDate").InnerText = MyBase.Instance.SelectSingleNode("tblSubscription/dStartDate").InnerText
                        End If

                        If nSubId = 0 Then
                            'we are creating a new subscription
                            'first we get the subscription content XML
                            MyBase.Instance.SelectSingleNode("tblSubscription/nDirId").InnerText = myWeb.moRequest("userId")
                            MyBase.Instance.SelectSingleNode("tblSubscription/nDirType").InnerText = "user"
                            MyBase.Instance.SelectSingleNode("tblSubscription/dPublishDate").InnerText = MyBase.Instance.SelectSingleNode("tblSubscription/dStartDate").InnerText

                            'Calculate Renewal Date
                            Dim oSub As New Protean.Cms.Cart.Subscriptions()
                            Dim dSubEndDate As DateTime = oSub.SubscriptionEndDate(CDate(MyBase.Instance.SelectSingleNode("tblSubscription/dStartDate").InnerText), ContentXml.SelectSingleNode("Content"))
                            MyBase.Instance.SelectSingleNode("tblSubscription/dExpireDate").InnerText = xmlDate(dSubEndDate)

                        Else
                            'updating an existing subscription
                            If MyBase.Instance.SelectSingleNode("tblSubscription/cRenewalStatus").InnerText <> "Cancelled" Then
                                MyBase.Instance.SelectSingleNode("tblSubscription/nStatus").InnerText = "1"
                            End If
                        End If
                        If MyBase.isSubmitted Then
                            MyBase.validate()
                        End If

                        If MyBase.valid Then
                            Dim nCId As Integer = moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Subscription, MyBase.Instance, nSubId)
                            If MyBase.Instance.SelectSingleNode("tblSubscription/cRenewalStatus").InnerText <> "Cancelled" Then
                                Dim oElmt As XmlElement
                                If nSubId > 0 Then
                                    For Each oElmt In MyBase.Instance.SelectNodes("tblSubscription/cSubXml/Content/UserGroups/Group[@id!='']")
                                        Dim nGrpID As Integer = oElmt.Attributes("id").Value
                                        myWeb.moDbHelper.saveDirectoryRelations(CInt(MyBase.Instance.SelectSingleNode("tblSubscription/nDirId").InnerText), nGrpID)
                                    Next
                                End If
                            End If
                            myWeb.moSession("tempInstance") = Nothing

                        ElseIf MyBase.isTriggered Then
                            'we have clicked a trigger so we must update the instance
                            MyBase.updateInstanceFromRequest()
                            'lets save the instance
                            goSession("tempInstance") = MyBase.Instance
                        Else
                            goSession("tempInstance") = MyBase.Instance
                            End If
                        End If

                        MyBase.addValues()

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditUserSubscription", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmRenewSubscription(ByVal nSubscriptionId As String) As XmlElement
                Dim cProcessInfo As String = ""
                Try

                    Dim oSub As New Cart.Subscriptions(myWeb)

                    MyBase.NewFrm("RenewSubscription")
                    MyBase.submission("RenewSubscription", "", "post")
                    Dim oFrmElmt As XmlElement

                    oSub.GetSubscriptionDetail(MyBase.Instance, nSubscriptionId)
                    Dim SubXml = MyBase.Instance.FirstChild
                    'calculate new expiry date

                    Dim renewInterval As DateInterval = DateInterval.Day
                    Select Case SubXml.GetAttribute("periodUnit")
                        Case "Week"
                            renewInterval = DateInterval.WeekOfYear
                        Case "Year"
                            renewInterval = DateInterval.Year
                    End Select
                    Dim SubId As Long = SubXml.GetAttribute("id")

                    Dim dNewStart As Date = DateAdd(DateInterval.Day, 1, CDate(SubXml.GetAttribute("expireDate")))
                    Dim dNewEnd As Date = DateAdd(renewInterval, CInt(SubXml.GetAttribute("period")), CDate(SubXml.GetAttribute("expireDate")))
                    Dim RenewalCost As Double = CDbl(SubXml.GetAttribute("value"))
                    SubXml.setAttribute("newStart", xmlDate(dNewStart))
                    SubXml.setAttribute("newExpire", xmlDate(dNewEnd))

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "RenewSubscription")

                    MyBase.addInput(oFrmElmt, "nUserID", False, "UserId", "hidden")
                    MyBase.addInput(oFrmElmt, "nSubscriptionId", False, "SubscriptionId", "hidden")
                    Dim oSelElmt As XmlElement = MyBase.addSelect(oFrmElmt, "emailClient", True, "", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Email Renewal Invoice", "yes")


                    MyBase.addNote(oFrmElmt, noteTypes.Hint, "Renew Subscription", True, "renew-sub")

                    MyBase.addSubmit(oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left")
                    MyBase.addSubmit(oFrmElmt, "Confirm", "Confirm Renewal", "Confirm", "btn-success principle", "fa-repeat")

                    If Me.isSubmitted Then
                        If MyBase.getSubmitted = "Back" Then
                            Return MyBase.moXformElmt
                            myWeb.msRedirectOnEnd = "/?ewCmd=RenewSubscription"
                        ElseIf MyBase.getSubmitted = "Confirm" Then
                            Dim bEmailClient As Boolean = False
                            If myWeb.moRequest("emailClient") = "yes" Then bEmailClient = True
                            oSub.RenewSubscription(nSubscriptionId, bEmailClient)
                            MyBase.valid = True
                            Return MyBase.moXformElmt
                        End If
                    End If
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmSchedulerItem", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmConfirmCancelSubscription(ByVal nUserId As String, ByVal nSubscriptionId As String, ByVal nCurrentUser As Integer, ByVal bAdminMode As Boolean) As XmlElement

                Try
                    If Not IsNumeric(nUserId) Then nUserId = 0
                    If Not IsNumeric(nSubscriptionId) Then nSubscriptionId = 0
                    MyBase.NewFrm("CancelSubscription")
                    MyBase.submission("CancelSubscription", "", "post")
                    Dim oFrmElmt As XmlElement

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "CancelSubscription")

                    MyBase.addInput(oFrmElmt, "nUserID", False, "UserId", "hidden")
                    MyBase.addInput(oFrmElmt, "nSubscriptionId", False, "SubscriptionId", "hidden")

                    MyBase.addInput(oFrmElmt, "cStatedReason", False, "Reason for cancelation")

                    MyBase.addNote(oFrmElmt, noteTypes.Hint, "Are you sure you wish to cancel this subscription", True)

                    MyBase.addSubmit(oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left")
                    MyBase.addSubmit(oFrmElmt, "Cancel", "Cancel Subscription", "Cancel", "btn-warning principle", "fa-stop")

                    If Me.isSubmitted Then
                        If MyBase.getSubmitted = "Back" Then
                            Return MyBase.moXformElmt
                        ElseIf MyBase.getSubmitted = "Cancel" Then
                            Dim oSub As New Cart.Subscriptions(myWeb)
                            oSub.CancelSubscription(nSubscriptionId, myWeb.moRequest("cStatedReason"))
                            MyBase.valid = True
                            Return MyBase.moXformElmt
                        End If
                    End If
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmConfirmCancelSubscription", ex, "", , gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmConfirmExpireSubscription(ByVal nUserId As String, ByVal nSubscriptionId As String, ByVal nCurrentUser As Integer, ByVal bAdminMode As Boolean) As XmlElement

                Try
                    If Not IsNumeric(nUserId) Then nUserId = 0
                    If Not IsNumeric(nSubscriptionId) Then nSubscriptionId = 0
                    MyBase.NewFrm("CancelSubscription")
                    MyBase.submission("CancelSubscription", "", "post")
                    Dim oFrmElmt As XmlElement

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "ExpireSubscription")

                    MyBase.addInput(oFrmElmt, "nUserID", False, "UserId", "hidden")
                    MyBase.addInput(oFrmElmt, "nSubscriptionId", False, "SubscriptionId", "hidden")

                    MyBase.addInput(oFrmElmt, "cStatedReason", False, "Reason for expiry")

                    MyBase.addNote(oFrmElmt, noteTypes.Hint, "Are you sure you wish this subscription to expire", True)

                    MyBase.addSubmit(oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left")
                    MyBase.addSubmit(oFrmElmt, "Expire", "Expire Subscription", "Expire", "btn-warning principle", "fa-stop")

                    If Me.isSubmitted Then
                        If MyBase.getSubmitted = "Back" Then
                            Return MyBase.moXformElmt
                        ElseIf MyBase.getSubmitted = "Expire" Then
                            Dim oSub As New Cart.Subscriptions(myWeb)
                            oSub.ExpireSubscription(nSubscriptionId, myWeb.moRequest("cStatedReason"))
                            MyBase.valid = True
                            Return MyBase.moXformElmt
                        End If
                    End If
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmConfirmCancelSubscription", ex, "", , gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmCartSettings() As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oSelElmt As XmlElement
                Dim cProcessInfo As String = ""
                Dim oFsh As fsHelper

                Try
                    oFsh = New fsHelper
                    oFsh.open(moPageXML)

                    MyBase.NewFrm("WebSettings")

                    MyBase.submission("WebSettings", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "CartSettings", "", "Cart Settings")

                    MyBase.addNote(oFrmElmt, noteTypes.Alert, "Any Changes you make to this form risk making this site completely non-functional. Please be sure you know what you are doing before making any changes, or call your web developer for support.")

                    MyBase.addInput(oFrmElmt, "ewSiteURL", True, "Site URL")
                    MyBase.addBind("ewSiteURL", "cart/add[@key='SiteURL']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewSecureURL", True, "Secure URL")
                    MyBase.addBind("ewSecureURL", "cart/add[@key='SecureURL']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewTaxRate", True, "Tax Rate")
                    MyBase.addBind("ewTaxRate", "cart/add[@key='TaxRate']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewMerchantName", True, "Merchant Name")
                    MyBase.addBind("ewMerchantName", "cart/add[@key='MerchantName']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewMerchantEmail", True, "Merchant Email")
                    MyBase.addBind("ewMerchantEmail", "cart/add[@key='MerchantEmail']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewOrderEmailSubject", True, "Order Email Subject")
                    MyBase.addBind("ewOrderEmailSubject", "cart/add[@key='OrderEmailSubject']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewOrderNoPrefix", True, "Order No. Prefix")
                    MyBase.addBind("ewOrderNoPrefix", "cart/add[@key='OrderNoPrefix']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewCurrencySymbol", True, "Currency Symbol")
                    MyBase.addBind("ewCurrencySymbol", "cart/add[@key='CurrencySymbol']/@value", "false()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewStockControl", True, "Stock Control", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewStockControl", "cart/add[@key='StockControl']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewDeposit", True, "Deposit", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "1")
                    MyBase.addOption(oSelElmt, "Off", "0")
                    MyBase.addBind("ewDeposit", "cart/add[@key='Deposit']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewDepositAmount", True, "DepositAmount")
                    MyBase.addBind("ewDepositAmount", "cart/add[@key='DepositAmount']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewNotesXForm", True, "Notes Xform")
                    MyBase.addBind("ewNotesXForm", "cart/add[@key='NotesXForm']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewBillingAddressXForm", True, "Billing Address Xform")
                    MyBase.addBind("ewBillingAddressXForm", "cart/add[@key='BillingAddressXForm']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewDeliveryAddressXForm", True, "DeliveryAddress Xform")
                    MyBase.addBind("ewDeliveryAddressXForm", "cart/add[@key='DeliveryAddressXForm']/@value", "false()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewNoDeliveryAddress", True, "Disable Delivery Address", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Yes", "on")
                    MyBase.addOption(oSelElmt, "No", "off")
                    MyBase.addBind("ewNoDeliveryAddress", "cart/add[@key='NoDeliveryAddress']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewEmailReceipts", True, "Email Receipts", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Yes", "on")
                    MyBase.addOption(oSelElmt, "No", "off")
                    MyBase.addBind("ewEmailReceipts", "cart/add[@key='EmailReceipts']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewMerchantEmailTemplatePath", True, "Merchant Email Template Path")
                    MyBase.addBind("ewMerchantEmailTemplatePath", "cart/add[@key='MerchantEmailTemplatePath']/@value", "false()")

                    MyBase.addInput(oFrmElmt, "ewCustomerEmailTemplatePath", True, "Customer Email Template Path")
                    MyBase.addBind("ewCustomerEmailTemplatePath", "cart/add[@key='CustomerEmailTemplatePath']/@value", "false()")


                    MyBase.addInput(oFrmElmt, "ewPriorityCountries", True, "Priority Countries")
                    MyBase.addBind("ewPriorityCountries", "cart/add[@key='PriorityCountries']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewSavePayments", True, "SavePayments", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewSavePayments", "cart/add[@key='SavePayments']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewVatAtUnit", True, "Vat At Unit", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "Yes", "yes")
                    MyBase.addOption(oSelElmt, "No", "no")
                    MyBase.addBind("ewVatAtUnit", "cart/add[@key='VatAtUnit']/@value", "true()")

                    oSelElmt = MyBase.addSelect1(oFrmElmt, "ewDiscounts", True, "Discounts", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt, "On", "on")
                    MyBase.addOption(oSelElmt, "Off", "off")
                    MyBase.addBind("ewDiscounts", "cart/add[@key='Discounts']/@value", "true()")

                    MyBase.addInput(oFrmElmt, "ewPriceModOrder", True, "Price Mod Order")
                    MyBase.addBind("ewPriceModOrder", "cart/add[@key='PriceModOrder']/@value", "false()")

                    MyBase.addSubmit(oFrmElmt, "", "Save Settings")

                    Dim oCfg As Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/")
                    Dim oCgfSect As System.Configuration.DefaultSection = oCfg.GetSection("protean/cart")

                    Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                    If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), , goConfig("AdminGroup")) Then

                        MyBase.Instance.InnerXml = oCgfSect.SectionInformation.GetRawXml

                        'code here to replace any missing nodes
                        'all of the required config settings
                        Dim aSettingValues() As String = Split("SiteURL,SecureURL,TaxRate,MerchantName,MerchantEmail,OrderEmailSubject,OrderNoPrefix,CurrencySymbol,StockControl,Deposit,DepositAmount,NotesXForm,BillingAddressXForm,DeliveryAddressXForm,NoDeliveryAddress,MerchantEmailTemplatePath,PriorityCountries,SavePayments,VatAtUnit,Discounts,PriceModOrder", ",")

                        Dim i As Long
                        Dim oElmt As XmlElement
                        Dim oElmtAft As XmlElement = Nothing

                        For i = 0 To UBound(aSettingValues)
                            oElmt = MyBase.Instance.SelectSingleNode("cart/add[@key='" & aSettingValues(i) & "']")
                            If oElmt Is Nothing Then
                                oElmt = moPageXML.CreateElement("add")
                                oElmt.SetAttribute("key", aSettingValues(i))
                                oElmt.SetAttribute("value", "")
                                If oElmtAft Is Nothing Then
                                    MyBase.Instance.FirstChild.InsertBefore(oElmt, MyBase.Instance.FirstChild.FirstChild)
                                Else
                                    MyBase.Instance.FirstChild.InsertAfter(oElmt, oElmtAft)
                                End If
                            End If
                            oElmtAft = oElmt
                        Next

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            If MyBase.valid Then
                                oCgfSect.SectionInformation.RestartOnExternalChanges = False
                                oCgfSect.SectionInformation.SetRawXml(MyBase.Instance.InnerXml)
                                oCfg.Save()
                            End If
                        End If

                        oImp.UndoImpersonation()
                    End If
                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmWebSettings", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmFindContentToLocate(ByVal nNewLocationPage As String, ByVal nFromPage As String, ByVal bIncludeChildren As String, ByVal cContentType As String, ByVal cSearchTerm As String, ByRef oDetailElement As XmlElement) As XmlElement
                If nFromPage = "" Then nFromPage = 0
                If bIncludeChildren Is Nothing Then bIncludeChildren = "0"
                Dim oFrmElmt As XmlElement
                Dim oSelElmt1 As XmlElement
                Dim oSelElmt2 As XmlElement
                Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")
                Dim bCascade As Boolean = False
                Dim cProcessInfo As String = ""

                Try

                    MyBase.NewFrm("FindContentToRelate")
                    'nNewLocationPage 
                    Dim oElement As XmlElement = MyBase.Instance.OwnerDocument.CreateElement("nNewLocationPage")
                    oElement.InnerText = nNewLocationPage
                    MyBase.Instance.AppendChild(oElement)
                    'nFromPage
                    oElement = MyBase.Instance.OwnerDocument.CreateElement("nFromPage")
                    oElement.InnerText = nFromPage
                    MyBase.Instance.AppendChild(oElement)
                    'bIncludeChildren
                    oElement = MyBase.Instance.OwnerDocument.CreateElement("bIncludeChildren")
                    oElement.InnerText = bIncludeChildren
                    MyBase.Instance.AppendChild(oElement)
                    'cContentType 
                    oElement = MyBase.Instance.OwnerDocument.CreateElement("cContentType")
                    oElement.InnerText = cContentType
                    MyBase.Instance.AppendChild(oElement)
                    'cSearchTerm
                    oElement = MyBase.Instance.OwnerDocument.CreateElement("cSearchTerm")
                    oElement.InnerText = cSearchTerm
                    MyBase.Instance.AppendChild(oElement)

                    MyBase.submission("AddLocation", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "SearchContent")
                    'oGrp1Elmt = MyBase.addGroup(oFrmElmt, "Criteria", "", "")

                    'Definitions

                    'Hidden
                    MyBase.addInput(oFrmElmt, "nNewLocationPage", True, "nNewLocationPage", "hidden")
                    MyBase.addBind("nNewLocationPage", "nNewLocationPage")

                    MyBase.addInput(oFrmElmt, "type", True, "cContentType", "hidden")
                    MyBase.addBind("type", "cContentType")
                    'Textbox
                    MyBase.addInput(oFrmElmt, "cSearchTerm", True, "Search Expression")
                    MyBase.addBind("cSearchTerm", "cSearchTerm", "false()")

                    'Select
                    'Pages
                    oSelElmt1 = MyBase.addSelect1(oFrmElmt, "nFromPage", False, "Page", "siteTree", ApperanceTypes.Minimal)
                    MyBase.addBind("nFromPage", "nFromPage", "true()")

                    'Checkbox
                    oSelElmt2 = MyBase.addSelect(oFrmElmt, "bIncludeChildren", True, "&#160;", "", ApperanceTypes.Full)
                    MyBase.addOption(oSelElmt2, "Search Children", 1)
                    MyBase.addBind("bIncludeChildren", "bIncludeChildren", "false()")

                    'search button
                    MyBase.addSubmit(oFrmElmt, "Search", "Search", "Search")
                    MyBase.addValues()
                    oDetailElement.AppendChild(MyBase.moXformElmt)

                    If MyBase.isSubmitted Or moRequest("cSearched") = "1" Then
                        oDetailElement.AppendChild(xFrmLocateContent(nNewLocationPage, nFromPage, bIncludeChildren, cContentType, cSearchTerm))
                    End If

                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmFindRelated", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmLocateContent(ByVal nNewLocationPage As Integer, ByVal nFromPage As Integer, ByVal bIncludeChildren As String, ByVal cContentType As String, ByVal cSearchTerm As String) As XmlElement
                'if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                Dim oFrmElmt As XmlElement
                Dim oGrp1Elmt As XmlElement
                Dim oSelElmt2 As XmlElement
                Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")
                Dim bCascade As Boolean = False
                Dim cProcessInfo As String = ""

                Try


                    MyBase.NewFrm("SelectContentToLocate")

                    MyBase.submission("SelectLocation", "", "post", "")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Content")

                    Dim oGrp0Elmt As XmlElement = MyBase.addGroup(oFrmElmt, "ResultsHeader", , "&#160;")

                    MyBase.addSubmit(oFrmElmt, "Locate", "Add To Page", "Locate")

                    If MyBase.isSubmitted Then

                        Dim oItems() As String = Split(myWeb.moRequest("Results"), ",")
                        Dim cPosition As String = myWeb.moRequest("Position")
                        Dim i As Integer = 0
                        For i = 0 To oItems.Length - 1
                            myWeb.moDbHelper.setContentLocation(nNewLocationPage, oItems(i), False, False, False)
                        Next
                        MyBase.valid = True
                    Else


                        Dim oResults As XmlNodeList = moDbHelper.LocateContentSearch(nFromPage, cContentType, bIncludeChildren, cSearchTerm, nNewLocationPage)
                        Dim nCount As Integer = 0
                        If Not oResults Is Nothing Then nCount = oResults.Count

                        oGrp0Elmt.SelectSingleNode("label").InnerText = "Results (" & nCount & ")"

                        oGrp1Elmt = MyBase.addGroup(oGrp0Elmt, "Results", "horizontal ", "")
                        'nNewLocation
                        Dim oElement As XmlElement = MyBase.Instance.OwnerDocument.CreateElement("nNewLocationPage")
                        oElement.InnerText = nNewLocationPage
                        MyBase.Instance.AppendChild(oElement)

                        'nFromPage
                        oElement = MyBase.Instance.OwnerDocument.CreateElement("nFromPage")
                        oElement.InnerText = nFromPage
                        MyBase.Instance.AppendChild(oElement)
                        MyBase.addInput(oGrp1Elmt, "nNewLocationPage", True, "nNewLocationPage", "hidden").SetAttribute("value", nNewLocationPage)
                        MyBase.addBind("nNewLocationPage", "nNewLocationPage")

                        'bIncludeChildren
                        oElement = MyBase.Instance.OwnerDocument.CreateElement("bIncludeChildren")
                        oElement.InnerText = bIncludeChildren
                        MyBase.Instance.AppendChild(oElement)
                        MyBase.addInput(oGrp1Elmt, "type", True, "type", "hidden").SetAttribute("value", cContentType)
                        MyBase.addBind("type", "type")

                        'cContentType 
                        oElement = MyBase.Instance.OwnerDocument.CreateElement("cContentType")
                        oElement.InnerText = cContentType
                        MyBase.Instance.AppendChild(oElement)
                        MyBase.addInput(oGrp1Elmt, "nFromPage", True, "nFromPage", "hidden").SetAttribute("value", nFromPage)
                        MyBase.addBind("nFromPage", "nFromPage")
                        MyBase.addInput(oGrp1Elmt, "bIncludeChildren", True, "bIncludeChildren", "hidden").SetAttribute("value", bIncludeChildren)
                        MyBase.addBind("bIncludeChildren", "bIncludeChildren")

                        'cSearchTerm
                        oElement = MyBase.Instance.OwnerDocument.CreateElement("cSearchTerm")
                        oElement.InnerText = cSearchTerm
                        MyBase.Instance.AppendChild(oElement)
                        MyBase.addInput(oGrp1Elmt, "cSearchTerm", True, "cSearchTerm", "hidden").SetAttribute("value", cSearchTerm)
                        MyBase.addBind("cSearchTerm", "cSearchTerm")

                        'cSearched
                        oElement = MyBase.Instance.OwnerDocument.CreateElement("cSearched")
                        oElement.InnerText = "1"
                        MyBase.Instance.AppendChild(oElement)
                        MyBase.addInput(oGrp1Elmt, "cSearched", True, "cSearched", "hidden").SetAttribute("value", "1")
                        MyBase.addBind("cSearched", "cSearched")

                        MyBase.addSubmit(oGrp1Elmt, "Locate", "Add To Page", "Locate")
                        Dim oResult As XmlElement
                        oSelElmt2 = MyBase.addSelect(oGrp1Elmt, "Results", True, "", "radiocheckbox multiline selectAll content", Protean.xForm.ApperanceTypes.Full)
                        If Not oResults Is Nothing Then
                            For Each oResult In oResults
                                If oSelElmt2.SelectSingleNode("item[value/node()='" & oResult.GetAttribute("id") & "']") Is Nothing Then
                                    MyBase.addOption(oSelElmt2, oResult.OuterXml, oResult.GetAttribute("id"), True)
                                End If
                            Next
                        End If

                        MyBase.addValues()
                    End If
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmLocateContent", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmCartOrderDownloads() As XmlElement
                Try
                    'if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                    Dim oFrmElmt As XmlElement

                    Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")
                    Dim bCascade As Boolean = False
                    Dim cProcessInfo As String = ""

                    MyBase.NewFrm("CartActivity")

                    MyBase.submission("SeeReport", "/ewcommon/tools/export.ashx?ewCmd=CartDownload", "get", "")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Content", "")

                    Dim oContElmt As XmlElement = MyBase.addGroup(oFrmElmt, "1", "xFormContainer")

                    Dim oGrp0Elmt As XmlElement = MyBase.addGroup(oContElmt, "Criteria", "xFormContainer", "Criteria")

                    MyBase.Instance.InnerXml = "<Criteria ewCmd=""CartDownload"" output=""csv""><dBegin>" & Protean.Tools.Xml.XmlDate(Now.AddMonths(-1), False) &
                    "</dBegin><dEnd>" & Protean.Tools.Xml.XmlDate(Now.AddDays(1), False) & "</dEnd>" &
                    "<cCurrencySymbol/><cOrderType>Order</cOrderType><cOrderStage>6</cOrderStage>" &
                    "</Criteria>"

                    MyBase.addBind("dBegin", "Criteria/dBegin", "true()")
                    MyBase.addBind("dEnd", "Criteria/dEnd", "true()")
                    MyBase.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", , "string")
                    MyBase.addBind("cOrderType", "Criteria/cOrderType", "true()", "string")
                    MyBase.addBind("cOrderStage", "Criteria/cOrderStage", "true()", "string")
                    MyBase.addBind("format", "Criteria/@output", "false()", "string")
                    MyBase.addBind("ewCmd", "Criteria/@ewCmd", "false()", "string")

                    MyBase.addInput(oGrp0Elmt, "ewCmd", True, "ewCmd", "hidden")

                    MyBase.addInput(oGrp0Elmt, "dBegin", True, "From", "calendarTime")
                    MyBase.addInput(oGrp0Elmt, "dEnd", True, "To", "calendarTime")
                    Dim oSel1 As XmlElement = MyBase.addSelect1(oGrp0Elmt, "cCurrencySymbol", True, "Currency")
                    If myWeb.moConfig("Quote") <> "on" Then
                        oSel1 = MyBase.addSelect1(oGrp0Elmt, "cOrderType", True, "Cart Type")
                        MyBase.addOption(oSel1, "Order", "Order")
                        MyBase.addOption(oSel1, "Quote", "Quote")
                    End If
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "cOrderStage", True, "Cart Type")
                    Dim nProcess As Protean.Cms.Cart.cartProcess
                    Dim i As Integer = 0
                    For Each nProcess In [Enum].GetValues(GetType(Protean.Cms.Cart.cartProcess))
                        MyBase.addOption(oSel1, nProcess.ToString, i)
                        i = i + 1
                    Next

                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "format", True, "Output")
                    MyBase.addOption(oSel1, "Excel", "excel")
                    MyBase.addOption(oSel1, "CSV", "csv")
                    MyBase.addOption(oSel1, "XML", "xml")
                    If myWeb.moConfig("Debug") = "on" Then
                        MyBase.addOption(oSel1, "Raw XML", "rawxml")
                    End If
                    MyBase.addSubmit(oGrp0Elmt, "Results", "Download Spreadsheet", "Results")

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                    End If

                    MyBase.addValues()

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmCartActivity", ex, "", "", gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmCartActivity() As XmlElement
                Try
                    'if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                    Dim oFrmElmt As XmlElement

                    Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")
                    Dim bCascade As Boolean = False
                    Dim cProcessInfo As String = ""

                    MyBase.NewFrm("CartActivity")

                    MyBase.submission("SeeReport", "", "post", "")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Content", "")

                    Dim oContElmt As XmlElement = MyBase.addGroup(oFrmElmt, "1", "xFormContainer")

                    Dim oGrp0Elmt As XmlElement = MyBase.addGroup(oContElmt, "Criteria", "xFormContainer", "Criteria")

                    MyBase.Instance.InnerXml = "<Criteria><dBegin>" & Protean.Tools.Xml.XmlDate(Now.AddMonths(-1), False) &
                    "</dBegin><dEnd>" & Protean.Tools.Xml.XmlDate(Now.AddDays(1), False) & "</dEnd><bSplit>0</bSplit>" &
                    "<cProductType/><nProductId>0</nProductId><cCurrencySymbol/>" &
                    "<nOrderStatus1>6</nOrderStatus1><nOrderStatus2>9</nOrderStatus2><cOrderType>Order</cOrderType>" &
                    "</Criteria>"

                    MyBase.addInput(oGrp0Elmt, "dBegin", True, "From", "calendar")
                    MyBase.addInput(oGrp0Elmt, "dEnd", True, "To", "calendar")
                    Dim oSel1 As XmlElement = MyBase.addSelect1(oGrp0Elmt, "cCurrencySymbol", True, "Currency")
                    MyBase.addOption(oSel1, "All/None", "")
                    MyBase.addOption(oSel1, "GBP", "£")
                    MyBase.addInput(oGrp0Elmt, "nOrderStatus1", True, "nOrderStatus1", "hidden")
                    MyBase.addInput(oGrp0Elmt, "nOrderStatus2", True, "nOrderStatus2", "hidden")

                    If myWeb.moConfig("Quote") <> "on" Then
                        oSel1 = MyBase.addSelect1(oGrp0Elmt, "cOrderType", True, "Cart Type")
                        MyBase.addOption(oSel1, "Order", "Order")
                        MyBase.addOption(oSel1, "Quote", "Quote")
                    End If

                    ' Lets get the content types if we have more than 1

                    Dim oDR As SqlDataReader
                    Dim cSQL As String = "SELECT tblContent.cContentSchemaName" &
                    " FROM tblCartItem LEFT OUTER JOIN" &
                    " tblContent ON tblCartItem.nItemId = tblContent.nContentKey" &
                    " GROUP BY tblContent.cContentSchemaName" &
                    " ORDER BY tblContent.cContentSchemaName"
                    oDR = myWeb.moDbHelper.getDataReader(cSQL)

                    If oDR.VisibleFieldCount > 1 Then

                        oSel1 = MyBase.addSelect1(oGrp0Elmt, "bSplit", True, "Group By Product Type")
                        MyBase.addOption(oSel1, "Yes", "1")
                        MyBase.addOption(oSel1, "No", "0")

                        oSel1 = MyBase.addSelect1(oGrp0Elmt, "cProductType", True, "Select Product Type")
                        MyBase.addOption(oSel1, "All", "")
                        MyBase.addOptionsFromSqlDataReader(oSel1, oDR, "cContentSchemaName", "cContentSchemaName")

                    End If

                    'Gets full list of products

                    cSQL = " SELECT tblContent.cContentName, tblContent.nContentKey" &
                    " FROM tblCartItem LEFT OUTER JOIN" &
                    " tblContent ON tblCartItem.nItemId = tblContent.nContentKey" &
                    " GROUP BY tblContent.cContentName, tblContent.nContentKey" &
                    " ORDER BY tblContent.cContentName"
                    oDR = myWeb.moDbHelper.getDataReader(cSQL)


                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "nProductId", True, "Single Product")
                    MyBase.addOption(oSel1, "All", "0")
                    MyBase.addOptionsFromSqlDataReader(oSel1, oDR, "cContentName", "nContentKey")

                    MyBase.addSubmit(oGrp0Elmt, "Results", "See Results", "Results")

                    MyBase.addBind("dBegin", "Criteria/dBegin", "true()")
                    MyBase.addBind("dEnd", "Criteria/dEnd", "true()")
                    MyBase.addBind("bSplit", "Criteria/bSplit", , "number")
                    MyBase.addBind("cProductType", "Criteria/cProductType", , "string")
                    MyBase.addBind("nProductId", "Criteria/nProductId", , "number")
                    MyBase.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", , "string")
                    MyBase.addBind("nOrderStatus1", "Criteria/nOrderStatus1", , "number")
                    MyBase.addBind("nOrderStatus2", "Criteria/nOrderStatus2", , "number")
                    MyBase.addBind("cOrderType", "Criteria/cOrderType", "true()", "string")


                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                    End If

                    MyBase.addValues()

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmCartActivity", ex, "", "", gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmCartActivityDrillDown() As XmlElement
                Try
                    'if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                    Dim oFrmElmt As XmlElement

                    Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")
                    Dim bCascade As Boolean = False
                    Dim cProcessInfo As String = ""

                    MyBase.NewFrm("CartActivityDrilldown")

                    MyBase.submission("SeeReport", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Content", "")

                    Dim oGrp0Elmt As XmlElement = MyBase.addGroup(oFrmElmt, "Criteria", , "Criteria")


                    MyBase.Instance.InnerXml = "<Criteria>" &
                    "<nYear>" & Now.Year & "</nYear>" &
                    "<nMonth>" & Now.Month & "</nMonth>" &
                    "<nDay>0</nDay>" &
                    "<cGrouping>Page</cGrouping><cCurrencySymbol/>" &
                    "<nOrderStatus1>6</nOrderStatus1><nOrderStatus2>9</nOrderStatus2>" &
                    "<cOrderType>Order</cOrderType>" &
                    "</Criteria>"
                    'Year
                    Dim oSel1 As XmlElement = MyBase.addSelect1(oGrp0Elmt, "nYear", True, "Year", "required")
                    MyBase.addOption(oSel1, "All", "0")
                    For i As Integer = 2000 To Now.Year
                        MyBase.addOption(oSel1, i, i)
                    Next
                    'Month
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "nMonth", True, "Month", "required")
                    MyBase.addOption(oSel1, "All", "0")
                    For i As Integer = 1 To 12
                        MyBase.addOption(oSel1, MonthName(i), i)
                    Next
                    'Day
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "nDay", True, "Day", "required")
                    MyBase.addOption(oSel1, "All", "0")
                    For i As Integer = 1 To 31
                        MyBase.addOption(oSel1, i, i)
                    Next
                    'Grouping
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "cGrouping", True, "Grouping", "required")
                    MyBase.addOption(oSel1, "By Page", "Page")
                    MyBase.addOption(oSel1, "By Group", "Group")
                    'Currency
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "cCurrencySymbol", True, "Currency")
                    MyBase.addOption(oSel1, "All/None", "")
                    MyBase.addOption(oSel1, "GBP", "£")
                    'OrderStatus
                    MyBase.addInput(oGrp0Elmt, "nOrderStatus1", True, "nOrderStatus1", "hidden")
                    MyBase.addInput(oGrp0Elmt, "nOrderStatus2", True, "nOrderStatus2", "hidden")
                    'CartType
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "cOrderType", True, "Cart Type", "required")
                    MyBase.addOption(oSel1, "Order", "Order")
                    MyBase.addOption(oSel1, "Quote", "Quote")

                    MyBase.addSubmit(oGrp0Elmt, "Results", "See Results", "Results")



                    MyBase.addBind("nYear", "Criteria/nYear")
                    MyBase.addBind("nMonth", "Criteria/nMonth")
                    MyBase.addBind("nDay", "Criteria/nDay")
                    MyBase.addBind("cGrouping", "Criteria/cGrouping", , "string")
                    MyBase.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", , "string")
                    MyBase.addBind("nOrderStatus1", "Criteria/nOrderStatus1", , "number")
                    MyBase.addBind("nOrderStatus2", "Criteria/nOrderStatus2", , "number")
                    MyBase.addBind("cOrderType", "Criteria/cOrderType", "true()", "string")


                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                    End If

                    MyBase.addValues()

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmCartActivityDrillDown", ex, "", "", gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmCartActivityPeriod() As XmlElement
                Try
                    'if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                    Dim oFrmElmt As XmlElement

                    Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")
                    Dim bCascade As Boolean = False
                    Dim cProcessInfo As String = ""

                    MyBase.NewFrm("CartActivityDrilldown")

                    MyBase.submission("SeeReport", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Content", "")

                    Dim oGrp0Elmt As XmlElement = MyBase.addGroup(oFrmElmt, "Criteria", , "Criteria")

                    MyBase.Instance.InnerXml = "<Criteria>" &
                    "<nYear>" & Now.Year & "</nYear>" &
                    "<nMonth>0</nMonth>" &
                    "<nWeek>0</nWeek>" &
                    "<cGroup>Month</cGroup><cCurrencySymbol/>" &
                    "<nOrderStatus1>6</nOrderStatus1><nOrderStatus2>9</nOrderStatus2>" &
                    "<cOrderType>Order</cOrderType>" &
                    "</Criteria>"
                    'Year
                    Dim oSel1 As XmlElement = MyBase.addSelect1(oGrp0Elmt, "nYear", True, "Year", "required")
                    MyBase.addOption(oSel1, "All", "0")
                    For i As Integer = 2000 To Now.Year
                        MyBase.addOption(oSel1, i, i)
                    Next
                    'Month
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "nMonth", True, "Month", "required")
                    MyBase.addOption(oSel1, "All", "0")
                    For i As Integer = 1 To 12
                        MyBase.addOption(oSel1, MonthName(i), i)
                    Next
                    'Day
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "nWeek", True, "Week", "required")
                    MyBase.addOption(oSel1, "All", "0")
                    For i As Integer = 1 To 52
                        MyBase.addOption(oSel1, i, i)
                    Next
                    'Grouping
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "cGroup", True, "Grouping", "required")
                    MyBase.addOption(oSel1, "By Month", "Month")
                    MyBase.addOption(oSel1, "By Week", "Week")
                    MyBase.addOption(oSel1, "By Day", "Day")
                    'Currency
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "cCurrencySymbol", True, "Currency")
                    MyBase.addOption(oSel1, "All/None", "")
                    MyBase.addOption(oSel1, "GBP", "£")
                    'OrderStatus
                    MyBase.addInput(oGrp0Elmt, "nOrderStatus1", True, "nOrderStatus1", "hidden")
                    MyBase.addInput(oGrp0Elmt, "nOrderStatus2", True, "nOrderStatus2", "hidden")
                    'CartType
                    oSel1 = MyBase.addSelect1(oGrp0Elmt, "cOrderType", True, "Cart Type", "required")
                    MyBase.addOption(oSel1, "Order", "Order")
                    MyBase.addOption(oSel1, "Quote", "Quote")

                    MyBase.addSubmit(oGrp0Elmt, "Results", "See Results", "Results")

                    MyBase.addBind("nYear", "Criteria/nYear")
                    MyBase.addBind("nMonth", "Criteria/nMonth")
                    MyBase.addBind("nWeek", "Criteria/nWeek")
                    MyBase.addBind("cGroup", "Criteria/cGroup", , "string")
                    MyBase.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", , "string")
                    MyBase.addBind("nOrderStatus1", "Criteria/nOrderStatus1", , "number")
                    MyBase.addBind("nOrderStatus2", "Criteria/nOrderStatus2", , "number")
                    MyBase.addBind("cOrderType", "Criteria/cOrderType", "true()", "string")


                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                    End If

                    MyBase.addValues()

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmCartActivityPeriod", ex, "", "", gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmMemberVisits() As XmlElement
                Try
                    Dim oFrmElmt As XmlElement
                    'Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")


                    MyBase.NewFrm("MemberVisits")
                    MyBase.Instance.InnerXml = "<Criteria><dFrom>" & Protean.Tools.Xml.XmlDate(Now.AddDays(-31), False) & "</dFrom><dTo>" & Protean.Tools.Xml.XmlDate(Now.AddDays(1), False) & "</dTo><cGroups>0</cGroups></Criteria>"
                    MyBase.submission("SeeReport", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Content", "")
                    Dim oGrp0Elmt As XmlElement = MyBase.addGroup(oFrmElmt, "Criteria", , "Search Visits")
                    MyBase.addInput(oGrp0Elmt, "dFrom", True, "From", "calendar")
                    MyBase.addInput(oGrp0Elmt, "dTo", True, "To", "calendar")
                    Dim oSel As XmlElement = MyBase.addSelect(oGrp0Elmt, "cGroups", True, "Filter by Group", , ApperanceTypes.Minimal)
                    MyBase.addSubmit(oGrp0Elmt, "Results", "See Results", "Results")
                    Dim cSQL As String = "SELECT cDirName, nDirKey FROM tblDirectory "
                    cSQL &= " WHERE (NOT (cDirSchema = 'User')) AND (NOT (cDirSchema = N'Role'))"
                    cSQL &= " ORDER BY cDirName"
                    Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                    MyBase.addOptionsFromSqlDataReader(oSel, oDR, "cDirName", "nDirKey")

                    MyBase.addBind("dFrom", "Criteria/dFrom", "true()")
                    MyBase.addBind("dTo", "Criteria/dTo", "true()")
                    MyBase.addBind("cGroups", "Criteria/cGroups")

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                    End If

                    MyBase.addValues()

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmMemberVisits", ex, "", "", gbDebug)
                    Return Nothing
                End Try
            End Function

            ''' <summary>
            ''' This adds or edits a codeset, or the groups associated with the code set
            ''' </summary>
            ''' <param name="nCodesetKey">The code set ID</param>
            ''' <param name="cFormName">The type of form we're dealing with - Codes or CodeGroups</param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function xFrmMemberCodeset(ByVal nCodesetKey As Integer, Optional ByVal cFormName As String = "Codes") As XmlElement
                Try

                    Dim oElmt As XmlElement = Nothing
                    Dim cCodeGroups As String = ""
                    Dim cSQL As String = ""

                    ' Build the form
                    MyBase.NewFrm("MemberCodes")
                    MyBase.load("/xforms/directory/" & cFormName & ".xml", myWeb.maCommonFolders)

                    ' Load the instance.
                    If nCodesetKey > 0 Then
                        MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Codes, nCodesetKey)
                    End If

                    ' Pre-population
                    ' ==============

                    ' Code Type
                    MyBase.Instance.SelectSingleNode("tblCodes/nCodeType").InnerText = Cms.dbHelper.CodeType.Membership

                    ' Groups
                    If Tools.Xml.NodeState(MyBase.moXformElmt, "//select[@bind='cCodeGroups']") <> Tools.Xml.XmlNodeState.NotInstantiated Then
                        oElmt = MyBase.moXformElmt.SelectSingleNode("//select[@bind='cCodeGroups']")
                        cSQL = "SELECT cDirName + ' [' + cDirSchema + ': ' + CAST(nDirKey As nvarchar) + ']' AS DirName, nDirKey FROM tblDirectory "
                        cSQL &= " WHERE NOT(cDirSchema IN ('Role','User'))"
                        cSQL &= " ORDER BY cDirSchema, cDirName"
                        Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                        MyBase.addOptionsFromSqlDataReader(oElmt, oDR, "DirName", "nDirKey")
                    End If

                    ' Handle Submission
                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()

                        If MyBase.valid Then
                            ' Create a unique ID for new code sets
                            If nCodesetKey = 0 Then
                                MyBase.Instance.SelectSingleNode("tblCodes/cCode").InnerText = System.Guid.NewGuid().ToString()
                            End If

                            ' Save the code set
                            myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Codes, MyBase.Instance, nCodesetKey)

                            'Update any sub-codes
                            If nCodesetKey > 0 Then
                                cSQL = "UPDATE tblAudit SET dPublishDate = " & Protean.Tools.Database.SqlDate(MyBase.Instance.SelectSingleNode("tblCodes/dPublishDate").InnerText, True)
                                cSQL &= ", dExpireDate  = " & Protean.Tools.Database.SqlDate(MyBase.Instance.SelectSingleNode("tblCodes/dExpireDate").InnerText, True)
                                cSQL &= ", nStatus  = " & MyBase.Instance.SelectSingleNode("tblCodes/nStatus").InnerText
                                cSQL &= " FROM tblAudit a INNER JOIN tblCodes c ON a.nauditkey = c.nauditid AND (c.nCodeParentId = " & nCodesetKey & " OR c.nCodeKey = " & nCodesetKey & ")"
                                myWeb.moDbHelper.ExeProcessSql(cSQL)
                            End If

                        End If
                    End If

                    ' Manually populate the groups in the dropdown
                    If Tools.Xml.NodeState(MyBase.Instance, "tblCodes/cCodeGroups", , , , , , cCodeGroups) = Tools.Xml.XmlNodeState.HasContents Then
                        Dim oGroups() As String = Split(cCodeGroups, ",")
                        For i As Integer = 0 To oGroups.Length - 1
                            If Tools.Xml.NodeState(MyBase.moXformElmt, "descendant-or-self::*[@bind='cCodeGroups']/item[value='" & oGroups(i) & "']") <> Tools.Xml.XmlNodeState.NotInstantiated Then
                                oElmt = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::*[@bind='cCodeGroups']/item[value='" & oGroups(i) & "']")
                                oElmt.SetAttribute("selected", "selected")
                            End If
                        Next
                    End If

                    ' Tidy Up
                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmMemberCodeset", ex, "", "", gbDebug)
                    Return Nothing
                End Try
            End Function

            ''' <summary>
            '''   Xform for generating codes for codes sets.
            ''' </summary>
            ''' <param name="nParentCodeKey">The parent code set id</param>
            ''' <param name="cFormName">Optional, probably not needed - the type of form we're dealing with - CodeGenerator</param>
            ''' <returns></returns>
            ''' <remarks></remarks>
            Public Function xFrmMemberCodeGenerator(ByVal nParentCodeKey As Integer, Optional ByVal cFormName As String = "CodeGenerator") As XmlElement

                Dim oElmt As XmlElement = Nothing
                Dim oParentInstance As XmlElement = Nothing
                Dim oInstanceRoot As XmlElement = Nothing
                Dim cCodeGroups As String = ""
                Dim cCodeXForm As String = ""

                Try

                    ' Build the form
                    MyBase.NewFrm("MemberCodes")
                    MyBase.load("/xforms/directory/" & cFormName & ".xml", myWeb.maCommonFolders)

                    MyBase.Instance.SelectSingleNode("tblCodes/nCodeType").InnerText = Cms.dbHelper.CodeType.Membership

                    ' Get the parent code instance
                    oParentInstance = MyBase.moXformElmt.OwnerDocument.CreateElement("instance")
                    oParentInstance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Codes, nParentCodeKey)

                    ' Update the form with relevant values from the parent
                    For Each oElmt In oParentInstance.SelectNodes("tblCodes/dPublishDate|tblCodes/dExpireDate|tblCodes/nStatus")
                        ' Populate the local instance with parent code information
                        If Not (MyBase.Instance.SelectSingleNode("tblCodes/" & oElmt.Name) Is Nothing) Then
                            MyBase.Instance.SelectSingleNode("tblCodes/" & oElmt.Name).InnerText = oElmt.InnerText
                        End If
                    Next

                    ' Add the parent code id
                    MyBase.Instance.SelectSingleNode("tblCodes/nCodeParentId").InnerText = nParentCodeKey

                    ' Update the label
                    If NodeState(MyBase.moXformElmt, "group/label", , , , oElmt) <> XmlNodeState.NotInstantiated Then
                        oElmt.InnerText &= " for " & oParentInstance.SelectSingleNode("tblCodes/cCodeName").InnerText
                    End If


                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()

                        If MyBase.valid Then
                            ' Generate the Codes
                            oInstanceRoot = MyBase.Instance.SelectSingleNode("tblCodes")

                            Dim nNoCodes As Integer = CInt(oInstanceRoot.SelectSingleNode("nNumberOfCodes").InnerText)

                            Dim oCodes(nNoCodes - 1) As String

                            If Xml.getNodeValueByType(oInstanceRoot, "bRND", , "0") = "0" Then

                                ' Generate non-random codes
                                oCodes = Protean.Tools.Text.CodeGen(
                                oInstanceRoot.SelectSingleNode("cPreceedingText").InnerText,
                                oInstanceRoot.SelectSingleNode("nStartNumber").InnerText,
                                nNoCodes,
                                oInstanceRoot.SelectSingleNode("bKeepProceedingZeros").InnerText,
                                oInstanceRoot.SelectSingleNode("bMD5Results").InnerText)
                            Else

                                ' Generate random codes
                                Dim random As New Number.Random()

                                ' Set the options

                                ' Option: Case
                                Dim options As Text.TextOptions = TextOptions.UpperCase

                                ' Option: Unambiguous Letters
                                If Xml.getNodeValueByType(oInstanceRoot, "bRNDVague", , "0") = "1" Then options = options Or TextOptions.UnambiguousCharacters

                                ' Option: Character Sets
                                Dim cCharDefs As String = Xml.getNodeValueByType(oInstanceRoot, "cRNDAlpha", , "Letters,Numbers").ToString()
                                If cCharDefs.Contains("Letters") Then options = options Or TextOptions.UseAlpha
                                If cCharDefs.Contains("Numbers") Then options = options Or TextOptions.UseNumeric
                                If cCharDefs.Contains("Symbols") Then options = options Or TextOptions.UseSymbols

                                ' Generate the codes
                                For i As Integer = 0 To nNoCodes - 1
                                    ' Generate a random password
                                    Dim cC As String = Text.RandomPassword(CInt(Xml.getNodeValueByType(oInstanceRoot, "nRNDLength", , "8")), , options, random)

                                    ' Check for duplicates
                                    Do Until Array.LastIndexOf(oCodes, cC) = -1 Or cC = ""
                                        cC = Text.RandomPassword(CInt(Xml.getNodeValueByType(oInstanceRoot, "nRNDLength", , "8")), , options)
                                    Loop

                                    oCodes(i) = cC
                                Next

                            End If

                            ' Add the codes to the database
                            Dim nAdded As Integer = 0
                            Dim nSkipped As Integer = 0
                            For i As Integer = 0 To oCodes.Length - 1
                                If Not oCodes(i) = "" Then
                                    If myWeb.moDbHelper.GetDataValue("SELECT nCodeKey FROM tblCodes WHERE cCode ='" & oCodes(i) & "'", , , 0) > 0 Then
                                        nSkipped += 1
                                    Else
                                        MyBase.Instance.SelectSingleNode("tblCodes/cCode").InnerText = oCodes(i)
                                        Dim nSubId As Integer = myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Codes, MyBase.Instance)
                                        nAdded += 1
                                    End If
                                End If
                            Next
                            MyBase.addNote(MyBase.moXformElmt.SelectSingleNode("group"), xForm.noteTypes.Help, nAdded & " Codes Added, " & nSkipped & " Codes Skipped (Duplicates)", True)
                        End If

                    End If


                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmMemberCodeGenerator", ex, "", "", gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmVoucherCode(ByVal nCodeId As Integer) As XmlElement

                Dim cProcessInfo As String = ""
                Dim cTypePath As String = ""
                Try


                    MyBase.NewFrm("EditVoucherCode")
                    If Not MyBase.load("/xforms/codes/" & cTypePath, myWeb.maCommonFolders) Then
                        'not allot we can do really except try defaults
                        If Not MyBase.load("/xforms/code/Voucher.xml", myWeb.maCommonFolders) Then
                            'not allot we can do really 
                        End If
                    End If

                    If nCodeId > 0 Then
                        MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Codes, nCodeId)
                    End If
                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.addValues()
                        MyBase.validate()
                        If MyBase.valid Then
                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Codes, MyBase.Instance, IIf(nCodeId > 0, nCodeId, -1))
                        End If
                    End If
                    MyBase.addValues()
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmVoucherCode", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmImportFile(ByVal cPath As String) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim sValidResponse As String = ""
                Dim cProcessInfo As String = ""
                Dim oImportManifestXml As New XmlDocument

                Try
                    Try
                        oImportManifestXml.Load(goServer.MapPath(myWeb.moConfig("ProjectPath") & "/xsl/import") & "/ImportManifest.xml")
                    Catch
                        'do nothing
                    End Try

                    If Not oImportManifestXml Is Nothing Then

                        MyBase.NewFrm("ImportFile")

                        MyBase.submission("Inport File", "", "post", "form_check(this)")

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Import file into ProteanCMS", "", "Please select the file to import")

                        Dim oSelectElmt As XmlElement
                        oSelectElmt = MyBase.addSelect1(oFrmElmt, "importXslt", True, "Import Type")
                        Dim oChoices As XmlElement
                        Dim sDefaultXslt As String = ""
                        For Each oChoices In oImportManifestXml.SelectNodes("/Imports/ImportGroup/Import")
                            'Dim oChoicesElmt As XmlElement = MyBase.addChoices(oSelectElmt, oChoices.GetAttribute("name"))
                            'For Each oItem In oChoices.SelectNodes("Import")
                            MyBase.addOption(oSelectElmt, Replace(oChoices.GetAttribute("name"), "_", " "), oChoices.GetAttribute("xslFile"))
                            If sDefaultXslt = "" Then sDefaultXslt = oChoices.GetAttribute("xslFile")
                            'Next
                        Next
                        MyBase.addNote("importXslt", noteTypes.Hint, "This defines the layout and columns of the import file. Each import file must be in the pre-agreed format. To create additional import filters contact your web developer.")

                        MyBase.addBind("importXslt", "file/@importXslt", "true()")

                        MyBase.addInput(oFrmElmt, "fld", True, "Upload Path", "readonly")
                        MyBase.addBind("fld", "file/@path", "true()")

                        MyBase.addUpload(oFrmElmt, "uploadFile", True, "image/*", "Upload File")
                        MyBase.addBind("uploadFile", "file", "true()")

                        Dim oSelectElmt2 As XmlElement
                        oSelectElmt2 = MyBase.addSelect1(oFrmElmt, "opperationMode", True, "Opperation Mode", , ApperanceTypes.Full)
                        MyBase.addOption(oSelectElmt2, "Test", "test")
                        MyBase.addOption(oSelectElmt2, "Full Import", "import")
                        MyBase.addBind("opperationMode", "file/@opsMode", "true()")

                        '   If myWeb.moConfig("debug") = "on" Then
                        Dim oSelectElmt3 As XmlElement
                        oSelectElmt3 = MyBase.addSelect1(oFrmElmt, "contentType", True, "Response Xml", , ApperanceTypes.Full)
                        MyBase.addOption(oSelectElmt3, "on", "xml")
                        MyBase.addOption(oSelectElmt3, "off", "")
                        '   End If
                        MyBase.addBind("xml", "file/@xml", "false()")

                        MyBase.addSubmit(oFrmElmt, "", "Upload", "ewSubmit")

                        MyBase.Instance.InnerXml = "<file path=""" & cPath & """ filename="""" mediatype="""" opsMode=""test"" importXslt=""" & sDefaultXslt & """ xml=""""/>"

                        If MyBase.isSubmitted Then

                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()

                            'lets do some hacking 
                            Dim fUpld As System.Web.HttpPostedFile
                            fUpld = goRequest.Files("uploadFile")

                            If Not fUpld Is Nothing Then
                                MyBase.valid = True
                            End If

                            If MyBase.valid Then

                                Dim oFs As fsHelper = New fsHelper
                                oFs.initialiseVariables(fsHelper.LibraryType.Documents)
                                oFs.mcStartFolder = goServer.MapPath("/") & cPath

                                sValidResponse = oFs.SaveFile(fUpld, "")

                                Dim oElmt As XmlElement = MyBase.Instance.FirstChild

                                Dim cFilename As String = oFs.mcStartFolder & Right(fUpld.FileName, Len(fUpld.FileName) - fUpld.FileName.LastIndexOf("\"))
                                cFilename = cFilename.Replace(" ", "-")
                                oElmt.SetAttribute("filename", cFilename)

                                If sValidResponse = fUpld.FileName Then
                                    valid = True
                                    MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse & " - File Imported")
                                Else
                                    valid = False
                                    MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
                                End If
                            Else
                                valid = False
                            End If
                        End If

                        MyBase.addValues()
                        Return MyBase.moXformElmt
                    Else
                        MyBase.NewFrm("ImportFile")

                        MyBase.submission("Import File Error", "", "post", "form_check(this)")
                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Import File", "", "Error")
                        MyBase.addNote(oFrmElmt, noteTypes.Alert, "There are no imports configured for this site.")
                        Return MyBase.moXformElmt
                    End If



                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Function xFrmStartIndex() As XmlElement
                Dim oFrmElmt As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    'load the xform to be edited
                    moDbHelper.moPageXml = moPageXML
                    Dim idx As Protean.Indexer = New Protean.Indexer(myWeb)
                    MyBase.NewFrm("StartIndex")

                    MyBase.submission("DeleteFile", "", "post")
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "folderItem", "", "Start Index")
                    MyBase.Instance.InnerXml = idx.GetIndexInfo

                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, "Starting off the indexing process can take up to an hour for larger sites")

                    MyBase.addSubmit(oFrmElmt, "", "Start Index", , "principle pleaseWait")



                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                        If MyBase.valid Then
                            Dim bResult As Boolean = True
                            idx.DoIndex(0, bResult)

                            Dim cSubResponse As String = idx.cExError
                            If cSubResponse = "" Then
                                bResult = True
                                cSubResponse = "Completed Successfully"
                            Else
                                bResult = False
                            End If
                            cSubResponse += vbCrLf & "Pages: " & idx.nPagesIndexed
                            cSubResponse += vbCrLf & "Documents: " & idx.nDocumentsIndexed
                            cSubResponse += vbCrLf & "Contents: " & idx.nContentsIndexed

                            MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, cSubResponse)

                            'fire this off in its own thread.
                            'Dim t As Thread
                            't = New Thread(AddressOf idx.DoIndex)
                            't.Start()

                        Else
                            MyBase.addValues()
                        End If
                    Else
                        MyBase.addValues()
                    End If

                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmStartIndex", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmGetReport(ByVal cReportName As String) As XmlElement

                Dim cProcessInfo As String = ""

                Try

                    'Replace Spaces with hypens
                    cReportName = Replace(cReportName, " ", "-")

                    If Not MyBase.load("/xforms/Reports/" & cReportName & ".xml", myWeb.maCommonFolders) Then
                        'show xform load error message
                    End If

                    Dim queryNode As XmlElement = moXformElmt.SelectSingleNode("descendant-or-self::Query")

                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.validate()
                    ElseIf LCase(queryNode.GetAttribute("autoSubmit")) = "true" Then
                        MyBase.validate()
                    End If


                    MyBase.addValues()
                    Return MyBase.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmGetReport", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmLookup(ByVal nLookupId As Integer, Optional ByVal Category As String = "", Optional ByVal ParentId As Long = 0) As XmlElement
                Dim oFrmElmt As XmlElement
                Dim oGrp1Elmt As XmlElement
                Dim cProcessInfo As String = ""

                Try

                    Dim parentOptions As String = "" & myWeb.moConfig("LookupParentOptions")

                    Dim oDict As New Dictionary(Of String, String)
                    Dim s As String

                    If parentOptions <> "" Then
                        For Each s In Split(parentOptions, ";")
                            Dim arr As String() = Split(s, ":")
                            oDict.Add(arr(0), arr(1))
                        Next
                    End If

                    MyBase.NewFrm("EditProductGroup")
                    MyBase.Instance.InnerXml = "<tblLookup><nLkpID/><cLkpKey/><cLkpValue/><cLkpCategory>" & Category & "</cLkpCategory><nLkpParent>" & ParentId & "</nLkpParent><nAuditId/></tblLookup>"
                    If nLookupId > 0 Then
                        'MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Lookup, nLookupId)
                        Category = MyBase.Instance.SelectSingleNode("tblLookup/cLkpCategory").InnerText
                    End If
                    MyBase.submission("EditLookup", "", "post", "form_check(this)")

                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "Lookup")
                    MyBase.addNote("pgheader", noteTypes.Help, IIf(nLookupId > 0, "Edit ", "Add ") & "Lookup")
                    oGrp1Elmt = MyBase.addGroup(oFrmElmt, "Lookup", "1col", "Details")

                    'Definitions
                    MyBase.addInput(oGrp1Elmt, "nLkpID", True, "nLkpID", "hidden")
                    MyBase.addBind("nLkpID", "tblLookup/nLkpID")

                    If parentOptions <> "" Then
                        If oDict.ContainsKey(Category) Then
                            Dim SelectElmt As XmlElement = MyBase.addSelect1(oGrp1Elmt, "nLkpParent", True, oDict(Category), ApperanceTypes.Minimal)
                            MyBase.addBind("nLkpParent", "tblLookup/nLkpParent")
                            Dim sSql As String = "select nLkpId as value, cLkpKey as name from tblLookup where cLkpCategory like '" & oDict(Category) & "'"
                            Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(sSql)
                            MyBase.addOptionsFromSqlDataReader(SelectElmt, oDr)
                        End If
                    End If

                    MyBase.addInput(oGrp1Elmt, "cLkpKey", True, "Name")
                    MyBase.addBind("cLkpKey", "tblLookup/cLkpKey", "true()")

                    MyBase.addInput(oGrp1Elmt, "cLkpValue", True, "Value")
                    MyBase.addBind("cLkpValue", "tblLookup/cLkpValue", "true()")

                    MyBase.addInput(oGrp1Elmt, "cLkpCategory", True, "Category", "readonly")
                    MyBase.addBind("cLkpCategory", "tblLookup/cLkpCategory", "true()")

                    MyBase.addInput(oGrp1Elmt, "nAuditId", True, "nAuditId", "hidden")
                    MyBase.addBind("nAuditId", "tblLookup/nAuditId")

                    'search button

                    MyBase.addSubmit(oFrmElmt, "EditLookup", "Save Lookup", "SaveLookup")


                    If MyBase.isSubmitted Then
                        MyBase.updateInstanceFromRequest()
                        MyBase.addValues()
                        MyBase.validate()
                        If MyBase.valid Then
                            moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Lookup, MyBase.Instance, IIf(nLookupId > 0, nLookupId, -1))
                        End If
                    End If
                    MyBase.addValues()
                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmLookup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Function xFrmEditTemplate() As XmlElement
                Dim cProcessInfo As String = ""
                Dim xslFilename As String = ""
                Dim oFrmElmt As XmlElement
                Try
                    MyBase.NewFrm("EditTemplate")
                    Select Case myWeb.moRequest("ewCmd2")
                        Case "RenewalAlerts"
                            xslFilename = "/xsl/email/subscriptionReminder.xsl"
                    End Select

                    MyBase.Instance.InnerXml = "<Template name=""""><TemplateContent/></Template>"
                    oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "SelectTemplate")

                    Dim XslDocument As New XmlDocument
                    XslDocument.Load(goServer.MapPath(xslFilename))
                    MyBase.submission("EditTemplate", "", "post", "form_check(this)")

                    Dim xmlnsManager = New System.Xml.XmlNamespaceManager(XslDocument.NameTable)
                    xmlnsManager.AddNamespace("xsl", "http://www.w3.org/1999/XSL/Transform")

                    Dim SelectElmt As XmlElement = MyBase.addSelect1(oFrmElmt, "Template", True, "Template", ApperanceTypes.Minimal)
                    Dim oTmpt As XmlElement
                    Dim i As Int16 = 1
                    For Each oTmpt In XslDocument.DocumentElement.SelectNodes("xsl:template", xmlnsManager)
                        addOption(SelectElmt, oTmpt.GetAttribute("mode") & " - " & oTmpt.GetAttribute("match"), i)

                        If CInt(myWeb.moRequest("Template")) = i Then

                            addInput(oFrmElmt, "tplt-mode", True, "Mode")
                            MyBase.addBind("tplt-mode", "Template/TemplateContent/*/@mode", "true()")
                            addInput(oFrmElmt, "tplt-match", True, "Match")
                            MyBase.addBind("tplt-match", "Template/TemplateContent/*/@match", "true()")

                            MyBase.addTextArea(oFrmElmt, "TemplateContent", True, "Template Content", "xsl")
                            MyBase.addBind("TemplateContent", "Template/TemplateContent", "true()")
                            Dim oElmt As XmlElement = MyBase.Instance.SelectSingleNode("Template/TemplateContent")
                            oElmt.InnerXml = oTmpt.OuterXml

                        End If
                        i = i + 1
                    Next

                    Dim xmlElmt As XmlElement = MyBase.addSelect1(oFrmElmt, "contentType", True, "contentType", ApperanceTypes.Minimal)
                    addOption(xmlElmt, "xml", "xml")

                    If CInt(myWeb.moRequest("Template")) > 0 Then
                        MyBase.addSubmit(oFrmElmt, "EditTemplate", "Save Template", "SaveTemplate")
                    Else
                        MyBase.addSubmit(oFrmElmt, "EditTemplate", "Edit Template", "EditTemplate")
                    End If

                    MyBase.addValues()

                    Return MyBase.moXformElmt
                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmLookup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            ''' <summary>
            ''' 
            ''' </summary>
            ''' <remarks></remarks>
            Private Class xFormContentLocations

#Region " Declarations"
                Shadows mcModuleName As String = "xFormContentLocations"


                ' Declarations
                Private _form As AdminXforms
                Private _locations As XmlElement
                Private _locationCount As Integer = 1
                Private _contentId As Long
                Private _structureXml As XmlElement = Nothing
                Private _currentLocations As Hashtable
                Private _locationsScope As Hashtable
                Private _selects As XmlNodeList = Nothing

                ' Constants
                Private Const _selectsXPath As String = "//node()[contains(name(),'select') and contains(@class,'contentLocations')]"
                Private Const _locationInstanceNodeName As String = "locatomatic"
#End Region
#Region " Initialisation"
                Public Sub New(ByVal ContentId As Long, ByRef Form As xForm)
                    PerfMon.Log(mcModuleName, "New")
                    Try
                        ' Set variables
                        _contentId = ContentId
                        _form = Form
                        _selects = _form.RootGroup.SelectNodes(_selectsXPath)


                    Catch ex As Exception
                        returnException(Form.myWeb.msException, mcModuleName, "New", ex, "", "", gbDebug)
                    End Try
                End Sub
#End Region
#Region " Public Methods"
                Public Sub Refresh()
                    PerfMon.Log(mcModuleName, "Refresh")
                    Try
                        _selects = _form.RootGroup.SelectNodes(_selectsXPath)
                    Catch ex As Exception
                        returnException(_form.myWeb.msException, mcModuleName, "Refresh", ex, "", "", gbDebug)
                    End Try
                End Sub
                Public Function IsActive() As Boolean
                    PerfMon.Log(mcModuleName, "IsActive")
                    Try
                        Return (_selects.Count > 0)
                    Catch ex As Exception
                        returnException(_form.myWeb.msException, mcModuleName, "IsActive", ex, "", "", gbDebug)
                        Return False
                    End Try
                End Function
                Public Sub ProcessSelects()
                    PerfMon.Log(mcModuleName, "ProcessSelects")

                    Dim menuId As Long
                    Dim bind As XmlElement
                    Dim locations As XmlElement
                    Dim menuItems As XmlNodeList
                    Dim selectItem As xFormContentLocationsSelect
                    Dim value As String
                    Dim location As XmlElement
                    Dim locationid As String
                    Dim cXPath As String
                    Dim cXPathModifier As String
                    Dim menuName As String = ""


                    Try
                        If Me.IsActive() Then

                            ' Get the site structure
                            _structureXml = _form.myWeb.GetStructureXML()

                            ' Get the current locations
                            _currentLocations = New Hashtable
                            _locationsScope = New Hashtable
                            If _contentId > 0 Then
                                _currentLocations = _form.moDbHelper.getHashTable("SELECT nStructId,1 As Location FROM tblContentLocation WHERE nContentId=" & _contentId, "nStructId", "Location")
                            End If

                            ' Create a bind element
                            bind = Xml.addNewTextNode("bind", _form.model)
                            bind.SetAttribute("nodeset", _locationInstanceNodeName)

                            locations = Xml.addNewTextNode(_locationInstanceNodeName, _form.moXformElmt.SelectSingleNode("//instance"))


                            ' Iterate through each select
                            For Each _selectItem As XmlElement In _selects

                                ' Get the rootid - look for class root-id
                                selectItem = New xFormContentLocationsSelect(_selectItem)

                                ' Get the menuItems - construct an xpath
                                ' The rootmode is the path modifier
                                cXPath = "//MenuItem[@id=" & selectItem.Root.ToString & " and ((not(@cloneparent) or @cloneparent=0) and (not(@clone) or @clone=0))]"

                                If _selectItem.GetAttribute("locationsXpath") <> "" Then
                                    cXPath = cXPath + _selectItem.GetAttribute("locationsXpath")
                                Else

                                    cXPathModifier = ""

                                    Select Case selectItem.RootMode
                                        Case xFormContentLocationsSelect.RootModes.Exclude
                                            cXPath = cXPath + "/descendant::MenuItem"
                                        Case xFormContentLocationsSelect.RootModes.Include
                                            cXPath = cXPath + "/descendant-or-self::MenuItem"
                                        Case xFormContentLocationsSelect.RootModes.ChildrenOnly
                                            cXPath = cXPath + "/MenuItem"
                                    End Select

                                End If

                                menuItems = _structureXml.SelectNodes(cXPath)

                                ' Add the instance node - assume that there could be more then one select here.
                                location = Xml.addNewTextNode("location", locations)
                                locationid = "loc_idx_" & _locationCount.ToString
                                _locationCount = _locationCount + 1
                                location.SetAttribute("id", locationid)


                                ' location the bind
                                _form.addBind(selectItem.Id(), "location[@id='" & locationid & "']", , , bind)


                                Dim proceedingParent As XmlElement
                                Dim oChoices As XmlElement
                                ' Process the menu items
                                ' For each menuitem, check if it's already in scope.
                                ' If not add the option to the select.
                                For Each menuItem As XmlElement In menuItems
                                    menuId = CLng(menuItem.GetAttribute("id"))

                                    ' Check if we've added it already
                                    If Not (_locationsScope.ContainsKey(menuId)) Then
                                        If _currentLocations.ContainsKey(CStr(menuId)) Then
                                            value = "true"
                                            If Not (String.IsNullOrEmpty(location.InnerText)) Then location.InnerText &= ","
                                            location.InnerText &= menuId.ToString
                                        Else
                                            value = "false"
                                        End If

                                        ' Add to in-scope location hashtable
                                        _locationsScope.Add(menuId, value)

                                        ' Determine the name - NodeState effectively sets menuName as
                                        ' the DisplayName node if it's populated, if not is sets it to be the
                                        ' name attribute.

                                        If NodeState(menuItem, "DisplayName", , , , , , menuName, True) <> XmlNodeState.HasContents Then
                                            menuName = menuItem.GetAttribute("name")
                                        Else
                                            menuName = menuItem.SelectSingleNode("DisplayName").InnerText
                                        End If


                                        Dim oParentNode As XmlElement = menuItem.ParentNode
                                        Dim oParentParentNode As XmlElement = oParentNode.ParentNode

                                        ' _form.addOption(_selectItem, menuName, menuId)

                                        'if we are only 2 levels from the root then we use choices
                                        If Not oParentParentNode Is Nothing Then
                                            If oParentParentNode.GetAttribute("id") = selectItem.Root.ToString And LCase(_selectItem.GetAttribute("showAllLevels")) <> "true" Then
                                                If proceedingParent Is Nothing Then
                                                    oChoices = _form.addChoices(_selectItem, oParentNode.GetAttribute("name"))
                                                ElseIf proceedingParent.GetAttribute("id") <> oParentNode.GetAttribute("id") Then
                                                    oChoices = _form.addChoices(_selectItem, oParentNode.GetAttribute("name"))

                                                End If
                                                ' Add the checkbox
                                                _form.addOption(oChoices, menuName, menuId)
                                            Else
                                                If oParentNode.GetAttribute("id") <> _form.myWeb.moConfig("RootPageId") Then
                                                    Do While oParentNode.GetAttribute("id") <> selectItem.Root.ToString
                                                        menuName = oParentNode.GetAttribute("name") & " / " & menuName
                                                        oParentNode = oParentNode.ParentNode
                                                    Loop
                                                End If
                                            End If
                                            ' Add the checkbox
                                            _form.addOption(_selectItem, menuName, menuId)
                                        Else

                                            If menuItem.GetAttribute("id") <> _form.myWeb.moConfig("RootPageId") Then
                                                Do While menuItem.GetAttribute("id") <> selectItem.Root.ToString
                                                    menuName = menuItem.GetAttribute("name") & " / " & menuName
                                                    oParentNode = menuItem.ParentNode
                                                Loop
                                            End If

                                            ' Add the checkbox
                                            _form.addOption(_selectItem, menuName, menuId)
                                        End If
                                        proceedingParent = oParentNode
                                    End If
                                Next
                            Next
                        End If

                    Catch ex As Exception
                        returnException(_form.myWeb.msException, mcModuleName, "ProcessSelects", ex, "", "", gbDebug)

                    End Try
                End Sub


                Public Sub ProcessRequest(ByVal ContentId As Long)
                    PerfMon.Log(mcModuleName, "ProcessRequest")

                    Dim InclusionList As String = ""
                    Dim ScopeList As String = ""

                    Try
                        If Me.IsActive() Then

                            ' Go through the location binds and deal with them.
                            ' _locationInstanceNodeName

                            For Each location As XmlElement In _form.moXformElmt.SelectNodes("//instance/" & _locationInstanceNodeName & "/location")

                                ' The inner text will be a comma separated list, we need to add this to the inclusion list.
                                If Not (String.IsNullOrEmpty(location.InnerText)) Then
                                    If Not (String.IsNullOrEmpty(InclusionList)) Then
                                        InclusionList &= ","
                                    End If
                                    InclusionList &= location.InnerText
                                End If
                            Next

                            ' Convert the Scope to a CSV
                            ScopeList = Dictionary.hashtableToCSV(Me._locationsScope, Dictionary.Dimension.Key)

                            ' manage the locations
                            _form.moDbHelper.updateLocationsWithScope(ContentId, InclusionList, ScopeList)

                        End If

                    Catch ex As Exception
                        returnException(_form.myWeb.msException, mcModuleName, "ProcessRequest", ex, "", "", gbDebug)

                    End Try
                End Sub


#End Region
#Region " Private Class: xFormContentLocationsSelect"
                Private Class xFormContentLocationsSelect

#Region " Declarations"
                    Shadows mcModuleName As String = "xFormContentLocationsSelect"

                    Private _selectItem As XmlElement
                    Private _rootId As Long
                    Private _rootMode As RootModes

                    Enum RootModes
                        Include
                        Exclude
                        RootOnly
                        ChildrenOnly
                    End Enum
#End Region
#Region " Initialisation"
                    Public Sub New(ByRef selectItem As XmlElement)
                        PerfMon.Log(mcModuleName, "New")
                        Try
                            _rootMode = RootModes.Exclude
                            Item = selectItem

                        Catch ex As Exception
                            '  returnException(Form.myWeb.msException, mcModuleName, "New", ex, "", "", gbDebug)
                        End Try
                    End Sub

#End Region
#Region " Private Properties"
                    Private ReadOnly Property ClassName() As String
                        Get
                            Return _selectItem.GetAttribute("class")
                        End Get

                    End Property

#End Region
#Region " Public Properties"
                    Public Property Item() As XmlElement
                        Get
                            Return _selectItem
                        End Get
                        Set(ByVal value As XmlElement)

                            ' Set the Item
                            _selectItem = value

                            ' Determine its Root Id
                            Dim rootId As String = getPropertyFromClass("root")
                            _rootId = IIf(Not (String.IsNullOrEmpty(rootId)) And IsNumeric(rootId), CLng(rootId), 0)

                            ' Determine the root mode
                            Dim rootModeParameter As String = "" & getPropertyFromClass("rootMode")
                            Select Case rootModeParameter.ToLower
                                Case "exclude"
                                    _rootMode = RootModes.Exclude
                                Case "include"
                                    _rootMode = RootModes.Include
                                Case "rootonly"
                                    _rootMode = RootModes.RootOnly
                                Case "childrenonly"
                                    _rootMode = RootModes.ChildrenOnly
                            End Select

                        End Set
                    End Property
                    Public ReadOnly Property Root() As Long
                        Get
                            Return _rootId
                        End Get
                    End Property
                    Public ReadOnly Property RootMode() As RootModes
                        Get
                            Return _rootMode
                        End Get
                    End Property
                    Public ReadOnly Property Id() As String
                        Get
                            If Not (String.IsNullOrEmpty(_selectItem.GetAttribute("bind"))) Then
                                Return _selectItem.GetAttribute("bind")
                            ElseIf Not (String.IsNullOrEmpty(_selectItem.GetAttribute("ref"))) Then
                                Return _selectItem.GetAttribute("ref")
                            Else
                                Return ""
                            End If
                        End Get
                    End Property
#End Region
#Region " Private Methods"
                    Private Function getPropertyFromClass(ByRef propertyName As String) As String

                        PerfMon.Log(mcModuleName, "getPropertyFromClass")
                        Try

                            Dim pattern As String = "^.*\s" & propertyName & "-([\S]*)\s.*$"
                            Return "" & SimpleRegexFind(" " & ClassName() & " ", pattern, 1)

                        Catch ex As Exception
                            '   returnException(myWeb.msException, mcModuleName, "getPropertyFromClass", ex, "", "", gbDebug)
                            Return ""
                        End Try

                    End Function
#End Region

                End Class

#End Region


            End Class


        End Class






        ''' <summary>
        ''' <para>XFormEditor facilitates the editting of xforms within an xform.</para>
        ''' <para>
        ''' It creates a MasterXForm, which is the Content node under ContentDetail of the content being worked on.
        ''' It maintains a MasterInstance which is the object instance of the content as a whole (from GetObjectInstance)
        ''' </para>
        ''' <para>
        ''' MasterInstance and MasterXform are different, with MasterXform being a subcomponent of MasterInstance
        ''' </para>
        ''' <para>
        ''' It is instantiaated with a content id.  It is then invoked with methods specific to each xform control (e.g. group, input etc).
        ''' </para>
        ''' </summary> 
        ''' <remarks>
        ''' <para>
        ''' This is overridden by EonicLMS, which uses it for editting questionnaires.
        ''' This has been written in a way that should work without EonicLMS (i.e. for generic form editting),
        ''' but needs the following areas addressed for this generic implementation
        ''' </para>
        ''' <list>
        ''' <item>The xform xml files for each control do not exist.  Currently only found in wellardsCommon</item>
        ''' <item>xFrmEditXFormInput - New control items will not have binds created. </item>
        ''' <item>I also think instantiating the object without a content id will not work.  I'm not sure if it actually should be possible.</item>
        ''' </list>
        ''' </remarks>
        Public Class XFormEditor
            Inherits Protean.xForm

            Private Const mcModuleName As String = "Protean.Cms.Admin.XFormEditor"
            Public Const FORMPATH As String = "/xforms/content"

            Private _masterInstance As XmlElement
            Protected _masterXform As Protean.xForm
            Private _request As System.Web.HttpRequest
            Protected _schema As String = "generic"
            Public myWeb As Protean.Cms

            Public Sub New(ByRef aWeb As Protean.Cms, Optional ByVal contentId As Long = 0)
                MyBase.New(aWeb.msException)

                ' Set the Web context variables
                myWeb = aWeb
                _request = myWeb.moRequest
                Me.moPageXML = myWeb.moPageXml

                ' Create the form
                CreateMasterForm(contentId)

            End Sub

            Public ReadOnly Property MasterInstance() As XmlElement
                Get
                    Return _masterInstance
                End Get
            End Property


            Public ReadOnly Property Ready() As Boolean
                Get
                    Try
                        Return _masterXform IsNot Nothing
                    Catch ex As Exception
                        Return False
                    End Try
                End Get
            End Property

            Public ReadOnly Property ContentSchema() As String
                Get
                    Return _schema
                End Get
            End Property

            Protected Overridable ReadOnly Property schemaPreferenceList() As String()
                Get
                    Dim schemaList(2) As String
                    schemaList(0) = _schema
                    schemaList(1) = "generic"
                    Return schemaList
                End Get
            End Property

            Protected Sub CreateMasterForm(Optional ByVal contentId As Long = 0)
                Try
                    _masterXform = New Protean.xForm(myWeb.msException)
                    _masterXform.moPageXML = Me.moPageXML
                    _masterInstance = Me.moPageXML.CreateElement("instance")

                    ' If content id has been set, then get the instance
                    If contentId > 0 Then
                        _masterInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Content, contentId)
                        _masterXform.load(_masterInstance.SelectSingleNode("descendant-or-self::cContentXmlDetail/Content"))
                        _schema = _masterInstance.SelectSingleNode("descendant-or-self::cContentSchemaName").InnerText
                    End If

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "CreateMasterForm", ex, "", "", gbDebug)
                End Try
            End Sub

            Protected Function loadControlForm(ByVal controlType As String) As Boolean

                Dim success As Boolean = False

                Try

                    For Each schemaType As String In schemaPreferenceList

                        If Me.load(FORMPATH & "/" & schemaType & "." & controlType & ".xml", myWeb.maCommonFolders) Then
                            success = True
                            Exit For
                        End If

                    Next

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "loadForm", ex, "", "", gbDebug)
                    Return False
                End Try

            End Function


            ''' <summary>
            ''' This function returns an xForm to update the form being edited
            ''' </summary>
            ''' <param name="cRef">The Ref or Bind of the form element to be edited</param>
            ''' <param name="cParRef">The Ref or Bind of the form element under which the new element will be inserted</param>
            ''' <returns></returns>
            ''' <remarks></remarks>

            Public Overridable Function xFrmEditXFormGroup(ByVal cRef As String, Optional ByVal cParRef As String = "") As XmlElement

                Dim oElmt As XmlElement
                Dim cProcessInfo As String = "cRef = " & cRef & ", ParRef=" & cParRef
                Dim cMode As String
                Dim newRef As String

                Try
                    ' Set the update mode
                    cMode = IIf(String.IsNullOrEmpty(cRef), "Add", "Edit")

                    ' Create the form that we're going to populate for updating this xform control
                    Me.NewFrm("EditGroup")

                    ' Load in the form from a file
                    Me.loadControlForm("group")

                    ' load a default content xform if no alternative.
                    If Not String.IsNullOrEmpty(cRef) Then
                        oElmt = _masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                        Me.LoadInstanceFromInnerXml(oElmt.OuterXml)
                    End If

                    If Me.isSubmitted Then
                        Me.updateInstanceFromRequest()
                        Me.validate()
                        If Me.valid Then
                            If cRef <> "" Then
                                'drop the instance back into the full xform
                                Dim oNode As XmlNode = _masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                                oNode.ParentNode.ReplaceChild(Me.Instance.FirstChild, oNode)
                            Else
                                'add new
                                Dim oNode As XmlNode = _masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" & cParRef & "' or @bind='" & cParRef & "']")
                                newRef = _masterXform.getNewRef(goRequest("cRef"))
                                oElmt = Me.Instance.FirstChild()
                                oElmt.SetAttribute("ref", newRef)
                                oNode.AppendChild(Me.Instance.FirstChild())
                            End If
                        Else
                            Me.addValues()
                        End If
                    Else
                        Me.addValues()
                    End If

                    Return Me.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function


            Public Overridable Function xFrmDeleteElement(ByVal cRef As String, ByVal cPosIndex As String) As XmlElement

                Dim oFrmElmt As XmlElement
                Dim oNode As XmlNode
                Dim cProcessInfo As String = "cRef = " & cRef & ", cPosIndex=" & cPosIndex

                Try

                    ' Generate the Delete form within the component

                    ' Indentify the node by ref and position, if given.
                    If cPosIndex <> "" Then
                        oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & cPosIndex & "]")
                    Else
                        oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                    End If

                    ' Create the form
                    Me.NewFrm("EditSelect")
                    Me.submission("EditInput", "", "post")
                    oFrmElmt = Me.addGroup(Me.moXformElmt, "EditGroup", "", "Delete Element")
                    Me.addNote(oFrmElmt, xForm.noteTypes.Alert, "Are you sure you want to delete this element - """ & oNode.SelectSingleNode("label").InnerText & """")
                    Me.addSubmit(oFrmElmt, "", "Delete Element")
                    Me.LoadInstanceFromInnerXml("<delete/>")

                    ' Handle the submission
                    If Me.isSubmitted Then
                        Me.updateInstanceFromRequest()
                        Me.validate()
                        If Me.valid Then

                            ' Delete the node and/or bind
                            deleteElementAction(oNode, cRef, cPosIndex)

                        Else
                            Me.addValues()
                        End If
                    Else
                        Me.addValues()
                    End If

                    Return Me.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmDeleteElement", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            ''' <summary>
            ''' Deletes nodes that relate to the element / item.
            ''' Problem we face is that we don't know what to delete from the instance, 
            ''' so we will assume that the instance node we're loooking for has a matching @ref attribute.
            ''' If it's an item we will also assume that the @ref node has children that have a child node of value which
            ''' matches the value of the item being deleted.
            ''' </summary>
            ''' <param name="ref"></param>
            ''' <param name="position"></param>
            ''' <remarks></remarks>
            Protected Overridable Sub deleteElementAction(ByRef node As XmlNode, ByVal ref As String, Optional ByVal position As String = "")
                Dim processInfo As String = ""
                Dim value As String = ""
                Dim node2 As XmlNode
                Try

                    ' If we're deleting an item, then we don't need to remove the bind, but we do need to remove the item
                    If Not String.IsNullOrEmpty(position) Then
                        'remove related node's item
                        value = node.SelectSingleNode("value").InnerText
                        node2 = _masterXform.Instance.SelectSingleNode("//*[@ref='" & ref & "']")
                        If Not node2.SelectSingleNode("*[value/node()='" & value & "']") Is Nothing Then
                            'remove existing node
                            node2.RemoveChild(node2.SelectSingleNode("*[value/node()='" & value & "']"))
                        End If
                    Else
                        'remove related node
                        node2 = _masterXform.Instance.SelectSingleNode("//*[@ref='" & ref & "']")
                        If Not node2 Is Nothing Then
                            node2.ParentNode.RemoveChild(node2)
                        End If

                        'remove bind
                        If Not _masterXform.model.SelectSingleNode("descendant-or-self::bind[@id='" & ref & "']") Is Nothing Then
                            node2 = _masterXform.model.SelectSingleNode("descendant-or-self::bind[@id='" & ref & "']")
                            node2.ParentNode.RemoveChild(node2)
                        End If

                    End If

                    'remove the element itself
                    node.ParentNode.RemoveChild(node)

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "deleteElementAction", ex, "", processInfo, gbDebug)
                End Try
            End Sub

            Public Sub moveElement(ByVal nContentId As Long, ByVal cRef As String, ByVal ewCmd As String, Optional ByVal nItemIndex As Long = 0)

                Dim oElmt As XmlElement

                Dim cProcessInfo As String = "nContentId = " & nContentId & ",cRef = " & cRef & ",ewCmd = " & ewCmd & ", nItemIndex=" & nItemIndex

                Try
                    If nItemIndex = 0 Then
                        oElmt = _masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                    Else
                        oElmt = _masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & nItemIndex & "]")
                    End If
                    Select Case ewCmd
                        Case "MoveTop", "MoveItemTop"
                            oElmt.ParentNode.InsertBefore(oElmt.CloneNode(True), oElmt.ParentNode.FirstChild)
                            oElmt.ParentNode.RemoveChild(oElmt)
                        Case "MoveUp", "MoveItemUp"
                            oElmt.ParentNode.InsertBefore(oElmt.CloneNode(True), oElmt.PreviousSibling)
                            oElmt.ParentNode.RemoveChild(oElmt)
                        Case "MoveDown", "MoveItemDown"
                            oElmt.ParentNode.InsertAfter(oElmt.CloneNode(True), oElmt.NextSibling)
                            oElmt.ParentNode.RemoveChild(oElmt)
                        Case "MoveBottom", "MoveItemBottom"
                            oElmt.ParentNode.AppendChild(oElmt.CloneNode(True))
                            oElmt.ParentNode.RemoveChild(oElmt)
                        Case "DeleteItem"
                            oElmt.ParentNode.RemoveChild(oElmt)
                    End Select

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "moveElement", ex, "", cProcessInfo, gbDebug)
                End Try

            End Sub

            Public Overridable Function xFrmEditXFormItem(ByVal cRef As String, ByVal nItemIndex As Long) As XmlElement

                Dim oElmt As XmlElement = Nothing
                Dim oNode As XmlNode
                Dim sValue As String

                Dim nCount As Long

                Dim cProcessInfo As String = "cRef = " & cRef & ", nItemIndex=" & nItemIndex

                Try
                    'load the xform to be edited

                    If nItemIndex <> 0 Then
                        oElmt = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & nItemIndex & "]")
                    End If

                    ' Create the form that we're going to populate for updating this xform control
                    Me.NewFrm("EditSelect")

                    ' Load in the form from a file
                    Me.loadControlForm("item")

                    ' set the instance from the item loaded from the xform
                    If nItemIndex <> 0 Then

                        Me.Instance.AppendChild(oElmt.CloneNode(True))

                        ' add weighting and correct flag to the item node from answer
                        oElmt = Me.Instance.FirstChild
                        sValue = oElmt.SelectSingleNode("value").InnerText

                    Else

                        ' Multi choice, auto indexing
                        For Each oNode In _masterXform.moXformElmt.SelectNodes("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item")
                            If CInt(oNode.SelectSingleNode("value").InnerText) > nCount Then
                                nCount = CInt(oNode.SelectSingleNode("value").InnerText)
                            End If
                        Next
                        Me.LoadInstanceFromInnerXml("<item><label/><value>" & nCount + 1 & "</value></item>")
                    End If

                    If Me.isSubmitted Then
                        Me.updateInstanceFromRequest()
                        Me.validate()
                        If Me.valid Then

                            sValue = Me.Instance.SelectSingleNode("item/value").InnerText

                            If nItemIndex <> 0 Then
                                'drop the instance back into the full xform
                                oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & nItemIndex & "]")
                                oNode.ParentNode.ReplaceChild(Me.Instance.FirstChild, oNode)
                            Else
                                'add new
                                oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                                oNode.AppendChild(Me.Instance.FirstChild)
                            End If
                            oNode = Nothing
                        Else
                            Me.addValues()
                        End If
                    Else
                        Me.addValues()
                    End If

                    Return Me.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormItem", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function

            Public Overridable Function xFrmEditXFormInput(ByVal cRef As String, Optional ByVal cParRef As String = "", Optional ByVal cElementType As String = "") As XmlElement

                Dim oElmt As XmlElement
                Dim cBind As String = ""
                Dim oNode As XmlNode
                Dim newRef As String
                Dim cProcessInfo As String = ""

                Try

                    ' Determine the node we're looking at
                    If cRef <> "" Then
                        oElmt = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                        cElementType = oElmt.Name
                    End If

                    ' Load in the form
                    Select Case cElementType
                        Case "select1"

                            ' Create a new form
                            Me.NewFrm("EditSelect")

                            ' Load in the form from a file
                            Me.loadControlForm("select1")

                        Case "select"

                            ' Create a new form
                            Me.NewFrm("EditSelect")

                            ' Load in the form from a file
                            Me.loadControlForm("select")

                        Case "input"

                            ' Create a new form
                            Me.NewFrm("EditSelect")

                            ' Load in the form from a file
                            Me.loadControlForm("input")

                        Case "textarea"

                            ' Create a new form
                            Me.NewFrm("EditSelect")

                            ' Load in the form from a file
                            Me.loadControlForm("textarea")

                    End Select

                    ' Load existing data
                    If Not String.IsNullOrEmpty(cRef) Then
                        oElmt = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                        Me.LoadInstanceFromInnerXml(oElmt.OuterXml)
                    End If


                    If Me.isSubmitted Then
                        Me.updateInstanceFromRequest()
                        Me.validate()

                        If Me.valid Then

                            If cRef = "" Then
                                newRef = _masterXform.getNewRef("control")
                            Else
                                newRef = cRef
                            End If

                            oElmt = Nothing

                            'remove anything unnessesary before save
                            Select Case cElementType
                                Case "select1", "select"
                                    'remove any empty item nodes
                                    For Each oNode In Me.Instance.FirstChild.SelectNodes("item")
                                        If oNode.FirstChild Is Nothing Then
                                            oNode.ParentNode.RemoveChild(oNode)
                                        Else
                                            If oNode.FirstChild.InnerText = "" Then
                                                oNode.ParentNode.RemoveChild(oNode)
                                            End If
                                        End If
                                    Next
                            End Select

                            'drop the instance back into the full xform
                            If cRef <> "" Then
                                'replace existing
                                oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                                oNode.ParentNode.ReplaceChild(Me.Instance.FirstChild, oNode)
                            Else
                                'add new
                                oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cParRef & "' or @bind='" & cParRef & "']")
                                oElmt = Me.Instance.FirstChild()

                                'TODO: XFormEditor (Generic) how do we identify the node xpath to create the bind.
                                ' This is just for the EonicWeb Generic xformeditor function
                                ' not for anything overridden like EonicLMS

                                'cBind = "whatgoeshere[@ref='" & newRef & "']"
                                'oElmt.SetAttribute("bind", newRef)
                                '_masterXform.addBind(newRef, cBind)

                                oNode.AppendChild(Me.Instance.FirstChild())
                            End If

                        Else
                            Me.addValues()
                        End If
                    Else
                        Me.addValues()
                    End If

                    Return Me.moXformElmt

                Catch ex As Exception
                    returnException(myWeb.msException, mcModuleName, "xFrmEditXFormInput", ex, "", cProcessInfo, gbDebug)
                    Return Nothing
                End Try
            End Function








            'Public Overridable Function xFrmEditXFormInput(ByVal cRef As String, Optional ByVal cParRef As String = "", Optional ByVal cElementType As String = "") As XmlElement
            '    Dim oFrmElmt As XmlElement
            '    Dim oRpt1Elmt As XmlElement
            '    Dim oSelElmt As XmlElement

            '    Dim oElmt As XmlElement
            '    Dim oElmt1 As XmlElement
            '    Dim oElmt2 As XmlElement
            '    Dim oElmt3 As XmlElement
            '    Dim oElmt4 As XmlElement
            '    Dim nCount As Long
            '    Dim sValidAnswers As String = ""

            '    Dim nAnswerCount As Long
            '    Dim cReqd As String
            '    Dim cBind As String = ""

            '    Dim oNode As XmlNode

            '    Dim i As Integer

            '    Dim newRef As String

            '    Dim cProcessInfo As String = ""

            '    Try

            '        If cRef <> "" Then
            '            oElmt = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
            '            cElementType = oElmt.Name
            '            nAnswerCount = oElmt.SelectNodes("item").Count + 1
            '        Else
            '            nAnswerCount = 4
            '        End If

            '        Select Case cElementType
            '            Case "select1"

            '                Me.NewFrm("EditSelect")

            '                Me.submission("EditInput", "", "post", "return form_check(this)")

            '                oFrmElmt = Me.addGroup(Me.moXformElmt, "EditSelect1", "", "Edit Select1")

            '                Me.addInput(oFrmElmt, "ref", True, "Question Ref", "hidden")
            '                Me.addBind("ref", "select1/@ref", "true()")

            '                Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
            '                Me.addBind("cName", "select1/label", "true()")

            '                Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
            '                Me.addBind("cDesc", "select1/div[@class='description']", "false()")

            '                Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
            '                Me.addBind("cRecRead", "select1/div[@class='recRead']", "false()")

            '                oSelElmt = Me.addSelect1(oFrmElmt, "cAppearance", True, "Appearance", "", ApperanceTypes.Minimal)
            '                Me.addOption(oSelElmt, "Radio Buttons", "full")
            '                Me.addOption(oSelElmt, "Dropdown Selector", "minimal")
            '                Me.addBind("cAppearance", "select1/@appearance", "true()")

            '                For i = 1 To nAnswerCount
            '                    oRpt1Elmt = Me.addGroup(oFrmElmt, "Answer " & i, "horizontal", "Answer " & i)
            '                    Me.addTextArea(oRpt1Elmt, "A" & i, True, "Answer", "xhtml answerEditor")
            '                    If i = nAnswerCount Then
            '                        cReqd = "false()"
            '                    Else
            '                        cReqd = "true()"
            '                    End If
            '                    Me.addBind("A" & i, "select1/item[" & i & "]/label", cReqd)

            '                    oSelElmt = Me.addSelect1(oRpt1Elmt, "ATick", True, "Correct ", "", ApperanceTypes.Full)
            '                    Me.addOption(oSelElmt, "", i)
            '                Next
            '                Me.addBind("ATick", "select1/@correctIndex", "false()")

            '                Me.addRange(oFrmElmt, "AWeighting", True, "Weighting", "0", "100", "10", "short")

            '                Me.addBind("AWeighting", "select1/@weighting", "true()")

            '                Me.addSubmit(oFrmElmt, "", "Save Group")

            '            Case "select"

            '                Me.NewFrm("EditSelect")

            '                Me.submission("EditInput", "", "post", "return form_check(this)")

            '                oFrmElmt = Me.addGroup(Me.moXformElmt, "EditSelect", "", "Edit Select")

            '                Me.addInput(oFrmElmt, "ref", True, "Question Ref", "hidden")
            '                Me.addBind("ref", "select/@ref", "true()")

            '                Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
            '                Me.addBind("cName", "select/label", "true()")

            '                Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
            '                Me.addBind("cDesc", "select/div[@class='description']", "false()")

            '                Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
            '                Me.addBind("cRecRead", "select/div[@class='recRead']", "false()")

            '                oSelElmt = Me.addSelect1(oFrmElmt, "cAppearance", True, "Appearance", "", ApperanceTypes.Minimal)
            '                Me.addOption(oSelElmt, "CheckBoxes", "full")
            '                Me.addOption(oSelElmt, "Dropdown Selector", "minimal")
            '                Me.addBind("cAppearance", "select/@appearance", "true()")

            '                For i = 1 To nAnswerCount
            '                    oRpt1Elmt = Me.addGroup(oFrmElmt, "Answer " & i, "horizontal", "Answer " & i)
            '                    Me.addTextArea(oRpt1Elmt, "A" & i, True, "Answer", "xhtml answerEditor")
            '                    Me.addBind("A" & i, "select/item[" & i & "]/label", "false()")

            '                    oSelElmt = Me.addSelect(oRpt1Elmt, "A" & i & "Tick", True, "Correct ", "", ApperanceTypes.Full)
            '                    Me.addOption(oSelElmt, "", "true")
            '                    Me.addBind("A" & i & "Tick", "select/item[" & i & "]/@correct", "false()")

            '                    Me.addRange(oRpt1Elmt, "A" & i & "Weighting", True, "Weighting", "0", "100", "10", "short")

            '                    Me.addBind("A" & i & "Weighting", "select/item[" & i & "]/@weighting", "false()")
            '                Next

            '                Me.addSubmit(oFrmElmt, "", "Save Group")

            '            Case "input"

            '                Me.NewFrm("EditSelect")

            '                Me.submission("EditInput", "", "post", "return form_check(this)")

            '                oFrmElmt = Me.addGroup(Me.moXformElmt, "EditInput", "", "Edit Input")

            '                '  Me.addInput(oFrmElmt, "ref", True, "Question Ref", "hidden")
            '                ' Me.addBind("ref", "input/@ref", "true()")

            '                Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
            '                Me.addBind("cName", "input/label", "true()")

            '                Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
            '                Me.addBind("cDesc", "input/div[@class='description']", "false()")

            '                Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
            '                Me.addBind("cRecRead", "input/div[@class='recRead']", "false()")

            '                Me.addInput(oFrmElmt, "cAnswers", True, "Valid Answers")
            '                Me.addBind("cAnswers", "input/@validAnswers", "false()")
            '                Me.addNote("cAnswers", xForm.noteTypes.Hint, "Comma separated list of valid answers")

            '                Me.addRange(oFrmElmt, "cWeighting", True, "Weighting", "0", "100", "10")
            '                Me.addBind("cWeighting", "input/@weighting", "true()")

            '                Me.addSubmit(oFrmElmt, "", "Save Group")

            '            Case "textarea"

            '                Me.NewFrm("EditSelect")

            '                Me.submission("EditInput", "", "post", "return form_check(this)")

            '                oFrmElmt = Me.addGroup(Me.moXformElmt, "EditInput", "", "Edit Comprehension Question")

            '                Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
            '                Me.addBind("cName", "textarea/label", "true()")

            '                Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
            '                Me.addBind("cDesc", "textarea/div[@class='description']", "false()")

            '                Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
            '                Me.addBind("cRecRead", "textarea/div[@class='recRead']", "false()")

            '                Me.addSubmit(oFrmElmt, "", "Save Group")

            '        End Select

            '        ' load a default content xform if no alternative.

            '        If cRef <> "" Then
            '            oElmt = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
            '            Select Case cElementType
            '                Case "select1"
            '                    'step through the answers in the question were editing
            '                    i = 1
            '                    Dim bResult As Boolean = False
            '                    For Each oElmt2 In oElmt.SelectNodes("item")

            '                        'get the answer value
            '                        Dim sVal As String = oElmt2.SelectSingleNode("value").InnerText
            '                        'see if that value is in score of the answers fot that question
            '                        If Not moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']") Is Nothing Then
            '                            oElmt.SetAttribute("correctIndex", i)
            '                            oElmt3 = moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']")
            '                            oElmt.SetAttribute("weighting", oElmt3.GetAttribute("weighting"))
            '                            bResult = True
            '                        End If
            '                        i = i + 1

            '                    Next
            '                    If Not bResult Then ' add in the empties
            '                        oElmt.SetAttribute("correctIndex", "")
            '                        oElmt.SetAttribute("weighting", "")
            '                    End If
            '                Case "select"
            '                    'step through the answers in the question were editing
            '                    For Each oElmt2 In oElmt.SelectNodes("item")
            '                        'get the answer value
            '                        Dim sVal As String = oElmt2.SelectSingleNode("value").InnerText
            '                        'see if that value is in score of the answers fot that question
            '                        If Not moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']") Is Nothing Then
            '                            oElmt2.SetAttribute("correct", "true")
            '                            oElmt3 = moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']")
            '                            oElmt2.SetAttribute("weighting", oElmt3.GetAttribute("weighting"))
            '                        Else
            '                            oElmt2.SetAttribute("correct", "false")
            '                            oElmt2.SetAttribute("weighting", "")
            '                        End If
            '                    Next
            '                Case "input"
            '                    For Each oElmt2 In moXForm2Edit.Instance.SelectNodes("results/answers/answer[@ref='" & cRef & "']/score/value")
            '                        If sValidAnswers = "" Then
            '                            sValidAnswers = oElmt2.InnerText
            '                        Else
            '                            sValidAnswers = sValidAnswers & ", " & oElmt2.InnerText
            '                        End If
            '                    Next
            '                    oElmt.SetAttribute("validAnswers", sValidAnswers)
            '                    oElmt.SetAttribute("weighting", moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score/@weighting").Value)
            '            End Select

            '            Me.Instance.AppendChild(oElmt.CloneNode(True))

            '        Else
            '            Select Case cElementType
            '                Case "select1"
            '                    Me.Instance.InnerXml = "<select1 bind="""" appearance="""" correctIndex="""" weighting=""""><label/><div class=""description""/><div class=""recRead""/></select1>"
            '                Case "select"
            '                    Me.Instance.InnerXml = "<select bind="""" appearance=""""><label/><div class=""description""/><div class=""recRead""/></select>"
            '                Case "input"
            '                    Me.Instance.InnerXml = "<input bind="""" validAnswers="""" weighting=""""><label/><div class=""description""/><div class=""recRead""/></input>"
            '                Case "textarea"
            '                    Me.Instance.InnerXml = "<textarea bind="""" rows=""20"" class=""textarea_compquiz""><label/><div class=""description""/><div class=""recRead""/></textarea>"

            '            End Select
            '        End If

            '        Select Case cElementType
            '            Case "select1", "select"
            '                'ensure we have the right number of blank item nodes to update.
            '                For i = 1 To nAnswerCount
            '                    If Me.Instance.FirstChild.SelectSingleNode("item[" & i & "]") Is Nothing Then
            '                        'populate new nodes with values
            '                        For Each oNode In Me.Instance.FirstChild.SelectNodes("item")
            '                            If CInt(oNode.SelectSingleNode("value").InnerText) > nCount Then
            '                                nCount = CInt(oNode.SelectSingleNode("value").InnerText)
            '                            End If
            '                        Next
            '                        oElmt1 = Me.addOption(Me.Instance.FirstChild, "", nCount + 1)
            '                        If cElementType = "select" Then
            '                            oElmt1.SetAttribute("correct", "")
            '                            oElmt1.SetAttribute("weighting", "")
            '                        End If
            '                    End If
            '                Next

            '        End Select

            '        If Me.isSubmitted Then
            '            Me.updateInstanceFromRequest()
            '            Me.validate()

            '            If Me.valid Then

            '                If cRef = "" Then
            '                    newRef = moXForm2Edit.getNewRef("Q")
            '                Else
            '                    newRef = cRef
            '                End If

            '                oElmt = Nothing

            '                'Update the Answer Node
            '                Select Case cElementType
            '                    Case "select1"
            '                        'Code to update single answer node
            '                        'get the correctIndex and find the value
            '                        oElmt2 = Me.Instance.SelectSingleNode("select1")
            '                        If oElmt2.GetAttribute("correctIndex") <> "" Then
            '                            Dim nCorIdx As Integer = oElmt2.GetAttribute("correctIndex")

            '                            If Not Me.Instance.SelectSingleNode("select1/item[" & nCorIdx & "]") Is Nothing Then
            '                                oElmt3 = Me.Instance.SelectSingleNode("select1/item[" & nCorIdx & "]")
            '                                oElmt4 = oElmt3.SelectSingleNode("value")
            '                                oElmt = addAnswerElmt(moXForm2Edit, newRef, oElmt2.GetAttribute("weighting"), oElmt4.InnerText, True)
            '                            End If
            '                        End If

            '                    Case "select"
            '                        'Code to update any answer nodes required
            '                        'remove any existing answernodes
            '                        clearAnswerScores(moXForm2Edit, newRef)
            '                        'step through the answers in the select instance to find valid correct ones
            '                        For Each oElmt2 In Me.Instance.SelectNodes("select/item")
            '                            If oElmt2.GetAttribute("correct") = "true" Then
            '                                oElmt3 = oElmt2.SelectSingleNode("value")
            '                                oElmt = addAnswerElmt(moXForm2Edit, newRef, oElmt2.GetAttribute("weighting"), oElmt3.InnerText)
            '                            End If
            '                        Next
            '                    Case "input"
            '                        'Code to update answer node
            '                        oElmt = addAnswerElmt(moXForm2Edit, newRef, moRequest("cWeighting"), "", True)
            '                        If moRequest("cAnswers") <> "" Then
            '                            Dim aCorrect() As String = Split(moRequest("cAnswers"), ",")
            '                            For i = 0 To UBound(aCorrect)
            '                                addCorrectAnswer(oElmt, Trim(aCorrect(i)))
            '                            Next
            '                        End If
            '                    Case "textarea"
            '                        'Code to update answer node
            '                        oElmt = addAnswerElmt(moXForm2Edit, newRef, moRequest("cWeighting"), "", True, cElementType)
            '                End Select


            '                'remove anything unnessesary before save
            '                Select Case cElementType
            '                    Case "select1", "select"
            '                        'remove any empty item nodes
            '                        For Each oNode In Me.Instance.FirstChild.SelectNodes("item")
            '                            If oNode.FirstChild Is Nothing Then
            '                                oNode.ParentNode.RemoveChild(oNode)
            '                            Else
            '                                If oNode.FirstChild.InnerText = "" Then
            '                                    oNode.ParentNode.RemoveChild(oNode)
            '                                End If
            '                            End If
            '                            If cElementType = "select" Then
            '                                oElmt2 = oNode
            '                                oElmt2.RemoveAttribute("correct")
            '                                oElmt2.RemoveAttribute("weighting")
            '                                oElmt2 = Nothing
            '                            End If
            '                        Next
            '                        If cElementType = "select" Then
            '                            oElmt2 = Me.Instance.FirstChild
            '                            oElmt2.RemoveAttribute("correctIndex")
            '                            oElmt2.RemoveAttribute("weighting")
            '                            oElmt2 = Nothing
            '                        End If

            '                    Case "input"
            '                        'remove validAnswers Attrib
            '                        oElmt = Me.Instance.FirstChild
            '                        oElmt.RemoveAttribute("validAnswers")

            '                End Select

            '                'drop the instance back into the full xform
            '                If cRef <> "" Then
            '                    'replace existing
            '                    oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
            '                    oNode.ParentNode.ReplaceChild(Me.Instance.FirstChild, oNode)
            '                Else
            '                    'add new
            '                    oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cParRef & "' or @bind='" & cParRef & "']")
            '                    oElmt = Me.Instance.FirstChild()

            '                    'everything is bound to a single answer node
            '                    Select Case cElementType
            '                        Case "textarea"
            '                            cBind = "results/answers/answer[@ref='" & newRef & "']/given"
            '                        Case Else
            '                            cBind = "results/answers/answer[@ref='" & newRef & "']/@given"
            '                    End Select
            '                    oElmt.SetAttribute("bind", newRef)
            '                    moXForm2Edit.addBind(newRef, cBind)

            '                    oNode.AppendChild(Me.Instance.FirstChild())
            '                End If

            '            Else
            '                Me.addValues()
            '            End If
            '        Else
            '            Me.addValues()
            '        End If

            '        Return Me.moXformElmt

            '    Catch ex As Exception
            '        returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
            '        Return Nothing
            '    End Try
            'End Function


            'Public Overridable Function xFrmEditXFormItem(ByVal cRef As String, ByVal nItemIndex As Long) As XmlElement
            '    Dim oFrmElmt As XmlElement
            '    Dim oSelElmt As XmlElement

            '    Dim oElmt As XmlElement = Nothing

            '    Dim oNode As XmlNode

            '    Dim sQuestionType As String
            '    Dim sValue As String

            '    Dim nCount As Long

            '    Dim cProcessInfo As String = ""

            '    Try
            '        'load the xform to be edited

            '        If nItemIndex <> 0 Then
            '            oElmt = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & nItemIndex & "]")
            '        End If

            '        'What Q type are we editing...?
            '        sQuestionType = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']").Name


            '        Me.NewFrm("EditSelect")

            '        Me.submission("EditInput", "", "post", "return form_check(this)")
            '        oFrmElmt = Me.addGroup(Me.moXformElmt, "EditGroup", "", "Edit Answer")

            '        Me.addTextArea(oFrmElmt, "cName", True, "Answer", "xhtml answerEditor")
            '        Me.addBind("cName", "item/label", "true()")

            '        oSelElmt = Me.addSelect(oFrmElmt, "bCorrect", True, "Correct Answer", "", ApperanceTypes.Full)
            '        Me.addOption(oSelElmt, "Correct", "true")
            '        Me.addBind("bCorrect", "item/@correct", "false()")

            '        Me.addRange(oFrmElmt, "cWeighting", True, "Weighting", "0", "100", "5")
            '        Me.addBind("cWeighting", "item/@weighting", "false()")

            '        Me.addSubmit(oFrmElmt, "", "Save Answer")

            '        ' set the instance from the item loaded from the xform
            '        If nItemIndex <> 0 Then
            '            Me.Instance.AppendChild(oElmt.CloneNode(True))
            '            ' add weighting and correct flag to the item node from answer
            '            oElmt = Me.Instance.FirstChild

            '            sValue = oElmt.SelectSingleNode("value").InnerText

            '            If moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sValue & "']") Is Nothing Then
            '                oElmt.SetAttribute("correct", "false")
            '                oElmt.SetAttribute("weighting", "")
            '            Else
            '                oElmt.SetAttribute("correct", "true")
            '                oElmt.SetAttribute("weighting", moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sValue & "']/@weighting").Value)
            '            End If

            '        Else
            '            For Each oNode In moXForm2Edit.moXformElmt.SelectNodes("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item")
            '                If CInt(oNode.SelectSingleNode("value").InnerText) > nCount Then
            '                    nCount = CInt(oNode.SelectSingleNode("value").InnerText)
            '                End If
            '            Next
            '            Me.Instance.InnerXml = "<item><label/><value>" & nCount + 1 & "</value></item>"
            '        End If

            '        If Me.isSubmitted Then
            '            Me.updateInstanceFromRequest()
            '            Me.validate()
            '            If Me.valid Then
            '                'remove the weighting and correct attribs
            '                Me.Instance.RemoveAttribute("correct")
            '                Me.Instance.RemoveAttribute("weighting")

            '                sValue = Me.Instance.SelectSingleNode("item/value").InnerText

            '                If nItemIndex <> 0 Then
            '                    'drop the instance back into the full xform
            '                    oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & nItemIndex & "]")
            '                    oNode.ParentNode.ReplaceChild(Me.Instance.FirstChild, oNode)
            '                Else
            '                    'add new
            '                    oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
            '                    oNode.AppendChild(Me.Instance.FirstChild)
            '                End If
            '                oNode = Nothing

            '                If moRequest("bCorrect") = "true" Then
            '                    Select Case sQuestionType
            '                        Case "select1"
            '                            oElmt = addAnswerElmt(moXForm2Edit, cRef, moRequest("cWeighting"), sValue, True)
            '                        Case Else
            '                            oElmt = addAnswerElmt(moXForm2Edit, cRef, moRequest("cWeighting"), sValue, False)
            '                    End Select

            '                Else
            '                    'if we are a select (multi choice) we need to remove the score node if exists.
            '                    oNode = moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']")
            '                    Select Case sQuestionType
            '                        Case "select"
            '                            If Not oNode.SelectSingleNode("score[value/node()='" & sValue & "']") Is Nothing Then
            '                                'remove existing node
            '                                oNode.RemoveChild(oNode.SelectSingleNode("score[value/node()='" & sValue & "']"))

            '                            End If
            '                    End Select
            '                End If
            '            Else
            '                Me.addValues()
            '            End If
            '        Else
            '            Me.addValues()
            '        End If

            '        Return Me.moXformElmt

            '    Catch ex As Exception
            '        returnException(myWeb.msException, mcModuleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
            '        Return Nothing
            '    End Try
            'End Function

            'Private Function addAnswerElmt(ByRef oxFrm As Protean.xForm, ByVal cRef As String, ByVal cWeighting As String, Optional ByVal cValue As String = "", Optional ByVal bSingleAnswer As Boolean = False, Optional ByVal cElementType As String = "") As XmlElement

            '    Dim oElmt As XmlElement
            '    Dim oElmt2 As XmlElement
            '    Dim oElmt3 As XmlElement

            '    Dim oNode As XmlNode


            '    Dim cProcessInfo As String = ""
            '    Try
            '        If oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']") Is Nothing Then

            '            oElmt = oxFrm.moPageXML.CreateElement("answer")
            '            oElmt.SetAttribute("ref", cRef)
            '            Select Case cElementType
            '                Case "textarea"
            '                    addNewTextNode("given", oElmt)
            '                Case Else
            '                    oElmt.SetAttribute("given", "")
            '            End Select
            '            oElmt.SetAttribute("mark", "")
            '        Else
            '            oElmt = oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']").CloneNode(True)
            '            If bSingleAnswer Then
            '                'remove existing score
            '                For Each oNode In oElmt.SelectNodes("score")
            '                    oNode.ParentNode.RemoveChild(oNode)
            '                Next
            '            End If
            '        End If
            '        If Not oElmt.SelectSingleNode("score[value/node()='" & cValue & "']") Is Nothing Then
            '            'score exists update the weighting
            '            oElmt2 = oElmt.SelectSingleNode("score[value/node()='" & cValue & "']")
            '            oElmt2.SetAttribute("weighting", cWeighting)
            '        Else
            '            oElmt2 = oxFrm.moPageXML.CreateElement("score")
            '            oElmt2.SetAttribute("weighting", cWeighting)
            '            If cValue <> "" Then
            '                oElmt3 = oxFrm.moPageXML.CreateElement("value")
            '                oElmt3.InnerText = cValue
            '                oElmt2.AppendChild(oElmt3)
            '            End If
            '            oElmt.AppendChild(oElmt2)
            '        End If

            '        If oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']") Is Nothing Then
            '            'Create a new one
            '            oNode = oxFrm.Instance.SelectSingleNode("results/answers")
            '            oNode.AppendChild(oElmt)
            '        Else
            '            'replace existing
            '            oNode = oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']")
            '            'replace the whole lot
            '            oNode.ParentNode.ReplaceChild(oElmt, oNode)
            '        End If

            '        Return oElmt

            '    Catch ex As Exception
            '        returnException(myWeb.msException, mcModuleName, "addAnswerNode", ex, "", cProcessInfo, gbDebug)
            '        Return Nothing
            '    End Try
            'End Function

            'Private Sub clearAnswerScores(ByRef oxFrm As Protean.xForm, ByVal cRef As String)

            '    Dim oElmt As XmlElement

            '    Dim oNode As XmlNode


            '    Dim cProcessInfo As String = ""
            '    Try
            '        If Not oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']") Is Nothing Then
            '            oElmt = oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']")
            '            For Each oNode In oElmt.SelectNodes("score")
            '                oNode.ParentNode.RemoveChild(oNode)
            '            Next
            '        End If

            '    Catch ex As Exception
            '        returnException(myWeb.msException, mcModuleName, "clearAnswerScores", ex, "", cProcessInfo, gbDebug)
            '    End Try
            'End Sub

            'Private Function addCorrectAnswer(ByRef oElmt As XmlElement, ByVal cValue As String) As XmlElement

            '    Dim oElmt3 As XmlElement

            '    Dim cProcessInfo As String = ""
            '    Try

            '        oElmt3 = oElmt.OwnerDocument.CreateElement("value")
            '        oElmt3.InnerText = cValue
            '        oElmt.FirstChild.AppendChild(oElmt3)

            '        Return oElmt
            '    Catch ex As Exception
            '        returnException(myWeb.msException, mcModuleName, "addCorrectAnswer", ex, "", cProcessInfo, gbDebug)
            '        Return Nothing
            '    End Try
            'End Function


        End Class




        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class
End Class

