Imports System.Xml
Imports System.IO
Imports System.Data
Imports System.Data.OleDb
Imports System.Linq
Imports System.Collections.Generic
Imports System.Text.RegularExpressions
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet

''' <summary>
'''   <para>   Eonic.Tools.Conversion is designed to covert from one data source to another (e.g. Excel to Xml)</para>
'''   <example>
'''     <para>Exmaple usage to convert an Excel file to XML</para>
'''     <code>Dim c As New Conversion(Conversion.Type.Excel, Conversion.Type.Xml, "c:\text.xls")</code>
'''     <code>c.Convert()</code>
'''     <code>If c.State = Status.Succeeded Then Response.Write c.Output.OuterXml Else Response.Write c.Message</code>
'''   </example>
''' </summary>
''' <remarks></remarks>
Public Class Conversion

#Region "    Declarations"

    Public Shared Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
    Private Const mcModuleName As String = "Eonic.Tools.Conversion"

    Public Enum Type

        Undefined = 0
        CSV = 1
        Excel = 2
        Xml = 3

    End Enum
    Public Enum Status
        Failed = 1
        NotReady = 2
        Ready = 100
        Succeeded = 101
    End Enum
    Public Enum StatusReason
        Undefined = 0
        InputAndOutputTypeCannotBeSame = 1
        InputOrOutputTypeNotSet = 2
        ConversionNotAvailable = 3
        InputSourceNotValidorEmpty = 4
    End Enum

    Private nInputType As Type = Type.Undefined
    Private nOutputType As Type = Type.Undefined

    Private oInput As Object = Nothing
    Private oOutputXml As XmlElement = Nothing
    Private oOutput As Object = Nothing

    Private nStatus As Status = Status.NotReady
    Private nStatusReason As StatusReason = StatusReason.Undefined

    Public writePath As String
    Public CSVseparator As Char = ","
    'Public CSVfirstLineTitles As Boolean = True
    Public CSVfirstLineTitles As String
    Public CSVLineName As String = "DataRow"

#End Region

#Region "    Properties"

    Public ReadOnly Property State() As Status
        Get
            Return Me.nStatus
        End Get
    End Property

    Public ReadOnly Property StateReason() As StatusReason
        Get
            Return Me.nStatusReason
        End Get
    End Property


    Public Property InputType() As Type
        Get
            Return nInputType
        End Get
        Set(ByVal value As Type)
            nInputType = value
            UpdateStatus()
        End Set
    End Property

    Public Property OutputType() As Type
        Get
            Return nOutputType
        End Get
        Set(ByVal value As Type)
            nOutputType = value
            UpdateStatus()
        End Set
    End Property

    Public Property Input() As Object
        Get
            Return oInput
        End Get
        Set(ByVal value As Object)
            oInput = value
            UpdateStatus()
        End Set
    End Property

    Public ReadOnly Property Output()
        Get
            Return oOutput
        End Get
    End Property

    Public ReadOnly Property Message() As String
        Get
            Dim cMessage As String = ""

            Select Case Me.nStatus
                Case Status.Failed
                    cMessage += "The conversion failed."
                Case Status.NotReady
                    cMessage += "The conversion is not ready to be processed."
                Case Status.Ready
                    cMessage += "All parameters are valid, and the conversion can proceed."
                Case Status.Failed
                    cMessage += "The conversion suceeded."
            End Select

            Select Case Me.nStatusReason
                Case StatusReason.ConversionNotAvailable
                    cMessage += " The conversion for the types requested is not available."
                Case StatusReason.InputAndOutputTypeCannotBeSame
                    cMessage += " The input type and output type are the same."
                Case StatusReason.InputOrOutputTypeNotSet
                    cMessage += " Either the input type or output type has not been specified."
                Case StatusReason.InputSourceNotValidorEmpty
                    cMessage += " The input is not valid.  It may not match the input type, may be empty, or if it is a file, it may not be available."
            End Select

            Return cMessage
        End Get
    End Property
