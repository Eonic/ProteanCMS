using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;
using Protean.Tools;
using Protean.Tools.Integration.Twitter;

namespace Protean
{


    public class FeedHandler
    {

        public string cFeedURL; // placeholder values so we dont have to keep parsing them to all the subs
        public string cXSLTransformPath;
        public int nHostPageID;
        public SaveMode nSave;

        public bool bResult = true;
        public XmlElement oResultElmt;
        public XmlElement TotalsElmt;
        public string FeedItemNode;
        public XmlDocument FeedXml;

        public Protean.Cms.dbHelper oDBH;
        public Protean.XmlHelper.Transform oTransform;
        // Public oAdmXFrm As New Cms.Admin.AdminXforms()

        private System.Collections.Specialized.NameValueCollection oConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

        private string[] _countertypes = new[] { "add", "update", "delete", "archive", "total", "notupdated" };
        private CounterCollection _counters;
        private bool _updateExistingItems;


        public enum SaveMode
        {
            Delete = 0,
            Archive = 1,
            Manual = 2
        }


        public FeedHandler(string cURL, string cXSLPath, long nPageId, int nSaveMode, [Optional, DefaultParameterValue(null)] ref XmlElement oResultRecorderElmt, string cItemNodeName = "")
        {
            // PerfMon.Log("FeedHandler", "New")
            try
            {
                oDBH = new Cms.dbHelper("Data Source=" + oConfig["DatabaseServer"] + "; " + "Initial Catalog=" + oConfig["DatabaseName"] + "; " + "user id=" + oConfig["DatabaseUsername"] + "; password=" + oConfig["DatabasePassword"], 1);
                oDBH.ResetConnection("Data Source=" + oConfig["DatabaseServer"] + "; " + "Initial Catalog=" + oConfig["DatabaseName"] + "; " + oDBH.GetDBAuth());
                oDBH.myWeb = new Cms(System.Web.HttpContext.Current);
                oDBH.myWeb.InitializeVariables();
                // oDBH.myWeb.Open()
                var oAdmXFrm = new Cms.Admin.AdminXforms(ref oDBH.myWeb.msException);
                oAdmXFrm.goConfig = oConfig;
                oAdmXFrm.moDbHelper = oDBH;
                oAdmXFrm.myWeb = oDBH.myWeb;

                // set the main values
                cFeedURL = Strings.Replace(cURL, "&amp;", "&"); // when saving a url it can replace ampersands
                cXSLTransformPath = cXSLPath;
                nHostPageID = (int)nPageId;
                nSave = (SaveMode)nSaveMode;
                FeedItemNode = cItemNodeName;
                oResultElmt = oResultRecorderElmt;
                TotalsElmt = Xml.addElement(ref oResultElmt, "Totals");
                _updateExistingItems = true;
                _counters = new CounterCollection();
                InitialiseCounters();
                oTransform = new Protean.XmlHelper.Transform(ref oDBH.myWeb, cXSLTransformPath, false);
            }
            catch (Exception ex)
            {
                AddExternalError(ex);
            }
        }

        public bool UpdateExistingItems
        {
            get
            {
                return _updateExistingItems;
            }
            set
            {
                _updateExistingItems = value;
            }
        }


        private void _OnCounterChange(Counter sender, EventArgs e)
        {

            if (TotalsElmt != null)
            {
                TotalsElmt.SetAttribute(sender.Name, sender.ToInt().ToString());
            }

        }

        private void InitialiseCounters()
        {
            try
            {
                Counter ctr;
                _counters.Clear();
                foreach (string countertype in _countertypes)
                {
                    // ctr = _counters.Add(countertype)
                    ctr = _counters.Add(countertype);
                    ctr.OnChange += _OnCounterChange;
                }
            }
            catch (Exception ex)
            {
                AddExternalError(ex);
            }
        }



