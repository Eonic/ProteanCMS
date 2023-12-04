using System;
using System.Diagnostics;
using System.Text;
using System.Web.Configuration;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;

namespace Protean
{

    public class PerfLog
    {

        public string cSiteName;
        public string cDataConn = "";

        private bool bLoggingOn;
        private int nStep;
        private StringBuilder oBuilder;
        private PerformanceCounter oPerfMonRequests;
        private string[] Entries;
        private DateTime dLast = DateTime.Now;
        private double nTimeAccumalative = 0d;
        private int nMemLast = 0;
        private int nProcLast = 0;
        private string LatestLog = "";


        // Counters
        private PerformanceCounter _workingSetPrivateMemoryCounter;
        private PerformanceCounter _workingSetMemoryCounter;


        private System.Web.HttpContext moCtx = System.Web.HttpContext.Current;

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

        public PerfLog(string SiteName)
        {
            try
            {
                cSiteName = SiteName;

                if (moCtx != null)
                {
                    moRequest = moCtx.Request;
                    moResponse = moCtx.Response;
                    if (moCtx.Session != null)
                        moSession = moCtx.Session;
                    moServer = moCtx.Server;
                }


                if (moSession != null)
                {
                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(moSession["Logging"], "On", false)))
                    {
                        TurnOn();
                    }
                }
            }
            catch (Exception ex)
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
                    Entries = new string[1001];
                    bLoggingOn = true;
                    nStep = 0;
                    moSession["Logging"] = "On";
                    try
                    {
                    }
                    // oPerfMonRequests = New System.Diagnostics.PerformanceCounter("ASP.NET v4.0.30319", "Requests Current")
                    catch (Exception ex)
                    {
                        // do nothing
                    }

                    string cSessionRequest = Conversions.ToString(moSession["SessionRequest"]);
                    if (Information.IsNumeric(cSessionRequest))
                    {
                        moSession["SessionRequest"] = Conversions.ToInteger(cSessionRequest) + 1;
                        dLast = DateTime.Now;
                        nTimeAccumalative = 0d;
                        nMemLast = 0;
                        nProcLast = 0;
                    }
                    else
                    {
                        moSession["SessionRequest"] = 0;
                        dLast = DateTime.Now;
                        nTimeAccumalative = 0d;
                        nMemLast = 0;
                        nProcLast = 0;
                    }

