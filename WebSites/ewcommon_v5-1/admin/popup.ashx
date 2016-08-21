<%@ WebHandler Language="VB" Class="popup" %>

Imports System
Imports System.Web
Imports System.Web.SessionState
Imports Eonic

Public Class popup : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "text/html"
        Dim oEw As Eonic.Web = New Eonic.Web
                        
        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If

		oEw.InitializeVariables()
        If IO.File.Exists(context.Server.MapPath("/xsl/admin/AdminPopups.xsl")) Then
            oEw.mcEwSiteXsl = "/xsl/admin/AdminPopups.xsl"
        Else
            oEw.mcEwSiteXsl = "/ewcommon/xsl/admin/AdminPopups.xsl"
        End If
  
        oEw.mbPopupMode = True
        oEw.mbAdminMode = True

        oEw.GetPageHTML()

        oEw = Nothing
    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class