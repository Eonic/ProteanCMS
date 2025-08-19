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

        XmlElement RunDiscountTest(string TestPath)
        {           
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;            
            string projectRoot = Path.GetFullPath(Path.Combine(baseDir, @"..\..\"));

            string discountXmlPath = Path.Combine(projectRoot, TestPath, "discounts.xml"); 
            string cartXmlPath = Path.Combine(projectRoot, TestPath, "cart.xml"); ;

            // Load test XMLs
            XmlDocument xDiscounts = new XmlDocument();
            xDiscounts.Load(discountXmlPath);

            XmlDocument xCart = new XmlDocument();
            xCart.Load(cartXmlPath);

            // Prepare nodes
            XmlElement oXmlDiscounts = xDiscounts.DocumentElement;
            XmlElement oCartXML = xCart.DocumentElement;
            string appliedCode = "";

            // Create Cms object and call CheckDiscounts
           
            moDiscount= new Protean.Cms.Cart.Discount();
            myCart = new Protean.Cms.Cart();
            XmlElement result = moDiscount.CheckDiscounts(oXmlDiscounts, ref oCartXML, ref appliedCode, myCart);          

            return result;
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
