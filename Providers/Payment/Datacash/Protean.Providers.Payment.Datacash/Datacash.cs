using System;
using System.Linq;
using System.Xml;

using System.Web.Configuration;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using VB = Microsoft.VisualBasic;
using Protean;

using System.Net;
using System.Web;
using DataCash;

// https://github.com/Worldpay/worldpay-lib-dotnet
// 
namespace Protean.Providers.Payment
{ 
    public class DatacashProvider
{
   
    public void Initiate(object _AdminXforms, object _AdminProcess,  object _Activities, Protean.Providers.Payment.BaseProvider MemProvider, Cms myWeb)
    {
        try
        {
                MemProvider.AdminXforms = new AdminXForms(myWeb);
                MemProvider.AdminProcess = new AdminProcess(myWeb);
            //    MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms;
                MemProvider.Activities = new Activities();
            }
        catch (Exception ex)
        {
            
        }
    }

    public class AdminXForms : Cms.Admin.AdminXforms
    {
        private const string mcModuleName = "Protean.Providers.Datacash.AdminXForms";

            public AdminXForms(Cms aWeb) : base(ref aWeb)
            {
            }
        }

    public class AdminProcess : Cms.Admin
    {
        private DatacashProvider.AdminXForms _oAdXfm;

        public object oAdXfm
        {
            set
            {
                _oAdXfm = (DatacashProvider.AdminXForms)value;
            }
            get
            {
                return _oAdXfm;
            }
        }

            public AdminProcess(Cms aWeb) : base(ref aWeb)
            {
            }
        }


    public class Activities
    {
        private const string mcModuleName = "Providers.Payment.DatacashProvider.Activities";
        private Protean.Cms myWeb;
        //private moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment");
        private XmlNode oWPCfg;
        private string _ServiceKey;
        private string _ClientKey;
        private string _opperationMode;
        private string cMethodName = "Datacash";

         public   Activities() { 

         //oWPCfg = moPaymentCfg. SelectSingleNode("provider[@name='Datacash']");
               // _opperationMode = oWPCfg.SelectSingleNode("opperationMode").Value.ToLower();
                //switch (_opperationMode)
                //{
                //    case "live":
                //        {
                //            _ServiceKey = oWPCfg.SelectSingleNode("ServiceKeyLive").Value;
                //            _ClientKey = oWPCfg.SelectSingleNode("ClientKeyLive").Value;
                //            break;
                //        }

                //    default:
                //        {
                //            _ServiceKey = oWPCfg.SelectSingleNode("ServiceKeyTest").Value;
                //            _ClientKey = oWPCfg.SelectSingleNode("ClientKeyTest").Value; 
                //            break;
                //        }
                //}
        

          
        }

        public string CancelPayments(ref Protean.Cms oWeb, ref string nPaymentProviderRef)
        {
            string cProcessInfo = "";

            XmlElement moPaymentCfg = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/payment");
            XmlNode oWorldPayCfg;

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                oWorldPayCfg = moPaymentCfg.SelectSingleNode("provider[@name='WorldPay']");
                // _merchantId = oWorldPayCfg.SelectSingleNode("merchantId").Attributes("value").Value()
                // _merchantToken = oWorldPayCfg.SelectSingleNode("merchantToken").Attributes("value").Value()
                // _sandboxToken = oWorldPayCfg.SelectSingleNode("merchantSandboxToken").Attributes("value").Value()

                // _opperationMode = oWorldPayCfg.SelectSingleNode("opperationMode").Attributes("value").Value()

                return "No Action Required";
            }
            catch (Exception ex)
            {
              //  returnException(mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug);
                return "";
            }
        }


