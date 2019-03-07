Imports System.Xml
Imports System

Public Class CSVHelper

    Dim oTable As DataTable
    Dim oError As Exception = Nothing

    Public Sub New(ByVal cTableName As String, ByVal cFileString As String, ByVal cDelimiter As String, ByVal cQualifier As String, ByVal bColumnNames As Boolean)
        Try

            oTable = New DataTable
            oTable.TableName = cTableName
            Convert(cFileString, cDelimiter, cQualifier, bColumnNames)
        Catch ex As Exception
            If oError Is Nothing Then oError = ex
        End Try
    End Sub

    Private Sub Convert(ByVal cFileString As String, ByVal cDelimiter As String, ByVal cQualifier As String, ByVal bColumnNames As Boolean)
        If Not oError Is Nothing Then Exit Sub
        Try
            Dim cRows() As String = GetRows(cFileString)
            Dim i As Integer
            For i = 0 To UBound(cRows)
                If i = 0 And bColumnNames Then
                    SetColNames(GetColumns(cRows(i), cDelimiter, cQualifier))
                Else
                    AddRow(GetColumns(cRows(i), cDelimiter, cQualifier))
                End If
            Next
        Catch ex As Exception
            If oError Is Nothing Then oError = ex
        End Try
    End Sub

    Private Function GetRows(ByVal cFileString As String) As String()
        If Not oError Is Nothing Then Return Nothing
        Try
            Return Split(cFileString, vbCrLf)
        Catch ex As Exception
            If oError Is Nothing Then oError = ex
            Return Nothing
        End Try
    End Function

    Private Function GetColumns(ByVal cRow As String, ByVal cDelimiter As String, ByVal cQualifier As String) As String()
        If Not oError Is Nothing Then Return Nothing
        Try
            Dim cCols() As String = Split(cRow, cDelimiter)
            Dim i As Integer
            For i = 0 To UBound(cCols)
                If Not cCols(i) = cQualifier And cCols(i).Length > 0 Then
                    If cCols(i).Substring(0, 1) = cQualifier Then
                        cCols(i) = Right(cCols(i), cCols(i).Length - 1)
                    End If
                Else
                    cCols(i) = ""
                End If
                If Not cCols(i) = cQualifier And cCols(i).Length > 0 Then
                    If cCols(i).Substring(cCols(i).Length - 1, 1) = cQualifier Then
                        cCols(i) = Left(cCols(i), cCols(i).Length - 1)
                    End If
                Else
                    cCols(i) = ""
                End If
            Next
            Return cCols
        Catch ex As Exception
            If oError Is Nothing Then oError = ex
            Return Nothing
        End Try
    End Function

    Private Sub SetColNames(ByVal cCols() As String)
        If Not oError Is Nothing Then Exit Sub
        Try
            Dim i As Integer
            For i = 0 To UBound(cCols)
                oTable.Columns.Add(New DataColumn(cCols(i)))
            Next
        Catch ex As Exception
            If oError Is Nothing Then oError = ex
        End Try
    End Sub

    Private Sub AddRow(ByVal cCols() As String)
        If Not oError Is Nothing Then Exit Sub
        Try
            Dim i As Integer
            For i = 0 To UBound(cCols)
                If i >= oTable.Columns.Count Then
                    oTable.Columns.Add(New DataColumn("Column_" & i))
                End If
            Next
            oTable.Rows.Add(cCols)
        Catch ex As Exception
            If oError Is Nothing Then oError = ex
        End Try
    End Sub

    Public ReadOnly Property Table() As DataTable
        Get
            Return oTable
        End Get
    End Property

    Public ReadOnly Property Exception() As Exception
        Get
            Return oError
        End Get
    End Property

    Public ReadOnly Property XML() As XmlDocument
        Get
            Try
                Dim oDS As New DataSet("CSV")
                oDS.Tables.Add(oTable)
                Dim oXML As New XmlDocument
                oXML.InnerXml = oDS.GetXml
                Return oXML
            Catch ex As Exception
                If oError Is Nothing Then oError = ex
                Return Nothing
            End Try
        End Get
    End Property
End Class

