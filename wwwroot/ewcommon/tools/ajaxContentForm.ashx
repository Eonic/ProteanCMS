<%@ WebHandler Language="VB" Class="ewAjaxContentForm" %>

Imports System
Imports System.Web

Public Class ewAjaxContentForm : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Eonic.Web = New Eonic.Web
        oEw.InitializeVariables()
        oEw.Open()

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If

        oEw.mcEwSiteXsl = "/xsl/ajaxStandard.xsl"
        oEw.GetAjaxHTML()

        oEw = Nothing
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class