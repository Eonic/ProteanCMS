Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Web.HttpUtility
Imports VB = Microsoft.VisualBasic
Imports System
Imports Protean.Providers.Membership.EonicProvider

Partial Public Class Cms

    Public Class Quote
        Inherits Cart
#Region "Properties"
        Public Overrides Property OrderNoPrefix() As String
            Get
                If cOrderNoPrefix = "" Then
                    cOrderNoPrefix = moQuoteConfig("QuoteNoPrefix")
                End If
                Return cOrderNoPrefix
            End Get
            Set(ByVal value As String)
                cOrderNoPrefix = value
            End Set
        End Property
#End Region

        Dim mcCartSchemaName As String = "quote"
        Dim bListAllQuotes As Boolean = False
        Public moQuoteConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/quote")
        Private cOrderNoPrefix As String = ""


        Public Sub New(ByRef aWeb As Protean.Cms)
            MyBase.New(aWeb)


            Dim cProcessInfo As String = ""
            Try
                InitializeVariables()
            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "Close", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Private Overloads Sub InitializeVariables()
            PerfMon.Log("Quote", "InitializeVariables")
            'Author:        Trevor Spink
            'Copyright:     Eonic Ltd 2006
            'Date:          2006-10-04

            '   called at the beginning, whenever ewCart is run
            '   sets the global variables and initialises the current cart
            moCartConfig = WebConfigurationManager.GetWebApplicationSection("protean/quote")
            Dim sSql As String
            Dim oDr As SqlDataReader
            mcOrderType = "Quote"
            cOrderReference = ""
            mcModuleName = "Eonic.Quote"
            mmcOrderType = "Quote"
            Dim cProcessInfo As String = "" = "initialise variables"
            Try

                If mcPagePath = "" Then
                    mcPagePath = "?pgid=" & myWeb.mnPageId & "&"
                End If
                moDBHelper = myWeb.moDbHelper
                If moConfig("ewMembership") = "on" Then mbEwMembership = True
                mcNotesXForm = moQuoteConfig("NotesXForm")
                mcBillingAddressXform = moQuoteConfig("BillingAddressXForm")
                mcDeliveryAddressXform = moQuoteConfig("DeliveryAddressXForm")
                If Not moQuoteConfig("ListAllQuotes") Is Nothing Then
                    If moQuoteConfig("ListAllQuotes") = "on" Then
                        bListAllQuotes = True
                    End If
                End If

                mcSiteURL = moCartConfig("SiteURL")
                mcTermsAndConditions = moCartConfig("TermsAndConditions")
                'OrderNoPrefix = moQuoteConfig("QuoteNoPrefix")
                mcCurrencySymbol = moCartConfig("CurrencySymbol")
                mcCurrency = moCartConfig("Currency")
                If mcCurrency = "" Then mcCurrency = "GBP"

                If moCartConfig("NoDeliveryAddress") = "on" Then mbNoDeliveryAddress = True

                If myWeb.moRequest.Form("url") <> "" Then
                    myWeb.moSession("returnPage") = myWeb.moRequest.Form("url")
                End If
                If myWeb.moRequest.QueryString("url") <> "" Then
                    myWeb.moSession("returnPage") = myWeb.moRequest.QueryString("url")
                End If

                mcReturnPage = myWeb.moSession("returnPage")

                'If goApp("sCookieDomain") = "" Then
                If Not myWeb.moSession("nEwUserId") Is Nothing Then
                    mnEwUserId = CInt("0" & myWeb.moSession("nEwUserId"))
                Else
                    mnEwUserId = 0
                End If
                If myWeb.mnUserId > 0 And mnEwUserId = 0 Then mnEwUserId = myWeb.mnUserId
                'MEMB - eEDIT
                If myWeb.goApp("bFullCartOption") = True Then
                    bFullCartOption = True
                Else
                    bFullCartOption = False
                End If

                If myWeb.moSession("QuoteId") Is Nothing Then
                    mnCartId = 0
                Else
                    If Not (IsNumeric(myWeb.moSession("QuoteId"))) Or myWeb.moSession("QuoteId") <= 0 Then
                        mnCartId = 0
                    Else
                        mnCartId = CInt(myWeb.moSession("QuoteId"))
                    End If
                End If

                If Not (myWeb.moRequest("refSessionId") Is Nothing) Then
                    mcSessionId = myWeb.moRequest("refSessionId")
                    myWeb.moSession.Add("refSessionId", mcSessionId)
                ElseIf Not (myWeb.moSession("refSessionId") Is Nothing) Then
                    mcSessionId = myWeb.moSession("refSessionId")
                Else
                    mcSessionId = myWeb.moSession.SessionID
                End If

                If IsNumeric(myWeb.moRequest.QueryString("cartErr")) Then mnProcessError = CInt(myWeb.moRequest.QueryString("cartErr"))
                mcCartCmd = Nothing
                If myWeb.moRequest.QueryString("quoteCmd") <> "" Then
                    mcCartCmd = myWeb.moRequest.QueryString("quoteCmd")
                End If
                If mcCartCmd = "" Then
                    mcCartCmd = myWeb.moRequest.Form("quoteCmd")
                End If


                mmcOrderType = myWeb.moSession("mmcOrderType")
                mcItemOrderType = myWeb.moRequest.Form("ordertype")

                '  MsgBox "Item: " & mcItemOrderType & vbCrLf & "Order: " & mmcOrderType
                '  set global variable for submit button

                mcSubmitText = myWeb.moRequest("submit")

                If mnCartId > 0 Then
                    '   cart exists
                    sSql = "select * from tblCartOrder where (nCartStatus < 7 or nCartStatus = 10) and nCartOrderKey = " & mnCartId & " and not(cCartSessionId like 'OLD_%')"

                    oDr = moDBHelper.getDataReader(sSql)
                    If oDr.HasRows Then
                        While oDr.Read
                            mnGiftListId = oDr("nGiftListId")
                            mnTaxRate = CDbl("0" & oDr("nTaxRate"))
                            mnProcessId = CLng("0" & oDr("nCartStatus"))
                        End While
                    Else
                        ' Cart no longer exists - a quit command has probably been issued.  Clear the session
                        mnCartId = 0
                        mnProcessId = 0
                        mcCartCmd = ""
                    End If
                    oDr.Close()
                    If mnCartId = 0 Then
                        EndSession()
                    End If
                Else
                    ' Cart doesn't exist - check if it can be found in the database, although only run this check if we know that we've visited the cart
                    ' Also check out if this is coming from a Worldpay callback.
                    If Not (myWeb.moRequest("refSessionId") Is Nothing) Or Not (myWeb.moRequest("transStatus") Is Nothing) Or Not (myWeb.moRequest("settlementRef") Is Nothing) Then
                        If Not (myWeb.moRequest("transStatus") Is Nothing) Then
                            sSql = "select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId  where o.cCartSchemaName='cart' and o.nCartOrderKey=" & myWeb.moRequest("cartId") & " and DATEDIFF(hh,a.dInsertDate,GETDATE())<24"
                            'mcPaymentMethod = "WorldPay"
                        ElseIf Not (myWeb.moRequest("settlementRef") Is Nothing) Then
                            ' Go get the cart, restore settings
                            sSql = "select * from tblCartOrder where cCartSchemaName='cart' and cSettlementID='" & myWeb.moRequest("settlementRef") & "'"
                        Else
                            sSql = "select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId where o.cCartSchemaName='cart' and o.cCartSessionId = '" & SqlFmt(mcSessionId) & "' and DATEDIFF(hh,a.dInsertDate,GETDATE())<24"
                        End If
                        oDr = moDBHelper.getDataReader(sSql)
                        If oDr.HasRows Then
                            While oDr.Read
                                mnGiftListId = oDr("nGiftListId")
                                mnCartId = oDr("nCartOrderKey") ' get cart id
                                mnProcessId = oDr("nCartStatus") ' get cart status
                                mnTaxRate = oDr("nTaxRate")
                                If Not (myWeb.moRequest("settlementRef") Is Nothing) Then

                                    ' Set eh commands for a settlement
                                    mcSubmitText = "Go To Checkout"
                                    mnProcessId = 5

                                    ' If a cart has been found, we need to update the session ID in it.
                                    If oDr("cCartSessionId") <> mcSessionId Then
                                        moDBHelper.ExeProcessSql("update tblCartOrder set cCartSessionId = '" & mcSessionId & "' where nCartOrderKey = " & mnCartId)
                                    End If

                                    ' Reactivate the order in the database
                                    moDBHelper.ExeProcessSql("update tblCartOrder set nCartStatus = '" & mnProcessId & "' where nCartOrderKey = " & mnCartId)

                                End If
                                If mnProcessId > 5 Then
                                    ' Cart has passed a status of "Succeeded" - we can't do anything to this cart. Clear the session.
                                    EndSession()
                                    mnCartId = 0
                                    mnProcessId = 0
                                    mcCartCmd = ""
                                End If
                            End While
                            oDr.Close()
                        End If
                    End If

                End If

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "Open", ex, "", cProcessInfo, gbDebug)
                'close()
            Finally
                oDr = Nothing
            End Try
        End Sub

        Public Shadows Sub close()
            PerfMon.Log("Quote", "close")
            Dim cProcessInfo As String = ""
            Try
                PersistVariables()
                MyBase.close()

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "Close", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Overrides Sub PersistVariables()
            PerfMon.Log("Quote", "PersistVariables")
            'Author:        Trevor Spink
            'Copyright:     Eonic Ltd 2003
            'Date:          2003-02-01

            'called at the end of the main procedure (ewCartPlugin())
            'holds variable values after cart module ends for use next time it starts
            'they are stored in either a session attribute or in the database

            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try

                cProcessInfo = "set session variables" '   persist global sProcessInfo

                If myWeb.moSession("QuoteId") Is Nothing Then
                    myWeb.moSession.Add("QuoteId", CStr(mnCartId))
                Else
                    myWeb.moSession("QuoteId") = CStr(mnCartId)
                End If
                '    oResponse.Cookies(mcSiteURL & "CartId").Domain = mcSiteURL
                '    oSession("nCartOrderId") = mnCartId    '   session attribute holds Cart ID

                If mnCartId > 0 Then
                    '   If we have a cart, update its status in the db

                    sSql = "update tblCartOrder set nCartStatus = " & mnProcessId & ", nGiftListId = " & mnGiftListId & " where nCartOrderKey = " & mnCartId

                    'update the auditId

                    moDBHelper.ExeProcessSql(sSql)
                End If

                myWeb.moSession("nProcessId") = mnProcessId '   persist global mnProcessId
                myWeb.moSession("mmcOrderType") = mmcOrderType
                myWeb.moSession("nTaxRate") = mnTaxRate

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "PersistVariables", ex, "", cProcessInfo, gbDebug)
                'close()
            End Try
        End Sub

        Overrides Sub checkButtons()
            PerfMon.Log("Quote", "checkButtons")
            Dim cProcessInfo As String = ""
            Try
                'legacy button handling looking at button values rather than names, should not be required soon
                'Select Case mcSubmitText
                '    Case "Goto Checkout", "Send Quote Request"
                '        mcCartCmd = "Logon"
                '        'old for the cart
                '        'updateCart("RedirectSecure")
                '        'mcCartCmd = "RedirectSecure"

                '    Case "Edit Billing Details", "Proceed without Logon"
                '        mcCartCmd = "Billing"
                '    Case "Edit Delivery Details"
                '        mcCartCmd = "Delivery"
                '        'Case "Confirm Order", "Proceed with Order", "Proceed"
                '        'updateCart("ChoosePaymentShippingOption")

                '    Case "Update Quote", "Update Order"
                '        updateCart("Quote")
                '    Case "Empty Quote", "Empty Order"
                '        mcCartCmd = "Quit"
                '        'Case "Make Secure Payment"
                '        'updateCart(mcCartCmd)
                '    Case "Continue Shopping"
                '        mcCartCmd = "BackToSite"
                'End Select

                'New button handling based on the name of the button rather than the value
                If ButtonSubmitted(myWeb.moRequest, "quoteAdd") Then
                    mcCartCmd = "Add"
                End If
                If ButtonSubmitted(myWeb.moRequest, "quoteDetail") Then
                    mcCartCmd = "Quote"
                End If
                If ButtonSubmitted(myWeb.moRequest, "quoteUpdate") Then
                    mcCartCmd = "Update"
                End If
                If ButtonSubmitted(myWeb.moRequest, "quoteBrief") Then
                    mcCartCmd = "BackToSite"
                End If
                If ButtonSubmitted(myWeb.moRequest, "quoteNotes") Then
                    mcCartCmd = "Notes"
                End If
                If ButtonSubmitted(myWeb.moRequest, "quoteSearch") Then
                    mcCartCmd = "Search"
                End If

                If ButtonSubmitted(myWeb.moRequest, "quoteLogon") Or ButtonSubmitted(myWeb.moRequest, "quoteRegister") Or ButtonSubmitted(myWeb.moRequest, "ewRegSubmit") Then
                    mcCartCmd = "Logon"
                End If

                ' Pick Address Buttions
                If ButtonSubmitted(myWeb.moRequest, "quoteBillAddress") Or ButtonSubmitted(myWeb.moRequest, "quoteBillcontact") Or ButtonSubmitted(myWeb.moRequest, "quoteBilladdNewAddress") Or ButtonSubmitted(myWeb.moRequest, "quoteBilleditAddress") Then
                    mcCartCmd = "Billing"
                End If
                If ButtonSubmitted(myWeb.moRequest, "quoteDelAddress") Or ButtonSubmitted(myWeb.moRequest, "quoteDelcontact") Or ButtonSubmitted(myWeb.moRequest, "quoteDeladdNewAddress") Or ButtonSubmitted(myWeb.moRequest, "quoteDeleditAddress") Then
                    mcCartCmd = "Delivery"
                End If

                If ButtonSubmitted(myWeb.moRequest, "quoteSend") Then
                    mcCartCmd = "Send"
                End If
                If ButtonSubmitted(myWeb.moRequest, "quoteQuit") Then
                    mcCartCmd = "Quit"
                End If

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "checkButtons", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Overrides Sub apply()
            PerfMon.Log("Quote", "apply")
            '   this function is the main function.

            Dim oCartXML As XmlDocument = moPageXml
            Dim oContentElmt As XmlElement
            Dim oElmt As XmlElement

            Dim oPickContactXForm As xForm

            Dim cProcessInfo As String = ""
            Dim bRedirect As Boolean

            Try

                oContentElmt = oCartXML.CreateElement("Cart")
                oContentElmt.SetAttribute("type", "quote")
                oContentElmt.SetAttribute("currency", mcCurrency)
                oContentElmt.SetAttribute("currencySymbol", mcCurrencySymbol)
                'If Not mbDisplayPrice Then oContentElmt.SetAttribute("displayPrice", mbDisplayPrice.ToString)

                oElmt = oCartXML.CreateElement("Quote")

                '   if the cartCmd is not on a link but on a button
                '   we need to set the cartCmd dependant upon the button name
                checkButtons()

                If Not (mnCartId > 0) Then
                    ' Cart doesn't exist - if the process flow has a valid command (except add or quit), then this is an error
                    Select Case mcCartCmd
                        Case "Logon", "Remove", "Quote", "Form", "Billing", "Delivery", "ChoosePaymentShippingOption", "Confirm", "EnterPaymentDetails", "SubmitPaymentDetails", "SubmitPaymentDetails", "ShowInvoice", "ShowCallBackInvoice"
                            mcCartCmd = "CookiesDisabled"
                        Case "Error"
                            mcCartCmd = "Error"
                    End Select

                End If

                'Cart Process

