using Microsoft.Ajax.Utilities;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.Filter;
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using static Protean.xForm;

namespace Protean.Providers
{

    namespace Filters
    {

        public class PriceBandFilter : DefaultFilter
        {

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
            public override void AddControl(ref Cms aWeb, ref XmlElement FilterConfig, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement oContentNode, string cWhereSql)
            {
                string cProcessInfo = "AddControl";
                try
                {
                    string sSql = "spGetPriceRange";
                    var arrParams = new Hashtable();
                    XmlElement priceBandFilterSelect;
                    string sCotrolDisplayName = "PriceBand Filter";
                    string cFilterTarget = string.Empty;
                    XmlElement oPriceBandGroup;
                    var oXml = oXform.moPageXML.CreateElement("PriceBandFilter");
                    if (aWeb.moRequest.Form["PriceBandFilter"] != null)
                    {
                        oXml.InnerText = Convert.ToString(aWeb.moRequest.Form["PriceBandFilter"]);

                    }
                    if (!string.IsNullOrEmpty(oXml.InnerText))
                    {
                        oPriceBandGroup = oXform.addGroup(ref oXform.moXformElmt, "PriceBandFilter", "PriceBandfilter filter active-filter");

                    }
                    else
                    {
                        oPriceBandGroup = oXform.addGroup(ref oXform.moXformElmt, "PriceBandFilter", "PriceBandfilter filter");
                    }

                    oFromGroup.AppendChild(oPriceBandGroup);
                   
                    var oMinPriceBand = oXform.moPageXML.CreateAttribute("MinPriceBand");
                    var oMaxPriceBand = oXform.moPageXML.CreateAttribute("MaxPriceBand");
                    var oMaxPriceBandLimit = oXform.moPageXML.CreateAttribute("MaxPriceBandLimit");
                    var oStep = oXform.moPageXML.CreateAttribute("PriceBandStep");
                   
                    string sProductCount = string.Empty;
                    int cnt = 0;
                    string cProductCountList = string.Empty;
                    string nPageId = string.Empty;
                    int nMaxPriceBandProduct = 0;
                    int nMinPriceBandProduct = 0;
                
                    string className = string.Empty;
                    string cWhereQuery = string.Empty;

                   
                    if (oContentNode.Attributes["filterTarget"] != null)
                    {
                        cFilterTarget = oContentNode.Attributes["filterTarget"].Value;
                    }

                    if (FilterConfig.Attributes["maxPriceBandLimit"] != null)
                    {
                        oMaxPriceBandLimit.Value = Convert.ToString(FilterConfig.Attributes["maxPriceLimit"].Value);
                    }
                    if (FilterConfig.Attributes["name"] != null)
                    {
                        sCotrolDisplayName = Convert.ToString(FilterConfig.Attributes["name"].Value);
                    }

                   

                    if (oXform.Instance.SelectSingleNode("PageFilter") != null)
                    {
                        if (!string.IsNullOrEmpty(oXform.Instance.SelectSingleNode("PageFilter").InnerText))
                        {
                            nPageId = oXform.Instance.SelectSingleNode("PageFilter").InnerText.ToString();
                        }
                        else
                        {
                            nPageId = aWeb.mnPageId.ToString();
                        }
                    }
                    else
                    {
                        nPageId = aWeb.mnPageId.ToString();
                    }

                    if (oMaxPriceBandLimit.Value != null)
                    {
                        if (oMaxPriceBandLimit.Value != string.Empty)
                        {
                            oMaxPriceBandLimit.Value = Convert.ToString(FilterConfig.Attributes["maxPriceLimit"].Value);
                        }
                        else
                        {
                            oMaxPriceBandLimit.Value = Convert.ToString(FilterConfig.GetAttribute("toPrice"));
                        }
                    }
                    else
                    {
                        oMaxPriceBandLimit.Value = FilterConfig.GetAttribute("toPrice");
                    }
                    arrParams.Add("MinPrice", FilterConfig.GetAttribute("fromPrice"));
                    arrParams.Add("MaxPrice", oMaxPriceBandLimit.Value);
                    arrParams.Add("Step", FilterConfig.GetAttribute("step"));
                    arrParams.Add("PageId", nPageId);
                    arrParams.Add("whereSql", cWhereSql);
                    arrParams.Add("FilterTarget", cFilterTarget);

                    priceBandFilterSelect = oXform.addSelect(ref oPriceBandGroup, "PriceBandFilter", false, sCotrolDisplayName, "checkbox SubmitPriceBandFilter", ApperanceTypes.Full);
                   

                    using (SqlDataReader oDr = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams))
                    {
                        if (oDr != null)
                        {
                            while (oDr.Read())
                            {
                                nMinPriceBandProduct = Conversions.ToInteger(oDr["MinPrice"]);
                                string sText= string.Empty;
                                nMaxPriceBandProduct = Conversions.ToInteger(oDr["MaxPrice"]);
                                sProductCount = Convert.ToString(oDr["PriceBandContentCount"]);
                                cProductCountList = cProductCountList + cnt.ToString() + ":" + sProductCount + ",";
                                if(nMinPriceBandProduct== nMaxPriceBandProduct)
                                {
                                     sText = ">" + aWeb.moCart.mcCurrencySymbol + "" + nMaxPriceBandProduct + " <span class='badge ms-2' id='ProductCount'>" + sProductCount + "</span>"/*"From " + oMinPriceBand.Value.Trim() + " to " + oMaxPriceBand.Value.Trim()*/;

                                }
                                else
                                {
                                     sText = aWeb.moCart.mcCurrencySymbol + "" + nMinPriceBandProduct + " - " + aWeb.moCart.mcCurrencySymbol + "" + nMaxPriceBandProduct + " <span class='badge ms-2' id='ProductCount'>" + sProductCount + "</span>"/*"From " + oMinPriceBand.Value.Trim() + " to " + oMaxPriceBand.Value.Trim()*/;


                                }
                                //string sText = aWeb.moCart.mcCurrencySymbol + "" + nMinPriceBandProduct + " - " + aWeb.moCart.mcCurrencySymbol + "" + nMaxPriceBandProduct + " <span class='badge ms-2' id='ProductCount'>" + sProductCount + "</span>"/*"From " + oMinPriceBand.Value.Trim() + " to " + oMaxPriceBand.Value.Trim()*/;
                                string value = Convert.ToString(nMinPriceBandProduct) + "-" + Convert.ToString(nMaxPriceBandProduct);

                                oXform.addOption(ref priceBandFilterSelect, sText, value, true);
                            }

                        }

                    }
                        
                        oStep.Value = FilterConfig.GetAttribute("step");
                        //oMinPriceBand.Value= Convert.ToString(nMinPriceBandProduct);
                        oXml.Attributes.Append(oMinPriceBand);
                        oXml.Attributes.Append(oMaxPriceBand);
                        oXml.Attributes.Append(oMaxPriceBandLimit);
                        oXml.Attributes.Append(oStep);
                   

                   

                    oXform.Instance.AppendChild(oXml);


                    oXform.addBind("PriceBandFilter", "PriceBandFilter", ref oXform.model, "false()", "string");

                 
                    //oXform.addInput(ref oPriceBandGroup, "MinPriceBand", true, "", "hidden");
                    //oXform.addInput(ref oPriceBandGroup, "MaxPriceBand", true, "", "hidden");
                   oXform.addSubmit(ref oPriceBandGroup, "", "Apply", "PriceBandFilter", "  btnPriceBandSubmit hidden", "");


                    if (oPriceBandGroup.SelectSingleNode("select[@ref='PriceBandFilter']/item") != null)
                    {
                        if (!string.IsNullOrEmpty(oXml.InnerText.Trim()))
                        {
                            string sText;
                            // Dim sValue As String
                            //int cnt;
                            string[] aPriceFilters = oXml.InnerText.Split(',');
                            if (aPriceFilters.Length != 0 & aPriceFilters.Length != default)
                            {
                                var loopTo = aPriceFilters.Length - 1;
                                for (cnt = 0; cnt <= loopTo; cnt++)
                                {
                                    sText = oPriceBandGroup.SelectSingleNode("select[@ref='PriceBandFilter']/item[value='" + aPriceFilters[cnt] + "']").FirstChild.FirstChild.InnerText;

                                    oXform.addSubmit(ref oFromGroup, sText, sText, "PriceBandFilter_" + aPriceFilters[cnt], " remove-PriceBandFilter  filter-applied", "fa-times");

                                }
                            }

                            else
                            {

                                sText = oPriceBandGroup.SelectSingleNode("select[@ref='PriceBandFilter']/item[value='" + oXml.InnerText + "']").FirstChild.FirstChild.InnerText;
                                oXform.addSubmit(ref oFromGroup, sText, sText, "PriceBandFilter", " remove-PriceBandFilter filter-applied", "fa-times");
                            }
                            oXform.addDiv(ref oPriceBandGroup, "", "PriceBandClearFilter", true);
                        }
                    }
                

                    //if (aWeb.moRequest.Form["PriceBandFilter"] != null )
                    //{

                    //    string sText = "From " + aWeb.moCart.mcCurrencySymbol + "" + oMinPriceBand.Value.Trim() + " to " + aWeb.moCart.mcCurrencySymbol + "" + oMaxPriceBand.Value.Trim()/*"From " + oMinPriceBand.Value.Trim() + " to " + oMaxPriceBand.Value.Trim()*/;
                    //    oXform.addSubmit(ref oPriceBandGroup, "PriceBandFilter", sText, "PriceBandFilter", "remove-PriceBandFilter filter-applied", "fa-times");
                    //    oXform.addDiv(ref oPriceBandGroup, "", "PriceBandClearFilter", true);
                    //}

                   

                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceBandFilter", ex, ""));
                }
            }

