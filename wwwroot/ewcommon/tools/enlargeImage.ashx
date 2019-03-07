<%@ WebHandler Language="VB" Class="ewEnlargeImage" %>

Imports System
Imports System.Web

Public Class ewEnlargeImage : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Protean.Cms = New Protean.Cms
        oEw.InitializeVariables()
        oEw.mcEwSiteXsl = "/ewcommon/xsl/tools/imgenlarge.xsl"

        context.Response.ContentType = "text/html"
        oEw.GetPageHTML()
        oEw = Nothing

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class