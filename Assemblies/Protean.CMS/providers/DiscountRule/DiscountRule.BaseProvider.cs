using Microsoft.Ajax.Utilities;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protean.Providers.CDN;
using Protean.Providers.Payment;
using Protean.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml;
using static Protean.Cms;
using static Protean.stdTools;
using static Protean.Tools.Xml;


namespace Protean.Providers
{
    namespace DiscountRule
    {
        public interface IdiscountRuleProvider
        {
            IdiscountRuleProvider Initiate(Cms myWeb, NameValueCollection config);

            bool CheckDiscountApplicable(ref XmlDocument oXmlDiscounts, ref XmlElement oCartXml, out double finalDiscount);

            string ApplyDiscount(ref DataRow oDrDiscount, ref Protean.Cms.Cart oCart);
            //long CheckAuthenticationResponse(HttpRequest request, HttpSessionState session, HttpResponse response); // returns userid

           

        }

        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.Authentication.GetProvider";
          
            public IdiscountRuleProvider Get(ref Cms myWeb)
            {
                try
                {
                    Type calledType;
                    string ProviderClass = "";
                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/discountProviders");
                    if (moPrvConfig != null)
                    {
                        ProviderClass = "";

                        if (string.IsNullOrEmpty(ProviderClass))
                        {
                            ProviderClass = "Protean.Providers.DiscountRule.DefaultProvider";
                            calledType = Type.GetType(ProviderClass, true);
                        }
                        else
                        {
                            if (moPrvConfig.Providers[0].Name != null)
                            {
                                var assemblyInstance = Assembly.Load(moPrvConfig.Providers[0].Type);
                                calledType = assemblyInstance.GetType("Protean.Providers.DiscountRule." + ProviderClass, true);
                            }
                            else
                            {
                                calledType = Type.GetType("Protean.Providers.DiscountRule." + ProviderClass, true);
                            }
                        }

                        var o = Activator.CreateInstance(calledType);
                        var args = new object[2];
                        args[0] = myWeb;
                        args[1] = null; // now passing config as a null

                        return (IdiscountRuleProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args);
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    // TS commented this out as if we have an old payment provider that has been retired we do not want errors.
                    //stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", ProviderName + " Could Not be Loaded", gbDebug);
                    return null;
                }
            }

        }
        public class DefaultProvider : IdiscountRuleProvider
        {
            private string _Name = "Default";
            public Protean.Cms _myWeb;
            System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

            private NameValueCollection _Config;
            public DefaultProvider()
            {
                // do nothing
            }
            
          
            public IdiscountRuleProvider Initiate(Cms myWeb, NameValueCollection config)
            {
                _myWeb = myWeb;
                _Config = config;
                return this;
            }           

            public string ApplyDiscount(ref DataRow oDrDiscount, ref Cart oCart)
            {
                throw new NotImplementedException();
            }

