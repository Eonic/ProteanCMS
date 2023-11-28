using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
// regular expressions
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
// This is the Indexer/Search items
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;

using static Protean.Tools.Number;
using static Protean.Tools.Xml;

namespace Protean
{

    public partial class Cms
    {

        public class Search
        {

            protected XmlDocument moPageXml; // the actual page, given from the web object
            private string mcIndexFolder = "";
            private new string mcModuleName = "Eonic.Search.Search"; // module name
            public XmlElement moContextNode;
            public Cms myWeb;
            public NameValueCollection moConfig;

            // Settings - some of these are specific to search type
            // Ideally I would like to extract search types into separate classes

            private bool _includeFuzzySearch = false;
            private bool _runPreFuzzySearch = false;
            private bool _includePrefixNameSearch = false;
            private bool _overrideQueryBuilder = false;
            private int _pagingDefaultSize = 300;
            private string _indexReadFolder = "";
            private string _indexPath = "../Index";
            private bool _logSearches = false;

            public class Modules
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Protean.Cms.Search.Modules";

                public Modules()
                {
                    // do nowt
                }

                public void GetResults(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    myWeb.PerfMon.Log("Search", "New");
                    Search oSrch;
                    try
                    {
                        oSrch = new Search(ref myWeb);
                        oSrch.moContextNode = oContentNode;

                        string searchMode = myWeb.moRequest["searchMode"];
                        if (string.IsNullOrEmpty(searchMode))
                            oContentNode.GetAttribute("searchMode");

                        string fuzzySearch = myWeb.moRequest["fuzzysearch"];

                        string contentType = myWeb.moRequest["contentType"];
                        if (string.IsNullOrEmpty(contentType))
                            oContentNode.GetAttribute("contentType");

                        switch (Strings.UCase(searchMode) ?? "")
                        {
                            case "REGEX":
                            case var @case when @case == "":
                                {
                                    // RegEx
                                    oSrch.RegExpQuery(myWeb.moRequest["searchString"], contentType);
                                    break;
                                }
                            case "XPATH":
                                {
                                    // xPath
                                    oSrch.XPathQuery(myWeb.moRequest["searchString"], contentType);
                                    break;
                                }
                            case "INDEX":
                                {
                                    // Searches an index of the entire site.
                                    oSrch.IndexQuery(myWeb.moRequest["searchString"], fuzzySearch);
                                    break;
                                }
                            case "USER":
                                {
                                    oSrch.RegExpQuery(myWeb.moRequest["searchString"], myWeb.moRequest["groupId"], true);
                                    break;
                                }
                            case "LATESTCONTENT":
                                {
                                    oSrch.ContentByDateQuery(contentType, myWeb.moRequest["searchDateUnit"], myWeb.moRequest["searchDateUnitTotal"], myWeb.moRequest["searchStartDate"], myWeb.moRequest["searchEndDate"], myWeb.moRequest["searchColumn"]);
                                    break;
                                }
                            case "BESPOKE":
                                {
                                    oSrch.BespokeQuery();
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                                // Do nothing
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetResults", ex, "", bDebug: gbDebug);
                    }
                }

            }

            public Search(ref Cms aWeb)
            {
                try
                {

                    // Set the global variables
                    myWeb = aWeb;
                    myWeb.PerfMon.Log("Search", "New");
                    moPageXml = myWeb.moPageXml;
                    moConfig = myWeb.moConfig;

                    // Read settings in from the config

                    // General settings
                    _logSearches = moConfig["SearchLogging"] == "on";
                    _includeFuzzySearch = moConfig["SearchFuzzy"] == "on";
                    _runPreFuzzySearch = moConfig["SearchGetFuzzyCount"] == "on";

                    string pageDefaultSize = moConfig["SearchDefaultPageSize"] + "";
                    _pagingDefaultSize = ConvertStringToIntegerWithFallback(pageDefaultSize, _pagingDefaultSize);

                    // Site Search specifics
                    if (moConfig["SiteSearch"] == "on")
                    {
                        // Search path with default
                        if (!string.IsNullOrEmpty(moConfig["SiteSearchPath"] + ""))
                            _indexPath = moConfig["SiteSearchPath"];
                        _indexPath = _indexPath.TrimEnd(@"/\".ToCharArray()) + "/";
                        _indexReadFolder = myWeb.goServer.MapPath("/") + _indexPath;

                        // Search read path
                        if (string.IsNullOrEmpty(moConfig["SiteSearchReadPath"]))
                        {
                            _indexReadFolder += "Read/";
                        }
                        else
                        {
                            _indexReadFolder = myWeb.goServer.MapPath("/") + moConfig["SiteSearchReadPath"];
                            _indexReadFolder = _indexReadFolder.TrimEnd(@"/\".ToCharArray()) + "/";
                        }

                        mcIndexFolder = _indexReadFolder;


                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", bDebug: gbDebug);
                }
            }

            public Search(ref Protean.Rest myAPi)
            {
                // myAPi.PerfMon.Log("Search", "New")
                try
                {

                    // Set the global variables

                    moConfig = myAPi.moConfig;

                    // Read settings in from the config

                    // General settings
                    _logSearches = moConfig["SearchLogging"] == "on";
                    _includeFuzzySearch = moConfig["SearchFuzzy"] == "on";
                    _runPreFuzzySearch = moConfig["SearchGetFuzzyCount"] == "on";

                    string pageDefaultSize = moConfig["SearchDefaultPageSize"] + "";
                    _pagingDefaultSize = ConvertStringToIntegerWithFallback(pageDefaultSize, 25);

                    // Site Search specifics
                    if (moConfig["SiteSearch"] == "on")
                    {
                        // Search path with default
                        if (!string.IsNullOrEmpty(moConfig["SiteSearchPath"] + ""))
                            _indexPath = moConfig["SiteSearchPath"];
                        _indexPath = _indexPath.TrimEnd(@"/\".ToCharArray()) + "/";
                        _indexReadFolder = myAPi.goServer.MapPath("/") + _indexPath;

                        // Search read path
                        if (string.IsNullOrEmpty(moConfig["SiteSearchReadPath"]))
                        {
                            _indexReadFolder += "Read/";
                        }
                        else
                        {
                            _indexReadFolder = myWeb.goServer.MapPath("/") + moConfig["SiteSearchReadPath"];
                            _indexReadFolder = _indexReadFolder.TrimEnd(@"/\".ToCharArray()) + "/";
                        }

                        mcIndexFolder = _indexReadFolder;


                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", bDebug: gbDebug);
                }
            }


            public virtual void apply()
            {
                myWeb.PerfMon.Log("Search", "apply");
                string sXpath = "";
                string sSearch = myWeb.moRequest["searchString"];
                string[] aNewSearchWords = null;
                string sError = "";
                string cSearchTerm = "";
                bool bFirst = true;
                bool bSearchSubmitted = false;

                string sProcessInfo = "Search Routing";

                try
                {
                    var dStartTime = DateTime.Now;

                    // Get a context element
                    // This is the last element added tothe Contents Node, if applicable - otherwise create the Contents Node
                    // This gives us a marker when constructing a list of what was added to the Contents node by the search
                    moContextNode = (XmlElement)moPageXml.SelectSingleNode("/Page/Contents");
                    if (moContextNode is null)
                    {
                        moContextNode = moPageXml.CreateElement("Contents");
                        moPageXml.DocumentElement.AppendChild(moContextNode);
                    }

                    // If Not (oContextElmt.LastChild Is Nothing) Then oContextElmt = oContextElmt.LastChild

                    // There are five types of search:
                    // 
                    // CACHED ::         This is a search that has already been submitted and the user is simply navigating to 
                    // another page of these results.  These use content IDs stored in a session variable to 
                    // retrieve content.
                    // 
                    // XPATH ::          This search uses XPath in the SQL statement to filter out the search terms.  It is 
                    // slow, but can be used to query specific parts of a content node, rather than all the nodes.
                    // 
                    // REGEX ::          This search uses a combination of approximate SQL filtering followed by refined Regular
                    // Expression comparison.  It is very fast, but can only be executed on all the nodes in the 
                    // content node, which is usually what most people want.
                    // This is turned on by adding the config item ewSearchMode = "RegEx"
                    // 
                    // MSINDEX ::        This uses Microsoft Indexing Service to do a compeltely different, much more page content
                    // focussed search.  Trevor knows more about this. (DEPRICATED)
                    // 
                    // INDEX ::          This uses the dot.Lucene Library from apache ported to C# an index is created from firing 
                    // off the indexing routine from the admin area

                    // BESPOKE ::        Allows the search to be overridden



                    switch (myWeb.moRequest["searchMode"] ?? "")
                    {
                        case "REGEX":
                        case var @case when @case == "":
                            {
                                // RegEx
                                RegExpQuery(myWeb.moRequest["searchString"], myWeb.moRequest["contentType"]);
                                break;
                            }
                        case "XPATH":
                            {
                                // xPath
                                XPathQuery(myWeb.moRequest["searchString"], myWeb.moRequest["contentType"]);
                                break;
                            }
                        case "INDEX":
                            {
                                // Searches an index of the entire site.
                                IndexQuery(myWeb.moRequest["searchString"]);
                                break;
                            }
                        case "USER":
                            {
                                RegExpQuery(myWeb.moRequest["searchString"], myWeb.moRequest["groupId"], true);
                                break;
                            }
                        case "LATESTCONTENT":
                            {
                                ContentByDateQuery(myWeb.moRequest["contentType"], myWeb.moRequest["searchDateUnit"], myWeb.moRequest["searchDateUnitTotal"], myWeb.moRequest["searchStartDate"], myWeb.moRequest["searchEndDate"], myWeb.moRequest["searchColumn"]);
                                break;
                            }
                        case "BESPOKE":
                            {
                                BespokeQuery();
                                break;
                            }

                        default:
                            {
                                break;
                            }
                            // Do nothing
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ExecuteSearch", ex, "", sProcessInfo, gbDebug);
                }
            }


            #region Search Methods

            protected XmlElement BuildFiltersFromRequest(ref XmlDocument XmlDoc, ref HttpRequest currentRequest)
            {
                myWeb.PerfMon.Log("Search", "BuildFiltersFromRequest");
                string processInfo = "";

                XmlElement filters;
                XmlElement filter;

                int requestItemsLength;

                try
                {

                    // Create filter metadata for search criteria based on the request
                    // The request should be in the form <reserved_parameter>_ordinal
                    // reserved parameters are:
                    // sf-name - the field name
                    // sf-value - the value to search for - if this equals searchstring, then use the main searchstring
                    // sf-min - for range queries, the min
                    // sf-max - for range queries, the max
                    // sf-type - the data type of the field "string","number" or "float"

                    filters = XmlDoc.CreateElement("Filters");

                    requestItemsLength = currentRequest.QueryString.AllKeys.Length + currentRequest.Form.AllKeys.Length;

                    if (requestItemsLength > 0)
                    {

                        var formsAndQueryStringKeys = new string[requestItemsLength];
                        Array.Copy(currentRequest.QueryString.AllKeys, 0, formsAndQueryStringKeys, 0, currentRequest.QueryString.AllKeys.Length);
                        Array.Copy(currentRequest.Form.AllKeys, 0, formsAndQueryStringKeys, currentRequest.QueryString.AllKeys.Length, currentRequest.Form.AllKeys.Length);

                        string requestOrdinal = "";

                        bool addFilter;

                        string fieldName = "";
                        string fieldValue = "";
                        string fieldMin = "";
                        string fieldMax = "";
                        string fieldType = "";
                        string[] fieldTextTerms = null;


                        foreach (string requestKey in formsAndQueryStringKeys)
                        {

                            if (requestKey != null)
                            {
                                if (requestKey.StartsWith("sf-name-") & !string.IsNullOrEmpty(currentRequest[requestKey]))
                                {

                                    addFilter = false;

                                    // Get the field data
                                    requestOrdinal = requestKey.Substring(8);
                                    fieldName = currentRequest[requestKey];
                                    fieldValue = currentRequest["sf-value-" + requestOrdinal];
                                    fieldMin = currentRequest["sf-min-" + requestOrdinal];
                                    fieldMax = currentRequest["sf-max-" + requestOrdinal];
                                    fieldType = currentRequest["sf-type-" + requestOrdinal];

                                    filter = filters.OwnerDocument.CreateElement("Filter");
                                    filter.SetAttribute("ordinal", requestOrdinal.ToString());
                                    filter.SetAttribute("field", currentRequest[requestKey]);
                                    filter.SetAttribute("fieldType", fieldType);

                                    // Determine the type of query
                                    // If min or max are populated then it's a range query
                                    // If value is populated then it's a term query
                                    if (!string.IsNullOrEmpty(fieldMin) | !string.IsNullOrEmpty(fieldMax))
                                    {
                                        // Range query
                                        filter.SetAttribute("type", "range");
                                        filter.SetAttribute("min", fieldMin);
                                        filter.SetAttribute("max", fieldMax);
                                        addFilter = true;
                                    }
                                    else if (!string.IsNullOrEmpty(fieldValue))
                                    {
                                        // term query
                                        filter.SetAttribute("type", "term");
                                        filter.SetAttribute("value", fieldValue);
                                        addFilter = true;
                                    }

                                    if (addFilter)
                                    {
                                        filters.AppendChild(filter);
                                    }

                                }
                            }

                        }
                    }

                    return filters;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "BuildFiltersFromRequest", ex, "", processInfo, gbDebug);
                    return null;
                }
            }


            public void IndexQuery(string cQuery, string fuzzySearch = "on")
            {
                myWeb.PerfMon.Log("Search", "IndexQuery");
                string processInfo = "Looking for : " + cQuery;
                try
                {

                    int resultsCount = 0;
                    var resultsXML = moContextNode.OwnerDocument.CreateElement("Content");
                    int HitsLimit;
                    int command;
                    int Page = 0;
                    int NextPage = 0;
                    // Dim pathList As String() = Split(CleanSearchString(HttpContext.Current.Request.Url.Host), "?")

                    Document resultDoc = null;
                    XmlElement result = null;
                    Field pageIdField = null;
                    long pageId = 0L;
                    string url = "";
                    XmlElement menuItem = null;
                    string[] reservedFieldNames = new string[] { "type", "text", "abstract" };
                    int pageStart = Math.Max(myWeb.GetRequestItemAsInteger("pageStart", 1), 1);
                    int pageSize = myWeb.GetRequestItemAsInteger("pageSize", _pagingDefaultSize);
                    int pageEnd;
                    long totalResults = 0L;
                    // allow paging as per config setting 
                    // If myWeb.moConfig("SiteSearchIndexResultPaging") IsNot Nothing And (myWeb.moConfig("SiteSearchIndexResultPaging") = "on") Then 'allow paging for search index page result
                    if (myWeb.moConfig["SearchDefaultPageSize"] != null) // allow paging for search index page result
                    {
                        if (pageStart > 1)
                        {
                            HitsLimit = pageStart + pageSize;
                        }
                        else
                        {
                            HitsLimit = pageSize;
                        } // first load as per page count
                        if (Convert.ToInt32(myWeb.moRequest["PageSize"]) > 0)
                        {
                            pageSize = Convert.ToInt32(myWeb.moRequest["PageSize"]);
                        }
                        if (Convert.ToInt32(myWeb.moRequest["command"]) > 0)
                        {
                            command = Convert.ToInt32(myWeb.moRequest["command"]);
                        }
                        else
                        {
                            command = 0;
                        }
                        if (Convert.ToInt32(myWeb.moRequest["page"]) > 0)
                        {
                            Page = Convert.ToInt32(myWeb.moRequest["page"]);
                        }
                    }

                    else // search index result will show without pagination and records will load as per config setting value
                    {
                        HitsLimit = Convert.ToInt32("0" + myWeb.moConfig["SearchDefaultPageSize"]); // 300
                        if (HitsLimit == 0)
                            HitsLimit = 300;
                        pageSize = Convert.ToInt32("0" + myWeb.moConfig["SearchDefaultPageSize"]);
                        if (pageSize == 0)
                            pageSize = 300;
                    }


                    if (!cQuery.Equals(""))
                    {

                        // do the actual search
                        var dateStart = DateTime.Now;
                        DateTime dateFinish;
                        var oIndexDir = new DirectoryInfo(mcIndexFolder);
                        var fsDir = FSDirectory.Open(oIndexDir);
                        var searcher = new IndexSearcher(fsDir, true);

                        // check whether logged in user is csuser and skip checking status
                        bool bShowHiddenForUser = false; // set for normal user default value
                        if (myWeb.moConfig["UserRoleAllowedHiddenProductSearch"] != null)
                        {
                            int nUserId = Convert.ToInt32(myWeb.moSession["nUserId"]);
                            bShowHiddenForUser = myWeb.moDbHelper.checkUserRole(myWeb.moConfig["UserRoleAllowedHiddenProductSearch"], "Role", nUserId);
                        }

                        // Check for settings
                        if (fuzzySearch != "off")
                        {
                            if ((myWeb.moConfig["SiteSearchFuzzy"].ToLower()) == "on")
                                _includeFuzzySearch = true;
                        }

                        // If myWeb.moRequest("fuzzySearch") = "on" Then _includeFuzzySearch = True
                        // If myWeb.moRequest("fuzzySearch") = "off" Then _includeFuzzySearch = False
                        if (bShowHiddenForUser)
                        {
                            _includeFuzzySearch = false; // to get exact matching result
                                                         // keep page size for csuser as default
                            HitsLimit = Convert.ToInt32("0" + myWeb.moConfig["SiteSearchDefaultHitsLimit"]); // 300
                            if (HitsLimit == 0)
                                HitsLimit = 300;
                            pageSize = Convert.ToInt32("0" + myWeb.moConfig["SiteSearchDefaultHitsLimit"]);
                            if (pageSize == 0)
                                pageSize = 300;
                        }
                        else
                        {
                            _includeFuzzySearch = true;
                        }

                        _overrideQueryBuilder = myWeb.moRequest["overrideQueryBuilder"] == "true";
                        _includePrefixNameSearch = myWeb.moRequest["prefixNameSearch"] == "true";

                        resultsXML.SetAttribute("fuzzy", Conversions.ToString(Interaction.IIf(_includeFuzzySearch, "on", "off")));
                        resultsXML.SetAttribute("prefixNameSearch", Conversions.ToString(Interaction.IIf(_includePrefixNameSearch, "true", "false")));

                        // Generate the live page filter
                        var livePages = LivePageLuceneFilter();

                        // Generate the query criteria
                        var argXmlDoc = resultsXML.OwnerDocument;
                        var searchFilters = BuildFiltersFromRequest(ref argXmlDoc, ref myWeb.moRequest);
                        if (searchFilters != null)
                            resultsXML.AppendChild(searchFilters);

                        // Generate the search query
                        var searchQuery = BuildLuceneQuery(cQuery, searchFilters, bShowHiddenForUser);

                        // Add a sort from the request
                        var queryOrder = SetSortFieldFromRequest(myWeb.moRequest);

                        // Perform the search
                        TopDocs results;

                        if ((myWeb.moConfig["SiteSearchDebug"]).ToLower() == "on")
                        {
                            var argoParent = resultsXML;
                            addElement(ref argoParent, "LuceneQuery", searchQuery.ToString());
                            var argoParent1 = resultsXML;
                            addElement(ref argoParent1, "LuceneLivePageFilter", livePages.ToString());
                        }

                        if (livePages is null)
                        {
                            results = searcher.Search(searchQuery, HitsLimit);
                        }
                        else
                        {
                            results = searcher.Search(searchQuery, livePages, HitsLimit, queryOrder);
                        }

                        // Log the search
                        if (_logSearches)
                        {
                            myWeb.moDbHelper.logActivity((_includeFuzzySearch)? dbHelper.ActivityType.FuzzySearch: dbHelper.ActivityType.Search, myWeb.mnUserId, 0, 0, results.TotalHits.ToString(), cQuery);
                        }

                        // Optional fuzzysearch for figures
                        // This is simply designed to get a count that can be fed back to users
                        // This could be a performance no-no, although most searches are quick
                        if (_runPreFuzzySearch & !_includeFuzzySearch)
                        {
                            _includeFuzzySearch = true;
                            searchQuery = BuildLuceneQuery(cQuery, searchFilters);
                            resultsXML.SetAttribute("fuzzyCount", searcher.Search(searchQuery, livePages, HitsLimit, queryOrder).TotalHits.ToString());
                        }


                        // Paging settings
                        // Hits is so lightweight that we don't have to filter it beforehand
                        // See: http://wiki.apache.org/lucene-java/LuceneFAQ#How_do_I_implement_paging.2C_i.e._showing_result_from_1-10.2C_11-20_etc.3F
                        totalResults = results.TotalHits;


                        if (pageSize <= 0)
                        {
                            pageSize = results.TotalHits;
                        }

                        if (totalResults == 0L)
                        {
                            pageStart = (int)totalResults;
                            pageEnd = (int)totalResults;
                        }
                        else
                        {
                            pageEnd = (int)Math.Min(totalResults, pageStart + pageSize - 1);
                        }

                        if (pageEnd < pageStart)
                            pageEnd = pageStart;

                        if (totalResults > 0L & pageSize > 0)
                        {
                            int pageNumber = (int)(totalResults % pageSize);
                        }

                        resultsXML.SetAttribute("pageStart", pageStart.ToString()); // Start result number
                        resultsXML.SetAttribute("pageEnd", pageEnd.ToString()); // End result number
                        resultsXML.SetAttribute("nextPage", (pageEnd + 1).ToString());

                        resultsXML.SetAttribute("sortCol", myWeb.moRequest["sortCol"]);
                        resultsXML.SetAttribute("sortColType", myWeb.moRequest["sortColType"]);
                        resultsXML.SetAttribute("sortDir", myWeb.moRequest["sortDir"]);


                        resultsXML.SetAttribute("pageSize", pageSize.ToString()); // Max Number of items per page
                        resultsXML.SetAttribute("totalPages", Math.Ceiling(totalResults / (double)pageSize).ToString());

                        // Don't believe this to be required.
                        if (myWeb.moConfig["SearchDefaultPageSize"] != null)
                        {
                            resultsXML.SetAttribute("sitePaging", "on");
                            if (bShowHiddenForUser)
                            {
                                resultsXML.SetAttribute("sitePaging", "off");
                            }
                        }
                        else
                        {
                            resultsXML.SetAttribute("sitePaging", "off");
                        }

                        resultsXML.SetAttribute("Hits", HitsLimit.ToString());

                        if (myWeb.moConfig["SearchDefaultPageSize"] != null) // allow paging for search index page result
                        {
                            resultsXML.SetAttribute("startCount", (HitsLimit - pageSize + 1).ToString());
                        }

                        var artIdResults = new List<long>();

                        // Process the results
                        if (totalResults > 0L)
                        {
                            int skipRecords = Convert.ToInt32(myWeb.moRequest["page"]) * pageSize;
                            // Dim luceneDocuments As IList(Of Document) = New List(Of Document)()
                            ScoreDoc[] scoreDocs = results.ScoreDocs;

                            string thisArtIdList = "";
                            // For Each sDoc In results.ScoreDocs()

                            // resultDoc = searcher.Doc(sDoc.Doc)
                            for (int i = pageStart - 1, loopTo = results.TotalHits - 1; i <= loopTo; i++)
                            {

                                if (i > pageStart - 1 + pageSize - 1)
                                {
                                    break;
                                }

                                resultDoc = searcher.Doc(scoreDocs[i].Doc);
                                pageIdField = resultDoc.GetField("pgid");
                                if (pageIdField != null && IsStringNumeric(pageIdField.StringValue))
                                {
                                    pageId = Convert.ToInt32(pageIdField.StringValue);
                                }
                                else
                                {
                                    pageId = 0L;
                                }

                                url = ""; // this is the link for the page

                                // Get the menuitem element from the xml
                                var argoNode = moPageXml.DocumentElement;
                                if (Tools.Xml.NodeState(ref argoNode, "/Page/Menu/descendant-or-self::MenuItem[@id=" + pageId + "]", "", "", XmlNodeState.IsEmpty, menuItem, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                {

                                    // don't add artId more than twice to results.
                                    long thisArtId;

                                    if (resultDoc.GetField("artid") != null)
                                    {
                                        thisArtId = Conversions.ToInteger(resultDoc.GetField("artid").StringValue);

                                        if (string.IsNullOrEmpty(thisArtIdList))
                                        {
                                            thisArtIdList = thisArtId.ToString();
                                        }
                                        else
                                        {
                                            thisArtIdList = thisArtIdList + "," + thisArtId;
                                        }
                                    }
                                }

                                else
                                {
                                    // Couldn't find the menuitme in the xml - which is odd given the livepagefilter
                                    processInfo = "not found in live page filter";
                                }

                            }

                            // 'check whether logged in user is csuser and skip checking status
                            // Dim bShowHiddenForUser As Boolean = False 'set for normal user default value
                            // If myWeb.moConfig("UserRoleAllowedHiddenProductSearch") IsNot Nothing Then
                            // Dim nUserId As Integer = myWeb.moSession("nUserId")
                            // bShowHiddenForUser = myWeb.moDbHelper.checkUserRole(myWeb.moConfig("UserRoleAllowedHiddenProductSearch"), "Role", nUserId)
                            // End If
                            // check artid/product is active
                            if (!bShowHiddenForUser & !string.IsNullOrEmpty(thisArtIdList))
                            {
                                thisArtIdList = myWeb.CheckProductStatus(thisArtIdList);
                            }

                            for (int i = pageStart - 1, loopTo1 = results.TotalHits - 1; i <= loopTo1; i++)
                            {

                                if (i > pageStart - 1 + pageSize - 1)
                                {
                                    break;
                                }

                                resultDoc = searcher.Doc(scoreDocs[i].Doc);
                                pageIdField = resultDoc.GetField("pgid");
                                if (pageIdField != null && IsStringNumeric(pageIdField.StringValue))
                                {
                                    pageId = Convert.ToInt32(pageIdField.StringValue);
                                }
                                else
                                {
                                    pageId = 0L;
                                }

                                url = ""; // this is the link for the page

                                // Get the menuitem element from the xml
                                var argoNode1 = moPageXml.DocumentElement;
                                if (Tools.Xml.NodeState(ref argoNode1, "/Page/Menu/descendant-or-self::MenuItem[@id=" + pageId + "]", "", "", XmlNodeState.IsEmpty, menuItem, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                {

                                    // Only process this result if it's in the paging zone
                                    // pageStart - 1 To pageEnd - 1

                                    // don't add artId more than twice to results.
                                    long thisArtId = 0L;

                                    if (resultDoc.GetField("artid") != null)
                                    {
                                        thisArtId = Conversions.ToInteger(resultDoc.GetField("artid").StringValue);
                                    }

                                    if (thisArtId == 0L | thisArtIdList.Contains(thisArtId.ToString()))
                                    {
                                        if (thisArtId == default | !artIdResults.Exists(x => x == thisArtId))
                                        {
                                            if (thisArtId != default)
                                                artIdResults.Add(thisArtId);

                                            url = resultDoc.GetField("url").StringValue + "";

                                            // Build the URL
                                            if (string.IsNullOrEmpty(url))
                                            {
                                                url = menuItem.GetAttribute("url");
                                                // Add the artId, if exists
                                                if (resultDoc.GetField("artid") != null)
                                                {
                                                    if (resultDoc.GetField("contenttype") != null && resultDoc.GetField("contenttype").StringValue == "Download")
                                                    {
                                                        url = resultDoc.GetField("url").StringValue;
                                                    }
                                                    else if (moConfig["LegacyRedirect"] == "on")
                                                    {
                                                        url = Conversions.ToString(url + Operators.ConcatenateObject(Operators.ConcatenateObject(Interaction.IIf(url == "/", "", "/"), resultDoc.GetField("artid").StringValue), "-/"));

                                                        string artName = "";
                                                        if (resultDoc.GetField("name") != null)
                                                        {
                                                            artName = resultDoc.GetField("name").StringValue;
                                                            var oRe = new Regex("[^A-Z0-9]", RegexOptions.IgnoreCase);
                                                            artName = oRe.Replace(artName, "-").Trim('-');
                                                            url += artName;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        url = Conversions.ToString(url + Operators.ConcatenateObject(Operators.ConcatenateObject(Interaction.IIf(url == "/", "", "/"), "item"), resultDoc.GetField("artid").StringValue));
                                                    }

                                                }
                                            }

                                            result = moPageXml.CreateElement("Content");
                                            result.SetAttribute("type", "SearchResult");
                                            // result.SetAttribute("indexId", )
                                            // result.SetAttribute("indexRank", sDoc.Score)
                                            result.SetAttribute("indexRank", scoreDocs[i].Score.ToString());
                                            foreach (Field docField in resultDoc.GetFields())
                                            {

                                                // Don't add info to certain fields
                                                if (Array.IndexOf(reservedFieldNames, docField.Name) == -1)
                                                {
                                                    result.SetAttribute(docField.Name, docField.StringValue);
                                                }

                                                if (docField.Name == "abstract")
                                                {

                                                    // Try to output this as Xml
                                                    string innerString = docField.StringValue + "";
                                                    processInfo = innerString;
                                                    try
                                                    {
                                                        result.InnerXml = innerString.Trim();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        innerString = innerString.Replace("&", "&amp;").Replace("&amp;amp;", "&amp;").Trim();
                                                        processInfo = innerString;
                                                        result.InnerText = innerString;
                                                    }

                                                }
                                            }
                                            result.SetAttribute("url", url);

                                            moContextNode.AppendChild(result);
                                            resultsCount = resultsCount + 1;
                                        }
                                        else
                                        {
                                            processInfo = "this is a duplicate art id";
                                            totalResults = totalResults - 1L;
                                        }
                                    }
                                    else
                                    {
                                        processInfo = "this is a duplicate art id";
                                        totalResults = totalResults - 1L;
                                    }
                                }
                                else
                                {
                                    // Couldn't find the menuitme in the xml - which is odd given the livepagefilter
                                    processInfo = "not found in live page filter";
                                    totalResults = totalResults - 1L;
                                }
                            }

                        }

                        dateFinish = DateTime.Now;
                        resultsXML.SetAttribute("Time", dateFinish.Subtract(dateStart).TotalMilliseconds.ToString());
                    }
                    // resultsCount = results.TotalHits()
                    else
                    {
                        resultsXML.SetAttribute("Time", "0");
                    }
                    resultsXML.SetAttribute("totalResults", totalResults.ToString());
                    resultsXML.SetAttribute("searchString", cQuery);
                    resultsXML.SetAttribute("searchType", "INDEX");
                    resultsXML.SetAttribute("type", "SearchHeader");
                    resultsXML.SetAttribute("resultsReturned", (resultsCount + 1).ToString());

                    moContextNode.AppendChild(resultsXML);
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Search", ex, "", processInfo, gbDebug);
                }
            }

            public void IndexQuery(ref Protean.Rest myAPI, string cQuery, int HitsLimit = 300, string fuzzySearch = "on")
            {
                // myWeb.PerfMon.Log("Search", "IndexQuery")
                string processInfo = "Looking for : " + cQuery;
                try
                {
                    if (myWeb is null)
                    {
                        myWeb = new Cms();
                    }

                    int resultsCount = 0;
                    var resultsXML = moContextNode.OwnerDocument.CreateElement("Content");

                    if (!cQuery.Equals(""))
                    {

                        // do the actual search
                        var dateStart = DateTime.Now;
                        DateTime dateFinish;
                        var oIndexDir = new DirectoryInfo(mcIndexFolder);
                        var fsDir = FSDirectory.Open(oIndexDir);
                        var searcher = new IndexSearcher(fsDir, true);

                        // Check for settings
                        if (myAPI.moRequest["fuzzy"] == "on")
                            _includeFuzzySearch = true;
                        if (fuzzySearch != "off")
                        {
                            if ((myAPI.moConfig["SiteSearchFuzzy"]).ToLower() == "on")
                                _includeFuzzySearch = true;
                        }

                        _overrideQueryBuilder = myAPI.moRequest["overrideQueryBuilder"] == "true";
                        _includePrefixNameSearch = myAPI.moRequest["prefixNameSearch"] == "true";

                        resultsXML.SetAttribute("fuzzy", Conversions.ToString(Interaction.IIf(_includeFuzzySearch, "on", "off")));
                        resultsXML.SetAttribute("prefixNameSearch", Conversions.ToString(Interaction.IIf(_includePrefixNameSearch, "true", "false")));
                        // check whether logged in user is csuser and skip checking status
                        bool bShowHiddenForUser = false; // set for normal user default value
                        if (myWeb.moConfig["UserRoleAllowedHiddenProductSearch"] != null)
                        {
                            int nUserId = Convert.ToInt32(myWeb.moSession["nUserId"]);
                            bShowHiddenForUser = myWeb.moDbHelper.checkUserRole(myWeb.moConfig["UserRoleAllowedHiddenProductSearch"], "Role", nUserId);
                        }
                        // Generate the live page filter
                        var livePages = LivePageLuceneFilter(ref myAPI);

                        // Generate the query criteria
                        var argXmlDoc = resultsXML.OwnerDocument;
                        var searchFilters = BuildFiltersFromRequest(ref argXmlDoc, ref myAPI.moRequest);
                        if (searchFilters != null)
                            resultsXML.AppendChild(searchFilters);

                        // Generate the search query
                        var searchQuery = BuildLuceneQuery(cQuery, searchFilters, bShowHiddenForUser);

                        // Add a sort from the request
                        var queryOrder = SetSortFieldFromRequest(myAPI.moRequest);

                        // Perform the search
                        TopDocs results;

                        if ((myAPI.moConfig["SiteSearchDebug"]).ToLower() == "on")
                        {
                            var argoParent = resultsXML;
                            addElement(ref argoParent, "LuceneQuery", searchQuery.ToString());
                            if (livePages != null)
                            {
                                var argoParent1 = resultsXML;
                                addElement(ref argoParent1, "LuceneLivePageFilter", livePages.ToString());
                            }
                        }

                        // Dim HitsLimit As Integer = 300

                        if (livePages is null)
                        {
                            results = searcher.Search(searchQuery, HitsLimit);
                        }
                        else
                        {
                            results = searcher.Search(searchQuery, livePages, HitsLimit, queryOrder);
                        }

                        // Log the search
                        if (_logSearches)
                        {
                            myAPI.moDbHelper.logActivity((_includeFuzzySearch)? dbHelper.ActivityType.FuzzySearch: dbHelper.ActivityType.Search, myAPI.mnUserId, 0, 0, results.TotalHits, cQuery);
                        }

                        // Optional fuzzysearch for figures
                        // This is simply designed to get a count that can be fed back to users
                        // This could be a performance no-no, although most searches are quick
                        if (_runPreFuzzySearch & !_includeFuzzySearch)
                        {
                            _includeFuzzySearch = true;
                            searchQuery = BuildLuceneQuery(cQuery, searchFilters);
                            resultsXML.SetAttribute("fuzzyCount", searcher.Search(searchQuery, livePages, HitsLimit, queryOrder).TotalHits.ToString());
                        }

                        Document resultDoc = null;
                        XmlElement result = null;
                        Field pageIdField = null;
                        long pageId = 0L;
                        string url = "";
                        XmlElement menuItem = null;
                        string[] reservedFieldNames = new string[] { "type", "text", "abstract" };
                        int pageStart;
                        int pageCount;
                        int pageEnd;
                        // Paging settings
                        // Hits is so lightweight that we don't have to filter it beforehand
                        // See: http://wiki.apache.org/lucene-java/LuceneFAQ#How_do_I_implement_paging.2C_i.e._showing_result_from_1-10.2C_11-20_etc.3F
                        long totalResults = results.ScoreDocs.Length; // TotalHits

                        pageCount = 0; // myAPI.GetRequestItemAsInteger("pageCount", _pagingDefaultSize)
                        if (pageCount <= 0)
                        {
                            pageCount = results.ScoreDocs.Length; // TotalHits
                        }

                        if (totalResults == 0L)
                        {
                            pageStart = (int)totalResults;
                            pageEnd = (int)totalResults;
                        }
                        else
                        {
                            // pageStart = Math.Max(myAPI.GetRequestItemAsInteger("pageStart", 1), 1)

                            pageStart = 1;
                            pageEnd = (int)Math.Min(totalResults, pageStart + pageCount - 1);
                        }

                        if (pageEnd < pageStart)
                            pageEnd = pageStart;

                        if (totalResults > 0L & pageCount > 0)
                        {
                            int pageNumber = (int)(totalResults % pageCount);
                        }


                        resultsXML.SetAttribute("pageStart", pageStart.ToString());
                        resultsXML.SetAttribute("pageCount", pageCount.ToString());
                        resultsXML.SetAttribute("pageEnd", pageEnd.ToString());

                        resultsXML.SetAttribute("sortCol", myAPI.moRequest["sortCol"]);
                        resultsXML.SetAttribute("sortColType", myAPI.moRequest["sortColType"]);
                        resultsXML.SetAttribute("sortDir", myAPI.moRequest["sortDir"]);

                        var artIdResults = new List<long>();
                        string thisArtIdList = "";
                        // Process the results
                        if (totalResults > 0L)
                        {
                            ScoreDoc sDoc;
                            foreach (var currentSDoc in results.ScoreDocs)
                            {
                                sDoc = currentSDoc;

                                resultDoc = searcher.Doc(sDoc.Doc);

                                pageIdField = resultDoc.GetField("pgid");
                                if (pageIdField != null && IsStringNumeric(pageIdField.StringValue))
                                {
                                    pageId = Convert.ToInt32(pageIdField.StringValue);
                                }
                                else
                                {
                                    pageId = 0L;
                                }

                                url = ""; // this is the link for the page

                                // don't add artId more than twice to results.
                                long thisArtId;
                                if (resultDoc.GetField("artid") != null)
                                {
                                    thisArtId = Conversions.ToInteger(resultDoc.GetField("artid").StringValue);
                                    if (string.IsNullOrEmpty(thisArtIdList))
                                    {
                                        thisArtIdList = thisArtId.ToString();
                                    }
                                    else
                                    {
                                        thisArtIdList = thisArtIdList + "," + thisArtId;
                                    }
                                }

                            }
                            // 'check whether logged in user is csuser and skip checking status
                            // Dim bShowHiddenForUser As Boolean = False 'set for normal user default value
                            // If myWeb.moConfig("UserRoleAllowedHiddenProductSearch") IsNot Nothing Then
                            // Dim nUserId As Integer = myWeb.moSession("nUserId")
                            // bShowHiddenForUser = myWeb.moDbHelper.checkUserRole(myWeb.moConfig("UserRoleAllowedHiddenProductSearch"), "Role", nUserId)
                            // End If
                            // check artid/product is active
                            if (!bShowHiddenForUser & !string.IsNullOrEmpty(thisArtIdList))
                            {
                                thisArtIdList = myWeb.CheckProductStatus(thisArtIdList);
                            }

                            foreach (var currentSDoc1 in results.ScoreDocs)
                            {
                                sDoc = currentSDoc1;

                                resultDoc = searcher.Doc(sDoc.Doc);

                                pageIdField = resultDoc.GetField("pgid");
                                if (pageIdField != null && IsStringNumeric(pageIdField.StringValue))
                                {
                                    pageId = Convert.ToInt32(pageIdField.StringValue);
                                }
                                else
                                {
                                    pageId = 0L;
                                }

                                url = ""; // this is the link for the page
                                long thisArtId = 0L;
                                if (resultDoc.GetField("artid") != null)
                                {
                                    thisArtId = Conversions.ToInteger(resultDoc.GetField("artid").StringValue);
                                }
                                if (thisArtId == 0L | thisArtIdList.Contains(thisArtId.ToString()))
                                {
                                    if (thisArtId == default | !artIdResults.Exists(x => x == thisArtId))
                                    {
                                        if (thisArtId != default)
                                            artIdResults.Add(thisArtId);

                                        url = resultDoc.GetField("url").StringValue + "";

                                        // Build the URL
                                        if (string.IsNullOrEmpty(url))
                                        {
                                            url = menuItem.GetAttribute("url");
                                            // Add the artId, if exists
                                            if (resultDoc.GetField("artid") != null)
                                            {


                                                if (resultDoc.GetField("contenttype") != null && resultDoc.GetField("contenttype").StringValue == "Download")
                                                {
                                                    url = resultDoc.GetField("url").StringValue;
                                                }
                                                else if (moConfig["LegacyRedirect"] == "on")
                                                {
                                                    url = Conversions.ToString(url + Operators.ConcatenateObject(Operators.ConcatenateObject(Interaction.IIf(url == "/", "", "/"), resultDoc.GetField("artid").StringValue), "-/"));

                                                    string artName = "";
                                                    if (resultDoc.GetField("name") != null)
                                                    {
                                                        artName = resultDoc.GetField("name").StringValue;
                                                        var oRe = new Regex("[^A-Z0-9]", RegexOptions.IgnoreCase);
                                                        artName = oRe.Replace(artName, "-").Trim('-');
                                                        url += artName;
                                                    }
                                                }
                                                else
                                                {
                                                    url = Conversions.ToString(url + Operators.ConcatenateObject(Operators.ConcatenateObject(Interaction.IIf(url == "/", "", "/"), "item"), resultDoc.GetField("artid").StringValue));
                                                }
                                            }
                                        }

                                        result = moContextNode.OwnerDocument.CreateElement("Content");
                                        result.SetAttribute("type", "SearchResult");
                                        // result.SetAttribute("indexId", )
                                        result.SetAttribute("indexRank", sDoc.Score.ToString());

                                        foreach (Field docField in resultDoc.GetFields())
                                        {

                                            // Don't add info to certain fields
                                            // If Array.IndexOf(reservedFieldNames, docField.Name) = -1 Then
                                            // result.SetAttribute(docField.Name, docField.StringValue)
                                            // End If

                                            if (Array.IndexOf(reservedFieldNames, docField.Name) == -1)
                                            {

                                                // check whether logged in user is csuser and skip checking status
                                                if (myWeb.moDbHelper is null)
                                                {
                                                    myWeb.moDbHelper = myWeb.GetDbHelper();
                                                }
                                                result.SetAttribute(docField.Name, docField.StringValue); // search all the products

                                            }

                                            if (docField.Name == "abstract")
                                            {

                                                // Try to output this as Xml
                                                string innerString = docField.StringValue + "";
                                                processInfo = innerString;
                                                try
                                                {
                                                    result.InnerXml = innerString.Trim();
                                                }
                                                catch (Exception ex)
                                                {
                                                    innerString = innerString.Replace("&", "&amp;").Replace("&amp;amp;", "&amp;").Trim();
                                                    processInfo = innerString;
                                                    result.InnerText = innerString;
                                                }

                                            }
                                        }
                                        result.SetAttribute("url", url);

                                        moContextNode.AppendChild(result);
                                        resultsCount = resultsCount + 1;
                                    }
                                }
                            }
                        }

                        dateFinish = DateTime.Now;
                        resultsXML.SetAttribute("Time", dateFinish.Subtract(dateStart).TotalMilliseconds.ToString());
                    }
                    // resultsCount = results.TotalHits()
                    else
                    {
                        resultsXML.SetAttribute("Time", "0");
                    }

                    // Dim oResXML As XmlElement = moPageXml.CreateElement("Content")

                    resultsXML.SetAttribute("SearchString", cQuery);
                    resultsXML.SetAttribute("searchType", "INDEX");
                    resultsXML.SetAttribute("type", "SearchHeader");
                    resultsXML.SetAttribute("Hits", resultsCount.ToString());

                    moContextNode.AppendChild(resultsXML);
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Search", ex, "", processInfo, gbDebug);
                }
            }


            public void XPathQuery(string sSearch, string cContentType)
            {
                myWeb.PerfMon.Log("Search", "XPathQuery");
                string sXpath = "";
                string sXpathRoot = "";
                string[] aSearchWords;
                string[] aNewSearchWords = null;
                int i;
                string sError = "";
                string cSearchTerm = "";
                bool bFirst = true;
                bool bSearchSubmitted = false;
                var dtStart = DateTime.Now;
                try
                {

                    // going to see if there is a a form
                    XmlElement ofrmElmt = (XmlElement)moContextNode.SelectSingleNode("Content[@name='search' and @type='xform']");
                    if (ofrmElmt != null)
                    {
                        var ofrm = new Cms.xForm(ref myWeb);
                        ofrm.load(ref ofrmElmt);
                        ofrm.updateInstanceFromRequest();
                        if (ofrm.Instance is null)
                            ofrmElmt = ofrm.Instance;
                        sXpath = Protean.xmlTools.getXpathFromQueryXml(ofrm.Instance);
                        ofrm.addValues();
                    }

                    // XPATH Search
                    bSearchSubmitted = true;

                    if (string.IsNullOrEmpty(moConfig["ewSearchXpath"]))
                    {
                        sXpathRoot = "//*";
                    }
                    // sXpathRoot = "//"
                    else
                    {
                        sXpathRoot = moConfig["ewSearchXpath"];
                    }


                    // Clean the search term and put it into an array of words
                    aSearchWords = Strings.Split(CleanSearchString(sSearch), " ");

                    // Construct the Xpath
                    // Note that the logic for searching is (Word 1 OR a variant of it) AND (Word 2 OR a variant of it)
                    var loopTo = Information.UBound(aSearchWords);
                    for (i = 0; i <= loopTo; i++)
                    {
                        cSearchTerm = aSearchWords[i];
                        if (!string.IsNullOrEmpty(Strings.Trim(cSearchTerm)))
                        {

                            if (bFirst)
                            {
                                bFirst = !bFirst;
                            }
                            else
                            {
                                sXpath = sXpath + " and ";
                            }

                            // Add the word
                            sXpath = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(sXpath + "[contains(upper-case(.),''", SqlFmt(Strings.UCase(cSearchTerm))), "'')"));

                            // Get the variants of the word
                            if (cSearchTerm.EndsWith("s"))
                            {
                                sXpath = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(sXpath + " or contains(upper-case(.),''", SqlFmt(Strings.UCase(Strings.Left(cSearchTerm, Strings.Len(cSearchTerm) - 1)))), "'')"));
                            }
                            if (aSearchWords[i].EndsWith("ies"))
                            {
                                sXpath = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(sXpath + " or contains(upper-case(.),''", SqlFmt(Strings.UCase(Strings.Left(cSearchTerm, Strings.Len(cSearchTerm) - 3) + "y"))), "'')"));
                            }
                            if (aSearchWords[i].EndsWith("y"))
                            {
                                sXpath = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(sXpath + " or contains(upper-case(.),''", SqlFmt(Strings.UCase(Strings.Left(cSearchTerm, Strings.Len(cSearchTerm) - 1) + "ies"))), "'')"));
                            }

                            sXpath = sXpath + "]";
                        }
                    }

                    if (!string.IsNullOrEmpty(sXpath))
                    {
                        // sXpath = sXpathRoot & "[" & sXpath & "]"
                        sXpath = sXpathRoot + sXpath;
                        if (myWeb.moRequest["contentType"] == "")
                        {
                        }
                        // GetPageContentFromSelect(" where dbo.fxn_SearchXML(cContentXML,'" & sXpath & "') = 1")
                        else
                        {
                            // GetPageContentFromSelect(" where typ.cContentTypeName='" & myWeb.moRequest("contentType") & "' and dbo.fxn_SearchXML(cContentXML,'" & sXpath & "') = 1")
                        }
                        string cSQL = Conversions.ToString(Operators.ConcatenateObject("SET ARITHABORT ON SELECT nContentKey, cContentXmlBrief,  cContentXmlDetail, nContentPrimaryId, cContentName, cContentSchemaName " + " FROM tblContent WHERE" + "  (CAST(cContentXmlBrief as xml).exist('" + sXpath + "') = 1 or CAST(cContentXmlDetail as xml).exist('" + sXpath + "') = 1)", Interaction.IIf(string.IsNullOrEmpty(cContentType), "", Operators.ConcatenateObject(Operators.ConcatenateObject(" AND (cContentSchemaName = '", SqlFmt(cContentType)), "')"))));
                        // Dim oDr As SqlDataReader
                        using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                        {
                            int nResultCount;
                            string cResultIDsCSV = "";
                            while (oDr.Read())
                                // If checkPagePermission(oDr("nContentPrimaryId")) = oDr("nContentPrimaryId") And ContentPagesLive(oDr("nContentKey")) Then




                                cResultIDsCSV = Conversions.ToString(cResultIDsCSV + Operators.ConcatenateObject(oDr["nContentKey"], ","));
                            oDr.Close();

                            if (string.IsNullOrEmpty(cResultIDsCSV))
                                cResultIDsCSV = "0";
                            nResultCount = GetContentXml(ref moContextNode, cResultIDsCSV);

                            // Log the search
                            if (_logSearches)
                            {
                                myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Search, myWeb.mnUserId, 0, 0, nResultCount, sSearch);
                            }

                            var oResXML = moPageXml.CreateElement("Content");

                            oResXML.SetAttribute("SearchString", sSearch);
                            oResXML.SetAttribute("type", "SearchHeader");
                            oResXML.SetAttribute("searchType", "XPATH");
                            oResXML.SetAttribute("Hits", nResultCount.ToString());
                            oResXML.SetAttribute("Time", DateTime.Now.Subtract(dtStart).TotalMilliseconds.ToString());
                            oResXML.SetAttribute("DebugSQL", cSQL);

                            moContextNode.AppendChild(oResXML);

                        }

                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Search", ex, "", bDebug: gbDebug);
                }
            }

            public void RegExpQuery(string sSearch, string cContentType, bool bUserQuery = false)
            {
                myWeb.PerfMon.Log("Search", "RegExpQuery");
                string sXpath = "";
                string[] aSearchWords;
                string[] aNewSearchWords = null;
                int i;
                string sError = "";
                string cSearchTerm = "";
                bool bFirst = true;
                bool bSearchSubmitted = false;
                Regex oRegEx;
                string sProcessInfo = "Search Routing";
                int nResultCount = 0;
                var dtStart = DateTime.Now;
                // REGEXP Search

                // Optional parameters (from the Web.config):
                // ewSearchMatchWholeWords  - when set to "on", this will only match entered words, as opposed to substring-match (e.g. matching web in eonicweb)
                // ewSearchRegExIncludeTags - experimental.  This tries to replicate simple Xpath by only looking at what's in certain tags.
                // This should be a pipe separated list (i.e. h1|h2|h3)

                bSearchSubmitted = true;

                string cIDs = "";
                string cSearchWhereCONTENT = "";
                string cSearchWhereUSER = "";
                string cRegEx = "";
                string cRegExPattern = "";
                Regex reMasterCheck;
                string cSql = "";
                var aSearchTerms = new Collection();
                bool bFullMatch = true;
                string cSearchVariant;


                string cRegEx_TagStartPattern = "";
                string cRegEx_TagEndPattern = "";
                try
                {
                    // If ewSearchRegExIncludeTags is initiated then, we need to create a regex to match 
                    // the opening tag and closing tag (e.g. Match <a> and </a>).  These will be added to the 
                    // each of the search terms, making a very large regex.

                    if (!string.IsNullOrEmpty(moConfig["ewSearchRegExIncludeTags"]))
                    {
                        cRegEx_TagStartPattern = "(<(" + moConfig["ewSearchRegExIncludeTags"] + ")[^>]*>).*?";
                        cRegEx_TagEndPattern = @".*?(<\/(" + moConfig["ewSearchRegExIncludeTags"] + ")>)";
                    }

                    // remove any single quotes to prevent injection attacks
                    cContentType = Strings.Replace(cContentType, "'", "");
                    // Change contentType from CSV to CSV with quotes!
                    cContentType = Strings.Replace(cContentType, ",", "','");
                    cContentType = "'" + cContentType + "'";


                    // Note that the logic for searching is (Word 1 OR a variant of it) AND (Word 2 OR a variant of it)

                    // Two things are being constructed here:
                    // An array of a word and its variants, which is then added to aSearchTerms
                    // A SQL statement, which will allow us to get anything that is likely any of the search words or their variants.
                    aSearchWords = Strings.Split(CleanSearchString(sSearch), " ");
                    var loopTo = Information.UBound(aSearchWords);
                    for (i = 0; i <= loopTo; i++)
                    {
                        cSearchTerm = aSearchWords[i];
                        if (!string.IsNullOrEmpty(Strings.Trim(cSearchTerm)))
                        {

                            if (bFirst)
                            {
                                bFirst = !bFirst;
                            }
                            else
                            {
                                cSearchWhereCONTENT = cSearchWhereCONTENT + " OR ";
                                cSearchWhereUSER = cSearchWhereUSER + " OR ";
                            }



                            cRegEx = cSearchTerm;
                            // Note :: in the SQL statement below, the inclusion of [^<] is a token gesture to make sure we don't match to tags that begin with a search term, e.g. <Content and </Content etc...
                            cSearchWhereCONTENT = Conversions.ToString(cSearchWhereCONTENT + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" (cContentXMLBrief LIKE '%[^<]", SqlFmt(cSearchTerm)), "%' AND cContentXMLBrief LIKE '%[^<][^/]"), SqlFmt(cSearchTerm)), "%') "));
                            cSearchWhereCONTENT += " or ";
                            cSearchWhereCONTENT = Conversions.ToString(cSearchWhereCONTENT + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" (cContentXMLDetail LIKE '%[^<]", SqlFmt(cSearchTerm)), "%' AND cContentXMLDetail LIKE '%[^<][^/]"), SqlFmt(cSearchTerm)), "%') "));

