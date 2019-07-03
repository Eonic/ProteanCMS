using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Xml;
using System.Web.HttpUtility;
using System.Web.Configuration;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using VB = Microsoft.VisualBasic;
using Protean;
using Protean.Cms.Cart;
using System.Net;

// https://github.com/Worldpay/worldpay-lib-dotnet
// 

public class DatacashProvider
{
    public DatacashProvider()
    {
    }

    public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, ref object MemProvider, ref Cms myWeb)
    {
        try
        {
            MemProvider.AdminXforms = new AdminXForms(ref myWeb);
            MemProvider.AdminProcess = new AdminProcess(ref myWeb);
            MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms;
            MemProvider.Activities = new Activities();
        }
        catch (Exception ex)
        {
            returnException("WorldPayProvider", "Initiate", ex, "", "", gbDebug);
        }
    }

    public class AdminXForms : Cms.Admin.AdminXforms
    {
        private const string mcModuleName = "Protean.Providers.WorldPay.AdminXForms";

        public AdminXForms(ref Cms aWeb) : base(aWeb)
        {
        }
    }

    public class AdminProcess : Cms.Admin
    {
        private Protean.Providers.Payment.DatacashProvider.AdminXForms _oAdXfm;

        public object oAdXfm
        {
            set
            {
                _oAdXfm = value;
            }
            get
            {
                return _oAdXfm;
            }
        }

        public AdminProcess(ref Cms aWeb) : base(aWeb)
        {
        }
    }


    public class Activities
    {
        private const string mcModuleName = "Providers.Payment.WorldPayProvider.Activities";
        private Protean.Cms myWeb;
        private XmlElement moPaymentCfg = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/payment");
        private XmlNode oWPCfg;
        private string _ServiceKey;
        private string _ClientKey;
        private string _opperationMode;
        private string cMethodName = "WorldPay";

        public Activities()
        {
            oWPCfg = moPaymentCfg.SelectSingleNode("provider[@name='WorldPay']");
            _opperationMode = Strings.LCase(oWPCfg.SelectSingleNode("opperationMode").Attributes["value"].Value());
            switch (_opperationMode)
            {
                case "live":
                    {
                        _ServiceKey = oWPCfg.SelectSingleNode("ServiceKeyLive").Attributes["value"].Value();
                        _ClientKey = oWPCfg.SelectSingleNode("ClientKeyLive").Attributes["value"].Value();
                        break;
                    }

                default:
                    {
                        _ServiceKey = oWPCfg.SelectSingleNode("ServiceKeyTest").Attributes["value"].Value();
                        _ClientKey = oWPCfg.SelectSingleNode("ClientKeyTest").Attributes["value"].Value();
                        break;
                    }
            }
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
                returnException(mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug);
                return "";
            }
        }


        public string CheckStatus(ref Protean.Cms myWeb, ref long cPaymentProviderId)
        {
            string cProcessInfo = "";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string sPaymentMethodXml;

                sPaymentMethodXml = myWeb.moDbHelper.GetDataValue("select cPayMthdDetailXml from tblCartPaymentMethod where nPayMthdKey = " + cPaymentProviderId);

                XmlDocument PaymentXml = new XmlDocument();
                PaymentXml.LoadXml(sPaymentMethodXml);

                string ccExpire = PaymentXml.DocumentElement.SelectSingleNode("expireDate").InnerText;
                string[] ccExpireArr = ccExpire.Split(" ");
                int DaysInMonth = DateTime.DaysInMonth(ccExpireArr[1], ccExpireArr[0]);
                DateTime LastDayInMonthDate = new DateTime(ccExpireArr[1], ccExpireArr[0], DaysInMonth);

                if (LastDayInMonthDate > DateTime.Today)
                    return "Expires " + niceDate(LastDayInMonthDate);
                else
                    return "Card Expired";
            }
            catch (Exception ex)
            {
                returnException(mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug);
                return "";
            }
        }

        public string CollectPayment(ref Protean.Cms myWeb, long nPaymentMethodId, double Amount, string CurrencyCode, string PaymentDescription, ref Protean.Cms.Cart oCart)
        {
            WorldPay.Sdk.Models.OrderResponse orderResponse;
            var wpClient = new WorldpayRestClient("https://api.worldpay.com/v1", _ServiceKey);
            string cProcessInfo = "";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Get the token
                string sToken = myWeb.moDbHelper.GetDataValue("select cPayMthdProviderRef from tblCartPaymentMethod where nPayMthdKey = " + nPaymentMethodId);

                var orderRequest = new WorldPay.Sdk.Models.OrderRequest()
                {
                    token = sToken,
                    orderType = "RECURRING",
                    orderDescription = PaymentDescription,
                    amount = System.Convert.ToInt32(Amount * 100),
                    currencyCode = CurrencyCode
                };
                orderResponse = wpClient.GetOrderService().Create(orderRequest);

                return "Success";
            }
            catch (Exception ex)
            {
                returnException(mcModuleName, "CollectPayment", ex, "", cProcessInfo, gbDebug);
                return "Payment Error";
            }
        }

        public xForm GetPaymentForm(ref Protean.Cms oWeb, ref Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails")
        {
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

                double repeatAmt = System.Convert.ToDouble("0" + oOrder.GetAttribute("repeatPrice"));
                string repeatInterval = Strings.LCase(oOrder.GetAttribute("repeatInterval"));
                int repeatLength = System.Convert.ToInt32("0" + oOrder.GetAttribute("repeatLength"));
                bool delayStart = Interaction.IIf(Strings.LCase(oOrder.GetAttribute("delayStart")) == "true", true, false);

                oEwProv.mcCurrency = IIf(oCart.mcCurrencyCode == "", oCart.mcCurrency, oCart.mcCurrencyCode);
                oEwProv.mcCurrencySymbol = oCart.mcCurrencySymbol;
                if (oOrder.GetAttribute("payableType") == "")
                    oEwProv.mnPaymentAmount = oOrder.GetAttribute("total");
                else
                {
                    oEwProv.mnPaymentAmount = oOrder.GetAttribute("payableAmount");
                    oEwProv.mnPaymentMaxAmount = oOrder.GetAttribute("total");
                    oEwProv.mcPaymentType = oOrder.GetAttribute("payableType");
                }
                oEwProv.mnCartId = oCart.mnCartId;
                oEwProv.mcPaymentOrderDescription = "Ref:" + oCart.OrderNoPrefix + System.Convert.ToString(oCart.mnCartId) + " An online purchase from: " + oCart.mcSiteURL + " on " + niceDate(DateTime.Now) + " " + DateTime.TimeValue(DateTime.Now);
                oEwProv.mcCardHolderName = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText;

                string purchaseDesc = "Make Payment of " + FormatNumber(oEwProv.mnPaymentAmount, 2) + " " + oWPCfg.SelectSingleNode("currency").Attributes["value"].Value + " by Credit/Debit Card";
                if (repeatAmt > 0)
                {
                    purchaseDesc = "repeat payment of " + Strings.FormatNumber(repeatAmt, 2) + " " + oWPCfg.SelectSingleNode("currency").Attributes["value"].Value + " by credit/debit card";
                    if (oEwProv.mnPaymentAmount > 0)
                        purchaseDesc = "Make initial payment of " + FormatNumber(oEwProv.mnPaymentAmount, 2) + " " + oWPCfg.SelectSingleNode("currency").Attributes["value"].Value + " and then " + purchaseDesc;
                    else
                        purchaseDesc = "Make " + purchaseDesc;
                }

                ccXform = oEwProv.creditCardXform(ref oOrder, "PayForm", sSubmitPath, oWPCfg.SelectSingleNode("cardsAccepted").Attributes["value"].Value, true, purchaseDesc, false);

                // if xform is valid or we have a 3d secure passback
                if (ccXform.valid | !myWeb.moRequest("PaRes") == "")
                {
                    var wpClient = new WorldpayRestClient("https://api.worldpay.com/v1", _ServiceKey);
                    string PaymentRef = "";
                    bool OrderSuccess = false;
                    string AuthCode = "";
                    try
                    {
                        WorldPay.Sdk.Models.OrderResponse orderResponse;
                        WorldPay.Sdk.Models.TokenResponse tokenResponse;

                        if (myWeb.moRequest("PaRes") == "")
                        {
                            string GivenName = "";
                            string Email = "";
                            string[] aGivenName;
                            // Billing Address
                            var billingAddress = new WorldPay.Sdk.Models.Address();
                            XmlElement oCartAdd = oOrder.SelectSingleNode("Contact[@type='Billing Address']");
                            if (!oCartAdd == null)
                            {
                                GivenName = oCartAdd.SelectSingleNode("GivenName").InnerText;
                                Email = oCartAdd.SelectSingleNode("Email").InnerText;
                                aGivenName = Strings.Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ");
                                if (!oCartAdd.SelectSingleNode("Company") == null)
                                {
                                    billingAddress.address1 = oCartAdd.SelectSingleNode("Company").InnerText;
                                    if (!oCartAdd.SelectSingleNode("Street") == null)
                                        billingAddress.address2 = oCartAdd.SelectSingleNode("Street").InnerText;
                                }
                                else if (!oCartAdd.SelectSingleNode("Street") == null)
                                    billingAddress.address1 = oCartAdd.SelectSingleNode("Street").InnerText;
                                if (!oCartAdd.SelectSingleNode("City") == null)
                                    billingAddress.city = Strings.Left(oCartAdd.SelectSingleNode("City").InnerText, 25);
                                if (!oCartAdd.SelectSingleNode("State") == null)
                                    billingAddress.state = oCartAdd.SelectSingleNode("State").InnerText;
                                if (!oCartAdd.SelectSingleNode("PostalCode") == null)
                                    billingAddress.postalCode = oCartAdd.SelectSingleNode("PostalCode").InnerText;
                                if (!oCartAdd.SelectSingleNode("Country") == null)
                                    billingAddress.countryCode = getCountryISONum(ref oCartAdd.SelectSingleNode("Country").InnerText);
                                if (!oCartAdd.SelectSingleNode("Telephone") == null)
                                    billingAddress.telephoneNumber = oCartAdd.SelectSingleNode("Telephone").InnerText;
                            }

                            var deliveryAddress = new WorldPay.Sdk.Models.DeliveryAddress();
                            oCartAdd = oOrder.SelectSingleNode("Contact[@type='Delivery Address']");
                            if (!oCartAdd == null)
                            {
                                aGivenName = Strings.Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ");
                                deliveryAddress.firstName = aGivenName[0];
                                deliveryAddress.lastName = aGivenName[aGivenName.Length - 1];

                                if (!oCartAdd.SelectSingleNode("Company") == null)
                                {
                                    deliveryAddress.address1 = oCartAdd.SelectSingleNode("Company").InnerText;
                                    if (!oCartAdd.SelectSingleNode("Street") == null)
                                        deliveryAddress.address2 = oCartAdd.SelectSingleNode("Street").InnerText;
                                }
                                else if (!oCartAdd.SelectSingleNode("Street") == null)
                                    deliveryAddress.address1 = oCartAdd.SelectSingleNode("Street").InnerText;
                                if (!oCartAdd.SelectSingleNode("City") == null)
                                    deliveryAddress.city = Strings.Left(oCartAdd.SelectSingleNode("City").InnerText, 25);
                                if (!oCartAdd.SelectSingleNode("State") == null)
                                    deliveryAddress.state = oCartAdd.SelectSingleNode("State").InnerText;
                                if (!oCartAdd.SelectSingleNode("PostalCode") == null)
                                    deliveryAddress.postalCode = oCartAdd.SelectSingleNode("PostalCode").InnerText;
                                if (!oCartAdd.SelectSingleNode("Country") == null)
                                    deliveryAddress.countryCode = getCountryISONum(ref oCartAdd.SelectSingleNode("Country").InnerText);
                                if (!oCartAdd.SelectSingleNode("Telephone") == null)
                                    deliveryAddress.telephoneNumber = oCartAdd.SelectSingleNode("Telephone").InnerText;
                            }

                            var paymentMethod = new WorldPay.Sdk.Models.CardRequest()
                            {
                                cardNumber = myWeb.moRequest("creditCard/number"),
                                name = GivenName,
                                expiryMonth = System.Convert.ToInt32(Left(myWeb.moRequest("creditCard/expireDate"), 2)),
                                expiryYear = System.Convert.ToInt32(Right(myWeb.moRequest("creditCard/expireDate"), 4)),
                                cvc = myWeb.moRequest("creditCard/CV2"),
                                type = "Card"
                            };
                            if (myWeb.moRequest("creditCard/issueNumber") != "")
                                paymentMethod.issueNumber = myWeb.moRequest("creditCard/issueNumber");
                            if (myWeb.moRequest("creditCard/issueDate") != "")
                            {
                                paymentMethod.startMonth = Left(myWeb.moRequest("creditCard/issueDate"), 2);
                                paymentMethod.startYear = Right(myWeb.moRequest("creditCard/issueDate"), 4);
                            }

                            if (oEwProv.mnPaymentAmount > 0)
                            {
                                var orderRequest = new WorldPay.Sdk.Models.OrderRequest()
                                {
                                    amount = System.Convert.ToInt32(oEwProv.mnPaymentAmount * 100),
                                    currencyCode = oCart.mcCurrency,
                                    settlementCurrency = oCart.mcCurrency,
                                    name = GivenName,
                                    billingAddress = billingAddress,
                                    deliveryAddress = deliveryAddress,
                                    paymentMethod = paymentMethod,
                                    orderDescription = oEwProv.mcPaymentOrderDescription,
                                    customerOrderCode = mnCartId,
                                    shopperEmailAddress = Email,
                                    shopperAcceptHeader = myWeb.moRequest.Headers("Accept"),
                                    shopperUserAgent = myWeb.moRequest.ServerVariables("HTTP_USER_AGENT"),
                                    shopperIpAddress = myWeb.moRequest.ServerVariables("REMOTE_ADDR"),
                                    shopperSessionId = myWeb.moSession.SessionID
                                };

                                if (repeatAmt > 0)
                                {
                                    orderRequest.reusable = true;
                                    orderRequest.orderType = "RECURRING";
                                }
                                else
                                {
                                    orderRequest.orderType = "ECOM";
                                    if (Strings.LCase(oWPCfg.SelectSingleNode("secure3d").Attributes["value"].Value) == "on")
                                    {
                                        orderRequest.is3DSOrder = true; // needs to be a config value
                                        if (_opperationMode == "test")
                                            orderRequest.name = "3D";
                                    }
                                }
                                orderResponse = wpClient.GetOrderService().Create(orderRequest);
                            }
                            else
                            {
                                var oTokenRequest = new WorldPay.Sdk.Models.TokenRequest()
                                {
                                    clientKey = _ClientKey,
                                    paymentMethod = paymentMethod,
                                    reusable = true
                                };

                                AuthService authSvc = wpClient.GetAuthService();

                                tokenResponse = wpClient.GetTokenService().Create(oTokenRequest);
                            }
                        }
                        else
                        {
                            WorldPay.Sdk.Models.ThreeDSecureInfo threedsinfo = new WorldPay.Sdk.Models.ThreeDSecureInfo()
                            {
                                shopperAcceptHeader = myWeb.moRequest.Headers("Accept"),
                                shopperUserAgent = myWeb.moRequest.ServerVariables("HTTP_USER_AGENT"),
                                shopperIpAddress = myWeb.moRequest.ServerVariables("REMOTE_ADDR"),
                                shopperSessionId = myWeb.moSession.SessionID
                            };
                            orderResponse = wpClient.GetOrderService().Authorize(myWeb.moSession("WPCode"), myWeb.moRequest("PaRes"), threedsinfo);
                        }
                        if (orderResponse == null)
                        {
                            OrderSuccess = true;
                            PaymentRef = tokenResponse.token;
                        }
                        else if (orderResponse.paymentStatus == WorldPay.Sdk.Enums.OrderStatus.PRE_AUTHORIZED)
                        {
                            string sRedirectURL;
                            sRedirectURL = moCartConfig["SecureURL"] + "?cartCmd=SubmitPaymentDetails&3dsec=return";
                            // err_msg = cMethodName & vbLf
                            err_msg = err_msg + "This card has subscribed to 3D Secure. You will now be re-directed to your banks website for further verification.";
                            ccXform = oEwProv.xfrmSecure3D(orderResponse.redirectURL, myWeb.moSession.SessionID, orderResponse.oneTime3DsToken, sRedirectURL);
                            ccXform.addNote("creditCard", xForm.noteTypes.Alert, err_msg, true);
                            // save the credit card details in the sessions
                            myWeb.moSession("WPCode") = orderResponse.orderCode;
                            foreach (string cItem in myWeb.moRequest.Form.Keys)
                                myWeb.moSession.Add(cItem, myWeb.moRequest.Form.Item(cItem));
                            ccXform.valid = false;
                        }
                        else if (orderResponse.paymentStatus == WorldPay.Sdk.Enums.OrderStatus.SUCCESS)
                        {
                            OrderSuccess = true;
                            PaymentRef = orderResponse.token;
                            AuthCode = orderResponse.orderCode;
                        }
                        else
                        {
                            err_msg = "WorldPay" + Constants.vbLf + orderResponse.paymentStatus + ": " + orderResponse.paymentStatusReason;
                            ccXform.addNote(ccXform.moXformElmt, xForm.noteTypes.Alert, err_msg);
                            ccXform.valid = false;

                            // if not valid return from 3DS then popout of window back to standard form.
                            if (myWeb.moRequest("3dsec") == "return" & !myWeb.moRequest("PaRes") == "")
                            {
                                string sRedirectURL;
                                sRedirectURL = moCartConfig["SecureURL"] + "?cartCmd=SubmitPaymentDetails";
                                ccXform = this.xfrmSecure3DReturn(sRedirectURL);
                            }

                            throw new WorldpayException("Expected order status PRE_AUTHORIZED");
                        }
                    }
                    catch (WorldpayException e)
                    {
                        if (gbDebug)
                            err_msg = "WorldPay" + Constants.vbLf + "Error code:" + e.apiError.customCode + ": " + Constants.vbLf + "Error description: " + e.apiError.description + Constants.vbLf + "Error message: " + e.apiError.message;
                        else
                            err_msg = e.apiError.message;
                        ccXform.addNote(ccXform.moXformElmt, xForm.noteTypes.Alert, err_msg);
                        ccXform.valid = false;
                    }

                    if (OrderSuccess)
                    {
                        XmlElement oeResponseElmt = ccXform.Instance.OwnerDocument.CreateElement("Response");
                        oeResponseElmt.SetAttribute("AuthCode", AuthCode);
                        ccXform.Instance.FirstChild.AppendChild(oeResponseElmt);

                        oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "WorldPay", PaymentRef, cMethodName, ccXform.Instance.FirstChild, DateTime.Now, false, oEwProv.mnPaymentAmount);

                        if (myWeb.moRequest("3dsec") == "return" & !myWeb.moRequest("PaRes") == "")
                        {
                            // Create a form to submit back and get fully valid result
                            // redirect to show invoice
                            string sRedirectURL;
                            sRedirectURL = moCartConfig["SecureURL"] + returnCmd + "&3dsec=showInvoice";
                            ccXform = this.xfrmSecure3DReturn(sRedirectURL);
                            myWeb.moSession("PaRes") = myWeb.moRequest("PaRes");
                        }
                        else
                            ccXform.valid = true;
                        err_msg = cMethodName + ":WP_Response = Valid";
                    }


                    // Update Seller Notes:
                    string sSql = "select * from tblCartOrder where nCartOrderKey = " + mnCartId;
                    DataSet oDs;
                    DataRow oRow;
                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                    foreach (var oRow in oDs.Tables["Order"].Rows)
                    {
                        if (myWeb.moRequest("3dsec") == "return" & !myWeb.moRequest("PaRes") == "")
                        {
                            oRow["cPaymentRef"] = PaymentRef;
                            oRow["cSellerNotes"] = oRow["cSellerNotes"] + Constants.vbLf + DateTime.Today + " " + DateTime.TimeOfDay + ": changed to: (Payment Received) " + Constants.vbLf + "comment: " + err_msg + Constants.vbLf;
                        }
                        else
                            oRow["cSellerNotes"] = oRow["cSellerNotes"] + Constants.vbLf + DateTime.Today + " " + DateTime.TimeOfDay + ": changed to: (Payment Failed) " + Constants.vbLf + "comment: " + err_msg + Constants.vbLf;
                    }
                    myWeb.moDbHelper.updateDataset(oDs, "Order");
                }
                else
                    // if we are redirected after 3Dsecure then we are valid
                    if (myWeb.moRequest("3dsec") == "showInvoice" & !myWeb.moSession("PaRes") == "")
                    ccXform.valid = true;



                return ccXform;
            }

            catch (Exception ex)
            {
                returnException(mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug);
                return null/* TODO Change to default(_) if this is not a reference type */;
            }
        }

        public xForm xfrmSecure3DReturn(string acs_url)
        {
            PerfMon.Log("EPDQ", "xfrmSecure3DReturn");
            xForm oXform = new Protean.Cms.xForm();
            XmlElement oFrmInstance;
            XmlElement oFrmGroup;

            string cProcessInfo = "xfrmSecure3D";
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                oXform.moPageXML = myWeb.moPageXml;

                // create the instance
                oXform.NewFrm("Secure3DReturn");
                oXform.submission("Secure3DReturn", goServer.UrlDecode(acs_url), "POST", "return form_check(this);");
                oFrmInstance = oXform.moPageXML.CreateElement("Secure3DReturn");
                oXform.Instance.AppendChild(oFrmInstance);
                oFrmGroup = oXform.addGroup(oXform.moXformElmt, "Secure3DReturn1", "Secure3DReturn1", "Redirect to 3D Secure");
                // build the form and the binds
                // oXform.addDiv(oFrmGroup, "<SCRIPT LANGUAGE=""Javascript"">function onXformLoad(){document.Secure3DReturn.submit();};appendLoader(onXformLoad);</SCRIPT>")
                oXform.addSubmit(oFrmGroup, "Secure3DReturn", "Show Invoice", "ewSubmit");
                oXform.addValues();
                return oXform;
            }
            catch (Exception ex)
            {
                returnException(mcModuleName, "xfrmSecure3D", ex, "", cProcessInfo, gbDebug);
                return null/* TODO Change to default(_) if this is not a reference type */;
            }
        }

        public string getCountryISONum(ref string sCountry)
        {
            PerfMon.Log(mcModuleName, "getCountryISONum");
            SqlDataReader oDr;
            string sSql;
            string strReturn = "";
            string cProcessInfo = "getCountryISONum";
            try
            {
                sSql = "select cLocationISOa2 from tblCartShippingLocations where cLocationNameFull Like '" + sCountry + "' or cLocationNameShort Like '" + sCountry + "'";
                oDr = myWeb.moDbHelper.getDataReader(sSql);
                if (oDr.HasRows)
                {
                    while (oDr.Read())
                        strReturn = oDr["cLocationISOa2"];
                }
                else
                    strReturn = "";

                oDr.Close();
                oDr = null;
                return strReturn;
            }
            catch (Exception ex)
            {
                returnException(mcModuleName, "getCountryISOnum", ex, "", cProcessInfo, gbDebug);
                return null;
            }
        }
    }
}
