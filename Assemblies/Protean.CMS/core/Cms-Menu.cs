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

        /// <summary>
        ///    Gets the Structure XML, but doesn't add it to the page XML
        /// </summary>
        /// <param name="nUserId">User ID that you want to check for permissions on - if in Admin Mode, then if this is -1, no permissions will be checked, otherwise the permissions will be checked AND enumerated</param>
        /// <param name="nRootId">The root ID of the Structure that you want to get</param>
        /// <param name="nCloneContextId">If the root ID occurs within a cloned part of the structure, then give the cloned part's root node id</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual XmlElement GetStructureXML(long nUserId = 0L, long nRootId = 0L, long nCloneContextId = 0L)
        {

            string cFunctionDef = "GetStructureXML([Long], [Long], [Long])";
            PerfMon.Log("Web", cFunctionDef);

            try
            {

                return GetStructureXML(nUserId, nRootId, nCloneContextId, "", false, false, true, false, false, "MenuItem", "Menu");
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, ""));
                return null;
            }

        }

        /// <summary>
        ///    Gets the Structure XML, and adds it to the page XML
        /// </summary>
        /// <param name="cMenuId">Adds an id attribute to the root node</param>
        /// <param name="nUserId">User ID that you want to check for permissions on - if in Admin Mode, then if this is -1, no permissions will be checked, otherwise the permissions will be checked AND enumerated</param>
        /// <param name="nRootId">The root ID of the Structure that you want to get</param>
        /// <param name="bLockRoot">Adds a loced attribute to the root node</param>
        /// <param name="nCloneContextId">If the root ID occurs within a cloned part of the structure, then give the cloned part's root node id</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual XmlElement GetStructureXML(string cMenuId, long nUserId = 0L, long nRootId = 0L, bool bLockRoot = false, long nCloneContextId = 0L)
        {

            string cFunctionDef = "GetStructureXML(String, [Long], [Long], [Boolean], [Long])";
            PerfMon.Log("Web", cFunctionDef);

            try
            {

                return GetStructureXML(nUserId, nRootId, nCloneContextId, cMenuId, true, bLockRoot, true, false, false, "MenuItem", "Menu");
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, ""));
                return null;
            }

        }

        public virtual XmlElement GetStructurePageXML(long nPageId, string cMenuItemNodeName, string cRootNodeName)
        {

            string cFunctionDef = "GetStructureXML(String, [Long], [Long], [Boolean], [Long])";
            PerfMon.Log("Web", cFunctionDef);

            string sSql;
            DataSet oDs;
            string sProcessInfo;
            XmlElement oElmt = null;

            try
            {

                sSql = "SELECT		" + " s.nStructKey as id, " + " s.nStructParId as parId, " + " s.cStructName as name, " + " s.cUrl as url, " + " s.cStructDescription as Description, " + " a.dPublishDate as publish, " + " a.dExpireDate as expire, " + " a.nStatus as status, " + " s.cStructForiegnRef as ref, " + " 'ADMIN' as access,	" + " s.cStructLayout as layout," + " s.nCloneStructId as clone," + " '' As accessSource," + " 0 As accessSourceId, " + " s.nVersionParId as vParId" + " FROM	tblContentStructure s" + " INNER JOIN  tblAudit a " + " ON s.nAuditId = a.nAuditKey" + " where(s.nVersionParId Is null Or s.nVersionParId = 0)" + " and s.nStructKey = " + nPageId;


                // Get the dataset
                oDs = moDbHelper.GetDataSet(sSql, cMenuItemNodeName, cRootNodeName);

                // Add nestings
                oDs.Relations.Add("rel01", oDs.Tables[0].Columns["id"], oDs.Tables[0].Columns["parId"], false);
                oDs.Relations["rel01"].Nested = true;

                if (oDs.Tables[0].Rows.Count > 0)
                {

                    // COLUMN MAPPING - STANDARD
                    // =========================
                    oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                    oDs.Tables[0].Columns["url"].ColumnMapping = MappingType.Attribute;
                    if (mbAdminMode)
                    {
                        oDs.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["publish"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["expire"].ColumnMapping = MappingType.Attribute;
                    }

                    // COLUMN MAPPING - ACCESS
                    // =========================
                    oDs.Tables[0].Columns["access"].ColumnMapping = MappingType.Attribute;
                    if (oDs.Tables[0].Columns.Contains("accessSource"))
                    {
                        oDs.Tables[0].Columns["accessSource"].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["accessSourceId"].ColumnMapping = MappingType.Attribute;
                    }

                    if (oDs.Tables[0].Columns.Contains("vParId"))
                    {
                        oDs.Tables[0].Columns["vParId"].ColumnMapping = MappingType.Attribute;
                    }

                    if (oDs.Tables[0].Columns.Contains("ref"))
                    {
                        oDs.Tables[0].Columns["ref"].ColumnMapping = MappingType.Attribute;
                    }

                    // COLUMN MAPPING - CLONES
                    // =========================
                    if (oDs.Tables[0].Columns.Contains("clone"))
                        oDs.Tables[0].Columns["clone"].ColumnMapping = MappingType.Attribute;


                    // COVERT DATASET TO XML
                    // =====================
                    oElmt = moPageXml.CreateElement(cRootNodeName);


                    sProcessInfo = "GetStructureXML-dsToXml";
                    PerfMon.Log("Web", sProcessInfo);

                    // TS added lines to avoid whitespace issues
                    var oXml = new XmlDocument();
                    oXml.LoadXml(oDs.GetXml());
                    oXml.PreserveWhitespace = false;

                    oElmt.InnerXml = oXml.DocumentElement.OuterXml;
                    string sContent;
                    // Convert any text to xml
                    foreach (XmlElement oElmt2 in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName + "/Description"))
                    {
                        sContent = oElmt2.InnerText;
                        if (!string.IsNullOrEmpty(sContent))
                        {
                            try
                            {
                                oElmt2.InnerXml = sContent;
                            }
                            catch
                            {
                                oElmt2.InnerXml = stdTools.tidyXhtmlFrag(sContent);
                            }
                        }
                    }

                }

                return oElmt;
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, ""));
                return null;
            }

        }

        /// <summary>
        /// Gets the Structure XML
        /// </summary>
        /// <param name="nUserId">User ID that you want to check for permissions on - if in Admin Mode, then if this is -1, no permissions will be checked, otherwise the permissions will be checked AND enumerated</param>
        /// <param name="nRootId">The root ID of the Structure that you want to get</param>
        /// <param name="nCloneContextId">If the root ID occurs within a cloned part of the structure, then give the cloned part's root node id</param>
        /// <param name="cMenuId">Adds an id attribute to the root node</param>
        /// <param name="bAddMenuToPageXML">If True, this will add the elmt to the page XML</param>
        /// <param name="bLockRoot">Adds a loced attribute to the root node</param>
        /// <param name="bUseCache">Can be used to force use of the Cache</param>
        /// <param name="bIncludeExpiredAndHidden">Include pages that are expired, yet to be published or hidden.</param>
        /// <param name="bPruneEvenIfInAdminMode">By default in admin mode all pages even DENIED ones are returned.  If this value is True then regardless of the admin mode status DENIED pages will be removed.</param>
        /// <param name="cMenuItemNodeName">Specifies the node name of each Page node.  Default should be "MenuItem".  Note - if not default, then caching will not be employed.</param>
        /// <param name="cRootNodeName">Specifies the root node name.  Default should be "Menu".  Note - if not default, then caching will not be employed.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual XmlElement GetStructureXML(long nUserId, long nRootId, long nCloneContextId, string cMenuId, bool bAddMenuToPageXML, bool bLockRoot, bool bUseCache, bool bIncludeExpiredAndHidden, bool bPruneEvenIfInAdminMode, string cMenuItemNodeName, string cRootNodeName)
        {
            string cFunctionDef = "GetStructureXML(Long,Long,Long,String,Boolean,Boolean,Boolean,Boolean,Boolean,String,String)";
            PerfMon.Log("Web", cFunctionDef);

            DataSet oDs;
            XmlElement oElmt = null;
            XmlElement oClone = null;
            int nCloneId = 0;
            int nCloneParentId = 0;

            int nTempRootId = 0;
            XmlElement oMenuItem;

            long nAuthUsers = gnAuthUsers;

            string sContent;
            string sSql;
            string cCacheMode = "off";
            bool bCacheXml = false;
            string cFilePathModifier = "";
            bool bAuth = true;

            string sProcessInfo = Convert.ToString(string.IsNullOrEmpty("GetStructureXML"));
            string cCacheType;

            try
            {

                // INITIALISE VARIABLES
                // =====================

                // Node names
                if (string.IsNullOrEmpty(cMenuItemNodeName))
                    cMenuItemNodeName = "MenuItem";
                if (string.IsNullOrEmpty(cRootNodeName))
                    cRootNodeName = "Menu";
                cCacheType = cRootNodeName + "/" + cMenuItemNodeName;

                // Override the cache if we're not getting menu items
                // If cMenuItemNodeName <> "MenuItem" And cRootNodeName <> "Menu" Then bUseCache = False

                // Project Path modifier
                if (moConfig["ProjectPath"] != null)
                {
                    cFilePathModifier = moConfig["ProjectPath"].TrimEnd('/').TrimStart('/');
                    if (!string.IsNullOrEmpty(cFilePathModifier))
                        cFilePathModifier = "/" + cFilePathModifier;
                }

                // Root Id
                if (nRootId == 0L)
                    nRootId = RootPageId;

                // User Id
                // If AdminMode and no user then set user to be -1 otherwise apply the userid
                if (nUserId == 0L)
                    nUserId = mbAdminMode ? -1L : mnUserId;
                if (nUserId == -1)
                    bAuth = false;

                // Set Non-Authenticated user permissions
                // Don't forget to clear authenticated users if the user is not logged on.
                if (gbMembership)
                {
                    if (nUserId == 0L | nUserId == gnNonAuthUsers)
                    {
                        nAuthUsers = 0L;
                        if (nUserId == 0L & gnNonAuthUsers != 0)
                        {
                            nUserId = gnNonAuthUsers;
                            bAuth = false;
                        }
                    }
                }
                else
                {
                    bAuth = false;
                }

                // CACHE MODE
                // ===========
                // Work out whether or not to use site structure caching
                // Only check caching if user is logged on and is not admin mode, and caching has been turned on

                // If we are indexing from SOAP we have no session object therefore we don't use cache
                if (moSession is null)
                    bUseCache = false;
                // If we are in admin mode we don't use cache
                // If mbAdminMode Then bUseCache = False

                // Site caching can be turned on through a site request - this will only work for sites with membership
                // OR if site caching is turned on and membership is off, then save a single site structure for all users (i.e. don;t use session)
                if (bUseCache)
                {
                    sProcessInfo = "GetStructureXML-CheckCaching";
                    PerfMon.Log("Web", sProcessInfo);

                    if (moSession["cacheMode"] != null && !string.IsNullOrEmpty(moSession["cacheMode"].ToString()))
                    {
                        cCacheMode = moSession["cacheMode"].ToString();
                    }
                    else if (!string.IsNullOrEmpty(moConfig["SiteCache"]))
                    {
                        cCacheMode = gbSiteCacheMode ? "on" : "off";
                    }

                    // Check out of a caching override has been passed through
                    if (!string.IsNullOrEmpty(moRequest["cacheMode"]))
                    {
                        cCacheMode = moRequest["cacheMode"].ToLower();
                    }

                    // Store the cache to the session
                    moSession["cacheMode"] = cCacheMode;

                    // If Cache is on then check for a cached strcture
                    if (cCacheMode == "on")
                    {

                        var oCache = moPageXml.CreateElement(cRootNodeName);

                        if (mbAdminMode & cCacheType == "Menu/MenuItem")
                        {
                            oCache.InnerXml = Convert.ToString(moCtx.Application["AdminStructureCache"]);
                        }
                        else
                        {


                            string cacheSearchCriteria = $" WHERE nCacheDirId = {SqlFmt(nUserId.ToString())} AND cCacheType='{cCacheType}'";
                            if (bAuth)
                            {
                                cacheSearchCriteria += " AND cCacheSessionID = '" + moSession.SessionID + "' AND DATEDIFF(hh,dCacheDate,GETDATE()) > 12";
                            }
                            sProcessInfo = "GetStructureXML-SelectFromCache";
                            PerfMon.Log("Web", sProcessInfo);
                            // Get the cached structure - returns empty string if no structure found.
                            sSql = "SELECT TOP 1 cCacheStructure FROM dbo.tblXmlCache " + cacheSearchCriteria;

                            sProcessInfo = "GetStructureXML-getCachefromDB-Start";
                            PerfMon.Log("Web", sProcessInfo);

                            moDbHelper.AddXMLValueToNode(sSql, ref oCache);

                            sProcessInfo = "GetStructureXML-getCachefromDB-End";
                            PerfMon.Log("Web", sProcessInfo);


                        }

                        if (cRootNodeName != "Menu")
                        {
                            foreach (XmlElement currentOElmt in oCache.SelectNodes("descendant-or-self::Menu"))
                            {
                                oElmt = currentOElmt;
                                Tools.Xml.renameNode(ref oElmt, cRootNodeName);
                            }
                        }

                        if (cMenuItemNodeName != "MenuItem")
                        {
                            foreach (XmlElement currentOElmt1 in oCache.SelectNodes("descendant-or-self::MenuItem"))
                            {
                                oElmt = currentOElmt1;
                                Tools.Xml.renameNode(ref oElmt, cMenuItemNodeName);
                            }
                        }



                        if (oCache.FirstChild != null)
                        {
                            bCacheXml = true;
                            oElmt = oCache;
                        }


                    }

                }

                // If we don't have a cache to check then get a new structure
                if (bCacheXml == false | cCacheMode == "off")
                {

                    // DATA CALL : GET STRUCTURE
                    // =========================
                    // 
                    // This will return structure nodes, that will optionally have the following:
                    // - checks for page level permissions (indicated by nUserId not being -1) - if these are returned they will need to cleaned up.
                    // - enumerate who teh permissions have come from (indicated by nUserId not being -1 and badminMode being 1)
                    // - exclude expired, not yet published and hidden pages if not in adminmode.

                    // If preview mode is set to show hidden
                    bool spoofAdminMode = mbAdminMode;
                    if (mbPreviewHidden)
                    {
                        bIncludeExpiredAndHidden = true;
                    }

                    sSql = "EXEC getContentStructure_v2 @userId=" + nUserId + ", @bAdminMode=" + Convert.ToInt16(mbAdminMode) + ", @dateNow=" + sqlDate(mdDate) + ", @authUsersGrp = " + nAuthUsers + ", @bReturnDenied=1";

                    sProcessInfo = "GetStructureXML-getContentStrcuture";
                    PerfMon.Log("Web", sProcessInfo);

                    if (bIncludeExpiredAndHidden)
                        sSql += ",@bShowAll=1";

                    // Get the dataset
                    oDs = moDbHelper.GetDataSet(sSql, cMenuItemNodeName, cRootNodeName);

                    // Add Page Version Info
                    if (Features.ContainsKey("PageVersions"))
                    {
                        if (mbAdminMode & nUserId == -1)
                        {
                            // check we are returning strucutre for current user and not for another user such as in bespoke report (PDP for LMS system).
                            sSql = "EXEC getAllPageVersions";
                        }
                        else
                        {
                            sSql = "EXEC getUserPageVersions @userId=" + nUserId + ", @dateNow=" + sqlDate(mdDate) + ", @authUsersGrp = " + nAuthUsers + ", @bReturnDenied=0, @bShowAll=0";
                        }

                        sProcessInfo = "GetStructureXML-getPageVersions";
                        PerfMon.Log("Web", sProcessInfo);

                        moDbHelper.addTableToDataSet(ref oDs, sSql, "PageVersion");
                        oDs.Relations.Add("rel02", oDs.Tables[0].Columns["id"], oDs.Tables[1].Columns["vParId"], false);
                        oDs.Relations["rel02"].Nested = true;

                        if (oDs.Tables[1].Rows.Count > 0)
                        {
                            oDs.Tables[1].Columns["id"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["parId"].ColumnMapping = MappingType.Hidden;
                            oDs.Tables[1].Columns["name"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["url"].ColumnMapping = MappingType.Attribute;
                            // oDs.Tables(1).Columns("Description").ColumnMapping = Data.MappingType.SimpleContent
                            oDs.Tables[1].Columns["publish"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["expire"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["status"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["access"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["layout"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["clone"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["vParId"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["lang"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["desc"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[1].Columns["verType"].ColumnMapping = MappingType.Attribute;
                        }

                    }

                    // Add nestings
                    oDs.Relations.Add("rel01", oDs.Tables[0].Columns["id"], oDs.Tables[0].Columns["parId"], false);
                    oDs.Relations["rel01"].Nested = true;

                    if (oDs.Tables[0].Rows.Count > 0)
                    {

                        // COLUMN MAPPING - STANDARD
                        // =========================
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns["url"].ColumnMapping = MappingType.Attribute;
                        if (mbAdminMode)
                        {
                            oDs.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns["publish"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns["expire"].ColumnMapping = MappingType.Attribute;
                        }

                        // COLUMN MAPPING - ACCESS
                        // =========================
                        oDs.Tables[0].Columns["access"].ColumnMapping = MappingType.Attribute;
                        if (oDs.Tables[0].Columns.Contains("accessSource"))
                        {
                            oDs.Tables[0].Columns["accessSource"].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns["accessSourceId"].ColumnMapping = MappingType.Attribute;
                        }

                        if (oDs.Tables[0].Columns.Contains("vParId"))
                        {
                            oDs.Tables[0].Columns["vParId"].ColumnMapping = MappingType.Attribute;
                        }

                        if (oDs.Tables[0].Columns.Contains("ref"))
                        {
                            oDs.Tables[0].Columns["ref"].ColumnMapping = MappingType.Attribute;
                        }

                        // COLUMN MAPPING - CLONES
                        // =========================
                        if (oDs.Tables[0].Columns.Contains("clone"))
                            oDs.Tables[0].Columns["clone"].ColumnMapping = MappingType.Attribute;


                        // COVERT DATASET TO XML
                        // =====================
                        oElmt = moPageXml.CreateElement(cRootNodeName);

                        sProcessInfo = "GetStructureXML-dsToXml";
                        PerfMon.Log("Web", sProcessInfo);

                        // TS added lines to avoid whitespace issues
                        var oXml = new XmlDocument();
                        oXml.LoadXml(oDs.GetXml());
                        oXml.PreserveWhitespace = false;

                        oElmt.InnerXml = oXml.DocumentElement.OuterXml;



                        // MENU TIDY UP
                        // ==================
                        // 'Rename the VersionMenuNodes
                        // If mbAdminMode And Features.ContainsKey("PageVersions") Then
                        // sProcessInfo = "GetStructureXML-RenameVersions"
                        // PerfMon.Log("Web", sProcessInfo)
                        // Dim oVersionMenuItems As XmlNodeList = oElmt.SelectNodes(cRootNodeName & "/" & cMenuItemNodeName & "[@vParId!='']")
                        // For Each oVerMenuItem As XmlElement In oVersionMenuItems
                        // Tools.Xml.renameNode(oVerMenuItem, "PageVersion")
                        // Next
                        // End If

                        // REMOVE THE ORPHANS
                        // ==================
                        // Because the SQL may not return DENIED pages, we need to check for orphaned items
                        // Orphaned records are ones that have no parent menu node (because it wasn't returned by the SQL)
                        // By default these will be returned to the root node, which will also include the root page node - which we don't want to delete.
                        // The genuine root node will be node with a parId of 0 (possibly also including System Pages)

                        sProcessInfo = "GetStructureXML-CleanOrphans";
                        PerfMon.Log("Web", sProcessInfo);

                        var oRootMenuItems = oElmt.SelectNodes(cRootNodeName + "/" + cMenuItemNodeName + "[@parId!=0]");
                        foreach (XmlNode oRootMenuItem in oRootMenuItems)
                            oRootMenuItem.ParentNode.RemoveChild(oRootMenuItem);



                        // DETERMINE THE ROOT NODE ID
                        // ==========================
                        // We need to determine the root id
                        // This allows us to not do unnecessary cloning
                        // If a clonecontextnode has been passed, then we need to get that for now

                        sProcessInfo = "GetStructureXML-GetIndicativeRootId";
                        PerfMon.Log("Web", sProcessInfo);

                        if (nCloneContextId > 0L)
                        {
                            nTempRootId = (int)nCloneContextId;
                        }
                        else if (nRootId > 0L)
                        {
                            nTempRootId = (int)nRootId;
                        }
                        else
                        {
                            nTempRootId = Convert.ToInt16(oElmt.FirstChild.FirstChild.Attributes["id"].Value);
                        }



                        // GET CLONED NODES
                        // ==================
                        // Note - we only want to find cloned nodes under the root id in question
                        if (gbClone)
                        {

                            PerfMon.Log("Web", "GetStructureXML-cloneNodes");
                            var cNodeSnapshot = new Hashtable();

                            // GET CLONE SNAPSHOT
                            // Why snapshot?  It stops cloned pages that also may have parent nodes cloned later changing through this process.
                            foreach (XmlElement currentOMenuItem in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName + "[@id='" + nTempRootId + "']/descendant-or-self::" + cMenuItemNodeName + "[@clone and not(@clone=0)]"))
                            {
                                oMenuItem = currentOMenuItem;
                                // Go and get the cloned node
                                nCloneId = Convert.ToInt16(oMenuItem.GetAttribute("clone"));
                                if (!(Tools.Xml.NodeState(ref oElmt, "descendant-or-self::" + cMenuItemNodeName + "[@id='" + nCloneId + "']", ref oClone) == Tools.Xml.XmlNodeState.NotInstantiated))
                                {
                                    if (oClone != null)
                                    {
                                        if (!cNodeSnapshot.ContainsKey(nCloneId))
                                            cNodeSnapshot.Add(nCloneId, oClone.InnerXml);
                                    }

                                }
                            }

                            // ADD CLONE SNAPSHOTS TO MENU
                            // Run through the nodes again, this time processing them 
                            foreach (XmlElement currentOMenuItem1 in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName + "[@id='" + nTempRootId + "']/descendant-or-self::" + cMenuItemNodeName + "[@clone and not(@clone=0)]"))
                            {
                                oMenuItem = currentOMenuItem1;

                                // Go and get the cloned node
                                nCloneId = Convert.ToInt16(oMenuItem.GetAttribute("clone"));
                                nCloneParentId = Convert.ToInt16(oMenuItem.GetAttribute("id"));
                                if (!(Tools.Xml.NodeState(ref oElmt, "descendant-or-self::" + cMenuItemNodeName + "[@id='" + nCloneId + "']", "", "", XmlNodeState.IsEmpty, oClone, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) == Tools.Xml.XmlNodeState.NotInstantiated))
                                {

                                    oMenuItem.InnerXml = Convert.ToString(cNodeSnapshot[nCloneId]);

                                    // Add the cloned flag to nodes and children
                                    foreach (XmlElement oChild in oMenuItem.SelectNodes("descendant-or-self::" + cMenuItemNodeName))
                                        oChild.SetAttribute("cloneparent", nCloneParentId.ToString());
                                }
                            }

                        }


                        // PROPOGATE THE PERMISSIONS AND PRUNE
                        // ===================================
                        sProcessInfo = "GetStructureXML-TidyMenunode";
                        PerfMon.Log("Web", sProcessInfo);
                        XmlElement argoMenuItem = (XmlElement)oElmt.FirstChild;
                        TidyMenunode(ref argoMenuItem, "OPEN", "", bPruneEvenIfInAdminMode | !mbAdminMode, nUserId, cMenuItemNodeName, cRootNodeName);

                        foreach (XmlElement currentOMenuItem2 in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName + " | descendant-or-self::PageVersion"))
                        {
                            oMenuItem = currentOMenuItem2;
                            // For Each oMenuItem In oElmt.SelectNodes("descendant-or-self::*")
                            XmlElement oDesc = (XmlElement)oMenuItem.SelectSingleNode("Description");
                            if (oDesc != null)
                            {

                                // First try to convert the node into Xml
                                sContent = oDesc.InnerText + "";
                                try
                                {
                                    oDesc.InnerXml = sContent;
                                }
                                catch
                                {
                                    oDesc.InnerText = sContent;
                                }

                                // we have a historic error where some sites have 2 description nodes in some database
                                // This should be data cleansed but for now we hack

                                // Work through all the child nodes of Description and move them to the MenuItem
                                foreach (XmlElement oDescChild in oDesc.SelectNodes("*[name()!='Description' or (name()='Description' and not(preceding-sibling::Description))]"))
                                    // Tidy the node
                                    // sContent = oDescChild.InnerText
                                    // If sContent <> "" Then
                                    // Try
                                    // oDescChild.InnerXml = Protean.Tools.Xml.convertEntitiesToCodes(sContent)
                                    // Catch
                                    // PerfMon.Log("Web", "GetStructureXML-tidy")
                                    // oDescChild.InnerXml = tidyXhtmlFrag(sContent)
                                    // End Try
                                    // End If

                                    // Move the node
                                    oMenuItem.InsertBefore(oDescChild.CloneNode(true), oDesc);

                                // Remove the original oDesc
                                oMenuItem.RemoveChild(oDesc);

                            }

                            // Remove the parId attribute
                            oMenuItem.RemoveAttribute("parId");

                        }

                        if (bLockRoot)
                        {
                            // LOCK THE ROOT
                            // ===============
                            sProcessInfo = "GetStructureXML-lockRoot";
                            PerfMon.Log("Web", sProcessInfo);
                            XmlElement oMenuFirstChild = (XmlElement)oElmt.FirstChild;
                            oMenuFirstChild.SetAttribute("Locked", Convert.ToString(true));
                        }
                    }

                    else
                    {

                        // NO ROWS WERE FOUND - CREATE AN EMPTY Menu NODE
                        // ===============================================
                        oElmt = moPageXml.CreateElement(cRootNodeName);

                    }

                    // CACHING - ADD THE STRUCTURE TO THE CACHE
                    // ========================================
                    // Only if caching is on, user is logged on, and not in AdminMode.
                    if (bUseCache & cCacheMode == "on")
                    {
                        sProcessInfo = "GetStructureXML-addCacheToStructure";
                        PerfMon.Log("Web", sProcessInfo);
                        // ts this was commented out I have restored 04/11/2022 please leave not to say why commented next time
                        if (moRequest["reBundle"] != null)
                        {
                            moDbHelper.clearStructureCacheAll();
                        }
                        // only cache if MenuItem / Menu
                        if (cMenuItemNodeName == "MenuItem" & cRootNodeName == "Menu")
                        {
                            if (mbAdminMode)
                            {
                                moCtx.Application["AdminStructureCache"] = oElmt.InnerXml;
                            }
                            else
                            {
                                XmlElement argStructureXml = (XmlElement)oElmt.FirstChild;
                                moDbHelper.addStructureCache(bAuth, nUserId, ref cCacheType, ref argStructureXml);
                            }
                        }
                        else
                        {
                            XmlElement argStructureXml1 = (XmlElement)oElmt.FirstChild;
                            moDbHelper.addStructureCache(bAuth, nUserId, ref cCacheType, ref argStructureXml1);

                        }


                        // sSql = "INSERT INTO dbo.tblXmlCache (cCacheSessionID,nCacheDirId,cCacheStructure,cCacheType) " _
                        // & "VALUES (" _
                        // & "'" & IIf(bAuth, Eonic.SqlFmt(moSession.SessionID), "") & "'," _
                        // & Eonic.SqlFmt(nUserId) & "," _
                        // & "'" & Eonic.SqlFmt(oElmt.InnerXml) & "'," _
                        // & "'" & cCacheType & "'" _
                        // & ")"
                        // moDbHelper.ExeProcessSql(sSql)

                    }
                }

                // Now we need to do some page dependant processing

                // MENU TIDY: XML TIDY
                // ===================================
                sProcessInfo = "GetStructureXML-txt2xml";
                PerfMon.Log("Web", sProcessInfo);
                string sUrl;
                string cPageName;
                string cCloneParent;
                var oRe = new Regex("[^A-Z0-9]", RegexOptions.IgnoreCase);
                XmlElement oPageVerElmts;
                // string DomainURL = (mbIsUsingHTTPS ? "https://" : "http://") + moRequest.ServerVariables["SERVER_NAME"];
                string DomainURL = mcRequestDomain;
                string ExcludeFoldersFromPaths = ("" + moConfig["ExcludeFoldersFromPaths"]).ToLower();
                string[] foldersExcludedFromPaths = ExcludeFoldersFromPaths.Split(',');

                foreach (XmlElement currentOMenuItem3 in oElmt.SelectNodes("descendant-or-self::" + cMenuItemNodeName))
                {
                    oMenuItem = currentOMenuItem3;
                    string urlPrefix = "";
                    XmlElement verNodeLoop = null;
                    XmlElement verNode = null;

                    // Determine Page Version
                    if (Features.ContainsKey("PageVersions"))
                    {
                        if (!mbAdminMode | moRequest["ewCmd"] == "Normal" | moRequest["ewCmd"] == "EditContent")
                        {

                            // check for language version
                            foreach (XmlElement currentVerNodeLoop in oMenuItem.SelectNodes("PageVersion[@lang='" + gcLang + "']"))
                            {
                                verNodeLoop = currentVerNodeLoop;
                                verNode = verNodeLoop;
                                urlPrefix = mcPageLanguageUrlPrefix;
                            }

                            // check for permission version
                            foreach (XmlElement currentVerNodeLoop1 in oMenuItem.SelectNodes("PageVersion[@verType='1']"))
                            {
                                verNodeLoop = currentVerNodeLoop1;
                                if (verNode is null)
                                    verNode = verNodeLoop;
                            }

                            // update our pageId
                            if (verNode != null)
                            {
                                // Case for if our version is also the root page
                                if (nRootId == Convert.ToDouble(oMenuItem.GetAttribute("id")))
                                {
                                    nRootId = Convert.ToInt64(verNode.GetAttribute("id"));
                                }

                                // Case if we are on the current page then we reset the mnPageId so we pull in the right content
                                if ((double)mnPageId == Convert.ToDouble(oMenuItem.GetAttribute("id")))
                                {
                                    // If (verNode.GetAttribute("lang") = gcLang Or gcLang = "" Or verNode.GetAttribute("lang") = "") And verNode.GetAttribute("verType") <> 1 Then
                                    // If (verNode.GetAttribute("lang") = gcLang Or gcLang = "" Or verNode.GetAttribute("lang") = "") Then
                                    switch (Convert.ToInt16(verNode.GetAttribute("verType")))
                                    {
                                        case 1: // case for permission version
                                            {
                                                // Dim permLevel As dbHelper.PermissionLevel = moDbHelper.getPagePermissionLevel(verNode.GetAttribute("id"))
                                                if (!mbAdminMode)
                                                {
                                                    mnPageId = Convert.ToInt16(verNode.GetAttribute("id"));
                                                }

                                                break;
                                            }
                                        case 3: // case for language version
                                            {
                                                if ((verNode.GetAttribute("lang") ?? "") == (gcLang ?? ""))
                                                {
                                                    mnPageId = Convert.ToInt16(verNode.GetAttribute("id"));
                                                }

                                                break;
                                            }

                                        default:
                                            {
                                                mnPageId = Convert.ToInt16(verNode.GetAttribute("id"));
                                                break;
                                            }
                                    }
                                }

                                // create a version for the default we are replacing
                                var newVerNode = moPageXml.CreateElement("PageVersion");
                                newVerNode.SetAttribute("id", oMenuItem.GetAttribute("id"));
                                newVerNode.SetAttribute("name", oMenuItem.GetAttribute("name"));
                                newVerNode.SetAttribute("url", oMenuItem.GetAttribute("url"));
                                newVerNode.SetAttribute("publish", oMenuItem.GetAttribute("publish"));
                                newVerNode.SetAttribute("expire", oMenuItem.GetAttribute("expire"));
                                newVerNode.SetAttribute("status", oMenuItem.GetAttribute("status"));
                                newVerNode.SetAttribute("access", oMenuItem.GetAttribute("access"));
                                newVerNode.SetAttribute("layout", oMenuItem.GetAttribute("layout"));
                                string sInnerXml = string.Empty;
                                foreach (XmlElement infoElmt in oMenuItem.SelectNodes("*[name()!='PageVersion' and name()!='MenuItem']"))
                                    sInnerXml = sInnerXml + infoElmt.OuterXml;
                                newVerNode.InnerXml = sInnerXml;
                                sInnerXml = "";
                                if (goLangConfig != null)
                                {
                                    newVerNode.SetAttribute("lang", goLangConfig.GetAttribute("code"));
                                }

                                newVerNode.SetAttribute("verType", "0");
                                oMenuItem.AppendChild(newVerNode);

                                // now replace the menuitem with the node we are on
                                oMenuItem.SetAttribute("id", verNode.GetAttribute("id"));
                                oMenuItem.SetAttribute("name", verNode.GetAttribute("name"));
                                oMenuItem.SetAttribute("url", verNode.GetAttribute("url"));
                                oMenuItem.SetAttribute("publish", verNode.GetAttribute("publish"));
                                oMenuItem.SetAttribute("expire", verNode.GetAttribute("expire"));
                                oMenuItem.SetAttribute("status", verNode.GetAttribute("status"));
                                oMenuItem.SetAttribute("access", verNode.GetAttribute("access"));
                                oMenuItem.SetAttribute("layout", verNode.GetAttribute("layout"));
                                oMenuItem.SetAttribute("clone", verNode.GetAttribute("clone"));
                                oMenuItem.SetAttribute("lang", verNode.GetAttribute("lang"));
                                oMenuItem.SetAttribute("verDesc", verNode.GetAttribute("verDesc"));
                                oMenuItem.SetAttribute("verType", verNode.GetAttribute("verType"));




                                foreach (XmlElement currentOPageVerElmts in verNode.SelectNodes("*"))
                                {
                                    oPageVerElmts = currentOPageVerElmts;
                                    string nodeName = oPageVerElmts.Name;
                                    if (oMenuItem.SelectSingleNode(nodeName) != null)
                                    {
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml;
                                    }
                                    else
                                    {
                                        oMenuItem.AppendChild(moPageXml.CreateElement(nodeName));
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml;
                                    }
                                }
                                // If Not oMenuItem.SelectSingleNode("Description") Is Nothing Then
                                // oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                // Else
                                // oMenuItem.AppendChild(moPageXml.CreateElement("Description"))
                                // oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                // End If
                            }
                        }
                        else if (moRequest["ewCmd"] != "LocateContent" & moRequest["ewCmd"] != "MoveContent" & moRequest["ewCmd"] != "EditStructure")
                        {
                            foreach (XmlElement currentVerNode in oMenuItem.SelectNodes("PageVersion[@id=" + mnPageId + "]"))
                            {
                                verNode = currentVerNode;
                                if (Convert.ToDouble(oMenuItem.GetAttribute("id")) == nRootId)
                                {
                                    // case for replacing homepage in admin
                                    nRootId = Convert.ToInt64(verNode.GetAttribute("id"));
                                }
                                // update menu item with current page
                                oMenuItem.SetAttribute("id", verNode.GetAttribute("id"));
                                oMenuItem.SetAttribute("name", verNode.GetAttribute("name"));
                                oMenuItem.SetAttribute("url", verNode.GetAttribute("url"));
                                oMenuItem.SetAttribute("publish", verNode.GetAttribute("publish"));
                                oMenuItem.SetAttribute("expire", verNode.GetAttribute("expire"));
                                oMenuItem.SetAttribute("status", verNode.GetAttribute("status"));
                                oMenuItem.SetAttribute("access", verNode.GetAttribute("access"));
                                oMenuItem.SetAttribute("layout", verNode.GetAttribute("layout"));
                                oMenuItem.SetAttribute("clone", string.IsNullOrEmpty(verNode.GetAttribute("clone")) ? "0" : verNode.GetAttribute("clone"));
                                oMenuItem.SetAttribute("lang", verNode.GetAttribute("lang"));
                                oMenuItem.SetAttribute("verDesc", verNode.GetAttribute("desc"));
                                oMenuItem.SetAttribute("verType", verNode.GetAttribute("verType"));

                                foreach (XmlElement currentOPageVerElmts1 in verNode.SelectNodes("*"))
                                {
                                    oPageVerElmts = currentOPageVerElmts1;
                                    string nodeName = oPageVerElmts.Name;
                                    if (oMenuItem.SelectSingleNode(nodeName) != null)
                                    {
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml;
                                    }
                                    else
                                    {
                                        oMenuItem.AppendChild(moPageXml.CreateElement(nodeName));
                                        oMenuItem.SelectSingleNode(nodeName).InnerXml = verNode.SelectSingleNode(nodeName).InnerXml;
                                    }
                                }
                                // If Not oMenuItem.SelectSingleNode("Description") Is Nothing Then
                                // oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                // Else
                                // oMenuItem.AppendChild(moPageXml.CreateElement("Description"))
                                // oMenuItem.SelectSingleNode("Description").InnerText = verNode.SelectSingleNode("Description").InnerText
                                // End If
                            }
                            verNode = null;

                        }
                    }

                    if (goLangConfig != null)
                    {

                        if (verNode is null & goLangConfig.GetAttribute("localDefaults") != "off")
                        {
                            urlPrefix = mcPageLanguageUrlPrefix; // & cFilePathModifier
                        }

                    }



                    // Only generate URLs for MneuItems that do not already have a url explicitly defined
                    if (string.IsNullOrEmpty(oMenuItem.GetAttribute("url")))
                    {

                        // Start with the base path
                        sUrl = moConfig["BasePath"] + urlPrefix + cFilePathModifier;

                        if (moConfig["UsePageIdsForURLs"] == "on")
                        {
                            // Use the page ID instead of a Pretty URL
                            sUrl = sUrl + "/?pgid=" + oMenuItem.GetAttribute("id");
                        }
                        else
                        {
                            // Get all the descendant menuitems and append the names onto the Url string
                            foreach (XmlElement oDescendant in oMenuItem.SelectNodes("ancestor-or-self::" + cMenuItemNodeName + "[ancestor::MenuItem[@id=" + nRootId + "]]"))
                            {
                                if (!(oDescendant.ParentNode.Name == "Menu"))
                                {
                                    if (moConfig["PageURLFormat"] == "hyphens")
                                    {
                                        cPageName = oRe.Replace(oDescendant.GetAttribute("name"), "-");
                                    }
                                    else
                                    {
                                        cPageName = goServer.UrlEncode(oDescendant.GetAttribute("name"));
                                    }

                                    sUrl = sUrl + "/" + cPageName;

                                }
                            }
                        }

                        if (moConfig["TrailingSlash"] == "on")
                        {
                            sUrl = "/" + sUrl.Trim('/') + "/";
                        }

                        // Account for a root url
                        if (string.IsNullOrEmpty(sUrl))
                        {
                            sUrl = "/";
                        }

                        if (sUrl == "//")
                        {
                            sUrl = "/";
                        }

                        if (sUrl == "/")
                        {
                            sUrl = DomainURL;
                            if (moRequest.ServerVariables["SERVER_PORT"] != "80" & moRequest.ServerVariables["SERVER_PORT"] != "443")
                            {
                                sUrl = sUrl + ":" + moRequest.ServerVariables["SERVER_PORT"];
                            }
                        }
                        if (moConfig["LowerCaseUrl"] == "on")
                        {
                            sUrl = sUrl.ToLower();
                        }
                        // for admin mode we tag the pgid on the end to be safe for duplicate pagenames with different permissions.
                        if (mbAdminMode & string.IsNullOrEmpty(moConfig["pageExt"]) & moConfig["UsePageIdsForURLs"] != "on")


                            sUrl = sUrl + "?pgid=" + oMenuItem.GetAttribute("id");



                        if (moConfig["LowerCaseUrl"] == "on")
                        {
                            sUrl = sUrl.ToLower();
                        }

                        oMenuItem.SetAttribute("url", sUrl);

                        // If oMenuItem.GetAttribute("id") = "609" Then
                        // mbIgnorePath = mbIgnorePath
                        // If

                        if (!mbIgnorePath)
                        {
                            if (moRequest.QueryString.Count > 0)
                            {
                                if (moRequest["path"] != null)
                                {
                                    // If this matches the path requested then change the pageId
                                    if (!string.IsNullOrEmpty(sUrl))
                                    {
                                        string PathToMatch;
                                        //string PathToMatch = sUrl.Replace(DomainURL, "").ToLower();
                                        if (!string.IsNullOrEmpty(DomainURL))
                                        {
                                            PathToMatch = sUrl.Replace(DomainURL, "").ToLower();
                                        }
                                        else
                                        {
                                            PathToMatch = sUrl.ToLower(); // fallback
                                        }
                                        string PathToMatch2 = "/" + gcLang + PathToMatch;
                                        string PathToTest = moRequest["path"].ToLower().TrimEnd('/');
                                        if ((PathToMatch ?? "") == (PathToTest ?? "") | (PathToMatch2 ?? "") == (PathToTest ?? ""))
                                        {
                                            if (oMenuItem.SelectSingleNode("ancestor-or-self::MenuItem[@id=" + nRootId + "]") != null)
                                            {
                                                // case for if newsletter has same page name as menu item
                                                if (Features.ContainsKey("PageVersions"))
                                                {
                                                    // catch for page version
                                                    if (oMenuItem.SelectSingleNode("PageVersion[@id='" + mnPageId + "']") is null)
                                                    {
                                                        mnPageId = Convert.ToInt16(oMenuItem.GetAttribute("id"));
                                                    }
                                                }
                                                else
                                                {
                                                    mnPageId = Convert.ToInt16(oMenuItem.GetAttribute("id"));
                                                }

                                                if (mnUserId != 0 | mbAdminMode != true)
                                                {
                                                    // case for personalisation and admin TS 14/02/2021
                                                    mnPageId = Convert.ToInt16(oMenuItem.GetAttribute("id"));
                                                }
                                                // If oMenuItem.GetAttribute("verType") = "3" Then
                                                mnClonePageVersionId = mnPageId;
                                                // this is used in clone mode to determine the page content in GetPageContent.
                                                // End If
                                                oMenuItem.SetAttribute("requestedPage", "1");
                                            }

                                        }
                                    }
                                }
                            }
                        }
                        // set the URL for each language pageversion so we can link between

                        // Address the context of the page
                        if (gbClone)
                        {
                            cCloneParent = oMenuItem.GetAttribute("cloneparent");
                            if (Tools.Number.IsNumeric(cCloneParent) && Convert.ToInt16(cCloneParent) > 0)
                            {
                                sUrl = sUrl + (sUrl.Contains("?") ? "&" : "?");
                                sUrl += "context=" + cCloneParent;
                            }
                        }

                        foreach (XmlElement pvElmt in oMenuItem.SelectNodes("PageVersion"))
                        {
                            string pageLang = pvElmt.GetAttribute("lang");
                            if (!string.IsNullOrEmpty(pageLang))
                            {
                                sUrl = "";
                                foreach (XmlElement oDescendant in oMenuItem.SelectNodes("ancestor-or-self::" + cMenuItemNodeName))
                                {
                                    if (!(oDescendant.ParentNode.Name == "Menu"))
                                    {
                                        cPageName = null;
                                        foreach (XmlElement parPvElmt in oDescendant.SelectNodes("PageVersion[@lang='" + pageLang + "']"))
                                        {
                                            if (moConfig["PageURLFormat"] == "hyphens")
                                            {
                                                cPageName = oRe.Replace(parPvElmt.GetAttribute("name"), "-");
                                            }
                                            else
                                            {
                                                cPageName = goServer.UrlEncode(parPvElmt.GetAttribute("name"));
                                            }
                                            // I know this means we get the last one but we should only have one anyway.
                                        }
                                        if (cPageName is null)
                                        {
                                            if (moConfig["PageURLFormat"] == "hyphens")
                                            {
                                                cPageName = oRe.Replace(oDescendant.GetAttribute("name"), "-");
                                            }
                                            else
                                            {
                                                cPageName = goServer.UrlEncode(oDescendant.GetAttribute("name"));
                                            }
                                        }
                                        sUrl = sUrl + "/" + cPageName;
                                        if (moConfig["LowerCaseUrl"] == "on")
                                        {
                                            sUrl = sUrl.ToLower();
                                        }
                                    }
                                }

                                string pvUrlPrefix = "";
                                // Check Language by Domain
                                if (goLangConfig != null)
                                {
                                    string httpStart;
                                    if (moRequest.ServerVariables["SERVER_PORT_SECURE"] == "1")
                                    {
                                        httpStart = "https://";
                                    }
                                    else
                                    {
                                        httpStart = "http://";
                                    }

                                    if (goLangConfig.SelectSingleNode("Language[@code='" + pageLang + "']") != null)
                                    {
                                        foreach (XmlElement oLangElmt in goLangConfig.SelectNodes("Language[@code='" + pageLang + "']"))
                                        {
                                            switch (oLangElmt.GetAttribute("identMethod")?.ToLower() ?? "")
                                            {
                                                case "domain":
                                                    {
                                                        pvUrlPrefix = httpStart + oLangElmt.GetAttribute("identifier");
                                                        break;
                                                    }
                                                case "path":
                                                    {
                                                        pvUrlPrefix = httpStart + goLangConfig.GetAttribute("defaultDomain") + "/" + oLangElmt.GetAttribute("identifier");
                                                        break;
                                                    }
                                            }
                                        }
                                    }

                                    if (string.IsNullOrEmpty(pvUrlPrefix))
                                    {
                                        if (string.IsNullOrEmpty(goLangConfig.GetAttribute("defaultDomain")))
                                        {
                                            pvUrlPrefix = "/";
                                        }
                                        else
                                            pvUrlPrefix = httpStart + goLangConfig.GetAttribute("defaultDomain");
                                    }



                                    pvElmt.SetAttribute("url", pvUrlPrefix + sUrl);

                                }


                            }


                        }

                        if ((double)mnPageId == Convert.ToDouble(oMenuItem.GetAttribute("id")))
                        {
                            mcPageURL = sUrl;
                        }
                    }
                }



                // GET THE ROOT NODE
                // ==================
                // get the Menu from the site root.
                PerfMon.Log("Web", "GetStructureXML-rootnode");
                if (nRootId > 0L)
                {
                    string cCloneModifier = "";
                    if (nCloneContextId > 0L)
                    {
                        cCloneModifier = " and @cloneparent='" + nCloneContextId + "'";
                    }

                    XmlElement oRoot = (XmlElement)oElmt.SelectSingleNode("descendant-or-self::" + cMenuItemNodeName + "[@id='" + nRootId + "'" + cCloneModifier + "]");
                    if (oRoot is null)
                    {
                    }
                    // Return error for when root page id does not exist.
                    // Dim cErrorMsg As String = "EonicWeb Config Error: The root page id does not exist, it may be hidden."
                    // cErrorMsg += " Root Id: " & nRootId & ";"
                    // cErrorMsg += " Top Level Id: " & RootPageId & ";"
                    // cErrorMsg += " User Id: " & nUserId & ";"
                    // cErrorMsg += " Clone Context Id: " & nCloneContextId & ";"
                    // cErrorMsg += " Menu Name: " & cMenuId & ";"
                    // Err.Raise(1001, "GetStructure", cErrorMsg)
                    else
                    {
                        oElmt.InnerXml = oRoot.OuterXml;
                    }
                }
                else
                {
                    oElmt.InnerXml = oElmt.FirstChild.FirstChild.OuterXml;
                }

                // MENU - ADD ID ATTRIBUTE
                // ========================
                if (!string.IsNullOrEmpty(cMenuId))
                {
                    oElmt.SetAttribute("id", cMenuId);
                }

                // MENU - ADD THE MENU TO THE PAGE XML
                // ===================================
                if (bAddMenuToPageXML && moPageXml.DocumentElement != null)
                {
                    sProcessInfo = "GetStructureXML-addMenuToPageXML";
                    PerfMon.Log("Web", sProcessInfo);
                    // Check if there's already a menu node.
                    if (moPageXml.SelectSingleNode("/Page/" + cRootNodeName) is null)
                    {
                        // No menu node - add it to the pagexml
                        moPageXml.DocumentElement.AppendChild(oElmt);
                    }
                    else
                    {
                        // Menu node found - add it after the last Menu node
                        moPageXml.DocumentElement.InsertAfter(oElmt, moPageXml.SelectSingleNode("/Page/" + cRootNodeName + "[position()=last()]"));
                    }
                }

                sProcessInfo = "GetStructureXML-End";
                PerfMon.Log("Web", sProcessInfo);

                return oElmt;
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, cFunctionDef, ex, sProcessInfo));
                return null;
            }

        }


        public virtual void AddContentCount(XmlElement oMenu, string SchemaType)
        {
            XmlElement oElmt;
            string cSQL;
            try
            {
                cSQL = "select COUNT(nContentKey) as count,nStructId from tblContentLocation cl" + " inner join tblContent c on c.nContentKey = cl.nContentId" + " inner join tblAudit a on c.nAuditId = a.nAuditKey" + " where c.cContentSchemaName = '" + SchemaType.Trim() + "' " + moConfig["MenuContentCountWhere"] + GetStandardFilterSQLForContent(true) + " group by nStructId";


                using (var oDR = moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                {
                    while (oDR.Read())
                    {
                        oElmt = (XmlElement)oMenu.SelectSingleNode($"descendant::MenuItem[@id='{oDR["nStructId"]}']" );
                        if (oElmt != null)
                        {
                            var newElmt = moPageXml.CreateElement("ContentCount");
                            newElmt.SetAttribute("type", SchemaType.Trim());
                            newElmt.SetAttribute("count", Convert.ToString(oDR["count"]));
                            oElmt.AppendChild(newElmt);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddContentCount", ex, ""));
            }

        }

        public virtual void AddContentBrief(XmlElement oMenu, string SchemaType)
        {
            XmlElement oRoot;
            string parId;
            XmlElement ParentMenuElmt;
            try
            {

                oRoot = moPageXml.CreateElement("Contents");

                int argnCount = 0;
                GetMenuContentFromSelect("cContentSchemaName = '" + SchemaType + "'", ref argnCount, oContentsNode: ref oRoot, false, false, 0);

                foreach (XmlElement oElmt in oRoot.SelectNodes("*"))
                {
                    parId = oElmt.GetAttribute("locId");
                    ParentMenuElmt = (XmlElement)oMenu.SelectSingleNode("descendant-or-self::MenuItem[@id='" + parId + "']");
                    if (ParentMenuElmt != null)
                    {
                        ParentMenuElmt.AppendChild(oElmt);
                    }
                }
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddContentCount", ex, ""));
            }

        }

        /// <summary>
        /// Takes a menu node, and removes the denied nodes
        /// 
        /// A note on how permissions are inherited.
        /// If a page is OPEN, then it will inherit the permission.
        /// If a page has a parent permission of DENIED then it will be denied.
        /// </summary>
        /// <param name="oMenuItem"></param>
        /// <param name="cPerm"></param>
        /// <param name="cPermSource"></param>
        /// <param name="bPruneDenied"></param>
        /// <remarks></remarks>
        private void TidyMenunode(ref XmlElement oMenuItem, string cPerm, string cPermSource, bool bPruneDenied, long nUserId, string cMenuItemNodeName, string cRootNodeName)
        {
            string cCurrentPerm;
            string cCurrentPermSource;
            try
            {
                foreach (XmlElement oChild in oMenuItem.SelectNodes(cRootNodeName + " | " + cMenuItemNodeName))
                {
                    if (bPruneDenied && oChild.GetAttribute("access").Contains("DENIED"))
                    {
                        oMenuItem.RemoveChild(oChild);
                    }
                    else
                    {
                        // Work out the child permissions
                        cCurrentPerm = cPerm;
                        cCurrentPermSource = cPermSource;

                        if (cCurrentPerm.Contains("DENIED") | oChild.GetAttribute("access").Contains("OPEN") & !cCurrentPerm.Contains("OPEN"))


                        {

                            // If parent is DENIED,
                            // OR the child page is OPEN but the parent is not
                            // THEN set the child page permission to be the INHERITED parent perm and the source.

                            if (!cCurrentPerm.Contains("INHERITED"))
                                cCurrentPerm = "INHERITED " + cCurrentPerm;
                        }


                        else
                        {
                            // ELSE set perm and source variables to the current ones
                            cCurrentPerm = oChild.GetAttribute("access");

                            if ((oChild.GetAttribute("accessSourceId") ?? "") == (nUserId.ToString() ?? ""))
                            {
                                cCurrentPermSource = "";
                            }

                            else
                            {
                                if (cCurrentPerm == "DENIED")
                                    cCurrentPerm = "IMPLIED " + cCurrentPerm;
                                cCurrentPermSource = oChild.GetAttribute("accessSource");
                                if (!string.IsNullOrEmpty(cCurrentPermSource) & !cCurrentPerm.Contains("DENIED"))
                                    cCurrentPerm += " by " + cCurrentPermSource;
                            }

                        }

                        oChild.SetAttribute("access", cCurrentPerm);

                        oChild.RemoveAttribute("accessSource");
                        oChild.RemoveAttribute("accessSourceId");
                        XmlElement xmloChild = oChild;
                        TidyMenunode(ref xmloChild, cCurrentPerm, cCurrentPermSource, bPruneDenied, nUserId, cMenuItemNodeName, cRootNodeName);
                    }
                }
            }

            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "TidyMenunode", ex, ""));
            }
        }

        public void addPageDetailLinksToStructure(string cContentTypes)
        {
            string cProcessInfo = "addPageDetailLinksToStructure";
            string cIndexDetailSubTypes = "";
            string[] IndexDetailSubTypes;
            try
            {

                if (!string.IsNullOrEmpty(moConfig["SiteSearchIndexDetailSubTypes"]))
                {
                    cIndexDetailSubTypes = moConfig["SiteSearchIndexDetailSubTypes"];
                }
                IndexDetailSubTypes = cIndexDetailSubTypes.Replace(" ", "").Split(',');


                XmlElement oMenuElmt = (XmlElement)moPageXml.DocumentElement.SelectSingleNode("Menu");
                if (oMenuElmt is null)
                    return;

                string[] cContentTypesArray = cContentTypes.Split(',');
                cContentTypes = "";
                foreach (string cContentType in cContentTypesArray)
                    cContentTypes += Tools.Database.SqlString(cContentType.Trim()) + ",";

                cContentTypes = cContentTypes.TrimEnd(',');
                var pageDict = new SortedDictionary<long, string>();
                foreach (XmlElement MenuItem in oMenuElmt.SelectNodes("descendant-or-self::MenuItem"))
                    pageDict.Add(Convert.ToInt64(MenuItem.GetAttribute("id")), MenuItem.GetAttribute("url"));

                // Dim keys As List(Of Long) = pageDict.KeyCollection
                // keys.Sort()

                // AG 19-Jan-2010 - Replaced with above code to add protection against SQL Injections
                // cContentTypes = cContentTypes.Replace(",", "','")
                // cContentTypes = "'" & cContentTypes & "'"
                // cContentTypes = cContentTypes.Replace("''", "'")


                //string sProcessInfo = "addPageDetailLinksToStructure";
                string cSQL = "SELECT tblContent.nContentKey, tblContent.cContentName, tblContentLocation.nStructId, tblAudit.dPublishDate, tblAudit.dUpdateDate, tblContent.cContentSchemaName" + " FROM tblContent INNER JOIN" + " tblAudit ON tblContent.nAuditId = tblAudit.nAuditKey INNER JOIN" + " tblContentLocation ON tblContent.nContentKey = tblContentLocation.nContentId" + " WHERE (tblContentLocation.bPrimary = 1) AND (tblAudit.nStatus = 1) AND (tblAudit.dPublishDate <= " + Tools.Database.SqlDate(mdDate) + " or tblAudit.dPublishDate is null) AND " + " (tblAudit.dExpireDate >= " + Tools.Database.SqlDate(mdDate) + " or tblAudit.dExpireDate is null) AND (tblContent.cContentSchemaName IN (" + cContentTypes + ")) ";
                string ContentIdsCSV = "";

                using (var oDR = moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                {

                    var oRe = new Regex("[^A-Z0-9]", RegexOptions.IgnoreCase);

                    while (oDR.Read())
                    {
                        string cURL = "";
                        var oContElmt = moPageXml.CreateElement("MenuItem");
                        long ContentId = Convert.ToInt64(oDR[0]);
                        ContentIdsCSV = ContentIdsCSV + ContentId + ",";
                        cURL = GetDetailURL(ContentId, oDR[5].ToString(), oDR[1].ToString(), "", Convert.ToInt64(oDR[2]), pageDict);

                        #region old code
                        //switch (moConfig["DetailPathType"] ?? "")
                        //{
                        //    case "ContentType/ContentName":
                        //        {
                        //            string[] prefixs = moConfig["DetailPrefix"].Split(',');
                        //            string thisPrefix = "";
                        //            string thisContentType = "";
                        //            int i;
                        //            var loopTo = prefixs.Length - 1;
                        //            for (i = 0; i <= loopTo; i++)
                        //            {
                        //                thisPrefix = prefixs[i].Substring(0, prefixs[i].IndexOf("/"));
                        //                thisContentType = prefixs[i].Substring(prefixs[i].IndexOf("/") + 1, prefixs[i].Length - prefixs[i].IndexOf("/") - 1);
                        //                if ((thisContentType ?? "") == (oDR[5].ToString() ?? ""))
                        //                {
                        //                    string ItemIdPath = "";
                        //                    if (moConfig["addPathArtId"] == "on")
                        //                    {
                        //                        ItemIdPath = oDR[0] + "-/";
                        //                    }
                        //                    cURL = "/" + thisPrefix + "/" + ItemIdPath + oRe.Replace(oDR[1].ToString(), "-").Trim('-');
                        //                    if (moConfig["DetailPathTrailingSlash"] == "on")
                        //                    {
                        //                        cURL = cURL + "/";
                        //                    }
                        //                    if (moConfig["LowerCaseUrl"] == "on")
                        //                    {
                        //                        cURL = cURL.ToLower();
                        //                    }
                        //                }
                        //            }

                        //            break;
                        //        }

                        //    default:
                        //        {
                        //            if (pageDict.ContainsKey(Convert.ToInt64(oDR[2])))
                        //            {
                        //                cURL = pageDict[Convert.ToInt64(oDR[2])];
                        //                // If moConfig("LegacyRedirect") = "on" Then
                        //                cURL += "/" + oDR[0].ToString() + "-/" + Tools.Text.CleanName(oDR[1].ToString(), false, true);
                        //            }
                        //            // Else
                        //            // cURL &= "/Item" & oDR(0).ToString
                        //            // End If
                        //            else
                        //            {
                        //                cProcessInfo = "orphan Content";
                        //            }

                        //            break;
                        //        }
                        //}
                        //if (moConfig["LowerCaseUrl"] == "on")
                        //{
                        //    cURL = cURL.ToLower();
                        //}
                        #endregion

                        if (!string.IsNullOrEmpty(cURL))
                        {
                            oContElmt.SetAttribute("url", cURL);
                            oContElmt.SetAttribute("name", oDR[1].ToString());
                            oContElmt.SetAttribute("publish", Tools.Xml.XmlDate(oDR[3].ToString(), false));
                            oContElmt.SetAttribute("update", Tools.Xml.XmlDate(oDR[4].ToString(), false));
                            oMenuElmt.AppendChild(oContElmt);
                        }



                    }
                    oDR.Close();

                    ContentIdsCSV = ContentIdsCSV.TrimEnd(',');
                    // we have sub products with there own pages which need to be indexed but they are not on the parent page
                    if (cIndexDetailSubTypes != "")
                    {
                        foreach (string subType in IndexDetailSubTypes)
                        {
                            string cSQL2 = "SELECT tblContent.nContentKey, tblContent.cContentName, tblAudit.dPublishDate, tblAudit.dUpdateDate, tblContent.cContentSchemaName" + " FROM tblContent INNER JOIN" + " tblAudit ON tblContent.nAuditId = tblAudit.nAuditKey INNER JOIN" + " tblContentRelation ON tblContent.nContentKey = tblContentRelation.nContentChildId" + " WHERE tblContentRelation.nContentParentId IN (" + ContentIdsCSV + ") AND tblContent.nContentKey NOT IN (" + ContentIdsCSV + ") AND (tblAudit.nStatus = 1) AND (tblAudit.dPublishDate <= " + Tools.Database.SqlDate(mdDate) + " or tblAudit.dPublishDate is null) AND " + " (tblAudit.dExpireDate >= " + Tools.Database.SqlDate(mdDate) + " or tblAudit.dExpireDate is null) AND (tblContent.cContentSchemaName IN ('" + cIndexDetailSubTypes + "')) ";
                            using (SqlDataReader oDR2 = moDbHelper.getDataReaderDisposable(cSQL2))  // Done by nita on 6/7/22
                            {
                                while (oDR2.Read())
                                {
                                    string cURL2 = "";
                                    var oContElmt2 = moPageXml.CreateElement("MenuItem");
                                    cURL2 = GetDetailURL(Convert.ToInt64(oDR2[0]), oDR2[4].ToString(), oDR2[1].ToString(), "", 0, pageDict);
                                    if (!string.IsNullOrEmpty(cURL2))
                                    {
                                        oContElmt2.SetAttribute("url", cURL2);
                                        oContElmt2.SetAttribute("name", oDR2[1].ToString());
                                        oContElmt2.SetAttribute("publish", Tools.Xml.XmlDate(oDR2[2].ToString(), false));
                                        oContElmt2.SetAttribute("update", Tools.Xml.XmlDate(oDR2[3].ToString(), false));
                                        oMenuElmt.AppendChild(oContElmt2);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "addPageDetailLinksToStructure", ex, gcEwSiteXsl, "", gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "addPageDetailLinksToStructure", ex, cProcessInfo));
            }
        }

        public void AddBreadCrumb(string cPath, string cDisplayName)
        {
            PerfMon.Log("Web", "AddBreadCrumb");
            XmlNode oNode;
            XmlElement oElmt;
            XmlElement oElmt2;

            string sProcessInfo = Convert.ToString(string.IsNullOrEmpty("adding Link to Breadcrumb"));

            try
            {
                oNode = moPageXml.DocumentElement.SelectSingleNode("Breadcrumb");
                if (oNode is null)
                {
                    oElmt = moPageXml.CreateElement("Breadcrumb");
                    moPageXml.DocumentElement.AppendChild(oElmt);
                }
                else
                {
                    oElmt = (XmlElement)oNode;
                }
                oElmt2 = moPageXml.CreateElement("Link");
                oElmt2.SetAttribute("name", cDisplayName);
                oElmt2.SetAttribute("url", cPath);
                oElmt.AppendChild(oElmt2);
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "AddBreadCrumb", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddBreadCrumb", ex, sProcessInfo));
            }
        }


    }
}