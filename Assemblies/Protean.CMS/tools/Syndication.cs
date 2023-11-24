// ================================================================================================
// Eonic.Syndication
// Desc:       Retrieves site content and syndicates it to distributors
// Author:     Ali Granger
// Updated:    17-Aug-09
// Docs:       http://intra.office.eonic.co.uk/intranet/Documents/Development/Eonic.Syndication.docx  
// 
// To create a new distrubtor, you need to add the following (search Technorati for the various update points)
// - Add name to Shared Constant in Syndication.distributorTypesList 
// - Add the method into the creation list in Private Sub Syndication.CreateDistributors
// - Create the distributor as a class under Syndication.Distributor
// - The class inherits Syndication.Distributor
// - It should override New (where specific config settings can be defined) and RunSyndicate
// Note: At the moment the only distributors are ping servers, which use the same XMLRPC interface
// and to this end Weblogs, Technorati and PingOMatic inherit Syndication.Distributor.GenericXmlRpc
// 
// ================================================================================================



using System;
using System.Collections;
using System.IO;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Tools;
using Protean.Tools.Integration.Twitter;
using static Protean.Tools.Database;
using static Protean.Tools.Xml;

namespace Protean
{

    /// <summary>
    ///    Eonic.Syndication retrieves site content and syndicates it to distributors
    /// </summary>
    /// <remarks></remarks>
    public class Syndication
    {

        #region Declarations

        // Eonic objects
        private Protean.Cms _myWeb;
        private string _moduleName = "Eonic.Syndication";

        // Core collections
        private string[] _distributorTypes;
        private string[] _contentTypes;
        private Distributor[] _distributors;

        // Core variables
        private string _distributorTypesList = "";
        private XmlElement _structureNode = null;
        private long _sourcePage;
        private bool _iterate;
        private XmlElement _extendedConfig;

        // Flag variables
        private bool _hasDistributors = false;

        // State variables
        private int _activityLog = 0;
        private string _diagnostics = "";
        private bool _isCompleted = false;
        private bool _hasFailures = false;
        private int _totalCompleted = 0;
        private int _contentSyndicated = 0;
        private DateTime _lastRun;
        private bool _hasBeenRunBefore = false;

        #endregion

        #region Enums and Constants

        // Pseudo-constant array
        // Use this instead of Enum, as the process of parsing values from Enum is inaccurate and processor heavy.
        private static string[] distributorTypesList = new[] { "Weblogs", "Technorati", "GenericXmlRpc", "PingOMatic" };

        #endregion

        #region Events

        public new event OnErrorEventHandler OnError;

        public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

        private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
        {
            OnError?.Invoke(sender, e);
        }

        #endregion

        #region Constructor

        public Syndication(ref Protean.Cms aWeb, string distributorTypes, string contentTypes, long pageId, bool iterate)




        {

            try
            {
                _myWeb = aWeb;
                _distributorTypesList = distributorTypes;
                DistributorTypeCollection = GetCSVArray(distributorTypes);
                ContentTypes = GetCSVArray(contentTypes);
                SourcePage = pageId;
                Iterate = iterate;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New(Web,String,String,Long,Boolean)", ex, ""));
            }
        }

        public Syndication(ref Protean.Cms aWeb, string distributorTypes, string contentTypes, long pageId, bool iterate, XmlElement extendedConfig)





        {
            try
            {
                _myWeb = aWeb;
                _distributorTypesList = distributorTypes;
                DistributorTypeCollection = GetCSVArray(distributorTypes);
                ContentTypes = GetCSVArray(contentTypes);
                SourcePage = pageId;
                Iterate = iterate;
                ExtendedConfig = extendedConfig;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New(Web,String,String,Long,Boolean,XmlElement)", ex, ""));
            }
        }

        #endregion

        #region Private Properties