processFlow:

                '  user can't change things if we are to show the invoice
                'If mnProcessId = 6 And mcCartCmd <> "Quit" Then mcCartCmd = "QuoteComplete"

                Select Case mcCartCmd
                    Case "Update"
                        mcCartCmd = updateCart("Quote")
                        GoTo processFlow

                    Case "Remove" '   take away an item and set the command to display the cart
                        If RemoveItem() > 0 Then
                            mcCartCmd = "Quote"
                        Else
                            ' RemoveItem has removed the last item in the cart - quit the cart.
                            mcCartCmd = "Quit"
                        End If
                        GoTo processFlow

                    Case "Add" '   add an item to the cart, if its a new cart we must initialise it and change its status
                        If mnCartId < 1 Then
                            myWeb.moSession("previousPage") = "ADD"
                            CreateNewCart(oElmt)
                            If mcItemOrderType <> "" Then
                                mmcOrderType = mcItemOrderType
                            Else
                                mmcOrderType = ""
                            End If
                            mnProcessId = 1
                        End If

                        If Not AddItems() Then mnProcessError = 2 ' Error: The current item's order type does not match the cart's order type

                        'Case for if a items have been added from a giftlist
                        If Not myWeb.moRequest("giftlistId") Is Nothing Then
                            'AddDeliveryFromGiftList(myWeb.moRequest("giftlistId"))
                        End If

                        'MsgBox AddItem
                        mcCartCmd = "Quote"
                        GoTo processFlow

                        '-------------------------------------------------------------------------------
                        'new process flow for Product Config Items--------------------------------------
                    Case "addNoteLine", "removeNoteLine", "updateNoteLine"

                        If mnCartId < 1 And mcCartCmd = "addNoteLine" Then
                            CreateNewCart(oElmt)
                            If mcItemOrderType <> "" Then
                                mmcOrderType = mcItemOrderType
                            Else
                                mmcOrderType = ""
                            End If
                            mnProcessId = 1
                        End If
                        'Dim bdoRemove As Boolean = IIf(mcCartCmd = "removeNoteLine", True, False)
                        DoNotesItem(mcCartCmd)
                        'dont want to display cart though
                        'so going to cheat
                        'bFullCartOption = True
                        mcCartCmd = "Notes"
                        GoTo processFlow

                        'we do want to show the cart when we add a button
                        'GetCart(oElmt)
                        '-------------------------------------------------------------------------------


                    Case "Quit"
                        '   action depends on whether order is complete or not
                        If mnProcessId = 6 Or mnProcessId = 10 Then
                            ' QuitCart()
                            EndSession()
                            mcCartCmd = ""
                            mnCartId = 0
                            mnProcessId = 0
                        Else
                            QuitCart()
                            EndSession()
                            mnProcessId = 0
                            If bFullCartOption = True Then
                                GetCart(oElmt)
                            Else
                                GetCartSummary(oElmt)
                            End If
                            mnCartId = 0
                        End If
                        'return to site
                        bRedirect = True
                        myWeb.moResponse.Redirect(mcSiteURL & mcReturnPage)

                    Case "Error"
                        GetCart(oElmt)

                    Case "Quote" 'Choose Shipping Costs
                        If mnProcessId > 3 Then
                            '   when everything is ready we can show the invoice screen
                            mcCartCmd = "ShowInvoice"
                            GoTo processFlow '   execute next step (make the payment)
                        End If
                        '   info to display the cart
                        GetCart(oElmt)
                    Case "Notes"
                        'If oElmt.GetAttribute("itemCount") = "0" Or oElmt.GetAttribute("itemCount") = "" Then
                        '    mcCartCmd = "Quote"
                        '    GoTo processFlow
                        'End If

                        myWeb.moSession("cLogonCmd") = ""
                        If mcNotesXForm <> "" Then
                            Dim oNotesXform As xForm = notesXform("notesForm", "?quoteCmd=Notes")
                            If oNotesXform.valid = False Then
                                moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oNotesXform.moXformElmt)
                                GetCart(oElmt)
                            Else
                                ' mcCartCmd = "Billing" or "Search" this is set in NotesXform
                                GoTo processFlow
                            End If
                        Else
                            mcCartCmd = "Billing"
                            GoTo processFlow
                        End If
                    Case "Search"
                        Dim oNotesXform As xForm = notesXform("notesForm", "?quoteCmd=Notes")
                        moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oNotesXform.moXformElmt)
                        ' [TS] take the notes xml and use an xslt to tranform it to a query xml as found in the form instance from xPathSearch in web
                        'remove any products on this page and return the search results
                        'change the xslt to add the "Search" step and include product search result.

                        Dim sXpath As String
                        sXpath = getXpathFromQueryXml(oNotesXform.Instance, "/xsl/quote/search.xsl")
                        'Remove the current content here
                        Dim oNode As XmlNode
                        For Each oNode In moPageXml.SelectNodes("/Page/Contents/Content[@type='" & oNotesXform.Instance.SelectSingleNode("Query/@contentType").InnerText & "']")
                            oNode.ParentNode.RemoveChild(oNode)
                        Next

                        'connect to Web for GetPageContentFromSelect
                        Dim oWeb As Protean.Cms = New Cms
                        oWeb.moPageXml = moPageXml
                        oWeb.InitializeVariables()
                        oWeb.Open()
                        If oNotesXform.Instance.SelectSingleNode("Query/@contentType") Is Nothing Then
                            oWeb.GetPageContentFromSelect(" CAST(cContentXmlDetail as xml).exist('" & sXpath & "') = 1")
                        Else
                            oWeb.GetPageContentFromSelect(" cContentSchemaName='" & oNotesXform.Instance.SelectSingleNode("Query/@contentType").InnerText & "' and CAST(cContentXmlDetail as xml).exist('" & sXpath & "') = 1")
                        End If

                        GetCart(oElmt)
                        oWeb = Nothing

                    Case "Logon" ' offer the user the ability to logon / register

                        If mbEwMembership = True Then

                            'logon xform !!! We disable this because it is being brought in allready by .Web
                            If myWeb.mnUserId = 0 Then
                                'addtional string for membership to check
                                myWeb.moSession("cLogonCmd") = "quoteCmd=Logon"
                                'registration xform
                                Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                                Dim oRegXform As AdminXForms = oMembershipProv.AdminXforms
                                oRegXform.xFrmEditDirectoryItem(myWeb.mnUserId, "User", CInt("0" & moCartConfig("DefaultSubscriptionGroupId")), "CartRegistration")
                                If oRegXform.valid Then
                                    Dim sReturn As String = moDBHelper.validateUser(myWeb.moRequest("cDirName"), myWeb.moRequest("cDirPassword"))
                                    If IsNumeric(sReturn) Then
                                        myWeb.mnUserId = CLng(sReturn)
                                        Dim oUserElmt As XmlElement = moDBHelper.GetUserXML(myWeb.mnUserId)
                                        moPageXml.DocumentElement.AppendChild(oUserElmt)
                                        myWeb.moSession("nUserId") = myWeb.mnUserId
                                        mcCartCmd = "Billing"
                                        GoTo processFlow
                                    Else
                                        oRegXform.addNote(oRegXform.moXformElmt.FirstChild, xForm.noteTypes.Alert, sReturn)
                                        moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt)
                                    End If
                                Else
                                    moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt)
                                    GetCart(oElmt)
                                End If


                                oRegXform = Nothing
                            Else
                                mcCartCmd = "Billing"
                                GoTo processFlow
                            End If
                        Else
                            mcCartCmd = "Billing"
                            GoTo processFlow
                        End If


                    Case "Billing" 'Check if order has Billing Address                

                        oPickContactXForm = pickContactXform("Billing Address", "quoteBill", "quoteCmd", "Billing")

                        If oPickContactXForm.valid = False Then

                            'show the form

                            moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oPickContactXForm.moXformElmt)
                        Else
                            ' Valid Form, let's adjust the Vat rate
                            UpdateTaxRate(oPickContactXForm.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText)

                            ' Skip Delivery if:
                            '  - Deliver to this address is selected
                            '  - mbNoDeliveryAddress is True
                            '  - the order is part of a giftlist (the delivery address is pre-determined)
                            If myWeb.moRequest("cIsDelivery") = "True" Or mbNoDeliveryAddress Or mnGiftListId > 0 Then
                                mcCartCmd = "ShowInvoice"
                                mnProcessId = 3
                                oPickContactXForm = Nothing
                                GoTo processFlow
                            Else
                                mcCartCmd = "Delivery"
                                '   billing address is saved, so up cart status if needed
                                If mnProcessId < 2 Then mnProcessId = 2
                                '^^^if we have delivery address already we can move to make payment^^^
                                oPickContactXForm = Nothing
                                GoTo processFlow
                            End If
                        End If

                        GetCart(oElmt)
                        oPickContactXForm = Nothing

                    Case "Delivery" 'Check if order needs a Delivery Address

                        oPickContactXForm = pickContactXform("Delivery Address", "quoteDel", "quoteCmd", "Delivery")

                        If oPickContactXForm.valid = False Then

                            moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oPickContactXForm.moXformElmt)

                        Else

                            'mcCartCmd = "ChoosePaymentShippingOption"
                            mcCartCmd = "ShowInvoice"
                            mnProcessId = 3
                            oPickContactXForm = Nothing
                            GoTo processFlow

                        End If

                        GetCart(oElmt)
                        oPickContactXForm = Nothing



                    Case "ShowInvoice" 'Payment confirmed / show invoice
                        If mnProcessId <> 10 Then mnProcessId = 6
                        If oElmt.FirstChild Is Nothing Then
                            GetCart(oElmt)
                        End If

                    Case "Send"
                        GetCart(oElmt)
                        addDateAndRef(oElmt)
                        emailReceipts(oElmt)
                        oContentElmt.AppendChild(oElmt)
                        mnProcessId = 6 '? "Payment Successful"... erm.. yeah
                        mcCartCmd = "Quit"
                        GoTo processFlow

                    Case "MakeOrder"
                        QuoteToOrder()


                    Case "CookiesDisabled" ' Cookies have been disabled or are undetectable
                        mnProcessError = 1
                        GetCart(oElmt)

                    Case "BackToSite"
                        mcCartCmd = ""
                        GetCartSummary(oElmt)

                    Case "List"
                        Dim nI As Integer = 0
                        If Not myWeb.moRequest.Item("OrderID") = "" Then nI = myWeb.moRequest.Item("OrderID")
                        ListOrders(nI, bListAllQuotes)
                        GetCartSummary(oElmt)
                        mcCartCmd = ""

                    Case "MakeCurrent"
                        Dim nI As Integer = 0
                        If Not myWeb.moRequest.Item("OrderID") = "" Then nI = myWeb.moRequest.Item("OrderID")
                        If Not nI = 0 Then MakeCurrent(nI)
                        mcCartCmd = "Quote"
                        GoTo processFlow
                        'GetCart(oElmt)

                    Case "Save"
                        If mbEwMembership = True Then
                            'logon xform !!! We disable this because it is being brought in allready by .Web
                            If myWeb.mnUserId = 0 Then
                                'registration xform
                                Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                                Dim oRegXform As AdminXForms = oMembershipProv.AdminXforms
                                oRegXform.xFrmEditDirectoryItem(myWeb.mnUserId, "User", CInt("0" & moCartConfig("DefaultSubscriptionGroupId")), "CartRegistration")
                                If oRegXform.valid Then
                                    Dim sReturn As String = moDBHelper.validateUser(myWeb.moRequest("cDirName"), myWeb.moRequest("cDirPassword"))
                                    If IsNumeric(sReturn) Then
                                        myWeb.mnUserId = CLng(sReturn)
                                    Else
                                        oRegXform.addNote(oRegXform.moXformElmt.FirstChild, xForm.noteTypes.Alert, sReturn)
                                        moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt)
                                    End If
                                Else
                                    moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oRegXform.moXformElmt)
                                    GetCart(oElmt)
                                End If
                            Else
                                SaveQuote()
                                mcCartCmd = ""
                                GoTo processFlow
                            End If
                        Else
                            mcCartCmd = ""
                            GoTo processFlow
                        End If

                    Case "Delete"
                        Dim nI As Integer = 0
                        If Not myWeb.moRequest.Item("OrderID") = "" Then nI = myWeb.moRequest.Item("OrderID")
                        If Not nI = 0 Then DeleteCart(nI)
                        mcCartCmd = "List"
                        GoTo processFlow



                    Case Else ' Show Cart Summary
                        mcCartCmd = ""
                        If bFullCartOption = True Then
                            GetCart(oElmt)
                        Else
                            GetCartSummary(oElmt)
                        End If

                End Select

                PersistVariables() '   store data for next time this function runs

                If Not (oElmt Is Nothing) Then
                    oElmt.SetAttribute("cmd", mcCartCmd)
                    oElmt.SetAttribute("sessionId", CStr(mcSessionId))
                End If
                oContentElmt.SetAttribute("Process", mnProcessId)
                oContentElmt.AppendChild(oElmt)
                moPageXml.DocumentElement.AppendChild(oContentElmt)

                Exit Sub

            Catch ex As Exception
                If bRedirect = True And ex.GetType() Is GetType(System.Threading.ThreadAbortException) Then
                    'mbRedirect = True
                    'do nothing
                Else
                    returnException(myWeb.msException, mcModuleName, "apply", ex, "", cProcessInfo, gbDebug)
                End If

            End Try
        End Sub




        Overrides Sub emailReceipts(ByRef oCartElmt As XmlElement)
            PerfMon.Log("Quote", "emailReceipts")
            Dim sMessageResponse As String
            Dim cProcessInfo As String = ""
            Dim oElmtTemp As XmlElement = oCartElmt.OwnerDocument.CreateElement("Temp")
            oElmtTemp.InnerXml = oCartElmt.InnerXml
            Try

                'send to customer
                sMessageResponse = emailCart(oCartElmt, moQuoteConfig("CustomerEmailTemplatePath"), moQuoteConfig("MerchantName"), moQuoteConfig("MerchantEmail"), (oElmtTemp.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText), moQuoteConfig("OrderEmailSubject"))

                'Send to merchant
                sMessageResponse = emailCart(oCartElmt, moQuoteConfig("MerchantEmailTemplatePath"), (oElmtTemp.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText), (oElmtTemp.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText), moQuoteConfig("MerchantEmail"), moQuoteConfig("OrderEmailSubject"))

                Dim oElmtEmail As XmlElement
                oElmtEmail = moPageXml.CreateElement("Reciept")
                oCartElmt.AppendChild(oElmtEmail)
                oElmtEmail.InnerText = sMessageResponse

                If sMessageResponse = "Message Sent" Then
                    oElmtEmail.SetAttribute("status", "sent")
                Else
                    oElmtEmail.SetAttribute("status", "failed")
                End If

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "emailReceipts", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Overrides Sub EndSession()
            PerfMon.Log("Quote", "EndSession")
            Dim sProcessInfo As String = ""
            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try

                sSql = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where(nCartOrderKey = " & mnCartId & ")"
                moDBHelper.ExeProcessSql(sSql)
                mmcOrderType = ""
                mnCartId = 0
                myWeb.moSession("QuoteID") = Nothing

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "EndSession", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Function QuoteToOrder() As Boolean
            PerfMon.Log("Quote", "QuoteToOrder")
            Dim cProcessInfo As String = "Quote to Order"
            Try
                Dim nCurrentCart As Integer = myWeb.moSession("CartId")
                Dim otmpcart As New Cart(myWeb)
                If nCurrentCart = 0 Then
                    'need a way to iniate the cart and then get the ID out

                    nCurrentCart = otmpcart.CreateNewCart(Nothing)
                End If
                Dim nQuoteId As Integer = mnCartId

                Dim oDS As New DataSet
                oDS = moDBHelper.GetDataSet("Select * From tblCartItem WHERE nCartOrderID = " & nQuoteId, "CartItems")
                Dim oDR1 As DataRow
                Dim oDR2 As DataRow
                Dim nParentID As Integer
                Dim sSQL As String
                'okay, now we need the ID of the current cart
                If Not myWeb.moSession.Item("oQID") = "" Then
                    nQuoteId = myWeb.moSession.Item("oQID")
                End If
                'pretty useless if the site doesnt have a cart
                'if we dont have quotes then we'll never get here
                If Not LCase(moConfig("Quote")) = "on" Or nQuoteId < 1 Then
                    Return False
                    'also better check the user owns this quote, we dont want them
                    'getting someone else quote by mucking around with the querystring
                    If Not moDBHelper.ExeProcessSqlScalar("Select nCartUserDirId FROM tblCartOrder WHERE nCartOrderKey = " & nQuoteId & " AND cCartSchemaName = 'Quote'") = mnEwUserId Then
                        Return False 'else we carry on
                    End If
                End If
                Dim bDelivery As Boolean
                Dim bBilling As Boolean

                'ok, going to do this very simply, in the database, 
                'transferring all cartitems under the quote to the 
                'current cart

                For Each oDR1 In oDS.Tables("CartItems").Rows
                    If CInt("0" & oDR1("nParentId")) = 0 Then
                        sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " &
                        "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " &
                        "nTaxRate, nQuantity, nWeight, nAuditId) VALUES ("
                        sSQL &= nCurrentCart & ","
                        sSQL &= IIf(IsDBNull(oDR1("nItemId")), "Null", oDR1("nItemId")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nParentId")), "Null", oDR1("nParentId")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("cItemRef")), "Null", "'" & oDR1("cItemRef")) & "',"
                        sSQL &= IIf(IsDBNull(oDR1("cItemURL")), "Null", "'" & oDR1("cItemURL")) & "',"
                        sSQL &= IIf(IsDBNull(oDR1("cItemName")), "Null", "'" & oDR1("cItemName")) & "',"
                        sSQL &= IIf(IsDBNull(oDR1("nItemOptGrpIdx")), "Null", oDR1("nItemOptGrpIdx")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nItemOptIdx")), "Null", oDR1("nItemOptIdx")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nPrice")), "Null", oDR1("nPrice")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nShpCat")), "Null", oDR1("nShpCat")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nDiscountCat")), "Null", oDR1("nDiscountCat")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nDiscountValue")), "Null", oDR1("nDiscountValue")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nTaxRate")), "Null", oDR1("nTaxRate")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nQuantity")), "Null", oDR1("nQuantity")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nWeight")), "Null", oDR1("nWeight")) & ","
                        sSQL &= moDBHelper.getAuditId() & ")"
                        nParentID = moDBHelper.GetIdInsertSql(sSQL)
                        'now for any children
                        For Each oDR2 In oDS.Tables("CartItems").Rows
                            If Not IsDBNull(oDR2("nParentId")) Then
                                If oDR2("nParentId") = oDR1("nCartItemKey") Then
                                    sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " &
                                    "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " &
                                    "nTaxRate, nQuantity, nWeight, nAuditId) VALUES ("
                                    sSQL &= nCurrentCart & ","
                                    sSQL &= IIf(IsDBNull(oDR2("nItemId")), "Null", oDR2("nItemId")) & ","
                                    sSQL &= nParentID & ","
                                    sSQL &= IIf(IsDBNull(oDR2("cItemRef")), "Null", "'" & oDR2("cItemRef")) & "',"
                                    sSQL &= IIf(IsDBNull(oDR2("cItemURL")), "Null", "'" & oDR2("cItemURL")) & "',"
                                    sSQL &= IIf(IsDBNull(oDR2("cItemName")), "Null", "'" & oDR2("cItemName")) & "',"
                                    sSQL &= IIf(IsDBNull(oDR2("nItemOptGrpIdx")), "Null", oDR2("nItemOptGrpIdx")) & ","
                                    sSQL &= IIf(IsDBNull(oDR2("nItemOptIdx")), "Null", oDR2("nItemOptIdx")) & ","
                                    sSQL &= IIf(IsDBNull(oDR2("nPrice")), "Null", oDR2("nPrice")) & ","
                                    sSQL &= IIf(IsDBNull(oDR2("nShpCat")), "Null", oDR2("nShpCat")) & ","
                                    sSQL &= IIf(IsDBNull(oDR2("nDiscountCat")), "Null", oDR2("nDiscountCat")) & ","
                                    sSQL &= IIf(IsDBNull(oDR2("nDiscountValue")), "Null", oDR2("nDiscountValue")) & ","
                                    sSQL &= IIf(IsDBNull(oDR2("nTaxRate")), "Null", oDR2("nTaxRate")) & ","
                                    sSQL &= IIf(IsDBNull(oDR2("nQuantity")), "Null", oDR2("nQuantity")) & ","
                                    sSQL &= IIf(IsDBNull(oDR2("nWeight")), "Null", oDR2("nWeight")) & ","
                                    sSQL &= moDBHelper.getAuditId() & ")"
                                    moDBHelper.GetIdInsertSql(sSQL)
                                    'now for any children
                                End If
                            End If

                        Next
                    End If
                    'now to copy the addresses
                    oDS = moDBHelper.GetDataSet("Select * From tblCartContact WHERE nContactCartID = " & nQuoteId, "CartContacts")
                    Dim cNewSQL As String = ""
                    Dim oDR As DataRow
                    Dim odc As DataColumn
                    For Each oDR In oDS.Tables("CartContacts").Rows
                        cNewSQL = "INSERT INTO tblCartContact "
                        Dim cFields As String = ""
                        Dim cValues As String = ""
                        For Each odc In oDS.Tables("CartContacts").Columns
                            If odc.ColumnName = "nContactKey" Or odc.ColumnName = "cCartXml" Or IsDBNull(oDR(odc.ColumnName)) Then
                                'do nothing
                            ElseIf odc.ColumnName = "nAuditId" Then
                                cFields &= "nContactCartID,"
                                cValues &= moDBHelper.getAuditId() & ","
                            ElseIf odc.ColumnName = "nContactCartId" Then
                                cFields &= "nContactCartID,"
                                cValues &= nParentID & ","
                            ElseIf IsNumeric(oDR(odc.ColumnName)) Then
                                cFields &= odc.ColumnName & ","
                                cValues &= oDR(odc.ColumnName) & ","
                            Else
                                cFields &= odc.ColumnName & ","
                                cValues &= "'" & oDR(odc.ColumnName) & "',"
                            End If
                        Next
                        cFields = Left(cFields, cFields.Length - 1)
                        cValues = Left(cValues, cValues.Length - 1)
                        cNewSQL &= "(" & cFields & ") VALUES "
                        cNewSQL &= "(" & cValues & ")  "
                        moDBHelper.GetIdInsertSql(cNewSQL)
                        If cNewSQL.Contains("Delivery Address") Then bDelivery = True
                        If cNewSQL.Contains("Billing Address") Then bBilling = True
                    Next
                Next
                myWeb.moSession("CartId") = nCurrentCart
                If Not otmpcart Is Nothing Then
                    otmpcart.mnProcessId = 1
                    otmpcart.mcCartCmd = "Cart"
                    If bBilling Then otmpcart.mcCartCmd = "Delivery"
                    If bDelivery Then otmpcart.mcCartCmd = "ChoosePaymentShippingOption"
                    otmpcart.apply()
                End If

                'now we need to redirect somewhere?
                'bRedirect = True
                myWeb.moResponse.Redirect("/?cartCmd=Cart")
                Return True
            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "QuoteToOrder", ex, "", cProcessInfo, gbDebug)
                Return False
            End Try
        End Function

        Public Overrides Sub MakeCurrent(ByVal nOrderID As Integer)
            PerfMon.Log("Quote", "MakeCurrent")
            'procedure to make a selected historical
            'order or quote into the currently active one

            Dim oDS As New DataSet
            Dim otmpcart As Cart = Nothing
            Dim cNotes As String = ""
            Dim nCheckUser As Integer = 0
            Try

                If myWeb.mnUserId = 0 Then Exit Sub
                Dim oDre As SqlDataReader = moDBHelper.getDataReader("Select nCartUserDirId, cClientNotes FROM tblCartOrder WHERE nCartOrderKey = " & nOrderID)
                Do While oDre.Read
                    nCheckUser = oDre.GetValue(0)
                    If Not oDre.IsDBNull(1) Then cNotes = oDre.GetValue(1)
                Loop
                oDre.Close()
                If Not nCheckUser = mnEwUserId Then
                    Exit Sub 'else we carry on
                End If
                If mnCartId = 0 Then
                    'create a new cart
                    otmpcart = New Quote(myWeb)
                    otmpcart.CreateNewCart(Nothing)
                    mnCartId = otmpcart.mnCartId
                End If
                'now add the details to it

                oDS = moDBHelper.GetDataSet("Select * From tblCartItem WHERE nCartOrderID = " & nOrderID, "CartItems")
                Dim oDR1 As DataRow
                Dim oDR2 As DataRow
                Dim nParentID As Integer
                Dim sSQL As String

                moDBHelper.ReturnNullsEmpty(oDS)
                sSQL = "Update tblCartOrder SET cClientNotes ='" & cNotes & "'  WHERE nCartOrderKey = " & mnCartId
                moDBHelper.ExeProcessSql(sSQL)
                For Each oDR1 In oDS.Tables("CartItems").Rows
                    If oDR1("nParentId") = 0 Then
                        sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " &
                        "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " &
                        "nTaxRate, nQuantity, nWeight, nAuditId) VALUES ("
                        sSQL &= mnCartId & ","
                        sSQL &= IIf(IsDBNull(oDR1("nItemId")), "Null", oDR1("nItemId")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nParentId")), "Null", oDR1("nParentId")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("cItemRef")), "Null", "'" & oDR1("cItemRef")) & "',"
                        sSQL &= IIf(IsDBNull(oDR1("cItemURL")), "Null", "'" & oDR1("cItemURL")) & "',"
                        sSQL &= IIf(IsDBNull(oDR1("cItemName")), "Null", "'" & oDR1("cItemName")) & "',"
                        sSQL &= IIf(IsDBNull(oDR1("nItemOptGrpIdx")), "Null", oDR1("nItemOptGrpIdx")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nItemOptIdx")), "Null", oDR1("nItemOptIdx")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nPrice")), "Null", oDR1("nPrice")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nShpCat")), "Null", oDR1("nShpCat")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nDiscountCat")), "Null", oDR1("nDiscountCat")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nDiscountValue")), "Null", oDR1("nDiscountValue")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nTaxRate")), "Null", oDR1("nTaxRate")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nQuantity")), "Null", oDR1("nQuantity")) & ","
                        sSQL &= IIf(IsDBNull(oDR1("nWeight")), "Null", oDR1("nWeight")) & ","
                        sSQL &= moDBHelper.getAuditId() & ")"
                        nParentID = moDBHelper.GetIdInsertSql(sSQL)
                        'now for any children
                        For Each oDR2 In oDS.Tables("CartItems").Rows
                            If oDR2("nParentId") = oDR1("nCartItemKey") Then
                                sSQL = "INSERT INTO tblCartItem (nCartOrderId, nItemId, nParentId, cItemRef, cItemURL, " &
                                "cItemName, nItemOptGrpIdx, nItemOptIdx, nPrice, nShpCat, nDiscountCat, nDiscountValue, " &
                                "nTaxRate, nQuantity, nWeight, nAuditId) VALUES ("
                                sSQL &= mnCartId & ","
                                sSQL &= IIf(IsDBNull(oDR2("nItemId")), "Null", oDR2("nItemId")) & ","
                                sSQL &= nParentID & ","
                                sSQL &= IIf(IsDBNull(oDR2("cItemRef")), "Null", "'" & oDR2("cItemRef")) & "',"
                                sSQL &= IIf(IsDBNull(oDR2("cItemURL")), "Null", "'" & oDR2("cItemURL")) & "',"
                                sSQL &= IIf(IsDBNull(oDR2("cItemName")), "Null", "'" & oDR2("cItemName")) & "',"
                                sSQL &= IIf(IsDBNull(oDR2("nItemOptGrpIdx")), "Null", oDR2("nItemOptGrpIdx")) & ","
                                sSQL &= IIf(IsDBNull(oDR2("nItemOptIdx")), "Null", oDR2("nItemOptIdx")) & ","
                                sSQL &= IIf(IsDBNull(oDR2("nPrice")), "Null", oDR2("nPrice")) & ","
                                sSQL &= IIf(IsDBNull(oDR2("nShpCat")), "Null", oDR2("nShpCat")) & ","
                                sSQL &= IIf(IsDBNull(oDR2("nDiscountCat")), "Null", oDR2("nDiscountCat")) & ","
                                sSQL &= IIf(IsDBNull(oDR2("nDiscountValue")), "Null", oDR2("nDiscountValue")) & ","
                                sSQL &= IIf(IsDBNull(oDR2("nTaxRate")), "Null", oDR2("nTaxRate")) & ","
                                sSQL &= IIf(IsDBNull(oDR2("nQuantity")), "Null", oDR2("nQuantity")) & ","
                                sSQL &= IIf(IsDBNull(oDR2("nWeight")), "Null", oDR2("nWeight")) & ","
                                sSQL &= moDBHelper.getAuditId() & ")"
                                moDBHelper.GetIdInsertSql(sSQL)
                                'now for any children
                            End If
                        Next
                    End If
                Next

                'If Not otmpcart Is Nothing Then
                '    otmpcart.mnProcessId = 1
                '    otmpcart.mcCartCmd = "Quote"
                '    otmpcart.apply()
                'End If

                'now we need to redirect somewhere?
                'bRedirect = True
                'myWeb.moResponse.Redirect("?quoteCmd=Quote")
            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "MakeCurrent", ex, "", "", gbDebug)
            End Try
        End Sub

        Public Sub SaveQuote()
            PerfMon.Log("Quote", "SaveQuote")
            '   set the cart status to 7

            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try

                sSql = "update tblCartOrder set nCartStatus = 7, nCartUserDirId = " & myWeb.mnUserId & " where(nCartOrderKey = " & mnCartId & ")"
                moDBHelper.ExeProcessSql(sSql)
                myWeb.moSession("QuoteId") = Nothing
                mnCartId = 0

            Catch ex As Exception
                returnException(myWeb.msException, mcModuleName, "QuitCart", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub
    End Class
End Class