        public bool ProcessFeeds()
        {

            try
            {
                // get the feed instances

                // sort the guids so we have something to compare
                // UpdateGuids(oInstanceXML)
                // now we need to compare them to existing feed items on the page
                // and depending on the save mode, ignore/delete
                // we wont overwrite details in case the admin has edited some text



                switch (Strings.LCase(oConfig["FeedMode"]) ?? "")
                {
                    case "import":
                        {
                            var oInstanceXML = GetFeedItems();
                            if (oInstanceXML != null)
                            {
                                if (Strings.LCase(oConfig["Debug"]) == "on")
                                {
                                    oInstanceXML.Save(goServer.MapPath("/parsedFeed.xml"));
                                }
                                this.AddExternalMessage(oDBH.importObjects(oInstanceXML.DocumentElement, cFeedURL, cXSLTransformPath));
                            }
                            else
                            {
                                AddExternalMessage("No Feed Items");
                            }

                            break;
                        }
                    case "stream":
                        {
                            AddExternalMessage(ImportStream());
                            break;
                        }

                    default:
                        {
                            var oInstanceXML = GetFeedItems();
                            if (oInstanceXML != null)
                            {
                                CompareFeedItems(ref oInstanceXML);
                            }
                            else
                            {
                                AddExternalMessage("No Feed Items");
                            }

                            break;
                        }

                }


                if (!(oDBH.myWeb.msException == ""))
                {
                    bResult = false;
                    AddExternalMessage(oDBH.myWeb.msException);
                }
                return bResult;
            }
            catch (Exception ex)
            {
                AddExternalError(ex);
            }

            return default;
        }

