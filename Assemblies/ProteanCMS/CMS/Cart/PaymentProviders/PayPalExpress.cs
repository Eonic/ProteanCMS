using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using PayPal.Api;
using static Protean.Cms.Cart;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean.Providers
{
    namespace Payment
    {
        public class PayPalExpressRest
        {

            public PayPalExpressRest()
            {
                // do nothing
            }

            public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, ref object BaseProvider, ref Cms myWeb)
            {

                BaseProvider.AdminXforms = new AdminXForms(ref myWeb);
                BaseProvider.AdminProcess = new AdminProcess(ref myWeb);
                BaseProvider.AdminProcess.oAdXfm = BaseProvider.AdminXforms;
                BaseProvider.Activities = new Activities();

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

                public new Protean.xForm GetPaymentForm(ref Cms myWeb, ref Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails")
                {
                    myWeb.PerfMon.Log("PaymentProviders", "payPayPalExpress");
                    string sSql;

                    var ppXform = new Protean.xForm(ref myWeb.msException);

                    Protean.xForm Xform3dSec = (Protean.xForm)null;

                    var bIsValid = default(bool);
                    string err_msg = "";
                    string err_msg_log = "";
                    string sProcessInfo = "";
                    string cResponse = "";

                    var oDictOpt = new Hashtable();

                    bool bCv2 = false;
                    bool b3DSecure = false;
                    bool b3DAuthorised = false;
                    string sRedirectURL = "";
                    string sPaymentRef = "";

                    string cProcessInfo = "payPayPalExpress";

                    // Get the payment options into a hashtable
                    XmlNode oPayPalCfg;
                    bool bSavePayment = false;
                    bool bAllowSavePayment = false;
                    string host = "www.paypal.com";
                    string mcCurrency = string.Empty;

                    try
                    {

                        Global.Protean.Cms.Cart.PaymentProviders oEwProv = new PaymentProviders(myWeb);

                        // Get values form cart
                        oEwProv.mcCurrency = Interaction.IIf(string.IsNullOrEmpty(oCart.mcCurrencyCode), oCart.mcCurrency, oCart.mcCurrencyCode);
                        oEwProv.mcCurrencySymbol = oCart.mcCurrencySymbol;
                        if (string.IsNullOrEmpty(oOrder.GetAttribute("payableType")))
                        {
                            oEwProv.mnPaymentAmount = oOrder.GetAttribute("total");
                        }
                        else
                        {
                            oEwProv.mnPaymentAmount = oOrder.GetAttribute("payableAmount");
                            oEwProv.mnPaymentMaxAmount = oOrder.GetAttribute("total");
                            oEwProv.mcPaymentType = oOrder.GetAttribute("payableType");
                        }
                        oEwProv.mnCartId = oCart.mnCartId;
                        oEwProv.mcPaymentOrderDescription = "Ref:" + oCart.OrderNoPrefix + oCart.mnCartId.ToString() + " An online purchase from: " + oCart.mcSiteURL + " on " + niceDate(DateTime.Now) + " " + Conversions.ToString(DateAndTime.TimeValue(Conversions.ToString(DateTime.Now)));
                        oEwProv.mcCardHolderName = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText;

                        // Repeat Payment Settings
                        double repeatAmt = Conversions.ToDouble("0" + oOrder.GetAttribute("repeatPrice"));
                        string repeatInterval = Strings.LCase(oOrder.GetAttribute("repeatInterval"));
                        int repeatFrequency = (int)Math.Round(Conversions.ToDouble("0" + oOrder.GetAttribute("repeatFrequency")));
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

                        // If sProfile <> "" Then
                        // oPayPalCfg= moPaymentCfg.SelectSingleNode("provider[@name='PayPalExpress' and @profile='" & sProfile & "']")
                        // Else
                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        oPayPalCfg = moPaymentCfg.SelectSingleNode("provider[@name='PayPalExpressRest']");
                        // End If

                        // Get the payment options
                        // oPayPalCfg= moPaymentCfg.SelectSingleNode("provider[@name='SecPay']")
                        oDictOpt = xmlToHashTable(oPayPalCfg, "value");

                        switch (oDictOpt["opperationMode"].ToString() ?? "")
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
                        {
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDictOpt["currency"], "", false)))
                            {
                                mcCurrency = Conversions.ToString(oDictOpt["currency"]);
                            }
                        }

                        // Create the API Context
                        string AccessToken = new OAuthTokenCredential(Conversions.ToString(oDictOpt["ClientId"]), Conversions.ToString(oDictOpt["ClientSecret"])).GetAccessToken();
                        var ppApiCtx = new APIContext(AccessToken);

                        // Load the Xform

                        switch (myWeb.moRequest["ppCmd"] ?? "")
                        {

                            // 'Case for Successful payment return
                            // Dim oGetExpChk As Protean.PayPalAPI.GetExpressCheckoutDetailsRequestType = New Protean.PayPalAPI.GetExpressCheckoutDetailsRequestType()
                            // oGetExpChk.Token = myWeb.moRequest("token")
                            // oGetExpChk.Version = "63.0"

                            // Dim oGetExpChkReq As New Protean.PayPalAPI.GetExpressCheckoutDetailsReq()
                            // oGetExpChkReq.GetExpressCheckoutDetailsRequest = oGetExpChk

                            // 'Get the Transaction Details back
                            // Dim oExpChkResponse As Protean.PayPalAPI.GetExpressCheckoutDetailsResponseType = New Protean.PayPalAPI.GetExpressCheckoutDetailsResponseType
                            // oExpChkResponse = ppIface.GetExpressCheckoutDetails(ppProfile, oGetExpChkReq)

                            // 'Confirm the Sale
                            // Dim oDoExpChkReqType As Protean.PayPalAPI.DoExpressCheckoutPaymentRequestType = New Protean.PayPalAPI.DoExpressCheckoutPaymentRequestType
                            // oDoExpChkReqType.Version = "63.0"

                            // Dim oDoExpChkReq As Protean.PayPalAPI.DoExpressCheckoutPaymentReq = New Protean.PayPalAPI.DoExpressCheckoutPaymentReq
                            // oDoExpChkReq.DoExpressCheckoutPaymentRequest = oDoExpChkReqType

                            // Dim oDoExpChkDetails As Protean.PayPalAPI.DoExpressCheckoutPaymentRequestDetailsType = New Protean.PayPalAPI.DoExpressCheckoutPaymentRequestDetailsType
                            // oDoExpChkReqType.DoExpressCheckoutPaymentRequestDetails = oDoExpChkDetails
                            // oDoExpChkDetails.Token = myWeb.moRequest("token")

                            // oDoExpChkDetails.PayerID = oExpChkResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID
                            // oDoExpChkDetails.PaymentAction = getPPPaymentActionFromCode(oDictOpt("PaymentAction"))
                            // oDoExpChkDetails.PaymentDetails = oExpChkResponse.GetExpressCheckoutDetailsResponseDetails.PaymentDetails

                            // Dim oDoExpChkResponse As Protean.PayPalAPI.DoExpressCheckoutPaymentResponseType = New Protean.PayPalAPI.DoExpressCheckoutPaymentResponseType

                            // 'TS added following up on this http://stackoverflow.com/questions/15817839/you-do-not-have-permissions-to-make-this-api-call-using-soap-for-dodirectpayment
                            // 'ppProfile.Credentials.Subject = Replace(oDictOpt("accountId"), "_api1", "@")

                            // Try

                            // oDoExpChkResponse = ppIface.DoExpressCheckoutPayment(ppProfile2, oDoExpChkReq)

                            // Select Case oDoExpChkResponse.Ack
                            // Case Protean.PayPalAPI.AckCodeType.Success
                            // bIsValid = True

                            // err_msg = "Paypal Payment Completed - Ref: " & CStr(mnCartId)
                            // ppXform.moPageXML = moPageXml
                            // ppXform.NewFrm("PayForm")
                            // ppXform.valid = bIsValid

                            // Dim cAccountName As String = oExpChkResponse.GetExpressCheckoutDetailsResponseDetails.PayerInfo.PayerID 'PayPal User

                            // Dim oInstanceElmt As XmlElement = ppXform.Instance.OwnerDocument.CreateElement("instance")
                            // Dim oPayPalElmt As XmlElement = ppXform.Instance.OwnerDocument.CreateElement("PayPalExpress")
                            // oInstanceElmt.AppendChild(oPayPalElmt)
                            // 'Add stuff we want to save here...
                            // oPayPalElmt.SetAttribute("payerId", cAccountName)

                            // ppXform.Instance = oInstanceElmt

                            // 'update delivery address

                            // 'update notes ??
                            // err_msg = "Notes:" & oExpChkResponse.GetExpressCheckoutDetailsResponseDetails.Note


                            // savedPaymentId = savePayment(myWeb.mnUserId, "PayPalExpress", mnCartId, cAccountName, ppXform.Instance.FirstChild, Now, False, mnPaymentAmount)
                            // Case Else
                            // Dim ppError As Protean.PayPalAPI.ErrorType
                            // For Each ppError In oDoExpChkResponse.Errors
                            // err_msg = err_msg & " Error:" & ppError.ErrorCode
                            // err_msg = err_msg & " Msg:" & ppError.LongMessage
                            // Next
                            // ppXform.moPageXML = moPageXml
                            // ppXform.NewFrm("PayForm")
                            // ppXform.valid = False
                            // ppXform.addNote(ppXform.moXformElmt, xForm.noteTypes.Alert, err_msg)

                            // End Select

                            // Catch ex As Exception
                            // err_msg = err_msg & "Could not Connect to PayPal at this time please use an alternative payment method."
                            // err_msg = err_msg & " Error:" & ex.Message

                            // ppXform.moPageXML = myWeb.moPageXml
                            // ppXform.NewFrm("PayForm")
                            // ppXform.valid = False
                            // ppXform.addNote(ppXform.moXformElmt, xForm.noteTypes.Alert, "Could not Connect to PayPal at this time please use an alternative payment method.")

                            // End Try

                            case "callback":
                            case "return":
                                {
                                    break;
                                }

                            default:
                                {

                                    string cCurrentURL = oEwProv.moCartConfig("SecureURL") + "?" + returnCmd;

                                    var reDirURLs = new RedirectUrls()
                                    {
                                        return_url = cCurrentURL + "&ppCmd=return", // Return to pay selector page 
                                        cancel_url = cCurrentURL + "&ppCmd=cancel" // Return to pay selector 
                                    };

                                    var oPayerInfo = new PayerInfo();
                                    XmlElement oCartAdd = (XmlElement)oOrder.SelectSingleNode("Contact[@type='Billing']");
                                    string[] aGivenName = Strings.Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ");
                                    switch (Information.UBound(aGivenName))
                                    {

                                        case 0:
                                            {
                                                oPayerInfo.last_name = aGivenName[0];
                                                oPayerInfo.first_name = aGivenName[0];
                                                break;
                                            }
                                        case 1:
                                            {
                                                oPayerInfo.first_name = aGivenName[0];
                                                oPayerInfo.last_name = aGivenName[1];
                                                break;
                                            }
                                        case 2:
                                            {
                                                oPayerInfo.first_name = aGivenName[0];
                                                oPayerInfo.middle_name = aGivenName[1];
                                                oPayerInfo.last_name = aGivenName[2];
                                                break;
                                            }
                                        case 3:
                                            {
                                                oPayerInfo.first_name = aGivenName[0];
                                                oPayerInfo.middle_name = aGivenName[1];
                                                oPayerInfo.last_name = aGivenName[2];
                                                oPayerInfo.suffix = aGivenName[3];
                                                break;
                                            }
                                    }
                                    oPayerInfo.email = oCartAdd.SelectSingleNode("Email").InnerText;
                                    string localgetCountryISONum() { string argsCountry = oCartAdd.SelectSingleNode("Country").InnerText; var ret = getCountryISONum(ref argsCountry); oCartAdd.SelectSingleNode("Country").InnerText = argsCountry; return ret; }

                                    oPayerInfo.billing_address = new Address()
                                    {
                                        line1 = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(oCartAdd.SelectSingleNode("Company").InnerText), oCartAdd.SelectSingleNode("Street").InnerText, oCartAdd.SelectSingleNode("Company").InnerText)),
                                        line2 = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(oCartAdd.SelectSingleNode("Company").InnerText), null, oCartAdd.SelectSingleNode("Street").InnerText)),
                                        city = oCartAdd.SelectSingleNode("City").InnerText,
                                        state = oCartAdd.SelectSingleNode("State").InnerText,
                                        postal_code = oCartAdd.SelectSingleNode("PostalCode").InnerText,
                                        phone = oCartAdd.SelectSingleNode("Tel").InnerText,
                                        country_code = localgetCountryISONum()
                                    };
                                    oCartAdd = (XmlElement)oOrder.SelectSingleNode("Contact[@type='Delivery Address']");
                                    string localgetCountryISONum1() { string argsCountry1 = oCartAdd.SelectSingleNode("Country").InnerText; var ret = getCountryISONum(ref argsCountry1); oCartAdd.SelectSingleNode("Country").InnerText = argsCountry1; return ret; }

                                    oPayerInfo.shipping_address = (ShippingAddress)new Address()
                                    {
                                        line1 = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(oCartAdd.SelectSingleNode("Company").InnerText), oCartAdd.SelectSingleNode("Street").InnerText, oCartAdd.SelectSingleNode("Company").InnerText)),
                                        line2 = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(oCartAdd.SelectSingleNode("Company").InnerText), null, oCartAdd.SelectSingleNode("Street").InnerText)),
                                        city = oCartAdd.SelectSingleNode("City").InnerText,
                                        state = oCartAdd.SelectSingleNode("State").InnerText,
                                        postal_code = oCartAdd.SelectSingleNode("PostalCode").InnerText,
                                        phone = oCartAdd.SelectSingleNode("Tel").InnerText,
                                        country_code = localgetCountryISONum1()
                                    };
                                    var oPayer = new Payer()
                                    {
                                        payment_method = "paypal",
                                        payer_info = oPayerInfo
                                    };

                                    decimal nShippingCost = Conversions.ToDecimal(oOrder.GetAttribute("shippingCost"));
                                    decimal nTaxCost = Round(oOrder.GetAttribute("vatAmt"), bForceRoundup: true);

                                    if (Conversions.ToBoolean(Operators.AndObject(Conversions.ToDouble(oOrder.GetAttribute("vatRate")) > 0d, Operators.ConditionalCompareObjectEqual(LCase(oDictOpt["VATonShipping"]), "on", false))))
                                    {
                                        decimal nShippingVat = Round((double)nShippingCost * (Conversions.ToDouble(oOrder.GetAttribute("vatRate")) / 100d), bForceRoundup: true);
                                        nTaxCost = nTaxCost - nShippingVat;
                                        nShippingCost = nShippingCost + nShippingVat;
                                    }

                                    var oDetails = new Details()
                                    {
                                        tax = nTaxCost.ToString(),
                                        shipping = nShippingCost.ToString(),
                                        subtotal = oEwProv.mnPaymentAmount - nTaxCost - nShippingCost
                                    };

                                    var oAmount = new Amount()
                                    {
                                        currency = Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(oCart.mcCurrencyCode), oCart.mcCurrency, oCart.mcCurrencyCode)),
                                        details = oDetails,
                                        total = oEwProv.mnPaymentAmount
                                    };

                                    var oPayee = new Payee()
                                    {
                                        email = "",
                                        phone = new Phone()
                                        {
                                            country_code = "+44",
                                            national_number = ""
                                        }
                                    };
                                    var oItemListCol = new List<Item>();
                                    foreach (XmlElement oItemElmt in oOrder.SelectNodes("Item"))
                                    {
                                        var oItem = new Item()
                                        {
                                            name = Strings.Left(oItemElmt.SelectSingleNode("Name").InnerText, 120),
                                            quantity = oItemElmt.GetAttribute("quantity"),
                                            description = Strings.Left(oItemElmt.SelectSingleNode("Body").InnerText, 127),
                                            tax = Strings.FormatNumber(Conversions.ToDouble("0" + oItemElmt.GetAttribute("itemTax")), 2),
                                            currency = getCountryISONum(ref mcCurrency),
                                            price = Strings.FormatNumber(oItemElmt.GetAttribute("itemTotal"), 2)
                                        };
                                        oItemListCol.Add(oItem);

                                        // loop through and add any options
                                        foreach (XmlElement oOptElmt in oItemElmt.SelectNodes("Item"))
                                        {
                                            var oItemOpt = new Item()
                                            {
                                                name = Strings.Left(oOptElmt.SelectSingleNode("Name").InnerText, 120),
                                                quantity = oOptElmt.GetAttribute("quantity"),
                                                description = Strings.Left(oOptElmt.SelectSingleNode("Body").InnerText, 127),
                                                tax = Strings.FormatNumber(Conversions.ToDouble("0" + oOptElmt.GetAttribute("itemTax")), 2),
                                                currency = getCountryISONum(ref mcCurrency),
                                                price = Strings.FormatNumber(oOptElmt.GetAttribute("itemTotal"), 2)
                                            };
                                            oItemListCol.Add(oItemOpt);
                                        }
                                    }
                                    var oItemList = new ItemList() { items = oItemListCol };

                                    // build the transaction list / cart items
                                    var oTrasnaction = new Transaction()
                                    {
                                        amount = oAmount,
                                        description = oCart.moCartConfig["OrderNoPrefix"] + oEwProv.mnCartId + " - " + oCartAdd.SelectSingleNode("GivenName").InnerText,
                                        invoice_number = oCart.moCartConfig["OrderNoPrefix"] + oEwProv.mnCartId,
                                        payee = oPayee,
                                        item_list = oItemList
                                    };
                                    var oTransactionList = new List<Transaction>();
                                    oTransactionList.Add(oTrasnaction);

                                    var oPayment = new PayPal.Api.Payment()
                                    {
                                        intent = "sale",
                                        payer = oPayer,
                                        transactions = oTransactionList,
                                        redirect_urls = reDirURLs
                                    };

                                    var oCreatePayment = oPayment.Create(ppApiCtx);


                                    // If mcPaymentType = "deposit" Then

                                    // Dim oDiscountItem As Protean.PayPalAPI.PaymentDetailsItemType = New Protean.PayPalAPI.PaymentDetailsItemType
                                    // oDiscountItem.Name = "Final payment to be made later"
                                    // oDiscountItem.Quantity = "1"
                                    // 'itemtax
                                    // Dim oItemTax As Protean.PayPalAPI.BasicAmountType = New Protean.PayPalAPI.BasicAmountType
                                    // oItemTax.currencyID = getPPCurencyFromCode(mcCurrency)
                                    // oItemTax.Value = "0"
                                    // oDiscountItem.Tax = oItemTax
                                    // 'itemAmount
                                    // 'Dim nItemPlusTax As Double = FormatNumber(oItemElmt.GetAttribute("price"), 2) + (CDbl("0" & oItemElmt.GetAttribute("itemTax")) / oItemElmt.GetAttribute("quantity"))
                                    // Dim oItemAmount As Protean.PayPalAPI.BasicAmountType = New Protean.PayPalAPI.BasicAmountType
                                    // oItemAmount.currencyID = getPPCurencyFromCode(mcCurrency)
                                    // oItemAmount.Value = FormatNumber(mnPaymentMaxAmount - mnPaymentAmount, 2) * -1
                                    // oDiscountItem.Amount = oItemAmount
                                    // oItemGroup(i) = oDiscountItem

                                    // End If

                                    string redirectUrl;
                                    var links = oCreatePayment.links.GetEnumerator();
                                    while (links.MoveNext())
                                    {
                                        var thislink = links.Current;
                                        if (thislink.rel.ToLower().Trim() == "approval_url")
                                        {
                                            redirectUrl = thislink.href;
                                        }
                                    }

                                    ppXform.NewFrm("PayForm");
                                    ppXform.valid = false;
                                    break;
                                }

                                // Select Case oExpChkResponse.Ack
                                // Case Protean.PayPalAPI.AckCodeType.Success, Protean.PayPalAPI.AckCodeType.SuccessWithWarning
                                // myWeb.msRedirectOnEnd = redirectUrl
                                // err_msg = "Redirect to - " & redirectUrl
                                // Case Else
                                // Dim ppError As Protean.PayPalAPI.ErrorType
                                // For Each ppError In oExpChkResponse.Errors
                                // err_msg = err_msg & " Error:" & ppError.ErrorCode
                                // err_msg = err_msg & " Msg:" & ppError.LongMessage
                                // Next
                                // ppXform.addNote(ppXform.moXformElmt, xForm.noteTypes.Alert, err_msg)
                                // End Select

                        }


                        // Update Seller Notes:

                        sSql = "select * from tblCartOrder where nCartOrderKey = " + oCart.mnCartId;
                        DataSet oDs;
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                        {
                            if (bIsValid)
                            {
                                oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), DateTime.Today), " "), DateAndTime.TimeOfDay), ": changed to: (Payment Received) "), Constants.vbLf), "comment: "), err_msg);
                            }
                            else if (err_msg.StartsWith("Redirect"))
                            {
                                oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), DateTime.Today), " "), DateAndTime.TimeOfDay), ": changed to: (User Redirected)"), Constants.vbLf), "comment: "), err_msg);
                            }
                            else
                            {
                                oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), DateTime.Today), " "), DateAndTime.TimeOfDay), ": changed to: (Payment Failed) "), Constants.vbLf), "comment: "), err_msg);
                            }
                        }
                        myWeb.moDbHelper.updateDataset(ref oDs, "Order");

                        return ppXform;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug);
                        return (Protean.xForm)null;
                    }
                }

                public new bool AddPaymentButton(ref Protean.xForm oOptXform, ref XmlElement oFrmElmt, XmlElement configXml, double nPaymentAmount, string submissionValue, string refValue)
                {
                    try
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
                        if (configXml.SelectSingleNode("icon/@value") is not null)
                        {
                            iconclass = configXml.SelectSingleNode("icon/@value").InnerText;
                        }

                        oOptXform.addSubmit(ref oFrmElmt, submissionValue, PaymentLabel, refValue, "pay-button pay-" + configXml.GetAttribute("name"), iconclass, configXml.GetAttribute("name"));
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "AddPaymentButton", ex, "", "", gbDebug);
                        return default;
                    }

                    return default;
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


                public new string CheckStatus(ref Cms myWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";

                    // Dim oPayPalProCfg As XmlNode    'Never Used
                    // Dim nTransactionMode As TransactionMode

                    try
                    {
                        return "not checked";
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }

                public string CancelPayments(ref Cms myWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";
                    // Dim oPayPalProCfg As XmlNode  'Never Used

                    try
                    {

                        return "Not Implemented";
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug);
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