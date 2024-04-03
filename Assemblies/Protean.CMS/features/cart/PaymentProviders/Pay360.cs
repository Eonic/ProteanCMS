using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.Cms.Cart;
using static Protean.stdTools;
using static Protean.Tools.Xml;


namespace Protean.Providers
{
    namespace Payment
    {
        public class Pay360
        {

            public Pay360()
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

                private const string mcModuleName = "Providers.Payment.Pay360.Activities";
                private new Cms myWeb;
                protected XmlNode moPaymentCfg;
                private TransactionMode nTransactionMode;

                public enum TransactionMode
                {
                    Live = 0,
                    Test = 1,
                    Fail = 2
                }


                private string[] ScheduleIntervals = new string[] { "yearly", "half-yearly", "quarterly", "monthly", "weekly", "daily" };



                public new Protean.xForm GetPaymentForm(ref Cms oWeb, ref Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails")
                {
                    oWeb.PerfMon.Log("PaymentProviders", "GetPaymentForm");
                    string sSql;

                    Protean.xForm ccXform;

                    Protean.xForm Xform3dSec = (Protean.xForm)null;

                    var bIsValid = default(bool);
                    string err_msg = "";
                    string err_msg_log = "";
                    string sProcessInfo = "";
                    string[] aResponse;
                    Hashtable oDictResp;
                    string cResponse = string.Empty;

                    var oDictOpt = new Hashtable();
                    string sSubmitPath = oCart.mcPagePath + returnCmd;
                    int i;
                    int nPos;
                    string sOpts;
                    string cOrderType;
                    bool bCv2 = false;
                    bool b3DSecure = false;
                    bool b3DAuthorised = false;
                    string sRedirectURL = "";
                    string sPaymentRef = "";

                    string cProcessInfo = "payGetPaymentForm";

                    // Get the payment options into a hashtable
                    XmlNode oPay360Cfg;
                    bool bSavePayment = false;
                    bool bAllowSavePayment = false;
                    string sProfile = "";

                    try
                    {
                        myWeb = oWeb;
                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='Pay360']");
                        oDictOpt = xmlToHashTable(oPay360Cfg, "value");


                        var oEwProv = new PaymentProviders(ref myWeb);
                        // Get values form cart
                        oEwProv.mcCurrency = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(oCart.mcCurrencyCode), oCart.mcCurrency, oCart.mcCurrencyCode));
                        oEwProv.mcCurrencySymbol = oCart.mcCurrencySymbol;
                        if (string.IsNullOrEmpty(oOrder.GetAttribute("payableType")))
                        {
                            oEwProv.mnPaymentAmount = Conversions.ToDouble(oOrder.GetAttribute("total"));
                        }
                        else
                        {
                            oEwProv.mnPaymentAmount = Conversions.ToDouble(oOrder.GetAttribute("payableAmount"));
                            oEwProv.mnPaymentMaxAmount = Conversions.ToDouble(oOrder.GetAttribute("total"));
                            oEwProv.mcPaymentType = oOrder.GetAttribute("payableType");
                        }
                        oEwProv.mnCartId = oCart.mnCartId;
                        oEwProv.mcPaymentOrderDescription = "Ref:" + oCart.OrderNoPrefix + oCart.mnCartId.ToString() + " An online purchase from: " + oCart.mcSiteURL + " on " + niceDate(DateTime.Now) + " " + Conversions.ToString(DateAndTime.TimeValue(Conversions.ToString(DateTime.Now)));
                        oEwProv.mcCardHolderName = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText;

                        if (!string.IsNullOrEmpty(sProfile))
                        {
                            oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='Pay360' and @profile='" + sProfile + "']");
                        }
                        else
                        {
                            oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='Pay360']");
                        }

                        // Get the payment options
                        // oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='SecPay']")
                        oDictOpt = xmlToHashTable(oPay360Cfg, "value");

