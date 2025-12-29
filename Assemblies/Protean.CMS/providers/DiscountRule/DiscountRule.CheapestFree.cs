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
        public class CheapestFree : DiscountRule.DefaultProvider, IdiscountRuleProvider
        {
            private System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

            public CheapestFree()
            {
                // do nothing
            }

            public new IdiscountRuleProvider Initiate(NameValueCollection config)
            {
                return this;
            }

            public new void ApplyDiscount(ref XmlDocument oFinalDiscounts, ref int nPriceCount, bool mbRoundUp, ref Cms.Cart myCart, string[] cPriceModifiers, ref int nPromocodeApplyFlag, ref XmlElement oCartXML)
            {
                string exceptionMessage = string.Empty;
                XmlElement oPriceElmt = null;
                XmlElement oDiscount = null;

                try
                {
                    // Collect discount keys
                    string cIDs = ",";
                    foreach (XmlElement oDiscountLoop in oFinalDiscounts.SelectNodes("descendant-or-self::Discount[@nDiscountCat='4']"))
                    {
                        if (!cIDs.Contains("," + oDiscountLoop.GetAttribute("nDiscountKey") + ","))
                        {
                            cIDs += oDiscountLoop.GetAttribute("nDiscountKey") + ",";
                        }
                    }

                    if (cIDs == ",")
                        return;

                    // Trim leading/trailing commas and split
                    cIDs = cIDs.Trim(',');
                    string[] oIDs = cIDs.Split(',');

                    // Process each discount rule
                    for (int nI = 0; nI < oIDs.Length; nI++)
                    {
                        XmlElement oCurDiscount = (XmlElement)oFinalDiscounts
                            .SelectSingleNode($"Discounts/Item/Discount[@nDiscountKey={oIDs[nI]}]");

                        if (oCurDiscount == null) continue;

                        // Discount rule values
                        decimal nDiscountValue = Convert.ToDecimal(oCurDiscount.GetAttribute("nDiscountValue"));
                        bool bDiscountIsPercent = Convert.ToBoolean(oCurDiscount.GetAttribute("bDiscountIsPercent"));

                        int nMinItems = Tools.Number.IsNumeric(oCurDiscount.GetAttribute("nDiscountMinQuantity"))
                            ? Convert.ToInt16(oCurDiscount.GetAttribute("nDiscountMinQuantity"))
                            : 1;

                        decimal nDiscountMinPrice = Tools.Number.IsNumeric(oCurDiscount.GetAttribute("nDiscountMinPrice"))
                            ? Convert.ToDecimal(oCurDiscount.GetAttribute("nDiscountMinPrice"))
                            : 0m;

                        decimal nDiscountMaxPrice = decimal.MaxValue;
                        var maxNode = oCurDiscount.SelectSingleNode("nDiscountMaxPrice");
                        if (maxNode != null && Tools.Number.IsNumeric(maxNode.InnerText) && Convert.ToDecimal(maxNode.InnerText) > 0)
                        {
                            nDiscountMaxPrice = Convert.ToDecimal(maxNode.InnerText);
                        }

                        // Collect eligible items
                        var items = oFinalDiscounts.SelectNodes($"Discounts/Item[Discount/@nDiscountKey={oIDs[nI]}]");
                        double[] aPriceArray = new double[items.Count];

                        int i = 0;
                        int nQualifyingItemsQty = 0;

                        foreach (XmlElement oItemLoop in items)
                        {
                            decimal nCurrentUnitPrice = priceRound(oItemLoop.GetAttribute("price"), bForceRoundup:mbRoundUp);

                            // Ensure <DiscountPrice> exists
                            oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("Item/DiscountPrice");
                            if (oPriceElmt == null)
                            {
                                oPriceElmt = oFinalDiscounts.CreateElement("DiscountPrice");
                                oPriceElmt.SetAttribute("discountId", oIDs[nI]);
                                oPriceElmt.SetAttribute("OriginalUnitPrice", nCurrentUnitPrice.ToString());
                                oPriceElmt.SetAttribute("UnitPrice", nCurrentUnitPrice.ToString());
                                oPriceElmt.SetAttribute("Units", oItemLoop.GetAttribute("quantity"));
                                oPriceElmt.SetAttribute("Total", (nCurrentUnitPrice * Convert.ToDecimal(oItemLoop.GetAttribute("quantity"))).ToString());
                                oPriceElmt.SetAttribute("UnitSaving", "0");
                                oPriceElmt.SetAttribute("TotalSaving", "0");
                                oItemLoop.AppendChild(oPriceElmt);
                            }

                            // Check eligibility
                            if (nCurrentUnitPrice > nDiscountMinPrice)
                            {
                                nQualifyingItemsQty += Convert.ToInt16(oPriceElmt.GetAttribute("Units"));
                            }

                            // Add price to array for sorting
                            aPriceArray[i++] = (double)nCurrentUnitPrice;
                        }

                        // Apply discount (Cheapest Free logic)
                        if (aPriceArray.Length > 0)
                        {
                            Array.Sort(aPriceArray);
                            //int itemsToDiscount = (int)Math.Floor(nQualifyingItemsQty / (double)nMinItems);
                            int itemsToDiscount = nQualifyingItemsQty / (nMinItems + 1);
                            decimal nLastPrice = 0m;

                            for (i = 0; i < aPriceArray.Length && itemsToDiscount > 0; i++)
                            {
                                double price = aPriceArray[i];
                                if (price < (double)nLastPrice || price > (double)nDiscountMaxPrice)
                                    continue;

                                foreach (XmlElement oItemLoop in oFinalDiscounts.SelectNodes($"Discounts/Item[DiscountPrice/@UnitPrice={price}]"))
                                {
                                    oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("DiscountPrice");
                                    nLastPrice = Convert.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));

                                    int unitCount = Convert.ToInt16(oPriceElmt.GetAttribute("Units"));
                                    int discountableUnits = Math.Min(itemsToDiscount, unitCount);
                                    itemsToDiscount -= discountableUnits;

                                    decimal nUnitPriceOriginal = Convert.ToDecimal(oPriceElmt.GetAttribute("UnitPrice"));
                                    decimal nUnitPriceDiscounted = (nDiscountValue == 100m) ? 0m :
                                        bDiscountIsPercent ? nUnitPriceOriginal * ((100m - nDiscountValue) / 100m) :
                                        Math.Max(0m, nUnitPriceOriginal - nDiscountValue);

                                    nUnitPriceDiscounted = priceRound(nUnitPriceDiscounted, bForceRoundup:mbRoundUp);

                                    // Calculate savings
                                    decimal nTotalSaving = (nUnitPriceOriginal - nUnitPriceDiscounted) * discountableUnits;

                                    // Create DiscountItem node
                                    oDiscount = oFinalDiscounts.CreateElement("DiscountItem");
                                    oDiscount.SetAttribute("nDiscountKey", oIDs[nI]);
                                    oDiscount.SetAttribute("nDiscountCat", "4");
                                    oDiscount.SetAttribute("oldUnits", unitCount.ToString());
                                    oDiscount.SetAttribute("Units", (nDiscountValue == 100m)
                                        ? (unitCount - discountableUnits).ToString()
                                        : unitCount.ToString());
                                    oDiscount.SetAttribute("oldTotal", oPriceElmt.GetAttribute("Total"));
                                    oDiscount.SetAttribute("Total", (Convert.ToDouble(oPriceElmt.GetAttribute("Total")) - (double)nTotalSaving).ToString());
                                    oDiscount.SetAttribute("TotalSaving", nTotalSaving.ToString());

                                    // Update <DiscountPrice>
                                    int discountedUnits = discountableUnits;
                                    int chargedUnits = unitCount - discountedUnits;

                                    decimal newTotal = priceRound(
                                        (nUnitPriceDiscounted * discountedUnits) + (nUnitPriceOriginal * chargedUnits),
                                        bForceRoundup: mbRoundUp
                                    );

                                    oPriceElmt.SetAttribute("UnitSaving", (nUnitPriceOriginal - nUnitPriceDiscounted).ToString());
                                    oPriceElmt.SetAttribute("TotalSaving", ((nUnitPriceOriginal - nUnitPriceDiscounted) * discountedUnits).ToString());
                                    oPriceElmt.SetAttribute("Total", newTotal.ToString());

                                    if (unitCount == 1)
                                        oPriceElmt.SetAttribute("UnitPrice", nUnitPriceDiscounted.ToString());

                                    oItemLoop.AppendChild(oDiscount);                                    
                                }
                                // Update all cart items after applying discounts
                                foreach (XmlElement oItemLoop in oFinalDiscounts.SelectNodes("//Item"))
                                {
                                    int nId = Convert.ToInt16(oItemLoop.GetAttribute("id"));
                                    XmlElement cartItem = (XmlElement)oCartXML.SelectSingleNode($"Item[@id='{nId}']");
                                    if (cartItem == null) continue;

                                    // Get DiscountPrice if exists, else fallback to original price
                                    XmlElement discountPrice = (XmlElement)oItemLoop.SelectSingleNode("DiscountPrice");

                                    decimal originalUnitPrice = Convert.ToDecimal(oItemLoop.GetAttribute("price"));
                                    decimal unitPrice = originalUnitPrice;
                                    int quantity = Convert.ToInt16(oItemLoop.GetAttribute("quantity"));
                                    decimal unitSaving = 0m;
                                    decimal totalSaving = 0m;

                                    if (discountPrice != null)
                                    {
                                        unitPrice = Convert.ToDecimal(discountPrice.GetAttribute("UnitPrice"));
                                        unitSaving = Convert.ToDecimal(discountPrice.GetAttribute("UnitSaving"));
                                        totalSaving = Convert.ToDecimal(discountPrice.GetAttribute("TotalSaving"));
                                    }

                                    // Calculate total for this item
                                    decimal itemTotal = priceRound((unitPrice * quantity), bForceRoundup: mbRoundUp);

                                    // Update cart XML attributes
                                    cartItem.SetAttribute("originalPrice", originalUnitPrice.ToString("0.00"));
                                    cartItem.SetAttribute("price", unitPrice.ToString("0.00"));
                                    cartItem.SetAttribute("itemTotal", itemTotal.ToString("0.00"));
                                    cartItem.SetAttribute("unitSaving", unitSaving.ToString("0.00"));
                                    cartItem.SetAttribute("itemSaving", totalSaving.ToString("0.00"));
                                    cartItem.SetAttribute("discount", totalSaving.ToString("0.00"));
                                }


                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref exceptionMessage, "", "Discount_CheapestFree", ex, "", "", gbDebug);
                }
            }

        }
    }
}
