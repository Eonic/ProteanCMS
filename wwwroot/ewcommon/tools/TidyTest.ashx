<%@ WebHandler Language="VB" Class="ewTidyTest" %>

Imports System
Imports System.Web

Public Class ewTidyTest : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim htmltotest As String = "<h1>Tidy is Tidying</h1>"
        context.Response.ContentType = "text/html"
        Dim sResponse As String = Protean.Tools.Text.tidyXhtmlFrag(htmltotest, True, True, True)
        context.Response.Write(sResponse & "<br/>If server server is 64 bit and failed to load tidy.dll reference and broke functionality Get dll from below link with the version you want http://binaries.html-tidy.org/ Please use following steps Put tidyX86.dll, tidyX64.dll and tidy.dll in C:\Windows\System32 and it will start working.")

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class