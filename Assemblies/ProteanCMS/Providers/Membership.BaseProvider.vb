'***********************************************************************
' $Library:     Protean.Providers.membership.base
' $Revision:    3.1  
' $Date:        2012-07-21
' $Author:      Trevor Spink (trevor@eonic.co.uk)
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
'***********************************************************************

Option Strict Off
Option Explicit On

Imports System
Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.Configuration
Imports System.IO
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Protean.Cms
Imports Protean.Tools
Imports Protean.Tools.Xml
Imports Protean.Cms.Cart
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports VB = Microsoft.VisualBasic

Namespace Providers
    Namespace Membership

        Public Class BaseProvider


            Private Const mcModuleName As String = "Protean.Providers.Membership.BaseProvider"

            Private _AdminXforms As Object
            Private _AdminProcess As Object
            Private _Activities As Object

            Protected moPaymentCfg As XmlNode

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Public Event OnErrorWithWeb(ByRef myweb As Protean.Cms, ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

            Public Property AdminXforms() As Object
                Set(ByVal value As Object)
                    _AdminXforms = value
                End Set
                Get
                    Return _AdminXforms
                End Get
            End Property

            Public Property AdminProcess() As Object
                Set(ByVal value As Object)
                    _AdminProcess = value
                End Set
                Get
                    Return _AdminProcess
                End Get
            End Property

            Public Property Activities() As Object
                Set(ByVal value As Object)
                    _Activities = value
                End Set
                Get
                    Return _Activities
                End Get
            End Property

            Public Sub New(ByRef myWeb As Object, ByVal ProviderName As String)
                Try

                    Dim calledType As Type
                    If ProviderName = "" Then
                        ProviderName = "Protean.Providers.Membership.EonicProvider"
                        calledType = System.Type.GetType(ProviderName, True)
                    Else
                        Dim castObject As Object = WebConfigurationManager.GetWebApplicationSection("protean/membershipProviders")
                        Dim moPrvConfig As Protean.ProviderSectionHandler = castObject
                        Dim ourProvider As Object = moPrvConfig.Providers(ProviderName)
                        Dim assemblyInstance As [Assembly]
                        If ourProvider.parameters("path") <> "" Then
                            assemblyInstance = [Assembly].LoadFrom(goServer.MapPath(ourProvider.parameters("path")))
                        Else
                            assemblyInstance = [Assembly].Load(ourProvider.Type)
                        End If
                        If ourProvider.parameters("rootClass") = "" Then
                            calledType = assemblyInstance.GetType("Protean.Providers.Membership." & ProviderName, True)
                        Else
                            'calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & ".Providers.Messaging", True)
                            calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & ".Providers.Membership." & ProviderName, True)
                        End If
                    End If

                    Dim o As Object = Activator.CreateInstance(calledType)

                    Dim args(4) As Object
                    args(0) = _AdminXforms
                    args(1) = _AdminProcess
                    args(2) = _Activities
                    args(3) = Me
                    args(4) = myWeb

                    calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, Nothing, o, args)

                Catch ex As Exception
                    returnException(mcModuleName, "New", ex, "", ProviderName & " Could Not be Loaded", gbDebug)
                End Try

            End Sub

        End Class

        Public Class EonicProvider

            Public Sub New()
                'do nothing
            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Protean.Cms)

                MemProvider.AdminXforms = New AdminXForms(myWeb)
                '  MemProvider.AdminProcess = New AdminProcess(myWeb)
                '  MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                MemProvider.Activities = New Activities()

            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Protean.Base)

                ' MemProvider.AdminXforms = New AdminXForms(myWeb)
                '  MemProvider.AdminProcess = New AdminProcess(myWeb)
                '  MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                MemProvider.Activities = New Activities()

            End Sub

            Public Class AdminXForms
                Inherits Cms.Admin.AdminXforms
                Private Const mcModuleName As String = "Providers.Membership.Eonic.AdminXForms"
                Public maintainMembershipsOnAdd As Boolean = True

                Sub New(ByRef aWeb As Cms)
                    MyBase.New(aWeb)
                End Sub

                Public Overridable Function xFrmUserLogon(Optional ByVal FormName As String = "UserLogon") As XmlElement

                    ' Called to get XML for the User Logon.

                    Dim oFrmElmt As XmlElement
                    Dim oSelElmt As XmlElement
                    Dim sValidResponse As String
                    Dim cProcessInfo As String = ""
                    Dim bRememberMe As Boolean = False
                    Try
                        MyBase.NewFrm("UserLogon")

                        If mbAdminMode And myWeb.mnUserId = 0 Then GoTo BuildForm
                        If myWeb.moConfig("RememberMeMode") = "KeepCookieAfterLogoff" Or myWeb.moConfig("RememberMeMode") = "ClearCookieAfterLogoff" Then bRememberMe = True

                        'maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        If Not MyBase.load("/xforms/directory/" & FormName & ".xml", myWeb.maCommonFolders) Then
                            'If this does not load manually then build a form to do it.
                            GoTo BuildForm
                        Else
                            GoTo Check
                        End If
