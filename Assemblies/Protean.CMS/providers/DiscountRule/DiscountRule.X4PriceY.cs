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
        public class X4PriceY : DiscountRule.DefaultProvider, IdiscountRuleProvider
        {
            private System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");

            public X4PriceY()
            {
                // do nothing
            }

            public IdiscountRuleProvider Initiate(ref Cms myWeb)
            {
                return this;
            }

            public new void ApplyDiscount(ref XmlDocument oFinalDiscounts, ref int nPriceCount, ref string strcFreeShippingMethods, ref string strbFreeGiftBox, bool mbRoundUp, ref Cms.Cart myCart, string[] cPriceModifiers, ref int nPromocodeApplyFlag)
            {
                string exceptionMessage = string.Empty;
                // this will work basic discount discounts
                //myWeb.PerfMon.Log("Discount", "Discount_XForPriceY");
                XmlElement oPriceElmt;
                try
                {                    
                    foreach (XmlElement oItemLoop in oFinalDiscounts.SelectNodes("Discounts/Item"))
                    {
                        oPriceElmt = (XmlElement)oItemLoop.SelectSingleNode("DiscountPrice");
                        if (oPriceElmt is null)
                        {
                            // NB 16/02/2010
                            // Time to pull price out so we can round it, to avoid the multiple decimal place issues
                            decimal nPrice;
                            nPrice = Round(oItemLoop.GetAttribute("price"), bForceRoundup: mbRoundUp);

                            oPriceElmt = oFinalDiscounts.CreateElement("Item/DiscountPrice");
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
                                var oDiscount = oFinalDiscounts.CreateElement("DiscountItem");
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
                    stdTools.returnException(ref exceptionMessage, "", "Discount_xForPriceY", ex, "", "", gbDebug);
                }
            }
        }
    }
}
