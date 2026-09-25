<%@ WebHandler Language="VB" Class="ErrorEmailTest" %>

Imports System
Imports System.Web
Imports System.Xml
Imports Protean

Public Class ErrorEmailTest : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim sException As String = ""

        Try
            ' Deliberately throw a test exception so we have something real to report on.
            Throw New Exception("This is a test exception raised by ErrorEmailTest.ashx to verify the error handler / email notification.")

        Catch oException As Exception

            ' bDebug:=False ensures the error is emailed (and not just rendered inline for debugging).
            stdTools.returnException(
                sException,
                "ErrorEmailTest",
                "ProcessRequest",
                oException,
                context,
                "/ewcommon/xsl/standard.xsl",
                "Manual test of the error handler / error email from ErrorEmailTest.ashx",
                False,
                "[TEST] Error Email Test")

        End Try

        context.Response.ContentType = "text/html"
        context.Response.Write(sException)

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class