            public bool CheckDiscountApplicable(ref XmlDocument oXmlDiscounts, ref XmlElement oCartXML, out double finalDiscount)
            {
                finalDiscount = 0d;

                try
                {
                    XmlNodeList discountNodes = oXmlDiscounts.SelectNodes("//Discount");
                    bool validateAddedDiscount = false;
                    List<int> validPromoContentIds = new List<int>();
                    double dMinPrice = 0;
                    double discountValue = 0;
                    double dMaxPrice = 0;
                    double dMinOrderTotal = 0;
                    double dMaxOrderTotal = 0;
                    int nMinQuantity = 0;

                    XmlNodeList cartItems = oCartXML.SelectNodes("//Item");

                    foreach (XmlNode discountNode in discountNodes)
                    {
                        int discountContentId = Convert.ToInt32(discountNode.SelectSingleNode("nContentId")?.InnerText ?? "0");
                        short bDiscountIsPercent = Convert.ToInt16(discountNode.SelectSingleNode("bDiscountIsPercent")?.InnerText ?? "0");
                        double.TryParse(discountNode.SelectSingleNode("nDiscountMinPrice")?.InnerText, out dMinPrice);
                        int.TryParse(discountNode.SelectSingleNode("nDiscountMinQuantity")?.InnerText, out nMinQuantity);
                        double.TryParse(discountNode.SelectSingleNode("nDiscountValue")?.InnerText, out discountValue);

                        string additionalXmlRaw = discountNode.SelectSingleNode("cAdditionalXML")?.InnerText ?? "";
                        XmlDocument docAdditionalXml = new XmlDocument();
                        docAdditionalXml.LoadXml("<additionalXml>" + additionalXmlRaw + "</additionalXml>");

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

                        double totalAmount = 0d;
                        double nItemCost = 0d;
                        short nValidProductCount = 0;
                        bool isValid = true;

                        foreach (XmlNode itemNode in cartItems)
                        {
                            string parentId = itemNode.SelectSingleNode("nParentId")?.InnerText ?? "0";
                            if (parentId != "0") continue;

                            int cartContentId = Convert.ToInt32(itemNode.Attributes["contentId"]?.Value ?? "0");
                            if (cartContentId != discountContentId) continue;

                            double itemPrice = Convert.ToDouble(itemNode.Attributes["price"]?.Value ?? "0");
                            double itemQty = Convert.ToDouble(itemNode.Attributes["quantity"]?.Value ?? "0");

                            nItemCost = itemPrice * itemQty;
                            totalAmount += nItemCost;

                            if (!bApplyToTotal)
                            {
                                if (dMaxPrice > 0)
                                {
                                    if (nItemCost >= dMinPrice && nItemCost <= dMaxPrice)
                                        nValidProductCount++;
                                }
                                else if (nItemCost >= dMinPrice)
                                {
                                    nValidProductCount++;
                                }
                            }
                        }

                        if (!bApplyToTotal && nValidProductCount < nMinQuantity)
                        {
                            isValid = false;
                        }

                        if (bApplyToTotal && dMaxOrderTotal != 0d)
                        {
                            if (!(totalAmount >= dMinOrderTotal && totalAmount <= dMaxOrderTotal))
                            {
                                isValid = false;
                            }
                        }

                        if (!isValid)
                        {
                            if (discountNode.ParentNode != null)
                            {
                                discountNode.ParentNode.RemoveChild(discountNode);
                            }
                            continue;
                        }

                        validateAddedDiscount = true;
                        validPromoContentIds.Add(discountContentId);

                        if (bDiscountIsPercent != 0)
                        {
                            finalDiscount += (totalAmount * discountValue) / 100.0;
                        }
                        else
                        {
                            finalDiscount += discountValue;
                        }
                    }

                    // Transform only valid <Discount> nodes after validation
                    // changing cAdditionXMl column mapping logic here to xml only
                    //Here we get fiteredvalid discount in oXmlDiscounts 
                    // we will set oXmlDiscounts xml with oDsCart values into it.
                    foreach (XmlElement discountEl in oXmlDiscounts.SelectNodes("//Discount"))
                    {
                        // Move non-cAdditionalXML child elements to attributes
                        foreach (XmlNode child in discountEl.SelectNodes("*[name() != 'cAdditionalXML']").Cast<XmlNode>().ToList())
                        {
                            discountEl.SetAttribute(child.Name, child.InnerText.Trim());
                            discountEl.RemoveChild(child);
                        }

                        // Expand <cAdditionalXML> to real elements
                        XmlNode cAddXml = discountEl.SelectSingleNode("cAdditionalXML");
                        if (cAddXml != null && !string.IsNullOrWhiteSpace(cAddXml.InnerText))
                        {
                            var addDoc = new XmlDocument();
                            addDoc.LoadXml("<root>" + cAddXml.InnerText + "</root>");
                            foreach (XmlNode addChild in addDoc.DocumentElement.ChildNodes)
                            {
                                if (addChild.NodeType == XmlNodeType.Element)
                                {
                                    XmlElement el = oXmlDiscounts.CreateElement(addChild.Name);
                                    el.InnerText = addChild.InnerText;
                                    discountEl.AppendChild(el);
                                }
                            }
                            discountEl.RemoveChild(cAddXml);
                        }
                    }
                    
                    // Remove child item from carxml that have nparentid>0 in XML
                    XmlNodeList itemNodes = oCartXML.SelectNodes("//Item");
                    for (int i = itemNodes.Count - 1; i >= 0; i--)
                    {
                        XmlNode itemNode = itemNodes[i];
                        string parentId = itemNode.SelectSingleNode("nParentId")?.InnerText ?? "0";

                        if (int.TryParse(parentId, out int nParentId) && nParentId > 0)
                        {
                            itemNode.ParentNode.RemoveChild(itemNode); 
                        }
                    }

                    // select parent items and add them to oDXML
                    XmlDocument oDXML = new XmlDocument();
                    XmlElement oDXmlElement = oDXML.CreateElement("Discounts");
                    oDXML.AppendChild(oDXmlElement);

                    XmlNodeList parentItems = oCartXML.SelectNodes("//Item[nParentId='0']");
                    foreach (XmlNode itemNode in parentItems)
                    {
                        XmlNode importedItem = oDXML.ImportNode(itemNode, true);
                        oDXmlElement.AppendChild(importedItem);
                    }

                    // Now append matching <Discount> nodes from oXmlDiscounts to matching <Item> nodes in oDXML
                    XmlNodeList oDxmlItems = oDXML.SelectNodes("//Item");
                    discountNodes = oXmlDiscounts.SelectNodes("//Discount");

                    foreach (XmlNode itemNode in oDxmlItems)
                    {
                        // Try to get id from <id> element or id attribute
                        string itemId = itemNode.SelectSingleNode("id")?.InnerText ?? itemNode.Attributes["id"]?.Value;

                        if (!string.IsNullOrEmpty(itemId))
                        {
                            foreach (XmlNode discountNode in discountNodes)
                            {
                                string discountRefId = discountNode.Attributes["nCartItemKey"]?.Value;

                                if (discountRefId == itemId)
                                {
                                    XmlNode importedDiscount = oDXML.ImportNode(discountNode, true);
                                    itemNode.AppendChild(importedDiscount);
                                }
                            }
                        }
                    }

                    oDXML.PreserveWhitespace = false;
                    oXmlDiscounts = oDXML;

                    return validateAddedDiscount;
                }
                catch (Exception ex)
                {
                    finalDiscount = 0;
                    return false;
                }
            }

        }
    }
}

