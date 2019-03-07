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
            oEw.LogSingleUserSession()
            oEw.Close()
        End If
        oEw = Nothing

        context.Response.ContentType = "text/html"
        context.Response.AddHeader("Refresh", Convert.ToString((context.Session.Timeout * 60) - 120))
        ' This overcomes a problem with zero length content not giving out a content type, which can cause browser to treat the file as a download
        context.Response.Write(" ")
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class