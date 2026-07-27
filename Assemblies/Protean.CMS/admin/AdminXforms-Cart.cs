// ***********************************************************************
// $Library:     protean.cms.adminXforms
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.com)
// &Website:     eonic.com
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2022 Eonic Digital LLP.
// ***********************************************************************


//using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
//using System.Text.Json.Nodes;
using System.Web;
using System.Web.Configuration;
using System.Web.Services.Description;
using System.Xml;
using Protean.Providers.CDN;
using Protean.Providers.Membership;
using Protean.Providers.Payment;
using Protean.Tools;
using static System.Web.HttpUtility;
using static Lucene.Net.QueryParsers.QueryParser;
using static Protean.Cms;
using static Protean.stdTools;
using static Protean.Tools.Text;
using static Protean.Tools.Xml;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin
        {
            public partial class AdminXforms : xForm, IDisposable
            {

                public XmlElement xFrmDeleteDeliveryMethod(long id)
                {

                    XmlElement oFrmElmt;
                    XmlElement oElmt;

                    // Dim oDr As SqlDataReader
                    string cProcessInfo = "";


                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = moPageXML;

                        base.NewFrm("DeleteDeliveryMethod");

                        // Lets get the object
                        oElmt = moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", id.ToString());
                        if (id != 0L)
                        {
                            // oDr = moDbHelper.getDataReader("SELECT tblCartShippingMethods.* FROM tblCartShippingMethods WHERE nShipOptKey = " & id)
                            using (var oDR = moDbHelper.getDataReaderDisposable("SELECT tblCartShippingMethods.* FROM tblCartShippingMethods WHERE nShipOptKey = " + id))  // Done by nita on 6/7/22
                            {
                                while (oDR.Read())
                                    oElmt.SetAttribute("name", Convert.ToString(oDR["cShipOptName"]));
                            }
                        }

                        base.Instance.AppendChild(oElmt);


                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteDM", "", "Delete Delivery Method");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this Delivery Method: " + oElmt.GetAttribute("name"));
                        //oFrmElmt = (XmlElement)argoNode;


                        base.addSubmit(ref oFrmElmt, "", "Delete Delivery Method");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartShippingMethod, id);
                            }

                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmDeleteDeliveryMethod", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeleteCarrier(long id)
                {

                    XmlElement oFrmElmt;
                    XmlElement oElmt;

                    // Dim oDr As SqlDataReader
                    string cProcessInfo = "";


                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = moPageXML;

                        base.NewFrm("DeleteCarrier");

                        // Lets get the object
                        oElmt = moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", id.ToString());
                        if (id != 0L)
                        {
                            // oDr = moDbHelper.getDataReader("SELECT tblCartCarrier.* FROM tblCartCarrier WHERE nCarrierKey = " & id)
                            using (var oDr = moDbHelper.getDataReaderDisposable("SELECT tblCartCarrier.* FROM tblCartCarrier WHERE nCarrierKey = " + id))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                    oElmt.SetAttribute("name", Convert.ToString(oDr["cCarrierName"]));
                            }
                        }

                        base.Instance.AppendChild(oElmt);


                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteDM", "", "Delete Delivery Method");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this Carrier: " + oElmt.GetAttribute("name"));
                        //oFrmElmt = (XmlElement)argoNode;


                        base.addSubmit(ref oFrmElmt, "", "Delete Carrier");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartCarrier, id);
                            }

                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmDeleteCarrier", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeleteShippingLocation(long id)
                {

                    XmlElement oFrmElmt;
                    XmlElement oElmt;

                    // Dim oDr As SqlDataReader
                    string cProcessInfo = "";


                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = moPageXML;

                        base.NewFrm("DeleteShippingLocation");

                        // Lets get the object
                        oElmt = moPageXML.CreateElement("sType");
                        oElmt.SetAttribute("id", id.ToString());
                        if (id != 0L)
                        {
                            // oDr = moDbHelper.getDataReader("SELECT cLocationNameFull FROM tblCartShippingLocations WHERE nLocationKey = " & id)
                            using (var oDr = moDbHelper.getDataReaderDisposable("SELECT cLocationNameFull FROM tblCartShippingLocations WHERE nLocationKey = " + id))  // Done by nita on 6/7/22
                            {
                                while (oDr.Read())
                                    oElmt.SetAttribute("name", Convert.ToString(oDr[0]));
                            }
                        }

                        base.Instance.AppendChild(oElmt);


                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteDM", "", "Delete Shipping Location");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this Shipping Location: " + oElmt.GetAttribute("name") + " and all of its children?");
                        //oFrmElmt = (XmlElement)argoNode;

                        base.addSubmit(ref oFrmElmt, "", "Delete Shipping Location");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {

                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.CartShippingLocation, id);
                            }

                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmDeleteShippingLocation", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                // +++++++++++++++++++++++++++ Ecommerce Forms ++++++++++++++++++++++++++++++++'

                public XmlElement xFrmEditShippingLocation(long id = -1, long parId = -1)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    string cProcessInfo = "";
                    try
                    {
                        if (id == 0L)
                            id = -1;

                        base.NewFrm("EditShippingLocation");

                        base.submission("EditShippingLocation", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditShippingLocation", "", "Edit Shipping Location");
                        base.addInput(ref oFrmElmt, "nStructParId", true, "ParId", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nStructParId", "tblCartShippingLocations/nLocationParId", oBindParent: ref argoBindParent, "true()");

                        oSelElmt = base.addSelect1(ref oFrmElmt, "nLocationType", true, "Type", "required", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Global", 0.ToString());
                        base.addOption(ref oSelElmt, "Continental", 1.ToString());
                        base.addOption(ref oSelElmt, "Country", 2.ToString());
                        base.addOption(ref oSelElmt, "Region", 3.ToString());
                        base.addOption(ref oSelElmt, "County", 4.ToString());
                        base.addOption(ref oSelElmt, "Post Town", 5.ToString());
                        base.addOption(ref oSelElmt, "Postal Code", 5.ToString());
                        XmlElement argoBindParent1 = null;
                        base.addBind("nLocationType", "tblCartShippingLocations/nLocationType", oBindParent: ref argoBindParent1, "true()");

                        base.addInput(ref oFrmElmt, "cNameFull", true, "Full Name", "required");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cNameFull", "tblCartShippingLocations/cLocationNameFull", oBindParent: ref argoBindParent2, "true()");

                        base.addInput(ref oFrmElmt, "cNameShort", true, "Short Name", "required");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cNameShort", "tblCartShippingLocations/cLocationNameShort", oBindParent: ref argoBindParent3, "true()");

                        base.addInput(ref oFrmElmt, "cISOnum", true, "ISO Num");
                        XmlElement argoBindParent4 = null;
                        base.addBind("cISOnum", "tblCartShippingLocations/cLocationISOnum", oBindParent: ref argoBindParent4, "false()");

                        base.addInput(ref oFrmElmt, "cISOa2", true, "ISOa2");
                        XmlElement argoBindParent5 = null;
                        base.addBind("cISOa2", "tblCartShippingLocations/cLocationISOa2", oBindParent: ref argoBindParent5, "false()");

                        base.addInput(ref oFrmElmt, "cISOa3", true, "ISOa3");
                        XmlElement argoBindParent6 = null;
                        base.addBind("cISOa3", "tblCartShippingLocations/cLocationISOa3", oBindParent: ref argoBindParent6, "false()");

                        base.addInput(ref oFrmElmt, "cCode", true, "Code");
                        XmlElement argoBindParent7 = null;
                        base.addBind("cCode", "tblCartShippingLocations/cLocationCode", oBindParent: ref argoBindParent7, "false()");

                        base.addInput(ref oFrmElmt, "cTaxRate", true, "TaxRate");
                        XmlElement argoBindParent8 = null;
                        base.addBind("cTaxRate", "tblCartShippingLocations/nLocationTaxRate", oBindParent: ref argoBindParent8, "false()");

                        base.addSubmit(ref oFrmElmt, "ewSubmit", "Save Page");

                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartShippingLocation, id);
                        // set the parId for a new record
                        if (id < 0L)
                        {
                            Instance.SelectSingleNode("tblCartShippingLocations/nLocationParId").InnerText = parId.ToString();
                        }
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartShippingLocation, base.Instance);
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditShippingLocation", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmEditDeliveryMethod(long id = -1, long parId = -1)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oGrp1Elmt;
                    XmlElement oGrp2Elmt;
                    string cProcessInfo = "";
                    try
                    {
                        if (id == 0L)
                            id = -1;

                        base.NewFrm("EditShippingMethod");

                        base.submission("EditShippingMethod", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditDeliveryMethod", "2Col", "Edit Delivery Method");

                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Settings", "", "Content Settings");
                        oGrp2Elmt = base.addGroup(ref oFrmElmt, "Content", "", "Terms and Conditions");

                        XmlNode moPaymentCfg;
                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        // check we have differenct currencies
                        if (moPaymentCfg != null)
                        {
                            if (moPaymentCfg.SelectSingleNode("currencies/Currency") != null)
                            {
                                XmlElement oCurElmt;
                                oCurElmt = base.addSelect1(ref oGrp1Elmt, "cCurrency", true, "Currency Code");
                                XmlElement argoBindParent = null;
                                base.addBind("cCurrency", "tblCartShippingMethods/cCurrency", oBindParent: ref argoBindParent);
                                base.addOption(ref oCurElmt, "All", "");
                                foreach (XmlElement ocSetElmt in moPaymentCfg.SelectNodes("currencies/Currency"))
                                    base.addOption(ref oCurElmt, ocSetElmt.SelectSingleNode("name").InnerText, ocSetElmt.GetAttribute("ref"));
                            }
                        }



                        base.addInput(ref oGrp1Elmt, "cShipOptName", true, "Service Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cShipOptName", "tblCartShippingMethods/cShipOptName", oBindParent: ref argoBindParent1, "true()");

                        base.addInput(ref oGrp1Elmt, "cShipOptCarrier", true, "Carrier");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cShipOptCarrier", "tblCartShippingMethods/cShipOptCarrier", oBindParent: ref argoBindParent2, "false()");

                        base.addInput(ref oGrp1Elmt, "cShipOptTime", true, "Delivery Period");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cShipOptTime", "tblCartShippingMethods/cShipOptTime", oBindParent: ref argoBindParent3, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptCost", true, "Cost", "short");
                        XmlElement argoBindParent4 = null;
                        base.addBind("nShipOptCost", "tblCartShippingMethods/nShipOptCost", oBindParent: ref argoBindParent4, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptPercentage", true, "Percentage", "short");
                        XmlElement argoBindParent5 = null;
                        base.addBind("nShipOptPercentage", "tblCartShippingMethods/nShipOptPercentage", oBindParent: ref argoBindParent5, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptQuantMin", true, "Minimum Quantity", "short");
                        XmlElement argoBindParent6 = null;
                        base.addBind("nShipOptQuantMin", "tblCartShippingMethods/nShipOptQuantMin", oBindParent: ref argoBindParent6, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptQuantMax", true, "Maximum Quantity", "short");
                        XmlElement argoBindParent7 = null;
                        base.addBind("nShipOptQuantMax", "tblCartShippingMethods/nShipOptQuantMax", oBindParent: ref argoBindParent7, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptWeightMin", true, "Minimum Weight", "short");
                        XmlElement argoBindParent8 = null;
                        base.addBind("nShipOptWeightMin", "tblCartShippingMethods/nShipOptWeightMin", oBindParent: ref argoBindParent8, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptWeightMax", true, "Maximum Weight", "short");
                        XmlElement argoBindParent9 = null;
                        base.addBind("nShipOptWeightMax", "tblCartShippingMethods/nShipOptWeightMax", oBindParent: ref argoBindParent9, "false()");

                        if (myWeb.moDbHelper.checkTableColumnExists("tblCartShippingMethods", "nShipOptWeightOverageRate"))
                        {
                            base.addInput(ref oGrp1Elmt, "nShipOptWeightOverageUnit", true, "Overage Unit", "short");
                            XmlElement oElmtOverageUnit = null;
                            base.addBind("nShipOptWeightOverageUnit", "tblCartShippingMethods/nShipOptWeightOverageUnit", oBindParent: ref oElmtOverageUnit, "false()");

                            base.addInput(ref oGrp1Elmt, "nShipOptWeightOverageRate", true, "Overage Rate", "short");
                            XmlElement oElmtOverageRate = null;
                            base.addBind("nShipOptWeightOverageRate", "tblCartShippingMethods/nShipOptWeightOverageRate", oBindParent: ref oElmtOverageRate, "false()");
                        }

                        base.addInput(ref oGrp1Elmt, "nShipOptPriceMin", true, "Minimum Price", "short");
                        XmlElement argoBindParent10 = null;
                        base.addBind("nShipOptPriceMin", "tblCartShippingMethods/nShipOptPriceMin", oBindParent: ref argoBindParent10, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptPriceMax", true, "Maximum Price", "short");
                        XmlElement argoBindParent11 = null;
                        base.addBind("nShipOptPriceMax", "tblCartShippingMethods/nShipOptPriceMax", oBindParent: ref argoBindParent11, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptHandlingPercentage", true, "Handling Percent", "short");
                        XmlElement argoBindParent12 = null;
                        base.addBind("nShipOptHandlingPercentage", "tblCartShippingMethods/nShipOptHandlingPercentage", oBindParent: ref argoBindParent12, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptHandlingFixedCost", true, "Handling Fixed Cost", "short");
                        XmlElement argoBindParent13 = null;
                        base.addBind("nShipOptHandlingFixedCost", "tblCartShippingMethods/nShipOptHandlingFixedCost", oBindParent: ref argoBindParent13, "false()");

                        base.addInput(ref oGrp1Elmt, "nShipOptTaxRate", true, "Tax Rate", "short");
                        XmlElement argoBindParent14 = null;
                        base.addBind("nShipOptTaxRate", "tblCartShippingMethods/nShipOptTaxRate", oBindParent: ref argoBindParent14, "false()");

                        base.addInput(ref oGrp1Elmt, "nDisplayPriority", true, "Display Priority", "short");
                        XmlElement argoBindParent15 = null;
                        base.addBind("nDisplayPriority", "tblCartShippingMethods/nDisplayPriority", oBindParent: ref argoBindParent15, "false()");

                        string argsClass = "xhtml";
                        int argnRows = 0;
                        int argnCols = 0;
                        base.addTextArea(ref oGrp2Elmt, "cTerms", true, "Special Terms", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent16 = null;
                        base.addBind("cTerms", "tblCartShippingMethods/cShipOptTandC", oBindParent: ref argoBindParent16, "false()");

                        base.addInput(ref oGrp2Elmt, "dPublishDate", true, "Start Date", "calendar short");
                        XmlElement argoBindParent17 = null;
                        base.addBind("dPublishDate", "tblCartShippingMethods/dPublishDate", oBindParent: ref argoBindParent17, "false()");

                        base.addInput(ref oGrp2Elmt, "dExpireDate", true, "Expire Date", "calendar short");
                        XmlElement argoBindParent18 = null;
                        base.addBind("dExpireDate", "tblCartShippingMethods/dExpireDate", oBindParent: ref argoBindParent18, "false()");

                        if (moDbHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                        {
                            oSelElmt = base.addSelect(ref oGrp2Elmt, "bCollection", true, "Collection or Virtual Option", "multiline", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt, "Collection", "True");
                            XmlElement argoBindParent19 = null;
                            base.addBind("bCollection", "tblCartShippingMethods/bCollection", oBindParent: ref argoBindParent19, "false()");
                        }
                        // new column added
                        // Set if you need this shipping option to override all other shipping options, if this option is valid for any item in the cart.

                        if (moDbHelper.checkTableColumnExists("tblCartShippingMethods", "bOverrideForWholeOrder"))
                        {
                            oSelElmt = base.addSelect(ref oGrp2Elmt, "bOverrideForWholeOrder", true, "Override For Whole Order", "multiline", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt, "Override", "True");
                            XmlElement argoBindParent19 = null;
                            base.addBind("bOverrideForWholeOrder", "tblCartShippingMethods/bOverrideForWholeOrder", oBindParent: ref argoBindParent19, "false()");
                        }

                        oSelElmt = base.addSelect1(ref oGrp2Elmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Minimal);
                        base.addOption(ref oSelElmt, "Active", 1.ToString());
                        base.addOption(ref oSelElmt, "In-Active", 0.ToString());
                        XmlElement argoBindParent20 = null;
                        base.addBind("nStatus", "tblCartShippingMethods/nStatus", oBindParent: ref argoBindParent20, "true()");

                        base.addSubmit(ref oGrp2Elmt, "ewSubmit", "Save Method");

                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartShippingMethod, id);

                        if (id == -1)
                        {
                            // set some default values in the instance.
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptPercentage").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptQuantMin").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptQuantMax").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptWeightMin").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptWeightMax").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptPriceMin").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptPriceMax").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptHandlingPercentage").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptHandlingFixedCost").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nShipOptTaxRate").InnerText = "0";
                            Instance.SelectSingleNode("tblCartShippingMethods/nDisplayPriority").InnerText = "0";
                        }

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartShippingMethod, base.Instance);
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditDeliveryMethod", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmEditCarrier(long id = -1, long parId = -1)
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oGrp1Elmt;
                    XmlElement oGrp2Elmt;
                    string cProcessInfo = "";
                    try
                    {
                        if (id == 0L)
                            id = -1;

                        base.NewFrm("EditCarrier");

                        base.submission("EditCarrier", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditCarrier", "2Col", "Edit Carrier");

                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Settings", "", "Settings");
                        oGrp2Elmt = base.addGroup(ref oFrmElmt, "Content", "", "Carrier");


                        base.addInput(ref oGrp1Elmt, "dPublishDate", true, "Start Date", "calendar short");
                        XmlElement argoBindParent = null;
                        base.addBind("dPublishDate", "tblCartCarrier/dPublishDate", oBindParent: ref argoBindParent, "false()");

                        base.addInput(ref oGrp1Elmt, "dExpireDate", true, "Expire Date", "calendar short");
                        XmlElement argoBindParent1 = null;
                        base.addBind("dExpireDate", "tblCartCarrier/dExpireDate", oBindParent: ref argoBindParent1, "false()");


                        oSelElmt = base.addSelect1(ref oGrp1Elmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Active", 1.ToString());
                        base.addOption(ref oSelElmt, "In-Active", 0.ToString());
                        XmlElement argoBindParent2 = null;
                        base.addBind("nStatus", "tblCartCarrier/nStatus", oBindParent: ref argoBindParent2, "true()");


                        base.addInput(ref oGrp2Elmt, "cCarrierName", true, "Name");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cCarrierName", "tblCartCarrier/cCarrierName", oBindParent: ref argoBindParent3, "true()");

                        string argsClass = "xhtml";
                        int argnRows = 0;
                        int argnCols = 0;
                        base.addTextArea(ref oGrp2Elmt, "cCarrierTrackingInstructions", true, "Tracking Instructions", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent4 = null;
                        base.addBind("cCarrierTrackingInstructions", "tblCartCarrier/cCarrierTrackingInstructions", oBindParent: ref argoBindParent4, "false()");
                        //XmlNode argoNode = oGrp2Elmt;
                        base.addNote(ref oGrp2Elmt, Protean.xForm.noteTypes.Help, "{@code} will be replaced with the code entered at the time of sending");
                        //oGrp2Elmt = (XmlElement)argoNode;

                        base.addSubmit(ref oGrp2Elmt, "ewSubmit", "Save Method");

                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartCarrier, id);

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartCarrier, base.Instance);
                            }
                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditCarrier", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmPaymentProvider(string cProviderType)
                {

                    string cProcessInfo = "";
                    XmlElement oPaymentCfg;

                    try
                    {

                        var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                        DefaultSection oCfgSect = (DefaultSection)oCfg.GetSection("protean/payment");
                        oPaymentCfg = moPageXML.CreateElement("Config");
                        oPaymentCfg.InnerXml = oCfgSect.SectionInformation.GetRawXml();

                        // Replace Spaces with hypens
                        cProviderType = cProviderType.Replace(" ", "-");
                        string formPath = "/xforms/PaymentProvider/";
                        string filename = formPath + cProviderType + ".xml";
                        if (myWeb.bs5)
                        {
                            formPath = "/providers/payment/";
                            filename = formPath + cProviderType + "/config.xml";
                        }
                        if (!base.load(Convert.ToString(filename), myWeb.maCommonFolders))
                        {
                        }
                        // show xform load error message

                        else
                        {
                            // remove hyphens
                            cProviderType = cProviderType.Replace("-", "");
                            startImp();
                            // replace the instance if it exists in the web.config
                            if (oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']") != null)
                            {
                                base.Instance.InnerXml = oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']").OuterXml;
                            }

                            if (base.isSubmitted())
                            {
                                base.updateInstanceFromRequest();
                                base.validate();
                                if (base.valid)
                                {
                                    // here we update the web.config
                                    if (oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']") is null)
                                    {
                                        oPaymentCfg.SelectSingleNode("payment").AppendChild(base.Instance.FirstChild);
                                    }
                                    else
                                    {
                                        oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']").InnerXml = Instance.FirstChild.InnerXml;
                                    }

                                    oCfgSect.SectionInformation.RestartOnExternalChanges = false;
                                    oCfgSect.SectionInformation.SetRawXml(oPaymentCfg.InnerXml);
                                    oCfg.Save();

                                    // Copy file to secure if secure directory exists
                                    if (File.Exists(goServer.MapPath("protean.payment.config")))
                                    {
                                        var fsHelper = new Protean.fsHelper();
                                        fsHelper.CopyFile("protean.payment.config", "", @"\..\secure", true);
                                        fsHelper = (Protean.fsHelper)null;
                                    }
                                    // Copy file to secure if secure directory exists
                                    if (File.Exists(goServer.MapPath("Protean.Config")))
                                    {
                                        var fsHelper = new Protean.fsHelper();
                                        fsHelper.CopyFile("Protean.Config", "", @"\..\secure", true);
                                        fsHelper = (Protean.fsHelper)null;
                                    }
                                }
                            }
                            endImp();
                        }
                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditPaymentProvider", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDeletePaymentProvider(string cProviderType)
                {

                    XmlElement oFrmElmt;
                    string cProcessInfo = "";
                    XmlElement oPaymentCfg;

                    try
                    {

                        var oCfg = WebConfigurationManager.OpenWebConfiguration("/");
                        DefaultSection oCfgSect = (DefaultSection)oCfg.GetSection("protean/payment");
                        oPaymentCfg = moPageXML.CreateElement("Config");
                        oPaymentCfg.InnerXml = oCfgSect.SectionInformation.GetRawXml();

                        base.NewFrm("DeleteProvider");

                        base.submission("DeleteProvider", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "folderItem", "", "Delete Payment Provider");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this payment provider? - \"" + encodeAllHTML(cProviderType) + "\"");
                        //oFrmElmt = (XmlElement)argoNode;

                        base.addSubmit(ref oFrmElmt, "", "Delete Provider");

                        base.Instance.InnerXml = "<delete/>";

                        // remove hyphens
                        cProviderType = cProviderType.Replace("-", "");
                        startImp();
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                cProviderType = cProviderType.Replace(" ", "");
                                // here we update the web.config
                                if (oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']") is null)
                                {
                                }
                                // can find do nothing
                                else
                                {
                                    oPaymentCfg.SelectSingleNode("payment").RemoveChild(oPaymentCfg.SelectSingleNode("payment/provider[@name='" + cProviderType + "']"));
                                }

                                oCfgSect.SectionInformation.RestartOnExternalChanges = false;
                                oCfgSect.SectionInformation.SetRawXml(oPaymentCfg.InnerXml);
                                oCfg.Save();

                                // Copy file to secure if secure directory exists
                                var fsHelper = new Protean.fsHelper();
                                fsHelper.CopyFile("Protean.Config", "", @"\..\secure", true);
                                fsHelper = (Protean.fsHelper)null;

                            }
                            endImp();

                        }

                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditPaymentProvider", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmUpdateOrder(long nOrderId, string cSchemaName)
                {

                    XmlElement oFrmElmt;
                    XmlElement oSelElmt;
                    XmlElement oGrp1Elmt;
                    XmlElement oGrp2Elmt;
                    string cProcessInfo = "";
                    int nStatus;

                    XmlElement tempElement;

                    NameValueCollection moCartConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");


                    try
                    {

                        base.NewFrm("Update" + cSchemaName);

                        base.submission("Update" + cSchemaName, "", "post", "form_check(this)");

                        base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartOrder, nOrderId);
                        nStatus = Convert.ToInt16(base.Instance.SelectSingleNode("tblCartOrder/nCartStatus").InnerText);

                        // Add a note for shipped status goRequest("nStatus")
                        string shippedStatus = "Shipped";
                        string customerShippedTemplate = "";
                        bool sendEmailOnShipped = false;
                        if (moCartConfig != null)
                            customerShippedTemplate = moCartConfig["CustomerEmailShippedTemplatePath"];
                        if (!string.IsNullOrEmpty(customerShippedTemplate) && nStatus != 9 && File.Exists(goServer.MapPath(customerShippedTemplate)))

                        {
                            sendEmailOnShipped = true;
                            if (goRequest["nStatus"] != "9")
                                shippedStatus += " (Confirmation email will be sent to customer)";
                        }

                        string completedMsg = "";
                        if (moCartConfig["SendRecieptsFromAdmin"] != "off")
                        {
                            completedMsg = " - Payment Recieved (Resends receipt email)";
                        }

                        // update the status if we have submitted it allready
                        if (!string.IsNullOrEmpty(goRequest["nStatus"])) {
                            if (goRequest["nStatus"] == "9.1")
                            {
                                nStatus = 9;
                            }
                            else {

                                nStatus = Convert.ToInt16(goRequest["nStatus"]);
                            }
                        }
                         
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Update" + cSchemaName, "", "");
                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Status", "", cSchemaName + " Status");
                        oSelElmt = base.addSelect1(ref oGrp1Elmt, "nStatus", true, "Status", "", Protean.xForm.ApperanceTypes.Full);
                        switch (nStatus)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                            case 5: // new
                                {
                                    base.addOption(ref oSelElmt, "Abandoned", 11.ToString());
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString());
                                    break;
                                }
                            case 6: // Completed
                                {
                                    base.addOption(ref oSelElmt, "Awaiting Payment", 13.ToString(), false, "Awaiting_Payment");
                                    base.addOption(ref oSelElmt, "New Sale", 6.ToString(), false, "New Sale");
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString(), false, "Refunded");
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString(), false, "Shipped");
                                    base.addOption(ref oSelElmt, "Shipped (No email)", 9.ToString() + ".1", false, "No email");
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString(), false, "Delete");
                                    break;
                                }
                            case 7: // Refunded
                                {
                                    base.addOption(ref oSelElmt, "New Sale" + completedMsg, 6.ToString());
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString(), false, "Refunded");
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString(), false, "Delete");
                                    break;
                                }
                            case 8: // Failed
                                {
                                    base.addOption(ref oSelElmt, "Abandoned", 11.ToString(), false, "Abandoned");
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString(), false, "Delete");
                                    break;
                                }
                            case 9: // Shipped
                                {
                                    base.addOption(ref oSelElmt, "New Sale" + completedMsg, 6.ToString());
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString());
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString());
                                    break;
                                }
                            case 10: // Deposit Paid
                                {
                                    base.addOption(ref oSelElmt, "Deposit Paid", 10.ToString());
                                    base.addOption(ref oSelElmt, "New Sale" + completedMsg, 6.ToString());
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString());
                                    base.addOption(ref oSelElmt, "Shipped (No email)", 9.ToString() + ".1", false, "No email");
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString());
                                    break;
                                }
                            case 13: // Awaiting Payment
                                {
                                    base.addOption(ref oSelElmt, "Awaiting Payment", 13.ToString());
                                    base.addOption(ref oSelElmt, "New Sale" + completedMsg, 6.ToString());
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString());
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString());
                                    base.addOption(ref oSelElmt, "Shipped (No email)", 9.ToString() + ".1", false, "No email");
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString());
                                    break;
                                }
                            case 17: // In Progress
                                {
                                    base.addOption(ref oSelElmt, "Awaiting Payment", 13.ToString(), false, "Awaiting_Payment");
                                    base.addOption(ref oSelElmt, "New Sale", 6.ToString(), false, "New Sale");
                                    base.addOption(ref oSelElmt, "Refunded", 7.ToString(), false, "Refunded");
                                    base.addOption(ref oSelElmt, shippedStatus, 9.ToString(), false, "Shipped");
                                    base.addOption(ref oSelElmt, "Shipped (No email)", 9.ToString() + ".1", false, "No email");
                                    base.addOption(ref oSelElmt, "Delete", 12.ToString());
                                    break;
                                }

                        }
                        XmlElement argoBindParent = null;
                        base.addBind("nStatus", "tblCartOrder/nCartStatus", oBindParent: ref argoBindParent, "true()");

                        if (nStatus == 6 | myWeb.moRequest["nStatus"] == "9" | nStatus == 17)
                        {
                            // Add carrier information
                            XmlElement argoInsertBeforeNode = null;
                            var oSwitch = base.addSwitch(ref oGrp1Elmt, "", oInsertBeforeNode: ref argoInsertBeforeNode);
                            var oCase = base.addCase(ref oSwitch, "Awaiting_Payment");
                            var oCase1 = base.addCase(ref oSwitch, "New Sale");
                            var oCase2 = base.addCase(ref oSwitch, "Refunded");
                            var oCase3 = base.addCase(ref oSwitch, "Shipped");
                            //var oCase4 = base.addCase(ref oSwitch, "Processed");

                            // Turn of validation when switching back to completed
                            string validationOn = "true()";
                            if (nStatus == 6L & myWeb.moRequest["nStatus"] == "6")
                            {
                                validationOn = "false()";
                            }

                            if (moCartConfig["ShippedValidation"]?.ToLower() == "off")
                            {
                                validationOn = "false()";
                            }

                            if (moDbHelper.checkDBObjectExists("tblCartCarrier"))
                            {

                                var oCarrierElmt = base.addGroup(ref oCase3, "Carrier", "inline", cSchemaName + " Carrier");

                                var CarrierSelect = base.addSelect1(ref oCarrierElmt, "nCarrierId", true, "Carrier");
                                // Dim oDr As SqlDataReader = moDbHelper.getDataReader("select cCarrierName as name, nCarrierKey as value from tblCartCarrier")
                                using (var oDr = moDbHelper.getDataReaderDisposable("select cCarrierName as name, nCarrierKey as value from tblCartCarrier"))  // Done by nita on 6/7/22
                                {
                                    base.addOptionsFromSqlDataReader(CarrierSelect, oDr);
                                }

                                XmlElement argoBindParent1 = null;
                                base.addBind("nCarrierId", "tblCartOrderDelivery/nCarrierId", oBindParent: ref argoBindParent1, validationOn);

                                base.addInput(ref oCarrierElmt, "cCarrierRef", true, "Carrier Reference");
                                XmlElement argoBindParent2 = null;
                                base.addBind("cCarrierRef", "tblCartOrderDelivery/cCarrierRef", oBindParent: ref argoBindParent2, "false()");


                                base.addInput(ref oCarrierElmt, "cCarrierNotes", true, "Carrier Notes", "long");
                                XmlElement argoBindParent3 = null;
                                base.addBind("cCarrierNotes", "tblCartOrderDelivery/cCarrierNotes", oBindParent: ref argoBindParent3, "false()");

                                string validClass = "";
                                if (validationOn == "true()")
                                {
                                    validClass = " required";
                                }

                                base.addInput(ref oCarrierElmt, "dExpectedDeliveryDate", true, "Target Delivery Date", "calendar" + validClass);
                                XmlElement argoBindParent4 = null;
                                base.addBind("dExpectedDeliveryDate", "tblCartOrderDelivery/dExpectedDeliveryDate", oBindParent: ref argoBindParent4, validationOn);

                                base.addInput(ref oCarrierElmt, "dCollectionDate", true, "Collection Date", "calendar" + validClass);
                                XmlElement argoBindParent5 = null;
                                base.addBind("dCollectionDate", "tblCartOrderDelivery/dCollectionDate", oBindParent: ref argoBindParent5, validationOn);

                                var deliveryInstance = moPageXML.CreateElement("instance");
                                deliveryInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartDelivery);

                                deliveryInstance.SelectSingleNode("tblCartOrderDelivery/dCollectionDate").InnerText = XmlDate(DateTime.Now);

                                base.Instance.AppendChild(deliveryInstance.FirstChild);

                                // set the order id
                                base.Instance.SelectSingleNode("tblCartOrderDelivery/nOrderId").InnerText = nOrderId.ToString();
                            }
                        }

                        oGrp2Elmt = base.addGroup(ref oFrmElmt, "Notes", "", "Notes");

                        // Get the seller notes
                        string sellerNotes = Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText;

                        tempElement = (XmlElement)base.addDiv(ref oGrp2Elmt, sellerNotes, "orderNotes", false);

                        string[] aSellerNotes = sellerNotes.Split(new[] { "/n" }, StringSplitOptions.None);
                        string cSellerNotesHtml = "<ul>";
                        for (int snCount = 0, loopTo = aSellerNotes.Length - 1; snCount <= loopTo; snCount++)
                        {
                            string replaceWith = " ";
                            string removedBreaks = convertEntitiesToCodes(aSellerNotes[snCount]).Replace("\r\n", replaceWith).Replace("\n", replaceWith).Replace("\r", replaceWith).Replace("\\n", replaceWith);
                            cSellerNotesHtml = cSellerNotesHtml + "<li>" + removedBreaks + "</li>";
                        }
                        ;
                        tempElement.InnerXml = cSellerNotesHtml + "</ul>";

                        string argsClass = "";
                        int argnRows = Convert.ToInt16("2");
                        int argnCols = 0;
                        base.addTextArea(ref oGrp2Elmt, "cNotesAmend", false, "Add comment to notes (not sent to customer)", ref argsClass, ref argnRows, nCols: ref argnCols);

                        base.addSubmit(ref oGrp2Elmt, "ewUpdate" + cSchemaName, "Update Order");

                        if (base.isSubmitted())
                        {
                            // MyBase.updateInstanceFromRequest()
                            base.Instance.SelectSingleNode("tblCartOrder/nCartStatus").InnerText = goRequest["nStatus"];
                            string sStatusDesc;
                            switch (goRequest["nStatus"] ?? "")
                            {
                                case "0":
                                    {
                                        sStatusDesc = "New Cart";
                                        break;
                                    }
                                case "1":
                                    {
                                        sStatusDesc = "Items Added";
                                        break;
                                    }
                                case "2":
                                    {
                                        sStatusDesc = "Billing Address Added";
                                        break;
                                    }
                                case "3":
                                    {
                                        sStatusDesc = "Delivery Address Added";
                                        break;
                                    }
                                case "4":
                                    {
                                        sStatusDesc = "Confirmed";
                                        break;
                                    }
                                case "5":
                                    {
                                        sStatusDesc = "Pass for Payment";
                                        break;
                                    }
                                case "6":
                                    {
                                        sStatusDesc = "New Sale";
                                        break;
                                    }
                                case "7":
                                    {
                                        sStatusDesc = "Refunded";
                                        break;
                                    }
                                case "8":
                                    {
                                        sStatusDesc = "Failed";
                                        break;
                                    }
                                case "9":
                                    {
                                        sStatusDesc = "Shipped";
                                        break;
                                    }
                                case "9.1":
                                    {
                                        sStatusDesc = "Shipped";
                                        sendEmailOnShipped = false;
                                        break;
                                    }
                                case "10":
                                    {
                                        sStatusDesc = "Deposit Paid";
                                        break;
                                    }
                                case "11":
                                    {
                                        sStatusDesc = "Abandoned";
                                        break;
                                    }
                                case "12":
                                    {
                                        sStatusDesc = "Deleted";
                                        break;
                                    }
                                case "13":
                                    {
                                        sStatusDesc = "Awaiting Payment";
                                        break;
                                    }

                                default:
                                    {
                                        sStatusDesc = "No Change";
                                        break;
                                    }
                            }

                            string updateNotes = goRequest["cNotesAmend"];
                            // If Not String.IsNullOrEmpty(updateNotes) Then updateNotes = ControlChars.CrLf & Now.ToString() & ":" & updateNotes

                            string notes = base.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText + "/n" + Convert.ToString(DateTime.Now) + ": changed to: (" + goRequest["nStatus"] + ") " + sStatusDesc + " - " + updateNotes;
                            string AdminUserName = myWeb.moPageXml.SelectSingleNode("Page/User/@name").InnerText;
                            notes += " - By user: " + AdminUserName;

                            base.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText = notes;
                            moDbHelper.logActivity(Cms.dbHelper.ActivityType.OrderStatusChange, (long)myWeb.mnUserId, 0L, 0L, notes);

                            aSellerNotes = base.Instance.SelectSingleNode("tblCartOrder/cSellerNotes").InnerText.Split(new[] { "/n" }, StringSplitOptions.None);
                            cSellerNotesHtml = "<ul>";
                            for (int snCount = 0, loopTo1 = aSellerNotes.Length - 1; snCount <= loopTo1; snCount++)
                                cSellerNotesHtml = cSellerNotesHtml + "<li>" + convertEntitiesToCodes(aSellerNotes[snCount]) + "</li>";
                            tempElement.InnerXml = cSellerNotesHtml + "</ul>";

                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartOrder, base.Instance);

                                if (Convert.ToDouble(goRequest["nStatus"]) == 9d)
                                {

                                    // Get the carrier name from the ID
                                    if (moDbHelper.checkDBObjectExists("tblCartCarrier") & moDbHelper.checkDBObjectExists("tblCartOrderDelivery"))
                                    {
                                        string CarrierName;
                                        CarrierName = Convert.ToString(moDbHelper.GetDataValue("select cCarrierName from tblCartCarrier where nCarrierKey = " + myWeb.moRequest["nCarrierId"]));
                                        Instance.SelectSingleNode("tblCartOrderDelivery/cCarrierName").InnerText = CarrierName;
                                        moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartDelivery, base.Instance);
                                    }

                                    if (sendEmailOnShipped)
                                    {
                                        string cSubject = moCartConfig["OrderEmailSubject"];
                                        if (string.IsNullOrEmpty(cSubject))
                                            cSubject = "Order Shipped";
                                        // send to customer
                                        var oMsg = new Protean.Messaging(ref myWeb.msException);
                                        var cartXml = new XmlDocument();
                                        var cartElement = cartXml.CreateElement("Cart");
                                        cartElement.InnerXml = base.Instance.SelectSingleNode("tblCartOrder/cCartXml").InnerXml;
                                        cartElement.SetAttribute("InvoiceRef", moCartConfig["OrderNoPrefix"] + base.Instance.SelectSingleNode("tblCartOrder/nCartOrderKey").InnerXml);
                                        string invoiceDate = base.Instance.SelectSingleNode("tblCartOrder/dInsertDate").InnerXml;
                                        cartElement.SetAttribute("InvoiceDate", invoiceDate.Length >= 10 ? invoiceDate.Substring(0, 10) : invoiceDate);
                                        cartElement.SetAttribute("AccountId", base.Instance.SelectSingleNode("tblCartOrder/nCartUserDirId").InnerXml);
                                        if (base.Instance.SelectSingleNode("tblCartOrderDelivery") != null)
                                        {
                                            var delElmt = cartXml.CreateElement("Delivery");
                                            delElmt.InnerXml = base.Instance.SelectSingleNode("tblCartOrderDelivery").InnerXml;
                                            cartElement.AppendChild(delElmt);

                                            long carrierId = Convert.ToInt16(delElmt.SelectSingleNode("nCarrierId").InnerText);
                                            var carrierElmt = cartXml.CreateElement("Carrier");
                                            carrierElmt.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartCarrier, carrierId).Replace("{@code}", delElmt.SelectSingleNode("cCarrierRef").InnerText);
                                            cartElement.AppendChild(carrierElmt);
                                        }
                                        string CustomerEmailShippedTemplatePath = !string.IsNullOrEmpty(moCartConfig["CustomerEmailShippedTemplatePath"]) ? moCartConfig["CustomerEmailShippedTemplatePath"] : "/xsl/Cart/mailOrderCustomerDelivery.xsl";
                                        Cms.dbHelper argodbHelper = null;
                                        cProcessInfo = Convert.ToString(oMsg.emailer(cartElement, CustomerEmailShippedTemplatePath, moCartConfig["MerchantName"], moCartConfig["MerchantEmail"], cartElement.SelectSingleNode("//Contact[@type='Billing Address']/Email").InnerText, "Order Shipped", bccRecipient: moCartConfig["MerchantEmailShippedBcc"], odbHelper: ref argodbHelper));

                                        oMsg = (Protean.Messaging)null;
                                    }
                                }
                            }
                        }
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmUpdateOrder", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmRefundOrder(long nOrderId, string providerName, string providerPaymentReference)
                {

                    string cProcessInfo = "";
                    NameValueCollection moCartConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
                    var oCfg = WebConfigurationManager.OpenWebConfiguration("/" + myWeb.moConfig["ProjectPath"]);
                    DefaultSection oCgfSect = (DefaultSection)oCfg.GetSection("protean/web");
                    try
                    {
                        string IsRefund = "";
                        var oCart = new Cms.Cart(ref myWeb);
                        base.NewFrm("Refund");
                        base.submission("Refund", "", "post", "form_check(this)");
                        decimal refundAmount;
                        //string cResponse = "";   // check this
                        var xdoc = new XmlDocument();
                        var amount = default(double);
                        if (nOrderId > 0L)
                        {
                            string cartXmlSql = "select cCartXml from tblCartOrder where nCartOrderKey = " + nOrderId;
                            if (!string.IsNullOrEmpty(cartXmlSql))
                            {
                                string orderXml = Convert.ToString(myWeb.moDbHelper.GetDataValue(cartXmlSql));
                                xdoc.LoadXml(orderXml);
                            }
                            if (!string.IsNullOrEmpty(xdoc.InnerXml))
                            {

                                // Dim xn As XmlNode = xdoc.SelectSingleNode("/Order/PaymentDetails/instance/Response")
                                var xnInstance = xdoc.SelectSingleNode("/Order/PaymentDetails/*[1]");
                                if (xnInstance != null)
                                {
                                    amount = Convert.ToDouble("0" + xnInstance.Attributes["AmountPaid"].InnerText);
                                }
                            }

                        }

                        refundAmount = (decimal)amount;

                        base.Instance.InnerXml = "<Refund><RefundAmount> " + refundAmount + " </RefundAmount><ProviderName>" + providerName + "</ProviderName> <ProviderReference>" + providerPaymentReference + " </ProviderReference><OrderId>" + nOrderId + "</OrderId></Refund>";
                        XmlElement oFrmElmt;
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Refund " + providerName, "", "");
                        base.addInput(ref oFrmElmt, "RefundAmount", true, "Refund Amount");
                        XmlElement argoBindParent = null;
                        base.addBind("RefundAmount", "Refund/RefundAmount", oBindParent: ref argoBindParent, "true()");

                        base.addInput(ref oFrmElmt, "ProviderName", true, "Provider Name", "readonly");
                        XmlElement argoBindParent1 = null;
                        base.addBind("ProviderName", "Refund/ProviderName", oBindParent: ref argoBindParent1, "true()");

                        base.addInput(ref oFrmElmt, "ProviderReference", true, "Provider Reference", "readonly");
                        XmlElement argoBindParent2 = null;
                        base.addBind("ProviderReference", "Refund/ProviderReference", oBindParent: ref argoBindParent2, "true()");

                        base.addInput(ref oFrmElmt, "id", true, "Order Id", "readonly");
                        XmlElement argoBindParent3 = null;
                        base.addBind("id", "Refund/OrderId", oBindParent: ref argoBindParent3, "true()");

                        base.addSubmit(ref oFrmElmt, "Refund", "Refund", "ewSubmit");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (amount >= (double)refundAmount)
                            {
                                if (base.valid)
                                {
                                    // oCgfSect.SectionInformation.RestartOnExternalChanges = False    'check this
                                    // oCgfSect.SectionInformation.SetRawXml(MyBase.Instance.InnerXml)
                                    // oCfg.Save()

                                    //var oPayProv = new Protean.Providers.Payment.BaseProvider(ref this.myWeb, providerName);
                                    Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                                    IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, providerName);
                                    IsRefund = Convert.ToString(oPaymentProv.Activities.RefundPayment(providerPaymentReference, refundAmount));
                                    if (IsRefund.StartsWith("Error"))
                                    {
                                        // XmlNode argoNode = oFrmElmt;
                                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Refund Failed:" + IsRefund);
                                        // oFrmElmt = (XmlElement)argoNode;
                                        // myWeb.msRedirectOnEnd = "/?ewCmd=Orders&ewCmd2=Display&id=" + nOrderId
                                        base.valid = false;
                                    }
                                    // Update Seller Notes:
                                    string sSql = "select * from tblCartOrder where nCartOrderKey = " + nOrderId;
                                    DataSet oDs;
                                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                                    {
                                        if (IsRefund != null)
                                        {
                                            oRow["cSellerNotes"] = oRow["cSellerNotes"] + Environment.NewLine + DateTime.Today + " " + DateTime.Now.TimeOfDay + ": changed to: (Refund Payment Successful) " + Environment.NewLine + "comment: " + "Refund amount:" + refundAmount + Environment.NewLine + "Full Response:' Refunded Amount is " + refundAmount + " And ReceiptId is: " + IsRefund + "'";
                                        }
                                        else
                                        {
                                            oRow["cSellerNotes"] = oRow["cSellerNotes"] + Environment.NewLine + DateTime.Today + " " + DateTime.Now.TimeOfDay + ": changed to: (Refund Payment Failed) " + Environment.NewLine + "comment: " + "Refund amount:" + refundAmount + Environment.NewLine + "Full Response:' Refunded Amount is " + refundAmount + " And Error is: " + IsRefund + "'";
                                        }
                                    }
                                    myWeb.moDbHelper.updateDataset(ref oDs, "Order");

                                    if (IsRefund != null)
                                    {
                                        moDbHelper.savePayment((int)nOrderId, mnUserId, providerName, providerPaymentReference, "Refund", (XmlElement)null, default(DateTime), false, (double)(refundAmount * -1), "refund");
                                    }
                                }
                            }
                        }
                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmRefundOrder", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmFindRelated(string nParentID, string cContentType, ref XmlElement oPageDetail, string nParId, bool bIgnoreParID, string cTableName, string cSelectField, string cFilterField, string redirect = "")
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt1;
                    XmlElement oSelElmt2;
                    var oTempInstance = moPageXML.CreateElement("instance");
                    //bool bCascade = false;
                    string cProcessInfo = "";
                    try
                    {
                        string cParentContentName = Xml.convertEntitiesToCodes(moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Content, Convert.ToInt64(nParentID)));

                        base.NewFrm("FindRelatedContent");
                        base.Instance.InnerXml = "<nParentContentId>" + nParentID + "</nParentContentId>" + "<cSchemaName>" + cContentType + "</cSchemaName>" + "<cSection/><nSearchChildren/><nSearchInactive/><nIncludeRelated/><cParentContentName>" + cParentContentName + "</cParentContentName><redirect>" + redirect + "</redirect><cSearch/>";

                        // MyBase.submission("AddRelated", "?ewCmd=RelateSearch&Type=Document&xml=x", "post", "form_check(this)")
                        base.submission("AddRelated", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "SearchRelated", sLabel: "Search For Related " + cContentType);

                        // Definitions
                        if (!string.IsNullOrEmpty(redirect))
                        {
                            base.addInput(ref oFrmElmt, "redirect", true, "redirect", "hidden");
                            XmlElement argoBindParent = null;
                            base.addBind("redirect", "redirect", oBindParent: ref argoBindParent);
                        }
                        base.addInput(ref oFrmElmt, "nParentContentId", true, "nParentContentId", "hidden");
                        XmlElement argoBindParent1 = null;
                        base.addBind("nParentContentId", "nParentContentId", oBindParent: ref argoBindParent1);

                        base.addInput(ref oFrmElmt, "cSchemaName", true, "cSchemaName", "hidden");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cSchemaName", "cSchemaName", oBindParent: ref argoBindParent2);

                        // What we are searching for
                        base.addInput(ref oFrmElmt, "cSearch", true, "Search Text");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cSearch", "cSearch", oBindParent: ref argoBindParent3, "false()");

                        // Pages
                        oSelElmt1 = base.addSelect1(ref oFrmElmt, "cSection", false, "Page", "", Protean.xForm.ApperanceTypes.Minimal);
                        base.addOption(ref oSelElmt1, "All", 0.ToString());
                        base.addOption(ref oSelElmt1, "All Orphan " + cContentType + "s", (-1).ToString());
                        string cSQL;
                        cSQL = "SELECT tblContentStructure.* FROM tblContentStructure ORDER BY nStructOrder";
                        var oDS = new DataSet();
                        oDS = moDbHelper.GetDataSet(cSQL, "Menu", "Struct");
                        oDS.Relations.Add("RelMenu", oDS.Tables["Menu"].Columns["nStructKey"], oDS.Tables["Menu"].Columns["nStructParID"], false);
                        oDS.Relations["RelMenu"].Nested = true;
                        var oMenuXml = new XmlDocument();
                        oMenuXml.InnerXml = oDS.GetXml();
                        foreach (XmlElement oMenuElmt in oMenuXml.SelectNodes("descendant-or-self::Menu"))
                        {
                            var oTmpNode = oMenuElmt;
                            string cNameString = "";
                            while (oTmpNode.ParentNode.Name != "Struct")
                            {
                                cNameString += "-";
                                oTmpNode = (XmlElement)oTmpNode.ParentNode;
                            }
                            cNameString += oMenuElmt.SelectSingleNode("cStructName").InnerText;
                            base.addOption(ref oSelElmt1, cNameString, oMenuElmt.SelectSingleNode("nStructKey").InnerText);

                        }
                        XmlElement argoBindParent4 = null;
                        base.addBind("cSection", "cSection", oBindParent: ref argoBindParent4, "true()");
                        // Search sub pages
                        oSelElmt2 = base.addSelect(ref oFrmElmt, "nSearchChildren", true, "", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt2, "Search all sub-pages", 1.ToString());
                        XmlElement argoBindParent5 = null;
                        base.addBind("nSearchChildren", "nSearchChildren", oBindParent: ref argoBindParent5, "false()");

                        if (cContentType.Contains("Product") & cContentType.Contains("SKU"))
                        {
                            oSelElmt2 = base.addSelect(ref oFrmElmt, "nIncludeRelated", true, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt2, "Include Related Items", 1.ToString());
                            XmlElement argoBindParent6 = null;
                            base.addBind("nIncludeRelated", "nIncludeRelated", oBindParent: ref argoBindParent6, "false()");
                        }

                        // Search inactive pages
                        oSelElmt2 = base.addSelect(ref oFrmElmt, "nSearchInactive", true, "", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt2, "In Active", 1.ToString());
                        XmlElement argoBindParent7 = null;
                        base.addBind("nSearchInactive", "nSearchInactive", oBindParent: ref argoBindParent7, "false()");

                        // search button
                        base.addSubmit(ref oFrmElmt, "Search", "Search", "ewSubmit");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                // Dim nPar As Integer = goRequest.QueryString("GroupId")
                                if (!Tools.Number.IsNumeric(nParId))
                                {
                                    XmlElement oParElmt = (XmlElement)base.Instance.SelectSingleNode(nParId);
                                    if (oParElmt != null)
                                        nParId = oParElmt.InnerText;
                                }
                                int nRoot = Convert.ToInt16(base.Instance.SelectSingleNode("cSection").InnerText);
                                bool bChilds = base.Instance.SelectSingleNode("nSearchChildren").InnerText == "1";
                                string cExpression = base.Instance.SelectSingleNode("cSearch").InnerText;
                                bool bIncRelated = base.Instance.SelectSingleNode("nIncludeRelated").InnerText == "1";
                                bool binactive = base.Instance.SelectSingleNode("nSearchInactive").InnerText == "1";

                                string sSQL = "Select " + cSelectField + " From " + cTableName + " WHERE " + cFilterField + " = " + nParId;
                                using (var oDre = moDbHelper.getDataReaderDisposable(sSQL))  // Done by nita on 6/7/22
                                {
                                    string cTmp = "";
                                    while (oDre.Read())
                                        cTmp = cTmp + oDre[0] + ",";

                                    if (!string.IsNullOrEmpty(cTmp))
                                        cTmp = cTmp.Substring(0, cTmp.Length - 1);
                                    oPageDetail.AppendChild(moDbHelper.RelatedContentSearch(nRoot, cContentType, bChilds, cExpression, Convert.ToInt32(nParId), bIgnoreParID ? 0 : Convert.ToInt32(nParId), cTmp.Split(','), bIncRelated, binactive));

                                }
                            }
                        }

                        else
                        {
                            if (myWeb.moRequest["pgid"] != null)
                            {
                                base.Instance.InnerXml = "<nParentContentId>" + nParentID + "</nParentContentId>" + "<cSchemaName>" + cContentType + "</cSchemaName>" + "<cSection>" + myWeb.moRequest["pgid"].ToString() + "</cSection>" + "<nSearchChildren>1</nSearchChildren>" + "<nSearchInactive>0</nSearchInactive>" + "<cParentContentName>" + cParentContentName + "</cParentContentName>" + "<redirect>" + redirect + "</redirect><cSearch/>";
                            }
                            else
                            {
                                base.Instance.InnerXml = "<nParentContentId>" + nParentID + "</nParentContentId>" + "<cSchemaName>" + cContentType + "</cSchemaName>" + "<cSection>0</cSection>" + "<nSearchChildren>1</nSearchChildren>" + "<nSearchInactive>0</nSearchInactive>" + "<cParentContentName>" + cParentContentName + "</cParentContentName>" + "<redirect>" + redirect + "</redirect><cSearch/>";
                            }

                            base.addValues();
                        }
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmFindRelated", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                // New xForm for return product for change 
                public XmlElement xFrmFindParent(string nParentID, string childId, string cContentType, ref XmlElement oPageDetail, string nParId, bool bIgnoreParID, string cTableName, string cSelectField, string cFilterField, string redirect = "")
                {
                    XmlElement oFrmElmt;
                    XmlElement oSelElmt1;
                    XmlElement oSelElmt2;
                    var oTempInstance = moPageXML.CreateElement("instance");
                    //bool bCascade = false;
                    string cProcessInfo = "";
                    try
                    {
                        string cParentContentName = Xml.convertEntitiesToCodes(moDbHelper.getNameByKey(Cms.dbHelper.objectTypes.Content, Convert.ToInt64(nParentID)));

                        base.NewFrm("FindRelatedContent");
                        base.Instance.InnerXml = "<nParentContentId>" + nParentID + "</nParentContentId>" + "<cSchemaName>" + cContentType + "</cSchemaName>" + "<cSection/><nSearchChildren/><nIncludeRelated/><cParentContentName>" + cParentContentName + "</cParentContentName><redirect>" + redirect + "</redirect><cSearch/>";

                        // MyBase.submission("AddRelated", "?ewCmd=RelateSearch&Type=Document&xml=x", "post", "form_check(this)")
                        base.submission("AddRelated", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "SearchRelated", sLabel: "Search For " + cContentType);

                        // Definitions
                        if (!string.IsNullOrEmpty(redirect))
                        {
                            base.addInput(ref oFrmElmt, "redirect", true, "redirect", "hidden");
                            XmlElement argoBindParent = null;
                            base.addBind("redirect", "redirect", oBindParent: ref argoBindParent);
                        }
                        base.addInput(ref oFrmElmt, "nParentContentId", true, "nParentContentId", "hidden");
                        XmlElement argoBindParent1 = null;
                        base.addBind("nParentContentId", "nParentContentId", oBindParent: ref argoBindParent1);

                        base.addInput(ref oFrmElmt, "cSchemaName", true, "cSchemaName", "hidden");
                        XmlElement argoBindParent2 = null;
                        base.addBind("cSchemaName", "cSchemaName", oBindParent: ref argoBindParent2);

                        // What we are searching for
                        base.addInput(ref oFrmElmt, "cSearch", true, "Search Text");
                        XmlElement argoBindParent3 = null;
                        base.addBind("cSearch", "cSearch", oBindParent: ref argoBindParent3, "false()");

                        // Pages
                        oSelElmt1 = base.addSelect1(ref oFrmElmt, "cSection", false, "Page", "", Protean.xForm.ApperanceTypes.Minimal);
                        base.addOption(ref oSelElmt1, "All", 0.ToString());
                        base.addOption(ref oSelElmt1, "All Orphan " + cContentType + "s", (-1).ToString());
                        string cSQL;
                        cSQL = "SELECT tblContentStructure.* FROM tblContentStructure ORDER BY nStructOrder";
                        var oDS = new DataSet();
                        oDS = moDbHelper.GetDataSet(cSQL, "Menu", "Struct");
                        oDS.Relations.Add("RelMenu", oDS.Tables["Menu"].Columns["nStructKey"], oDS.Tables["Menu"].Columns["nStructParID"], false);
                        oDS.Relations["RelMenu"].Nested = true;
                        var oMenuXml = new XmlDocument();
                        oMenuXml.InnerXml = oDS.GetXml();
                        foreach (XmlElement oMenuElmt in oMenuXml.SelectNodes("descendant-or-self::Menu"))
                        {
                            var oTmpNode = oMenuElmt;
                            string cNameString = "";
                            while (oTmpNode.ParentNode.Name != "Struct")
                            {
                                cNameString += "-";
                                oTmpNode = (XmlElement)oTmpNode.ParentNode;
                            }
                            cNameString += oMenuElmt.SelectSingleNode("cStructName").InnerText;
                            base.addOption(ref oSelElmt1, cNameString, oMenuElmt.SelectSingleNode("nStructKey").InnerText);
                        }
                        XmlElement argoBindParent4 = null;
                        base.addBind("cSection", "cSection", oBindParent: ref argoBindParent4, "true()");
                        // Search sub pages
                        oSelElmt2 = base.addSelect(ref oFrmElmt, "nSearchChildren", true, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt2, "Search all sub-pages", 1.ToString());
                        XmlElement argoBindParent5 = null;
                        base.addBind("nSearchChildren", "nSearchChildren", oBindParent: ref argoBindParent5, "false()");

                        if (cContentType.Contains("Product") & cContentType.Contains("SKU"))
                        {
                            oSelElmt2 = base.addSelect(ref oFrmElmt, "nIncludeRelated", true, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                            base.addOption(ref oSelElmt2, "Include Related Sku's", 1.ToString());
                            XmlElement argoBindParent6 = null;
                            base.addBind("nIncludeRelated", "nIncludeRelated", oBindParent: ref argoBindParent6, "false()");
                        }

                        // search button
                        base.addSubmit(ref oFrmElmt, "Search", "Search", "ewSubmit");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                // Dim nPar As Integer = goRequest.QueryString("GroupId")
                                if (!Tools.Number.IsNumeric(nParId))
                                {
                                    XmlElement oParElmt = (XmlElement)base.Instance.SelectSingleNode(nParId);
                                    if (oParElmt != null)
                                        nParId = oParElmt.InnerText;
                                }
                                int nRoot = Convert.ToInt16(base.Instance.SelectSingleNode("cSection").InnerText);
                                bool bChilds = base.Instance.SelectSingleNode("nSearchChildren").InnerText == "1";
                                string cExpression = base.Instance.SelectSingleNode("cSearch").InnerText;
                                bool bIncRelated = base.Instance.SelectSingleNode("nIncludeRelated").InnerText == "1";

                                string sSQL = "Select " + cSelectField + " From " + cTableName + " WHERE " + cFilterField + " = " + nParId;
                                using (var oDre = moDbHelper.getDataReaderDisposable(sSQL))  // Done by nita on 6/7/22
                                {
                                    string cTmp = "";
                                    while (oDre.Read())
                                        cTmp = cTmp + oDre[0] + ",";

                                    if (!string.IsNullOrEmpty(cTmp))
                                        cTmp = cTmp.Substring(0, cTmp.Length - 1);
                                    oPageDetail.AppendChild(moDbHelper.RelatedContentSearch(nRoot, cContentType, bChilds, cExpression, Convert.ToInt16(nParId), bIgnoreParID ? 0 : Convert.ToInt16(nParId), cTmp.Split(','), bIncRelated));

                                }
                            }
                        }

                        else
                        {
                            base.Instance.InnerXml = "<nParentContentId>" + nParentID + "</nParentContentId>" + "<cSchemaName>" + cContentType + "</cSchemaName>" + "<cSection>0</cSection>" + "<nSearchChildren>1</nSearchChildren>" + "<cParentContentName>" + cParentContentName + "</cParentContentName>" + "<redirect>" + redirect + "</redirect><cSearch/>";
                            base.addValues();
                        }
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmFindRelated", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmProductGroup(int nGroupId, string SchemaName = "Discount")
                {
                    XmlElement oFrmElmt;
                    XmlElement oGrp1Elmt;
                    string cProcessInfo = "";

                    try
                    {

                        base.NewFrm("EditProductGroup");
                        base.Instance.InnerXml = "<tblCartProductCategories><nCatKey/><cCatSchemaName>" + SchemaName + "</cCatSchemaName><cCatForeignRef/><nCatParentId/><cCatName/><cCatDescription/><nAuditId/></tblCartProductCategories>";
                        if (nGroupId > 0)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartProductCategories, (long)nGroupId);
                        }
                        base.submission("EditProductGroup", "", "post", "form_check(this)");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "ProductGroup");
                        base.addNote("pgheader", Protean.xForm.noteTypes.Help, (nGroupId > 0 ? "Edit " : "Add ") + "Product Group");
                        oGrp1Elmt = base.addGroup(ref oFrmElmt, "Details", "1col", "Details");

                        // Definitions
                        base.addInput(ref oGrp1Elmt, "nCatKey", true, "nCatKey", "hidden");
                        XmlElement argoBindParent = null;
                        base.addBind("nCatKey", "tblCartProductCategories/nCatKey", oBindParent: ref argoBindParent);
                        // MyBase.addInput(oGrp1Elmt, "cCatSchemaName", True, "Schema")
                        // MyBase.addBind("cCatSchemaName", "tblCartProductCategories/cCatSchemaName", "true()")
                        // MyBase.addInput(oGrp1Elmt, "cCatForeignRef", True, "Foreign Ref")
                        // MyBase.addBind("cCatForeignRef", "tblCartProductCategories/cCatForeignRef")
                        // MyBase.addInput(oGrp1Elmt, "nCatParentId", True, "nCatParentId", "hidden")
                        // MyBase.addBind("nCatParentId", "tblCartProductCategories/nCatParentId")
                        base.addInput(ref oGrp1Elmt, "cCatName", true, "Name");
                        XmlElement argoBindParent1 = null;
                        base.addBind("cCatName", "tblCartProductCategories/cCatName", oBindParent: ref argoBindParent1, "true()");

                        var oSchemaSelect = base.addSelect1(ref oGrp1Elmt, "cCatSchemaName", true, "Group Type", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSchemaSelect, SchemaName, SchemaName);
                        string[] aOptions = null;
                        if (myWeb.moCart.moCartConfig["ProductCategoryTypes"] != null)
                        {
                            aOptions = myWeb.moCart.moCartConfig["ProductCategoryTypes"].Split(',');
                            if (aOptions.Length > 0)
                            {
                                for (int i = 0, loopTo = aOptions.Length - 1; i <= loopTo; i++)
                                    base.addOption(ref oSchemaSelect, aOptions[i], aOptions[i]);
                            }
                        }
                        XmlElement argoBindParent2 = null;
                        base.addBind("cCatSchemaName", "tblCartProductCategories/cCatSchemaName", oBindParent: ref argoBindParent2);


                        int argnRows = 15;
                        int argnCols = 50;
                        string cClass = "";
                        base.addTextArea(ref oGrp1Elmt, "cCatDescription", true, "Description", ref cClass, nRows: ref argnRows, nCols: ref argnCols);
                        XmlElement argoBindParent3 = null;
                        base.addBind("cCatDescription", "tblCartProductCategories/cCatDescription", oBindParent: ref argoBindParent3);

                        base.addInput(ref oGrp1Elmt, "nAuditId", true, "nAuditId", "hidden");
                        XmlElement argoBindParent4 = null;
                        base.addBind("nAuditId", "tblCartProductCategories/nAuditId", oBindParent: ref argoBindParent4);

                        // search button

                        base.addSubmit(ref oFrmElmt, "EditProductGroup", "Save Product Group", "SaveProductGroup");


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartProductCategories, base.Instance, nGroupId > 0 ? (long)nGroupId : -1L);
                            }
                        }
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmProductGroup", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDiscountRule(int nDiscountId, int nDiscountType = 0)
                {

                    string cProcessInfo = "";

                    string cTypePath;
                    DiscountCategory eDiscountType;
                    string sSQL;
                    try
                    {
                        if (nDiscountType > 0)
                            eDiscountType = (DiscountCategory)nDiscountType;
                        if (nDiscountId > 0)
                        {
                            sSQL = "Select nDiscountCat From tblCartDiscountRules WHERE nDiscountKey = " + nDiscountId;
                            nDiscountType = Convert.ToInt16(moDbHelper.ExeProcessSqlScalar(sSQL));
                        }

                        if (nDiscountType > 0)
                        {
                            eDiscountType = (DiscountCategory)nDiscountType;
                            cTypePath = "DiscountRule_" + eDiscountType.ToString() + ".xml";
                        }
                        else
                        {
                            cTypePath = "DiscountRule.xml";
                        }
                        string DiscountFormPath = "/xforms/discounts/";
                        if (myWeb.bs5)
                        {
                            DiscountFormPath = "/features/cart/discounts/";
                        }
                        base.NewFrm("EditDiscountRules");
                        if (!base.load(DiscountFormPath + cTypePath, myWeb.maCommonFolders))
                        {
                            // not allot we can do really except try defaults
                            if (!base.load(DiscountFormPath + "DiscountRule.xml", myWeb.maCommonFolders))
                            {
                                // not allot we can do really 
                            }
                        }

                        var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");


                        if (nDiscountId > 0)
                        {
                            existingInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartDiscountRules, (long)nDiscountId);
                            LoadInstanceFromInnerXml(existingInstance.InnerXml);
                        }
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartDiscountRules, base.Instance, nDiscountId > 0 ? (long)nDiscountId : -1L);
                            }
                        }
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmDiscountRule", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDiscountProductRelations(long id, string dname)
                {

                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    string sSql;

                    string cProcessInfo = "";

                    try
                    {

                        base.NewFrm("EditDiscountProductRelations");

                        base.submission("EditDiscountRelations", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditRelations", "3col", "Product Group Relations for Discount");

                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the product groups you want to have access to this discount");
                        //XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref oFrmGrp1, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");
                        //oFrmGrp1 = (XmlElement)argoNode;

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditRelations", "RelationButtons", "Buttons");
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Add Selected", "", "PermissionButton btn-add");
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "Remove Selected", "", "PermissionButton btn-remove");

                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    if (!string.IsNullOrWhiteSpace(goRequest["Groups"]))
                                    {
                                        moDbHelper.saveDiscountProdGroupRelation((int)id, goRequest["Groups"]);
                                    }
                                    break;
                                }
                            case "RemoveSelected":
                                {
                                    if (!string.IsNullOrWhiteSpace(goRequest["Items"]))
                                    {
                                        moDbHelper.saveDiscountProdGroupRelation((int)id, goRequest["Items"], false);
                                    }
                                    break;
                                }
                        }

                        oElmt2 = base.addSelect(ref oFrmGrp1, "Groups", false, "Product Groups", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        sSql = "SELECT nCatKey AS value, cCatName AS name" + " FROM tblCartProductCategories" + " WHERE (cCatSchemaName = N'Discount') AND" + " (((SELECT nDiscountProdCatRelationKey" + " FROM tblCartDiscountProdCatRelations" + " WHERE (nProductCatId = tblCartProductCategories.nCatKey) AND (nDiscountId = " + id + "))) IS NULL)" + " ORDER BY cCatName";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt2, oDr, "name", "value");
                        }



                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "RelatedObjects", "", "All items with permissions to access page");
                        //XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref oFrmGrp3, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        //oFrmGrp3 = (XmlElement)argoNode1;

                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Related", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                        sSql = "SELECT tblCartDiscountProdCatRelations.nDiscountProdCatRelationKey as value, tblCartProductCategories.cCatName as name" + " FROM tblCartDiscountProdCatRelations INNER JOIN tblCartProductCategories ON tblCartDiscountProdCatRelations.nProductCatId = tblCartProductCategories.nCatKey" + " WHERE (tblCartDiscountProdCatRelations.nDiscountId = " + id + ") ORDER BY tblCartProductCategories.cCatName";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt4, oDr, "name", "value");
                        }

                        base.Instance.InnerXml = "<relations/>";

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmDiscountDirRelations(long id, string dname)
                {

                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    XmlElement oElmt5;
                    string sSql;

                    string cProcessInfo = "";
                    var bDeny = default(bool);
                    string cDenyFilter = "";
                    try
                    {

                        base.NewFrm("EditDiscountProductDirs");

                        if (moDbHelper.checkTableColumnExists("tblCartDiscountDirRelations", "nPermLevel"))
                        {
                            bDeny = true;
                            cDenyFilter = " and nPermLevel <> 0";
                        }

                        base.submission("EditDiscountDirs", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditDirs", "3col", "User Group Relations for Discount " + dname);

                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the user groups you want to have access to this discount");
                        //XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref oFrmGrp1, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");
                        //oFrmGrp1 = (XmlElement)argoNode;

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditDirs", "DirButtons", "Buttons");
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Add Selected", "", "PermissionButton btn-add");
                        if (bDeny)
                        {
                            base.addSubmit(ref oFrmGrp2, "DenySelected", "Deny Selected", "", "PermissionButton btn-deny");
                        }
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "Remove Selected", "", "PermissionButton btn-remove");

                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    moDbHelper.saveDiscountDirRelation((int)id, goRequest["Groups"]);
                                    break;
                                }
                            case "DenySelected":
                                {
                                    if (bDeny)
                                    {
                                        moDbHelper.saveDiscountDirRelation((int)id, goRequest["Groups"], true, Cms.dbHelper.PermissionLevel.Denied);
                                    }

                                    break;
                                }
                            case "RemoveSelected":
                                {
                                    moDbHelper.saveDiscountDirRelation((int)id, goRequest["Items"], false);
                                    break;
                                }
                        }

                        oElmt2 = base.addSelect(ref oFrmGrp1, "Groups", false, "User Groups", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        // Dim nxxx As Integer = moDbhelper.exeProcessSQLScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " & id & ") AND (nDirId = 0)")
                        if (!(Convert.ToDouble(moDbHelper.ExeProcessSqlScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " + id + ") AND (nDirId = 0)")) > 0d))
                        {
                            base.addOption(ref oElmt2, "<<All Users>>", 0.ToString());
                        }
                        sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Group') AND" + " (((SELECT nDiscountDirRelationKey" + " FROM tblCartDiscountDirRelations" + " WHERE (nDiscountId = " + id + ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" + "ORDER BY cDirName";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {

                            base.addOptionsFromSqlDataReader(oElmt2, oDr, "name", "value");
                        }


                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "RelatedObjects", "", "All items with permissions to access page");
                        //XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref oFrmGrp3, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        //oFrmGrp3 = (XmlElement)argoNode1;

                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Related", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                        sSql = "SELECT tblCartDiscountDirRelations.nDiscountDirRelationKey AS value, " + " CASE WHEN tblCartDiscountDirRelations.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" + " FROM tblCartDiscountDirRelations LEFT OUTER JOIN" + "  tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey" + " WHERE (tblCartDiscountDirRelations.ndiscountid = " + id + ")" + cDenyFilter + " ORDER BY cDirName";
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt4, oDr, "name", "value");
                        }


                        if (bDeny)
                        {

                            oElmt5 = base.addSelect(ref oFrmGrp3, "Items", false, "Denied", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                            sSql = "SELECT tblCartDiscountDirRelations.nDiscountDirRelationKey AS value, " + " CASE WHEN tblCartDiscountDirRelations.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" + " FROM tblCartDiscountDirRelations LEFT OUTER JOIN" + "  tblDirectory ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey" + " WHERE (tblCartDiscountDirRelations.ndiscountid = " + id + ") and nPermLevel = 0 ORDER BY cDirName";
                            using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                            {
                                base.addOptionsFromSqlDataReader(oElmt5, oDr, "name", "value");
                            }

                        }

                        base.Instance.InnerXml = "<Dirs/>";

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmDiscountDirRelations", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmShippingDirRelations(long id, string dname)
                {

                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    XmlElement oElmt5;
                    string sSql;
                    var bDeny = default(bool);
                    string cProcessInfo = "";
                    string cDenyFilter = "";
                    try
                    {

                        base.NewFrm("EditShippingDirRelations");

                        if (moDbHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel"))
                        {
                            bDeny = true;
                            cDenyFilter = " and nPermLevel <> 0";
                        }


                        base.submission("EditInputPageRights", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditDirs", "3col", "User Group Relations for Shipping Method " + dname);

                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the user groups you want to have access to this shipping method");
                        //XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref oFrmGrp1, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");
                        //oFrmGrp1 = (XmlElement)argoNode;

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditDirs", "DirButtons", "Buttons");
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Allow Selected", "", "PermissionButton btn-allow");
                        if (bDeny)
                        {
                            base.addSubmit(ref oFrmGrp2, "DenySelected", "Deny Selected", "", "PermissionButton btn-deny");
                        }
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "Remove Selected", "", "PermissionButton btn-remove");

                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    if (!string.IsNullOrEmpty(goRequest["Groups"]))
                                    {
                                        moDbHelper.saveShippingDirRelation((int)id, goRequest["Groups"]);
                                    }
                                    if (!string.IsNullOrEmpty(goRequest["Roles"]))
                                    {
                                        moDbHelper.saveShippingDirRelation((int)id, goRequest["Roles"]);
                                    }

                                    break;
                                }
                            case "DenySelected":
                                {
                                    if (bDeny)
                                    {
                                        if (!string.IsNullOrEmpty(goRequest["Groups"]))
                                        {
                                            moDbHelper.saveShippingDirRelation((int)id, goRequest["Groups"], true, Cms.dbHelper.PermissionLevel.Denied);
                                        }
                                        if (!string.IsNullOrEmpty(goRequest["Roles"]))
                                        {
                                            moDbHelper.saveShippingDirRelation((int)id, goRequest["Roles"], true, Cms.dbHelper.PermissionLevel.Denied);
                                        }
                                    }

                                    break;
                                }
                            case "RemoveSelected":
                                {
                                    moDbHelper.saveShippingDirRelation((int)id, goRequest["Items"], false);
                                    break;
                                }
                        }

                        oElmt2 = base.addSelect(ref oFrmGrp1, "Groups", false, "User Groups", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        // Dim nxxx As Integer = moDbhelper.exeProcessSQLScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " & id & ") AND (nDirId = 0)")
                        if (!(Convert.ToDouble(moDbHelper.ExeProcessSqlScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " + id + ") AND (nDirId = 0)")) > 0d))
                        {
                            base.addOption(ref oElmt2, "<<All Users>>", 0.ToString());
                        }
                        sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Group') AND" + " (((SELECT nCartShippingPermissionKey" + " FROM tblCartShippingPermission" + " WHERE (nShippingMethodId = " + id + ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" + "ORDER BY cDirName";

                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt2, oDr, "name", "value");
                        }
                        oElmt2 = base.addSelect(ref oFrmGrp1, "Roles", false, "User Roles", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                        sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Role') AND" + " (((SELECT nCartShippingPermissionKey" + " FROM tblCartShippingPermission" + " WHERE (nShippingMethodId = " + id + ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" + "ORDER BY cDirName";

                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt2, oDr, "name", "value");
                        }


                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "RelatedObjects", "", "All Groups with permissions for Shipping Method");
                        //XmlNode argoNode1 = oFrmGrp3;
                        base.addNote(ref oFrmGrp3, Protean.xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above");
                        //oFrmGrp3 = (XmlElement)argoNode1;

                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Allowed", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);


                        sSql = "SELECT tblCartShippingPermission.nCartShippingPermissionKey AS value, " + " CASE WHEN tblCartShippingPermission.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" + " FROM tblCartShippingPermission LEFT OUTER JOIN" + " tblDirectory ON tblCartShippingPermission.nDirId = tblDirectory.nDirKey" + " WHERE (tblCartShippingPermission.nShippingMethodId = " + id + ")" + cDenyFilter + " ORDER BY cDirName";

                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {

                            base.addOptionsFromSqlDataReader(oElmt4, oDr, "name", "value");
                        }
                        if (bDeny)
                        {

                            oElmt5 = base.addSelect(ref oFrmGrp3, "Items", false, "Denied", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                            sSql = "SELECT tblCartShippingPermission.nCartShippingPermissionKey AS value, " + " CASE WHEN tblCartShippingPermission.nDirid = 0 THEN '<<All Users>>' ELSE tblDirectory.cDirName END AS name" + " FROM tblCartShippingPermission LEFT OUTER JOIN" + "  tblDirectory ON tblCartShippingPermission.nDirId = tblDirectory.nDirKey" + " WHERE (tblCartShippingPermission.nShippingMethodId = " + id + ") and nPermLevel = 0 ORDER BY cDirName";
                            using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                            {
                                base.addOptionsFromSqlDataReader(oElmt5, oDr, "name", "value");
                            }
                        }

                        base.Instance.InnerXml = "<Dirs/>";

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmShippingDirRelations", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                // New method added for new form for shipping group for product discounts.
                public XmlElement xFrmProductShippingGroupRelations(long id, string dname)
                {

                    XmlElement oFrmElmt;
                    XmlElement oFrmGrp1;
                    XmlElement oFrmGrp2;
                    XmlElement oFrmGrp3;
                    XmlElement oElmt2;
                    XmlElement oElmt4;
                    XmlElement oElmt5;
                    string sSql;
                    var bDeny = default(bool);
                    string cProcessInfo = "";
                    string cDenyFilter = "";
                    try
                    {

                        base.NewFrm("EditShippingDirRelations");

                        if (moDbHelper.checkTableColumnExists("tblCartShippingProductCategoryRelations", "nRuleType"))
                        {
                            bDeny = true;
                            cDenyFilter = " and nRuleType <> 2";
                        }


                        base.submission("EditInputPageRights", "", "post");

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "EditDirs", "3col", "Shipping Group Relations for Shipping Method " + dname);

                        oFrmGrp1 = base.addGroup(ref oFrmElmt, "AllObjects", "", "Select the shipping groups you want to have access to this shipping method");
                        //XmlNode argoNode = oFrmGrp1;
                        base.addNote(ref oFrmGrp1, Protean.xForm.noteTypes.Hint, "You can select multiple items by holding down CRTL whilse clicking the names");
                        //oFrmGrp1 = (XmlElement)argoNode;

                        // add the buttons so we can test for submission
                        oFrmGrp2 = base.addGroup(ref oFrmElmt, "EditDirs", "DirButtons", "Buttons ");
                        base.addSubmit(ref oFrmGrp2, "AddSelected", "Include Selected", "", "PermissionButton btn-allow");
                        if (bDeny)
                        {
                            base.addSubmit(ref oFrmGrp2, "DenySelected", "Exclude Selected", "", "PermissionButton btn-deny");
                        }
                        base.addSubmit(ref oFrmGrp2, "RemoveSelected", "Remove Selected", "", "PermissionButton btn-remove");

                        switch (base.getSubmitted() ?? "")
                        {
                            case "AddSelected":
                                {
                                    if (!string.IsNullOrEmpty(goRequest["Groups"]))
                                    {
                                        moDbHelper.saveProductShippingGroupDirRelation((int)id, goRequest["Groups"]);
                                    }

                                    break;
                                }

                            case "DenySelected":
                                {
                                    if (bDeny)
                                    {
                                        if (!string.IsNullOrEmpty(goRequest["Groups"]))
                                        {
                                            moDbHelper.saveProductShippingGroupDirRelation((int)id, goRequest["Groups"], true, Cms.dbHelper.PermissionLevel.Denied);
                                        }

                                    }

                                    break;
                                }
                            case "RemoveSelected":
                                {
                                    moDbHelper.saveProductShippingGroupDirRelation((int)id, goRequest["Items"], false);
                                    break;
                                }
                        }

                        oElmt2 = base.addSelect(ref oFrmGrp1, "Groups", false, "Shipping Groups", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);
                        // Dim nxxx As Integer = moDbhelper.exeProcessSQLScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " & id & ") AND (nDirId = 0)")
                        if (!(Convert.ToDouble(moDbHelper.ExeProcessSqlScalar("Select nDiscountDirRelationKey From tblCartDiscountDirRelations WHERE (nDiscountId = " + id + ") AND (nDirId = 0)")) > 0d))
                        {
                            base.addOption(ref oElmt2, "<<All Products>>", 0.ToString());
                        }
                        sSql = "SELECT nCatKey as value, cCatName as name FROM tblCartProductCategories WHERE (cCatSchemaName = N'Shipping') AND" + " (((SELECT nShipProdCatRelKey" + " FROM tblCartShippingProductCategoryRelations" + " WHERE (nShipOptId  = " + id + ") AND (nCatId = tblCartProductCategories.nCatKey))) IS NULL)" + " ORDER BY cCatName";

                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt2, oDr, "name", "value");
                        }
                        // oElmt2 = MyBase.addSelect(oFrmGrp1, "Roles", False, "User Roles", "scroll_10", xForm.ApperanceTypes.Minimal)

                        // sSql = "SELECT nDirKey as value, cDirName as name FROM tblDirectory WHERE (cDirSchema = N'Role') AND" &
                        // " (((SELECT nCartShippingPermissionKey" &
                        // " FROM tblCartShippingPermission" &
                        // " WHERE (nShippingMethodId = " & id & ") AND (nDirId = tblDirectory.nDirKey))) IS NULL)" &
                        // "ORDER BY cDirName"

                        // Using oDr As SqlDataReader = moDbHelper.getDataReaderDisposable(sSql) 'done by sonali at 12/7/22
                        // MyBase.addOptionsFromSqlDataReader(oElmt2, oDr, "name", "value")
                        // End Using


                        oFrmGrp3 = base.addGroup(ref oFrmElmt, "RelatedObjects", "", "All Groups with include/exclude for Shipping Method");
                        // MyBase.addNote(oFrmGrp3, xForm.noteTypes.Hint, "Please note: Permissions can also be inherited from pages above")

                        oElmt4 = base.addSelect(ref oFrmGrp3, "Items", false, "Included", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);


                        sSql = "SELECT tblCartShippingProductCategoryRelations.nShipProdCatRelKey AS value, " + "  CASE WHEN tblCartShippingProductCategoryRelations.nCatId = 0 THEN '<<All Products>>' ELSE tblCartProductCategories.cCatName END AS name" + " FROM tblCartShippingProductCategoryRelations LEFT OUTER JOIN" + " tblCartProductCategories ON tblCartShippingProductCategoryRelations.nCatId = tblCartProductCategories.nCatKey" + " WHERE (tblCartShippingProductCategoryRelations.nShipOptId = " + id + ")" + cDenyFilter + " ORDER BY cCatName";

                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                        {
                            base.addOptionsFromSqlDataReader(oElmt4, oDr, "name", "value");
                        }
                        if (bDeny)
                        {

                            oElmt5 = base.addSelect(ref oFrmGrp3, "Items", false, "Excluded", "scroll_10", Protean.xForm.ApperanceTypes.Minimal);

                            sSql = "SELECT tblCartShippingProductCategoryRelations.nShipProdCatRelKey AS value, " + "  CASE WHEN tblCartShippingProductCategoryRelations.nCatId = 0 THEN '<<All Products>>' ELSE tblCartProductCategories.cCatName END AS name" + " FROM tblCartShippingProductCategoryRelations LEFT OUTER JOIN" + " tblCartProductCategories ON tblCartShippingProductCategoryRelations.nCatId = tblCartProductCategories.nCatKey" + " WHERE (tblCartShippingProductCategoryRelations.nShipOptId = " + id + ") and nRuleType = 2 ORDER BY cCatName";
                            using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // done by sonali at 12/7/22
                            {
                                base.addOptionsFromSqlDataReader(oElmt5, oDr, "name", "value");
                            }

                        }

                        base.Instance.InnerXml = "<Dirs/>";

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmProductShippingGroupRelations", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                /// <summary>
                /// This adds or edits a codeset, or the groups associated with the code set
                /// </summary>
                /// <param name="nCodesetKey">The code set ID</param>
                /// <param name="cFormName">The type of form we're dealing with - Codes or CodeGroups</param>
                /// <returns></returns>
                /// <remarks></remarks>
                public XmlElement xFrmMemberCodeset(int nCodesetKey, string cFormName = "Codes")
                {
                    try
                    {

                        XmlElement oElmt = null;
                        string cCodeGroups = "";
                        string cSQL = "";

                        // Build the form
                        base.NewFrm("MemberCodes");
                        string formPath = "/xforms/directory/" + cFormName + ".xml";
                        if (myWeb.bs5)
                        {
                            formPath = "/admin/xforms/directory/" + cFormName + ".xml";
                        }
                        base.load(formPath, myWeb.maCommonFolders);

                        // Load the instance.
                        if (nCodesetKey > 0)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Codes, (long)nCodesetKey);
                        }

                        // Pre-population
                        // ==============

                        // Code Type
                        base.Instance.SelectSingleNode("tblCodes/nCodeType").InnerText = ((int)Cms.dbHelper.CodeType.Membership).ToString();

                        // Groups
                        if (Xml.NodeState(ref base.moXformElmt, "//select[@bind='cCodeGroups']") != XmlNodeState.NotInstantiated)
                        {
                            oElmt = (XmlElement)base.moXformElmt.SelectSingleNode("//select[@bind='cCodeGroups']");
                            cSQL = "SELECT cDirName + ' [' + cDirSchema + ': ' + CAST(nDirKey As nvarchar) + ']' AS DirName, nDirKey FROM tblDirectory ";
                            cSQL += " WHERE NOT(cDirSchema IN ('Role','User'))";
                            cSQL += " ORDER BY cDirSchema, cDirName";
                            using (var oDR = moDbHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                            {
                                base.addOptionsFromSqlDataReader(oElmt, oDR, "DirName", "nDirKey");
                            }
                        }

                        // Handle Submission
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();

                            if (base.valid)
                            {
                                // Create a unique ID for new code sets
                                if (nCodesetKey == 0)
                                {
                                    base.Instance.SelectSingleNode("tblCodes/cCode").InnerText = Guid.NewGuid().ToString();
                                }

                                // Save the code set
                                myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Codes, base.Instance, (long)nCodesetKey);

                                // Update any sub-codes
                                if (nCodesetKey > 0)
                                {
                                    cSQL = "UPDATE tblAudit SET dPublishDate = " + Database.SqlDate(base.Instance.SelectSingleNode("tblCodes/dPublishDate").InnerText, true);
                                    cSQL += ", dExpireDate  = " + Database.SqlDate(base.Instance.SelectSingleNode("tblCodes/dExpireDate").InnerText, true);
                                    cSQL += ", nStatus  = " + base.Instance.SelectSingleNode("tblCodes/nStatus").InnerText;
                                    cSQL += " FROM tblAudit a INNER JOIN tblCodes c ON a.nauditkey = c.nauditid AND (c.nCodeParentId = " + nCodesetKey + " OR c.nCodeKey = " + nCodesetKey + ")";
                                    myWeb.moDbHelper.ExeProcessSql(cSQL);
                                }

                            }
                        }

                        // Manually populate the groups in the dropdown
                        XmlNodeState localNodeState() { var argoNode4 = base.Instance; var ret = Xml.NodeState(ref argoNode4, "tblCodes/cCodeGroups", "", "", XmlNodeState.IsEmpty, null, "", cCodeGroups, bCheckTrimmedInnerText: false); base.Instance = argoNode4; return ret; }

                        if (localNodeState() == XmlNodeState.HasContents)
                        {
                            string[] oGroups = cCodeGroups.Split(',');
                            for (int i = 0, loopTo = oGroups.Length - 1; i <= loopTo; i++)
                            {
                                if (Xml.NodeState(ref base.moXformElmt, "descendant-or-self::*[@bind='cCodeGroups']/item[value='" + oGroups[i] + "']") != XmlNodeState.NotInstantiated)
                                {
                                    oElmt = (XmlElement)base.moXformElmt.SelectSingleNode("descendant-or-self::*[@bind='cCodeGroups']/item[value='" + oGroups[i] + "']");
                                    oElmt.SetAttribute("selected", "selected");
                                }
                            }
                        }

                        // Tidy Up
                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmMemberCodeset", ex, "", "", gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmDeleteMemberCodeset(long nCodesetKey)
                {

                    XmlElement oFrmElmt;
                    XmlElement oElmt;

                    // Dim oDr As SqlDataReader
                    string cProcessInfo = "";


                    try
                    {
                        // load the directory item to be deleted
                        moDbHelper.moPageXml = moPageXML;

                        base.NewFrm("DeleteMemberCodeset");

                        // Lets get the object
                        if (nCodesetKey != 0L)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Codes, (long)nCodesetKey);
                        }
                        base.submission("EditInput", "", "post");
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "DeleteDM", "", "Delete MemberCodeset");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this MemberCodeset: " + base.Instance.SelectSingleNode("cCodeName"));
                        //oFrmElmt = (XmlElement)argoNode;


                        base.addSubmit(ref oFrmElmt, "", "Delete Codeset");

                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Codes, nCodesetKey);
                            }
                            else
                            {
                                base.addValues();
                            }
                        }
                        else
                        {
                            base.addValues();
                        }

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmDeleteDeliveryMethod", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                /// <summary>
                ///   Xform for generating codes for codes sets.
                /// </summary>
                /// <param name="nParentCodeKey">The parent code set id</param>
                /// <param name="cFormName">Optional, probably not needed - the type of form we're dealing with - CodeGenerator</param>
                /// <returns></returns>
                /// <remarks></remarks>
                public XmlElement xFrmMemberCodeGenerator(int nParentCodeKey, string cFormName = "CodeGenerator")
                {
                    XmlElement oElmt = null;
                    XmlElement oParentInstance = null;
                    XmlElement oInstanceRoot = null;
                    //string cCodeGroups = "";
                    //string cCodeXForm = "";

                    try
                    {
                        // Build the form
                        base.NewFrm("MemberCodes");
                        string formPath = "/xforms/directory/";
                        if (myWeb.bs5)
                            formPath = "/admin/xforms/directory/";
                        base.load(formPath + cFormName + ".xml", myWeb.maCommonFolders);

                        base.Instance.SelectSingleNode("tblCodes/nCodeType").InnerText = ((int)Cms.dbHelper.CodeType.Membership).ToString();

                        // Get the parent code instance
                        oParentInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                        oParentInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Codes, (long)nParentCodeKey);

                        // Update the form with relevant values from the parent
                        foreach (XmlElement currentOElmt in oParentInstance.SelectNodes("tblCodes/dPublishDate|tblCodes/dExpireDate|tblCodes/nStatus"))
                        {
                            oElmt = currentOElmt;
                            // Populate the local instance with parent code information
                            if (base.Instance.SelectSingleNode("tblCodes/" + oElmt.Name) != null)
                            {
                                base.Instance.SelectSingleNode("tblCodes/" + oElmt.Name).InnerText = oElmt.InnerText;
                            }
                        }

                        // Add the parent code id
                        base.Instance.SelectSingleNode("tblCodes/nCodeParentId").InnerText = nParentCodeKey.ToString();

                        // Update the label
                        if (Xml.NodeState(ref base.moXformElmt, "group/label", "", "", XmlNodeState.IsEmpty, oElmt, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                        {
                            oElmt.InnerText += " for " + oParentInstance.SelectSingleNode("tblCodes/cCodeName").InnerText;
                        }


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();

                            if (base.valid)
                            {
                                // Generate the Codes
                                oInstanceRoot = (XmlElement)base.Instance.SelectSingleNode("tblCodes");

                                int nNoCodes = Convert.ToInt16(oInstanceRoot.SelectSingleNode("nNumberOfCodes").InnerText);

                                var oCodes = new string[nNoCodes];

                                object localgetNodeValueByType6() { XmlNode argoParent = oInstanceRoot; var ret = getNodeValueByType(ref argoParent, "bRND", vDefaultValue: "0"); oInstanceRoot = (XmlElement)argoParent; return ret; }

                                if (localgetNodeValueByType6()?.ToString() == "0")
                                {

                                    // Generate non-random codes
                                    oCodes = CodeGen(oInstanceRoot.SelectSingleNode("cPreceedingText").InnerText, Convert.ToInt16(oInstanceRoot.SelectSingleNode("nStartNumber").InnerText), nNoCodes, XmlConvert.ToBoolean(oInstanceRoot.SelectSingleNode("bKeepProceedingZeros").InnerText), XmlConvert.ToBoolean(oInstanceRoot.SelectSingleNode("bMD5Results").InnerText));
                                }
                                else
                                {

                                    // Generate random codes
                                    var random = new Number.Random();

                                    // Set the options

                                    // Option: Case
                                    var options = TextOptions.UpperCase;

                                    // Option: Unambiguous Letters
                                    object localgetNodeValueByType() { XmlNode argoParent1 = oInstanceRoot; var ret = getNodeValueByType(ref argoParent1, "bRNDVague", vDefaultValue: "0"); oInstanceRoot = (XmlElement)argoParent1; return ret; }

                                    if (localgetNodeValueByType()?.ToString() == "1")
                                        options = options | TextOptions.UnambiguousCharacters;

                                    // Option: Character Sets
                                    object localgetNodeValueByType1() { XmlNode argoParent2 = oInstanceRoot; var ret = getNodeValueByType(ref argoParent2, "cRNDAlpha", vDefaultValue: "Letters,Numbers"); oInstanceRoot = (XmlElement)argoParent2; return ret; }

                                    string cCharDefs = localgetNodeValueByType1().ToString();
                                    if (cCharDefs.Contains("Letters"))
                                        options = options | TextOptions.UseAlpha;
                                    if (cCharDefs.Contains("Numbers"))
                                        options = options | TextOptions.UseNumeric;
                                    if (cCharDefs.Contains("Symbols"))
                                        options = options | TextOptions.UseSymbols;

                                    // Generate the codes
                                    for (int i = 0, loopTo = nNoCodes - 1; i <= loopTo; i++)
                                    {
                                        // Generate a random password
                                        // object localgetNodeValueByType2() { XmlNode argoParent3 = oInstanceRoot; var ret = getNodeValueByType(ref argoParent3, "nRNDLength", vDefaultValue: "8"); oInstanceRoot = (XmlElement)argoParent3; return ret; }

                                        object localgetNodeValueByType3() { XmlNode argoParent4 = oInstanceRoot; var ret = getNodeValueByType(ref argoParent4, "nRNDLength", vDefaultValue: "8"); oInstanceRoot = (XmlElement)argoParent4; return ret; }

                                        string cC = RandomPassword(Convert.ToInt16(localgetNodeValueByType3()), options: options, oRandomObject: random);

                                        // Check for duplicates
                                        while (!(Array.LastIndexOf(oCodes, cC) == -1 | string.IsNullOrEmpty(cC)))
                                        {
                                            //   object localgetNodeValueByType4() { XmlNode argoParent5 = oInstanceRoot; var ret = getNodeValueByType(ref argoParent5, "nRNDLength", vDefaultValue: "8"); oInstanceRoot = (XmlElement)argoParent5; return ret; }

                                            object localgetNodeValueByType5() { XmlNode argoParent6 = oInstanceRoot; var ret = getNodeValueByType(ref argoParent6, "nRNDLength", vDefaultValue: "8"); oInstanceRoot = (XmlElement)argoParent6; return ret; }

                                            cC = RandomPassword(Convert.ToInt16(localgetNodeValueByType5()), options: options);
                                        }

                                        oCodes[i] = cC;
                                    }

                                }

                                // Add the codes to the database
                                int nAdded = 0;
                                int nSkipped = 0;
                                for (int i = 0, loopTo1 = oCodes.Length - 1; i <= loopTo1; i++)
                                {
                                    if (!string.IsNullOrEmpty(oCodes[i]))
                                    {
                                        if (Convert.ToInt32(myWeb.moDbHelper.GetDataValue("SELECT nCodeKey FROM tblCodes WHERE cCode ='" + oCodes[i] + "'")) > 0)
                                        {
                                            nSkipped += 1;
                                        }
                                        else
                                        {
                                            base.Instance.SelectSingleNode("tblCodes/cCode").InnerText = oCodes[i];
                                            int nSubId = Convert.ToInt16(myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Codes, base.Instance));
                                            nAdded += 1;
                                        }
                                    }
                                }
                                var argoNode = base.moXformElmt.SelectSingleNode("group");
                                base.addNote(ref argoNode, Protean.xForm.noteTypes.Help, nAdded + " Codes Added, " + nSkipped + " Codes Skipped (Duplicates)", true);
                            }

                        }


                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmMemberCodeGenerator", ex, "", "", gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmImportCodes(int nParentCodeKey, string cFormName = "CodeGenerator")
                {
                    XmlElement oElmt = null;
                    XmlElement oParentInstance = null;
                    XmlElement oInstanceRoot = null;
                    XmlElement oFrmElmt;
                    //string cCodeGroups = "";
                    //string cCodeXForm = "";

                    try
                    {

                        //Get the Group name

                        string cCodeGroup = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Codes, (long)nParentCodeKey);
                        XmlDocument oCodeGroup = new XmlDocument();
                        oCodeGroup.LoadXml(cCodeGroup);

                        string GroupName = oCodeGroup.SelectSingleNode("tblCodes/cCodeName").InnerText;


                        // Build the form
                        base.NewFrm("ImportCodes");
                        base.submission("Import Codes", "", "post", "form_check(this)");
                        base.Instance.InnerXml = "<ImportCodes/>";
                        oFrmElmt = base.addGroup(ref base.moXformElmt, "Import Comma Separated Codes for " + GroupName, "", "Please copy and paste the codes below separated by commas");
                        Int16 rows = 20;
                        Int16 cols = 80;
                        string ClassName = "";
                        base.addTextArea(ref oFrmElmt, "ImportCodes", true, "Import Comma Separated Codes for " + GroupName, ClassName, rows, cols);

                        XmlElement argoBindParent1 = null;
                        base.addBind("ImportCodes", "ImportCodes", oBindParent: ref argoBindParent1, "true()");

                        base.addSubmit(ref oFrmElmt, "", "Import", "ewSubmit");

                        // Add the parent code id
                        XmlElement ImportCodes = (XmlElement)base.Instance.SelectSingleNode("ImportCodes");

                        ImportCodes.SetAttribute("groupId", nParentCodeKey.ToString());

                        // Update the label
                        if (Xml.NodeState(ref base.moXformElmt, "group/label", "", "", XmlNodeState.IsEmpty, oElmt, returnAsXml: "", returnAsText: "", bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                        {
                            //    oElmt.InnerText += " for " + oParentInstance.SelectSingleNode("tblCodes/cCodeName").InnerText;
                        }


                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.validate();

                            if (base.valid)
                            {
                                // Generate the Codes
                                oInstanceRoot = (XmlElement)base.Instance.SelectSingleNode("ImportCodes");



                                string rawText = oInstanceRoot.InnerText;
                                string[] oCodes = rawText.Contains(',')
                                    ? rawText.Split(',')
                                    : rawText.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
                                int nNoCodes = oCodes.Count();
                                // Add the codes to the database
                                int nAdded = 0;
                                int nSkipped = 0;
                                for (int i = 0, loopTo1 = oCodes.Length - 1; i <= loopTo1; i++)
                                {
                                    if (!string.IsNullOrEmpty(oCodes[i]))
                                    {
                                        if (Convert.ToInt32(myWeb.moDbHelper.GetDataValue("SELECT nCodeKey FROM tblCodes WHERE cCode ='" + oCodes[i] + "'")) > 0)
                                        {
                                            nSkipped += 1;
                                        }
                                        else
                                        {
                                            string codeInstance = "<tblCodes>\r\n\t\t\t\t<nCodeKey />\r\n\t\t\t\t<cCodeName />\r\n\t\t\t\t<nCodeType>1</nCodeType>\r\n\t\t\t\t<nCodeParentId />\r\n\t\t\t\t<cCodeGroups />\r\n\t\t\t\t<cCode />\r\n\t\t\t\t<nUseId />\r\n\t\t\t\t<dUseDate />\r\n\t\t\t\t</tblCodes>";
                                            XmlElement newInstance = base.moPageXML.CreateElement("instance");
                                            newInstance.InnerXml = codeInstance;

                                            newInstance.SelectSingleNode("tblCodes/cCode").InnerText = oCodes[i];
                                            newInstance.SelectSingleNode("tblCodes/nCodeParentId").InnerText = oInstanceRoot.GetAttribute("groupId");

                                            int nSubId = Convert.ToInt16(myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Codes, newInstance));
                                            nAdded += 1;
                                        }
                                    }
                                }
                                var argoNode = base.moXformElmt.SelectSingleNode("group");
                                base.addNote(ref argoNode, Protean.xForm.noteTypes.Help, nAdded + " Codes Added, " + nSkipped + " Codes Skipped (Duplicates)", true);
                            }

                        }


                        base.addValues();
                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmMemberCodeGenerator", ex, "", "", gbDebug);
                        return null;
                    }
                }
                public XmlElement xFrmVoucherCode(int nCodeId)
                {

                    string cProcessInfo = "";
                    string cTypePath = "";
                    try
                    {


                        base.NewFrm("EditVoucherCode");
                        if (!base.load("/xforms/codes/" + cTypePath, myWeb.maCommonFolders))
                        {
                            // not allot we can do really except try defaults
                            if (!base.load("/xforms/code/Voucher.xml", myWeb.maCommonFolders))
                            {
                                // not allot we can do really 
                            }
                        }

                        if (nCodeId > 0)
                        {
                            base.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Codes, (long)nCodeId);
                        }
                        if (base.isSubmitted())
                        {
                            base.updateInstanceFromRequest();
                            base.addValues();
                            base.validate();
                            if (base.valid)
                            {
                                moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Codes, base.Instance, nCodeId > 0 ? (long)nCodeId : -1L);
                            }
                        }
                        base.addValues();
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmVoucherCode", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

            }
        }
    }
}