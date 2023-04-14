<%@ WebHandler Language="VB" Class="setup" %>

Imports System
Imports System.Web
Imports System.Web.SessionState
Imports System.Web.Configuration
Imports System.Configuration
Imports System.DirectoryServices
Imports System.Security.Principal
Imports Microsoft.Web.Administration

Public Class setup : Implements IHttpHandler, IRequiresSessionState

    Public goConfig As System.Collections.Specialized.NameValueCollection = System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection("protean/web")
    Public goRequest As System.Web.HttpRequest
    Public goResponse As System.Web.HttpResponse
    Public oContext As HttpContext
    Public goServer As System.Web.HttpServerUtility

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        oContext = context
        goRequest = oContext.Request
        goResponse = oContext.Response
        goServer = oContext.Server
        createVirtualDirectories()
    End Sub

    Public Sub createVirtualDirectories()

        Dim ServerName As String = goConfig("ServerName")
        If ServerName = "" Then ServerName = "localhost"
        Dim Path As String = "IIS://" & ServerName & "/W3SVC"
        Dim CommonDirectoryPath As String = goConfig("V6CommonDirectoryPath")

        If CommonDirectoryPath = "" Then CommonDirectoryPath = "c:\HostingSpaces\ptn"

        Try
            goResponse.Write("Connecting Common Directory... " & CommonDirectoryPath)
            Dim oImp As Impersonate = New Impersonate
            If oImp.ImpersonateValidUser(goConfig("SetupAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), True, goConfig("AdminGroup")) Then

                goResponse.Write("<br/>Logon Successful... " & goConfig("SetupAcct"))

                'delete default files
                Dim defaultFile As String = goServer.MapPath("/") & "default.ashx"

                If System.IO.File.Exists(defaultFile) Then
                    System.IO.File.Delete(defaultFile)
                    goResponse.Write("<br/>Default File Deleted... ")
                End If

                defaultFile = goServer.MapPath("/") & "default.aspx"

                If System.IO.File.Exists(defaultFile) Then
                    System.IO.File.Delete(defaultFile)
                    goResponse.Write("<br/>Default File Deleted... ")
                End If

                Dim siteName As String = goRequest.ServerVariables("SERVER_NAME")
                Dim prepropurl As String = goConfig("PrePropUrl")

                If prepropurl = "" Then
                    siteName = Replace(goRequest.ServerVariables("SERVER_NAME"), ".web01.eonichost.co.uk", "")
                    siteName = Replace(siteName, ".web02.eonichost.co.uk", "")
                    siteName = Replace(siteName, ".web03.eonichost.co.uk", "")
                    siteName = Replace(siteName, ".web05.eonichost.co.uk", "")
                    siteName = Replace(siteName, ".app01.eonichost.co.uk", "")
                    siteName = Replace(siteName, ".preview.succinctdigital.co.uk", "")
                    siteName = Replace(siteName, "www.", "")
                Else
                    Dim prepropurl2 As String
                    For Each prepropurl2 In prepropurl.Split()
                        siteName = Replace(siteName, prepropurl2, "")
                    Next
                End If



                goResponse.Write("<br/>Sitename: " & siteName)

                Dim iisManager As ServerManager = New ServerManager()
                goResponse.Write("<br/>Servermanager created: " & siteName)
                Dim app As Application

                If iisManager.Sites(siteName) Is Nothing Then
                    siteName = Replace(siteName, "www.", "")
                End If
                If iisManager.Sites(siteName) Is Nothing Then
                    siteName = Replace(siteName, "demo.", "")
                End If
                If iisManager.Sites(siteName) Is Nothing Then
                    goResponse.Write("<br/>Error Sitename: " & siteName & " could not be found try an alternative URL.")
                Else
                    app = iisManager.Sites(siteName).Applications("/")
                    Try
                        If iisManager.Sites(siteName).Applications("/ptn") Is Nothing Then
                            goResponse.Write("<br/>Creating Common Directory... " & CommonDirectoryPath)
                            Dim oApp As Application = iisManager.Sites(siteName).Applications.Add("/ptn", CommonDirectoryPath)
                            oApp.ApplicationPoolName = "ProteanCMS"

                            '  goResponse.Write("<br/>Creating Freestock Directory... ")
                            '  Dim rootApp As Application = iisManager.Sites(siteName).Applications(0)
                            '  rootApp.VirtualDirectories.Add("/images/FreeStock", "d:\web\freestock\freestock")

                            iisManager.CommitChanges()

                        Else
                            goResponse.Write("<br/>ERROR Common Directory allready Exists")
                        End If
                        goResponse.Write("<html><head><meta http-equiv=""refresh"" content=""2;url=/ewcommon/setup""></head><body><p>Redirecting to setup...</p></body></html>")
                    Catch ex As Exception
                        goResponse.Write("<h1>" & ex.Message & "</h1>")
                    End Try
                End If
                oImp.UndoImpersonation()
            Else
                goResponse.Write("Impersonation Failed " & goConfig("SetupAcct") & "/" & goConfig("AdminDomain") & "/" & goConfig("AdminPassword"))
            End If


        Catch Ex As Exception
            goResponse.Write("<br/><br/>SETUP ERROR: " & Ex.Message & "<br/>" & Ex.StackTrace)
        End Try


    End Sub

    Public Sub CreateVirtualDir(ByVal ServerName As String, ByVal SiteId As Int32, ByVal AppName As String, ByVal Path As String)

        Dim oImp As Impersonate = New Impersonate
        If oImp.ImpersonateValidUser(goConfig("AdminAcct"), goConfig("AdminDomain"), goConfig("AdminPassword"), True, goConfig("AdminGroup")) Then

            Dim IISSchema As New System.DirectoryServices.DirectoryEntry("IIS://" & ServerName & "/Schema/AppIsolated")

            Dim CanCreate As Boolean = Not IISSchema.Properties("Syntax").Value.ToString.ToUpper() = "BOOLEAN"
            IISSchema.Dispose()
            If CanCreate Then
                Dim PathCreated As Boolean
                Try
                    Dim IISAdmin As New System.DirectoryServices.DirectoryEntry("IIS://" & ServerName & "/W3SVC/" & SiteId & "/Root")
                    'make sure folder exists                
                    If Not System.IO.Directory.Exists(Path) Then
                        System.IO.Directory.CreateDirectory(Path)
                        PathCreated = True
                    End If
                    'If the virtual directory already exists then delete it                
                    For Each VD As System.DirectoryServices.DirectoryEntry In IISAdmin.Children
                        If VD.Name = AppName Then
                            IISAdmin.Invoke("Delete", New String() {VD.SchemaClassName, AppName})
                            IISAdmin.CommitChanges()
                            Exit For
                        End If
                    Next VD
                    'Create and setup new virtual directory                
                    Dim VDir As System.DirectoryServices.DirectoryEntry = IISAdmin.Children.Add(AppName, "IIsWebVirtualDir")
                    VDir.Properties("Path").Item(0) = Path
                    VDir.Properties("AppFriendlyName").Item(0) = AppName
                    VDir.Properties("EnableDirBrowsing").Item(0) = False
                    VDir.Properties("AccessRead").Item(0) = True
                    VDir.Properties("AccessExecute").Item(0) = True
                    VDir.Properties("AccessWrite").Item(0) = False
                    VDir.Properties("AccessScript").Item(0) = True
                    VDir.Properties("AuthNTLM").Item(0) = True
                    VDir.Properties("EnableDefaultDoc").Item(0) = True
                    VDir.Properties("DefaultDoc").Item(0) = "default.ashx"
                    VDir.Properties("AspEnableParentPaths").Item(0) = True
                    VDir.Properties("AppPoolId").Item(0) = "ProteanCMS"
                    VDir.CommitChanges()
                    'the following are acceptable params                
                    'INPROC = 0                
                    'OUTPROC = 1                
                    'POOLED = 2                
                    VDir.Invoke("AppCreate", 1)
                Catch Ex As Exception
                    If PathCreated Then
                        System.IO.Directory.Delete(Path)
                    End If
                    goResponse.Write(Ex.InnerException)
                End Try
            End If
        End If
        oImp.UndoImpersonation()

    End Sub


    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property


    Public Class Impersonate

        Private Const LOGON32_PROVIDER_DEFAULT As Integer = 0
        Private Const LOGON32_LOGON_INTERACTIVE As Integer = 2
        Private Const LOGON32_LOGON_NETWORK As Integer = 3
        Private Const LOGON32_LOGON_BATCH As Integer = 4
        Private Const LOGON32_LOGON_SERVICE As Integer = 5
        Private Const LOGON32_LOGON_UNLOCK As Integer = 7
        Private Const LOGON32_LOGON_NETWORK_CLEARTEXT As Integer = 8
        Private Const LOGON32_LOGON_NEW_CREDENTIALS As Integer = 9

        Private Shared ImpersonationContext As WindowsImpersonationContext

        Declare Function LogonUserA Lib "advapi32.dll" ( _
                                ByVal lpszUsername As String, _
                                ByVal lpszDomain As String, _
                                ByVal lpszPassword As String, _
                                ByVal dwLogonType As Integer, _
                                ByVal dwLogonProvider As Integer, _
                                ByRef phToken As IntPtr) As Integer

        Declare Auto Function DuplicateToken Lib "advapi32.dll" ( _
                                ByVal ExistingTokenHandle As IntPtr, _
                                ByVal ImpersonationLevel As Integer, _
                                ByRef DuplicateTokenHandle As IntPtr) As Integer
        Declare Auto Function RevertToSelf Lib "advapi32.dll" () As Long
        Declare Auto Function CloseHandle Lib "kernel32.dll" (ByVal handle As IntPtr) As Long


        ' NOTE:
        ' The identity of the process that impersonates a specific user on a thread must have 
        ' "Act as part of the operating system" privilege. If the the Aspnet_wp.exe process runs
        ' under a the ASPNET account, this account does not have the required privileges to 
        ' impersonate a specific user. This information applies only to the .NET Framework 1.0. 
        ' This privilege is not required for the .NET Framework 1.1.
        '
        ' Sample call:
        '
        '    If impersonateValidUser("username", "domain", "password") Then
        '        'Insert your code here.
        '
        '        undoImpersonation()
        '    Else
        '        'Impersonation failed. Include a fail-safe mechanism here.
        '    End If
        '
        Public Function ImpersonateValidUser(ByVal strUserName As String, _
                    ByVal strDomain As String, _
                    ByVal strPassword As String, _
                    Optional ByVal bCheckAdmin As Boolean = False, _
                    Optional ByVal cInGroup As String = "") As Boolean

            'PerfMon.Log("Impersonate", "ImpersonateValidUser")
            Dim token As IntPtr = IntPtr.Zero
            Dim tokenDuplicate As IntPtr = IntPtr.Zero
            Dim tempWindowsIdentity As WindowsIdentity
            Dim isDomAdmin As Boolean = False
            If cInGroup = "" Then cInGroup = "Domain Admins"
            ImpersonateValidUser = False

            ' bCheckAdmin seems to be a bit of a hangover - it's a white elephant, so for now, I'll ignore it
            Try


                If RevertToSelf() <> 0 Then
                    If LogonUserA(strUserName, strDomain, strPassword, _
                       LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, token) <> 0 Then
                        If DuplicateToken(token, 2, tokenDuplicate) <> 0 Then
                            tempWindowsIdentity = New WindowsIdentity(tokenDuplicate)

                            If strDomain <> "" Then
                                isDomAdmin = New WindowsPrincipal(tempWindowsIdentity).IsInRole(strDomain & "\" & cInGroup)
                            Else
                                isDomAdmin = New WindowsPrincipal(tempWindowsIdentity).IsInRole(cInGroup)
                            End If

                            ImpersonationContext = tempWindowsIdentity.Impersonate()
                            If Not (ImpersonationContext Is Nothing) Then
                                ''if we are checking for admin then return that value
                                'If bCheckAdmin Then
                                '    ImpersonateValidUser = True
                                'Else
                                '    Return isDomAdmin
                                'End If

                                Return isDomAdmin
                            End If
                        End If
                    Else
                        Return False
                    End If
                End If

                If Not tokenDuplicate.Equals(IntPtr.Zero) Then
                    CloseHandle(tokenDuplicate)
                End If

                If Not token.Equals(IntPtr.Zero) Then
                    CloseHandle(token)
                End If
            Catch ex As Exception

                Return False
            End Try
        End Function

        Public Sub UndoImpersonation()
            ImpersonationContext.Undo()
        End Sub
    End Class
End Class
