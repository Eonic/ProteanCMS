using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
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
        public class SagePayV3
        {

            public SagePayV3()
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

                private const string mcModuleName = "Providers.Payment.SagePayV3.Activities";
                private new Cms myWeb;
                protected XmlNode moPaymentCfg;
                private TransactionMode nTransactionMode;

                public enum TransactionMode
                {
                    Live = 0,
                    Test = 1,
                    Fail = 2
                }

                public new Protean.xForm GetPaymentForm(ref Cms oWeb, ref Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails")
                {

                    HttpWebRequest oRequest;
                    HttpWebResponse oResponse;
                    Stream oStream;
                    StreamReader oStreamReader;

                    long nResult;

                    string cResponse;
                    string cRequest;
                    var oEncoding = new ASCIIEncoding();
                    byte[] byRequest;

                    string sSql;

                    string cMessage;

                    Protean.xForm ccXform;

                    bool bIsValid = false;
                    string err_msg = "";
                    string err_msg_log = "";
                    string sProcessInfo = "";
                    var oDictOpt = new Hashtable();

                    bool bCv2 = false;
                    bool b3dSecure = false;
                    Hashtable oResponseDict = null;
                    XmlNode oCartAdd;
                    bool b3dAuthorised = false;

                    XmlNode oSagePayV3Cfg;

                    string cProcessInfo = "SagePayV3Tx";
                    string sAPIVer = "3.00";
                    string sVSPUrl = "";
                    string sVSP3DSUrl = "";
                    long nAttemptCount;

                    string SagePayPartnerID = "C1B0075E-F99D-4E5D-8AE0-E66F13FD081E";

                    string cSellerNotes = "";


                    bool bSavePayment = false;

                    try
                    {
                        myWeb = oWeb;
                        string cIPAddress = myWeb.moRequest.ServerVariables["REMOTE_ADDR"];
                        string sSubmitPath = oCart.mcPagePath + returnCmd;

                        // confirm TLS 1.2
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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



                        if (myWeb.moSession["attemptCount"] is not null)
                        {
                            nAttemptCount = Conversions.ToLong(myWeb.moSession["attemptCount"]);
                            nAttemptCount = nAttemptCount + 1L;
                        }
                        else
                        {
                            nAttemptCount = 1L;
                        }

                        // Get the payment options into a hashtable
                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        oSagePayV3Cfg = moPaymentCfg.SelectSingleNode("provider[@name='SagePayV3']");
                        foreach (XmlElement oElmt in oSagePayV3Cfg.SelectNodes("*"))
                        {
                            if (oElmt.GetAttribute("value") is not null)
                            {
                                oDictOpt.Add(oElmt.Name, oElmt.GetAttribute("value"));
                            }
                        }

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictOpt["validateCV2"], "on", false)))
                            bCv2 = true;
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDictOpt["secure3d"], "2", false)))
                        {
                            b3dSecure = true;
                            // Check the IP Addresses for a block
                            if (oDictOpt["secure3dIpBlock"] is not null)
                            {
                                var oRE = new Regex("(,|^)" + Strings.Replace(cIPAddress, ".", @"\.") + "(,|$)");
                                if (oRE.IsMatch(Conversions.ToString(oDictOpt["secure3dIpBlock"])))
                                    b3dSecure = false;
                            }
                        }

                        // sAPIVer = "2.22"

                        sAPIVer = "3.00";

                        switch (Convert.ToString(oDictOpt["opperationMode"]) ?? "")
                        {
                            case "test":
                            case "true":
                                {
                                    sVSPUrl = "https://test.sagepay.com/gateway/service/vspdirect-register.vsp";
                                    sVSP3DSUrl = "https://test.sagepay.com/gateway/service/direct3dcallback.vsp";
                                    nTransactionMode = TransactionMode.Test;
                                    break;
                                }
                            case "live":
                                {
                                    sVSPUrl = "https://live.sagepay.com/gateway/service/vspdirect-register.vsp";
                                    sVSP3DSUrl = "https://live.sagepay.com/gateway/service/direct3dcallback.vsp";
                                    nTransactionMode = TransactionMode.Live;
                                    break;
                                }
                        }


                        // Load the Xform
                        ccXform = oEwProv.creditCardXform(oOrder, "PayForm", sSubmitPath, oDictOpt["cardsAccepted"], true, Operators.ConcatenateObject(Operators.ConcatenateObject("Make Payment of " + Strings.FormatNumber(oEwProv.mnPaymentAmount, 2) + " ", oDictOpt["currency"]), " by Credit/Debit Card"));

                        if (b3dSecure)
                        {
                            // check for return from aquiring bank
                            if (!string.IsNullOrEmpty(myWeb.moRequest["PARes"]))
                            {
                                b3dAuthorised = true;
                            }
                        }

                        string orderPrefix = "";

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDictOpt["sendPrefix"], "true", false)))
                        {
                            orderPrefix = Conversions.ToString(Interaction.IIf(nTransactionMode == TransactionMode.Test, "D-", "")) + oEwProv.moCartConfig("OrderNoPrefix");
                        }

                        if (ccXform.valid == true | b3dAuthorised)
                        {
                            if (b3dAuthorised)
                            {
                                // 3D Secure Resume
                                cRequest = "MD=" + myWeb.moRequest["MD"] + "&";
                                cRequest = cRequest + "PARes=" + goServer.UrlEncode(myWeb.moRequest["PARes"]) + "&";
                            }
                            else
                            {
                                XmlElement oSaveElmt = (XmlElement)ccXform.Instance.SelectSingleNode("creditCard/bSavePayment");
                                if (oSaveElmt is not null)
                                {
                                    if (oSaveElmt.InnerText == "true")
                                    {
                                        bSavePayment = true;
                                    }
                                }

                                // standard card validation request
                                cRequest = "VPSProtocol=" + sAPIVer + "&";
                                cRequest = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(cRequest + "TxType=", oDictOpt["transactionType"]), "&")); // 0=omited 1=full auth 2=pre-auth only
                                cRequest = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(cRequest + "Vendor=", oDictOpt["accountId"]), "&")); // 0=omited 1=full auth 2=pre-auth only
                                if (nAttemptCount > 1L)
                                {
                                    cRequest = cRequest + "VendorTXCode=" + goServer.UrlEncode(orderPrefix + oEwProv.mnCartId) + "-" + nAttemptCount + "&";
                                }
                                else
                                {
                                    cRequest = cRequest + "VendorTXCode=" + goServer.UrlEncode(orderPrefix + oEwProv.mnCartId) + "&";
                                }
                                cRequest = cRequest + "Amount=" + goServer.UrlEncode(FullMoneyString(oEwProv.mnPaymentAmount)) + "&";
                                cRequest = cRequest + "Currency=" + goServer.UrlEncode(Conversions.ToString(oDictOpt["currency"])) + "&";
                                cRequest = cRequest + "Description=" + goServer.UrlEncode(Strings.Left(oEwProv.mcPaymentOrderDescription, 90)) + "&";
                                cRequest = cRequest + "CardHolder=" + goServer.UrlEncode(oEwProv.mcCardHolderName) + "&";
                                cRequest = cRequest + "CardNumber=" + goServer.UrlEncode(this.FormatCreditCardNumber(myWeb.moRequest["creditCard/number"]).ToString()) + "&";
                                // If Not myWeb.moRequest("creditCard/issueDate") = "" Then
                                // cRequest = cRequest & "StartDate=" & fmtSecPayDate(myWeb.moRequest("creditCard/issueDate")) & "&"
                                // End If
                                cRequest = cRequest + "ExpiryDate=" + this.fmtSecPayDate(myWeb.moRequest["creditCard/expireDate"]) + "&";
                                // cRequest = cRequest & "IssueNo=" & goServer.UrlEncode(myWeb.moRequest("creditCard/issueNumber")) & "&"
                                cRequest = cRequest + "CV2=" + goServer.UrlEncode(myWeb.moRequest["creditCard/CV2"]) + "&";
                                cRequest = cRequest + "CardType=" + goServer.UrlEncode(myWeb.moRequest["creditCard/type"]) + "&";
                                // cRequest = cRequest & "Token=" & goServer.UrlEncode(myWeb.moRequest("creditCard/type")) & "&"

                                string FirstName = "";
                                string LastName = "";
                                string[] aGivenName = Strings.Split(oEwProv.mcCardHolderName, " ");
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
                                            // MiddleName = aGivenName(1)
                                            LastName = aGivenName[2];
                                            break;
                                        }
                                    case 3:
                                        {
                                            FirstName = aGivenName[0];
                                            // MiddleName = aGivenName(1)
                                            LastName = aGivenName[2];
                                            break;
                                        }
                                        // Suffix = aGivenName(3)
                                }


                                cRequest = cRequest + "BillingSurname=" + LastName + "&";
                                cRequest = cRequest + "BillingFirstnames=" + FirstName + "&";


                                oCartAdd = oOrder.SelectSingleNode("Contact[@type='Billing Address']");

                                cRequest = cRequest + "BillingAddress1=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("Street").InnerText) + "&";
                                // cRequest = cRequest & "BillingAddress2=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("Street2").InnerText) & "&"
                                cRequest = cRequest + "BillingCity=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("City").InnerText) + "&";
                                cRequest = cRequest + "BillingPostCode=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("PostalCode").InnerText) + "&";

                                cRequest = cRequest + "BillingCountry=" + goServer.UrlEncode(oEwProv.getCountryISO2Code(oCartAdd.SelectSingleNode("Country").InnerText)) + "&";
                                if (oEwProv.getCountryISO2Code(oCartAdd.SelectSingleNode("Country").InnerText) == "US")
                                {
                                    cRequest = cRequest + "BillingState=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("State").InnerText) + "&";
                                }
                                cRequest = cRequest + "BillingPhone=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("Telephone").InnerText) + "&";


                                oCartAdd = oOrder.SelectSingleNode("Contact[@type='Delivery Address']");
                                aGivenName = Strings.Split(oCartAdd.SelectSingleNode("GivenName").InnerText, " ");

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
                                            // MiddleName = aGivenName(1)
                                            LastName = aGivenName[2];
                                            break;
                                        }
                                    case 3:
                                        {
                                            FirstName = aGivenName[0];
                                            // MiddleName = aGivenName(1)
                                            LastName = aGivenName[2];
                                            break;
                                        }
                                        // Suffix = aGivenName(3)
                                }

                                cRequest = cRequest + "DeliverySurname=" + LastName + "&";
                                cRequest = cRequest + "DeliveryFirstnames=" + FirstName + "&";
                                cRequest = cRequest + "DeliveryAddress1=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("Street").InnerText) + "&";
                                // cRequest = cRequest & "BillingAddress2=" & goServer.UrlEncode(oCartAdd.SelectSingleNode("Street2").InnerText) & "&"
                                cRequest = cRequest + "DeliveryCity=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("City").InnerText) + "&";
                                cRequest = cRequest + "DeliveryPostCode=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("PostalCode").InnerText) + "&";
                                cRequest = cRequest + "DeliveryCountry=" + goServer.UrlEncode(oEwProv.getCountryISO2Code(oCartAdd.SelectSingleNode("Country").InnerText)) + "&";
                                if (oEwProv.getCountryISO2Code(oCartAdd.SelectSingleNode("Country").InnerText) == "US")
                                {
                                    cRequest = cRequest + "DeliveryState=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("State").InnerText) + "&";
                                }

                                cRequest = cRequest + "DeliveryPhone=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("Telephone").InnerText) + "&";

                                if (!string.IsNullOrEmpty(oCartAdd.SelectSingleNode("Email").InnerText))
                                {
                                    cRequest = cRequest + "CustomerEmail=" + goServer.UrlEncode(oCartAdd.SelectSingleNode("Email").InnerText) + "&";
                                }
                                else
                                {
                                    cRequest = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(cRequest + "CustomerEmail=", oDictOpt["MerchantEmail"]), "&"));
                                }

                                cRequest = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(cRequest + "ApplyAVSCV2=", oDictOpt["validateCV2"]), "&"));

                                // This can be used to send the unique reference for the Partner that referred the Vendor to Sage Pay.
                                cRequest = cRequest + "ReferrerID=" + SagePayPartnerID + "&";

                                string cIP = myWeb.moRequest.ServerVariables["REMOTE_ADDR"];
                                if (cIP.Length < 4)
                                    cIP = "127.0.0.1";
                                cRequest = cRequest + "ClientIPAddress=" + cIP + "&";

                                cRequest = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(cRequest + "Apply3DSecure=", oDictOpt["secure3d"]), "&"));

                                if (bSavePayment)
                                {
                                    cRequest = cRequest + "CreateToken=1&StoreToken=1&";
                                }
                                var oBasketXml = new XmlDocument();
                                var oBasketRoot = oBasketXml.CreateElement("basket");
                                oBasketXml.AppendChild(oBasketRoot);

                                foreach (XmlElement oItemElmt in oOrder.SelectNodes("Item"))
                                {
                                    var oItemRoot = oBasketXml.CreateElement("item");
                                    double unitTaxAmount = Conversions.ToDouble("0" + oItemElmt.GetAttribute("itemTax")) / Conversions.ToDouble("0" + oItemElmt.GetAttribute("quantity"));
                                    unitTaxAmount = (double)decimal.Round((decimal)unitTaxAmount, 2, MidpointRounding.AwayFromZero);

                                    double unitGrossAmount = Conversions.ToDouble("0" + oItemElmt.GetAttribute("price")) + unitTaxAmount;
                                    unitGrossAmount = (double)decimal.Round((decimal)unitGrossAmount, 2, MidpointRounding.AwayFromZero);

                                    double totalGrossAmount = unitGrossAmount * Conversions.ToDouble("0" + oItemElmt.GetAttribute("quantity"));
                                    totalGrossAmount = (double)decimal.Round((decimal)totalGrossAmount, 2, MidpointRounding.AwayFromZero);

                                    string productDescription = oItemElmt.SelectSingleNode("Name").InnerText;
                                    productDescription = Regex.Replace(productDescription, @"[^A-Za-z0-9\-/]", "");
                                    if (productDescription.Length > 100)
                                    {
                                        productDescription = productDescription.Substring(0, 100);
                                    }
                                    XmlNode argoNode = oItemRoot;
                                    addNewTextNode("description", ref argoNode, productDescription);
                                    // If Not oItemElmt.SelectSingleNode("productDetail/StockCode") Is Nothing Then
                                    // xmlTools.addNewTextNode("productSku", oItemRoot, oItemElmt.SelectSingleNode("productDetail/StockCode").InnerText)
                                    // End If
                                    // If Not oItemElmt.SelectSingleNode("productDetail/Manufacturer") Is Nothing Then
                                    // xmlTools.addNewTextNode("productCode", oItemRoot, oItemElmt.SelectSingleNode("productDetail/Manufacturer").InnerText)
                                    // End If
                                    XmlNode argoNode1 = oItemRoot;
                                    addNewTextNode("quantity", ref argoNode1, oItemElmt.GetAttribute("quantity"));
                                    XmlNode argoNode2 = oItemRoot;
                                    addNewTextNode("unitNetAmount", ref argoNode2, oItemElmt.GetAttribute("price"));
                                    XmlNode argoNode3 = oItemRoot;
                                    addNewTextNode("unitTaxAmount", ref argoNode3, unitTaxAmount.ToString());
                                    XmlNode argoNode4 = oItemRoot;
                                    addNewTextNode("unitGrossAmount", ref argoNode4, unitGrossAmount.ToString());
                                    XmlNode argoNode5 = oItemRoot;
                                    addNewTextNode("totalGrossAmount", ref argoNode5, totalGrossAmount.ToString());
                                    oBasketRoot.AppendChild(oItemRoot);
                                }

                                XmlNode argoNode6 = oBasketRoot;
                                addNewTextNode("deliveryGrossAmount", ref argoNode6, oOrder.GetAttribute("shippingCost"));

                                cRequest = cRequest + "BasketXML=" + oBasketXml.OuterXml + "&";

                                cRequest = cRequest + "Website=" + goServer.UrlEncode(oEwProv.moCartConfig("SiteURL")) + "&";

                                // goSession("attemptCount") = nAttemptCount + 1
                            }

                            // Convert the request to bytes
                            if (cRequest.EndsWith("&"))
                                cRequest = cRequest.Trim("&".ToCharArray());

                            byRequest = oEncoding.GetBytes(cRequest);

                            if (b3dAuthorised)
                            {
                                oRequest = (HttpWebRequest)WebRequest.Create(sVSP3DSUrl);
                            }
                            else
                            {
                                oRequest = (HttpWebRequest)WebRequest.Create(sVSPUrl);
                            }

                            oRequest.ContentType = "application/x-www-form-urlencoded";
                            oRequest.ContentLength = byRequest.Length;
                            oRequest.Method = "POST";
                            oStream = oRequest.GetRequestStream();
                            oStream.Write(byRequest, 0, byRequest.Length);
                            oStream.Close();

                            oResponse = (HttpWebResponse)oRequest.GetResponse();
                            oStream = oResponse.GetResponseStream();
                            oStreamReader = new StreamReader(oStream, Encoding.UTF8);
                            cResponse = oStreamReader.ReadToEnd();

                            oStreamReader.Close();
                            oResponse.Close();

                            myWeb.moSession["attemptCount"] = (object)nAttemptCount;

                            // Validate the response.

                            if (string.IsNullOrEmpty(cResponse) | Strings.InStr(cResponse, "=") == 0)
                            {
                                err_msg = "There was a communications error.";
                            }
                            else
                            {
                                // lets take the response and put it in a hash table
                                cProcessInfo = "Error translating response:" + cResponse;
                                oResponseDict = UrlResponseToHashTable(cResponse, Constants.vbCrLf, "=", false);

                                nResult = Conversions.ToLong(oResponseDict["intStatus"]);

                                switch (oResponseDict["Status"].ToString() ?? "")
                                {

                                    case "OK":
                                    case "AUTHENTICATED":
                                    case "REGISTERED":
                                        {
                                            // Successful Authorisation
                                            err_msg = Conversions.ToString(Operators.ConcatenateObject("Payment was successful. Transaction ref: ", oResponseDict["VPSTxId"]));
                                            bIsValid = true;

                                            myWeb.moSession["attemptCount"] = (object)null;
                                            break;
                                        }

                                    case "3DAUTH":
                                        {

                                            // create an xform that automatically redirects to Aquiring Banks 3DS portal.
                                            // Save MD as paymentRef
                                            myWeb.moSession["VPSTxId"] = oResponseDict["VPSTxId"];
                                            myWeb.moSession["SecurityKey"] = oResponseDict["SecurityKey"];

                                            string sRedirectURL;

                                            if (oEwProv.moCartConfig("SecureURL").EndsWith("/"))
                                            {
                                                sRedirectURL = oEwProv.moCartConfig("SecureURL") + "?cartCmd=Redirect3ds";
                                            }
                                            else
                                            {
                                                sRedirectURL = oEwProv.moCartConfig("SecureURL") + "/?cartCmd=Redirect3ds";
                                            }

                                            bIsValid = false;
                                            ccXform.valid = false;

                                            err_msg_log = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("ACS URL:", oResponseDict["ACSURL"]), " ACS URLDecoded:"), goServer.UrlDecode(Conversions.ToString(oResponseDict["ACSURL"]))), " MD:"), oResponseDict["MD"]), " PAReq:"), oResponseDict["PAReq"]));
                                            err_msg = "This card has subscribed to 3D Secure. You will now be re-directed to your banks website for further verification.";

                                            ccXform = oEwProv.xfrmSecure3D(goServer.UrlDecode(Conversions.ToString(oResponseDict["ACSURL"])), oResponseDict["MD"], oResponseDict["PAReq"], sRedirectURL);
                                            break;
                                        }

                                    case "MALFORMED":
                                    case "INVALID":
                                    case "NOTAUTHED":
                                    case "REJECTED":
                                    case "ERROR":
                                        {
                                            // Failed / Error Authorisation

                                            cMessage = Conversions.ToString(oResponseDict["StatusDetail"]);

                                            if (Strings.InStr(cMessage, "card number") > 0)
                                                ccXform.addNote("creditCard/number", Protean.xForm.noteTypes.Alert, "The card number given is not valid.");
                                            if (Strings.InStr(cMessage, "IssueNumber") > 0)
                                                ccXform.addNote("creditCard/issueNumber", Protean.xForm.noteTypes.Alert, "The issue number is not valid - it may not be required for non-Switch/Solo cards.");
                                            if (Strings.InStr(cMessage, "StartDate") > 0 | Strings.InStr(cMessage, "Start Date") > 0)
                                                ccXform.addNote("creditCard/issueDate", Protean.xForm.noteTypes.Alert, "The issue date is not valid - it may not be required for Switch or Solo cards.");
                                            if (Strings.InStr(cMessage, "ExpiryDate") > 0 | Strings.InStr(cMessage, "Expiry Date") > 0)
                                                ccXform.addNote("creditCard/expireDate", Protean.xForm.noteTypes.Alert, "The expiry date is not valid.");

                                            err_msg_log = cMessage;

                                            if (gbDebug)
                                            {
                                                err_msg = "<br/>Full Request:" + goServer.HtmlEncode(goServer.UrlDecode(cRequest)) + "<br/>Full Response:" + goServer.HtmlEncode(goServer.UrlDecode(cResponse));
                                            }
                                            else
                                            {
                                                err_msg = "There was an error processing this payment.<br/>  No payment has been made.<br/>  Please check the details you entered and try again, or call for assistance.<br/><br/>  The error returned fron the bank was :<br/><br/> " + goServer.HtmlEncode(goServer.UrlDecode(cMessage));

                                                // goSession("attemptCount") = Nothing
                                            }

                                            break;
                                        }

                                    default:
                                        {
                                            // Response not recognised.
                                            cMessage = Conversions.ToString(oResponseDict["StatusDetail"]);

                                            if (gbDebug)
                                            {
                                                err_msg = "<br/>Full Request:" + goServer.HtmlEncode(goServer.UrlDecode(cRequest)) + "<br/>Full Response:" + goServer.HtmlEncode(goServer.UrlDecode(cResponse));
                                            }
                                            else
                                            {
                                                err_msg_log = "<br/>Full Request:" + goServer.HtmlEncode(goServer.UrlDecode(cRequest)) + "<br/>Full Response:" + goServer.HtmlEncode(goServer.UrlDecode(cResponse));
                                                err_msg = "There was an error processing this payment.  No payment has been made.  Please check the details you entered and try again, or call for assistance.  The error detail was : " + goServer.HtmlEncode(goServer.UrlDecode(cMessage));
                                            }

                                            myWeb.moSession["attemptCount"] = (object)null;
                                            break;
                                        }
                                }

                            }

                            ccXform.addNote("creditCard", Protean.xForm.noteTypes.Alert, err_msg, true);

                            if (bIsValid)
                            {
                                cSellerNotes = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Conversions.ToString(DateTime.Today) + " " + Conversions.ToString(DateAndTime.TimeOfDay) + ": changed to: (Payment Received) " + Constants.vbLf + "Transaction Ref:", oResponseDict["VPSTxId"]), Constants.vbLf), "comment: "), err_msg));
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(err_msg_log) & !string.IsNullOrEmpty(err_msg))
                                {
                                    err_msg_log = err_msg;
                                }
                                cSellerNotes = Conversions.ToString(DateTime.Today) + " " + Conversions.ToString(DateAndTime.TimeOfDay) + ": changed to: (Payment Failed) " + Constants.vbLf + "comment: " + err_msg_log;
                            }

                            ccXform.valid = bIsValid;
                            // Save Payment

                            // Set up the save payment.
                            // Note from Ali - not really sure what's involved here and this falls over if 3d Secure is involved,
                            // so need to have other options if coming back from 3d secure

                            if (!b3dAuthorised)
                            {
                                var oDate = new DateTime(Conversions.ToInteger("20" + Strings.Right(myWeb.moRequest["creditCard/expireDate"], 2)), Conversions.ToInteger(Strings.Left(myWeb.moRequest["creditCard/expireDate"], 2)), 1);
                                oDate = oDate.AddMonths(1);
                                oDate = oDate.AddDays(-1);
                                string cMethodName = myWeb.moRequest["creditCard/type"] + ": " + stdTools.MaskString(myWeb.moRequest["creditCard/number"], "*", false, 4) + " Expires: " + oDate.ToShortDateString();

                                var oCustomElmt = ccXform.Instance.OwnerDocument.CreateElement("SagePayV3");

                                oCustomElmt.SetAttribute("VPSTxId", Conversions.ToString(oResponseDict["VPSTxId"]));
                                oCustomElmt.SetAttribute("SecurityKey", Conversions.ToString(oResponseDict["SecurityKey"]));
                                oCustomElmt.SetAttribute("TxAuthNo", Conversions.ToString(oResponseDict["TxAuthNo"]));
                                ccXform.Instance.FirstChild.AppendChild(oCustomElmt);


                                if (bSavePayment & bIsValid)
                                {
                                    oEwProv.savePayment(myWeb.mnUserId, "SagePayV3", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, oDate, true, oEwProv.mnPaymentAmount);
                                }
                                else
                                {
                                    oEwProv.savePayment(myWeb.mnUserId, "SagePayV3", oEwProv.mnCartId, cMethodName, ccXform.Instance.FirstChild, DateTime.Now, false, oEwProv.mnPaymentAmount);
                                }

                            }
                        }

                        else
                        {
                            if (ccXform.isSubmitted())
                            {
                                cSellerNotes = Conversions.ToString(DateTime.Today) + " " + Conversions.ToString(DateAndTime.TimeOfDay) + ": changed to: (Payment Form EonicWeb Validation Failed) " + Constants.vbLf + "comment: " + ccXform.validationError;
                            }
                            else
                            {
                                cSellerNotes = Conversions.ToString(DateTime.Today) + " " + Conversions.ToString(DateAndTime.TimeOfDay) + ": changed to: (Payment Form Presented) " + Constants.vbLf + "comment: " + err_msg_log;
                            }
                            ccXform.valid = false;
                        }

                        // Update Seller Notes:
                        sSql = "select * from tblCartOrder where nCartOrderKey = " + oEwProv.mnCartId;
                        DataSet oDs;
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                            oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), cSellerNotes);
                        myWeb.moDbHelper.updateDataset(ref oDs, "Order");

                        return ccXform;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug);
                        return (Protean.xForm)null;
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



                public new string CheckStatus(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";
                    // Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                    // Dim oSagePayV3Cfg As XmlNode
                    // Dim nTransactionMode As TransactionMode

                    try
                    {

                        return "";
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }

                public string CancelPayments(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";
                    string moPaymentCfg = Conversions.ToString(WebConfigurationManager.GetWebApplicationSection("protean/payment"));
                    // Dim oSagePayV3Cfg As XmlNode

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