        public string ImportStream()
        {

            string instanceNodeName = FeedItemNode;
            XElement origInstance = null;
            long ProcessedQty = 0L;
            long completeCount = 0L;
            long failedCount = 0L;
            long startNo = 0L;
            string processInfo;
            long logId = 0L;
            try
            {

                if (Strings.LCase(oConfig["CompileImportXsl"]) == "off")
                {

                    oTransform.Compiled = false;
                }
                else
                {

                    oTransform.Compiled = true;
                }
                oTransform.XslFilePath = cXSLTransformPath;

                string ReturnMessage = "Streaming Feed ";

                if (oTransform.transformException is null)
                {

                    logId = oDBH.logActivity(Cms.dbHelper.ActivityType.ContentImport, 0, 0, 0, ReturnMessage + " Started using " + cXSLTransformPath);

                    string cDeleteTempTableName = "tmp-" + cFeedURL.Substring(cFeedURL.LastIndexOf("/") + 1).Replace(".xml", "").Replace(".ashx", "");
                    var eventsDoneEvt = new System.Threading.ManualResetEvent(false);
                    var Tasks = new Cms.dbImport(oDBH.oConn.ConnectionString, 0);
                    int workerThreads = 0;
                    int portThreads = 0;
                    System.Threading.ThreadPool.GetMaxThreads(out workerThreads, out portThreads);
                    System.Threading.ThreadPool.SetMaxThreads((int)Math.Round(workerThreads / 2d), (int)Math.Round(portThreads / 2d));
                    var doneEvents = new System.Threading.ManualResetEvent[1];

                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.OmitXmlDeclaration = true;
                    settings.NewLineOnAttributes = false;
                    settings.ConformanceLevel = ConformanceLevel.Document;
                    settings.CheckCharacters = true;
                    string debugFolder = "";

                    if (Strings.LCase(oConfig["Debug"]) == "on")
                    {
                        var ofs = new fsHelper();
                        ofs.mcRoot = "../";
                        ofs.CreatePath("/importtest");
                        ofs = default;
                        var newDir = new DirectoryInfo(System.Web.HttpContext.Current.Request.MapPath("/"));
                        debugFolder = newDir.Parent.FullName + @"\importtest\";
                    }

                    oDBH.updateActivity(logId, cDeleteTempTableName + " Streaming Start x Objects");
                    // is the feed XML

                    var wrequest = WebRequest.Create(cFeedURL);
                    wrequest.Timeout = -1;
                    using (var response = wrequest.GetResponse())
                    {
                        using (var reader = XmlReader.Create(response.GetResponseStream()))
                        {
                            XElement name = null;
                            XElement item = null;
                            string sDoc = "";
                            string sDocBefore = "";
                            reader.MoveToContent();
                            while (!reader.EOF)
                            {
                                if (reader.NodeType == XmlNodeType.Element && (reader.Name ?? "") != (instanceNodeName ?? ""))
                                {
                                    reader.ReadToFollowing(instanceNodeName);
                                }
                                else if (!reader.EOF & reader.NodeType != XmlNodeType.EndElement)
                                {
                                    try
                                    {
                                        origInstance = XNode.ReadFrom(reader) as XElement;
                                    }
                                    catch (Exception)
                                    {
                                        // reader.Read()
                                        reader.ReadToFollowing(instanceNodeName);
                                        // reader.MoveToContent()

                                        processInfo = "error at " + completeCount;
                                    }

                                    if (!(origInstance == null))
                                    {
                                        TextWriter oWriter = new StringWriter();
                                        var xWriter = XmlWriter.Create(oWriter, settings);
                                        try
                                        {

                                            var xreader = origInstance.CreateReader();
                                            xreader.MoveToContent();
                                            oTransform.Process(xreader, xWriter);

                                            sDoc = oWriter.ToString();

                                            sDocBefore = sDoc;
                                            // sDoc = Regex.Replace(sDoc, "&gt;", ">")
                                            // sDoc = Regex.Replace(sDoc, "&lt;", "<")
                                            sDoc = Xml.convertEntitiesToCodesFast(sDoc);
                                            string filename;
                                            var xDoc = new XmlDocument();
                                            if (string.IsNullOrEmpty(sDoc))
                                            {
                                                failedCount = failedCount + 1L;
                                            }
                                            else
                                            {
                                                xDoc.LoadXml(sDoc);
                                                foreach (XmlElement oInstance in xDoc.DocumentElement.SelectNodes("descendant-or-self::instance"))
                                                {
                                                    var stateObj = new Cms.dbImport.ImportStateObj();
                                                    stateObj.oInstance = oInstance;
                                                    stateObj.LogId = logId;
                                                    stateObj.FeedRef = cFeedURL;
                                                    stateObj.CompleteCount = completeCount;
                                                    stateObj.totalInstances = 0;
                                                    stateObj.bSkipExisting = false;
                                                    stateObj.bResetLocations = true;
                                                    stateObj.nResetLocationIfHere = 0;
                                                    stateObj.bOrphan = false;
                                                    stateObj.bDeleteNonEntries = false;
                                                    stateObj.cDeleteTempTableName = cDeleteTempTableName;
                                                    stateObj.moTransform = oTransform;

                                                    // If oInstance.NextSibling Is Nothing Then
                                                    // cProcessInfo = "Is Last"
                                                    // eventsDoneEvt.Set()
                                                    // End If

                                                    System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(Tasks.ImportSingleObject), stateObj);
                                                    stateObj = default;
                                                    completeCount = completeCount + 1L;
                                                }

                                                if (Strings.LCase(oConfig["Debug"]) == "on")
                                                {
                                                    if (xDoc.DocumentElement.SelectSingleNode("descendant-or-self::cContentForiegnRef[1]") is null)
                                                    {
                                                        filename = "ImportStreamFile";
                                                    }
                                                    else
                                                    {
                                                        filename = xDoc.DocumentElement.SelectSingleNode("descendant-or-self::cContentForiegnRef[1]").InnerText.Replace("/", "-");
                                                    }
                                                    xDoc.Save(debugFolder + filename + ".xml");
                                                }

                                            }
                                            xDoc = null;
                                            origInstance = null;
                                            oWriter = null;
                                            xWriter = null;
                                        }

                                        catch (Exception ex2)
                                        {
                                            processInfo = sDoc;

                                            AddExternalMessage(ex2.ToString() + ex2.StackTrace.ToString() + " DOC {" + sDocBefore + "} EndDoc");
                                            bResult = false;
                                            // AddExternalError(ex2)
                                        }
                                    }
                                }
                                else
                                {
                                    reader.Read();
                                }
                            }
                            eventsDoneEvt.Set();
                        }
                    }
                    ReturnMessage = cDeleteTempTableName + " " + completeCount + " Items Queued For Import";
                    oDBH.logActivity(Cms.dbHelper.ActivityType.ContentImport, 0, 0, 0, ReturnMessage + " Queued");

                    oDBH.myWeb.ClearPageCache();

                    return completeCount + " Items Processed";
                }
                else
                {
                    ReturnMessage = oTransform.transformException.ToString();
                    oDBH.logActivity(Cms.dbHelper.ActivityType.ContentImport, 0, 0, 0, ReturnMessage + " FAILED");
                    return ReturnMessage;
                }
            }



            catch (Exception ex)
            {
                if (logId > 0L)
                {
                    oDBH.updateActivity(logId, cFeedURL + "Error" + ex.Message);
                }
                AddExternalError(ex);
                return null;
            }
        }

