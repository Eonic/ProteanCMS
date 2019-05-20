Option Strict Off
Option Explicit On

Imports System.Xml
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Reflection
Imports System.Collections.Generic


Partial Public Class Cms
    Public Class Membership

#Region "Declarations"

        Dim myWeb As Protean.Cms
        Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        Public Event OnErrorWithWeb(ByRef myweb As Protean.Cms, ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        Private Const mcModuleName As String = "Eonic.EonicWeb.Membership.Membership"

#End Region

#Region "Public Procedures"

        Public Sub New(ByRef oWeb As Protean.Cms)
            Try
                myWeb = oWeb
            Catch ex As Exception

            End Try
        End Sub

        Public Function AccountResetLink(ByVal AccountID As Integer) As String
            Try
                'RJP 7 Nov 2012. Added LCase to MembershipEncryption.
                Dim cLink As String = Trim(Protean.Tools.Encryption.HashString(UCase(Now()), LCase(myWeb.moConfig("MembershipEncryption")), True))
                Dim cSQL As String = "UPDATE tblDirectory SET cDirPassword = '" & cLink & "' WHERE nDirKey = " & AccountID
                cLink = Protean.Tools.Text.AscString(cLink)
                Debug.WriteLine(cLink)
                If IsNumeric(myWeb.moDbHelper.ExeProcessSql(cSQL)) Then
                    Return cLink
                Else
                    Return ""
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AccountResetLink", ex, ""))
                Return ""
            End Try
        End Function

        ''' <summary>
        ''' 'We need to activate the link create a hash, We store the activation hash in the UserXML
        ''' </summary>
        ''' <param name="AccountID"></param>
        ''' <returns></returns>
        ''' <remarks>Cannot store in password field because we need this, We could store in the audit description, allthough don't think this is indexed. Storing in the userXml would be inefficient to search however it could be useful because it is contained in the email</remarks>

        Public Function AccountActivateLink(ByVal AccountID As Integer) As String
            Try
                Dim oGuid As System.Guid = System.Guid.NewGuid()

                Dim cLink As String = oGuid.ToString

                Dim oUserXml As New XmlDocument
                oUserXml.AppendChild(oUserXml.CreateElement("instance"))
                oUserXml.DocumentElement.InnerXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, AccountID)
                Dim oUserElmt As XmlElement = oUserXml.DocumentElement.SelectSingleNode("tblDirectory/cDirXml/User")
                Dim ActivationKeyElmt As XmlElement = oUserXml.CreateElement("ActivationKey")

                ActivationKeyElmt.InnerText = cLink

                oUserElmt.AppendChild(ActivationKeyElmt)
                myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Directory, oUserXml.DocumentElement, AccountID)

                Return cLink


            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AccountActivateLink", ex, ""))
                Return ""
            End Try
        End Function

        Public Function DecryptResetLink(ByVal AccountID As Integer, ByVal EncryptedString As String) As Integer
            Try
                EncryptedString = Protean.Tools.Text.DeAscString(EncryptedString)
                Dim cSQL As String = "SELECT tblDirectory.nDirKey FROM tblDirectory INNER JOIN tblAudit ON tblDirectory.nAuditId = tblAudit.nAuditKey WHERE cDirPassword = '" & EncryptedString & "' AND nDirKey = " & AccountID
                Return myWeb.moDbHelper.GetDataValue(cSQL, , , 0)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "DecryptResetLink", ex, ""))
                Return 0
            End Try
        End Function

        Public Function CheckPasswordHistory(ByVal AccountID As Integer, ByVal cPassword As String) As Boolean
            Dim odr As SqlDataReader
            Dim valid As Boolean = True
            Try

                If LCase(myWeb.moConfig("MembershipEncryption")) = "md5salt" Then
                    Dim cSalt As String = Protean.Tools.Encryption.generateSalt
                    Dim inputPassword As String = String.Concat(cSalt, cPassword) 'Take the users password and add the salt at the front
                    'Note leave the value below hard coded md5.
                    Dim md5Password As String = Protean.Tools.Encryption.HashString(inputPassword, "md5", True) 'Md5 the marged string of the password and salt
                    Dim resultPassword As String = String.Concat(md5Password, ":", cSalt) 'Adds the salt to the end of the hashed password
                    cPassword = resultPassword 'Store the resultant password with salt in the database
                Else

                    cPassword = Protean.Tools.Encryption.HashString(cPassword, LCase(myWeb.moConfig("MembershipEncryption")), True) 'plain - md5 - sha1

                End If

                'ensure the password is XML safe
                Dim oConvDoc As New XmlDocument
                Dim oConvElmt As XmlElement = oConvDoc.CreateElement("PW")
                oConvElmt.InnerText = cPassword
                cPassword = oConvElmt.InnerXml

                Dim moPolicy As XmlElement
                moPolicy = WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy")
                Dim nHistoricPasswordCount As Integer = CInt("0" & moPolicy.FirstChild.SelectSingleNode("blockHistoricPassword").InnerText)

                Dim sSql2 As String = "select cActivityDetail as password, dDatetime from tblActivityLog where nActivityType=" & dbHelper.ActivityType.HistoricPassword & " and nUserDirId = " & AccountID _
                & " Order By dDateTime Desc"

                odr = myWeb.moDbHelper.getDataReader(sSql2)
                While odr.Read
                    If nHistoricPasswordCount > 0 And cPassword = odr("password") Then
                        valid = False
                    End If
                    nHistoricPasswordCount = nHistoricPasswordCount - 1
                End While

                Return valid

            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CheckPasswordHistory", ex, ""))
                Return False
            End Try
        End Function



        Public Function ReactivateAccount(ByVal AccountID As Integer, ByVal cPassword As String) As Boolean
            Try

                ' cPassword = Protean.Tools.Encryption.HashString(cPassword, myWeb.moConfig("MembershipEncryption"), True)

                'RJP 7 Nov 2012. Added LCase to MembershipEncryption. Note leave the value below for md5Password hard coded as md5.
                If LCase(myWeb.moConfig("MembershipEncryption")) = "md5salt" Then
                    Dim cSalt As String = Protean.Tools.Encryption.generateSalt
                    Dim inputPassword As String = String.Concat(cSalt, cPassword) 'Take the users password and add the salt at the front
                    'Note leave the value below hard coded md5.
                    Dim md5Password As String = Protean.Tools.Encryption.HashString(inputPassword, "md5", True) 'Md5 the marged string of the password and salt
                    Dim resultPassword As String = String.Concat(md5Password, ":", cSalt) 'Adds the salt to the end of the hashed password
                    cPassword = resultPassword 'Store the resultant password with salt in the database
                Else
                    cPassword = Protean.Tools.Encryption.HashString(cPassword, LCase(myWeb.moConfig("MembershipEncryption")), True) 'plain - md5 - sha1
                End If

                'ensure the password is XML safe
                Dim oConvDoc As New XmlDocument
                Dim oConvElmt As XmlElement = oConvDoc.CreateElement("PW")
                oConvElmt.InnerText = cPassword
                cPassword = oConvElmt.InnerXml

                Dim moPolicy As XmlElement
                moPolicy = WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy")
                Dim nHistoricPasswordCount As Integer = CInt("0" & moPolicy.FirstChild.SelectSingleNode("blockHistoricPassword").InnerText)
                If nHistoricPasswordCount > 0 Then
                    myWeb.moDbHelper.logActivity(dbHelper.ActivityType.HistoricPassword, AccountID, 0, , cPassword)
                End If

                Dim cSQL As String = "UPDATE tblDirectory SET cDirPassword = '" & cPassword & "' WHERE nDirKey = " & AccountID
                If myWeb.moDbHelper.ExeProcessSql(cSQL) > 0 Then
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ReactivateAccount", ex, ""))
                Return False
            End Try
        End Function

        Public Function ActivateAccount(ByVal cLink As String) As Boolean
            Try
                Dim userId As Long

                'lets get the userId form the hash supplied
                Dim cSQL As String = "SELECT tblDirectory.nDirKey FROM tblDirectory INNER JOIN tblAudit ON tblDirectory.nAuditId = tblAudit.nAuditKey WHERE cDirXml LIKE '%<ActivationKey>" & cLink & "</ActivationKey>%'"
                userId = myWeb.moDbHelper.GetDataValue(cSQL, , , 0)

                If userId > 0 Then
                    'change the account status
                    myWeb.moDbHelper.setObjectStatus(dbHelper.objectTypes.Directory, dbHelper.Status.Live, userId)
                    'remove the activation key from userXML
                    Dim oUserXml As New XmlDocument
                    Dim oUserInstance As XmlElement = oUserXml.CreateElement("Instance")
                    oUserXml.AppendChild(oUserInstance)
                    oUserInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.Directory, userId)
                    Dim ActivationKeyElmt As XmlElement
                    ActivationKeyElmt = oUserInstance.FirstChild.SelectSingleNode("cDirXml/User/ActivationKey")
                    ActivationKeyElmt.ParentNode.RemoveChild(ActivationKeyElmt)
                    myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Directory, oUserXml.DocumentElement, userId)

                    myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Register, userId, myWeb.moSession.SessionID, Now, 0, 0, "Activate")
                    Select Case myWeb.moConfig("ActivateBehaviour")
                        Case "LogonReload"
                            myWeb.mnUserId = userId
                            If Not myWeb.moSession Is Nothing Then
                                myWeb.moSession("nUserId") = myWeb.mnUserId
                            End If
                            'Add subscribe to subscribe to the subscription if we have one
                            If myWeb.mnArtId > 0 Then
                                myWeb.msRedirectOnEnd = myWeb.mcPageURL & "?artid=" & myWeb.mnArtId & "&ewCmd=Subscribe"
                            Else

                                myWeb.msRedirectOnEnd = myWeb.mcPageURL & "?ewCmd=Subscribe"
                            End If

                        Case "LogonRedirect"
                            'todo
                        Case Else
                            'do nuffin
                    End Select
                    Return True

                Else
                    Return False
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ReactivateAccount", ex, ""))
                Return False
            End Try
        End Function

