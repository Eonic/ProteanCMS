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

            public IdiscountRuleProvider Initiate(ref Cms myWeb)
            {
                return this;
            }

            public new void ApplyDiscount(ref XmlElement oCartXML, ref int nPriceCount, ref Cms myWeb, ref string strcFreeShippingMethods, ref string strbFreeGiftBox, bool mbRoundUp, ref Cms.Cart myCart)
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
                    XmlDocument oDiscountXML = new XmlDocument();
                    oDiscountXML.AppendChild(oDiscountXML.ImportNode(oCartXML, true));
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
                    stdTools.returnException(ref myWeb.msException, "", "Discount_Break Group", ex, "", "", gbDebug);
                }
            }
        }
    }
}
