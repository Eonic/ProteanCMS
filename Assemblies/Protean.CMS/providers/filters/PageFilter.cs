using Protean.Providers.Filter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;


using System.Data.SqlClient;
using System.Linq;
using System.Xml;
using Protean.Providers.Filter;

namespace Protean.Providers
{
    namespace Filters
    {
        /// <summary>
        /// PageFilter - Provides filtering of content by page location in site hierarchy
        /// 
        /// KEY FEATURES:
        /// - Hierarchical page selection with visual tree indentation
        /// - Support for filtering by current page and all descendants
        /// - Multiple page selection with individual removal buttons
        /// - "Clear All" functionality to reset all selections
        /// - Active/inactive visual states for better UX
        /// - Configurable to show immediate children or all descendants
        /// 
        /// CONFIGURATION OPTIONS (in Filters.xml):
        /// - name: Display name of the filter
        /// - parId: Parent page ID to start from
        /// - parentPageId: Flag to use parent page filtering (0/1)
        /// - showAllDescendants: Show all levels or just immediate children (on/off)
        /// 
        /// FORM DATA:
        /// - Instance node: PageFilter (contains comma-separated page IDs)
        /// - Form field: PageFilter (posted on submission)
        /// - CSS classes: pagefilter, filter, active-filter, page-level-{n}, page-child, page-parent
        /// </summary>
        public class PageFilter : DefaultFilter, IContentFilter
        {

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            /// <summary>
            /// Adds the page filter control to the form with selected value display functionality
            /// 
            /// STRUCTURE:
            /// 1. INITIALIZATION: Set up variables and retrieve configuration
            /// 2. RESTORE STATE: Get selected values from form submission
            /// 3. CREATE INSTANCE NODE: Add filter data to XForm instance
            /// 4. CREATE FORM GROUP: Add visual grouping with active/inactive state
            /// 5. CREATE BINDING: Link control to data model
            /// 6. POPULATE OPTIONS: Load available pages from database
            /// 7. RENDER HIERARCHY: Calculate and display page levels
            /// 8. ADD BUTTONS: Create removal buttons for selected values
            /// 9. ADD CLEAR ALL: Add option to clear all selections
            /// </summary>
         
