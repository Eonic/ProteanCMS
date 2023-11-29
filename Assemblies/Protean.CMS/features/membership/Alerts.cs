using System;
using System.Collections;
using System.Data;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

using static Protean.stdTools;

namespace Protean
{

    public partial class Cms
    {
        public partial class Membership
        {
            public class Alerts
            {

                #region Declarations
                private Cms _myWeb;

                private Cms myWeb
                {
                    [MethodImpl(MethodImplOptions.Synchronized)]
                    get
                    {
                        return _myWeb;
                    }

                    [MethodImpl(MethodImplOptions.Synchronized)]
                    set
                    {
                        _myWeb = value;
                    }
                }

                private AlertMember _oMember;

                private AlertMember oMember
                {
                    [MethodImpl(MethodImplOptions.Synchronized)]
                    get
                    {
                        return _oMember;
                    }

                    [MethodImpl(MethodImplOptions.Synchronized)]
                    set
                    {
                        if (_oMember != null)
                        {
                            _oMember.OnError -= _OnError;
                        }

                        _oMember = value;
                        if (_oMember != null)
                        {
                            _oMember.OnError += _OnError;
                        }
                    }
                }
                private Hashtable oAlertItems = new Hashtable(); // List of Alerts
                private Hashtable oAlertUsers = new Hashtable(); // List of Users
                                                                 // alert config section
                private System.Collections.Specialized.NameValueCollection oAlertConfig = (System.Collections.Specialized.NameValueCollection)System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection("protean/alerts");
                public new event OnErrorEventHandler OnError;

                public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

                public object oNewAlert;

                private void OnAlertError(object sender, Tools.Errors.ErrorEventArgs e)
                {
                    OnError?.Invoke(this, e);
                }

                #endregion


                #region Public Procedures


                public Alerts(Cms aWeb)
                {
                    myWeb = aWeb;
                    // Note we are adding error handling to myWeb, so that it can be picked up by the Service handler
                    myWeb.OnError += this.OnAlertError;
                }

                public XmlElement CurrentAlerts(ref bool bResponse)
                {
                    return CurrentAlerts(ref bResponse, true);
                }

