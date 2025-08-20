using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protean.Providers.DiscountRule;
using System;
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
            if (expected == null && actual == null)
                return;

            if (expected == null || actual == null)
                Assert.Fail($"Node mismatch at {path}. One is null, the other is not.");

            // Compare node names
            if (expected.Name != actual.Name)
                Assert.Fail($"Node name mismatch at {path}. Expected '{expected.Name}', got '{actual.Name}'.");

            // Compare attributes (ignore order, normalize whitespace)
            if (expected.Attributes != null)
            {
                foreach (XmlAttribute expAttr in expected.Attributes)
                {
                    var actAttr = actual.Attributes?[expAttr.Name];
                    string expVal = expAttr.Value?.Trim() ?? string.Empty;
                    string actVal = actAttr?.Value?.Trim() ?? string.Empty;

                    if (actAttr == null)
                    {
                        Assert.Fail($"Missing attribute at {path}/{expected.Name}[@{expAttr.Name}]. Expected '{expVal}', but it was not found.");
                    }

                    if (!string.Equals(expVal, actVal, StringComparison.Ordinal))
                    {
                        Assert.Fail(
                            $"Attribute mismatch at {path}/{expected.Name}[@{expAttr.Name}]. " +
                            $"Expected '{expVal}', got '{actVal}'.");
                    }
                }

                // Check for unexpected extra attributes
                foreach (XmlAttribute actAttr in actual.Attributes)
                {
                    if (expected.Attributes[actAttr.Name] == null)
                    {
                        Assert.Fail($"Unexpected attribute at {path}/{expected.Name}[@{actAttr.Name}] with value '{actAttr.Value}'.");
                    }
                }
            }

            // Compare inner text (trimmed, whitespace normalized)
            string expectedText = expected.InnerText?.Trim() ?? string.Empty;
            string actualText = actual.InnerText?.Trim() ?? string.Empty;
            if (expectedText != actualText && expected.ChildNodes.Count == 1 && expected.FirstChild is XmlText)
            {
                Assert.Fail(
                    $"Text mismatch at {path}/{expected.Name}. " +
                    $"Expected '{expectedText}', got '{actualText}'.");
            }

            // Compare child nodes count
            XmlNodeList expectedChildren = expected.ChildNodes;
            XmlNodeList actualChildren = actual.ChildNodes;

            // Only compare element nodes (ignore whitespace-only text nodes)
            var expElems = expectedChildren.Cast<XmlNode>().Where(n => n.NodeType == XmlNodeType.Element).ToList();
            var actElems = actualChildren.Cast<XmlNode>().Where(n => n.NodeType == XmlNodeType.Element).ToList();

            if (expElems.Count != actElems.Count)
            {
                Assert.Fail($"Child node count mismatch at {path}/{expected.Name}. " +
                            $"Expected {expElems.Count}, got {actElems.Count}.");
            }

            // Recursively compare children
            for (int i = 0; i < expElems.Count; i++)
            {
                CompareNodes(expElems[i], actElems[i], $"{path}/{expected.Name}");
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
