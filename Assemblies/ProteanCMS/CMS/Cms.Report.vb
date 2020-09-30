Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Web.HttpUtility
Imports VB = Microsoft.VisualBasic
Imports System

Partial Public Class Cms

    Public Class Report

        Dim myWeb As Cms
        Dim moPageXml As XmlDocument 'the actual page, given from the web object
        Public moReport As XmlElement
        Dim moDB As dbHelper

        Shadows mcModuleName As String = "Eonic.Report" 'module name


        Public Sub New(ByRef aWeb As Protean.Cms)
            PerfMon.Log(mcModuleName, "New")
            Try
                myWeb = aWeb
                moPageXml = myWeb.moPageXml
                moDB = myWeb.moDbHelper
            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "New", ex, "", , gbDebug)
            End Try
        End Sub

        Public Shadows Sub close()
            PerfMon.Log("mcModuleName", "close")
            Dim cProcessInfo As String = ""
            Try

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "Close", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Overridable Sub apply()
            PerfMon.Log(mcModuleName, "apply")

            Dim oElmt As XmlElement
            Dim oReport As XmlElement
            Dim bOnArticleDisableReport As Boolean = False

            Try
                ' Go through the content nodes of type report
                For Each moReport In moPageXml.SelectNodes("/Page/Contents/Content[@type='Report']")

                    oElmt = moReport.SelectSingleNode("OnArticle[node()='DisableReport']")
                    If Not (oElmt Is Nothing) Then bOnArticleDisableReport = True

                    If Not (myWeb.moRequest("artid") <> "" And bOnArticleDisableReport) Then
                        ' Run Reports

                        oReport = addElement(moReport, "Report")

                        Select Case moReport.SelectSingleNode("Type").InnerText

                            Case "Eonic_Stored_Procedure"
                                report_StoredProcedure(oReport)

                        End Select

                    End If

                Next

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "apply", ex, "", , gbDebug)
            End Try

        End Sub

        Public Sub report_StoredProcedure(ByRef oReport As XmlElement)
            PerfMon.Log(mcModuleName, "report_StoredProcedure")

            '***********************************************************************
            ' Report :      Stored Procedure
            ' Parameters :  Param 1, "sp", is the name of the SP to execute
            '               Other parameters are the parameters fed into the SP
            ' Description : This can be used to pull back a stored procedure from
            '               from the database.
            '               For security, the following measures are taken:
            '               - only stored procedures can be called.
            '               - only stored procedures with the prefix "esp_" can
            '                 be called.
            '               - stored procedures can only be called as read only.
            '***********************************************************************


            Dim oElmt As XmlElement
            Dim oRow As XmlElement
            Dim oDr As SqlDataReader
            Dim nColumn As Integer
            Dim nColumns As Integer
            Dim cStoredProcedure As String = ""

            Try

                'Get the Stored Procedure name
                oElmt = moReport.SelectSingleNode("Parameters/Parameter[@name='sp' and node()!='']")
                If Not (oElmt Is Nothing) Then
                    cStoredProcedure = oElmt.InnerText

                    ' Security check : Prefix
                    If cStoredProcedure.StartsWith("esp_") Then

                        Dim oParams As New Hashtable
                        Dim cValue As String = ""


                        ' Build the other parameters
                        For Each oElmt In moReport.SelectNodes("Parameters/Parameter[@name!='sp']")
                            If oElmt.GetAttribute("name") <> "" Then
                                oParams.Add(oElmt.GetAttribute("name"), oElmt.InnerText)
                            End If
                        Next

                        ' Normally we would execute the SP as a Dataset, 
                        ' but because of the security constraints needed,
                        ' we'll run this as a Reader, and convert into Xml
                        oDr = moDB.getDataReader(cStoredProcedure, CommandType.StoredProcedure, oParams)

                        If oDr.HasRows Then
                            nColumns = oDr.FieldCount


                            While oDr.Read
                                oRow = addElement(oReport, "row")
                                For nColumn = 0 To nColumns - 1
                                    If Not (oDr.IsDBNull(nColumn)) Then
                                        Select Case oDr.GetFieldType(nColumn).ToString
                                            Case "SqlDateTime"
                                                cValue = Protean.Tools.Xml.XmlDate(oDr.Item(nColumn).ToString, True)
                                            Case Else
                                                cValue = oDr.Item(nColumn).ToString
                                        End Select
                                    End If

                                    ' Check if it's xml
                                    Dim bAddAsXml As Boolean = False
                                    If oDr.GetName(nColumn).ToLower.Contains("xml") Then bAddAsXml = True

                                    ' Add the data to the row.
                                    addElement(oRow, oDr.GetName(nColumn), cValue, bAddAsXml)
                                Next
                            End While
                        End If

                        oDr.Close()
                    End If
                End If

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "report_StoredProcedure", ex, "", , gbDebug)
            End Try
        End Sub

        Sub SetDefaultSortColumn(ByVal nSortColumn As Long, Optional ByVal nSortDirection As SortDirection = SortDirection.Ascending)
            PerfMon.Log("stdTools", "SetDefaultSortColumn")
            Try
                Dim oElmt As XmlElement
                If moPageXml.DocumentElement Is Nothing Then
                    myWeb.GetPageXML()
                End If

                If moPageXml.SelectSingleNode("/Page/Request/QueryString/Item[@name='sortCol']") Is Nothing Then
                    ' Add a default column sort
                    oElmt = addNewTextNode("Item", moPageXml.SelectSingleNode("/Page/Request/QueryString"), nSortColumn, , False)
                    oElmt.SetAttribute("name", "sortCol")

                    oElmt = moPageXml.SelectSingleNode("/Page/Request/QueryString/Item[@name='sortDir']")
                    If Not (oElmt Is Nothing) Then
                        oElmt.InnerText = SortDirectionVal(nSortDirection)
                    Else
                        oElmt = addNewTextNode("Item", moPageXml.SelectSingleNode("/Page/Request/QueryString"), SortDirectionVal(nSortDirection), , False)
                        oElmt.SetAttribute("name", "sortDir")
                    End If
                End If
            Catch ex As Exception
                returnException(myWeb.msException, "stdTools", "SetDefaultSortColumn", ex, "", "", gbDebug)
            End Try
        End Sub

    End Class
End Class
