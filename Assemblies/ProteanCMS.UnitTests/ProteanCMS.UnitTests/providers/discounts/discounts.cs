using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protean.Providers.DiscountRule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Web;
using System.Xml;
using static Protean.Cms;
using static Protean.Cms.Cart;


namespace ProteanCMS.UnitTests
{
    [TestClass]
    public class discounts
    {
        public Cart myCart;
        public Cart.Discount moDiscount;

        // Runs discount engine and returns actual cart XML
        public XmlElement RunDiscountTest(string testFolderPath)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\UnitTestCases"));

            string discountXmlPath = Path.Combine(projectRoot, testFolderPath, "discounts.xml");
            string cartXmlPath = Path.Combine(projectRoot, testFolderPath, "cart.xml");

            XmlDocument xDiscounts = new XmlDocument();
            xDiscounts.Load(discountXmlPath);

            XmlDocument xCart = new XmlDocument();
            xCart.Load(cartXmlPath);

            XmlElement oXmlDiscounts = xDiscounts.DocumentElement;
            XmlElement oCartXML = xCart.DocumentElement;
            string appliedCode = "";

            moDiscount = new Protean.Cms.Cart.Discount();
            myCart = new Protean.Cms.Cart();

            XmlElement result = moDiscount.CheckDiscounts(oXmlDiscounts, ref oCartXML, ref appliedCode, myCart);
            return result;
        }       

        // Full XML assert
        public void AssertDiscountResult(string testFolderPath)
        {
            // Run your existing method to get actual + expected XML
            XmlElement actualCartXml = RunDiscountTest(testFolderPath);
            string expectedXmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\UnitTestCases", testFolderPath, "expectedCart.xml");

            XmlDocument expected = new XmlDocument();
            expected.Load(expectedXmlPath);

            XmlDocument CartXML = new XmlDocument();
            CartXML.LoadXml(actualCartXml.OuterXml);

            CompareNodes(expected.DocumentElement, CartXML.DocumentElement, "/");
        }

        private static void CompareNodes(XmlNode expected, XmlNode actual, string path)
        {
            if (expected == null && actual == null) return;
            if (expected == null || actual == null)
            {
                Assert.Fail($"Node mismatch at {path}");
            }

            // Compare node name (case-insensitive)
            if (!string.Equals(expected.Name, actual.Name, StringComparison.OrdinalIgnoreCase))
            {
                Assert.Fail($"Node name mismatch at {path}. Expected '{expected.Name}', got '{actual.Name}'");
            }

            // Compare attributes (ignore extra in actual)
            if (expected.Attributes != null)
            {
                foreach (XmlAttribute expAttr in expected.Attributes)
                {
                    var actAttr = actual.Attributes?[expAttr.Name];
                    string expVal = (expAttr.Value ?? "").Trim();
                    string actVal = (actAttr?.Value ?? "").Trim();

                    if (actAttr == null)
                        Assert.Fail($"Missing attribute {expAttr.Name} at {path}");

                    // Case-insensitive compare
                    if (!string.Equals(expVal, actVal, StringComparison.OrdinalIgnoreCase))
                        Assert.Fail($"Attribute mismatch at {path}/{expAttr.Name}: expected '{expVal}', got '{actVal}'");
                }
            }

            // Compare text if both nodes are simple text nodes (case-insensitive)
            string expText = (expected.InnerText ?? "").Trim();
            string actText = (actual.InnerText ?? "").Trim();
            if (expected.ChildNodes.Count == 1 && expected.FirstChild is XmlText &&
                !string.Equals(expText, actText, StringComparison.OrdinalIgnoreCase))
            {
                Assert.Fail($"Text mismatch at {path}: expected '{expText}', got '{actText}'");
            }

            // Compare child elements by name (ignore order, ignore missing/extra)
            Dictionary<string, List<XmlNode>> expChildren = new Dictionary<string, List<XmlNode>>(StringComparer.OrdinalIgnoreCase);
            foreach (XmlNode n in expected.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    if (!expChildren.ContainsKey(n.Name))
                        expChildren[n.Name] = new List<XmlNode>();
                    expChildren[n.Name].Add(n);
                }
            }

