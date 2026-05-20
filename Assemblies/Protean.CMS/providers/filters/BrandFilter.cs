using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;
using Lucene.Net.Search;
using Protean.Providers.Filter;

namespace Protean.Providers
{

    namespace Filters
    {
        /// <summary>
        /// BrandFilter - Provides filtering of products by brand using ContentIndex values
        /// 
        /// KEY FEATURES (mirrors PageFilter structure):
        /// - Multiple brand selection with count display
        /// - Individual removal buttons for each selected brand
        /// - "Clear All" functionality to reset all selections
        /// - Active/inactive visual states for better UX
        /// - Configurable scope (all brands or brands in current page hierarchy)
        /// - Duplicate prevention and value cleanup
        /// 
        /// CONFIGURATION OPTIONS (in Filters.xml):
        /// - specName: The ContentIndexDef name to use (e.g., "Brand")
        /// - selectFrom: Scope of brands to display:
        ///   * "all" - Show all brands in the system
        ///   * "descendants" - Show only brands for products on current page and child pages
        /// 
        /// FORM DATA:
        /// - Instance node: BrandFilter[@name='{specName}'] (contains comma-separated brand values)
        /// - Form field: BrandFilter[@name='{specName}'] (posted on submission)
        /// - CSS classes: brandfilter, filter, active-filter, remove-BrandFilter, filter-applied
        /// 
        /// DATABASE STRUCTURE:
        /// - Uses tblContentIndex and tblContentIndexDef tables
        /// - Filters based on cTextValue matching brand names
        /// - Displays count of products per brand in brackets
        /// </summary>
        public class BrandFilter : DefaultFilter, IContentFilter
        {

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            /// <summary>
            /// Adds the brand filter control to the form with selected value display functionality
            /// 
            /// STRUCTURE (mirrors PageFilter.cs):
            /// 1. INITIALIZATION: Set up variables and retrieve configuration
            /// 2. RESTORE STATE: Get selected values from form submission
            /// 3. CREATE INSTANCE NODE: Add filter data to XForm instance
            /// 4. CREATE FORM GROUP: Add visual grouping with active/inactive state
            /// 5. CREATE BINDING: Link control to data model
            /// 6. POPULATE OPTIONS: Load available brands from database
            /// 7. ADD BUTTONS: Create removal buttons for selected values
            /// 8. ADD CLEAR ALL: Add option to clear all selections
            /// </summary>
            /// 

