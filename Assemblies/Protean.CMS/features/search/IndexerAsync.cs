using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
// This is the Indexer/Search items
using Lucene.Net.Index;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;
// regular expressions
using static Protean.Tools.FileHelper;
using static Protean.Tools.Xml;

namespace Protean
{

    public class IndexerAsync
    {

        private new string mcModuleName = "Protean.IndexerAsync";

        private string mcIndexReadFolder = ""; // the folder where the index is stored (from config)
        private string mcIndexWriteFolder = ""; // the folder where the index is stored (from config)
        private string mcIndexCopyFolder = "";
        private IndexWriter oIndexWriter; // Lucene class

        private Tools.Security.Impersonate moImp = null;
        private bool bNewIndex = false; // if we need a new index or just add to one
        private bool bIsError = false;
        private DateTime dStartTime;
        private bool bDebug = false;

        public int nPagesIndexed;
        public int nDocumentsIndexed;
        public int nDocumentsSkipped;
        public int nContentsIndexed;

        public string cExError;
        private System.Collections.Specialized.NameValueCollection moConfig;

        private Cms myWeb;

        public IndexerAsync(ref Cms aWeb)
        {
            // PerfMon.Log("Indexer", "New")
            mcModuleName = "Eonic.Search.Indexer";
            string cProcessInfo = string.Empty;
            myWeb = aWeb;
            moConfig = myWeb.moConfig;
            string siteSearchPath = moConfig["SiteSearchPath"];
            if (Strings.LCase(moConfig["SiteSearchDebug"]) == "on")
            {
                bDebug = true;
            }
            try
            {
                if (string.IsNullOrEmpty(siteSearchPath))
                    siteSearchPath = @"..\Index";
                if (!siteSearchPath.EndsWith(@"\"))
                    siteSearchPath = siteSearchPath + @"\";
                string IndexFolder = myWeb.goServer.MapPath(@"\") + siteSearchPath;
                var dir = new DirectoryInfo(IndexFolder);

                if (string.IsNullOrEmpty(moConfig["SiteSearchReadPath"]))
                {
                    mcIndexReadFolder = IndexFolder + @"Read\";
                }
                else
                {
                    mcIndexReadFolder = myWeb.goServer.MapPath(@"\") + moConfig["SiteSearchReadPath"];
                } // get the location to store the index
                var dirRead = new DirectoryInfo(mcIndexReadFolder);
                if (!dirRead.Exists)
                {
                    dir.CreateSubdirectory("Read");
                }

                if (string.IsNullOrEmpty(moConfig["SiteSearchWritePath"]))
                {
                    mcIndexWriteFolder = IndexFolder + @"Write\";
                }
                else
                {
                    mcIndexWriteFolder = myWeb.goServer.MapPath(@"\") + moConfig["SiteSearchWritePath"];
                } // get the location to store the index
                var dirWrite = new DirectoryInfo(mcIndexReadFolder);
                if (!dirWrite.Exists)
                {
                    dir.CreateSubdirectory("Write");
                }

                mcIndexCopyFolder = IndexFolder + @"IndexedSite\";
                var dirCopy = new DirectoryInfo(mcIndexCopyFolder);
                if (!dirCopy.Exists)
                {
                    dir.CreateSubdirectory("IndexedSite");
                }
            }

            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", bDebug: gbDebug);
            }
        }

        public string GetIndexInfo()
        {
            var oXmlDoc = new XmlDocument();
            var oXmlDocRead = new XmlDocument();
            var oXmlDocWrite = new XmlDocument();
            var oXmlElmt = oXmlDoc.CreateElement("IndexInfo");
            var oXmlReadElmt = oXmlDoc.CreateElement("Read");
            var oXmlWriteElmt = oXmlDoc.CreateElement("Write");
            try
            {
                var fs = new FileInfo(mcIndexReadFolder + "/indexInfo.xml");
                if (fs.Exists)
                {
                    oXmlDocRead.Load(mcIndexReadFolder + "/indexInfo.xml");
                    oXmlDocRead.DocumentElement.SetAttribute("folder", "read");
                    oXmlReadElmt.InnerXml = oXmlDocRead.DocumentElement.OuterXml;
                    oXmlElmt.AppendChild(oXmlReadElmt.FirstChild);
                }
                fs = new FileInfo(mcIndexWriteFolder + "/indexInfo.xml");
                if (fs.Exists)
                {
                    oXmlDocWrite.Load(mcIndexWriteFolder + "/indexInfo.xml");
                    oXmlDocWrite.DocumentElement.SetAttribute("folder", "write");
                    oXmlWriteElmt.InnerXml = oXmlDocWrite.DocumentElement.OuterXml;
                    oXmlElmt.AppendChild(oXmlWriteElmt.FirstChild);
                }

                fs = null;
                return oXmlElmt.OuterXml;
            }

            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", bDebug: gbDebug);
                return null;
            }
        }

        public string DoIndex(ref bool bResult,int nPage = 0 )
        {
            // nPage = 59
            // PerfMon.Log("Indexer", "DoIndex")
            string cProcessInfo = "";
            string cPageHtml = string.Empty;
            string cPageExtract = string.Empty;
            string cPageXsl = "/xsl/search/cleanPage.xsl";
            string cExtractXsl = "/xsl/search/extract.xsl";
            var oPageXml = new XmlDocument();

            string cRules = string.Empty;

            long nPagesRemaining = 0L;
            long nPagesSkipped = 0L;
            long nContentSkipped = 0L;
            int nIndexed = 0; // count of the indexed items
            string cIndexDetailTypes = "NewsArticle,Event,Product,Contact,Document,Job";
            var oDS = new DataSet();
            string cSQL = "";
            string[] IndexDetailTypes;
            XmlElement errElmt;

            var oIndexInfo = new XmlDocument();
            var oInfoElmt = oIndexInfo.CreateElement("indexInfo");
            oIndexInfo.AppendChild(oInfoElmt);
            int idxConcurrency = 10;
            if (!string.IsNullOrEmpty(moConfig["indexConcurrency"]))
            {
                idxConcurrency = Conversions.ToInteger(moConfig["indexConcurrency"]);
            }
            var lcts = new LimitedConcurrencyLevelTaskScheduler(idxConcurrency);
            var factory = new TaskFactory(lcts);
            var cts = new CancellationTokenSource();

            var TaskArray = new List<Task>();
            string ResponseMessage = "";
            try
            {

                bIsError = false;
                if (nPage == 0)
                    bNewIndex = true; // if we are indexing everything then is a new index
                if (!string.IsNullOrEmpty(moConfig["SiteSearchIndexDetailTypes"]))
                {
                    cIndexDetailTypes = moConfig["SiteSearchIndexDetailTypes"];
                }
                IndexDetailTypes = Strings.Split(Strings.Replace(cIndexDetailTypes, " ", ""), ",");
                dStartTime = DateTime.Now;

                DateTime dLastRun;
                var oFs = new fsHelper();

                if (File.Exists(mcIndexWriteFolder + "/indexInfo.xml"))
                {
                    var oLastIndexInfo = new XmlDocument();
                    oLastIndexInfo.Load(mcIndexWriteFolder + "/indexInfo.xml");
                    int minInterval = 12;
                    // If moConfig("SiteSearchIndexMinInterval") <> "" Then
                    // minInterval = moConfig("SiteSearchIndexMinInterval")
                    // End If
                    if (moConfig["SiteSearchIndexResultPaging"] != null)
                    {
                        minInterval = Conversions.ToInteger(moConfig["SiteSearchIndexResultPaging"]);
                    }
                    // If moConfig("SiteSearchIndexResultPaging") <> "" Then
                    // minInterval = moConfig("SiteSearchIndexResultPaging")
                    // End If

                    if (oLastIndexInfo != null)
                    {
                        var oLastInfoElmt = oLastIndexInfo.DocumentElement;
                        dLastRun = Conversions.ToDate(oLastInfoElmt.GetAttribute("startTime"));
                        if (string.IsNullOrEmpty(oLastInfoElmt.GetAttribute("endTime")) & dLastRun > DateTime.Now.AddHours(minInterval * -1))
                        {
                            ResponseMessage = "Last Index is still running. Started:" + Conversions.ToString(dLastRun);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(ResponseMessage))
                {
                    return ResponseMessage;
                }

                else
                {
                    // checking index file size start
                    string TotalSize;

                    var infoReader = new FileInfo(Path.GetDirectoryName(Path.GetDirectoryName(myWeb.goServer.MapPath(@"\"))) + @"\Index\Write\indexInfo.xml");
                    if (infoReader.Exists)
                    {
                        TotalSize = infoReader.Length.ToString();

                        if (myWeb.moConfig["indexFileSize"] != null)
                        {
                            if (Convert.ToInt32(myWeb.moConfig["indexFileSize"]) * Convert.ToInt32(1048576) < Convert.ToInt32(TotalSize)) // converted config mb size to byte
                            {
                                // StopIndex()
                                return default;
                            }
                        }
                    }

                    // checking index file size end


                    StartIndex();

                    if (bIsError)
                        return default;

                    // Check for local xsl or go to common
                    if (!File.Exists(goServer.MapPath(cPageXsl)))
                    {
                        cPageXsl = "/ewcommon" + cPageXsl;
                    }
                    if (!File.Exists(goServer.MapPath(cExtractXsl)))
                    {
                        cExtractXsl = "/ewcommon" + cExtractXsl;
                    }


                    oInfoElmt.SetAttribute("startTime", XmlDate(DateTime.Now, true));
                    oInfoElmt.SetAttribute("cPageXsl", cPageXsl);
                    oInfoElmt.SetAttribute("cExtractXsl", cExtractXsl);
                    oInfoElmt.SetAttribute("IndexDetailTypes", cIndexDetailTypes);

                    oIndexInfo.Save(mcIndexWriteFolder + "/indexInfo.xml");

                    // make a new web but going to have to overide some stuff
                    // and double override to be sure

                    string styleFile = goServer.MapPath(cPageXsl);
                    // PerfMon.Log("Web", "ReturnPageHTML - loaded Style")
                    var oTransform = new Protean.XmlHelper.Transform();
                    oTransform.XslFilePath = styleFile;
                    oTransform.Compiled = false; // IIf(LCase(moConfig("CompiledTransform")) = "on", True, False)
                    oTransform.xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                    var ewXsltExt = new Protean.xmlTools.xsltExtensions(ref myWeb);
                    oTransform.xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);

                    // full pages
                    cSQL = "Select nStructKey,cStructName From tblContentStructure"; // get all structure
                    if (nPage > 0)
                        cSQL += " WHERE nStructKey = " + nPage; // unless a specific page
                                                                // If nPage > 0 Then cSQL &= " WHERE nStructKey = " & nPage & " Or nStructParId =" & nPage 'unless a specific page

                    oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Structure");

                    // now we loop through the different tables and index the data
                    // Do Pages
                    if (oDS.Tables.Contains("Structure"))
                    {

                        var Tasks = new IndexPageAsync(myWeb.moCtx, oTransform, oIndexWriter, cPageXsl);
                        Tasks.bDebug = bDebug;
                        Tasks.mcIndexCopyFolder = mcIndexCopyFolder;
                        Tasks.mcIndexReadFolder = mcIndexReadFolder;
                        Tasks.mcIndexWriteFolder = mcIndexWriteFolder;
                        Tasks.cIndexDetailTypes = cIndexDetailTypes;
                        Tasks.IndexDetailTypes = IndexDetailTypes;
                        Tasks.oInfoElmt = oInfoElmt;
                        Tasks.oIndexInfo = oIndexInfo;
                        Tasks.nPagesRemaining = nPagesRemaining;

                        nPagesRemaining = oDS.Tables["Structure"].Rows.Count;
                        Tasks.nPagesRemaining = nPagesRemaining;

                        foreach (DataRow oDR in oDS.Tables["Structure"].Rows)
                        {

                            var pageObj = new IndexPageAsync.oPage();
                            pageObj.pgid = Conversions.ToLong(oDR["nStructKey"]);
                            pageObj.pagename = Conversions.ToString(oDR["cStructName"]);

                            // checking index file size start
                            infoReader = new FileInfo(Path.GetDirectoryName(Path.GetDirectoryName(myWeb.goServer.MapPath(@"\"))) + @"\Index\Write\indexInfo.xml");
                            if (infoReader.Exists)
                            {
                                TotalSize = infoReader.Length.ToString();

                                if (myWeb.moConfig["indexFileSize"] != null)
                                {
                                    if (Convert.ToInt32(myWeb.moConfig["indexFileSize"]) * 1048576 < Convert.ToInt32(TotalSize)) // converted config mb size to byte
                                    {
                                        StopIndex();
                                        return default;
                                    }
                                }
                            }
                            // checking index file size end

                            // don't do async
                            // Tasks.IndexSinglePage(pageObj)
                            // or do aSync
                            TaskArray.Add(factory.StartNew(() => Tasks.IndexSinglePage(pageObj), cts.Token));
                        }

                        Task.WaitAll(TaskArray.ToArray());

                    }

                    StopIndex();

                    oInfoElmt.SetAttribute("endTime", XmlDate(DateTime.Now, true));

                    // any non-critical errors ?
                    if (!string.IsNullOrEmpty(cExError))
                    {
                        errElmt = oIndexInfo.CreateElement("error");
                        errElmt.InnerXml = cExError;
                        oIndexInfo.FirstChild.AppendChild(errElmt);
                    }

                    oIndexInfo.Save(mcIndexWriteFolder + "/indexInfo.xml");

                    bResult = bIsError;

                    cts.Dispose();

                    // send email update as to index success or failure
                    cProcessInfo = "Sending Email Report";
                    var msg = new Protean.Messaging(ref myWeb.msException);
                    string serverSenderEmail = moConfig["ServerSenderEmail"] + "";
                    string serverSenderEmailName = moConfig["ServerSenderEmailName"] + "";
                    if (!Tools.Text.IsEmail(serverSenderEmail))
                    {
                        serverSenderEmail = "emailsender@protean.site";
                        serverSenderEmailName = "ProteanCMS Emailer";
                    }
                    string recipientEmail = moConfig["SiteAdminEmail"];
                    if (!string.IsNullOrEmpty(moConfig["IndexAlertEmail"]))
                    {
                        recipientEmail = moConfig["IndexAlertEmail"];
                    }

                    Protean.Cms.dbHelper argodbHelper = null;
                    msg.emailer(oInfoElmt, "/ewcommon/xsl/Email/IndexerAlert.xsl", "ProteanCMS Indexer", serverSenderEmail, recipientEmail, myWeb.moRequest.ServerVariables["SERVER_NAME"] + " Indexer Report", odbHelper: ref argodbHelper);
                    msg = (Protean.Messaging)null;

                    return "Index Complete";

                }
            }

            catch (Exception ex)
            {

                cExError += ex.InnerException.StackTrace.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "DoIndex", ex, "", cProcessInfo, gbDebug);
                errElmt = oIndexInfo.CreateElement("error");
                errElmt.InnerXml = cExError;
                oIndexInfo.FirstChild.AppendChild(errElmt);
                oInfoElmt.SetAttribute("endTime", XmlDate(DateTime.Now, true));

                oIndexInfo.Save(mcIndexWriteFolder + "/indexInfo.xml");

                try
                {
                    oIndexWriter.Dispose();
                    oIndexWriter = null;
                }
                catch (Exception ex2)
                {

                }

                return null;
            }
            finally
            {
                try
                {
                }



                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "DoIndex", ex, "", cProcessInfo, gbDebug);

                }
            }


        }

        private void StartIndex()
        {
            // PerfMon.Log("Indexer", "StartIndex")
            string cProcessInfo = "";
            do
            {
                try
                {
                    if (!string.IsNullOrEmpty(moConfig["AdminAcct"]))
                    {
                        moImp = new Tools.Security.Impersonate(); // for access
                        if (moImp.ImpersonateValidUser(moConfig["AdminAcct"], moConfig["AdminDomain"], moConfig["AdminPassword"], cInGroup: moConfig["AdminGroup"]))
                        {
                        }
                        else
                        {
                            Information.Err().Raise(108, Description: "Indexer did not validate");
                            break;
                        }
                    }
                    EmptyFolder(mcIndexWriteFolder);
                    if (gbDebug)
                    {
                        EmptyFolder(mcIndexCopyFolder);
                    }

                    bool bCreate = true; // check if file exists or if we need to create an index
                    if (File.Exists(mcIndexWriteFolder + "segments"))
                        bCreate = false;
                    // If bNewIndex Then bCreate = True 'override creation dependant on type of index
                    Lucene.Net.Store.Directory indexDir = Lucene.Net.Store.FSDirectory.Open(mcIndexWriteFolder);

                    var maxLen = new IndexWriter.MaxFieldLength(10000);

                    oIndexWriter = new IndexWriter(indexDir, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_CURRENT), bCreate, maxLen); // create the index writer

                    // TS added to limit memory usage
                    oIndexWriter.SetRAMBufferSizeMB(500d);
                    oIndexWriter.SetMaxBufferedDeleteTerms(50);
                    oIndexWriter.SetMaxBufferedDocs(100);
                }


                catch (Exception ex)
                {
                    cExError += ex.StackTrace.ToString() + Constants.vbCrLf;
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "StartIndex", ex, "", cProcessInfo, gbDebug);

                    bIsError = true;
                    try
                    {
                        oIndexWriter.Dispose();
                        oIndexWriter = null;
                    }
                    catch (Exception ex2)
                    {

                    }
                }
            }
            while (false);
        }

        private void EmptyFolder(string cDirectory)
        {
            // PerfMon.Log("Indexer", "EmptyFolder")
            string cProcessInfo = "";
            try
            {
                if (bNewIndex)
                {
                    // delete directories and thier children
                    while (Information.UBound(Directory.GetDirectories(cDirectory)) > 0)
                        Directory.Delete(Directory.GetDirectories(cDirectory)[0], true);
                    // delete files
                    while (Information.UBound(Directory.GetFiles(cDirectory)) > 0)
                    {
                        try
                        {
                            File.SetAttributes(Directory.GetFiles(cDirectory)[0], FileAttributes.Normal);
                            File.Delete(Directory.GetFiles(cDirectory)[0]);
                        }
                        catch (Exception ex)
                        {
                            cExError += ex.StackTrace.ToString() + Constants.vbCrLf;
                            stdTools.returnException(ref myWeb.msException, mcModuleName, "Empty Folder", ex, "", cProcessInfo, gbDebug);
                            return;

                        }
                    }
                    // try deleting a hidden folder
                    if (Directory.Exists(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(cDirectory, Interaction.IIf(Strings.Right(cDirectory, 1) == @"\", "", @"\")), "_vti_cnf"))))
                    {
                        try
                        {
                            Directory.Delete(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(cDirectory, Interaction.IIf(Strings.Right(cDirectory, 1) == @"\", "", @"\")), "_vti_cnf")), true);
                        }
                        catch (Exception ex)
                        {
                            cExError += ex.ToString() + Constants.vbCrLf;
                            stdTools.returnException(ref myWeb.msException, mcModuleName, "Empty Folder", ex, "", cProcessInfo, gbDebug);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    oIndexWriter.Dispose();
                    oIndexWriter = null;
                }
                catch (Exception ex2)
                {

                }
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "Empty Folder", ex, "", cProcessInfo, gbDebug);
            }
        }

        private void StopIndex()
        {
            // PerfMon.Log("Indexer", "StopIndex")
            string cProcessInfo = "";
            try
            {
                oIndexWriter.Optimize();
                oIndexWriter.Commit();
                oIndexWriter.Dispose();
                EmptyFolder(mcIndexReadFolder);
                CopyFolderContents(mcIndexWriteFolder, mcIndexReadFolder);
                if (!string.IsNullOrEmpty(moConfig["AdminAcct"]))
                {
                    moImp.UndoImpersonation();
                    moImp = null;
                }

                oIndexWriter = null;
            }
            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "StopIndex", ex, "", cProcessInfo, gbDebug);
                bIsError = true;
            }
        }

        private void CopyFolderContents(string cLocation, string cDestination)
        {
            // PerfMon.Log("Indexer", "CopyFolderContents")
            string cProcessInfo = "";
            try
            {
                var oDI = new DirectoryInfo(mcIndexWriteFolder);
                int FileCount = oDI.GetFiles().Length;
                int i;
                var loopTo = FileCount - 1;
                for (i = 0; i <= loopTo; i++)
                {
                    string cFileName = Strings.Replace(Directory.GetFiles(cLocation)[i], cLocation, cDestination);
                    Debug.WriteLine(cFileName);
                    File.Copy(Directory.GetFiles(cLocation)[i], cFileName, true);
                }
            }
            catch (Exception ex)
            {
                cExError += ex.ToString() + Constants.vbCrLf;
                stdTools.returnException(ref myWeb.msException, mcModuleName, "CopyFolderContents", ex, "", cProcessInfo, gbDebug);
            }
        }

        // Provides a task scheduler that ensures a maximum concurrency level while 
        // running on top of the thread pool.
        public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
        {
            // Indicates whether the current thread is processing work items.
            [ThreadStatic()]
            private static bool _currentThreadIsProcessingItems;

            // The list of tasks to be executed 
            private readonly LinkedList<Task> _tasks = new LinkedList<Task>();

            // The maximum concurrency level allowed by this scheduler. 
            private readonly int _maxDegreeOfParallelism;

            // Indicates whether the scheduler is currently processing work items. 
            private int _delegatesQueuedOrRunning = 0; // protected by lock(_tasks)

            // Creates a new instance with the specified degree of parallelism. 
            public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
            {
                if (maxDegreeOfParallelism < 1)
                {
                    throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
                }
                _maxDegreeOfParallelism = maxDegreeOfParallelism;
            }

            // Queues a task to the scheduler. 
            protected override void QueueTask(Task t)
            {
                // Add the task to the list of tasks to be processed.  If there aren't enough 
                // delegates currently queued or running to process tasks, schedule another. 
                lock (_tasks)
                {
                    _tasks.AddLast(t);
                    if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                    {
                        _delegatesQueuedOrRunning = _delegatesQueuedOrRunning + 1;
                        NotifyThreadPoolOfPendingWork();
                    }
                }
            }

            // Inform the ThreadPool that there's work to be executed for this scheduler. 
            private void NotifyThreadPoolOfPendingWork()
            {

                ThreadPool.UnsafeQueueUserWorkItem(NotifyThreadPoolOfPendingWork =>
                {
                    // Note that the current thread is now processing work items. 
                    // This is necessary to enable inlining of tasks into this thread.
                    _currentThreadIsProcessingItems = true;
                    try
                    {
                        // Process all available items in the queue. 
                        while (true)
                        {
                            Task item;
                            lock (_tasks)
                            {
                                // When there are no more items to be processed, 
                                // note that we're done processing, and get out. 
                                if (_tasks.Count == 0)
                                {
                                    _delegatesQueuedOrRunning = _delegatesQueuedOrRunning - 1;
                                    break;
                                }

                                // Get the next item from the queue
                                item = _tasks.First.Value;
                                _tasks.RemoveFirst();
                            }

                            // Execute the task we pulled out of the queue 
                            TryExecuteTask(item);
                            // We're done processing items on the current thread 
                        }
                    }
                    finally
                    {
                        _currentThreadIsProcessingItems = false;
                    }
                }, null);
            }

            // Attempts to execute the specified task on the current thread. 
            protected override bool TryExecuteTaskInline(Task t, bool taskWasPreviouslyQueued)
            {
                // If this thread isn't already processing a task, we don't support inlining 
                if (!_currentThreadIsProcessingItems)
                {
                    return false;
                }

                // If the task was previously queued, remove it from the queue 
                if (taskWasPreviouslyQueued)
                {
                    // Try to run the task. 
                    if (TryDequeue(t))
                    {
                        return TryExecuteTask(t);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return TryExecuteTask(t);
                }
            }

            // Attempt to remove a previously scheduled task from the scheduler. 
            protected override bool TryDequeue(Task t)
            {
                lock (_tasks)
                    return _tasks.Remove(t);
            }

            // Gets the maximum concurrency level supported by this scheduler. 
            public override int MaximumConcurrencyLevel
            {
                get
                {
                    return _maxDegreeOfParallelism;
                }
            }

            // Gets an enumerable of the tasks currently scheduled on this scheduler. 
            protected override IEnumerable<Task> GetScheduledTasks()
            {
                bool lockTaken = false;
                try
                {
                    System.Threading.Monitor.TryEnter(_tasks, ref lockTaken);
                    if (lockTaken)
                    {
                        return _tasks.ToArray();
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
                finally
                {
                    if (lockTaken)
                    {
                        System.Threading.Monitor.Exit(_tasks);
                    }
                }
            }

        }

        public class IndexPageAsync
        {


            public IndexWriter oIndexWriter; // Lucene class
            public string mcIndexReadFolder = "";
            public string mcIndexWriteFolder = "";
            public string mcIndexCopyFolder = "";
            public XmlDocument oIndexInfo;
            public string cXslPath;
            public bool bIsError = false;
            public XmlElement errElmt;
            public string cIndexDetailTypes;
            public string[] IndexDetailTypes;
            public bool bDebug;
            public XmlElement oInfoElmt;

            public long nPagesSkipped = 0L;
            public long nPagesIndexed = 0L;
            public long nDocumentsIndexed = 0L;
            public long nDocumentsSkipped = 0L;
            public long nContentSkipped = 0L;
            public long nContentsIndexed = 0L;
            public long nPagesRemaining = 0L;
            public ManualResetEvent doneEvent;
            public long nRootId;
            public Protean.XmlHelper.Transform moTransform;
            public System.Web.HttpContext moCtx;

            private int nIndexed = 0; // count of the indexed items

            public string cExError;
            private string mcModuleName = "IndexPageAsync";

            #region Error Handling
            public new event OnErrorEventHandler OnError;

            public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

            private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
            {
                OnError?.Invoke(sender, e);
            }
            #endregion

            public class oPage
            {
                public long pgid;
                public string pagename;
            }

            public IndexPageAsync(System.Web.HttpContext oCtx, Protean.XmlHelper.Transform oTransform, IndexWriter objIndexWriter, string pageXSL)
            {

                try
                {
                    moCtx = oCtx;
                    moTransform = oTransform;
                    oIndexWriter = objIndexWriter;
                    cXslPath = pageXSL;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }
            }


            public void IndexSinglePage(IndexPageAsync.oPage oPage)
            {               
                string cPageHtml;
                var oPageXml = new XmlDocument();
                string cRules = "";
                XmlElement oElmtRules;
                XmlElement oElmtURL = null;

                string cProcessInfo;
                string cPageExtract = "";
                var myWeb = new Cms(moCtx);

                try
                {
                    myWeb.InitializeVariables();
                    myWeb.Open();
                    myWeb.mnUserId = 1;
                    if (myWeb.moConfig["SiteSearchIndexHiddenDetail"] == "on")
                    {
                        myWeb.mbAdminMode = true;
                    }
                    else
                    {
                        myWeb.mbAdminMode = false;
                    }
                    myWeb.ibIndexMode = true;
                    myWeb.ibIndexRelatedContent = myWeb.moConfig["SiteSearchIndexRelatedContent"] == "on";
                    myWeb.moTransform = moTransform;
                    myWeb.moTransform.myWeb = myWeb;

                    // here we get a copy of the outputted html
                    // as the admin user would see it
                    // without bieng in admin mode
                    // so we can see everything
                    myWeb.mnPageId =Convert.ToInt32(oPage.pgid);
                    myWeb.moPageXml = new XmlDocument();
                    myWeb.mnArtId = 0;
                    myWeb.mbIgnorePath = true;
                    myWeb.mcEwSiteXsl = cXslPath;

                    cPageHtml = myWeb.ReturnPageHTML(0, true);
                    cPageHtml = Strings.Replace(cPageHtml, "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">", "");
                    cPageHtml = Strings.Replace(cPageHtml, " xmlns=\"http://www.w3.org/1999/xhtml\"", "");

                    var oPageElmt = oInfoElmt.OwnerDocument.CreateElement("page");

                    if (string.IsNullOrEmpty(cPageHtml))
                    {
                        // we have an error to handle
                        if (myWeb.msException != null)
                        {
                            var errorElmt = oIndexInfo.CreateElement("IndexElement");
                            errorElmt.InnerText = myWeb.msException;
                            try
                            {
                                errorElmt.SetAttribute("pgid", Conversions.ToString(oPage.pgid));
                                errorElmt.SetAttribute("name", Conversions.ToString(oPage.pagename));
                            }
                            catch (Exception)
                            {

                            }
                            cExError += ControlChars.CrLf + errorElmt.OuterXml;
                        }
                        nPagesSkipped += 1L;
                    }
                    else
                    {
                        try
                        {
                            oPageXml.LoadXml(cPageHtml);
                            oElmtRules = (XmlElement)oPageXml.SelectSingleNode("/html/head/meta[@name='ROBOTS']");
                            oElmtURL =(XmlElement)myWeb.moPageXml.SelectSingleNode("/Page/Menu/descendant-or-self::MenuItem[@id='" + myWeb.mnPageId + "']");
                            cRules = "";

                            // If xWeb.mnPageId = 156 Then
                            // Testing what happens when we hit a specific page
                            // cProcessInfo = "our page"
                            // End If

                            if (oElmtRules != null)
                                cRules = oElmtRules.GetAttribute("content");
                            if (!(Strings.InStr(cRules, "NOINDEX") > 0) & oElmtURL != null)
                            {
                                if (!oElmtURL.GetAttribute("url").StartsWith("http"))
                                {
                                    IndexPage(oElmtURL.GetAttribute("url"), oPageXml.DocumentElement, "Page", ref myWeb.msException);

                                    oPageElmt.SetAttribute("url", oElmtURL.GetAttribute("url"));
                                    oInfoElmt.AppendChild(oPageElmt);

                                    nPagesIndexed += 1L;
                                }
                                else
                                {
                                    nPagesSkipped += 1L;
                                }
                            }
                            else
                            {
                                nPagesSkipped += 1L;
                            }
                        }
                        catch (Exception ex)
                        {
                            var oPageErrElmt = oIndexInfo.CreateElement("errorInfo");
                            oPageErrElmt.SetAttribute("pgid", myWeb.mnPageId.ToString());
                            oPageErrElmt.SetAttribute("type", "Page");
                            oPageErrElmt.InnerText = ex.Message;
                            oInfoElmt.AppendChild(oPageErrElmt);
                            nPagesSkipped += 1L;
                        }

                        if (!(Strings.InStr(cRules, "NOFOLLOW") > 0))
                        {
                            // Now let index the content of the pages
                            // Only index content where this is the parent page, so we don't index for multiple locations.
                            long itemContentCount = 0L;
                            XmlNodeList oContentElmts = myWeb.moPageXml.SelectNodes("/Page/Contents/Content[@type!='FormattedText' and @type!='PlainText' and @type!='MetaData' and @type!='Image' and @type!='xform' and @type!='report' and @type!='xformQuiz' and @type!='Module' and @parId=/Page/@id]");
                            foreach (XmlElement oElmt in oContentElmts)
                            {
                                // If oElmt.GetAttribute("type") = "Company" Then
                                // cProcessInfo = "Not Indexing - " & oElmt.GetAttribute("type")
                                // End If
                                // Dim wordToFind As String = oElmt.GetAttribute("type")
                                // Dim wordIndex As Integer = Array.IndexOf(IndexDetailTypes, oElmt.GetAttribute("type"))
                                if (!(Array.IndexOf(IndexDetailTypes, oElmt.GetAttribute("type")) >= 0))
                                {
                                    // Don't index we don't want this type.
                                    cProcessInfo = "Not Indexing - " + oElmt.GetAttribute("type");
                                }

                                else
                                {
                                    cProcessInfo = "Indexing - " + oElmt.GetAttribute("type") + "id=" + oElmt.GetAttribute("id") + " name=" + oElmt.GetAttribute("name");

                                    myWeb.moPageXml = new XmlDocument(); // we need to get this again with our content Detail
                                    myWeb.moDbHelper.moPageXml = myWeb.moPageXml;
                                    myWeb.mcEwSiteXsl = cXslPath;
                                    myWeb.mnArtId = Convert.ToInt32(oElmt.GetAttribute("id"));
                                    myWeb.moContentDetail = null;
                                    cPageHtml = myWeb.ReturnPageHTML(0, true);
                                    // remove any declarations that might affect and Xpath Search
                                    cPageHtml = Strings.Replace(cPageHtml, "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">", "");
                                    cPageHtml = Strings.Replace(cPageHtml, " xmlns=\"http://www.w3.org/1999/xhtml\"", "");

                                    if (string.IsNullOrEmpty(cPageHtml))
                                    {
                                        // we have an error to handle
                                        if (myWeb.msException == "")
                                            myWeb.msException = null;
                                        if (myWeb.msException != null)
                                        {
                                            var errorElmt = oIndexInfo.CreateElement("IndexElement");
                                            errorElmt.InnerXml = myWeb.msException;
                                            try
                                            {
                                                errorElmt.SetAttribute("pgid", "0");
                                                errorElmt.SetAttribute("name", Conversions.ToString(oPage.pagename));
                                            }
                                            catch (Exception)
                                            {
                                                // Don't error if you can't set the above.
                                            }
                                            cExError += ControlChars.CrLf + errorElmt.OuterXml;
                                        }
                                        nPagesSkipped += 1L;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            oPageXml.LoadXml(cPageHtml);

                                            if (!(oElmt.GetAttribute("type") == "Document"))
                                            {
                                                oElmtRules = (XmlElement)oPageXml.SelectSingleNode("/html/head/meta[@name='ROBOTS']");
                                                cRules = "";
                                                string sPageUrl = "";
                                                if (oElmtURL != null)
                                                {
                                                    sPageUrl = oElmtURL.GetAttribute("url");
                                                }
                                                if (oElmtRules != null)
                                                    cRules = oElmtRules.GetAttribute("content");
                                                if (!(Strings.InStr(cRules, "NOINDEX") > 0) & !sPageUrl.StartsWith("http"))
                                                {

                                                    // handle cannonical link tag
                                                    if (oPageXml.DocumentElement.SelectSingleNode("descendant-or-self::link[@rel='canonical']") != null)
                                                    {
                                                        XmlElement oLinkElmt = (XmlElement)oPageXml.DocumentElement.SelectSingleNode("descendant-or-self::link[@rel='canonical']");
                                                        if (!string.IsNullOrEmpty(oLinkElmt.GetAttribute("href")))
                                                        {
                                                            sPageUrl = oLinkElmt.GetAttribute("href");
                                                        }
                                                    }

                                                    IndexPage(sPageUrl, oPageXml.DocumentElement, oElmt.GetAttribute("type"), ref myWeb.msException);

                                                    var oContentElmt = oInfoElmt.OwnerDocument.CreateElement(oElmt.GetAttribute("type"));
                                                    oContentElmt.SetAttribute("artId", myWeb.mnArtId.ToString());
                                                    oContentElmt.SetAttribute("url", sPageUrl);

                                                    oPageElmt.AppendChild(oContentElmt);

                                                    nIndexed += 1;
                                                    nContentsIndexed += 1L;
                                                    itemContentCount += 1L;
                                                }
                                                else
                                                {
                                                    nContentSkipped += 1L;
                                                }
                                            }
                                            else
                                            {
                                                foreach (XmlElement oDocElmt in oElmt.SelectNodes("descendant-or-self::Path"))
                                                {
                                                    if (!string.IsNullOrEmpty(oDocElmt.InnerText))
                                                    {
                                                        if (oDocElmt.InnerText.StartsWith("http"))
                                                        {
                                                            // don't index
                                                            nDocumentsSkipped += 1L;
                                                        }
                                                        else
                                                        {
                                                            cProcessInfo = "Indexing - " + oDocElmt.InnerText;
                                                            string fileAsText = GetFileText(myWeb.goServer.MapPath(oDocElmt.InnerText), ref myWeb.msException);
                                                            string argsException = oElmt.GetAttribute("name");
                                                         
                                                            DateTime date;
                                                            DateTime? publishDate = DateTime.TryParse(oElmt.GetAttribute("publish"), out date) ? date : (DateTime?)null;
                                                            DateTime? expireDate = DateTime.TryParse(oElmt.GetAttribute("update"), out date) ? date : (DateTime?)null;
                                                            IndexPage(myWeb.mnPageId, "<h1>" + oElmt.GetAttribute("name") + "</h1>" + fileAsText, oDocElmt.InnerText, myWeb.msException, ref argsException, "Download", myWeb.mnArtId, cPageExtract, publishDate, expireDate);

                                                            var oFileElmt = oInfoElmt.OwnerDocument.CreateElement("file");
                                                            oPageElmt.SetAttribute("file", oDocElmt.InnerText);
                                                            oInfoElmt.AppendChild(oPageElmt);

                                                            nIndexed += 1;
                                                            nDocumentsIndexed += 1L;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            var oPageErrElmt = oIndexInfo.CreateElement("errorInfo");
                                            oPageErrElmt.SetAttribute("pgid", myWeb.mnPageId.ToString());
                                            oPageErrElmt.SetAttribute("type", oElmt.GetAttribute("type"));
                                            oPageErrElmt.SetAttribute("artid", oElmt.GetAttribute("id"));
                                            oPageErrElmt.InnerText = ex.Message;
                                            oPageElmt.AppendChild(oPageErrElmt);
                                            nContentSkipped += 1L;
                                            cProcessInfo = cPageHtml;
                                        }

                                    }

                                }
                            }
                            oPageElmt.SetAttribute("contentCount", itemContentCount.ToString());
                        }

                        if (bIsError)
                        {
                            errElmt = oIndexInfo.CreateElement("error");
                            errElmt.InnerXml = cExError;
                            oIndexInfo.FirstChild.AppendChild(errElmt);
                            oInfoElmt.SetAttribute("endTime", XmlDate(DateTime.Now, true));
                        }

                        Interlocked.Decrement(ref nPagesRemaining);

                        if (oInfoElmt.GetAttribute("indexCount") != null)
                        {
                            oInfoElmt.SetAttribute("indexCount", nIndexed.ToString());
                        }
                        if (oInfoElmt.GetAttribute("pagesIndexed") != null)
                        {
                            oInfoElmt.SetAttribute("pagesIndexed", nPagesIndexed.ToString());
                        }
                        if (oInfoElmt.GetAttribute("pagesRemaining") != null)
                        {
                            oInfoElmt.SetAttribute("pagesRemaining", nPagesRemaining.ToString());
                        }
                        if (oInfoElmt.GetAttribute("pagesSkipped") != null)
                        {
                            oInfoElmt.SetAttribute("pagesSkipped", nPagesSkipped.ToString());
                        }
                        if (oInfoElmt.GetAttribute("contentCount") != null)
                        {
                            oInfoElmt.SetAttribute("contentCount", nContentsIndexed.ToString());
                        }
                        if (oInfoElmt.GetAttribute("contentSkipped") != null)
                        {
                            oInfoElmt.SetAttribute("contentSkipped", nContentSkipped.ToString());
                        }
                        if (oInfoElmt.GetAttribute("documentsIndexed") != null)
                        {
                            oInfoElmt.SetAttribute("documentsIndexed", nDocumentsIndexed.ToString());
                        }
                        if (oInfoElmt.GetAttribute("documentsSkipped") != null)
                        {
                            oInfoElmt.SetAttribute("documentsSkipped", nDocumentsSkipped.ToString());
                        }
                        // sometimes thefile will be locked, that is OK we save it when we can.
                        if (!FileInUse(mcIndexWriteFolder + "/indexInfo.xml"))
                        {
                            oIndexInfo.Save(mcIndexWriteFolder + "/indexInfo.xml");
                        }

                        if (bIsError)
                        {
                            return;
                        }

                    }
                }

                catch (Exception ex)
                {
                    cExError = ex.Message;
                }
                finally
                {
                    oPage = null;
                    myWeb.Close();
                    myWeb = null;
                }
            }

            public bool FileInUse(string sFile)
            {
                if (File.Exists(sFile))
                {
                    try
                    {
                        short F = (short)FileSystem.FreeFile();
                        FileSystem.FileOpen(F, sFile, OpenMode.Binary, OpenAccess.ReadWrite, OpenShare.LockReadWrite);
                        FileSystem.FileClose(F);
                    }
                    catch
                    {
                        return true;
                    }
                }

                return default;
            }

            private void IndexPage(string url, XmlElement pageXml, string pageType, ref string sException)
            {

                string methodName = "IndexPage(String,XmlElement,[String])";
                // PerfMon.Log("Indexer", methodName)

                string processInfo = url;

                try
                {

                    var indexDoc = new Document();

                    // Add the basic field types
                    indexDoc.Add(new Field("url", url, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    indexDoc.Add(new Field("type", pageType, Field.Store.YES, Field.Index.NOT_ANALYZED));


                    if (pageXml != null)
                    {

                        // Add the meta data to the fields
                        foreach (XmlElement metaContent in pageXml.SelectNodes("/html/head/meta"))


                            indexMeta(ref indexDoc, metaContent, ref sException);

                        // Add the text
                        XmlElement body = (XmlElement)pageXml.SelectSingleNode("/html/body");
                        string text = "";
                        if (body is null)
                        {
                            text = pageXml.OuterXml;
                        }
                        else
                        {
                            text = body.InnerXml;
                        }
                        indexDoc.Add(new Field("text", text, Field.Store.YES, Field.Index.ANALYZED)); // the actual content/text 


                    }

                    // Save the page
                    SavePage(url, pageXml.OuterXml);

                    // Add document to the index
                    oIndexWriter.AddDocument(indexDoc);
                }



                catch (Exception ex)
                {
                    try
                    {
                    }
                    // oIndexWriter.Close()
                    // oIndexWriter = Nothing
                    catch (Exception ex2)
                    {

                    }
                    cExError += ex.StackTrace.ToString() + Constants.vbCrLf;
                    returnException(ref sException, mcModuleName, methodName, ex, "", processInfo, gbDebug);
                    bIsError = true;
                }
            }


            private void IndexPage(int nPageId, string cPageText, string cURL, string cPageTitle, ref string sException, string cContentType = "Page", long nContentId = 0L, string cAbstract = "", DateTime? dPublish = null, DateTime? dUpdate = null)
            {
                // PerfMon.Log("Indexer", "IndexPage")
                string cProcessInfo = cURL;

                try
                {
                    var oDoc = new Document(); // This is the document element
                                               // here we need to get the proper paths

                    oDoc.Add(new Field("pgid", nPageId.ToString(), Field.Store.YES, Field.Index.ANALYZED));
                    oDoc.Add(new Field("artid", nContentId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

                    oDoc.Add(new Field("url", cURL, Field.Store.YES, Field.Index.NOT_ANALYZED)); // url of the page (simple)
                    oDoc.Add(new Field("name", cPageTitle, Field.Store.YES, Field.Index.NOT_ANALYZED)); // the name of the page
                    oDoc.Add(new Field("contenttype", cContentType, Field.Store.YES, Field.Index.NOT_ANALYZED));  // the type of the content


                    if (!string.IsNullOrEmpty(cAbstract))
                    {
                        oDoc.Add(new Field("abstract", cAbstract, Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }
                    if (dPublish != default)
                    {
                        oDoc.Add(new Field("publishDate", XmlDate(dPublish), Field.Store.YES, Field.Index.NOT_ANALYZED));
                    }
                    if (dUpdate != default)
                    {
                        oDoc.Add(new Field("updateDate", XmlDate(dUpdate), Field.Store.YES, Field.Index.NOT_ANALYZED));

                    }
                    oDoc.Add(new Field("text", cPageText, Field.Store.YES, Field.Index.ANALYZED)); // the actual content/text 

                    oIndexWriter.AddDocument(oDoc); // add it to the index

                    SavePage(cURL, cPageText);
                }

                // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                // This is also where we can recreate the site as static html if needed
                // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@


                catch (Exception ex)
                {
                    cExError += ex.ToString() + Constants.vbCrLf;
                    returnException(ref sException, mcModuleName, "IndexPage", ex, "", cProcessInfo, gbDebug);
                    bIsError = true;
                }
            }


            private void indexMeta(ref Document indexDocument, XmlElement metaContent, ref string sException, bool forSorting = false)
            {
                string processInfo = "";

                // Values grabbed from each content item
                var indexContent = Field.Index.NOT_ANALYZED;
                var storeContent = Field.Store.YES;
                string metaName = "";
                string metaContentValue = "";
                string metaType = "";

                // The abstract field and type specific fields
                AbstractField metaField = null;
                NumericField metaNumericField;

                // type specific value conversion
                object convertedNumber = null;
                DateTime convertedDate;

                try
                {

                    // Determine whether to tokenize this - by default, no
                    indexContent = (Field.Index)Conversions.ToInteger(Interaction.IIf(metaContent.GetAttribute("tokenize") == "true" & !forSorting, Field.Index.ANALYZED, Field.Index.NOT_ANALYZED));

                    // Determine whether to store this - by default, YES
                    storeContent = (Field.Store)Conversions.ToInteger(Interaction.IIf(metaContent.GetAttribute("store") == "false" | forSorting, Field.Store.NO, Field.Store.YES));

                    metaName = metaContent.GetAttribute("name");
                    if (forSorting)
                        metaName = "ewsort-" + metaName;

                    // Get content - this will either be an attribute, or, if not present, the innerXml of the node
                    if (metaContent.Attributes.GetNamedItem("content") is null)
                    {
                        metaContentValue = metaContent.InnerXml;
                    }

                    else
                    {
                        metaContentValue = metaContent.GetAttribute("content");
                    }

                    metaType = metaContent.GetAttribute("type");

                    // Create the field - based on type
                    switch (metaType ?? "")
                    {



                        case "float":
                        case "number":
                            {

                                if (Tools.Number.CheckAndReturnStringAsNumber(metaContentValue, ref convertedNumber, (Type)Interaction.IIf(metaType == "float", typeof(float), typeof(long))))
                                {

                                    // Create the numeric field
                                    metaNumericField = new NumericField(metaName, storeContent, true);


                                    // Set the value
                                    switch (metaType ?? "")
                                    {

                                        case "float":
                                            {
                                                metaNumericField.SetFloatValue(Conversions.ToSingle(convertedNumber));
                                                break;
                                            }

                                        default:
                                            {
                                                metaNumericField.SetLongValue(Conversions.ToLong(convertedNumber));
                                                break;
                                            }

                                    }

                                    metaField = metaNumericField;
                                }

                                else
                                {

                                    // It's not a number don't add it
                                    metaField = null;

                                }

                                break;
                            }


                        case "date":
                            {

                                // Check if the string is a valid date
                                if (!string.IsNullOrEmpty(metaContentValue) && DateTime.TryParse(metaContentValue, out convertedDate))
                                {

                                    metaField = new Field(metaName, DateTools.DateToString(Conversions.ToDate(metaContentValue), DateTools.Resolution.SECOND), storeContent, indexContent);
                                }
                                else
                                {

                                    // It's not a date don't add it
                                    metaField = null;

                                }

                                break;
                            }

                        default:
                            {

                                metaField = new Field(metaName, metaContentValue, storeContent, indexContent);
                                break;
                            }

                    }


                    if (metaField != null)
                    {
                        indexDocument.Add(metaField);

                        // Check for sorting
                        if (metaContent.GetAttribute("sortable") == "true" & !forSorting)
                        {
                            indexMeta(ref indexDocument, metaContent, ref sException, true);
                        }
                    }
                }

                catch (Exception ex)
                {
                    cExError += ex.ToString() + Constants.vbCrLf;
                    returnException(ref sException, mcModuleName, "indexMeta", ex, "", processInfo, gbDebug);
                    bIsError = true;
                }

            }

            private string GetFileText(string cPath, ref string sException, string cOtherText = "")
            {
                // PerfMon.Log("Indexer", "GetFileText")
                string cProcessInfo = "";
                try
                {
                    var oFile = new FileDoc(cPath);
                    return cOtherText + oFile.Text;
                }
                catch (Exception ex)
                {
                    cExError += ex.ToString() + Constants.vbCrLf;
                    returnException(ref sException, mcModuleName, "DoCheck", ex, "", cProcessInfo, gbDebug);
                    return cOtherText;
                }
            }

            private void SavePage(string cUrl, string cBody)
            {
                // PerfMon.Log("Indexer", "IndexPage")
                string cProcessInfo = "";
                string filename = "";
                string filepath = "";
                string artId = string.Empty;
                string Ext = ".html";
                try
                {
                    if (bDebug)
                    {


                        // let's clean up the url
                        if (cUrl.LastIndexOf("/?") > -1)
                        {
                            cUrl = cUrl.Substring(0, cUrl.LastIndexOf("/?"));
                        }
                        if (cUrl.LastIndexOf("?") > -1)
                        {
                            cUrl = cUrl.Substring(0, cUrl.LastIndexOf("?"));
                        }

                        if (string.IsNullOrEmpty(cUrl) | cUrl == "/")
                        {
                            filename = "home.html";
                        }
                        else
                        {
                            filename = Strings.Left(cUrl.Substring(cUrl.LastIndexOf("/") + 1), 240);
                            if (cUrl.LastIndexOf("/") > 0)
                            {
                                filepath = Strings.Left(cUrl.Substring(0, cUrl.LastIndexOf("/")), 240) + "";
                            }
                        }

                        var oFS = new fsHelper(moCtx);
                        oFS.mcStartFolder = mcIndexCopyFolder;

                        cProcessInfo = "Saving:" + mcIndexCopyFolder + filepath + @"\" + filename + Ext;

                        // Tidy up the filename
                        filename = ReplaceIllegalChars(filename);
                        filename = Strings.Replace(filename, @"\", "-");
                        filepath = Strings.Replace(filepath, "/", @"\") + "";
                        if (filepath.StartsWith(@"\") & mcIndexCopyFolder.EndsWith(@"\"))
                        {
                            filepath.Remove(0, 1);
                        }

                        cProcessInfo = "Saving:" + mcIndexCopyFolder + filepath + @"\" + filename + Ext;
                        string FullFilePath = mcIndexCopyFolder + filepath + @"\" + filename;

                        if (FullFilePath.Length > 255)
                        {
                            FullFilePath = Strings.Left(FullFilePath, 240) + Ext;
                        }
                        else
                        {
                            FullFilePath = FullFilePath + Ext;
                        }

                        if (!string.IsNullOrEmpty(filepath))
                        {
                            string sError = oFS.CreatePath(filepath);
                            if (sError == "1")
                            {
                                File.WriteAllText(FullFilePath, cBody, System.Text.Encoding.UTF8);
                            }
                            else
                            {
                                cExError += "<Error>Create Path: " + filepath + " - " + sError + "</Error>" + Constants.vbCrLf;
                            }
                        }
                        else
                        {
                            File.WriteAllText(mcIndexCopyFolder + filename + Ext, cBody, System.Text.Encoding.UTF8);
                        }



                        oFS = default;
                    }
                }
                catch (Exception ex)
                {
                    // if saving of a page fails we are not that bothered.
                    cExError += "<Error>" + filepath + filename + ex.Message + "</Error>" + Constants.vbCrLf;
                    // returnException(myWeb.msException, mcModuleName, "SavePage", ex, "", cProcessInfo, gbDebug)
                    // bIsError = True
                }
            }

        }


    }
}