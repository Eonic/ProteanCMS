Option Strict Off
Option Explicit On

Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Drawing.Drawing2D
Imports System.Drawing.text
Imports System.Security.Cryptography
Imports System.Text
Imports Protean.Tools.Text
Imports Protean.stdTools

Public Class ImgVerify

    Private moCtx As System.Web.HttpContext = System.Web.HttpContext.Current
    Public goSession As System.Web.SessionState.HttpSessionState = moCtx.Session

    ''' <summary>
    ''' Returns a random password 
    ''' DEPRECATED: use Protean.Tools.Text.RandomPassword instead
    ''' </summary>
    ''' <param name="size">The length of the password required</param>
    ''' <param name="lowerCase">Convert the password to lowercase</param>
    ''' <returns>String - a randomly generated password</returns>
    ''' <remarks>DEPRECATED: use Protean.Tools.Text.RandomPassword instead</remarks>
    Public Shared Function getRandomPassword(ByVal size As Integer, ByVal lowerCase As Boolean) As String

        Dim options As TextOptions = TextOptions.UseAlpha Or TextOptions.UseNumeric
        If lowerCase Then options = options Or TextOptions.LowerCase
        Return Protean.Tools.Text.RandomPassword(size, , options)

    End Function


    Public Function generateImage(Optional ByVal sTextToImg As String = "") As Bitmap
        ' 
        If sTextToImg = "" Then
            sTextToImg = getRandomPassword(5, True)
        End If
        'Here, i haven't used any try..catch 
        'Dim pxImagePattern As PixelFormat = PixelFormat.Format32bppArgb
        'Dim bmpImage As New Bitmap(1, 1, pxImagePattern)
        'Dim fntImageFont As New Font("Trebuchets", 14)
        'Dim gdImageGrp As Graphics = Graphics.FromImage(bmpImage)
        'Dim iWidth As Single = gdImageGrp.MeasureString(sTextToImg, fntImageFont).Width
        'Dim iHeight As Single = gdImageGrp.MeasureString(sTextToImg, fntImageFont).Height
        'bmpImage = New Bitmap(CInt(iWidth), CInt(iHeight), pxImagePattern)
        'gdImageGrp = Graphics.FromImage(bmpImage)
        'gdImageGrp.Clear(Color.White)
        'gdImageGrp.TextRenderingHint = TextRenderingHint.AntiAlias
        'gdImageGrp.DrawString(sTextToImg, fntImageFont, New SolidBrush(Color.Red), 0, 0)
        'gdImageGrp.Flush()
        'goSession("imgVerification") = sTextToImg
        'Return bmpImage

        Dim ci As Protean.Tools.Image.CaptchaImage = New Protean.Tools.Image.CaptchaImage

        Dim b As Bitmap = ci.RenderImage

        goSession("imgVerification") = ci.Text

        Return b

    End Function

End Class


Public Class ImageHelper
    Inherits Protean.Tools.Image

    Public Sub New(ByVal Location As String)
        MyBase.New(Location)
    End Sub

    'For the PDF Thumbnail functionality to work you need to copy (GhostScript) gsdll.dll into Windows/System32

    Public Class PDFThumbNail
        Public FilePath As String
        Public newImageFilepath As String
        Public maxWidth As Int16
        Public goServer As System.Web.HttpServerUtility
    End Class

    Public Sub GeneratePDFThumbNail(PDFThumbNail As PDFThumbNail)
        Try
            Dim cCheckServerPath As String = PDFThumbNail.newImageFilepath.Substring(0, PDFThumbNail.newImageFilepath.LastIndexOf("/") + 1)
            cCheckServerPath = goServer.MapPath(cCheckServerPath)
            If Directory.Exists(cCheckServerPath) = False Then
                Directory.CreateDirectory(cCheckServerPath)
            End If

            Dim LayerBuilder As SoundInTheory.DynamicImage.Fluent.PdfLayerBuilder = New SoundInTheory.DynamicImage.Fluent.PdfLayerBuilder().SourceFileName(PDFThumbNail.goServer.MapPath(PDFThumbNail.FilePath)).PageNumber(1).WithFilter(SoundInTheory.DynamicImage.Fluent.FilterBuilder.Resize.ToWidth(500)) _
                                                                                .WithFilter(SoundInTheory.DynamicImage.Fluent.FilterBuilder.Resize.ToWidth(PDFThumbNail.maxWidth)) _
                                                                                .WithFilter(SoundInTheory.DynamicImage.Fluent.FilterBuilder.Border.Width(1).Fill(SoundInTheory.DynamicImage.Colors.Black))

            Dim imgComp As SoundInTheory.DynamicImage.Fluent.CompositionBuilder = New SoundInTheory.DynamicImage.Fluent.CompositionBuilder() _
                                                                                .WithLayer(LayerBuilder)
            imgComp.ImageFormat(SoundInTheory.DynamicImage.DynamicImageFormat.Png)
            imgComp.SaveTo(PDFThumbNail.goServer.MapPath(PDFThumbNail.newImageFilepath))
            imgComp = Nothing
            LayerBuilder = Nothing

        Catch ex As Exception
            Err.Raise(8032, "PDFThumbNail", ex.Message)
        End Try


    End Sub

End Class


