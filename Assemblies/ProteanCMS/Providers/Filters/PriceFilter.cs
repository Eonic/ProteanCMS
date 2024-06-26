﻿using System;
using System.Collections;
using System.Data;


using System.Data.SqlClient;
using System.Xml;
using Lucene.Net.Search;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean.Providers
{

    namespace Filters
    {

        public class PriceFilter
        {

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Global.Protean.Tools.Errors.ErrorEventArgs e);
            public void AddControl(ref Protean.Cms aWeb, ref XmlElement FilterConfig, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement oContentNode, string cWhereSql)
            {
                string cProcessInfo = "AddControl";
                try
                {
                    string sSql = "spGetPriceRange";
                    var arrParams = new Hashtable();
                    string sCotrolDisplayName = "Price Filter";
                    string cFilterTarget = string.Empty;

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
                    XmlElement oFilterElmt = null;
                    string className = string.Empty;
                    string cWhereQuery = string.Empty;

                    if (aWeb.moRequest.Form["MaxPrice"] is not null)
                    {

                        oMinPrice.Value = Convert.ToString(aWeb.moRequest.Form["MinPrice"]);
                        oMaxPrice.Value = Convert.ToString(aWeb.moRequest.Form["MaxPrice"]);

                    }
                    if (oContentNode.Attributes["filterTarget"] is not null)
                    {
                        cFilterTarget = oContentNode.Attributes["filterTarget"].Value;
                    }


                    if (FilterConfig.Attributes["name"] is not null)
                    {
                        sCotrolDisplayName = Convert.ToString(FilterConfig.Attributes["name"].Value);
                    }

                    if (oXform.Instance.SelectSingleNode("PageFilter") is not null)
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
                    arrParams.Add("MinPrice", FilterConfig.GetAttribute("fromPrice"));
                    arrParams.Add("MaxPrice", FilterConfig.GetAttribute("toPrice"));
                    arrParams.Add("Step", FilterConfig.GetAttribute("step"));
                    arrParams.Add("PageId", nPageId);
                    arrParams.Add("whereSql", cWhereSql);
                    arrParams.Add("FilterTarget", cFilterTarget);
                    using (SqlDataReader oDr = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams))
                    {
                        if (oDr is not null)
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
                        oSliderMinPrice.Value = FilterConfig.GetAttribute("fromPrice");
                        oMaxPriceLimit.Value = FilterConfig.GetAttribute("toPrice");

                        oSliderMaxPrice.Value = nMaxPRiceProduct.ToString();
                        // oMaxPrice.Value = FilterConfig.GetAttribute("toPrice")


                        oStep.Value = FilterConfig.GetAttribute("step");
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


                    oXform.addBind("MinPrice", "PriceFilter/@MinPrice", "false()", "string", ref oXform.model);
                    oXform.addBind("MaxPrice", "PriceFilter/@MaxPrice", "false()", "string", ref oXform.model);
                    oXform.addBind("MaxPriceLimit", "PriceFilter/@MaxPriceLimit", "false()", "string", ref oXform.model);
                    oXform.addBind("SliderMinPrice", "PriceFilter/@SliderMinPrice", "false()", "string", ref oXform.model);
                    oXform.addBind("SliderMaxPrice", "PriceFilter/@SliderMaxPrice", "false()", "string", ref oXform.model);
                    oXform.addBind("PriceStep", "PriceFilter/@PriceStep", "false()", "string", ref oXform.model);
                    oXform.addBind("PriceListCount", "PriceFilter/@PriceCountList", "false()", "string", ref oXform.model);
                    oXform.addBind("PriceFilter", "PriceFilter/@MaxPrice", "false()", "string", ref oXform.model);
                    // oXform.addBind("PriceTotalCount", "PriceFilter/@PriceTotalCount", "false()", "string", oXform.model)

                    oXform.addInput(ref oFromGroup, "MinPrice", true, "", "hidden");
                    oXform.addInput(ref oFromGroup, "MaxPrice", true, "", "hidden");
                    oXform.addInput(ref oFromGroup, "MaxPriceLimit", true, "", "hidden");
                    oXform.addInput(ref oFromGroup, "SliderMinPrice", true, "", "hidden");
                    oXform.addInput(ref oFromGroup, "SliderMaxPrice", true, "", "hidden");
                    oXform.addInput(ref oFromGroup, "PriceStep", true, "", "hidden");
                    oXform.addInput(ref oFromGroup, "PriceListCount", true, "", "hidden");
                    oXform.addInput(ref oFromGroup, "PriceFilter", true, "", "hidden");
                    oXform.addSubmit(ref oFromGroup, "", "Apply", "PriceFilter", "  btnPriceSubmit hidden", "");


                    if (aWeb.moRequest.Form["MinPrice"] is not null & !string.IsNullOrEmpty(aWeb.moRequest.Form["MinPrice"]))
                    {

                        // Dim sText As String = "From " + aWeb.moCart.mcCurrencySymbol + "" + oMinPrice.Value.Trim() + " to " + aWeb.moCart.mcCurrencySymbol + "" + oMaxPrice.Value.Trim()
                        string sText = "From " + oMinPrice.Value.Trim() + " to " + oMaxPrice.Value.Trim();
                        oXform.addSubmit(ref oFromGroup, sText, sText, "PriceFilter" + sText, "btnCrossForPrice filter-applied", "fa-times");

                    }

                    if (aWeb.moRequest.Form["MinPrice"] is not null & !string.IsNullOrEmpty(aWeb.moRequest.Form["MinPrice"]))
                    {
                        oXform.addInput(ref oFromGroup, "", false, sCotrolDisplayName, "histogramSliderMainDivPrice filter-selected");
                    }
                    else
                    {
                        oXform.addInput(ref oFromGroup, "", false, sCotrolDisplayName, "histogramSliderMainDivPrice");
                    }
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceFilter", ex, ""));
                }
            }


            public string ApplyFilter(ref Protean.Cms aWeb, ref string cWhereSql, ref Protean.xForm oXform, ref XmlElement oFromGroup, ref XmlElement FilterConfig, ref string cFilterTarget)
            {
                string cProcessInfo = "ApplyFilter";
                string cPriceCond = "";
                try
                {
                    // Dim priceRange() As String
                    string cDefinitionName = "Price";
                    string cSelectedMinPrice = string.Empty;
                    string cSelectedMaxPrice = string.Empty;
                    string cPageIds = string.Empty;
                    // cSelectedMinPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MinPrice").InnerText)
                    // cSelectedMaxPrice = Convert.ToString(oXform.Instance.SelectSingleNode("PriceFilter/@MaxPrice").InnerText)
                    cSelectedMinPrice = Convert.ToString(aWeb.moRequest.Form["MinPrice"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    cSelectedMaxPrice = Convert.ToString(aWeb.moRequest.Form["MaxPrice"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    bool bParentPageId = false;


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


            public string GetFilterSQL(ref Protean.Cms aWeb)
            {
                string cWhereSql = string.Empty;
                string cProcessInfo = "GetFilterSQL";
                string cIndexDefinationName = "Price";
                try
                {
                    string cSelectedMinPrice = "";
                    string cSelectedMaxPrice = "";
                    cSelectedMinPrice = Convert.ToString(aWeb.moRequest.Form["MinPrice"]).Replace(aWeb.moCart.mcCurrency, "");
                    cSelectedMaxPrice = Convert.ToString(aWeb.moRequest.Form["MaxPrice"]).Replace(aWeb.moCart.mcCurrency, "");
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

        }

    }
}