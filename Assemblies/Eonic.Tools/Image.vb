Imports System.Drawing
Imports System.Drawing.Imaging

Imports System.IO
Imports System.Web
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Drawing.Drawing2D

Public Class Image
#Region "Declarations"
    Private cLocation As String 'Location of the file to load
    Private bKeepRelational As Boolean = True 'keep sizes relation or skew
    Private oImg As System.Drawing.Bitmap 'the base image
    Private oSourceImg As System.Drawing.Bitmap 'the base image
    Private oGraphics As Graphics
    Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
    Private Const mcModuleName As String = "Eonic.Tools.Image"
    Private bCrop As Boolean = False 'Crop the image?
    Private bNoStretch As Boolean = False 'Don't Expand, only Shrink

    Private nMaxHeightCrop As Integer = 0
    Private nMaxWidthCrop As Integer = 0

    Public Property Image() As Drawing.Image
        Get
            Return oImg
        End Get
        Set(ByVal value As Drawing.Image)
            oImg = value
        End Set
    End Property

#End Region


#Region "Public Subs"
    Public Sub New(ByVal Location As String)
        Try
            cLocation = Location 'set the location
            ReLoad() 'load the image
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
        End Try
    End Sub

    Public Sub Close()
        'closes
        Try
            If Not oGraphics Is Nothing Then
                oGraphics.Dispose()
                oGraphics = Nothing
            End If
            If Not oImg Is Nothing Then
                oImg.Dispose()
                oImg = Nothing
            End If
            If Not oSourceImg Is Nothing Then
                oSourceImg.Dispose()
                oSourceImg = Nothing
            End If
            Me.Finalize()
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Close", ex, ""))
        End Try
    End Sub

    Public Sub ReLoad()
        'load the image
        Try
            If IO.File.Exists(cLocation) Then
                Using stream = File.OpenRead(cLocation)
                    oImg = System.Drawing.Image.FromStream(stream)
                End Using
                'oImg = System.Drawing.Image.FromFile(cLocation)
            End If
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ReLoad", ex, ""))
        End Try
    End Sub

    Public Sub UploadProcessing(ByVal _WatermarkText As String, _WatermarkImgPath As String)
        Try
            If _WatermarkText <> "" Then
                AddWatermark(oImg, _WatermarkText, _WatermarkImgPath)
            End If
            'overwrite


        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
        End Try
    End Sub

    Public Sub SetMaxSize(ByVal nWidth As Integer, ByVal nHeight As Integer)
        'decides wether it needs to be sized
        Try
            If (nWidth > 0 And Not (nWidth = oImg.Width)) Or (nHeight > 0 And Not (nHeight = oImg.Height)) Then
                If bKeepRelational Then
                    ResizeMax(nWidth, nHeight)
                Else
                    Resize(nWidth, nHeight)
                End If
            End If
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SetMaxSize", ex, ""))
        End Try
    End Sub

    Public Sub DeleteOriginal()
        'deletes the original image
        Try
            IO.File.Delete(cLocation)
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteOriginal", ex, ""))
        End Try
    End Sub



    Public Sub Reflect(ByVal _BackgroundColor As Color, ByVal _Reflectivity As Integer)
        ' Calculate the size of the new image
        Try
            AddReflection(_BackgroundColor, _Reflectivity)
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Reflect", ex, ""))
        End Try
    End Sub

#End Region

#Region "Public Functions"
    Public Function Save(ByVal cPath As String, Optional ByVal nCompression As Integer = 25, Optional ByVal serverPath As String = "") As Boolean
        'saves the file to designated location
        Try
            'check the compression ratio
            If nCompression > 100 Then nCompression = 100
            If nCompression < 1 Then nCompression = 1
            'saves with compression and formatting
            Return SaveJPGWithCompressionSetting(oImg, cPath, nCompression, serverPath)
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Save", ex, ""))
            Return False
        End Try
    End Function



#End Region

#Region "Public Properties"

    Public Property Width() As Integer
        'returns current width
        Get
            Return oImg.Width
        End Get
        Set(ByVal value As Integer)
            'sets the width to value specified
            If value >= 1 Then Resize(value, 0)
        End Set
    End Property

    Public Property Height() As Integer
        Get
            'returns current height
            Return oImg.Height
        End Get
        Set(ByVal value As Integer)
            'resizes
            If value >= 1 Then Resize(0, value)
        End Set
    End Property

    Public Property KeepXYRelation() As Boolean
        'simple property for the relationship between X and Y
        Get
            Return bKeepRelational
        End Get
        Set(ByVal value As Boolean)
            bKeepRelational = value
        End Set
    End Property

    Public Property IsCrop() As Boolean
        'Property for if image is wanting to be cropped
        Get
            Return bCrop
        End Get
        Set(ByVal value As Boolean)
            bCrop = value
        End Set
    End Property

    Public Property NoStretch() As Boolean
        'Property for if image is wanting to be cropped
        Get
            Return bNoStretch
        End Get
        Set(ByVal value As Boolean)
            bNoStretch = value
        End Set
    End Property


