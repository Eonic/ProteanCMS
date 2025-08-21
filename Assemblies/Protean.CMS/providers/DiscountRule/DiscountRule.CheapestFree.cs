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

            public IdiscountRuleProvider Initiate(ref Cms myWeb)
            {
                return this;
            }

            public new void ApplyDiscount(ref XmlDocument oFinalDiscounts, ref int nPriceCount, bool mbRoundUp, ref Cms.Cart myCart, string[] cPriceModifiers, ref int nPromocodeApplyFlag)
            {
                string exceptionMessage = string.Empty;
                //myWeb.PerfMon.Log("Discount", "Discount_CheapestFree");
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
                    foreach (XmlElement oDiscountLoop in oFinalDiscounts.SelectNodes("descendant-or-self::Discount[@nDiscountCat='4']"))
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

                        oCurDiscount = (XmlElement)oFinalDiscounts.SelectSingleNode("Discounts/Item/Discount[@nDiscountKey=" + oIDs[nI] + "]");
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

                        var aPriceArray = new double[oFinalDiscounts.SelectNodes("Discounts/Item[Discount/@nDiscountKey=" + oIDs[nI] + "]").Count];
                        // Step through each product in the cart associated with current rule
                        foreach (XmlElement currentOItemLoop in oFinalDiscounts.SelectNodes("Discounts/Item[Discount/@nDiscountKey=" + oIDs[nI] + "]"))
                        {
                            oItemLoop = currentOItemLoop;
                            // bool bAllowedDiscount = false;
                            // Dim nCurrentUnitPrice As Decimal = oItemLoop.GetAttribute("price")

                            // NB 16/02/2010
                            // Time to pull price out so we can round it, to avoid the multiple decimal place issues
                            decimal nCurrentUnitPrice;
                            nCurrentUnitPrice = priceRound(oItemLoop.GetAttribute("price"), bForceRoundup: mbRoundUp);


                            oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("DiscountPrice");
                            if (oPriceElmt is null)
                            {
                                oPriceElmt = oFinalDiscounts.CreateElement("Item/DiscountPrice");
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
                                    foreach (XmlElement currentOItemLoop1 in oFinalDiscounts.SelectNodes("Discounts/Item[DiscountPrice/@UnitPrice=" + aPriceArray[i] + "]"))
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

                                        oDiscount = oFinalDiscounts.CreateElement("DiscountItem");
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
                                        nUnitPriceDiscounted = priceRound(nUnitPriceDiscounted, bForceRoundup: mbRoundUp);

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
                    stdTools.returnException(ref exceptionMessage, "", "Discount_CheapestFree", ex, "", "", gbDebug);
                }

            }
        }
    }
}