            public override void AddControl(ref Cms aWeb, ref XmlElement FilterConfig, ref Cms.xForm oXform, ref XmlElement oFromGroup, ref XmlElement oContentNode, string cWhereSql)
            {
                string cProcessInfo = "AddControl";
                try
                {
                    // ========================================
                    // STEP 1: INITIALIZATION - Declare variables and read configuration
                    // ========================================
                    XmlElement pageFilterSelect;
                    string sCotrolDisplayName = "Page Filter";
                    bool bParentPageId = false;
                    string cFilterTarget = string.Empty;
                    XmlElement oPageGroup;
                    int nParentId = 1;
                    string sSql = "spGetPagesByParentPageId";
                    bool bShowAllDescendants = false;
                    Hashtable arrParams = new Hashtable();

                    // Read filter target from content node if specified
                    if (oContentNode.Attributes["filterTarget"] != null)
                    {
                        cFilterTarget = oContentNode.Attributes["filterTarget"].Value;
                    }

                    // Check if we should show all descendants or just immediate children
                    if (FilterConfig.Attributes["showAllDescendants"] != null && 
                        FilterConfig.Attributes["showAllDescendants"].Value.ToLower() == "on")
                    {
                        bShowAllDescendants = true;
                    }

                    // Select the appropriate stored procedure based on configuration
                    if (bShowAllDescendants)
                    {
                        sSql = "spGetPagesByParentPageIdAllDescendants";
                    }

                    // Read display name from configuration if provided
                    if (FilterConfig.Attributes["name"] != null)
                    {
                        sCotrolDisplayName = Convert.ToString(FilterConfig.Attributes["name"].Value);
                    }

                    // Get parent page ID configuration
                    if (FilterConfig.Attributes["parId"] != null)
                    {
                        nParentId = Convert.ToInt32(FilterConfig.Attributes["parId"].Value);
                    }
                    if (FilterConfig.Attributes["parentPageId"].Value != null)
                    {
                        bParentPageId = Convert.ToBoolean(Convert.ToInt32(FilterConfig.Attributes["parentPageId"].Value));
                    }

                    // ========================================
                    // STEP 2: RESTORE STATE - Get previously selected values from form submission
                    // ========================================
                    var oXml = oXform.moPageXML.CreateElement("PageFilter");

                    if (aWeb.moRequest.Form["PageFilter"] != null)
                    {
                        string cpageIds = Convert.ToString(aWeb.moRequest.Form["PageFilter"]);
                        List<string> uniques = cpageIds.Split(',').Distinct().ToList();
                        oXml.InnerText = string.Join(",", uniques);
                    }

                    // ========================================
                    // STEP 3: CREATE INSTANCE NODE - Add filter data to XForm instance for data binding
                    // ========================================
                    // ========================================
                    // STEP 3: CREATE INSTANCE NODE - Add filter data to XForm instance for data binding
                    // ========================================
                    oXform.Instance.AppendChild(oXml);

                    // ========================================
                    // STEP 4: CREATE FORM GROUP - Add visual grouping with active/inactive CSS state
                    // This allows styling to show when filter is active vs inactive
                    // ========================================
                    if (!string.IsNullOrEmpty(oXml.InnerText))
                    {
                        // Filter has selections - mark as active
                        oPageGroup = oXform.addGroup(ref oXform.moXformElmt, "PageFilter", "pagefilter filter active-filter");
                    }
                    else
                    {
                        // Filter has no selections - mark as inactive
                        oPageGroup = oXform.addGroup(ref oXform.moXformElmt, "PageFilter", "pagefilter filter");
                    }
                    oFromGroup.AppendChild(oPageGroup);

                    // ========================================
                    // STEP 5: CREATE BINDING - Link the control to the data model
                    // ========================================
                    oXform.addBind("PageFilter", "PageFilter", ref oXform.model, "false()", "string");

                    // ========================================
                    // STEP 6: POPULATE OPTIONS - Query database and set up parameters
                    // ========================================
                    // ========================================
                    // STEP 6: POPULATE OPTIONS - Query database and set up parameters
                    // ========================================
                    if (bParentPageId)
                    {
                        arrParams.Add("PageId", nParentId);
                        arrParams.Add("whereSql", cWhereSql);
                        arrParams.Add("FilterTarget", cFilterTarget);
                    }

                    // Execute stored procedure and populate the select control
                    using (SqlDataReader oDr = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams))
                    {
                        if (oDr != null && oDr.HasRows)
                        {
                            // Create the select control (checkbox list in this case)
                            pageFilterSelect = oXform.addSelect(ref oPageGroup, "PageFilter", false, sCotrolDisplayName, "checkbox SubmitPageFilter", Protean.xForm.ApperanceTypes.Full);

                            // ========================================
                            // STEP 7: RENDER HIERARCHY - Calculate page levels for indentation
                            // This creates a visual tree structure in the filter
                            // ========================================

                            // Check if nStructParId column exists (indicates hierarchical data)
                            bool hasParentColumn = false;
                            for (int i = 0; i < oDr.FieldCount; i++)
                            {
                                if (oDr.GetName(i) == "nStructParId")
                                {
                                    hasParentColumn = true;
                                    break;
                                }
                            }

                            // Read all data first to build parent-child relationships
                            Dictionary<int, int> parentLookup = new Dictionary<int, int>();
                            List<Dictionary<string, object>> pageData = new List<Dictionary<string, object>>();

                            while (oDr.Read())
                            {
                                var row = new Dictionary<string, object>();
                                row["nStructKey"] = Convert.ToInt32(oDr["nStructKey"]);
                                row["cStructName"] = Convert.ToString(oDr["cStructName"]);
                                row["ContentCount"] = Convert.ToString(oDr["ContentCount"]);

                                if (hasParentColumn)
                                {
                                    row["nStructParId"] = Convert.ToInt32(oDr["nStructParId"]);
                                    parentLookup[Convert.ToInt32(oDr["nStructKey"])] = Convert.ToInt32(oDr["nStructParId"]);
                                }

                                pageData.Add(row);
                            }

                            // Function to calculate hierarchical level by walking up parent chain
                            Func<int, int, int> CalculateLevel = null;
                            CalculateLevel = (pageId, rootId) =>
                            {
                                if (pageId == rootId) return 0;
                                if (!parentLookup.ContainsKey(pageId)) return 0;

                                int parentId = parentLookup[pageId];
                                if (parentId == rootId) return 0;

                                // Count levels up to root, with infinite loop protection
                                int level = 0;
                                int currentId = pageId;
                                HashSet<int> visited = new HashSet<int>();

                                while (parentLookup.ContainsKey(currentId) && !visited.Contains(currentId))
                                {
                                    visited.Add(currentId);
                                    currentId = parentLookup[currentId];
                                    level++;

                                    if (currentId == rootId || currentId == nParentId)
                                        break;
                                }

                                return level;
                            };

                            // Render all options with calculated hierarchy levels
                            foreach (var row in pageData)
                            {
                                int structKey = (int)row["nStructKey"];
                                string name = (string)row["cStructName"] + " <span class='badge ms-2' id='ProductCount'>" + (string)row["ContentCount"] + "</span>";
                                string value = structKey.ToString();

                                XmlElement optionElement = oXform.addOption(ref pageFilterSelect, name, value, true);

                                // Add hierarchical CSS classes and data attributes
                                if (hasParentColumn && optionElement != null)
                                {
                                    int level = CalculateLevel(structKey, nParentId);
                                    string levelClass = "page-level-" + level.ToString();

                                    // Add visual hierarchy classes
                                    if (level > 0)
                                    {
                                        levelClass += " page-child";
                                    }
                                    else
                                    {
                                        levelClass += " page-parent";
                                    }

                                    // Set CSS class for styling
                                    optionElement.SetAttribute("class", levelClass);

                                    // Add data attributes for JavaScript interactions
                                    if (row.ContainsKey("nStructParId"))
                                    {
                                        optionElement.SetAttribute("data-parent-id", ((int)row["nStructParId"]).ToString());
                                    }
                                    optionElement.SetAttribute("data-level", level.ToString());
                                }
                            }

                            // Add hidden submit button (activated by JavaScript)
                            oXform.addSubmit(ref oPageGroup, "", "Apply", "PageFilter", "  btnPageSubmit hidden", "");
                        }
                    }

                    // ========================================
                    // STEP 8: ADD BUTTONS - Create removal buttons for each selected value
                    // This allows users to remove individual selections without reopening the filter
                    // ========================================
                    if (oPageGroup.SelectSingleNode("select[@ref='PageFilter']/item") != null)
                    {
                        if (!string.IsNullOrEmpty(oXml.InnerText.Trim()))
                        {
                            string sText;
                            string[] aPages = oXml.InnerText.Split(',').Distinct().ToArray();

                            if (aPages.Length != 0 && aPages.Length != default)
                            {
                                // Multiple selections - create a button for each
                                for (int cnt = 0; cnt <= aPages.Length - 1; cnt++)
                                {
                                    XmlNode nameNode = oPageGroup.SelectSingleNode("select[@ref='PageFilter']/item[value='" + aPages[cnt] + "']");
                                    if (nameNode != null && nameNode.FirstChild != null && nameNode.FirstChild.FirstChild != null)
                                    {
                                        sText = nameNode.FirstChild.FirstChild.InnerText;
                                        oXform.addSubmit(ref oFromGroup, sText, sText, "PageFilter_" + aPages[cnt], " remove-PageFilter filter-applied", "fa-times");
                                    }
                                }
                            }
                            else
                            {
                                // Single selection - create one button
                                XmlNode nameNode = oPageGroup.SelectSingleNode("select[@ref='PageFilter']/item[value='" + oXml.InnerText + "']");
                                if (nameNode != null && nameNode.FirstChild != null && nameNode.FirstChild.FirstChild != null)
                                {
                                    sText = nameNode.FirstChild.FirstChild.InnerText;
                                    oXform.addSubmit(ref oFromGroup, sText, sText, "PageFilter", " remove-PageFilter filter-applied", "fa-times");
                                }
                            }

                            // ========================================
                            // STEP 9: ADD CLEAR ALL - Add button to clear all selections at once
                            // ========================================
                            oXform.addDiv(ref oFromGroup, "&#160;", "PageClearAll", true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""));
                }
            }