#End Region

#Region "Private Procedures"
    Private Sub Resize(ByVal nWidth As Integer, ByVal nHeight As Integer)
        'decides on the new sizes for the image
        Try
            Dim nNewWidth As Integer
            Dim nNewHeight As Integer

            If Not bKeepRelational Then
                If nWidth > 0 Then
                    nNewWidth = nWidth
                    nNewHeight = oImg.Height
                Else
                    nNewHeight = nHeight
                    nNewWidth = oImg.Width
                End If
            Else
                Dim nPercent As Double
                If nWidth > 0 Then
                    nPercent = nWidth / oImg.Width
                    nNewWidth = oImg.Width * nPercent
                    nNewHeight = oImg.Height * nPercent
                Else
                    nPercent = nHeight / oImg.Height
                    nNewWidth = oImg.Width * nPercent
                    nNewHeight = oImg.Height * nPercent
                End If
            End If

            oImg = ImageResize(oImg, nNewHeight, nNewWidth)
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Resize", ex, ""))
        End Try
    End Sub

    Private Sub ResizeMax(ByVal nMaxWidth As Integer, ByVal nMaxHeight As Integer)
        Try

            If bCrop Then
                ' For Cropped images, we want to expand to the biggest variant
                nMaxHeightCrop = nMaxHeight
                nMaxWidthCrop = nMaxWidth

                'NB 13th April 2010 Changes!

                Dim nXCalc As Double = oImg.Width / nMaxWidth
                Dim yXCalc As Double = oImg.Height / nMaxHeight


                If bNoStretch = False Then

                    If yXCalc < nXCalc Then
                        Resize(0, nMaxHeight)
                    Else
                        Resize(nMaxWidth, 0)
                    End If

                Else

                    'If not Stretch
                    'If both bigger, shrink
                    If ((oImg.Width >= nMaxWidth) And (oImg.Height >= nMaxHeight)) Then
                        If yXCalc < nXCalc Then
                            Resize(0, nMaxHeight)
                        Else
                            Resize(nMaxWidth, 0)
                        End If
                        'Else Shrink accordingly
                    ElseIf ((oImg.Width < nMaxWidth) And (oImg.Height >= nMaxHeight)) Then
                        Resize(nMaxWidth, 0)
                    ElseIf ((oImg.Height < nMaxHeight) And (oImg.Width >= nMaxWidth)) Then
                        Resize(0, nMaxHeight)
                    ElseIf ((oImg.Width < nMaxWidth) And (oImg.Height < nMaxHeight)) Then
                        'Do nothing for if both smaller!
                        'Blow it up....
                        If oImg.Width < oImg.Height Then
                            Resize(0, nMaxHeight)
                        Else
                            Resize(nMaxWidth, 0)
                        End If
                    End If
                End If

                'If oImg.Height < oImg.Width Then
                '    Resize(0, nMaxHeight)
                'Else
                '    Resize(nMaxWidth, 0)
                'End If

                'ElseIf bNoStretch Then
                '    If ((oImg.Height > nMaxHeight) Or (oImg.Width > nMaxWidth)) Then
                '        'Squares, since they are squares just shrink to the smaller side
                '        If oImg.Height = oImg.Width Then
                '            If nMaxWidth > nMaxHeight Then
                '                Resize(0, nMaxHeight)
                '            Else
                '                Resize(nMaxWidth, 0)
                '            End If


                '            'Rectangles
                '        ElseIf oImg.Height > oImg.Width Then
                '            Resize(0, nMaxHeight)
                '        Else
                '            Resize(nMaxWidth, 0)
                '        End If
                '        ' No need to do anything if both sides are smaller
                '    End If

            Else
                ' For regular stretches, make this biggest possible within the max height/width bounds

                ' NB 20-Feb-2009 Redone, old if statements feel a bit clumsy
                Dim nMaxResizedArea As Double = nMaxWidth * nMaxHeight
                Dim nXscaledArea As Double = nMaxWidth * (oImg.Height * (nMaxWidth / oImg.Width))
                Dim nYscaledArea As Double = (oImg.Width * (nMaxHeight / oImg.Height)) * nMaxHeight


                If bNoStretch And Not ((oImg.Width >= nMaxWidth) And (oImg.Height >= nMaxHeight)) Then
                    ' IF Both bigger skip
                    If (oImg.Width >= nMaxWidth) Then
                        Resize(nMaxWidth, 0)
                    ElseIf (oImg.Height >= nMaxHeight) Then
                        Resize(0, nMaxHeight)
                    Else
                        'increase size of the image
                        Resize(oImg.Width, oImg.Height)
                        ' Do nothing if both smaller!
                    End If

                    ' Regular aka no Crop/Stretch tests
                ElseIf (nXscaledArea <= nMaxResizedArea) And (nYscaledArea <= nMaxResizedArea) Then
                    If nXscaledArea >= nYscaledArea Then
                        Resize(nMaxWidth, 0)
                    Else
                        Resize(0, nMaxHeight)
                    End If
                ElseIf (nXscaledArea <= nMaxResizedArea) Then
                    Resize(nMaxWidth, 0)
                Else
                    Resize(0, nMaxHeight)
                End If

                'If oImg.Height = oImg.Width Then
                '    If nMaxWidth > nMaxHeight Then
                '        Resize(0, nMaxHeight)
                '    Else
                '        Resize(nMaxWidth, 0)
                '    End If
                'ElseIf oImg.Height > oImg.Width Then
                '    Resize(0, nMaxHeight)
                'Else
                '    Resize(nMaxWidth, 0)
                'End If

            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ResizeMax", ex, ""))
        End Try
    End Sub

    Private Function ImageResize(ByVal oImage As System.Drawing.Image, ByVal nHeight As Int32, ByVal nWidth As Int32) As System.Drawing.Image
        'does the actual resize
        Try
            oSourceImg = New Bitmap(oImage)
            oImg = New Bitmap(nWidth, nHeight)
            oGraphics = Graphics.FromImage(oImg)
            'GFX Additions--
            'Trev
            ' oGraphics.Clear(Color.White)
            oGraphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            oGraphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            'NB
            oGraphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            oGraphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            '--
            oGraphics.DrawImage(oSourceImg, 0, 0, oImg.Width, oImg.Height)
            oImage = oImg
            'Added--
            If bCrop Then oImage = CropImage(oImage)
            '-------
            Return oImage
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ImageResize", ex, ""))
            Return oImage
        End Try

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
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetEncoderInfo", ex, ""))
            Return Nothing
        End Try
    End Function

    Private Function SaveJPGWithCompressionSetting(ByVal theImg As System.Drawing.Image, ByVal szFileName As String, ByVal compression As Long, Optional ByVal serverPath As String = "") As Boolean
        'save the image
        Dim cProcessInfo As String = ""
        Try

            If serverPath <> "" Then
                If Directory.Exists(serverPath) = False Then
                    Directory.CreateDirectory(serverPath)
                End If
            End If

            If szFileName.EndsWith(".gif") Then
                theImg.Save(Replace(szFileName, ".gif", ".png"), ImageFormat.Png)

                Dim imgFile As New FileInfo(Replace(szFileName, ".gif", ".png"))
                If compression = 100 Then
                    CompressImage(imgFile, True)
                Else
                    CompressImage(imgFile, False)
                End If

            ElseIf szFileName.EndsWith(".png") Then
                theImg.Save(szFileName, ImageFormat.Png)

                Dim imgFile As New FileInfo(szFileName)
                If compression = 100 Then
                    CompressImage(imgFile, True)
                Else
                    CompressImage(imgFile, False)
                End If

            Else
                Dim eps As New EncoderParameters(1)
                eps.Param(0) = New EncoderParameter(Imaging.Encoder.Quality, 100)
                Dim cEncoder As String = "image/jpeg"
                Dim ici As ImageCodecInfo = GetEncoderInfo(cEncoder)
                theImg.Save(szFileName, ici, eps)

                Dim imgFile As New FileInfo(szFileName)
                If compression = 100 Then
                    CompressImage(imgFile, True)
                Else
                    CompressImage(imgFile, False)
                End If
                imgFile.Refresh()

            End If
            cProcessInfo = cProcessInfo
            Return True
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "SaveJPGWithCompressionSetting", ex, cProcessInfo))
            Return False
        Finally
            theImg.Dispose()
            Close()
        End Try
    End Function

    Public Function CompressImage(ByRef imgfileInfo As FileInfo, ByVal lossless As Boolean) As Long
        Dim difference As Long
        Try
            'Compress the File using ImageMagick
            Select Case LCase(imgfileInfo.Extension)
            Case ".gif"

                difference = imgfileInfo.Length
                Dim optimizer As New ImageMagick.ImageOptimizers.GifOptimizer()
                If lossless Then
                    optimizer.LosslessCompress(imgfileInfo)
                Else
                    optimizer.OptimalCompression = True
                    optimizer.Compress(imgfileInfo)
                End If
                imgfileInfo.Refresh()
                difference = difference - imgfileInfo.Length
            Case ".png"

                difference = imgfileInfo.Length
                Dim optimizer As New ImageMagick.ImageOptimizers.PngOptimizer()
                If lossless Then
                    optimizer.LosslessCompress(imgfileInfo)
                Else
                    optimizer.OptimalCompression = True
                    optimizer.Compress(imgfileInfo)
                End If
                imgfileInfo.Refresh()
                difference = difference - imgfileInfo.Length
            Case ".jpg", ".jpeg"

                difference = imgfileInfo.Length
                Dim optimizer As New ImageMagick.ImageOptimizers.JpegOptimizer()
                If lossless Then
                    optimizer.LosslessCompress(imgfileInfo)
                Else
                    optimizer.OptimalCompression = True
                    optimizer.Compress(imgfileInfo)
                End If
                imgfileInfo.Refresh()
                difference = difference - imgfileInfo.Length
        End Select
        Return difference

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CompressImage", ex, ""))
            Return 0
        End Try
    End Function


    Private Function CropImage(ByVal oImage As System.Drawing.Image) As System.Drawing.Image
        Try
            Dim oBitmapOrig As New Bitmap(oImage)
            Dim sRect As RectangleF = oBitmapOrig.GetBounds(GraphicsUnit.Pixel)
            Dim oBitmapCrop As New Bitmap(nMaxWidthCrop, nMaxHeightCrop)
            Dim oGraphics As Graphics = Graphics.FromImage(oBitmapCrop)
            oGraphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            oGraphics.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
            oGraphics.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            oGraphics.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            If (oImage.Width = nMaxWidthCrop And nMaxHeightCrop > 0) Then
                ' If the Width is perfect, crop the Height
                Dim nNewY As Integer = (oImage.Height - nMaxHeightCrop) / 2
                sRect.Inflate(-0, -nNewY)
                oGraphics.DrawImage(oBitmapOrig, 0, 0, sRect, GraphicsUnit.Pixel)
            ElseIf nMaxWidthCrop > 0 Then
                ' Else crop the width
                Dim nNewW As Integer = (oImage.Width - nMaxWidthCrop) / 2
                sRect.Inflate(-nNewW, -0)
                oGraphics.DrawImage(oBitmapOrig, 0, 0, sRect, GraphicsUnit.Pixel)
            End If
            oImage = oBitmapCrop
            Return oImage
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CropImage", ex, ""))
            Return oImage
        End Try
    End Function

    Private Function AddReflection(ByVal _BackgroundColor As Color, ByVal _Reflectivity As Integer) As System.Drawing.Image

        Dim _image As Drawing.Image = oImg
        ' Calculate the size of the new image
        Dim height As Integer = CInt((_image.Height + (_image.Height * (CSng(_Reflectivity) / 255))))
        Dim newImage As New Bitmap(_image.Width, height, PixelFormat.Format24bppRgb)
        newImage.SetResolution(_image.HorizontalResolution, _image.VerticalResolution)

        Using graphics__1 As Graphics = Graphics.FromImage(newImage)
            ' Initialize main graphics buffer
            graphics__1.Clear(_BackgroundColor)
            graphics__1.DrawImage(_image, New Point(0, 0))
            graphics__1.InterpolationMode = InterpolationMode.HighQualityBicubic
            Dim destinationRectangle As New Rectangle(0, _image.Size.Height, _image.Size.Width, _image.Size.Height)

            ' Prepare the reflected image
            Dim reflectionHeight As Integer = (_image.Height * _Reflectivity) / 255
            Dim reflectedImage As Drawing.Image = New Bitmap(_image.Width, reflectionHeight)

            ' Draw just the reflection on a second graphics buffer
            Using gReflection As Graphics = Graphics.FromImage(reflectedImage)
                gReflection.DrawImage(_image, New Rectangle(0, 0, reflectedImage.Width, reflectedImage.Height), 0, _image.Height - reflectedImage.Height, reflectedImage.Width, reflectedImage.Height, _
                GraphicsUnit.Pixel)
            End Using
            reflectedImage.RotateFlip(RotateFlipType.RotateNoneFlipY)
            Dim imageRectangle As New Rectangle(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, (destinationRectangle.Height * _Reflectivity) / 255)

            ' Draw the image on the original graphics
            graphics__1.DrawImage(reflectedImage, imageRectangle)

            ' Finish the reflection using a gradiend brush
            Dim brush As New LinearGradientBrush(imageRectangle, Color.FromArgb(255 - _Reflectivity, _BackgroundColor), _BackgroundColor, 90, False)
            graphics__1.FillRectangle(brush, imageRectangle)
        End Using

        oImg = newImage
        Return oImg

    End Function

    Private Function AddWatermark(ByVal imgPhoto As System.Drawing.Image, ByVal _WatermarkText As String, _WatermarkImgPath As String) As System.Drawing.Image

        Try

            Dim phWidth As Integer = imgPhoto.Width
            Dim phHeight As Integer = imgPhoto.Height

            Dim bmPhoto As New Bitmap(phWidth, phHeight, PixelFormat.Format24bppRgb)
            bmPhoto.SetResolution(72, 72)

            Dim grPhoto As Graphics = Graphics.FromImage(bmPhoto)

            grPhoto.SmoothingMode = SmoothingMode.AntiAlias
            grPhoto.DrawImage(imgPhoto, New Rectangle(0, 0, phWidth, phHeight), 0, 0, phWidth, phHeight, GraphicsUnit.Pixel)


            ' Write the Watermark Text centred at the bottom of the image
            ' Check the font size
            Dim sizes As Integer() = New Integer() {48, 24, 20, 16, 14, 12, 10, 8, 6, 4}
            Dim crFont As Font = Nothing
            Dim crSize As New SizeF()
            For i As Integer = 0 To 9
                crFont = New Font("arial", sizes(i), FontStyle.Bold)
                crSize = grPhoto.MeasureString(_WatermarkText, crFont)

                If CUShort(crSize.Width) < CUShort(phWidth * 0.66) Then
                    Exit For
                End If
            Next
            'Place at the bottom
            Dim yPixlesFromBottom As Integer = CInt(Math.Truncate(phHeight * 0.05))
            Dim yPosFromBottom As Single = ((phHeight - yPixlesFromBottom) - (crSize.Height / 2))
            Dim xCenterOfImg As Single = (phWidth / 2)

            'Write the text
            Dim StrFormat As New StringFormat()
            StrFormat.Alignment = StringAlignment.Center

            Dim semiTransBrush2 As New SolidBrush(Color.FromArgb(153, 0, 0, 0))

            grPhoto.DrawString(_WatermarkText, crFont, semiTransBrush2, New PointF(xCenterOfImg + 1, yPosFromBottom + 1), StrFormat)

            Dim semiTransBrush As New SolidBrush(Color.FromArgb(153, 255, 255, 255))

            grPhoto.DrawString(_WatermarkText, crFont, semiTransBrush, New PointF(xCenterOfImg, yPosFromBottom), StrFormat)

            'Now add the image watermark
            If _WatermarkImgPath <> "" Then

                Dim imgWatermark As System.Drawing.Image = System.Drawing.Image.FromFile(_WatermarkImgPath)
                Dim wmWidth As Integer = imgWatermark.Width
                Dim wmHeight As Integer = imgWatermark.Height

                Dim bmWatermark As New Bitmap(bmPhoto)
                bmWatermark.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution)

                Dim grWatermark As Graphics = Graphics.FromImage(bmWatermark)


                Dim imageAttributes As New ImageAttributes()
                Dim colorMap As New ColorMap()

                colorMap.OldColor = Color.FromArgb(255, 0, 255, 0)
                colorMap.NewColor = Color.FromArgb(0, 0, 0, 0)
                Dim remapTable As ColorMap() = {colorMap}

                imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap)

                Dim colorMatrixElements As Single()() = {New Single() {1.0F, 0F, 0F, 0F, 0F}, New Single() {0F, 1.0F, 0F, 0F, 0F}, New Single() {0F, 0F, 1.0F, 0F, 0F}, New Single() {0F, 0F, 0F, 0.3F, 0F}, New Single() {0F, 0F, 0F, 0F, 1.0F}}

                Dim wmColorMatrix As New ColorMatrix(colorMatrixElements)

                imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.[Default], ColorAdjustType.Bitmap)

                Dim xPosOfWm As Integer = ((phWidth - wmWidth) - 10)
                Dim yPosOfWm As Integer = 10

                grWatermark.DrawImage(imgWatermark, New Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight), 0, 0, wmWidth, wmHeight, GraphicsUnit.Pixel, imageAttributes)

                oImg = bmWatermark


                grWatermark.Dispose()
            Else
                oImg = bmPhoto
            End If
            Save(cLocation)
            Return oImg
            grPhoto.Dispose()

        Catch ex As Exception
            Return imgPhoto
        End Try

    End Function

