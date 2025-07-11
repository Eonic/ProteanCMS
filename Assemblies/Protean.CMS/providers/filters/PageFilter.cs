using Protean.Providers.Filter;
using System;
using System.Collections;
using System.Data;


using System.Data.SqlClient;
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
                    Hashtable arrParams = new Hashtable();
                    var oXml = oXform.moPageXML.CreateElement("PageFilter");
                    //XmlElement oFilterElmt = null;
                    string className = string.Empty;

                    if (oContentNode.Attributes["filterTarget"] != null)
                    {
                        cFilterTarget = oContentNode.Attributes["filterTarget"].Value;
                    }
                    if (aWeb.moRequest.Form["PageFilter"] != null)
                    {
                        oXml.InnerText = Convert.ToString(aWeb.moRequest.Form["PageFilter"]);

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

                    // arrParams = null;
                    //  sSql = sSql + $" @FilterTarget = '{cFilterTarget}', @PageId = null, @whereSQL = '{cWhereSql}' ";

                    using (SqlDataReader oDr = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams))  // Done by nita on 6/7/22
                    {
                        // Adding controls to the form like dropdown, radiobuttons
                        if (oDr != null && oDr.HasRows)
                        {
                           pageFilterSelect = oXform.addSelect(ref oPageGroup, "PageFilter", false, sCotrolDisplayName, "checkbox SubmitPageFilter", Protean.xForm.ApperanceTypes.Full);

                            //if (!string.IsNullOrEmpty(oXml.InnerText))
                            //{

                            //    pageFilterSelect = oXform.addSelect(ref oPageGroup, "PageFilter", false, sCotrolDisplayName, "checkbox SubmitPageFilter filter-selected", Protean.xForm.ApperanceTypes.Full);
                            //}
                            //else
                            //{
                            //    pageFilterSelect = oXform.addSelect(ref oPageGroup, "PageFilter", false, sCotrolDisplayName, "checkbox SubmitPageFilter", Protean.xForm.ApperanceTypes.Full);
                            //}

                            // oXform.addOptionsFromSqlDataReader(pageFilterSelect, oDr, "name", "nStructKey")
                            while (oDr.Read())
                            {
                                string name = Convert.ToString(oDr["cStructName"]) + " <span class='ProductCount'>" + Convert.ToString(oDr["ContentCount"]) + "</span>";
                                string value = Convert.ToString(oDr["nStructKey"]);

                                oXform.addOption(ref pageFilterSelect, name, value, true);

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
                            string[] aPages = oXml.InnerText.Split(',');
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
                            oXform.addDiv(ref oFromGroup, "", "PageClearAll", true);
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
                        cPageIds = oXform.Instance.SelectSingleNode("PageFilter").InnerText;

                    }



                    if (!string.IsNullOrEmpty(cPageIds))
                    {



                        if (!string.IsNullOrEmpty(cWhereSql))
                        {
                            cWhereSql = " AND ";
                        }
                        // If (bParentPageId) Then
                        // cWhereSql = " nStructId IN (" + cPageIds + ")"
                        // Else
                        cWhereSql = " nStructId IN (select nStructKey from tblContentStructure where (nStructKey in ( " + cPageIds + ") OR nStructParId in ( " + cPageIds + "))	)";
                        // nStructParId in (" & cPageIds & "))"
                        // End If
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
                    if (aWeb.moRequest.Form["PageFilter"] != null)
                    {
                        // cWhereSql = cWhereSql & "  nStructId IN(" + aWeb.moRequest.Form("PageFilter") & ")"
                        cWhereSql = cWhereSql + " nStructId IN (select nStructKey from tblContentStructure where (nStructKey in ( " + aWeb.moRequest.Form["PageFilter"] + ") OR nStructParId in ( " + aWeb.moRequest.Form["PageFilter"] + "))	)";

                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PageFilter", ex, ""));
                }
                return cWhereSql;
            }


        }

    }
}