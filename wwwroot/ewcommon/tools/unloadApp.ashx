<%@ WebHandler Language="VB" Class="ewUploadFiles" %>

Imports System
Imports System.Web
Imports System.Web.SessionState

Public Class ewUploadFiles : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest


        AppDomain.Unload(AppDomain.CurrentDomain)
        System.Web.HttpRuntime.UnloadAppDomain()

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class
