<%@ WebHandler Language="VB" Class="ewTestPDFThumbnail" %>

Imports System
Imports System.IO
Imports System.Web

Public Class ewTestPDFThumbnail : Implements IHttpHandler, IRequiresSessionState

    Private Const TempPdfRelPath As String = "/ptn/tools/tests/temp/test-thumbnail-source.pdf"
    Private Const TempImgRelPath As String = "/ptn/tools/tests/temp/test-thumbnail-output.png"

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        context.Response.ContentType = "text/html"

        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine("<!DOCTYPE html><html><head><title>PDF Thumbnail Test</title></head><body style='font-family:Arial;padding:20px'>")
        sb.AppendLine("<h1>PDF Thumbnail Test</h1>")
        sb.AppendLine("<p>Tests PDF creation via <strong>SelectPdf</strong> and thumbnail generation via <strong>SoundInTheory.DynamicImage</strong>.</p>")
        sb.AppendLine("<hr/>")

        Try
            ' Step 1: Ensure temp directory exists
            Dim tempPdfPath As String = context.Server.MapPath(TempPdfRelPath)
            Dim tempDir As String = Path.GetDirectoryName(tempPdfPath)

            If Not Directory.Exists(tempDir) Then
                Directory.CreateDirectory(tempDir)
            End If

            sb.AppendLine("<p style='color:green'>&#10003; Step 1: Temp directory ready: " & tempDir & "</p>")

            ' Step 2: Create a simple test PDF dynamically using SelectPdf
            Dim testHtml As String = "<!DOCTYPE html><html><body style='font-family:Arial;padding:20px'>" &
                                     "<h1>DynamicImage PDF Thumbnail Test</h1>" &
                                     "<p>This PDF was generated dynamically to verify that SoundInTheory.DynamicImage is correctly installed.</p>" &
                                     "<p>Generated: " & DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") & " UTC</p>" &
                                     "</body></html>"

            Dim converter As New SelectPdf.HtmlToPdf()
            converter.Options.MaxPageLoadTime = 15
            Dim pdfDoc As SelectPdf.PdfDocument = converter.ConvertHtmlString(testHtml, "")
            pdfDoc.Save(tempPdfPath)
            pdfDoc.Close()
            pdfDoc = Nothing
            converter = Nothing

            sb.AppendLine("<p style='color:green'>&#10003; Step 2: Test PDF created at: " & TempPdfRelPath & "</p>")

            ' Step 3: Generate PNG thumbnail from PDF using SoundInTheory.DynamicImage
            Dim oPDF As New Protean.Tools.PDF()
            Dim oThumb As New Protean.Tools.PDF.PDFThumbNail()
            oThumb.FilePath = TempPdfRelPath
            oThumb.newImageFilepath = TempImgRelPath
            oThumb.maxWidth = 300
            oThumb.goServer = context.Server

            oPDF.GeneratePDFThumbNail(oThumb)

            sb.AppendLine("<p style='color:green'>&#10003; Step 3: Thumbnail generated at: " & TempImgRelPath & "</p>")

            ' Step 4: Verify the output image exists on disk
            Dim outputPath As String = context.Server.MapPath(TempImgRelPath)
            If File.Exists(outputPath) Then
                Dim fi As New FileInfo(outputPath)
                Dim imgUrl As String = TempImgRelPath & "?t=" & DateTime.UtcNow.Ticks.ToString()
                sb.AppendLine("<p style='color:green'>&#10003; Step 4: Output image verified on disk (" & fi.Length & " bytes).</p>")
                sb.AppendLine("<hr/>")
                sb.AppendLine("<h2 style='color:green'>&#10003; SoundInTheory.DynamicImage is correctly installed and functional.</h2>")
                sb.AppendLine("<p><img src='" & imgUrl & "' style='border:1px solid #ccc;max-width:300px;display:block;margin-top:10px' /></p>")
            Else
                sb.AppendLine("<p style='color:red'>&#10007; Step 4: Output image was not found at: " & TempImgRelPath & "</p>")
            End If

        Catch ex As Exception
            sb.AppendLine("<hr/>")
            sb.AppendLine("<h2 style='color:red'>&#10007; Error</h2>")
            sb.AppendLine("<p><strong>" & context.Server.HtmlEncode(ex.Message) & "</strong></p>")
            If ex.InnerException IsNot Nothing Then
                sb.AppendLine("<p>" & context.Server.HtmlEncode(ex.InnerException.Message) & "</p>")
            End If
            sb.AppendLine("<pre style='background:#f8f8f8;border:1px solid #ccc;padding:10px;font-size:12px'>" & context.Server.HtmlEncode(ex.StackTrace) & "</pre>")
        End Try

        sb.AppendLine("</body></html>")
        context.Response.Write(sb.ToString())

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class