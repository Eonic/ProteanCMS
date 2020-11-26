<%@ WebHandler Language="VB" Class="ewTidyTest" %>

Imports System
Imports System.Web

Public Class ewTidyTest : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim htmltotest As String = "<h1>Test FS Write</h1>"
        context.Response.ContentType = "text/html"

        Dim oFS As New Protean.fsHelper(context)
Dim filepath as string = "/ewCache/FSTEST"
        Dim sError As String = oFS.CreatePath(filepath)

        context.Response.Write(sError)

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class