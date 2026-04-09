using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using static Protean.stdTools;

namespace Protean
{

    public class PerfLog : IDisposable
    {

        public string cSiteName;
        public string cDataConn = "";

        private bool bLoggingOn;
        private int nStep;
        private PerformanceCounter oPerfMonRequests;
        private List<string> Entries;
        private DateTime dLast = DateTime.Now;
        private double nTimeAccumalative = 0d;
        private int nMemLast = 0;
        private int nProcLast = 0;
        private string LatestLog = "";

        // Shared performance counters (singleton pattern for efficiency)
        private static readonly Lazy<PerformanceCounter> _sharedWorkingSetPrivateMemoryCounter = 
            new Lazy<PerformanceCounter>(() => 
            {
                try
                {
                    return new PerformanceCounter("Process", "Working Set - Private", Process.GetCurrentProcess().ProcessName);
                }
                catch
                {
                    return null;
                }
            });

        private static readonly Lazy<PerformanceCounter> _sharedWorkingSetMemoryCounter = 
            new Lazy<PerformanceCounter>(() => 
            {
                try
                {
                    return new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName);
                }
                catch
                {
                    return null;
                }
            });

        // Instance references to shared counters
        private PerformanceCounter _workingSetPrivateMemoryCounter;
        private PerformanceCounter _workingSetMemoryCounter;

        // Cached SQL insert prefix
        private static readonly string InsertPrefix = 
            "INSERT INTO tblPerfMon ( MachineName, Website, SessionID, SessionRequest, Path, [Module], [Procedure], Description, Step, [Time], TimeAccumalative, Requests, PrivateMemorySize64, PrivilegedProcessorTimeMilliseconds) VALUES(";

        // StringBuilder pool for better memory reuse
        private StringBuilder _stringBuilder;

        // Disposal flag
        private bool _disposed = false;

        private System.Web.HttpContext moCtx;

        // Session / Request Level Properties
        public System.Web.HttpRequest moRequest;
        public System.Web.HttpResponse moResponse;
        public System.Web.SessionState.HttpSessionState moSession;
        public System.Web.HttpServerUtility moServer;

        public System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");




        public bool Logging
        {
            get
            {
                return bLoggingOn;
            }
            set
            {
                bLoggingOn = value;
            }
        }

