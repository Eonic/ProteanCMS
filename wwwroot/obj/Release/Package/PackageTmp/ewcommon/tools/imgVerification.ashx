<%@ WebHandler Language="VB" Class="ewImgVerificaiton" %>

Imports System
Imports System.Web
Imports System.Web.SessionState
Imports System.Drawing

Public Class ewImgVerificaiton : Implements IHttpHandler, IRequiresSessionState
    
    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        
        Dim oEw As eonic.imgverify = New eonic.imgverify
              
        '-- write the image to the HTTP output stream as an array of bytes
        Dim b As Bitmap
        b = oEw.generateImage
        b.Save(context.Response.OutputStream, Drawing.Imaging.ImageFormat.Jpeg)
        b.Dispose()
        context.Response.ContentType = "image/jpeg"
        context.Response.StatusCode = 200
        context.ApplicationInstance.CompleteRequest()
        
    End Sub
 
    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class