                            cSearchWhereUSER = Conversions.ToString(cSearchWhereUSER + Operators.ConcatenateObject(Operators.ConcatenateObject(" (cDirXml LIKE '%[^<]", SqlFmt(cSearchTerm)), "%'  )"));

                            if (cSearchTerm.ToLower().EndsWith("s"))
                            {
                                cSearchVariant = Strings.Left(cSearchTerm, Strings.Len(cSearchTerm) - 1);
                                cRegEx += "|" + cSearchVariant;
                                cSearchWhereCONTENT = Conversions.ToString(cSearchWhereCONTENT + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" OR (cContentXMLBrief LIKE '%[^<]", SqlFmt(cSearchVariant)), "%' AND cContentXMLBrief LIKE '%[^<][^/]"), SqlFmt(cSearchVariant)), "%') "));
                                cSearchWhereCONTENT = Conversions.ToString(cSearchWhereCONTENT + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" OR (cContentXMLDetail LIKE '%[^<]", SqlFmt(cSearchVariant)), "%' AND cContentXMLDetail LIKE '%[^<][^/]"), SqlFmt(cSearchVariant)), "%') "));

                                cSearchWhereUSER = Conversions.ToString(cSearchWhereUSER + Operators.ConcatenateObject(Operators.ConcatenateObject(" OR  (cDirXml LIKE '%[^<]", SqlFmt(cSearchVariant)), "%'  )"));
                            }
                            if (aSearchWords[i].ToLower().EndsWith("ies"))
                            {
                                cSearchVariant = Strings.Left(cSearchTerm, Strings.Len(cSearchTerm) - 3) + "y";
                                cRegEx += "|" + cSearchVariant;
                                cSearchWhereCONTENT = Conversions.ToString(cSearchWhereCONTENT + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" OR (cContentXMLBrief LIKE '%[^<]", SqlFmt(cSearchVariant)), "%' AND cContentXMLBrief LIKE '%[^<][^/]"), SqlFmt(cSearchVariant)), "%') "));
                                cSearchWhereCONTENT = Conversions.ToString(cSearchWhereCONTENT + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" OR (cContentXMLDetail LIKE '%[^<]", SqlFmt(cSearchVariant)), "%' AND cContentXMLDetail LIKE '%[^<][^/]"), SqlFmt(cSearchVariant)), "%') "));

                                cSearchWhereUSER = Conversions.ToString(cSearchWhereUSER + Operators.ConcatenateObject(Operators.ConcatenateObject(" OR  (cDirXml LIKE '%[^<]", SqlFmt(cSearchVariant)), "%'  )"));
                            }
                            if (aSearchWords[i].ToLower().EndsWith("y"))
                            {
                                cSearchVariant = Strings.Left(cSearchTerm, Strings.Len(cSearchTerm) - 1) + "ies";
                                cRegEx += "|" + cSearchVariant;
                                cSearchWhereCONTENT = Conversions.ToString(cSearchWhereCONTENT + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" OR (cContentXMLBrief LIKE '%[^<]", SqlFmt(cSearchVariant)), "%' AND cContentXMLBrief LIKE '%[^<][^/]"), SqlFmt(cSearchVariant)), "%') "));
                                cSearchWhereCONTENT = Conversions.ToString(cSearchWhereCONTENT + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" OR (cContentXMLDetail LIKE '%[^<]", SqlFmt(cSearchVariant)), "%' AND cContentXMLDetail LIKE '%[^<][^/]"), SqlFmt(cSearchVariant)), "%') "));


                                cSearchWhereUSER = Conversions.ToString(cSearchWhereUSER + Operators.ConcatenateObject(Operators.ConcatenateObject(" OR (cDirXml LIKE '%[^<]", SqlFmt(cSearchVariant)), "%'  )"));
                            }

                            // Each word and its variants are put into a distinct array which is then added to a collection of search terms  
                            // aSearchTerms.Add(LCase(cRegEx).Split("|"))
                            if (moConfig["ewSearchMatchWholeWords"] == "on")
                            {
                                cRegEx = @"\b(" + cRegEx + @")\b";
                            }
                            else
                            {
                                cRegEx = "(" + cRegEx + ")";
                                // cRegEx = "(?>" & cRegEx & ")"
                            }

                            cRegExPattern += "(?=.*?" + cRegEx_TagStartPattern + cRegEx + cRegEx_TagEndPattern + ")";
                        }
                    }


                    if (!string.IsNullOrEmpty(cRegExPattern) | bUserQuery)
                    {
                        // Dim oDr As SqlClient.SqlDataReader

                        // Create a Reg Exp to strip out any tags
                        // Pattern: "<", optionally followed by "/", followed by one or more occurences of any character that isn't ">", followed by ">"
                        oRegEx = new Regex(@"<\/?[^>]+>");

                        // Critical for performance: this search matches the beginning (^) and end ($) of the string,
                        // meaning that this phrase will only attempt the match once, rather than on every character
                        // of the search string
                        cRegExPattern = "^" + cRegExPattern + ".*?$";
                        try
                        {

                            reMasterCheck = new Regex(cRegExPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled); // ex As Exception
                        }
                        catch
                        {

                            // return invalid search
                            var oResXMLErr = moPageXml.CreateElement("Content");
                            oResXMLErr.SetAttribute("searchType", Conversions.ToString(Interaction.IIf(bUserQuery, "USER", "REGEX")));
                            oResXMLErr.SetAttribute("SearchString", "Invalid Query");
                            oResXMLErr.SetAttribute("type", "SearchHeader");
                            oResXMLErr.SetAttribute("Hits", "0");
                            oResXMLErr.SetAttribute("Time", DateTime.Now.Subtract(dtStart).TotalMilliseconds.ToString());
                            oResXMLErr.SetAttribute("resultIds", "");
                            moContextNode.AppendChild(oResXMLErr);

                            return;
                        }

                        // Get the SQL that will look for any of the search words or their variants

                        cSql = Conversions.ToString(Operators.ConcatenateObject("SELECT nContentKey, cContentXmlBrief,  cContentXmlDetail, nContentPrimaryId, cContentName, cContentSchemaName " + " FROM tblContent " + " WHERE (" + cSearchWhereCONTENT + ")", Interaction.IIf(string.IsNullOrEmpty(cContentType), "", " AND (cContentSchemaName IN (" + cContentType + "))")));


                        string cResultIDsCSV = "";
                        if (!bUserQuery)
                        {
                            using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(cSql))  // Done by nita on 6/7/22
                            {

                                int ppppp = 0;
                                while (oDr.Read())
                                {
                                    Debug.WriteLine("Search loop " + ppppp);
                                    ppppp += 1;
                                    // If moDbHelper.checkPagePermission(oDr("nContentPrimaryId")) = oDr("nContentPrimaryId") And ContentPagesLive(oDr("nContentKey")) Then

                                    string cNewLineLessBrief = Conversions.ToString(oDr["cContentXmlBrief"]);
                                    string cNewLineLessDetail = Conversions.ToString(oDr["cContentXmlDetail"]);
                                    if (!string.IsNullOrEmpty(cNewLineLessBrief))
                                        cNewLineLessBrief = Strings.Replace(cNewLineLessBrief, Conversions.ToString('\n'), "");
                                    if (!string.IsNullOrEmpty(cNewLineLessBrief))
                                        cNewLineLessBrief = Strings.Replace(cNewLineLessBrief, Conversions.ToString('\r'), "");
                                    if (!string.IsNullOrEmpty(cNewLineLessDetail))
                                        cNewLineLessDetail = Strings.Replace(cNewLineLessDetail, Conversions.ToString('\n'), "");
                                    if (!string.IsNullOrEmpty(cNewLineLessDetail))
                                        cNewLineLessDetail = Strings.Replace(cNewLineLessDetail, Conversions.ToString('\r'), "");

                                    if (reMasterCheck.IsMatch(cNewLineLessBrief) | reMasterCheck.IsMatch(cNewLineLessDetail))
                                    {
                                        // If (reMasterCheck.IsMatch(oDr("cContentXmlBrief")) Or reMasterCheck.IsMatch(oDr("cContentXmlDetail"))) Then
                                        // If reMasterCheck.IsMatch(oDr("cContentXmlDetail")) Then
                                        cResultIDsCSV = Conversions.ToString(cResultIDsCSV + Operators.ConcatenateObject(oDr["nContentKey"], ","));
                                    }

                                }
                                oDr.Close();
                                if (string.IsNullOrEmpty(cResultIDsCSV))
                                    cResultIDsCSV = "0";
                                nResultCount = GetContentXml(ref moContextNode, cResultIDsCSV);
                            }
                        }
                        else
                        {
                            if (myWeb.mnUserId == 0)
                                return;

                            // users in all groups
                            cSql = "SELECT nDirKey, cDirSchema, cDirForiegnRef, cDirName, cDirXml" + " FROM tblDirectory Users";

                            string cOverrideUserGroups;
                            cOverrideUserGroups = moConfig["SearchAllUserGroups"];
                            if (cOverrideUserGroups != null & !string.IsNullOrEmpty(cOverrideUserGroups))
                            {
                                if (!(cOverrideUserGroups == "on"))
                                {
                                    if (string.IsNullOrEmpty(cContentType))
                                    {
                                        cSql += "WHERE (cDirSchema = N'User') AND" + " (((SELECT TOP 1 DirRel1.nDirChildId AS OtherUser" + " FROM tblDirectory Dir INNER JOIN" + " tblDirectoryRelation DirRel ON Dir.nDirKey = DirRel.nDirParentId INNER JOIN" + " tblDirectoryRelation DirRel1 ON Dir.nDirKey = DirRel1.nDirParentId" + " WHERE (DirRel.nDirChildId = " + myWeb.mnUserId + ") AND (DirRel1.nDirChildId = Users.nDirKey))) IS NOT NULL)";
                                        cSql += " And (cDirSchema = 'User')";
                                    }
                                    else
                                    {
                                        cSql += " WHERE (cDirSchema = N'User') AND" + " (((SELECT TOP 1 DirRel1.nDirChildId AS OtherUser" + " FROM tblDirectory Dir INNER JOIN" + " tblDirectoryRelation DirRel ON Dir.nDirKey = DirRel.nDirParentId INNER JOIN" + " tblDirectoryRelation DirRel1 ON Dir.nDirKey = DirRel1.nDirParentId" + " WHERE (DirRel.nDirChildId = " + myWeb.mnUserId + ") AND (DirRel1.nDirChildId = Users.nDirKey) AND (Dir.cDirName = '" + cContentType + "'))) IS NOT NULL)";
                                        cSql += " And (cDirSchema = 'User')";
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cContentType) & !(cContentType == "''"))
                                {
                                    cSql += " WHERE (cDirSchema = N'User') AND" + " (((SELECT nDirParentId" + " FROM tblDirectoryRelation " + " WHERE (nDirParentId = " + cContentType + ") AND (nDirChildId = Users.nDirKey))) IS NOT NULL)";
                                    cSql += " And (cDirSchema = 'User')";
                                }
                                else
                                {
                                    cSql += " WHERE (cDirSchema = 'User')";
                                }
                            }

                            if (!string.IsNullOrEmpty(sSearch))
                            {
                                if (!cSql.Contains(" WHERE "))
                                    cSql += " WHERE ";
                                else
                                    cSql += " AND ";
                                cSql += "(" + cSearchWhereUSER + " )";
                            }


                            cSql += " ORDER BY cDirName";
                            using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(cSql))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                {

                                    var oContent = moContextNode.OwnerDocument.CreateElement("Content");
                                    oContent.SetAttribute("id", Conversions.ToString(oDr["nDirKey"]));
                                    oContent.SetAttribute("type", "User");
                                    oContent.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                    oContent.InnerXml = Conversions.ToString(oDr["cDirXml"]);
                                    if (reMasterCheck.IsMatch(oContent.OuterXml))
                                    {
                                        nResultCount += 1;
                                        oContent.FirstChild.AppendChild(oContent.OwnerDocument.ImportNode(myWeb.moDbHelper.GetUserContactsXml(Convert.ToInt32(oDr["nDirKey"])).CloneNode(true), true));
                                        moContextNode.AppendChild(oContent);
                                    }
                                }
                                oDr.Close();
                            }
                        }

                        // Log the search
                        if (_logSearches)
                        {
                            myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Search, myWeb.mnUserId, 0, 0, nResultCount, sSearch);
                        }

                        var oResXML = moPageXml.CreateElement("Content");
                        oResXML.SetAttribute("searchType", Conversions.ToString(Interaction.IIf(bUserQuery, "USER", "REGEX")));
                        oResXML.SetAttribute("SearchString", sSearch);
                        oResXML.SetAttribute("type", "SearchHeader");
                        oResXML.SetAttribute("Hits", nResultCount.ToString());
                        oResXML.SetAttribute("Time", DateTime.Now.Subtract(dtStart).TotalMilliseconds.ToString());
                        oResXML.SetAttribute("resultIds", cResultIDsCSV);
                        moContextNode.AppendChild(oResXML);



                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "RegExpQuery", ex, "", sProcessInfo, gbDebug);
                }
            }

            public void FullTextQuery(string sSearch, string cContentType, bool bUserQuery = false)
            {
                myWeb.PerfMon.Log("Search", "FullTextQuery");
                string sXpath = "";
                string[] aSearchWords;
                string[] aNewSearchWords = null;
                int i;
                string sError = "";
                string cSearchTerm = "";
                bool bFirst = true;
                bool bSearchSubmitted = false;
                string sProcessInfo = "Search Routing";
                int nResultCount = 0;
                var dtStart = DateTime.Now;

                bSearchSubmitted = true;

                string cIDs = "";
                string cSearchWhereCONTENT = "";
                string cSearchWhereCONTENTforCode = "";
                string cSearchWhereUSER = "";
                string cRegEx = "";
                string cRegExPattern = "";
                Regex reMasterCheck = null;
                string cSql = "";
                var aSearchTerms = new Collection();
                bool bFullMatch = true;

                try
                {

                    // remove any single quotes to prevent injection attacks
                    cContentType = Strings.Replace(cContentType, "'", "");
                    // Change contentType from CSV to CSV with quotes!
                    cContentType = Strings.Replace(cContentType, ",", "','");
                    cContentType = "'" + cContentType + "'";


                    aSearchWords = Strings.Split(CleanSearchString(sSearch), " ");
                    var loopTo = Information.UBound(aSearchWords);
                    for (i = 0; i <= loopTo; i++)
                    {
                        cSearchTerm = aSearchWords[i];
                        if (!string.IsNullOrEmpty(Strings.Trim(cSearchTerm)))
                        {

                            if (bFirst)
                            {
                                bFirst = !bFirst;
                            }
                            else
                            {
                                cSearchWhereCONTENT = cSearchWhereCONTENT + " AND ";
                                cSearchWhereUSER = cSearchWhereUSER + " AND ";
                            }
                            cRegEx = cSearchTerm;
                            cSearchWhereCONTENT = cSearchWhereCONTENT + " (CONTAINS(parentContent.cContentXmlDetail, '" + cSearchTerm + "') OR CONTAINS(parentContent.cContentXmlBrief, '" + cSearchTerm + "'))";
                        }
                    }

                    if (!string.IsNullOrEmpty(cSearchWhereCONTENT))
                    {
                        cSearchWhereCONTENT = " ( " + cSearchWhereCONTENT + " ) ";
                        bFirst = true;
                    }

                    var loopTo1 = Information.UBound(aSearchWords);
                    for (i = 0; i <= loopTo1; i++)
                    {
                        cSearchTerm = aSearchWords[i];
                        if (!string.IsNullOrEmpty(Strings.Trim(cSearchTerm)))
                        {

                            if (bFirst)
                            {
                                bFirst = !bFirst;
                            }
                            else
                            {
                                cSearchWhereCONTENTforCode = cSearchWhereCONTENTforCode + " AND ";
                                cSearchWhereUSER = cSearchWhereUSER + " AND ";
                            }
                            cRegEx = cSearchTerm;
                            cSearchWhereCONTENTforCode = cSearchWhereCONTENTforCode + " (CONTAINS(childContent.cContentXmlDetail, '" + cSearchTerm + "') OR CONTAINS(childContent.cContentXmlBrief, '" + cSearchTerm + "'))";
                        }
                    }

                    if (!string.IsNullOrEmpty(cSearchWhereCONTENTforCode))
                    {
                        cSearchWhereCONTENT = cSearchWhereCONTENT + " OR ( " + cSearchWhereCONTENTforCode + " ) ";
                    }

                    if (!string.IsNullOrEmpty(cSearchWhereCONTENT) | bUserQuery)
                    {
                        // Dim oDr As SqlClient.SqlDataReader

                        cSql = Conversions.ToString(Operators.ConcatenateObject("SELECT distinct  parentContent.nContentKey, Cast(parentContent.cContentXmlBrief as NVarchar(Max)) as cContentXmlBrief,  Cast(parentContent.cContentXmlDetail as NVarchar(Max)) as cContentXmlDetail, parentContent.nContentPrimaryId, parentContent.cContentName, parentContent.cContentSchemaName " + @" FROM tblContentRelation r  
inner join tblContent parentContent on (r.nContentParentId = parentContent.nContentKey) "
                              + "inner join tblContent childContent on (r.nContentChildId = childContent.nContentKey) " + " WHERE (" + cSearchWhereCONTENT + ")", Interaction.IIf(string.IsNullOrEmpty(cContentType), "", " AND (parentContent.cContentSchemaName IN (" + cContentType + "))")));


                        string cResultIDsCSV = "";
                        if (!bUserQuery)
                        {
                            using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(cSql))  // Done by nita on 6/7/22
                            {

                                int ppppp = 0;
                                while (oDr.Read())
                                {
                                    // Debug.WriteLine("Search loop " & ppppp)
                                    // ppppp += 1

                                    string cNewLineLessBrief = Conversions.ToString(oDr["cContentXmlBrief"]);
                                    string cNewLineLessDetail = Conversions.ToString(oDr["cContentXmlDetail"]);

                                    cResultIDsCSV = Conversions.ToString(cResultIDsCSV + Operators.ConcatenateObject(oDr["nContentKey"], ","));

                                }
                                oDr.Close();
                                if (string.IsNullOrEmpty(cResultIDsCSV))
                                    cResultIDsCSV = "0";
                                nResultCount = GetContentXml(ref moContextNode, cResultIDsCSV);
                            }
                        }
                        else
                        {
                            if (myWeb.mnUserId == 0)
                                return;

                            // users in all groups
                            cSql = "SELECT nDirKey, cDirSchema, cDirForiegnRef, cDirName, cDirXml" + " FROM tblDirectory Users";

                            string cOverrideUserGroups;
                            cOverrideUserGroups = moConfig["SearchAllUserGroups"];
                            if (cOverrideUserGroups != null & !string.IsNullOrEmpty(cOverrideUserGroups))
                            {
                                if (!(cOverrideUserGroups == "on"))
                                {
                                    if (string.IsNullOrEmpty(cContentType))
                                    {
                                        cSql += "WHERE (cDirSchema = N'User') AND" + " (((SELECT TOP 1 DirRel1.nDirChildId AS OtherUser" + " FROM tblDirectory Dir INNER JOIN" + " tblDirectoryRelation DirRel ON Dir.nDirKey = DirRel.nDirParentId INNER JOIN" + " tblDirectoryRelation DirRel1 ON Dir.nDirKey = DirRel1.nDirParentId" + " WHERE (DirRel.nDirChildId = " + myWeb.mnUserId + ") AND (DirRel1.nDirChildId = Users.nDirKey))) IS NOT NULL)";
                                        cSql += " And (cDirSchema = 'User')";
                                    }
                                    else
                                    {
                                        cSql += " WHERE (cDirSchema = N'User') AND" + " (((SELECT TOP 1 DirRel1.nDirChildId AS OtherUser" + " FROM tblDirectory Dir INNER JOIN" + " tblDirectoryRelation DirRel ON Dir.nDirKey = DirRel.nDirParentId INNER JOIN" + " tblDirectoryRelation DirRel1 ON Dir.nDirKey = DirRel1.nDirParentId" + " WHERE (DirRel.nDirChildId = " + myWeb.mnUserId + ") AND (DirRel1.nDirChildId = Users.nDirKey) AND (Dir.cDirName = '" + cContentType + "'))) IS NOT NULL)";
                                        cSql += " And (cDirSchema = 'User')";
                                    }
                                }
                                else if (!string.IsNullOrEmpty(cContentType) & !(cContentType == "''"))
                                {
                                    cSql += " WHERE (cDirSchema = N'User') AND" + " (((SELECT nDirParentId" + " FROM tblDirectoryRelation " + " WHERE (nDirParentId = " + cContentType + ") AND (nDirChildId = Users.nDirKey))) IS NOT NULL)";
                                    cSql += " And (cDirSchema = 'User')";
                                }
                                else
                                {
                                    cSql += " WHERE (cDirSchema = 'User')";
                                }
                            }

                            if (!string.IsNullOrEmpty(sSearch))
                            {
                                if (!cSql.Contains(" WHERE "))
                                    cSql += " WHERE ";
                                else
                                    cSql += " AND ";
                                cSql += "(" + cSearchWhereUSER + " )";
                            }


                            cSql += " ORDER BY cDirName";
                            using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(cSql))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                {

                                    var oContent = moContextNode.OwnerDocument.CreateElement("Content");
                                    oContent.SetAttribute("id", Conversions.ToString(oDr["nDirKey"]));
                                    oContent.SetAttribute("type", "User");
                                    oContent.SetAttribute("name", Conversions.ToString(oDr["cDirName"]));
                                    oContent.InnerXml = Conversions.ToString(oDr["cDirXml"]);
                                    if (reMasterCheck.IsMatch(oContent.OuterXml))
                                    {
                                        nResultCount += 1;
                                        oContent.FirstChild.AppendChild(oContent.OwnerDocument.ImportNode(myWeb.moDbHelper.GetUserContactsXml(Convert.ToInt32(oDr["nDirKey"])).CloneNode(true), true));
                                        moContextNode.AppendChild(oContent);
                                    }
                                }
                                oDr.Close();
                            }
                        }

                        // Log the search
                        if (_logSearches)
                        {
                            myWeb.moDbHelper.logActivity(dbHelper.ActivityType.Search, myWeb.mnUserId, 0, 0, nResultCount, sSearch);
                        }

                        var oResXML = moPageXml.CreateElement("Content");
                        oResXML.SetAttribute("searchType", Conversions.ToString(Interaction.IIf(bUserQuery, "USER", "REGEX")));
                        oResXML.SetAttribute("SearchString", sSearch);
                        oResXML.SetAttribute("type", "SearchHeader");
                        oResXML.SetAttribute("Hits", nResultCount.ToString());
                        oResXML.SetAttribute("Time", DateTime.Now.Subtract(dtStart).TotalMilliseconds.ToString());
                        oResXML.SetAttribute("resultIds", cResultIDsCSV);
                        moContextNode.AppendChild(oResXML);



                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "RegExpQuery", ex, "", sProcessInfo, gbDebug);
                }
            }


            /// <summary>
            ///    This returns all valid content within a site for a daterange.
            ///    Parameters can be optionally called by unit (cUnit, cValue) or a specififed date range (cStartDate, cEndDate).
            /// </summary>
            /// <param name="cContentTypes">CSV list of content types to search for.</param>
            /// <param name="cUnit">Date period to search by : values are "Year","Month","Week","Day"</param>
            /// <param name="cValue">Number of units to search for</param>
            /// <param name="cStartDate">If searching for specific dates, then set this as the start of the range.</param>
            /// <param name="cEndDate">If searching for specific dates, then set this as the end of the range.</param>
            /// <param name="cSqlColumnToCheck">Specifies the preference on whether to check the Publish, Insert or Update column (values: PUBLISH,INSERT,UPDATE)</param>
            /// <remarks></remarks>
            protected void ContentByDateQuery(string cContentTypes, string cUnit = "", string cValue = "", string cStartDate = "", string cEndDate = "", string cSqlColumnToCheck = "PUBLISH")
            {

                myWeb.PerfMon.Log("Search", "LatestContentQuery");

                var dStart = default(DateTime);
                var dEnd = default(DateTime);

                var dtStart = DateTime.Now;

                var cRangeDate_Start = DateTime.Today;
                var cRangeDate_End = default(DateTime);

                string cSql = "";
                string cSqlDateRange = "";
                string cSqlColumn = "";

                // Retuns the number of results found.
                long nResultCount = 0L;

                try
                {

                    // Test for a value
                    if (Information.IsNumeric(cValue))
                    {

                        if (Conversions.ToDouble(cValue) != 0d)
                        {

                            // Valid value - now check the unit
                            switch (cUnit ?? "")
                            {
                                case "Day":
                                    {
                                        cRangeDate_End = DateTime.Today.AddDays(Conversions.ToDouble(cValue));
                                        break;
                                    }
                                case "Week":
                                    {
                                        cRangeDate_End = DateTime.Today.AddDays(Conversions.ToDouble(cValue) * 7d);
                                        break;
                                    }
                                case "Month":
                                    {
                                        cRangeDate_End = DateTime.Today.AddMonths(Conversions.ToInteger(cValue));
                                        break;
                                    }
                                case "Year":
                                    {
                                        cRangeDate_End = DateTime.Today.AddYears(Conversions.ToInteger(cValue));
                                        break;
                                    }

                            }

                            if (cRangeDate_End != DateTime.MinValue)
                            {

                                if (DateTime.Compare(cRangeDate_Start, cRangeDate_End) > 0)
                                {
                                    dStart = cRangeDate_End;
                                    dEnd = cRangeDate_Start;
                                }
                                else
                                {
                                    dStart = cRangeDate_Start;
                                    dEnd = cRangeDate_End;
                                }

                            }

                        }

                    }

                    // See if the previous code has returned any values
                    if (dStart == DateTime.MinValue)
                    {
                        // No values, let's see if any explicit dates have been requested.
                        if ((Information.IsDate(cStartDate) | cStartDate.ToLower() == "now") & (Information.IsDate(cEndDate) | cEndDate.ToLower() == "now"))
                        {
                            if (cStartDate.ToLower() == "now")
                                dStart = DateTime.Today;
                            else
                                dStart = Conversions.ToDate(cStartDate);
                            if (cEndDate.ToLower() == "now")
                                dEnd = DateTime.Today;
                            else
                                dEnd = Conversions.ToDate(cEndDate);
                        }
                    }

                    // We should now have a date range to run the search against
                    if (dStart != DateTime.MinValue & dEnd != DateTime.MinValue)
                    {

                        string cResultIDsCSV = "";
                        // Dim oDr As Data.SqlClient.SqlDataReader

                        // Set the column to search by
                        switch (Strings.UCase(cSqlColumnToCheck) ?? "")
                        {
                            case "INSERT":
                                {
                                    cSqlColumn = "dInsertDate";
                                    break;
                                }
                            case "UPDATE":
                                {
                                    cSqlColumn = "dUpdateDate";
                                    break;
                                }

                            default:
                                {
                                    cSqlColumn = "dPublishDate";
                                    break;
                                }
                        }

                        // Start search
                        dEnd = Conversions.ToDate(Strings.Format(dEnd, "dd-MMM-yyyy") + " 23:59:59");
                        cSqlDateRange = " sa." + cSqlColumn + " >=  " + Tools.Database.SqlDate(dStart);
                        cSqlDateRange += " AND sa." + cSqlColumn + " <=  " + Tools.Database.SqlDate(dEnd, true);


                        // Change contentType from CSV to CSV with quotes!
                        cContentTypes = Strings.Replace(cContentTypes, ",", "','");
                        cContentTypes = "'" + cContentTypes + "'";


                        // Ensure we get Distinct values
                        cSql = Conversions.ToString(Operators.ConcatenateObject("SELECT DISTINCT sc.nContentKey" + " FROM tblContent sc INNER JOIN tblAudit sa ON sc.nAuditId = sa.nAuditKey" + " WHERE (" + cSqlDateRange + ")", Interaction.IIf(string.IsNullOrEmpty(cContentTypes), "", " AND (sc.cContentSchemaName IN (" + cContentTypes + "))")));


                        using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(cSql))  // Done by nita on 6/7/22
                        {
                            while (oDr.Read())
                                cResultIDsCSV = Conversions.ToString(cResultIDsCSV + Operators.ConcatenateObject(oDr["nContentKey"], ","));
                            oDr.Close();
                        }

                        if (!string.IsNullOrEmpty(cResultIDsCSV))
                        {
                            // Add the content
                            nResultCount = GetContentXml(ref moContextNode, cResultIDsCSV, "a." + cSqlColumn + " DESC");
                        }
                    }


                    var oResXML = moPageXml.CreateElement("Content");
                    oResXML.SetAttribute("searchType", "LATESTCONTENT");
                    oResXML.SetAttribute("contentType", cContentTypes);
                    oResXML.SetAttribute("searchDateUnit", cUnit);
                    oResXML.SetAttribute("searchDateUnitTotal", cValue);
                    oResXML.SetAttribute("searchStartDate", cStartDate);
                    oResXML.SetAttribute("searchEndDate", cEndDate);
                    oResXML.SetAttribute("searchedRangeStart", Conversions.ToString(Interaction.IIf(dStart == DateTime.MinValue, "Could not evaluate", XmlDate(dStart))));
                    oResXML.SetAttribute("searchedRangeEnd", Conversions.ToString(Interaction.IIf(dEnd == DateTime.MinValue, "Could not evaluate", XmlDate(dEnd))));
                    oResXML.SetAttribute("type", "SearchHeader");
                    oResXML.SetAttribute("Hits", nResultCount.ToString());
                    oResXML.SetAttribute("Time", DateTime.Now.Subtract(dtStart).TotalMilliseconds.ToString());
                    moContextNode.AppendChild(oResXML);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, "Search", "SearchLatestContent", ex, "", bDebug: gbDebug);
                }

            }

            public virtual void BespokeQuery()
            {
                // placeholder for overloads

            }
            #endregion

            #region General Search Functions
            /// <summary>
            /// This is not needed at the moment.
            /// </summary>
            /// <remarks></remarks>
            private void SetSearchTrackingCookie()
            {
                myWeb.PerfMon.Log("Search", "SetSearchTrackingCookie");
                try
                {

                    // Only need a cookie if not logged in
                    if (myWeb.moSession != null)
                    {
                        // Search tracking cookie
                        // As the cookie is client readable, we should encrypt it in some way
                        // therefore let's store The Activity Key + a hash of the date and time and session id

                        // Get the last search for this session
                        // Dim lastSearch As SqlDataReader = myWeb.moDbHelper.getDataReader("SELECT TOP 1 nActivityKey,dDatetime,cSessionId FROM tblActivityLog WHERE cSessionId = " & Tools.Database.SqlString(myWeb.moSession.SessionID) & " ORDER BY 1 DESC")
                        using (SqlDataReader lastSearch = myWeb.moDbHelper.getDataReaderDisposable("SELECT TOP 1 nActivityKey,dDatetime,cSessionId FROM tblActivityLog WHERE cSessionId = " + Tools.Database.SqlString(myWeb.moSession.SessionID) + " ORDER BY 1 DESC"))  // Done by nita on 6/7/22
                        {
                            if (lastSearch.Read())
                            {

                                string cookieValue;
                                cookieValue = lastSearch[0].ToString() + "|";
                                cookieValue += Tools.Text.AscString(Tools.Encryption.HashString(Strings.Format(lastSearch[1].ToString(), "s") + lastSearch[2].ToString(), Tools.Encryption.Hash.Provider.Md5, false), "|");

                                var trackingCookie = new HttpCookie("search", cookieValue);
                                trackingCookie.Expires = DateTime.Now.AddDays(2d);

                                myWeb.moResponse.Cookies.Remove("search");
                                myWeb.moResponse.Cookies.Add(trackingCookie);

                            }
                            lastSearch.Close();
                        }
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "SetSearchTrackingCookie", ex, "", "", gbDebug);
                }
            }

            /// <summary>
            /// This takes a string and produces an array of the keywords and phrases, where keywords are individual words,
            /// and phrases are contained within quotemarks.   Phrases take precedence over keywords.
            /// </summary>
            /// <param name="searchString">The search string to parse</param>
            /// <returns></returns>
            /// <remarks></remarks>
            private string[] ParseKeywordsAndPhrases(string searchString)
            {
                myWeb.PerfMon.Log("Search", "ParseKeywordsAndPhrases");
                string processInfo = "Processing: " + searchString;

                try
                {

                    var terms = new ArrayList();
                    var term = new StringBuilder();
                    bool foundOpeningQuoteMark = false;
                    bool whiteSpaceAtBeginning = true;

                    foreach (char character in searchString)
                    {

                        if (Conversions.ToString(character) == " " & whiteSpaceAtBeginning)
                        {
                        }
                        // Ignore whitespace at the start of the search substring

                        else if (Conversions.ToString(character) == "\"")
                        {

                            // If a quotemark, then add it and work out whether to add it as a term
                            term.Append(character);

                            if (foundOpeningQuoteMark)
                            {

                                // This is the second quote - we have found a phrase
                                terms.Add(term.ToString());

                                foundOpeningQuoteMark = false;
                                whiteSpaceAtBeginning = true;
                                term = new StringBuilder();
                            }

                            else
                            {

                                // This is the first quote - flag it up
                                foundOpeningQuoteMark = true;
                                whiteSpaceAtBeginning = false;
                            }
                        }

                        else if (Conversions.ToString(character) == " " & !foundOpeningQuoteMark)
                        {

                            // For whitespace in normal circumstances, add the term
                            terms.Add(term.ToString());
                            whiteSpaceAtBeginning = true;
                            term = new StringBuilder();
                        }

                        // add something for the last term

                        else
                        {

                            // Add the character
                            term.Append(character);
                            whiteSpaceAtBeginning = false;
                        }

                    }

                    // Add the last term
                    if (!string.IsNullOrEmpty(term.ToString().Trim()))
                    {
                        terms.Add(term.ToString());
                    }

                    // Add the terms as a strign array
                    return (string[])terms.ToArray(typeof(string));
                }

                catch (Exception ex)
                {

                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ParseKeywordsAndPhrases", ex, "", processInfo, gbDebug);

                    var defaultString = new string[2];
                    defaultString[0] = searchString;
                    return defaultString;
                }

            }

            public string CleanSearchString(string cSearchString, string cReservedWords = "a|an|for|with|the|and|if|then|or|from|where", string cPunctuation = ",-.&+:;\"")
            {
                myWeb.PerfMon.Log("Search", "CleanSearchString");
                try
                {
                    // General RegEx Object
                    var reGeneral = new Regex(((int)RegexOptions.IgnoreCase).ToString());
                    // RegEx to search for special characters in the punctuation list variable
                    var reSpecialChars = new Regex(@"([\[\\\^\$\.\|\?\*\+\(\)])");
                    // Remove Reserved Words - replace with a space
                    cSearchString = Strings.Replace(cSearchString, @"\b(" + cReservedWords + @")\b", " ");
                    // Remove certain punctuation
                    cSearchString = Strings.Replace(cSearchString, "[" + reSpecialChars.Replace(cPunctuation, @"\$1") + "]", " ");
                    // Remove any repeated whitespace
                    cSearchString = Strings.Replace(cSearchString, @"\s+", " ");
                    // Remove any doublequotes
                    cSearchString = Strings.Replace(cSearchString, "\"", "");
                    // Finally Trim the WhiteSpace
                    return Strings.Trim(cSearchString);
                }
                catch
                {
                    return "";
                }
            }

            public int GetContentXml(ref XmlElement oContentElmt, string nContentIds, string cSqlOrderClause = "type, cl.nDisplayOrder")
            {
                myWeb.PerfMon.Log("Search", "GetContentXml");
                string sSql;
                string sProcessInfo = "building the Content XML";
                int nResultCount = 0;
                if (Strings.Left(nContentIds, 1) == ",")
                    nContentIds = Strings.Right(nContentIds, nContentIds.Length - 1);
                if (Strings.Right(nContentIds, 1) == ",")
                    nContentIds = Strings.Left(nContentIds, nContentIds.Length - 1);

                try
                {
                    // sSql = "select c.nContentKey as id, (select TOP 1 CL2.nStructId from tblContentLocation CL2 where CL2.nContentId=c.nContentKey and CL2.bPrimary = 1) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"
                    // sSql = "select c.nContentKey as id, 'Search' as source, dbo.fxn_getContentParents(c.nContentKey) as parId, c.cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey"
                    if (myWeb.mbAdminMode)
                    {
                        sSql = "select c.nContentKey as id,  dbo.fxn_getContentParents(c.nContentKey) as parId, c.cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey";
                        sSql += " where( C.nContentKey IN (" + nContentIds + ")";
                        sSql += ") ";
                        sSql += "AND (( SELECT tblContentStructure.nStructKey FROM tblContentStructure INNER JOIN tblAudit ON tblContentStructure.nAuditId = tblAudit.nAuditKey";
                        sSql += " WHERE (tblContentStructure.nStructKey = CL.nStructId)";
                        sSql += ")) != null";
                        sSql += " order by " + cSqlOrderClause;
                    }
                    else
                    {
                        sSql = "select c.nContentKey as id,  dbo.fxn_getContentParents(c.nContentKey) as parId, c.cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.nInsertDirId as owner from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey";
                        sSql += " where( C.nContentKey IN (" + nContentIds + ")";
                        sSql += " and nStatus = 1 ";
                        sSql += " and (dPublishDate is null or dPublishDate = 0 or dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + " )";
                        sSql += " and (dExpireDate is null or dExpireDate = 0 or dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + " )";
                        sSql += ") ";
                        sSql += "AND (( SELECT tblContentStructure.nStructKey FROM tblContentStructure INNER JOIN tblAudit ON tblContentStructure.nAuditId = tblAudit.nAuditKey";
                        sSql += " WHERE (tblAudit.nStatus = 1)";
                        sSql += " and (tblAudit.dPublishDate is null or tblAudit.dPublishDate = 0 or tblAudit.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + " )";
                        sSql += " and (tblAudit.dExpireDate is null or tblAudit.dExpireDate = 0 or tblAudit.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + " )";
                        sSql += " and (tblContentStructure.nStructKey = CL.nStructId)";
                        sSql += ")) is not null";
                        sSql += " order by " + cSqlOrderClause;
                    }

                    var oDs = new DataSet();
                    oDs = myWeb.moDbHelper.GetDataSet(sSql, "Content", "Contents");

                    nResultCount = oDs.Tables["Content"].Rows.Count;
                    DateTime? nulldate = null;
                    myWeb.moDbHelper.AddDataSetToContent(ref oDs,ref oContentElmt,ref nulldate, ref nulldate,  myWeb.mnPageId, default, "search");

                    // need to remove duplicate items

                    string[] oSplit = Strings.Split(nContentIds, ",");
                    int i;
                    int nRemoved = 0;
                    var loopTo = Information.UBound(oSplit);
                    for (i = 0; i <= loopTo; i++)
                    {
                        var oElmts = oContentElmt.SelectNodes("Content[@id=" + oSplit[i] + "]");
                        int nCount = 0;
                        foreach (XmlElement oElmt in oElmts)
                        {
                            if (nCount > 0)
                            {
                                oElmt.ParentNode.RemoveChild(oElmt);
                                nRemoved += 1;
                            }
                            nCount += 1;
                        }
                    }
                    nResultCount -= nRemoved;

                    return nResultCount;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getContentXml", ex, vstrFurtherInfo: sProcessInfo, bDebug: gbDebug);
                    return nResultCount;
                }

            }

            #endregion

            #region Index Query Functions
            /// <summary>
            /// <para>Builds a Lucene Query Parser based on the keyword string and predefined values in the request object</para>
            /// <list>
            /// <listheader>The keyword search does the following queries (in order of priority)</listheader>
            /// <item>The ss in quotemarks (i.e. as a phrase)</item>
            /// <item>The ss without quotemarks (i.e. as it comes)</item>
            /// <item>Each term in ss as a fuzzy search</item>
            /// </list>
            /// 
            /// </summary>
            /// <param name="keywordsToSearch"></param>
            /// <param name="filters"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            private Query BuildLuceneQuery(string keywordsToSearch, XmlElement filters, bool bShowHiddenForUser = false)
            {

                myWeb.PerfMon.Log("Search", "BuildLuceneQuery");
                string processInfo = "Looking for : " + keywordsToSearch;

                string fieldValue = "";
                string fieldName = "";
                string[] fieldTextTerms = null;
                string fieldMin = "";
                string fieldMax = "";

                try
                {

                    var analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29);
                    var parser = new QueryParser(Lucene.Net.Util.Version.LUCENE_29, "text", analyzer);
                    var queryBuilder = new BooleanQuery();
                    var queryToBeParsed = new StringBuilder();
                    string[] queryTerms = null;
                    string keywords = "";

                    // Tidy up the searchString
                    keywords = CleanUpLuceneSearchString(keywordsToSearch, !_overrideQueryBuilder);

                    // Choose if a manual override has been set
                    if (_overrideQueryBuilder)
                    {

                        queryToBeParsed.Append(keywords);
                    }

                    else
                    {

                        // Build a prioritised query
                        // We could build this through Lucene objects, or we could build this as a string
                        // and let the QueryParser do the honours.

                        queryTerms = ParseKeywordsAndPhrases(keywords);

                        // Prioritise the name field
                        BuildLuceneKeywordQuery(ref queryToBeParsed, queryTerms, "name", 3, _includeFuzzySearch);

                        // Default field search
                        queryToBeParsed.Append(" OR ");
                        BuildLuceneKeywordQuery(ref queryToBeParsed, queryTerms, "", 1, _includeFuzzySearch);
                        // apply status filter to show only active Products
                        if (Strings.LCase(moConfig["IndexIncludesHidden"]) == "on")
                        {
                            if (!bShowHiddenForUser)
                            {
                                queryToBeParsed.Append(" AND ");
                                queryTerms = ParseKeywordsAndPhrases("1");
                                BuildLuceneKeywordQuery(ref queryToBeParsed, queryTerms, "status", 1, _includeFuzzySearch);
                            }
                        }
                    }

                    // Prefix name search is an optional hardcoded search that prefix searches the qsname field
                    // Which represents an untokenized version of the name field.
                    // In other words if you include this option, you will need to update your index to 
                    // index the name field under qsname lowercased untokenized.
                    // e.g. <meta name="qsname" content="{$displayNameToLower}" tokenize="false" />
                    Query keywordQuery;
                    if (moConfig["SiteWildCardSearch"] == "on")
                    {
                        parser.AllowLeadingWildcard = true;
                        keywordQuery = parser.Parse($"*{keywordsToSearch}*");
                    }
                    else
                    {
                        keywordQuery = parser.Parse(queryToBeParsed.ToString());
                    }

                    var booleanQ = new BooleanQuery();
                    if (_includePrefixNameSearch & !_overrideQueryBuilder)
                    {

                        booleanQ.Add(keywordQuery, Occur.SHOULD);
                        booleanQ.Add(new PrefixQuery(new Term("qsname", keywords)), Occur.SHOULD);
                        queryBuilder.Add(booleanQ, Occur.MUST);
                    }
                    else
                    {
                        booleanQ.Add(keywordQuery, Occur.MUST);
                        booleanQ.Add(new PrefixQuery(new Term("qsnoname", keywords)), Occur.MUST_NOT);
                        queryBuilder.Add(booleanQ, Occur.MUST);
                    }


                    // Process any additional search criteria
                    if (filters != null)
                    {
                        foreach (XmlElement searchFilter in filters)
                        {

                            fieldName = searchFilter.GetAttribute("field");

                            switch (searchFilter.GetAttribute("type") ?? "")
                            {

                                case "term":
                                    {

                                        // Clean up the search terms
                                        fieldValue = CleanUpLuceneSearchString(searchFilter.GetAttribute("value"), true);
                                        fieldTextTerms = ParseKeywordsAndPhrases(fieldValue);

                                        // Field keyword search
                                        queryToBeParsed = new StringBuilder();
                                        BuildLuceneKeywordQuery(ref queryToBeParsed, fieldTextTerms, fieldName, 3, _includeFuzzySearch);
                                        queryBuilder.Add(parser.Parse(queryToBeParsed.ToString()), Occur.MUST);
                                        break;
                                    }


                                case "range":
                                    {

                                        fieldMin = searchFilter.GetAttribute("min");
                                        fieldMax = searchFilter.GetAttribute("max");

                                        switch (searchFilter.GetAttribute("fieldType") ?? "")
                                        {

                                            case "number":
                                            case "integer":
                                                {
                                                    int minNumber;
                                                    int maxNumber;

                                                    if (!string.IsNullOrEmpty(fieldMin) && Information.IsNumeric(fieldMin))
                                                    {
                                                        minNumber = Convert.ToInt16(fieldMin);
                                                    }
                                                    else
                                                    {
                                                        minNumber = int.MinValue;
                                                    }

                                                    if (!string.IsNullOrEmpty(fieldMax) && Information.IsNumeric(fieldMax))
                                                    {
                                                        maxNumber = Convert.ToInt16(fieldMax);
                                                    }
                                                    else
                                                    {
                                                        maxNumber = int.MaxValue;
                                                    }
                                                    Query numericQuery = NumericRangeQuery.NewIntRange(fieldName, minNumber, maxNumber, true, true);
                                                    queryBuilder.Add(numericQuery, Occur.MUST);
                                                    break;
                                                }

                                            case "float":
                                                {
                                                    float minNumber;
                                                    float maxNumber;

                                                    if (!string.IsNullOrEmpty(fieldMin) && Information.IsNumeric(fieldMin))
                                                    {
                                                        minNumber = Convert.ToSingle(fieldMin);
                                                    }
                                                    else
                                                    {
                                                        minNumber = float.MinValue;
                                                    }

                                                    if (!string.IsNullOrEmpty(fieldMax) && Information.IsNumeric(fieldMax))
                                                    {
                                                        maxNumber = Convert.ToSingle(fieldMax);
                                                    }
                                                    else
                                                    {
                                                        maxNumber = float.MaxValue;
                                                    }
                                                    Query numericQuery = NumericRangeQuery.NewFloatRange(fieldName, minNumber, maxNumber, true, true);
                                                    queryBuilder.Add(numericQuery, Occur.MUST);
                                                    break;
                                                }

                                            default:
                                                {

                                                    var termQuery = new TermRangeQuery(fieldName, fieldMin, fieldMax, true, true);
                                                    queryBuilder.Add(termQuery, Occur.MUST);
                                                    break;
                                                }


                                        }

                                        break;
                                    }
                            }

                        }
                    }



                    return queryBuilder;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "BuildLuceneQuery", ex, "", processInfo, gbDebug);
                    return null;
                }

            }

            /// <summary>
            /// Returns a Lucene Filter of the live pages
            /// 
            /// </summary>
            /// <returns></returns>
            /// <remarks>
            /// Justification for this is that Lucene may become out of date if a page is hidden. 
            /// If we return a large result set, then we would have to trawl through all the results
            /// checking if they on a live page = Fail.
            /// It's quicker to create a live page filter here and filter that into lucene.
            /// </remarks>
            private Filter LivePageLuceneFilter()
            {

                try
                {

                    // Live page check - to avoid checking each result from EonicWeb, we factor the current live pages into the search.
                    // This means we can leverage a true search result set from Lucene and paging becomes a helluvalot easier

                    var livePagesQuery = new BooleanQuery();
                    XmlNodeList livePages = myWeb.moPageXml.SelectNodes("/Page/Menu/descendant-or-self::MenuItem");
                    BooleanQuery.MaxClauseCount = Math.Max(BooleanQuery.MaxClauseCount, livePages.Count);
                    foreach (XmlElement livePage in livePages)
                        livePagesQuery.Add(new BooleanClause(new TermQuery(new Term("pgid", livePage.GetAttribute("id"))), Occur.SHOULD));

                    return new QueryWrapperFilter(livePagesQuery);
                }

                catch (Exception ex)
                {
                    return null;
                }

            }

            private Filter LivePageLuceneFilter(ref Protean.Rest myApi)
            {

                try
                {

                    // Live page check - to avoid checking each result from EonicWeb, we factor the current live pages into the search.
                    // This means we can leverage a true search result set from Lucene and paging becomes a helluvalot easier

                    var livePagesQuery = new BooleanQuery();
                    var myWeb = new Cms(myApi.moCtx);
                    myWeb.Open();

                    XmlElement siteStructure = myWeb.GetStructureXML(myWeb.mnUserId);
                    var livePages = siteStructure.SelectNodes("*/descendant-or-self::MenuItem");
                    BooleanQuery.MaxClauseCount = Math.Max(BooleanQuery.MaxClauseCount, livePages.Count);
                    foreach (XmlElement livePage in livePages)
                        livePagesQuery.Add(new BooleanClause(new TermQuery(new Term("pgid", livePage.GetAttribute("id"))), Occur.SHOULD));

                    return new QueryWrapperFilter(livePagesQuery);
                }

                catch (Exception ex)
                {
                    return null;
                }

            }


            private void BuildLuceneKeywordQuery(ref StringBuilder queryBuilder, string[] keywords, string fieldName = "", int boostBase = 1, bool includeFuzzySearch = false)
            {

                myWeb.PerfMon.Log("Search", "BuildLuceneKeywordQuery");
                string processInfo = "";
                bool firstItem = false;
                // Dim queryBuilder1 As New StringBuilder
                // Dim fieldName1 As String = "status"

                try
                {
                    // Text query 1:
                    // Remove all quotes and put the whole thing in quotes
                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        queryBuilder.Append(fieldName).Append(":");
                        // queryBuilder1.Append(fieldName1).Append(":")
                    }
                    queryBuilder.Append("\"");
                    // queryBuilder1.Append("""")
                    foreach (string keyword in keywords)
                    {
                        queryBuilder.Append(keyword.Replace("\"", ""));
                        queryBuilder.Append(" ");
                    }
                    // queryBuilder1.Append("1")
                    // queryBuilder1.Append(" ")
                    queryBuilder.Append("\"^").Append((boostBase + 2).ToString());
                    // queryBuilder1.Append("""^").Append((boostBase + 2).ToString)

                    // Text query 2 - as it comes:
                    if (keywords.Length > 1)
                    {
                        queryBuilder.Append(" OR (");
                        firstItem = true;
                        foreach (string keyword in keywords)
                        {
                            if (!firstItem)
                            {
                                queryBuilder.Append(" AND ");
                            }
                            else
                            {
                                firstItem = false;
                            }
                            if (!string.IsNullOrEmpty(fieldName))
                                queryBuilder.Append(fieldName).Append(":");
                            queryBuilder.Append(keyword);
                            queryBuilder.Append("^").Append((boostBase + 1).ToString());
                        }
                        queryBuilder.Append(") ");
                    }

                    // Text query 3 - fuzzy search:
                    if (keywords.Length > 0 & includeFuzzySearch)
                    {
                        queryBuilder.Append(" OR (");
                        firstItem = true;
                        foreach (string keyword in keywords)
                        {
                            if (!firstItem)
                            {
                                queryBuilder.Append(" AND ");
                            }
                            else
                            {
                                firstItem = false;
                            }
                            if (!string.IsNullOrEmpty(fieldName))
                                queryBuilder.Append(fieldName).Append(":");
                            queryBuilder.Append(keyword);
                            queryBuilder.Append("~^").Append(boostBase.ToString());
                        }
                        // queryBuilder.Append(") OR (")
                        // queryBuilder.Append(queryBuilder1)
                        queryBuilder.Append(") ");
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "BuildLuceneKeywordQuery", ex, "", processInfo, gbDebug);
                }

            }


            private Sort SetSortFieldFromRequest(HttpRequest currentRequest)
            {
                myWeb.PerfMon.Log("Search", "SetSortFieldFromRequest");
                string processInfo = "";

                try
                {

                    string sortFieldName = "" + currentRequest["sortCol"];
                    string sortDir = "" + currentRequest["sortDir"];
                    string sortFieldType = "" + currentRequest["sortColType"];
                    bool sortOverridePrefixes = currentRequest["sortOverrideEWPrefixes"] == "true";
                    var returnedSort = new Sort();

                    if (!string.IsNullOrEmpty(sortFieldName))
                    {

                        if (!sortOverridePrefixes)
                            sortFieldName = "ewsort-" + sortFieldName;

                        int fieldToSortType;
                        switch (sortFieldType.ToLower() ?? "")
                        {
                            case "number":
                            case "float":
                                {
                                    fieldToSortType = SortField.FLOAT;
                                    break;
                                }

                            default:
                                {
                                    fieldToSortType = SortField.STRING;
                                    break;
                                }
                        }


                        returnedSort.SetSort(new SortField(sortFieldName, fieldToSortType, sortDir.ToLower() == "desc"));
                    }

                    else
                    {

                        // Set the default sort order - relevance
                        returnedSort.SetSort(SortField.FIELD_SCORE);

                    }

                    return returnedSort;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "SetSortFieldFromRequest", ex, "", processInfo, gbDebug);
                    return null;
                }
            }

            public static string CleanUpLuceneSearchString(string keywords, bool escapeLuceneChars = true)
            {
                try
                {
                    // Tidy up 1: make sure we have an even number of quotes
                    // If not, remove the last quote
                    // e.g. from "My test" with this"
                    // to   "My test" with this

                    keywords = keywords.Replace("AND", "and");

                    if (IsOdd(Regex.Matches(keywords, "\"").Count))
                    {
                        // remove the last quote
                        keywords = Tools.Text.ReplaceLastCharacter(ref keywords, '"', " ");
                    }

                    // Tidy up 2 : collapse the whitespace
                    // e.g. from "foo     foo" to "foo foo"
                    keywords = Regex.Replace(keywords, @"\s\s+", " ");

                    // Tidy up 3: escape special characters
                    if (escapeLuceneChars)
                        keywords = Regex.Replace(keywords, @"[\\\+\-\!\(\)\{\}\[\]\^\~\*\?\:]", @"\$0");

                    return keywords;
                }
                catch (Exception ex)
                {

                    return "";
                }

            }

            #endregion

        }

    }
}