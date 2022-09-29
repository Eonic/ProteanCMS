<%@ WebHandler Language="VB" Class="ewTidyTest" %>

Imports System
Imports System.Web
Imports System.IO
Imports ImageMagick


Public Class ewTidyTest : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim FilePath As String = context.Request("path")

        Dim imgFile As New FileInfo(context.Server.MapPath(FilePath))
        context.Response.Write("Filesize Start" & imgFile.Length)
        context.Response.Write("<br/>")
        context.Response.Write("<img src=""" & FilePath & """/>")

        context.Response.Write("<br/>")
        Dim ptnImg As New Protean.Tools.Image("")
        Dim oEw As Protean.Cms = New Protean.Cms
        oEw.InitializeVariables()
        ptnImg.TinifyKey = oEw.moConfig("TinifyKey")
        Dim Quality As Int16 = 50
        ' Do While Quality < 101
        Dim fileSuffix As String = "" ' "-" & Quality.ToString()
        ptnImg.CompressImage(imgFile, False, Quality, fileSuffix)
        Dim newFilePath As String = FilePath.Replace(".jpg", fileSuffix & ".jpg")
        Dim newImgFile As New FileInfo(context.Server.MapPath(newFilePath))
        context.Response.Write("<br/>")
        context.Response.Write("Filesize Quality " & Quality.ToString() & " " & newImgFile.Length)
        context.Response.Write("<br/>")
        context.Response.Write("<img src=""" & newFilePath & """/>")
        '  Quality = Quality + 5
        '  Loop

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class