''***********************************************************************
'' $Library:     eonic.dbtools
'' $Revision:    3.1  
'' $Date:        2006-03-02
'' $Author:      Trevor Spink (trevor@eonic.co.uk)
'' &Website:     www.eonic.co.uk
'' &Licence:     All Rights Reserved.
'' $Copyright:   Copyright (c) 2002, 2003, 2004, 2005, 2006 Eonic Ltd.
''***********************************************************************

'Option Strict Off
'Option Explicit On

'Imports System.Xml
'Imports System.Data
'Imports System.Data.sqlClient
'Imports System.Web
'Imports System.Web.HttpUtility
'Imports System.Web.Configuration
'Imports System.Collections

'Imports VB = Microsoft.VisualBasic

'Public Class dbTools
'#Region "New Error Handling"
'    Public Event OnError(ByVal cModuleName As String, ByVal cRoutineName As String, ByVal oException As Exception, ByVal cFurtherInfo As String, ByVal bDebug As Boolean)
'#End Region


'    Public moConn As SqlConnection
'    Private moDataAdpt As SqlDataAdapter

'    'this is the start of the .net settings
'    Private mcEwConnStr As String
'    'Private mcDbType As String
'    Private bPooling As Boolean = False

'    Public Property ConnectionPooling() As Boolean
'        Get
'            Return bPooling
'        End Get
'        Set(ByVal value As Boolean)
'            bPooling = value
'            resetConnection(mcEwConnStr)
'        End Set
'    End Property

'    Public mbTimeoutException As Boolean = False


'    Private Const mcModuleName As String = "Eonic.dbTools"


'    Enum DataReaderCommand
'        ExecuteReader = 0
'        ExecuteScalar = 1
'        ExecuteNonQuery = 2
'        ExecuteXmlReader = 3
'    End Enum

'    Public Sub New(ByVal cConnectionString As String)
'        ''PerfMon.Log("dbTools", "open")
'        Dim cProcessInfo As String = "open"
'        Try
'            'mcDbType = goConfig("DatabaseType")
'            'Select Case mcDbType
'            'Case "SQL"


'            'OK open the .net connection
'            'resetConnection("Data Source=" & goConfig("DatabaseServer") & "; " & _
'            '"Initial Catalog=" & goConfig("DatabaseName") & "; " & _
'            'goConfig("DatabaseAuth"))
'            resetConnection(cConnectionString)


'            'Case "Access"
'            'mcEwDataConn = goConfig("DatabaseConn")

'            'End Select

'        Catch ex As Exception
'            returnException(mcModuleName, "open", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Sub

'    Public Sub New()
'        'PerfMon.Log("dbTools", "open")
'        Dim cProcessInfo As String = "open"
'        Try

'            ' do nothing we will set the connection another way

'        Catch ex As Exception
'            returnException(mcModuleName, "open", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Sub

'    Public Sub Close()
'        'PerfMon.Log("dbTools", "close")
'        Dim cProcessInfo As String = "close"
'        Try

'            'Select Case mcDbType

'            'Case "SQL"
'            'close the ado .net connection
'            If Not moConn Is Nothing Then
'                moConn.Close()
'                moConn = Nothing
'            End If

'            'Case "Access"

'            'End Select

'        Catch ex As Exception
'            returnException(mcModuleName, "close", ex, "", cProcessInfo, gbDebug)
'        Finally
'            Me.Finalize()
'        End Try
'    End Sub

'    Public Function createDB(ByVal DatabaseName As String) As Boolean
'        Dim cProcessInfo As String = "createDB"
'        Try
'            Dim oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/web")
'            Dim myConn As SqlConnection = New SqlConnection("Data Source=" & oConfig("DatabaseServer") & "; Initial Catalog=master;" & oConfig("DatabaseAuth"))
'            Dim sSql As String
'            moConn = myConn

'            sSql = "select db_id('" & DatabaseName & "')"
'            If exeProcessSQLScalar(sSql) Is Nothing Then
'                exeProcessSQL("CREATE DATABASE " & DatabaseName)
'            Else
'                moConn = Nothing
'                Return False
'            End If
'            moConn = Nothing
'            Return True

