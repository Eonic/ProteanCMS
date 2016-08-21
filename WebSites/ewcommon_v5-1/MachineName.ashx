<%@ WebHandler Language="VB" Class="Handler" %>

Imports System
Imports System.Web

Imports System.Reflection

Public Class Handler : Implements IHttpHandler
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "text/plain"
               
        
        context.Response.Write(Environment.MachineName & " - " & context.Request.ServerVariables("LOCAL_ADDR"))

    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class