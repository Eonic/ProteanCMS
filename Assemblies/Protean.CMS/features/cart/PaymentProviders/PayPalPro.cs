using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Web.Configuration;
using System.Xml;
using CardinalCommerce;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Tools.Integration.Twitter;
using static Protean.Cms.Cart;
using static Protean.stdTools;
using static Protean.Tools.Xml;


namespace Protean.Providers
{
    namespace Payment
    {
        public class PayPalPro
        {

            public PayPalPro()
            {
                // do nothing
            }

            public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, ref object MemProvider, ref Cms myWeb)
            {

                MemProvider.AdminXforms = new AdminXForms(ref myWeb);
                MemProvider.AdminProcess = new AdminProcess(ref myWeb);
                MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms;
                MemProvider.Activities = new Activities();

            }

            public class AdminXForms : Cms.Admin.AdminXforms
            {
                private const string mcModuleName = "Providers.Providers.Eonic.AdminXForms";

                public AdminXForms(ref Cms aWeb) : base(aWeb)
                {
                }

            }

            public class AdminProcess : Cms.Admin
            {

                private AdminXForms _oAdXfm;

                public object oAdXfm
                {
                    set
                    {
                        _oAdXfm = (AdminXForms)value;
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


            public class Activities : Payment.DefaultProvider.Activities
            {

                private const string mcModuleName = "Providers.Payment.PayPalPro.Activities";
                private new Cms myWeb;
                protected XmlNode moPaymentCfg;
                private TransactionMode nTransactionMode;

                public enum TransactionMode
                {
                    Live = 0,
                    Test = 1,
                    Fail = 2
                }


                public string CollectPayment(ref Cms oWeb, long nPaymentMethodId, double Amount, string CurrencyCode, string PaymentDescription, ref Cms.Cart oCart)
                {
                    string cProcessInfo = "";
                    try
                    {

                        // Do nothing because recurring payments are automatic

                        return "Success";
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CollectPayment", ex, "", cProcessInfo, gbDebug);
                        return "Payment Error";
                    }
                }

                public new Protean.xForm GetPaymentForm(ref Cms oWeb, ref Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails")
                {
                    myWeb.PerfMon.Log("Protean.Providers.payment.PayPalPro", "GetPaymentForm");
                    string sSql;

                    Protean.xForm ccXform;

                    Protean.xForm Xform3dSec = (Protean.xForm)null;

                    var bIsValid = default(bool);
                    string err_msg = "";
                    string err_msg_log = "";
                    string sProcessInfo = "";
                    string cResponse = string.Empty;

                    var oDictOpt = new Hashtable();

                    bool bCv2 = false;
                    bool b3DSecure = false;
                    bool b3DSecureV2 = false;
                    bool b3DAuthorised = false;
                    string sRedirectURL = "";
                    string sPaymentRef = "";

                    string cProcessInfo = "PalPalPro";

                    // Get the payment options into a hashtable
                    XmlNode oPaymentCfg;
                    bool bSavePayment = false;
                    bool bAllowSavePayment = false;
                    string sSubmitPath = oCart.mcPagePath + returnCmd;

                    // 3d Sec return values
                    string PAResStatus = "";
                    string Enrolled = "";
                    string ACSUrl = "";
                    string Payload = "";
                    string EciFlag = "";
                    string CAVV = "";
                    string Xid = "";
                    string PaymemtRef = "";
                    string cAuthCode = "";
                    bool bValidateFirst = false;

                    try
                    {
                        myWeb = oWeb;
                        Protean.Cms.Cart.PaymentProviders oEwProv = new PaymentProviders(ref myWeb);

                        // Get values form cart
                        oEwProv.mcCurrency =Convert.ToString(Interaction.IIf(string.IsNullOrEmpty(oCart.mcCurrencyCode), oCart.mcCurrency, oCart.mcCurrencyCode));
                        oEwProv.mcCurrencySymbol = oCart.mcCurrencySymbol;
                        if (string.IsNullOrEmpty(oOrder.GetAttribute("payableType")))
                        {
                            oEwProv.mnPaymentAmount = Convert.ToDouble(oOrder.GetAttribute("total"));
                        }
                        else
                        {
                            oEwProv.mnPaymentAmount = Convert.ToDouble(oOrder.GetAttribute("payableAmount"));
                            oEwProv.mnPaymentMaxAmount = Convert.ToDouble(oOrder.GetAttribute("total"));
                            oEwProv.mcPaymentType = oOrder.GetAttribute("payableType");
                        }
                        oEwProv.mnCartId = oCart.mnCartId;
                        oEwProv.mcPaymentOrderDescription = "Ref:" + oCart.OrderNoPrefix + oCart.mnCartId.ToString() + " An online purchase from: " + oCart.mcSiteURL + " on " + niceDate(DateTime.Now) + " " + Conversions.ToString(DateAndTime.TimeValue(Conversions.ToString(DateTime.Now)));
                        oEwProv.mcCardHolderName = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText;

                        // Repeat Payment Settings
                        double repeatAmt = Conversions.ToDouble("0" + oOrder.GetAttribute("repeatPrice"));
                        string repeatInterval = Strings.LCase(oOrder.GetAttribute("repeatInterval"));
                        int repeatFrequency = (int)Math.Round(Conversions.ToDouble("0" + oOrder.GetAttribute("repeatFrequency")));
                        if (repeatFrequency == 0)
                            repeatFrequency = (int)Math.Round(Conversions.ToDouble("0" + oOrder.GetAttribute("repeatLength")));
                        int repeatLength = (int)Math.Round(Conversions.ToDouble("0" + oOrder.GetAttribute("repeatLength")));
                        if (repeatLength == 0)
                            repeatLength = 1;
                        bool delayStart = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(oOrder.GetAttribute("delayStart")) == "true", true, false));
                        DateTime startDate;

                        if (!string.IsNullOrEmpty(oOrder.GetAttribute("startDate")))
                        {
                            startDate = Conversions.ToDate(oOrder.GetAttribute("startDate"));
                        }
                        else
                        {
                            startDate = DateTime.Now;
                        }

                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");

                        oPaymentCfg = moPaymentCfg.SelectSingleNode("provider[@name='PayPalPro']");
                        oDictOpt = xmlToHashTable(oPaymentCfg, "value");

                        switch (Conversions.ToString(oDictOpt["opperationMode"]) ?? "")
                        {
                            case "true":
                            case "test":
                                {
                                    nTransactionMode = TransactionMode.Test;
                                    break;
                                }
                            case "false":
                                {
                                    nTransactionMode = TransactionMode.Fail;
                                    break;
                                }
                            case "live":
                                {
                                    nTransactionMode = TransactionMode.Live;
                                    break;
                                }
                        }

                        // override the currency
                        // If oDictOpt.ContainsKey("currency") Then oDictOpt.Remove("currency")
                        // oDictOpt.Add("currency", oEwProv.mcCurrency)

                        // We want to use the currency specified in the payment method.
                        if (!Operators.ConditionalCompareObjectEqual(oDictOpt["currency"], oEwProv.mcCurrency, false))
                        {
                            oEwProv.mcCurrency = Convert.ToString(oDictOpt["currency"]);
                        }


                        // Set common variables
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictOpt["validateCV2"], "on", false)))
                            bCv2 = true;
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictOpt["secure3d"], "on", false)))
                            b3DSecure = true;
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictOpt["secure3d"], "v2", false)))
                            b3DSecureV2 = true;
                        // Commented out because not allowed on this form
                        // If oDictOpt("allowSavePayment") = "on" Then bAllowSavePayment = True

                        // Load the Xform
                        oEwProv.SetTransactionMode = nTransactionMode;
                        string PaymentDescription = "Make Payment of " + Strings.FormatNumber(oEwProv.mnPaymentAmount, 2) + " " + oEwProv.mcCurrency + " by Credit/Debit Card";
                        if (repeatAmt > 0d)
                        {
                            PaymentDescription = "Make Repeat Payments of " + repeatAmt + " " + oEwProv.mcCurrency + " per " + repeatInterval + " by Credit/Debit Card";
                        }
                        if (repeatAmt > 0d & oEwProv.mnPaymentAmount > 0)
                        {
                            PaymentDescription = "Make Initial Payment of " + Strings.FormatNumber(oEwProv.mnPaymentAmount, 2) + " " + oEwProv.mcCurrency + " and then " + repeatAmt + " " + oEwProv.mcCurrency + " per " + repeatInterval + " by Credit/Debit Card";
                        }

                        ccXform = oEwProv.creditCardXform(ref oOrder, "PayForm", sSubmitPath, oDictOpt["cardsAccepted"].ToString(), bCv2, PaymentDescription, b3DSecure, default, default, bAllowSavePayment);

                        string sType = "Billing Address";
                        XmlElement oCartAdd = (XmlElement)oOrder.SelectSingleNode("Contact[@type='" + sType + "']");

                        var ppRequestDetail = new Protean.PayPalAPI.DoDirectPaymentRequestDetailsType();
                        var ppRecuringRequestDetail = new Protean.PayPalAPI.CreateRecurringPaymentsProfileRequestDetailsType();



                        // only do this if we are not being redirected back
                        if (ccXform.valid)
                        {

                            // save some card details in session for later
                            myWeb.moSession["ExpireDate"] = myWeb.moRequest["creditCard/expireDate"];
                            myWeb.moSession["CardType"] = myWeb.moRequest["creditCard/type"];
                            myWeb.moSession["MaskedCard"] = stdTools.MaskString(myWeb.moRequest["creditCard/number"], "*", false, 4);

                            if (b3DSecureV2)
                            {
                                string FirstName = "";
                                string MiddleName = "";
                                string LastName = "";
                                string[] aGivenName = Strings.Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ");
                                switch (Information.UBound(aGivenName))
                                {
                                    case 0:
                                        {
                                            LastName = aGivenName[0];
                                            FirstName = aGivenName[0];
                                            break;
                                        }
                                    case 1:
                                        {
                                            FirstName = aGivenName[0];
                                            LastName = aGivenName[1];
                                            break;
                                        }
                                    case 2:
                                        {
                                            FirstName = aGivenName[0];
                                            MiddleName = aGivenName[1];
                                            LastName = aGivenName[2];
                                            break;
                                        }
                                    case 3:
                                        {
                                            FirstName = aGivenName[0];
                                            MiddleName = aGivenName[1];
                                            LastName = aGivenName[2];
                                            break;
                                        }
                                        // Suffix = aGivenName(3)
                                }

                                string Street1 = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(oCartAdd.SelectSingleNode("Company").InnerText), oCartAdd.SelectSingleNode("Street").InnerText, oCartAdd.SelectSingleNode("Company").InnerText));
                                string Street2 = "";
                                if (string.IsNullOrEmpty(oCartAdd.SelectSingleNode("Company").InnerText))
                                {
                                    Street2 = oCartAdd.SelectSingleNode("Street").InnerText;
                                }

                                var jwtPayload = new Dictionary<string, object>() { { "Payload", new Dictionary<string, object>() { { "Consumer", new Dictionary<string, object>() { { "Email1", "" }, { "Email2", "" }, { "ShippingAddress", new Dictionary<string, object>() { { "FullName", oCartAdd.SelectSingleNode("GivenName").InnerText }, { "FirstName", FirstName }, { "MiddleName", MiddleName }, { "LastName", LastName }, { "Address1", Street1 }, { "Address2", Street2 }, { "Address3", "" }, { "City", oCartAdd.SelectSingleNode("City").InnerText }, { "State", oCartAdd.SelectSingleNode("State").InnerText }, { "PostalCode", oCartAdd.SelectSingleNode("PostalCode").InnerText }, { "CountryCode", "" }, { "Phone1", "" }, { "Phone2", "" } } }, { "BillingAddress", new Dictionary<string, object>() { { "FullName", oCartAdd.SelectSingleNode("GivenName").InnerText }, { "FirstName", FirstName }, { "MiddleName", MiddleName }, { "LastName", LastName }, { "Address1", Street1 }, { "Address2", Street2 }, { "Address3", "" }, { "City", oCartAdd.SelectSingleNode("City").InnerText }, { "State", oCartAdd.SelectSingleNode("State").InnerText }, { "PostalCode", oCartAdd.SelectSingleNode("PostalCode").InnerText }, { "CountryCode", "" }, { "Phone1", "" }, { "Phone2", "" } } }, { "Account", new Dictionary<string, object>() { { "AccountNumber", myWeb.moRequest["creditCard/number"].Trim() }, { "ExpirationMonth", Conversions.ToInteger(Strings.Left(myWeb.moRequest["creditCard/expireDate"], 2)) }, { "ExpirationYear", Conversions.ToInteger(Strings.Right(myWeb.moRequest["creditCard/expireDate"], 4)) }, { "CardCode", myWeb.moRequest["creditCard/CV2"].Trim() }, { "NameOnAccount", oCartAdd.SelectSingleNode("GivenName").InnerText } } } } }, { "OrderDetails", new Dictionary<string, object>() { { "OrderNumber", oEwProv.mnCartId }, { "Amount", (oEwProv.mnPaymentAmount * 100).ToString() }, { "CurrencyCode", oDictOpt["centinalCurrencyCode"] }, { "TransactionId", oEwProv.mnCartId } } } } } };



                                var jwtHelper = new Tools.IdentityModel.Tokens.JwtHelper();
                                string sJwt = jwtHelper.GenerateJwt(Conversions.ToString(oDictOpt["centinalAppKey"]), Conversions.ToString(oDictOpt["centinalAppId"]), Conversions.ToString(oDictOpt["OrgUnitId"]), jwtPayload);

                                Xform3dSec = oEwProv.xfrmSecure3Dv2(ACSUrl, sJwt, oDictOpt["SongbirdUrl"].ToString(), sRedirectURL);

                            }

                            if (b3DSecure)
                            {

                                // first we check if the card is enrolled

                                string strErrorNo = "";
                                var centinelRequest = new CentinelRequest();
                                var centinelResponse = new CentinelResponse();

                                centinelRequest.add("MsgType", "cmpi_lookup");
                                centinelRequest.add("Version", "1.7");

                                // centinelRequest.add("ProcessorId", "134-01")
                                // centinelRequest.add("MerchantId", "trevor@eonic.co.uk")
                                // centinelRequest.add("TransactionPwd", "soNIC14")

                                centinelRequest.add("ProcessorId", Conversions.ToString(oDictOpt["centinalProcessorId"]));
                                centinelRequest.add("MerchantId", Conversions.ToString(oDictOpt["centinalMerchantId"]));

                                if (!string.IsNullOrEmpty(Conversions.ToString(Operators.ConcatenateObject(oDictOpt["centinalTransactionPwd"], ""))))
                                {
                                    centinelRequest.add("TransactionPwd", Conversions.ToString(oDictOpt["centinalTransactionPwd"]));
                                }

                                centinelRequest.add("TransactionType", "C");
                                centinelRequest.add("Amount", (oEwProv.mnPaymentAmount * 100).ToString()); // in pence
                                centinelRequest.add("CurrencyCode", Conversions.ToString(oDictOpt["centinalCurrencyCode"]));
                                centinelRequest.add("CardNumber", myWeb.moRequest["creditCard/number"].Trim());
                                centinelRequest.add("CardExpMonth", Strings.Left(myWeb.moRequest["creditCard/expireDate"], 2));
                                centinelRequest.add("CardExpYear", Strings.Right(myWeb.moRequest["creditCard/expireDate"], 4));
                                centinelRequest.add("OrderNumber", oEwProv.mnCartId);

                                string testRequest = centinelRequest.generatePayload(Conversions.ToString(oDictOpt["centinalTransactionPwd"]));

                                try
                                {
                                    string centinelURL = "https://paypal.cardinalcommerce.com/maps/txns.asp";
                                    if (nTransactionMode == TransactionMode.Test)
                                    {
                                        centinelURL = "https://centineltest.cardinalcommerce.com/maps/txns.asp";
                                    }
                                    centinelResponse = centinelRequest.sendHTTP(centinelURL, Conversions.ToInteger(oDictOpt["centinalTimeout"]));
                                }

                                catch (Exception ex)
                                {
                                    strErrorNo = "9040";
                                    err_msg = "3D Secure Communication Error - 9040";
                                }

                                strErrorNo = centinelResponse.getValue("ErrorNo");
                                err_msg = centinelResponse.getValue("ErrorDesc");

                                if (strErrorNo == "0")
                                {

                                    PAResStatus = centinelResponse.getValue("PAResStatus");
                                    Enrolled = centinelResponse.getValue("Enrolled");
                                    ACSUrl = centinelResponse.getValue("ACSUrl");
                                    Payload = centinelResponse.getValue("Payload");
                                    EciFlag = centinelResponse.getValue("EciFlag");
                                    CAVV = centinelResponse.getValue("Cavv");
                                    Xid = centinelResponse.getValue("Xid");

                                    if (Enrolled == "Y" & !(ACSUrl == "U" | ACSUrl == "N"))
                                    {
                                        // Card is enrolled and interface is active.
                                        if (oEwProv.moCartConfig["SecureURL"].EndsWith("/"))
                                        {
                                            sRedirectURL = oEwProv.moCartConfig["SecureURL"] + "?cartCmd=Redirect3ds&PaymentMethod=PayPalPro";
                                        }
                                        else
                                        {
                                            sRedirectURL = oEwProv.moCartConfig["SecureURL"] + "/?cartCmd=Redirect3ds&PaymentMethod=PayPalPro";
                                        }
                                        Xform3dSec = oEwProv.xfrmSecure3D(ACSUrl, oEwProv.mnCartId.ToString(), Payload, sRedirectURL);
                                    }
                                    else
                                    {
                                        // Card is not enrolled continue as usual
                                        b3DAuthorised = true;

                                    }
                                }
                                else
                                {
                                    ccXform.valid = false;
                                    b3DAuthorised = false;
                                }

                                centinelRequest = null;
                                centinelResponse = null;
                            }
                            else
                            {
                                b3DAuthorised = true;
                            }

                        }

                        // create the payment request
                        if (ccXform.valid)
                        {
                            // 'Setup the PayPal Payment Object.
                            var ppBillingAddress = new Protean.PayPalAPI.AddressType();

                            ppBillingAddress.Street1 = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(oCartAdd.SelectSingleNode("Company").InnerText), oCartAdd.SelectSingleNode("Street").InnerText, oCartAdd.SelectSingleNode("Company").InnerText));
                            if (string.IsNullOrEmpty(oCartAdd.SelectSingleNode("Company").InnerText))
                            {
                                ppBillingAddress.Street2 = oCartAdd.SelectSingleNode("Street").InnerText;
                            }
                            ppBillingAddress.CityName = oCartAdd.SelectSingleNode("City").InnerText;
                            ppBillingAddress.StateOrProvince = oCartAdd.SelectSingleNode("State").InnerText;
                            ppBillingAddress.PostalCode = oCartAdd.SelectSingleNode("PostalCode").InnerText;
                            ppBillingAddress.CountryName = oCartAdd.SelectSingleNode("Country").InnerText;

                            var ppCardOwnerName = new Protean.PayPalAPI.PersonNameType();
                            string[] aGivenName = Strings.Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ");
                            switch (Information.UBound(aGivenName))
                            {
                                case 0:
                                    {
                                        ppCardOwnerName.LastName = aGivenName[0];
                                        ppCardOwnerName.FirstName = aGivenName[0];
                                        break;
                                    }
                                case 1:
                                    {
                                        ppCardOwnerName.FirstName = aGivenName[0];
                                        ppCardOwnerName.LastName = aGivenName[1];
                                        break;
                                    }
                                case 2:
                                    {
                                        ppCardOwnerName.FirstName = aGivenName[0];
                                        ppCardOwnerName.MiddleName = aGivenName[1];
                                        ppCardOwnerName.LastName = aGivenName[2];
                                        break;
                                    }
                                case 3:
                                    {
                                        ppCardOwnerName.FirstName = aGivenName[0];
                                        ppCardOwnerName.MiddleName = aGivenName[1];
                                        ppCardOwnerName.LastName = aGivenName[2];
                                        ppCardOwnerName.Suffix = aGivenName[3];
                                        break;
                                    }
                            }

                            var ppCardOwner = new Protean.PayPalAPI.PayerInfoType();
                            ppCardOwner.Address = ppBillingAddress;
                            ppCardOwner.PayerName = ppCardOwnerName;



                            var ppCreditCard = new Protean.PayPalAPI.CreditCardDetailsType();

                            ppCreditCard.CardOwner = ppCardOwner;

                            ppCreditCard.CVV2 = myWeb.moRequest["creditCard/CV2"].Trim();

                            ppCreditCard.ExpMonth = Conversions.ToInteger(Strings.Left(myWeb.moRequest["creditCard/expireDate"], 2));
                            ppCreditCard.ExpYear = Conversions.ToInteger(Strings.Right(myWeb.moRequest["creditCard/expireDate"], 4));
                            ppCreditCard.ExpMonthSpecified = true;
                            ppCreditCard.ExpYearSpecified = true;
                            ppCreditCard.CreditCardNumber = myWeb.moRequest["creditCard/number"].Trim();

                            switch (Strings.UCase(myWeb.moRequest["creditCard/type"]) ?? "")
                            {
                                case "AMEX":
                                case "AMERICAN EXPRESS":
                                    {
                                        ppCreditCard.CreditCardType = Protean.PayPalAPI.CreditCardTypeType.Amex;
                                        break;
                                    }
                                case "DISCOVER":
                                    {
                                        ppCreditCard.CreditCardType = Protean.PayPalAPI.CreditCardTypeType.Discover;
                                        break;
                                    }
                                case "MASTERCARD":
                                case "MASTER CARD":
                                    {
                                        ppCreditCard.CreditCardType = Protean.PayPalAPI.CreditCardTypeType.MasterCard;
                                        break;
                                    }
                                case "SOLO":
                                    {
                                        ppCreditCard.CreditCardType = Protean.PayPalAPI.CreditCardTypeType.Solo;
                                        break;
                                    }
                                case "SWITCH":
                                case object _ when ppCreditCard.CreditCardType == Protean.PayPalAPI.CreditCardTypeType.Switch:
                                    {
                                        break;
                                    }
                                case "MAESTRO":
                                    {
                                        ppCreditCard.CreditCardType = Protean.PayPalAPI.CreditCardTypeType.Maestro;
                                        // For Maestro
                                        ppCreditCard.StartMonth = Conversions.ToInteger(Strings.Left(myWeb.moRequest["creditCard/issueDate"], 2));
                                        ppCreditCard.StartYear = Conversions.ToInteger(Strings.Right(myWeb.moRequest["creditCard/issueDate"], 4));
                                        ppCreditCard.IssueNumber = Conversions.ToInteger(myWeb.moRequest["creditCard/issueNumber"]).ToString();
                                        break;
                                    }
                                case "VISA":
                                    {
                                        ppCreditCard.CreditCardType = Protean.PayPalAPI.CreditCardTypeType.Visa;
                                        break;
                                    }
                            }

                            if (b3DSecure)
                            {
                                var pp3DsecReq = new Protean.PayPalAPI.ThreeDSecureRequestType();
                                pp3DsecReq.Xid = Xid;
                                pp3DsecReq.MpiVendor3ds = Enrolled;
                                pp3DsecReq.Cavv = CAVV;
                                pp3DsecReq.Eci3ds = EciFlag;
                                pp3DsecReq.AuthStatus3ds = PAResStatus;
                                ppCreditCard.ThreeDSecureRequest = pp3DsecReq;
                            }

                            // ' ###Details
                            // ' Let's you specify details of a payment amount.
                            var ppDetails = new Protean.PayPalAPI.PaymentDetailsType();

                            // ' ###Amount
                            // ' Let's you specify a payment amount.
                            var ppAmount = new Protean.PayPalAPI.BasicAmountType();
                            switch ((oEwProv.mcCurrency).ToUpper())
                            {
                                case "EUR":
                                    {
                                        ppAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.EUR;
                                        break;
                                    }
                                case "USD":
                                    {
                                        ppAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.USD;
                                        break;
                                    }
                                case "GBP":
                                    {
                                        ppAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.GBP;
                                        break;
                                    }

                                default:
                                    {
                                        ppAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.GBP;
                                        break;
                                    }
                            }

                            // Total must be equal to sum of shipping, tax and subtotal.
                            ppAmount.Value = oEwProv.mnPaymentAmount;

                            ppDetails.OrderTotal = ppAmount;
                            var RandGen = new Random();
                            ppDetails.InvoiceID = oCart.moCartConfig["OrderNoPrefix"] + oEwProv.mnCartId + "-" + RandGen.Next(1000, 9999).ToString();
                            ppDetails.PaymentAction = Protean.PayPalAPI.PaymentActionCodeType.Sale;

                            ppRequestDetail.CreditCard = ppCreditCard;
                            ppRequestDetail.PaymentDetails = ppDetails;
                            ppRequestDetail.IPAddress = Conversions.ToString(Interaction.IIf(myWeb.moRequest.ServerVariables["REMOTE_ADDR"] == "1", "88.97.12.224", myWeb.moRequest.ServerVariables["REMOTE_ADDR"]));
                            ppRequestDetail.MerchantSessionId = myWeb.moSession.SessionID;
                            ppRequestDetail.PaymentAction = Protean.PayPalAPI.PaymentActionCodeType.Sale;

                            if (repeatAmt > 0d)
                            {
                                var ppScheduleDetails = new Protean.PayPalAPI.ScheduleDetailsType();
                                var ppPaymentPeriod = new Protean.PayPalAPI.BillingPeriodDetailsType();
                                var ppRepeatAmount = new Protean.PayPalAPI.BasicAmountType();
                                switch ((oEwProv.mcCurrency).ToUpper())
                                {
                                    case "EUR":
                                        {
                                            ppRepeatAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.EUR;
                                            break;
                                        }
                                    case "USD":
                                        {
                                            ppRepeatAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.USD;
                                            break;
                                        }
                                    case "GBP":
                                        {
                                            ppRepeatAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.GBP;
                                            break;
                                        }

                                    default:
                                        {
                                            ppRepeatAmount.currencyID = Protean.PayPalAPI.CurrencyCodeType.GBP;
                                            break;
                                        }
                                }

                                ppRepeatAmount.Value = repeatAmt.ToString();

                                ppScheduleDetails.AutoBillOutstandingAmount = Protean.PayPalAPI.AutoBillType.AddToNextBilling;
                                ppPaymentPeriod.Amount = ppRepeatAmount;
                                ppPaymentPeriod.BillingFrequency = repeatFrequency;
                                switch (repeatInterval ?? "")
                                {
                                    case "day":
                                        {
                                            ppPaymentPeriod.BillingPeriod = Protean.PayPalAPI.BillingPeriodType.Day;
                                            break;
                                        }
                                    case "week":
                                        {
                                            ppPaymentPeriod.BillingPeriod = Protean.PayPalAPI.BillingPeriodType.Week;
                                            break;
                                        }
                                    case "month":
                                        {
                                            ppPaymentPeriod.BillingPeriod = Protean.PayPalAPI.BillingPeriodType.Month;
                                            break;
                                        }
                                    case "year":
                                        {
                                            ppPaymentPeriod.BillingPeriod = Protean.PayPalAPI.BillingPeriodType.Year;
                                            break;
                                        }
                                }
                                ppPaymentPeriod.TotalBillingCyclesSpecified = false;
                                ppScheduleDetails.PaymentPeriod = ppPaymentPeriod;
                                ppScheduleDetails.MaxFailedPaymentsSpecified = false;
                                ppScheduleDetails.Description = oCart.moCartConfig["OrderNoPrefix"] + oEwProv.mnCartId + " - " + oCartAdd.SelectSingleNode("GivenName").InnerText;

                                if (oEwProv.mnPaymentAmount > 0)
                                {
                                    ppScheduleDetails.ActivationDetails = new Protean.PayPalAPI.ActivationDetailsType();
                                    ppScheduleDetails.ActivationDetails.InitialAmount = ppAmount;
                                    ppScheduleDetails.ActivationDetails.FailedInitialAmountAction = Protean.PayPalAPI.FailedPaymentActionType.CancelOnFailure;
                                }
                                else
                                {
                                    bValidateFirst = true;
                                }

                                var ppRecuringProfileDetails = new Protean.PayPalAPI.RecurringPaymentsProfileDetailsType();

                                if (delayStart)
                                {
                                    switch (repeatInterval ?? "")
                                    {
                                        case "day":
                                            {
                                                startDate = DateAndTime.DateAdd(DateInterval.Day, repeatFrequency, DateTime.Now);
                                                break;
                                            }
                                        case "week":
                                            {
                                                startDate = DateAndTime.DateAdd(DateInterval.WeekOfYear, repeatFrequency, DateTime.Now);
                                                break;
                                            }
                                        case "month":
                                            {
                                                startDate = DateAndTime.DateAdd(DateInterval.Month, repeatFrequency, DateTime.Now);
                                                break;
                                            }
                                        case "year":
                                            {
                                                startDate = DateAndTime.DateAdd(DateInterval.Year, repeatFrequency, DateTime.Now);
                                                break;
                                            }
                                    }
                                }

                                ppRecuringProfileDetails.BillingStartDate = startDate;
                                ppRecuringProfileDetails.SubscriberName = ppCardOwnerName.FirstName + " " + ppCardOwnerName.LastName;
                                ppRecuringProfileDetails.SubscriberShippingAddress = ppBillingAddress;
                                ppRecuringProfileDetails.ProfileReference = "";

                                ppRecuringRequestDetail.CreditCard = ppCreditCard;
                                ppRecuringRequestDetail.ScheduleDetails = ppScheduleDetails;
                                ppRecuringRequestDetail.RecurringPaymentsProfileDetails = ppRecuringProfileDetails;

                            }
                        }

                        if (b3DSecure)
                        {
                            // check for return from aquiring bank
                            if (!string.IsNullOrEmpty(myWeb.moRequest["MD"]))
                            {
                                b3DAuthorised = true;
                                ppRequestDetail = (Protean.PayPalAPI.DoDirectPaymentRequestDetailsType)myWeb.moSession["ppPayment"];
                            }
                            else
                            {
                                myWeb.moSession["ppPayment"] = ppRequestDetail;
                            }
                        }
                        else if (ccXform.valid)
                        {
                            // 3DSec turned off so force authorised logic
                            b3DAuthorised = true;
                        }

                        if (ccXform.valid | b3DAuthorised)
                        {

                            if (b3DAuthorised)
                            {

                                try
                                {

                                    var ppProfile = new Protean.PayPalAPI.CustomSecurityHeaderType();
                                    ppProfile.Credentials = new Protean.PayPalAPI.UserIdPasswordType();
                                    ppProfile.Credentials.Username = Conversions.ToString(oDictOpt["apiUsername"]);
                                    ppProfile.Credentials.Password = Conversions.ToString(oDictOpt["apiPassword"]);
                                    ppProfile.Credentials.Signature = Conversions.ToString(oDictOpt["apiSignature"]);

                                    // Create a service Binding in code

                                    string endpointAddress = "https://api-3t.paypal.com/2.0/";
                                    if (nTransactionMode == TransactionMode.Test)
                                    {
                                        endpointAddress = "https://api-3t.sandbox.paypal.com/2.0/";
                                    }
                                    var ppEndpointAddress = new System.ServiceModel.EndpointAddress(endpointAddress);

                                    var ppBinding = getBinding();

                                    // Take the fixed payment

                                    if (repeatAmt > 0d)
                                    {
                                        // Setup the recurring payment
                                        var ppIface = new Protean.PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress);
                                        bool AuthSuccess = true;

                                        if (bValidateFirst)
                                        {
                                            // code to validate the credit card
                                            var ppIfaceAuth = new Protean.PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress);

                                            var ppAuthRequest = new Protean.PayPalAPI.DoDirectPaymentRequestType();
                                            ppAuthRequest.Version = "51.0";
                                            ppAuthRequest.DoDirectPaymentRequestDetails = ppRequestDetail;

                                            ppRequestDetail.PaymentAction = Protean.PayPalAPI.PaymentActionCodeType.Authorization;

                                            var ppAuthPaymentReq = new Protean.PayPalAPI.DoDirectPaymentReq();

                                            ppAuthPaymentReq.DoDirectPaymentRequest = ppAuthRequest;

                                            Protean.PayPalAPI.DoDirectPaymentResponseType ppAuthPaymentResponse;

                                            ppAuthPaymentResponse = ppIfaceAuth.DoDirectPayment(ref ppProfile, ppAuthPaymentReq);

                                            var ppAuthPaymentStatus = ppAuthPaymentResponse.PaymentStatus;

                                            if (ppAuthPaymentResponse.Ack == Protean.PayPalAPI.AckCodeType.SuccessWithWarning)
                                            {
                                                AuthSuccess = true;
                                            }
                                            else
                                            {
                                                // case for auth failure
                                                bIsValid = false;
                                                ccXform.valid = false;

                                                cProcessInfo = ppAuthPaymentStatus.ToString();

                                                if (ppAuthPaymentResponse.Errors is null)
                                                {
                                                    err_msg = "Response Not handled " + cProcessInfo;
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        Protean.PayPalAPI.ErrorType[] ppErrors = ppAuthPaymentResponse.Errors;

                                                        foreach (var ppError in ppErrors)
                                                            err_msg = err_msg + ppError.ErrorCode + " - " + ppError.ShortMessage + " - " + ppError.LongMessage;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        err_msg = "Response Not handled " + cProcessInfo;
                                                    }
                                                }
                                            }
                                        }

                                        if (AuthSuccess)
                                        {

                                            var ppRecuringRequest = new Protean.PayPalAPI.CreateRecurringPaymentsProfileRequestType();
                                            ppRecuringRequest.Version = "51.0";
                                            ppRecuringRequest.CreateRecurringPaymentsProfileRequestDetails = ppRecuringRequestDetail;

                                            var ppRecuringReq = new Protean.PayPalAPI.CreateRecurringPaymentsProfileReq();

                                            ppRecuringReq.CreateRecurringPaymentsProfileRequest = ppRecuringRequest;

                                            Protean.PayPalAPI.CreateRecurringPaymentsProfileResponseType ppRecuringResponse;

                                            var ppProfile2 = new Protean.PayPalAPI.CustomSecurityHeaderType();
                                            ppProfile2.Credentials = new Protean.PayPalAPI.UserIdPasswordType();
                                            ppProfile2.Credentials.Username = Conversions.ToString(oDictOpt["apiUsername"]);
                                            ppProfile2.Credentials.Password = Conversions.ToString(oDictOpt["apiPassword"]);
                                            ppProfile2.Credentials.Signature = Conversions.ToString(oDictOpt["apiSignature"]);

                                            ppRecuringResponse = ppIface.CreateRecurringPaymentsProfile(ref ppProfile2, ppRecuringReq);

                                            Protean.PayPalAPI.CreateRecurringPaymentsProfileResponseDetailsType ppRecuringResponseDetails;
                                            ppRecuringResponseDetails = ppRecuringResponse.CreateRecurringPaymentsProfileResponseDetails;

                                            Protean.PayPalAPI.PaymentStatusCodeType ppPaymentStatus = (Protean.PayPalAPI.PaymentStatusCodeType)ppRecuringResponseDetails.ProfileStatus;

                                            switch (ppPaymentStatus)
                                            {
                                                case Protean.PayPalAPI.PaymentStatusCodeType.Completed:
                                                case Protean.PayPalAPI.PaymentStatusCodeType.Processed:
                                                case Protean.PayPalAPI.PaymentStatusCodeType.None:
                                                    {

                                                        cAuthCode = ppRecuringResponseDetails.ProfileID;
                                                        if (!string.IsNullOrEmpty(cAuthCode))
                                                        {
                                                            bIsValid = true;
                                                            PaymemtRef = ppRecuringResponseDetails.ProfileID;
                                                            err_msg = "Repeat Payment Authorised Ref " + ppRecuringResponseDetails.ProfileID;
                                                            ccXform.valid = true;
                                                            bSavePayment = true;
                                                        }
                                                        else
                                                        {
                                                            bIsValid = false;
                                                            ccXform.valid = false;
                                                            Protean.PayPalAPI.ErrorType[] ppErrors = ppRecuringResponse.Errors;

                                                            foreach (var ppError in ppErrors)
                                                                err_msg = err_msg + ppError.LongMessage;
                                                        }

                                                        break;
                                                    }

                                                case Protean.PayPalAPI.PaymentStatusCodeType.Failed:
                                                case Protean.PayPalAPI.PaymentStatusCodeType.Expired:
                                                    {

                                                        bIsValid = false;
                                                        ccXform.valid = false;
                                                        Protean.PayPalAPI.ErrorType[] ppErrors = ppRecuringResponse.Errors;

                                                        foreach (var ppError in ppErrors)
                                                            err_msg = err_msg + ppError.LongMessage;
                                                        break;
                                                    }

                                                default:
                                                    {

                                                        bIsValid = false;
                                                        ccXform.valid = false;

                                                        cProcessInfo = ppPaymentStatus.ToString();

                                                        if (ppRecuringResponse.Errors is null)
                                                        {
                                                            err_msg = "Response Not handled " + cProcessInfo;
                                                        }
                                                        else
                                                        {
                                                            try
                                                            {
                                                                Protean.PayPalAPI.ErrorType[] ppErrors = ppRecuringResponse.Errors;

                                                                foreach (var ppError in ppErrors)
                                                                    err_msg = err_msg + ppError.ErrorCode + " - " + ppError.ShortMessage + " - " + ppError.LongMessage;
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                err_msg = "Response Not handled " + cProcessInfo;
                                                            }
                                                        }

                                                        break;
                                                    }

                                            }
                                        }
                                    }


                                    // Try a standard payment
                                    else if (oEwProv.mnPaymentAmount > 0 & string.IsNullOrEmpty(err_msg))
                                    {

                                        var ppIface2 = new Protean.PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress);

                                        var ppRequest = new Protean.PayPalAPI.DoDirectPaymentRequestType();
                                        ppRequest.Version = "51.0";
                                        ppRequest.DoDirectPaymentRequestDetails = ppRequestDetail;

                                        var ppPaymentReq = new Protean.PayPalAPI.DoDirectPaymentReq();

                                        ppPaymentReq.DoDirectPaymentRequest = ppRequest;

                                        Protean.PayPalAPI.DoDirectPaymentResponseType ppPaymentResponse;

                                        ppPaymentResponse = ppIface2.DoDirectPayment(ref ppProfile, ppPaymentReq);

                                        var ppPaymentStatus = ppPaymentResponse.PaymentStatus;

                                        string responseState = "";

                                        switch (ppPaymentStatus)
                                        {
                                            case Protean.PayPalAPI.PaymentStatusCodeType.Completed:
                                            case Protean.PayPalAPI.PaymentStatusCodeType.Processed:
                                            case Protean.PayPalAPI.PaymentStatusCodeType.None:
                                                {

                                                    cAuthCode = ppPaymentResponse.TransactionID;
                                                    if (!string.IsNullOrEmpty(cAuthCode))
                                                    {
                                                        bIsValid = true;
                                                        PaymemtRef = ppPaymentResponse.TransactionID;
                                                        err_msg = "Payment Authorised Ref " + ppPaymentResponse.TransactionID;
                                                        ccXform.valid = true;
                                                        bSavePayment = true;
                                                    }
                                                    else
                                                    {
                                                        bIsValid = false;
                                                        ccXform.valid = false;
                                                        Protean.PayPalAPI.ErrorType[] ppErrors = ppPaymentResponse.Errors;

                                                        foreach (var ppError in ppErrors)
                                                            err_msg = err_msg + ppError.ShortMessage + " - " + ppError.ErrorCode + " " + ppError.LongMessage;
                                                    }

                                                    break;
                                                }

                                            case Protean.PayPalAPI.PaymentStatusCodeType.Pending:
                                                {

                                                    cAuthCode = ppPaymentResponse.TransactionID;
                                                    bIsValid = true;
                                                    err_msg = "Payment Pending - Reason " + ppPaymentResponse.PendingReason.ToString() + " - " + ppPaymentResponse.TransactionID;
                                                    ccXform.valid = true;
                                                    bSavePayment = true;
                                                    break;
                                                }

                                            case Protean.PayPalAPI.PaymentStatusCodeType.Failed:
                                            case Protean.PayPalAPI.PaymentStatusCodeType.Expired:
                                                {

                                                    bIsValid = false;
                                                    ccXform.valid = false;
                                                    Protean.PayPalAPI.ErrorType[] ppErrors = ppPaymentResponse.Errors;

                                                    foreach (var ppError in ppErrors)
                                                        err_msg = err_msg + ppError.LongMessage;
                                                    break;
                                                }

                                            default:
                                                {

                                                    bIsValid = false;
                                                    ccXform.valid = false;

                                                    cProcessInfo = ppPaymentResponse.PaymentStatus.ToString();

                                                    if (ppPaymentResponse.Errors is null)
                                                    {
                                                        err_msg = "Response Not handled " + cProcessInfo;
                                                    }
                                                    else
                                                    {
                                                        try
                                                        {
                                                            Protean.PayPalAPI.ErrorType[] ppErrors = ppPaymentResponse.Errors;

                                                            foreach (var ppError in ppErrors)
                                                                err_msg = err_msg + ppError.ErrorCode + " - " + ppError.ShortMessage + " - " + ppError.LongMessage;
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            err_msg = "Response Not handled: " + cProcessInfo;
                                                        }
                                                    }

                                                    break;
                                                }

                                        }

                                    }
                                }




                                catch (Exception ex)
                                {
                                    bIsValid = false;
                                    ccXform.valid = false;
                                    // Dim ppError As ErrorType

                                    err_msg = err_msg + " Error:" + ex.Message;
                                    err_msg = err_msg + " Msg:" + ex.StackTrace;

                                }
                            }
                            else
                            {
                                ccXform.valid = false;
                            }
                            // bSavePayment = True
                        }

                        if (bSavePayment)
                        {
                            // We Save the payment method prior to ultimate validation because we only have access to the request object at the point it is submitted

                            // only do this for a valid payment method
                            XmlElement oSaveElmt = (XmlElement)ccXform.Instance.SelectSingleNode("creditCard/bSavePayment");
                            Debug.WriteLine(myWeb.moRequest["creditCard/expireDate"]);
                            var oDate = new DateTime(Conversions.ToInteger("20" + Strings.Right(Conversions.ToString(myWeb.moSession["ExpireDate"]), 2)), Conversions.ToInteger(Strings.Left(Conversions.ToString(myWeb.moSession["ExpireDate"]), 2)), 1);
                            oDate = oDate.AddMonths(1);
                            oDate = oDate.AddDays(-1);
                            string cMethodName = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(myWeb.moSession["CardType"], ": "), myWeb.moSession["MaskedCard"]), " Expires: "), oDate.ToShortDateString()));
                            // cAuthCode = oDictResp("auth_code")

                            var oPayPalElmt = ccXform.Instance.OwnerDocument.CreateElement("PayPal");
                            oPayPalElmt.SetAttribute("AuthCode", cAuthCode);
                            ccXform.Instance.FirstChild.AppendChild(oPayPalElmt);

                            if (oSaveElmt != null)
                            {
                                if (oSaveElmt.InnerText == "true" & bIsValid)
                                {
                                    // oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "PayPalPro", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, oDate, True, oEwProv.mnPaymentAmount)
                                    oCart.mnPaymentId = myWeb.moDbHelper.savePayment(oCart.mnCartId, myWeb.mnUserId, "PayPalPro", cAuthCode, "Credit Card", ccXform.Instance.FirstChild, DateTime.Now, false, oEwProv.mnPaymentAmount); // 0 amount paid as yet
                                }
                                else
                                {
                                    // oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "PayPalPro", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount)
                                    oCart.mnPaymentId = myWeb.moDbHelper.savePayment(oCart.mnCartId, myWeb.mnUserId, "PayPalPro", cAuthCode, "Credit Card", ccXform.Instance.FirstChild, DateTime.Now, false, oEwProv.mnPaymentAmount);
                                } // 0 amount paid as yet
                            }
                            else
                            {
                                // oCart.mnPaymentId = oEwProv.savePayment(myWeb.mnUserId, "PayPalPro", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, Now, False, oEwProv.mnPaymentAmount)
                                oCart.mnPaymentId = myWeb.moDbHelper.savePayment(oCart.mnCartId, myWeb.mnUserId, "PayPalPro", cAuthCode, "Credit Card", ccXform.Instance.FirstChild, DateTime.Now, false, oEwProv.mnPaymentAmount);

                            } // 0 amount paid as yet

                            myWeb.moSession["ExpireDate"] = (object)null;
                            myWeb.moSession["CardType"] = (object)null;
                            myWeb.moSession["MaskedCard"] = (object)null;

                        }

                        if (!string.IsNullOrEmpty(err_msg))
                        {
                            XmlNode argoNode = (XmlNode)ccXform.moXformElmt;
                            ccXform.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, err_msg);
                        }

                        // Update Seller Notes:

                        sSql = "select * from tblCartOrder where nCartOrderKey = " + oEwProv.mnCartId;
                        DataSet oDs;
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                        {
                            if (bIsValid)
                            {
                                oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), DateTime.Today), " "), DateAndTime.TimeOfDay), ": changed to: (Payment Received) "), Constants.vbLf), "comment: "), err_msg), Constants.vbLf), "Full Response:' "), cResponse), "'");
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(err_msg_log))
                                    err_msg_log = err_msg;
                                oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), DateTime.Today), " "), DateAndTime.TimeOfDay), ": changed to: (Payment Failed) "), Constants.vbLf), "comment: "), err_msg_log), Constants.vbLf), "Full Response:' "), cResponse), "'");
                            }
                            if (b3DSecure & bIsValid == false)
                            {
                                oRow["cPaymentRef"] = sPaymentRef;
                            }
                        }

                        myWeb.moDbHelper.updateDataset(ref oDs, "Order");

                        if (Xform3dSec != null)
                        {

                            // Save the payment object into the session
                            return Xform3dSec;
                        }
                        else
                        {
                            return ccXform;
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug);
                        return (Protean.xForm)null;
                    }
                }

                public new Protean.xForm GetRedirect3dsForm(ref Cms myWeb)
                {
                    myWeb.PerfMon.Log("EPDQ", "xfrmSecure3DReturn");
                    var moCartConfig = myWeb.moCart.moCartConfig;
                    Protean.xForm oXform = new Cms.xForm(ref myWeb.msException);
                    XmlElement oFrmInstance;
                    XmlElement oFrmGroup;
                    string RedirectURL;

                    string cProcessInfo = "xfrmSecure3DReturn";
                    try
                    {
                        oXform.moPageXML = myWeb.moPageXml;

                        if (moCartConfig["SecureURL"].EndsWith("/"))
                        {
                            RedirectURL = moCartConfig["SecureURL"] + "?cartCmd=SubmitPaymentDetails&paymentMethod=" + myWeb.moCart.mcPaymentMethod;
                        }
                        else
                        {
                            RedirectURL = moCartConfig["SecureURL"] + "/?cartCmd=SubmitPaymentDetails&paymentMethod=" + myWeb.moCart.mcPaymentMethod;
                        }

                        // create the instance
                        oXform.NewFrm("Secure3DReturn");
                        oXform.submission("Secure3DReturn", goServer.UrlDecode(RedirectURL), "POST", "return form_check(this);");
                        oFrmInstance = oXform.moPageXML.CreateElement("Secure3DReturn");
                        oXform.Instance.AppendChild(oFrmInstance);
                        oFrmGroup = oXform.addGroup(ref oXform.moXformElmt, "Secure3DReturn1", "Secure3DReturn1", "Redirecting... Please do not refresh");

                        foreach (var item in myWeb.moRequest.Form)
                        {
                            XmlNode newInput = oXform.addInput(ref oFrmGroup, Conversions.ToString(item), false, Conversions.ToString(item), "hidden");
                            XmlElement argoInputNode = (XmlElement)newInput;
                            oXform.addValue(ref argoInputNode, myWeb.moRequest.Form[Conversions.ToString(item)]);
                            newInput = argoInputNode;
                        }

                        // build the form and the binds
                        // oXform.addDiv(oFrmGroup, "<SCRIPT LANGUAGE=""Javascript"">function onXformLoad(){document.Secure3DReturn.submit();};appendLoader(onXformLoad);</SCRIPT>")
                        oXform.addSubmit(ref oFrmGroup, "Secure3DReturn", "Continue", "ewSubmit");
                        oXform.addValues();
                        return oXform;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetRedirect3dsForm", ex, "", cProcessInfo, gbDebug);
                        return (Protean.xForm)null;
                    }

                }

                public Protean.xForm xfrmSecure3DReturn(string acs_url)
                {
                    myWeb.PerfMon.Log("EPDQ", "xfrmSecure3DReturn");
                    Protean.xForm oXform = new Cms.xForm(ref myWeb.msException);
                    XmlElement oFrmInstance;
                    XmlElement oFrmGroup;

                    string cProcessInfo = "xfrmSecure3D";
                    try
                    {
                        oXform.moPageXML = myWeb.moPageXml;

                        // create the instance
                        oXform.NewFrm("Secure3DReturn");
                        oXform.submission("Secure3DReturn", goServer.UrlDecode(acs_url), "POST", "return form_check(this);");
                        oFrmInstance = oXform.moPageXML.CreateElement("Secure3DReturn");
                        oXform.Instance.AppendChild(oFrmInstance);
                        oFrmGroup = oXform.addGroup(ref oXform.moXformElmt, "Secure3DReturn1", "Secure3DReturn1", "Redirect to 3D Secure");
                        // build the form and the binds
                        // oXform.addDiv(oFrmGroup, "<SCRIPT LANGUAGE=""Javascript"">function onXformLoad(){document.Secure3DReturn.submit();};appendLoader(onXformLoad);</SCRIPT>")
                        oXform.addSubmit(ref oFrmGroup, "Secure3DReturn", "Show Invoice", "ewSubmit");
                        oXform.addValues();
                        return oXform;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "xfrmSecure3D", ex, "", cProcessInfo, gbDebug);
                        return (Protean.xForm)null;
                    }

                }

                public string getCountryISONum(ref string sCountry)
                {
                    myWeb.PerfMon.Log(mcModuleName, "getCountryISONum");
                    // Dim oDr As SqlDataReader
                    string sSql;
                    string strReturn = "";
                    string cProcessInfo = "getCountryISONum";
                    try
                    {

                        sSql = "select cLocationISOnum from tblCartShippingLocations where cLocationNameFull Like '" + sCountry + "' or cLocationNameShort Like '" + sCountry + "'";
                        using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            if (oDr.HasRows)
                            {
                                while (oDr.Read())
                                    strReturn = Conversions.ToString(oDr["cLocationISOnum"]);
                            }
                            else
                            {
                                strReturn = "";
                            }
                        }
                        return strReturn;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "getCountryISOnum", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public new string CheckStatus(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";

                    XmlNode oPayPalProCfg;
                    var nTransactionMode = default(TransactionMode);

                    try
                    {
                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        oPayPalProCfg = moPaymentCfg.SelectSingleNode("provider[@name='PayPalPro']");

                        var ppProfile = new Protean.PayPalAPI.CustomSecurityHeaderType();
                        ppProfile.Credentials = new Protean.PayPalAPI.UserIdPasswordType();
                        ppProfile.Credentials.Username = oPayPalProCfg.SelectSingleNode("apiUsername").Attributes["value"].Value;
                        ppProfile.Credentials.Password = oPayPalProCfg.SelectSingleNode("apiPassword").Attributes["value"].Value;
                        ppProfile.Credentials.Signature = oPayPalProCfg.SelectSingleNode("apiSignature").Attributes["value"].Value;


                        switch (oPayPalProCfg.SelectSingleNode("opperationMode").Attributes["value"].Value ?? "")
                        {
                            case "true":
                            case "test":
                                {
                                    nTransactionMode = TransactionMode.Test;
                                    break;
                                }
                            case "false":
                                {
                                    nTransactionMode = TransactionMode.Fail;
                                    break;
                                }
                            case "live":
                                {
                                    nTransactionMode = TransactionMode.Live;
                                    break;
                                }
                        }

                        // Create a service Binding in code

                        string endpointAddress = "https://api-3t.paypal.com/2.0/";
                        if (nTransactionMode == TransactionMode.Test)
                        {
                            endpointAddress = "https://api-3t.sandbox.paypal.com/2.0/";
                        }
                        var ppEndpointAddress = new System.ServiceModel.EndpointAddress(endpointAddress);

                        var ppBinding = getBinding();

                        var ppGetRecurringPaymentsProfileDetailsReq = new Protean.PayPalAPI.GetRecurringPaymentsProfileDetailsReq();

                        var ppGetRecurringPaymentsProfileDetailsRequestType = new Protean.PayPalAPI.GetRecurringPaymentsProfileDetailsRequestType();
                        ppGetRecurringPaymentsProfileDetailsRequestType.ProfileID = nPaymentProviderRef;
                        ppGetRecurringPaymentsProfileDetailsRequestType.Version = "51.0";

                        ppGetRecurringPaymentsProfileDetailsReq.GetRecurringPaymentsProfileDetailsRequest = ppGetRecurringPaymentsProfileDetailsRequestType;

                        var ppIface = new Protean.PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress);
                        Protean.PayPalAPI.GetRecurringPaymentsProfileDetailsResponseType ppRecuringResponse;
                        try
                        {
                            ppRecuringResponse = ppIface.GetRecurringPaymentsProfileDetails(ref ppProfile, ppGetRecurringPaymentsProfileDetailsReq);

                            switch (ppRecuringResponse.GetRecurringPaymentsProfileDetailsResponseDetails.ProfileStatus)
                            {
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.ActiveProfile:
                                    {
                                        return "active";
                                    }
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.CancelledProfile:
                                    {
                                        return "cancelled";
                                    }
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.ExpiredProfile:
                                    {
                                        return "expired";
                                    }
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.PendingProfile:
                                    {
                                        return "pending";
                                    }
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.SuspendedProfile:
                                    {
                                        return "suspended";
                                    }

                                default:
                                    {
                                        return "Error - no value returned";
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            return "Error - no value returned " + ex.Message;
                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }

                public new XmlElement UpdatePaymentStatus(ref Cms oWeb, ref long nPaymentMethodKey)
                {
                    string cProcessInfo = "";
                    XmlNode oPayPalProCfg;
                    var nTransactionMode = default(TransactionMode);
                    try
                    {
                        var oInstance = base.UpdatePaymentStatus(ref oWeb, ref nPaymentMethodKey);

                        string cPayMthdProviderRef;
                        cPayMthdProviderRef = oInstance.SelectSingleNode("cPayMthdProviderRef").InnerText;

                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        oPayPalProCfg = moPaymentCfg.SelectSingleNode("provider[@name='PayPalPro']");

                        var ppProfile = new Protean.PayPalAPI.CustomSecurityHeaderType();
                        ppProfile.Credentials = new Protean.PayPalAPI.UserIdPasswordType();
                        ppProfile.Credentials.Username = oPayPalProCfg.SelectSingleNode("apiUsername").Attributes["value"].Value;
                        ppProfile.Credentials.Password = oPayPalProCfg.SelectSingleNode("apiPassword").Attributes["value"].Value;
                        ppProfile.Credentials.Signature = oPayPalProCfg.SelectSingleNode("apiSignature").Attributes["value"].Value;


                        switch (oPayPalProCfg.SelectSingleNode("opperationMode").Attributes["value"].Value ?? "")
                        {
                            case "true":
                            case "test":
                                {
                                    nTransactionMode = TransactionMode.Test;
                                    break;
                                }
                            case "false":
                                {
                                    nTransactionMode = TransactionMode.Fail;
                                    break;
                                }
                            case "live":
                                {
                                    nTransactionMode = TransactionMode.Live;
                                    break;
                                }
                        }

                        // Create a service Binding in code

                        string endpointAddress = "https://api-3t.paypal.com/2.0/";
                        if (nTransactionMode == TransactionMode.Test)
                        {
                            endpointAddress = "https://api-3t.sandbox.paypal.com/2.0/";
                        }
                        var ppEndpointAddress = new System.ServiceModel.EndpointAddress(endpointAddress);

                        var ppBinding = getBinding();

                        var ppGetRecurringPaymentsProfileDetailsReq = new Protean.PayPalAPI.GetRecurringPaymentsProfileDetailsReq();

                        var ppGetRecurringPaymentsProfileDetailsRequestType = new Protean.PayPalAPI.GetRecurringPaymentsProfileDetailsRequestType();
                        ppGetRecurringPaymentsProfileDetailsRequestType.ProfileID = cPayMthdProviderRef;
                        ppGetRecurringPaymentsProfileDetailsRequestType.Version = "51.0";

                        ppGetRecurringPaymentsProfileDetailsReq.GetRecurringPaymentsProfileDetailsRequest = ppGetRecurringPaymentsProfileDetailsRequestType;

                        var ppIface = new Protean.PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress);
                        Protean.PayPalAPI.GetRecurringPaymentsProfileDetailsResponseType ppRecuringResponse;
                        string cStatus;
                        try
                        {
                            ppRecuringResponse = ppIface.GetRecurringPaymentsProfileDetails(ref ppProfile, ppGetRecurringPaymentsProfileDetailsReq);

                            switch (ppRecuringResponse.GetRecurringPaymentsProfileDetailsResponseDetails.ProfileStatus)
                            {
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.ActiveProfile:
                                    {
                                        cStatus = "active";
                                        break;
                                    }
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.CancelledProfile:
                                    {
                                        cStatus = "cancelled";
                                        break;
                                    }
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.ExpiredProfile:
                                    {
                                        cStatus = "expired";
                                        break;
                                    }
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.PendingProfile:
                                    {
                                        cStatus = "pending";
                                        break;
                                    }
                                case Protean.PayPalAPI.RecurringPaymentsProfileStatusType.SuspendedProfile:
                                    {
                                        cStatus = "suspended";
                                        break;
                                    }

                                default:
                                    {
                                        cStatus = "Error - no value returned";
                                        break;
                                    }
                            }
                        }
                        catch (Exception ex)
                        {
                            cStatus = "Error - no value returned " + ex.Message;
                        }

                        oInstance.SelectSingleNode("cPayMthdDescription").InnerText = cStatus;

                        oInstance.SelectSingleNode("dUpdateDate").InnerText = XmlDate(DateTime.Now);

                        if (cStatus == "active")
                        {
                            oInstance.SelectSingleNode("nStatus").InnerText = "1";
                        }
                        else
                        {
                            oInstance.SelectSingleNode("nStatus").InnerText = "0";
                        }

                        var newInstance = oWeb.moPageXml.CreateElement("instance");
                        newInstance.AppendChild(oInstance);
                        oWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartPaymentMethod, newInstance);
                        return oInstance;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdatePaymentStatus", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }

                }

                public string CancelPayments(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";
                    XmlNode oPayPalProCfg;

                    try
                    {

                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");

                        oPayPalProCfg = moPaymentCfg.SelectSingleNode("provider[@name='PayPalPro']");

                        var ppProfile = new Protean.PayPalAPI.CustomSecurityHeaderType();
                        ppProfile.Credentials = new Protean.PayPalAPI.UserIdPasswordType();
                        ppProfile.Credentials.Username = oPayPalProCfg.SelectSingleNode("apiUsername").Attributes["value"].Value;
                        ppProfile.Credentials.Password = oPayPalProCfg.SelectSingleNode("apiPassword").Attributes["value"].Value;
                        ppProfile.Credentials.Signature = oPayPalProCfg.SelectSingleNode("apiSignature").Attributes["value"].Value;


                        switch (oPayPalProCfg.SelectSingleNode("opperationMode").Attributes["value"].Value ?? "")
                        {
                            case "true":
                            case "test":
                                {
                                    nTransactionMode = TransactionMode.Test;
                                    break;
                                }
                            case "false":
                                {
                                    nTransactionMode = TransactionMode.Fail;
                                    break;
                                }
                            case "live":
                                {
                                    nTransactionMode = TransactionMode.Live;
                                    break;
                                }
                        }

                        // Create a service Binding in code

                        string endpointAddress = "https://api-3t.paypal.com/2.0/";
                        if (nTransactionMode == TransactionMode.Test)
                        {
                            endpointAddress = "https://api-3t.sandbox.paypal.com/2.0/";
                        }
                        var ppEndpointAddress = new System.ServiceModel.EndpointAddress(endpointAddress);

                        var ppBinding = getBinding();

                        var ppManageRecurringPaymentsProfileStatusReq = new Protean.PayPalAPI.ManageRecurringPaymentsProfileStatusReq();

                        var ppManageRecurringPaymentsProfileStatusRequestType = new Protean.PayPalAPI.ManageRecurringPaymentsProfileStatusRequestType();
                        ppManageRecurringPaymentsProfileStatusReq.ManageRecurringPaymentsProfileStatusRequest = ppManageRecurringPaymentsProfileStatusRequestType;
                        ppManageRecurringPaymentsProfileStatusRequestType.Version = "51.0";

                        var ppManageRecurringPaymentsProfileStatusRequestDetailsType = new Protean.PayPalAPI.ManageRecurringPaymentsProfileStatusRequestDetailsType();
                        ppManageRecurringPaymentsProfileStatusRequestType.ManageRecurringPaymentsProfileStatusRequestDetails = ppManageRecurringPaymentsProfileStatusRequestDetailsType;

                        ppManageRecurringPaymentsProfileStatusRequestDetailsType.ProfileID = nPaymentProviderRef;
                        ppManageRecurringPaymentsProfileStatusRequestDetailsType.Action = Protean.PayPalAPI.StatusChangeActionType.Cancel;


                        var ppIface = new Protean.PayPalAPI.PayPalAPIAAInterfaceClient(ppBinding, ppEndpointAddress);
                        Protean.PayPalAPI.ManageRecurringPaymentsProfileStatusResponseType ppRecuringResponse;

                        ppRecuringResponse = ppIface.ManageRecurringPaymentsProfileStatus(ref ppProfile, ppManageRecurringPaymentsProfileStatusReq);

                        return CheckStatus(ref oWeb, ref nPaymentProviderRef);
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }

                public new bool AddPaymentButton(ref Protean.xForm oOptXform, ref XmlElement oFrmElmt, XmlElement configXml, double nPaymentAmount, string submissionValue, string refValue)
                {

                    string PaymentLabel = configXml.SelectSingleNode("description/@value").InnerText;
                    // allow html in description node...
                    bool bXmlLabel = false;

                    if (!string.IsNullOrEmpty(configXml.SelectSingleNode("description").InnerXml))
                    {
                        PaymentLabel = configXml.SelectSingleNode("description").InnerXml;
                        bXmlLabel = true;
                    }

                    string iconclass = "";
                    if (configXml.SelectSingleNode("icon/@value") != null)
                    {
                        iconclass = configXml.SelectSingleNode("icon/@value").InnerText;
                    }

                    oOptXform.addSubmit(ref oFrmElmt, submissionValue, PaymentLabel, refValue, "pay-button pay-" + configXml.GetAttribute("name"), iconclass, configXml.GetAttribute("name"));
                    return default;

                }


                public string GetMethodDetail(ref Cms oWeb, ref long nPaymentProviderId)
                {
                    string cProcessInfo = "";
                    try
                    {


                        try
                        {
                            var oMethodInfo = oWeb.moPageXml.CreateElement("MethodInfo");

                            return oMethodInfo.OuterXml;
                        }


                        catch (Exception ex)
                        {
                            string ErrorMsg = "Error";
                            return ErrorMsg;
                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetMethodDetail", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }
                public System.ServiceModel.BasicHttpBinding getBinding()
                {

                    var ppBinding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport);
                    ppBinding.MaxBufferSize = 65536;
                    ppBinding.CloseTimeout = new TimeSpan(0, 2, 0);
                    ppBinding.OpenTimeout = new TimeSpan(0, 2, 0);
                    ppBinding.ReceiveTimeout = new TimeSpan(0, 10, 0);
                    ppBinding.SendTimeout = new TimeSpan(0, 2, 0);
                    ppBinding.AllowCookies = false;
                    ppBinding.BypassProxyOnLocal = false;
                    ppBinding.HostNameComparisonMode = System.ServiceModel.HostNameComparisonMode.StrongWildcard;
                    ppBinding.MaxBufferSize = 65536;
                    ppBinding.MaxBufferPoolSize = 524288L;
                    ppBinding.MaxReceivedMessageSize = 65536L;
                    ppBinding.MessageEncoding = System.ServiceModel.WSMessageEncoding.Text;
                    ppBinding.TextEncoding = System.Text.Encoding.UTF8;
                    ppBinding.TransferMode = System.ServiceModel.TransferMode.Buffered;
                    ppBinding.UseDefaultWebProxy = true;

                    ppBinding.ReaderQuotas.MaxDepth = 32;
                    ppBinding.ReaderQuotas.MaxStringContentLength = 8192;
                    ppBinding.ReaderQuotas.MaxArrayLength = 16384;
                    ppBinding.ReaderQuotas.MaxBytesPerRead = 4096;
                    ppBinding.ReaderQuotas.MaxNameTableCharCount = 65536;

                    return ppBinding;
                }

            }
        }
    }
}