<%@ WebHandler Language="VB" Class="ewTidyTest" %>

Imports System
Imports System.Web

Public Class ewTidyTest : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim htmltotest As String = "<h1>Tidy is Tidying</h1>"
        context.Response.ContentType = "text/html"
        Dim sResponse As String = Protean.Tools.Text.tidyXhtmlFrag(htmltotest, True, True, True)
        context.Response.Write(sResponse)

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class