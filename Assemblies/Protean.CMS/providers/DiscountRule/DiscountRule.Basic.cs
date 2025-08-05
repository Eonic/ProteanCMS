﻿using Microsoft.Ajax.Utilities;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protean.Providers.Authentication;
using Protean.Providers.Payment;
using Protean.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.PeerToPeer;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml;
using static Protean.Cms;
using static Protean.stdTools;


namespace Protean.Providers
{
    namespace DiscountRule
    {

        public class Basic : DiscountRule.DefaultProvider, IdiscountRuleProvider
        {
            private System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
            
            public Basic()
            {
                // do nothing
            }


            public IdiscountRuleProvider Initiate(ref Cms myWeb)
            {
                return this;
            }

            public new void ApplyDiscount(ref XmlElement oCartXML, ref int nPriceCount, ref Cms myWeb, ref string strcFreeShippingMethods, ref string strbFreeGiftBox, bool mbRoundUp, ref Cms.Cart myCart, string[] cPriceModifiers)
            {
                myWeb.PerfMon.Log("Discount", "ApplyDiscount");
                XmlElement oDiscountLoop;
                XmlElement oPriceElmt;
                bool bApplyOnTotal = false;
                double RemainingAmountToDiscount = 0d;
                int nPromocodeApplyFlag = 0;

                try
                {
                    decimal AmountToDiscount = default;
                    XmlDocument oDiscountXML = new XmlDocument();
                    oDiscountXML.AppendChild(oDiscountXML.ImportNode(oCartXML, true));
                    int nCount;
                    var loopTo1 = Information.UBound(cPriceModifiers);
                    for (nCount = 0; nCount <= loopTo1; nCount++)
                    {
                        switch (cPriceModifiers[nCount] ?? "")
                        {
                            case "Basic_Money":
                                {
                                    // loop through the items                                    
                                    foreach (XmlElement oItemLoop in oDiscountXML.SelectNodes("/Order/Item"))
                                    {
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
                                                foreach (XmlElement oDiscountElmt in oDiscountXML.SelectNodes("/Order/Item/Discount[@nDiscountKey=" + oDiscountLoop.GetAttribute("nDiscountKey") + "]"))
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
                                                foreach (XmlElement oDiscountElmt in oDiscountXML.SelectNodes("/Order/Item/Discount[@nDiscountKey=" + oDiscountLoop.GetAttribute("nDiscountKey") + "]"))
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
                                            Discount_Basic_FreeShipping(ref oDiscountXML, ref nPriceCount, ref strcFreeShippingMethods, ref strbFreeGiftBox, mbRoundUp, ref myCart, ref myWeb);
                                        }

                                    }
                                    break;
                                }
                            case "Basic_Percent":
                                {
                                    foreach (XmlElement oItemLoop in oDiscountXML.SelectNodes("/Order/Item"))
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
                                        foreach (XmlElement oDiscountLoop1 in oItemLoop.SelectNodes("Discount[@bDiscountIsPercent=1 and @nDiscountCat=1]"))
                                        {
                                            decimal nNewPrice = Conversions.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));
                                            nNewPrice = Round((double)nNewPrice * ((100d - Conversions.ToDouble(oDiscountLoop1.GetAttribute("nDiscountValue"))) / 100d), bForceRoundup: mbRoundUp);

                                            var oPriceLine = oDiscountXML.CreateElement("DiscountPriceLine");

                                            nPriceCount += 1;
                                            oPriceLine.SetAttribute("nDiscountKey", oDiscountLoop1.GetAttribute("nDiscountKey"));
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

                                            oDiscountLoop1.SetAttribute("Applied", 1.ToString());
                                            nPromocodeApplyFlag = 1;
                                        }
                                    }
                                    // set shipping option after applying promocode
                                    if (!string.IsNullOrEmpty(strcFreeShippingMethods) & nPromocodeApplyFlag == 1)
                                    {
                                        myCart.updateGCgetValidShippingOptionsDS(strcFreeShippingMethods);
                                    }
                                    break;
                                }
                            case "Break_Product":
                                {
                                    // loop through the items
                                    foreach (XmlElement oItemLoop in oDiscountXML.SelectNodes("/Order/Item"))
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
                                    break;
                                }
                        }
                    }

                   

                    // Final cart update
                    oCartXML.RemoveAll();
                    foreach (XmlNode child in oDiscountXML.DocumentElement.ChildNodes)
                    {
                        XmlNode importedChild = oCartXML.OwnerDocument.ImportNode(child, true);
                        oCartXML.AppendChild(importedChild);
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, "", "ApplyDiscount", ex, "", "", gbDebug);
                }
            }

            private void Discount_Basic_FreeShipping(ref XmlDocument oDiscountXML, ref int nPriceCount, ref string cFreeShippingMethods, [Optional, DefaultParameterValue("")] ref string strbFreeGiftBox, bool mbRoundUp, ref Cms.Cart myCart, ref Cms myWeb)
            {
                // this will work basic monetary discounts
               
                XmlElement oPriceElmt;
                bool bApplyOnTotal = false;
                double RemainingAmountToDiscount = 0d;
                try
                {
                    // loop through the items
                    foreach (XmlElement oItemLoop in oDiscountXML.SelectNodes("/Order/Item"))
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
                    stdTools.returnException(ref myWeb.msException, "", "FreeShipping", ex, "", "", gbDebug);
                }
            }

        }
    }
}
