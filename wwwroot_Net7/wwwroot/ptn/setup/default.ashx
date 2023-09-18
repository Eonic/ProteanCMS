<%@ WebHandler Language="VB" Class="setupDefault" %>

Imports System
Imports System.Web
Imports System.Web.SessionState
Imports System.Xml

Public Class setupDefault : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim oEw As Protean.Setup = New Protean.Setup()

        If context.Request("contentType") = "xml" Then
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