            public override void AddControl(ref Cms aWeb, ref XmlElement FilterConfig, ref Cms.xForm oXform, ref XmlElement oFromGroup, ref XmlElement oContentNode, string cWhereSql)
            {
                string cProcessInfo = "AddControl";
                try
                {
                    // ========================================
                    // STEP 1: INITIALIZATION - Declare variables and read configuration
                    // ========================================
                    string sControlDisplayName = FilterConfig.GetAttribute("specName");
                    string cFilterTarget = string.Empty;

                    // Read filter target from content node if specified
                    if (oContentNode.Attributes["filterTarget"] != null)
                    {
                        cFilterTarget = oContentNode.Attributes["filterTarget"].Value;
                    }

                    // ========================================
                    // STEP 2: RESTORE STATE - Get previously selected values from form submission
                    // ========================================
                    Hashtable arrParams = new Hashtable();
                    XmlElement oXml = oXform.moPageXML.CreateElement("BrandFilter");
                    oXml.SetAttribute("name", sControlDisplayName);

                    // Get selected brand values from form submission
                    string selectedBrands = string.Empty;
                    if (aWeb.moRequest.Form["BrandFilter"] != null)
                    {
                        selectedBrands = Convert.ToString(aWeb.moRequest.Form["BrandFilter"]);
                    }

                    // ========================================
                    // DETECT AND HANDLE REMOVAL BUTTON CLICKS
                    // ========================================
                    // Check if any removal button was clicked (buttons are named "BrandFilter_0", "BrandFilter_1", etc.)
                    // If detected, remove that specific brand from the selection
                    bool removalDetected = false;
                    string brandToRemove = string.Empty;

                    // Iterate through all form keys to find removal button submissions
                    foreach (string key in aWeb.moRequest.Form.AllKeys)
                    {
                        if (key != null && key.StartsWith("BrandFilter_"))
                        {
                            removalDetected = true;

                            // Extract the index from the button name (e.g., "BrandFilter_2" -> index 2)
                            string indexStr = key.Replace("BrandFilter_", "");

                            // Handle indexed buttons (BrandFilter_0, BrandFilter_1, etc.)
                            if (int.TryParse(indexStr, out int brandIndex))
                            {
                                // Multiple selections - remove by index
                                if (!string.IsNullOrEmpty(selectedBrands))
                                {
                                    string[] brands = selectedBrands.Split(',')
                                        .Select(b => b.Trim())
                                        .Where(b => !string.IsNullOrEmpty(b))
                                        .ToArray();

                                    if (brandIndex >= 0 && brandIndex < brands.Length)
                                    {
                                        brandToRemove = brands[brandIndex];
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Also check for non-indexed removal button (single selection case)
                    if (!removalDetected && aWeb.moRequest.Form["BrandFilter"] != null)
                    {
                        // Check if the form was submitted via a button (not just checkbox change)
                        string[] formKeys = aWeb.moRequest.Form.AllKeys;
                        if (formKeys.Contains("BrandFilter") && formKeys.Length > 1)
                        {
                            // This might be a single-selection removal
                            // The button value will match a brand name
                            string buttonValue = Convert.ToString(aWeb.moRequest.Form["BrandFilter"]);
                            if (!string.IsNullOrEmpty(buttonValue) && buttonValue == selectedBrands.Trim())
                            {
                                removalDetected = true;
                                brandToRemove = buttonValue;
                            }
                        }
                    }

                    // ========================================
                    // APPLY REMOVAL IF BUTTON WAS CLICKED
                    // ========================================
                    if (removalDetected && !string.IsNullOrEmpty(brandToRemove))
                    {
                        // Remove the specific brand from the selection
                        List<string> brandList = selectedBrands.Split(',')
                            .Select(b => b.Trim())
                            .Where(b => !string.IsNullOrEmpty(b) && b != brandToRemove.Trim())
                            .Distinct()
                            .ToList();
                        selectedBrands = string.Join(",", brandList);
                    }

                    // ========================================
                    // CLEAN UP AND STORE SELECTED VALUES
                    // ========================================
                    // Remove duplicates and empty values, then store in instance node
                    if (!string.IsNullOrEmpty(selectedBrands))
                    {
                        List<string> uniques = selectedBrands.Split(',')
                            .Select(b => b.Trim())
                            .Where(b => !string.IsNullOrEmpty(b))
                            .Distinct()
                            .ToList();
                        oXml.InnerText = string.Join(",", uniques);
                    }

                    // ========================================
                    // STEP 3: CREATE INSTANCE NODE - Add filter data to XForm instance for data binding
                    // ========================================
                    oXform.Instance.AppendChild(oXml);

                    // ========================================
                    // STEP 4: CREATE FORM GROUP - Add visual grouping with active/inactive CSS state
                    // This allows styling to show when filter is active vs inactive
                    // ========================================
                    XmlElement oBrandGroup;
                    if (!string.IsNullOrEmpty(oXml.InnerText))
                    {
                        // Filter has selections - mark as active
                        oBrandGroup = oXform.addGroup(ref oXform.moXformElmt, "BrandFilter", "brandfilter filter active-filter");
                    }
                    else
                    {
                        // Filter has no selections - mark as inactive
                        oBrandGroup = oXform.addGroup(ref oXform.moXformElmt, "BrandFilter", "brandfilter filter");
                    }
                    oFromGroup.AppendChild(oBrandGroup);

                    // ========================================
                    // STEP 5: CREATE BINDING - Link the control to the data model
                    // ========================================
                    oXform.addBind("BrandFilter", "BrandFilter[@name='" + sControlDisplayName + "']", ref oXform.model, "false()", "string");

                    // ========================================
                    // STEP 6: POPULATE OPTIONS - Query database for available brands
                    // ========================================

                    // Create the select control (checkbox list)
                    XmlElement brandFilterSelect = oXform.addSelect(ref oBrandGroup, "BrandFilter", true, sControlDisplayName, "brandfilter checkbox SubmitBrandFilter", xForm.ApperanceTypes.Full);

                   
                    // Build SQL query based on selectFrom configuration
                    string sSql = "SELECT DISTINCT CONCAT(cTextValue,' [',Count(*),']') as [Name], cTextValue as [Value] FROM [dbo].[tblContentIndex]\r\n  Where nContentIndexDefinitionKey = (Select nContentIndexDefKey from tblContentIndexDef where cDefinitionName = '" + sControlDisplayName + "') GROUP BY cTextValue Order By cTextValue";

                    // Check selectFrom attribute for scoping brands to current page hierarchy
                    switch (FilterConfig.GetAttribute("selectFrom")) {
                        case "all":
                            // Default - use the query above that gets all brands
                            break;
                        case "descendants":
                            // Scope to products on current page and all descendant pages only
                            long nCurrentPageId = aWeb.mnPageId;
                            sSql = @"
                                WITH PageHierarchy AS (
                                    -- Start with the current page
                                    SELECT nStructKey, 0 AS Level
                                    FROM tblContentStructure cs WITH(NOLOCK)
                                    INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = cs.nAuditId AND ca.nStatus = 1
                                    WHERE cs.nStructKey = " + nCurrentPageId + @"

                                    UNION ALL

                                    -- Get all descendant pages recursively
                                    SELECT cs.nStructKey, ph.Level + 1
                                    FROM tblContentStructure cs WITH(NOLOCK)
                                    INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = cs.nAuditId AND ca.nStatus = 1
                                    INNER JOIN PageHierarchy ph ON cs.nStructParId = ph.nStructKey
                                    WHERE ph.Level < 20
                                )
                                SELECT DISTINCT 
                                    CONCAT(ci.cTextValue,' [',Count(DISTINCT c.nContentKey),']') as [Name], 
                                    ci.cTextValue as [Value] 
                                FROM [dbo].[tblContentIndex] ci
                                INNER JOIN tblContent c WITH(NOLOCK) ON c.nContentKey = ci.nContentId
                                INNER JOIN tblAudit ca WITH(NOLOCK) ON ca.nAuditKey = c.nAuditId AND ca.nStatus = 1
                                INNER JOIN tblContentLocation cl WITH(NOLOCK) ON cl.nContentId = c.nContentKey
                                INNER JOIN tblAudit cla WITH(NOLOCK) ON cla.nAuditKey = cl.nAuditId AND cla.nStatus = 1
                                INNER JOIN PageHierarchy ph ON ph.nStructKey = cl.nStructId
                                WHERE ci.nContentIndexDefinitionKey = (
                                    SELECT nContentIndexDefKey 
                                    FROM tblContentIndexDef 
                                    WHERE cDefinitionName = '" + sControlDisplayName + @"'
                                )
                                GROUP BY ci.cTextValue 
                                ORDER BY ci.cTextValue
                                OPTION (MAXRECURSION 100)";
                            break;
                    }

                    // Execute query and populate options
                    using (var oDr = aWeb.moDbHelper.getDataReaderDisposable(sSql))
                    {
                        if (oDr != null)
                        {
                            var argoDr = oDr;
                            oXform.addOptionsFromSqlDataReader(brandFilterSelect, argoDr);
                        }
                    }

                    // Mark selected options based on saved values
                    if (!string.IsNullOrEmpty(oXml.InnerText))
                    {
                        string[] selectedBrandsArray = oXml.InnerText.Split(',');
                        foreach (string brand in selectedBrandsArray)
                        {
                            string cleanBrand = brand.Trim();
                            if (!string.IsNullOrEmpty(cleanBrand))
                            {
                                // Find the option node and mark it as selected
                                XmlNode optionNode = oBrandGroup.SelectSingleNode("select[@bind='BrandFilter']/item[value='" + cleanBrand.Replace("'", "&apos;") + "']");
                                if (optionNode != null)
                                {
                                    ((XmlElement)optionNode).SetAttribute("selected", "true");
                                }
                            }
                        }
                    }

                    // Add hidden submit button (activated by JavaScript)
                    oXform.addSubmit(ref oBrandGroup, "", "Apply", "BrandFilter", "btnBrandSubmit hidden", "");

                    // ========================================
                    // STEP 7: ADD BUTTONS - Create removal buttons for each selected value
                    // This allows users to remove individual brand selections
                    // ========================================
                    if (oBrandGroup.SelectSingleNode("select[@bind='BrandFilter']/item") != null)
                    {
                        if (!string.IsNullOrEmpty(oXml.InnerText.Trim()))
                        {
                            string sText;
                            string[] aBrands = oXml.InnerText.Split(',').Distinct().ToArray();

                            if (aBrands.Length != 0 && aBrands.Length != default)
                            {
                                // Multiple selections - create a button for each
                                for (int cnt = 0; cnt <= aBrands.Length - 1; cnt++)
                                {
                                    string cleanBrand = aBrands[cnt].Trim();
                                    if (!string.IsNullOrEmpty(cleanBrand))
                                    {
                                        XmlNode nameNode = oBrandGroup.SelectSingleNode("select[@bind='BrandFilter']/item[value='" + cleanBrand.Replace("'", "&apos;") + "']");
                                        if (nameNode != null && nameNode.FirstChild != null && nameNode.FirstChild.FirstChild != null)
                                        {
                                            sText = nameNode.FirstChild.FirstChild.InnerText;
                                            // Remove the count suffix if present (e.g., "Alpha [5]" -> "Alpha")
                                            if (sText.Contains(" ["))
                                            {
                                                sText = sText.Substring(0, sText.LastIndexOf(" ["));
                                            }
                                            oXform.addSubmit(ref oFromGroup, sText, sText, "BrandFilter_" + cnt, " remove-BrandFilter filter-applied", "fa-times");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Single selection - create one button
                                string cleanBrand = oXml.InnerText.Trim();
                                if (!string.IsNullOrEmpty(cleanBrand))
                                {
                                    XmlNode nameNode = oBrandGroup.SelectSingleNode("select[@bind='BrandFilter']/item[value='" + cleanBrand.Replace("'", "&apos;") + "']");
                                    if (nameNode != null && nameNode.FirstChild != null && nameNode.FirstChild.FirstChild != null)
                                    {
                                        sText = nameNode.FirstChild.FirstChild.InnerText;
                                        // Remove the count suffix if present
                                        if (sText.Contains(" ["))
                                        {
                                            sText = sText.Substring(0, sText.LastIndexOf(" ["));
                                        }
                                        oXform.addSubmit(ref oFromGroup, sText, sText, "BrandFilter", " remove-BrandFilter filter-applied", "fa-times");
                                    }
                                }
                            }

                            // ========================================
                            // STEP 8: ADD CLEAR ALL - Add button to clear all brand selections at once
                            // ========================================
                            oXform.addDiv(ref oFromGroup, "&#160;", "BrandClearAll", true);
                        }
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""));
                }
            }

            /// <summary>
            /// Applies the brand filter to the SQL WHERE clause
            /// 
            /// LOGIC:
            /// 1. Retrieves selected brand values from the form instance
            /// 2. If brands are selected, builds SQL to filter content by those brand index values
            /// 3. Uses ContentIndex table to match products to selected brands
            /// </summary>
            public override string ApplyFilter(ref Cms aWeb, ref string cWhereSql, ref Cms.xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget)
            {
                string cProcessInfo = "ApplyFilter";
                try
                {
                    string sControlDisplayName = FilterConfig.GetAttribute("specName");
                    string cBrandValues = string.Empty;

                    // Get selected brand values from the form instance
                    XmlNode brandNode = oXform.Instance.SelectSingleNode("BrandFilter[@name='" + sControlDisplayName + "']");
                    if (brandNode != null && !string.IsNullOrEmpty(brandNode.InnerText))
                    {
                        cBrandValues = brandNode.InnerText;
                    }

                    // If brand values are selected, add them to the WHERE clause
                    if (!string.IsNullOrEmpty(cBrandValues))
                    {
                        if (!string.IsNullOrEmpty(cWhereSql))
                        {
                            cWhereSql = cWhereSql + " AND ";
                        }
                        cWhereSql = cWhereSql + GetFilterSQL(ref aWeb, sControlDisplayName, cBrandValues);
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "BrandFilter", ex, ""));
                }
                return cWhereSql;
            }

            public override string GetFilterSQL(ref Cms aWeb)
            {
                // This override is not typically called directly for BrandFilter
                // Use the overloaded version instead
                return string.Empty;
            }

            // Overloaded method to build SQL filter for specific brand values
            private  string GetFilterSQL(ref Cms aWeb, string sControlDisplayName, string cBrandValues)
            {
                string cWhereSql = string.Empty;
                string cProcessInfo = "GetFilterSQL";
                try
                {
                    if (!string.IsNullOrEmpty(cBrandValues))
                    {
                        // Split multiple brand values (comma-separated)
                        string[] brandArray = cBrandValues.Split(',');

                        // Build SQL IN clause for brand filtering
                        // This filters content that has any of the selected brand values in the index
                        cWhereSql = " nContentKey IN (SELECT DISTINCT ci.nContentId FROM tblContentIndex ci " +
                                    "INNER JOIN tblContentIndexDef cid ON cid.nContentIndexDefKey = ci.nContentIndexDefinitionKey " +
                                    "INNER JOIN tblAudit ca ON ca.nAuditKey = cid.nAuditId AND ca.nStatus = 1 " +
                                    "WHERE cid.cDefinitionName = '" + sControlDisplayName + "' AND ci.cTextValue IN (";

                        // Add quoted brand values
                        for (int i = 0; i < brandArray.Length; i++)
                        {
                            if (i > 0) cWhereSql += ", ";
                            cWhereSql += "'" + brandArray[i].Replace("'", "''") + "'"; // Escape single quotes
                        }

                        cWhereSql += "))";
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "BrandFilter", ex, ""));
                }
                return cWhereSql;
            }

            public override string GetFilterOrderByClause(ref Cms myWeb)
            {
                // Brand filter doesn't have a specific order by clause
                // Return empty string to use default ordering
                return "";
            }

            public override string ContentIndexDefinationName(ref Cms aWeb)
            {
                // Return empty as the definition name is retrieved from FilterConfig
                return "";
            }


        }

    }
}