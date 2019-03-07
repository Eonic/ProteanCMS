<%@ WebHandler Language="VB" Class="ewEnlargeImage" %>

Imports System
Imports System.Web

Public Class ewEnlargeImage : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Protean.Cms = New Protean.Cms


        Dim cVirtualPath As String = context.Request("path")
        Dim maxHeight As Long = CLng(0 & context.Request("maxHeight"))
        Dim maxWidth As Long = CLng(0 & context.Request("maxWidth"))
        Dim sPrefix As String = context.Request("prefix")
        Dim sSuffix As String = context.Request("suffix")
        Dim nCompression As Integer = 100
        Dim noStretch As Boolean = True
        Dim isCrop As Boolean = True
        Dim newFilepath As String = ""

        Dim oXsltExt As Protean.xsltExtensions = New Protean.xsltExtensions

        Try
            If sPrefix = "" Then
                sPrefix = "/" & maxWidth & "x" & maxHeight & "/"
            End If
            Dim src As String = oXsltExt.ResizeImage(cVirtualPath, maxWidth, maxHeight, sPrefix, sSuffix, nCompression, noStretch, isCrop)
            Dim width As String = oXsltExt.ImageWidth(src)
            Dim height As String = oXsltExt.ImageHeight(src)

            context.Response.Write("<img src=""" & src & """ width=""" & width & """ height=""" & height & """/>")

        Catch ex As Exception
            context.Response.Write(ex.InnerException)
        End Try


    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class