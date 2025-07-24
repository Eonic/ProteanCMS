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
                    if (aWeb.moRequest.Form["MinPriceBand"] != null & aWeb.moRequest.Form["MinPriceBand"] != "")
                    {
                        oPriceBandGroup = oXform.addGroup(ref oXform.moXformElmt, "PriceBandFilter", "PriceBandfilter filter active-filter");

                    }
                    else
                    {
                        oPriceBandGroup = oXform.addGroup(ref oXform.moXformElmt, "PriceBandFilter", "PriceBandfilter filter");
                    }

                    oFromGroup.AppendChild(oPriceBandGroup);
                    var oXml = oXform.moPageXML.CreateElement("PriceBandFilter");
                    var oMinPriceBand = oXform.moPageXML.CreateAttribute("MinPriceBand");
                    var oMaxPriceBand = oXform.moPageXML.CreateAttribute("MaxPriceBand");
                    var oMaxPriceBandLimit = oXform.moPageXML.CreateAttribute("MaxPriceBandLimit");
                    var oSliderMinPriceBand = oXform.moPageXML.CreateAttribute("SliderMinPriceBand");
                    var oSliderMaxPriceBand = oXform.moPageXML.CreateAttribute("SliderMaxPriceBand");
                    var oStep = oXform.moPageXML.CreateAttribute("PriceBandStep");
                    var oProductCountList = oXform.moPageXML.CreateAttribute("PriceBandCountList");
                    var oProductTotalCount = oXform.moPageXML.CreateAttribute("PriceBandTotalCount");

                    string sProductCount = string.Empty;
                    int cnt = 0;
                    string cProductCountList = string.Empty;
                    string nPageId = string.Empty;
                    int nMaxPriceBandProduct = 0;
                    int nMinPriceBandProduct = 0;
                    //XmlElement oFilterElmt = null;
                    string className = string.Empty;
                    string cWhereQuery = string.Empty;

                    if (aWeb.moRequest.Form["MaxPriceBand"] != null)
                    {

                        oMinPriceBand.Value = Convert.ToString(aWeb.moRequest.Form["MinPriceBand"]);
                        oMaxPriceBand.Value = Convert.ToString(aWeb.moRequest.Form["MaxPriceBand"]);

                    }
                    if (oContentNode.Attributes["filterTarget"] != null)
                    {
                        cFilterTarget = oContentNode.Attributes["filterTarget"].Value;
                    }

                    if (FilterConfig.Attributes["maxPriceBandLimit"] != null)
                    {
                        oMaxPriceBandLimit.Value = Convert.ToString(FilterConfig.Attributes["maxPriceBandLimit"].Value);
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
                            oMaxPriceBandLimit.Value = Convert.ToString(FilterConfig.Attributes["maxPriceBandLimit"].Value);
                        }
                        else
                        {
                            oMaxPriceBandLimit.Value = Convert.ToString(FilterConfig.GetAttribute("toPriceBand"));
                        }
                    }
                    else
                    {
                        oMaxPriceBandLimit.Value = FilterConfig.GetAttribute("toPriceBand");
                    }
                    arrParams.Add("MinPrice", FilterConfig.GetAttribute("fromPriceBand"));
                    arrParams.Add("MaxPrice", oMaxPriceBandLimit.Value);//FilterConfig.GetAttribute("toPriceBand"));
                    arrParams.Add("Step", FilterConfig.GetAttribute("step"));
                    arrParams.Add("PageId", nPageId);
                    arrParams.Add("whereSql", cWhereSql);
                    arrParams.Add("FilterTarget", cFilterTarget);


                    //Adding controls to the form like dropdown, radiobuttons
                         if (oXml.InnerText != String.Empty)
                        {
                        priceBandFilterSelect = oXform.addSelect(ref oPriceBandGroup, "PriceBandFilter", false, sCotrolDisplayName, "checkbox filter-selected", ApperanceTypes.Full);
                        }
                        else
                        {
                        priceBandFilterSelect = oXform.addSelect(ref oPriceBandGroup, "PriceBandFilter", false, sCotrolDisplayName, "checkbox", ApperanceTypes.Full);
                        }
                          
                      

                    using (SqlDataReader oDr = aWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.StoredProcedure, arrParams))
                    {
                        if (oDr != null)
                        {
                            while (oDr.Read())
                            {
                                nMinPriceBandProduct = Conversions.ToInteger(oDr["MinProductPrice"]);

                                nMaxPriceBandProduct = Conversions.ToInteger(oDr["MaxProductPrice"]);
                                sProductCount = Convert.ToString(oDr["ContentCount"]);
                                cProductCountList = cProductCountList + cnt.ToString() + ":" + sProductCount + ",";

                                string sText = "From " + aWeb.moCart.mcCurrencySymbol + "" + nMinPriceBandProduct + " to " + aWeb.moCart.mcCurrencySymbol + "" + nMaxPriceBandProduct /*"From " + oMinPriceBand.Value.Trim() + " to " + oMaxPriceBand.Value.Trim()*/;
                                string value = Convert.ToString(nMinPriceBandProduct+'-'+ nMaxPriceBandProduct);

                                oXform.addOption(ref priceBandFilterSelect, sText, value, true);
                            }

                        }


                        





                        oSliderMaxPriceBand.Value = nMaxPriceBandProduct.ToString();
                        // oMaxPriceBand.Value = FilterConfig.GetAttribute("toPriceBand")

                        oStep.Value = FilterConfig.GetAttribute("step");
                        //oMinPriceBand.Value= Convert.ToString(nMinPriceBandProduct);
                        oXml.Attributes.Append(oMinPriceBand);
                        oXml.Attributes.Append(oMaxPriceBand);
                        oXml.Attributes.Append(oMaxPriceBandLimit);
                        oXml.Attributes.Append(oStep);
                        oXml.Attributes.Append(oProductTotalCount);



                        
                         
                    }


                    oProductCountList.Value = cProductCountList;
                    oXml.Attributes.Append(oProductCountList);

                    oXform.Instance.AppendChild(oXml);


                    oXform.addBind("MinPriceBand", "PriceBandFilter/@MinPriceBand", ref oXform.model, "false()", "string");
                    oXform.addBind("MaxPriceBand", "PriceBandFilter/@MaxPriceBand", ref oXform.model, "false()", "string");
                    oXform.addBind("MaxPriceBandLimit", "PriceBandFilter/@MaxPriceBandLimit", ref oXform.model, "false()", "string");
                    oXform.addBind("PriceBandStep", "PriceBandFilter/@PriceBandStep", ref oXform.model, "false()", "string");
                    oXform.addBind("PriceBandListCount", "PriceBandFilter/@PriceBandCountList", ref oXform.model, "false()", "string");
                    oXform.addBind("PriceBandFilter", "PriceBandFilter/@MaxPriceBand", ref oXform.model, "false()", "string");

                    // oXform.addBind("PriceBandTotalCount", "PriceBandFilter/@PriceBandTotalCount", "false()", "string", oXform.model)

                    oXform.addInput(ref oPriceBandGroup, "MinPriceBand", true, "", "hidden");
                    oXform.addInput(ref oPriceBandGroup, "MaxPriceBand", true, "", "hidden");
                    oXform.addInput(ref oPriceBandGroup, "MaxPriceBandLimit", true, "", "hidden");
                    oXform.addInput(ref oPriceBandGroup, "PriceBandStep", true, "", "hidden");
                    oXform.addInput(ref oPriceBandGroup, "PriceBandListCount", true, "", "hidden");
                    oXform.addInput(ref oPriceBandGroup, "PriceBandFilter", true, "", "hidden");
                    oXform.addSubmit(ref oPriceBandGroup, "", "Apply", "PriceBandFilter", "  btnPriceBandSubmit hidden", "");


                    if (aWeb.moRequest.Form["MinPriceBand"] != null & aWeb.moRequest.Form["MinPriceBand"] != "")
                    {

                        // Dim sText As String = "From " + aWeb.moCart.mcCurrencySymbol + "" + oMinPriceBand.Value.Trim() + " to " + aWeb.moCart.mcCurrencySymbol + "" + oMaxPriceBand.Value.Trim()
                        string sText = "From " + aWeb.moCart.mcCurrencySymbol + "" + oMinPriceBand.Value.Trim() + " to " + aWeb.moCart.mcCurrencySymbol + "" + oMaxPriceBand.Value.Trim()/*"From " + oMinPriceBand.Value.Trim() + " to " + oMaxPriceBand.Value.Trim()*/;
                        oXform.addSubmit(ref oFromGroup, "PriceBandFilter", sText, "PriceBandFilter", "remove-PriceBandFilter filter-applied", "fa-times");
                        oXform.addDiv(ref oFromGroup, "", "PriceBandClearAll", true);
                    }

                    oXform.addInput(ref oPriceBandGroup, "", false, sCotrolDisplayName, "histogramSliderMainDivPriceBand histogramMain");

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
                    string cSelectedMinPriceBand = string.Empty;
                    string cSelectedMaxPriceBand = string.Empty;
                    string cPageIds = string.Empty;
                  
                    if (aWeb.moRequest.Form["MinPriceBand"] != null)
                    {
                        cSelectedMinPriceBand = Convert.ToString(aWeb.moRequest.Form["MinPriceBand"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    }
                    if (aWeb.moRequest.Form["MaxPriceBand"] != null)
                    {
                        cSelectedMaxPriceBand = Convert.ToString(aWeb.moRequest.Form["MaxPriceBand"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    }
                   
                    if (!string.IsNullOrEmpty(cSelectedMaxPriceBand))
                    {

                        if (string.IsNullOrEmpty(cSelectedMinPriceBand))
                        {
                            cSelectedMinPriceBand = "0";
                        }
                        if (!string.IsNullOrEmpty(cWhereSql))
                        {
                            cWhereSql = cWhereSql + " AND ";
                        }

                        // ElseIf (cPageIds = String.Empty) Then

                       
                        cWhereSql = cWhereSql + GetFilterSQL(ref aWeb);




                    }
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
                string cIndexDefinationName = "PriceBand";
                try
                {
                    string cSelectedMinPriceBand = "";
                    string cSelectedMaxPriceBand = "";
                    if (aWeb.moRequest.Form["MinPriceBand"] != null)
                    {
                        cSelectedMinPriceBand = Convert.ToString(aWeb.moRequest.Form["MinPriceBand"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    }
                    if (aWeb.moRequest.Form["MaxPriceBand"] != null)
                    {
                        cSelectedMaxPriceBand = Convert.ToString(aWeb.moRequest.Form["MaxPriceBand"]).Replace(aWeb.moCart.mcCurrencySymbol, "");
                    }
                    if (!string.IsNullOrEmpty(cSelectedMaxPriceBand))
                    {
                        cWhereSql = cWhereSql + " nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey ";
                        cWhereSql = cWhereSql + " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 where cid.cDefinitionName='" + cIndexDefinationName + "' AND (";

                        if (string.IsNullOrEmpty(cSelectedMinPriceBand))
                        {
                            cSelectedMinPriceBand = "0";
                        }
                        if (cSelectedMaxPriceBand.Contains("+"))
                        {
                            cWhereSql = cWhereSql + "ci.nNumberValue > " + cSelectedMinPriceBand + "))";
                        }
                        else
                        {
                            cWhereSql = cWhereSql + "ci.nNumberValue between " + cSelectedMinPriceBand + " and " + cSelectedMaxPriceBand + "))";
                        }

                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceBandFilter", ex, ""));
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


                string cIndexDefinationName = "PriceBand";
                if (myWeb.moRequest.Form["SortBy"] != null)
                {
                    if (myWeb.moRequest.Form["SortBy"] != string.Empty)
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