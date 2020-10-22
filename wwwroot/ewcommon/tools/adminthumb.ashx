<%@ WebHandler Language="VB" Class="ewEnlargeImage" %>

Imports System
Imports System.Web
Imports System.IO


Public Class ewEnlargeImage : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim oEw As Protean.Cms = New Protean.Cms


        Dim cVirtualPath As String = context.Request("path")
        Dim maxHeight As Long = CLng(0 & context.Request("maxHeight"))
        Dim maxWidth As Long = CLng(0 & context.Request("maxWidth"))
        Dim sPrefix As String = "~ew/tn8-"
        Dim sSuffix As String = ""
        Dim nCompression As Integer = 90
        Dim noStretch As Boolean = True
        Dim isCrop As Boolean = True
        Dim newFilepath As String = ""
        Dim oPcms As Protean.Cms
        oPcms = New Protean.Cms
        oPcms.mbAdminMode = True
        Dim oXsltExt As Protean.xsltExtensions = New Protean.xsltExtensions(oPcms)

        Try
            If sPrefix = "" Then
                sPrefix = "/" & maxWidth & "x" & maxHeight & "/"
            End If
            Dim src As String = oXsltExt.ResizeImage(cVirtualPath, maxWidth, maxHeight, sPrefix, sSuffix, nCompression, noStretch, isCrop)
            context.Response.ContentType = "application/jpeg"
            Dim xFilePath As New System.IO.FileInfo(context.Server.MapPath(src))
            Select Case xFilePath.Extension
                Case ".png"
                    context.Response.ContentType = "application/png"
                Case Else
                    context.Response.ContentType = "application/jpeg"
            End Select
            context.Response.ContentType = "application/jpeg"
            context.Response.AddHeader("Content-Disposition", "inline;filename=" & xFilePath.Name)
            context.Response.WriteFile(context.Server.MapPath(src))
            context.Response.End()
	    
xFilePath = nothing
oPcms = Nothing

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