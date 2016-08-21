<%@ WebHandler Language="VB" Class="adminDefault" %>

Imports System
Imports System.Web
Imports System.Web.SessionState
Imports Eonic


Public Class adminDefault : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.Redirect("/?ewcmd=admin")
    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class