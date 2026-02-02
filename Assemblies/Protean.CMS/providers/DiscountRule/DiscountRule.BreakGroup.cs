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
        public class BreakGroup : DiscountRule.DefaultProvider, IdiscountRuleProvider
        {
            private System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

            public BreakGroup()
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

                try
                {
                    XmlElement oDiscount;
                    XmlElement oPriceElmt;
                    XmlElement oItem;

                    // Collect all discount keys in category 5
                    string cIDs = ",";
                    foreach (XmlElement discountNode in oFinalDiscounts.SelectNodes("descendant-or-self::Discount[@nDiscountCat=5]"))
                    {
                        if (!cIDs.Contains("," + discountNode.GetAttribute("nDiscountKey") + ","))
                        {
                            cIDs += discountNode.GetAttribute("nDiscountKey") + ",";
                        }
                    }

                    if (cIDs == ",") return;

                    // Trim commas and split into array
                    cIDs = cIDs.Trim(',');
                    string[] oIds = cIDs.Split(',');

                    foreach (string discountKey in oIds)
                    {
                        // Process each discount
                        foreach (XmlElement discountNode in oFinalDiscounts.SelectNodes("descendant-or-self::Discount[@nDiscountKey=" + discountKey + "]"))
                        {
                            oDiscount = discountNode;

                            // Find all items eligible for this discount
                            var eligibleItems = oFinalDiscounts.DocumentElement.SelectNodes(
                                "Item[Discount/@nDiscountKey=" + oDiscount.GetAttribute("nDiscountKey") + "]");

                            int totalQuantity = 0;
                            decimal totalValue = 0m;

                            // Ensure DiscountPrice nodes exist and calculate totals
                            foreach (XmlElement itemNode in eligibleItems)
                            {
                                oPriceElmt = (XmlElement)itemNode.SelectSingleNode("Item/DiscountPrice");
                                if (oPriceElmt == null)
                                {
                                    decimal nPrice = priceRound(itemNode.GetAttribute("price"), bForceRoundup: mbRoundUp);
                                    oPriceElmt = oFinalDiscounts.CreateElement("DiscountPrice");
                                    oPriceElmt.SetAttribute("OriginalUnitPrice", nPrice.ToString("0.00"));
                                    oPriceElmt.SetAttribute("UnitPrice", nPrice.ToString("0.00"));
                                    oPriceElmt.SetAttribute("Units", itemNode.GetAttribute("quantity"));
                                    oPriceElmt.SetAttribute("Total", (nPrice * Convert.ToDecimal(itemNode.GetAttribute("quantity"))).ToString());
                                    oPriceElmt.SetAttribute("UnitSaving", "0");
                                    oPriceElmt.SetAttribute("TotalSaving", "0");
                                    itemNode.AppendChild(oPriceElmt);
                                }

                                int qty = Convert.ToInt32(oPriceElmt.GetAttribute("Units"));
                                totalQuantity += qty;
                                totalValue += Convert.ToDecimal(oPriceElmt.GetAttribute("UnitPrice")) * qty;
                            }

                            // Minimum quantities or price for discount
                            int minQuantity = Information.IsNumeric(oDiscount.GetAttribute("nDiscountMinQuantity"))
                                              ? Convert.ToInt32(oDiscount.GetAttribute("nDiscountMinQuantity")) : 0;
                            decimal minPrice = Information.IsNumeric(oDiscount.GetAttribute("nDiscountMinPrice"))
                                               ? Convert.ToDecimal(oDiscount.GetAttribute("nDiscountMinPrice")) : 0;

                            if ((minQuantity > 0 && totalQuantity >= minQuantity) || (minPrice > 0 && totalValue >= minPrice))
                            {
                                // Split discount per item for full groups only
                                int groupSize = minQuantity; // e.g., 2 items per group
                                decimal groupDiscount = Convert.ToDecimal(oDiscount.GetAttribute("nDiscountValue")); // e.g., 15
                                int fullGroups = totalQuantity / groupSize; // only full groups get discount
                                int itemsToDiscount = fullGroups * groupSize;
                                decimal perItemDiscount = groupDiscount / groupSize;

                                int counter = 0;
                                foreach (XmlElement itemNode in eligibleItems)
                                {
                                    oPriceElmt = (XmlElement)itemNode.SelectSingleNode("DiscountPrice");
                                    int qty = Convert.ToInt32(oPriceElmt.GetAttribute("Units"));

                                    // Apply discount only to items in full groups
                                    for (int i = 0; i < qty; i++)
                                    {
                                        decimal originalUnitPrice = Convert.ToDecimal(oPriceElmt.GetAttribute("OriginalUnitPrice"));
                                        decimal discountedUnitPrice = originalUnitPrice;

                                        if (counter < itemsToDiscount)
                                        {
                                            if (Conversions.ToDouble(oDiscount.GetAttribute("bDiscountIsPercent")) == 1d)
                                            {
                                                discountedUnitPrice = priceRound(originalUnitPrice * (100 - Convert.ToDecimal(oDiscount.GetAttribute("nDiscountValue"))) / 100m, bForceRoundup: mbRoundUp);
                                            }
                                            else
                                            {
                                                discountedUnitPrice = originalUnitPrice - perItemDiscount;
                                            }
                                            counter++;
                                        }

                                        decimal totalLine = discountedUnitPrice * qty;
                                        decimal unitSaving = originalUnitPrice - discountedUnitPrice;
                                        decimal lineSaving = unitSaving * qty;

                                        // Create DiscountPriceLine
                                        var oPriceLine = oFinalDiscounts.CreateElement("DiscountPriceLine");
                                        nPriceCount++;
                                        oPriceLine.SetAttribute("nDiscountKey", oDiscount.GetAttribute("nDiscountKey"));
                                        oPriceLine.SetAttribute("PriceOrder", nPriceCount.ToString());
                                        oPriceLine.SetAttribute("UnitPrice", discountedUnitPrice.ToString("0.00"));
                                        oPriceLine.SetAttribute("Total", totalLine.ToString());
                                        oPriceLine.SetAttribute("UnitSaving", unitSaving.ToString("0.00"));
                                        oPriceLine.SetAttribute("TotalSaving", lineSaving.ToString());
                                        oPriceElmt.AppendChild(oPriceLine);

                                        // Update DiscountPrice node
                                        oPriceElmt.SetAttribute("UnitPrice", discountedUnitPrice.ToString("0.00"));
                                        oPriceElmt.SetAttribute("Total", totalLine.ToString());
                                        oPriceElmt.SetAttribute("UnitSaving", unitSaving.ToString("0.00"));
                                        oPriceElmt.SetAttribute("TotalSaving", lineSaving.ToString());

                                        // Update CartXML
                                        var cartItem = (XmlElement)oCartXML.SelectSingleNode("Item[@id='" + itemNode.GetAttribute("id") + "']");
                                        if (cartItem != null)
                                        {
                                            cartItem.SetAttribute("originalPrice", originalUnitPrice.ToString("0.00"));
                                            cartItem.SetAttribute("price", discountedUnitPrice.ToString("0.00"));
                                            cartItem.SetAttribute("itemTotal", totalLine.ToString("0.00"));
                                            cartItem.SetAttribute("unitSaving", unitSaving.ToString("0.00"));
                                            cartItem.SetAttribute("itemSaving", lineSaving.ToString("0.00"));
                                            cartItem.SetAttribute("discount", lineSaving.ToString("0.00"));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref exceptionMessage, "", "Discount_Break Group", ex, "", "", gbDebug);
                }
            }

        }
    }
}
