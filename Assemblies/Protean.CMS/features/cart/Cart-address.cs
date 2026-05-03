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


            public virtual void addressSubProcess(ref XmlElement oCartElmt, string cAddressType)
            {

                Cms.xForm oContactXform;
                string submitPrefix = "cartBill";
                string cProcessInfo = submitPrefix;
                PaymentProviders oPay;
                string buttonRef = "";
                bool bSubmitPaymentMethod = false;

                try
                {
                    myWeb.moSession["tempInstance"] = null;


                    if (cAddressType.Contains("Delivery"))
                        submitPrefix = "cartDel";
                    if (mbEwMembership == true & myWeb.mnUserId != 0 & submitPrefix != "cartDel")
                    {
                        // we now only need this on delivery.
                        oContactXform = pickContactXform(cAddressType, submitPrefix, cCmdAction: mcCartCmd);
                        GetCart(ref oCartElmt);
                    }
                    else
                    {
                        oContactXform = contactXform(cAddressType, "submit", "cartCmd", mcCartCmd);

                        if (moPay is null)
                        {
                            oPay = new PaymentProviders(ref myWeb);
                        }
                        else
                        {
                            oPay = moPay;
                        }
                        oPay.mcCurrency = mcCurrency;

                        GetCart(ref oCartElmt);
                        if (moCartConfig["PaymentTypeButtons"].ToLowerInvariant() == "on")
                        {
                            if (oCartElmt.SelectSingleNode("Shipping") != null & string.IsNullOrEmpty(moCartConfig["TermsContentId"]) & string.IsNullOrEmpty(moCartConfig["TermsAndConditions"]))
                            {
                                // we already have shipping selected threfore we can skip Options Xform
                                XmlElement oSubmitBtn = (XmlElement)oContactXform.moXformElmt.SelectSingleNode("descendant-or-self::submit[@submission='SubmitAdd']");
                                buttonRef = oSubmitBtn.GetAttribute("ref");
                                double PaymentAmount = Convert.ToDouble("0" + oCartElmt.GetAttribute("total"));
                                XmlElement xmlParentNodeElmt = (XmlElement)oSubmitBtn.ParentNode;
                                oPay.getPaymentMethodButtons(ref oContactXform, ref xmlParentNodeElmt, PaymentAmount);
                                bSubmitPaymentMethod = true;
                            }
                        }
                    }

                    if (oContactXform.valid == false)
                    {
                        // show the form
                        XmlElement oContentElmt = (XmlElement)moPageXml.SelectSingleNode("/Page/Contents");
                        if (oContentElmt is null)
                        {
                            oContentElmt = moPageXml.CreateElement("Contents");
                            if (moPageXml.DocumentElement is null)
                            {
                                throw new Exception("PAGE IS NOT CREATED");
                            }
                            else
                            {
                                moPageXml.DocumentElement.AppendChild(oContentElmt);
                            }
                        }
                        oContentElmt.AppendChild(oContactXform.moXformElmt);
                    }
                    else
                    {
                        if (!cAddressType.Contains("Delivery"))
                        {

                            // Valid Form, let's adjust the Vat rate
                            // AJG By default the tax rate is picked up from the billing address, unless otherwise specified.
                            // 
                            if (moCartConfig["TaxFromDeliveryAddress"] != "on" | myWeb.moRequest["cIsDelivery"] == "True" & moCartConfig["TaxFromDeliveryAddress"] == "on")
                            {
                                if (oContactXform.Instance.SelectSingleNode("tblCartContact[@type='Billing Address']") != null)
                                {
                                    if (oCartElmt.SelectSingleNode("Contact[@type='Billing Address']") != null)
                                    {
                                        string argcContactCountry = oCartElmt.SelectSingleNode("Contact[@type='Billing Address']/Country").InnerText;
                                        UpdateTaxRate(ref argcContactCountry);
                                        oCartElmt.SelectSingleNode("Contact[@type='Billing Address']/Country").InnerText = argcContactCountry;
                                    }
                                }
                            }

                            // to allow for single form with multiple addresses.
                            if (moCartConfig["TaxFromDeliveryAddress"] == "on" & oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']") != null)
                            {
                                string argcContactCountry1 = oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText;
                                UpdateTaxRate(ref argcContactCountry1);
                                oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText = argcContactCountry1;
                            }

                            // Skip Delivery if:
                            // - Deliver to this address is selected
                            // - mbNoDeliveryAddress is True
                            // - the order is part of a giftlist (the delivery address is pre-determined)
                            // - we have submitted the delivery address allready
                            if (myWeb.moRequest["cIsDelivery"] == "True" | mbNoDeliveryAddress | mnGiftListId > 0 | oContactXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='Delivery Address']") != null | oContactXform.moXformElmt.GetAttribute("cartCmd") == "ChoosePaymentShippingOption")




                            {

                                if (bSubmitPaymentMethod)
                                {
                                    // we have payment method buttons on the form.
                                    mcPaymentMethod = myWeb.moRequest[buttonRef];
                                }

                                mcCartCmd = "ChoosePaymentShippingOption";
                                mnProcessId = 3;
                            }

                            else
                            {
                                // If mbEwMembership = True And myWeb.mnUserId <> 0 Then
                                // 'all handled in pick form
                                // Else
                                long BillingAddressID = setCurrentBillingAddress((long)myWeb.mnUserId, 0L);
                                if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + BillingAddressID]))
                                {
                                    // we are editing an address form the pick address form so lets go back.
                                    mcCartCmd = "Billing";
                                    mnProcessId = 2;
                                }
                                else
                                {
                                    mcCartCmd = "Delivery";
                                }



                                // End If
                                // billing address is saved, so up cart status if needed
                                // If mnProcessId < 2 Then mnProcessId = 2
                                // If mnProcessId > 2 Then
                                // mcCartCmd = "ChoosePaymentShippingOption"
                                // mnProcessId = 3
                                // End If

                            }
                            if (myWeb.mnUserId > 0)
                            {
                                setCurrentBillingAddress((long)myWeb.mnUserId, 0L);
                            }
                        }


                        else // Case for Delivery
                        {

                            // AJG If specified, the tax rate can be picked up from the delivery address
                            if (moCartConfig["TaxFromDeliveryAddress"] == "on")
                            {
                                string argcContactCountry2 = oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText;
                                UpdateTaxRate(ref argcContactCountry2);
                                oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText = argcContactCountry2;
                            }

                            // TS if we don't have a billing address we need one
                            DataSet oDs2;
                            oDs2 = moDBHelper.GetDataSet("select * from tblCartContact where nContactCartId = " + mnCartId.ToString() + " and cContactType = 'Billing Address'", "tblCartContact");
                            if (oDs2.Tables["tblCartContact"].Rows.Count > 0)
                            {
                                mcCartCmd = "ChoosePaymentShippingOption";
                                mnProcessId = 3;
                            }

                            else if (myWeb.mnUserId > 0)
                            {
                                long BillingAddressID = setCurrentBillingAddress((long)myWeb.mnUserId, 0L);

                                if (BillingAddressID > 0L)
                                {
                                    // set the billing address
                                    string sSql = "Select nContactKey from tblCartContact where cContactType = 'Delivery Address' and nContactCartid=" + mnCartId;
                                    string DeliveryAddressID = moDBHelper.ExeProcessSqlScalar(sSql);
                                    useSavedAddressesOnCart(BillingAddressID, Convert.ToInt64(DeliveryAddressID), null);

                                    mcPaymentMethod = myWeb.moRequest[buttonRef];

                                    mcCartCmd = "ChoosePaymentShippingOption";
                                    mnProcessId = 3;
                                }
                                else
                                {
                                    mcCartCmd = "Billing";
                                    mnProcessId = 2;
                                }
                            }
                            else
                            {
                                mcCartCmd = "Billing";
                                mnProcessId = 2;


                            }

                        }

                        // save address against the user
                        if (mbEwMembership == true & myWeb.mnUserId > 0 & oContactXform.valid)
                        {

                            if (!(oContactXform.moXformElmt.GetAttribute("persistAddress") == "false"))
                            {
                                cProcessInfo = "UpdateExistingUserAddress for : " + myWeb.mnUserId;
                                UpdateExistingUserAddress(ref oContactXform);
                            }

                        }

                    }
                    oContactXform = (Cms.xForm)null;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "addressSubProcess", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }

            }

            public virtual bool usePreviousAddress(ref XmlElement oCartElmt)
            {
                string cProcessInfo = "usePreviousAddress";
                string sSql;
                long billingAddId = 0L;
                long deliveryAddId = 0L;
                DataSet oDs;
                try
                {
                    if (mbEwMembership == true & myWeb.mnUserId != 0)
                    {
                        if (moCartConfig["UsePreviousAddress"]?.ToLower() == "on")
                        {

                            sSql = "select nContactKey, cContactType, nAuditKey from tblCartContact inner join tblAudit a on nAuditId = a.nAuditKey where nContactCartId = 0 and nContactDirId =" + myWeb.mnUserId.ToString();
                            oDs = moDBHelper.GetDataSet(sSql, "tblCartContact");

                            foreach (DataRow odr in oDs.Tables["tblCartContact"].Rows)
                            {
                                if (odr["cContactType"]?.ToString() == "Billing Address")
                                {
                                    billingAddId = Convert.ToInt64(odr["nContactKey"]);
                                }
                                if (mbNoDeliveryAddress)
                                {
                                    deliveryAddId = billingAddId;
                                }
                                else if (odr["cContactType"]?.ToString() == "Delivery Address")
                                {
                                    deliveryAddId = Convert.ToInt64(odr["nContactKey"]);
                                }
                            }
                            if (deliveryAddId != 0L & billingAddId != 0L)
                            {
                                useSavedAddressesOnCart(billingAddId, deliveryAddId, null);
                                // skip
                                mcCartCmd = "ChoosePaymentShippingOption";
                                return true;
                            }
                            else
                            {
                                // we don't have the addresses we need so we need to go to address step anyhow
                                return false;
                            }
                        }

                        else
                        {
                            // Use previous address functionality turned off
                            return false;
                        }
                    }
                    else
                    {
                        // User not logged on or membership is off
                        return false;
                    }
                }



                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "addressSubProcess", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                    return false;
                }

            }


            public virtual Cms.xForm contactXform(string cAddressType, string cSubmitName = "submit", string cCmdType = "cartCmd", string cCmdAction = "")
            {

                return contactXform(cAddressType, cSubmitName, cCmdType, cCmdAction, false);

            }


            public virtual Cms.xForm contactXform(string cAddressType, string cSubmitName, string cCmdType, string cCmdAction, bool bDontPopulate, long ContactId = 0L, string cmd2 = "")
            {
                myWeb.PerfMon.Log("Cart", "contactXform");
                xForm oXform = new xForm(ref myWeb.msException);
                XmlElement oGrpElmt;
                DataSet oDs;
                XmlElement oElmt;
                string cWhere;
                string cProcessInfo = "";
                string cXformLocation = "";
                bool bIsBespokeXform = false;
                var bGetInstance = default(bool);
                string sSql;

                try
                {

                    // Build the xform
                    oXform.moPageXML = moPageXml;
                    oXform.NewFrm(cAddressType);

                    // Check for bespoke xform
                    switch (cAddressType ?? "")
                    {
                        case "Billing Address":
                            {
                                cXformLocation = mcBillingAddressXform;
                                break;
                            }
                        case "Delivery Address":
                            {
                                cXformLocation = mcDeliveryAddressXform;
                                break;
                            }
                    }

                    // Test that a bespoke form exists and is a valid filename
                    bIsBespokeXform = File.Exists(myWeb.goServer.MapPath(cXformLocation)) && cXformLocation.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);

                    // :::::::::::::::::::::::::::::::::::::::::::::::
                    // :::: CONTACT XFORM :: GROUP and BIND BUILD ::::
                    // :::::::::::::::::::::::::::::::::::::::::::::::

                    if (bIsBespokeXform)
                    {
                        // Load the bespoke form
                        oXform.load(cXformLocation);

                        // select the first group element for adding delivery checkbox
                        oGrpElmt = (XmlElement)oXform.moXformElmt.SelectSingleNode("group[1]");
                        if (ContactId > 0L)
                        {
                            bGetInstance = true;
                        }
                    }
                    else
                    {
                        // Build the xform because file not specified
                        bGetInstance = true;

                        oXform.addGroup(ref oXform.moXformElmt, "address", sLabel: cAddressType);
                        oGrpElmt = (XmlElement)oXform.moXformElmt.LastChild;

                        oXform.addInput(ref oGrpElmt, cCmdType, true, cCmdType, "hidden");
                        XmlElement argoBindParent = null;
                        oXform.addBind(cCmdType, cCmdType, oBindParent: ref argoBindParent);

                        oXform.addInput(ref oGrpElmt, "cContactType", true, "Type", "hidden");
                        XmlElement argoBindParent1 = null;
                        oXform.addBind("cContactType", "tblCartContact/cContactType", oBindParent: ref argoBindParent1);

                        oXform.addInput(ref oGrpElmt, "cContactName", true, "Name", "textbox required");
                        XmlElement argoBindParent2 = null;
                        oXform.addBind("cContactName", "tblCartContact/cContactName", oBindParent: ref argoBindParent2, "true()", "string");

                        oXform.addInput(ref oGrpElmt, "cContactCompany", true, "Company", "textbox");
                        XmlElement argoBindParent3 = null;
                        oXform.addBind("cContactCompany", "tblCartContact/cContactCompany", oBindParent: ref argoBindParent3, "false()");

                        oXform.addInput(ref oGrpElmt, "cContactAddress", true, "Address", "textbox required");
                        XmlElement argoBindParent4 = null;
                        oXform.addBind("cContactAddress", "tblCartContact/cContactAddress", oBindParent: ref argoBindParent4, "true()", "string");

                        oXform.addInput(ref oGrpElmt, "cContactCity", true, "City", "textbox required");
                        XmlElement argoBindParent5 = null;
                        oXform.addBind("cContactCity", "tblCartContact/cContactCity", oBindParent: ref argoBindParent5, "true()");

                        oXform.addInput(ref oGrpElmt, "cContactState", true, "County/State", "textbox");
                        XmlElement argoBindParent6 = null;
                        oXform.addBind("cContactState", "tblCartContact/cContactState", oBindParent: ref argoBindParent6);

                        oXform.addInput(ref oGrpElmt, "cContactZip", true, "Postcode/Zip", "textbox required");
                        XmlElement argoBindParent7 = null;
                        oXform.addBind("cContactZip", "tblCartContact/cContactZip", oBindParent: ref argoBindParent7, "true()", "string");

                        oXform.addSelect1(ref oGrpElmt, "cContactCountry", true, "Country", "dropdown required");
                        XmlElement argoBindParent8 = null;
                        oXform.addBind("cContactCountry", "tblCartContact/cContactCountry", oBindParent: ref argoBindParent8, "true()", "string");

                        oXform.addInput(ref oGrpElmt, "cContactTel", true, "Tel", "textbox");
                        XmlElement argoBindParent9 = null;
                        oXform.addBind("cContactTel", "tblCartContact/cContactTel", oBindParent: ref argoBindParent9);

                        oXform.addInput(ref oGrpElmt, "cContactFax", true, "Fax", "textbox");
                        XmlElement argoBindParent10 = null;
                        oXform.addBind("cContactFax", "tblCartContact/cContactFax", oBindParent: ref argoBindParent10);

                        // Only show email address for Billing
                        if (cAddressType == "Billing Address" | mnGiftListId > 0)
                        {
                            oXform.addInput(ref oGrpElmt, "cContactEmail", true, "Email", "textbox required");
                            XmlElement argoBindParent11 = null;
                            oXform.addBind("cContactEmail", "tblCartContact/cContactEmail", oBindParent: ref argoBindParent11, "true()", "email");
                        }
                        if (myWeb.moConfig["cssFramework"] == "bs3")
                        {
                            oXform.addSubmit(ref oGrpElmt, "Submit" + cAddressType.Replace(" ", ""), "Submit");
                        }
                        else
                        {
                            oXform.addSubmit(ref oGrpElmt, "Submit" + cAddressType.Replace(" ", ""), "Submit", cSubmitName + cAddressType.Replace(" ", ""));
                        }
                        oXform.submission("Submit" + cAddressType.Replace(" ", ""), string.IsNullOrEmpty(cCmdAction) ? "" : "?" + cCmdType + "=" + cCmdAction, "POST", "return form_check(this);");

                    }

                    // Add the countries list to the form
                    foreach (XmlElement currentOElmt in oXform.moXformElmt.SelectNodes("descendant-or-self::select1[contains(@class, 'country') or @bind='cContactCountry']"))
                    {
                        oElmt = currentOElmt;
                        string cThisAddressType;
                        var contactTypeNode = oElmt.ParentNode.SelectSingleNode("input[@bind='cContactType' or @bind='cDelContactType']/value");
                        if (contactTypeNode is null)
                        {
                            cThisAddressType = cAddressType;
                        }
                        else
                        {
                            cThisAddressType = contactTypeNode.InnerText;
                        }
                        if (mbNoDeliveryAddress)
                        {
                            cThisAddressType = "Delivery Address";
                        }

                        populateCountriesDropDown(ref oXform, ref oElmt, cThisAddressType);
                    }

                    // Add the Delivery Checkbox if needed

                    if (cAddressType == "Billing Address" & mnProcessId < 2 & !mbNoDeliveryAddress & !(mnGiftListId > 0))
                    {
                        // this can be optionally turned off in the xform, by the absence of cIsDelivery in the instance.
                        if (oXform.Instance.SelectSingleNode("tblCartContact[@type='Delivery Address']") is null & oXform.Instance.SelectSingleNode("//bDisallowDeliveryCheckbox") is null)
                        {
                            // shoppers have the option to send to same address as the billing with a checkbox
                            oXform.addSelect(ref oGrpElmt, "cIsDelivery", true, "Deliver to This Address", "checkbox", Protean.xForm.ApperanceTypes.Minimal);
                            XmlElement argoSelectNode = (XmlElement)oGrpElmt.LastChild;
                            oXform.addOption(ref argoSelectNode, "", "True");
                            XmlElement argoBindParent12 = null;
                            oXform.addBind("cIsDelivery", "tblCartContact/cIsDelivery", oBindParent: ref argoBindParent12);
                        }
                    }

                    if (moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                    {
                        // Add Collection options
                        XmlElement oIsDeliverySelect = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::select[@bind='cIsDelivery']");
                        if (oIsDeliverySelect != null)
                        {

                            // Create duplicate select as select1
                            var newElmt = moPageXml.CreateElement("select1");
                            foreach (XmlAttribute oAtt in oIsDeliverySelect.Attributes)
                                newElmt.SetAttribute(oAtt.Name, oAtt.Value);
                            foreach (XmlNode oNode in oIsDeliverySelect.ChildNodes)
                                newElmt.AppendChild(oNode.CloneNode(true));

                            var delBillingElmt = oXform.addOption(ref newElmt, "Deliver to Billing Address", "false");

                            bool bCollection = false;
                            bool bOverrideCollection = false;
                            // Get the collection delivery options
                            // Dim oDrCollectionOptions As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where bCollection = 1")
                            // Add one key in config for running functionality of bCollection - OverrideCollection
                            if (moConfig["OverrideCollection"] != null)
                            {
                                if ((moConfig["OverrideCollection"]) != "" && (moConfig["OverrideCollection"]).ToLower() == "true")
                                {
                                    bOverrideCollection = true;
                                }
                            }
                            if (bOverrideCollection == false)
                            {
                                using (var oDrCollectionOptions = moDBHelper.getDataReaderDisposable("select * from tblCartShippingMethods where bCollection = 1"))  // Done by nita on 6/7/22
                                {
                                    while (oDrCollectionOptions.Read())
                                    {
                                        string OptLabel = "<span class=\"opt-name\">" + oDrCollectionOptions["cShipOptName"].ToString() + "</span>";
                                        OptLabel = OptLabel + "<span class=\"opt-carrier\">" + oDrCollectionOptions["cShipOptCarrier"].ToString() + "</span>";
                                        oXform.addOption(ref newElmt, OptLabel, oDrCollectionOptions["nShipOptKey"].ToString(), true);
                                        bCollection = true;
                                    }
                                    // Only change this if collection shipping options exist.
                                    if (bCollection)
                                    {
                                        oIsDeliverySelect.ParentNode.ReplaceChild(newElmt, oIsDeliverySelect);
                                    }
                                    else
                                    {
                                        // this was all for nuffin
                                        newElmt = null;
                                    }
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(cmd2))
                    {
                        oXform.addInput(ref oGrpElmt, cmd2, true, cmd2, "hidden");
                        XmlElement argoBindParent13 = null;
                        oXform.addBind(cmd2, "cmd2", oBindParent: ref argoBindParent13);
                    }

                    // :::::::::::::::::::::::::::::::::::::::::::::::
                    // :::: CONTACT XFORM :: CREATE/LOAD INSTANCE ::::
                    // :::::::::::::::::::::::::::::::::::::::::::::::

                    // When there is no match this will get the default instance based on the table schema.  
                    // This will override any form that we have loaded in, so we need to put exceptions in.
                    if (bGetInstance)
                    {
                        oXform.Instance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, ContactId);
                        if (ContactId > 0L)
                        {
                            bDontPopulate = false;
                        }
                    }

                    // if the instance is empty fill these values
                    bool bAddIds = false;

                    // catch for sites where nContactKey is not specified.
                    if (oXform.Instance.SelectSingleNode("*/nContactKey") is null)
                    {
                        bAddIds = true;
                    }
                    else if (string.IsNullOrEmpty(oXform.Instance.SelectSingleNode("*/nContactKey").InnerText))
                    {
                        bAddIds = true;
                    }

                    if (bAddIds)
                    {
                        foreach (XmlElement currentOElmt1 in oXform.Instance.SelectNodes("*/nContactDirId"))
                        {
                            oElmt = currentOElmt1;
                            oElmt.InnerText = myWeb.mnUserId.ToString();
                        }
                        foreach (XmlElement currentOElmt2 in oXform.Instance.SelectNodes("*/nContactCartId"))
                        {
                            oElmt = currentOElmt2;
                            oElmt.InnerText = mnCartId.ToString();
                        }
                        foreach (XmlElement currentOElmt3 in oXform.Instance.SelectNodes("*/cContactType"))
                        {
                            oElmt = currentOElmt3;
                            if (string.IsNullOrEmpty(oElmt.InnerText))
                                oElmt.InnerText = cAddressType;
                        }
                    }

                    // make sure we don't show a random address.
                    if (mnCartId == 0)
                        bDontPopulate = true;

                    if (bDontPopulate == false)
                    {
                        // if we have addresses in the cart insert them
                        sSql = "select nContactKey, cContactType from tblCartContact where nContactCartId = " + mnCartId.ToString();
                        oDs = moDBHelper.GetDataSet(sSql, "tblCartContact");
                        foreach (DataRow oDr in oDs.Tables["tblCartContact"].Rows)
                        {
                            var tempInstance = moPageXml.CreateElement("TempInstance");
                            tempInstance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, Convert.ToInt64(oDr["nContactKey"]));
                            XmlElement instanceAdd = (XmlElement)oXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='" + oDr["cContactType"] + "']");

                            if (instanceAdd != null)
                            {
                                instanceAdd.ParentNode.ReplaceChild(tempInstance.FirstChild, instanceAdd);

                                instanceAdd = (XmlElement)oXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='" + oDr["cContactType"] + "']");

                                if (instanceAdd != null)
                                {
                                    instanceAdd.SetAttribute("type", oDr["cContactType"].ToString());
                                }
                            }

                        }
                        oDs = null;
                        // set the isDelivery Value
                        // remember the delivery address setting.
                        XmlElement delivElmt = (XmlElement)oXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='Delivery Address']");
                        if (delivElmt != null)
                        {
                            if (string.IsNullOrEmpty(myWeb.moSession["isDelivery"]?.ToString()))
                            {
                                delivElmt.SetAttribute("isDelivery", "false");
                            }
                            else
                            {
                                delivElmt.SetAttribute("isDelivery", Convert.ToString(myWeb.moSession["isDelivery"]));
                            }
                        }

                    }

                    // Dim bGetInstance As Boolean = True
                    // If bIsBespokeXform Then
                    // ' There is a bespoke form, let's check for the presence of an item in the contact table
                    // Dim oCartContactInDB As Object = moDBHelper.GetDataValue("SELECT COUNT(*) As FoundCount FROM tblCartContact " & ssql)
                    // If Not (oCartContactInDB > 0) Then bGetInstance = False ' No Item so do not get the instance
                    // End If



                    // If membership is on and the user is logged on, we need to check if this is an existing address in the user's list of addresses
                    if (mbEwMembership == true & myWeb.mnUserId > 0)
                    {

                        // If we are using the Pick Address list to EDIT an address, there will be a hidden control of userAddId
                        if (!string.IsNullOrEmpty(myWeb.moRequest["userAddId"]))
                        {
                            oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressId"));
                            oXform.Instance.LastChild.InnerText = myWeb.moRequest["userAddId"];
                            oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressType"));
                            oXform.Instance.LastChild.InnerText = myWeb.moRequest["userAddType"];
                        }
                        else
                        {
                            // Holy large where statement, Batman!  But how on earth else do we tell is a cart address is the same as a user address?
                            string value = string.Empty;
                            XmlElement contact = (XmlElement)oXform.Instance.SelectSingleNode("tblCartContact");
                            cWhere = "";
                            if (Tools.Xml.NodeState(ref contact, "cContactName", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere += " and cContactName='" + SqlFmt(value) + "' ";

                            if (Tools.Xml.NodeState(ref contact, "cContactCompany", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere += " and cContactCompany='" + SqlFmt(value) + "' ";

                            if (Tools.Xml.NodeState(ref contact, "cContactAddress", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere += " and cContactAddress='" + SqlFmt(value) + "' ";

                            if (Tools.Xml.NodeState(ref contact, "cContactCity", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere += " and cContactCity='" + SqlFmt(value) + "' ";

                            if (Tools.Xml.NodeState(ref contact, "cContactState", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere += " and cContactState='" + SqlFmt(value) + "' ";

                            if (Tools.Xml.NodeState(ref contact, "cContactZip", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere += " and cContactZip='" + SqlFmt(value) + "' ";

                            if (Tools.Xml.NodeState(ref contact, "cContactCountry", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere += " and cContactCountry='" + SqlFmt(value) + "' ";

                            // cWhere = " and cContactName='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactName").InnerText) & "' " & _
                            // "and cContactCompany='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactCompany").InnerText) & "' " & _
                            // "and cContactAddress='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactAddress").InnerText) & "' " & _
                            // "and cContactCity='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactCity").InnerText) & "' " & _
                            // "and cContactState='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactState").InnerText) & "' " & _
                            // "and cContactZip='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactZip").InnerText) & "' " & _
                            // "and cContactCountry='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText) & "' "
                            oDs = moDBHelper.GetDataSet("select * from tblCartContact where nContactDirId = " + myWeb.mnUserId.ToString() + cWhere, "tblCartContact");
                            if (oDs.Tables["tblCartContact"].Rows.Count > 0)
                            {
                                oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressId"));
                                oXform.Instance.LastChild.InnerText = Convert.ToString(oDs.Tables["tblCartContact"].Rows[0]["nContactKey"]);
                                oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressType"));
                                oXform.Instance.LastChild.InnerText = Convert.ToString(oDs.Tables["tblCartContact"].Rows[0]["cContactType"]);
                            }
                        }
                    }


                    // add some proceedual fields to instance
                    oElmt = oXform.moPageXML.CreateElement(cCmdType);
                    oXform.Instance.AppendChild(oElmt);

                    oElmt = oXform.moPageXML.CreateElement("cIsDelivery");
                    oXform.Instance.AppendChild(oElmt);

                    if (!string.IsNullOrEmpty(cmd2))
                    {
                        oElmt = oXform.moPageXML.CreateElement("cmd2");
                        oElmt.InnerText = "True";
                        oXform.Instance.AppendChild(oElmt);
                    }


                    // If oXform.isSubmitted And cAddressType = myWeb.moRequest.Form("cContactType") Then

                    if (oXform.isSubmitted())
                    {
                        oXform.updateInstanceFromRequest();
                        oXform.validate();

                        myWeb.moSession["isDelivery"] = myWeb.moRequest["isDelivery"];

                        // Catch for space as country
                        if (myWeb.moRequest["cContactCountry"] != default)
                        {
                            if (string.IsNullOrEmpty(myWeb.moRequest["cContactCountry"].Trim()))
                            {
                                oXform.valid = false;
                                oXform.addNote("cContactCountry", Protean.xForm.noteTypes.Alert, "Please select a country", true);
                            }
                        }

                        if (oXform.valid)
                        {
                            // the form is valid so update it - add a check for timed out session (mnCartId = 0)
                            if (ContactId > 0L)
                            {
                                // ID is specified so we simply update ignore relation to the cart
                                moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oXform.Instance, ContactId);
                            }
                            else if (mnCartId > 0)
                            {
                                // test if we have a address of this type against the order..!
                                // ssql = "Select nContactKey from tblCartContact where cContactType = '" & cAddressType & "' and nContactCartid=" & mnCartId
                                // Dim sContactKey1 As String = moDBHelper.ExeProcessSqlScalar(ssql)
                                // moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oXform.Instance, sContactKey1)

                                // Step through for multiple addresses
                                bool bSavedDelivery = false;



                                // check for collection options
                                if (Tools.Number.IsNumeric(myWeb.moRequest["cIsDelivery"]))
                                {
                                    // Save the delivery method allready
                                    string cSqlUpdate = "";
                                    // Dim oDrCollectionOptions2 As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where nShipOptKey = " & myWeb.moRequest("cIsDelivery"))
                                    using (var oDrCollectionOptions2 = moDBHelper.getDataReaderDisposable("select * from tblCartShippingMethods where nShipOptKey = " + myWeb.moRequest["cIsDelivery"]))  // Done by nita on 6/7/22
                                    {
                                        while (oDrCollectionOptions2.Read())
                                        {
                                            string cShippingDesc = oDrCollectionOptions2["cShipOptName"].ToString() + "-" + oDrCollectionOptions2["cShipOptCarrier"].ToString() + "</span>";
                                            // Convert shipping cost safely
                                            double nShippingCost = 0;
                                            if (oDrCollectionOptions2["nShipOptCost"] != DBNull.Value)
                                            {
                                                double.TryParse(oDrCollectionOptions2["nShipOptCost"].ToString(), out nShippingCost);
                                            }

                                            // Build SQL update query
                                            cSqlUpdate = $@"UPDATE tblCartOrder SET cShippingDesc = '{SqlFmt(cShippingDesc)}', nShippingCost = {SqlFmt(nShippingCost.ToString())}, nShippingMethodId = {SqlFmt(myWeb.moRequest["cIsDelivery"])} WHERE nCartOrderKey = {mnCartId}";
                                        }

                                        moDBHelper.ExeProcessSql(cSqlUpdate);
                                        bSavedDelivery = true;
                                    }
                                }
                                // If it exists and we are here means we may have changed the Delivery address country

                                else if ((moCartConfig["BlockRemoveDelivery"])?.ToLower() != "on")
                                {
                                    RemoveDeliveryOption(mnCartId);

                                }

                                if (moDBHelper.checkTableColumnExists("tblCartOrder", "nReceiptType"))
                                {
                                    if (myWeb.moRequest["cIsDelivery"] == "true" & myWeb.moRequest["cIsPaperRecieptForDelAddress"] == "true")
                                    {
                                        // check flag condition
                                        string cSqlUpdate = "UPDATE tblCartOrder SET nReceiptType=2 WHERE nCartOrderKey=" + mnCartId;
                                        moDBHelper.ExeProcessSql(cSqlUpdate);
                                    }

                                    else
                                    {
                                        // check flag condition
                                        string cSqlUpdate = "UPDATE tblCartOrder SET nReceiptType=1 WHERE nCartOrderKey=" + mnCartId;
                                        moDBHelper.ExeProcessSql(cSqlUpdate);
                                    }
                                }
                                foreach (XmlElement currentOElmt4 in oXform.Instance.SelectNodes("tblCartContact"))
                                {
                                    oElmt = currentOElmt4;
                                    string cThisAddressType = oElmt.SelectSingleNode("cContactType").InnerText;
                                    sSql = "Select nContactKey from tblCartContact where cContactType = '" + cThisAddressType + "' and nContactCartid=" + mnCartId;
                                    string sContactKey1 = moDBHelper.ExeProcessSqlScalar(sSql);
                                    var saveInstance = moPageXml.CreateElement("instance");
                                    saveInstance.AppendChild(oElmt.Clone());
                                    moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, saveInstance, Convert.ToInt64(sContactKey1));
                                    if (cThisAddressType == "Delivery Address")
                                        bSavedDelivery = true;
                                }

                                // if the option save Delivery is true then
                                if (bSavedDelivery == false)
                                {
                                    if (myWeb.moRequest["cIsDelivery"] == "True" | mbNoDeliveryAddress & cAddressType == "Billing Address")
                                    {
                                        if (myWeb.moRequest["cIsDelivery"] == "True" & mnShippingRootId > 0)
                                        {
                                            // mnShippingRootId
                                            // check if the submitted country matches one in the delivery list
                                            var oCheckElmt = moPageXml.CreateElement("ValidCountries");
                                            ListShippingLocations(ref oCheckElmt);
                                            string cCountry = oXform.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText;
                                            if (oCheckElmt.SelectSingleNode("descendant-or-self::TreeItem[@Name='" + cCountry + "' or @name='" + cCountry + "' or @nameShort='" + cCountry + "']") is null)
                                            {
                                                oXform.valid = false;
                                                oXform.addNote("cContactCountry", Protean.xForm.noteTypes.Alert, "Cannot Deliver to this country. please select another.", true);
                                            }
                                        }
                                        if (oXform.valid)
                                        {
                                            sSql = "Select nContactKey from tblCartContact where cContactType = 'Delivery Address' and nContactCartid=" + mnCartId;
                                            string sContactKey2 = moDBHelper.ExeProcessSqlScalar(sSql);
                                            oXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText = "Delivery Address";
                                            moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oXform.Instance, Convert.ToInt64(sContactKey2));
                                            // going to set it back to a billing address
                                            oXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText = "Billing Address";
                                        }
                                    }
                                }
                                if (oXform.Instance.SelectSingleNode("tblCartContact/cContactEmail") != null)
                                {
                                    if (myWeb.moDbHelper.checkTableColumnExists("tblOptOutAddresses", "nOptOutKey"))
                                    {
                                        if (oXform.Instance.SelectSingleNode("tblCartContact/cContactEmail/@optOut") != null)
                                        {
                                            sSql = "Select nContactKey from tblCartContact where cContactType = 'Billing Address' and nContactCartid=" + mnCartId;
                                            string sContactKey3 = moDBHelper.ExeProcessSqlScalar(sSql);
                                            moDBHelper.AddOptOutEmail(oXform.Instance.SelectSingleNode("tblCartContact/cContactEmail").InnerText, sContactKey3, oXform.Instance.SelectSingleNode("tblCartContact/cContactEmail/@optOut").InnerText);

                                        }
                                    }
                                    else
                                    {
                                        if (oXform.Instance.SelectSingleNode("tblCartContact/cContactEmail[@optOut='true']") != null)
                                        {
                                            moDBHelper.AddInvalidEmail(oXform.Instance.SelectSingleNode("tblCartContact/cUserId[@optOut='true']").InnerText);

                                        }
                                    }
                                }
                            }

                            else
                            {
                                // Throw an error to indicate that the user has timed out
                                mnProcessError = 4;
                            }
                            if (!string.IsNullOrEmpty(myWeb.moRequest["cContactOpt-In"]))
                            {
                                this.AddToLists("Newsletter", ref moCartXml, myWeb.moRequest["cContactName"], myWeb.moRequest["cContactEmail"]);
                            }

                        }

                    }



                    oXform.addValues();

                    return oXform;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "contactXform", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
                }

            }




            public virtual Cms.xForm pickContactXform(string cAddressType, string submitPrefix = "", string cCmdType = "cartCmd", string cCmdAction = "")
            {
                myWeb.PerfMon.Log("Cart", "pickContactXform");
                var oXform = new Cms.xForm(ref myWeb.msException);

                Cms.xForm oReturnForm;
                XmlElement oGrpElmt;
                DataSet oDs;
                DataSet oDs2;
                DataRow oDr;
                string cSql = "";
                // Dim sAddressHtml As String
                string cProcessInfo = "";
                long contactId = 0L;
                long billingAddId = 0L;
                Cms.xForm oContactXform = (Cms.xForm)null;
                bool bDontPopulate = false;
                bool bBillingSet = false;
                string newSubmitPrefix = submitPrefix;
                string newAddressType = cAddressType;
                string contactFormCmd2 = "";
                try
                {
                    myWeb.moSession["tempInstance"] = null;
                    // Get any existing addresses for user
                    // Changed this so it gets any

                    // Check if updated primiary billing address, (TS added order by reverse order added)
                    cSql = "select * from tblCartContact where nContactDirId = " + myWeb.mnUserId.ToString() + " and nContactCartId = 0 and (cContactType like 'Billing Address' or cContactType like 'Delivery Address')  order by cContactType ASC, nContactKey DESC";
                    oDs = moDBHelper.GetDataSet(cSql, "tblCartContact");
                    foreach (DataRow currentODr in oDs.Tables["tblCartContact"].Rows)
                    {
                        oDr = currentODr;
                        if (billingAddId == 0L)
                            billingAddId = Convert.ToInt64(oDr["nContactKey"]);

                        string requestKey = "cartDeleditAddress" + oDr["nContactKey"].ToString();
                        if (!string.IsNullOrEmpty(myWeb.moRequest[requestKey]))
                        {
                            submitPrefix = "cartDel";
                            cAddressType = "Delivery Address";

                            newSubmitPrefix = "cartDel";
                            newAddressType = "Delivery Address";

                            // ensure we hit this next time through...
                            cCmdAction = "Delivery";
                            contactFormCmd2 = "cartDeleditAddress" + oDr["nContactKey"].ToString();
                        }
                        else if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addDelivery" + oDr["nContactKey"].ToString()]))
                        {
                            bDontPopulate = true;
                            newSubmitPrefix = "cartDel";
                            newAddressType = "Delivery Address";
                            cCmdAction = "Delivery";
                        }
                        else if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + oDr["nContactKey"].ToString()]))
                        {
                            if (billingAddId == null || !billingAddId.Equals(oDr["nContactKey"]))
                            {
                                newSubmitPrefix = "cartDel";
                                newAddressType = "Delivery Address";
                            }
                            else
                            {
                                // we are editing a billing address and want to ensure we dont get a double form.
                                if (myWeb.bs5)
                                {
                                    if (mcBillingAddressXform.Contains("both-addresses.xml"))
                                    {
                                        mcBillingAddressXform = mcBillingAddressXform.Replace("both-addresses.xml", "billing-address.xml");
                                    }
                                }
                                else
                                {
                                    if (mcBillingAddressXform.Contains("BillingAndDeliveryAddress.xml"))
                                    {
                                        mcBillingAddressXform = mcBillingAddressXform.Replace("BillingAndDeliveryAddress.xml", "BillingAddress.xml");
                                    }
                                }


                                // ensure we hit this next time through...
                                cCmdAction = "Billing";
                                contactFormCmd2 = submitPrefix + "editAddress" + oDr["nContactKey"].ToString();
                                bDontPopulate = true;
                                // we specifiy a contactID to ensure we don't update the cart addresses just the ones on file.
                                contactId = Convert.ToInt64(oDr["nContactKey"]);
                            }
                        }
                        else if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "useBilling" + oDr["nContactKey"].ToString()]))
                        {
                            contactId = setCurrentBillingAddress((long)myWeb.mnUserId, Convert.ToInt64(oDr["nContactKey"]));
                            bBillingSet = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addNewAddress"]))
                    {
                        contactId = 0L;
                        bDontPopulate = true;
                    }

                    oContactXform = contactXform(newAddressType, newSubmitPrefix + "Address", cCmdType, cCmdAction, bDontPopulate, contactId, contactFormCmd2);

                    // Build the xform
                    oXform.moPageXML = moPageXml;
                    string cPickAddressXform = moCartConfig["PickAddressXForm"];

                    if (!string.IsNullOrEmpty(cPickAddressXform))
                    {
                        if (!oXform.load(cPickAddressXform))
                        {
                            oXform.NewFrm(cAddressType);
                        }
                    }
                    else
                    {
                        oXform.NewFrm(cAddressType);
                    }
                    oXform.valid = false;

                    // oReturnForm is going to be the form returned at the end of the function.
                    oReturnForm = oXform;

                    if (!bBillingSet)
                    {
                        contactId = setCurrentBillingAddress((long)myWeb.mnUserId, 0L);
                    }
                    else
                    {
                        cSql = "Select * from tblCartContact where nContactDirId = " + myWeb.mnUserId.ToString() + " And nContactCartId = 0 And (cContactType='Billing Address' or cContactType='Delivery Address') order by cContactType ASC";
                        oDs = moDBHelper.GetDataSet(cSql, "tblCartContact");
                    }

                    if (oDs.Tables["tblCartContact"].Rows.Count > 0)
                    {

                        // Create the instance
                        oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("cContactId"));

                        // Add a value if an address has been selected
                        oDs2 = moDBHelper.GetDataSet("select * from tblCartContact where nContactCartId = " + mnCartId.ToString() + " and cContactType = '" + cAddressType + "'", "tblCartContact");
                        if (oDs2.Tables["tblCartContact"].Rows.Count > 0)
                        {
                            oXform.Instance.SelectSingleNode("cContactId").InnerText = Convert.ToString(oDs2.Tables["tblCartContact"].Rows[0]["nContactKey"]);
                        }

                        oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("cIsDelivery"));
                        oXform.submission("contact", mcPagePath + cCmdType + "=" + cCmdAction, "POST");

                        oGrpElmt = oXform.addGroup(ref oXform.moXformElmt, "address", sLabel: "");

                        oXform.addInput(ref oGrpElmt, "addType", false, "", "hidden");
                        oGrpElmt.LastChild.AppendChild(oGrpElmt.OwnerDocument.CreateElement("value"));
                        oGrpElmt.LastChild.FirstChild.InnerText = cAddressType;

                        // oXform.addSelect1(oGrpElmt, "cContactId", True, "Select", "multiline", xForm.ApperanceTypes.Full)
                        // oXform.addBind("cContactId", "cContactId")

                        // Add Collection Options

                        if (moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                        {
                            // Add Collection options
                            // Get the collection delivery options
                            // Dim oDrCollectionOptions As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where bCollection = 1")
                            using (var oDrCollectionOptions = moDBHelper.getDataReaderDisposable("select * from tblCartShippingMethods where bCollection = 1"))  // Done by nita on 6/7/22
                            {
                                if (oDrCollectionOptions.HasRows)
                                {
                                    XmlElement oCollectionGrp;
                                    oCollectionGrp = oXform.addGroup(ref oGrpElmt, "CollectionOptions", "collection-options", "");

                                    while (oDrCollectionOptions.Read())
                                    {

                                        string OptLabel = oDrCollectionOptions["cShipOptName"].ToString() + " - " + oDrCollectionOptions["cShipOptCarrier"].ToString();

                                        oXform.addSubmit(ref oCollectionGrp, "collect", OptLabel, "CollectionID_" + oDrCollectionOptions["nShipOptKey"].ToString(), "collect btn-primary principle", "fa-truck");
                                    }
                                }
                            }
                        }

                        foreach (DataRow currentODr1 in oDs.Tables["tblCartContact"].Rows)
                        {
                            oDr = currentODr1;
                            XmlElement oAddressGrp;
                            oAddressGrp = oXform.addGroup(ref oGrpElmt, "addressGrp-" + oDr["nContactKey"].ToString(), "addressGrp", "");
                            oXform.addDiv(ref oAddressGrp, moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, Convert.ToInt64(oDr["nContactKey"])), "pickAddress");

                            if (billingAddId == null || !billingAddId.Equals(oDr["nContactKey"]))
                            {
                                oXform.addSubmit(ref oAddressGrp, "editAddress", "Edit", "cartDeleditAddress" + oDr["nContactKey"].ToString(), "btn-default edit", "fa-pencil");
                                oXform.addSubmit(ref oAddressGrp, "removeAddress", "Del", submitPrefix + "deleteAddress" + oDr["nContactKey"].ToString(), "btn-default delete", "fa-solid fa-trash");
                            }
                            else
                            {
                                oXform.addSubmit(ref oAddressGrp, "editAddress", "Edit", submitPrefix + "editAddress" + oDr["nContactKey"].ToString(), "btn-default edit", "fa-pencil");
                                // oXform.addSubmit(oAddressGrp, "removeAddress", "Delete", submitPrefix & "deleteAddress" & oDr.Item("nContactKey"), "delete")
                            }

                            if (!"Billing Address".Equals(oDr["cContactType"]?.ToString()))
                            {
                                oXform.addSubmit(ref oAddressGrp, Convert.ToString(oDr["nContactKey"]), "Use as Billing", submitPrefix + "useBilling" + oDr["nContactKey"].ToString(), "btn-default setAsBilling", "fa-solid fa-credit-card");
                            }
                            else
                            {

                                if (mbNoDeliveryAddress)
                                {
                                    oXform.addSubmit(ref oAddressGrp, "addNewAddress", "Add New Address", submitPrefix + "addNewAddress", "btn-default addnew", "fa-plus");
                                }
                                else
                                {
                                    oXform.addSubmit(ref oAddressGrp, "addNewAddress", "Add New Billing Address", submitPrefix + "addNewAddress", "btn-default addnew", "fa-plus");
                                }

                                if (mbNoDeliveryAddress == false)
                                {
                                    oXform.addSubmit(ref oGrpElmt, Convert.ToString(oDr["nContactKey"]), "New Delivery Address", submitPrefix + "addDelivery" + oDr["nContactKey"].ToString(), "setAsBilling btn-custom principle", "fa-plus");
                                }

                            }

                            if (mbNoDeliveryAddress)
                            {
                                oXform.addSubmit(ref oAddressGrp, Convert.ToString(oDr["nContactKey"]), "Use This Address", submitPrefix + "contact" + oDr["nContactKey"].ToString(), "deliver-here principle", "fas fa-truck");
                            }
                            else
                            {
                                oXform.addSubmit(ref oAddressGrp, Convert.ToString(oDr["nContactKey"]), "Deliver To This Address", submitPrefix + "contact" + oDr["nContactKey"].ToString(), "deliver-here principle", "fas fa-truck");
                            }
                        }
                        // Check if the form has been submitted
                        if (oXform.isSubmitted())
                        {
                            oXform.updateInstanceFromRequest();
                            // bool forCollection = false;
                            if (moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                            {
                                object bCollectionSelected = false;
                                // Dim oDrCollectionOptions As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where bCollection = 1")
                                using (var oDrCollectionOptions = moDBHelper.getDataReaderDisposable("select * from tblCartShippingMethods where bCollection = 1"))  // Done by nita on 6/7/22
                                {
                                    if (oDrCollectionOptions.HasRows)
                                    {
                                        while (oDrCollectionOptions.Read())
                                        {
                                            if (!string.IsNullOrEmpty(myWeb.moRequest["CollectionID_" + oDrCollectionOptions["nShipOptKey"].ToString()]))
                                            {
                                                bCollectionSelected = true;
                                                // Set the shipping option
                                                string cShippingDesc = oDrCollectionOptions["cShipOptName"].ToString() + "-" + oDrCollectionOptions["cShipOptCarrier"].ToString();
                                                double nShippingCost = 0;
                                                if (oDrCollectionOptions["nShipOptCost"] != DBNull.Value)
                                                {
                                                    double.TryParse(oDrCollectionOptions["nShipOptCost"].ToString(), out nShippingCost);
                                                }
                                                string cSqlUpdate;
                                                //cSqlUpdate = Convert.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE tblCartOrder SET cShippingDesc='", SqlFmt(cShippingDesc)), "', nShippingCost="), SqlFmt(nShippingCost.ToString())), ", nShippingMethodId = "), oDrCollectionOptions["nShipOptKey"]), " WHERE nCartOrderKey="), mnCartId));
                                                cSqlUpdate = $@"UPDATE tblCartOrder  SET cShippingDesc = '{SqlFmt(cShippingDesc)}',nShippingCost = {SqlFmt(nShippingCost.ToString())}, nShippingMethodId = {oDrCollectionOptions["nShipOptKey"]} WHERE nCartOrderKey = {mnCartId}";
                                                moDBHelper.ExeProcessSql(cSqlUpdate);
                                                // forCollection = true;
                                                oXform.valid = true;
                                                oContactXform.valid = true;
                                                mbNoDeliveryAddress = true;

                                                var NewInstance = moPageXml.CreateElement("instance");
                                                var delXform = contactXform("Delivery Address");

                                                NewInstance.InnerXml = delXform.Instance.SelectSingleNode("tblCartContact").OuterXml;
                                                // dissassciate from user so not shown again
                                                NewInstance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = "";
                                                NewInstance.SelectSingleNode("tblCartContact/cContactName").InnerText = oDrCollectionOptions["cShipOptName"].ToString();
                                                NewInstance.SelectSingleNode("tblCartContact/cContactCompany").InnerText = oDrCollectionOptions["cShipOptCarrier"].ToString();
                                                NewInstance.SelectSingleNode("tblCartContact/cContactCountry").InnerText = moCartConfig["DefaultDeliveryCountry"];

                                                string billingContactXml = null;
                                                string collectionContactID;

                                                collectionContactID = moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, NewInstance);

                                                useSavedAddressesOnCart(billingAddId, Convert.ToInt64(collectionContactID), billingContactXml);
                                                return oReturnForm;
                                            }
                                        }
                                        if (!Convert.ToBoolean(bCollectionSelected))
                                        {
                                            RemoveDeliveryOption(mnCartId);
                                        }
                                    }
                                }
                            }

                            foreach (DataRow currentODr2 in oDs.Tables["tblCartContact"].Rows)
                            {
                                oDr = currentODr2;
                                if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "contact" + oDr["nContactKey"].ToString()]))
                                {
                                    contactId = Convert.ToInt64(oDr["nContactKey"]);
                                    // Save Behaviour
                                    oXform.valid = true;
                                }
                                else if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addDelivery" + oDr["nContactKey"].ToString()]))
                                {
                                    contactId = Convert.ToInt64(oDr["nContactKey"]);
                                    oXform.valid = false;
                                    oContactXform.valid = false;
                                }
                                else if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + oDr["nContactKey"].ToString()]))
                                {
                                    contactId = Convert.ToInt64(oDr["nContactKey"]);
                                }
                                // edit Behavior
                                else if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "deleteAddress" + oDr["nContactKey"].ToString()]))
                                {
                                    contactId = Convert.ToInt64(oDr["nContactKey"]);
                                    // delete Behavior
                                    moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, contactId);
                                    // remove from form
                                    oXform.moXformElmt.SelectSingleNode("descendant-or-self::group[@ref='addressGrp-" + contactId + "']").ParentNode.RemoveChild(oXform.moXformElmt.SelectSingleNode("descendant-or-self::group[@ref='addressGrp-" + contactId + "']"));
                                    oXform.valid = false;
                                }
                                else if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "useBilling" + oDr["nContactKey"].ToString()]))
                                {
                                    // we have handled this at the top
                                    oXform.valid = false;
                                }
                            }

                            // Check if the contactID is populated
                            if (contactId == 0L)
                            {
                                oXform.addNote("address", Protean.xForm.noteTypes.Alert, "You must select an address from the list");
                            }
                            else
                            {

                                // Get the selected address
                                DataRow[] oMatches = oDs.Tables["tblCartContact"].Select("nContactKey = " + contactId);
                                if (oMatches != null)
                                {
                                    var oMR = oMatches[0];

                                    if (!bDontPopulate)
                                    {

                                        // Update the contactXform with the address
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText = newAddressType;
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactName").InnerText = oMR["cContactName"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactCompany").InnerText = oMR["cContactCompany"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactAddress").InnerText = oMR["cContactAddress"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactCity").InnerText = oMR["cContactCity"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactState").InnerText = oMR["cContactState"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactZip").InnerText = oMR["cContactZip"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText = oMR["cContactCountry"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactTel").InnerText = oMR["cContactTel"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactFax").InnerText = oMR["cContactFax"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactEmail").InnerText = oMR["cContactEmail"] + "".ToString();
                                        oContactXform.Instance.SelectSingleNode("cIsDelivery").InnerText = oXform.Instance.SelectSingleNode("cIsDelivery").InnerText;

                                        oContactXform.resetXFormUI();
                                        oContactXform.addValues();

                                        // Add hidden values for the parent address
                                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + contactId]))
                                        {

                                            oGrpElmt = (XmlElement)oContactXform.moXformElmt.SelectSingleNode("group");
                                            oContactXform.addInput(ref oGrpElmt, "userAddId", false, "", "hidden");
                                            oGrpElmt.LastChild.AppendChild(oGrpElmt.OwnerDocument.CreateElement("value"));
                                            oGrpElmt.LastChild.FirstChild.InnerText = contactId.ToString(); // oMR.Item("nContactKey")

                                            oContactXform.addInput(ref oGrpElmt, "userAddType", false, "", "hidden");
                                            oGrpElmt.LastChild.AppendChild(oGrpElmt.OwnerDocument.CreateElement("value"));
                                            oGrpElmt.LastChild.FirstChild.InnerText = newAddressType; // oMR.Item("cContactType")

                                        }
                                    }
                                }
                            }
                        }

                        oXform.addValues();

                    }

                    if (oContactXform.valid == false & oXform.valid == false)
                    {
                        // both forms are invalid so we need to output one of the forms.
                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + contactId]) | !string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addDelivery" + contactId]) | oContactXform.isSubmitted())
                        {
                            // we are editing an address so show the contactXform or a contactXform has been submitted to 
                            oReturnForm = oContactXform;
                        }
                        // We need to show the pick list if and only if :
                        // 1. It has addresses in it
                        // 2. There is no request to Add

                        else if (oXform.moXformElmt.InnerXml.ToString().Contains("addNewAddress") & !!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addNewAddress"]))
                        {
                            oReturnForm = oXform;
                        }
                        else
                        {
                            // Add address needs to clear out the existing xForm
                            if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addNewAddress"]))
                            {
                                oContactXform.resetXFormUI();
                                oContactXform.addValues();
                            }
                            oReturnForm = oContactXform;
                        }
                    }
                    else
                    {
                        // If pick address has been submitted, then we have a contactXform that has not been submitted, and therefore not saved.  Let's save it.
                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "contact" + contactId]))
                        {
                            string billingContactXml = null;
                            if (!string.IsNullOrEmpty(cPickAddressXform))
                            {
                                billingContactXml = oXform.Instance.SelectSingleNode("tblCartContact/cContactXml").InnerXml;
                            }

                            useSavedAddressesOnCart(billingAddId, contactId, billingContactXml);
                            // skip delivery
                            oContactXform.moXformElmt.SetAttribute("cartCmd", "ChoosePaymentShippingOption");
                        }

                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addDelivery" + contactId]))
                        {
                            // remove the deliver from the instance if it is there
                            useSavedAddressesOnCart(billingAddId, 0L, null);
                            foreach (XmlNode oNode in oContactXform.Instance.SelectNodes("tblCartContact[cContactType='Delivery Address']"))
                                oNode.ParentNode.RemoveChild(oNode);
                        }

                        if (oContactXform.valid == false)
                        {
                            oContactXform.valid = true;
                            oContactXform.moXformElmt.SetAttribute("persistAddress", "false");
                        }

                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + billingAddId]) & oContactXform.valid)
                        {
                            // We have edited a billing address and need to output the pickForm
                            oReturnForm = oXform;
                        }
                        else
                        {
                            // pass through the xform to make transparent
                            oReturnForm = oContactXform;
                        }
                    }

                    // TS not sure if required after rewrite, think it is deleting addresses unessesarily.

                    // If Not (oReturnForm Is Nothing) AndAlso oReturnForm.valid AndAlso oReturnForm.isSubmitted Then
                    // ' There seems to be an issue with duplicate addresses being submitted by type against an order.
                    // '  This script finds the duplciates and nullifies them (i.e. sets their cartid to be 0).
                    // cSql = "UPDATE tblCartContact SET nContactCartId = 0 " _
                    // & "FROM (SELECT nContactCartId id, cContactType type, MAX(nContactKey) As latest FROM dbo.tblCartContact WHERE nContactCartId <> 0 GROUP BY nContactCartId, cContactType HAVING COUNT(*) >1) dup " _
                    // & "INNER JOIN tblCartContact c ON c.nContactCartId = dup.id AND c.cContactType = dup.type AND c.nContactKey <> dup.latest"
                    // cProcessInfo = "Clear Duplicate Addresses: " & cSql
                    // moDBHelper.ExeProcessSql(cSql)
                    // End If

                    return oReturnForm;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "pickContactXform", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
                }

            }


            /// <summary>
            /// Each user need only have a single active billing address
            /// </summary>
            /// <param name="UserId"></param>
            /// <param name="ContactId"></param>
            /// <returns>If Contact ID = 0 then uses last updated</returns>
            /// <remarks></remarks>
            public long setCurrentBillingAddress(long UserId, long ContactId)
            {
                string cProcessInfo = "";
                string cSql;
                DataSet oDS;

                try
                {
                    if (myWeb.mnUserId > 0)
                    {

                        if (ContactId != 0L)
                        {
                            moDBHelper.updateInstanceField(Cms.dbHelper.objectTypes.CartContact, (int)ContactId, "cContactType", "Billing Address");
                        }

                        // Check for othersss
                        cSql = "select c.* from tblCartContact c inner JOIN tblAudit a on a.nAuditKey = c.nAuditId where nContactDirId = " + myWeb.mnUserId.ToString() + " and nContactCartId = 0  and cContactType='Billing Address' order by a.dUpdateDate DESC";
                        oDS = moDBHelper.GetDataSet(cSql, "tblCartContact");

                        foreach (DataRow oDr in oDS.Tables["tblCartContact"].Rows)
                        {
                            if (ContactId == 0L)
                            {
                                // gets the top one
                                ContactId = Convert.ToInt64(oDr["nContactKey"]);
                            }
                            if (Convert.ToInt64(oDr["nContactKey"]) != ContactId)
                            {
                                moDBHelper.ExeProcessSql("update tblCartContact set cContactType='Previous Billing Address' where nContactKey=" + oDr["nContactKey"].ToString());
                            }
                        }

                        return ContactId;
                    }

                    else
                    {
                        return 0L;
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "setCurrentBillingAddress", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                    return default;
                }
            }

            public void useSavedAddressesOnCart(long billingId, long deliveryId, string billingContactXml)
            {
                string cProcessInfo = "";
                DataSet oDs;
                try
                {
                    // get id's of addresses allready assoicated with this cart they are being replaced
                    string sSql;
                    sSql = "select nContactKey, cContactType, nAuditKey from tblCartContact inner join tblAudit a on nAuditId = a.nAuditKey where nContactCartId = " + mnCartId.ToString();
                    oDs = moDBHelper.GetDataSet(sSql, "tblCartContact");
                    string savedBillingId = "";
                    string savedDeliveryId = "";
                    string savedBillingAuditId = "";
                    string savedDeliveryAuditId = "";
                    foreach (DataRow odr in oDs.Tables["tblCartContact"].Rows)
                    {
                        if (odr["cContactType"].ToString() == "Billing Address")
                        {
                            if (!string.IsNullOrEmpty(savedBillingId))
                            {
                                // delete any duplicates
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, Convert.ToInt64(odr["nContactKey"]));
                            }
                            else
                            {
                                savedBillingId = Convert.ToString(odr["nContactKey"]);
                                savedBillingAuditId = Convert.ToString(odr["nAuditKey"]);
                            }
                        }
                        if (odr["cContactType"].ToString() == "Delivery Address")
                        {
                            if (!string.IsNullOrEmpty(savedDeliveryId))
                            {
                                // delete any duplicates
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, Convert.ToInt64(odr["nContactKey"]));
                            }
                            else
                            {
                                savedDeliveryId = Convert.ToString(odr["nContactKey"]);
                                savedDeliveryAuditId = Convert.ToString(odr["nAuditKey"]);
                            }
                        }
                    }
                    oDs = null;

                    // this should update the billing address
                    var billInstance = myWeb.moPageXml.CreateElement("Instance");
                    billInstance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, billingId);
                    billInstance.SelectSingleNode("*/nContactKey").InnerText = savedBillingId;
                    billInstance.SelectSingleNode("*/nContactCartId").InnerText = mnCartId.ToString();
                    billInstance.SelectSingleNode("*/cContactType").InnerText = "Billing Address";
                    billInstance.SelectSingleNode("*/nAuditId").InnerText = savedBillingAuditId;
                    billInstance.SelectSingleNode("*/nAuditKey").InnerText = savedBillingAuditId;
                    if (billingContactXml != null)
                    {
                        billInstance.SelectSingleNode("*/cContactXml    ").InnerXml = billingContactXml;
                    }

                    moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, billInstance);

                    // now get the submitted delivery id instance
                    if (!(deliveryId == 0L))
                    {
                        var delInstance = myWeb.moPageXml.CreateElement("Instance");
                        delInstance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, deliveryId);
                        delInstance.SelectSingleNode("*/nContactKey").InnerText = savedDeliveryId;
                        delInstance.SelectSingleNode("*/nContactCartId").InnerText = mnCartId.ToString();
                        delInstance.SelectSingleNode("*/cContactType").InnerText = "Delivery Address";
                        delInstance.SelectSingleNode("*/nAuditId").InnerText = savedDeliveryAuditId;
                        delInstance.SelectSingleNode("*/nAuditKey").InnerText = savedDeliveryAuditId;
                        moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, delInstance);
                    }

                    RemoveDeliveryOption(mnCartId);
                }

                // here we should update the current instance so we can calculate the shipping later

            

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "useAddressesOnCart", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }
            }

            protected void UpdateExistingUserAddress(ref Cms.xForm oContactXform)
            {
                myWeb.PerfMon.Log("Cart", "UpdateExistingUserAddress");
                // Check if it exists - if it does then update the nContactKey node
                var oTempCXform = new Cms.xForm(ref myWeb.msException);
                string cProcessInfo = "";
                string sSql;
                long nCount;
                XmlElement oElmt2;
                try
                {

                    foreach (XmlElement oAddElmt in oContactXform.Instance.SelectNodes("tblCartContact"))
                    {
                        if (oAddElmt.GetAttribute("saveToUser") != "false")
                        {

                            // does this address allready exist?
                            //sSql = Convert.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("select count(nContactKey) from tblCartContact where nContactDirId = " + myWeb.mnUserId + " and nContactCartId = 0 " + " and cContactName = '", SqlFmt(oAddElmt.SelectSingleNode("cContactName").InnerText)), "'"), " and cContactCompany = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactCompany").InnerText)), "'"), " and cContactAddress = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactAddress").InnerText)), "'"), " and cContactCity = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactCity").InnerText)), "'"), " and cContactState = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactState").InnerText)), "'"), " and cContactZip = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactZip").InnerText)), "'"), " and cContactCountry = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactCountry").InnerText)), "'"), " and cContactTel = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactTel").InnerText)), "'"), " and cContactFax = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactFax").InnerText)), "'"), " and cContactEmail = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactEmail").InnerText)), "'"), " and cContactXml = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactXml").InnerXml)), "'"));
                            sSql = $@"
    SELECT COUNT(nContactKey) 
    FROM tblCartContact
    WHERE nContactDirId = {myWeb.mnUserId} 
      AND nContactCartId = 0
      AND cContactName = '{SqlFmt(oAddElmt.SelectSingleNode("cContactName").InnerText)}'
      AND cContactCompany = '{SqlFmt(oAddElmt.SelectSingleNode("cContactCompany").InnerText)}'
      AND cContactAddress = '{SqlFmt(oAddElmt.SelectSingleNode("cContactAddress").InnerText)}'
      AND cContactCity = '{SqlFmt(oAddElmt.SelectSingleNode("cContactCity").InnerText)}'
      AND cContactState = '{SqlFmt(oAddElmt.SelectSingleNode("cContactState").InnerText)}'
      AND cContactZip = '{SqlFmt(oAddElmt.SelectSingleNode("cContactZip").InnerText)}'
      AND cContactCountry = '{SqlFmt(oAddElmt.SelectSingleNode("cContactCountry").InnerText)}'
      AND cContactTel = '{SqlFmt(oAddElmt.SelectSingleNode("cContactTel").InnerText)}'
      AND cContactFax = '{SqlFmt(oAddElmt.SelectSingleNode("cContactFax").InnerText)}'
      AND cContactEmail = '{SqlFmt(oAddElmt.SelectSingleNode("cContactEmail").InnerText)}'
      AND cContactXml = '{SqlFmt(oAddElmt.SelectSingleNode("cContactXml").InnerXml)}'";

                            nCount = Convert.ToInt64(moDBHelper.ExeProcessSqlScalar(sSql));

                            if (nCount == 0L)
                            {

                                oTempCXform.NewFrm("tblCartContact");
                                oTempCXform.Instance.InnerXml = oAddElmt.OuterXml;
                                var tempInstance = moPageXml.CreateElement("instance");
                                string ContactType = oTempCXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText;
                                // Update/add the address to the table
                                // make sure we are inserting by reseting the key

                                if (!string.IsNullOrEmpty(myWeb.moRequest["userAddId"]) & (ContactType ?? "") == (myWeb.moRequest["userAddType"] ?? ""))
                                {
                                    // get the id we are updating
                                    long updateId = Convert.ToInt64(myWeb.moRequest["userAddId"]);

                                    oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactKey").InnerText = updateId.ToString();
                                    // We need to populate the auditId feilds
                                    tempInstance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, updateId);
                                    // update with the fields specified
                                    foreach (XmlElement oElmt in oTempCXform.Instance.SelectNodes("tblCartContact/*[node()!='']"))
                                    {
                                        if (!(oElmt.Name == "nAuditId" | oElmt.Name == "nAuditKey"))
                                        {
                                            oElmt2 = (XmlElement)tempInstance.SelectSingleNode("tblCartContact/" + oElmt.Name);
                                            oElmt2.InnerXml = oElmt.InnerXml;
                                        }
                                    }
                                    oTempCXform.Instance = tempInstance;
                                }

                                else
                                {
                                    oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactKey").InnerText = "0";
                                    oTempCXform.Instance.SelectSingleNode("tblCartContact/nAuditId").InnerText = "";
                                    oTempCXform.Instance.SelectSingleNode("tblCartContact/nAuditKey").InnerText = "";
                                }

                                // separate from cart
                                oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactCartId").InnerText = "0";
                                // link to user
                                oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = myWeb.mnUserId.ToString();
                                moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oTempCXform.Instance);

                            }
                        }
                    }

                    setCurrentBillingAddress((long)myWeb.mnUserId, 0L);
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateExistingUserAddress", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }
                finally
                {
                    oTempCXform = (Cms.xForm)null;
                }
            }

        }
    }
}