'***********************************************************************
' $Library:     eonic.FileDoc
' $Revision:    Unknown  
' $Date:        Unknown
' $Author:      Unknown (Barry Rushton?)
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Eonicweb Ltd.
'***********************************************************************

Imports System.Data
Imports System.Data.SqlClient
Imports System.Xml
Imports System.IO
Imports System.Configuration
Imports System.Collections
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Security.Principal
Imports System.Web.Configuration
Imports Protean.Tools.DelegateWrappers
Imports Protean.Tools
Imports System


Public Class FileDoc
    Dim cPath As String = ""
    Dim mcModuleName As String = "FileDoc"

    Public Sub New(ByVal thePath As String)
        PerfMon.Log("FileDoc", "New")
        Try
            cPath = thePath
        Catch ex As Exception
            '  returnException(mcModuleName, "DocWord", ex, "")
            cPath = ""
        End Try
    End Sub

    Public ReadOnly Property Extension() As String
        Get
            PerfMon.Log("FileDoc", "Extension")
            Try
                Dim cTest As String = ""
                Dim cExt As String = ""
                If cPath.Contains(".") Then
                    Dim i As Integer = 1
                    Do Until cTest = "."
                        cTest = Left(Right(cPath, i), 1)
                        If i > 1 Then cExt = Right(cPath, i - 1)
                        i += 1
                    Loop
                End If
                Return cExt
            Catch ex As Exception
                '    returnException(mcModuleName, "Extension", ex, "")
                Return ""
            End Try
        End Get
    End Property

    Public ReadOnly Property Text() As String
        Get
            PerfMon.Log("FileDoc", "Text")
            Try
                Select Case Extension
                    Case "doc"
                        'word
                        Return DocWord()
                    Case "rtf"
                        'word
                        Return DocWord()
                    Case "txt"
                        'text
                        Return DocText()
                    Case "xls"
                        'excel
                        Return DocExcel()
                    Case "csv"
                        'text
                        Return DocText()
                    Case "pdf"
                        'pdf
                        Return DocPDF()
                    Case "zip"
                        'ignore
                        Return ""
                    Case Else
                        Return ""
                End Select
            Catch ex As Exception
                '   returnException(mcModuleName, "Text", ex, "")
                Return ""
            End Try
        End Get
    End Property

    Public Overrides Function ToString() As String
        PerfMon.Log("FileDoc", "ToString")
        Try
            Return Me.Text
        Catch ex As Exception
            '     returnException(mcModuleName, "ToString", ex, "")
            Return ""
        End Try
    End Function

#Region "Typed Document Handlers"
    Private Function DocWord() As String
        PerfMon.Log("FileDoc", "DocWord")
        Dim oIF As Protean.Tools.IFilter.DefaultParser
        Try
            oIF = New IFilter.DefaultParser
            Dim cReturn As String = IFilter.DefaultParser.Extract(cPath)
            oIF = Nothing
            Return cReturn
        Catch ex As Exception
            oIF = Nothing
            Return Nothing
        End Try
    End Function

    Private Function DocExcel() As String
        PerfMon.Log("FileDoc", "DocExcel")
        Dim oIF As Tools.IFilter.DefaultParser
        Try
            oIF = New IFilter.DefaultParser
            Dim cReturn As String = IFilter.DefaultParser.Extract(cPath)
            oIF = Nothing
            Return cReturn
        Catch ex As Exception
            oIF = Nothing
            Return ""
        End Try
    End Function

    Private Function DocPDF() As String
        PerfMon.Log("FileDoc", "DocPDF")
        Dim oIF As IFilter.DefaultParser
        Try
            oIF = New IFilter.DefaultParser
            Dim cReturn As String = IFilter.DefaultParser.Extract(cPath)
            oIF = Nothing
            Return cReturn
        Catch ex As Exception

            oIF = Nothing
            Return ""
        End Try
    End Function

    Private Function DocText() As String
        PerfMon.Log("FileDoc", "DocText")
        Dim oIF As IFilter.DefaultParser
        Try
            oIF = New IFilter.DefaultParser
            Dim cReturn As String = IFilter.DefaultParser.Extract(cPath)
            oIF = Nothing
            Return cReturn
        Catch ex As Exception

            oIF = Nothing
            Return ""
        End Try
    End Function
#End Region
End Class



