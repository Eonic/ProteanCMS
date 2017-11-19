<%@ WebHandler Language="VB" Class="ewNewsletterDefault" %>

Imports System
Imports System.Web
Imports System.Web.SessionState
Imports System.Web.Configuration

Public Class ewNewsletterDefault : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        dim moConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")

        Dim oEw As eonic.Web = New eonic.Web

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If
        oEw.InitializeVariables()
        oEw.Open()
        oEw.mcEwSiteXsl = moConfig("MailingXsl")
        oEw.mnMailMenuId = moConfig("RootPageId")
        context.Response.ContentType = "text/html"
        oEw.GetPageHTML()

        oEw = Nothing

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

End Class