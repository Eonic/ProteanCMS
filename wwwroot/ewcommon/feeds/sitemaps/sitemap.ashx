<%@ WebHandler Language="VB" Class="sitemapsOrg_sitemap" %>

Imports System
Imports System.Web
Imports System.xml

Public Class sitemapsOrg_sitemap : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest



        Dim oEw As Protean.Cms = New Protean.Cms

        oEw.InitializeVariables()
        oEw.Open()

        oEw.mcEwSiteXsl = "/ewcommon/xsl/feeds/sitemaps/sitemap.xsl"

        context.Response.ContentType = "application/xml"
        oEw.mcContentType = "application/xml"

        'TODO: Showing Content on sitemapsOrgSitemap
        If Not oEw.moConfig("GoogleContentTypes") = "" Then
            oEw.GetPageXML()
            oEw.addPageDetailLinksToStructure(oEw.moConfig("GoogleContentTypes"))
        End If

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If

        oEw.GetPageHTML()

        oEw = Nothing

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

End Class