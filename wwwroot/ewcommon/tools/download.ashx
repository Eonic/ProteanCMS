<%@ WebHandler Language="VB" Class="ewDownload" %>

Imports System
Imports System.Web
Imports System.Web.SessionState

Public Class ewDownload : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim oEw As Eonic.Web = New Eonic.Web
        oEw.InitializeVariables()
        oEw.Open()
        oEw.returnDocumentFromItem(context)
        oEw = Nothing

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class