BuildForm:
                        MyBase.submission("UserLogon", "", "post", "form_check(this)")

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "UserDetails", "", "Please fill in your login details below.")

                        MyBase.addInput(oFrmElmt, "cUserName", True, "Username")
                        MyBase.addBind("cUserName", "user/username", "true()")
                        MyBase.addSecret(oFrmElmt, "cPassword", True, "Password")
                        MyBase.addBind("cPassword", "user/password", "true()")

                        MyBase.addSubmit(oFrmElmt, "ewSubmit", "Login")

                        MyBase.Instance.InnerXml = "<user rememberMe=""""><username/><password/></user>"
Check:
                        ' Set the action URL

                        'Is the membership email address secure.
                        If myWeb.moConfig("SecureMembershipAddress") <> "" Then
                            Dim oSubElmt As XmlElement = MyBase.moXformElmt.SelectSingleNode("descendant::submission")
                            oSubElmt.SetAttribute("action", myWeb.moConfig("SecureMembershipAddress") & myWeb.moConfig("ProjectPath") & "/" & myWeb.mcPagePath)
                        End If

                        ' Set the remember me value, username from cookie.
                        If bRememberMe Then

                            ' Add elements to the form if not present
                            If Tools.Xml.NodeState(MyBase.model, "bind[@id='cRemember']") = Tools.Xml.XmlNodeState.NotInstantiated Then
                                oSelElmt = MyBase.addSelect(MyBase.moXformElmt.SelectSingleNode("group"), "cRemember", True, "&#160;", "", ApperanceTypes.Full)
                                MyBase.addOption(oSelElmt, "Remember me", "true")
                                MyBase.addBind("cRemember", "user/@rememberMe", "false()")
                            End If

                            ' Retrieve values from the cookie
                            If Not goRequest.Cookies("RememberMeUserName") Is Nothing Then
                                Dim cRememberedUsername As String = goRequest.Cookies("RememberMeUserName").Value
                                Dim bRemembered As Boolean = False
                                Dim oElmt As XmlElement = Nothing

                                If cRememberedUsername <> "" Then bRemembered = True

                                If Tools.Xml.NodeState(MyBase.Instance, "user", , , , oElmt) <> Tools.Xml.XmlNodeState.NotInstantiated And Not (MyBase.isSubmitted) Then

                                    oElmt.SetAttribute("rememberMe", LCase(CStr(bRemembered)))
                                    Tools.Xml.NodeState(MyBase.Instance, "user/username", cRememberedUsername)

                                End If
                            End If
                        End If

                        MyBase.updateInstanceFromRequest()

                        If MyBase.isSubmitted Then
                            MyBase.validate()
                            If MyBase.valid Then

                                'changed to get from instance rather than direct from querysting / form.
                                Dim username As String = MyBase.Instance.SelectSingleNode("user/username").InnerText
                                Dim password As String = MyBase.Instance.SelectSingleNode("user/password").InnerText

                                sValidResponse = moDbHelper.validateUser(username, password)

                                If IsNumeric(sValidResponse) Then
                                    myWeb.mnUserId = CLng(sValidResponse)
                                    moDbHelper.mnUserId = CLng(sValidResponse)
                                    If Not goSession Is Nothing Then
                                        goSession("nUserId") = myWeb.mnUserId

                                        Dim UserXml As XmlElement = myWeb.GetUserXML()
                                        If UserXml.GetAttribute("defaultCurrency") <> "" Then
                                            goSession("cCurrency") = UserXml.GetAttribute("defaultCurrency")
                                        End If

                                    End If

                                    ' Set the remember me cookie
                                    If bRememberMe Then
                                        If goRequest("cRemember") = "true" Then
                                            Dim oCookie As System.Web.HttpCookie
                                            If Not (myWeb.moRequest.Cookies("RememberMeUserName") Is Nothing) Then goResponse.Cookies.Remove("RememberMeUserName")
                                            oCookie = New System.Web.HttpCookie("RememberMeUserName")
                                            oCookie.Value = myWeb.moRequest("cUserName")
                                            oCookie.Expires = DateAdd(DateInterval.Day, 60, Now())
                                            goResponse.Cookies.Add(oCookie)

                                            If Not (myWeb.moRequest.Cookies("RememberMeUserId") Is Nothing) Then goResponse.Cookies.Remove("RememberMeUserId")
                                            oCookie = New System.Web.HttpCookie("RememberMeUserId")
                                            oCookie.Value = myWeb.mnUserId
                                            oCookie.Expires = DateAdd(DateInterval.Day, 60, Now())
                                            goResponse.Cookies.Add(oCookie)
                                        Else
                                            goResponse.Cookies("RememberMeUserName").Expires = DateTime.Now.AddDays(-1)
                                            goResponse.Cookies("RememberMeUserId").Expires = DateTime.Now.AddDays(-1)
                                        End If
                                    End If
                                Else
                                    valid = False
                                    MyBase.addNote(moXformElmt, xForm.noteTypes.Alert, sValidResponse)
                                End If
                            Else
                                valid = False
                            End If
                            If valid = False Then
                                myWeb.mnUserId = 0
                            End If
                        End If

                        MyBase.addValues()
                        Return MyBase.moXformElmt

                    Catch ex As Exception
                        returnException(mcModuleName, "xFrmUserLogon", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Overridable Function xFrmPasswordReminder() As XmlElement
                    Dim oFrmElmt As XmlElement
                    Dim sValidResponse As String
                    Dim cProcessInfo As String = ""
                    Dim getRecordByEmail As Boolean


                    Try

                        'Does the configuration setting indicate that email addresses are allowed.
                        If LCase(myWeb.moConfig("EmailUsernames")) = "on" Then
                            getRecordByEmail = True
                        Else
                            getRecordByEmail = False
                        End If

                        MyBase.NewFrm("PasswordReminder")

                        MyBase.submission("PasswordReminder", "", "post", "form_check(this)")

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "PasswordReminder")
                        MyBase.addDiv(oFrmElmt, "Please enter your email address and we will email you with your password.")
                        MyBase.addInput(oFrmElmt, "cEmail", True, "Email Address")
                        MyBase.addBind("cEmail", "user/email", "true()", "email")

                        MyBase.addSubmit(oFrmElmt, "", "Send Password", "ewSubmitReminder")

                        If Not MyBase.load("/xforms/passwordreminder.xml", myWeb.maCommonFolders) Then
                            MyBase.NewFrm("PasswordReminder")

                            MyBase.submission("PasswordReminder", "", "post", "form_check(this)")

                            oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "PasswordReminder")

                            MyBase.addDiv(oFrmElmt, "Please enter your email address and we will email you with your password.")

                            MyBase.addInput(oFrmElmt, "cEmail", True, "Email Address")
                            MyBase.addBind("cEmail", "user/email", "true()", "email")

                            MyBase.addSubmit(oFrmElmt, "", "Send Password", "ewSubmitReminder")
                            MyBase.Instance.InnerXml = "<user><email/></user>"
                        Else
                            oFrmElmt = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::group[1]")
                        End If

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            If MyBase.valid Then

                                'Get the data using either the email address or username dependant upon whether the application is
                                'congigured to store usernames as email addresses.
                                sValidResponse = moDbHelper.passwordReminder(goRequest("cEmail"))

                                If sValidResponse = "Your password has been emailed to you" Then
                                    valid = True
                                    'remove old form
                                    Dim oElmt As XmlElement
                                    For Each oElmt In oFrmElmt.SelectNodes("*")
                                        oFrmElmt.RemoveChild(oElmt)
                                    Next
                                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, sValidResponse, True)
                                Else
                                    valid = False
                                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Alert, sValidResponse, True)
                                End If
                            Else
                                valid = False
                            End If
                        End If

                        MyBase.addValues()
                        Return MyBase.moXformElmt

                    Catch ex As Exception
                        returnException(mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Function xFrmActivateAccount() As XmlElement
                    Dim oFrmElmt As XmlElement

                    'Dim sValidResponse As String
                    Dim cProcessInfo As String = ""
                    Try
                        'Find a user account with the right activation code.

                        'Change the account status and delete the activation code.
                        Dim oMembership As New Protean.Cms.Membership(myWeb)
                        'AddHandler oMembership.OnError, AddressOf OnComponentError

                        oMembership.ActivateAccount(moRequest("key"))

                        oFrmElmt = Me.xFrmUserLogon

                        Me.addNote("UserDetails", noteTypes.Hint, "Your account is now activated please logon", True)

                        'Update the user Xform to say "Thank you for activating your account please logon, Pre-populating the username"


                        Return MyBase.moXformElmt

                    Catch ex As Exception
                        returnException(mcModuleName, "xFrmActivateAccount", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Function xFrmResetAccount(Optional userId As Long = 0) As XmlElement

                    Dim oFrmElmt As XmlElement
                    Dim cSQL As String
                    Dim cResponse As String
                    Dim cProcessInfo As String = ""
                    Dim areEmailAddressesAllowed As Boolean
                    Dim cEmailAddress As String = ""
                    Dim dsUsers As DataSet
                    Dim nNumberOfUsers As Integer
                    Dim oUserDetails As DataRow
                    Dim cUsername As String
                    Dim isValidEmailAddress As Boolean
                    Dim formTitle As String
                    Try

                        'Does the configuration setting indicate that email addresses are allowed.
                        If LCase(myWeb.moConfig("EmailUsernames")) = "on" Then
                            areEmailAddressesAllowed = True
                        Else
                            areEmailAddressesAllowed = False
                        End If

                        If userId > 0 Then
                            'populate the user id in the form

                            cSQL = "SELECT cDirName, cDirEmail FROM tblDirectory WHERE cDirSchema = 'User' and nDirKey = '" & userId & "'"
                            dsUsers = myWeb.moDbHelper.GetDataSet(cSQL, "tblTemp")

                            oUserDetails = dsUsers.Tables(0).Rows(0)
                            cEmailAddress = oUserDetails("cDirName")
                            isValidEmailAddress = Tools.Xml.EmailAddressCheck(cEmailAddress)
                            formTitle = "<span class=""msg-1030"">Send account reset message to user.</span>"
                        Else
                            formTitle = "<span class=""msg-028"">Please enter your email address and we will email you with your password.</span>"
                        End If


                        Dim FormName As String = "ResetAccount"

                        'maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        If Not MyBase.load("/xforms/directory/" & FormName & ".xml", myWeb.maCommonFolders) Then
                            'If this does not load manually then build a form to do it.
                            GoTo BuildForm
                        Else
                            oFrmElmt = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::group[position()=1]")
                            GoTo Check
                        End If

BuildForm:

                        MyBase.NewFrm("ResetAccount")

                        MyBase.submission("ResetAccount", "", "post", "form_check(this)")

                        oFrmElmt = MyBase.addGroup(MyBase.moXformElmt, "ResetAccount", "", formTitle)

                        If userId > 0 Then
                            If isValidEmailAddress Then
                                MyBase.addInput(oFrmElmt, "cEmail", True, "Email address", "readonly")
                            Else
                                MyBase.addInput(oFrmElmt, "cEmail", True, "Username", "readonly")
                            End If
                        Else
                            MyBase.addInput(oFrmElmt, "cEmail", True, "Email address / Username")
                        End If
                        'check for legal chars in either email or username
                        MyBase.addBind("cEmail", "user/email", "true()", "format:^[a-zA-Z0-9._%+-@ ]*$")

                        MyBase.addSubmit(oFrmElmt, "", "Send Password", "ewAccountReset")

Check:

                        MyBase.Instance.InnerXml = "<user><email>" & cEmailAddress & "</email></user>"
                        MyBase.addValues()

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            If MyBase.valid Then

                                cUsername = Instance.SelectSingleNode("user/email").InnerText
                                'Returns true if cUsername is a valid email address.
                                isValidEmailAddress = Tools.Xml.EmailAddressCheck(cUsername)
                                If isValidEmailAddress Then
                                    If areEmailAddressesAllowed = True Then
                                        cSQL = "SELECT nDirKey FROM tblDirectory WHERE cDirSchema = 'User' and cDirEmail = '" & LCase(cUsername) & "'"
                                    Else
                                        cSQL = "SELECT nDirKey FROM tblDirectory WHERE cDirSchema = 'User' and cDirXml like '%<Email>" & LCase(cUsername) & "</Email>%'"
                                    End If
                                Else
                                    If areEmailAddressesAllowed = True Then
                                        cSQL = "SELECT nDirKey, cDirEmail FROM tblDirectory WHERE cDirSchema = 'User' and cDirName = '" & LCase(cUsername) & "'"
                                    Else
                                        cSQL = "SELECT nDirKey FROM tblDirectory WHERE cDirSchema = 'User' and cDirName = '" & LCase(cUsername) & "'"
                                    End If

                                End If

                                dsUsers = myWeb.moDbHelper.GetDataSet(cSQL, "tblTemp")
                                nNumberOfUsers = dsUsers.Tables(0).Rows.Count

                                If nNumberOfUsers = 0 Then
                                    cResponse = "<span class=""msg-1026"">There was a problem resetting this account, the username was not found. Please contact the website administrator</span>"
                                ElseIf nNumberOfUsers > 1 And areEmailAddressesAllowed = True Then
                                    cResponse = "<span class=""msg-1027"">There was a problem resetting this account, email address is ambiguous. Please contact the website administrator</span>"
                                Else
                                    oUserDetails = dsUsers.Tables(0).Rows(0)
                                    Dim nAcc As Integer = oUserDetails("nDirKey")

                                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))

                                    cResponse = oMembershipProv.Activities.ResetUserAcct(myWeb, nAcc)

                                End If

                                If Not String.IsNullOrEmpty(cResponse) Then
                                    MyBase.addNote(oFrmElmt, xForm.noteTypes.Hint, cResponse, True)
                                End If

                            Else
                                cResponse = "<span class=""msg-1028"">There was a problem resetting this account. Please contact the website administrator</span>"
                            End If 'endif MyBase.valid

                        End If ' endif MyBase.isSubmitted

                        MyBase.addValues()
                        Return MyBase.moXformElmt

                    Catch ex As Exception
                        returnException(mcModuleName, "xFrmResetAccount", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try

                End Function


                Public Function xFrmResetPassword(ByVal nAccount As Long) As XmlElement
                    Try

                        Dim moPolicy As XmlElement
                        Dim passwordClass As String = "required"
                        Dim passwordValidation As String = Nothing
                        Dim oPI1 As XmlElement
                        Dim oPI2 As XmlElement
                        Dim oSB As XmlElement
                        Dim oGrp As XmlElement
                        moPolicy = WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy")

                        If Not moPolicy Is Nothing Then
                            passwordClass = "required strongPassword"
                            passwordValidation = "strongPassword"
                        End If

                        Dim FormName As String = "ResetPassword"

                        'maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        If Not MyBase.load("/xforms/directory/" & FormName & ".xml", myWeb.maCommonFolders) Then
                            'If this does not load manually then build a form to do it.
                            GoTo BuildForm
                        Else
                            oGrp = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::group[position()=1]")
                            oPI1 = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::secret[@bind='cDirPassword']")
                            oPI2 = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::secret[@bind='cDirPassword2']")
                            oSB = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::submit[@submission='SetPassword']")
                            GoTo Check
                        End If

BuildForm:

                        MyBase.NewFrm("ResetPassword")

                        MyBase.Instance.InnerXml = "<Password><cDirPassword/><cDirPassword2/></Password>"

                        oGrp = MyBase.addGroup(MyBase.moXformElmt, "Password", , "Reset Password")
                        MyBase.submission("SetPassword", "", "POST")
                        oPI1 = MyBase.addSecret(oGrp, "cDirPassword", True, "Password", passwordClass)
                        MyBase.addBind("cDirPassword", "Password/cDirPassword", "true()", passwordValidation)
                        oPI2 = MyBase.addSecret(oGrp, "cDirPassword2", True, "Confirm Password", "required")
                        MyBase.addBind("cDirPassword2", "Password/cDirPassword2", "true()")
                        oSB = MyBase.addSubmit(oGrp, "SetPassword", "Set Password")

Check:

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            'any additonal validation goes here
                            'Passwords match?
                            If Len(goRequest("cDirPassword2")) > 0 Then
                                If goRequest("cDirPassword") <> goRequest("cDirPassword2") Then
                                    MyBase.valid = False
                                    MyBase.addNote("cDirPassword", xForm.noteTypes.Alert, "Passwords must match ")
                                End If
                            End If

                            If moPolicy Is Nothing Then
                                'Password policy?
                                If Len(MyBase.Instance.SelectSingleNode("Password/cDirPassword").InnerXml) < 4 Then
                                    MyBase.valid = False
                                    MyBase.addNote("cDirPassword", xForm.noteTypes.Alert, "Passwords must be 4 characters long ")
                                End If
                            End If

                            If MyBase.valid Then
                                Dim oMembership As New Protean.Cms.Membership(myWeb)

                                If Not oMembership.ReactivateAccount(nAccount, goRequest("cDirPassword")) Then
                                    MyBase.addNote("cDirPassword2", xForm.noteTypes.Alert, "There was a problem changing the password")
                                    MyBase.valid = False
                                Else
                                    MyBase.addNote(oGrp, xForm.noteTypes.Alert, "The password has been updated.")
                                    oPI1.ParentNode.RemoveChild(oPI1)
                                    oPI2.ParentNode.RemoveChild(oPI2)
                                    oSB.ParentNode.RemoveChild(oSB)

                                End If

                            End If
                        End If

                        MyBase.addValues()
                        Return MyBase.moXformElmt

                    Catch ex As Exception
                        returnException(mcModuleName, "xFrmResetPassword", ex, "", "", gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Function xFrmConfirmPassword(ByVal AccountHash As String) As XmlElement
                    Try
                        Dim oMembership As New Protean.Cms.Membership(myWeb)
                        Dim nUserId As Integer = oMembership.DecryptResetLink(goRequest("id"), AccountHash)

                        Return xFrmConfirmPassword(nUserId)

                    Catch ex As Exception
                        returnException(mcModuleName, "addInput", ex, "", "", gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Function xFrmConfirmPassword(ByVal nUserId As Long) As XmlElement
                    Try
                        Dim moPolicy As XmlElement
                        Dim passwordClass As String = "required"
                        Dim passwordValidation As String = Nothing
                        Dim oPI1 As XmlElement
                        Dim oPI2 As XmlElement
                        Dim oSB As XmlElement
                        Dim oGrp As XmlElement

                        moPolicy = WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy")

                        If Not moPolicy Is Nothing Then
                            passwordClass = "required strongPassword"
                            passwordValidation = "strongPassword"
                        End If

                        Dim FormName As String = "ConfirmPassword"

                        'maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                        If Not MyBase.load("/xforms/directory/" & FormName & ".xml", myWeb.maCommonFolders) Then
                            'If this does not load manually then build a form to do it.
                            GoTo BuildForm
                        Else
                            oGrp = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::group[position()=1]")
                            oPI1 = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::secret[@bind='cDirPassword']")
                            oPI2 = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::secret[@bind='cDirPassword2']")
                            oSB = MyBase.moXformElmt.SelectSingleNode("descendant-or-self::submit[@submission='SetPassword']")
                            GoTo Check
                        End If
BuildForm:

                        MyBase.NewFrm("ConfirmPassword")

                        MyBase.Instance.InnerXml = "<Password><cDirPassword/><cDirPassword2/></Password>"
                        Dim formTitle As String = "<span class=""trans-2031"">Enter new password</span>"
                        oGrp = MyBase.addGroup(MyBase.moXformElmt, "Password", , formTitle)
                        MyBase.submission("SetPassword", "", "POST")
                        oPI1 = MyBase.addSecret(oGrp, "cDirPassword", True, "Password", passwordClass)
                        MyBase.addBind("cDirPassword", "Password/cDirPassword", "true()", passwordValidation)
                        oPI2 = MyBase.addSecret(oGrp, "cDirPassword2", True, "Confirm Password", "required secret")
                        MyBase.addBind("cDirPassword2", "Password/cDirPassword2", "true()")
                        oSB = MyBase.addSubmit(oGrp, "SetPassword", "Set Password")

Check:
                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            'any additonal validation goes here
                            'Passwords match?
                            If Len(goRequest("cDirPassword2")) > 0 Then
                                If goRequest("cDirPassword") <> goRequest("cDirPassword2") Then
                                    MyBase.valid = False
                                    MyBase.addNote("cDirPassword", xForm.noteTypes.Alert, "Passwords must match ")
                                End If
                            End If

                            'Password policy?
                            If Len(MyBase.Instance.SelectSingleNode("Password/cDirPassword").InnerXml) < 4 Then
                                MyBase.valid = False
                                MyBase.addNote("cDirPassword", xForm.noteTypes.Alert, "Passwords must be 4 characters long ")
                            End If

                            Dim oMembership As New Protean.Cms.Membership(myWeb)

                            If Not moPolicy Is Nothing Then
                                'Password History?
                                If Not oMembership.CheckPasswordHistory(nUserId, goRequest("cDirPassword")) Then
                                    MyBase.valid = False
                                    MyBase.addNote("cDirPassword", xForm.noteTypes.Alert, "<span class=""msg-1020"">You cannot use a password you have used recently.</span>")
                                End If
                            End If

                            If MyBase.valid Then

                                Dim nAccount As Integer = nUserId
                                If nAccount = 0 Then
                                    MyBase.addNote("cDirPassword2", xForm.noteTypes.Alert, "This reset link has already been used")
                                    MyBase.valid = False
                                Else
                                    If Not oMembership.ReactivateAccount(nAccount, goRequest("cDirPassword")) Then
                                        MyBase.addNote("cDirPassword2", xForm.noteTypes.Alert, "There was an problem updating your account")
                                        MyBase.valid = False
                                    Else
                                        MyBase.addNote(oGrp, xForm.noteTypes.Alert, "Your password has been reset <a href=""/" & myWeb.moConfig("LogonRedirectPath") & """>click here</a> to login")
                                        oPI1.ParentNode.RemoveChild(oPI1)
                                        oPI2.ParentNode.RemoveChild(oPI2)
                                        oSB.ParentNode.RemoveChild(oSB)
                                        'delete failed logon attempts record
                                        Dim sSql As String = "delete from tblActivityLog where nActivityType = " & Cms.dbHelper.ActivityType.LogonInvalidPassword & " and nUserDirId=" & nAccount
                                        myWeb.moDbHelper.ExeProcessSql(sSql)

                                        If myWeb.mnUserId = 0 Then
                                            myWeb.mnUserId = nAccount
                                        End If
                                        ' myWeb.msRedirectOnEnd = myWeb.moConfig("LogonRedirectPath")
                                    End If
                                End If
                            End If
                        End If

                        MyBase.addValues()
                        Return MyBase.moXformElmt

                    Catch ex As Exception
                        returnException(mcModuleName, "addInput", ex, "", "", gbDebug)
                        Return Nothing
                    End Try
                End Function

                Public Overridable Function xFrmEditDirectoryItem(Optional ByVal id As Long = 0, Optional ByVal cDirectorySchemaName As String = "User", Optional ByVal parId As Long = 0, Optional ByVal cXformName As String = "", Optional ByVal FormXML As String = "") As XmlElement

                    Dim oGrpElmt As XmlElement
                    Dim cProcessInfo As String = ""
                    Dim cCurrentPassword As String = ""
                    Dim cCodeUsed As String = ""
                    Dim addNewitemToParId As Boolean = False
                    Dim moPolicy As XmlElement

                    Try

                        moPolicy = WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy")

                        If cXformName = "" Then cXformName = cDirectorySchemaName

                        ' ok lets load in an xform from the file location.
                        If FormXML = "" Then
                            If Not MyBase.load("/xforms/directory/" & cXformName & ".xml", myWeb.maCommonFolders) Then
                                ' load a default content xform if no alternative.

                            End If
                        Else
                            MyBase.NewFrm(cXformName)
                            MyBase.loadtext(FormXML)

                        End If

                        If id > 0 Then
                            MyBase.Instance.InnerXml = moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, id)
                            cCurrentPassword = Instance.SelectSingleNode("*/cDirPassword").InnerText
                        End If

                        cDirectorySchemaName = MyBase.Instance.SelectSingleNode("tblDirectory/cDirSchema").InnerText

                        If cDirectorySchemaName = "User" Then

                            'lets add the groups to the instance

                            oGrpElmt = moDbHelper.getGroupsInstance(id, parId)
                            MyBase.Instance.InsertAfter(oGrpElmt, MyBase.Instance.LastChild)
                            If goConfig("Subscriptions") = "on" Then
                                Dim oSub As New Cart.Subscriptions(myWeb)
                                oSub.AddSubscriptionToUserXML(MyBase.Instance, id)
                            End If

                            'now lets check our security, and if we are encrypted lets not show the password on edit.
                            If id > 0 Then
                                'RJP 7 Nov 2012. Added LCase as a precaution against people entering string in Protean.Cms.Config lowercase, i.e. md5.
                                If Not myWeb.moConfig("MembershipEncryption") Is Nothing Then
                                    If LCase(myWeb.moConfig("MembershipEncryption")).StartsWith("md5") Or LCase(myWeb.moConfig("MembershipEncryption")).StartsWith("sha") Then
                                        'Remove password (and confirm password) fields
                                        For Each oPwdNode As XmlElement In MyBase.moXformElmt.SelectNodes("/group/descendant-or-self::*[contains(@bind,'cDirPassword')]")
                                            oPwdNode.ParentNode.RemoveChild(oPwdNode)
                                        Next
                                    End If
                                End If

                            End If

                            'Is the membership email address secure.
                            If myWeb.moConfig("SecureMembershipAddress") <> "" And myWeb.mbAdminMode = False Then
                                Dim oSubElmt As XmlElement = MyBase.moXformElmt.SelectSingleNode("descendant::submission")
                                If myWeb.mcPagePath Is Nothing Then
                                    oSubElmt.SetAttribute("action", myWeb.moConfig("SecureMembershipAddress") & myWeb.mcOriginalURL)
                                Else
                                    oSubElmt.SetAttribute("action", myWeb.moConfig("SecureMembershipAddress") & myWeb.moConfig("ProjectPath") & "/" & myWeb.mcPagePath)
                                End If
                            End If
                        End If

                        If MyBase.isSubmitted Then
                            MyBase.updateInstanceFromRequest()
                            MyBase.validate()
                            'any additonal validation goes here
                            Select Case cDirectorySchemaName
                                Case "User", "UserMyAccount"
                                    If MyBase.valid = False Then
                                        MyBase.addNote("cDirName", xForm.noteTypes.Alert, MyBase.validationError)
                                    End If


                                    'Username exists?
                                    If MyBase.Instance.SelectSingleNode("*/cDirName").InnerXml <> MyBase.Instance.SelectSingleNode("*/cDirXml/User/Email").InnerXml Then
                                        If Not moDbHelper.checkUserUnique(MyBase.Instance.SelectSingleNode("*/cDirName").InnerXml, id) Then
                                            MyBase.valid = False
                                            MyBase.addNote("cDirName", xForm.noteTypes.Alert, "This username already exists please select another")
                                        End If
                                    End If

                                    'Email Exists?
                                    If Not moDbHelper.checkEmailUnique(MyBase.Instance.SelectSingleNode("*/cDirXml/User/Email").InnerXml, id) Then
                                        MyBase.valid = False
                                        If MyBase.Instance.SelectSingleNode("*/cDirName").InnerXml = MyBase.Instance.SelectSingleNode("*/cDirXml/User/Email").InnerXml Then
                                            MyBase.addNote("cEmail", xForm.noteTypes.Alert, "<span class=""msg-1031"">This email address already has an account, please use password reminder facility.</span>")
                                            MyBase.addNote("cDirName", xForm.noteTypes.Alert, "<span class=""msg-1031"">This email address already has an account, please use password reminder facility.</span>")
                                        Else
                                            MyBase.addNote("cEmail", xForm.noteTypes.Alert, "<span class=""msg-1031"">This email address already has an account, please use password reminder facility.</span>")
                                        End If

                                    End If

                                    'Only validate passwords if form contains the fields.
                                    If Not MyBase.moXformElmt.SelectSingleNode("/group/descendant-or-self::*[@bind='cDirPassword']") Is Nothing Then
                                        'Passwords match?
                                        If Len(goRequest("cDirPassword2")) > 0 Then
                                            If goRequest("cDirPassword") <> goRequest("cDirPassword2") Then
                                                MyBase.valid = False
                                                MyBase.addNote("cDirPassword", xForm.noteTypes.Alert, "Passwords must match ")
                                            End If
                                        End If

                                        If Not moPolicy Is Nothing Then
                                            'Password policy?
                                            If Len(MyBase.Instance.SelectSingleNode("*/cDirPassword").InnerXml) < 4 Then
                                                MyBase.valid = False
                                                MyBase.addNote("cDirPassword", xForm.noteTypes.Alert, "Passwords must be 4 characters long ")
                                            End If
                                        End If
                                    End If

                                    'Email exists...?

                                    'Membership codes
                                    cCodeUsed = validateMemberCode("*/RegistrationCode", "RegistrationCode")

                            End Select
                            If MyBase.valid Then

                                Dim cPassword As String = Instance.SelectSingleNode("*/cDirPassword").InnerText
                                Dim cClearPassword As String = cPassword
                                'RJP 7 Nov 2012. Added LCase to MembershipEncryption. Note leave the value below for md5Password hard coded as MD5.
                                If LCase(myWeb.moConfig("MembershipEncryption")) = "md5salt" Then
                                    Dim cSalt As String = Protean.Tools.Encryption.generateSalt
                                    Dim inputPassword As String = String.Concat(cSalt, cPassword) 'Take the users password and add the salt at the front
                                    Dim md5Password As String = Protean.Tools.Encryption.HashString(inputPassword, "md5", True) 'Md5 the marged string of the password and salt
                                    Dim resultPassword As String = String.Concat(md5Password, ":", cSalt) 'Adds the salt to the end of the hashed password
                                    cPassword = resultPassword 'Store the resultant password with salt in the database
                                Else
                                    cPassword = Protean.Tools.Encryption.HashString(cPassword, LCase(myWeb.moConfig("MembershipEncryption")), True) 'plain - md5 - sha1
                                End If
                                If Not cPassword = cCurrentPassword And Not cClearPassword = cCurrentPassword Then
                                    Instance.SelectSingleNode("*/cDirPassword").InnerText = cPassword
                                End If

                                If id > 0 Then

                                    moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, MyBase.Instance, id)
                                    If Not moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='EditContent' or @bind='EditContent']") Is Nothing Then
                                        MyBase.addNote("EditContent", xForm.noteTypes.Alert, "<span class=""msg-1010"">Your details have been updated.</span>", True)
                                    Else
                                        Dim oSubElmt As XmlElement = moXformElmt.SelectSingleNode("descendant-or-self::group[parent::Content][1]")
                                        If Not oSubElmt Is Nothing Then
                                            MyBase.addNote(oSubElmt, xForm.noteTypes.Alert, "<span class=""msg-1010"">Your details have been updated.</span>", True)
                                        End If
                                    End If
                                Else
                                    'add new
                                    id = moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Directory, MyBase.Instance)

                                    'update the instance with the id
                                    MyBase.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText = id
                                    addNewitemToParId = (parId > 0)
                                    MyBase.addNote("EditContent", xForm.noteTypes.Alert, "This user has been added.", True)

                                    'add addresses
                                    If Not MyBase.Instance.SelectSingleNode("tblCartContact") Is Nothing Then
                                        MyBase.Instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = id
                                        moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, MyBase.Instance)
                                    End If

                                    ' Save the member code, if applicable
                                    useMemberCode(cCodeUsed, id)

                                    ' If member codes were being applied then reconstruct the Group Instance.
                                    If gbMemberCodes And cCodeUsed <> "" Then
                                        oGrpElmt = moDbHelper.getGroupsInstance(id, parId)
                                        MyBase.Instance.ReplaceChild(oGrpElmt, MyBase.Instance.LastChild)
                                    End If

                                End If

                                'lets add the user to any groups
                                If cDirectorySchemaName = "User" And maintainMembershipsOnAdd Then
                                    maintainMembershipsFromXForm(id)

                                    'we want to ad the user to a specified group from a pick list of groups.
                                    Dim GroupsElmt As XmlElement = MyBase.Instance.SelectSingleNode("groups")
                                    If Not GroupsElmt Is Nothing Then
                                        If GroupsElmt.GetAttribute("addIds") <> "" Then
                                            Dim i As String
                                            For Each i In Split(GroupsElmt.GetAttribute("addIds"), ",")
                                                moDbHelper.maintainDirectoryRelation(CLng(i), id, False)
                                            Next
                                        End If
                                    End If

                                End If

                                If addNewitemToParId Then
                                    moDbHelper.maintainDirectoryRelation(parId, id, False)
                                End If

                            End If
                        End If

                        MyBase.addValues()
                        Return MyBase.moXformElmt

                    Catch ex As Exception
                        returnException(mcModuleName, "xFrmEditDirectoryItem", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                ''' <summary>
                '''     Maintains membership between userid and groups as found in xForm
                ''' </summary>
                ''' <param name="nUserId">The user id to be associated with</param>
                ''' <param name="cGroupNodeListXPath">The XPath from the xform instance to the group nodes.</param>
                ''' <remarks>Group nodes membership is indicated by a boolean attribute "isMember"</remarks>
                Public Sub maintainMembershipsFromXForm(ByVal nUserId As Integer, Optional ByVal cGroupNodeListXPath As String = "groups/group", Optional ByVal Email As String = Nothing, Optional addOnly As Boolean = False)
                    PerfMon.Log(mcModuleName, "maintainMembershipsFromXForm", "start")
                    Dim sSql As String = ""
                    Dim oDr As SqlDataReader
                    Dim userMembershipIds As New List(Of Integer)

                    Try
                        'get the users current memberships
                        sSql = "select * from tblDirectoryRelation where nDirChildId  = " & nUserId
                        oDr = moDbHelper.getDataReader(sSql)
                        While oDr.Read
                            userMembershipIds.Add(oDr("nDirParentId"))
                        End While

                        For Each oElmt As XmlElement In MyBase.Instance.SelectNodes(cGroupNodeListXPath)
                            'TS isLast forces an update everytime this loops not possible to tell if this will be the last time
                            Dim bIsLast As Boolean = True
                            'If oElmt.NextSibling Is Nothing Then bIsLast = True
                            If LCase(oElmt.GetAttribute("isMember")) = "true" Or LCase(oElmt.GetAttribute("isMember")) = "yes" Then
                                ' if user not in group
                                If Not userMembershipIds.Contains(CInt(oElmt.GetAttribute("id"))) Then
                                    moDbHelper.maintainDirectoryRelation(oElmt.GetAttribute("id"), nUserId, False, , , Email, oElmt.GetAttribute("name"), bIsLast)
                                End If

                            Else
                                ' if user is in group
                                If userMembershipIds.Contains(CInt(oElmt.GetAttribute("id"))) Then
                                    If addOnly = False Then
                                        moDbHelper.maintainDirectoryRelation(oElmt.GetAttribute("id"), nUserId, True, , , Email, oElmt.GetAttribute("name"), bIsLast)
                                    End If
                                End If
                            End If
                        Next


                        PerfMon.Log(mcModuleName, "maintainMembershipsFromXForm", "end")
                    Catch ex As Exception
                        returnException(mcModuleName, "maintainMembershipsFromXForm", ex, "", "", gbDebug)
                    End Try

                End Sub

                Public Overridable Function xFrmActivationCode(ByVal nUserId As Long, Optional ByVal cXformName As String = "ActivationCode", Optional ByVal cFormXml As String = "") As XmlElement


                    Dim cProcessInfo As String = ""
                    Dim cCodeUsed As String = ""
                    Try


                        ' Load the form
                        If gbMemberCodes Then


                            If cFormXml = "" Then
                                If Not MyBase.load("/xforms/directory/" & cXformName & ".xml", myWeb.maCommonFolders) Then


                                    ' No form.

                                End If
                            Else
                                MyBase.NewFrm(cXformName)
                                MyBase.loadtext(cFormXml)
                            End If

                            ' Check if submitted
                            If MyBase.isSubmitted Then
                                MyBase.updateInstanceFromRequest()
                                MyBase.validate()

                                Dim cCodeNode As String = "RegistrationCode"

                                ' Check if we're limiting this to a codeset
                                Dim zcCodeSet As String = ""
                                NodeState(MyBase.Instance, "//CodeSet", , , , , , zcCodeSet)

                                ' Validate the code
                                cCodeUsed = validateMemberCode("//" & cCodeNode, cCodeNode, zcCodeSet)

                                ' Invalidate if the user id and code are bad
                                If MyBase.valid And (Not (nUserId > 0) Or cCodeUsed = "") Then

                                    MyBase.valid = False
                                    MyBase.addNote(cCodeNode, noteTypes.Alert, "There was a problem using this code.  Please try another code, or contact the website team.")

                                End If

                                ' Prcess the valid form
                                If MyBase.valid Then

                                    ' Save the member code, if applicable
                                    useMemberCode(cCodeUsed, nUserId)

                                    ' Add an indication that the form succeeded.

                                    addElement(MyBase.model.SelectSingleNode("instance"), "formState", "success")

                                    ' Clear out the form
                                    'Dim oGroup As XmlElement = Nothing
                                    'If NodeState(MyBase.moXformElmt, "group", , , , oGroup) <> XmlNodeState.NotInstantiated Then
                                    '    oGroup.InnerXml = "<label>Activation Code successful</label><div>The activation code was applied successfully.</div>"
                                    '    oGroup.SetAttribute("class", "activationcodesuccess")
                                    'End If

                                End If

                            End If

                            ' Add in values
                            MyBase.addValues()
                            Return MyBase.moXformElmt

                        Else
                            Return Nothing
                        End If



                    Catch ex As Exception
                        returnException(mcModuleName, "xFrmActivationCode", ex, "", cProcessInfo, gbDebug)
                        Return Nothing
                    End Try
                End Function

                ''' <summary>
                ''' Validates a member code in an xform.
                ''' </summary>
                ''' <param name="cXPathToCode">The xpath to the code in the instance</param>
                ''' <param name="cBindOrReftoCode">The bind or ref of the node in the form</param>
                ''' <returns>String - returns the code if a code was entered and it is not a valid code</returns>
                ''' <remarks>
                ''' <para>If a code was entered and it's not valid, then the xform will be invalidated.</para>
                ''' <para>Note that if the xpath can't be found or a code has not been entered then the form will not be invalidated.</para>
                ''' <para>This requires MemberCodes to be turned on in web.config</para>
                '''</remarks> 
                Private Function validateMemberCode(ByVal cXPathToCode As String, ByVal cBindOrReftoCode As String, Optional ByVal cCodeSet As String = "") As String
                    Dim cCode As String = ""
                    Dim cReturnCode As String = ""
                    Try
                        ' Check:
                        ' - is member codes on
                        ' - have we got an xpath
                        ' - has a code been entered?
                        If gbMemberCodes _
                            AndAlso cXPathToCode <> "" _
                            AndAlso NodeState(MyBase.Instance, cXPathToCode, , , , , , cCode) = XmlNodeState.HasContents _
                            Then

                            If myWeb.moDbHelper.CheckCode(cCode, cCodeSet) Then
                                cReturnCode = cCode
                            Else
                                ' Invalidate the form
                                MyBase.valid = False
                                MyBase.addNote(cBindOrReftoCode, xForm.noteTypes.Alert, "Activation Code Incorrect")
                            End If

                        End If

                        Return cReturnCode

                    Catch ex As Exception
                        returnException(mcModuleName, "validateMemberCode", ex, "", "", gbDebug)
                        Return ""
                    End Try
                End Function

                ''' <summary>
                ''' Registers a user against a member code, and adds the user to the code memberships
                ''' </summary>
                ''' <param name="cCode">The code to use</param>
                ''' <param name="nUserId">The user to register the code against</param>
                Private Sub useMemberCode(ByVal cCode As String, ByVal nUserId As Long)
                    Try

                        If gbMemberCodes And cCode <> "" Then

                            ' Use the code
                            myWeb.moDbHelper.UseCode(cCode, nUserId)

                            ' Get the CSV list of directory items for membership
                            Dim cCodeCSVList As String = myWeb.moDbHelper.GetDataValue("SELECT tblCodes.cCodeGroups FROM tblCodes INNER JOIN tblCodes Child ON tblCodes.nCodeKey = Child.nCodeParentId WHERE (Child.cCode = '" & cCode & "')", , , "")

                            ' Process the List
                            For Each cDirId As String In cCodeCSVList.Split(",")
                                If IsNumeric(cDirId) Then moDbHelper.maintainDirectoryRelation(cDirId, nUserId, False, , )
                            Next

                        End If

                    Catch ex As Exception
                        returnException(mcModuleName, "useMemberCode", ex, "", "", gbDebug)
                    End Try
                End Sub

            End Class

            Public Class AdminProcess
                Inherits Cms.Admin

                Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
                Public Event OnErrorWithWeb(ByRef myweb As Protean.Cms, ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)


                Dim _oAdXfm As Protean.Providers.Payment.EonicProvider.AdminXForms

                Public Property oAdXfm() As Object
                    Set(ByVal value As Object)
                        _oAdXfm = value
                    End Set
                    Get
                        Return _oAdXfm
                    End Get
                End Property

                Sub New(ByRef aWeb As Cms)
                    MyBase.New(aWeb)
                End Sub

                Public Sub maintainUserInGroup(ByVal nUserId As Long, ByVal nGroupId As Long, ByVal remove As Boolean, Optional ByVal cUserEmail As String = Nothing, Optional ByVal cGroupName As String = Nothing)
                    PerfMon.Log("Messaging", "maintainUserInGroup")
                    Try

                        'do nothing this is a placeholder

                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "maintainUserInGroup", ex, ""))
                    End Try
                End Sub

            End Class


            Public Class Activities
                Private Const mcModuleName As String = "Providers.Membership.Eonic.Activities"