                        switch (Convert.ToString(oDictOpt["opperationMode"]) ?? "")
                        {
                            case "true":
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
                        if (oDictOpt.ContainsKey("currency"))
                            oDictOpt.Remove("currency");
                        oDictOpt.Add("currency", oEwProv.mcCurrency);

                        // Set common variables
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictOpt["validateCV2"], "on", false)))
                            bCv2 = true;
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictOpt["secure3d"], "on", false)))
                            b3DSecure = true;

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictOpt["allowSavePayment"], "on", false)))
                            bAllowSavePayment = true;

                        // Load the Xform

                        ccXform = oEwProv.creditCardXform(ref oOrder, "PayForm", sSubmitPath, Conversions.ToString(oDictOpt["cardsAccepted"]), bCv2, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("Make Payment of " + Strings.FormatNumber((object)oEwProv.mnPaymentAmount, 2) + " ", oDictOpt["currency"]), " by Credit/Debit Card")), b3DSecure, bSavePayment: bAllowSavePayment);

                        if (b3DSecure)
                        {
                            // check for return from aquiring bank
                            if (!string.IsNullOrEmpty(myWeb.moRequest["MD"]))
                            {
                                b3DAuthorised = true;
                            }
                        }

                        if (ccXform.valid | b3DAuthorised)
                        {


                            // Set up card options
                            sOpts = Conversions.ToString(Operators.ConcatenateObject("test_status=", oDictOpt["opperationMode"]));
                            sOpts = sOpts + ",dups=false,card_type=" + myWeb.moRequest["creditCard/type"];

                            // Account for scheduled payments from the payment config.
                            string scheduleInterval = Conversions.ToString(Operators.ConcatenateObject(oDictOpt["scheduleInterval"], "").ToLower);
                            string scheduleMaxRepeats = Conversions.ToString(Operators.ConcatenateObject(oDictOpt["scheduleMaxRepeats"], ""));
                            if (!string.IsNullOrEmpty(scheduleInterval) && Array.IndexOf(ScheduleIntervals, scheduleInterval) >= 0)
                            {


                                int maxRepeats;
                                if (!string.IsNullOrEmpty(scheduleMaxRepeats) && Information.IsNumeric(scheduleMaxRepeats) && Convert.ToInt16(scheduleMaxRepeats) > 0)

                                {
                                    maxRepeats = Conversions.ToInteger(scheduleMaxRepeats);
                                }
                                else
                                {
                                    maxRepeats = -1;
                                }

                                // We need to send through an immediate payment - ie, the actual payment
                                // and then schedule the same payment based on the interval
                                var scheduleDate = default(DateTime);
                                switch (scheduleInterval ?? "")
                                {

                                    case "yearly":
                                        {
                                            scheduleDate = DateTime.Today.AddYears(1);
                                            break;
                                        }
                                    case "half-yearly":
                                        {
                                            scheduleDate = DateTime.Today.AddMonths(6);
                                            break;
                                        }
                                    case "quarterly":
                                        {
                                            scheduleDate = DateTime.Today.AddMonths(3);
                                            break;
                                        }
                                    case "monthly":
                                        {
                                            scheduleDate = DateTime.Today.AddMonths(1);
                                            break;
                                        }
                                    case "weekly":
                                        {
                                            scheduleDate = DateTime.Today.AddDays(7d);
                                            break;
                                        }
                                    case "daily":
                                        {
                                            scheduleDate = DateTime.Today.AddDays(1d);
                                            break;
                                        }
                                }

                                sOpts += ",repeat=" + Strings.Format(scheduleDate, "yyyyMMdd");
                                sOpts += "/" + scheduleInterval;
                                sOpts += "/" + maxRepeats.ToString();
                                sOpts += ":" + oEwProv.mnPaymentAmount.ToString();

                            }


                            // Currency - if no currency then use GBP
                            if (!string.IsNullOrEmpty(oEwProv.mcCurrency))
                            {
                                sOpts = sOpts + ",currency=" + Strings.UCase(oEwProv.mcCurrency);
                            }
                            else
                            {
                                sOpts = sOpts + ",currency=GBP";
                            }

                            // Optional - CV2
                            if (bCv2)
                            {
                                sOpts = sOpts + ",cv2=" + myWeb.moRequest["creditCard/CV2"];
                            }
                            // Optional - 3DSecure
                            if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oDictOpt["opperationMode"], "true", false), b3DSecure)))
                            {
                                sOpts = sOpts + ",test_mpi_status=true";
                            }

                            // If test mode, then we must turn on cv2-avs checks - mandatory Visa mandate May 2009
                            if (Conversions.ToBoolean(Operators.OrObject(Operators.ConditionalCompareObjectEqual(LCase(oDictOpt["opperationMode"]), "true", false), Operators.ConditionalCompareObjectEqual(LCase(oDictOpt["opperationMode"]), "false", false))))
                            {
                                sOpts = sOpts + ",default_cv2avs=ALL MATCH";
                            }

                            if (Conversions.ToBoolean(Operators.OrObject(Operators.ConditionalCompareObjectEqual(LCase(oDictOpt["transactionType"]), "defer", false), Operators.ConditionalCompareObjectEqual(LCase(oDictOpt["transactionType"]), "deferred", false))))
                            {
                                if (Information.IsNumeric(oDictOpt["ccDeferDays"]) & Information.IsNumeric(oDictOpt["dcDeferDays"]))
                                {
                                    sOpts = Conversions.ToString(sOpts + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(",deferred=reuse:", oDictOpt["ccDeferDays"]), ":"), oDictOpt["dcDeferDays"]));
                                }
                                else
                                {
                                    sOpts += ",deferred=true";
                                }
                            }
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictOpt["digest"], "on", false)))
                            {
                                string cDigest = "";
                                if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(oDictOpt["accountId"], "secpay", false)))
                                {
                                    cDigest = oEwProv.mnCartId.ToString() + oEwProv.mnPaymentAmount.ToString() + Conversions.ToString(oDictOpt["accountPassword"]);
                                }
                                else
                                {
                                    cDigest = "secpay";
                                }
                                var encode = new System.Text.UnicodeEncoding();
                                byte[] inputDigest = encode.GetBytes(cDigest);

                                byte[] hash;
                                // get hash
                                var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                                hash = md5.ComputeHash(inputDigest);

                                // convert hash value to hex string
                                var sb = new System.Text.StringBuilder();
                                foreach (var outputByte in hash)
                                    // convert each byte to a Hexadecimal upper case string
                                    sb.Append(outputByte.ToString("x2"));

                                sOpts += ",digest=" + sb.ToString();

                            }

                            var oSecVpn = new Protean.Paypoint.SECVPNClient();

                            cOrderType = oOrder.GetAttribute("orderType");
                            if (!(!string.IsNullOrEmpty(cOrderType) & Conversions.ToString(oDictOpt["UseOrderType"]) == "true"))
                                cOrderType = Conversions.ToString(oDictOpt["accountId"]);
                            if (!b3DSecure)
                            {
                                string argsType = "Delivery Address";
                                string argsType1 = "Billing Address";
                                cResponse = oSecVpn.validateCardFull(cOrderType, Conversions.ToString(oDictOpt["accountPassword"]), oEwProv.mnCartId.ToString(), myWeb.moRequest.ServerVariables["REMOTE_ADDR"], oEwProv.mcCardHolderName, myWeb.moRequest["creditCard/number"], oEwProv.mnPaymentAmount.ToString(), this.fmtSecPayDate(myWeb.moRequest["creditCard/expireDate"]), myWeb.moRequest["creditCard/issueNumber"], this.fmtSecPayDate(myWeb.moRequest["creditCard/issueDate"]), getPay360Order(ref oOrder), getPay360Address(ref oOrder, ref argsType), getPay360Address(ref oOrder, ref argsType1), sOpts);

                                bSavePayment = true;
                            }

                            else if (!b3DAuthorised)
                            {
                                string argsType2 = "Delivery Address";
                                string argsType3 = "Billing Address";
                                cResponse = oSecVpn.threeDSecureEnrolmentRequest(cOrderType, Conversions.ToString(oDictOpt["accountPassword"]), oEwProv.mnCartId.ToString(), myWeb.moRequest.ServerVariables["REMOTE_ADDR"], oEwProv.mcCardHolderName, myWeb.moRequest["creditCard/number"], oEwProv.mnPaymentAmount.ToString(), this.fmtSecPayDate(myWeb.moRequest["creditCard/expireDate"]), myWeb.moRequest["creditCard/issueNumber"], this.fmtSecPayDate(myWeb.moRequest["creditCard/issueDate"]), getPay360Order(ref oOrder), getPay360Address(ref oOrder, ref argsType2), getPay360Address(ref oOrder, ref argsType3), sOpts, "0", myWeb.moRequest.ServerVariables["HTTP_ACCEPT"], myWeb.moRequest.ServerVariables["HTTP_USER_AGENT"], "", "", "", "", "", "");

                                bSavePayment = true;
                            }

                            else
                            {
                                // Pass the process back to Secpay
                                cResponse = oSecVpn.threeDSecureAuthorisationRequest(cOrderType, Conversions.ToString(oDictOpt["accountPassword"]), oEwProv.mnCartId.ToString(), myWeb.moRequest["MD"], myWeb.moRequest["PaRes"], sOpts);
                                // Save in the session the MD and the instance to save

                            }

                            // Parse the response
                            oDictResp = new Hashtable();

                            // cResponse = Replace(oXMLHttp.responseXML.selectSingleNode("//node()[local-name()='validateCardFullReturn']").Text, "+", " ")
                            string cAuthCode = "";
                            if (!string.IsNullOrEmpty(cResponse))
                            {

                                aResponse = Strings.Split(Strings.Right(cResponse, Strings.Len(cResponse) - 1), "&");

                                var loopTo = Information.UBound(aResponse);
                                for (i = 0; i <= loopTo; i++)
                                {
                                    string cPos = Strings.InStr(aResponse[i], "=").ToString();
                                    if (Information.IsNumeric(cPos))
                                    {
                                        nPos = Conversions.ToInteger(cPos);
                                        oDictResp.Add(Strings.Left(aResponse[i], nPos - 1), Strings.Right(aResponse[i], Strings.Len(aResponse[i]) - nPos));
                                    }
                                    else
                                    {
                                        oDictResp.Add(Strings.Trim(aResponse[i]), "");
                                    }
                                }


                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictResp["valid"], "true", false)))
                                {
                                    if (!b3DSecure)
                                    {
                                        bIsValid = true;
                                        err_msg = "Payment Authorised Ref: " + oEwProv.mnCartId.ToString();
                                        ccXform.valid = true;
                                    }

                                    else
                                    {
                                        switch (Convert.ToString(oDictResp["mpi_status_code"]) ?? "")
                                        {
                                            case "200":
                                                {
                                                    // we have to get the browser to redirect
                                                    // v4 change - don't explicitly redirect to /deafault.ashx - this breaks everything.
                                                    // AJG Remove defualt.ashx from RedirectUrl Compare this line to 4.1
                                                    if (oEwProv.moCartConfig["SecureURL"].EndsWith("/"))
                                                    {
                                                        sRedirectURL = oEwProv.moCartConfig["SecureURL"] + "?cartCmd=Redirect3ds";
                                                    }
                                                    else
                                                    {
                                                        sRedirectURL = oEwProv.moCartConfig["SecureURL"] + "/?cartCmd=Redirect3ds";
                                                    }

                                                    string cleanACSURL = myWeb.goServer.UrlDecode(Conversions.ToString(oDictResp["acs_url"]));
                                                    cleanACSURL = Strings.Replace(cleanACSURL, "&amp;", "&");

                                                    bIsValid = false;
                                                    ccXform.valid = false;
                                                    err_msg = "customer redirected to:" + cleanACSURL;

                                                    // Save MD as paymentRef
                                                    sPaymentRef = Conversions.ToString(oDictResp["MD"]);
                                                    // Save the payment instance in the session

                                                    Xform3dSec = oEwProv.xfrmSecure3D(Conversions.ToString(oDictResp["acs_url"]), Conversions.ToString(oDictResp["MD"]), Conversions.ToString(oDictResp["PaReq"]), sRedirectURL);
                                                    break;
                                                }

                                            case "212":
                                                {
                                                    // not subscribes to 3D Secure
                                                    bIsValid = true;
                                                    err_msg = "Payment Authorised Ref: " + oEwProv.mnCartId.ToString();
                                                    err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                                    ccXform.valid = true;
                                                    break;
                                                }

                                            case "237":
                                                {
                                                    // Payer Authenticated
                                                    bIsValid = true;
                                                    err_msg = "Payment Authorised Ref: " + oEwProv.mnCartId.ToString();
                                                    err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                                    ccXform.valid = true;
                                                    break;
                                                }

                                            case "236":
                                                {
                                                    // Payer Declined 3D Secure but Proceeded to confirm 
                                                    // the(authentication)
                                                    bIsValid = true;
                                                    err_msg = "Payment Authorised Ref: " + oEwProv.mnCartId.ToString();
                                                    err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                                    ccXform.valid = true;
                                                    break;
                                                }


                                            case "234":
                                                {
                                                    // unable to verify erolement but secpay passes
                                                    bIsValid = true;
                                                    err_msg = "Payment Authorised Ref: " + oEwProv.mnCartId.ToString();
                                                    err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                                    ccXform.valid = true;
                                                    break;
                                                }


                                            case "229":
                                                {
                                                    // Payer Not Authenticated
                                                    bIsValid = false;
                                                    ccXform.valid = false;
                                                    err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                                    break;
                                                }

                                            default:
                                                {
                                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictResp["code"], "A", false)))
                                                    {
                                                        bIsValid = true;
                                                        err_msg = "Payment Authorised Ref: " + oEwProv.mnCartId.ToString();
                                                        err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                                        ccXform.valid = true;
                                                    }
                                                    else
                                                    {
                                                        // Payer Not Authenticated
                                                        bIsValid = false;
                                                        ccXform.valid = false;
                                                        err_msg = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_status_code"]), " - "), oDictResp["mpi_message"]));
                                                    }

                                                    break;
                                                }
                                        }
                                    }
                                }
                                else
                                {
                                    ccXform.valid = false;
                                    bIsValid = false;
                                    err_msg_log = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Payment Failed : ", oDictResp["message"]), " (Code::"), oDictResp["code"]), ")"));

                                    // Produce nice format error messages.
                                    switch (Convert.ToString(oDictResp["code"]) ?? "")
                                    {
                                        case "N":
                                            {
                                                err_msg = "The transaction was not authorised by your payment provider.";
                                                break;
                                            }
                                        case "C":
                                            {
                                                err_msg = "There was a comunication problem. Please try resubmitting your order later.";
                                                break;
                                            }
                                        case "P:A":
                                            {
                                                err_msg = "There was a system error - the amount was not supplied or invalid.  Please call for assistance.";
                                                break;
                                            }
                                        case "P:X":
                                            {
                                                err_msg = "There was a system error - not all the mandatory parameters were supplied.  Please call for assistance.";
                                                break;
                                            }
                                        case "P:P":
                                            {
                                                err_msg = "The payment has already been processed.  This is a duplicate payment, and will not be processed.";
                                                break;
                                            }
                                        case "P:S":
                                            {
                                                err_msg = "The start date is invalid.  Please check that you have entered your card details correctly.";
                                                break;
                                            }
                                        case "P:E":
                                            {
                                                err_msg = "The expiry date is invalid.  Please check that you have entered your card details correctly.";
                                                break;
                                            }
                                        case "P:I":
                                            {
                                                err_msg = "The issue number is invalid.  Please check that you have entered your card details correctly.";
                                                break;
                                            }
                                        case "P:C":
                                            {
                                                err_msg = "The card number supplied is invalid.  Please check that you have entered your card details correctly.";
                                                break;
                                            }
                                        case "P:T":
                                            {
                                                err_msg = "The card type does not match the card number entered.  Please check that you have entered your card details correctly.";
                                                break;
                                            }
                                        case "P:N":
                                            {
                                                err_msg = "There was a system error - the customer name was not supplied.  Please call for assistance.";
                                                break;
                                            }
                                        case "P:M":
                                            {
                                                err_msg = "There was a system error - the merchant account deos not exist or has not been registered.  Please call for assistance.";
                                                break;
                                            }
                                        case "P:B":
                                            {
                                                err_msg = "There was a system error - the merchant account for this card type does not exist.  Please call for assistance.";
                                                break;
                                            }
                                        case "P:D":
                                            {
                                                err_msg = "There was a system error - the merchant account for this currency does not exist.  Please call for assistance.";
                                                break;
                                            }
                                        case "P:V":
                                            {
                                                err_msg = "The security code is invalid. Please check that you have entered your card details correctly. The security code can be found on the back of your card and is the last 3 digits of the series of digits on the back.";
                                                break;
                                            }
                                        case "P:R":
                                            {
                                                err_msg = "There was a communication problem and the transaction has timed out.  Please try resubmitting your order later.";
                                                break;
                                            }
                                        case "P:#":
                                            {
                                                err_msg = "There was a system error - no encryption key has been set up against this account.  Please call for assistance.";
                                                break;
                                            }

                                        default:
                                            {
                                                err_msg = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("There was an unspecified error. Please call for assistance.(code::", oDictResp["code"]), " | "), oDictResp["message"]));
                                                break;
                                            }
                                    }

                                    err_msg = "Payment Failed : " + err_msg;

                                }
                            }
                            else
                            {
                                bIsValid = false;
                                ccXform.valid = false;
                                err_msg = "Payment Failed : no response from Pay360 check settings and password";
                            }


                            if (bSavePayment)
                            {
                                // We Save the payment method prior to ultimate validation because we only have access to the request object at the point it is submitted

                                // only do this for a valid payment method
                                XmlElement oSaveElmt = (XmlElement)ccXform.Instance.SelectSingleNode("creditCard/bSavePayment");
                                Debug.WriteLine(myWeb.moRequest["creditCard/expireDate"]);
                                var oDate = new DateTime(Conversions.ToInteger("20" + Strings.Right(myWeb.moRequest["creditCard/expireDate"], 2)), Conversions.ToInteger(Strings.Left(myWeb.moRequest["creditCard/expireDate"], 2)), 1);
                                oDate = oDate.AddMonths(1);
                                oDate = oDate.AddDays(-1);
                                string cMethodName = myWeb.moRequest["creditCard/type"] + ": " + stdTools.MaskString(myWeb.moRequest["creditCard/number"], "*", false, 4) + " Expires: " + oDate.ToShortDateString();
                                cAuthCode = Conversions.ToString(oDictResp["auth_code"]);

                                // Dim oPay360Elmt As XmlElement = ccXform.Instance.OwnerDocument.CreateElement("Pay360")
                                // oPay360Elmt.SetAttribute("AuthCode", cAuthCode)
                                // ccXform.Instance.FirstChild.AppendChild(oPay360Elmt)

                                if (oSaveElmt is not null)
                                {
                                    if (oSaveElmt.InnerText == "true" & bIsValid)
                                    {
                                        oCart.mnPaymentId = oEwProv.savePayment((long)myWeb.mnUserId, "Pay360", oEwProv.mnCartId.ToString(), cMethodName, (XmlElement)ccXform.Instance.FirstChild, oDate, true, oEwProv.mnPaymentAmount);
                                    }
                                    else
                                    {
                                        oCart.mnPaymentId = oEwProv.savePayment((long)myWeb.mnUserId, "Pay360", oEwProv.mnCartId.ToString(), cMethodName, (XmlElement)ccXform.Instance.FirstChild, DateTime.Now, false, oEwProv.mnPaymentAmount);
                                    }
                                }
                                else
                                {
                                    oCart.mnPaymentId = oEwProv.savePayment((long)myWeb.mnUserId, "Pay360", oEwProv.mnCartId.ToString(), cMethodName, (XmlElement)ccXform.Instance.FirstChild, DateTime.Now, false, oEwProv.mnPaymentAmount);
                                }

                            }

                            //XmlNode argoNode = (XmlNode)ccXform.moXformElmt;
                            ccXform.addNote(ref ccXform.moXformElmt, Protean.xForm.noteTypes.Alert, err_msg);
                        }



                        else
                        {
                            if (ccXform.isSubmitted() & string.IsNullOrEmpty(ccXform.validationError))
                            {
                                err_msg = "Unknown Error: Please call";
                                //XmlNode argoNode1 = (XmlNode)ccXform.moXformElmt;
                                ccXform.addNote(ref ccXform.moXformElmt, Protean.xForm.noteTypes.Alert, err_msg);
                            }
                            else
                            {
                                err_msg = ccXform.validationError;
                            }
                            ccXform.valid = false;
                        }

                        if (ccXform.isSubmitted() | b3DAuthorised)
                        {
                            // Update Seller Notes:
                            sSql = "select * from tblCartOrder where nCartOrderKey = " + oEwProv.mnCartId;
                            DataSet oDs;
                            oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                            foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                            {
                                if (bIsValid | b3DAuthorised)
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
                        }

                        if (Xform3dSec is not null)
                        {
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


                private string getPay360Address(ref XmlElement oRoot, ref string sType)
                {
                    myWeb.PerfMon.Log("PaymentProviders", "getSecPayAddress");
                    XmlElement oCartAdd;

                    string sAddress;
                    string sPrefix;
                    string cProcessInfo = "getSecPayAddress";
                    try
                    {
                        sAddress = "";

                        oCartAdd = (XmlElement)oRoot.SelectSingleNode("Contact[@type='" + sType + "']");

                        if (sType == "Delivery")
                            sType = "Shipping";

                        if (oCartAdd is not null)
                        {

                            sPrefix = Strings.LCase(Strings.Left(sType, 4)) + "_";

                            // given name
                            if (oCartAdd.SelectSingleNode("GivenName") is not null)
                            {
                                sAddress = sAddress + sPrefix + "name=" + Strings.Replace(oCartAdd.SelectSingleNode("GivenName").InnerText, ",", "&comma;") + ",";
                            }

                            if (oCartAdd.SelectSingleNode("Company") is not null)
                            {
                                sAddress = sAddress + sPrefix + "company=" + Strings.Replace(oCartAdd.SelectSingleNode("Company").InnerText, ",", "&comma;") + ",";
                            }

                            string[] cStreet = Strings.Split(oCartAdd.SelectSingleNode("Street").InnerText, ",");
                            switch (Information.UBound(cStreet))
                            {
                                case 0:
                                    {
                                        if (oCartAdd.SelectSingleNode("Street") is not null)
                                        {
                                            sAddress = sAddress + sPrefix + "addr_1=" + cStreet[0] + ",";
                                        }

                                        break;
                                    }
                                case 1:
                                    {
                                        if (oCartAdd.SelectSingleNode("Street") is not null)
                                        {
                                            sAddress = sAddress + sPrefix + "addr_1=" + cStreet[0] + ",";
                                            sAddress = sAddress + sPrefix + "addr_2=" + cStreet[1] + ",";
                                        }

                                        break;
                                    }

                                default:
                                    {
                                        if (oCartAdd.SelectSingleNode("Street") is not null)
                                        {
                                            sAddress = sAddress + sPrefix + "addr_1=" + cStreet[0] + ",";
                                            // remove commas AVC doesn't like em.
                                            sAddress = sAddress + sPrefix + "addr_2=" + Strings.Replace(Strings.Replace(oCartAdd.SelectSingleNode("Street").InnerText, cStreet[0], ""), ",", " ") + ",";
                                        }

                                        break;
                                    }
                            }

                            if (oCartAdd.SelectSingleNode("City") is not null)
                            {
                                sAddress = sAddress + sPrefix + "city=" + Strings.Replace(oCartAdd.SelectSingleNode("City").InnerText, ",", "&comma;") + ",";
                            }

                            if (oCartAdd.SelectSingleNode("State") is not null)
                            {
                                sAddress = sAddress + sPrefix + "state=" + Strings.Replace(oCartAdd.SelectSingleNode("State").InnerText, ",", "&comma;") + ",";
                            }

                            if (oCartAdd.SelectSingleNode("Country") is not null)
                            {
                                sAddress = sAddress + sPrefix + "country=" + Strings.Replace(oCartAdd.SelectSingleNode("Country").InnerText, ",", "&comma;") + ",";
                            }

                            if (oCartAdd.SelectSingleNode("PostalCode") is not null)
                            {
                                sAddress = sAddress + sPrefix + "post_code=" + Strings.Replace(oCartAdd.SelectSingleNode("PostalCode").InnerText, ",", "&comma;") + ",";
                            }

                            if (oCartAdd.SelectSingleNode("Telephone") is not null)
                            {
                                sAddress = sAddress + sPrefix + "tel=" + Strings.Replace(oCartAdd.SelectSingleNode("Telephone").InnerText, ",", "&comma;") + ",";
                            }

                            if (oCartAdd.SelectSingleNode("Email") is not null)
                            {
                                sAddress = sAddress + sPrefix + "email=" + Strings.Replace(oCartAdd.SelectSingleNode("Email").InnerText, ",", "&comma;") + ",";
                            }

                            if (!string.IsNullOrEmpty(sAddress))
                                sAddress = Strings.Left(sAddress, Strings.Len(sAddress) - 1);

                        }

                        return sAddress;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "getSecPayAddress", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }

                }

                private string getPay360Order(ref XmlElement oRoot)
                {

                    myWeb.PerfMon.Log("PaymentProviders", "getSecPayOrder");
                    string sOrder;
                    string cProcessInfo = "getSecPayOrder";
                    try
                    {
                        sOrder = "";
                        foreach (XmlElement oItem in oRoot.SelectNodes("Item"))
                        {
                            sOrder = sOrder + "prod=" + Strings.Replace(oItem.SelectSingleNode("Name").InnerText, " ", "_");
                            sOrder = sOrder + ", item_amount=" + oItem.SelectSingleNode("@price").InnerText + "x" + oItem.SelectSingleNode("@quantity").InnerText + ";";
                        }
                        // add the shipping
                        if (Conversions.ToInteger("0" + oRoot.SelectSingleNode("@shippingCost").InnerText) > 0)
                        {
                            sOrder = sOrder + "prod=SHIPPING,item_amount=" + oRoot.SelectSingleNode("@shippingCost").InnerText + "x1;";
                        }
                        // add the tax
                        if (Conversions.ToInteger("0" + oRoot.SelectSingleNode("@vatAmt").InnerText) > 0)
                        {
                            sOrder = sOrder + "prod=TAX,item_amount=" + oRoot.SelectSingleNode("@vatAmt").InnerText + "x1;";
                        }

                        // strip the trailing semiColon
                        sOrder = Strings.Left(sOrder, Strings.Len(sOrder) - 1);
                        return sOrder;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "getSecPayOrder", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                private string fmtSecPayDate(string sdate)
                {
                    string cProcessInfo = "fmtSecPayDate";
                    string strReturn = "";
                    try
                    {
                        // The dates are formatted "mm yyyy" - convert them to "mmyy"
                        if (!string.IsNullOrEmpty(sdate))
                            strReturn = Strings.Left(sdate, 2) + Strings.Right(sdate, 2);
                        return strReturn;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "fmtSecPayDate", ex, "", cProcessInfo, gbDebug);
                        return "";

                    }
                }

                public long FormatCreditCardNumber(string sCCNumber)
                {
                    myWeb.PerfMon.Log("PaymentProviders", "FormatCreditCardNumber");
                    string cResult = "";
                    var oRE = new Regex(@"\D");
                    cResult = oRE.Replace(sCCNumber, "");
                    cResult = Strings.Replace(cResult, " ", ""); // also strip out spaces
                    if (string.IsNullOrEmpty(cResult))
                        cResult = 0.ToString();
                    return Conversions.ToLong(cResult);
                }

                public string FullMoneyString(string amount)
                {
                    try
                    {
                        if (!amount.Contains("."))
                            return Strings.Replace(amount, ",", "");
                        amount = Strings.Replace(amount, ",", "");
                        string[] cAmounts = Strings.Split(amount, ".");
                        if (Information.UBound(cAmounts) > 2)
                            return amount;
                        amount = cAmounts[0] + ".";
                        if (cAmounts[1].Length < 2)
                        {
                            amount += cAmounts[1] + "0";
                        }
                        else
                        {
                            amount += cAmounts[1];
                        }
                        return amount;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "FullMoneyString", ex, "", "", gbDebug);
                        return amount;
                    }

                }



                public new string CheckStatus(ref Cms oWeb, ref string nPaymentProviderRef)   // Overload method from base class
                {
                    string cProcessInfo = "";
                    // Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                    // Dim oSagePayV3Cfg As XmlNode
                    // Dim nTransactionMode As TransactionMode

                    try
                    {
                        // moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                        // oPay360Cfg = moPaymentCfg.SelectSingleNode("provider[@name='Pay360']")
                        // oDictOpt = xmlTools.xmlToHashTable(oPay360Cfg, "value")

                        // Dim oSecVpn As Paypoint.SECVPNClient = New Paypoint.SECVPNClient
                        // Dim cResponse As String

                        // cResponse = oSecVpn.repeatCardFull(cOrderType,
                        // CStr(oDictOpt("accountPassword")),
                        // CStr(oEwProv.mnCartId),
                        // myWeb.moRequest.ServerVariables("REMOTE_ADDR"),
                        // oEwProv.mcCardHolderName,
                        // myWeb.moRequest("creditCard/number"),
                        // CStr(oEwProv.mnPaymentAmount),
                        // fmtSecPayDate(myWeb.moRequest("creditCard/expireDate")),
                        // myWeb.moRequest("creditCard/issueNumber"),
                        // fmtSecPayDate(myWeb.moRequest("creditCard/issueDate")),
                        // getPay360Order(oOrder),
                        // getPay360Address(oOrder, "Delivery Address"),
                        // getPay360Address(oOrder, "Billing Address"),
                        // sOpts)

                        return "Active";
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }

                public string CollectPayment(ref Cms oWeb, long nPaymentMethodId, double Amount, string CurrencyCode, string PaymentDescription, ref Cms.Cart oOrder)
                {
                    string cProcessInfo = "";
                    XmlElement oPay360Cfg;
                    var oDictOpt = new Hashtable();
                    string sOpts;
                    try
                    {


                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        oPay360Cfg = (XmlElement)moPaymentCfg.SelectSingleNode("provider[@name='Pay360']");
                        oDictOpt = xmlToHashTable(oPay360Cfg, "value");
                        string RemotePassword = "";
                        string sToken = Conversions.ToString(myWeb.moDbHelper.GetDataValue("select cPayMthdProviderRef from tblCartPaymentMethod where nPayMthdKey = " + nPaymentMethodId));
                        var cardXml = new XmlDocument();
                        cardXml.LoadXml(Conversions.ToString(myWeb.moDbHelper.GetDataValue("select cPayMthdProviderRef from tblCartPaymentMethod where nPayMthdKey = " + nPaymentMethodId)));

                        string cardExpireDate = cardXml.SelectSingleNode("creditCard/expireDate").InnerText;
                        string cardType = cardXml.SelectSingleNode("creditCard/type").InnerText;
                        string CV2 = cardXml.SelectSingleNode("creditCard/CV2").InnerText;

                        sOpts = Conversions.ToString(Operators.ConcatenateObject("test_status=", oDictOpt["opperationMode"]));
                        sOpts = sOpts + ",dups=false,card_type=" + cardType;

                        // Currency - if no currency then use GBP
                        if (!string.IsNullOrEmpty(oOrder.mcCurrency))
                        {
                            sOpts = sOpts + ",currency=" + Strings.UCase(oOrder.mcCurrency);
                        }
                        else
                        {
                            sOpts = sOpts + ",currency=GBP";
                        }

                        if (!string.IsNullOrEmpty(CV2))
                        {
                            sOpts = sOpts + ",cv2=" + CV2;
                        }

                        var oSecVpn = new Protean.Paypoint.SECVPNClient();
                        string cResponse;
                        cResponse = oSecVpn.repeatCardFull(Conversions.ToString(oDictOpt["accountId"]), Conversions.ToString(oDictOpt["accountPassword"]), sToken, Amount.ToString(), Conversions.ToString(oDictOpt["remotePassword"]), fmtSecPayDate(cardExpireDate), oOrder.mnCartId.ToString(), sOpts);

                        string[] aResponse = Strings.Split(Strings.Right(cResponse, Strings.Len(cResponse) - 1), "&");
                        short i;
                        short nPos;
                        var oDictResp = new Hashtable();
                        var loopTo = (short)Information.UBound(aResponse);
                        for (i = 0; i <= loopTo; i++)
                        {
                            string cPos = Strings.InStr(aResponse[i], "=").ToString();
                            if (Information.IsNumeric(cPos))
                            {
                                nPos = (short)Conversions.ToInteger(cPos);
                                oDictResp.Add(Strings.Left(aResponse[i], nPos - 1), Strings.Right(aResponse[i], Strings.Len(aResponse[i]) - nPos));
                            }
                            else
                            {
                                oDictResp.Add(Strings.Trim(aResponse[i]), "");
                            }
                        }

                        bool bIsValid = false;
                        string err_msg = "";

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictResp["valid"], "true", false)))
                        {
                            switch (Convert.ToString(oDictResp["mpi_status_code"]) ?? "")
                            {

                                case "212":
                                    {
                                        // not subscribes to 3D Secure
                                        bIsValid = true;
                                        err_msg = "Payment Authorised Ref: " + oOrder.mnCartId.ToString();
                                        err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                        break;
                                    }

                                case "237":
                                    {
                                        // Payer Authenticated
                                        bIsValid = true;
                                        err_msg = "Payment Authorised Ref: " + oOrder.mnCartId.ToString();
                                        err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                        break;
                                    }

                                case "236":
                                    {
                                        // Payer Declined 3D Secure but Proceeded to confirm 
                                        // the(authentication)
                                        bIsValid = true;
                                        err_msg = "Payment Authorised Ref: " + oOrder.mnCartId.ToString();
                                        err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                        break;
                                    }



                                case "234":
                                    {
                                        // unable to verify erolement but secpay passes
                                        bIsValid = true;
                                        err_msg = "Payment Authorised Ref: " + oOrder.mnCartId.ToString();
                                        err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                        break;
                                    }


                                case "229":
                                    {
                                        // Payer Not Authenticated
                                        bIsValid = false;
                                        err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                        break;
                                    }

                                default:
                                    {
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictResp["code"], "A", false)))
                                        {
                                            bIsValid = true;
                                            err_msg = "Payment Authorised Ref: " + oOrder.mnCartId.ToString();
                                            err_msg = Conversions.ToString(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_message"]));
                                        }

                                        else
                                        {
                                            // Payer Not Authenticated
                                            bIsValid = false;
                                            err_msg = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(err_msg + " 3D Secure:", oDictResp["mpi_status_code"]), " - "), oDictResp["mpi_message"]));
                                        }

                                        break;
                                    }
                            }
                        }

                        if (bIsValid)
                        {
                            return "Success";
                        }
                        else
                        {
                            return err_msg;
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CollectPayment", ex, "", cProcessInfo, gbDebug);
                        return "Payment Error";
                    }
                }

                public string CancelPayments(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";
                    string moPaymentCfg = Conversions.ToString(WebConfigurationManager.GetWebApplicationSection("protean/payment"));
                    // Dim oPay360Cfg As XmlNode

                    try
                    {

                        return "";
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }

            }
        }
    }
}