#End Region

    ''' <summary>
    ''' CAPTCHA image generation class
    ''' </summary>
    ''' <remarks>
    ''' Adapted from the excellent code at 
    ''' http://www.codeproject.com/aspnet/CaptchaImage.asp
    '''
    ''' Jeff Atwood
    ''' http://www.codinghorror.com/
    ''' </remarks>
    Public Class CaptchaImage

        Private _height As Integer
        Private _width As Integer
        Private _rand As Random
        Private _generatedAt As DateTime
        Private _randomText As String
        Private _randomTextLength As Integer
        Private _randomTextChars As String
        Private _fontFamilyName As String
        Private _fontWarp As FontWarpFactor
        Private _backgroundNoise As BackgroundNoiseLevel
        Private _lineNoise As LineNoiseLevel
        Private _guid As String
        Private _fontWhitelist As String
        Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
        Private Const mcModuleName As String = "Eonic.Tools.Image.CaptchaImage"

#Region "  Public Enums"

        ''' <summary>
        ''' Amount of random font warping to apply to rendered text
        ''' </summary>
        Public Enum FontWarpFactor
            None
            Low
            Medium
            High
            Extreme
        End Enum

        ''' <summary>
        ''' Amount of background noise to add to rendered image
        ''' </summary>
        Public Enum BackgroundNoiseLevel
            None
            Low
            Medium
            High
            Extreme
        End Enum

        ''' <summary>
        ''' Amount of curved line noise to add to rendered image
        ''' </summary>
        Public Enum LineNoiseLevel
            None
            Low
            Medium
            High
            Extreme
        End Enum

#End Region

#Region "  Public Properties"

        ''' <summary>
        ''' Returns a GUID that uniquely identifies this Captcha
        ''' </summary>
        Public ReadOnly Property UniqueId() As String
            Get
                Return _guid
            End Get
        End Property

        ''' <summary>
        ''' Returns the date and time this image was last rendered
        ''' </summary>
        Public ReadOnly Property RenderedAt() As DateTime
            Get
                Return _generatedAt
            End Get
        End Property

        ''' <summary>
        ''' Font family to use when drawing the Captcha text. If no font is provided, a random font will be chosen from the font whitelist for each character.
        ''' </summary>
        Public Property Font() As String
            Get
                Return _fontFamilyName
            End Get
            Set(ByVal Value As String)
                Try
                    Dim font1 As Font = New Font(Value, 12.0!)
                    _fontFamilyName = Value
                    font1.Dispose()
                Catch ex As Exception
                    _fontFamilyName = Drawing.FontFamily.GenericSerif.Name
                End Try
            End Set
        End Property

        ''' <summary>
        ''' Amount of random warping to apply to the Captcha text.
        ''' </summary>
        Public Property FontWarp() As FontWarpFactor
            Get
                Return _fontWarp
            End Get
            Set(ByVal Value As FontWarpFactor)
                _fontWarp = Value
            End Set
        End Property

        ''' <summary>
        ''' Amount of background noise to apply to the Captcha image.
        ''' </summary>
        Public Property BackgroundNoise() As BackgroundNoiseLevel
            Get
                Return _backgroundNoise
            End Get
            Set(ByVal Value As BackgroundNoiseLevel)
                _backgroundNoise = Value
            End Set
        End Property

        Public Property LineNoise() As LineNoiseLevel
            Get
                Return _lineNoise
            End Get
            Set(ByVal value As LineNoiseLevel)
                _lineNoise = value
            End Set
        End Property

        ''' <summary>
        ''' A string of valid characters to use in the Captcha text. 
        ''' A random character will be selected from this string for each character.
        ''' </summary>
        Public Property TextChars() As String
            Get
                Return _randomTextChars
            End Get
            Set(ByVal Value As String)
                _randomTextChars = Value
                _randomText = GenerateRandomText()
            End Set
        End Property

        ''' <summary>
        ''' Number of characters to use in the Captcha text. 
        ''' </summary>
        Public Property TextLength() As Integer
            Get
                Return _randomTextLength
            End Get
            Set(ByVal Value As Integer)
                _randomTextLength = Value
                _randomText = GenerateRandomText()
            End Set
        End Property

        ''' <summary>
        ''' Returns the randomly generated Captcha text.
        ''' </summary>
        Public ReadOnly Property [Text]() As String
            Get
                Return _randomText
            End Get
        End Property

        ''' <summary>
        ''' Width of Captcha image to generate, in pixels 
        ''' </summary>
        Public Property Width() As Integer
            Get
                Return _width
            End Get
            Set(ByVal Value As Integer)
                If (Value <= 60) Then
                    Throw New ArgumentOutOfRangeException("width", Value, "width must be greater than 60.")
                End If
                _width = Value
            End Set
        End Property

        ''' <summary>
        ''' Height of Captcha image to generate, in pixels 
        ''' </summary>
        Public Property Height() As Integer
            Get
                Return _height
            End Get
            Set(ByVal Value As Integer)
                If Value <= 30 Then
                    Throw New ArgumentOutOfRangeException("height", Value, "height must be greater than 30.")
                End If
                _height = Value
            End Set
        End Property

        ''' <summary>
        ''' A semicolon-delimited list of valid fonts to use when no font is provided.
        ''' </summary>
        Public Property FontWhitelist() As String
            Get
                Return _fontWhitelist
            End Get
            Set(ByVal value As String)
                _fontWhitelist = value
            End Set
        End Property

#End Region

        Public Sub New()
            Try
                _rand = New Random
                _fontWarp = FontWarpFactor.Medium
                _backgroundNoise = BackgroundNoiseLevel.Low
                _lineNoise = LineNoiseLevel.Low
                _width = 180
                _height = 40
                _randomTextLength = 5
                _randomTextChars = "ACDEFGHJKLNPQRTUVXYZ2346789"
                _fontFamilyName = ""
                ' -- a list of known good fonts in on both Windows XP and Windows Server 2003
                _fontWhitelist = _
                    "arial;arial black;comic sans ms;courier new;" & _
                    "lucida console;lucida sans unicode;microsoft sans serif;" & _
                    "tahoma;times new roman;trebuchet ms;verdana"
                _randomText = GenerateRandomText()
                _generatedAt = DateTime.Now
                _guid = Guid.NewGuid.ToString()
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub

        ''' <summary>
        ''' Forces a new Captcha image to be generated using current property value settings.
        ''' </summary>
        Public Function RenderImage() As Bitmap
            Return GenerateImagePrivate()
        End Function

        ''' <summary>
        ''' Returns a random font family from the font whitelist
        ''' </summary>
        Private Function RandomFontFamily() As String
            Try
                Static ff() As String
                '-- small optimization so we don't have to split for each char
                If ff Is Nothing Then
                    ff = _fontWhitelist.Split(";"c)
                End If
                Return ff(_rand.Next(0, ff.Length - 1))
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "RandomFontFamily", ex, ""))
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' generate random text for the CAPTCHA
        ''' </summary>
        Private Function GenerateRandomText() As String
            Try
                'Dim sb As New System.Text.StringBuilder(_randomTextLength)
                'Dim maxLength As Integer = _randomTextChars.Length
                'For n As Integer = 0 To _randomTextLength - 1
                '    sb.Append(_randomTextChars.Substring(_rand.Next(maxLength), 1))
                'Next
                'Return sb.ToString
                Dim lowercase As Boolean = True
                Dim size As Long = _randomTextLength

                Dim options As Tools.TextOptions = Tools.TextOptions.UseAlpha Or Tools.TextOptions.UseNumeric Or Tools.TextOptions.UnambiguousCharacters

                If lowerCase Then options = options Or Tools.TextOptions.LowerCase
                Return Eonic.Tools.Text.RandomPassword(size, _randomTextChars, options)

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GenerateRandomText", ex, ""))
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Returns a random point within the specified x and y ranges
        ''' </summary>
        Private Function RandomPoint(ByVal xmin As Integer, ByVal xmax As Integer, ByRef ymin As Integer, ByRef ymax As Integer) As PointF
            Return New PointF(_rand.Next(xmin, xmax), _rand.Next(ymin, ymax))
        End Function

        ''' <summary>
        ''' Returns a random point within the specified rectangle
        ''' </summary>
        Private Function RandomPoint(ByVal rect As Rectangle) As PointF
            Return RandomPoint(rect.Left, rect.Width, rect.Top, rect.Bottom)
        End Function

        ''' <summary>
        ''' Returns a GraphicsPath containing the specified string and font
        ''' </summary>
        Private Function TextPath(ByVal s As String, ByVal f As Font, ByVal r As Rectangle) As GraphicsPath
            Dim sf As StringFormat = New StringFormat
            sf.Alignment = StringAlignment.Near
            sf.LineAlignment = StringAlignment.Near
            Dim gp As GraphicsPath = New GraphicsPath
            gp.AddString(s, f.FontFamily, CType(f.Style, Integer), f.Size, r, sf)
            Return gp
        End Function

        ''' <summary>
        ''' Returns the CAPTCHA font in an appropriate size 
        ''' </summary>
        Private Function GetFont() As Font
            Try
                Dim fsize As Single
                Dim fname As String = _fontFamilyName
                If fname = "" Then
                    fname = RandomFontFamily()
                End If
                Select Case Me.FontWarp
                    Case FontWarpFactor.None
                        fsize = Convert.ToInt32(_height * 0.7)
                    Case FontWarpFactor.Low
                        fsize = Convert.ToInt32(_height * 0.8)
                    Case FontWarpFactor.Medium
                        fsize = Convert.ToInt32(_height * 0.85)
                    Case FontWarpFactor.High
                        fsize = Convert.ToInt32(_height * 0.9)
                    Case FontWarpFactor.Extreme
                        fsize = Convert.ToInt32(_height * 0.95)
                End Select
                Return New Font(fname, fsize, FontStyle.Bold)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetFont", ex, ""))
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Renders the CAPTCHA image
        ''' </summary>
        Private Function GenerateImagePrivate() As Bitmap
            Try

                Dim fnt As Font = Nothing
                Dim rect As Rectangle
                Dim br As Brush
                Dim bmp As Bitmap = New Bitmap(_width, _height, PixelFormat.Format32bppArgb)
                Dim gr As Graphics = Graphics.FromImage(bmp)
                gr.SmoothingMode = SmoothingMode.AntiAlias

                '-- fill an empty white rectangle
                rect = New Rectangle(0, 0, _width, _height)
                br = New SolidBrush(Color.White)
                gr.FillRectangle(br, rect)

                Dim charOffset As Integer = 0
                Dim charWidth As Double = _width / _randomTextLength
                Dim rectChar As Rectangle

                For Each c As Char In _randomText
                    '-- establish font and draw area
                    fnt = GetFont()
                    rectChar = New Rectangle(Convert.ToInt32(charOffset * charWidth), 0, Convert.ToInt32(charWidth), _height)

                    '-- warp the character
                    Dim gp As GraphicsPath = TextPath(c, fnt, rectChar)
                    WarpText(gp, rectChar)

                    '-- draw the character
                    br = New SolidBrush(Color.Black)
                    gr.FillPath(br, gp)

                    charOffset += 1
                Next

                AddNoise(gr, rect)
                AddLine(gr, rect)

                '-- clean up unmanaged resources
                fnt.Dispose()
                br.Dispose()
                gr.Dispose()

                Return bmp

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
                Return Nothing
            End Try
        End Function

        ''' <summary>
        ''' Warp the provided text GraphicsPath by a variable amount
        ''' </summary>
        Private Sub WarpText(ByVal textPath As GraphicsPath, ByVal rect As Rectangle)
            Dim WarpDivisor As Single
            Dim RangeModifier As Single

            Select Case _fontWarp
                Case FontWarpFactor.None
                    Return
                Case FontWarpFactor.Low
                    WarpDivisor = 6
                    RangeModifier = 1
                Case FontWarpFactor.Medium
                    WarpDivisor = 5
                    RangeModifier = 1.3
                Case FontWarpFactor.High
                    WarpDivisor = 4.5
                    RangeModifier = 1.4
                Case FontWarpFactor.Extreme
                    WarpDivisor = 4
                    RangeModifier = 1.5
            End Select

            Dim rectF As RectangleF
            rectF = New RectangleF(Convert.ToSingle(rect.Left), 0, Convert.ToSingle(rect.Width), rect.Height)

            Dim hrange As Integer = Convert.ToInt32(rect.Height / WarpDivisor)
            Dim wrange As Integer = Convert.ToInt32(rect.Width / WarpDivisor)
            Dim left As Integer = rect.Left - Convert.ToInt32(wrange * RangeModifier)
            Dim top As Integer = rect.Top - Convert.ToInt32(hrange * RangeModifier)
            Dim width As Integer = rect.Left + rect.Width + Convert.ToInt32(wrange * RangeModifier)
            Dim height As Integer = rect.Top + rect.Height + Convert.ToInt32(hrange * RangeModifier)

            If left < 0 Then left = 0
            If top < 0 Then top = 0
            If width > Me.Width Then width = Me.Width
            If height > Me.Height Then height = Me.Height

            Dim leftTop As PointF = RandomPoint(left, left + wrange, top, top + hrange)
            Dim rightTop As PointF = RandomPoint(width - wrange, width, top, top + hrange)
            Dim leftBottom As PointF = RandomPoint(left, left + wrange, height - hrange, height)
            Dim rightBottom As PointF = RandomPoint(width - wrange, width, height - hrange, height)

            Dim points As PointF() = New PointF() {leftTop, rightTop, leftBottom, rightBottom}
            Dim m As New Matrix
            m.Translate(0, 0)
            textPath.Warp(points, rectF, m, WarpMode.Perspective, 0)
        End Sub


        ''' <summary>
        ''' Add a variable level of graphic noise to the image
        ''' </summary>
        Private Sub AddNoise(ByVal graphics1 As Graphics, ByVal rect As Rectangle)
            Dim density As Integer
            Dim size As Integer

            Select Case _backgroundNoise
                Case BackgroundNoiseLevel.None
                    Return
                Case BackgroundNoiseLevel.Low
                    density = 30
                    size = 40
                Case BackgroundNoiseLevel.Medium
                    density = 18
                    size = 40
                Case BackgroundNoiseLevel.High
                    density = 16
                    size = 39
                Case BackgroundNoiseLevel.Extreme
                    density = 12
                    size = 38
            End Select

            Dim br As New SolidBrush(Color.Black)
            Dim max As Integer = Convert.ToInt32(Math.Max(rect.Width, rect.Height) / size)

            For i As Integer = 0 To Convert.ToInt32((rect.Width * rect.Height) / density)
                graphics1.FillEllipse(br, _rand.Next(rect.Width), _rand.Next(rect.Height), _
                    _rand.Next(max), _rand.Next(max))
            Next
            br.Dispose()
        End Sub

        ''' <summary>
        ''' Add variable level of curved lines to the image
        ''' </summary>
        Private Sub AddLine(ByVal graphics1 As Graphics, ByVal rect As Rectangle)

            Dim length As Integer
            Dim width As Single
            Dim linecount As Integer

            Select Case _lineNoise
                Case LineNoiseLevel.None
                    Return
                Case LineNoiseLevel.Low
                    length = 4
                    width = Convert.ToSingle(_height / 31.25) ' 1.6
                    linecount = 1
                Case LineNoiseLevel.Medium
                    length = 5
                    width = Convert.ToSingle(_height / 27.7777) ' 1.8
                    linecount = 1
                Case LineNoiseLevel.High
                    length = 3
                    width = Convert.ToSingle(_height / 25) ' 2.0
                    linecount = 2
                Case LineNoiseLevel.Extreme
                    length = 3
                    width = Convert.ToSingle(_height / 22.7272) ' 2.2
                    linecount = 3
            End Select

            Dim pf(length) As PointF
            Dim p As New Pen(Color.Black, width)

            For l As Integer = 1 To linecount
                For i As Integer = 0 To length
                    pf(i) = RandomPoint(rect)
                Next
                graphics1.DrawCurve(p, pf, 1.75)
            Next

            p.Dispose()
        End Sub

    End Class
End Class