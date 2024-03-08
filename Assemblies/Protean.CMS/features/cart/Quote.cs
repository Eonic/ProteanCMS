using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.Membership;
using static Protean.stdTools;

namespace Protean
{

    public partial class Cms
    {

        public class Quote : Cart
        {
            #region Properties
            public override string OrderNoPrefix
            {
                get
                {
                    if (string.IsNullOrEmpty(cOrderNoPrefix))
                    {
                        cOrderNoPrefix = moQuoteConfig["QuoteNoPrefix"];
                    }
                    return cOrderNoPrefix;
                }
                set
                {
                    cOrderNoPrefix = value;
                }
            }
            #endregion

            //private string mcCartSchemaName = "quote";
            private bool bListAllQuotes = false;
            public System.Collections.Specialized.NameValueCollection moQuoteConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/quote");
            private string cOrderNoPrefix = "";


            public Quote(ref Cms aWeb) : base(ref aWeb)
            {


                string cProcessInfo = "";
                try
                {
                    InitializeVariables();
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "Close", ex, "", cProcessInfo, gbDebug);
                }
            }

            private new void InitializeVariables()
            {
                // PerfMon.Log("Quote", "InitializeVariables")
                // Author:        Trevor Spink
                // Copyright:     Eonic Ltd 2006
                // Date:          2006-10-04

                // called at the beginning, whenever ewCart is run
                // sets the global variables and initialises the current cart
                moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/quote");
                string sSql;
                // Dim oDr As SqlDataReader
                mcOrderType = "Quote";
                cOrderReference = "";
                base.mcModuleName = "Eonic.Quote";
                mmcOrderType = "Quote";
                string cProcessInfo = Conversions.ToString(string.IsNullOrEmpty("initialise variables"));
                try
                {

                    if (base.mcPagePath == "")
                    {
                        base.mcPagePath = "?pgid=" + myWeb.mnPageId + "&";
                    }
                    base.moDBHelper = myWeb.moDbHelper;
                    if (base.moConfig["ewMembership"] == "on")
                        mbEwMembership = true;
                    mcNotesXForm = moQuoteConfig["NotesXForm"];
                    mcBillingAddressXform = moQuoteConfig["BillingAddressXForm"];
                    mcDeliveryAddressXform = moQuoteConfig["DeliveryAddressXForm"];
                    if (moQuoteConfig["ListAllQuotes"] != null)
                    {
                        if (moQuoteConfig["ListAllQuotes"] == "on")
                        {
                            bListAllQuotes = true;
                        }
                    }

                    mcSiteURL = moCartConfig["SiteURL"];
                    mcTermsAndConditions = moCartConfig["TermsAndConditions"];
                    // OrderNoPrefix = moQuoteConfig("QuoteNoPrefix")
                    mcCurrencySymbol = moCartConfig["CurrencySymbol"];
                    mcCurrency = moCartConfig["Currency"];
                    if (mcCurrency == "")
                        mcCurrency = "GBP";

                    if (moCartConfig["NoDeliveryAddress"] == "on")
                        mbNoDeliveryAddress = true;

                    if (myWeb.moRequest.Form["url"] != "")
                    {
                        myWeb.moSession["returnPage"] = myWeb.moRequest.Form["url"];
                    }
                    if (myWeb.moRequest.QueryString["url"] != "")
                    {
                        myWeb.moSession["returnPage"] = myWeb.moRequest.QueryString["url"];
                    }

                    mcReturnPage = myWeb.moSession["returnPage"].ToString();

                    // If goApp("sCookieDomain") = "" Then
                    if (myWeb.moSession["nEwUserId"] != null)
                    {
                        mnEwUserId = Convert.ToInt32("0" + myWeb.moSession["nEwUserId"]);
                    }
                    else
                    {
                        mnEwUserId = 0;
                    }
                    if (myWeb.mnUserId > 0 & mnEwUserId == 0)
                        mnEwUserId = myWeb.mnUserId;
                    // MEMB - eEDIT
                    if (Convert.ToBoolean(myWeb.moCtx.Application["bFullCartOption"]) == true)
                    {
                        bFullCartOption = true;
                    }
                    else
                    {
                        bFullCartOption = false;
                    }

                    if (myWeb.moSession["QuoteId"] is null)
                    {
                        mnCartId = 0;
                    }
                    else if (!Information.IsNumeric(myWeb.moSession["QuoteId"]) | Convert.ToInt32(myWeb.moSession["QuoteId"]) <= 0)
                    {
                        mnCartId = 0;
                    }
                    else
                    {
                        mnCartId = Convert.ToInt32(myWeb.moSession["QuoteId"]);
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

                    if (Information.IsNumeric(myWeb.moRequest.QueryString["cartErr"]))
                        mnProcessError = Convert.ToInt16(myWeb.moRequest.QueryString["cartErr"]);
                    mcCartCmd = null;
                    if (myWeb.moRequest.QueryString["quoteCmd"] != "")
                    {
                        mcCartCmd = myWeb.moRequest.QueryString["quoteCmd"];
                    }
                    if (mcCartCmd == "")
                    {
                        mcCartCmd = myWeb.moRequest.Form["quoteCmd"];
                    }


                    mmcOrderType =Convert.ToString(myWeb.moSession["mmcOrderType"]);
                    mcItemOrderType = myWeb.moRequest.Form["ordertype"];

                    // MsgBox "Item: " & mcItemOrderType & vbCrLf & "Order: " & mmcOrderType
                    // set global variable for submit button

                    mcSubmitText = myWeb.moRequest["submit"];

                    if (mnCartId > 0)
                    {
                        // cart exists
                        sSql = "select * from tblCartOrder where (nCartStatus < 7 or nCartStatus = 10) and nCartOrderKey = " + mnCartId + " and not(cCartSessionId like 'OLD_%')";

                        using (SqlDataReader oDr = base.moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            if (oDr.HasRows)
                            {
                                while (oDr.Read())
                                {
                                    mnGiftListId = Convert.ToInt32(oDr["nGiftListId"]);
                                    mnTaxRate = Convert.ToDouble(Operators.ConcatenateObject("0", oDr["nTaxRate"]));
                                    mnProcessId = Convert.ToInt16(Operators.ConcatenateObject("0", oDr["nCartStatus"]));
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
                    // Cart doesn't exist - check if it can be found in the database, although only run this check if we know that we've visited the cart
                    // Also check out if this is coming from a Worldpay callback.
                    else if (myWeb.moRequest["refSessionId"] != null | myWeb.moRequest["transStatus"] != null | myWeb.moRequest["settlementRef"] != null)
                    {
                        if (myWeb.moRequest["transStatus"] != null)
                        {
                            sSql = "select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId  where o.cCartSchemaName='cart' and o.nCartOrderKey=" + myWeb.moRequest["cartId"] + " and DATEDIFF(hh,a.dInsertDate,GETDATE())<24";
                        }
                        // mcPaymentMethod = "WorldPay"
                        else if (myWeb.moRequest["settlementRef"] != null)
                        {
                            // Go get the cart, restore settings
                            sSql = "select * from tblCartOrder where cCartSchemaName='cart' and cSettlementID='" + myWeb.moRequest["settlementRef"] + "'";
                        }
                        else
                        {
                            sSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId where o.cCartSchemaName='cart' and o.cCartSessionId = '", stdTools.SqlFmt(mcSessionId)), "' and DATEDIFF(hh,a.dInsertDate,GETDATE())<24"));
                        }
                        using (SqlDataReader oDr = base.moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {
                            if (oDr.HasRows)
                            {
                                while (oDr.Read())
                                {
                                    mnGiftListId = Convert.ToInt32(oDr["nGiftListId"]);
                                    mnCartId = Convert.ToInt32(oDr["nCartOrderKey"]); // get cart id
                                    mnProcessId = Convert.ToInt16(oDr["nCartStatus"]); // get cart status
                                    mnTaxRate = Convert.ToInt32(oDr["nTaxRate"]);
                                    if (myWeb.moRequest["settlementRef"] != null)
                                    {

                                        // Set eh commands for a settlement
                                        mcSubmitText = "Go To Checkout";
                                        mnProcessId = 5;

                                        // If a cart has been found, we need to update the session ID in it.
                                        if (!Operators.ConditionalCompareObjectEqual(oDr["cCartSessionId"], mcSessionId, false))
                                        {
                                            base.moDBHelper.ExeProcessSql("update tblCartOrder set cCartSessionId = '" + mcSessionId + "' where nCartOrderKey = " + mnCartId);
                                        }

                                        // Reactivate the order in the database
                                        base.moDBHelper.ExeProcessSql("update tblCartOrder set nCartStatus = '" + mnProcessId + "' where nCartOrderKey = " + mnCartId);

                                    }
                                    if (mnProcessId > 5)
                                    {
                                        // Cart has passed a status of "Succeeded" - we can't do anything to this cart. Clear the session.
                                        EndSession();
                                        mnCartId = 0;
                                        mnProcessId = 0;
                                        mcCartCmd = "";
                                    }
                                }
                                oDr.Close();
                            }
                        }

                    }
                }

                catch (Exception ex)
                {
                    // close()
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "Open", ex, "", cProcessInfo, gbDebug);
                }
                finally
                {
                    // oDr = Nothing
                }
            }

            public new void close()
            {
                // PerfMon.Log("Quote", "close")
                string cProcessInfo = "";
                try
                {
                    PersistVariables();
                    base.close();
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "Close", ex, "", cProcessInfo, gbDebug);
                }
            }

            public override void PersistVariables()
            {
                // PerfMon.Log("Quote", "PersistVariables")
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

                    if (myWeb.moSession["QuoteId"] is null)
                    {
                        myWeb.moSession.Add("QuoteId", mnCartId);
                    }
                    else
                    {
                        myWeb.moSession["QuoteId"] = mnCartId;
                    }
                    // oResponse.Cookies(mcSiteURL & "CartId").Domain = mcSiteURL
                    // oSession("nCartOrderId") = mnCartId    '   session attribute holds Cart ID

                    if (mnCartId > 0)
                    {
                        // If we have a cart, update its status in the db

                        sSql = "update tblCartOrder set nCartStatus = " + mnProcessId + ", nGiftListId = " + mnGiftListId + " where nCartOrderKey = " + mnCartId;

                        // update the auditId

                        base.moDBHelper.ExeProcessSql(sSql);
                    }

                    myWeb.moSession["nProcessId"] = mnProcessId; // persist global mnProcessId
                    myWeb.moSession["mmcOrderType"] = mmcOrderType;
                    myWeb.moSession["nTaxRate"] = mnTaxRate;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "PersistVariables", ex, "", cProcessInfo, gbDebug);
                    // close()
                }
            }

            public override void checkButtons()
            {
                // PerfMon.Log("Quote", "checkButtons")
                string cProcessInfo = "";
                try
                {
                    // legacy button handling looking at button values rather than names, should not be required soon
                    // Select Case mcSubmitText
                    // Case "Goto Checkout", "Send Quote Request"
                    // mcCartCmd = "Logon"
                    // 'old for the cart
                    // 'updateCart("RedirectSecure")
                    // 'mcCartCmd = "RedirectSecure"

                    // Case "Edit Billing Details", "Proceed without Logon"
                    // mcCartCmd = "Billing"
                    // Case "Edit Delivery Details"
                    // mcCartCmd = "Delivery"
                    // 'Case "Confirm Order", "Proceed with Order", "Proceed"
                    // 'updateCart("ChoosePaymentShippingOption")

                    // Case "Update Quote", "Update Order"
                    // updateCart("Quote")
                    // Case "Empty Quote", "Empty Order"
                    // mcCartCmd = "Quit"
                    // 'Case "Make Secure Payment"
                    // 'updateCart(mcCartCmd)
                    // Case "Continue Shopping"
                    // mcCartCmd = "BackToSite"
                    // End Select

                    // New button handling based on the name of the button rather than the value
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteAdd"))
                    {
                        mcCartCmd = "Add";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteDetail"))
                    {
                        mcCartCmd = "Quote";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteUpdate"))
                    {
                        mcCartCmd = "Update";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteBrief"))
                    {
                        mcCartCmd = "BackToSite";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteNotes"))
                    {
                        mcCartCmd = "Notes";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteSearch"))
                    {
                        mcCartCmd = "Search";
                    }

                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteLogon") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteRegister") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "ewRegSubmit"))
                    {
                        mcCartCmd = "Logon";
                    }

                    // Pick Address Buttions
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteBillAddress") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteBillcontact") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteBilladdNewAddress") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteBilleditAddress"))
                    {
                        mcCartCmd = "Billing";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteDelAddress") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteDelcontact") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteDeladdNewAddress") | stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteDeleditAddress"))
                    {
                        mcCartCmd = "Delivery";
                    }

                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteSend"))
                    {
                        mcCartCmd = "Send";
                    }
                    if (stdTools.ButtonSubmitted(ref myWeb.moRequest, "quoteQuit"))
                    {
                        mcCartCmd = "Quit";
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "checkButtons", ex, "", cProcessInfo, gbDebug);
                }

            }

