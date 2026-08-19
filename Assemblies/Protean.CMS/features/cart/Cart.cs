using Newtonsoft.Json.Linq;
using Protean.Providers.Membership;
using Protean.Providers.Messaging;
using Protean.Providers.Payment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Protean.Cms;
using static Protean.Cms.dbHelper;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{


    public partial class Cms
    {

        public partial class Cart : IDisposable
        {
            #region Declarations


            public System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
            public System.Collections.Specialized.NameValueCollection moConfig;

            //XmlElement moPaymentCfg = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/payment");


            private System.Web.HttpServerUtility moServer;

            public XmlDocument moPageXml;

            public string mcModuleName = "Protean.Cms.Cart";

            // Session EonicWeb Details
            public string mcEwDataConn;
            public string mcEwSiteDomain;
            public bool mbEwMembership;
           

            public XmlElement oShippingOptions;

            // MEMB - notes beginning with MEMB relate to changes to the cart to incorporate logging in members and adding their addresses to contact fields in the billing/delivery forms
            // Cart Status Ref
            // 1   new cart
            // 2   entered address
            // 3   abbandoned
            // 4   pass to payment
            // 5   Transaction Failed
            // 6   Payment Successful
            // 7   Order Cancelled / Payment Refunded
            // 8   Failed and Checked
            // 9   Order Shipped
            // 10   Part Payment (e.g. depsoit) Received
            // 11   Settlement initiated?
            // 12   Awaiting Payment

            public string mcSiteURL; // Site Identifier, used for User Cookie Name
            public string mcCartURL; // Site Identifier, used for User Cookie Name

            public long mnCartId; // Unique Id refering to this session cart
            public string mcSessionId; // Session ID - Unique for each client browser
                                       // private string mcRefSessionId; // Referrer Site Session ID - The session ID from the referrer site, if passed.
            public long mnEwUserId; // User Id for Membership integration
            public string mmcOrderType; // The order type associated with the current cart
            public string mcItemOrderType; // The order type associated with the current page (if provided)
            public XmlElement moCartXml;

            public int mnGiftListId = -1; // If the current user is buying from a giftlist

            public string mcNotesXForm; // Location of Notes xform prior to billing details
            public double mnTaxRate; // Add Tax to Cart Rate
            public string mcTermsAndConditions = "";
            public string mcMerchantEmail;
            public string mcMerchantEmailTemplatePath;
            public bool mbStockControl = false; // Stock Control
            public string mcDeposit; // Deposits are Available
            public string mcDepositAmount; // Deposit Amount
            public string mcPaymentType;
            private string cOrderNoPrefix;
            public string mcCurrency = "";
            public string mcCurrencySymbol = "";

            public string mcCurrencyRef; // TS requires further investigation as to if these are requred or mcCurrency is sufficient
            public string mcCurrencyCode;

            public string mcVoucherNumber = "";
            public string mcVoucherValue = "";
            public string mcVoucherExpires = "";
            private string promocodeFromExternalRef = "";
            public string mcPersistCart = "";
            public string mcPagePath;
            public long mnPaymentId = 0; // to be populated by payment prvoider to pass to subscriptions

            public bool bFullCartOption;
            public bool mbAddItemWithNoPrice; // Switch to allow enquiries of items with no price

            // Address Mods
            public string mcBillingAddressXform = ""; // Location of bespoke Billing Address
            public string mcDeliveryAddressXform = ""; // Location of bespoke Delivery Address
            public string mcPriorityCountries; // List of countires to appear at the top of dropdowns 
            public bool mbNoDeliveryAddress = false; // Option to turn off the need for a delivery address

            public string mcReturnPage; // page to return to with continue Shopping
            public string mcPaymentGetwayEmergencyMessage;

            public string mcCartCmd = ""; // Action String for ewCart main function, ewcartPlugin()
                                          // Can be:     <case sensitive>
                                          // Billing
                                          // Delivery
                                          // Cart
                                          // Add
                                          // Remove
                                          // ShowInvoice
                                          // MakePayment
                                          // Failed Payment
                                          // Quit

            public string mcCartCmdAlt = ""; // alternative if we call apply again


            public short mnProcessId = 0; // State of Cart:
                                          // 0:      Empty / New Cart
                                          // 1:      Shopping / Items in Cart
                                          // 2:      Billing Address is Complete
                                          // 3:      Billing & Delivery Addressi are Complete
                                          // 4:      Order Confirmed / Ready to make payment
                                          // 5:      Transaction Complete
                                          // 6:      Tender Cancelled

            public short mnProcessError; // General Process Error has been encountered:
                                         // 1:      Cookies are disabled or undetectable
                                         // 2:      The current item's order type does not match the cart's order type
                                         // 100+:   Payment gateway errors
                                         // 1000+:  Bespoke errors - can be defined in the xsl

            public string mcPaymentMethod = ""; // Payment Method:
                                                // SecPay
                                                // ProTx
                                                // Secure Email
                                                // MetaCharge
                                                // WorldPay
                                                // Cheque
            public string mcPaymentProfile = "";



            // Behaviour mods
            private bool mbRedirectSecure = false;

            private bool mbDisplayPrice = true;

            public string mcSubmitText;
            public Cms.dbHelper moDBHelper;

            public string mcOrderType;
            public string cOrderReference;

            public bool mbVatAtUnit = false;
            public bool mbVatOnLine = false;
            public bool mbRoundup = false;
            public bool mbRoundDown = false;
            public bool mbDiscountsOn = false;
            public bool mbOveridePrice = false;
            public string mcPriceModOrder = "";
            public string mcUnitModOrder = "";


            public string mcReEstablishSession;


            public int mnShippingRootId;
            // Public mcCurrencySymbol As String

            public Cart.Discount moDiscount;
            public Cart.Subscriptions moSubscription;
            protected PaymentProviders moPay;

            public bool mbQuitOnShowInvoice = true;
            private bool mbDepositOnly = false;
            public bool mbBlockCartCmd = false; // Used for reseting payment on subscripitions
            public string mcBlockCartUpdate;


            public enum cartError
            {

                OutOfStock = 2,
                ProductQuantityOverLimit = 201,
                ProductQuantityUnderLimit = 202,
                ProductQuantityNotInBulk = 203

            }


            public enum cartProcess
            {

                Empty = 0,
                Items = 1,
                Billing = 2,
                Delivery = 3,
                Confirmed = 4,
                PassForPayment = 5,
                Complete = 6,
                Refunded = 7,
                Failed = 8,
                Shipped = 9,
                DepositPaid = 10,
                Abandoned = 11,
                Deleted = 12,
                AwaitingPayment = 13,
                SettlementInitiated = 14,
                SkipAddress = 15,
                Archived = 16,
                InProgress = 17
            }
            #endregion

            #region Properties
            public virtual string OrderNoPrefix
            {
                get
                {
                    if (string.IsNullOrEmpty(cOrderNoPrefix))
                    {
                        cOrderNoPrefix = moCartConfig["OrderNoPrefix"];
                    }
                    return cOrderNoPrefix;
                }
                set
                {
                    cOrderNoPrefix = value;
                }
            }


            #endregion

            #region Classes
            public class FormResult
            {

                public string Name;
                public string Value;
                public FormResult(string cName, string cValue)
                {
                    Name = cName;
                    Value = cValue;
                }
            }

            // Class Order

            // Private OrderElmt As XmlElement
            // Protected Friend myWeb As Cms
            // Public moConfig As System.Collections.Specialized.NameValueCollection
            // Private moServer As System.Web.HttpServerUtility

            // Dim nFirstPayment As Double
            // Dim nRepeatPayment As Double
            // Dim sRepeatInterval As String
            // Dim nRepeatLength As Integer
            // Dim bDelayStart As Boolean
            // Dim dStartDate As Date

            // Dim sPaymentMethod As String
            // Dim sTransactionRef As String
            // Dim sDescription As String

            // Dim sGivenName As String
            // Dim sBillingAddress1 As String
            // Dim sBillingAddress2 As String
            // Dim sBillingTown As String
            // Dim sBillingCounty As String
            // Dim sBillingPostcode As String
            // Dim sEmail As String

            // Public moPageXml As XmlDocument



            // Sub New(ByRef aWeb As Protean.Cms)
            // aWeb.PerfMon.Log("Order", "New")
            // myWeb = aWeb
            // moConfig = myWeb.moConfig
            // moPageXml = myWeb.moPageXml
            // moServer = aWeb.moCtx.Server
            // OrderElmt = moPageXml.CreateElement("Order")
            // End Sub

            // ReadOnly Property xml As XmlElement
            // Get
            // Return OrderElmt
            // End Get
            // End Property

            // Property PaymentMethod As String
            // Get
            // Return sPaymentMethod
            // End Get
            // Set(ByVal Value As String)
            // sPaymentMethod = Value
            // OrderElmt.SetAttribute("paymentMethod", Value)
            // End Set
            // End Property

            // Property firstPayment As Double
            // Get
            // Return nFirstPayment
            // End Get
            // Set(ByVal Value As Double)
            // nFirstPayment = Value
            // OrderElmt.SetAttribute("total", Value)
            // End Set
            // End Property

            // Property repeatPayment As Double
            // Get
            // Return nRepeatPayment
            // End Get
            // Set(ByVal Value As Double)
            // nRepeatPayment = Value
            // OrderElmt.SetAttribute("repeatPrice", Value)
            // End Set
            // End Property

            // Property repeatInterval As String
            // Get
            // Return sRepeatInterval
            // End Get
            // Set(ByVal Value As String)
            // sRepeatInterval = Value
            // OrderElmt.SetAttribute("repeatInterval", Value)
            // End Set
            // End Property

            // Property repeatLength As Integer
            // Get
            // Return nRepeatLength
            // End Get
            // Set(ByVal Value As Integer)
            // nRepeatLength = Value
            // OrderElmt.SetAttribute("repeatLength", Value)
            // End Set
            // End Property

            // Property delayStart As Boolean
            // Get
            // Return bDelayStart
            // End Get
            // Set(ByVal Value As Boolean)
            // bDelayStart = Value
            // If Value Then
            // OrderElmt.SetAttribute("delayStart", "true")
            // Else
            // OrderElmt.SetAttribute("delayStart", "false")
            // End If
            // End Set
            // End Property

            // Property startDate As Date
            // Get
            // Return dStartDate
            // End Get
            // Set(ByVal Value As Date)
            // dStartDate = Value
            // OrderElmt.SetAttribute("startDate", xmlDate(dStartDate))
            // End Set
            // End Property

            // Property TransactionRef As String
            // Get
            // Return sTransactionRef
            // End Get
            // Set(ByVal Value As String)
            // sTransactionRef = Value
            // OrderElmt.SetAttribute("transactionRef", sTransactionRef)
            // End Set
            // End Property


            // Property description As String
            // Get
            // Return sDescription
            // End Get
            // Set(ByVal Value As String)
            // sDescription = Value
            // Dim descElmt As XmlElement = moPageXml.CreateElement("Description")
            // descElmt.InnerText = sDescription
            // OrderElmt.AppendChild(descElmt)
            // End Set
            // End Property

            // Sub SetAddress(ByVal GivenName As String, ByVal Email As String, ByVal Telephone As String, ByVal TelephoneCountryCode As String, ByVal Company As String, ByVal Street As String, ByVal City As String, ByVal State As String, ByVal PostalCode As String, ByVal Country As String)

            // Dim addElmt As XmlElement = moPageXml.CreateElement("Contact")
            // addElmt.SetAttribute("type", "Billing Address")
            // xmlTools.addElement(addElmt, "GivenName", GivenName)
            // xmlTools.addElement(addElmt, "Email", Email)
            // xmlTools.addElement(addElmt, "Telephone", Telephone)
            // xmlTools.addElement(addElmt, "TelephoneCountryCode", TelephoneCountryCode)
            // xmlTools.addElement(addElmt, "Company", Company)
            // xmlTools.addElement(addElmt, "Street", Street)
            // xmlTools.addElement(addElmt, "City", City)
            // xmlTools.addElement(addElmt, "State", State)
            // xmlTools.addElement(addElmt, "PostalCode", PostalCode)
            // xmlTools.addElement(addElmt, "Country", Country)
            // OrderElmt.AppendChild(addElmt)
            // End Sub

            // End Class


            #endregion



            private string getProcessName(cartProcess cp)
            {
                switch (cp)
                {

                    case cartProcess.AwaitingPayment:
                        {
                            return "Awaiting Payment";
                        }
                    case cartProcess.SkipAddress:
                        {
                            return "SkipAddress";
                        }
                    case cartProcess.Billing:
                        {
                            return "Billing";
                        }
                    case cartProcess.Complete:
                        {
                            return "Complete";
                        }
                    case cartProcess.Confirmed:
                        {
                            return "Confirmed";
                        }
                    case cartProcess.Deleted:
                        {
                            return "Deleted";
                        }
                    case cartProcess.Delivery:
                        {
                            return "Delivery";
                        }
                    case cartProcess.DepositPaid:
                        {
                            return "Deposit Paid";
                        }
                    case cartProcess.Empty:
                        {
                            return "Empty";
                        }
                    case cartProcess.Failed:
                        {
                            return "Failed";
                        }
                    case cartProcess.PassForPayment:
                        {
                            return "Pass For Payment";
                        }
                    case cartProcess.Refunded:
                        {
                            return "Refunded";
                        }
                    case cartProcess.Shipped:
                        {
                            return "Shipped";
                        }
                    case cartProcess.Archived:
                        {
                            return "Archived";
                        }
                    case cartProcess.InProgress:
                        {
                            return "In Progress";
                        }

                    default:
                        {
                            return "Unknown Process ID";
                        }
                }
            }



            protected internal Cms myWeb;
            //This constructor is added for testing purpose
            public Cart()
            {
                mcCurrencySymbol = "£";
                mcCurrency = "GBP";
                mcCurrencyRef = "GBP";
                if (string.IsNullOrEmpty(mcCurrency))
                    mcCurrency = "GBP";
            }

            public Cart(ref Cms aWeb)
            {
                string cProcessInfo = "";
                try
                {
                    myWeb = aWeb;
                    myWeb.PerfMon.Log("Cart", "New");
                    moConfig = myWeb.moConfig;
                    moPageXml = myWeb.moPageXml;
                    moDBHelper = myWeb.moDbHelper;
                    InitializeVariables();
                    moServer = aWeb.moCtx.Server;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Close", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }
            }


            public string GetBlockCartUpdatesConfig()
            {

                string mcBlockCartUpdate = "";
                string paymentMethod = myWeb?.moSession?["mcPaymentMethod"] as string;

                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    Protean.Cms.Cart.PaymentProviders oEwProv = new Protean.Cms.Cart.PaymentProviders(ref myWeb);

                    XmlElement oProvider = oEwProv.GetValidPaymentProviders();
                    XmlNode oPaymentProviderCfg = oProvider.SelectSingleNode("provider[@name='" + paymentMethod + "']");


                    if (oPaymentProviderCfg != null)
                    {
                        XmlNode allowNode = oPaymentProviderCfg.SelectSingleNode("BlockCartUpdates");

                        if (allowNode != null && allowNode.Attributes["value"] != null)
                        {
                            mcBlockCartUpdate = allowNode.Attributes["value"].Value;
                        }
                    }
                }
                return mcBlockCartUpdate;
            }


            public void InitializeVariables()
            {
                myWeb.PerfMon.Log("Cart", "InitializeVariables");
                // Author:        Trevor Spink
                // Copyright:     Eonic Ltd 2006
                // Date:          2006-10-04

                // called at the beginning, whenever ewCart is run
                // sets the global variables and initialises the current cart

                string sSql = "";
                string cartXmlFromDatabase = "";
                mcOrderType = "Order";
                cOrderReference = "";
                mcModuleName = "Protean.Cart";

                string cProcessInfo = Convert.ToString(string.IsNullOrEmpty("initialise variables"));
                try
                {

                    if (moCartConfig != null)
                    {

                        if (myWeb.mnUserId > 0 & !string.IsNullOrEmpty(myWeb.moConfig["SecureMembershipAddress"]))
                        {
                            mcSiteURL = myWeb.moConfig["SecureMembershipAddress"] + moConfig["ProjectPath"] + "/";
                            mcCartURL = myWeb.moConfig["SecureMembershipAddress"] + moConfig["ProjectPath"] + "/";
                        }
                        else
                        {
                            mcSiteURL = moCartConfig["SiteURL"];
                            mcCartURL = moCartConfig["SecureURL"];
                        }

                        if ((myWeb.moRequest["ewCmd"]?.ToLower() ?? "") == "logoff")
                        {
                            EndSession();
                        }

                        var argaCart = this;
                        moDiscount = new Cart.Discount(ref argaCart);


                        mcPagePath = myWeb.mcPagePath;

                        if (string.IsNullOrEmpty(mcPagePath))
                        {
                            if (mcCartURL.EndsWith("/"))
                            {
                                mcPagePath = mcCartURL + "?";
                            }
                            else
                            {
                                mcPagePath = mcCartURL + "/?";
                            }
                        }
                        else
                        {
                            mcPagePath = mcCartURL.TrimEnd('/') + mcPagePath + "?";
                        }

                        if (moConfig["Membership"] == "on")
                            mbEwMembership = true;

                        mcMerchantEmail = moCartConfig["MerchantEmail"];
                        mcTermsAndConditions = moCartConfig["TermsAndConditions"];
                        // mcOrderNoPrefix = moCartConfig("OrderNoPrefix")
                        mcCurrencySymbol = moCartConfig["CurrencySymbol"];
                        mcCurrency = moCartConfig["Currency"];
                        mcCurrencyRef = moCartConfig["Currency"];
                        if (string.IsNullOrEmpty(mcCurrency))
                            mcCurrency = "GBP";

                        XmlNode moPaymentCfg;

                        // change currency based on language selection
                        if (!string.IsNullOrEmpty(myWeb.gcLang))
                        {
                            XmlNode moLangCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/languages");
                            if (moLangCfg != null)
                            {
                                XmlElement thisLangNode = (XmlElement)moLangCfg.SelectSingleNode("Language[@code='" + myWeb.gcLang + "']");
                                if (thisLangNode != null)
                                {
                                    mcCurrency = thisLangNode.GetAttribute("currency");
                                    mcCurrencyRef = thisLangNode.GetAttribute("currency");
                                    moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                                    XmlElement thisCurrencyNode = (XmlElement)moPaymentCfg.SelectSingleNode("currencies/Currency[@ref='" + mcCurrency + "']");
                                    mcCurrencySymbol = thisCurrencyNode.GetAttribute("symbol");
                                }
                            }
                        }

                        // change currency if default user currency is set
                        if (myWeb.mnUserId > 0)
                        {
                            XmlElement userxml = (XmlElement)myWeb.moPageXml.SelectSingleNode("/Page/User");
                            if (userxml is null)
                            {
                                userxml = myWeb.GetUserXML((long)myWeb.mnUserId);
                            }
                            if (!string.IsNullOrEmpty(userxml.GetAttribute("defaultCurrency")))
                            {
                                mcCurrency = userxml.GetAttribute("defaultCurrency");
                                mcCurrencyRef = userxml.GetAttribute("defaultCurrency");
                                moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                                XmlElement thisCurrencyNode = (XmlElement)moPaymentCfg.SelectSingleNode("currencies/Currency[@ref='" + mcCurrency + "']");
                                mcCurrencySymbol = thisCurrencyNode.GetAttribute("symbol");
                            }
                        }
                        moPaymentCfg = null;

                        // reset the currency on discounts
                        moDiscount.mcCurrency = mcCurrency;

                        if (moCartConfig["StockControl"] == "on")
                            mbStockControl = true;
                        if (moCartConfig["DisplayPrice"] == "off")
                            mbDisplayPrice = false;

                        mcDeposit = (moCartConfig["Deposit"]?.ToString() ?? "").ToLower();
                        mcDepositAmount = moCartConfig["DepositAmount"];
                        mcNotesXForm = moCartConfig["NotesXForm"];
                        mcBillingAddressXform = moCartConfig["BillingAddressXForm"];
                        mcDeliveryAddressXform = moCartConfig["DeliveryAddressXForm"];
                        if (moCartConfig["NoDeliveryAddress"] == "on")
                            mbNoDeliveryAddress = true;
                        mcMerchantEmailTemplatePath = moCartConfig["MerchantEmailTemplatePath"];
                        mcPriorityCountries = moCartConfig["PriorityCountries"];
                        mcPersistCart = moCartConfig["PersistCart"]; // might need to add checks for missing key
                        mcPaymentGetwayEmergencyMessage = moCartConfig["PaymentGetwayEmergencyMessage"];

                        if (mcPriorityCountries is null | string.IsNullOrEmpty(mcPriorityCountries))
                        {
                            mcPriorityCountries = "United Kingdom,United States";
                        }

                        mnTaxRate = Convert.ToDouble(moCartConfig["TaxRate"]);
                        if (myWeb.moSession != null)
                        {
                            if (myWeb.moSession["nTaxRate"] != null)
                            {
                                mnTaxRate = Convert.ToDouble("0" + myWeb.moSession["nTaxRate"] ?? "");
                            }
                        }
                        if (!string.IsNullOrEmpty(myWeb.moRequest.Form["url"]))
                        {
                            myWeb.moSession["returnPage"] = myWeb.moRequest.Form["url"];
                        }

                        if (!string.IsNullOrEmpty(myWeb.moRequest.QueryString["url"]))
                        {
                            myWeb.moSession["returnPage"] = myWeb.moRequest.QueryString["url"];
                        }
                        if (myWeb.moSession != null)
                        {
                            mcReturnPage = Convert.ToString(myWeb.moSession["returnPage"]);

                            if (myWeb.moSession["nEwUserId"] != null)
                            {
                                mnEwUserId = Convert.ToInt16((myWeb.moSession["nEwUserId"]?.ToString() ?? "0"));
                            }
                            else
                            {
                                mnEwUserId = 0;
                            }
                        }
                        else
                        {
                            mnEwUserId = 0;
                        }



                        if (myWeb.mnUserId > 0 & mnEwUserId == 0)
                            mnEwUserId = myWeb.mnUserId;
                        // MEMB - eEDIT
                        if ((myWeb.goApp["bFullCartOption"] as bool?) == true)
                        {
                            bFullCartOption = true;
                        }
                        else
                        {
                            bFullCartOption = false;
                        }
                        if (myWeb.moRequest.Form["cartId"] != null)
                        {
                            if ((myWeb.moSession["CartId"] as long?) != 0)
                            {
                                string CurrentCartId = myWeb.moRequest.Form["cartId"];
                                if ((CurrentCartId ?? "") != (Convert.ToString(myWeb.moSession["CartId"]) ?? ""))
                                {
                                    myWeb.moSession["CartId"] = Convert.ToString(CurrentCartId);
                                    mcReEstablishSession = "true";
                                }
                            }
                        }
                        string newCartId = Convert.ToString(myWeb.moSession["CartId"]);

                        if (myWeb.moSession["CartId"] is null)
                        {
                            mnCartId = 0;
                        }
                        else if (!Tools.Number.IsNumeric(myWeb.moSession["CartId"]) || ((myWeb.moSession["CartId"] as int?) <= 0))
                        {
                            mnCartId = 0;
                        }
                        else
                        {
                            mnCartId = Convert.ToInt64(myWeb.moSession["CartId"]) as long? ?? 0;
                        }

                        if (myWeb.moRequest["refSessionId"] != null)
                        {
                            mcSessionId = myWeb.moRequest["refSessionId"];
                            myWeb.moSession.Add("refSessionId", mcSessionId);
                        }
                        else if (myWeb.moSession["refSessionId"] != null)
                        {
                            mcSessionId = Convert.ToString(myWeb.moSession["refSessionId"]);
                        }
                        else
                        {
                            mcSessionId = myWeb.moSession.SessionID;
                        }
                        // session id is assigned
                        // add logic if same seession id is present or not in db if we have then generate diff session id

                        if (Tools.Number.IsNumeric(myWeb.moRequest.QueryString["cartErr"]))
                            mnProcessError = (short)Convert.ToInt16(myWeb.moRequest.QueryString["cartErr"]);

                        if (mbBlockCartCmd == false)
                        {
                            mcCartCmd = myWeb.moRequest.QueryString["cartCmd"];
                            if (string.IsNullOrEmpty(mcCartCmd))
                            {
                                mcCartCmd = myWeb.moRequest.Form["cartCmd"];
                            }
                        }
                        mcPaymentMethod = Convert.ToString(myWeb.moSession["mcPaymentMethod"]);
                        mmcOrderType = Convert.ToString(myWeb.moSession["mmcOrderType"]);
                        mcItemOrderType = myWeb.moRequest.Form["ordertype"];

                        // MsgBox "Item: " & mcItemOrderType & vbCrLf & "Order: " & mmcOrderType
                        // set global variable for submit button

                        mcSubmitText = myWeb.moRequest["submit"];

                        if (mnCartId > 0)
                        {
                            // cart exists
                            // turn off page caching
                            myWeb.bPageCache = false;

                            if (mcPersistCart == "on")
                            {
                                writeSessionCookie(); // write the cookie to persist the cart
                            }

                            if (!string.IsNullOrEmpty(mcReEstablishSession))
                            {
                                sSql = "select * from tblCartOrder where not(nCartStatus IN (6,9,13,14)) and nCartOrderKey = " + myWeb.moRequest["CartId"] + "And cCartSessionId Like '%" + mcReEstablishSession + "'";
                                // sSql = "select * from tblCartOrder where not(nCartStatus IN (6,9,13,14)) and nCartOrderKey = " + mnCartId + "And cCartSessionId Like '%" + mcSessionId + "'";
                            }
                            else
                            {
                                sSql = "select * from tblCartOrder where ((nCartStatus < 7 and not(cCartSessionId like 'OLD_%')) or nCartStatus IN (10,13,14)) and nCartOrderKey = " + mnCartId;
                            }

                            using (var oDr = moDBHelper.getDataReaderDisposable(sSql))
                            {
                                if (oDr.HasRows)
                                {
                                    while (oDr.Read())
                                    {
                                        mnGiftListId = Convert.ToInt16(oDr["nGiftListId"]);
                                        mnTaxRate = Convert.ToDouble(oDr["nTaxRate"]?.ToString() ?? "0");
                                        mnProcessId = (short)Convert.ToInt64(oDr["nCartStatus"]?.ToString() ?? "0");
                                        cartXmlFromDatabase = oDr["cCartXml"].ToString();
                                        // Check for deposit and earlier stages
                                        if (mcDeposit == "on")
                                        {
                                            if (!(oDr["nAmountReceived"] is DBNull))
                                            {
                                                if ((oDr["nAmountReceived"] as double? > 0) && mnProcessId < (int)cartProcess.Confirmed)
                                                {
                                                    mnProcessId = (short)cartProcess.SettlementInitiated;
                                                    moDBHelper.ExeProcessSql("update tblCartOrder set nCartStatus = '" + mnProcessId + "' where nCartOrderKey = " + mnCartId);
                                                }
                                            }
                                        }

                                    }
                                }

                                else
                                {
                                    // Cart no longer exists - a quit command has probably been issued.  Clear the session
                                    mnCartId = 0;
                                    mnProcessId = 0;
                                    mcCartCmd = "";
                                }
                            }
                            if (mnCartId == 0)
                            {
                                EndSession();
                            }
                        }
                        else
                        {
                            // -- Cart doesn't exist --

                            // check if we need to persist the cart
                            string cSessionFromSessionCookie = "";
                            if (mcPersistCart == "on")
                            {
                                string cSessionCookieName = "ewSession_" + myWeb.moSession.SessionID;
                                if (myWeb.moRequest.Cookies[cSessionCookieName] is null)
                                {
                                    writeSessionCookie();
                                }
                                else
                                {

                                    try
                                    {

                                        // get session ID from cookie IF session ID and current user if match
                                        // Dim cSessionCookieContents As String = myWeb.moRequest.Cookies("ewSession" & myWeb.mnUserId.ToString).Value
                                        string cSessionFromCookie = myWeb.moRequest.Cookies[cSessionCookieName].Value;
                                        // Dim nCartIdCheck As Integer = moDBHelper.ExeProcessSqlScalar("SELECT COUNT(nCartOrderKey) FROM tblCartOrder WHERE nCartUserDirId = " & cUserIdFromCookie.ToString & " AND cCartSessionId = '" & cSessionFromCookie & "'")

                                        if (!string.IsNullOrEmpty(cSessionFromCookie))
                                        {
                                            cSessionFromSessionCookie = cSessionFromCookie;
                                        }

                                        if (!string.IsNullOrEmpty(mcReEstablishSession))
                                        {
                                            cSessionFromSessionCookie = mcReEstablishSession;

                                        }
                                    }

                                    catch (Exception ex)
                                    {
                                        cProcessInfo = ex.Message;
                                    }

                                }
                            }

                            // check if the cart can be found in the database, although only run this check if we 
                            // know that we've visited the cart
                            // Also check out if this is coming from a Worldpay callback.
                            // Also check we need to udpate the session from the cookie
                            if (myWeb.moRequest["refSessionId"] != null | myWeb.moRequest["transStatus"] != null | myWeb.moRequest["settlementRef"] != null | !string.IsNullOrEmpty(cSessionFromSessionCookie))


                            {

                                if (myWeb.moRequest["transStatus"] != null)
                                {
                                    // add in check for session cookie
                                    sSql = "select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId  where o.cCartSchemaName='Order' and o.nCartOrderKey=" + myWeb.moRequest["cartId"] + " and DATEDIFF(hh,a.dInsertDate,GETDATE())<24";
                                    mcPaymentMethod = "WorldPay";
                                }
                                else if (myWeb.moRequest["settlementRef"] != null)
                                {
                                    // Go get the cart, restore settings
                                    sSql = "select * from tblCartOrder where cCartSchemaName='Order' and cSettlementID='" + myWeb.moRequest["settlementRef"] + "'";
                                }
                                else
                                {
                                    // get session id from ewSession cookie

                                    if (!string.IsNullOrEmpty(cSessionFromSessionCookie)) // myWeb.mnUserId > 0 And
                                    {
                                        mcSessionId = cSessionFromSessionCookie;
                                        cSessionFromSessionCookie = "";
                                    }
                                    if (mnCartId > 0)
                                    {
                                        sSql = "select Top 1 * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId " +
                                               "where o.cCartSchemaName='Order' and o.cCartSessionId = '" + SqlFmt(mcSessionId) +
                                               "' and o.nCartOrderKey='" + mnCartId + "' order by o.nCartOrderKey desc";
                                    }
                                    else
                                    {
                                        sSql = "select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId " +
                                               "where o.cCartSchemaName='Order' and o.cCartSessionId = '" + SqlFmt(mcSessionId) + "'";
                                        // logic needed here to check cart id; if we have a cart id, then pull with session id
                                    }

                                }

                                myWeb.PerfMon.Log("Cart", "InitializeVariables - check for cart start");
                                using (var oDr = moDBHelper.getDataReaderDisposable(sSql))
                                {
                                    myWeb.PerfMon.Log("Cart", "InitializeVariables - check for cart end");

                                    if (oDr.HasRows)
                                    {
                                        while (oDr.Read())
                                        {
                                            mnGiftListId = Convert.ToInt16(oDr["nGiftListId"]);
                                            mnCartId = Convert.ToInt32(oDr["nCartOrderKey"]); // get cart id
                                            mnProcessId = Convert.ToInt16(oDr["nCartStatus"]); // get cart status
                                            mnTaxRate = Convert.ToDouble(oDr["nTaxRate"]);
                                            if (myWeb.moRequest["settlementRef"] != null | myWeb.moRequest["settlementRef"] != null)
                                            {

                                                // Set to a holding state that indicates that the settlement has been initiated
                                                // mnProcessId = cartProcess.SettlementInitiated

                                                // If a cart has been found, we need to update the session ID in it.
                                                if ((oDr["cCartSessionId"]?.ToString() ?? "") != mcSessionId)
                                                {
                                                    moDBHelper.ExeProcessSql("update tblCartOrder set cCartSessionId = '" + mcSessionId + "' where nCartOrderKey = " + mnCartId);
                                                    // if mnCartId is not null then pull both otherwise pull session id
                                                }

                                                // Reactivate the order in the database
                                                moDBHelper.ExeProcessSql("update tblCartOrder set nCartStatus = '" + mnProcessId + "' where nCartOrderKey = " + mnCartId);

                                            }
                                            if (mnProcessId > 5 & mnProcessId != (int)cartProcess.SettlementInitiated & mnProcessId != (int)cartProcess.DepositPaid)
                                            {
                                                // Cart has passed a status of "Succeeded" - we can't do anything to this cart. Clear the session.
                                                EndSession();
                                                mnCartId = 0;
                                                mnTaxRate = default;
                                                mnProcessId = 0;
                                                mcCartCmd = "";
                                            }
                                            mcCurrencyRef = Convert.ToString(oDr["cCurrency"]);
                                            cartXmlFromDatabase = oDr["cCartXml"].ToString();
                                        }
                                    }
                                }

                            }

                        }



                        // Load the cart xml from the database for passing on transitory variables.
                        if (mnCartId > 0 && !string.IsNullOrEmpty(cartXmlFromDatabase))
                        {
                            // This is used to load in variables from the xml that is saved to the database.
                            // Much more for transitory data that may need to be stored across sessions - e.g. from site to secure site.
                            XmlElement cartXmlFromLoad = null;
                            cartXmlFromLoad = myWeb.moPageXml.CreateElement("Load");
                            cartXmlFromLoad.InnerXml = cartXmlFromDatabase;
                            if (cartXmlFromLoad.SelectSingleNode("Order") != null)
                            {
                                cartXmlFromLoad = (XmlElement)cartXmlFromLoad.SelectSingleNode("Order");
                                if (!string.IsNullOrEmpty(cartXmlFromLoad.GetAttribute("promocodeFromExternalRef")))
                                {
                                    promocodeFromExternalRef = cartXmlFromLoad.GetAttribute("promocodeFromExternalRef");
                                }
                            }
                            cartXmlFromLoad = null;
                        }

                        // Check for promo code from request object
                        if (!string.IsNullOrEmpty(myWeb.moRequest["promocode"]) && myWeb.moSession != null)
                        {

                            // This has been requested - update Session
                            myWeb.moSession["promocode"] = myWeb.moRequest["promocode"].ToString();
                            promocodeFromExternalRef = myWeb.moRequest["promocode"].ToString();
                        }

                        else if (myWeb.moSession != null && !string.IsNullOrEmpty(myWeb.moSession["promocode"] as string))
                        {
                            // Set the value from the session.
                            promocodeFromExternalRef = myWeb.moSession["promocode"].ToString();

                        }

                        mbVatAtUnit = (moCartConfig["VatAtUnit"]?.ToString().ToLower() == "yes" || moCartConfig["VatAtUnit"]?.ToString().ToLower() == "on");
                        mbVatOnLine = (moCartConfig["VatOnLine"]?.ToString().ToLower() == "yes" || moCartConfig["VatOnLine"]?.ToString().ToLower() == "on");

                        mbRoundup = (moCartConfig["Roundup"]?.ToString().ToLower() == "yes" || moCartConfig["Roundup"]?.ToString().ToLower() == "on");
                        mbRoundDown = (moCartConfig["Roundup"]?.ToString().ToLower() == "down");

                        mbDiscountsOn = (moCartConfig["Discounts"]?.ToString().ToLower() == "yes" || moCartConfig["Discounts"]?.ToString().ToLower() == "on");
                        mbOveridePrice = (moCartConfig["OveridePrice"]?.ToString().ToLower() == "yes" || moCartConfig["OveridePrice"]?.ToString().ToLower() == "on");

                        if (string.IsNullOrEmpty(mcCurrencyRef))
                            mcCurrencyRef = Convert.ToString(myWeb.moSession["cCurrency"]);
                        if (string.IsNullOrEmpty(mcCurrencyRef) | mcCurrencyRef is null)
                            mcCurrencyRef = moCartConfig["currencyRef"]; // Setting Deprecated
                        if (string.IsNullOrEmpty(mcCurrencyRef) | mcCurrencyRef is null)
                            mcCurrencyRef = moCartConfig["DefaultCurrencyOveride"];
                        GetCurrencyDefinition();

                        // try grabbing a userid if we dont have one but have a cart
                        if (myWeb.mnUserId == 0 & mnCartId > 0)
                        {
                            sSql = "SELECT nCartUserDirId FROM tblCartOrder WHERE nCartOrderKey = " + mnCartId;
                            string cRes = moDBHelper.ExeProcessSqlScalar(sSql);

                            if (Tools.Number.IsNumeric(cRes) && Convert.ToDouble(cRes) > 0d)
                            {
                                myWeb.mnUserId = Convert.ToInt64(cRes);
                                mnEwUserId = myWeb.mnUserId;
                                myWeb.moSession["nUserId"] = cRes;

                                string cRequestPage = myWeb.moRequest["pgid"];
                                if (Tools.Number.IsNumeric(cRequestPage) && Convert.ToDouble(cRequestPage) > 0d)
                                {
                                    myWeb.mnPageId = Convert.ToInt64(myWeb.moRequest["pgid"]);
                                }
                            }
                        }

                        // Behavioural Tweaks
                        if (mnProcessId == (int)cartProcess.SettlementInitiated)
                        {
                            mbRedirectSecure = true;
                        }

                        if (moConfig["Subscriptions"] == "on")
                        {
                            var argaCart1 = this;
                            moSubscription = new Cart.Subscriptions(ref argaCart1);
                        }
                        // If Not moDiscount Is Nothing Then
                        // moDiscount.bHasPromotionalDiscounts = myWeb.moSession("bHasPromotionalDiscounts")
                        // End If

                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "InitializeVariables", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }
            }


            public void writeSessionCookie()
            {
                // writes the session cookie to persist the cart
                if (mcPersistCart == "on")
                {
                    // Use session ID instead of user ID for cookie name
                    string cookieName = "ewSession_" + myWeb.moSession.SessionID;
                    var cookieEwSession = new System.Web.HttpCookie(cookieName);
                    cookieEwSession.Value = mcSessionId.ToString();
                    cookieEwSession.Expires = DateTime.Now.AddMonths(1);
                    myWeb.moResponse.Cookies.Add(cookieEwSession);
                }
            }

            private void clearSessionCookie()
            {

                string cSessionCookieName = "ewSession_" + myWeb.moSession.SessionID;

                if (myWeb.moResponse.Cookies[cSessionCookieName] != null)
                {
                    // clear ewSession cookie so cart doesn't get persisted
                    // we don't need to check for mcPersistCart = "on"
                    var cookieEwSession = new System.Web.HttpCookie(cSessionCookieName);
                    cookieEwSession.Expires = DateTime.Now.AddDays(-1);
                    myWeb.moResponse.Cookies.Add(cookieEwSession);
                    cookieEwSession.Expires = DateTime.Now.AddMonths(1);
                }
            }


            public virtual void PersistVariables()
            {
                myWeb.PerfMon.Log("Cart", "PersistVariables");
                // Author:        Trevor Spink
                // Copyright:     Eonic Ltd 2003
                // Date:          2003-02-01

                // called at the end of the main procedure (ewCartPlugin())
                // holds variable values after cart module ends for use next time it starts
                // they are stored in either a session attribute or in the database

                string sSql;
                string cProcessInfo = "";
                try
                {

                    cProcessInfo = "set session variables"; // persist global sProcessInfo
                    if (myWeb.moSession != null)
                    {
                        if (myWeb.moSession["CartId"] is null)
                        {
                            myWeb.moSession.Add("CartId", mnCartId.ToString());
                        }
                        else
                        {
                            if (mnCartId > 0)
                            {
                                myWeb.moSession["CartId"] = mnCartId.ToString();
                            }
                        }
                        // oResponse.Cookies(mcSiteURL & "CartId").Domain = mcSiteURL
                        // oSession("nCartOrderId") = mnCartId    '   session attribute holds Cart ID
                    }

                    if (mnCartId > 0)
                    {
                        // Only update the process if less than 6 we don't ever want to change the status of a completed order other than within the admin system. Boo Yah!
                        int currentStatus = Convert.ToInt16(moDBHelper.ExeProcessSqlScalar("select nCartStatus from tblCartOrder where nCartOrderKey = " + mnCartId));
                        if (currentStatus < 6 | currentStatus == 10 & mnProcessId == 6)
                        {
                            // If we have a cart, update its status in the db
                            if (mnProcessId != currentStatus)
                            {
                                sSql = "update tblCartOrder set nCartStatus = " + mnProcessId + ", nGiftListId = " + mnGiftListId + ", nCartUserDirId = " + myWeb.mnUserId + " where nCartOrderKey = " + mnCartId;
                            }
                            else
                            {
                                sSql = "update tblCartOrder set nGiftListId = " + mnGiftListId + ", nCartUserDirId = " + myWeb.mnUserId + " where nCartOrderKey = " + mnCartId;
                            }
                            moDBHelper.ExeProcessSql(sSql);
                        }


                    }

                    if (myWeb.moSession != null)
                    {
                        myWeb.moSession["nProcessId"] = (object)mnProcessId; // persist global mnProcessId
                        myWeb.moSession["mcPaymentMethod"] = mcPaymentMethod;
                        myWeb.moSession["mmcOrderType"] = mmcOrderType;
                        myWeb.moSession["nTaxRate"] = (object)mnTaxRate;
                        if (moDiscount != null)
                        {
                            myWeb.moSession["bHasPromotionalDiscounts"] = (object)moDiscount.bHasPromotionalDiscounts;
                        }
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "PersistVariables", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }
            }

            public virtual void checkButtons()
            {
                myWeb.PerfMon.Log("Cart", "checkButtons");
                string cProcessInfo = "";
                try
                {

                    // if we have set mcCartCmdAlt then that overides the button.
                    if (!string.IsNullOrEmpty(mcCartCmdAlt))
                    {
                        mcCartCmd = mcCartCmdAlt;
                        return;
                    }

                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartAdd"))
                    {
                        mcCartCmd = "Add";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartAddDeposit"))
                    {
                        mcCartCmd = "Add";
                        mbDepositOnly = true;
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartDetail"))
                    {
                        mcCartCmd = "Cart";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartProceed"))
                    {
                        mcCartCmd = "RedirectSecure";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartNotes"))
                    {
                        mcCartCmd = "Notes";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartUpdate"))
                    {
                        mcCartCmd = "Update";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartLogon"))
                    {
                        mcCartCmd = "Logon";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartRegister"))
                    {
                        mcCartCmd = "Logon";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartQuit"))
                    {
                        mcCartCmd = "Quit";
                    }
                    // Continue shopping
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartBrief"))
                    {
                        mcCartCmd = "Brief";
                    }
                    // Pick Address Buttions
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartBillAddress") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartBillcontact") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartBilladdNewAddress") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartBilleditAddress"))
                    {
                        mcCartCmd = "Billing";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartDelAddress") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartDelcontact") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartDeladdNewAddress") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "cartDeleditAddress"))
                    {
                        mcCartCmd = "Delivery";
                    }
                }

                // legacy button handling looking at button values rather than names, should not be required soon
                // Select Case mcSubmitText
                // Case "Goto Checkout", "Go To Checkout"
                // updateCart("RedirectSecure")
                // mcCartCmd = "RedirectSecure"
                // Case "Edit Billing Details", "Proceed without Logon"
                // mcCartCmd = "Billing"
                // Case "Edit Delivery Details"
                // mcCartCmd = "Delivery"
                // Case "Confirm Order", "Proceed with Order", "Proceed"
                // updateCart("ChoosePaymentShippingOption")
                // Case "Update Cart", "Update Order"
                // updateCart("Cart")
                // Case "Empty Cart", "Empty Order"
                // mcCartCmd = "Quit"
                // Case "Make Secure Payment"
                // updateCart(mcCartCmd)
                // Case "Continue Shopping"
                // mcCartCmd = "BackToSite"
                // End Select

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "checkButtons", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }

            }

            public XmlElement CreateCartElement(XmlDocument oCartXML)
            {
                XmlElement oContentElmt;
                XmlElement oElmt;

                try
                {
                    oContentElmt = oCartXML.CreateElement("Cart");
                    oContentElmt.SetAttribute("type", "order");
                    oContentElmt.SetAttribute("currency", mcCurrency);
                    oContentElmt.SetAttribute("currencySymbol", mcCurrencySymbol);
                    if (!mbDisplayPrice)
                        oContentElmt.SetAttribute("displayPrice", mbDisplayPrice.ToString());
                    oElmt = oCartXML.CreateElement("Order");
                    oContentElmt.AppendChild(oElmt);

                    if (!string.IsNullOrEmpty(mcPaymentGetwayEmergencyMessage))
                    {
                        XmlElement emergencyMessageElmt = oCartXML.CreateElement("PaymentGetwayEmergencyMessage");
                        emergencyMessageElmt.InnerXml = mcPaymentGetwayEmergencyMessage;
                        oContentElmt.AppendChild(emergencyMessageElmt);
                    }

                    moCartXml = oContentElmt;

                    return oContentElmt;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "apply", ex, myWeb.moCtx, "", "CreateCartElement", gbDebug);
                    return null;
                }

            }

            public virtual void apply()
            {
                myWeb.PerfMon.Log("Cart", "apply");
                // this function is the main function.

                var oCartXML = moPageXml;
                XmlElement oContentElmt;
                XmlElement oElmt;
                // Dim oPickContactXForm As xForm
                string cProcessInfo = "";
                var bRedirect = default(bool);
                string cRepeatPaymentError = "";

                try
                {

                    // myWeb.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "Start1 CALLBACK : " & mnProcessId & mcCartCmd)

                    oContentElmt = (XmlElement)CreateCartElement(oCartXML);
                    oElmt = (XmlElement)oContentElmt.FirstChild;
                    // if the cartCmd is not on a link but on a button
                    // we need to set the cartCmd dependant upon the button name

                    // TS for OpenQuote allow cart step to be set before checking against button allows apply to be called again
                    if (string.IsNullOrEmpty(mcCartCmd))
                    {
                    }
                    checkButtons();

                    // Cart Command overrides
                    if (mbRedirectSecure | mnProcessId == (int)cartProcess.SettlementInitiated)
                    {
                        mcCartCmd = "RedirectSecure";
                    }

                    // Secure Trading CallBack override
                    if (!string.IsNullOrEmpty(mcCartCmd))
                    {
                        if (mcCartCmd.StartsWith("SecureTradingReturn"))
                            mcCartCmd = "SubmitPaymentDetails";
                    }

                    if (!(mnCartId > 0))
                    {
                        // Cart doesn't exist - if the process flow has a valid command (except add or quit), then this is an error
                        switch (mcCartCmd ?? "")
                        {
                            case "Cart":
                                {
                                    mcCartCmd = "CartEmpty";
                                    break;
                                }
                            case "Logon":
                            case "Remove":
                            case "Notes":
                            case "Billing":
                            case "Delivery":
                            case "ChoosePaymentShippingOption":
                            case "Confirm":
                            case "EnterPaymentDetails":
                            case "SubmitPaymentDetails":
                            case "ShowInvoice":
                            case "ShowCallBackInvoice":
                                {
                                    mcCartCmd = "CookiesDisabled";
                                    break;
                                }
                            case "Error":
                                {
                                    mcCartCmd = "Error";
                                    break;
                                }
                        }

                    }

                // Cart Process

                processFlow:
                    ;

                    // myWeb.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "Start2 CALLBACK : " & mnProcessId & mcCartCmd)

                    // user can't change things if we are to show the invoice
                    if (mnProcessId == (int)cartProcess.Complete & mcCartCmd != "Quit" & mcCartCmd != "ShowCallBackInvoice")
                        mcCartCmd = "ShowInvoice";

                    cProcessInfo = cProcessInfo + (string.IsNullOrEmpty(mcCartCmd) ? "" : ", ") + mcCartCmd;
                    if (!string.IsNullOrEmpty(mcCartCmd))
                    {
                        // ensure the client is not able to hit the back button and go back to the page without refreshing.
                        // This should resolve some rare random errors in the cart process.
                        myWeb.mbSetNoBrowserCache = true;
                    }

                    switch (mcCartCmd ?? "")
                    {

                        case "Update":
                            {
                                string argcSuccessfulCartCmd = "Currency";
                                mcCartCmd = Convert.ToString(updateCart(ref argcSuccessfulCartCmd));
                                goto processFlow;
                            }
                        case "Remove": // take away an item and set the command to display the cart
                            {
                                if (RemoveItem() > 0)
                                {
                                    mcCartCmd = "Currency";
                                }
                                else
                                {
                                    // RemoveItem has removed the last item in the cart - quit the cart.
                                    mcCartCmd = "Quit";
                                }
                                goto processFlow;
                            }

                        case "Add": // add an item to the cart, if its a new cart we must initialise it and change its status
                            {
                                long qtyAdded = 0L;
                                var nQuantity = default(long);
                                // Check we are adding a quantity (we need to catch any adds that don't have a specified quantity and create empty carts)
                                foreach (string oItem1 in myWeb.moRequest.Form) // Loop for getting products/quants
                                {
                                    if ((oItem1?.ToString() ?? "").StartsWith("qty_"))
                                    {
                                        if (Tools.Number.IsNumeric(myWeb.moRequest.Form.Get(oItem1)))
                                        {
                                            nQuantity = Convert.ToInt64(myWeb.moRequest.Form.Get(oItem1));
                                        }

                                        // replacementName
                                        if (nQuantity > 0L)
                                        {
                                            qtyAdded = qtyAdded + nQuantity;
                                        } // end check for previously added
                                    } // end check for item/quant
                                } // End Loop for getting products/quants

                                if (mnCartId < 1 & qtyAdded > 0L)
                                {
                                    CreateNewCart(ref oElmt);
                                    if (!string.IsNullOrEmpty(mcItemOrderType))
                                    {
                                        mmcOrderType = mcItemOrderType;
                                    }
                                    else
                                    {
                                        mmcOrderType = "";
                                    }
                                    mnProcessId = 1;
                                }

                                if (qtyAdded > 0L & mnCartId > 0)
                                {
                                    if (!AddItems())
                                    {
                                        mnProcessError = 2; // Error: The current item's order type does not match the cart's order type
                                        mcCartCmd = "Error";
                                        goto processFlow;
                                    }
                                    else
                                    {
                                        // Case for if a items have been added from a giftlist
                                        if (myWeb.moRequest["giftlistId"] != null)
                                        {
                                            this.AddDeliveryFromGiftList(myWeb.moRequest["giftlistId"]);
                                        }
                                        mcCartCmd = "Currency";
                                        goto processFlow;
                                    }
                                }

                                if (qtyAdded > 0L & mnCartId == 0)
                                {
                                    mnProcessError = 1; // Error: Cookies Disabled
                                    mcCartCmd = "Error";
                                    goto processFlow;
                                }

                                break;
                            }


                        // here is where we should check cart for subscriptions


                        case "Currency":
                            {

                                if (SelectCurrency())
                                {
                                    if (mcCartCmd == "Cart")
                                    {
                                        AddBehaviour();
                                    }
                                    goto processFlow;
                                }

                                break;
                            }

                        case "Quit":
                            {
                                // action depends on whether order is complete or not
                                if (mnProcessId == 6 | mnProcessId == 10)
                                {
                                    // QuitCart()
                                    EndSession();
                                    mcCartCmd = "";
                                    mnCartId = 0;
                                    mnProcessId = 0;
                                }
                                else
                                {

                                    clearSessionCookie();
                                    QuitCart();
                                    EndSession();
                                    mnProcessId = 0;
                                    if (bFullCartOption == true)
                                    {
                                        GetCart(ref oElmt);
                                    }
                                    else
                                    {
                                        GetCartSummary(ref oElmt);
                                    }
                                    mnCartId = 0;
                                }
                                // return to site
                                bRedirect = true;
                                // hack for assure so we can choose not to redirect on quit
                                if (myWeb.moRequest["redirect"] != "false")
                                {
                                    if (mcReturnPage is null)
                                        mcReturnPage = "";
                                    if (myWeb.moRequest["redirect"] != null)
                                    {
                                        if (myWeb.moRequest["redirect"].StartsWith("/"))
                                            mcReturnPage = myWeb.moRequest["redirect"];
                                    }
                                    myWeb.msRedirectOnEnd =
     mcSiteURL + mcReturnPage +
     ((mcSiteURL + mcReturnPage).Contains("?") ? "&" : "?") +
     "cartCmd=finish";
                                }

                                break;
                            }

                        case "Error":
                            {
                                GetCart(ref oElmt);
                                break;
                            }

                        case "Cart": // Choose Shipping Costs
                            {

                                // If mnProcessId > 3 Then
                                // ' when everything is ready we can show the invoice screen
                                // mcCartCmd = "Confirm"
                                // GoTo processFlow '   execute next step (make the payment)
                                // End If

                                // info to display the cart
                                GetCart(ref oElmt);
                                if (Convert.ToString(oElmt.Attributes["statusId"].Value) == "6")
                                {
                                    mnProcessId = 6;
                                    // addDateAndRef(ref oElmt);
                                    // purchaseActions(oContentElmt,true);
                                    mcCartCmd = "ShowInvoice";
                                    goto processFlow;
                                }
                                GetWalletDetails(ref oElmt);
                                break;
                            }

                        case "Discounts":
                            {

                                mcCartCmd = discountsProcess(oElmt);
                                if (mcCartCmd != "Discounts")
                                {
                                    goto processFlow;
                                }

                                break;
                            }


                        case "RedirectSecure":
                        case "Settlement":
                            {
                                string cRedirectCommand;
                                // Set a Session variable flag to 
                                myWeb.moSession.Add("CartIsOn", "true");
                                bRedirect = true;
                                if (myWeb.moRequest["settlementRef"] is null)
                                {
                                    cRedirectCommand = "Logon";
                                }
                                else
                                {
                                    cRedirectCommand = "ChoosePaymentShippingOption";
                                }

                                // If a settlement has been initiated, then update the process
                                if (mnProcessId == (int)cartProcess.DepositPaid)
                                {

                                    myWeb.moSession["Settlement"] = "true";

                                    // mnProcessId = cartProcess.PassForPayment
                                    // moDBHelper.ExeProcessSql("update tblCartOrder set nCartStatus = '" & mnProcessId & "' where nCartOrderKey = " & mnCartId)

                                    // pickup any google tracking code.
                                }
                                string cGoogleTrackingCode = "";

                                foreach (var item in myWeb.moRequest.QueryString)
                                {
                                    // form needs to have this <form method="post" action="http://www.thissite.com" id="cart" onsubmit="pageTracker._linkByPost(this)">
                                    // the action URL is important
                                    // each querystring item in the google tracking code start with __utm
                                    if ((item?.ToString() ?? "").StartsWith("__utm"))
                                    {
                                        cGoogleTrackingCode = cGoogleTrackingCode + "&" + Convert.ToString(item) + "=" + myWeb.moRequest.QueryString[Convert.ToString(item)];
                                    }
                                }
                                if (mnCartId > 0)
                                {
                                    myWeb.msRedirectOnEnd = myWeb.mcOriginalURL.Split('?')[0] + "?cartCmd=" + cRedirectCommand + "&refSessionId=" + mcSessionId + cGoogleTrackingCode;
                                }
                                else
                                {
                                    mnProcessError = -1;
                                    GetCart(ref oElmt);
                                }
                                break;
                            }

                        // myWeb.moResponse.Redirect(mcPagePath & "cartCmd=" & cRedirectCommand & "&refSessionId=" & mcSessionId & cGoogleTrackingCode)
                        case "Archive":
                            {
                                mnProcessId = (short)cartProcess.Archived;
                                moDBHelper.ExeProcessSql("update tblCartOrder set nCartStatus = '" + mnProcessId + "' where nCartOrderKey = " + mnCartId);

                                clearSessionCookie();
                                QuitCart();
                                EndSession();

                                if (mcReturnPage is null)
                                    mcReturnPage = "";

                                myWeb.msRedirectOnEnd = mcSiteURL + mcReturnPage + ((mcSiteURL + mcReturnPage).Contains("?") ? "&" : "?") + "cartCmd=finish";
                                break;

                            }

                        case "Logon":
                        case "LogonSubs": // offer the user the ability to logon / register
                            {
                                bool bSkipLogon = false;
                                if (moCartConfig["SkipLogon"] == "on")
                                {
                                    bSkipLogon = true;
                                }

                                if (mbEwMembership == true && (bSkipLogon == false || mcCartCmd == "LogonSubs"))
                                {

                                    // logon xform !!! We disable this because it is being brought in allready by .Web
                                    if (myWeb.mnUserId == 0)
                                    {
                                        // addtional string for membership to check
                                        myWeb.moSession["cLogonCmd"] = "cartCmd=Logon";
                                        // registration xform
                                        Cms argmyWeb = myWeb;
                                        Protean.Providers.Membership.ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                        IMembershipProvider oMembershipProv = RetProv.Get(ref argmyWeb, this.moConfig["MembershipProvider"]);

                                        myWeb = (Cms)argmyWeb;
                                        IMembershipAdminXforms oRegXform = oMembershipProv.AdminXforms;
                                        oRegXform.open(moPageXml);
                                        XmlElement argIntanceAppend = null;
                                        oRegXform.xFrmEditDirectoryItem(IntanceAppend: ref argIntanceAppend, myWeb.mnUserId, "User", Convert.ToInt64("0" + moCartConfig["DefaultSubscriptionGroupId"]), "CartRegistration");
                                        if (oRegXform.valid)
                                        {
                                            string sReturn = moDBHelper.validateUser(myWeb.moRequest["cDirName"], myWeb.moRequest["cDirPassword"]);
                                            if (Tools.Number.IsNumeric(sReturn))
                                            {
                                                myWeb.mnUserId = (int)Convert.ToInt64(sReturn);
                                                var oUserElmt = moDBHelper.GetUserXML(myWeb.mnUserId);

                                                var oMembership = new Membership(ref myWeb);
                                                oMembership.RegistrationActions();

                                                moPageXml.DocumentElement.AppendChild(oUserElmt);
                                                myWeb.moSession["nUserId"] = (object)myWeb.mnUserId;
                                                mcCartCmd = "Notes";
                                                goto processFlow;
                                            }
                                            else
                                            {
                                                oRegXform.addNote(oRegXform.moXformElmt.FirstChild.ToString(), Protean.xForm.noteTypes.Alert, sReturn);
                                                moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt);
                                            }
                                        }
                                        else
                                        {
                                            moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt);
                                            GetCart(ref oElmt);
                                        }


                                        oRegXform = null;
                                    }
                                    else
                                    {
                                        mcCartCmd = "Notes";
                                        goto processFlow;
                                    }
                                }
                                else
                                {
                                    mcCartCmd = "Notes";
                                    goto processFlow;
                                }

                                break;
                            }
                        case "Notes":
                            {

                                mcCartCmd = notesProcess(oElmt);
                                if (mcCartCmd != "Notes")
                                {
                                    goto processFlow;
                                }

                                break;
                            }

                        case "SkipAddress": // Check if order has Billing Address                
                            {

                                if (usePreviousAddress(ref oElmt) == false)
                                {
                                    mcCartCmd = "Billing";
                                }
                                if (mcCartCmd != "SkipAddress")
                                {
                                    goto processFlow;
                                }

                                break;
                            }

                        case "Billing": // Check if order has Billing Address    
                            {
                                // reset payment - TS this was commented out and I am not sure why, I have put back in as of 11-03-22
                                mcPaymentMethod = null;
                                myWeb.moSession["mcPaymentMethod"] = (object)null;

                                // GetCart(oElmt)
                                addressSubProcess(ref oElmt, "Billing Address");
                                GetCart(ref oElmt);
                                if (mcCartCmd != "Billing")
                                {
                                    goto processFlow;
                                }

                                break;
                            }

                        case "Delivery": // Check if order needs a Delivery Address
                            {

                                addressSubProcess(ref oElmt, "Delivery Address");
                                GetCart(ref oElmt);
                                if (mcCartCmd != "Delivery")
                                {
                                    goto processFlow;
                                }

                                break;
                            }

                        case "ChoosePaymentShippingOption":
                        case "Confirm":  // and confirm terms and conditions
                            {
                                mnProcessId = 4;

                                GetCart(ref oElmt);

                                if (mcCartCmd == "ChoosePaymentShippingOption")
                                {
                                    if (oContentElmt != null)
                                    {
                                        AddToLists("Quote", ref oContentElmt);
                                    }
                                }


                                if (!string.IsNullOrEmpty(mcPaymentMethod) & moCartXml.SelectSingleNode("Order/Shipping") != null)
                                {
                                    mnProcessId = 5;
                                    mcCartCmd = "EnterPaymentDetails";
                                    // execute next step unless form filled out wrong / not in db
                                    goto processFlow;
                                }
                                else
                                {
                                    var oOptionXform = optionsXform(ref oElmt);
                                    if (oOptionXform.valid)
                                    {
                                        if (myWeb.moSession["paymentRecieved"] != null)
                                        {
                                            string sPaymentId = myWeb.moSession["paymentRecieved"].ToString();
                                            mnPaymentId = Int32.Parse(sPaymentId);
                                            myWeb.moSession["paymentRecieved"] = null;
                                            mnProcessId = (short)cartProcess.Complete;
                                            mcCartCmd = "ShowInvoice";
                                            goto processFlow;
                                        }
                                        else
                                        {
                                            mnProcessId = 5;
                                            mcCartCmd = "EnterPaymentDetails";
                                            // execute next step unless form filled out wrong / not in db
                                            goto processFlow;
                                        }
                                    }
                                    else
                                    {
                                        XmlElement oContentsElmt = (XmlElement)moPageXml.SelectSingleNode("/Page/Contents");
                                        if (oContentsElmt is null)
                                        {
                                            oContentsElmt = moPageXml.CreateElement("Contents");
                                            if (moPageXml.DocumentElement is null)
                                            {
                                                throw new Exception("PAGE IS NOT CREATED");
                                            }
                                            else
                                            {
                                                moPageXml.DocumentElement.AppendChild(oContentsElmt);
                                            }
                                        }
                                        oContentsElmt.AppendChild(oOptionXform.moXformElmt);

                                        // moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oOptionXform.moXformElmt)

                                        if (!string.IsNullOrEmpty(cRepeatPaymentError))
                                        {
                                            var argoNode = oOptionXform.moXformElmt.SelectSingleNode("group");
                                            oOptionXform.addNote(ref argoNode, Protean.xForm.noteTypes.Alert, cRepeatPaymentError, true);
                                        }
                                    }
                                }

                                break;
                            }


                        case "Redirect3ds":
                            {
                                if (!string.IsNullOrEmpty(myWeb.moRequest["PaymentMethod"]))
                                {
                                    // ts added because missing from session and now added to the return redirect from SagePay (may need to add to paypal pro too)
                                    mcPaymentMethod = myWeb.moRequest["PaymentMethod"];
                                }
                                // Dim oEwProv As Protean.Cms.Cart.PaymentProviders = New PaymentProviders(myWeb)
                                // Dim Redirect3dsXform As xForm = New xForm(myWeb.msException)
                                // Redirect3dsXform = oEwProv.GetRedirect3dsForm(myWeb)

                                Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                                IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, mcPaymentMethod);
                                // var oPayProv = new Providers.Payment.ReturnProvider(ref myWeb, mcPaymentMethod);
                                var Redirect3dsXform = new Cms.xForm(ref myWeb.msException);
                                Redirect3dsXform = (Cms.xForm)oPaymentProv.Activities.GetRedirect3dsForm(ref myWeb);

                                if (Redirect3dsXform is null)
                                {
                                    Protean.Cms.Cart.PaymentProviders oEwProv = new PaymentProviders(ref myWeb);
                                    Redirect3dsXform = oEwProv.GetRedirect3dsForm(ref myWeb);
                                }

                                moPageXml.SelectSingleNode("/Page/Contents").AppendChild(Redirect3dsXform.moXformElmt);
                                myWeb.moResponseType = Cms.pageResponseType.iframe;
                                break;
                            }

                        case "EnterPaymentDetails":
                        case "SubmitPaymentDetails": // confirm order and submit for payment
                            {
                                GetCart(ref oElmt);

                                if (Convert.ToString(oElmt.Attributes["statusId"].Value) == cartProcess.Complete.ToString())
                                {
                                    mnProcessId = (short)cartProcess.Complete;
                                    //  purchaseActions(oContentElmt, true);
                                    mcCartCmd = "ShowInvoice";
                                    goto processFlow;
                                }

                                mnProcessId = 5;

                                if (!string.IsNullOrEmpty(myWeb.moRequest["PaymentMethod"]))
                                {
                                    mcPaymentMethod = myWeb.moRequest["PaymentMethod"];
                                }

                                //if (oElmt.FirstChild is null)
                                //{
                                //    GetCart(ref oElmt);
                                //}

                                // Add the date and reference to the cart
                                if (oElmt != null)
                                {
                                    addDateAndRef(ref oElmt);
                                }

                                if (mcPaymentMethod == "No Charge")
                                {
                                    mcCartCmd = "ShowInvoice";
                                    mnProcessId = (short)cartProcess.Complete;
                                    goto processFlow;
                                }

                                cProcessInfo = "Payment Method from session = '" + mcPaymentMethod + "'";
                                //var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, mcPaymentMethod);
                                Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                                IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, mcPaymentMethod);
                                var ccPaymentXform = new Protean.xForm(ref myWeb.msException);
                                var cmsCart = this;
                                ccPaymentXform = (Protean.xForm)oPaymentProv.Activities.GetPaymentForm(ref myWeb, ref cmsCart, ref oElmt);

                                if ((mcPaymentMethod ?? "").Contains("Repeat_"))
                                {
                                    if (ccPaymentXform.valid == true)
                                    {
                                        mcCartCmd = "ShowInvoice";
                                        goto processFlow;
                                    }
                                    else if (ccPaymentXform.isSubmitted())
                                    {
                                        if (ccPaymentXform.getSubmitted() == "Cancel")
                                        {
                                            mcCartCmd = "ChoosePaymentShippingOption";
                                            goto processFlow;
                                        }
                                        else
                                        {
                                            // invalid redisplay form
                                        }
                                    }
                                }


                                // Don't show the payment screen if the stock levels are incorrect
                                if (oElmt.SelectSingleNode("error/msg") != null)
                                {
                                }
                                // oElmt.SelectSingleNode("error").PrependChild(oElmt.OwnerDocument.CreateElement("msg"))
                                // oElmt.SelectSingleNode("error").FirstChild.InnerXml = "<strong>PAYMENT CANNOT PROCEED UNTIL QUANTITIES ARE ADJUSTED</strong>"
                                else if (string.IsNullOrEmpty(mcPaymentMethod))
                                {

                                    mcCartCmd = "Confirm";
                                    goto processFlow;
                                }

                                else if (ccPaymentXform.valid == true)
                                {

                                    mcCartCmd = "ShowInvoice";

                                    // Move this from "ShowInvoice" to prevent URL requests from confirming successful payment
                                    if (mnProcessId != (int)cartProcess.DepositPaid & mnProcessId != (int)cartProcess.AwaitingPayment)
                                    {
                                        mnProcessId = (short)cartProcess.Complete;

                                        // remove the existing cart to force an update.
                                    }
                                    foreach (XmlNode oNodeCart in oElmt.SelectNodes("*"))
                                        oElmt.RemoveChild(oNodeCart);
                                    // oEwProv = Nothing
                                    goto processFlow;
                                }
                                else
                                {
                                    moPageXml.SelectSingleNode("/Page/Contents").AppendChild(ccPaymentXform.moXformElmt);
                                }

                                break;
                            }


                        // oEwProv = Nothing

                        case "ShowInvoice":
                        case "ShowCallBackInvoice": // Payment confirmed / show invoice
                            {

                                if (mnProcessId != (int)cartProcess.Complete & mnProcessId != (int)cartProcess.AwaitingPayment & mnProcessId != (int)cartProcess.DepositPaid)
                                {
                                    // check we are allready complete otherwise we will risk confirming sale just on URL request.
                                    // myWeb.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "FAILED CALLBACK : " & mnProcessId)
                                    mcCartCmd = "ChoosePaymentShippingOption";
                                    goto processFlow;
                                }
                                else
                                {
                                    GetCart(ref oElmt);

                                    if (oElmt != null && Convert.ToString(oElmt.Attributes["statusId"].Value) != "6")
                                    {
                                        CompleteOrder(oCartXML, ref oContentElmt, ref oElmt);
                                    }
                                    else
                                    {
                                        if (mnProcessId == (int)cartProcess.Complete | mnProcessId == (int)cartProcess.DepositPaid | mnProcessId == (int)cartProcess.AwaitingPayment)
                                        {

                                            addDateAndRef(ref oElmt);
                                            // purchaseActions(oContentElmt);
                                        }
                                    }


                                    if (mbQuitOnShowInvoice)
                                    {
                                        EndSession();
                                    }

                                }

                                break;
                            }

                        case "CookiesDisabled": // Cookies have been disabled or are undetectable
                            {
                                mnProcessError = 1;
                                GetCart(ref oElmt);
                                break;
                            }

                        case "CartEmpty": // Cookies have been disabled or are undetectable
                            {
                                mnProcessError = -1;
                                GetCart(ref oElmt);
                                break;
                            }

                        case "BackToSite":
                            {
                                bRedirect = true;
                                myWeb.moResponse.Redirect(mcSiteURL + mcReturnPage, false);
                                myWeb.moCtx.ApplicationInstance.CompleteRequest();
                                break;
                            }

                        case "List":
                            {
                                long nI = 0;
                                if (!string.IsNullOrEmpty(myWeb.moRequest["OrderID"]))
                                    nI = Convert.ToInt64(myWeb.moRequest["OrderID"]);
                                GetCartSummary(ref oElmt);
                                XmlElement argoPageDetail = null;
                                ListOrders(nI.ToString(), false, 0, oPageDetail: ref argoPageDetail);
                                break;
                            }

                        case "MakeCurrent":
                            {
                                long nI = 0;
                                if (!string.IsNullOrEmpty(myWeb.moRequest["OrderID"]))
                                    nI = Convert.ToInt64(myWeb.moRequest["OrderID"]);
                                if (!(nI == 0))
                                    MakeCurrent(nI);
                                mcCartCmd = "Cart";
                                goto processFlow;

                            }

                        case "Delete":
                            {
                                int nI = 0;
                                if (!string.IsNullOrEmpty(myWeb.moRequest["OrderID"]))
                                    nI = Convert.ToInt16(myWeb.moRequest["OrderID"]);
                                if (!(nI == 0))
                                    DeleteCart(nI);
                                mcCartCmd = "List";
                                goto processFlow;
                            }

                        case "Brief":
                            {
                                // Continue shopping
                                // go to the cart url
                                string cPage = moCartConfig["ContinuePath"];
                                if (!string.IsNullOrEmpty(moCartConfig["ContinuePath"]?.ToString().ToLower()))
                                {
                                    cPage = myWeb.moRequest["pgid"];
                                    if (string.IsNullOrEmpty(cPage) | cPage is null)
                                        cPage = moPageXml.DocumentElement.GetAttribute("id");
                                    cPage = "?pgid=" + cPage;
                                }
                                myWeb.moResponse.Redirect(mcSiteURL + cPage, false);
                                myWeb.moCtx.ApplicationInstance.CompleteRequest();
                                break;
                            }

                        case "NoteToAddress":
                            {
                                // do nothing this is a placeholder for openquote
                                GetCart(ref oElmt); // Show Cart Summary
                                break;
                            }

                        default:
                            {
                                mcCartCmd = "";
                                if (bFullCartOption == true)
                                {
                                    GetCart(ref oElmt);
                                }
                                else
                                {
                                    GetCartSummary(ref oElmt);
                                }

                                break;
                            }

                    }

                    PersistVariables(); // store data for next time this function runs

                    if (oElmt != null)
                    {
                        oElmt.SetAttribute("cmd", mcCartCmd);
                        oElmt.SetAttribute("sessionId", mcSessionId);
                        oElmt.SetAttribute("siteUrl", mcSiteURL);
                        oElmt.SetAttribute("cartUrl", mcCartURL);
                    }

                    AddCartToPage(moPageXml, oContentElmt);

                    return;
                }

                catch (Exception ex)
                {
                    if (bRedirect == true & ReferenceEquals(ex.GetType(), typeof(System.Threading.ThreadAbortException)))
                    {
                    }
                    // mbRedirect = True
                    // do nothing
                    else
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "apply", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                    }

                }
            }

            public void AddCartToPage(XmlDocument moPageXml, XmlElement oContentElmt)
            {
                string cProcessInfo = "";
                try
                {
                    oContentElmt.SetAttribute("currencyRef", mcCurrencyRef);
                    oContentElmt.SetAttribute("currency", mcCurrency);
                    oContentElmt.SetAttribute("currencySymbol", mcCurrencySymbol);
                    oContentElmt.SetAttribute("Process", mnProcessId.ToString());

                    // remove any existing Cart 
                    if (moPageXml.DocumentElement.SelectSingleNode("Cart") != null)
                    {
                        moPageXml.DocumentElement.RemoveChild(moPageXml.DocumentElement.SelectSingleNode("Cart"));
                    }

                    moPageXml.DocumentElement.AppendChild(oContentElmt);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "AddCartElement", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }
            }

            public virtual void AddBehaviour()
            {
                string cProcessInfo = "";
                try
                {

                    switch ((moCartConfig["AddBehaviour"]?.ToString().ToLower()) ?? "")
                    {
                        case "discounts":
                            {
                                mcCartCmd = "Discounts";
                                break;
                            }
                        case "notes":
                            {
                                mcCartCmd = "RedirectSecure";
                                break;
                            }
                        case "brief":
                            {
                                mcCartCmd = "Brief";
                                break;
                            }
                        case "logon":
                            {
                                mcCartCmd = "Logon";
                                break;
                            }

                        default:
                            {
                                break;
                            }
                            // do nothing
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "AddBehavior", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }

            }

            public virtual PaymentProviders GetPaymentProvider()
            {

                var oEwProv = new PaymentProviders(ref myWeb);
                return oEwProv;

            }

            /// <summary>
            /// This provides the ability to add customers to makreting lists if they reach a particular stage within a shopping cart.
            /// Only works if 3rd party messaging provider enabled.
            /// </summary>
            /// <param name="StepName"></param>
            /// <param name="oCartElmt"></param>
            public virtual void AddToLists(string StepName, ref XmlElement oCartElmt, string Name = "", string Email = "", Dictionary<string, string> valDict = null)
            {
                myWeb.PerfMon.Log("Cart", "AddToLists");
                // Dim sMessageResponse As String
                string cProcessInfo = "";
                try
                {
                    System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                    if (moMailConfig != null)
                    {

                        string sMessagingProvider = "";

                        if (moMailConfig != null)
                        {
                            sMessagingProvider = moMailConfig["MessagingProvider"];
                        }

                        if (!string.IsNullOrEmpty(sMessagingProvider) | !string.IsNullOrEmpty(moMailConfig["InvoiceList"]) & !string.IsNullOrEmpty(moMailConfig["QuoteList"]))
                        {

                            Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                            IMessagingProvider oMessaging = RetProv.Get(ref myWeb, sMessagingProvider);
                            string xsltPath = string.Empty;
                            bool bOptOut = false;
                            if (string.IsNullOrEmpty(Email))
                                if (oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email") != null)
                                {
                                    Email = oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText;
                                }
                            if (string.IsNullOrEmpty(Name))
                                if (oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/GivenName") != null)
                                {
                                    Name = oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText;
                                }

                            if (valDict is null)
                                valDict = new Dictionary<string, string>();

                            if (oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email/@optOut") != null)
                            {
                                bOptOut = Convert.ToBoolean(oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email/@optOut").InnerText);
                            }
                            else
                            {
                                if (myWeb.moDbHelper.checkTableColumnExists("tblOptOutAddresses", "nOptOutKey"))
                                {
                                    if (!string.IsNullOrEmpty(Email))
                                    {
                                        string cSQL = $"Select EmailAddress FROM tblOptOutAddresses WHERE (EmailAddress = '{Email}')";
                                        string cSQLStatusCheck = $"Select top 1 nStatus FROM tblOptOutAddresses WHERE (EmailAddress = '{Email}') order by dOptOut desc";

                                        bool bstatus = Convert.ToBoolean(moDBHelper.ExeProcessSqlScalar(cSQLStatusCheck));
                                        bOptOut = bstatus;
                                    }
                                      
                                   

                                }
                            }
                                foreach (XmlAttribute Attribute in oCartElmt.Attributes)
                                {
                                    if (!"errorMsg,hideDeliveryAddress,orderType,statusId,complete".Contains(Attribute.Name))
                                    {
                                        valDict.Add(Attribute.Name, Attribute.Value);
                                    }
                                }
                            string[] fullName = Name.Split(' ');
                            string firstName = "";
                            string lastName = "";
                            if (fullName.Length >= 3)
                            {
                                firstName = fullName[1];
                                lastName = fullName[2];
                            }

                            string ListId = "";
                            switch (StepName ?? "")
                            {
                                case "Invoice":
                                    {
                                        ListId = moMailConfig["InvoiceList"];
                                        xsltPath = moMailConfig["GetDictionaryForInvoiceListXsl"];
                                        if (!string.IsNullOrEmpty(moMailConfig["InvoiceList"]))
                                        {
                                            // if we have invoiced the customer we don't want to send them quote reminders
                                            if (oMessaging.Activities != null)
                                            {
                                                oMessaging.Activities.RemoveFromList(moMailConfig["InvoiceList"], Email);
                                            }
                                        }

                                        break;
                                    }
                                case "Quote":
                                    {
                                        ListId = moMailConfig["QuoteList"];
                                        xsltPath = moMailConfig["GetDictionaryForQuoteListXsl"];
                                        break;
                                    }
                                case "Deposit":
                                    {
                                        ListId = moMailConfig["DepositList"];
                                        break;
                                    }
                                case "Newsletter":
                                    {
                                        ListId = moMailConfig["NewsletterList"];
                                        if (!string.IsNullOrEmpty(moMailConfig["NewsletterList"]))
                                        {
                                            oMessaging.Activities.RemoveFromList(moMailConfig["NewsletterList"].ToString(), Email);
                                        }

                                        break;
                                    }
                            }
                            if (!string.IsNullOrEmpty(ListId))
                            {

                                if (!string.IsNullOrEmpty(xsltPath))
                                {
                                    valDict = GetDictionaryForCampaign(xsltPath, ref oCartElmt, valDict);
                                }
                                else
                                {
                                    valDict.Add("email", Email);
                                    valDict.Add("FirstName", firstName);
                                    valDict.Add("LastName", lastName);
                                }
                                if (oMessaging.Activities != null)
                                {
                                    if (!bOptOut)
                                    {
                                        oMessaging.Activities.AddToList(ListId, firstName, Email, valDict);

                                    }
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "purchaseActions", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }

            }

            private Dictionary<string, string> GetDictionaryForCampaign(string xsltPath, ref XmlElement oCartElmt, Dictionary<string, string> valDict = null)
            {
                string styleFile;
                string messageHtml = "";
                TextWriter sWriter = new StringWriter();
                var oXml = new XmlDocument();
                var oTransform = new Protean.XmlHelper.Transform();
                string XmlString = oCartElmt.OuterXml;
                oXml.LoadXml(XmlString);
                styleFile = moServer.MapPath(xsltPath);
                oTransform.Compiled = false;
                oTransform.XSLFile = styleFile;
                oTransform.Process(oXml, ref sWriter);
                if (oTransform.HasError)
                {
                    throw new Exception("There was an error transforming the email (Output: HTML).");
                }
                messageHtml = sWriter.ToString();
                sWriter.Close();
                var xMailingListDoc = Protean.Tools.Xml.HtmlConverter.htmlToXmlDoc(messageHtml);
                var xListElement = xMailingListDoc.DocumentElement;
                valDict = XmltoDictionary(xListElement, true);
                return valDict;
            }
            private void RemoveDeliveryOption(long nOrderId)
            {
                try
                {
                    string cSQL = "UPDATE tblCartOrder SET nShippingMethodId = 0, cShippingDesc = NULL, nShippingCost = 0 WHERE nCartOrderKey = " + nOrderId;
                    myWeb.moDbHelper.ExeProcessSqlorIgnore(cSQL);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "RemoveDeliveryOption", ex, myWeb.moCtx, "", "", gbDebug);
                }
            }


            /// <summary>
            /// This does the same as get cart without the item information, so we call get cart and delete any items we find
            /// </summary>
            /// <param name="oCartElmt"></param>
            /// <param name="nSelCartId"></param>
            /// <remarks></remarks>
            public void GetCartSummary(ref XmlElement oCartElmt, long nSelCartId = 0)
            {
                // Sets content for the XML to be displayed in the small summary plugin attached
                // to the current content page
                myWeb.PerfMon.Log("Cart", "GetCartSummary");
                long nCartIdUse;
                if (nSelCartId > 0)
                {
                    nCartIdUse = nSelCartId;
                }
                else
                {
                    nCartIdUse = mnCartId;
                }
                string cProcessInfo = "CartId=" + nCartIdUse;
                try
                {
                    GetCart(ref oCartElmt, nCartIdUse);
                    // remove all the items
                    foreach (XmlElement oElmt in oCartElmt.SelectNodes("/Cart/Order/Item"))
                        oElmt.ParentNode.RemoveChild(oElmt);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetCartSummary", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }

            }

            public void GetCart()
            {
                try
                {
                    XmlElement argoCartElmt = (XmlElement)moCartXml.FirstChild;
                    GetCart(ref argoCartElmt);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetCart", ex, myWeb.moCtx, "", "", gbDebug);
                }
            }

            public void GetCart(ref XmlElement oCartElmt, long nSelCartId = 0)
            {
                oCartElmt.InnerXml = "";
                myWeb.PerfMon.Log("Cart", "GetCart");
                // Content for the XML that will display all the information stored for the Cart
                // This is a list of cart items (and quantity, price ...), totals,
                // billing & delivery addressi and delivery method.
                short ReceiptDeliveryType;
                DataSet oDs;
                DataSet oDs2;
                //DataSet oDsShippingOptionKey;

                string sSql;
                DataRow oRow;
                DataRow oRow2;

                XmlElement oElmt;
                XmlElement oElmt2;
                XmlDocument oXml;

                long quant;
                double weight;
                double total;
                double nPayableAmount;
                double vatAmt;
                double shipCost;
                var nCheckPrice = default(double);
                long nStatusId = 0;
                XmlElement oCheckPrice;
                // We need to read this value from somewhere so we can change where vat is added
                // Currently defaulted to per line
                // If true will be added to the unit
                //decimal nLineVat = 0m;
                object bCheckSubscriptions = false;
                string cOptionGroupName = "";

                long nCartIdUse;
                if (nSelCartId > 0)
                {
                    nCartIdUse = nSelCartId;
                }
                else
                {
                    nCartIdUse = mnCartId;
                }

                long oldCartId = mnCartId;
                long ShippingOptionKey = Convert.ToInt64(moCartConfig["DefaultShippingMethod"]);
                // Dim cCartType As String = String.Empty

                string cProcessInfo = "CartId=" + nCartIdUse;

                // For related products
                var oItemList = new Hashtable(); // for related Products
                if (moCartConfig["RelatedProductsInCart"] == "on")
                {


                }
                try
                {

                    if (moSubscription != null & nCartIdUse != 0)
                    {
                        bCheckSubscriptions = (object)moSubscription.CheckCartForSubscriptions(nCartIdUse, myWeb.mnUserId);
                        if (moCartConfig["subCheck"] == "always")
                        {
                            bCheckSubscriptions = true;
                        }
                    }


                    if (!(nCartIdUse > 0)) // no shopping
                    {
                        oCartElmt.SetAttribute("status", "Empty"); // set CartXML attributes
                        oCartElmt.SetAttribute("itemCount", "0"); // to nothing
                        oCartElmt.SetAttribute("vatRate", moCartConfig["TaxRate"]);
                        oCartElmt.SetAttribute("total", "0.00"); // for nothing
                    }
                    else
                    {
                        // otherwise
                        oCartElmt.SetAttribute("cartId", mnCartId.ToString());
                        oCartElmt.SetAttribute("session", mcSessionId);
                        // Check tax rate
                        string argcContactCountry = "";
                        UpdateTaxRate(cContactCountry: ref argcContactCountry);

                        // and the address details we have obtained
                        // (if any)
                        // Add Totals
                        quant = 0L; // get number of items & sum of collective prices (ie. cart total) from db
                        total = 0.0d;
                        weight = 0.0d;
                        if (moCartConfig["TareWeight"] != "")
                        {
                            weight = Convert.ToInt32(moCartConfig["TareWeight"]);
                        }

                        ReceiptDeliveryType = 0;
                        // Process promo code from external refs.
                        if ((!string.IsNullOrEmpty(myWeb.moSession["promocode"] as string)) || !string.IsNullOrEmpty(myWeb.moRequest["promocode"]))
                        {
                            if (!string.IsNullOrEmpty(myWeb.moRequest["promocode"]))
                            {
                                promocodeFromExternalRef = myWeb.moRequest["promocode"].ToString();
                            }
                            else if (!string.IsNullOrEmpty(myWeb.moSession["promocode"] as string))
                            {
                                promocodeFromExternalRef = myWeb.moSession["promocode"].ToString();
                            }
                            myWeb.moSession["promocode"] = "";
                        }
                        if (!string.IsNullOrEmpty(promocodeFromExternalRef))
                        {
                            oCartElmt.SetAttribute("promocodeFromExternalRef", promocodeFromExternalRef);
                        }
                        string additionalFields = string.Empty;
                        if (moDBHelper.checkTableColumnExists("tblCartItem", "nDepositAmount"))
                        {
                            additionalFields = ", i.nDepositAmount as nDepositAmount";
                        }
                        // Added Left Join with tblCartCatProductRelations to bring group assigned to that product  
                        if (moDBHelper.checkTableColumnExists("tblCartItem", "xItemXml"))
                        {
                            sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, i.xItemXml as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, i.xItemXml.value('Content[1]/@type','nvarchar(50)') AS contentType, dbo.fxn_getContentParents(i.nItemId) as parId " + additionalFields + " ,A.nStatus As ProductStatus, '' As nShippingGroup,'' As nshippingType from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey left join tblAudit A ON p.nAuditId= A.nAuditKey where nCartOrderId=" + nCartIdUse;
                        }
                        else
                        {
                            sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, p.cContentXmlDetail as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, p.cContentSchemaName AS contentType, dbo.fxn_getContentParents(i.nItemId) as parId " + additionalFields + " from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" + nCartIdUse;
                        }

                        oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart");

                        // add relationship for options
                        oDs.Relations.Add("Rel1", oDs.Tables["Item"].Columns["id"], oDs.Tables["Item"].Columns["nParentId"], false);
                        oDs.Relations["Rel1"].Nested = true;
                        // 
                        var revisedPrice = default(double);
                        foreach (DataRow currentORow in oDs.Tables["Item"].Rows)
                        {
                            oRow = currentORow;
                            double Discount = 0d;
                            if (!oItemList.ContainsValue(oRow["contentId"]))
                            {
                                oItemList.Add(oItemList.Count, oRow["contentId"]);
                            }
                            if (Convert.ToInt32(moDBHelper.DBN2int(oRow["nParentId"])) == 0)
                            {
                                long nTaxRate = 0L;
                                bool bOverridePrice = false;
                                if (!mbOveridePrice) // for openquote
                                {
                                    if (myWeb.moSession["overridePriceSession"] != null & myWeb.moConfig["overridePriceKey"] != null)
                                    {
                                        // get the string value from session
                                        string sSessionKey = Convert.ToString(myWeb.moSession["overridePriceSession"]);
                                        string sKey = Convert.ToString(myWeb.moConfig["overridePriceKey"]);
                                        // create the key with the current user
                                        string sSessionId = Convert.ToString(myWeb.moSession.SessionID);
                                        // generate the key with current session id
                                        string sEncryptedKey = Tools.Encryption.RC4.Encrypt(sSessionId, sKey);
                                        if ((sEncryptedKey ?? "") == (sSessionKey ?? "")) // if both matches allow to overrdide price
                                        {
                                            bOverridePrice = true;
                                        }
                                    }
                                    // Go get the lowest price based on user and group
                                    else if (!(oRow["productDetail"] is DBNull))
                                    {

                                        var oProd = moPageXml.CreateElement("product");
                                        oProd.InnerXml = Convert.ToString(oRow["productDetail"]);
                                        if (oProd.SelectSingleNode("Content[@overridePrice='true']") is null && oProd.SelectSingleNode("Content[contains(@action,'VariableSubscription')]") is null)
                                        {
                                            oCheckPrice = getContentPricesNode(oProd, oRow["unit"]?.ToString() ?? "", Convert.ToInt64(oRow["quantity"]));

                                            cProcessInfo = "Error getting price for unit:" + (oRow["unit"]?.ToString() ?? "") +
                                                           " and Quantity:" + oRow["quantity"] +
                                                           " and Currency " + mcCurrencyRef +
                                                           " Check that a price is available for this quantity and a group for this current user.";
                                            if (oCheckPrice != null)
                                            {
                                                nCheckPrice = Convert.ToDouble(oCheckPrice.InnerText);
                                                // TS moved to the end when calcuating deposit totals as we need non deposit items to have discounts calculated allready.
                                                // If moDBHelper.checkTableColumnExists("tblCartItem", "nDepositAmount") Then
                                                // If CDbl("0" & oRow("nDepositAmount").ToString()) > 0 Then
                                                // nPayableAmount = nPayableAmount + CDbl("0" & oRow("nDepositAmount")) * oRow("quantity")
                                                // Else
                                                // 'TS added if full price product and also deposit in cart.
                                                // nPayableAmount = nPayableAmount + CDbl("0" & oCheckPrice.InnerText()) * oRow("quantity")
                                                // End If
                                                // End If
                                                nTaxRate = (long)Math.Round(getProductTaxRate(oCheckPrice));
                                            }
                                            // nCheckPrice = getProductPricesByXml(oRow("productDetail"), oRow("unit") & "", oRow("quantity"))
                                            if (moSubscription != null && (oRow["contentType"]?.ToString() ?? "") == "Subscription")
                                            {
                                                if (moSubscription.mbOveridePrices == false)
                                                {
                                                    // TS added when subscription when initial cost is changed in by external logic we should not refer back to the stored content.
                                                    if ((oRow["contentId"] as int? ?? 0) > 0)
                                                    {
                                                        revisedPrice = moSubscription.CartSubscriptionPrice(Convert.ToInt16(oRow["contentId"]), myWeb.mnUserId);
                                                    }
                                                    else
                                                    {
                                                        oCheckPrice = getContentPricesNode(oProd, oRow["unit"]?.ToString() ?? "", Convert.ToInt64(oRow["quantity"]), "SubscriptionPrices");
                                                        nCheckPrice = Convert.ToDouble(oCheckPrice.InnerText);
                                                        nTaxRate = (long)Math.Round(getProductTaxRate(oCheckPrice));
                                                    }
                                                    if (revisedPrice < nCheckPrice)
                                                    {
                                                        // nCheckPrice = revisedPrice
                                                        Discount = nCheckPrice - revisedPrice;
                                                        nCheckPrice = revisedPrice;
                                                    }
                                                }

                                            }
                                        }
                                        else
                                        {
                                            bOverridePrice = true;
                                        }
                                    }
                                    if (!bOverridePrice)
                                    {
                                        if (nCheckPrice > 0d && !nCheckPrice.Equals(oRow["price"]))
                                        {
                                            // If price is lower, then update the item price field
                                            oRow["price"] = nCheckPrice;
                                        }

                                        if (!nTaxRate.Equals(oRow["taxRate"]))
                                        {
                                            oRow["taxRate"] = nTaxRate;
                                        }
                                    }

                                    // option prices
                                }
                                decimal nOpPrices = 0m;
                                foreach (var oOpRow in oRow.GetChildRows("Rel1"))
                                {
                                    if (!mbOveridePrice) // for openquote
                                    {
                                        decimal nNPrice = (decimal)getOptionPricesByXml(Convert.ToString(oRow["productDetail"]), Convert.ToInt16(oRow["nItemOptGrpIdx"]), Convert.ToInt16(oRow["nItemOptIdx"]));
                                        if (nNPrice > 0m && !nNPrice.Equals(oOpRow["price"]))
                                        {
                                            nOpPrices += nNPrice;
                                            // oOpRow.BeginEdit()
                                            oOpRow["price"] = nNPrice;
                                        }
                                        // oOpRow.EndEdit()

                                        else if (moCartConfig["ProductOptionOverideQuantity"] == "on")
                                        {
                                            nOpPrices += Convert.ToDecimal(oOpRow["price"]) * Convert.ToDecimal(oOpRow["quantity"]);
                                        }
                                        else
                                        {
                                            nOpPrices = Convert.ToDecimal(nOpPrices + Convert.ToDecimal(oOpRow["price"]));

                                        }
                                    }
                                    else if ((moCartConfig["ProductOptionOverideQuantity"]?.ToString() == "on") && Convert.ToInt32(oOpRow["quantity"]) > 1)
                                    {
                                        nOpPrices += Convert.ToDecimal(oOpRow["price"]) * Convert.ToDecimal(oOpRow["quantity"]);
                                    }
                                }

                                // Apply stock control
                                if (mbStockControl)
                                    CheckStock(ref oCartElmt, Convert.ToString(oRow["productDetail"]), Convert.ToString(oRow["quantity"]));
                                // Apply quantity control
                                if (!(oRow["productDetail"] is DBNull))
                                {
                                    // not sure why the product has no detail but if it not we skip this, suspect it was old test data that raised this issue.
                                    CheckQuantities(ref oCartElmt, oRow["productDetail"]?.ToString() ?? "", Convert.ToInt64(oRow["quantity"]?.ToString() ?? "0").ToString());
                                }

                                weight += Convert.ToInt32(oRow["weight"]) * Convert.ToInt32(oRow["quantity"]);
                                quant += Convert.ToInt32(oRow["quantity"]);
                                if (moCartConfig["ProductOptionOverideQuantity"] == "on")
                                {
                                    total += Convert.ToDouble(oRow["quantity"]) *
                                             Convert.ToDouble(Round(Convert.ToDouble(oRow["price"]), bForceRoundup: mbRoundup))
                                             + Convert.ToDouble(Round(Convert.ToDouble(nOpPrices), bForceRoundup: mbRoundup));
                                }
                                else
                                {
                                    total += Convert.ToDouble(oRow["quantity"]) *
                                             Convert.ToDouble(
                                                 Round(
                                                     Convert.ToDouble(oRow["price"]) + Convert.ToDouble(nOpPrices),
                                                     bForceRoundup: mbRoundup
                                                 )
                                             );
                                }


                                // we do this later after we have applied discounts

                                // Round( Price * Vat ) * Quantity
                                // nUnitVat += Round((oRow("price") + nOpPrices) * (mnTaxRate / 100), , , mbRoundup) * oRow("quantity")
                                // Round( ( Price * Quantity )* VAT )
                                // nLineVat += Round((((oRow("price") + nOpPrices)) * oRow("quantity")) * (mnTaxRate / 100), , , mbRoundup)
                            }
                            // Dim ix As Integer
                            // Dim xstr As String
                            // For ix = 0 To oRow.Table.Columns.Count - 1
                            // xstr &= oRow.Table.Columns(ix).ColumnName & "="
                            // xstr &= oRow(ix) & ", "
                            // Next


                            // check if shipping group exists or not and then we set bydefault delivery option on cart

                            //if (myWeb.moDbHelper.checkDBObjectExists("spGetValidShippingOptions", Tools.Database.objectTypes.StoredProcedure))
                            //{
                            //if (nStatusId > 100) {

                            //// Get Shipping Group from query if assigned to that product and add new node in order and use this node for displaying messages for x50 and t03 category.
                            //if (moConfig["SelectShippingOptionForGroup"] != null)
                            //{
                            //    if ((moConfig["SelectShippingOptionForGroup"]) != "" && (moConfig["SelectShippingOptionForGroup"]).ToLower() == "on")
                            //    {
                            //        string sSqlShippingGroup = $"select csm.nShipOptKey,CPC.cCatName  from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey left join tblAudit A ON p.nAuditId= A.nAuditKey left join tblCartCatProductRelations cpr on p.nContentKey = cpr.nContentId left join tblCartProductCategories CPC ON cpr.nCatId= cpc.nCatKey Left JOIN tblCartShippingProductCategoryRelations cspcr ON cpr.nCatId= cspcr.nCatId LEFT join tblCartShippingMethods csm on csm.nShipOptKey=cspcr.nShipOptId where nCartOrderId={nCartIdUse.ToString()} and nCartItemKey={oRow["id"].ToString()} and cCatSchemaName = 'Shipping' and csm.nShipOptKey is not null and nItemId <>0 and cspcr.nRuleType=1 order by nShipOptCost asc";

                            //        using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sSqlShippingGroup))
                            //        {
                            //            if (oDr != null)
                            //            {
                            //                while (oDr.Read())
                            //                {
                            //                    ShippingOptionKey = Convert.ToInt64(oDr["nShipOptKey"]);
                            //                    oRow["nShippingGroup"] = oDr["cCatName"];
                            //                    oRow["nshippingType"] = ShippingOptionKey;
                            //                    updateGCgetValidShippingOptionsDS(ShippingOptionKey.ToString());
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
                            //}
                            //}


                            try
                            {
                                if (oRow["price"] != DBNull.Value)
                                {

                                    double cartPrice = Convert.ToDouble(oRow["price"]);
                                    double contentPrice = GetPriceFromContent(Convert.ToInt32(oRow["contentId"]));

                                    if (contentPrice > 0 && cartPrice != contentPrice)
                                    {
                                        oRow["price"] = contentPrice;
                                        UpdateItemPrice(Convert.ToInt64(oRow["id"]), contentPrice);
                                    }
                                    // If oRow("price") <> 0 Then
                                    string discountSQL = "";
                                    if (Discount != 0d)
                                    {
                                        // discountSQL = ", nDiscountValue = " & Discount & " "
                                    }
                                    string cUpdtSQL = "UPDATE tblCartItem Set nPrice = " + oRow["price"] + discountSQL + " WHERE nCartItemKey = " + oRow["id"];
                                    moDBHelper.ExeProcessSql(cUpdtSQL);
                                    // End If
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }


                        // Quantity based error messaging
                        XmlElement oError = (XmlElement)oCartElmt.SelectSingleNode("Error");
                        if (oError != null)
                        {
                            XmlElement oMsg;
                            oMsg = oCartElmt.OwnerDocument.CreateElement("msg");
                            oMsg.SetAttribute("type", "zz_footer"); // Forces it to the bottom of the message block
                            oMsg.InnerText = "<span Class=\"term3081\">Please adjust the quantities you require, Or Call For assistance.</span>";
                            oError.AppendChild(oMsg);
                        }


                        // moDBHelper.updateDataset(oDs, "Item", True)

                        // add to Cart XML
                        sSql = "Select nCartStatus from tblCartOrder where nCartOrderKey = " + nCartIdUse;
                        nStatusId = Convert.ToInt64(moDBHelper.DBN2Str(moDBHelper.ExeProcessSqlScalar(sSql), false, false));
                        // moCartConfig("OrderPaymentStatusId") = nStatusId
                        oCartElmt.SetAttribute("statusId", nStatusId.ToString());
                        oCartElmt.SetAttribute("status", getProcessName((cartProcess)nStatusId));
                        oCartElmt.SetAttribute("itemCount", quant.ToString());
                        oCartElmt.SetAttribute("weight", weight.ToString());
                        oCartElmt.SetAttribute("orderType", mmcOrderType + "");

                        mcBlockCartUpdate = GetBlockCartUpdatesConfig();

                        if (!string.IsNullOrEmpty(mcBlockCartUpdate)
                            && mcBlockCartUpdate.Trim().ToLower() == "on")
                        {
                            oCartElmt.SetAttribute("BlockCartUpdate", "on");
                        }
                        else
                        {
                            oCartElmt.SetAttribute("BlockCartUpdate", "off");
                        }
                        if (nStatusId == 6L)
                        {
                            oCartElmt.SetAttribute("complete", "True");
                        }
                        else
                        {
                            oCartElmt.SetAttribute("complete", "True");
                        }

                        // Add the addresses to the dataset
                        if (nCartIdUse > 0)
                        {
                            if (myWeb.moDbHelper.checkTableColumnExists("tblCartContact", "cContactTelCountryCode"))
                            {
                                sSql = "Select cContactType As type, cContactName As GivenName, cContactCompany As Company, cContactAddress As Street, cContactCity As City, cContactState As State, cContactZip As PostalCode, cContactCountry As Country, cContactTel As Telephone, cContactFax As Fax, cContactEmail As Email, cContactXml As Details,cContactTelCountryCode As TelephoneCountryCode from tblCartContact where nContactCartId=" + nCartIdUse;
                            }
                            else
                            {
                                sSql = "Select cContactType As type, cContactName As GivenName, cContactCompany As Company, cContactAddress As Street, cContactCity As City, cContactState As State, cContactZip As PostalCode, cContactCountry As Country, cContactTel As Telephone, cContactFax As Fax, cContactEmail As Email, cContactXml As Details from tblCartContact where nContactCartId=" + nCartIdUse;
                            }
                            moDBHelper.addTableToDataSet(ref oDs, sSql, "Contact");
                        }

                        // Add Items - note - do this AFTER we've updated the prices! 

                        if (oDs.Tables["Item"].Rows.Count > 0)
                        {
                            // cart items
                            oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[5].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[6].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[7].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[8].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[9].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[10].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns[11].ColumnMapping = MappingType.Attribute;
                            oDs.Tables[0].Columns["parId"].ColumnMapping = MappingType.Attribute;

                            // cart contacts
                            oDs.Tables["Contact"].Columns[0].ColumnMapping = MappingType.Attribute;

                            oXml = new XmlDocument();
                            oXml.LoadXml(oDs.GetXml());
                            oDs.EnforceConstraints = false;

                            // Convert the detail to xml
                            foreach (XmlElement currentOElmt in oXml.SelectNodes("/Cart/Item/productDetail | /Cart/Contact/Detail | /Cart/Contact/Details"))
                            {
                                oElmt = currentOElmt;
                                oElmt.InnerXml = oElmt.InnerText;
                                if (oElmt.SelectSingleNode("Content") != null)
                                {
                                    foreach (XmlAttribute oAtt in oElmt.SelectSingleNode("Content").Attributes)
                                        oElmt.SetAttribute(oAtt.Name, oAtt.Value);
                                    oElmt.InnerXml = oElmt.SelectSingleNode("Content").InnerXml;
                                    XmlElement oContent = (XmlElement)oElmt.SelectSingleNode("Content");
                                }
                            }


                            foreach (XmlElement currentOElmt1 in oXml.SelectNodes("/Cart/Contact/Email"))
                            {
                                oElmt = currentOElmt1;
                                if (moDBHelper.CheckOptOut(oElmt.InnerText))
                                {
                                    oElmt.SetAttribute("optOut", "True");
                                }
                            }

                            // get the option xml
                            foreach (XmlElement currentOElmt2 in oXml.SelectNodes("/Cart/Item/Item/productDetail"))
                            {
                                oElmt = currentOElmt2;
                                oElmt.InnerXml = oElmt.InnerText;
                                string nGroupIndex = oElmt.ParentNode.SelectSingleNode("nItemOptGrpIdx").InnerText;
                                string nOptionIndex = oElmt.ParentNode.SelectSingleNode("nItemOptIdx").InnerText;
                                cOptionGroupName = "";
                                if (oElmt.SelectSingleNode("Content/Options/OptGroup[" + nGroupIndex + "]/@name") != null)
                                {
                                    cOptionGroupName = oElmt.SelectSingleNode("Content/Options/OptGroup[" + nGroupIndex + "]/@name").InnerText;
                                }
                                if (Convert.ToDouble(nOptionIndex) >= 0d)
                                {
                                    oElmt2 = (XmlElement)oElmt.SelectSingleNode("Content/Options/OptGroup[" + nGroupIndex + "]/Option[" + nOptionIndex + "]");
                                    if (oElmt2 != null)
                                    {
                                        if (!string.IsNullOrEmpty(cOptionGroupName))
                                            oElmt2.SetAttribute("groupName", cOptionGroupName);
                                        oElmt.ParentNode.InnerXml = oElmt2.OuterXml;
                                    }
                                }
                                else
                                {
                                    // case for text option
                                    oElmt2 = (XmlElement)oElmt.SelectSingleNode("Content/Options/OptGroup[" + nGroupIndex + "]/Option[1]");
                                    if (oElmt2 != null)
                                    {
                                        if (!string.IsNullOrEmpty(cOptionGroupName))
                                            oElmt2.SetAttribute("groupName", cOptionGroupName);
                                        if (oElmt.ParentNode.SelectSingleNode("Name") != null)
                                        {
                                            oElmt2.SetAttribute("name", oElmt.ParentNode.SelectSingleNode("Name").InnerText);
                                        }
                                        else
                                        {
                                            oElmt2.SetAttribute("name", "Name Not defined");
                                        }
                                        oElmt.ParentNode.InnerXml = oElmt2.OuterXml;
                                    }
                                }

                            }
                            oElmt = moPageXml.CreateElement("Cart");
                            // Note: Preserve the original elements in oCartElmt
                            oCartElmt.InnerXml = oCartElmt.InnerXml + oXml.FirstChild.InnerXml;
                        }

                        myWeb.CheckMultiParents(ref oCartElmt);

                        sSql = "Select cClientNotes from tblCartOrder where nCartOrderKey=" + nCartIdUse;
                        var oNotes = oCartElmt.OwnerDocument.CreateElement("Notes");
                        string notes = "" + moDBHelper.ExeProcessSqlScalar(sSql);
                        oNotes.InnerXml = notes;

                        total -= (double)moDiscount.CheckDiscounts(oDs, oCartElmt, true, oNotes);

                        oXml = null;
                        oDs = null;

                        if (moDiscount.bHasPromotionalDiscounts)
                        {
                            oCartElmt.SetAttribute("showDiscountCodeBox", "true");
                        }

                        string cPromoCode = "";
                        bool IsPromocodeValid = false;
                        XmlElement oPromoElmt = (XmlElement)oNotes.SelectSingleNode("//Notes/PromotionalCode");
                        if (oPromoElmt != null)
                            cPromoCode = oPromoElmt.InnerText;

                        if (moSubscription != null)
                        {
                            if (moSubscription.CheckCartForSubscriptions(mnCartId, myWeb.mnUserId))
                            {
                                mbNoDeliveryAddress = true;
                            }
                        }

                        if (mbNoDeliveryAddress)
                            oCartElmt.SetAttribute("hideDeliveryAddress", "True");
                        if (mnGiftListId > 0)
                            oCartElmt.SetAttribute("giftListId", mnGiftListId.ToString());

                        sSql = "Select * from tblCartOrder where nCartOrderKey=" + nCartIdUse;

                        oDs = moDBHelper.GetDataSet(sSql, "Order", "Cart");
                        var dDueDate = default(DateTime);
                        foreach (DataRow currentORow1 in oDs.Tables["Order"].Rows)
                        {
                            oRow = currentORow1;
                            shipCost = Convert.ToDouble(Convert.IsDBNull(oRow["nShippingCost"]) ? 0.0 : Convert.ToDouble(oRow["nShippingCost"]));
                            oCartElmt.SetAttribute("shippingType", oRow["nShippingMethodId"]?.ToString() ?? "");
                            oCartElmt.SetAttribute("shippingCost", shipCost.ToString());
                            oCartElmt.SetAttribute("shippingDesc", oRow["cShippingDesc"]?.ToString() ?? "");

                            if (moDBHelper.checkTableColumnExists("tblCartOrder", "nReceiptType"))
                            {
                                if (oRow["nReceiptType"] is DBNull)
                                {
                                    ReceiptDeliveryType = 1;
                                }
                                else
                                {
                                    ReceiptDeliveryType = Convert.ToInt16(oRow["nReceiptType"]);
                                }
                                oCartElmt.SetAttribute("ReceiptType", ReceiptDeliveryType.ToString());

                            }

                            if (oCartElmt.GetAttribute("NonDiscountedShippingCost") != null)
                            {
                                // As NonDiscountedShippingCost is initialized in CheckDiscount method, Free shipping promocode is valid so set the flag to True
                                // for setting default shipping option to the cart with updating the NonDiscountedShippingCost amount to the free shipping amount.
                                if (oCartElmt.GetAttribute("NonDiscountedShippingCost") == "0")
                                {
                                    IsPromocodeValid = true;
                                }
                            }

                            Int32 nShipMethod = (int)oRow["nShippingMethodId"];
                            Int32 nCartStatus = (int)oRow["nCartStatus"];

                            if ((nShipMethod == 0 && nCartStatus != 5) | IsPromocodeValid == true)
                            {
                                Boolean bGetLowest = true;
                                // TS added to recalculate shipping cost !!!!!!
                                shipCost = -1;


                                if (!string.IsNullOrEmpty(oCartElmt.GetAttribute("bDiscountIsPercent")))
                                {
                                    shipCost = -1;
                                }

                                // Default Shipping Country.
                                string cDestinationCountry = moCartConfig["DefaultCountry"];

                                string cDestinationPostalCode = "";
                                if (oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country") != null)
                                {
                                    cDestinationCountry = oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText;
                                    cDestinationPostalCode = oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/PostalCode").InnerText;
                                    //  bGetLowest = false;
                                }
                                double lowestShipCost = 0;
                                if (!string.IsNullOrEmpty(cDestinationCountry))
                                {
                                    // Go and collect the valid shipping options available for this order
                                    int productId = 0;

                                    var oDsShipOptions = getValidShippingOptionsDS(cDestinationCountry, cDestinationPostalCode, total, quant, weight, cPromoCode, productId);
                                    if (oDsShipOptions != null)
                                    {
                                        foreach (DataRow oRowSO in oDsShipOptions.Tables[0].Rows)
                                        {


                                            // Robust, null/DBNull-safe one-liner replacement
                                            shipCost = Convert.ToDouble(oRowSO["nShipOptCost"]?.ToString() ?? "0");
                                            if (lowestShipCost == 0)
                                            {
                                                lowestShipCost = shipCost;
                                            }

                                            bool bCollection = false;
                                            if (!(oRowSO["bCollection"] is DBNull))
                                            {
                                                bCollection = Convert.ToBoolean(oRowSO["bCollection"]);
                                            }
                                            if (oRowSO.Table.Columns.Contains("nShippingGroup"))
                                            {
                                                if (Convert.ToString(oRowSO["nShippingGroup"]) != "")
                                                {
                                                    ShippingOptionKey = Convert.ToInt64(oRowSO["nShipOptKey"]);
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(moCartConfig["DefaultShippingMethod"]))
                                            {
                                                // logic to overide below...
                                                // Add extra condition for checking shipping delievry method set by default
                                                if (ShippingOptionKey != Convert.ToDouble(moCartConfig["DefaultShippingMethod"]))
                                                {
                                                    if (oCartElmt.HasAttribute("shippingType") & oCartElmt.GetAttribute("shippingType") == "0")
                                                    {
                                                        if (Convert.ToString(oRowSO["nShipOptKey"]) == Convert.ToString(ShippingOptionKey))
                                                        {
                                                            string shipOptName = oRowSO["cShipOptName"] == DBNull.Value ? "" : Convert.ToString(oRowSO["cShipOptName"]);
                                                            // Compact Convert variant with DBNull guard
                                                            string shipOptCarrier = oRowSO["cShipOptCarrier"] == DBNull.Value ? "" : Convert.ToString(oRowSO["cShipOptCarrier"]);
                                                            oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig["DefaultCountry"]);
                                                            oCartElmt.SetAttribute("shippingType", ShippingOptionKey + "");
                                                            oCartElmt.SetAttribute("shippingCost", shipCost + "");
                                                            oCartElmt.SetAttribute("shippingDesc", Convert.ToString(shipOptName));
                                                            oCartElmt.SetAttribute("shippingCarrier", Convert.ToString(shipOptCarrier));
                                                            // oCartElmt.SetAttribute("cCatSchemaName", cCartType & "")
                                                        }
                                                    }
                                                }
                                                else if (oCartElmt.HasAttribute("shippingType") & oCartElmt.GetAttribute("shippingType") == "0")
                                                {
                                                    if (Convert.ToString(oRowSO["nShipOptKey"]) == moCartConfig["DefaultShippingMethod"])
                                                    {
                                                        string shipOptName = oRowSO["cShipOptName"] == DBNull.Value ? "" : Convert.ToString(oRowSO["cShipOptName"]);
                                                        // Compact Convert variant with DBNull guard
                                                        string shipOptCarrier = oRowSO["cShipOptCarrier"] == DBNull.Value ? "" : Convert.ToString(oRowSO["cShipOptCarrier"]);
                                                        oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig["DefaultCountry"]);
                                                        oCartElmt.SetAttribute("shippingType", moCartConfig["DefaultShippingMethod"] + "");
                                                        oCartElmt.SetAttribute("shippingCost", shipCost + "");
                                                        oCartElmt.SetAttribute("shippingDesc", Convert.ToString(shipOptName));
                                                        oCartElmt.SetAttribute("shippingCarrier", Convert.ToString(shipOptCarrier));
                                                    }
                                                }
                                                // Add extra condition only when promocode is valid
                                                // Set nondiscountedshippingcost to attribute when promocode is valid(include free shipping methods)
                                                else if (IsPromocodeValid = true & Convert.ToString(oRowSO["NonDiscountedShippingCost"]) != "0")
                                                {
                                                    if (oCartElmt.GetAttribute("freeShippingMethods").Contains(oCartElmt.GetAttribute("shippingType")))
                                                    {
                                                        string shipOptName = oRowSO["cShipOptName"] == DBNull.Value ? "" : Convert.ToString(oRowSO["cShipOptName"]);
                                                        // Compact Convert variant with DBNull guard
                                                        string shipOptCarrier = oRowSO["cShipOptCarrier"] == DBNull.Value ? "" : Convert.ToString(oRowSO["cShipOptCarrier"]);
                                                        oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig["DefaultCountry"]);
                                                        // Safe, DBNull-aware replacement using DataRow.Field<T>() which returns null for DBNull
                                                        oCartElmt.SetAttribute("shippingType", oRowSO.Field<object>("nShipOptKey")?.ToString() ?? "");
                                                        oCartElmt.SetAttribute("shippingCost", shipCost + "");
                                                        oCartElmt.SetAttribute("shippingDesc", Convert.ToString(shipOptName));
                                                        oCartElmt.SetAttribute("shippingCarrier", Convert.ToString(shipOptCarrier));
                                                        //if (Convert.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oRowSO["NonDiscountedShippingCost"], "0", false)))
                                                        //{
                                                        //    oCartElmt.SetAttribute("NonDiscountedShippingCost", Convert.ToString(Operators.ConcatenateObject(oRowSO["NonDiscountedShippingCost"], "")));
                                                        //}
                                                        if ((oRowSO["NonDiscountedShippingCost"]?.ToString() ?? "0") != "0")
                                                        {
                                                            oCartElmt.SetAttribute("NonDiscountedShippingCost", oRowSO["NonDiscountedShippingCost"]?.ToString() ?? "");
                                                        }
                                                    }
                                                }
                                            }
                                            else if ((shipCost == -1 || shipCost <= lowestShipCost) & bCollection == false)
                                            {
                                                string shipOptName = oRowSO["cShipOptName"] == DBNull.Value ? "" : Convert.ToString(oRowSO["cShipOptName"]);
                                                // Compact Convert variant with DBNull guard
                                                string shipOptCarrier = oRowSO["cShipOptCarrier"] == DBNull.Value ? "" : Convert.ToString(oRowSO["cShipOptCarrier"]);
                                                lowestShipCost = shipCost;
                                                oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig["DefaultCountry"]);
                                                oCartElmt.SetAttribute("shippingType", oRowSO.Field<object>("nShipOptKey")?.ToString() ?? "");
                                                oCartElmt.SetAttribute("shippingCost", shipCost + "");
                                                oCartElmt.SetAttribute("shippingDesc", Convert.ToString(shipOptName));
                                                oCartElmt.SetAttribute("shippingCarrier", Convert.ToString(shipOptCarrier));
                                                // oCartElmt.SetAttribute("cCatSchemaName", "" & "")
                                            }

                                        }
                                    }
                                }
                                if (bGetLowest)
                                {
                                    shipCost = lowestShipCost;
                                    //code added for delivery promocode if it is applied then price set to 0 for selected delivery option.
                                    //condition for ITB
                                    if (oCartElmt.GetAttribute("freeShippingMethods") != "" && oCartElmt.GetAttribute("freeShippingMethods") != null)
                                    {
                                        if (oCartElmt.GetAttribute("freeShippingMethods").Contains(oCartElmt.GetAttribute("shippingType")))
                                        {
                                            shipCost = 0;
                                        }
                                    }
                                }

                                if (shipCost == -1)
                                    shipCost = 0d;
                            }


                            if (Convert.ToDouble(oCartElmt.GetAttribute("shippingType")) > 0d)
                            {
                                getShippingDetailXml(ref oCartElmt, Convert.ToInt64(oCartElmt.GetAttribute("shippingType")));
                            }

                            vatAmt = updateTotals(ref oCartElmt, total, shipCost, oCartElmt.GetAttribute("shippingType"));

                            // Check if the cart needs to be adjusted for deposits or settlements
                            if (mcDeposit == "on")
                            {
                                double nTotalAmount = total + shipCost + vatAmt;
                                double nPayable = 0.0d;
                                // First check if an inital deposit has been paid
                                // We are still in a payment type of deposit if:
                                // 1. No amount has been received
                                // OR 2. An amount has been received but the cart is still in a status of 10
                                if (oRow["nAmountReceived"] is DBNull)
                                {
                                    // No deposit has been paid yet - let's set the deposit value, if it has been specified
                                    if (!string.IsNullOrEmpty(mcDepositAmount))
                                    {
                                        // we defer to calculating the deposit by line about
                                        if (Convert.ToDouble(mcDepositAmount) == 0d)
                                        {

                                            nPayableAmount = 0d;
                                            if (moDBHelper.checkTableColumnExists("tblCartItem", "nDepositAmount"))
                                            {
                                                foreach (XmlElement oItem in oCartElmt.SelectNodes("Item"))
                                                {
                                                    if (oItem.SelectSingleNode("nDepositAmount") is null)
                                                    {
                                                        nPayableAmount = nPayableAmount + Convert.ToDouble(oItem.GetAttribute("itemTotal")) * Convert.ToInt64(oItem.GetAttribute("quantity"));
                                                    }
                                                    else
                                                    {
                                                        nPayableAmount = nPayableAmount + Convert.ToDouble(oItem.SelectSingleNode("nDepositAmount").InnerText) * Convert.ToInt64(oItem.GetAttribute("quantity"));
                                                    }
                                                }
                                            }


                                            nPayable = nPayableAmount;
                                        }

                                        else if (!string.IsNullOrWhiteSpace(mcDepositAmount) && mcDepositAmount.Trim().EndsWith("%"))
                                        {
                                            // remove '%' and parse the remaining text as a number (invariant culture)
                                            string pctText = mcDepositAmount.Trim().Substring(0, mcDepositAmount.Trim().Length - 1).Trim();
                                            if (double.TryParse(pctText, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, System.Globalization.CultureInfo.InvariantCulture, out double pct))
                                            {
                                                nPayable = nTotalAmount * (pct / 100.0);
                                            }
                                        }
                                        else if (Tools.Number.IsNumeric(mcDepositAmount))
                                            nPayable = Convert.ToDouble(mcDepositAmount);

                                        if (nPayable > nTotalAmount)
                                            nPayable = nTotalAmount;

                                        // Set the Payable Amount
                                        if (nPayable > 0d & nPayable < nTotalAmount)
                                        {
                                            oCartElmt.SetAttribute("payableType", "deposit");
                                            oCartElmt.SetAttribute("payableAmount", nPayable.ToString("F2", CultureInfo.CurrentCulture));
                                            oCartElmt.SetAttribute("paymentMade", "0");
                                        }

                                    }
                                }
                                // A deposit has been paid - should I check if it's the same as the total amount?
                                else if (Tools.Number.IsNumeric(oRow["nAmountReceived"]))
                                {
                                    nPayable = nTotalAmount - Convert.ToDouble(oRow["nAmountReceived"]);
                                    oCartElmt.SetAttribute("payableAmount", nPayable.ToString("F2", System.Globalization.CultureInfo.CurrentCulture));
                                    oCartElmt.SetAttribute("outstandingAmount", nPayable.ToString("F2", System.Globalization.CultureInfo.CurrentCulture));

                                    if (nPayable > 0d)
                                    {
                                    }
                                    // this is a deposit payment
                                    else
                                    {
                                        // this is settling the full amount, deposit items may have been moved to another order
                                        nStatusId = 6L;
                                        mnProcessId = 6;
                                    }
                                    oCartElmt.SetAttribute("paymentMade", Convert.ToDouble(oRow["nAmountReceived"]).ToString("N2"));
                                    oCartElmt.SetAttribute("payableType", "settlement");
                                }

                                // Set the payableType 
                                if (!Tools.Number.IsNumeric(oRow["nAmountReceived"]) && nStatusId != 10L)
                                {
                                    oCartElmt.SetAttribute("payableType", "deposit");
                                }
                                else
                                {
                                    oCartElmt.SetAttribute("payableType", "settlement");
                                }

                                // TS added for additional orders not sure if this will break elsewhere.
                                if (!Tools.Number.IsNumeric(oRow["nAmountReceived"]) && nStatusId == 10L)
                                {
                                    oCartElmt.SetAttribute("payableType", "deposit");
                                }

                                if (nPayable == 0d | nPayable == Convert.ToDouble(oCartElmt.GetAttribute("total")))
                                {
                                    oCartElmt.SetAttribute("payableType", "full");
                                }

                                if (nPayable == 0d)
                                {
                                    oCartElmt.SetAttribute("ReadOnly", "On");
                                }
                            }

                            // Add Any Client Notes                          
                            if (oRow["cClientNotes"] != System.DBNull.Value || oRow["cClientNotes"].ToString() != "")
                            {
                                oElmt = moPageXml.CreateElement("Notes");
                                oElmt.InnerXml = Convert.ToString(oRow["cClientNotes"]);
                                if (Convert.ToString(oElmt.FirstChild) != "")
                                {
                                    if (oElmt.FirstChild.Name == "Notes")
                                    {
                                        XmlElement NewNotes = (XmlElement)oCartElmt.OwnerDocument.ImportNode(oElmt.SelectSingleNode("Notes"), true);
                                        oCartElmt.AppendChild(NewNotes);
                                    }
                                    else
                                    {
                                        oCartElmt.AppendChild(oElmt);
                                    }
                                }
                            }

                            // Add the payment details if we have them
                            if (oRow.Field<int?>("nPayMthdId") > 0)
                            {
                                sSql = "Select * from tblCartPaymentMethod where nPayMthdKey=" + oRow["nPayMthdId"];
                                oDs2 = moDBHelper.GetDataSet(sSql, "Payment", "Cart");
                                oElmt = moPageXml.CreateElement("PaymentDetails");
                                foreach (DataRow currentORow2 in oDs2.Tables["Payment"].Rows)
                                {
                                    oRow2 = currentORow2;
                                    oElmt.InnerXml = Convert.ToString(oRow2["cPayMthdDetailXml"]);
                                    oElmt.SetAttribute("provider", Convert.ToString(oRow2["cPayMthdProviderName"]));
                                    oElmt.SetAttribute("ref", Convert.ToString(oRow2["cPayMthdProviderRef"]));
                                    oElmt.SetAttribute("acct", Convert.ToString(oRow2["cPayMthdAcctName"]));
                                }
                                oCartElmt.AppendChild(oCartElmt.OwnerDocument.ImportNode(oElmt, true));
                            }

                            // Add Delivery Details
                            if (nStatusId == 9L)
                            {
                                sSql = "Select * from tblCartOrderDelivery where nOrderId=" + nCartIdUse;
                                oDs2 = moDBHelper.GetDataSet(sSql, "Delivery", "Details");
                                foreach (DataRow currentORow21 in oDs2.Tables["Delivery"].Rows)
                                {
                                    oRow2 = currentORow21;
                                    oElmt = moPageXml.CreateElement("DeliveryDetails");
                                    oElmt.SetAttribute("carrierName", Convert.ToString(oRow2["cCarrierName"]));
                                    oElmt.SetAttribute("ref", Convert.ToString(oRow2["cCarrierRef"]));
                                    oElmt.SetAttribute("notes", Convert.ToString(oRow2["cCarrierNotes"]));
                                    oElmt.SetAttribute("deliveryDate", XmlDate(oRow2["dExpectedDeliveryDate"]));
                                    oElmt.SetAttribute("collectionDate", XmlDate(oRow2["dCollectionDate"]));
                                    oCartElmt.AppendChild(oCartElmt.OwnerDocument.ImportNode(oElmt, true));
                                }
                                oldCartId = nCartIdUse;
                            }


                            // get earliest event start date
                            if (nStatusId == 10L)
                            {
                                DateTime eventDate;
                                foreach (XmlElement itemElmt in oCartElmt.SelectNodes("Item"))
                                {
                                    if (itemElmt.SelectSingleNode("productDetail/StartDate") != null)
                                    {
                                        eventDate = Convert.ToDateTime(itemElmt.SelectSingleNode("productDetail/StartDate").InnerText);
                                        if (dDueDate == default)
                                        {
                                            dDueDate = eventDate;
                                        }
                                        else if (dDueDate < eventDate)
                                        {
                                            dDueDate = eventDate;
                                        }
                                    }
                                }
                                if (dDueDate != default)
                                {
                                    double DateInterval = 30d;
                                    if (!string.IsNullOrEmpty(moCartConfig["SettlementDays"]))
                                    {
                                        DateInterval = Convert.ToDouble(moCartConfig["SettlementDays"]);
                                    }
                                    dDueDate = dDueDate.AddDays(DateInterval * -1);
                                    oCartElmt.SetAttribute("settlementDueDate", dDueDate.ToString());
                                }

                                string sSql2 = "Select cSettlementId from tblCartOrder where nCartOrderKey=" + nCartIdUse;
                                string settlementId = "" + moDBHelper.ExeProcessSqlScalar(sSql2);
                                oCartElmt.SetAttribute("settlementID", settlementId);

                            }

                            // Ensure we persist the invoice date and ref.
                            if (nStatusId > 6L & string.IsNullOrEmpty(oCartElmt.GetAttribute("InvoiceDate")))
                            {
                                // Persist invoice date and invoice ref
                                var tempInstance = new XmlDocument();
                                tempInstance.LoadXml(myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartOrder, (long)nCartIdUse));
                                XmlElement tempOrder = (XmlElement)tempInstance.SelectSingleNode("descendant-or-self::Order");
                                if (tempOrder != null)
                                {
                                    if (string.IsNullOrEmpty(oCartElmt.GetAttribute("InvoiceDate")) & !string.IsNullOrEmpty(tempOrder.GetAttribute("InvoiceDate")))
                                    {
                                        oCartElmt.SetAttribute("InvoiceDate", tempOrder.GetAttribute("InvoiceDate"));
                                    }
                                    if (string.IsNullOrEmpty(oCartElmt.GetAttribute("InvoiceRef")) & !string.IsNullOrEmpty(tempOrder.GetAttribute("InvoiceRef")))
                                    {
                                        oCartElmt.SetAttribute("InvoiceRef", tempOrder.GetAttribute("InvoiceRef"));
                                    }
                                    tempInstance = null;
                                    tempOrder = null;
                                }

                            }

                        }
                    }

                    // Check for any process reported errors
                    if (!((object)mnProcessError is DBNull))
                        oCartElmt.SetAttribute("errorMsg", mnProcessError.ToString());
                    // Save the data
                    if (Convert.ToBoolean(bCheckSubscriptions))
                    {
                        moSubscription.UpdateSubscriptionsTotals(ref oCartElmt);
                    }

                    mnCartId = (int)oldCartId;

                    if (myWeb.moRequest["refresh"] == "true")
                    {
                        mnCartId = nCartIdUse;
                    }
                   
                    //mnCartId = (int)oldCartId;
                    SaveCartXML(oCartElmt);
                    // mnCartId = nCartIdUse

                    if (moCartConfig["RelatedProductsInCart"] == "On")
                    {
                        var oRelatedElmt = oCartElmt.OwnerDocument.CreateElement("RelatedItems");
                        for (int i = 0, loopTo = oItemList.Count - 1; i <= loopTo; i++)
                            myWeb.moDbHelper.addRelatedContent(ref oRelatedElmt, Convert.ToInt16(oItemList[i]), false);
                        foreach (XmlElement oRelElmt in oRelatedElmt.SelectNodes("Content"))
                        {
                            if (oItemList.ContainsValue(oRelElmt.GetAttribute("id")))
                            {
                                oRelElmt.ParentNode.RemoveChild(oRelElmt);
                            }
                        }
                        if (!string.IsNullOrEmpty(oRelatedElmt.InnerXml))
                            oCartElmt.AppendChild(oRelatedElmt);
                    }

                   // Save cookieFirst consent flag
                    if (mnCartId > 0)
                    {
                       
                            saveCookiesConsent();
                        
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetCart", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }

            }

            private double GetPriceFromContent(int contentId)
            {
                try
                {
                    string sql = @"SELECT CAST(CAST(cContentXmlBrief AS XML).value('(/Content/Prices/Price[@type=""sale""]/text())[1]', 'decimal(18,2)') AS decimal(18,2)) FROM tblContent
                     WHERE nContentKey = " + contentId;
                    var result = moDBHelper.GetDataValue(sql);

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDouble(result);
                    }
                }
                catch
                {
                }
                return 0;
            }

            //this is a method to display wallet buttons on cart screen.
            //input parameter is CartElement which will have values in 
            //node with 'Wallets/Wallet with attributes to it
            // which will be used to get data for rendering button with paymentdetails on cartprocess.xsl

            public bool GetWalletDetails(ref XmlElement oCartElmt)

            {
                try
                {


                    decimal nPaymentAmount = Convert.ToDecimal("0" + oCartElmt.GetAttribute("total"));
                    if (nPaymentAmount <= 0)
                    {
                        return false;
                    }


                    Protean.Cms.Cart.PaymentProviders oEwProv = new Protean.Cms.Cart.PaymentProviders(ref myWeb);

                    XmlElement xElmtPaymentProvider = oEwProv.GetValidPaymentProviders();

                    if (xElmtPaymentProvider != null)
                    {

                        foreach (XmlElement opElmt in xElmtPaymentProvider)
                        {
                            //if (opElmt.GetAttribute("name") == "Pay360")
                            //{
                            //    // Pay360 Google Pay is NOT a wallet provider
                            //    // It is just a payment method inside Pay360
                            //    continue; // Skip wallet logic entirely
                            //}
                            Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                            IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, opElmt.GetAttribute("name"));
                            XmlElement oWallets = oPaymentProv.Activities.GetWalletPaymentDetails(opElmt);
                            //just check if wallets object is empty.
                            if (oWallets != null)
                            {
                                if (oWallets.InnerXml != string.Empty)
                                {
                                    oCartElmt.AppendChild(oCartElmt.OwnerDocument.ImportNode(oWallets, true));
                                }
                            }
                        }

                    }
                    return true;

                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetWalletDetails", ex, myWeb.moCtx, "", "", gbDebug);
                    return false;
                }

            }

            public double updateTotals(ref XmlElement oCartElmt, double total, double shipCost, string ShipMethodId)
            {
                string cProcessInfo = "";
                double vatAmt = 0d;
                double nLineVat;
                try
                {

                    if (mnTaxRate > 0d)
                    {
                        // we calculate vat at the end after we have applied discounts etc
                        foreach (XmlElement oElmt in oCartElmt.SelectNodes("descendant-or-self::Item"))
                        {
                            long nOpPrices = 0L;

                            // get the prices of options to calculate vat
                            // AG - I think this is a mistake: For Each oElmt2 In oCartElmt.SelectNodes("/Item")
                            foreach (XmlElement oElmt2 in oElmt.SelectNodes("Item"))
                                nOpPrices = (long)Math.Round(nOpPrices + Convert.ToDouble(oElmt2.GetAttribute("price")));
                            double nItemDiscount = 0d;

                            double nLineTaxRate = mnTaxRate;
                            if (mbVatOnLine)
                            {
                                nLineTaxRate = Convert.ToDouble(oElmt.GetAttribute("taxRate"));
                            }
                            if (oElmt.SelectSingleNode("productDetail[@overideTaxRate!='']") != null)
                            {
                                XmlElement detailElmt = (XmlElement)oElmt.SelectSingleNode("productDetail");
                                nLineTaxRate = Convert.ToDouble(detailElmt.GetAttribute("overideTaxRate"));
                            }


                            // NB 14th Jan 2010 This doesn't work
                            // It generates a figure that matches oElement.GetAttribute("price") so vat is always 0
                            // even if one of the items is being paid for
                            // to test, 1 high qualifyer, 2 low (so only 1 free)


                            if (oElmt.SelectSingleNode("DiscountItem[@nDiscountCat='4']") != null)
                            {

                                XmlElement oDiscItem = (XmlElement)oElmt.SelectSingleNode("DiscountItem");
                                if (oElmt.SelectSingleNode("Discount[@nDiscountCat='4' and @bDiscountIsPercent='1' and  number(@nDiscountValue)=100]") != null)
                                {
                                    // checks for a 100% off discount
                                    nLineVat = (double)Round((Convert.ToDouble(oElmt.GetAttribute("price")) - nItemDiscount + nOpPrices) * (nLineTaxRate / 100d), bForceRoundup: mbRoundup) * Convert.ToDouble(oDiscItem.GetAttribute("Units"));
                                }
                                else
                                {
                                    // nLineVat = Round((oElmt.GetAttribute("price") - nItemDiscount + nOpPrices) * (mnTaxRate / 100), , , mbRoundup) * oDiscItem.GetAttribute("Units")

                                    nLineVat = (double)Round(Convert.ToDouble(oDiscItem.GetAttribute("Total")) * (nLineTaxRate / 100d), bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown);

                                    // nLineVat = 5000
                                }
                            }


                            // 
                            // NB 15th Jan: What is this use of this line? It's logic makes one of the multipliers 0 thus destroying VAT?
                            // Replaced with basic unit VAT change, charge per unit but reduce number of units to only those being paid for
                            // nItemDiscount = oDiscItem.GetAttribute("TotalSaving") / (oDiscItem.GetAttribute("Units") - oDiscItem.GetAttribute("oldUnits")) * -1

                            // NB: 15-01-2010 Moved into Else so Buy X Get Y Free do not use this, as they can get 0
                            else if (mbVatAtUnit)
                            {
                                // Round( Price * Vat ) * Quantity
                                nLineVat = (double)Round((Convert.ToDouble(oElmt.GetAttribute("price")) - nItemDiscount + nOpPrices) * (nLineTaxRate / 100d), bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown) * Convert.ToDouble(oElmt.GetAttribute("quantity"));
                            }
                            else
                            {
                                // Round( ( Price * Quantity )* VAT )
                                nLineVat = (double)Round((Convert.ToDouble(oElmt.GetAttribute("price")) - nItemDiscount + nOpPrices) * Convert.ToDouble(oElmt.GetAttribute("quantity")) * (nLineTaxRate / 100d), bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown);
                            }


                            oElmt.SetAttribute("itemTax", nLineVat.ToString());
                            vatAmt += nLineVat;
                        }

                        if ((moCartConfig["DontTaxShipping"]?.ToString().ToLower() ?? "") != "on")
                        {
                            vatAmt = (double)(Round(shipCost * (mnTaxRate / 100d), bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown) + Round(vatAmt, bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown));
                        }

                        oCartElmt.SetAttribute("totalNet", (total + shipCost).ToString("F2", CultureInfo.CurrentCulture));
                        oCartElmt.SetAttribute("vatRate", mnTaxRate.ToString());
                        oCartElmt.SetAttribute("shippingType", ShipMethodId + "");
                        oCartElmt.SetAttribute("shippingCost", shipCost.ToString("F2", CultureInfo.CurrentCulture));
                        oCartElmt.SetAttribute("vatAmt", vatAmt.ToString("F2", CultureInfo.CurrentCulture));
                        oCartElmt.SetAttribute("total", (total + shipCost + vatAmt).ToString("F2", CultureInfo.CurrentCulture));
                        oCartElmt.SetAttribute("currency", mcCurrencyCode);
                        oCartElmt.SetAttribute("currencySymbol", mcCurrencySymbol);
                    }
                    else
                    {
                        oCartElmt.SetAttribute("totalNet", (total + shipCost).ToString("F2", CultureInfo.CurrentCulture));
                        oCartElmt.SetAttribute("vatRate", 0.0d.ToString());
                        oCartElmt.SetAttribute("shippingType", ShipMethodId + "");
                        oCartElmt.SetAttribute("shippingCost", shipCost.ToString("F2", CultureInfo.CurrentCulture));
                        oCartElmt.SetAttribute("vatAmt", 0.0d.ToString());
                        oCartElmt.SetAttribute("total", (total + shipCost).ToString("F2", CultureInfo.CurrentCulture));
                        oCartElmt.SetAttribute("currency", mcCurrencyCode);
                        oCartElmt.SetAttribute("currencySymbol", mcCurrencySymbol);
                    }

                    return vatAmt;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updateTotals", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }

                return default;
            }

            public void getShippingDetailXml(ref XmlElement oCartXml, long nShippingId)
            {
                myWeb.PerfMon.Log("Cart", "getShippingDetailXml");
                string cProcessInfo = "";
                DataSet oDs;
                string sSql = "select cShipOptName as Name, cShipOptCarrier as Carrier, cShipOptTime as DeliveryTime from tblCartShippingMethods where nShipOptKey=" + nShippingId;
                var oXml = new XmlDocument();
                XmlElement oShippingXml;

                try
                {
                    oDs = moDBHelper.GetDataSet(sSql, "Shipping", "Cart");
                    oXml.LoadXml(oDs.GetXml());
                    oDs.EnforceConstraints = false;
                    oShippingXml = oCartXml.OwnerDocument.CreateElement("Cart");
                    oShippingXml.InnerXml = oXml.InnerXml;
                    oCartXml.AppendChild(oShippingXml.FirstChild.FirstChild);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getShippingDetailXml", ex, myWeb.moCtx, "", cProcessInfo, gbDebug);
                }

            }

            public double getProductPricesByXml(string cXml, string cUnit, long nQuantity, string PriceType = "Prices")
            {
                myWeb.PerfMon.Log("Cart", "getProductPricesByXml");
                string cGroupXPath = string.Empty;
                var oProd = moPageXml.CreateElement("product");
                try
                {

                    oProd.InnerXml = cXml;

                    var oThePrice = getContentPricesNode(oProd, cUnit, nQuantity, PriceType);

                    double nPrice = 0.0d;

                    if (Tools.Number.IsNumeric(oThePrice.InnerText))
                    {
                        nPrice = Convert.ToDouble(oThePrice.InnerText);
                    }

                    return nPrice;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getProductPricesByXml", ex, myWeb.moCtx, "", "", gbDebug);
                }

                return default;

            }

            public double getProductTaxRate(XmlElement priceXml)
            {
                myWeb.PerfMon.Log("Cart", "getProductVatRate");
                // string cGroupXPath = "";
                var oProd = moPageXml.CreateNode(XmlNodeType.Document, "", "product");
                try
                {

                    // string vatCode = "";
                    if (mbVatOnLine)
                    {
                        switch (priceXml.GetAttribute("taxCode") ?? "")
                        {

                            case "0":
                                {
                                    return 0d;
                                }
                            case "s":
                                {
                                    return mnTaxRate;
                                }

                            default:
                                {
                                    return mnTaxRate;
                                }
                        }
                    }
                    else
                    {
                        return 0d;
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getProductTaxRate", ex, myWeb.moCtx, "", "", gbDebug);
                    return (double)default;
                }
            }

            public XmlElement getContentPricesNode(XmlElement oContentXml, string cUnit, long nQuantity, string PriceType = "Prices")
            {
                myWeb.PerfMon.Log("Cart", "getContentPricesNode");
                string cGroupXPath = string.Empty;
                XmlNode oDefaultPrice;

                try
                {

                    // Get the Default Price - note if it does not exist, then this is menaingless.
                    oDefaultPrice = oContentXml.SelectSingleNode("Content/" + PriceType + "/Price[@default='true']");
                    // here we are checking if the price is the correct currency
                    // then if we are logged in we will also check if it belongs to
                    // one of the user's groups, just looking for 
                    string cGroups = getGroupsByName();
                    if (string.IsNullOrEmpty(cGroups))
                        cGroups += "default,all";
                    else
                        cGroups += ",default,all";
                    cGroups = " and ( contains(@validGroup,\"" + cGroups.Replace(",", "\") or contains(@validGroup,\"");
                    cGroups += "\") or not(@validGroup) or @validGroup=\"\")";

                    if (!string.IsNullOrEmpty(cUnit))
                    {
                        cGroups = cGroups + "and (@unit=\"" + cUnit + "\")";
                    }
                    // cGroups = ""
                    // Dim cxpath As String = "Content/Prices/Price[(@currency='" & mcCurrency & "') " & cGroups & " ][1]"


                    // Fix for content items that are not Content/Content done for legacy sites such as insure your move 09/06/2015
                    string xPathStart = "Content/";
                    if (oContentXml.FirstChild != null)
                    {
                        if (oContentXml.FirstChild.Name != "Content")
                        {
                            xPathStart = "";
                        }
                    }
                    else
                    {
                        xPathStart = "";
                    }

                    string cxpath = xPathStart + PriceType + "/Price[(@currency=\"" + mcCurrency + "\") " + cGroups + " and node()!=\"\"]"; // need to loop through all just in case we have splits'handled later on though

                    XmlElement oThePrice = (XmlElement)oDefaultPrice;
                    //double nPrice = 0.0d;

                    foreach (XmlElement oPNode in oContentXml.SelectNodes(cxpath))
                    {
                        // need to deal with "in-product" price splits
                        bool bHasSplits = false;
                        bool bValidSplit = false;
                        if (oPNode.HasAttribute("min") | oPNode.HasAttribute("max"))
                        {
                            if (!string.IsNullOrEmpty(oPNode.GetAttribute("min")) & !string.IsNullOrEmpty(oPNode.GetAttribute("max")))
                            {
                                bHasSplits = true;
                                // has a split
                                int nThisMin;
                                if (!int.TryParse(oPNode.GetAttribute("min"), NumberStyles.Integer, CultureInfo.InvariantCulture, out nThisMin))
                                {
                                    nThisMin = 1;
                                }

                                int nThisMax;
                                if (!int.TryParse(oPNode.GetAttribute("max"), NumberStyles.Integer, CultureInfo.InvariantCulture, out nThisMax))
                                {
                                    nThisMax = 0;
                                }
                                if (nThisMin <= nQuantity & (nThisMax >= nQuantity | nThisMax == 0))
                                {
                                    // now we know it is a valid split
                                    bValidSplit = true;
                                }
                            }
                        }
                        if (oThePrice != null)
                        {
                            if (Tools.Number.IsNumeric(oThePrice.InnerText))
                            {
                                // this selects the cheapest price for this user assuming not free
                                if (Tools.Number.IsNumeric(oPNode.InnerText))
                                {
                                    // if OverrideCheapestPrice is "on" - we will ensure that when sales price is greater than rrp - highest(sales) price is considered.
                                    if (!(moCartConfig["OverrideCheapestPrice"] == null) & moCartConfig["OverrideCheapestPrice"] == "on")
                                    {
                                        if (Convert.ToDouble(oPNode.InnerText) < Convert.ToDouble(oThePrice.InnerText) & Convert.ToDouble(oPNode.InnerText) != 0L)
                                        {
                                            string oThePriceType = oThePrice.GetAttribute("type");
                                            string oPNodeType = oPNode.GetAttribute("type");

                                            if (!(oPNodeType == "rrp" & oThePriceType == "sale"))
                                            {
                                                oThePrice = oPNode;
                                            }
                                        }
                                    }
                                    else if (Convert.ToDouble(oPNode.InnerText) < Convert.ToDouble(oThePrice.InnerText) & Convert.ToDouble(oPNode.InnerText) != 0L)
                                    {
                                        oThePrice = oPNode;
                                    }
                                }
                            }
                            else
                            {
                                oThePrice = oPNode;
                            }
                        }
                        else
                        {
                            oThePrice = oPNode;
                        }
                        // if there are splits and this is a valid split then we want to exit
                        if (bHasSplits & bValidSplit)
                            break;
                        // If Not bHasSplits Then Exit For 'we only need the first one if we dont have splits
                    }

                    return oThePrice;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getContentPricesNode", ex, "", "", gbDebug);
                    return null;
                }

            }

            public double getOptionPricesByXml(string cXml, int nGroupIndex, int nOptionIndex)
            {
                myWeb.PerfMon.Log("Cart", "getOptionPricesByXml");
                string cGroupXPath = string.Empty;
                var oProd = moPageXml.CreateNode(XmlNodeType.Document, "", "product");
                XmlNode oDefaultPrice;
                try
                {
                    // Load product xml
                    oProd.InnerXml = cXml;
                    // Get the Default Price - note if it does not exist, then this is menaingless.
                    oDefaultPrice = oProd.SelectSingleNode("Content/Options/OptGroup[" + nGroupIndex + "]/option[" + nOptionIndex + "]/Prices/Price[@default='true']");
                    // here we are checking if the price is the correct currency
                    // then if we are logged in we will also check if it belongs to
                    // one of the user's groups, just looking for 
                    string cGroups = getGroupsByName();
                    if (string.IsNullOrEmpty(cGroups))
                        cGroups += "default,all";
                    else
                        cGroups += ",default,all";
                    cGroups = " and ( contains(@validGroup,\"" + cGroups.Replace(",", "\") or contains(@validGroup,\"");
                    cGroups += "\") or not(@validGroup) or @validGroup=\"\")";

                    string cxpath = "Content/Options/OptGroup[" + nGroupIndex + "]/option[" + nOptionIndex + "]/Prices/Price[(@currency=\"" + mcCurrency + "\") " + cGroups + " ]";

                    XmlElement oThePrice = (XmlElement)oDefaultPrice;
                    double nPrice = 0.0d;
                    foreach (XmlElement oPNode in oProd.SelectNodes(cxpath))
                    {
                        // If Not oThePrice Is Nothing Then
                        // If CDbl(oPNode.InnerText) < CDbl(oThePrice.InnerText) Then
                        // oThePrice = oPNode
                        // nPrice = CDbl(oThePrice.InnerText)
                        // End If
                        // Else
                        // oThePrice = oPNode
                        // nPrice = CDbl(oThePrice.InnerText)
                        // End If
                        if (oThePrice != null)
                        {
                            if (Tools.Number.IsNumeric(oThePrice.InnerText))
                            {
                                if (Convert.ToDouble(oPNode.InnerText) < Convert.ToDouble(oThePrice.InnerText))
                                {
                                    oThePrice = oPNode;
                                    nPrice = Convert.ToDouble(oThePrice.InnerText);
                                }
                            }
                            else
                            {
                                oThePrice = oPNode;
                                nPrice = Convert.ToDouble(oThePrice.InnerText);
                            }
                        }
                        else
                        {
                            oThePrice = oPNode;
                            if (Tools.Number.IsNumeric(oThePrice.InnerText))
                            {
                                nPrice = Convert.ToDouble(oThePrice.InnerText);
                            }
                        }
                    }

                    return nPrice;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getOptionPricesByXml", ex, "", "", gbDebug);
                }

                return default;
            }

            private void CheckQuantities(ref XmlElement oCartElmt, string cProdXml, string cItemQuantity)
            {

                myWeb.PerfMon.Log("Cart", "CheckQuantities");
                // Check each product against max and mins

                try
                {

                    string oErrorMsg = string.Empty;
                    XmlElement oError;
                    XmlElement oMsg;

                    var oProd = moPageXml.CreateNode(XmlNodeType.Document, "", "product");
                    // Load product xml
                    oProd.InnerXml = cProdXml;

                    // Set the error node
                    oError = (XmlElement)oCartElmt.SelectSingleNode("error");

                    if (Tools.Number.IsNumeric(cItemQuantity))
                    {

                        // Check minimum value
                        if (Convert.ToInt64(cItemQuantity) < Convert.ToInt64(getNodeValueByType(ref oProd, "//Quantities/Minimum", XmlDataType.TypeNumber, 0)))
                        {
                            // Minimum has not been matched

                            // Check for existence of error node
                            if (oError is null)
                            {
                                oError = oCartElmt.OwnerDocument.CreateElement("error");
                                oCartElmt.AppendChild(oError);
                            }

                            // Check for existence of msg node for min
                            if (oError.SelectSingleNode("msg[@type='quantity_min']") is null)
                            {
                                oMsg = addElement(ref oError, "msg", "You have not requested enough of one or more products", true);
                                oMsg.SetAttribute("type", "quantity_min");
                            }

                            // Add product specific msg
                            oMsg = addElement(ref oError, "msg", "<strong>" + getNodeValueByType(ref oProd, "/Content/Name", XmlDataType.TypeString, "A product below ") +
     "</strong> requires a quantity equal to or above <em>" +
     getNodeValueByType(ref oProd, "//Quantities/Minimum", XmlDataType.TypeNumber, "an undetermined value (please call for assistance).") +
     "</em>",
     true
 );
                            oMsg.SetAttribute("type", "quantity_min_detail");
                        }

                        // Check maximum value
                        if (Convert.ToInt64(cItemQuantity) > Convert.ToInt64(getNodeValueByType(ref oProd, "//Quantities/Maximum", XmlDataType.TypeNumber, int.MaxValue)))
                        {
                            // Maximum has not been matched

                            // Check for existence of error node
                            if (oError is null)
                            {
                                oError = oCartElmt.OwnerDocument.CreateElement("error");
                                oCartElmt.AppendChild(oError);
                            }

                            // Check for existence of msg node for min
                            if (oError.SelectSingleNode("msg[@type='quantity_max']") is null)
                            {
                                oMsg = addElement(ref oError, "msg", "You have requested too much of one or more products");
                                oMsg.SetAttribute("type", "quantity_max");
                            }

                            // Add product specific msg
                            oMsg = addElement(
     ref oError,
     "msg",
     "<strong>" +
     getNodeValueByType(ref oProd, "/Content/Name", XmlDataType.TypeString, "A product below ") +
     "</strong> requires a quantity equal to or below <em>" +
     getNodeValueByType(ref oProd, "//Quantities/Maximum", XmlDataType.TypeNumber, "an undetermined value (please call for assistance).") +
     "</em>",
     true
 );

                            oMsg.SetAttribute("type", "quantity_max_detail");


                        }

                        // Check bulkunit value
                        int cBulkUnit = Convert.ToInt16(getNodeValueByType(ref oProd, "//Quantities/BulkUnit", XmlDataType.TypeNumber, 0));
                        if (Convert.ToInt64(cItemQuantity) % Convert.ToInt64(getNodeValueByType(ref oProd, "//Quantities/BulkUnit", XmlDataType.TypeNumber, 1)) != 0L)
                        {
                            // Bulk Unit has not been matched
                            // Check for existence of error node
                            if (oError is null)
                            {
                                oError = oCartElmt.OwnerDocument.CreateElement("error");
                                oCartElmt.AppendChild(oError);
                            }

                            // Check for existence of msg node for min
                            if (oError.SelectSingleNode("msg[@type='quantity_mod']") is null)
                            {
                                oMsg = addElement(ref oError, "msg", "One or more products below can only be bought in certain quantities.");
                                oMsg.SetAttribute("type", "quantity_mod");
                            }

                            // Add product specific msg
                            oMsg = addElement(ref oError, "msg", "<strong>" + getNodeValueByType(ref oProd, "/Content/Name", XmlDataType.TypeString, "A product below ") + "</strong> can only be bought in lots of <em>" + getNodeValueByType(ref oProd, "//Quantities/BulkUnit", XmlDataType.TypeNumber, "an undetermined value (please call for assistance).") + "</em>", true);
                            oMsg.SetAttribute("type", "quantity_mod_detail");
                        }
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckQuantities", ex, "", "", gbDebug);
                }

            }


            private void CheckStock(ref XmlElement oCartElmt, string cProdXml, string cItemQuantity)
            {
                myWeb.PerfMon.Log("Cart", "CheckStock");
                var oProd = moPageXml.CreateNode(XmlNodeType.Document, "", "product");
                XmlNode oStock;
                XmlElement oError;
                XmlElement oMsg;
                string cProcessInfo = "";
                long StockLevel = default;
                try
                {
                    // Load product xml
                    oProd.InnerXml = cProdXml;

                    if (oProd.SelectSingleNode("/Content[@ignoreStock='true']") != null)
                    {
                        mbStockControl = false;
                    }
                    else
                    {
                        // Locate the Stock Node
                        oStock = oProd.SelectSingleNode("//Stock/Location[@name='Default']");
                        if (oStock is null)
                        {
                            oStock = oProd.SelectSingleNode("//Stock");
                            if (oStock != null)
                            {
                                if (Tools.Number.IsNumeric(oStock.InnerText))
                                {
                                    StockLevel = Convert.ToInt64(oStock.InnerText);
                                }
                            }
                        }
                        else
                        {
                            // step through locations to get total quantity
                            foreach (XmlNode currentOStock in oProd.SelectNodes("//Stock/Location"))
                            {
                                oStock = currentOStock;
                                if (Tools.Number.IsNumeric(oStock.InnerText))
                                {
                                    if (StockLevel == default)
                                        StockLevel = 0L;
                                    StockLevel = StockLevel + Convert.ToInt64(oStock.InnerText);
                                }
                            }
                        }

                        if (StockLevel != default)
                        {
                            // If the requested quantity is greater than the stock level, add a warning to the cart - only check tihs on an active cart.
                            if (Convert.ToInt64(cItemQuantity) > StockLevel & mnProcessId < 6)
                            {
                                if (oCartElmt.SelectSingleNode("error") is null)
                                    oCartElmt.AppendChild(oCartElmt.OwnerDocument.CreateElement("error"));
                                oError = (XmlElement)oCartElmt.SelectSingleNode("error");
                                oMsg = addElement(ref oError, "msg", "<span class=\"term3080\">You have requested more items than are currently <em>in stock</em> for <strong class=\"product-name\">" + oProd.SelectSingleNode("//Name").InnerText + "</strong> (only <span class=\"quantity-available\">" + oStock.InnerText + "</span> available).</span><br/>", true);
                                oMsg.SetAttribute("type", "stock");
                            }
                        }
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckStock", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void UpdateStockLevels(ref XmlElement oCartElmt)
            {
                myWeb.PerfMon.Log("Cart", "UpdateStockLevels");
                string sSql;
                DataSet oDs;
                var oProd = moPageXml.CreateNode(XmlNodeType.Document, "", "product");
                XmlNode oStock = null;
                var nStockLevel = default(int);
                string cProcessInfo = "";
                try
                {

                    foreach (XmlElement oItem in oCartElmt.SelectNodes("Item"))
                    {
                        if (!string.IsNullOrEmpty(oItem.GetAttribute("contentId")))
                        {
                            sSql = "select * from tblContent where nContentKey=" + oItem.GetAttribute("contentId");
                            oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart");

                            foreach (DataRow oRow in oDs.Tables["Item"].Rows)
                            {
                                //if this is empty it is not a real product like a donation therefore stock control is not relevent.
                                if (Convert.ToString(oRow["cContentXmlDetail"]) != "")
                                {
                                    oProd.InnerXml = Convert.ToString(oRow["cContentXmlDetail"]);
                                    oStock = oProd.SelectSingleNode("//Stock/Location[@name='Default']");
                                    if (oStock is null)
                                    {
                                        oStock = oProd.SelectSingleNode("//Stock");
                                    }
                                }

                                // Ignore empty nodes
                                if (oStock != null)
                                {
                                    // Ignore non-numeric nodes
                                    if (Tools.Number.IsNumeric(oStock.InnerText))
                                    {
                                        nStockLevel = Convert.ToInt16(oStock.InnerText) - Convert.ToInt16(oItem.GetAttribute("quantity"));
                                        // Remember to delete the XmlCache
                                        moDBHelper.DeleteXMLCache();

                                        // Update stock level
                                        if (nStockLevel < 0)
                                            nStockLevel = 0;
                                        oStock.InnerText = nStockLevel.ToString();
                                        oRow["cContentXmlDetail"] = oProd.InnerXml;

                                    }
                                }

                                // For Brief
                                oProd.InnerXml = Convert.ToString(oRow["cContentXmlBrief"]);

                                oStock = null;
                                oStock = oProd.SelectSingleNode("//Stock/Location[@name='Default']");
                                if (oStock is null)
                                {
                                    oStock = oProd.SelectSingleNode("//Stock");
                                }

                                // Ignore empty nodes
                                if (oStock != null)
                                {
                                    // Ignore non-numeric nodes
                                    if (Tools.Number.IsNumeric(oStock.InnerText))
                                    {
                                        oStock.InnerText = nStockLevel.ToString();
                                        oRow["cContentXmlBrief"] = oProd.InnerXml;
                                    }
                                }
                            }
                            moDBHelper.updateDataset(ref oDs, "Item");
                        }
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateStockLevels", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void UpdateGiftListLevels()
            {
                myWeb.PerfMon.Log("Cart", "UpdateGiftListLevels");
                string sSql;

                string cProcessInfo = "";
                try
                {
                    if (mnGiftListId > 0)
                    {
                        // The SQL statement JOINS items in the current cart to items in giftlist (by matching ID_OPTION1_OPTION2), and updates the quantity in giftlist accordingly.
                        // CONVERT is needed to create concat the unique identifier.
                        sSql = "update g " + "set g.nQuantity=(g.nQuantity - o.nQuantity) " + "from tblCartItem o inner join tblCartItem g " + "on (convert(nvarchar,o.nItemId) + '_' + convert(nvarchar,o.cItemOption1) + '_' + convert(nvarchar,o.cItemOption2)) = (convert(nvarchar,g.nItemId) + '_' + convert(nvarchar,g.cItemOption1) + '_' + convert(nvarchar,g.cItemOption2)) " + "where o.nCartOrderId = " + mnCartId + " and g.nCartOrderId = " + mnGiftListId;



                        moDBHelper.ExeProcessSql(sSql);
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateGiftListLevels", ex, "", cProcessInfo, gbDebug);
                }
            }
            // deprecated for the single call function above.
            public void UpdateGiftListLevels_Old()
            {
                myWeb.PerfMon.Log("Cart", "UpdateGiftListLevels_Old");
                string sSql;
                DataSet oDs;
                // Dim oDr2 As SqlDataReader
                decimal nNewQty;

                string cProcessInfo = "";
                try
                {

                    if (mnGiftListId > 0)
                    {

                        sSql = "select * from tblCartItem where nCartOrderId=" + mnCartId;
                        oDs = moDBHelper.GetDataSet(sSql, "tblCartItem");

                        foreach (DataRow oRow in oDs.Tables["tblCartItem"].Rows)
                        {

                            nNewQty = Convert.ToDecimal(oRow["nQuantity"]);

                            sSql = "select * from tblCartItem where nCartOrderId=" + mnGiftListId + " and nItemId =" + oRow["nItemId"] + " and cItemOption1='" + SqlFmt(oRow["cItemOption1"]?.ToString() ?? "") + "' and cItemOption2='" + SqlFmt(oRow["cItemOption2"]?.ToString() ?? "") + "'";

                            using (var oDr2 = moDBHelper.getDataReaderDisposable(sSql)) // Done by nita on 6/7/22
                            {
                                while (oDr2.Read())
                                {
                                    nNewQty = Convert.ToDecimal(oDr2["nQuantity"]) - Convert.ToDecimal(oRow["nQuantity"]);
                                }

                                sSql = "Update tblCartItem set nQuantity = " + nNewQty + " where nCartOrderId=" + mnGiftListId + " and nItemId =" + oRow["nItemId"] + " and cItemOption1='" + SqlFmt(oRow["cItemOption1"]?.ToString() ?? "") + "' and cItemOption2='" + SqlFmt(oRow["cItemOption2"]?.ToString() ?? "") + "'";
                                moDBHelper.ExeProcessSql(sSql);
                            }
                        }

                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateGiftListLevels", ex, "", cProcessInfo, gbDebug);
                }
                finally
                {
                    // oDr2 = Nothing
                    // oDs = Nothing
                }
            }

            public string getGroupsByName()
            {
                myWeb.PerfMon.Log("Cart", "getGroupsByName");
                // !!!!!!!!!!!!!! Should this be in Membership Object???

                string cReturn;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    // Get groups from user ID return A comma separated string?
                    oDs = moDBHelper.GetDataSet("select * from tblDirectory g inner join tblDirectoryRelation r on g.nDirKey = r.nDirParentId where r.nDirChildId = " + mnEwUserId, "Groups");
                    cReturn = "";

                    if (oDs.Tables["Groups"].Rows.Count > 0)
                    {
                        foreach (DataRow oDr in oDs.Tables["Groups"].Rows)
                            cReturn += "," + (oDr["cDirName"]?.ToString() ?? "");
                        cReturn = cReturn.Length > 1 ? cReturn.Substring(1) : "";
                    }

                    return cReturn;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getGroupsByName", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
            }


            public string getParentCountries(ref string sTarget, ref int nIndex)
            {
                myWeb.PerfMon.Log("Cart", "getParentCountries");
                string sSql;
                Hashtable oLocations;

                int? nTargetId;
                string sCountryList;
                int nLocKey;
                string cProcessInfo = "";
                try
                {

                    // First let's go and get a list of all the countries and their parent id's
                    sSql = "SELECT * FROM tblCartShippingLocations ORDER BY nLocationParId";

                    using (var oDr = moDBHelper.getDataReaderDisposable(sSql))
                    {
                        sCountryList = "";
                        nTargetId = -1;

                        if (oDr.HasRows)
                        {
                            oLocations = new Hashtable();
                            while (oDr.Read())
                            {
                                var arrLoc = new string[4];

                                arrLoc[0] = oDr["nLocationParId"]?.ToString() ?? string.Empty;

                                if (oDr["nLocationTaxRate"] is DBNull | !Tools.Number.IsNumeric(oDr["nLocationTaxRate"]))
                                {
                                    arrLoc[2] = 0.ToString();
                                }
                                else
                                {
                                    arrLoc[2] = Convert.ToString(oDr["nLocationTaxRate"]);
                                }

                                if (oDr["cLocationNameShort"] is DBNull | oDr["cLocationNameShort"] == null)
                                {
                                    arrLoc[1] = Convert.ToString(oDr["cLocationNameFull"]);
                                }
                                else
                                {
                                    arrLoc[1] = Convert.ToString(oDr["cLocationNameShort"]);
                                }
                                nLocKey = Convert.ToInt16(oDr["nLocationKey"]);
                                oLocations[nLocKey] = arrLoc;

                                arrLoc = null;
                                string target = (sTarget ?? "").Trim();
                                // if (Convert.ToBoolean(Operators.OrObject(Operators.ConditionalCompareObjectEqual(Interaction.IIf((oDr["cLocationNameShort"]) is DBNull, "", (oDr["cLocationNameShort"])), Strings.LCase(Strings.Trim(sTarget)), false), Operators.ConditionalCompareObjectEqual(Interaction.IIf((oDr["cLocationNameFull"]) is DBNull, "", (oDr["cLocationNameFull"])), Strings.LCase(Strings.Trim(sTarget)), false))))
                                if (oDr["cLocationNameShort"].ToString() == target || oDr["cLocationNameFull"].ToString() == target)
                                {
                                    nTargetId = Convert.ToInt16(oDr["nLocationKey"]);
                                }

                            }

                            // Iterate through the country list
                            if (nTargetId != -1)
                            {
                                // Get country names
                                sCountryList = iterateCountryList(ref oLocations, ref nTargetId, ref nIndex);
                                sCountryList = "(" + sCountryList.Substring(1) + ")";
                            }

                            oLocations = null;
                        }

                    }

                    // If sCountryList = "" Then
                    // Err.Raise(1004, "getParentCountries", sTarget & " cannot be found as a delivery location, please add via the admin system.")
                    // End If

                    return sCountryList;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getParentCountries", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
            }

            private string iterateCountryList(ref Hashtable oDict, ref int? nParent, ref int nIndex)
            {
                string iterateCountryListRet = default;
                myWeb.PerfMon.Log("Cart", "iterateCountryList");
                string[] arrTmp;
                string sListReturn;
                string cProcessInfo = "";
                try
                {
                    sListReturn = "";

                    if (oDict.ContainsKey(nParent))
                    {
                        arrTmp = (string[])oDict[nParent];
                        sListReturn = ",'" + SqlFmt(arrTmp[nIndex].ToString()) + "'"; // Adding this line here allows the top root location to be added
                        if (arrTmp[0] != null && !Convert.IsDBNull(arrTmp[0]))
                        {
                            if (Int32.Parse("0" + arrTmp[0]) != nParent)
                            {
                                int? newParent = Int32.Parse("0" + arrTmp[0]);
                                sListReturn = sListReturn + iterateCountryList(ref oDict, ref newParent, ref nIndex);
                            }
                        }
                    }
                    iterateCountryListRet = sListReturn;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "iterateCountryList", ex, "", cProcessInfo, gbDebug);
                    return null;
                }

                return iterateCountryListRet;

            }

            public long CreateNewCart(ref XmlElement oCartElmt, string cCartSchemaName = "Order")
            {
                myWeb.PerfMon.Log("Cart", "CreateNewCart");
                // user has started shopping so we need to initialise the cart and add it to the db

                string cProcessInfo = "";
                var oInstance = new XmlDocument();  // Change XmlDataDocument to XmlDocument
                XmlElement oElmt;

                try
                {
                    // stop carts being added by robots
                    if (!string.IsNullOrEmpty(myWeb.moSession["previousPage"]?.ToString()))
                    {

                        oInstance.AppendChild(oInstance.CreateElement("instance"));
                        XmlNode argoNode = oInstance.DocumentElement;
                        oElmt = addNewTextNode("tblCartOrder", ref argoNode);
                        // addNewTextNode("nCartOrderKey", oElmt)
                        XmlNode argoNode1 = oElmt;
                        addNewTextNode("cCurrency", ref argoNode1, mcCurrencyRef);
                        oElmt = (XmlElement)argoNode1;
                        XmlNode argoNode2 = oElmt;
                        addNewTextNode("cCartSiteRef", ref argoNode2, moCartConfig["OrderNoPrefix"]);
                        oElmt = (XmlElement)argoNode2;
                        XmlNode argoNode3 = oElmt;
                        addNewTextNode("cCartForiegnRef", ref argoNode3);
                        oElmt = (XmlElement)argoNode3;
                        XmlNode argoNode4 = oElmt;
                        addNewTextNode("nCartStatus", ref argoNode4, "1");
                        oElmt = (XmlElement)argoNode4;
                        XmlNode argoNode5 = oElmt;
                        addNewTextNode("cCartSchemaName", ref argoNode5, mcOrderType);
                        oElmt = (XmlElement)argoNode5;
                        XmlNode argoNode6 = oElmt;
                        addNewTextNode("cCartSessionId", ref argoNode6, mcSessionId);
                        oElmt = (XmlElement)argoNode6;
                        // MEMB - add userid to oRs if we are logged on
                        if (mnEwUserId > 0)
                        {
                            XmlNode argoNode7 = oElmt;
                            addNewTextNode("nCartUserDirId", ref argoNode7, mnEwUserId.ToString());
                            oElmt = (XmlElement)argoNode7;
                        }
                        else
                        {
                            XmlNode argoNode8 = oElmt;
                            addNewTextNode("nCartUserDirId", ref argoNode8, "0");
                            oElmt = (XmlElement)argoNode8;
                        }
                        XmlNode argoNode9 = oElmt;
                        addNewTextNode("nPayMthdId", ref argoNode9, "0");
                        oElmt = (XmlElement)argoNode9;
                        XmlNode argoNode10 = oElmt;
                        addNewTextNode("cPaymentRef", ref argoNode10);
                        oElmt = (XmlElement)argoNode10;
                        XmlNode argoNode11 = oElmt;
                        addNewTextNode("cCartXml", ref argoNode11);
                        oElmt = (XmlElement)argoNode11;
                        XmlNode argoNode12 = oElmt;
                        addNewTextNode("nShippingMethodId", ref argoNode12, "0");
                        oElmt = (XmlElement)argoNode12;
                        XmlNode argoNode13 = oElmt;
                        addNewTextNode("cShippingDesc", ref argoNode13, moCartConfig["DefaultShippingDesc"]);
                        oElmt = (XmlElement)argoNode13;
                        XmlNode argoNode14 = oElmt;
                        addNewTextNode("nShippingCost", ref argoNode14, Convert.ToInt64(moCartConfig["DefaultShippingCost"] + "0").ToString());
                        oElmt = (XmlElement)argoNode14;
                        XmlNode argoNode15 = oElmt;
                        addNewTextNode("cClientNotes", ref argoNode15, cOrderReference);
                        oElmt = (XmlElement)argoNode15;
                        XmlNode argoNode16 = oElmt;
                        addNewTextNode("cSellerNotes", ref argoNode16, "referer:" + myWeb.moSession["previousPage"]?.ToString() + "\n");
                        oElmt = (XmlElement)argoNode16;
                        if (moPageXml.SelectSingleNode("/Page/Request/GoogleCampaign") != null)
                        {
                            addElement(ref oElmt, "cCampaignCode", moPageXml.SelectSingleNode("/Page/Request/GoogleCampaign").OuterXml, true);
                        }
                        XmlNode argoNode17 = oElmt;
                        addNewTextNode("nTaxRate", ref argoNode17, mnTaxRate.ToString());
                        oElmt = (XmlElement)argoNode17;
                        XmlNode argoNode18 = oElmt;
                        addNewTextNode("nGiftListId", ref argoNode18, "0");
                        oElmt = (XmlElement)argoNode18;
                        XmlNode argoNode19 = oElmt;
                        addNewTextNode("nAuditId", ref argoNode19);
                        oElmt = (XmlElement)argoNode19;
                        // validate column exists then only
                        if (moDBHelper.checkTableColumnExists("tblCartOrder", "nReceiptType"))
                        {
                            XmlNode argoNode20 = oElmt;
                            addNewTextNode("nReceiptType", ref argoNode20, "0");
                            oElmt = (XmlElement)argoNode20;
                        }

                        mnCartId = Convert.ToInt64(moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartOrder, oInstance.DocumentElement));
                        return mnCartId;
                    }
                    else
                    {
                        mnCartId = 0;
                        return mnCartId;
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CreateNewCart", ex, "", cProcessInfo, gbDebug);

                }

                return default;

            }

            public object SetPaymentMethod(long nPayMthdId)
            {
                string sSql = "";
                DataSet oDs;
                string cProcessInfo = "SetPaymentMethod";
                try
                {
                    if (mnCartId > 0)
                    {
                        // Update Seller Notes:
                        sSql = "select * from tblCartOrder where nCartOrderKey = " + mnCartId;
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                            oRow["nPayMthdId"] = nPayMthdId;
                        myWeb.moDbHelper.updateDataset(ref oDs, "Order");
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "SetPaymentMethod", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
            }


            public bool AddItem(long nProductId, long nQuantity, string[][] oProdOptions, string cProductText = "", double nPrice = 0d, string ProductXml = "", bool UniqueProduct = false, string overideUrl = "", bool bDepositOnly = false, string cProductOption = "", double dProductOptionPrice = 0d)
            {
                myWeb.PerfMon.Log("Cart", "AddItem");
                string cSQL = "Select * From tblCartItem WHERE nCartOrderID = " + mnCartId + " AND nItemiD =" + nProductId;
                var oDS = new DataSet();
                DataRow oDR1; // Parent Rows
                              // Child Rows
                long nItemID = 0; // ID of the cart item record
                int nCountExOptions; // number of matching options in the old cart item
                string cProcessInfo = "";
                int NoOptions; // the number of options for the item
                var oProdXml = new XmlDocument();
                string strPrice1;
                long nTaxRate = 0L;
                // Dim giftMessageNode As XmlNode
                short itemLimit = 5000;
                int i;
                try
                {
                    if (!string.IsNullOrEmpty(moCartConfig["ItemLimit"]))
                    {
                        itemLimit = (short)Convert.ToInt16(moCartConfig["ItemLimit"]);
                    }

                    if (nQuantity < itemLimit)
                    {

                        mcBlockCartUpdate = GetBlockCartUpdatesConfig();

                        if (mnProcessId < 5 || string.Equals(mcBlockCartUpdate?.Trim(), "off", StringComparison.OrdinalIgnoreCase))
                        {
                            oDS = moDBHelper.getDataSetForUpdate(cSQL, "CartItems", "Cart");
                            oDS.EnforceConstraints = false;
                            // create relationship
                            oDS.Relations.Add("Rel1", oDS.Tables["CartItems"].Columns["nCartItemKey"], oDS.Tables["CartItems"].Columns["nParentId"], false);
                            oDS.Relations["Rel1"].Nested = true;

                            if (myWeb.moRequest["UniqueProduct"] != null)
                            {

                                UniqueProduct = Convert.ToBoolean(myWeb.moRequest["UniqueProduct"]);

                            }
                            if (myWeb.moRequest["overideUrl"] != null)
                            {

                                overideUrl = Convert.ToString(myWeb.moRequest["overideUrl"]);

                            }

                            // loop through the parent rows to check the product
                            if (oDS.Tables["CartItems"].Rows.Count > 0 & UniqueProduct == false)
                            {

                                foreach (DataRow currentODR1 in oDS.Tables["CartItems"].Rows)
                                {
                                    oDR1 = currentODR1;
                                    if (Convert.ToInt32(moDBHelper.DBN2int(oDR1["nParentId"])) == 0 && oDR1["nItemId"] != null && Convert.ToInt32(oDR1["nItemId"]) == nProductId)
                                    {
                                        nCountExOptions = 0;
                                        NoOptions = 0;
                                        // loop through the children(options) and count how many are the same
                                        foreach (var oDr2 in oDR1.GetChildRows("Rel1"))
                                        {
                                            for (i = 0; i <= oProdOptions.Length - 2; i++)
                                            {
                                                string cProdOpt1 = "";
                                                if (oProdOptions[i].Length > 1)
                                                {
                                                    cProdOpt1 = oProdOptions[i][1].ToString();
                                                }

                                                if (oProdOptions[i].Count() < 1)
                                                {
                                                    // Case for text option with no index
                                                    if (oProdOptions[i][0]?.ToString() == oDr2["nItemOptGrpIdx"]?.ToString())
                                                    {
                                                        nCountExOptions += 1;
                                                    }
                                                }
                                                else if ((oProdOptions[i][0].ToString() != oDr2["nItemOptGrpIdx"].ToString()) && (cProdOpt1.ToString() != oDr2["nItemOptIdx"].ToString()))
                                                    nCountExOptions += 1;
                                            }
                                            NoOptions += 1;
                                        }
                                        if (oProdOptions != null)
                                        {
                                            // if they are all the same then we have the correct record so it is an update
                                            if (nCountExOptions == oProdOptions.Length - 1 && NoOptions == oProdOptions.Length - 1)
                                            {
                                                nItemID = Convert.ToInt64(oDR1["NCartItemKey"]); // ok, got the bugger
                                                break; // exit the loop otherwise we might go through some other ones
                                            }
                                        }

                                        else if (NoOptions == 0)
                                            nItemID = Convert.ToInt64(oDR1["NCartItemKey"]);
                                    }
                                }
                            }
                            if (nItemID == 0)
                            {
                                // New
                                XmlElement oElmt;
                                XmlElement oPrice = null;
                                long nWeight = 0L;

                                XmlDocument oItemInstance = new XmlDocument();
                                oItemInstance.AppendChild(oItemInstance.CreateElement("instance"));
                                XmlNode argoNode = oItemInstance.DocumentElement;
                                oElmt = addNewTextNode("tblCartItem", ref argoNode);
                                addNewTextNode("nCartOrderId", ref oElmt, mnCartId.ToString());
                                addNewTextNode("nItemId", ref oElmt, nProductId.ToString());
                                if (string.IsNullOrEmpty(overideUrl))
                                {
                                    Tools.Xml.addNewTextNode("cItemURL", ref oElmt, myWeb.GetContentUrl(nProductId));
                                }
                                else
                                {
                                    addNewTextNode("cItemURL", ref oElmt, overideUrl);
                                }
                                if (!string.IsNullOrEmpty(ProductXml))
                                {
                                    oProdXml.InnerXml = ProductXml;
                                }
                                else if (nProductId > 0L)
                                {
                                    string cContentType = moDBHelper.ExeProcessSqlScalar("Select cContentSchemaName FROM tblContent WHERE nContentKey = " + nProductId);
                                    string sItemXml = "" + moDBHelper.ExeProcessSqlScalar("Select cContentXmlDetail FROM tblContent WHERE nContentKey = " + nProductId);
                                    if (!string.IsNullOrEmpty(sItemXml))
                                    {
                                        oProdXml.InnerXml = sItemXml;
                                    }
                                    else
                                    {
                                        oProdXml.InnerXml = moDBHelper.ExeProcessSqlScalar("Select cContentXmlBrief FROM tblContent WHERE nContentKey = " + nProductId);
                                    }
                                    if (oProdXml.SelectSingleNode("/Content/StockCode") != null)
                                    {
                                        XmlNode argoNode5 = oElmt;
                                        addNewTextNode("cItemRef", ref argoNode5, oProdXml.SelectSingleNode("/Content/StockCode").InnerText);
                                        oElmt = (XmlElement)argoNode5;
                                    } // @ Where do we get this from?
                                    if (string.IsNullOrEmpty(cProductText))
                                    {
                                        if (oProdXml.SelectSingleNode("/Content/*[1]") != null)
                                        {
                                            cProductText = oProdXml.SelectSingleNode("/Content/*[1]").InnerText;
                                        }
                                        else
                                        {
                                            cProductText = "Donation";
                                        }
                                    }


                                    if (nPrice == 0d)
                                    {
                                        oPrice = this.getContentPricesNode(oProdXml.DocumentElement, myWeb.moRequest["unit"], nQuantity);
                                    }

                                    if (oProdXml.SelectSingleNode("/Content[@overridePrice='true']") != null)
                                    {
                                        mbOveridePrice = true;
                                    }

                                    if (oProdXml.SelectSingleNode("/Content[@ignoreStock='true']") != null)
                                    {
                                        mbStockControl = false;
                                    }

                                    // lets add the discount to the cart if supplied
                                    if (oProdXml.SelectSingleNode("/Content/Prices/Discount[@currency='" + mcCurrency + "']") != null)
                                    {
                                        string strDiscount1 = oProdXml.SelectSingleNode("/Content/Prices/Discount[@currency='" + mcCurrency + "']").InnerText;
                                        addNewTextNode("nDiscountValue", ref oElmt, Tools.Number.IsNumeric(strDiscount1) ? strDiscount1 : "0");
                                    }

                                    if (oProdXml.SelectSingleNode("/Content/ShippingWeight") != null)
                                    {
                                        nWeight = (long)Math.Round(Convert.ToDouble("0" + oProdXml.SelectSingleNode("/Content/ShippingWeight").InnerText));
                                    }

                                    // If (UniqueProduct) Then

                                    // If oProdXml.SelectSingleNode("/Content/GiftMessage") Is Nothing Then
                                    // giftMessageNode = oProdXml.CreateNode(Xml.XmlNodeType.Element, "GiftMessage", "")
                                    // oProdXml.DocumentElement.AppendChild(giftMessageNode)
                                    // Else
                                    // ' sGiftMessage = oProdXml.SelectSingleNode("/Content/GiftMessage").InnerText
                                    // End If
                                    // End If

                                    // Add Parent Product to cart if SKU.add
                                    if (cContentType == "SKU" | cContentType == "Ticket")
                                    {
                                        // Then we need to add the Xml for the ParentProduct.
                                        string sSQL2 = "select TOP 1 nContentParentId from tblContentRelation as a inner join tblAudit as b on a.nAuditId=b.nAuditKey where nContentChildId =" + nProductId + "Order by nContentParentId desc";

                                        long nParentId = Convert.ToInt64(moDBHelper.ExeProcessSqlScalar(sSQL2));
                                        XmlNode argoNode7 = oProdXml.DocumentElement;
                                        var ItemParent = addNewTextNode("ParentProduct", ref argoNode7, "");
                                        XmlElement parentElmt = moDBHelper.GetContentDetailXml(nParentId, true);
                                        if (parentElmt != null)
                                        {
                                            // ItemParent.InnerXml = parentElmt.OuterXml;
                                            if (nPrice != 0)
                                            {
                                                //parentElmt.SetAttribute("overridePrice", "true");
                                                //parentElmt.SelectSingleNode("/Content/Prices/Price[@type='sale']").InnerText = Convert.ToString(nPrice);
                                                parentElmt.SelectSingleNode("Prices/Price[@type='sale']").InnerText = Convert.ToString(nPrice);
                                            }
                                            ItemParent.InnerXml = parentElmt.OuterXml;
                                        }
                                    }

                                    oProdXml.DocumentElement.SetAttribute("type", cContentType);
                                }

                                addNewTextNode("cItemName", ref oElmt, cProductText);
                                addNewTextNode("nItemOptGrpIdx", ref oElmt, 0.ToString());
                                addNewTextNode("nItemOptIdx", ref oElmt, 0.ToString());
                                if (!string.IsNullOrEmpty(myWeb.moRequest["unit"]))
                                {
                                    Tools.Xml.addNewTextNode("cItemUnit", ref oElmt, myWeb.moRequest["unit"]);
                                }
                                if (oPrice != null)
                                {
                                    strPrice1 = oPrice.InnerText;
                                    nTaxRate = (long)Math.Round(getProductTaxRate(oPrice));
                                }
                                else
                                {
                                    strPrice1 = nPrice.ToString();
                                }

                                if (mbOveridePrice)
                                {
                                    if (Convert.ToDouble(myWeb.moRequest["price_" + nProductId]) > 0d)
                                    {
                                        strPrice1 = myWeb.moRequest["price_" + nProductId];
                                    }
                                }
                                addNewTextNode("nPrice", ref oElmt, Tools.Number.IsNumeric(strPrice1) ? strPrice1 : "0");
                                addNewTextNode("nShpCat", ref oElmt, (-1).ToString());
                                addNewTextNode("nTaxRate", ref oElmt, nTaxRate.ToString());
                                addNewTextNode("nQuantity", ref oElmt, nQuantity.ToString());
                                addNewTextNode("nWeight", ref oElmt, nWeight.ToString());
                                addNewTextNode("nParentId", ref oElmt, 0.ToString());
                                if (bDepositOnly)
                                {
                                    XmlNode argoNode18 = oElmt;
                                    addNewTextNode("nDepositAmount", ref argoNode18, Tools.Number.IsNumeric(oPrice.GetAttribute("deposit")) ? oPrice.GetAttribute("deposit") : "0");
                                    oElmt = (XmlElement)argoNode18;
                                }

                                XmlNode argoNode19 = oElmt;
                                var ProductXmlElmt = addNewTextNode("xItemXml", ref argoNode19, "");
                                oElmt = (XmlElement)argoNode19;
                                ProductXmlElmt.InnerXml = oProdXml.DocumentElement.OuterXml;

                                nItemID = Convert.ToInt32(moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement));

                                // Options
                                if (oProdOptions != null)
                                {
                                    for (i = 0; i < oProdOptions.Length; i++)
                                    {
                                        if (oProdOptions[i] != null & nQuantity > 0L)
                                        {
                                            // Add Options
                                            oItemInstance = new XmlDocument();
                                            oItemInstance.AppendChild(oItemInstance.CreateElement("instance"));
                                            XmlElement docElmt = oItemInstance.DocumentElement;
                                            oElmt = addNewTextNode("tblCartItem", ref docElmt);
                                            addNewTextNode("nCartOrderId", ref oElmt, mnCartId.ToString());

                                            string cStockCode = "";
                                            string cOptName = "";
                                            bool bTextOption = false;
                                            // string opt1stval = "";
                                            string opt2ndval = "";
                                            if (oProdOptions[i].Count() == 2)
                                            {
                                                opt2ndval = oProdOptions[i][1].ToString();
                                            }
                                            else
                                            {
                                                // opt2ndval
                                                opt2ndval = "0";
                                            }

                                            if (oProdOptions[i].Count() < 2 && opt2ndval != "0")
                                            {
                                                // This option dosen't have an index value
                                                // Save the submitted value against stock code.
                                                cStockCode = myWeb.moRequest.Form["opt_" + nProductId + "_" + (i + 1)];
                                                cOptName = cStockCode;
                                                bTextOption = true;
                                            }
                                            else if (Tools.Number.IsNumeric(oProdOptions[i][0]) & Tools.Number.IsNumeric(opt2ndval))
                                            {
                                                // add the stock code from the option
                                                if (oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/StockCode") != null)
                                                {
                                                    cStockCode = oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/StockCode").InnerText;
                                                }
                                                else if (oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/code") != null)
                                                {
                                                    cStockCode = oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/code").InnerText;
                                                }
                                                else if (oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/name") != null)
                                                {
                                                    cStockCode = oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/name").InnerText;
                                                }
                                                // add the name from the option
                                                if (oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/Name") != null)
                                                {
                                                    cOptName = oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/Name").InnerText;
                                                }
                                                else if (oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/name") != null)
                                                {
                                                    cOptName = oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/name").InnerText;
                                                }
                                                else if (oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/@name") != null)
                                                {
                                                    cOptName = oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/@name").InnerText;
                                                }
                                            }
                                            else
                                            {
                                                cStockCode = "";
                                                cOptName = "Invalid Option";
                                            }
                                            addNewTextNode("cItemRef", ref oElmt, cStockCode);
                                            addNewTextNode("nItemId", ref oElmt, nProductId.ToString());
                                            addNewTextNode("cItemURL", ref oElmt, myWeb.mcOriginalURL);
                                            addNewTextNode("cItemName", ref oElmt, cOptName);
                                            XmlElement oItemXml = oElmt.OwnerDocument.CreateElement("xItemXml");
                                            oItemXml.InnerXml = oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]").OuterXml;
                                            oElmt.AppendChild(oItemXml);
                                            if (bTextOption)
                                            {
                                                // save the option index as -1 for text option
                                                addNewTextNode("nItemOptGrpIdx", ref oElmt, (i + 1).ToString());
                                                addNewTextNode("nItemOptIdx", ref oElmt, (-1).ToString());
                                                // No price variation for text options
                                                addNewTextNode("nPrice", ref oElmt, "0");
                                            }
                                            else
                                            {
                                                addNewTextNode("nItemOptGrpIdx", ref oElmt, Convert.ToString(oProdOptions[i][0]));
                                                addNewTextNode("nItemOptIdx", ref oElmt, Convert.ToString(opt2ndval));
                                                XmlElement oPriceElmt = (XmlElement)oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/Prices/Price[@currency='{mcCurrency}']");
                                                string strPrice2 = 0.ToString();
                                                if (oPriceElmt != null)
                                                    strPrice2 = oPriceElmt.InnerText;
                                                addNewTextNode("nPrice", ref oElmt, Tools.Number.IsNumeric(strPrice2) ? strPrice2 : "0");
                                            }
                                            addNewTextNode("nShpCat", ref oElmt, (-1).ToString());
                                            addNewTextNode("nTaxRate", ref oElmt, 0.ToString());
                                            addNewTextNode("nQuantity", ref oElmt, nQuantity.ToString());
                                            addNewTextNode("nWeight", ref oElmt, 0.ToString());
                                            addNewTextNode("nParentId", ref oElmt, nItemID.ToString());
                                            moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement);
                                        }
                                    }
                                }
                                // 
                                if (myWeb.moRequest["OptionName_" + nProductId] != null)
                                {
                                    this.AddProductOption(nItemID, myWeb.moRequest["OptionName_" + nProductId], Convert.ToDouble(myWeb.moRequest["OptionValue_" + nProductId]));
                                }
                                else if (!string.IsNullOrEmpty(cProductOption))
                                {
                                    AddProductOption(nItemID, cProductOption, dProductOptionPrice);
                                }
                            }
                            else
                            {
                                // Existing
                                oDS.Relations.Clear();
                                if (nQuantity <= 0L)
                                {
                                    moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, (long)nItemID, false);
                                }
                                else
                                {

                                    foreach (DataRow currentODR11 in oDS.Tables["CartItems"].Rows)
                                    {
                                        oDR1 = currentODR11;
                                        if (oDR1["nCartItemKey"] != null && Convert.ToInt64(oDR1["nCartItemKey"]).Equals(nItemID))
                                        {
                                            oDR1.BeginEdit();

                                            if (moCartConfig["OverwriteItemQuantity"]?.ToString().ToLower() == "on")
                                            {
                                                oDR1["nQuantity"] = nQuantity;
                                            }
                                            else if (oDR1["nQuantity"] != null && Convert.ToInt32(oDR1["nQuantity"]) + nQuantity < itemLimit)
                                            {
                                                oDR1["nQuantity"] = Convert.ToInt32(oDR1["nQuantity"]) + nQuantity;
                                            }

                                            oDR1.EndEdit();
                                            break;
                                        }
                                    }
                                }
                                moDBHelper.updateDataset(ref oDS, "CartItems");
                            }
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "addItem", ex, "", cProcessInfo, gbDebug);
                }

                return default;
            }

            public virtual bool AddItems()
            {
                bool AddItemsRet = default;
                myWeb.PerfMon.Log("Cart", "AddItems");
                // this function checks for an identical item in the database.
                // If there is, the quantity is increased accordingly.
                // If not, a new item is added to the table
                string cProcessInfo = "Checking Submitted Products and Options"; // Object for product keys/quantittie
                                                                                 // Object for options
                string strAddedProducts = "Start:"; // string of products added
                string[][] oOptions; // an array of option arrays (2 dimensional array)
                int nCurOptNo = 0;
                string[] oCurOpt; // CurrentOption bieng evaluated
                long nProductKey;
                long nQuantity;
                int nI;
                string cReplacementName = "";
                // test string
                // qty_233=1 opt_233_1=1_2,1_3 opt_233_2=1_5
                string cSql;
                DataSet oDs;
                int qtyAdded = 0;
                try
                {
                    if ((moCartConfig["ClearOnAdd"]).ToLower() == "on")
                    {
                        cSql = "select nCartItemKey from tblCartItem where nCartOrderId = " + mnCartId;
                        oDs = moDBHelper.GetDataSet(cSql, "Item");
                        if (oDs.Tables["Item"].Rows.Count > 0)
                        {
                            foreach (DataRow oRow in oDs.Tables["Item"].Rows)
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, Convert.ToInt64(oRow["nCartItemKey"]));
                        }
                    }
                    if ((mmcOrderType?.ToLower() ?? "") == (mcItemOrderType?.ToLower() ?? "")) // test for order?
                    {
                        foreach (string oItem1 in myWeb.moRequest.Form) // Loop for getting products/quants
                        {
                            // set defaults
                            string cProductKey = "";
                            nProductKey = 0L;
                            nQuantity = 0L;
                            oOptions = null;
                            cReplacementName = "";
                            // begin
                            if (oItem1?.ToString().StartsWith("qty_") == true) // check for getting productID and quantity
                            {
                                if (oItem1.ToString().StartsWith("qty_deposit_"))
                                {
                                    cProductKey = oItem1.ToString().Replace("qty_deposit_", "");
                                    if (Tools.Number.IsNumeric(cProductKey))
                                    {
                                        nProductKey = Convert.ToInt64(cProductKey);
                                    }
                                    else
                                    {
                                        // injection attempt don't add to cart
                                        //return false;
                                        return AddItemsRet;
                                    }
                                    mbDepositOnly = true;
                                }
                                else
                                {
                                    cProductKey = oItem1?.ToString().Replace("qty_", "");
                                    if (Tools.Number.IsNumeric(cProductKey))
                                    {
                                        nProductKey = Convert.ToInt64(cProductKey);
                                    }
                                    else
                                    {
                                        // injection attempt don't add to cart
                                        //return false;
                                        return AddItemsRet;
                                    }
                                }

                                cProcessInfo = oItem1 + " = " + myWeb.moRequest.Form.Get(oItem1.ToString());

                                if (Tools.Number.IsNumeric(myWeb.moRequest.Form.Get(oItem1)))
                                {
                                    nQuantity = Convert.ToInt64(myWeb.moRequest.Form.Get(oItem1));
                                }

                                // replacementName
                                if (nQuantity > 0L)
                                {
                                    qtyAdded = (int)(qtyAdded + nQuantity);
                                    // bool bBlockCartAdd = false;
                                    string sBlockCartAddMsg = string.Empty;
                                    if (moSubscription != null)
                                    {
                                        if (moCartConfig["SubsExclusiveOrder"]?.ToLower() == "on")
                                        {

                                            // get contentType to be added
                                            XmlDocument tempProduct = new XmlDocument();
                                            XmlElement rootxml = tempProduct.CreateElement("page");
                                            XmlElement ProductXml = myWeb.GetContentBriefXml(rootxml, nProductKey);
                                            switch (ProductXml.SelectSingleNode("@contentType").InnerText)
                                            {
                                                case "Subscription":




                                                    break;
                                                default:

                                                    break;

                                            }
                                            // if contentType = sub
                                            // if cart contains product then block
                                            sBlockCartAddMsg = "You cannot purchase a subscription and a product as part of the same order. Please complete origional purchase then start again.";
                                            // else
                                        }
                                    }
                                    if (!strAddedProducts.Contains("'" + nProductKey + "'")) // double check we havent added this product
                                    {
                                        foreach (string oItem2 in myWeb.moRequest.Form) // loop through again checking for options
                                        {
                                            if (oItem2 == "replacementName_" + nProductKey)
                                                cReplacementName = myWeb.moRequest.Form.Get(oItem2);

                                            if (oItem2.Contains("_"))
                                            {
                                                var parts = oItem2.Split('_');
                                                if (parts.Length > 1 && parts[0] + "_" + parts[1] == "opt_" + nProductKey) // check it is an option
                                                {
                                                    oCurOpt = myWeb.moRequest.Form.Get(oItem2)?.Split(','); // get array of option in "1_2" format
                                                    for (nI = 0; nI < oCurOpt.Length; nI++) // loop through current options to split into another array
                                                    {
                                                        Array.Resize(ref oOptions, nCurOptNo + 1); // redim the array to new length while preserving the current data
                                                        char[] delimiterChars = { '_' };
                                                        oOptions[nCurOptNo] = oCurOpt[nI].Split(delimiterChars); // split out the arrays of options
                                                        nCurOptNo += 1; // update number of options
                                                    }
                                                } // end option check
                                            }
                                        } // end option loop
                                          // Add Item
                                        if (!string.IsNullOrEmpty(myWeb.moRequest.Form.Get("donationAmount")))
                                        {
                                            if (Tools.Number.IsNumeric(myWeb.moRequest.Form.Get("donationAmount")))
                                            {
                                                string CartItemName = "Donation";
                                                string CartItemXml = "";
                                                if (!string.IsNullOrEmpty(myWeb.moRequest.Form.Get("donationName")))
                                                {
                                                    CartItemName = myWeb.moRequest.Form.Get("donationName");
                                                }
                                                if (!string.IsNullOrEmpty(myWeb.moRequest.Form.Get("donationMessage")))
                                                {
                                                    CartItemXml = "<donation><message>" + myWeb.moRequest.Form.Get("donationMessage") + "</message></donation>";
                                                }

                                                if (!AddItem(nProductKey, nQuantity, oOptions, CartItemName, Convert.ToDouble(myWeb.moRequest.Form.Get("donationAmount")), CartItemXml))
                                                {
                                                    qtyAdded = 0;
                                                }
                                            }
                                        }
                                        else if (!AddItem(nProductKey, nQuantity, oOptions, cReplacementName, bDepositOnly: mbDepositOnly))
                                        {
                                            qtyAdded = 0;
                                        }
                                        // Add Item to "Done" List
                                        strAddedProducts += "'" + nProductKey + "',";
                                    }
                                } // end check for previously added
                            } // end check for item/quant
                        } // End Loop for getting products/quants
                        if (qtyAdded > 0)
                        {
                            AddItemsRet = true;
                        }
                        else
                        {
                            AddItemsRet = false;
                        }
                    }
                    else
                    {
                        AddItemsRet = false;
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "addItems", ex, "", cProcessInfo, gbDebug);
                    return false;
                }

                return AddItemsRet;

            }

            public int RemoveItem(long nItemId = 0L, long nContentId = 0L)
            {

                mcBlockCartUpdate = GetBlockCartUpdatesConfig();

                if (mnProcessId > 4 && !string.Equals(mcBlockCartUpdate?.Trim(), "off", StringComparison.OrdinalIgnoreCase))
                {
                    return 1;
                }

                else
                {

                    myWeb.PerfMon.Log("Cart", "RemoveItem");
                    // deletes record from item table in db
                    string sSql;
                    DataSet oDs;
                    string cProcessInfo = "";
                    var itemCount = default(long);
                    if (Tools.Number.IsNumeric(myWeb.moRequest["id"]))
                        nItemId = Convert.ToInt64(myWeb.moRequest["id"]);
                    try
                    {
                        // If myWeb.moRequest("id") <> "" Then

                        if (nContentId == 0L)
                        {
                            sSql = "select nCartItemKey from tblCartItem where (nCartItemKey = " + nItemId + " Or nParentId = " + nItemId + ") and nCartOrderId = " + mnCartId;
                        }
                        else
                        {
                            sSql = "select nCartItemKey from tblCartItem where nItemId = " + nContentId + " and nCartOrderId = " + mnCartId;
                        }


                        oDs = moDBHelper.GetDataSet(sSql, "Item");
                        if (oDs.Tables["Item"].Rows.Count > 0)
                        {
                            foreach (DataRow oRow in oDs.Tables["Item"].Rows)
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, Convert.ToInt64(oRow["nCartItemKey"]));
                        }


                        // REturn the cart order item count
                        sSql = "select count(*) As ItemCount from tblCartItem where nCartOrderId = " + mnCartId;
                        // oDr = moDBHelper.getDataReader(sSql)
                        using (var oDr = moDBHelper.getDataReaderDisposable(sSql))
                        {
                            if (oDr.HasRows)
                            {
                                while (oDr.Read())
                                    itemCount = Convert.ToInt16(oDr["ItemCount"]);
                            }

                            // oDr.Close()
                            // oDr = Nothing

                            return (int)itemCount;
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "removeItem", ex, "", cProcessInfo, gbDebug);
                    }
                }

                return default;
            }

            public int UpdateItem(long nItemId = 0L, long nContentId = 0L, long qty = 1L, bool SkipPackaging = false)
            {
                myWeb.PerfMon.Log("Cart", "RemoveItem");
                // deletes record from item table in db

                // Dim oDr As SqlDataReader
                string sSql;
                DataSet oDs;
                DataRow oRow;
                string cProcessInfo = "";
                var itemCount = default(long);
                try
                {

                    if ((moCartConfig["ClearOnAdd"]).ToLower() == "on")
                    {
                        string cSql = "select nCartItemKey from tblCartItem where nCartOrderId = " + mnCartId;
                        oDs = moDBHelper.GetDataSet(cSql, "Item");
                        if (oDs.Tables["Item"].Rows.Count > 0)
                        {
                            foreach (DataRow currentORow in oDs.Tables["Item"].Rows)
                            {
                                oRow = currentORow;
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, Convert.ToInt64(oRow["nCartItemKey"]));
                            }
                        }
                    }

                    // If myWeb.moRequest("id") <> "" Then
                    if (qty > 0L)
                    {

                        // sSql = "delete from tblCartItem where nCartItemKey = " & myWeb.moRequest("id") & "and nCartOrderId = " & mnCartId
                        if (nContentId == 0L)
                        {
                            if (SkipPackaging == false)
                            {
                                sSql = "select * from tblCartItem where (nCartItemKey = " + nItemId + " Or nParentId = " + nItemId + ") and nCartOrderId = " + mnCartId;
                            }
                            else
                            {
                                sSql = "select * from tblCartItem where (nCartItemKey = " + nItemId + ") and nCartOrderId = " + mnCartId;
                            }
                        }
                        else
                        {
                            sSql = "select * from tblCartItem where nItemId = " + nContentId + " and nCartOrderId = " + mnCartId;
                        }
                        oDs = moDBHelper.getDataSetForUpdate(sSql, "Item");
                        if (oDs.Tables["Item"].Rows.Count > 0)
                        {
                            foreach (DataRow currentORow1 in oDs.Tables["Item"].Rows)
                            {
                                oRow = currentORow1;
                                oRow["nQuantity"] = qty;
                            }
                        }
                        else
                        {
                            AddItem(nContentId, qty, null);
                        }
                        moDBHelper.updateDataset(ref oDs, "Item");
                        oDs = null;
                    }
                    else
                    {
                        RemoveItem(nItemId, nContentId);
                    }


                    // REturn the cart order item count
                    sSql = "select count(*) As ItemCount from tblCartItem where nCartOrderId = " + mnCartId;
                    using (var oDr = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            while (oDr.Read())
                                itemCount = Convert.ToInt16(oDr["ItemCount"]);
                        }

                    }
                    return (int)itemCount;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "removeItem", ex, "", cProcessInfo, gbDebug);
                }

                return default;

            }


            public void UpdateItemPrice(long nItemId, double nPrice)
            {
                myWeb.PerfMon.Log("Cart", "RemoveItem");
                // deletes record from item table in db

                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    sSql = "select * from tblCartItem where (nCartItemKey = " + nItemId + ") and nCartOrderId = " + mnCartId;

                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Item");
                    if (oDs.Tables["Item"].Rows.Count > 0)
                    {
                        foreach (DataRow oRow in oDs.Tables["Item"].Rows)
                        {
                            oRow["nPrice"] = nPrice;

                            var oContentXml = new XmlDocument();

                            oContentXml.LoadXml(Convert.ToString(oRow["xItemXml"]));
                            XmlElement oRootElmt = (XmlElement)oContentXml.FirstChild;
                            oRootElmt.SetAttribute("overridePrice", "true");

                            // Update SKU sale price in XML
                            XmlNode oSalePriceNode = oContentXml.SelectSingleNode("/Content/Prices/Price[@type='sale']");
                            if (oSalePriceNode != null)
                            {
                                oSalePriceNode.InnerText = nPrice.ToString("0.##");
                            }

                            oRow["xItemXml"] = oContentXml.OuterXml;



                        }
                    }
                    moDBHelper.updateDataset(ref oDs, "Item");
                    oDs = null;
                }



                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "removeItem", ex, "", cProcessInfo, gbDebug);
                }

            }



            /// <summary>
            /// Empties all items in a shopping cart.
            /// </summary>
            /// <remarks></remarks>
            public void EmptyCart()
            {
                myWeb.PerfMon.Log("Cart", "EmptyCart");

                // Dim oDr As SqlDataReader
                string sSql;
                string cProcessInfo = "";
                try
                {
                    // Return the cart order item count
                    sSql = "select nCartItemKey from tblCartItem where nCartOrderId = " + mnCartId;
                    using (var oDr = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            while (oDr.Read())
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, Convert.ToInt64(oDr["nCartItemKey"]));
                        }

                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "EmptyCart", ex, "", cProcessInfo, gbDebug);
                }

            }

            public void UpdateCartDeposit(ref XmlElement oRoot, double nPaymentAmount, string cPaymentType)
            {
                myWeb.PerfMon.Log("Cart", "UpdateCartDeposit");
                // Dim oDr As SqlDataReader
                string sSql;
                double nAmountReceived = 0.0d;
                string cUniqueLink = "";
                string cProcessInfo = "";
                try
                {
                    if (moDBHelper.checkTableColumnExists("tblCartOrder", "nAmountReceived"))
                    {
                        // If the cPaymentType is deposit then we need to make a link, otherwise we need to get the paymentReceived details.
                        if (cPaymentType == "deposit")
                        {
                            // Get the unique link from the cart
                            cUniqueLink = ", cSettlementID='" + oRoot.GetAttribute("settlementID") + "' ";
                        }
                        else
                        {
                            // Get the amount received so far

                            sSql = "select * from tblCartOrder where nCartOrderKey = " + mnCartId;
                            using (var oDr = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                            {
                                if (oDr.HasRows)
                                {
                                    while (oDr.Read())
                                    {
                                        nAmountReceived = Convert.ToDouble("0" + oDr["nAmountReceived"]);
                                        cUniqueLink = ", cSettlementID='OLD_" + oDr["cSettlementID"] + "' ";
                                    }

                                }
                            }
                        }

                        nAmountReceived = nAmountReceived + nPaymentAmount;

                        sSql = "update tblCartOrder set nAmountReceived = " + nAmountReceived + ", nLastPaymentMade= " + nPaymentAmount + cUniqueLink + " where nCartOrderKey = " + mnCartId;
                        moDBHelper.ExeProcessSql(sSql);

                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateCartDeposit", ex, "", cProcessInfo, gbDebug);
                }
                finally
                {
                    // oDr = Nothing
                }

            }

            public void QuitCart()
            {
                myWeb.PerfMon.Log("Cart", "QuitCart");
                // set the cart status to 7

                string sSql;
                string cProcessInfo = "";
                try
                {


                    // Old delete calls - DON't DELETE THE CART, Simply set the status to abandoned.
                    // sSql = "delete from tblCartItem where nCartOrderId = " & mnCartId
                    // oDb.exeProcessSQL sSql, mcEwDataConn
                    // sSql = "delete from tblCartOrder where nCartOrderKey =" & mnCartId
                    // oDb.exeProcessSQL sSql, mcEwDataConn

                    sSql = "update tblCartOrder set nCartStatus = 11 where nCartOrderKey = " + mnCartId;
                    moDBHelper.ExeProcessSql(sSql);
                    mnTaxRate = Convert.ToDouble(moCartConfig["TaxRate"]);

                    myWeb.moSession["mcPaymentMethod"] = (object)null;
                    myWeb.moSession["mmcOrderType"] = (object)null;
                }
                // myWeb.moRequest.Form("ordertype") = Nothing

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "QuitCart", ex, "", cProcessInfo, gbDebug);
                }

            }

            public virtual void EndSession()
            {
                myWeb.PerfMon.Log("Cart", "EndSession");
                string sProcessInfo = string.Empty;
                string sSql;
                string cProcessInfo = "";
                try
                {
                    clearSessionCookie();
                    sSql = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where(nCartOrderKey = " + mnCartId + ")";
                    moDBHelper.ExeProcessSql(sSql);
                    mmcOrderType = "";
                    mnCartId = 0;
                    myWeb.moSession["CartId"] = (object)null;
                    mnTaxRate = Convert.ToDouble(moCartConfig["TaxRate"]);
                    mcPaymentMethod = null;
                    myWeb.moSession["mcPaymentMethod"] = (object)null;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "EndSession", ex, "", cProcessInfo, gbDebug);
                }

            }

            public object updateCart(ref string cSuccessfulCartCmd)
            {
                myWeb.PerfMon.Log("Cart", "updateCart");
                // user can decide to change the quantity of identical items or change the shipping options
                // changes to the database are made in this function

                DataSet oDs;
                DataRow oRow;
                string sSql;
                int nItemCount;

                string cProcessInfo = "";
                try
                {

                    // Go through the items associated with the order
                    sSql = "select * from tblCartItem where nCartOrderId = " + mnCartId;
                    cProcessInfo = sSql;
                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart");
                    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    oDs.Relations.Add("Rel1", oDs.Tables["Item"].Columns["nCartItemKey"], oDs.Tables["Item"].Columns["nParentId"], false);
                    oDs.Relations["Rel1"].Nested = true;
                    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    nItemCount = 0; // nItemCount - keeps a running total (accounting for deletions)

                    bool bNullParentId = false;

                    foreach (DataRow currentORow in oDs.Tables["Item"].Rows)
                    {
                        oRow = currentORow;
                        if (!(oRow.RowState == DataRowState.Deleted))
                        {

                            if (ReferenceEquals(oRow["nParentId"], DBNull.Value) || Convert.ToInt32(oRow["nParentId"]) == 0)
                            {
                                nItemCount = nItemCount + 1;
                                // First check if the quantity is numeric (if not ignore it)
                                string key = "itemId-" + oRow["nCartItemKey"];

                                if (Tools.Number.IsNumeric(myWeb.moRequest[key]))
                                {
                                    short qty = Convert.ToInt16(myWeb.moRequest[key]);

                                    if (qty > 0)
                                    {
                                        oRow["nQuantity"] = qty;
                                    }
                                    else
                                    {
                                        DataRow[] oCRows = oRow.GetChildRows("Rel1");
                                        for (int nDels = 0; nDels <= oCRows.GetUpperBound(0); nDels++)
                                            oCRows[nDels].Delete();

                                        oRow.Delete();
                                        nItemCount--;
                                    }
                                }


                            } // for options
                            else
                            {
                                // ensure any product options keep the same quantity as parent.
                                string parkey = "itemId-" + oRow["nParentId"];
                                if (Tools.Number.IsNumeric(myWeb.moRequest[parkey]))
                                {
                                    short qty = Convert.ToInt16(myWeb.moRequest[parkey]);

                                    if (qty > 0)
                                    {
                                        oRow["nQuantity"] = qty;
                                    }
                                    else
                                    {
                                        DataRow[] oCRows = oRow.GetChildRows("Rel1");
                                        for (int nDels = 0; nDels <= oCRows.GetUpperBound(0); nDels++)
                                            oCRows[nDels].Delete();

                                        oRow.Delete();
                                        nItemCount--;
                                    }
                                }
                            }
                        }
                    }
                    moDBHelper.updateDataset(ref oDs, "Item");

                    // If itemCount is 0 or less Then quit the cart, otherwise update the cart
                    if (nItemCount > 0)
                    {

                        // Get the Cart Order
                        sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                        cProcessInfo = sSql;
                        oDs = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow currentORow1 in oDs.Tables["Order"].Rows)
                        {
                            oRow = currentORow1;
                            oRow.BeginEdit();
                            // update the "cart last update" date
                            moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.Audit, nKey: Convert.ToInt64(oRow["nAuditId"]));

                            // Update the Client notes, only if no separate form
                            if (string.IsNullOrEmpty(mcNotesXForm) & !string.IsNullOrEmpty(myWeb.moRequest["cClientNotes"]))
                            {
                                oRow["cClientNotes"] = myWeb.moRequest["cClientNotes"];
                            }
                            oRow["nCartStatus"] = mnProcessId;

                            // ------------BJR-------------------
                            oRow["cCartSchemaName"] = mcOrderType;
                            // oRow("cClientNotes") = cOrderReference
                            // ----------------------------------
                            if (!Convert.ToString(oRow["cSellerNotes"]).Contains("Referrer: " + myWeb.Referrer) & !string.IsNullOrEmpty(myWeb.Referrer))
                            {
                                oRow["cSellerNotes"] += "/n" + "Referrer: " + myWeb.Referrer + "/n";
                            }
                            oRow.EndEdit();
                        }
                        moDBHelper.updateDataset(ref oDs, "Order");

                        // Set the successful Cart Cmd
                        mcCartCmd = cSuccessfulCartCmd;
                        return cSuccessfulCartCmd;
                    }
                    else
                    {

                        mnProcessId = 0;
                        mcCartCmd = "Quit";
                        // Return Nothing
                        return mcCartCmd;
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateCart", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
            }


            private void AddDeliveryFromGiftList(string nGiftListId)
            {
                myWeb.PerfMon.Log("Cart", "AddDeliveryFromGiftList");
                DataSet oDs;
                var oXml = new XmlDocument();
                string cProcessInfo = "";
                try
                {

                    // does the order allready contain a delivery address?
                    oDs = moDBHelper.GetDataSet("select * from tblCartContact where cContactType='Delivery Address' and nContactParentId > 0 and nContactParentId=" + mnCartId.ToString() + " and nContactParentType=1", "tblCartContact");
                    if (oDs.Tables["tblCartContact"].Rows.Count == 0)
                    {

                        oDs.Dispose();
                        oDs = null;
                        oDs = moDBHelper.GetDataSet("select * from tblCartContact where cContactType='Delivery Address' and nContactParentId > 0 and nContactParentId=" + nGiftListId + " and nContactParentType=1", "tblCartContact");

                        if (oDs.Tables["tblCartContact"].Rows.Count == 1)
                        {

                            oXml.LoadXml(oDs.GetXml());
                            oXml.SelectSingleNode("NewDataSet/tblCartContact/nContactKey").InnerText = "-1";
                            // oXml.SelectSingleNode("NewDataSet/tblCartContact/nContactParentType").InnerText = "1"
                            oXml.SelectSingleNode("NewDataSet/tblCartContact/nContactParentId").InnerText = mnCartId.ToString();

                            var arginstanceElmt = oXml.DocumentElement;
                            moDBHelper.saveInstance(ref arginstanceElmt, "tblCartContact", "nContactKey");

                        }

                    }

                    // OK now add the GiftList Id to the cart
                    mnGiftListId = Convert.ToInt16(nGiftListId);

                    oDs.Dispose();
                    oDs = null;
                    oXml = null;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "AddDeliveryFromGiftList", ex, "", cProcessInfo, gbDebug);
                }

            }

            /// <summary>
            /// Calculates and updates the tax rate.
            /// </summary>
            /// <param name="cContactCountry">Optional: The name of the current current to work out the tax rate for.  If empty, then the default tax rate is assumed.</param>
            /// <remarks>If customer is logged on user and they are in a specified group, then void their tax rate</remarks>
            public void UpdateTaxRate(ref string cContactCountry)
            {
                myWeb.PerfMon.Log("Cart", "UpdateTaxRate");
                string sCountryList;
                string[] aVatRates;
                string cVatExclusionGroup;

                double nUpdateTaxRate;
                double nCurrentTaxRate;
                bool bAllZero;

                string sSql;
                string cProcessInfo = "";
                try
                {

                    // Store the current tax rate for comparison later
                    nCurrentTaxRate = mnTaxRate;
                    nUpdateTaxRate = nCurrentTaxRate;
                    cVatExclusionGroup = "" + moCartConfig["TaxRateExclusionGroupId"];

                    // First check if the user is in a tax exclusion group
                    XmlNodeState localNodeState() { var argoNode = myWeb.moPageXml.DocumentElement; var ret = Tools.Xml.NodeState(ref argoNode, "/Page/User/*[@id='" + cVatExclusionGroup + "']"); return ret; }

                    if (Tools.Number.IsNumeric(cVatExclusionGroup) && Convert.ToInt16(cVatExclusionGroup) > 0 && Convert.ToBoolean(localNodeState()))

                    {
                        cProcessInfo = "User is in Tax Rate exclusion group";
                        nUpdateTaxRate = 0d;
                    }

                    else if (!string.IsNullOrEmpty(cContactCountry))
                    {
                        // First get an iterative list of the location and its parents tax rates
                        int argnIndex = 2;
                        sCountryList = getParentCountries(ref cContactCountry, ref argnIndex);
                        cProcessInfo = "Get tax for:" + cContactCountry;
                        if (string.IsNullOrEmpty(sCountryList))
                        {
                            bAllZero = true;
                        }
                        else
                        {

                            sCountryList = sCountryList.Substring(2, sCountryList.Length - 4);
                            aVatRates = sCountryList.Split(new[] { "','" }, StringSplitOptions.None);
                            Array.Reverse(aVatRates);

                            // go backwards through the list, and use the last non-zero tax rate
                            bAllZero = true;

                            foreach (var cVatRate in aVatRates)
                            {
                                if (Convert.ToDouble(cVatRate) > 0d)
                                {
                                    nUpdateTaxRate = Convert.ToDouble(cVatRate);
                                    bAllZero = false;
                                }
                            }
                        }
                        // If all the countries are 0 then get the tax rate for default country, otherwise set the zero
                        if (bAllZero)
                        {
                            string cDefaultCountry = moCartConfig["DefaultCountry"];
                            if (string.IsNullOrWhiteSpace(sCountryList) & !string.IsNullOrEmpty(cDefaultCountry))
                            {
                                sSql = $"SELECT nLocationTaxRate FROM tblCartShippingLocations WHERE cLocationNameFull='{cDefaultCountry}' OR cLocationNameShort='{cDefaultCountry}'";
                                nUpdateTaxRate = Convert.ToDouble(moDBHelper.ExeProcessSqlScalar(sSql));
                            }
                            else
                            {
                                nUpdateTaxRate = 0d;
                            }
                        }
                    }


                    if (nUpdateTaxRate != nCurrentTaxRate)
                    {
                        // return the (amended) rate to the mnTaxRate global variable.
                        mnTaxRate = nUpdateTaxRate;
                        // update the cart order table with the new tax rate
                        sSql = "update tblCartOrder set nTaxRate = " + mnTaxRate + " where nCartOrderKey=" + mnCartId;
                        moDBHelper.ExeProcessSql(sSql);
                    }
                    myWeb.PerfMon.Log("Cart", "UpdateTaxEnd");
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateTaxRate", ex, "", cProcessInfo, gbDebug);

                }

            }

            public void populateCountriesDropDown(ref Cms.xForm oXform, ref XmlElement oCountriesDropDown, string cAddressType = "", bool IDValues = false)
            {
                myWeb.PerfMon.Log("Cart", "populateCountriesDropDown");
                string sSql;
                // Dim oDr As SqlDataReader
                XmlElement oLoctree;
                XmlElement oLocation;
                string[] arrPreLocs = mcPriorityCountries.Split(',');
                int arrIdx;
                var bPreSelect = default(bool);
                string cProcessInfo = Convert.ToString(string.IsNullOrEmpty(oCountriesDropDown.OuterXml));
                try
                {

                    switch (cAddressType ?? "")
                    {
                        case "Delivery Address":
                            {
                                // Delivery countries are restricted
                                // Go and build a tree of all locations - this will allow us to detect whether or not a country is in iteself or in a zone that has a Shipping Option
                                oLoctree = moPageXml.CreateElement("Contents");
                                ListShippingLocations(ref oLoctree, Convert.ToInt64(false));

                                // Add any priority countries
                                for (arrIdx = 0; arrIdx < arrPreLocs.Length; arrIdx++)
                                {
                                    oLocation = (XmlElement)oLoctree.SelectSingleNode("//TreeItem[@nameShort='" + arrPreLocs[arrIdx] + "']/ancestor-or-self::*[@nOptCount!='0']");

                                    if (oLocation != null)
                                    {
                                        oXform.addOption(ref oCountriesDropDown, arrPreLocs[arrIdx].Trim(), arrPreLocs[arrIdx].Trim());
                                        bPreSelect = true;
                                    }
                                }

                                if (bPreSelect)
                                {
                                    oXform.addOption(ref oCountriesDropDown, "--------", " ");
                                }

                                // Now let's go and get a list of all the COUNTRIES sorted ALPHABETICALLY
                                sSql = "SELECT DISTINCT cLocationNameShort FROM tblCartShippingLocations WHERE nLocationType = 2 ORDER BY cLocationNameShort";
                                using (var oDr = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {

                                    while (oDr.Read())
                                    {
                                        // Let's find the country node
                                        // XPath says "Get the context node for the location I'm looking at.  Does it or its ancestors have an OptCount > 0?

                                        if (oLoctree.SelectSingleNode("//TreeItem[@nameShort=\"" + oDr["cLocationNameShort"] + "\"]/ancestor-or-self::*[@nOptCount!='0']") != null)
                                        {
                                            oXform.addOption(
                                                ref oCountriesDropDown,
                                                oDr["cLocationNameShort"].ToString(),
                                                oDr["cLocationNameShort"].ToString()
                                            );
                                        }
                                    }

                                    oLoctree = null;
                                    oLocation = null;
                                }

                                break;
                            }
                        case "ISOa2":
                            {
                                sSql = "SELECT DISTINCT cLocationNameShort as name, cLocationISOa2 as value FROM tblCartShippingLocations WHERE nLocationType = 2 ORDER BY cLocationNameShort";
                                using (var oDr = moDBHelper.getDataReaderDisposable(sSql))
                                {
                                    oXform.addOptionsFromSqlDataReader(oCountriesDropDown, oDr);
                                }
                                break;
                            }
                        default:
                            {
                                // Not restricted by delivery address - add all countries.
                                for (arrIdx = 0; arrIdx < arrPreLocs.Length; arrIdx++)
                                {
                                    oXform.addOption(ref oCountriesDropDown, arrPreLocs[arrIdx].Trim(), arrPreLocs[arrIdx].Trim());
                                    bPreSelect = true;
                                }

                                if (bPreSelect)
                                {
                                    oXform.addOption(ref oCountriesDropDown, "--------", " ");
                                }
                                if (IDValues)
                                {
                                    sSql = "SELECT DISTINCT cLocationNameShort as name, nLocationKey as value FROM tblCartShippingLocations WHERE nLocationType = 2 ORDER BY cLocationNameShort";
                                }
                                else
                                {
                                    sSql = "SELECT DISTINCT cLocationNameShort as name, cLocationNameShort as value FROM tblCartShippingLocations WHERE nLocationType = 2 ORDER BY cLocationNameShort";

                                }
                                using (var oDr = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {
                                    oXform.addOptionsFromSqlDataReader(oCountriesDropDown, oDr);
                                    // this closes the oDr too
                                }

                                break;
                            }
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "populateCountriesDropDown", ex, "", cProcessInfo, gbDebug);
                }
            }
            public void DoNotesItem(string cAction)
            {
                myWeb.PerfMon.Log("Cart", "DoNotesItem");
                string sSql;
                string cNotes; // xml string value, gets reused
                XmlElement oNoteElmt;
                var oNoteXML = new XmlDocument();
                var oAttribs = new FormResult[1];
                int nQuantity = -1; // making it this number so we know 
                string cProcessInfo = "DoNotesLine";
                int i;
                string cxPath = "";
                var tmpDoc = new XmlDocument();
                try
                {
                    // if there is no active cart object (or quote) we need to make one
                    if (mnCartId == 0)
                    {
                        var otmpcart = new Cart(ref myWeb);

                        XmlElement argoCartElmt = null;
                        mnCartId = (int)otmpcart.CreateNewCart(ref argoCartElmt);
                    }
                    // now we need to get the notes from the cart
                    sSql = "Select cClientNotes from tblCartOrder where nCartOrderKey = " + mnCartId;
                    cNotes = moDBHelper.DBN2Str(moDBHelper.ExeProcessSqlScalar(sSql), false, false);

                    // Check if it is empty
                    if (string.IsNullOrEmpty(cNotes))
                    {
                        // we get the empty notes schema from the notes xForm instance.
                        var oXform = new Cms.xForm(ref myWeb.msException);
                        oXform.moPageXML = moPageXml;
                        oXform.NewFrm("notesForm");
                        if (oXform.load(mcNotesXForm))
                        {
                            oNoteXML.LoadXml(oXform.Instance.InnerXml);
                        }
                        else
                        {
                            // no notes xform is spcificed so create new notes node
                            oNoteElmt = oNoteXML.CreateElement("Notes");
                            oNoteXML.AppendChild(oNoteElmt);
                            oNoteElmt = oNoteXML.CreateElement("Notes");
                            oNoteXML.SelectSingleNode("Notes").AppendChild(oNoteElmt);
                        }
                        oXform = (Cms.xForm)null;
                    }

                    else
                    {
                        oNoteXML.InnerXml = cNotes;
                    }

                    cNotes = "";

                    // now to get on and create our nodes
                    foreach (string oItem in myWeb.moRequest.Form)
                    {
                        if (oItem?.ToString() == "node")
                        {
                            // this is the basic node text
                            cNotes = myWeb.moRequest.Form.Get(oItem.ToString());
                        }
                        else
                        {
                            // this is the rest of the submitted form data
                            // going to be saved as attributes
                            Array.Resize(ref oAttribs, oAttribs.Length + 1);
                            oAttribs[oAttribs.Length - 1] = new FormResult(
                                oItem.ToString(),
                                myWeb.moRequest.Form.Get(oItem.ToString())
                            );
                        }

                    }
                    // creating a temporary xml document so we can turn the notes string
                    // into actual xml
                    tmpDoc.InnerXml = cNotes;
                    oNoteElmt = null;
                    // now we can create an actual node in the main document with the right name
                    oNoteElmt = oNoteXML.CreateElement(tmpDoc.ChildNodes[0].Name);
                    // give it the same xml
                    oNoteElmt.InnerXml = tmpDoc.ChildNodes[0].InnerXml;


                    // set the attributes
                    // and create an xpath excluding the quantity field to see if we have an identical node
                    string cIncludeList = myWeb.moRequest.Form["InputList"];

                    int loopTo = oAttribs.Length - 2;
                    for (i = 0; i <= loopTo; i++)
                    {
                        if (cIncludeList.Contains(oAttribs[i].Name) | string.IsNullOrEmpty(cIncludeList))
                        {
                            oNoteElmt.SetAttribute(oAttribs[i].Name, oAttribs[i].Value);
                            if (!(oAttribs[i].Name == "qty"))
                            {
                                if (!string.IsNullOrEmpty(cxPath))
                                    cxPath += " and ";
                                cxPath += "@" + oAttribs[i].Name + "='" + oAttribs[i].Value + "'";
                            }
                            else
                            {
                                nQuantity = Convert.ToInt16(oAttribs[i].Value);
                            }
                        }
                    }







                    // If Not oAttribs(i).Name = "cartCmd" And Not oAttribs(i).Name = "quoteCmd" Then
                    // finish of the xpath
                    cxPath = "Notes/" + oNoteElmt.Name + "[" + cxPath + "]";
                    // "addNoteLine", "removeNoteLine", "updateNoteLine"
                    // loop through any basic matchest to see if it exisists already
                    foreach (XmlElement oTmpElements in oNoteXML.SelectNodes("descendant-or-self::" + cxPath))
                    {
                        // we have a basic top level match
                        // does it match at a lower level
                        if ((oTmpElements.InnerXml ?? "") == (oNoteElmt.InnerXml ?? ""))
                        {
                            // ok, it matches, do we add to the quantity or remove it
                            if (cAction == "removeNoteLine")
                            {
                                // its a remove
                                // check this
                                oNoteXML.SelectSingleNode("Notes").RemoveChild(oTmpElements);
                                // skip the addition of a notes item
                                goto SaveNotes;
                            }
                            else if (cAction == "updateNoteLine")
                            {
                                // an update
                                oTmpElements.SetAttribute("qty", nQuantity.ToString());
                                // Complete Bodge for keysource
                                string cVAs = myWeb.moRequest.Form["VA"];
                                if (cVAs is null | string.IsNullOrEmpty(cVAs))
                                    cVAs = myWeb.moRequest.Form["Custom_VARating"];
                                int nTotalVA = 0;
                                if (Tools.Number.IsNumeric(cVAs))
                                {
                                    nTotalVA = (int)Math.Round(Convert.ToDecimal(oTmpElements.GetAttribute("qty")) * Convert.ToDecimal(cVAs));
                                    if (nTotalVA > 0)
                                        oTmpElements.SetAttribute("TotalVA", nTotalVA.ToString());
                                }
                                goto SaveNotes;
                            }
                            else if (!(nQuantity == -1))
                            {
                                // its an Add
                                oTmpElements.SetAttribute("qty", (Convert.ToInt16(oTmpElements.GetAttribute("qty")) + nQuantity).ToString());
                                // Complete Bodge for keysource
                                string cVAs = myWeb.moRequest.Form["VA"];
                                if (cVAs is null | string.IsNullOrEmpty(cVAs))
                                    cVAs = myWeb.moRequest.Form["Custom_VARating"];
                                int nTotalVA = 0;
                                if (Tools.Number.IsNumeric(cVAs))
                                {
                                    nTotalVA = (int)Math.Round(Convert.ToDecimal(oTmpElements.GetAttribute("qty")) * Convert.ToDecimal(cVAs));
                                    if (nTotalVA > 0)
                                        oTmpElements.SetAttribute("TotalVA", nTotalVA.ToString());
                                }
                                // skip the addition of a notes item
                                goto SaveNotes;
                            }
                            else
                            {
                                // dont do anything, there is no quantity involved so we just add the item anyway
                            }
                            // there should only be one exact match so no need to go through the rest
                            break;
                        }
                    }


                    // Complete Bodge for keysource
                    string cVAsN = myWeb.moRequest.Form["VA"];
                    if (cVAsN is null | string.IsNullOrEmpty(cVAsN))
                        cVAsN = myWeb.moRequest.Form["Custom_VARating"];
                    int nTotalVAN = 0;
                    if (Tools.Number.IsNumeric(cVAsN))
                    {
                        nTotalVAN = (int)Math.Round(nQuantity * Convert.ToDecimal(cVAsN));
                        if (nTotalVAN > 0)
                            oNoteElmt.SetAttribute("TotalVA", nTotalVAN.ToString());
                    }


                    // we only actually hit this if there is no exact match

                    oNoteXML.DocumentElement.InsertAfter(oNoteElmt, oNoteXML.DocumentElement.LastChild);

                SaveNotes:
                    ;
                    // this is so we can skip the appending of new node
                    // now we just need to update the cart notes and the other 
                    // procedures will do the rest
                    sSql = "UPDATE tblCartOrder SET  cClientNotes = '" + oNoteXML.OuterXml + "' WHERE (nCartOrderKey = " + mnCartId + ")";
                    moDBHelper.ExeProcessSql(sSql);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "addItems", ex, "", cProcessInfo, gbDebug);
                }
            }

            public virtual void MakeCurrent(long nOrderID)
            {
                myWeb.PerfMon.Log("Cart", "MakeCurrent");
                // procedure to make a selected historical
                // order or quote into the currently active one

                var oDS = new DataSet();
                Cart otmpcart = null;
                try
                {


                    if (myWeb.mnUserId == 0)
                        return;
                    if (!(Convert.ToDouble(moDBHelper.ExeProcessSqlScalar("Select nCartUserDirId FROM tblCartOrder WHERE nCartOrderKey = " + nOrderID.ToString())) == (double)mnEwUserId))
                    {
                        return; // else we carry on
                    }
                    if (mnCartId == 0)
                    {
                        // create a new cart
                        otmpcart = new Cart(ref myWeb);

                        XmlElement argoCartElmt = null;
                        otmpcart.CreateNewCart(ref argoCartElmt);
                        mnCartId = otmpcart.mnCartId;
                    }
                    // now add the details to it

                    oDS = moDBHelper.GetDataSet("Select * From tblCartItem WHERE nCartOrderID = " + nOrderID.ToString(), "CartItems");
                    long nParentID;
                    string sSQL;

                    moDBHelper.ReturnNullsEmpty(ref oDS);

                    foreach (DataRow oDR1 in oDS.Tables["CartItems"].Rows)
                    {
                        if (Convert.ToInt32(oDR1["nParentId"]) == 0)
                        {
                            sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " + "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " + "nTaxRate, nQuantity, nWeight, nAuditId) VALUES (";
                            sSQL += mnCartId + ",";
                            sSQL += (oDR1["nItemId"] is DBNull ? "Null" : oDR1["nItemId"].ToString()) + ",";
                            sSQL += (oDR1["nParentId"] is DBNull ? "Null" : oDR1["nParentId"].ToString()) + ",";
                            sSQL += (oDR1["cItemRef"] is DBNull ? "Null" : "'" + oDR1["cItemRef"] + "'") + ",";
                            sSQL += (oDR1["cItemURL"] is DBNull ? "Null" : "'" + oDR1["cItemURL"] + "'") + ",";
                            sSQL += (oDR1["cItemName"] is DBNull ? "Null" : "'" + oDR1["cItemName"] + "'") + ",";
                            sSQL += (oDR1["nItemOptGrpIdx"] is DBNull ? "Null" : oDR1["nItemOptGrpIdx"].ToString()) + ",";
                            sSQL += (oDR1["nItemOptIdx"] is DBNull ? "Null" : oDR1["nItemOptIdx"].ToString()) + ",";
                            sSQL += (oDR1["nPrice"] is DBNull ? "Null" : oDR1["nPrice"].ToString()) + ",";
                            sSQL += (oDR1["nShpCat"] is DBNull ? "Null" : oDR1["nShpCat"].ToString()) + ",";
                            sSQL += (oDR1["nDiscountCat"] is DBNull ? "Null" : oDR1["nDiscountCat"].ToString()) + ",";
                            sSQL += (oDR1["nDiscountValue"] is DBNull ? "Null" : oDR1["nDiscountValue"].ToString()) + ",";
                            sSQL += (oDR1["nTaxRate"] is DBNull ? "Null" : oDR1["nTaxRate"].ToString()) + ",";
                            sSQL += (oDR1["nQuantity"] is DBNull ? "Null" : oDR1["nQuantity"].ToString()) + ",";
                            sSQL += (oDR1["nWeight"] is DBNull ? "Null" : oDR1["nWeight"].ToString()) + ",";
                            sSQL += moDBHelper.getAuditId() + ")";

                            nParentID = Convert.ToInt64(moDBHelper.GetIdInsertSql(sSQL));
                            // now for any children
                            foreach (DataRow oDR2 in oDS.Tables["CartItems"].Rows)
                            {
                                if (Convert.ToInt64(oDR2["nParentId"]) == Convert.ToInt64(oDR1["nCartItemKey"]))
                                {
                                    sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " +
                                           "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " +
                                           "nTaxRate, nQuantity, nWeight, nAuditId) VALUES (" +
                                           mnCartId + "," +
                                           (oDR2["nItemId"] is DBNull ? "Null" : oDR2["nItemId"].ToString()) + "," +
                                           nParentID + "," +
                                           (oDR2["cItemRef"] is DBNull ? "Null" : "'" + oDR2["cItemRef"] + "'") + "," +
                                           (oDR2["cItemURL"] is DBNull ? "Null" : "'" + oDR2["cItemURL"] + "'") + "," +
                                           (oDR2["cItemName"] is DBNull ? "Null" : "'" + oDR2["cItemName"] + "'") + "," +
                                           (oDR2["nItemOptGrpIdx"] is DBNull ? "Null" : oDR2["nItemOptGrpIdx"].ToString()) + "," +
                                           (oDR2["nItemOptIdx"] is DBNull ? "Null" : oDR2["nItemOptIdx"].ToString()) + "," +
                                           (oDR2["nPrice"] is DBNull ? "Null" : oDR2["nPrice"].ToString()) + "," +
                                           (oDR2["nShpCat"] is DBNull ? "Null" : oDR2["nShpCat"].ToString()) + "," +
                                           (oDR2["nDiscountCat"] is DBNull ? "Null" : oDR2["nDiscountCat"].ToString()) + "," +
                                           (oDR2["nDiscountValue"] is DBNull ? "Null" : oDR2["nDiscountValue"].ToString()) + "," +
                                           (oDR2["nTaxRate"] is DBNull ? "Null" : oDR2["nTaxRate"].ToString()) + "," +
                                           (oDR2["nQuantity"] is DBNull ? "Null" : oDR2["nQuantity"].ToString()) + "," +
                                           (oDR2["nWeight"] is DBNull ? "Null" : oDR2["nWeight"].ToString()) + "," +
                                           moDBHelper.getAuditId() + ")";

                                    moDBHelper.GetIdInsertSql(sSQL);

                                    // now for any children
                                }

                            }
                        }
                    }
                    if (otmpcart != null)
                    {
                        otmpcart.mnProcessId = 1;
                        otmpcart.mcCartCmd = "Cart";
                        otmpcart.apply();
                    }

                    // now we need to redirect somewhere?
                    // bRedirect = True
                    myWeb.moResponse.Redirect("?cartCmd=Cart", false);
                    myWeb.moCtx.ApplicationInstance.CompleteRequest();
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "MakeCurrent", ex, "", "", gbDebug);
                }
            }

            public bool DeleteCart(long nOrderID)
            {
                myWeb.PerfMon.Log("Cart", "DeleteCart");
                if (myWeb.mnUserId == 0)
                    return default;
                if (nOrderID <= 0)
                    return default;
                try
                {
                    clearSessionCookie();

                    string cSQL = "Select nCartStatus, nCartUserDirId from tblCartOrder WHERE nCartOrderKey=" + nOrderID;
                    using (var oDR = moDBHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                    {
                        int nStat;
                        var nOwner = default(int);
                        while (oDR.Read())
                        {
                            nStat = Convert.ToInt16(oDR.GetValue(0));
                            nOwner = Convert.ToInt16(oDR.GetValue(1));
                        }

                        // If (nOwner = myWeb.mnUserId And (nStat = 7 Or nStat < 4)) Then moDBHelper.DeleteObject(dbHelper.objectTypes.CartOrder, nOrderID)
                        if (nOwner == myWeb.mnUserId)
                        {
                            moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartOrder, (long)nOrderID);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Delete Cart", ex, "", "", gbDebug);
                }

                return default;
            }

            public void SaveCartXML(XmlElement cartXML, long nCartId = 0L)
            {
                myWeb.PerfMon.Log("Cart", "SaveCartXML");
                if (nCartId == 0L)
                    nCartId = mnCartId;
                try
                {
                    if (nCartId > 0L)
                    {
                        cartXML.SetAttribute("cartId", nCartId.ToString());
                        string sSQL = "Update tblCartOrder SET cCartXML ='" + SqlFmt(cartXML.OuterXml) + "' WHERE nCartOrderKey = " + nCartId;
                        moDBHelper.ExeProcessSql(sSQL);
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "SaveCartXML", ex, "", "", gbDebug);
                }
            }




            /// <summary>
            /// Select currency deals with the workflow around choosing a currency when an item is added to the cart.
            /// </summary>
            /// <returns></returns>
            public bool SelectCurrency()
            {
                if (myWeb.PerfMon != null)
                    myWeb.PerfMon.Log("Cart", "SelectCurrency");
                string cProcessInfo = "";
                try
                {
                    cProcessInfo = "checking of Override";
                    // if we are not looking to switch the currency then
                    // we just check if there are any available
                    string cOverrideCur;
                    cOverrideCur = myWeb.moRequest["Currency"];
                    if (cOverrideCur != null & !string.IsNullOrEmpty(cOverrideCur))
                    {
                        cProcessInfo = "Using Override";
                        mcCurrencyRef = cOverrideCur;
                        GetCurrencyDefinition();
                        myWeb.moSession["bCurrencySelected"] = (object)true;
                        if (mnProcessId >= 1)
                        {
                            if (mnShippingRootId > 0)
                            {
                                mnProcessId = 2;
                                mcCartCmd = "Delivery";
                            }
                            else
                            {
                                mcCartCmd = "Cart";
                            }
                        }
                        else
                        {
                            mcCartCmd = "";
                        }
                        return true;
                    }
                    else
                    {
                        cProcessInfo = "Check to see if already used";
                        if (myWeb.moSession["bCurrencySelected"] is true)
                        {
                            if (mcCartCmd == "Currency")
                            {
                                mcCartCmd = "Cart";
                            }
                            return true;
                        }
                    }

                    cProcessInfo = "Getting Currencies";
                    XmlNode moPaymentCfg;
                    moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                    if (moPaymentCfg is null)
                    {
                        cProcessInfo = "protean/payment Config node is missing";
                        mcCartCmd = "Cart";
                        return true;
                    }

                    // check we have differenct currencies
                    if (moPaymentCfg.SelectSingleNode("currencies/Currency") is null)
                    {
                        // mcCartCmd = "Cart"
                        myWeb.moSession["bCurrencySelected"] = (object)true;
                        return true;
                    }
                    else
                    {
                        var oCurrencies = moPaymentCfg.SelectNodes("currencies/Currency");

                        if (oCurrencies.Count == 1)
                        {
                            XmlElement oCurrency = (XmlElement)oCurrencies[0];
                            myWeb.moSession["cCurrency"] = oCurrency.GetAttribute("ref");

                            // here we need to re-get the cart stuff in the new currency
                            GetCurrencyDefinition();
                            // mcCartCmd = "Cart"
                            myWeb.moSession["bCurrencySelected"] = (object)true;
                            return true;
                        }
                        oCurrencies = null;
                        // If multiple currencies then we need to pick
                    }

                    // If Not bOverride And Not mcCurrencyRef = "" Then Return True

                    if (mcCartCmd == "Currency")
                    {
                        // And mcCurrencyRef = "" Then Return True
                        // get and load the currency selector xform
                        cProcessInfo = "Supplying and Checking Form";
                        var oCForm = new Cms.xForm(ref myWeb);
                        oCForm.NewFrm();
                        oCForm.load("/ewcommon/xforms/cart/CurrencySelector.xml");
                        XmlElement oInputElmt = (XmlElement)oCForm.moXformElmt.SelectSingleNode("group/group/group/select1[@bind='cRef']");
                        XmlElement oCur = (XmlElement)oCForm.Instance.SelectSingleNode("Currency/ref");
                        oCur.InnerText = mcCurrency;
                        foreach (XmlElement oCurrencyElmt in moPaymentCfg.SelectNodes("currencies/Currency"))
                        {
                            // going to need to do something about languages
                            XmlElement oOptionElmt;
                            oOptionElmt = oCForm.addOption(ref oInputElmt, oCurrencyElmt.SelectSingleNode("name").InnerText, oCurrencyElmt.GetAttribute("ref"));
                            //XmlNode argoNode = oOptionElmt;
                            oCForm.addNote(ref oOptionElmt, Protean.xForm.noteTypes.Hint, oCurrencyElmt.SelectSingleNode("description").InnerText);
                            //oOptionElmt = (XmlElement)argoNode;
                        }
                        if (oCForm.isSubmitted())
                        {
                            oCForm.updateInstanceFromRequest();
                            oCForm.addValues();
                            oCForm.validate();
                            if (oCForm.valid)
                            {
                                mcCurrencyRef = oCForm.Instance.SelectSingleNode("Currency/ref").InnerText;
                                myWeb.moSession["cCurrency"] = mcCurrencyRef;
                                // here we need to re-get the cart stuff in the new currency
                                GetCurrencyDefinition();
                                myWeb.moSession["bCurrencySelected"] = (object)true;
                                mcCartCmd = "Cart";
                                return true;
                            }
                        }
                        oCForm.addValues();
                        if (moPageXml.SelectSingleNode("/Page/Contents") != null)
                        {
                            moPageXml.SelectSingleNode("/Page/Contents").AppendChild(moPageXml.ImportNode(oCForm.moXformElmt.CloneNode(true), true));
                        }
                        mcCartCmd = "Currency";
                        return false;
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "SelectCurrency", ex, "", cProcessInfo, gbDebug);
                }

                return default;

            }

            /// <summary>
            /// takes the mcCurrencyRef and sets the currency for the current order 
            /// </summary>
            public void GetCurrencyDefinition()
            {
                if (myWeb.PerfMon != null)
                    myWeb.PerfMon.Log("Cart", "GetCurrencyDefinition");
                string cProcessInfo = "";
                try
                {
                    if (!string.IsNullOrEmpty(mcCurrencyRef) & mcCurrencyRef != null)
                    {

                        XmlNode moPaymentCfg;

                        moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");

                        XmlElement oCurrency = (XmlElement)moPaymentCfg.SelectSingleNode("currencies/Currency[@ref='" + mcCurrencyRef + "']");
                        if (oCurrency is null)
                            return;

                        mcCurrencySymbol = oCurrency.GetAttribute("symbol");

                        mcCurrencyCode = oCurrency.GetAttribute("code");

                        mnShippingRootId = -1;

                        if (Tools.Number.IsNumeric(oCurrency.GetAttribute("ShippingRootId")))
                        {
                            mnShippingRootId = Convert.ToInt16(oCurrency.GetAttribute("ShippingRootId"));
                        }

                        mcCurrency = mcCurrencyCode;

                        myWeb.moSession["cCurrency"] = mcCurrencyRef;
                        myWeb.moSession["mcCurrency"] = mcCurrency;
                        // now update the cart database row
                        string sSQL = "UPDATE tblCartOrder  SET cCurrency = '" + mcCurrency + "' WHERE nCartOrderKey = " + mnCartId;

                        myWeb.moDbHelper.ExeProcessSqlScalar(sSQL);

                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetCurrencyDefinition", ex, vstrFurtherInfo: cProcessInfo, bDebug: gbDebug);
                }
            }


            public XmlElement CartOverview()
            {

                try
                {
                    var oRptElmt = myWeb.moPageXml.CreateElement("CartOverview");



                    return oRptElmt;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartOverview", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }

            }

            public void AddProductOption(Newtonsoft.Json.Linq.JObject jObj)
            {

                try
                {
                    XmlElement oelmt;
                    // Dim cSqlUpdate As String
                    var oItemInstance = new XmlDocument();
                    oItemInstance.AppendChild(oItemInstance.CreateElement("instance"));
                    XmlNode argoNode = oItemInstance.DocumentElement;
                    oelmt = addNewTextNode("tblCartItem", ref argoNode);


                    var json = jObj;

                    long CartItemId = (long)json.SelectToken("CartItemId");
                    long ReplaceId = (long)json.SelectToken("ReplaceId");
                    string OptionName = (string)json.SelectToken("ItemName");
                    int ShippingKey = Convert.ToInt32(json.SelectToken("ShippingKey"));

                    if (ReplaceId != 0L)
                    {
                        XmlNode argoNode1 = oelmt;
                        addNewTextNode("nCartItemKey", ref argoNode1, ReplaceId.ToString());
                        oelmt = (XmlElement)argoNode1;
                    }
                    XmlNode argoNode2 = oelmt;
                    addNewTextNode("nCartOrderId", ref argoNode2, mnCartId.ToString());
                    oelmt = (XmlElement)argoNode2;
                    XmlNode argoNode3 = oelmt;
                    addNewTextNode("nItemId", ref argoNode3, (string)json.SelectToken("ItemId"));
                    oelmt = (XmlElement)argoNode3;
                    XmlNode argoNode4 = oelmt;
                    addNewTextNode("cItemURL", ref argoNode4, (string)json.SelectToken("ItemURL"));
                    oelmt = (XmlElement)argoNode4; // Erm?
                    XmlNode argoNode5 = oelmt;
                    addNewTextNode("cItemName", ref argoNode5, OptionName);
                    oelmt = (XmlElement)argoNode5;
                    XmlNode argoNode6 = oelmt;
                    addNewTextNode("nItemOptGrpIdx", ref argoNode6, (string)json.SelectToken("ItemOptGrpIdx"));
                    oelmt = (XmlElement)argoNode6; // Dont Need
                    XmlNode argoNode7 = oelmt;
                    addNewTextNode("nItemOptIdx", ref argoNode7, (string)json.SelectToken("ItemOptIdx"));
                    oelmt = (XmlElement)argoNode7; // Dont Need
                    XmlNode argoNode8 = oelmt;
                    addNewTextNode("cItemRef", ref argoNode8, (string)json.SelectToken("ItemRef"));
                    oelmt = (XmlElement)argoNode8;
                    XmlNode argoNode9 = oelmt;
                    addNewTextNode("nPrice", ref argoNode9, (string)json.SelectToken("Price"));
                    oelmt = (XmlElement)argoNode9;
                    XmlNode argoNode10 = oelmt;
                    addNewTextNode("nShpCat", ref argoNode10, (string)json.SelectToken("ShpCat"));
                    oelmt = (XmlElement)argoNode10;
                    XmlNode argoNode11 = oelmt;
                    addNewTextNode("nDiscountCat", ref argoNode11, (string)json.SelectToken("DiscountCat"));
                    oelmt = (XmlElement)argoNode11;
                    XmlNode argoNode12 = oelmt;
                    addNewTextNode("nDiscountValue", ref argoNode12, (string)json.SelectToken("DiscountValue"));
                    oelmt = (XmlElement)argoNode12;
                    XmlNode argoNode13 = oelmt;
                    addNewTextNode("nTaxRate", ref argoNode13, (string)json.SelectToken("TaxRate"));
                    oelmt = (XmlElement)argoNode13;
                    XmlNode argoNode14 = oelmt;
                    addNewTextNode("nParentId", ref argoNode14, CartItemId.ToString());
                    oelmt = (XmlElement)argoNode14;
                    XmlNode argoNode15 = oelmt;
                    addNewTextNode("cItemUnit", ref argoNode15, (string)json.SelectToken("TaxRate"));
                    oelmt = (XmlElement)argoNode15;
                    XmlNode argoNode16 = oelmt;
                    addNewTextNode("nQuantity", ref argoNode16, (string)json.SelectToken("Qunatity"));
                    oelmt = (XmlElement)argoNode16;
                    XmlNode argoNode17 = oelmt;
                    addNewTextNode("nweight", ref argoNode17, (string)json.SelectToken("Weight"));
                    oelmt = (XmlElement)argoNode17;
                    XmlNode argoNode18 = oelmt;
                    addNewTextNode("xItemXml", ref argoNode18, (string)json.SelectToken("ItemXml"));
                    oelmt = (XmlElement)argoNode18;
                    XmlNode argoNode19 = oelmt;
                    addNewTextNode("nDepositAmount", ref argoNode19, (string)json.SelectToken("DepositAmount"));
                    oelmt = (XmlElement)argoNode19;

                    moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement);


                }
                // UpdatePackagingANdDeliveryType(mnCartId, ShippingKey)
                catch (Exception)
                {
                }
            }

            public void AddProductOption(long nCartItemId, string cOptionName, double nOptionCost)
            {

                try
                {
                    XmlElement oelmt;
                    // Dim cSqlUpdate As String
                    var oItemInstance = new XmlDocument();
                    oItemInstance.AppendChild(oItemInstance.CreateElement("instance"));
                    XmlNode argoNode = oItemInstance.DocumentElement;
                    oelmt = addNewTextNode("tblCartItem", ref argoNode);

                    // Dim json As Newtonsoft.Json.Linq.JObject = jObj

                    // Dim CartItemId As Long = json.SelectToken("CartItemId")
                    // Dim ReplaceId As Long = json.SelectToken("ReplaceId")
                    // Dim OptionName As String = json.SelectToken("ItemName")
                    // Dim ShippingKey As Int32 = Convert.ToInt32(json.SelectToken("ShippingKey"))

                    // If (ReplaceId <> 0) Then
                    // addNewTextNode("nCartItemKey", oelmt, CStr(ReplaceId))
                    // End If
                    XmlNode argoNode1 = oelmt;
                    addNewTextNode("nCartOrderId", ref argoNode1, mnCartId.ToString());
                    oelmt = (XmlElement)argoNode1;
                    XmlNode argoNode2 = oelmt;
                    addNewTextNode("nItemId", ref argoNode2, "0");
                    oelmt = (XmlElement)argoNode2;
                    XmlNode argoNode3 = oelmt;
                    addNewTextNode("cItemURL", ref argoNode3, "");
                    oelmt = (XmlElement)argoNode3; // Erm?
                    XmlNode argoNode4 = oelmt;
                    addNewTextNode("cItemName", ref argoNode4, cOptionName);
                    oelmt = (XmlElement)argoNode4;
                    XmlNode argoNode5 = oelmt;
                    addNewTextNode("nItemOptGrpIdx", ref argoNode5, "0");
                    oelmt = (XmlElement)argoNode5; // Dont Need
                    XmlNode argoNode6 = oelmt;
                    addNewTextNode("nItemOptIdx", ref argoNode6, "0");
                    oelmt = (XmlElement)argoNode6; // Dont Need
                    XmlNode argoNode7 = oelmt;
                    addNewTextNode("cItemRef", ref argoNode7, "0");
                    oelmt = (XmlElement)argoNode7;
                    XmlNode argoNode8 = oelmt;
                    addNewTextNode("nPrice", ref argoNode8, nOptionCost.ToString());
                    oelmt = (XmlElement)argoNode8;
                    XmlNode argoNode9 = oelmt;
                    addNewTextNode("nShpCat", ref argoNode9, "-1");
                    oelmt = (XmlElement)argoNode9;
                    XmlNode argoNode10 = oelmt;
                    addNewTextNode("nDiscountCat", ref argoNode10, "");
                    oelmt = (XmlElement)argoNode10;
                    XmlNode argoNode11 = oelmt;
                    addNewTextNode("nDiscountValue", ref argoNode11, "0.00");
                    oelmt = (XmlElement)argoNode11;
                    XmlNode argoNode12 = oelmt;
                    addNewTextNode("nTaxRate", ref argoNode12, "0");
                    oelmt = (XmlElement)argoNode12;
                    XmlNode argoNode13 = oelmt;
                    addNewTextNode("nParentId", ref argoNode13, nCartItemId.ToString());
                    oelmt = (XmlElement)argoNode13;
                    XmlNode argoNode14 = oelmt;
                    addNewTextNode("cItemUnit", ref argoNode14, "0");
                    oelmt = (XmlElement)argoNode14;
                    XmlNode argoNode15 = oelmt;
                    addNewTextNode("nQuantity", ref argoNode15, "1");
                    oelmt = (XmlElement)argoNode15;
                    XmlNode argoNode16 = oelmt;
                    addNewTextNode("nweight", ref argoNode16, "0");
                    oelmt = (XmlElement)argoNode16;
                    XmlNode argoNode17 = oelmt;
                    addNewTextNode("xItemXml", ref argoNode17, "");
                    oelmt = (XmlElement)argoNode17;

                    moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement);
                }
                // UpdatePackagingANdDeliveryType(mnCartId, ShippingKey)
                catch (Exception)
                {

                }


            }


            // check whether promocode is applier for delivery option
            public string CheckPromocodeAppliedForDelivery()
            {
                string sSql = string.Empty;  // Assign empty value
                string sPromocode = string.Empty;
                DataSet oDs;
                var doc = new XmlDocument();
                string strcFreeShippingMethods = "";
                try
                {
                    // get applied promocode
                    sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                    XmlElement xmlNotes = null;
                    var xmlDoc = new XmlDocument();

                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                    {
                        xmlDoc.LoadXml(Convert.ToString(oRow["cClientNotes"]));
                        xmlNotes = (XmlElement)xmlDoc.SelectSingleNode("Notes/PromotionalCode");
                    }
                    if (xmlNotes != null)
                    {
                        sPromocode = xmlNotes.InnerText;
                    }
                    // check promocode applicable for delivery
                    sSql = "Select cAdditionalXML From tblCartDiscountRules Where cDiscountUserCode = '" + sPromocode + "'";
                    oDs = myWeb.moDbHelper.GetDataSet(sSql.ToString(), "Discount", "Discounts");
                    if (oDs.Tables["Discount"].Rows.Count > 0)
                    {
                        string additionalInfo = "<additionalXml>" + oDs.Tables["Discount"].Rows[0]["cAdditionalXML"] + "</additionalXml>";
                        doc.LoadXml(additionalInfo);

                        if (doc.InnerXml.Contains("cFreeShippingMethods"))
                        {
                            strcFreeShippingMethods = doc.SelectSingleNode("additionalXml").SelectSingleNode("cFreeShippingMethods").InnerText;
                        }
                    }

                    oDs.Clear();
                    oDs = null;

                    return strcFreeShippingMethods;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckPromocodeAppliedForDelivery", ex, "", "", gbDebug);
                    return null;
                }
            }

            public string UpdateDeliveryOptionByCountry(ref XmlElement oCartElmt, string country = "", string cOrderofDeliveryOption = "")
            {
                try
                {
                    // 'check if country is not default country
                    string DeliveryOption = "";
                    var quant = default(long);
                    var oItemList = new Hashtable();
                    var weight = default(double);
                    var total = default(double);
                    DataSet oDs;
                    string cDestinationCountry;
                    bool bChangedDelivery = true;
                    string sSql;
                    string cProcessInfo;
                    var nCheckPrice = default(double);
                    XmlElement oCheckPrice;

                    long nCartIdUse;
                    nCartIdUse = mnCartId;

                    if (moDBHelper.checkTableColumnExists("tblCartItem", "xItemXml"))
                    {
                        sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, i.xItemXml as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, i.xItemXml.value('Content[1]/@type','nvarchar(50)') AS contentType, dbo.fxn_getContentParents(i.nItemId) as parId  from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" + nCartIdUse;
                    }
                    else
                    {
                        sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, p.cContentXmlDetail as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, p.cContentSchemaName AS contentType, dbo.fxn_getContentParents(i.nItemId) as parId  from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" + nCartIdUse;
                    }

                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart");
                    // add relationship for options
                    oDs.Relations.Add("Rel1", oDs.Tables["Item"].Columns["id"], oDs.Tables["Item"].Columns["nParentId"], false);
                    oDs.Relations["Rel1"].Nested = true;
                    // 
                    var revisedPrice = default(double);
                    foreach (DataRow oRow in oDs.Tables["Item"].Rows)
                    {

                        double Discount = 0d;

                        if (!oItemList.ContainsValue(oRow["contentId"]))
                        {
                            oItemList.Add(oItemList.Count, oRow["contentId"]);
                        }
                        long parentId = Convert.ToInt64(moDBHelper.DBN2int(oRow["nParentId"]));
                        if (parentId == 0)
                        {
                            long nTaxRate = 0L;
                            bool bOverridePrice = false;
                            if (!mbOveridePrice) // for openquote
                            {
                                // Go get the lowest price based on user and group
                                if (!(oRow["productDetail"] is DBNull))
                                {

                                    var oProd = moPageXml.CreateElement("product");
                                    oProd.InnerXml = Convert.ToString(oRow["productDetail"]);
                                    if (oProd.SelectSingleNode("Content[@overridePrice='true']") is null)
                                    {
                                        oCheckPrice = getContentPricesNode(oProd, oRow["unit"]?.ToString() ?? "", Convert.ToInt64(oRow["quantity"]));

                                        cProcessInfo = "Error getting price for unit:" + oRow["unit"] +
                                                       " and Quantity:" + oRow["quantity"] +
                                                       " and Currency " + mcCurrencyRef +
                                                       " Check that a price is available for this quantity and a group for this current user.";

                                        if (oCheckPrice != null)
                                        {
                                            nCheckPrice = Convert.ToDouble(oCheckPrice.InnerText);
                                            nTaxRate = (long)Math.Round(getProductTaxRate(oCheckPrice));
                                        }
                                        // nCheckPrice = getProductPricesByXml(oRow("productDetail"), oRow("unit") & "", oRow("quantity"))

                                        if (moSubscription != null && (oRow["contentType"]?.ToString() ?? "") == "Subscription")
                                        {
                                            if (Convert.ToInt32(oRow["contentId"]) > 0)
                                            {
                                                revisedPrice = moSubscription.CartSubscriptionPrice(Convert.ToInt16(oRow["contentId"]), myWeb.mnUserId);
                                            }
                                            else
                                            {
                                                oCheckPrice = getContentPricesNode(oProd, oRow["unit"]?.ToString() ?? "", Convert.ToInt64(oRow["quantity"]), "SubscriptionPrices");
                                                nCheckPrice = Convert.ToDouble(oCheckPrice.InnerText);
                                                nTaxRate = (long)Math.Round(getProductTaxRate(oCheckPrice));
                                            }

                                            if (revisedPrice < nCheckPrice)
                                            {
                                                Discount = nCheckPrice - revisedPrice;
                                                nCheckPrice = revisedPrice;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        bOverridePrice = true;
                                    }

                                }
                                if (!bOverridePrice)
                                {
                                    if (nCheckPrice > 0d && Convert.ToDouble(oRow["price"]) != nCheckPrice)
                                    {
                                        // If price is lower, then update the item price field
                                        oRow["price"] = nCheckPrice;
                                        // oRow["taxRate"] = nTaxRate; // optional if needed here
                                    }

                                    if (Convert.ToDouble(oRow["taxRate"]) != nTaxRate)
                                    {
                                        oRow["taxRate"] = nTaxRate;
                                    }
                                }

                                // option prices
                            }
                            decimal nOpPrices = 0m;
                            foreach (var oOpRow in oRow.GetChildRows("Rel1"))
                            {
                                if (!mbOveridePrice) // for openquote
                                {
                                    decimal nNPrice = (decimal)getOptionPricesByXml(Convert.ToString(oRow["productDetail"]), Convert.ToInt16(oRow["nItemOptGrpIdx"]), Convert.ToInt16(oRow["nItemOptIdx"]));
                                    if (nNPrice > 0m && Convert.ToDecimal(oOpRow["price"]) != nNPrice)
                                    {
                                        nOpPrices += nNPrice;
                                        // oOpRow.BeginEdit()
                                        oOpRow["price"] = nNPrice;
                                    }

                                    // oOpRow.EndEdit()
                                    else
                                    {
                                        nOpPrices = Convert.ToDecimal(nOpPrices + Convert.ToDecimal(oOpRow["price"]));
                                    }
                                }
                            }

                            // Apply stock control
                            if (mbStockControl)
                                CheckStock(ref oCartElmt, Convert.ToString(oRow["productDetail"]), Convert.ToString(oRow["quantity"]));
                            // Apply quantity control
                            if (oRow["productDetail"] != DBNull.Value)
                            {
                                // not sure why the product has no detail but if it not we skip this, suspect it was old test data that raised this issue.
                                CheckQuantities(ref oCartElmt, oRow["productDetail"]?.ToString() ?? "", Convert.ToInt64(oRow["quantity"] ?? 0).ToString());
                            }

                            decimal weightDecimal = Convert.ToDecimal(oRow["weight"]) * Convert.ToDecimal(oRow["quantity"]);
                            decimal totalDecimal = Convert.ToDecimal(oRow["quantity"]) * Round(Convert.ToDecimal(oRow["price"]) + Convert.ToDecimal(nOpPrices), bForceRoundup: mbRoundup);
                            quant += Convert.ToInt64(oRow["quantity"]);

                            weight += (double)weightDecimal; // if weight must remain double
                            total += (double)totalDecimal;   // if total must remain double

                        }
                    }

                    if (!string.IsNullOrEmpty(country))
                    {
                        cDestinationCountry = country;

                        // ' pass other parameters as well-
                        // 'get it from cart
                        // sort dataset for applied delivery option
                        // code commented by sonali to set evoucher for other than uk country
                        // If (oDsShipOptions.Tables(0) IsNot Nothing And cOrderofDeliveryOption = "1") Then
                        // Dim TempTable As New DataTable
                        // Dim dv As DataView
                        // TempTable = oDsShipOptions.Tables(0)
                        // dv = TempTable.DefaultView
                        // ' dv.Sort = " nShippingTotal DESC"
                        // dv.RowFilter = "nShipOptCost > 0"
                        // TempTable = dv.ToTable

                        // oDsShipOptions.Tables(0).Clear()
                        // oDsShipOptions.Tables(0).Merge(TempTable)
                        // End If
                        var oDsShipOptions = getValidShippingOptionsDS(cDestinationCountry, total, quant, weight);
                        foreach (DataRow oRowSO in oDsShipOptions.Tables[0].Rows)
                        {
                            if (bChangedDelivery)
                            {
                                // If (cOrderofDeliveryOption = oRowSO("nShipOptKey")) Then
                                updateGCgetValidShippingOptionsDS(Convert.ToString(oRowSO["nShipOptKey"]));
                                DeliveryOption = Convert.ToString(oRowSO["cShipOptName"]);
                                // pass total item cost including packaging amount
                                DeliveryOption = DeliveryOption + "#" + total.ToString() + "#" + oRowSO["nShipOptKey"]?.ToString();
                                bChangedDelivery = false;
                                // End If
                            }
                        }
                    }


                    return DeliveryOption;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "setDeliveryOptionByCountry", ex, "", "", gbDebug);
                    return null;
                }
            }


            // creating the duplicate order from old order
            public string CreateDuplicateOrder(XmlDocument oldCartxml, long nOrderId, string cMethodName, string cNewAuthNumber)
            {
                try
                {
                    // string cResult = "Success";
                    var oCartListElmt = moPageXml.CreateElement("Order");
                    //GetCart(ref oCartListElmt, nOrderId);
                    // Insert code into tblcartOrder

                    oCartListElmt = (XmlElement)oldCartxml.DocumentElement.Clone();

                    var oInstance = new XmlDocument();
                    XmlElement oElmt;
                    XmlElement oeResponseElmt = (XmlElement)oCartListElmt.SelectSingleNode("/PaymentDetails/instance/Response");
                    string ReceiptId = oCartListElmt.SelectSingleNode("/PaymentDetails/@ref").Value.ToString();
                    double Amount = Convert.ToDouble(oCartListElmt.GetAttribute("total"));
                    //int nItemID = 0; // ID of the cart item record
                    // Dim oDs As DataSet

                    XmlElement oePaymentDetailsInstanceElmt = (XmlElement)oCartListElmt.SelectSingleNode("/PaymentDetails");

                    oInstance.AppendChild(oInstance.CreateElement("instance"));
                    XmlNode argoNode = oInstance.DocumentElement;
                    oElmt = addNewTextNode("tblCartOrder", ref argoNode);
                    XmlNode argoNode1 = oElmt;
                    addNewTextNode("cCurrency", ref argoNode1, oCartListElmt.GetAttribute("currency"));
                    oElmt = (XmlElement)argoNode1;
                    XmlNode argoNode2 = oElmt;
                    addNewTextNode("cCartSiteRef", ref argoNode2, moCartConfig["OrderNoPrefix"]);
                    oElmt = (XmlElement)argoNode2;
                    XmlNode argoNode3 = oElmt;
                    addNewTextNode("cCartForiegnRef", ref argoNode3);
                    oElmt = (XmlElement)argoNode3;
                    XmlNode argoNode4 = oElmt;
                    addNewTextNode("nCartStatus", ref argoNode4, oCartListElmt.GetAttribute("statusId"));
                    oElmt = (XmlElement)argoNode4;
                    XmlNode argoNode5 = oElmt;
                    addNewTextNode("cCartSchemaName", ref argoNode5, "Order");
                    oElmt = (XmlElement)argoNode5;
                    XmlNode argoNode6 = oElmt;
                    addNewTextNode("cCartSessionId", ref argoNode6, oCartListElmt.GetAttribute("session"));
                    oElmt = (XmlElement)argoNode6;

                    XmlNode argoNode7 = oElmt;
                    addNewTextNode("nCartUserDirId", ref argoNode7, "0");
                    oElmt = (XmlElement)argoNode7;
                    XmlNode argoNode8 = oElmt;
                    addNewTextNode("nPayMthdId", ref argoNode8, "0");
                    oElmt = (XmlElement)argoNode8;
                    XmlNode argoNode9 = oElmt;
                    addNewTextNode("cPaymentRef", ref argoNode9);
                    oElmt = (XmlElement)argoNode9;
                    XmlNode argoNode10 = oElmt;
                    addNewTextNode("cCartXml", ref argoNode10);
                    oElmt = (XmlElement)argoNode10;
                    XmlNode argoNode11 = oElmt;
                    addNewTextNode("nShippingMethodId", ref argoNode11, oCartListElmt.GetAttribute("shippingType"));
                    oElmt = (XmlElement)argoNode11;
                    XmlNode argoNode12 = oElmt;
                    addNewTextNode("cShippingDesc", ref argoNode12, oCartListElmt.GetAttribute("shippingDesc"));
                    oElmt = (XmlElement)argoNode12;
                    XmlNode argoNode13 = oElmt;
                    addNewTextNode("nShippingCost", ref argoNode13, oCartListElmt.GetAttribute("shippingCost"));
                    oElmt = (XmlElement)argoNode13;
                    if (oCartListElmt.SelectSingleNode("/Notes") != null)
                    {
                        XmlNode argoNode14 = oElmt;
                        addNewTextNode("cClientNotes", ref argoNode14, oCartListElmt.SelectSingleNode("/Notes").OuterXml);
                        oElmt = (XmlElement)argoNode14;
                    }
                    else
                    {
                        XmlNode argoNode15 = oElmt;
                        addNewTextNode("cClientNotes", ref argoNode15, "");
                        oElmt = (XmlElement)argoNode15;
                    }
                    XmlNode argoNode16 = oElmt;
                    addNewTextNode("cSellerNotes", ref argoNode16);
                    oElmt = (XmlElement)argoNode16;
                    XmlNode argoNode17 = oElmt;
                    addNewTextNode("nTaxRate", ref argoNode17, "0");
                    oElmt = (XmlElement)argoNode17;
                    XmlNode argoNode18 = oElmt;
                    addNewTextNode("nGiftListId", ref argoNode18, "-1");
                    oElmt = (XmlElement)argoNode18;
                    XmlNode argoNode19 = oElmt;
                    addNewTextNode("nAuditId", ref argoNode19);
                    oElmt = (XmlElement)argoNode19;
                    // validate column exists then only
                    if (moDBHelper.checkTableColumnExists("tblCartOrder", "nReceiptType"))
                    {
                        XmlNode argoNode20 = oElmt;
                        addNewTextNode("nReceiptType", ref argoNode20, "0");
                        oElmt = (XmlElement)argoNode20;
                    }

                    mnCartId = Convert.ToInt64(moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartOrder, oInstance.DocumentElement));

                    mnProcessId = 1;
                    string oOptionName = string.Empty;
                    double oOptionValue = 0d;
                    if (oCartListElmt.SelectSingleNode("/Item") != null)
                    {
                        foreach (XmlNode oItem in oCartListElmt.SelectNodes("Item"))
                        {

                            long nProductKey = Convert.ToInt64(oItem.Attributes["contentId"].InnerText);

                            long nQuantity = Convert.ToInt64(oItem.Attributes["quantity"].InnerText);
                            AddItem(nProductKey, nQuantity, null, "", UniqueProduct: true);
                            if (oItem.SelectSingleNode("/Item/Item") != null)
                            {

                                string sSQL2 = "select TOP 1 nCartItemKey  from tblCartItem  as a inner join tblAudit as b on a.nAuditId=b.nAuditKey where b.nStatus=1 and nParentId=0 and nCartOrderId =" + mnCartId.ToString() + "Order by nCartItemKey desc";

                                long nCartItemId = Convert.ToInt64(moDBHelper.ExeProcessSqlScalar(sSQL2));

                                foreach (XmlElement oOption in oItem.SelectNodes("Item"))
                                {

                                    if (oOption.SelectSingleNode("Name") != null)
                                    {
                                        oOptionName = oOption.SelectSingleNode("Name").InnerText;
                                    }
                                    if (oOption.Attributes["nPrice"] != null)
                                    {
                                        oOptionValue = Convert.ToDouble(oOption.Attributes["nPrice"].Value);
                                    }
                                    AddProductOption((int)nCartItemId, oOptionName, oOptionValue);
                                }
                            }
                        }
                    }

                    long deliveryAddId = 0;
                    long billingAddId = 0;
                    string sSql = "select nContactKey, cContactType, nAuditKey from tblCartContact inner join tblAudit a on nAuditId = a.nAuditKey where nContactCartId = " + nOrderId.ToString();
                    using (var oDr = moDBHelper.getDataReaderDisposable(sSql))
                    {
                        while (oDr.Read())
                        {
                            if ((oDr["cContactType"]?.ToString() ?? "") == "Billing Address")
                            {
                                billingAddId = Convert.ToInt64(oDr["nContactKey"]);
                            }

                            if (mbNoDeliveryAddress)
                            {
                                deliveryAddId = billingAddId;
                            }
                            else if ((oDr["cContactType"]?.ToString() ?? "") == "Delivery Address")
                            {
                                deliveryAddId = Convert.ToInt64(oDr["nContactKey"]);
                            }
                        }

                    }


                    if (deliveryAddId != 0 & billingAddId != 0)
                    {
                        useSavedAddressesOnCart(billingAddId, deliveryAddId, null);
                    }
                    XmlElement instanceNode = (XmlElement)oePaymentDetailsInstanceElmt
                             .SelectSingleNode("//PaymentDetails/instance");


                    XmlElement targetNode = instanceNode ?? oePaymentDetailsInstanceElmt;

                    ConfirmPayment(ref oCartListElmt, ref targetNode, cNewAuthNumber, cMethodName, Amount);

                    GetCart(ref oCartListElmt, mnCartId);

                    oCartListElmt.InnerXml = oCartListElmt.InnerXml.Replace(ReceiptId, cNewAuthNumber);

                    SaveCartXML(oCartListElmt, mnCartId);

                    return mnCartId.ToString();
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CreateDuplicateOrder", ex, "", "", gbDebug);
                    return null;
                }
            }

            public string GDPRAnonomize(string cEmailAddress)
            {

                string result = "";
                try
                {
                    Protean.Cms.dbHelper dbHelper = new Cms.dbHelper(myWeb);
                    Protean.Cms.model.Contact contact = new Cms.model.Contact();
                    if (!string.IsNullOrEmpty(cEmailAddress))
                    {
                        DataSet oDS;
                        if (myWeb.moDbHelper.checkDBObjectExists("spGetCartContact", Tools.Database.objectTypes.StoredProcedure))
                        {

                            var param = new Hashtable();
                            param.Add("EmailAddress", cEmailAddress);
                            XmlDocument oXml = new XmlDocument();
                            oDS = moDBHelper.GetDataSet("spGetCartContact", "tblCartContact", "", false, param, CommandType.StoredProcedure);
                            if (oDS.Tables["tblCartContact"] != null && oDS.Tables["tblCartContact"].Rows.Count > 0)
                            {
                                foreach (DataRow row in oDS.Tables["tblCartContact"].Rows)
                                {
                                    DataRow oRow = row;
                                    contact.cContactName = "GDPR Removal Request (" + DateTime.Now + ")";
                                    contact.cContactAddress = "";
                                    contact.cContactCity = "";
                                    contact.cContactZip = "";
                                    contact.cContactCountry = "";
                                    contact.cContactTel = "";
                                    contact.cContactEmail = "";
                                    contact.cContactFirstName = "";
                                    contact.cContactLastName = "";
                                    contact.nContactKey = Convert.ToInt32(oRow["nContactkey"]);
                                    dbHelper.SetContact(ref contact);
                                    int nCartOrderid = Convert.ToInt32(oRow["nContactCartId"]);
                                    if (nCartOrderid > 0)
                                    {
                                        var oCartElmt = moPageXml.CreateElement("Order");
                                        oCartElmt.InnerXml = "";
                                        GetCart(ref oCartElmt, nCartOrderid);
                                        SaveCartXML(oCartElmt, nCartOrderid);
                                        result = "Cart data anonymized successfully.";
                                    }


                                }
                                // OptOut from spotler mail +
                                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                                if (moMailConfig != null)
                                {
                                    string sMessagingProvider = "";
                                    sMessagingProvider = moMailConfig["MessagingProvider"];

                                    if (!string.IsNullOrEmpty(sMessagingProvider))
                                    {
                                        Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                                        IMessagingProvider oMessaging = RetProv.Get(ref myWeb, sMessagingProvider);
                                        bool isOptOut = oMessaging.Activities.OptOutAll(cEmailAddress);
                                        if (isOptOut)
                                        {
                                            result += " Messaging opt-out successful.";
                                        }
                                        else
                                        {
                                            result += " Messaging opt-out failed or already unsubscribed.";
                                        }
                                    }
                                }
                                //update in tblOptOutAddresses
                                DataSet oDSOptOut;
                                string cSql = "select nOptOutKey  from tblOptOutAddresses where EmailAddress='" + cEmailAddress + "'";
                                oDSOptOut = moDBHelper.GetDataSet(cSql, "Item");
                                if (oDSOptOut.Tables["Item"].Rows.Count > 0)
                                {
                                    List<string> optOutKeys = new List<string>();
                                    foreach (DataRow row in oDSOptOut.Tables["Item"].Rows)
                                    {
                                        optOutKeys.Add(row["nOptOutKey"].ToString());
                                    }
                                    string keyList = string.Join(",", optOutKeys);
                                    string updateSql = "UPDATE tblOptOutAddresses SET dGDPROptOut =" + Tools.Database.SqlDate(DateTime.Now) + ",optout_reason='GDPR Opt Out request',nStatus='1'  WHERE nOptOutKey IN (" + keyList + ")";
                                    moDBHelper.ExeProcessSql(updateSql);
                                    result += " update in tblOptOutAddresses.";
                                }
                                else
                                {
                                    string cSQL = "INSERT INTO tblOptOutAddresses (EmailAddress,optout_reason,nStatus,dOptOut,dGDPROptOut) VALUES ('" + cEmailAddress + "','GDPR Opt Out request','1'," + Tools.Database.SqlDate(DateTime.Now) + "," + Tools.Database.SqlDate(DateTime.Now) + ")";
                                    moDBHelper.ExeProcessSql(cSQL);
                                    result += " added GDPR in tblOptOutAddresses.";
                                }
                                //if (oDSOptOut.Tables["Item"].Rows.Count > 0)
                                //{
                                //    foreach (DataRow currentRow in oDSOptOut.Tables["Item"].Rows)
                                //    {
                                //        DataRow oOptOutRow;
                                //        oOptOutRow = currentRow;
                                //        if(oOptOutRow["nOptOutKey"]!=null)
                                //        {
                                //            moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.OptOutAddresses, Convert.ToInt64(oOptOutRow["nOptOutKey"]));
                                //            result += " Removed from tblOptOutAddresses.";
                                //        }

                                //    }
                                //}
                            }
                        }
                    }
                    return result;

                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GDPRAnonomize", ex, "", "", gbDebug);
                    if (ex != null)
                    {
                        result = "Error: " + ex.Message;
                    }
                    return result;
                }
            }

            public void saveCookiesConsent()
            {
                if (moDBHelper.checkTableColumnExists("tblCartOrder", "bCookieConsentEnabled") && (myWeb.moRequest.Cookies["bCookieConsentEnabled"] == null))
                {
                    int isCookieConsentEnabled = 0; // default = disabled

                    var request = HttpContext.Current?.Request;

                    if (request != null)
                    {
                        HttpCookie consentCookie = myWeb.moRequest.Cookies["cookiefirst-consent"];
                        HttpCookie cookieId = request.Cookies["cookiefirst-id"];

                        if (consentCookie != null && !string.IsNullOrWhiteSpace(consentCookie.Value))
                        {
                            string cookieValue = HttpUtility.UrlDecode(consentCookie.Value);

                            try
                            {
                                JObject consentJson = JObject.Parse(cookieValue);

                                bool preferences = consentJson.Value<bool?>("preferences") == true;
                                bool statistics = consentJson.Value<bool?>("statistics") == true;
                                bool advertising = consentJson.Value<bool?>("advertising") == true;


                                if (preferences || statistics || advertising)
                                {
                                    isCookieConsentEnabled = 1;
                                }
                            }
                            catch
                            {
                                isCookieConsentEnabled = 0;
                            }
                        }


                        string sSqlupdate = "UPDATE tblCartOrder SET bCookieConsentEnabled = " + isCookieConsentEnabled + " WHERE nCartOrderKey = " + mnCartId;

                        moDBHelper.ExeProcessSql(sSqlupdate);
                    }
                }
            }

            #region IDisposable Implementation

            private bool disposedValue = false; // To detect redundant calls

            // IDisposable
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        try
                        {
                            // ====================
                            // 1. DISPOSE CHILD COMPONENTS
                            // ====================

                            // Discount engine
                            if (moDiscount != null)
                            {
                                try
                                {
                                    if (moDiscount is IDisposable disposableDiscount)
                                    {
                                        disposableDiscount.Dispose();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        $"Error disposing moDiscount: {ex.Message}");
                                }
                                finally
                                {
                                    moDiscount = null;
                                }
                            }

                            // Subscription engine
                            if (moSubscription != null)
                            {
                                try
                                {
                                    if (moSubscription is IDisposable disposableSubscription)
                                    {
                                        disposableSubscription.Dispose();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        $"Error disposing moSubscription: {ex.Message}");
                                }
                                finally
                                {
                                    moSubscription = null;
                                }
                            }

                            // Payment provider
                            if (moPay != null)
                            {
                                try
                                {
                                    if (moPay is IDisposable disposablePay)
                                    {
                                        disposablePay.Dispose();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        $"Error disposing moPay: {ex.Message}");
                                }
                                finally
                                {
                                    moPay = null;
                                }
                            }

                            // Database helper (DO NOT dispose - owned by parent Cms object)
                            // moDBHelper is a reference to myWeb.moDbHelper, not owned by Cart
                            moDBHelper = null;

                            // ====================
                            // 2. NULL OUT LARGE OBJECTS
                            // ====================
                            moPageXml = null;
                            moCartXml = null;
                            oShippingOptions = null;

                            // ====================
                            // 3. NULL OUT REFERENCES
                            // ====================
                            myWeb = null;
                            moConfig = null;
                            moCartConfig = null;
                            moServer = null;
                        }
                        catch (Exception ex)
                        {
                            // Log disposal errors but don't throw
                            System.Diagnostics.Debug.WriteLine(
                                $"Error in Cart.Dispose: {ex.Message}");
                        }
                    }

                    // Free unmanaged resources (if any)

                    disposedValue = true;
                }
            }

            // Finalizer
            ~Cart()
            {
                Dispose(false);
            }

            // Public Dispose method
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }


            public void close(bool bNoClose=false)
            {
                myWeb.PerfMon.Log("Cart", "close");
                string cProcessInfo = "";
                try
                {
                    //This allows us to run jsonactions from within other functions like AddProductOption without closing dbhelper object.
                    if (bNoClose == false)
                    {
                        PersistVariables();
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Close", ex, "", cProcessInfo, gbDebug);
                }
                finally
                {
                   
                        Dispose(!bNoClose);
                }
            }

            // Helper method to prevent use after disposal
            protected void ThrowIfDisposed()
            {
                if (disposedValue)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
            }

            #endregion
        }
    }
}