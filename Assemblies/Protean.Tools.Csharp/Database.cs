using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;

namespace Protean.Tools
{
    
    public class Database : IDisposable
    {
        private driverType oDriver;
        private string cServer = "";
        private string cDatabase = "";
        private string cUserName = "";
        private string cPassword = "";
        private string cFtpUserName = "";
        private string cFtpPassword = "";

        private int nConnectTimeout = 15;
        private int nMaxPoolSize = 100;
        private int nMinPoolSize = 0;
        private bool bPooling = false;
        public bool bAsync = false;

        public string ErrorMsg = "";

        private bool bTimeoutException = false;

        private SqlConnection _oConn;

        public SqlConnection oConn
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _oConn;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_oConn != null)
                {
                    _oConn.StateChange -= _ConnectionState;
                }

                _oConn = value;
                if (_oConn != null)
                {
                    _oConn.StateChange += _ConnectionState;
                }
            }
        }

        public event OnErrorEventHandler OnError; // , ByVal cModuleName As String, ByVal cRoutineName As String, ByVal oException As Exception, ByVal cFurtherInfo As String)

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        public event ConnectedEventHandler Connected;

        public delegate void ConnectedEventHandler(object sender, EventArgs e);

        public event DisconnectedEventHandler Disconnected;

        public delegate void DisconnectedEventHandler(object sender, EventArgs e);

        public event ConnectionStringChangedEventHandler ConnectionStringChanged;

        public delegate void ConnectionStringChangedEventHandler(object sender, EventArgs e);

        private const string mcModuleName = "Protean.Tools.Database";

        public enum objectTypes
        {
            Table,
            View,
            StoredProcedure,
            UserFunction,
            Database
        }

        public enum driverType
        {
            SQLServer,
            SQLServerNativeClient10ODBC
        }



        public driverType Driver
        {
            get
            {
                return oDriver;
            }
            set
            {
                oDriver = value;
            }
        }

        public string DatabaseServer
        {
            get
            {
                return cServer;
            }
            set
            {
                cServer = value;
                ResetConnection();
            }
        }

        public string DatabaseName
        {
            get
            {
                return cDatabase;
            }
            set
            {
                cDatabase = value;
                ResetConnection();
            }
        }

        public string DatabaseUser
        {
            get
            {
                return cUserName;
            }
            set
            {
                cUserName = value;
                ResetConnection();
            }
        }

        public string DatabasePassword
        {
            get
            {
                return cPassword;
            }
            set
            {
                cPassword = value;
                ResetConnection();
            }
        }


        public string FTPUser
        {
            get
            {
                return cFtpUserName;
            }
            set
            {
                cFtpUserName = value;
            }
        }

        public string FtpPassword
        {
            get
            {
                return cFtpPassword;
            }
            set
            {
                cFtpPassword = value;
            }
        }

        public string DatabaseConnectionString
        {
            get
            {
                string cReturn = "";

                // Select Case oDriver

                // Case driverType.SQLServerNativeClient10ODBC
                // cReturn = "Provider=SQLNCLI10; Server=" & DatabaseServer
                // cReturn &= "; Database=" & DatabaseName
                // cReturn &= "; Uid=" & DatabaseUser
                // cReturn &= "; Pwd=" & DatabasePassword
                // cReturn &= ";"
                // Case Else
                cReturn = "Data Source=" + DatabaseServer;
                cReturn += ";Initial Catalog=" + DatabaseName;
                cReturn += ";User ID=" + DatabaseUser;
                cReturn += ";password=" + DatabasePassword;
                cReturn += ";Persist Security Info=True";
                if (bPooling)
                {
                    cReturn += ";Pooling=true";
                    cReturn += ";Connect Timeout=" + nConnectTimeout;
                    cReturn += ";Max Pool Size=" + nMaxPoolSize;
                    cReturn += ";Min Pool Size=" + nMinPoolSize;
                }
                if (bAsync)
                    cReturn += ";Asynchronous Processing=true";
                // End Select

                return cReturn;
            }
        }

        public bool ConnectionValid
        {
            get
            {
                try
                {
                    oConn.Open();
                    oConn.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool CredentialsValid
        {
            get
            {
                try
                {
                    bool convalid;
                    oConn.Open();
                    convalid = this.checkDBObjectExists(DatabaseName, objectTypes.Database);
                    oConn.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool ConnectionPooling
        {
            get
            {
                return bPooling;
            }
            set
            {
                bPooling = value;
                ResetConnection();
            }
        }

        public int ConnectTimeout
        {
            get
            {
                return nConnectTimeout;
            }
            set
            {
                nConnectTimeout = value;
                ResetConnection();
            }
        }

        public int MaxPoolSize
        {
            get
            {
                return nMaxPoolSize;
            }
            set
            {
                nMaxPoolSize = value;
                ResetConnection();
            }
        }

        public int MinPoolSize
        {
            get
            {
                return nMinPoolSize;
            }
            set
            {
                nMinPoolSize = value;
                ResetConnection();
            }
        }

        public bool TimeOutException
        {
            get
            {
                return bTimeoutException;
            }
            set
            {
                bTimeoutException = value;
            }
        }



        private void _ConnectionState(object sender, System.Data.StateChangeEventArgs e)
        {
            switch (oConn.State)
            {
                case ConnectionState.Closed:
                    {
                        Disconnected?.Invoke(this, EventArgs.Empty);
                        break;
                    }

                case ConnectionState.Open:
                    {
                        Connected?.Invoke(this, EventArgs.Empty);
                        break;
                    }
            }
        }

        private void ResetConnection()
        {
            try
            {
                CloseConnection();
                // This was calling an error and am not sure if it is needed. so have removed whilst evalutating
                // CloseConnectionPool()
                oConn = new SqlConnection(DatabaseConnectionString);
                ConnectionStringChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
         
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ResetConnection", ex, "Setting Connection Info", 0));
            }
        }

        public bool CreateDatabase(string databasename)
        {
            string cProcessInfo = "createDB";
            bool bReturn = false;
            try
            {
                this.DatabaseName = "master";
                // Dim myConn As SqlConnection = New SqlConnection("Data Source=" & DatabaseServer & "; Initial Catalog=master; User ID=" & DatabaseUser & ";password=" & DatabasePassword & ";Persist Security Info=True")
                string sSql;

                sSql = "select db_id('" + databasename + "')";
                if (ExeProcessSqlScalar(sSql) == null)
                    ExeProcessSql("CREATE DATABASE " + databasename);
                else
                    bReturn = false;

                bReturn = true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "createDB", ex, cProcessInfo));
                return default(bool);
            }
            finally
            {
                CloseConnection();
            }
            return bReturn;
        }

        public bool BackupDatabase(string databasename, string filepath)
        {
            string cProcessInfo = "createDB";
            bool bReturn = false;
            string backupName = null;
            string zipName = null;
            try
            {

                // FIRST RUN THIS ON THE SQL SERVER

                // -- To allow advanced options to be changed.
                // EXEC(sp_configure) 'show advanced options', 1
                // GO()
                // -- To update the currently configured value for advanced options.
                // RECONFIGURE()
                // GO()
                // -- To enable the feature.
                // EXEC(sp_configure) 'xp_cmdshell', 1
                // GO()
                // -- To update the currently configured value for this feature.
                // RECONFIGURE()
                // GO()

                string tempPath = @"c:\temp";
                if (backupName == null)
                {
                    backupName = databasename + ".bak";
                    zipName = databasename + ".zip";
                }
                string bakFile = tempPath + backupName;
                string zipFile = tempPath + zipName;

                // Based on code found here...http://www.codeproject.com/Articles/33963/Transferring-backup-files-from-a-remote-SQL-Server

                // nice filename on local side, so we know when backup was done
                string fileName = databasename + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + "-" + DateTime.Now.Millisecond.ToString() + ".bak";
                // we invoke this method to ensure we didnt mess up with other programs
                string temporaryTableName = "tblDBBackupTemp";

                ExeProcessSql(String.Format(@"BACKUP DATABASE {0} TO DISK = N'{1}\{0}.bak' " + "WITH FORMAT, COPY_ONLY, INIT, NAME = N'{0} - Full Database " + "Backup', SKIP ", databasename, tempPath, databasename));

                if (this.checkDBObjectExists(temporaryTableName, objectTypes.Table))
                    ExeProcessSql(String.Format("DROP TABLE {0}", temporaryTableName));

                ExeProcessSql(String.Format("CREATE TABLE {0} (bck VARBINARY(MAX))", temporaryTableName));

                ExeProcessSql(String.Format("INSERT INTO {0} SELECT bck.* FROM " + @"OPENROWSET(BULK '{1}\{2}.bak',SINGLE_BLOB) bck", temporaryTableName, tempPath, databasename));

                DataSet ds;
                ds = this.GetDataSet(String.Format("SELECT bck FROM {0}", temporaryTableName), "temp");

                DataRow dr = ds.Tables[0].Rows[0];
                byte[] backupFromServer = new byte[0] { };
                backupFromServer = (byte[])dr["bck"];
                int aSize = new int();
                aSize = backupFromServer.GetUpperBound(0) + 1;

                System.IO.FileStream fs = new System.IO.FileStream(String.Format(@"{0}\{1}", filepath, fileName), System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);
                fs.Write(backupFromServer, 0, aSize);
                fs.Close();

                ExeProcessSql(String.Format("DROP TABLE {0}", temporaryTableName));
                // delete remote tempfile
                ExeProcessSql(String.Format(@"master..xp_cmdshell 'del {0}\{1}.bak'", tempPath, databasename));


                System.IO.FileStream fsOut = System.IO.File.Create(filepath + "/" + fileName.Replace(".bak", ".zip"));
                ICSharpCode.SharpZipLib.Zip.ZipOutputStream zipStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(fsOut);

                zipStream.SetLevel(3);       // 0-9, 9 being the highest level of compression
                                             // zipStream.Password = DBNull.Value   ' optional. Null is the same as not setting.

                // This setting will strip the leading part of the folder path in the entries, to
                // make the entries relative to the starting folder.
                // To include the full path for each entry up to the drive root, assign folderOffset = 0.
                // Dim folderOffset As Integer = folderName.Length + (If(folderName.EndsWith("\"), 0, 1))

                // CompressFolder(folderName, zipStream, folderOffset)

                System.IO.FileInfo fi = new System.IO.FileInfo(filepath + @"\" + fileName);

                string entryName = fileName;  // Makes the name in zip based on the folder
                entryName = ICSharpCode.SharpZipLib.Zip.ZipEntry.CleanName(entryName);       // Removes drive from name and fixes slash direction
                ICSharpCode.SharpZipLib.Zip.ZipEntry newEntry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime;            // Note the zip format stores 2 second granularity

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                // zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (System.IO.FileStream streamReader = System.IO.File.OpenRead(filepath + @"\" + fileName))
                {
                    ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();

                zipStream.IsStreamOwner = true;
                // Makes the Close also Close the underlying stream
                zipStream.Close();

                // delete the non-zipped version
                fi.Delete();

                return true;
            }



            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "BackupDatabase", ex, cProcessInfo));
                return default(bool);
            }
            finally
            {
                CloseConnection();
            }
            return bReturn;
        }


        public bool RestoreDatabase(string databaseName, string filepath)
        {
            string cProcessInfo = "RestoreDatabase";
            bool bReturn = false;
            string PathOnly;
            bool unzipped = false;
            string restoreFilePath = "";
            try
            {
                PathOnly = System.IO.Path.GetDirectoryName(filepath);

                // First we need to extract the .bak from a zip file
                if (filepath.EndsWith(".zip"))
                {
                    ICSharpCode.SharpZipLib.Zip.FastZip fz = new ICSharpCode.SharpZipLib.Zip.FastZip();
                    fz.ExtractZip(filepath, PathOnly, "");
                    filepath = filepath.Replace(".zip", ".bak");
                    unzipped = true;
                }

                // then we need to copy the file and save it on the SQL server
                if (this.DatabaseServer == "127.0.0.1")
                    restoreFilePath = filepath;
                else
                {
                    string miUri = "ftp://" + DatabaseServer + "/temp/" + System.IO.Path.GetFileName(filepath);
                    System.Net.FtpWebRequest miRequest = (System.Net.FtpWebRequest)System.Net.WebRequest.Create(miUri);
                    miRequest.Credentials = new System.Net.NetworkCredential(cFtpUserName, cFtpPassword);
                    miRequest.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
                    try
                    {
                        byte[] bFile = System.IO.File.ReadAllBytes(filepath);
                        System.IO.Stream miStream = miRequest.GetRequestStream();
                        miStream.Write(bFile, 0, bFile.Length);
                        miStream.Close();
                        miStream.Dispose();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + ". El Archivo no pudo ser enviado.");
                    }
                    // delete the unzipped local file now it is transfered
                    if (unzipped)
                    {
                        System.IO.FileInfo delFile = new System.IO.FileInfo(filepath);
                        delFile.Delete();
                    }
                }
                // then we can restore the database
                string tempPath = @"c:\temp";
                SqlDatabase Database = GetDatabase(databaseName);

                string[][] backupFiles = GetBackupFiles(tempPath + @"\" + System.IO.Path.GetFileName(filepath));

                if (backupFiles.Length < 1)
                    throw new ApplicationException("Backup set should contain at least 1 logical file");
                // sort out the movings
                string[] movings = new string[backupFiles.Length - 1 + 1];
                var loopTo = backupFiles.Length - 1;
                for (int i = 0; i <= loopTo; i++)
                {
                    string name = backupFiles[i][0];
                    string path__1 = backupFiles[i][1];
                    if (System.IO.Path.GetExtension(path__1).ToLower() == ".mdf")
                        path__1 = Database.DataPath;
                    else
                        path__1 = Database.LogPath;

                    movings[i] = String.Format("MOVE '{0}' TO '{1}'", SqlFmt(name), SqlFmt(path__1));
                }

                // Set as single_user
                ExeProcessSql(String.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", databaseName));

                ExeProcessSql(String.Format("RESTORE DATABASE {0} FROM DISK = N'{1}' WITH REPLACE, {2}", databaseName, tempPath + @"\" + System.IO.Path.GetFileName(filepath), string.Join(", ", movings)));

                ExeProcessSql(String.Format("ALTER DATABASE {0} SET MULTI_USER WITH ROLLBACK IMMEDIATE", databaseName));

                // delete remote tempfile
                ExeProcessSql(String.Format(@"master..xp_cmdshell 'del {0}\{1}'", tempPath, System.IO.Path.GetFileName(filepath)));

                // then we need to sort out the users access 
                ExeProcessSql(String.Format("EXEC sp_adduser '{0}', '{1}'", cUserName, cUserName));
                ExeProcessSql(String.Format("EXEC sp_addrolemember '{0}', '{1}'", "db_owner", cUserName));
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "createDB", ex, cProcessInfo));
                return default(bool);
            }
            finally
            {
                CloseConnection();
            }
            return bReturn;
        }

        private string[][] GetBackupFiles(string file)
        {
            DataView dvFiles = GetDataSet(String.Format("RESTORE FILELISTONLY FROM DISK = '{0}'", SqlFmt(file)), "files").Tables[0].DefaultView;

            string[][] files = new string[dvFiles.Count - 1 + 1][];
            var loopTo = dvFiles.Count - 1;
            for (int i = 0; i <= loopTo; i++)
                files[i] = new string[] { (string)dvFiles[i]["LogicalName"], (string)dvFiles[i]["PhysicalName"] };
            return files;
        }

        public virtual SqlDatabase GetDatabase(string databaseName)
        {
            if (!this.checkDBObjectExists(databaseName, objectTypes.Database))
                return null;
            else
            {
                SqlDatabase database = new SqlDatabase();
                // database.Name = databaseName

                // get database size
                DataView dvFiles = GetDataSet(String.Format("SELECT Status, (Size * 8) AS DbSize, Name, FileName FROM [{0}]..sysfiles", databaseName), "dbInfo").Tables[0].DefaultView;

                foreach (DataRowView drFile in dvFiles)
                {
                    int status = System.Convert.ToInt32(drFile["Status"]);
                    if ((status & 64) == 0)
                    {
                        // data file
                        database.DataName = ((string)drFile["Name"]).Trim();
                        database.DataPath = ((string)drFile["FileName"]).Trim();
                        database.DataSize = System.Convert.ToInt32(drFile["DbSize"]);
                    }
                    else
                    {
                        // log file
                        database.LogName = ((string)drFile["Name"]).Trim();
                        database.LogPath = ((string)drFile["FileName"]).Trim();
                        database.LogSize = System.Convert.ToInt32(drFile["DbSize"]);
                    }
                }

                // get database uzers
                // database.Users = GetDatabaseUsers(databaseName)
                return database;
            }
        }

        public int ExeProcessSql(string sql)
        {
            int nUpdateCount = 0;
            string cProcessInfo = "Running: " + sql;
            try
            {
                SqlCommand oCmd = new SqlCommand(sql, oConn);

                if (oConn.State ==  System.Data.ConnectionState.Closed)
                    oConn.Open();
                cProcessInfo = "Running Sql: " + sql;
                nUpdateCount = oCmd.ExecuteNonQuery();

                oCmd = null/* TODO Change to default(_) if this is not a reference type */;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "exeProcessSql", ex, cProcessInfo));
            }
            finally
            {
                CloseConnection();
            }
            return nUpdateCount;
        }

        public void ExeProcessSql(string sql, CommandType commandtype = CommandType.Text, Hashtable parameters = null)
        {
            string cProcessInfo = "Running Sql: " + sql;
            try
            {
                SqlCommand oCmd = new SqlCommand(sql, oConn);

                // Set the command type
                oCmd.CommandType = commandtype;

                // Set the command timeout
                if (nConnectTimeout > 15)
                {
                    oCmd.CommandTimeout = nConnectTimeout;
                }

                // Set the Paremeters if any
                if (!(parameters == null))
                {
                    foreach (DictionaryEntry oEntry in parameters)
                        oCmd.Parameters.AddWithValue(oEntry.Key.ToString(), oEntry.Value);
                }

                // Open the connection
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();

                oCmd.ExecuteNonQuery();
                oConn.Close();
                oCmd = null/* TODO Change to default(_) if this is not a reference type */;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataValue", ex, cProcessInfo));
            }
            finally
            {
                CloseConnection();
            }
        }

        public int ExeProcessSqlfromFile(string filepath, string errmsg = "")
        {
            int nUpdateCount;
            string vstrSql;
            System.IO.StreamReader oFr;

            string cProcessInfo = filepath;
            try
            {

                // Dot Net version will only work with Sql Server at the moment, we'll need to cater for other connectors here.

                oFr = System.IO.File.OpenText(filepath);
                vstrSql = oFr.ReadToEnd();
                oFr.Close();
                SqlCommand oCmd = new SqlCommand(vstrSql, oConn);
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();
                cProcessInfo = "Running Sql ('" + filepath + "'): "; // & vstrSql
                nUpdateCount = oCmd.ExecuteNonQuery();

                oCmd = null/* TODO Change to default(_) if this is not a reference type */;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "exeProcessSqlfromFile", ex, cProcessInfo));
                errmsg = ex.Message;
                nUpdateCount = -1;
            }
            finally
            {
                CloseConnection();
            }
            return nUpdateCount;
        }

        public int ExeProcessSqlorIgnore(string sql)
        {
            int nUpdateCount;
            string cProcessInfo = "Running: " + sql;
            try
            {
                SqlCommand oCmd = new SqlCommand(sql, oConn);
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();

                cProcessInfo = "Running Sql: " + sql;
                nUpdateCount = oCmd.ExecuteNonQuery();

                oCmd = null/* TODO Change to default(_) if this is not a reference type */;
            }

            catch (Exception ex)
            {
                // Dont Handle errors!
                nUpdateCount = -1;
            }
            finally
            {
                CloseConnection();
            }
            return nUpdateCount;
        }

        public string ExeProcessSqlScalar(string sql)
        {
            object cRes;
            string cProcessInfo = "Running: " + sql;
            try
            {
                SqlCommand oCmd = new SqlCommand(sql, oConn);
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();
                cProcessInfo = "Running Sql: " + sql;
                cRes = oCmd.ExecuteScalar();

                if (oConn.State != ConnectionState.Closed)
                    oConn.Close();
                oCmd = null/* TODO Change to default(_) if this is not a reference type */;
                if (cRes == System.DBNull.Value)
                    cRes = null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "exeProcessSqlScalar", ex, cProcessInfo));
                cRes = null;
            }

            finally
            {
                CloseConnection();
            }
            if (cRes == null) {
                return null;
            }
            else {
            return cRes.ToString();
            }
        }

        public SqlDataReader getDataReader(string sql, CommandType commandtype = CommandType.Text, Hashtable parameters = null/* TODO Change to default(_) if this is not a reference type */)
        {
            string cProcessInfo = "Running Sql: " + sql;
            SqlDataReader oReader;
            try
            {
                SqlConnection oLConn = new SqlConnection(DatabaseConnectionString);
                oLConn.StateChange += _ConnectionState;
                SqlCommand oCmd = new SqlCommand(sql, oLConn);

                // Set the command type
                oCmd.CommandType = commandtype;

                // Set the Paremeters if any
                if (!(parameters == null))
                {
                    foreach (DictionaryEntry oEntry in parameters)
                        oCmd.Parameters.AddWithValue(oEntry.Key.ToString(), oEntry.Value);
                }

                // Open the connection
                if (oLConn.State == ConnectionState.Closed)
                    oLConn.Open();

                oReader = oCmd.ExecuteReader(CommandBehavior.CloseConnection);

                oCmd = null/* TODO Change to default(_) if this is not a reference type */;
                return oReader;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataReader", ex, cProcessInfo));
                ErrorMsg = ex.Message;
                return null/* TODO Change to default(_) if this is not a reference type */;
            }
        }
        /// <summary>
        /// Returns a dataset
        /// </summary>
        /// <param name="sql">The sql, table or stored procedure to call - when calling a table or SP remember to change querytype.</param>
        /// <param name="tablename">The name of the table that will be added to the dataset</param>
        /// <param name="parameters">A hashtable of parameters to be fed into the calling command</param>
        /// <param name="querytype">The query type: plain text, table or stored procedure</param>
        /// <param name="datasetname">The name of the dataset to be returned</param>
        /// <param name="bHandleTimeouts">Indicates whether to pass timeouts through the standard error handling, or ignore them.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public DataSet GetDataSet(string sql, string tablename, string datasetname = "", bool bHandleTimeouts = false, Hashtable parameters = null/* TODO Change to default(_) if this is not a reference type */, CommandType querytype = CommandType.Text, int pageSize = 0, int pageNumber = 0)
        {
            string cProcessInfo = "Running Sql:  " + sql;
            DataSet oDs = new DataSet();
            int nStartIndex = (pageSize * pageNumber) - pageSize + 1;
            try
            {
                if (oConn == null)
                    ResetConnection();
                SqlDataAdapter oDataAdpt = new SqlDataAdapter(sql, oConn);

                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();

                if (datasetname != "")
                    oDs.DataSetName = datasetname;

                oDataAdpt.SelectCommand.CommandType = querytype;
                if (nConnectTimeout > 15)
                    oDataAdpt.SelectCommand.CommandTimeout = nConnectTimeout;
                // Set the Paremeters if any
                if (!(parameters == null))
                {
                    foreach (DictionaryEntry oEntry in parameters)
                        oDataAdpt.SelectCommand.Parameters.Add(oEntry.Key.ToString(), oEntry.Value);
                }
                if (pageSize > 0)
                    oDataAdpt.Fill(oDs, nStartIndex, pageSize, tablename);
                else
                    oDataAdpt.Fill(oDs, tablename);
                return oDs;
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                if (ex.Message.StartsWith("Timeout expired.") & bHandleTimeouts)
                {
                    // Deal with timeouts, return emtpy dataset
                    this.TimeOutException = true;
                    oDs = null/* TODO Change to default(_) if this is not a reference type */;
                }
                else
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo));
                    oDs = null/* TODO Change to default(_) if this is not a reference type */;
                }
            }
            catch (Exception ex)
            {
               
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo));
                oDs = null/* TODO Change to default(_) if this is not a reference type */;
            }
            finally
            {
                CloseConnection();
            }
            return oDs;
        }

        public object GetDataValue(string sql, CommandType commandtype = CommandType.Text, Hashtable parameters = null/* TODO Change to default(_) if this is not a reference type */, object nullreturnvalue = null)
        {
            string cProcessInfo = "Running Sql: " + sql;
            object oScalarValue;
            try
            {
                SqlCommand oCmd = new SqlCommand(sql, oConn);

                // Set the command type
                oCmd.CommandType = commandtype;

                // Set the Paremeters if any
                if (!(parameters == null))
                {
                    
                    foreach (DictionaryEntry oEntry in parameters)
                        oCmd.Parameters.AddWithValue(oEntry.Key.ToString(), oEntry.Value);
                }

                // Open the connection
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();

                oScalarValue = oCmd.ExecuteScalar();
                oConn.Close();
                oCmd = null/* TODO Change to default(_) if this is not a reference type */;

                // If the return value is NULL and a default return value for NULLs has been specififed, then return this instead
                if ((!(nullreturnvalue == null)) & (oScalarValue == null | oScalarValue == null))
                    oScalarValue = nullreturnvalue;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataValue", ex, cProcessInfo));
                oScalarValue = nullreturnvalue;
            }
            finally
            {
                CloseConnection();
            }
            if (oScalarValue != nullreturnvalue)
            {
                if (oScalarValue.GetType() != typeof(DBNull))
                {
                    return oScalarValue;
                }
                else
                {
                    return nullreturnvalue;
                };

            }
            else
            {
                return nullreturnvalue;
            };
        }

        public object GetXmlValue(string sql, CommandType commandtype = CommandType.Text, Hashtable parameters = null/* TODO Change to default(_) if this is not a reference type */, object nullreturnvalue = null)
        {
            string cProcessInfo = "Running Sql: " + sql;
            string cXmlValue = null;
            try
            {
                bAsync = true;
                ResetConnection();
                SqlCommand oCmd = new SqlCommand(sql, oConn);

                // Set the command type
                oCmd.CommandType = commandtype;

                // Set the Paremeters if any
                if (!(parameters == null))
                {
                       foreach (DictionaryEntry oEntry in parameters)
                        oCmd.Parameters.AddWithValue(oEntry.Key.ToString(), oEntry.Value);
                }

                // Open the connection
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();

                IAsyncResult result;
                result = oCmd.BeginExecuteXmlReader();

                using (XmlReader reader = oCmd.EndExecuteXmlReader(result))
                {
                    cXmlValue += reader.ReadOuterXml();
                }


                bAsync = false;
                ResetConnection();
                // oConn.Close()
                oCmd = null/* TODO Change to default(_) if this is not a reference type */;

                // If the return value is NULL and a default return value for NULLs has been specififed, then return this instead
                if ((!(nullreturnvalue == null)) & (cXmlValue == null | cXmlValue == null))
                    cXmlValue = nullreturnvalue.ToString();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataValue", ex, cProcessInfo));
                cXmlValue = nullreturnvalue.ToString();
            }
            finally
            {
                CloseConnection();
            }
            return cXmlValue;
        }

        public object GetXmlReaderValue(string sql, CommandType commandtype = CommandType.Text, Hashtable parameters = null/* TODO Change to default(_) if this is not a reference type */, object nullreturnvalue = null)
        {
            string cProcessInfo = "Running Sql: " + sql;
            System.Data.SqlTypes.SqlXml oXmlValue = null;
            try
            {
                SqlDataReader oDr;
                oDr = getDataReader(sql, commandtype, parameters);

                while (oDr.Read())
                    oXmlValue = oDr.GetSqlXml(0);

                // oXmlValue = GetDataValue(sql, commandtype, parameters)

                // If the return value is NULL and a default return value for NULLs has been specififed, then return this instead
                if ((!(nullreturnvalue == null)) & (oXmlValue == null | oXmlValue == null))
                    oXmlValue = null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataValue", ex, cProcessInfo));
                oXmlValue = null;
            }
            finally
            {
                CloseConnection();
            }
            return oXmlValue;
        }


        public XmlDocument GetXml(DataSet src)
        {
            // TS This function was added when we move to C# as GetXML does not return null fields in the XML. Need to convert to string and return empty string.
            // TS Additional issues with date formats as we need to ensure the convertion to date still outputs Valid XML dates.
            string cProcessInfo = "";
            try
                {
                    
                    DataTable dtCloned = src.Tables[0].Clone();
                    foreach (DataColumn dc in dtCloned.Columns){
                            dc.DataType = typeof(string);
                    }

                    foreach (DataRow row in src.Tables[0].Rows)
                    {
                        dtCloned.ImportRow(row);
                    }

                    foreach (DataRow row in dtCloned.Rows)
                    {
                        for (int i = 0; i < dtCloned.Columns.Count; i++)
                        {
                            dtCloned.Columns[i].ReadOnly = false;
                            if (src.Tables[0].Columns[i].DataType == typeof(DateTime)) {
                                if (string.IsNullOrEmpty(row[i].ToString()))
                                {
                                    row[i] = string.Empty;
                                }
                                else {
                                    row[i] = Protean.Tools.Xml.XmlDate(row[i], true);
                                  }
                            }
                            else if(string.IsNullOrEmpty(row[i].ToString())){
                                row[i] = string.Empty;
                            }
                        }
                    }

                    DataSet ds = new DataSet(src.DataSetName);
                    ds.Tables.Add(dtCloned);
                    //string strXdocOrig = src.GetXml();
            
                    string strXdoc = ds.GetXml();
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(strXdoc);
                    return xdoc;
                }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetXml", ex, cProcessInfo));
                return null;
            }

        }

       

        public void AddXMLValueToNode(string sql, ref XmlElement oElmt)
        {
            string cProcessInfo = "Running Sql: " + sql;
            XmlDocument oXdoc = new XmlDocument();
            SqlCommand cmd = null/* TODO Change to default(_) if this is not a reference type */;
            try
            {
                if (oConn.State == ConnectionState.Closed)
                    oConn.Open();


                cmd = new SqlCommand(sql, oConn);
                XmlReader reader = cmd.ExecuteXmlReader();

                if (reader.Read())
                    oXdoc.Load(reader);

                if (!(oXdoc.DocumentElement == null))
                {
                    XmlNode newNode = oElmt.OwnerDocument.ImportNode(oXdoc.DocumentElement, true);
                    oElmt.AppendChild(newNode);
                }

                oXdoc = null;

                reader.Dispose();
                reader = null;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataValue", ex, cProcessInfo));
            }

            finally
            {
                cmd.Dispose();
                CloseConnection();
            }
        }

        public Hashtable getHashTable(string sSql, string sNameField, ref string sValueField)
        {
            // PerfMon.Log("dbTools", "getHashTable")
            SqlDataAdapter oDataAdpt = new SqlDataAdapter(sSql, oConn);
            DataSet oDs = new DataSet();
   
            Hashtable oHash = new Hashtable();
            string cProcessInfo = "getDataSet";
            try
            {
                oDataAdpt.Fill(oDs, "HashPairs");
                foreach (DataRow oDr in oDs.Tables["HashPairs"].Rows)
                    oHash.Add(System.Convert.ToString(oDr[sNameField]), System.Convert.ToString(oDr[sValueField]));

                oConn.Close();
                return oHash;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getDataSet", ex, cProcessInfo));
                return null/* TODO Change to default(_) if this is not a reference type */;
            }
        }

        public string GetIdInsertSql(string sql)
        {
            int nInsertId = 0;
            SqlDataReader dr;
            string cProcessInfo = "Running: " + sql;
            try
            {
                SqlCommand oCmd = new SqlCommand(sql + ";select @@identity", oConn);
                if (oConn == null)
                    oConn.Open();
                else if (oConn.State == ConnectionState.Closed)
                    oConn.Open();
                cProcessInfo = "Running Sql: " + sql;
                dr = oCmd.ExecuteReader();
                while (dr.Read())
                    nInsertId = Convert.ToInt32(dr[0]);
                dr.Close();
                dr = null/* TODO Change to default(_) if this is not a reference type */;
                oCmd = null/* TODO Change to default(_) if this is not a reference type */;
                oConn.Close();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getIdInsertSql", ex, cProcessInfo));
                nInsertId = default(int);
                oConn.Close();
            }
            finally
            {
                CloseConnection();
            }
            return nInsertId.ToString();
        }

        public static string SqlDate(object dDateTime, bool bIncludeTime = false)
        {
            try
            {
                if (dDateTime is DateTime) {
                    DateTime thisDate = (DateTime)dDateTime;
                    if (thisDate.Year.ToString() == "1") {
                        return "null";
                    } else {
                        if (bIncludeTime)
                            return "'" + thisDate.ToString("dd-MMM-yyyy HH:mm:ss") + "'";
                        else
                            return "'" + thisDate.ToString("dd-MMM-yyyy") + "'";
                    }

                }
                else if (dDateTime is null) {
                    return "null";
                }
                else if (Microsoft.VisualBasic.Information.IsDate(dDateTime.ToString()))
                {
                    // DateTime dxDate = (DateTime)dDateTime;
                    DateTime dxDate = Convert.ToDateTime(dDateTime.ToString());
                    //DateTime dxDate = DateTime.ParseExact(dDateTime.ToString(), "yyyy-mm-dd", null);
                    if (dxDate == DateTime.Parse("0001-01-01") | dxDate == DateTime.Parse("0001-01-01"))
                        return "null";
                    else if (bIncludeTime)
                        return "'" + dxDate.ToString("dd-MMM-yyyy HH:mm:ss") + "'";
                    else
                        return "'" + dxDate.ToString("dd-MMM-yyyy") + "'";
                }
                else
                    return "null";
        }
            catch (Exception ex)
            {
                return "date error";
            }
}

        public static string SqlFmt(string text)
        {
            if (text == null)
            {
                return "";
            }
            else { 
                return text.Replace("'", "''");
            }
        }

        public static string SqlString(string text)
        {
            try
            {
                if (text == "Null")
                    return text;
                if (text == "''" | text == "")
                    return "''";
                if (Microsoft.VisualBasic.Strings.Right(text,1) == "'")
                    text = text.Substring(0, text.Length - 1);
                if (Microsoft.VisualBasic.Strings.Left(text, 1) == "'")
                    text = text.Substring(1, text.Length - 1);
                text = SqlFmt(text);
                text = "'" + text + "'";
                return text;
            }
            catch (Exception ex)
            {
                return text;
            }
        }

        public static object NothingDateDBNull(DateTime value)
        {
            if (value == default(DateTime))
                return DBNull.Value;
            else
                return value;
        }

        public bool checkDBObjectExists(string cObjectName, objectTypes nObjectType)
        {
            string cObjectProperty = "";
            string cSql = "";
            bool bReturn = false;

            try
            {
                if (cObjectName != "")
                {
                    if (nObjectType == objectTypes.Database)
                    {
                        if (GetDataSet(string.Format("select name from master..sysdatabases where name = '{0}'", SqlFmt(cObjectName)), "db").Tables[0].Rows.Count > 0)
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        switch (nObjectType)
                        {
                            case objectTypes.Table:
                                {
                                    cObjectProperty = " AND OBJECTPROPERTY(id, N'IsUserTable') = 1";
                                    break;
                                }

                            case objectTypes.View:
                                {
                                    cObjectProperty = " AND OBJECTPROPERTY(id, N'IsView') = 1";
                                    break;
                                }

                            case objectTypes.StoredProcedure:
                                {
                                    cObjectProperty = " AND OBJECTPROPERTY(id, N'IsProcedure') = 1";
                                    break;
                                }

                            case objectTypes.UserFunction:
                                {
                                    cObjectProperty = " AND xtype in (N'FN', N'IF', N'TF')";
                                    break;
                                }
                        }
                        cSql = "SELECT COUNT(*) As c FROM sysobjects WHERE id = OBJECT_ID(N" + SqlString(cObjectName) + ")" + cObjectProperty;
                        bReturn = ((int)this.GetDataValue(cSql, nullreturnvalue: 0) > 0);
                    }
                }
                return bReturn;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool checkDBObjectExists(string dbObjectName)
        {
            string objectProperty = "";
            string sql = "";
            bool objectExists = false;

            try
            {
                if (!string.IsNullOrEmpty(dbObjectName))
                {
                    objectProperty = "OBJECTPROPERTY(id, N'IsUserTable') = 1 OR OBJECTPROPERTY(id, N'IsView') = 1 OR OBJECTPROPERTY(id, N'IsProcedure') = 1 OR xtype in (N'FN', N'IF', N'TF')";
                    sql = "SELECT COUNT(*) As c FROM sysobjects WHERE id = OBJECT_ID(N" + SqlString(dbObjectName) + ") AND (" + objectProperty + ")";
                    objectExists = ((int)this.GetDataValue(sql, nullreturnvalue: 0) > 0);
                }

                return objectExists;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool checkTableColumnExists(string tableName, string columnName)
        {
            bool columnExists = false;

            try
            {

                // Check for empty strings.
                if (!(string.IsNullOrEmpty(tableName) | string.IsNullOrEmpty(columnName)))
                {
                    // Check table exists
                    if (checkDBObjectExists(tableName, objectTypes.Table))
                    {
                        // Get an empty data set to check the schema
                        DataSet checkSet = GetDataSet("SELECT * FROM " + tableName + " WHERE 0=1", "setCheck");
                        columnExists = checkSet.Tables[0].Columns.Contains(columnName);
                    }
                }

                return columnExists;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public static bool HasColumn(IDataRecord dataRecordType, string columnToCheck)
        {
            var loopTo = dataRecordType.FieldCount;
            for (int index = 0; index <= loopTo; index++)
            {
                if (dataRecordType.GetName(index).Equals(columnToCheck, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }




        public void CloseConnection()
        {
            try
            {
                if (!(oConn == null))
                {
                    if (oConn.State != ConnectionState.Closed)
                        oConn.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void CloseConnectionPool()
        {
            try
            {
                SqlConnection.ClearPool(oConn);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CloseConnectionPool", ex, "Setting Connection Info", 0));
            }
        }

        public void addTableToDataSet(ref DataSet ds, string sql, string tablename)
        {
            string cProcessInfo = "tablename:" + tablename + " sql:" + sql;
            try
            {
                SqlDataAdapter oDdpt = new SqlDataAdapter(sql, oConn);
                oDdpt.Fill(ds, tablename);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addTableToDataSet", ex, cProcessInfo));
            }
            finally
            {
                CloseConnection();
            }
        }

        public void ReturnEmptyNulls(ref DataSet ds)
        {

            string cProcessInfo = "getDataReader";
            try
            {
                foreach (DataTable oTable in ds.Tables)
                {
                    foreach (DataRow oRow in oTable.Rows)
                    {
                        foreach (DataColumn oColumn in oTable.Columns)
                        {
                            // If IsDBNull(oRow.Item(oColumn.ColumnName)) Then
                            switch (oColumn.DataType.ToString())
                            {
                                case "System.DateTime":
                                    {
                                        if (!(oRow[oColumn.ColumnName] == null))
                                        {
                                            if ((DateTime)oRow[oColumn.ColumnName] == (DateTime)DateTime.Parse("0001-01-01"))
                                                oRow[oColumn.ColumnName] = DBNull.Value;
                                        }

                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "setDataSet", ex, cProcessInfo));
            }

            finally
            {
                CloseConnection();
            }
        }

        public void ReturnNullsEmpty(ref DataSet ds)
        {
            string cProcessInfo = "returnNullsEmpty";
            try
            {
                foreach (DataTable oTable in ds.Tables)
                {
                    foreach (DataRow oRow in oTable.Rows)
                    {
                        foreach (DataColumn oColumn in oTable.Columns)
                        {
                            if (oRow[oColumn.ColumnName] == null)
                            {
                                cProcessInfo = "Error in Feild:" + oColumn.ColumnName + " DataType:" + oColumn.DataType.ToString();
                                switch (oColumn.DataType.ToString())
                                {
                                    case "System.DateTime":
                                        {
                                            oRow[oColumn.ColumnName] = (DateTime)DateTime.Parse("0001-01-01");
                                            break;
                                        }

                                    case "System.Integer":
                                    case "System.Int32":
                                    case "System.Double":
                                    case "System.Decimal":
                                        {
                                            oRow[oColumn.ColumnName] = 0;
                                            break;
                                        }

                                    case "System.Boolean":
                                        {
                                            oRow[oColumn.ColumnName] = false;
                                            break;
                                        }

                                    default:
                                        {
                                            oRow[oColumn.ColumnName] = "";
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "returnNullsEmpty", ex, cProcessInfo));
            }

            finally
            {
                CloseConnection();
            }
        }

        public DataColumn[] GetColumnArray(ref DataSet oDs, string tableName, string[] columnNames)
        {
            string cProcessInfo = "GetColumnArray";
            try
            {
                int i;
                DataColumn[] returnColumns = new DataColumn[Microsoft.VisualBasic.Information.UBound(columnNames) + 1];
                var loopTo = Microsoft.VisualBasic.Information.UBound(columnNames);
                for (i = 0; i <= loopTo; i++)
                    returnColumns[i] = oDs.Tables[tableName].Columns[columnNames[i]];
                return returnColumns;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetColumnArray", ex, cProcessInfo));
                return null;
            }
        }


        public class UpdateableDataset : System.Data.DataSet
        {
            private SqlDataAdapter oDA;
            private SqlConnection oCN;
            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

            private const string mcModuleName = "Protean.Tools.Database";

            public UpdateableDataset(string connection)
            {
                try
                {
                    oCN = new SqlConnection(connection);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }
            }
            public int Fill(string sql, string sourcetablename, string destinationtablename)
            {
                try
                {
                    oDA = new SqlDataAdapter(sql, oCN);
                    int nResult = oDA.Fill(this, destinationtablename);
                    SqlCommandBuilder oCB = new SqlCommandBuilder(oDA);
                    oDA.TableMappings.Add(sourcetablename, destinationtablename);
                    oDA.DeleteCommand = oCB.GetDeleteCommand(true);
                    oDA.InsertCommand = oCB.GetInsertCommand(true);
                    oDA.UpdateCommand = oCB.GetUpdateCommand(true);
                    return nResult;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Fill", ex, ""));
                    return 0;
                }
            }
            public int Update(string tablename)
            {
                try
                {
                    return oDA.Update(this, "tablename");
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Update", ex, ""));
                    return 0;
                }
            }
        }




        private bool disposedValue = false;        // To detect redundant calls

        // IDisposable
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                    CloseConnection();
            }
            this.disposedValue = true;
        }
        // This code added by Visual Basic to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Database()
        {
        }


        public class SqlDatabase
        {
            // Inherits ServiceProviderItem
            private string m_dataName;
            private string m_dataPath;
            private int m_dataSize;
            private string m_logName;
            private string m_logPath;
            private int m_logSize;
            private string[] m_users;
            private string m_location;
            private string m_internalServerName;
            private string m_externalServerName;

            public SqlDatabase()
            {
            }

            public string DataName
            {
                get
                {
                    return m_dataName;
                }
                set
                {
                    m_dataName = value;
                }
            }

            public string DataPath
            {
                get
                {
                    return m_dataPath;
                }
                set
                {
                    m_dataPath = value;
                }
            }

            public int DataSize
            {
                get
                {
                    return m_dataSize;
                }
                set
                {
                    m_dataSize = value;
                }
            }

            public string LogName
            {
                get
                {
                    return m_logName;
                }
                set
                {
                    m_logName = value;
                }
            }

            public string LogPath
            {
                get
                {
                    return m_logPath;
                }
                set
                {
                    m_logPath = value;
                }
            }

            public int LogSize
            {
                get
                {
                    return m_logSize;
                }
                set
                {
                    m_logSize = value;
                }
            }

            public string[] Users
            {
                get
                {
                    return m_users;
                }
                set
                {
                    m_users = value;
                }
            }

            public string Location
            {
                get
                {
                    return this.m_location;
                }
                set
                {
                    this.m_location = value;
                }
            }

            public string InternalServerName
            {
                get
                {
                    return this.m_internalServerName;
                }
                set
                {
                    this.m_internalServerName = value;
                }
            }

            public string ExternalServerName
            {
                get
                {
                    return this.m_externalServerName;
                }
                set
                {
                    this.m_externalServerName = value;
                }
            }
        }
    }

}
