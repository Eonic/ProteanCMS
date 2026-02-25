// ***********************************************************************
// $Library:     protean.cms.adminXforms
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.com)
// &Website:     eonic.com
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2022 Eonic Digital LLP.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin
        {
            public partial class AdminXforms : xForm, IDisposable
            {

                public XmlElement xFrmGetReport(string cReportName)
                {

                    string cProcessInfo = "";

                    try
                    {

                        // Replace Spaces with hypens
                        cReportName = cReportName.Replace(" ", "-");
                        string reportsFolder = "/xforms/Reports/";
                        if (myWeb.bs5)
                            reportsFolder = "/admin/xforms/reports/";

                        if (!base.load(reportsFolder + cReportName + ".xml", myWeb.maCommonFolders))
                        {
                            // show xform load error message
                        }

                        XmlElement queryNode = (XmlElement)moXformElmt.SelectSingleNode("descendant-or-self::Query");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }
                        else if (queryNode.GetAttribute("autoSubmit").ToLower() == "true")
                        {
                            base.validate();
                        }


                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmGetReport", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement GetAllMenuMetaDetails()
                {
                    string cProcessInfo = "";
                    try
                    {
                        // get menu
                        var oMenuElmt = myWeb.GetStructureXML("Site");
                        oMenuElmt = myWeb.moDbHelper.GetMenuMetaTitleDescriptionDetailsXml(oMenuElmt);
                        return oMenuElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "GetAllMenuMetaDetails", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement GetAllHiddenProducts(int page, int pageSize)
                {
                    try
                    {
                        DataTable dt = myWeb.moDbHelper.GetAllHiddenProducts();
                        if (!dt.Columns.Contains("ProductUrl"))
                        {
                            dt.Columns.Add("ProductUrl", typeof(string));
                        }
                        // Load rewriteMaps.config
                        HashSet<string> mapUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        string mapPath = myWeb.goServer.MapPath("/rewriteMaps.config");
                        if (File.Exists(mapPath))
                        {
                            XmlDocument mapDoc = new XmlDocument();
                            mapDoc.Load(mapPath);

                            foreach (XmlNode n in mapDoc.SelectNodes("//add[@key]"))
                            {
                                string key = n.Attributes["key"]?.Value?.Trim();
                                string val = n.Attributes["value"]?.Value?.Trim();

                                if (!string.IsNullOrEmpty(key))
                                    mapUrls.Add(key.Trim('/').ToLower());

                                if (!string.IsNullOrEmpty(val))
                                    mapUrls.Add(val.Trim('/').ToLower());
                            }
                        }

                        // Filter rows
                        List<DataRow> result = new List<DataRow>();
                        string[] prefixs = goConfig["DetailPrefix"].Split(',');
                        string thisPrefix = "";
                        var loopTo = prefixs.Length - 1;
                        thisPrefix = prefixs[0].Substring(0, prefixs[0].IndexOf("/"));
                        foreach (DataRow row in dt.Rows)
                        {

                            string sContentName = (row["cContentName"]?.ToString() ?? "").Trim().ToLower();
                            string sProductUrl = "";

                            sProductUrl = "/" + thisPrefix + "/" + sContentName.Replace(" ", "-");

                            row["ProductUrl"] = sProductUrl;

                            if (string.IsNullOrWhiteSpace(sProductUrl))
                                continue;

                            string normalized = sProductUrl.Trim('/');

                            if (!mapUrls.Contains(normalized))
                                result.Add(row);
                        }

                        int total = result.Count;
                        int startIndex = (page - 1) * pageSize;
                        int endIndex = Math.Min(startIndex + pageSize, total);

                        // Create XML
                        XmlDocument xmlDoc = new XmlDocument();
                        XmlElement root = xmlDoc.CreateElement("Products");
                        xmlDoc.AppendChild(root);

                        // Params
                        XmlElement paramsNode = xmlDoc.CreateElement("Params");
                        root.AppendChild(paramsNode);

                        XmlElement p1 = xmlDoc.CreateElement("Param");
                        p1.SetAttribute("name", "Page");
                        p1.SetAttribute("value", page.ToString());
                        paramsNode.AppendChild(p1);

                        XmlElement p2 = xmlDoc.CreateElement("Param");
                        p2.SetAttribute("name", "PageSize");
                        p2.SetAttribute("value", pageSize.ToString());
                        paramsNode.AppendChild(p2);

                        XmlElement totalNode = xmlDoc.CreateElement("Param");
                        totalNode.SetAttribute("name", "Total");
                        totalNode.SetAttribute("value", total.ToString());
                        paramsNode.AppendChild(totalNode);

                        for (int i = startIndex; i < endIndex; i++)
                        {
                            DataRow row = result[i];

                            XmlElement item = xmlDoc.CreateElement("Product");

                            foreach (DataColumn col in row.Table.Columns)
                            {
                                XmlElement node = xmlDoc.CreateElement(col.ColumnName);
                                node.InnerText = row[col]?.ToString() ?? "";
                                item.AppendChild(node);
                            }

                            root.AppendChild(item);
                        }

                        return root;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetAllHiddenProducts", ex, ""));
                        return null;
                    }
                }


                public XmlElement xFrmCartOrderDownloads()
                {
                    try
                    {
                        // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                        XmlElement oFrmElmt;
                        var oTempInstance = moPageXML.CreateElement("instance");
                        //bool bCascade = false;
                        //string cProcessInfo = "";



                        base.NewFrm("CartActivity");
                        base.submission("SeeReport", ReportExportPath, "get", "");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");

                        var oContElmt = base.addGroup(ref oFrmElmt, "1", "xFormContainer");

                        var oGrp0Elmt = base.addGroup(ref oContElmt, "Criteria", "xFormContainer", "Criteria");

                        base.Instance.InnerXml = "<Criteria ewCmd=\"CartDownload\" output=\"csv\"><dBegin>" + XmlDate(DateTime.Now.AddMonths(-1), false) + "</dBegin><dEnd>" + XmlDate(DateTime.Now.AddDays(1d), false) + "</dEnd>" + "<cCurrencySymbol/><cOrderType>Order</cOrderType><cOrderStage>6</cOrderStage>" + "</Criteria>";

                        XmlElement argoBindParent = null;
                        base.addBind("dBegin", "Criteria/dBegin", oBindParent: ref argoBindParent, "true()");
                        XmlElement argoBindParent1 = null;
                        base.addBind("dEnd", "Criteria/dEnd", oBindParent: ref argoBindParent1, "true()");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", sType: "string", oBindParent: ref argoBindParent2);
                        XmlElement argoBindParent3 = null;
                        base.addBind("cOrderType", "Criteria/cOrderType", oBindParent: ref argoBindParent3, "true()", "string");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cOrderStage", "Criteria/cOrderStage", oBindParent: ref argoBindParent4, "true()", "string");
                        XmlElement argoBindParent5 = null;
                        base.addBind("format", "Criteria/@output", oBindParent: ref argoBindParent5, "false()", "string");
                        XmlElement argoBindParent6 = null;
                        base.addBind("ewCmd", "Criteria/@ewCmd", oBindParent: ref argoBindParent6, "false()", "string");

                        base.addInput(ref oGrp0Elmt, "ewCmd", true, "ewCmd", "hidden");

                        base.addInput(ref oGrp0Elmt, "dBegin", true, "From", "calendarTime");
                        base.addInput(ref oGrp0Elmt, "dEnd", true, "To", "calendarTime");
                        XmlElement oSel1;
                        //oSel1 = base.addSelect1(ref oGrp0Elmt, "cCurrencySymbol", true, "Currency");
                        if (myWeb.moConfig["Quote"] != "on")
                        {
                            oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderType", true, "Cart Type");
                            base.addOption(ref oSel1, "Order", "Order");
                            base.addOption(ref oSel1, "Quote", "Quote");
                        }
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderStage", true, "Cart Type");
                        int i = 0;
                        foreach (Cms.Cart.cartProcess nProcess in Enum.GetValues(typeof(Cms.Cart.cartProcess)))
                        {
                            base.addOption(ref oSel1, nProcess.ToString(), i.ToString());
                            i = i + 1;
                        }

                        oSel1 = base.addSelect1(ref oGrp0Elmt, "format", true, "Output");
                        base.addOption(ref oSel1, "Excel", "excel");
                        base.addOption(ref oSel1, "CSV", "csv");
                        base.addOption(ref oSel1, "XML", "xml");
                        if (myWeb.moConfig["Debug"] == "on")
                        {
                            base.addOption(ref oSel1, "Raw XML", "rawxml");
                        }
                        base.addSubmit(ref oGrp0Elmt, "Results", "Download Spreadsheet", "Results");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmCartActivity", ex, "", "", gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCartActivity()
                {
                    try
                    {
                        // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                        XmlElement oFrmElmt;

                        var oTempInstance = moPageXML.CreateElement("instance");
                        //bool bCascade = false;
                        //string cProcessInfo = "";

                        base.NewFrm("CartActivity");

                        base.submission("SeeReport", "", "post", "");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");

                        var oContElmt = base.addGroup(ref oFrmElmt, "1", "xFormContainer");

                        var oGrp0Elmt = base.addGroup(ref oContElmt, "Criteria", "xFormContainer", "Criteria");

                        base.Instance.InnerXml = "<Criteria><dBegin>" + XmlDate(DateTime.Now.AddMonths(-1), false) + "</dBegin><dEnd>" + XmlDate(DateTime.Now.AddDays(1d), false) + "</dEnd><bSplit>0</bSplit>" + "<cProductType/><nProductId>0</nProductId><cCurrencySymbol/>" + "<nOrderStatus>6,9,17</nOrderStatus><cOrderType>Order</cOrderType>" + "</Criteria>";

                        base.addInput(ref oGrp0Elmt, "dBegin", true, "From", "calendar");
                        base.addInput(ref oGrp0Elmt, "dEnd", true, "To", "calendar");
                        var oSel1 = base.addSelect1(ref oGrp0Elmt, "cCurrencySymbol", true, "Currency");
                        base.addOption(ref oSel1, "All", "");
                        base.addOption(ref oSel1, "GBP", "GBP");

                        if (myWeb.moConfig["Quote"].ToLower() == "on")
                        {
                            oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderType", true, "Cart Type");
                            base.addOption(ref oSel1, "Orders", "Order");
                            base.addOption(ref oSel1, "Quotes", "Quote");
                        }

                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nOrderStatus", true, "Cart Type");
                        base.addOption(ref oSel1, "Orders,In Progress,Shipped", "6,9,17");
                        base.addOption(ref oSel1, "Refunds", "7");
                        base.addOption(ref oSel1, "Payment Failed", "5");

                        // Lets get the content types if we have more than 1

                        // Dim oDR As SqlDataReader
                        string cSQL = "SELECT tblContent.cContentSchemaName" + " FROM tblCartItem LEFT OUTER JOIN" + " tblContent ON tblCartItem.nItemId = tblContent.nContentKey" + " GROUP BY tblContent.cContentSchemaName" + " ORDER BY tblContent.cContentSchemaName";
                        using (var oDR = moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                        {

                            if (oDR.VisibleFieldCount > 1)
                            {

                                oSel1 = base.addSelect1(ref oGrp0Elmt, "bSplit", true, "Group By Product Type");
                                base.addOption(ref oSel1, "Yes", "1");
                                base.addOption(ref oSel1, "No", "0");

                                oSel1 = base.addSelect1(ref oGrp0Elmt, "cProductType", true, "Select Product Type");
                                base.addOption(ref oSel1, "All", "");
                                base.addOptionsFromSqlDataReader(oSel1, oDR, "cContentSchemaName", "cContentSchemaName");

                            }
                        }


                        // Gets full list of products

                        cSQL = " SELECT tblContent.cContentName, tblContent.nContentKey" + " FROM tblCartItem LEFT OUTER JOIN" + " tblContent ON tblCartItem.nItemId = tblContent.nContentKey" + " GROUP BY tblContent.cContentName, tblContent.nContentKey" + " ORDER BY tblContent.cContentName";
                        using (var oDR = moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                        {

                            oSel1 = base.addSelect1(ref oGrp0Elmt, "nProductId", true, "Single Product");
                            base.addOption(ref oSel1, "All", "0");
                            base.addOptionsFromSqlDataReader(oSel1, oDR, "cContentName", "nContentKey");

                            base.addSubmit(ref oGrp0Elmt, "Results", "See Results", "Results");

                            XmlElement argoBindParent = null;
                            base.addBind("dBegin", "Criteria/dBegin", oBindParent: ref argoBindParent, "true()");
                            XmlElement argoBindParent1 = null;
                            base.addBind("dEnd", "Criteria/dEnd", oBindParent: ref argoBindParent1, "true()");
                            XmlElement argoBindParent2 = null;
                            base.addBind("bSplit", "Criteria/bSplit", sType: "number", oBindParent: ref argoBindParent2);
                            XmlElement argoBindParent3 = null;
                            base.addBind("cProductType", "Criteria/cProductType", sType: "string", oBindParent: ref argoBindParent3);
                            XmlElement argoBindParent4 = null;
                            base.addBind("nProductId", "Criteria/nProductId", sType: "number", oBindParent: ref argoBindParent4);
                            XmlElement argoBindParent5 = null;
                            base.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", sType: "string", oBindParent: ref argoBindParent5);
                            XmlElement argoBindParent6 = null;
                            base.addBind("nOrderStatus", "Criteria/nOrderStatus", sType: "string", oBindParent: ref argoBindParent6);
                            if (myWeb.moConfig["Quote"].ToLower() == "on")
                            {
                                XmlElement argoBindParent7 = null;
                                base.addBind("cOrderType", "Criteria/cOrderType", oBindParent: ref argoBindParent7, "true()", "string");
                            }

                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmCartActivity", ex, "", "", gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCartActivityDrillDown()
                {
                    try
                    {
                        // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                        XmlElement oFrmElmt;

                        var oTempInstance = moPageXML.CreateElement("instance");
                        //bool bCascade = false;
                        //string cProcessInfo = "";

                        base.NewFrm("CartActivityDrilldown");
                        base.submission("SeeReport", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");

                        var oGrp0Elmt = base.addGroup(ref oFrmElmt, "Criteria", sLabel: "Criteria");


                        base.Instance.InnerXml = "<Criteria>" + "<nYear>" + DateTime.Now.Year + "</nYear>" + "<nMonth>" + DateTime.Now.Month + "</nMonth>" + "<nDay>0</nDay>" + "<cGrouping>Page</cGrouping><cCurrencySymbol/>" + "<nOrderStatus>6,9,17</nOrderStatus>" + "<cOrderType>Order</cOrderType>" + "</Criteria>";
                        // Year
                        var oSel1 = base.addSelect1(ref oGrp0Elmt, "nYear", true, "Year", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 2000, loopTo = DateTime.Now.Year; i <= loopTo; i++)
                            base.addOption(ref oSel1, i.ToString(), i.ToString());
                        // Month
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nMonth", true, "Month", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 1; i <= 12; i++)
                            base.addOption(ref oSel1, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i), i.ToString());
                        // Day
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nDay", true, "Day", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 1; i <= 31; i++)
                            base.addOption(ref oSel1, i.ToString(), i.ToString());
                        // Grouping
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cGrouping", true, "Grouping", "required");
                        base.addOption(ref oSel1, "By Page", "Page");
                        base.addOption(ref oSel1, "By Group", "Group");
                        // Currency
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cCurrencySymbol", true, "Currency");
                        base.addOption(ref oSel1, "All/None", "");
                        base.addOption(ref oSel1, "GBP", "£");
                        // OrderStatus
                        base.addInput(ref oGrp0Elmt, "nOrderStatus", true, "nOrderStatus", "hidden");
                        // CartType
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderType", true, "Cart Type", "required");
                        base.addOption(ref oSel1, "Order", "Order");
                        base.addOption(ref oSel1, "Quote", "Quote");

                        base.addSubmit(ref oGrp0Elmt, "Results", "See Results", "Results");



                        XmlElement argoBindParent = null;
                        base.addBind("nYear", "Criteria/nYear", oBindParent: ref argoBindParent);
                        XmlElement argoBindParent1 = null;
                        base.addBind("nMonth", "Criteria/nMonth", oBindParent: ref argoBindParent1);
                        XmlElement argoBindParent2 = null;
                        base.addBind("nDay", "Criteria/nDay", oBindParent: ref argoBindParent2);
                        XmlElement argoBindParent3 = null;
                        base.addBind("cGrouping", "Criteria/cGrouping", sType: "string", oBindParent: ref argoBindParent3);
                        XmlElement argoBindParent4 = null;
                        base.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", sType: "string", oBindParent: ref argoBindParent4);
                        XmlElement argoBindParent5 = null;
                        base.addBind("nOrderStatus", "Criteria/nOrderStatus", sType: "string", oBindParent: ref argoBindParent5);
                        XmlElement argoBindParent6 = null;
                        base.addBind("cOrderType", "Criteria/cOrderType", oBindParent: ref argoBindParent6, "true()", "string");


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmCartActivityDrillDown", ex, "", "", gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmCartActivityPeriod()
                {
                    try
                    {
                        // if nNewLocationPage =0 or nFromPage=0 or cContentType="" then 
                        XmlElement oFrmElmt;

                        var oTempInstance = moPageXML.CreateElement("instance");
                        //bool bCascade = false;
                        //string cProcessInfo = "";

                        base.NewFrm("CartActivityDrilldown");

                        base.submission("SeeReport", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");

                        var oGrp0Elmt = base.addGroup(ref oFrmElmt, "Criteria", sLabel: "Criteria");

                        base.Instance.InnerXml = "<Criteria>" + "<nYear>" + DateTime.Now.Year + "</nYear>" + "<nMonth>0</nMonth>" + "<nWeek>0</nWeek>" + "<cGroup>Month</cGroup><cCurrencySymbol/>" + "<nOrderStatus>6,9,17</nOrderStatus>" + "<cOrderType>Order</cOrderType>" + "</Criteria>";
                        // Year
                        var oSel1 = base.addSelect1(ref oGrp0Elmt, "nYear", true, "Year", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 2000, loopTo = DateTime.Now.Year; i <= loopTo; i++)
                            base.addOption(ref oSel1, i.ToString(), i.ToString());
                        // Month
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nMonth", true, "Month", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 1; i <= 12; i++)
                            base.addOption(ref oSel1, System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i), i.ToString());
                        // Day
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "nWeek", true, "Week", "required");
                        base.addOption(ref oSel1, "All", "0");
                        for (int i = 1; i <= 52; i++)
                            base.addOption(ref oSel1, i.ToString(), i.ToString());
                        // Grouping
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cGroup", true, "Grouping", "required");
                        base.addOption(ref oSel1, "By Month", "Month");
                        base.addOption(ref oSel1, "By Week", "Week");
                        base.addOption(ref oSel1, "By Day", "Day");
                        // Currency
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cCurrencySymbol", true, "Currency");
                        base.addOption(ref oSel1, "All/None", "");
                        base.addOption(ref oSel1, "GBP", "£");
                        // OrderStatus
                        base.addInput(ref oGrp0Elmt, "nOrderStatus", true, "nOrderStatus", "hidden");
                        // CartType
                        oSel1 = base.addSelect1(ref oGrp0Elmt, "cOrderType", true, "Cart Type", "required");
                        base.addOption(ref oSel1, "Order", "Order");
                        base.addOption(ref oSel1, "Quote", "Quote");

                        base.addSubmit(ref oGrp0Elmt, "Results", "See Results", "Results");

                        XmlElement argoBindParent = null;
                        base.addBind("nYear", "Criteria/nYear", oBindParent: ref argoBindParent);
                        XmlElement argoBindParent1 = null;
                        base.addBind("nMonth", "Criteria/nMonth", oBindParent: ref argoBindParent1);
                        XmlElement argoBindParent2 = null;
                        base.addBind("nWeek", "Criteria/nWeek", oBindParent: ref argoBindParent2);
                        XmlElement argoBindParent3 = null;
                        base.addBind("cGroup", "Criteria/cGroup", sType: "string", oBindParent: ref argoBindParent3);
                        XmlElement argoBindParent4 = null;
                        base.addBind("cCurrencySymbol", "Criteria/cCurrencySymbol", sType: "string", oBindParent: ref argoBindParent4);
                        XmlElement argoBindParent5 = null;
                        base.addBind("nOrderStatus", "Criteria/nOrderStatus", sType: "string", oBindParent: ref argoBindParent5);
                        XmlElement argoBindParent6 = null;
                        base.addBind("cOrderType", "Criteria/cOrderType", oBindParent: ref argoBindParent6, "true()", "string");


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmCartActivityPeriod", ex, "", "", gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmMemberVisits()
                {
                    try
                    {
                        XmlElement oFrmElmt;
                        // Dim oTempInstance As XmlElement = moPageXML.CreateElement("instance")


                        base.NewFrm("MemberVisits");
                        base.Instance.InnerXml = "<Criteria><dFrom>" + XmlDate(DateTime.Now.AddDays(-31), false) + "</dFrom><dTo>" + XmlDate(DateTime.Now.AddDays(1d), false) + "</dTo><cGroups>0</cGroups></Criteria>";
                        base.submission("SeeReport", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Content", "");
                        var oGrp0Elmt = base.addGroup(ref oFrmElmt, "Criteria", sLabel: "Search Visits");
                        base.addInput(ref oGrp0Elmt, "dFrom", true, "From", "calendar");
                        base.addInput(ref oGrp0Elmt, "dTo", true, "To", "calendar");
                        var oSel = base.addSelect(ref oGrp0Elmt, "cGroups", true, "Filter by Group", nAppearance: Protean.xForm.ApperanceTypes.Minimal);
                        base.addSubmit(ref oGrp0Elmt, "Results", "See Results", "Results");
                        string cSQL = "SELECT cDirName, nDirKey FROM tblDirectory ";
                        cSQL += " WHERE (NOT (cDirSchema = 'User')) AND (NOT (cDirSchema = N'Role'))";
                        cSQL += " ORDER BY cDirName";
                        using (var oDR = moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                        {
                            base.addOptionsFromSqlDataReader(oSel, oDR, "cDirName", "nDirKey");
                        }
                        XmlElement argoBindParent = null;
                        base.addBind("dFrom", "Criteria/dFrom", oBindParent: ref argoBindParent, "true()");
                        XmlElement argoBindParent1 = null;
                        base.addBind("dTo", "Criteria/dTo", oBindParent: ref argoBindParent1, "true()");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cGroups", "Criteria/cGroups", oBindParent: ref argoBindParent2);

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                        }

                        base.addValues();

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmMemberVisits", ex, "", "", gbDebug);
                        return null;
                    }
                }


            }
        }
    }
}