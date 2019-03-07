<%@ WebHandler Language="VB" Class="eonicWebExportAsFile" %>

Imports System
Imports System.Web

Public Class eonicWebExportAsFile : Implements IHttpHandler, IRequiresSessionState

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest

        ' Declarations
        Dim contentType As String = ""
        Dim siteXSL As String = ""
        Dim fileExtension As String = ""

        '  Get the values from request string
        Dim mode As String = context.Request("format") & ""
        Dim filename As String = context.Request("ewCmd") & Format(Date.Now, "yyyyMMddhhmmss")
        Dim reportXsl As String = context.Request("reportXsl") & ""



        'Customise the output
        ' Default is Excel
        Dim ofs As New Protean.fsHelper
        Select Case mode.ToLower
            Case "txt"
                contentType = "text/plain"
                If reportXsl <> "" Then
                    siteXSL = ofs.checkCommonFilePath("/xsl/reports/" & reportXsl & ".xsl")
                Else
                    siteXSL = ofs.checkCommonFilePath("/xsl/tools/txt.xsl")
                End If
                fileExtension = "txt"
            Case "csv"
                'contentType = "text/csv"
                contentType = "text/plain"
                If reportXsl <> "" Then
                    siteXSL = ofs.checkCommonFilePath("/xsl/reports/" & reportXsl & ".xsl")
                Else
                    siteXSL = ofs.checkCommonFilePath("/xsl/tools/csv.xsl")
                End If
                fileExtension = "csv"
            Case "xml"
                contentType = "text/xml"
                If reportXsl <> "" Then
                    siteXSL = ofs.checkCommonFilePath("/xsl/reports/" & reportXsl & ".xsl")
                Else
                    siteXSL = ofs.checkCommonFilePath("/xsl/tools/xml.xsl")
                End If
                fileExtension = "xml"
            Case Else
                contentType = "application/vnd.ms-excel"
                If reportXsl <> "" Then
                    siteXSL = ofs.checkCommonFilePath("/xsl/reports/" & reportXsl & ".xsl")
                Else
                    siteXSL = ofs.checkCommonFilePath("/xsl/reports/formats/Report-Excel-2000.xsl")

                End If
                fileExtension = "xlsx"
        End Select

        ' Initialise EonicWeb
        Dim oEw As Protean.Cms = New Protean.Cms
        '  oEw.mbAdminMode = True
        '  oEw.Open()

        oEw.InitializeVariables()
        oEw.mcEwSiteXsl = siteXSL

        ' Determine whether to show the XML or not. 
        If context.Request("format") = "rawxml" Then
            oEw.mcContentType = "application/xml"
            oEw.mbOutputXml = True
        Else
            ' Set the response headers
            oEw.mcContentType = contentType
            '   oEw.mcContentDisposition = "attachment;filename=" & filename & "." & fileExtension

        End If

        ' Get the output
        oEw.GetPageHTML()
        oEw = Nothing
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

End Class