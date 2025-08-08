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
        //public HttpContext moCtx = HttpContext.Current;
        public Cart.Discount moDiscount;        
        XmlDocument RunDiscountTest(string TestPath)
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
            XmlElement result = moDiscount.CheckDiscounts(oXmlDiscounts, ref oCartXML, ref appliedCode);

            // Set result in XmlDocument to return
            XmlDocument updatedCartDoc = new XmlDocument();
            updatedCartDoc.LoadXml(result.OuterXml);

            return updatedCartDoc;
        }
       
       
        [TestMethod]
        public void TenPercentDiscount()
        {            
            // Get updated cart with discount applied
            XmlDocument oCartXML = RunDiscountTest("providers/discounts/test-data/basic-discount/TenPercentDiscount/");
            XmlNode discountPriceLineNode = oCartXML.SelectSingleNode("//DiscountPriceLine");
            string totalSaving = discountPriceLineNode.Attributes["TotalSaving"]?.Value;
            string unitPrice = discountPriceLineNode.Attributes["UnitPrice"]?.Value;
            string priceOrder = discountPriceLineNode.Attributes["PriceOrder"]?.Value;
            if (discountPriceLineNode != null && discountPriceLineNode.Attributes["Total"] != null)
            {
                string totalStr = discountPriceLineNode.Attributes["Total"].Value;

                if (decimal.TryParse(totalStr, out decimal total))
                {                                    
                    Assert.AreEqual(105.01m, total);
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
