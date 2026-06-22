// ***********************************************************************
// $Library:     protean.cms.admin
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.digital)
// &Website:     eonic.digital
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2026 Eonic Digital Group Ltd.
// ***********************************************************************

//using Microsoft.VisualBasic;
//using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.Membership;
using Protean.Providers.Messaging;
using Protean.Tools;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using static Protean.FeedHandler;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin : IDisposable
        {

            private void OrderProcess(ref XmlElement oPageDetail, ref string sAdminLayout, string cSchemaName)
            {
                string sProcessInfo = "";
                System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

                try
                {
                    int nOrderStatus1 = 0;
                    int nOrderStatus2 = 0;
                    int nOrderStatus3 = 0;

                    if (mcEwCmd.Contains("Order") | mcEwCmd == "BulkCartAction")
                    {
                        var oCart = new Cms.Cart(ref myWeb);

                        object ewCmd2 = myWeb.moRequest["ewCmd2"];

                        switch (mcEwCmd ?? "")
                        {
                            case "BulkCartAction":
                                {
                                    switch (myWeb.moRequest["BulkAction"].ToLower() ?? "")
                                    {
                                        case "print":
                                            {
                                                ewCmd2 = "Print";
                                                sAdminLayout = "Print";
                                                break;
                                            }
                                        case "setinprogress":
                                            {

                                                string[] ids = myWeb.moRequest["id"].Split(',');
                                                foreach (var id in ids)
                                                    myWeb.moDbHelper.ExeProcessSql("update tblCartOrder set nCartStatus = 17 where nCartOrderKey = " + id);
                                                mcEwCmd = "OrdersInProgress";
                                                break;
                                            }
                                        case "setshipped":
                                            {

                                                string[] ids = myWeb.moRequest["id"].Split(',');
                                                foreach (var id in ids)
                                                    myWeb.moDbHelper.ExeProcessSql("update tblCartOrder set nCartStatus = 9 where nCartOrderKey = " + id);
                                                mcEwCmd = "OrdersShipped";
                                                break;
                                            }


                                    }

                                    break;
                                }
                        }

                        switch (ewCmd2)
                        {
                            case "Display":
                                {
                                    long nStatus;

                                    string sSql = "select nCartStatus from tblCartOrder WHERE nCartOrderKey =" + myWeb.moRequest["id"];
                                    nStatus = Convert.ToInt64(myWeb.moDbHelper.ExeProcessSqlScalar(sSql));

                                    oPageDetail.AppendChild(moAdXfm.xFrmUpdateOrder(Convert.ToInt64(myWeb.moRequest["id"]), cSchemaName));

                                    bool forceRefresh = false;
                                    // TS removed as we do not want to refresh the cart XML as it destroys discount info and order ref etc.
                                    if (myWeb.moRequest["refresh"] == "true")
                                    {
                                        forceRefresh = true;
                                    }

                                    oCart.ListOrders(myWeb.moRequest["id"], true, 0, ref oPageDetail, forceRefresh, nUserId: 0L);

                                    // :TODO Behaviour to manage resending recipts.
                                    if (moCartConfig["SendRecieptsFromAdmin"] != "off")
                                    {
                                        if (moAdXfm.isSubmitted() & moAdXfm.valid)
                                        {
                                            if ((double)nStatus != Convert.ToDouble(myWeb.moRequest["nStatus"]) & Convert.ToDouble(myWeb.moRequest["nStatus"]) == (double)Cms.Cart.cartProcess.Complete)
                                            {
                                                oCart.mnCartId = Convert.ToInt64(myWeb.moRequest["id"]);
                                                XmlElement argoCartElmt = (XmlElement)oPageDetail.LastChild.FirstChild;
                                                oCart.addDateAndRef(ref argoCartElmt);
                                                XmlElement argoCartElmt1 = (XmlElement)oPageDetail.LastChild;
                                                oCart.emailReceipts(ref argoCartElmt1);
                                            }
                                        }
                                    }

                                    break;
                                }

                            case "Print":
                                {
                                    string orderId = myWeb.moRequest["id"];    //Can be CSV                                           

                                    myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.Custom1, (long)myWeb.mnUserId, 0L, 0L, 0L, "Print Delivery " + orderId, false);

                                    var ofs = new Protean.fsHelper();
                                    myWeb.moResponseType = Cms.pageResponseType.pdf;

                                    if (orderId.Contains(","))
                                    {
                                        myWeb.mcOutputFileName = "DeliveryNote-various.pdf";
                                    }
                                    else
                                    {
                                        myWeb.mcOutputFileName = "DeliveryNote-" + orderId + ".pdf";
                                    }

                                    string DeliveryNoteXslPath = @"\xsl\docs\deliverynote.xsl";
                                    if (myWeb.bs5)
                                    {
                                        DeliveryNoteXslPath = @"\features\cart\docs\delivery-note.xsl";
                                    }

                                    myWeb.mcEwSiteXsl = ofs.checkCommonFilePath(moConfig["ProjectPath"] + DeliveryNoteXslPath);

                                    oCart.ListOrders(orderId, true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);

                                    myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.Custom1, (long)myWeb.mnUserId, 0L, 0L, 0L, "Print Delivery 2" + orderId, false);

                                    break;
                                }

                            case "PrintConfirm":
                                {
                                    break;
                                }


                            case "ResendReceipt":
                                {

                                    oCart.ListOrders(myWeb.moRequest["id"], true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);
                                    break;
                                }

                            case "RequestSettlement":
                                {
                                    oPageDetail.AppendChild(moAdXfm.xFrmRequestSettlement(Convert.ToInt64(myWeb.moRequest["id"])));
                                    oPageDetail.AppendChild(myWeb.moDbHelper.ActivityReport(Cms.dbHelper.ActivityType.Email, 0L, 0L, 0L, Convert.ToInt64(myWeb.moRequest["id"])));
                                    break;
                                }

                            default:
                                {
                                    switch (mcEwCmd ?? "")
                                    {
                                        case "Orders":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Complete, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersInProgress":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.InProgress, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersSaved":
                                        case "OrdersConfirmed":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Confirmed, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersAwaitingPayment":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.AwaitingPayment, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersShipped":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Shipped, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersRefunded":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Refunded, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersAbandoned":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Abandoned, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersFailed":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.PassForPayment, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersDeposit":
                                            {
                                                oCart.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.DepositPaid, ref oPageDetail);
                                                break;
                                            }
                                        case "OrdersHistory":
                                            {
                                                oCart.ListOrders(0.ToString(), true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);
                                                break;
                                            }
                                    }

                                    break;
                                }
                        }
                        sAdminLayout = cSchemaName + "s";
                    }
                    else if (myWeb.moRequest["ewCmd"].Contains("Quote"))
                    {
                        var oQuote = new Cms.Quote(ref myWeb);

                        switch (myWeb.moRequest["ewCmd2"] ?? "")
                        {

                            case "Display":
                                {


                                    oPageDetail.AppendChild(moAdXfm.xFrmUpdateOrder(Convert.ToInt64(myWeb.moRequest["id"]), cSchemaName));
                                    oQuote.ListOrders(myWeb.moRequest["id"], true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);
                                    break;
                                }

                            default:
                                {
                                    switch (myWeb.moRequest["ewCmd"] ?? "")
                                    {
                                        case "Quotes":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Complete, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesShipped":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Shipped, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesRefunded":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Refunded, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesAbandoned":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.Abandoned, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesFailed":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.PassForPayment, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesDeposit":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, (int)Cms.Cart.cartProcess.DepositPaid, ref oPageDetail);
                                                break;
                                            }
                                        case "QuotesHistory":
                                            {
                                                oQuote.ListOrders(0.ToString(), true, 0, ref oPageDetail, bForceRefresh: false, nUserId: 0L);
                                                break;
                                            }
                                    }

                                    break;
                                }
                        }
                        sAdminLayout = cSchemaName + "s";
                    }
                    else if (myWeb.moRequest["ewCmd"] == "CartActivity" | myWeb.moRequest["ewCmd"] == "CartReports")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(moAdXfm.xFrmCartActivity());
                        if (moAdXfm.valid)
                        {
                            oPageDetail.AppendChild(oCart.CartReports(Convert.ToDateTime(moAdXfm.Instance.FirstChild.SelectSingleNode("dBegin").InnerText), Convert.ToDateTime(moAdXfm.Instance.FirstChild.SelectSingleNode("dEnd").InnerText), Convert.ToInt16(moAdXfm.Instance.FirstChild.SelectSingleNode("bSplit").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cProductType").InnerText, Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nProductId").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cCurrencySymbol").InnerText, moAdXfm.Instance.FirstChild.SelectSingleNode("nOrderStatus").InnerText, moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderType").InnerText));
                        }
                        sAdminLayout = "CartActivity";
                    }
                    else if (myWeb.moRequest["ewCmd"] == "CartActivityDrilldown")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(moAdXfm.xFrmCartActivityDrillDown());
                        if (moAdXfm.valid)
                        {
                            string OrderSatus = Convert.ToString(moAdXfm.Instance.FirstChild.SelectSingleNode("nOrderStatus").InnerText);
                            if (OrderSatus.Contains(","))
                            {
                                string[] keys = OrderSatus.Split(',');
                                if (keys.Length > 0)
                                {
                                    nOrderStatus1 = Convert.ToInt32(keys[0]);
                                    nOrderStatus2 = Convert.ToInt32(keys[1]);
                                    nOrderStatus3 = Convert.ToInt32(keys[2]);
                                }
                            }
                            oPageDetail.AppendChild(oCart.CartReportsDrilldown(moAdXfm.Instance.FirstChild.SelectSingleNode("cGrouping").InnerText, Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nYear").InnerText), Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nMonth").InnerText), Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nDay").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cCurrencySymbol").InnerText, nOrderStatus1, nOrderStatus2, Convert.ToString(moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderType").InnerText)));
                        }
                        sAdminLayout = "CartActivityDrilldown";
                    }
                    else if (myWeb.moRequest["ewCmd"] == "CartActivityPeriod")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(moAdXfm.xFrmCartActivityPeriod());
                        if (moAdXfm.valid)
                        {
                            oPageDetail.AppendChild(oCart.CartReportsPeriod(moAdXfm.Instance.FirstChild.SelectSingleNode("cGroup").InnerText, Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nYear").InnerText), Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nMonth").InnerText), Convert.ToInt32(moAdXfm.Instance.FirstChild.SelectSingleNode("nWeek").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cCurrencySymbol").InnerText, moAdXfm.Instance.FirstChild.SelectSingleNode("nOrderStatus").InnerText, Convert.ToString(moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderType").InnerText)));
                        }
                        sAdminLayout = "CartActivityPeriod";
                    }
                    else if (myWeb.moRequest["ewCmd"] == "CartDownload")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(moAdXfm.xFrmCartOrderDownloads());
                        if (moAdXfm.valid)
                        {
                            oPageDetail.AppendChild(oCart.CartReportsDownload(Convert.ToDateTime(moAdXfm.Instance.FirstChild.SelectSingleNode("dBegin").InnerText), Convert.ToDateTime(moAdXfm.Instance.FirstChild.SelectSingleNode("dEnd").InnerText), moAdXfm.Instance.FirstChild.SelectSingleNode("cCurrencySymbol").InnerText, moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderType").InnerText, Convert.ToInt16(moAdXfm.Instance.FirstChild.SelectSingleNode("cOrderStage").InnerText)));
                        }
                        sAdminLayout = "CartDownload";
                    }

                    else if (myWeb.moRequest["ewCmd"] == "Ecommerce")
                    {
                        var oCart = new Cms.Cart(ref myWeb);
                        oPageDetail.AppendChild(oCart.CartOverview());
                    }
                }

                catch (Exception ex)
                {
                    myWeb.OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "OrderProcess", ex, sProcessInfo));
                }
            }

            private void ShippingLocationsProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                Cms.Cart oCart;

                try
                {
                    oCart = new Cms.Cart(ref myWeb);

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "edit":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmEditShippingLocation(Convert.ToInt64(myWeb.moRequest["id"]), Convert.ToInt64(myWeb.moRequest["parid"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                        case "movehere":
                            {
                                myWeb.moDbHelper.moveShippingLocation(Convert.ToInt64(myWeb.moRequest["id"]), Convert.ToInt64(myWeb.moRequest["parId"]));
                                break;
                            }
                        case "delete":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmDeleteShippingLocation(Convert.ToInt64(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                    }
                    if (string.IsNullOrEmpty(oPageDetail.InnerXml))
                    {
                        oCart.ListShippingLocations(ref oPageDetail);
                    }
                    oCart.close();
                    oCart = (Cms.Cart)null;
                }
                catch (Exception ex)
                {
                    myWeb.OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ShippingLocationsProcess", ex, sProcessInfo));
                }
            }

            private void DeliveryMethodProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                Cms.Cart oCart;

                try
                {
                    oCart = new Cms.Cart(ref myWeb);

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "edit":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmEditDeliveryMethod(Convert.ToInt64(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                        case "locations":
                            {
                                if (!string.IsNullOrEmpty(myWeb.moRequest["ewSubmit"]))
                                {
                                    myWeb.moDbHelper.updateShippingLocations(Convert.ToInt64(myWeb.moRequest["nShpOptId"]), myWeb.moRequest["aLocations"]);
                                }
                                else
                                {
                                    oCart.ListShippingLocations(ref oPageDetail, Convert.ToInt64("0" + myWeb.moRequest["id"]));
                                    sAdminLayout = "DeliveryMethodLocations";
                                }

                                break;
                            }
                        case "permissions":
                            {

                                sAdminLayout = "AdminXForm";
                                oPageDetail.AppendChild(moAdXfm.xFrmShippingDirRelations(Convert.ToInt64(myWeb.moRequest.QueryString["id"]), ""));
                                break;
                            }

                        case "ShippingGroup":
                            {
                                sAdminLayout = "AdminXForm";
                                oPageDetail.AppendChild(moAdXfm.xFrmProductShippingGroupRelations(Convert.ToInt64(myWeb.moRequest.QueryString["id"]), myWeb.moRequest.QueryString["name"]));
                                break;
                            }

                        case "delete":
                            {
                                // xFrmDeleteDeliveryMethod
                                oPageDetail.AppendChild(moAdXfm.xFrmDeleteDeliveryMethod(Convert.ToInt64(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                    sAdminLayout = "DeliveryMethods";
                                }

                                break;
                            }
                    }
                    if (string.IsNullOrEmpty(oPageDetail.InnerXml))
                    {
                        oCart.ListDeliveryMethods(ref oPageDetail);
                    }
                    oCart.close();
                    oCart = (Cms.Cart)null;
                }
                catch (Exception ex)
                {
                    myWeb.OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DeliveryMethodProcess", ex, sProcessInfo));
                }
            }

            private void CarriersProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                Cms.Cart oCart;

                try
                {
                    oCart = new Cms.Cart(ref myWeb);

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "edit":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmEditCarrier(Convert.ToInt64(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                        case "delete":
                            {
                                // xFrmDeleteDeliveryMethod
                                oPageDetail.AppendChild(moAdXfm.xFrmDeleteCarrier(Convert.ToInt64(myWeb.moRequest["id"])));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                    sAdminLayout = "Carriers";
                                }

                                break;
                            }
                    }
                    if (string.IsNullOrEmpty(oPageDetail.InnerXml))
                    {
                        oCart.ListCarriers(ref oPageDetail);
                        sAdminLayout = "Carriers";
                    }
                    oCart.close();
                    oCart = (Cms.Cart)null;
                }
                catch (Exception ex)
                {
                    myWeb.OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CarriersProcess", ex, sProcessInfo));
               }
            }

            private void PaymentProviderProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                Cms.Cart oCart;

                try
                {
                    oCart = new Cms.Cart(ref myWeb);

                    switch (myWeb.moRequest["ewCmd2"] ?? "")
                    {
                        case "edit":
                        case "add":
                            {
                                oPageDetail.AppendChild(moAdXfm.xFrmPaymentProvider(myWeb.moRequest["type"]));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                        case "delete":
                            {
                                // :TODO delete payment provider xform
                                oPageDetail.AppendChild(moAdXfm.xFrmDeletePaymentProvider(myWeb.moRequest["type"]));
                                if (!moAdXfm.valid)
                                {
                                    sAdminLayout = "AdminXForm";
                                }
                                else
                                {
                                    oPageDetail.RemoveAll();
                                }

                                break;
                            }
                    }
                    if (string.IsNullOrEmpty(oPageDetail.InnerXml))
                    {
                        oCart.ListPaymentProviders(ref oPageDetail);
                    }
                    oCart.close();
                    oCart = (Cms.Cart)null;
                }
                catch (Exception ex)
                {
                    myWeb.OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DeliveryMethodProcess", ex, sProcessInfo));
                }
            }



            private void ProductGroupsProcess(ref XmlElement oPageDetail, ref string sAdminLayout, int nGroupID = 0)
            {
                string sProcessInfo = "";
                sAdminLayout = "ProductGroups";
                string cSql;
                DataSet oDS;
                try
                {
                    cSql = "Select * From tblCartProductCategories";
                    oDS = myWeb.moDbHelper.GetDataSet(cSql, "ProductCategory", "ProductCategories");
                    if (oDS.Tables.Count == 1)
                    {
                        oDS.Tables["ProductCategory"].Columns.Add("Count", typeof(int));
                        oDS.Tables["ProductCategory"].Columns["Count"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables["ProductCategory"].Columns["nCatKey"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables["ProductCategory"].Columns["cCatSchemaName"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables["ProductCategory"].Columns["cCatForeignRef"].ColumnMapping = MappingType.Attribute;
                    }
                    cSql = "SELECT c.nContentKey AS id, c.cContentForiegnRef AS ref, c.cContentName AS name, c.cContentSchemaName AS type, c.cContentXmlBrief AS content, tblCartCatProductRelations.nCatProductRelKey AS relid, tblCartCatProductRelations.nCatId AS catid FROM tblContent c INNER JOIN tblCartCatProductRelations ON c.nContentKey = tblCartCatProductRelations.nContentId " + "WHERE (tblCartCatProductRelations.nCatId Is not Null) order by nDisplayOrder";
                    myWeb.moDbHelper.addTableToDataSet(ref oDS, cSql, "Content");

                    if (oDS.Tables.Count == 2)
                    {

                        if (oDS.Tables["Content"].Columns.Contains("parID"))
                        {
                            oDS.Tables["Content"].Columns["parId"].ColumnMapping = MappingType.Attribute;
                        }
                        foreach (DataColumn oDC in oDS.Tables["Content"].Columns)
                        {
                            if (!(oDC.ColumnName == "content"))
                                oDC.ColumnMapping = MappingType.Attribute;
                        }
                        oDS.Tables["Content"].Columns["content"].ColumnMapping = MappingType.SimpleContent;

                        oDS.Relations.Add("CatCont", oDS.Tables["ProductCategory"].Columns["nCatKey"], oDS.Tables["Content"].Columns["catid"], false);

                        oDS.Relations["CatCont"].Nested = true;
                    }
                    foreach (DataRow oDr in oDS.Tables["ProductCategory"].Rows)
                    {
                        oDr["Count"] = oDr.GetChildRows("CatCont").Length;
                        if (Convert.ToInt32(oDr["nCatKey"]) != nGroupID)
                        {
                            foreach (var oDr2 in oDr.GetChildRows("CatCont"))
                                oDr2.Delete();

                        }
                    }


                    var oElmt = oPageDetail.OwnerDocument.CreateElement("ProductCats");
                    oElmt.InnerXml = oDS.GetXml().Replace("&lt;", "<").Replace("&gt;", ">");

                    foreach (XmlElement contentElmt in oElmt.FirstChild.SelectNodes("ProductCategory/Content"))
                    {
                        XmlElement contentElmtL2 = (XmlElement)contentElmt.FirstChild;
                        foreach (XmlElement ChildElmts in (IEnumerable)contentElmtL2.SelectNodes("*"))
                            contentElmt.AppendChild(ChildElmts.Clone());
                        contentElmt.RemoveChild((XmlNode)contentElmtL2);
                    }
                    oPageDetail.AppendChild(oElmt.FirstChild);
                }


                catch (Exception ex)
                {
                    myWeb.OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DeliveryMethodProcess", ex, sProcessInfo));
                }
            }

            private void DiscountRulesProcess(ref XmlElement oPageDetail, ref string sAdminLayout)
            {
                string sProcessInfo = "";
                sAdminLayout = "DiscountRules";
                string cSql;
                DataSet oDS;
                try
                {
                    string status = myWeb.moRequest["isActive"];
                    string search = myWeb.moRequest["search"];
                    if (status == "1")
                    {
                        cSql = "Select *, a.nStatus as status, a.dPublishDate as publishDate, a.dExpireDate as expireDate From tblCartDiscountRules dr inner join tblaudit a on dr.nAuditid = a.nAuditKey where (a.dExpireDate >= getdate() or a.dExpireDate is null)  and a.nStatus=1 and dr.cDiscountCode not like '%VOUCHER' AND (dr.nUseLimit IS NULL OR dr.nUseLimit = 0 OR dr.nUseCount IS NULL OR dr.nUseCount < dr.nUseLimit )  order by a.dPublishDate desc";
                    }
                    else if (status == "0")
                    {
                        cSql = "Select *, a.nStatus as status, a.dPublishDate as publishDate, a.dExpireDate as expireDate From tblCartDiscountRules dr inner join tblaudit a on dr.nAuditid = a.nAuditKey where (a.dExpireDate <= getdate()  or a.nStatus=0) and dr.cDiscountCode not like '%VOUCHER' order by a.dPublishDate desc";
                    }
                    else if (status == "singleUse")
                    {
                        cSql = "Select *, a.nStatus as status, a.dPublishDate as publishDate, a.dExpireDate as expireDate From tblCartDiscountRules dr inner join tblaudit a on dr.nAuditid = a.nAuditKey where a.nStatus=0 and dr.cDiscountCode like '%VOUCHER' order by a.dPublishDate desc";
                    }
                    else
                    {
                        if (search != null)
                        {
                            cSql = "Select *, a.nStatus as status, a.dPublishDate as publishDate, a.dExpireDate as expireDate From tblCartDiscountRules dr inner join tblaudit a on dr.nAuditid = a.nAuditKey where (a.dExpireDate >= getdate() or a.dExpireDate is null)   and a.nStatus=1 and dr.cDiscountCode like '%" + search + "%' order by a.dPublishDate desc";
                        }
                        else
                        {
                            cSql = "Select *, a.nStatus as status, a.dPublishDate as publishDate, a.dExpireDate as expireDate From tblCartDiscountRules dr inner join tblaudit a on dr.nAuditid = a.nAuditKey where (a.dExpireDate >= getdate() or a.dExpireDate is null)   and a.nStatus=1 and dr.cDiscountCode not like '%VOUCHER'  AND (dr.nUseLimit IS NULL OR dr.nUseLimit = 0 OR dr.nUseCount IS NULL OR dr.nUseCount < dr.nUseLimit ) order by a.dPublishDate desc";
                        }
                    }


                    oDS = myWeb.moDbHelper.GetDataSet(cSql, "DiscountRule", "DiscountRules");
                    if (oDS.Tables.Count == 1)
                    {
                        oDS.Tables[0].Columns["nDiscountKey"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["nDiscountForeignRef"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["cDiscountName"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["cDiscountCode"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["bDiscountIsPercent"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountCompoundBehaviour"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountValue"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountMinPrice"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountMinQuantity"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nDiscountCat"].ColumnMapping = MappingType.Element;
                        oDS.Tables[0].Columns["nAuditId"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["status"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["publishDate"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["expireDate"].ColumnMapping = MappingType.Attribute;
                        oDS.Tables[0].Columns["cAdditionalXML"].ColumnMapping = MappingType.Element;

                        if (myWeb.moDbHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel"))
                        {
                            cSql = "SELECT tblDirectory.*, tblCartDiscountDirRelations.nDiscountDirRelationKey, tblCartDiscountDirRelations.nPermLevel, tblCartDiscountDirRelations.nDiscountId FROM tblCartDiscountDirRelations LEFT OUTER JOIN tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey WHERE (tblCartDiscountDirRelations.nDiscountDirRelationKey IS NOT NULL)";
                        }
                        else
                        {
                            cSql = "SELECT tblDirectory.*, tblCartDiscountDirRelations.nDiscountDirRelationKey, tblCartDiscountDirRelations.nDiscountId FROM tblCartDiscountDirRelations LEFT OUTER JOIN tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey WHERE (tblCartDiscountDirRelations.nDiscountDirRelationKey IS NOT NULL)";
                        }

                        myWeb.moDbHelper.addTableToDataSet(ref oDS, cSql, "Dir");
                        cSql = "SELECT tblCartProductCategories.*, tblCartDiscountProdCatRelations.nDiscountProdCatRelationKey, tblCartDiscountProdCatRelations.nProductCatId, tblCartDiscountProdCatRelations.nDiscountId FROM tblCartProductCategories RIGHT OUTER JOIN tblCartDiscountProdCatRelations ON tblCartProductCategories.nCatKey = tblCartDiscountProdCatRelations.nProductCatId"; // WHERE (tblCartProductCategories.cCatSchemaName = N'Discount')"
                        myWeb.moDbHelper.addTableToDataSet(ref oDS, cSql, "ProdCat");
                        if (oDS.Tables.Contains("Dir"))
                        {
                            oDS.Relations.Add("RelDiscDir", oDS.Tables["DiscountRule"].Columns["nDiscountKey"], oDS.Tables["Dir"].Columns["nDiscountId"], false);
                            oDS.Relations["RelDiscDir"].Nested = true;
                            oDS.Tables["Dir"].Columns["nDirKey"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Dir"].Columns["cDirName"].ColumnMapping = MappingType.Attribute;
                            if (myWeb.moDbHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel"))
                            {
                                oDS.Tables["Dir"].Columns["nPermLevel"].ColumnMapping = MappingType.Attribute;
                            }
                        }
                        if (oDS.Tables.Contains("ProdCat"))
                        {
                            oDS.Relations.Add("RelDiscProdCat", oDS.Tables["DiscountRule"].Columns["nDiscountKey"], oDS.Tables["ProdCat"].Columns["nDiscountId"], false);
                            oDS.Relations["RelDiscProdCat"].Nested = true;
                            oDS.Tables["ProdCat"].Columns["nCatKey"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["ProdCat"].Columns["cCatName"].ColumnMapping = MappingType.Attribute;
                        }
                    }
                    var oElmt = oPageDetail.OwnerDocument.CreateElement("DiscountRules");
                    oElmt.InnerXml = oDS.GetXml().Replace("&lt;", "<").Replace("&gt;", ">");
                    oPageDetail.AppendChild(oElmt.FirstChild);
                }
                catch (Exception ex)
                {
                    myWeb.OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DiscountRulesProcess", ex, sProcessInfo));
                }
            }

        }
    }
}
