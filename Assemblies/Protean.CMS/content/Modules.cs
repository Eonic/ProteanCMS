using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Web.Configuration;

using System.Xml;
using System.Xml.Xsl;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;


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

                        var PageDate = myWeb.mdDate;

                        // Overide for testing
                        // PageDate = New Date(2016, 3, 15)

                        // handle querystrings
                        if (myWeb.mcOriginalURL.Contains("?"))
                        {
                            cOrigUrl = myWeb.mcOriginalURL.Split('?')[0];
                            cOrigQS = "?" + myWeb.mcOriginalURL.Split('?')[1];
                        }

                        if (Strings.InStr(cOrigUrl, "-/") > 0)
                        {
                            dateQuery = cOrigUrl.Substring(Strings.InStr(cOrigUrl, "-/") + 1);
                        }

                        string thisDateQuery;
                        // Week start date...
                        // Get ranges for articles on this page

                        string sFilterSql = myWeb.GetStandardFilterSQLForContent();
                        string sSql = "select  a.dpublishDate as publish from tblContent c" + " inner join tblContentLocation CL on c.nContentKey = CL.nContentId" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where( CL.nStructId = " + myWeb.mnPageId;
                        sSql = sSql + sFilterSql + " and c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "') order by a.dpublishDate desc";

                        var DateSet = myWeb.moDbHelper.GetDataSet(sSql, "ArticleDates");
                        var dEarliestDate = PageDate;
                        // Latest Articles
                        int nFirstPageCount = Conversions.ToInteger("0" + oContentNode.GetAttribute("firstPageCount"));
                        DateTime FirstPageLastDate = default;
                        long counter = nFirstPageCount;

                        foreach (DataRow dr in DateSet.Tables[0].Rows)
                        {
                            if (Information.IsDate(dr["publish"]))
                            {
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectLess(dr["publish"], dEarliestDate, false)))
                                {
                                    dEarliestDate = Conversions.ToDate(dr["publish"]);
                                }
                                counter = counter - 1L;
                                if (counter == 0L)
                                {
                                    FirstPageLastDate = Conversions.ToDate(dr["publish"]);
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
                            NewMenu.AddMenuItem("Latest Articles", thisDateQuery, cPageURL + "/" + oContentNode.GetAttribute("id") + "-/" + thisDateQuery + cOrigQS, contentCount: contentCount);
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
                            NewMenu.AddMenuItem("This Week", thisDateQuery, cPageURL + "/" + oContentNode.GetAttribute("id") + "-/" + thisDateQuery + cOrigQS, contentCount: contentCount);
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
                            NewMenu.AddMenuItem("Last Week", thisDateQuery, cPageURL + "/" + oContentNode.GetAttribute("id") + "-/" + thisDateQuery + cOrigQS, contentCount: contentCount);
                            if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                            {
                                startDate = mondayDate.AddDays((double)-8);
                                endDate = mondayDate.AddDays((double)-1);
                                dateQuery = thisDateQuery;
                            }
                        }

                        // This Month
                        object firstDayMonth = new DateTime(PageDate.Year, PageDate.Month, 1);
                        contentCount = this.getArticleCount(DateSet, Conversions.ToDate(firstDayMonth), PageDate);
                        if (contentCount > 0)
                        {
                            thisDateQuery = "thismonth";
                            NewMenu.AddMenuItem("This Month", thisDateQuery, cPageURL + "/" + oContentNode.GetAttribute("id") + "-/" + thisDateQuery + cOrigQS, contentCount: contentCount);
                            if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                            {
                                startDate = Conversions.ToDate(firstDayMonth);
                                endDate = PageDate;
                                dateQuery = thisDateQuery;
                            }
                        }

                        // Step through this years months
                        int nPrevMonths = Conversions.ToInteger("0" + oContentNode.GetAttribute("previousMonthsListed"));
                        if (nPrevMonths == 0)
                            nPrevMonths = 12;
                        int thisCount = 1;
                        short nThisMonth = (short)(PageDate.Month - 1);
                        short nThisYear = (short)PageDate.Year;
                        while (nThisMonth != 0 & thisCount <= nPrevMonths)
                        {
                            object firstDayloopMonth = new DateTime(PageDate.Year, (int)nThisMonth, 1);
                            contentCount = this.getArticleCount(DateSet, Conversions.ToDate(firstDayloopMonth), dhLastDayInMonth(Conversions.ToDate(firstDayloopMonth)));
                            if (contentCount > 0)
                            {
                                thisDateQuery = nThisYear + "-" + nThisMonth;
                                NewMenu.AddMenuItem(DateAndTime.MonthName(nThisMonth) + " " + nThisYear, thisDateQuery, cPageURL + "/" + oContentNode.GetAttribute("id") + "-/" + thisDateQuery + cOrigQS, contentCount: contentCount);
                                if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                                {
                                    startDate = Conversions.ToDate(firstDayloopMonth);
                                    endDate = dhLastDayInMonth(Conversions.ToDate(firstDayloopMonth));
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
                            contentCount = this.getArticleCount(DateSet, Conversions.ToDate(firstDayYear), lastMonthDate);
                            if (contentCount > 0)
                            {
                                thisDateQuery = "restofyear";
                                NewMenu.AddMenuItem("Rest of " + PageDate.Year, thisDateQuery, cPageURL + "/" + oContentNode.GetAttribute("id") + "-/" + thisDateQuery + cOrigQS, contentCount: contentCount);
                                if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                                {
                                    startDate = Conversions.ToDate(firstDayYear);
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
                            contentCount = this.getArticleCount(DateSet, Conversions.ToDate(firstDayloopYear), Conversions.ToDate(lastDayloopYear));
                            if (contentCount > 0)
                            {
                                thisDateQuery = nThisYear.ToString();
                                NewMenu.AddMenuItem(nThisYear.ToString(), thisDateQuery, cPageURL + "/" + oContent.XmlElement.GetAttribute("id") + "-/" + thisDateQuery + cOrigQS, contentCount: contentCount);
                                if ((dateQuery ?? "") == (thisDateQuery ?? "") | string.IsNullOrEmpty(dateQuery))
                                {
                                    startDate = Conversions.ToDate(firstDayloopYear);
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
                                int argnCount = 0;
                                XmlElement argoContentsNode = null;
                                XmlElement argoPageDetail = null;
                                myWeb.GetPageContentFromSelect("CL.nStructId = " + myWeb.mnPageId + " And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "'", nCount: ref argnCount, oContentsNode: ref argoContentsNode, oPageDetail: ref argoPageDetail);
                            }
                            else
                            {
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
                                int argnCount2 = 0;
                                XmlElement argoContentsNode2 = null;
                                XmlElement argoPageDetail2 = null;
                                myWeb.GetPageContentFromSelect("CL.nStructId = " + myWeb.mnPageId + " And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "'" + endstr, nCount: ref argnCount2, oContentsNode: ref argoContentsNode2, oPageDetail: ref argoPageDetail2);
                            }
                            else
                            {
                                int argnCount3 = 0;
                                XmlElement argoContentsNode3 = null;
                                XmlElement argoPageDetail3 = null;
                                myWeb.GetPageContentFromSelect("CL.nStructId = " + myWeb.mnPageId + " And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "' and a.dpublishDate >= " + sqlDate(startDate) + endstr, nCount: ref argnCount3, oContentsNode: ref argoContentsNode3, oPageDetail: ref argoPageDetail3);
                            }
                        }

                        // remove content detail
                        if (myWeb.moContentDetail != null)
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
                        if (Information.IsDate(dr["publish"]))
                        {
                            if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectGreaterEqual(dr["publish"], startDate, false), Operators.ConditionalCompareObjectLessEqual(dr["publish"], endDate, false))))
                            {
                                ReturnCount = ReturnCount + 1;
                            }
                        }
                    }
                    return ReturnCount;
                }

                public DateTime dhLastDayInMonth(DateTime dtmDate)
                {
                    return DateAndTime.DateSerial(System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetYear(dtmDate), System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.GetMonth(dtmDate) + 1, 0);
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
                        long nItemsPerPage = Conversions.ToLong(oContentNode.GetAttribute("stepCount"));
                        long nCurrentPage = 1L;
                        long itemCount = Conversions.ToLong(myWeb.moDbHelper.GetDataValue(@"select count(nContentKey) from tblContent c inner join tblContentLocation cl on c.nContentKey =  cl.nContentId
where cl.nStructId = " + myWeb.mnPageId));
                        oContentNode.SetAttribute("itemCount", itemCount.ToString());
                        if (!string.IsNullOrEmpty(myWeb.moRequest["startPos" + oContentNode.GetAttribute("id")]))
                        {
                            nCurrentPage = (long)Math.Round((double)Conversions.ToLong(myWeb.moRequest["startPos" + oContentNode.GetAttribute("id")]) / (double)nItemsPerPage + 1d);
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
                        myWeb.GetPageContentFromSelect("CL.nStructId = " + myWeb.mnPageId + " And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "' ",ref nCount, ref xmlContentNode,oPageDetail: ref argoPageDetail, nReturnRows: (int)nItemsPerPage, pageNumber: nCurrentPage);
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
                                int contentId = Conversions.ToInteger(oContentNode.GetAttribute("id"));
                                myWeb.mbAdminMode = true;
                                myWeb.moDbHelper.addRelatedContent(ref oContentNode, contentId, true);
                                myWeb.mbAdminMode = false;
                            }
                            // myWeb.moDbHelper.getRelationsByContentId(contentId, oContentNode, 2)
                            else
                            {
                                XmlElement argoPageDetail = null;
                                XmlElement xmlContentNode = null;int nCount = 0;
                                myWeb.GetPageContentFromSelect("CL.nStructId = " + PageId + " And a.dExpireDate < GETDATE() And a.nStatus = 1 And c.cContentSchemaName = '" + oContentNode.GetAttribute("contentType") + "' ",ref nCount, ref xmlContentNode, oPageDetail: ref argoPageDetail, nReturnRows: (int)nItemsPerPage, pageNumber: nCurrentPage, ignoreActiveAndDate: true);
                            }



                            myWeb.bAllowExpired = true;
                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ListHistoricEvents", ex, "", cProcessInfo, gbDebug);
                    }
                }


                public void ContentFilter(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    string cProcessInfo = "ContentFilter";
                    try
                    {
                        // current contentfilter id
                        XmlElement oFilterElmt;
                        string parentPageId = string.Empty;
                        string formName = "ContentFilter";
                        string cFilterTarget = "Product";
                        string cWhereSql = string.Empty;
                        XmlElement oFrmGroup;
                        var filterForm = new Cms.xForm(ref myWeb);
                        string className = string.Empty;
                        var oAdditionalFilterInput = new Hashtable();
                        filterForm.NewFrm(formName);
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


                        oFrmGroup = filterForm.addGroup(ref filterForm.moXformElmt, "main-group");



                        foreach (XmlElement currentOFilterElmt in oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']"))
                        {
                            oFilterElmt = currentOFilterElmt;

                            Type calledType;
                            className = oFilterElmt.GetAttribute("className");
                            string providerName = oFilterElmt.GetAttribute("providerName");

                            cWhereSql = GetFilterWhereClause(ref myWeb, ref filterForm, ref oContentNode, className);
                            if (!string.IsNullOrEmpty(className))
                            {

                                if (string.IsNullOrEmpty(providerName) | Strings.LCase(providerName) == "default")
                                {
                                    providerName = "Protean.Providers.Filters." + className;
                                    calledType = Type.GetType(providerName, true);
                                }
                                else
                                {
                                    var castObject = WebConfigurationManager.GetWebApplicationSection("protean/filterProviders");
                                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                                    System.Configuration.ProviderSettings ourProvider = moPrvConfig.Providers[providerName];
                                    Assembly assemblyInstance;

                                    if (ourProvider.Parameters["path"] != "" && ourProvider.Parameters["path"] != null)
                                    {
                                        assemblyInstance = Assembly.LoadFrom(myWeb.goServer.MapPath(Conversions.ToString(ourProvider.Parameters["path"])));
                                    }
                                    else
                                    {
                                        assemblyInstance = Assembly.Load(ourProvider.Type);
                                    }
                                    if (ourProvider.Parameters["rootClass"] == "")
                                    {
                                        calledType = assemblyInstance.GetType("Protean.Providers.Filters." + providerName, true);
                                    }
                                    else
                                    {

                                        calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(ourProvider.Parameters["rootClass"], "."), className)), true);
                                    }
                                }

                                string methodname = "AddControl";

                                var o = Activator.CreateInstance(calledType);

                                var args = new object[6];
                                args[0] = myWeb;
                                args[1] = oFilterElmt;
                                args[2] = filterForm;
                                args[3] = oFrmGroup;
                                args[4] = oContentNode;
                                args[5] = cWhereSql;
                                calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, null, o, args);
                            }

                        }
                        oContentNode.AppendChild(filterForm.moXformElmt);

                        string whereSQL = "";

                        filterForm.addSubmit(ref oFrmGroup, "Show Experiences", "Show Experiences", "Show Experiences", "hidden-sm hidden-md hidden-lg filter-xs-btn showexperiences");
                        // filterForm.addSubmit(oFrmGroup, "Clear Filters", "Clear Filters", "submit", "ClearFilters")
                        filterForm.addValues();

                        if (filterForm.isSubmitted())
                        {



                            filterForm.updateInstanceFromRequest();
                            filterForm.validate();

                            if (filterForm.valid)
                            {


                                foreach (XmlElement currentOFilterElmt1 in oContentNode.SelectNodes("Content[@type='Filter' and @providerName!='']"))
                                {
                                    oFilterElmt = currentOFilterElmt1;

                                    Type calledType;
                                    className = oFilterElmt.GetAttribute("className");
                                    string providerName = oFilterElmt.GetAttribute("providerName");

                                    if (!string.IsNullOrEmpty(className))
                                    {

                                        if (string.IsNullOrEmpty(providerName) | Strings.LCase(providerName) == "default")
                                        {
                                            providerName = "Protean.Providers.Filters." + className;
                                            calledType = Type.GetType(providerName, true);
                                        }
                                        else
                                        {
                                            var castObject = WebConfigurationManager.GetWebApplicationSection("protean/filterProviders");
                                            Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                                            System.Configuration.ProviderSettings ourProvider = moPrvConfig.Providers[providerName];
                                            Assembly assemblyInstance;

                                            if (ourProvider.Parameters["path"] != "" && ourProvider.Parameters["path"] != null)
                                            {
                                                assemblyInstance = Assembly.LoadFrom(myWeb.goServer.MapPath(Conversions.ToString(ourProvider.Parameters["path"])));
                                            }
                                            else
                                            {
                                                assemblyInstance = Assembly.Load(ourProvider.Type);
                                            }
                                            if (ourProvider.Parameters["rootClass"] == "")
                                            {
                                                calledType = assemblyInstance.GetType("Protean.Providers.Filters." + providerName, true);
                                            }
                                            else
                                            {

                                                calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(ourProvider.Parameters["rootClass"], "."), className)), true);
                                            }
                                        }

                                        string methodname = "ApplyFilter";

                                        var o = Activator.CreateInstance(calledType);

                                        var args = new object[6];
                                        args[0] = myWeb;
                                        args[1] = whereSQL;
                                        args[2] = filterForm;
                                        args[3] = oFrmGroup;
                                        args[4] = oFilterElmt;
                                        args[5] = cFilterTarget;
                                        whereSQL = Convert.ToString(calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, null, o, args));
                                        if (oFilterElmt.Attributes["parId"] != null)
                                        {
                                            parentPageId = oFilterElmt.Attributes["parId"].Value;

                                        }

                                    }

                                }



                                if (!string.IsNullOrEmpty(parentPageId) & !string.IsNullOrEmpty(whereSQL) & whereSQL.ToLower().Contains("nstructid") == false)
                                {
                                    if (whereSQL.ToLower().StartsWith(" and ") == false)
                                    {
                                        whereSQL = " AND " + whereSQL;
                                    }

                                    whereSQL = " c.cContentSchemaName='" + cFilterTarget + "' And nStructId IN (select nStructKey from tblContentStructure where nStructParId in (" + parentPageId + "))" + whereSQL;
                                    // Else
                                    // whereSQL = " c.cContentSchemaName='" & cFilterTarget & whereSQL
                                }
                            }
                        }




                        // now we go and get the results from the filter.
                        if (!string.IsNullOrEmpty(whereSQL))
                        {

                            if (whereSQL.ToLower().Trim().EndsWith(" and") == false)
                            {
                                myWeb.moSession["FilterWhereCondition"] = whereSQL;
                            XmlElement argoPageDetail = null;int nCount = 0;
                            myWeb.GetPageContentFromSelect(whereSQL,ref nCount, oContentsNode: ref oContentNode, oPageDetail: ref argoPageDetail, cShowSpecificContentTypes: cFilterTarget, bIgnorePermissionsCheck:true);


                                if (oContentNode.SelectNodes("Content[@type='Product']").Count == 0)
                                {
                                    filterForm.addSubmit(ref oFrmGroup, "Clear Filters", "No results found", "clearfilters", "clear-filters", sValue: "clearfilters");
                                }
                            }
                            else
                            {
                                filterForm.addSubmit(ref oFrmGroup, "Clear Filters", "No results found", "clearfilters", "clear-filters", sValue: "clearfilters");
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ContentFilter", ex, "", cProcessInfo, gbDebug);
                    }
                }


                public string GetFilterWhereClause(ref Cms myWeb, ref Cms.xForm filterForm, ref XmlElement oContentNode, string excludeClassName)
                {
                    string cWhereSQL = string.Empty;
                    string className = string.Empty;
                    string cFilterTarget;

                    Type calledType;
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

                                    if (string.IsNullOrEmpty(providerName) | Strings.LCase(providerName) == "default")
                                    {
                                        providerName = "Protean.Providers.Filters." + className;
                                        calledType = Type.GetType(providerName, true);
                                    }
                                    else
                                    {
                                        var castObject = WebConfigurationManager.GetWebApplicationSection("protean/filterProviders");
                                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)castObject;
                                        System.Configuration.ProviderSettings ourProvider = moPrvConfig.Providers[providerName];
                                        Assembly assemblyInstance;

                                        if (ourProvider.Parameters["path"] != "" && ourProvider.Parameters["path"] != null)
                                        {
                                            assemblyInstance = Assembly.LoadFrom(myWeb.goServer.MapPath(Conversions.ToString(ourProvider.Parameters["path"])));
                                        }
                                        else
                                        {
                                            assemblyInstance = Assembly.Load(ourProvider.Type);
                                        }
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(ourProvider.Parameters["rootClass"], "", false)))
                                        {
                                            calledType = assemblyInstance.GetType("Protean.Providers.Filters." + providerName, true);
                                        }
                                        else
                                        {

                                            calledType = assemblyInstance.GetType(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(ourProvider.Parameters["rootClass"], "."), className)), true);
                                        }
                                    }

                                    string methodname = "GetFilterSQL";

                                    var o = Activator.CreateInstance(calledType);

                                    var args = new object[1];
                                    args[0] = myWeb;
                                    string cAdditionalCondition = string.Empty;
                                    cAdditionalCondition = Conversions.ToString(calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, null, o, args));
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

                                        // cWhereSQL = cWhereSQL & calledType.InvokeMember(methodname, BindingFlags.InvokeMethod, Nothing, o, args)
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