Imports System.Xml
Imports System.Data.SqlClient
Imports System.Collections

Public Class Database
    Implements IDisposable
#Region "    Declarations"

    Private oDriver As driverType
    Private cServer As String = ""
    Private cDatabase As String = ""
    Private cUserName As String = ""
    Private cPassword As String = ""
    Private cFtpUserName As String = ""
    Private cFtpPassword As String = ""

    Private nConnectTimeout As Integer = 15
    Private nMaxPoolSize As Integer = 100
    Private nMinPoolSize As Integer = 0
    Private bPooling As Boolean = False
    Public bAsync As Boolean = False

    Public ErrorMsg As String = ""

    Private bTimeoutException As Boolean = False

    Public WithEvents oConn As SqlClient.SqlConnection

    Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) ', ByVal cModuleName As String, ByVal cRoutineName As String, ByVal oException As Exception, ByVal cFurtherInfo As String)
    Public Event Connected(ByVal sender As Object, ByVal e As EventArgs)
    Public Event Disconnected(ByVal sender As Object, ByVal e As EventArgs)
    Public Event ConnectionStringChanged(ByVal sender As Object, ByVal e As EventArgs)
    Private Const mcModuleName As String = "Eonic.Tools.Database"

    Public Enum objectTypes
        Table
        View
        StoredProcedure
        UserFunction
        Database
    End Enum

    Public Enum driverType
        SQLServer
        SQLServerNativeClient10ODBC
    End Enum

#End Region

#Region "    Properties"

    Public Property Driver() As driverType
        Get
            Return oDriver
        End Get
        Set(ByVal value As driverType)
            oDriver = value
        End Set
    End Property

    Public Property DatabaseServer() As String
        Get
            Return cServer
        End Get
        Set(ByVal value As String)
            cServer = value
            ResetConnection()
        End Set
    End Property

    Public Property DatabaseName() As String
        Get
            Return cDatabase
        End Get
        Set(ByVal value As String)
            cDatabase = value
            ResetConnection()
        End Set
    End Property

    Public Property DatabaseUser() As String
        Get
            Return cUserName
        End Get
        Set(ByVal value As String)
            cUserName = value
            ResetConnection()
        End Set
    End Property

    Public Property DatabasePassword() As String
        Get
            Return cPassword
        End Get
        Set(ByVal value As String)
            cPassword = value
            ResetConnection()
        End Set
    End Property


    Public Property FTPUser() As String
        Get
            Return cFtpUserName
        End Get
        Set(ByVal value As String)
            cFtpUserName = value
        End Set
    End Property

    Public Property FtpPassword() As String
        Get
            Return cFtpPassword
        End Get
        Set(ByVal value As String)
            cFtpPassword = value
        End Set
    End Property

    Public ReadOnly Property DatabaseConnectionString() As String
        Get
            Dim cReturn As String = ""

            'Select Case oDriver

            '    Case driverType.SQLServerNativeClient10ODBC
            '        cReturn = "Provider=SQLNCLI10; Server=" & DatabaseServer
            '        cReturn &= "; Database=" & DatabaseName
            '        cReturn &= "; Uid=" & DatabaseUser
            '        cReturn &= "; Pwd=" & DatabasePassword
            '        cReturn &= ";"
            '    Case Else
            cReturn = "Data Source=" & DatabaseServer
            cReturn &= ";Initial Catalog=" & DatabaseName
            cReturn &= ";User ID=" & DatabaseUser
            cReturn &= ";password=" & DatabasePassword
            cReturn &= ";Persist Security Info=True"
            If bPooling Then
                cReturn &= ";Pooling=true"
                cReturn &= ";Connect Timeout=" & nConnectTimeout
                cReturn &= ";Max Pool Size=" & nMaxPoolSize
                cReturn &= ";Min Pool Size=" & nMinPoolSize
            End If
            If bAsync Then
                cReturn &= ";Asynchronous Processing=true"
            End If
            'End Select

            Return cReturn

        End Get
    End Property

    Public ReadOnly Property ConnectionValid() As Boolean
        Get
            Try
                oConn.Open()
                oConn.Close()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Get
    End Property

    Public ReadOnly Property CredentialsValid() As Boolean
        Get
            Try
                Dim convalid As Boolean
                oConn.Open()
                convalid = Me.checkDBObjectExists(DatabaseName, objectTypes.Database)
                oConn.Close()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Get
    End Property

    Public Property ConnectionPooling() As Boolean
        Get
            Return bPooling
        End Get
        Set(ByVal value As Boolean)
            bPooling = value
            ResetConnection()
        End Set
    End Property

    Public Property ConnectTimeout() As Integer
        Get
            Return nConnectTimeout
        End Get
        Set(ByVal value As Integer)
            nConnectTimeout = value
            ResetConnection()
        End Set
    End Property

    Public Property MaxPoolSize() As Integer
        Get
            Return nMaxPoolSize
        End Get
        Set(ByVal value As Integer)
            nMaxPoolSize = value
            ResetConnection()
        End Set
    End Property

    Public Property MinPoolSize() As Integer
        Get
            Return nMinPoolSize
        End Get
        Set(ByVal value As Integer)
            nMinPoolSize = value
            ResetConnection()
        End Set
    End Property

    Public Property TimeOutException() As Boolean
        Get
            Return bTimeoutException
        End Get
        Set(ByVal value As Boolean)
            bTimeoutException = value
        End Set
    End Property

#End Region