        public XmlDocument GetFeedItems()
        {
            string oFeedXML = "";
            try
            {
                var oResXML = new XmlDocument();
                if (FeedXml is null)
                {
                    // Get the feed xml
                    HttpWebRequest oRequest;
                    HttpWebResponse oResponse = null;
                    StreamReader oReader;

                    // request the page
                    oRequest = (HttpWebRequest)WebRequest.Create(cFeedURL);
                    // Force a user agent for rubbish feed providers.
                    oRequest.UserAgent = "Mozilla/5.0 (compatible; eonicweb v5.1)";
                    // Set a 10 min timeout
                    if (!string.IsNullOrEmpty(oConfig["FeedTimeout"]))
                    {
                        oRequest.Timeout = Conversions.ToInteger(oConfig["FeedTimeout"]);
                    }

                    oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "getting url: " + cFeedURL);

                    oResponse = (HttpWebResponse)oRequest.GetResponse();
                    oReader = new StreamReader(oResponse.GetResponseStream());
                    oFeedXML = oReader.ReadToEnd();
                    // oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "received url: " & cFeedURL)
                    // The problem with masking namespaces is that you have to deal with any node that calls that namespace.
                    // oFeedXML = Replace(oFeedXML, "xmlns:", "exemelnamespace")
                    // oFeedXML = Replace(oFeedXML, "xmlns", "exemelnamespace")

                    // TS commented out for LogicRc Feed
                    // oFeedXML = Regex.Replace(oFeedXML, "&gt;", ">")
                    // oFeedXML = Regex.Replace(oFeedXML, "&lt;", "<")

                    // oFeedXML = xmlTools.convertEntitiesToCodes(oFeedXML)
                    oResXML.InnerXml = oFeedXML;
                }
                else
                {
                    oResXML = FeedXml;
                }



                oResXML.InnerXml = oFeedXML;
                if (Strings.LCase(oConfig["Debug"]) == "on")
                {
                    File.WriteAllText(goServer.MapPath("/recivedFeedRaw.xml"), oResXML.OuterXml);
                }
                // now get the feed into out format
                string cFeedItemXML;
                TextWriter oTW = new StringWriter();
                TextReader oTR;
                oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "Start transform url: " + cFeedURL);
                oTransform.XslFilePath = cXSLTransformPath;
                oTransform.Compiled = false;
                oTransform.Process(oResXML, ref oTW);
                oTR = new StringReader(oTW.ToString());
                cFeedItemXML = oTR.ReadToEnd();
                oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "End transform url: " + cFeedURL);


                // Strip out the xmlns
                cFeedItemXML = Regex.Replace(cFeedItemXML, @"xmlns(\:\w*)?=""[^""]*""", "");

                // Fix Number Entities
                cFeedItemXML = cFeedItemXML.Replace("&amp;#", "&#");

                cFeedItemXML = cFeedItemXML.Replace("&amp;", "&");
                cFeedItemXML = cFeedItemXML.Replace("&", "&amp;");
                // Fix any missing &amp;
                // cFeedItemXML = Regex.Replace(cFeedItemXML, "/&(?!amp;)/", "&amp;")

                File.WriteAllText(goServer.MapPath("/recivedFeedTransformed.xml"), cFeedItemXML);

                var oInstanceXML = new XmlDocument();
                oInstanceXML.LoadXml(stripNonValidXMLCharacters(cFeedItemXML));
                // oInstanceXML.InnerXml = cFeedItemXML

                // Populate empty url nodes
                foreach (XmlElement oUrlNode in oInstanceXML.SelectNodes("//url[.='']"))
                    oUrlNode.InnerText = cFeedURL;
                oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "Start Tidy");
                // If the Body has been cast as CData then the html will not have been converted
                string sContent;
                foreach (XmlElement oBodyItem in oInstanceXML.SelectNodes("//*[(local-name()='Body' and not(@htmlTransform='off')) or @htmlTransform='on']"))
                {
                    sContent = oBodyItem.InnerText;
                    if (!string.IsNullOrEmpty(sContent))
                    {
                        try
                        {
                            oBodyItem.InnerXml = sContent;
                            oBodyItem.SetAttribute("htmlTransform", "innertext-innerxml");
                        }
                        catch
                        {
                            oBodyItem.InnerXml = Text.tidyXhtmlFrag(sContent);
                            oBodyItem.SetAttribute("htmlTransform", "tidyXhtml");
                        }
                    }
                }
                oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "End Tidy");
                return oInstanceXML;
            }

            catch (Exception ex)
            {
                oDBH.logActivity(Cms.dbHelper.ActivityType.Custom1, 0, 0, 0, 0, "error getting url: " + ex.Message + ex.StackTrace);
                AddExternalError(ex);
                if (!string.IsNullOrEmpty(oFeedXML))
                    AddExternalMessage(oFeedXML);
                return null;
            }
        }

        public string stripNonValidXMLCharacters(string textIn)
        {
            var textOut = new System.Text.StringBuilder();
            // Used to hold the output.
            int current;
            // Used to reference the current character.
            if (textIn is null || string.IsNullOrEmpty(textIn))
            {
                return string.Empty;
            }
            // vacancy test.
            for (int i = 0, loopTo = textIn.Length - 1; i <= loopTo; i++)
            {
                current = Strings.AscW(textIn[i]);


                if (current == 0x9 || current == 0xA || current == 0xD || current >= 0x20 && current <= 0xD7FF || current >= 0xE000 && current <= 0xFFFD || current >= 0x10000 && current <= 0x10FFFF)
                {
                    textOut.Append(Strings.ChrW(current));
                }
            }
            return textOut.ToString();
        }

        public void CompareFeedItems(ref XmlDocument oInstanceXML)
        {
            try
            {
                // Dim oAdmXFrm As New Protean.Cms.Admin.AdminXforms
                // oAdmXFrm.open(New XmlDocument)
                string cContentType = oInstanceXML.DocumentElement.SelectSingleNode("instance[1]/tblContent/cContentSchemaName").InnerText;

                string cSQL;

                if (cContentType == "FeedItem")
                {
                    cSQL = "SELECT tblContent.* ,0 AS InFeed FROM tblContent INNER JOIN tblContentLocation ON tblContent.nContentKey = tblContentLocation.nContentId WHERE (cContentXmlBrief LIKE '%<url>" + cFeedURL + "</url>%') AND (cContentSchemaName = 'FeedItem')  AND (tblContentLocation.nStructId = " + nHostPageID + ")";
                }
                else
                {
                    cSQL = "SELECT tblContent.* ,0 AS InFeed FROM tblContent WHERE (cContentXmlBrief LIKE '%<url>" + cFeedURL + "</url>%') AND (cContentForiegnRef <> '') AND (cContentSchemaName = '" + cContentType + "')";
                }

                DataSet oDS = oDBH.GetDataSet(cSQL, "Items");
                DataRow oDR;
                int nContentKey;

                foreach (XmlElement oInstanceElmt in oInstanceXML.DocumentElement.SelectNodes("instance"))
                {
                    string cId = oInstanceElmt.SelectSingleNode("tblContent/cContentForiegnRef").InnerText;
                    oInstanceElmt.SelectSingleNode("tblContent/cContentName").InnerText = Text.CleanName(oInstanceElmt.SelectSingleNode("tblContent/cContentName").InnerText, true);
                    // If there's no foreign ref then let's use the contentname
                    if (string.IsNullOrEmpty(cId))
                    {
                        cId = oInstanceElmt.SelectSingleNode("tblContent/cContentName").InnerText;
                        cId = cId.Substring(0, Math.Min(50, cId.Length));
                        oInstanceElmt.SelectSingleNode("tblContent/cContentForiegnRef").InnerText = cId;
                    }
                    nContentKey = 0;
                    // nContentKey = oDBH.getContentByRef(cId)

                    // Try to find the item in the existing items

                    if (oDS.Tables.Count > 0)
                    {
                        foreach (DataRow currentODR in oDS.Tables["Items"].Rows)
                        {
                            oDR = currentODR;
                            Debug.WriteLine(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("'", oDR["cContentForiegnRef"]), "' = '"), cId), "' = ("), Interaction.IIf(Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDR["cContentForiegnRef"], cId, false)), "True", "False")), ")"));
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDR["cContentForiegnRef"], cId, false)))
                            {
                                oDR["InFeed"] = 1;
                                nContentKey = Conversions.ToInteger(oDR["nContentKey"]);
                                break;
                            }
                        }
                    }


                    // If the item was not found, then add it.
                    if (nContentKey == 0)
                    {

                        AddExternalMessage("Adding Item", cId);
                        _counters["add"].Add();
                        var oAdmXFrm = new Cms.Admin.AdminXforms(ref oDBH.myWeb.msException);
                        oAdmXFrm.myWeb = oDBH.myWeb;
                        oAdmXFrm.xFrmFeedItem(default, oInstanceElmt, nHostPageID, cFeedURL);
                    }

                    else if (nContentKey > 0 & UpdateExistingItems)
                    {
                        // If found, and update is flagged, then update it.


                        var oNewElmt = oInstanceXML.CreateElement("instance");
                        oNewElmt.InnerXml = oInstanceElmt.InnerXml;

                        if (oNewElmt.SelectSingleNode("//nContentKey") != null)
                            oNewElmt.SelectSingleNode("//nContentKey").InnerText = nContentKey.ToString();
                        if (oNewElmt.SelectSingleNode("//nContentPrimaryId") != null)
                            oNewElmt.SelectSingleNode("//nContentPrimaryId").InnerText = "0";
                        if (oNewElmt.SelectSingleNode("//nAuditId") != null)
                            oNewElmt.SelectSingleNode("//nAuditId").ParentNode.RemoveChild(oNewElmt.SelectSingleNode("//nAuditId"));
                        var oAdmXFrm = new Cms.Admin.AdminXforms(ref oDBH.myWeb.msException);
                        oAdmXFrm.myWeb = oDBH.myWeb;
                        XmlElement oXfrm = oAdmXFrm.xFrmFeedItem(nContentKey, oNewElmt, 0, cFeedURL);

                        if (oXfrm.GetAttribute("itemupdated") == "true")
                        {
                            AddExternalMessage("Updating Item", cId);
                            _counters["update"].Add();
                        }
                        else
                        {
                            AddExternalMessage("Not Updated", cId);
                            _counters["notupdated"].Add();
                        }

                    }
                    _counters["total"].Add();
                }

                // now we need to look at the old ones
                // if they want them manually removed then we leave it
                if (nSave == SaveMode.Archive | nSave == SaveMode.Delete)
                {
                    if (oDS.Tables.Count > 0)
                    {
                        foreach (DataRow currentODR1 in oDS.Tables["Items"].Rows)
                        {
                            oDR = currentODR1;
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDR["InFeed"], 0, false)))
                            {
                                switch (nSave)
                                {
                                    case SaveMode.Delete:
                                        {
                                            AddExternalMessage("Deleteing Item", oDR["cContentForiegnRef"].ToString());
                                            _counters["delete"].Add();
                                            oDBH.DeleteObject(Cms.dbHelper.objectTypes.Content, Convert.ToInt64(oDR["nContentKey"]));
                                            break;
                                        }
                                    case SaveMode.Archive:
                                        {
                                            AddExternalMessage("Archiving Item", oDR["cContentForiegnRef"].ToString());
                                            _counters["archive"].Add();
                                            ArchiveFeed(Conversions.ToInteger(oDR["nContentKey"]));
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
                AddExternalError(ex);
            }
        }

        public void ArchiveFeed(int nFeedId)
        {
            try
            {
                string cSQL = "UPDATE a SET a.dExpireDate =" + Database.SqlDate(DateTime.Now, true) + ", a.nStatus=0";
                cSQL += " from tblAudit as a";
                cSQL += " INNER JOIN tblContent as c ON a.nAuditKey = c.nAuditId";
                cSQL += " WHERE c.nContentKey = " + nFeedId + "";

                oDBH.ExeProcessSql(cSQL);
            }

            catch (Exception ex)
            {
                AddExternalError(ex);
            }
        }

        public void AddExternalMessage(string cMessage, string id = "")
        {
            if (oResultElmt != null)
            {
                var oElmt = oResultElmt.OwnerDocument.CreateElement("FeedMessage");
                try
                {
                    oElmt.InnerXml = cMessage;
                }
                catch (Exception)
                {
                    oElmt.InnerText = Strings.Replace(Strings.Replace(cMessage, "&gt;", ">"), "&lt;", "<");
                }
                if (!string.IsNullOrEmpty(id))
                    oElmt.SetAttribute("id", id);
                oResultElmt.AppendChild(oElmt);
            }
        }

        public void AddExternalError(Exception ex)
        {
            AddExternalMessage(ex.ToString() + ex.StackTrace.ToString());
            bResult = false;
        }





    }
}