#End Region

#Region "    Initialisation"

    Public Sub New(ByVal inputType As Eonic.Tools.Conversion.Type, ByVal outputType As Eonic.Tools.Conversion.Type)

        Me.nInputType = inputType
        Me.nOutputType = outputType
        Me.UpdateStatus()

    End Sub

    Public Sub New(ByVal inputType As Eonic.Tools.Conversion.Type, ByVal outputType As Eonic.Tools.Conversion.Type, ByVal input As Object)

        Me.nInputType = inputType
        Me.nOutputType = outputType
        Me.oInput = input
        Me.UpdateStatus()

    End Sub

    Public Sub Dispose()
        Me.nInputType = Nothing
        Me.nOutputType = Nothing
        Me.oInput = Nothing
        Me.oOutputXml = Nothing
    End Sub

#End Region

#Region "    Private Procedures"

    Private Sub UpdateStatus()

        If nInputType = Type.Undefined Or nOutputType = Type.Undefined Then
            ' Input or Output are undefined
            Me.SetStatus(Status.NotReady, StatusReason.InputOrOutputTypeNotSet)
        ElseIf nInputType = nOutputType Then
            ' Input and Output are the same
            Me.SetStatus(Status.NotReady, StatusReason.InputOrOutputTypeNotSet)
        ElseIf Not (IsConversionAvailableConversion()) Then
            ' Conversion method is not available
            Me.SetStatus(Status.NotReady, StatusReason.ConversionNotAvailable)
        ElseIf Not (IsResourceValid(Me.oInput, Me.nInputType)) Then
            ' Input is not valid
            Me.SetStatus(Status.NotReady, StatusReason.InputSourceNotValidorEmpty)
        Else
            ' Everything is okay
            Me.SetStatus(Status.Ready, StatusReason.Undefined)
        End If

    End Sub

    Private Sub SetStatus(ByVal status As Status, ByVal statusReason As StatusReason)
        Me.nStatus = status
        Me.nStatusReason = statusReason
    End Sub

    Private Sub ConvertExcelToXml()


        Dim oExcelConn As OleDbConnection = Nothing
        Dim oExcelAdapter As New OleDbDataAdapter()
        Dim oExcelDataset As DataSet
        Dim oExcelColumn As DataColumn

        Dim oWorksheets As DataTable
        Dim oWorksheet As DataRow
        Dim cWorksheetname As String = ""

        Dim oXml As XmlDataDocument
        Dim oOutputXml As New XmlDocument
        Dim oOutputRoot As XmlElement
        Dim oOutputWorksheet As XmlElement

        Dim cSql As String = ""

        Try

            ' Establish the connection
            '  oExcelConn = New OleDbConnection( _
            '              "Provider=Microsoft.Jet.OLEDB.4.0;" & _
            '              "Data Source=" & Me.oInput & ";" & _
            '             "Extended Properties=""Excel 8.0;HDR=Yes;IMEX=1;""")

            If Me.oInput.endswith(".xlsx") Then

                oOutputXml.LoadXml(GetXML(Me.oInput))
                Me.oOutputXml = oOutputXml.DocumentElement()

            ElseIf Me.oInput.endswith(".csv") Then

                oOutputXml.LoadXml(GetXML(Me.oInput))
                Me.oOutputXml = oOutputXml.DocumentElement()
            Else

                oExcelConn = New OleDbConnection(
                       "Provider=Microsoft.ACE.OLEDB.12.0;" &
                       "Data Source=" & Me.oInput & ";" &
                       "Extended Properties=Excel 12.0")

                oExcelConn.Open()

                ' Get the worksheet names
                oWorksheets = oExcelConn.GetSchema("Tables")

                ' Set up the Xml
                oOutputRoot = oOutputXml.CreateElement("Workbook")

                If oWorksheets.Rows.Count = 0 Then
                    SetStatus(Status.Failed, StatusReason.InputSourceNotValidorEmpty)
                Else
                    For Each oWorksheet In oWorksheets.Rows
                        ' .Trim("'") added to deal with worksheet names encompassed with 's
                        cWorksheetname = oWorksheet("TABLE_NAME").ToString.Trim("'")
                        If cWorksheetname.EndsWith("$") Then

                            ' Get the data
                            cSql = "SELECT * FROM [" & cWorksheetname & "]"
                            oExcelAdapter.SelectCommand = New OleDbCommand(cSql, oExcelConn)
                            oExcelDataset = New DataSet("Sheet")
                            oExcelAdapter.Fill(oExcelDataset, "Row")

                            ' Make the Column Names XML Friendly
                            For Each oExcelColumn In oExcelDataset.Tables(0).Columns
                                oExcelColumn.ColumnName = Replace(Replace(oExcelColumn.ColumnName, " ", "_"), ":", "_")
                            Next

                            ' Turn the DS into XML

                            oExcelDataset.EnforceConstraints = False

                            oXml = New XmlDataDocument(oExcelDataset)

                            oOutputWorksheet = oXml.FirstChild

                            ' Add the sheet
                            If Not (oOutputWorksheet Is Nothing) Then
                                oOutputWorksheet.SetAttribute("name", Replace(cWorksheetname, "$", ""))
                            End If
                            oOutputRoot.AppendChild(oOutputXml.ImportNode(oOutputWorksheet, True))

                        End If
                    Next

                    Me.oOutputXml = oOutputRoot
                    SetStatus(Status.Succeeded, StatusReason.Undefined)
                End If
            End If


        Catch ex As Exception

            SetStatus(Status.Failed, StatusReason.Undefined)

        Finally
            If Not (oExcelConn Is Nothing) Then
                If oExcelConn.State <> ConnectionState.Closed Then
                    oExcelConn.Close()
                End If
            End If
        End Try
    End Sub

    Private Sub ConvertCSVToXml()
        Dim firstRow As Boolean = True
        Dim _separator As Char = CSVseparator
        Dim _fieldnames As String = Nothing
        Dim succeeded As Int64 = 0
        Dim failed As Int64 = 0
        Dim ResponseXml As New XmlDocument
        Dim cProcessInfo As String
        Dim sr As StreamReader
        ' Dim writeFilePath As String = Me.writePath & "debug.txt"
        ' Dim sw As New StreamWriter(File.Open(writeFilePath, FileMode.OpenOrCreate))
        Try

            If Me.oInput.GetType() Is GetType(StreamReader) Then
                sr = Me.oInput
            Else
                sr = New StreamReader(Me.oInput.ToString())
                '      writeFilePath = Me.oInput.ToString().Replace(".csv", ".txt")
            End If

            ResponseXml.AppendChild(ResponseXml.CreateElement("Feed"))
            Dim rowTemplate As XmlElement = ResponseXml.CreateElement(CSVLineName)
            Dim sLine As String

            Dim nSepCount As Int16 = 0
            Dim afieldsTitles() As String

            If Not CSVfirstLineTitles Is Nothing And firstRow Then
                afieldsTitles = Split(CSVfirstLineTitles, _separator)
                For ii As Integer = 0 To afieldsTitles.Count - 1
                    Dim _fName As String = ""
                    _fName = afieldsTitles(ii).Replace(_separator, "")
                    rowTemplate.AppendChild(ResponseXml.CreateElement(_fName))
                Next
                firstRow = False
            End If

            ' Do While (succeeded < 10000) And Not (sr.EndOfStream)

            Do While Not (sr.EndOfStream)

                sLine = sr.ReadLine
                sLine = sLine.Replace(vbCrLf, "")

                'get the number of seps in line 1
                If nSepCount = 0 Then
                    nSepCount = sLine.Split(_separator).Length - 1
                End If
                'keep reading till no of seps are hit.
                Do While (sLine.Split(_separator).Length - 1) < nSepCount
                    sLine = sLine + sr.ReadLine
                    sLine = sLine.Replace(vbCrLf, "")
                Loop

                If sLine Is Nothing Then
                    Exit Do
                End If
                Dim fields() As String = SplitWhilePreservingQuotedValues(sLine, _separator).ToArray()

                If firstRow Then
                    For ii As Integer = 0 To fields.Count - 1
                        Dim _fName As String = ""

                        _fName = fields(ii)
                        _fName = _fName.Replace("""", "")
                        rowTemplate.AppendChild(ResponseXml.CreateElement(_fName))
                    Next
                    firstRow = False
                Else
                    Try
                        Dim bHasHtml As Boolean = False
                        Dim rowElmt As XmlElement = rowTemplate.CloneNode(True)
                        For ii As Integer = 0 To fields.Count - 1
                            Dim _fValue = fields(ii)
                            If Not (Trim(_fValue) = "") Then

                                If rowElmt.SelectSingleNode("*[position() = " & CLng(ii + 1) & "]") Is Nothing Then

                                    Dim wtf1 As XmlElement = rowElmt.SelectSingleNode("*[position() = " & CLng(ii + 1) & "]")

                                Else
                                    If _fValue.startswith("<") Then
                                        rowElmt.SelectSingleNode("*[position() = " & CLng(ii + 1) & "]").InnerXml = Eonic.Tools.Text.tidyXhtmlFrag(_fValue, True, True)
                                        bHasHtml = True
                                    Else
                                        rowElmt.SelectSingleNode("*[position() = " & CLng(ii + 1) & "]").InnerText = Trim(_fValue).Replace(vbTab, "")
                                    End If
                                End If
                            End If
                        Next

                        If bHasHtml = False Then
                            cProcessInfo = "stop"
                        End If

                        succeeded = succeeded + 1

                        rowElmt.SetAttribute("count", succeeded)

                        ResponseXml.DocumentElement.AppendChild(rowElmt)
                        ' ResponseXml.Save(writeFilePath.Replace(".txt", ".xml"))

                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine(ex.ToString())
                        failed = failed + 1
                    End Try
                End If
            Loop

            Dim rootElmt As XmlElement = ResponseXml.FirstChild
            rootElmt.SetAttribute("succeeded", succeeded)
            rootElmt.SetAttribute("failed", failed)

            Dim oOutputXml As New XmlDocument
            oOutputXml.LoadXml(ResponseXml.OuterXml)
            Me.oOutputXml = oOutputXml.DocumentElement()

        Catch ex As Exception

            SetStatus(Status.Failed, StatusReason.Undefined)

        Finally
            '  sw.Close()
            '   sw.Dispose()
            sr.Dispose()
            ResponseXml = Nothing
        End Try
    End Sub

#End Region

#Region "    Private Functions"

    Private Function SplitWhilePreservingQuotedValues(value As String, delimiter As Char) As IEnumerable(Of String)
        Try


            ' Dim csvPreservingQuotedStrings As New Regex(String.Format("(""[^""]*""|[^{0}])+", delimiter))
            Dim csvPreservingQuotedStrings As New Regex(String.Format("(?:^|{0})(\""(?:[^\""]+|\""\"")*\""|[^{0}]*)", delimiter))
            Dim values(0) As String
            For Each match As Match In csvPreservingQuotedStrings.Matches(value)
                If values(0) = Nothing Then
                    values(0) = match.Value.TrimStart(delimiter)
                Else
                    Array.Resize(values, values.Length + 1)
                    values(values.Length - 1) = TrimQuotes(match.Value.TrimStart(delimiter).Replace("""""", """"))

                End If
            Next
            'Dim values() As String = csvPreservingQuotedStrings.Matches(value).Cast(Of Match)().[Select](Function(m) m.Value).ToArray()

            Return values
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Function TrimQuotes(myString As String) As String

        Try

            If myString.EndsWith("""") Then
                myString = myString.Substring(0, myString.Length - 1)
            End If

            If myString.StartsWith("""") Then
                myString = myString.Substring(1)
            End If

            Return myString
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    Private Function IsResourceValid(ByVal oResource As Object, ByVal oResourceType As Type) As Boolean

        Dim bCheck As Boolean = True

        Try

            ' Check against type, and for populated values
            Select Case oResourceType

                Case Type.CSV, Type.Excel
                    If TypeOf oResource Is String Then
                        ' Expected input is a filepath in the form of a string
                        If Not (oResource <> "" And File.Exists(oResource)) Then bCheck = False
                    ElseIf TypeOf oResource Is StreamReader Then
                        bCheck = True
                    Else
                        bCheck = False
                    End If


                Case Type.Xml
                    If Not (TypeOf oResource Is XmlElement Or TypeOf oResource Is XmlNode) And Not (oResource Is Nothing) Then bCheck = False

                Case Type.Undefined
                    bCheck = False

            End Select

            Return bCheck

        Catch ex As Exception
            Return False
        End Try


    End Function

    Private Function IsConversionAvailableConversion() As Boolean
        ' Explicitly determine if we can offer the conversion
        If _
            (nInputType = Type.Excel And nOutputType = Type.Xml) _
          Or (nInputType = Type.CSV And nOutputType = Type.Xml) _
        Then
            Return True
        Else
            Return False
        End If
    End Function

#End Region

#Region "    Public Functions"

#End Region

#Region "    Public Procedures"

    Public Sub Convert()

        ' Convert works by converting to Xml and then converting to the output type, if different from Xml
        Try


            If Me.nStatus = Status.Ready Then
                ' Convert the input type to Xml
                Select Case Me.nInputType

                    Case Type.Excel
                        ConvertExcelToXml()
                        If Not Me.oOutputXml Is Nothing Then
                            Me.nStatus = Status.Succeeded
                        Else
                            Me.nStatus = Status.Failed
                        End If
                    Case Type.CSV
                        ConvertCSVToXml()
                        If Not Me.oOutputXml Is Nothing Then
                            Me.nStatus = Status.Succeeded
                        Else
                            Me.nStatus = Status.Failed
                        End If
                    Case Type.Xml
                        Me.oOutputXml = Me.oInput
                        Me.nStatus = Status.Succeeded
                End Select

                If Me.nStatus = Status.Succeeded Then

                    ' Convert the XML to the output type
                    Select Case Me.nOutputType

                      '  Case Type.CSV
                       ' ConvertXMLToCSV()

                        Case Type.Xml
                            Me.oOutput = Me.oOutputXml

                    End Select
                End If

            End If
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Convert", ex, ""))

        End Try

    End Sub


#End Region

    Public Function GetXML(filename As String) As String
        Try

            Dim ds As DataSet = Me.ReadExcelFile(filename)
            Return ds.GetXml()

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetXML", ex, ""))
        End Try
    End Function

    Private Function ReadExcelFile(ByVal filename As String) As DataSet

        Dim ds As New DataSet()
        Try
            Dim spreadsheetDocument As SpreadsheetDocument = SpreadsheetDocument.Open(filename, False)
            Dim workbookPart As WorkbookPart = spreadsheetDocument.WorkbookPart
            Dim sheetcollection As IEnumerable(Of Sheet) = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild(Of Sheets)().Elements(Of Sheet)()
            Dim myWorksheet As Sheet
            Dim sheetCount As Integer = 0
            For Each myWorksheet In sheetcollection

                Dim dt As DataTable = New DataTable()
                Dim relationshipId As String = myWorksheet.Id.Value
                Dim worksheetPart As WorksheetPart = CType(spreadsheetDocument.WorkbookPart.GetPartById(relationshipId), WorksheetPart)
                Dim sheetData As SheetData = worksheetPart.Worksheet.Elements(Of SheetData)().First()
                Dim rowcollection As IEnumerable(Of Row) = sheetData.Descendants(Of Row)()
                If Not rowcollection.Count() = 0 Then
                    'TS - First row on second worksheet is empty so stepping on one
                    'For Each cell As Cell In rowcollection.ElementAt(0)
                    For Each cell As Cell In rowcollection.ElementAt(sheetCount)
                        dt.Columns.Add(GetValueOfCell(spreadsheetDocument, cell))
                    Next

                    For Each row As Row In rowcollection
                        Dim temprow As DataRow = dt.NewRow()
                        Dim columnIndex As Integer = 0
                        For Each cell As Cell In row.Descendants(Of Cell)()
                            Dim cellColumnIndex As Integer = GetColumnIndex(GetColumnName(cell.CellReference))
                            If columnIndex < cellColumnIndex Then
                                Do
                                    temprow(columnIndex) = String.Empty
                                    columnIndex += 1
                                Loop While columnIndex < cellColumnIndex
                            End If
                            temprow(columnIndex) = GetValueOfCell(spreadsheetDocument, cell)
                            columnIndex += 1
                        Next

                        dt.Rows.Add(temprow)

                    Next
                    ' Here remove header row
                    dt.Rows.RemoveAt(0)
                    ds.Tables.Add(dt)

                End If
                sheetCount = sheetCount + 1
            Next
            'End Using
            spreadsheetDocument.Close()
            spreadsheetDocument = Nothing
            Return ds
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetXML", ex, ""))
        End Try
    End Function

    Private Shared Function GetValueOfCell(ByVal spreadsheetdocument As SpreadsheetDocument, ByVal cell As Cell) As String
        Dim sharedString As SharedStringTablePart = spreadsheetdocument.WorkbookPart.SharedStringTablePart
        If cell.CellValue Is Nothing Then
            Return String.Empty
        End If

        Dim cellValue As String = cell.CellValue.InnerText
        If cell.DataType IsNot Nothing AndAlso cell.DataType.Value = CellValues.SharedString Then
            Return sharedString.SharedStringTable.ChildElements(Integer.Parse(cellValue)).InnerText
        Else
            Return cellValue
        End If
    End Function

    Private Function GetColumnIndex(columnName As String) As Integer
        Dim columnIndex As Integer = 0
        Dim factor As Integer = 1

        ' From right to left
        For position As Integer = columnName.Length - 1 To 0 Step -1
            ' For letters
            If [Char].IsLetter(columnName(position)) Then
                columnIndex += factor * ((AscW(columnName(position)) - AscW("A"c)) + 1) - 1

                factor *= 26
            End If
        Next

        Return columnIndex
    End Function

    Private Function GetColumnName(cellReference As String) As String
        ' Create a regular expression to match the column name of cell
        Dim regex As New Regex("[A-Za-z]+")
        Dim match As Match = regex.Match(cellReference)
        Return match.Value
    End Function


End Class

Public Class ReverseIterator
    Implements IEnumerable

    ' a low-overhead ArrayList to store references
    Dim items As New ArrayList()

    Sub New(ByVal collection As IEnumerable)
        ' load all the items in the ArrayList, but in reverse order
        Dim o As Object
        For Each o In collection
            items.Insert(0, o)
        Next
    End Sub

    Public Function GetEnumerator() As System.Collections.IEnumerator _
        Implements System.Collections.IEnumerable.GetEnumerator
        ' return the enumerator of the inner ArrayList
        Return items.GetEnumerator()
    End Function
End Class

Public Class classFileLineReader
    Implements IDisposable
    'Provides a File Reader that improves upon the .ReadLine method
    'in order to read through single CR and LF and return a full
    'line based on CrLf
    Private m_objInputFile As System.IO.StreamReader
    Private m_objTempFile As System.IO.FileStream
    Private m_objTempReader As System.IO.BinaryReader

    Private m_lngFilePointer As Long = 0

    Sub Dispose()

        'Clean up our resources

        On Error Resume Next

        m_lngFilePointer = 0

        If IsNothing(m_objTempReader) = False Then

            m_objTempReader.Close()

            m_objTempReader = Nothing

        End If

        If IsNothing(m_objTempFile) = False Then

            m_objTempFile.Close()

            m_objTempFile.Dispose()

            m_objTempFile = Nothing

        End If

        If IsNothing(m_objInputFile) = False Then

            m_objInputFile.Close()

            m_objInputFile.Dispose()

            m_objInputFile = Nothing

        End If

    End Sub
    Public Sub New(ByVal pFilePath As String)
        'When the Class is created, we need to know what file we'll be working with

        Try

            m_objInputFile = New System.IO.StreamReader(pFilePath, System.Text.Encoding.ASCII)

            m_objTempFile = System.IO.File.OpenRead(pFilePath)

            m_objTempReader = New System.IO.BinaryReader(m_objTempFile)

        Catch ex As Exception

            Throw New Exception("Error: [" & Err.Number & "] " & Err.Description, ex)

        End Try

    End Sub

    Protected Overrides Sub Finalize()

        'Clean up our resources
        On Error Resume Next
        Me.Dispose()
        MyBase.Finalize()

    End Sub

    Public ReadOnly Property EOF() As Boolean

        'Replaces the End Of File test for a normal stream

        Get
            On Error Resume Next
            Return m_objInputFile.EndOfStream
        End Get

    End Property



    Public Function ReadLine() As String
        'Returns a Text line from the file up to the next CrLf
        'while skipping any single Cr or Lf's (which the normal ReadLine does not do)

        Dim strLine As String = ""
        Dim strNextLine As String = ""
        Dim bytPeekChars(2) As Byte
        Const cCR As Byte = 13
        Const cLf As Byte = 10
        Dim test As String = ""

        Try
            If m_objInputFile.EndOfStream = False Then
                strLine = m_objInputFile.ReadLine 'Read in the Next Line Like Normal
                'Now we use our 2nd Reader to to PEEK at the character
                'that caused the ReadLine to End PLUS the next one to Determine
                'If we are really at the end of line
                m_lngFilePointer += strLine.Length
                Do
                    strNextLine = "[Lf]" 'By Defaulting to Text, it prevents CrCr or LfLf from stopping the loop early
                    m_objTempReader.BaseStream.Seek(m_lngFilePointer, IO.SeekOrigin.Begin)
                    bytPeekChars = m_objTempReader.ReadBytes(2)
                    'See if what Caused the EOL was the End of the File, a Single Cr, or a Single Lf
                    'If (bytPeekChars.Length > 0) AndAlso (bytPeekChars(0) <> cCR Or bytPeekChars(1) <> cLf) Then
                    If (bytPeekChars.Length > 0) AndAlso (bytPeekChars(1) <> cLf) Then
                        strNextLine = m_objInputFile.ReadLine 'Read Lines until we reach the end or get a real CrLf
                        m_lngFilePointer += strNextLine.Length + 1 'We only had a +1 becaue it was a Single Cr or Lf
                        If strNextLine <> "" Then
                            strLine &= " " & strNextLine 'Append it to the Real Line
                        End If
                    Else
                        test = ""
                    End If
                Loop Until strNextLine = "[Lf]"
                m_lngFilePointer += 1 'We Add +1 to our position to account for the Final Lf
            End If
        Catch ex As Exception

            strLine = ""

            Throw New Exception("Error: [" & Err.Number & "] " & Err.Description, ex)

        End Try

        Return strLine

    End Function

    Private Sub IDisposable_Dispose() Implements IDisposable.Dispose
        Dispose()
    End Sub
End Class