            public override string ApplyFilter(ref Cms aWeb, ref string cWhereSql, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget)
            {
                string cProcessInfo = "ApplyFilter";
                string cPriceBandCond = string.Empty;
                try
                {
                    // Dim PriceBandRange() As String
                    //string cDefinitionName = "PriceBand";

                    

                    string filterSQL = GetFilterSQL(ref aWeb);

                    if (!string.IsNullOrEmpty(cWhereSql))
                    {
                        if (filterSQL != "")
                        {
                            cWhereSql = cWhereSql + " AND " + filterSQL;
                        }
                    }
                   // cWhereSql = cWhereSql + filterSQL;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceBandFilter", ex, ""));
                }
                return cWhereSql;
            }

            public override string GetFilterSQL(ref Cms aWeb)
            {
                string cWhereSql = string.Empty;
                string cProcessInfo = "GetFilterSQL";
               // string cIndexDefinationName = "Price";
                try
                {
                    string cSelectedMinPriceBand = string.Empty;
                    string cSelectedMaxPriceBand = string.Empty;
                    string cPageIds = string.Empty;
                    string cIndexDefinationName = "Price";
                    int cnt = 0;

                    string cPriceBandRange = string.Empty;

                    if (aWeb.moRequest.Form["PriceBandFilter"] != null)
                    {
                        cPriceBandRange = aWeb.moRequest.Form["PriceBandFilter"];

                    }


                    if (cPriceBandRange != string.Empty)
                    {
                        
                        cWhereSql = cWhereSql + " nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey ";
                        cWhereSql = cWhereSql + " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 where cid.cDefinitionName='" + cIndexDefinationName + "' AND (";

                        string[] aPriceFilters = cPriceBandRange.Split(',');

                        if (aPriceFilters.Length != 0 & aPriceFilters.Length != default)
                        {
                            var loopTo = aPriceFilters.Length - 1;
                            for (cnt = 0; cnt <= loopTo; cnt++)
                            {

                                string[] priceRange = aPriceFilters[cnt].Split('-');
                                if (cnt > 0)
                                {
                                    cWhereSql = cWhereSql + " OR ";
                                }
                                if (priceRange[0] == priceRange[1])
                                {
                                    cWhereSql = cWhereSql + " (ci.nNumberValue > " + priceRange[0] + ") ";
                                }
                                else
                                {
                                    cWhereSql = cWhereSql + " (ci.nNumberValue between " + priceRange[0] + " and " + priceRange[1] + ") ";
                                   
                                }
                            }

                            cWhereSql = cWhereSql + "))";
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""));
                }
                return cWhereSql;
            }

            public override string GetFilterOrderByClause(ref Cms myWeb)
            {
                //cIndexDefinationName: Use name same as filter name, so that it will be easy to use in joins as per need.
                // this is specific to filter functionality
                // column which you are passing here is either 
                // - agreegate function
                // - column name from existing select query
                // - returns empty then if order by clause is not required.
                // -or an xpath/xquery too eg : return Convert(XML, cContentXmlBrief).value("/Content/StockCode[1]",'varchar(10)')


                string cFilterName = "PriceBand";
                if (myWeb.moRequest.Form["SortBy"] != null)
                {
                    if (myWeb.moRequest.Form["SortBy"] != string.Empty)
                    {
                        if (myWeb.moRequest.Form["SortBy"] == "asc" || myWeb.moRequest.Form["SortBy"] == "desc")
                        {
                            return " min(cii" + cFilterName + ".nNumberValue) " + Convert.ToString(myWeb.moRequest.Form["SortBy"]);
                        }
                    }
                }
                return " min(cii" + cFilterName + ".nNumberValue) asc";
                //return string.Empty;
            }

            public override string ContentIndexDefinationName(ref Cms aWeb)
            {
                return "Price";
            }

        }

    }
}