using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web.Configuration;
using System.Xml;
using VB = Microsoft.VisualBasic;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;
using static Protean.Tools.Xml;
using System.Linq;
using Protean.Providers.Membership;
using Protean.Providers.Messaging;
using Protean.Providers.Payment;
using static Protean.Cms.dbImport;
using Lucene.Net.Analysis;
using System.Web.UI.WebControls;

namespace Protean
{


    public partial class Cms
    {

        public partial class Cart
        {
            #region Declarations


            public System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
            public System.Collections.Specialized.NameValueCollection moConfig;
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

            public int mnCartId; // Unique Id refering to this session cart
            public string mcSessionId; // Session ID - Unique for each client browser
           // private string mcRefSessionId; // Referrer Site Session ID - The session ID from the referrer site, if passed.
            public int mnEwUserId; // User Id for Membership integration
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
            public int mnPaymentId = 0; // to be populated by payment prvoider to pass to subscriptions

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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Close", ex, "", cProcessInfo, gbDebug);
                }
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
                mcModuleName = "Eonic.Cart";

                string cProcessInfo = Conversions.ToString(string.IsNullOrEmpty("initialise variables"));
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

                        if (Strings.LCase(myWeb.moRequest["ewCmd"]) == "logoff")
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

                        mcDeposit = Strings.LCase(moCartConfig["Deposit"]);
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

                        mnTaxRate = Conversions.ToDouble(moCartConfig["TaxRate"]);
                        if (myWeb.moSession["nTaxRate"] != null)
                        {
                            mnTaxRate = Conversions.ToDouble(Operators.ConcatenateObject("0", myWeb.moSession["nTaxRate"]));
                        }

                        if (!string.IsNullOrEmpty(myWeb.moRequest.Form["url"]))
                        {
                            myWeb.moSession["returnPage"] = myWeb.moRequest.Form["url"];
                        }

                        if (!string.IsNullOrEmpty(myWeb.moRequest.QueryString["url"]))
                        {
                            myWeb.moSession["returnPage"] = myWeb.moRequest.QueryString["url"];
                        }

                        mcReturnPage = Conversions.ToString(myWeb.moSession["returnPage"]);

                        if (myWeb.moSession["nEwUserId"] != null)
                        {
                            mnEwUserId = Conversions.ToInteger(Operators.ConcatenateObject("0", myWeb.moSession["nEwUserId"]));
                        }
                        else
                        {
                            mnEwUserId = 0;
                        }