#Region "    Private Procedures"

    Private Sub _ConnectionState(ByVal sender As Object, ByVal e As System.Data.StateChangeEventArgs) Handles oConn.StateChange
        Select Case oConn.State
            Case ConnectionState.Closed
                RaiseEvent Disconnected(Me, EventArgs.Empty)
            Case ConnectionState.Open
                RaiseEvent Connected(Me, EventArgs.Empty)
        End Select
    End Sub

    Private Sub ResetConnection()
        Try
            CloseConnection()
            ' This was calling an error and am not sure if it is needed. so have removed whilst evalutating
            ' CloseConnectionPool()
            oConn = New SqlClient.SqlConnection(DatabaseConnectionString)
            RaiseEvent ConnectionStringChanged(Me, EventArgs.Empty)
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "ResetConnection", ex, "Setting Connection Info", Nothing))
        End Try
    End Sub




#End Region

#Region "    Public Functions"

    Public Function CreateDatabase(ByVal databasename As String) As Boolean
        Dim cProcessInfo As String = "createDB"
        Dim bReturn As Boolean = False
        Try
            Me.DatabaseName = "master"
            'Dim myConn As SqlConnection = New SqlConnection("Data Source=" & DatabaseServer & "; Initial Catalog=master; User ID=" & DatabaseUser & ";password=" & DatabasePassword & ";Persist Security Info=True")
            Dim sSql As String

            sSql = "select db_id('" & databasename & "')"
            If ExeProcessSqlScalar(sSql) Is Nothing Then
                ExeProcessSql("CREATE DATABASE " & databasename)
            Else
                bReturn = False
            End If

            bReturn = True

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "createDB", ex, cProcessInfo))
            Return Nothing
        Finally
            CloseConnection()
        End Try
        Return bReturn
    End Function

    Public Function BackupDatabase(ByVal databasename As String, ByVal filepath As String) As Boolean
        Dim cProcessInfo As String = "createDB"
        Dim bReturn As Boolean = False
        Dim backupName As String = Nothing
        Dim zipName As String = Nothing
        Try

            'FIRST RUN THIS ON THE SQL SERVER

            '            -- To allow advanced options to be changed.
            '            EXEC(sp_configure) 'show advanced options', 1
            '            GO()
            '-- To update the currently configured value for advanced options.
            '            RECONFIGURE()
            '            GO()
            '-- To enable the feature.
            '            EXEC(sp_configure) 'xp_cmdshell', 1
            '            GO()
            '-- To update the currently configured value for this feature.
            '            RECONFIGURE()
            '            GO()

            Dim tempPath As String = "c:\temp"
            If backupName Is Nothing Then
                backupName = databasename & ".bak"
                zipName = databasename & ".zip"
            End If
            Dim bakFile As String = tempPath + backupName
            Dim zipFile As String = tempPath + zipName

            ' Based on code found here...http://www.codeproject.com/Articles/33963/Transferring-backup-files-from-a-remote-SQL-Server

            ' nice filename on local side, so we know when backup was done
            Dim fileName As String = databasename + DateTime.Now.Year.ToString() + "-" & DateTime.Now.Month.ToString() & "-" & DateTime.Now.Day.ToString() & "-" & DateTime.Now.Millisecond.ToString() & ".bak"
            ' we invoke this method to ensure we didnt mess up with other programs
            Dim temporaryTableName As String = "tblDBBackupTemp"

            ExeProcessSql([String].Format("BACKUP DATABASE {0} TO DISK = N'{1}\{0}.bak' " & _
                                          "WITH FORMAT, COPY_ONLY, INIT, NAME = N'{0} - Full Database " & _
                                          "Backup', SKIP ", databasename, tempPath, databasename))

            If Me.checkDBObjectExists(temporaryTableName, objectTypes.Table) Then
                ExeProcessSql([String].Format("DROP TABLE {0}", temporaryTableName))
            End If

            ExeProcessSql([String].Format("CREATE TABLE {0} (bck VARBINARY(MAX))", temporaryTableName))

            ExeProcessSql([String].Format("INSERT INTO {0} SELECT bck.* FROM " & _
                                          "OPENROWSET(BULK '{1}\{2}.bak',SINGLE_BLOB) bck", temporaryTableName, tempPath, databasename))

            Dim ds As DataSet
            ds = Me.GetDataSet([String].Format("SELECT bck FROM {0}", temporaryTableName), "temp", )

            Dim dr As DataRow = ds.Tables(0).Rows(0)
            Dim backupFromServer As Byte() = New Byte(-1) {}
            backupFromServer = DirectCast(dr("bck"), Byte())
            Dim aSize As New Integer()
            aSize = backupFromServer.GetUpperBound(0) + 1

            Dim fs As New System.IO.FileStream([String].Format("{0}\{1}", filepath, fileName), System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write)
            fs.Write(backupFromServer, 0, aSize)
            fs.Close()

            ExeProcessSql([String].Format("DROP TABLE {0}", temporaryTableName))
            'delete remote tempfile
            ExeProcessSql([String].Format("master..xp_cmdshell 'del {0}\{1}.bak'", tempPath, databasename))


            Dim fsOut As System.IO.FileStream = System.IO.File.Create(filepath & "/" & fileName.Replace(".bak", ".zip"))
            Dim zipStream As New ICSharpCode.SharpZipLib.Zip.ZipOutputStream(fsOut)

            zipStream.SetLevel(3)       '0-9, 9 being the highest level of compression
            'zipStream.Password = DBNull.Value   ' optional. Null is the same as not setting.

            ' This setting will strip the leading part of the folder path in the entries, to
            ' make the entries relative to the starting folder.
            ' To include the full path for each entry up to the drive root, assign folderOffset = 0.
            ' Dim folderOffset As Integer = folderName.Length + (If(folderName.EndsWith("\"), 0, 1))

            'CompressFolder(folderName, zipStream, folderOffset)

            Dim fi As New System.IO.FileInfo(filepath & "\" & fileName)

            Dim entryName As String = fileName  ' Makes the name in zip based on the folder
            entryName = ICSharpCode.SharpZipLib.Zip.ZipEntry.CleanName(entryName)       ' Removes drive from name and fixes slash direction
            Dim newEntry As New ICSharpCode.SharpZipLib.Zip.ZipEntry(entryName)
            newEntry.DateTime = fi.LastWriteTime            ' Note the zip format stores 2 second granularity

            ' Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
            '   newEntry.AESKeySize = 256;

            ' To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
            ' you need to do one of the following: Specify UseZip64.Off, or set the Size.
            ' If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
            ' but the zip will be in Zip64 format which not all utilities can understand.
            '   zipStream.UseZip64 = UseZip64.Off;
            newEntry.Size = fi.Length

            zipStream.PutNextEntry(newEntry)

            ' Zip the file in buffered chunks
            ' the "using" will close the stream even if an exception occurs
            Dim buffer As Byte() = New Byte(4095) {}
            Using streamReader As System.IO.FileStream = System.IO.File.OpenRead(filepath & "\" & fileName)
                ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(streamReader, zipStream, buffer)
            End Using
            zipStream.CloseEntry()

            zipStream.IsStreamOwner = True
            ' Makes the Close also Close the underlying stream
            zipStream.Close()

            'delete the non-zipped version
            fi.Delete()

            Return True


         

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "BackupDatabase", ex, cProcessInfo))
            Return Nothing
        Finally
            CloseConnection()
        End Try
        Return bReturn
    End Function


    Public Function RestoreDatabase(ByVal databaseName As String, ByVal filepath As String) As Boolean
        Dim cProcessInfo As String = "RestoreDatabase"
        Dim bReturn As Boolean = False
        Dim PathOnly As String
        Dim unzipped As Boolean = False
        Dim restoreFilePath As String = ""
        Try

            PathOnly = System.IO.Path.GetDirectoryName(filepath)

            'First we need to extract the .bak from a zip file
            If filepath.EndsWith(".zip") Then
                Dim fz As New ICSharpCode.SharpZipLib.Zip.FastZip
                fz.ExtractZip(filepath, PathOnly, "")
                filepath = filepath.Replace(".zip", ".bak")
                unzipped = True
            End If

            'then we need to copy the file and save it on the SQL server
            If Me.DatabaseServer = "127.0.0.1" Then
                restoreFilePath = filepath
            Else
                Dim miUri As String = "ftp://" & DatabaseServer & "/temp/" & System.IO.Path.GetFileName(filepath)
                Dim miRequest As Net.FtpWebRequest = Net.WebRequest.Create(miUri)
                miRequest.Credentials = New Net.NetworkCredential(cFtpUserName, cFtpPassword)
                miRequest.Method = Net.WebRequestMethods.Ftp.UploadFile
                Try
                    Dim bFile() As Byte = System.IO.File.ReadAllBytes(filepath)
                    Dim miStream As System.IO.Stream = miRequest.GetRequestStream()
                    miStream.Write(bFile, 0, bFile.Length)
                    miStream.Close()
                    miStream.Dispose()
                Catch ex As Exception
                    Throw New Exception(ex.Message & ". El Archivo no pudo ser enviado.")
                End Try
                'delete the unzipped local file now it is transfered
                If unzipped Then
                    Dim delFile As New System.IO.FileInfo(filepath)
                    delFile.Delete()
                End If
            End If
            'then we can restore the database
            Dim tempPath As String = "c:\temp"
            Dim Database As SqlDatabase = GetDatabase(databaseName)

            Dim backupFiles As String()() = GetBackupFiles(tempPath & "\" & System.IO.Path.GetFileName(filepath))

            If backupFiles.Length < 1 Then
                Throw New ApplicationException("Backup set should contain at least 1 logical file")
            End If
            ' sort out the movings
            Dim movings As String() = New String(backupFiles.Length - 1) {}
            For i As Integer = 0 To backupFiles.Length - 1
                Dim name As String = backupFiles(i)(0)
                Dim path__1 As String = backupFiles(i)(1)
                If System.IO.Path.GetExtension(path__1).ToLower() = ".mdf" Then
                    path__1 = Database.DataPath
                Else
                    path__1 = Database.LogPath
                End If

                movings(i) = [String].Format("MOVE '{0}' TO '{1}'", SqlFmt(name), SqlFmt(path__1))
            Next

            'Set as single_user
            ExeProcessSql([String].Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", databaseName))

            ExeProcessSql([String].Format("RESTORE DATABASE {0} FROM DISK = N'{1}' WITH REPLACE, {2}", databaseName, tempPath & "\" & System.IO.Path.GetFileName(filepath), String.Join(", ", movings)))

            ExeProcessSql([String].Format("ALTER DATABASE {0} SET MULTI_USER WITH ROLLBACK IMMEDIATE", databaseName))

            'delete remote tempfile
            ExeProcessSql([String].Format("master..xp_cmdshell 'del {0}\{1}'", tempPath, System.IO.Path.GetFileName(filepath)))

            'then we need to sort out the users access 
            ExeProcessSql([String].Format("EXEC sp_adduser '{0}', '{1}'", cUserName, cUserName))
            ExeProcessSql([String].Format("EXEC sp_addrolemember '{0}', '{1}'", "db_owner", cUserName))


        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "createDB", ex, cProcessInfo))
            Return Nothing
        Finally
            CloseConnection()
        End Try
        Return bReturn
    End Function

    Private Function GetBackupFiles(file As String) As String()()

        Dim dvFiles As DataView = GetDataSet([String].Format("RESTORE FILELISTONLY FROM DISK = '{0}'", SqlFmt(file)), "files").Tables(0).DefaultView

        Dim files As String()() = New String(dvFiles.Count - 1)() {}
        For i As Integer = 0 To dvFiles.Count - 1
            files(i) = New String() {DirectCast(dvFiles(i)("LogicalName"), String), DirectCast(dvFiles(i)("PhysicalName"), String)}
        Next
        Return files
    End Function

    Public Overridable Function GetDatabase(databaseName As String) As SqlDatabase
        If Not Me.checkDBObjectExists(databaseName, objectTypes.Database) Then
            Return Nothing
        Else



            Dim database As New SqlDatabase()
            ' database.Name = databaseName

            ' get database size
            Dim dvFiles As DataView = GetDataSet([String].Format("SELECT Status, (Size * 8) AS DbSize, Name, FileName FROM [{0}]..sysfiles", databaseName), "dbInfo").Tables(0).DefaultView

            For Each drFile As DataRowView In dvFiles
                Dim status As Integer = CInt(drFile("Status"))
                If (status And 64) = 0 Then
                    ' data file
                    database.DataName = DirectCast(drFile("Name"), String).Trim()
                    database.DataPath = DirectCast(drFile("FileName"), String).Trim()
                    database.DataSize = CInt(drFile("DbSize"))
                Else
                    ' log file
                    database.LogName = DirectCast(drFile("Name"), String).Trim()
                    database.LogPath = DirectCast(drFile("FileName"), String).Trim()
                    database.LogSize = CInt(drFile("DbSize"))
                End If
            Next

            ' get database uzers
            ' database.Users = GetDatabaseUsers(databaseName)
            Return database
        End If
    End Function

    Public Function ExeProcessSql(ByVal sql As String) As Integer
        Dim nUpdateCount As Integer
        Dim cProcessInfo As String = "Running: " & sql
        Try

            Dim oCmd As New SqlCommand(sql, oConn)

            If oConn.State = ConnectionState.Closed Then oConn.Open()
            cProcessInfo = "Running Sql: " & sql
            nUpdateCount = oCmd.ExecuteNonQuery

            oCmd = Nothing

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "exeProcessSql", ex, cProcessInfo))
        Finally
            CloseConnection()
        End Try
        Return nUpdateCount
    End Function

    Public Function ExeProcessSqlfromFile(ByVal filepath As String) As Integer

        Dim nUpdateCount As Integer
        Dim vstrSql As String
        Dim oFr As System.IO.StreamReader

        Dim cProcessInfo As String = filepath
        Try

            'Dot Net version will only work with Sql Server at the moment, we'll need to cater for other connectors here.

            oFr = System.IO.File.OpenText(filepath)
            vstrSql = oFr.ReadToEnd()
            oFr.Close()
            Dim oCmd As New SqlCommand(vstrSql, oConn)
            If oConn.State = ConnectionState.Closed Then oConn.Open()
            cProcessInfo = "Running Sql ('" & filepath & "'): " ' & vstrSql
            nUpdateCount = oCmd.ExecuteNonQuery

            oCmd = Nothing

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "exeProcessSqlfromFile", ex, cProcessInfo))
        Finally
            CloseConnection()
        End Try
        Return nUpdateCount
    End Function

    Public Function ExeProcessSqlorIgnore(ByVal sql As String) As Integer

        Dim nUpdateCount As Integer
        Dim cProcessInfo As String = "Running: " & sql
        Try

            Dim oCmd As New SqlCommand(sql, oConn)
            If oConn.State = ConnectionState.Closed Then oConn.Open()

            cProcessInfo = "Running Sql: " & sql
            nUpdateCount = oCmd.ExecuteNonQuery

            oCmd = Nothing


        Catch ex As Exception
            ' Dont Handle errors!
            nUpdateCount = -1
        Finally
            CloseConnection()
        End Try
        Return nUpdateCount
    End Function

    Public Function ExeProcessSqlScalar(ByVal sql As String) As String
        Dim cRes As Object
        Dim cProcessInfo As String = "Running: " & sql
        Try

            Dim oCmd As New SqlCommand(sql, oConn)
            If oConn.State = ConnectionState.Closed Then oConn.Open()
            cProcessInfo = "Running Sql: " & sql
            cRes = oCmd.ExecuteScalar

            If oConn.State <> ConnectionState.Closed Then
                oConn.Close()
            End If
            oCmd = Nothing
            If IsDBNull(cRes) Then
                cRes = Nothing
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "exeProcessSqlScalar", ex, cProcessInfo))
            cRes = Nothing

        Finally
            CloseConnection()
        End Try
        Return cRes
    End Function

    Public Function getDataReader(ByVal sql As String, Optional ByVal commandtype As CommandType = CommandType.Text, Optional ByVal parameters As Hashtable = Nothing) As SqlDataReader

        Dim cProcessInfo As String = "Running Sql: " & sql
        Dim oReader As SqlDataReader
        Try
            Dim oLConn As New SqlConnection(DatabaseConnectionString)
            AddHandler oLConn.StateChange, AddressOf _ConnectionState
            Dim oCmd As New SqlCommand(sql, oLConn)

            ' Set the command type
            oCmd.CommandType = commandtype

            ' Set the Paremeters if any
            If Not parameters Is Nothing Then
                Dim oEntry As DictionaryEntry
                For Each oEntry In parameters
                    oCmd.Parameters.Add(oEntry.Key, oEntry.Value)
                Next
            End If

            ' Open the connection
            If oLConn.State = ConnectionState.Closed Then oLConn.Open()

            oReader = oCmd.ExecuteReader(CommandBehavior.CloseConnection)

            oCmd = Nothing
            Return oReader

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataReader", ex, cProcessInfo))
            ErrorMsg = ex.Message
            Return Nothing
        End Try
    End Function
    ''' <summary>
    '''   Returns a dataset
    ''' </summary>
    ''' <param name="sql">The sql, table or stored procedure to call - when calling a table or SP remember to change querytype.</param>
    ''' <param name="tablename">The name of the table that will be added to the dataset</param>
    ''' <param name="parameters">A hashtable of parameters to be fed into the calling command</param>
    ''' <param name="querytype">The query type: plain text, table or stored procedure</param>
    ''' <param name="datasetname">The name of the dataset to be returned</param>
    ''' <param name="bHandleTimeouts">Indicates whether to pass timeouts through the standard error handling, or ignore them.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetDataSet(ByVal sql As String, ByVal tablename As String, Optional ByVal datasetname As String = "", Optional ByVal bHandleTimeouts As Boolean = False, Optional ByVal parameters As Hashtable = Nothing, Optional ByVal querytype As CommandType = CommandType.Text, Optional pageSize As Integer = 0, Optional pageNumber As Integer = 0) As DataSet
        Dim cProcessInfo As String = "Running Sql:  " & sql
        Dim oDs As New DataSet
        Dim nStartIndex As Integer = (pageSize * pageNumber) - pageSize + 1
        Try
            If oConn Is Nothing Then ResetConnection()
            Dim oDataAdpt As New SqlDataAdapter(sql, oConn)

            If oConn.State = ConnectionState.Closed Then oConn.Open()

            If datasetname <> "" Then
                oDs.DataSetName = datasetname
            End If

            oDataAdpt.SelectCommand.CommandType = querytype
            If nConnectTimeout > 15 Then
                oDataAdpt.SelectCommand.CommandTimeout = nConnectTimeout
            End If
            ' Set the Paremeters if any
            If Not parameters Is Nothing Then
                For Each oEntry As DictionaryEntry In parameters
                    oDataAdpt.SelectCommand.Parameters.Add(oEntry.Key, oEntry.Value)
                Next
            End If
            If pageSize > 0 Then
                oDataAdpt.Fill(oDs, nStartIndex, pageSize, tablename)
            Else
                oDataAdpt.Fill(oDs, tablename)
            End If
            Return oDs

        Catch ex As System.Data.SqlClient.SqlException
            If ex.Message.StartsWith("Timeout expired.") And bHandleTimeouts Then
                ' Deal with timeouts, return emtpy dataset
                Me.TimeOutException = True
                oDs = Nothing
            Else
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo))
                oDs = Nothing
            End If
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo))
            oDs = Nothing
        Finally
            CloseConnection()
        End Try
        Return oDs
    End Function

    Public Function GetDataValue(ByVal sql As String, Optional ByVal commandtype As CommandType = CommandType.Text, Optional ByVal parameters As Hashtable = Nothing, Optional ByVal nullreturnvalue As Object = Nothing) As Object

        Dim cProcessInfo As String = "Running Sql: " & sql
        Dim oScalarValue As Object
        Try
            Dim oCmd As New SqlCommand(sql, oConn)

            ' Set the command type
            oCmd.CommandType = commandtype

            ' Set the Paremeters if any
            If Not parameters Is Nothing Then
                Dim oEntry As DictionaryEntry
                For Each oEntry In parameters
                    oCmd.Parameters.Add(oEntry.Key, oEntry.Value)
                Next
            End If

            ' Open the connection
            If oConn.State = ConnectionState.Closed Then
                oConn.Open()
            End If

            oScalarValue = oCmd.ExecuteScalar()
            oConn.Close()
            oCmd = Nothing

            ' If the return value is NULL and a default return value for NULLs has been specififed, then return this instead
            If (Not (IsNothing(nullreturnvalue))) And (IsNothing(oScalarValue) Or IsDBNull(oScalarValue)) Then
                oScalarValue = nullreturnvalue
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataValue", ex, cProcessInfo))
            oScalarValue = nullreturnvalue
        Finally
            CloseConnection()
        End Try
        Return oScalarValue
    End Function

    Public Function GetXmlValue(ByVal sql As String, Optional ByVal commandtype As CommandType = CommandType.Text, Optional ByVal parameters As Hashtable = Nothing, Optional ByVal nullreturnvalue As Object = Nothing) As Object

        Dim cProcessInfo As String = "Running Sql: " & sql
        Dim cXmlValue As String = Nothing
        Try
            bAsync = True
            ResetConnection()
            Dim oCmd As New SqlCommand(sql, oConn)

            ' Set the command type
            oCmd.CommandType = commandtype

            ' Set the Paremeters if any
            If Not parameters Is Nothing Then
                Dim oEntry As DictionaryEntry
                For Each oEntry In parameters
                    oCmd.Parameters.Add(oEntry.Key, oEntry.Value)
                Next
            End If

            ' Open the connection
            If oConn.State = ConnectionState.Closed Then
                oConn.Open()
            End If

            Dim result As IAsyncResult
            result = oCmd.BeginExecuteXmlReader()

            Using reader As XmlReader = oCmd.EndExecuteXmlReader(result)
                cXmlValue += reader.ReadOuterXml
            End Using


            bAsync = False
            ResetConnection()
            'oConn.Close()
            oCmd = Nothing

            ' If the return value is NULL and a default return value for NULLs has been specififed, then return this instead
            If (Not (IsNothing(nullreturnvalue))) And (IsNothing(cXmlValue) Or IsDBNull(cXmlValue)) Then
                cXmlValue = nullreturnvalue
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataValue", ex, cProcessInfo))
            cXmlValue = nullreturnvalue
        Finally
            CloseConnection()
        End Try
        Return cXmlValue
    End Function

    Public Function GetXmlReaderValue(ByVal sql As String, Optional ByVal commandtype As CommandType = CommandType.Text, Optional ByVal parameters As Hashtable = Nothing, Optional ByVal nullreturnvalue As Object = Nothing) As Object

        Dim cProcessInfo As String = "Running Sql: " & sql
        Dim oXmlValue As System.Data.SqlTypes.SqlXml = Nothing
        Try

            Dim oDr As SqlDataReader
            oDr = getDataReader(sql, commandtype, parameters)

            Do While oDr.Read
                oXmlValue = oDr.GetSqlXml(0)
            Loop

            '  oXmlValue = GetDataValue(sql, commandtype, parameters)

            ' If the return value is NULL and a default return value for NULLs has been specififed, then return this instead
            If (Not (IsNothing(nullreturnvalue))) And (IsNothing(oXmlValue) Or IsDBNull(oXmlValue)) Then
                oXmlValue = nullreturnvalue
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataValue", ex, cProcessInfo))
            oXmlValue = nullreturnvalue
        Finally
            CloseConnection()
        End Try
        Return oXmlValue
    End Function


    Public Sub AddXMLValueToNode(ByVal sql As String, ByRef oElmt As XmlElement)

        Dim cProcessInfo As String = "Running Sql: " & sql
        Dim oXdoc As New XmlDocument
        Dim cmd As SqlCommand = Nothing
        Try

            If oConn.State = ConnectionState.Closed Then
                oConn.Open()
            End If


            cmd = New SqlCommand(sql, oConn)
            Dim reader As XmlReader = cmd.ExecuteXmlReader()

            If reader.Read() Then
                oXdoc.Load(reader)
            End If

            If Not oXdoc.DocumentElement Is Nothing Then
                Dim newNode As XmlNode = oElmt.OwnerDocument.ImportNode(oXdoc.DocumentElement, True)
                oElmt.AppendChild(newNode)
            End If

            oXdoc = Nothing

            reader.Dispose()
            reader = Nothing

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataValue", ex, cProcessInfo))

        Finally
            cmd.Dispose()
            CloseConnection()
        End Try

    End Sub

    Public Function getHashTable(ByVal sSql As String, ByVal sNameField As String, ByRef sValueField As String) As Hashtable
        'PerfMon.Log("dbTools", "getHashTable")
        Dim oDataAdpt As New SqlDataAdapter(sSql, oConn)
        Dim oDs As New DataSet
        Dim oDr As DataRow
        Dim oHash As Hashtable = New Hashtable
        Dim cProcessInfo As String = "getDataSet"
        Try

            oDataAdpt.Fill(oDs, "HashPairs")
            For Each oDr In oDs.Tables("HashPairs").Rows
                oHash.Add(CStr(oDr(sNameField)), CStr(oDr(sValueField)))
            Next

            oConn.Close()
            getHashTable = oHash

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo))
            Return Nothing
        End Try

    End Function

    Public Function GetIdInsertSql(ByVal sql As String) As String
        Dim nInsertId As Integer
        Dim dr As SqlDataReader
        Dim cProcessInfo As String = "Running: " & sql
        Try

            Dim oCmd As New SqlCommand(sql & ";select @@identity", oConn)
            If oConn Is Nothing Then
                oConn.Open()
            Else
                If oConn.State = ConnectionState.Closed Then oConn.Open()
            End If
            cProcessInfo = "Running Sql: " & sql
            dr = oCmd.ExecuteReader
            While dr.Read
                nInsertId = dr(0)
            End While
            dr.Close()
            dr = Nothing
            oCmd = Nothing
            oConn.Close()

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "getIdInsertSql", ex, cProcessInfo))
            nInsertId = Nothing
        Finally
            CloseConnection()
        End Try
        Return nInsertId
    End Function

    Public Shared Function SqlDate(ByVal dDateTime As Object, Optional ByVal bIncludeTime As Boolean = False) As String
        If IsDate(dDateTime) Then
            Dim dxDate As Date = CDate(dDateTime)
            If dxDate = #12:00:00 AM# Or dxDate = #12:00:00 AM# Then
                Return "null"
            ElseIf bIncludeTime Then
                Return "'" & Format(dxDate, "dd-MMM-yyyy HH:mm:ss") & "'"
            Else
                Return "'" & Format(dxDate, "dd-MMM-yyyy") & "'"
            End If
        Else
            Return "null"
        End If
    End Function

    Public Shared Function SqlFmt(ByVal text As String) As Object
        Return Replace(text, "'", "''")
    End Function

    Public Shared Function SqlString(ByVal text As String) As String
        Try
            If text = "Null" Then Return text
            If text = "''" Or text = "" Then Return "''"
            If Right(text, 1) = "'" Then text = text.Substring(0, text.Length - 1)
            If Left(text, 1) = "'" Then text = text.Substring(1, text.Length - 1)
            text = SqlFmt(text)
            text = "'" & text & "'"
            Return text
        Catch ex As Exception
            Return text
        End Try
    End Function

    Public Shared Function NothingDateDBNull(ByVal value As Date) As Object
        If value = Nothing Then Return DBNull.Value Else Return value
    End Function

    Public Function checkDBObjectExists(ByVal cObjectName As String, ByVal nObjectType As objectTypes) As Boolean

        Dim cObjectProperty As String = ""
        Dim cSql As String = ""
        Dim bReturn As Boolean = False

        Try
            If cObjectName <> "" Then
                If nObjectType = objectTypes.Database Then
                    If GetDataSet(String.Format("select name from master..sysdatabases where name = '{0}'", SqlFmt(cObjectName)), "db").Tables(0).Rows.Count > 0 Then
                        Return True
                    Else
                        Return False
                    End If
                Else
                    Select Case nObjectType
                        Case objectTypes.Table
                            cObjectProperty = " AND OBJECTPROPERTY(id, N'IsUserTable') = 1"
                        Case objectTypes.View
                            cObjectProperty = " AND OBJECTPROPERTY(id, N'IsView') = 1"
                        Case objectTypes.StoredProcedure
                            cObjectProperty = " AND OBJECTPROPERTY(id, N'IsProcedure') = 1"
                        Case objectTypes.UserFunction
                            cObjectProperty = " AND xtype in (N'FN', N'IF', N'TF')"
                    End Select
                    cSql = "SELECT COUNT(*) As c FROM sysobjects WHERE id = OBJECT_ID(N" & SqlString(cObjectName) & ")" & cObjectProperty
                    bReturn = (Me.GetDataValue(cSql, , , 0) > 0)
                End If
                End If
            Return bReturn

        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function checkDBObjectExists(ByVal dbObjectName As String) As Boolean

        Dim objectProperty As String = ""
        Dim sql As String = ""
        Dim objectExists As Boolean = False

        Try

            If Not String.IsNullOrEmpty(dbObjectName) Then
                objectProperty = "OBJECTPROPERTY(id, N'IsUserTable') = 1 OR OBJECTPROPERTY(id, N'IsView') = 1 OR OBJECTPROPERTY(id, N'IsProcedure') = 1 OR xtype in (N'FN', N'IF', N'TF')"
                sql = "SELECT COUNT(*) As c FROM sysobjects WHERE id = OBJECT_ID(N" & SqlString(dbObjectName) & ") AND (" & objectProperty & ")"
                objectExists = (Me.GetDataValue(sql, , , 0) > 0)
            End If

            Return objectExists

        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function checkTableColumnExists(ByVal tableName As String, ByVal columnName As String) As Boolean

        Dim columnExists As Boolean = False

        Try

            ' Check for empty strings.
            If Not (String.IsNullOrEmpty(tableName) Or String.IsNullOrEmpty(columnName)) Then



                ' Check table exists
                If checkDBObjectExists(tableName, objectTypes.Table) Then

                    ' Get an empty data set to check the schema
                    Dim checkSet As DataSet = GetDataSet("SELECT * FROM " & tableName & " WHERE 0=1", "setCheck")
                    columnExists = checkSet.Tables(0).Columns.Contains(columnName)

                End If

            End If

            Return columnExists

        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Shared Function HasColumn(ByVal dataRecordType As IDataRecord, ByVal columnToCheck As String) As Boolean

        For index As Integer = 0 To dataRecordType.FieldCount
            If dataRecordType.GetName(index).Equals(columnToCheck, StringComparison.InvariantCultureIgnoreCase) Then
                Return True
            End If
        Next

        Return False
    End Function


#End Region

#Region "    Public Procedures"

    Public Sub CloseConnection()
        Try
            If Not oConn Is Nothing Then
                If oConn.State <> ConnectionState.Closed Then oConn.Close()
            End If
        Catch ex As Exception
            '   RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CloseConnection", ex, "Setting Connection Info", Nothing))
            '   Dont raise event in case of looping
        End Try
    End Sub

    Public Sub CloseConnectionPool()
        Try
            SqlConnection.ClearPool(oConn)
        Catch ex As Exception

            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "CloseConnectionPool", ex, "Setting Connection Info", Nothing))
            'Dont raise event in case of looping

        End Try
    End Sub

    Public Sub addTableToDataSet(ByRef ds As DataSet, ByVal sql As String, ByVal tablename As String)

        Dim cProcessInfo As String = "tablename:" & tablename & " sql:" & sql
        Try

            Dim oDdpt As New SqlDataAdapter(sql, oConn)
            oDdpt.Fill(ds, tablename)

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "addTableToDataSet", ex, cProcessInfo))
        Finally
            CloseConnection()
        End Try
    End Sub

    Public Sub ReturnEmptyNulls(ByRef ds As DataSet)

        Dim oTable As DataTable
        Dim oRow As DataRow
        Dim oColumn As DataColumn

        Dim cProcessInfo As String = "getDataReader"
        Try
            For Each oTable In ds.Tables
                For Each oRow In oTable.Rows
                    For Each oColumn In oTable.Columns
                        'If IsDBNull(oRow.Item(oColumn.ColumnName)) Then
                        Select Case oColumn.DataType.ToString
                            Case "System.DateTime"
                                If Not IsDBNull(oRow.Item(oColumn.ColumnName)) Then
                                    If oRow.Item(oColumn.ColumnName) = CDate(#12:00:00 AM#) Then
                                        oRow.Item(oColumn.ColumnName) = DBNull.Value
                                    End If
                                End If

                        End Select
                        'End If
                    Next
                Next
            Next
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "setDataSet", ex, cProcessInfo))

        Finally
            CloseConnection()
        End Try

    End Sub

    Public Sub ReturnNullsEmpty(ByRef ds As DataSet)

        Dim oTable As DataTable
        Dim oRow As DataRow
        Dim oColumn As DataColumn

        Dim cProcessInfo As String = "returnNullsEmpty"
        Try
            For Each oTable In ds.Tables
                For Each oRow In oTable.Rows
                    For Each oColumn In oTable.Columns
                        If IsDBNull(oRow.Item(oColumn.ColumnName)) Then
                            cProcessInfo = "Error in Feild:" & oColumn.ColumnName & " DataType:" & oColumn.DataType.ToString
                            Select Case oColumn.DataType.ToString
                                Case "System.DateTime"
                                    oRow.Item(oColumn.ColumnName) = CDate(#12:00:00 AM#)
                                Case "System.Integer", "System.Int32", "System.Double", "System.Decimal"
                                    oRow.Item(oColumn.ColumnName) = 0
                                Case "System.Boolean"
                                    oRow.Item(oColumn.ColumnName) = False
                                Case Else
                                    oRow.Item(oColumn.ColumnName) = ""
                            End Select
                        End If
                    Next
                Next
            Next
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "returnNullsEmpty", ex, cProcessInfo))

        Finally
            CloseConnection()
        End Try

    End Sub

    Public Function GetColumnArray(ByRef oDs As DataSet, ByVal tableName As String, ByVal columnNames As String()) As DataColumn()
        Dim cProcessInfo As String = "GetColumnArray"
        Try
            Dim i As Integer
            Dim returnColumns(UBound(columnNames)) As DataColumn
            For i = 0 To UBound(columnNames)
                returnColumns(i) = oDs.Tables(tableName).Columns(columnNames(i))
            Next
            Return returnColumns
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetColumnArray", ex, cProcessInfo))
            Return Nothing
        End Try
    End Function

#End Region

    Public Class UpdateableDataset
        Inherits System.Data.DataSet
#Region "Declarations"
        Private oDA As SqlClient.SqlDataAdapter
        Private oCN As SqlClient.SqlConnection
        Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
        Private Const mcModuleName As String = "Eonic.Tools.Database"
#End Region

#Region "Public Procedures"
        Public Sub New(ByVal connection As String)
            Try
                oCN = New SqlClient.SqlConnection(connection)
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub
        Public Function Fill(ByVal sql As String, ByVal sourcetablename As String, ByVal destinationtablename As String) As Integer
            Try
                oDA = New SqlClient.SqlDataAdapter(sql, oCN)
                Dim nResult As Integer = oDA.Fill(Me, destinationtablename)
                Dim oCB As New SqlClient.SqlCommandBuilder(oDA)
                oDA.TableMappings.Add(sourcetablename, destinationtablename)
                oDA.DeleteCommand = oCB.GetDeleteCommand(True)
                oDA.InsertCommand = oCB.GetInsertCommand(True)
                oDA.UpdateCommand = oCB.GetUpdateCommand(True)
                Return nResult
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Fill", ex, ""))
                Return 0
            End Try
        End Function
        Public Function Update(ByVal tablename As String) As Integer
            Try
                Return oDA.Update(Me, "tablename")
            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Update", ex, ""))
                Return 0
            End Try
        End Function
#End Region
    End Class




#Region " IDisposable Support "
    Private disposedValue As Boolean = False        ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                CloseConnection()
                ' This was calling an error and am not sure if it is needed. so have removed whilst evalutating
                ' CloseConnectionPool()
            End If
        End If
        Me.disposedValue = True
    End Sub
    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    Public Sub New()

    End Sub


    Public Class SqlDatabase
        'Inherits ServiceProviderItem
        Private m_dataName As String
        Private m_dataPath As String
        Private m_dataSize As Integer
        Private m_logName As String
        Private m_logPath As String
        Private m_logSize As Integer
        Private m_users As String()
        Private m_location As String
        Private m_internalServerName As String
        Private m_externalServerName As String

        Public Sub New()
        End Sub

        Public Property DataName() As String
            Get
                Return m_dataName
            End Get
            Set(value As String)
                m_dataName = value
            End Set
        End Property

        Public Property DataPath() As String
            Get
                Return m_dataPath
            End Get
            Set(value As String)
                m_dataPath = value
            End Set
        End Property

        Public Property DataSize() As Integer
            Get
                Return m_dataSize
            End Get
            Set(value As Integer)
                m_dataSize = value
            End Set
        End Property

        Public Property LogName() As String
            Get
                Return m_logName
            End Get
            Set(value As String)
                m_logName = value
            End Set
        End Property

        Public Property LogPath() As String
            Get
                Return m_logPath
            End Get
            Set(value As String)
                m_logPath = value
            End Set
        End Property

        Public Property LogSize() As Integer
            Get
                Return m_logSize
            End Get
            Set(value As Integer)
                m_logSize = value
            End Set
        End Property

        Public Property Users() As String()
            Get
                Return m_users
            End Get
            Set(value As String())
                m_users = value
            End Set
        End Property

        Public Property Location() As String
            Get
                Return Me.m_location
            End Get
            Set(value As String)
                Me.m_location = value
            End Set
        End Property

        Public Property InternalServerName() As String
            Get
                Return Me.m_internalServerName
            End Get
            Set(value As String)
                Me.m_internalServerName = value
            End Set
        End Property

        Public Property ExternalServerName() As String
            Get
                Return Me.m_externalServerName
            End Get
            Set(value As String)
                Me.m_externalServerName = value
            End Set
        End Property


    End Class

End Class



