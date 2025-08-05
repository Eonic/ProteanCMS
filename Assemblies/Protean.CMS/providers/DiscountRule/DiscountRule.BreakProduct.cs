using Microsoft.Ajax.Utilities;
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
        public class BreakProduct : DiscountRule.DefaultProvider, IdiscountRuleProvider

        {
            public BreakProduct()
            {
                // do nothing
            }

            public IdiscountRuleProvider Initiate(ref Cms myWeb)
            {
                return this;
            }


            public new void ApplyDiscount(ref XmlElement oCartXML, ref int nPriceCount, ref Cms myWeb, ref string strcFreeShippingMethods, ref string strbFreeGiftBox, bool mbRoundUp, ref Cms.Cart myCart)
            {
                // this will work basic monetary discounts
                myWeb.PerfMon.Log("Discount", "Discount_Break_Product");
                XmlElement oPriceElmt;
                try
                {
                    XmlDocument oDiscountXML = new XmlDocument();
                    oDiscountXML.AppendChild(oDiscountXML.ImportNode(oCartXML, true));
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

                    // now we have the new prices, lets update the original cart xml
                    oCartXML.RemoveAll();
                    oCartXML.AppendChild(oCartXML.OwnerDocument.ImportNode(oDiscountXML.DocumentElement, true));
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, "", "Discount_Break_Product", ex, "", "", gbDebug);
                }
            }
        }
    }
}