                public XmlElement CurrentAlerts(ref bool bResponse, bool bReportDeep)
                {
                    string cInfo = "";
                    var oResponseXML = new XmlDocument();
                    oResponseXML.AppendChild(oResponseXML.CreateElement("Response"));
                    try
                    {
                        cInfo = "Date";
                        var dTimeNow = DateTime.Now; // Constant Time
                        cInfo = "Build";
                        myWeb.BuildPageXML();
                        // myWeb.moPageXml = New XmlDocument
                        // myWeb.moPageXml.AppendChild(myWeb.moPageXml.CreateElement("Page"))
                        // Gets a list of all the alerts and checks which need to run
                        string cSQL = "SELECT DISTINCT tblAlerts.nAlertKey, tblAlerts.nDirId, tblAlerts.nFrequency, ((SELECT TOP 1 dDateTime FROM tblActivityLog WHERE nOtherId = tblAlerts.nAlertKey ORDER BY dDateTime DESC)) AS dDateTime, tblDirectory.cDirSchema FROM tblAlerts LEFT OUTER JOIN tblDirectory ON tblAlerts.nDirId = tblDirectory.nDirKey LEFT OUTER JOIN tblAudit ON tblAlerts.nAuditId = tblAudit.nAuditKey WHERE (tblAudit.nStatus = 1) AND (tblAudit.dPublishDate <= " + Tools.Database.SqlDate(dTimeNow, true) + " OR tblAudit.dPublishDate IS NULL) AND (tblAudit.dExpireDate >= " + Tools.Database.SqlDate(dTimeNow, true) + " OR tblAudit.dExpireDate IS NULL) AND  (tblAlerts.nAlertParent IS NULL)";
                        cInfo = "Get oDR";
                        using (var oDR = myWeb.moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                        {
                            while (oDR.Read())
                            {
                                cInfo = "Due Next";
                                DateTime dNextDue = default;
                                // work out the date next due
                                cInfo = "Check Date";
                                if (!(oDR[3] is DBNull))
                                    dNextDue = Conversions.ToDate(oDR[3]).AddMinutes(Conversions.ToDouble(oDR[2]));
                                // if its due or not been run then run it
                                // add the alerts/users/groups to the lists
                                cInfo = "Check Due";
                                if (dNextDue == default | dNextDue <= dTimeNow)
                                {
                                    // Add a new alert Item
                                    cInfo = "New Alert";
                                    // oNewAlert = New AlertItem(myWeb, oDR(0), oDR(1))
                                    oNewAlert = CreateAlertItem(Conversions.ToInteger(oDR[0]), Conversions.ToInteger(oDR[1]));

                                    // Log the alert
                                    oNewAlert.Log("Started");

                                    cInfo = "Pop Alert";
                                    oNewAlert.Populate();
                                    cInfo = "Add To AlertItems";
                                    oAlertItems.Add(Operators.ConcatenateObject("A", oDR[0]), oNewAlert);

                                    // Go through the dir item to get the user id
                                    // if its a group we iterate down through the groups
                                    // to get all the users underneath
                                    var oDSUsers = new DataSet();
                                    cInfo = "CheckGroup";
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual((oDR[4]), "group", false)))
                                    {
                                        cInfo = "Is Group";
                                        myWeb.moDbHelper.addTableToDataSet(ref oDSUsers, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("EXEC sp_AllDirUsers ", oDR[1]), ", "), Tools.Database.SqlDate(dTimeNow, true))), "Users");
                                    }
                                    else
                                    {
                                        cInfo = "Is User";
                                        cSQL = "SELECT tblDirectory.nDirKey, tblDirectory.cDirSchema, tblDirectory.cDirForiegnRef, tblDirectory.cDirName, tblDirectory.cDirXml ";
                                        cSQL += " FROM tblDirectory INNER JOIN tblAudit ON tblDirectory.nAuditId = tblAudit.nAuditKey";
                                        cSQL = Conversions.ToString(cSQL + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" WHERE (tblDirectory.nDirKey = ", oDR[1]), ") AND (tblAudit.dPublishDate >= "), Tools.Database.SqlDate(dTimeNow, true)), " OR"));
                                        cSQL += " tblAudit.dPublishDate IS NULL) AND (tblAudit.dExpireDate <= " + Tools.Database.SqlDate(dTimeNow, true) + " OR";
                                        cSQL += " tblAudit.dExpireDate IS NULL)";
                                        myWeb.moDbHelper.addTableToDataSet(ref oDSUsers, cSQL, "Users");
                                    }
                                    foreach (DataRow oUserRow in oDSUsers.Tables["Users"].Rows)
                                    {
                                        // add this to the user list
                                        cInfo = "AddMember";
                                        AddMember(Conversions.ToInteger(oUserRow["nDirKey"]), Conversions.ToString(oUserRow["cDirXml"]), Conversions.ToInteger(oDR[0]));
                                    }
                                }
                            }

                        }


                        // Loop through all the users
                        // so we can send individual emails.
                        // we already have the content required for each alert
                        // so we just need to add that for the user into one email
                        if (bReportDeep)
                        {
                            var oElmt = oResponseXML.CreateElement("Alerts");
                            oElmt.SetAttribute("Count", oAlertItems.Count.ToString());
                            oResponseXML.DocumentElement.AppendChild(oElmt);
                        }
                        if (bReportDeep)
                        {
                            var oElmt = oResponseXML.CreateElement("Members");
                            oElmt.SetAttribute("Count", oAlertUsers.Count.ToString());
                            oResponseXML.DocumentElement.AppendChild(oElmt);
                        }
                        cInfo = "Each User";

                        int nEmailCount = 0;
                        int nEmailSuccess = 0;

                        // Note on the error handling / exception catching.
                        // ================================================
                        // The excessive layers of exception handling here are designed to 
                        // carry on trying with the next users if the e-mailing goes tits up.
                        // What we need to do is hit the point where the Alert is logged, or else  
                        // we risk sending users repeated content.


                        string cStatus = "Alert Done";
                        try
                        {
                            foreach (string cUserKey in oAlertUsers.Keys)
                            {
                                cInfo = "GetMember";
                                AlertMember oMember = (AlertMember)oAlertUsers[cUserKey];
                                cInfo = "Create Alert Elmt";
                                var oAlertElmt = myWeb.moPageXml.CreateElement("Alerts");
                                cInfo = "Set alertElmt " + cUserKey + " xml:" + oMember.MemberXML;
                                oAlertElmt.InnerXml = oMember.MemberXML;
                                string cAlertTitles = "";
                                for (int i = 0, loopTo = oMember.Alerts.Count - 1; i <= loopTo; i++)
                                {
                                    cInfo = "Each MemberAlert";
                                    AlertItem oAlert = (AlertItem)oAlertItems[Operators.ConcatenateObject("A", oMember.Alerts[i])];
                                    cInfo += "1";
                                    if (oAlert.ContentElement != null)
                                        oAlertElmt.AppendChild(oAlert.ContentElement);
                                    cInfo += "2";
                                    if (!string.IsNullOrEmpty(cAlertTitles))
                                        cAlertTitles += ", ";
                                    cInfo += "3";
                                    cAlertTitles += oAlert.AlertTitle;
                                }
                                // Now to send it
                                cInfo = "Send It";
                                if (oAlertElmt.SelectNodes("Contents/Content").Count > 0)
                                {
                                    nEmailCount += 1;
                                    try
                                    {
                                        var oMailer = new Protean.Messaging(ref myWeb.msException);
                                        Cms.dbHelper odbhelper = null;
                                        string cResponse = Conversions.ToString(oMailer.emailer(oAlertElmt, oAlertConfig["AlertXsl"], oAlertConfig["AlertFrom"], oAlertConfig["AlertFromEmail"], oMember.Email, cAlertTitles,ref odbhelper, cPickupHost: oAlertConfig["AlertPickupHost"], cPickupLocation: oAlertConfig["AlertPickupLocation"]));
                                        myWeb.msException = ""; // Clear the sodding error
                                        if (bReportDeep)
                                        {
                                            var oElmt = oResponseXML.CreateElement("Email");
                                            oElmt.SetAttribute("Address", oMember.Email);
                                            oElmt.InnerText = cResponse + " (" + oMember.Email + ")";
                                            oResponseXML.DocumentElement.AppendChild(oElmt);
                                        }
                                        if (cResponse.Contains("Message Sent"))
                                        {
                                            // success
                                            nEmailSuccess += 1;
                                            cInfo = "Sent Fine";
                                        }
                                        else
                                        {
                                            // failed
                                            cInfo = "Not Sent";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(Membership.mcModuleName, "CurrentAlerts", ex, cInfo + ". Problem sending email"));
                                        cStatus = "ErrorEncountered";
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(Membership.mcModuleName, "CurrentAlerts", ex, cInfo + ". Problem iterating through users"));
                            cStatus = "ErrorEncountered";

                        }


                        if (nEmailCount > 0 & nEmailCount != nEmailSuccess)
                        {

                            // Emails have been sent but not all were successful
                            bResponse = false;

                            if (!cStatus.Contains("Error"))
                                cStatus = "Not All Sent";

                            // Add a response.
                            string cMessage = "Alert sending was incomplete due to problems sending e-mail (" + nEmailSuccess + " out of " + nEmailCount + " email(s) were sent successfully).";
                            var argoParent = oResponseXML.DocumentElement;
                            Tools.Xml.addElement(ref argoParent, "Message", cMessage, bPrepend: true);

                            // Raise an Error
                            OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(Membership.mcModuleName, "CurrentAlerts", new Exception(cMessage), cInfo));
                        }

                        else
                        {
                            bResponse = true;
                        }


                        // Log the alerts
                        foreach (string cKey in oAlertItems.Keys)
                        {
                            cInfo = "Mark Done";
                            AlertItem oAlert = (AlertItem)oAlertItems[cKey];
                            oAlert.Log(cStatus);
                        }
                    }

                    catch (Exception ex)
                    {
                        bResponse = false;
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(Membership.mcModuleName, "CurrentAlerts", ex, cInfo));
                    }
                    return oResponseXML.DocumentElement;
                }

                public virtual object CreateAlertItem(int AlertId, int AlertDirId = 0)
                {
                    var argaWeb = myWeb;
                    var oAlert = new AlertItem(ref argaWeb, AlertId, AlertDirId);
                    myWeb = argaWeb;
                    oAlert.OnError += _OnError;
                    return oAlert;
                }

                #endregion



                #region Private Procedures

                private void AddMember(int nUserId, string DirXml, int nAlertId)
                {
                    try
                    {
                        if (!oAlertUsers.ContainsKey("u" + nUserId))
                        {
                            oMember = new AlertMember(nUserId, DirXml);

                            oMember.OnError += _OnError;
                            oMember.AddAlert(nAlertId);
                            oAlertUsers.Add("u" + nUserId, oMember);
                        }
                        else
                        {
                            oMember = (AlertMember)oAlertUsers["u" + nUserId];
                            oMember.AddAlert(nAlertId);
                            oAlertUsers["u" + nUserId] = oMember;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(Membership.mcModuleName, "addMember", ex, ""));
                    }
                }

                protected void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
                {
                    OnError?.Invoke(sender, e);
                }

                #endregion

                public class AlertItem
                {

                    #region Declarations

                    private Cms myWeb;
                    // Private dTimeNow As Date = Now

                    protected internal int nAlertKey;
                    protected internal int nAlertLogKey;
                    protected internal string cAlertTitle;
                    protected internal int nPageId;
                    protected internal string cPageIds;
                    protected internal int nFrequency;
                    protected internal string cContentTypes;
                    protected internal string cXsltFile;
                    protected internal bool bUpdatedOnly;
                    protected internal bool bItterateDown;
                    protected internal bool bRelatedContentUpdates;
                    protected internal string cExtraXml;
                    protected internal XmlElement oExtraXML;
                    protected internal DateTime dLastDone;
                    protected internal XmlElement oContentElmt;
                    protected internal bool bTransformed;
                    protected internal int nAlertDirId;

                    private const string mcModuleName = "Protean.Cms.Membership.Alerts.AlertItem";

                    public event OnErrorEventHandler OnError;

                    public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

                    #endregion

                    #region Properties

                    private int AlertLogKey
                    {
                        get
                        {
                            return nAlertLogKey;
                        }
                        set
                        {
                            nAlertLogKey = value;
                        }
                    }

                    public int AlertKey
                    {
                        get
                        {
                            return nAlertKey;
                        }
                    }

                    public string AlertTitle
                    {
                        get
                        {
                            return cAlertTitle;
                        }
                    }


                    public int PageId
                    {
                        get
                        {
                            return nPageId;
                        }
                    }

                    public string PageIds
                    {
                        get
                        {
                            return cPageIds;
                        }
                    }

                    public int FrequencyMinutes
                    {
                        get
                        {
                            return nFrequency;
                        }
                    }

                    public DateTime LastDone
                    {
                        get
                        {
                            return dLastDone;
                        }
                    }

                    public DateTime NextDue
                    {
                        get
                        {
                            return dLastDone.AddMinutes(nFrequency);
                        }
                    }

                    public string ContentTypes
                    {
                        get
                        {
                            return cContentTypes;
                        }
                    }

                    public string XsltFile
                    {
                        get
                        {
                            return cXsltFile;
                        }
                    }

                    public bool UpdatedOnly
                    {
                        get
                        {
                            return bUpdatedOnly;
                        }
                    }

                    public bool ItterateDown
                    {
                        get
                        {
                            return bItterateDown;
                        }
                    }

                    public bool RelatedContentUpdates
                    {
                        get
                        {
                            return bRelatedContentUpdates;
                        }
                    }

                    public XmlElement ExtraXml
                    {
                        get
                        {
                            if (string.IsNullOrEmpty(cExtraXml))
                                return null;
                            if (oExtraXML is null)
                            {
                                var oXML = new XmlDocument();
                                oXML.LoadXml(cExtraXml);
                                oExtraXML = oXML.DocumentElement;
                            }
                            return oExtraXML;
                        }
                    }

                    public virtual XmlElement ContentElement
                    {
                        get
                        {
                            if (oContentElmt.HasChildNodes)
                            {
                                return oContentElmt;
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }

                    public bool Transformed
                    {
                        get
                        {
                            return bTransformed;
                        }
                    }

                    private int AlertDirId
                    {
                        get
                        {
                            return nAlertDirId;
                        }
                        set
                        {
                            nAlertDirId = value;
                        }
                    }

                    #endregion

                    #region Public Procedures
                    public AlertItem(ref Cms aWeb, int AlertId, int AlertDirId = 0)
                    {
                        myWeb = aWeb;
                        nAlertKey = AlertId;
                        this.AlertDirId = AlertDirId;
                    }

                    public void Log(string cStatus = "No Status")
                    {
                        string cSql = "";

                        try
                        {
                            // Test if this has been logged
                            if (Information.IsNumeric(AlertLogKey) && AlertLogKey > 0)
                            {
                                // Alert Item has been logged, therefore update the record

                                cSql = "UPDATE tblActivityLog ";
                                cSql = Conversions.ToString(cSql + Operators.ConcatenateObject(Operators.ConcatenateObject("SET cActivityDetail = '", SqlFmt(Strings.Left(cStatus, 800))), "' "));
                                cSql += "WHERE nActivityKey = " + AlertLogKey;
                                myWeb.moDbHelper.ExeProcessSql(cSql);
                            }
                            else
                            {
                                // Alert Item has been not been logged, therefore insert the record

                                cSql = "INSERT INTO tblActivityLog ( nUserDirId, nStructId, nArtId, nOtherId, dDateTime, nActivityType, cActivityDetail, cSessionId) VALUES (";
                                cSql += "1,";
                                cSql += "0,";
                                cSql += "0,";
                                cSql += nAlertKey + ",";
                                cSql += Tools.Database.SqlDate(DateTime.Now, true) + ",";
                                cSql += ((int)Cms.dbHelper.ActivityType.Alert).ToString() + ",";
                                cSql = Conversions.ToString(cSql + Operators.ConcatenateObject(Operators.ConcatenateObject("'", SqlFmt(Strings.Left(cStatus, 800))), "',"));
                                cSql += "'')";

                                // Changed this from ExeProcessSqlOrIgnore to ExeProcessSql as for user alerts
                                // this step is essential, and should raise an error if it fails.
                                AlertLogKey = Conversions.ToInteger(myWeb.moDbHelper.GetIdInsertSql(cSql));
                            }
                        }

                        catch (Exception ex)
                        {
                            OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Log", ex, cSql));
                        }
                    }


                    public virtual void Populate()
                    {
                        try
                        {
                            string cSQL = "SELECT * FROM tblAlerts WHERE nAlertKey = " + nAlertKey + " OR nAlertParent = " + nAlertKey;
                            var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "tblAlerts");

                            foreach (DataRow oRow in oDS.Tables["tblAlerts"].Rows)
                            {
                                if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(CellValue(oRow["cAlertTitle"], ""), "", false)))
                                    cAlertTitle = Conversions.ToString(oRow["cAlertTitle"]);
                                if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(CellValue(oRow["nPageId"], 0), 0, false)))
                                    nPageId = Conversions.ToInteger(oRow["nPageId"]);
                                if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(CellValue(oRow["nFrequency"], 0), 0, false)))
                                    nFrequency = Conversions.ToInteger(oRow["nFrequency"]);
                                if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(CellValue(oRow["cXsltFile"], ""), "", false)))
                                    cXsltFile = Conversions.ToString(oRow["cXsltFile"]);

                                bUpdatedOnly = Conversions.ToBoolean(CellValue(oRow["bUpdatedOnly"], false));
                                bItterateDown = Conversions.ToBoolean(CellValue(oRow["bItterateDown"], false));
                                bRelatedContentUpdates = Conversions.ToBoolean(CellValue(oRow["bRelatedContentUpdates"], false));
                                if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(CellValue(oRow["cExtraXml"], ""), "", false)))
                                    cAlertTitle = Conversions.ToString(oRow["cExtraXml"]);

                                if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(CellValue(oRow["cContentType"], ""), "", false)))
                                {
                                    if (!string.IsNullOrEmpty(cContentTypes))
                                        cContentTypes += ",";
                                    cContentTypes = Conversions.ToString(cContentTypes + Operators.ConcatenateObject(Operators.ConcatenateObject("'", oRow["cContentType"]), "'"));
                                }
                                string cLastDone = Conversions.ToString(myWeb.moDbHelper.GetDataValue("SELECT TOP 1 dDateTime FROM tblActivityLog WHERE (nActivityType = " + ((int)Cms.dbHelper.ActivityType.Alert).ToString() + ") AND (nOtherId = " + nAlertKey + ") AND nActivityKey <> " + AlertLogKey + " ORDER BY dDateTime DESC", CommandType.Text, null, ""));
                                if (Information.IsDate(cLastDone))
                                    dLastDone = Conversions.ToDate(cLastDone);
                                else
                                    dLastDone = DateTime.Now;
                                // PopulateContent(nPageId, Nothing)
                            }
                            if (!string.IsNullOrEmpty(cContentTypes))
                            {
                                PopulateContent(nPageId, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Populate", ex, ""));
                        }
                    }

                    public virtual void PopulateContent(int nCurrentPageId, XmlElement oCurrElmt)
                    {
                        try
                        {
                            string cWhere = " (cContentSchemaName IN (" + cContentTypes + "))";
                            // Only updatead?
                            if (bUpdatedOnly)
                                cWhere += " AND (dInsertDate >= " + Tools.Database.SqlDate(dLastDone, true) + "  OR dUpdateDate>= " + Tools.Database.SqlDate(dLastDone, true) + ")";
                            // Get The Root Item Content
                            cWhere += " AND CL.nStructId = " + nCurrentPageId;
                            // Add it to the Content
                            if (oContentElmt is null)
                            {
                                oContentElmt = myWeb.moPageXml.CreateElement("Contents");
                            }
                            oContentElmt.SetAttribute("title", cAlertTitle);
                            XmlElement oCont = (XmlElement)myWeb.moPageXml.DocumentElement.SelectSingleNode("Contents");
                            if (oCont != null)
                                oCont.InnerXml = "";
                            int argnCount = 0;
                            XmlElement argoContentsNode = null;
                            XmlElement argoPageDetail = null;
                            myWeb.GetPageContentFromSelect(cWhere, nCount: ref argnCount, oContentsNode: ref argoContentsNode, oPageDetail: ref argoPageDetail);
                            oCont = (XmlElement)myWeb.moPageXml.DocumentElement.SelectSingleNode("Contents");
                            oContentElmt.InnerXml += oCont.InnerXml;
                            // Child Items
                            if (bItterateDown)
                            {
                                if (oCurrElmt is null)
                                {
                                    var oMainElmt = myWeb.GetStructureXML();
                                    oCurrElmt = (XmlElement)oCurrElmt.SelectSingleNode("descendant-or-self::MenuItem[@id=" + nCurrentPageId + "]");
                                }
                                foreach (XmlElement oChild in oCurrElmt.SelectNodes("MenuItem"))
                                    PopulateContent(Conversions.ToInteger(oChild.GetAttribute("id")), oChild);
                            }
                        }
                        catch (Exception ex)
                        {
                            OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Populatecontent", ex, ""));
                        }
                    }
                    #endregion

                    #region Private Procedures


                    private object CellValue(object obj, object nullValue)
                    {
                        if (obj is DBNull)
                            return nullValue;
                        else
                            return obj;
                    }


                    #endregion

                }

                private class AlertMember
                {
                    #region Declarations
                    private int nUserId;
                    private string cEmail;
                    private Hashtable oAlerts = new Hashtable();
                    private string cDirXML;
                    private const string mcModuleName = "Protean.Cms.Membership.Alerts.Member";

                    public event OnErrorEventHandler OnError;

                    public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                    #endregion

                    #region Properties
                    public string Email
                    {
                        get
                        {
                            return cEmail;
                        }
                    }

                    public Hashtable Alerts
                    {
                        get
                        {
                            return oAlerts;
                        }
                    }

                    public string MemberXML
                    {
                        get
                        {
                            return cDirXML;
                        }
                    }
                    #endregion

                    public AlertMember(int UserId, string DirXml)
                    {
                        nUserId = UserId;
                        cDirXML = DirXml;
                        GetEmail();
                    }

                    public void AddAlert(int nAlertId)
                    {
                        try
                        {
                            if (!oAlerts.ContainsValue(nAlertId))
                            {
                                oAlerts.Add(oAlerts.Count, nAlertId);
                            }
                        }
                        catch (Exception ex)
                        {
                            OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddAlert", ex, ""));
                        }
                    }

                    private void GetEmail()
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cDirXML))
                                return;
                            var oXML = new XmlDocument();
                            oXML.LoadXml(cDirXML);
                            XmlElement oEmailelmt = (XmlElement)oXML.SelectSingleNode("descendant-or-self::Email");
                            if (oEmailelmt != null)
                                cEmail = oEmailelmt.InnerText;
                            oEmailelmt = null;
                            oXML = null;
                        }
                        catch (Exception ex)
                        {
                            OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetEmail", ex, ""));
                        }
                    }
                }


            }
        }
    }
}