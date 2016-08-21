<%@ WebHandler Language="VB" Class="RegularProxy" %>
Imports System.Web
Imports System.Web.Caching
Imports System.Net
Imports System.Data
Imports System.Configuration
Imports System.Web.Security
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports System.Web.UI.WebControls.WebParts
Imports System.Web.UI.HtmlControls
Imports System.Threading
Imports System.IO
'Imports ProxyHelpers

Public Class RegularProxy
    Implements IHttpHandler

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim url As String = context.Request("url")
        Dim cacheDuration As Integer ' = Convert.ToInt32(If(context.Request("cache"), "0"))
        Dim contentType As String = context.Request("type")

        ' We don't want to buffer because we want to save memory
        context.Response.Buffer = False

        ' Serve from cache if available
        If context.Cache(url) IsNot Nothing Then
            context.Response.BinaryWrite(TryCast(context.Cache(url), Byte()))
            context.Response.Flush()
            Return
        End If
        Using client As New WebClient()
            If Not String.IsNullOrEmpty(contentType) Then
                client.Headers("Content-Type") = contentType
            End If

            client.Headers("Accept-Encoding") = "gzip"
            client.Headers("Accept") = "*/*"
            client.Headers("Accept-Language") = "en-US"
            client.Headers("User-Agent") = "Mozilla/5.0 (Windows; U; Windows NT 6.0; " & "en-US; rv:1.8.1.6) Gecko/20070725 Firefox/2.0.0.6"

            Dim data As Byte() = client.DownloadData(url)

            context.Cache.Insert(url, data, Nothing, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(cacheDuration), CacheItemPriority.Normal, _
             Nothing)

            If Not context.Response.IsClientConnected Then
                Return
            End If


            ' Deliver content type, encoding and length
            ' as it is received from the external URL
            context.Response.ContentType = client.ResponseHeaders("Content-Type")
            Dim contentEncoding As String = client.ResponseHeaders("Content-Encoding")
            Dim contentLength As String = client.ResponseHeaders("Content-Length")

            If Not String.IsNullOrEmpty(contentEncoding) Then
                context.Response.AppendHeader("Content-Encoding", contentEncoding)
            End If
            If Not String.IsNullOrEmpty(contentLength) Then
                context.Response.AppendHeader("Content-Length", contentLength)
            End If

            'If cacheDuration > 0 Then
            '    HttpHelper.CacheResponse(context, cacheDuration)
            'End If

            ' Transmit the exact bytes downloaded
            context.Response.BinaryWrite(data)
        End Using
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property
   
End Class
