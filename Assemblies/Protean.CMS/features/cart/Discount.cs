using ImageMagick;
using Lucene.Net.Search;
using Lucene.Net.Support;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.CDN;
using Protean.Providers.DiscountRule;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Web.Configuration;
using System.Windows.Controls;
using System.Xml;
using static Protean.stdTools;

namespace Protean
{

    public partial class Cms
    {
        public partial class Cart
        {
            public class Discount
            {

                private System.Collections.Specialized.NameValueCollection moCartConfig;
                private System.Collections.Specialized.NameValueCollection moConfig;
                private string mcPriceModOrder;
                private string mcUnitModOrder;
                private string mcModuleName = "Eonic.Discount";

                private bool bIsCartOn = false;
                private bool bIsQuoteOn = false;
                public Cms myWeb;

                private Cart myCart;
                private bool mbRoundUp;
                private bool mbRoundDown;
                private string mcGroups;

                public string mcCurrency;
                public bool bHasPromotionalDiscounts = false;

                private string cPromotionalDiscounts = ",";
                private string cVouchersUsed = ",";

                public enum promoCodeType
                {
                    Open = 0,
                    SingleCode = 1, // was "12N"
                    UseOnce = 2, // was "121"
                    MultiCode = 3 // to be implemented later using tblCode
                }
                public Discount()
                {
                    mcCurrency = "GBP";
                    mbRoundUp = true;
                    mbRoundDown = false;
                    mcPriceModOrder = "Basic_Money,Basic_Percent,Break_Product";
                    mcUnitModOrder = "";
                }

                public Discount(ref Cms aWeb)
                {
                    aWeb.PerfMon.Log("Discount", "New");
                    try
                    {
                        myWeb = aWeb;
                        moConfig = myWeb.moConfig;
                        moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
                        mcGroups = getGroupsByName();

                        if (myWeb.HasSession)
                        {
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["mcCurrency"], "", false)))
                            {
                                // NB 19th Feb 2010 - Caused Consultant Portal to fall over here without these
                                // additional checks?
                                if (moCartConfig != null)
                                {
                                    if (moCartConfig["currency"] != null)
                                    {
                                        myWeb.moSession["mcCurrency"] = moCartConfig["currency"];
                                    }
                                }
                            }
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["mcCurrency"], "", false)))
                                myWeb.moSession["mcCurrency"] = "GBP";

