Option Strict Off
Option Explicit On


Imports System.Web.Configuration
Imports System.IO
Imports System.Reflection
Imports System.Linq
Imports System.Collections.Generic
Imports System.Text

Public Class API
    Inherits Base

    Public gbDebug As Boolean = False

    Public Sub New()

        MyBase.New(System.Web.HttpContext.Current)
        InitialiseVariables()

    End Sub

    Public Sub InitialiseVariables()
        PerfMon.Log("API", "Open")
        Dim sProcessInfo As String = ""
        Dim cCloneContext As String = ""
        Dim rootPageIdFromConfig As String = ""
        Try

            'if we access base via soap the session is not available
            If Not moSession Is Nothing Then

                'below code has beem moved to membership base provider

                Dim oMembershipProv As New Providers.Membership.BaseProvider(Me, moConfig("MembershipProvider"))
                mnUserId = oMembershipProv.Activities.GetUserId(Me)

                If moSession("adminMode") = "true" Then
                    mbAdminMode = True
                    ' moDbHelper.gbAdminMode = mbAdminMode
                End If
            End If

            'We need the userId placed into dbhelper.
            'moDbHelper.mnUserId = mnUserId

            If Not (moConfig("Debug") Is Nothing) Then
                Select Case LCase(moConfig("Debug"))
                    Case "on" : gbDebug = True
                    Case "off" : gbDebug = False
                    Case Else : gbDebug = False
                End Select
            End If


        Catch ex As Exception

            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Open", ex, sProcessInfo))

        End Try

    End Sub

    Public Overridable Sub JSONRequest()

        PerfMon.Log("API", "Request")
        Dim sProcessInfo As String = ""
        Try

            Dim path As String = moRequest.ServerVariables("HTTP_X_ORIGINAL_URL")

            If path.Contains("?") Then
                path = path.Substring(0, path.IndexOf("?"))
            End If

            Dim pathsplit() As String = Split(path, "/")
            'URL = /API/ProviderName/methodName

            Dim ProviderName As String = pathsplit(2)
            Dim methodName As String = pathsplit(3)
            Dim classPath As String = ProviderName & ".JSONActions"
            Dim assemblytype As String = ""

            Dim s As Stream = moRequest.InputStream
            Dim sr As New StreamReader(s)
            Dim jsonString As String = sr.ReadLine()
            If jsonString = Nothing Then
                jsonString = moRequest("data")
            End If

            Dim jObj As Newtonsoft.Json.Linq.JObject = Nothing
            Dim paramDictionary As Dictionary(Of String, String) = Nothing
            If Not jsonString Is Nothing Then
                Try
                    jObj = Newtonsoft.Json.Linq.JObject.Parse(jsonString)
                Catch ex As Exception
                    'Not a valid json string
                    Dim query As String = System.Web.HttpUtility.UrlDecode(jsonString)
                    Dim formData As System.Collections.Specialized.NameValueCollection = System.Web.HttpUtility.ParseQueryString(query)
                    paramDictionary = formData.AllKeys.ToDictionary(Function(k) k, Function(k) formData(k))
                End Try
            End If

            Dim calledType As Type

            If LCase(ProviderName) = "cms.cart" Or LCase(ProviderName) = "cms.content" Or LCase(ProviderName) = "cms.admin" Then ProviderName = ""

            If ProviderName <> "" Then
                'case for external Providers
                Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders")
                Dim assemblyInstance As [Assembly] = [Assembly].LoadFrom(goServer.MapPath(moPrvConfig.Providers(ProviderName).Parameters("path")))
                'Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(ProviderName).Type)
                classPath = moPrvConfig.Providers(ProviderName).Parameters("className") & ".JSONActions"
                calledType = assemblyInstance.GetType(classPath, True)
            Else
                'case for methods within ProteanCMS Core DLL
                calledType = System.Type.GetType("Protean." & Replace(classPath, ".", "+"), True)
            End If

            Dim o As Object = Activator.CreateInstance(calledType)

            Dim args(1) As Object
            args(0) = Me
            If Not jObj Is Nothing Then
                args(1) = jObj
            ElseIf Not paramDictionary Is Nothing Then
                args(1) = paramDictionary
            Else
                args(1) = Nothing
            End If

            'check the response whatever is coming like with code 400, 200, based on the output- return in Json

            Dim myResponse As String = calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, Nothing, o, args)

            moResponse.Write(myResponse)

        Catch ex As Exception
            OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "JSONRequest", ex, sProcessInfo))
            'returnException(mcModuleName, "getPageHtml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
            '
            moResponse.Write(ex.Message)
            Me.Finalize()
        Finally

        End Try
        PerfMon.Write()
    End Sub


    Public Class JsonActions

        Public Function ValidateAPICall(ByRef myWeb As Cms, ByVal sGroupName As String) As Boolean
            'Create -InsertOrder Group and pass as a input
            ' check user present in the group
            Dim bIsAuthorized As Boolean = False
            Dim authHeader As String = String.Empty
            Dim encodedUsernamePassword As String = String.Empty
            Dim usernamePassword As String = String.Empty
            Dim encoding As Encoding = Encoding.GetEncoding("iso-8859-1")
            Dim seperatorIndex As Integer
            Dim username As String = String.Empty
            Dim password As String = String.Empty
            Dim nUserId As Integer
            Dim sValidResponse As String = String.Empty

            Try
                'HttpContext httpContext = HttpContext.Current;
                If myWeb.moCtx.Request.Headers IsNot Nothing Then
                    If myWeb.moCtx.Request.Headers("Authorization") IsNot Nothing Then
                        authHeader = myWeb.moCtx.Request.Headers("Authorization")
                        If authHeader.Substring("Basic ".Length).Trim().Length <> 0 Then
                            encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim()
                            usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword))
                            seperatorIndex = usernamePassword.IndexOf(":")
                            username = usernamePassword.Substring(0, seperatorIndex)
                            password = usernamePassword.Substring(seperatorIndex + 1)
                            sValidResponse = myWeb.moDbHelper.validateUser(username, password)
                            If IsNumeric(sValidResponse) Then
                                nUserId = CLng(sValidResponse)
                                bIsAuthorized = myWeb.moDbHelper.checkUserRole(sGroupName, "Group", nUserId)
                                If (bIsAuthorized) Then
                                    myWeb.mnUserId = nUserId
                                End If
                            End If
                        Else
                            bIsAuthorized = False
                        End If

                    Else
                        bIsAuthorized = False
                    End If
                End If


            Catch ex As Exception
                ' OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs("API", "ValidateAPICall", ex, ""))

                Return False
            End Try
            Return bIsAuthorized
        End Function


    End Class
End Class
