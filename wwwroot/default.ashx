<%@ WebHandler Language="VB" Class="ewDefault" %>

Imports System
Imports System.Web
Imports System.Web.SessionState


Public Class ewDefault : Implements IHttpHandler, IRequiresSessionState
    Dim WithEvents oPcms As Protean.Cms
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        oPcms = New Protean.Cms

        If context.Request("xml") <> "" Then
            oPcms.mbOutputXml = True
        End If

        oPcms.InitializeVariables()

        context.Response.ContentType = "text/html"
        oPcms.GetPageHTML()

        oPcms = Nothing

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

End Class