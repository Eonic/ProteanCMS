using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace Protean.Tools
{

    public class MySqlDatabase : IDisposable
    {
        private driverType oDriver;
        private string cServer = "";
        private string cPort = "";
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

        private MySqlConnection _oConn;

        public MySqlConnection oConn
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

        private const string mcModuleName = "Protean.Tools.Csharp.MySqlDatabase";

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

        public MySqlDatabase(string cConnectionString)
        {
            try
            {
                ResetConnection(cConnectionString);
                ConnectionPooling = true;
                ConnectTimeout = 15;
                MinPoolSize = 0;
                MaxPoolSize = 100;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
            }
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

        public string DatabasePort
        {
            get
            {
                return cPort;
            }
            set
            {
                cPort = value;
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
                cReturn = "Server=" + DatabaseServer;
                cReturn += ";Port=" + DatabasePort;
                cReturn += ";Database=" + DatabaseName;
                cReturn += ";Uid=" + DatabaseUser;
                cReturn += ";Pwd=" + DatabasePassword;
                if (bPooling)
                {
                    cReturn += ";Pooling=True";
                    cReturn += ";Connection Timeout=" + nConnectTimeout;
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
                oConn = new MySqlConnection(DatabaseConnectionString);
                ConnectionStringChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ResetConnection", ex, "Setting Connection Info", 0));
            }
        }

        public void ResetConnection(string cConnectionString)
        {
            string cPassword;
            string cUsername;
            string cDatabasePort;
            string cDBAuth = string.Empty;
            try
            {
                MySqlConnection ocon = new MySqlConnection(cConnectionString);
                DatabaseName = ocon.Database;
                DatabaseServer = ocon.DataSource;
                ocon.Close();
                ocon = null/* TODO Change to default(_) if this is not a reference type */;

                // Let's work out where to get the authorisation from - ideally it should be the connection string.
                if (cConnectionString.ToLower().Contains("uid=") & cConnectionString.ToLower().Contains("pwd="))
                {
                    cDBAuth = cConnectionString;
                }
                //else
                //    // No authorisation information provided in the connection string.  
                //    // We need to source it from somewhere, let's try the web.config 
                //    cDBAuth = GetDBAuth();

                // Let's find the username and password
                cDatabasePort = Protean.Tools.Text.SimpleRegexFind(cDBAuth, "port=([^;]*)", 1, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                cUsername = Protean.Tools.Text.SimpleRegexFind(cDBAuth, "uid=([^;]*)", 1, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                cPassword = Protean.Tools.Text.SimpleRegexFind(cDBAuth, "pwd=([^;]*)", 1, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                DatabasePort = cDatabasePort;
                DatabaseUser = cUsername;
                DatabasePassword = cPassword;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ResetConnection", ex, ""));
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
                MySqlDataAdapter oDataAdpt = new MySqlDataAdapter(sql, oConn);

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
                        oDataAdpt.SelectCommand.Parameters.AddWithValue(oEntry.Key.ToString(), oEntry.Value);
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

        public int ExecuteScalar(string sql, Hashtable parameters = null)
        {
            string cProcessInfo = "Running Sql:  " + sql;
            int iResult = 0;

            if (oConn == null)
                ResetConnection();
            try
            {
                MySqlCommand myCommand = new MySqlCommand(sql, oConn);
                myCommand.Connection.Open();
                myCommand.ExecuteScalar();
                oConn.Close();
                iResult = 1;
            }
            catch (Exception ex)
            {
                iResult = 0;
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ExecuteScalar", ex, cProcessInfo));
            }

            finally
            {
                CloseConnection();
            }
            return iResult;
        }

        public int ExecuteNonQuery(string sql, string parameters, int commandType)
        {
            string cProcessInfo = "Running Sql:  " + sql;
            int iResult = 0;

            if (oConn == null)
                ResetConnection();
            try
            {
                MySqlCommand myCommand = new MySqlCommand(sql, oConn);
                if (commandType == 0)
                {
                    myCommand.CommandType = CommandType.StoredProcedure;
                    myCommand.Parameters.AddWithValue("", parameters);
                }
                else { myCommand.CommandType = CommandType.Text; }

                myCommand.Connection.Open();
                myCommand.ExecuteNonQuery();
                oConn.Close();
            }
            catch (Exception ex)
            {
                iResult = 0;
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ExecuteNonQuery", ex, cProcessInfo));
            }

            finally
            {
                CloseConnection();
            }
            return iResult;
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
    }

}