                        if (myWeb.mnUserId > 0 & mnEwUserId == 0)
                            mnEwUserId = myWeb.mnUserId;
                        // MEMB - eEDIT
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moCtx.Application["bFullCartOption"], true, false)))
                        {
                            bFullCartOption = true;
                        }
                        else
                        {
                            bFullCartOption = false;
                        }
                        if (myWeb.moRequest.Form["cartId"] != null)
                        {
                            if (Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["CartId"], 0, false))
                            {
                                string CurrentCartId = myWeb.moRequest.Form["cartId"];
                                if ((CurrentCartId ?? "") != (myWeb.moSession["CartId"].ToString() ?? ""))
                                {
                                    myWeb.moSession["CartId"] = (object)int.Parse(CurrentCartId);
                                    mcReEstablishSession = "true";
                                }
                            }
                        }
                        string newCartId = (String)myWeb.moSession["CartId"];

                        if (myWeb.moSession["CartId"] is null)
                        {
                            mnCartId = 0;
                        }
                        else if (Conversions.ToBoolean(Operators.OrObject(!Information.IsNumeric(myWeb.moSession["CartId"]), Operators.ConditionalCompareObjectLessEqual(myWeb.moSession["CartId"], 0, false))))
                        {
                            mnCartId = 0;
                        }
                        else
                        {
                            mnCartId = Conversions.ToInteger(myWeb.moSession["CartId"]);
                        }

                        if (myWeb.moRequest["refSessionId"] != null)
                        {
                            mcSessionId = myWeb.moRequest["refSessionId"];
                            myWeb.moSession.Add("refSessionId", mcSessionId);
                        }
                        else if (myWeb.moSession["refSessionId"] != null)
                        {
                            mcSessionId = Conversions.ToString(myWeb.moSession["refSessionId"]);
                        }
                        else
                        {
                            mcSessionId = myWeb.moSession.SessionID;
                        }
                        // session id is assigned
                        // add logic if same seession id is present or not in db if we have then generate diff session id

                        if (Information.IsNumeric(myWeb.moRequest.QueryString["cartErr"]))
                            mnProcessError = (short)Conversions.ToInteger(myWeb.moRequest.QueryString["cartErr"]);

                        mcCartCmd = myWeb.moRequest.QueryString["cartCmd"];
                        if (string.IsNullOrEmpty(mcCartCmd))
                        {
                            mcCartCmd = myWeb.moRequest.Form["cartCmd"];
                        }

                        mcPaymentMethod = Conversions.ToString(myWeb.moSession["mcPaymentMethod"]);
                        mmcOrderType = Conversions.ToString(myWeb.moSession["mmcOrderType"]);
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
                                sSql = "select * from tblCartOrder where not(nCartStatus IN (6,9,13,14)) and nCartOrderKey = " + mnCartId + "And cCartSessionId Like '%" + mcSessionId + "'";
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
                                        mnGiftListId = Conversions.ToInteger(oDr["nGiftListId"]);
                                        mnTaxRate = Conversions.ToDouble(Operators.ConcatenateObject("0", oDr["nTaxRate"]));
                                        mnProcessId = (short)Conversions.ToLong(Operators.ConcatenateObject("0", oDr["nCartStatus"]));
                                        cartXmlFromDatabase = oDr["cCartXml"].ToString();
                                        // Check for deposit and earlier stages
                                        if (mcDeposit == "on")
                                        {
                                            if (!(oDr["nAmountReceived"] is DBNull))
                                            {
                                                if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectGreater(oDr["nAmountReceived"], 0, false), mnProcessId < (int)cartProcess.Confirmed)))
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
                                string cSessionCookieName = "ewSession" + myWeb.mnUserId.ToString();
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
                                        // sSql = "select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId where o.cCartSchemaName='Order' and o.cCartSessionId = '" & SqlFmt(mcSessionId) & "'"
                                        sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("select Top 1* from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId where o.cCartSchemaName='Order' and o.cCartSessionId = '", SqlFmt(mcSessionId)), "' and o.nCartOrderKey='"), Convert.ToString(mnCartId)), "' order by o.nCartOrderKey desc "));
                                    }
                                    else
                                    {
                                        sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId where o.cCartSchemaName='Order' and o.cCartSessionId = '", SqlFmt(mcSessionId)), "'"));
                                        // logic needs here to check cart id if we have car id then pull wiith session id
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
                                            mnGiftListId = Conversions.ToInteger(oDr["nGiftListId"]);
                                            mnCartId = Conversions.ToInteger(oDr["nCartOrderKey"]); // get cart id
                                            mnProcessId = Conversions.ToShort(oDr["nCartStatus"]); // get cart status
                                            mnTaxRate = Conversions.ToDouble(oDr["nTaxRate"]);
                                            if (myWeb.moRequest["settlementRef"] != null | myWeb.moRequest["settlementRef"] != null)
                                            {

                                                // Set to a holding state that indicates that the settlement has been initiated
                                                // mnProcessId = cartProcess.SettlementInitiated

                                                // If a cart has been found, we need to update the session ID in it.
                                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDr["cCartSessionId"], mcSessionId, false)))
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
                                            mcCurrencyRef = Conversions.ToString(oDr["cCurrency"]);
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

                        else if (myWeb.moSession != null && Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["promocode"], "", false)))
                        {

                            // Set the value from the session.
                            promocodeFromExternalRef = myWeb.moSession["promocode"].ToString();

                        }

                        mbVatAtUnit = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(moCartConfig["VatAtUnit"]) == "yes" | Strings.LCase(moCartConfig["VatAtUnit"]) == "on", true, false));
                        mbVatOnLine = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(moCartConfig["VatOnLine"]) == "yes" | Strings.LCase(moCartConfig["VatOnLine"]) == "on", true, false));

                        mbRoundup = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(moCartConfig["Roundup"]) == "yes" | Strings.LCase(moCartConfig["Roundup"]) == "on", true, false));
                        mbRoundDown = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(moCartConfig["Roundup"]) == "down", true, false));

                        mbDiscountsOn = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(moCartConfig["Discounts"]) == "yes" | Strings.LCase(moCartConfig["Discounts"]) == "on", true, false));
                        mbOveridePrice = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(moCartConfig["OveridePrice"]) == "yes" | Strings.LCase(moCartConfig["OveridePrice"]) == "on", true, false));

                        if (string.IsNullOrEmpty(mcCurrencyRef))
                            mcCurrencyRef = Conversions.ToString(myWeb.moSession["cCurrency"]);
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

                            if (Information.IsNumeric(cRes) && Conversions.ToDouble(cRes) > 0d)
                            {
                                myWeb.mnUserId = Conversions.ToInteger(cRes);
                                mnEwUserId = myWeb.mnUserId;
                                myWeb.moSession["nUserId"] = cRes;

                                string cRequestPage = myWeb.moRequest["pgid"];
                                if (Information.IsNumeric(cRequestPage) && Conversions.ToDouble(cRequestPage) > 0d)
                                {
                                    myWeb.mnPageId = Conversions.ToInteger(myWeb.moRequest["pgid"]);
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "InitializeVariables", ex, "", cProcessInfo, gbDebug);
                }
            }



            public void writeSessionCookie()
            {
                // writes the session cookie to persist the cart
                if (mcPersistCart == "on")
                {
                    // make or update the session cookie
                    var cookieEwSession = new System.Web.HttpCookie("ewSession" + myWeb.mnUserId.ToString());
                    cookieEwSession.Value = mcSessionId.ToString();
                    cookieEwSession.Expires = DateAndTime.DateAdd(DateInterval.Month, 1d, DateTime.Now);
                    myWeb.moResponse.Cookies.Add(cookieEwSession);
                }
            }

            private void clearSessionCookie()
            {

                string cSessionCookieName = "ewSession" + myWeb.mnUserId.ToString();

                if (myWeb.moResponse.Cookies[cSessionCookieName] != null)
                {
                    // clear ewSession cookie so cart doesn't get persisted
                    // we don't need to check for mcPersistCart = "on"
                    var cookieEwSession = new System.Web.HttpCookie(cSessionCookieName);
                    cookieEwSession.Expires = DateTime.Now.AddDays(-1);
                    myWeb.moResponse.Cookies.Add(cookieEwSession);
                    cookieEwSession.Expires = DateAndTime.DateAdd(DateInterval.Month, 1d, DateTime.Now);
                }
            }

            public void close()
            {
                myWeb.PerfMon.Log("Cart", "close");
                string cProcessInfo = "";
                try
                {
                    PersistVariables();
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Close", ex, "", cProcessInfo, gbDebug);
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
                            myWeb.moSession["CartId"] = mnCartId.ToString();
                        }
                        // oResponse.Cookies(mcSiteURL & "CartId").Domain = mcSiteURL
                        // oSession("nCartOrderId") = mnCartId    '   session attribute holds Cart ID
                    }

                    if (mnCartId > 0)
                    {
                        // Only update the process if less than 6 we don't ever want to change the status of a completed order other than within the admin system. Boo Yah!
                        int currentStatus = Conversions.ToInteger(moDBHelper.ExeProcessSqlScalar("select nCartStatus from tblCartOrder where nCartOrderKey = " + mnCartId));
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "PersistVariables", ex, "", cProcessInfo, gbDebug);
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "checkButtons", ex, "", cProcessInfo, gbDebug);
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "apply", ex, "", "CreateCartElement", gbDebug);
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

                    cProcessInfo = Conversions.ToString(cProcessInfo + Operators.ConcatenateObject(Interaction.IIf(string.IsNullOrEmpty(mcCartCmd), "", ", "), mcCartCmd));

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
                                mcCartCmd = Conversions.ToString(updateCart(ref argcSuccessfulCartCmd));
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
                                    if (Strings.InStr(Conversions.ToString(oItem1), "qty_") == 1) // check for getting productID and quantity (since there will only be one of these per item submitted)
                                    {
                                        if (Information.IsNumeric(myWeb.moRequest.Form.Get(oItem1)))
                                        {
                                            nQuantity = Conversions.ToLong(myWeb.moRequest.Form.Get(oItem1));
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
                                    myWeb.msRedirectOnEnd = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(mcSiteURL + mcReturnPage, Interaction.IIf((mcSiteURL + mcReturnPage).Contains("?"), "&", "?")), "cartCmd=finish"));
                                    // myWeb.moResponse.Redirect(mcSiteURL & mcReturnPage & IIf((mcSiteURL & mcReturnPage).Contains("?"), "&", "?") & "cartCmd=finish")
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
                                    if (Strings.InStr(Conversions.ToString(item), "__utm") == 1)
                                    {
                                        cGoogleTrackingCode = cGoogleTrackingCode + "&" + Conversions.ToString(item) + "=" + myWeb.moRequest.QueryString[Conversions.ToString(item)];
                                    }
                                }

                                if (mnCartId > 0)
                                {

                                    myWeb.msRedirectOnEnd = mcPagePath + "cartCmd=" + cRedirectCommand + "&refSessionId=" + mcSessionId + cGoogleTrackingCode;
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

                                myWeb.msRedirectOnEnd = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(mcSiteURL + mcReturnPage, Interaction.IIf((mcSiteURL + mcReturnPage).Contains("?"), "&", "?")), "cartCmd=finish"));
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
                                        oRegXform.xFrmEditDirectoryItem(IntanceAppend: ref argIntanceAppend, (long)myWeb.mnUserId, "User", (long)Conversions.ToInteger("0" + moCartConfig["DefaultSubscriptionGroupId"]), "CartRegistration");
                                        if (oRegXform.valid)
                                        {
                                            string sReturn = moDBHelper.validateUser(myWeb.moRequest["cDirName"], myWeb.moRequest["cDirPassword"]);
                                            if (Information.IsNumeric(sReturn))
                                            {
                                                myWeb.mnUserId = (int)Conversions.ToLong(sReturn);
                                                var oUserElmt = moDBHelper.GetUserXML((long)myWeb.mnUserId);

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
                                                Information.Err().Raise(1004, "addressSubProcess", " PAGE IS NOT CREATED");
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
                                mnProcessId = 5;
                                if (!string.IsNullOrEmpty(myWeb.moRequest["PaymentMethod"]))
                                {
                                    mcPaymentMethod = myWeb.moRequest["PaymentMethod"];
                                }
                                if (oElmt.FirstChild is null)
                                {
                                    GetCart(ref oElmt);
                                }

                                // Add the date and reference to the cart

                                addDateAndRef(ref oElmt);

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

                                if (Strings.InStr(mcPaymentMethod, "Repeat_") > 0)
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

                                    PersistVariables();

                                    if (oElmt.FirstChild is null)
                                    {
                                        GetCart(ref oElmt);
                                    }

                                    if (mnProcessId == (int)cartProcess.Complete | mnProcessId == (int)cartProcess.DepositPaid | mnProcessId == (int)cartProcess.AwaitingPayment)
                                    {

                                        if (moCartConfig["StockControl"] == "on")
                                        {
                                            UpdateStockLevels(ref oElmt);
                                        }
                                        UpdateGiftListLevels();
                                        addDateAndRef(ref oElmt);
                                        if (myWeb.mnUserId > 0)
                                        {
                                            var userXml = myWeb.moDbHelper.GetUserXML((long)myWeb.mnUserId, false);
                                            if (userXml != null)
                                            {
                                                XmlElement cartElement = (XmlElement)oContentElmt.SelectSingleNode("Cart");
                                                if (cartElement != null)
                                                {
                                                    cartElement.AppendChild(cartElement.OwnerDocument.ImportNode(userXml, true));
                                                }
                                            }
                                        }

                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["Settlement"], "true", false)))
                                        {
                                            // modifiy the cartXml in line with settlement
                                            if (mnProcessId == (int)cartProcess.DepositPaid)
                                            {
                                                mnProcessId = (short)cartProcess.Complete;

                                            }
                                            myWeb.moSession["Settlement"] = (object)null;
                                        }



                                        if (mnProcessId == (int)cartProcess.DepositPaid)
                                        {
                                            AddToLists("Deposit", ref oContentElmt);
                                        }
                                        else
                                        {
                                            AddToLists("Invoice", ref oContentElmt);
                                        }

                                        purchaseActions(ref oContentElmt);
                                        // update the cart if purchase actions have changed it
                                        // GetCart(oElmt)
                                        // done for ammerdown as we have removed a product.



                                        if (myWeb.mnUserId > 0)
                                        {
                                            if (moSubscription != null)
                                            {
                                                moSubscription.AddUserSubscriptions(mnCartId, myWeb.mnUserId, ref oContentElmt, mnPaymentId);
                                            }
                                        }

                                        if (moCartConfig["SendReceiptEmailForAwaitingPaymentStatusId"] != null)
                                        {
                                            if ((oElmt.GetAttribute("statusId") ?? "") != (moCartConfig["SendReceiptEmailForAwaitingPaymentStatusId"] ?? ""))
                                            {
                                                emailReceipts(ref oContentElmt);
                                            }
                                        }
                                        else
                                        {
                                            emailReceipts(ref oContentElmt);
                                        }


                                        moDiscount.DisablePromotionalDiscounts();

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
                                int nI = 0;
                                if (!string.IsNullOrEmpty(myWeb.moRequest["OrderID"]))
                                    nI = Conversions.ToInteger(myWeb.moRequest["OrderID"]);
                                GetCartSummary(ref oElmt);
                                XmlElement argoPageDetail = null;
                                ListOrders(nI.ToString(), false, 0, oPageDetail: ref argoPageDetail);
                                break;
                            }

                        case "MakeCurrent":
                            {
                                int nI = 0;
                                if (!string.IsNullOrEmpty(myWeb.moRequest["OrderID"]))
                                    nI = Conversions.ToInteger(myWeb.moRequest["OrderID"]);
                                if (!(nI == 0))
                                    MakeCurrent(nI);
                                mcCartCmd = "Cart";
                                goto processFlow;

                            }

                        case "Delete":
                            {
                                int nI = 0;
                                if (!string.IsNullOrEmpty(myWeb.moRequest["OrderID"]))
                                    nI = Conversions.ToInteger(myWeb.moRequest["OrderID"]);
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
                                if (!!string.IsNullOrEmpty(Strings.LCase(moCartConfig["ContinuePath"])))
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
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "apply", ex, "", cProcessInfo, gbDebug);
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "AddCartElement", ex, "", cProcessInfo, gbDebug);
                }
            }

            public virtual void AddBehaviour()
            {
                string cProcessInfo = "";
                try
                {

                    switch (Strings.LCase(moCartConfig["AddBehaviour"]) ?? "")
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "AddBehavior", ex, "", cProcessInfo, gbDebug);
                }

            }

            public virtual PaymentProviders GetPaymentProvider()
            {

                var oEwProv = new PaymentProviders(ref myWeb);
                return oEwProv;

            }
            public virtual void emailReceipts(ref XmlElement oCartElmt)
            {
                emailReceipts(ref oCartElmt, "");
            }
            public virtual void emailReceipts(ref XmlElement oCartElmt, string ccCustomerEmail = "")
            {
                myWeb.PerfMon.Log("Cart", "emailReceipts");
                string sMessageResponse;
                string cProcessInfo = "";
                try
                {
                    if (Strings.LCase(moCartConfig["EmailReceipts"]) != "off")
                    {
                        // Default subject line
                        string cSubject = moCartConfig["OrderEmailSubject"];
                        if (string.IsNullOrEmpty(cSubject))
                            cSubject = "Website Order";

                        string CustomerEmailTemplatePath = "/xsl/Cart/mailOrderCustomer.xsl";
                        string MerchantEmailTemplatePath = "/xsl/Cart/mailOrderMerchant.xsl";
                        if (myWeb.bs5)
                        {
                            CustomerEmailTemplatePath = "/features/cart/email/order-customer.xsl";
                            MerchantEmailTemplatePath = "/features/cart/email/order-merchant.xsl";
                        }
                        if (!string.IsNullOrEmpty(moCartConfig["CustomerEmailTemplatePath"]))
                        {
                            CustomerEmailTemplatePath = moCartConfig["CustomerEmailTemplatePath"];
                        }
                        if (!string.IsNullOrEmpty(moCartConfig["MerchantEmailTemplatePath"]))
                        {
                            MerchantEmailTemplatePath = moCartConfig["MerchantEmailTemplatePath"];
                        }

                        // send to customer
                        sMessageResponse = Conversions.ToString(emailCart(ref oCartElmt, CustomerEmailTemplatePath, moCartConfig["MerchantName"], moCartConfig["MerchantEmail"], oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText, cSubject, cAttachementTemplatePath: moCartConfig["CustomerAttachmentTemplatePath"], cCCEmail: ccCustomerEmail));

                        // Send to merchant
                        sMessageResponse = Conversions.ToString(emailCart(ref oCartElmt, MerchantEmailTemplatePath, oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText, oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText, moCartConfig["MerchantEmail"], cSubject, false, moCartConfig["MerchantAttachmentTemplatePath"], moCartConfig["MerchantEmailBcc"]));

                        XmlElement oElmtEmail;
                        oElmtEmail = moPageXml.CreateElement("Reciept");
                        oCartElmt.AppendChild(oCartElmt.OwnerDocument.ImportNode(oElmtEmail, true));
                        oElmtEmail.InnerText = sMessageResponse;

                        if (sMessageResponse == "Message Sent")
                        {
                            oElmtEmail.SetAttribute("status", "sent");
                        }
                        else
                        {
                            oElmtEmail.SetAttribute("status", "failed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "emailReceipts", ex, "", cProcessInfo, gbDebug);
                }

            }

            public object AddPayment(double amountPaid, string Description)
            {
                string cProcessInfo = "";
                string sSql;
                try
                {
                    var nAmountReceived = default(double);
                    // Get the amount received so far
                    sSql = "select * from tblCartOrder where nCartOrderKey = " + mnCartId;
                    using (var oDr = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            while (oDr.Read())
                                nAmountReceived = Conversions.ToDouble(Operators.ConcatenateObject(0, oDr["nAmountReceived"]));
                        }
                    }
                    nAmountReceived = nAmountReceived + amountPaid;

                    sSql = "update tblCartOrder set nAmountReceived = " + nAmountReceived + " where nCartOrderKey = " + mnCartId;
                    moDBHelper.ExeProcessSql(sSql);

                    mnPaymentId = moDBHelper.savePayment(mnCartId, (long)myWeb.mnUserId, "", "", Description, (XmlElement)null, DateTime.Now, false, amountPaid, "deduction");
                    return null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ConfirmPayment", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
                finally
                {
                    // oDr = Nothing
                }
            }


            public object ConfirmPayment(ref XmlElement oCartElmt, ref XmlElement PaymentDetailXml, string providerPaymentRef, string providerName, double amountPaid)
            {
                string cProcessInfo = "ConfirmPayment";
                try
                {
                    string PayableType = oCartElmt.GetAttribute("payableType");

                    // Add processing for deposits.
                    switch (PayableType ?? "")
                    {
                        case "deposit":
                            {
                                mcDepositAmount = Conversions.ToDouble("0" + oCartElmt.GetAttribute("payableAmount")).ToString();
                                double outstandingAmount;
                                if (Conversions.ToDouble(mcDepositAmount) == 0d)
                                {
                                    // no deposit payment paid in full
                                    outstandingAmount = 0d;
                                }
                                else
                                {
                                    outstandingAmount = Conversions.ToDouble("0" + oCartElmt.GetAttribute("total")) - Conversions.ToDouble(mcDepositAmount);
                                }

                                // Let's update the cart element
                                oCartElmt.SetAttribute("paymentMade", mcDepositAmount);
                                oCartElmt.SetAttribute("outstandingAmount", Strings.FormatNumber(outstandingAmount, 2, TriState.True, TriState.False, TriState.False));

                                // Let's create a unique link for settlement
                                // Make a unique link
                                string cUniqueLink = "";
                                while (string.IsNullOrEmpty(cUniqueLink))
                                {
                                    object testLink = Guid.NewGuid().ToString();
                                    string sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("select * from tblCartOrder where cSettlementID = '", testLink), "'"));
                                    using (var oDr = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                    {
                                        if (!oDr.HasRows)
                                            cUniqueLink = Conversions.ToString(testLink);
                                    }
                                }
                                if (outstandingAmount == 0d)
                                {
                                    PayableType = "full";
                                    oCartElmt.SetAttribute("transStatus", "Paid In Full");
                                    mnProcessId = 6;
                                }
                                else
                                {
                                    oCartElmt.SetAttribute("settlementID", cUniqueLink);
                                    oCartElmt.SetAttribute("transStatus", "Deposit Paid");
                                    mnProcessId = 10;
                                }

                                UpdateCartDeposit(ref oCartElmt, amountPaid, PayableType);
                                break;
                            }


                        case "settlement":
                            {
                                mnProcessId = 6;
                                double totalPaid = Conversions.ToDouble(oCartElmt.GetAttribute("paymentMade"));
                                totalPaid = totalPaid + amountPaid;
                                double outstandingAmount = Conversions.ToDouble("0" + oCartElmt.GetAttribute("total")) - totalPaid;
                                oCartElmt.SetAttribute("paymentMade", amountPaid.ToString());
                                oCartElmt.SetAttribute("outstandingAmount", outstandingAmount.ToString());
                                oCartElmt.SetAttribute("payableAmount", outstandingAmount.ToString());
                                oCartElmt.SetAttribute("transStatus", "Settlement Paid");
                                oCartElmt.SetAttribute("status", "Settlement Paid");
                                oCartElmt.SetAttribute("statusId", mnProcessId.ToString());
                                UpdateCartDeposit(ref oCartElmt, amountPaid, PayableType);
                                break;
                            }

                        default:
                            {
                                PayableType = "full";
                                UpdateCartDeposit(ref oCartElmt, amountPaid, PayableType);
                                oCartElmt.SetAttribute("transStatus", "Paid In Full");
                                mnProcessId = 6;
                                break;
                            }
                    }

                    mnPaymentId = moDBHelper.savePayment(mnCartId, (long)myWeb.mnUserId, providerName, providerPaymentRef, providerName, PaymentDetailXml, DateTime.Now, false, amountPaid, PayableType);
                    return null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ConfirmPayment", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
                finally
                {
                    // oDr = Nothing
                }
            }

            public virtual void purchaseActions(ref XmlElement oCartElmt)
            {
                myWeb.PerfMon.Log("Cart", "purchaseActions");
                // Dim sMessageResponse As String
                string cProcessInfo = "";

                try
                {

                    if (!string.IsNullOrEmpty(moCartConfig["AccountingProvider"]))
                    {
                        object providerName = moCartConfig["AccountingProvider"];
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/accountingProviders");
                        var assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName.ToString()].Type.ToString());
                        Type calledType;
                        string classPath = Conversions.ToString(moPrvConfig.Providers[providerName.ToString()].Parameters["rootClass"]);

                        string passCMS = Conversions.ToString(moPrvConfig.Providers[providerName.ToString()].Parameters["passCMS"]);

                        string methodName = "ProcessOrder";
                        calledType = assemblyInstance.GetType(classPath, true);
                        var o = Activator.CreateInstance(calledType);

                        var args = new object[1];

                        if (passCMS == "true")
                        {
                            args = new object[2];
                            args[0] = myWeb;
                            args[1] = oCartElmt;
                        }
                        else
                        {
                            args[0] = oCartElmt;
                        }

                        if (oCartElmt.FirstChild.SelectSingleNode("Notes/PromotionalCode") != null)
                        {

                            string sDiscoutCode = oCartElmt.FirstChild.SelectSingleNode("Notes/PromotionalCode").InnerText;
                            if (myWeb.moDbHelper.checkTableColumnExists("tblSingleUsePromoCode", "PromoCode"))
                            {
                                string sSql = "Insert into tblSingleUsePromoCode (OrderId, PromoCode) values (";
                                sSql += mnCartId + ",'";
                                sSql += sDiscoutCode + "')";


                                moDBHelper.ExeProcessSql(sSql);
                            }
                        }
                        calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);

                    }


                    foreach (XmlElement ocNode in oCartElmt.SelectNodes("descendant-or-self::Order/Item/productDetail[@purchaseAction!='']"))
                    {
                        string classPath = ocNode.GetAttribute("purchaseAction");
                        string assemblyName = ocNode.GetAttribute("assembly");
                        string providerName = ocNode.GetAttribute("providerName");
                        string assemblyType = ocNode.GetAttribute("assemblyType");

                        string methodName = Strings.Right(classPath, Strings.Len(classPath) - classPath.LastIndexOf(".") - 1);

                        classPath = Strings.Left(classPath, classPath.LastIndexOf("."));

                        if (!string.IsNullOrEmpty(classPath))
                        {
                            try
                            {
                                Type calledType;

                                if (!string.IsNullOrEmpty(assemblyName))
                                {
                                    classPath = classPath + ", " + assemblyName;
                                }
                                // Dim oModules As New Protean.Cms.Membership.Modules

                                if (!string.IsNullOrEmpty(providerName))
                                {
                                    // case for external Providers
                                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders");
                                    var assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName].Type);
                                    calledType = assemblyInstance.GetType(classPath, true);
                                }

                                else if (!string.IsNullOrEmpty(assemblyType))
                                {
                                    // case for external DLL's
                                    var assemblyInstance = Assembly.Load(assemblyType);
                                    calledType = assemblyInstance.GetType(classPath, true);
                                }
                                else
                                {
                                    // case for methods within ProteanCMS Core DLL
                                    calledType = Type.GetType(classPath, true);
                                }

                                var o = Activator.CreateInstance(calledType);

                                var args = new object[2];
                                args[0] = myWeb;
                                args[1] = ocNode;

                                calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);
                            }

                            // Error Handling ?
                            // Object Clearup ?


                            catch (Exception)
                            {
                                // OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo))
                                cProcessInfo = classPath + "." + methodName + " not found";
                                ocNode.InnerXml = "<Content type=\"error\"><div>" + cProcessInfo + "</div></Content>";
                            }
                        }

                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "purchaseActions", ex, "", cProcessInfo, gbDebug);
                }

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
                            if (string.IsNullOrEmpty(Email))
                                Email = oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText;
                            if (string.IsNullOrEmpty(Name))
                                Name = oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText;
                            if (valDict is null)
                                valDict = new Dictionary<string, string>();
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
                                        if (!string.IsNullOrEmpty(moMailConfig["QuoteList"]))
                                        {
                                            // if we have invoiced the customer we don't want to send them quote reminders
                                            if (oMessaging.Activities != null)
                                            {
                                                oMessaging.Activities.RemoveFromList(moMailConfig["QuoteList"], Email);
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
                                    oMessaging.Activities.AddToList(ListId, firstName, Email, valDict);
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "purchaseActions", ex, "", cProcessInfo, gbDebug);
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
                var xMailingListDoc = htmlToXmlDoc(messageHtml);
                var xListElement = xMailingListDoc.DocumentElement;
                valDict = XmltoDictionary(xListElement, true);
                return valDict;
            }
            private void RemoveDeliveryOption(int nOrderId)
            {
                try
                {
                    string cSQL = "UPDATE tblCartOrder SET nShippingMethodId = 0, cShippingDesc = NULL, nShippingCost = 0 WHERE nCartOrderKey = " + nOrderId;
                    myWeb.moDbHelper.ExeProcessSqlorIgnore(cSQL);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "RemoveDeliveryOption", ex, "", "", gbDebug);
                }
            }
            // Sub GetCartSummary(ByRef oCartElmt As XmlElement, Optional ByVal nSelCartId As Integer = 0)
            // myWeb.PerfMon.Log("Cart", "GetCartSummary")
            // '   Sets content for the XML to be displayed in the small summary plugin attached
            // '   to the current content page

            // Dim oDs As DataSet
            // Dim sSql As String

            // Dim cNotes As String
            // Dim total As Double
            // Dim quant As Integer
            // Dim weight As Integer
            // Dim nCheckPrice As Double
            // Dim oRow As DataRow
            // 'We need to read this value from somewhere so we can change where vat is added
            // 'Currently defaulted to per line
            // 'If true will be added to the unit
            // Dim nUnitVat As Decimal = 0
            // Dim nLineVat As Decimal = 0

            // Dim nCartIdUse As Integer
            // If nSelCartId > 0 Then nCartIdUse = nSelCartId Else nCartIdUse = mnCartId

            // Dim cProcessInfo As String = ""

            // oCartElmt.SetAttribute("vatRate", moCartConfig("TaxRate"))
            // Try


            // If Not moSubscription Is Nothing Then moSubscription.CheckCartForSubscriptions(nCartIdUse, myWeb.mnUserId)

            // If Not (nCartIdUse > 0) Then '   no shopping
            // oCartElmt.SetAttribute("status", "Empty") '   set CartXML attributes
            // oCartElmt.SetAttribute("itemCount", "0") '       to nothing
            // oCartElmt.SetAttribute("vatRate", moCartConfig("TaxRate"))
            // oCartElmt.SetAttribute("total", "0.00") '       for nothing
            // Else
            // '   otherwise

            // '   and the address details we have obtained
            // '   (if any)


            // 'Add Totals
            // quant = 0 '   get number of items & sum of collective prices (ie. cart total) from db
            // total = 0.0#
            // weight = 0.0#

            // sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, p.cContentXmlDetail as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, p.cContentSchemaName AS contentType from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" & nCartIdUse


            // ':TODO we only want to check prices on current orders not history
            // oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart")

            // '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // oDs.Relations.Add("Rel1", oDs.Tables("Item").Columns("id"), oDs.Tables("Item").Columns("nParentId"), False)
            // oDs.Relations("Rel1").Nested = True
            // '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // For Each oRow In oDs.Tables("Item").Rows

            // If moDBHelper.DBN2int(oRow("nParentId")) = 0 Then
            // Dim oOpRow As DataRow
            // Dim nOpPrices As Decimal = 0

            // If Not IsDBNull(oRow("productDetail")) Then
            // ' Go get the lowest price based on user and group
            // nCheckPrice = getProductPricesByXml(oRow("productDetail"), oRow("unit") & "", oRow("quantity"))

            // If Not moSubscription Is Nothing And oRow("contentType") = "Subscription" Then nCheckPrice = moSubscription.CartSubscriptionPrice(oRow("contentId"), myWeb.mnUserId)

            // If nCheckPrice > 0 And nCheckPrice <> oRow("price") Then
            // ' If price is lower, then update the item price field
            // oRow.BeginEdit()
            // oRow("price") = nCheckPrice
            // oRow.EndEdit()
            // End If

            // '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

            // For Each oOpRow In oRow.GetChildRows("Rel1")
            // 'need an option check price bit here
            // Dim nNPrice As Decimal = getOptionPricesByXml(oRow("productDetail"), oRow("nItemOptGrpIdx"), oRow("nItemOptIdx"))
            // If nNPrice > 0 And nNPrice <> oOpRow("price") Then
            // nOpPrices += nNPrice
            // oOpRow.BeginEdit()
            // oOpRow("price") = nNPrice
            // oOpRow.EndEdit()
            // Else
            // nOpPrices += oOpRow("price")
            // End If
            // Next
            // 'oRow.BeginEdit()
            // 'oRow("price") = nOpPrices
            // 'oRow.EndEdit()
            // '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            // End If

            // ' Apply stock control
            // If mbStockControl Then CheckStock(oCartElmt, oRow("productDetail"), oRow("quantity"))
            // ' Apply quantity control
            // CheckQuantities(oCartElmt, oRow("productDetail"), oRow("quantity"))

            // weight = weight + (oRow("weight") * oRow("quantity"))
            // quant = quant + oRow("quantity")

            // If moCartConfig("DiscountDirection") = "up" Then
            // total = total + (oRow("quantity") * (oRow("price") + nOpPrices)) + CDec("0" & oRow("discount"))
            // Else
            // total = total + (oRow("quantity") * (oRow("price") + nOpPrices)) - CDec("0" & oRow("discount"))
            // End If

            // 'Round( Price * Vat ) * Quantity
            // nUnitVat += RoundUp(((oRow("price") + nOpPrices)) - CDec("0" & oRow("discount")) * (mnTaxRate / 100)) * oRow("quantity")
            // 'Round( ( Price * Quantity )* VAT )
            // nLineVat += RoundUp((((oRow("price") + nOpPrices) - CDec("0" & oRow("discount"))) * oRow("quantity")) * (mnTaxRate / 100))
            // End If
            // Dim cUpdtSQL As String = "UPDATE tblCartItem SET nPrice = " & oRow("price") & " WHERE nCartItemKey = " & oRow("id")
            // moDBHelper.exeProcessSQLScalar(cUpdtSQL)
            // Next


            // ' Quantity based error messaging
            // Dim oError As XmlElement = oCartElmt.SelectSingleNode("error")
            // If Not (oError Is Nothing) Then
            // Dim oMsg As XmlElement
            // oMsg = oCartElmt.OwnerDocument.CreateElement("msg")
            // oMsg.SetAttribute("type", "zz_footer") ' Forces it to the bottom of the message block
            // oMsg.InnerText = "Please adjust the quantities you require, or call for assistance."
            // oError.AppendChild(oMsg)
            // End If

            // 'moDBHelper.updateDataset(oDs, "Item", True)

            // sSql = "select cClientNotes from tblCartOrder where nCartOrderKey=" & nCartIdUse
            // Dim oNotes As XmlElement = oCartElmt.OwnerDocument.CreateElement("Notes")
            // oNotes.InnerXml = moDBHelper.exeProcessSQLScalar(sSql)
            // total -= moDiscount.CheckDiscounts(oDs, oCartElmt, False, oNotes)

            // oCartElmt.SetAttribute("status", "Active")
            // oCartElmt.SetAttribute("itemCount", quant)
            // oCartElmt.SetAttribute("vatRate", moCartConfig("TaxRate"))
            // oCartElmt.SetAttribute("total", total)
            // oCartElmt.SetAttribute("weight", weight)
            // oCartElmt.SetAttribute("orderType", mmcOrderType & "")

            // 'Adding notes to cart summary
            // sSql = "Select cClientNotes from tblCartOrder where nCartOrderKey = " & nCartIdUse
            // cNotes = moDBHelper.DBN2Str(moDBHelper.exeProcessSQLScalar(sSql), False, False)
            // sSql = "Select nCartStatus  from tblCartOrder where nCartOrderKey = " & nCartIdUse
            // If moDBHelper.DBN2Str(moDBHelper.exeProcessSQLScalar(sSql), False, False) = 6 Then
            // oCartElmt.SetAttribute("complete", "true")
            // Else
            // oCartElmt.SetAttribute("complete", "true")
            // End If
            // Dim oElmt As XmlElement
            // If Not (cNotes = "") Then
            // oElmt = moPageXml.CreateElement("Notes")
            // oElmt.InnerXml = cNotes
            // oCartElmt.AppendChild(oElmt.FirstChild())
            // End If
            // End If

            // Catch ex As Exception
            // returnException(myWeb.msException, mcModuleName, "GetCartSummary", ex, "", cProcessInfo, gbDebug)
            // End Try

            // End Sub
            // Public Sub GetCartSummary(ByRef oCartElmt As XmlElement, Optional ByVal nSelCartId As Integer = 0)
            // 'made same as get cart
            // myWeb.PerfMon.Log("Cart", "GetCartSummary")
            // '   Content for the XML that will display all the information stored for the Cart
            // '   This is a list of cart items (and quantity, price ...), totals,
            // '   billing & delivery addressi and delivery method.

            // Dim oDs As DataSet

            // Dim sSql As String
            // Dim oRow As DataRow

            // Dim oElmt As XmlElement
            // Dim oElmt2 As XmlElement
            // Dim oXml As XmlDataDocument

            // Dim quant As Integer
            // Dim weight As Double
            // Dim total As Double
            // Dim vatAmt As Double
            // Dim shipCost As Double
            // Dim nCheckPrice As Double
            // 'We need to read this value from somewhere so we can change where vat is added
            // 'Currently defaulted to per line
            // 'If true will be added to the unit
            // Dim nUnitVat As Decimal = 0
            // Dim nLineVat As Decimal = 0


            // Dim cProcessInfo As String = ""
            // Dim cOptionGroupName As String = ""

            // Dim nCartIdUse As Integer
            // If nSelCartId > 0 Then nCartIdUse = nSelCartId Else nCartIdUse = mnCartId


            // Try

            // If Not moSubscription Is Nothing Then moSubscription.CheckCartForSubscriptions(nCartIdUse, myWeb.mnUserId)

            // If Not (nCartIdUse > 0) Then '   no shopping
            // oCartElmt.SetAttribute("status", "Empty") '   set CartXML attributes
            // oCartElmt.SetAttribute("itemCount", "0") '       to nothing
            // oCartElmt.SetAttribute("vatRate", moCartConfig("TaxRate"))
            // oCartElmt.SetAttribute("total", "0.00") '       for nothing

            // Else
            // '   otherwise

            // '   and the address details we have obtained
            // '   (if any)
            // 'Add Totals
            // quant = 0 '   get number of items & sum of collective prices (ie. cart total) from db
            // total = 0.0#
            // weight = 0.0#

            // sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, p.cContentXmlDetail as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, p.cContentSchemaName AS contentType, dbo.fxn_getContentParents(i.nItemId) as parId  from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" & nCartIdUse

            // oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart")
            // 'add relationship for options
            // oDs.Relations.Add("Rel1", oDs.Tables("Item").Columns("id"), oDs.Tables("Item").Columns("nParentId"), False)
            // oDs.Relations("Rel1").Nested = True
            // '
            // For Each oRow In oDs.Tables("Item").Rows
            // If moDBHelper.DBN2int(oRow("nParentId")) = 0 Then
            // ' Go get the lowest price based on user and group
            // If Not IsDBNull(oRow("productDetail")) Then
            // nCheckPrice = getProductPricesByXml(oRow("productDetail"), oRow("unit") & "", oRow("quantity"))

            // If Not moSubscription Is Nothing And oRow("contentType") = "Subscription" Then nCheckPrice = moSubscription.CartSubscriptionPrice(oRow("contentId"), myWeb.mnUserId)
            // End If
            // If nCheckPrice > 0 And nCheckPrice <> oRow("price") Then
            // ' If price is lower, then update the item price field
            // 'oRow.BeginEdit()
            // oRow("price") = nCheckPrice
            // 'oRow.EndEdit()
            // End If

            // 'option prices
            // Dim oOpRow As DataRow
            // Dim nOpPrices As Decimal = 0
            // For Each oOpRow In oRow.GetChildRows("Rel1")

            // Dim nNPrice As Decimal = getOptionPricesByXml(oRow("productDetail"), oRow("nItemOptGrpIdx"), oRow("nItemOptIdx"))
            // If nNPrice > 0 And nNPrice <> oOpRow("price") Then
            // nOpPrices += nNPrice
            // 'oOpRow.BeginEdit()
            // oOpRow("price") = nNPrice

            // 'oOpRow.EndEdit()
            // Else
            // nOpPrices += oOpRow("price")
            // End If
            // Next

            // ' Apply stock control
            // If mbStockControl Then CheckStock(oCartElmt, oRow("productDetail"), oRow("quantity"))
            // ' Apply quantity control
            // CheckQuantities(oCartElmt, oRow("productDetail"), oRow("quantity"))

            // weight = weight + (oRow("weight") * oRow("quantity"))
            // quant = quant + oRow("quantity")
            // total = total + (oRow("quantity") * (oRow("price") + nOpPrices))

            // 'Round( Price * Vat ) * Quantity
            // ' nUnitVat += Round((oRow("price") + nOpPrices) * (mnTaxRate / 100), , , mbRoundup) * oRow("quantity")
            // 'Round( ( Price * Quantity )* VAT )
            // '  nLineVat += Round((((oRow("price") + nOpPrices)) * oRow("quantity")) * (mnTaxRate / 100), , , mbRoundup)

            // End If
            // 'Dim ix As Integer
            // 'Dim xstr As String
            // 'For ix = 0 To oRow.Table.Columns.Count - 1
            // '    xstr &= oRow.Table.Columns(ix).ColumnName & "="
            // '    xstr &= oRow(ix) & ", "
            // 'Next

            // Dim cUpdtSQL As String = "UPDATE tblCartItem SET nPrice = " & oRow("price") & " WHERE nCartItemKey = " & oRow("id")
            // moDBHelper.exeProcessSQLScalar(cUpdtSQL)
            // Next


            // ' Quantity based error messaging
            // Dim oError As XmlElement = oCartElmt.SelectSingleNode("error")
            // If Not (oError Is Nothing) Then
            // Dim oMsg As XmlElement
            // oMsg = oCartElmt.OwnerDocument.CreateElement("msg")
            // oMsg.SetAttribute("type", "zz_footer") ' Forces it to the bottom of the message block
            // oMsg.InnerText = "Please adjust the quantities you require, or call for assistance."
            // oError.AppendChild(oMsg)
            // End If


            // 'moDBHelper.updateDataset(oDs, "Item", True)

            // '   add to Cart XML
            // sSql = "Select nCartStatus from tblCartOrder where nCartOrderKey = " & mnCartId
            // Dim nStatusId As Long = moDBHelper.DBN2Str(moDBHelper.exeProcessSQLScalar(sSql), False, False)
            // oCartElmt.SetAttribute("statusId", nStatusId)
            // oCartElmt.SetAttribute("status", Me.getProcessName(nStatusId))
            // oCartElmt.SetAttribute("itemCount", quant)
            // oCartElmt.SetAttribute("weight", weight)
            // oCartElmt.SetAttribute("orderType", mmcOrderType & "")

            // If nStatusId = 6 Then
            // oCartElmt.SetAttribute("complete", "true")
            // Else
            // oCartElmt.SetAttribute("complete", "true")
            // End If
            // 'Add the addresses to the dataset
            // If nCartIdUse > 0 Then
            // sSql = "select cContactType as type, cContactName as GivenName, cContactCompany as Company, cContactAddress as Street, cContactCity as City, cContactState as State, cContactZip as PostalCode, cContactCountry as Country, cContactTel as Telephone, cContactFax as Fax, cContactEmail as Email, cContactXml as Details from tblCartContact where nContactCartId=" & nCartIdUse
            // moDBHelper.addTableToDataSet(oDs, sSql, "Contact")
            // End If
            // 'Add Items - note - do this AFTER we've updated the prices! 

            // If oDs.Tables("Item").Rows.Count > 0 Then
            // 'cart items
            // oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(5).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(6).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(7).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(8).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(9).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(10).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns(11).ColumnMapping = Data.MappingType.Attribute
            // oDs.Tables(0).Columns("parId").ColumnMapping = Data.MappingType.Attribute
            // 'cart contacts
            // oDs.Tables("Contact").Columns(0).ColumnMapping = Data.MappingType.Attribute

            // oXml = New XmlDataDocument(oDs)
            // oDs.EnforceConstraints = False
            // 'Convert the detail to xml
            // For Each oElmt In oXml.SelectNodes("/Cart/Item/productDetail | /Cart/Contact/Detail | /Cart/Contact/Details")
            // oElmt.InnerXml = oElmt.InnerText
            // If Not oElmt.SelectSingleNode("Content") Is Nothing Then
            // oElmt.InnerXml = oElmt.SelectSingleNode("Content").InnerXml
            // End If
            // Next

            // 'get the option xml
            // For Each oElmt In oXml.SelectNodes("/Cart/Item/Item/productDetail")
            // oElmt.InnerXml = oElmt.InnerText

            // Dim nGroupIndex As String = oElmt.ParentNode.SelectSingleNode("nItemOptGrpIdx").InnerText
            // Dim nOptionIndex As String = oElmt.ParentNode.SelectSingleNode("nItemOptIdx").InnerText
            // cOptionGroupName = ""
            // If Not oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/@name") Is Nothing Then
            // cOptionGroupName = oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/@name").InnerText
            // End If
            // If nOptionIndex >= 0 Then

            // oElmt2 = oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/option[" & nOptionIndex & "]")
            // If cOptionGroupName <> "" Then oElmt2.SetAttribute("groupName", cOptionGroupName)
            // oElmt.ParentNode.InnerXml = oElmt2.OuterXml
            // Else
            // 'case for text option
            // oElmt2 = oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/option[1]")
            // If cOptionGroupName <> "" Then oElmt2.SetAttribute("groupName", cOptionGroupName)
            // oElmt2.SetAttribute("name", oElmt.ParentNode.SelectSingleNode("Name").InnerText)
            // oElmt.ParentNode.InnerXml = oElmt2.OuterXml
            // End If

            // Next

            // oElmt = moPageXml.CreateElement("Cart")
            // ' Note: Preserve the original elements in oCartElmt
            // 'oCartElmt.InnerXml = oCartElmt.InnerXml + oXml.FirstChild.InnerXml


            // End If
            // myWeb.CheckMultiParents(oCartElmt)
            // sSql = "select cClientNotes from tblCartOrder where nCartOrderKey=" & nCartIdUse
            // Dim oNotes As XmlElement = oCartElmt.OwnerDocument.CreateElement("Notes")
            // oNotes.InnerXml = moDBHelper.exeProcessSQLScalar(sSql)
            // total -= moDiscount.CheckDiscounts(oDs, oCartElmt, True, oNotes)

            // oXml = Nothing
            // oDs = Nothing

            // If mbNoDeliveryAddress Then oCartElmt.SetAttribute("hideDeliveryAddress", "true")
            // If mnGiftListId > 0 Then oCartElmt.SetAttribute("giftListId", mnGiftListId)

            // sSql = "select * from tblCartOrder where nCartOrderKey=" & nCartIdUse

            // oDs = moDBHelper.getDataSet(sSql, "Order", "Cart")

            // For Each oRow In oDs.Tables("Order").Rows
            // shipCost = CDbl("0" & oRow("nShippingCost"))
            // oCartElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
            // oCartElmt.SetAttribute("shippingCost", shipCost & "")
            // oCartElmt.SetAttribute("shippingDesc", oRow("cShippingDesc") & "")

            // If oRow("nShippingMethodId") > 0 Then
            // getShippingDetailXml(oCartElmt, oRow("nShippingMethodId"))
            // End If

            // If mnTaxRate > 0 Then
            // 'we calculate vat at the end after we have applied discounts etc
            // For Each oElmt In oXml.SelectNodes("/Cart/Item")
            // Dim nOpPrices As Long = 0

            // 'get the prices of options to calculate vat
            // For Each oElmt2 In oXml.SelectNodes("/Item")
            // nOpPrices += oElmt2.GetAttribute("price")
            // Next

            // If mbVatAtUnit Then
            // 'Round( Price * Vat ) * Quantity
            // nLineVat = Round((oElmt.GetAttribute("price") + nOpPrices) * (mnTaxRate / 100), , , mbRoundup) * oElmt.GetAttribute("quantity"))
            // Else
            // 'Round( ( Price * Quantity )* VAT )
            // nLineVat = Round((((oRow("price") + nOpPrices)) * oRow("quantity")) * (mnTaxRate / 100), , , mbRoundup)
            // End If

            // ' oElmt.SetAttribute("itemTax", nLineVat)
            // vatAmt += nLineVat
            // Next

            // vatAmt = Round((shipCost) * (mnTaxRate / 100), , , mbRoundup) + vatAmt

            // oCartElmt.SetAttribute("totalNet", FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False))
            // oCartElmt.SetAttribute("vatRate", mnTaxRate)
            // oCartElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
            // oCartElmt.SetAttribute("shippingCost", FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False))
            // oCartElmt.SetAttribute("vatAmt", FormatNumber(vatAmt, 2, TriState.True, TriState.False, TriState.False))
            // oCartElmt.SetAttribute("total", FormatNumber(total + shipCost + vatAmt, 2, TriState.True, TriState.False, TriState.False))
            // Else
            // oCartElmt.SetAttribute("totalNet", FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False))
            // oCartElmt.SetAttribute("vatRate", 0.0#)
            // oCartElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
            // oCartElmt.SetAttribute("shippingCost", FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False))
            // oCartElmt.SetAttribute("vatAmt", 0.0#)
            // oCartElmt.SetAttribute("total", FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False))
            // End If
            // Next
            // End If



            // Catch ex As Exception
            // returnException(myWeb.msException, mcModuleName, "GetCartSummary", ex, "", cProcessInfo, gbDebug)
            // End Try

            // End Sub

            /// <summary>
            /// This does the same as get cart without the item information, so we call get cart and delete any items we find
            /// </summary>
            /// <param name="oCartElmt"></param>
            /// <param name="nSelCartId"></param>
            /// <remarks></remarks>
            public void GetCartSummary(ref XmlElement oCartElmt, int nSelCartId = 0)
            {
                // Sets content for the XML to be displayed in the small summary plugin attached
                // to the current content page
                myWeb.PerfMon.Log("Cart", "GetCartSummary");
                int nCartIdUse;
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetCartSummary", ex, "", cProcessInfo, gbDebug);
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetCart", ex, "", "", gbDebug);
                }
            }

            public void GetCart(ref XmlElement oCartElmt, int nSelCartId = 0)
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

                int nCartIdUse;
                if (nSelCartId > 0)
                {
                    nCartIdUse = nSelCartId;
                }
                else
                {
                    nCartIdUse = mnCartId;
                }

                long oldCartId = mnCartId;
                long ShippingOptionKey = Conversions.ToLong(moCartConfig["DefaultShippingMethod"]);
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
                        ReceiptDeliveryType = 0;
                        // Process promo code from external refs.
                        if (Conversions.ToBoolean(Operators.OrObject(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["promocode"], "", false), !string.IsNullOrEmpty(myWeb.moRequest["promocode"]))))
                        {
                            if (!string.IsNullOrEmpty(myWeb.moRequest["promocode"]))
                            {
                                promocodeFromExternalRef = myWeb.moRequest["promocode"].ToString();
                            }
                            else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["promocode"], "", false)))
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
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(moDBHelper.DBN2int(oRow["nParentId"]), 0, false)))
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
                                        oProd.InnerXml = Conversions.ToString(oRow["productDetail"]);
                                        if (oProd.SelectSingleNode("Content[@overridePrice='true']") is null && oProd.SelectSingleNode("Content[contains(@action,'VariableSubscription')]") is null)
                                        {
                                            oCheckPrice = getContentPricesNode(oProd, Conversions.ToString(Operators.ConcatenateObject(oRow["unit"], "")), Conversions.ToLong(oRow["quantity"]));
                                            cProcessInfo = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Error getting price for unit:", oRow["unit"]), " and Quantity:"), oRow["quantity"]), " and Currency "), mcCurrencyRef), " Check that a price is available for this quantity and a group for this current user."));
                                            if (oCheckPrice != null)
                                            {
                                                nCheckPrice = Conversions.ToDouble(oCheckPrice.InnerText);
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

                                            if (moSubscription != null & Conversions.ToString(Operators.ConcatenateObject(oRow["contentType"], "")) == "Subscription")
                                            {
                                                if (moSubscription.mbOveridePrices == false)
                                                {
                                                    // TS added when subscription when initial cost is changed in by external logic we should not refer back to the stored content.
                                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(oRow["contentId"], 0, false)))
                                                    {
                                                        revisedPrice = moSubscription.CartSubscriptionPrice(Conversions.ToInteger(oRow["contentId"]), myWeb.mnUserId);
                                                    }
                                                    else
                                                    {
                                                        oCheckPrice = getContentPricesNode(oProd, Conversions.ToString(Operators.ConcatenateObject(oRow["unit"], "")), Conversions.ToLong(oRow["quantity"]), "SubscriptionPrices");
                                                        nCheckPrice = Conversions.ToDouble(oCheckPrice.InnerText);
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
                                        if (Conversions.ToBoolean(Operators.AndObject(nCheckPrice > 0d, Operators.ConditionalCompareObjectNotEqual(nCheckPrice, oRow["price"], false))))
                                        {
                                            // If price is lower, then update the item price field
                                            oRow["price"] = nCheckPrice;
                                        }
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oRow["taxRate"], nTaxRate, false)))
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
                                        decimal nNPrice = (decimal)getOptionPricesByXml(Conversions.ToString(oRow["productDetail"]), Conversions.ToInteger(oRow["nItemOptGrpIdx"]), Conversions.ToInteger(oRow["nItemOptIdx"]));
                                        if (Conversions.ToBoolean(Operators.AndObject(nNPrice > 0m, Operators.ConditionalCompareObjectNotEqual(nNPrice, oOpRow["price"], false))))
                                        {
                                            nOpPrices += nNPrice;
                                            // oOpRow.BeginEdit()
                                            oOpRow["price"] = nNPrice;
                                        }
                                        // oOpRow.EndEdit()

                                        else if (moCartConfig["ProductOptionOverideQuantity"] == "on")
                                        {
                                            nOpPrices = Conversions.ToDecimal(nOpPrices + Convert.ToDecimal(Operators.MultiplyObject(oOpRow["price"], oOpRow["quantity"])));
                                        }
                                        else
                                        {
                                            nOpPrices = Conversions.ToDecimal(nOpPrices + Convert.ToDecimal(oOpRow["price"]));

                                        }
                                    }
                                    else if (Conversions.ToBoolean(Operators.AndObject(moCartConfig["ProductOptionOverideQuantity"] == "on", Operators.ConditionalCompareObjectGreater(oOpRow["quantity"], 1, false))))
                                    {
                                        nOpPrices = Conversions.ToDecimal(nOpPrices + Convert.ToDecimal(Operators.MultiplyObject(oOpRow["price"], oOpRow["quantity"])));
                                        // Else
                                        // nOpPrices += (oOpRow("price"))
                                    }
                                }

                                // Apply stock control
                                if (mbStockControl)
                                    CheckStock(ref oCartElmt, Conversions.ToString(oRow["productDetail"]), Conversions.ToString(oRow["quantity"]));
                                // Apply quantity control
                                if (!(oRow["productDetail"] is DBNull))
                                {
                                    // not sure why the product has no detail but if it not we skip this, suspect it was old test data that raised this issue.
                                    CheckQuantities(ref oCartElmt, Conversions.ToString(Operators.ConcatenateObject(oRow["productDetail"], "")), Conversions.ToLong(Operators.ConcatenateObject("0", oRow["quantity"])).ToString());
                                }

                                weight += Convert.ToInt32(oRow["weight"]) * Convert.ToInt32(oRow["quantity"]);
                                quant += Convert.ToInt32(oRow["quantity"]);
                                if (moCartConfig["ProductOptionOverideQuantity"] == "on")
                                {
                                    total = total + Conversions.ToDouble(Operators.AddObject(Operators.MultiplyObject(oRow["quantity"], Round(oRow["price"], bForceRoundup: mbRoundup)), Round(nOpPrices, bForceRoundup: mbRoundup)));
                                }
                                else
                                {
                                    total = total + Conversions.ToDouble(Operators.MultiplyObject(oRow["quantity"], Round(Operators.AddObject(oRow["price"], nOpPrices), bForceRoundup: mbRoundup)));
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
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oRow["price"], null, false)))
                                {
                                    // If oRow("price") <> 0 Then
                                    string discountSQL = "";
                                    if (Discount != 0d)
                                    {
                                        // discountSQL = ", nDiscountValue = " & Discount & " "
                                    }
                                    string cUpdtSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE tblCartItem Set nPrice = ", oRow["price"]), discountSQL), " WHERE nCartItemKey = "), oRow["id"]));
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
                        nStatusId = Conversions.ToLong(moDBHelper.DBN2Str(moDBHelper.ExeProcessSqlScalar(sSql), false, false));
                        // moCartConfig("OrderPaymentStatusId") = nStatusId
                        oCartElmt.SetAttribute("statusId", nStatusId.ToString());
                        oCartElmt.SetAttribute("status", getProcessName((cartProcess)nStatusId));
                        oCartElmt.SetAttribute("itemCount", quant.ToString());
                        oCartElmt.SetAttribute("weight", weight.ToString());
                        oCartElmt.SetAttribute("orderType", mmcOrderType + "");

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
                                if (Conversions.ToDouble(nOptionIndex) >= 0d)
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
                            shipCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oRow["nShippingCost"]));
                            oCartElmt.SetAttribute("shippingType", Conversions.ToString(Operators.ConcatenateObject(oRow["nShippingMethodId"], "")));
                            oCartElmt.SetAttribute("shippingCost", shipCost + "");
                            oCartElmt.SetAttribute("shippingDesc", Conversions.ToString(Operators.ConcatenateObject(oRow["cShippingDesc"], "")));

                            if (moDBHelper.checkTableColumnExists("tblCartOrder", "nReceiptType"))
                            {
                                if (oRow["nReceiptType"] is DBNull)
                                {
                                    ReceiptDeliveryType = 1;
                                }
                                else
                                {
                                    ReceiptDeliveryType = Conversions.ToShort(oRow["nReceiptType"]);
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
                                }
                                if (!string.IsNullOrEmpty(cDestinationCountry))
                                {
                                    // Go and collect the valid shipping options available for this order
                                    int productId = 0;
                                    var oDsShipOptions = getValidShippingOptionsDS(cDestinationCountry, cDestinationPostalCode, total, quant, weight, cPromoCode, productId);
                                    if (oDsShipOptions != null)
                                    {
                                        foreach (DataRow oRowSO in oDsShipOptions.Tables[0].Rows)
                                        {
                                            bool bCollection = false;
                                            if (!(oRowSO["bCollection"] is DBNull))
                                            {
                                                bCollection = Conversions.ToBoolean(oRowSO["bCollection"]);
                                            }
                                            if (oRowSO.Table.Columns.Contains("nShippingGroup")) {
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
                                                            shipCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oRowSO["nShipOptCost"]));
                                                            oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig["DefaultCountry"]);
                                                            oCartElmt.SetAttribute("shippingType", ShippingOptionKey + "");
                                                            oCartElmt.SetAttribute("shippingCost", shipCost + "");
                                                            oCartElmt.SetAttribute("shippingDesc", Conversions.ToString(Operators.ConcatenateObject(oRowSO["cShipOptName"], "")));
                                                            oCartElmt.SetAttribute("shippingCarrier", Conversions.ToString(Operators.ConcatenateObject(oRowSO["cShipOptCarrier"], "")));
                                                            // oCartElmt.SetAttribute("cCatSchemaName", cCartType & "")
                                                        }
                                                    }
                                                }
                                                else if (oCartElmt.HasAttribute("shippingType") & oCartElmt.GetAttribute("shippingType") == "0")
                                                {
                                                    if (Convert.ToString(oRowSO["nShipOptKey"]) == moCartConfig["DefaultShippingMethod"])
                                                    {
                                                        shipCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oRowSO["nShipOptCost"]));
                                                        oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig["DefaultCountry"]);
                                                        oCartElmt.SetAttribute("shippingType", moCartConfig["DefaultShippingMethod"] + "");
                                                        oCartElmt.SetAttribute("shippingCost", shipCost + "");
                                                        oCartElmt.SetAttribute("shippingDesc", Conversions.ToString(Operators.ConcatenateObject(oRowSO["cShipOptName"], "")));
                                                        oCartElmt.SetAttribute("shippingCarrier", Conversions.ToString(Operators.ConcatenateObject(oRowSO["cShipOptCarrier"], "")));
                                                    }
                                                }
                                                // Add extra condition only when promocode is valid
                                                // Set nondiscountedshippingcost to attribute when promocode is valid(include free shipping methods)
                                                else if (IsPromocodeValid = true & Convert.ToString(oRowSO["NonDiscountedShippingCost"]) != "0")
                                                {
                                                    if (oCartElmt.GetAttribute("freeShippingMethods").Contains(oCartElmt.GetAttribute("shippingType")))
                                                    {
                                                        shipCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oRowSO["nShipOptCost"]));
                                                        oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig["DefaultCountry"]);
                                                        oCartElmt.SetAttribute("shippingType", Conversions.ToString(Operators.ConcatenateObject(oRowSO["nShipOptKey"], "")));
                                                        oCartElmt.SetAttribute("shippingCost", shipCost + "");
                                                        oCartElmt.SetAttribute("shippingDesc", Conversions.ToString(Operators.ConcatenateObject(oRowSO["cShipOptName"], "")));
                                                        oCartElmt.SetAttribute("shippingCarrier", Conversions.ToString(Operators.ConcatenateObject(oRowSO["cShipOptCarrier"], "")));
                                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oRowSO["NonDiscountedShippingCost"], "0", false)))
                                                        {
                                                            oCartElmt.SetAttribute("NonDiscountedShippingCost", Conversions.ToString(Operators.ConcatenateObject(oRowSO["NonDiscountedShippingCost"], "")));
                                                        }
                                                    }
                                                }
                                            }
                                            else if ((shipCost == -1 || Convert.ToDouble("0" + oRowSO["nShipOptCost"].ToString()) < shipCost) & bCollection == false)
                                            {
                                                shipCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oRowSO["nShipOptCost"]));
                                                oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig["DefaultCountry"]);
                                                oCartElmt.SetAttribute("shippingType", Conversions.ToString(Operators.ConcatenateObject(oRowSO["nShipOptKey"], "")));
                                                oCartElmt.SetAttribute("shippingCost", shipCost + "");
                                                oCartElmt.SetAttribute("shippingDesc", Conversions.ToString(Operators.ConcatenateObject(oRowSO["cShipOptName"], "")));
                                                oCartElmt.SetAttribute("shippingCarrier", Conversions.ToString(Operators.ConcatenateObject(oRowSO["cShipOptCarrier"], "")));
                                                // oCartElmt.SetAttribute("cCatSchemaName", "" & "")
                                            }

                                        }
                                    }
                                }
                                if (shipCost == -1)
                                    shipCost = 0d;
                            }

                            if (Conversions.ToDouble(oCartElmt.GetAttribute("shippingType")) > 0d)
                            {
                                getShippingDetailXml(ref oCartElmt, Conversions.ToLong(oCartElmt.GetAttribute("shippingType")));
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
                                        if (Conversions.ToDouble(mcDepositAmount) == 0d)
                                        {

                                            nPayableAmount = 0d;
                                            if (moDBHelper.checkTableColumnExists("tblCartItem", "nDepositAmount"))
                                            {
                                                foreach (XmlElement oItem in oCartElmt.SelectNodes("Item"))
                                                {
                                                    if (oItem.SelectSingleNode("nDepositAmount") is null)
                                                    {
                                                        nPayableAmount = nPayableAmount + Conversions.ToDouble(oItem.GetAttribute("itemTotal")) * Conversions.ToLong(oItem.GetAttribute("quantity"));
                                                    }
                                                    else
                                                    {
                                                        nPayableAmount = nPayableAmount + Conversions.ToDouble(oItem.SelectSingleNode("nDepositAmount").InnerText) * Conversions.ToLong(oItem.GetAttribute("quantity"));
                                                    }
                                                }
                                            }


                                            nPayable = nPayableAmount;
                                        }

                                        else if (Strings.Right(mcDepositAmount, 1) == "%")
                                        {
                                            if (Information.IsNumeric(Strings.Left(mcDepositAmount, Strings.Len(mcDepositAmount) - 1)))
                                            {
                                                nPayable = nTotalAmount * Conversions.ToDouble(Strings.Left(mcDepositAmount, Strings.Len(mcDepositAmount) - 1)) / 100d;
                                            }
                                        }
                                        else if (Information.IsNumeric(mcDepositAmount))
                                            nPayable = Conversions.ToDouble(mcDepositAmount);

                                        if (nPayable > nTotalAmount)
                                            nPayable = nTotalAmount;

                                        // Set the Payable Amount
                                        if (nPayable > 0d & nPayable < nTotalAmount)
                                        {
                                            oCartElmt.SetAttribute("payableType", "deposit");
                                            oCartElmt.SetAttribute("payableAmount", Strings.FormatNumber(nPayable, 2, TriState.True, TriState.False, TriState.False));
                                            oCartElmt.SetAttribute("paymentMade", "0");
                                        }

                                    }
                                }
                                // A deposit has been paid - should I check if it's the same as the total amount?
                                else if (Information.IsNumeric(oRow["nAmountReceived"]))
                                {
                                    nPayable = nTotalAmount - Conversions.ToDouble(oRow["nAmountReceived"]);
                                    oCartElmt.SetAttribute("payableAmount", Strings.FormatNumber(nPayable, 2, TriState.True, TriState.False, TriState.False));
                                    oCartElmt.SetAttribute("outstandingAmount", Strings.FormatNumber(nPayable, 2, TriState.True, TriState.False, TriState.False));

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
                                    oCartElmt.SetAttribute("paymentMade", Strings.FormatNumber(Conversions.ToDouble(oRow["nAmountReceived"]), 2, TriState.True, TriState.False, TriState.False));
                                    oCartElmt.SetAttribute("payableType", "settlement");
                                }

                                // Set the payableType 
                                if (!Information.IsNumeric(oRow["nAmountReceived"]) && nStatusId != 10L)
                                {
                                    oCartElmt.SetAttribute("payableType", "deposit");
                                }
                                else
                                {
                                    oCartElmt.SetAttribute("payableType", "settlement");
                                }

                                // TS added for additional orders not sure if this will break elsewhere.
                                if (!Information.IsNumeric(oRow["nAmountReceived"]) && nStatusId == 10L)
                                {
                                    oCartElmt.SetAttribute("payableType", "deposit");
                                }

                                if (nPayable == 0d | nPayable == Conversions.ToDouble(oCartElmt.GetAttribute("total")))
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
                                oElmt.InnerXml = Conversions.ToString(oRow["cClientNotes"]);
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
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(oRow["nPayMthdId"], 0, false)))
                            {
                                sSql = Conversions.ToString(Operators.ConcatenateObject("Select * from tblCartPaymentMethod where nPayMthdKey=", oRow["nPayMthdId"]));
                                oDs2 = moDBHelper.GetDataSet(sSql, "Payment", "Cart");
                                oElmt = moPageXml.CreateElement("PaymentDetails");
                                foreach (DataRow currentORow2 in oDs2.Tables["Payment"].Rows)
                                {
                                    oRow2 = currentORow2;
                                    oElmt.InnerXml = Conversions.ToString(oRow2["cPayMthdDetailXml"]);
                                    oElmt.SetAttribute("provider", Conversions.ToString(oRow2["cPayMthdProviderName"]));
                                    oElmt.SetAttribute("ref", Conversions.ToString(oRow2["cPayMthdProviderRef"]));
                                    oElmt.SetAttribute("acct", Conversions.ToString(oRow2["cPayMthdAcctName"]));
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
                                    oElmt.SetAttribute("carrierName", Conversions.ToString(oRow2["cCarrierName"]));
                                    oElmt.SetAttribute("ref", Conversions.ToString(oRow2["cCarrierRef"]));
                                    oElmt.SetAttribute("notes", Conversions.ToString(oRow2["cCarrierNotes"]));
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
                                        eventDate = Conversions.ToDate(itemElmt.SelectSingleNode("productDetail/StartDate").InnerText);
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
                                        DateInterval = Conversions.ToDouble(moCartConfig["SettlementDays"]);
                                    }
                                    dDueDate = DateAndTime.DateAdd(VB.DateInterval.Day, DateInterval * -1, dDueDate);
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
                    if (Conversions.ToBoolean(bCheckSubscriptions))
                    {
                        moSubscription.UpdateSubscriptionsTotals(ref oCartElmt);
                    }


                    mnCartId = (int)oldCartId;
                    SaveCartXML(oCartElmt);
                    // mnCartId = nCartIdUse

                    if (moCartConfig["RelatedProductsInCart"] == "On")
                    {
                        var oRelatedElmt = oCartElmt.OwnerDocument.CreateElement("RelatedItems");
                        for (int i = 0, loopTo = oItemList.Count - 1; i <= loopTo; i++)
                            myWeb.moDbHelper.addRelatedContent(ref oRelatedElmt, Conversions.ToInteger(oItemList[i]), false);
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

                    // sonalis code for session set
                    if (System.Web.HttpContext.Current.Request.Cookies["Flag"] is null)
                    {

                        var flagCookie = new System.Web.HttpCookie("Flag");
                        flagCookie.Value = "1";
                        System.Web.HttpContext.Current.Response.Cookies.Add(flagCookie);

                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "GetCart", ex, "", cProcessInfo, gbDebug);
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
                                nOpPrices = (long)Math.Round(nOpPrices + Conversions.ToDouble(oElmt2.GetAttribute("price")));
                            double nItemDiscount = 0d;

                            double nLineTaxRate = mnTaxRate;
                            if (mbVatOnLine)
                            {
                                nLineTaxRate = Conversions.ToDouble(oElmt.GetAttribute("taxRate"));
                            }
                            if (oElmt.SelectSingleNode("productDetail[@overideTaxRate!='']") != null)
                            {
                                XmlElement detailElmt = (XmlElement)oElmt.SelectSingleNode("productDetail");
                                nLineTaxRate = Conversions.ToDouble(detailElmt.GetAttribute("overideTaxRate"));
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
                                    nLineVat = (double)Round((Conversions.ToDouble(oElmt.GetAttribute("price")) - nItemDiscount + nOpPrices) * (nLineTaxRate / 100d), bForceRoundup: mbRoundup) * Conversions.ToDouble(oDiscItem.GetAttribute("Units"));
                                }
                                else
                                {
                                    // nLineVat = Round((oElmt.GetAttribute("price") - nItemDiscount + nOpPrices) * (mnTaxRate / 100), , , mbRoundup) * oDiscItem.GetAttribute("Units")

                                    nLineVat = (double)Round(Conversions.ToDouble(oDiscItem.GetAttribute("Total")) * (nLineTaxRate / 100d), bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown);

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
                                nLineVat = (double)Round((Conversions.ToDouble(oElmt.GetAttribute("price")) - nItemDiscount + nOpPrices) * (nLineTaxRate / 100d), bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown) * Conversions.ToDouble(oElmt.GetAttribute("quantity"));
                            }
                            else
                            {
                                // Round( ( Price * Quantity )* VAT )
                                nLineVat = (double)Round((Conversions.ToDouble(oElmt.GetAttribute("price")) - nItemDiscount + nOpPrices) * Conversions.ToDouble(oElmt.GetAttribute("quantity")) * (nLineTaxRate / 100d), bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown);
                            }


                            oElmt.SetAttribute("itemTax", nLineVat.ToString());
                            vatAmt += nLineVat;
                        }

                        if (!(Strings.LCase(moCartConfig["DontTaxShipping"]) == "on"))
                        {
                            vatAmt = (double)(Round(shipCost * (mnTaxRate / 100d), bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown) + Round(vatAmt, bForceRoundup: mbRoundup, bForceRoundDown: mbRoundDown));
                        }

                        oCartElmt.SetAttribute("totalNet", Strings.FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False));
                        oCartElmt.SetAttribute("vatRate", mnTaxRate.ToString());
                        oCartElmt.SetAttribute("shippingType", ShipMethodId + "");
                        oCartElmt.SetAttribute("shippingCost", Strings.FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False));
                        oCartElmt.SetAttribute("vatAmt", Strings.FormatNumber(vatAmt, 2, TriState.True, TriState.False, TriState.False));
                        oCartElmt.SetAttribute("total", Strings.FormatNumber(total + shipCost + vatAmt, 2, TriState.True, TriState.False, TriState.False));
                        oCartElmt.SetAttribute("currency", mcCurrencyCode);
                        oCartElmt.SetAttribute("currencySymbol", mcCurrencySymbol);
                    }
                    else
                    {
                        oCartElmt.SetAttribute("totalNet", Strings.FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False));
                        oCartElmt.SetAttribute("vatRate", 0.0d.ToString());
                        oCartElmt.SetAttribute("shippingType", ShipMethodId + "");
                        oCartElmt.SetAttribute("shippingCost", Strings.FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False));
                        oCartElmt.SetAttribute("vatAmt", 0.0d.ToString());
                        oCartElmt.SetAttribute("total", Strings.FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False));
                        oCartElmt.SetAttribute("currency", mcCurrencyCode);
                        oCartElmt.SetAttribute("currencySymbol", mcCurrencySymbol);
                    }

                    return vatAmt;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updateTotals", ex, "", cProcessInfo, gbDebug);
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getShippingDetailXml", ex, "", cProcessInfo, gbDebug);
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

                    if (Information.IsNumeric(oThePrice.InnerText))
                    {
                        nPrice = Conversions.ToDouble(oThePrice.InnerText);
                    }

                    return nPrice;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getProductPricesByXml", ex, "", "", gbDebug);
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
                            case var @case when @case == (Strings.LCase("s") ?? ""):
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
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getProductTaxRate", ex, "", "", gbDebug);
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
                    cGroups = " and ( contains(@validGroup,\"" + Strings.Replace(cGroups, ",", "\") or contains(@validGroup,\"");
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
                    else {
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
                                int nThisMin = Conversions.ToInteger(Interaction.IIf(string.IsNullOrEmpty(oPNode.GetAttribute("min")), 1, oPNode.GetAttribute("min")));
                                int nThisMax = Conversions.ToInteger(Interaction.IIf(string.IsNullOrEmpty(oPNode.GetAttribute("max")), 0, oPNode.GetAttribute("max")));
                                if (nThisMin <= nQuantity & (nThisMax >= nQuantity | nThisMax == 0))
                                {
                                    // now we know it is a valid split
                                    bValidSplit = true;
                                }
                            }
                        }
                        if (oThePrice != null)
                        {
                            if (Information.IsNumeric(oThePrice.InnerText))
                            {
                                // this selects the cheapest price for this user assuming not free
                                if (Information.IsNumeric(oPNode.InnerText))
                                {
                                    // if OverrideCheapestPrice is "on" - we will ensure that when sales price is greater than rrp - highest(sales) price is considered.
                                    if (!(moCartConfig["OverrideCheapestPrice"] == null) & moCartConfig["OverrideCheapestPrice"] == "on")
                                    {
                                        if (Conversions.ToDouble(oPNode.InnerText) < Conversions.ToDouble(oThePrice.InnerText) & Conversions.ToLong(oPNode.InnerText) != 0L)
                                        {
                                            string oThePriceType = oThePrice.GetAttribute("type");
                                            string oPNodeType = oPNode.GetAttribute("type");

                                            if (!(oPNodeType == "rrp" & oThePriceType == "sale"))
                                            {
                                                oThePrice = oPNode;
                                            }
                                        }
                                    }
                                    else if (Conversions.ToDouble(oPNode.InnerText) < Conversions.ToDouble(oThePrice.InnerText) & Conversions.ToLong(oPNode.InnerText) != 0L)
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
                    cGroups = " and ( contains(@validGroup,\"" + Strings.Replace(cGroups, ",", "\") or contains(@validGroup,\"");
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
                            if (Information.IsNumeric(oThePrice.InnerText))
                            {
                                if (Conversions.ToDouble(oPNode.InnerText) < Conversions.ToDouble(oThePrice.InnerText))
                                {
                                    oThePrice = oPNode;
                                    nPrice = Conversions.ToDouble(oThePrice.InnerText);
                                }
                            }
                            else
                            {
                                oThePrice = oPNode;
                                nPrice = Conversions.ToDouble(oThePrice.InnerText);
                            }
                        }
                        else
                        {
                            oThePrice = oPNode;
                            if (Information.IsNumeric(oThePrice.InnerText))
                            {
                                nPrice = Conversions.ToDouble(oThePrice.InnerText);
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

                    if (Information.IsNumeric(cItemQuantity))
                    {

                        // Check minimum value
                        if (Conversions.ToLong(cItemQuantity) < Conversions.ToLong(getNodeValueByType(ref oProd, "//Quantities/Minimum", XmlDataType.TypeNumber, 0)))
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
                            oMsg = addElement(ref oError, "msg", Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("<strong>", getNodeValueByType(ref oProd, "/Content/Name", XmlDataType.TypeString, "A product below ")), "</strong> requires a quantity equal to or above <em>"), getNodeValueByType(ref oProd, "//Quantities/Minimum", XmlDataType.TypeNumber, "an undetermined value (please call for assistance).")), "</em>")), true);
                            oMsg.SetAttribute("type", "quantity_min_detail");

                        }

                        // Check maximum value
                        if (Conversions.ToLong(cItemQuantity) > Conversions.ToLong(getNodeValueByType(ref oProd, "//Quantities/Maximum", XmlDataType.TypeNumber, int.MaxValue)))
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
                            oMsg = addElement(ref oError, "msg", Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("<strong>", getNodeValueByType(ref oProd, "/Content/Name", XmlDataType.TypeString, "A product below ")), "</strong> requires a quantity equal to or below <em>"), getNodeValueByType(ref oProd, "//Quantities/Maximum", XmlDataType.TypeNumber, "an undetermined value (please call for assistance).")), "</em>")), true);
                            oMsg.SetAttribute("type", "quantity_max_detail");

                        }

                        // Check bulkunit value
                        int cBulkUnit = Conversions.ToInteger(getNodeValueByType(ref oProd, "//Quantities/BulkUnit", XmlDataType.TypeNumber, 0));
                        if (Conversions.ToLong(cItemQuantity) % Conversions.ToLong(getNodeValueByType(ref oProd, "//Quantities/BulkUnit", XmlDataType.TypeNumber, 1)) != 0L)
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
                            oMsg = addElement(ref oError, "msg", Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("<strong>", getNodeValueByType(ref oProd, "/Content/Name", XmlDataType.TypeString, "A product below ")), "</strong> can only be bought in lots of <em>"), getNodeValueByType(ref oProd, "//Quantities/BulkUnit", XmlDataType.TypeNumber, "an undetermined value (please call for assistance).")), "</em>")), true);
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
                                if (Information.IsNumeric(oStock.InnerText))
                                {
                                    StockLevel = Conversions.ToLong(oStock.InnerText);
                                }
                            }
                        }
                        else
                        {
                            // step through locations to get total quantity
                            foreach (XmlNode currentOStock in oProd.SelectNodes("//Stock/Location"))
                            {
                                oStock = currentOStock;
                                if (Information.IsNumeric(oStock.InnerText))
                                {
                                    if (StockLevel == default)
                                        StockLevel = 0L;
                                    StockLevel = StockLevel + Conversions.ToLong(oStock.InnerText);
                                }
                            }
                        }

                        if (StockLevel != default)
                        {
                            // If the requested quantity is greater than the stock level, add a warning to the cart - only check tihs on an active cart.
                            if (Conversions.ToLong(cItemQuantity) > StockLevel & mnProcessId < 6)
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
                                if (Conversions.ToString(oRow["cContentXmlDetail"]) != "") { 
                                    oProd.InnerXml = Conversions.ToString(oRow["cContentXmlDetail"]);                                
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
                                    if (Information.IsNumeric(oStock.InnerText))
                                    {
                                        nStockLevel = Conversions.ToInteger(oStock.InnerText) - Conversions.ToInteger(oItem.GetAttribute("quantity"));
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
                                oProd.InnerXml = Conversions.ToString(oRow["cContentXmlBrief"]);

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
                                    if (Information.IsNumeric(oStock.InnerText))
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

                            nNewQty = Conversions.ToDecimal(oRow["nQuantity"]);

                            sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("select * from tblCartItem where nCartOrderId=" + mnGiftListId + " and nItemId =" + oRow["nItemId"].ToString() + " and cItemOption1='", SqlFmt(oRow["cItemOption1"].ToString())), "' and cItemOption2='"), SqlFmt(oRow["cItemOption2"].ToString())), "'"));
                            using (var oDr2 = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                            {
                                while (oDr2.Read())
                                    nNewQty = Conversions.ToDecimal(Operators.SubtractObject(oDr2["nQuantity"], oRow["nQuantity"]));

                                sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Update tblCartItem set nQuantity = " + nNewQty.ToString() + " where nCartOrderId=" + mnGiftListId + " and nItemId =" + oRow["nItemId"].ToString() + " and cItemOption1='", SqlFmt(oRow["cItemOption1"].ToString())), "' and cItemOption2='"), SqlFmt(oRow["cItemOption2"].ToString())), "'"));
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
                            cReturn = Conversions.ToString(Operators.ConcatenateObject(cReturn + ",", oDr["cDirName"]));
                        cReturn = Strings.Mid(cReturn, 2);
                    }

                    return cReturn;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getGroupsByName", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
            }

            public virtual void addressSubProcess(ref XmlElement oCartElmt, string cAddressType)
            {

                Cms.xForm oContactXform;
                string submitPrefix = "cartBill";
                string cProcessInfo = submitPrefix;
                PaymentProviders oPay;
                string buttonRef = "";
                bool bSubmitPaymentMethod = false;

                try
                {
                    myWeb.moSession["tempInstance"] = null;


                    if (cAddressType.Contains("Delivery"))
                        submitPrefix = "cartDel";
                    if (mbEwMembership == true & myWeb.mnUserId != 0 & submitPrefix != "cartDel")
                    {
                        // we now only need this on delivery.
                        oContactXform = pickContactXform(cAddressType, submitPrefix, cCmdAction: mcCartCmd);
                        GetCart(ref oCartElmt);
                    }
                    else
                    {
                        oContactXform = contactXform(cAddressType, "submit", "cartCmd", mcCartCmd);

                        if (moPay is null)
                        {
                            oPay = new PaymentProviders(ref myWeb);
                        }
                        else
                        {
                            oPay = moPay;
                        }
                        oPay.mcCurrency = mcCurrency;

                        GetCart(ref oCartElmt);
                        if (Strings.LCase(moCartConfig["PaymentTypeButtons"]) == "on")
                        {
                            if (oCartElmt.SelectSingleNode("Shipping") != null & string.IsNullOrEmpty(moCartConfig["TermsContentId"]) & string.IsNullOrEmpty(moCartConfig["TermsAndConditions"]))
                            {
                                // we already have shipping selected threfore we can skip Options Xform
                                XmlElement oSubmitBtn = (XmlElement)oContactXform.moXformElmt.SelectSingleNode("descendant-or-self::submit[@submission='SubmitAdd']");
                                buttonRef = oSubmitBtn.GetAttribute("ref");
                                double PaymentAmount = Conversions.ToDouble("0" + oCartElmt.GetAttribute("total"));
                                XmlElement xmlParentNodeElmt = (XmlElement)oSubmitBtn.ParentNode;
                                oPay.getPaymentMethodButtons(ref oContactXform, ref xmlParentNodeElmt, PaymentAmount);
                                bSubmitPaymentMethod = true;
                            }
                        }
                    }

                    if (oContactXform.valid == false)
                    {
                        // show the form
                        XmlElement oContentElmt = (XmlElement)moPageXml.SelectSingleNode("/Page/Contents");
                        if (oContentElmt is null)
                        {
                            oContentElmt = moPageXml.CreateElement("Contents");
                            if (moPageXml.DocumentElement is null)
                            {
                                Information.Err().Raise(1004, "addressSubProcess", " PAGE IS NOT CREATED");
                            }
                            else
                            {
                                moPageXml.DocumentElement.AppendChild(oContentElmt);
                            }
                        }
                        oContentElmt.AppendChild(oContactXform.moXformElmt);
                    }
                    else
                    {
                        if (!cAddressType.Contains("Delivery"))
                        {

                            // Valid Form, let's adjust the Vat rate
                            // AJG By default the tax rate is picked up from the billing address, unless otherwise specified.
                            // 
                            if (moCartConfig["TaxFromDeliveryAddress"] != "on" | myWeb.moRequest["cIsDelivery"] == "True" & moCartConfig["TaxFromDeliveryAddress"] == "on")
                            {
                                if (oContactXform.Instance.SelectSingleNode("tblCartContact[@type='Billing Address']") != null)
                                {
                                    if (oCartElmt.SelectSingleNode("Contact[@type='Billing Address']") != null)
                                    {
                                        string argcContactCountry = oCartElmt.SelectSingleNode("Contact[@type='Billing Address']/Country").InnerText;
                                        UpdateTaxRate(ref argcContactCountry);
                                        oCartElmt.SelectSingleNode("Contact[@type='Billing Address']/Country").InnerText = argcContactCountry;
                                    }
                                }
                            }

                            // to allow for single form with multiple addresses.
                            if (moCartConfig["TaxFromDeliveryAddress"] == "on" & oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']") != null)
                            {
                                string argcContactCountry1 = oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText;
                                UpdateTaxRate(ref argcContactCountry1);
                                oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText = argcContactCountry1;
                            }

                            // Skip Delivery if:
                            // - Deliver to this address is selected
                            // - mbNoDeliveryAddress is True
                            // - the order is part of a giftlist (the delivery address is pre-determined)
                            // - we have submitted the delivery address allready
                            if (myWeb.moRequest["cIsDelivery"] == "True" | mbNoDeliveryAddress | mnGiftListId > 0 | oContactXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='Delivery Address']") != null | oContactXform.moXformElmt.GetAttribute("cartCmd") == "ChoosePaymentShippingOption")




                            {

                                if (bSubmitPaymentMethod)
                                {
                                    // we have payment method buttons on the form.
                                    mcPaymentMethod = myWeb.moRequest[buttonRef];
                                }

                                mcCartCmd = "ChoosePaymentShippingOption";
                                mnProcessId = 3;
                            }

                            else
                            {
                                // If mbEwMembership = True And myWeb.mnUserId <> 0 Then
                                // 'all handled in pick form
                                // Else
                                long BillingAddressID = setCurrentBillingAddress((long)myWeb.mnUserId, 0L);
                                if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + BillingAddressID]))
                                {
                                    // we are editing an address form the pick address form so lets go back.
                                    mcCartCmd = "Billing";
                                    mnProcessId = 2;
                                }
                                else
                                {
                                    mcCartCmd = "Delivery";
                                }



                                // End If
                                // billing address is saved, so up cart status if needed
                                // If mnProcessId < 2 Then mnProcessId = 2
                                // If mnProcessId > 2 Then
                                // mcCartCmd = "ChoosePaymentShippingOption"
                                // mnProcessId = 3
                                // End If

                            }
                            if (myWeb.mnUserId > 0)
                            {
                                setCurrentBillingAddress((long)myWeb.mnUserId, 0L);
                            }
                        }


                        else // Case for Delivery
                        {

                            // AJG If specified, the tax rate can be picked up from the delivery address
                            if (moCartConfig["TaxFromDeliveryAddress"] == "on")
                            {
                                string argcContactCountry2 = oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText;
                                UpdateTaxRate(ref argcContactCountry2);
                                oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText = argcContactCountry2;
                            }

                            // TS if we don't have a billing address we need one
                            DataSet oDs2;
                            oDs2 = moDBHelper.GetDataSet("select * from tblCartContact where nContactCartId = " + mnCartId.ToString() + " and cContactType = 'Billing Address'", "tblCartContact");
                            if (oDs2.Tables["tblCartContact"].Rows.Count > 0)
                            {
                                mcCartCmd = "ChoosePaymentShippingOption";
                                mnProcessId = 3;
                            }

                            else if (myWeb.mnUserId > 0)
                            {
                                long BillingAddressID = setCurrentBillingAddress((long)myWeb.mnUserId, 0L);

                                if (BillingAddressID > 0L)
                                {
                                    // set the billing address
                                    string sSql = "Select nContactKey from tblCartContact where cContactType = 'Delivery Address' and nContactCartid=" + mnCartId;
                                    string DeliveryAddressID = moDBHelper.ExeProcessSqlScalar(sSql);
                                    useSavedAddressesOnCart(BillingAddressID, Conversions.ToLong(DeliveryAddressID),null);

                                    mcPaymentMethod = myWeb.moRequest[buttonRef];

                                    mcCartCmd = "ChoosePaymentShippingOption";
                                    mnProcessId = 3;
                                }
                                else
                                {
                                    mcCartCmd = "Billing";
                                    mnProcessId = 2;
                                }
                            }
                            else
                            {
                                mcCartCmd = "Billing";
                                mnProcessId = 2;


                            }

                        }

                        // save address against the user
                        if (mbEwMembership == true & myWeb.mnUserId > 0 & oContactXform.valid)
                        {

                            if (!(oContactXform.moXformElmt.GetAttribute("persistAddress") == "false"))
                            {
                                cProcessInfo = "UpdateExistingUserAddress for : " + myWeb.mnUserId;
                                UpdateExistingUserAddress(ref oContactXform);
                            }

                        }

                    }
                    oContactXform = (Cms.xForm)null;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "addressSubProcess", ex, "", cProcessInfo, gbDebug);
                }

            }

            public virtual bool usePreviousAddress(ref XmlElement oCartElmt)
            {
                string cProcessInfo = "usePreviousAddress";
                string sSql;
                long billingAddId = 0L;
                long deliveryAddId = 0L;
                DataSet oDs;
                try
                {
                    if (mbEwMembership == true & myWeb.mnUserId != 0)
                    {
                        if (Strings.LCase(moCartConfig["UsePreviousAddress"]) == "on")
                        {

                            sSql = "select nContactKey, cContactType, nAuditKey from tblCartContact inner join tblAudit a on nAuditId = a.nAuditKey where nContactCartId = 0 and nContactDirId =" + myWeb.mnUserId.ToString();
                            oDs = moDBHelper.GetDataSet(sSql, "tblCartContact");

                            foreach (DataRow odr in oDs.Tables["tblCartContact"].Rows)
                            {
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(odr["cContactType"], "Billing Address", false)))
                                {
                                    billingAddId = Conversions.ToLong(odr["nContactKey"]);
                                }
                                if (mbNoDeliveryAddress)
                                {
                                    deliveryAddId = billingAddId;
                                }
                                else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(odr["cContactType"], "Delivery Address", false)))
                                {
                                    deliveryAddId = Conversions.ToLong(odr["nContactKey"]);
                                }
                            }
                            if (deliveryAddId != 0L & billingAddId != 0L)
                            {
                                useSavedAddressesOnCart(billingAddId, deliveryAddId, null);
                                // skip
                                mcCartCmd = "ChoosePaymentShippingOption";
                                return true;
                            }
                            else
                            {
                                // we don't have the addresses we need so we need to go to address step anyhow
                                return false;
                            }
                        }

                        else
                        {
                            // Use previous address functionality turned off
                            return false;
                        }
                    }
                    else
                    {
                        // User not logged on or membership is off
                        return false;
                    }
                }



                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "addressSubProcess", ex, "", cProcessInfo, gbDebug);
                    return false;
                }

            }


            public virtual Cms.xForm contactXform(string cAddressType, string cSubmitName = "submit", string cCmdType = "cartCmd", string cCmdAction = "")
            {

                return contactXform(cAddressType, cSubmitName, cCmdType, cCmdAction, false);

            }


            public virtual Cms.xForm contactXform(string cAddressType, string cSubmitName, string cCmdType, string cCmdAction, bool bDontPopulate, long ContactId = 0L, string cmd2 = "")
            {
                myWeb.PerfMon.Log("Cart", "contactXform");
                xForm oXform = new xForm(ref myWeb.msException);
                XmlElement oGrpElmt;
                DataSet oDs;
                XmlElement oElmt;
                string cWhere;
                string cProcessInfo = "";
                string cXformLocation = "";
                bool bIsBespokeXform = false;
                var bGetInstance = default(bool);
                string sSql;

                try
                {

                    // Build the xform
                    oXform.moPageXML = moPageXml;
                    oXform.NewFrm(cAddressType);

                    // Check for bespoke xform
                    switch (cAddressType ?? "")
                    {
                        case "Billing Address":
                            {
                                cXformLocation = mcBillingAddressXform;
                                break;
                            }
                        case "Delivery Address":
                            {
                                cXformLocation = mcDeliveryAddressXform;
                                break;
                            }
                    }

                    // Test that a bespoke form exists and is a valid filename
                    bIsBespokeXform = File.Exists(myWeb.goServer.MapPath(cXformLocation)) & Strings.LCase(Strings.Right(cXformLocation, 4)) == ".xml";

                    // :::::::::::::::::::::::::::::::::::::::::::::::
                    // :::: CONTACT XFORM :: GROUP and BIND BUILD ::::
                    // :::::::::::::::::::::::::::::::::::::::::::::::

                    if (bIsBespokeXform)
                    {
                        // Load the bespoke form
                        oXform.load(cXformLocation);

                        // select the first group element for adding delivery checkbox
                        oGrpElmt = (XmlElement)oXform.moXformElmt.SelectSingleNode("group[1]");
                        if (ContactId > 0L)
                        {
                            bGetInstance = true;
                        }
                    }
                    else
                    {
                        // Build the xform because file not specified
                        bGetInstance = true;

                        oXform.addGroup(ref oXform.moXformElmt, "address", sLabel: cAddressType);
                        oGrpElmt = (XmlElement)oXform.moXformElmt.LastChild;

                        oXform.addInput(ref oGrpElmt, cCmdType, true, cCmdType, "hidden");
                        XmlElement argoBindParent = null;
                        oXform.addBind(cCmdType, cCmdType, oBindParent: ref argoBindParent);

                        oXform.addInput(ref oGrpElmt, "cContactType", true, "Type", "hidden");
                        XmlElement argoBindParent1 = null;
                        oXform.addBind("cContactType", "tblCartContact/cContactType", oBindParent: ref argoBindParent1);

                        oXform.addInput(ref oGrpElmt, "cContactName", true, "Name", "textbox required");
                        XmlElement argoBindParent2 = null;
                        oXform.addBind("cContactName", "tblCartContact/cContactName", oBindParent: ref argoBindParent2, "true()", "string");

                        oXform.addInput(ref oGrpElmt, "cContactCompany", true, "Company", "textbox");
                        XmlElement argoBindParent3 = null;
                        oXform.addBind("cContactCompany", "tblCartContact/cContactCompany", oBindParent: ref argoBindParent3, "false()");

                        oXform.addInput(ref oGrpElmt, "cContactAddress", true, "Address", "textbox required");
                        XmlElement argoBindParent4 = null;
                        oXform.addBind("cContactAddress", "tblCartContact/cContactAddress", oBindParent: ref argoBindParent4, "true()", "string");

                        oXform.addInput(ref oGrpElmt, "cContactCity", true, "City", "textbox required");
                        XmlElement argoBindParent5 = null;
                        oXform.addBind("cContactCity", "tblCartContact/cContactCity", oBindParent: ref argoBindParent5, "true()");

                        oXform.addInput(ref oGrpElmt, "cContactState", true, "County/State", "textbox");
                        XmlElement argoBindParent6 = null;
                        oXform.addBind("cContactState", "tblCartContact/cContactState", oBindParent: ref argoBindParent6);

                        oXform.addInput(ref oGrpElmt, "cContactZip", true, "Postcode/Zip", "textbox required");
                        XmlElement argoBindParent7 = null;
                        oXform.addBind("cContactZip", "tblCartContact/cContactZip", oBindParent: ref argoBindParent7, "true()", "string");

                        oXform.addSelect1(ref oGrpElmt, "cContactCountry", true, "Country", "dropdown required");
                        XmlElement argoBindParent8 = null;
                        oXform.addBind("cContactCountry", "tblCartContact/cContactCountry", oBindParent: ref argoBindParent8, "true()", "string");

                        oXform.addInput(ref oGrpElmt, "cContactTel", true, "Tel", "textbox");
                        XmlElement argoBindParent9 = null;
                        oXform.addBind("cContactTel", "tblCartContact/cContactTel", oBindParent: ref argoBindParent9);

                        oXform.addInput(ref oGrpElmt, "cContactFax", true, "Fax", "textbox");
                        XmlElement argoBindParent10 = null;
                        oXform.addBind("cContactFax", "tblCartContact/cContactFax", oBindParent: ref argoBindParent10);

                        // Only show email address for Billing
                        if (cAddressType == "Billing Address" | mnGiftListId > 0)
                        {
                            oXform.addInput(ref oGrpElmt, "cContactEmail", true, "Email", "textbox required");
                            XmlElement argoBindParent11 = null;
                            oXform.addBind("cContactEmail", "tblCartContact/cContactEmail", oBindParent: ref argoBindParent11, "true()", "email");
                        }
                        if (myWeb.moConfig["cssFramework"] == "bs3")
                        {
                            oXform.addSubmit(ref oGrpElmt, "Submit" + Strings.Replace(cAddressType, " ", ""), "Submit");
                        }
                        else
                        {
                            oXform.addSubmit(ref oGrpElmt, "Submit" + Strings.Replace(cAddressType, " ", ""), "Submit", cSubmitName + Strings.Replace(cAddressType, " ", ""));
                        }
                        oXform.submission("Submit" + Strings.Replace(cAddressType, " ", ""), Conversions.ToString(Interaction.IIf(string.IsNullOrEmpty(cCmdAction), "", "?" + cCmdType + "=" + cCmdAction)), "POST", "return form_check(this);");
                    }

                    // Add the countries list to the form
                    foreach (XmlElement currentOElmt in oXform.moXformElmt.SelectNodes("descendant-or-self::select1[contains(@class, 'country') or @bind='cContactCountry']"))
                    {
                        oElmt = currentOElmt;
                        string cThisAddressType;
                        if (oElmt.ParentNode.SelectSingleNode("input[@bind='cContactType' or @bind='cDelContactType']/value") is null)
                        {
                            cThisAddressType = cAddressType;
                        }
                        else
                        {
                            cThisAddressType = oElmt.ParentNode.SelectSingleNode("input[@bind='cContactType' or @bind='cDelContactType']/value").InnerText;
                        }
                        if (mbNoDeliveryAddress)
                        {
                            cThisAddressType = "Delivery Address";
                        }

                        populateCountriesDropDown(ref oXform, ref oElmt, cThisAddressType);
                    }

                    // Add the Delivery Checkbox if needed

                    if (cAddressType == "Billing Address" & mnProcessId < 2 & !mbNoDeliveryAddress & !(mnGiftListId > 0))
                    {
                        // this can be optionally turned off in the xform, by the absence of cIsDelivery in the instance.
                        if (oXform.Instance.SelectSingleNode("tblCartContact[@type='Delivery Address']") is null & oXform.Instance.SelectSingleNode("//bDisallowDeliveryCheckbox") is null)
                        {
                            // shoppers have the option to send to same address as the billing with a checkbox
                            oXform.addSelect(ref oGrpElmt, "cIsDelivery", true, "Deliver to This Address", "checkbox", Protean.xForm.ApperanceTypes.Minimal);
                            XmlElement argoSelectNode = (XmlElement)oGrpElmt.LastChild;
                            oXform.addOption(ref argoSelectNode, "", "True");
                            XmlElement argoBindParent12 = null;
                            oXform.addBind("cIsDelivery", "tblCartContact/cIsDelivery", oBindParent: ref argoBindParent12);
                        }
                    }

                    if (moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                    {
                        // Add Collection options
                        XmlElement oIsDeliverySelect = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::select[@bind='cIsDelivery']");
                        if (oIsDeliverySelect != null)
                        {

                            // Create duplicate select as select1
                            var newElmt = moPageXml.CreateElement("select1");
                            foreach (XmlAttribute oAtt in oIsDeliverySelect.Attributes)
                                newElmt.SetAttribute(oAtt.Name, oAtt.Value);
                            foreach (XmlNode oNode in oIsDeliverySelect.ChildNodes)
                                newElmt.AppendChild(oNode.CloneNode(true));

                            var delBillingElmt = oXform.addOption(ref newElmt, "Deliver to Billing Address", "false");

                            bool bCollection = false;
                            bool bOverrideCollection = false;
                            // Get the collection delivery options
                            // Dim oDrCollectionOptions As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where bCollection = 1")
                            // Add one key in config for running functionality of bCollection - OverrideCollection
                            if (moConfig["OverrideCollection"] != null)
                            {
                                if ((moConfig["OverrideCollection"]) != "" && (moConfig["OverrideCollection"]).ToLower() == "true")
                                {
                                    bOverrideCollection = true;
                                }
                            }
                            if (bOverrideCollection == false)
                            {
                                using (var oDrCollectionOptions = moDBHelper.getDataReaderDisposable("select * from tblCartShippingMethods where bCollection = 1"))  // Done by nita on 6/7/22
                                {
                                    while (oDrCollectionOptions.Read())
                                    {
                                        string OptLabel = "<span class=\"opt-name\">" + oDrCollectionOptions["cShipOptName"].ToString() + "</span>";
                                        OptLabel = OptLabel + "<span class=\"opt-carrier\">" + oDrCollectionOptions["cShipOptCarrier"].ToString() + "</span>";
                                        oXform.addOption(ref newElmt, OptLabel, oDrCollectionOptions["nShipOptKey"].ToString(), true);
                                        bCollection = true;
                                    }
                                    // Only change this if collection shipping options exist.
                                    if (bCollection)
                                    {
                                        oIsDeliverySelect.ParentNode.ReplaceChild(newElmt, oIsDeliverySelect);
                                    }
                                    else
                                    {
                                        // this was all for nuffin
                                        newElmt = null;
                                    }
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(cmd2))
                    {
                        oXform.addInput(ref oGrpElmt, cmd2, true, cmd2, "hidden");
                        XmlElement argoBindParent13 = null;
                        oXform.addBind(cmd2, "cmd2", oBindParent: ref argoBindParent13);
                    }

                    // :::::::::::::::::::::::::::::::::::::::::::::::
                    // :::: CONTACT XFORM :: CREATE/LOAD INSTANCE ::::
                    // :::::::::::::::::::::::::::::::::::::::::::::::

                    // When there is no match this will get the default instance based on the table schema.  
                    // This will override any form that we have loaded in, so we need to put exceptions in.
                    if (bGetInstance)
                    {
                        oXform.Instance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, ContactId);
                        if (ContactId > 0L)
                        {
                            bDontPopulate = false;
                        }
                    }

                    // if the instance is empty fill these values
                    bool bAddIds = false;

                    // catch for sites where nContactKey is not specified.
                    if (oXform.Instance.SelectSingleNode("*/nContactKey") is null)
                    {
                        bAddIds = true;
                    }
                    else if (string.IsNullOrEmpty(oXform.Instance.SelectSingleNode("*/nContactKey").InnerText))
                    {
                        bAddIds = true;
                    }

                    if (bAddIds)
                    {
                        foreach (XmlElement currentOElmt1 in oXform.Instance.SelectNodes("*/nContactDirId"))
                        {
                            oElmt = currentOElmt1;
                            oElmt.InnerText = myWeb.mnUserId.ToString();
                        }
                        foreach (XmlElement currentOElmt2 in oXform.Instance.SelectNodes("*/nContactCartId"))
                        {
                            oElmt = currentOElmt2;
                            oElmt.InnerText = mnCartId.ToString();
                        }
                        foreach (XmlElement currentOElmt3 in oXform.Instance.SelectNodes("*/cContactType"))
                        {
                            oElmt = currentOElmt3;
                            if (string.IsNullOrEmpty(oElmt.InnerText))
                                oElmt.InnerText = cAddressType;
                        }
                    }

                    // make sure we don't show a random address.
                    if (mnCartId == 0)
                        bDontPopulate = true;

                    if (bDontPopulate == false)
                    {
                        // if we have addresses in the cart insert them
                        sSql = "select nContactKey, cContactType from tblCartContact where nContactCartId = " + mnCartId.ToString();
                        oDs = moDBHelper.GetDataSet(sSql, "tblCartContact");
                        foreach (DataRow oDr in oDs.Tables["tblCartContact"].Rows)
                        {
                            var tempInstance = moPageXml.CreateElement("TempInstance");
                            tempInstance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, Conversions.ToLong(oDr["nContactKey"]));
                            XmlElement instanceAdd = (XmlElement)oXform.Instance.SelectSingleNode(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("tblCartContact[cContactType/node()='", oDr["cContactType"]), "']")));
                            if (instanceAdd != null)
                            {
                                instanceAdd.ParentNode.ReplaceChild(tempInstance.FirstChild, instanceAdd);
                                instanceAdd = (XmlElement)oXform.Instance.SelectSingleNode(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("tblCartContact[cContactType/node()='", oDr["cContactType"]), "']")));
                                if (instanceAdd != null)
                                {
                                    instanceAdd.SetAttribute("type", Conversions.ToString(oDr["cContactType"]));
                                }
                            }
                        }
                        oDs = null;
                        // set the isDelivery Value
                        // remember the delivery address setting.
                        XmlElement delivElmt = (XmlElement)oXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='Delivery Address']");
                        if (delivElmt != null)
                        {
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["isDelivery"], "", false)))
                            {
                                delivElmt.SetAttribute("isDelivery", "false");
                            }
                            else
                            {
                                delivElmt.SetAttribute("isDelivery", Conversions.ToString(myWeb.moSession["isDelivery"]));
                            }
                        }

                    }

                    // Dim bGetInstance As Boolean = True
                    // If bIsBespokeXform Then
                    // ' There is a bespoke form, let's check for the presence of an item in the contact table
                    // Dim oCartContactInDB As Object = moDBHelper.GetDataValue("SELECT COUNT(*) As FoundCount FROM tblCartContact " & ssql)
                    // If Not (oCartContactInDB > 0) Then bGetInstance = False ' No Item so do not get the instance
                    // End If



                    // If membership is on and the user is logged on, we need to check if this is an existing address in the user's list of addresses
                    if (mbEwMembership == true & myWeb.mnUserId > 0)
                    {

                        // If we are using the Pick Address list to EDIT an address, there will be a hidden control of userAddId
                        if (!string.IsNullOrEmpty(myWeb.moRequest["userAddId"]))
                        {
                            oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressId"));
                            oXform.Instance.LastChild.InnerText = myWeb.moRequest["userAddId"];
                            oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressType"));
                            oXform.Instance.LastChild.InnerText = myWeb.moRequest["userAddType"];
                        }
                        else
                        {
                            // Holy large where statement, Batman!  But how on earth else do we tell is a cart address is the same as a user address?
                            string value = string.Empty;
                            XmlElement contact = (XmlElement)oXform.Instance.SelectSingleNode("tblCartContact");
                            cWhere = "";
                            if (Tools.Xml.NodeState(ref contact, "cContactName", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere = Conversions.ToString(cWhere + Operators.ConcatenateObject(Operators.ConcatenateObject("  and cContactName='", SqlFmt(value)), "' "));
                            if (Tools.Xml.NodeState(ref contact, "cContactCompany", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere = Conversions.ToString(cWhere + Operators.ConcatenateObject(Operators.ConcatenateObject("  and cContactCompany='", SqlFmt(value)), "' "));
                            if (Tools.Xml.NodeState(ref contact, "cContactAddress", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere = Conversions.ToString(cWhere + Operators.ConcatenateObject(Operators.ConcatenateObject("  and cContactAddress='", SqlFmt(value)), "' "));
                            if (Tools.Xml.NodeState(ref contact, "cContactCity", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere = Conversions.ToString(cWhere + Operators.ConcatenateObject(Operators.ConcatenateObject("  and cContactCity='", SqlFmt(value)), "' "));
                            if (Tools.Xml.NodeState(ref contact, "cContactState", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere = Conversions.ToString(cWhere + Operators.ConcatenateObject(Operators.ConcatenateObject("  and cContactState='", SqlFmt(value)), "' "));
                            if (Tools.Xml.NodeState(ref contact, "cContactZip", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere = Conversions.ToString(cWhere + Operators.ConcatenateObject(Operators.ConcatenateObject("  and cContactZip='", SqlFmt(value)), "' "));
                            if (Tools.Xml.NodeState(ref contact, "cContactCountry", "", "", XmlNodeState.IsEmpty, null, "", value, bCheckTrimmedInnerText: false) != XmlNodeState.NotInstantiated)
                                cWhere = Conversions.ToString(cWhere + Operators.ConcatenateObject(Operators.ConcatenateObject("  and cContactCountry='", SqlFmt(value)), "' "));

                            // cWhere = " and cContactName='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactName").InnerText) & "' " & _
                            // "and cContactCompany='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactCompany").InnerText) & "' " & _
                            // "and cContactAddress='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactAddress").InnerText) & "' " & _
                            // "and cContactCity='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactCity").InnerText) & "' " & _
                            // "and cContactState='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactState").InnerText) & "' " & _
                            // "and cContactZip='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactZip").InnerText) & "' " & _
                            // "and cContactCountry='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText) & "' "
                            oDs = moDBHelper.GetDataSet("select * from tblCartContact where nContactDirId = " + myWeb.mnUserId.ToString() + cWhere, "tblCartContact");
                            if (oDs.Tables["tblCartContact"].Rows.Count > 0)
                            {
                                oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressId"));
                                oXform.Instance.LastChild.InnerText = Conversions.ToString(oDs.Tables["tblCartContact"].Rows[0]["nContactKey"]);
                                oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressType"));
                                oXform.Instance.LastChild.InnerText = Conversions.ToString(oDs.Tables["tblCartContact"].Rows[0]["cContactType"]);
                            }
                        }
                    }


                    // add some proceedual fields to instance
                    oElmt = oXform.moPageXML.CreateElement(cCmdType);
                    oXform.Instance.AppendChild(oElmt);

                    oElmt = oXform.moPageXML.CreateElement("cIsDelivery");
                    oXform.Instance.AppendChild(oElmt);

                    if (!string.IsNullOrEmpty(cmd2))
                    {
                        oElmt = oXform.moPageXML.CreateElement("cmd2");
                        oElmt.InnerText = "True";
                        oXform.Instance.AppendChild(oElmt);
                    }


                    // If oXform.isSubmitted And cAddressType = myWeb.moRequest.Form("cContactType") Then

                    if (oXform.isSubmitted())
                    {
                        oXform.updateInstanceFromRequest();
                        oXform.validate();

                        myWeb.moSession["isDelivery"] = myWeb.moRequest["isDelivery"];

                        // Catch for space as country
                        if (myWeb.moRequest["cContactCountry"] != default)
                        {
                            if (string.IsNullOrEmpty(myWeb.moRequest["cContactCountry"].Trim()))
                            {
                                oXform.valid = false;
                                oXform.addNote("cContactCountry", Protean.xForm.noteTypes.Alert, "Please select a country", true);
                            }
                        }

                        if (oXform.valid)
                        {
                            // the form is valid so update it - add a check for timed out session (mnCartId = 0)
                            if (ContactId > 0L)
                            {
                                // ID is specified so we simply update ignore relation to the cart
                                moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oXform.Instance, ContactId);
                            }
                            else if (mnCartId > 0)
                            {
                                // test if we have a address of this type against the order..!
                                // ssql = "Select nContactKey from tblCartContact where cContactType = '" & cAddressType & "' and nContactCartid=" & mnCartId
                                // Dim sContactKey1 As String = moDBHelper.ExeProcessSqlScalar(ssql)
                                // moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oXform.Instance, sContactKey1)

                                // Step through for multiple addresses
                                bool bSavedDelivery = false;



                                // check for collection options
                                if (Information.IsNumeric(myWeb.moRequest["cIsDelivery"]))
                                {
                                    // Save the delivery method allready
                                    string cSqlUpdate = "";
                                    // Dim oDrCollectionOptions2 As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where nShipOptKey = " & myWeb.moRequest("cIsDelivery"))
                                    using (var oDrCollectionOptions2 = moDBHelper.getDataReaderDisposable("select * from tblCartShippingMethods where nShipOptKey = " + myWeb.moRequest["cIsDelivery"]))  // Done by nita on 6/7/22
                                    {
                                        while (oDrCollectionOptions2.Read())
                                        {
                                            string cShippingDesc = oDrCollectionOptions2["cShipOptName"].ToString() + "-" + oDrCollectionOptions2["cShipOptCarrier"].ToString() + "</span>";
                                            double nShippingCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oDrCollectionOptions2["nShipOptCost"]));

                                            cSqlUpdate = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE tblCartOrder SET cShippingDesc='", SqlFmt(cShippingDesc)), "', nShippingCost="), SqlFmt(nShippingCost.ToString())), ", nShippingMethodId = "), myWeb.moRequest["cIsDelivery"]), " WHERE nCartOrderKey="), mnCartId));
                                        }

                                        moDBHelper.ExeProcessSql(cSqlUpdate);
                                        bSavedDelivery = true;
                                    }
                                }
                                // If it exists and we are here means we may have changed the Delivery address country

                                else if (Strings.LCase(moCartConfig["BlockRemoveDelivery"]) != "on")
                                {
                                    RemoveDeliveryOption(mnCartId);

                                }

                                if (moDBHelper.checkTableColumnExists("tblCartOrder", "nReceiptType"))
                                {
                                    if (myWeb.moRequest["cIsDelivery"] == "true" & myWeb.moRequest["cIsPaperRecieptForDelAddress"] == "true")
                                    {
                                        // check flag condition
                                        string cSqlUpdate = "UPDATE tblCartOrder SET nReceiptType=2 WHERE nCartOrderKey=" + mnCartId;
                                        moDBHelper.ExeProcessSql(cSqlUpdate);
                                    }

                                    else
                                    {
                                        // check flag condition
                                        string cSqlUpdate = "UPDATE tblCartOrder SET nReceiptType=1 WHERE nCartOrderKey=" + mnCartId;
                                        moDBHelper.ExeProcessSql(cSqlUpdate);
                                    }
                                }
                                foreach (XmlElement currentOElmt4 in oXform.Instance.SelectNodes("tblCartContact"))
                                {
                                    oElmt = currentOElmt4;
                                    string cThisAddressType = oElmt.SelectSingleNode("cContactType").InnerText;
                                    sSql = "Select nContactKey from tblCartContact where cContactType = '" + cThisAddressType + "' and nContactCartid=" + mnCartId;
                                    string sContactKey1 = moDBHelper.ExeProcessSqlScalar(sSql);
                                    var saveInstance = moPageXml.CreateElement("instance");
                                    saveInstance.AppendChild(oElmt.Clone());
                                    moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, saveInstance, Conversions.ToLong(sContactKey1));
                                    if (cThisAddressType == "Delivery Address")
                                        bSavedDelivery = true;
                                }

                                // if the option save Delivery is true then
                                if (bSavedDelivery == false)
                                {
                                    if (myWeb.moRequest["cIsDelivery"] == "True" | mbNoDeliveryAddress & cAddressType == "Billing Address")
                                    {
                                        if (myWeb.moRequest["cIsDelivery"] == "True" & mnShippingRootId > 0)
                                        {
                                            // mnShippingRootId
                                            // check if the submitted country matches one in the delivery list
                                            var oCheckElmt = moPageXml.CreateElement("ValidCountries");
                                            ListShippingLocations(ref oCheckElmt);
                                            string cCountry = oXform.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText;
                                            if (oCheckElmt.SelectSingleNode("descendant-or-self::TreeItem[@Name='" + cCountry + "' or @name='" + cCountry + "' or @nameShort='" + cCountry + "']") is null)
                                            {
                                                oXform.valid = false;
                                                oXform.addNote("cContactCountry", Protean.xForm.noteTypes.Alert, "Cannot Deliver to this country. please select another.", true);
                                            }
                                        }
                                        if (oXform.valid)
                                        {
                                            sSql = "Select nContactKey from tblCartContact where cContactType = 'Delivery Address' and nContactCartid=" + mnCartId;
                                            string sContactKey2 = moDBHelper.ExeProcessSqlScalar(sSql);
                                            oXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText = "Delivery Address";
                                            moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oXform.Instance, Conversions.ToLong(sContactKey2));
                                            // going to set it back to a billing address
                                            oXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText = "Billing Address";
                                        }
                                    }
                                }

                                if (oXform.Instance.SelectSingleNode("tblCartContact/cContactEmail[@optOut='true']") != null)
                                {
                                    moDBHelper.AddInvalidEmail(oXform.Instance.SelectSingleNode("tblCartContact/cContactEmail[@optOut='true']").InnerText);
                                }
                            }

                            else
                            {
                                // Throw an error to indicate that the user has timed out
                                mnProcessError = 4;
                            }


                            if (!string.IsNullOrEmpty(myWeb.moRequest["cContactOpt-In"]))
                            {
                                this.AddToLists("Newsletter", ref moCartXml, myWeb.moRequest["cContactName"], myWeb.moRequest["cContactEmail"]);
                            }

                        }

                    }



                    oXform.addValues();

                    return oXform;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "contactXform", ex, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
                }

            }




            public virtual Cms.xForm pickContactXform(string cAddressType, string submitPrefix = "", string cCmdType = "cartCmd", string cCmdAction = "")
            {
                myWeb.PerfMon.Log("Cart", "pickContactXform");
                var oXform = new Cms.xForm(ref myWeb.msException);

                Cms.xForm oReturnForm;
                XmlElement oGrpElmt;
                DataSet oDs;
                DataSet oDs2;
                DataRow oDr;
                string cSql = "";
                // Dim sAddressHtml As String
                string cProcessInfo = "";
                long contactId = 0L;
                long billingAddId = 0L;
                Cms.xForm oContactXform = (Cms.xForm)null;
                bool bDontPopulate = false;
                bool bBillingSet = false;
                string newSubmitPrefix = submitPrefix;
                string newAddressType = cAddressType;
                string contactFormCmd2 = "";
                try
                {
                    myWeb.moSession["tempInstance"] = null;
                    // Get any existing addresses for user
                    // Changed this so it gets any

                    // Check if updated primiary billing address, (TS added order by reverse order added)
                    cSql = "select * from tblCartContact where nContactDirId = " + myWeb.mnUserId.ToString() + " and nContactCartId = 0 and (cContactType = 'Billing Address' or cContactType = 'Delivery Address')  order by cContactType ASC, nContactKey DESC";
                    oDs = moDBHelper.GetDataSet(cSql, "tblCartContact");
                    foreach (DataRow currentODr in oDs.Tables["tblCartContact"].Rows)
                    {
                        oDr = currentODr;
                        if (billingAddId == 0L)
                            billingAddId = Conversions.ToLong(oDr["nContactKey"]);

                        if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject("cartDeleditAddress", oDr["nContactKey"]))]))
                        {
                            submitPrefix = "cartDel";
                            cAddressType = "Delivery Address";

                            newSubmitPrefix = "cartDel";
                            newAddressType = "Delivery Address";

                            // ensure we hit this next time through...
                            cCmdAction = "Delivery";
                            contactFormCmd2 = Conversions.ToString(Operators.ConcatenateObject("cartDeleditAddress", oDr["nContactKey"]));
                        }


                        else if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "addDelivery", oDr["nContactKey"]))]))
                        {
                            bDontPopulate = true;
                            newSubmitPrefix = "cartDel";
                            newAddressType = "Delivery Address";
                            cCmdAction = "Delivery";
                        }
                        else if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "editAddress", oDr["nContactKey"]))]))
                        {
                            if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(billingAddId, oDr["nContactKey"], false)))
                            {
                                newSubmitPrefix = "cartDel";
                                newAddressType = "Delivery Address";
                            }
                            else
                            {
                                // we are editing a billing address and want to ensure we dont get a double form.
                                if (mcBillingAddressXform.Contains("BillingAndDeliveryAddress.xml"))
                                {
                                    mcBillingAddressXform = mcBillingAddressXform.Replace("BillingAndDeliveryAddress.xml", "BillingAddress.xml");
                                }
                                // ensure we hit this next time through...
                                cCmdAction = "Billing";
                                contactFormCmd2 = Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "editAddress", oDr["nContactKey"]));
                                bDontPopulate = true;
                                // we specifiy a contactID to ensure we don't update the cart addresses just the ones on file.
                                contactId = Conversions.ToLong(oDr["nContactKey"]);
                            }
                        }
                        else if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "useBilling", oDr["nContactKey"]))]))
                        {
                            contactId = setCurrentBillingAddress((long)myWeb.mnUserId, Conversions.ToLong(oDr["nContactKey"]));
                            bBillingSet = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addNewAddress"]))
                    {
                        contactId = 0L;
                        bDontPopulate = true;
                    }

                    oContactXform = contactXform(newAddressType, newSubmitPrefix + "Address", cCmdType, cCmdAction, bDontPopulate, contactId, contactFormCmd2);

                    // Build the xform
                    oXform.moPageXML = moPageXml;
                    string cPickAddressXform = moCartConfig["PickAddressXForm"];

                    if (!string.IsNullOrEmpty(cPickAddressXform))
                    {
                        if (!oXform.load(cPickAddressXform))
                        {
                            oXform.NewFrm(cAddressType);
                        }
                    }
                    else {
                        oXform.NewFrm(cAddressType);
                    }
                    oXform.valid = false;

                    // oReturnForm is going to be the form returned at the end of the function.
                    oReturnForm = oXform;

                    if (!bBillingSet)
                    {
                        contactId = setCurrentBillingAddress((long)myWeb.mnUserId, 0L);
                    }
                    else
                    {
                        cSql = "Select * from tblCartContact where nContactDirId = " + myWeb.mnUserId.ToString() + " And nContactCartId = 0 And (cContactType='Billing Address' or cContactType='Delivery Address') order by cContactType ASC";
                        oDs = moDBHelper.GetDataSet(cSql, "tblCartContact");
                    }

                    if (oDs.Tables["tblCartContact"].Rows.Count > 0)
                    {

                        // Create the instance
                        oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("cContactId"));

                        // Add a value if an address has been selected
                        oDs2 = moDBHelper.GetDataSet("select * from tblCartContact where nContactCartId = " + mnCartId.ToString() + " and cContactType = '" + cAddressType + "'", "tblCartContact");
                        if (oDs2.Tables["tblCartContact"].Rows.Count > 0)
                        {
                            oXform.Instance.SelectSingleNode("cContactId").InnerText = Conversions.ToString(oDs2.Tables["tblCartContact"].Rows[0]["nContactKey"]);
                        }

                        oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("cIsDelivery"));
                        oXform.submission("contact", mcPagePath + cCmdType + "=" + cCmdAction, "POST");

                        oGrpElmt = oXform.addGroup(ref oXform.moXformElmt, "address", sLabel: "");

                        oXform.addInput(ref oGrpElmt, "addType", false, "", "hidden");
                        oGrpElmt.LastChild.AppendChild(oGrpElmt.OwnerDocument.CreateElement("value"));
                        oGrpElmt.LastChild.FirstChild.InnerText = cAddressType;

                        // oXform.addSelect1(oGrpElmt, "cContactId", True, "Select", "multiline", xForm.ApperanceTypes.Full)
                        // oXform.addBind("cContactId", "cContactId")

                        // Add Collection Options

                        if (moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                        {
                            // Add Collection options
                            // Get the collection delivery options
                            // Dim oDrCollectionOptions As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where bCollection = 1")
                            using (var oDrCollectionOptions = moDBHelper.getDataReaderDisposable("select * from tblCartShippingMethods where bCollection = 1"))  // Done by nita on 6/7/22
                            {
                                if (oDrCollectionOptions.HasRows)
                                {
                                    XmlElement oCollectionGrp;
                                    oCollectionGrp = oXform.addGroup(ref oGrpElmt, "CollectionOptions", "collection-options", "");

                                    while (oDrCollectionOptions.Read())
                                    {

                                        string OptLabel = oDrCollectionOptions["cShipOptName"].ToString() + " - " + oDrCollectionOptions["cShipOptCarrier"].ToString();

                                        oXform.addSubmit(ref oCollectionGrp, "collect", OptLabel, "CollectionID_" + oDrCollectionOptions["nShipOptKey"].ToString(), "collect btn-success principle", "fa-truck");
                                    }
                                }
                            }
                        }

                        foreach (DataRow currentODr1 in oDs.Tables["tblCartContact"].Rows)
                        {
                            oDr = currentODr1;
                            XmlElement oAddressGrp;
                            oAddressGrp = oXform.addGroup(ref oGrpElmt, Conversions.ToString(Operators.ConcatenateObject("addressGrp-", oDr["nContactKey"])), "addressGrp", "");
                            oXform.addDiv(ref oAddressGrp, moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, Conversions.ToLong(oDr["nContactKey"])), "pickAddress");

                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(billingAddId, oDr["nContactKey"], false)))
                            {
                                oXform.addSubmit(ref oAddressGrp, "editAddress", "Edit", Conversions.ToString(Operators.ConcatenateObject("cartDeleditAddress", oDr["nContactKey"])), "btn-default edit", "fa-pencil");
                                oXform.addSubmit(ref oAddressGrp, "removeAddress", "Del", Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "deleteAddress", oDr["nContactKey"])), "btn-default delete", "fa-trash-o");
                            }
                            else
                            {
                                oXform.addSubmit(ref oAddressGrp, "editAddress", "Edit", Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "editAddress", oDr["nContactKey"])), "btn-default edit", "fa-pencil");
                                // oXform.addSubmit(oAddressGrp, "removeAddress", "Delete", submitPrefix & "deleteAddress" & oDr.Item("nContactKey"), "delete")
                            }

                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDr["cContactType"], "Billing Address", false)))
                            {
                                oXform.addSubmit(ref oAddressGrp, Conversions.ToString(oDr["nContactKey"]), "Use as Billing", Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "useBilling", oDr["nContactKey"])), "setAsBilling");
                            }
                            else
                            {

                                if (mbNoDeliveryAddress)
                                {
                                    oXform.addSubmit(ref oAddressGrp, "addNewAddress", "Add New Address", submitPrefix + "addNewAddress", "btn-default addnew", "fa-plus");
                                }
                                else
                                {
                                    oXform.addSubmit(ref oAddressGrp, "addNewAddress", "Add New Billing Address", submitPrefix + "addNewAddress", "btn-default addnew", "fa-plus");
                                }

                                if (mbNoDeliveryAddress == false)
                                {
                                    oXform.addSubmit(ref oGrpElmt, Conversions.ToString(oDr["nContactKey"]), "New Delivery Address", Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "addDelivery", oDr["nContactKey"])), "setAsBilling btn-success principle", "fa-plus");
                                }

                            }

                            if (mbNoDeliveryAddress)
                            {
                                oXform.addSubmit(ref oAddressGrp, Conversions.ToString(oDr["nContactKey"]), "Use This Address", Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "contact", oDr["nContactKey"])), "deliver-here principle", "fas fa-truck");
                            }
                            else
                            {
                                oXform.addSubmit(ref oAddressGrp, Conversions.ToString(oDr["nContactKey"]), "Deliver To This Address", Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "contact", oDr["nContactKey"])), "deliver-here principle", "fas fa-truck");
                            }
                        }
                        // Check if the form has been submitted
                        if (oXform.isSubmitted())
                        {
                            oXform.updateInstanceFromRequest();
                            // bool forCollection = false;
                            if (moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                            {
                                object bCollectionSelected = false;
                                // Dim oDrCollectionOptions As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where bCollection = 1")
                                using (var oDrCollectionOptions = moDBHelper.getDataReaderDisposable("select * from tblCartShippingMethods where bCollection = 1"))  // Done by nita on 6/7/22
                                {
                                    if (oDrCollectionOptions.HasRows)
                                    {
                                        while (oDrCollectionOptions.Read())
                                        {
                                            if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject("CollectionID_", oDrCollectionOptions["nShipOptKey"]))]))
                                            {
                                                bCollectionSelected = true;
                                                // Set the shipping option
                                                string cShippingDesc = oDrCollectionOptions["cShipOptName"].ToString() + "-" + oDrCollectionOptions["cShipOptCarrier"].ToString();
                                                double nShippingCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oDrCollectionOptions["nShipOptCost"]));
                                                string cSqlUpdate;
                                                cSqlUpdate = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE tblCartOrder SET cShippingDesc='", SqlFmt(cShippingDesc)), "', nShippingCost="), SqlFmt(nShippingCost.ToString())), ", nShippingMethodId = "), oDrCollectionOptions["nShipOptKey"]), " WHERE nCartOrderKey="), mnCartId));
                                                moDBHelper.ExeProcessSql(cSqlUpdate);
                                                // forCollection = true;
                                                oXform.valid = true;
                                                oContactXform.valid = true;
                                                mbNoDeliveryAddress = true;

                                                var NewInstance = moPageXml.CreateElement("instance");
                                                var delXform = contactXform("Delivery Address");

                                                NewInstance.InnerXml = delXform.Instance.SelectSingleNode("tblCartContact").OuterXml;
                                                // dissassciate from user so not shown again
                                                NewInstance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = "";
                                                NewInstance.SelectSingleNode("tblCartContact/cContactName").InnerText = oDrCollectionOptions["cShipOptName"].ToString();
                                                NewInstance.SelectSingleNode("tblCartContact/cContactCompany").InnerText = oDrCollectionOptions["cShipOptCarrier"].ToString();
                                                NewInstance.SelectSingleNode("tblCartContact/cContactCountry").InnerText = moCartConfig["DefaultDeliveryCountry"];

                                                string billingContactXml = null;
                                                string collectionContactID;

                                                collectionContactID = moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, NewInstance);

                                                useSavedAddressesOnCart(billingAddId, Conversions.ToInteger(collectionContactID), billingContactXml);
                                                return oReturnForm;
                                            }
                                        }
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(bCollectionSelected, false, false)))
                                        {
                                            RemoveDeliveryOption(mnCartId);
                                        }
                                    }
                                }
                            }

                            foreach (DataRow currentODr2 in oDs.Tables["tblCartContact"].Rows)
                            {
                                oDr = currentODr2;
                                if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "contact", oDr["nContactKey"]))]))
                                {
                                    contactId = Conversions.ToLong(oDr["nContactKey"]);
                                    // Save Behaviour
                                    oXform.valid = true;
                                }
                                else if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "addDelivery", oDr["nContactKey"]))]))
                                {
                                    contactId = Conversions.ToLong(oDr["nContactKey"]);
                                    oXform.valid = false;
                                    oContactXform.valid = false;
                                }
                                else if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "editAddress", oDr["nContactKey"]))]))
                                {
                                    contactId = Conversions.ToLong(oDr["nContactKey"]);
                                }
                                // edit Behavior
                                else if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "deleteAddress", oDr["nContactKey"]))]))
                                {
                                    contactId = Conversions.ToLong(oDr["nContactKey"]);
                                    // delete Behavior
                                    moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, contactId);
                                    // remove from form
                                    oXform.moXformElmt.SelectSingleNode("descendant-or-self::group[@ref='addressGrp-" + contactId + "']").ParentNode.RemoveChild(oXform.moXformElmt.SelectSingleNode("descendant-or-self::group[@ref='addressGrp-" + contactId + "']"));
                                    oXform.valid = false;
                                }
                                else if (!string.IsNullOrEmpty(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject(submitPrefix + "useBilling", oDr["nContactKey"]))]))
                                {
                                    // we have handled this at the top
                                    oXform.valid = false;
                                }
                            }

                            // Check if the contactID is populated
                            if (contactId == 0L)
                            {
                                oXform.addNote("address", Protean.xForm.noteTypes.Alert, "You must select an address from the list");
                            }
                            else
                            {

                                // Get the selected address
                                DataRow[] oMatches = oDs.Tables["tblCartContact"].Select("nContactKey = " + contactId);
                                if (oMatches != null)
                                {
                                    var oMR = oMatches[0];

                                    if (!bDontPopulate)
                                    {

                                        // Update the contactXform with the address
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText = newAddressType;
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactName").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactName"], ""));
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactCompany").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactCompany"], ""));
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactAddress").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactAddress"], ""));
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactCity").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactCity"], ""));
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactState").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactState"], ""));
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactZip").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactZip"], ""));
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactCountry"], ""));
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactTel").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactTel"], ""));
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactFax").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactFax"], ""));
                                        oContactXform.Instance.SelectSingleNode("tblCartContact/cContactEmail").InnerText = Conversions.ToString(Operators.ConcatenateObject(oMR["cContactEmail"], ""));
                                        oContactXform.Instance.SelectSingleNode("cIsDelivery").InnerText = oXform.Instance.SelectSingleNode("cIsDelivery").InnerText;

                                        oContactXform.resetXFormUI();
                                        oContactXform.addValues();

                                        // Add hidden values for the parent address
                                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + contactId]))
                                        {

                                            oGrpElmt = (XmlElement)oContactXform.moXformElmt.SelectSingleNode("group");
                                            oContactXform.addInput(ref oGrpElmt, "userAddId", false, "", "hidden");
                                            oGrpElmt.LastChild.AppendChild(oGrpElmt.OwnerDocument.CreateElement("value"));
                                            oGrpElmt.LastChild.FirstChild.InnerText = contactId.ToString(); // oMR.Item("nContactKey")

                                            oContactXform.addInput(ref oGrpElmt, "userAddType", false, "", "hidden");
                                            oGrpElmt.LastChild.AppendChild(oGrpElmt.OwnerDocument.CreateElement("value"));
                                            oGrpElmt.LastChild.FirstChild.InnerText = newAddressType; // oMR.Item("cContactType")

                                        }
                                    }
                                }
                            }
                        }

                        oXform.addValues();

                    }

                    if (oContactXform.valid == false & oXform.valid == false)
                    {
                        // both forms are invalid so we need to output one of the forms.
                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + contactId]) | !string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addDelivery" + contactId]) | oContactXform.isSubmitted())
                        {
                            // we are editing an address so show the contactXform or a contactXform has been submitted to 
                            oReturnForm = oContactXform;
                        }
                        // We need to show the pick list if and only if :
                        // 1. It has addresses in it
                        // 2. There is no request to Add

                        else if (oXform.moXformElmt.SelectSingleNode("/model/instance").HasChildNodes & !!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addNewAddress"]))
                        {
                            oReturnForm = oXform;
                        }
                        else
                        {
                            // Add address needs to clear out the existing xForm
                            if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addNewAddress"]))
                            {
                                oContactXform.resetXFormUI();
                                oContactXform.addValues();
                            }
                            oReturnForm = oContactXform;
                        }
                    }
                    else
                    {
                        // If pick address has been submitted, then we have a contactXform that has not been submitted, and therefore not saved.  Let's save it.
                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "contact" + contactId]))
                        {
                            string billingContactXml = null;
                            if (!string.IsNullOrEmpty(cPickAddressXform)){
                                billingContactXml = oXform.Instance.SelectSingleNode("tblCartContact/cContactXml").InnerXml;
                            }

                             useSavedAddressesOnCart(billingAddId, contactId, billingContactXml);
                            // skip delivery
                            oContactXform.moXformElmt.SetAttribute("cartCmd", "ChoosePaymentShippingOption");
                        }

                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "addDelivery" + contactId]))
                        {
                            // remove the deliver from the instance if it is there
                            useSavedAddressesOnCart(billingAddId, 0L, null);
                            foreach (XmlNode oNode in oContactXform.Instance.SelectNodes("tblCartContact[cContactType='Delivery Address']"))
                                oNode.ParentNode.RemoveChild(oNode);
                        }

                        if (oContactXform.valid == false)
                        {
                            oContactXform.valid = true;
                            oContactXform.moXformElmt.SetAttribute("persistAddress", "false");
                        }

                        if (!string.IsNullOrEmpty(myWeb.moRequest[submitPrefix + "editAddress" + billingAddId]) & oContactXform.valid)
                        {
                            // We have edited a billing address and need to output the pickForm
                            oReturnForm = oXform;
                        }
                        else
                        {
                            // pass through the xform to make transparent
                            oReturnForm = oContactXform;
                        }




                    }

                    // TS not sure if required after rewrite, think it is deleting addresses unessesarily.

                    // If Not (oReturnForm Is Nothing) AndAlso oReturnForm.valid AndAlso oReturnForm.isSubmitted Then
                    // ' There seems to be an issue with duplicate addresses being submitted by type against an order.
                    // '  This script finds the duplciates and nullifies them (i.e. sets their cartid to be 0).
                    // cSql = "UPDATE tblCartContact SET nContactCartId = 0 " _
                    // & "FROM (SELECT nContactCartId id, cContactType type, MAX(nContactKey) As latest FROM dbo.tblCartContact WHERE nContactCartId <> 0 GROUP BY nContactCartId, cContactType HAVING COUNT(*) >1) dup " _
                    // & "INNER JOIN tblCartContact c ON c.nContactCartId = dup.id AND c.cContactType = dup.type AND c.nContactKey <> dup.latest"
                    // cProcessInfo = "Clear Duplicate Addresses: " & cSql
                    // moDBHelper.ExeProcessSql(cSql)
                    // End If

                    return oReturnForm;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "pickContactXform", ex, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
                }

            }


            /// <summary>
            /// Each user need only have a single active billing address
            /// </summary>
            /// <param name="UserId"></param>
            /// <param name="ContactId"></param>
            /// <returns>If Contact ID = 0 then uses last updated</returns>
            /// <remarks></remarks>
            public long setCurrentBillingAddress(long UserId, long ContactId)
            {
                string cProcessInfo = "";
                string cSql;
                DataSet oDS;

                try
                {
                    if (myWeb.mnUserId > 0)
                    {

                        if (ContactId != 0L)
                        {
                            moDBHelper.updateInstanceField(Cms.dbHelper.objectTypes.CartContact, (int)ContactId, "cContactType", "Billing Address");
                        }

                        // Check for othersss
                        cSql = "select c.* from tblCartContact c inner JOIN tblAudit a on a.nAuditKey = c.nAuditId where nContactDirId = " + myWeb.mnUserId.ToString() + " and nContactCartId = 0  and cContactType='Billing Address' order by a.dUpdateDate DESC";
                        oDS = moDBHelper.GetDataSet(cSql, "tblCartContact");

                        foreach (DataRow oDr in oDS.Tables["tblCartContact"].Rows)
                        {
                            if (ContactId == Conversions.ToDouble("0"))
                            {
                                // gets the top one
                                ContactId = Conversions.ToLong(oDr["nContactKey"]);
                            }
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDr["nContactKey"], ContactId, false)))
                            {
                                moDBHelper.ExeProcessSql(Conversions.ToString(Operators.ConcatenateObject("update tblCartContact set cContactType='Previous Billing Address' where nContactKey=", oDr["nContactKey"])));
                            }
                        }

                        return ContactId;
                    }

                    else
                    {
                        return 0L;
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "setCurrentBillingAddress", ex, "", cProcessInfo, gbDebug);
                    return default;
                }
            }

            public void useSavedAddressesOnCart(long billingId, long deliveryId, string billingContactXml)
            {
                string cProcessInfo = "";
                DataSet oDs;
                try
                {
                    // get id's of addresses allready assoicated with this cart they are being replaced
                    string sSql;
                    sSql = "select nContactKey, cContactType, nAuditKey from tblCartContact inner join tblAudit a on nAuditId = a.nAuditKey where nContactCartId = " + mnCartId.ToString();
                    oDs = moDBHelper.GetDataSet(sSql, "tblCartContact");
                    string savedBillingId = "";
                    string savedDeliveryId = "";
                    string savedBillingAuditId = "";
                    string savedDeliveryAuditId = "";
                    foreach (DataRow odr in oDs.Tables["tblCartContact"].Rows)
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(odr["cContactType"], "Billing Address", false)))
                        {
                            if (!string.IsNullOrEmpty(savedBillingId))
                            {
                                // delete any duplicates
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, Conversions.ToLong(odr["nContactKey"]));
                            }
                            else
                            {
                                savedBillingId = Conversions.ToString(odr["nContactKey"]);
                                savedBillingAuditId = Conversions.ToString(odr["nAuditKey"]);
                            }
                        }
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(odr["cContactType"], "Delivery Address", false)))
                        {
                            if (!string.IsNullOrEmpty(savedDeliveryId))
                            {
                                // delete any duplicates
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartContact, Conversions.ToLong(odr["nContactKey"]));
                            }
                            else
                            {
                                savedDeliveryId = Conversions.ToString(odr["nContactKey"]);
                                savedDeliveryAuditId = Conversions.ToString(odr["nAuditKey"]);
                            }
                        }
                    }
                    oDs = null;

                    // this should update the billing address
                    var billInstance = myWeb.moPageXml.CreateElement("Instance");
                    billInstance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, billingId);
                    billInstance.SelectSingleNode("*/nContactKey").InnerText = savedBillingId;
                    billInstance.SelectSingleNode("*/nContactCartId").InnerText = mnCartId.ToString();
                    billInstance.SelectSingleNode("*/cContactType").InnerText = "Billing Address";
                    billInstance.SelectSingleNode("*/nAuditId").InnerText = savedBillingAuditId;
                    billInstance.SelectSingleNode("*/nAuditKey").InnerText = savedBillingAuditId;
                    if (billingContactXml != null) { 
                        billInstance.SelectSingleNode("*/cContactXml    ").InnerXml = billingContactXml;
                    }

                    moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, billInstance);

                    // now get the submitted delivery id instance
                    if (!(deliveryId == 0L))
                    {
                        var delInstance = myWeb.moPageXml.CreateElement("Instance");
                        delInstance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, deliveryId);
                        delInstance.SelectSingleNode("*/nContactKey").InnerText = savedDeliveryId;
                        delInstance.SelectSingleNode("*/nContactCartId").InnerText = mnCartId.ToString();
                        delInstance.SelectSingleNode("*/cContactType").InnerText = "Delivery Address";
                        delInstance.SelectSingleNode("*/nAuditId").InnerText = savedDeliveryAuditId;
                        delInstance.SelectSingleNode("*/nAuditKey").InnerText = savedDeliveryAuditId;
                        moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, delInstance);
                    }
                }

                // here we should update the current instance so we can calculate the shipping later


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "useAddressesOnCart", ex, "", cProcessInfo, gbDebug);
                }
            }

            protected void UpdateExistingUserAddress(ref Cms.xForm oContactXform)
            {
                myWeb.PerfMon.Log("Cart", "UpdateExistingUserAddress");
                // Check if it exists - if it does then update the nContactKey node
                var oTempCXform = new Cms.xForm(ref myWeb.msException);
                string cProcessInfo = "";
                string sSql;
                long nCount;
                XmlElement oElmt2;
                try
                {

                    foreach (XmlElement oAddElmt in oContactXform.Instance.SelectNodes("tblCartContact"))
                    {
                        if (oAddElmt.GetAttribute("saveToUser") != "false")
                        {

                            // does this address allready exist?
                            sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("select count(nContactKey) from tblCartContact where nContactDirId = " + myWeb.mnUserId + " and nContactCartId = 0 " + " and cContactName = '", SqlFmt(oAddElmt.SelectSingleNode("cContactName").InnerText)), "'"), " and cContactCompany = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactCompany").InnerText)), "'"), " and cContactAddress = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactAddress").InnerText)), "'"), " and cContactCity = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactCity").InnerText)), "'"), " and cContactState = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactState").InnerText)), "'"), " and cContactZip = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactZip").InnerText)), "'"), " and cContactCountry = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactCountry").InnerText)), "'"), " and cContactTel = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactTel").InnerText)), "'"), " and cContactFax = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactFax").InnerText)), "'"), " and cContactEmail = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactEmail").InnerText)), "'"), " and cContactXml = '"), SqlFmt(oAddElmt.SelectSingleNode("cContactXml").InnerXml)), "'"));

                            nCount = Conversions.ToLong(moDBHelper.ExeProcessSqlScalar(sSql));

                            if (nCount == 0L)
                            {

                                oTempCXform.NewFrm("tblCartContact");
                                oTempCXform.Instance.InnerXml = oAddElmt.OuterXml;
                                var tempInstance = moPageXml.CreateElement("instance");
                                string ContactType = oTempCXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText;
                                // Update/add the address to the table
                                // make sure we are inserting by reseting the key

                                if (!string.IsNullOrEmpty(myWeb.moRequest["userAddId"]) & (ContactType ?? "") == (myWeb.moRequest["userAddType"] ?? ""))
                                {
                                    // get the id we are updating
                                    long updateId = Conversions.ToLong(myWeb.moRequest["userAddId"]);

                                    oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactKey").InnerText = updateId.ToString();
                                    // We need to populate the auditId feilds
                                    tempInstance.InnerXml = moDBHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartContact, updateId);
                                    // update with the fields specified
                                    foreach (XmlElement oElmt in oTempCXform.Instance.SelectNodes("tblCartContact/*[node()!='']"))
                                    {
                                        if (!(oElmt.Name == "nAuditId" | oElmt.Name == "nAuditKey"))
                                        {
                                            oElmt2 = (XmlElement)tempInstance.SelectSingleNode("tblCartContact/" + oElmt.Name);
                                            oElmt2.InnerXml = oElmt.InnerXml;
                                        }
                                    }
                                    oTempCXform.Instance = tempInstance;
                                }

                                else
                                {
                                    oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactKey").InnerText = "0";
                                    oTempCXform.Instance.SelectSingleNode("tblCartContact/nAuditId").InnerText = "";
                                    oTempCXform.Instance.SelectSingleNode("tblCartContact/nAuditKey").InnerText = "";
                                }

                                // separate from cart
                                oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactCartId").InnerText = "0";
                                // link to user
                                oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = myWeb.mnUserId.ToString();
                                moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oTempCXform.Instance);

                            }
                        }
                    }

                    setCurrentBillingAddress((long)myWeb.mnUserId, 0L);
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateExistingUserAddress", ex, "", cProcessInfo, gbDebug);
                }
                finally
                {
                    oTempCXform = (Cms.xForm)null;
                }
            }

            public virtual string discountsProcess(XmlElement oElmt)
            {
                string sCartCmd = mcCartCmd;
                string cProcessInfo = "";
                bool bAlwaysAskForDiscountCode = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(moCartConfig["AlwaysAskForDiscountCode"]) == "on", true, false));
                bool bSkipDiscountCode = Conversions.ToBoolean(Interaction.IIf(Strings.LCase(moCartConfig["SkipDiscountCode"]) == "on", true, false));
                try
                {

                    myWeb.moSession["cLogonCmd"] = "";
                    GetCart(ref oElmt);
                    if (bSkipDiscountCode)
                    {
                        oElmt.RemoveAll();
                        sCartCmd = "RedirectSecure";
                    }
                    else if (moDiscount.bHasPromotionalDiscounts | bAlwaysAskForDiscountCode)
                    {
                        var oDiscountsXform = this.discountsXform("discountsForm", "?pgid=" + myWeb.mnPageId + "&cartCmd=Discounts");
                        if (oDiscountsXform.valid == false)
                        {
                            moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oDiscountsXform.moXformElmt);
                        }

                        else
                        {
                            oElmt.RemoveAll();
                            sCartCmd = "RedirectSecure";
                        }
                    }
                    else
                    {
                        oElmt.RemoveAll();
                        sCartCmd = "RedirectSecure";
                    }


                    // if this returns Notes then we display for otherwise we goto processflow
                    return sCartCmd;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "discountsProcess", ex, "", cProcessInfo, gbDebug);
                    return "";
                }

            }


            public virtual Cms.xForm discountsXform(string formName = "notesForm", string action = "?cartCmd=Discounts")
            {
                myWeb.PerfMon.Log("Cart", "discountsXform");
                // this function is called for the collection from a form and addition to the database
                // of address information.

                DataSet oDs;
                string sSql;
                XmlElement oFormGrp;
                string sXmlContent;
                XmlElement promocodeElement = null;
                bool usedPromocodeFromExternalRef = false;

                string cProcessInfo = "";
                try
                {
                    // Get notes XML
                    var oXform = new Cms.xForm(ref myWeb.msException);
                    oXform.moPageXML = moPageXml;
                    // oXform.NewFrm(formName)
                    string cDiscountsXform = moCartConfig["DiscountsXform"];

                    if (!string.IsNullOrEmpty(cDiscountsXform))
                    {
                        if (!oXform.load(cDiscountsXform))
                        {
                            oXform.NewFrm(formName);
                            oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "discounts", sLabel: "Missing File: " + mcNotesXForm);
                        }
                        else
                        {
                            // add missing submission or submit buttons
                            if (oXform.moXformElmt.SelectSingleNode("model/submission") is null)
                            {
                                // If oXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                                oXform.submission(formName, action, "POST", "return form_check(this);");
                            }
                            if (oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit") is null)
                            {
                                oXform.addSubmit(ref oXform.moXformElmt, "Submit", "Continue");
                            }

                            XmlElement oSubmit = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit");
                            if (oSubmit != null)
                            {
                                oFormGrp = (XmlElement)oSubmit.ParentNode;
                            }
                            else
                            {
                                oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "Promo", sLabel: "Enter Promotional Code");
                            }
                            if (oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode") is null)
                            {
                                if (oXform.Instance.FirstChild.SelectSingleNode("Notes") is null)
                                {
                                    oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"));
                                }
                                promocodeElement = (XmlElement)oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"));
                                oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");
                            }

                        }
                    }
                    else
                    {
                        oXform.NewFrm(formName);
                        oXform.submission(formName, action, "POST", "return form_check(this);");
                        oXform.Instance.InnerXml = "<Notes/>";
                        oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "notes", sLabel: "");

                        if (oXform.Instance.FirstChild.SelectSingleNode("Notes") is null)
                        {
                            oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"));
                        }

                        promocodeElement = (XmlElement)oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"));
                        oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");
                        oXform.addSubmit(ref oFormGrp, "Submit", "Continue");

                    }
                    // Open database for reading and writing


                    // External promo code checks
                    if (promocodeElement != null & !string.IsNullOrEmpty(promocodeFromExternalRef))
                    {

                        usedPromocodeFromExternalRef = true;
                    }

                    sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                    {
                        // load existing notes from Cart
                        sXmlContent = Conversions.ToString(Operators.ConcatenateObject(oRow["cClientNotes"], ""));
                        if (!string.IsNullOrEmpty(sXmlContent))
                        {
                            oXform.Instance.InnerXml = sXmlContent;
                        }

                        // If this xform is being submitted
                        bool isSubmitted = oXform.isSubmitted();
                        if (isSubmitted | myWeb.moRequest["Submit"] == "Continue" | myWeb.moRequest["Submit"] == "Search")
                        {
                            oXform.updateInstanceFromRequest();
                            oXform.validate();
                            if (oXform.valid == true)
                            {
                                oRow["cClientNotes"] = oXform.Instance.InnerXml;
                                mcCartCmd = "RedirectSecure";
                            }
                        }
                        else if (!isSubmitted & usedPromocodeFromExternalRef)
                        {
                            // If an external promo code is in the system then save it, even before it has been submitted
                            promocodeElement = (XmlElement)oXform.Instance.SelectSingleNode("//PromotionalCode");
                            if (promocodeElement != null)
                            {
                                promocodeElement.InnerText = promocodeFromExternalRef;
                                oRow["cClientNotes"] = oXform.Instance.InnerXml;
                                // Promo code is officially in the process, so we can ditch any transitory variables.
                                promocodeFromExternalRef = "";
                            }
                        }
                    }
                    moDBHelper.updateDataset(ref oDs, "Order", true);

                    oDs.Clear();
                    oDs = null;
                    oXform.addValues();

                    return oXform;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "discountsXform", ex, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
                }

            }


            public virtual string notesProcess(XmlElement oElmt)
            {
                string sCartCmd = mcCartCmd;
                string cProcessInfo = "";
                try
                {

                    // should never get this far for subscriptions unless logged on.

                    if (moSubscription != null)
                    {
                        if (!moSubscription.CheckCartForSubscriptions(mnCartId, myWeb.mnUserId))
                        {
                            if (myWeb.mnUserId == 0)
                            {
                                sCartCmd = "LogonSubs";
                            }
                        }
                    }

                    myWeb.moSession["cLogonCmd"] = "";

                    GetCart(ref oElmt);

                    if (!string.IsNullOrEmpty(mcNotesXForm))
                    {
                        var oNotesXform = notesXform("notesForm", mcPagePath + "cartCmd=Notes", oElmt);
                        if (oNotesXform.valid == false)
                        {
                            moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oNotesXform.moXformElmt);
                        }
                        else
                        {
                            oElmt.RemoveAll();
                            sCartCmd = "SkipAddress";
                        }
                    }
                    else
                    {
                        oElmt.RemoveAll();
                        sCartCmd = "SkipAddress";
                    }

                    // if this returns Notes then we display for otherwise we goto processflow
                    return sCartCmd;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "notesProcess", ex, "", cProcessInfo, gbDebug);
                    return "";
                }




            }






            public virtual Cms.xForm notesXform(string formName = "notesForm", string action = "?cartCmd=Notes", XmlElement oCart = null)
            {
                Cms.xForm notesXformRet = default;
                myWeb.PerfMon.Log("Cart", "notesXform");
                // this function is called for the collection from a form and addition to the database
                // of address information.

                DataSet oDs;
                string sSql;
                XmlElement oFormGrp;
                string sXmlContent;
                XmlElement promocodeElement = null;
                string cProcessInfo = "";
                try
                {
                    // Get notes XML
                    var oXform = new Cms.xForm(ref myWeb.msException);
                    oXform.moPageXML = moPageXml;
                    // 

                    switch (Strings.LCase(mcNotesXForm) ?? "")
                    {
                        case "default":
                            {
                                oXform.NewFrm(formName);
                                oXform.submission(formName, action, "POST", "return form_check(this);");
                                oXform.Instance.InnerXml = "<Notes><Notes/></Notes>";
                                oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "notes", "term4051", "Please enter any comments on your order here");
                                string argsClass = "";
                                int argnRows = 0;
                                int argnCols = 0;
                                oXform.addTextArea(ref oFormGrp, "Notes/Notes", false, "", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                                if (moDiscount.bHasPromotionalDiscounts)
                                {
                                    // If oXform.Instance.FirstChild.SelectSingleNode("Notes") Is Nothing Then
                                    XmlElement localfirstElement1() { var argoElement = oXform.Instance; var ret = Tools.Xml.firstElement(ref argoElement); oXform.Instance = argoElement; return ret; }

                                    if (localfirstElement1().SelectSingleNode("Notes") is null)
                                    {
                                        // oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                        // Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                        XmlElement localfirstElement() { XmlElement argoElement1 = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance"); var ret = firstElement(ref argoElement1); return ret; }

                                        localfirstElement().AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"));
                                    }
                                    // oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                    // Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                    XmlElement localfirstElement2() { XmlElement argoElement2 = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance"); var ret = firstElement(ref argoElement2); return ret; }

                                    promocodeElement = (XmlElement)localfirstElement2().AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"));
                                    oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");
                                }
                                oXform.addSubmit(ref oFormGrp, "Submit", "Continue");
                                break;
                            }
                        case "productspecific":
                            {
                                XmlElement oMasterFormXml = null;
                                foreach (XmlElement oOrderLine in oCart.SelectNodes("Item"))
                                {
                                    // get any Xform related to cart items
                                    long contentId = Conversions.ToLong(oOrderLine.GetAttribute("contentId"));
                                    sSql = "select nContentKey from tblContent c inner join tblContentRelation cr on cr.nContentChildId = c.nContentKey where c.cContentSchemaName = 'xform' and cr.nContentParentId = " + contentId;
                                    long FormId = Conversions.ToLong(myWeb.moDbHelper.GetDataValue(sSql));
                                    if (FormId != default)
                                    {
                                        var oFormXml = moPageXml.CreateElement("NewXform");
                                        oFormXml.InnerXml = myWeb.moDbHelper.getContentBrief((int)FormId);

                                        if (oMasterFormXml is null)
                                        {
                                            // Duplication the items for each qty in cart
                                            int n = 1;
                                            XmlElement oItem = (XmlElement)oFormXml.SelectSingleNode("descendant-or-self::Item");
                                            oItem.SetAttribute("name", oOrderLine.SelectSingleNode("Name").InnerText);
                                            oItem.SetAttribute("stockCode", oOrderLine.SelectSingleNode("productDetail/StockCode").InnerText);
                                            oItem.SetAttribute("number", n.ToString());

                                            int i;
                                            var loopTo = Conversions.ToInteger(oOrderLine.GetAttribute("quantity"));
                                            for (i = 2; i <= loopTo; i++)
                                            {
                                                n = n + 1;
                                                XmlElement newItem = (XmlElement)oItem.CloneNode(true);
                                                newItem.SetAttribute("number", n.ToString());
                                                oItem.ParentNode.InsertAfter(newItem, oItem.ParentNode.LastChild);
                                            }
                                            oMasterFormXml = oFormXml;
                                        }
                                        else
                                        {
                                            // behaviour for appending additioanl product forms
                                            int n = 1;
                                            XmlElement oItem = (XmlElement)oFormXml.SelectSingleNode("descendant-or-self::Item");
                                            oItem.SetAttribute("name", oOrderLine.SelectSingleNode("Name").InnerText);
                                            oItem.SetAttribute("stockCode", oOrderLine.SelectSingleNode("productDetail/StockCode").InnerText);
                                            oItem.SetAttribute("number", n.ToString());

                                            int i;
                                            var loopTo1 = Conversions.ToInteger(oOrderLine.GetAttribute("quantity"));
                                            for (i = 1; i <= loopTo1; i++)
                                            {
                                                XmlElement newItem = (XmlElement)oItem.CloneNode(true);
                                                newItem.SetAttribute("number", n.ToString());
                                                n = n + 1;
                                                XmlElement AddAfterNode = (XmlElement)oMasterFormXml.SelectSingleNode("Content/model/instance/Notes/Item[last()]");
                                                AddAfterNode.ParentNode.InsertAfter(newItem, AddAfterNode);
                                            }
                                        }
                                    }

                                }
                                // Load with repeats.
                                if (oMasterFormXml != null)
                                {
                                    var argoNode = oMasterFormXml.SelectSingleNode("descendant-or-self::Content");
                                    oXform.load(ref argoNode, true);
                                }

                                if (moDiscount.bHasPromotionalDiscounts)
                                {

                                    XmlElement oNotesRoot = (XmlElement)oXform.Instance.SelectSingleNode("Notes");
                                    if (oNotesRoot.SelectSingleNode("PromotionalCode") is null)
                                    {
                                        promocodeElement = (XmlElement)oNotesRoot.AppendChild(oMasterFormXml.OwnerDocument.CreateElement("PromotionalCode"));
                                    }

                                    oFormGrp = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::group[1]");
                                    oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");

                                }

                                if (oXform.moXformElmt != null)
                                {
                                    // add missing submission or submit buttons
                                    if (oXform.moXformElmt.SelectSingleNode("model/submission") is null)
                                    {
                                        // If oXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                                        oXform.submission(formName, action, "POST", "return form_check(this);");
                                    }
                                    if (oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit") is null)
                                    {
                                        oXform.addSubmit(ref oXform.moXformElmt, "Submit", "Continue");
                                    }
                                    oXform.moXformElmt.SetAttribute("type", "xform");
                                    oXform.moXformElmt.SetAttribute("name", "notesForm");
                                }
                                else
                                {
                                    oXform.NewFrm(formName);
                                    oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "notes", sLabel: "Missing File: Product has no form request ");
                                    // force to true so we move on.
                                    oXform.valid = true;
                                }

                                break;
                            }

                        default:
                            {
                                if (!oXform.load(mcNotesXForm))
                                {
                                    oXform.NewFrm(formName);
                                    oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "notes", sLabel: "Missing File: " + mcNotesXForm);
                                }
                                else
                                {
                                    string cTicketTypes = moCartConfig["TicketTypes"];
                                    // 'Modify the notes for dependant on tickets
                                    int totalAttendees = 0;
                                    if (!string.IsNullOrEmpty(cTicketTypes))
                                    {
                                        if (!(moCartXml.SelectNodes("Order/Item[productDetail/Name[@ticketType!='']]").Count == 0))
                                        {
                                            string ticketType;
                                            // For Each ticketType In Split(cTicketTypes, ",")
                                            XmlElement oNotesRoot = (XmlElement)oXform.Instance.SelectSingleNode("Notes/Notes");
                                            XmlElement oBindRoot = (XmlElement)oXform.model.SelectSingleNode("bind");
                                            XmlElement oControlRoot = (XmlElement)oXform.moXformElmt.SelectSingleNode("group");

                                            // Case for Run
                                            XmlElement newElmt2;
                                            int nCount = 0;
                                            int i = 0;

                                            // For Each oItemElmt In moCartXml.SelectNodes("Order/Item[productDetail/Name[@ticketType='" & ticketType & "']]")
                                            foreach (XmlElement oItemElmt in moCartXml.SelectNodes("Order/Item"))
                                            {

                                                ticketType = oItemElmt.SelectSingleNode("productDetail/Name/@ticketType").InnerText;

                                                XmlElement blankElmt = (XmlElement)oXform.Instance.SelectSingleNode("Notes/Notes/Attendee[@type='" + ticketType + "'][1]");
                                                XmlElement blankBind = (XmlElement)oXform.model.SelectSingleNode("bind/bind[@nodeset='Attendee' and @class='" + ticketType + "'][1]");
                                                XmlElement blankControl = (XmlElement)oXform.moXformElmt.SelectSingleNode("group/group[contains(@class,'" + ticketType + "')][1]");

                                                var loopTo2 = Conversions.ToInteger(oItemElmt.GetAttribute("quantity"));
                                                for (i = 1; i <= loopTo2; i++)
                                                {
                                                    totalAttendees = totalAttendees + 1;
                                                    // Update the instance
                                                    oNotesRoot.AppendChild(blankElmt.CloneNode(true));
                                                    XmlElement newElmt = (XmlElement)oNotesRoot.LastChild;
                                                    newElmt.SelectSingleNode("AttTicketType").InnerText = oItemElmt.SelectSingleNode("Name").InnerText + " - " + moCartConfig["TicketAttendeeLabel"] + " " + i;
                                                    newElmt.SetAttribute("id", ticketType + nCount);
                                                    newElmt.SetAttribute("itemId", oItemElmt.GetAttribute("id"));

                                                    newElmt = null;

                                                    // Update the binds
                                                    oBindRoot.AppendChild(blankBind.CloneNode(true));
                                                    newElmt = (XmlElement)oBindRoot.LastChild;
                                                    newElmt.SetAttribute("nodeset", "Attendee[@id='" + ticketType + nCount + "']");
                                                    foreach (XmlElement currentNewElmt2 in newElmt.SelectNodes("descendant-or-self::*"))
                                                    {
                                                        newElmt2 = currentNewElmt2;
                                                        if (!string.IsNullOrEmpty(newElmt2.GetAttribute("id")))
                                                        {
                                                            newElmt2.SetAttribute("id", newElmt2.GetAttribute("id") + "-" + ticketType + nCount);
                                                        }
                                                        // remove lead booker from all subsequent tickets
                                                        if (totalAttendees > 1 & newElmt2.GetAttribute("lead-booker-only") == "true")
                                                        {
                                                            newElmt2.SetAttribute("required", "false()");
                                                        }
                                                    }
                                                    newElmt = null;
                                                    // Update the controls
                                                    if (blankControl != null)
                                                    {
                                                        blankControl.SetAttribute("id", "ticket-form-" + totalAttendees);
                                                        oControlRoot.AppendChild(blankControl.CloneNode(true));
                                                    }
                                                    newElmt = (XmlElement)oControlRoot.LastChild;

                                                    var labelElmt = moPageXml.CreateElement("label");
                                                    labelElmt.InnerText = oItemElmt.SelectSingleNode("Name").InnerText + " - " + moCartConfig["TicketAttendeeLabel"] + " " + i;
                                                    newElmt.InsertBefore(labelElmt, newElmt.FirstChild);

                                                    foreach (XmlElement currentNewElmt21 in newElmt.SelectNodes("descendant-or-self::*[@bind]"))
                                                    {
                                                        newElmt2 = currentNewElmt21;
                                                        if (!string.IsNullOrEmpty(newElmt2.GetAttribute("bind")))
                                                        {
                                                            if (i != Conversions.ToDouble(oItemElmt.GetAttribute("quantity")))
                                                            {
                                                                // remove all but the last delcarations
                                                                if (newElmt2.GetAttribute("bind").StartsWith("AttDeclaration"))
                                                                {
                                                                    // newElmt2.ParentNode.RemoveChild(newElmt2.PreviousSibling)
                                                                    XmlElement delGroup = (XmlElement)newElmt2.ParentNode;
                                                                    delGroup.SetAttribute("delete", Conversions.ToString(true));
                                                                }
                                                            }

                                                            newElmt2.SetAttribute("bind", newElmt2.GetAttribute("bind") + "-" + ticketType + nCount);
                                                        }
                                                    }
                                                    if (totalAttendees > 1)
                                                    {
                                                        foreach (XmlElement currentNewElmt22 in newElmt.SelectNodes("descendant-or-self::*[@lead-booker-only='true']"))
                                                        {
                                                            newElmt2 = currentNewElmt22;
                                                            // remove lead booker from all subsequent tickets
                                                            newElmt2.ParentNode.RemoveChild(newElmt2);
                                                        }
                                                    }

                                                    newElmt = null;
                                                    nCount = nCount + 1;
                                                }

                                            }

                                            // remove the blanks
                                            foreach (XmlElement currentNewElmt23 in oControlRoot.SelectNodes("descendant-or-self::*[@delete]"))
                                            {
                                                newElmt2 = currentNewElmt23;
                                                newElmt2.ParentNode.RemoveChild(newElmt2);
                                            }


                                            foreach (var currentTicketType in cTicketTypes.Split(','))
                                            {
                                                ticketType = currentTicketType;
                                                // remove the initial versions
                                                XmlElement blankElmt = (XmlElement)oXform.Instance.SelectSingleNode("Notes/Notes/Attendee[@type='" + ticketType + "'][1]");
                                                XmlElement blankBind = (XmlElement)oXform.model.SelectSingleNode("bind/bind[@nodeset='Attendee' and @class='" + ticketType + "'][1]");
                                                XmlElement blankControl = (XmlElement)oXform.moXformElmt.SelectSingleNode("group/group[contains(@class,'" + ticketType + "')][1]");

                                                blankElmt.ParentNode.RemoveChild(blankElmt);
                                                blankBind.ParentNode.RemoveChild(blankBind);
                                                blankControl.ParentNode.RemoveChild(blankControl);
                                            }



                                        }
                                    }


                                    // add missing submission or submit buttons
                                    if (oXform.moXformElmt.SelectSingleNode("model/submission") is null)
                                    {
                                        // If oXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                                        oXform.submission(formName, action, "POST", "return form_check(this);");
                                    }
                                    if (oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit") is null)
                                    {
                                        oXform.addSubmit(ref oXform.moXformElmt, "Submit", "Continue");
                                    }
                                    if (moDiscount.bHasPromotionalDiscounts)
                                    {
                                        XmlElement oSubmit = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit");
                                        if (oSubmit != null)
                                        {
                                            oFormGrp = (XmlElement)oSubmit.ParentNode;
                                        }
                                        else
                                        {
                                            oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "Promo", sLabel: "Enter Promotional Code");
                                        }
                                        if (oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode") is null)
                                        {
                                            // If oXform.Instance.FirstChild.SelectSingleNode("Notes") Is Nothing Then
                                            XmlElement localfirstElement4() { var argoElement3 = oXform.Instance; var ret = Tools.Xml.firstElement(ref argoElement3); oXform.Instance = argoElement3; return ret; }

                                            if (localfirstElement4().SelectSingleNode("Notes") is null)
                                            {
                                                // ocNode.AppendChild(moPageXml.ImportNode(Protean.Tools.Xml.firstElement(newXml.DocumentElement), True))
                                                // oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                                // Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                                XmlElement localfirstElement3() { XmlElement argoElement4 = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance"); var ret = firstElement(ref argoElement4); return ret; }

                                                localfirstElement3().AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"));
                                            }
                                            // oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                            // Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                            XmlElement localfirstElement5() { XmlElement argoElement5 = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance"); var ret = firstElement(ref argoElement5); return ret; }

                                            promocodeElement = (XmlElement)localfirstElement5().AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"));
                                            oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");
                                        }
                                    }
                                }

                                break;
                            }
                    }

                    // External promo code checks
                    if (promocodeElement != null & !string.IsNullOrEmpty(promocodeFromExternalRef))
                    {
                        promocodeElement.InnerText = promocodeFromExternalRef;
                        // Promo code is officially in the process, so we can ditch any transitory variables.
                        promocodeFromExternalRef = "";
                    }

                    // Open database for reading and writing

                    sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                    {
                        // load existing notes from Cart
                        sXmlContent = Conversions.ToString(Operators.ConcatenateObject(oRow["cClientNotes"], ""));
                        if (!string.IsNullOrEmpty(sXmlContent))
                        {

                            var savedInstance = moPageXml.CreateElement("instance");
                            moPageXml.PreserveWhitespace = false;
                            savedInstance.InnerXml = sXmlContent;

                            if (oXform.Instance.SelectNodes("*/*/*").Count > savedInstance.SelectNodes("*/*/*").Count)
                            {
                                // we have a greater amount of childnodes we need to merge....

                                // Dim oStepElmtCount As Integer = 0

                                // step through each child element and replace where attributes match, leaving final
                                foreach (XmlElement oStepElmt in oXform.Instance.SelectNodes("*/*/*"))
                                {

                                    XmlElement replacementNode = (XmlElement)savedInstance.SelectSingleNode("*/*/*[@id='" + oStepElmt.GetAttribute("id") + "']");

                                    if (replacementNode != null)
                                    {
                                        oStepElmt.ParentNode.ReplaceChild(replacementNode.CloneNode(true), oStepElmt);
                                    }


                                    // Dim attXpath As String = oStepElmt.Name
                                    // Dim attElmt As XmlAttribute
                                    // Dim bfirst As Boolean = True
                                    // For Each attElmt In oStepElmt.Attributes
                                    // If bfirst Then attXpath = attXpath & "["
                                    // If Not bfirst Then attXpath = attXpath & " and "
                                    // attXpath = attXpath + "@" & attElmt.Name & "='" & attElmt.Value & "'"
                                    // bfirst = False
                                    // Next
                                    // If Not bfirst Then attXpath = attXpath & "]"

                                    // If Not savedInstance.SelectSingleNode("*/*/" & attXpath) Is Nothing Then
                                    // oStepElmt.ParentNode.ReplaceChild(savedInstance.SelectSingleNode("*/*/" & attXpath).CloneNode(True), oStepElmt)
                                    // End If
                                    // oStepElmtCount = oStepElmtCount + 1
                                }
                            }

                            else
                            {
                                oXform.Instance.InnerXml = sXmlContent;
                            }

                        }

                        // If this xform is being submitted

                        if (oXform.isSubmitted() | myWeb.moRequest["Submit"] == "Continue" | myWeb.moRequest["Submit"] == "Search")
                        {
                            oXform.updateInstanceFromRequest();
                            oXform.validate();
                            if (!string.IsNullOrEmpty(moCartConfig["NotesToContactsXSL"]))
                            {

                                oXform.Instance.SetAttribute("userId", mnEwUserId.ToString());
                                oXform.Instance.SetAttribute("cartId", mnCartId.ToString());

                                var oInstanceDoc = new XmlDocument();
                                oInstanceDoc.LoadXml(oXform.Instance.OuterXml);

                                var oTransform = new Protean.XmlHelper.Transform(ref myWeb, moServer.MapPath(moCartConfig["NotesToContactsXSL"]), false);
                                var ImportElmt = oTransform.ProcessDocument(oInstanceDoc).DocumentElement;

                                moDBHelper.importObjects(ImportElmt, mnCartId.ToString(), "");

                                oTransform = (Protean.XmlHelper.Transform)null;

                            }
                            if (oXform.valid == true)
                            {
                                oRow["cClientNotes"] = oXform.Instance.InnerXml;
                                // if we are useing the notes as a search facility for products
                                if (myWeb.moRequest["Submit"] == "Search")
                                {
                                    mcCartCmd = "Search";
                                }
                                else
                                {
                                    mcCartCmd = "SkipAddress";
                                }
                            }
                        }
                    }
                    moDBHelper.updateDataset(ref oDs, "Order", true);

                    oDs.Clear();
                    oDs = null;
                    oXform.addValues();
                    notesXformRet = oXform;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "notesXform", ex, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
                }

                return notesXformRet;

            }

            public virtual Cms.xForm optionsXform(ref XmlElement cartElmt)
            {

                myWeb.PerfMon.Log("Cart", "optionsXform");
                DataSet ods;
                DataSet ods2;
                DataRow oRow;

                string sSql;
                string sSql2;

                XmlElement oGrpElmt;

                short nQuantity;
                double nAmount;
                double nWeight;
                string cDestinationCountry;
                string cDestinationPostalCode = "";
                var nShippingCost = default(double);
                string cShippingDesc = "";

                string cHidden = string.Empty;
                bool bHideDelivery = false;
                bool bHidePayment = false;
                bool bFirstRow = true;

                // Dim oElmt As XmlElement

                string sProcessInfo = string.Empty;
                bool bForceValidation = false;
                bool bAdjustTitle = true;

                // Dim cFormURL As String
                // Dim cExternalGateway As String
                // Dim cBillingAddress As String
                // Dim cPaymentResponse As String
                string cProcessInfo = "";
                bool bAddTerms = false;
                PaymentProviders oPay;
                bool bDeny = false;
                var AllowedPaymentMethods = new System.Collections.Specialized.StringCollection();

                if (moPay is null)
                {
                    oPay = new PaymentProviders(ref myWeb);
                }
                else
                {
                    oPay = moPay;
                }

                oPay.mcCurrency = mcCurrency;
                string cDenyFilter = string.Empty;

                try
                {

                    if (moDBHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel"))
                    {
                        bDeny = true;
                        cDenyFilter = " and nPermLevel <> 0";
                    }

                    if (string.IsNullOrEmpty(moCartConfig["TermsContentId"]) & string.IsNullOrEmpty(moCartConfig["TermsAndConditions"]))
                        bAddTerms = false;

                    nQuantity = (short)Conversions.ToInteger("0" + cartElmt.GetAttribute("itemCount"));
                    nAmount = Conversions.ToDouble("0" + cartElmt.GetAttribute("totalNet")) - Conversions.ToDouble("0" + cartElmt.GetAttribute("shippingCost"));
                    nWeight = Conversions.ToDouble("0" + cartElmt.GetAttribute("weight"));

                    double nRepeatAmount = Conversions.ToDouble("0" + cartElmt.GetAttribute("repeatPrice"));

                    int nShippingMethodId = (int)Math.Round(Conversions.ToDouble("0" + cartElmt.GetAttribute("shippingType")));

                    if (cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country") is null)
                    {
                        sProcessInfo = "Destination Country not specified in Delivery Address";
                        cDestinationCountry = "";
                        string sTarget = "";
                        foreach (XmlElement oAddressElmt in cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/*"))
                        {
                            if (!string.IsNullOrEmpty(sTarget))
                                sTarget = sTarget + ", ";
                            sTarget = sTarget + oAddressElmt.InnerText;
                        }
                        Information.Err().Raise(1004, "getParentCountries", sTarget + " Destination Country not specified in Delivery Address.");
                    }
                    else
                    {
                        cDestinationCountry = cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText;
                        cDestinationPostalCode = cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/PostalCode").InnerText;
                    }
                    if (string.IsNullOrEmpty(cDestinationCountry))
                        cDestinationCountry = moCartConfig["DefaultCountry"];
                    // Go and collect the valid shipping options available for this order
                    ods = getValidShippingOptionsDS(cDestinationCountry, cDestinationPostalCode, nAmount, nQuantity, nWeight,"",0);

                    var oOptXform = new Cms.xForm(ref myWeb.msException);
                    oOptXform.moPageXML = moPageXml;

                    if (!oOptXform.load("/xforms/Cart/Options.xml"))
                    {
                        string notesXml = "";
                        if (cartElmt.SelectSingleNode("Notes") != null)
                        {
                            notesXml = cartElmt.SelectSingleNode("Notes").OuterXml;
                        }
                        oOptXform.NewFrm("optionsForm");
                        oOptXform.Instance.InnerXml = "<nShipOptKey/><cPaymentMethod/><terms/><confirmterms>No</confirmterms><tblCartOrder><cShippingDesc/><cClientNotes>" + notesXml + "</cClientNotes></tblCartOrder>";
                        if (!(string.IsNullOrEmpty(moCartConfig["TermsContentId"]) & string.IsNullOrEmpty(moCartConfig["TermsAndConditions"])))
                        {
                            bAddTerms = true;
                        }
                    }
                    else
                    {
                        bAdjustTitle = false;
                        bForceValidation = true;
                        if (!(string.IsNullOrEmpty(moCartConfig["TermsContentId"]) & string.IsNullOrEmpty(moCartConfig["TermsAndConditions"])))
                        {
                            bAddTerms = true;
                        }
                    }

                    // If there is already a submit item in the form, then maintain the event node
                    // Would rather that this whole form obeyed xform validation, but hey-ho. Ali
                    string cEvent = "";
                    XmlElement oSub = (XmlElement)oOptXform.model.SelectSingleNode("submission");
                    if (oSub is null)
                    {
                        cEvent = "return form_check(this);";
                    }
                    else
                    {
                        cEvent = oSub.GetAttribute("event");

                        // now remove the origional submit node coz we are going to add another. TS.
                        oSub.ParentNode.RemoveChild(oSub);
                    }

                    oOptXform.submission("optionsForm", mcPagePath + "cartCmd=ChoosePaymentShippingOption", "POST", cEvent);

                    string cUserGroups = "";

                    long rowCount = ods.Tables["Option"].Rows.Count;

                    if (bDeny)
                    {
                        // remove denied delivery methods
                        if (myWeb.mnUserId > 0)
                        {
                            foreach (XmlElement grpElmt in moPageXml.SelectNodes("/Page/User/Group[@isMember='yes']"))
                                cUserGroups = cUserGroups + grpElmt.GetAttribute("id") + ",";
                            cUserGroups = cUserGroups + Cms.gnAuthUsers;
                        }
                        else
                        {
                            cUserGroups = Cms.gnNonAuthUsers.ToString();
                        }

                        foreach (DataRow currentORow in ods.Tables["Option"].Rows)
                        {
                            oRow = currentORow;
                            int denyCount = 0;
                            if (bDeny)
                            {
                                string permSQL;
                                // check option is not denied
                                if (!string.IsNullOrEmpty(cUserGroups))
                                {
                                    permSQL = Conversions.ToString(Operators.ConcatenateObject("select count(*) from tblCartShippingPermission where nPermLevel = 0 and nDirId IN (" + cUserGroups + ") and nShippingMethodId = ", oRow["nShipOptKey"]));
                                    denyCount = Conversions.ToInteger(moDBHelper.ExeProcessSqlScalar(permSQL));
                                }
                            }
                            if (denyCount > 0)
                            {
                                oRow.Delete();
                                rowCount = rowCount - 1L;
                            }
                        }
                    }

                    if (rowCount == 0L)
                    {

                        oOptXform.addGroup(ref oOptXform.moXformElmt, "options");
                        cartElmt.SetAttribute("errorMsg", 3.ToString());
                    }

                    else
                    {

                        // Build the Payment Options
                        // if the root group element exists i.e. we have loaded a form in.
                        oGrpElmt = (XmlElement)oOptXform.moXformElmt.SelectSingleNode("group");
                        if (oGrpElmt is null)
                        {
                            oGrpElmt = oOptXform.addGroup(ref oOptXform.moXformElmt, "options", "", "Select Payment Method");
                        }


                        // Even if there is only 1 option we still want to display it, if it is a non-zero value - the visitor should know the description of their delivery option
                        if (ods.Tables["Option"].Rows.Count == 1)
                        {
                            foreach (DataRow currentORow1 in ods.Tables["Option"].Rows)
                            {
                                oRow = currentORow1;
                                bool bCollection = false;
                                if (!(oRow["bCollection"] is DBNull))
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRow["bCollection"], "1", false)))
                                        bCollection = true;
                                }
                                if (oRow["nShippingTotal"] is DBNull)
                                {
                                    cHidden = " hidden";
                                    bHideDelivery = true;
                                }
                                else if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oRow["nShippingTotal"], 0, false), !bCollection)))
                                {
                                    cHidden = " hidden";
                                    bHideDelivery = true;
                                }
                                else
                                {
                                    nShippingCost = Conversions.ToDouble(oRow["nShippingTotal"]);
                                    nShippingCost = Conversions.ToDouble(Strings.FormatNumber(nShippingCost, 2, TriState.True, TriState.False, TriState.False));

                                    oOptXform.addInput(ref oGrpElmt, "nShipOptKey", false, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cShipOptName"], "-"), oRow["cShipOptCarrier"])), "hidden");
                                    oOptXform.Instance.SelectSingleNode("nShipOptKey").InnerText = Conversions.ToString(oRow["nShipOptKey"]);

                                    var DelInputElmt = oOptXform.addInput(ref oGrpElmt, "tblCartOrder/cShippingDesc", false, "Delivery", "readonly term4047");
                                    XmlElement DelInputElmtLabel = (XmlElement)DelInputElmt.SelectSingleNode("label");
                                    DelInputElmtLabel.SetAttribute("name", Conversions.ToString(oRow["cShipOptName"]));
                                    DelInputElmtLabel.SetAttribute("carrier", Conversions.ToString(oRow["cShipOptCarrier"]));
                                    DelInputElmtLabel.SetAttribute("cost", Strings.FormatNumber(nShippingCost, 2));

                                    XmlElement DescElement = (XmlElement)oOptXform.Instance.SelectSingleNode("tblCartOrder/cShippingDesc");
                                    DescElement.InnerText = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cShipOptName"], "-"), oRow["cShipOptCarrier"]), ": "), mcCurrencySymbol), Strings.FormatNumber(nShippingCost, 2)));
                                    DescElement.SetAttribute("name", Conversions.ToString(oRow["cShipOptName"]));
                                    DescElement.SetAttribute("carrier", Conversions.ToString(oRow["cShipOptCarrier"]));
                                    DescElement.SetAttribute("cost", Strings.FormatNumber(nShippingCost, 2));
                                }
                            }
                        }
                        else
                        {
                            oOptXform.addSelect1(ref oGrpElmt, "nShipOptKey", false, "Select Delivery", "radios multiline", Protean.xForm.ApperanceTypes.Full);
                            bFirstRow = true;
                            int nLastID = 0;

                            // If selected shipping method is still in those available (because we now )
                            if (nShippingMethodId != 0)
                            {
                                bool bIsAvail = false;
                                foreach (DataRow currentORow2 in ods.Tables["Option"].Rows)
                                {
                                    oRow = currentORow2;
                                    if (!(oRow.RowState == DataRowState.Deleted))
                                    {
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(nShippingMethodId, oRow["nShipOptKey"], false)))
                                        {
                                            bIsAvail = true;
                                        }
                                    }
                                }
                                // If not then strip it out.
                                if (bIsAvail == false)
                                {
                                    nShippingMethodId = 0;
                                    cartElmt.SetAttribute("shippingType", "0");
                                    cartElmt.SetAttribute("shippingCost", "");
                                    cartElmt.SetAttribute("shippingDesc", "");
                                    string cSqlUpdate = "UPDATE tblCartOrder SET cShippingDesc= null, nShippingCost=null, nShippingMethodId = 0 WHERE nCartOrderKey=" + mnCartId;
                                    moDBHelper.ExeProcessSql(cSqlUpdate);
                                }
                            }

                            // If shipping option selected is collection don't change
                            bool bCollectionSelected = false;
                            foreach (DataRow currentORow3 in ods.Tables["Option"].Rows)
                            {
                                oRow = currentORow3;
                                if (!(oRow.RowState == DataRowState.Deleted))
                                {
                                    if (!(oRow["bCollection"] is DBNull))
                                    {
                                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oRow["nShipOptKey"], nShippingMethodId, false), Operators.ConditionalCompareObjectEqual(oRow["bCollection"], true, false))))
                                        {
                                            bCollectionSelected = true;
                                        }
                                    }
                                }
                            }

                            foreach (DataRow currentORow4 in ods.Tables["Option"].Rows)
                            {
                                oRow = currentORow4;
                                if (!(oRow.RowState == DataRowState.Deleted))
                                {
                                    if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(oRow["nShipOptKey"], nLastID, false)))
                                    {

                                        if (bCollectionSelected)
                                        {
                                            // if collection allready selected... Show only this option
                                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(nShippingMethodId, oRow["nShipOptKey"], false)))
                                            {
                                                oOptXform.Instance.SelectSingleNode("nShipOptKey").InnerText = Conversions.ToString(oRow["nShipOptKey"]);
                                                nShippingCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oRow["nShippingTotal"]));
                                                nShippingCost = Conversions.ToDouble(Strings.FormatNumber(nShippingCost, 2, TriState.True, TriState.False, TriState.False));
                                                XmlElement argoSelectNode = (XmlElement)oGrpElmt.LastChild;
                                                var optElmt = oOptXform.addOption(ref argoSelectNode, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cShipOptName"], "-"), oRow["cShipOptCarrier"]), ": "), mcCurrencySymbol), Strings.FormatNumber(nShippingCost, 2))), Conversions.ToString(oRow["nShipOptKey"]));
                                                XmlElement optLabel = (XmlElement)optElmt.SelectSingleNode("label");
                                                optLabel.SetAttribute("name", Conversions.ToString(oRow["cShipOptName"]));
                                                optLabel.SetAttribute("carrier", Conversions.ToString(oRow["cShipOptCarrier"]));
                                                optLabel.SetAttribute("cost", Strings.FormatNumber(nShippingCost, 2));
                                            }
                                        }
                                        else
                                        {
                                            bool bShowMethod = true;
                                            // Don't show if a collection method
                                            if (moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                                            {
                                                if (!(oRow["bCollection"] is DBNull))
                                                {
                                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRow["bCollection"], true, false)))
                                                    {
                                                        bShowMethod = false;
                                                    }
                                                }
                                            }
                                            if (bShowMethod)
                                            {
                                                if (bFirstRow)
                                                    oOptXform.Instance.SelectSingleNode("nShipOptKey").InnerText = Conversions.ToString(oRow["nShipOptKey"]);
                                                nShippingCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oRow["nShippingTotal"]));
                                                nShippingCost = Conversions.ToDouble(Strings.FormatNumber(nShippingCost, 2, TriState.True, TriState.False, TriState.False));
                                                XmlElement argoSelectNode1 = (XmlElement)oGrpElmt.LastChild;
                                                var optElmt = oOptXform.addOption(ref argoSelectNode1, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cShipOptName"], "-"), oRow["cShipOptCarrier"]), ": "), mcCurrencySymbol), Strings.FormatNumber(nShippingCost, 2))), Conversions.ToString(oRow["nShipOptKey"]));
                                                XmlElement optLabel = (XmlElement)optElmt.SelectSingleNode("label");
                                                optLabel.SetAttribute("name", Conversions.ToString(oRow["cShipOptName"]));
                                                optLabel.SetAttribute("carrier", Conversions.ToString(oRow["cShipOptCarrier"]));
                                                optLabel.SetAttribute("cost", Strings.FormatNumber(nShippingCost, 2));
                                                bFirstRow = false;
                                                nLastID = Conversions.ToInteger(oRow["nShipOptKey"]);
                                            }
                                        }

                                    }
                                }

                            }
                        }

                        ods = null;

                        if (Strings.LCase(moCartConfig["NotesOnOptions"]) == "on")
                        {

                            // Dim oNotesGrp As XmlElement = oOptXform.addGroup(oOptXform.moXformElmt, "notes", "term4051", "Please add any details for the delivery here")
                            string argsClass = "";
                            int argnRows = 0;
                            int argnCols = 0;
                            oOptXform.addTextArea(ref oGrpElmt, "tblCartOrder/cClientNotes/Notes/Notes", false, "Please add any details for the delivery here", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                            // oGrpElmt.AppendChild(oNotesGrp)

                        }



                        // Allow to Select Multiple Payment Methods or just one
                        XmlNode oPaymentCfg;

                        oPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        // more than one..

                        bool bPaymentTypeButtons = false;
                        if (Strings.LCase(moCartConfig["PaymentTypeButtons"]) == "on")
                            bPaymentTypeButtons = true;

                        bFirstRow = true;
                        if (oPaymentCfg != null)
                        {
                            if (nAmount == 0d & nRepeatAmount == 0d)
                            {
                                if (!bPaymentTypeButtons)
                                {
                                    oOptXform.Instance.SelectSingleNode("cPaymentMethod").InnerText = "No Charge";
                                    var oSelectElmt = oOptXform.addSelect1(ref oGrpElmt, "cPaymentMethod", false, "Payment Method", "radios multiline", Protean.xForm.ApperanceTypes.Full);
                                    oOptXform.addOption(ref oSelectElmt, "No Charge", "No Charge");
                                    bHidePayment = false;
                                    AllowedPaymentMethods.Add("No Charge");
                                }
                            }

                            else if (oPaymentCfg.SelectNodes("provider").Count > 1)
                            {

                                if (!bPaymentTypeButtons)
                                {
                                    XmlElement oSelectElmt;
                                    oSelectElmt = (XmlElement)oOptXform.moXformElmt.SelectSingleNode("descendant-or-self::select1[@ref='cPaymentMethod']");
                                    if (oSelectElmt is null)
                                    {
                                        oSelectElmt = oOptXform.addSelect1(ref oGrpElmt, "cPaymentMethod", false, "Payment Method", "radios multiline", Protean.xForm.ApperanceTypes.Full);
                                    }
                                    int nOptCount = oPay.getPaymentMethods(ref oOptXform, ref oSelectElmt, nAmount, ref mcPaymentMethod);

                                    // Code Moved to Get PaymentMethods

                                    if (nOptCount == 0)
                                    {
                                        oOptXform.valid = false;
                                        //XmlNode argoNode1 = oGrpElmt;
                                        oOptXform.addNote(ref oGrpElmt, Protean.xForm.noteTypes.Alert, "There is no method of payment available for your account - please contact the site administrator.");
                                        //oGrpElmt = (XmlElement)argoNode1;
                                    }
                                    else if (nOptCount == 1)
                                    {
                                        // hide the options
                                        oSelectElmt.SetAttribute("class", "hidden");

                                        // step throught the payment methods to set as allowed.
                                    }
                                    foreach (XmlElement oOptElmt in oSelectElmt.SelectNodes("item"))
                                        AllowedPaymentMethods.Add(oOptElmt.SelectSingleNode("value").InnerText);
                                }
                            }




                            else if (oPaymentCfg.SelectNodes("provider").Count == 1)
                            {
                                // or just one
                                if (!bPaymentTypeButtons)
                                {
                                    if (Convert.ToBoolean(oPay.HasRepeatPayments()))
                                    {
                                        var oSelectElmt = oOptXform.addSelect1(ref oGrpElmt, "cPaymentMethod", false, "Payment Method", "radios multiline", Protean.xForm.ApperanceTypes.Full);
                                        oPay.ReturnRepeatPayments(oPaymentCfg.SelectSingleNode("provider/@name").InnerText, ref oOptXform, ref oSelectElmt);

                                        oOptXform.addOption(ref oSelectElmt, oPaymentCfg.SelectSingleNode("provider/description").Attributes["value"].Value, oPaymentCfg.SelectSingleNode("provider").Attributes["name"].Value);
                                        bHidePayment = false;
                                        AllowedPaymentMethods.Add(oPaymentCfg.SelectSingleNode("provider/@name").InnerText);
                                    }
                                    else
                                    {
                                        bHidePayment = true;
                                        oOptXform.addInput(ref oGrpElmt, "cPaymentMethod", false, oPaymentCfg.SelectSingleNode("provider/@name").InnerText, "hidden");
                                        oOptXform.Instance.SelectSingleNode("cPaymentMethod").InnerText = oPaymentCfg.SelectSingleNode("provider/@name").InnerText;
                                        AllowedPaymentMethods.Add(oPaymentCfg.SelectSingleNode("provider/@name").InnerText);
                                    }
                                }
                            }
                            else
                            {
                                oOptXform.valid = false;
                                //XmlNode argoNode = oGrpElmt;
                                oOptXform.addNote(ref oGrpElmt, Protean.xForm.noteTypes.Alert, "There is no method of payment setup on this site - please contact the site administrator.");
                                //oGrpElmt = (XmlElement)argoNode;
                            }
                        }
                        else
                        {
                            oOptXform.valid = false;
                            //XmlNode argoNode2 = oGrpElmt;
                            oOptXform.addNote(ref oGrpElmt, Protean.xForm.noteTypes.Alert, "There is no method of payment setup on this site - please contact the site administrator.");
                            //oGrpElmt = (XmlElement)argoNode2;
                        }

                        string cTermsTitle = "Terms and Conditions";

                        // Adjust the group title
                        if (bAdjustTitle)
                        {
                            string cGroupTitle = "Select Delivery and Payment Option";
                            if (bHideDelivery & bHidePayment)
                                cGroupTitle = "Terms and Conditions";
                            if (bHideDelivery & !bHidePayment)
                                cGroupTitle = "Select Payment Option";
                            if (!bHideDelivery & bHidePayment)
                                cGroupTitle = "Select Shipping Option";
                            XmlElement labelElmt = (XmlElement)oGrpElmt.SelectSingleNode("label");
                            labelElmt.InnerText = cGroupTitle;
                            labelElmt.SetAttribute("class", "term3019");

                            // Just so we don't show the terms and conditions title twice

                            if (cGroupTitle == "Terms and Conditions")
                            {
                                cTermsTitle = "";
                            }
                        }

                        if (bAddTerms)
                        {

                            if (oGrpElmt.SelectSingleNode("*[@ref='terms']") is null)
                            {
                                string argsClass1 = "readonly terms-and-condiditons";
                                int argnRows1 = 0;
                                int argnCols1 = 0;
                                oOptXform.addTextArea(ref oGrpElmt, "terms", false, cTermsTitle, ref argsClass1, nRows: ref argnRows1, nCols: ref argnCols1);
                            }

                            if (oGrpElmt.SelectSingleNode("*[@ref='confirmterms']") is null)
                            {
                                oOptXform.addSelect(ref oGrpElmt, "confirmterms", false, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                                XmlElement argoSelectNode2 = (XmlElement)oGrpElmt.LastChild;
                                oOptXform.addOption(ref argoSelectNode2, "I agree to the Terms and Conditions", "Agree");
                            }

                            if (Conversions.ToInteger("0" + moCartConfig["TermsContentId"]) > 0)
                            {
                                var termsElmt = new XmlDocument();
                                termsElmt.LoadXml(moDBHelper.getContentBrief(Conversions.ToInteger(moCartConfig["TermsContentId"])));
                                mcTermsAndConditions = termsElmt.DocumentElement.InnerXml;
                            }
                            else
                            {
                                mcTermsAndConditions = moCartConfig["TermsAndConditions"];
                            }

                            if (mcTermsAndConditions is null)
                                mcTermsAndConditions = "";

                            oOptXform.Instance.SelectSingleNode("terms").InnerXml = mcTermsAndConditions;

                        }

                        oOptXform.addSubmit(ref oGrpElmt, "optionsForm", "Make Secure Payment");

                        if (bPaymentTypeButtons)
                        {
                            XmlElement xmlXfromGroup = (XmlElement)oOptXform.moXformElmt.SelectSingleNode("group");

                            // added by TS, if you need just the product amount without VAT or shipping we need to talk.
                            double totalAmount = Convert.ToDouble(cartElmt.GetAttribute("total"));
                            oPay.getPaymentMethodButtons(ref oOptXform, ref xmlXfromGroup, totalAmount);
                            
                            foreach (XmlElement oSubmitBtn in oOptXform.moXformElmt.SelectNodes("descendant-or-self::submit"))
                                AllowedPaymentMethods.Add(oSubmitBtn.GetAttribute("value"));

                            if (nAmount == 0d & nRepeatAmount == 0d)
                            {
                                // oOptXform.addSubmit(oGrpElmt, "optionsForm", "Complete Order")
                                AllowedPaymentMethods.Add("No Charge");
                                oOptXform.addSubmit(ref oGrpElmt, "No Charge", "Complete Order", "submit", "pay-button pay-nothing", "fas fa-check", "No Charge");

                            }

                        }
                    }

                    oOptXform.valid = false;

                    string submittedPaymentMethod = myWeb.moRequest["submit"];
                    if (submittedPaymentMethod == "Make Secure Payment")
                    {
                        submittedPaymentMethod = myWeb.moRequest["cPaymentMethod"];
                    }

                    if (AllowedPaymentMethods.Contains(submittedPaymentMethod)) // equates to is submitted
                    {

                        // Save notes to cart

                        if (Strings.LCase(moCartConfig["NotesOnOptions"]) == "on")
                        {
                            // If myWeb.moRequest("tblCartOrder/cClientNotes/Notes/Notes") <> "" Then
                            this.AddClientNotes(myWeb.moRequest["tblCartOrder/cClientNotes/Notes/Notes"]);
                            // End If
                        }

                        if (myWeb.moRequest["confirmterms"] == "Agree" | !bAddTerms)
                        {

                            mcPaymentMethod = submittedPaymentMethod;

                            // if we have a profile split it out, allows for more than one set of settings for each payment method, only done for SecPay right now.
                            if (Conversions.ToBoolean(Strings.InStr(mcPaymentMethod, "-")))
                            {
                                string[] aPayMth = Strings.Split(mcPaymentMethod, "-");
                                mcPaymentMethod = aPayMth[0];
                                mcPaymentProfile = aPayMth[1];
                            }

                            sSql2 = "select * from tblCartOrder where nCartOrderKey = " + mnCartId;
                            ods2 = moDBHelper.GetDataSet(sSql2, "Order", "Cart");
                            string cSqlUpdate;
                            foreach (DataRow oRow2 in ods2.Tables["Order"].Rows)
                            {
                                long nShipOptKey;

                                if (myWeb.moRequest["nShipOptKey"] != null)
                                {
                                    oRow2["nShippingMethodId"] = myWeb.moRequest["nShipOptKey"];
                                }
                                nShipOptKey = Conversions.ToLong(oRow2["nShippingMethodId"]);
                                sSql = "select * from tblCartShippingMethods ";
                                sSql = sSql + " where nShipOptKey = " + nShipOptKey;
                                ods = moDBHelper.GetDataSet(sSql, "Order", "Cart");

                                foreach (DataRow currentORow5 in ods.Tables["Order"].Rows)
                                {
                                    oRow = currentORow5;
                                    cShippingDesc = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cShipOptName"], "-"), oRow["cShipOptCarrier"]));
                                    nShippingCost = Conversions.ToDouble(Operators.ConcatenateObject("0", oRow["nShipOptCost"]));
                                    cSqlUpdate = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE tblCartOrder SET cShippingDesc='", SqlFmt(cShippingDesc)), "', nShippingCost="), SqlFmt(nShippingCost.ToString())), ", nShippingMethodId = "), nShipOptKey), " WHERE nCartOrderKey="), mnCartId));
                                    moDBHelper.ExeProcessSql(cSqlUpdate);
                                }

                                // update the cart xml

                                updateTotals(ref cartElmt, nAmount, nShippingCost, nShipOptKey.ToString());

                                ods2 = null;

                                if (bForceValidation)
                                {
                                    oOptXform.updateInstanceFromRequest();
                                    oOptXform.validate();
                                }
                                else
                                {
                                    oOptXform.valid = true;
                                }
                            }
                        }
                        else
                        {
                            oOptXform.addNote("confirmterms", Protean.xForm.noteTypes.Alert, "You must agree to the terms and conditions to proceed");
                        }
                    }

                    if (oOptXform.valid)
                    {
                        // If we have any order notes we save them
                        if (oOptXform.Instance.SelectSingleNode("Notes") != null)
                        {
                            // Open database for reading and writing
                            sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                            ods = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                            foreach (DataRow currentORow6 in ods.Tables["Order"].Rows)
                            {
                                oRow = currentORow6;
                                oRow["cClientNotes"] = oOptXform.Instance.SelectSingleNode("Notes").OuterXml;
                                moDBHelper.updateDataset(ref ods, "Order", true);
                            }
                            ods.Clear();
                            ods = null;
                        }
                    }
                    oOptXform.addValues();

                    return oOptXform;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "optionsXform", ex, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
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

                                arrLoc[0] = Conversions.ToString(Operators.ConcatenateObject(oDr["nLocationParId"], ""));

                                if (oDr["nLocationTaxRate"] is DBNull | !Information.IsNumeric(oDr["nLocationTaxRate"]))
                                {
                                    arrLoc[2] = 0.ToString();
                                }
                                else
                                {
                                    arrLoc[2] = Conversions.ToString(oDr["nLocationTaxRate"]);
                                }

                                if (oDr["cLocationNameShort"] is DBNull | oDr["cLocationNameShort"] == null)
                                {
                                    arrLoc[1] = Conversions.ToString(oDr["cLocationNameFull"]);
                                }
                                else
                                {
                                    arrLoc[1] = Conversions.ToString(oDr["cLocationNameShort"]);
                                }
                                nLocKey = Conversions.ToInteger(oDr["nLocationKey"]);
                                oLocations[nLocKey] = arrLoc;

                                arrLoc = null;

                                // if (Conversions.ToBoolean(Operators.OrObject(Operators.ConditionalCompareObjectEqual(Interaction.IIf((oDr["cLocationNameShort"]) is DBNull, "", (oDr["cLocationNameShort"])), Strings.LCase(Strings.Trim(sTarget)), false), Operators.ConditionalCompareObjectEqual(Interaction.IIf((oDr["cLocationNameFull"]) is DBNull, "", (oDr["cLocationNameFull"])), Strings.LCase(Strings.Trim(sTarget)), false))))
                                if (oDr["cLocationNameShort"].ToString() == Strings.Trim(sTarget) || oDr["cLocationNameFull"].ToString() == Strings.Trim(sTarget))
                                {
                                    nTargetId = Conversions.ToInteger(oDr["nLocationKey"]);
                                }
                            }

                            // Iterate through the country list
                            if (nTargetId != -1)
                            {
                                // Get country names
                                sCountryList = iterateCountryList(ref oLocations, ref nTargetId, ref nIndex);
                                sCountryList = "(" + Strings.Right(sCountryList, Strings.Len(sCountryList) - 1) + ")";
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
                        if (!(Information.IsDBNull(arrTmp[0]) | arrTmp[0] == null))
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

            public void addDateAndRef(ref XmlElement oCartElmt, DateTime invoiceDate = default, long nCartId = 0L)
            {
                myWeb.PerfMon.Log("Cart", "addDateAndRef");
                // adds current date and an invoice reference number to the cart object.
                // so the cart now contains all details needed for an invoice
                string cProcessInfo = "";
                if (nCartId == 0L)
                    nCartId = mnCartId;
                try
                {
                    if (invoiceDate == default)
                        invoiceDate = DateTime.Now;
                    if (nCartId == 0L)
                        nCartId = Conversions.ToLong(oCartElmt.GetAttribute("cartId"));
                    oCartElmt.SetAttribute("InvoiceDate", niceDate(invoiceDate));
                    oCartElmt.SetAttribute("InvoiceDateTime", XmlDate(invoiceDate, true));
                    oCartElmt.SetAttribute("InvoiceRef", OrderNoPrefix + nCartId.ToString());
                    if (!string.IsNullOrEmpty(mcVoucherNumber))
                    {
                        oCartElmt.SetAttribute("payableType", "Voucher");
                        oCartElmt.SetAttribute("voucherNumber", mcVoucherNumber);
                        oCartElmt.SetAttribute("voucherValue", mcVoucherValue);
                        oCartElmt.SetAttribute("voucherExpires", mcVoucherExpires);
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "addDateAndRef", ex, "", cProcessInfo, gbDebug);
                }

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
                    if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(myWeb.moSession["previousPage"], "", false)))
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
                        addNewTextNode("nShippingCost", ref argoNode14, Conversions.ToLong(moCartConfig["DefaultShippingCost"] + "0").ToString());
                        oElmt = (XmlElement)argoNode14;
                        XmlNode argoNode15 = oElmt;
                        addNewTextNode("cClientNotes", ref argoNode15, cOrderReference);
                        oElmt = (XmlElement)argoNode15;
                        XmlNode argoNode16 = oElmt;
                        addNewTextNode("cSellerNotes", ref argoNode16, Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("referer:", myWeb.moSession["previousPage"]), "/n")));
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

                        mnCartId = Conversions.ToInteger(moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartOrder, oInstance.DocumentElement));
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

            public void SetClientNotes(string Notes)
            {
                string sSql = "";
                DataSet oDs;
                string cProcessInfo = "SetClientNotes";
                try
                {
                    if (mnCartId > 0)
                    {
                        // Update Seller Notes:
                        sSql = "select * from tblCartOrder where nCartOrderKey = " + mnCartId;
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                            oRow["cClientNotes"] = Notes;
                        myWeb.moDbHelper.updateDataset(ref oDs, "Order");
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateSellerNotes", ex, "", cProcessInfo, gbDebug);
                }

            }

            public bool AddItem(long nProductId, long nQuantity, string[][] oProdOptions, string cProductText = "", double nPrice = 0d, string ProductXml = "", bool UniqueProduct = false, string overideUrl = "", bool bDepositOnly = false, string cProductOption = "", double dProductOptionPrice = 0d)
            {
                myWeb.PerfMon.Log("Cart", "AddItem");
                string cSQL = "Select * From tblCartItem WHERE nCartOrderID = " + mnCartId + " AND nItemiD =" + nProductId;
                var oDS = new DataSet();
                DataRow oDR1; // Parent Rows
                              // Child Rows
                int nItemID = 0; // ID of the cart item record
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
                        itemLimit = (short)Conversions.ToInteger(moCartConfig["ItemLimit"]);
                    }

                    if (nQuantity < itemLimit)
                    {

                        if (mnProcessId < 5)
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

                            // loop through the parent rows to check the product
                            if (oDS.Tables["CartItems"].Rows.Count > 0 & UniqueProduct == false)
                            {

                                foreach (DataRow currentODR1 in oDS.Tables["CartItems"].Rows)
                                {
                                    oDR1 = currentODR1;
                                    if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectEqual(moDBHelper.DBN2int(oDR1["nParentId"]), 0, false), Operators.ConditionalCompareObjectEqual(oDR1["nItemId"], nProductId, false)))) // (oDR1.Item("nParentId") = 0 Or IsDBNull(oDR1.Item("nParentId"))) And oDR1.Item("nItemId") = nProductId Then
                                    {
                                        nCountExOptions = 0;
                                        NoOptions = 0;
                                        // loop through the children(options) and count how many are the same
                                        foreach (var oDr2 in oDR1.GetChildRows("Rel1"))
                                        {
                                            var loopTo = Information.UBound(oProdOptions) - 1;
                                            for (i = 0; i <= loopTo; i++)
                                            {
                                                string cProdOpt1 = "";
                                                if (oProdOptions[i].Length > 1) {
                                                    cProdOpt1 = oProdOptions[i][1].ToString();
                                                }

                                                if (oProdOptions[i].Count() < 1)
                                                {
                                                    // Case for text option with no index
                                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oProdOptions[i][0], Conversions.ToString(oDr2["nItemOptGrpIdx"]), false)))
                                                        nCountExOptions += 1;
                                                }
                                                else if ((oProdOptions[i][0].ToString() != oDr2["nItemOptGrpIdx"].ToString()) && (cProdOpt1.ToString() != oDr2["nItemOptIdx"].ToString()))
                                                    nCountExOptions += 1;
                                            }
                                            NoOptions += 1;
                                        }
                                        if (oProdOptions != null)
                                        {
                                            // if they are all the same then we have the correct record so it is an update
                                            if (nCountExOptions == Information.UBound(oProdOptions) & NoOptions == Information.UBound(oProdOptions))
                                            {
                                                nItemID = Conversions.ToInteger(oDR1["NCartItemKey"]); // ok, got the bugger
                                                break; // exit the loop other wise we might go through some other ones
                                            }
                                        }

                                        else if (NoOptions == 0)
                                            nItemID = Conversions.ToInteger(oDR1["NCartItemKey"]);
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
                                        addNewTextNode("nDiscountValue", ref oElmt, Conversions.ToString(Interaction.IIf(Information.IsNumeric(strDiscount1), strDiscount1, 0)));
                                    }

                                    if (oProdXml.SelectSingleNode("/Content/ShippingWeight") != null)
                                    {
                                        nWeight = (long)Math.Round(Conversions.ToDouble("0" + oProdXml.SelectSingleNode("/Content/ShippingWeight").InnerText));
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

                                        long nParentId = Conversions.ToLong(moDBHelper.ExeProcessSqlScalar(sSQL2));
                                        XmlNode argoNode7 = oProdXml.DocumentElement;
                                        var ItemParent = addNewTextNode("ParentProduct", ref argoNode7, "");

                                        ItemParent.InnerXml = moDBHelper.GetContentDetailXml(nParentId).OuterXml;
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
                                    if (Conversions.ToDouble(myWeb.moRequest["price_" + nProductId]) > 0d)
                                    {
                                        strPrice1 = myWeb.moRequest["price_" + nProductId];
                                    }
                                }                              
                                addNewTextNode("nPrice", ref oElmt, Conversions.ToString(Interaction.IIf(Information.IsNumeric(strPrice1), strPrice1, 0)));
                                addNewTextNode("nShpCat", ref oElmt, (-1).ToString());
                                addNewTextNode("nTaxRate", ref oElmt, nTaxRate.ToString());
                                addNewTextNode("nQuantity", ref oElmt, nQuantity.ToString());
                                addNewTextNode("nWeight", ref oElmt, nWeight.ToString());
                                addNewTextNode("nParentId", ref oElmt, 0.ToString());
                                if (bDepositOnly)
                                {
                                    XmlNode argoNode18 = oElmt;
                                    addNewTextNode("nDepositAmount", ref argoNode18, Conversions.ToString(Interaction.IIf(Information.IsNumeric(oPrice.GetAttribute("deposit")), oPrice.GetAttribute("deposit"), 0)));
                                    oElmt = (XmlElement)argoNode18;
                                }

                                XmlNode argoNode19 = oElmt;
                                var ProductXmlElmt = addNewTextNode("xItemXml", ref argoNode19, "");
                                oElmt = (XmlElement)argoNode19;
                                ProductXmlElmt.InnerXml = oProdXml.DocumentElement.OuterXml;

                                nItemID = Conversions.ToInteger(moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement));

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
                                            else {
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
                                            else if (Information.IsNumeric(oProdOptions[i][0]) & Information.IsNumeric(opt2ndval))
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
                                                addNewTextNode("nItemOptGrpIdx", ref oElmt, Conversions.ToString(oProdOptions[i][0]));
                                                addNewTextNode("nItemOptIdx", ref oElmt, Conversions.ToString(opt2ndval));
                                                XmlElement oPriceElmt = (XmlElement)oProdXml.SelectSingleNode($"/Content/Options/OptGroup[{oProdOptions[i][0]}]/option[{opt2ndval}]/Prices/Price[@currency='{mcCurrency}']");
                                                string strPrice2 = 0.ToString();
                                                if (oPriceElmt != null)
                                                    strPrice2 = oPriceElmt.InnerText;
                                                addNewTextNode("nPrice", ref oElmt, Conversions.ToString(Interaction.IIf(Information.IsNumeric(strPrice2), strPrice2, 0)));
                                            }                                           
                                            addNewTextNode("nShpCat", ref oElmt, (-1).ToString());                                                                               
                                            addNewTextNode("nTaxRate", ref oElmt, 0.ToString());                                       
                                            addNewTextNode("nQuantity", ref oElmt, 1.ToString());                                
                                            addNewTextNode("nWeight", ref oElmt, 0.ToString());                                   
                                            addNewTextNode("nParentId", ref oElmt, nItemID.ToString());                         
                                            moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement);
                                        }
                                    }
                                }
                                // 
                                if (myWeb.moRequest["OptionName_" + nProductId] != null)
                                {
                                    this.AddProductOption(nItemID, myWeb.moRequest["OptionName_" + nProductId], Conversions.ToDouble(myWeb.moRequest["OptionValue_" + nProductId]));
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
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDR1["nCartItemKey"], nItemID, false)))
                                        {
                                            oDR1.BeginEdit();
                                            if (Strings.LCase(moCartConfig["OverwriteItemQuantity"]) == "on")
                                            {
                                                oDR1["nQuantity"] = nQuantity;
                                            }
                                            else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectLess(Operators.AddObject(oDR1["nQuantity"], nQuantity), itemLimit, false)))
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
                    if (Strings.LCase(moCartConfig["ClearOnAdd"]) == "on")
                    {
                        cSql = "select nCartItemKey from tblCartItem where nCartOrderId = " + mnCartId;
                        oDs = moDBHelper.GetDataSet(cSql, "Item");
                        if (oDs.Tables["Item"].Rows.Count > 0)
                        {
                            foreach (DataRow oRow in oDs.Tables["Item"].Rows)
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, Conversions.ToLong(oRow["nCartItemKey"]));
                        }
                    }
                    if ((Strings.LCase(mmcOrderType) ?? "") == (Strings.LCase(mcItemOrderType) ?? "")) // test for order?
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
                            if (Strings.InStr(Conversions.ToString(oItem1), "qty_") == 1) // check for getting productID and quantity (since there will only be one of these per item submitted)
                            {

                                if (Strings.InStr(Conversions.ToString(oItem1), "qty_deposit_") == 1)
                                {
                                    cProductKey = Strings.Replace(Conversions.ToString(oItem1), "qty_deposit_", "");
                                    if (Information.IsNumeric(cProductKey))
                                    {
                                        nProductKey = Conversions.ToLong(cProductKey);
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
                                    cProductKey = Strings.Replace(Conversions.ToString(oItem1), "qty_", "");
                                    if (Information.IsNumeric(cProductKey))
                                    {
                                        nProductKey = Conversions.ToLong(cProductKey);
                                    }
                                    else
                                    {
                                        // injection attempt don't add to cart
                                        //return false;
                                        return AddItemsRet;
                                    }
                                }

                                cProcessInfo = Conversions.ToString(Operators.ConcatenateObject(oItem1.ToString() + " = ", myWeb.moRequest.Form.Get(oItem1)));

                                if (Information.IsNumeric(myWeb.moRequest.Form.Get(oItem1)))
                                {
                                    nQuantity = Conversions.ToLong(myWeb.moRequest.Form.Get(oItem1));
                                }

                                // replacementName
                                if (nQuantity > 0L)
                                {
                                    qtyAdded = (int)(qtyAdded + nQuantity);
                                    // bool bBlockCartAdd = false;
                                    string sBlockCartAddMsg = string.Empty;
                                    if (moSubscription != null)
                                    {
                                        if (Strings.LCase(moCartConfig["SubsExclusiveOrder"]) == "on")
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
                                    if (!(Strings.InStr(strAddedProducts, "'" + nProductKey + "'") > 0)) // double check we havent added this product (dont really need but good just in case)
                                    {
                                        foreach (string oItem2 in myWeb.moRequest.Form) // loop through again checking for options
                                        {
                                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oItem2, "replacementName_" + nProductKey, false)))
                                                cReplacementName = Conversions.ToString(myWeb.moRequest.Form.Get(oItem2));
                                            if (Strings.InStr(Conversions.ToString(oItem2), "_") > 0)
                                            {
                                                if ((Strings.Split(Conversions.ToString(oItem2), "_")[0] + "_" + Strings.Split(Conversions.ToString(oItem2), "_")[1] ?? "") == ("opt_" + nProductKey ?? "")) // check it is an option
                                                {
                                                    oCurOpt = Strings.Split(Conversions.ToString(myWeb.moRequest.Form.Get(oItem2)), ","); // get array of option in "1_2" format
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
                                            if (Information.IsNumeric(myWeb.moRequest.Form.Get("donationAmount")))
                                            {
                                                string CartItemName = "Donation";
                                                if (!string.IsNullOrEmpty(myWeb.moRequest.Form.Get("donationName")))
                                                {
                                                    CartItemName = myWeb.moRequest.Form.Get("donationName");
                                                }
                                                if (!AddItem(nProductKey, nQuantity, oOptions, CartItemName, Conversions.ToDouble(myWeb.moRequest.Form.Get("donationAmount"))))
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
                if (mnProcessId > 4)
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
                    if (Information.IsNumeric(myWeb.moRequest["id"]))
                        nItemId = Conversions.ToLong(myWeb.moRequest["id"]);
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
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, Conversions.ToLong(oRow["nCartItemKey"]));
                        }


                        // REturn the cart order item count
                        sSql = "select count(*) As ItemCount from tblCartItem where nCartOrderId = " + mnCartId;
                        // oDr = moDBHelper.getDataReader(sSql)
                        using (var oDr = moDBHelper.getDataReaderDisposable(sSql))
                        {
                            if (oDr.HasRows)
                            {
                                while (oDr.Read())
                                    itemCount = Conversions.ToInteger(oDr["ItemCount"]);
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

                    if (Strings.LCase(moCartConfig["ClearOnAdd"]) == "on")
                    {
                        string cSql = "select nCartItemKey from tblCartItem where nCartOrderId = " + mnCartId;
                        oDs = moDBHelper.GetDataSet(cSql, "Item");
                        if (oDs.Tables["Item"].Rows.Count > 0)
                        {
                            foreach (DataRow currentORow in oDs.Tables["Item"].Rows)
                            {
                                oRow = currentORow;
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, Conversions.ToLong(oRow["nCartItemKey"]));
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
                                itemCount = Conversions.ToInteger(oDr["ItemCount"]);
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

                            oContentXml.LoadXml(Conversions.ToString(oRow["xItemXml"]));
                            XmlElement oRootElmt = (XmlElement)oContentXml.FirstChild;
                            oRootElmt.SetAttribute("overridePrice", "true");

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
                                moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, Conversions.ToLong(oDr["nCartItemKey"]));
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
                                        nAmountReceived = Conversions.ToDouble(Operators.ConcatenateObject("0", oDr["nAmountReceived"]));
                                        cUniqueLink = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(", cSettlementID='OLD_", oDr["cSettlementID"]), "' "));
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
                    mnTaxRate = Conversions.ToDouble(moCartConfig["TaxRate"]);

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
                    mnTaxRate = Conversions.ToDouble(moCartConfig["TaxRate"]);
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
                            bNullParentId = false;
                            if (ReferenceEquals(oRow["nParentId"], DBNull.Value))
                            {
                                bNullParentId = true;
                            }
                            else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oRow["nParentId"], 0, false)))
                            {
                                bNullParentId = true;
                            }
                            if (bNullParentId) // for options
                            {
                                nItemCount = nItemCount + 1;
                                // First check if the quantity is numeric (if not ignore it)
                                if (Information.IsNumeric(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject("itemId-", oRow["nCartItemKey"]))]))
                                {
                                    // It's numeric - let's see if it's positive (i.e. update it, if not delete it)
                                    if (Conversions.ToInteger(myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject("itemId-", oRow["nCartItemKey"]))]) > 0)
                                    {
                                        oRow["nQuantity"] = myWeb.moRequest[Conversions.ToString(Operators.ConcatenateObject("itemId-", oRow["nCartItemKey"]))];
                                    }
                                    else
                                    {
                                        // delete options first
                                        DataRow[] oCRows = oRow.GetChildRows("Rel1");
                                        int nDels;
                                        var loopTo = Information.UBound(oCRows);
                                        for (nDels = 0; nDels <= loopTo; nDels++)
                                            oCRows[nDels].Delete();
                                        // end delete options
                                        oRow.Delete();
                                        nItemCount = nItemCount - 1;
                                    }
                                }
                            } // for options
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
                            moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.Audit, nKey: Conversions.ToLong(oRow["nAuditId"]));

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
                            if (!Conversions.ToString(oRow["cSellerNotes"]).Contains("Referrer: " + myWeb.Referrer) & !string.IsNullOrEmpty(myWeb.Referrer))
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

            public void ListOrders(ref XmlElement oContentsXML, cartProcess ProcessId)
            {
                myWeb.PerfMon.Log("Cart", "ListOrders");
                XmlElement oRoot;
                XmlElement oElmt;
                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    oRoot = moPageXml.CreateElement("Content");
                    oRoot.SetAttribute("type", "listTree");
                    oRoot.SetAttribute("template", "default");
                    oRoot.SetAttribute("name", "Orders - " + ProcessId.GetType().ToString());

                    sSql = "SELECT nCartOrderKey as id, c.cContactName as name, c.cContactEmail as email, a.dUpdateDate from tblCartOrder inner join tblAudit a on nAuditId = a.nAuditKey left outer join tblCartContact c on (nCartOrderKey = c.nContactCartId and cContactType = 'Billing Address') where nCartStatus = " + ((int)ProcessId).ToString();

                    oDs = moDBHelper.GetDataSet(sSql, "Order", "List");

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;

                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("List");
                        oElmt.InnerXml = oDs.GetXml();

                        oContentsXML.AppendChild(oElmt);
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListShippingLocations", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void ListShippingLocations(ref XmlElement oContentsXML, long OptId = 0L)
            {
                myWeb.PerfMon.Log("Cart", "ListShippingLocations");
                XmlElement oRoot;
                XmlElement oElmt;
                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    oRoot = moPageXml.CreateElement("Content");
                    oRoot.SetAttribute("type", "listTree");
                    oRoot.SetAttribute("template", "default");
                    if (OptId != 0L)
                    {
                        oRoot.SetAttribute("name", "Shipping Locations Form");
                        sSql = "SELECT nLocationKey as id, nLocationType as type, nLocationParId as parid, cLocationNameFull as Name, cLocationNameShort as nameShort, (SELECT COUNT(*) from tblCartShippingRelations r where r.nShpLocId = n.nLocationKey and r.nShpOptId = " + OptId + ") As selected from tblCartShippingLocations n ";
                    }
                    else
                    {
                        oRoot.SetAttribute("name", "Shipping Locations");
                        sSql = "SELECT nLocationKey as id, nLocationType as type, nLocationParId as parid, cLocationNameFull as Name, cLocationNameShort as nameShort, (SELECT COUNT(*) from tblCartShippingRelations r where r.nShpLocId = n.nLocationKey) As nOptCount from tblCartShippingLocations n ";
                    }

                    // NOTE : This SQL is NOT the same as the equivalent function in the EonicWeb component.
                    // It adds a count of shipping option relations for each location

                    oDs = moDBHelper.GetDataSet(sSql, "TreeItem", "Tree");

                    oDs.Relations.Add("rel01", oDs.Tables[0].Columns["id"], oDs.Tables[0].Columns["parId"], false);
                    oDs.Relations["rel01"].Nested = true;

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Hidden;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[5].ColumnMapping = MappingType.Attribute;

                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("Tree");
                        oElmt.InnerXml = oDs.GetXml();


                        XmlElement oCheckElmt = null;
                        oCheckElmt = (XmlElement)oElmt.SelectSingleNode("descendant-or-self::TreeItem[@id='" + mnShippingRootId + "']");
                        if (oCheckElmt != null)
                            oElmt = oCheckElmt;

                        oContentsXML.AppendChild(oElmt);
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListShippingLocations", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void ListDeliveryMethods(ref XmlElement oContentsXML)
            {
                myWeb.PerfMon.Log("Cart", "ListDeliveryMethods");
                XmlElement oElmt;
                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    sSql = "select a.nStatus as status, nShipOptKey as id, cShipOptName as name, cShipOptCarrier as carrier, a.dPublishDate as startDate, a.dExpireDate as endDate, tblCartShippingMethods.cCurrency from tblCartShippingMethods left join tblAudit a on a.nAuditKey = nAuditId order by nDisplayPriority";
                    oDs = moDBHelper.GetDataSet(sSql, "ListItem", "List");

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[2].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[3].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[5].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[6].ColumnMapping = MappingType.Attribute;
                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("List");
                        oElmt.InnerXml = oDs.GetXml();

                        oContentsXML.AppendChild(oElmt.FirstChild);
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListDeliveryMethods", ex, "", cProcessInfo, gbDebug);
                }
            }


            public void ListCarriers(ref XmlElement oContentsXML)
            {
                myWeb.PerfMon.Log("Cart", "ListDeliveryMethods");
                XmlElement oElmt;
                string sSql;
                DataSet oDs;
                string cProcessInfo = "";
                try
                {

                    sSql = "select a.nStatus as status, nCarrierKey as id, cCarrierName as name, cCarrierTrackingInstructions as info, a.dPublishDate as startDate, a.dExpireDate as endDate from tblCartCarrier left join tblAudit a on a.nAuditKey = nAuditId";
                    oDs = moDBHelper.GetDataSet(sSql, "Carrier", "Carriers");

                    if (oDs.Tables[0].Rows.Count > 0)
                    {
                        oDs.Tables[0].Columns[0].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[1].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[4].ColumnMapping = MappingType.Attribute;
                        oDs.Tables[0].Columns[5].ColumnMapping = MappingType.Attribute;
                        // load existing data into the instance
                        oElmt = moPageXml.CreateElement("Carriers");
                        oElmt.InnerXml = oDs.GetXml();

                        oContentsXML.AppendChild(oElmt.FirstChild);
                    }
                }


                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListCarriers", ex, "", cProcessInfo, gbDebug);
                }
            }

            public void ListPaymentProviders(ref XmlElement oContentsXML)
            {
                myWeb.PerfMon.Log("Cart", "ListPaymentProviders");
                XmlElement oElmt;
                XmlElement oElmt2;
                string cProcessInfo = "";
                XmlNode oPaymentCfg;
                string ptnFolder = "/ewcommon/xforms/PaymentProvider/";
                string localFolder = "/xforms/PaymentProvider/";
                FileInfo fi;
                string ProviderName;
                if (myWeb.bs5)
                {
                    ptnFolder = "/ptn/providers/payment/";
                    localFolder = "/providers/payment/";
                }
                try
                {

                    oPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                    oElmt = moPageXml.CreateElement("List");


                    if (myWeb.bs5)
                    {
                        var dir = new DirectoryInfo(moServer.MapPath(ptnFolder));
                        if (dir.Exists)
                        {
                            DirectoryInfo[] dirs;
                            dirs = dir.GetDirectories();
                            foreach (var dir2 in dirs)
                            {
                                ProviderName = dir2.Name;
                                XmlNode argoNode = oElmt;
                                oElmt2 = Protean.Tools.Xml.addNewTextNode("Provider", ref argoNode, Strings.Replace(ProviderName, "-", " "));
                                oElmt = (XmlElement)argoNode;
                                if (oPaymentCfg.SelectSingleNode("/payment/provider[@name='" + Strings.Replace(ProviderName, "-", "") + "']") != null)
                                {
                                    oElmt2.SetAttribute("active", "true");
                                }
                            }
                        }
                        dir = new DirectoryInfo(moServer.MapPath(localFolder));
                        if (dir.Exists)
                        {
                            DirectoryInfo[] dirs;
                            dirs = dir.GetDirectories();
                            foreach (var dir2 in dirs)
                            {
                                ProviderName = dir2.Name;
                                XmlNode argoNode1 = oElmt;
                                oElmt2 = Protean.Tools.Xml.addNewTextNode("Provider", ref argoNode1, Strings.Replace(ProviderName, "-", " "));
                                oElmt = (XmlElement)argoNode1;
                                if (oPaymentCfg.SelectSingleNode("/payment/provider[@name='" + Strings.Replace(ProviderName, "-", "") + "']") != null)
                                {
                                    oElmt2.SetAttribute("active", "true");
                                }
                            }
                        }
                    }
                    else
                    {
                        var dir = new DirectoryInfo(moServer.MapPath(ptnFolder));
                        FileInfo[] files = dir.GetFiles();
                        foreach (var currentFi in files)
                        {
                            fi = currentFi;
                            if (fi.Extension == ".xml")
                            {
                                ProviderName = Strings.Replace(fi.Name, fi.Extension, "");
                                XmlNode argoNode2 = oElmt;
                                oElmt2 = Protean.Tools.Xml.addNewTextNode("Provider", ref argoNode2, Strings.Replace(ProviderName, "-", " "));
                                oElmt = (XmlElement)argoNode2;
                                if (oPaymentCfg.SelectSingleNode("/payment/provider[@name='" + Strings.Replace(ProviderName, "-", "") + "']") != null)
                                {
                                    oElmt2.SetAttribute("active", "true");
                                }
                            }
                        }
                        dir = new DirectoryInfo(moServer.MapPath(localFolder));
                        if (dir.Exists)
                        {
                            files = dir.GetFiles();
                            foreach (var currentFi1 in files)
                            {
                                fi = currentFi1;
                                if (fi.Extension == ".xml")
                                {
                                    ProviderName = Strings.Replace(fi.Name, fi.Extension, "");
                                    XmlNode argoNode3 = oElmt;
                                    oElmt2 = Protean.Tools.Xml.addNewTextNode("Provider", ref argoNode3, Strings.Replace(ProviderName, "-", " "));
                                    oElmt = (XmlElement)argoNode3;
                                    if (oPaymentCfg.SelectSingleNode("/payment/provider[@name='" + Strings.Replace(ProviderName, "-", "") + "']") != null)
                                    {
                                        oElmt2.SetAttribute("active", "true");
                                    }
                                }
                            }
                        }
                    }


                    oContentsXML.AppendChild(oElmt);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListPaymentProviders", ex, "", cProcessInfo, gbDebug);
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
                    mnGiftListId = Conversions.ToInteger(nGiftListId);

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

                    if (Information.IsNumeric(cVatExclusionGroup) && Conversions.ToInteger(cVatExclusionGroup) > 0 && Conversions.ToBoolean(localNodeState()))

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

                            sCountryList = Strings.Mid(sCountryList, 3, Strings.Len(sCountryList) - 4);

                            aVatRates = Strings.Split(sCountryList, "','");
                            Array.Reverse(aVatRates);

                            // go backwards through the list, and use the last non-zero tax rate
                            bAllZero = true;

                            foreach (var cVatRate in aVatRates)
                            {
                                if (Conversions.ToDouble(cVatRate) > 0d)
                                {
                                    nUpdateTaxRate = Conversions.ToDouble(cVatRate);
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
                                nUpdateTaxRate = Conversions.ToDouble(moDBHelper.ExeProcessSqlScalar(sSql));
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
                string[] arrPreLocs = Strings.Split(mcPriorityCountries, ",");
                int arrIdx;
                var bPreSelect = default(bool);
                string cProcessInfo = Conversions.ToString(string.IsNullOrEmpty(oCountriesDropDown.OuterXml));
                try
                {

                    switch (cAddressType ?? "")
                    {
                        case "Delivery Address":
                            {
                                // Delivery countries are restricted
                                // Go and build a tree of all locations - this will allow us to detect whether or not a country is in iteself or in a zone that has a Shipping Option
                                oLoctree = moPageXml.CreateElement("Contents");
                                ListShippingLocations(ref oLoctree, Conversions.ToLong(false));

                                // Add any priority countries
                                var loopTo = Information.UBound(arrPreLocs);
                                for (arrIdx = 0; arrIdx <= loopTo; arrIdx++)
                                {
                                    oLocation = (XmlElement)oLoctree.SelectSingleNode("//TreeItem[@nameShort='" + arrPreLocs[arrIdx] + "']/ancestor-or-self::*[@nOptCount!='0']");
                                    if (oLocation != null)
                                    {
                                        oXform.addOption(ref oCountriesDropDown, Strings.Trim(arrPreLocs[arrIdx]), Strings.Trim(arrPreLocs[arrIdx]));
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

                                        if (oLoctree.SelectSingleNode(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("//TreeItem[@nameShort=\"", oDr["cLocationNameShort"]), "\"]/ancestor-or-self::*[@nOptCount!='0']"))) != null)
                                        {
                                            oXform.addOption(ref oCountriesDropDown, Conversions.ToString(oDr["cLocationNameShort"]), Conversions.ToString(oDr["cLocationNameShort"]));
                                        }
                                    }

                                    oLoctree = null;
                                    oLocation = null;
                                }

                                break;
                            }

                        default:
                            {
                                // Not restricted by delivery address - add all countries.
                                var loopTo1 = Information.UBound(arrPreLocs);
                                for (arrIdx = 0; arrIdx <= loopTo1; arrIdx++)
                                {
                                    oXform.addOption(ref oCountriesDropDown, Strings.Trim(arrPreLocs[arrIdx]), Strings.Trim(arrPreLocs[arrIdx]));
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
                                    var argoDr = oDr;
                                    oXform.addOptionsFromSqlDataReader(ref oCountriesDropDown, ref argoDr);
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

            public object emailCart(ref XmlElement oCartXML, string xsltPath, string fromName, string fromEmail, string recipientEmail, string SubjectLine, bool bEncrypt = false, string cAttachementTemplatePath = "", string cBCCEmail = "", string cCCEmail = "")
            {
                myWeb.PerfMon.Log("Cart", "emailCart");
                var oXml = new XmlDocument();
                string cProcessInfo = "emailCart";
                try
                {
                    // check file path

                    var ofs = new Protean.fsHelper();

                    oXml.LoadXml(oCartXML.OuterXml);
                    xsltPath = ofs.checkCommonFilePath(moConfig["ProjectPath"] + xsltPath);

                    oCartXML.SetAttribute("lang", myWeb.mcPageLanguage);

                    var oMsg = new Protean.Messaging(ref myWeb.msException);
                    if (string.IsNullOrEmpty(cAttachementTemplatePath))
                    {

                        Cms.dbHelper argodbHelper = null;
                        cProcessInfo = Conversions.ToString(oMsg.emailer(oCartXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, odbHelper: ref argodbHelper, "Message Sent", "Message Failed", ccRecipient: cCCEmail, bccRecipient: cBCCEmail));
                    }
                    else
                    {
                        cAttachementTemplatePath = moServer.MapPath(cAttachementTemplatePath);
                        string cFontPath = moServer.MapPath("/fonts");
                        var oPDF = new Tools.PDF();

                        // create the xmlFO document
                        Protean.XmlHelper.Transform oTransform;

                        string styleFile = cAttachementTemplatePath;
                        myWeb.PerfMon.Log("Web", "ReturnPageHTML - loaded Style");
                        oTransform = new Protean.XmlHelper.Transform(ref myWeb, styleFile, false);

                        myWeb.msException = "";

                        oTransform.mbDebug = gbDebug;
                        TextWriter oTW = new StringWriter();
                        XmlWriter icXmlWriter = XmlWriter.Create(oTW);
                        var OrderDoc = new XmlDocument();
                        OrderDoc.LoadXml(oCartXML.OuterXml);

                        XmlReader oXMLReaderInstance = new XmlNodeReader(oCartXML);

                        oTransform.ProcessTimed(oXMLReaderInstance, ref icXmlWriter);
                        OrderDoc = null;

                        string foNetXml = oTW.ToString();
                        string FileName = "Attachment.pdf";

                        var FoDoc = new XmlDocument();
                        FoDoc.LoadXml(foNetXml);
                        var nsMgr = new XmlNamespaceManager(FoDoc.NameTable);
                        nsMgr.AddNamespace("fo", "http://www.w3.org/1999/XSL/Format");
                        if (FoDoc.DocumentElement.SelectSingleNode("descendant::fo:title", nsMgr) != null)
                        {
                            FileName = FoDoc.DocumentElement.SelectSingleNode("descendant::fo:title", nsMgr).InnerText.Replace(" ", "-") + ".pdf";
                        }
                        FoDoc = null;

                        oMsg.addAttachment(oPDF.GetPDFstream(foNetXml, cFontPath), FileName);
                        Cms.dbHelper argodbHelper1 = null;
                        cProcessInfo = Conversions.ToString(oMsg.emailer(oCartXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, odbHelper: ref argodbHelper1, "Message Sent", "Message Failed", ccRecipient: cCCEmail, bccRecipient: cBCCEmail));

                    }
                    oMsg = (Protean.Messaging)null;

                    return cProcessInfo;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "emailCart", ex, "", cProcessInfo, gbDebug);
                    return null;
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
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oItem, "node", false)))
                        {
                            // this is the basic node text
                            cNotes = Conversions.ToString(myWeb.moRequest.Form.Get(oItem));
                        }
                        else
                        {
                            // this is the rest of the submitted form data
                            // going to be saved as attributes
                            Array.Resize(ref oAttribs, Information.UBound(oAttribs) + 1 + 1);
                            oAttribs[Information.UBound(oAttribs) - 1] = new FormResult(Conversions.ToString(oItem), Conversions.ToString(myWeb.moRequest.Form.Get(oItem)));
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

                    var loopTo = Information.UBound(oAttribs) - 1;
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
                                nQuantity = Conversions.ToInteger(oAttribs[i].Value);
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
                                if (Information.IsNumeric(cVAs))
                                {
                                    nTotalVA = (int)Math.Round(Conversions.ToDecimal(oTmpElements.GetAttribute("qty")) * Conversions.ToDecimal(cVAs));
                                    if (nTotalVA > 0)
                                        oTmpElements.SetAttribute("TotalVA", nTotalVA.ToString());
                                }
                                goto SaveNotes;
                            }
                            else if (!(nQuantity == -1))
                            {
                                // its an Add
                                oTmpElements.SetAttribute("qty", (Conversions.ToInteger(oTmpElements.GetAttribute("qty")) + nQuantity).ToString());
                                // Complete Bodge for keysource
                                string cVAs = myWeb.moRequest.Form["VA"];
                                if (cVAs is null | string.IsNullOrEmpty(cVAs))
                                    cVAs = myWeb.moRequest.Form["Custom_VARating"];
                                int nTotalVA = 0;
                                if (Information.IsNumeric(cVAs))
                                {
                                    nTotalVA = (int)Math.Round(Conversions.ToDecimal(oTmpElements.GetAttribute("qty")) * Conversions.ToDecimal(cVAs));
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
                    if (Information.IsNumeric(cVAsN))
                    {
                        nTotalVAN = (int)Math.Round(nQuantity * Conversions.ToDecimal(cVAsN));
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

            public void ListOrders(string sOrderID, bool bListAllQuotes, int ProcessId, ref XmlElement oPageDetail, bool bForceRefresh = false, long nUserId = 0L)
            {
                myWeb.PerfMon.Log("Cart", "ListOrders");
                if (myWeb.mnUserId == 0)
                    return; // if not logged in, dont bother
                            // For listing a users previous orders/quotes

                var oDs = new DataSet();
                string cSQL;
                string cWhereSQL = "";

                // Paging variables
                int nStart = 0;
                int nRows = 100;

                int nCurrentRow = 0;
                XmlElement moPaymentCfg = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/payment");

                try
                {

                    // Set the paging variables, if provided.
                    if (myWeb.moRequest["startPos"] != null && Information.IsNumeric(myWeb.moRequest["startPos"]))
                        nStart = Conversions.ToInteger(myWeb.moRequest["startPos"]);
                    if (myWeb.moRequest["rows"] != null && Information.IsNumeric(myWeb.moRequest["rows"]))
                        nRows = Conversions.ToInteger(myWeb.moRequest["rows"]);

                    if (nStart < 0)
                        nStart = 0;
                    if (nRows < 1)
                        nRows = 100;

                    if (!(nUserId == 0L))
                    {
                        cWhereSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" WHERE nCartUserDirId = " + nUserId, Interaction.IIf(sOrderID != "0", " AND nCartOrderKey IN (" + sOrderID + ")", "")), " AND cCartSchemaName = '"), mcOrderType), "'"));
                    }
                    else if (!myWeb.mbAdminMode)
                    {
                        cWhereSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" WHERE nCartUserDirId = " + myWeb.mnUserId, Interaction.IIf(sOrderID != "0", " AND nCartOrderKey IN (" + sOrderID + ")", "")), " AND cCartSchemaName = '"), mcOrderType), "'"));
                    }
                    else
                    {
                        cWhereSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" WHERE ", Interaction.IIf(sOrderID != "0", "  nCartOrderKey IN (" + sOrderID + ") AND ", "")), " cCartSchemaName = '"), mcOrderType), "' "));
                        // if nCartStatus = " & ProcessId
                        if (!(ProcessId == 0))
                        {
                            cWhereSQL += " and nCartStatus = " + ProcessId;
                        }
                    }

                    // Quick call to get the total number of records
                    cSQL = "SELECT COUNT(*) As Count FROM tblCartOrder " + cWhereSQL;
                    long nTotal = Conversions.ToLong(moDBHelper.GetDataValue(cSQL));

                    if (nTotal > 0L)
                    {

                        // Initial paging option is limit the the rows returned
                        cSQL = "SELECT TOP " + (nStart + nRows) + " * FROM tblCartOrder ";
                        cSQL += cWhereSQL + " ORDER BY nCartOrderKey Desc";

                        oDs = moDBHelper.GetDataSet(cSQL, mcOrderType, mcOrderType + "List");

                        if (oDs.Tables.Count > 0)
                        {


                            XmlElement oContentDetails;
                            // Get the content Detail element
                            if (oPageDetail is null)
                            {
                                oContentDetails = (XmlElement)moPageXml.SelectSingleNode("Page/ContentDetail");
                                if (oContentDetails is null)
                                {
                                    oContentDetails = moPageXml.CreateElement("ContentDetail");
                                    if (!string.IsNullOrEmpty(moPageXml.InnerXml))
                                    {
                                        moPageXml.FirstChild.AppendChild(oContentDetails);
                                    }
                                    else
                                    {
                                        oPageDetail.AppendChild(oContentDetails);
                                    }

                                }
                            }
                            else
                            {
                                oContentDetails = oPageDetail;
                            }

                            oContentDetails.SetAttribute("start", nStart.ToString());
                            oContentDetails.SetAttribute("total", nTotal.ToString());
                            bool bSingleRecord = false;
                            if (oDs.Tables[mcOrderType].Rows.Count == 1)
                                bSingleRecord = true;

                            // go through each cart
                            foreach (DataRow oDR in oDs.Tables[mcOrderType].Rows)
                            {
                                // Only add the relevant rows (page selected)
                                nCurrentRow += 1;
                                if (nCurrentRow > nStart)
                                {
                                    var oContent = moPageXml.CreateElement("Content");
                                    oContent.SetAttribute("type", mcOrderType);
                                    oContent.SetAttribute("id", Conversions.ToString(oDR["nCartOrderKey"]));
                                    oContent.SetAttribute("statusId", Conversions.ToString(oDR["nCartStatus"]));
                                    oContent.SetAttribute("cartForiegnRef", (oDR["cCartForiegnRef"] == null) ? string.Empty : (string)oDR["cCartForiegnRef"]);
                                    // Get Date
                                    cSQL = Conversions.ToString(Operators.ConcatenateObject("Select dInsertDate from tblAudit where nAuditKey =", oDR["nAuditId"]));
                                    using (var oDRe = moDBHelper.getDataReaderDisposable(cSQL))  // Done by nita on 6/7/22
                                    {
                                        while (oDRe.Read())
                                            oContent.SetAttribute("created", Tools.Xml.XmlDate(oDRe.GetValue(0), true));
                                    }

                                    // Get stored CartXML
                                    if (Conversions.ToBoolean(Operators.AndObject(!Operators.ConditionalCompareObjectEqual(oDR["cCartXML"], "", false), bForceRefresh == false)))
                                    {
                                        oContent.InnerXml = Conversions.ToString(oDR["cCartXML"]);
                                        if (oContent.InnerXml.Contains("\n"))
                                        {
                                            oContent.InnerXml = oContent.InnerXml.TrimStart('\n');
                                        }
                                        XmlElement oCartElmt = (XmlElement)oContent.FirstChild;

                                        // check for invoice date etc.
                                        if (Conversions.ToLong("0" + oContent.GetAttribute("statusId")) >= 6L & (string.IsNullOrEmpty(oCartElmt.GetAttribute("InvoiceDate")) | !oCartElmt.GetAttribute("InvoiceDateTime").Contains("T")))
                                        {
                                            // fix for any items that have lost the invoice date and ref.
                                            // also fix when datetime no stored in XML format.
                                            long cartId = Conversions.ToLong(oDR["nCartOrderKey"]);
                                            oCartElmt.SetAttribute("statusId", oContent.GetAttribute("statusId"));
                                            string insertDate = moDBHelper.ExeProcessSqlScalar("SELECT a.dInsertDate FROM tblCartOrder inner join tblAudit a on nAuditId = nAuditKey where nCartOrderKey = " + cartId);
                                            addDateAndRef(ref oCartElmt, Conversions.ToDate(insertDate), cartId);
                                            SaveCartXML(oCartElmt, cartId);
                                        }

                                    }

                                    if (bForceRefresh)
                                    {
                                        var oCartListElmt = moPageXml.CreateElement("Order");
                                        GetCart(ref oCartListElmt, Conversions.ToInteger(oDR["nCartOrderKey"]));
                                        oContent.InnerXml = oCartListElmt.OuterXml;
                                    }

                                    XmlElement orderNode = (XmlElement)oContent.FirstChild;
                                    // Add values not stored in cartXml
                                    if (orderNode != null)
                                    {
                                        orderNode.SetAttribute("statusId", Conversions.ToString(oDR["nCartStatus"]));
                                    }
                                    if (Conversions.ToBoolean(Operators.OrObject(oDR["cCurrency"] is null, Operators.ConditionalCompareObjectEqual(oDR["cCurrency"], "", false))))
                                    {
                                        oContent.SetAttribute("currency", mcCurrency);
                                        oContent.SetAttribute("currencySymbol", mcCurrencySymbol);
                                    }
                                    else
                                    {
                                        oContent.SetAttribute("currency", Conversions.ToString(oDR["cCurrency"]));
                                        XmlElement thisCurrencyNode = (XmlElement)moPaymentCfg.SelectSingleNode("currencies/Currency[@ref='" + oDR["cCurrency"] + "']");
                                        if (thisCurrencyNode != null)
                                        {
                                            oContent.SetAttribute("currencySymbol", thisCurrencyNode.GetAttribute("symbol"));
                                        }
                                        else
                                        {
                                            oContent.SetAttribute("currencySymbol", mcCurrencySymbol);
                                        }
                                    }
                                    oContent.SetAttribute("type", Strings.LCase(mcOrderType));

                                    // oContent.SetAttribute("currency", mcCurrency)
                                    // oContent.SetAttribute("currencySymbol", mcCurrencySymbol)

                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oDR["nCartUserDirId"], 0, false)))
                                    {
                                        oContent.SetAttribute("userId", Conversions.ToString(oDR["nCartUserDirId"]));
                                    }

                                    // TS: Removed because it gives a massive overhead when Listing loads of orders.
                                    if (bSingleRecord)
                                    {
                                        if (myWeb.mbAdminMode & Conversions.ToInteger(Operators.ConcatenateObject("0", oDR["nCartUserDirId"])) > 0)
                                        {
                                            oContent.AppendChild(moDBHelper.GetUserXML((long)Conversions.ToInteger(oDR["nCartUserDirId"]), false));
                                        }

                                        string[] aSellerNotes = Strings.Split(Conversions.ToString(oDR["cSellerNotes"]), "/n");
                                        string cSellerNotesHtml = "<ul>";
                                        for (int snCount = 0, loopTo = Information.UBound(aSellerNotes); snCount <= loopTo; snCount++)
                                            cSellerNotesHtml = cSellerNotesHtml + "<li>" + convertEntitiesToCodes(aSellerNotes[snCount]) + "</li>";
                                        var argoNode = oContent.FirstChild;
                                        var sellerNode = Protean.Tools.Xml.addNewTextNode("SellerNotes", ref argoNode, "");
                                        try
                                        {
                                            sellerNode.InnerXml = cSellerNotesHtml + "</ul>";
                                        }
                                        catch (Exception)
                                        {
                                            sellerNode.InnerXml = Tools.Text.tidyXhtmlFrag(cSellerNotesHtml + "</ul>");
                                        }

                                        // Add the Delivery Details
                                        // Add Delivery Details
                                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDR["nCartStatus"], 9, false)))
                                        {
                                            string sSql = Conversions.ToString(Operators.ConcatenateObject("Select * from tblCartOrderDelivery where nOrderId=", oDR["nCartOrderKey"]));
                                            DataSet oDs2 = moDBHelper.GetDataSet(sSql, "Delivery", "Details");
                                            foreach (DataRow oRow2 in oDs2.Tables["Delivery"].Rows)
                                            {
                                                var oElmt = moPageXml.CreateElement("DeliveryDetails");
                                                oElmt.SetAttribute("carrierName", Conversions.ToString(oRow2["cCarrierName"]));
                                                oElmt.SetAttribute("ref", Conversions.ToString(oRow2["cCarrierRef"]));
                                                oElmt.SetAttribute("notes", Conversions.ToString(oRow2["cCarrierNotes"]));
                                                oElmt.SetAttribute("deliveryDate", XmlDate(oRow2["dExpectedDeliveryDate"]));
                                                oElmt.SetAttribute("collectionDate", XmlDate(oRow2["dCollectionDate"]));
                                                oContent.AppendChild(oElmt);
                                            }
                                        }
                                        // Add Payment History
                                        string argsTableName = "tblCartPayment";
                                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectGreater(oDR["nCartStatus"], 5, false), moDBHelper.doesTableExist(ref argsTableName))))
                                        {
                                            DataSet oDs3 = new DataSet();
                                            string sSql = Conversions.ToString(Operators.ConcatenateObject("Select p.*, pm.*, a.dInsertDate from tblCartPayment p inner join tblCartPaymentMethod pm on p.nCartPaymentMethodId = pm.nPayMthdKey left outer join tblAudit a on a.nAuditKey = p.nAuditId where nCartOrderId=", oDR["nCartOrderKey"]));
                                            oDs3 = moDBHelper.GetDataSet(sSql, "Payment", "Details");
                                            oDs3.Tables["Payment"].Columns["cPayMthdDetailXml"].ColumnMapping = MappingType.Element;
                                            var oXML2 = new XmlDocument();
                                            oXML2.InnerXml = Strings.Replace(Strings.Replace(Conversions.ToString(oDs3.GetXml()), "&gt;", ">"), "&lt;", "<");
                                            var oPaymentNode = oContent.OwnerDocument.CreateElement("Payments");
                                            oPaymentNode.InnerXml = oXML2.InnerXml;
                                            foreach (XmlElement oElmt in oPaymentNode.FirstChild.SelectNodes("*"))
                                                oContent.FirstChild.AppendChild(oPaymentNode.FirstChild.FirstChild);
                                        }

                                    }

                                    XmlElement oTestNode = (XmlElement)oContentDetails.SelectSingleNode("Content[@id=" + oContent.GetAttribute("id") + " and @type='" + Strings.LCase(mcOrderType) + "']");
                                    if (mcOrderType == "Cart" | mcOrderType == "Order")
                                    {
                                        // If (Not oContent.FirstChild.Attributes("itemCount").Value = 0) And oTestNode Is Nothing Then

                                        oContentDetails.AppendChild(oContent);
                                    }

                                    // End If
                                    else if (oTestNode is null)
                                    {
                                        if (bListAllQuotes)
                                        {
                                            oContentDetails.AppendChild(oContent);
                                        }
                                        else
                                        {
                                            // If (Not oContent.FirstChild.Attributes("itemCount").Value = 0) Then
                                            oContentDetails.AppendChild(oContent);
                                            // End If
                                        }
                                    }



                                    // If (Not oContent.FirstChild.Attributes("itemCount").Value = 0) And oTestNode Is Nothing Then
                                    // oContentDetails.AppendChild(oContent)
                                    // End If
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ListOrders", ex, "", "", gbDebug);
                }
            }


            public virtual void MakeCurrent(int nOrderID)
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
                    if (!(Conversions.ToDouble(moDBHelper.ExeProcessSqlScalar("Select nCartUserDirId FROM tblCartOrder WHERE nCartOrderKey = " + nOrderID)) == (double)mnEwUserId))
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

                    oDS = moDBHelper.GetDataSet("Select * From tblCartItem WHERE nCartOrderID = " + nOrderID, "CartItems");
                    int nParentID;
                    string sSQL;

                    moDBHelper.ReturnNullsEmpty(ref oDS);

                    foreach (DataRow oDR1 in oDS.Tables["CartItems"].Rows)
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDR1["nParentId"], 0, false)))
                        {
                            sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " + "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " + "nTaxRate, nQuantity, nWeight, nAuditId) VALUES (";
                            sSQL += mnCartId + ",";
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nItemId"] is DBNull, "Null", oDR1["nItemId"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nParentId"] is DBNull, "Null", oDR1["nParentId"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["cItemRef"] is DBNull, "Null", Operators.ConcatenateObject("'", oDR1["cItemRef"])), "',"));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["cItemURL"] is DBNull, "Null", Operators.ConcatenateObject("'", oDR1["cItemURL"])), "',"));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["cItemName"] is DBNull, "Null", Operators.ConcatenateObject("'", oDR1["cItemName"])), "',"));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nItemOptGrpIdx"] is DBNull, "Null", oDR1["nItemOptGrpIdx"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nItemOptIdx"] is DBNull, "Null", oDR1["nItemOptIdx"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nPrice"] is DBNull, "Null", oDR1["nPrice"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nShpCat"] is DBNull, "Null", oDR1["nShpCat"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nDiscountCat"] is DBNull, "Null", oDR1["nDiscountCat"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nDiscountValue"] is DBNull, "Null", oDR1["nDiscountValue"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nTaxRate"] is DBNull, "Null", oDR1["nTaxRate"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nQuantity"] is DBNull, "Null", oDR1["nQuantity"]), ","));
                            sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR1["nWeight"] is DBNull, "Null", oDR1["nWeight"]), ","));
                            sSQL += moDBHelper.getAuditId() + ")";
                            nParentID = Conversions.ToInteger(moDBHelper.GetIdInsertSql(sSQL));
                            // now for any children
                            foreach (DataRow oDR2 in oDS.Tables["CartItems"].Rows)
                            {
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDR2["nParentId"], oDR1["nCartItemKey"], false)))
                                {
                                    sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " + "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " + "nTaxRate, nQuantity, nWeight, nAuditId) VALUES (";
                                    sSQL += mnCartId + ",";
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nItemId"] is DBNull, "Null", oDR2["nItemId"]), ","));
                                    sSQL += nParentID + ",";
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["cItemRef"] is DBNull, "Null", Operators.ConcatenateObject("'", oDR2["cItemRef"])), "',"));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["cItemURL"] is DBNull, "Null", Operators.ConcatenateObject("'", oDR2["cItemURL"])), "',"));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["cItemName"] is DBNull, "Null", Operators.ConcatenateObject("'", oDR2["cItemName"])), "',"));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nItemOptGrpIdx"] is DBNull, "Null", oDR2["nItemOptGrpIdx"]), ","));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nItemOptIdx"] is DBNull, "Null", oDR2["nItemOptIdx"]), ","));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nPrice"] is DBNull, "Null", oDR2["nPrice"]), ","));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nShpCat"] is DBNull, "Null", oDR2["nShpCat"]), ","));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nDiscountCat"] is DBNull, "Null", oDR2["nDiscountCat"]), ","));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nDiscountValue"] is DBNull, "Null", oDR2["nDiscountValue"]), ","));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nTaxRate"] is DBNull, "Null", oDR2["nTaxRate"]), ","));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nQuantity"] is DBNull, "Null", oDR2["nQuantity"]), ","));
                                    sSQL = Conversions.ToString(sSQL + Operators.ConcatenateObject(Interaction.IIf(oDR2["nWeight"] is DBNull, "Null", oDR2["nWeight"]), ","));
                                    sSQL += moDBHelper.getAuditId() + ")";
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

            public bool DeleteCart(int nOrderID)
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
                            nStat = Conversions.ToInteger(oDR.GetValue(0));
                            nOwner = Conversions.ToInteger(oDR.GetValue(1));
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
                        string sSQL = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Update tblCartOrder SET cCartXML ='", SqlFmt(cartXML.OuterXml.ToString())), "' WHERE nCartOrderKey = "), nCartId));
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
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(myWeb.moSession["bCurrencySelected"], true, false)))
                        {
                            if (mcCartCmd == "Currency")
                            {
                                mcCartCmd = "Cart";
                            }
                            else
                            {
                                mcCartCmd = mcCartCmd;
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

                        if (Information.IsNumeric(oCurrency.GetAttribute("ShippingRootId")))
                        {
                            mnShippingRootId = Conversions.ToInteger(oCurrency.GetAttribute("ShippingRootId"));
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

            public XmlElement CartReportsDownload(DateTime dBegin, DateTime dEnd, string cCurrencySymbol, string cOrderType, int nOrderStage, bool updateDatesWithStartAndEndTimes = true)
            {
                try
                {
                    string cSQL = "exec ";
                    string cCustomParam = string.Empty;
                    string cReportType = "CartDownload";

                    // Set the times for each date
                    if (updateDatesWithStartAndEndTimes)
                    {
                        dBegin = dBegin.Date;
                        dEnd = dEnd.Date.AddHours(23d).AddMinutes(59d).AddSeconds(59d);
                    }


                    cSQL += moDBHelper.getDBObjectNameWithBespokeCheck("spOrderDownload") + " ";
                    cSQL += Tools.Database.SqlDate(dBegin, true) + ",";
                    cSQL += Tools.Database.SqlDate(dEnd, true) + ",";
                    cSQL += "'" + cOrderType + "',";
                    cSQL += nOrderStage.ToString();

                    var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report");

                    if (oDS.Tables["Item"].Columns.Contains("cCartXML"))
                    {
                        oDS.Tables["Item"].Columns["cCartXML"].ColumnMapping = MappingType.Element;
                    }
                    var oRptElmt = myWeb.moPageXml.CreateElement("Content");
                    oRptElmt.SetAttribute("type", "Report");
                    oRptElmt.SetAttribute("name", "CartDownloads");
                    // NB editing this line to add in &'s
                    oRptElmt.InnerXml = oDS.GetXml();
                    foreach (XmlElement oElmt in oRptElmt.SelectNodes("Report/Item/cCartXml"))
                        oElmt.InnerXml = oElmt.InnerText;

                    // oRptElmt.InnerXml = Replace(Replace(Replace(Replace(oDS.GetXml, "&amp;", "&"), "&gt;", ">"), "&lt;", "<"), " xmlns=""""", "")
                    // oRptElmt.InnerXml = Replace(Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<"), " xmlns=""""", "")
                    XmlElement oReturnElmt = (XmlElement)oRptElmt.FirstChild;
                    oReturnElmt.SetAttribute("cReportType", cReportType);
                    oReturnElmt.SetAttribute("dBegin", Conversions.ToString(dBegin));
                    oReturnElmt.SetAttribute("dEnd", Conversions.ToString(dEnd));
                    oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol);
                    oReturnElmt.SetAttribute("cOrderType", cOrderType);
                    oReturnElmt.SetAttribute("nOrderStage", nOrderStage.ToString());

                    return oRptElmt;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartReports", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            public XmlElement CartReports(DateTime dBegin, DateTime dEnd, int bSplit = 0, string cProductType = "", int nProductId = 0, string cCurrencySymbol = "", string nOrderStatus = "6,9,17", string cOrderType = "ORDER")
            {
                try
                {
                    string cSQL = "exec ";
                    string cCustomParam = "";
                    string cReportType = "";
                    if (nProductId > 0)
                    {
                        // Low Level
                        cSQL += "spCartActivityLowLevel ";
                        cCustomParam = nProductId.ToString();
                        cReportType = "Item Totals";
                    }
                    else if (!string.IsNullOrEmpty(cProductType))
                    {
                        // Med Level
                        cSQL += "spCartActivityMedLevel ";
                        cCustomParam = "'" + cProductType + "'";
                        cReportType = "Type Totals";
                    }
                    else
                    {
                        // HighLevel
                        cSQL += "spCartActivityTopLevel ";
                        cCustomParam = bSplit.ToString();
                        cReportType = "All Totals";
                    }
                    cSQL += Tools.Database.SqlDate(dBegin) + ",";
                    cSQL += Tools.Database.SqlDate(dEnd) + ",";
                    cSQL += cCustomParam + ",";
                    cSQL += "'" + cCurrencySymbol + "','";
                    cSQL += nOrderStatus + "',";
                    cSQL += "'" + cOrderType + "'";

                    var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report");

                    if (oDS.Tables["Item"].Columns.Contains("cCartXML"))
                    {
                        oDS.Tables["Item"].Columns["cCartXML"].ColumnMapping = MappingType.Element;
                    }
                    var oRptElmt = myWeb.moPageXml.CreateElement("Content");
                    oRptElmt.SetAttribute("type", "Report");
                    oRptElmt.SetAttribute("name", "Cart Activity");
                    oRptElmt.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                    XmlElement oReturnElmt = (XmlElement)oRptElmt.FirstChild;
                    oReturnElmt.SetAttribute("cReportType", cReportType);

                    oReturnElmt.SetAttribute("dBegin", Conversions.ToString(dBegin));
                    oReturnElmt.SetAttribute("dEnd", Conversions.ToString(dEnd));
                    oReturnElmt.SetAttribute("bSplit", Conversions.ToString(Interaction.IIf(Conversions.ToBoolean(bSplit), 1, 0)));
                    oReturnElmt.SetAttribute("cProductType", cProductType);
                    oReturnElmt.SetAttribute("nProductId", nProductId.ToString());
                    oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol);
                    oReturnElmt.SetAttribute("nOrderStatus", nOrderStatus);
                    oReturnElmt.SetAttribute("cOrderType", cOrderType);
                    return oRptElmt;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartReports", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            public XmlElement CartReportsDrilldown(string cGrouping = "Page", int nYear = 0, int nMonth = 0, int nDay = 0, string cCurrencySymbol = "", int nOrderStatus1 = 6, int nOrderStatus2 = 9, string cOrderType = "ORDER")
            {
                try
                {
                    string cSQL = "exec spCartActivityGroupsPages ";
                    bool bPage = false;
                    string cReportType = "";

                    cSQL += "'" + cGrouping + "',";
                    cSQL += nYear + ",";
                    cSQL += nMonth + ",";
                    cSQL += nDay + ",";
                    cSQL += "'" + cCurrencySymbol + "',";
                    cSQL += nOrderStatus1 + ",";
                    cSQL += nOrderStatus2 + ",";
                    cSQL += "'" + cOrderType + "'";

                    var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report");
                    // For Grouped by page is going to be bloody hard
                    if (oDS.Tables["Item"].Columns.Contains("nStructId"))
                    {
                        bPage = true;
                        cSQL = "EXEC getContentStructure @userId=" + myWeb.mnUserId + ", @bAdminMode=1, @dateNow=" + Tools.Database.SqlDate(DateTime.Now) + ", @authUsersGrp = " + Cms.gnAuthUsers;
                        myWeb.moDbHelper.addTableToDataSet(ref oDS, cSQL, "MenuItem");
                        // oDS.Tables("MenuItem").Columns.Add(New DataColumn("PageQuantity", GetType(Double)))
                        // oDS.Tables("MenuItem").Columns.Add(New DataColumn("PageCost", GetType(Double)))
                        // oDS.Tables("MenuItem").Columns.Add(New DataColumn("DecendantQuantity", GetType(Double)))
                        // oDS.Tables("MenuItem").Columns.Add(New DataColumn("DecendantCost", GetType(Double)))
                        foreach (DataColumn oDC in oDS.Tables["MenuItem"].Columns)
                        {
                            string cValid = "id,parid,name"; // ,PageQuantity,PageCost,DecendantQuantity,DecendantCost"
                            if (!cValid.Contains(oDC.ColumnName))
                            {
                                oDC.ColumnMapping = MappingType.Hidden;
                            }
                            else
                            {
                                oDC.ColumnMapping = MappingType.Attribute;
                            }
                        }
                        foreach (DataColumn oDC in oDS.Tables["Item"].Columns)
                            oDC.ColumnMapping = MappingType.Attribute;
                        oDS.Relations.Add("Rel01", oDS.Tables["MenuItem"].Columns["id"], oDS.Tables["MenuItem"].Columns["parId"], false);
                        oDS.Relations["Rel01"].Nested = true;
                        oDS.Relations.Add(new DataRelation("Rel02", oDS.Tables["MenuItem"].Columns["id"], oDS.Tables["Item"].Columns["nStructId"], false));
                        oDS.Relations["Rel02"].Nested = true;
                    }



                    var oRptElmt = myWeb.moPageXml.CreateElement("Content");
                    oRptElmt.SetAttribute("type", "Report");
                    oRptElmt.SetAttribute("name", "Cart Activity");
                    oRptElmt.InnerXml = oDS.GetXml();
                    XmlElement oReturnElmt = (XmlElement)oRptElmt.FirstChild;
                    oReturnElmt.SetAttribute("cReportType", cReportType);

                    oReturnElmt.SetAttribute("nYear", nYear.ToString());
                    oReturnElmt.SetAttribute("nMonth", nMonth.ToString());
                    oReturnElmt.SetAttribute("nDay", nDay.ToString());
                    oReturnElmt.SetAttribute("cGrouping", cGrouping);
                    oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol);
                    oReturnElmt.SetAttribute("nOrderStatus1", nOrderStatus1.ToString());
                    oReturnElmt.SetAttribute("nOrderStatus2", nOrderStatus2.ToString());
                    oReturnElmt.SetAttribute("cOrderType", cOrderType);

                    if (bPage)
                    {
                        // Page Totals
                        foreach (XmlElement oElmt in oReturnElmt.SelectNodes("descendant-or-self::MenuItem"))
                        {
                            int nQ = 0;
                            double nC = 0d;
                            foreach (XmlElement oItemElmt in oElmt.SelectNodes("Item"))
                            {
                                nQ = (int)Math.Round(nQ + Conversions.ToDouble(oItemElmt.GetAttribute("nQuantity")));
                                nC += Conversions.ToDouble(oItemElmt.GetAttribute("nLinePrice"));
                            }
                            oElmt.SetAttribute("PageQuantity", nQ.ToString());
                            oElmt.SetAttribute("PageCost", nC.ToString());
                        }
                        // loop through each node and then each item and see how many of each
                        // item it has and its decendants
                        foreach (XmlElement oElmt in oReturnElmt.SelectNodes("descendant-or-self::MenuItem"))
                        {
                            var oHN = new Hashtable(); // Number found
                            var oHQ = new Hashtable(); // Quantity
                            var oHC = new Hashtable(); // Cost
                            foreach (XmlElement oItem in oElmt.SelectNodes("descendant-or-self::Item"))
                            {
                                string cKey = "I" + oItem.GetAttribute("nCartItemKey");
                                if (!oHN.ContainsKey(cKey))
                                {
                                    oHN.Add(cKey, 1);
                                    oHQ.Add(cKey, oItem.GetAttribute("nQuantity"));
                                    oHC.Add(cKey, oItem.GetAttribute("nLinePrice"));
                                }
                                else
                                {
                                    oHN[cKey] += Convert.ToString(1);
                                }
                            }
                            int nQ = 0;
                            double nC = 0d;
                            foreach (string ci in oHN.Keys)
                            {
                                nQ = Conversions.ToInteger(nQ + Convert.ToInt32(oHQ[ci]));
                                nC = Conversions.ToDouble(nC + Convert.ToInt32(oHC[ci]));
                            }
                            oElmt.SetAttribute("PageAndDescendantQuantity", nQ.ToString());
                            oElmt.SetAttribute("PageAndDescendantCost", nC.ToString());
                        }



                    }


                    if (bPage & Cms.gnTopLevel > 0)
                    {
                        XmlElement oElmt = (XmlElement)oRptElmt.SelectSingleNode("descendant-or-self::MenuItem[@id=" + Cms.gnTopLevel + "]");
                        if (oElmt != null)
                        {
                            oRptElmt.FirstChild.InnerXml = oElmt.OuterXml;
                        }
                        else
                        {
                            oRptElmt.FirstChild.InnerXml = "";
                        }
                    }

                    return oRptElmt;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartReports", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            public XmlElement CartReportsPeriod(string cGroup = "Month", int nYear = 0, int nMonth = 0, int nWeek = 0, string cCurrencySymbol = "", string nOrderStatus = "", string cOrderType = "ORDER")
            {
                try
                {
                    string cSQL = "exec spCartActivityPagesPeriod ";
                    //bool bPage = false;
                    string cReportType = "";
                    if (nYear == 0)
                        nYear = DateTime.Now.Year;
                    cSQL += "@Group='" + cGroup + "'";
                    cSQL += ",@nYear=" + nYear;
                    cSQL += ",@nMonth=" + nMonth;
                    cSQL += ",@nWeek=" + nWeek;
                    cSQL += ",@cCurrencySymbol='" + cCurrencySymbol + "'";
                    cSQL += ",@nOrderStatus='" + nOrderStatus + "'";
                    // cSQL += ",@nOrderStatus2=" + nOrderStatus2;
                    cSQL += ",@cOrderType='" + cOrderType + "'";

                    var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report");
                    // For Grouped by page is going to be bloody hard

                    var oRptElmt = myWeb.moPageXml.CreateElement("Content");
                    oRptElmt.SetAttribute("type", "Report");
                    oRptElmt.SetAttribute("name", "Cart Activity");
                    oRptElmt.InnerXml = oDS.GetXml();
                    XmlElement oReturnElmt = (XmlElement)oRptElmt.FirstChild;
                    oReturnElmt.SetAttribute("cReportType", cReportType);

                    oReturnElmt.SetAttribute("nYear", nYear.ToString());
                    oReturnElmt.SetAttribute("nMonth", nMonth.ToString());
                    oReturnElmt.SetAttribute("nWeek", nWeek.ToString());
                    oReturnElmt.SetAttribute("cGroup", cGroup);
                    oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol);
                    oReturnElmt.SetAttribute("nOrderStatus", nOrderStatus.ToString());
                    // oReturnElmt.SetAttribute("nOrderStatus2", nOrderStatus2.ToString());
                    oReturnElmt.SetAttribute("cOrderType", cOrderType);

                    return oRptElmt;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CartReports", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }


            public void AddShippingCosts(ref XmlElement xmlProduct, string nPrice, string nWeight)
            {
                try
                {
                    //long nQuantity = 1L;
                    xmlProduct.SetAttribute("test1", "1");
                    var xmlShippingOptions = makeShippingOptionsXML();

                    // 'step through oShipping Options and add to oElmt those options 
                    // 'that are valid for our price and weight.
                    var xmlShippingOptionsValid = moPageXml.CreateElement("ShippingOptions");

                    var strXpath = new System.Text.StringBuilder();
                    strXpath.Append("Method[ ");
                    strXpath.Append("(WeightMin=0 or WeightMin<=" + nWeight.ToString() + ") ");
                    strXpath.Append(" and ");
                    strXpath.Append("(WeightMax=0 or WeightMax>=" + nWeight.ToString() + ") ");
                    strXpath.Append(" and ");
                    strXpath.Append("(PriceMin=0 or PriceMin<=" + nPrice.ToString() + ") ");
                    strXpath.Append(" and ");
                    strXpath.Append("(PriceMax=0 or PriceMax>=" + nPrice.ToString() + ") ");
                    strXpath.Append(" ]");


                    // add filtered list to xmlShippingOptionsValid
                    XmlElement xmlMethod;
                    foreach (XmlElement currentXmlMethod in xmlShippingOptions.SelectNodes(strXpath.ToString()))
                    {
                        xmlMethod = currentXmlMethod;
                        // add to 
                        xmlShippingOptionsValid.AppendChild(xmlMethod.CloneNode(true));
                    }

                    // itterate though xmlShippingOptionsValid and get cheapest for each Location
                    string cShippingLocation = "";
                    string cShippingLocationPrev = "";

                    foreach (XmlElement currentXmlMethod1 in xmlShippingOptionsValid.SelectNodes("Method"))
                    {
                        xmlMethod = currentXmlMethod1;
                        try
                        {
                            cShippingLocation = xmlMethod.SelectSingleNode("Location").InnerText;
                            if (!string.IsNullOrEmpty(cShippingLocationPrev) & (cShippingLocationPrev ?? "") == (cShippingLocation ?? ""))
                            {
                                xmlShippingOptionsValid.RemoveChild(xmlMethod);
                            }

                            cShippingLocationPrev = cShippingLocation; // set cShippingLocationPrev for next loop
                        }
                        catch (Exception ex)
                        {
                            //xmlMethod = xmlMethod;
                            //xmlProduct = xmlProduct;
                            stdTools.returnException(ref myWeb.msException, mcModuleName, "AddShippingCosts", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                        }

                    }

                    // add to product XML
                    xmlProduct.AppendChild(xmlShippingOptionsValid.CloneNode(true));
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "AddShippingCosts", ex, vstrFurtherInfo: "", bDebug: gbDebug);

                }



            }

            public DataSet getValidShippingOptionsDS(string cDestinationCountry, double nAmount, long nQuantity, double nWeight)
            {
                try
                {
                    var dsShippingOption = getValidShippingOptionsDS(cDestinationCountry, nAmount, nQuantity, nWeight, string.Empty, 0);
                    return dsShippingOption;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }
            public DataSet getValidShippingOptionsDS(string cDestinationCountry, double nAmount, long nQuantity, double nWeight, int ProductId)
            {
                try
                {
                    var dsShippingOption = getValidShippingOptionsDS(cDestinationCountry, nAmount, nQuantity, nWeight, string.Empty, ProductId);
                    return dsShippingOption;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }
            public DataSet getValidShippingOptionsDS(string cDestinationCountry, double nAmount, long nQuantity, double nWeight, string cPromoCode)
            {
                try
                {
                    var dsShippingOption = getValidShippingOptionsDS(cDestinationCountry, nAmount, nQuantity, nWeight, cPromoCode, 0);
                    return dsShippingOption;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            public DataSet getValidShippingOptionsDS(string cDestinationCountry, double nAmount, long nQuantity, double nWeight, string cPromoCode, int ProductId)
            {
                try
                {
                    var dsShippingOption = getValidShippingOptionsDS(cDestinationCountry,"", nAmount, nQuantity, nWeight, cPromoCode, ProductId);
                    return dsShippingOption;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }
            public DataSet getValidShippingOptionsDS(string cDestinationCountry, string cDestinationPostalCode, double nAmount, long nQuantity, double nWeight, string cPromoCode, int ProductId)
            {

                try
                {
                    int userId = 0;
                    if (myWeb.moSession != null)
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(myWeb.moSession["nUserId"], 0, false)))
                        {
                            userId = Conversions.ToInteger(myWeb.moSession["nUserId"]);
                        }
                        else
                        {
                            userId = myWeb.mnUserId;
                        }
                    }
                    else
                    {
                        userId = myWeb.mnUserId;

                    }
                    int argnIndex = 1;
                    string sCountryList = ""; 
                    // Add code for checking shipping group is included/Excluded for delievry methods
                    var PublishExpireDate = DateTime.Now;
                    if (moCartConfig["ShippingPostcodes"] == "on" && cDestinationPostalCode != "") {

                        string PostcodePrefix = System.Text.RegularExpressions.Regex.Split(cDestinationPostalCode, "(?m)^([A-Z0-9]{2,4})(?:\\s*[A-Z0-9]{3})?$")[1];
                        sCountryList = getParentCountries(ref PostcodePrefix, ref argnIndex);
                       
                    }

                    if (sCountryList == "") {
                        sCountryList = getParentCountries(ref cDestinationCountry, ref argnIndex);
                    }


                    if (myWeb.moDbHelper.checkDBObjectExists("spGetValidShippingOptions", Tools.Database.objectTypes.StoredProcedure))
                    {
                        // ' call stored procedure else existing code.
                        // ' Passing parameter: nCartId

                        var param = new Hashtable();
                        param.Add("CartOrderId", mnCartId);
                        param.Add("Amount", nAmount);
                        param.Add("Quantity", nQuantity);
                        param.Add("Weight", nWeight);
                        param.Add("Currency", mcCurrency);
                        param.Add("userId", userId);
                        param.Add("AuthUsers", (object)Cms.gnAuthUsers);
                        param.Add("NonAuthUsers", (object)Cms.gnNonAuthUsers);
                        param.Add("CountryList", sCountryList);
                        param.Add("dValidDate", PublishExpireDate);
                        param.Add("PromoCode", cPromoCode);
                        param.Add("ProductId", ProductId);
                        return moDBHelper.GetDataSet("spGetValidShippingOptions", "Option", "Shipping", false, param, CommandType.StoredProcedure);
                    }
                    // End If
                    else
                    {

                        string sSql;

                        sSql = "select opt.*, dbo.fxn_shippingTotal(opt.nShipOptKey," + nAmount + "," + nQuantity + "," + nWeight + ") as nShippingTotal  from tblCartShippingLocations Loc ";
                        sSql = sSql + "Inner Join tblCartShippingRelations rel ON Loc.nLocationKey = rel.nShpLocId ";
                        sSql = sSql + "Inner Join tblCartShippingMethods opt ON rel.nShpOptId = opt.nShipOptKey ";
                        sSql += "INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey";

                        sSql = sSql + " WHERE (nShipOptQuantMin <= 0 or nShipOptQuantMin <= " + nQuantity + ") and (nShipOptQuantMax <= 0 or nShipOptQuantMax >= " + nQuantity + ") and ";
                        sSql = sSql + "(nShipOptPriceMin <= 0 or nShipOptPriceMin <= " + nAmount + ") and (nShipOptPriceMax <= 0 or nShipOptPriceMax >= " + nAmount + ") and ";
                        sSql = sSql + "(nShipOptWeightMin <= 0 or nShipOptWeightMin <= " + nWeight + ") and (nShipOptWeightMax <= 0 or nShipOptWeightMax >= " + nWeight + ") ";

                        sSql += " and ((opt.cCurrency Is Null) or (opt.cCurrency = '') or (opt.cCurrency = '" + mcCurrency + "'))";
                        // If myWeb.mnUserId > 0 Then
                        // ' if user in group then return it
                        // sSql &= " and ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" &
                        // " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" &
                        // "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " & myWeb.mnUserId & " and perm.nPermLevel = 1) > 0"
                        // sSql &= " and not((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" &
                        // " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" &
                        // "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " & myWeb.mnUserId & " and perm.nPermLevel = 0) > 0)"
                        if (userId > 0)
                        {
                            // if user in group then return it
                            sSql += " and ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" + " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" + "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " + userId + " and perm.nPermLevel = 1) > 0";
                            sSql += " and not((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" + " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" + "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " + userId + " and perm.nPermLevel = 0) > 0)";
                            // method allowed for authenticated or imporsonating CS users.
                            string shippingGroupCondition;

                            shippingGroupCondition = "perm.nDirId = " + Cms.gnAuthUsers;

                            sSql += " Or (SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" + "  where perm.nShippingMethodId = opt.nShipOptKey And " + shippingGroupCondition + " And perm.nPermLevel = 1) > 0";

                            // if no group exists return it.
                            sSql += " or (SELECT COUNT(*) from tblCartShippingPermission perm where opt.nShipOptKey = perm.nShippingMethodId and perm.nPermLevel = 1) = 0)";

                            sSql += @" And opt.nShipOptKey not in ( select nShippingMethodId
                                from tblCartShippingPermission perm 
                                Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId  
                                 and  nPermLevel = 0  and PermGroup.nDirChildId =" + userId + ")";
                        }

                        else
                        {
                            long nonAuthID = (long)Cms.gnNonAuthUsers;
                            long AuthID = (long)Cms.gnAuthUsers;
                            // method allowed for non-authenticated
                            sSql += " and ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" + "  where perm.nShippingMethodId = opt.nShipOptKey and perm.nDirId = " + Cms.gnNonAuthUsers + "  and perm.nPermLevel = 1) > 0";
                            // method has no group 
                            sSql += " or (SELECT COUNT(*) from tblCartShippingPermission perm where opt.nShipOptKey = perm.nShippingMethodId and perm.nPermLevel = 1) = 0)";

                        }
                        // Restrict the shipping options by looking at the delivery country currently selected.  
                        // Of course, if we are hiding the delivery address then this can be ignored.

                        if (!string.IsNullOrEmpty(sCountryList))
                        {
                            sSql = sSql + " and ((loc.cLocationNameShort IN " + sCountryList + ") or (loc.cLocationNameFull IN " + sCountryList + ")) ";
                        }

                        // Active methods

                        sSql += " AND (tblAudit.nStatus >0)";
                        sSql += " AND ((tblAudit.dPublishDate = 0) or (tblAudit.dPublishDate Is Null) or (tblAudit.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + "))";
                        sSql += " AND ((tblAudit.dExpireDate = 0) or (tblAudit.dExpireDate Is Null) or (tblAudit.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + "))";
                        // Build Form

                        // Go and collect the valid shipping options available for this order
                        return moDBHelper.GetDataSet(sSql + " order by opt.nDisplayPriority, nShippingTotal", "Option", "Shipping");

                        }
                    
                }

                catch (Exception ex)
                {

                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }

            }


            private XmlElement makeShippingOptionsXML()
            {

                try
                {


                    if (oShippingOptions is null)
                    {

                        XmlElement xmlTemp;

                        // create XML of all possible shipping methods and add it to the page XML
                        oShippingOptions = moPageXml.CreateElement("oShippingOptions");


                        // get all the shipping options for a given shipping weight and price
                        var strSql = new System.Text.StringBuilder();
                        strSql.Append("SELECT opt.cShipOptCarrier as Carrier, opt.cShipOptTime AS ShippingTime, ");
                        strSql.Append("opt.nShipOptCost AS Cost, ");
                        strSql.Append("tblCartShippingLocations.cLocationNameShort as Location, tblCartShippingLocations.cLocationISOa2 as LocationISOa2, ");
                        strSql.Append("opt.nShipOptWeightMin AS WeightMin, opt.nShipOptWeightMax AS WeightMax,  ");
                        strSql.Append("opt.nShipOptPriceMin AS PriceMin, opt.nShipOptPriceMax AS PriceMax,  ");
                        strSql.Append("opt.nShipOptQuantMin AS QuantMin, opt.nShipOptQuantMax AS QuantMax, ");
                        strSql.Append("tblCartShippingLocations.nLocationType, tblCartShippingLocations.cLocationNameFull, opt.cShipOptName,opt.nShipOptKey ");
                        strSql.Append("FROM tblCartShippingLocations ");
                        strSql.Append("INNER JOIN tblCartShippingRelations ON tblCartShippingLocations.nLocationKey = tblCartShippingRelations.nShpLocId ");
                        strSql.Append("RIGHT OUTER JOIN tblCartShippingMethods AS opt ");
                        strSql.Append("INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey ON tblCartShippingRelations.nShpOptId = opt.nShipOptKey ");

                        strSql.Append("WHERE (tblAudit.nStatus > 0) ");
                        strSql.Append("AND (tblAudit.dPublishDate = 0 OR tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + ") ");
                        strSql.Append("AND (tblAudit.dExpireDate = 0 OR tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + ") ");
                        strSql.Append("AND (tblCartShippingLocations.cLocationNameShort IS NOT NULL) ");
                        strSql.Append("ORDER BY tblCartShippingLocations.cLocationNameShort, opt.nShipOptCost ");



                        var oDs = moDBHelper.GetDataSet(strSql.ToString(), "Method", "ShippingMethods");
                        oShippingOptions.InnerXml = oDs.GetXml();

                        // move all the shipping methods up a level
                        foreach (XmlElement currentXmlTemp in oShippingOptions.SelectNodes("ShippingMethods/Method"))
                        {
                            xmlTemp = currentXmlTemp;
                            oShippingOptions.AppendChild(xmlTemp);
                        }

                        foreach (XmlElement currentXmlTemp1 in oShippingOptions.SelectNodes("ShippingMethods"))
                        {
                            xmlTemp = currentXmlTemp1;
                            oShippingOptions.RemoveChild(xmlTemp);
                        }

                    }

                    return oShippingOptions;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "makeShippingOptionsXML", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }

            }

            private string updatePackagingForFreeGiftDiscount(string nCartItemKey, decimal AmountToDiscount)
            {
                try
                {
                    string cSqlUpdate;
                    // cSqlUpdate = " update tblCartItem set nPrice=0.00, nDiscountValue=" & AmountToDiscount & ", cItemName =  '" & moConfig("GiftPack") & "' where  nitemid=0 and nParentid = " & nCartItemKey
                    cSqlUpdate = " update tblCartItem set nPrice=" + AmountToDiscount + ", nDiscountValue=" + AmountToDiscount + ", cItemName =  '" + moConfig["GiftPack"] + "' where  nitemid=0 and nParentid = " + nCartItemKey;
                    moDBHelper.ExeProcessSql(cSqlUpdate);
                    return null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updatePackagingForFreeGiftDiscount", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            private string updatePackagingForRemovingFreeGiftDiscount(string nCartOrderId, decimal AmountToDiscount)
            {
                try
                {
                    string cSqlUpdate;
                    cSqlUpdate = " update tblCartItem set nDiscountValue=" + AmountToDiscount + " where  nitemid=0 and nCartOrderId = " + nCartOrderId;
                    moDBHelper.ExeProcessSql(cSqlUpdate);
                    return null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updatePackagingForFreeGiftDiscount", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            private string updateGCgetValidShippingOptionsDS(string nShipOptKey)
            {
                try
                {

                    // Dim ods As DataSet
                    // Dim oRow As DataRow
                    string sSql;
                    string cShippingDesc;
                    string nShippingCost;
                    string cSqlUpdate;

                    sSql = "select * from tblCartShippingMethods ";
                    sSql = sSql + " where nShipOptKey in ( " + nShipOptKey + ")";
                    // ods = moDBHelper.GetDataSet(sSql, "Order", "Cart")

                    // Check if shipping option contains multiple option then get lowest 
                    if (Strings.InStr(1, nShipOptKey, ",") > 0)
                    {
                        nShipOptKey = nShipOptKey.Split(',')[0];
                    }
                    // For Each oRow In ods.Tables("Order").Rows

                    // cShippingDesc = oRow("cShipOptName") & "-" & oRow("cShipOptCarrier")
                    // nShippingCost = oRow("nShipOptCost")
                    // cSqlUpdate = "UPDATE tblCartOrder Set cShippingDesc='" & SqlFmt(cShippingDesc) & "', nShippingCost=" & SqlFmt(nShippingCost) & ", nShippingMethodId = " & nShipOptKey & " WHERE nCartOrderKey=" & mnCartId
                    // moDBHelper.ExeProcessSql(cSqlUpdate)
                    // Next

                    using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(sSql))
                    {
                        if (oDr != null)
                        {
                            while (oDr.Read())
                            {
                                cShippingDesc = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(oDr["cShipOptName"], "-"), oDr["cShipOptCarrier"]));
                                nShippingCost = Conversions.ToString(oDr["nShipOptCost"]);
                                cSqlUpdate = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("UPDATE tblCartOrder Set cShippingDesc='", SqlFmt(cShippingDesc)), "', nShippingCost="), SqlFmt(nShippingCost)), ", nShippingMethodId = "), nShipOptKey), " WHERE nCartOrderKey="), mnCartId));
                                moDBHelper.ExeProcessSql(cSqlUpdate);
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {

                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updateGCgetValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }


            public void AddProductOption(ref Newtonsoft.Json.Linq.JObject jObj)
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

                    moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement);
                }
                // UpdatePackagingANdDeliveryType(mnCartId, ShippingKey)
                catch (Exception)
                {
                }
            }

            public void AddProductOption(int nCartItemId, string cOptionName, double nOptionCost)
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

            public string AddClientNotes(string sNotesText)
            {
                string cProcessInfo = "AddClientNotes";
                string sSql;
                DataSet oDs;
                string sXmlContent;
                try
                {
                    // myCart.moCartXml
                    if (mnCartId > 0)
                    {
                        sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                        {
                            // load existing notes from Cart
                            sXmlContent = Conversions.ToString(Operators.ConcatenateObject(oRow["cClientNotes"], ""));
                            if (string.IsNullOrEmpty(sXmlContent))
                            {
                                sXmlContent = "<Notes><Notes/><PromotionalCode/></Notes>";
                            }
                            var NotesXml = new XmlDocument();
                            NotesXml.LoadXml(sXmlContent);

                            if (NotesXml.SelectSingleNode("Notes/Notes") is null)
                            {
                                NotesXml.DocumentElement.AppendChild(NotesXml.CreateElement("Notes"));
                            }

                            NotesXml.SelectSingleNode("Notes/Notes").InnerText = sNotesText;

                            oRow["cClientNotes"] = NotesXml.OuterXml;
                        }
                        myWeb.moDbHelper.updateDataset(ref oDs, "Order", true);
                        oDs.Clear();
                        oDs = null;

                        return sNotesText;
                    }
                    else
                    {

                        return "";
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "AddDiscountCode", ex, "", cProcessInfo, gbDebug);
                    return null;
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
                        xmlDoc.LoadXml(Conversions.ToString(oRow["cClientNotes"]));
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
                        string additionalInfo = Conversions.ToString(Operators.AddObject(Operators.AddObject("<additionalXml>", oDs.Tables["Discount"].Rows[0]["cAdditionalXML"]), "</additionalXml>"));
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

                    int nCartIdUse;
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

                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(moDBHelper.DBN2int(oRow["nParentId"]), 0, false)))
                        {
                            long nTaxRate = 0L;
                            bool bOverridePrice = false;
                            if (!mbOveridePrice) // for openquote
                            {
                                // Go get the lowest price based on user and group
                                if (!(oRow["productDetail"] is DBNull))
                                {

                                    var oProd = moPageXml.CreateElement("product");
                                    oProd.InnerXml = Conversions.ToString(oRow["productDetail"]);
                                    if (oProd.SelectSingleNode("Content[@overridePrice='true']") is null)
                                    {
                                        oCheckPrice = getContentPricesNode(oProd, Conversions.ToString(Operators.ConcatenateObject(oRow["unit"], "")), Conversions.ToLong(oRow["quantity"]));
                                        cProcessInfo = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("Error getting price for unit:", oRow["unit"]), " and Quantity:"), oRow["quantity"]), " and Currency "), mcCurrencyRef), " Check that a price is available for this quantity and a group for this current user."));
                                        if (oCheckPrice != null)
                                        {
                                            nCheckPrice = Conversions.ToDouble(oCheckPrice.InnerText);
                                            nTaxRate = (long)Math.Round(getProductTaxRate(oCheckPrice));
                                        }
                                        // nCheckPrice = getProductPricesByXml(oRow("productDetail"), oRow("unit") & "", oRow("quantity"))

                                        if (moSubscription != null & Conversions.ToString(Operators.ConcatenateObject(oRow["contentType"], "")) == "Subscription")
                                        {
                                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectGreater(oRow["contentId"], 0, false)))
                                            {
                                                revisedPrice = moSubscription.CartSubscriptionPrice(Conversions.ToInteger(oRow["contentId"]), myWeb.mnUserId);
                                            }
                                            else
                                            {
                                                oCheckPrice = getContentPricesNode(oProd, Conversions.ToString(Operators.ConcatenateObject(oRow["unit"], "")), Conversions.ToLong(oRow["quantity"]), "SubscriptionPrices");
                                                nCheckPrice = Conversions.ToDouble(oCheckPrice.InnerText);
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
                                    else
                                    {
                                        bOverridePrice = true;
                                    }

                                }
                                if (!bOverridePrice)
                                {
                                    if (Conversions.ToBoolean(Operators.AndObject(nCheckPrice > 0d, Operators.ConditionalCompareObjectNotEqual(nCheckPrice, oRow["price"], false))))
                                    {
                                        // If price is lower, then update the item price field
                                        // oRow.BeginEdit()
                                        oRow["price"] = nCheckPrice;
                                        // oRow("taxRate") = nTaxRate
                                        // oRow.EndEdit()
                                    }

                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(oRow["taxRate"], nTaxRate, false)))
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
                                    decimal nNPrice = (decimal)getOptionPricesByXml(Conversions.ToString(oRow["productDetail"]), Conversions.ToInteger(oRow["nItemOptGrpIdx"]), Conversions.ToInteger(oRow["nItemOptIdx"]));
                                    if (Conversions.ToBoolean(Operators.AndObject(nNPrice > 0m, Operators.ConditionalCompareObjectNotEqual(nNPrice, oOpRow["price"], false))))
                                    {
                                        nOpPrices += nNPrice;
                                        // oOpRow.BeginEdit()
                                        oOpRow["price"] = nNPrice;
                                    }

                                    // oOpRow.EndEdit()
                                    else
                                    {
                                        nOpPrices = Conversions.ToDecimal(nOpPrices + Convert.ToDecimal(oOpRow["price"]));
                                    }
                                }
                            }

                            // Apply stock control
                            if (mbStockControl)
                                CheckStock(ref oCartElmt, Conversions.ToString(oRow["productDetail"]), Conversions.ToString(oRow["quantity"]));
                            // Apply quantity control
                            if (!(oRow["productDetail"] is DBNull))
                            {
                                // not sure why the product has no detail but if it not we skip this, suspect it was old test data that raised this issue.
                                CheckQuantities(ref oCartElmt, Conversions.ToString(Operators.ConcatenateObject(oRow["productDetail"], "")), Conversions.ToLong(Operators.ConcatenateObject("0", oRow["quantity"])).ToString());
                            }

                            weight = Conversions.ToDouble(Operators.AddObject(weight, Operators.MultiplyObject(oRow["weight"], oRow["quantity"])));
                            quant = Conversions.ToLong(Operators.AddObject(quant, oRow["quantity"]));

                            total = Conversions.ToDouble(Operators.AddObject(total, Operators.MultiplyObject(oRow["quantity"], Round(Operators.AddObject(oRow["price"], nOpPrices), bForceRoundup: mbRoundup))));
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
                                updateGCgetValidShippingOptionsDS(Conversions.ToString(oRowSO["nShipOptKey"]));
                                DeliveryOption = Conversions.ToString(oRowSO["cShipOptName"]);
                                // pass total item cost including packaging amount
                                DeliveryOption = Conversions.ToString(Operators.ConcatenateObject(DeliveryOption + "#" + total + "#", oRowSO["nShipOptKey"]));
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
            public string CreateDuplicateOrder(XmlDocument oldCartxml, int nOrderId, string cMethodName, string cNewAuthNumber)
            {
                try
                {
                    string cResult = "Success";
                    var oCartListElmt = moPageXml.CreateElement("Order");
                    //GetCart(ref oCartListElmt, nOrderId);
                    // Insert code into tblcartOrder

                    oCartListElmt = (XmlElement)oldCartxml.DocumentElement.Clone();

                    var oInstance = new XmlDocument();
                    XmlElement oElmt;
                    XmlElement oeResponseElmt = (XmlElement)oCartListElmt.SelectSingleNode("/PaymentDetails/instance/Response");
                    string ReceiptId = oCartListElmt.SelectSingleNode("/PaymentDetails/instance/Response/@ReceiptId").Value.ToString();
                    double Amount = Convert.ToDouble(oCartListElmt.GetAttribute("total"));
                    //int nItemID = 0; // ID of the cart item record
                    // Dim oDs As DataSet

                    XmlElement oePaymentDetailsInstanceElmt = (XmlElement)oCartListElmt.SelectSingleNode("/PaymentDetails/instance");

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

                    mnCartId = Conversions.ToInteger(moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartOrder, oInstance.DocumentElement));

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

                                long nCartItemId = Conversions.ToLong(moDBHelper.ExeProcessSqlScalar(sSQL2));

                                foreach (XmlElement oOption in oItem.SelectNodes("Item"))
                                {

                                    if (oOption.SelectSingleNode("Name") != null)
                                    {
                                        oOptionName = oOption.SelectSingleNode("Name").InnerText;
                                    }
                                    if (oOption.Attributes["nPrice"] != null)
                                    {
                                        oOptionValue = Conversions.ToDouble(oOption.Attributes["nPrice"].Value);
                                    }
                                    AddProductOption((int)nCartItemId, oOptionName, oOptionValue);
                                }
                            }
                        }
                    }

                    int deliveryAddId = 0;
                    int billingAddId = 0;
                    string sSql = "select nContactKey, cContactType, nAuditKey from tblCartContact inner join tblAudit a on nAuditId = a.nAuditKey where nContactCartId = " + nOrderId.ToString();
                    using (var oDr = moDBHelper.getDataReaderDisposable(sSql))
                    {
                        while (oDr.Read())
                        {
                            if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDr["cContactType"], "Billing Address", false)))
                            {
                                billingAddId = Conversions.ToInteger(oDr["nContactKey"]);
                            }
                            if (mbNoDeliveryAddress)
                            {
                                deliveryAddId = billingAddId;
                            }
                            else if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDr["cContactType"], "Delivery Address", false)))
                            {
                                deliveryAddId = Conversions.ToInteger(oDr["nContactKey"]);
                            }
                        }

                    }


                    if (deliveryAddId != 0 & billingAddId != 0)
                    {
                        useSavedAddressesOnCart(billingAddId, deliveryAddId, null);
                    }
                    ConfirmPayment(ref oCartListElmt, ref oePaymentDetailsInstanceElmt, cNewAuthNumber, cMethodName, Amount);
                    GetCart(ref oCartListElmt, mnCartId);
                    oCartListElmt.ToString().Replace(ReceiptId, cNewAuthNumber);
                    SaveCartXML(oCartListElmt, mnCartId);
                    return mnCartId.ToString();
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "CreateDuplicateOrder", ex, "", "", gbDebug);
                    return null;
                }
            }
        }
    }
}