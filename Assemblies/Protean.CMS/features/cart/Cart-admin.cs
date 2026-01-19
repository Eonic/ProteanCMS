using Protean.Providers.Membership;
using Protean.Providers.Messaging;
using Protean.Providers.Payment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Protean.Cms;
using static Protean.Cms.dbHelper;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{


    public partial class Cms
    {

        public partial class Cart : IDisposable
        {


            public void ListOrders(ref XmlElement oContentsXML, cartProcess ProcessId)
            {
                myWeb.PerfMon.Log("Cart", "ListOrders");
                XmlElement oRoot;
                XmlElement oElmt;
                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    oRoot = moPageXml.CreateElement("Content");
                    oRoot.SetAttribute("type", "listTree");
                    oRoot.SetAttribute("template", "default");
                    oRoot.SetAttribute("name", "Orders - " + ProcessId.GetType().ToString());

                    sSql = "SELECT nCartOrderKey as id, c.cContactName as name, c.cContactEmail as email, a.dUpdateDate from tblCartOrder inner join tblAudit a on nAuditId = a.nAuditKey left outer join tblCartContact c on (nCartOrderKey = c.nContactCartId and cContactType = 'Billing Address') where nCartStatus = " + ((int)ProcessId).ToString();

                    oDs = moDBHelper.GetDataSet(sSql, "Order", "List");

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;

                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("List");
                        oElmt.InnerXml = oDs.GetXml();

                        oContentsXML.AppendChild(oElmt);
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListShippingLocations", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void ListShippingLocations(ref XmlElement oContentsXML, long OptId = 0L)
            {
                myWeb.PerfMon.Log("Cart", "ListShippingLocations");
                XmlElement oRoot;
                XmlElement oElmt;
                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    oRoot = moPageXml.CreateElement("Content");
                    oRoot.SetAttribute("type", "listTree");
                    oRoot.SetAttribute("template", "default");
                    if (OptId != 0L)
                    {
                        oRoot.SetAttribute("name", "Shipping Locations Form");
                        sSql = "SELECT nLocationKey as id, nLocationType as type, nLocationParId as parid, cLocationNameFull as Name, cLocationNameShort as nameShort, (SELECT COUNT(*) from tblCartShippingRelations r where r.nShpLocId = n.nLocationKey and r.nShpOptId = " + OptId + ") As selected from tblCartShippingLocations n ";
                    }
                    else
                    {
                        oRoot.SetAttribute("name", "Shipping Locations");
                        sSql = "SELECT nLocationKey as id, nLocationType as type, nLocationParId as parid, cLocationNameFull as Name, cLocationNameShort as nameShort, (SELECT COUNT(*) from tblCartShippingRelations r where r.nShpLocId = n.nLocationKey) As nOptCount from tblCartShippingLocations n ";
                    }

                    // NOTE : This SQL is NOT the same as the equivalent function in the EonicWeb component.
                    // It adds a count of shipping option relations for each location

                    oDs = moDBHelper.GetDataSet(sSql, "TreeItem", "Tree");

                    oDs.Relations.Add("rel01", oDs.Tables[0].Columns["id"], oDs.Tables[0].Columns["parId"], false);
                    oDs.Relations["rel01"].Nested = true;

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Hidden;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[5].ColumnMapping = MappingType.Attribute;

                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("Tree");
                        oElmt.InnerXml = oDs.GetXml();


                        XmlElement oCheckElmt = null;
                        oCheckElmt = (XmlElement)oElmt.SelectSingleNode("descendant-or-self::TreeItem[@id='" + mnShippingRootId + "']");
                        if (oCheckElmt != null)
                            oElmt = oCheckElmt;

                        oContentsXML.AppendChild(oElmt);
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListShippingLocations", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void ListDeliveryMethods(ref XmlElement oContentsXML)
            {
                myWeb.PerfMon.Log("Cart", "ListDeliveryMethods");
                XmlElement oElmt;
                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    sSql = "select a.nStatus as status, nShipOptKey as id, cShipOptName as name, cShipOptCarrier as carrier, a.dPublishDate as startDate, a.dExpireDate as endDate, tblCartShippingMethods.cCurrency from tblCartShippingMethods left join tblAudit a on a.nAuditKey = nAuditId order by nDisplayPriority";
                    oDs = moDBHelper.GetDataSet(sSql, "ListItem", "List");

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[5].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[6].ColumnMapping = MappingType.Attribute;
                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("List");
                        oElmt.InnerXml = oDs.GetXml();

                        oContentsXML.AppendChild(oElmt.FirstChild);
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListDeliveryMethods", ex, "", cProcessInfo, gbDebug);
                }
            }


            public void ListCarriers(ref XmlElement oContentsXML)
            {
                myWeb.PerfMon.Log("Cart", "ListDeliveryMethods");
                XmlElement oElmt;
                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    sSql = "select a.nStatus as status, nCarrierKey as id, cCarrierName as name, cCarrierTrackingInstructions as info, a.dPublishDate as startDate, a.dExpireDate as endDate from tblCartCarrier left join tblAudit a on a.nAuditKey = nAuditId";
                    oDs = moDBHelper.GetDataSet(sSql, "Carrier", "Carriers");

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[5].ColumnMapping = MappingType.Attribute;
                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("Carriers");
                        oElmt.InnerXml = oDs.GetXml();

                        oContentsXML.AppendChild(oElmt.FirstChild);
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListCarriers", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void ListPaymentProviders(ref XmlElement oContentsXML)
            {
                myWeb.PerfMon.Log("Cart", "ListPaymentProviders");
                XmlElement oElmt;
                XmlElement oElmt2;
                string cProcessInfo = "";
                XmlNode oPaymentCfg;
                string ptnFolder = "/ewcommon/xforms/PaymentProvider/";
                string localFolder = "/xforms/PaymentProvider/";
                FileInfo fi;
                string ProviderName;
                if (myWeb.bs5)
                {
                    ptnFolder = "/ptn/providers/payment/";
                    localFolder = "/providers/payment/";
                }
                try
                {

                    oPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                    oElmt = moPageXml.CreateElement("List");


                    if (myWeb.bs5)
                    {
                        var dir = new DirectoryInfo(moServer.MapPath(ptnFolder));
                        if (dir.Exists)
                        {
                            DirectoryInfo[] dirs;
                            dirs = dir.GetDirectories();
                            foreach (var dir2 in dirs)
                            {
                                ProviderName = dir2.Name;
                                XmlNode argoNode = oElmt;
                                oElmt2 = Protean.Tools.Xml.addNewTextNode("Provider", ref argoNode, ProviderName.Replace("-", " "));
                                oElmt = (XmlElement)argoNode;
                                if (oPaymentCfg.SelectSingleNode("/payment/provider[@name='" + ProviderName.Replace("-", "") + "']") != null)
                                {
                                    oElmt2.SetAttribute("active", "true");
                                }
                            }
                        }
                        dir = new DirectoryInfo(moServer.MapPath(localFolder));
                        if (dir.Exists)
                        {
                            DirectoryInfo[] dirs;
                            dirs = dir.GetDirectories();
                            foreach (var dir2 in dirs)
                            {
                                ProviderName = dir2.Name;
                                XmlNode argoNode1 = oElmt;
                                oElmt2 = Protean.Tools.Xml.addNewTextNode("Provider", ref argoNode1, ProviderName.Replace("-", " "));
                                oElmt = (XmlElement)argoNode1;

                                if (oPaymentCfg.SelectSingleNode("/payment/provider[@name='" + ProviderName.Replace("-", "") + "']") != null)
                                {
                                    oElmt2.SetAttribute("active", "true");
                                }
                            }
                        }
                    }
                    else
                    {
                        var dir = new DirectoryInfo(moServer.MapPath(ptnFolder));
                        FileInfo[] files = dir.GetFiles();
                        foreach (var currentFi in files)
                        {
                            fi = currentFi;
                            if (fi.Extension == ".xml")
                            {
                                ProviderName = fi.Name.Replace(fi.Extension, "");

                                XmlNode argoNode2 = oElmt;
                                oElmt2 = Protean.Tools.Xml.addNewTextNode("Provider", ref argoNode2, ProviderName.Replace("-", " "));
                                oElmt = (XmlElement)argoNode2;
                                if (oPaymentCfg.SelectSingleNode("/payment/provider[@name='" + ProviderName.Replace("-", "") + "']") != null)
                                {
                                    oElmt2.SetAttribute("active", "true");
                                }
                            }
                        }
                        dir = new DirectoryInfo(moServer.MapPath(localFolder));
                        if (dir.Exists)
                        {
                            files = dir.GetFiles();
                            foreach (var currentFi1 in files)
                            {
                                fi = currentFi1;
                                if (fi.Extension == ".xml")
                                {
                                    ProviderName = fi.Name.Replace(fi.Extension, "");
                                    XmlNode argoNode3 = oElmt;
                                    oElmt2 = Protean.Tools.Xml.addNewTextNode("Provider", ref argoNode3, ProviderName.Replace("-", " "));
                                    oElmt = (XmlElement)argoNode3;
                                    if (oPaymentCfg.SelectSingleNode("/payment/provider[@name='" + ProviderName.Replace("-", "") + "']") != null)
                                    {
                                        oElmt2.SetAttribute("active", "true");
                                    }
                                }
                            }
                        }
                    }


                    oContentsXML.AppendChild(oElmt);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListPaymentProviders", ex, "", cProcessInfo, gbDebug);
                }
            }



            public void ListOrders(string sOrderID, bool bListAllQuotes, int ProcessId, ref XmlElement oPageDetail, bool bForceRefresh = false, long nUserId = 0L)
            {
                myWeb.PerfMon.Log("Cart", "ListOrders");
                if (myWeb.mnUserId == 0)
                    return; // if not logged in, dont bother
                            // For listing a users previous orders/quotes

                var oDs = new DataSet();
                string cSQL;
                string cWhereSQL = "";
                string cProcessInfo = "";
                // Paging variables
                int nStart = 0;
                int nRows = 100;

                int nCurrentRow = 0;
                XmlElement moPaymentCfg = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/payment");

                try
                {

                    // Set the paging variables, if provided.
                    if (myWeb.moRequest["startPos"] != null && Tools.Number.IsNumeric(myWeb.moRequest["startPos"]))
                        nStart = Convert.ToInt16(myWeb.moRequest["startPos"]);
                    if (myWeb.moRequest["rows"] != null && Tools.Number.IsNumeric(myWeb.moRequest["rows"]))
                        nRows = Convert.ToInt16(myWeb.moRequest["rows"]);

                    if (nStart < 0)
                        nStart = 0;
                    if (nRows < 1)
                        nRows = 100;

                    if (nUserId != 0L)
                    {
                        cWhereSQL = " WHERE nCartUserDirId = " + nUserId + (sOrderID != "0" ? " AND nCartOrderKey IN (" + sOrderID + ")" : "") + " AND cCartSchemaName = '" + mcOrderType + "'";
                    }
                    else if (!myWeb.mbAdminMode)
                    {
                        cWhereSQL = " WHERE nCartUserDirId = " + myWeb.mnUserId + (sOrderID != "0" ? " AND nCartOrderKey IN (" + sOrderID + ")" : "") + " AND cCartSchemaName = '" + mcOrderType + "'";
                    }
                    else
                    {
                        cWhereSQL = " WHERE " + (sOrderID != "0" ? " nCartOrderKey IN (" + sOrderID + ") AND " : "") + "cCartSchemaName = '" + mcOrderType + "'";

                        if (ProcessId != 0)
                        {
                            cWhereSQL += " and nCartStatus = " + ProcessId;
                        }
                    }


                    // Quick call to get the total number of records
                    cSQL = "SELECT COUNT(*) As Count FROM tblCartOrder " + cWhereSQL;
                    long nTotal = Convert.ToInt64(moDBHelper.GetDataValue(cSQL));

                    if (nTotal > 0L)
                    {

                        // Initial paging option is limit the the rows returned
                        cSQL = "SELECT TOP " + (nStart + nRows) + " * FROM tblCartOrder ";
                        cSQL += cWhereSQL + " ORDER BY nCartOrderKey Desc";

                        oDs = moDBHelper.GetDataSet(cSQL, mcOrderType, mcOrderType + "List");

                        if (oDs.Tables.Count > 0)
                        {


                            XmlElement oContentDetails;
                            // Get the content Detail element
                            if (oPageDetail is null)
                            {
                                oContentDetails = (XmlElement)moPageXml.SelectSingleNode("Page/ContentDetail");
                                if (oContentDetails is null)
                                {
                                    oContentDetails = moPageXml.CreateElement("ContentDetail");
                                    if (!string.IsNullOrEmpty(moPageXml.InnerXml))
                                    {
                                        moPageXml.FirstChild.AppendChild(oContentDetails);
                                    }
                                    else
                                    {
                                        oPageDetail.AppendChild(oContentDetails);
                                    }

                                }
                            }
                            else
                            {
                                oContentDetails = oPageDetail;
                            }

                            oContentDetails.SetAttribute("start", nStart.ToString());
                            oContentDetails.SetAttribute("total", nTotal.ToString());
                            bool bSingleRecord = false;
                            if (oDs.Tables[mcOrderType].Rows.Count == 1)
                                bSingleRecord = true;

                            // go through each cart
                            foreach (DataRow oDR in oDs.Tables[mcOrderType].Rows)
                            {
                                // Only add the relevant rows (page selected)
                                nCurrentRow += 1;
                                if (nCurrentRow > nStart)
                                {
                                    var oContent = moPageXml.CreateElement("Content");
                                    oContent.SetAttribute("type", mcOrderType);
                                    oContent.SetAttribute("id", Convert.ToString(oDR["nCartOrderKey"]));
                                    oContent.SetAttribute("statusId", Convert.ToString(oDR["nCartStatus"]));
                                    oContent.SetAttribute("cartForiegnRef", (oDR["cCartForiegnRef"] == null) ? string.Empty : (string)oDR["cCartForiegnRef"]);
                                    // Get Date
                                    cSQL = "Select dInsertDate from tblAudit where nAuditKey=" + oDR["nAuditId"];
                                    using (var oDRe = moDBHelper.getDataReaderDisposable(cSQL))  // Done by Nita on 6/7/22
                                    {
                                        while (oDRe.Read())
                                        {
                                            oContent.SetAttribute("created", Tools.Xml.XmlDate(oDRe.GetValue(0), true));
                                        }
                                    }

                                    // Get stored CartXML
                                    if (!string.IsNullOrEmpty(oDR["cCartXML"]?.ToString()) && bForceRefresh == false)
                                    {
                                        try
                                        {
                                            // if we have a badly saved xml we get a new one.
                                            oContent.InnerXml = Convert.ToString(oDR["cCartXML"]);
                                        }
                                        catch (Exception ex)
                                        {
                                            cProcessInfo = ex.Message;
                                            mnCartId = Convert.ToInt32(oDR["nCartOrderKey"]);
                                            GetCart(ref oContent, mnCartId);
                                            mnCartId = 0;
                                        }
                                        if (oContent.InnerXml.Contains("\n"))
                                        {
                                            oContent.InnerXml = oContent.InnerXml.TrimStart('\n');
                                        }
                                        XmlElement oCartElmt = (XmlElement)oContent.FirstChild;

                                        // check for invoice date etc.
                                        if (Convert.ToInt64("0" + oContent.GetAttribute("statusId")) >= 6L & (string.IsNullOrEmpty(oCartElmt.GetAttribute("InvoiceDate")) | !oCartElmt.GetAttribute("InvoiceDateTime").Contains("T")))
                                        {
                                            // fix for any items that have lost the invoice date and ref.
                                            // also fix when datetime no stored in XML format.
                                            long cartId = Convert.ToInt64(oDR["nCartOrderKey"]);
                                            oCartElmt.SetAttribute("statusId", oContent.GetAttribute("statusId"));
                                            string insertDate = moDBHelper.ExeProcessSqlScalar("SELECT a.dInsertDate FROM tblCartOrder inner join tblAudit a on nAuditId = nAuditKey where nCartOrderKey = " + cartId);
                                            addDateAndRef(ref oCartElmt, Convert.ToDateTime(insertDate), cartId);
                                            SaveCartXML(oCartElmt, cartId);
                                        }

                                    }

                                    if (bForceRefresh)
                                    {
                                        var oCartListElmt = moPageXml.CreateElement("Order");
                                        GetCart(ref oCartListElmt, Convert.ToInt16(oDR["nCartOrderKey"]));
                                        oContent.InnerXml = oCartListElmt.OuterXml;
                                    }

                                    XmlElement orderNode = (XmlElement)oContent.FirstChild;
                                    // Add values not stored in cartXml
                                    if (orderNode != null)
                                    {
                                        orderNode.SetAttribute("statusId", Convert.ToString(oDR["nCartStatus"]));
                                    }
                                    if (oDR["cCurrency"] == null || oDR["cCurrency"]?.ToString() == "")
                                    {
                                        oContent.SetAttribute("currency", mcCurrency);
                                        oContent.SetAttribute("currencySymbol", mcCurrencySymbol);
                                    }
                                    else
                                    {
                                        oContent.SetAttribute("currency", Convert.ToString(oDR["cCurrency"]));
                                        XmlElement thisCurrencyNode = (XmlElement)moPaymentCfg.SelectSingleNode("currencies/Currency[@ref='" + oDR["cCurrency"] + "']");
                                        if (thisCurrencyNode != null)
                                        {
                                            oContent.SetAttribute("currencySymbol", thisCurrencyNode.GetAttribute("symbol"));
                                        }
                                        else
                                        {
                                            oContent.SetAttribute("currencySymbol", mcCurrencySymbol);
                                        }
                                    }
                                    oContent.SetAttribute("type", mcOrderType.ToLower());

                                    // oContent.SetAttribute("currency", mcCurrency)
                                    // oContent.SetAttribute("currencySymbol", mcCurrencySymbol)

                                    if (Convert.ToInt32(oDR["nCartUserDirId"]) != 0)
                                    {
                                        oContent.SetAttribute("userId", oDR["nCartUserDirId"].ToString());
                                    }

                                    // TS: Removed because it gives a massive overhead when Listing loads of orders.
                                    if (bSingleRecord)
                                    {
                                        if (myWeb.mbAdminMode && Convert.ToInt16(oDR["nCartUserDirId"]) > 0)
                                        {
                                            oContent.AppendChild(moDBHelper.GetUserXML((long)Convert.ToInt16(oDR["nCartUserDirId"]), false));
                                        }

                                        string[] aSellerNotes = oDR["cSellerNotes"]?.ToString().Split(new[] { "\n" }, StringSplitOptions.None) ?? Array.Empty<string>();
                                        string cSellerNotesHtml = "<ul>";
                                        for (int snCount = 0; snCount < aSellerNotes.Length; snCount++)
                                        {
                                            cSellerNotesHtml += "<li>" + convertEntitiesToCodes(aSellerNotes[snCount]) + "</li>";
                                        }
                                        var argoNode = oContent.FirstChild;
                                        var sellerNode = Protean.Tools.Xml.addNewTextNode("SellerNotes", ref argoNode, "");
                                        try
                                        {
                                            sellerNode.InnerXml = cSellerNotesHtml + "</ul>";
                                        }
                                        catch (Exception)
                                        {
                                            sellerNode.InnerXml = stdTools.tidyXhtmlFrag(cSellerNotesHtml + "</ul>");
                                        }

                                        // Add the Delivery Details
                                        // Add Delivery Details
                                        if (Convert.ToInt32(oDR["nCartStatus"]) == 9)
                                        {
                                            string sSql = "Select * from tblCartOrderDelivery where nOrderId=" + oDR["nCartOrderKey"];
                                            DataSet oDs2 = moDBHelper.GetDataSet(sSql, "Delivery", "Details");
                                            foreach (DataRow oRow2 in oDs2.Tables["Delivery"].Rows)
                                            {
                                                var oElmt = moPageXml.CreateElement("DeliveryDetails");
                                                oElmt.SetAttribute("carrierName", Convert.ToString(oRow2["cCarrierName"]));
                                                oElmt.SetAttribute("ref", Convert.ToString(oRow2["cCarrierRef"]));
                                                oElmt.SetAttribute("notes", Convert.ToString(oRow2["cCarrierNotes"]));
                                                oElmt.SetAttribute("deliveryDate", XmlDate(oRow2["dExpectedDeliveryDate"]));
                                                oElmt.SetAttribute("collectionDate", XmlDate(oRow2["dCollectionDate"]));
                                                oContent.AppendChild(oElmt);
                                            }
                                        }
                                        // Add Payment History
                                        string argsTableName = "tblCartPayment";
                                        if (Convert.ToInt32(oDR["nCartStatus"]) > 5 && moDBHelper.doesTableExist(ref argsTableName))
                                        {
                                            DataSet oDs3 = new DataSet();
                                            string sSql = "Select p.*, pm.*, a.dInsertDate " + "from tblCartPayment p " + "inner join tblCartPaymentMethod pm on p.nCartPaymentMethodId = pm.nPayMthdKey " +
                                                          "left outer join tblAudit a on a.nAuditKey = p.nAuditId " +
                                                          "where nCartOrderId=" + oDR["nCartOrderKey"];

                                            oDs3 = moDBHelper.GetDataSet(sSql, "Payment", "Details");
                                            oDs3.Tables["Payment"].Columns["cPayMthdDetailXml"].ColumnMapping = MappingType.Element;

                                            var oXML2 = new XmlDocument();
                                            oXML2.InnerXml = oDs3.GetXml().Replace("&gt;", ">").Replace("&lt;", "<");

                                            var oPaymentNode = oContent.OwnerDocument.CreateElement("Payments");
                                            oPaymentNode.InnerXml = oXML2.InnerXml;

                                            foreach (XmlElement oElmt in oPaymentNode.FirstChild.SelectNodes("*"))
                                                oContent.FirstChild.AppendChild(oPaymentNode.FirstChild.FirstChild);
                                        }

                                    }

                                    XmlElement oTestNode = (XmlElement)oContentDetails.SelectSingleNode("Content[@id=" + oContent.GetAttribute("id") + " and @type='" + (mcOrderType).ToLower() + "']");
                                    if (mcOrderType == "Cart" | mcOrderType == "Order")
                                    {
                                        // If (Not oContent.FirstChild.Attributes("itemCount").Value = 0) And oTestNode Is Nothing Then

                                        oContentDetails.AppendChild(oContent);
                                    }

                                    // End If
                                    else if (oTestNode is null)
                                    {
                                        if (bListAllQuotes)
                                        {
                                            oContentDetails.AppendChild(oContent);
                                        }
                                        else
                                        {
                                            // If (Not oContent.FirstChild.Attributes("itemCount").Value = 0) Then
                                            oContentDetails.AppendChild(oContent);
                                            // End If
                                        }
                                    }



                                    // If (Not oContent.FirstChild.Attributes("itemCount").Value = 0) And oTestNode Is Nothing Then
                                    // oContentDetails.AppendChild(oContent)
                                    // End If
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListOrders", ex, "", cProcessInfo, gbDebug);
                }
            }


            public XmlElement CartReportsDownload(DateTime dBegin, DateTime dEnd, string cCurrencySymbol, string cOrderType, int nOrderStage, bool updateDatesWithStartAndEndTimes = true)
            {
                try
                {
                    string cSQL = "exec ";
                    string cCustomParam = string.Empty;
                    string cReportType = "CartDownload";

                    // Set the times for each date
                    if (updateDatesWithStartAndEndTimes)
                    {
                        dBegin = dBegin.Date;
                        dEnd = dEnd.Date.AddHours(23d).AddMinutes(59d).AddSeconds(59d);
                    }


                    cSQL += moDBHelper.getDBObjectNameWithBespokeCheck("spOrderDownload") + " ";
                    cSQL += Tools.Database.SqlDate(dBegin, true) + ",";
                    cSQL += Tools.Database.SqlDate(dEnd, true) + ",";
                    cSQL += "'" + cOrderType + "',";
                    cSQL += nOrderStage.ToString();

                    var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report");

                    if (oDS.Tables["Item"].Columns.Contains("cCartXML"))
                    {
                        oDS.Tables["Item"].Columns["cCartXML"].ColumnMapping = MappingType.Element;
                    }
                    var oRptElmt = myWeb.moPageXml.CreateElement("Content");
                    oRptElmt.SetAttribute("type", "Report");
                    oRptElmt.SetAttribute("name", "CartDownloads");
                    // NB editing this line to add in &'s
                    oRptElmt.InnerXml = oDS.GetXml();
                    foreach (XmlElement oElmt in oRptElmt.SelectNodes("Report/Item/cCartXml"))
                        oElmt.InnerXml = oElmt.InnerText;

                    // oRptElmt.InnerXml = Replace(Replace(Replace(Replace(oDS.GetXml, "&amp;", "&"), "&gt;", ">"), "&lt;", "<"), " xmlns=""""", "")
                    // oRptElmt.InnerXml = Replace(Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<"), " xmlns=""""", "")
                    XmlElement oReturnElmt = (XmlElement)oRptElmt.FirstChild;
                    oReturnElmt.SetAttribute("cReportType", cReportType);
                    oReturnElmt.SetAttribute("dBegin", Convert.ToString(dBegin));
                    oReturnElmt.SetAttribute("dEnd", Convert.ToString(dEnd));
                    oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol);
                    oReturnElmt.SetAttribute("cOrderType", cOrderType);
                    oReturnElmt.SetAttribute("nOrderStage", nOrderStage.ToString());

                    return oRptElmt;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartReports", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            public XmlElement CartReports(DateTime dBegin, DateTime dEnd, int bSplit = 0, string cProductType = "", int nProductId = 0, string cCurrencySymbol = "", string nOrderStatus = "6,9,17", string cOrderType = "ORDER")
            {
                try
                {
                    string cSQL = "exec ";
                    string cCustomParam = "";
                    string cReportType = "";
                    if (nProductId > 0)
                    {
                        // Low Level
                        cSQL += "spCartActivityLowLevel ";
                        cCustomParam = nProductId.ToString();
                        cReportType = "Item Totals";
                    }
                    else if (!string.IsNullOrEmpty(cProductType))
                    {
                        // Med Level
                        cSQL += "spCartActivityMedLevel ";
                        cCustomParam = "'" + cProductType + "'";
                        cReportType = "Type Totals";
                    }
                    else
                    {
                        // HighLevel
                        cSQL += "spCartActivityTopLevel ";
                        cCustomParam = bSplit.ToString();
                        cReportType = "All Totals";
                    }
                    cSQL += Tools.Database.SqlDate(dBegin) + ",";
                    cSQL += Tools.Database.SqlDate(dEnd) + ",";
                    cSQL += cCustomParam + ",";
                    cSQL += "'" + cCurrencySymbol + "','";
                    cSQL += nOrderStatus + "',";
                    cSQL += "'" + cOrderType + "'";

                    var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report");

                    if (oDS.Tables["Item"].Columns.Contains("cCartXML"))
                    {
                        oDS.Tables["Item"].Columns["cCartXML"].ColumnMapping = MappingType.Element;
                    }
                    var oRptElmt = myWeb.moPageXml.CreateElement("Content");
                    oRptElmt.SetAttribute("type", "Report");
                    oRptElmt.SetAttribute("name", "Cart Activity");
                    oRptElmt.InnerXml = oDS.GetXml().Replace("&gt;", ">").Replace("&lt;", "<");
                    XmlElement oReturnElmt = (XmlElement)oRptElmt.FirstChild;
                    oReturnElmt.SetAttribute("cReportType", cReportType);

                    oReturnElmt.SetAttribute("dBegin", Convert.ToString(dBegin));
                    oReturnElmt.SetAttribute("dEnd", Convert.ToString(dEnd));
                    oReturnElmt.SetAttribute("bSplit", (bSplit != 0) ? "1" : "0");
                    oReturnElmt.SetAttribute("cProductType", cProductType);
                    oReturnElmt.SetAttribute("nProductId", nProductId.ToString());
                    oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol);
                    oReturnElmt.SetAttribute("nOrderStatus", nOrderStatus);
                    oReturnElmt.SetAttribute("cOrderType", cOrderType);
                    return oRptElmt;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartReports", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            public XmlElement CartReportsDrilldown(string cGrouping = "Page", int nYear = 0, int nMonth = 0, int nDay = 0, string cCurrencySymbol = "", int nOrderStatus1 = 6, int nOrderStatus2 = 9, string cOrderType = "ORDER")
            {
                try
                {
                    string cSQL = "exec spCartActivityGroupsPages ";
                    bool bPage = false;
                    string cReportType = "";

                    cSQL += "'" + cGrouping + "',";
                    cSQL += nYear + ",";
                    cSQL += nMonth + ",";
                    cSQL += nDay + ",";
                    cSQL += "'" + cCurrencySymbol + "',";
                    cSQL += nOrderStatus1 + ",";
                    cSQL += nOrderStatus2 + ",";
                    cSQL += "'" + cOrderType + "'";

                    var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report");
                    // For Grouped by page is going to be bloody hard
                    if (oDS.Tables["Item"].Columns.Contains("nStructId"))
                    {
                        bPage = true;
                        cSQL = "EXEC getContentStructure @userId=" + myWeb.mnUserId + ", @bAdminMode=1, @dateNow=" + Tools.Database.SqlDate(DateTime.Now) + ", @authUsersGrp = " + Cms.gnAuthUsers;
                        myWeb.moDbHelper.addTableToDataSet(ref oDS, cSQL, "MenuItem");
                        // oDS.Tables("MenuItem").Columns.Add(New DataColumn("PageQuantity", GetType(Double)))
                        // oDS.Tables("MenuItem").Columns.Add(New DataColumn("PageCost", GetType(Double)))
                        // oDS.Tables("MenuItem").Columns.Add(New DataColumn("DecendantQuantity", GetType(Double)))
                        // oDS.Tables("MenuItem").Columns.Add(New DataColumn("DecendantCost", GetType(Double)))
                        foreach (DataColumn oDC in oDS.Tables["MenuItem"].Columns)
                        {
                            string cValid = "id,parid,name"; // ,PageQuantity,PageCost,DecendantQuantity,DecendantCost"
                            if (!cValid.Contains(oDC.ColumnName))
                            {
                                oDC.ColumnMapping = MappingType.Hidden;
                            }
                            else
                            {
                                oDC.ColumnMapping = MappingType.Attribute;
                            }
                        }
                        foreach (DataColumn oDC in oDS.Tables["Item"].Columns)
                            oDC.ColumnMapping = MappingType.Attribute;
                        oDS.Relations.Add("Rel01", oDS.Tables["MenuItem"].Columns["id"], oDS.Tables["MenuItem"].Columns["parId"], false);
                        oDS.Relations["Rel01"].Nested = true;
                        oDS.Relations.Add(new DataRelation("Rel02", oDS.Tables["MenuItem"].Columns["id"], oDS.Tables["Item"].Columns["nStructId"], false));
                        oDS.Relations["Rel02"].Nested = true;
                    }



                    var oRptElmt = myWeb.moPageXml.CreateElement("Content");
                    oRptElmt.SetAttribute("type", "Report");
                    oRptElmt.SetAttribute("name", "Cart Activity");
                    oRptElmt.InnerXml = oDS.GetXml();
                    XmlElement oReturnElmt = (XmlElement)oRptElmt.FirstChild;
                    oReturnElmt.SetAttribute("cReportType", cReportType);

                    oReturnElmt.SetAttribute("nYear", nYear.ToString());
                    oReturnElmt.SetAttribute("nMonth", nMonth.ToString());
                    oReturnElmt.SetAttribute("nDay", nDay.ToString());
                    oReturnElmt.SetAttribute("cGrouping", cGrouping);
                    oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol);
                    oReturnElmt.SetAttribute("nOrderStatus1", nOrderStatus1.ToString());
                    oReturnElmt.SetAttribute("nOrderStatus2", nOrderStatus2.ToString());
                    oReturnElmt.SetAttribute("cOrderType", cOrderType);

                    if (bPage)
                    {
                        // Page Totals
                        foreach (XmlElement oElmt in oReturnElmt.SelectNodes("descendant-or-self::MenuItem"))
                        {
                            int nQ = 0;
                            double nC = 0d;
                            foreach (XmlElement oItemElmt in oElmt.SelectNodes("Item"))
                            {
                                nQ = (int)Math.Round(nQ + Convert.ToDouble(oItemElmt.GetAttribute("nQuantity")));
                                nC += Convert.ToDouble(oItemElmt.GetAttribute("nLinePrice"));
                            }
                            oElmt.SetAttribute("PageQuantity", nQ.ToString());
                            oElmt.SetAttribute("PageCost", nC.ToString());
                        }
                        // loop through each node and then each item and see how many of each
                        // item it has and its decendants
                        foreach (XmlElement oElmt in oReturnElmt.SelectNodes("descendant-or-self::MenuItem"))
                        {
                            var oHN = new Hashtable(); // Number found
                            var oHQ = new Hashtable(); // Quantity
                            var oHC = new Hashtable(); // Cost
                            foreach (XmlElement oItem in oElmt.SelectNodes("descendant-or-self::Item"))
                            {
                                string cKey = "I" + oItem.GetAttribute("nCartItemKey");
                                if (!oHN.ContainsKey(cKey))
                                {
                                    oHN.Add(cKey, 1);
                                    oHQ.Add(cKey, oItem.GetAttribute("nQuantity"));
                                    oHC.Add(cKey, oItem.GetAttribute("nLinePrice"));
                                }
                                else
                                {
                                    oHN[cKey] += Convert.ToString(1);
                                }
                            }
                            int nQ = 0;
                            double nC = 0d;
                            foreach (string ci in oHN.Keys)
                            {
                                nQ = Convert.ToInt16(nQ + Convert.ToInt32(oHQ[ci]));
                                nC = Convert.ToDouble(nC + Convert.ToInt32(oHC[ci]));
                            }
                            oElmt.SetAttribute("PageAndDescendantQuantity", nQ.ToString());
                            oElmt.SetAttribute("PageAndDescendantCost", nC.ToString());
                        }



                    }


                    if (bPage & Cms.gnTopLevel > 0)
                    {
                        XmlElement oElmt = (XmlElement)oRptElmt.SelectSingleNode("descendant-or-self::MenuItem[@id=" + Cms.gnTopLevel + "]");
                        if (oElmt != null)
                        {
                            oRptElmt.FirstChild.InnerXml = oElmt.OuterXml;
                        }
                        else
                        {
                            oRptElmt.FirstChild.InnerXml = "";
                        }
                    }

                    return oRptElmt;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartReports", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            public XmlElement CartReportsPeriod(string cGroup = "Month", int nYear = 0, int nMonth = 0, int nWeek = 0, string cCurrencySymbol = "", string nOrderStatus = "", string cOrderType = "ORDER")
            {
                try
                {
                    string cSQL = "exec spCartActivityPagesPeriod ";
                    //bool bPage = false;
                    string cReportType = "";
                    if (nYear == 0)
                        nYear = DateTime.Now.Year;
                    cSQL += "@Group='" + cGroup + "'";
                    cSQL += ",@nYear=" + nYear;
                    cSQL += ",@nMonth=" + nMonth;
                    cSQL += ",@nWeek=" + nWeek;
                    cSQL += ",@cCurrencySymbol='" + cCurrencySymbol + "'";
                    cSQL += ",@nOrderStatus='" + nOrderStatus + "'";
                    // cSQL += ",@nOrderStatus2=" + nOrderStatus2;
                    cSQL += ",@cOrderType='" + cOrderType + "'";

                    var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report");
                    // For Grouped by page is going to be bloody hard

                    var oRptElmt = myWeb.moPageXml.CreateElement("Content");
                    oRptElmt.SetAttribute("type", "Report");
                    oRptElmt.SetAttribute("name", "Cart Activity");
                    oRptElmt.InnerXml = oDS.GetXml();
                    XmlElement oReturnElmt = (XmlElement)oRptElmt.FirstChild;
                    oReturnElmt.SetAttribute("cReportType", cReportType);

                    oReturnElmt.SetAttribute("nYear", nYear.ToString());
                    oReturnElmt.SetAttribute("nMonth", nMonth.ToString());
                    oReturnElmt.SetAttribute("nWeek", nWeek.ToString());
                    oReturnElmt.SetAttribute("cGroup", cGroup);
                    oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol);
                    oReturnElmt.SetAttribute("nOrderStatus", nOrderStatus.ToString());
                    // oReturnElmt.SetAttribute("nOrderStatus2", nOrderStatus2.ToString());
                    oReturnElmt.SetAttribute("cOrderType", cOrderType);

                    return oRptElmt;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartReports", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }




        }
    }
}