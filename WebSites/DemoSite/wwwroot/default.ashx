<%@ WebHandler Language="VB" Class="ewDefault" %>

Imports System
Imports System.Web
Imports System.Web.SessionState


Public Class ewDefault : Implements IHttpHandler, IRequiresSessionState
    Dim WithEvents oEw As Eonic.Web
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        
        oEw = New Eonic.Web
                
        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If

        oEw.InitializeVariables()
        
        
        
        
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