using Microsoft.VisualBasic.CompilerServices;
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

        public class PriceFilter : DefaultFilter
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
                    string sCotrolDisplayName = "Price Filter";
                    string cFilterTarget = string.Empty;
                    XmlElement oPriceGroup;
                    if (aWeb.moRequest.Form["MinPrice"] != null & aWeb.moRequest.Form["MinPrice"] != "")
                    { 
                        oPriceGroup=oXform.addGroup(ref oXform.moXformElmt, "PriceFilter", "pricefilter filter active-filter");
                    
                    }
                    else
                    {
                        oPriceGroup= oXform.addGroup(ref oXform.moXformElmt, "PriceFilter", "pricefilter filter");
                    }
                        
                    oFromGroup.AppendChild(oPriceGroup);
                    var oXml = oXform.moPageXML.CreateElement("PriceFilter");
                    var oMinPrice = oXform.moPageXML.CreateAttribute("MinPrice");
                    var oMaxPrice = oXform.moPageXML.CreateAttribute("MaxPrice");
                    var oMaxPriceLimit = oXform.moPageXML.CreateAttribute("MaxPriceLimit");
                    var oSliderMinPrice = oXform.moPageXML.CreateAttribute("SliderMinPrice");
                    var oSliderMaxPrice = oXform.moPageXML.CreateAttribute("SliderMaxPrice");
                    var oStep = oXform.moPageXML.CreateAttribute("PriceStep");
                    var oProductCountList = oXform.moPageXML.CreateAttribute("PriceCountList");
                    var oProductTotalCount = oXform.moPageXML.CreateAttribute("PriceTotalCount");
                   
                    string sProductCount = string.Empty;
                    int cnt = 0;
                    string cProductCountList = string.Empty;
                    string nPageId = string.Empty;
                    int nMaxPRiceProduct = 0;
                    int nMinPriceProduct = 0;
                    //XmlElement oFilterElmt = null;
                    string className = string.Empty;
                    string cWhereQuery = string.Empty;

                    if (aWeb.moRequest.Form["MaxPrice"] != null)
                    {

                        oMinPrice.Value = Convert.ToString(aWeb.moRequest.Form["MinPrice"]);
                        oMaxPrice.Value = Convert.ToString(aWeb.moRequest.Form["MaxPrice"]);

                    }
                    if (oContentNode.Attributes["filterTarget"] != null)
                    {
                        cFilterTarget = oContentNode.Attributes["filterTarget"].Value;
                    }

                    if (FilterConfig.Attributes["maxPriceLimit"] != null)
                    {
                        oMaxPriceLimit.Value = Convert.ToString(FilterConfig.Attributes["maxPriceLimit"].Value);
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

                    if (oMaxPriceLimit.Value != null)
                    {
                        if (oMaxPriceLimit.Value != string.Empty)
                        {
                            oMaxPriceLimit.Value = Convert.ToString(FilterConfig.Attributes["maxPriceLimit"].Value);
                        }
                        else
                        {
                            oMaxPriceLimit.Value = Convert.ToString(FilterConfig.GetAttribute("toPrice"));
                        }
                    }
                    else
                    {
                        oMaxPriceLimit.Value = FilterConfig.GetAttribute("toPrice");
                    }
                    arrParams.Add("MinPrice", FilterConfig.GetAttribute("fromPrice"));
                    arrParams.Add("MaxPrice", oMaxPriceLimit.Value);//FilterConfig.GetAttribute("toPrice"));
                    arrParams.Add("Step", FilterConfig.GetAttribute("step"));
                    arrParams.Add("PageId", nPageId);
                    arrParams.Add("whereSql", cWhereSql);
                    arrParams.Add("FilterTarget", cFilterTarget);
                    using (SqlDataReader oDr = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams))
                    {
                        if (oDr != null)
                        {
                            while (oDr.Read())
                            {
                                cnt = cnt + 1;
                                if (cnt == 1)
                                {
                                    nMinPriceProduct = Conversions.ToInteger(oDr["MinProductPrice"]);

                                }
                                nMaxPRiceProduct = Conversions.ToInteger(oDr["MaxProductPrice"]);
                                sProductCount = Convert.ToString(oDr["ContentCount"]);
                                cProductCountList = cProductCountList + cnt.ToString() + ":" + sProductCount + ",";
                            }

                        }


                        oSliderMinPrice.Value = Convert.ToString(nMinPriceProduct);// FilterConfig.GetAttribute("fromPrice");





                        oSliderMaxPrice.Value = nMaxPRiceProduct.ToString();
                        // oMaxPrice.Value = FilterConfig.GetAttribute("toPrice")

                        oStep.Value = FilterConfig.GetAttribute("step");
                        //oMinPrice.Value= Convert.ToString(nMinPriceProduct);
                        oXml.Attributes.Append(oMinPrice);
                        oXml.Attributes.Append(oMaxPrice);
                        oXml.Attributes.Append(oMaxPriceLimit);
                        oXml.Attributes.Append(oSliderMinPrice);
                        oXml.Attributes.Append(oSliderMaxPrice);
                        oXml.Attributes.Append(oStep);
                        oXml.Attributes.Append(oProductTotalCount);
                      


                        // 'Adding controls to the form like dropdown, radiobuttons
                        // 'If (oXml.InnerText <> String.Empty) Then
                        // '    priceFilterRange = oXform.addSelect(oFromGroup, "PriceFilter", False, sCotrolDisplayName, "checkbox filter-selected", ApperanceTypes.Full)
                        // 'Else
                        // '    priceFilterRange = oXform.addSelect(oFromGroup, "PriceFilter", False, sCotrolDisplayName, "checkbox", ApperanceTypes.Full)
                        // 'End If

                        // While oDr.Read
                        // Dim name As String = aWeb.moCart.mcCurrencySymbol + Convert.ToString(oDr("minPrice")) + "-" + aWeb.moCart.mcCurrencySymbol + Convert.ToString(oDr("maxPrice")) + " <span class='ProductCount'>" + Convert.ToString(oDr("ProductCount")) + "</span>"
                        // Dim value As String = Convert.ToString(oDr("minPrice")) + "-" + Convert.ToString(oDr("maxPrice"))

                        // oXform.add(priceFilterRange, name, value, True)
                        // End While
                    }


                    oProductCountList.Value = cProductCountList;
                    oXml.Attributes.Append(oProductCountList);

                    oXform.Instance.AppendChild(oXml);


                    oXform.addBind("MinPrice", "PriceFilter/@MinPrice", ref oXform.model, "false()", "string");
                    oXform.addBind("MaxPrice", "PriceFilter/@MaxPrice", ref oXform.model, "false()", "string");
                    oXform.addBind("MaxPriceLimit", "PriceFilter/@MaxPriceLimit", ref oXform.model, "false()", "string");
                    oXform.addBind("SliderMinPrice", "PriceFilter/@SliderMinPrice", ref oXform.model, "false()", "string");
                    oXform.addBind("SliderMaxPrice", "PriceFilter/@SliderMaxPrice", ref oXform.model, "false()", "string");
                    oXform.addBind("PriceStep", "PriceFilter/@PriceStep", ref oXform.model, "false()", "string");
                    oXform.addBind("PriceListCount", "PriceFilter/@PriceCountList", ref oXform.model, "false()", "string");
                    oXform.addBind("PriceFilter", "PriceFilter/@MaxPrice", ref oXform.model, "false()", "string");
                 
                    // oXform.addBind("PriceTotalCount", "PriceFilter/@PriceTotalCount", "false()", "string", oXform.model)

                    oXform.addInput(ref oPriceGroup, "MinPrice", true, "", "hidden");
                    oXform.addInput(ref oPriceGroup, "MaxPrice", true, "", "hidden");
                    oXform.addInput(ref oPriceGroup, "MaxPriceLimit", true, "", "hidden");
                    oXform.addInput(ref oPriceGroup, "SliderMinPrice", true, "", "hidden");
                    oXform.addInput(ref oPriceGroup, "SliderMaxPrice", true, "", "hidden");
                    oXform.addInput(ref oPriceGroup, "PriceStep", true, "", "hidden");
                    oXform.addInput(ref oPriceGroup, "PriceListCount", true, "", "hidden");
                    oXform.addInput(ref oPriceGroup, "PriceFilter", true, "", "hidden");
                    oXform.addSubmit(ref oPriceGroup, "", "Apply", "PriceFilter", "  btnPriceSubmit hidden", "");


                    if (aWeb.moRequest.Form["MinPrice"] != null & aWeb.moRequest.Form["MinPrice"] != "")
                    {

                        // Dim sText As String = "From " + aWeb.moCart.mcCurrencySymbol + "" + oMinPrice.Value.Trim() + " to " + aWeb.moCart.mcCurrencySymbol + "" + oMaxPrice.Value.Trim()
                        string sText = "From " + aWeb.moCart.mcCurrencySymbol + "" + oMinPrice.Value.Trim() + " to " + aWeb.moCart.mcCurrencySymbol + "" + oMaxPrice.Value.Trim()/*"From " + oMinPrice.Value.Trim() + " to " + oMaxPrice.Value.Trim()*/;
                        oXform.addSubmit(ref oFromGroup, "PriceFilter", sText, "PriceFilter", "remove-PriceFilter filter-applied", "fa-times");
                        oXform.addDiv(ref oFromGroup, "", "PriceClearAll", true);
                    }
                   
                    oXform.addInput(ref oPriceGroup, "", false, sCotrolDisplayName, "histogramSliderMainDivPrice histogramMain");

                    //if (aWeb.moRequest.Form["MinPrice"] != null & aWeb.moRequest.Form["MinPrice"] != "")
                    //{
                    //    oXform.addInput(ref oPriceGroup, "", false, sCotrolDisplayName, "histogramSliderMainDivPrice histogramMain filter-selected");
                    //}
                    //else
                    //{
                    //    oXform.addInput(ref oPriceGroup, "", false, sCotrolDisplayName, "histogramSliderMainDivPrice histogramMain");
                    //}
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""));
                }
            }

            public override string ApplyFilter(ref Cms aWeb, ref string cWhereSql, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget)
            {
                string cProcessInfo = "ApplyFilter";
                string cPriceCond = string.Empty;
                try
                {
                    // Dim priceRange() As String
                    //string cDefinitionName = "Price";
                    string cSelectedMinPrice = string.Empty;
                    string cSelectedMaxPrice = string.Empty;
                    string cPageIds = string.Empty;
                    // cSelectedMinPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MinPrice").InnerText)
                    // cSelectedMaxPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MaxPrice").InnerText)

                    if (aWeb.moRequest.Form["MinPrice"] != null)
                    {
                        cSelectedMinPrice = Convert.ToString(aWeb.moRequest.Form["MinPrice"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    }
                    if (aWeb.moRequest.Form["MaxPrice"] != null)
                    {
                        cSelectedMaxPrice = Convert.ToString(aWeb.moRequest.Form["MaxPrice"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    }
                    //bool bParentPageId = false;


                    // If (oXform.Instance.SelectSingleNode("PageFilter") IsNot Nothing) Then
                    // cPageIds = oXform.Instance.SelectSingleNode("PageFilter").InnerText

                    // End If

                    if (!string.IsNullOrEmpty(cSelectedMaxPrice))
                    {

                        if (string.IsNullOrEmpty(cSelectedMinPrice))
                        {
                            cSelectedMinPrice = "0";
                        }
                        // cPriceCond = " ci.nNumberValue between " + cSelectedMinPrice + " and " + cSelectedMaxPrice
                        if (!string.IsNullOrEmpty(cWhereSql))
                        {
                            cWhereSql = cWhereSql + " AND ";
                        }

                        // ElseIf (cPageIds = String.Empty) Then

                        // cPageIds = aWeb.moPageXml.SelectSingleNode("Page/@id").Value.ToString()
                        // cWhereSql = " nStructId IN (select nStructKey from tblContentStructure where nStructParId in (" & cPageIds & ")) AND "
                        // End If
                        cWhereSql = cWhereSql + GetFilterSQL(ref aWeb);




                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""));
                }
                return cWhereSql;
            }

            public override string GetFilterSQL(ref Cms aWeb)
            {
                string cWhereSql = string.Empty;
                string cProcessInfo = "GetFilterSQL";
                string cIndexDefinationName = "Price";
                try
                {
                    string cSelectedMinPrice = "";
                    string cSelectedMaxPrice = "";
                    //cSelectedMinPrice = Convert.ToString(aWeb.moRequest.Form["MinPrice"]).Replace(aWeb.moCart.mcCurrency, "");
                    //cSelectedMaxPrice = Convert.ToString(aWeb.moRequest.Form["MaxPrice"]).Replace(aWeb.moCart.mcCurrency, "");
                    if (aWeb.moRequest.Form["MinPrice"] != null)
                    {
                        cSelectedMinPrice = Convert.ToString(aWeb.moRequest.Form["MinPrice"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    }
                    if (aWeb.moRequest.Form["MaxPrice"] != null)
                    {
                        cSelectedMaxPrice = Convert.ToString(aWeb.moRequest.Form["MaxPrice"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    }
                    if (!string.IsNullOrEmpty(cSelectedMaxPrice))
                    {
                        cWhereSql = cWhereSql + " nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey ";
                        cWhereSql = cWhereSql + " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 where cid.cDefinitionName='" + cIndexDefinationName + "' AND (";

                        if (string.IsNullOrEmpty(cSelectedMinPrice))
                        {
                            cSelectedMinPrice = "0";
                        }
                        if (cSelectedMaxPrice.Contains("+"))
                        {
                            cWhereSql = cWhereSql + "ci.nNumberValue > " + cSelectedMinPrice + "))";
                        }
                        else
                        {
                            cWhereSql = cWhereSql + "ci.nNumberValue between " + cSelectedMinPrice + " and " + cSelectedMaxPrice + "))";
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

               
                string cIndexDefinationName = "Price";
                if(myWeb.moRequest.Form["SortBy"]!=null)
                {
                    if(myWeb.moRequest.Form["SortBy"]!=string.Empty)
                    {
                        if (myWeb.moRequest.Form["SortBy"] == "asc" || myWeb.moRequest.Form["SortBy"] == "desc")
                        {
                            return " min(cii" + cIndexDefinationName + ".nNumberValue) " + Convert.ToString(myWeb.moRequest.Form["SortBy"]);
                        }
                    }
                }
                return " min(cii" + cIndexDefinationName + ".nNumberValue) asc";
                //return string.Empty;
            }

        }

    }
}