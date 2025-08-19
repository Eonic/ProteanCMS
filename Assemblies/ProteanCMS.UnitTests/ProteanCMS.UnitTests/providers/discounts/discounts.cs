using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Xml;
using Protean.Providers.DiscountRule;
using System.Web;
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

        private static void CompareNodes(XmlNode expected, XmlNode CartXML, string path)
        {
            if (expected == null && CartXML == null)
                return;

            if (expected == null || CartXML == null)
                Assert.Fail($"Node mismatch at {path}. One is null, the other is not.");

            // Compare node names
            if (expected.Name != CartXML.Name)
                Assert.Fail($"Node name mismatch at {path}. Expected '{expected.Name}', got '{CartXML.Name}'.");

            // Compare attributes
            foreach (XmlAttribute expAttr in expected.Attributes)
            {
                string actVal = CartXML.Attributes[expAttr.Name]?.Value;
                if (actVal != expAttr.Value)
                {
                    Assert.Fail(
                        $"Attribute mismatch at {path}/{expected.Name}[@{expAttr.Name}]. " +
                        $"Expected '{expAttr.Value}', got '{actVal ?? "MISSING"}'.");
                }
            }

            // Compare child nodes count
            XmlNodeList expectedChildren = expected.ChildNodes;
            XmlNodeList actualChildren = CartXML.ChildNodes;

            if (expectedChildren.Count != actualChildren.Count)
                Assert.Fail($"Child count mismatch at {path}/{expected.Name}. Expected {expectedChildren.Count}, got {actualChildren.Count}.");

            // Recurse into children
            for (int i = 0; i < expectedChildren.Count; i++)
            {
                CompareNodes(expectedChildren[i], actualChildren[i], $"{path}/{expected.Name}[{i}]");
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
