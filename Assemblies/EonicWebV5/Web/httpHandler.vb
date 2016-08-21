
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Text
Imports System.Web
Imports System.Threading
Imports System.Web.SessionState

Public Class WebHttpHandler
    Implements IHttpHandler, IRequiresSessionState

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return True
        End Get
    End Property

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest

        Dim oEw As New Eonic.Web

        If context.Request("xml") <> "" Then
            oEw.mbOutputXml = True
        End If

        oEw.InitializeVariables()
        context.Response.ContentType = "text/html"
        oEw.GetPageHTML()

        oEw = Nothing
    End Sub


End Class



