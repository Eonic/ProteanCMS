
using Protean.Providers.Membership;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Protean.stdTools;
using static Protean.Tools.Xml;
using static System.Web.HttpUtility;

namespace Protean
{

    public partial class Cms : Protean.Base, IDisposable
    {


        public virtual void AddContentXml(ref XmlElement oContentElmt)
        {
            PerfMon.Log("Web", "AddContentXml");
            XmlElement oContents;
            string sProcessInfo = "Add Content XML";

            try
            {
                if (oContentElmt is null)
                    return;
                oContents = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oContents is null)
                {
                    oContents = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oContents);
                }

                oContents.AppendChild(oContentElmt);
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "AddContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddContentXml", ex, sProcessInfo));
            }

        }

        public void GetContentXml(ref XmlElement oPageElmt)
        {
            PerfMon.Log("Web", "GetContentXml");

            string sNodeName = string.Empty;
            string cXPathModifier = "";
            string sContent = string.Empty;
            string IsInTree = Convert.ToString(false);
            string sProcessInfo = "building the Content XML";

            try
            {


                if (!mbSystemPage)
                {
                    // Clone page adjustment
                    if (gbClone)
                    {

                        if (mbIsClonePage)
                        {
                            // Is this a clone page that directly references its cloned page.
                            cXPathModifier = " and @clone='" + mnClonePageId.ToString() + "'";
                        }
                        else if (mnCloneContextPageId > 0)
                        {
                            // Is this a child of a clone page
                            cXPathModifier = " and @cloneparent='" + mnCloneContextPageId.ToString() + "'";
                        }

                        else if (mnClonePageVersionId > 0)
                        {

                            cXPathModifier = " and @requestedPage='1'";
                        }
                        // Page Version of a cloned page.

                        else
                        {
                            // this is not a cloned page, make sure we don't accidentally look for the cloned pages.

                            // this version works on clone language variations but wrong menu
                            cXPathModifier = " and (((not(@cloneparent) or @cloneparent=0) and (not(@clone) or @clone='' or @clone=0)) or (starts-with(@url,'" + mcPageLanguageUrlPrefix + mcPagePath + "') or starts-with(@url,'" + mcRequestDomain + mcPageLanguageUrlPrefix.Replace(mcRequestDomain, "") + mcPagePath + "')))";

                            // cXPathModifier = " and (((not(@cloneparent) or @cloneparent=0) and (not(@clone) or @clone='' or @clone=0)) and contains(@url,'" & mcPageLanguageUrlPrefix & mcPagePath & "'))"
                        }



                    }

                    // Check for blocked content


                    gcBlockContentType = moDbHelper.GetPageBlockedContent((long)mnPageId);
                    string parentXpath = "/Page/Menu/descendant-or-self::MenuItem[descendant-or-self::MenuItem[@id='" + mnPageId + "'" + cXPathModifier + "]]";

                    oPageElmt.SetAttribute("blockedContent", Regex.Replace(gcBlockContentType, @"^[|\d]+", ""));

                    // this is for load more steppers - we do not want any other content other than the one on the list
                    // the page url looks like
                    // /ourpage/?singleContentType=Product&startPos=10&rows=10

                    if (!string.IsNullOrEmpty(moRequest["singleContentType"]))
                    {
                        // sql for content on page and permissions etc
                        string sFilterSql = GetStandardFilterSQLForContent();
                        sFilterSql = sFilterSql + " and nstructid=" + mnPageId;
                        string cSort = "|ASC_cl.nDisplayOrder";
                        switch (moRequest["sortby"] ?? "")
                        {
                            case "name":
                                {
                                    cSort = "|ASC_c.cContentName";
                                    break;
                                }

                            default:
                                {
                                    cSort = "|ASC_cl.nDisplayOrder";
                                    break;
                                }
                        }
                        // Paging variables
                        int nStart = 0;
                        int nRows = 500;

                        // Set the paging variables, if provided.
                        if (moRequest["startPos"] != null && Tools.Number.IsNumeric(moRequest["startPos"]))
                            nStart = Convert.ToInt16(moRequest["startPos"]);
                        if (moRequest["rows"] != null && Tools.Number.IsNumeric(moRequest["rows"]))
                            nRows = Convert.ToInt16(moRequest["rows"]);
                        // In admin mode want active and hidden products separatly
                        if (mbAdminMode)
                        {
                            if (moRequest["status"] != null && Tools.Number.IsNumeric(moRequest["status"]))
                            {
                                int nstatus = Convert.ToInt16(moRequest["status"]);
                                if (nstatus == 0)
                                {
                                    sFilterSql = sFilterSql + " and nstructid=" + mnPageId + " and a.nStatus!=1";
                                    nStart = 0;
                                    nRows = Convert.ToInt16(moRequest["TotalCount"]);  // getting all hidden products in list
                                }
                                else
                                {
                                    sFilterSql = sFilterSql + " and nstructid=" + mnPageId + " and a.nStatus=" + nstatus;
                                }
                            }
                        }
                        else
                        {
                            sFilterSql = sFilterSql + " and nstructid=" + mnPageId;
                        }
                        if (moSession["FilterWhereCondition"] != null && !string.IsNullOrEmpty(moSession["FilterWhereCondition"].ToString()))
                        {
                            string whereSQL = moSession["FilterWhereCondition"].ToString();
                            string cAdditionalColumns = Convert.ToString(moSession["AdditionalColumns"]);
                            string cAdditionalJoins = Convert.ToString(moSession["AdditionalJoins"]);
                            string cOrderBySql = Convert.ToString(moSession["OrderBy"]);
                            string cAdminMode = Convert.ToString(moSession["AdminMode"]);
                            XmlElement argoPageDetail = null;
                            int nCount = 0;
                            string cGroupBySql = string.Empty;
                            GetPageContentFromSelectFilterPagination(ref nCount, oContentsNode: ref oPageElmt, oPageDetail: ref argoPageDetail, whereSQL, bIgnorePermissionsCheck: true, cShowSpecificContentTypes: moRequest["singleContentType"], ignoreActiveAndDate: false, nStartPos: (long)nStart, nItemCount: (long)nRows, distinct: true, cAdditionalJoins: cAdditionalJoins, cAdditionalColumns: cAdditionalColumns, cOrderBy: cOrderBySql, cAdminMode: cAdminMode, cGroupBySql: cGroupBySql);
                        }
                        else
                        {
                            var argoPageElmt = moPageXml.DocumentElement;
                            XmlElement argoPageDetail1 = null;
                            XmlElement argoContentModule = null;
                            GetContentXMLByTypeAndOffset(ref argoPageElmt, moRequest["singleContentType"] + cSort, (long)nStart, (long)nRows, oPageDetail: ref argoPageDetail1, oContentModule: ref argoContentModule, sFilterSql);

                        }
                    }

                    else
                    {

                        // Set nothing to Filter Pagination session
                        if (moSession != null)
                        {
                            if (moSession["FilterWhereCondition"] != null)
                            {
                                moSession["FilterWhereCondition"] = (object)null;
                                moSession["AdditionalColumns"] = null;
                                moSession["AdditionalJoins"] = null;
                                moSession["OrderBy"] = null;
                                // moSession.Remove("FilterWhereCondition")
                            }
                        }


                        // step through the tree from home to our current page
                        foreach (XmlElement oElmt in oPageElmt.SelectNodes(parentXpath))
                        {
                            oElmt.SetAttribute("active", "1");
                            long nPageId = Convert.ToInt64(oElmt.GetAttribute("id"));
                            GetPageContentXml(nPageId);
                            nPageId = default;
                            IsInTree = Convert.ToString(true);
                        }

                        if (mbPreview & Convert.ToBoolean(IsInTree) == false)
                        {
                            GetPageContentXml((long)mnPageId);
                        }

                        if (Features.ContainsKey("PageVersions"))
                        {
                            if (Convert.ToBoolean(IsInTree) == false & mbAdminMode == true)
                            {
                                GetPageContentXml((long)mnPageId);
                            }
                        }
                        if ((long)mnPageId == gnPageNotFoundId)
                        {
                            GetPageContentXml((long)mnPageId);
                        }

                        // get the first records in the case of a load more stepper.
                        if (!string.IsNullOrEmpty(gcBlockContentType))
                        {

                            string[] cContentTypes = gcBlockContentType.Split(',');
                            int i;
                            var loopTo = cContentTypes.Length - 1;
                            for (i = 0; i <= loopTo; i++)
                            {
                                string[] cContentType = cContentTypes[i].Split('|');

                                string SingleContentType = cContentType[0];
                                string ModuleId = cContentType[1];
                                XmlElement ContentModule = (XmlElement)moPageXml.SelectSingleNode("/Page/Contents/Content[@id='" + ModuleId + "']");
                                if (ContentModule != null)
                                {
                                    string cSort = "|ASC_cl.nDisplayOrder";
                                    switch (ContentModule.GetAttribute("sortBy") ?? "")
                                    {
                                        case "name":
                                            {
                                                cSort = "|ASC_c.cContentName";
                                                break;
                                            }

                                        default:
                                            {
                                                cSort = "|ASC_cl.nDisplayOrder";
                                                break;
                                            }
                                    }
                                    // Paging variables
                                    int nStart = 0;
                                    int nRows = 500;
                                    nRows = Convert.ToInt16("0" + ContentModule.GetAttribute("stepCount"));
                                    if (Convert.ToInt16("0" + ContentModule.GetAttribute("firstPageCount")) > 0)
                                    {
                                        nRows = 0;// Convert.ToInt16("0" + ContentModule.GetAttribute("firstPageCount"));
                                    }
                                    if (nRows > 0)
                                    {
                                        string sFilterSql = GetStandardFilterSQLForContent();
                                        sFilterSql = sFilterSql + " and nstructid=" + mnPageId;
                                        if (ContentModule.HasAttribute("TotalCount") == false)
                                        {
                                            ContentModule.SetAttribute("TotalCount", 0.ToString());
                                        }
                                        var argoPageElmt1 = moPageXml.DocumentElement;
                                        XmlElement oPagedetail = null;
                                        GetContentXMLByTypeAndOffset(ref argoPageElmt1, SingleContentType + cSort, (long)nStart, (long)nRows, ref oPagedetail, oContentModule: ref ContentModule, sFilterSql, bShowContentDetails: false);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    // if we are on a system page we only want the content on that page not parents.
                    GetPageContentXml((long)mnPageId);
                    oPageElmt.SetAttribute("systempage", "true");

                }

                // Check content versions
                // If gbVersionControl Then CheckContentVersions()
                // moved to GetPageXml because content may be added from module overloads or grabbers etc.


                // Always ensure we have a content node
                XmlElement oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oRoot);
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXml", ex, sProcessInfo));
            }

        }

        public virtual void GetPageContentXml(long nPageId)
        {
            PerfMon.Log("Web", "GetPageContentXml");
            string sProcessInfo = "Getting the content from page " + nPageId;
            string sSql = "";
            string sFilterSql = "";
            string sWhereSql = string.Empty;
            string sSql2 = string.Empty;
            XmlElement oRoot;
            try
            {

                // Create the live filter
                sFilterSql = GetStandardFilterSQLForContent();

                oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oRoot);
                }

                string nCurrentPageId = nPageId.ToString();
                // Adjust the page id if it's a cloned page.
                if (Convert.ToDouble(nCurrentPageId) != (double)mnPageId)
                {
                    // we are only pulling in cascaded items
                    sFilterSql += " and CL.bCascade = 1 and CL.bPrimary = 1 ";
                }
                else
                {
                    // If we have an article id we only want to show cascaded content
                    if (moConfig["ContentDetailShowOnlyCascaded"] != null)
                    {
                        if (moConfig["ContentDetailShowOnlyCascaded"].ToLower() == "on" && mnArtId != 0)
                        {
                            if (ibIndexMode)
                            {
                                //when we are indexing we want to be able to index the brief because we use this as the abstract.
                                sFilterSql += " and ((CL.bCascade = 1 and CL.bPrimary = 1) or nContentKey = " + mnArtId + ") ";
                            }
                            else
                            {
                                sFilterSql += " and CL.bCascade = 1 and CL.bPrimary = 1 ";
                            }
                        }
                    }
                    // we are pulling in located and native items but not cascaded
                }

                // Check if the page is a cloned page
                // If it is, then we need to switch the page id.
                if (gbClone)
                {
                    int nClonePage = moDbHelper.getClonePageID((int)nPageId);
                    if (nClonePage > 0)
                        nPageId = nClonePage;
                }

                // sSql = "select c.nContentKey as id, (select TOP 1 CL2.nStructId from tblContentLocation CL2 where CL2.nContentId=c.nContentKey and CL2.bPrimary = 1) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" & _

                if (!string.IsNullOrEmpty(gcBlockContentType))
                {

                    string gcBlockContentTypeRemovedIds = Regex.Replace(gcBlockContentType, @"[|\d]+", "");

                    sFilterSql = sFilterSql + " and c.cContentSchemaName NOT IN ('" + gcBlockContentTypeRemovedIds.Replace(",", "','") + "') ";
                }
                string cContentLimit = "";
                if (!string.IsNullOrEmpty(moConfig["ContentLimit"]) & Tools.Number.IsNumeric(moConfig["ContentLimit"]))
                {
                    cContentLimit = " TOP " + moConfig["ContentLimit"] + " ";
                }

                sSql = "select " + cContentLimit + "c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c" + " inner join tblContentLocation CL on c.nContentKey = CL.nContentId" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where( CL.nStructId = " + nPageId;
                sSql = sSql + sFilterSql + ") order by type, cl.nDisplayOrder";

                var oDs = new DataSet();
                oDs = moDbHelper.GetDataSet(sSql, "Content", "Contents");
                PerfMon.Log("Web", "AddDataSetToContent - For Page ", sSql);
                moDbHelper.AddDataSetToContent(ref oDs, ref oRoot, ref mdPageExpireDate, ref mdPageUpdateDate, Convert.ToInt64(nCurrentPageId), false, "");
            }

            // If gbCart Or gbQuote Then
            // moDiscount.getAvailableDiscounts(oRoot)
            // End If


            // AddGroupsToContent(oRoot)



            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getPageContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetPagecontentXml", ex, sProcessInfo));
            }

        }

        public void AddGroupsToContent(ref XmlElement oContentElmt)
        {
            // Adds ProductGroups to content nodes
            PerfMon.Log("Web", "AddGroupsToContent-start");
            try
            {

                // TS This is hammering performance on sites with lots of discount rules and doesn't seem to be used anywhere ?
                // if this is requred we need only pull back categories for (products or other content names sold) that are on the current page seriously reducing the time required.

                // build In statement

                XmlElement oContElmt;
                string InStatement = " tblCartCatProductRelations.nContentId IN(";
                foreach (XmlElement currentOContElmt in oContentElmt.SelectNodes("Content"))
                {
                    oContElmt = currentOContElmt;
                    InStatement = InStatement + oContElmt.GetAttribute("id") + ",";
                }
                InStatement = InStatement.Trim(',') + ")";

                string cSQL = "SELECT tblCartProductCategories.nCatKey AS id, tblCartProductCategories.cCatSchemaName As type, tblCartProductCategories.nCatParentId As parent, " + " tblCartProductCategories.cCatName As name, tblCartProductCategories.cCatDescription As description, tblCartCatProductRelations.nContentId" + " FROM tblCartProductCategories INNER JOIN" + " tblAudit On tblCartProductCategories.nAuditId = tblAudit.nAuditKey INNER JOIN" + " tblCartCatProductRelations On tblCartProductCategories.nCatKey = tblCartCatProductRelations.nCatId";
                if (!mbAdminMode)
                {
                    cSQL += " WHERE tblAudit.nStatus = 1 " + " And (tblAudit.dPublishDate Is null Or tblAudit.dPublishDate = 0 Or tblAudit.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + " )" + " And (tblAudit.dExpireDate Is null Or tblAudit.dExpireDate = 0 Or tblAudit.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + " )" + " And " + InStatement;
                }
                else
                {
                    cSQL += " where " + InStatement;
                }
                var oDS = new DataSet();
                oDS = moDbHelper.GetDataSet(cSQL, "ContentGroup", "ContentGroups");

                PerfMon.Log("Web", "AddGroupsToContent-startloop-" + oDS.Tables["ContentGroup"].Rows.Count);

                foreach (DataRow oDR in oDS.Tables["ContentGroup"].Rows)
                {

                    foreach (XmlElement currentOContElmt1 in oContentElmt.SelectNodes($"Content[@id='{oDR["nContentId"]}']" ))
                    {
                        oContElmt = currentOContElmt1;
                        var oGroupElmt = oContentElmt.OwnerDocument.CreateElement("ContentGroup");
                        oGroupElmt.SetAttribute("id", (oDR["id"] ?? "").ToString());
                        oGroupElmt.SetAttribute("type", (oDR["type"] ?? "").ToString());
                        oGroupElmt.SetAttribute("name", (oDR["name"] ?? "").ToString());
                        oGroupElmt.SetAttribute("parent", (oDR["parent"] ?? "").ToString());
                        oGroupElmt.InnerText = (oDR["description"] ?? "").ToString();
                        oContElmt.AppendChild(oGroupElmt);
                    }
                }
                PerfMon.Log("Web", "AddGroupsToContent-end");
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "AddGroupsToConent", ex, gcEwSiteXsl, "", gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddGroupsToContent", ex, ""));
            }
        }



        public string GetContentUrl(long nContentId)
        {
            PerfMon.Log("Web", "GetContentUrl");
            string sSql;
            string sFilterSql;
            string ContentName = "";
            string[] ParentPages;
            var PrimaryPageId = default(long);
            string ContentURL = "/" + nContentId.ToString() + "-/";
            string sProcessInfo = "Getting path for content id " + nContentId;
            try
            {
                sFilterSql = GetStandardFilterSQLForContent();

                sSql = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c" + " left outer join tblContentLocation CL on c.nContentKey = CL.nContentId and CL.bPrimary = 1" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where(c.nContentKey = " + nContentId + sFilterSql + ") order by type, cl.nDisplayOrder";

                var oDs = new DataSet();
                oDs = moDbHelper.GetDataSet(sSql, "Content", "Contents");

                if (oDs.Tables.Count > 0 && oDs.Tables[0].Rows.Count > 0)
                {

                    foreach (DataRow oRow in oDs.Tables[0].Rows)
                    {

                        if (oRow["type"]?.ToString() == "SKU")
                        {
                            // get the id of the parent product

                            sSql = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c" + " inner join tblContentLocation CL on c.nContentKey = CL.nContentId" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " inner join tblContentRelation cr on c.nContentKey = cr.nContentParentId" + " where( cr.nContentChildId = " + nContentId;
                            sSql = sSql + sFilterSql + " and CL.bPrimary = 1) order by type, cl.nDisplayOrder";
                            var oDs2 = new DataSet();
                            oDs2 = moDbHelper.GetDataSet(sSql, "Content", "Contents");
                            foreach (DataRow oRow2 in oDs2.Tables[0].Rows)
                            {
                                ContentName = Convert.ToString(oRow2["name"]);
                                if (oRow2["parId"].ToString().Contains(","))
                                {
                                    ParentPages = oRow2["parId"].ToString().Split(',');
                                    if (Convert.ToDouble(ParentPages[0]) > 0d)
                                    {
                                        PrimaryPageId = Convert.ToInt64(ParentPages[0]);
                                    }
                                }
                                else if (Tools.Number.IsNumeric(oRow2["parId"]))
                                {
                                    PrimaryPageId = Convert.ToInt64(oRow2["parId"]);
                                }
                                ContentURL = "/" + Convert.ToString(oRow2["id"]) + "-/";
                            }
                        }

                        else
                        {

                            ContentName = Convert.ToString(oRow["name"]);
                            if (oRow["parId"].ToString().Contains(","))
                            {
                                ParentPages = oRow["parId"].ToString().Split(',');
                                if (Convert.ToDouble(ParentPages[0]) > 0d)
                                {
                                    PrimaryPageId = Convert.ToInt64(ParentPages[0]);
                                }
                            }
                            else if (Tools.Number.IsNumeric(oRow["parId"]))
                            {
                                PrimaryPageId = Convert.ToInt64(oRow["parId"]);
                            }
                        }

                    }
                }
                if (PrimaryPageId > 0L)
                {
                    XmlElement pageMenuElmt = (XmlElement)moPageXml.SelectSingleNode("/Page/Menu/descendant-or-self::MenuItem[@id='" + PrimaryPageId + "']");
                    if (pageMenuElmt != null)
                    {
                        ContentURL = pageMenuElmt.GetAttribute("url") + ContentURL;
                    }
                }
                if (!string.IsNullOrEmpty(ContentName))
                {
                    ContentName = ContentName.Replace(" ", "-");
                    ContentName = goServer.UrlEncode(ContentName);
                    ContentURL = ContentURL + ContentName;
                }


                if (moConfig["LowerCaseUrl"] == "on")
                {
                    ContentURL = ContentURL.ToLower();
                }
                return ContentURL;
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXml", ex, sProcessInfo));
                return "";
            }

        }
        /// <summary>
        /// The purpose of this function is to check if the user should be seeing a Pending version of the content
        /// as opposed to the live one.  If the user is an admin or has AddUpdate permissions then this should be the case
        /// </summary>
        /// <remarks></remarks>
        protected void CheckContentVersions()
        {
            PerfMon.Log("Web", "CheckContentVersions");

            string cProcessInfo = "";

            DataSet oDs;
            string cSql = "";
            string cOwnerFilter = "";

            XmlNodeList childNodes = null;
            XmlElement oReplacement;

            try
            {

                // We need to check two things
                // - for content that hasn't been retrieved for this page, but is Pending (this will be in the content table)
                // - for content that is on this page, but has a version on the Versions table called Pending.


                var nPagePermission = moDbHelper.getPagePermissionLevel((long)mnPageId);

                if (Cms.dbHelper.CanAddUpdate(nPagePermission) | mbAdminMode)
                {

                    var oTempNode = moPageXml.CreateElement("temp");

                    // Now check if it's everything we're getting, or just what the user created.
                    if (!mbAdminMode & nPagePermission == Cms.dbHelper.PermissionLevel.AddUpdateOwn | nPagePermission == Cms.dbHelper.PermissionLevel.AddUpdateOwnPublish)
                    {
                        // Put a filter in to limit checks to content we've added
                        cOwnerFilter = " and @owner=" + mnUserId;
                    }

                    string filter = "[@id[number(.)=number(.)]" + cOwnerFilter + "]";

                    string cCheckContentList = "";

                    foreach (XmlElement oContent in moPageXml.SelectNodes("//Content" + filter))
                    {

                        // Add the id if it is not null or zero
                        if (!string.IsNullOrEmpty(oContent.GetAttribute("id")) && Tools.Number.IsNumeric(oContent.GetAttribute("id")) && Convert.ToInt64(oContent.GetAttribute("id")) > 0L)

                        {
                            if (!string.IsNullOrEmpty(cCheckContentList))
                                cCheckContentList += ",";
                            cCheckContentList += oContent.GetAttribute("id");
                        }

                    }

                    if (!string.IsNullOrEmpty(cCheckContentList))
                    {
                        // Get any content on the page that has a Status Pending Version
                        cSql = "select c.nContentPrimaryId as id, " + "   dbo.fxn_getContentParents(c.nContentPrimaryId) as parId, " + "   cContentForiegnRef as ref, cContentName as name, " + "   cContentSchemaName as type, " + "   cContentXmlBrief as content, " + "   a.nStatus as status, " + "   a.dpublishDate as publish, " + "   a.dExpireDate as expire, " + "   a.dUpdateDate as [update], " + "   c.nVersion as version, " + "   c.nContentVersionKey as versionid, " + "   a.nInsertDirId as owner " + " from tblContentVersions c " + "   inner join tblAudit a " + "   on c.nAuditId = a.nAuditKey " + " WHERE c.nContentPrimaryId IN (" + cCheckContentList + ")" + "   AND a.nStatus = " + ((int)Cms.dbHelper.Status.Pending).ToString() + " AND c.nVersion = (Select TOP 1 nVersion from tblContentVersions c2 where c.nContentPrimaryId = c2.nContentPrimaryId AND a.nStatus = " + ((int)Cms.dbHelper.Status.Pending).ToString() + " ORDER BY nVersion DESC)";

                        oDs = moDbHelper.GetDataSet(cSql, "Content", "Contents");

                        if (oDs.Tables.Count > 0 && oDs.Tables[0].Rows.Count > 0)
                        {
                            // If contentversion exists, replace the current node with this.

                            oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;

                            if (oDs.Tables[0].Columns.Contains("parID"))
                            {
                                oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                            }

                            // Added to handle the new relationship type
                            if (oDs.Tables[0].Columns.Contains("rtype"))
                            {
                                oDs.Tables[0].Columns["rtype"].ColumnMapping = MappingType.Attribute;
                            }

                            {
                                var withBlock = oDs.Tables[0];
                                withBlock.Columns["ref"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["name"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["type"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["status"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["publish"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["expire"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["owner"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["update"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["version"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["versionid"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["content"].ColumnMapping = MappingType.SimpleContent;
                            }

                            oDs.EnforceConstraints = false;
                            // convert to Xml Dom
                            //var oXml = new XmlDataDocument(oDs);
                            XmlDocument oXml = new XmlDocument();
                            if (oDs.Tables[0].Rows.Count > 0)
                            {
                                oXml.LoadXml(oDs.GetXml());
                            }
                            oXml.PreserveWhitespace = false;

                            foreach (XmlElement oReplaceContent in oXml.SelectNodes("/Contents/Content"))
                            {

                                XmlElement oContent = (XmlElement)moPageXml.SelectSingleNode("//Content[@id='" + oReplaceContent.GetAttribute("id") + "']");

                                // Before we replace the node, move any child Content nodes (i.e. related content) out from it.
                                childNodes = oContent.SelectNodes("Content");
                                if (childNodes.Count > 0)
                                {
                                    foreach (XmlElement child in childNodes)
                                    {
                                        oContent.RemoveChild(child);
                                        oTempNode.AppendChild(child);
                                    }
                                }

                                // Replace the contentNode
                                oReplacement = (XmlElement)moPageXml.ImportNode(oReplaceContent, true);
                                DateTime argdExpireDate = DateTime.Parse("0001-01-01");
                                DateTime argdUpdateDate = DateTime.Parse("0001-01-01");
                                oReplacement = moDbHelper.SimpleTidyContentNode(ref oReplacement, dExpireDate: ref argdExpireDate, dUpdateDate: ref argdUpdateDate);
                                oContent.ParentNode.ReplaceChild(oReplacement, oContent);

                                // Move the related content back
                                if (childNodes.Count > 0)
                                {
                                    foreach (XmlElement child in childNodes)
                                    {
                                        oTempNode.RemoveChild(child);
                                        oReplacement.AppendChild(child);
                                    }
                                }

                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckContentVersions", ex, cProcessInfo));
            }

        }

        protected void CheckContentVersions_Replaced()
        {
            PerfMon.Log("Web", "CheckContentVersions");

            string cProcessInfo = "";

            DataSet oDs;
            string cSql = "";
            string cOwnerFilter = "";

            XmlNodeList childNodes = null;
            XmlElement oReplacement;

            try
            {

                // We need to check two things
                // - for content that hasn't been retrieved for this page, but is Pending (this will be in the content table)
                // - for content that is on this page, but has a version on the Versions table called Pending.


                var nPagePermission = moDbHelper.getPagePermissionLevel((long)mnPageId);

                if (Cms.dbHelper.CanAddUpdate(nPagePermission) | mbAdminMode)
                {

                    var oTempNode = moPageXml.CreateElement("temp");

                    // Now check if it's everything we're getting, or just what the user created.
                    if (!mbAdminMode & nPagePermission == Cms.dbHelper.PermissionLevel.AddUpdateOwn | nPagePermission == Cms.dbHelper.PermissionLevel.AddUpdateOwnPublish)
                    {

                        // Put a filter in to limit checks to content we've added
                        cOwnerFilter = " and @owner=" + mnUserId;

                    }

                    string filter = "[@id[number(.)=number(.)]" + cOwnerFilter + "]";



                    // only search content that has an id which is numeric
                    foreach (XmlElement oContent in moPageXml.SelectNodes("//Content" + filter))
                    {

                        // Search for pending versions of the content

                        cSql = "select TOP 1 c.nContentPrimaryId as id, " + "   dbo.fxn_getContentParents(c.nContentPrimaryId) as parId, " + "   cContentForiegnRef as ref, cContentName as name, " + "   cContentSchemaName as type, " + "   cContentXmlBrief as content, " + "   a.nStatus as status, " + "   a.dpublishDate as publish, " + "   a.dExpireDate as expire, " + "   a.dUpdateDate as [update], " + "   c.nVersion as version, " + "   c.nContentVersionKey as versionid, " + "   a.nInsertDirId as owner " + " from tblContentVersions c " + "   inner join tblAudit a " + "     on c.nAuditId = a.nAuditKey " + " WHERE c.nContentPrimaryId = " + oContent.GetAttribute("id") + "   AND a.nStatus = " + ((int)Cms.dbHelper.Status.Pending).ToString() + " ORDER BY c.nVersion DESC ";

                        oDs = moDbHelper.GetDataSet(cSql, "Content", "Contents");

                        if (oDs.Tables.Count > 0 && oDs.Tables[0].Rows.Count > 0)
                        {
                            // If contentversion exists, replace the current node with this.


                            oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;

                            if (oDs.Tables[0].Columns.Contains("parID"))
                            {
                                oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                            }

                            // Added to handle the new relationship type
                            if (oDs.Tables[0].Columns.Contains("rtype"))
                            {
                                oDs.Tables[0].Columns["rtype"].ColumnMapping = MappingType.Attribute;
                            }

                            {
                                var withBlock = oDs.Tables[0];
                                withBlock.Columns["ref"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["name"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["type"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["status"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["publish"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["expire"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["owner"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["update"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["version"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["versionid"].ColumnMapping = MappingType.Attribute;
                                withBlock.Columns["content"].ColumnMapping = MappingType.SimpleContent;
                            }

                            oDs.EnforceConstraints = false;
                            // convert to Xml Dom
                            //var oXml = new XmlDataDocument(oDs);
                            XmlDocument oXml = new XmlDocument();
                            if (oDs.Tables[0].Rows.Count > 0)
                            {
                                oXml.LoadXml(oDs.GetXml());
                            }
                            oXml.PreserveWhitespace = false;


                            // Before we replace the node, move any child Content nodes (i.e. related content) out from it.
                            childNodes = oContent.SelectNodes("Content");
                            if (childNodes.Count > 0)
                            {
                                foreach (XmlElement child in childNodes)
                                {
                                    oContent.RemoveChild(child);
                                    oTempNode.AppendChild(child);
                                }
                            }

                            // Replace the contentNode
                            oReplacement = (XmlElement)moPageXml.ImportNode(oXml.SelectSingleNode("/Contents/Content"), true);
                            DateTime argdExpireDate = DateTime.Parse("0001-01-01");
                            DateTime argdUpdateDate = DateTime.Parse("0001-01-01");
                            oReplacement = moDbHelper.SimpleTidyContentNode(ref oReplacement, dExpireDate: ref argdExpireDate, dUpdateDate: ref argdUpdateDate);
                            oContent.ParentNode.ReplaceChild(oReplacement, oContent);

                            // Move the related content back
                            if (childNodes.Count > 0)
                            {
                                foreach (XmlElement child in childNodes)
                                {
                                    oTempNode.RemoveChild(child);
                                    oReplacement.AppendChild(child);
                                }
                            }
                        }


                    }


                }
            }


            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CheckContentVersions", ex, cProcessInfo));
            }

        }


        public void GetErrorXml(ref XmlElement oPageElmt)
        {
            PerfMon.Log("Web", "GetErrorXml");
            XmlElement oRoot;
            XmlElement oElmt;
            string sFilterSql = string.Empty;
            string sSql2 = string.Empty;
            string sNodeName = string.Empty;
            string sContent = string.Empty;
            string sErrorModule = "";
            string sErrorHTML = string.Empty;
            string strMessageHtml = "";
            string strMessageText = "";

            string sProcessInfo = "building the Content XML";

            try
            {

                oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oRoot);
                }

                oPageElmt.SetAttribute("systempage", "true");

                oElmt = moPageXml.CreateElement("Content");
                oElmt.SetAttribute("type", "Error");
                oElmt.SetAttribute("name", mnProteanCMSError.ToString());
                switch (mnProteanCMSError)
                {
                    case 1005L:
                        {
                            strMessageText = "Page Not Found";
                            strMessageHtml = "<div><h2>Page Not Found</h2>" + "</div>";
                            gnResponseCode = 404L;
                            sErrorModule = "BuildPageXml";
                            bPageCache = false;
                            break;
                        }
                    case 1006L:
                        {
                            strMessageText = "Access Denied";
                            strMessageHtml = "<div><h2>Access Denied</h2>" + "</div>";
                            // gnResponseCode = 401
                            sErrorModule = "BuildPageXml";

                            bPageCache = false;
                            break;
                        }
                    case 1007L:
                        {
                            strMessageText = "File Not Found";
                            strMessageHtml = "<div><h2>File Not Found</h2>" + "</div>";
                            // gnResponseCode = 404
                            sErrorModule = "Get Document";

                            bPageCache = false;
                            break;
                        }

                    case 1008L:
                        {
                            strMessageText = "Invalid Licence";
                            strMessageHtml = "<div><h2>Please get a valid ProteanCMS Licence</h2>" + "</div>";
                            sErrorModule = "BuildPageXml";

                            bPageCache = false;
                            break;
                        }
                }

                if (gbDebug)
                {
                    try
                    {
                        throw new Exception(strMessageText);
                    }
                    catch (Exception)
                    {
                        oPageElmt.SetAttribute("layout", "Error");
                        oElmt.InnerXml = strMessageHtml;
                    }
                }
                else
                {
                    oPageElmt.SetAttribute("layout", "Error");
                    oElmt.InnerXml = strMessageHtml;
                }

                oRoot.AppendChild(oElmt);
                oPageElmt.AppendChild(oRoot);
            }



            catch (Exception ex)
            {

                // returnException(msException, mcModuleName, "GetErrorXml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetErrorXml", ex, sProcessInfo));
            }

        }

        public void GetContentXMLByType(ref XmlElement oPageElmt, string cContentType, string sqlFilter = "", string fullSQL = "")
        {
            PerfMon.Log("Web", "GetContentXMLByType");
            // <add key="ControlPanelTypes" value="Event,Document|Top_10|DESC_Publish"/>
            try
            {
                string[] oTypeCriteria = cContentType.Split('|');
                string cTop = "";
                string cOrderDirection = "";
                string oOrderField = "";
                string strContentType = "";

                int i;
                var loopTo = oTypeCriteria.Length - 1;
                for (i = 0; i <= loopTo; i++)
                {
                    if (oTypeCriteria[i].Contains("Top_"))
                    {
                        cTop = oTypeCriteria[i].Split('_')[1];
                        if (!Tools.Number.IsNumeric(cTop))
                            cTop = "";
                    }
                    else if (oTypeCriteria[i].Contains("ASC_"))
                    {
                        cOrderDirection = "";
                        oOrderField = oTypeCriteria[i].Split('_')[1];
                    }
                    else if (oTypeCriteria[i].Contains("DESC_"))
                    {
                        cOrderDirection = "DESC";
                        oOrderField = oTypeCriteria[i].Split('_')[1];
                    }
                    else
                    {
                        // its the field name
                        strContentType = oTypeCriteria[i];
                    }
                }

                string cSQL = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c left outer join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where (cContentSchemaName = '" + strContentType + "') ";
                if (!string.IsNullOrEmpty(sqlFilter))
                {
                    cSQL += sqlFilter;
                }
                cSQL += GetStandardFilterSQLForContent();

                if (!string.IsNullOrEmpty(oOrderField))
                {
                    cSQL += " ORDER BY " + oOrderField + " " + cOrderDirection;
                }

                if (!string.IsNullOrEmpty(fullSQL))
                {
                    cSQL = fullSQL;
                }

                var oDS = moDbHelper.GetDataSet(cSQL, "Content1", "Contents");
                var oDT = new DataTable();
                oDT = oDS.Tables["Content1"].Copy();
                oDT.Rows.Clear();
                oDT.TableName = "Content";
                oDS.Tables.Add(oDT);
                int nMax = 0;
                string cDoneIds = ",";
                string ochkStr = "";
                if (Tools.Number.IsNumeric(cTop))
                    nMax = Convert.ToInt16(cTop);
                foreach (DataRow oDR in oDS.Tables["Content1"].Rows)
                {
                    if (oDS.Tables["Content"].Rows.Count < nMax | nMax == 0)
                    {
                        if (Tools.Number.IsNumeric(oDR["parId"]) && !oDR["parId"].ToString().Contains(","))
                        {
                            ochkStr = moDbHelper.checkPagePermission(Convert.ToInt64(oDR["parId"])).ToString();
                            if (Tools.Number.IsNumeric(ochkStr))
                            {
                                if (Convert.ToInt16(ochkStr).ToString() == oDR["parId"].ToString() && !cDoneIds.Contains($",{oDR["id"]},"))
                                {
                                    oDS.Tables["Content"].ImportRow(oDR);
                                    cDoneIds = cDoneIds + oDR["id"] + ",";
                                }
                            }
                        }
                        else if (mbAdminMode) // if in adminmode get everything regardless
                        {
                            oDS.Tables["Content"].ImportRow(oDR);
                            cDoneIds = cDoneIds + oDR["id"] + ",";
                        }

                    }
                }
                oDS.Tables.Remove(oDS.Tables["Content1"]);
                XmlElement oRoot;
                oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                if (oRoot is null)
                {
                    oRoot = moPageXml.CreateElement("Contents");
                    moPageXml.DocumentElement.AppendChild(oRoot);
                }
                moDbHelper.AddDataSetToContent(ref oDS, ref oRoot, ref mdPageExpireDate, ref mdPageUpdateDate, (long)mnPageId, false, "");
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, "", gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXMLByType", ex, ""));
            }
        }

        public void GetContentXMLByTypeAndOffset(ref XmlElement oPageElmt, string cContentType, long nStartPos, long nItemCount, ref XmlElement oPageDetail, ref XmlElement oContentModule, string sqlFilter = "", string fullSQL = "", bool bShowContentDetails = true)
        {
            PerfMon.Log("Web", "GetContentXMLByTypeAndOffset");
            // <add key="ControlPanelTypes" value="Event,Document|Top_10|DESC_Publish"/>
            try
            {
                string[] oTypeCriteria = cContentType.Split('|');
                string cTop = "";
                string cOrderDirection = "";
                string oOrderField = "";
                string strContentType = "";

                int i;
                var loopTo = oTypeCriteria.Length - 1;
                for (i = 0; i <= loopTo; i++)
                {
                    if (oTypeCriteria[i].Contains("Top_"))
                    {
                        cTop = oTypeCriteria[i].Split('_')[1];
                        if (!Tools.Number.IsNumeric(cTop))
                            cTop = "";
                    }
                    else if (oTypeCriteria[i].Contains("ASC_"))
                    {
                        cOrderDirection = "";
                        oOrderField = oTypeCriteria[i].Split('_')[1];
                    }
                    else if (oTypeCriteria[i].Contains("DESC_"))
                    {
                        cOrderDirection = "DESC";
                        oOrderField = oTypeCriteria[i].Split('_')[1];
                    }
                    else
                    {
                        // its the field name
                        strContentType = oTypeCriteria[i];
                    }
                }


                if (nStartPos < 0L)
                    nStartPos = 0L;
                if (nItemCount < 1L)
                    nItemCount = 1000L;


                // Quick call to get the total number of records
                string cSQL = "select count(*) from tblContent c left outer join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where (cContentSchemaName = '" + strContentType + "') ";
                if (!string.IsNullOrEmpty(sqlFilter))
                {
                    cSQL += sqlFilter;
                }
                long nTotal = Convert.ToInt64(moDbHelper.GetDataValue(cSQL, CommandType.Text, null, (object)0));

                if (nTotal > 0L)
                {
                    cSQL = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, CL.cPosition as position from tblContent c left outer join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where (cContentSchemaName = '" + strContentType + "') ";

                    if (!string.IsNullOrEmpty(sqlFilter))
                    {
                        cSQL += sqlFilter;
                    }
                    cSQL += GetStandardFilterSQLForContent();

                    if (!string.IsNullOrEmpty(oOrderField))
                    {
                        cSQL += " ORDER BY " + oOrderField + " " + cOrderDirection;
                    }

                    if (!string.IsNullOrEmpty(fullSQL))
                    {
                        cSQL = fullSQL;
                    }

                    cSQL += " offset " + nStartPos + " rows fetch next " + nItemCount + " rows only";

                    var oDS = moDbHelper.GetDataSet(cSQL, "Content1", "Contents");
                    var oDT = new DataTable();
                    oDT = oDS.Tables["Content1"].Copy();
                    oDT.Rows.Clear();
                    oDT.TableName = "Content";
                    oDS.Tables.Add(oDT);
                    int nMax = 0;
                    string cDoneIds = ",";
                    string ochkStr = "";
                    if (Tools.Number.IsNumeric(cTop))
                        nMax = Convert.ToInt16(cTop);
                    foreach (DataRow oDR in oDS.Tables["Content1"].Rows)
                    {
                        if (oDS.Tables["Content"].Rows.Count < nMax | nMax == 0)
                        {
                            if (Tools.Number.IsNumeric(oDR["parId"]) && !oDR["parId"].ToString().Contains(","))
                            {
                                ochkStr = moDbHelper.checkPagePermission(Convert.ToInt64(oDR["parId"])).ToString();
                                if (Tools.Number.IsNumeric(ochkStr))
                                {
                                    if (Convert.ToInt16(ochkStr).ToString() == oDR["parId"].ToString() && !cDoneIds.Contains($",{oDR["id"]},"))
                                    {
                                        oDS.Tables["Content"].ImportRow(oDR);
                                        cDoneIds = cDoneIds + oDR["id"] + ",";
                                    }
                                }
                            }
                            else if (mbAdminMode) // if in adminmode get everything regardless
                            {
                                oDS.Tables["Content"].ImportRow(oDR);
                                cDoneIds = cDoneIds + oDR["id"] + ",";
                            }

                        }
                    }
                    oDS.Tables.Remove(oDS.Tables["Content1"]);
                    XmlElement oRoot;
                    oRoot = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Contents");
                    if (oRoot is null)
                    {
                        oRoot = moPageXml.CreateElement("Contents");
                        moPageXml.DocumentElement.AppendChild(oRoot);
                    }
                    moDbHelper.AddDataSetToContent(ref oDS, ref oRoot, ref mdPageExpireDate, ref mdPageUpdateDate, (long)mnPageId, false, "");
                    if (bShowContentDetails)
                    {
                        // Get the content Detail element
                        XmlElement oContentDetails;
                        if (oPageDetail is null)
                        {
                            oContentDetails = (XmlElement)moPageXml.SelectSingleNode("Page/ContentDetail");
                            if (oContentDetails is null)
                            {
                                oContentDetails = moPageXml.CreateElement("ContentDetail");
                                if (!string.IsNullOrEmpty(moPageXml.InnerXml))
                                {
                                    moPageXml.FirstChild.AppendChild(oContentDetails);
                                }
                                else
                                {
                                    oPageDetail.AppendChild(oContentDetails);
                                }

                            }
                        }
                        else
                        {
                            oContentDetails = oPageDetail;
                        }

                        oContentDetails.SetAttribute("start", nStartPos.ToString());
                        oContentDetails.SetAttribute("total", nTotal.ToString());
                        oContentDetails.SetAttribute("rows", nItemCount.ToString());
                    }

                    if (oContentModule != null)
                    {
                        if (oContentModule.HasAttribute("TotalCount"))
                        {
                            oContentModule.SetAttribute("TotalCount", nTotal.ToString());
                        }
                    }

                }
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "getContentXml", ex, gcEwSiteXsl, "", gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentXMLByTypeAndOffset", ex, ""));
            }
        }

        public XmlElement BuildPageContentDetailXml(XmlElement oPageElmt = null, long nArtId = 0L, bool disableRedirect = false, bool bCheckAccessToContentLocation = false, long nVersionId = 0L, bool bIgnoreContentStatus = false)
        {
            PerfMon.Log("Web", "BuildPageContentDetailXml");
            XmlElement oRoot;
            XmlElement oElmt;
            XmlElement retElmt = null;
            string sProcessInfo = "BuildContentDetailXml";
            if (nArtId > 0L)
            {
                mnArtId = (int)nArtId;
            }
            if (mnArtId == 0L)
            {
                return null;
            }
            try
            {
                if (moContentDetail is null)
                {

                    // If requested, we need to make sure that the content we are looking for doesn't belong to a page
                    // that the user is not allowed to access.
                    // We can review the current menu structure xml instead of calling the super slow permissions functions.

                    // Check for historic events special case
                    if (moPageXml.SelectSingleNode("Page/Contents/Content[@action='Protean.Cms+Content+Modules.ListHistoricEvents']") != null)
                    {
                        bAllowExpired = true;
                    }

                    // Use the consolidated GetContentDetailXml method which handles:
                    // - Access location checking
                    // - SQL query building (including version support)
                    // - DataSet processing
                    // - XML conversion
                    DateTime? contentUpdateDate;
                    oElmt = moDbHelper.GetContentDetailXml(
                        mnArtId,
                        bIgnoreContentStatus,  // noFilter
                        nVersionId,
                        bIgnoreContentStatus,
                        bCheckAccessToContentLocation,
                        true,  // bIncludeLocations
                        out contentUpdateDate);

                    if (contentUpdateDate.HasValue)
                    {
                        mdPageUpdateDate = contentUpdateDate.Value;
                    }

                    if (oElmt != null)
                    {
                        // Create the ContentDetail wrapper and import the content element
                        oRoot = moPageXml.CreateElement("ContentDetail");
                        var importedElmt = (XmlElement)moPageXml.ImportNode(oElmt, true);
                        oRoot.AppendChild(importedElmt);
                        oElmt = importedElmt;

                        // Apply page-level processing
                        XmlElement argoContentElmt = oElmt;
                        moDbHelper.addRelatedContent(ref argoContentElmt, mnArtId, mbAdminMode);
                        if (!string.IsNullOrEmpty(moConfig["ShowOwnerOnDetail"]))
                        {
                            string cContentType = oElmt.GetAttribute("type");
                            if (moConfig["ShowOwnerOnDetail"].Contains(cContentType))
                            {
                                long nOwner = Convert.ToInt64("0" + oElmt.GetAttribute("owner"));
                                if (nOwner > 0L)
                                {
                                    oElmt.AppendChild(GetUserXML(nOwner));
                                }
                            }
                        }

                        // If gbCart Or gbQuote Then
                        // moDiscount.getAvailableDiscounts(oRoot)
                        // End If

                        XmlElement contentElmt = (XmlElement)oRoot.SelectSingleNode("Content");
                        if (nVersionId > 0L)
                        {
                            contentElmt.SetAttribute("previewKey", Tools.Encryption.RC4.Encrypt(nVersionId.ToString(), moConfig["SharedKey"]));
                        }
                        XmlElement argoContentElmt1 = oRoot;  // oRoot IS the ContentDetail element
                        AddGroupsToContent(ref argoContentElmt1);

                        // Add single item shipping costs for JSON-LD - MOVED BEFORE CLONE
                        string ProductTypes = moConfig["ProductTypes"];
                        if (string.IsNullOrEmpty(ProductTypes))
                            ProductTypes = defaultProductTypes;
                        if (ProductTypes.Contains(contentElmt.GetAttribute("type")) & moCart != null)
                        {
                            try
                            {
                                var oShippingElmt = moPageXml.CreateElement("ShippingCosts");
                                string cDestinationCountry = moCart.moCartConfig["DefaultDeliveryCountry"];
                                double nPrice = 0d;
                                if (contentElmt.SelectSingleNode("Prices/Price[@type='sale']") != null)
                                {
                                    nPrice = Convert.ToDouble("0" + contentElmt.SelectSingleNode("Prices/Price[@type='sale']").InnerText);
                                }

                                if (nPrice == 0d)
                                {
                                    if (contentElmt.SelectSingleNode("Prices/Price[@type='rrp']") != null)
                                    {
                                        nPrice = Convert.ToDouble("0" + contentElmt.SelectSingleNode("Prices/Price[@type='rrp']").InnerText);
                                    }
                                }
                                double nWeight = 0d;
                                if (contentElmt.SelectSingleNode("ShippingWeight") != null)
                                {
                                    nWeight = Convert.ToDouble("0" + contentElmt.SelectSingleNode("ShippingWeight").InnerText);
                                }
                                var dsShippingOption = moCart.getValidShippingOptionsDS(cDestinationCountry, nPrice, 1L, nWeight, mnArtId);
                                if (dsShippingOption != null)
                                {
                                    oShippingElmt.InnerXml = dsShippingOption.GetXml().Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");

                                    // Process cShipOptTandC nodes to convert text to InnerXml
                                    foreach (XmlNode oTandCNode in oShippingElmt.SelectNodes("//cShipOptTandC"))
                                    {
                                        if (oTandCNode is XmlElement oTandCElmt && !string.IsNullOrEmpty(oTandCElmt.InnerText))
                                        {
                                            string sHtmlContent = oTandCElmt.InnerText;
                                            try
                                            {
                                                // Try to convert the InnerText to InnerXml
                                                oTandCElmt.InnerXml = sHtmlContent;
                                            }
                                            catch (Exception)
                                            {
                                                oTandCElmt.InnerXml = stdTools.tidyXhtmlFrag(sHtmlContent, true);
                                            }
                                        }
                                    }
                                }
                                contentElmt.AppendChild(oShippingElmt);
                            }
                            catch (Exception)
                            {
                            }
                        }

                        if (oPageElmt != null)
                        {
                            var oContentDetail = contentElmt;
                            if (oContentDetail != null && !string.IsNullOrEmpty(oContentDetail.InnerXml.Trim()))
                            {
                                // If we can find a content detail Content node, 
                                // AND it contains some InnerXml, then YAY.
                                oPageElmt.AppendChild(oRoot.CloneNode(true));
                            }
                            else
                            {
                                // OTHERWISE if there is nothing in the detail we get the brief instead.
                                GetContentBriefXml(oPageElmt, nArtId);
                            }
                        }
                        retElmt = (XmlElement)oRoot.FirstChild;

                        if (mbAdminMode == false & moConfig["RedirectToDescriptiveContentURLs"]?.ToLower() == "true")
                        {
                            string SafeURLName = Tools.Text.CleanName(contentElmt.GetAttribute("name"), false, true);
                            string myOrigURL;
                            string myQueryString = "";
                            if (mcOriginalURL.Contains("?"))
                            {
                                myOrigURL = mcOriginalURL.Substring(0, mcOriginalURL.IndexOf("?"));
                                myQueryString = mcOriginalURL.Substring(mcOriginalURL.LastIndexOf("?"));
                            }
                            else
                            {
                                myOrigURL = mcOriginalURL;
                            }

                            if ((myOrigURL ?? "") != (mcPageURL + "/" + mnArtId + "-/" + SafeURLName ?? ""))
                            {
                                // we redirect perminently
                                mbRedirectPerm = Convert.ToString(true);
                                msRedirectOnEnd = mcPageURL + "/" + mnArtId + "-/" + SafeURLName + myQueryString;
                            }
                        }

                        // MEMORY FIX: Clear previous moContentDetail reference to allow GC
                        moContentDetail = null;
                        moContentDetail = (XmlElement)oRoot.FirstChild;

                        return moContentDetail;
                    }
                    else
                    {
                        sProcessInfo = "no content to add - we redirect";
                        // this content is not found either page not found or re-direct home.
                        if (!disableRedirect)
                        {
                            // put this in to prevent a redirect if we are calling this from somewhere strange.
                            if (gnPageNotFoundId > 1L)
                            {
                                mbAdminMode = false;
                                mnPageId = (int)gnPageNotFoundId;
                                mnArtId = 0;
                                moPageXml = new XmlDocument();
                                BuildPageXML();
                                moResponse.StatusCode = 404;
                            }
                            else
                            {
                                msRedirectOnEnd = moConfig["BaseUrl"];
                                moResponse.StatusCode = 404;
                            }
                        }
                        // Just a page no detail requested
                        return null;
                    }
                }

                else
                {
                    sProcessInfo = "content exists adding content";
                    oRoot = moContentDetail.OwnerDocument.CreateElement("ContentDetail");
                    oRoot.AppendChild(moContentDetail);
                    if (oPageElmt != null)
                    {
                        oPageElmt.AppendChild(oRoot);
                    }
                    AddGroupsToContent(ref oRoot);
                    retElmt = moContentDetail;
                    moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentDetailViewed, mnUserId, SessionID, DateTime.Now, mnArtId, 0, "");
                    return moContentDetail;
                }
            }
            catch (Exception ex)
            {

                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "BuildPageContentDetailXml", ex, sProcessInfo));
                return null;
            }
            //sSql = null;
        }


        public XmlElement GetContentBriefXml(XmlElement oPageElmt = null, long nArtId = 0L)
        {
            PerfMon.Log("Web", "GetContentBriefXml");
            XmlElement oRoot;
            XmlElement oElmt;
            XmlElement retElmt = null;
            string sContent;
            string sSql;
            string sProcessInfo = "GetContentBriefXml";
            var oDs = new DataSet();
            string sFilterSql = "";
            bool bLoadAsXml;
            XmlComment oComment;
            if (nArtId > 0L)
            {
                mnArtId = (int)nArtId;
            }
            try
            {
                if (moContentDetail is null)
                {
                    if (mnArtId > 0)
                    {
                        sProcessInfo = "loading content" + mnArtId;
                        sFilterSql += GetStandardFilterSQLForContent();
                        oRoot = moPageXml.CreateElement("ContentDetail");
                        sSql = "select c.nContentKey as id, cContentForiegnRef as ref, dbo.fxn_getContentParents(c.nContentKey) as parId, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.dpublishDate as publish, a.dExpireDate as expire, a.dUpdateDate as [update], a.nInsertDirId as owner, a.nStatus as status " + "from tblContent c inner join tblAudit a on c.nAuditId = a.nAuditKey  where c.nContentKey = " + mnArtId + sFilterSql;
                        oDs = moDbHelper.GetDataSet(sSql, "Content", "ContentDetail");
                        oDs.Tables[0].Columns["id"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["ref"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["name"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["type"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["publish"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["expire"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["update"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["owner"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["content"].ColumnMapping = MappingType.SimpleContent;

                        oRoot.InnerXml = oDs.GetXml().Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                        foreach (XmlNode oNode in oRoot.SelectNodes("/ContentDetail/Content"))
                        {
                            oElmt = (XmlElement)oNode;
                            sContent = oElmt.InnerText;
                            if (DateTime.TryParse(oElmt.GetAttribute("update"), out DateTime updateDate))
                                mdPageUpdateDate = updateDate;

                            // Try to convert the InnerText to InnerXml
                            // Also if the innerxml has Content as a first node, then get the innerxml of the content node.
                            try
                            {
                                oElmt.InnerXml = sContent;
                                bLoadAsXml = true;
                            }

                            catch (Exception)
                            {
                                // If the load failed, then flag it in the Content node and return the InnerText as a Comment
                                oComment = oRoot.OwnerDocument.CreateComment(oElmt.InnerText);
                                oElmt.SetAttribute("xmlerror", "getContentBriefXml");
                                oElmt.InnerXml = "";
                                oElmt.AppendChild(oComment);
                                oComment = null;
                                bLoadAsXml = false;
                            }

                            if (bLoadAsXml)
                            {

                                // Successfully converted to XML.
                                // Now check if the node imported is a Content node - if so get rid of the Content node
                                var oFirst = Tools.Xml.firstElement(ref oElmt);
                                if (oFirst.LocalName == "Content")
                                {
                                    oElmt.InnerXml = oFirst.InnerXml;
                                }

                                XmlElement argoContentElmt = (XmlElement)oNode;
                                moDbHelper.addRelatedContent(ref argoContentElmt, mnArtId, mbAdminMode);
                                //oNode = argoContentElmt;

                            }

                        }

                        // If gbCart Or gbQuote Then
                        // moDiscount.getAvailableDiscounts(oRoot)
                        // End If

                        if (oPageElmt != null)
                        {
                            oPageElmt.AppendChild(oRoot.FirstChild);
                        }
                        // AddGroupsToContent(oRoot)
                        retElmt = (XmlElement)oRoot.FirstChild;
                        return (XmlElement)oRoot.FirstChild;
                    }
                    else
                    {
                        sProcessInfo = "no content to add";
                    }
                }
                else
                {
                    sProcessInfo = "content exists adding content";
                    oRoot = moPageXml.CreateElement("ContentDetail");
                    oRoot.AppendChild(moContentDetail);
                    if (oPageElmt != null)
                    {
                        oPageElmt.AppendChild(oRoot);
                    }
                    // AddGroupsToContent(oRoot)
                    retElmt = moContentDetail;
                    moDbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.ContentDetailViewed, mnUserId, SessionID, DateTime.Now, mnArtId, 0, "");
                    return moContentDetail;
                }

                return retElmt;
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetContentBriefXml", ex, sProcessInfo));
                return null;
            }

        }

        /// <summary>
        /// This attempts to construct the standard SQL filter for getting LIVE content
        /// If version control is on it will also assess the page permissions, 
        /// and if appropriate, it will get content that is not LIVe but, say, PENDING.
        /// This method delegates to the dbHelper's consolidated implementation.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public string GetStandardFilterSQLForContent(bool bPrecedingAND = true)
        {
            PerfMon.Log("Web", "GetStandardFilterSQLForContent");

            try
            {
                // Determine expire at end of day setting from config
                bool expireAtEndOfDay = moConfig["ExpireAtEndOfDay"]?.ToLower() == "on";

                // Delegate to the dbHelper's consolidated implementation with all context values
                string sFilterSQL = moDbHelper.GetStandardFilterSQLForContent(
                    bPrecedingAND,
                    mbAdminMode,
                    mnUserPagePermission,
                    mbPreviewHidden,
                    bAllowExpired,
                    mdDate,
                    expireAtEndOfDay);

                PerfMon.Log("Web", "GetStandardFilterSQLForContent-END");
                return sFilterSQL;
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetStandardFilterSQLForContent", ex, ""));
                return "";
            }
        }


    }
}