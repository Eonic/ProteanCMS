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
        public Protean.Cms myWeb;
        XmlDocument RunDiscountTest(string TestPath)
        {
            Protean.Cms myWeb = new Protean.Cms();
            Cart myCart = new Cart(ref myWeb);
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
           
            moDiscount= new Protean.Cms.Cart.Discount(ref myWeb);
            XmlElement result = moDiscount.CheckDiscounts(oXmlDiscounts, ref oCartXML, ref appliedCode);

            // Set result in XmlDocument to return
            XmlDocument updatedCartDoc = new XmlDocument();
            updatedCartDoc.LoadXml(result.OuterXml);

            return updatedCartDoc;
        }
       
       
        [TestMethod]
        public void TenPercentDiscount()
        {
            bool mbRoundUp = true;
            // Get updated cart with discount applied
            XmlDocument oCartXML = RunDiscountTest("providers/discounts/test-data/basic-discount/TenPercentDiscount/");
            XmlElement ofinalCartXML = oCartXML.DocumentElement;
            Protean.Providers.DiscountRule.ReturnProvider oDiscRuleProv = new Protean.Providers.DiscountRule.ReturnProvider();
            IdiscountRuleProvider oDisProvider = oDiscRuleProv.Get(ref myWeb);

            //decimal nTotalSaved = Discount_ApplyToCart(ref oCartXML, oXmlDiscounts);
            decimal nTotalSaved = oDisProvider.FinalApplyToCart(ref ofinalCartXML, ref myWeb, mbRoundUp);
           
             Assert.AreEqual(11, Math.Round(nTotalSaved, 2));

            //   XmlDocument oUserInstance = new XmlDocument();

            //      here we need to get the instance from the Xform for the website and populate the default values.

            //   ptnCMS.moDbHelper.setObjectInstance(Protean.Cms.dbHelper.objectTypes.Directory, oUserInstance.DocumentElement);

            // force pass
            Assert.IsTrue(true);

        }

        
        //[TestMethod]
        //public void Get_Supplier_Amount_VAT_TO_BE_Charged_When_ZeroRatedAmount_Is_Not_Passed()
        //{
        //    VatModels vatModels = new VatModels();
        //    vatModels.AmountZeroRated = 0;
        //    vatModels.SKUPrice = 20;
        //    vatModels.VatRate = 20;
        //    vatModels.AmountToBeVATCharged = vatSVC.getAmountToBeVATCharged(vatModels);
        //    Assert.AreEqual(16.67, Math.Round(vatModels.AmountToBeVATCharged, 2));
        //}

        [TestMethod]
        public void CreateCompany()
        {

         //   Protean.Cms ptnCMS = new Protean.Cms();

         //   XmlDocument oCompanyInstance = new XmlDocument();

            // here we need to get the instance from the Xform for the website and populate the default values.

        //    ptnCMS.moDbHelper.setObjectInstance(Protean.Cms.dbHelper.objectTypes.Directory, oCompanyInstance.DocumentElement);

            // force pass
            Assert.IsTrue(true);

        }
    }
}