'        Catch ex As Exception
'            returnException(mcModuleName, "createDB", ex, "", cProcessInfo, gbDebug)
'            Return Nothing
'        End Try
'    End Function


'    Public Sub resetConnection(ByVal sDataConn As String)
'        'PerfMon.Log("dbTools", "resetConnection")
'        Dim cProcessInfo As String = "resetConnection"
'        Try

'            If bPooling Then
'                mcEwConnStr &= ";Pooling=true"
'                mcEwConnStr &= ";Connect Timeout=" & 15
'                mcEwConnStr &= ";Max Pool Size=" & 50
'                mcEwConnStr &= ";Min Pool Size=" & 0
'            End If

'            'moConn.Close()
'            mcEwConnStr = sDataConn
'            moConn = New SqlConnection(mcEwConnStr)
'            'PerfMon.Log("dbTools", "open-connectionCreated")
'            moConn.Open()
'            'PerfMon.Log("dbTools", "open-connectionConnected")
'        Catch ex As Exception
'            returnException(mcModuleName, "resetConnection", ex, "", cProcessInfo, gbDebug)
'        End Try
'    End Sub



'    Public Function getIdInsertSQL(ByVal cSql As String) As String
'        'PerfMon.Log("dbTools", "getIdInsertSQL")
'        Dim nInsertId As Integer
'        Dim dr As SqlDataReader
'        Dim cProcessInfo As String = "Running: " & cSql
'        Try

'            Dim oCmd As New SqlCommand(cSql & ";select @@identity", moConn)
'            If moConn Is Nothing Then
'                moConn = New SqlConnection(mcEwConnStr)
'                moConn.Open()
'            Else
'                If moConn.State = ConnectionState.Closed Then moConn.Open()
'            End If
'            cProcessInfo = "Running SQL: " & cSql
'            dr = oCmd.ExecuteReader
'            While dr.Read
'                nInsertId = dr(0)
'            End While
'            dr.Close()
'            dr = Nothing
'            oCmd = Nothing
'            moConn.Close()
'            Return nInsertId

'        Catch ex As Exception
'            returnException(mcModuleName, "getIdInsertSQL", ex, "", cProcessInfo, gbDebug)
'            Return Nothing
'        End Try
'    End Function

'    Public Function exeProcessSQL(ByVal vstrSQL As String) As Integer
'        'PerfMon.Log("dbTools", "exeProcessSQL")
'        Dim nUpdateCount As Integer
'        Dim cProcessInfo As String = "Running: " & vstrSQL
'        Try

'            'Dot Net version will only work with SQL Server at the moment, we'll need to cater for other connectors here.

'            Dim oCmd As New SqlCommand(vstrSQL, moConn)

'            If moConn.State = ConnectionState.Closed Then moConn.Open()
'            cProcessInfo = "Running SQL: " & vstrSQL
'            nUpdateCount = oCmd.ExecuteNonQuery

'            oCmd = Nothing

'            exeProcessSQL = nUpdateCount

'        Catch ex As Exception
'            returnException(mcModuleName, "exeProcessSQL", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Function
'    Public Function exeProcessSQLScalar(ByVal vstrSQL As String) As String
'        'PerfMon.Log("dbTools", "exeProcessSQLScalar")
'        Dim nRes As Object
'        Dim cProcessInfo As String = "Running: " & vstrSQL
'        Try

'            'Dot Net version will only work with SQL Server at the moment, we'll need to cater for other connectors here.

'            Dim oCmd As New SqlCommand(vstrSQL, moConn)
'            If moConn.State = ConnectionState.Closed Then moConn.Open()
'            cProcessInfo = "Running SQL: " & vstrSQL
'            nRes = oCmd.ExecuteScalar

'            If moConn.State <> ConnectionState.Closed Then
'                moConn.Close()
'            End If
'            oCmd = Nothing
'            If IsDBNull(nRes) Then
'                Return Nothing
'            Else
'                Return nRes
'            End If

'        Catch ex As Exception
'            returnException(mcModuleName, "exeProcessSQLScalar", ex, "", cProcessInfo, gbDebug)
'            Return Nothing
'        End Try