#Region "ErrorHandling"

                'for anything controlling web
                Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

                Protected Overridable Sub OnComponentError(ByRef myWeb As Protean.Cms, ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs) 'Handles moDBHelper.OnErrorwithweb, oSync.OnError, moCalendar.OnError
                    'deals with the error
                    Dim moDbHelper As Protean.Cms.dbHelper = myWeb.moDbHelper

                    returnException(e.ModuleName, e.ProcedureName, e.Exception, myWeb.mcEwSiteXsl, e.AddtionalInformation, gbDebug)
                    'close connection pooling
                    If Not moDbHelper Is Nothing Then
                        Try
                            moDbHelper.CloseConnection()
                        Catch ex As Exception

                        End Try
                    End If
                    'then raises a public event
                    RaiseEvent OnError(sender, e)
                End Sub

#End Region

                Public Overridable Function GetUserSessionId(ByRef myWeb As Protean.Base) As Long

                    Dim sProcessInfo As String = ""
                    Dim moSession As System.Web.SessionState.HttpSessionState = myWeb.moSession
                    Dim mnUserId As Integer = myWeb.mnUserId

                    Try
                        If moSession("nUserId") <> 0 Then
                            myWeb.mnUserId = moSession("nUserId")
                        Else
                            myWeb.mnUserId = 0
                        End If

                        Return myWeb.mnUserId

                    Catch ex As Exception
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserSessionId", ex, sProcessInfo))
                        Return Nothing
                    End Try
                End Function


                Public Overridable Function GetUserId(ByRef myWeb As Protean.Base) As String
                    PerfMon.Log("Web", "getUserId")
                    Dim sProcessInfo As String = ""
                    Dim sReturnValue As String = Nothing
                    Dim cLogonCmd As String = ""
                    Dim mnUserId As Integer = myWeb.mnUserId
                    Dim moSession As System.Web.SessionState.HttpSessionState = myWeb.moSession
                    Dim moRequest As System.Web.HttpRequest = myWeb.moRequest
                    Dim moResponse As System.Web.HttpResponse = myWeb.moResponse
                    Dim moConfig As System.Collections.Specialized.NameValueCollection = myWeb.moConfig
                    Dim moDbHelper As Protean.Cms.dbHelper = myWeb.moDbHelper
                    Dim sDomain As String = myWeb.moRequest.ServerVariables("HTTP_HOST")

                    Try
                        sProcessInfo = IIf(myWeb.moRequest.ServerVariables("HTTPS") = "on", "https://", "http://") & sDomain
                        If moSession("nUserId") = Nothing And mnUserId = 0 Then

                            'first lets check for a remember me cookie
                            Dim rememberMeMode As String = moConfig("RememberMeMode")
                            If Not moRequest.Cookies("RememberMeUserId") Is Nothing And rememberMeMode <> "KeepCookieAfterLogoff" And Not String.IsNullOrEmpty(rememberMeMode) Then
                                If IsNumeric(moRequest.Cookies("RememberMeUserId").Value) Then
                                    ' AG - MAJOR SECURITY FUBAR!!! Commenting out for now.
                                    ' mnUserId = moRequest.Cookies("RememberMeUserId").Value
                                End If
                            End If


                            ' If single user login is set we need to check if a cookie exists and reconnect if needs be
                            Dim oldSession As System.Web.HttpCookie = moRequest.Cookies("ewslock")
                            If gbSingleLoginSessionPerUser AndAlso oldSession IsNot Nothing Then


                                ' Find out what the last thing the user did (login-wise) was
                                If Not String.IsNullOrEmpty(oldSession.Value) Then

                                    ' We need to find a few things:
                                    ' 1. Find the last activity for the session id (within the timeout period)
                                    ' 2. Work out if the user id for that session id has had a more recent session
                                    ' 3. If not, then we can assume that the session is still alive and we need to update for this session.
                                    Dim lastSeenQuery As String = "SELECT nUserDirId FROM (SELECT TOP 1 nUserDirId,nACtivityType,cSessionId, (SELECT TOP 1 l2.cSessionId As sessionId FROM tblActivityLog l2 WHERE l2.nUserDirId = l.nUserDirId ORDER BY dDateTime DESC) As lastSessionForUser FROM tblActivityLog l " _
                                    & "WHERE cSessionId = " & dbHelper.SqlString(oldSession.Value) & " " _
                                    & "AND DATEDIFF(s,l.dDateTime,GETDATE()) < " & gnSingleLoginSessionTimeout & " " _
                                    & "ORDER BY dDateTime DESC) s WHERE s.cSessionId = s.lastSessionForUser AND s.nACtivityType <> " & dbHelper.ActivityType.Logoff

                                    Dim lastSeenUser As Integer = moDbHelper.GetDataValue(lastSeenQuery, , , 0)
                                    If lastSeenUser > 0 Then

                                        ' Reconnect with the new session ID
                                        mnUserId = lastSeenUser
                                        moResponse.Cookies("ewslock").Value = moSession.SessionID
                                        moDbHelper.logActivity(dbHelper.ActivityType.SessionReconnectFromCookie, mnUserId, 0, 0, oldSession.Value)

                                    End If
                                End If
                            End If
                        Else
                            If moSession("nUserId") = Nothing Or moSession("nUserId") = 0 Then
                                'this will get set on close
                                If IsNumeric(moSession("PreviewUser")) Then
                                    mnUserId = moSession("PreviewUser")
                                    myWeb.mbPreview = True
                                End If
                            Else
                                ' If there is a user set, we need to check if we are transferring over to a secure site,
                                ' to see if we should actually be logged off (which we can tell by looking at the cart order
                                ' based on the session id).
                                If gbCart AndAlso moRequest("refSessionId") <> "" Then

                                    Dim nCartUserId As Long = 0

                                    If Not moDbHelper Is Nothing Then
                                        nCartUserId = moDbHelper.GetDataValue("SELECT nCartUserDirId FROM tblCartOrder o where o.cCartSchemaName='Order' and o.cCartSessionId = '" & SqlFmt(moRequest("refSessionId")) & "'", , , 0)
                                    End If

                                    If nCartUserId <> moSession("nUserId") Then
                                        mnUserId = 0
                                    Else
                                        mnUserId = moSession("nUserId")
                                    End If
                                Else

                                    'feature to turn on preview mode with supplied user ID if token provided then this is happening in alternativeauthentication
                                    If myWeb.moRequest("ewCmd") = "PreviewOn" And IsNumeric(myWeb.moRequest("PreviewUser")) Then
                                        myWeb.moSession("PreviewUser") = myWeb.moRequest("PreviewUser")
                                    End If


                                    'lets finally set the user Id from the session
                                    If IsNumeric(moSession("PreviewUser")) Then
                                            If myWeb.moRequest("ewCmd") = "Normal" Or myWeb.moRequest("ewCmd") = "ExitPreview" Then
                                                'jump out of admin mode...
                                                myWeb.moSession("PreviewUser") = Nothing
                                                myWeb.mbPreview = False
                                            Else
                                                mnUserId = moSession("PreviewUser")
                                                myWeb.mbPreview = True
                                            End If
                                        Else
                                            mnUserId = moSession("nUserId")
                                        End If

                                    End If

                                End If
                        End If

                        Return mnUserId

                    Catch ex As Exception
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserId", ex, sProcessInfo))
                        Return Nothing
                    End Try
                End Function

                Public Overridable Sub SetUserId(ByRef myWeb As Protean.Cms)
                    PerfMon.Log("Web", "getUserId")
                    Dim sProcessInfo As String = ""
                    Dim sReturnValue As String = Nothing
                    Dim cLogonCmd As String = ""
                    Dim mnUserId As Integer = myWeb.mnUserId
                    Dim moSession As System.Web.SessionState.HttpSessionState = myWeb.moSession

                    Try
                        If Not moSession("nUserId") Is Nothing Then
                            If moSession("nUserId") = 0 Then
                                moSession("nUserId") = mnUserId
                            Else
                                If (moSession("nUserId") <> mnUserId) And CStr(moSession("PreviewUser")) = "" Then
                                    'reset to a different value
                                    moSession("nUserId") = mnUserId
                                End If
                                'have a user id so dont remove it
                            End If
                        Else
                            moSession("nUserId") = mnUserId
                        End If
                    Catch ex As Exception
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SetUserId", ex, sProcessInfo))
                    End Try
                End Sub

                Public Overridable Function GetUserXML(ByRef myWeb As Protean.Cms, Optional ByVal nUserId As Long = 0) As XmlElement
                    PerfMon.Log("Web", "GetUserXML")
                    Dim sProcessInfo As String = ""
                    Dim mnUserId As Integer = myWeb.mnUserId
                    Dim moDbHelper As Protean.Cms.dbHelper = myWeb.moDbHelper
                    Try
                        If nUserId = 0 Then nUserId = mnUserId

                        Return moDbHelper.GetUserXML(nUserId)

                    Catch ex As Exception
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserXml", ex, sProcessInfo))
                        Return Nothing
                    End Try

                End Function

                Public Overridable Function MembershipProcess(ByRef myWeb As Protean.Cms) As String
                    PerfMon.Log("Web", "MembershipProcess")
                    Dim sProcessInfo As String = ""
                    Dim sReturnValue As String = Nothing
                    Dim cLogonCmd As String = ""
                    Dim mnUserId As Integer = myWeb.mnUserId
                    Dim moSession As System.Web.SessionState.HttpSessionState = myWeb.moSession
                    Dim moRequest As System.Web.HttpRequest = myWeb.moRequest
                    Dim moConfig As System.Collections.Specialized.NameValueCollection = myWeb.moConfig
                    Dim moDbHelper As Protean.Cms.dbHelper = myWeb.moDbHelper

                    Try

                        'Dim adXfm As EonicProvider.AdminXForms = New EonicProvider.AdminXForms(myWeb)
                        Dim adXfm As Object = myWeb.getAdminXform()

                        adXfm.open(myWeb.moPageXml)

                        'logoff handler
                        If LCase(myWeb.moRequest("ewCmd")) = "logoff" And mnUserId <> 0 Then

                            If myWeb.moSession("ewCmd") <> "PreviewOn" Then
                                LogOffProcess(myWeb)
                            End If

                            'we are logging off so lets redirect
                            If Not myWeb.moConfig("BaseUrl") Is Nothing And Not myWeb.moConfig("BaseUrl") = "" Then
                                myWeb.msRedirectOnEnd = myWeb.moConfig("BaseUrl")
                            ElseIf Not myWeb.moConfig("RootPageId") Is Nothing And Not myWeb.moConfig("RootPageId") = "" Then
                                myWeb.msRedirectOnEnd = myWeb.moConfig("ProjectPath") & "/"
                            Else
                                myWeb.msRedirectOnEnd = myWeb.mcOriginalURL
                            End If
                            Dim oMembership As New Protean.Cms.Membership(myWeb)
                            oMembership.ProviderActions(myWeb, "logoffAction")
                            'BaseUrl
                            sReturnValue = "LogOff"

                        ElseIf myWeb.moRequest("ewCmd") = "CancelSubscription" Then

                            Dim oAdx As New Admin.AdminXforms(myWeb)
                            oAdx.moPageXML = myWeb.moPageXml

                            Dim oCSFrm As XmlElement = oAdx.xFrmConfirmCancelSubscription(myWeb.moRequest("nUserId"), myWeb.moRequest("nSubscriptionId"), mnUserId, myWeb.mbAdminMode)
                            If oCSFrm Is Nothing Then
                                myWeb.moPageXml.SelectSingleNode("/Page/@layout").InnerText = "My_Account"
                            Else
                                myWeb.moPageXml.SelectSingleNode("/Page/@layout").InnerText = "CancelSubscription"
                                myWeb.AddContentXml(oCSFrm)
                            End If
                        ElseIf myWeb.moRequest("ewCmd") = "AR" Then ' AccountReset
                            Dim cAccountHash As String = myWeb.moRequest("AI")
                            If Not cAccountHash = "" Then
                                Dim oXfmElmt As XmlElement = adXfm.xFrmConfirmPassword(cAccountHash)
                                myWeb.AddContentXml(oXfmElmt)
                                myWeb.moPageXml.DocumentElement.SetAttribute("layout", "Account_Reset")

                                If adXfm.valid = True Then

                                    sReturnValue = "LogOn"

                                    If gbSingleLoginSessionPerUser Then
                                        myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Logon, mnUserId, 0)
                                    Else
                                        myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logon, mnUserId, moSession.SessionID, Now, 0, 0, "")
                                    End If

                                    'Now we want to reload as permissions have changed
                                    If Not moSession Is Nothing Then
                                        If Not moSession("cLogonCmd") Is Nothing Then
                                            cLogonCmd = Split(moSession("cLogonCmd"), "=")(0)
                                            If myWeb.mcOriginalURL.Contains(cLogonCmd & "=") Then
                                                cLogonCmd = ""
                                            Else
                                                If myWeb.mcOriginalURL.Contains("=") Then
                                                    cLogonCmd = "&" & moSession("cLogonCmd")
                                                Else
                                                    cLogonCmd = "?" & moSession("cLogonCmd")
                                                End If
                                            End If
                                        End If
                                    End If

                                    If cLogonCmd <> "" Then
                                        myWeb.logonRedirect(cLogonCmd)
                                    End If

                                End If
                            End If

                        End If

                        If Not myWeb.moConfig("SecureMembershipAddress") = "" Then

                            Dim oMembership As New Protean.Cms.Membership(myWeb)
                            AddHandler oMembership.OnErrorWithWeb, AddressOf OnComponentError
                            oMembership.SecureMembershipProcess(sReturnValue)

                        End If

                        'display logon form for all pages if user is not logged on.
                        If mnUserId = 0 And (myWeb.moRequest("ewCmd") <> "passwordReminder" And myWeb.moRequest("ewCmd") <> "ActivateAccount" And myWeb.moRequest("ewCmd") <> "AR") Then

                            Dim oXfmElmt As XmlElement = adXfm.xFrmUserLogon()
                            Dim bAdditionalChecks As Boolean = False

                            If Not (adXfm.valid) Then
                                ' Call in additional authentication checks
                                If LCase(myWeb.moConfig("AlternativeAuthentication")) = "on" Then
                                    bAdditionalChecks = Me.AlternativeAuthentication(myWeb)
                                End If

                            End If

                            If adXfm.valid Or bAdditionalChecks Then
                                myWeb.moContentDetail = Nothing
                                mnUserId = myWeb.mnUserId
                                If Not myWeb.moSession Is Nothing Then myWeb.moSession("nUserId") = mnUserId
                                If myWeb.moRequest("cRemember") = "true" Then
                                    Dim oCookie As System.Web.HttpCookie = New System.Web.HttpCookie("RememberMe")
                                    oCookie.Value = mnUserId
                                    oCookie.Expires = DateAdd(DateInterval.Day, 60, Now())
                                    myWeb.moResponse.Cookies.Add(oCookie)
                                End If
                                sReturnValue = "LogOn"

                                If gbSingleLoginSessionPerUser Then
                                    myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Logon, mnUserId, 0)
                                Else
                                    myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logon, mnUserId, moSession.SessionID, Now, 0, 0, "")
                                End If

                                'Now we want to reload as permissions have changed
                                If Not moSession Is Nothing Then
                                    If Not moSession("cLogonCmd") Is Nothing Then
                                        cLogonCmd = Split(moSession("cLogonCmd"), "=")(0)
                                        If myWeb.mcOriginalURL.Contains(cLogonCmd & "=") Then
                                            cLogonCmd = ""
                                        Else
                                            If myWeb.mcOriginalURL.Contains("=") Then
                                                cLogonCmd = "&" & moSession("cLogonCmd")
                                            Else
                                                cLogonCmd = "?" & moSession("cLogonCmd")
                                            End If
                                        End If
                                    End If
                                End If

                                'LogonProviderOptions

                                ' ProviderActions(myWeb, "logonAction")

                                'do not cache
                                myWeb.bPageCache = False
                                myWeb.logonRedirect(cLogonCmd)
                            Else
                                myWeb.AddContentXml(oXfmElmt)
                                '  mnUserId = adXfm.mnUserId
                            End If
                        ElseIf moRequest("ewCmd") = "passwordReminder" Then

                            Dim oXfmElmt As XmlElement
                            Select Case LCase(moConfig("MembershipEncryption"))
                                Case "md5salt", "md5", "sha1", "sha256"
                                    oXfmElmt = adXfm.xFrmResetAccount()
                                Case Else
                                    oXfmElmt = adXfm.xFrmPasswordReminder()
                            End Select
                            myWeb.AddContentXml(oXfmElmt)

                        ElseIf moRequest("ewCmd") = "ActivateAccount" Then

                            Dim oMembership As New Protean.Cms.Membership(myWeb)
                            AddHandler oMembership.OnErrorWithWeb, AddressOf OnComponentError

                            Dim oXfmElmt As XmlElement
                            oXfmElmt = adXfm.xFrmActivateAccount()

                            myWeb.AddContentXml(oXfmElmt)

                        End If

                        Select Case moRequest("ewCmd")
                            Case "UserIntegrations"
                                myWeb.AddContentXml(adXfm.xFrmUserIntegrations(mnUserId, moRequest("ewCmd2")))
                                ' moContentDetail.AppendChild(adXfm.xFrmUserIntegrations(mnUserId, moRequest("ewCmd2")))
                                If adXfm.valid Then
                                    ' moContentDetail.RemoveAll()
                                    'clear the listDirectory cache
                                    myWeb.moDbHelper.clearDirectoryCache()
                                    'return to process flow
                                End If
                        End Select


                        'add the user logon details to the page xml.
                        If mnUserId <> 0 Then

                            myWeb.RefreshUserXML()

                            myWeb.GetUserXML(mnUserId)

                            'moPageXml.DocumentElement.AppendChild(moPageXml.ImportNode(GetUserXML().CloneNode(True), True))
                        End If

                        ' Site Redirection Process
                        If moConfig("SiteGroupRedirection") <> "" And mnUserId <> 0 Then
                            myWeb.SiteRedirection()
                        End If


                        'behaviour based on layout page (not required in V5 sites, this behaviour uses modules instead)
                        MembershipV4LayoutProcess(myWeb, adXfm)

                        LogSingleUserSession(myWeb)

                        If moRequest("ewEdit") <> "" Then
                            UserEditProcess(myWeb)
                        End If

                        Return sReturnValue

                    Catch ex As Exception
                        'returnException(mcModuleName, "MembershipLogon", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "MembershipProcess", ex, sProcessInfo))
                        Return Nothing
                    End Try
                End Function

                Public Overridable Function MembershipV4LayoutProcess(ByRef myWeb As Protean.Cms, adXfm As Object) As String
                    PerfMon.Log("Web", "MembershipProcess")
                    Dim sProcessInfo As String = ""
                    Dim sReturnValue As String = Nothing
                    Dim cLogonCmd As String = ""
                    Dim mnUserId As Integer = myWeb.mnUserId
                    Dim moSession As System.Web.SessionState.HttpSessionState = myWeb.moSession
                    Dim moRequest As System.Web.HttpRequest = myWeb.moRequest
                    Dim moConfig As System.Collections.Specialized.NameValueCollection = myWeb.moConfig
                    Dim moDbHelper As Protean.Cms.dbHelper = myWeb.moDbHelper
                    Dim clearUserId As Boolean = False

                    Try

                        'behaviour based on layout page
                        Select Case myWeb.moPageXml.SelectSingleNode("/Page/@layout").Value
                            Case "Logon_Register", "My_Account", "Register"
                                Dim oXfmElmt As XmlElement
                                ' If not in admin mode then base our choice on whether the user is logged in. 
                                ' If in Admin Mode, then present it as WYSIWYG
                                If (Not (myWeb.mbAdminMode) And mnUserId > 0) Or (myWeb.mbAdminMode And myWeb.moPageXml.SelectSingleNode("/Page/@layout").Value = "My_Account") Then
                                    Dim oContentForm As XmlElement = myWeb.moPageXml.SelectSingleNode("descendant-or-self::Content[@type='xform' and @name='UserMyAccount']")
                                    If oContentForm Is Nothing Then
                                        oXfmElmt = adXfm.xFrmEditDirectoryItem(mnUserId, "User", , "UserMyAccount")
                                    Else
                                        oXfmElmt = adXfm.xFrmEditDirectoryItem(mnUserId, "User", , "UserMyAccount", oContentForm.OuterXml)
                                        If Not myWeb.mbAdminMode Then oContentForm.ParentNode.RemoveChild(oContentForm)
                                    End If
                                    If adXfm.valid Then
                                        If sReturnValue = "" Then sReturnValue = "updateUser"
                                    End If


                                    myWeb.AddContentXml(oXfmElmt)
                                Else

                                    Dim oContentForm As XmlElement = myWeb.moPageXml.SelectSingleNode("descendant-or-self::Content[@type='xform' and @name='UserRegister']")
                                    If oContentForm Is Nothing Then
                                        oXfmElmt = adXfm.xFrmEditDirectoryItem(mnUserId, "User", , "UserRegister")
                                    Else
                                        oXfmElmt = adXfm.xFrmEditDirectoryItem(mnUserId, "User", , "UserRegister", oContentForm.OuterXml)
                                        If Not myWeb.mbAdminMode Then oContentForm.ParentNode.RemoveChild(oContentForm)
                                    End If

                                    ' ok if the user is valid we then need to handle what happens next.
                                    If adXfm.valid Then
                                        Dim bRedirect As Boolean = True
                                        Select Case moConfig("RegisterBehaviour")

                                            Case "validateByEmail"
                                                'don't redirect because we want to reuse this form
                                                bRedirect = False
                                                'say thanks for registering and update the form
                                                adXfm.addNote("EditContent", xForm.noteTypes.Hint, "Thanks for registering you have been sent an email with a link you must click to activate your account", True)

                                                'lets get the new userid from the instance
                                                mnUserId = adXfm.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText

                                                'first we set the user account to be pending
                                                myWeb.moDbHelper.setObjectStatus(dbHelper.objectTypes.Directory, dbHelper.Status.Pending, mnUserId)

                                                Dim oMembership As New Protean.Cms.Membership(myWeb)
                                                AddHandler oMembership.OnErrorWithWeb, AddressOf OnComponentError
                                                oMembership.AccountActivateLink(mnUserId)
                                                clearUserId = True
                                                myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Register, mnUserId, moSession.SessionID, Now, 0, 0, "Send Activation")


                                            Case Else ' Auto logon
                                                mnUserId = adXfm.Instance.SelectSingleNode("tblDirectory/nDirKey").InnerText
                                                If Not moSession Is Nothing Then
                                                    myWeb.mnUserId = mnUserId
                                                    moSession("nUserId") = mnUserId
                                                End If

                                                myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Register, mnUserId, moSession.SessionID, Now, 0, 0, "First Logon")

                                                'Now we want to reload as permissions have changed

                                                If Not moSession Is Nothing Then
                                                    If Not moSession("cLogonCmd") Is Nothing Then
                                                        cLogonCmd = Split(moSession("cLogonCmd"), "=")(0)
                                                        If myWeb.mcOriginalURL.Contains(cLogonCmd & "=") Then
                                                            cLogonCmd = ""
                                                        Else
                                                            If myWeb.mcOriginalURL.Contains("=") Then
                                                                cLogonCmd = "&" & moSession("cLogonCmd")
                                                            Else
                                                                cLogonCmd = "?" & moSession("cLogonCmd")
                                                            End If
                                                        End If
                                                    End If
                                                End If

                                                moSession("RedirectReason") = "registration"
                                                myWeb.bRedirectStarted = True ' This acts as a local suppressant allowing for the sessio to pass through to the redirected page
                                                myWeb.logonRedirect(cLogonCmd)
                                                bRedirect = False

                                        End Select

                                        'send registration confirmation
                                        Dim xsltPath As String = "/xsl/email/registration.xsl"

                                        If IO.File.Exists(goServer.MapPath(xsltPath)) Then
                                            Dim oUserElmt As XmlElement = myWeb.moDbHelper.GetUserXML(mnUserId)
                                            If clearUserId Then mnUserId = 0 ' clear user Id so we don't stay logged on
                                            Dim oElmtPwd As XmlElement = myWeb.moPageXml.CreateElement("Password")
                                            oElmtPwd.InnerText = moRequest("cDirPassword")
                                            oUserElmt.AppendChild(oElmtPwd)

                                            Dim oUserEmail As XmlElement = oUserElmt.SelectSingleNode("Email")
                                            Dim fromName As String = moConfig("SiteAdminName")
                                            Dim fromEmail As String = moConfig("SiteAdminEmail")
                                            Dim recipientEmail As String = ""
                                            If Not oUserEmail Is Nothing Then recipientEmail = oUserEmail.InnerText
                                            Dim SubjectLine As String = "Your Registration Details"
                                            Dim oMsg As Protean.Messaging = New Protean.Messaging
                                            'send an email to the new registrant
                                            If Not recipientEmail = "" Then sProcessInfo = oMsg.emailer(oUserElmt, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, "Message Sent", "Message Failed")
                                            'send an email to the webadmin
                                            recipientEmail = moConfig("SiteAdminEmail")
                                            If IO.File.Exists(goServer.MapPath(moConfig("ProjectPath") & "/xsl/email/registrationAlert.xsl")) Then
                                                sProcessInfo = oMsg.emailer(oUserElmt, moConfig("ProjectPath") & "/xsl/email/registrationAlert.xsl", "New User", recipientEmail, fromEmail, SubjectLine, "Message Sent", "Message Failed")
                                            End If
                                            oMsg = Nothing
                                        End If

                                        'redirect to this page or alternative page.
                                        If bRedirect Then
                                            myWeb.msRedirectOnEnd = myWeb.mcOriginalURL
                                        End If

                                        sReturnValue = "newUser"
                                    Else
                                        myWeb.AddContentXml(oXfmElmt)
                                    End If
                                End If

                            Case "Password_Reminder"

                                Dim oXfmElmt As XmlElement
                                If moConfig("MembershipEncryption") = "" Then

                                    oXfmElmt = adXfm.xFrmPasswordReminder()

                                Else
                                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                                    Dim oAdXfm As Object = oMembershipProv.AdminXforms
                                    oXfmElmt = oAdXfm.xFrmResetAccount()

                                End If
                                myWeb.AddContentXml(oXfmElmt)

                            Case "Password_Change"

                                Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                                Dim oAdXfm As Object = oMembershipProv.AdminXforms

                                Dim oXfmElmt As XmlElement
                                oXfmElmt = oAdXfm.xFrmResetPassword(mnUserId)
                                myWeb.AddContentXml(oXfmElmt)

                            Case "Activation_Code"

                                If Not myWeb.mbAdminMode Then
                                    Dim oXfmElmt As XmlElement = Nothing
                                    Dim cExistingFormXml As String = ""
                                    'For Each ocNode In moPageXml.SelectNodes("/Page/Contents/Content[@type='xform' and model/submission/@SOAPAction!='']")

                                    ' Look for activation code xforms
                                    If Tools.Xml.NodeState(myWeb.moPageXml.DocumentElement, "Contents/Content[@type='xform' and @name='ActivationCode']", , , , oXfmElmt) <> Tools.Xml.XmlNodeState.NotInstantiated Then
                                        oXfmElmt.ParentNode.RemoveChild(oXfmElmt)
                                        cExistingFormXml = oXfmElmt.OuterXml
                                    End If

                                    oXfmElmt = adXfm.xFrmActivationCode(mnUserId, , cExistingFormXml)
                                    If Not (oXfmElmt) Is Nothing Then
                                        myWeb.AddContentXml(oXfmElmt)

                                        ' If the form has been successful, then check for a redirect
                                        ' If the redirectpage is a number then continue
                                        If Not (oXfmElmt.SelectSingleNode("//instance/formState[node()='success']") Is Nothing) Then

                                            ' Activation was succesful, let's prepare the redirect

                                            ' Clear the cache.
                                            Dim cSql As String = "DELETE dbo.tblXmlCache " _
                                                    & " WHERE cCacheSessionID = '" & moSession.SessionID & "' " _
                                                    & "         AND nCacheDirId = " & Protean.SqlFmt(mnUserId)
                                            myWeb.moDbHelper.ExeProcessSqlorIgnore(cSql)

                                            ' Check if the redirect is another page or just redirect to the current url
                                            If Not (oXfmElmt.SelectSingleNode("//instance/RedirectPage[number(.)=number(.)]") Is Nothing) Then
                                                myWeb.msRedirectOnEnd = moConfig("ProjectPath") & "/?pgid=" & adXfm.Instance.SelectSingleNode("RedirectPage").InnerText
                                            Else
                                                myWeb.msRedirectOnEnd = myWeb.mcOriginalURL
                                            End If
                                            myWeb.bRedirectStarted = True
                                            moSession("RedirectReason") = "activation"
                                        End If
                                    End If
                                End If



                            Case "User_Contact"
                                If mnUserId > 0 Then
                                    Select Case moRequest("ewCmd")
                                        Case "addContact", "editContact"
                                            Dim oXfmElmt As XmlElement = adXfm.xFrmEditDirectoryContact(moRequest("id"), mnUserId)
                                            If Not adXfm.valid Then
                                                myWeb.AddContentXml(oXfmElmt)
                                            Else
                                                myWeb.RefreshUserXML()
                                            End If
                                        Case "delContact"
                                            Dim oContactElmt As XmlElement
                                            For Each oContactElmt In myWeb.GetUserXML(mnUserId).SelectNodes("descendant-or-self::Contact")
                                                Dim oId As XmlElement = oContactElmt.SelectSingleNode("nContactKey")
                                                If Not oId Is Nothing Then
                                                    If oId.InnerText = moRequest("id") Then
                                                        moDbHelper.DeleteObject(dbHelper.objectTypes.CartContact, moRequest("id"))
                                                        myWeb.RefreshUserXML()
                                                    End If
                                                End If
                                            Next
                                    End Select
                                End If

                        End Select
                    Catch ex As Exception
                        'returnException(mcModuleName, "MembershipLogon", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "MembershipV4LayoutProcess", ex, sProcessInfo))
                        Return Nothing
                    End Try
                End Function

                ''' <summary>
                ''' If in Single Session per Session mode this will log an activity indicating that the user is still in session.
                ''' </summary>
                ''' <remarks>It is called in EonicWeb but has been extracted so that it may be called by lightweight EonicWeb calls (e.g. ajax calls)</remarks>

                Public Sub LogSingleUserSession()
                    'Do Nothing
                End Sub



                Public Sub LogSingleUserSession(ByRef myWeb As Protean.Cms)

                    Dim mnUserId As Integer = myWeb.mnUserId
                    Dim moSession As System.Web.SessionState.HttpSessionState = myWeb.moSession
                    Dim moRequest As System.Web.HttpRequest = myWeb.moRequest
                    Dim moConfig As System.Collections.Specialized.NameValueCollection = myWeb.moConfig
                    Dim moDbHelper As Protean.Cms.dbHelper = myWeb.moDbHelper
                    Dim moResponse As System.Web.HttpResponse = myWeb.moResponse

                    Try
                        ' If logged on and in single login per user mode, log a session continuation flag.
                        If gbSingleLoginSessionPerUser And mnUserId > 0 And moSession IsNot Nothing Then
                            ' Log the session
                            moDbHelper.logActivity(dbHelper.ActivityType.SessionContinuation, mnUserId, 0, 0, 0, "", True)

                            ' Add a cookie / update it.
                            Dim cookieLock As System.Web.HttpCookie = moRequest.Cookies("ewslock")
                            If cookieLock Is Nothing Then
                                cookieLock = New System.Web.HttpCookie("ewslock")
                            End If
                            cookieLock.Expires = Now.AddSeconds(gnSingleLoginSessionTimeout)
                            cookieLock.Value = moSession.SessionID
                            moResponse.Cookies.Add(cookieLock)
                        End If

                    Catch ex As Exception
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "LogSingleUserSession", ex, ""))
                    End Try
                End Sub

                Public Overridable Function AlternativeAuthentication(ByRef myWeb As Protean.Cms) As Boolean

                    PerfMon.Log("Web", "AlternativeAuthentication")


                    Dim cProcessInfo As String = ""
                    Dim bCheck As Boolean = False
                    Dim cToken As String = ""
                    Dim cKey As String = ""
                    Dim cDecrypted As String = ""
                    Dim nReturnId As Integer

                    Dim mnUserId As Integer = myWeb.mnUserId
                    Dim moSession As System.Web.SessionState.HttpSessionState = myWeb.moSession
                    Dim moRequest As System.Web.HttpRequest = myWeb.moRequest
                    Dim moConfig As System.Collections.Specialized.NameValueCollection = myWeb.moConfig
                    Dim moDbHelper As Protean.Cms.dbHelper = myWeb.moDbHelper


                    Try

                        ' Look for the RC4 token
                        If moRequest("token") <> "" And moConfig("AlternativeAuthenticationKey") <> "" Then

                            cProcessInfo = "IP Address Checking"

                            Dim cIPList As String = CStr(moConfig("AlternativeAuthenticationIPList"))

                            If cIPList = "" OrElse Tools.Text.IsIPAddressInList(moRequest.UserHostAddress, cIPList) Then

                                cProcessInfo = "Decrypting token"
                                ' Dim oEnc As New Protean.Tools.Encryption.RC4()

                                cToken = moRequest("token")
                                cKey = moConfig("AlternativeAuthenticationKey")

                                ' There are two accepted formats to receive:
                                '  1. Email address
                                '  2. User ID

                                cDecrypted = Trim(Tools.Encryption.RC4.Decrypt(cToken, cKey))

                                If Tools.Text.IsEmail(cDecrypted) Then

                                    ' Authentication is by way of e-mail address
                                    cProcessInfo = "Email authenctication: Retrieving user for email: " & cDecrypted
                                    ' Get the user id based on the e-mail address
                                    nReturnId = moDbHelper.GetUserIDFromEmail(cDecrypted)

                                    If nReturnId > 0 Then
                                        bCheck = True
                                        mnUserId = nReturnId
                                        myWeb.mnUserId = mnUserId
                                        If moRequest("ewCmd") = "PreviewOn" Then
                                            moSession("adminMode") = "true"
                                            moSession("ewCmd") = "PreviewOn"
                                            myWeb.mbAdminMode = True
                                            myWeb.mbPreview = True
                                            myWeb.moSession("PreviewUser") = "0"
                                        End If

                                    End If

                                    ElseIf IsNumeric(cDecrypted) AndAlso CInt(cDecrypted) > 0 Then

                                    ' Authentication is by way of user ID
                                    cProcessInfo = "User ID Authentication: " & cDecrypted
                                    ' Get the user id based on the e-mail address
                                    bCheck = moDbHelper.IsValidUser(CInt(cDecrypted))
                                    If bCheck Then mnUserId = CInt(cDecrypted)

                                End If

                            End If

                        End If

                        Return bCheck

                    Catch ex As Exception
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AlternativeAuthentication", ex, cProcessInfo))
                        Return False
                    End Try

                End Function

                Public Overridable Sub LogOffProcess(ByRef myWeb As Protean.Cms)
                    PerfMon.Log("Web", "LogOffProcess")
                    Dim cProcessInfo As String = ""

                    Dim mnUserId As Integer = myWeb.mnUserId
                    Dim moSession As System.Web.SessionState.HttpSessionState = myWeb.moSession
                    Dim moRequest As System.Web.HttpRequest = myWeb.moRequest
                    Dim moConfig As System.Collections.Specialized.NameValueCollection = myWeb.moConfig
                    Dim moDbHelper As Protean.Cms.dbHelper = myWeb.moDbHelper
                    Dim moResponse As System.Web.HttpResponse = myWeb.moResponse

                    Try

                        cProcessInfo = "Commit to Log"
                        If gbSingleLoginSessionPerUser Then
                            moDbHelper.logActivity(dbHelper.ActivityType.Logoff, mnUserId, 0)
                            If moRequest.Cookies("ewslock") IsNot Nothing Then
                                moResponse.Cookies("ewslock").Expires = DateTime.Now.AddDays(-1)
                            End If
                        Else
                            moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logoff, mnUserId, moSession.SessionID, Now, 0, 0, "")
                        End If


                        ' Call this BEFORE clearing the user ID.
                        cProcessInfo = "Clear Site Stucture"
                        moDbHelper.clearStructureCacheUser()

                        ' Clear the user ID.
                        mnUserId = 0
                        myWeb.mnUserId = 0

                        ' Clear the cart
                        If Not moSession Is Nothing AndAlso gbCart Then
                            Dim cSql As String = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where (cCartSessionId = '" & moSession.SessionID & "' and cCartSessionId <> '')"
                            moDbHelper.ExeProcessSql(cSql)
                        End If

                        If Not moSession Is Nothing Then
                            cProcessInfo = "Abandon Session"
                            ' AJG : Question - why does this not clear the Session ID?
                            moSession("nEwUserId") = Nothing
                            moSession("nUserId") = Nothing
                            moSession.Abandon()
                        End If

                        If moConfig("RememberMeMode") <> "KeepCookieAfterLogoff" Then
                            cProcessInfo = "Clear Cookies"
                            moResponse.Cookies("RememberMeUserName").Expires = DateTime.Now.AddDays(-1)
                            moResponse.Cookies("RememberMeUserId").Expires = DateTime.Now.AddDays(-1)
                        End If

                    Catch ex As Exception
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "LogOffProcess", ex, cProcessInfo))
                    End Try

                End Sub

                Public Overridable Function UserEditProcess(ByRef myWeb As Protean.Cms) As String
                    PerfMon.Log("Web", "UserEditProcess")
                    Dim sProcessInfo As String = ""
                    Dim sReturnValue As String = Nothing
                    Try


                        Return Nothing

                    Catch ex As Exception
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "UserEditProcess", ex, sProcessInfo))
                        Return Nothing
                    End Try
                End Function

                Public Overridable Function ResetUserAcct(ByRef myWeb As Protean.Cms, ByVal nUserId As Integer) As String
                    PerfMon.Log("Web", "ResetUserAcct")
                    Dim sProcessInfo As String = ""
                    Dim sReturnValue As String = Nothing
                    Try

                        ' Get the user XML
                        Dim oUserXml As XmlElement = myWeb.moDbHelper.GetUserXML(nUserId, False)

                        If oUserXml Is Nothing Then
                            sReturnValue = "<span class=""msg-1028"">There was a problem resetting this account. Please contact the website administrator</span>"
                        Else
                            ' Check the xsl
                            Dim userEmail As String
                            'If areEmailAddressesAllowed = True And Not (isValidEmailAddress) Then
                            '    userEmail = oUserDetails("cDirEmail")
                            'Else
                            userEmail = oUserXml.SelectSingleNode("Email").InnerText
                            'End If

                            Dim oMembership As New Protean.Cms.Membership(myWeb)
                            Dim oEmailDoc As New XmlDocument
                            oEmailDoc.AppendChild(oEmailDoc.CreateElement("AccountReset"))
                            oEmailDoc.DocumentElement.AppendChild(oEmailDoc.ImportNode(oUserXml, True))
                            oEmailDoc.DocumentElement.SetAttribute("Link", oMembership.AccountResetLink(nUserId))
                            oEmailDoc.DocumentElement.SetAttribute("Url", myWeb.mcOriginalURL)
                            oEmailDoc.DocumentElement.SetAttribute("logonRedirect", myWeb.moSession("LogonRedirectId"))
                            oEmailDoc.DocumentElement.SetAttribute("lang", myWeb.mcPageLanguage)
                            oEmailDoc.DocumentElement.SetAttribute("translang", myWeb.mcPreferredLanguage)

                            Dim oMessage As New Protean.Messaging

                            Dim fs As fsHelper = New fsHelper()
                            Dim path As String = fs.FindFilePathInCommonFolders("/xsl/Email/passwordReset.xsl", myWeb.maCommonFolders)

                            sReturnValue = oMessage.emailer(oEmailDoc.DocumentElement, path, "", myWeb.moConfig("SiteAdminEmail"), userEmail, "Account Reset ")
                            sReturnValue = IIf(sReturnValue = "Message Sent", "<span class=""msg-1035"">" & sReturnValue & " to </span>" & userEmail, "")

                        End If 'endif oUserXml Is Nothing


                        Return sReturnValue

                    Catch ex As Exception
                        OnComponentError(myWeb, Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ResetUserAcct", ex, sProcessInfo))
                        Return Nothing
                    End Try
                End Function



                Public Sub New()

                End Sub
            End Class
        End Class

    End Namespace
End Namespace
