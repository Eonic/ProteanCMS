using Protean.Providers.Filter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;


using System.Data.SqlClient;
using System.Linq;
using System.Xml;

namespace Protean.Providers
{
    namespace Filters
    {

        public class PageFilter : DefaultFilter
        {

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            public override void AddControl(ref Cms aWeb, ref XmlElement FilterConfig, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement oContentNode, string cWhereSql)
            {
                string cProcessInfo = "AddControl";
                try
                {
                    XmlElement pageFilterSelect;
                    // Dim pageFilterButtons As XmlElement
                    string sCotrolDisplayName = "Page Filter";
                    // Parent page id flag used to populate the root level pages or pages under current page.
                    bool bParentPageId = false;
                    string cFilterTarget = string.Empty;

                    XmlElement oPageGroup;



                    int nParentId = 1;
                    string sSql = "spGetPagesByParentPageId";
                    bool bShowAllDescendants = false;
                    Hashtable arrParams = new Hashtable();
                    var oXml = oXform.moPageXML.CreateElement("PageFilter");
                    //XmlElement oFilterElmt = null;
                    string className = string.Empty;

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

                    // Select the appropriate stored procedure
                    if (bShowAllDescendants)
                    {
                        sSql = "spGetPagesByParentPageIdAllDescendants";
                    }
                    if (aWeb.moRequest.Form["PageFilter"] != null)
                    {

                        string cpageIds = Convert.ToString(aWeb.moRequest.Form["PageFilter"]);

                        List<string> uniques = cpageIds.Split(',').Distinct().ToList();//(string[])cpageIds.Split(',').Distinct();

                        oXml.InnerText = string.Join(",", uniques);// Convert.ToString(aWeb.moRequest.Form["PageFilter"]);

                    }



                    oXform.Instance.AppendChild(oXml);

                    if (!string.IsNullOrEmpty(oXml.InnerText))
                    {
                        oPageGroup = oXform.addGroup(ref oXform.moXformElmt, "PageFilter", "pagefilter filter active-filter");
                    }
                    else
                    {
                        oPageGroup = oXform.addGroup(ref oXform.moXformElmt, "PageFilter", "pagefilter filter");
                    }
                    oFromGroup.AppendChild(oPageGroup);
                    // Adding a binding to the form bindings
                    oXform.addBind("PageFilter", "PageFilter", ref oXform.model, "false()", "string");
                    if (FilterConfig.Attributes["name"] != null)
                    {
                        sCotrolDisplayName = Convert.ToString(FilterConfig.Attributes["name"].Value);
                    }
                    // Get Parent page id flag and current id
                    if (FilterConfig.Attributes["parId"] != null)
                    {
                        nParentId = Convert.ToInt32(FilterConfig.Attributes["parId"].Value);
                    }
                    if (FilterConfig.Attributes["parentPageId"].Value != null)
                    {
                        bParentPageId = Convert.ToBoolean(Convert.ToInt32(FilterConfig.Attributes["parentPageId"].Value));
                    }
                    if (bParentPageId)
                    {
                        arrParams.Add("PageId", nParentId);
                        arrParams.Add("whereSql", cWhereSql);
                        arrParams.Add("FilterTarget", cFilterTarget);
                    }


                    using (SqlDataReader oDr = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams))  // Done by nita on 6/7/22
                    {
                        // Adding controls to the form like dropdown, radiobuttons
                        if (oDr != null && oDr.HasRows)
                        {
                            pageFilterSelect = oXform.addSelect(ref oPageGroup, "PageFilter", false, sCotrolDisplayName, "checkbox SubmitPageFilter", Protean.xForm.ApperanceTypes.Full);

                            // Check if nStructParId column exists (for hierarchical display)
                            bool hasParentColumn = false;
                            for (int i = 0; i < oDr.FieldCount; i++)
                            {
                                if (oDr.GetName(i) == "nStructParId")
                                {
                                    hasParentColumn = true;
                                    break;
                                }
                            }

                            // If hierarchical, read all data first to calculate levels
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

                            // Function to calculate level by walking up the parent chain
                            Func<int, int, int> CalculateLevel = null;
                            CalculateLevel = (pageId, rootId) =>
                            {
                                if (pageId == rootId) return 0;
                                if (!parentLookup.ContainsKey(pageId)) return 0;

                                int parentId = parentLookup[pageId];
                                if (parentId == rootId) return 0;

                                // Count levels up to root
                                int level = 0;
                                int currentId = pageId;
                                HashSet<int> visited = new HashSet<int>(); // Prevent infinite loops

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

                            // Now render all the options with calculated levels
                            foreach (var row in pageData)
                            {
                                int structKey = (int)row["nStructKey"];
                                string name = (string)row["cStructName"] + " <span class='badge ms-2' id='ProductCount'>" + (string)row["ContentCount"] + "</span>";
                                string value = structKey.ToString();

                                XmlElement optionElement = oXform.addOption(ref pageFilterSelect, name, value, true);

                                // Add level-based class if hierarchical data is available
                                if (hasParentColumn && optionElement != null)
                                {
                                    int level = CalculateLevel(structKey, nParentId);
                                    string levelClass = "page-level-" + level.ToString();

                                    // Add indent class for visual hierarchy
                                    if (level > 0)
                                    {
                                        levelClass += " page-child";
                                    }
                                    else
                                    {
                                        levelClass += " page-parent";
                                    }

                                    // Set class attribute
                                    optionElement.SetAttribute("class", levelClass);

                                    // Add data attributes
                                    if (row.ContainsKey("nStructParId"))
                                    {
                                        optionElement.SetAttribute("data-parent-id", ((int)row["nStructParId"]).ToString());
                                    }
                                    optionElement.SetAttribute("data-level", level.ToString());
                                }
                            }

                            oXform.addSubmit(ref oPageGroup, "", "Apply", "PageFilter", "  btnPageSubmit hidden", "");
                        }

                    }
                    if (oPageGroup.SelectSingleNode("select[@ref='PageFilter']/item") != null)
                    {
                        if (!string.IsNullOrEmpty(oXml.InnerText.Trim()))
                        {
                            string sText;
                            // Dim sValue As String
                            int cnt;
                            string[] aPages = oXml.InnerText.Split(',').Distinct().ToArray();
                            if (aPages.Length != 0 & aPages.Length != default)
                            {
                                var loopTo = aPages.Length - 1;
                                for (cnt = 0; cnt <= loopTo; cnt++)
                                {
                                    sText = oPageGroup.SelectSingleNode("select[@ref='PageFilter']/item[value='" + aPages[cnt] + "']").FirstChild.FirstChild.InnerText;

                                    oXform.addSubmit(ref oFromGroup, sText, sText, "PageFilter_" + aPages[cnt], " remove-PageFilter filter-applied", "fa-times");

                                }
                            }

                            else
                            {

                                sText = oPageGroup.SelectSingleNode("select[@ref='PageFilter']/item[value='" + oXml.InnerText + "']").FirstChild.FirstChild.InnerText;
                                oXform.addSubmit(ref oFromGroup, sText, sText, "PageFilter", " remove-PageFilter filter-applied", "fa-times");
                            }
                            oXform.addDiv(ref oFromGroup, "&#160;", "PageClearAll", true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""));
                }
            }

            public override string ApplyFilter(ref Cms aWeb, ref string cWhereSql, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget)
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