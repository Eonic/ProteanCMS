<%@ WebHandler Language="VB" Class="keepalive" %>

Imports System
Imports System.Web

Public Class keepalive : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        ' Load Eonic Web
        Dim oEw As Protean.Cms = New Protean.Cms
        oEw.InitializeVariables()
        If oEw.gbSingleLoginSessionPerUser Then
            oEw.Open()
            oEw.mbSuppressLastPageOverrides = True
            oEw.LogSingleUserSession()
            oEw.Close()
        End If
        oEw = Nothing

        Dim timeoutSec As String = Convert.ToString(((context.Session.Timeout / 2) * 60) - 60)

        context.Response.ContentType = "text/html"
        context.Response.AddHeader("Refresh", timeoutSec)
        ' This overcomes a problem with zero length content not giving out a content type, which can cause browser to treat the file as a download
        context.Response.Write("refresh every " + timeoutSec + "secs")
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class