            Dictionary<string, List<XmlNode>> actChildren = new Dictionary<string, List<XmlNode>>(StringComparer.OrdinalIgnoreCase);
            foreach (XmlNode n in actual.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    if (!actChildren.ContainsKey(n.Name))
                        actChildren[n.Name] = new List<XmlNode>();
                    actChildren[n.Name].Add(n);
                }
            }

            // Compare only for expected nodes that exist in actual
            foreach (var kv in expChildren)
            {
                if (actChildren.ContainsKey(kv.Key))
                {
                    var expList = kv.Value;
                    var actList = actChildren[kv.Key];

                    int minCount = Math.Min(expList.Count, actList.Count);
                    for (int i = 0; i < minCount; i++)
                    {
                        CompareNodes(expList[i], actList[i], path + "/" + kv.Key);
                    }
                }
                // else → if missing should be ignored, do nothing
            }

        }


        [TestMethod]
        public void Run_All_Discount_Tests()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\UnitTestCases"));

            foreach (string folder in System.IO.Directory.GetDirectories(projectRoot))
            {
                string testName = Path.GetFileName(folder);
                AssertDiscountResult(testName);
            }
        }


        [TestMethod]
        public void Apply_And_Validate_ITBTEST_Ten_Monitory_PromotionalCode()
        {            
            // Get updated cart with discount applied
            XmlElement oCartXML = RunDiscountTest("providers/discounts/test-data/basic-discount/BasicMonitory/");
            XmlNode discountPriceLineNode = oCartXML.SelectSingleNode("//DiscountPriceLine");
            string totalSaving = discountPriceLineNode.Attributes["TotalSaving"]?.Value;
            string unitPrice = discountPriceLineNode.Attributes["UnitPrice"]?.Value;
            string priceOrder = discountPriceLineNode.Attributes["PriceOrder"]?.Value;
            if (discountPriceLineNode != null && discountPriceLineNode.Attributes["Total"] != null)
            {
                string totalStr = discountPriceLineNode.Attributes["Total"].Value;

                if (decimal.TryParse(totalStr, out decimal total))
                {                                    
                    Assert.AreEqual(90m, total);
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail("Failed to parse Total attribute as decimal.");
                }
            }
            else
            {
                Assert.Fail("DiscountPriceLine node or Total attribute not found.");
            }  
        }

        [TestMethod]
        public void Apply_And_Validate_BringAFriend_Ten_Percentage_PromotionalCode()
        {
            // Get updated cart with discount applied
            XmlElement oCartXML = RunDiscountTest("providers/discounts/test-data/basic-discount/Percent_BringAFriend/");
            XmlNode discountPriceLineNode = oCartXML.SelectSingleNode("//DiscountPriceLine");
            string totalSaving = discountPriceLineNode.Attributes["TotalSaving"]?.Value;
            string unitPrice = discountPriceLineNode.Attributes["UnitPrice"]?.Value;
            string priceOrder = discountPriceLineNode.Attributes["PriceOrder"]?.Value;
            if (discountPriceLineNode != null && discountPriceLineNode.Attributes["Total"] != null)
            {
                string totalStr = discountPriceLineNode.Attributes["Total"].Value;

                if (decimal.TryParse(totalStr, out decimal total))
                {
                    Assert.AreEqual(90m, total);
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail("Failed to parse Total attribute as decimal.");
                }
            }
            else
            {
                Assert.Fail("DiscountPriceLine node or Total attribute not found.");
            }
        }

        [TestMethod]
        public void Apply_And_Validate_COMER20_Twenty_Percentage_PromotionalCode()
        {
            // Get updated cart with discount applied
            XmlElement oCartXML = RunDiscountTest("providers/discounts/test-data/basic-discount/Percent_COMER20_Evoucher_Free/");
            XmlNode discountPriceLineNode = oCartXML.SelectSingleNode("//DiscountPriceLine");
            string totalSaving = discountPriceLineNode.Attributes["TotalSaving"]?.Value;
            string unitPrice = discountPriceLineNode.Attributes["UnitPrice"]?.Value;
            string priceOrder = discountPriceLineNode.Attributes["PriceOrder"]?.Value;
            if (discountPriceLineNode != null && discountPriceLineNode.Attributes["Total"] != null)
            {
                string totalStr = discountPriceLineNode.Attributes["Total"].Value;

                if (decimal.TryParse(totalStr, out decimal total))
                {
                    Assert.AreEqual(168m, total);
                    Assert.IsTrue(true);
                }
                else
                {
                    Assert.Fail("Failed to parse Total attribute as decimal.");
                }
            }
            else
            {
                Assert.Fail("DiscountPriceLine node or Total attribute not found.");
            }
        }
    }
}
