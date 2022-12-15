Option Strict On
Option Explicit On
Imports System.Xml
Imports System.Web.Configuration
Imports System.Configuration
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System.IO
Imports Microsoft.ClearScript.Util

Public Class Tests

    Public goApp As System.Web.HttpApplicationState
    Public goRequest As System.Web.HttpRequest
    Public goResponse As System.Web.HttpResponse
    Public goSession As System.Web.SessionState.HttpSessionState
    Public goServer As System.Web.HttpServerUtility

    Public goConfig As System.Collections.Specialized.NameValueCollection = CType(WebConfigurationManager.GetWebApplicationSection("protean/web"), System.Collections.Specialized.NameValueCollection)
    Public moCtx As System.Web.HttpContext

    Public Sub New()

        Me.New(System.Web.HttpContext.Current)

    End Sub
    Public Sub New(oCtx As System.Web.HttpContext)

        moCtx = oCtx

    End Sub

    Public Function TestEmailSend() As String
        Try
            Dim errMsg As String = ""

            Dim oMsg As New Protean.Messaging(errMsg)

            Dim oBodyXml As New XmlDocument
            oBodyXml.LoadXml("<Items><Name>ProteanCMS Test</Name><Telephone /><Email>" & goConfig("SiteAdminEmail") & "</Email><Message>This is a test</Message></Items>")
            Dim emailerMsg As String
            Dim bodyXml As XmlElement = CType(oBodyXml.FirstChild, XmlElement)
            Dim emailXSL As String = "/ptn/core/email/mailform.xsl"
            If goConfig("cssFramework") <> "bs5" Then
                emailXSL = "/ewcommon/xsl/email/mailform.xsl"
            End If

            emailerMsg = CType(oMsg.emailer(bodyXml, emailXSL, "ProteanCMS Test", goConfig("SiteAdminEmail"), goConfig("SiteAdminEmail"), "This is a TEST").ToString, String)

            If emailerMsg <> "" Then
                Return emailerMsg
            Else
                Return "1"
            End If
        Catch ex As Exception
            Return ex.Message
        End Try
    End Function


    Public Function TestWriteFile() As String
        Try
            Dim htmltotest As String = "<h1>Test Write</h1>"

            Dim oEw As Protean.Cms = New Protean.Cms
            oEw.InitializeVariables()
            Dim filepath As String = "/ewCache/"
            Dim filename As String = "FS-TEST.html"
            Dim oImp As Protean.Tools.Security.Impersonate = Nothing
            If oEw.moConfig("AdminAcct") <> "" Then
                oImp = New Protean.Tools.Security.Impersonate
                If oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), , oEw.moConfig("AdminGroup")) Then
                Else
                    Return "Impersonation Failed"
                End If
            End If

            oEw.moFSHelper.SaveFile(filename, oEw.goServer.MapPath("/" & oEw.gcProjectPath) & filepath, System.Text.Encoding.Unicode.GetBytes(htmltotest))

            Return "File Written Using SaveFile :" & filepath & filename

            If oEw.moConfig("AdminAcct") <> "" Then
                oImp.UndoImpersonation()
                oImp = Nothing
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function TestDeleteFile() As String
        Try

            Dim oEw As Protean.Cms = New Protean.Cms
            oEw.InitializeVariables()
            Dim filepath As String = "\ewCache\"
            Dim filename As String = "FS-TEST.html"
            Dim oImp As Protean.Tools.Security.Impersonate = Nothing
            If oEw.moConfig("AdminAcct") <> "" Then
                oImp = New Protean.Tools.Security.Impersonate
                If oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), , oEw.moConfig("AdminGroup")) Then
                Else
                    Return "Impersonation Failed"
                End If
            End If
            Dim response As String

            response = oEw.moFSHelper.DeleteFile(oEw.goServer.MapPath("/" & oEw.gcProjectPath) & filepath, filename)

            oEw.moFSHelper.DeleteFile(oEw.goServer.MapPath("/" & oEw.gcProjectPath) & filepath, "FS-Alpha-TEST.html")

            If oEw.moConfig("AdminAcct") <> "" Then
                oImp.UndoImpersonation()
                oImp = Nothing
            End If

            If response = "1" Then
                Return "File Deleted:" & filepath & filename
            Else
                Return response
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function TestImpersonation() As String
        Try

            Dim oEw As Protean.Cms = New Protean.Cms
            oEw.InitializeVariables()

            Dim oImp As Protean.Tools.Security.Impersonate = Nothing
            If oEw.moConfig("AdminAcct") <> "" Then
                oImp = New Protean.Tools.Security.Impersonate
                If oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), , oEw.moConfig("AdminGroup")) Then
                    Return "Impersonation Success"
                Else
                    Return "Error Impersonation Failed"
                End If
            Else
                Return "Impersonation Disabled"
            End If

            If oEw.moConfig("AdminAcct") <> "" Then
                oImp.UndoImpersonation()
                oImp = Nothing
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function TestWriteFileAlphaFS() As String
        Try
            Dim htmltotest As String = "<h1>Test Alpha FS Write</h1>"

            Dim oEw As Protean.Cms = New Protean.Cms
            oEw.InitializeVariables()
            Dim filepath As String = "/ewCache/FS-Alpha-TEST.html"
            Dim oImp As Protean.Tools.Security.Impersonate = Nothing
            If oEw.moConfig("AdminAcct") <> "" Then
                oImp = New Protean.Tools.Security.Impersonate
                If oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), , oEw.moConfig("AdminGroup")) Then
                Else
                    Return "Impersonation Failed"
                End If
            End If

            Alphaleonis.Win32.Filesystem.File.WriteAllText("\\?\" & oEw.goServer.MapPath("/" & oEw.gcProjectPath) & filepath, htmltotest, System.Text.Encoding.UTF8)

            Return "File Written Using AlphaFS :" & filepath

            If oEw.moConfig("AdminAcct") <> "" Then
                oImp.UndoImpersonation()
                oImp = Nothing
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function


    Public Function TestCreateFolder() As String
        Try

            Dim oEw As Protean.Cms = New Protean.Cms
            oEw.InitializeVariables()
            Dim filepath As String = "/ewCache/Testfolder/test2"

            Dim response As String = oEw.moFSHelper.CreatePath(filepath)
            If response = "1" Then
                Return "Folder Created:" & filepath
            Else
                Return response
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function


    Public Function TestDeleteFolder() As String
        Try

            Dim oEw As Protean.Cms = New Protean.Cms
            oEw.InitializeVariables()
            Dim filepath As String = "/ewCache/Testfolder/test2"

            Dim response As String = oEw.moFSHelper.DeleteFolder("Testfolder", oEw.goServer.MapPath("/ewCache/"))
            If response = "1" Then
                Return "Folder Deleted:" & filepath
            Else
                Return response
            End If

        Catch ex As Exception
            Return ex.Message
        End Try
    End Function

    Public Function TestHtmlTidy() As String
        Try


            Dim htmltotest As String = "<h1>HTMLTidy is Tidying</H1>"

            Dim sResponse As String = Protean.Tools.Text.tidyXhtmlFrag(htmltotest, True, True, "")

            If sResponse.StartsWith("<h1>HTMLTidy is Tidying</h1>") Then
                sResponse = "HTML Tidy is working"
            End If

            Return sResponse



        Catch ex As Exception
            Return ex.Message & "<br/>If server is 64 bit and failed to load tidy.dll reference and broke functionality. Get dll from below link with the version you want http://binaries.html-tidy.org/ Please fllow steps- Put tidyX86.dll, tidyX64.dll and tidy.dll in C:\Windows\System32 and it will start working."
        End Try
    End Function

End Class