            public override void apply()
            {
                // PerfMon.Log("Quote", "apply")
                // this function is the main function.

                XmlDocument oCartXML = base.moPageXml;
                XmlElement oContentElmt;
                XmlElement oElmt;

                Cms.xForm oPickContactXForm;

                string cProcessInfo = "";
                var bRedirect = default(bool);

                try
                {

                    oContentElmt = oCartXML.CreateElement("Cart");
                    oContentElmt.SetAttribute("type", "quote");
                    oContentElmt.SetAttribute("currency", mcCurrency);
                    oContentElmt.SetAttribute("currencySymbol", mcCurrencySymbol);
                    // If Not mbDisplayPrice Then oContentElmt.SetAttribute("displayPrice", mbDisplayPrice.ToString)

                    oElmt = oCartXML.CreateElement("Quote");

                    // if the cartCmd is not on a link but on a button
                    // we need to set the cartCmd dependant upon the button name
                    checkButtons();

                    if (!(mnCartId > 0))
                    {
                        // Cart doesn't exist - if the process flow has a valid command (except add or quit), then this is an error
                        switch (mcCartCmd)
                        {
                            case "Logon":
                            case "Remove":
                            case "Quote":
                            case "Form":
                            case "Billing":
                            case "Delivery":
                            case "ChoosePaymentShippingOption":
                            case "Confirm":
                            case "EnterPaymentDetails":
                            case "SubmitPaymentDetails":
                            case var @case when @case == "SubmitPaymentDetails":
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


                    // user can't change things if we are to show the invoice
                    // If mnProcessId = 6 And mcCartCmd <> "Quit" Then mcCartCmd = "QuoteComplete"

                    switch (mcCartCmd)
                    {
                        case "Update":
                            {
                                string quote = "Quote";
                                mcCartCmd = Convert.ToString(updateCart(ref quote));
                                goto processFlow;
                                
                            }

                        case "Remove": // take away an item and set the command to display the cart
                            {
                                if (RemoveItem() > 0)
                                {
                                    mcCartCmd = "Quote";
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
                                if (mnCartId < 1)
                                {
                                    myWeb.moSession["previousPage"] = "ADD";
                                    CreateNewCart(ref oElmt);
                                    if (mcItemOrderType != "")
                                    {
                                        mmcOrderType = mcItemOrderType;
                                    }
                                    else
                                    {
                                        mmcOrderType = "";
                                    }
                                    mnProcessId = 1;
                                }

                                if (!AddItems())
                                    mnProcessError = 2; // Error: The current item's order type does not match the cart's order type

                                // Case for if a items have been added from a giftlist
                                if (myWeb.moRequest["giftlistId"] != null)
                                {
                                    // AddDeliveryFromGiftList(myWeb.moRequest("giftlistId"))
                                }

                                // MsgBox AddItem
                                mcCartCmd = "Quote";
                                goto processFlow;
                                
                            }

                        // -------------------------------------------------------------------------------
                        // new process flow for Product Config Items--------------------------------------
                        case "addNoteLine":
                        case "removeNoteLine":
                        case "updateNoteLine":
                            {

                                if (mnCartId < 1 & mcCartCmd == "addNoteLine")
                                {
                                    CreateNewCart(ref oElmt);
                                    if (mcItemOrderType != "")
                                    {
                                        mmcOrderType = mcItemOrderType;
                                    }
                                    else
                                    {
                                        mmcOrderType = "";
                                    }
                                    mnProcessId = 1;
                                }
                                // Dim bdoRemove As Boolean = IIf(mcCartCmd = "removeNoteLine", True, False)
                                DoNotesItem(mcCartCmd);
                                // dont want to display cart though
                                // so going to cheat
                                // bFullCartOption = True
                                mcCartCmd = "Notes";
                                goto processFlow;
                                
                            }

                        // we do want to show the cart when we add a button
                        // GetCart(oElmt)
                        // -------------------------------------------------------------------------------


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
                                myWeb.moResponse.Redirect(mcSiteURL + mcReturnPage);
                                break;
                            }

                        case "Error":
                            {
                                GetCart(ref oElmt);
                                break;
                            }

                        case "Quote": // Choose Shipping Costs
                            {
                                if (mnProcessId > 3)
                                {
                                    // when everything is ready we can show the invoice screen
                                    mcCartCmd = "ShowInvoice";
                                    goto processFlow; // execute next step (make the payment)
                                }
                                // info to display the cart
                                GetCart(ref oElmt);
                                break;
                            }
                        case "Notes":
                            {
                                // If oElmt.GetAttribute("itemCount") = "0" Or oElmt.GetAttribute("itemCount") = "" Then
                                // mcCartCmd = "Quote"
                                // GoTo processFlow
                                // End If

                                myWeb.moSession["cLogonCmd"] = "";
                                if (mcNotesXForm != "")
                                {
                                    Cms.xForm oNotesXform = notesXform("notesForm", "?quoteCmd=Notes");
                                    if (oNotesXform.valid == false)
                                    {
                                        base.moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oNotesXform.moXformElmt);
                                        GetCart(ref oElmt);
                                    }
                                    else
                                    {
                                        // mcCartCmd = "Billing" or "Search" this is set in NotesXform
                                        goto processFlow;
                                    }
                                }
                                else
                                {
                                    mcCartCmd = "Billing";
                                    goto processFlow;
                                }

                                break;
                            }
                        case "Search":
                            {
                                Cms.xForm oNotesXform = notesXform("notesForm", "?quoteCmd=Notes");
                                base.moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oNotesXform.moXformElmt);
                                // [TS] take the notes xml and use an xslt to tranform it to a query xml as found in the form instance from xPathSearch in web
                                // remove any products on this page and return the search results
                                // change the xslt to add the "Search" step and include product search result.

                                string sXpath;
                                // Remove the current content here
                                string formRef = "";
                                xForm newXform = new xForm(ref formRef);
                                sXpath = newXform.getXpathFromQueryXml(oNotesXform.Instance, "/xsl/quote/search.xsl");
                                foreach (XmlNode oNode in base.moPageXml.SelectNodes("/Page/Contents/Content[@type='" + oNotesXform.Instance.SelectSingleNode("Query/@contentType").InnerText + "']"))
                                    oNode.ParentNode.RemoveChild(oNode);

                                // connect to Web for GetPageContentFromSelect
                                var oWeb = new Cms();
                                oWeb.moPageXml = base.moPageXml;
                                oWeb.InitializeVariables();
                                oWeb.Open();
                                if (oNotesXform.Instance.SelectSingleNode("Query/@contentType") is null)
                                {
                                    int argnCount = 0;
                                    XmlElement argoContentsNode = null;
                                    XmlElement argoPageDetail = null;
                                    oWeb.GetPageContentFromSelect(" CAST(cContentXmlDetail as xml).exist('" + sXpath + "') = 1", ref argnCount, ref argoContentsNode, ref argoPageDetail);
                                }
                                else
                                {
                                    int argnCount1 = 0;
                                    XmlElement argoContentsNode1 = null;
                                    XmlElement argoPageDetail1 = null;
                                    oWeb.GetPageContentFromSelect(" cContentSchemaName='" + oNotesXform.Instance.SelectSingleNode("Query/@contentType").InnerText + "' and CAST(cContentXmlDetail as xml).exist('" + sXpath + "') = 1", ref argnCount1, ref argoContentsNode1, ref argoPageDetail1);
                                }

                                GetCart(ref oElmt);
                                oWeb = null;
                                break;
                            }

                        case "Logon": // offer the user the ability to logon / register
                            {

                                if (mbEwMembership == true)
                                {

                                    // logon xform !!! We disable this because it is being brought in allready by .Web
                                    if (myWeb.mnUserId == 0)
                                    {
                                        // addtional string for membership to check
                                        myWeb.moSession["cLogonCmd"] = "quoteCmd=Logon";
                                        // registration xform
                                        ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                        IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, this.moConfig["MembershipProvider"]);
                                        IMembershipAdminXforms oRegXform = oMembershipProv.AdminXforms;

                                        XmlElement argIntanceAppend = null;
                                        oRegXform.xFrmEditDirectoryItem(IntanceAppend: ref argIntanceAppend,myWeb.mnUserId, "User",Convert.ToInt64("0" + moCartConfig["DefaultSubscriptionGroupId"]), "CartRegistration");
                                        if (oRegXform.valid)
                                        {
                                            string sReturn = base.moDBHelper.validateUser(myWeb.moRequest["cDirName"], myWeb.moRequest["cDirPassword"]);
                                            if (Information.IsNumeric(sReturn))
                                            {
                                                myWeb.mnUserId = Convert.ToInt32(sReturn);
                                                XmlElement oUserElmt = base.moDBHelper.GetUserXML(myWeb.mnUserId);
                                                base.moPageXml.DocumentElement.AppendChild(oUserElmt);
                                                myWeb.moSession["nUserId"] = myWeb.mnUserId;
                                                mcCartCmd = "Billing";
                                                goto processFlow;
                                            }
                                            else
                                            {
                                                oRegXform.addNote(oRegXform.moXformElmt.FirstChild.ToString(), Protean.xForm.noteTypes.Alert, sReturn);
                                                base.moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt);
                                            }
                                        }
                                        else
                                        {
                                            base.moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt);
                                            GetCart(ref oElmt);
                                        }


                                        oRegXform = null;
                                    }
                                    else
                                    {
                                        mcCartCmd = "Billing";
                                        goto processFlow;
                                    }
                                }
                                else
                                {
                                    mcCartCmd = "Billing";
                                    goto processFlow;
                                }

                                break;
                            }