#End Region

#Region "Secure Membership"

        Public Function SetCookie(ByVal Name As String, ByVal Value As Object, Optional ByVal Expires As Date = Nothing) As System.Web.HttpCookie
            Try
                Dim Cookie As System.Web.HttpCookie = myWeb.moResponse.Cookies(Name)
                If Cookie Is Nothing Then Cookie = New System.Web.HttpCookie(Name)
                Cookie.Value = Value
                If myWeb.moConfig("SecureMembershipDomain") <> "" Then
                    Cookie.Domain = myWeb.moConfig("SecureMembershipDomain")
                End If

                If Expires = Nothing Then Cookie.Expires = Expires
                myWeb.moResponse.Cookies.Add(Cookie)
                Return Cookie
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SetCookie", ex, ""))
                Return Nothing
            End Try
        End Function



        Public Function CookieValue(ByVal Name As String, Optional ByVal ValueIfNothing As Object = Nothing) As Object
            Try
                If Not myWeb.moRequest.Cookies(Name) Is Nothing Then
                    Return myWeb.moRequest.Cookies(Name).Value
                Else
                    Return ValueIfNothing
                End If
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CookieValue", ex, ""))
                Return Nothing
            End Try
        End Function

        Public Sub SecureMembershipProcess(ByVal cForceCommand As String)
            Try
                'Variables
                Dim ASPSessionName As String = IIf(myWeb.moConfig("ASPSessionName") = "", "ASP.NET_SessionId", myWeb.moConfig("ASPSessionName"))
                Dim UserCookieName As String = IIf(myWeb.moConfig("UserCookieName") = "", "nUserId", myWeb.moConfig("UserCookieName"))
                Dim SecureMembershipAddress As String = myWeb.moConfig("SecureMembershipAddress")
                Dim SecureMembershipDomain As String = myWeb.moConfig("SecureMembershipDomain")

                'Session Cookie
                System.Web.HttpContext.Current.Response.Cookies(ASPSessionName).Value = System.Web.HttpContext.Current.Session.SessionID
                If SecureMembershipDomain <> "" Then
                    System.Web.HttpContext.Current.Response.Cookies(ASPSessionName).Domain = SecureMembershipDomain
                End If
                'Path
                Dim cPath As String = "" & myWeb.moRequest.QueryString("path")
                For i As Integer = 0 To myWeb.moRequest.QueryString.Count - 1
                    If Not myWeb.moRequest.QueryString.Keys(i) = "path" Then
                        cPath &= "&" & myWeb.moRequest.QueryString.Keys(i)
                        cPath &= "=" & myWeb.moRequest.QueryString.Item(i)
                    End If
                Next
                'Check Request Cookie first
                Dim nCookieUser As Integer = CookieValue(UserCookieName, -1)
                'Check what we are doing
                If LCase(cForceCommand) = "logoff" Then
                    'redirect to the logoff with no cookie
                    SetCookie(UserCookieName, 0, Now.AddMinutes(-20))
                    If Not LCase(cPath).Contains(LCase("ewCmd=LogOff")) Then
                        If cPath.Contains("?") Then
                            cPath &= "&"
                        Else
                            cPath &= "?"
                        End If
                        cPath &= "ewCmd=LogOff"
                    End If
                    'Ultimate logoff cmd
                    myWeb.moSession("nUserId") = 0
                    myWeb.moSession.Abandon()
                    myWeb.mnUserId = 0
                    myWeb.msRedirectOnEnd = myWeb.moConfig("SecureMembershipAddress") & myWeb.moConfig("ProjectPath") & "/"
                ElseIf LCase(cForceCommand) = "logoffimpersonate" Then
                    'redirect to the logoff with no cookie
                    SetCookie(UserCookieName, 0, Now.AddMinutes(-20))
                    myWeb.moSession("nUserId") = 0
                    myWeb.mnUserId = 0
                ElseIf nCookieUser > 0 Then
                    'logon with this user
                    myWeb.mnUserId = nCookieUser
                End If

                'Check to see if we need to do a redirect
                Dim cHost As String = IIf(myWeb.moRequest.ServerVariables("HTTPS") = "on", "https://", "http://") & myWeb.moRequest.ServerVariables("HTTP_HOST")
                Dim oVariants As New Hashtable
                oVariants.Add("0", cHost)
                oVariants.Add("1", cHost & "/")
                oVariants.Add("2", cHost & myWeb.moConfig("ProjectPath"))
                oVariants.Add("3", cHost & myWeb.moConfig("ProjectPath") & "/")

                'oVariants.Add("2", "http://" & cHost)
                'oVariants.Add("3", "http://" & cHost & "/")

                'If oVariants.ContainsValue(SecureMembershipAddress) And myWeb.mnUserId = 0 Then
                '    If Left(cPath, 1) = "/" And Right(myWeb.moConfig("HomeUrl"), 1) = "/" Then cPath = cPath.Remove(0, 1)
                '    myWeb.moResponse.Redirect(myWeb.moConfig("HomeUrl") & cPath)
                'Else
                If Not oVariants.ContainsValue(SecureMembershipAddress) And (myWeb.mnUserId > 0 Or myWeb.mbAdminMode Or LCase(myWeb.moRequest("ewCmd")) = "ar") Then
                    SetCookie(UserCookieName, myWeb.mnUserId, Now.AddMinutes(20))
                    If Not Right(SecureMembershipAddress, 1) = "/" Then SecureMembershipAddress &= "/"
                    If Left(cPath, 1) = "/" And Right(SecureMembershipAddress, 1) = "/" Then cPath = cPath.Remove(0, 1)
                    If cPath.StartsWith("&") Then cPath = "?" & cPath.Substring(1)
                    myWeb.msRedirectOnEnd = SecureMembershipAddress & cPath
                    ' myWeb.moResponse.Redirect(SecureMembershipAddress & cPath)
                ElseIf oVariants.ContainsValue(SecureMembershipAddress) And myWeb.mnUserId > 0 Then
                    If Not myWeb.moPageXml Is Nothing Then
                        If Not myWeb.moPageXml.DocumentElement Is Nothing Then
                            myWeb.moPageXml.DocumentElement.SetAttribute("baseUrl", SecureMembershipAddress & myWeb.moConfig("ProjectPath") & "/")
                        End If
                    End If
                    SetCookie(UserCookieName, myWeb.mnUserId, Now.AddMinutes(20))
                ElseIf myWeb.moRequest.ServerVariables("HTTPS") = "on" Then
                    If Not myWeb.moPageXml Is Nothing Then
                        If Not myWeb.moPageXml.DocumentElement Is Nothing Then
                            'don't want this for everything just the logon form
                            myWeb.moPageXml.DocumentElement.SetAttribute("baseUrl", SecureMembershipAddress & myWeb.moConfig("ProjectPath"))
                        End If
                    End If
                End If


            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SecureMembershipProcess", ex, ""))
            End Try
        End Sub

        Public Sub ProviderActions(ByRef myWeb As Protean.Cms, ByVal actionName As String)
            Dim sProcessInfo As String = ""
            Try

                Dim castObject As Object = WebConfigurationManager.GetWebApplicationSection("protean/membershipProviders")
                Dim moPrvConfig As Protean.ProviderSectionHandler = castObject

                Dim ourProvider As Object
                For Each ourProvider In moPrvConfig.Providers
                    If Not ourProvider.parameters(actionName) Is Nothing Then
                        Dim calledType As Type
                        Dim assemblyInstance As [Assembly]

                        If ourProvider.parameters("path") <> "" Then
                            assemblyInstance = [Assembly].LoadFrom(myWeb.goServer.MapPath(ourProvider.parameters("path")))
                        Else
                            assemblyInstance = [Assembly].Load(ourProvider.Type)
                        End If
                        If ourProvider.parameters("rootClass") = "" Then
                            calledType = assemblyInstance.GetType("Protean.Providers.Membership." & ourProvider.Name & "Tools.Actions", True)
                        Else
                            calledType = assemblyInstance.GetType(ourProvider.parameters("rootClass") & ".Providers.Membership." & ourProvider.Name & "Tools.Actions", True)
                        End If

                        Dim o As Object = Activator.CreateInstance(calledType)

                        Dim args(0) As Object
                        args(0) = myWeb

                        calledType.InvokeMember(ourProvider.parameters(actionName), BindingFlags.InvokeMethod, Nothing, o, args)

                    End If
                Next


            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProviderActions", ex, ""))
            End Try
        End Sub