            /// <summary>
            /// Applies the page filter to the SQL WHERE clause
            /// 
            /// LOGIC:
            /// 1. If user has selected specific pages, filter to those pages and their children
            /// 2. If no selection, default to current page and all descendants (when on a page)
            /// 3. Handles both parent and child page filtering based on configuration
            /// </summary>
            public override string ApplyFilter(ref Cms aWeb, ref string cWhereSql, ref Cms.xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget)
            {
                string cProcessInfo = "ApplyFilter";
                try
                {

                    // Get the filter type parent or child based on the value of the parentPageId attribute
                    bool bParentPageId = false;
                    if (FilterConfig.Attributes["parentPageId"].Value != null)
                    {
                        bParentPageId = Convert.ToBoolean(Convert.ToInt32(FilterConfig.Attributes["parentPageId"].Value));
                    }

                    string cPageIds = string.Empty;

                    if (oXform.Instance.SelectSingleNode("PageFilter") != null)
                    {
                        string cpageIds = oXform.Instance.SelectSingleNode("PageFilter").InnerText;

                        List<string> uniques = cpageIds.Split(',').Distinct().ToList();//(string[])cpageIds.Split(',').Distinct();

                        //oXml.InnerText = string.Join(",", uniques);

                        cPageIds = string.Join(",", uniques); ;//oXform.Instance.SelectSingleNode("PageFilter").InnerText;

                    }



                    if (!string.IsNullOrEmpty(cPageIds))
                    {



                        if (cWhereSql != string.Empty)
                        {
                            cWhereSql = cWhereSql + " AND ";
                        }


                        cWhereSql = cWhereSql + " nStructId IN (select nStructKey from tblContentStructure where (nStructKey in ( " + cPageIds + ") OR nStructParId in ( " + cPageIds + ")))";// GetFilterSQL(ref aWeb);
                    }
                    else if (aWeb.mnPageId > 0)
                    {
                        // Default behavior: when no page filter is selected, scope to current page and all descendants
                        if (!string.IsNullOrEmpty(cWhereSql))
                        {
                            cWhereSql = cWhereSql + " AND ";
                        }

                        // Use a recursive subquery instead of CTE to avoid syntax errors when embedded in WHERE clause
                        cWhereSql = cWhereSql + " nStructId IN (" +
                            "SELECT nStructKey FROM tblContentStructure WHERE nStructKey = " + aWeb.mnPageId + " " +
                            "UNION ALL " +
                            "SELECT cs.nStructKey FROM tblContentStructure cs WITH(NOLOCK) " +
                            "INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = cs.nAuditId AND ca.nStatus = 1 " +
                            "WHERE cs.nStructParId = " + aWeb.mnPageId + " " +
                            "OR cs.nStructParId IN (" +
                            "SELECT nStructKey FROM tblContentStructure WHERE nStructParId = " + aWeb.mnPageId + " AND nAuditId IN (SELECT nAuditKey FROM tblAudit WHERE nStatus = 1)" +
                            "))";
                    }
                    return cWhereSql;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""));
                    return null;
                }

            }