                        case "Billing": // Check if order has Billing Address                
                            {

                                oPickContactXForm = pickContactXform("Billing Address", "quoteBill", "quoteCmd", "Billing");

                                if (oPickContactXForm.valid == false)
                                {

                                    // show the form

                                    base.moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oPickContactXForm.moXformElmt);
                                }
                                else
                                {
                                    // Valid Form, let's adjust the Vat rate
                                    string cContactCountry = oPickContactXForm.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText;
                                    UpdateTaxRate(ref cContactCountry);

                                    // Skip Delivery if:
                                    // - Deliver to this address is selected
                                    // - mbNoDeliveryAddress is True
                                    // - the order is part of a giftlist (the delivery address is pre-determined)
                                    if (myWeb.moRequest["cIsDelivery"] == "True" | mbNoDeliveryAddress | mnGiftListId > 0)
                                    {
                                        mcCartCmd = "ShowInvoice";
                                        mnProcessId = 3;
                                        oPickContactXForm = (Cms.xForm)null;
                                        goto processFlow;
                                    }
                                    else
                                    {
                                        mcCartCmd = "Delivery";
                                        // billing address is saved, so up cart status if needed
                                        if (mnProcessId < 2)
                                            mnProcessId = 2;
                                        // ^^^if we have delivery address already we can move to make payment^^^
                                        oPickContactXForm = (Cms.xForm)null;
                                        goto processFlow;
                                    }
                                }