        public string CheckStatus(ref Protean.Cms myWeb, ref long cPaymentProviderId)
        {
            string cProcessInfo = "";
            try
            {
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                //string sPaymentMethodXml;

                //sPaymentMethodXml = myWeb.moDbHelper.GetDataValue("select cPayMthdDetailXml from tblCartPaymentMethod where nPayMthdKey = " + cPaymentProviderId);

                //XmlDocument PaymentXml = new XmlDocument();
                //PaymentXml.LoadXml(sPaymentMethodXml);

                //string ccExpire = PaymentXml.DocumentElement.SelectSingleNode("expireDate").InnerText;
                //string[] ccExpireArr = ccExpire.Split(" ");
                //int DaysInMonth = DateTime.DaysInMonth(ccExpireArr[1], ccExpireArr[0]);
                //DateTime LastDayInMonthDate = new DateTime(ccExpireArr[1], ccExpireArr[0], DaysInMonth);

                //if (LastDayInMonthDate > DateTime.Today)
                //    return "Expires " + niceDate(LastDayInMonthDate);
                //else
                    return "Card Expired";
            }
            catch (Exception ex)
            {
            //    returnException(mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug);
                return "";
            }
        }

        public string CollectPayment(ref Protean.Cms myWeb, long nPaymentMethodId, double Amount, string CurrencyCode, string PaymentDescription, ref Protean.Cms.Cart oCart)
        {
            //WorldPay.Sdk.Models.OrderResponse orderResponse;
            //var wpClient = new WorldpayRestClient("https://api.worldpay.com/v1", _ServiceKey);
            //string cProcessInfo = "";
            try
            {
            //    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //    // Get the token
            //    string sToken = myWeb.moDbHelper.GetDataValue("select cPayMthdProviderRef from tblCartPaymentMethod where nPayMthdKey = " + nPaymentMethodId);

            //    var orderRequest = new WorldPay.Sdk.Models.OrderRequest()
            //    {
            //        token = sToken,
            //        orderType = "RECURRING",
            //        orderDescription = PaymentDescription,
            //        amount = System.Convert.ToInt32(Amount * 100),
            //        currencyCode = CurrencyCode
            //    };
            //    orderResponse = wpClient.GetOrderService().Create(orderRequest);

                return "Success";
            }
            catch (Exception ex)
            {
                //returnException(mcModuleName, "CollectPayment", ex, "", cProcessInfo, gbDebug);
                return "Payment Error";
            }
        }

