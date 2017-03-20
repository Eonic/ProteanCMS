<%@ WebHandler Language="VB" Class="ewEnlargeImage" %>

Imports System
Imports System.Web
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Collections.Generic
Imports System.Text
Imports System.IO

Public Class ewEnlargeImage : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
    
        Dim oEwImg As Eonic.Tools.Image = New Eonic.Tools.Image(context.Server.MapPath(context.Request("img")))
        Dim oColour As System.Drawing.Color
        
        Dim red As Integer = 255
        Dim green As Integer = 255
        Dim blue As Integer = 255
        
        ' Determine the base colour
        oColour = Color.FromArgb(getColourFromRequest(context.Request, "red"), getColourFromRequest(context.Request, "green"), getColourFromRequest(context.Request, "blue"))
        
        oEwImg.Reflect(oColour, 75)
        context.Response.ContentType = "image/jpeg"
        'oEwImg.Image.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.jpg)
                
        Dim eps As New EncoderParameters(1)
        eps.Param(0) = New EncoderParameter(Imaging.Encoder.Quality, 80)
        
        Dim cEncoder As String = "image/jpeg"
        If oEwImg.Image Is Nothing Then
            context.Response.StatusCode = 404
            context.ApplicationInstance.CompleteRequest()
            Return
        Else
            Using stream As New IO.MemoryStream()
                oEwImg.Image.Save(stream, GetEncoderInfo(cEncoder), eps)
                stream.WriteTo(context.Response.OutputStream)
            End Using
            context.Response.StatusCode = 200
            context.ApplicationInstance.CompleteRequest()
        End If
      
        oEwImg.Close()
        
    End Sub
 
    Private Function getColourFromRequest(ByRef request As HttpRequest, ByVal colourChannel As String, Optional ByVal defaultValue As Integer = 255) As Integer
        Dim colourRequest As String = request(colourChannel)
        If colourRequest IsNot Nothing AndAlso IsNumeric(colourRequest) AndAlso CInt(colourRequest) >= 0 AndAlso CInt(colourRequest) <= 255 Then
            Return CInt(colourRequest)
        Else
            Return defaultValue
        End If
    End Function
    
    Public Function GetEncoderInfo(ByVal mimeType As String) As ImageCodecInfo
        'gets jpg encoder info
        Try
            Dim j As Integer
            Dim encoders() As ImageCodecInfo
            encoders = ImageCodecInfo.GetImageEncoders()
            For j = 0 To encoders.Length - 1
                If encoders(j).MimeType = mimeType Then Return encoders(j)
            Next
            Return Nothing
        Catch ex As Exception
            'RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetEncoderInfo", ex, ""))
            Return Nothing
        End Try
    End Function
    
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class