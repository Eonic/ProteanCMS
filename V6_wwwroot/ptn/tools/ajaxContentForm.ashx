<%@ WebHandler Language="VB" Class="ewAjaxContentForm" %>

Imports System
Imports System.Web
Imports System.Collections.Specialized
Imports System.Web.Configuration


Public Class ewAjaxContentForm : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim moConfig As NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")

        Dim oEw As Protean.Cms = New Protean.Cms
        oEw.InitializeVariables()
        oEw.Open()

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If
        If (moConfig("AjaxXsl") <> "") Then
            oEw.mcEwSiteXsl = moConfig("AjaxXsl")
        Else
            oEw.mcEwSiteXsl = "/ptn/core/ajax.xsl"
        End If
        oEw.GetAjaxHTML()

        oEw = Nothing
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class