#End Region

#Region "Module Behaviour"

        Public Class Modules

            Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Protean.Cms.Membership.Modules"

            Public Sub New()

                'do nowt

            End Sub

            'sReturnValue = "LogOn"
            'moDbHelper.logActivity(dbHelper.ActivityType.Logon, mnUserId, mnPageId, mnArtId)
            ' myWeb.moDbHelper.CommitLogToDB(dbHelper.ActivityType.Logon, myWeb.mnUserId, myWeb.moSession.SessionID, Now, 0, 0, "")

            Public Sub Logon(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try
                    Dim adXfm As Object = myWeb.getAdminXform()
                    adXfm.open(myWeb.moPageXml)
                    Dim oXfmElmt As XmlElement
                    Dim sReturnValue As String = Nothing
                    Dim cLogonCmd As String = ""

                    If myWeb.mnUserId = 0 And (myWeb.moRequest("ewCmd") <> "passwordReminder" And myWeb.moRequest("ewCmd") <> "ActivateAccount") Then

                        oXfmElmt = adXfm.xFrmUserLogon()
                        Dim bAdditionalChecks As Boolean = False
                        If Not (adXfm.valid) Then
                            ' Call in additional authentication checks
                            If myWeb.moConfig("AlternativeAuthentication") = "On" Then
                                bAdditionalChecks = myWeb.AlternativeAuthentication()
                            End If
                        End If

                        If adXfm.valid Or bAdditionalChecks Then
                            myWeb.moContentDetail = Nothing
                            'mnUserId = adXfm.mnUserId
                            If Not myWeb.moSession Is Nothing Then myWeb.moSession("nUserId") = myWeb.mnUserId
                            If myWeb.moRequest("cRemember") = "true" Then
                                Dim oCookie As System.Web.HttpCookie = New System.Web.HttpCookie("RememberMe")
                                oCookie.Value = myWeb.mnUserId
                                oCookie.Expires = DateAdd(DateInterval.Day, 60, Now())
                                myWeb.moResponse.Cookies.Add(oCookie)
                            End If
                            'Now we want to reload as permissions have changed
                            If Not myWeb.moSession Is Nothing Then
                                If Not myWeb.moSession("cLogonCmd") Is Nothing Then
                                    cLogonCmd = Split(myWeb.moSession("cLogonCmd"), "=")(0)
                                    If myWeb.mcOriginalURL.Contains(cLogonCmd & "=") Then
                                        cLogonCmd = ""
                                    Else
                                        If myWeb.mcOriginalURL.Contains("=") Then
                                            cLogonCmd = "&" & myWeb.moSession("cLogonCmd")
                                        Else
                                            cLogonCmd = "?" & myWeb.moSession("cLogonCmd")
                                        End If
                                    End If
                                End If
                            End If
                            myWeb.moSession("RedirectReason") = "logonSuccessful"

                            Dim oMembership As New Protean.Cms.Membership(myWeb)
                            oMembership.ProviderActions(myWeb, "logonAction")

                            myWeb.logonRedirect(cLogonCmd)
                        Else
                            oContentNode.InnerXml = oXfmElmt.InnerXml
                        End If

                    ElseIf myWeb.moRequest("ewCmd") = "passwordReminder" Then
                        'RJP 7 Nov 2012. Amended to use Lower Case to prevent against case sensitive entries in Protean.Cms.Config.
                        Select Case LCase(myWeb.moConfig("MembershipEncryption"))
                            Case "md5", "sha1", "sha256"
                                oXfmElmt = adXfm.xFrmResetAccount()
                            Case Else
                                oXfmElmt = adXfm.xFrmPasswordReminder()
                        End Select
                        oContentNode.InnerXml = oXfmElmt.InnerXml

                    ElseIf myWeb.moRequest("ewCmd") = "ActivateAccount" Then

                        oXfmElmt = adXfm.xFrmActivateAccount()
                        oContentNode.InnerXml = oXfmElmt.InnerXml

                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""))
                End Try
            End Sub

            Public Sub Register(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Try
                    myWeb.bPageCache = False
                    Dim mnUserId As Long = myWeb.mnUserId
                    Dim mbAdminMode As Boolean = myWeb.mbAdminMode
                    Dim moPageXml As XmlDocument = myWeb.moPageXml
                    Dim moDbHelper As Protean.Cms.dbHelper = myWeb.moDbHelper
                    Dim moSession As System.Web.SessionState.HttpSessionState = myWeb.moSession
                    Dim moConfig As System.Collections.Specialized.NameValueCollection = myWeb.moConfig
                    Dim moRequest As System.Web.HttpRequest = myWeb.moRequest

                    Dim goServer As System.Web.HttpServerUtility = myWeb.goServer

                    Dim cLogonCmd As String = ""
                    Dim bRedirectStarted As Boolean = False
                    Dim oXfmElmt As XmlElement
                    Dim sProcessInfo As String

                    Dim AccountCreateForm As String = "UserRegister"
                    Dim AccountUpdateForm As String = "UserMyAccount"

                    If oContentNode.GetAttribute("accountCreateFormName") <> "" Then AccountCreateForm = oContentNode.GetAttribute("accountCreateFormName")
                    If oContentNode.GetAttribute("accountUpdateFormName") <> "" Then AccountUpdateForm = oContentNode.GetAttribute("accountUpdateFormName")

                    Dim bLogon As Boolean = False

                    Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                    Dim adXfm As Object = oMembershipProv.AdminXforms
                    Dim bRedirect As Boolean = True


                    ' OAuth Functionality

                    If Not moRequest("oAuthResp") <> "" Then
                        If moRequest("oAuthReg") <> "" And myWeb.msRedirectOnEnd = "" Then
                            Dim sRedirectPath = ""
                            Dim appId = ""
                            Dim redirectURI = "https://" & moRequest.ServerVariables("SERVER_NAME") & myWeb.mcPageURL & "?oAuthResp=" & moRequest("oAuthReg")
                            Select Case moRequest("oAuthReg")
                                Case "facebook"
                                    sRedirectPath = "https://www.facebook.com/v2.8/dialog/oauth?"
                                    appId = moConfig("OauthFacebookId")
                                    sRedirectPath = sRedirectPath & "client_id=" & appId & "&redirect_uri=" & redirectURI
                                Case "twitter"
                                    Dim twApi As New Integration.Directory.Twitter(myWeb)
                                    twApi.twitterConsumerKey = moConfig("OauthTwitterId")
                                    twApi.twitterConsumerSecret = moConfig("OauthTwitterKey")
                                    sRedirectPath = twApi.GetRequestToken()
                            End Select
                            myWeb.msRedirectOnEnd = sRedirectPath
                        Else

                        End If
                    Else
                        Select Case moRequest("oAuthResp")
                            Case "facebook"
                                Dim redirectURI = "http://" & moRequest.ServerVariables("SERVER_NAME") & myWeb.mcPageURL & "?oAuthResp=facebook"
                                sProcessInfo = "Facebook Response"
                                Dim fbClient As New Protean.Integration.Directory.Facebook(myWeb, moConfig("OauthFacebookId"), moConfig("OauthFacebookKey"))
                                Dim fbUsers As List(Of Protean.Integration.Directory.Facebook.User)
                                fbUsers = fbClient.GetFacebookUserData(moRequest("code"), redirectURI)
                                sProcessInfo = fbUsers(0).first_name & " " & fbUsers(0).last_name

                                mnUserId = fbClient.CreateUser(fbUsers(0))

                                If Not moSession Is Nothing Then moSession("nUserId") = mnUserId
                                moDbHelper.CommitLogToDB(dbHelper.ActivityType.Register, mnUserId, moSession.SessionID, Now, 0, 0, "First Logon")

                                bLogon = True

                            Case "twitter"
                                sProcessInfo = "Twitter Response"
                                Dim twApi As New Integration.Directory.Twitter(myWeb)
                                twApi.twitterConsumerKey = moConfig("OauthTwitterId")
                                twApi.twitterConsumerSecret = moConfig("OauthTwitterKey")
                                'Get twitter user
                                Dim twUsers As List(Of Protean.Integration.Directory.Twitter.User)


                        End Select
                        If mnUserId > 0 Then



                        End If


                    End If

                    ' If not in admin mode then base our choice on whether the user is logged in. 
                    ' If in Admin Mode, then present it as WYSIWYG
                    If (Not (myWeb.mbAdminMode) And myWeb.mnUserId > 0) Then

                        Dim oContentForm As XmlElement = myWeb.moPageXml.SelectSingleNode("descendant-Or-selfContent[@type='xform' and @name='UserMyAccount']")
                        If oContentForm Is Nothing Then
                            oXfmElmt = adXfm.xFrmEditDirectoryItem(myWeb.mnUserId, "User", , AccountUpdateForm)
                        Else
                            oXfmElmt = adXfm.xFrmEditDirectoryItem(myWeb.mnUserId, "User", , AccountUpdateForm, oContentForm.OuterXml)
                            If Not myWeb.mbAdminMode Then oContentForm.ParentNode.RemoveChild(oContentForm)
                        End If
                        oContentNode.InnerXml = oXfmElmt.InnerXml

                    Else

                        Dim oContentForm As XmlElement = moPageXml.SelectSingleNode("descendant-or-self::Content[@type='xform' and @name='UserRegister']")
                        If oContentForm Is Nothing Then
                            oXfmElmt = adXfm.xFrmEditDirectoryItem(mnUserId, "User", , AccountCreateForm)
                        Else
                            oXfmElmt = adXfm.xFrmEditDirectoryItem(mnUserId, "User", , AccountCreateForm, oContentForm.OuterXml)
                            If Not mbAdminMode Then oContentForm.ParentNode.RemoveChild(oContentForm)
                        End If

                        If myWeb.moConfig("SecureMembershipAddress") <> "" And myWeb.mbAdminMode = False Then
                            Dim oSubElmt As XmlElement = adXfm.moXformElmt.SelectSingleNode("descendant::submission")
                            oSubElmt.SetAttribute("action", myWeb.moConfig("SecureMembershipAddress") & myWeb.mcPagePath)
                        End If

                        ' ok if the user is valid we then need to handle what happens next.
                        If adXfm.valid Then

                            Select Case myWeb.moConfig("RegisterBehaviour")

                                Case "validateByEmail"
                                    'don't redirect because we want to reuse this form
                                    bRedirect = False

                                    'say thanks for registering and update the form

                                    'hide the current form
                                    Dim oFrmGrp As XmlElement = adXfm.moXformElmt.SelectSingleNode("group")
                                    oFrmGrp.SetAttribute("class", "hidden")
                                    'create a new note
                                    Dim oFrmGrp2 As XmlElement = adXfm.addGroup(adXfm.moXformElmt, "validateByEmail")
                                    adXfm.addNote(oFrmGrp2, xForm.noteTypes.Hint, "<span class=""msg-1029"">Thanks for registering you have been sent an email with a link you must click to activate your account</span>", True)

                                    'lets get the new userid from the instance
                                    mnUserId = adXfm.instance.SelectSingleNode("tblDirectory/nDirKey").InnerText

                                    'first we set the user account to be pending
                                    moDbHelper.setObjectStatus(dbHelper.objectTypes.Directory, dbHelper.Status.Pending, mnUserId)

                                    Dim oMembership As New Protean.Cms.Membership(myWeb)
                                    AddHandler oMembership.OnError, AddressOf myWeb.OnComponentError
                                    oMembership.AccountActivateLink(mnUserId)

                                    moDbHelper.CommitLogToDB(dbHelper.ActivityType.Register, mnUserId, moSession.SessionID, Now, 0, 0, "Send Activation")


                                Case Else ' Auto logon
                                    mnUserId = adXfm.instance.SelectSingleNode("tblDirectory/nDirKey").InnerText
                                    If Not moSession Is Nothing Then moSession("nUserId") = mnUserId

                                    moDbHelper.CommitLogToDB(dbHelper.ActivityType.Register, mnUserId, moSession.SessionID, Now, 0, 0, "First Logon")

                                    bLogon = True

                            End Select






                            'send registration confirmation
                            Dim xsltPath As String = "/xsl/email/registration.xsl"

                            If IO.File.Exists(goServer.MapPath(xsltPath)) Then
                                Dim oUserElmt As XmlElement = moDbHelper.GetUserXML(mnUserId)

                                Dim oElmtPwd As XmlElement = moPageXml.CreateElement("Password")
                                oElmtPwd.InnerText = moRequest("cDirPassword")
                                oUserElmt.AppendChild(oElmtPwd)

                                oUserElmt.SetAttribute("Url", myWeb.mcOriginalURL)

                                Dim oUserEmail As XmlElement = oUserElmt.SelectSingleNode("Email")
                                Dim fromName As String = moConfig("SiteAdminName")
                                Dim fromEmail As String = moConfig("SiteAdminEmail")
                                Dim recipientEmail As String = ""
                                If Not oUserEmail Is Nothing Then recipientEmail = oUserEmail.InnerText
                                Dim SubjectLine As String = "Your Registration Details"
                                Dim oMsg As Messaging = New Messaging
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
                            Else
                                oContentNode.InnerXml = oXfmElmt.InnerXml
                            End If
                        Else
                            oContentNode.InnerXml = oXfmElmt.InnerXml
                        End If
                    End If

                    If bLogon Then
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

                        Dim redirectId As String = myWeb.moConfig("RegisterRedirectPageId")

                        If oContentNode.GetAttribute("redirectPathId") <> "" Then
                            redirectId = oContentNode.GetAttribute("redirectPathId")
                        End If

                        If redirectId <> "" Then
                            'refresh the site strucutre with new userId
                            myWeb.mnUserId = mnUserId
                            myWeb.GetStructureXML("Site")
                            Dim oElmt As XmlElement = myWeb.moPageXml.SelectSingleNode("/Page/Menu/descendant-or-self::MenuItem[@id = '" & redirectId & "']")
                            Dim redirectPath As String = myWeb.mcOriginalURL
                            If oElmt Is Nothing Then
                                myWeb.msRedirectOnEnd = redirectPath
                                bRedirect = True
                            Else
                                redirectPath = oElmt.GetAttribute("url")
                                myWeb.msRedirectOnEnd = redirectPath
                                bRedirect = False
                            End If
                        End If
                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""))
                End Try
            End Sub

            Public Sub PasswordReminder(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)

                Dim moConfig As System.Collections.Specialized.NameValueCollection = myWeb.moConfig
                Try
                    Dim adXfm As Object = myWeb.getAdminXform()

                    Dim oXfmElmt As XmlElement
                    If myWeb.mnUserId = 0 Then
                        Select Case LCase(moConfig("MembershipEncryption"))
                            Case "md5salt", "md5", "sha1", "sha256"
                                If myWeb.moRequest("ewCmd") = "AR-MOD" Then
                                    Dim cAccountHash As String = myWeb.moRequest("AI")
                                    If Not cAccountHash = "" Then
                                        oXfmElmt = adXfm.xFrmConfirmPassword(cAccountHash)
                                    Else
                                        oXfmElmt = adXfm.xFrmResetAccount()
                                    End If
                                Else
                                    oXfmElmt = adXfm.xFrmResetAccount()
                                End If
                            Case Else
                                oXfmElmt = adXfm.xFrmPasswordReminder()
                        End Select
                    Else
                        oXfmElmt = adXfm.xFrmConfirmPassword(myWeb.mnUserId)
                    End If


                    oContentNode.InnerXml = oXfmElmt.InnerXml

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""))
                End Try
            End Sub

            Public Sub GetUserContent(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)
                Dim ContentType As String = oContentNode.GetAttribute("contentType")
                Dim UserId As String = myWeb.mnUserId
                Dim WhereSql As String = " cContentSchemaName = '" & ContentType & "' and nInsertDirId = " & UserId

                Try
                    If UserId <> 0 Then
                        myWeb.mbAdminMode = True
                        myWeb.GetPageContentFromSelect(WhereSql, False,  , , , "", oContentNode, , , , True)
                        myWeb.mbAdminMode = False
                    End If

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetUserContent", ex, ""))
                End Try
            End Sub


            Public Sub GetUsersByGroup(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                Try

                    Dim cSchemaName As String = contentNode.Attributes("parentSchemaType").InnerText
                    Dim sArrDirKeys As String = contentNode.Attributes("parentIds").InnerText 'comma separated int array
                    Dim nStatus As Integer = contentNode.Attributes("parentStatus").InnerText
                    Dim nParDirId As String = contentNode.Attributes("parentParId").InnerText

                    If sArrDirKeys.Length > 0 Then

                        'make SQL statement
                        Dim strSql As New Text.StringBuilder
                        strSql.Append("CREATE TABLE #tmpTblUsers ")
                        strSql.Append("(")
                        strSql.Append("[id] INT, ")
                        strSql.Append("[Status] INT, ")
                        strSql.Append("[Username] NVARCHAR(100), ")
                        strSql.Append("[UserXml] NVARCHAR(max), ")
                        strSql.Append("[Companies] NVARCHAR(100) ")
                        strSql.Append(") ")

                        'gets all users using standard stored proc
                        strSql.Append("INSERT INTO #tmpTblUsers ")
                        strSql.Append("EXEC spGetUsers " & nParDirId.ToString & ", " & nStatus.ToString & " ")

                        'narrows scope of users
                        strSql.Append("SELECT ")
                        strSql.Append("#tmpTblUsers.[id] ")
                        strSql.Append("FROM tblDirectory AS tblGroups ")
                        strSql.Append("INNER JOIN tblDirectoryRelation ON tblGroups.nDirKey = tblDirectoryRelation.nDirParentId ")
                        strSql.Append("INNER JOIN #tmpTblUsers ON tblDirectoryRelation.nDirChildId = #tmpTblUsers.id ")
                        strSql.Append("WHERE (tblGroups.cDirSchema = '" & cSchemaName & "') ")
                        strSql.Append(" AND (tblGroups.nDirKey in (" & sArrDirKeys & ")) ")

                        'add each result to content node xml
                        Dim oDsUsers As DataSet = myWeb.moDbHelper.GetDataSet(strSql.ToString, "User", "UserGroups", )
                        Dim oDrUser As DataRow
                        For Each oDrUser In oDsUsers.Tables(0).Rows
                            contentNode.AppendChild(myWeb.GetUserXML(oDrUser("id")).Clone)
                        Next

                    End If


                Catch ex As Exception
                    returnException(mcModuleName, "GetUsersByGroup", ex, "", "", gbDebug)
                    'Return Nothing
                End Try

            End Sub


            Public Sub UserContact(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                Try

                    If myWeb.mnUserId > 0 Then

                        Dim adXfm As Object = myWeb.getAdminXform()
                        adXfm.open(myWeb.moPageXml)

                        Select Case myWeb.moRequest("ewCmd")
                            Case "addContact", "editContact"
                                Dim oXfmElmt As XmlElement = adXfm.xFrmEditDirectoryContact(myWeb.moRequest("id"), myWeb.mnUserId)
                                If Not adXfm.valid Then
                                    contentNode.AppendChild(oXfmElmt)
                                Else
                                    myWeb.RefreshUserXML()
                                End If
                            Case "delContact"
                                Dim oContactElmt As XmlElement
                                For Each oContactElmt In myWeb.GetUserXML(myWeb.mnUserId).SelectNodes("descendant-or-self::Contact")
                                    Dim oId As XmlElement = oContactElmt.SelectSingleNode("nContactKey")
                                    If Not oId Is Nothing Then
                                        If oId.InnerText = myWeb.moRequest("id") Then
                                            myWeb.moDbHelper.DeleteObject(dbHelper.objectTypes.CartContact, myWeb.moRequest("id"))
                                            myWeb.RefreshUserXML()
                                        End If
                                    End If
                                Next
                        End Select
                    End If

                Catch ex As Exception
                    returnException(mcModuleName, "UserContacts", ex, "", "", gbDebug)
                    'Return Nothing
                End Try

            End Sub

            Public Sub CompanyContact(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)

                Try

                    If myWeb.mnUserId > 0 Then

                        Dim adXfm As Object = myWeb.getAdminXform()
                        adXfm.open(myWeb.moPageXml)

                        Select Case myWeb.moRequest("ewCmd")
                            Case "addContact", "editContact"
                                Dim oXfmElmt As XmlElement = adXfm.xFrmEditDirectoryContact(myWeb.moRequest("id"), myWeb.mnUserId)
                                If Not adXfm.valid Then
                                    contentNode.AppendChild(oXfmElmt)
                                Else
                                    myWeb.RefreshUserXML()
                                End If
                            Case "delContact"
                                Dim oContactElmt As XmlElement
                                For Each oContactElmt In myWeb.GetUserXML(myWeb.mnUserId).SelectNodes("descendant-or-self::Contact")
                                    Dim oId As XmlElement = oContactElmt.SelectSingleNode("nContactKey")
                                    If Not oId Is Nothing Then
                                        If oId.InnerText = myWeb.moRequest("id") Then
                                            myWeb.moDbHelper.DeleteObject(dbHelper.objectTypes.CartContact, myWeb.moRequest("id"))
                                            myWeb.RefreshUserXML()
                                        End If
                                    End If
                                Next
                        End Select
                    End If

                Catch ex As Exception
                    returnException(mcModuleName, "UserContacts", ex, "", "", gbDebug)
                    'Return Nothing
                End Try

            End Sub


            Public Sub AddUserToGroup(ByRef myWeb As Protean.Cms, ByRef contentNode As XmlElement)
                Dim groupNode As XmlElement
                Try

                    If myWeb.mnUserId > 0 And Not (contentNode.SelectSingleNode("ancestor::Order") Is Nothing) Then

                        For Each groupNode In contentNode.SelectNodes("descendant-or-self::UserGroups/Group")
                            Dim groupId As String = groupNode.GetAttribute("id")
                            If IsNumeric(groupId) Then
                                'Dim cUserEmail As String = myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, myWeb.mnUserId)
                                'Dim cGroupName As String = myWeb.moDbHelper.getNameByKey(dbHelper.objectTypes.Directory, CLng(groupId))

                                myWeb.moDbHelper.maintainDirectoryRelation(CLng(groupId), myWeb.mnUserId, False)
                            End If
                        Next

                    End If

                Catch ex As Exception
                    returnException(mcModuleName, "AddUserToGroup", ex, "", "", gbDebug)
                    'Return Nothing
                End Try

            End Sub

            Public Sub JobApplication(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)

                Dim cProcessInfo As String
                Dim moConfig As System.Collections.Specialized.NameValueCollection = myWeb.moConfig
                Dim oElmt As XmlElement
                Dim JobId As Long = CLng("0" & myWeb.moRequest("JobId"))
                Dim FormId As Long = CLng("0" & myWeb.moRequest("FormId"))
                Try
                    If myWeb.mnUserId = 0 Then
                        Register(myWeb, oContentNode)

                        Dim logonContentNode As XmlElement = myWeb.moPageXml.CreateElement("Content")
                        oContentNode.ParentNode.AppendChild(logonContentNode)

                        Logon(myWeb, logonContentNode)

                    Else
                        If myWeb.moRequest("FormId") = "" Then
                            'List User Applications
                            Dim strSql As New Text.StringBuilder
                            strSql.Append("SELECT * FROM [tblEmailActivityLog] eal ")
                            strSql.Append("INNER JOIN tblActivityLog al on al.nOtherId = eal.nEmailActivityKey ")
                            strSql.Append("where al.nUserDirId = " & myWeb.mnUserId)
                            Dim oDsJobs As DataSet = myWeb.moDbHelper.GetDataSet(strSql.ToString, "Application", "JobApplications", )
                            oContentNode.InnerXml = Replace(oDsJobs.GetXml, "xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""", "")
                            Dim oElmt2 As XmlElement
                            Dim sContent As String
                            For Each oElmt2 In oContentNode.SelectNodes("descendant-or-self::cActivityXml | descendant-or-self::cActivityDetail")
                                sContent = oElmt2.InnerText
                                If sContent <> "" Then
                                    Try
                                        oElmt2.InnerXml = sContent
                                    Catch
                                        oElmt2.InnerXml = Protean.tidyXhtmlFrag(sContent)
                                    End Try
                                End If
                            Next


                        ElseIf oContentNode.ParentNode.Name = "ContentDetail" Then
                            'remove application forms not required
                            For Each oElmt In oContentNode.SelectNodes("Content[@id!='" & myWeb.moRequest("FormId") & "']")
                                oElmt.ParentNode.RemoveChild(oElmt)
                            Next

                            For Each oElmt In oContentNode.SelectNodes("Content[@id='" & myWeb.moRequest("FormId") & "']")

                                'Form Handling
                                Dim oXform As Protean.xForm = myWeb.getXform()

                                oXform.moPageXML = myWeb.moPageXml
                                oXform.load(oElmt, True)

                                If myWeb.moRequest("stageButton") = "" And myWeb.moRequest("stage") <> "" Then
                                    Dim passportNode2 As XmlElement = oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport")
                                    passportNode2.SetAttribute("stage", myWeb.moRequest("stage"))
                                    myWeb.moSession("tempInstance") = oXform.Instance
                                End If

                                Dim strSql As New Text.StringBuilder
                                strSql.Append("SELECT eal.nEmailActivityKey FROM [tblEmailActivityLog] eal ")
                                strSql.Append("INNER JOIN tblActivityLog al on al.nOtherId = eal.nEmailActivityKey ")
                                strSql.Append("where al.nUserDirId = " & myWeb.mnUserId & " and al.nArtId = " & JobId)
                                Dim EmailActivityId As Long = CLng("0" & myWeb.moDbHelper.GetDataValue(strSql.ToString))
                                If EmailActivityId > 0 Then
                                    'load in a saved instance
                                    Dim strSql2 As New Text.StringBuilder
                                    strSql2.Append("SELECT cActivityXml FROM [tblEmailActivityLog] eal ")
                                    strSql2.Append("INNER JOIN tblActivityLog al on al.nOtherId = eal.nEmailActivityKey ")
                                    strSql2.Append("where al.nUserDirId = " & myWeb.mnUserId & " and al.nArtId = " & JobId)

                                    Dim loadedInstance As String = myWeb.moDbHelper.GetDataValue(strSql2.ToString)
                                    If loadedInstance <> "" And myWeb.moSession("tempInstance") Is Nothing Then
                                        Dim oLoadedInstance As XmlElement = myWeb.moPageXml.CreateElement("instance")
                                        oLoadedInstance.InnerXml = loadedInstance
                                        'tidy up the stage editor

                                        oXform.Instance = oLoadedInstance.FirstChild
                                    End If
                                Else
                                    'set the job title
                                    Dim JobTitleNode As XmlElement = oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport/LearnerInfo/Headline/Type/Label")
                                    JobTitleNode.InnerText = myWeb.moRequest("JobApply")
                                    Dim JobCodeNode As XmlElement = oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport/LearnerInfo/Headline/Type/Code")
                                    JobCodeNode.InnerText = myWeb.moRequest("JobId")
                                End If

                                If oXform.isSubmitted Then

                                    oXform.updateInstanceFromRequest()

                                    'tidy up the stage editor
                                    If myWeb.moRequest("stageButton") = "" Then
                                        Dim passportNode As XmlElement = oXform.Instance.SelectSingleNode("descendant-or-self::SkillsPassport")
                                        passportNode.SetAttribute("stage", myWeb.moRequest("stage"))
                                    End If

                                    ' save instace & progress
                                    If EmailActivityId > 0 Then
                                        Dim strSql3 As New Text.StringBuilder
                                        strSql3.Append("update tblEmailActivityLog set cActivityXml = '" & SqlFmt(oXform.Instance.OuterXml) & "'")
                                        strSql3.Append("where nEmailActivityKey = " & EmailActivityId)
                                        myWeb.moDbHelper.ExeProcessSql(strSql3.ToString)
                                    Else
                                        EmailActivityId = myWeb.moDbHelper.emailActivity(myWeb.mnUserId, , , , oXform.Instance.OuterXml)
                                        myWeb.moDbHelper.logActivity(dbHelper.ActivityType.JobApplication, myWeb.mnUserId, 0, JobId, EmailActivityId, "")
                                    End If

                                    If myWeb.moRequest("SaveExit") <> "" Then

                                        myWeb.msRedirectOnEnd = myWeb.mcPagePath

                                    End If

                                    If myWeb.moRequest("stageButton") = "Validate" Then

                                        'We validate the form

                                        Dim oSkillsPassport As XmlElement = oXform.Instance.SelectSingleNode("emailer/oBodyXML/SkillsPassport")
                                        oSkillsPassport.SetAttribute("stage", "Validate")
                                        oXform.validate()

                                        If oXform.valid Then
                                            Dim sMessage As String

                                            Dim oMsg As Protean.Messaging = New Protean.Messaging

                                            sMessage = oMsg.emailer(oXform.Instance.SelectSingleNode("emailer/oBodyXML"),
                                                                    oXform.Instance.SelectSingleNode("emailer/xsltPath").InnerText,
                                                                    oXform.Instance.SelectSingleNode("emailer/fromName").InnerText,
                                                                    oXform.Instance.SelectSingleNode("emailer/fromEmail").InnerText,
                                                                    oXform.Instance.SelectSingleNode("emailer/recipientEmail").InnerText,
                                                                    oXform.Instance.SelectSingleNode("emailer/SubjectLine").InnerText, , , ,
                                                                    oXform.Instance.SelectSingleNode("emailer/ccRecipient").InnerText,
                                                                    oXform.Instance.SelectSingleNode("emailer/bccRecipient").InnerText,
                                                                    "")
                                            'Return sMessage
                                            Dim oEmailer As XmlElement = oXform.Instance.SelectSingleNode("emailer")
                                            oEmailer.SetAttribute("SubmitMessage", sMessage)

                                            oSkillsPassport.SetAttribute("stage", "Complete")

                                        End If

                                        Dim strSql3 As New Text.StringBuilder
                                        strSql3.Append("update tblEmailActivityLog set cActivityXml = '" & SqlFmt(oXform.Instance.OuterXml) & "'")
                                        strSql3.Append("where nEmailActivityKey = " & EmailActivityId)
                                        myWeb.moDbHelper.ExeProcessSql(strSql3.ToString)

                                    End If
                                End If

                                oXform.addValues()

                                oElmt.InnerXml = oXform.moXformElmt.InnerXml
                                oXform = Nothing

                            Next
                            'Load Application Form
                        Else
                            cProcessInfo = "only run on content detail"
                            'do nothing
                        End If
                    End If


                    'Dim adXfm As Object = myWeb.getAdminXform()
                    'Dim oXfmElmt As XmlElement

                    'oContentNode.InnerXml = oXfmElmt.InnerXml

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""))
                End Try
            End Sub

        End Class

#End Region

        Protected Overrides Sub Finalize()
            MyBase.Finalize()
        End Sub
    End Class
End Class
