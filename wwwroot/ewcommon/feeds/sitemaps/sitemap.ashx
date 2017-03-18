<%@ WebHandler Language="VB" Class="sitemapsOrg_sitemap" %>

Imports System
Imports System.Web
Imports System.xml

Public Class sitemapsOrg_sitemap : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
       
            
        
        Dim oEw As Eonic.Web = New Eonic.Web
           
        oEw.InitializeVariables()
        oEw.mcEwSiteXsl = "/ewcommon/xsl/feeds/sitemaps/sitemap.xsl"
        
        context.Response.ContentType = "text/xml"
             
        'TODO: Showing Content on sitemapsOrgSitemap
        If Not oEw.moConfig("sitemapsOrgContentTypes") = "" Then
            oEw.GetPageXML()
            oEw.addPageDetailLinksToStructure(oEw.moConfig("sitemapsOrgContentTypes"))
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