                    _workingSetPrivateMemoryCounter = new PerformanceCounter("Process", "Working Set - Private", Process.GetCurrentProcess().ProcessName);
                    _workingSetMemoryCounter = new PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess().ProcessName);

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
        }

        public void Log(string cModuleName, string cProcessName, string cDescription = "")
        {
            // If Not bLoggingOn Then Exit Sub
            try
            {

                // TS moved to run regardless as this seems to improve peformance if you call these values.
                if (bLoggingOn)
                {
                    var oLN = DateTime.Now - dLast;
                    nTimeAccumalative += oLN.TotalMilliseconds;

                    long memoryPrivate;
                    if (_workingSetPrivateMemoryCounter is null)
                    {
                        memoryPrivate = 0L;
                    }
                    else
                    {
                        memoryPrivate = (long)Math.Round(_workingSetPrivateMemoryCounter.NextValue());
                    }

                    nMemLast = (int)memoryPrivate;
                    nProcLast = Process.GetCurrentProcess().PrivilegedProcessorTime.Milliseconds;

                    long nMemDif = memoryPrivate - nMemLast;
                    long nProcDif = Process.GetCurrentProcess().PrivilegedProcessorTime.Milliseconds - nProcLast;
                    long nMemoryCounterNextVal = default;
                    if (_workingSetMemoryCounter != null)
                    {
                        nMemoryCounterNextVal = (long)Math.Round(_workingSetMemoryCounter.NextValue());
                    }

                    // If bLoggingOn Then

                    string cEntryFull = "INSERT INTO tblPerfMon" + " ( MachineName, Website, SessionID, SessionRequest, Path, [Module], [Procedure],Description, Step, [Time],TimeAccumalative, Requests, PrivateMemorySize64, PrivilegedProcessorTimeMilliseconds)" + " VALUES(";
                    cEntryFull += "'";
                    cEntryFull += moServer.MachineName + "','";
                    cEntryFull += cSiteName + "','";
                    if (moSession.SessionID != null)
                    {
                        try
                        {
                            cEntryFull += moSession.SessionID + "" + "','";
                            cEntryFull += Conversions.ToString(Operators.ConcatenateObject(moSession["SessionRequest"], "")) + "','";
                        }
                        catch (Exception)
                        {
                            cEntryFull += "','','";
                        }
                    }
                    else
                    {
                        cEntryFull += "','','";
                    }

                    // If moSession.SessionID Is Nothing Then

                    // Else
                    // cEntryFull &= CStr(moSession.SessionID & "") & "','"
                    // cEntryFull &= CStr(moSession("SessionRequest") & "") & "','"
                    // End If
                    string cPath = "";
                    if (System.Web.HttpContext.Current != null)
                    {
                        if (System.Web.HttpContext.Current.Request != null)
                        {
                            cPath = System.Web.HttpContext.Current.Request["path"];
                        }
                    }

                    cEntryFull = Conversions.ToString(cEntryFull + Operators.ConcatenateObject(SqlFmt(cPath), "','"));
                    cEntryFull = Conversions.ToString(cEntryFull + Operators.ConcatenateObject(SqlFmt(cModuleName), "','"));
                    cEntryFull += Strings.Left(Conversions.ToString(SqlFmt(cProcessName)), 254) + "','";
                    cEntryFull += Strings.Left(Conversions.ToString(SqlFmt(cDescription)), 3999) + "',";
                    cEntryFull += nStep + ",";
                    cEntryFull += oLN.TotalMilliseconds + ",";
                    cEntryFull += nTimeAccumalative + ",";
                    if (oPerfMonRequests is null)
                    {
                        cEntryFull += "null,'";
                    }
                    else
                    {
                        cEntryFull += oPerfMonRequests.RawValue + ",'";
                    }

                    cEntryFull += nMemLast + "','";
                    if (_workingSetMemoryCounter is null)
                    {
                        cEntryFull += "";
                    }
                    else
                    {
                        cEntryFull += ((long)Math.Round(_workingSetMemoryCounter.NextValue())).ToString();
                    }
                    // cEntryFull &= Process.GetCurrentProcess.PrivateMemorySize64 & "','"
                    // cEntryFull &= Process.GetCurrentProcess.WorkingSet64
                    cEntryFull += "')";
                    nStep += 1;

                    if (nStep > 128)
                    {
                        //string test = "text";
                    }

                    // ReDim Preserve Entries(nStep)
                    Entries[nStep - 1] = cEntryFull;

                    // nMemLast = Process.GetCurrentProcess.PrivateMemorySize64
                    // nProcLast = Process.GetCurrentProcess.PrivilegedProcessorTime.Milliseconds
                    dLast = DateTime.Now;
                }

                // Else

                // Dim nMemDif As Long = Process.GetCurrentProcess.WorkingSet64
                // Dim nMemTotal As Long = Process.GetCurrentProcess.PrivateMemorySize64
                else
                {
                    // TS this is to be viewed in a memory dump to see how far the CMS object has proceeded.
                    LatestLog = cModuleName + "-" + cProcessName + "-" + cDescription;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void Write()
        {
            // If Not bLoggingOn Then Exit Sub
            string cProcessInfo = null;
            try
            {
                if (bLoggingOn)
                {
                    string ConStr = ""; // moConfig("PerfMonConnection")
                    if (string.IsNullOrEmpty(ConStr))
                    {
                        string dbAuth;
                        if (!string.IsNullOrEmpty(moConfig["DatabasePassword"]))
                        {
                            dbAuth = "user id=" + moConfig["DatabaseUsername"] + "; password=" + moConfig["DatabasePassword"];
                        }
                        else if (!string.IsNullOrEmpty(moConfig["DatabaseAuth"]))
                        {
                            dbAuth = moConfig["DatabaseAuth"];
                        }
                        else
                        {
                            dbAuth = "Integrated Security=SSPI;";
                        }
                        ConStr = "Data Source=" + moConfig["DatabaseServer"] + "; " + "Initial Catalog=" + moConfig["DatabaseName"] + "; " + dbAuth;
                    }
                    var oCon = new System.Data.SqlClient.SqlConnection(ConStr);
                    var oCmd = new System.Data.SqlClient.SqlCommand();
                    oCmd.Connection = oCon;
                    oCon.Open();
                    int i;
                    var loopTo = Information.UBound(Entries);
                    for (i = 0; i <= loopTo; i++)
                    {
                        if (!string.IsNullOrEmpty(Entries[i]))
                        {
                            cProcessInfo = Entries[i];
                            oCmd.CommandText = Entries[i];
                            try
                            {
                                oCmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                cProcessInfo = oCmd.CommandText;
                            }
                        }
                    }
                    oCmd.Dispose();
                    oCon.Close();
                    oCon.Dispose();
                    oCon = null;
                    bLoggingOn = false;
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine(cProcessInfo + " - errormsg - " + ex.ToString());
            }
            finally
            {
                Entries = null;
            }
        }

    }
}