        public xForm GetPaymentForm(ref Protean.Cms oWeb, ref Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails")
        {
                XmlElement moPaymentCfg = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                XmlNode _oDatacashcfg;
                _oDatacashcfg = moPaymentCfg.SelectSingleNode("provider[@name='Datacash']");
                string _AccountId = _oDatacashcfg.SelectSingleNode("accountId").Attributes["value"].Value;
                string _AccountPassword = _oDatacashcfg.SelectSingleNode("accountPassword").Attributes["value"].Value;
                string _Currency = _oDatacashcfg.SelectSingleNode("currency").Attributes["value"].Value;
                string confFile = _oDatacashcfg.SelectSingleNode("ConfigurationFile").Attributes["value"].Value.ToString();
                string cProcessInfo = "";

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                myWeb = oWeb;
                System.Collections.Specialized.NameValueCollection moCartConfig = oCart.moCartConfig;
                string mcPaymentMethod = oCart.mcPaymentMethod;

                var sSubmitPath = oCart.mcPagePath + returnCmd;
                xForm ccXform;
                Protean.Cms.Cart.PaymentProviders oEwProv = new Protean.Cms.Cart.PaymentProviders(ref myWeb);

                var mnCartId = oCart.mnCartId;
                string err_msg = "";
                oEwProv.mcCurrency = (oCart.mcCurrencyCode == "" ? oCart.mcCurrency: oCart.mcCurrencyCode);
                oEwProv.mcCurrencySymbol = oCart.mcCurrencySymbol;
                oEwProv.mnPaymentAmount = Convert.ToDouble(oOrder.GetAttribute("total"));

                oEwProv.mnCartId = oCart.mnCartId;
                oEwProv.mcPaymentOrderDescription = "Ref:" + oCart.OrderNoPrefix + System.Convert.ToString(oCart.mnCartId) + " An online purchase from: " + oCart.mcSiteURL + " on " + DateTime.Now;
                oEwProv.mcCardHolderName = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText;

                string purchaseDesc = "Make Payment of " + oEwProv.mnPaymentAmount + " " + _oDatacashcfg.SelectSingleNode("currency").Attributes["value"].Value + " by Credit/Debit Card";

                ccXform = oEwProv.creditCardXform(ref oOrder, "PayForm", sSubmitPath, _oDatacashcfg.SelectSingleNode("cardsAccepted").Attributes["value"].Value, true, purchaseDesc, false);
                  
                // if xform is valid or we have a 3d secure passback
                if ((ccXform.valid || myWeb.moRequest["PaRes"] != "") && ccXform.isSubmitted()==true)
                {
                    

                    DataCash.Config cfg = default(DataCash.Config);
                    try
                    {
                        cfg = new DataCash.Config(confFile);
                    }
                    catch (Exception ex)
                    {

                    }
                    string deviceCat = "0";
                    DataCash.Document requestDoc = default(DataCash.Document);
                   
                        string GivenName = "";

                        string[] aGivenName;
                        var captureMethodObj = "ecomm";
                        string strMerchantReference = "1234567891";//(oCart.mnCartId).ToString();
                        requestDoc = new DataCash.Document(cfg);
                        requestDoc.@set("Request.Authentication.client", _AccountId);
                        requestDoc.@set("Request.Authentication.password", _AccountPassword);

                        requestDoc.@set("Request.Transaction.TxnDetails.merchantreference", strMerchantReference);
                        Hashtable attrs = new Hashtable();
                        attrs["currency"] = _Currency;
                        requestDoc.setWithAttributes("Request.Transaction.TxnDetails.amount", Convert.ToString(oOrder.GetAttribute("total")), attrs);
                        //requestDoc.@set("Request.Transaction.TxnDetails.capturemethod", "ecomm");
                        requestDoc.@set("Request.Transaction.TxnDetails.capturemethod", captureMethodObj);


                        string cardNumber = myWeb.moRequest["creditCard/number"];
                        string name = GivenName;
                        string expiryMonth = Convert.ToString(myWeb.moRequest["creditCard/expireDate"]).Substring(0, 2);
                        int len = Convert.ToString(myWeb.moRequest["creditCard/expireDate"]).Length;
                        string expiryYear = Convert.ToString(myWeb.moRequest["creditCard/expireDate"]).Substring(len - 2);
                        string cvc = myWeb.moRequest["creditCard/CV2"];
                        string type = "Card";
                        string issueNumber = "";
                        string startMonth = "";
                        string startYear = "";
                        if (myWeb.moRequest["creditCard/issueNumber"] != "")
                            issueNumber = Convert.ToString(myWeb.moRequest["creditCard/issueNumber"]);
                        if (myWeb.moRequest["creditCard/issueDate"] != "")
                        {
                            startMonth = Convert.ToString(myWeb.moRequest["creditCard/issueDate"]).Substring(0, 2);
                            len = Convert.ToString(myWeb.moRequest["creditCard/issueDate"]).Length;
                            startYear = Convert.ToString(myWeb.moRequest["creditCard/issueDate"]).Substring(len - 2);
                        }


                        requestDoc.set("Request.Transaction.CardTxn.Card.pan", cardNumber);
                        requestDoc.set("Request.Transaction.CardTxn.Card.expirydate", expiryMonth + '/' + expiryYear);
                        requestDoc.set("Request.Transaction.CardTxn.Card.Cv2Avs.cv2", cvc);

                        if (type == "SOLO" || type == "MAESTRO")
                        {
                            requestDoc.set("Request.Transaction.CardTxn.Card.startdate", startMonth + '/' + startYear);
                            requestDoc.set("Request.Transaction.CardTxn.Card.issuenumber", issueNumber);
                        }

                        //if (captureMethodObj == "ecomm")
                        //{
                        //    //3d secure
                        //    requestDoc.@set("Request.Transaction.TxnDetails.ThreeDSecure.verify", "yes");
                        //    requestDoc.@set("Request.Transaction.TxnDetails.ThreeDSecure.merchant_url", _oDatacashcfg.SelectSingleNode("IntoTheBlueDomain").Attributes["value"].Value.ToString() + "/");
                        //    requestDoc.@set("Request.Transaction.TxnDetails.ThreeDSecure.purchase_datetime", DateTime.Now.ToString("yyyyMMdd HH:mm:ss"));
                        //    requestDoc.@set("Request.Transaction.TxnDetails.ThreeDSecure.purchase_desc", "vouchers");
                        //    requestDoc.@set("Request.Transaction.TxnDetails.ThreeDSecure.Browser.device_category", "0");
                        //    requestDoc.@set("Request.Transaction.TxnDetails.ThreeDSecure.Browser.accept_headers", "*/*");
                        //    requestDoc.@set("Request.Transaction.TxnDetails.ThreeDSecure.Browser.user_agent", HttpContext.Current.Request.Browser.Browser);
                        //}

                        XmlNode oCartAdd = oOrder.SelectSingleNode("Contact[@type='Billing Address']");
                        if (oCartAdd != null)
                        {

                            GivenName = oCartAdd.SelectSingleNode("GivenName").InnerText;
                            aGivenName = oCartAdd.SelectSingleNode("GivenName").InnerText.ToString().Split(' ');

                            //get the3rdMan details
                            //customer info
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.title", "");
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.forename", aGivenName[0]);
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.surname", aGivenName[aGivenName.Length - 1]);
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.email", oCartAdd.SelectSingleNode("Email").InnerText);
                            string PhoneNo = oCartAdd.SelectSingleNode("Telephone").InnerText;
                            PhoneNo = PhoneNo.Replace("+", "");
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.telephone", PhoneNo);

                            //Billing Address
                            if (oCartAdd.SelectSingleNode("Company") != null)
                            {
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.street_address_1", oCartAdd.SelectSingleNode("Company").InnerText);
                                if (oCartAdd.SelectSingleNode("Street") != null)
                                {
                                    requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.street_address_2", oCartAdd.SelectSingleNode("Street").InnerText);
                                }
                            }
                            else if (oCartAdd.SelectSingleNode("Street") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.street_address_1", oCartAdd.SelectSingleNode("Street").InnerText);
                            if (oCartAdd.SelectSingleNode("City") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.city", oCartAdd.SelectSingleNode("City").InnerText);
                            if (oCartAdd.SelectSingleNode("State") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.county", oCartAdd.SelectSingleNode("State").InnerText);
                            if (oCartAdd.SelectSingleNode("PostalCode") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.postcode", oCartAdd.SelectSingleNode("PostalCode").InnerText);
                            if (oCartAdd.SelectSingleNode("StrCountry") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.country", getCountryISONum(oCartAdd.SelectSingleNode("Country").InnerText));

                        }
                        //requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.ip_address", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString());

                        oCartAdd = oOrder.SelectSingleNode("Contact[@type='Delivery Address']");
                        if (oCartAdd != null)
                        {
                            GivenName = oCartAdd.SelectSingleNode("GivenName").InnerText;
                            aGivenName = oCartAdd.SelectSingleNode("GivenName").InnerText.ToString().Split(' ');
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.delivery_title", "");
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.delivery_forename", aGivenName[0]);
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.delivery_surname", aGivenName[aGivenName.Length - 1]);
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.order_number", "123");
                            requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.CustomerInformation.sales_channel", "3");


                            //Delivery(Address)
                            if (oCartAdd.SelectSingleNode("Company") != null)
                            {
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.street_address_1", oCartAdd.SelectSingleNode("Company").InnerText);
                                if (oCartAdd.SelectSingleNode("Street") != null)
                                {
                                    requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.street_address_2", oCartAdd.SelectSingleNode("Street").InnerText);
                                }
                            }
                            else if (oCartAdd.SelectSingleNode("Street") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.BillingAddress.street_address_1", oCartAdd.SelectSingleNode("Street").InnerText);
                            if (oCartAdd.SelectSingleNode("City") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.DeliveryAddress.city", oCartAdd.SelectSingleNode("City").InnerText);
                            if (oCartAdd.SelectSingleNode("State") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.DeliveryAddress.county", oCartAdd.SelectSingleNode("State").InnerText);
                            if (oCartAdd.SelectSingleNode("PostalCode") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.DeliveryAddress.postcode", oCartAdd.SelectSingleNode("PostalCode").InnerText);
                            if (oCartAdd.SelectSingleNode("StrCountry") != null)
                                requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.DeliveryAddress.country", getCountryISONum(oCartAdd.SelectSingleNode("Country").InnerText));
                        }
                        //Order Information

                        requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.OrderInformation.event_date", DateTime.Now.ToString("yyyy-MM-dd"));
                        requestDoc.@set("Request.Transaction.TxnDetails.The3rdMan.OrderInformation.distribution_channel", "First Class Royaql Mail");
                        //XElement xmlActivity = xmlParentBasket.Element("Activities");

                        //XElement stateXml = default(XElement);
                        //stateXml = new XElement("Products", new XAttribute("count", xmlParentBasket.Element("Activities").Elements().Count().ToString()), from nodeActivity in xmlActivity.Elements() select new XElement("Product", new XElement("code", nodeActivity.Element("strCode").Value), new XElement("quantity", nodeActivity.Element("intquantity").Value), new XElement("price", nodeActivity.Element("dblPrice").Value), new XElement("prod_description", nodeActivity.Element("BasketDescription").Value.Replace("&", ""))));



                        //XmlDocumentFragment docFrag = default(XmlDocumentFragment);
                        //docFrag = requestDoc.XMLDocument.CreateDocumentFragment();
                        //docFrag.InnerXml = stateXml.ToString();


                        //XmlNode nodeProd = requestDoc.XMLDocument.DocumentElement;

                        //XmlNodeList xmlnlist = default(XmlNodeList);
                        ////condition for distribution_channel is remaining
                        //xmlnlist = nodeProd.SelectNodes("//Request/Transaction/TxnDetails/The3rdMan/OrderInformation");
                        //  xmlnlist.Item(0).AppendChild(docFrag);

                        requestDoc.@set("Request.Transaction.CardTxn.method", "auth");

                        DataCash.Document responseDoc = default(DataCash.Document);
                        Agent agt = default(Agent);
                        agt = new Agent(cfg);
                        System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
                        responseDoc = agt.send(requestDoc);

                      

                }
                return ccXform;
            }
            catch (Exception ex)
            {
                //returnException(mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug);
                return null/* TODO Change to default(_) if this is not a reference type */;
            }
        
            }

        public xForm xfrmSecure3DReturn(string acs_url)
        {
            //PerfMon.Log("EPDQ", "xfrmSecure3DReturn");
            xForm oXform = new Protean.Cms.xForm();
            XmlElement oFrmInstance;
            XmlElement oFrmGroup;

           // string cProcessInfo = "xfrmSecure3D";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                oXform.moPageXML = myWeb.moPageXml;

                // create the instance
                oXform.NewFrm("Secure3DReturn");
               // oXform.submission("Secure3DReturn", goServer.UrlDecode(acs_url), "POST", "return form_check(this);");
                oFrmInstance = oXform.moPageXML.CreateElement("Secure3DReturn");
                oXform.Instance.AppendChild(oFrmInstance);
                //oFrmGroup = oXform.addGroup(oXform.moXformElmt, "Secure3DReturn1", "Secure3DReturn1", "Redirect to 3D Secure");
                // build the form and the binds
                // oXform.addDiv(oFrmGroup, "<SCRIPT LANGUAGE=""Javascript"">function onXformLoad(){document.Secure3DReturn.submit();};appendLoader(onXformLoad);</SCRIPT>")
                //oXform.addSubmit(oFrmGroup, "Secure3DReturn", "Show Invoice", "ewSubmit");
                oXform.addValues();
                return oXform;
            }
            catch (Exception ex)
            {
               // returnException(mcModuleName, "xfrmSecure3D", ex, "", cProcessInfo, gbDebug);
                return null/* TODO Change to default(_) if this is not a reference type */;
            }
        }

        public string getCountryISONum(string sCountry)
        {
            //PerfMon.Log(mcModuleName, "getCountryISONum");
            SqlDataReader oDr;
            string sSql;
            string strReturn = "";
            //string cProcessInfo = "getCountryISONum";
            try
            {
                sSql = "select cLocationISOa2 from tblCartShippingLocations where cLocationNameFull Like '" + sCountry + "' or cLocationNameShort Like '" + sCountry + "'";
                oDr = myWeb.moDbHelper.getDataReader(sSql);
                if (oDr.HasRows)
                {
                    while (oDr.Read())
                        strReturn = oDr["cLocationISOa2"].ToString();
                }
                else
                    strReturn = "";

                oDr.Close();
                oDr = null;
                return strReturn;
            }
            catch (Exception ex)
            {
                //returnException(mcModuleName, "getCountryISOnum", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }
    }
}
}