'    End Function
'    Public Function exeProcessSQLorIgnore(ByVal vstrSQL As String) As Integer
'        'PerfMon.Log("dbTools", "exeProcessSQLorIgnore")
'        Dim nUpdateCount As Integer
'        Dim cProcessInfo As String = "Running: " & vstrSQL
'        Try

'            'Dot Net version will only work with SQL Server at the moment, we'll need to cater for other connectors here.

'            Dim oCmd As New SqlCommand(vstrSQL, moConn)

'            cProcessInfo = "Running SQL: " & vstrSQL
'            nUpdateCount = oCmd.ExecuteNonQuery

'            oCmd = Nothing
'            exeProcessSQLorIgnore = nUpdateCount

'        Catch ex As Exception
'            ' Dont Handle errors!
'            exeProcessSQLorIgnore = -1

'        End Try

'    End Function


'    Public Function saveInstance(ByRef instanceElmt As XmlElement, ByVal targetTable As String, ByVal keyField As String, Optional ByVal whereStmt As String = "") As Integer
'        'PerfMon.Log("dbTools", "saveInstance")
'        'Generic function to save xml to a database, picking only the relevent fields out of the XML

'        Dim keyValue As String = Nothing
'        Dim sSql As String
'        Dim cProcessInfo As String = "AddRStoXMLnode"
'        Dim nUpdateCount As Long
'        Dim oRow As DataRow
'        Dim column As DataColumn

'        Try
'            If moConn.State = ConnectionState.Closed Then moConn.Open()

'            'Identify the keyValue and build the initial SQL Statement.
'            If whereStmt = "" Then
'                If Not instanceElmt.SelectSingleNode("descendant-or-self::" & keyField) Is Nothing Then
'                    keyValue = instanceElmt.SelectSingleNode("descendant-or-self::" & keyField).InnerText
'                    If keyValue = "" Then keyValue = "-1"
'                Else
'                    keyValue = "-1"
'                End If
'                sSql = "select * from " & targetTable & " where " & keyField & " = " & keyValue
'            Else
'                sSql = "select * from " & targetTable & " where " & whereStmt
'            End If

'            cProcessInfo = "error running SQL: " & sSql

'            Dim oDataAdpt As New SqlDataAdapter(sSql, moConn)
'            'autogenerate commands
'            Dim cmdBuilder As SqlCommandBuilder = New SqlCommandBuilder(oDataAdpt)
'            Dim oDs As New DataSet
'            oDataAdpt.Fill(oDs, targetTable)

'            If oDs.Tables(targetTable).Rows.Count > 0 Then ' CASE FOR UPDATE
'                oRow = oDs.Tables(targetTable).Rows(0)
'                oRow.BeginEdit()
'                For Each column In oDs.Tables(targetTable).Columns
'                    If Not instanceElmt.SelectSingleNode("*/" & column.ToString) Is Nothing Then
'                        cProcessInfo = column.ToString & " - " & instanceElmt.SelectSingleNode("*/" & column.ToString).InnerXml
'                        oRow(column) = convertDtXMLtoSQL(column.DataType, instanceElmt.SelectSingleNode("*/" & column.ToString).InnerXml)
'                    End If
'                Next
'                oRow.EndEdit()

'                'run the update
'                nUpdateCount = oDataAdpt.Update(oDs, targetTable)

'            Else ' CASE FOR INSERT
'                oRow = oDs.Tables(targetTable).NewRow
'                For Each column In oDs.Tables(targetTable).Columns

'                    If Not (instanceElmt.SelectSingleNode("*/" & column.ToString) Is Nothing) Then
'                        'don't want to set the value on the key feild on insert
'                        If Not (column.ToString = keyField) Then
'                            cProcessInfo = column.ToString & " - " & instanceElmt.SelectSingleNode("*/" & column.ToString).InnerXml
'                            oRow(column) = convertDtXMLtoSQL(column.DataType, instanceElmt.SelectSingleNode("*/" & column.ToString).InnerXml)
'                        End If
'                    End If
'                Next
'                oDs.Tables(targetTable).Rows.Add(oRow)

