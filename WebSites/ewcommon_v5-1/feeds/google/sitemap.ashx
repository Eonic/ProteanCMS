<%@ WebHandler Language="VB" Class="google_sitemap" %>

Imports System
Imports System.Web
Imports System.xml

Public Class google_sitemap : Implements IHttpHandler, IRequiresSessionState
    
	Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
             
            Dim oEw As eonic.Web = New eonic.Web
           
            oEw.InitializeVariables()
            
		If IO.File.Exists(context.Server.MapPath("/xsl/feeds/google/sitemap.xsl")) Then
			oEw.mcEwSiteXsl = "/xsl/feeds/google/sitemap.xsl"
		Else
			oEw.mcEwSiteXsl = "/ewcommon/xsl/feeds/google/sitemap.xsl"
		End If
 
            context.Response.ContentType = "text/xml"
             
            'TODO: Showing Content on GoogleSitemap
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