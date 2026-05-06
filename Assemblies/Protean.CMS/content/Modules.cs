using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Web.Configuration;
using System.Xml;
using static Protean.stdTools;

using Protean.Providers.Filters;


namespace Protean
{

    public partial class Cms
    {
        public partial class Content
        {

            #region Declarations

            private Cms myWeb;

            //public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            public event OnErrorWithWebEventHandler OnErrorWithWeb;

            public delegate void OnErrorWithWebEventHandler(ref Cms myweb, object sender, Tools.Errors.ErrorEventArgs e);
            private const string mcModuleName = "Protean.Cms.Content";

            #endregion


            #region Module Behaviour

            public class Modules
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Protean.Cms.Content.Modules";

                public Modules()
                {

                    // do nowt

                }

                public void Filters(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {
                    }

                    catch (Exception)
                    {

                    }
                }

                public void NewsByDate(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {
                        string datestring = string.Empty;
                        var startDate = default(DateTime);
                        DateTime? endDate = default;
                        string dateQuery = "";
                        string cOrigUrl = myWeb.mcOriginalURL;
                        string cOrigQS = "";
                        string cPageURL = myWeb.mcPagePath.TrimEnd('/');
                        string thisId = oContentNode.GetAttribute("id");
                        var PageDate = myWeb.mdDate;
                        thisId = thisId + "-";
                        thisId = "ByDate";

                        // Overide for testing
                        // PageDate = New Date(2016, 3, 15)

                        // handle querystrings
                        if (myWeb.mcOriginalURL.Contains("?"))
                        {
                            cOrigUrl = myWeb.mcOriginalURL.Split('?')[0];
                            cOrigQS = "?" + myWeb.mcOriginalURL.Split('?')[1];
                        }
                        cOrigQS = "";
                        dateQuery = myWeb.moRequest["bydate"];

                        string thisDateQuery;
                        // Week start date...
                        // Get ranges for articles on this page

                        string sFilterSql = myWeb.GetStandardFilterSQLForContent();
                        string sSql = "select  a.dpublishDate as publish from tblContent c" + " inner join tblContentLocation CL on c.nContentKey = CL.nContentId" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where( CL.nStructId = " + myWeb.mnPageId;
                        sSql = sSql + sFilterSql + " and c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "') order by a.dpublishDate desc";

                        var DateSet = myWeb.moDbHelper.GetDataSet(sSql, "ArticleDates");
                        var dEarliestDate = PageDate;
                        // Latest Articles
                        int nFirstPageCount = Convert.ToInt16("0" + oContentNode.GetAttribute("firstPageCount"));
                        DateTime FirstPageLastDate = default;
                        long counter = nFirstPageCount;

                        foreach (DataRow dr in DateSet.Tables[0].Rows)
                        {
                            if (DateTime.TryParse(dr["publish"]?.ToString(), out DateTime publishDate))
                            {
                                if (publishDate < dEarliestDate)
                                {
                                    dEarliestDate = publishDate;
                                }

                                counter -= 1;

                                if (counter == 0)
                                {
                                    FirstPageLastDate = publishDate;
                                }
                            }
                        }

                        var mondayDate = PageDate;
                        while (mondayDate.DayOfWeek != DayOfWeek.Monday)
                            mondayDate = mondayDate.AddDays((double)-1);

                        var oContent = new Protean.xmlTools.ewXmlElement(oContentNode);
                        var NewMenu = oContent.AddElement("Menu");
                        int contentCount = 0;
                        NewMenu.XmlElement.SetAttribute("id", "newsByDate");

                        if (nFirstPageCount > 0)
                        {

                            thisDateQuery = "latest";
                            NewMenu.AddMenuItem("Latest Articles", thisDateQuery, cOrigUrl + "?" + thisId + "=" + thisDateQuery + cOrigQS, contentCount: contentCount);
                            if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                            {
                                startDate = FirstPageLastDate;
                                endDate = PageDate;
                                dateQuery = thisDateQuery;
                            }
                        }


                        // This Week
                        contentCount = this.getArticleCount(DateSet, mondayDate, PageDate);
                        if (contentCount > 0)
                        {
                            thisDateQuery = "thisweek";
                            NewMenu.AddMenuItem("This Week", thisDateQuery, cOrigUrl + "?" + thisId + "=" + thisDateQuery + cOrigQS, contentCount: contentCount);
                            if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                            {
                                startDate = mondayDate;
                                endDate = PageDate;
                                dateQuery = thisDateQuery;
                            }
                        }

                        // Last Week
                        contentCount = this.getArticleCount(DateSet, mondayDate.AddDays((double)-8), mondayDate.AddDays((double)-1));
                        if (contentCount > 0)
                        {
                            thisDateQuery = "lastweek";
                            NewMenu.AddMenuItem("Last Week", thisDateQuery, cOrigUrl + "?" + thisId + "=" + thisDateQuery + cOrigQS, contentCount: contentCount);
                            if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                            {
                                startDate = mondayDate.AddDays((double)-8);
                                endDate = mondayDate.AddDays((double)-1);
                                dateQuery = thisDateQuery;
                            }
                        }

                        // This Month
                        object firstDayMonth = new DateTime(PageDate.Year, PageDate.Month, 1);
                        contentCount = this.getArticleCount(DateSet, Convert.ToDateTime(firstDayMonth), PageDate);
                        if (contentCount > 0)
                        {
                            thisDateQuery = "thismonth";
                            NewMenu.AddMenuItem("This Month", thisDateQuery, cOrigUrl + "?" + thisId + "=" + thisDateQuery + cOrigQS, contentCount: contentCount);
                            if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                            {
                                startDate = Convert.ToDateTime(firstDayMonth);
                                endDate = PageDate;
                                dateQuery = thisDateQuery;
                            }
                        }

                        // Step through this years months
                        int nPrevMonths = Convert.ToInt16("0" + oContentNode.GetAttribute("previousMonthsListed"));
                        if (nPrevMonths == 0)
                            nPrevMonths = 12;
                        int thisCount = 1;
                        short nThisMonth = (short)(PageDate.Month - 1);
                        short nThisYear = (short)PageDate.Year;
                        while (nThisMonth != 0 & thisCount <= nPrevMonths)
                        {
                            object firstDayloopMonth = new DateTime(PageDate.Year, (int)nThisMonth, 1);
                            contentCount = this.getArticleCount(DateSet, Convert.ToDateTime(firstDayloopMonth), dhLastDayInMonth(Convert.ToDateTime(firstDayloopMonth)));
                            if (contentCount > 0)
                            {
                                thisDateQuery = nThisYear + "-" + nThisMonth;
                                NewMenu.AddMenuItem(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(nThisMonth) + " " + nThisYear, thisDateQuery, cOrigUrl + "?" + thisId + "=" + thisDateQuery + cOrigQS, contentCount: contentCount);

                                if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                                {
                                    startDate = Convert.ToDateTime(firstDayloopMonth);
                                    endDate = dhLastDayInMonth(Convert.ToDateTime(firstDayloopMonth));
                                    dateQuery = thisDateQuery;
                                }
                            }
                            nThisMonth = (short)(nThisMonth - 1);
                            thisCount = thisCount + 1;
                        }
                        nThisYear = (short)(nThisYear - 1);

                        if (nPrevMonths < 12 & nThisMonth > 0)
                        {
                            var lastMonthDate = new DateTime(PageDate.Year, (int)nThisMonth, 1);
                            lastMonthDate = dhLastDayInMonth(lastMonthDate);
                            object firstDayYear = new DateTime(PageDate.Year, 1, 1);
                            contentCount = this.getArticleCount(DateSet, Convert.ToDateTime(firstDayYear), lastMonthDate);
                            if (contentCount > 0)
                            {
                                thisDateQuery = "restofyear";
                                NewMenu.AddMenuItem("Rest of " + PageDate.Year, thisDateQuery, cOrigUrl + "?" + thisId + "=" + thisDateQuery + cOrigQS, contentCount: contentCount);
                                if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                                {
                                    startDate = Convert.ToDateTime(firstDayYear);
                                    endDate = lastMonthDate;
                                    dateQuery = thisDateQuery;
                                }
                            }
                        }


                        // Step through previous years 
                        short nYearOfOldestArticle = (short)dEarliestDate.Year;
                        while (nThisYear >= nYearOfOldestArticle)
                        {
                            object firstDayloopYear = new DateTime(nThisYear, 1, 1);
                            object lastDayloopYear = new DateTime(nThisYear, 12, 31);
                            contentCount = this.getArticleCount(DateSet, Convert.ToDateTime(firstDayloopYear), Convert.ToDateTime(lastDayloopYear));
                            if (contentCount > 0)
                            {
                                thisDateQuery = nThisYear.ToString();
                                string sClass = null;
                                if (thisDateQuery == dateQuery) {
                                    sClass = "active";
                                }

                                NewMenu.AddMenuItem(nThisYear.ToString(), thisDateQuery, cOrigUrl + "?" + thisId + "=" + thisDateQuery + cOrigQS, null ,null, contentCount, sClass);
                                if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                                {
                                    startDate = Convert.ToDateTime(firstDayloopYear);
                                    endDate = (DateTime?)lastDayloopYear;
                                    dateQuery = thisDateQuery;
                                }
                            }

                            nThisYear = (short)(nThisYear - 1);
                        }

                        if ((myWeb.mbAdminMode & (DateTime.Today is var arg1 && endDate.HasValue ? endDate.Value == arg1 : (bool?)null)) == true)
                        {
                            // Get content by date range and future posts
                            if (startDate == DateTime.MinValue)
                            {
                                myWeb.mbCheckDetailPath = false;
                                myWeb.msRedirectOnEnd = null;
                                int argnCount = 0;
                                XmlElement argoContentsNode = null;
                                XmlElement argoPageDetail = null;
                                myWeb.GetPageContentFromSelect("CL.nStructId = " + myWeb.mnPageId + " And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "'", nCount: ref argnCount, oContentsNode: ref argoContentsNode, oPageDetail: ref argoPageDetail);
                            }
                            else
                            {
                                myWeb.mbCheckDetailPath = false;
                                myWeb.msRedirectOnEnd = null;
                                int argnCount1 = 0;
                                XmlElement argoContentsNode1 = null;
                                XmlElement argoPageDetail1 = null;
                                myWeb.GetPageContentFromSelect("CL.nStructId = " + myWeb.mnPageId + " And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "' and a.dpublishDate >= " + sqlDate(startDate), nCount: ref argnCount1, oContentsNode: ref argoContentsNode1, oPageDetail: ref argoPageDetail1);
                            }
                        }
                        else
                        {
                            string endstr;
                            if (endDate is null)
                            {
                                endstr = "";
                            }
                            else
                            {
                                endstr = " and a.dpublishDate <= " + sqlDate(endDate);
                            }

                            // Get content by date range
                            if (startDate == DateTime.MinValue)
                            {
                                myWeb.mbCheckDetailPath = false;
                                myWeb.msRedirectOnEnd = null;
                                int argnCount2 = 0;
                                XmlElement argoContentsNode2 = null;
                                XmlElement argoPageDetail2 = null;
                                myWeb.GetPageContentFromSelect("CL.nStructId = " + myWeb.mnPageId + " And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "'" + endstr, nCount: ref argnCount2, oContentsNode: ref argoContentsNode2, oPageDetail: ref argoPageDetail2);
                            }
                            else
                            {
                                myWeb.mbCheckDetailPath = false;
                                myWeb.msRedirectOnEnd = null;
                                int argnCount3 = 0;
                                XmlElement argoContentsNode3 = null;
                                XmlElement argoPageDetail3 = null;
                                string cShowRelatedBriefDepth =myWeb.moConfig["ShowRelatedBriefDepth"] + "";                               
                                myWeb.GetPageContentFromSelect("CL.nStructId = " + myWeb.mnPageId + " And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "' and a.dpublishDate >= " + sqlDate(startDate) + endstr, nCount: ref argnCount3, oContentsNode: ref argoContentsNode3, oPageDetail: ref argoPageDetail3);
                            }
                        }
                                         
                        // remove content detail
                        if (myWeb.mnArtId == Convert.ToInt16(oContentNode.GetAttribute("id")))
                        {
                            myWeb.moPageXml.DocumentElement.RemoveChild(myWeb.moPageXml.DocumentElement.SelectSingleNode("ContentDetail"));
                            myWeb.moContentDetail = (XmlElement)null;
                            myWeb.moPageXml.DocumentElement.RemoveAttribute("artid");
                            myWeb.mnArtId = default(int);
                        } 
                        oContent.XmlElement.SetAttribute("dateQuery", dateQuery);
                        oContentNode = oContent.XmlElement;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                }

                public int getArticleCount(DataSet ods, DateTime startDate, DateTime endDate)
                {
                    int ReturnCount = 0;
                    foreach (DataRow dr in ods.Tables[0].Rows)
                    {
                        if (DateTime.TryParse(dr["publish"]?.ToString(), out DateTime publishDate))
                        {
                            if (publishDate >= startDate && publishDate <= endDate)
                            {
                                ReturnCount += 1;
                            }
                        }
                    }
                    return ReturnCount;
                }

                public DateTime dhLastDayInMonth(DateTime dtmDate)
                {
                    return new DateTime( System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(dtmDate), System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMonth(dtmDate) + 1, 1).AddDays(-1);
                }

                public void ProductStepper(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {
                        string cOrigUrl = myWeb.mcOriginalURL;
                        string cOrigQS = "";
                        string cPageURL = "";
                        if (myWeb.mcPagePath != null)
                        {
                            cPageURL = myWeb.mcPagePath.TrimEnd('/');
                        }
                        long nItemsPerPage = Convert.ToInt64(oContentNode.GetAttribute("stepCount"));
                        long nCurrentPage = 1L;
                        long itemCount = Convert.ToInt64(myWeb.moDbHelper.GetDataValue(@"select count(nContentKey) from tblContent c inner join tblContentLocation cl on c.nContentKey =  cl.nContentId
                        where cl.nStructId = " + myWeb.mnPageId));
                        oContentNode.SetAttribute("itemCount", itemCount.ToString());
                        if (!string.IsNullOrEmpty(myWeb.moRequest["startPos" + oContentNode.GetAttribute("id")]))
                        {
                            nCurrentPage = (long)Math.Round((double)Convert.ToInt64(myWeb.moRequest["startPos" + oContentNode.GetAttribute("id")]) / (double)nItemsPerPage + 1d);
                        }
                        // handle querystrings
                        if (myWeb.mcOriginalURL.Contains("?"))
                        {
                            cOrigUrl = myWeb.mcOriginalURL.Split('?')[0];
                            cOrigQS = "?" + myWeb.mcOriginalURL.Split('?')[1];
                        }

                        // Get content by date range
                        XmlElement xmlContentNode = null; int nCount = 0;
                        XmlElement argoPageDetail = null;
                        myWeb.GetPageContentFromSelect("CL.nStructId = " + myWeb.mnPageId + " And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "' ", ref nCount, ref xmlContentNode, oPageDetail: ref argoPageDetail, nReturnRows: (int)nItemsPerPage, pageNumber: nCurrentPage);
                        // remove content detail
                        if (myWeb.moContentDetail != null)
                        {
                            myWeb.moPageXml.DocumentElement.RemoveChild(myWeb.moPageXml.DocumentElement.SelectSingleNode("ContentDetail"));
                            myWeb.moContentDetail = (XmlElement)null;
                            myWeb.moPageXml.DocumentElement.RemoveAttribute("artid");
                            myWeb.mnArtId = default(int);
                        }
                    }


                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                }


                public void ListHistoricEvents(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    string cProcessInfo = "ListHistoricEvents";
                    string PageId = oContentNode.GetAttribute("grabberRoot");
                    if (string.IsNullOrEmpty(PageId))
                    {
                        PageId = myWeb.mnPageId.ToString();
                    }
                    long nItemsPerPage = 0L;
                    long nCurrentPage = 1L;

                    try
                    {
                        if (!string.IsNullOrEmpty(PageId))
                        {
                            if (oContentNode.GetAttribute("display") == "related")
                            {
                                int contentId = Convert.ToInt16(oContentNode.GetAttribute("id"));
                                myWeb.mbAdminMode = true;
                                myWeb.moDbHelper.addRelatedContent(ref oContentNode, contentId, true);
                                myWeb.mbAdminMode = false;
                            }
                            // myWeb.moDbHelper.getRelationsByContentId(contentId, oContentNode, 2)
                            else
                            {
                                XmlElement argoPageDetail = null;
                                XmlElement xmlContentNode = null; int nCount = 0;
                                myWeb.GetPageContentFromSelect("CL.nStructId = " + PageId + " And a.dExpireDate < GETDATE() And a.nStatus = 1 And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "' ", ref nCount, ref xmlContentNode, oPageDetail: ref argoPageDetail, nReturnRows: (int)nItemsPerPage, pageNumber: nCurrentPage, ignoreActiveAndDate: true);
                            }



                            myWeb.bAllowExpired = true;
                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ListHistoricEvents", ex, "", cProcessInfo, gbDebug);
                    }
                }


                /// <summary>
                /// CONTENT FILTER ORCHESTRATION MODULE
                /// ===================================
                /// 
                /// This method orchestrates multiple content filter providers using a reflection-based provider pattern.
                /// It coordinates the filter lifecycle through five distinct phases:
                /// 
                /// ARCHITECTURE OVERVIEW:
                /// ---------------------
                /// 1. INITIALIZATION PHASE (Lines 460-500)
                ///    - Creates XForm for filter UI
                ///    - Initializes SQL composition variables (WHERE, ORDER BY, GROUP BY, JOINs)
                ///    - Determines filter target content type (usually "Product")
                ///    - Handles "clear filters" redirect
                /// 
                /// 2. PROVIDER INTEGRATION POINT #1: AddControl (Lines 504-575)
                ///    - Iterates through all filter Content nodes with @type='Filter' and @providerName!=''
                ///    - For each filter:
                ///      a) Loads provider Type via reflection (WebConfigurationManager or Type.GetType)
                ///      b) Creates instance with Activator.CreateInstance
                ///      c) Invokes AddControl method with parameters:
                ///         - myWeb: CMS context
                ///         - oFilterElmt: Filter configuration XML
                ///         - filterForm: XForm instance for UI generation
                ///         - oFrmGroup: Form group to add controls to
                ///         - oContentNode: Parent content node
                ///         - cWhereSql: WHERE clause from other filters (for dynamic option filtering)
                ///    - AddControl is responsible for:
                ///      * Restoring filter state from form submission
                ///      * Creating filter UI (checkboxes, selects, etc.)
                ///      * Adding removal buttons for selected values
                ///      * Populating options from database
                /// 
                /// 3. FORM SUBMISSION PHASE (Lines 598-605)
                ///    - Checks if form was submitted
                ///    - Updates XForm instance from request data
                ///    - Validates form inputs
                /// 
                /// 4. PROVIDER INTEGRATION POINT #2-5: SQL Composition (Lines 609-735)
                ///    - Iterates through filter providers again (only if form valid)
                ///    - For each filter, invokes multiple methods:
                /// 
                ///      INTEGRATION POINT #2: ApplyFilter (Line 663)
                ///      - Parameters: myWeb, whereSQL (cumulative), filterForm, oFrmGroup, oFilterElmt, cFilterTarget
                ///      - Returns: Updated WHERE clause SQL (appends to existing whereSQL)
                ///      - Purpose: Converts filter selections into SQL WHERE conditions
                ///      - Example: "AND c.nContentKey IN (SELECT nContentId FROM tblContentIndex WHERE cIndexValue IN ('Brand1','Brand2'))"
                /// 
                ///      INTEGRATION POINT #3: GetFilterGroupByClause (Line 669)
                ///      - Parameters: myWeb
                ///      - Returns: GROUP BY clause SQL (if provider needs grouping)
                ///      - Purpose: Allows filters to specify result grouping (e.g., for aggregations)
                ///      - Stored in cGroupBySql variable
                /// 
                ///      INTEGRATION POINT #4: GetFilterOrderByClause (Line 674)
                ///      - Parameters: myWeb
                ///      - Returns: ORDER BY clause SQL (provider-specific sorting)
                ///      - Purpose: Allows filters to control result ordering
                ///      - Handles duplicate ORDER BY prevention (Lines 677-704)
                ///      - Stored in cOrderBySql variable
                /// 
                ///      INTEGRATION POINT #5: GetContentIndexDefinationName (Line 714)
                ///      - Parameters: myWeb
                ///      - Returns: Content index definition name for JOIN clause
                ///      - Purpose: Determines which content index to join for sorting
                ///      - Used to build additional JOINs (Lines 717-719):
                ///        * inner join tblContentIndex cii{Alias}
                ///        * inner join tblContentIndexDef cid{Alias}
                ///      - Excluded for certain filters via ExcludeFilterForJoin config
                /// 
                /// 5. RESULT EXECUTION PHASE (Lines 760-800)
                ///    - Combines all SQL fragments (WHERE + ORDER BY + GROUP BY + JOINs)
                ///    - Calls myWeb.GetPageContentFromSelect with composed SQL
                ///    - Handles "No results found" case with Clear Filters button
                ///    - Calls myWeb.CallPostFilterContentUpdates() for post-processing
                /// 
                /// PROVIDER PATTERN DETAILS:
                /// ------------------------
                /// - Providers loaded from web.config <protean/filterProviders> section
                /// - Two loading strategies:
                ///   1. Default: "Protean.Providers.Filters.{className}" (Type.GetType)
                ///   2. Custom: Assembly loaded from path or type string via WebConfigurationManager
                /// - Each provider must implement:
                ///   * AddControl(myWeb, filterElmt, filterForm, frmGroup, contentNode, whereSql)
                ///   * ApplyFilter(myWeb, whereSQL, filterForm, frmGroup, filterElmt, filterTarget)
                ///   * GetFilterOrderByClause(myWeb) - Optional, return empty string if not needed
                ///   * GetFilterGroupByClause(myWeb) - Optional, return empty string if not needed
                ///   * ContentIndexDefinationName(myWeb) - Optional, return empty string if not needed
                /// 
                /// EXAMPLE PROVIDERS:
                /// -----------------
                /// - PageFilter: Filters by page location (hierarchical checkbox tree)
                /// - BrandFilter: Filters by product brand (checkbox list with counts)
                /// - PriceFilter: Filters by price range (range slider or inputs)
                /// - SpecFilter: Filters by product specifications (dynamic checkbox groups)
                /// 
                /// SQL COMPOSITION STRATEGY:
                /// ------------------------
                /// - Each filter appends to whereSQL with "AND {condition}"
                /// - Final WHERE clause combines all filter conditions
                /// - ORDER BY clauses are comma-separated, duplicates removed
                /// - GROUP BY uses last provider's clause (only one supported)
                /// - Additional JOINs ensure required tables available for ORDER BY columns
                /// 
                /// SESSION STATE:
                /// -------------
                /// - Filter values persisted through form submission (XForm handles this)
                /// - Individual providers restore state in their AddControl methods
                /// - Combined SQL stored in session variables (FilterWhereCondition, OrderBy, etc.)
                /// 
                /// ERROR HANDLING:
                /// --------------
                /// - Reflection errors caught at provider loading
                /// - SQL errors caught during execution
                /// - Invalid form submissions handled by XForm validation
                /// </summary>
                public void ContentFilter(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    string cProcessInfo = "ContentFilter";

                    try
                    {
                        // ========================================
                        // PHASE 1: INITIALIZATION
                        // ========================================
                        // Initialize variables for SQL composition and form management
                        bool bDistinct = true;
                        XmlElement oFilterElmt;
                        string cAdditionalJoins = string.Empty;
                        string cAdditionalColumns = string.Empty;
                        string cOrderBySql = string.Empty;
                        string cGroupBySql = string.Empty;
                        string parentPageId = string.Empty;
                        string formName = "ContentFilter";
                        string cFilterTarget = "Product";
                        string cWhereSql = string.Empty;
                        XmlElement oFrmGroup;
                        var filterForm = new Cms.xForm(ref myWeb);
                        string className = string.Empty;
                        var oAdditionalFilterInput = new Hashtable();
                        filterForm.NewFrm(formName);
                        var oSortBy = filterForm.moPageXML.CreateAttribute("SortBy");
                        oContentNode.Attributes.Append(oSortBy);
                       

                        filterForm.submission(formName, "", "POST", "");
                        if (oContentNode.Attributes["filterTarget"] != null)
                        {
                            cFilterTarget = oContentNode.Attributes["filterTarget"].Value;
                        }
                        if (myWeb.moRequest.Form["clearfilters"] != null)
                        {
                            if (Convert.ToString(myWeb.moRequest.Form["clearfilters"]) == "clearfilters")
                            {
                                myWeb.moResponse.Redirect(myWeb.moRequest.RawUrl);

                            }
                        }
                        //string cShowMore = string.Empty;
                        //if (myWeb.moRequest.Form["cShowMore"] != null)
                        //{
                        //    cShowMore = Convert.ToString(myWeb.moRequest.Form["cShowMore"]);
                        //}
                        //bool bShowMoreFilterButton = false;

                        oFrmGroup = filterForm.addGroup(ref filterForm.moXformElmt, "", "filter-main");
                        filterForm.addInput(ref oFrmGroup, "SortBy", true, "", "hidden");
                        // XmlElement oXml = filterForm.moPageXML.CreateElement("ShowMore");
                        // oXml.InnerText = cShowMore;
                        // filterForm.Instance.AppendChild(oXml);

                        // ========================================
                        // PHASE 2: PROVIDER INTEGRATION POINT #1 - AddControl
                        // ========================================
                        // FIRST LOOP: Iterate through all filter providers to generate UI controls
                        // Each provider's AddControl method is responsible for:
                        // - Restoring filter state from previous submission (reading form values)
                        // - Creating filter UI elements (checkboxes, select dropdowns, etc.)
                        // - Populating options from database (with counts if applicable)
                        // - Adding removal buttons for selected values
                        // - Setting active/inactive CSS classes based on selection state
                        foreach (XmlElement currentOFilterElmt in oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']"))
                        {
                            oFilterElmt = currentOFilterElmt;

                            Type calledType;
                            className = oFilterElmt.GetAttribute("className");
                            string providerName = oFilterElmt.GetAttribute("providerName");

                            cWhereSql = GetFilterWhereClause(ref myWeb, ref filterForm, ref oContentNode, className);
                            if (!string.IsNullOrEmpty(className))
                            {
                                // Get cached provider instance (zero reflection on subsequent calls)
                                IContentFilter provider = FilterProviderFactory.GetProvider(className, providerName, myWeb);
                                
                                // Direct method call (10-20x faster than InvokeMember)
                                provider.AddControl(ref myWeb, ref oFilterElmt, ref filterForm, ref oFrmGroup, ref oContentNode, cWhereSql);


                            }

                        }

                        // Append completed filter form to content node for XSLT transformation
                        oContentNode.AppendChild(filterForm.moXformElmt);

                        // ========================================
                        // PHASE 3: FORM SUBMISSION CHECK
                        // ========================================
                        // Prepare variables for SQL composition (used only if form submitted and valid)
                        string whereSQL = string.Empty;
                        string orderBySql = string.Empty;
                        string groupBySql = string.Empty;
                        string cCssClassName = "hidden";
                        //  filterForm.addBind("cShowMore", "ShowMore", ref filterForm.model, "false()", "string");

                        // filterForm.addInput(ref oFrmGroup, "cShowMore", true, "ShowMore", "hidden");
                        //if (cShowMore == string.Empty)
                        //{
                        //    if (bShowMoreFilterButton == true)
                        //    {
                        //        cCssClassName = string.Empty;
                        //    }
                        //}
                        // filterForm.addInput(ref oFrmGroup, "", false, "More +", cCssClassName + " btnShowMoreFilter");
                        //  filterForm.addSubmit(ref oFrmGroup, "< Less", "< Less ", "Submit", "hidden filter-xs-btn btnHideFilter");
                        filterForm.addSubmit(ref oFrmGroup, "ContentFilter", "Show " + cFilterTarget, "ContentFilter", "hidden-sm hidden-md hidden-lg filter-xs-btn showfiltertarget");



                        filterForm.addValues();

                        // Check if filter form was submitted by user
                        if (filterForm.isSubmitted())
                        {
                            // Update form instance with submitted values from request
                            filterForm.updateInstanceFromRequest();

                            // Validate form inputs against defined rules
                            filterForm.validate();

                            // ========================================
                            // PHASE 4: PROVIDER INTEGRATION POINTS #2-5 - SQL COMPOSITION
                            // ========================================
                            // SECOND LOOP: Only executed if form is valid
                            // Iterate through providers again to build combined SQL query
                            // PERFORMANCE: Uses cached provider instances and direct interface calls (10-15x faster than reflection)
                            if (filterForm.valid)
                            {
                                foreach (XmlElement currentOFilterElmt1 in oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']"))
                                {
                                    oFilterElmt = currentOFilterElmt1;
                                    className = oFilterElmt.GetAttribute("className");
                                    string providerName = oFilterElmt.GetAttribute("providerName");

                                    if (!string.IsNullOrEmpty(className))
                                    {
                                        // ========================================
                                        // PROVIDER LOADING - Use cached instance from Phase 2
                                        // ========================================
                                        // Get cached provider instance (zero reflection on subsequent calls)
                                        IContentFilter provider = FilterProviderFactory.GetProvider(className, providerName, myWeb);

                                        // ========================================
                                        // INTEGRATION POINT #2: ApplyFilter
                                        // ========================================
                                        // Converts user's filter selections into SQL WHERE clause
                                        // Each provider appends its conditions to the cumulative whereSQL string
                                        // Example BrandFilter output: "AND c.nContentKey IN (SELECT nContentId FROM tblContentIndex WHERE ...)"
                                        // Example PageFilter output: "AND CL.nStructId IN (SELECT nStructKey FROM tblContentStructure WHERE ...)"

                                        // Direct interface method call (10-20x faster than InvokeMember)
                                        whereSQL = provider.ApplyFilter(ref myWeb, ref whereSQL, ref filterForm, ref oFrmGroup, ref oFilterElmt, ref cFilterTarget);

                                        // Extract parent page ID if specified (used for hierarchical filtering)
                                        if (oFilterElmt.Attributes["parId"] != null)
                                        {
                                            parentPageId = oFilterElmt.Attributes["parId"].Value;
                                        }

                                        // ========================================
                                        // INTEGRATION POINT #3: GetFilterGroupByClause
                                        // ========================================
                                        // Allows provider to specify GROUP BY clause for result aggregation
                                        // Only one GROUP BY clause is used (last provider wins)
                                        // Example: "c.nContentKey" (group products to prevent duplicates from joins)
                                        groupBySql = provider.GetFilterGroupByClause(ref myWeb);
                                        if (!string.IsNullOrEmpty(groupBySql))
                                        {
                                            cGroupBySql = groupBySql;
                                        }

                                        // ========================================
                                        // INTEGRATION POINT #4: GetFilterOrderByClause
                                        // ========================================
                                        // Allows provider to specify ORDER BY clause for result sorting
                                        // Multiple ORDER BY clauses are combined with commas
                                        // Duplicates are detected and removed to prevent SQL errors
                                        orderBySql = provider.GetFilterOrderByClause(ref myWeb);
                                        if (!string.IsNullOrEmpty(orderBySql))
                                        {
                                            // Handle multiple ORDER BY clauses from different providers
                                            // Strategy: Combine with commas, detect and remove duplicates
                                            if (!string.IsNullOrEmpty(cOrderBySql))
                                            {
                                                // Check for duplicate ORDER BY columns
                                                string orderby = orderBySql.Replace("desc", "").Replace("asc", "").Trim();
                                                if (cOrderBySql.ToLower().Contains(orderby.ToLower()))
                                                {
                                                    // Remove existing duplicate (DESC variant)
                                                    if (cOrderBySql.ToLower().Contains(orderby.ToLower() + "desc"))
                                                    {
                                                        cOrderBySql = cOrderBySql.Replace(orderby + "desc", "");
                                                    }
                                                    // Remove existing duplicate (ASC variant)
                                                    if (cOrderBySql.ToLower().Contains(orderby.ToLower() + "asc"))
                                                    {
                                                        cOrderBySql = cOrderBySql.Replace(orderby + "asc", "");
                                                    }

                                                    // Add new ORDER BY at front (higher priority)
                                                    cOrderBySql = orderBySql + "," + cOrderBySql;
                                                    cOrderBySql = cOrderBySql.Replace(",,", ",");
                                                }
                                                else
                                                {
                                                    // No duplicate - simply append
                                                    cOrderBySql = orderBySql + "," + cOrderBySql;
                                                }
                                            }
                                            else
                                            {
                                                // First ORDER BY clause
                                                cOrderBySql = orderBySql + ",";
                                            }
                                        }

                                        // ========================================
                                        // INTEGRATION POINT #5: GetContentIndexDefinationName
                                        // ========================================
                                        // For filters that sort by indexed values, additional JOINs are required
                                        // This integration point gets the ContentIndexDefinition name for JOIN clause
                                        if (!string.IsNullOrEmpty(orderBySql))
                                        {
                                            // Extract ORDER BY column name for additional SELECT columns
                                            // (columns in ORDER BY must be in SELECT list)
                                            cAdditionalColumns += "," + orderBySql.ToLower().Replace("asc", "").Replace(" desc", "").Trim();

                                            // Check if this filter should be excluded from JOIN generation
                                            // Some filters (like location-based) don't need ContentIndex JOINs
                                            // Configured via web.config: <add key="ExcludeFilterForJoin" value="PageFilter,LocationFilter" />
                                            if (myWeb.moConfig["ExcludeFilterForJoin"] == null || !myWeb.moConfig["ExcludeFilterForJoin"].Contains(className))
                                            {
                                                // Generate table alias based on filter class name
                                                // Example: "BrandFilter" → "Brand"
                                                string cAlias = className.Replace("Filter", "");

                                                // Get the ContentIndexDefinition name from provider (direct interface call)
                                                string cIndexDefinitionName = provider.ContentIndexDefinationName(ref myWeb);

                                                if (!string.IsNullOrEmpty(cIndexDefinitionName))
                                                {
                                                    // Build JOIN clauses to link content with indexed values
                                                    // Pattern: JOIN tblContentIndex ON content → JOIN tblContentIndexDef ON definition
                                                    // This ensures ORDER BY columns are available in result set
                                                    cAdditionalJoins += "inner join tblContentIndex cii" + cAlias + " on cii" + cAlias + ".nContentId=c.nContentKey inner join tblContentIndexDef cid" + cAlias;
                                                    cAdditionalJoins += " on cii" + cAlias + ".nContentIndexDefinitionKey=cid" + cAlias + ".nContentIndexDefKey ";
                                                    cAdditionalJoins += " and cid" + cAlias + ".cDefinitionName='" + cIndexDefinitionName + "'";
                                                }
                                            }
                                            else
                                            {
                                                // Filter excluded from JOIN generation (e.g., location-based filters)
                                                // Still append ORDER BY clause
                                                // Use DISTINCT to prevent duplicate rows from other JOINs
                                                cOrderBySql = orderBySql + ",";
                                                bDistinct = true;
                                            }
                                        }
                                    }
                                }

                                // ========================================
                                // FINAL SQL COMPOSITION
                                // ========================================
                                // Build final WHERE clause with parent page filtering if needed
                                // This adds the base content type and location filter before provider-specific conditions
                                if (!string.IsNullOrEmpty(parentPageId) & !string.IsNullOrEmpty(whereSQL) & whereSQL.ToLower().Contains("nstructid") == false)
                                {
                                    // Ensure AND prefix for proper SQL syntax
                                    if (whereSQL.ToLower().StartsWith(" and ") == false)
                                    {
                                        whereSQL = " AND " + whereSQL;
                                    }

                                    // Check if filtering should include child pages or just exact page match
                                    // ShowContentListlevel="true" → Only content on exact page
                                    // ShowContentListlevel="false" or missing → Include content from child pages
                                    if (oContentNode.Attributes["ShowContentListlevel"] != null)
                                    {
                                        if (oContentNode.Attributes["ShowContentListlevel"].Value.ToString().ToLower() == "true")
                                        {
                                            // Exact page match only
                                            whereSQL = " c.cContentSchemaName='" + cFilterTarget + "' And nStructId =" + parentPageId + whereSQL;
                                        }
                                        else
                                        {
                                            // Include child pages (hierarchical filter)
                                            whereSQL = " c.cContentSchemaName='" + cFilterTarget + "' And nStructId IN (select nStructKey from tblContentStructure where nStructParId in (" + parentPageId + "))" + whereSQL;
                                        }

                                    }
                                    else
                                    {
                                        // Default: Include child pages
                                        whereSQL = " c.cContentSchemaName='" + cFilterTarget + "' And nStructId IN (select nStructKey from tblContentStructure where nStructParId in (" + parentPageId + "))" + whereSQL;
                                    }
                                }
                            }
                        }

                        // ========================================
                        // PHASE 5: RESULT EXECUTION
                        // ========================================
                        // Execute the combined SQL query and retrieve filtered content
                        // This only runs if filters were applied (whereSQL not empty)
                        if (!string.IsNullOrEmpty(whereSQL))
                        {
                            // Safety check: Ensure WHERE clause doesn't end with dangling "AND"
                            // This can happen if a provider returns empty condition
                            if (whereSQL.ToLower().Trim().EndsWith(" and") == false)
                            {
                                // Store SQL components in session for potential reuse (e.g., pagination, AJAX updates)
                                myWeb.moSession["FilterWhereCondition"] = whereSQL;
                                myWeb.moSession["AdditionalColumns"] = cAdditionalColumns;
                                myWeb.moSession["AdditionalJoins"] = cAdditionalJoins;
                                myWeb.moSession["OrderBy"] = cOrderBySql;
                                XmlElement argoPageDetail = null; int nCount = 0;


                                // Admin mode handling: Show draft/expired content with status-based sorting
                                if (myWeb.mbAdminMode)
                                {
                                    myWeb.moSession["AdminMode"] = "true";
                                    if (cOrderBySql != string.Empty)
                                    {
                                        // Prepend status sort to show published content first, then drafts
                                        cOrderBySql = " a.nStatus desc," + cOrderBySql;
                                    }
                                    else
                                    {
                                        // Default sort by status only
                                        cOrderBySql = " a.nStatus desc";
                                    }


                                }

                                // ========================================
                                // EXECUTE FINAL QUERY
                                // ========================================
                                // Call GetPageContentFromSelect with composed SQL from all providers
                                // This method:
                                // - Builds complete SQL with JOINs, WHERE, ORDER BY, GROUP BY
                                // - Applies permissions filtering
                                // - Handles pagination if configured
                                // - Returns content nodes appended to oContentNode
                                // Parameters:
                                // - whereSQL: Combined WHERE conditions from all filters
                                // - cShowSpecificContentTypes: Target content type (e.g., "Product")
                                // - bIgnorePermissionsCheck: true (filters handle their own security)
                                // - distinct: Use DISTINCT to prevent duplicates from JOINs
                                // - cOrderBy: Combined ORDER BY from all providers
                                // - cAdditionalJoins: JOIN clauses for ContentIndex tables
                                // - cAdditionalColumns: Additional SELECT columns needed for ORDER BY
                                // - cGroupBySql: GROUP BY clause (if any provider specified one)
                                myWeb.GetPageContentFromSelect(whereSQL, ref nCount, oContentsNode: ref oContentNode, oPageDetail: ref argoPageDetail,
                                cShowSpecificContentTypes: cFilterTarget, bIgnorePermissionsCheck: true, distinct: bDistinct, cOrderBy: cOrderBySql, cAdditionalJoins: cAdditionalJoins, cAdditionalColumns: cAdditionalColumns,cGroupBySql: cGroupBySql);

                                // Handle no results case
                                // Add "Clear Filters" button to allow user to reset and try again
                                if (oContentNode.SelectNodes("Content[@type='Product']").Count == 0)
                                {
                                    filterForm.addSubmit(ref oFrmGroup, "Clear Filters", "No results found", "clearfilters", "clear-filters", sValue: "clearfilters");
                                }
                            }
                            else
                            {
                                // WHERE clause ended with "AND" - invalid SQL
                                // Show error state with Clear Filters option
                                filterForm.addSubmit(ref oFrmGroup, "Clear Filters", "No results found", "clearfilters", "clear-filters", sValue: "clearfilters");
                            }
                        }

                        // ========================================
                        // POST-FILTER PROCESSING HOOK
                        // ========================================
                        // Allow providers or extensions to modify results after SQL execution
                        // This is useful for:
                        // - Adding computed properties to content nodes
                        // - Applying additional filtering that can't be done in SQL
                        // - Enriching content with external data
                        // - Modifying sort order based on business logic
                        // Modify results after they are loaded onto the page.
                        myWeb.CallPostFilterContentUpdates();
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ContentFilter", ex, "", cProcessInfo, gbDebug);
                    }
                }

                /// <summary>
                /// OBSOLETE: Use provider.GetFilterOrderByClause(ref myWeb) directly instead
                /// This method is kept for backward compatibility only
                /// </summary>
                [Obsolete("Use provider.GetFilterOrderByClause(ref myWeb) directly instead. This method will be removed in a future version.")]
                public string GetFilterOrderByClause(Type calledType, string existingOrderBy, ref Cms myWeb)
                {
                    string filterOrderByClause = string.Empty;

                    if (calledType != null)
                    {
                        string methodname = "GetFilterOrderByClause";

                        var o = Activator.CreateInstance(calledType);
                        var args = new object[1];
                        args[0] = myWeb;
                        filterOrderByClause = Convert.ToString(calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, null, o, args));
                    }

                    if (!string.IsNullOrEmpty(existingOrderBy))
                    {
                        if (existingOrderBy.IndexOf(filterOrderByClause) >= 0)
                        {
                            filterOrderByClause = null;
                        }
                        else
                        {
                            filterOrderByClause = existingOrderBy + ", " + filterOrderByClause;
                        }
                    }

                    return filterOrderByClause;
                }

                /// <summary>
                /// OBSOLETE: Use provider.GetFilterGroupByClause(ref myWeb) directly instead
                /// This method is kept for backward compatibility only
                /// </summary>
                [Obsolete("Use provider.GetFilterGroupByClause(ref myWeb) directly instead. This method will be removed in a future version.")]
                public string GetFilterGroupByClause(Type calledType, string existingOrder, ref Cms myWeb)
                {
                    string filterGroupByClause = string.Empty;

                    if (calledType != null)
                    {
                        string methodname = "GetFilterGroupByClause";

                        var o = Activator.CreateInstance(calledType);
                        var args = new object[1];
                        args[0] = myWeb;
                        filterGroupByClause = Convert.ToString(calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, null, o, args));
                    }
                    return filterGroupByClause;
                }

                /// <summary>
                /// OBSOLETE: Use provider.ContentIndexDefinationName(ref myWeb) directly instead
                /// This method is kept for backward compatibility only
                /// </summary>
                [Obsolete("Use provider.ContentIndexDefinationName(ref myWeb) directly instead. This method will be removed in a future version.")]
                public string GetContentIndexDefinationName(Type calledType, ref Cms myWeb)
                {
                    string filterGroupByClause = string.Empty;

                    if (calledType != null)
                    {
                        string methodname = "ContentIndexDefinationName";

                        var o = Activator.CreateInstance(calledType);
                        var args = new object[1];
                        args[0] = myWeb;
                        filterGroupByClause = Convert.ToString(calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, null, o, args));
                    }
                    return filterGroupByClause;
                }

                public string GetFilterWhereClause(ref Cms myWeb, ref Cms.xForm filterForm, ref XmlElement oContentNode, string excludeClassName)
                {
                    string cWhereSQL = string.Empty;
                    string className = string.Empty;
                    string cFilterTarget = string.Empty;

                    if (oContentNode.Attributes["filterTarget"] != null)
                    {
                        cFilterTarget = oContentNode.Attributes["filterTarget"].Value;
                    }

                    try
                    {
                        foreach (XmlElement oFilterElmt in oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']"))
                        {
                            string providerName = oFilterElmt.GetAttribute("providerName");
                            className = oFilterElmt.Attributes["className"].Value.ToString();

                            if (myWeb.moRequest.Form[className] != null)
                            {
                                if ((excludeClassName ?? "") != (className ?? ""))
                                {
                                    // Get cached provider instance (zero reflection)
                                    IContentFilter provider = FilterProviderFactory.GetProvider(className, providerName, myWeb);

                                    // Direct interface method call (10-20x faster than reflection)
                                    string cAdditionalCondition = provider.GetFilterSQL(ref myWeb);

                                    if (!string.IsNullOrEmpty(cAdditionalCondition))
                                    {
                                        if (!string.IsNullOrEmpty(cWhereSQL))
                                        {
                                            cWhereSQL = cWhereSQL + " AND " + cAdditionalCondition;
                                        }
                                        else
                                        {
                                            cWhereSQL = cAdditionalCondition;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetFilterWhereClause", ex, ""));
                    }
                    return cWhereSQL;
                }
                public void Conditional(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {
                        if (!myWeb.mbAdminMode)
                        {
                            if (!myWeb.moRequest.QueryString.ToString().Contains(oContentNode.GetAttribute("querystringcontains")))
                            {
                                oContentNode.ParentNode.RemoveChild(oContentNode);
                            }
                        }
                    }


                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Logon", ex, ""));
                    }
                }


            }


            #endregion

            ~Content()
            {
            }
        }
    }
}