'                'Run the insert and get back the new id
'                Dim ExecuteQuery As String = "SELECT @@IDENTITY"
'                Dim getid As New SqlCommand(ExecuteQuery, moConn)
'                nUpdateCount = oDataAdpt.Update(oDs, targetTable)
'                keyValue = getid.ExecuteScalar()
'            End If

'            If nUpdateCount = 0 Then
'                Err.Raise(1000, mcModuleName, "No Update")
'            End If

'            oDs.Dispose()
'            oDs = Nothing

'            oDataAdpt.Dispose()
'            oDataAdpt = Nothing

'            Return keyValue

'        Catch ex As Exception
'            returnException(mcModuleName, "saveInstance", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Function

'    Public Function convertDtXMLtoSQL(ByVal datatype As System.Type, ByVal value As Object) As Object
'        'PerfMon.Log("dbTools", "convertDtXMLtoSQL")
'        Dim cProcessInfo As String = "Converting Datatype:  " & datatype.Name
'        Try
'            Select Case datatype.Name
'                Case "Boolean"
'                    If value = "true" Then
'                        Return True
'                    Else
'                        Return False
'                    End If
'                Case "DateTime"
'                    If IsDate(value) And Not value.ToString.StartsWith("0001-01-01") Then
'                        Return CDate(value)
'                    Else
'                        Return System.DBNull.Value
'                    End If
'                Case "Double", "Int32", "Int16"
'                    If IsNumeric(value) Then
'                        Return CDbl(value)
'                    Else
'                        Return System.DBNull.Value
'                    End If
'                Case Else
'                    Return value
'            End Select
'        Catch ex As Exception
'            returnException(mcModuleName, "convertDtXMLtoSQL", ex, "", cProcessInfo, gbDebug)
'            Return Nothing
'        End Try
'    End Function

'    Public Function convertDtSQLtoXML(ByVal datatype As System.Type, ByVal value As Object) As String
'        'PerfMon.Log("dbTools", "convertDtSQLtoXML")
'        Dim cProcessInfo As String = "Converting Datatype:  " & datatype.Name
'        Try
'            If IsDBNull(value) Then
'                Return ""
'            Else
'                Select Case datatype.Name
'                    Case "Boolean"
'                        If value = True Then
'                            Return "true"
'                        Else
'                            Return "false"
'                        End If
'                    Case "DateTime"
'                        If IsDate(value) And Not IsDBNull(value) Then
'                            Return Eonic.Tools.Xml.XmlDate(value)
'                        Else
'                            Return ""
'                        End If
'                    Case "Double", "Int32", "Int16"
'                        If IsNumeric(value) Then
'                            Return CStr(value)
'                        Else
'                            Return ""
'                        End If
'                    Case "DBNull"
'                        Return ""
'                    Case Else
'                        Return CStr(value)
'                End Select
'            End If

'        Catch ex As Exception
'            returnException(mcModuleName, "convertDtSQLtoXML", ex, "", cProcessInfo, gbDebug)
'            Return Nothing
'        End Try
'    End Function

'    Public Function getDataSet(ByVal sSql As String, ByVal tableName As String, Optional ByVal datasetName As String = "", Optional ByVal bHandleTimeouts As Boolean = False) As DataSet

'        ' PerfMon.Log("dtTools", "getDataset-s-" & sSql)

'        Dim cProcessInfo As String = "Running SQL:  " & sSql
'        Dim oDs As DataSet
'        Try

'            Dim oDataAdpt As New SqlDataAdapter(sSql, moConn)
'            If moConn.State = ConnectionState.Closed Then moConn.Open()

'            oDs = New DataSet
'            If datasetName <> "" Then
'                oDs.DataSetName = datasetName
'            End If
'            oDataAdpt.Fill(oDs, tableName)
'            getDataSet = oDs

'            '   PerfMon.Log("dtTools", "getDataset-e-" & sSql)


'        Catch ex As System.Data.SqlClient.SqlException
'            If ex.Message.StartsWith("Timeout expired.") And bHandleTimeouts Then
'                ' Deal with timeouts, return emtpy dataset
'                mbTimeoutException = True
'                Return New DataSet
'            Else
'                returnException(mcModuleName, "getDataSet", ex, "", cProcessInfo, gbDebug)
'                Return New DataSet
'            End If
'        Catch ex As Exception
'            returnException(mcModuleName, "getDataSet", ex, "", cProcessInfo, gbDebug)
'            Return New DataSet
'        End Try