        public PerfLog(string SiteName, System.Web.HttpContext oCtx)
        {
            try
            {
                cSiteName = SiteName;
                moCtx = oCtx;

                if (moSession != null)
                {
                    if (moSession["Logging"]?.ToString() == "On")
                    {
                        TurnOn();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void TurnOn()
        {
            // If bLoggingOn Then Exit Sub
            try
            {
                if (!bLoggingOn)
                {
                    if (moCtx != null)
                    {
                        moRequest = moCtx.Request;
                        moResponse = moCtx.Response;
                        if (moCtx.Session != null)
                            moSession = moCtx.Session;
                        moServer = moCtx.Server;
                    }

                    Entries = new List<string>(64); // Start with reasonable capacity
                    _stringBuilder = new StringBuilder(512); // Reusable StringBuilder
                    bLoggingOn = true;
                    nStep = 0;
                    moSession["Logging"] = "On";

                    string cSessionRequest = Convert.ToString(moSession["SessionRequest"]);
                    if (Tools.Number.IsNumeric(cSessionRequest))
                    {
                        moSession["SessionRequest"] = Convert.ToInt16(cSessionRequest) + 1;
                    }
                    else
                    {
                        moSession["SessionRequest"] = 0;
                    }
                    
                    dLast = DateTime.Now;
                    nTimeAccumalative = 0d;
                    nMemLast = 0;
                    nProcLast = 0;

                    // Use shared static counters instead of creating new instances
                    _workingSetPrivateMemoryCounter = _sharedWorkingSetPrivateMemoryCounter.Value;
                    _workingSetMemoryCounter = _sharedWorkingSetMemoryCounter.Value;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void Start()
        {
            TurnOn();
        }

        public void Stop()
        {
            bLoggingOn = false;
            moSession["Logging"] = "Off";
            Dispose();
        }

        public void Log(string cModuleName, string cProcessName, string cDescription = "")
        {
            if (!bLoggingOn)
            {
                // Still update LatestLog for memory dumps even when not logging
                LatestLog = $"{cModuleName}-{cProcessName}-{cDescription}";
                return;
            }

            try
            {
                var oLN = DateTime.Now - dLast;
                nTimeAccumalative += oLN.TotalMilliseconds;

                long memoryPrivate = _workingSetPrivateMemoryCounter != null 
                    ? (long)Math.Round(_workingSetPrivateMemoryCounter.NextValue()) 
                    : 0L;

                nMemLast = (int)memoryPrivate;
                nProcLast = Process.GetCurrentProcess().PrivilegedProcessorTime.Milliseconds;

                // Clear and reuse StringBuilder
                _stringBuilder.Clear();
                _stringBuilder.Append(InsertPrefix)
                    .Append('\'').Append(moServer?.MachineName ?? "").Append("','");
                _stringBuilder.Append(cSiteName).Append("','");

                if (moSession?.SessionID != null)
                {
                    _stringBuilder.Append(moSession.SessionID).Append("','");
                    _stringBuilder.Append(moSession["SessionRequest"]?.ToString() ?? "").Append("','");
                }
                else
                {
                    _stringBuilder.Append("','").Append("','");
                }

                string cPath = moCtx?.Request?["Path"] ?? "";

                _stringBuilder.Append(SqlFmt(cPath)).Append("','");
                _stringBuilder.Append(SqlFmt(cModuleName)).Append("','");
                _stringBuilder.Append(TruncateSqlFmt(cProcessName, 254)).Append("','");
                _stringBuilder.Append(TruncateSqlFmt(cDescription, 3999)).Append("',");
                _stringBuilder.Append(nStep).Append(',');
                _stringBuilder.Append(oLN.TotalMilliseconds).Append(',');
                _stringBuilder.Append(nTimeAccumalative).Append(',');
                _stringBuilder.Append(oPerfMonRequests?.RawValue.ToString() ?? "null").Append(",'");
                _stringBuilder.Append(nMemLast).Append("','");
                
                if (_workingSetMemoryCounter != null)
                {
                    _stringBuilder.Append(((long)Math.Round(_workingSetMemoryCounter.NextValue())).ToString());
                }
                
                _stringBuilder.Append("')");

                Entries.Add(_stringBuilder.ToString());
                nStep++;
                dLast = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PerfLog.Log Error: {ex}");
            }
        }

        /// <summary>
        /// Helper method to truncate SqlFmt results efficiently
        /// </summary>
        private static string TruncateSqlFmt(string value, int maxLength)
        {
            string formatted = SqlFmt(value).ToString();
            return formatted.Length <= maxLength ? formatted : formatted.Substring(0, maxLength);
        }

        public void Write()
        {
            if (!bLoggingOn || Entries == null || Entries.Count == 0) return;
            
            string cProcessInfo = null;
            
            try
            {
                string ConStr = BuildConnectionString();
                
                using (var oCon = new System.Data.SqlClient.SqlConnection(ConStr))
                using (var oCmd = oCon.CreateCommand())
                {
                    oCon.Open();
                    
                    // Batch inserts in groups to avoid SQL Server command limits
                    const int batchSize = 100;
                    var validEntries = Entries.Where(e => !string.IsNullOrEmpty(e)).ToList();
                    
                    for (int i = 0; i < validEntries.Count; i += batchSize)
                    {
                        var batch = validEntries.Skip(i).Take(batchSize);
                        var batchSql = string.Join(";", batch);
                        
                        cProcessInfo = $"Batch {i / batchSize + 1}";
                        oCmd.CommandText = batchSql;
                        oCmd.CommandTimeout = 30; // Explicit timeout
                        
                        try
                        {
                            oCmd.ExecuteNonQuery();
                        }
                        catch (Exception batchEx)
                        {
                            Debug.WriteLine($"PerfLog batch insert failed: {cProcessInfo} - {batchEx}");
                            // Continue with next batch even if this one fails
                        }
                    }
                }
                
                bLoggingOn = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"PerfLog.Write Error: {cProcessInfo} - {ex}");
            }
            finally
            {
                // Clear entries to free memory
                Entries?.Clear();
                Entries = null;
                _stringBuilder = null;
            }
        }

        /// <summary>
        /// Builds the database connection string from config
        /// </summary>
        private string BuildConnectionString()
        {
            string ConStr = moConfig?["PerfMonConnection"];
            if (string.IsNullOrEmpty(ConStr))
            {
                string dbAuth = !string.IsNullOrEmpty(moConfig?["DatabasePassword"])
                    ? $"user id={moConfig["DatabaseUsername"]}; password={moConfig["DatabasePassword"]}"
                    : moConfig?["DatabaseAuth"] ?? "Integrated Security=SSPI;";
                
                ConStr = $"Data Source={moConfig["DatabaseServer"]}; Initial Catalog={moConfig["DatabaseName"]}; {dbAuth}";
            }
            return ConStr;
        }

        #region IDisposable Support
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        // Dispose non-shared Performance Counters only
                        if (oPerfMonRequests != null)
                        {
                            oPerfMonRequests.Dispose();
                            oPerfMonRequests = null;
                        }
                        
                        // DO NOT dispose shared static counters - they are reused across requests
                        // Just null out the references
                        _workingSetPrivateMemoryCounter = null;
                        _workingSetMemoryCounter = null;
                        
                        // Clear list (faster than array clearing)
                        if (Entries != null)
                        {
                            Entries.Clear();
                            Entries = null;
                        }
                        
                        // Clear StringBuilder
                        if (_stringBuilder != null)
                        {
                            _stringBuilder.Clear();
                            _stringBuilder = null;
                        }
                        
                        // Clear string to help GC
                        LatestLog = null;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error disposing PerfLog: {ex}");
                    }
                }
                
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        #endregion

    }
}