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
                    var oStep = oXform.moPageXML.CreateAttribute("PriceBandStep");
                   
                    string sProductCount = string.Empty;
                    int cnt = 0;
                    string cProductCountList = string.Empty;
                    string nPageId = string.Empty;
                    int nMaxPriceBandProduct = 0;
                    int nMinPriceBandProduct = 0;
                    //XmlElement oFilterElmt = null;
                    string className = string.Empty;
                    string cWhereQuery = string.Empty;

                    //if (aWeb.moRequest.Form["MaxPriceBand"] != null)
                    //{

                    //    oMinPriceBand.Value = Convert.ToString(aWeb.moRequest.Form["MinPriceBand"]);
                    //    oMaxPriceBand.Value = Convert.ToString(aWeb.moRequest.Form["MaxPriceBand"]);

                    //}
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
                    arrParams.Add("MaxPrice", oMaxPriceBandLimit.Value);//FilterConfig.GetAttribute("toPriceBand"));
                    arrParams.Add("Step", FilterConfig.GetAttribute("step"));
                    arrParams.Add("PageId", nPageId);
                    arrParams.Add("whereSql", cWhereSql);
                    arrParams.Add("FilterTarget", cFilterTarget);


                    //Adding controls to the form like dropdown, radiobuttons
                      //   if (oXml.InnerText != String.Empty)
                       // {
                        priceBandFilterSelect = oXform.addSelect(ref oPriceBandGroup, "PriceBandFilter", false, sCotrolDisplayName, "checkbox SubmitPriceBandFilter", ApperanceTypes.Full);
                      //  }
                        //else
                        //{
                        //priceBandFilterSelect = oXform.addSelect(ref oPriceBandGroup, "PriceBandFilter", false, sCotrolDisplayName, "checkbox", ApperanceTypes.Full);
                        //}
                          
                      

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

                                string sText =  aWeb.moCart.mcCurrencySymbol + "" + nMinPriceBandProduct + " - " + aWeb.moCart.mcCurrencySymbol + "" + nMaxPriceBandProduct + " <span class='badge ms-2' id='ProductCount'>" + Convert.ToString(oDr["ContentCount"]) + "</span>"/*"From " + oMinPriceBand.Value.Trim() + " to " + oMaxPriceBand.Value.Trim()*/;
                                string value = Convert.ToString(nMinPriceBandProduct)+"-"+ Convert.ToString(nMaxPriceBandProduct);

                                oXform.addOption(ref priceBandFilterSelect, sText, value, true);
                            }

                        }


                        





                        

                        oStep.Value = FilterConfig.GetAttribute("step");
                        //oMinPriceBand.Value= Convert.ToString(nMinPriceBandProduct);
                        oXml.Attributes.Append(oMinPriceBand);
                        oXml.Attributes.Append(oMaxPriceBand);
                        oXml.Attributes.Append(oMaxPriceBandLimit);
                        oXml.Attributes.Append(oStep);
                       



                        
                         
                    }


                   

                    oXform.Instance.AppendChild(oXml);


                    //oXform.addBind("MinPriceBand", "PriceBandFilter/@MinPriceBand", ref oXform.model, "false()", "string");
                    //oXform.addBind("MaxPriceBand", "PriceBandFilter/@MaxPriceBand", ref oXform.model, "false()", "string");
                    oXform.addBind("MaxPriceBandLimit", "PriceBandFilter/@MaxPriceBandLimit", ref oXform.model, "false()", "string");
                    oXform.addBind("PriceBandStep", "PriceBandFilter/@PriceBandStep", ref oXform.model, "false()", "string");
                     oXform.addBind("PriceBandFilter", "PriceBandFilter", ref oXform.model, "false()", "string");

                 
                    //oXform.addInput(ref oPriceBandGroup, "MinPriceBand", true, "", "hidden");
                    //oXform.addInput(ref oPriceBandGroup, "MaxPriceBand", true, "", "hidden");
                    oXform.addInput(ref oPriceBandGroup, "MaxPriceBandLimit", true, "", "hidden");
                    oXform.addInput(ref oPriceBandGroup, "PriceBandStep", true, "", "hidden");
                    oXform.addInput(ref oPriceBandGroup, "PriceBandFilter", true, "", "hidden");
                    oXform.addSubmit(ref oPriceBandGroup, "", "Apply", "PriceBandFilter", "  btnPriceBandSubmit hidden", "");


                    if (aWeb.moRequest.Form["PriceBandFilter"] != null )
                    {

                        // Dim sText As String = "From " + aWeb.moCart.mcCurrencySymbol + "" + oMinPriceBand.Value.Trim() + " to " + aWeb.moCart.mcCurrencySymbol + "" + oMaxPriceBand.Value.Trim()
                        string sText = "From " + aWeb.moCart.mcCurrencySymbol + "" + oMinPriceBand.Value.Trim() + " to " + aWeb.moCart.mcCurrencySymbol + "" + oMaxPriceBand.Value.Trim()/*"From " + oMinPriceBand.Value.Trim() + " to " + oMaxPriceBand.Value.Trim()*/;
                        oXform.addSubmit(ref oFromGroup, "PriceBandFilter", sText, "PriceBandFilter", "remove-PriceBandFilter filter-applied", "fa-times");
                        oXform.addDiv(ref oFromGroup, "", "PriceBandClearAll", true);
                    }

                   

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
                    string cIndexDefinationName = "Price";

                   
                    string cPriceBandRange = string.Empty;

                    if (oXform.Instance.SelectSingleNode("PriceBandFilter") != null)
                    {
                        cPriceBandRange = oXform.Instance.SelectSingleNode("PriceBandFilter").InnerText;

                    }
                    cPriceBandRange = cPriceBandRange.Replace(",", "");
                    string[] priceRange = cPriceBandRange.Split('-');

                    
                    if (!string.IsNullOrEmpty(cWhereSql))
                    {
                        cWhereSql = " AND ";
                    }

                    cWhereSql = cWhereSql + " nContentKey in ( Select distinct ci.nContentId from tblContentIndex ci inner join tblContentIndexDef cid on cid.nContentIndexDefKey=ci.nContentIndexDefinitionKey ";
                    cWhereSql = cWhereSql + " inner join tblAudit ca on ca.nAuditKey=cid.nAuditId and nStatus=1 where cid.cDefinitionName='" + cIndexDefinationName + "' AND (";
                    cWhereSql = cWhereSql + "ci.nNumberValue between " + priceRange[0] + " and " + priceRange[1] + "))";
                    
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(cProcessInfo, "PriceBandFilter", ex, ""));
                }
                return cWhereSql;
            }

            public override string GetFilterSQL(ref Cms aWeb)
            {
               
                return "";
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

            public override string ContentIndexDefinationName(ref Cms aWeb)
            {
                return "Price";
            }

        }

    }
}