'    End Function


'    Public Function getDatasetAddRows(ByVal sSQL As Array, ByVal cTableName As String, Optional ByVal cDatasetName As String = "") As DataSet
'        'PerfMon.Log("dbTools", "getDatasetAddRows")
'        'Creates a Dataset and a datatable
'        'with specified names
'        'runs multiple sql fills to add rows
'        'to the table
'        If sSQL Is Nothing Then Return Nothing
'        Dim cProcessInfo As String = ""
'        Try
'            Dim cSQL As String
'            Dim nI As Integer
'            Dim oDs As New DataSet


'            Dim oDA As SqlDataAdapter

'            For nI = 0 To UBound(sSQL)
'                cSQL = sSQL(nI)
'                cProcessInfo = "Running SQL:  " & cSQL
'                oDA = New SqlDataAdapter(cSQL, moConn)
'                If Not cDatasetName = "" Then oDs.DataSetName = cDatasetName
'                oDA.Fill(oDs, cTableName)
'            Next

'            Return oDs

'        Catch ex As Exception
'            returnException(mcModuleName, "GetDatasetAddRows", ex, "", cProcessInfo, gbDebug)
'            Return Nothing
'        End Try
'    End Function


'    Public Function getDataSetForUpdate(ByVal sSql As String, ByVal tableName As String, Optional ByVal datasetName As String = "") As DataSet
'        'PerfMon.Log("dbTools", "getDataSetForUpdate")
'        Dim oDs As DataSet
'        Dim cProcessInfo As String = "Running SQL:  " & sSql
'        Try

'            moDataAdpt = New SqlDataAdapter
'            If datasetName = "" Then
'                oDs = New DataSet
'            Else
'                oDs = New DataSet(datasetName)
'            End If

'            If moConn.State = ConnectionState.Closed Then moConn.Open()

'            Dim oSqlCmd As SqlCommand = New SqlCommand(sSql, moConn)
'            moDataAdpt.SelectCommand = oSqlCmd
'            Dim cb As SqlCommandBuilder = New SqlCommandBuilder(moDataAdpt)
'            moDataAdpt.TableMappings.Add(tableName, tableName)

'            moDataAdpt.Fill(oDs, tableName)
'            moDataAdpt.Update(oDs, tableName)
'            getDataSetForUpdate = oDs

'        Catch ex As Exception
'            returnException(mcModuleName, "getDataSetForUpdate", ex, "", cProcessInfo, gbDebug)
'            Return Nothing
'        End Try

'    End Function

'    Public Sub addTable(ByRef oDs As DataSet, ByVal sSql As String, ByVal tableName As String)
'        'PerfMon.Log("dbTools", "addTable")
'        Dim cProcessInfo As String = "Running SQL:  " & sSql
'        Try

'            If moConn.State = ConnectionState.Closed Then moConn.Open()

'            Dim oSqlCmd As SqlCommand = New SqlCommand(sSql, moConn)
'            moDataAdpt = New SqlDataAdapter
'            moDataAdpt.SelectCommand = oSqlCmd

'            moDataAdpt.Fill(oDs, tableName)

'        Catch ex As Exception
'            returnException(mcModuleName, "getDataSet", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Sub


'    Public Sub addTableToDataSet(ByRef oDs As DataSet, ByVal sSql As String, ByVal tableName As String)
'        'PerfMon.Log("dbTools", "addTableToDataSet")
'        Dim cProcessInfo As String = "getDataSet"
'        Try

'            Dim oDdpt As New SqlDataAdapter(sSql, moConn)
'            oDdpt.Fill(oDs, tableName)

'        Catch ex As Exception
'            returnException(mcModuleName, "getDataSet", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Sub

'    Public Function updateDataset(ByRef oDs As DataSet, ByVal sTableName As String, Optional ByVal bReUse As Boolean = False) As Boolean
'        'PerfMon.Log("dbTools", "updateDataset")
'        Dim cProcessInfo As String = "returnDataSet"

