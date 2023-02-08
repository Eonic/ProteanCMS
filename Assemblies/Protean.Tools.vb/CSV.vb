Imports System.Xml
Public Class Csv
    Implements IDisposable

#Region "Declarations"
    Dim oTable As DataTable
    '    Dim oError As Exception = Nothing
    Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
    Private Const mcModuleName As String = "Protean.Tools.Csv"
#End Region
#Region "Properties"
    Public ReadOnly Property Table() As DataTable
        Get
            Return oTable
        End Get
    End Property

    Public ReadOnly Property Xml() As XPath.IXPathNavigable
        Get
            Try
                Dim oDS As New DataSet("CSV")
                oDS.Tables.Add(oTable)
                Dim oXml As New XmlDocument
                oXml.InnerXml = oDS.GetXml
                Return oXml
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Xml", ex, ""))
                Return Nothing
            End Try
        End Get
    End Property
#End Region
#Region "Public Procedures"
    Public Sub New(ByVal Tablename As String, ByVal Filebody As String, ByVal Delimiter As String, ByVal Qualifier As String, ByVal Columnnames As Boolean)
        Try

            oTable = New DataTable
            oTable.TableName = Tablename
            Convert(Filebody, Delimiter, qualifier, columnNames)
        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
        End Try
    End Sub
#End Region
#Region "Private Procedures"
    Private Sub Convert(ByVal Filebody As String, ByVal Delimiter As String, ByVal Qualifier As String, ByVal Columnnames As Boolean)

        Try
            Dim cRows() As String = GetRows(fileBody)
            Dim i As Integer
            For i = 0 To UBound(cRows)
                If i = 0 And columnNames Then
                    SetColNames(GetColumns(cRows(i), delimiter, qualifier))
                Else
                    AddRow(GetColumns(cRows(i), delimiter, qualifier))
                End If
            Next
        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Convert", ex, ""))
        End Try
    End Sub

    Private Function GetRows(ByVal Filebody As String) As String()

        Try
            Return Split(fileBody, vbCrLf)
        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetRows", ex, ""))
            Return Nothing
        End Try
    End Function

    Private Function GetColumns(ByVal Rowcolumns As String, ByVal Delimiter As String, ByVal Qualifier As String) As String()

        Try
            Dim cCols() As String = Split(Rowcolumns, Delimiter)
            Dim i As Integer
            For i = 0 To UBound(cCols)
                If Not cCols(i) = Qualifier And cCols(i).Length > 0 Then
                    If cCols(i).Substring(0, 1) = Qualifier Then
                        cCols(i) = Right(cCols(i), cCols(i).Length - 1)
                    End If
                Else
                    cCols(i) = ""
                End If
                If Not cCols(i) = Qualifier And cCols(i).Length > 0 Then
                    If cCols(i).Substring(cCols(i).Length - 1, 1) = Qualifier Then
                        cCols(i) = Left(cCols(i), cCols(i).Length - 1)
                    End If
                Else
                    cCols(i) = ""
                End If
            Next
            Return cCols
        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetColumns", ex, ""))
            Return Nothing
        End Try
    End Function

    Private Sub SetColNames(ByVal Cols() As String)

        Try
            Dim i As Integer
            For i = 0 To UBound(cols)
                oTable.Columns.Add(New DataColumn(cols(i)))
            Next
        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SetColNames", ex, ""))
        End Try
    End Sub

    Private Sub AddRow(ByVal Cols() As String)

        Try
            Dim i As Integer
            For i = 0 To UBound(Cols)
                If i >= oTable.Columns.Count Then
                    oTable.Columns.Add(New DataColumn("Column_" & i))
                End If
            Next
            oTable.Rows.Add(Cols)
        Catch ex As Exception
            RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddRow", ex, ""))
        End Try
    End Sub

#End Region

    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal Disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then

                If Not oTable Is Nothing Then
                    oTable.Dispose()
                End If

            End If


        End If
        Me.disposedValue = True
    End Sub

#Region " IDisposable Support "
    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