            public override string GetFilterSQL(ref Cms aWeb)
            {
                string cWhereSql = string.Empty;
                string cProcessInfo = "GetFilterSQL";
                string cPageIds = string.Empty;
                try
                {

                    //if (aWeb.Attributes["parId"] != null)
                    //{
                    //    cPageIds = Convert.ToInt32(aWeb.Attributes["parId"].Value);
                    //}
                    //if (aWeb.moRequest.Form["PageFilter"] != null)
                    //{

                    //    string cpageIds = Convert.ToString(aWeb.moRequest.Form["PageFilter"]);

                    //    List<string> uniques = cpageIds.Split(',').Distinct().ToList();

                    //    cPageIds = string.Join(",", uniques);

                    //}

                    cPageIds =Convert.ToString(aWeb.mnPageId);
                    if (cPageIds != "")
                    {

                        // cWhereSql = cWhereSql & "  nStructId IN(" + aWeb.moRequest.Form("PageFilter") & ")"
                        cWhereSql = cWhereSql + " nStructId IN (select nStructKey from tblContentStructure where (nStructKey in ( " + cPageIds + ") OR nStructParId in ( " + cPageIds + "))	)";

                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""));
                }
                return cWhereSql;
            }

            public override string ContentIndexDefinationName(ref Cms aWeb)
            {
                return "";
            }

        }

    }
}