'        Try
'            Dim oxx As New SqlClient.SqlCommandBuilder(moDataAdpt)
'            moDataAdpt.DeleteCommand = oxx.GetDeleteCommand()
'            moDataAdpt.InsertCommand = oxx.GetInsertCommand()
'            moDataAdpt.UpdateCommand = oxx.GetUpdateCommand()


'            moDataAdpt.Update(oDs, sTableName)

'            If Not bReUse Then
'                'lets tidy up
'                moDataAdpt = Nothing
'                oDs.Clear()
'                oDs = Nothing
'            End If


'        Catch ex As Exception
'            Dim oXml As XmlDocument = New XmlDataDocument(oDs)
'            cProcessInfo = oXml.OuterXml
'            'Return False
'            returnException(mcModuleName, "getDataSet", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Function

'    Public Sub closeDataset(ByRef oDs As DataSet)
'        'PerfMon.Log("dbTools", "closeDataset")
'        Dim cProcessInfo As String = "returnDataSet"

'        Try

'            moDataAdpt = Nothing
'            oDs.Clear()
'            oDs = Nothing

'        Catch ex As Exception
'            returnException(mcModuleName, "closeDataset", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Sub



'    Public Sub setDataSet(ByVal oDs As DataSet, ByVal tableName As String)
'        'PerfMon.Log("dbTools", "setDataSet")
'        Dim cProcessInfo As String = "setDataSet"
'        Try
'            Dim oDataAdpt As New SqlDataAdapter("select * from " & tableName, moConn)
'            oDataAdpt.Update(oDs)

'        Catch ex As Exception
'            returnException(mcModuleName, "setDataSet", ex, "", cProcessInfo, gbDebug)

'        End Try

'    End Sub

'    Public Sub returnNullsEmpty(ByRef ods As DataSet)
'        'PerfMon.Log("dbTools", "returnNullsEmpty")
'        Dim oTable As DataTable
'        Dim oRow As DataRow
'        Dim oColumn As DataColumn

'        Dim cProcessInfo As String = "returnNullsEmpty"
'        Try
'            For Each oTable In ods.Tables
'                For Each oRow In oTable.Rows
'                    For Each oColumn In oTable.Columns
'                        If IsDBNull(oRow.Item(oColumn.ColumnName)) Then
'                            cProcessInfo = "Error in Feild:" & oColumn.ColumnName & " DataType:" & oColumn.DataType.ToString
'                            Select Case oColumn.DataType.ToString
'                                Case "System.DateTime"
'                                    oRow.Item(oColumn.ColumnName) = CDate(#12:00:00 AM#)
'                                Case "System.Integer", "System.Int32", "System.Double", "System.Decimal"
'                                    oRow.Item(oColumn.ColumnName) = 0
'                                Case "System.Boolean"
'                                    oRow.Item(oColumn.ColumnName) = False
'                                Case Else
'                                    oRow.Item(oColumn.ColumnName) = ""
'                            End Select
'                        End If
'                    Next
'                Next
'            Next
'        Catch ex As Exception
'            returnException(mcModuleName, "returnNullsEmpty", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Sub

'    Public Sub returnEmptyNulls(ByRef ods As DataSet)
'        'PerfMon.Log("dbTools", "returnEmptyNulls")
'        Dim oTable As DataTable
'        Dim oRow As DataRow
'        Dim oColumn As DataColumn

'        Dim cProcessInfo As String = "getDataReader"
'        Try
'            For Each oTable In ods.Tables
'                For Each oRow In oTable.Rows
'                    For Each oColumn In oTable.Columns
'                        'If IsDBNull(oRow.Item(oColumn.ColumnName)) Then
'                        Select Case oColumn.DataType.ToString
'                            Case "System.DateTime"
'                                If Not IsDBNull(oRow.Item(oColumn.ColumnName)) Then
'                                    If oRow.Item(oColumn.ColumnName) = CDate(#12:00:00 AM#) Then
'                                        oRow.Item(oColumn.ColumnName) = DBNull.Value
'                                    End If
'                                End If
'                                'Case "System.Integer", "System.Double"
'                                '    If oRow.Item(oColumn.ColumnName) = 0 Then
'                                '        oRow.Item(oColumn.ColumnName) = Nothing
'                                '    End If
'                                'Case Else
'                                '    If oRow.Item(oColumn.ColumnName) = "" Then
'                                '        oRow.Item(oColumn.ColumnName) = Nothing
'                                '    End If
'                        End Select
'                        'End If
'                    Next
'                Next
'            Next
'        Catch ex As Exception
'            returnException(mcModuleName, "setDataSet", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Sub