                                GetCart(ref oElmt);
                                oPickContactXForm = (Cms.xForm)null;
                                break;
                            }

                        case "Delivery": // Check if order needs a Delivery Address
                            {

                                oPickContactXForm = pickContactXform("Delivery Address", "quoteDel", "quoteCmd", "Delivery");

                                if (oPickContactXForm.valid == false)
                                {

                                    base.moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oPickContactXForm.moXformElmt);
                                }

                                else
                                {

                                    // mcCartCmd = "ChoosePaymentShippingOption"
                                    mcCartCmd = "ShowInvoice";
                                    mnProcessId = 3;
                                    oPickContactXForm = (Cms.xForm)null;
                                    goto processFlow;

                                }

                                GetCart(ref oElmt);
                                oPickContactXForm = (Cms.xForm)null;
                                break;
                            }



                        case "ShowInvoice": // Payment confirmed / show invoice
                            {
                                if (mnProcessId != 10)
                                    mnProcessId = 6;
                                if (oElmt.FirstChild is null)
                                {
                                    GetCart(ref oElmt);
                                }

                                break;
                            }

                        case "Send":
                            {
                                GetCart(ref oElmt);
                                addDateAndRef(ref oElmt);
                                emailReceipts(ref oElmt);
                                oContentElmt.AppendChild(oElmt);
                                mnProcessId = 6; // ? "Payment Successful"... erm.. yeah
                                mcCartCmd = "Quit";
                                goto processFlow;
                                
                            }

                        case "MakeOrder":
                            {
                                QuoteToOrder();
                                break;
                            }


                        case "CookiesDisabled": // Cookies have been disabled or are undetectable
                            {
                                mnProcessError = 1;
                                GetCart(ref oElmt);
                                break;
                            }

                        case "BackToSite":
                            {
                                mcCartCmd = "";
                                GetCartSummary(ref oElmt);
                                break;
                            }

                        case "List":
                            {
                                int nI = 0;
                                if (!(myWeb.moRequest["OrderID"] == ""))
                                    nI =Convert.ToInt32(myWeb.moRequest["OrderID"]);                                
                                //ListOrders(nI, bListAllQuotes);
                                //GetCartSummary(oElmt);
                                mcCartCmd = "";
                                break;
                            }

                        case "MakeCurrent":
                            {
                                int nI = 0;
                                if (!(myWeb.moRequest["OrderID"] == ""))
                                    nI =Convert.ToInt32(myWeb.moRequest["OrderID"]);
                                if (!(nI == 0))
                                    MakeCurrent(nI);
                                mcCartCmd = "Quote";
                                goto processFlow;
                                
                            }
                        // GetCart(oElmt)

                        case "Save":
                            {
                                if (mbEwMembership == true)
                                {
                                    // logon xform !!! We disable this because it is being brought in allready by .Web
                                    if (myWeb.mnUserId == 0)
                                    {
                                        // registration xform
                                        ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                                        IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, this.moConfig["MembershipProvider"]);
                                        IMembershipAdminXforms oRegXform = oMembershipProv.AdminXforms;

                                        XmlElement argIntanceAppend1 = null;
                                        oRegXform.xFrmEditDirectoryItem(IntanceAppend: ref argIntanceAppend1,myWeb.mnUserId, "User",Convert.ToInt32("0" + moCartConfig["DefaultSubscriptionGroupId"]), "CartRegistration");
                                        if (oRegXform.valid)
                                        {
                                            string sReturn = base.moDBHelper.validateUser(myWeb.moRequest["cDirName"], myWeb.moRequest["cDirPassword"]);
                                            if (Information.IsNumeric(sReturn))
                                            {
                                                myWeb.mnUserId = Convert.ToInt32(sReturn);
                                            }
                                            else
                                            {
                                                oRegXform.addNote(oRegXform.moXformElmt.FirstChild.ToString(), Protean.xForm.noteTypes.Alert, sReturn);
                                                base.moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt);
                                            }
                                        }
                                        else
                                        {
                                            base.moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt);
                                            GetCart(ref oElmt);
                                        }
                                    }
                                    else
                                    {
                                        SaveQuote();
                                        mcCartCmd = "";
                                        goto processFlow;
                                    }
                                }
                                else
                                {
                                    mcCartCmd = "";
                                    goto processFlow;
                                }

                                break;
                            }

                        case "Delete":
                            {
                                int nI = 0;
                                if (!(myWeb.moRequest["OrderID"] == ""))
                                    nI = Convert.ToInt32(myWeb.moRequest["OrderID"]);
                                if (!(nI == 0))
                                    DeleteCart(nI);
                                mcCartCmd = "List";
                                goto processFlow; // Show Cart Summary                               
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
                    }
                    oContentElmt.SetAttribute("Process", mnProcessId.ToString());
                    oContentElmt.AppendChild(oElmt);
                    base.moPageXml.DocumentElement.AppendChild(oContentElmt);

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
                        stdTools.returnException(ref myWeb.msException, base.mcModuleName, "apply", ex, "", cProcessInfo, gbDebug);
                    }

                }
            }




            public override void emailReceipts(ref XmlElement oCartElmt)
            {
                // PerfMon.Log("Quote", "emailReceipts")
                string sMessageResponse;
                string cProcessInfo = "";
                var oElmtTemp = oCartElmt.OwnerDocument.CreateElement("Temp");
                oElmtTemp.InnerXml = oCartElmt.InnerXml;
                try
                {

                    // send to customer
                    sMessageResponse =Convert.ToString(emailCart(ref oCartElmt, moQuoteConfig["CustomerEmailTemplatePath"], moQuoteConfig["MerchantName"], moQuoteConfig["MerchantEmail"], oElmtTemp.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText, moQuoteConfig["OrderEmailSubject"]));

                    // Send to merchant
                    sMessageResponse = Convert.ToString(emailCart(ref oCartElmt, moQuoteConfig["MerchantEmailTemplatePath"], oElmtTemp.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText, oElmtTemp.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText, moQuoteConfig["MerchantEmail"], moQuoteConfig["OrderEmailSubject"]));

                    XmlElement oElmtEmail;
                    oElmtEmail = base.moPageXml.CreateElement("Reciept");
                    oCartElmt.AppendChild(oElmtEmail);
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

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "emailReceipts", ex, "", cProcessInfo, gbDebug);
                }

            }

            public override void EndSession()
            {
                // PerfMon.Log("Quote", "EndSession")
                string sProcessInfo = string.Empty;
                string sSql;
                string cProcessInfo = "";
                try
                {

                    sSql = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where(nCartOrderKey = " + mnCartId + ")";
                    base.moDBHelper.ExeProcessSql(sSql);
                    mmcOrderType = "";
                    mnCartId = 0;
                    myWeb.moSession["QuoteID"] = (object)null;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "EndSession", ex, "", cProcessInfo, gbDebug);
                }
            }

            public bool QuoteToOrder()
            {
                // PerfMon.Log("Quote", "QuoteToOrder")
                string cProcessInfo = "Quote to Order";
                try
                {
                    int nCurrentCart = Convert.ToInt32(myWeb.moSession["CartId"]);
                    var otmpcart = new Cms.Cart(ref myWeb);
                    if (nCurrentCart == 0)
                    {
                        // need a way to iniate the cart and then get the ID out
                        XmlElement xmlnothing = null;
                        nCurrentCart =Convert.ToInt32(otmpcart.CreateNewCart(ref xmlnothing));
                    }
                    int nQuoteId = mnCartId;

                    var oDS = new DataSet();
                    oDS = base.moDBHelper.GetDataSet("Select * From tblCartItem WHERE nCartOrderID = " + nQuoteId, "CartItems");
                    var nParentID = default(int);
                    string sSQL;
                    // okay, now we need the ID of the current cart
                    if (!(myWeb.moSession["oQID"].ToString() == ""))
                    {
                        nQuoteId = Convert.ToInt32(myWeb.moSession["oQID"]);
                    }
                    // pretty useless if the site doesnt have a cart
                    // if we dont have quotes then we'll never get here
                    if (!((base.moConfig["Quote"]).ToLower() == "on") | nQuoteId < 1)
                    {                        
                        // also better check the user owns this quote, we dont want them
                        // getting someone else quote by mucking around with the querystring
                        if (!(Convert.ToInt32(base.moDBHelper.ExeProcessSqlScalar("Select nCartUserDirId FROM tblCartOrder WHERE nCartOrderKey = " + nQuoteId + " AND cCartSchemaName = 'Quote'")) == mnEwUserId))
                        {
                            return false; // else we carry on
                        }
                        return false;
                    }
                    var bDelivery = default(bool);
                    var bBilling = default(bool);

                    // ok, going to do this very simply, in the database, 
                    // transferring all cartitems under the quote to the 
                    // current cart

                    foreach (DataRow oDR1 in oDS.Tables["CartItems"].Rows)
                    {
                        if (Conversions.ToInteger(Operators.ConcatenateObject("0", oDR1["nParentId"])) == 0)
                        {
                            sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " + "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " + "nTaxRate, nQuantity, nWeight, nAuditId) VALUES (";
                            sSQL += nCurrentCart + ",";
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
                            sSQL += base.moDBHelper.getAuditId() + ")";
                            nParentID = Convert.ToInt32(base.moDBHelper.GetIdInsertSql(sSQL));
                            // now for any children
                            foreach (DataRow oDR2 in oDS.Tables["CartItems"].Rows)
                            {
                                if (!(oDR2["nParentId"] is DBNull))
                                {
                                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(oDR2["nParentId"], oDR1["nCartItemKey"], false)))
                                    {
                                        sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " + "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " + "nTaxRate, nQuantity, nWeight, nAuditId) VALUES (";
                                        sSQL += nCurrentCart + ",";
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
                                        sSQL += base.moDBHelper.getAuditId() + ")";
                                        base.moDBHelper.GetIdInsertSql(sSQL);
                                        // now for any children
                                    }
                                }

                            }
                        }
                        // now to copy the addresses
                        oDS = base.moDBHelper.GetDataSet("Select * From tblCartContact WHERE nContactCartID = " + nQuoteId, "CartContacts");
                        string cNewSQL = "";
                        foreach (DataRow oDR in oDS.Tables["CartContacts"].Rows)
                        {
                            cNewSQL = "INSERT INTO tblCartContact ";
                            string cFields = "";
                            string cValues = "";
                            foreach (DataColumn odc in oDS.Tables["CartContacts"].Columns)
                            {
                                if (odc.ColumnName == "nContactKey" | odc.ColumnName == "cCartXml" | oDR[odc.ColumnName] is DBNull)
                                {
                                }
                                // do nothing
                                else if (odc.ColumnName == "nAuditId")
                                {
                                    cFields += "nContactCartID,";
                                    cValues += base.moDBHelper.getAuditId() + ",";
                                }
                                else if (odc.ColumnName == "nContactCartId")
                                {
                                    cFields += "nContactCartID,";
                                    cValues += nParentID + ",";
                                }
                                else if (Information.IsNumeric(oDR[odc.ColumnName]))
                                {
                                    cFields += odc.ColumnName + ",";
                                    cValues = Conversions.ToString(cValues + Operators.ConcatenateObject(oDR[odc.ColumnName], ","));
                                }
                                else
                                {
                                    cFields += odc.ColumnName + ",";
                                    cValues = Conversions.ToString(cValues + Operators.ConcatenateObject(Operators.ConcatenateObject("'", oDR[odc.ColumnName]), "',"));
                                }
                            }
                            cFields = Strings.Left(cFields, cFields.Length - 1);
                            cValues = Strings.Left(cValues, cValues.Length - 1);
                            cNewSQL += "(" + cFields + ") VALUES ";
                            cNewSQL += "(" + cValues + ")  ";
                            base.moDBHelper.GetIdInsertSql(cNewSQL);
                            if (cNewSQL.Contains("Delivery Address"))
                                bDelivery = true;
                            if (cNewSQL.Contains("Billing Address"))
                                bBilling = true;
                        }
                    }
                    myWeb.moSession["CartId"] = nCurrentCart;
                    if (otmpcart != null)
                    {
                        otmpcart.mnProcessId = 1;
                        otmpcart.mcCartCmd = "Cart";
                        if (bBilling)
                            otmpcart.mcCartCmd = "Delivery";
                        if (bDelivery)
                            otmpcart.mcCartCmd = "ChoosePaymentShippingOption";
                        otmpcart.apply();
                    }

                    // now we need to redirect somewhere?
                    // bRedirect = True
                    myWeb.moResponse.Redirect("/?cartCmd=Cart");
                    return true;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "QuoteToOrder", ex, "", cProcessInfo, gbDebug);
                    return false;
                }
            }

            public override void MakeCurrent(int nOrderID)
            {
                // PerfMon.Log("Quote", "MakeCurrent")
                // procedure to make a selected historical
                // order or quote into the currently active one

                var oDS = new DataSet();
                Cart otmpcart = (Cms.Cart)null;
                string cNotes = "";
                int nCheckUser = 0;
                try
                {

                    if (myWeb.mnUserId == 0)
                        return;
                    // Dim oDre As SqlDataReader = moDBHelper.getDataReader("Select nCartUserDirId, cClientNotes FROM tblCartOrder WHERE nCartOrderKey = " & nOrderID)
                    using (SqlDataReader oDre = base.moDBHelper.getDataReaderDisposable("Select nCartUserDirId, cClientNotes FROM tblCartOrder WHERE nCartOrderKey = " + nOrderID))  // Done by nita on 6/7/22
                    {
                        while (oDre.Read())
                        {
                            nCheckUser = Conversions.ToInteger(oDre.GetValue(0));
                            if (!oDre.IsDBNull(1))
                                cNotes = Conversions.ToString(oDre.GetValue(1));
                        }
                        oDre.Close();
                    }
                    if (!(nCheckUser == mnEwUserId))
                    {
                        return; // else we carry on
                    }
                    if (mnCartId == 0)
                    {
                        // create a new cart
                        otmpcart = new Quote(ref myWeb);
                        XmlElement xmlNull = null;
                        otmpcart.CreateNewCart(ref xmlNull);
                        mnCartId = otmpcart.mnCartId;
                    }
                    // now add the details to it

                    oDS = base.moDBHelper.GetDataSet("Select * From tblCartItem WHERE nCartOrderID = " + nOrderID, "CartItems");
                    int nParentID;
                    string sSQL;

                    base.moDBHelper.ReturnNullsEmpty(ref oDS);
                    sSQL = "Update tblCartOrder SET cClientNotes ='" + cNotes + "'  WHERE nCartOrderKey = " + mnCartId;
                    base.moDBHelper.ExeProcessSql(sSQL);
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
                            sSQL += base.moDBHelper.getAuditId() + ")";
                            nParentID =Convert.ToInt32(base.moDBHelper.GetIdInsertSql(sSQL));
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
                                    sSQL += base.moDBHelper.getAuditId() + ")";
                                    base.moDBHelper.GetIdInsertSql(sSQL);
                                    // now for any children
                                }
                            }
                        }
                    }
                }

                // If Not otmpcart Is Nothing Then
                // otmpcart.mnProcessId = 1
                // otmpcart.mcCartCmd = "Quote"
                // otmpcart.apply()
                // End If

                // now we need to redirect somewhere?
                // bRedirect = True
                // myWeb.moResponse.Redirect("?quoteCmd=Quote")
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "MakeCurrent", ex, "", "", gbDebug);
                }
            }

            public void SaveQuote()
            {
                // PerfMon.Log("Quote", "SaveQuote")
                // set the cart status to 7

                string sSql;
                string cProcessInfo = "";
                try
                {

                    sSql = "update tblCartOrder set nCartStatus = 7, nCartUserDirId = " + myWeb.mnUserId + " where(nCartOrderKey = " + mnCartId + ")";
                    base.moDBHelper.ExeProcessSql(sSql);
                    myWeb.moSession["QuoteId"] = (object)null;
                    mnCartId = 0;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, base.mcModuleName, "QuitCart", ex, "", cProcessInfo, gbDebug);
                }

            }
        }
    }
}