        private XmlElement ContentsNode
        {
            get
            {
                try
                {
                    return _myWeb.moPageXml.DocumentElement.SelectSingleNode("Contents");
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "ContentsNode(Get)", ex, ""));
                    return null;
                }
            }
            set
            {
                try
                {
                    if (value.Name == "Contents")
                    {
                        _myWeb.moPageXml.DocumentElement.SelectSingleNode("Contents").InnerXml = value.InnerXml;
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "ContentsNode(Set)", ex, ""));
                }
            }
        }

        private bool IsStructureLoaded
        {
            get
            {
                return _structureNode != null;
            }
        }

        #endregion

        #region Public Properties

        public string[] ContentTypes
        {
            get
            {
                return _contentTypes;
            }
            set
            {
                _contentTypes = value;
            }
        }

        public string[] DistributorTypeCollection
        {
            get
            {
                return _distributorTypes;
            }
            set
            {

                try
                {
                    var tempDistributorTypes = new ArrayList();
                    bool hasValidDistributors = false;
                    foreach (string distributorType in value)
                    {

                        // For each value we check whether or not it's valid
                        if (IsDistributor(distributorType))
                        {
                            tempDistributorTypes.Add(distributorType);
                            hasValidDistributors = true;
                        }

                    }

                    // Set the private class variables
                    _hasDistributors = hasValidDistributors;
                    _distributorTypes = (string[])tempDistributorTypes.ToArray(typeof(string));

                    // Now try to set the distributors
                    CreateDistributors();
                }

                catch (Exception ex)
                {

                }

            }
        }

        public XmlElement ExtendedConfig
        {
            get
            {
                return _extendedConfig;
            }
            set
            {
                _extendedConfig = value;
            }
        }

        public bool Iterate
        {
            get
            {
                return _iterate;
            }
            set
            {
                _iterate = value;
            }
        }

        public long SourcePage
        {
            get
            {
                return _sourcePage;
            }
            set
            {
                _sourcePage = value;
            }
        }

        public int ActivityKey
        {
            get
            {
                return _activityLog;
            }
        }

        public string Diagnostics
        {
            get
            {
                return _diagnostics;
            }
        }

        public int ContentCount
        {
            get
            {
                return _contentSyndicated;
            }
        }

        private bool HasBeenRunBefore
        {
            get
            {
                string sqlQuery = "SELECT MAX(dDateTime) As LastRun FROM tblActivityLog WHERE nActivityType IN (200,201,202,203,204) AND nActivityKey <> " + _activityLog + " ";

                if (HasSourcePage)
                {
                    sqlQuery += " AND nStructId=" + SourcePage;
                }

                object objDate = _myWeb.moDbHelper.GetDataValue(sqlQuery, default, default, default);

                if (objDate != null && Information.IsDate(objDate))
                {
                    _lastRun = Conversions.ToDate(objDate);
                    _hasBeenRunBefore = true;
                }
                else
                {
                    _hasBeenRunBefore = false;
                }

                return _hasBeenRunBefore;
            }
        }

        public bool HasDistributors
        {
            get
            {
                return _hasDistributors;
            }
        }

        public bool HasSourcePage
        {
            get
            {
                return Information.IsNumeric(SourcePage) && SourcePage > 0L;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return _isCompleted;
            }
        }

        public bool IsContentsNodePopulated
        {
            get
            {
                return ContentsNode.SelectNodes("Content").Count > 0;
            }
        }

        public bool IsReady
        {
            get
            {
                return HasDistributors & ContentTypes.GetLength(0) > 0;
            }
        }


        #endregion

        #region Private Members

        /// <summary>
        /// Runs through the content looking for specific data types, retrieving metadata (e.g. BlogControl) for them
        /// </summary>
        /// <remarks></remarks>
        private void AddContentMetadata()
        {
            try
            {
                if (IsContentsNodePopulated)
                {

                    // Because each content type has specific circumstances, rather than run through each content node,
                    // we'll specifically search for content types.  Some content types will only require one call for all content

                    // ==============================================
                    // METADATA ADD:  BLOG ARTICLES
                    // ==============================================
                    var blogArticles = ContentsNode.SelectNodes("Content[@type='BlogArticle']");
                    if (blogArticles.Count > 0)
                    {

                        var blogArticlePageIds = new Hashtable();

                        // Go get all the pages to search for 
                        foreach (XmlElement blogArticle in blogArticles)
                        {
                            if (!blogArticlePageIds.ContainsKey(blogArticle.GetAttribute("parId")))
                            {
                                blogArticlePageIds.Add(blogArticle.GetAttribute("parId"), "");
                            }
                        }

                        // Now get the BlogSettings
                        var blogControls = ContentsNode.OwnerDocument.CreateElement("Contents");

                        string sqlCriteria = "";
                        sqlCriteria += " cContentSchemaName ='BlogSettings'";
                        sqlCriteria += " AND CL.nStructId IN (" + Dictionary.hashtableToCSV(ref blogArticlePageIds, Dictionary.Dimension.Key) + ")";
                        _myWeb.GetPageContentFromSelect(sqlCriteria, default, default, true, default, default, blogControls);

                        XmlElement blogControl = null;
                        if (blogControls.HasChildNodes)
                        {
                            // Go throgh all the blogArticles and add the metadata
                            foreach (XmlElement blogArticle in blogArticles)
                            {
                                if (Xml.NodeState(ref blogControls, "Content[@parId = " + blogArticle.GetAttribute("parId") + "]", "", "", 1, blogControl, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                {
                                    blogArticle.AppendChild(blogArticle.OwnerDocument.ImportNode(blogControl, true));
                                }
                            }
                        }


                    }

                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "AddContentMetadata", ex, ""));
            }
        }

        /// <summary>
        /// Check the submitted distributors against actual distributors, and create the distributors if valid.
        /// This also loads extended distributor settings if provided/required.
        /// </summary>
        /// <remarks></remarks>
        private void CreateDistributors()
        {
            try
            {
                Distributor newDistributor = null;

                // We use ArrayLists because they can be easily dynamically extended using the Add method.
                var tempDistributors = new ArrayList();
                foreach (string distributorType in DistributorTypeCollection)
                {

                    newDistributor = null;

                    // Create the distributor specific object
                    switch (distributorType ?? "")
                    {
                        case "Weblogs":
                            {
                                newDistributor = new Distributor.Weblogs(ref _myWeb);
                                break;
                            }
                        case "Technorati":
                            {
                                newDistributor = new Distributor.Technorati(ref _myWeb);
                                break;
                            }
                        case "GenericXmlRpc":
                            {
                                newDistributor = new Distributor.GenericXmlRpc(ref _myWeb);
                                break;
                            }
                        case "PingOMatic":
                            {
                                newDistributor = new Distributor.PingOMatic(ref _myWeb);
                                break;
                            }
                    }

                    if (newDistributor != null)
                    {
                        // Set common variables
                        newDistributor.OnError += _OnError;

                        if (newDistributor.UsesExtendedConfig)
                        {
                            // Try to locate the config
                            newDistributor.LoadConfig(ExtendedConfig);
                        }

                        // Add the distributor to the array
                        tempDistributors.Add(newDistributor);
                    }


                }

                // Convert the ArrayList back to an array
                _distributors = (Distributor[])tempDistributors.ToArray(typeof(Distributor));
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "CreateDistributors", ex, ""));
            }
        }

        /// <summary>
        /// Converts a CSV list into a String Array
        /// </summary>
        /// <param name="csvList">The list to convert</param>
        /// <param name="separator">The seperator in the list</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private string[] GetCSVArray(string csvList, string separator = ",")
        {
            try
            {
                if (string.IsNullOrEmpty(csvList))
                {
                    return null;
                }
                else
                {
                    return csvList.Split(Conversions.ToChar(separator));
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Handles Syndication logging.  Creates a new log if needed and sets the log key, or updates the current log.
        /// Note that logging is ties to SourcePage if it is specified.  This should allow multiple Syndications to be run against a website if needed.
        /// </summary>
        /// <param name="activityType">The activity type to log</param>
        /// <param name="logDetail">Optional. Additional detail to log</param>
        /// <remarks></remarks>
        public void Log(Protean.Cms.dbHelper.ActivityType activityType, string logDetail = "")
        {
            string sqlQuery = "";

            try
            {
                // Test if this has been logged
                if (Information.IsNumeric(ActivityKey) && ActivityKey > 0)
                {
                    // Alert Item has been logged, therefore update the record

                    sqlQuery = "UPDATE tblActivityLog ";
                    sqlQuery += "SET nActivityType = " + activityType + " ";
                    if (!string.IsNullOrEmpty(logDetail))
                        sqlQuery += "   ,cActivityDetail = '" + SqlFmt(Strings.Left(logDetail, 800)) + "' ";
                    sqlQuery += "WHERE nActivityKey = " + ActivityKey;
                    _myWeb.moDbHelper.ExeProcessSql(sqlQuery);
                }
                else
                {
                    // Alert Item has been not been logged, therefore insert the record

                    sqlQuery = "INSERT INTO tblActivityLog ( nUserDirId, nStructId, nArtId, nOtherId, dDateTime, nActivityType, cActivityDetail, cSessionId) VALUES (";
                    sqlQuery += "1,";
                    sqlQuery += SourcePage + ",";
                    sqlQuery += "0,";
                    sqlQuery += "0,";
                    sqlQuery += SqlDate(DateTime.Now, true) + ",";
                    sqlQuery += activityType + ",";
                    sqlQuery += "'" + SqlFmt(Strings.Left(logDetail, 800)) + "',";
                    sqlQuery += "'')";

                    _activityLog = Convert.ToInt32(_myWeb.moDbHelper.GetIdInsertSql(sqlQuery));
                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "Log", ex, sqlQuery));
            }
        }

        /// <summary>
        /// Runs through anything in the contents node and adds it to the activity log so that it doesn't get picked up again.
        /// Note that logging is ties to SourcePage if it is specified.  This should allow multiple Syndications to be run against a website if needed.
        /// </summary>
        /// <remarks></remarks>
        private void LogContent()
        {
            string sqlQuery = "";
            try
            {
                if (IsContentsNodePopulated)
                {
                    // Go through the content and flag it up on the database.
                    foreach (XmlElement content in ContentsNode.SelectNodes("Content"))
                    {
                        _contentSyndicated += 1;
                        sqlQuery = "INSERT INTO tblActivityLog ( nUserDirId, nStructId, nArtId, nOtherId, dDateTime, nActivityType, cActivityDetail, cSessionId) VALUES (";
                        sqlQuery += "1,";
                        sqlQuery += "0,";
                        sqlQuery += content.GetAttribute("id") + ",";
                        sqlQuery += _activityLog + ",";
                        sqlQuery += SqlDate(DateTime.Now, true) + ",";
                        sqlQuery += Cms.dbHelper.ActivityType.ContentSyndicated + ",";
                        sqlQuery += "'',";
                        sqlQuery += "'')";

                        _myWeb.moDbHelper.ExeProcessSql(sqlQuery);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "LogContent", ex, sqlQuery));
            }
        }

        /// <summary>
        /// Retrieve website content and add to the contentnode.
        /// Criteria for retrieval are content types, optional page id and iteration.
        /// There are also checks that content has not been synidcated already.
        /// </summary>
        /// <param name="contentTypeSqlList">Sql formatted, comma-separated list of content types to retireve</param>
        /// <param name="pageId">Optional, page to retrieve content from</param>
        /// <remarks></remarks>
        private void PopulateContent(string contentTypeSqlList, long pageId = 0L)
        {
            try
            {

                string sqlCriteria = "";
                string sqlAdditionalTables = "";

                sqlCriteria += " (cContentSchemaName IN (" + contentTypeSqlList + "))";

                // Page Id
                if (pageId > 0L)
                {
                    sqlCriteria += " AND CL.nStructId = " + pageId;
                }

                // Reset myWeb content
                XmlElement contents = null;
                Xml.NodeState(ref _myWeb.moPageXml.DocumentElement, "Contents", " ", "", XmlNodeState.HasContents, contents, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false);

                // Add some criteria to stop retrieving items that have been syndicated.
                sqlAdditionalTables = " LEFT JOIN tblActivityLog activity ON c.nContentKey = activity.nArtId And activity.nActivityType = 205 ";
                if (HasSourcePage)
                    sqlAdditionalTables += " AND activity.nStructId = " + SourcePage + " ";
                sqlCriteria += " AND activity.nActivityKey Is NULL ";

                // Only get content since the last time run.
                if (HasBeenRunBefore)
                {
                    sqlCriteria += " AND a.dUpdateDate >= " + SqlDate(_lastRun, true) + " ";
                }

                // Go get the content
                _myWeb.GetPageContentFromSelect(sqlCriteria, default, default, default, default, default, default, sqlAdditionalTables);


                // Add the contents to content node
                ContentsNode.InnerXml += Strings.Trim(contents.InnerXml);


                // Address ChildItems
                if (Iterate & IsStructureLoaded & pageId > 0L)
                {

                    foreach (XmlElement childPage in _structureNode.SelectNodes("//MenuItem[@id=" + pageId + "]/MenuItem"))


                        PopulateContent(contentTypeSqlList, Conversions.ToLong(childPage.GetAttribute("id")));

                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "Populatecontent", ex, ""));
            }
        }

        /// <summary>
        /// Runs through the contents found and removes any duplicate content nodes.
        /// </summary>
        /// <remarks></remarks>
        private void RemoveDuplicateContent()
        {
            try
            {
                if (IsContentsNodePopulated)
                {
                    var contentIds = new Hashtable();
                    foreach (XmlElement contentNode in ContentsNode.SelectNodes("Content"))
                    {
                        if (contentIds.ContainsKey(contentNode.GetAttribute("id")))
                        {
                            contentNode.ParentNode.RemoveChild(contentNode);
                        }
                        else
                        {
                            contentIds.Add(contentNode.GetAttribute("id"), "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "RemoveDuplicateContent", ex, ""));
            }
        }

        /// <summary>
        /// Clears out the page's Contents node.
        /// </summary>
        /// <remarks></remarks>
        private void ResetContentsNode()
        {
            try
            {
                XmlElement pageContentsNode = null;
                if (Conversions.ToBoolean(Xml.NodeState(ref _myWeb.moPageXml.DocumentElement, "Contents", "", "", 1, pageContentsNode, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false)))
                {
                    pageContentsNode.InnerXml = "";
                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "ResetContentsNode", ex, ""));
            }
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Gathers the content togethers and tidies it up where necessary.
        /// </summary>
        /// <remarks></remarks>
        public void Populate()
        {
            try
            {

                if (IsReady)
                {

                    // Build the page XML
                    _myWeb.InitializeVariables();
                    _myWeb.Open();
                    _myWeb.BuildPageXML();

                    // Initialise the Contents Node
                    ResetContentsNode();

                    // Get the ContentTypes as a list
                    string contentTypeSqlList = "";
                    foreach (string contentType in ContentTypes)
                    {

                        if (!string.IsNullOrEmpty(contentTypeSqlList))
                            contentTypeSqlList += ",";
                        contentTypeSqlList += SqlString(contentType);

                    }

                    // If pages are being referenced, then we need to get the structure
                    if (HasSourcePage)
                        _structureNode = _myWeb.GetStructureXML();

                    // Go and get the content
                    PopulateContent(contentTypeSqlList, Conversions.ToLong(Interaction.IIf(HasSourcePage, SourcePage, 0)));

                    // Clean up duplicates
                    RemoveDuplicateContent();

                    // Add Content Metadata
                    AddContentMetadata();

                    // Mark the content as syndicated
                    LogContent();

                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "Populate", ex, ""));
            }
        }

        /// <summary>
        /// Runs the content synidcation
        /// </summary>
        /// <remarks></remarks>
        public void Syndicate()
        {
            try
            {

                // Kick off the logging.
                Log(Cms.dbHelper.ActivityType.SyndicationStarted, _distributorTypesList);

                // Get the content
                Populate();

                // Syndicate the content
                if (IsContentsNodePopulated)
                {
                    // Log: begun
                    Log(Cms.dbHelper.ActivityType.SyndicationInProgress);

                    // Run through each distributor
                    foreach (Distributor distributorInstance in _distributors)
                    {

                        distributorInstance.ContentsNode = ContentsNode;
                        distributorInstance.Syndicate();

                        // Update the completion totals.
                        if (distributorInstance.IsCompleted)
                        {
                            _totalCompleted += 1;
                        }
                        else
                        {
                            _hasFailures = true;
                        }

                        _diagnostics += distributorInstance.Diagnostics + Constants.vbCrLf;

                    }

                    // At the end indicate partial or full completion or total failure
                    // Add the diagnostics to the log
                    if (_hasFailures & _totalCompleted > 0)
                    {
                        Log(Cms.dbHelper.ActivityType.SyndicationPartialSuccess, _diagnostics);
                        _diagnostics = "Partial Completion (Failure detail):" + Constants.vbCrLf + _diagnostics;
                    }
                    else if (_hasFailures)
                    {
                        Log(Cms.dbHelper.ActivityType.SyndicationFailed, _diagnostics);
                        _diagnostics = "Syndication Failed (Detail):" + Constants.vbCrLf + _diagnostics;
                    }
                    else
                    {
                        Log(Cms.dbHelper.ActivityType.SyndicationCompleted, _diagnostics);
                        _isCompleted = true;

                    }
                }

                else
                {
                    _isCompleted = true;
                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "Syndicate", ex, ""));
            }
        }

        #endregion

        #region Shared Members

        /// <summary>
        /// Checks whether a submitted string is a valid Distributor
        /// </summary>
        /// <param name="possibleDistributor">The distributor name to check.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static bool IsDistributor(string possibleDistributor)
        {
            try
            {
                return Array.IndexOf(distributorTypesList, possibleDistributor) >= distributorTypesList.GetLowerBound(0);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion


        /// <summary>
        ///    Distributor is an abstract (base) class for all distributors.
        ///    It contains common properties.
        /// </summary>
        /// <remarks></remarks>
        public abstract class Distributor
        {

            #region Declarations

            private Protean.Cms _myWeb;
            private string _moduleName = "Eonic.Syndication.Distributor";

            // Core variables
            private XmlElement _contentsNode = null;
            private string _transformedData = "";
            private Config _config = null;
            private string _diagnostics = "";


            // Flags
            private bool _usesXslTransform = false;
            private bool _transformExists = false;
            private bool _transformCompleted = false;
            private bool _usesExtendedConfig = false;
            private bool _completed = false;

            #endregion

            #region Events
            public new event OnErrorEventHandler OnError;

            public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

            private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
            {
                _diagnostics += Constants.vbCrLf + "Error:" + e.ToString();
                OnError?.Invoke(sender, e);
            }
            #endregion

            #region Constructor
            public Distributor(ref Protean.Cms aWeb)
            {
                try
                {
                    _myWeb = aWeb;
                    _moduleName += "." + Name;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""));
                }

            }
            #endregion

            #region Private Properties

            /// <summary>
            /// The name of the distributor
            /// </summary>
            /// <returns>String</returns>
            /// <remarks></remarks>
            private string Name
            {
                get
                {
                    return GetType().Name;
                }
            }

            /// <summary>
            /// If xsl is being used, this stores the transformation output
            /// </summary>
            /// <value></value>
            /// <returns></returns>
            /// <remarks></remarks>
            private string TransformedData
            {
                get
                {
                    return _transformedData;
                }
                set
                {
                    _transformedData = value;
                }
            }

            private bool TransformCompleted
            {
                get
                {
                    return _transformCompleted;
                }
                set
                {
                    _transformCompleted = value;
                }
            }

            private bool UsesXslTransform
            {
                get
                {
                    return _usesXslTransform;
                }
            }

            #endregion

            #region Public Properties

            public XmlElement ContentsNode
            {
                get
                {
                    return _contentsNode;
                }
                set
                {
                    _contentsNode = value;
                }
            }

            public string Diagnostics
            {
                get
                {
                    return Name + ":" + _diagnostics;
                }
            }

            public Config ExtendedConfig
            {
                get
                {
                    if (_config is null | !UsesExtendedConfig)
                    {
                        return null;
                    }
                    else
                    {
                        return _config;
                    }
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return _completed;
                }
            }

            public bool UsesExtendedConfig
            {
                get
                {
                    return _usesExtendedConfig;
                }

            }

            #endregion

            #region Private Members
            private bool HasContent
            {
                get
                {
                    return !(ContentsNode is null || ContentsNode.SelectNodes("Content").Count == 0);
                }
            }

            private bool TransformExists
            {
                get
                {
                    return _transformExists;
                }
                set
                {
                    _transformExists = value;
                }
            }
            #endregion

            #region Public Members

            protected abstract void RunSyndicate();

            /// <summary>
            /// If the Distributor syndication is acheived partially through transforming the Contents by XSL
            /// then this function will look for a local and then common distributor-specific xsl
            /// </summary>
            /// <remarks></remarks>
            public virtual void Transform()
            {
                try
                {

                    if (UsesXslTransform)
                    {
                        bool testTransformExists = true;

                        string xslPath = "/xsl/Syndication/" + Name + ".xsl";

                        // Does a local xsl exist?
                        if (!File.Exists(_myWeb.goServer.MapPath(xslPath)))
                        {
                            xslPath = "/ewcommon" + xslPath;
                            if (!File.Exists(_myWeb.goServer.MapPath(xslPath)))
                            {
                                testTransformExists = false;
                            }
                        }

                        TransformExists = testTransformExists;

                        // If it doesn't exist then we can't proceed.
                        if (TransformExists)
                        {

                            var xslTransform = new Protean.XmlHelper.Transform();
                            TextWriter output = new StringWriter();

                            xslTransform.XSLFile = _myWeb.goServer.MapPath(xslPath);
                            xslTransform.Compiled = false;
                            xslTransform.Process(ContentsNode.OwnerDocument, ref output);
                            if (xslTransform.HasError)
                                throw new Exception("There was an error transforming the distributor");

                            // Run transformation
                            TransformedData = output.ToString();
                            output.Close();
                            output = null;

                            TransformCompleted = true;
                        }
                        else
                        {
                            // Flag up that it hasn't run
                            TransformCompleted = false;
                        }
                    }
                }

                catch (Exception ex)
                {
                    TransformCompleted = false;
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "Transform", ex, ""));
                }
            }

            /// <summary>
            /// Runs the Syndication
            /// </summary>
            /// <remarks></remarks>
            public void Syndicate()
            {
                try
                {
                    _completed = false;
                    if (!UsesExtendedConfig || UsesExtendedConfig && ExtendedConfig.IsLoaded)
                    {
                        if (HasContent)
                        {
                            Transform();
                            RunSyndicate();
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "Syndicate", ex, ""));
                }


            }

            /// <summary>
            /// This routine is for loading configs - it tries to find a Distributor node with the Distributor name
            /// </summary>
            /// <remarks></remarks>
            public void LoadConfig(XmlElement value)
            {
                try
                {
                    if (UsesExtendedConfig)
                    {
                        // Try to find the current provider
                        XmlElement distributorSettings = null;
                        if (Xml.NodeState(ref value, "Distributor[@name='" + Name + "']", "", "", 1, distributorSettings, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                        {
                            _config = new Config(distributorSettings);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "LoadConfig", ex, ""));
                }

            }
            #endregion

            // Example config for GenericXmlRpc
            // <Distributor name="GenericXmlRpc"><add key="methodName" value="weblogUpdates.ping" /><add key="endpoint" value="http://myrpcexample/rpc" /><add key="contentType" value="text/xml"></add></Distributor>

            /// <summary>
            /// GenericXmlRpc is a generic ping distributor, that is inherited by other xmlrpc ping distributors
            /// It can work in its own right if config settings are passed through for it.
            /// See the example in the code
            /// </summary>
            /// <remarks></remarks>
            public class GenericXmlRpc : Distributor
            {

                protected string _endpoint = "";
                protected string _contentType = "text/xml";
                protected string _methodName = "weblogUpdates.ping";

                public GenericXmlRpc(ref Protean.Cms aWeb) : base(ref aWeb)
                {
                    try
                    {
                        _usesExtendedConfig = true;
                        _usesXslTransform = true;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""));
                    }
                }

                public override void Transform()
                {
                    try
                    {
                        if (_usesExtendedConfig)
                        {
                            if (!string.IsNullOrEmpty(ExtendedConfig.GetValue("methodName")))
                                _methodName = ExtendedConfig.GetValue("methodName");
                            if (!string.IsNullOrEmpty(ExtendedConfig.GetValue("endpoint")))
                                _endpoint = ExtendedConfig.GetValue("endpoint");
                            if (!string.IsNullOrEmpty(ExtendedConfig.GetValue("contentType")))
                                _contentType = ExtendedConfig.GetValue("contentType");
                        }

                        ContentsNode.OwnerDocument.DocumentElement.SetAttribute("xmlRpcMethodName", _methodName);
                        base.Transform();
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "Transform", ex, ""));
                    }

                }

                /// <summary>
                ///    Syndicate Content for Generic XmlRpc interfaces
                /// </summary>
                /// <remarks></remarks>
                protected override void RunSyndicate()
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(_methodName) & !string.IsNullOrEmpty(_endpoint) & !string.IsNullOrEmpty(_contentType))


                        {
                            // Define the request
                            var syndicateRequest = new Tools.Http.WebRequest(_contentType, "POST", "EonicWeb");

                            // Send the request and get the response
                            _diagnostics = syndicateRequest.Send(_endpoint, TransformedData);

                            // Weblogs indicated success with boolean value of 0.
                            // Rather than parse this, we can simply search the string for an expected repsonse.
                            if (_diagnostics.Contains("<boolean>0</boolean>"))
                            {
                                _completed = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "RunSyndicate", ex, ""));
                    }

                }
            }

            public class Weblogs : GenericXmlRpc
            {

                public Weblogs(ref Protean.Cms aWeb) : base(ref aWeb)
                {
                    try
                    {
                        _usesXslTransform = true;
                        _usesExtendedConfig = false;
                        _endpoint = "http://rpc.weblogs.com/RPC2";
                        _methodName = "weblogUpdates.extendedPing";
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""));
                    }
                }

            }

            public class Technorati : GenericXmlRpc
            {

                public Technorati(ref Protean.Cms aWeb) : base(ref aWeb)
                {
                    try
                    {
                        _usesXslTransform = true;
                        _usesExtendedConfig = false;
                        _endpoint = "http://rpc.technorati.com/rpc/ping";
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""));
                    }
                }

            }

            public class PingOMatic : GenericXmlRpc
            {

                public PingOMatic(ref Protean.Cms aWeb) : base(ref aWeb)
                {
                    try
                    {
                        _usesXslTransform = true;
                        _usesExtendedConfig = false;
                        _endpoint = "http://rpc.pingomatic.com/";
                        _methodName = "weblogUpdates.extendedPing";
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""));
                    }
                }

            }

            // Class Eonic.Syndication.Distributor.Config

            // Lightweight xml config loader that picks up xml in web.config form
            // e.g. <mySettings>
            // <add key="myKey" value="myValue"/>
            // </mySettings>
            /// <summary>
            /// Class Eonic.Syndication.Distributor.Config
            /// Lightweight xml config loader that picks up xml in web.config form
            /// </summary>
            /// <remarks></remarks>
            public class Config
            {

                private string _moduleName = "Eonic.Syndication.Distributor.Config";
                private XmlElement _settings = null;
                private bool _isLoaded = false;

                #region Events
                public new event OnErrorEventHandler OnError;

                public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);

                private void _OnError(object sender, Tools.Errors.ErrorEventArgs e)
                {
                    OnError?.Invoke(sender, e);
                }
                #endregion

                public Config()
                {
                    _isLoaded = false;
                }

                public Config(XmlElement newSettings)
                {
                    Settings = newSettings;
                }

                public bool IsLoaded
                {
                    get
                    {
                        return _isLoaded;
                    }
                }

                public XmlElement Settings
                {
                    get
                    {
                        return _settings;
                    }
                    set
                    {
                        try
                        {

                            _settings = value;
                            _isLoaded = true;
                        }

                        catch (Exception ex)
                        {
                            _settings = null;
                            _isLoaded = false;
                        }
                    }
                }

                public string GetValue(string key)
                {
                    try
                    {
                        XmlElement setting = null;
                        XmlNodeState localNodeState() { var argoNode = Settings; var ret = Xml.NodeState(ref argoNode, "add[@key='" + key + "']", "", "", 1, setting, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false); Settings = argoNode; return ret; }

                        if (localNodeState() != XmlNodeState.NotInstantiated)
                        {
                            return setting.GetAttribute("value");
                        }
                        else
                        {
                            return "";
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(_moduleName, "New", ex, ""));
                        return "";
                    }
                }
            }
        }

    }
}