'    Public Function getDataReader(ByVal sSql As String, Optional ByVal nCommandType As CommandType = CommandType.Text, Optional ByVal oParameters As Hashtable = Nothing) As SqlDataReader
'        'PerfMon.Log("dbTools", "getDataReader")
'        Dim cProcessInfo As String = "Running SQL: " & sSql
'        Dim oReader As SqlDataReader
'        Try
'            Dim oConn As New SqlConnection(mcEwConnStr)
'            Dim oCmd As New SqlCommand(sSql, oConn)

'            ' Set the command type
'            oCmd.CommandType = nCommandType

'            ' Set the Paremeters if any
'            If Not oParameters Is Nothing Then
'                Dim oEntry As DictionaryEntry
'                For Each oEntry In oParameters
'                    oCmd.Parameters.Add(oEntry.Key, oEntry.Value)
'                Next
'            End If

'            ' Open the connection
'            If oConn.State = ConnectionState.Closed Then oConn.Open()

'            oReader = oCmd.ExecuteReader(CommandBehavior.CloseConnection)

'            oCmd = Nothing
'            Return oReader

'        Catch ex As Exception
'            returnException(mcModuleName, "getDataReader", ex, "", cProcessInfo, gbDebug)
'            Return Nothing
'        End Try

'    End Function

'    Public Function getDataValue(ByVal sSql As String, Optional ByVal nCommandType As CommandType = CommandType.Text, Optional ByVal oParameters As Hashtable = Nothing, Optional ByVal oNullReturnValue As Object = Nothing) As Object
'        'PerfMon.Log("dbTools", "getDataValue")
'        Dim cProcessInfo As String = "Running SQL: " & sSql
'        Dim oScalarValue As Object
'        Try
'            Dim oCmd As New SqlCommand(sSql, moConn)

'            ' Set the command type
'            oCmd.CommandType = nCommandType

'            ' Set the Paremeters if any
'            If Not oParameters Is Nothing Then
'                Dim oEntry As DictionaryEntry
'                For Each oEntry In oParameters
'                    oCmd.Parameters.Add(oEntry.Key, oEntry.Value)
'                Next
'            End If

'            ' Open the connection
'            If moConn.State = ConnectionState.Closed Then
'                moConn.Open()
'            End If

'            oScalarValue = oCmd.ExecuteScalar()
'            oCmd = Nothing

'            ' If the return value is NULL and a default return value for NULLs has been specififed, then return this instead
'            If (Not (IsNothing(oNullReturnValue))) And (IsNothing(oScalarValue) Or IsDBNull(oScalarValue)) Then
'                oScalarValue = oNullReturnValue
'            End If

'            Return oScalarValue

'        Catch ex As Exception
'            returnException(mcModuleName, "getDataValue", ex, "", cProcessInfo, gbDebug)
'            Return Nothing

'        End Try

'    End Function


'    Public Function getHashTable(ByVal sSql As String, ByVal sNameField As String, ByRef sValueField As String) As Hashtable
'        'PerfMon.Log("dbTools", "getHashTable")
'        Dim oDataAdpt As New SqlDataAdapter(sSql, moConn)
'        Dim oDs As New DataSet
'        Dim oDr As DataRow
'        Dim oHash As Hashtable = New Hashtable
'        Dim cProcessInfo As String = "getDataSet"
'        Try

'            oDataAdpt.Fill(oDs, "HashPairs")
'            For Each oDr In oDs.Tables("HashPairs").Rows
'                oHash.Add(CStr(oDr(sNameField)), CStr(oDr(sValueField)))
'            Next

'            moConn.Close()
'            getHashTable = oHash

