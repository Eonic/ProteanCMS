<%@ WebHandler Language="VB" Class="AjaxProxy" %>

Imports System
Imports System.Web
Imports System.Net

Public Class AjaxProxy : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        context.Response.ContentType = "text/plain"

        Using client As New WebClient()
            context.Response.BinaryWrite(client.DownloadData(context.Request.QueryString("url")))
        End Using

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class