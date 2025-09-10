using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Protean.Providers.DiscountRule.DefaultProvider;
using static Protean.stdTools;

namespace Protean.Providers
{
    namespace DiscountRule
    {
        public interface IdiscountRuleProvider
        {
            IdiscountRuleProvider Initiate(NameValueCollection config);

            bool CheckDiscountApplicable(XmlNode discountNode, ref XmlElement oCartXml, out int ProviderType);

            void UpdateCartXMLwithDiscounts(XmlNode discountNode, XmlElement oCartXML, ref XmlDocument oFinalDiscounts);
            //long CheckAuthenticationResponse(HttpRequest request, HttpSessionState session, HttpResponse response); // returns userid

            void ApplyDiscount(ref XmlDocument oFinalDiscounts, ref int nPriceCount, bool mbRoundUp, ref Cms.Cart myCart, string[] cPriceModifiers, ref int nPromocodeApplyFlag);
            void FinalUpdateCartXMLwithDiscounts(ref XmlElement oCartXML, XmlDocument oDiscountXml, bool mbRoundUp);
            decimal FinalCartUpdateDB(ref XmlElement oCartXML, ref Cms myWeb, bool mbRoundUp, ref Cms.Cart myCart);
        }

        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.Authentication.GetProvider";

            public IdiscountRuleProvider Get(int? ProviderType = null)
            {
                string ExceptionMessage = "";
                try
                {
                    string className = ProviderType.HasValue
                        ? Enum.GetName(typeof(DiscountProviderType), ProviderType.Value)
                        : "DefaultProvider";

                    if (string.IsNullOrEmpty(className)) return null;

                    string fullClassName = $"Protean.Providers.DiscountRule.{className}";
                    Type providerType = Type.GetType(fullClassName, throwOnError: false);
                    if (providerType == null) return null;

                    object instance = Activator.CreateInstance(providerType);

                    return (IdiscountRuleProvider)providerType.InvokeMember(
                        "Initiate",
                        BindingFlags.InvokeMethod,
                        null,
                        instance,
                        new object[] { null }  // config is null
                    );
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref ExceptionMessage, "", "Get", ex, "", "", gbDebug);
                    return null;
                }
            }


        }
        public class DefaultProvider : IdiscountRuleProvider
        {
            private string _Name = "Default";
            public Protean.Cms _myWeb;
            private System.Collections.Specialized.NameValueCollection moConfig;
            public enum DiscountProviderType
            {
                DefaultProvider = 0,   // optional “catch‑all”
                Basic = 1,
                BreakProduct =2,
                X4PriceY = 3,
                CheapestFree = 4,
                BreakGroup = 5  
            }
            System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

            private NameValueCollection _Config;
            public DefaultProvider()
            {
                // do nothing
            }

            public IdiscountRuleProvider Initiate(NameValueCollection config)
            {               
                _Config = config;
                return this;
            }

            public void UpdateCartXMLwithDiscounts(XmlNode discountNode, XmlElement oCartXML, ref XmlDocument oFinalDiscounts)
            {
                // Clone the discount node so original stays untouched
                XmlElement discountEl = (XmlElement)discountNode.CloneNode(true);

                // Move child elements (except cAdditionalXML) to attributes
                foreach (XmlNode child in discountEl.SelectNodes("*[name() != 'cAdditionalXML']").Cast<XmlNode>().ToList())
                {
                    discountEl.SetAttribute(child.Name, child.InnerText.Trim());
                    discountEl.RemoveChild(child);
                }

                // Expand <cAdditionalXML> if present
                XmlNode cAddXml = discountEl.SelectSingleNode("*[local-name()='cAdditionalXML']");
                if (cAddXml != null && !string.IsNullOrWhiteSpace(cAddXml.InnerXml))
                {
                    try
                    {
                        string decodedXml = HttpUtility.HtmlDecode(cAddXml.InnerXml.Trim());
                        string wrappedXml = "<root>" + decodedXml + "</root>";
                        XmlDocument addDoc = new XmlDocument();
                        addDoc.LoadXml(wrappedXml);

                        foreach (XmlNode child in addDoc.DocumentElement.ChildNodes)
                        {
                            if (child.NodeType == XmlNodeType.Text && string.IsNullOrWhiteSpace(child.Value))
                                continue;

                            if (child.NodeType == XmlNodeType.Element)
                            {
                                XmlNode imported = discountEl.OwnerDocument.ImportNode(child, true);
                                discountEl.AppendChild(imported);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error parsing cAdditionalXML: " + ex.Message);
                    }
                    discountEl.RemoveChild(cAddXml);
                }

                // Parse discount rules
                double.TryParse(discountEl.GetAttribute("nDiscountMinPrice"), out double dMinPrice);
                double.TryParse(discountEl.GetAttribute("nDiscountMaxPrice"), out double dMaxPrice);
                XmlNode maxNode = discountEl.SelectSingleNode("nDiscountMaxPrice");
                if (dMaxPrice == 0 && maxNode != null)
                {
                    double.TryParse(maxNode.InnerText, out dMaxPrice);
                }
                int.TryParse(discountEl.GetAttribute("nDiscountMinQuantity"), out int nMinQty);
                double.TryParse(discountEl.SelectSingleNode("nMinimumOrderValue")?.InnerText, out double dMinOrderValue);
                double.TryParse(discountEl.SelectSingleNode("nMaximumOrderValue")?.InnerText, out double dMaxOrderValue);

                bool bApplyToOrder = false;
                var applyNode = discountEl.SelectSingleNode("bApplyToOrder");
                if (applyNode != null && !string.IsNullOrWhiteSpace(applyNode.InnerText))
                    bool.TryParse(applyNode.InnerText, out bApplyToOrder);

                // --- Eligibility logic (same as your earlier method) ---
                double totalAmount = 0d;
                int nValidProductCount = 0;
                bool isValid = true;

                List<XmlNode> eligibleItems = new List<XmlNode>();
                XmlNodeList cartItems = oCartXML.SelectNodes("//Item");

                foreach (XmlNode itemNode in cartItems)
                {
                    string parentId = itemNode.SelectSingleNode("nParentId")?.InnerText ?? "0";
                    if (parentId != "0") continue; // skip child items

                    double itemPrice = Convert.ToDouble(itemNode.Attributes["price"]?.Value ?? "0");
                    int itemQty = Convert.ToInt32(itemNode.Attributes["quantity"]?.Value ?? "0");
                    double itemCost = itemPrice * itemQty;

                    totalAmount += itemCost;

                    if (!bApplyToOrder)
                    {
                        bool withinPriceRange = dMaxPrice > 0? (itemCost >= dMinPrice && itemCost <= dMaxPrice): (itemCost >= dMinPrice);
                        if (withinPriceRange)
                        {
                            nValidProductCount += itemQty; // count total quantity
                            eligibleItems.Add(itemNode);
                        }
                    }
                }

                // Quantity check (for item-level discounts)
                if (!bApplyToOrder && nValidProductCount < nMinQty)
                    isValid = false;

                // Order total check
                if (bApplyToOrder)
                {
                    if (dMaxOrderValue > 0)
                    {
                        if (!(totalAmount >= dMinOrderValue && totalAmount <= dMaxOrderValue))
                            isValid = false;
                    }
                    else
                    {
                        if (totalAmount < dMinOrderValue)
                            isValid = false;
                    }

                    if (isValid)
                    {
                        // If order is valid, apply discount to all parent items
                        eligibleItems.Clear();
                        foreach (XmlNode item in cartItems)
                        {
                            string parentId = item.SelectSingleNode("nParentId")?.InnerText ?? "0";
                            if (parentId == "0") eligibleItems.Add(item);
                        }
                    }
                }

                // --- Apply discount only if valid ---
                if (isValid)
                {
                    foreach (XmlNode eligibleItem in eligibleItems)
                    {
                        double itemPrice = Convert.ToDouble(eligibleItem.Attributes["price"]?.Value ?? "0");
                        int itemQty = Convert.ToInt32(eligibleItem.Attributes["quantity"]?.Value ?? "0");
                        double itemCost = itemPrice * itemQty;

                        // 🔥 Double-check price range per item here
                        bool withinPriceRange = dMaxPrice > 0
                            ? (itemCost >= dMinPrice && itemCost <= dMaxPrice)
                            : (itemCost >= dMinPrice);

                        if (!withinPriceRange)
                            continue; // skip items not matching this discount’s range

                        XmlElement itemCopy = (XmlElement)oFinalDiscounts.ImportNode(eligibleItem, true);

                        // Remove existing discounts if necessary
                        foreach (XmlNode existingDiscount in itemCopy.SelectNodes("Discount").Cast<XmlNode>().ToList())
                        {
                            itemCopy.RemoveChild(existingDiscount);
                        }

                        // Attach only the correct discount
                        XmlNode importedDiscount = oFinalDiscounts.ImportNode(discountEl, true);
                        itemCopy.AppendChild(importedDiscount);

                        oFinalDiscounts.DocumentElement.AppendChild(itemCopy);
                    }
                }

                //move this block to basic provider - nita added below code after valid discount append to cartxml, just add new attributes for freeshipping and giftbox
                string strcFreeShippingMethods = "";
                string strbFreeGiftBox = "";
                XmlElement orderElement = (XmlElement)oCartXML.SelectSingleNode("/Order"); // root <Order>
                if (orderElement != null)
                {
                    XmlNodeList itemNodes = oFinalDiscounts.SelectNodes("/Discounts/Item");
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
                                orderElement.SetAttribute("NonDiscountedShippingCost", "0");
                                orderElement.SetAttribute("freeShippingMethods", freeShippingNode.InnerText);
                            }

                            // Free Gift Box
                            XmlNode freeGiftBoxNode = itemdiscountNode.SelectSingleNode("bFreeGiftBox");
                            if (freeGiftBoxNode != null && !string.IsNullOrEmpty(freeGiftBoxNode.InnerText))
                            {
                                strbFreeGiftBox = freeGiftBoxNode.InnerText;
                                orderElement.SetAttribute("bFreeGiftBox", freeGiftBoxNode.InnerText);
                            }
                        }
                    }
                }
            }

            //this method only return discount is applicable or not with matching Provider type
            public bool CheckDiscountApplicable(XmlNode discountNode, ref XmlElement oCartXML, out int ProviderType)
            {

                try
                {

                    bool validateAddedDiscount = false;
                    List<int> validPromoContentIds = new List<int>();
                    double dMinPrice = 0;
                    double discountValue = 0;
                    double dMaxPrice = 0;
                    double dMinOrderTotal = 0;
                    double dMaxOrderTotal = 0;
                    int nMinQuantity = 0;

                    XmlNodeList cartItems = oCartXML.SelectNodes("//Item");

                    ProviderType = Convert.ToInt32(discountNode.SelectSingleNode("nDiscountCat")?.InnerText ?? "0");
                    int discountContentId = Convert.ToInt32(discountNode.SelectSingleNode("nContentId")?.InnerText ?? "0");
                    short bDiscountIsPercent = Convert.ToInt16(discountNode.SelectSingleNode("bDiscountIsPercent")?.InnerText ?? "0");
                    double.TryParse(discountNode.SelectSingleNode("nDiscountMinPrice")?.InnerText, out dMinPrice);
                    int.TryParse(discountNode.SelectSingleNode("nDiscountMinQuantity")?.InnerText, out nMinQuantity);
                    double.TryParse(discountNode.SelectSingleNode("nDiscountValue")?.InnerText, out discountValue);

                    // Get actual XML content
                    string additionalXmlRaw = discountNode.SelectSingleNode("cAdditionalXML")?.InnerXml ?? "";
                    // Decode HTML entities (e.g., &lt; becomes <)
                    string decodedXml = HttpUtility.HtmlDecode(additionalXmlRaw);
                    // Wrap in root and load
                    XmlDocument docAdditionalXml = new XmlDocument();
                    docAdditionalXml.LoadXml("<additionalXml>" + decodedXml + "</additionalXml>");
                    double.TryParse(docAdditionalXml.SelectSingleNode("additionalXml/nDiscountMaxPrice")?.InnerText, out dMaxPrice);
                    double.TryParse(docAdditionalXml.SelectSingleNode("additionalXml/nMinimumOrderValue")?.InnerText, out dMinOrderTotal);
                    double.TryParse(docAdditionalXml.SelectSingleNode("additionalXml/nMaximumOrderValue")?.InnerText, out dMaxOrderTotal);

                    bool bApplyToTotal = false;
                    string applyToOrderText = docAdditionalXml.SelectSingleNode("additionalXml/bApplyToOrder")?.InnerText;
                    bApplyToTotal = !string.IsNullOrEmpty(applyToOrderText) && Convert.ToBoolean(applyToOrderText);
                    string shippingMethods = docAdditionalXml.SelectSingleNode("additionalXml/cFreeShippingMethods")?.InnerText;
                    if (!string.IsNullOrEmpty(shippingMethods))
                    {
                        oCartXML.SetAttribute("NonDiscountedShippingCost", "0");
                    }
                    if (bDiscountIsPercent != default(short))
                    {
                        oCartXML.SetAttribute("bDiscountIsPercent", bDiscountIsPercent + "");
                    }

                    double totalAmount = 0d;
                    int nValidProductCount = 0;
                    bool isValid = true;

                    foreach (XmlNode itemNode in cartItems)
                    {
                        // Skip child/variant items
                        string parentId = itemNode.SelectSingleNode("nParentId")?.InnerText ?? "0";
                        if (parentId != "0") continue;

                        double itemPrice = Convert.ToDouble(itemNode.Attributes["price"]?.Value ?? "0");
                        int itemQty = Convert.ToInt32(itemNode.Attributes["quantity"]?.Value ?? "0");

                        double itemCost = itemPrice * itemQty;
                        totalAmount += itemCost;

                        // Item-level check (if not applying to total order)
                        if (!bApplyToTotal)
                        {
                            if (dMaxPrice > 0)
                            {
                                if (itemCost >= dMinPrice && itemCost <= dMaxPrice)
                                    nValidProductCount++;
                            }
                            else if (itemCost >= dMinPrice)
                            {
                                nValidProductCount++;
                            }
                        }
                    }

                    // Quantity check (item-based)
                    if (!bApplyToTotal && nValidProductCount < nMinQuantity)
                    {
                        isValid = false;
                    }

                    // Order total check
                    if (bApplyToTotal)
                    {
                        if (dMaxOrderTotal > 0)
                        {
                            if (!(totalAmount >= dMinOrderTotal && totalAmount <= dMaxOrderTotal))
                                isValid = false;
                        }
                        else
                        {
                            if (totalAmount < dMinOrderTotal)
                                isValid = false;
                        }
                    }

                    if (isValid)
                        validateAddedDiscount = true;

                    return validateAddedDiscount;
                }
                catch (Exception ex)
                {
                    ProviderType = 0;
                    return false;
                }
            }

            public void ApplyDiscount(ref XmlDocument oFinalDiscounts, ref int nPriceCount, bool mbRoundUp, ref Cms.Cart myCart, string[] cPriceModifiers, ref int nPromocodeApplyFlag)
            {
                throw new NotImplementedException("This method should be overridden in derived classes.");
            }

            public void FinalUpdateCartXMLwithDiscounts(ref XmlElement oCartXML, XmlDocument oDiscountXml, bool mbRoundUp)
            {
                string exceptionMessage = string.Empty;
                try
                {
                    XmlElement oPriceElmt;
                    var nDelIDs = new int[1];
                    decimal nTotalSaved = 0m;

                    // Loop each cart item
                    foreach (XmlElement oItemElmt in oCartXML.SelectNodes("Item"))
                    {
                        int nId = Conversions.ToInteger(oItemElmt.GetAttribute("id"));

                        // original unit price (use existing originalPrice if present, else price)
                        decimal nOriginalUnitPrice = 0m;
                        if (oItemElmt.HasAttribute("originalPrice") && Information.IsNumeric(oItemElmt.GetAttribute("originalPrice")))
                            nOriginalUnitPrice = Conversions.ToDecimal(oItemElmt.GetAttribute("originalPrice"));
                        else
                            nOriginalUnitPrice = priceRound(oItemElmt.GetAttribute("price"), bForceRoundup: mbRoundUp);

                        decimal qty = Conversions.ToDecimal(oItemElmt.GetAttribute("quantity"));
                        decimal originalLineTotal = nOriginalUnitPrice * qty;

                        // Reset / ensure original price attributes
                        oItemElmt.SetAttribute("originalPrice", nOriginalUnitPrice.ToString("0.00"));

                        // Start per-line saving accumulator
                        decimal nLineTotalSaving = 0m;
                        decimal nUnitSaving = 0m;

                        // If there's a DiscountPrice node, read its saving
                        oPriceElmt = (XmlElement)oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/DiscountPrice");
                        if (oPriceElmt != null)
                        {
                            decimal priceNodeTotalSaving = 0m;
                            if (Information.IsNumeric(oPriceElmt.GetAttribute("TotalSaving")))
                                priceNodeTotalSaving = Conversions.ToDecimal(oPriceElmt.GetAttribute("TotalSaving"));

                            nLineTotalSaving += priceNodeTotalSaving;
                        }

                        // Handle DiscountItem modifiers
                        foreach (XmlElement oDiscountItemTest in oDiscountXml.SelectNodes("Discounts/Item[@id=" + nId + "]/DiscountItem"))
                        {
                            decimal discountItemSaving = 0m;
                            if (Information.IsNumeric(oDiscountItemTest.GetAttribute("TotalSaving")))
                                discountItemSaving = Conversions.ToDecimal(oDiscountItemTest.GetAttribute("TotalSaving"));

                            if (discountItemSaving > 0m)
                            {
                                oDiscountItemTest.SetAttribute("AppliedToCart", "1");
                                oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountItemTest.CloneNode(true), true));

                                XmlElement oDiscountInfo = (XmlElement)oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/Discount[@nDiscountKey=" + oDiscountItemTest.GetAttribute("nDiscountKey") + "]");
                                if (oDiscountInfo != null)
                                {
                                    oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountInfo.CloneNode(true), true));
                                }

                                nLineTotalSaving += discountItemSaving;
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
                                    if (discountItemSaving > 0m)
                                    {
                                        oDiscountItemTest.SetAttribute("AppliedToCart", "1");
                                        oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountItemTest.CloneNode(true), true));

                                        XmlElement oDiscountInfo = (XmlElement)oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/Discount[@nDiscountKey=" + oDiscountItemTest.GetAttribute("nDiscountKey") + "]");
                                        if (oDiscountInfo != null)
                                        {
                                            oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountInfo.CloneNode(true), true));
                                        }

                                        nLineTotalSaving += discountItemSaving;
                                    }
                                }
                            }
                        }

                        // Per-item saving attributes
                        if (qty > 0)
                            nUnitSaving = (nLineTotalSaving / qty);

                        // Calculate discounted values
                        decimal discountedLineTotal = originalLineTotal - nLineTotalSaving;
                        decimal discountedUnitPrice = (qty > 0 ? discountedLineTotal / qty : 0);

                        //// Check if this item has a Product Break discount applied
                        //bool isProductBreak = oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/Discount[@nDiscountCat=2]") != null;
                        //// Check if this item has an "X for the price of Y" discount
                        //bool isXforY = oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/Discount[@nDiscountCat=3]") != null;

                        //if (isProductBreak)
                        //{
                        //    // Keep normal prices for line
                        //    oItemElmt.SetAttribute("price", nOriginalUnitPrice.ToString("0.00"));
                        //    oItemElmt.SetAttribute("itemTotal", originalLineTotal.ToString("0.00"));
                        //}
                        //else if(isXforY)
                        //{
                        //    // Do not overwrite item price or totals
                        //    oItemElmt.SetAttribute("price", nOriginalUnitPrice.ToString("0.00"));
                        //    oItemElmt.SetAttribute("itemTotal", originalLineTotal.ToString("0.00"));

                        //    // But store the savings separately
                        //    oItemElmt.SetAttribute("unitSaving", nUnitSaving.ToString("0.00"));
                        //    oItemElmt.SetAttribute("itemSaving", nLineTotalSaving.ToString("0.00"));
                        //    oItemElmt.SetAttribute("discount", nLineTotalSaving.ToString("0.00"));

                        //    // Total saved still accumulates correctly
                        //    nTotalSaved += nLineTotalSaving;
                        //}
                        //else
                        //{
                        //    // Normal behavior for all other discount types
                        //    oItemElmt.SetAttribute("price", discountedUnitPrice.ToString("0.00"));
                        //    oItemElmt.SetAttribute("itemTotal", discountedLineTotal.ToString("0.00"));
                        //}

                        // need to set attributes for Item node - catch it from discountprice node.
                        oItemElmt.SetAttribute("price", discountedUnitPrice.ToString("0.00"));
                        oItemElmt.SetAttribute("itemTotal", discountedLineTotal.ToString("0.00"));
                        // Store savings
                        oItemElmt.SetAttribute("unitSaving", nUnitSaving.ToString("0.00"));
                        oItemElmt.SetAttribute("itemSaving", nLineTotalSaving.ToString("0.00"));
                        oItemElmt.SetAttribute("discount", nLineTotalSaving.ToString("0.00"));

                        // accumulate into order-level discount total
                        nTotalSaved += nLineTotalSaving;

                        // Append applied Discount nodes
                        foreach (XmlElement oDiscountElmt in oDiscountXml.SelectNodes("Discounts/Item[@id=" + nId + "]/Discount[((@nDiscountCat=1 or @nDiscountCat=2) and @Applied=1) or (@nDiscountCat=3) or (@nDiscountCat=5)]"))
                        {
                            oDiscountElmt.SetAttribute("AppliedToCart", "1");
                            oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oDiscountElmt.CloneNode(true), true));
                        }
                        XmlElement oTmpElmt = (XmlElement)oDiscountXml.SelectSingleNode("Discounts/Item[@id=" + nId + "]/DiscountPrice");
                        if (oTmpElmt != null)
                        {
                            oItemElmt.AppendChild(oItemElmt.OwnerDocument.ImportNode(oTmpElmt.CloneNode(true), true));
                        }
                    }

                    // Clean up unwanted discount nodes
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

                    // Write order-level discount total
                    oCartXML.SetAttribute("discountTotal", nTotalSaved.ToString("0.00"));
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref exceptionMessage, "", "Discount_ApplyToCart", ex, "", "", gbDebug);
                }
            }

            public decimal FinalCartUpdateDB(ref XmlElement oCartXML, ref Cms myWeb, bool mbRoundUp, ref Cms.Cart myCart)
            {
                var strSQL = new System.Text.StringBuilder();
                moConfig = myWeb.moConfig;

                string strbFreeGiftBox = oCartXML.GetAttribute("bFreeGiftBox");

                // Handle free giftbox discount
                foreach (XmlElement oItemLoop in oCartXML.SelectNodes("/Order/Item"))
                {
                    if (!string.IsNullOrEmpty(strbFreeGiftBox) && oItemLoop.SelectSingleNode("DiscountPrice") != null)
                    {
                        decimal AmountToDiscount = Convert.ToDecimal(oItemLoop.GetAttribute("discount"));
                        myCart.updatePackagingForFreeGiftDiscount(oItemLoop.GetAttribute("id"), AmountToDiscount);

                        if (moConfig["GiftBoxDiscount"] != null && moConfig["GiftBoxDiscount"] == "on")
                        {
                            strSQL = new System.Text.StringBuilder();
                            string sSql = "SELECT nShippingMethodId FROM tblCartOrder WHERE nCartOrderKey=" + myCart.mnCartId;
                            DataSet oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");

                            if (moConfig["eShippingMethodId"] != null && moConfig["DefaultShippingMethodId"] != null)
                            {
                                if (Convert.ToInt32(oDs.Tables[0].Rows[0]["nShippingMethodId"]) == Convert.ToInt32(moConfig["eShippingMethodId"]))
                                {
                                    myCart.updateGCgetValidShippingOptionsDS(moConfig["DefaultShippingMethodId"]);
                                }
                            }
                        }
                    }
                }

                // Handle free shipping update after promocode
                string strcFreeShippingMethods = oCartXML.GetAttribute("freeShippingMethods");
                if (!string.IsNullOrEmpty(strcFreeShippingMethods))
                {
                    myCart.updateGCgetValidShippingOptionsDS(strcFreeShippingMethods);
                }

                // Calculate total discount properly
                decimal nTotalSaved = 0;
                foreach (XmlElement oItemElmt in oCartXML.SelectNodes("Item"))
                {
                    int nId;
                    if (!int.TryParse(oItemElmt.GetAttribute("id"), out nId))
                        continue; // skip synthetic rows (like DISCOUNT rows)

                    XmlElement oPriceElmt = (XmlElement)oItemElmt.SelectSingleNode("DiscountPrice");
                    if (oPriceElmt != null)
                    {
                        decimal nLineTotalSaving = Convert.ToDecimal(oPriceElmt.GetAttribute("TotalSaving"));
                        nTotalSaved += nLineTotalSaving; // accumulate savings

                        string cUpdtSQL = "UPDATE tblCartItem SET nDiscountValue = " + nLineTotalSaving + " WHERE nCartItemKey = " + nId;
                        myWeb.moDbHelper.ExeProcessSql(cUpdtSQL);
                    }
                }

                // keep XML consistent with DB
                oCartXML.SetAttribute("discountTotal", nTotalSaved.ToString("0.00"));

                return nTotalSaved;
            }

            public static decimal priceRound(object nNumber,int nDecimalPlaces = 2,int nSplitNo = 5, bool bForceRoundup = false, bool bForceRoundDown = false)
            {
                try
                {
                    if (!Information.IsNumeric(nNumber))
                        return 0m;

                    decimal value = Convert.ToDecimal(nNumber);

                    if (bForceRoundup)
                    {
                        // Custom RoundUp logic inline
                        string sVal = value.ToString(System.Globalization.CultureInfo.InvariantCulture);

                        // no decimal places
                        if (!sVal.Contains("."))
                            return value;

                        string[] parts = sVal.Split('.');
                        int nWholeNo = Convert.ToInt32(parts[0]);
                        string decimals = parts[1];
                        if (decimals.Length <= nDecimalPlaces)
                            return value; // already has fewer decimals

                        int nTotalLength = decimals.Length;
                        int nCarry = 0;

                        for (int nI = 0; nI <= (nTotalLength - nDecimalPlaces); nI++)
                        {
                            int nCurrent = Convert.ToInt32(decimals.Substring(nTotalLength - nI - 1, 1));
                            nCurrent += nCarry;
                            nCarry = (nCurrent >= nSplitNo) ? 1 : 0;
                        }

                        int nDecimal = Convert.ToInt32(decimals.Substring(0, nDecimalPlaces));
                        nDecimal += nCarry;

                        if (nDecimal.ToString().Length > nDecimalPlaces)
                        {
                            nCarry = 1;
                            nDecimal = Convert.ToInt32(nDecimal.ToString().Substring(nDecimal.ToString().Length - nDecimalPlaces));
                        }
                        else
                        {
                            nCarry = 0;
                        }

                        nWholeNo += nCarry;

                        string finalVal = nWholeNo + "." + nDecimal.ToString().PadLeft(nDecimalPlaces, '0');
                        return Convert.ToDecimal(finalVal);
                    }
                    else if (bForceRoundDown)
                    {
                        return Math.Floor(value * (decimal)Math.Pow(10, nDecimalPlaces)) / (decimal)Math.Pow(10, nDecimalPlaces);
                    }
                    else
                    {
                        return Math.Round(value, nDecimalPlaces, MidpointRounding.ToEven);
                    }
                }
                catch
                {
                    return 0m;
                }
            }

        }
    }
}

