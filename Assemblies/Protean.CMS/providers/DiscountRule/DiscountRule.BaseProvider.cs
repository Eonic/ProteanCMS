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
using static Protean.Cms.Cart.Discount;
using static Protean.Providers.DiscountRule.DefaultProvider;
using static Protean.stdTools;
using static Protean.Tools.Xml;


namespace Protean.Providers
{
    namespace DiscountRule
    {
        public interface IdiscountRuleProvider
        {
            IdiscountRuleProvider Initiate(Cms myWeb, NameValueCollection config);

            bool CheckDiscountApplicable(XmlNode discountNode, ref XmlElement oCartXml, out int ProviderType);

            void UpdateCartXMLwithDiscounts(XmlNode discountNode, ref XmlElement oCartXML, ref Cms myWeb);
            //long CheckAuthenticationResponse(HttpRequest request, HttpSessionState session, HttpResponse response); // returns userid

            void ApplyDiscount(ref XmlElement oCartXML, ref Cms myWeb);

        }

        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.Authentication.GetProvider";

            public IdiscountRuleProvider Get(ref Cms myWeb, int? ProviderType = null)
            {
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
                        new object[] { myWeb, null }  // config is null
                    );
                }
                catch
                {
                    return null;
                }
            }


        }
        public class DefaultProvider : IdiscountRuleProvider
        {
            private string _Name = "Default";
            public Protean.Cms _myWeb;
            public enum DiscountProviderType
            {
                DefaultProvider = 0,   // optional “catch‑all”
                Basic = 1,
                BreakGroup = 2,
                BreakProduct = 3,
                CheapestFree = 4,
                X4PriceY = 5
            }
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

            public void UpdateCartXMLwithDiscounts(XmlNode discountNode, ref XmlElement oCartXML, ref Cms myWeb)
            {
                XmlElement discountEl = (XmlElement)discountNode;

                // Move all child nodes (except cAdditionalXML) to attributes
                foreach (XmlNode child in discountEl.SelectNodes("*[name() != 'cAdditionalXML']").Cast<XmlNode>().ToList())
                {
                    discountEl.SetAttribute(child.Name, child.InnerText.Trim());
                    discountEl.RemoveChild(child);
                }

                // Expand cAdditionalXML if it exists and contains XML content
                XmlNode cAddXml = discountEl.SelectSingleNode("cAdditionalXML");
                if (cAddXml != null && !string.IsNullOrWhiteSpace(cAddXml.InnerText))
                {
                    var addDoc = new XmlDocument();
                    // Wrap with root and load
                    addDoc.LoadXml("<root>" + cAddXml.InnerText + "</root>");

                    foreach (XmlNode addChild in addDoc.DocumentElement.ChildNodes)
                    {
                        if (addChild.NodeType == XmlNodeType.Element)
                        {
                            XmlElement el = discountEl.OwnerDocument.CreateElement(addChild.Name);
                            el.InnerText = addChild.InnerText;
                            discountEl.AppendChild(el);
                        }
                    }
                    discountEl.RemoveChild(cAddXml);
                }

                //Now need to import the discountNode to the oCartXML

                // Get the cart item key from the discount
                XmlElement discountElement = (XmlElement)discountNode;
                string cartItemKey = discountElement.GetAttribute("nCartItemKey");

                if (!string.IsNullOrEmpty(cartItemKey))
                {
                    // Find <Item> node with matching cartItemKey id
                    XmlNode itemNode = oCartXML.SelectSingleNode($"//Item[@id='{cartItemKey}']");

                    if (itemNode != null)
                    {
                        XmlNode importedDiscount = oCartXML.OwnerDocument.ImportNode(discountElement, true);
                        // Append the discount as a child to the matched <Item>
                        itemNode.AppendChild(importedDiscount);
                    }
                }

                //END here final update the oCartXML with the discount is ready to be applied
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

                    ProviderType = Convert.ToInt32(discountNode.SelectSingleNode("nDiscountCodeType")?.InnerText ?? "0");
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
                    if(!isValid)
                    {
                        return false;
                    }
                    validateAddedDiscount = true;
                    return validateAddedDiscount;
                }
                catch (Exception ex)
                {
                    ProviderType = 0;
                    return false;
                }
            }

            public void ApplyDiscount(ref XmlElement oCartXML, ref Cms myWeb)
            {
                throw new NotImplementedException("This method should be overridden in derived classes.");
            }
        }
    }
}

