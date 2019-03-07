<%@ WebHandler Language="VB" Class="feed" %>

Imports System
Imports System.Web

Public Class feed : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Protean.Cms = New Protean.Cms

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        Else
            oEw.mcContentType = "application/rss+xml"
        End If

        'oEw.mbFeedMode = True
		oEw.InitializeVariables()
		
		If IO.File.Exists(context.Server.MapPath("/xsl/feeds/rss/feed.xsl")) Then
			oEw.mcEwSiteXsl = "/xsl/feeds/rss/feed.xsl"
		Else
			oEw.mcEwSiteXsl = "/ewcommon/xsl/Feeds/rss/feed.xsl"
		End If
		
		
        oEw.GetPageHTML()
        
        oEw = Nothing
    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class