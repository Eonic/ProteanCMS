<%@ WebHandler Language="VB" Class="ewAjaxAdmin" %>

Imports System
Imports System.Web
Imports System.Web.Configuration
Imports System.Xml

Public Class ewAjaxAdmin : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim moPolicy As xmlElement
        
        moPolicy = WebConfigurationManager.GetWebApplicationSection("eonic/PasswordPolicy")
        
        context.Response.Write("<?xml version=""1.0"" encoding=""utf-8"" ?>")
        context.Response.Write("<?xml-stylesheet type=""text/xsl"" href=""/ewcommon/xsl/tools/PasswordPolicy.xslt""?>")
        context.Response.Write(moPolicy.OuterXml)

    End Sub   
    
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class