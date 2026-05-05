using System;
using System.Collections;
using System.Data;


using System.Data.SqlClient;
using System.Xml;
using Lucene.Net.Search;
using Protean.Providers.Filter;

namespace Protean.Providers
{

    namespace Filters
    {

        public class BrandFilter : DefaultFilter
        {

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            public override void AddControl(ref Cms aWeb, ref XmlElement FilterConfig, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement oContentNode, string cWhereSql)
            {
                string cProcessInfo = "AddControl";
                try
                {
                    string sControlDisplayName = FilterConfig.GetAttribute("specName");

                    string cFilterTarget = string.Empty;

                    //add to instance
                    Hashtable arrParams = new Hashtable();
                    XmlElement oXml = oXform.moPageXML.CreateElement("BrandFilter");
                    oXml.SetAttribute("name", sControlDisplayName);

                    // Get selected brand values from form submission
                    if (aWeb.moRequest.Form["BrandFilter[@name='" + sControlDisplayName + "']"] != null)
                    {
                        oXml.InnerText = Convert.ToString(aWeb.moRequest.Form["BrandFilter[@name='" + sControlDisplayName + "']"]);
                    }

                    oXform.Instance.AppendChild(oXml);

                    //add to binds
                    oXform.addBind("BrandFilter", "BrandFilter[@name='" + sControlDisplayName + "']", ref oXform.model, "false()", "string");

                    //add control
                    XmlElement thisSelect = oXform.addSelect(ref oFromGroup,"", false, sControlDisplayName, "specfilter", xForm.ApperanceTypes.Full);

                    string sSql = "SELECT DISTINCT CONCAT(cTextValue,' [',Count(*),']') as [Name], cTextValue as [Value] FROM [dbo].[tblContentIndex]\r\n  Where nContentIndexDefinitionKey = (Select nContentIndexDefKey from tblContentIndexDef where cDefinitionName = '" + sControlDisplayName + "') GROUP BY cTextValue Order By cTextValue";

                    switch (FilterConfig.GetAttribute("selectFrom")) {
                        case "all":
                            // do nothing - use the default query that gets all brands
                            break;
                        case "descendants":
                            // Get brands only for products on current page and its descendants
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

                       
                    using (var oDr = aWeb.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr != null)
                        {
                            var argoDr = oDr;
                            oXform.addOptionsFromSqlDataReader(thisSelect, argoDr);                          
                        }
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""));
                }
            }

            public override string ApplyFilter(ref Cms aWeb, ref string cWhereSql, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget)
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
            private string GetFilterSQL(ref Cms aWeb, string sControlDisplayName, string cBrandValues)
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