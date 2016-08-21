Imports System.Xml
Imports System.IO
Imports System.Data
Imports System.Data.Oledb

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

            oExcelConn = New OleDbConnection( _
                       "Provider=Microsoft.ACE.OLEDB.12.0;" & _
                       "Data Source=" & Me.oInput & ";" & _
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

#End Region

#Region "    Private Functions"

    Private Function IsResourceValid(ByVal oResource As Object, ByVal oResourceType As Type) As Boolean

        Dim bCheck As Boolean = True

        Try

            ' Check against type, and for populated values
            Select Case oResourceType

                Case Type.CSV, Type.Excel
                    If TypeOf oResource Is String Then
                        ' Expected input is a filepath in the form of a string
                        If Not (oResource <> "" And File.Exists(oResource)) Then bCheck = False
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

        If Me.nStatus = Status.Ready Then
            ' Convert the input type to Xml
            Select Case Me.nInputType

                Case Type.Excel
                    ConvertExcelToXml()

                Case Type.Xml
                    Me.oOutputXml = Me.oInput
                    Me.nStatus = Status.Succeeded
            End Select

            If Me.nStatus = Status.Succeeded Then

                ' Convert the XML to the output type
                Select Case Me.nOutputType

                    ' e.g. Case Type.CSV
                    '        ConvertXMLToCSV()

                    Case Type.Xml
                        Me.oOutput = Me.oOutputXml

                End Select
            End If

        End If

    End Sub


#End Region

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
