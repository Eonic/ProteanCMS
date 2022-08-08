<%@ WebHandler Language="VB" Class="ewTidyTest" %>

Imports System
Imports System.Web

Public Class ewTidyTest : Implements IHttpHandler, IRequiresSessionState

    Public Shared Function GetRandomAlphaNumeric(ByVal count As Integer) As String
        Dim s As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
        Dim r As New Random
        Dim sb As New StringBuilder
        For i As Integer = 1 To count
            Dim idx As Integer = r.Next(0, 35)
            sb.Append(s.Substring(idx, 1))
        Next
        Return sb.ToString()
    End Function

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim htmltotest As String = "<h1>Test Alpha FS Write</h1>"
        context.Response.ContentType = "text/html"

        Dim oEw As Protean.Cms = New Protean.Cms
        oEw.InitializeVariables()
        Dim filepath As String = "ewCache\FS-Alpha-TEST " & GetRandomAlphaNumeric(230) & ".html"

        Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
        If oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), , oEw.moConfig("AdminGroup")) Then


            'System.IO.File.WriteAllText("\\?\" & oEw.goServer.MapPath("/" & oEw.gcProjectPath) & filepath, htmltotest, System.Text.Encoding.UTF8)

            Alphaleonis.Win32.Filesystem.File.WriteAllText("\\?\" & oEw.goServer.MapPath("/" & oEw.gcProjectPath) & filepath, htmltotest, System.Text.Encoding.UTF8)

            oImp.UndoImpersonation()
            oImp = Nothing


            context.Response.Write("file written - " & filepath)

            '   If oFS.VirtualFileExistsAndRecent(FullFilePath, 10) Then

            'Else
            '   cProcessInfo &= "<Error>Create Path: " & filepath & " - " & sError & "</Error>" & vbCrLf
            '  Err.Raise(1001, "File not saved", cProcessInfo)
            '   End If
        Else
            ' Response.write() "<Error>Create File: " & filepath & " - " & sError & "</Error>" & vbCrLf)

            context.Response.Write("impersonation failed")
        End If


    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class