'        Catch ex As Exception
'            returnException(mcModuleName, "getDataSet", ex, "", cProcessInfo, gbDebug)
'            Return Nothing
'        End Try

'    End Function

'    Public Function exeProcessSQLfromFile(ByVal sFilePath As String) As Integer
'        'PerfMon.Log("dbTools", "exeProcessSQLfromFile")
'        Dim nUpdateCount As Integer
'        Dim vstrSQL As String
'        Dim oFr As System.IO.StreamReader

'        Dim cProcessInfo As String = sFilePath
'        Try

'            'Dot Net version will only work with SQL Server at the moment, we'll need to cater for other connectors here.

'            oFr = System.IO.File.OpenText(sFilePath)
'            vstrSQL = oFr.ReadToEnd()
'            oFr.Close()
'            Dim oCmd As New SqlCommand(vstrSQL, moConn)
'            If moConn.State = ConnectionState.Closed Then moConn.Open()
'            cProcessInfo = "Running SQL ('" & sFilePath & "'): " & vstrSQL
'            nUpdateCount = oCmd.ExecuteNonQuery

'            oCmd = Nothing

'            Return nUpdateCount

'        Catch ex As Exception
'            returnException(mcModuleName, "exeProcessSQLfromFile", ex, "", cProcessInfo, gbDebug)
'        End Try
'    End Function

'    Public Function DBN2Str(ByVal Frm As Object, Optional ByVal useMarks As Boolean = False, Optional ByVal NullText As Boolean = False) As String
'        'PerfMon.Log("dbTools", "DBN2Str")
'        Dim strNull As String
'        strNull = ""

'        If Frm Is Nothing Then
'            strNull = "Null"
'            GoTo ReturnMe
'        End If
'        If NullText Then strNull = "Null"
'        If useMarks Then strNull = "''"
'ReturnMe:
'        If IsDBNull(Frm) Then Return strNull Else Return IIf(useMarks, "'" & Replace(CStr(Frm), "'", "''") & "'", CStr(Frm))
'    End Function
'    Public Function DBN2int(ByVal Frm As Object, Optional ByVal NullText As Boolean = False) As Object
'        'PerfMon.Log("dbTools", "DBN2int")
'        If IsDBNull(Frm) Then Return IIf(NullText, "Null", 0) Else Return CInt(Frm)
'    End Function
'    Public Function DBN2dte(ByVal Frm As Object, Optional ByVal NullText As Boolean = False) As Object
'        'PerfMon.Log("dbTools", "DBN2dte")
'        If IsDBNull(Frm) Then Return IIf(NullText, "Null", #12:00:00 AM#) Else Return CDate(Frm)
'    End Function





'    Public Function doesTableExist(ByRef sTableName As String) As Boolean

'        Dim sSqlStr As String
'        Dim oDr As SqlDataReader
'        Dim cProcessInfo As String = "doesTableExist"
'        Try
'            sSqlStr = "select * from dbo.sysobjects where name = '" & sTableName & "'"
'            oDr = getDataReader(sSqlStr)
'            If Not oDr.HasRows Then
'                doesTableExist = False
'            Else
'                doesTableExist = True
'            End If

'            oDr = Nothing
'        Catch ex As Exception
'            returnException(mcModuleName, "doesTableExist", ex, "", cProcessInfo, gbDebug)
'        End Try

'    End Function
'    Public Function TableExists(ByVal cTableName As String) As Boolean
'        Dim bRes As Boolean = False
'        Dim cProcessInfo As String = "TableExists: " & cTableName
'        Try
'            If cTableName = "" Then Return False
'            Dim sSqlStr As String
'            sSqlStr = "if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[" & cTableName & "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)"
'            sSqlStr = sSqlStr & "select object_id(N'[dbo].[" & cTableName & "]') as ID"
'            Dim cRes As String = exeProcessSQLScalar(sSqlStr)
'            If IsNumeric(cRes) Then
'                If CInt(cRes) > 0 Then bRes = True
'            End If
'            Return bRes
'        Catch ex As Exception
'            returnException(mcModuleName, "doesTableExist", ex, "", cProcessInfo, gbDebug)
'            Return bRes
'        End Try
'    End Function

'End Class

