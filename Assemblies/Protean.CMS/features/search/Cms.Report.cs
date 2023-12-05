using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{

    public partial class Cms
    {

        public class Report
        {

            private Cms myWeb;
            private XmlDocument moPageXml; // the actual page, given from the web object
            public XmlElement moReport;
            private dbHelper moDB;

            private string mcModuleName = "Eonic.Report"; // module name


            public Report(ref Cms aWeb)
            {
                aWeb.PerfMon.Log(mcModuleName, "New");
                try
                {
                    myWeb = aWeb;
                    moPageXml = myWeb.moPageXml;
                    moDB = myWeb.moDbHelper;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", bDebug: gbDebug);
                }
            }

            public void close()
            {
                myWeb.PerfMon.Log("mcModuleName", "close");
                string cProcessInfo = "";
                try
                {
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Close", ex, "", cProcessInfo, gbDebug);
                }
            }

            public virtual void apply()
            {
                myWeb.PerfMon.Log(mcModuleName, "apply");

                XmlElement oElmt;
                XmlElement oReport;
                bool bOnArticleDisableReport = false;

                try
                {
                    // Go through the content nodes of type report
                    foreach (XmlElement currentMoReport in moPageXml.SelectNodes("/Page/Contents/Content[@type='Report']"))
                    {
                        moReport = currentMoReport;

                        oElmt = (XmlElement)moReport.SelectSingleNode("OnArticle[node()='DisableReport']");
                        if (oElmt != null)
                            bOnArticleDisableReport = true;

                        if (!(myWeb.moRequest["artid"] != "" & bOnArticleDisableReport))
                        {
                            // Run Reports

                            oReport = addElement(ref moReport, "Report");

                            switch (moReport.SelectSingleNode("Type").InnerText ?? "")
                            {

                                case "Eonic_Stored_Procedure":
                                    {
                                        report_StoredProcedure(ref oReport);
                                        break;
                                    }

                            }

                        }

                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "apply", ex, "", bDebug: gbDebug);
                }

            }

            public void report_StoredProcedure(ref XmlElement oReport)
            {
                myWeb.PerfMon.Log(mcModuleName, "report_StoredProcedure");

                // ***********************************************************************
                // Report :      Stored Procedure
                // Parameters :  Param 1, "sp", is the name of the SP to execute
                // Other parameters are the parameters fed into the SP
                // Description : This can be used to pull back a stored procedure from
                // from the database.
                // For security, the following measures are taken:
                // - only stored procedures can be called.
                // - only stored procedures with the prefix "esp_" can
                // be called.
                // - stored procedures can only be called as read only.
                // ***********************************************************************


                XmlElement oElmt;
                XmlElement oRow;
                // Dim oDr As SqlDataReader
                int nColumn;
                int nColumns;
                string cStoredProcedure = "";

                try
                {

                    // Get the Stored Procedure name
                    oElmt = (XmlElement)moReport.SelectSingleNode("Parameters/Parameter[@name='sp' and node()!='']");
                    if (oElmt != null)
                    {
                        cStoredProcedure = oElmt.InnerText;

                        // Security check : Prefix
                        if (cStoredProcedure.StartsWith("esp_"))
                        {

                            var oParams = new Hashtable();
                            string cValue = "";


                            // Build the other parameters
                            foreach (XmlElement currentOElmt in moReport.SelectNodes("Parameters/Parameter[@name!='sp']"))
                            {
                                oElmt = currentOElmt;
                                if (!string.IsNullOrEmpty(oElmt.GetAttribute("name")))
                                {
                                    oParams.Add(oElmt.GetAttribute("name"), oElmt.InnerText);
                                }
                            }

                            // Normally we would execute the SP as a Dataset, 
                            // but because of the security constraints needed,
                            // we'll run this as a Reader, and convert into Xml
                            // oDr = moDB.getDataReader(cStoredProcedure, CommandType.StoredProcedure, oParams)
                            using (SqlDataReader oDr = moDB.getDataReaderDisposable(cStoredProcedure, CommandType.StoredProcedure, oParams))  // Done by nita on 6/7/22
                            {
                                if (oDr.HasRows)
                                {
                                    nColumns = oDr.FieldCount;


                                    while (oDr.Read())
                                    {
                                        oRow = addElement(ref oReport, "row");
                                        var loopTo = nColumns - 1;
                                        for (nColumn = 0; nColumn <= loopTo; nColumn++)
                                        {
                                            if (!oDr.IsDBNull(nColumn))
                                            {
                                                switch (oDr.GetFieldType(nColumn).ToString() ?? "")
                                                {
                                                    case "SqlDateTime":
                                                        {
                                                            cValue = XmlDate(oDr[nColumn].ToString(), true);
                                                            break;
                                                        }

                                                    default:
                                                        {
                                                            cValue = oDr[nColumn].ToString();
                                                            break;
                                                        }
                                                }
                                            }

                                            // Check if it's xml
                                            bool bAddAsXml = false;
                                            if (oDr.GetName(nColumn).ToLower().Contains("xml"))
                                                bAddAsXml = true;

                                            // Add the data to the row.
                                            addElement(ref oRow, oDr.GetName(nColumn), cValue, bAddAsXml);
                                        }
                                    }
                                }

                                oDr.Close();
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "report_StoredProcedure", ex, "", bDebug: gbDebug);
                }
            }

            public void SetDefaultSortColumn(long nSortColumn, SortDirection nSortDirection = SortDirection.Ascending)
            {
                myWeb.PerfMon.Log("stdTools", "SetDefaultSortColumn");
                try
                {
                    XmlElement oElmt;
                    if (moPageXml.DocumentElement is null)
                    {
                        myWeb.GetPageXML();
                    }

                    if (moPageXml.SelectSingleNode("/Page/Request/QueryString/Item[@name='sortCol']") is null)
                    {
                        // Add a default column sort
                        var argoNode = moPageXml.SelectSingleNode("/Page/Request/QueryString");
                        oElmt = addNewTextNode("Item", ref argoNode, nSortColumn.ToString(), bOverwriteExistingNode: false);
                        oElmt.SetAttribute("name", "sortCol");

                        oElmt = (XmlElement)moPageXml.SelectSingleNode("/Page/Request/QueryString/Item[@name='sortDir']");
                        if (oElmt != null)
                        {
                            oElmt.InnerText = SortDirectionVal[(int)nSortDirection];
                        }
                        else
                        {
                            var argoNode1 = moPageXml.SelectSingleNode("/Page/Request/QueryString");
                            oElmt = addNewTextNode("Item", ref argoNode1, SortDirectionVal[(int)nSortDirection], bOverwriteExistingNode: false);
                            oElmt.SetAttribute("name", "sortDir");
                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, "stdTools", "SetDefaultSortColumn", ex, "", "", gbDebug);
                }
            }

        }
    }
}