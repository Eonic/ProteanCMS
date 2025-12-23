using DocumentFormat.OpenXml.Office.Word;
using Org.BouncyCastle.Crypto.Engines;
using SkiaSharp;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;

namespace Protean.Tools
{
    public partial class Database : IDisposable
    {
        /// <summary>
        /// Asynchronously adds a table to an existing DataSet.
        /// </summary>
        /// <param name="ds">Existing DataSet to add table to</param>
        /// <param name="sql">SQL query to execute</param>
        /// <param name="tablename">Name for the new table</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task addTableToDataSetAsync(
            DataSet ds,
            string sql,
            string tablename,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string cProcessInfo = "tablename:" + tablename + " sql:" + sql;

            try
            {
                using (SqlDataAdapter oDdpt = new SqlDataAdapter(sql, oConn))
                {
                    // Open connection if needed
                    if (oConn.State == ConnectionState.Closed)
                    {
                        await oConn.OpenAsync(cancellationToken).ConfigureAwait(false);
                    }

                    // Fill asynchronously
                    await Task.Run(() =>
                        oDdpt.Fill(ds, tablename),
                        cancellationToken
                    ).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName, "addTableToDataSetAsync", ex, cProcessInfo));
                throw;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Asynchronously executes a non-query command (INSERT, UPDATE, DELETE).
        /// </summary>
        /// <param name="sql">SQL command</param>
        /// <param name="commandtype">Command type</param>
        /// <param name="parameters">Optional parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of rows affected</returns>
        public async Task<int> ExeProcessSqlAsync(
            string sql,
            CommandType commandtype = CommandType.Text,
            Hashtable parameters = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string cProcessInfo = "Running Sql: " + sql;

            try
            {
                using (SqlCommand oCmd = new SqlCommand(sql, oConn))
                {
                    // Set the command type
                    oCmd.CommandType = commandtype;

                    // Set the command timeout
                    if (nConnectTimeout > 15)
                    {
                        oCmd.CommandTimeout = 1800;
                    }

                    // Set the Parameters if any
                    if (parameters != null)
                    {
                        foreach (DictionaryEntry oEntry in parameters)
                        {
                            oCmd.Parameters.AddWithValue(oEntry.Key.ToString(), oEntry.Value);
                        }
                    }

                    // Open the connection asynchronously
                    if (oConn.State == ConnectionState.Closed)
                    {
                        await oConn.OpenAsync(cancellationToken).ConfigureAwait(false);
                    }

                    int result = await oCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

                    if (oConn.State != ConnectionState.Closed)
                    {
                        oConn.Close();
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName, "ExeProcessSqlAsync", ex, cProcessInfo));
                throw;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Asynchronously returns a SqlDataReader with automatic connection disposal.
        /// This is the ASYNC version - use this for all new code and gradual migration.
        /// </summary>
        /// <param name="sql">SQL query to execute</param>
        /// <param name="commandtype">Command type (Text, StoredProcedure, etc.)</param>
        /// <param name="parameters">Optional parameters</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>Task containing SqlDataReader that must be disposed</returns>
        /// <remarks>
        /// The SqlDataReader MUST be disposed (use 'using' statement).
        /// Connection will automatically close when reader is disposed.
        /// Calling code MUST use 'await' to get the benefits of async.
        /// 
        /// Example usage:
        /// <code>
        /// using (SqlDataReader dr = await db.getDataReaderDisposableAsync(sql).ConfigureAwait(false))
        /// {
        ///     while (await dr.ReadAsync().ConfigureAwait(false))
        ///     {
        ///         // Process row
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public async Task<SqlDataReader> getDataReaderDisposableAsync(
            string sql,
            CommandType commandtype = CommandType.Text,
            Hashtable parameters = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string cProcessInfo = "Running Sql: " + sql;
            SqlConnection oLConn = new SqlConnection(DatabaseConnectionString);

            try
            {
                oLConn.StateChange += _ConnectionState;

                using (SqlCommand oCmd = new SqlCommand(sql, oLConn))
                {
                    // Set the command type
                    oCmd.CommandType = commandtype;

                    // Set command timeout
                    if (nConnectTimeout > 15)
                    {
                        oCmd.CommandTimeout = 1800;
                    }

                    // Set parameters
                    oCmd.Parameters.Clear();
                    if (parameters != null)
                    {
                        foreach (DictionaryEntry oEntry in parameters)
                        {
                            Type thisType = oEntry.GetType();
                            oCmd.Parameters.AddWithValue(oEntry.Key.ToString(), oEntry.Value);
                        }
                    }

                    // Open connection asynchronously - THIS is where the thread is freed
                    if (oLConn.State == ConnectionState.Closed)
                    {
                        await oLConn.OpenAsync(cancellationToken).ConfigureAwait(false);
                    }

                    // Execute reader asynchronously - THIS is where the thread is freed
                    // CommandBehavior.CloseConnection ensures the connection closes when reader is disposed
                    return await oCmd.ExecuteReaderAsync(
                        CommandBehavior.CloseConnection,
                        cancellationToken
                    ).ConfigureAwait(false);
                }
            }
            catch (SqlException ex) when (ex.Number == -2) // SQL timeout
            {
                oLConn.Close();
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName,
                    "getDataReaderDisposableAsync",
                    ex,
                    cProcessInfo + " - Query timeout"));
                ErrorMsg = "Query timeout: " + ex.Message;
                return null;
            }
            catch (OperationCanceledException ex)
            {
                oLConn.Close();
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName,
                    "getDataReaderDisposableAsync",
                    ex,
                    cProcessInfo + " - Operation cancelled"));
                ErrorMsg = "Operation cancelled: " + ex.Message;
                return null;
            }
            catch (Exception ex)
            {
                oLConn.Close();
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName,
                    "getDataReaderDisposableAsync",
                    ex,
                    cProcessInfo));
                ErrorMsg = ex.Message;
                return null;
            }
        }



        /// <summary>
        /// Asynchronously executes a query and processes results with automatic disposal.
        /// Use this for simple data reading operations.
        /// </summary>
        /// <param name="sql">SQL query to execute</param>
        /// <param name="processAction">Async function to process each row (receives SqlDataReader)</param>
        /// <param name="commandtype">Command type</param>
        /// <param name="parameters">Optional parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <example>
        /// await ExecuteReaderAsync("SELECT * FROM Users", 
        ///     async dr => await ProcessUserAsync(dr));
        /// </example>
        public async Task ExecuteReaderAsync(
            string sql,
            Func<SqlDataReader, Task> processAction,
            CommandType commandtype = CommandType.Text,
            Hashtable parameters = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (SqlDataReader oDr = await getDataReaderDisposableAsync(
                    sql, commandtype, parameters, cancellationToken).ConfigureAwait(false))
                {
                    if (oDr != null)
                    {
                        while (await oDr.ReadAsync(cancellationToken).ConfigureAwait(false))
                        {
                            if (processAction != null)
                            {
                                await processAction(oDr).ConfigureAwait(false);
                            }
                        }
                    }
                } // ✅ Automatic disposal
            }
            catch (SqlException ex)
            {
                ErrorMsg = $"SQL Error in ExecuteReaderAsync: {ex.Message}";
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName, "ExecuteReaderAsync", ex, sql));
                throw;
            }
            catch (Exception ex)
            {
                ErrorMsg = $"Error in ExecuteReaderAsync: {ex.Message}";
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName, "ExecuteReaderAsync", ex, sql));
                throw;
            }
        }

        /// <summary>
        /// Asynchronously executes a query that returns a single scalar value.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="sql">SQL query</param>
        /// <param name="defaultValue">Default value if no results</param>
        /// <param name="commandtype">Command type</param>
        /// <param name="parameters">Optional parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>First column of first row, or defaultValue</returns>
        public async Task<T> ExecuteScalarAsync<T>(
            string sql,
            T defaultValue = default(T),
            CommandType commandtype = CommandType.Text,
            Hashtable parameters = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                using (SqlDataReader oDr = await getDataReaderDisposableAsync(
                    sql, commandtype, parameters, cancellationToken).ConfigureAwait(false))
                {
                    if (oDr != null && await oDr.ReadAsync(cancellationToken).ConfigureAwait(false)
                        && !oDr.IsDBNull(0))
                    {
                        return (T)Convert.ChangeType(oDr[0], typeof(T));
                    }
                } // ✅ Automatic disposal

                return defaultValue;
            }
            catch (Exception ex)
            {
                ErrorMsg = $"Error in ExecuteScalarAsync: {ex.Message}";
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName, "ExecuteScalarAsync", ex, sql));
                throw;
            }
        }



        /// <summary>
        /// Asynchronously returns a DataSet from SQL query.
        /// This is the PRIMARY method for page building - use for all new code.
        /// </summary>
        /// <param name="sql">SQL query to execute</param>
        /// <param name="tablename">Name for the DataTable within the DataSet</param>
        /// <param name="datasetname">Optional name for the DataSet itself</param>
        /// <param name="bHandleTimeouts">If true, returns null on timeout instead of throwing exception</param>
        /// <param name="parameters">Optional Hashtable of SQL parameters</param>
        /// <param name="querytype">Command type (Text, StoredProcedure, TableDirect)</param>
        /// <param name="pageSize">For paging - number of rows to return (0 = all rows)</param>
        /// <param name="pageNumber">For paging - page number (1-based indexing)</param>
        /// <param name="cConn">Optional connection string override (if empty, uses default connection)</param>
        /// <param name="cancellationToken">Cancellation token for async operations</param>
        /// <returns>Task containing DataSet, or null on error/timeout</returns>
        /// <remarks>
        /// This method frees the calling thread during database I/O operations.
        /// Use ConfigureAwait(false) when calling from library code.
        /// 
        /// Example usage:
        /// <code>
        /// var ds = await db.GetDataSetAsync(
        ///     "SELECT * FROM tblContent WHERE nContentKey = @id", 
        ///     "Content",
        ///     parameters: new Hashtable { { "@id", contentId } }
        /// ).ConfigureAwait(false);
        /// </code>
        /// </remarks>
        public async Task<DataSet> GetDataSetAsync(
            string sql,
            string tablename,
            string datasetname = "",
            bool bHandleTimeouts = false,
            Hashtable parameters = null,
            CommandType querytype = CommandType.Text,
            int pageSize = 0,
            int pageNumber = 0,
            string cConn = "",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string cProcessInfo = "Running Sql: " + sql;
            DataSet oDs = new DataSet();
            int nStartIndex = (pageSize * pageNumber) - pageSize + 1;

            try
            {
                SqlConnection oConnection = null;

                // Determine which connection to use
                if (!string.IsNullOrEmpty(cConn))
                {
                    oConnection = new SqlConnection(cConn);
                }
                else
                {
                    oConnection = oConn;
                    if (oConnection == null)
                    {
                        ResetConnection();
                        oConnection = oConn;
                    }
                }

                using (SqlDataAdapter oDataAdpt = new SqlDataAdapter(sql, oConnection))
                {
                    // Open connection asynchronously - THIS frees the thread
                    if (oConn.State == ConnectionState.Closed)
                    {
                        await oConn.OpenAsync(cancellationToken).ConfigureAwait(false);
                    }

                    if (oConnection.State == ConnectionState.Closed)
                    {
                        await oConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
                    }

                    // Set DataSet name if provided
                    if (!string.IsNullOrEmpty(datasetname))
                    {
                        oDs.DataSetName = datasetname;
                    }

                    // Configure command
                    oDataAdpt.SelectCommand.CommandType = querytype;

                    // Set timeout
                    if (nConnectTimeout > 15)
                    {
                        oDataAdpt.SelectCommand.CommandTimeout = nConnectTimeout;
                    }

                    // Add parameters if provided
                    if (parameters != null)
                    {
                        foreach (DictionaryEntry oEntry in parameters)
                        {
                            oDataAdpt.SelectCommand.Parameters.AddWithValue(
                                oEntry.Key.ToString(),
                                oEntry.Value);
                        }
                    }

                    // ✅ CRITICAL: Fill adapter asynchronously
                    // Note: SqlDataAdapter.Fill() is NOT truly async in .NET Framework 4.8
                    // We use Task.Run to prevent blocking the calling thread
                    if (pageSize > 0)
                    {
                        // Paged version
                        await Task.Run(() =>
                            oDataAdpt.Fill(oDs, nStartIndex, pageSize, tablename),
                            cancellationToken
                        ).ConfigureAwait(false);
                    }
                    else
                    {
                        // Non-paged version (most common)
                        await Task.Run(() =>
                            oDataAdpt.Fill(oDs, tablename),
                            cancellationToken
                        ).ConfigureAwait(false);
                    }
                }

                return oDs;
            }
            catch (SqlException ex)
            {
                // Handle SQL-specific errors
                if (ex.Message.StartsWith("Timeout expired.") && bHandleTimeouts)
                {
                    // Graceful timeout handling - return null instead of throwing
                    TimeOutException = true;
                    return null;
                }
                else
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                        mcModuleName, "GetDataSetAsync", ex, cProcessInfo));
                    return null;
                }
            }
            catch (OperationCanceledException ex)
            {
                // Handle cancellation gracefully
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName, "GetDataSetAsync", ex,
                    cProcessInfo + " - Operation cancelled"));
                TimeOutException = true; // Treat cancellation similar to timeout
                return null;
            }
            catch (Exception ex)
            {
                // Handle all other errors
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName, "GetDataSetAsync", ex, cProcessInfo));
                return null;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Asynchronously retrieves a Hashtable from a SQL query.
        /// Useful for lookup tables and key-value pairs.
        /// </summary>
        /// <param name="sSql">SQL query</param>
        /// <param name="sNameField">Column name for hash keys</param>
        /// <param name="sValueField">Column name for hash values</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Hashtable of key-value pairs</returns>
        public async Task<Hashtable> getHashTableAsync(
            string sSql,
            string sNameField,
            string sValueField,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Hashtable oHash = new Hashtable();
            string cProcessInfo = "getHashTableAsync";

            try
            {
                using (SqlDataAdapter oDataAdpt = new SqlDataAdapter(sSql, oConn))
                {
                    DataSet oDs = new DataSet();

                    // Fill asynchronously
                    await Task.Run(() =>
                        oDataAdpt.Fill(oDs, "HashPairs"),
                        cancellationToken
                    ).ConfigureAwait(false);

                    // Build hashtable
                    foreach (DataRow oDr in oDs.Tables["HashPairs"].Rows)
                    {
                        oHash.Add(
                            Convert.ToString(oDr[sNameField]),
                            Convert.ToString(oDr[sValueField]));
                    }
                }

                if (oConn.State != ConnectionState.Closed)
                {
                    oConn.Close();
                }

                return oHash;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(
                    mcModuleName, "getHashTableAsync", ex, cProcessInfo));
                return null;
            }
        }

    }

}
