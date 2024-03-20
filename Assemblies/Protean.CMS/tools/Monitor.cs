using System;
using System.Data;
using System.Web.Configuration;
using System.Xml;
using static Protean.Cms;

namespace Protean
{

    public class Monitor
    {

        #region Declarations

        private Cms myWeb;
        private string mcModuleName = "Eonic.Monitor";

        #endregion
        #region New Error Handling
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            OnError?.Invoke(sender, e);
        }
        #endregion

        #region   Initialise

        public Monitor(ref Cms aWeb)
        {
            myWeb = aWeb;
        }

        #endregion

        #region   Public Procedures

        public string EmailMonitorScheduler()
        {

            string cProcessInfo = "";
            string cMonitorEmail = "alerts@eonic.co.uk";
            string cMonitorXsl = "/ewcommon/xsl/Tools/schedulermonitor.xsl";
            XmlElement oMonitorXml;
            string cResponse = "Not Sent";
            try
            {

                System.Collections.Specialized.NameValueCollection oSchedulerConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/scheduler");
                var oMsg = new Messaging(ref myWeb.msException);

                if (oSchedulerConfig != null)
                {
                    cMonitorEmail = oSchedulerConfig["SchedulerMonitorEmail"];
                    if (cMonitorEmail is null)
                        cMonitorXsl = "alerts@eonic.co.uk";
                    cMonitorXsl = oSchedulerConfig["SchedulerMonitorXsl"];
                    if (cMonitorXsl is null)
                        cMonitorXsl = "/ewcommon/xsl/Tools/schedulermonitor.xsl";
                    oMonitorXml = GetMonitorSchedulerXml();
                    Cms.dbHelper odbhelper=null;
                    cResponse =Convert.ToString(oMsg.emailer(oMonitorXml, cMonitorXsl, "EonicWebV5", "eonicwebV5@eonic.co.uk", cMonitorEmail, "", ref odbhelper));
                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "EmailMonitorScheduler", ex, cProcessInfo));
            }

            return cResponse;
        }

        public XmlElement GetMonitorSchedulerXml()
        {

            string cProcessInfo = "";
            string cUrl = string.Empty;

            string cConStr;
            var oDBh = new dbHelper(ref myWeb);
            DataSet oDS;
            XmlDocument oMXML = new XmlDocument();
            var oElmt = oMXML.CreateElement("NoData");
            System.Collections.Specialized.NameValueCollection oSchedulerConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/scheduler");

            try
            {

                if (oSchedulerConfig != null)
                {

                    // If Scheduler Info is present then
                    cProcessInfo = "Connecting to the Scheduler";

                    cConStr = "Data Source=" + oSchedulerConfig["DatabaseServer"] + "; ";
                    cConStr += "Initial Catalog=" + oSchedulerConfig["DatabaseName"] + "; ";
                    if (!string.IsNullOrEmpty(oSchedulerConfig["DatabaseAuth"]))
                    {
                        cConStr += oSchedulerConfig["DatabaseAuth"];
                    }
                    else
                    {
                        cConStr += "user id=" + oSchedulerConfig["DatabaseUsername"] + ";password=" + oSchedulerConfig["DatabasePassword"] + ";";
                    }

                    oDBh.ResetConnection(cConStr);

                    oDS = oDBh.GetDataSet("EXEC spGetSchedulerSummary @date=" + Tools.Database.SqlDate(DateTime.Now.AddDays(-1), true), "Scan", "Monitor");
                    oDBh.ReturnNullsEmpty(ref oDS);
                    //oMXML = new XmlDataDocument(oDS);
                    if (oDS.Tables[0].Rows.Count>0)
                    {
                        oMXML.LoadXml(oDS.GetXml());
                    }                    
                    if (oMXML != null && oMXML.DocumentElement != null)
                    {
                        oElmt = oMXML.DocumentElement;
                    }
                    oDS.EnforceConstraints = false;

                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetMonitorSchedulerXml", ex, cProcessInfo));
            }
            finally
            {
                oDBh.CloseConnection();
                oDBh = default;
            }

            if (oSchedulerConfig != null)
            {
                oElmt.SetAttribute("DatabaseServer", oSchedulerConfig["DatabaseServer"]);
                oElmt.SetAttribute("Database", oSchedulerConfig["DatabaseName"]);
            }

            return oElmt;


        }

        #endregion

    }
}