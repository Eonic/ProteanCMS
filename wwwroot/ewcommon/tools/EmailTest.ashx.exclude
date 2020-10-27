<%@ WebHandler Language="VB" Class="ewEmailTest" %>

Imports System
Imports System.Web
Imports System.Xml

Public Class ewEmailTest : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim errMsg As String = ""

        Dim oMsg As New Protean.Messaging(errMsg)

        Dim oBodyXml As New XmlDocument
        oBodyXml.LoadXml("<Items><Name /><Telephone /><Email>support@eonic.co.uk</Email><Message>This is a test</Message></Items>")
        oMsg.emailer(oBodyXml.FirstChild, "/ewcommon/xsl/email/mailform.xsl", "ProteanCMS Test", "trevor@eonic.co.uk", "trevor@eonic.co.uk", "This is a TEST")

        If errMsg <> "" Then
            context.Response.Write(errMsg)
        Else
            context.Response.Write("<h1>Message Sent</h1>")
        End If

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class