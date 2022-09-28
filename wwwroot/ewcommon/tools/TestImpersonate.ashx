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

        context.Response.Write("Testing Impersonation: User:" & oEw.moConfig("AdminDomain") & "/" & oEw.moConfig("AdminAcct"))
        context.Response.Write("<br/>")
        Dim AdminGroup = "Protean Users" 'oEw.moConfig("AdminGroup")
        context.Response.Write("Testing Impersonation: Admin Group:" & AdminGroup)
        Dim Execution_Start As New System.Diagnostics.Stopwatch
        Execution_Start.Start()
        Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
        If oImp.ImpersonateValidUser(oEw.moConfig("AdminAcct"), oEw.moConfig("AdminDomain"), oEw.moConfig("AdminPassword"), , AdminGroup) Then

            context.Response.Write("<br/>")
            context.Response.Write("impersonation successful - time:" & Execution_Start.Elapsed.ToString())

            oImp.UndoImpersonation()
            oImp = Nothing



        Else

            context.Response.Write("impersonation failed")
        End If


    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class