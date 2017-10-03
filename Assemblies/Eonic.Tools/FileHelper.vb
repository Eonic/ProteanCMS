Imports System
Imports System.Text
Imports System.IO

'' ---------------------------------------------------------
'' Class       :   Eonic.Tools.FileHelper
'' Author      :   Ali Granger
'' Website     :   www.eonic.co.uk
'' Description :   File classes and functions.
''
'' (c) Copyright 2010 Eonic Ltd.
'' ---------------------------------------------------------

Public Class FileHelper




    Shared Function ReplaceIllegalChars(ByVal filePath As String, Optional ByVal replacementChar As String = "-") As String

        Try

            ' Replace illegal characters 
            For Each illegalChar As Char In Path.GetInvalidFileNameChars()
                filePath = filePath.Replace(illegalChar.ToString(), replacementChar)
            Next

            For Each illegalChar As Char In Path.GetInvalidPathChars()
                filePath = filePath.Replace(illegalChar.ToString(), replacementChar)
            Next

            Return filePath

        Catch ex As Exception
            Return ""
        End Try

    End Function


    ''' <summary>
    ''' Returns a MIME type for a given extension
    ''' </summary>
    ''' <param name="extension"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Shared Function GetMIMEType(ByVal extension As String) As String

        Dim mimeType As String

        Select Case extension.ToUpper
            'Common documents
            Case "TXT", "TEXT", "JS", "VBS", "ASP", "CGI", "PL", "NFO", "ME", "DTD"
                mimeType = "text/plain"
            Case "HTM", "HTML", "HTA", "HTX", "MHT"
                mimeType = "text/html"
            Case "CSV"
                mimeType = "text/comma-separated-values"
            Case "JS"
                mimeType = "text/javascript"
            Case "CSS"
                mimeType = "text/css"
            Case "PDF"
                mimeType = "application/pdf"
            Case "RTF"
                mimeType = "application/rtf"
            Case "XML", "XSL", "XSLT"
                mimeType = "text/xml"
            Case "WPD"
                mimeType = "application/wordperfect"
            Case "WRI"
                mimeType = "application/mswrite"
            Case "XLS", "XLS3", "XLS4", "XLS5", "XLW"
                mimeType = "application/msexcel"
            Case "DOC"
                mimeType = "application/msword"
            Case "PPT", "PPS"
                mimeType = "application/mspowerpoint"

                'WAP/WML files 
            Case "WML"
                mimeType = "text/vnd.wap.wml"
            Case "WMLS"
                mimeType = "text/vnd.wap.wmlscript"
            Case "WBMP"
                mimeType = "image/vnd.wap.wbmp"
            Case "WMLC"
                mimeType = "application/vnd.wap.wmlc"
            Case "WMLSC"
                mimeType = "application/vnd.wap.wmlscriptc"

                'Images
            Case "GIF"
                mimeType = "image/gif"
            Case "JPG", "JPE", "JPEG"
                mimeType = "image/jpeg"
            Case "PNG"
                mimeType = "image/png"
            Case "BMP"
                mimeType = "image/bmp"
            Case "TIF", "TIFF"
                mimeType = "image/tiff"
            Case "AI", "EPS", "PS"
                mimeType = "application/postscript"

                'Sound files
            Case "AU", "SND"
                mimeType = "audio/basic"
            Case "WAV"
                mimeType = "audio/wav"
            Case "RA", "RM", "RAM"
                mimeType = "audio/x-pn-realaudio"
            Case "MID", "MIDI"
                mimeType = "audio/x-midi"
            Case "MP3"
                mimeType = "audio/mp3"
            Case "M3U"
                mimeType = "audio/m3u"

                'Video/Multimedia files
            Case "ASF"
                mimeType = "video/x-ms-asf"
            Case "AVI"
                mimeType = "video/avi"
            Case "MPG", "MPEG"
                mimeType = "video/mpeg"
            Case "QT", "MOV", "QTVR"
                mimeType = "video/quicktime"
            Case "SWA"
                mimeType = "application/x-director"
            Case "SWF"
                mimeType = "application/x-shockwave-flash"
                'Compressed/archives
            Case "ZIP"
                mimeType = "application/x-zip-compressed"
            Case "GZ"
                mimeType = "application/x-gzip"
            Case "RAR"
                mimeType = "application/x-rar-compressed"

                'Miscellaneous
            Case "COM", "EXE", "DLL", "OCX"
                mimeType = "application/octet-stream"

                'Unknown (send as binary stream)
            Case Else
                mimeType = "application/octet-stream"
        End Select

        Return mimeType

    End Function



End Class