                            mcCurrency = Conversions.ToString(myWeb.moSession["mcCurrency"]);
                        }

                        if (moCartConfig != null)
                        {
                            mbRoundUp = Strings.LCase(moCartConfig["Roundup"]) == "yes" | Strings.LCase(moCartConfig["Roundup"]) == "on";
                            mbRoundDown = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(moCartConfig["Roundup"]) == "down", true, false));
                            mcPriceModOrder = moCartConfig["PriceModOrder"];
                            mcUnitModOrder = moCartConfig["UnitModOrder"];
                        }

                        bIsCartOn = Strings.LCase(moConfig["Cart"]) == "on";
                        bIsQuoteOn = Strings.LCase(moConfig["Quote"]) == "on";

                        mcModuleName = "Eonic.Discount";
                        myWeb.PerfMon.Log("Discount", "New-End");
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", "", gbDebug);
                    }
                }


                public Discount(ref Cart aCart)
                {
                    try
                    {
                        myWeb = aCart.myWeb;
                        myWeb.PerfMon.Log("Discount", "New");
                        myCart = aCart;
                        moConfig = myWeb.moConfig;
                        moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

                        mbRoundUp = myCart.mbRoundup;
                        mbRoundDown = myCart.mbRoundDown;

                        if (Strings.LCase(moConfig["Cart"]) == "on")
                        {
                            bIsCartOn = true;
                        }

                        if (Strings.LCase(moConfig["Quote"]) == "on")
                        {
                            bIsQuoteOn = true;
                        }

                        if (moCartConfig != null)
                        {
                            mcPriceModOrder = moCartConfig["PriceModOrder"];
                            mcUnitModOrder = moCartConfig["UnitModOrder"];
                        }

                        mcModuleName = "Eonic.Discount";
                        myWeb.PerfMon.Log("Discount", "New-End");
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", "", gbDebug);
                    }
                }


                #region Discount Application
                public decimal CheckDiscounts(DataSet oDsCart, XmlElement oCartXML, bool bFullCart, XmlElement oNotesElmt)
                {
                    myWeb.PerfMon.Log("Discount", "CheckDiscounts");
                    if (!bIsCartOn & !bIsQuoteOn)
                        return 0m;
                    DataSet oDsDiscounts = new DataSet();                    
                    var strSQL = new System.Text.StringBuilder();                  
                    var DiscountApplyDate = DateTime.Now;
                    // TS we should add logic here to get the invoiceDate from the xml if it exists. then we can apply historic discounts by refreshing the cartxml.
                   
                   
                    double dDisountAmount = 0d;
                    string validateShippingGroup = string.Empty;                   

                    try
                    {
                        // get cart contentIds
                        var strItemIds = new System.Text.StringBuilder();
                        foreach (XmlElement xmlCartItem in oCartXML.SelectNodes("Item"))
                        {
                            strItemIds.Append(xmlCartItem.GetAttribute("contentId"));
                            strItemIds.Append(",");
                        }
                        string cCartItemIds = strItemIds.ToString();
                        string cPromoCodeUserEntered = getUserEnteredPromoCode(ref oNotesElmt, ref oCartXML); // Check for promotional codes 
                        bool bDefaultPromoCode = false;

                        if (string.IsNullOrEmpty(cPromoCodeUserEntered))
                        {
                            strSQL.Append("select distinct cDiscountName from tblCartDiscountRules inner join tblAudit on nAuditId=nAuditKey and nStatus=1 ");
                            strSQL.Append(" and isnull(dExpireDate,getdate())>=getdate()  where isnull(cDiscountUserCode,'')=''");
                            using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(strSQL.ToString()))
                            {
                                if (oDr != null & oDr.HasRows)
                                {
                                    bDefaultPromoCode = true;
                                }
                            }
                        }
                        if (moCartConfig["CheckCartForDiscountToRequestDiscountCode"] != null)
                        {
                            if (moCartConfig["CheckCartForDiscountToRequestDiscountCode"].ToLower() == "on")
                            {
                                bDefaultPromoCode = true;
                            }
                        }
                        if (!string.IsNullOrEmpty(cCartItemIds) & (bDefaultPromoCode | !string.IsNullOrEmpty(cPromoCodeUserEntered)))
                        {
                            string cUserGroupIds = getUserGroupIDs(); // get the user groups

                            // 'comment----------
                            // ' we selecting all discounts applicable to all Items of the cart.
                            // ' we passing through the cart item ids at any promocode that has been entered
                            // ' we need to include all discount rules applicable to products by virtue of ther group memebership
                            // ' we also needs to include all discount rules that applied to all products after checking they are not in a excluded group
                            // ' we return the cart item id and discount that applied to it.
                            // ' we may have multiple discount per product 
                            // ' In future we may have multiple discount code , right now we can apply one per order.
                            // '
                            if (oCartXML.Attributes["InvoiceDateTime"] != null)
                            {
                                DiscountApplyDate = Conversions.ToDate(oCartXML.Attributes["InvoiceDateTime"].Value);
                            }

                            cCartItemIds = cCartItemIds.Remove(cCartItemIds.Length - 1);
                            if (myWeb.moDbHelper.checkTableColumnExists("tblCartDiscountRules", "bAllProductExcludeGroups"))
                            {
                                // ' call stored procedure else existing code.
                                // ' Passing parameter: cPromoCodeUserEntered,DiscountApplyDate,cUserGroupIds,nCartId
                                // ' This returns all of the discount methods that are relevant for the user, the user group, and
                                //   the product groups and the Date Range for the items in the shopping cart.
                                // ' It does not check specific quanity / amount rules for the current cart these discounts need to be filtered out later.
                                var param = new Hashtable();
                                param.Add("PromoCodeEntered", cPromoCodeUserEntered);
                                param.Add("UserGroupIds", cUserGroupIds);
                                param.Add("CartOrderId", myCart.mnCartId);
                                param.Add("CartOrderDate", DiscountApplyDate);
                                oDsDiscounts = myWeb.moDbHelper.GetDataSet("spCheckDiscounts", "Discount", "Discounts", false, param, CommandType.StoredProcedure);
                            }
                            else
                            {                               
                            }

                            // TS: Add a union in here to add discount rule applied at an order level.
                            // New method introduced to just validate xml nodes values added in cAdditionalXML.
                            // It is used for validation order total, minimum order value and  maximum order value.
                            // If promocode applied to added product in cart, and if user tried to add another product in cart, that time it will validate if total is crossing limit or not.
                            // if total crossed more or less than defined range then it will remove promocode for the user.

                            XmlDocument oXmlDiscounts = new XmlDocument();
                            XmlDocument oFinalXmlDiscount = new XmlDocument(); 
                            // TS: Move to new CheckDiscounts

                            if (oDsDiscounts != null)
                            {
                                if (oDsDiscounts.Tables["Discount"].Rows.Count > 0)
                                {
                                    oXmlDiscounts.LoadXml(oDsDiscounts.GetXml());
                                    oFinalXmlDiscount = CheckDiscounts(oXmlDiscounts.DocumentElement, oCartXML, ref cPromoCodeUserEntered);

                                    string strbFreeGiftBox = oFinalXmlDiscount.DocumentElement?.Attributes["bFreeGiftBox"]?.Value ?? "";
                                    // Code for setting default delivery option if discount code option is 'Giftbox'
                                    foreach (XmlElement oItemLoop in oFinalXmlDiscount.SelectNodes("/Discounts/Item"))
                                    {
                                        if (!string.IsNullOrEmpty(strbFreeGiftBox) & oItemLoop.SelectSingleNode("Discount") != null)
                                        {
                                            decimal AmountToDiscount = Conversions.ToDecimal(oItemLoop.SelectSingleNode("Discount").Attributes["nDiscountValue"].InnerText);
                                            myCart.updatePackagingForFreeGiftDiscount(oItemLoop.Attributes["id"].Value, AmountToDiscount);

                                            if (moConfig["GiftBoxDiscount"] != null & moConfig["GiftBoxDiscount"] == "on")
                                            {
                                                string sSql;
                                                strSQL = new System.Text.StringBuilder();
                                                DataSet oDs;
                                                sSql = "select nShippingMethodId from tblCartOrder where nCartOrderKey=" + myCart.mnCartId;
                                                oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                                                if (moConfig["eShippingMethodId"] != null & moConfig["DefaultShippingMethodId"] != null)
                                                {
                                                    if (Operators.ConditionalCompareObjectEqual(oDs.Tables[0].Rows[0]["nShippingMethodId"], moConfig["eShippingMethodId"], false))
                                                    {
                                                        myCart.updateGCgetValidShippingOptionsDS(moConfig["DefaultShippingMethodId"]);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    Protean.Providers.DiscountRule.ReturnProvider oDiscRuleProv = new Protean.Providers.DiscountRule.ReturnProvider();
                                    IdiscountRuleProvider oDisProvider = oDiscRuleProv.Get();

                                    //decimal nTotalSaved = Discount_ApplyToCart(ref oCartXML, oXmlDiscounts);
                                    decimal nTotalSaved = oDisProvider.FinalApplyToCart(ref oCartXML, oFinalXmlDiscount, ref myWeb, mbRoundUp);

                                    if (!bFullCart)
                                        oCartXML.InnerXml = "";

                                    return nTotalSaved;
                                }
                                else
                                {
                                    foreach (XmlElement oItemElmt in oCartXML.SelectNodes("Item"))
                                    {
                                        // later sites are dependant on these values
                                        oItemElmt.SetAttribute("originalPrice", Round(oItemElmt.GetAttribute("price"), bForceRoundup: mbRoundUp).ToString());
                                        oItemElmt.SetAttribute("unitSaving", 0.ToString());
                                        oItemElmt.SetAttribute("itemSaving", 0.ToString());
                                        oItemElmt.SetAttribute("discount", 0.ToString());
                                        oItemElmt.SetAttribute("itemTotal", (Conversions.ToDouble(oItemElmt.GetAttribute("price")) * Conversions.ToDouble(oItemElmt.GetAttribute("quantity"))).ToString());

                                    }
                                    oDsDiscounts = null;
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(cPromoCodeUserEntered))
                                {
                                    var oDiscountMessage = oCartXML.OwnerDocument.CreateElement("DiscountMessage");
                                    oDiscountMessage.InnerXml = "<span class=\"msg-1030\">The code you have provided is invalid for this transaction</span>";
                                    oCartXML.AppendChild(oDiscountMessage);
                                    // If promociode appiled and then it is inactive then also remove from cart also
                                    RemoveDiscountCode();
                                }
                                return 0m;
                            }
                            if (oDsDiscounts is null)
                            {
                                
                            }
                            else
                            {
                                #region Old Code

                                //// now need to make sure there are no duplicates where multi groups exists
                                //XmlElement oItemElmt;
                                //foreach (XmlElement currentOItemElmt in oXmlDiscounts.SelectNodes("Discounts/Item"))
                                //{
                                //    oItemElmt = currentOItemElmt;
                                //    int[] nDiscConts = new int[] { 0 };
                                //    string cDiscConts = ",";
                                //    foreach (XmlElement oDupElmt in oItemElmt.SelectNodes("Discount"))
                                //    {

                                //        if (cDiscConts.Contains("," + oDupElmt.GetAttribute("nDiscountKey") + ","))
                                //        {
                                //            oItemElmt.RemoveChild(oDupElmt);
                                //        }
                                //        else
                                //        {
                                //            cDiscConts += oDupElmt.GetAttribute("nDiscountKey") + ",";
                                //        }

                                //    }
                                //}

                                //// Itterate through those that have a cDiscountUserCode
                                //foreach (XmlElement currentOItemElmt1 in oXmlDiscounts.SelectNodes("Discounts/Item/Discount[@cDiscountUserCode!='' or @nDiscountCodeType='3']"))
                                //{
                                //    oItemElmt = currentOItemElmt1;
                                //    bHasPromotionalDiscounts = true;

                                //    string cDiscountUserCode = oItemElmt.GetAttribute("cDiscountUserCode").ToLower();
                                //    promoCodeType nDiscountCodeType = (promoCodeType)Conversions.ToInteger(oItemElmt.GetAttribute("nDiscountCodeType").ToLower());

                                //    if (nDiscountCodeType == promoCodeType.MultiCode)
                                //    {
                                //        if (string.IsNullOrEmpty(cPromoCodeUserEntered))
                                //        {
                                //            oItemElmt.ParentNode.RemoveChild(oItemElmt);
                                //        }
                                //        else
                                //        {
                                //            // do nothing we will process this rule because it matches the incoming query which contains the code.
                                //        }
                                //    }
                                //    else if (!((cDiscountUserCode ?? "") == (cPromoCodeUserEntered.ToLower() ?? "")))
                                //    {
                                //        oItemElmt.ParentNode.RemoveChild(oItemElmt);
                                //    }
                                //    else if (nDiscountCodeType == promoCodeType.UseOnce)
                                //    {
                                //        if (!cPromotionalDiscounts.Contains("," + oItemElmt.GetAttribute("nDiscountKey") + ","))
                                //        {
                                //            cPromotionalDiscounts += oItemElmt.GetAttribute("nDiscountKey") + ",";
                                //        }
                                //    }

                                //}

                                //foreach (XmlElement currentOItemElmt2 in oXmlDiscounts.SelectNodes("Discounts/Item/Discount[@CodeUsedId!='']"))
                                //{
                                //    oItemElmt = currentOItemElmt2;

                                //    cVouchersUsed += oItemElmt.GetAttribute("CodeUsedId") + ",";

                                //}

                                //// Price Modifiers
                                //string[] cPriceModifiers = new string[] { "Basic_Money", "Basic_Percent", "Break_Product" };

                                //if (!string.IsNullOrEmpty(mcPriceModOrder))
                                //    cPriceModifiers = Strings.Split(mcPriceModOrder, ",");
                                //string strcFreeShippingMethods = "";
                                //string strbFreeGiftBox = "";

                                //if (oDsDiscounts != null)
                                //{
                                //    strcFreeShippingMethods = "";
                                //    var doc = new XmlDocument();
                                //    bool ProductGroups = Conversions.ToBoolean(1);

                                //    if (!string.IsNullOrEmpty(cPromoCodeUserEntered))
                                //    {
                                //        // getting productgroups value
                                //        strSQL.Clear();
                                //        strSQL.Append("Select cAdditionalXML From tblCartDiscountRules Where cDiscountUserCode = '" + cPromoCodeUserEntered + "'");

                                //        oDsDiscounts = myWeb.moDbHelper.GetDataSet(strSQL.ToString(), "Discount", "Discounts");
                                //        if (oDsDiscounts.Tables["Discount"].Rows.Count > 0)
                                //        {
                                //            string additionalInfo = Conversions.ToString(Operators.AddObject(Operators.AddObject("<additionalXml>", oDsDiscounts.Tables["Discount"].Rows[0]["cAdditionalXML"]), "</additionalXml>"));
                                //            doc.LoadXml(additionalInfo);

                                //            if (doc.InnerXml.Contains("cFreeShippingMethods"))
                                //            {
                                //                if (!string.IsNullOrEmpty(doc.SelectSingleNode("additionalXml").SelectSingleNode("cFreeShippingMethods").InnerText))
                                //                {
                                //                    strcFreeShippingMethods = doc.SelectSingleNode("additionalXml").SelectSingleNode("cFreeShippingMethods").InnerText;
                                //                    // Initializing the attribute NonDiscountedShippingCost which will get update once promocode applied
                                //                    oCartXML.SetAttribute("NonDiscountedShippingCost", "0");
                                //                    oCartXML.SetAttribute("freeShippingMethods", strcFreeShippingMethods);
                                //                }
                                //            }
                                //            if (doc.InnerXml.Contains("bFreeGiftBox"))
                                //            {
                                //                strbFreeGiftBox = doc.SelectSingleNode("additionalXml").SelectSingleNode("bFreeGiftBox").InnerText;
                                //                // If strbFreeGiftBox = "True" Then
                                //                // oCartXML.SetAttribute("bFreeGiftBox", strbFreeGiftBox)
                                //                // End If
                                //            }
                                //        }

                                //    }

                                //}


                                //int nPriceCount = 0; // this counts where we are on the prices, shows the order we done them in
                                //var loopTo1 = Information.UBound(cPriceModifiers);
                                //for (nCount = 0; nCount <= loopTo1; nCount++)
                                //{
                                //    switch (cPriceModifiers[nCount] ?? "")
                                //    {
                                //        case "Basic_Money":
                                //            {
                                //                Discount_Basic_Money(ref oXmlDiscounts, ref nPriceCount, ref strcFreeShippingMethods, ref strbFreeGiftBox);
                                //                break;
                                //            }
                                //        case "Basic_Percent":
                                //            {
                                //                Discount_Basic_Percent(ref oXmlDiscounts, ref nPriceCount, ref strcFreeShippingMethods);
                                //                break;
                                //            }
                                //        case "Break_Product":
                                //            {
                                //                Discount_Break_Product(ref oXmlDiscounts, ref nPriceCount);
                                //                break;
                                //            }
                                //    }
                                //}
                                //// these  need to be ordered since they are dependant
                                //// on each other
                                //Discount_XForPriceY(ref oXmlDiscounts, ref nPriceCount);
                                //Discount_CheapestDiscount(ref oXmlDiscounts, ref nPriceCount);
                                //Discount_Break_Group(ref oXmlDiscounts, ref nPriceCount);

                                #endregion Old Code                                
                            }
                        }
                        // TS: END Move to new CheckDiscounts, below code is else part for if CartItemIds is empty or PromoCodeUserEntered is empty

                        // 'code to validate exchange functionality
                        else if (string.IsNullOrEmpty(cCartItemIds) & !string.IsNullOrEmpty(cPromoCodeUserEntered))
                        {

                            strSQL.Append(" SELECT tblCartDiscountRules.cDiscountCode, tblCartDiscountRules.bDiscountIsPercent, ");
                            strSQL.Append("tblCartDiscountRules.nDiscountCompoundBehaviour, tblCartDiscountRules.nDiscountValue from tblCartDiscountRules where cDiscountCode='" + cPromoCodeUserEntered + "'");
                            // oDsDiscounts = myWeb.moDbHelper.GetDataSet(strSQL.ToString, "Discount", "Discounts")
                            // dDisountAmount = oDsDiscounts.Tables("Discount").Rows(0)("nDiscountValue")
                            using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(strSQL.ToString()))
                            {
                                if (oDr.HasRows)
                                {
                                    while (oDr.Read())
                                        dDisountAmount = Conversions.ToDouble(oDr["nDiscountValue"]);
                                }
                            }
                            return (decimal)dDisountAmount;
                        }
                        else
                        {
                            foreach (XmlElement oItemElmt in oCartXML.SelectNodes("Item"))
                            {
                                // later sites are dependant on these values
                                oItemElmt.SetAttribute("originalPrice", Round(oItemElmt.GetAttribute("price"), bForceRoundup: mbRoundUp).ToString());
                                oItemElmt.SetAttribute("unitSaving", 0.ToString());
                                oItemElmt.SetAttribute("itemSaving", 0.ToString());
                                oItemElmt.SetAttribute("discount", 0.ToString());
                                oItemElmt.SetAttribute("itemTotal", (Conversions.ToDouble(oItemElmt.GetAttribute("price")) * Conversions.ToDouble(oItemElmt.GetAttribute("quantity"))).ToString());

                            }
                            return 0m;
                        }                        
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckDiscounts", ex, "", "", gbDebug);
                    }
                    finally
                    {
                    }
                    return default;
                }



                public XmlDocument CheckDiscounts(XmlElement oXmlDiscounts, XmlElement oCartXML, ref string AppliedCode)
                {
                    try {
                        bool isApplicable = false;
                        int ProviderType = 0;
                        int nPromocodeApplyFlag = 0;
                        //ProviderType = Convert.ToInt32(discountNode.SelectSingleNode("nDiscountCodeType")?.InnerText ?? "0");
                        Protean.Providers.DiscountRule.ReturnProvider oDiscRuleProv = new Protean.Providers.DiscountRule.ReturnProvider();
                        IdiscountRuleProvider oDisProvider = oDiscRuleProv.Get();

                        // Loop through the oDiscountXml to get the provider Type and run checkApplicable
                        // Create a fresh XML doc for final output
                        XmlDocument oFinalDiscounts = new XmlDocument();
                        XmlElement root = oFinalDiscounts.CreateElement("Discounts");
                        oFinalDiscounts.AppendChild(root);
                        XmlNodeList discountNodes = oXmlDiscounts.SelectNodes("//Discount");
                        foreach (XmlNode discountNode in discountNodes)
                        {                           
                            if (oDisProvider != null)
                            {
                                isApplicable = oDisProvider.CheckDiscountApplicable(discountNode, ref oCartXML, out ProviderType);
                                if (isApplicable)
                                {                                    
                                    // Append discount to oCartXML
                                    oDisProvider.UpdateCartXMLwithDiscounts(discountNode, oCartXML, ref oFinalDiscounts);                                   
                                }
                                else
                                {
                                    discountNode.ParentNode.RemoveChild(discountNode); // remove invalid discounts
                                }
                            }                            
                        }
                        // If we have a valid discount code, we can apply it to the cart
                        // Based on providerType 1,2,3... get the actual provider
                        IdiscountRuleProvider ApplicableProviderType = oDiscRuleProv.Get(ProviderType);
                        if (ApplicableProviderType != null)
                        {
                            // now need to make sure there are no duplicates where multi groups exists
                            XmlElement oItemElmt;
                            foreach (XmlElement currentOItemElmt in oFinalDiscounts.SelectNodes("/Discounts/Item"))
                            {
                                oItemElmt = currentOItemElmt;
                                int[] nDiscConts = new int[] { 0 };
                                string cDiscConts = ",";
                                foreach (XmlElement oDupElmt in oItemElmt.SelectNodes("Discount"))
                                {
                                    if (cDiscConts.Contains("," + oDupElmt.GetAttribute("nDiscountKey") + ","))
                                    {
                                        oItemElmt.RemoveChild(oDupElmt);
                                    }
                                    else
                                    {
                                        cDiscConts += oDupElmt.GetAttribute("nDiscountKey") + ",";
                                    }
                                }
                            }

                            // Itterate through those that have a cDiscountUserCode
                            foreach (XmlElement currentOItemElmt1 in oFinalDiscounts.SelectNodes("/Discounts/Item/Discount[@cDiscountUserCode!='' or @nDiscountCodeType='3']"))
                            {
                                oItemElmt = currentOItemElmt1;
                                bHasPromotionalDiscounts = true;
                                string cDiscountUserCode = oItemElmt.GetAttribute("cDiscountUserCode").ToLower();
                                promoCodeType nDiscountCodeType = (promoCodeType)Conversions.ToInteger(oItemElmt.GetAttribute("nDiscountCodeType").ToLower());

                                if (nDiscountCodeType == promoCodeType.MultiCode)
                                {
                                    if (string.IsNullOrEmpty(AppliedCode))
                                    {
                                        oItemElmt.ParentNode.RemoveChild(oItemElmt);
                                    }
                                    else
                                    {
                                        // do nothing we will process this rule because it matches the incoming query which contains the code.
                                    }
                                }
                                //else if (!((cDiscountUserCode ?? "") == (AppliedCode.ToLower() ?? "")))
                                //{
                                //    oItemElmt.ParentNode.RemoveChild(oItemElmt);
                                //}
                                else if (nDiscountCodeType == promoCodeType.UseOnce)
                                {
                                    if (!cPromotionalDiscounts.Contains("," + oItemElmt.GetAttribute("nDiscountKey") + ","))
                                    {
                                        cPromotionalDiscounts += oItemElmt.GetAttribute("nDiscountKey") + ",";
                                    }
                                }
                            }

                            foreach (XmlElement currentOItemElmt2 in oFinalDiscounts.SelectNodes("/Discounts/Item/Discount[@CodeUsedId!='']"))
                            {
                                oItemElmt = currentOItemElmt2;
                                cVouchersUsed += oItemElmt.GetAttribute("CodeUsedId") + ",";
                            }

                            string strcFreeShippingMethods = "";
                            string strbFreeGiftBox = "";
                            XmlNodeList itemNodes = oFinalDiscounts.SelectNodes("/Discounts/Item");
                            XmlElement oElement = oFinalDiscounts.DocumentElement; // root element <Discounts>
                            if (oElement != null)
                            {
                                foreach (XmlNode itemNode in itemNodes)
                                {
                                    XmlNode itemdiscountNode = itemNode.SelectSingleNode("Discount");
                                    if (itemdiscountNode != null)
                                    {
                                        // Free Shipping Methods
                                        XmlNode freeShippingNode = itemdiscountNode.SelectSingleNode("cFreeShippingMethods");
                                        if (freeShippingNode != null && !string.IsNullOrEmpty(freeShippingNode.InnerText))
                                        {
                                            strcFreeShippingMethods = freeShippingNode.InnerText;
                                            oElement.SetAttribute("NonDiscountedShippingCost", "0");
                                            oElement.SetAttribute("freeShippingMethods", freeShippingNode.InnerText);
                                        }

                                        // Free Gift Box
                                        XmlNode freeGiftBoxNode = itemdiscountNode.SelectSingleNode("bFreeGiftBox");
                                        if (freeGiftBoxNode != null && !string.IsNullOrEmpty(freeGiftBoxNode.InnerText))
                                        {
                                            strbFreeGiftBox = freeGiftBoxNode.InnerText;
                                            oElement.SetAttribute("bFreeGiftBox", freeGiftBoxNode.InnerText);
                                        }
                                    }
                                }
                            }  

                            // Look through the oDiscountXml to apply each discount rule by providerType
                            // Price Modifiers
                            string[] cPriceModifiers = new string[] { "Basic_Money", "Basic_Percent", "Break_Product" };

                            if (!string.IsNullOrEmpty(mcPriceModOrder))
                                cPriceModifiers = Strings.Split(mcPriceModOrder, ",");
                            int nPriceCount = 0;
                            ApplicableProviderType.ApplyDiscount(ref oFinalDiscounts, ref nPriceCount, ref strcFreeShippingMethods, ref strbFreeGiftBox, mbRoundUp, ref myCart, cPriceModifiers, ref nPromocodeApplyFlag);
                            
                            // set shipping option after applying promocode
                            if (!string.IsNullOrEmpty(strcFreeShippingMethods) & nPromocodeApplyFlag == 1)
                            {
                                myCart.updateGCgetValidShippingOptionsDS(strcFreeShippingMethods);
                            }
                        }
                        
                        //updated CartXML with Discounts Applied.
                        return oFinalDiscounts;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckDiscounts", ex, "", "", gbDebug);
                    }
                    finally
                    {

                    }

                    return default;
                }

                private string getUserEnteredPromoCode(ref XmlElement xmlNotes, ref XmlElement xmlCart)
                {

                    string cPromotionalCode = "";

                    // try to get it from promocodeFromExternalRef in cart xml
                    cPromotionalCode = xmlCart.GetAttribute("promocodeFromExternalRef");

                    // try to get it from request
                    if (string.IsNullOrEmpty(cPromotionalCode))
                    {
                        cPromotionalCode = myWeb.moRequest["promocode"];
                    }

                    // try to get it from cart xml
                    if (string.IsNullOrEmpty(cPromotionalCode))
                    {
                        XmlElement oPromoElmt = (XmlElement)xmlNotes.SelectSingleNode("//Notes/PromotionalCode");
                        if (oPromoElmt != null)
                            cPromotionalCode = oPromoElmt.InnerText;
                    }

                    // try to get it from cart xform
                    if (string.IsNullOrEmpty(cPromotionalCode))
                    {
                        // Dim oXform As xForm = New xForm
                        // oXform.moPageXML = myWeb.moPageXml
                        cPromotionalCode = myWeb.moRequest["//Notes/PromotionalCode"];
                        // If Not oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode") Is Nothing Then
                        // cPromotionalCode = oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode").InnerText
                        // End If
                    }


                    return "" + cPromotionalCode;


                }

                private string getUserGroupIDs()
                {

                    int nNonAuthenticatedUsersGroupId;
                    int nAuthenticatedUsersGroupId;
                    int nAllsersGroupId = 0;
                    var strGroupIds = new System.Text.StringBuilder();
                    nNonAuthenticatedUsersGroupId = Conversions.ToInteger("0" + myWeb.moConfig["NonAuthenticatedUsersGroupId"]);
                    nAuthenticatedUsersGroupId = Conversions.ToInteger("0" + myWeb.moConfig["AuthenticatedUsersGroupId"]);

                    // start by adding the all user group, 0
                    strGroupIds.Append(nAllsersGroupId.ToString());


                    if (myWeb.mnUserId > 0)
                    {
                        // user  logged in
                        XmlNodeList xNodeList;
                        strGroupIds.Append(",");
                        // add Authenticated User Group 
                        strGroupIds.Append(nAuthenticatedUsersGroupId.ToString());

                        XmlElement xUserXml = (XmlElement)myWeb.moPageXml.SelectSingleNode("Page/User");
                        if (xUserXml is null)
                        {
                            xUserXml = myWeb.GetUserXML((long)myWeb.mnUserId);
                        }
                        // get user's groups
                        xNodeList = xUserXml.SelectNodes("Group");
                        foreach (XmlElement xmlRole in xNodeList)
                        {
                            strGroupIds.Append(",");
                            strGroupIds.Append(xmlRole.GetAttribute("id"));
                        }
                    }

                    else
                    {
                        // user not logged in
                        strGroupIds.Append(",");
                        strGroupIds.Append(nNonAuthenticatedUsersGroupId.ToString());
                    }

                    return strGroupIds.ToString();

                }

                public decimal Discount_ApplyToCart(ref XmlElement oCartXML, XmlDocument oDiscountXml)
                {
                    myWeb.PerfMon.Log("Discount", "Discount_ApplyToCart");
                    try
                    {
                        // for basic we need to loop through and apply the new price
                        // also link the discounts applied
                        // for price
                        // for units
                        XmlElement oPriceElmt;
                        // the item
                        var nDelIDs = new int[1];
                        decimal nTotalSaved = 0m;

                        // Item ID
                        var nLineTotalSaving = default(decimal);
                        foreach (XmlElement oItemElmt in oCartXML.SelectNodes("Item"))
                        {
                            int nId = Conversions.ToInteger(oItemElmt.GetAttribute("id"));
                            oPriceElmt = (XmlElement)oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/DiscountPrice");
                            if (oPriceElmt != null)
                            {

                                // NB 16/02/2010 added rounding
                                decimal nNewUnitPrice = Round(oPriceElmt.GetAttribute("UnitPrice"), bForceRoundup: mbRoundUp);
                                decimal nNewTotal = Conversions.ToDecimal(oPriceElmt.GetAttribute("Total"));
                                decimal nUnitSaving = Conversions.ToDecimal(oPriceElmt.GetAttribute("UnitSaving"));
                                nLineTotalSaving = Conversions.ToDecimal(oPriceElmt.GetAttribute("TotalSaving"));


                                // set up new attreibutes and change price
                                oItemElmt.SetAttribute("id", nId.ToString());
                                // NB 16/02/2010 added rounding
                                oItemElmt.SetAttribute("originalPrice", Round(oItemElmt.GetAttribute("price"), bForceRoundup: mbRoundUp).ToString());
                                oItemElmt.SetAttribute("price", nNewUnitPrice.ToString());
                                oItemElmt.SetAttribute("unitSaving", nUnitSaving.ToString());
                                oItemElmt.SetAttribute("itemSaving", nLineTotalSaving.ToString());
                                oItemElmt.SetAttribute("itemTotal", nNewTotal.ToString());

                                // this will change
                                nTotalSaved += nLineTotalSaving;



                                // now to add the discount items to the cart
                                foreach (XmlElement oDiscountElmt in oDiscountXml.SelectNodes("Discounts/Item[@id=" + nId + "]/Discount[((@nDiscountCat=1 or @nDiscountCat=2) and @Applied=1) or (@nDiscountCat=3) or (@nDiscountCat=5)]"))
                                {
                                    oDiscountElmt.SetAttribute("AppliedToCart", 1.ToString());
                                    oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountElmt.CloneNode(true), true));
                                }
                                XmlElement oTmpElmt = (XmlElement)oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/DiscountPrice");
                                oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oTmpElmt.CloneNode(true), true));
                            }

                            // That was the Price Modifiers done, now for the unit modifiers (not group yet)
                            // TS: I don't really understand this next bit, if anyone does can you document it better.

                            foreach (XmlElement oDiscountItemTest in oDiscountXml.SelectNodes("Discounts/Item[@id=" + nId + "]/DiscountItem"))
                            {
                                if (Conversions.ToDecimal(oDiscountItemTest.GetAttribute("TotalSaving")) <= Conversions.ToDecimal(oItemElmt.GetAttribute("itemTotal")))
                                {
                                    oDiscountItemTest.SetAttribute("AppliedToCart", 1.ToString());
                                    oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountItemTest.CloneNode(true), true));
                                    XmlElement oDiscountInfo = (XmlElement)oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/Discount[@nDiscountKey=" + oDiscountItemTest.GetAttribute("nDiscountKey") + "]");
                                    if (oDiscountInfo != null)
                                    {
                                        oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountInfo.CloneNode(true), true));
                                    }
                                    oItemElmt.SetAttribute("itemTotal", (Conversions.ToDouble(oItemElmt.GetAttribute("itemTotal")) - Conversions.ToDouble(oDiscountItemTest.GetAttribute("TotalSaving"))).ToString());
                                    nLineTotalSaving += Conversions.ToDecimal(oDiscountItemTest.GetAttribute("TotalSaving"));
                                    nTotalSaved += Conversions.ToDecimal(oDiscountItemTest.GetAttribute("TotalSaving"));
                                }
                                else if (Information.IsNumeric(oDiscountItemTest.GetAttribute("nDiscountCat")))
                                {
                                    if (Conversions.ToDouble(oDiscountItemTest.GetAttribute("nDiscountCat")) == 4d)
                                    {
                                        if (nDelIDs[0] == 0)
                                        {
                                            nDelIDs[0] = Conversions.ToInteger(oDiscountItemTest.GetAttribute("nDiscountKey"));
                                        }
                                        else
                                        {
                                            Array.Resize(ref nDelIDs, Information.UBound(nDelIDs) + 1 + 1);
                                            nDelIDs[Information.UBound(nDelIDs)] = Conversions.ToInteger(oDiscountItemTest.GetAttribute("nDiscountKey"));
                                        }
                                    }

                                    else
                                    {

                                        // Discount greater than quanity on this line.... should give discount overall

                                        oDiscountItemTest.SetAttribute("AppliedToCart", 1.ToString());
                                        oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountItemTest.CloneNode(true), true));
                                        XmlElement oDiscountInfo = (XmlElement)oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/Discount[@nDiscountKey=" + oDiscountItemTest.GetAttribute("nDiscountKey") + "]");
                                        if (oDiscountInfo != null)
                                        {
                                            oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountInfo.CloneNode(true), true));
                                        }
                                        oItemElmt.SetAttribute("itemTotal", (Conversions.ToDouble(oItemElmt.GetAttribute("itemTotal")) - Conversions.ToDouble(oDiscountItemTest.GetAttribute("TotalSaving"))).ToString());
                                        nLineTotalSaving += Conversions.ToDecimal(oDiscountItemTest.GetAttribute("TotalSaving"));
                                        nTotalSaved += Conversions.ToDecimal(oDiscountItemTest.GetAttribute("TotalSaving"));
                                    }


                                }
                            }

                            // also need to save the total discount in the cart
                            // TS added in April 09
                            string cUpdtSQL = "UPDATE tblCartItem SET nDiscountValue = " + nLineTotalSaving + " WHERE nCartItemKey = " + nId;
                            myWeb.moDbHelper.ExeProcessSql(cUpdtSQL);

                        }
                        // End If


                        if (!(nDelIDs[0] == 0))
                        {
                            int nIX;
                            var loopTo = Information.UBound(nDelIDs);
                            for (nIX = 0; nIX <= loopTo; nIX++)
                            {
                                foreach (XmlElement nDelElmt in oCartXML.SelectNodes("descendant-or-self::DiscountItem[@nDiscountKey=" + nDelIDs[nIX] + "] | descendant-or-self::Discount[@nDiscountKey=" + nDelIDs[nIX] + "]"))
                                    nDelElmt.ParentNode.RemoveChild(nDelElmt);
                            }
                        }


                        return nTotalSaved;
                    }
                    // Thats all folks!
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "Discount_ApplyToCart", ex, "", "", gbDebug);
                    }

                    return default;
                }
                #endregion

                #region Discount Rule Application

                private void Discount_Basic_Money(ref XmlDocument oDiscountXML, ref int nPriceCount, ref string cFreeShippingMethods, [Optional, DefaultParameterValue("")] ref string strbFreeGiftBox)
                {
                    // this will work basic monetary discounts
                    myWeb.PerfMon.Log("Discount", "Discount_Basic_Money");
                    XmlElement oDiscountLoop;
                    XmlElement oPriceElmt;
                    bool bApplyOnTotal = false;
                    double RemainingAmountToDiscount = 0d;
                    try
                    {
                        // loop through the items
                        var AmountToDiscount = default(decimal);
                        foreach (XmlElement oItemLoop in oDiscountXML.SelectNodes("Discounts/Item"))
                        {
                            // check for promotional codes

                            // look for new price element, if not one, create one
                            oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("Item/DiscountPrice");
                            if (oPriceElmt is null)
                            {
                                // NB 16/02/2010
                                // Time to pull price out so we can round it, to avoid the multiple decimal place issues
                                decimal nPrice;
                                nPrice = Round(oItemLoop.GetAttribute("price"), bForceRoundup: mbRoundUp);

                                // set default attributes
                                oPriceElmt = oDiscountXML.CreateElement("DiscountPrice");
                                oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("OriginalUnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("UnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("UnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"));
                                oPriceElmt.SetAttribute("Total", ((double)nPrice * Conversions.ToDouble(oItemLoop.GetAttribute("quantity"))).ToString());
                                // oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                                oPriceElmt.SetAttribute("UnitSaving", 0.ToString());
                                oPriceElmt.SetAttribute("TotalSaving", 0.ToString());
                                oItemLoop.AppendChild(oPriceElmt);
                            }
                            // loop through the basic money discounts'
                            foreach (XmlElement currentODiscountLoop in oItemLoop.SelectNodes("Discount[@bDiscountIsPercent=0 and @nDiscountCat=1 and not(@Applied='1')]"))
                            {
                                oDiscountLoop = currentODiscountLoop;
                                // now work out new unit prices etc

                                decimal nNewPrice = Conversions.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));
                                AmountToDiscount = Conversions.ToDecimal(oDiscountLoop.GetAttribute("nDiscountValue"));
                                if (!string.IsNullOrEmpty(oDiscountLoop.GetAttribute("nDiscountRemaining")))
                                {
                                    AmountToDiscount = Conversions.ToDecimal(oDiscountLoop.GetAttribute("nDiscountRemaining"));
                                }


                                nNewPrice = (decimal)((double)nNewPrice - (double)AmountToDiscount / Conversions.ToDouble(oItemLoop.GetAttribute("quantity")));

                                if (nNewPrice > 0m & bApplyOnTotal == false) // only apply it if its not gonna go below 0
                                {

                                    var oPriceLine = oDiscountXML.CreateElement("DiscountPriceLine");

                                    // this works the price out for this discount based on previous stuff
                                    nPriceCount += 1;
                                    oPriceLine.SetAttribute("PriceOrder", nPriceCount.ToString());
                                    oPriceLine.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey"));
                                    oPriceLine.SetAttribute("UnitPrice", nNewPrice.ToString());
                                    oPriceLine.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    oPriceLine.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) - (double)nNewPrice).ToString());
                                    oPriceLine.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());

                                    oPriceElmt.AppendChild(oPriceLine);

                                    // this works the overall price
                                    oPriceElmt.SetAttribute("UnitPrice", nNewPrice.ToString());
                                    oPriceElmt.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    oPriceElmt.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("OriginalUnitPrice")) - (double)nNewPrice).ToString());

                                    // we will always apply these
                                    oPriceElmt.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    foreach (XmlElement oDiscountElmt in oDiscountXML.SelectNodes("Discounts/Item/Discount[@nDiscountKey=" + oDiscountLoop.GetAttribute("nDiscountKey") + "]"))
                                    {

                                        // set shipping option after applied promocode
                                        // If (cFreeShippingMethods <> "") Then
                                        // myCart.updateGCgetValidShippingOptionsDS(cFreeShippingMethods)
                                        // End If
                                        if (oDiscountLoop.SelectSingleNode("bApplyToOrder") != null)
                                        {
                                            if (oDiscountLoop.SelectSingleNode("bApplyToOrder").InnerText.ToString() == "True")
                                            {
                                                oDiscountElmt.SetAttribute("Applied", 1.ToString());
                                                if (AmountToDiscount == 0m)
                                                {
                                                    bApplyOnTotal = true;
                                                }
                                                else
                                                {
                                                    bApplyOnTotal = false;
                                                }
                                            }
                                        }
                                    }
                                }

                                // if apply on total is true in discount rule, set flag to true
                                // which will skip flag status to true.
                                else
                                {

                                    nNewPrice = 0m;

                                    var oPriceLine = oDiscountXML.CreateElement("DiscountPriceLine");
                                    nPriceCount += 1;
                                    oPriceLine.SetAttribute("PriceOrder", nPriceCount.ToString());
                                    oPriceLine.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey"));
                                    oPriceLine.SetAttribute("UnitPrice", nNewPrice.ToString());
                                    oPriceLine.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    oPriceLine.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("UnitPrice"));
                                    oPriceLine.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());

                                    oPriceElmt.AppendChild(oPriceLine);

                                    // this works the overall price
                                    oPriceElmt.SetAttribute("UnitPrice", nNewPrice.ToString());
                                    oPriceElmt.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    oPriceElmt.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("OriginalUnitPrice")) - (double)nNewPrice).ToString());
                                    oPriceElmt.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());

                                    // we will always apply these
                                    oDiscountLoop.SetAttribute("Applied", 1.ToString());
                                    RemainingAmountToDiscount = RemainingAmountToDiscount + Conversions.ToDouble(oPriceLine.GetAttribute("TotalSaving"));
                                    // set the discount remianing if this rule is available on other products..
                                    foreach (XmlElement oDiscountElmt in oDiscountXML.SelectNodes("Discounts/Item/Discount[@nDiscountKey=" + oDiscountLoop.GetAttribute("nDiscountKey") + "]"))
                                    {
                                        // 'oDiscountElmt.SetAttribute("nDiscountRemaining", oDiscountLoop.GetAttribute("nDiscountValue") - oPriceLine.GetAttribute("TotalSaving"))

                                        if (oDiscountLoop.SelectSingleNode("bApplyToOrder") != null)
                                        {
                                            if (oDiscountLoop.SelectSingleNode("bApplyToOrder").InnerText.ToString() == "True")
                                            {
                                                if (AmountToDiscount == 0m)
                                                {
                                                    bApplyOnTotal = true;
                                                }
                                                else
                                                {
                                                    bApplyOnTotal = false;
                                                    oDiscountElmt.SetAttribute("nDiscountRemaining", (Conversions.ToDouble(oDiscountLoop.GetAttribute("nDiscountValue")) - RemainingAmountToDiscount).ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            oDiscountElmt.SetAttribute("nDiscountRemaining", (Conversions.ToDouble(oDiscountLoop.GetAttribute("nDiscountValue")) - Conversions.ToDouble(oPriceLine.GetAttribute("TotalSaving"))).ToString());
                                        }
                                    }
                                }
                            }


                            // Code for setting default delivery option if discount code option is 'Giftbox'

                            if (!string.IsNullOrEmpty(strbFreeGiftBox) & oItemLoop.SelectSingleNode("Discount") != null)
                            {
                                myCart.updatePackagingForFreeGiftDiscount(oItemLoop.Attributes["id"].Value, AmountToDiscount);

                                if (moConfig["GiftBoxDiscount"] != null & moConfig["GiftBoxDiscount"] == "on")
                                {
                                    string sSql;
                                    var strSQL = new System.Text.StringBuilder();
                                    DataSet oDs;
                                    sSql = "select nShippingMethodId from tblCartOrder where nCartOrderKey=" + myCart.mnCartId;
                                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                                    if (moConfig["eShippingMethodId"] != null & moConfig["DefaultShippingMethodId"] != null)
                                    {
                                        if (Operators.ConditionalCompareObjectEqual(oDs.Tables[0].Rows[0]["nShippingMethodId"], moConfig["eShippingMethodId"], false))
                                        {
                                            myCart.updateGCgetValidShippingOptionsDS(moConfig["DefaultShippingMethodId"]);
                                        }
                                    }
                                }

                            }

                            // Code added for if value basic is Free Shipping then set discount amount=0 and if multiple delivery free shipping selected then 
                            // chose lowest one price
                            foreach (XmlElement currentODiscountLoop1 in oItemLoop.SelectNodes("Discount[@bDiscountIsPercent=2 and @nDiscountCat=1 and not(@Applied='1')]"))
                            {
                                oDiscountLoop = currentODiscountLoop1;
                                Discount_Basic_FreeShipping(ref oDiscountXML, ref nPriceCount, ref cFreeShippingMethods, ref strbFreeGiftBox);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "Discount_Basic_Money", ex, "", "", gbDebug);
                    }
                }

                private void Discount_Basic_FreeShipping(ref XmlDocument oDiscountXML, ref int nPriceCount, ref string cFreeShippingMethods, [Optional, DefaultParameterValue("")] ref string strbFreeGiftBox)
                {
                    // this will work basic monetary discounts
                    myWeb.PerfMon.Log("Discount", "Discount_Basic_Money");
                    XmlElement oPriceElmt;
                    bool bApplyOnTotal = false;
                    double RemainingAmountToDiscount = 0d;
                    try
                    {
                        // loop through the items
                        foreach (XmlElement oItemLoop in oDiscountXML.SelectNodes("Discounts/Item"))
                        {
                            // check for promotional codes

                            // look for new price element, if not one, create one
                            oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("Item/DiscountPrice");
                            if (oPriceElmt is null)
                            {
                                // NB 16/02/2010
                                // Time to pull price out so we can round it, to avoid the multiple decimal place issues
                                decimal nPrice;
                                nPrice = Round(oItemLoop.GetAttribute("price"), bForceRoundup: mbRoundUp);

                                // set default attributes
                                oPriceElmt = oDiscountXML.CreateElement("DiscountPrice");
                                oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("OriginalUnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("UnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("UnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"));
                                oPriceElmt.SetAttribute("Total", ((double)nPrice * Conversions.ToDouble(oItemLoop.GetAttribute("quantity"))).ToString());
                                // oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                                oPriceElmt.SetAttribute("UnitSaving", 0.ToString());
                                oPriceElmt.SetAttribute("TotalSaving", 0.ToString());
                                oItemLoop.AppendChild(oPriceElmt);
                            }

                            decimal AmountToDiscount;
                            // loop through the basic money discounts'
                            foreach (XmlElement oDiscountLoop in oItemLoop.SelectNodes("Discount[@bDiscountIsPercent=2 and @nDiscountCat=1 and not(@Applied='1')]"))
                            {
                                // now work out new unit prices etc

                                decimal nNewPrice = Conversions.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));
                                AmountToDiscount = Conversions.ToDecimal(oDiscountLoop.GetAttribute("nDiscountValue"));
                                if (!string.IsNullOrEmpty(oDiscountLoop.GetAttribute("nDiscountRemaining")))
                                {
                                    AmountToDiscount = Conversions.ToDecimal(oDiscountLoop.GetAttribute("nDiscountRemaining"));
                                }


                                nNewPrice = (decimal)((double)nNewPrice - (double)AmountToDiscount / Conversions.ToDouble(oItemLoop.GetAttribute("quantity")));

                                if (nNewPrice > 0m & bApplyOnTotal == false) // only apply it if its not gonna go below 0
                                {

                                    var oPriceLine = oDiscountXML.CreateElement("DiscountPriceLine");

                                    // this works the price out for this discount based on previous stuff
                                    nPriceCount += 1;
                                    oPriceLine.SetAttribute("PriceOrder", nPriceCount.ToString());
                                    oPriceLine.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey"));
                                    oPriceLine.SetAttribute("UnitPrice", nNewPrice.ToString());
                                    oPriceLine.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    oPriceLine.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) - (double)nNewPrice).ToString());
                                    oPriceLine.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());

                                    oPriceElmt.AppendChild(oPriceLine);

                                    // this works the overall price
                                    oPriceElmt.SetAttribute("UnitPrice", nNewPrice.ToString());
                                    oPriceElmt.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    oPriceElmt.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("OriginalUnitPrice")) - (double)nNewPrice).ToString());

                                    // we will always apply these
                                    oPriceElmt.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    foreach (XmlElement oDiscountElmt in oDiscountXML.SelectNodes("Discounts/Item/Discount[@nDiscountKey=" + oDiscountLoop.GetAttribute("nDiscountKey") + "]"))
                                    {

                                        // set shipping option after applied promocode
                                        if (!string.IsNullOrEmpty(cFreeShippingMethods))
                                        {
                                            myCart.updateGCgetValidShippingOptionsDS(cFreeShippingMethods);
                                        }
                                        if (oDiscountLoop.SelectSingleNode("bApplyToOrder") != null)
                                        {
                                            if (oDiscountLoop.SelectSingleNode("bApplyToOrder").InnerText.ToString() == "True")
                                            {
                                                oDiscountElmt.SetAttribute("Applied", 1.ToString());
                                                if (AmountToDiscount == 0m)
                                                {
                                                    bApplyOnTotal = true;
                                                }
                                                else
                                                {
                                                    bApplyOnTotal = false;
                                                }
                                            }
                                        }
                                    }
                                }

                                // if apply on total is true in discount rule, set flag to true
                                // which will skip flag status to true.
                                else
                                {

                                    nNewPrice = 0m;

                                    var oPriceLine = oDiscountXML.CreateElement("DiscountPriceLine");
                                    nPriceCount += 1;
                                    oPriceLine.SetAttribute("PriceOrder", nPriceCount.ToString());
                                    oPriceLine.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey"));
                                    oPriceLine.SetAttribute("UnitPrice", nNewPrice.ToString());
                                    oPriceLine.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    oPriceLine.SetAttribute("UnitSaving", oPriceElmt.GetAttribute("UnitPrice"));
                                    oPriceLine.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());

                                    oPriceElmt.AppendChild(oPriceLine);

                                    // this works the overall price
                                    oPriceElmt.SetAttribute("UnitPrice", nNewPrice.ToString());
                                    oPriceElmt.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    oPriceElmt.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("OriginalUnitPrice")) - (double)nNewPrice).ToString());
                                    oPriceElmt.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());

                                    // we will always apply these
                                    oDiscountLoop.SetAttribute("Applied", 1.ToString());
                                    RemainingAmountToDiscount = RemainingAmountToDiscount + Conversions.ToDouble(oPriceLine.GetAttribute("TotalSaving"));
                                    // set the discount remianing if this rule is available on other products..
                                    foreach (XmlElement oDiscountElmt in oDiscountXML.SelectNodes("Discounts/Item/Discount[@nDiscountKey=" + oDiscountLoop.GetAttribute("nDiscountKey") + "]"))
                                    {
                                        // 'oDiscountElmt.SetAttribute("nDiscountRemaining", oDiscountLoop.GetAttribute("nDiscountValue") - oPriceLine.GetAttribute("TotalSaving"))

                                        if (oDiscountLoop.SelectSingleNode("bApplyToOrder") != null)
                                        {
                                            if (oDiscountLoop.SelectSingleNode("bApplyToOrder").InnerText.ToString() == "True")
                                            {
                                                if (AmountToDiscount == 0m)
                                                {
                                                    bApplyOnTotal = true;
                                                }
                                                else
                                                {
                                                    bApplyOnTotal = false;
                                                    oDiscountElmt.SetAttribute("nDiscountRemaining", (Conversions.ToDouble(oDiscountLoop.GetAttribute("nDiscountValue")) - RemainingAmountToDiscount).ToString());
                                                }
                                            }
                                        }
                                        else
                                        {
                                            oDiscountElmt.SetAttribute("nDiscountRemaining", (Conversions.ToDouble(oDiscountLoop.GetAttribute("nDiscountValue")) - Conversions.ToDouble(oPriceLine.GetAttribute("TotalSaving"))).ToString());
                                        }
                                    }
                                }
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "Discount_Basic_Money", ex, "", "", gbDebug);
                    }
                }
                private void Discount_Break_Product(ref XmlDocument oDiscountXML, ref int nPriceCount)
                {
                    // this will work basic monetary discounts
                    myWeb.PerfMon.Log("Discount", "Discount_Break_Product");
                    XmlElement oPriceElmt;
                    try
                    {
                        // loop through the items
                        foreach (XmlElement oItemLoop in oDiscountXML.SelectNodes("Discounts/Item"))
                        {
                            // look for new price element, if not one, create one
                            oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("DiscountPrice");
                            if (oPriceElmt is null)
                            {
                                // NB 16/02/2010
                                // Time to pull price out so we can round it, to avoid the multiple decimal place issues
                                decimal nPrice;
                                nPrice = Round(oItemLoop.GetAttribute("price"), bForceRoundup: mbRoundUp);

                                // set default attributes
                                oPriceElmt = oDiscountXML.CreateElement("DiscountPrice");
                                oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("OriginalUnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("UnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("UnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"));
                                oPriceElmt.SetAttribute("Total", ((double)nPrice * Conversions.ToDouble(oItemLoop.GetAttribute("quantity"))).ToString());
                                // oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                                oPriceElmt.SetAttribute("UnitSaving", 0.ToString());
                                oPriceElmt.SetAttribute("TotalSaving", 0.ToString());
                                oItemLoop.AppendChild(oPriceElmt);

                                // Need to loop through all the breaks and get highest break
                            }
                            XmlElement oPriceBreakElmt = null;
                            XmlElement oQuantityBreakElmt = null;
                            foreach (XmlElement oTmpLoop in oItemLoop.SelectNodes("Discount[@nDiscountCat=2]"))
                            {
                                // here we go through and find the biggest discount
                                if (oPriceBreakElmt != null)
                                {
                                    if (Operators.CompareString(oTmpLoop.GetAttribute("nDiscountMinPrice"), oPriceElmt.GetAttribute("Total"), false) <= 0 & Operators.CompareString(oTmpLoop.GetAttribute("nDiscountMinPrice"), oPriceBreakElmt.GetAttribute("nDiscountMinPrice"), false) > 0)

                                        oPriceBreakElmt = oTmpLoop;
                                }
                                else if (Information.IsNumeric(oTmpLoop.GetAttribute("nDiscountMinPrice")) & Information.IsNumeric(oPriceElmt.GetAttribute("Total")))
                                {
                                    if (Conversions.ToDecimal(oTmpLoop.GetAttribute("nDiscountMinPrice")) <= Conversions.ToDecimal(oPriceElmt.GetAttribute("Total")))
                                    {
                                        oPriceBreakElmt = oTmpLoop;
                                    }
                                }

                                if (oQuantityBreakElmt != null)
                                {
                                    if (Operators.CompareString(oTmpLoop.GetAttribute("nDiscountMinQuantity"), oPriceElmt.GetAttribute("Units"), false) <= 0 & Operators.CompareString(oTmpLoop.GetAttribute("nDiscountMinQuantity"), oQuantityBreakElmt.GetAttribute("nDiscountMinQuantity"), false) > 0)

                                        oQuantityBreakElmt = oTmpLoop;
                                }
                                else if (Information.IsNumeric(oTmpLoop.GetAttribute("nDiscountMinQuantity")) & Operators.CompareString(oTmpLoop.GetAttribute("nDiscountMinQuantity"), oPriceElmt.GetAttribute("Units"), false) <= 0)
                                    oQuantityBreakElmt = oTmpLoop;
                            }

                            // which is going to be the bigger discount
                            XmlElement oTestElmt = null;
                            XmlElement oHighestElmt = null;
                            decimal nCurrentSaving = 0m;

                            long nUnits = Conversions.ToLong(oPriceElmt.GetAttribute("Units"));
                            decimal nUnitPrice = 0m;
                            decimal nTotal = 0m;
                            decimal nUnitSaving = 0m;
                            decimal nTotalSaving = 0m;


                            int nTestStatus = 0; // 0=Price, 1=Quantity
                        ResumeTest:
                            ;

                            if (nTestStatus == 2)
                                goto FinishTest;
                            if (nTestStatus == 1)
                                oTestElmt = oQuantityBreakElmt;
                            if (nTestStatus == 0)
                                oTestElmt = oPriceBreakElmt;
                            if (oTestElmt is null)
                                goto NextTestLoop;
                            // now the actual test
                            if (oTestElmt != null)
                            {
                                nUnitPrice = Conversions.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));
                                // work out depending on value/percent
                                if (Conversions.ToDouble(oTestElmt.GetAttribute("bDiscountIsPercent")) == 0d)
                                {
                                    nUnitPrice = (decimal)(nUnitPrice - Convert.ToDecimal(oTestElmt.GetAttribute("nDiscountValue")));
                                }
                                else
                                {
                                    nUnitPrice = Round((double)nUnitPrice * ((100d - Conversions.ToDouble(oTestElmt.GetAttribute("nDiscountValue"))) / 100d), bForceRoundup: mbRoundUp);
                                }
                                // make the totals
                                nTotal = nUnitPrice * nUnits;
                                nUnitSaving = (decimal)(Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) - (double)nUnitPrice);
                                nTotalSaving = (decimal)((double)nUnitSaving * Conversions.ToDouble(oPriceElmt.GetAttribute("Units")));
                                // if its higher than current we make that the item to use
                                if (nTotalSaving > nCurrentSaving)
                                {
                                    nCurrentSaving = nTotalSaving;
                                    oHighestElmt = oTestElmt;
                                }
                            }

                        NextTestLoop:
                            ;

                            nTestStatus += 1;
                            goto ResumeTest;
                        FinishTest:
                            ;

                            if (oHighestElmt is null)
                                goto NoDiscount;
                            // Dim nNewPrice As Decimal = oPriceElmt.GetAttribute("UnitPrice")
                            // nNewPrice = nNewPrice - oHighestElmt.GetAttribute("nDiscountValue")
                            // If nNewPrice > 0 Then 'only apply it if its not gonna go below 0
                            var oPriceLine = oDiscountXML.CreateElement("DiscountPriceLine");
                            // this works the overall price
                            oPriceElmt.SetAttribute("UnitPrice", nUnitPrice.ToString());
                            oPriceElmt.SetAttribute("Total", (nUnitPrice * nUnits).ToString());
                            oPriceElmt.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("OriginalUnitPrice")) - (double)nUnitPrice).ToString());
                            oPriceElmt.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                            // this works the price out for this discount based on previous stuff
                            nPriceCount += 1;
                            oPriceLine.SetAttribute("PriceOrder", nPriceCount.ToString());
                            oPriceLine.SetAttribute("nDiscountKey", oHighestElmt.GetAttribute("nDiscountKey"));
                            oPriceLine.SetAttribute("UnitPrice", nUnitPrice.ToString());
                            oPriceLine.SetAttribute("Total", (nUnitPrice * nUnits).ToString());
                            oPriceLine.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("OriginalUnitPrice")) - (double)nUnitPrice).ToString());
                            oPriceLine.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());

                            oPriceElmt.AppendChild(oPriceLine);
                            // we will always apply these
                            oHighestElmt.SetAttribute("Applied", 1.ToString());
                        // End If
                        NoDiscount:
                            ;

                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "Discount_Basic_Money", ex, "", "", gbDebug);
                    }
                }

                private void Discount_Basic_Percent(ref XmlDocument oDiscountXML, ref int nPriceCount, ref string cFreeShippingMethods)
                {
                    // this will work basic discount discounts
                    myWeb.PerfMon.Log("Discount", "Discount_Basic_Percent");
                    XmlElement oPriceElmt;
                    int nPromocodeApplyFlag = 0;
                    try
                    {
                        foreach (XmlElement oItemLoop in oDiscountXML.SelectNodes("Discounts/Item"))
                        {

                            oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("DiscountPrice");
                            if (oPriceElmt is null)
                            {
                                // NB 16/02/2010
                                // Time to pull price out so we can round it, to avoid the multiple decimal place issues
                                decimal nPrice;
                                nPrice = Round(oItemLoop.GetAttribute("price"), bForceRoundup: mbRoundUp);

                                oPriceElmt = oDiscountXML.CreateElement("Item/DiscountPrice");
                                oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("OriginalUnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("UnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("UnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"));
                                oPriceElmt.SetAttribute("Total", ((double)nPrice * Conversions.ToDouble(oItemLoop.GetAttribute("quantity"))).ToString());
                                // oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                                oPriceElmt.SetAttribute("UnitSaving", 0.ToString());
                                oPriceElmt.SetAttribute("TotalSaving", 0.ToString());
                                oItemLoop.AppendChild(oPriceElmt);
                            }
                            foreach (XmlElement oDiscountLoop in oItemLoop.SelectNodes("Discount[@bDiscountIsPercent=1 and @nDiscountCat=1]"))
                            {

                                decimal nNewPrice = Conversions.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));
                                nNewPrice = Round((double)nNewPrice * ((100d - Conversions.ToDouble(oDiscountLoop.GetAttribute("nDiscountValue"))) / 100d), bForceRoundup: mbRoundUp);

                                var oPriceLine = oDiscountXML.CreateElement("DiscountPriceLine");

                                nPriceCount += 1;
                                oPriceLine.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey"));
                                oPriceLine.SetAttribute("PriceOrder", nPriceCount.ToString());
                                oPriceLine.SetAttribute("UnitPrice", nNewPrice.ToString());
                                oPriceLine.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                oPriceLine.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) - (double)nNewPrice).ToString());
                                oPriceLine.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                oPriceElmt.AppendChild(oPriceLine);

                                // this works the overall price

                                oPriceElmt.SetAttribute("UnitPrice", nNewPrice.ToString());
                                oPriceElmt.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                oPriceElmt.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("OriginalUnitPrice")) - (double)nNewPrice).ToString());
                                oPriceElmt.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());

                                oDiscountLoop.SetAttribute("Applied", 1.ToString());
                                nPromocodeApplyFlag = 1;
                            }
                        }
                        // set shipping option after applying promocode
                        if (!string.IsNullOrEmpty(cFreeShippingMethods) & nPromocodeApplyFlag == 1)
                        {
                            myCart.updateGCgetValidShippingOptionsDS(cFreeShippingMethods);
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "Discount_Basic_Percent", ex, "", "", gbDebug);
                    }
                }

                private void Discount_XForPriceY(ref XmlDocument oDiscountXML, ref int nPriceCount)
                {
                    // this will work basic discount discounts
                    myWeb.PerfMon.Log("Discount", "Discount_XForPriceY");
                    XmlElement oPriceElmt;
                    try
                    {
                        foreach (XmlElement oItemLoop in oDiscountXML.SelectNodes("Discounts/Item"))
                        {
                            oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("DiscountPrice");
                            if (oPriceElmt is null)
                            {
                                // NB 16/02/2010
                                // Time to pull price out so we can round it, to avoid the multiple decimal place issues
                                decimal nPrice;
                                nPrice = Round(oItemLoop.GetAttribute("price"), bForceRoundup: mbRoundUp);

                                oPriceElmt = oDiscountXML.CreateElement("Item/DiscountPrice");
                                oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("OriginalUnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("UnitPrice", nPrice.ToString());
                                // oPriceElmt.SetAttribute("UnitPrice", oItemLoop.GetAttribute("price"))
                                oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"));
                                oPriceElmt.SetAttribute("Total", ((double)nPrice * Conversions.ToDouble(oItemLoop.GetAttribute("quantity"))).ToString());
                                // oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                                oPriceElmt.SetAttribute("UnitSaving", 0.ToString());
                                oPriceElmt.SetAttribute("TotalSaving", 0.ToString());
                                oItemLoop.AppendChild(oPriceElmt);
                            }

                            foreach (XmlElement oDiscountLoop in oItemLoop.SelectNodes("Discount[@nDiscountCat=3]"))
                            {
                                int nQX; // Quantity buying
                                int nQY; // For the price of
                                int nTotalQOff; // total number of units we need to deduct
                                int nQ; // Total Quanity

                                nQX = Conversions.ToInteger(oDiscountLoop.GetAttribute("nDiscountMinQuantity"));
                                nQY = Conversions.ToInteger(oDiscountLoop.GetAttribute("nDiscountValue"));
                                nQ = Conversions.ToInteger(oItemLoop.GetAttribute("quantity"));
                                int nQtotal = nQ;
                                string ItemId = oItemLoop.GetAttribute("contentId");

                                // calculate quanties of other items with same ID in the cart, BUT ONLY THE LAST ONE... SO WE APPLY THE DISCOUNT ON THE LAST ITEM
                                // If oItemLoop.SelectNodes("./preceding-sibling::Item[@contentId='" & ItemId & "']").Count > 0 And oItemLoop.SelectNodes("./following-sibling::Item[@contentId='" & ItemId & "']").Count = 0 Then
                                if (oItemLoop.SelectNodes("./preceding-sibling::Item[@contentId='" + ItemId + "' and Discount[@nDiscountCat=3 and not(@Applied='1')]]").Count > 0)
                                {
                                    foreach (XmlElement preceedingItems in oItemLoop.SelectNodes("./preceding-sibling::Item[@contentId='" + ItemId + "' and Discount[@nDiscountCat=3 and not(@Applied='1')]]"))
                                        nQtotal = (int)Math.Round(nQtotal + Conversions.ToDouble(preceedingItems.GetAttribute("quantity")));
                                }

                                nTotalQOff = (int)Math.Round(Conversions.ToDouble(Strings.Split((nQtotal / (double)nQX).ToString(), ".")[0]) * (nQX - nQY));
                                if (nTotalQOff > 0)
                                {
                                    var oDiscount = oDiscountXML.CreateElement("DiscountItem");
                                    oDiscount.SetAttribute("nDiscountKey", oDiscountLoop.GetAttribute("nDiscountKey")); // ID
                                    oDiscount.SetAttribute("nDiscountCat", 3.ToString());
                                    oDiscount.SetAttribute("oldUnits", oPriceElmt.GetAttribute("Units")); // Original Charged Units
                                    oDiscount.SetAttribute("Units", (nQ - nTotalQOff).ToString()); // Now charged Units
                                    oDiscount.SetAttribute("oldTotal", oPriceElmt.GetAttribute("Total")); // original total
                                    oDiscount.SetAttribute("Total", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) * (nQ - nTotalQOff)).ToString()); // Now total
                                    oDiscount.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("Total")) - Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) * (nQ - nTotalQOff)).ToString()); // total saving

                                    oItemLoop.AppendChild(oDiscount);
                                    // set previous items as applied...
                                    foreach (XmlElement preceedingItems in oItemLoop.SelectNodes("./preceding-sibling::Item[@contentId='" + ItemId + "' and Discount[@nDiscountCat=3 and not(@Applied='1')]]"))
                                    {
                                        foreach (XmlElement oDiscountloop2 in preceedingItems.SelectNodes("Discount[@nDiscountCat=3]"))
                                            oDiscountloop2.SetAttribute("Applied", 1.ToString());
                                    }
                                    oDiscountLoop.SetAttribute("Applied", 1.ToString());
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "Discount_xForPriceY", ex, "", "", gbDebug);
                    }
                }

                private void Discount_CheapestDiscount(ref XmlDocument oDiscXml, ref int nPriceCount)
                {

                    myWeb.PerfMon.Log("Discount", "Discount_CheapestFree");
                    // this is going to be the wierdest one
                    // we will need to loop through discounts first, then the items
                    XmlElement oItemLoop = null;
                    XmlElement oPriceElmt = null;
                    XmlElement oDiscount = null;
                    XmlElement oCurDiscount = null;
                    try
                    {
                        // need to record all the discount Ids
                        string cIDs = ",";
                        foreach (XmlElement oDiscountLoop in oDiscXml.SelectNodes("descendant-or-self::Discount[@nDiscountCat='4']"))
                        {
                            // For Each oDiscountLoop In oDiscountXML.SelectNodes("descendant-or-self::Discount[@type=4]")
                            if (!cIDs.Contains("," + oDiscountLoop.GetAttribute("nDiscountKey") + ","))
                            {
                                cIDs += oDiscountLoop.GetAttribute("nDiscountKey") + ",";
                            }
                        }
                        // now we have all the keys we can go though them and get the information we need
                        if (cIDs == ",")
                            return;
                        cIDs = Strings.Right(cIDs, cIDs.Length - 1);
                        cIDs = Strings.Left(cIDs, cIDs.Length - 1);
                        string[] oIDs = cIDs.Split(',');

                        int nMinItems = 1;
                        decimal nDiscountMaxPrice = 0m;
                        decimal nDiscountMinPrice = 0m;


                        int nI;

                        // Step through each individual discount rule
                        var loopTo = Information.UBound(oIDs);
                        for (nI = 0; nI <= loopTo; nI++)
                        {
                            //decimal nCheapestPrice = 0m; // records the cheapest price
                            //XmlElement oCheapestItem = null; // records the cheapest item
                            var oAllItems = new XmlElement[1]; // gets all the items so we dont have to do the same loop
                            decimal nValueOfItems = 0m; // records the total value of the items to check
                            int nItemQty = 0;
                            int nQualifyingItemsQty = 0;
                            decimal nDiscountValue;
                            bool bDiscountIsPercent;


                            int i = 0;

                            oCurDiscount = (XmlElement)oDiscXml.SelectSingleNode("Discounts/Item/Discount[@nDiscountKey=" + oIDs[nI] + "]");
                            nDiscountValue = Conversions.ToDecimal(oCurDiscount.GetAttribute("nDiscountValue"));
                            bDiscountIsPercent = Conversions.ToBoolean(oCurDiscount.GetAttribute("bDiscountIsPercent"));
                            // Set nMinItems
                            if (Information.IsNumeric(oCurDiscount.GetAttribute("nDiscountMinQuantity")))
                                nMinItems = Conversions.ToInteger(oCurDiscount.GetAttribute("nDiscountMinQuantity"));
                            // Set nDiscountMinPrice
                            if (Information.IsNumeric(oCurDiscount.GetAttribute("nDiscountMinPrice")))
                                nDiscountMinPrice = Conversions.ToDecimal(oCurDiscount.GetAttribute("nDiscountMinPrice"));
                            // Set nDiscountMaxPrice
                            if (Information.IsNumeric(oCurDiscount.SelectSingleNode("nDiscountMaxPrice").InnerText))
                                nDiscountMaxPrice = Conversions.ToDecimal(oCurDiscount.SelectSingleNode("nDiscountMaxPrice").InnerText);

                            var aPriceArray = new double[oDiscXml.SelectNodes("Discounts/Item[Discount/@nDiscountKey=" + oIDs[nI] + "]").Count];
                            // Step through each product in the cart associated with current rule
                            foreach (XmlElement currentOItemLoop in oDiscXml.SelectNodes("Discounts/Item[Discount/@nDiscountKey=" + oIDs[nI] + "]"))
                            {
                                oItemLoop = currentOItemLoop;
                                // bool bAllowedDiscount = false;
                                // Dim nCurrentUnitPrice As Decimal = oItemLoop.GetAttribute("price")

                                // NB 16/02/2010
                                // Time to pull price out so we can round it, to avoid the multiple decimal place issues
                                decimal nCurrentUnitPrice;
                                nCurrentUnitPrice = Round(oItemLoop.GetAttribute("price"), bForceRoundup: mbRoundUp);


                                oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("DiscountPrice");
                                if (oPriceElmt is null)
                                {
                                    oPriceElmt = oDiscXml.CreateElement("Item/DiscountPrice");
                                    oPriceElmt.SetAttribute("discountId", oIDs[nI]);
                                    oPriceElmt.SetAttribute("OriginalUnitPrice", nCurrentUnitPrice.ToString());
                                    oPriceElmt.SetAttribute("UnitPrice", nCurrentUnitPrice.ToString());
                                    oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"));
                                    oPriceElmt.SetAttribute("Total", ((double)nCurrentUnitPrice * Conversions.ToDouble(oItemLoop.GetAttribute("quantity"))).ToString());
                                    // oPriceElmt.SetAttribute("Total", oItemLoop.GetAttribute("price") * oItemLoop.GetAttribute("quantity"))
                                    oPriceElmt.SetAttribute("UnitSaving", 0.ToString());
                                    oPriceElmt.SetAttribute("TotalSaving", 0.ToString());
                                    oItemLoop.AppendChild(oPriceElmt);
                                }
                                else
                                {
                                    nCurrentUnitPrice = Conversions.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));
                                }

                                // calculate the total value of items associated with this discount
                                nValueOfItems = (decimal)(nValueOfItems + (decimal)nCurrentUnitPrice * Convert.ToDecimal(oPriceElmt.GetAttribute("Units")));
                                nItemQty = (int)Math.Round(nItemQty + Conversions.ToDouble(oPriceElmt.GetAttribute("Units")));

                                if (nCurrentUnitPrice > nDiscountMinPrice)
                                {
                                    nQualifyingItemsQty = (int)Math.Round(nQualifyingItemsQty + Conversions.ToDouble(oPriceElmt.GetAttribute("Units")));
                                }
                                // create an array of prices to discount that we can sort
                                aPriceArray[i] = (double)nCurrentUnitPrice;
                                i = i + 1;
                                // add each item into an array for sorting by price.
                            }

                            // check that anything is below value
                            if (aPriceArray != null)
                            {
                                // sort the prices to discount
                                Array.Sort(aPriceArray);
                                int itemsToDiscount = (int)Math.Round(Math.Floor(nQualifyingItemsQty / (double)nMinItems));

                                decimal nLastPrice = 0m;

                                var loopTo1 = Information.UBound(aPriceArray);
                                for (i = 0; i <= loopTo1; i++)
                                {
                                    // step through prices cheapest first
                                    if (aPriceArray[i] > (double)nLastPrice & aPriceArray[i] <= (double)nDiscountMaxPrice & itemsToDiscount > 0)
                                    {
                                        foreach (XmlElement currentOItemLoop1 in oDiscXml.SelectNodes("Discounts/Item[DiscountPrice/@UnitPrice=" + aPriceArray[i] + "]"))
                                        {
                                            oItemLoop = currentOItemLoop1;

                                            nLastPrice = Conversions.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));
                                            oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("DiscountPrice");


                                            long nDiscountNumber;
                                            if (Conversions.ToDouble(oPriceElmt.GetAttribute("Units")) > itemsToDiscount)
                                            {
                                                nDiscountNumber = itemsToDiscount;
                                            }
                                            else
                                            {
                                                nDiscountNumber = Conversions.ToLong(oPriceElmt.GetAttribute("Units"));
                                            }

                                            oDiscount = oDiscXml.CreateElement("DiscountItem");
                                            itemsToDiscount = (int)(itemsToDiscount - nDiscountNumber);

                                            // define the by Unit Discounted Price
                                            decimal nUnitPriceOriginal = Conversions.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));
                                            decimal nUnitPriceDiscounted;
                                            if (bDiscountIsPercent)
                                            {
                                                nUnitPriceDiscounted = nUnitPriceOriginal * ((100m - nDiscountValue) / 100m);
                                            }
                                            else if (nUnitPriceOriginal >= nDiscountValue)
                                            {
                                                nUnitPriceDiscounted = nUnitPriceOriginal - nDiscountValue;
                                            }
                                            else
                                            {
                                                nUnitPriceDiscounted = 0m;
                                            }



                                            // nUnitPriceDiscounted = Math.Round(nUnitPriceDiscounted, 2)
                                            nUnitPriceDiscounted = Round(nUnitPriceDiscounted, bForceRoundup: mbRoundUp);

                                            int nUnitCount = Conversions.ToInteger(oPriceElmt.GetAttribute("Units"));

                                            if (nDiscountNumber > 0L)
                                            {

                                                decimal nTotalSaving = (nUnitPriceOriginal - nUnitPriceDiscounted) * nDiscountNumber;
                                                oDiscount.SetAttribute("nDiscountKey", oIDs[nI]); // ID
                                                oDiscount.SetAttribute("nDiscountCat", 4.ToString());
                                                oDiscount.SetAttribute("oldUnits", nUnitCount.ToString()); // Original Charged Units
                                                if (nDiscountValue == 100m)
                                                {
                                                    oDiscount.SetAttribute("Units", (nUnitCount - nDiscountNumber).ToString()); // Now charged Units
                                                }
                                                else
                                                {
                                                    oDiscount.SetAttribute("Units", nUnitCount.ToString());
                                                } // Now charged Units

                                                oDiscount.SetAttribute("oldTotal", oPriceElmt.GetAttribute("Total")); // original total
                                                oDiscount.SetAttribute("Total", (Conversions.ToDouble(oPriceElmt.GetAttribute("Total")) - (double)nTotalSaving).ToString()); // Now total
                                                oDiscount.SetAttribute("TotalSaving", nTotalSaving.ToString()); // total saving
                                            }

                                            else
                                            {
                                                oDiscount.SetAttribute("nDiscountKey", oIDs[nI]); // ID
                                                oDiscount.SetAttribute("nDiscountCat", 4.ToString());
                                                oDiscount.SetAttribute("oldUnits", nUnitCount.ToString()); // Original Charged Units
                                                oDiscount.SetAttribute("Units", nUnitCount.ToString()); // Now charged Units
                                                oDiscount.SetAttribute("oldTotal", oPriceElmt.GetAttribute("Total")); // original total
                                                oDiscount.SetAttribute("Total", (nUnitPriceOriginal * nUnitCount).ToString()); // Now total
                                                oDiscount.SetAttribute("TotalSaving", 0.ToString());
                                            } // total saving
                                            oItemLoop.AppendChild(oDiscount);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "Discount_CheapestFree", ex, "", "", gbDebug);
                    }

                }

                private void Discount_Break_Group(ref XmlDocument oDiscountXML, ref int nPriceCount)
                {
                    myWeb.PerfMon.Log("Discount", "Discount_Break_Group");
                    try
                    {
                        XmlElement oDiscount;
                        var nTotalItems = default(int);
                        var nTotalItemsValue = default(decimal);
                        XmlElement oPriceElmt;
                        XmlElement oItem;

                        string cIDs = ",";
                        foreach (XmlElement currentODiscount in oDiscountXML.SelectNodes("descendant-or-self::Discount[@nDiscountCat=5]"))
                        {
                            oDiscount = currentODiscount;
                            if (!cIDs.Contains("," + oDiscount.GetAttribute("nDiscountKey") + ","))
                            {
                                cIDs += oDiscount.GetAttribute("nDiscountKey") + ",";
                            }
                        }
                        if (cIDs == ",")
                            return;
                        cIDs = Strings.Right(cIDs, cIDs.Length - 1);
                        cIDs = Strings.Left(cIDs, cIDs.Length - 1);
                        string[] oIds = Strings.Split(cIDs, ",");
                        int i = 0;
                        var loopTo = Information.UBound(oIds);
                        for (i = 0; i <= loopTo; i++)
                        {
                            foreach (XmlElement currentODiscount1 in oDiscountXML.SelectNodes("descendant-or-self::Discount[@nDiscountKey=" + oIds[i] + "]"))
                            {
                                oDiscount = currentODiscount1;
                                foreach (XmlElement currentOItem in oDiscountXML.DocumentElement.SelectNodes("Item[Discount/@nDiscountKey=" + oDiscount.GetAttribute("nDiscountKey") + "]"))
                                {
                                    oItem = currentOItem;
                                    oPriceElmt = (XmlElement)oItem.SelectSingleNode("DiscountPrice");
                                    if (oPriceElmt is null)
                                    {
                                        // NB 16/02/2010
                                        // Time to pull price out so we can round it, to avoid the multiple decimal place issues
                                        decimal nPrice;
                                        nPrice = Round(oItem.GetAttribute("price"), bForceRoundup: mbRoundUp);

                                        oPriceElmt = oDiscountXML.CreateElement("Item/DiscountPrice");
                                        oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice.ToString());
                                        // oPriceElmt.SetAttribute("OriginalUnitPrice", oItem.GetAttribute("price"))
                                        oPriceElmt.SetAttribute("UnitPrice", nPrice.ToString());
                                        // oPriceElmt.SetAttribute("UnitPrice", oItem.GetAttribute("price"))
                                        oPriceElmt.SetAttribute("Units", oItem.GetAttribute("quantity"));
                                        oPriceElmt.SetAttribute("Total", ((double)nPrice * Conversions.ToDouble(oItem.GetAttribute("quantity"))).ToString());
                                        // oPriceElmt.SetAttribute("Total", oItem.GetAttribute("price") * oItem.GetAttribute("quantity"))
                                        oPriceElmt.SetAttribute("UnitSaving", 0.ToString());
                                        oPriceElmt.SetAttribute("TotalSaving", 0.ToString());
                                        oItem.AppendChild(oPriceElmt);
                                    }
                                    nTotalItems = (int)Math.Round(nTotalItems + Conversions.ToDouble(oPriceElmt.GetAttribute("Units")));
                                    nTotalItemsValue = (decimal)(nTotalItemsValue + Convert.ToDecimal(oPriceElmt.GetAttribute("UnitPrice")) * Convert.ToDecimal(oPriceElmt.GetAttribute("Units")));
                                }
                                decimal nNewPrice = 0m;


                                int nDQ = 0;
                                if (Information.IsNumeric(oDiscount.GetAttribute("nDiscountMinQuantity")))
                                    nDQ = Conversions.ToInteger(oDiscount.GetAttribute("nDiscountMinQuantity"));
                                int nDT = 0;
                                if (Information.IsNumeric(oDiscount.GetAttribute("nDiscountMinPrice")))
                                    nDT = Conversions.ToInteger(oDiscount.GetAttribute("nDiscountMinPrice"));
                                if (nTotalItems >= nDQ & nDQ > 0 | nTotalItemsValue >= nDT & nDT > 0)
                                {
                                    //int nDiscountedSoFar = 0;
                                    foreach (XmlElement currentOItem1 in oDiscountXML.DocumentElement.SelectNodes("Item[Discount/@nDiscountKey=" + oDiscount.GetAttribute("nDiscountKey") + "]"))
                                    {
                                        oItem = currentOItem1;
                                        // if its a percentage we can just discount them all since its a flat rate.
                                        oPriceElmt = (XmlElement)oItem.SelectSingleNode("DiscountPrice");

                                        if (Conversions.ToDouble(oDiscount.GetAttribute("bDiscountIsPercent")) == 1d)
                                        {
                                            nNewPrice = Round(Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) / 100d * (100d - Conversions.ToDouble(oDiscount.GetAttribute("nDiscountValue"))), bForceRoundup: mbRoundUp);
                                        }
                                        else
                                        {
                                            nNewPrice = (decimal)(Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) - Conversions.ToDouble(oDiscount.GetAttribute("nDiscountValue")));
                                        }

                                        var oPriceLine = oDiscountXML.CreateElement("DiscountPriceLine");
                                        nPriceCount += 1;
                                        oPriceLine.SetAttribute("nDiscountKey", oDiscount.GetAttribute("nDiscountKey"));
                                        oPriceLine.SetAttribute("PriceOrder", nPriceCount.ToString());
                                        oPriceLine.SetAttribute("UnitPrice", nNewPrice.ToString());
                                        oPriceLine.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                        oPriceLine.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitPrice")) - (double)nNewPrice).ToString());
                                        oPriceLine.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                        oPriceElmt.AppendChild(oPriceLine);

                                        // this works the overall price

                                        oPriceElmt.SetAttribute("UnitPrice", nNewPrice.ToString());
                                        oPriceElmt.SetAttribute("Total", ((double)nNewPrice * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                        oPriceElmt.SetAttribute("UnitSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("OriginalUnitPrice")) - (double)nNewPrice).ToString());
                                        oPriceElmt.SetAttribute("TotalSaving", (Conversions.ToDouble(oPriceElmt.GetAttribute("UnitSaving")) * Conversions.ToDouble(oPriceElmt.GetAttribute("Units"))).ToString());
                                    }
                                }
                                break;
                            }
                            nTotalItems = 0;
                            nTotalItemsValue = 0m;
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "Discount_Break Group", ex, "", "", gbDebug);
                    }
                }

                #endregion

                #region Promotional codes
                public void DisablePromotionalDiscounts()
                {
                    try
                    {
                        if (!(cPromotionalDiscounts == ","))
                        {
                            string[] oIdArr = Strings.Split(cPromotionalDiscounts, ",");
                            int i;
                            var loopTo = Information.UBound(oIdArr);
                            for (i = 0; i <= loopTo; i++)
                            {
                                if (!string.IsNullOrEmpty(oIdArr[i]))
                                {
                                    myWeb.moDbHelper.setObjectStatus(Cms.dbHelper.objectTypes.CartDiscountRules, Cms.dbHelper.Status.Hidden, Conversions.ToLong(oIdArr[i]));
                                }
                            }
                        }

                        if (!(cVouchersUsed == ","))
                        {
                            string[] oIdArr = Strings.Split(cVouchersUsed, ",");
                            int i;
                            var loopTo1 = Information.UBound(oIdArr);
                            for (i = 0; i <= loopTo1; i++)
                            {
                                if (!string.IsNullOrEmpty(oIdArr[i]))
                                {
                                    myWeb.moDbHelper.UseCode(Conversions.ToInteger(oIdArr[i]), myWeb.mnUserId, myCart.mnCartId);

                                }
                            }
                        }
                    }

                    catch (Exception)
                    {

                    }
                }
                #endregion

                #region Content Procedures

                public string AddDiscountCode(string sCode)
                {
                    string cProcessInfo = "AddDiscountCode";
                    string sSql;
                    var strSQL = new System.Text.StringBuilder();
                    DataSet oDs;
                    string sXmlContent;
                    var docOrder = new XmlDocument();
                    var DiscountApplyDate = DateTime.Now;
                    DataSet oDsDiscounts;
                    var doc = new XmlDocument();
                    string oDiscountMessage = "The promo code you have provided is invalid for this transaction";
                    double minimumOrderTotal = 0d;
                    double maximumOrderTotal = 0d;
                    double dMaxPrice = 0d;
                    int nDiscountQuantity = 0;
                    double itemCost = 0d;
                    //int productGroups = 0;
                    double dMinPrice = 0d;
                    int nCount = 0;
                    bool applyToTotal = false;
                    string validateShippingGroup = string.Empty;

                    string cUserGroupIds = getUserGroupIDs(); // get the user groups
                    try
                    {
                        if (myCart.mnProcessId > 4)
                        {
                            return "";
                        }
                        else if (myCart.mnCartId > 0)
                        {
                            sSql = "select * from tblCartOrder where nCartOrderKey=" + myCart.mnCartId;
                            oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                            sXmlContent = Conversions.ToString(Operators.ConcatenateObject(oDs.Tables[0].Rows[0]["cCartXml"], ""));
                            docOrder.LoadXml(sXmlContent);
                            // Dim OrderPaymentStatus As String = docOrder.SelectSingleNode("Order").Attributes("status").Value
                            double orderTotal = Conversions.ToDouble(docOrder.SelectSingleNode("Order").Attributes["total"].Value);
                            // check for quantity
                            int nItemCount = docOrder.SelectNodes("Order/Item").Count;

                            if (myWeb.moDbHelper.checkTableColumnExists("tblCartDiscountRules", "bAllProductExcludeGroups"))
                            {
                                // ' call stored procedure else existing code.
                                // ' Passing parameter: cPromoCodeUserEntered,DiscountApplyDate,cUserGroupIds,nCartId
                                var param = new Hashtable();
                                param.Add("PromoCodeEntered", sCode);
                                param.Add("UserGroupIds", cUserGroupIds);
                                param.Add("CartOrderId", myCart.mnCartId);
                                param.Add("CartOrderDate", DiscountApplyDate);
                                oDsDiscounts = myWeb.moDbHelper.GetDataSet("spCheckDiscounts", "Discount", "Discounts", false, param, CommandType.StoredProcedure);
                            }

                            else
                            {

                                strSQL.Append("SELECT tblCartDiscountRules.nDiscountKey, tblCartDiscountRules.nDiscountForeignRef, tblCartDiscountRules.cDiscountName,  ");
                                strSQL.Append("tblCartDiscountRules.cDiscountCode, tblCartDiscountRules.bDiscountIsPercent, tblCartDiscountRules.nDiscountCompoundBehaviour,  ");
                                strSQL.Append("tblCartDiscountRules.nDiscountValue, tblCartDiscountRules.nDiscountMinPrice, tblCartDiscountRules.nDiscountMinQuantity,  ");
                                strSQL.Append("  tblCartDiscountRules.nDiscountCat, tblCartDiscountRules.cAdditionalXML, tblCartDiscountRules.nAuditId,  ");
                                strSQL.Append("tblCartDiscountRules.nDiscountCodeType, tblCartDiscountRules.cDiscountUserCode  ");
                                strSQL.Append("FROM tblCartDiscountRules  ");
                                strSQL.Append("INNER JOIN tblAudit ON tblCartDiscountRules.nAuditId = tblAudit.nAuditKey AND (tblAudit.nStatus = 1) ");
                                if (!string.IsNullOrEmpty(sCode))
                                {
                                    strSQL.Append("WHERE tblCartDiscountRules.cDiscountCode= '" + sCode + "'");
                                }
                                strSQL.Append("AND (tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate > " + sqlDate(DiscountApplyDate) + ")  ");
                                strSQL.Append("AND (tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= " + sqlDate(DiscountApplyDate) + ") ");


                                oDsDiscounts = myWeb.moDbHelper.GetDataSet(strSQL.ToString(), "Discount", "Discounts");
                            }

                            if (oDsDiscounts.Tables[0].Rows.Count == 0)
                            {
                                if (!string.IsNullOrEmpty(sCode))
                                {
                                    oDsDiscounts.Clear();
                                    oDsDiscounts = null;
                                    return oDiscountMessage;
                                }
                            }
                            else
                            {
                                //string additionalInfo = Conversions.ToString(Operators.AddObject(Operators.AddObject("<additionalXml>", oDsDiscounts.Tables["Discount"].Rows[0]["cAdditionalXML"]), "</additionalXml>"));
                                //doc.LoadXml(additionalInfo);

                                //if (doc.InnerXml.Contains("nMinimumOrderValue"))
                                //{
                                //    minimumOrderTotal = Conversions.ToDouble("0" + doc.SelectSingleNode("additionalXml").SelectSingleNode("nMinimumOrderValue").InnerText);
                                //}
                                //if (doc.InnerXml.Contains("nMaximumOrderValue"))
                                //{
                                //    maximumOrderTotal = Conversions.ToDouble("0" + doc.SelectSingleNode("additionalXml").SelectSingleNode("nMaximumOrderValue").InnerText);
                                //}
                                //if (doc.InnerXml.Contains("nDiscountMaxPrice"))
                                //{
                                //    dMaxPrice = Conversions.ToDouble("0" + doc.SelectSingleNode("additionalXml").SelectSingleNode("nDiscountMaxPrice").InnerText);
                                //}

                                //if (doc.InnerXml.Contains("bApplyToOrder"))
                                //{
                                //    if (string.IsNullOrEmpty(doc.SelectSingleNode("additionalXml").SelectSingleNode("bApplyToOrder").InnerText))
                                //    {
                                //        applyToTotal = false;
                                //    }
                                //    else
                                //    {
                                //        applyToTotal = Convert.ToBoolean(doc.SelectSingleNode("additionalXml").SelectSingleNode("bApplyToOrder").InnerText);
                                //    }

                                //    if (maximumOrderTotal != 0d)
                                //    {
                                //        if (!(orderTotal >= minimumOrderTotal & orderTotal <= maximumOrderTotal))
                                //        {
                                //            oDsDiscounts.Clear();
                                //            oDsDiscounts = null;
                                //            return oDiscountMessage;
                                //        }
                                //    }
                                //    if (applyToTotal)
                                //    {
                                //        if (maximumOrderTotal != 0d)
                                //        {
                                //            if (!(orderTotal >= minimumOrderTotal & orderTotal <= maximumOrderTotal))
                                //            {
                                //                oDsDiscounts.Clear();
                                //                oDsDiscounts = null;
                                //                return oDiscountMessage;
                                //            }
                                //        }

                                //    }

                                //    // check maximum item price value set or not
                                //    if (dMaxPrice != 0d)
                                //    {
                                //        // validate quantity of cart as individual item not total quantity of item purchased
                                //        nDiscountQuantity = Conversions.ToInteger(Operators.ConcatenateObject("0", oDsDiscounts.Tables["Discount"].Rows[0]["nDiscountMinQuantity"]));
                                //        dMinPrice = Conversions.ToDouble(Operators.ConcatenateObject("0", oDsDiscounts.Tables["Discount"].Rows[0]["nDiscountMinPrice"]));

                                //        foreach (XmlNode item in docOrder.SelectNodes("Order/Item"))
                                //        {
                                //            itemCost = Conversions.ToDouble(item.Attributes["itemTotal"].Value);
                                //            if (itemCost >= dMinPrice & itemCost <= dMaxPrice)
                                //            {
                                //                nCount = nCount + 1;
                                //            }
                                //        }
                                //        if (nCount < nDiscountQuantity)
                                //        {
                                //            oDsDiscounts.Clear();
                                //            oDsDiscounts = null;
                                //            return oDiscountMessage;
                                //        }
                                //    }

                                //}
                                //oDsDiscounts.Clear();
                                //oDsDiscounts = null;

                                //Add new code modifications for multiple discounts with different types like different price range.
                                bool validPromoFound = false;

                                foreach (DataRow discountRow in oDsDiscounts.Tables["Discount"].Rows)
                                {
                                    // Wrap Additional XML
                                    string additionalInfo = Conversions.ToString(
                                        Operators.AddObject(
                                            Operators.AddObject("<additionalXml>", discountRow["cAdditionalXML"]),
                                            "</additionalXml>"
                                        )
                                    );

                                    doc.LoadXml(additionalInfo);

                                    // Initialize variables for each promo code

                                    // Parse XML values safely
                                    XmlNode nodeMin = doc.SelectSingleNode("additionalXml/nMinimumOrderValue");
                                    if (nodeMin != null && !string.IsNullOrWhiteSpace(nodeMin.InnerText))
                                    {
                                        minimumOrderTotal = Conversions.ToDouble("0" + nodeMin.InnerText);
                                    }

                                    XmlNode nodeMax = doc.SelectSingleNode("additionalXml/nMaximumOrderValue");
                                    if (nodeMax != null && !string.IsNullOrWhiteSpace(nodeMax.InnerText))
                                    {
                                        maximumOrderTotal = Conversions.ToDouble("0" + nodeMax.InnerText);
                                    }

                                    XmlNode nodeMaxPrice = doc.SelectSingleNode("additionalXml/nDiscountMaxPrice");
                                    if (nodeMaxPrice != null && !string.IsNullOrWhiteSpace(nodeMaxPrice.InnerText))
                                    {
                                        dMaxPrice = Conversions.ToDouble("0" + nodeMaxPrice.InnerText);
                                    }

                                    XmlNode nodeApply = doc.SelectSingleNode("additionalXml/bApplyToOrder");
                                    if (nodeApply != null && !string.IsNullOrWhiteSpace(nodeApply.InnerText))
                                    {
                                        applyToTotal = Convert.ToBoolean(nodeApply.InnerText);
                                    }

                                    // If applyToTotal is true, validate order total
                                    if (applyToTotal)
                                    {
                                        if (maximumOrderTotal != 0d)
                                        {
                                            if (!(orderTotal >= minimumOrderTotal && orderTotal <= maximumOrderTotal))
                                            {
                                                continue; // Order total not in range
                                            }
                                        }
                                    }

                                    // If applyToTotal is false, validate items only
                                    if (!applyToTotal)
                                    {
                                        if (dMaxPrice != 0d)
                                        {
                                            nDiscountQuantity = Conversions.ToInteger("0" + discountRow["nDiscountMinQuantity"]);
                                            dMinPrice = Conversions.ToDouble("0" + discountRow["nDiscountMinPrice"]);

                                            nCount = 0;
                                            foreach (XmlNode item in docOrder.SelectNodes("Order/Item"))
                                            {
                                                itemCost = Conversions.ToDouble(item.Attributes["itemTotal"].Value);
                                                if (itemCost >= dMinPrice && itemCost <= dMaxPrice)
                                                {
                                                    nCount++;
                                                }
                                            }

                                            if (nCount < nDiscountQuantity)
                                            {
                                                continue; // Not enough qualifying items
                                            }
                                        }
                                    }

                                    //If all checks pass, promo is valid
                                    validPromoFound = true;
                                }

                                // Final cleanup
                                oDsDiscounts.Clear();
                                oDsDiscounts = null;

                                // Return message if no promo applied
                                if (!validPromoFound)
                                {
                                    return oDiscountMessage;
                                }

                                // End Code Modifications

                            }
                            // myCart.moCartXml

                            foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                            {

                                // load existing notes from Cart
                                sXmlContent = Conversions.ToString(Operators.ConcatenateObject(oRow["cClientNotes"], ""));
                                if (string.IsNullOrEmpty(sXmlContent))
                                {
                                    sXmlContent = "<Notes><PromotionalCode/></Notes>";
                                }
                                var NotesXml = new XmlDocument();
                                NotesXml.LoadXml(sXmlContent);

                                if (NotesXml.SelectSingleNode("Notes") is null)
                                {
                                    var notesElement = NotesXml.CreateElement("NoteInfo");
                                    notesElement.InnerXml = "<Notes><PromotionalCode/></Notes>";
                                    NotesXml.FirstChild.AppendChild(notesElement.FirstChild);
                                }

                                if (NotesXml.SelectSingleNode("//Notes/PromotionalCode[node()='" + sCode + "']") != null)
                                {
                                }
                                // do nothing code exists
                                else if (NotesXml.SelectSingleNode("//Notes/PromotionalCode") is null)
                                {
                                    // add another promotional code
                                    var newElmt = NotesXml.CreateElement("PromotionalCode");
                                    NotesXml.SelectSingleNode("//Notes").AppendChild(newElmt);
                                }
                                else if (string.IsNullOrEmpty(NotesXml.SelectSingleNode("//Notes/PromotionalCode").InnerText))
                                {
                                    NotesXml.SelectSingleNode("//Notes/PromotionalCode").InnerText = sCode;
                                }
                                else
                                {
                                    // add another promotional code
                                    var newElmt = NotesXml.CreateElement("PromotionalCode");
                                    NotesXml.SelectSingleNode("//Notes").AppendChild(newElmt);
                                }
                                oRow["cClientNotes"] = NotesXml.OuterXml;
                            }
                            myWeb.moDbHelper.updateDataset(ref oDs, "Order", true);
                            oDs.Clear();
                            oDs = null;

                            return sCode;
                        }

                        else
                        {

                            return "";
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "AddDiscountCode", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }

                }

                public bool ValidateDiscount(double dAmount, string additionalInfo)
                {
                    string cProcessInfo = "ValidateDiscount";
                    try
                    {
                        var doc = new XmlDocument();
                        bool applyToTotal = false;
                        double minimumOrderTotal = 0d;
                        double maximumOrderTotal = 0d;
                        // Dim dMaxPrice As Double = 0
                        if (!string.IsNullOrEmpty(additionalInfo))
                        {


                            doc.LoadXml(additionalInfo);

                            if (doc.InnerXml.Contains("nMinimumOrderValue"))
                            {
                                minimumOrderTotal = Conversions.ToDouble("0" + doc.SelectSingleNode("additionalXml").SelectSingleNode("nMinimumOrderValue").InnerText);
                            }
                            if (doc.InnerXml.Contains("nMaximumOrderValue"))
                            {
                                maximumOrderTotal = Conversions.ToDouble("0" + doc.SelectSingleNode("additionalXml").SelectSingleNode("nMaximumOrderValue").InnerText);
                            }
                            // If (doc.InnerXml.Contains("nDiscountMaxPrice")) Then
                            // dMaxPrice = CDbl("0" & doc.SelectSingleNode("additionalXml").SelectSingleNode("nDiscountMaxPrice").InnerText)
                            // End If

                            if (doc.InnerXml.Contains("bApplyToOrder"))
                            {
                                if (string.IsNullOrEmpty(doc.SelectSingleNode("additionalXml").SelectSingleNode("bApplyToOrder").InnerText))
                                {
                                    applyToTotal = false;
                                }
                                else
                                {
                                    applyToTotal = Convert.ToBoolean(doc.SelectSingleNode("additionalXml").SelectSingleNode("bApplyToOrder").InnerText);
                                }
                                if (maximumOrderTotal != 0d)
                                {
                                    if (!(dAmount >= minimumOrderTotal & dAmount <= maximumOrderTotal))
                                    {
                                        return false;
                                    }
                                }
                                if (applyToTotal)
                                {
                                    if (maximumOrderTotal != 0d)
                                    {
                                        if (!(dAmount >= minimumOrderTotal & dAmount <= maximumOrderTotal))
                                        {
                                            return false;
                                        }
                                    }
                                    // Else
                                    // If (dMaxPrice <> 0) Then
                                    // If Not (dAmount >= dMinPrice And dAmount <= dMaxPrice) Then
                                    // Return False
                                    // End If
                                    // End If
                                }

                            }
                            return true;

                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ValidateDiscount", ex, "", cProcessInfo, gbDebug);
                    }

                    return default;
                }

                public void getAvailableDiscounts(ref XmlElement oRootElmt)
                {
                    myWeb.PerfMon.Log("Discount", "getAvailableDiscounts");
                    if (!bIsCartOn & !bIsQuoteOn)
                        return;
                    // Gets the discounts applicable to the listed products, filtered by user
                    // and adds them under the relevant content. Will do this for both Brief and Detail
                    try
                    {
                        var strSQL = new System.Text.StringBuilder();
                        var strItemIds = new System.Text.StringBuilder();
                        string cItemIds;
                        //int nMode = 1;
                        XmlElement oDiscountElmt;
                        XmlElement oContentElmt;

                        string cContentTypes = moConfig["ProductTypes"];
                        if (string.IsNullOrEmpty(cContentTypes))
                            cContentTypes = myWeb.defaultProductTypes;
                        string cUserGroupIds = getUserGroupIDs();

                        // Get the content types that are products with discounts on this site.

                        string[] cContentTypesArray = cContentTypes.Split(',');
                        if (cContentTypesArray.Length > 0)
                        {
                            cContentTypes = "[(";
                            int i = 0;
                            foreach (string cContentType in cContentTypesArray)
                            {
                                cContentTypes += "@type='" + cContentType.Trim() + "'";
                                i = i + 1;
                                if (i < cContentTypesArray.Length)
                                {
                                    cContentTypes += " or ";
                                }
                                else
                                {
                                    // cContentTypes &= "]"
                                    cContentTypes += ") and not(ancestor::Discounts)]";
                                }
                            }
                        }
                        else
                        {
                            cContentTypes = "";
                        }

                        // Get a list of the content ID's for all products on the site
                        foreach (XmlElement currentOContentElmt in oRootElmt.SelectNodes("descendant-or-self::Content" + cContentTypes))
                        {
                            oContentElmt = currentOContentElmt;
                            if (oContentElmt.GetAttribute("id") != null)
                            {
                                strItemIds.Append(Tools.Database.SqlString(oContentElmt.GetAttribute("id").Trim()));
                                strItemIds.Append(",");
                            }
                        }
                        cItemIds = strItemIds.ToString();
                        cItemIds = cItemIds.TrimEnd(',');



                        if (!cItemIds.Equals(""))
                        {

                            strSQL.Append("SELECT tblCartDiscountRules.nDiscountKey as id, ");
                            strSQL.Append("tblCartDiscountRules.bDiscountIsPercent as isPercent, ");
                            strSQL.Append("tblCartDiscountRules.nDiscountValue as value, ");
                            strSQL.Append("tblCartDiscountRules.nDiscountCat as type, ");
                            strSQL.Append("tblCartCatProductRelations.nContentId as nContentId, ");
                            strSQL.Append("tblCartDiscountRules.nDiscountCodeType, ");
                            strSQL.Append("tblCartDiscountRules.cDiscountUserCode, ");
                            strSQL.Append("tblCartDiscountRules.cAdditionalXML ");
                            strSQL.Append("FROM tblCartCatProductRelations ");
                            strSQL.Append("INNER JOIN tblCartDiscountProdCatRelations ON tblCartCatProductRelations.nCatId = tblCartDiscountProdCatRelations.nProductCatId ");
                            strSQL.Append("INNER JOIN tblCartDiscountRules ");
                            strSQL.Append("INNER JOIN tblCartDiscountDirRelations ON tblCartDiscountRules.nDiscountKey = tblCartDiscountDirRelations.nDiscountId ");
                            strSQL.Append("INNER JOIN tblAudit ON tblCartDiscountRules.nAuditId = tblAudit.nAuditKey ON tblCartDiscountProdCatRelations.nDiscountId = tblCartDiscountRules.nDiscountKey ");
                            strSQL.Append("WHERE (tblAudit.nStatus = 1) ");
                            strSQL.Append("AND (tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= GETDATE())  ");
                            strSQL.Append("AND (tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= GETDATE()) ");
                            strSQL.Append("AND (tblCartDiscountDirRelations.nDirId IN (" + cUserGroupIds + ")) ");
                            strSQL.Append("AND (tblCartCatProductRelations.nContentId IN (" + cItemIds + ")) ");
                            strSQL.Append("AND (tblCartDiscountRules.cDiscountUserCode = '' AND  tblCartDiscountRules.nDiscountCodeType = 0) ");


                            // strSQL.Append("SELECT dr.nDiscountKey as id, dr.bDiscountIsPercent as isPercent, dr.nDiscountValue as value, dr.nDiscountCat as type, tblCartCatProductRelations.nContentId, dr.cAdditionalXML ")
                            // strSQL.Append("FROM tblCartCatProductRelations RIGHT OUTER JOIN ")
                            // strSQL.Append("tblCartDiscountProdCatRelations ON tblCartCatProductRelations.nCatId = tblCartDiscountProdCatRelations.nProductCatId RIGHT OUTER JOIN ")
                            // strSQL.Append("tblAudit RIGHT OUTER JOIN ")
                            // strSQL.Append("tblCartDiscountRules dr ON tblAudit.nAuditKey = dr.nAuditId LEFT OUTER JOIN" & _
                            // " tblCartDiscountDirRelations LEFT OUTER JOIN ")
                            // strSQL.Append("tblDirectoryRelation RIGHT OUTER JOIN ")
                            // strSQL.Append("tblDirectory ON tblDirectoryRelation.nDirParentId = tblDirectory.nDirKey ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey ON ")
                            // strSQL.Append("dr.nDiscountKey = tblCartDiscountDirRelations.nDiscountId ON ")
                            // strSQL.Append("tblCartDiscountProdCatRelations.nDiscountId = dr.nDiscountKey ")
                            // strSQL.Append(sSqlWhere & " ")
                            // strSQL.Append("and (tblCartCatProductRelations.nContentId IN (" & cContentIds & ")) ")
                            // strSQL.Append("and tblAudit.nStatus = 1 ")
                            // strSQL.Append("and (tblAudit.dPublishDate is null or tblAudit.dPublishDate = 0 or tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(Today) & " ) ")
                            // strSQL.Append("and (tblAudit.dExpireDate is null or tblAudit.dExpireDate = 0 or tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(Today.AddHours(23).AddMinutes(59).AddSeconds(59), True) & " ) ")
                            // strSQL.Append("AND (tr.cDiscountUserCode = '' AND  tr.nDiscountCodeType = 0) ")



                            myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-startGetDataset");

                            string sSql = strSQL.ToString();
                            var oXML = oRootElmt.OwnerDocument.CreateElement("DiscountsRoot");

                            using (var oDS = myWeb.moDbHelper.GetDataSet(sSql, "Discount", "Discounts"))
                            {
                                myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-startEndDataset");
                                if (oDS is null)
                                {
                                    return;
                                }

                                if (oDS.Tables.Count == 0)
                                    return;

                                foreach (DataColumn oDC in oDS.Tables["Discount"].Columns)
                                {
                                    if (!(oDC.ColumnName == "cAdditionalXML"))
                                        oDC.ColumnMapping = MappingType.Attribute;
                                }
                                oDS.Tables["Discount"].Columns["cAdditionalXML"].ColumnMapping = MappingType.SimpleContent;

                                myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-startGetDatasetXml");

                                oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                            }

                            myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-endGetDatasetXml");


                            myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-startIterateContentNodes");

                            // NB : 19-01-2010 - this still runs many checks for non products
                            // What does all this do? Nothing is updating the price until you buy it
                            // It should be appending discounts to the content right? Then why isn't it

                            myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-startAddDiscountsToContent");
                            long nStep = 0L;

                            foreach (XmlElement currentODiscountElmt in oXML.SelectNodes("Discounts/Discount[not(descendant-or-self::cPromotionalCode/node()!='') or not(descendant-or-self::cPromotionalCode)]"))
                            {
                                oDiscountElmt = currentODiscountElmt;

                                string cContentId = oDiscountElmt.GetAttribute("nContentId");

                                if (!string.IsNullOrEmpty(cContentId))
                                {
                                    if (Information.IsNumeric(cContentId))
                                    {
                                        long nContentId = Conversions.ToLong(cContentId);
                                        // For Each oContentElmt In oRootElmt.SelectNodes("descendant-or-self::Content[@id=" & nContentId & "]")
                                        foreach (XmlElement currentOContentElmt1 in oRootElmt.SelectNodes("descendant-or-self::Content[@id=" + nContentId + " and not(ancestor::Discounts)]"))
                                        {
                                            oContentElmt = currentOContentElmt1;
                                            // For Each oContentElmt In oRootElmt.SelectNodes("/Page/Contents/descendant-or-self::Content[@id=" & nContentId & "]")
                                            XmlElement oTmpCheck = (XmlElement)oContentElmt.SelectSingleNode("Discount[@id=" + oDiscountElmt.GetAttribute("id") + "]");
                                            if (oTmpCheck is null)
                                            {
                                                // TS adjusted so can be added more than once
                                                oContentElmt.AppendChild(oDiscountElmt.CloneNode(true));
                                            }
                                            nStep = nStep + 1L;
                                        }
                                    }
                                }
                            }

                            myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-endAddDiscountsToContent-" + nStep + " Contents with Discounts Added");

                            myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-startCalculateDiscounts");
                            // For Each oContentElmt In oRootElmt.SelectNodes("/Page/Contents/descendant-or-self::Content")
                            foreach (XmlElement currentOContentElmt2 in oRootElmt.SelectNodes("descendant-or-self::Content[Prices/Price[@currency='" + mcCurrency + "' and node()!=''] and Discount]"))
                            {
                                oContentElmt = currentOContentElmt2;

                                string nContentId = oContentElmt.GetAttribute("id");
                                if (string.IsNullOrEmpty(nContentId))
                                    nContentId = 0.ToString();

                                // for showing basic discounts
                                //decimal nNewPrice = 0m;

                                string[] cPriceModifiers = new string[] { "Basic_Money", "Basic_Percent", "Cheapest_Free" };
                                if (!string.IsNullOrEmpty(mcPriceModOrder))
                                    cPriceModifiers = Strings.Split(mcPriceModOrder, ",");
                                int nI;
                                //int nPriceCount = 0;
                                // this counts where we are on the prices, shows the order we done them in
                                var loopTo = Information.UBound(cPriceModifiers);
                                for (nI = 0; nI <= loopTo; nI++)
                                {
                                    switch (cPriceModifiers[nI] ?? "")
                                    {
                                        case "Basic_Money":
                                            {
                                                foreach (XmlElement currentODiscountElmt1 in oContentElmt.SelectNodes("Discount[@type=1]"))
                                                {
                                                    oDiscountElmt = currentODiscountElmt1;
                                                    if (oDiscountElmt.GetAttribute("isPercent").Equals("0"))
                                                    {
                                                        applyDiscountsToPriceXml(oContentElmt, oDiscountElmt);
                                                    }
                                                }

                                                break;
                                            }
                                        case "Basic_Percent":
                                            {
                                                foreach (XmlElement currentODiscountElmt2 in oContentElmt.SelectNodes("Discount[@type=1]"))
                                                {
                                                    oDiscountElmt = currentODiscountElmt2;
                                                    if (oDiscountElmt.GetAttribute("isPercent").Equals("1"))
                                                    {
                                                        applyDiscountsToPriceXml(oContentElmt, oDiscountElmt);
                                                    }
                                                }

                                                break;
                                            }
                                        case "Cheapest_Free":
                                            {
                                                break;
                                            }
                                            // Here we want to show the other items available with this offer... Maybe !

                                    }

                                }
                            }
                            myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-endCalculateDiscounts");
                        }
                        myWeb.PerfMon.Log("Discount", "getAvailableDiscounts-endIterateContentNodes");
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "getAvailableDiscounts", ex, "", "", gbDebug);
                    }
                    finally
                    {
                        myWeb.moDbHelper.CloseConnection();
                    }
                }

                /// <summary>
                /// Returns an XML contain information about all of the discounts on the site.
                /// Only used to inumberate discounts to display to customers. 
                /// WE DO NOT WANT TO CALL THIS ON EVERY PAGE as it suffers from poor performance once a lot of discounts are enabled.
                /// </summary>
                /// <param name="PageElmt"></param>
                /// <remarks></remarks>
                public virtual void getDiscountXML(ref XmlElement PageElmt)
                {
                    myWeb.PerfMon.Log("Discount", "start-getDiscountXML");
                    if (!bIsCartOn & !bIsQuoteOn)
                        return;
                    try
                    {
                        int nDiscountID = 0;
                        if (Information.IsNumeric(myWeb.moRequest.QueryString["DiscountID"]))
                            nDiscountID = Conversions.ToInteger(myWeb.moRequest.QueryString["DiscountID"]);
                        string sSQL;
                        DataSet oDS;
                        var oDiscounts = PageElmt.OwnerDocument.CreateElement("Discounts");
                        var oContent = PageElmt.OwnerDocument.CreateElement("Contents");
                        // get the discounts
                        sSQL = "SELECT dr.nDiscountKey as id, dr.cDiscountName as name, dr.cDiscountCode as code, dr.bDiscountIsPercent as isPercent, dr.nDiscountValue as value, dr.nDiscountMinPrice as minPrice, dr.nDiscountMinQuantity as minQty, dr.nDiscountCat as type, dr.cAdditionalXML" + " FROM tblAudit RIGHT OUTER JOIN" + " tblCartDiscountRules dr ON tblAudit.nAuditKey = dr.nAuditId LEFT OUTER JOIN" + " tblCartDiscountDirRelations LEFT OUTER JOIN" + " tblDirectoryRelation RIGHT OUTER JOIN" + " tblDirectory ON tblDirectoryRelation.nDirParentId = tblDirectory.nDirKey ON tblCartDiscountDirRelations.nDirId = tblDirectory.nDirKey ON " + " dr.nDiscountKey = tblCartDiscountDirRelations.nDiscountId" + " WHERE (tblDirectoryRelation.nDirChildId = " + myWeb.mnUserId + " OR" + " tblDirectoryRelation.nDirChildId IS NULL) AND (tblAudit.nStatus = 1) AND (tblAudit.dPublishDate IS NULL OR" + " tblAudit.dPublishDate = 0 OR" + " tblAudit.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + ") AND (tblAudit.dExpireDate IS NULL OR" + " tblAudit.dExpireDate = 0 Or tblAudit.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + ")";
                        if (nDiscountID > 0)
                        {
                            sSQL += " AND dr.nDiscountKey = " + nDiscountID;
                        }
                        oDS = myWeb.moDbHelper.GetDataSet(sSQL, "Discount", "Discounts");
                        myWeb.PerfMon.Log("Discount", "getDiscountXML-gotDiscounts");
                        foreach (DataColumn oDC in oDS.Tables["Discount"].Columns)
                            oDC.ColumnMapping = MappingType.Attribute;
                        oDS.Tables["Discount"].Columns["cAdditionalXML"].ColumnMapping = MappingType.SimpleContent;

                        oDiscounts.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                        oDiscounts = (XmlElement)oDiscounts.FirstChild;
                        // now the contents

                        // TS modified to only pull in contents with currently active discounts

                        sSQL = "SELECT c.nContentKey AS id," + " dbo.fxn_getContentParents(c.nContentKey) as parId " + " , c.cContentForiegnRef AS ref, c.cContentName AS name, " + " c.cContentSchemaName AS type, c.cContentXmlBrief AS content, a.nStatus AS status, a.dPublishDate AS publish, a.dExpireDate AS expire, a.nInsertDirId as owner, " + " pcr.nDiscountId" + " FROM tblContent c INNER JOIN" + " tblAudit a ON c.nAuditId = a.nAuditKey INNER JOIN" + " tblCartCatProductRelations ON c.nContentKey = tblCartCatProductRelations.nContentId INNER JOIN" + " tblCartDiscountProdCatRelations pcr ON tblCartCatProductRelations.nCatId = pcr.nProductCatId" + " INNER JOIN tblCartDiscountRules d ON pcr.nDiscountId  = d.nDiscountKey" + " INNER JOIN tblAudit da ON d.nAuditId = da.nAuditKey " + " WHERE (a.nStatus = 1) AND (a.dPublishDate IS NULL OR" + " a.dPublishDate = 0 OR" + " a.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + ") AND (a.dExpireDate IS NULL OR" + " a.dExpireDate = 0 OR" + " a.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + ")" + " AND (da.nStatus = 1) AND (da.dPublishDate IS NULL OR" + " da.dPublishDate = 0 OR" + " da.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + ") AND (da.dExpireDate IS NULL OR" + " da.dExpireDate = 0 OR" + " da.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + ")";
                        oDS = new DataSet();
                        oDS = myWeb.moDbHelper.GetDataSet(sSQL, "Content", "Contents");
                        myWeb.PerfMon.Log("Discount", "getDiscountXML-gotContent");
                        oDS.Tables[0].Columns["nDiscountId"].ColumnMapping = MappingType.Attribute;
                        // now to add them to discounts 

                        DateTime? argdExpireDate = DateTime.Parse("0001-01-01");
                        DateTime? argdUpdateDate = DateTime.Parse("0001-01-01");
                        myWeb.moDbHelper.AddDataSetToContent(ref oDS, ref oContent, dExpireDate: ref argdExpireDate, dUpdateDate: ref argdUpdateDate, (long)myWeb.mnPageId, true, "", false);

                        myWeb.PerfMon.Log("Discount", "getDiscountXML-appendStart");
                        XmlElement oDisc;

                        int appendCount = 0;

                        foreach (XmlElement oTmp in oContent.SelectNodes("Content"))
                        {
                            // oTmp.InnerText = Replace(Replace(oTmp.InnerText, "&gt;", ">"), "&lt;", "<")
                            int nDiscID = Conversions.ToInteger(oTmp.GetAttribute("nDiscountId"));
                            oDisc = (XmlElement)oDiscounts.SelectSingleNode("Discount[@id=" + nDiscID + "]");
                            if (oDisc != null)
                            {
                                if (oDisc.SelectSingleNode("Content[@id=" + oTmp.GetAttribute("id") + "]") is null)
                                {
                                    oDisc.AppendChild(oDisc.OwnerDocument.ImportNode(oTmp.CloneNode(true), true));
                                }
                            }
                            appendCount = appendCount + 1;
                        }
                        myWeb.PerfMon.Log("Discount", "getDiscountXML appended " + appendCount + " Items");

                        myWeb.PerfMon.Log("Discount", "end-getDiscountXML");
                        PageElmt.AppendChild(oDiscounts);
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "returnDocumentFromItem", ex, "", "", gbDebug);
                    }
                }

                public string RemoveDiscountCode()
                {
                    string cProcessInfo = "RemoveDiscountCode";
                    string sSql;
                    DataSet oDs;
                    string sPromoCode = "";
                    try
                    {
                        if (myCart.mnProcessId > 4)
                        {
                            return "";
                        }
                        else
                        {
                            // myCart.moCartXml
                            if (myCart.mnCartId > 0)
                            {
                                sSql = "select * from tblCartOrder where nCartOrderKey=" + myCart.mnCartId;
                                oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                                XmlElement xmlNotes = null;
                                var xmlDoc = new XmlDocument();

                                foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                                {
                                    xmlDoc.LoadXml(Conversions.ToString(oRow["cClientNotes"]));
                                    xmlNotes = (XmlElement)xmlDoc.SelectSingleNode("Notes/PromotionalCode");

                                    oRow["cClientNotes"] = null;
                                }
                                myWeb.moDbHelper.updateDataset(ref oDs, "Order", true);
                                oDs.Clear();
                                oDs = null;
                                if (xmlNotes != null)
                                {
                                    sPromoCode = xmlNotes.InnerText;
                                }

                                UpdatePackagingforRemovePromoCode(myCart.mnCartId, sPromoCode);
                            }
                            return "";
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "RemoveDiscountCode", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }

                }

                // update packaging from giftbox to standard when removing promocode
                public void UpdatePackagingforRemovePromoCode(int CartId, string sPromoCode)
                {
                    try
                    {
                        string sSQL = string.Empty;
                        string sValidtoremove = string.Empty;
                        if (!string.IsNullOrEmpty(sPromoCode))
                        {
                            sSQL = "select nDiscountKey from tblCartDiscountRules where cDiscountCode = '" + sPromoCode + "' and cAdditionalXML like '%<bFreeGiftBox>True</bFreeGiftBox>%'";
                            sValidtoremove = myWeb.moDbHelper.ExeProcessSqlScalar(sSQL);
                        }

                        if (!string.IsNullOrEmpty(sValidtoremove))
                        {
                            if (moConfig["DefaultPack"] != null & moConfig["GiftPack"] != null)
                            {
                                sSQL = "";
                                sSQL = "update tblcartitem set cItemName = '" + moConfig["DefaultPack"] + "', nPrice = 0.00 where nParentId != 0 and  cItemName = '" + moConfig["GiftPack"] + "' and nCartOrderId =" + CartId.ToString();
                                myWeb.moDbHelper.ExeProcessSql(sSQL);
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdatePackagingforRemovePromoCode", ex, "", "", gbDebug);
                    }
                }
                #endregion

                #region Copied Cart Functions

                // to use for contentprices

                public double getProductPricesByXml_OLD(string cXml)
                {
                    double getProductPricesByXml_OLDRet = default;
                    myWeb.PerfMon.Log("Discount", "getProductPricesByXml");
                    string cGroupXPath = "";
                    var oProd = myWeb.moPageXml.CreateNode(XmlNodeType.Document, "", "product");
                    XmlNodeList oPrices;
                    XmlNode oDefaultPrice;
                    double nPrice = 0.0d;
                    string cProcessInfo = "";
                    try
                    {
                        // Load product xml
                        oProd.InnerXml = cXml;
                        // Get the Default Price - note if it does not exist, then this is menaingless.
                        oDefaultPrice = oProd.SelectSingleNode("Content/Prices/Default");
                        // If logged on we need to search prices by group, otherwise use the default value.
                        if (myWeb.mnUserId > 0)
                        {
                            // Define the group xpath
                            if (!string.IsNullOrEmpty(mcGroups))
                            {
                                string sGroups = mcGroups;
                                sGroups = Strings.Replace(sGroups, " ", "_");
                                cGroupXPath = "[self::" + Strings.Replace(sGroups, ",", " or self::") + "]";
                                // Get the prices
                                oPrices = oProd.SelectNodes("Content/Prices/*" + cGroupXPath);
                                if (oDefaultPrice != null)
                                {
                                    // Get the minimum price on offer
                                    foreach (XmlNode oPrice in oPrices)
                                    {
                                        if (Information.IsNumeric(oPrice.InnerText))
                                        {
                                            if (Conversions.ToDouble(oPrice.InnerText) < nPrice & Conversions.ToDouble(oPrice.InnerText) > 0d | nPrice == 0d)
                                                nPrice = Conversions.ToDouble(oPrice.InnerText);
                                        }
                                    }
                                }
                            }
                        }
                        // Not logged on - ensure that the default price is returned, if applicable.
                        else if (oDefaultPrice != null)
                        {
                            if (Information.IsNumeric(oDefaultPrice.InnerText))
                                nPrice = Conversions.ToDouble(oDefaultPrice.InnerText);
                        }
                        if (nPrice == 0d)
                        {

                            string cCur = Conversions.ToString(myWeb.moSession["cCurrency"]); // moCartConfig("Currency")
                            if (string.IsNullOrEmpty(cCur))
                                cCur = "GBP";

                            foreach (XmlElement oPriceElmt in oProd.SelectNodes("Content/Prices/Price"))
                            {
                                if ((oPriceElmt.GetAttribute("currency") ?? "") == (cCur ?? ""))
                                {
                                    string cPrice = oPriceElmt.InnerText;
                                    if (Information.IsNumeric(cPrice))
                                        nPrice = Conversions.ToDouble(cPrice);
                                    break;
                                }
                            }
                        }
                        getProductPricesByXml_OLDRet = nPrice;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "getProductPricesByXml", ex, "", cProcessInfo, gbDebug);
                    }

                    return getProductPricesByXml_OLDRet;
                }

                public double getProductPricesByXml(string cXml)
                {
                    myWeb.PerfMon.Log("Cart", "getProductPricesByXml");
                    string cGroupXPath = string.Empty;
                    var oProd = myWeb.moPageXml.CreateNode(XmlNodeType.Document, "", "product");
                    XmlNode oDefaultPrice;

                    try
                    {
                        // Load product xml
                        oProd.InnerXml = cXml;
                        // Get the Default Price - note if it does not exist, then this is menaingless.
                        oDefaultPrice = oProd.SelectSingleNode("Content/Prices/Price[@default='true']");
                        // here we are checking if the price is the correct currency
                        // then if we are logged in we will also check if it belongs to
                        // one of the user's groups, just looking for 
                        string cGroups = mcGroups;
                        if (string.IsNullOrEmpty(cGroups))
                            cGroups += "default,all,Standard,standard";
                        else
                            cGroups += ",default,all,Standard,standard";
                        cGroups = " and ( contains(@validGroup,'" + Strings.Replace(cGroups, ",", "') or contains(@validGroup,'");
                        cGroups += "') or not(@validGroup) or @validGroup='')";

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["mcCurrency"], "", false)))
                        {
                            myWeb.moSession["mcCurrency"] = moCartConfig["currency"];
                        }
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["mcCurrency"], "", false)))
                            myWeb.moSession["mcCurrency"] = "GBP";

                        string cxpath = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Content/Prices/Price[(@currency='", myWeb.moSession["mcCurrency"]), "') "), cGroups), " ][1]"));

                        XmlElement oThePrice = (XmlElement)oDefaultPrice;
                        double nPrice = 0.0d;
                        foreach (XmlElement oPNode in oProd.SelectNodes(cxpath))
                        {
                            if (oThePrice != null)
                            {
                                if (Information.IsNumeric(oThePrice.InnerText))
                                {
                                    if (Conversions.ToDouble(oPNode.InnerText) < Conversions.ToDouble(oThePrice.InnerText))
                                    {
                                        oThePrice = oPNode;
                                        nPrice = Conversions.ToDouble(oThePrice.InnerText);
                                    }
                                }
                                else
                                {
                                    oThePrice = oPNode;
                                    nPrice = Conversions.ToDouble(oThePrice.InnerText);
                                }
                            }
                            else
                            {
                                oThePrice = oPNode;
                                if (Information.IsNumeric(oThePrice.InnerText))
                                {
                                    nPrice = Conversions.ToDouble(oThePrice.InnerText);
                                }
                            }
                        }

                        return nPrice;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "getProductPricesByXml", ex, "", "", gbDebug);
                    }

                    return default;

                }

                private void applyDiscountsToPriceXml(XmlElement oContentElmt, XmlElement oDiscountElmt)
                {

                    try
                    {
                        // myWeb.PerfMon.Log("Cart", "applyDiscountsToPriceXml")

                        string cGroups = mcGroups;
                        if (string.IsNullOrEmpty(cGroups))
                            cGroups += "default,all,Standard,standard";
                        else
                            cGroups += ",default,all,Standard,standard";
                        string cGroupsXp;
                        cGroupsXp = " and ( contains(@validGroup,'" + Strings.Replace(cGroups, ",", "') or contains(@validGroup,'");
                        cGroupsXp += "') or not(@validGroup) or @validGroup='')";

                        cGroupsXp = "";

                        // Dim cxpath As String = "Prices/Price[@currency='" & mcCurrency & "' " & cGroupsXp & " ]"

                        string cxpath = "Prices/Price[@currency='" + mcCurrency + "' and node()!='' " + cGroupsXp + " ]";



                        foreach (XmlElement oPNode in oContentElmt.SelectNodes(cxpath))
                        {

                            if (oPNode.GetAttribute("originalPrice").Equals(""))
                            {
                                oPNode.SetAttribute("originalPrice", oPNode.InnerText);
                                // End If
                            }

                            if (oDiscountElmt.GetAttribute("isPercent").Equals("0"))
                            {
                                if (!string.IsNullOrEmpty(oDiscountElmt.GetAttribute("value")))
                                {
                                    // Check it allows strings to be used here
                                    if (!oPNode.InnerText.Equals(""))
                                    {
                                        if (Information.IsNumeric(oPNode.InnerText))
                                        {
                                            oPNode.InnerText = Round(Conversions.ToDouble(oPNode.InnerText) - Conversions.ToDouble(oDiscountElmt.GetAttribute("value")), bForceRoundup: mbRoundUp).ToString();
                                            oDiscountElmt.SetAttribute("saving", Round(Conversions.ToDouble(oDiscountElmt.GetAttribute("value")), bForceRoundup: mbRoundUp).ToString());

                                        }
                                    }
                                }
                            }
                            else if (oDiscountElmt.GetAttribute("isPercent").Equals("1"))
                            {
                                if (!string.IsNullOrEmpty(oDiscountElmt.GetAttribute("value")))
                                {
                                    if (!oPNode.InnerText.Equals(""))
                                    {
                                        if (Information.IsNumeric(oPNode.InnerText))
                                        {
                                            oPNode.InnerText = Round(Conversions.ToDouble(oPNode.InnerText) - Conversions.ToDouble(oPNode.InnerText) / 100d * Conversions.ToDouble(oDiscountElmt.GetAttribute("value")), bForceRoundup: mbRoundUp).ToString();
                                            oDiscountElmt.SetAttribute("saving", Round(Conversions.ToDouble(oPNode.InnerText) / 100d * Conversions.ToDouble(oDiscountElmt.GetAttribute("value")), bForceRoundup: mbRoundUp).ToString());
                                        }
                                    }
                                }
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "applyDiscountsToPriceXml", ex, "", "", gbDebug);
                    }
                }


                public string getGroupsByName()
                {
                    myWeb.PerfMon.Log("Discount", "getGroupsByName");
                    string cReturn = "";
                    DataSet oDs;
                    string cProcessInfo = "";
                    try
                    {
                        myWeb.PerfMon.Log("Discount", "getGroupsByName-start");
                        if (myWeb.mnUserId > 0)
                        {
                            oDs = myWeb.moDbHelper.GetDataSet("select * from tblDirectory g inner join tblDirectoryRelation r on g.nDirKey = r.nDirParentId where r.nDirChildId = " + myWeb.mnUserId, "Groups");
                            if (oDs.Tables["Groups"].Rows.Count > 0)
                            {
                                foreach (DataRow oDr in oDs.Tables["Groups"].Rows)
                                    cReturn = Conversions.ToString(Operators.ConcatenateObject(cReturn + ",", oDr["cDirName"]));
                                cReturn = Strings.Mid(cReturn, 2);
                            }
                        }
                        myWeb.PerfMon.Log("Discount", "getGroupsByName-end");
                        return cReturn;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "getGroupsByName", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                #endregion


                ~Discount()
                {
                }
            }
        }
    }
}