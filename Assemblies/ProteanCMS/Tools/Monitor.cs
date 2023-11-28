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
        public new event OnErrorEventHandler OnError;

        public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

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
                var oMsg = new Messaging(myWeb.msException);

                if (oSchedulerConfig is not null)
                {
                    cMonitorEmail = oSchedulerConfig["SchedulerMonitorEmail"];
                    if (cMonitorEmail is null)
                        cMonitorXsl = "alerts@eonic.co.uk";
                    cMonitorXsl = oSchedulerConfig["SchedulerMonitorXsl"];
                    if (cMonitorXsl is null)
                        cMonitorXsl = "/ewcommon/xsl/Tools/schedulermonitor.xsl";
                    oMonitorXml = GetMonitorSchedulerXml();
                    cResponse = oMsg.emailer(oMonitorXml, cMonitorXsl, "EonicWebV5", "eonicwebV5@eonic.co.uk", cMonitorEmail, "");
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
            string cUrl = "";

            string cConStr;
            var oDBh = new dbHelper(myWeb);
            DataSet oDS;
            var oMXML = new XmlDataDocument();
            var oElmt = oMXML.CreateElement("NoData");
            System.Collections.Specialized.NameValueCollection oSchedulerConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/scheduler");

            try
            {

                if (oSchedulerConfig is not null)
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
                    oDBh.ReturnNullsEmpty(oDS);
                    oMXML = new XmlDataDocument(oDS);
                    if (oMXML is not null && oMXML.DocumentElement is not null)
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

            if (oSchedulerConfig is not null)
            {
                oElmt.SetAttribute("DatabaseServer", oSchedulerConfig["DatabaseServer"]);
                oElmt.SetAttribute("Database", oSchedulerConfig["DatabaseName"]);
            }

            return oElmt;


        }

        #endregion

    }
}