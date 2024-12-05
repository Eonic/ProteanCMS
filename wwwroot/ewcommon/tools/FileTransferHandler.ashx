﻿<%@ WebHandler Language="VB" Class="FileTransferHandler" %>

Imports System
Imports System.Web
Imports System.Collections.Generic
Imports System.Configuration
Imports System.IO
Imports System.Linq
Imports System.Web.Script.Serialization
Imports System.Web.Configuration


Public Class FileTransferHandler : Implements IHttpHandler

    Private ReadOnly js As New System.Web.Script.Serialization.JavaScriptSerializer()

    Public StorageRoot As String

    Public SiteRoot As String


    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.AddHeader("Pragma", "no-cache")
        context.Response.AddHeader("Cache-Control", "private, no-cache")

        StorageRoot = context.Server.MapPath(context.Request("storageRoot").Replace("\", "/"))
        SiteRoot = context.Server.MapPath("/")

        HandleMethod(context)
    End Sub

    Public Class FilesStatus
        Public Const HandlerPath As String = "/"

        Public Property group() As String
            Get
                Return m_group
            End Get
            Set(ByVal value As String)
                m_group = Value
            End Set
        End Property
        Private m_group As String
        Public Property name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                m_name = Value
            End Set
        End Property
        Private m_name As String
        Public Property type() As String
            Get
                Return m_type
            End Get
            Set(ByVal value As String)
                m_type = Value
            End Set
        End Property
        Private m_type As String
        Public Property size() As Integer
            Get
                Return m_size
            End Get
            Set(ByVal value As Integer)
                m_size = Value
            End Set
        End Property
        Private m_size As Integer
        Public Property progress() As String
            Get
                Return m_progress
            End Get
            Set(ByVal value As String)
                m_progress = Value
            End Set
        End Property
        Private m_progress As String
        Public Property url() As String
            Get
                Return m_url
            End Get
            Set(ByVal value As String)
                m_url = Value
            End Set
        End Property
        Private m_url As String
        Public Property thumbnail_url() As String
            Get
                Return m_thumbnail_url
            End Get
            Set(ByVal value As String)
                m_thumbnail_url = Value
            End Set
        End Property
        Private m_thumbnail_url As String
        Public Property delete_url() As String
            Get
                Return m_delete_url
            End Get
            Set(ByVal value As String)
                m_delete_url = Value
            End Set
        End Property
        Private m_delete_url As String
        Public Property delete_type() As String
            Get
                Return m_delete_type
            End Get
            Set(ByVal value As String)
                m_delete_type = Value
            End Set
        End Property
        Private m_delete_type As String
        Public Property [error]() As String
            Get
                Return m_error
            End Get
            Set(ByVal value As String)
                m_error = Value
            End Set
        End Property
        Private m_error As String

        Public Sub New()
        End Sub

        Public Sub New(ByVal fileInfo As FileInfo)
            SetValues(fileInfo.Name, CInt(fileInfo.Length))
        End Sub

        Public Sub New(ByVal fileName As String, ByVal fileLength As Integer)
            SetValues(fileName, fileLength)
        End Sub

        Private Sub SetValues(ByVal fileName As String, ByVal fileLength As Integer)
            name = fileName
            type = "image/png"
            size = fileLength
            progress = "1.0"
            url = HandlerPath & "FileTransferHandler.ashx?f=" & fileName
            delete_url = HandlerPath & "FileTransferHandler.ashx?f=" & fileName
            delete_type = "DELETE"
        End Sub
    End Class

    ' Handle request based on method
    Private Sub HandleMethod(ByVal context As HttpContext)

        'Check user has permissions


        Select Case context.Request.HttpMethod
            Case "POST", "PUT"
                UploadFile(context)
                Exit Select

            Case "OPTIONS"
                ReturnOptions(context)
                Exit Select
            Case Else

                context.Response.ClearHeaders()
                context.Response.StatusCode = 405
                Exit Select
        End Select
    End Sub

    Private Sub ReturnOptions(ByVal context As HttpContext)
        context.Response.AddHeader("Allow", "POST,PUT,OPTIONS")
        context.Response.StatusCode = 200
    End Sub

    ' Upload file to the server
    Private Sub UploadFile(ByVal context As HttpContext)
        Dim statuses = New List(Of FilesStatus)()
        Dim headers = context.Request.Headers

        If String.IsNullOrEmpty(headers("X-File-Name")) Then
            UploadWholeFile(context, statuses)
        Else
            UploadPartialFile(headers("X-File-Name"), context, statuses)
        End If

        WriteJsonIframeSafe(context, statuses)
    End Sub

    ' Upload partial file
    Private Sub UploadPartialFile(ByVal fileName As String, ByVal context As HttpContext, ByVal statuses As List(Of FilesStatus))
        If context.Request.Files.Count <> 1 Then
            Throw New HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request")
        End If
        Dim inputStream = context.Request.Files(0).InputStream
        Dim fullName = StorageRoot & Path.GetFileName(fileName)

        Using fs = New FileStream(fullName, FileMode.Append, FileAccess.Write)
            Dim buffer = New Byte(1023) {}

            Dim l = inputStream.Read(buffer, 0, 1024)
            While l > 0
                fs.Write(buffer, 0, l)
                l = inputStream.Read(buffer, 0, 1024)
            End While
            fs.Flush()
            fs.Close()
        End Using
        statuses.Add(New FilesStatus(New FileInfo(fullName)))
    End Sub

    ' Upload entire file
    Private Sub UploadWholeFile(ByVal context As HttpContext, ByVal statuses As List(Of FilesStatus))
        For i As Integer = 0 To context.Request.Files.Count - 1
            Dim file As Object = context.Request.Files(i)
            Try
                If Not StorageRoot.EndsWith("\") Then StorageRoot = StorageRoot & "\"
                Dim fileNameFixed As String = Path.GetFileName(file.FileName).Replace(" ", "-")



                file.SaveAs(StorageRoot & fileNameFixed)

                If LCase(StorageRoot & fileNameFixed).EndsWith(".jpg") Or LCase(StorageRoot & fileNameFixed).EndsWith(".jpeg") Or LCase(StorageRoot & fileNameFixed).EndsWith(".png") Then
                    Dim eImg As New Protean.Tools.Image(StorageRoot & fileNameFixed)
                    Dim moWebCfg As Object = WebConfigurationManager.GetWebApplicationSection("protean/web")
                    eImg.UploadProcessing(moWebCfg("WatermarkText"), SiteRoot & moWebCfg("WatermarkImage"))
                End If
                Dim fullName As String = Path.GetFileName(file.FileName)
                statuses.Add(New FilesStatus(fullName, file.ContentLength))


            Catch ex As Exception
                statuses.Add(New FilesStatus("failed", 0))
            End Try



        Next
    End Sub

    Private Sub WriteJsonIframeSafe(ByVal context As HttpContext, ByVal statuses As List(Of FilesStatus))
        context.Response.AddHeader("Vary", "Accept")
        Try
            If context.Request("HTTP_ACCEPT").Contains("application/json") Then
                context.Response.ContentType = "application/json"
            Else
                context.Response.ContentType = "text/plain"
            End If
        Catch
            context.Response.ContentType = "text/plain"
        End Try

        Dim jsonObj = js.Serialize(statuses.ToArray())
        context.Response.Write(jsonObj)
    End Sub

    Private Function GivenFilename(ByVal context As HttpContext) As Boolean
        Return Not String.IsNullOrEmpty(context.Request("f"))
    End Function


    Public Sub New()

    End Sub
End Class
