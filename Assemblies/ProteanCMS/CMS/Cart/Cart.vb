Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Collections
Imports System.Web.Configuration
Imports System.Data.SqlClient
Imports System.Web.HttpUtility
Imports VB = Microsoft.VisualBasic
Imports System.IO
Imports Protean.Tools.Xml
Imports Protean.Tools.Xml.XmlNodeState
Imports System
Imports System.Reflection
Imports Protean.Providers.Membership.EonicProvider

Partial Public Class Cms

    Public Class Cart
#Region "Declarations"


        Public moCartConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/cart")
        Public moConfig As System.Collections.Specialized.NameValueCollection
        Private moServer As System.Web.HttpServerUtility

        Public moPageXml As XmlDocument

        Public Shadows mcModuleName As String = "Eonic.Cart"

        'Session EonicWeb Details
        Public mcEwDataConn As String
        Public mcEwSiteDomain As String
        Public mbEwMembership As Boolean

        Public oShippingOptions As XmlElement

        ' MEMB - notes beginning with MEMB relate to changes to the cart to incorporate logging in members and adding their addresses to contact fields in the billing/delivery forms
        ' Cart Status Ref
        '   1   new cart
        '   2   entered address
        '   3   abbandoned
        '   4   pass to payment
        '   5   Transaction Failed
        '   6   Payment Successful
        '   7   Order Cancelled / Payment Refunded
        '   8   Failed and Checked
        '   9   Order Shipped
        '  10   Part Payment (e.g. depsoit) Received
        '  11   Settlement initiated?
        '  12   Awaiting Payment

        Public mcSiteURL As String '    Site Identifier, used for User Cookie Name
        Public mcCartURL As String '    Site Identifier, used for User Cookie Name

        Public mnCartId As Integer '    Unique Id refering to this session cart
        Public mcSessionId As String '    Session ID - Unique for each client browser
        Private mcRefSessionId As String '    Referrer Site Session ID - The session ID from the referrer site, if passed.
        Public mnEwUserId As Integer '    User Id for Membership integration
        Public mmcOrderType As String '    The order type associated with the current cart
        Public mcItemOrderType As String '    The order type associated with the current page (if provided)
        Public moCartXml As XmlElement

        Public mnGiftListId As Integer = -1 '   If the current user is buying from a giftlist

        Public mcNotesXForm As String '    Location of Notes xform prior to billing details
        Public mnTaxRate As Double '    Add Tax to Cart Rate
        Public mcTermsAndConditions As String = ""
        Public mcMerchantEmail As String
        Public mcMerchantEmailTemplatePath As String
        Public mbStockControl As Boolean = False ' Stock Control
        Public mcDeposit As String ' Deposits are Available
        Public mcDepositAmount As String ' Deposit Amount
        Private cOrderNoPrefix As String
        Public mcCurrency As String = ""
        Public mcCurrencySymbol As String = ""

        Public mcCurrencyRef As String 'TS requires further investigation as to if these are requred or mcCurrency is sufficient
        Public mcCurrencyCode As String

        Public mcVoucherNumber As String = ""
        Public mcVoucherValue As String = ""
        Public mcVoucherExpires As String = ""
        Private promocodeFromExternalRef As String = ""
        Public mcPersistCart As String = ""
        Public mcPagePath As String
        Public mnPaymentId As Integer = 0 ' to be populated by payment prvoider to pass to subscriptions

        Public bFullCartOption As Boolean
        Public mbAddItemWithNoPrice As Boolean ' Switch to allow enquiries of items with no price

        ' Address Mods
        Public mcBillingAddressXform As String = "" ' Location of bespoke Billing Address
        Public mcDeliveryAddressXform As String = "" ' Location of bespoke Delivery Address
        Public mcPriorityCountries As String ' List of countires to appear at the top of dropdowns 
        Public mbNoDeliveryAddress As Boolean = False ' Option to turn off the need for a delivery address

        Public mcReturnPage As String ' page to return to with continue Shopping

        Public mcCartCmd As String = "" '    Action String for ewCart main function, ewcartPlugin()
        '       Can be:     <case sensitive>
        '           Billing
        '           Delivery
        '           Cart
        '           Add
        '           Remove
        '           ShowInvoice
        '           MakePayment
        '           Failed Payment
        '           Quit

        Public mcCartCmdAlt As String = "" 'alternative if we call apply again


        Public mnProcessId As Short = 0 '    State of Cart:
        '       0:      Empty / New Cart
        '       1:      Shopping / Items in Cart
        '       2:      Billing Address is Complete
        '       3:      Billing & Delivery Addressi are Complete
        '       4:      Order Confirmed / Ready to make payment
        '       5:      Transaction Complete
        '       6:      Tender Cancelled

        Public mnProcessError As Short '    General Process Error has been encountered:
        '       1:      Cookies are disabled or undetectable
        '       2:      The current item's order type does not match the cart's order type
        '       100+:   Payment gateway errors
        '       1000+:  Bespoke errors - can be defined in the xsl

        Public mcPaymentMethod As String = "" '   Payment Method:
        '       SecPay
        '       ProTx
        '       Secure Email
        '       MetaCharge
        '       WorldPay
        '       Cheque
        Public mcPaymentProfile As String = ""



        ' Behaviour mods
        Private mbRedirectSecure As Boolean = False

        Private mbDisplayPrice As Boolean = True

        Public mcSubmitText As String
        Public moDBHelper As dbHelper

        Public mcOrderType As String
        Public cOrderReference As String

        Public mbVatAtUnit As Boolean = False
        Public mbVatOnLine As Boolean = False
        Public mbRoundup As Boolean = False
        Public mbRoundDown As Boolean = False
        Public mbDiscountsOn As Boolean = False
        Public mbOveridePrice As Boolean = False
        Public mcPriceModOrder As String = ""
        Public mcUnitModOrder As String = ""


        Public mcReEstablishSession As String


        Public mnShippingRootId As Integer
        'Public mcCurrencySymbol As String

        Public moDiscount As Discount
        Public moSubscription As Subscriptions
        Protected moPay As PaymentProviders

        Public mbQuitOnShowInvoice As Boolean = True

        Enum cartError

            OutOfStock = 2
            ProductQuantityOverLimit = 201
            ProductQuantityUnderLimit = 202
            ProductQuantityNotInBulk = 203

        End Enum


        Enum cartProcess

            Empty = 0
            Items = 1
            Billing = 2
            Delivery = 3
            Confirmed = 4
            PassForPayment = 5
            Complete = 6
            Refunded = 7
            Failed = 8
            Shipped = 9
            DepositPaid = 10
            Abandoned = 11
            Deleted = 12
            AwaitingPayment = 13
            SettlementInitiated = 14
            SkipAddress = 15

        End Enum
#End Region

#Region "Properties"
        Public Overridable Property OrderNoPrefix() As String
            Get
                If cOrderNoPrefix = "" Then
                    cOrderNoPrefix = moCartConfig("OrderNoPrefix")
                End If
                Return cOrderNoPrefix
            End Get
            Set(ByVal value As String)
                cOrderNoPrefix = value
            End Set
        End Property
#End Region

#Region "Classes"
        Class FormResult

            Public Name As String
            Public Value As String
            Sub New(ByVal cName As String, ByVal cValue As String)
                PerfMon.Log("Cart", "New")
                Name = cName
                Value = cValue
            End Sub
        End Class

        Class Order

            Private OrderElmt As XmlElement
            Protected Friend myWeb As Cms
            Public moConfig As System.Collections.Specialized.NameValueCollection
            Private moServer As System.Web.HttpServerUtility

            Dim nFirstPayment As Double
            Dim nRepeatPayment As Double
            Dim sRepeatInterval As String
            Dim nRepeatLength As Integer
            Dim bDelayStart As Boolean
            Dim dStartDate As Date

            Dim sPaymentMethod As String
            Dim sTransactionRef As String
            Dim sDescription As String

            Dim sGivenName As String
            Dim sBillingAddress1 As String
            Dim sBillingAddress2 As String
            Dim sBillingTown As String
            Dim sBillingCounty As String
            Dim sBillingPostcode As String
            Dim sEmail As String

            Public moPageXml As XmlDocument


            Sub New(ByRef aWeb As Protean.Cms)
                PerfMon.Log("Order", "New")
                myWeb = aWeb
                moConfig = myWeb.moConfig
                moPageXml = myWeb.moPageXml
                moServer = aWeb.moCtx.Server
                OrderElmt = moPageXml.CreateElement("Order")
            End Sub

            ReadOnly Property xml As XmlElement
                Get
                    Return OrderElmt
                End Get
            End Property

            Property PaymentMethod As String
                Get
                    Return sPaymentMethod
                End Get
                Set(ByVal Value As String)
                    sPaymentMethod = Value
                    OrderElmt.SetAttribute("paymentMethod", Value)
                End Set
            End Property

            Property firstPayment As Double
                Get
                    Return nFirstPayment
                End Get
                Set(ByVal Value As Double)
                    nFirstPayment = Value
                    OrderElmt.SetAttribute("total", Value)
                End Set
            End Property

            Property repeatPayment As Double
                Get
                    Return nRepeatPayment
                End Get
                Set(ByVal Value As Double)
                    nRepeatPayment = Value
                    OrderElmt.SetAttribute("repeatPrice", Value)
                End Set
            End Property

            Property repeatInterval As String
                Get
                    Return sRepeatInterval
                End Get
                Set(ByVal Value As String)
                    sRepeatInterval = Value
                    OrderElmt.SetAttribute("repeatInterval", Value)
                End Set
            End Property

            Property repeatLength As Integer
                Get
                    Return nRepeatLength
                End Get
                Set(ByVal Value As Integer)
                    nRepeatLength = Value
                    OrderElmt.SetAttribute("repeatLength", Value)
                End Set
            End Property

            Property delayStart As Boolean
                Get
                    Return bDelayStart
                End Get
                Set(ByVal Value As Boolean)
                    bDelayStart = Value
                    If Value Then
                        OrderElmt.SetAttribute("delayStart", "true")
                    Else
                        OrderElmt.SetAttribute("delayStart", "false")
                    End If
                End Set
            End Property

            Property startDate As Date
                Get
                    Return dStartDate
                End Get
                Set(ByVal Value As Date)
                    dStartDate = Value
                    OrderElmt.SetAttribute("startDate", xmlDate(dStartDate))
                End Set
            End Property

            Property TransactionRef As String
                Get
                    Return sTransactionRef
                End Get
                Set(ByVal Value As String)
                    sTransactionRef = Value
                    OrderElmt.SetAttribute("transactionRef", sTransactionRef)
                End Set
            End Property


            Property description As String
                Get
                    Return sDescription
                End Get
                Set(ByVal Value As String)
                    sDescription = Value
                    Dim descElmt As XmlElement = moPageXml.CreateElement("Description")
                    descElmt.InnerText = sDescription
                    OrderElmt.AppendChild(descElmt)
                End Set
            End Property

            Sub SetAddress(ByVal GivenName As String, ByVal Email As String, ByVal Company As String, ByVal Street As String, ByVal City As String, ByVal State As String, ByVal PostalCode As String, ByVal Country As String)

                Dim addElmt As XmlElement = moPageXml.CreateElement("Contact")
                addElmt.SetAttribute("type", "Billing Address")
                xmlTools.addElement(addElmt, "GivenName", GivenName)
                xmlTools.addElement(addElmt, "Email", Email)
                xmlTools.addElement(addElmt, "Company", Company)
                xmlTools.addElement(addElmt, "Street", Street)
                xmlTools.addElement(addElmt, "City", City)
                xmlTools.addElement(addElmt, "State", State)
                xmlTools.addElement(addElmt, "PostalCode", PostalCode)
                xmlTools.addElement(addElmt, "Country", Country)
                OrderElmt.AppendChild(addElmt)
            End Sub

        End Class


#End Region



        Private Function getProcessName(ByVal cp As cartProcess) As String
            Select Case cp
                Case cartProcess.Abandoned
                    Return "Abandoned"
                Case cartProcess.AwaitingPayment
                    Return "Awaiting Payment"
                Case cartProcess.SkipAddress
                    Return "SkipAddress"
                Case cartProcess.Billing
                    Return "Billing"
                Case cartProcess.Complete
                    Return "Complete"
                Case cartProcess.Confirmed
                    Return "Confirmed"
                Case cartProcess.Deleted
                    Return "Deleted"
                Case cartProcess.Delivery
                    Return "Delivery"
                Case cartProcess.DepositPaid
                    Return "Deposit Paid"
                Case cartProcess.Empty
                    Return "Empty"
                Case cartProcess.Failed
                    Return "Failed"
                Case cartProcess.PassForPayment
                    Return "Pass For Payment"
                Case cartProcess.Refunded
                    Return "Refunded"
                Case cartProcess.Shipped
                    Return "Shipped"
                Case Else
                    Return "Unknown Process ID"
            End Select
        End Function



        Protected Friend myWeb As Cms

        Public Sub New(ByRef aWeb As Protean.Cms)
            PerfMon.Log("Cart", "New")
            Dim cProcessInfo As String = ""
            Try
                myWeb = aWeb
                moConfig = myWeb.moConfig
                moPageXml = myWeb.moPageXml
                moDBHelper = myWeb.moDbHelper
                InitializeVariables()
                moServer = aWeb.moCtx.Server

            Catch ex As Exception
                returnException(mcModuleName, "Close", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub


        Public Sub InitializeVariables()
            PerfMon.Log("Cart", "InitializeVariables")
            'Author:        Trevor Spink
            'Copyright:     Eonic Ltd 2006
            'Date:          2006-10-04

            '   called at the beginning, whenever ewCart is run
            '   sets the global variables and initialises the current cart

            Dim sSql As String = ""
            Dim oDr As SqlDataReader
            Dim cartXmlFromDatabase As String = ""
            mcOrderType = "Order"
            cOrderReference = ""
            mcModuleName = "Eonic.Cart"

            Dim cProcessInfo As String = "" = "initialise variables"
            Try

                If Not moCartConfig Is Nothing Then

                    If myWeb.mnUserId > 0 And myWeb.moConfig("SecureMembershipAddress") <> "" Then
                        mcSiteURL = myWeb.moConfig("SecureMembershipAddress") & moConfig("ProjectPath") & "/"
                        mcCartURL = myWeb.moConfig("SecureMembershipAddress") & moConfig("ProjectPath") & "/"
                    Else
                        mcSiteURL = moCartConfig("SiteURL")
                        mcCartURL = moCartConfig("SecureURL")
                    End If

                    If LCase(myWeb.moRequest("ewCmd")) = "logoff" Then
                        EndSession()
                    End If

                    moDiscount = New Discount(Me)
                    mcPagePath = myWeb.mcPagePath

                    If mcPagePath = "" Then
                        If mcCartURL.EndsWith("/") Then
                            mcPagePath = mcCartURL & "?"
                        Else
                            mcPagePath = mcCartURL & "/?"
                        End If
                    Else
                        mcPagePath = mcCartURL & mcPagePath & "?"
                    End If

                    If moConfig("Membership") = "on" Then mbEwMembership = True

                    mcMerchantEmail = moCartConfig("MerchantEmail")
                    mcTermsAndConditions = moCartConfig("TermsAndConditions")
                    'mcOrderNoPrefix = moCartConfig("OrderNoPrefix")
                    mcCurrencySymbol = moCartConfig("CurrencySymbol")
                    mcCurrency = moCartConfig("Currency")
                    mcCurrencyRef = moCartConfig("Currency")
                    If mcCurrency = "" Then mcCurrency = "GBP"

                    Dim moPaymentCfg As XmlNode

                    'change currency based on language selection
                    If myWeb.gcLang <> "" Then
                        Dim moLangCfg As XmlNode = WebConfigurationManager.GetWebApplicationSection("protean/languages")
                        If Not moLangCfg Is Nothing Then
                            Dim thisLangNode As XmlElement = moLangCfg.SelectSingleNode("Language[@code='" & myWeb.gcLang & "']")
                            If Not thisLangNode Is Nothing Then
                                mcCurrency = thisLangNode.GetAttribute("currency")
                                mcCurrencyRef = thisLangNode.GetAttribute("currency")
                                moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                                Dim thisCurrencyNode As XmlElement = moPaymentCfg.SelectSingleNode("currencies/Currency[@ref='" & mcCurrency & "']")
                                mcCurrencySymbol = thisCurrencyNode.GetAttribute("symbol")
                            End If
                        End If
                    End If

                    'change currency if default user currency is set
                    If myWeb.mnUserId > 0 Then
                        Dim userxml As XmlElement = myWeb.moPageXml.SelectSingleNode("/Page/User")
                        If userxml Is Nothing Then
                            userxml = myWeb.GetUserXML(myWeb.mnUserId)
                        End If
                        If userxml.GetAttribute("defaultCurrency") <> "" Then
                            mcCurrency = userxml.GetAttribute("defaultCurrency")
                            mcCurrencyRef = userxml.GetAttribute("defaultCurrency")
                            moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                            Dim thisCurrencyNode As XmlElement = moPaymentCfg.SelectSingleNode("currencies/Currency[@ref='" & mcCurrency & "']")
                            mcCurrencySymbol = thisCurrencyNode.GetAttribute("symbol")
                        End If
                    End If
                    moPaymentCfg = Nothing

                    'reset the currency on discounts
                    moDiscount.mcCurrency = mcCurrency

                    If moCartConfig("StockControl") = "on" Then mbStockControl = True
                    If moCartConfig("DisplayPrice") = "off" Then mbDisplayPrice = False

                    mcDeposit = LCase(moCartConfig("Deposit"))
                    mcDepositAmount = moCartConfig("DepositAmount")
                    mcNotesXForm = moCartConfig("NotesXForm")
                    mcBillingAddressXform = moCartConfig("BillingAddressXForm")
                    mcDeliveryAddressXform = moCartConfig("DeliveryAddressXForm")
                    If moCartConfig("NoDeliveryAddress") = "on" Then mbNoDeliveryAddress = True
                    mcMerchantEmailTemplatePath = moCartConfig("MerchantEmailTemplatePath")
                    mcPriorityCountries = moCartConfig("PriorityCountries")
                    mcPersistCart = moCartConfig("PersistCart") 'might need to add checks for missing key
                    If mcPriorityCountries Is Nothing Or mcPriorityCountries = "" Then
                        mcPriorityCountries = "United Kingdom,United States"
                    End If

                    mnTaxRate = moCartConfig("TaxRate")
                    If Not (myWeb.moSession("nTaxRate") Is Nothing) Then
                        mnTaxRate = CDbl("0" & myWeb.moSession("nTaxRate"))
                    End If

                    If myWeb.moRequest.Form("url") <> "" Then
                        myWeb.moSession("returnPage") = myWeb.moRequest.Form("url")
                    End If

                    If myWeb.moRequest.QueryString("url") <> "" Then
                        myWeb.moSession("returnPage") = myWeb.moRequest.QueryString("url")
                    End If

                    mcReturnPage = myWeb.moSession("returnPage")

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

                    If myWeb.moSession("CartId") Is Nothing Then
                        mnCartId = 0
                    Else
                        If Not (IsNumeric(myWeb.moSession("CartId"))) Or myWeb.moSession("CartId") <= 0 Then
                            mnCartId = 0
                        Else
                            mnCartId = CInt(myWeb.moSession("CartId"))
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

                    mcCartCmd = myWeb.moRequest.QueryString("cartCmd")
                    If mcCartCmd = "" Then
                        mcCartCmd = myWeb.moRequest.Form("cartCmd")
                    End If

                    mcPaymentMethod = myWeb.moSession("mcPaymentMethod")
                    mmcOrderType = myWeb.moSession("mmcOrderType")
                    mcItemOrderType = myWeb.moRequest.Form("ordertype")

                    '  MsgBox "Item: " & mcItemOrderType & vbCrLf & "Order: " & mmcOrderType
                    '  set global variable for submit button

                    mcSubmitText = myWeb.moRequest("submit")

                    If mnCartId > 0 Then
                        '   cart exists
                        'turn off page caching
                        myWeb.bPageCache = False

                        If mcPersistCart = "on" Then
                            writeSessionCookie() 'write the cookie to persist the cart
                        End If
                        If mcReEstablishSession <> "" Then
                            sSql = "select * from tblCartOrder where not(nCartStatus IN (6,9,13,14)) and nCartOrderKey = " & mnCartId
                        Else
                            sSql = "select * from tblCartOrder where (nCartStatus < 7 or nCartStatus IN (10,14)) and nCartOrderKey = " & mnCartId & " and not(cCartSessionId like 'OLD_%')"
                        End If
                        oDr = moDBHelper.getDataReader(sSql)
                        If oDr.HasRows Then
                            While oDr.Read
                                mnGiftListId = oDr("nGiftListId")
                                mnTaxRate = CDbl("0" & oDr("nTaxRate"))
                                mnProcessId = CLng("0" & oDr("nCartStatus"))
                                cartXmlFromDatabase = oDr("cCartXml").ToString
                                ' Check for deposit and earlier stages
                                If mcDeposit = "on" Then
                                    If Not (IsDBNull((oDr("nAmountReceived")))) Then
                                        If oDr("nAmountReceived") > 0 And mnProcessId < cartProcess.Confirmed Then
                                            mnProcessId = cartProcess.SettlementInitiated
                                            moDBHelper.ExeProcessSql("update tblCartOrder set nCartStatus = '" & mnProcessId & "' where nCartOrderKey = " & mnCartId)
                                        End If
                                    End If
                                End If

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
                        '-- Cart doesn't exist --

                        'check if we need to persist the cart
                        Dim cSessionFromSessionCookie As String = ""
                        If mcPersistCart = "on" Then
                            Dim cSessionCookieName As String = "ewSession" & myWeb.mnUserId.ToString
                            If myWeb.moRequest.Cookies(cSessionCookieName) Is Nothing Then
                                writeSessionCookie()
                            Else

                                Try

                                    'get session ID from cookie IF session ID and current user if match
                                    'Dim cSessionCookieContents As String = myWeb.moRequest.Cookies("ewSession" & myWeb.mnUserId.ToString).Value
                                    Dim cSessionFromCookie As String = myWeb.moRequest.Cookies(cSessionCookieName).Value
                                    'Dim nCartIdCheck As Integer = moDBHelper.ExeProcessSqlScalar("SELECT COUNT(nCartOrderKey) FROM tblCartOrder WHERE nCartUserDirId = " & cUserIdFromCookie.ToString & " AND cCartSessionId = '" & cSessionFromCookie & "'")

                                    If cSessionFromCookie <> "" Then
                                        cSessionFromSessionCookie = cSessionFromCookie
                                    End If

                                    If mcReEstablishSession <> "" Then
                                        cSessionFromSessionCookie = mcReEstablishSession
                                    End If

                                Catch ex As Exception
                                    cProcessInfo = ex.Message
                                End Try

                            End If
                        End If

                        ' check if the cart can be found in the database, although only run this check if we 
                        ' know that we've visited the cart
                        ' Also check out if this is coming from a Worldpay callback.
                        ' Also check we need to udpate the session from the cookie
                        If Not (myWeb.moRequest("refSessionId") Is Nothing) _
                            Or Not (myWeb.moRequest("transStatus") Is Nothing) _
                            Or Not (myWeb.moRequest("ewSettlement") Is Nothing) _
                            Or (cSessionFromSessionCookie <> "") Then

                            If Not (myWeb.moRequest("transStatus") Is Nothing) Then
                                'add in check for session cookie
                                sSql = "select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId  where o.cCartSchemaName='Order' and o.nCartOrderKey=" & myWeb.moRequest("cartId") & " and DATEDIFF(hh,a.dInsertDate,GETDATE())<24"
                                mcPaymentMethod = "WorldPay"
                            ElseIf Not (myWeb.moRequest("ewSettlement") Is Nothing) Then
                                ' Go get the cart, restore settings
                                sSql = "select * from tblCartOrder where cCartSchemaName='Order' and cSettlementID='" & myWeb.moRequest("ewSettlement") & "'"
                            Else
                                'get session id from ewSession cookie

                                If cSessionFromSessionCookie <> "" Then ' myWeb.mnUserId > 0 And
                                    mcSessionId = cSessionFromSessionCookie
                                    cSessionFromSessionCookie = ""
                                End If

                                sSql = "select * from tblCartOrder o inner join tblAudit a on a.nAuditKey=o.nAuditId where o.cCartSchemaName='Order' and o.cCartSessionId = '" & SqlFmt(mcSessionId) & "'"
                            End If

                            PerfMon.Log("Cart", "InitializeVariables - check for cart start")
                            oDr = moDBHelper.getDataReader(sSql)
                            PerfMon.Log("Cart", "InitializeVariables - check for cart end")

                            If oDr.HasRows Then
                                While oDr.Read
                                    mnGiftListId = oDr("nGiftListId")
                                    mnCartId = oDr("nCartOrderKey") ' get cart id
                                    mnProcessId = oDr("nCartStatus") ' get cart status
                                    mnTaxRate = oDr("nTaxRate")
                                    If Not (myWeb.moRequest("ewSettlement") Is Nothing) Or Not (myWeb.moRequest("ewsettlement") Is Nothing) Then

                                        ' Set to a holding state that indicates that the settlement has been initiated
                                        mnProcessId = cartProcess.SettlementInitiated

                                        ' If a cart has been found, we need to update the session ID in it.
                                        If oDr("cCartSessionId") <> mcSessionId Then
                                            moDBHelper.ExeProcessSql("update tblCartOrder set cCartSessionId = '" & mcSessionId & "' where nCartOrderKey = " & mnCartId)
                                        End If

                                        ' Reactivate the order in the database
                                        moDBHelper.ExeProcessSql("update tblCartOrder set nCartStatus = '" & mnProcessId & "' where nCartOrderKey = " & mnCartId)

                                    End If
                                    If mnProcessId > 5 And mnProcessId <> cartProcess.SettlementInitiated Then
                                        ' Cart has passed a status of "Succeeded" - we can't do anything to this cart. Clear the session.
                                        EndSession()
                                        mnCartId = 0
                                        mnTaxRate = Nothing
                                        mnProcessId = 0
                                        mcCartCmd = ""
                                    End If
                                    mcCurrencyRef = oDr("cCurrency")
                                    cartXmlFromDatabase = oDr("cCartXml").ToString
                                End While
                                oDr.Close()
                            End If
                        End If

                    End If



                    ' Load the cart xml from the database for passing on transitory variables.
                    If mnCartId > 0 AndAlso Not String.IsNullOrEmpty(cartXmlFromDatabase) Then
                        ' This is used to load in variables from the xml that is saved to the database.
                        ' Much more for transitory data that may need to be stored across sessions - e.g. from site to secure site.
                        Dim cartXmlFromLoad As XmlElement = Nothing
                        cartXmlFromLoad = myWeb.moPageXml.CreateElement("Load")
                        cartXmlFromLoad.InnerXml = cartXmlFromDatabase
                        If Not cartXmlFromLoad.SelectSingleNode("Order") Is Nothing Then
                            cartXmlFromLoad = cartXmlFromLoad.SelectSingleNode("Order")
                            If Not String.IsNullOrEmpty(cartXmlFromLoad.GetAttribute("promocodeFromExternalRef")) Then
                                promocodeFromExternalRef = cartXmlFromLoad.GetAttribute("promocodeFromExternalRef")
                            End If
                        End If
                        cartXmlFromLoad = Nothing
                    End If

                    ' Check for promo code from request object
                    If myWeb.moRequest("promocode") <> "" AndAlso myWeb.moSession IsNot Nothing Then

                        ' This has been requested - update Session
                        myWeb.moSession("promocode") = myWeb.moRequest("promocode").ToString
                        Me.promocodeFromExternalRef = myWeb.moRequest("promocode").ToString

                    ElseIf myWeb.moSession IsNot Nothing AndAlso myWeb.moSession("promocode") <> "" Then

                        ' Set the value from the session.
                        Me.promocodeFromExternalRef = myWeb.moSession("promocode").ToString

                    End If

                    mbVatAtUnit = IIf(LCase(moCartConfig("VatAtUnit")) = "yes" Or LCase(moCartConfig("VatAtUnit")) = "on", True, False)
                    mbVatOnLine = IIf(LCase(moCartConfig("VatOnLine")) = "yes" Or LCase(moCartConfig("VatOnLine")) = "on", True, False)

                    mbRoundup = IIf(LCase(moCartConfig("Roundup")) = "yes" Or LCase(moCartConfig("Roundup")) = "on", True, False)
                    mbRoundDown = IIf(LCase(moCartConfig("Roundup")) = "down", True, False)

                    mbDiscountsOn = IIf(LCase(moCartConfig("Discounts")) = "yes" Or LCase(moCartConfig("Discounts")) = "on", True, False)
                    mbOveridePrice = IIf(LCase(moCartConfig("OveridePrice")) = "yes" Or LCase(moCartConfig("OveridePrice")) = "on", True, False)

                    If mcCurrencyRef = "" Then mcCurrencyRef = myWeb.moSession("cCurrency")
                    If mcCurrencyRef = "" Or mcCurrencyRef Is Nothing Then mcCurrencyRef = moCartConfig("currencyRef") 'Setting Deprecated
                    If mcCurrencyRef = "" Or mcCurrencyRef Is Nothing Then mcCurrencyRef = moCartConfig("DefaultCurrencyOveride")
                    GetCurrencyDefinition()

                    'try grabbing a userid if we dont have one but have a cart
                    If myWeb.mnUserId = 0 And mnCartId > 0 Then
                        sSql = "SELECT nCartUserDirId FROM tblCartOrder WHERE nCartOrderKey = " & mnCartId
                        Dim cRes As String = moDBHelper.ExeProcessSqlScalar(sSql)

                        If IsNumeric(cRes) AndAlso cRes > 0 Then
                            myWeb.mnUserId = CInt(cRes)
                            mnEwUserId = myWeb.mnUserId
                            myWeb.moSession("nUserId") = cRes

                            Dim cRequestPage As String = myWeb.moRequest("pgid")
                            If IsNumeric(cRequestPage) AndAlso cRequestPage > 0 Then
                                myWeb.mnPageId = myWeb.moRequest("pgid")
                            End If
                        End If
                    End If

                    ' Behavioural Tweaks
                    If mnProcessId = cartProcess.SettlementInitiated Then
                        mbRedirectSecure = True
                    End If

                    If moConfig("Subscriptions") = "on" Then
                        moSubscription = New Subscriptions(Me)
                    End If
                    'If Not moDiscount Is Nothing Then
                    '    moDiscount.bHasPromotionalDiscounts = myWeb.moSession("bHasPromotionalDiscounts")
                    'End If

                End If

            Catch ex As Exception
                returnException(mcModuleName, "InitializeVariables", ex, "", cProcessInfo, gbDebug)
                'close()
            Finally
                oDr = Nothing
            End Try
        End Sub

        Public Sub writeSessionCookie()
            'writes the session cookie to persist the cart
            If mcPersistCart = "on" Then
                'make or update the session cookie
                Dim cookieEwSession As System.Web.HttpCookie = New System.Web.HttpCookie("ewSession" & myWeb.mnUserId.ToString)
                cookieEwSession.Value = mcSessionId.ToString
                cookieEwSession.Expires = DateAdd(DateInterval.Month, 1, Date.Now)
                myWeb.moResponse.Cookies.Add(cookieEwSession)
            End If
        End Sub

        Private Sub clearSessionCookie()

            Dim cSessionCookieName As String = "ewSession" & myWeb.mnUserId.ToString

            If (Not myWeb.moResponse.Cookies(cSessionCookieName) Is Nothing) Then
                'clear ewSession cookie so cart doesn't get persisted
                'we don't need to check for mcPersistCart = "on"
                Dim cookieEwSession As New System.Web.HttpCookie(cSessionCookieName)
                cookieEwSession.Expires = Now.AddDays(-1)
                myWeb.moResponse.Cookies.Add(cookieEwSession)
                cookieEwSession.Expires = DateAdd(DateInterval.Month, 1, Date.Now)
            End If

        End Sub


        Public Shadows Sub close()
            PerfMon.Log("Cart", "close")
            Dim cProcessInfo As String = ""
            Try
                PersistVariables()
            Catch ex As Exception
                returnException(mcModuleName, "Close", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Overridable Sub PersistVariables()
            PerfMon.Log("Cart", "PersistVariables")
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
                If Not myWeb.moSession Is Nothing Then
                    If myWeb.moSession("CartId") Is Nothing Then
                        myWeb.moSession.Add("CartId", CStr(mnCartId))
                    Else
                        myWeb.moSession("CartId") = CStr(mnCartId)
                    End If
                    '    oResponse.Cookies(mcSiteURL & "CartId").Domain = mcSiteURL
                    '    oSession("nCartOrderId") = mnCartId    '   session attribute holds Cart ID
                End If

                If mnCartId > 0 Then
                    ' Only update the process if less than 6 we don't ever want to change the status of a completed order other than within the admin system. Boo Yah!
                    Dim currentStatus As Integer = moDBHelper.ExeProcessSqlScalar("select nCartStatus from tblCartOrder where nCartOrderKey = " & mnCartId)
                    If currentStatus < 6 Then
                        '   If we have a cart, update its status in the db
                        If mnProcessId <> currentStatus Then
                            sSql = "update tblCartOrder set nCartStatus = " & mnProcessId & ", nGiftListId = " & mnGiftListId & ", nCartUserDirId = " & myWeb.mnUserId & " where nCartOrderKey = " & mnCartId
                        Else
                            sSql = "update tblCartOrder set nGiftListId = " & mnGiftListId & ", nCartUserDirId = " & myWeb.mnUserId & " where nCartOrderKey = " & mnCartId
                        End If
                        moDBHelper.ExeProcessSql(sSql)
                    End If


                End If

                If Not myWeb.moSession Is Nothing Then
                    myWeb.moSession("nProcessId") = mnProcessId '   persist global mnProcessId
                    myWeb.moSession("mcPaymentMethod") = mcPaymentMethod
                    myWeb.moSession("mmcOrderType") = mmcOrderType
                    myWeb.moSession("nTaxRate") = mnTaxRate
                    If Not moDiscount Is Nothing Then
                        myWeb.moSession("bHasPromotionalDiscounts") = moDiscount.bHasPromotionalDiscounts
                    End If
                End If

            Catch ex As Exception
                returnException(mcModuleName, "PersistVariables", ex, "", cProcessInfo, gbDebug)
                'close()
            End Try
        End Sub

        Overridable Sub checkButtons()
            PerfMon.Log("Cart", "checkButtons")
            Dim cProcessInfo As String = ""
            Try

                'if we have set mcCartCmdAlt then that overides the button.
                If mcCartCmdAlt <> "" Then
                    mcCartCmd = mcCartCmdAlt
                    Exit Sub
                End If

                If ButtonSubmitted(myWeb.moRequest, "cartAdd") Then
                    mcCartCmd = "Add"
                End If
                If ButtonSubmitted(myWeb.moRequest, "cartDetail") Then
                    mcCartCmd = "Cart"
                End If
                If ButtonSubmitted(myWeb.moRequest, "cartProceed") Then
                    mcCartCmd = "RedirectSecure"
                End If
                If ButtonSubmitted(myWeb.moRequest, "cartNotes") Then
                    mcCartCmd = "Notes"
                End If
                If ButtonSubmitted(myWeb.moRequest, "cartUpdate") Then
                    mcCartCmd = "Update"
                End If
                If ButtonSubmitted(myWeb.moRequest, "cartLogon") Then
                    mcCartCmd = "Logon"
                End If
                If ButtonSubmitted(myWeb.moRequest, "cartRegister") Then
                    mcCartCmd = "Logon"
                End If
                If ButtonSubmitted(myWeb.moRequest, "cartQuit") Then
                    mcCartCmd = "Quit"
                End If
                'Continue shopping
                If ButtonSubmitted(myWeb.moRequest, "cartBrief") Then
                    mcCartCmd = "Brief"
                End If
                ' Pick Address Buttions
                If ButtonSubmitted(myWeb.moRequest, "cartBillAddress") Or ButtonSubmitted(myWeb.moRequest, "cartBillcontact") Or ButtonSubmitted(myWeb.moRequest, "cartBilladdNewAddress") Or ButtonSubmitted(myWeb.moRequest, "cartBilleditAddress") Then
                    mcCartCmd = "Billing"
                End If
                If ButtonSubmitted(myWeb.moRequest, "cartDelAddress") Or ButtonSubmitted(myWeb.moRequest, "cartDelcontact") Or ButtonSubmitted(myWeb.moRequest, "cartDeladdNewAddress") Or ButtonSubmitted(myWeb.moRequest, "cartDeleditAddress") Then
                    mcCartCmd = "Delivery"
                End If

                'legacy button handling looking at button values rather than names, should not be required soon
                'Select Case mcSubmitText
                '    Case "Goto Checkout", "Go To Checkout"
                '        updateCart("RedirectSecure")
                '        mcCartCmd = "RedirectSecure"
                '    Case "Edit Billing Details", "Proceed without Logon"
                '        mcCartCmd = "Billing"
                '    Case "Edit Delivery Details"
                '        mcCartCmd = "Delivery"
                '    Case "Confirm Order", "Proceed with Order", "Proceed"
                '        updateCart("ChoosePaymentShippingOption")
                '    Case "Update Cart", "Update Order"
                '        updateCart("Cart")
                '    Case "Empty Cart", "Empty Order"
                '        mcCartCmd = "Quit"
                '    Case "Make Secure Payment"
                '        updateCart(mcCartCmd)
                '    Case "Continue Shopping"
                '        mcCartCmd = "BackToSite"
                'End Select

            Catch ex As Exception
                returnException(mcModuleName, "checkButtons", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Function CreateCartElement(oCartXML As XmlDocument)
            Dim oContentElmt As XmlElement
            Dim oElmt As XmlElement

            Try
                oContentElmt = oCartXML.CreateElement("Cart")
                oContentElmt.SetAttribute("type", "order")
                oContentElmt.SetAttribute("currency", mcCurrency)
                oContentElmt.SetAttribute("currencySymbol", mcCurrencySymbol)
                If Not mbDisplayPrice Then oContentElmt.SetAttribute("displayPrice", mbDisplayPrice.ToString)
                oElmt = oCartXML.CreateElement("Order")
                oContentElmt.AppendChild(oElmt)

                moCartXml = oContentElmt

                Return oContentElmt

            Catch ex As Exception
                returnException(mcModuleName, "apply", ex, "", "CreateCartElement", gbDebug)
                Return Nothing
            End Try

        End Function



        Public Overridable Sub apply()
            PerfMon.Log("Cart", "apply")
            ' this function is the main function.

            Dim oCartXML As XmlDocument = moPageXml
            Dim oContentElmt As XmlElement
            Dim oElmt As XmlElement
            'Dim oPickContactXForm As xForm
            Dim cProcessInfo As String = ""
            Dim bRedirect As Boolean
            Dim cRepeatPaymentError As String = ""

            Try

                '  myWeb.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "Start1 CALLBACK : " & mnProcessId & mcCartCmd)

                oContentElmt = CreateCartElement(oCartXML)
                oElmt = oContentElmt.FirstChild
                '   if the cartCmd is not on a link but on a button
                '   we need to set the cartCmd dependant upon the button name

                'TS for OpenQuote allow cart step to be set before checking against button allows apply to be called again
                If mcCartCmd = "" Then
                End If
                checkButtons()

                ' Cart Command overrides
                If mbRedirectSecure Or mnProcessId = cartProcess.SettlementInitiated Then
                    mcCartCmd = "RedirectSecure"
                End If

                ' Secure Trading CallBack override
                If mcCartCmd <> "" Then
                    If mcCartCmd.StartsWith("SecureTradingReturn") Then mcCartCmd = "SubmitPaymentDetails"
                End If

                If Not (mnCartId > 0) Then
                    ' Cart doesn't exist - if the process flow has a valid command (except add or quit), then this is an error
                    Select Case mcCartCmd
                        Case "Cart"
                            mcCartCmd = "CartEmpty"
                        Case "Logon", "Remove", "Notes", "Billing", "Delivery", "ChoosePaymentShippingOption", "Confirm", "EnterPaymentDetails", "SubmitPaymentDetails", "SubmitPaymentDetails", "ShowInvoice", "ShowCallBackInvoice"
                            mcCartCmd = "CookiesDisabled"
                        Case "Error"
                            mcCartCmd = "Error"
                    End Select

                End If

                'Cart Process

processFlow:
                ' myWeb.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "Start2 CALLBACK : " & mnProcessId & mcCartCmd)

                '  user can't change things if we are to show the invoice
                If mnProcessId = Cart.cartProcess.Complete And mcCartCmd <> "Quit" And mcCartCmd <> "ShowCallBackInvoice" Then mcCartCmd = "ShowInvoice"

                cProcessInfo &= IIf(mcCartCmd = "", "", ", ") & mcCartCmd

                If mcCartCmd <> "" Then
                    'ensure the client is not able to hit the back button and go back to the page without refreshing.
                    'This should resolve some rare random errors in the cart process.
                    myWeb.mbSetNoBrowserCache = True
                End If

                Select Case mcCartCmd
                    Case "Update"
                        mcCartCmd = updateCart("Currency")
                        GoTo processFlow
                    Case "Remove" '   take away an item and set the command to display the cart
                        If RemoveItem() > 0 Then
                            mcCartCmd = "Currency"
                        Else
                            ' RemoveItem has removed the last item in the cart - quit the cart.
                            mcCartCmd = "Quit"
                        End If
                        GoTo processFlow


                    Case "Add" '   add an item to the cart, if its a new cart we must initialise it and change its status
                        Dim qtyAdded As Long = 0

                        Dim oItem1 As Object
                        Dim nQuantity As Long
                        'Check we are adding a quantity (we need to catch any adds that don't have a specified quantity and create empty carts)
                        For Each oItem1 In myWeb.moRequest.Form 'Loop for getting products/quants
                            If InStr(oItem1, "qty_") = 1 Then 'check for getting productID and quantity (since there will only be one of these per item submitted)
                                If IsNumeric(myWeb.moRequest.Form.Get(oItem1)) Then
                                    nQuantity = myWeb.moRequest.Form.Get(oItem1)
                                End If

                                'replacementName
                                If nQuantity > 0 Then
                                    qtyAdded = qtyAdded + nQuantity
                                End If 'end check for previously added
                            End If 'end check for item/quant
                        Next 'End Loop for getting products/quants

                        If mnCartId < 1 And qtyAdded > 0 Then
                            CreateNewCart(oElmt)
                            If mcItemOrderType <> "" Then
                                mmcOrderType = mcItemOrderType
                            Else
                                mmcOrderType = ""
                            End If
                            mnProcessId = 1
                        End If

                        If qtyAdded > 0 And mnCartId > 0 Then
                            If Not AddItems() Then
                                mnProcessError = 2 ' Error: The current item's order type does not match the cart's order type
                                mcCartCmd = "Error"
                                GoTo processFlow
                            Else
                                'Case for if a items have been added from a giftlist
                                If Not myWeb.moRequest("giftlistId") Is Nothing Then
                                    AddDeliveryFromGiftList(myWeb.moRequest("giftlistId"))
                                End If
                                mcCartCmd = "Currency"
                                GoTo processFlow
                            End If
                        End If

                        If qtyAdded > 0 And mnCartId = 0 Then
                            mnProcessError = 1 ' Error: Cookies Disabled
                            mcCartCmd = "Error"
                            GoTo processFlow
                        End If


                        'here is where we should check cart for subscriptions


                    Case "Currency"

                        If SelectCurrency() Then
                            If mcCartCmd = "Cart" Then
                                AddBehaviour()
                            End If
                            GoTo processFlow
                        End If

                    Case "Quit"
                        '   action depends on whether order is complete or not
                        If mnProcessId = 6 Or mnProcessId = 10 Then
                            ' QuitCart()
                            EndSession()
                            mcCartCmd = ""
                            mnCartId = 0
                            mnProcessId = 0
                        Else

                            clearSessionCookie()
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
                        'hack for assure so we can choose not to redirect on quit
                        If myWeb.moRequest("redirect") <> "false" Then
                            If mcReturnPage Is Nothing Then mcReturnPage = ""
                            If Not myWeb.moRequest("redirect") Is Nothing Then
                                If myWeb.moRequest("redirect").StartsWith("/") Then mcReturnPage = myWeb.moRequest("redirect")
                            End If
                            myWeb.msRedirectOnEnd = mcSiteURL & mcReturnPage & IIf((mcSiteURL & mcReturnPage).Contains("?"), "&", "?") & "cartCmd=finish"
                            'myWeb.moResponse.Redirect(mcSiteURL & mcReturnPage & IIf((mcSiteURL & mcReturnPage).Contains("?"), "&", "?") & "cartCmd=finish")
                        End If

                    Case "Error"
                        GetCart(oElmt)

                    Case "Cart" 'Choose Shipping Costs

                        'If mnProcessId > 3 Then
                        '  ' when everything is ready we can show the invoice screen
                        '   mcCartCmd = "Confirm"
                        '   GoTo processFlow '   execute next step (make the payment)
                        'End If

                        '   info to display the cart
                        GetCart(oElmt)

                    Case "Discounts"

                        mcCartCmd = discountsProcess(oElmt)
                        If mcCartCmd <> "Discounts" Then
                            GoTo processFlow
                        End If


                    Case "RedirectSecure"
                        Dim cRedirectCommand As String
                        ' Set a Session variable flag to 
                        myWeb.moSession.Add("CartIsOn", "true")
                        bRedirect = True
                        If myWeb.moRequest("ewSettlement") Is Nothing Then
                            cRedirectCommand = "Logon"
                        Else
                            cRedirectCommand = "ChoosePaymentShippingOption"
                        End If
                        ' If a settlement has been initiated, then update the process
                        If mnProcessId = cartProcess.SettlementInitiated Then
                            mnProcessId = cartProcess.PassForPayment
                            moDBHelper.ExeProcessSql("update tblCartOrder set nCartStatus = '" & mnProcessId & "' where nCartOrderKey = " & mnCartId)
                        End If
                        ' pickup any google tracking code.
                        Dim item As Object
                        Dim cGoogleTrackingCode As String = ""

                        For Each item In myWeb.moRequest.QueryString
                            ' form needs to have this <form method="post" action="http://www.thissite.com" id="cart" onsubmit="pageTracker._linkByPost(this)">
                            ' the action URL is important
                            ' each querystring item in the google tracking code start with __utm
                            If InStr(CStr(item), "__utm") = 1 Then
                                cGoogleTrackingCode = cGoogleTrackingCode & "&" & CStr(item) & "=" & myWeb.moRequest.QueryString(CStr(item))
                            End If
                        Next item

                        myWeb.msRedirectOnEnd = mcPagePath & "cartCmd=" & cRedirectCommand & "&refSessionId=" & mcSessionId & cGoogleTrackingCode

                        ' myWeb.moResponse.Redirect(mcPagePath & "cartCmd=" & cRedirectCommand & "&refSessionId=" & mcSessionId & cGoogleTrackingCode)

                    Case "Logon", "LogonSubs" ' offer the user the ability to logon / register

                        If mbEwMembership = True And (moCartConfig("SkipLogon") <> "on" Or mcCartCmd = "LogonSubs") Then

                            'logon xform !!! We disable this because it is being brought in allready by .Web
                            If myWeb.mnUserId = 0 Then
                                'addtional string for membership to check
                                myWeb.moSession("cLogonCmd") = "cartCmd=Logon"
                                'registration xform
                                Dim oMembershipProv As New Providers.Membership.BaseProvider(myWeb, myWeb.moConfig("MembershipProvider"))
                                Dim oRegXform As AdminXForms = oMembershipProv.AdminXforms
                                oRegXform.open(moPageXml)
                                oRegXform.xFrmEditDirectoryItem(myWeb.mnUserId, "User", CInt("0" & moCartConfig("DefaultSubscriptionGroupId")), "CartRegistration")
                                If oRegXform.valid Then
                                    Dim sReturn As String = moDBHelper.validateUser(myWeb.moRequest("cDirName"), myWeb.moRequest("cDirPassword"))
                                    If IsNumeric(sReturn) Then
                                        myWeb.mnUserId = CLng(sReturn)
                                        Dim oUserElmt As XmlElement = moDBHelper.GetUserXML(myWeb.mnUserId)
                                        moPageXml.DocumentElement.AppendChild(oUserElmt)
                                        myWeb.moSession("nUserId") = myWeb.mnUserId
                                        mcCartCmd = "Notes"
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
                                mcCartCmd = "Notes"
                                GoTo processFlow
                            End If
                        Else
                            mcCartCmd = "Notes"
                            GoTo processFlow
                        End If
                    Case "Notes"

                        mcCartCmd = notesProcess(oElmt)
                        If mcCartCmd <> "Notes" Then
                            GoTo processFlow
                        End If

                    Case "SkipAddress" 'Check if order has Billing Address                

                        If usePreviousAddress(oElmt) = False Then
                            mcCartCmd = "Billing"
                        End If
                        If mcCartCmd <> "SkipAddress" Then
                            GoTo processFlow
                        End If

                    Case "Billing" 'Check if order has Billing Address                
                        GetCart(oElmt)
                        addressSubProcess(oElmt, "Billing Address")
                        GetCart(oElmt)
                        If mcCartCmd <> "Billing" Then
                            GoTo processFlow
                        End If

                    Case "Delivery" 'Check if order needs a Delivery Address

                        addressSubProcess(oElmt, "Delivery Address")
                        GetCart(oElmt)
                        If mcCartCmd <> "Delivery" Then
                            GoTo processFlow
                        End If

                    Case "ChoosePaymentShippingOption", "Confirm"  ' and confirm terms and conditions
                        mnProcessId = 4

                        GetCart(oElmt)

                        If mcCartCmd = "ChoosePaymentShippingOption" Then
                            If Not oContentElmt Is Nothing Then
                                AddToLists("Quote", oContentElmt)
                            End If
                        End If


                        If mcPaymentMethod <> "" And Not moCartXml.SelectSingleNode("Order/Shipping") Is Nothing Then
                            mnProcessId = 5
                            mcCartCmd = "EnterPaymentDetails"
                            '   execute next step unless form filled out wrong / not in db
                            GoTo processFlow
                        Else
                            Dim oOptionXform As xForm = optionsXform(oElmt)
                            If oOptionXform.valid Then
                                mnProcessId = 5
                                mcCartCmd = "EnterPaymentDetails"
                                '   execute next step unless form filled out wrong / not in db
                                GoTo processFlow
                            Else
                                Dim oContentsElmt As XmlElement = moPageXml.SelectSingleNode("/Page/Contents")
                                If oContentsElmt Is Nothing Then
                                    oContentsElmt = moPageXml.CreateElement("Contents")
                                    If moPageXml.DocumentElement Is Nothing Then
                                        Err.Raise(1004, "addressSubProcess", " PAGE IS NOT CREATED")
                                    Else
                                        moPageXml.DocumentElement.AppendChild(oContentsElmt)
                                    End If
                                End If
                                oContentsElmt.AppendChild(oOptionXform.moXformElmt)

                                'moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oOptionXform.moXformElmt)

                                If Not cRepeatPaymentError = "" Then
                                    oOptionXform.addNote(oOptionXform.moXformElmt.SelectSingleNode("group"), xForm.noteTypes.Alert, cRepeatPaymentError, True)
                                End If
                            End If
                        End If


                    Case "Redirect3ds"

                        Dim oEwProv As Protean.Cms.Cart.PaymentProviders = New PaymentProviders(myWeb)
                        Dim Redirect3dsXform As xForm = New xForm
                        Redirect3dsXform = oEwProv.GetRedirect3dsForm(myWeb)
                        moPageXml.SelectSingleNode("/Page/Contents").AppendChild(Redirect3dsXform.moXformElmt)
                        myWeb.moResponseType = pageResponseType.iframe

                    Case "EnterPaymentDetails", "SubmitPaymentDetails" 'confirm order and submit for payment
                        mnProcessId = 5

                        If oElmt.FirstChild Is Nothing Then
                            GetCart(oElmt)
                        End If

                        ' Add the date and reference to the cart

                        addDateAndRef(oElmt)

                        If mcPaymentMethod = "No Charge" Then
                            mcCartCmd = "ShowInvoice"
                            mnProcessId = Cart.cartProcess.Complete
                            GoTo processFlow
                        End If

                        Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, mcPaymentMethod)

                        Dim ccPaymentXform As xForm = New xForm

                        ccPaymentXform = oPayProv.Activities.GetPaymentForm(myWeb, Me, oElmt)

                        If InStr(mcPaymentMethod, "Repeat_") > 0 Then
                            If ccPaymentXform.valid = True Then
                                mcCartCmd = "ShowInvoice"
                                GoTo processFlow
                            ElseIf ccPaymentXform.isSubmitted Then
                                If ccPaymentXform.getSubmitted = "Cancel" Then
                                    mcCartCmd = "ChoosePaymentShippingOption"
                                    GoTo processFlow
                                Else
                                    'invalid redisplay form
                                End If
                            End If
                        End If


                        ' Don't show the payment screen if the stock levels are incorrect
                        If Not (oElmt.SelectSingleNode("error/msg") Is Nothing) Then
                            'oElmt.SelectSingleNode("error").PrependChild(oElmt.OwnerDocument.CreateElement("msg"))
                            'oElmt.SelectSingleNode("error").FirstChild.InnerXml = "<strong>PAYMENT CANNOT PROCEED UNTIL QUANTITIES ARE ADJUSTED</strong>"

                        ElseIf ccPaymentXform.valid = True Then

                            mcCartCmd = "ShowInvoice"

                            'Move this from "ShowInvoice" to prevent URL requests from confirming successful payment
                            If (mnProcessId <> Cart.cartProcess.DepositPaid And mnProcessId <> Cart.cartProcess.AwaitingPayment) Then
                                mnProcessId = Cart.cartProcess.Complete
                            End If

                            'remove the existing cart to force an update.
                            Dim oNodeCart As XmlNode
                            For Each oNodeCart In oElmt.SelectNodes("*")
                                oElmt.RemoveChild(oNodeCart)
                            Next
                            'oEwProv = Nothing
                            GoTo processFlow
                        Else


                            moPageXml.SelectSingleNode("/Page/Contents").AppendChild(ccPaymentXform.moXformElmt)
                        End If
                        'oEwProv = Nothing

                    Case "ShowInvoice", "ShowCallBackInvoice" 'Payment confirmed / show invoice

                        If mnProcessId <> Cart.cartProcess.Complete And mnProcessId <> Cart.cartProcess.AwaitingPayment Then
                            'check we are allready complete otherwise we will risk confirming sale just on URL request.
                            'myWeb.moDbHelper.logActivity(Protean.Cms.dbHelper.ActivityType.Alert, 0, 0, 0, "FAILED CALLBACK : " & mnProcessId)
                            mcCartCmd = "ChoosePaymentShippingOption"
                            GoTo processFlow
                        Else

                            PersistVariables()

                            If oElmt.FirstChild Is Nothing Then
                                GetCart(oElmt)
                            End If
                            If moCartConfig("StockControl") = "on" Then
                                UpdateStockLevels(oElmt)
                            End If
                            UpdateGiftListLevels()

                            addDateAndRef(oElmt)

                            If myWeb.mnUserId > 0 Then
                                Dim userXml As XmlElement = myWeb.moDbHelper.GetUserXML(myWeb.mnUserId, False)
                                If userXml IsNot Nothing Then
                                    Dim cartElement As XmlElement = oContentElmt.SelectSingleNode("Cart")
                                    If cartElement IsNot Nothing Then
                                        cartElement.AppendChild(cartElement.OwnerDocument.ImportNode(userXml, True))
                                    End If
                                End If
                            End If

                            purchaseActions(oContentElmt)
                            AddToLists("Invoice", oContentElmt)

                            If myWeb.mnUserId > 0 Then
                                If Not moSubscription Is Nothing Then
                                    moSubscription.AddUserSubscriptions(Me.mnCartId, myWeb.mnUserId, mnPaymentId)
                                End If
                            End If

                            emailReceipts(oContentElmt)


                            moDiscount.DisablePromotionalDiscounts()

                            If mbQuitOnShowInvoice Then
                                EndSession()
                            End If

                        End If

                    Case "CookiesDisabled" ' Cookies have been disabled or are undetectable
                        mnProcessError = 1
                        GetCart(oElmt)

                    Case "CartEmpty" ' Cookies have been disabled or are undetectable
                        mnProcessError = -1
                        GetCart(oElmt)

                    Case "BackToSite"
                        bRedirect = True
                        myWeb.moResponse.Redirect(mcSiteURL & mcReturnPage, False)
                        myWeb.moCtx.ApplicationInstance.CompleteRequest()

                    Case "List"
                        Dim nI As Integer = 0
                        If Not myWeb.moRequest.Item("OrderID") = "" Then nI = myWeb.moRequest.Item("OrderID")
                        GetCartSummary(oElmt)
                        ListOrders(nI)

                    Case "MakeCurrent"
                        Dim nI As Integer = 0
                        If Not myWeb.moRequest.Item("OrderID") = "" Then nI = myWeb.moRequest.Item("OrderID")
                        If Not nI = 0 Then MakeCurrent(nI)
                        mcCartCmd = "Cart"
                        GoTo processFlow

                    Case "Delete"
                        Dim nI As Integer = 0
                        If Not myWeb.moRequest.Item("OrderID") = "" Then nI = myWeb.moRequest.Item("OrderID")
                        If Not nI = 0 Then DeleteCart(nI)
                        mcCartCmd = "List"
                        GoTo processFlow

                    Case "Brief"
                        'Continue shopping
                        'go to the cart url
                        Dim cPage As String = myWeb.moRequest("pgid")
                        If cPage = "" Or cPage Is Nothing Then cPage = moPageXml.DocumentElement.GetAttribute("id")
                        cPage = "?pgid=" & cPage
                        myWeb.moResponse.Redirect(mcSiteURL & cPage, False)
                        myWeb.moCtx.ApplicationInstance.CompleteRequest()

                    Case "NoteToAddress"
                        'do nothing this is a placeholder for openquote
                        GetCart(oElmt)

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
                    oElmt.SetAttribute("siteUrl", mcSiteURL)
                    oElmt.SetAttribute("cartUrl", mcCartURL)
                End If

                AddCartToPage(moPageXml, oContentElmt)

                Exit Sub

            Catch ex As Exception
                If bRedirect = True And ex.GetType() Is GetType(System.Threading.ThreadAbortException) Then
                    'mbRedirect = True
                    'do nothing
                Else
                    returnException(mcModuleName, "apply", ex, "", cProcessInfo, gbDebug)
                End If

            End Try
        End Sub

        Sub AddCartToPage(moPageXml As XmlDocument, oContentElmt As XmlElement)
            Dim cProcessInfo As String = ""
            Try
                oContentElmt.SetAttribute("currencyRef", mcCurrencyRef)
                oContentElmt.SetAttribute("currency", mcCurrency)
                oContentElmt.SetAttribute("currencySymbol", mcCurrencySymbol)
                oContentElmt.SetAttribute("Process", mnProcessId)

                'remove any existing Cart 
                If Not moPageXml.DocumentElement.SelectSingleNode("Cart") Is Nothing Then
                    moPageXml.DocumentElement.RemoveChild(moPageXml.DocumentElement.SelectSingleNode("Cart"))
                End If

                moPageXml.DocumentElement.AppendChild(oContentElmt)
            Catch ex As Exception
                returnException(mcModuleName, "AddCartElement", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Overridable Sub AddBehaviour()
            Dim cProcessInfo As String = ""
            Try

                Select Case LCase(moCartConfig("AddBehaviour"))
                    Case "discounts"
                        mcCartCmd = "Discounts"
                    Case "notes"
                        mcCartCmd = "RedirectSecure"
                    Case "brief"
                        mcCartCmd = "Brief"
                    Case Else
                        'do nothing
                End Select

            Catch ex As Exception
                returnException(mcModuleName, "AddBehavior", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Overridable Function GetPaymentProvider() As PaymentProviders

            Dim oEwProv As PaymentProviders = New PaymentProviders(myWeb)
            Return oEwProv

        End Function

        Overridable Sub emailReceipts(ByRef oCartElmt As XmlElement)
            PerfMon.Log("Cart", "emailReceipts")
            Dim sMessageResponse As String
            Dim cProcessInfo As String = ""
            Try
                If LCase(moCartConfig("EmailReceipts")) <> "off" Then
                    ' Default subject line
                    Dim cSubject As String = moCartConfig("OrderEmailSubject")
                    If String.IsNullOrEmpty(cSubject) Then cSubject = "Website Order"

                    Dim CustomerEmailTemplatePath As String = IIf(moCartConfig("CustomerEmailTemplatePath") <> "", moCartConfig("CustomerEmailTemplatePath"), "/xsl/Cart/mailOrderCustomer.xsl")
                    Dim MerchantEmailTemplatePath As String = IIf(moCartConfig("MerchantEmailTemplatePath") <> "", moCartConfig("MerchantEmailTemplatePath"), "/xsl/Cart/mailOrderMerchant.xsl")

                    'send to customer
                    sMessageResponse = emailCart(oCartElmt, CustomerEmailTemplatePath, moCartConfig("MerchantName"), moCartConfig("MerchantEmail"), (oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText), cSubject,, moCartConfig("CustomerAttachmentTemplatePath"))

                    'Send to merchant
                    sMessageResponse = emailCart(oCartElmt, MerchantEmailTemplatePath, (oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText), (oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText), moCartConfig("MerchantEmail"), cSubject)

                    Dim oElmtEmail As XmlElement
                    oElmtEmail = moPageXml.CreateElement("Reciept")
                    oCartElmt.AppendChild(oElmtEmail)
                    oElmtEmail.InnerText = sMessageResponse

                    If sMessageResponse = "Message Sent" Then
                        oElmtEmail.SetAttribute("status", "sent")
                    Else
                        oElmtEmail.SetAttribute("status", "failed")
                    End If
                End If
            Catch ex As Exception
                returnException(mcModuleName, "emailReceipts", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub


        Overridable Sub purchaseActions(ByRef oCartElmt As XmlElement)
            PerfMon.Log("Cart", "purchaseActions")
            ' Dim sMessageResponse As String
            Dim cProcessInfo As String = ""
            Dim ocNode As XmlElement

            Try

                If moCartConfig("AccountingProvider") <> "" Then
                    Dim providerName = moCartConfig("AccountingProvider")
                    Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/accountingProviders")
                    Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(providerName).Type.ToString())
                    Dim calledType As Type
                    Dim classPath As String = moPrvConfig.Providers(providerName).Parameters("rootClass")
                    Dim methodName As String = "ProcessOrder"
                    calledType = assemblyInstance.GetType(classPath, True)
                    Dim o As Object = Activator.CreateInstance(calledType)

                    Dim args(0) As Object
                    args(0) = oCartElmt

                    calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, Nothing, o, args)

                End If


                For Each ocNode In oCartElmt.SelectNodes("descendant-or-self::Order/Item/productDetail[@purchaseAction!='']")
                    Dim classPath As String = ocNode.GetAttribute("purchaseAction")
                    Dim assemblyName As String = ocNode.GetAttribute("assembly")
                    Dim providerName As String = ocNode.GetAttribute("providerName")
                    Dim assemblyType As String = ocNode.GetAttribute("assemblyType")

                    Dim methodName As String = Right(classPath, Len(classPath) - classPath.LastIndexOf(".") - 1)

                    classPath = Left(classPath, classPath.LastIndexOf("."))

                    If classPath <> "" Then
                        Try
                            Dim calledType As Type

                            If assemblyName <> "" Then
                                classPath = classPath & ", " & assemblyName
                            End If
                            'Dim oModules As New Protean.Cms.Membership.Modules

                            If providerName <> "" Then
                                'case for external Providers
                                Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders")
                                Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(providerName).Type)
                                calledType = assemblyInstance.GetType(classPath, True)

                            ElseIf assemblyType <> "" Then
                                'case for external DLL's
                                Dim assemblyInstance As [Assembly] = [Assembly].Load(assemblyType)
                                calledType = assemblyInstance.GetType(classPath, True)
                            Else
                                'case for methods within EonicWeb Core DLL
                                calledType = System.Type.GetType(classPath, True)
                            End If

                            Dim o As Object = Activator.CreateInstance(calledType)

                            Dim args(1) As Object
                            args(0) = Me.myWeb
                            args(1) = ocNode

                            calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, Nothing, o, args)

                            'Error Handling ?
                            'Object Clearup ?


                        Catch ex As Exception
                            '  OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo))
                            cProcessInfo = classPath & "." & methodName & " not found"
                            ocNode.InnerXml = "<Content type=""error""><div>" & cProcessInfo & "</div></Content>"
                        End Try
                    End If

                Next

            Catch ex As Exception
                returnException(mcModuleName, "purchaseActions", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        ''' <summary>
        ''' This provides the ability to add customers to makreting lists if they reach a particular stage within a shopping cart.
        ''' Only works if 3rd party messaging provider enabled.
        ''' </summary>
        ''' <param name="StepName"></param>
        ''' <param name="oCartElmt"></param>
        Overridable Sub AddToLists(StepName As String, ByRef oCartElmt As XmlElement, Optional Name As String = "", Optional Email As String = "", Optional valDict As System.Collections.Generic.Dictionary(Of String, String) = Nothing)
            PerfMon.Log("Cart", "AddToLists")
            ' Dim sMessageResponse As String
            Dim cProcessInfo As String = ""
            Try
                Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                If Not moMailConfig Is Nothing Then

                    Dim sMessagingProvider As String = ""

                    If Not moMailConfig Is Nothing Then
                        sMessagingProvider = moMailConfig("MessagingProvider")
                    End If

                    If sMessagingProvider <> "" Or (moMailConfig("InvoiceList") <> "" And moMailConfig("QuoteList") <> "") Then
                        Dim oMessaging As New Protean.Providers.Messaging.BaseProvider(myWeb, sMessagingProvider)
                        If Email = "" Then Email = oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText
                        If Name = "" Then Name = oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText
                        If valDict Is Nothing Then valDict = New System.Collections.Generic.Dictionary(Of String, String)
                        For Each Attribute As XmlAttribute In oCartElmt.Attributes
                            If Not "errorMsg,hideDeliveryAddress,orderType,statusId,complete".Contains(Attribute.Name) Then
                                valDict.Add(Attribute.Name, Attribute.Value)
                            End If
                        Next
                        Dim ListId As String = ""
                        Select Case StepName
                            Case "Invoice"
                                ListId = moMailConfig("InvoiceList")
                                If moMailConfig("QuoteList") <> "" Then
                                    oMessaging.Activities.RemoveFromList(moMailConfig("QuoteList"), Email)
                                End If
                            Case "Quote"
                                If moMailConfig("QuoteList") <> "" Then
                                    oMessaging.Activities.RemoveFromList(moMailConfig("QuoteList"), Email)
                                End If
                                ListId = moMailConfig("QuoteList")
                            Case "Newsletter"
                                If moMailConfig("NewsletterList") <> "" Then
                                    oMessaging.Activities.RemoveFromList(moMailConfig("NewsletterList"), Email)
                                End If
                                ListId = moMailConfig("NewsletterList")
                        End Select
                        If ListId <> "" Then
                            oMessaging.Activities.addToList(ListId, Name, Email, valDict)
                        End If
                    End If
                End If

            Catch ex As Exception
                returnException(mcModuleName, "purchaseActions", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Private Sub RemoveDeliveryOption(ByVal nOrderId As Integer)
            Try
                Dim cSQL As String = "UPDATE tblCartOrder SET nShippingMethodId = 0, cShippingDesc = NULL, nShippingCost = 0 WHERE nCartOrderKey = " & nOrderId
                myWeb.moDbHelper.ExeProcessSqlorIgnore(cSQL)
            Catch ex As Exception
                returnException(mcModuleName, "RemoveDeliveryOption", ex, "", "", gbDebug)
            End Try
        End Sub
        'Sub GetCartSummary(ByRef oCartElmt As XmlElement, Optional ByVal nSelCartId As Integer = 0)
        '    PerfMon.Log("Cart", "GetCartSummary")
        '    '   Sets content for the XML to be displayed in the small summary plugin attached
        '    '   to the current content page

        '    Dim oDs As DataSet
        '    Dim sSql As String

        '    Dim cNotes As String
        '    Dim total As Double
        '    Dim quant As Integer
        '    Dim weight As Integer
        '    Dim nCheckPrice As Double
        '    Dim oRow As DataRow
        '    'We need to read this value from somewhere so we can change where vat is added
        '    'Currently defaulted to per line
        '    'If true will be added to the unit
        '    Dim nUnitVat As Decimal = 0
        '    Dim nLineVat As Decimal = 0

        '    Dim nCartIdUse As Integer
        '    If nSelCartId > 0 Then nCartIdUse = nSelCartId Else nCartIdUse = mnCartId

        '    Dim cProcessInfo As String = ""

        '    oCartElmt.SetAttribute("vatRate", moCartConfig("TaxRate"))
        '    Try


        '        If Not moSubscription Is Nothing Then moSubscription.CheckCartForSubscriptions(nCartIdUse, myWeb.mnUserId)

        '        If Not (nCartIdUse > 0) Then '   no shopping
        '            oCartElmt.SetAttribute("status", "Empty") '   set CartXML attributes
        '            oCartElmt.SetAttribute("itemCount", "0") '       to nothing
        '            oCartElmt.SetAttribute("vatRate", moCartConfig("TaxRate"))
        '            oCartElmt.SetAttribute("total", "0.00") '       for nothing
        '        Else
        '            '   otherwise

        '            '   and the address details we have obtained
        '            '   (if any)


        '            'Add Totals
        '            quant = 0 '   get number of items & sum of collective prices (ie. cart total) from db
        '            total = 0.0#
        '            weight = 0.0#

        '            sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, p.cContentXmlDetail as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, p.cContentSchemaName AS contentType from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" & nCartIdUse


        '            ':TODO we only want to check prices on current orders not history
        '            oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart")

        '            '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        '            oDs.Relations.Add("Rel1", oDs.Tables("Item").Columns("id"), oDs.Tables("Item").Columns("nParentId"), False)
        '            oDs.Relations("Rel1").Nested = True
        '            '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        '            For Each oRow In oDs.Tables("Item").Rows

        '                If moDBHelper.DBN2int(oRow("nParentId")) = 0 Then
        '                    Dim oOpRow As DataRow
        '                    Dim nOpPrices As Decimal = 0

        '                    If Not IsDBNull(oRow("productDetail")) Then
        '                        ' Go get the lowest price based on user and group
        '                        nCheckPrice = getProductPricesByXml(oRow("productDetail"), oRow("unit") & "", oRow("quantity"))

        '                        If Not moSubscription Is Nothing And oRow("contentType") = "Subscription" Then nCheckPrice = moSubscription.CartSubscriptionPrice(oRow("contentId"), myWeb.mnUserId)

        '                        If nCheckPrice > 0 And nCheckPrice <> oRow("price") Then
        '                            ' If price is lower, then update the item price field
        '                            oRow.BeginEdit()
        '                            oRow("price") = nCheckPrice
        '                            oRow.EndEdit()
        '                        End If

        '                        '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

        '                        For Each oOpRow In oRow.GetChildRows("Rel1")
        '                            'need an option check price bit here
        '                            Dim nNPrice As Decimal = getOptionPricesByXml(oRow("productDetail"), oRow("nItemOptGrpIdx"), oRow("nItemOptIdx"))
        '                            If nNPrice > 0 And nNPrice <> oOpRow("price") Then
        '                                nOpPrices += nNPrice
        '                                oOpRow.BeginEdit()
        '                                oOpRow("price") = nNPrice
        '                                oOpRow.EndEdit()
        '                            Else
        '                                nOpPrices += oOpRow("price")
        '                            End If
        '                        Next
        '                        'oRow.BeginEdit()
        '                        'oRow("price") = nOpPrices
        '                        'oRow.EndEdit()
        '                        '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        '                    End If

        '                    ' Apply stock control
        '                    If mbStockControl Then CheckStock(oCartElmt, oRow("productDetail"), oRow("quantity"))
        '                    ' Apply quantity control
        '                    CheckQuantities(oCartElmt, oRow("productDetail"), oRow("quantity"))

        '                    weight = weight + (oRow("weight") * oRow("quantity"))
        '                    quant = quant + oRow("quantity")

        '                    If moCartConfig("DiscountDirection") = "up" Then
        '                        total = total + (oRow("quantity") * (oRow("price") + nOpPrices)) + CDec("0" & oRow("discount"))
        '                    Else
        '                        total = total + (oRow("quantity") * (oRow("price") + nOpPrices)) - CDec("0" & oRow("discount"))
        '                    End If

        '                    'Round( Price * Vat ) * Quantity
        '                    nUnitVat += RoundUp(((oRow("price") + nOpPrices)) - CDec("0" & oRow("discount")) * (mnTaxRate / 100)) * oRow("quantity")
        '                    'Round( ( Price * Quantity )* VAT )
        '                    nLineVat += RoundUp((((oRow("price") + nOpPrices) - CDec("0" & oRow("discount"))) * oRow("quantity")) * (mnTaxRate / 100))
        '                End If
        '                Dim cUpdtSQL As String = "UPDATE tblCartItem SET nPrice = " & oRow("price") & " WHERE nCartItemKey = " & oRow("id")
        '                moDBHelper.exeProcessSQLScalar(cUpdtSQL)
        '            Next


        '            ' Quantity based error messaging
        '            Dim oError As XmlElement = oCartElmt.SelectSingleNode("error")
        '            If Not (oError Is Nothing) Then
        '                Dim oMsg As XmlElement
        '                oMsg = oCartElmt.OwnerDocument.CreateElement("msg")
        '                oMsg.SetAttribute("type", "zz_footer") ' Forces it to the bottom of the message block
        '                oMsg.InnerText = "Please adjust the quantities you require, or call for assistance."
        '                oError.AppendChild(oMsg)
        '            End If

        '            'moDBHelper.updateDataset(oDs, "Item", True)

        '            sSql = "select cClientNotes from tblCartOrder where nCartOrderKey=" & nCartIdUse
        '            Dim oNotes As XmlElement = oCartElmt.OwnerDocument.CreateElement("Notes")
        '            oNotes.InnerXml = moDBHelper.exeProcessSQLScalar(sSql)
        '            total -= moDiscount.CheckDiscounts(oDs, oCartElmt, False, oNotes)

        '            oCartElmt.SetAttribute("status", "Active")
        '            oCartElmt.SetAttribute("itemCount", quant)
        '            oCartElmt.SetAttribute("vatRate", moCartConfig("TaxRate"))
        '            oCartElmt.SetAttribute("total", total)
        '            oCartElmt.SetAttribute("weight", weight)
        '            oCartElmt.SetAttribute("orderType", mmcOrderType & "")

        '            'Adding notes to cart summary
        '            sSql = "Select cClientNotes from tblCartOrder where nCartOrderKey = " & nCartIdUse
        '            cNotes = moDBHelper.DBN2Str(moDBHelper.exeProcessSQLScalar(sSql), False, False)
        '            sSql = "Select nCartStatus  from tblCartOrder where nCartOrderKey = " & nCartIdUse
        '            If moDBHelper.DBN2Str(moDBHelper.exeProcessSQLScalar(sSql), False, False) = 6 Then
        '                oCartElmt.SetAttribute("complete", "true")
        '            Else
        '                oCartElmt.SetAttribute("complete", "true")
        '            End If
        '            Dim oElmt As XmlElement
        '            If Not (cNotes = "") Then
        '                oElmt = moPageXml.CreateElement("Notes")
        '                oElmt.InnerXml = cNotes
        '                oCartElmt.AppendChild(oElmt.FirstChild())
        '            End If
        '        End If

        '    Catch ex As Exception
        '        returnException(mcModuleName, "GetCartSummary", ex, "", cProcessInfo, gbDebug)
        '    End Try

        'End Sub
        'Public Sub GetCartSummary(ByRef oCartElmt As XmlElement, Optional ByVal nSelCartId As Integer = 0)
        '    'made same as get cart
        '    PerfMon.Log("Cart", "GetCartSummary")
        '    '   Content for the XML that will display all the information stored for the Cart
        '    '   This is a list of cart items (and quantity, price ...), totals,
        '    '   billing & delivery addressi and delivery method.

        '    Dim oDs As DataSet

        '    Dim sSql As String
        '    Dim oRow As DataRow

        '    Dim oElmt As XmlElement
        '    Dim oElmt2 As XmlElement
        '    Dim oXml As XmlDataDocument

        '    Dim quant As Integer
        '    Dim weight As Double
        '    Dim total As Double
        '    Dim vatAmt As Double
        '    Dim shipCost As Double
        '    Dim nCheckPrice As Double
        '    'We need to read this value from somewhere so we can change where vat is added
        '    'Currently defaulted to per line
        '    'If true will be added to the unit
        '    Dim nUnitVat As Decimal = 0
        '    Dim nLineVat As Decimal = 0


        '    Dim cProcessInfo As String = ""
        '    Dim cOptionGroupName As String = ""

        '    Dim nCartIdUse As Integer
        '    If nSelCartId > 0 Then nCartIdUse = nSelCartId Else nCartIdUse = mnCartId


        '    Try

        '        If Not moSubscription Is Nothing Then moSubscription.CheckCartForSubscriptions(nCartIdUse, myWeb.mnUserId)

        '        If Not (nCartIdUse > 0) Then '   no shopping
        '            oCartElmt.SetAttribute("status", "Empty") '   set CartXML attributes
        '            oCartElmt.SetAttribute("itemCount", "0") '       to nothing
        '            oCartElmt.SetAttribute("vatRate", moCartConfig("TaxRate"))
        '            oCartElmt.SetAttribute("total", "0.00") '       for nothing

        '        Else
        '            '   otherwise

        '            '   and the address details we have obtained
        '            '   (if any)
        '            'Add Totals
        '            quant = 0 '   get number of items & sum of collective prices (ie. cart total) from db
        '            total = 0.0#
        '            weight = 0.0#

        '            sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, p.cContentXmlDetail as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, p.cContentSchemaName AS contentType, dbo.fxn_getContentParents(i.nItemId) as parId  from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" & nCartIdUse

        '            oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart")
        '            'add relationship for options
        '            oDs.Relations.Add("Rel1", oDs.Tables("Item").Columns("id"), oDs.Tables("Item").Columns("nParentId"), False)
        '            oDs.Relations("Rel1").Nested = True
        '            '
        '            For Each oRow In oDs.Tables("Item").Rows
        '                If moDBHelper.DBN2int(oRow("nParentId")) = 0 Then
        '                    ' Go get the lowest price based on user and group
        '                    If Not IsDBNull(oRow("productDetail")) Then
        '                        nCheckPrice = getProductPricesByXml(oRow("productDetail"), oRow("unit") & "", oRow("quantity"))

        '                        If Not moSubscription Is Nothing And oRow("contentType") = "Subscription" Then nCheckPrice = moSubscription.CartSubscriptionPrice(oRow("contentId"), myWeb.mnUserId)
        '                    End If
        '                    If nCheckPrice > 0 And nCheckPrice <> oRow("price") Then
        '                        ' If price is lower, then update the item price field
        '                        'oRow.BeginEdit()
        '                        oRow("price") = nCheckPrice
        '                        'oRow.EndEdit()
        '                    End If

        '                    'option prices
        '                    Dim oOpRow As DataRow
        '                    Dim nOpPrices As Decimal = 0
        '                    For Each oOpRow In oRow.GetChildRows("Rel1")

        '                        Dim nNPrice As Decimal = getOptionPricesByXml(oRow("productDetail"), oRow("nItemOptGrpIdx"), oRow("nItemOptIdx"))
        '                        If nNPrice > 0 And nNPrice <> oOpRow("price") Then
        '                            nOpPrices += nNPrice
        '                            'oOpRow.BeginEdit()
        '                            oOpRow("price") = nNPrice

        '                            'oOpRow.EndEdit()
        '                        Else
        '                            nOpPrices += oOpRow("price")
        '                        End If
        '                    Next

        '                    ' Apply stock control
        '                    If mbStockControl Then CheckStock(oCartElmt, oRow("productDetail"), oRow("quantity"))
        '                    ' Apply quantity control
        '                    CheckQuantities(oCartElmt, oRow("productDetail"), oRow("quantity"))

        '                    weight = weight + (oRow("weight") * oRow("quantity"))
        '                    quant = quant + oRow("quantity")
        '                    total = total + (oRow("quantity") * (oRow("price") + nOpPrices))

        '                    'Round( Price * Vat ) * Quantity
        '                    ' nUnitVat += Round((oRow("price") + nOpPrices) * (mnTaxRate / 100), , , mbRoundup) * oRow("quantity")
        '                    'Round( ( Price * Quantity )* VAT )
        '                    '  nLineVat += Round((((oRow("price") + nOpPrices)) * oRow("quantity")) * (mnTaxRate / 100), , , mbRoundup)

        '                End If
        '                'Dim ix As Integer
        '                'Dim xstr As String
        '                'For ix = 0 To oRow.Table.Columns.Count - 1
        '                '    xstr &= oRow.Table.Columns(ix).ColumnName & "="
        '                '    xstr &= oRow(ix) & ", "
        '                'Next

        '                Dim cUpdtSQL As String = "UPDATE tblCartItem SET nPrice = " & oRow("price") & " WHERE nCartItemKey = " & oRow("id")
        '                moDBHelper.exeProcessSQLScalar(cUpdtSQL)
        '            Next


        '            ' Quantity based error messaging
        '            Dim oError As XmlElement = oCartElmt.SelectSingleNode("error")
        '            If Not (oError Is Nothing) Then
        '                Dim oMsg As XmlElement
        '                oMsg = oCartElmt.OwnerDocument.CreateElement("msg")
        '                oMsg.SetAttribute("type", "zz_footer") ' Forces it to the bottom of the message block
        '                oMsg.InnerText = "Please adjust the quantities you require, or call for assistance."
        '                oError.AppendChild(oMsg)
        '            End If


        '            'moDBHelper.updateDataset(oDs, "Item", True)

        '            '   add to Cart XML
        '            sSql = "Select nCartStatus from tblCartOrder where nCartOrderKey = " & mnCartId
        '            Dim nStatusId As Long = moDBHelper.DBN2Str(moDBHelper.exeProcessSQLScalar(sSql), False, False)
        '            oCartElmt.SetAttribute("statusId", nStatusId)
        '            oCartElmt.SetAttribute("status", Me.getProcessName(nStatusId))
        '            oCartElmt.SetAttribute("itemCount", quant)
        '            oCartElmt.SetAttribute("weight", weight)
        '            oCartElmt.SetAttribute("orderType", mmcOrderType & "")

        '            If nStatusId = 6 Then
        '                oCartElmt.SetAttribute("complete", "true")
        '            Else
        '                oCartElmt.SetAttribute("complete", "true")
        '            End If
        '            'Add the addresses to the dataset
        '            If nCartIdUse > 0 Then
        '                sSql = "select cContactType as type, cContactName as GivenName, cContactCompany as Company, cContactAddress as Street, cContactCity as City, cContactState as State, cContactZip as PostalCode, cContactCountry as Country, cContactTel as Telephone, cContactFax as Fax, cContactEmail as Email, cContactXml as Details from tblCartContact where nContactCartId=" & nCartIdUse
        '                moDBHelper.addTableToDataSet(oDs, sSql, "Contact")
        '            End If
        '            'Add Items - note - do this AFTER we've updated the prices! 

        '            If oDs.Tables("Item").Rows.Count > 0 Then
        '                'cart items
        '                oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(5).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(6).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(7).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(8).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(9).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(10).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns(11).ColumnMapping = Data.MappingType.Attribute
        '                oDs.Tables(0).Columns("parId").ColumnMapping = Data.MappingType.Attribute
        '                'cart contacts
        '                oDs.Tables("Contact").Columns(0).ColumnMapping = Data.MappingType.Attribute

        '                oXml = New XmlDataDocument(oDs)
        '                oDs.EnforceConstraints = False
        '                'Convert the detail to xml
        '                For Each oElmt In oXml.SelectNodes("/Cart/Item/productDetail | /Cart/Contact/Detail | /Cart/Contact/Details")
        '                    oElmt.InnerXml = oElmt.InnerText
        '                    If Not oElmt.SelectSingleNode("Content") Is Nothing Then
        '                        oElmt.InnerXml = oElmt.SelectSingleNode("Content").InnerXml
        '                    End If
        '                Next

        '                'get the option xml
        '                For Each oElmt In oXml.SelectNodes("/Cart/Item/Item/productDetail")
        '                    oElmt.InnerXml = oElmt.InnerText

        '                    Dim nGroupIndex As String = oElmt.ParentNode.SelectSingleNode("nItemOptGrpIdx").InnerText
        '                    Dim nOptionIndex As String = oElmt.ParentNode.SelectSingleNode("nItemOptIdx").InnerText
        '                    cOptionGroupName = ""
        '                    If Not oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/@name") Is Nothing Then
        '                        cOptionGroupName = oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/@name").InnerText
        '                    End If
        '                    If nOptionIndex >= 0 Then

        '                        oElmt2 = oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/option[" & nOptionIndex & "]")
        '                        If cOptionGroupName <> "" Then oElmt2.SetAttribute("groupName", cOptionGroupName)
        '                        oElmt.ParentNode.InnerXml = oElmt2.OuterXml
        '                    Else
        '                        'case for text option
        '                        oElmt2 = oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/option[1]")
        '                        If cOptionGroupName <> "" Then oElmt2.SetAttribute("groupName", cOptionGroupName)
        '                        oElmt2.SetAttribute("name", oElmt.ParentNode.SelectSingleNode("Name").InnerText)
        '                        oElmt.ParentNode.InnerXml = oElmt2.OuterXml
        '                    End If

        '                Next

        '                oElmt = moPageXml.CreateElement("Cart")
        '                ' Note: Preserve the original elements in oCartElmt
        '                'oCartElmt.InnerXml = oCartElmt.InnerXml + oXml.FirstChild.InnerXml


        '            End If
        '            myWeb.CheckMultiParents(oCartElmt)
        '            sSql = "select cClientNotes from tblCartOrder where nCartOrderKey=" & nCartIdUse
        '            Dim oNotes As XmlElement = oCartElmt.OwnerDocument.CreateElement("Notes")
        '            oNotes.InnerXml = moDBHelper.exeProcessSQLScalar(sSql)
        '            total -= moDiscount.CheckDiscounts(oDs, oCartElmt, True, oNotes)

        '            oXml = Nothing
        '            oDs = Nothing

        '            If mbNoDeliveryAddress Then oCartElmt.SetAttribute("hideDeliveryAddress", "true")
        '            If mnGiftListId > 0 Then oCartElmt.SetAttribute("giftListId", mnGiftListId)

        '            sSql = "select * from tblCartOrder where nCartOrderKey=" & nCartIdUse

        '            oDs = moDBHelper.getDataSet(sSql, "Order", "Cart")

        '            For Each oRow In oDs.Tables("Order").Rows
        '                shipCost = CDbl("0" & oRow("nShippingCost"))
        '                oCartElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
        '                oCartElmt.SetAttribute("shippingCost", shipCost & "")
        '                oCartElmt.SetAttribute("shippingDesc", oRow("cShippingDesc") & "")

        '                If oRow("nShippingMethodId") > 0 Then
        '                    getShippingDetailXml(oCartElmt, oRow("nShippingMethodId"))
        '                End If

        '                If mnTaxRate > 0 Then
        '                    'we calculate vat at the end after we have applied discounts etc
        '                    For Each oElmt In oXml.SelectNodes("/Cart/Item")
        '                        Dim nOpPrices As Long = 0

        '                        'get the prices of options to calculate vat
        '                        For Each oElmt2 In oXml.SelectNodes("/Item")
        '                            nOpPrices += oElmt2.GetAttribute("price")
        '                        Next

        '                        If mbVatAtUnit Then
        '                            'Round( Price * Vat ) * Quantity
        '                            nLineVat = Round((oElmt.GetAttribute("price") + nOpPrices) * (mnTaxRate / 100), , , mbRoundup) * oElmt.GetAttribute("quantity"))
        '                        Else
        '                            'Round( ( Price * Quantity )* VAT )
        '                            nLineVat = Round((((oRow("price") + nOpPrices)) * oRow("quantity")) * (mnTaxRate / 100), , , mbRoundup)
        '                        End If

        '                        ' oElmt.SetAttribute("itemTax", nLineVat)
        '                        vatAmt += nLineVat
        '                    Next

        '                    vatAmt = Round((shipCost) * (mnTaxRate / 100), , , mbRoundup) + vatAmt

        '                    oCartElmt.SetAttribute("totalNet", FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False))
        '                    oCartElmt.SetAttribute("vatRate", mnTaxRate)
        '                    oCartElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
        '                    oCartElmt.SetAttribute("shippingCost", FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False))
        '                    oCartElmt.SetAttribute("vatAmt", FormatNumber(vatAmt, 2, TriState.True, TriState.False, TriState.False))
        '                    oCartElmt.SetAttribute("total", FormatNumber(total + shipCost + vatAmt, 2, TriState.True, TriState.False, TriState.False))
        '                Else
        '                    oCartElmt.SetAttribute("totalNet", FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False))
        '                    oCartElmt.SetAttribute("vatRate", 0.0#)
        '                    oCartElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
        '                    oCartElmt.SetAttribute("shippingCost", FormatNumber(shipCost, 2, TriState.True, TriState.False, TriState.False))
        '                    oCartElmt.SetAttribute("vatAmt", 0.0#)
        '                    oCartElmt.SetAttribute("total", FormatNumber(total + shipCost, 2, TriState.True, TriState.False, TriState.False))
        '                End If
        '            Next
        '        End If



        '    Catch ex As Exception
        '        returnException(mcModuleName, "GetCartSummary", ex, "", cProcessInfo, gbDebug)
        '    End Try

        'End Sub

        ''' <summary>
        ''' This does the same as get cart without the item information, so we call get cart and delete any items we find
        ''' </summary>
        ''' <param name="oCartElmt"></param>
        ''' <param name="nSelCartId"></param>
        ''' <remarks></remarks>
        Sub GetCartSummary(ByRef oCartElmt As XmlElement, Optional ByVal nSelCartId As Integer = 0)
            PerfMon.Log("Cart", "GetCartSummary")
            '   Sets content for the XML to be displayed in the small summary plugin attached
            '   to the current content page
            Dim oElmt As XmlElement
            Dim nCartIdUse As Integer
            If nSelCartId > 0 Then
                nCartIdUse = nSelCartId
            Else
                nCartIdUse = mnCartId
            End If
            Dim cProcessInfo As String = "CartId=" & nCartIdUse
            Try
                GetCart(oCartElmt, nCartIdUse)
                'remove all the items
                For Each oElmt In oCartElmt.SelectNodes("/Cart/Order/Item")
                    oElmt.ParentNode.RemoveChild(oElmt)
                Next

            Catch ex As Exception
                returnException(mcModuleName, "GetCartSummary", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Sub GetCart()
            GetCart(moCartXml.FirstChild)
        End Sub

        Public Sub GetCart(ByRef oCartElmt As XmlElement, Optional ByVal nSelCartId As Integer = 0)
            oCartElmt.InnerXml = ""
            PerfMon.Log("Cart", "GetCart")
            '   Content for the XML that will display all the information stored for the Cart
            '   This is a list of cart items (and quantity, price ...), totals,
            '   billing & delivery addressi and delivery method.

            Dim oDs As DataSet
            Dim oDs2 As DataSet

            Dim sSql As String
            Dim oRow As DataRow
            Dim oRow2 As DataRow

            Dim oElmt As XmlElement
            Dim oElmt2 As XmlElement
            Dim oXml As XmlDocument

            Dim quant As Long
            Dim weight As Double
            Dim total As Double
            Dim vatAmt As Double
            Dim shipCost As Double
            Dim nCheckPrice As Double
            Dim oCheckPrice As XmlElement
            'We need to read this value from somewhere so we can change where vat is added
            'Currently defaulted to per line
            'If true will be added to the unit
            Dim nLineVat As Decimal = 0
            Dim bCheckSubscriptions = False

            Dim cOptionGroupName As String = ""

            Dim nCartIdUse As Integer
            If nSelCartId > 0 Then
                nCartIdUse = nSelCartId
            Else
                nCartIdUse = mnCartId
            End If

            Dim oldCartId As Long = mnCartId

            Dim cProcessInfo As String = "CartId=" & nCartIdUse

            'For related products
            Dim oItemList As New Hashtable ' for related Products
            If moCartConfig("RelatedProductsInCart") = "on" Then


            End If
            Try

                If Not moSubscription Is Nothing And nCartIdUse <> 0 Then
                    bCheckSubscriptions = moSubscription.CheckCartForSubscriptions(nCartIdUse, myWeb.mnUserId)
                    If moCartConfig("subCheck") = "always" Then
                        bCheckSubscriptions = True
                    End If
                End If


                If Not (nCartIdUse > 0) Then '   no shopping
                    oCartElmt.SetAttribute("status", "Empty") '   set CartXML attributes
                    oCartElmt.SetAttribute("itemCount", "0") '       to nothing
                    oCartElmt.SetAttribute("vatRate", moCartConfig("TaxRate"))
                    oCartElmt.SetAttribute("total", "0.00") '       for nothing

                Else
                    '   otherwise
                    oCartElmt.SetAttribute("cartId", mnCartId)
                    oCartElmt.SetAttribute("session", mcSessionId)
                    ' Check tax rate
                    UpdateTaxRate()

                    '   and the address details we have obtained
                    '   (if any)
                    'Add Totals
                    quant = 0 '   get number of items & sum of collective prices (ie. cart total) from db
                    total = 0.0#
                    weight = 0.0#

                    ' Process promo code from external refs.
                    If myWeb.moSession("promocode") <> "" Or myWeb.moRequest("promocode") <> "" Then
                        If myWeb.moRequest("promocode") <> "" Then
                            promocodeFromExternalRef = myWeb.moRequest("promocode").ToString
                        ElseIf myWeb.moSession("promocode") <> "" Then
                            promocodeFromExternalRef = myWeb.moSession("promocode").ToString
                        End If
                        myWeb.moSession("promocode") = ""
                    End If
                    If Not String.IsNullOrEmpty(promocodeFromExternalRef) Then
                        oCartElmt.SetAttribute("promocodeFromExternalRef", promocodeFromExternalRef)
                    End If

                    If moDBHelper.checkTableColumnExists("tblCartItem", "xItemXml") Then
                        sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, i.xItemXml as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, i.xItemXml.value('Content[1]/@type','nvarchar(50)') AS contentType, dbo.fxn_getContentParents(i.nItemId) as parId  from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" & nCartIdUse
                    Else
                        sSql = "select i.nCartItemKey as id, i.nItemId as contentId, i.cItemRef as ref, i.cItemURL as url, i.cItemName as Name, i.cItemUnit as unit, i.nPrice as price, i.nTaxRate as taxRate, i.nQuantity as quantity, i.nShpCat as shippingLevel, i.nDiscountValue as discount,i.nWeight as weight, p.cContentXmlDetail as productDetail, i.nItemOptGrpIdx, i.nItemOptIdx, i.nParentId, p.cContentSchemaName AS contentType, dbo.fxn_getContentParents(i.nItemId) as parId  from tblCartItem i left join tblContent p on i.nItemId = p.nContentKey where nCartOrderId=" & nCartIdUse
                    End If

                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart")
                    'add relationship for options
                    oDs.Relations.Add("Rel1", oDs.Tables("Item").Columns("id"), oDs.Tables("Item").Columns("nParentId"), False)
                    oDs.Relations("Rel1").Nested = True
                    '
                    For Each oRow In oDs.Tables("Item").Rows

                        Dim Discount As Double = 0

                        If Not oItemList.ContainsValue(oRow("contentId")) Then
                            oItemList.Add(oItemList.Count, oRow("contentId"))
                        End If

                        If moDBHelper.DBN2int(oRow("nParentId")) = 0 Then
                            Dim nTaxRate As Long = 0
                            Dim bOverridePrice As Boolean = False
                            If Not mbOveridePrice Then ' for openquote
                                ' Go get the lowest price based on user and group
                                If Not IsDBNull(oRow("productDetail")) Then

                                    Dim oProd As XmlElement = moPageXml.CreateElement("product")
                                    oProd.InnerXml = oRow("productDetail")
                                    If oProd.SelectSingleNode("Content[@overridePrice='true']") Is Nothing Then
                                        oCheckPrice = getContentPricesNode(oProd, oRow("unit") & "", oRow("quantity"))
                                        cProcessInfo = "Error getting price for unit:" & oRow("unit") & " and Quantity:" & oRow("quantity") & " and Currency " & mcCurrencyRef & " Check that a price is available for this quantity and a group for this current user."
                                        If Not oCheckPrice Is Nothing Then
                                            nCheckPrice = oCheckPrice.InnerText
                                            nTaxRate = getProductTaxRate(oCheckPrice)
                                        End If
                                        'nCheckPrice = getProductPricesByXml(oRow("productDetail"), oRow("unit") & "", oRow("quantity"))

                                        If Not moSubscription Is Nothing And CStr(oRow("contentType") & "") = "Subscription" Then

                                            Dim revisedPrice As Double
                                            If oRow("contentId") > 0 Then
                                                revisedPrice = moSubscription.CartSubscriptionPrice(oRow("contentId"), myWeb.mnUserId)
                                            Else
                                                oCheckPrice = getContentPricesNode(oProd, oRow("unit") & "", oRow("quantity"), "SubscriptionPrices")
                                                nCheckPrice = oCheckPrice.InnerText
                                                nTaxRate = getProductTaxRate(oCheckPrice)
                                            End If
                                            If revisedPrice < nCheckPrice Then
                                                'nCheckPrice = revisedPrice
                                                Discount = nCheckPrice - revisedPrice
                                                nCheckPrice = revisedPrice
                                            End If

                                        End If
                                    Else
                                        bOverridePrice = True
                                    End If

                                End If
                                If Not bOverridePrice Then
                                    If nCheckPrice > 0 And nCheckPrice <> oRow("price") Then
                                        ' If price is lower, then update the item price field
                                        'oRow.BeginEdit()
                                        oRow("price") = nCheckPrice
                                        'oRow("taxRate") = nTaxRate
                                        'oRow.EndEdit()
                                    End If

                                    If oRow("taxRate") <> nTaxRate Then
                                        oRow("taxRate") = nTaxRate
                                    End If
                                End If
                            End If

                            'option prices
                            Dim oOpRow As DataRow
                            Dim nOpPrices As Decimal = 0
                            For Each oOpRow In oRow.GetChildRows("Rel1")
                                If Not mbOveridePrice Then ' for openquote
                                    Dim nNPrice As Decimal = getOptionPricesByXml(oRow("productDetail"), oRow("nItemOptGrpIdx"), oRow("nItemOptIdx"))
                                    If nNPrice > 0 And nNPrice <> oOpRow("price") Then
                                        nOpPrices += nNPrice
                                        'oOpRow.BeginEdit()
                                        oOpRow("price") = nNPrice

                                        'oOpRow.EndEdit()
                                    Else
                                        nOpPrices += oOpRow("price")
                                    End If
                                End If
                            Next

                            ' Apply stock control
                            If mbStockControl Then CheckStock(oCartElmt, oRow("productDetail"), oRow("quantity"))
                            ' Apply quantity control
                            If Not IsDBNull(oRow("productDetail")) Then
                                'not sure why the product has no detail but if it not we skip this, suspect it was old test data that raised this issue.
                                CheckQuantities(oCartElmt, oRow("productDetail") & "", CLng("0" & oRow("quantity")))
                            End If

                            weight = weight + (oRow("weight") * oRow("quantity"))
                            quant = quant + oRow("quantity")

                            total = total + (oRow("quantity") * Round(oRow("price") + nOpPrices, , , mbRoundup))

                            'we do this later after we have applied discounts

                            'Round( Price * Vat ) * Quantity
                            ' nUnitVat += Round((oRow("price") + nOpPrices) * (mnTaxRate / 100), , , mbRoundup) * oRow("quantity")
                            'Round( ( Price * Quantity )* VAT )
                            '  nLineVat += Round((((oRow("price") + nOpPrices)) * oRow("quantity")) * (mnTaxRate / 100), , , mbRoundup)
                        End If
                        'Dim ix As Integer
                        'Dim xstr As String
                        'For ix = 0 To oRow.Table.Columns.Count - 1
                        '    xstr &= oRow.Table.Columns(ix).ColumnName & "="
                        '    xstr &= oRow(ix) & ", "
                        'Next

                        Dim discountSQL As String = ""
                        If Discount <> 0 Then
                            '     discountSQL = ", nDiscountValue = " & Discount & " "
                        End If

                        Dim cUpdtSQL As String = "UPDATE tblCartItem Set nPrice = " & oRow("price") & discountSQL & " WHERE nCartItemKey = " & oRow("id")
                        moDBHelper.ExeProcessSql(cUpdtSQL)


                    Next


                    ' Quantity based error messaging
                    Dim oError As XmlElement = oCartElmt.SelectSingleNode("Error")
                    If Not (oError Is Nothing) Then
                        Dim oMsg As XmlElement
                        oMsg = oCartElmt.OwnerDocument.CreateElement("msg")
                        oMsg.SetAttribute("type", "zz_footer") ' Forces it to the bottom of the message block
                        oMsg.InnerText = "<span Class=""term3081"">Please adjust the quantities you require, Or Call For assistance.</span>"
                        oError.AppendChild(oMsg)
                    End If


                    'moDBHelper.updateDataset(oDs, "Item", True)

                    '   add to Cart XML
                    sSql = "Select nCartStatus from tblCartOrder where nCartOrderKey = " & nCartIdUse
                    Dim nStatusId As Long = moDBHelper.DBN2Str(moDBHelper.ExeProcessSqlScalar(sSql), False, False)
                    oCartElmt.SetAttribute("statusId", nStatusId)
                    oCartElmt.SetAttribute("status", Me.getProcessName(nStatusId))
                    oCartElmt.SetAttribute("itemCount", quant)
                    oCartElmt.SetAttribute("weight", weight)
                    oCartElmt.SetAttribute("orderType", mmcOrderType & "")

                    If nStatusId = 6 Then
                        oCartElmt.SetAttribute("complete", "True")
                    Else
                        oCartElmt.SetAttribute("complete", "True")
                    End If

                    'Add the addresses to the dataset
                    If nCartIdUse > 0 Then
                        sSql = "Select cContactType As type, cContactName As GivenName, cContactCompany As Company, cContactAddress As Street, cContactCity As City, cContactState As State, cContactZip As PostalCode, cContactCountry As Country, cContactTel As Telephone, cContactFax As Fax, cContactEmail As Email, cContactXml As Details from tblCartContact where nContactCartId=" & nCartIdUse
                        moDBHelper.addTableToDataSet(oDs, sSql, "Contact")
                    End If

                    'Add Items - note - do this AFTER we've updated the prices! 

                    If oDs.Tables("Item").Rows.Count > 0 Then
                        'cart items
                        oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(5).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(6).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(7).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(8).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(9).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(10).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns(11).ColumnMapping = Data.MappingType.Attribute
                        oDs.Tables(0).Columns("parId").ColumnMapping = Data.MappingType.Attribute
                        'cart contacts
                        oDs.Tables("Contact").Columns(0).ColumnMapping = Data.MappingType.Attribute

                        oXml = New XmlDocument
                        oXml.LoadXml(oDs.GetXml)

                        oDs.EnforceConstraints = False

                        'Convert the detail to xml
                        For Each oElmt In oXml.SelectNodes("/Cart/Item/productDetail | /Cart/Contact/Detail | /Cart/Contact/Details")
                            oElmt.InnerXml = oElmt.InnerText
                            If Not oElmt.SelectSingleNode("Content") Is Nothing Then
                                Dim oAtt As XmlAttribute
                                For Each oAtt In oElmt.SelectSingleNode("Content").Attributes
                                    oElmt.SetAttribute(oAtt.Name, oAtt.Value)
                                Next
                                oElmt.InnerXml = oElmt.SelectSingleNode("Content").InnerXml
                                Dim oContent As XmlElement = oElmt.SelectSingleNode("Content")

                            End If
                        Next

                        For Each oElmt In oXml.SelectNodes("/Cart/Contact/Email")
                            If moDBHelper.CheckOptOut(oElmt.InnerText) Then
                                oElmt.SetAttribute("optOut", "True")
                            End If
                        Next

                        'get the option xml
                        For Each oElmt In oXml.SelectNodes("/Cart/Item/Item/productDetail")
                            oElmt.InnerXml = oElmt.InnerText

                            Dim nGroupIndex As String = oElmt.ParentNode.SelectSingleNode("nItemOptGrpIdx").InnerText
                            Dim nOptionIndex As String = oElmt.ParentNode.SelectSingleNode("nItemOptIdx").InnerText
                            cOptionGroupName = ""
                            If Not oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/@name") Is Nothing Then
                                cOptionGroupName = oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/@name").InnerText
                            End If
                            If nOptionIndex >= 0 Then

                                oElmt2 = oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/Option[" & nOptionIndex & "]")
                                If Not oElmt2 Is Nothing Then
                                    If cOptionGroupName <> "" Then oElmt2.SetAttribute("groupName", cOptionGroupName)
                                    oElmt.ParentNode.InnerXml = oElmt2.OuterXml
                                End If

                            Else
                                'case for text option
                                oElmt2 = oElmt.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/Option[1]")
                                If Not oElmt2 Is Nothing Then
                                    If cOptionGroupName <> "" Then oElmt2.SetAttribute("groupName", cOptionGroupName)
                                    If Not oElmt.ParentNode.SelectSingleNode("Name") Is Nothing Then
                                        oElmt2.SetAttribute("name", oElmt.ParentNode.SelectSingleNode("Name").InnerText)
                                    Else
                                        oElmt2.SetAttribute("name", "Name Not defined")
                                    End If
                                    oElmt.ParentNode.InnerXml = oElmt2.OuterXml
                                End If
                            End If

                        Next

                        oElmt = moPageXml.CreateElement("Cart")
                        ' Note: Preserve the original elements in oCartElmt
                        oCartElmt.InnerXml = oCartElmt.InnerXml + oXml.FirstChild.InnerXml


                    End If
                    myWeb.CheckMultiParents(oCartElmt)
                    sSql = "Select cClientNotes from tblCartOrder where nCartOrderKey=" & nCartIdUse
                    Dim oNotes As XmlElement = oCartElmt.OwnerDocument.CreateElement("Notes")
                    Dim notes As String = CStr("" & moDBHelper.ExeProcessSqlScalar(sSql))
                    oNotes.InnerXml = notes

                    total -= moDiscount.CheckDiscounts(oDs, oCartElmt, True, oNotes)

                    oXml = Nothing
                    oDs = Nothing

                    If mbNoDeliveryAddress Then oCartElmt.SetAttribute("hideDeliveryAddress", "True")
                    If mnGiftListId > 0 Then oCartElmt.SetAttribute("giftListId", mnGiftListId)

                    sSql = "Select * from tblCartOrder where nCartOrderKey=" & nCartIdUse

                    oDs = moDBHelper.GetDataSet(sSql, "Order", "Cart")
                    For Each oRow In oDs.Tables("Order").Rows
                        shipCost = CDbl("0" & oRow("nShippingCost"))
                        oCartElmt.SetAttribute("shippingType", oRow("nShippingMethodId") & "")
                        oCartElmt.SetAttribute("shippingCost", shipCost & "")
                        oCartElmt.SetAttribute("shippingDesc", oRow("cShippingDesc") & "")

                        If oRow("nShippingMethodId") = 0 And oRow("nCartStatus") < 4 Then
                            shipCost = -1
                            'Default Shipping Country.
                            Dim cDestinationCountry As String = moCartConfig("DefaultCountry")
                            If cDestinationCountry <> "" Then
                                'Go and collect the valid shipping options available for this order
                                Dim oDsShipOptions As DataSet = getValidShippingOptionsDS(cDestinationCountry, total, quant, weight)
                                Dim oRowSO As DataRow
                                For Each oRowSO In oDsShipOptions.Tables(0).Rows
                                    Dim bCollection As Boolean = False
                                    If Not IsDBNull(oRowSO("bCollection")) Then
                                        bCollection = oRowSO("bCollection")
                                    End If
                                    If (moCartConfig("DefaultShippingMethod") <> Nothing And moCartConfig("DefaultShippingMethod") <> "") Then
                                        'logic to overide below...
                                        If (oCartElmt.HasAttribute("shippingType") And oCartElmt.GetAttribute("shippingType") = "0") Then
                                            If (oRowSO("nShipOptKey") = moCartConfig("DefaultShippingMethod")) Then
                                                shipCost = CDbl("0" & oRowSO("nShipOptCost"))
                                                oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig("DefaultCountry"))
                                                oCartElmt.SetAttribute("shippingType", moCartConfig("DefaultShippingMethod") & "")
                                                oCartElmt.SetAttribute("shippingCost", shipCost & "")
                                                oCartElmt.SetAttribute("shippingDesc", oRowSO("cShipOptName") & "")
                                                oCartElmt.SetAttribute("shippingCarrier", oRowSO("cShipOptCarrier") & "")
                                            End If
                                        End If
                                    ElseIf (shipCost = -1 Or CDbl("0" & oRowSO("nShipOptCost")) < shipCost) And bCollection = False Then
                                        shipCost = CDbl("0" & oRowSO("nShipOptCost"))
                                        oCartElmt.SetAttribute("shippingDefaultDestination", moCartConfig("DefaultCountry"))
                                        oCartElmt.SetAttribute("shippingType", oRowSO("nShipOptKey") & "")
                                        oCartElmt.SetAttribute("shippingCost", shipCost & "")
                                        oCartElmt.SetAttribute("shippingDesc", oRowSO("cShipOptName") & "")
                                        oCartElmt.SetAttribute("shippingCarrier", oRowSO("cShipOptCarrier") & "")
                                    End If


                                Next

                            End If
                            If shipCost = -1 Then shipCost = 0
                        End If

                        If oCartElmt.GetAttribute("shippingType") > 0 Then
                            getShippingDetailXml(oCartElmt, oCartElmt.GetAttribute("shippingType"))
                        End If

                        vatAmt = updateTotals(oCartElmt, total, shipCost, oCartElmt.GetAttribute("shippingType"))


                        ' Check if the cart needs to be adjusted for deposits or settlements
                        If mcDeposit = "On" Then
                            Dim nTotalAmount As Double = total + shipCost + vatAmt
                            Dim nPayable As Double = 0.0#
                            ' First check if an inital deposit has been paid
                            ' We are still in a payment type of deposit if:
                            '   1. No amount has been received
                            '   OR 2. An amount has been received but the cart is still in a status of 10
                            If IsDBNull(oRow("nAmountReceived")) Then
                                ' No deposit has been paid yet - let's set the deposit value, if it has been specified
                                If mcDepositAmount <> "" Then
                                    If Right(mcDepositAmount, 1) = "%" Then
                                        If IsNumeric(Left(mcDepositAmount, Len(mcDepositAmount) - 1)) Then
                                            nPayable = (nTotalAmount) * CDbl(Left(mcDepositAmount, Len(mcDepositAmount) - 1)) / 100
                                        End If

                                    Else
                                        If IsNumeric(mcDepositAmount) Then nPayable = CDbl(mcDepositAmount)
                                    End If
                                    If nPayable > nTotalAmount Then nPayable = nTotalAmount

                                    ' Set the Payable Amount
                                    If nPayable > 0 Then
                                        oCartElmt.SetAttribute("payableAmount", FormatNumber(nPayable, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                                        oCartElmt.SetAttribute("paymentMade", "0")
                                    End If
                                End If
                            Else
                                ' A deposit has been paid - should I check if it's the same as the total amount?
                                If IsNumeric(oRow("nAmountReceived")) Then
                                    nPayable = nTotalAmount - CDbl(oRow("nAmountReceived"))
                                    If nPayable > 0 Then
                                        oCartElmt.SetAttribute("payableAmount", FormatNumber(nPayable, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                                    End If
                                    oCartElmt.SetAttribute("paymentMade", FormatNumber(CDbl(oRow("nLastPaymentMade")), 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))

                                End If
                            End If

                            ' Set the payableType 
                            If IsDBNull(oRow("nAmountReceived")) Or nStatusId = 10 Then
                                oCartElmt.SetAttribute("payableType", "deposit")
                            Else
                                oCartElmt.SetAttribute("payableType", "settlement")
                            End If

                            If nPayable = 0 Then
                                oCartElmt.SetAttribute("ReadOnly", "On")
                            End If
                        End If

                        'Add Any Client Notes
                        If Not (IsDBNull(oRow("cClientNotes")) Or oRow("cClientNotes") & "" = "") Then
                            oElmt = moPageXml.CreateElement("Notes")
                            oElmt.InnerXml = oRow("cClientNotes")
                            If oElmt.FirstChild.Name = "Notes" Then
                                oCartElmt.AppendChild(oElmt.SelectSingleNode("Notes"))
                            Else
                                oCartElmt.AppendChild(oElmt)
                            End If
                        End If

                        'Add the payment details if we have them
                        If oRow("nPayMthdId") > 0 Then
                            sSql = "Select * from tblCartPaymentMethod where nPayMthdKey=" & oRow("nPayMthdId")
                            oDs2 = moDBHelper.GetDataSet(sSql, "Payment", "Cart")
                            oElmt = moPageXml.CreateElement("PaymentDetails")
                            For Each oRow2 In oDs2.Tables("Payment").Rows
                                oElmt.InnerXml = oRow2("cPayMthdDetailXml")
                                oElmt.SetAttribute("provider", oRow2("cPayMthdProviderName"))
                                oElmt.SetAttribute("ref", oRow2("cPayMthdProviderRef"))
                                oElmt.SetAttribute("acct", oRow2("cPayMthdAcctName"))
                            Next
                            oCartElmt.AppendChild(oElmt)
                        End If

                        'Add Delivery Details
                        If nStatusId = 9 Then
                            sSql = "Select * from tblCartOrderDelivery where nOrderId=" & nCartIdUse
                            oDs2 = moDBHelper.GetDataSet(sSql, "Delivery", "Details")
                            For Each oRow2 In oDs2.Tables("Delivery").Rows
                                oElmt = moPageXml.CreateElement("DeliveryDetails")
                                oElmt.SetAttribute("carrierName", oRow2("cCarrierName"))
                                oElmt.SetAttribute("ref", oRow2("cCarrierRef"))
                                oElmt.SetAttribute("notes", oRow2("cCarrierNotes"))
                                oElmt.SetAttribute("deliveryDate", xmlDate(oRow2("dExpectedDeliveryDate")))
                                oElmt.SetAttribute("collectionDate", xmlDate(oRow2("dCollectionDate")))
                                oCartElmt.AppendChild(oElmt)
                            Next
                            oldCartId = nCartIdUse
                        End If
                        'Ensure we persist the invoice date and ref.
                        If nStatusId > 6 And oCartElmt.GetAttribute("InvoiceDate") = "" Then
                            'Persist invoice date and invoice ref
                            Dim tempInstance As New XmlDocument
                            tempInstance.LoadXml(myWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.CartOrder, nCartIdUse))
                            Dim tempOrder As XmlElement = tempInstance.SelectSingleNode("descendant-or-self::Order")
                            If oCartElmt.GetAttribute("InvoiceDate") = "" And tempOrder.GetAttribute("InvoiceDate") <> "" Then
                                oCartElmt.SetAttribute("InvoiceDate", tempOrder.GetAttribute("InvoiceDate"))
                            End If
                            If oCartElmt.GetAttribute("InvoiceRef") = "" And tempOrder.GetAttribute("InvoiceRef") <> "" Then
                                oCartElmt.SetAttribute("InvoiceRef", tempOrder.GetAttribute("InvoiceRef"))
                            End If
                            tempInstance = Nothing
                            tempOrder = Nothing
                        End If

                    Next
                End If

                ' Check for any process reported errors
                If Not IsDBNull(mnProcessError) Then oCartElmt.SetAttribute("errorMsg", mnProcessError)
                'Save the data
                If bCheckSubscriptions Then
                    moSubscription.UpdateSubscriptionsTotals(oCartElmt)
                End If

                mnCartId = oldCartId
                SaveCartXML(oCartElmt)
                'mnCartId = nCartIdUse

                If moCartConfig("RelatedProductsInCart") = "On" Then
                    Dim oRelatedElmt As XmlElement = oCartElmt.OwnerDocument.CreateElement("RelatedItems")
                    For i As Integer = 0 To oItemList.Count - 1
                        myWeb.moDbHelper.addRelatedContent(oRelatedElmt, oItemList(i), False)
                    Next
                    For Each oRelElmt As XmlElement In oRelatedElmt.SelectNodes("Content")
                        If oItemList.ContainsValue(oRelElmt.GetAttribute("id")) Then
                            oRelElmt.ParentNode.RemoveChild(oRelElmt)
                        End If
                    Next
                    If Not oRelatedElmt.InnerXml = "" Then oCartElmt.AppendChild(oRelatedElmt)
                End If

            Catch ex As Exception
                returnException(mcModuleName, "GetCart", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Function updateTotals(ByRef oCartElmt As XmlElement, ByVal total As Double, ByVal shipCost As Double, ByVal ShipMethodId As String) As Double
            Dim cProcessInfo As String = ""
            Dim oElmt As XmlElement
            Dim oElmt2 As XmlElement
            Dim vatAmt As Double = 0
            Dim nLineVat As Double
            Try

                If mnTaxRate > 0 Then
                    'we calculate vat at the end after we have applied discounts etc
                    For Each oElmt In oCartElmt.SelectNodes("/Order/Item")
                        Dim nOpPrices As Long = 0

                        'get the prices of options to calculate vat
                        '  AG - I think this is a mistake: For Each oElmt2 In oCartElmt.SelectNodes("/Item")
                        For Each oElmt2 In oElmt.SelectNodes("Item")
                            nOpPrices += oElmt2.GetAttribute("price")
                        Next
                        Dim nItemDiscount As Double = 0

                        Dim nLineTaxRate As Double = mnTaxRate
                        If mbVatOnLine Then
                            nLineTaxRate = oElmt.GetAttribute("taxRate")
                        End If

                        'NB 14th Jan 2010 This doesn't work
                        ' It generates a figure that matches oElement.GetAttribute("price") so vat is always 0
                        ' even if one of the items is being paid for
                        ' to test, 1 high qualifyer, 2 low (so only 1 free)


                        If Not oElmt.SelectSingleNode("DiscountItem[@nDiscountCat='4']") Is Nothing Then

                            Dim oDiscItem As XmlElement = oElmt.SelectSingleNode("DiscountItem")
                            If Not oElmt.SelectSingleNode("Discount[@nDiscountCat='4' and @bDiscountIsPercent='1' and  number(@nDiscountValue)=100]") Is Nothing Then
                                'checks for a 100% off discount
                                nLineVat = Round((oElmt.GetAttribute("price") - nItemDiscount + nOpPrices) * (nLineTaxRate / 100), , , mbRoundup) * oDiscItem.GetAttribute("Units")
                            Else
                                'nLineVat = Round((oElmt.GetAttribute("price") - nItemDiscount + nOpPrices) * (mnTaxRate / 100), , , mbRoundup) * oDiscItem.GetAttribute("Units")

                                nLineVat = Round((oDiscItem.GetAttribute("Total")) * (nLineTaxRate / 100), , , mbRoundup, mbRoundDown)

                                'nLineVat = 5000
                            End If


                            '
                            'NB 15th Jan: What is this use of this line? It's logic makes one of the multipliers 0 thus destroying VAT?
                            'Replaced with basic unit VAT change, charge per unit but reduce number of units to only those being paid for
                            'nItemDiscount = oDiscItem.GetAttribute("TotalSaving") / (oDiscItem.GetAttribute("Units") - oDiscItem.GetAttribute("oldUnits")) * -1

                        Else
                            'NB: 15-01-2010 Moved into Else so Buy X Get Y Free do not use this, as they can get 0
                            If mbVatAtUnit Then
                                'Round( Price * Vat ) * Quantity
                                nLineVat = Round((oElmt.GetAttribute("price") - nItemDiscount + nOpPrices) * (nLineTaxRate / 100), , , mbRoundup, mbRoundDown) * oElmt.GetAttribute("quantity")
                            Else
                                'Round( ( Price * Quantity )* VAT )
                                nLineVat = Round((((oElmt.GetAttribute("price") - nItemDiscount + nOpPrices)) * oElmt.GetAttribute("quantity")) * (nLineTaxRate / 100), , , mbRoundup, mbRoundDown)
                            End If
                        End If


                        oElmt.SetAttribute("itemTax", nLineVat)
                        vatAmt += nLineVat
                    Next

                    If Not LCase(moCartConfig("DontTaxShipping")) = "on" Then
                        vatAmt = Round((shipCost) * (mnTaxRate / 100), , , mbRoundup, mbRoundDown) + Round(vatAmt, , , mbRoundup, mbRoundDown)
                    End If

                    oCartElmt.SetAttribute("totalNet", FormatNumber(total + shipCost, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                    oCartElmt.SetAttribute("vatRate", mnTaxRate)
                    oCartElmt.SetAttribute("shippingType", ShipMethodId & "")
                    oCartElmt.SetAttribute("shippingCost", FormatNumber(shipCost, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                    oCartElmt.SetAttribute("vatAmt", FormatNumber(vatAmt, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                    oCartElmt.SetAttribute("total", FormatNumber(total + shipCost + vatAmt, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                Else
                    oCartElmt.SetAttribute("totalNet", FormatNumber(total + shipCost, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                    oCartElmt.SetAttribute("vatRate", 0.0#)
                    oCartElmt.SetAttribute("shippingType", ShipMethodId & "")
                    oCartElmt.SetAttribute("shippingCost", FormatNumber(shipCost, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                    oCartElmt.SetAttribute("vatAmt", 0.0#)
                    oCartElmt.SetAttribute("total", FormatNumber(total + shipCost, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                End If

                Return vatAmt

            Catch ex As Exception
                returnException(mcModuleName, "updateTotals", ex, "", cProcessInfo, gbDebug)
            End Try
        End Function

        Sub getShippingDetailXml(ByRef oCartXml As XmlElement, ByVal nShippingId As Long)
            PerfMon.Log("Cart", "getShippingDetailXml")
            Dim cProcessInfo As String = ""
            Dim oDs As DataSet
            Dim sSql As String = "select cShipOptName as Name, cShipOptCarrier as Carrier, cShipOptTime as DeliveryTime from tblCartShippingMethods where nShipOptKey=" & nShippingId
            Dim oXml As XmlDataDocument
            Dim oShippingXml As XmlElement

            Try
                oDs = moDBHelper.GetDataSet(sSql, "Shipping", "Cart")
                oXml = New XmlDataDocument(oDs)
                oDs.EnforceConstraints = False
                oShippingXml = moPageXml.CreateElement("Cart")
                oShippingXml.InnerXml = oXml.InnerXml
                oCartXml.AppendChild(oShippingXml.FirstChild.FirstChild)

            Catch ex As Exception
                returnException(mcModuleName, "getShippingDetailXml", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Function getProductPricesByXml(ByVal cXml As String, ByVal cUnit As String, ByVal nQuantity As Long, Optional ByVal PriceType As String = "Prices") As Double
            PerfMon.Log("Cart", "getProductPricesByXml")
            Dim cGroupXPath As String = ""
            Dim oProd As XmlElement = moPageXml.CreateElement("product")
            Try

                oProd.InnerXml = cXml

                Dim oThePrice As XmlElement = getContentPricesNode(oProd, cUnit, nQuantity, PriceType)

                Dim nPrice As Double = 0.0#

                If IsNumeric(oThePrice.InnerText) Then
                    nPrice = CDbl(oThePrice.InnerText)
                End If

                Return nPrice

            Catch ex As Exception
                returnException(mcModuleName, "getProductPricesByXml", ex, "", "", gbDebug)
            End Try

        End Function

        Function getProductTaxRate(ByVal priceXml As XmlElement) As Double
            PerfMon.Log("Cart", "getProductVatRate")
            Dim cGroupXPath As String = ""
            Dim oProd As XmlNode = moPageXml.CreateNode(XmlNodeType.Document, "", "product")
            Try

                Dim vatCode As String = ""

                If mbVatOnLine Then
                    Select Case priceXml.GetAttribute("taxCode")

                        Case "0"
                            Return 0
                        Case LCase("s")
                            Return mnTaxRate
                        Case Else
                            Return mnTaxRate

                    End Select
                Else
                    Return 0
                End If

            Catch ex As Exception
                returnException(mcModuleName, "getProductTaxRate", ex, "", "", gbDebug)
                Return Nothing
            End Try

        End Function

        Function getContentPricesNode(ByVal oContentXml As XmlElement, ByVal cUnit As String, ByVal nQuantity As Long, Optional ByVal PriceType As String = "Prices") As XmlElement
            PerfMon.Log("Cart", "getContentPricesNode")
            Dim cGroupXPath As String = ""
            Dim oDefaultPrice As XmlNode

            Try

                ' Get the Default Price - note if it does not exist, then this is menaingless.
                oDefaultPrice = oContentXml.SelectSingleNode("Content/" & PriceType & "/Price[@default='true']")
                'here we are checking if the price is the correct currency
                'then if we are logged in we will also check if it belongs to
                'one of the user's groups, just looking for 
                Dim cGroups As String = getGroupsByName()
                If cGroups = "" Then cGroups &= "default,all" Else cGroups &= ",default,all"
                cGroups = " and ( contains(@validGroup,""" & Replace(cGroups, ",", """) or contains(@validGroup,""")
                cGroups &= """) or not(@validGroup) or @validGroup="""")"

                If cUnit <> "" Then
                    cGroups = cGroups & "and (@unit=""" & cUnit & """)"
                End If
                ' cGroups = ""
                'Dim cxpath As String = "Content/Prices/Price[(@currency='" & mcCurrency & "') " & cGroups & " ][1]"


                'Fix for content items that are not Content/Content done for legacy sites such as insure your move 09/06/2015
                Dim xPathStart As String = "Content/"
                If oContentXml.FirstChild.Name <> "Content" Then
                    xPathStart = ""
                End If

                Dim cxpath As String = xPathStart & PriceType & "/Price[(@currency=""" & mcCurrency & """) " & cGroups & " and node()!=""""]" 'need to loop through all just in case we have splits'handled later on though

                Dim oThePrice As XmlElement = oDefaultPrice

                Dim oPNode As XmlElement
                Dim nPrice As Double = 0.0#

                For Each oPNode In oContentXml.SelectNodes(cxpath)
                    'need to deal with "in-product" price splits
                    Dim bHasSplits As Boolean = False
                    Dim bValidSplit As Boolean = False
                    If oPNode.HasAttribute("min") Or oPNode.HasAttribute("max") Then
                        If oPNode.GetAttribute("min") <> "" And oPNode.GetAttribute("max") <> "" Then
                            bHasSplits = True
                            'has a split
                            Dim nThisMin As Integer = IIf(oPNode.GetAttribute("min") = "", 1, oPNode.GetAttribute("min"))
                            Dim nThisMax As Integer = IIf(oPNode.GetAttribute("max") = "", 0, oPNode.GetAttribute("max"))
                            If (nThisMin <= nQuantity) And (nThisMax >= nQuantity Or nThisMax = 0) Then
                                'now we know it is a valid split
                                bValidSplit = True
                            End If
                        End If
                    End If
                    If Not oThePrice Is Nothing Then
                        If IsNumeric(oThePrice.InnerText) Then
                            'this selects the cheapest price for this user assuming not free
                            If IsNumeric(oPNode.InnerText) Then
                                If CDbl(oPNode.InnerText) < CDbl(oThePrice.InnerText) And CLng(oPNode.InnerText) <> 0 Then
                                    oThePrice = oPNode
                                End If
                            End If

                        Else
                            oThePrice = oPNode
                        End If
                    Else
                        oThePrice = oPNode
                    End If
                    'if there are splits and this is a valid split then we want to exit
                    If bHasSplits And bValidSplit Then Exit For
                    'If Not bHasSplits Then Exit For 'we only need the first one if we dont have splits
                Next

                Return oThePrice

            Catch ex As Exception
                returnException(mcModuleName, "getContentPricesNode", ex, "", "", gbDebug)
                Return Nothing
            End Try

        End Function

        Function getOptionPricesByXml(ByVal cXml As String, ByVal nGroupIndex As Integer, ByVal nOptionIndex As Integer) As Double
            PerfMon.Log("Cart", "getOptionPricesByXml")
            Dim cGroupXPath As String = ""
            Dim oProd As XmlNode = moPageXml.CreateNode(XmlNodeType.Document, "", "product")
            Dim oDefaultPrice As XmlNode
            Try
                ' Load product xml
                oProd.InnerXml = cXml
                ' Get the Default Price - note if it does not exist, then this is menaingless.
                oDefaultPrice = oProd.SelectSingleNode("Content/Options/OptGroup[" & nGroupIndex & "]/option[" & nOptionIndex & "]/Prices/Price[@default='true']")
                'here we are checking if the price is the correct currency
                'then if we are logged in we will also check if it belongs to
                'one of the user's groups, just looking for 
                Dim cGroups As String = getGroupsByName()
                If cGroups = "" Then cGroups &= "default,all" Else cGroups &= ",default,all"
                cGroups = " and ( contains(@validGroup,""" & Replace(cGroups, ",", """) or contains(@validGroup,""")
                cGroups &= """) or not(@validGroup) or @validGroup="""")"

                Dim cxpath As String = "Content/Options/OptGroup[" & nGroupIndex & "]/option[" & nOptionIndex & "]/Prices/Price[(@currency=""" & mcCurrency & """) " & cGroups & " ]"

                Dim oThePrice As XmlElement = oDefaultPrice

                Dim oPNode As XmlElement
                Dim nPrice As Double = 0.0#
                For Each oPNode In oProd.SelectNodes(cxpath)
                    'If Not oThePrice Is Nothing Then
                    '    If CDbl(oPNode.InnerText) < CDbl(oThePrice.InnerText) Then
                    '        oThePrice = oPNode
                    '        nPrice = CDbl(oThePrice.InnerText)
                    '    End If
                    'Else
                    '    oThePrice = oPNode
                    '    nPrice = CDbl(oThePrice.InnerText)
                    'End If
                    If Not oThePrice Is Nothing Then
                        If IsNumeric(oThePrice.InnerText) Then
                            If CDbl(oPNode.InnerText) < CDbl(oThePrice.InnerText) Then
                                oThePrice = oPNode
                                nPrice = CDbl(oThePrice.InnerText)
                            End If
                        Else
                            oThePrice = oPNode
                            nPrice = CDbl(oThePrice.InnerText)
                        End If
                    Else
                        oThePrice = oPNode
                        If IsNumeric(oThePrice.InnerText) Then
                            nPrice = CDbl(oThePrice.InnerText)
                        End If
                    End If
                Next

                Return nPrice
            Catch ex As Exception
                returnException(mcModuleName, "getOptionPricesByXml", ex, "", "", gbDebug)
            End Try
        End Function

        Private Sub CheckQuantities(ByRef oCartElmt As XmlElement, ByVal cProdXml As String, ByVal cItemQuantity As String)

            PerfMon.Log("Cart", "CheckQuantities")
            ' Check each product against max and mins

            Try

                Dim oErrorMsg As String = ""
                Dim oError As XmlElement
                Dim oMsg As XmlElement

                Dim oProd As XmlNode = moPageXml.CreateNode(XmlNodeType.Document, "", "product")
                ' Load product xml
                oProd.InnerXml = cProdXml

                ' Set the error node
                oError = oCartElmt.SelectSingleNode("error")

                If IsNumeric(cItemQuantity) Then

                    'Check minimum value
                    If CLng(cItemQuantity) < CLng(getNodeValueByType(oProd, "//Quantities/Minimum", dataType.TypeNumber, 0)) Then
                        ' Minimum has not been matched

                        ' Check for existence of error node
                        If oError Is Nothing Then
                            oError = oCartElmt.OwnerDocument.CreateElement("error")
                            oCartElmt.AppendChild(oError)
                        End If

                        ' Check for existence of msg node for min
                        If oError.SelectSingleNode("msg[@type='quantity_min']") Is Nothing Then
                            oMsg = addElement(oError, "msg", "You have not requested enough of one or more products", True)
                            oMsg.SetAttribute("type", "quantity_min")
                        End If

                        ' Add product specific msg
                        oMsg = addElement(oError, "msg", "<strong>" & getNodeValueByType(oProd, "/Content/Name", dataType.TypeString, "A product below ") & "</strong> requires a quantity equal to or above <em>" & getNodeValueByType(oProd, "//Quantities/Minimum", dataType.TypeNumber, "an undetermined value (please call for assistance).") & "</em>", True)
                        oMsg.SetAttribute("type", "quantity_min_detail")

                    End If

                    'Check maximum value
                    If CLng(cItemQuantity) > CLng(getNodeValueByType(oProd, "//Quantities/Maximum", dataType.TypeNumber, Integer.MaxValue)) Then
                        ' Maximum has not been matched

                        ' Check for existence of error node
                        If oError Is Nothing Then
                            oError = oCartElmt.OwnerDocument.CreateElement("error")
                            oCartElmt.AppendChild(oError)
                        End If

                        ' Check for existence of msg node for min
                        If oError.SelectSingleNode("msg[@type='quantity_max']") Is Nothing Then
                            oMsg = addElement(oError, "msg", "You have requested too much of one or more products")
                            oMsg.SetAttribute("type", "quantity_max")
                        End If

                        ' Add product specific msg
                        oMsg = addElement(oError, "msg", "<strong>" & getNodeValueByType(oProd, "/Content/Name", dataType.TypeString, "A product below ") & "</strong> requires a quantity equal to or below <em>" & getNodeValueByType(oProd, "//Quantities/Maximum", dataType.TypeNumber, "an undetermined value (please call for assistance).") & "</em>", True)
                        oMsg.SetAttribute("type", "quantity_max_detail")

                    End If

                    'Check bulkunit value
                    Dim cBulkUnit As Integer = getNodeValueByType(oProd, "//Quantities/BulkUnit", dataType.TypeNumber, 0)
                    If (CLng(cItemQuantity) Mod CLng(getNodeValueByType(oProd, "//Quantities/BulkUnit", dataType.TypeNumber, 1))) <> 0 Then
                        ' Bulk Unit has not been matched
                        ' Check for existence of error node
                        If oError Is Nothing Then
                            oError = oCartElmt.OwnerDocument.CreateElement("error")
                            oCartElmt.AppendChild(oError)
                        End If

                        ' Check for existence of msg node for min
                        If oError.SelectSingleNode("msg[@type='quantity_mod']") Is Nothing Then
                            oMsg = addElement(oError, "msg", "One or more products below can only be bought in certain quantities.")
                            oMsg.SetAttribute("type", "quantity_mod")
                        End If

                        ' Add product specific msg
                        oMsg = addElement(oError, "msg", "<strong>" & getNodeValueByType(oProd, "/Content/Name", dataType.TypeString, "A product below ") & "</strong> can only be bought in lots of <em>" & getNodeValueByType(oProd, "//Quantities/BulkUnit", dataType.TypeNumber, "an undetermined value (please call for assistance).") & "</em>", True)
                        oMsg.SetAttribute("type", "quantity_mod_detail")
                    End If
                End If


            Catch ex As Exception
                returnException(mcModuleName, "CheckQuantities", ex, "", "", gbDebug)
            End Try

        End Sub


        Private Sub CheckStock(ByRef oCartElmt As XmlElement, ByVal cProdXml As String, ByVal cItemQuantity As String)
            PerfMon.Log("Cart", "CheckStock")
            Dim oProd As XmlNode = moPageXml.CreateNode(XmlNodeType.Document, "", "product")
            Dim oStock As XmlNode
            Dim oError As XmlElement
            Dim oMsg As XmlElement
            Dim cProcessInfo As String = ""
            Try
                ' Load product xml
                oProd.InnerXml = cProdXml

                ' Locate the Stock Node
                oStock = oProd.SelectSingleNode("//Stock")

                ' Ignore empty nodes
                If Not (oStock Is Nothing) Then
                    ' Ignore non-numeric nodes
                    If IsNumeric(oStock.InnerText) Then
                        ' If the requested quantity is greater than the stock level, add a warning to the cart - only check tihs on an active cart.
                        If CLng(cItemQuantity) > CLng(oStock.InnerText) And mnProcessId < 6 Then
                            If oCartElmt.SelectSingleNode("error") Is Nothing Then oCartElmt.AppendChild(oCartElmt.OwnerDocument.CreateElement("error"))
                            oError = oCartElmt.SelectSingleNode("error")
                            oMsg = addElement(oError, "msg", "<span class=""term3080"">You have requested more items than are currently <em>in stock</em> for <strong class=""product-name"">" & oProd.SelectSingleNode("//Name").InnerText & "</strong> (only <span class=""quantity-available"">" & oStock.InnerText & "</span> available).</span><br/>", True)
                            oMsg.SetAttribute("type", "stock")
                        End If
                    End If
                End If
            Catch ex As Exception
                returnException(mcModuleName, "CheckStock", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Sub UpdateStockLevels(ByRef oCartElmt As XmlElement)
            PerfMon.Log("Cart", "UpdateStockLevels")
            Dim sSql As String
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim oProd As XmlNode = moPageXml.CreateNode(XmlNodeType.Document, "", "product")
            Dim oItem As XmlElement
            Dim oStock As XmlNode
            Dim nStockLevel As Integer
            Dim cProcessInfo As String = ""
            Try

                For Each oItem In oCartElmt.SelectNodes("Item")
                    If oItem.GetAttribute("contentId") <> "" Then
                        sSql = "select * from tblContent where nContentKey=" & oItem.GetAttribute("contentId")
                        oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart")

                        For Each oRow In oDs.Tables("Item").Rows

                            oProd.InnerXml = oRow("cContentXmlDetail")
                            oStock = oProd.SelectSingleNode("//Stock")

                            ' Ignore empty nodes
                            If Not (oStock Is Nothing) Then
                                ' Ignore non-numeric nodes
                                If IsNumeric(oStock.InnerText) Then
                                    nStockLevel = CInt(oStock.InnerText) - CInt(oItem.GetAttribute("quantity"))
                                    ' Remember to delete the XmlCache
                                    moDBHelper.DeleteXMLCache()

                                    ' Update stock level
                                    If nStockLevel < 0 Then nStockLevel = 0
                                    oStock.InnerText = nStockLevel
                                    oRow("cContentXmlDetail") = oProd.InnerXml

                                End If
                            End If

                            'For Brief
                            oProd.InnerXml = oRow("cContentXmlBrief")
                            oStock = oProd.SelectSingleNode("//Stock")
                            ' Ignore empty nodes
                            If Not (oStock Is Nothing) Then
                                ' Ignore non-numeric nodes
                                If IsNumeric(oStock.InnerText) Then
                                    oStock.InnerText = nStockLevel
                                    oRow("cContentXmlBrief") = oProd.InnerXml
                                End If
                            End If
                        Next
                        moDBHelper.updateDataset(oDs, "Item")
                    End If
                Next

            Catch ex As Exception
                returnException(mcModuleName, "UpdateStockLevels", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Sub UpdateGiftListLevels()
            PerfMon.Log("Cart", "UpdateGiftListLevels")
            Dim sSql As String

            Dim cProcessInfo As String = ""
            Try
                If mnGiftListId > 0 Then
                    ' The SQL statement JOINS items in the current cart to items in giftlist (by matching ID_OPTION1_OPTION2), and updates the quantity in giftlist accordingly.
                    ' CONVERT is needed to create concat the unique identifier.
                    sSql = "update g " _
                         & "set g.nQuantity=(g.nQuantity - o.nQuantity) " _
                         & "from tblCartItem o inner join tblCartItem g " _
                         & "on (convert(nvarchar,o.nItemId) + '_' + convert(nvarchar,o.cItemOption1) + '_' + convert(nvarchar,o.cItemOption2)) = (convert(nvarchar,g.nItemId) + '_' + convert(nvarchar,g.cItemOption1) + '_' + convert(nvarchar,g.cItemOption2)) " _
                         & "where o.nCartOrderId = " & mnCartId & " and g.nCartOrderId = " & mnGiftListId
                    moDBHelper.ExeProcessSql(sSql)
                End If

            Catch ex As Exception
                returnException(mcModuleName, "UpdateGiftListLevels", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub
        'deprecated for the single call function above.
        Public Sub UpdateGiftListLevels_Old()
            PerfMon.Log("Cart", "UpdateGiftListLevels_Old")
            Dim sSql As String
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim oDr2 As SqlDataReader
            Dim nNewQty As Decimal

            Dim cProcessInfo As String = ""
            Try

                If mnGiftListId > 0 Then

                    sSql = "select * from tblCartItem where nCartOrderId=" & mnCartId
                    oDs = moDBHelper.GetDataSet(sSql, "tblCartItem")

                    For Each oRow In oDs.Tables("tblCartItem").Rows

                        nNewQty = oRow.Item("nQuantity")

                        sSql = "select * from tblCartItem where nCartOrderId=" & mnGiftListId & " and nItemId =" & oRow("nItemId").ToString() & " and cItemOption1='" & SqlFmt(oRow("cItemOption1").ToString()) & "' and cItemOption2='" & SqlFmt(oRow("cItemOption2").ToString()) & "'"
                        oDr2 = moDBHelper.getDataReader(sSql)
                        While (oDr2.Read())
                            nNewQty = oDr2.Item("nQuantity") - oRow.Item("nQuantity")
                        End While

                        sSql = "Update tblCartItem set nQuantity = " & nNewQty.ToString() & " where nCartOrderId=" & mnGiftListId & " and nItemId =" & oRow("nItemId").ToString() & " and cItemOption1='" & SqlFmt(oRow("cItemOption1").ToString()) & "' and cItemOption2='" & SqlFmt(oRow("cItemOption2").ToString()) & "'"
                        moDBHelper.ExeProcessSql(sSql)
                        oDr2.Close()


                    Next

                End If

            Catch ex As Exception
                returnException(mcModuleName, "UpdateGiftListLevels", ex, "", cProcessInfo, gbDebug)
            Finally
                oDr2 = Nothing
                oDs = Nothing
            End Try
        End Sub

        Public Function getGroupsByName() As String
            PerfMon.Log("Cart", "getGroupsByName")
            '!!!!!!!!!!!!!! Should this be in Membership Object???

            Dim cReturn As String
            Dim oDs As DataSet
            Dim oDr As DataRow
            Dim cProcessInfo As String = ""
            Try

                ' Get groups from user ID return A comma separated string?
                oDs = moDBHelper.GetDataSet("select * from tblDirectory g inner join tblDirectoryRelation r on g.nDirKey = r.nDirParentId where r.nDirChildId = " & mnEwUserId, "Groups")
                cReturn = ""

                If oDs.Tables("Groups").Rows.Count > 0 Then
                    For Each oDr In oDs.Tables("Groups").Rows
                        cReturn = cReturn & "," & oDr.Item("cDirName")
                    Next
                    cReturn = Mid(cReturn, 2)
                End If

                Return cReturn

            Catch ex As Exception
                returnException(mcModuleName, "getGroupsByName", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try
        End Function

        Public Overridable Sub addressSubProcess(ByRef oCartElmt As XmlElement, ByVal cAddressType As String)

            Dim oContactXform As xForm
            Dim submitPrefix As String = "cartBill"
            Dim cProcessInfo As String = submitPrefix
            Dim oPay As PaymentProviders
            Dim buttonRef As String = ""
            Dim bSubmitPaymentMethod As Boolean = False

            Try

                If cAddressType.Contains("Delivery") Then submitPrefix = "cartDel"
                If mbEwMembership = True And myWeb.mnUserId <> 0 And submitPrefix <> "cartDel" Then
                    'we now only need this on delivery.
                    oContactXform = pickContactXform(cAddressType, submitPrefix, , mcCartCmd)
                    GetCart(oCartElmt)
                Else
                    oContactXform = contactXform(cAddressType, , , mcCartCmd)

                    If moPay Is Nothing Then
                        oPay = New PaymentProviders(myWeb)
                    Else
                        oPay = moPay
                    End If
                    oPay.mcCurrency = mcCurrency

                    GetCart(oCartElmt)
                    If LCase(moCartConfig("PaymentTypeButtons")) = "on" Then
                        If Not oCartElmt.SelectSingleNode("Shipping") Is Nothing And moCartConfig("TermsContentId") = "" And moCartConfig("TermsAndConditions") = "" Then
                            ' we already have shipping selected threfore we can skip Options Xform
                            Dim oSubmitBtn As XmlElement = oContactXform.moXformElmt.SelectSingleNode("descendant-or-self::submit[@submission='SubmitAdd']")
                            buttonRef = oSubmitBtn.GetAttribute("ref")
                            oPay.getPaymentMethodButtons(oContactXform, oSubmitBtn.ParentNode, 0)
                            bSubmitPaymentMethod = True
                        End If
                    End If
                End If

                    If oContactXform.valid = False Then
                    'show the form
                    Dim oContentElmt As XmlElement = moPageXml.SelectSingleNode("/Page/Contents")
                    If oContentElmt Is Nothing Then
                        oContentElmt = moPageXml.CreateElement("Contents")
                        If moPageXml.DocumentElement Is Nothing Then
                            Err.Raise(1004, "addressSubProcess", " PAGE IS NOT CREATED")
                        Else
                            moPageXml.DocumentElement.AppendChild(oContentElmt)
                        End If
                    End If
                    oContentElmt.AppendChild(oContactXform.moXformElmt)
                Else
                    If Not cAddressType.Contains("Delivery") Then

                        ' Valid Form, let's adjust the Vat rate
                        ' AJG By default the tax rate is picked up from the billing address, unless otherwise specified.
                        ' 
                        If moCartConfig("TaxFromDeliveryAddress") <> "on" Or (myWeb.moRequest("cIsDelivery") = "True" And moCartConfig("TaxFromDeliveryAddress") = "on") Then
                            If Not oContactXform.Instance.SelectSingleNode("tblCartContact[@type='Billing Address']") Is Nothing Then
                                UpdateTaxRate(oCartElmt.SelectSingleNode("Contact[@type='Billing Address']/Country").InnerText)
                            End If
                        End If

                        'to allow for single form with multiple addresses.
                        If moCartConfig("TaxFromDeliveryAddress") = "on" And Not (oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']") Is Nothing) Then
                            UpdateTaxRate(oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText)
                        End If

                        ' Skip Delivery if:
                        '  - Deliver to this address is selected
                        '  - mbNoDeliveryAddress is True
                        '  - the order is part of a giftlist (the delivery address is pre-determined)
                        ' - we have submitted the delivery address allready
                        If myWeb.moRequest("cIsDelivery") = "True" _
                            Or mbNoDeliveryAddress _
                            Or mnGiftListId > 0 _
                            Or Not (oContactXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='Delivery Address']") Is Nothing) _
                            Or oContactXform.moXformElmt.GetAttribute("cartCmd") = "ChoosePaymentShippingOption" _
                            Then

                            If bSubmitPaymentMethod Then
                                ' we have payment method buttons on the form.
                                mcPaymentMethod = myWeb.moRequest(buttonRef)
                            End If

                            mcCartCmd = "ChoosePaymentShippingOption"
                            mnProcessId = 3

                        Else
                                'If mbEwMembership = True And myWeb.mnUserId <> 0 Then
                                '    'all handled in pick form
                                'Else
                                Dim BillingAddressID As Long = setCurrentBillingAddress(myWeb.mnUserId, 0)
                            If myWeb.moRequest(submitPrefix & "editAddress" & BillingAddressID) <> "" Then
                                'we are editing an address form the pick address form so lets go back.
                                mcCartCmd = "Billing"
                                mnProcessId = 2
                            Else
                                mcCartCmd = "Delivery"
                            End If



                            'End If
                            '   billing address is saved, so up cart status if needed
                            'If mnProcessId < 2 Then mnProcessId = 2
                            'If mnProcessId > 2 Then
                            '    mcCartCmd = "ChoosePaymentShippingOption"
                            '    mnProcessId = 3
                            'End If

                        End If
                        If myWeb.mnUserId > 0 Then
                            setCurrentBillingAddress(myWeb.mnUserId, 0)
                        End If


                    Else 'Case for Delivery

                        ' AJG If specified, the tax rate can be picked up from the delivery address
                        If moCartConfig("TaxFromDeliveryAddress") = "on" Then
                            UpdateTaxRate(oCartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText)
                        End If

                        'TS if we don't have a billing address we need one
                        Dim oDs2 As DataSet
                        oDs2 = moDBHelper.GetDataSet("select * from tblCartContact where nContactCartId = " & CStr(Me.mnCartId) & " and cContactType = 'Billing Address'", "tblCartContact")
                        If oDs2.Tables("tblCartContact").Rows.Count > 0 Then
                            mcCartCmd = "ChoosePaymentShippingOption"
                            mnProcessId = 3
                        Else

                            If myWeb.mnUserId > 0 Then
                                Dim BillingAddressID As Long = setCurrentBillingAddress(myWeb.mnUserId, 0)

                                If BillingAddressID > 0 Then
                                    'set the billing address
                                    Dim sSql As String = "Select nContactKey from tblCartContact where cContactType = 'Delivery Address' and nContactCartid=" & mnCartId
                                    Dim DeliveryAddressID As String = moDBHelper.ExeProcessSqlScalar(sSql)
                                    useSavedAddressesOnCart(BillingAddressID, DeliveryAddressID)

                                    mcPaymentMethod = myWeb.moRequest(buttonRef)

                                    mcCartCmd = "ChoosePaymentShippingOption"
                                    mnProcessId = 3
                                Else
                                    mcCartCmd = "Billing"
                                    mnProcessId = 2
                                End If
                            Else
                                mcCartCmd = "Billing"
                                mnProcessId = 2
                            End If


                        End If

                    End If

                    'save address against the user
                    If mbEwMembership = True And myWeb.mnUserId > 0 And oContactXform.valid Then

                        If Not oContactXform.moXformElmt.GetAttribute("persistAddress") = "false" Then
                            cProcessInfo = "UpdateExistingUserAddress for : " & myWeb.mnUserId
                            UpdateExistingUserAddress(oContactXform)
                        End If

                    End If

                End If
                oContactXform = Nothing

            Catch ex As Exception
                returnException(mcModuleName, "addressSubProcess", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Overridable Function usePreviousAddress(ByRef oCartElmt As XmlElement) As Boolean
            Dim cProcessInfo As String
            Dim sSql As String
            Dim billingAddId As Long = 0
            Dim deliveryAddId As Long = 0
            Dim oDs As DataSet
            Dim odr As DataRow
            Try
                If mbEwMembership = True And myWeb.mnUserId <> 0 Then
                    If LCase(moCartConfig("UsePreviousAddress")) = "on" Then

                        sSql = "select nContactKey, cContactType, nAuditKey from tblCartContact inner join tblAudit a on nAuditId = a.nAuditKey where nContactCartId = 0 and nContactDirId =" & CStr(myWeb.mnUserId)
                        oDs = moDBHelper.GetDataSet(sSql, "tblCartContact")

                        For Each odr In oDs.Tables("tblCartContact").Rows
                            If odr("cContactType") = "Billing Address" Then
                                billingAddId = odr("nContactKey")
                            End If
                            If LCase(moCartConfig("NoDeliveryAddress")) = "on" Then
                                deliveryAddId = billingAddId
                            Else
                                If odr("cContactType") = "Delivery Address" Then
                                    deliveryAddId = odr("nContactKey")
                                End If
                            End If
                        Next
                        If deliveryAddId <> 0 And billingAddId <> 0 Then
                            useSavedAddressesOnCart(billingAddId, deliveryAddId)
                            'skip
                            mcCartCmd = "ChoosePaymentShippingOption"
                            Return True
                        Else
                            'we don't have the addresses we need so we need to go to address step anyhow
                            Return False
                        End If

                    Else
                        'Use previous address functionality turned off
                        Return False
                    End If
                Else
                    'User not logged on or membership is off
                    Return False
                End If



            Catch ex As Exception
                returnException(mcModuleName, "addressSubProcess", ex, "", cProcessInfo, gbDebug)
                Return False
            End Try

        End Function


        Public Overridable Function contactXform(ByVal cAddressType As String, Optional ByVal cSubmitName As String = "submit", Optional ByVal cCmdType As String = "cartCmd", Optional ByVal cCmdAction As String = "") As xForm

            Return contactXform(cAddressType, cSubmitName, cCmdType, cCmdAction, False)

        End Function


        Public Overridable Function contactXform(ByVal cAddressType As String, ByVal cSubmitName As String, ByVal cCmdType As String, ByVal cCmdAction As String, ByVal bDontPopulate As Boolean, Optional ContactId As Long = 0, Optional cmd2 As String = "") As xForm
            PerfMon.Log("Cart", "contactXform")
            Dim oXform As xForm = New xForm
            Dim oGrpElmt As XmlElement
            Dim oDs As DataSet
            Dim oDr As DataRow
            Dim oElmt As XmlElement
            Dim cWhere As String
            Dim cProcessInfo As String = ""
            Dim cXformLocation As String = ""
            Dim bIsBespokeXform As Boolean = False
            Dim bGetInstance As Boolean
            Dim sSql As String

            Try
                ' Build the xform
                oXform.moPageXML = moPageXml
                oXform.NewFrm(cAddressType)

                ' Check for bespoke xform
                Select Case cAddressType
                    Case "Billing Address" : cXformLocation = mcBillingAddressXform
                    Case "Delivery Address" : cXformLocation = mcDeliveryAddressXform
                End Select

                ' Test that a bespoke form exists and is a valid filename
                bIsBespokeXform = System.IO.File.Exists(myWeb.goServer.MapPath(cXformLocation)) And LCase(Right(cXformLocation, 4)) = ".xml"

                ' :::::::::::::::::::::::::::::::::::::::::::::::
                ' :::: CONTACT XFORM :: GROUP and BIND BUILD ::::
                ' :::::::::::::::::::::::::::::::::::::::::::::::

                If bIsBespokeXform Then
                    ' Load the bespoke form
                    oXform.load(cXformLocation)

                    'select the first group element for adding delivery checkbox
                    oGrpElmt = oXform.moXformElmt.SelectSingleNode("group[1]")
                    If ContactId > 0 Then
                        bGetInstance = True
                    End If
                Else
                    'Build the xform because file not specified
                    bGetInstance = True

                    oXform.addGroup(oXform.moXformElmt, "address", , cAddressType)
                    oGrpElmt = oXform.moXformElmt.LastChild

                    oXform.addInput(oGrpElmt, cCmdType, True, cCmdType, "hidden")
                    oXform.addBind(cCmdType, cCmdType)

                    oXform.addInput(oGrpElmt, "cContactType", True, "Type", "hidden")
                    oXform.addBind("cContactType", "tblCartContact/cContactType")

                    oXform.addInput(oGrpElmt, "cContactName", True, "Name", "textbox required")
                    oXform.addBind("cContactName", "tblCartContact/cContactName", "true()", "string")

                    oXform.addInput(oGrpElmt, "cContactCompany", True, "Company", "textbox")
                    oXform.addBind("cContactCompany", "tblCartContact/cContactCompany", "false()")

                    oXform.addInput(oGrpElmt, "cContactAddress", True, "Address", "textbox required")
                    oXform.addBind("cContactAddress", "tblCartContact/cContactAddress", "true()", "string")

                    oXform.addInput(oGrpElmt, "cContactCity", True, "City", "textbox required")
                    oXform.addBind("cContactCity", "tblCartContact/cContactCity", "true()")

                    oXform.addInput(oGrpElmt, "cContactState", True, "County/State", "textbox")
                    oXform.addBind("cContactState", "tblCartContact/cContactState")

                    oXform.addInput(oGrpElmt, "cContactZip", True, "Postcode/Zip", "textbox required")
                    oXform.addBind("cContactZip", "tblCartContact/cContactZip", "true()", "string")

                    oXform.addSelect1(oGrpElmt, "cContactCountry", True, "Country", "dropdown required")
                    oXform.addBind("cContactCountry", "tblCartContact/cContactCountry", "true()", "string")

                    oXform.addInput(oGrpElmt, "cContactTel", True, "Tel", "textbox")
                    oXform.addBind("cContactTel", "tblCartContact/cContactTel")

                    oXform.addInput(oGrpElmt, "cContactFax", True, "Fax", "textbox")
                    oXform.addBind("cContactFax", "tblCartContact/cContactFax")

                    ' Only show email address for Billing
                    If cAddressType = "Billing Address" Or mnGiftListId > 0 Then
                        oXform.addInput(oGrpElmt, "cContactEmail", True, "Email", "textbox required")
                        oXform.addBind("cContactEmail", "tblCartContact/cContactEmail", "true()", "email")
                    End If
                    If myWeb.moConfig("cssFramework") = "bs3" Then
                        oXform.addSubmit(oGrpElmt, "Submit" & Replace(cAddressType, " ", ""), "Submit")
                    Else
                        oXform.addSubmit(oGrpElmt, "Submit" & Replace(cAddressType, " ", ""), "Submit", cSubmitName & Replace(cAddressType, " ", ""))
                    End If
                    oXform.submission("Submit" & Replace(cAddressType, " ", ""), IIf(cCmdAction = "", "", "?" & cCmdType & "=" & cCmdAction), "POST", "return form_check(this);")
                End If

                ' Add the countries list to the form
                For Each oElmt In oXform.moXformElmt.SelectNodes("descendant-or-self::select1[contains(@class, 'country') or @bind='cContactCountry']")
                    Dim cThisAddressType As String
                    If oElmt.ParentNode.SelectSingleNode("input[@bind='cContactType' or @bind='cDelContactType']/value") Is Nothing Then
                        cThisAddressType = cAddressType
                    Else
                        cThisAddressType = oElmt.ParentNode.SelectSingleNode("input[@bind='cContactType' or @bind='cDelContactType']/value").InnerText
                    End If
                    If LCase(moCartConfig("NoDeliveryAddress")) = "on" Then
                        cThisAddressType = "Delivery Address"
                    End If

                    populateCountriesDropDown(oXform, oElmt, cThisAddressType)
                Next

                ' Add the Delivery Checkbox if needed

                If cAddressType = "Billing Address" And mnProcessId < 2 And (Not (mbNoDeliveryAddress)) And Not (mnGiftListId > 0) Then
                    ' this can be optionally turned off in the xform, by the absence of cIsDelivery in the instance.
                    If oXform.Instance.SelectSingleNode("tblCartContact[@type='Delivery Address']") Is Nothing And oXform.Instance.SelectSingleNode("//bDisallowDeliveryCheckbox") Is Nothing Then
                        '   shoppers have the option to send to same address as the billing with a checkbox
                        oXform.addSelect(oGrpElmt, "cIsDelivery", True, "Deliver to This Address", "checkbox", xForm.ApperanceTypes.Minimal)
                        oXform.addOption((oGrpElmt.LastChild), "", "True")
                        oXform.addBind("cIsDelivery", "tblCartContact/cIsDelivery")
                    End If
                End If

                If moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection") Then
                    'Add Collection options
                    Dim oIsDeliverySelect As XmlElement = oXform.moXformElmt.SelectSingleNode("descendant-or-self::select[@bind='cIsDelivery']")
                    If Not oIsDeliverySelect Is Nothing Then

                        'Create duplicate select as select1
                        Dim newElmt As XmlElement = moPageXml.CreateElement("select1")
                        Dim oAtt As XmlAttribute
                        For Each oAtt In oIsDeliverySelect.Attributes
                            newElmt.SetAttribute(oAtt.Name, oAtt.Value)
                        Next
                        Dim oNode As XmlNode
                        For Each oNode In oIsDeliverySelect.ChildNodes
                            newElmt.AppendChild(oNode.CloneNode(True))
                        Next

                        Dim delBillingElmt As XmlElement = oXform.addOption(newElmt, "Deliver to Billing Address", "false")

                        Dim bCollection As Boolean = False
                        'Get the collection delivery options
                        Dim oDrCollectionOptions As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where bCollection = 1")
                        Do While oDrCollectionOptions.Read()
                            Dim OptLabel As String = "<span class=""opt-name"">" & oDrCollectionOptions.Item("cShipOptName").ToString() & "</span>"
                            OptLabel = OptLabel & "<span class=""opt-carrier"">" & oDrCollectionOptions.Item("cShipOptCarrier").ToString() & "</span>"
                            oXform.addOption(newElmt, OptLabel, oDrCollectionOptions.Item("nShipOptKey").ToString(), True)
                            bCollection = True
                        Loop
                        'Only change this if collection shipping options exist.
                        If bCollection Then
                            oIsDeliverySelect.ParentNode.ReplaceChild(newElmt, oIsDeliverySelect)
                        Else
                            'this was all for nuffin
                            newElmt = Nothing
                        End If

                    End If
                End If

                If cmd2 <> "" Then
                    oXform.addInput(oGrpElmt, cmd2, True, cmd2, "hidden")
                    oXform.addBind(cmd2, "cmd2")
                End If

                ' :::::::::::::::::::::::::::::::::::::::::::::::
                ' :::: CONTACT XFORM :: CREATE/LOAD INSTANCE ::::
                ' :::::::::::::::::::::::::::::::::::::::::::::::

                ' When there is no match this will get the default instance based on the table schema.  
                ' This will override any form that we have loaded in, so we need to put exceptions in.
                If bGetInstance Then
                    oXform.Instance.InnerXml = moDBHelper.getObjectInstance(dbHelper.objectTypes.CartContact, ContactId)
                    If ContactId > 0 Then
                        bDontPopulate = True
                    End If
                End If

                'if the instance is empty fill these values
                Dim bAddIds As Boolean = False

                'catch for sites where nContactKey is not specified.
                If oXform.Instance.SelectSingleNode("*/nContactKey") Is Nothing Then
                    bAddIds = True
                Else
                    If oXform.Instance.SelectSingleNode("*/nContactKey").InnerText = "" Then
                        bAddIds = True
                    End If
                End If

                If bAddIds Then
                    For Each oElmt In oXform.Instance.SelectNodes("*/nContactDirId")
                        oElmt.InnerText = myWeb.mnUserId
                    Next
                    For Each oElmt In oXform.Instance.SelectNodes("*/nContactCartId")
                        oElmt.InnerText = mnCartId
                    Next
                    For Each oElmt In oXform.Instance.SelectNodes("*/cContactType")
                        If oElmt.InnerText = "" Then oElmt.InnerText = cAddressType
                    Next
                End If

                'make sure we don't show a random address.
                If mnCartId = 0 Then bDontPopulate = True

                If bDontPopulate = False Then
                    'if we have addresses in the cart insert them
                    sSql = "select nContactKey, cContactType from tblCartContact where nContactCartId = " & CStr(mnCartId)
                    oDs = moDBHelper.GetDataSet(sSql, "tblCartContact")
                    For Each oDr In oDs.Tables("tblCartContact").Rows
                        Dim tempInstance As XmlElement = moPageXml.CreateElement("TempInstance")
                        tempInstance.InnerXml = moDBHelper.getObjectInstance(dbHelper.objectTypes.CartContact, oDr("nContactKey"))
                        Dim instanceAdd As XmlElement = oXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='" & oDr("cContactType") & "']")
                        If Not instanceAdd Is Nothing Then
                            instanceAdd.ParentNode.ReplaceChild(tempInstance.FirstChild, instanceAdd)
                            instanceAdd = oXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='" & oDr("cContactType") & "']")
                            If Not instanceAdd Is Nothing Then
                                instanceAdd.SetAttribute("type", oDr("cContactType"))
                            End If
                        End If
                    Next
                    oDs = Nothing
                    'set the isDelivery Value
                    'remember the delivery address setting.
                    Dim delivElmt As XmlElement = oXform.Instance.SelectSingleNode("tblCartContact[cContactType/node()='Delivery Address']")
                    If Not delivElmt Is Nothing Then
                        If myWeb.moSession("isDelivery") = "" Then
                            delivElmt.SetAttribute("isDelivery", "false")
                        Else
                            delivElmt.SetAttribute("isDelivery", myWeb.moSession("isDelivery"))
                        End If
                    End If

                End If

                'Dim bGetInstance As Boolean = True
                'If bIsBespokeXform Then
                '    ' There is a bespoke form, let's check for the presence of an item in the contact table
                '    Dim oCartContactInDB As Object = moDBHelper.GetDataValue("SELECT COUNT(*) As FoundCount FROM tblCartContact " & ssql)
                '    If Not (oCartContactInDB > 0) Then bGetInstance = False ' No Item so do not get the instance
                'End If



                ' If membership is on and the user is logged on, we need to check if this is an existing address in the user's list of addresses
                If mbEwMembership = True And myWeb.mnUserId > 0 Then

                    ' If we are using the Pick Address list to EDIT an address, there will be a hidden control of userAddId
                    If myWeb.moRequest("userAddId") <> "" Then
                        oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressId"))
                        oXform.Instance.LastChild.InnerText = myWeb.moRequest("userAddId")
                        oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressType"))
                        oXform.Instance.LastChild.InnerText = myWeb.moRequest("userAddType")
                    Else
                        ' Holy large where statement, Batman!  But how on earth else do we tell is a cart address is the same as a user address?
                        Dim value As String
                        Dim contact As XmlElement = oXform.Instance.SelectSingleNode("tblCartContact")
                        cWhere = ""
                        If NodeState(contact, "cContactName", , , , , , value) <> NotInstantiated Then cWhere &= "  and cContactName='" & SqlFmt(value) & "' "
                        If NodeState(contact, "cContactCompany", , , , , , value) <> NotInstantiated Then cWhere &= "  and cContactCompany='" & SqlFmt(value) & "' "
                        If NodeState(contact, "cContactAddress", , , , , , value) <> NotInstantiated Then cWhere &= "  and cContactAddress='" & SqlFmt(value) & "' "
                        If NodeState(contact, "cContactCity", , , , , , value) <> NotInstantiated Then cWhere &= "  and cContactCity='" & SqlFmt(value) & "' "
                        If NodeState(contact, "cContactState", , , , , , value) <> NotInstantiated Then cWhere &= "  and cContactState='" & SqlFmt(value) & "' "
                        If NodeState(contact, "cContactZip", , , , , , value) <> NotInstantiated Then cWhere &= "  and cContactZip='" & SqlFmt(value) & "' "
                        If NodeState(contact, "cContactCountry", , , , , , value) <> NotInstantiated Then cWhere &= "  and cContactCountry='" & SqlFmt(value) & "' "

                        'cWhere = " and cContactName='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactName").InnerText) & "' " & _
                        '         "and cContactCompany='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactCompany").InnerText) & "' " & _
                        '         "and cContactAddress='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactAddress").InnerText) & "' " & _
                        '         "and cContactCity='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactCity").InnerText) & "' " & _
                        '         "and cContactState='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactState").InnerText) & "' " & _
                        '         "and cContactZip='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactZip").InnerText) & "' " & _
                        '         "and cContactCountry='" & SqlFmt(oXform.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText) & "' "
                        oDs = moDBHelper.GetDataSet("select * from tblCartContact where nContactDirId = " & CStr(myWeb.mnUserId) & cWhere, "tblCartContact")
                        If oDs.Tables("tblCartContact").Rows.Count > 0 Then
                            oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressId"))
                            oXform.Instance.LastChild.InnerText = oDs.Tables("tblCartContact").Rows(0).Item("nContactKey")
                            oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("nUserAddressType"))
                            oXform.Instance.LastChild.InnerText = oDs.Tables("tblCartContact").Rows(0).Item("cContactType")
                        End If
                    End If
                End If


                'add some proceedual fields to instance
                oElmt = oXform.moPageXML.CreateElement(cCmdType)
                oXform.Instance.AppendChild(oElmt)

                oElmt = oXform.moPageXML.CreateElement("cIsDelivery")
                oXform.Instance.AppendChild(oElmt)

                If cmd2 <> "" Then
                    oElmt = oXform.moPageXML.CreateElement("cmd2")
                    oElmt.InnerText = "True"
                    oXform.Instance.AppendChild(oElmt)
                End If


                ' If oXform.isSubmitted And cAddressType = myWeb.moRequest.Form("cContactType") Then

                If oXform.isSubmitted Then
                    oXform.updateInstanceFromRequest()
                    oXform.validate()

                    myWeb.moSession("isDelivery") = myWeb.moRequest("isDelivery")

                    'Catch for space as country
                    If Not myWeb.moRequest("cContactCountry") = Nothing Then
                        If myWeb.moRequest("cContactCountry").Trim = "" Then
                            oXform.valid = False
                            oXform.addNote("cContactCountry", xForm.noteTypes.Alert, "Please select a country", True)
                        End If
                    End If

                    If oXform.valid Then
                        'the form is valid so update it - add a check for timed out session (mnCartId = 0)
                        If ContactId > 0 Then
                            'ID is specified so we simply update ignore relation to the cart
                            moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oXform.Instance, ContactId)
                        Else
                            If mnCartId > 0 Then
                                'test if we have a address of this type against the order..!
                                'ssql = "Select nContactKey from tblCartContact where cContactType = '" & cAddressType & "' and nContactCartid=" & mnCartId
                                'Dim sContactKey1 As String = moDBHelper.ExeProcessSqlScalar(ssql)
                                'moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oXform.Instance, sContactKey1)

                                'Step through for multiple addresses
                                Dim bSavedDelivery As Boolean = False

                                'check for collection options
                                If IsNumeric(myWeb.moRequest("cIsDelivery")) Then
                                    'Save the delivery method allready
                                    Dim cSqlUpdate As String = ""
                                    Dim oDrCollectionOptions2 As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where nShipOptKey = " & myWeb.moRequest("cIsDelivery"))
                                    Do While oDrCollectionOptions2.Read()
                                        Dim cShippingDesc As String = oDrCollectionOptions2.Item("cShipOptName").ToString() & "-" &
                                         oDrCollectionOptions2.Item("cShipOptCarrier").ToString() & "</span>"
                                        Dim nShippingCost As Double = CDbl("0" & oDrCollectionOptions2.Item("nShipOptCost"))

                                        cSqlUpdate = "UPDATE tblCartOrder SET cShippingDesc='" & SqlFmt(cShippingDesc) & "', nShippingCost=" & SqlFmt(nShippingCost) & ", nShippingMethodId = " & myWeb.moRequest("cIsDelivery") & " WHERE nCartOrderKey=" & mnCartId
                                    Loop

                                    moDBHelper.ExeProcessSql(cSqlUpdate)
                                    bSavedDelivery = True
                                Else
                                    'If it exists and we are here means we may have changed the Delivery address
                                    'country

                                    'TS commented out for ITB as deliery option has been set earlier we don't want to remove unless invalid for target address.

                                    ' RemoveDeliveryOption(mnCartId)
                                End If


                                For Each oElmt In oXform.Instance.SelectNodes("tblCartContact")
                                    Dim cThisAddressType As String = oElmt.SelectSingleNode("cContactType").InnerText
                                    sSql = "Select nContactKey from tblCartContact where cContactType = '" & cThisAddressType & "' and nContactCartid=" & mnCartId
                                    Dim sContactKey1 As String = moDBHelper.ExeProcessSqlScalar(sSql)
                                    Dim saveInstance As XmlElement = moPageXml.CreateElement("instance")
                                    saveInstance.AppendChild(oElmt.Clone)
                                    moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, saveInstance, sContactKey1)
                                    If cThisAddressType = "Delivery Address" Then bSavedDelivery = True
                                Next

                                'if the option save Delivery is true then
                                If bSavedDelivery = False Then
                                    If myWeb.moRequest("cIsDelivery") = "True" Or (mbNoDeliveryAddress And cAddressType = "Billing Address") Then
                                        If myWeb.moRequest("cIsDelivery") = "True" And mnShippingRootId > 0 Then
                                            'mnShippingRootId
                                            'check if the submitted country matches one in the delivery list
                                            Dim oCheckElmt As XmlElement = moPageXml.CreateElement("ValidCountries")
                                            ListShippingLocations(oCheckElmt)
                                            Dim cCountry As String = oXform.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText
                                            If oCheckElmt.SelectSingleNode("descendant-or-self::TreeItem[@Name='" & cCountry & "' or @name='" & cCountry & "' or @nameShort='" & cCountry & "']") Is Nothing Then
                                                oXform.valid = False
                                                oXform.addNote("cContactCountry", xForm.noteTypes.Alert, "Cannot Deliver to this country. please select another.", True)
                                            End If
                                        End If
                                        If oXform.valid Then
                                            sSql = "Select nContactKey from tblCartContact where cContactType = 'Delivery Address' and nContactCartid=" & mnCartId
                                            Dim sContactKey2 As String = moDBHelper.ExeProcessSqlScalar(sSql)
                                            oXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText = "Delivery Address"
                                            moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oXform.Instance, sContactKey2)
                                            'going to set it back to a billing address
                                            oXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText = "Billing Address"
                                        End If
                                    End If
                                End If

                                If Not oXform.Instance.SelectSingleNode("tblCartContact/cContactEmail[@optOut='true']") Is Nothing Then
                                    moDBHelper.AddInvalidEmail(oXform.Instance.SelectSingleNode("tblCartContact/cContactEmail[@optOut='true']").InnerText)
                                End If

                            Else
                                ' Throw an error to indicate that the user has timed out
                                mnProcessError = 4
                            End If
                        End If


                        If myWeb.moRequest("cContactOpt-In") <> "" Then
                            AddToLists("Newsletter", moCartXml, myWeb.moRequest("cContactName"), myWeb.moRequest("cContactEmail"))
                        End If

                    End If

                End If



                oXform.addValues()

                Return oXform

            Catch ex As Exception
                returnException(mcModuleName, "contactXform", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try

        End Function




        Public Overridable Function pickContactXform(ByVal cAddressType As String, Optional ByVal submitPrefix As String = "", Optional ByVal cCmdType As String = "cartCmd", Optional ByVal cCmdAction As String = "") As xForm
            PerfMon.Log("Cart", "pickContactXform")
            Dim oXform As xForm = New xForm

            Dim oReturnForm As xForm
            Dim oGrpElmt As XmlElement
            Dim oDs As DataSet
            Dim oDs2 As DataSet
            Dim oDr As DataRow
            Dim cSql As String = ""
            'Dim sAddressHtml As String
            Dim cProcessInfo As String = ""
            Dim contactId As Long = 0
            Dim billingAddId As Long = 0
            Dim oContactXform As xForm = Nothing
            Dim bDontPopulate As Boolean = False
            Dim bBillingSet As Boolean = False
            Dim newSubmitPrefix As String = submitPrefix
            Dim newAddressType As String = cAddressType
            Dim contactFormCmd2 As String = ""
            Try

                ' Get any existing addresses for user
                ' Changed this so it gets any

                'Check if updated primiary billing address, (TS added order by reverse order added)
                cSql = "select * from tblCartContact where nContactDirId = " & CStr(myWeb.mnUserId) & " and nContactCartId = 0 order by cContactType ASC, nContactKey DESC"
                oDs = moDBHelper.GetDataSet(cSql, "tblCartContact")
                For Each oDr In oDs.Tables("tblCartContact").Rows
                    If billingAddId = 0 Then billingAddId = oDr.Item("nContactKey")

                    If myWeb.moRequest("cartDeleditAddress" & oDr.Item("nContactKey")) <> "" Then
                        submitPrefix = "cartDel"
                        cAddressType = "Delivery Address"

                        newSubmitPrefix = "cartDel"
                        newAddressType = "Delivery Address"

                        'ensure we hit this next time through...
                        cCmdAction = "Delivery"
                        contactFormCmd2 = "cartDeleditAddress" & oDr.Item("nContactKey")


                    ElseIf myWeb.moRequest(submitPrefix & "addDelivery" & oDr.Item("nContactKey")) <> "" Then
                        bDontPopulate = True
                        newSubmitPrefix = "cartDel"
                        newAddressType = "Delivery Address"
                        cCmdAction = "Delivery"
                    ElseIf myWeb.moRequest(submitPrefix & "editAddress" & oDr.Item("nContactKey")) <> "" Then
                        If Not billingAddId = oDr.Item("nContactKey") Then
                            newSubmitPrefix = "cartDel"
                            newAddressType = "Delivery Address"
                        Else
                            'we are editing a billing address and want to ensure we dont get a double form.
                            If mcBillingAddressXform.Contains("BillingAndDeliveryAddress.xml") Then
                                mcBillingAddressXform = mcBillingAddressXform.Replace("BillingAndDeliveryAddress.xml", "BillingAddress.xml")
                            End If
                            'ensure we hit this next time through...
                            cCmdAction = "Billing"
                            contactFormCmd2 = submitPrefix & "editAddress" & oDr.Item("nContactKey")
                            bDontPopulate = True
                            'we specifiy a contactID to ensure we don't update the cart addresses just the ones on file.
                            contactId = oDr.Item("nContactKey")
                        End If
                    ElseIf myWeb.moRequest(submitPrefix & "useBilling" & oDr.Item("nContactKey")) <> "" Then
                        contactId = setCurrentBillingAddress(myWeb.mnUserId, oDr.Item("nContactKey"))
                        bBillingSet = True
                    End If
                Next

                If myWeb.moRequest(submitPrefix & "addNewAddress") <> "" Then
                    contactId = 0
                    bDontPopulate = True
                End If

                oContactXform = contactXform(newAddressType, newSubmitPrefix & "Address", cCmdType, cCmdAction, bDontPopulate, contactId, contactFormCmd2)

                'Build the xform
                oXform.moPageXML = moPageXml
                oXform.NewFrm(cAddressType)
                oXform.valid = False

                ' oReturnForm is going to be the form returned at the end of the function.
                oReturnForm = oXform

                If Not bBillingSet Then
                    contactId = setCurrentBillingAddress(myWeb.mnUserId, 0)
                Else
                    cSql = "select * from tblCartContact where nContactDirId = " & CStr(myWeb.mnUserId) & " and nContactCartId = 0 order by cContactType ASC"
                    oDs = moDBHelper.GetDataSet(cSql, "tblCartContact")
                End If

                If oDs.Tables("tblCartContact").Rows.Count > 0 Then

                    ' Create the instance
                    oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("cContactId"))

                    ' Add a value if an address has been selected
                    oDs2 = moDBHelper.GetDataSet("select * from tblCartContact where nContactCartId = " & CStr(Me.mnCartId) & " and cContactType = '" & cAddressType & "'", "tblCartContact")
                    If oDs2.Tables("tblCartContact").Rows.Count > 0 Then
                        oXform.Instance.SelectSingleNode("cContactId").InnerText = oDs2.Tables("tblCartContact").Rows(0).Item("nContactKey")
                    End If

                    oXform.Instance.AppendChild(oXform.moPageXML.CreateElement("cIsDelivery"))
                    oXform.submission("contact", mcPagePath & cCmdType & "=" & cCmdAction, "POST", )

                    oGrpElmt = oXform.addGroup(oXform.moXformElmt, "address", , "")

                    oXform.addInput(oGrpElmt, "addType", False, "", "hidden")
                    oGrpElmt.LastChild.AppendChild(oGrpElmt.OwnerDocument.CreateElement("value"))
                    oGrpElmt.LastChild.FirstChild.InnerText = cAddressType

                    'oXform.addSelect1(oGrpElmt, "cContactId", True, "Select", "multiline", xForm.ApperanceTypes.Full)
                    'oXform.addBind("cContactId", "cContactId")

                    'Add Collection Options

                    If moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection") Then
                        'Add Collection options
                        'Get the collection delivery options
                        Dim oDrCollectionOptions As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where bCollection = 1")
                        If oDrCollectionOptions.HasRows Then
                            Dim oCollectionGrp As XmlElement
                            oCollectionGrp = oXform.addGroup(oGrpElmt, "CollectionOptions", "collection-options", "")

                            Do While oDrCollectionOptions.Read()

                                Dim OptLabel As String = oDrCollectionOptions.Item("cShipOptName").ToString() & " - " & oDrCollectionOptions.Item("cShipOptCarrier").ToString()

                                oXform.addSubmit(oCollectionGrp, "collect", OptLabel, "CollectionID_" & oDrCollectionOptions.Item("nShipOptKey").ToString(), "collect btn-success principle", "fa-truck")
                            Loop
                        End If
                    End If

                    For Each oDr In oDs.Tables("tblCartContact").Rows
                        Dim oAddressGrp As XmlElement
                        oAddressGrp = oXform.addGroup(oGrpElmt, "addressGrp-" & oDr.Item("nContactKey"), "addressGrp", "")
                        oXform.addDiv(oAddressGrp, moDBHelper.getObjectInstance(dbHelper.objectTypes.CartContact, oDr.Item("nContactKey")), "pickAddress")

                        If billingAddId <> oDr.Item("nContactKey") Then
                            oXform.addSubmit(oAddressGrp, "editAddress", "Edit", "cartDeleditAddress" & oDr.Item("nContactKey"), "edit", "fa-pencil")
                            oXform.addSubmit(oAddressGrp, "removeAddress", "Del", submitPrefix & "deleteAddress" & oDr.Item("nContactKey"), "delete", "fa-trash-o")
                        Else
                            oXform.addSubmit(oAddressGrp, "editAddress", "Edit", submitPrefix & "editAddress" & oDr.Item("nContactKey"), "edit", "fa-pencil")
                            ' oXform.addSubmit(oAddressGrp, "removeAddress", "Delete", submitPrefix & "deleteAddress" & oDr.Item("nContactKey"), "delete")
                        End If

                        If oDr.Item("cContactType") <> "Billing Address" Then
                            oXform.addSubmit(oAddressGrp, oDr.Item("nContactKey"), "Use as Billing", submitPrefix & "useBilling" & oDr.Item("nContactKey"), "setAsBilling")
                        Else

                            If LCase(moCartConfig("NoDeliveryAddress")) = "on" Then
                                oXform.addSubmit(oAddressGrp, "addNewAddress", "Add New Address", submitPrefix & "addNewAddress", "addnew", "fa-plus")
                            Else
                                oXform.addSubmit(oAddressGrp, "addNewAddress", "Add New Billing Address", submitPrefix & "addNewAddress", "addnew", "fa-plus")
                            End If

                            If Not (LCase(moCartConfig("NoDeliveryAddress"))) = "on" Then
                                oXform.addSubmit(oGrpElmt, oDr.Item("nContactKey"), "New Delivery Address", submitPrefix & "addDelivery" & oDr.Item("nContactKey"), "setAsBilling btn-success principle", "fa-plus")
                            End If

                        End If

                        If LCase(moCartConfig("NoDeliveryAddress")) = "on" Then
                            oXform.addSubmit(oAddressGrp, oDr.Item("nContactKey"), "Use This Address", submitPrefix & "contact" & oDr.Item("nContactKey"))
                        Else
                            oXform.addSubmit(oAddressGrp, oDr.Item("nContactKey"), "Deliver To This Address", submitPrefix & "contact" & oDr.Item("nContactKey"))
                        End If


                    Next




                    ' Check if the form has been submitted
                    If oXform.isSubmitted Then

                        oXform.updateInstanceFromRequest()
                        Dim forCollection As Boolean = False
                        If moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection") Then
                            Dim bCollectionSelected = False
                            Dim oDrCollectionOptions As SqlDataReader = moDBHelper.getDataReader("select * from tblCartShippingMethods where bCollection = 1")

                            If oDrCollectionOptions.HasRows Then
                                Do While oDrCollectionOptions.Read()
                                    If myWeb.moRequest("CollectionID_" & oDrCollectionOptions.Item("nShipOptKey")) <> "" Then
                                        bCollectionSelected = True
                                        'Set the shipping option
                                        Dim cShippingDesc As String = oDrCollectionOptions.Item("cShipOptName").ToString() & "-" &
                                         oDrCollectionOptions.Item("cShipOptCarrier").ToString()
                                        Dim nShippingCost As Double = CDbl("0" & oDrCollectionOptions.Item("nShipOptCost"))
                                        Dim cSqlUpdate As String
                                        cSqlUpdate = "UPDATE tblCartOrder SET cShippingDesc='" & SqlFmt(cShippingDesc) & "', nShippingCost=" & SqlFmt(nShippingCost) & ", nShippingMethodId = " & oDrCollectionOptions.Item("nShipOptKey") & " WHERE nCartOrderKey=" & mnCartId
                                        moDBHelper.ExeProcessSql(cSqlUpdate)
                                        forCollection = True
                                        oXform.valid = True
                                        oContactXform.valid = True
                                        mbNoDeliveryAddress = True

                                        Dim NewInstance As XmlElement = moPageXml.CreateElement("instance")
                                        Dim delXform As xForm = contactXform("Delivery Address")

                                        NewInstance.InnerXml = delXform.Instance.SelectSingleNode("tblCartContact").OuterXml
                                        'dissassciate from user so not shown again
                                        NewInstance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = ""
                                        NewInstance.SelectSingleNode("tblCartContact/cContactName").InnerText = oDrCollectionOptions.Item("cShipOptName").ToString()
                                        NewInstance.SelectSingleNode("tblCartContact/cContactCompany").InnerText = oDrCollectionOptions.Item("cShipOptCarrier").ToString()
                                        NewInstance.SelectSingleNode("tblCartContact/cContactCountry").InnerText = moCartConfig("DefaultDeliveryCountry")


                                        Dim collectionContactID As String

                                        collectionContactID = moDBHelper.setObjectInstance(dbHelper.objectTypes.CartContact, NewInstance)

                                        useSavedAddressesOnCart(billingAddId, CInt(collectionContactID))
                                        Return oReturnForm
                                    End If
                                Loop
                                If bCollectionSelected = False Then
                                    RemoveDeliveryOption(mnCartId)
                                End If
                            End If

                        End If

                        For Each oDr In oDs.Tables("tblCartContact").Rows
                            If myWeb.moRequest(submitPrefix & "contact" & oDr.Item("nContactKey")) <> "" Then
                                contactId = oDr.Item("nContactKey")
                                'Save Behaviour
                                oXform.valid = True
                            ElseIf myWeb.moRequest(submitPrefix & "addDelivery" & oDr.Item("nContactKey")) <> "" Then
                                contactId = oDr.Item("nContactKey")
                                oXform.valid = False
                                oContactXform.valid = False
                            ElseIf myWeb.moRequest(submitPrefix & "editAddress" & oDr.Item("nContactKey")) <> "" Then
                                contactId = oDr.Item("nContactKey")
                                'edit Behavior
                            ElseIf myWeb.moRequest(submitPrefix & "deleteAddress" & oDr.Item("nContactKey")) <> "" Then
                                contactId = oDr.Item("nContactKey")
                                'delete Behavior
                                moDBHelper.DeleteObject(dbHelper.objectTypes.CartContact, contactId)
                                'remove from form
                                oXform.moXformElmt.SelectSingleNode("descendant-or-self::group[@ref='addressGrp-" & contactId & "']").ParentNode.RemoveChild(oXform.moXformElmt.SelectSingleNode("descendant-or-self::group[@ref='addressGrp-" & contactId & "']"))
                                oXform.valid = False
                            ElseIf myWeb.moRequest(submitPrefix & "useBilling" & oDr.Item("nContactKey")) <> "" Then
                                'we have handled this at the top
                                oXform.valid = False
                            End If
                        Next

                        ' Check if the contactID is populated
                        If contactId = 0 Then
                            oXform.addNote("address", xForm.noteTypes.Alert, "You must select an address from the list")
                        Else

                            ' Get the selected address
                            Dim oMatches() As DataRow = oDs.Tables("tblCartContact").Select("nContactKey = " & contactId)
                            If Not oMatches Is Nothing Then
                                Dim oMR As DataRow = oMatches(0)

                                If Not bDontPopulate Then

                                    ' Update the contactXform with the address
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText = newAddressType
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactName").InnerText = oMR.Item("cContactName") & ""
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactCompany").InnerText = oMR.Item("cContactCompany") & ""
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactAddress").InnerText = oMR.Item("cContactAddress") & ""
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactCity").InnerText = oMR.Item("cContactCity") & ""
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactState").InnerText = oMR.Item("cContactState") & ""
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactZip").InnerText = oMR.Item("cContactZip") & ""
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactCountry").InnerText = oMR.Item("cContactCountry") & ""
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactTel").InnerText = oMR.Item("cContactTel") & ""
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactFax").InnerText = oMR.Item("cContactFax") & ""
                                    oContactXform.Instance.SelectSingleNode("tblCartContact/cContactEmail").InnerText = oMR.Item("cContactEmail") & ""
                                    oContactXform.Instance.SelectSingleNode("cIsDelivery").InnerText = oXform.Instance.SelectSingleNode("cIsDelivery").InnerText

                                    oContactXform.resetXFormUI()
                                    oContactXform.addValues()

                                    ' Add hidden values for the parent address
                                    If myWeb.moRequest(submitPrefix & "editAddress" & contactId) <> "" Then

                                        oGrpElmt = oContactXform.moXformElmt.SelectSingleNode("group")
                                        oContactXform.addInput(oGrpElmt, "userAddId", False, "", "hidden")
                                        oGrpElmt.LastChild.AppendChild(oGrpElmt.OwnerDocument.CreateElement("value"))
                                        oGrpElmt.LastChild.FirstChild.InnerText = contactId 'oMR.Item("nContactKey")

                                        oContactXform.addInput(oGrpElmt, "userAddType", False, "", "hidden")
                                        oGrpElmt.LastChild.AppendChild(oGrpElmt.OwnerDocument.CreateElement("value"))
                                        oGrpElmt.LastChild.FirstChild.InnerText = newAddressType 'oMR.Item("cContactType")

                                    End If
                                End If
                            End If
                        End If
                    End If

                    oXform.addValues()

                End If

                If oContactXform.valid = False And oXform.valid = False Then
                    'both forms are invalid so we need to output one of the forms.
                    If myWeb.moRequest(submitPrefix & "editAddress" & contactId) <> "" Or myWeb.moRequest(submitPrefix & "addDelivery" & contactId) <> "" Or oContactXform.isSubmitted Then
                        'we are editing an address so show the contactXform or a contactXform has been submitted to 
                        oReturnForm = oContactXform
                    Else
                        ' We need to show the pick list if and only if :
                        ' 1. It has addresses in it
                        ' 2. There is no request to Add

                        If oXform.moXformElmt.SelectSingleNode("/model/instance").HasChildNodes() And Not (myWeb.moRequest(submitPrefix & "addNewAddress") <> "") Then
                            oReturnForm = oXform
                        Else
                            ' Add address needs to clear out the existing xForm
                            If myWeb.moRequest(submitPrefix & "addNewAddress") <> "" Then
                                oContactXform.resetXFormUI()
                                oContactXform.addValues()
                            End If
                            oReturnForm = oContactXform
                        End If
                    End If
                Else
                    ' If pick address has been submitted, then we have a contactXform that has not been submitted, and therefore not saved.  Let's save it.
                    If myWeb.moRequest(submitPrefix & "contact" & contactId) <> "" Then
                        useSavedAddressesOnCart(billingAddId, contactId)
                        'skip delivery
                        oContactXform.moXformElmt.SetAttribute("cartCmd", "ChoosePaymentShippingOption")
                    End If

                    If myWeb.moRequest(submitPrefix & "addDelivery" & contactId) <> "" Then
                        useSavedAddressesOnCart(billingAddId, 0)
                        'remove the deliver from the instance if it is there
                        Dim oNode As XmlNode
                        For Each oNode In oContactXform.Instance.SelectNodes("tblCartContact[cContactType='Delivery Address']")
                            oNode.ParentNode.RemoveChild(oNode)
                        Next
                    End If

                    If oContactXform.valid = False Then
                        oContactXform.valid = True
                        oContactXform.moXformElmt.SetAttribute("persistAddress", "false")
                    End If

                    If myWeb.moRequest(submitPrefix & "editAddress" & billingAddId) <> "" And oContactXform.valid Then
                        'We have edited a billing address and need to output the pickForm
                        oReturnForm = oXform
                    Else
                        'pass through the xform to make transparent
                        oReturnForm = oContactXform
                    End If




                End If

                'TS not sure if required after rewrite, think it is deleting addresses unessesarily.

                'If Not (oReturnForm Is Nothing) AndAlso oReturnForm.valid AndAlso oReturnForm.isSubmitted Then
                '    ' There seems to be an issue with duplicate addresses being submitted by type against an order.
                '    '  This script finds the duplciates and nullifies them (i.e. sets their cartid to be 0).
                '    cSql = "UPDATE tblCartContact SET nContactCartId = 0 " _
                '            & "FROM (SELECT nContactCartId id, cContactType type, MAX(nContactKey) As latest FROM dbo.tblCartContact WHERE nContactCartId <> 0 GROUP BY nContactCartId, cContactType HAVING COUNT(*) >1) dup " _
                '            & "INNER JOIN tblCartContact c ON c.nContactCartId = dup.id AND c.cContactType = dup.type AND c.nContactKey <> dup.latest"
                '    cProcessInfo = "Clear Duplicate Addresses: " & cSql
                '    moDBHelper.ExeProcessSql(cSql)
                'End If

                Return oReturnForm

            Catch ex As Exception
                returnException(mcModuleName, "pickContactXform", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try

        End Function


        ''' <summary>
        ''' Each user need only have a single active billing address
        ''' </summary>
        ''' <param name="UserId"></param>
        ''' <param name="ContactId"></param>
        ''' <returns>If Contact ID = 0 then uses last updated</returns>
        ''' <remarks></remarks>
        Public Function setCurrentBillingAddress(ByVal UserId As Long, ByVal ContactId As Long) As Long
            Dim cProcessInfo As String = ""
            Dim cSql As String
            Dim oDS As DataSet
            Dim oDr As DataRow

            Try
                If myWeb.mnUserId > 0 Then

                    If ContactId <> 0 Then
                        moDBHelper.updateInstanceField(dbHelper.objectTypes.CartContact, ContactId, "cContactType", "Billing Address")
                    End If

                    'Check for othersss
                    cSql = "select c.* from tblCartContact c inner JOIN tblAudit a on a.nAuditKey = c.nAuditId where nContactDirId = " & CStr(myWeb.mnUserId) & " and nContactCartId = 0  and cContactType='Billing Address' order by a.dUpdateDate DESC"
                    oDS = moDBHelper.GetDataSet(cSql, "tblCartContact")

                    For Each oDr In oDS.Tables("tblCartContact").Rows
                        If ContactId = "0" Then
                            'gets the top one
                            ContactId = oDr("nContactKey")
                        End If
                        If oDr("nContactKey") <> ContactId Then
                            moDBHelper.ExeProcessSql("update tblCartContact set cContactType='Previous Billing Address' where nContactKey=" & oDr("nContactKey"))
                        End If
                    Next

                    Return ContactId

                Else
                    Return 0
                End If

            Catch ex As Exception
                returnException(mcModuleName, "setCurrentBillingAddress", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try
        End Function

        Public Sub useSavedAddressesOnCart(ByVal billingId As Long, ByVal deliveryId As Long)
            Dim cProcessInfo As String = ""
            Dim oDs As DataSet
            Dim odr As DataRow
            Try
                'get id's of addresses allready assoicated with this cart they are being replaced
                Dim sSql As String
                sSql = "select nContactKey, cContactType, nAuditKey from tblCartContact inner join tblAudit a on nAuditId = a.nAuditKey where nContactCartId = " & CStr(mnCartId)
                oDs = moDBHelper.GetDataSet(sSql, "tblCartContact")
                Dim savedBillingId As String = ""
                Dim savedDeliveryId As String = ""
                Dim savedBillingAuditId As String = ""
                Dim savedDeliveryAuditId As String = ""
                For Each odr In oDs.Tables("tblCartContact").Rows
                    If odr("cContactType") = "Billing Address" Then
                        If savedBillingId <> "" Then
                            'delete any duplicates
                            moDBHelper.DeleteObject(dbHelper.objectTypes.CartContact, odr("nContactKey"))
                        Else
                            savedBillingId = odr("nContactKey")
                            savedBillingAuditId = odr("nAuditKey")
                        End If
                    End If
                    If odr("cContactType") = "Delivery Address" Then
                        If savedDeliveryId <> "" Then
                            'delete any duplicates
                            moDBHelper.DeleteObject(dbHelper.objectTypes.CartContact, odr("nContactKey"))
                        Else
                            savedDeliveryId = odr("nContactKey")
                            savedDeliveryAuditId = odr("nAuditKey")
                        End If
                    End If
                Next
                oDs = Nothing

                'this should update the billing address
                Dim billInstance As XmlElement = myWeb.moPageXml.CreateElement("Instance")
                billInstance.InnerXml = moDBHelper.getObjectInstance(dbHelper.objectTypes.CartContact, billingId)
                billInstance.SelectSingleNode("*/nContactKey").InnerText = savedBillingId
                billInstance.SelectSingleNode("*/nContactCartId").InnerText = mnCartId
                billInstance.SelectSingleNode("*/cContactType").InnerText = "Billing Address"
                billInstance.SelectSingleNode("*/nAuditId").InnerText = savedBillingAuditId
                billInstance.SelectSingleNode("*/nAuditKey").InnerText = savedBillingAuditId

                moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, billInstance)

                'now get the submitted delivery id instance
                If Not deliveryId = 0 Then
                    Dim delInstance As XmlElement = myWeb.moPageXml.CreateElement("Instance")
                    delInstance.InnerXml = moDBHelper.getObjectInstance(dbHelper.objectTypes.CartContact, deliveryId)
                    delInstance.SelectSingleNode("*/nContactKey").InnerText = savedDeliveryId
                    delInstance.SelectSingleNode("*/nContactCartId").InnerText = mnCartId
                    delInstance.SelectSingleNode("*/cContactType").InnerText = "Delivery Address"
                    delInstance.SelectSingleNode("*/nAuditId").InnerText = savedDeliveryAuditId
                    delInstance.SelectSingleNode("*/nAuditKey").InnerText = savedDeliveryAuditId
                    moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, delInstance)
                End If

                'here we should update the current instance so we can calculate the shipping later


            Catch ex As Exception
                returnException(mcModuleName, "useAddressesOnCart", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Protected Sub UpdateExistingUserAddress(ByRef oContactXform As xForm)
            PerfMon.Log("Cart", "UpdateExistingUserAddress")
            ' Check if it exists - if it does then update the nContactKey node
            Dim oTempCXform As New xForm
            Dim cProcessInfo As String = ""
            Dim sSql As String
            Dim nCount As Long
            Dim oAddElmt As XmlElement
            Dim oElmt As XmlElement
            Dim oElmt2 As XmlElement
            Try

                For Each oAddElmt In oContactXform.Instance.SelectNodes("tblCartContact")
                    If oAddElmt.GetAttribute("saveToUser") <> "false" Then

                        'does this address allready exist?
                        sSql = "select count(nContactKey) from tblCartContact where nContactDirId = " & myWeb.mnUserId & " and nContactCartId = 0 " &
                        " and cContactName = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactName").InnerText) & "'" &
                        " and cContactCompany = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactCompany").InnerText) & "'" &
                        " and cContactAddress = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactAddress").InnerText) & "'" &
                        " and cContactCity = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactCity").InnerText) & "'" &
                        " and cContactState = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactState").InnerText) & "'" &
                        " and cContactZip = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactZip").InnerText) & "'" &
                        " and cContactCountry = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactCountry").InnerText) & "'" &
                        " and cContactTel = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactTel").InnerText) & "'" &
                        " and cContactFax = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactFax").InnerText) & "'" &
                        " and cContactEmail = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactEmail").InnerText) & "'" &
                        " and cContactXml = '" & SqlFmt(oAddElmt.SelectSingleNode("cContactXml").InnerXml) & "'"

                        nCount = moDBHelper.ExeProcessSqlScalar(sSql)

                        If nCount = 0 Then

                            oTempCXform.NewFrm("tblCartContact")
                            oTempCXform.Instance.InnerXml = oAddElmt.OuterXml
                            Dim tempInstance As XmlElement = moPageXml.CreateElement("instance")
                            Dim ContactType As String = oTempCXform.Instance.SelectSingleNode("tblCartContact/cContactType").InnerText
                            ' Update/add the address to the table
                            ' make sure we are inserting by reseting the key

                            If myWeb.moRequest("userAddId") <> "" And ContactType = myWeb.moRequest("userAddType") Then
                                'get the id we are updating
                                Dim updateId As Long = myWeb.moRequest("userAddId")

                                oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactKey").InnerText = updateId
                                'We need to populate the auditId feilds
                                tempInstance.InnerXml = moDBHelper.getObjectInstance(dbHelper.objectTypes.CartContact, updateId)
                                'update with the fields specified
                                For Each oElmt In oTempCXform.Instance.SelectNodes("tblCartContact/*[node()!='']")
                                    If Not (oElmt.Name = "nAuditId" Or oElmt.Name = "nAuditKey") Then
                                        oElmt2 = tempInstance.SelectSingleNode("tblCartContact/" & oElmt.Name)
                                        oElmt2.InnerXml = oElmt.InnerXml
                                    End If
                                Next
                                oTempCXform.Instance = tempInstance

                            Else
                                oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactKey").InnerText = "0"
                                oTempCXform.Instance.SelectSingleNode("tblCartContact/nAuditId").InnerText = ""
                                oTempCXform.Instance.SelectSingleNode("tblCartContact/nAuditKey").InnerText = ""
                            End If

                            'separate from cart
                            oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactCartId").InnerText = "0"
                            'link to user
                            oTempCXform.Instance.SelectSingleNode("tblCartContact/nContactDirId").InnerText = myWeb.mnUserId
                            moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartContact, oTempCXform.Instance)

                        End If
                    End If
                Next

                setCurrentBillingAddress(myWeb.mnUserId, 0)


            Catch ex As Exception
                returnException(mcModuleName, "UpdateExistingUserAddress", ex, "", cProcessInfo, gbDebug)
            Finally
                oTempCXform = Nothing
            End Try
        End Sub

        Public Overridable Function discountsProcess(ByVal oElmt As XmlElement) As String
            Dim sCartCmd As String = mcCartCmd
            Dim cProcessInfo As String = ""
            Dim bAlwaysAskForDiscountCode As Boolean = IIf(LCase(moCartConfig("AlwaysAskForDiscountCode")) = "on", True, False)
            Try

                myWeb.moSession("cLogonCmd") = ""
                GetCart(oElmt)

                If moDiscount.bHasPromotionalDiscounts Or bAlwaysAskForDiscountCode Then
                    Dim oDiscountsXform As xForm = discountsXform("discountsForm", "?pgid=" & myWeb.mnPageId & "&cartCmd=Discounts")
                    If oDiscountsXform.valid = False Then
                        moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oDiscountsXform.moXformElmt)

                    Else
                        oElmt.RemoveAll()
                        sCartCmd = "RedirectSecure"
                    End If
                Else
                    oElmt.RemoveAll()
                    sCartCmd = "RedirectSecure"
                End If

                'if this returns Notes then we display for otherwise we goto processflow
                Return sCartCmd

            Catch ex As Exception
                returnException(mcModuleName, "discountsProcess", ex, "", cProcessInfo, gbDebug)
                Return ""
            End Try

        End Function


        Public Overridable Function discountsXform(Optional ByVal formName As String = "notesForm", Optional ByVal action As String = "?cartCmd=Discounts") As xForm
            PerfMon.Log("Cart", "notesXform")
            '   this function is called for the collection from a form and addition to the database
            '   of address information.

            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim sSql As String
            Dim oFormGrp As XmlElement
            Dim sXmlContent As String
            Dim promocodeElement As XmlElement = Nothing
            Dim usedPromocodeFromExternalRef As Boolean = False

            Dim cProcessInfo As String = ""
            Try
                'Get notes XML
                Dim oXform As xForm = New xForm
                oXform.moPageXML = moPageXml
                'oXform.NewFrm(formName)
                Dim cDiscountsXform As String = moCartConfig("DiscountsXform")

                If cDiscountsXform <> "" Then
                    If Not oXform.load(cDiscountsXform) Then
                        oXform.NewFrm(formName)
                        oFormGrp = oXform.addGroup(oXform.moXformElmt, "discounts", , "Missing File: " & mcNotesXForm)
                    Else

                        'add missing submission or submit buttons
                        If oXform.moXformElmt.SelectSingleNode("model/submission") Is Nothing Then
                            'If oXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                            oXform.submission(formName, action, "POST", "return form_check(this);")
                        End If
                        If oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit") Is Nothing Then
                            oXform.addSubmit(oXform.moXformElmt, "Submit", "Continue")
                        End If

                        Dim oSubmit As XmlElement = oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit")
                        If Not oSubmit Is Nothing Then
                            oFormGrp = oSubmit.ParentNode
                        Else
                            oFormGrp = oXform.addGroup(oXform.moXformElmt, "Promo", , "Enter Promotional Code")
                        End If
                        If oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode") Is Nothing Then
                            If oXform.Instance.FirstChild.SelectSingleNode("Notes") Is Nothing Then
                                oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                            End If
                            promocodeElement = oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                            oXform.addInput(oFormGrp, "Notes/PromotionalCode", False, "Promotional Code", "")
                        End If

                    End If
                Else
                    oXform.NewFrm(formName)
                    oXform.submission(formName, action, "POST", "return form_check(this);")
                    oXform.Instance.InnerXml = "<Notes/>"
                    oFormGrp = oXform.addGroup(oXform.moXformElmt, "notes", , "")

                    If oXform.Instance.FirstChild.SelectSingleNode("Notes") Is Nothing Then
                        oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                    End If

                    promocodeElement = oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                    oXform.addInput(oFormGrp, "Notes/PromotionalCode", False, "Promotional Code", "")
                    oXform.addSubmit(oFormGrp, "Submit", "Continue")

                End If
                'Open database for reading and writing


                ' External promo code checks
                If promocodeElement IsNot Nothing And Not String.IsNullOrEmpty(Me.promocodeFromExternalRef) Then

                    usedPromocodeFromExternalRef = True
                End If

                sSql = "select * from tblCartOrder where nCartOrderKey=" & mnCartId
                oDs = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                For Each oRow In oDs.Tables("Order").Rows
                    'load existing notes from Cart
                    sXmlContent = oRow("cClientNotes") & ""
                    If sXmlContent <> "" Then
                        oXform.Instance.InnerXml = sXmlContent
                    End If

                    'If this xform is being submitted
                    Dim isSubmitted As Boolean = oXform.isSubmitted
                    If isSubmitted Or myWeb.moRequest("Submit") = "Continue" _
                        Or myWeb.moRequest("Submit") = "Search" Then
                        oXform.updateInstanceFromRequest()
                        oXform.validate()
                        If oXform.valid = True Then
                            oRow("cClientNotes") = oXform.Instance.InnerXml
                            mcCartCmd = "RedirectSecure"
                        End If
                    ElseIf Not isSubmitted And usedPromocodeFromExternalRef Then
                        ' If an external promo code is in the system then save it, even before it has been submitted
                        promocodeElement = oXform.Instance.SelectSingleNode("//PromotionalCode")
                        If promocodeElement IsNot Nothing Then
                            promocodeElement.InnerText = Me.promocodeFromExternalRef
                            oRow("cClientNotes") = oXform.Instance.InnerXml
                            ' Promo code is officially in the process, so we can ditch any transitory variables.
                            Me.promocodeFromExternalRef = ""
                        End If
                    End If
                Next
                moDBHelper.updateDataset(oDs, "Order", True)

                oDs.Clear()
                oDs = Nothing
                oXform.addValues()

                Return oXform

            Catch ex As Exception
                returnException(mcModuleName, "notesXform", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try

        End Function


        Public Overridable Function notesProcess(ByVal oElmt As XmlElement) As String
            Dim sCartCmd As String = mcCartCmd
            Dim cProcessInfo As String = ""
            Try

                'should never get this far for subscriptions unless logged on.

                If Not moSubscription Is Nothing Then
                    If Not moSubscription.CheckCartForSubscriptions(mnCartId, myWeb.mnUserId) Then
                        If myWeb.mnUserId = 0 Then
                            sCartCmd = "LogonSubs"
                        End If
                    End If
                End If

                myWeb.moSession("cLogonCmd") = ""

                GetCart(oElmt)

                If mcNotesXForm <> "" Then
                    Dim oNotesXform As xForm = notesXform("notesForm", mcPagePath & "cartCmd=Notes", oElmt)
                    If oNotesXform.valid = False Then
                        moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oNotesXform.moXformElmt)
                    Else
                        oElmt.RemoveAll()
                        sCartCmd = "SkipAddress"
                    End If
                Else
                    oElmt.RemoveAll()
                    sCartCmd = "SkipAddress"
                End If

                'if this returns Notes then we display for otherwise we goto processflow
                Return sCartCmd

            Catch ex As Exception
                returnException(mcModuleName, "notesProcess", ex, "", cProcessInfo, gbDebug)
                Return ""
            End Try




        End Function






        Public Overridable Function notesXform(Optional ByVal formName As String = "notesForm", Optional ByVal action As String = "?cartCmd=Notes", Optional oCart As XmlElement = Nothing) As xForm
            PerfMon.Log("Cart", "notesXform")
            '   this function is called for the collection from a form and addition to the database
            '   of address information.

            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim sSql As String
            Dim oFormGrp As XmlElement
            Dim sXmlContent As String
            Dim promocodeElement As XmlElement = Nothing
            Dim cProcessInfo As String = ""
            Try
                'Get notes XML
                Dim oXform As xForm = New xForm
                oXform.moPageXML = moPageXml
                '         

                Select Case LCase(mcNotesXForm)
                    Case "default"
                        oXform.NewFrm(formName)
                        oXform.submission(formName, action, "POST", "return form_check(this);")
                        oXform.Instance.InnerXml = "<Notes><Notes/></Notes>"
                        oFormGrp = oXform.addGroup(oXform.moXformElmt, "notes", "term4051", "Please enter any comments on your order here")
                        oXform.addTextArea(oFormGrp, "Notes/Notes", False, "", "")
                        If moDiscount.bHasPromotionalDiscounts Then
                            'If oXform.Instance.FirstChild.SelectSingleNode("Notes") Is Nothing Then
                            If Protean.Tools.Xml.firstElement(oXform.Instance).SelectSingleNode("Notes") Is Nothing Then
                                'oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                'Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                Protean.Tools.Xml.firstElement(oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance")).AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                            End If
                            'oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                            'Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                            promocodeElement = Protean.Tools.Xml.firstElement(oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance")).AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                            oXform.addInput(oFormGrp, "Notes/PromotionalCode", False, "Promotional Code", "")
                        End If
                        oXform.addSubmit(oFormGrp, "Submit", "Continue")
                    Case "productspecific"

                        Dim oOrderLine As XmlElement
                        Dim oMasterFormXml As XmlElement = Nothing
                        For Each oOrderLine In oCart.SelectNodes("Item")
                            'get any Xform related to cart items
                            Dim contentId As Long = oOrderLine.GetAttribute("contentId")
                            sSql = "select nContentKey from tblContent c inner join tblContentRelation cr on cr.nContentChildId = c.nContentKey where c.cContentSchemaName = 'xform' and cr.nContentParentId = " & contentId
                            Dim FormId As Long = myWeb.moDbHelper.GetDataValue(sSql)
                            If Not FormId = Nothing Then
                                Dim oFormXml As XmlElement = moPageXml.CreateElement("NewXform")
                                oFormXml.InnerXml = myWeb.moDbHelper.getContentBrief(FormId)

                                If oMasterFormXml Is Nothing Then
                                    'Duplication the items for each qty in cart
                                    Dim n As Integer = 1
                                    Dim oItem As XmlElement = oFormXml.SelectSingleNode("descendant-or-self::Item")
                                    oItem.SetAttribute("name", oOrderLine.SelectSingleNode("Name").InnerText)
                                    oItem.SetAttribute("stockCode", oOrderLine.SelectSingleNode("productDetail/StockCode").InnerText)
                                    oItem.SetAttribute("number", n)

                                    Dim i As Integer
                                    For i = 2 To oOrderLine.GetAttribute("quantity")
                                        n = n + 1
                                        Dim newItem As XmlElement = oItem.CloneNode(True)
                                        newItem.SetAttribute("number", n)
                                        oItem.ParentNode.InsertAfter(newItem, oItem.ParentNode.LastChild)
                                    Next
                                    oMasterFormXml = oFormXml
                                Else
                                    'behaviour for appending additioanl product forms
                                    Dim n As Integer = 1
                                    Dim oItem As XmlElement = oFormXml.SelectSingleNode("descendant-or-self::Item")
                                    oItem.SetAttribute("name", oOrderLine.SelectSingleNode("Name").InnerText)
                                    oItem.SetAttribute("stockCode", oOrderLine.SelectSingleNode("productDetail/StockCode").InnerText)
                                    oItem.SetAttribute("number", n)

                                    Dim i As Integer
                                    For i = 1 To oOrderLine.GetAttribute("quantity")
                                        Dim newItem As XmlElement = oItem.CloneNode(True)
                                        newItem.SetAttribute("number", n)
                                        n = n + 1
                                        Dim AddAfterNode As XmlElement = oMasterFormXml.SelectSingleNode("Content/model/instance/Notes/Item[last()]")
                                        AddAfterNode.ParentNode.InsertAfter(newItem, AddAfterNode)
                                    Next
                                End If
                            End If

                        Next
                        'Load with repeats.
                        If Not oMasterFormXml Is Nothing Then
                            oXform.load(oMasterFormXml.SelectSingleNode("descendant-or-self::Content"), True)
                        End If

                        If moDiscount.bHasPromotionalDiscounts Then

                            Dim oNotesRoot As XmlElement = oXform.Instance.SelectSingleNode("Notes")
                            If oNotesRoot.SelectSingleNode("PromotionalCode") Is Nothing Then
                                promocodeElement = oNotesRoot.AppendChild(oMasterFormXml.OwnerDocument.CreateElement("PromotionalCode"))
                            End If

                            oFormGrp = oXform.moXformElmt.SelectSingleNode("descendant-or-self::group[1]")
                            oXform.addInput(oFormGrp, "Notes/PromotionalCode", False, "Promotional Code", "")

                        End If

                        If Not oXform.moXformElmt Is Nothing Then
                            'add missing submission or submit buttons
                            If oXform.moXformElmt.SelectSingleNode("model/submission") Is Nothing Then
                                'If oXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                                oXform.submission(formName, action, "POST", "return form_check(this);")
                            End If
                            If oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit") Is Nothing Then
                                oXform.addSubmit(oXform.moXformElmt, "Submit", "Continue")
                            End If
                            oXform.moXformElmt.SetAttribute("type", "xform")
                            oXform.moXformElmt.SetAttribute("name", "notesForm")
                        Else
                            oXform.NewFrm(formName)
                            oFormGrp = oXform.addGroup(oXform.moXformElmt, "notes", , "Missing File: Product has no form request ")
                            'force to true so we move on.
                            oXform.valid = True
                        End If

                    Case Else
                        If Not oXform.load(mcNotesXForm) Then
                            oXform.NewFrm(formName)
                            oFormGrp = oXform.addGroup(oXform.moXformElmt, "notes", , "Missing File: " & mcNotesXForm)
                        Else

                            'add missing submission or submit buttons
                            If oXform.moXformElmt.SelectSingleNode("model/submission") Is Nothing Then
                                'If oXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                                oXform.submission(formName, action, "POST", "return form_check(this);")
                            End If
                            If oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit") Is Nothing Then
                                oXform.addSubmit(oXform.moXformElmt, "Submit", "Continue")
                            End If
                            If moDiscount.bHasPromotionalDiscounts Then
                                Dim oSubmit As XmlElement = oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit")
                                If Not oSubmit Is Nothing Then
                                    oFormGrp = oSubmit.ParentNode
                                Else
                                    oFormGrp = oXform.addGroup(oXform.moXformElmt, "Promo", , "Enter Promotional Code")
                                End If
                                If oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode") Is Nothing Then
                                    'If oXform.Instance.FirstChild.SelectSingleNode("Notes") Is Nothing Then
                                    If Protean.Tools.Xml.firstElement(oXform.Instance).SelectSingleNode("Notes") Is Nothing Then
                                        'ocNode.AppendChild(moPageXml.ImportNode(Protean.Tools.Xml.firstElement(newXml.DocumentElement), True))
                                        'oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                        'Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                        Protean.Tools.Xml.firstElement(oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance")).AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                    End If
                                    'oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                    'Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                    promocodeElement = Protean.Tools.Xml.firstElement(oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance")).AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                    oXform.addInput(oFormGrp, "Notes/PromotionalCode", False, "Promotional Code", "")
                                End If
                            End If
                        End If
                End Select

                ' External promo code checks
                If promocodeElement IsNot Nothing And Not String.IsNullOrEmpty(Me.promocodeFromExternalRef) Then
                    promocodeElement.InnerText = Me.promocodeFromExternalRef
                    ' Promo code is officially in the process, so we can ditch any transitory variables.
                    Me.promocodeFromExternalRef = ""
                End If

                'Open database for reading and writing

                sSql = "select * from tblCartOrder where nCartOrderKey=" & mnCartId
                oDs = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                For Each oRow In oDs.Tables("Order").Rows
                    'load existing notes from Cart
                    sXmlContent = oRow("cClientNotes") & ""
                    If sXmlContent <> "" Then

                        Dim savedInstance As XmlElement = moPageXml.CreateElement("instance")
                        moPageXml.PreserveWhitespace = False
                        savedInstance.InnerXml = sXmlContent

                        If oXform.Instance.SelectNodes("*/*").Count > savedInstance.SelectNodes("*/*").Count Then
                            ' we have a greater amount of childnodes we need to merge....
                            Dim oStepElmt As XmlElement
                            Dim oStepElmtCount As Integer = 0

                            'step through each child element and replace where attributes match, leaving final
                            For Each oStepElmt In oXform.Instance.SelectNodes("*/*")
                                Dim attXpath As String = oStepElmt.Name
                                Dim attElmt As XmlAttribute
                                Dim bfirst As Boolean = True
                                For Each attElmt In oStepElmt.Attributes
                                    If bfirst Then attXpath = attXpath & "["
                                    If Not bfirst Then attXpath = attXpath & " and "
                                    attXpath = attXpath + "@" & attElmt.Name & "='" & attElmt.Value & "'"
                                    bfirst = False
                                Next
                                If Not bfirst Then attXpath = attXpath & "]"

                                If Not savedInstance.SelectSingleNode("*/" & attXpath) Is Nothing Then
                                    oStepElmt.ParentNode.ReplaceChild(savedInstance.SelectSingleNode("*/" & attXpath).CloneNode(True), oStepElmt)
                                End If
                                oStepElmtCount = oStepElmtCount + 1
                            Next

                        Else
                            oXform.Instance.InnerXml = sXmlContent
                        End If

                    End If

                    'If this xform is being submitted

                    If oXform.isSubmitted Or myWeb.moRequest("Submit") = "Continue" Or myWeb.moRequest("Submit") = "Search" Then
                        oXform.updateInstanceFromRequest()
                        oXform.validate()
                        If oXform.valid = True Then
                            oRow("cClientNotes") = oXform.Instance.InnerXml
                            'if we are useing the notes as a search facility for products
                            If myWeb.moRequest("Submit") = "Search" Then
                                mcCartCmd = "Search"
                            Else
                                mcCartCmd = "SkipAddress"
                            End If
                        End If
                    End If
                Next
                moDBHelper.updateDataset(oDs, "Order", True)

                oDs.Clear()
                oDs = Nothing
                oXform.addValues()
                notesXform = oXform

            Catch ex As Exception
                returnException(mcModuleName, "notesXform", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try

        End Function

        Public Overridable Function optionsXform(ByRef cartElmt As XmlElement) As xForm

            PerfMon.Log("Cart", "optionsXform")
            Dim ods As DataSet
            Dim ods2 As DataSet
            Dim oRow As DataRow

            Dim sSql As String
            Dim sSql2 As String

            Dim oGrpElmt As XmlElement

            Dim nQuantity As Short
            Dim nAmount As Double
            Dim nWeight As Double
            Dim cDestinationCountry As String
            Dim nShippingCost As Double
            Dim cShippingDesc As String = ""

            Dim cHidden As String = ""
            Dim bHideDelivery As Boolean = False
            Dim bHidePayment As Boolean = False
            Dim bFirstRow As Boolean = True

            ' Dim oElmt As XmlElement

            Dim sProcessInfo As String = ""
            Dim bForceValidation As Boolean = False
            Dim bAdjustTitle As Boolean = True

            'Dim cFormURL As String
            'Dim cExternalGateway As String
            'Dim cBillingAddress As String
            'Dim cPaymentResponse As String
            Dim cProcessInfo As String = ""
            Dim bAddTerms As Boolean = False
            Dim oPay As PaymentProviders
            Dim bDeny As Boolean = False
            Dim AllowedPaymentMethods As New Collections.Specialized.StringCollection

            If moPay Is Nothing Then
                oPay = New PaymentProviders(myWeb)
            Else
                oPay = moPay
            End If

            oPay.mcCurrency = mcCurrency
            Dim cDenyFilter As String = ""

            Try

                If moDBHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel") Then
                    bDeny = True
                    cDenyFilter = " and nPermLevel <> 0"
                End If

                If moCartConfig("TermsContentId") = "" And moCartConfig("TermsAndConditions") = "" Then bAddTerms = False

                nQuantity = CInt("0" & cartElmt.GetAttribute("itemCount"))
                nAmount = CDbl("0" & cartElmt.GetAttribute("totalNet")) - CDbl("0" & cartElmt.GetAttribute("shippingCost"))
                nWeight = CDbl("0" & cartElmt.GetAttribute("weight"))

                Dim nRepeatAmount As Double = CDbl("0" & cartElmt.GetAttribute("repeatPrice"))

                Dim nShippingMethodId As Integer = CDbl("0" & cartElmt.GetAttribute("shippingType"))

                If cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country") Is Nothing Then
                    sProcessInfo = "Destination Country not specified in Delivery Address"
                    cDestinationCountry = ""
                    Dim sTarget As String = ""
                    Dim oAddressElmt As XmlElement
                    For Each oAddressElmt In cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/*")
                        If sTarget <> "" Then sTarget = sTarget & ", "
                        sTarget = sTarget & oAddressElmt.InnerText
                    Next
                    Err.Raise(1004, "getParentCountries", sTarget & " Destination Country not specified in Delivery Address.")
                Else
                    cDestinationCountry = cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText
                End If
                If cDestinationCountry = "" Then cDestinationCountry = moCartConfig("DefaultCountry")
                'Go and collect the valid shipping options available for this order
                ods = getValidShippingOptionsDS(cDestinationCountry, nAmount, nQuantity, nWeight)

                Dim oOptXform As New xForm
                oOptXform.moPageXML = moPageXml

                If Not oOptXform.load("/xforms/Cart/Options.xml") Then
                    Dim notesXml As String = ""
                    If Not cartElmt.SelectSingleNode("Notes") Is Nothing Then
                        notesXml = cartElmt.SelectSingleNode("Notes").OuterXml
                    End If
                    oOptXform.NewFrm("optionsForm")
                    oOptXform.Instance.InnerXml = "<nShipOptKey/><cPaymentMethod/><terms/><confirmterms>No</confirmterms><tblCartOrder><cShippingDesc/><cClientNotes>" & notesXml & "</cClientNotes></tblCartOrder>"
                    If Not (moCartConfig("TermsContentId") = "" And moCartConfig("TermsAndConditions") = "") Then
                        bAddTerms = True
                    End If
                Else
                    bAdjustTitle = False
                    bForceValidation = True
                    If Not (moCartConfig("TermsContentId") = "" And moCartConfig("TermsAndConditions") = "") Then
                        bAddTerms = True
                    End If
                End If

                ' If there is already a submit item in the form, then maintain the event node
                ' Would rather that this whole form obeyed xform validation, but hey-ho. Ali
                Dim cEvent As String = ""
                Dim oSub As XmlElement = oOptXform.model.SelectSingleNode("submission")
                If oSub Is Nothing Then
                    cEvent = "return form_check(this);"
                Else
                    cEvent = oSub.GetAttribute("event")

                    'now remove the origional submit node coz we are going to add another. TS.
                    oSub.ParentNode.RemoveChild(oSub)
                End If

                oOptXform.submission("optionsForm", mcPagePath & "cartCmd=ChoosePaymentShippingOption", "POST", cEvent)

                Dim cUserGroups As String = ""

                Dim rowCount As Long = ods.Tables("Option").Rows.Count

                If bDeny Then
                    'remove denied delivery methods
                    If myWeb.mnUserId > 0 Then
                        Dim grpElmt As XmlElement
                        For Each grpElmt In moPageXml.SelectNodes("/Page/User/Group[@isMember='yes']")
                            cUserGroups = cUserGroups & grpElmt.GetAttribute("id") & ","
                        Next
                        cUserGroups = cUserGroups & gnAuthUsers
                    Else
                        cUserGroups = gnNonAuthUsers
                    End If

                    For Each oRow In ods.Tables("Option").Rows
                        Dim denyCount As Integer = 0
                        If bDeny Then
                            Dim permSQL As String
                            'check option is not denied
                            If Not cUserGroups = "" Then
                                permSQL = "select count(*) from tblCartShippingPermission where nPermLevel = 0 and nDirId IN (" & cUserGroups & ") and nShippingMethodId = " & oRow("nShipOptKey")
                                denyCount = moDBHelper.ExeProcessSqlScalar(permSQL)
                            End If
                        End If
                        If denyCount > 0 Then
                            oRow.Delete()
                            rowCount = rowCount - 1
                        End If
                    Next
                End If

                If rowCount = 0 Then

                    oOptXform.addGroup(oOptXform.moXformElmt, "options")
                    cartElmt.SetAttribute("errorMsg", 3)

                Else

                    ' Build the Payment Options
                    'if the root group element exists i.e. we have loaded a form in.
                    oGrpElmt = oOptXform.moXformElmt.SelectSingleNode("group")
                    If oGrpElmt Is Nothing Then
                        oGrpElmt = oOptXform.addGroup(oOptXform.moXformElmt, "options", "", "Select Payment Method")
                    End If

                    ' Even if there is only 1 option we still want to display it, if it is a non-zero value - the visitor should know the description of their delivery option
                    If ods.Tables("Option").Rows.Count = 1 Then
                        For Each oRow In ods.Tables("Option").Rows
                            If IsDBNull(oRow("nShippingTotal")) Then
                                cHidden = " hidden"
                                bHideDelivery = True
                            ElseIf oRow("nShippingTotal") = 0 Then
                                cHidden = " hidden"
                                bHideDelivery = True
                            Else
                                nShippingCost = oRow("nShippingTotal")
                                nShippingCost = CDbl(FormatNumber(nShippingCost, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))

                                oOptXform.addInput(oGrpElmt, "nShipOptKey", False, oRow("cShipOptName") & "-" & oRow("cShipOptCarrier"), "hidden")
                                oOptXform.Instance.SelectSingleNode("nShipOptKey").InnerText = oRow("nShipOptKey")

                                Dim DelInputElmt As XmlElement = oOptXform.addInput(oGrpElmt, "tblCartOrder/cShippingDesc", False, "Delivery Option", "readonly term4047")
                                Dim DelInputElmtLabel As XmlElement = DelInputElmt.SelectSingleNode("label")
                                DelInputElmtLabel.SetAttribute("name", oRow("cShipOptName"))
                                DelInputElmtLabel.SetAttribute("carrier", oRow("cShipOptCarrier"))
                                DelInputElmtLabel.SetAttribute("cost", FormatNumber(nShippingCost, 2))

                                Dim DescElement As XmlElement = oOptXform.Instance.SelectSingleNode("tblCartOrder/cShippingDesc")
                                DescElement.InnerText = oRow("cShipOptName") & "-" & oRow("cShipOptCarrier") & ": " & mcCurrencySymbol & FormatNumber(nShippingCost, 2)
                                DescElement.SetAttribute("name", oRow("cShipOptName"))
                                DescElement.SetAttribute("carrier", oRow("cShipOptCarrier"))
                                DescElement.SetAttribute("cost", FormatNumber(nShippingCost, 2))
                            End If
                        Next
                    Else
                        oOptXform.addSelect1(oGrpElmt, "nShipOptKey", False, "Delivery Type", "radios multiline", xForm.ApperanceTypes.Full)
                        bFirstRow = True
                        Dim nLastID As Integer = 0

                        ' If selected shipping method is still in those available (because we now )
                        If nShippingMethodId <> 0 Then
                            Dim bIsAvail As Boolean = False
                            For Each oRow In ods.Tables("Option").Rows
                                If Not oRow.RowState = DataRowState.Deleted Then
                                    If nShippingMethodId = oRow("nShipOptKey") Then
                                        bIsAvail = True
                                    End If
                                End If
                            Next
                            'If not then strip it out.
                            If bIsAvail = False Then
                                nShippingMethodId = 0
                                cartElmt.SetAttribute("shippingType", "0")
                                cartElmt.SetAttribute("shippingCost", "")
                                cartElmt.SetAttribute("shippingDesc", "")
                                Dim cSqlUpdate As String = "UPDATE tblCartOrder SET cShippingDesc= null, nShippingCost=null, nShippingMethodId = 0 WHERE nCartOrderKey=" & mnCartId
                                moDBHelper.ExeProcessSql(cSqlUpdate)
                            End If
                        End If

                        'If shipping option selected is collection don't change
                        Dim bCollectionSelected As Boolean = False
                        For Each oRow In ods.Tables("Option").Rows
                            If Not oRow.RowState = DataRowState.Deleted Then
                                If Not IsDBNull(oRow("bCollection")) Then
                                    If oRow("nShipOptKey") = nShippingMethodId And oRow("bCollection") = True Then
                                        bCollectionSelected = True
                                    End If
                                End If
                            End If
                        Next

                        For Each oRow In ods.Tables("Option").Rows
                            If Not oRow.RowState = DataRowState.Deleted Then
                                If (Not oRow("nShipOptKey") = nLastID) Then

                                    If bCollectionSelected Then
                                        'if collection allready selected... Show only this option
                                        If nShippingMethodId = oRow("nShipOptKey") Then
                                            oOptXform.Instance.SelectSingleNode("nShipOptKey").InnerText = CStr(oRow("nShipOptKey"))
                                            nShippingCost = CDbl("0" & oRow("nShippingTotal"))
                                            nShippingCost = CDbl(FormatNumber(nShippingCost, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                                            Dim optElmt As XmlElement = oOptXform.addOption((oGrpElmt.LastChild), oRow("cShipOptName") & "-" & oRow("cShipOptCarrier") & ": " & mcCurrencySymbol & FormatNumber(nShippingCost, 2), oRow("nShipOptKey"))
                                            Dim optLabel As XmlElement = optElmt.SelectSingleNode("label")
                                            optLabel.SetAttribute("name", oRow("cShipOptName"))
                                            optLabel.SetAttribute("carrier", oRow("cShipOptCarrier"))
                                            optLabel.SetAttribute("cost", FormatNumber(nShippingCost, 2))
                                        End If
                                    Else
                                        Dim bShowMethod As Boolean = True
                                        'Don't show if a collection method
                                        If moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection") Then
                                            If Not IsDBNull(oRow("bCollection")) Then
                                                If oRow("bCollection") = True Then
                                                    bShowMethod = False
                                                End If
                                            End If
                                        End If
                                        If bShowMethod Then
                                            If bFirstRow Then oOptXform.Instance.SelectSingleNode("nShipOptKey").InnerText = CStr(oRow("nShipOptKey"))
                                            nShippingCost = CDbl("0" & oRow("nShippingTotal"))
                                            nShippingCost = CDbl(FormatNumber(nShippingCost, 2, Microsoft.VisualBasic.TriState.True, Microsoft.VisualBasic.TriState.False, Microsoft.VisualBasic.TriState.False))
                                            Dim optElmt As XmlElement = oOptXform.addOption((oGrpElmt.LastChild), oRow("cShipOptName") & "-" & oRow("cShipOptCarrier") & ": " & mcCurrencySymbol & FormatNumber(nShippingCost, 2), oRow("nShipOptKey"))
                                            Dim optLabel As XmlElement = optElmt.SelectSingleNode("label")
                                            optLabel.SetAttribute("name", oRow("cShipOptName"))
                                            optLabel.SetAttribute("carrier", oRow("cShipOptCarrier"))
                                            optLabel.SetAttribute("cost", FormatNumber(nShippingCost, 2))
                                            bFirstRow = False
                                            nLastID = oRow("nShipOptKey")
                                        End If
                                    End If

                                End If
                            End If

                        Next
                    End If

                    ods = Nothing

                    If LCase(moCartConfig("NotesOnOptions")) = "on" Then

                        ' Dim oNotesGrp As XmlElement = oOptXform.addGroup(oOptXform.moXformElmt, "notes", "term4051", "Please add any details for the delivery here")
                        oOptXform.addTextArea(oGrpElmt, "tblCartOrder/cClientNotes/Notes/Notes", False, "Please add any details for the delivery here", "")
                        ' oGrpElmt.AppendChild(oNotesGrp)

                    End If



                    ' Allow to Select Multiple Payment Methods or just one
                    Dim oPaymentCfg As XmlNode

                    oPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                    'more than one..

                    Dim bPaymentTypeButtons As Boolean = False
                    If LCase(moCartConfig("PaymentTypeButtons")) = "on" Then bPaymentTypeButtons = True

                    bFirstRow = True
                        If Not oPaymentCfg Is Nothing Then
                            If nAmount = 0 And nRepeatAmount = 0 Then

                                oOptXform.Instance.SelectSingleNode("cPaymentMethod").InnerText = "No Charge"
                                Dim oSelectElmt As XmlElement = oOptXform.addSelect1(oGrpElmt, "cPaymentMethod", False, "Payment Method", "radios multiline", xForm.ApperanceTypes.Full)
                                oOptXform.addOption(oSelectElmt, "No Charge", "No Charge")
                                bHidePayment = False
                                AllowedPaymentMethods.Add("No Charge")

                            ElseIf oPaymentCfg.SelectNodes("provider").Count > 1 Then

                                If Not bPaymentTypeButtons Then
                                    Dim oSelectElmt As XmlElement
                                    oSelectElmt = oOptXform.moXformElmt.SelectSingleNode("descendant-or-self::select1[@ref='cPaymentMethod']")
                                    If oSelectElmt Is Nothing Then
                                        oSelectElmt = oOptXform.addSelect1(oGrpElmt, "cPaymentMethod", False, "Payment Method", "radios multiline", xForm.ApperanceTypes.Full)
                                    End If
                                    Dim nOptCount As Integer = oPay.getPaymentMethods(oOptXform, oSelectElmt, nAmount, mcPaymentMethod)

                                    'Code Moved to Get PaymentMethods

                                    If nOptCount = 0 Then
                                        oOptXform.valid = False
                                        oOptXform.addNote(oGrpElmt, xForm.noteTypes.Alert, "There is no method of payment available for your account - please contact the site administrator.")
                                    ElseIf nOptCount = 1 Then
                                        'hide the options
                                        oSelectElmt.SetAttribute("class", "hidden")
                                    End If

                                    'step throught the payment methods to set as allowed.
                                    Dim oOptElmt As XmlElement
                                    For Each oOptElmt In oSelectElmt.SelectNodes("item")
                                        AllowedPaymentMethods.Add(oOptElmt.SelectSingleNode("value").InnerText)
                                    Next
                                End If




                            ElseIf oPaymentCfg.SelectNodes("provider").Count = 1 Then
                                'or just one
                                If Not bPaymentTypeButtons Then
                                    If oPay.HasRepeatPayments Then
                                        Dim oSelectElmt As XmlElement = oOptXform.addSelect1(oGrpElmt, "cPaymentMethod", False, "Payment Method", "radios multiline", xForm.ApperanceTypes.Full)
                                        oPay.ReturnRepeatPayments(oPaymentCfg.SelectSingleNode("provider/@name").InnerText, oOptXform, oSelectElmt)

                                        oOptXform.addOption(oSelectElmt, oPaymentCfg.SelectSingleNode("provider/description").Attributes("value").Value, oPaymentCfg.SelectSingleNode("provider").Attributes("name").Value)
                                        bHidePayment = False
                                        AllowedPaymentMethods.Add(oPaymentCfg.SelectSingleNode("provider/@name").InnerText)
                                    Else
                                        bHidePayment = True
                                        oOptXform.addInput(oGrpElmt, "cPaymentMethod", False, oPaymentCfg.SelectSingleNode("provider/@name").InnerText, "hidden")
                                        oOptXform.Instance.SelectSingleNode("cPaymentMethod").InnerText = oPaymentCfg.SelectSingleNode("provider/@name").InnerText
                                        AllowedPaymentMethods.Add(oPaymentCfg.SelectSingleNode("provider/@name").InnerText)
                                    End If
                                End If
                            Else
                                oOptXform.valid = False
                                oOptXform.addNote(oGrpElmt, xForm.noteTypes.Alert, "There is no method of payment setup on this site - please contact the site administrator.")
                            End If
                        Else
                            oOptXform.valid = False
                            oOptXform.addNote(oGrpElmt, xForm.noteTypes.Alert, "There is no method of payment setup on this site - please contact the site administrator.")
                        End If

                        Dim cTermsTitle As String = "Terms and Conditions"

                        ' Adjust the group title
                        If bAdjustTitle Then
                        Dim cGroupTitle As String = "Choose Delivery Type and Payment Method"
                        If bHideDelivery And bHidePayment Then cGroupTitle = "Terms and Conditions"
                        If bHideDelivery And Not (bHidePayment) Then cGroupTitle = "Choose Payment Method"
                        If Not (bHideDelivery) And bHidePayment Then cGroupTitle = "Choose Delivery Type"
                        Dim labelElmt As XmlElement = oGrpElmt.SelectSingleNode("label")
                            labelElmt.InnerText = cGroupTitle
                            labelElmt.SetAttribute("class", "term3019")

                            ' Just so we don't show the terms and conditions title twice

                            If cGroupTitle = "Terms and Conditions" Then
                                cTermsTitle = ""
                            End If
                        End If

                        If bAddTerms Then

                            If oGrpElmt.SelectSingleNode("*[@ref='terms']") Is Nothing Then
                                oOptXform.addTextArea(oGrpElmt, "terms", False, cTermsTitle, "readonly terms-and-condiditons")
                            End If

                            If oGrpElmt.SelectSingleNode("*[@ref='confirmterms']") Is Nothing Then
                                oOptXform.addSelect(oGrpElmt, "confirmterms", False, "&#160;", "", xForm.ApperanceTypes.Full)
                                oOptXform.addOption(oGrpElmt.LastChild, "I agree to the Terms and Conditions", "Agree")
                            End If

                            If CInt("0" & moCartConfig("TermsContentId")) > 0 Then
                                Dim termsElmt As New XmlDocument
                                termsElmt.LoadXml(moDBHelper.getContentBrief(moCartConfig("TermsContentId")))
                                mcTermsAndConditions = termsElmt.DocumentElement.InnerXml
                            Else
                                mcTermsAndConditions = moCartConfig("TermsAndConditions")
                            End If

                            If mcTermsAndConditions Is Nothing Then mcTermsAndConditions = ""

                            oOptXform.Instance.SelectSingleNode("terms").InnerXml = mcTermsAndConditions

                        End If

                        oOptXform.addSubmit(oGrpElmt, "optionsForm", "Make Secure Payment")

                        If bPaymentTypeButtons Then
                            oPay.getPaymentMethodButtons(oOptXform, oOptXform.moXformElmt.SelectSingleNode("group"), 0)
                            Dim oSubmitBtn As XmlElement
                            For Each oSubmitBtn In oOptXform.moXformElmt.SelectNodes("descendant-or-self::submit")
                                AllowedPaymentMethods.Add(oSubmitBtn.GetAttribute("value"))
                            Next
                        End If
                    End If

                    oOptXform.valid = False

                Dim submittedPaymentMethod As String = myWeb.moRequest("submit")
                If submittedPaymentMethod = "Make Secure Payment" Then
                    submittedPaymentMethod = myWeb.moRequest("cPaymentMethod")
                End If

                If AllowedPaymentMethods.Contains(submittedPaymentMethod) Then ' equates to is submitted

                    'Save notes to cart

                    If LCase(moCartConfig("NotesOnOptions")) = "on" Then
                        ' If myWeb.moRequest("tblCartOrder/cClientNotes/Notes/Notes") <> "" Then
                        AddClientNotes(myWeb.moRequest("tblCartOrder/cClientNotes/Notes/Notes"))
                        ' End If
                    End If

                    If myWeb.moRequest("confirmterms") = "Agree" Or Not bAddTerms Then

                            mcPaymentMethod = submittedPaymentMethod

                            'if we have a profile split it out, allows for more than one set of settings for each payment method, only done for SecPay right now.
                            If InStr(mcPaymentMethod, "-") Then
                                Dim aPayMth() As String = Split(mcPaymentMethod, "-")
                                mcPaymentMethod = aPayMth(0)
                                mcPaymentProfile = aPayMth(1)
                            End If

                            sSql2 = "select * from tblCartOrder where nCartOrderKey = " & mnCartId
                            ods2 = moDBHelper.GetDataSet(sSql2, "Order", "Cart")
                            Dim oRow2 As DataRow, cSqlUpdate As String
                            For Each oRow2 In ods2.Tables("Order").Rows
                                Dim nShipOptKey As Long

                                If Not myWeb.moRequest("nShipOptKey") Is Nothing Then
                                    oRow2("nShippingMethodId") = myWeb.moRequest("nShipOptKey")
                                End If
                                nShipOptKey = oRow2("nShippingMethodId")
                                sSql = "select * from tblCartShippingMethods "
                                sSql = sSql & " where nShipOptKey = " & nShipOptKey
                                ods = moDBHelper.GetDataSet(sSql, "Order", "Cart")

                                For Each oRow In ods.Tables("Order").Rows
                                    cShippingDesc = oRow("cShipOptName") & "-" & oRow("cShipOptCarrier")
                                    nShippingCost = oRow("nShipOptCost")
                                    cSqlUpdate = "UPDATE tblCartOrder SET cShippingDesc='" & SqlFmt(cShippingDesc) & "', nShippingCost=" & SqlFmt(nShippingCost) & ", nShippingMethodId = " & nShipOptKey & " WHERE nCartOrderKey=" & mnCartId
                                    moDBHelper.ExeProcessSql(cSqlUpdate)
                                Next

                                ' update the cart xml

                                updateTotals(cartElmt, nAmount, nShippingCost, nShipOptKey)

                                ods2 = Nothing

                                If bForceValidation Then
                                    oOptXform.updateInstanceFromRequest()
                                    oOptXform.validate()
                                Else
                                    oOptXform.valid = True
                                End If
                            Next
                        Else
                            oOptXform.addNote("confirmterms", xForm.noteTypes.Alert, "You must agree to the terms and conditions to proceed")
                        End If
                    End If

                    If oOptXform.valid Then
                    'If we have any order notes we save them
                    If Not oOptXform.Instance.SelectSingleNode("Notes") Is Nothing Then
                        'Open database for reading and writing
                        sSql = "select * from tblCartOrder where nCartOrderKey=" & mnCartId
                        ods = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                        For Each oRow In ods.Tables("Order").Rows
                            oRow("cClientNotes") = oOptXform.Instance.SelectSingleNode("Notes").OuterXml
                            moDBHelper.updateDataset(ods, "Order", True)
                        Next
                        ods.Clear()
                        ods = Nothing
                    End If
                End If
                oOptXform.addValues()

                Return oOptXform

            Catch ex As Exception
                returnException(mcModuleName, "optionsXform", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try

        End Function


        Public Function getParentCountries(ByRef sTarget As String, ByRef nIndex As Integer) As String
            PerfMon.Log("Cart", "getParentCountries")
            Dim oDr As SqlDataReader
            Dim sSql As String


            Dim oLocations As Hashtable

            Dim nTargetId As Integer
            Dim sCountryList As String
            Dim nLocKey As Integer
            Dim cProcessInfo As String = ""
            Try

                ' First let's go and get a list of all the countries and their parent id's
                sSql = "SELECT * FROM tblCartShippingLocations ORDER BY nLocationParId"

                oDr = moDBHelper.getDataReader(sSql)
                sCountryList = ""
                nTargetId = -1

                If oDr.HasRows Then
                    oLocations = New Hashtable
                    While oDr.Read
                        Dim arrLoc(3) As String

                        arrLoc(0) = oDr("nLocationParId") & ""

                        If IsDBNull(oDr("nLocationTaxRate")) Or Not (IsNumeric(oDr("nLocationTaxRate"))) Then
                            arrLoc(2) = 0
                        Else
                            arrLoc(2) = oDr("nLocationTaxRate")
                        End If

                        If IsDBNull(oDr("cLocationNameShort")) Or IsNothing(oDr("cLocationNameShort")) Then
                            arrLoc(1) = oDr("cLocationNameFull")
                        Else
                            arrLoc(1) = oDr("cLocationNameShort")
                        End If
                        nLocKey = CInt(oDr("nLocationKey"))
                        oLocations(nLocKey) = arrLoc

                        arrLoc = Nothing

                        If IIf(IsDBNull(LCase(oDr("cLocationNameShort"))), "", LCase(oDr("cLocationNameShort"))) = LCase(Trim(sTarget)) _
                        Or IIf(IsDBNull(LCase(oDr("cLocationNameFull"))), "", LCase(oDr("cLocationNameFull"))) = LCase(Trim(sTarget)) Then
                            nTargetId = oDr("nLocationKey")
                        End If
                    End While

                    ' Iterate through the country list
                    If nTargetId <> -1 Then
                        ' Get country names
                        sCountryList = iterateCountryList(oLocations, nTargetId, nIndex)
                        sCountryList = "(" & Right(sCountryList, Len(sCountryList) - 1) & ")"
                    End If

                    oLocations = Nothing
                End If

                oDr.Close()
                oDr = Nothing
                ' If sCountryList = "" Then
                '  Err.Raise(1004, "getParentCountries", sTarget & " cannot be found as a delivery location, please add via the admin system.")
                ' End If

                Return sCountryList
            Catch ex As Exception
                returnException(mcModuleName, "getParentCountries", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try
        End Function

        Private Function iterateCountryList(ByRef oDict As Hashtable, ByRef nParent As Integer, ByRef nIndex As Integer) As String
            PerfMon.Log("Cart", "iterateCountryList")
            Dim arrTmp As Object
            Dim sListReturn As String
            Dim cProcessInfo As String = ""
            Try
                sListReturn = ""

                If oDict.ContainsKey(nParent) Then
                    arrTmp = oDict.Item(nParent)
                    sListReturn = ",'" & SqlFmt(arrTmp(nIndex)) & "'" ' Adding this line here allows the top root location to be added
                    If Not (IsDBNull(arrTmp(0)) Or arrTmp(0) = "") Then ' You're not at the root - arrTmp(0) is the parent of the location
                        If arrTmp(0) <> nParent Then ' guard against recursive loops
                            sListReturn = sListReturn & iterateCountryList(oDict, arrTmp(0), nIndex)
                        End If
                    End If
                End If

                iterateCountryList = sListReturn

            Catch ex As Exception
                returnException(mcModuleName, "iterateCountryList", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try

        End Function

        Public Sub addDateAndRef(ByRef oCartElmt As XmlElement, Optional invoiceDate As DateTime = Nothing, Optional nCartId As Long = 0)
            PerfMon.Log("Cart", "addDateAndRef")
            ' adds current date and an invoice reference number to the cart object.
            ' so the cart now contains all details needed for an invoice
            Dim cProcessInfo As String = ""
            If nCartId = 0 Then nCartId = mnCartId
            Try
                If invoiceDate = Nothing Then invoiceDate = Now()
                If nCartId = 0 Then nCartId = oCartElmt.GetAttribute("cartId")
                oCartElmt.SetAttribute("InvoiceDate", niceDate(invoiceDate))
                oCartElmt.SetAttribute("InvoiceDateTime", Now())
                oCartElmt.SetAttribute("InvoiceRef", OrderNoPrefix & CStr(nCartId))
                If mcVoucherNumber <> "" Then
                    oCartElmt.SetAttribute("payableType", "Voucher")
                    oCartElmt.SetAttribute("voucherNumber", mcVoucherNumber)
                    oCartElmt.SetAttribute("voucherValue", mcVoucherValue)
                    oCartElmt.SetAttribute("voucherExpires", mcVoucherExpires)
                End If

            Catch ex As Exception
                returnException(mcModuleName, "addDateAndRef", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Function CreateNewCart(ByRef oCartElmt As XmlElement, Optional ByVal cCartSchemaName As String = "Order") As Long
            PerfMon.Log("Cart", "CreateNewCart")
            '   user has started shopping so we need to initialise the cart and add it to the db

            Dim cProcessInfo As String = ""
            Dim oInstance As XmlDataDocument = New XmlDataDocument
            Dim oElmt As XmlElement

            Try
                'stop carts being added by robots
                If Not myWeb.moSession("previousPage") = "" Then

                    oInstance.AppendChild(oInstance.CreateElement("instance"))
                    oElmt = addNewTextNode("tblCartOrder", oInstance.DocumentElement)
                    'addNewTextNode("nCartOrderKey", oElmt)
                    addNewTextNode("cCurrency", oElmt, mcCurrencyRef)
                    addNewTextNode("cCartSiteRef", oElmt, moCartConfig("OrderNoPrefix"))
                    addNewTextNode("cCartForiegnRef", oElmt)
                    addNewTextNode("nCartStatus", oElmt, "1")
                    addNewTextNode("cCartSchemaName", oElmt, mcOrderType) '-----BJR----cCartSchemaName)
                    addNewTextNode("cCartSessionId", oElmt, mcSessionId)
                    ' MEMB - add userid to oRs if we are logged on
                    If mnEwUserId > 0 Then
                        addNewTextNode("nCartUserDirId", oElmt, CStr(mnEwUserId))
                    Else
                        addNewTextNode("nCartUserDirId", oElmt, "0")
                    End If
                    addNewTextNode("nPayMthdId", oElmt, "0")
                    addNewTextNode("cPaymentRef", oElmt)
                    addNewTextNode("cCartXml", oElmt)
                    addNewTextNode("nShippingMethodId", oElmt, "0")
                    addNewTextNode("cShippingDesc", oElmt, moCartConfig("DefaultShippingDesc"))
                    addNewTextNode("nShippingCost", oElmt, CLng(moCartConfig("DefaultShippingCost") & "0"))
                    addNewTextNode("cClientNotes", oElmt, cOrderReference) '----BJR
                    addNewTextNode("cSellerNotes", oElmt, "referer:" & myWeb.moSession("previousPage") & "/n")
                    If Not (moPageXml.SelectSingleNode("/Page/Request/GoogleCampaign") Is Nothing) Then
                        addElement(oElmt, "cCampaignCode", moPageXml.SelectSingleNode("/Page/Request/GoogleCampaign").OuterXml, True)
                    End If
                    addNewTextNode("nTaxRate", oElmt, CStr(mnTaxRate))
                    addNewTextNode("nGiftListId", oElmt, "0")
                    addNewTextNode("nAuditId", oElmt)
                    mnCartId = moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartOrder, oInstance.DocumentElement)
                    Return mnCartId
                Else
                    mnCartId = 0
                    Return mnCartId
                End If

            Catch ex As Exception
                returnException(mcModuleName, "CreateNewCart", ex, "", cProcessInfo, gbDebug)

            End Try

        End Function

        Public Function SetPaymentMethod(ByVal nPayMthdId As Long)
            Dim sSql As String = ""
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim cProcessInfo As String
            Try
                If mnCartId > 0 Then
                    'Update Seller Notes:
                    sSql = "select * from tblCartOrder where nCartOrderKey = " & mnCartId
                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                    For Each oRow In oDs.Tables("Order").Rows
                        oRow("nPayMthdId") = nPayMthdId
                    Next
                    myWeb.moDbHelper.updateDataset(oDs, "Order")
                End If

            Catch ex As Exception
                returnException(mcModuleName, "AddPaymentMethod", ex, "", cProcessInfo, gbDebug)
            End Try
        End Function

        Public Sub SetClientNotes(ByVal Notes As String)
            Dim sSql As String = ""
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim cProcessInfo As String
            Try
                If mnCartId > 0 Then
                    'Update Seller Notes:
                    sSql = "select * from tblCartOrder where nCartOrderKey = " & mnCartId
                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                    For Each oRow In oDs.Tables("Order").Rows
                        oRow("cClientNotes") = Notes
                    Next
                    myWeb.moDbHelper.updateDataset(oDs, "Order")
                End If

            Catch ex As Exception
                returnException(mcModuleName, "UpdateSellerNotes", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Function AddItem(ByVal nProductId As Long, ByVal nQuantity As Long, ByVal oProdOptions As Array, Optional ByVal cProductText As String = "", Optional ByVal nPrice As Double = 0, Optional ProductXml As String = "", Optional UniqueProduct As Boolean = False) As Boolean
            PerfMon.Log("Cart", "AddItem")
            Dim cSQL As String = "Select * From tblCartItem WHERE nCartOrderID = " & mnCartId & " AND nItemiD =" & nProductId
            Dim oDS As New DataSet
            Dim oDR1 As DataRow 'Parent Rows
            Dim oDr2 As DataRow 'Child Rows
            Dim nItemID As Integer = 0 'ID of the cart item record
            Dim nCountExOptions As Integer 'number of matching options in the old cart item
            Dim cProcessInfo As String = ""
            Dim NoOptions As Integer 'the number of options for the item
            Dim oProdXml As New XmlDocument
            Dim strPrice1 As String
            Dim nTaxRate As Long = 0
            Dim giftMessageNode As XmlNode

            Dim i As Integer
            Try
                oDS = moDBHelper.getDataSetForUpdate(cSQL, "CartItems", "Cart")
                oDS.EnforceConstraints = False
                'create relationship
                oDS.Relations.Add("Rel1", oDS.Tables("CartItems").Columns("nCartItemKey"), oDS.Tables("CartItems").Columns("nParentId"), False)
                oDS.Relations("Rel1").Nested = True
                'loop through the parent rows to check the product
                If (oDS.Tables("CartItems").Rows.Count > 0 And UniqueProduct = False) Then

                    For Each oDR1 In oDS.Tables("CartItems").Rows
                        If moDBHelper.DBN2int(oDR1.Item("nParentId")) = 0 And oDR1.Item("nItemId") = nProductId Then '(oDR1.Item("nParentId") = 0 Or IsDBNull(oDR1.Item("nParentId"))) And oDR1.Item("nItemId") = nProductId Then
                            nCountExOptions = 0
                            NoOptions = 0
                            'loop through the children(options) and count how many are the same
                            For Each oDr2 In oDR1.GetChildRows("Rel1")
                                For i = 0 To UBound(oProdOptions) - 1
                                    If UBound(oProdOptions(i)) < 1 Then
                                        'Case for text option with no index
                                        If oProdOptions(i)(0) = CStr(oDr2.Item("nItemOptGrpIdx")) Then nCountExOptions += 1
                                    Else
                                        If oProdOptions(i)(0) = oDr2.Item("nItemOptGrpIdx") And oProdOptions(i)(1) = oDr2.Item("nItemOptIdx") Then nCountExOptions += 1
                                    End If
                                Next
                                NoOptions += 1
                            Next
                            If Not oProdOptions Is Nothing Then
                                'if they are all the same then we have the correct record so it is an update
                                If ((nCountExOptions) = UBound(oProdOptions)) And ((NoOptions) = UBound(oProdOptions)) Then
                                    nItemID = oDR1.Item("NCartItemKey") 'ok, got the bugger
                                    Exit For 'exit the loop other wise we might go through some other ones
                                End If

                            Else
                                If NoOptions = 0 Then nItemID = oDR1.Item("NCartItemKey")
                            End If
                        End If
                    Next
                End If
                If nItemID = 0 Then
                    'New
                    Dim oElmt As XmlElement
                    Dim oPrice As XmlElement = Nothing
                    Dim nWeight As Long = 0

                    Dim oItemInstance As XmlDataDocument = New XmlDataDocument
                    oItemInstance.AppendChild(oItemInstance.CreateElement("instance"))
                    oElmt = addNewTextNode("tblCartItem", oItemInstance.DocumentElement)

                    addNewTextNode("nCartOrderId", oElmt, CStr(mnCartId))
                    addNewTextNode("nItemId", oElmt, nProductId)
                    addNewTextNode("cItemURL", oElmt, myWeb.GetContentUrl(nProductId)) 'Erm?

                    If ProductXml <> "" Then
                        oProdXml.InnerXml = ProductXml
                    Else
                        If nProductId > 0 Then
                            oProdXml.InnerXml = moDBHelper.ExeProcessSqlScalar("Select cContentXmlDetail FROM tblContent WHERE nContentKey = " & nProductId)
                            If Not oProdXml.SelectSingleNode("/Content/StockCode") Is Nothing Then addNewTextNode("cItemRef", oElmt, oProdXml.SelectSingleNode("/Content/StockCode").InnerText) '@ Where do we get this from?
                            If cProductText = "" Then
                                cProductText = oProdXml.SelectSingleNode("/Content/*[1]").InnerText
                            End If
                            If nPrice = 0 Then
                                oPrice = getContentPricesNode(oProdXml.DocumentElement, myWeb.moRequest("unit"), nQuantity)
                            End If
                            If Not oProdXml.SelectSingleNode("/Content[@overridePrice='true']") Is Nothing Then
                                mbOveridePrice = True
                            End If
                            'lets add the discount to the cart if supplied
                            If Not oProdXml.SelectSingleNode("/Content/Prices/Discount[@currency='" & mcCurrency & "']") Is Nothing Then
                                Dim strDiscount1 As String = oProdXml.SelectSingleNode(
                                                    "/Content/Prices/Discount[@currency='" & mcCurrency & "']"
                                                    ).InnerText
                                addNewTextNode("nDiscountValue", oElmt, IIf(IsNumeric(strDiscount1), strDiscount1, 0))
                            End If
                            If Not oProdXml.SelectSingleNode("/Content/ShippingWeight") Is Nothing Then
                                nWeight = CDbl("0" & oProdXml.SelectSingleNode("/Content/ShippingWeight").InnerText)
                            End If
                            If (UniqueProduct) Then

                                If oProdXml.SelectSingleNode("/Content/GiftMessage") Is Nothing Then
                                    giftMessageNode = oProdXml.CreateNode(Xml.XmlNodeType.Element, "GiftMessage", "")
                                    oProdXml.DocumentElement.AppendChild(giftMessageNode)
                                Else
                                    ' sGiftMessage = oProdXml.SelectSingleNode("/Content/GiftMessage").InnerText
                                End If
                            End If
                            'Add Parent Product to cart if SKU.
                            If moDBHelper.ExeProcessSqlScalar("Select cContentSchemaName FROM tblContent WHERE nContentKey = " & nProductId) = "SKU" Then
                                'Then we need to add the Xml for the ParentProduct.
                                Dim sSQL2 As String = "select TOP 1 nContentParentId from tblContentRelation where nContentChildId=" & nProductId
                                Dim nParentId As Long = moDBHelper.ExeProcessSqlScalar(sSQL2)
                                Dim ItemParent As XmlElement = addNewTextNode("ParentProduct", oProdXml.DocumentElement, "")

                                ItemParent.InnerXml = moDBHelper.GetContentDetailXml(nParentId).OuterXml
                            End If

                        End If
                    End If




                    addNewTextNode("cItemName", oElmt, cProductText)
                    addNewTextNode("nItemOptGrpIdx", oElmt, 0) 'Dont Need
                    addNewTextNode("nItemOptIdx", oElmt, 0) 'Dont Need

                    If myWeb.moRequest("unit") <> "" Then
                        addNewTextNode("cItemUnit", oElmt, myWeb.moRequest("unit"))
                    End If

                    If Not oPrice Is Nothing Then
                        strPrice1 = oPrice.InnerText
                        nTaxRate = getProductTaxRate(oPrice)
                    Else
                        strPrice1 = CStr(nPrice)
                    End If

                    If mbOveridePrice Then
                        If myWeb.moRequest("price_" & nProductId) > 0 Then
                            strPrice1 = myWeb.moRequest("price_" & nProductId)
                        End If
                    End If

                    addNewTextNode("nPrice", oElmt, IIf(IsNumeric(strPrice1), strPrice1, 0))
                    addNewTextNode("nShpCat", oElmt, -1) '@ Where do we get this from?
                    addNewTextNode("nTaxRate", oElmt, nTaxRate)
                    addNewTextNode("nQuantity", oElmt, nQuantity)
                    addNewTextNode("nWeight", oElmt, nWeight)
                    addNewTextNode("nParentId", oElmt, 0)


                    Dim ProductXmlElmt As XmlElement = addNewTextNode("xItemXml", oElmt, "")
                    ProductXmlElmt.InnerXml = oProdXml.DocumentElement.OuterXml

                    nItemID = moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement)

                    'Options
                    If Not oProdOptions Is Nothing Then
                        For i = 0 To UBound(oProdOptions)
                            If Not oProdOptions(i) Is Nothing And nQuantity > 0 Then
                                'Add Options
                                oItemInstance = New XmlDataDocument
                                oItemInstance.AppendChild(oItemInstance.CreateElement("instance"))
                                oElmt = addNewTextNode("tblCartItem", oItemInstance.DocumentElement)
                                addNewTextNode("nCartOrderId", oElmt, CStr(mnCartId))

                                Dim cStockCode As String = ""
                                Dim cOptName As String = ""
                                Dim bTextOption As Boolean = False

                                If UBound(oProdOptions(i)) < 1 Then
                                    'This option dosen't have an index value
                                    'Save the submitted value against stock code.
                                    cStockCode = Me.myWeb.moRequest.Form("opt_" & nProductId & "_" & (i + 1))
                                    cOptName = cStockCode
                                    bTextOption = True
                                Else
                                    If IsNumeric(oProdOptions(i)(0)) And IsNumeric(oProdOptions(i)(1)) Then
                                        'add the stock code from the option
                                        If Not oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]/option[" & oProdOptions(i)(1) & "]/StockCode") Is Nothing Then
                                            cStockCode = oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]/option[" & oProdOptions(i)(1) & "]/StockCode").InnerText
                                        ElseIf Not oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]/option[" & oProdOptions(i)(1) & "]/code") Is Nothing Then
                                            cStockCode = oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]/option[" & oProdOptions(i)(1) & "]/code").InnerText
                                        ElseIf Not oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]/option[" & oProdOptions(i)(1) & "]/name") Is Nothing Then
                                            cStockCode = oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]/option[" & oProdOptions(i)(1) & "]/name").InnerText
                                        End If
                                        'add the name from the option
                                        If Not oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]" & "/option[" & oProdOptions(i)(1) & "]/Name") Is Nothing Then
                                            cOptName = oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]" & "/option[" & oProdOptions(i)(1) & "]/Name").InnerText
                                        ElseIf Not oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]" & "/option[" & oProdOptions(i)(1) & "]/name") Is Nothing Then
                                            cOptName = oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]" & "/option[" & oProdOptions(i)(1) & "]/name").InnerText
                                        ElseIf Not oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]" & "/option[" & oProdOptions(i)(1) & "]/@name") Is Nothing Then
                                            cOptName = oProdXml.SelectSingleNode("/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]" & "/option[" & oProdOptions(i)(1) & "]/@name").InnerText
                                        End If
                                    Else
                                        cStockCode = ""
                                        cOptName = "Invalid Option"
                                    End If
                                End If

                                addNewTextNode("cItemRef", oElmt, cStockCode)
                                addNewTextNode("nItemId", oElmt, nProductId)
                                addNewTextNode("cItemURL", oElmt, myWeb.mcOriginalURL) 'Erm?
                                addNewTextNode("cItemName", oElmt, cOptName)

                                If bTextOption Then
                                    'save the option index as -1 for text option
                                    addNewTextNode("nItemOptGrpIdx", oElmt, (i + 1))
                                    addNewTextNode("nItemOptIdx", oElmt, -1)
                                    'No price variation for text options
                                    addNewTextNode("nPrice", oElmt, "0")
                                Else
                                    addNewTextNode("nItemOptGrpIdx", oElmt, oProdOptions(i)(0))
                                    addNewTextNode("nItemOptIdx", oElmt, oProdOptions(i)(1))

                                    Dim oPriceElmt As XmlElement = oProdXml.SelectSingleNode(
                                                                "/Content/Options/OptGroup[" & oProdOptions(i)(0) & "]" &
                                                                "/option[" & oProdOptions(i)(1) & "]/Prices/Price[@currency='" & mcCurrency & "']"
                                                                )
                                    Dim strPrice2 As String = 0
                                    If Not oPriceElmt Is Nothing Then strPrice2 = oPriceElmt.InnerText
                                    addNewTextNode("nPrice", oElmt, IIf(IsNumeric(strPrice2), strPrice2, 0))
                                End If
                                addNewTextNode("nShpCat", oElmt, -1)
                                addNewTextNode("nTaxRate", oElmt, 0)
                                addNewTextNode("nQuantity", oElmt, 1)
                                addNewTextNode("nWeight", oElmt, 0)
                                addNewTextNode("nParentId", oElmt, nItemID)
                                moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement)
                            End If
                        Next
                    End If
                Else
                    'Existing
                    oDS.Relations.Clear()
                    If nQuantity <= 0 Then
                        moDBHelper.DeleteObject(Cms.dbHelper.objectTypes.CartItem, nItemID, False)
                    Else

                        For Each oDR1 In oDS.Tables("CartItems").Rows
                            If oDR1.Item("nCartItemKey") = nItemID Then
                                oDR1.BeginEdit()
                                oDR1("nQuantity") += nQuantity
                                oDR1.EndEdit()
                                Exit For
                            End If
                        Next
                    End If
                    moDBHelper.updateDataset(oDS, "CartItems")
                End If
                Return True
            Catch ex As Exception
                returnException(mcModuleName, "addItem", ex, "", cProcessInfo, gbDebug)
            End Try
        End Function

        Public Overridable Function AddItems() As Boolean
            PerfMon.Log("Cart", "AddItems")
            'this function checks for an identical item in the database.
            ' If there is, the quantity is increased accordingly.
            ' If not, a new item is added to the table


            Dim cProcessInfo As String = "Checking Submitted Products and Options"
            Dim oItem1 As Object 'Object for product keys/quantittie
            Dim oItem2 As Object 'Object for options
            Dim strAddedProducts As String = "Start:" 'string of products added
            Dim oOptions(1) As Array 'an array of option arrays (2 dimensional array)
            Dim nCurOptNo As Integer = 0
            Dim oCurOpt() As String 'CurrentOption bieng evaluated
            Dim nProductKey As Long
            Dim nQuantity As Long
            Dim nI As Integer
            Dim cReplacementName As String = ""
            'test string
            'qty_233=1 opt_233_1=1_2,1_3 opt_233_2=1_5
            Dim cSql As String
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim qtyAdded As Integer = 0

            Try

                If LCase(moCartConfig("ClearOnAdd")) = "on" Then
                    cSql = "select nCartItemKey from tblCartItem where nCartOrderId = " & mnCartId
                    oDs = moDBHelper.GetDataSet(cSql, "Item")
                    If oDs.Tables("Item").Rows.Count > 0 Then
                        For Each oRow In oDs.Tables("Item").Rows
                            moDBHelper.DeleteObject(dbHelper.objectTypes.CartItem, oRow("nCartItemKey"))
                        Next
                    End If
                End If

                If LCase(mmcOrderType) = LCase(mcItemOrderType) Then ' test for order?
                    For Each oItem1 In myWeb.moRequest.Form 'Loop for getting products/quants
                        'set defaults
                        nProductKey = 0
                        nQuantity = 0
                        oOptions = Nothing
                        cReplacementName = ""



                        'begin
                        If InStr(oItem1, "qty_") = 1 Then 'check for getting productID and quantity (since there will only be one of these per item submitted)
                            nProductKey = CLng(Replace(oItem1, "qty_", "")) 'Product key
                            cProcessInfo = oItem1.ToString & " = " & myWeb.moRequest.Form.Get(oItem1)

                            If IsNumeric(myWeb.moRequest.Form.Get(oItem1)) Then
                                nQuantity = myWeb.moRequest.Form.Get(oItem1)
                            End If

                            'replacementName
                            If nQuantity > 0 Then
                                qtyAdded = qtyAdded + nQuantity
                                If Not InStr(strAddedProducts, "'" & nProductKey & "'") > 0 Then ' double check we havent added this product (dont really need but good just in case)
                                    For Each oItem2 In myWeb.moRequest.Form 'loop through again checking for options
                                        If oItem2 = "replacementName_" & nProductKey Then cReplacementName = myWeb.moRequest.Form.Get(oItem2)
                                        If InStr(oItem2, "_") > 0 Then
                                            If Split(oItem2, "_")(0) & "_" & Split(oItem2, "_")(1) = "opt_" & nProductKey Then 'check it is an option
                                                oCurOpt = Split(myWeb.moRequest.Form.Get(oItem2), ",") 'get array of option in "1_2" format
                                                For nI = 0 To UBound(oCurOpt) 'loop through current options to split into another array
                                                    ReDim Preserve oOptions(nCurOptNo + 1) 'redim the array to new length while preserving the current data
                                                    oOptions(nCurOptNo) = Split(oCurOpt(nI), "_") 'split out the arrays of options
                                                    nCurOptNo += 1 'update number of options
                                                Next
                                            End If 'end option check
                                        End If
                                    Next 'end option loop
                                    'Add Item
                                    AddItem(nProductKey, nQuantity, oOptions, cReplacementName)
                                    'Add Item to "Done" List
                                    strAddedProducts &= "'" & nProductKey & "',"
                                End If
                            End If 'end check for previously added
                        End If 'end check for item/quant
                    Next 'End Loop for getting products/quants
                    If qtyAdded > 0 Then
                        AddItems = True
                    Else
                        AddItems = False
                    End If
                Else
                    AddItems = False
                End If
            Catch ex As Exception
                returnException(mcModuleName, "addItems", ex, "", cProcessInfo, gbDebug)
                Return False
            End Try

        End Function

        Public Function RemoveItem(Optional ByVal nItemId As Long = 0, Optional ByVal nContentId As Long = 0) As Integer
            PerfMon.Log("Cart", "RemoveItem")
            '   deletes record from item table in db

            Dim oDr As SqlDataReader
            Dim sSql As String
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim cProcessInfo As String = ""
            Dim itemCount As Long
            If IsNumeric(myWeb.moRequest("id")) Then nItemId = myWeb.moRequest("id")
            Try
                'If myWeb.moRequest("id") <> "" Then

                If nContentId = 0 Then
                    sSql = "select nCartItemKey from tblCartItem where (nCartItemKey = " & nItemId & " Or nParentId = " & nItemId & ") and nCartOrderId = " & mnCartId
                Else
                    sSql = "select nCartItemKey from tblCartItem where nItemId = " & nContentId & " and nCartOrderId = " & mnCartId
                End If


                oDs = moDBHelper.GetDataSet(sSql, "Item")
                If oDs.Tables("Item").Rows.Count > 0 Then
                    For Each oRow In oDs.Tables("Item").Rows
                        moDBHelper.DeleteObject(dbHelper.objectTypes.CartItem, oRow("nCartItemKey"))
                    Next
                End If


                ' REturn the cart order item count
                sSql = "select count(*) As ItemCount from tblCartItem where nCartOrderId = " & mnCartId
                oDr = moDBHelper.getDataReader(sSql)
                If oDr.HasRows Then
                    While oDr.Read
                        itemCount = CInt(oDr("ItemCount"))
                    End While
                End If

                oDr.Close()
                oDr = Nothing
                Return itemCount

            Catch ex As Exception
                returnException(mcModuleName, "removeItem", ex, "", cProcessInfo, gbDebug)
            End Try

        End Function

        Public Function UpdateItem(Optional ByVal nItemId As Long = 0, Optional ByVal nContentId As Long = 0, Optional ByVal qty As Long = 1, Optional ByVal SkipPackaging As Boolean = False) As Integer
            PerfMon.Log("Cart", "RemoveItem")
            '   deletes record from item table in db

            Dim oDr As SqlDataReader
            Dim sSql As String
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim cProcessInfo As String = ""
            Dim itemCount As Long
            Try

                If LCase(moCartConfig("ClearOnAdd")) = "on" Then
                    Dim cSql As String = "select nCartItemKey from tblCartItem where nCartOrderId = " & mnCartId
                    oDs = moDBHelper.GetDataSet(cSql, "Item")
                    If oDs.Tables("Item").Rows.Count > 0 Then
                        For Each oRow In oDs.Tables("Item").Rows
                            moDBHelper.DeleteObject(dbHelper.objectTypes.CartItem, oRow("nCartItemKey"))
                        Next
                    End If
                End If

                'If myWeb.moRequest("id") <> "" Then
                If qty > 0 Then

                    'sSql = "delete from tblCartItem where nCartItemKey = " & myWeb.moRequest("id") & "and nCartOrderId = " & mnCartId
                    If nContentId = 0 Then
                        If (SkipPackaging = False) Then
                            sSql = "select * from tblCartItem where (nCartItemKey = " & nItemId & " Or nParentId = " & nItemId & ") and nCartOrderId = " & mnCartId
                        Else
                            sSql = "select * from tblCartItem where (nCartItemKey = " & nItemId & ") and nCartOrderId = " & mnCartId
                        End If
                    Else
                        sSql = "select * from tblCartItem where nItemId = " & nContentId & " and nCartOrderId = " & mnCartId
                    End If
                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Item")
                    If oDs.Tables("Item").Rows.Count > 0 Then
                        For Each oRow In oDs.Tables("Item").Rows
                            oRow("nQuantity") = qty
                        Next
                    Else
                        AddItem(nContentId, qty, Nothing)
                    End If
                    moDBHelper.updateDataset(oDs, "Item")
                    oDs = Nothing
                Else
                    RemoveItem(nItemId, nContentId)
                End If


                ' REturn the cart order item count
                sSql = "select count(*) As ItemCount from tblCartItem where nCartOrderId = " & mnCartId
                oDr = moDBHelper.getDataReader(sSql)
                If oDr.HasRows Then
                    While oDr.Read
                        itemCount = CInt(oDr("ItemCount"))
                    End While
                End If

                oDr.Close()
                oDr = Nothing
                Return itemCount

            Catch ex As Exception
                returnException(mcModuleName, "removeItem", ex, "", cProcessInfo, gbDebug)
            End Try

        End Function




        ''' <summary>
        ''' Empties all items in a shopping cart.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub EmptyCart()
            PerfMon.Log("Cart", "EmptyCart")

            Dim oDr As SqlDataReader
            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try
                ' Return the cart order item count
                sSql = "select nCartItemKey from tblCartItem where nCartOrderId = " & mnCartId
                oDr = moDBHelper.getDataReader(sSql)
                If oDr.HasRows Then
                    While oDr.Read
                        moDBHelper.DeleteObject(dbHelper.objectTypes.CartItem, oDr("nCartItemKey"))
                    End While
                End If

                oDr.Close()
                oDr = Nothing

            Catch ex As Exception
                returnException(mcModuleName, "EmptyCart", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Sub UpdateCartDeposit(ByRef oRoot As XmlElement, ByVal nPaymentAmount As Double, ByVal cPaymentType As String)
            PerfMon.Log("Cart", "UpdateCartDeposit")
            Dim oDr As SqlDataReader
            Dim sSql As String
            Dim nAmountReceived As Double = 0.0#
            Dim cUniqueLink As String = ""
            Dim cProcessInfo As String = ""
            Try

                ' If the cPaymentType is deposit then we need to make a link, otherwise we need to get the paymentReceived details.
                If cPaymentType = "deposit" Then
                    ' Get the unique link from the cart
                    cUniqueLink = ", cSettlementID='" & oRoot.GetAttribute("settlementID") & "' "
                Else
                    ' Get the amount received so far
                    sSql = "select * from tblCartOrder where nCartOrderKey = " & mnCartId
                    oDr = moDBHelper.getDataReader(sSql)
                    If oDr.HasRows Then
                        While oDr.Read
                            nAmountReceived = CDbl(oDr("nAmountReceived"))
                            cUniqueLink = ", cSettlementID='OLD_" & oDr("cSettlementID") & "' "
                        End While
                    End If
                    oDr.Close()


                End If

                nAmountReceived = nAmountReceived + nPaymentAmount

                sSql = "update tblCartOrder set nAmountReceived = " & nAmountReceived & ", nLastPaymentMade= " & nPaymentAmount & cUniqueLink & " where nCartOrderKey = " & mnCartId
                moDBHelper.ExeProcessSql(sSql)

            Catch ex As Exception
                returnException(mcModuleName, "QuitCart", ex, "", cProcessInfo, gbDebug)
            Finally
                oDr = Nothing
            End Try

        End Sub

        Public Sub QuitCart()
            PerfMon.Log("Cart", "QuitCart")
            '   set the cart status to 7

            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try


                ' Old delete calls - DON't DELETE THE CART, Simply set the status to abandoned.
                ' sSql = "delete from tblCartItem where nCartOrderId = " & mnCartId
                ' oDb.exeProcessSQL sSql, mcEwDataConn
                ' sSql = "delete from tblCartOrder where nCartOrderKey =" & mnCartId
                ' oDb.exeProcessSQL sSql, mcEwDataConn

                sSql = "update tblCartOrder set nCartStatus = 11 where(nCartOrderKey = " & mnCartId & ")"
                moDBHelper.ExeProcessSql(sSql)
                mnTaxRate = moCartConfig("TaxRate")

            Catch ex As Exception
                returnException(mcModuleName, "QuitCart", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Overridable Sub EndSession()
            PerfMon.Log("Cart", "EndSession")
            Dim sProcessInfo As String = ""
            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try
                clearSessionCookie()
                sSql = "update tblCartOrder set cCartSessionId = 'OLD_' + cCartSessionId where(nCartOrderKey = " & mnCartId & ")"
                moDBHelper.ExeProcessSql(sSql)
                mmcOrderType = ""
                mnCartId = 0
                myWeb.moSession("CartId") = Nothing
                mnTaxRate = moCartConfig("TaxRate")

            Catch ex As Exception
                returnException(mcModuleName, "EndSession", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        Public Function updateCart(ByRef cSuccessfulCartCmd As String) As Object
            PerfMon.Log("Cart", "updateCart")
            '   user can decide to change the quantity of identical items or change the shipping options
            '   changes to the database are made in this function

            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim sSql As String
            Dim nItemCount As Integer

            Dim cProcessInfo As String = ""
            Try

                'Go through the items associated with the order
                sSql = "select * from tblCartItem where nCartOrderId = " & mnCartId
                cProcessInfo = sSql
                oDs = moDBHelper.getDataSetForUpdate(sSql, "Item", "Cart")
                '@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                oDs.Relations.Add("Rel1", oDs.Tables("Item").Columns("nCartItemKey"), oDs.Tables("Item").Columns("nParentId"), False)
                oDs.Relations("Rel1").Nested = True
                '@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                nItemCount = 0 ' nItemCount - keeps a running total (accounting for deletions)

                Dim bNullParentId As Boolean = False

                For Each oRow In oDs.Tables("Item").Rows
                    If Not oRow.RowState = DataRowState.Deleted Then
                        bNullParentId = False
                        If oRow("nParentId") Is DBNull.Value Then
                            bNullParentId = True
                        ElseIf oRow("nParentId") = 0 Then
                            bNullParentId = True
                        End If
                        If bNullParentId Then ' for options
                            nItemCount = nItemCount + 1
                            ' First check if the quantity is numeric (if not ignore it)
                            If IsNumeric(myWeb.moRequest("itemId-" & oRow("nCartItemKey"))) Then
                                ' It's numeric - let's see if it's positive (i.e. update it, if not delete it)
                                If CInt(myWeb.moRequest("itemId-" & oRow("nCartItemKey"))) > 0 Then
                                    oRow("nQuantity") = myWeb.moRequest("itemId-" & oRow("nCartItemKey"))
                                Else
                                    'delete options first
                                    Dim oCRows() As DataRow = oRow.GetChildRows("Rel1")
                                    Dim nDels As Integer
                                    For nDels = 0 To UBound(oCRows)
                                        oCRows(nDels).Delete()
                                    Next
                                    'end delete options
                                    oRow.Delete()
                                    nItemCount = nItemCount - 1
                                End If
                            End If
                        End If 'for options
                    End If
                Next
                moDBHelper.updateDataset(oDs, "Item")

                ' If itemCount is 0 or less Then quit the cart, otherwise update the cart
                If nItemCount > 0 Then

                    'Get the Cart Order
                    sSql = "select * from tblCartOrder where nCartOrderKey=" & mnCartId
                    cProcessInfo = sSql
                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                    For Each oRow In oDs.Tables("Order").Rows
                        oRow.BeginEdit()
                        'update the "cart last update" date
                        moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.Audit, , oRow("nAuditId"))

                        'Update the Client notes, only if no separate form
                        If mcNotesXForm = "" And Not myWeb.moRequest("cClientNotes") = "" Then
                            oRow("cClientNotes") = myWeb.moRequest("cClientNotes")
                        End If
                        oRow("nCartStatus") = mnProcessId

                        '------------BJR-------------------
                        oRow("cCartSchemaName") = mcOrderType
                        'oRow("cClientNotes") = cOrderReference
                        '----------------------------------
                        If Not CStr(oRow("cSellerNotes")).Contains("Referrer: " & myWeb.Referrer) And Not myWeb.Referrer = "" Then
                            oRow("cSellerNotes") &= "/n" & "Referrer: " & myWeb.Referrer & "/n"
                        End If
                        oRow.EndEdit()
                    Next
                    moDBHelper.updateDataset(oDs, "Order")

                    ' Set the successful Cart Cmd
                    mcCartCmd = cSuccessfulCartCmd
                    Return cSuccessfulCartCmd
                Else

                    mnProcessId = 0
                    mcCartCmd = "Quit"
                    'Return Nothing
                    Return mcCartCmd
                End If


            Catch ex As Exception
                returnException(mcModuleName, "UpdateCart", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try
        End Function

        Public Sub ListOrders(ByRef oContentsXML As XmlElement, ByVal ProcessId As cartProcess)
            PerfMon.Log("Cart", "ListOrders")
            Dim oRoot As XmlElement
            Dim oElmt As XmlElement
            Dim sSql As String
            Dim oDs As DataSet
            Dim cProcessInfo As String = ""
            Try

                oRoot = moPageXml.CreateElement("Content")
                oRoot.SetAttribute("type", "listTree")
                oRoot.SetAttribute("template", "default")
                oRoot.SetAttribute("name", "Orders - " & ProcessId.GetType.ToString)

                sSql = "SELECT nCartOrderKey as id, c.cContactName as name, c.cContactEmail as email, a.dUpdateDate from tblCartOrder inner join tblAudit a on nAuditId = a.nAuditKey left outer join tblCartContact c on (nCartOrderKey = c.nContactCartId and cContactType = 'Billing Address') where nCartStatus = " & ProcessId

                oDs = moDBHelper.GetDataSet(sSql, "Order", "List")

                If oDs.Tables(0).Rows.Count > 0 Then
                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute

                    'load existing data into the instance
                    oElmt = moPageXml.CreateElement("List")
                    oElmt.InnerXml = oDs.GetXml

                    oContentsXML.AppendChild(oElmt)
                End If

            Catch ex As Exception
                returnException(mcModuleName, "ListShippingLocations", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Sub ListShippingLocations(ByRef oContentsXML As XmlElement, Optional ByVal OptId As Long = 0)
            PerfMon.Log("Cart", "ListShippingLocations")
            Dim oRoot As XmlElement
            Dim oElmt As XmlElement
            Dim sSql As String
            Dim oDs As DataSet
            Dim cProcessInfo As String = ""
            Try

                oRoot = moPageXml.CreateElement("Content")
                oRoot.SetAttribute("type", "listTree")
                oRoot.SetAttribute("template", "default")
                If OptId <> 0 Then
                    oRoot.SetAttribute("name", "Shipping Locations Form")
                    sSql = "SELECT nLocationKey as id, nLocationType as type, nLocationParId as parid, cLocationNameFull as Name, cLocationNameShort as nameShort, (SELECT COUNT(*) from tblCartShippingRelations r where r.nShpLocId = n.nLocationKey and r.nShpOptId = " & OptId & ") As selected from tblCartShippingLocations n "
                Else
                    oRoot.SetAttribute("name", "Shipping Locations")
                    sSql = "SELECT nLocationKey as id, nLocationType as type, nLocationParId as parid, cLocationNameFull as Name, cLocationNameShort as nameShort, (SELECT COUNT(*) from tblCartShippingRelations r where r.nShpLocId = n.nLocationKey) As nOptCount from tblCartShippingLocations n "
                End If

                ' NOTE : This SQL is NOT the same as the equivalent function in the EonicWeb component.
                '        It adds a count of shipping option relations for each location

                oDs = moDBHelper.GetDataSet(sSql, "TreeItem", "Tree")

                oDs.Relations.Add("rel01", oDs.Tables(0).Columns("id"), oDs.Tables(0).Columns("parId"), False)
                oDs.Relations("rel01").Nested = True

                If oDs.Tables(0).Rows.Count > 0 Then
                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Hidden
                    oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(4).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(5).ColumnMapping = Data.MappingType.Attribute

                    'load existing data into the instance
                    oElmt = moPageXml.CreateElement("Tree")
                    oElmt.InnerXml = oDs.GetXml


                    Dim oCheckElmt As XmlElement = Nothing
                    oCheckElmt = oElmt.SelectSingleNode("descendant-or-self::TreeItem[@id='" & mnShippingRootId & "']")
                    If Not oCheckElmt Is Nothing Then oElmt = oCheckElmt

                    oContentsXML.AppendChild(oElmt)
                End If


            Catch ex As Exception
                returnException(mcModuleName, "ListShippingLocations", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Sub ListDeliveryMethods(ByRef oContentsXML As XmlElement)
            PerfMon.Log("Cart", "ListDeliveryMethods")
            Dim oElmt As XmlElement
            Dim sSql As String
            Dim oDs As DataSet
            Dim cProcessInfo As String = ""
            Try

                sSql = "select a.nStatus as status, nShipOptKey as id, cShipOptName as name, cShipOptCarrier as carrier, a.dPublishDate as startDate, a.dExpireDate as endDate, tblCartShippingMethods.cCurrency from tblCartShippingMethods left join tblAudit a on a.nAuditKey = nAuditId order by nDisplayPriority"
                oDs = moDBHelper.GetDataSet(sSql, "ListItem", "List")

                If oDs.Tables(0).Rows.Count > 0 Then
                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(2).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(3).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(4).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(5).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(6).ColumnMapping = Data.MappingType.Attribute
                    'load existing data into the instance
                    oElmt = moPageXml.CreateElement("List")
                    oElmt.InnerXml = oDs.GetXml

                    oContentsXML.AppendChild(oElmt.FirstChild)
                End If


            Catch ex As Exception
                returnException(mcModuleName, "ListDeliveryMethods", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub


        Public Sub ListCarriers(ByRef oContentsXML As XmlElement)
            PerfMon.Log("Cart", "ListDeliveryMethods")
            Dim oElmt As XmlElement
            Dim sSql As String
            Dim oDs As DataSet
            Dim cProcessInfo As String = ""
            Try

                sSql = "select a.nStatus as status, nCarrierKey as id, cCarrierName as name, cCarrierTrackingInstructions as info, a.dPublishDate as startDate, a.dExpireDate as endDate from tblCartCarrier left join tblAudit a on a.nAuditKey = nAuditId"
                oDs = moDBHelper.GetDataSet(sSql, "Carrier", "Carriers")

                If oDs.Tables(0).Rows.Count > 0 Then
                    oDs.Tables(0).Columns(0).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(1).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(4).ColumnMapping = Data.MappingType.Attribute
                    oDs.Tables(0).Columns(5).ColumnMapping = Data.MappingType.Attribute
                    'load existing data into the instance
                    oElmt = moPageXml.CreateElement("Carriers")
                    oElmt.InnerXml = oDs.GetXml

                    oContentsXML.AppendChild(oElmt.FirstChild)
                End If


            Catch ex As Exception
                returnException(mcModuleName, "ListCarriers", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Sub ListPaymentProviders(ByRef oContentsXML As XmlElement)
            PerfMon.Log("Cart", "ListPaymentProviders")
            Dim oElmt As XmlElement
            Dim oElmt2 As XmlElement
            Dim cProcessInfo As String = ""
            Dim oPaymentCfg As XmlNode
            Dim Folder As String = "/ewcommon/xforms/PaymentProvider/"
            Dim fi As FileInfo
            Dim ProviderName As String

            Try

                oPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")

                oElmt = moPageXml.CreateElement("List")

                Dim dir As New DirectoryInfo(moServer.MapPath(Folder))
                Dim files As FileInfo() = dir.GetFiles()

                For Each fi In files
                    If fi.Extension = ".xml" Then
                        ProviderName = Replace(fi.Name, fi.Extension, "")
                        oElmt2 = addNewTextNode("Provider", oElmt, Replace(ProviderName, "-", " "))
                        If Not oPaymentCfg.SelectSingleNode("/payment/provider[@name='" & Replace(ProviderName, "-", "") & "']") Is Nothing Then
                            oElmt2.SetAttribute("active", "true")
                        End If
                    End If
                Next

                oContentsXML.AppendChild(oElmt)

            Catch ex As Exception
                returnException(mcModuleName, "ListPaymentProviders", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub




        Private Sub AddDeliveryFromGiftList(ByVal nGiftListId As String)
            PerfMon.Log("Cart", "AddDeliveryFromGiftList")
            Dim oDs As DataSet
            Dim oXml As XmlDocument = New XmlDocument
            Dim cProcessInfo As String = ""
            Try

                'does the order allready contain a delivery address?
                oDs = moDBHelper.GetDataSet("select * from tblCartContact where cContactType='Delivery Address' and nContactParentId > 0 and nContactParentId=" & mnCartId.ToString & " and nContactParentType=1", "tblCartContact")
                If oDs.Tables("tblCartContact").Rows.Count = 0 Then

                    oDs.Dispose()
                    oDs = Nothing
                    oDs = moDBHelper.GetDataSet("select * from tblCartContact where cContactType='Delivery Address' and nContactParentId > 0 and nContactParentId=" & nGiftListId & " and nContactParentType=1", "tblCartContact")

                    If oDs.Tables("tblCartContact").Rows.Count = 1 Then

                        oXml.LoadXml(oDs.GetXml)
                        oXml.SelectSingleNode("NewDataSet/tblCartContact/nContactKey").InnerText = "-1"
                        '   oXml.SelectSingleNode("NewDataSet/tblCartContact/nContactParentType").InnerText = "1"
                        oXml.SelectSingleNode("NewDataSet/tblCartContact/nContactParentId").InnerText = mnCartId.ToString

                        moDBHelper.saveInstance(oXml.DocumentElement, "tblCartContact", "nContactKey")

                    End If

                End If

                'OK now add the GiftList Id to the cart
                mnGiftListId = nGiftListId

                oDs.Dispose()
                oDs = Nothing
                oXml = Nothing

            Catch ex As Exception
                returnException(mcModuleName, "AddDeliveryFromGiftList", ex, "", cProcessInfo, gbDebug)
            End Try

        End Sub

        ''' <summary>
        ''' Calculates and updates the tax rate.
        ''' </summary>
        ''' <param name="cContactCountry">Optional: The name of the current current to work out the tax rate for.  If empty, then the default tax rate is assumed.</param>
        ''' <remarks>If customer is logged on user and they are in a specified group, then void their tax rate</remarks>
        Public Sub UpdateTaxRate(Optional ByRef cContactCountry As String = "")
            PerfMon.Log("Cart", "UpdateTaxRate")
            Dim sCountryList As String
            Dim aVatRates() As String
            Dim cVatRate As String
            Dim cVatExclusionGroup As String

            Dim nUpdateTaxRate As Double
            Dim nCurrentTaxRate As Double
            Dim bAllZero As Boolean

            Dim sSql As String
            Dim cProcessInfo As String = ""
            Try

                ' Store the current tax rate for comparison later
                nCurrentTaxRate = mnTaxRate
                nUpdateTaxRate = nCurrentTaxRate
                cVatExclusionGroup = "" & moCartConfig("TaxRateExclusionGroupId")

                ' First check if the user is in a tax exclusion group
                If IsNumeric(cVatExclusionGroup) _
                    AndAlso CInt(cVatExclusionGroup) > 0 _
                    AndAlso Tools.Xml.NodeState(myWeb.moPageXml.DocumentElement, "/Page/User/*[@id='" & cVatExclusionGroup & "']") Then
                    cProcessInfo = "User is in Tax Rate exclusion group"
                    nUpdateTaxRate = 0

                ElseIf cContactCountry <> "" Then
                    ' First get an iterative list of the location and its parents tax rates
                    sCountryList = getParentCountries(cContactCountry, 2)
                    cProcessInfo = "Get tax for:" & cContactCountry
                    If sCountryList = "" Then
                        bAllZero = True
                    Else

                        sCountryList = Mid(sCountryList, 3, Len(sCountryList) - 4)

                        aVatRates = Split(sCountryList, "','")
                        Array.Reverse(aVatRates)




                        ' go backwards through the list, and use the last non-zero tax rate
                        bAllZero = True

                        For Each cVatRate In aVatRates
                            If CDbl(cVatRate) > 0 Then
                                nUpdateTaxRate = CDbl(cVatRate)
                                bAllZero = False
                            End If
                        Next
                    End If
                    ' If all the countries are 0 then we need to set the tax rate accordingly
                    If bAllZero Then nUpdateTaxRate = 0
                End If


                If nUpdateTaxRate <> nCurrentTaxRate Then
                    ' return the (amended) rate to the mnTaxRate global variable.
                    mnTaxRate = nUpdateTaxRate
                    ' update the cart order table with the new tax rate
                    sSql = "update tblCartOrder set nTaxRate = " & mnTaxRate & " where nCartOrderKey=" & mnCartId
                    moDBHelper.ExeProcessSql(sSql)
                End If
                PerfMon.Log("Cart", "UpdateTaxEnd")
            Catch ex As Exception
                returnException(mcModuleName, "UpdateTaxRate", ex, "", cProcessInfo, gbDebug)

            End Try

        End Sub

        Public Sub populateCountriesDropDown(ByRef oXform As xForm, ByRef oCountriesDropDown As XmlElement, Optional ByVal cAddressType As String = "", Optional IDValues As Boolean = False)
            PerfMon.Log("Cart", "populateCountriesDropDown")
            Dim sSql As String
            Dim oDr As SqlDataReader
            Dim oLoctree As XmlElement
            Dim oLocation As XmlElement
            Dim arrPreLocs() As String = Split(mcPriorityCountries, ",")
            Dim arrIdx As Integer
            Dim bPreSelect As Boolean
            Dim cProcessInfo As String = "" = oCountriesDropDown.OuterXml
            Try

                Select Case cAddressType
                    Case "Delivery Address"
                        ' Delivery countries are restricted
                        ' Go and build a tree of all locations - this will allow us to detect whether or not a country is in iteself or in a zone that has a Shipping Option
                        oLoctree = moPageXml.CreateElement("Contents")
                        ListShippingLocations(oLoctree, False)

                        ' Add any priority countries
                        For arrIdx = 0 To UBound(arrPreLocs)
                            oLocation = oLoctree.SelectSingleNode("//TreeItem[@nameShort='" & arrPreLocs(arrIdx) & "']/ancestor-or-self::*[@nOptCount!='0']")
                            If Not (oLocation Is Nothing) Then
                                oXform.addOption((oCountriesDropDown), Trim(CStr(arrPreLocs(arrIdx))), Trim(CStr(arrPreLocs(arrIdx))))
                                bPreSelect = True
                            End If
                        Next

                        If bPreSelect Then
                            oXform.addOption((oCountriesDropDown), "--------", " ")
                        End If

                        ' Now let's go and get a list of all the COUNTRIES sorted ALPHABETICALLY
                        sSql = "SELECT DISTINCT cLocationNameShort FROM tblCartShippingLocations WHERE nLocationType = 2 ORDER BY cLocationNameShort"
                        oDr = moDBHelper.getDataReader(sSql)

                        While oDr.Read()
                            'Let's find the country node
                            ' XPath says "Get the context node for the location I'm looking at.  Does it or its ancestors have an OptCount > 0?

                            If Not (oLoctree.SelectSingleNode("//TreeItem[@nameShort=""" & oDr("cLocationNameShort") & """]/ancestor-or-self::*[@nOptCount!='0']") Is Nothing) Then
                                oXform.addOption((oCountriesDropDown), oDr("cLocationNameShort"), oDr("cLocationNameShort"))
                            End If
                        End While

                        oLoctree = Nothing
                        oLocation = Nothing
                        oDr.Close()
                        oDr = Nothing
                    Case Else
                        ' Not restricted by delivery address - add all countries.
                        For arrIdx = 0 To UBound(arrPreLocs)
                            oXform.addOption((oCountriesDropDown), Trim(CStr(arrPreLocs(arrIdx))), Trim(CStr(arrPreLocs(arrIdx))))
                            bPreSelect = True
                        Next

                        If bPreSelect Then
                            oXform.addOption((oCountriesDropDown), "--------", " ")
                        End If
                        If IDValues Then
                            sSql = "SELECT DISTINCT cLocationNameShort as name, nLocationKey as value FROM tblCartShippingLocations WHERE nLocationType = 2 ORDER BY cLocationNameShort"
                        Else
                            sSql = "SELECT DISTINCT cLocationNameShort as name, cLocationNameShort as value FROM tblCartShippingLocations WHERE nLocationType = 2 ORDER BY cLocationNameShort"

                        End If
                        oDr = moDBHelper.getDataReader(sSql)
                        oXform.addOptionsFromSqlDataReader(oCountriesDropDown, oDr)
                        'this closes the oDr too
                End Select

            Catch ex As Exception
                returnException(mcModuleName, "populateCountriesDropDown", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Function emailCart(ByRef oCartXML As XmlElement, ByVal xsltPath As String, ByVal fromName As String, ByVal fromEmail As String, ByVal recipientEmail As String, ByVal SubjectLine As String, Optional ByVal bEncrypt As Boolean = False, Optional ByVal cCustomerAttachementTemplatePath As String = "") As Object
            PerfMon.Log("Cart", "emailCart")
            Dim oXml As XmlDocument = New XmlDocument
            Dim cProcessInfo As String = "emailCart"
            Try
                'check file path

                Dim ofs As New Protean.fsHelper()

                oXml.LoadXml(oCartXML.OuterXml)
                xsltPath = ofs.checkCommonFilePath(moConfig("ProjectPath") & xsltPath)

                oCartXML.SetAttribute("lang", myWeb.mcPageLanguage)

                Dim oMsg As Messaging = New Messaging
                If cCustomerAttachementTemplatePath = "" Then

                    cProcessInfo = oMsg.emailer(oCartXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, "Message Sent", "Message Failed")
                Else
                    cCustomerAttachementTemplatePath = moServer.MapPath(cCustomerAttachementTemplatePath)
                    Dim cFontPath As String = moServer.MapPath("/fonts")
                    Dim oPDF As New Protean.Tools.PDF
                    oMsg.addAttachment(oPDF.GetPDFstream(oCartXML, cCustomerAttachementTemplatePath, cFontPath), "Attachment.pdf")
                    cProcessInfo = oMsg.emailer(oCartXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, "Message Sent", "Message Failed")

                End If
                oMsg = Nothing

                Return cProcessInfo

            Catch ex As Exception
                returnException(mcModuleName, "emailCart", ex, "", cProcessInfo, gbDebug)
                Return Nothing
            End Try

        End Function

        Public Sub DoNotesItem(ByVal cAction As String)
            PerfMon.Log("Cart", "DoNotesItem")
            Dim sSql As String
            Dim cNotes As String ' xml string value, gets reused
            Dim oNoteElmt As XmlElement
            Dim oNoteXML As New XmlDocument
            Dim oItem As Object
            Dim oAttribs(0) As FormResult
            Dim nQuantity As Integer = -1 'making it this number so we know 
            Dim cProcessInfo As String = "DoNotesLine"
            Dim i As Integer
            Dim cxPath As String = ""
            Dim oTmpElements As XmlElement
            Dim tmpDoc As New XmlDocument
            Try
                'if there is no active cart object (or quote) we need to make one
                If mnCartId = 0 Then
                    Dim otmpcart As New Cart(myWeb)

                    mnCartId = otmpcart.CreateNewCart(Nothing)
                End If
                'now we need to get the notes from the cart
                sSql = "Select cClientNotes from tblCartOrder where nCartOrderKey = " & mnCartId
                cNotes = moDBHelper.DBN2Str(moDBHelper.ExeProcessSqlScalar(sSql), False, False)

                'Check if it is empty
                If cNotes = "" Then
                    'we get the empty notes schema from the notes xForm instance.
                    Dim oXform As xForm = New xForm
                    oXform.moPageXML = moPageXml
                    oXform.NewFrm("notesForm")
                    If oXform.load(mcNotesXForm) Then
                        oNoteXML.LoadXml(oXform.Instance.InnerXml)
                    Else
                        'no notes xform is spcificed so create new notes node
                        oNoteElmt = oNoteXML.CreateElement("Notes")
                        oNoteXML.AppendChild(oNoteElmt)
                        oNoteElmt = oNoteXML.CreateElement("Notes")
                        oNoteXML.SelectSingleNode("Notes").AppendChild(oNoteElmt)
                    End If
                    oXform = Nothing

                Else
                    oNoteXML.InnerXml = cNotes
                End If

                cNotes = ""

                'now to get on and create our nodes
                For Each oItem In myWeb.moRequest.Form
                    If oItem = "node" Then
                        'this is the basic node text
                        cNotes = myWeb.moRequest.Form.Get(oItem)
                    Else
                        'this is the rest of the submitted form data
                        'going to be saved as attributes
                        ReDim Preserve oAttribs(UBound(oAttribs) + 1)
                        oAttribs(UBound(oAttribs) - 1) = New FormResult(oItem, myWeb.moRequest.Form.Get(oItem))
                    End If
                Next
                'creating a temporary xml document so we can turn the notes string
                'into actual xml
                tmpDoc.InnerXml = cNotes
                oNoteElmt = Nothing
                'now we can create an actual node in the main document with the right name
                oNoteElmt = oNoteXML.CreateElement(tmpDoc.ChildNodes(0).Name)
                'give it the same xml
                oNoteElmt.InnerXml = tmpDoc.ChildNodes(0).InnerXml


                'set the attributes
                'and create an xpath excluding the quantity field to see if we have an identical node
                Dim cIncludeList As String = myWeb.moRequest.Form("InputList")

                For i = 0 To UBound(oAttribs) - 1
                    If cIncludeList.Contains(oAttribs(i).Name) Or cIncludeList = "" Then
                        oNoteElmt.SetAttribute(oAttribs(i).Name, oAttribs(i).Value)
                        If Not oAttribs(i).Name = "qty" Then
                            If Not cxPath = "" Then cxPath &= " and "
                            cxPath &= "@" & oAttribs(i).Name & "='" & oAttribs(i).Value & "'"
                        Else
                            nQuantity = oAttribs(i).Value
                        End If
                    End If
                Next







                'If Not oAttribs(i).Name = "cartCmd" And Not oAttribs(i).Name = "quoteCmd" Then
                'finish of the xpath
                cxPath = "Notes/" & oNoteElmt.Name & "[" & cxPath & "]"
                '"addNoteLine", "removeNoteLine", "updateNoteLine"
                'loop through any basic matchest to see if it exisists already
                For Each oTmpElements In oNoteXML.SelectNodes("descendant-or-self::" & cxPath)
                    'we have a basic top level match
                    'does it match at a lower level
                    If oTmpElements.InnerXml = oNoteElmt.InnerXml Then
                        'ok, it matches, do we add to the quantity or remove it
                        If cAction = "removeNoteLine" Then
                            'its a remove
                            'check this
                            oNoteXML.SelectSingleNode("Notes").RemoveChild(oTmpElements)
                            'skip the addition of a notes item
                            GoTo SaveNotes
                        ElseIf cAction = "updateNoteLine" Then
                            'an update
                            oTmpElements.SetAttribute("qty", nQuantity)
                            ' Complete Bodge for keysource
                            Dim cVAs As String = myWeb.moRequest.Form("VA")
                            If cVAs Is Nothing Or cVAs = "" Then cVAs = myWeb.moRequest.Form("Custom_VARating")
                            Dim nTotalVA As Integer = 0
                            If IsNumeric(cVAs) Then
                                nTotalVA = CDec(oTmpElements.GetAttribute("qty")) * CDec(cVAs)
                                If nTotalVA > 0 Then oTmpElements.SetAttribute("TotalVA", nTotalVA)
                            End If
                            GoTo SaveNotes
                        ElseIf Not nQuantity = -1 Then
                            'its an Add
                            oTmpElements.SetAttribute("qty", CInt(oTmpElements.GetAttribute("qty")) + nQuantity)
                            ' Complete Bodge for keysource
                            Dim cVAs As String = myWeb.moRequest.Form("VA")
                            If cVAs Is Nothing Or cVAs = "" Then cVAs = myWeb.moRequest.Form("Custom_VARating")
                            Dim nTotalVA As Integer = 0
                            If IsNumeric(cVAs) Then
                                nTotalVA = CDec(oTmpElements.GetAttribute("qty")) * CDec(cVAs)
                                If nTotalVA > 0 Then oTmpElements.SetAttribute("TotalVA", nTotalVA)
                            End If
                            'skip the addition of a notes item
                            GoTo SaveNotes
                        Else
                            'dont do anything, there is no quantity involved so we just add the item anyway
                        End If
                        'there should only be one exact match so no need to go through the rest
                        Exit For
                    End If
                Next


                ' Complete Bodge for keysource
                Dim cVAsN As String = myWeb.moRequest.Form("VA")
                If cVAsN Is Nothing Or cVAsN = "" Then cVAsN = myWeb.moRequest.Form("Custom_VARating")
                Dim nTotalVAN As Integer = 0
                If IsNumeric(cVAsN) Then
                    nTotalVAN = nQuantity * CDec(cVAsN)
                    If nTotalVAN > 0 Then oNoteElmt.SetAttribute("TotalVA", nTotalVAN)
                End If


                'we only actually hit this if there is no exact match

                oNoteXML.DocumentElement.InsertAfter(oNoteElmt, oNoteXML.DocumentElement.LastChild)

SaveNotes:      ' this is so we can skip the appending of new node
                'now we just need to update the cart notes and the other 
                'procedures will do the rest
                sSql = "UPDATE tblCartOrder SET  cClientNotes = '" & oNoteXML.OuterXml & "' WHERE (nCartOrderKey = " & mnCartId & ")"
                moDBHelper.ExeProcessSql(sSql)
            Catch ex As Exception
                returnException(mcModuleName, "addItems", ex, "", cProcessInfo, gbDebug)
            End Try
        End Sub

        Public Sub ListOrders(Optional ByVal nOrderID As Integer = 0, Optional ByVal bListAllQuotes As Boolean = False, Optional ByVal ProcessId As Integer = 0, Optional ByRef oPageDetail As XmlElement = Nothing, Optional ByVal bForceRefresh As Boolean = False, Optional nUserId As Long = 0)
            PerfMon.Log("Cart", "ListOrders")
            If myWeb.mnUserId = 0 Then Exit Sub ' if not logged in, dont bother
            'For listing a users previous orders/quotes

            Dim oDs As New DataSet
            Dim cSQL As String
            Dim cWhereSQL As String = ""

            ' Paging variables
            Dim nStart As Integer = 0
            Dim nRows As Integer = 100

            Dim nCurrentRow As Integer = 0
            Dim moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")

            Try

                ' Set the paging variables, if provided.
                If Not (myWeb.moRequest("startPos") Is Nothing) AndAlso IsNumeric(myWeb.moRequest("startPos")) Then nStart = CInt(myWeb.moRequest("startPos"))
                If Not (myWeb.moRequest("rows") Is Nothing) AndAlso IsNumeric(myWeb.moRequest("rows")) Then nRows = CInt(myWeb.moRequest("rows"))

                If nStart < 0 Then nStart = 0
                If nRows < 1 Then nRows = 100

                If Not nUserId = 0 Then
                    cWhereSQL = " WHERE nCartUserDirId = " & nUserId & IIf(nOrderID > 0, " AND nCartOrderKey = " & nOrderID, "") & " AND cCartSchemaName = '" & mcOrderType & "'"
                ElseIf Not myWeb.mbAdminMode Then
                    cWhereSQL = " WHERE nCartUserDirId = " & myWeb.mnUserId & IIf(nOrderID > 0, " AND nCartOrderKey = " & nOrderID, "") & " AND cCartSchemaName = '" & mcOrderType & "'"
                Else
                    cWhereSQL = " WHERE " & IIf(nOrderID > 0, "  nCartOrderKey = " & nOrderID & " AND ", "") & " cCartSchemaName = '" & mcOrderType & "' "
                    'if nCartStatus = " & ProcessId
                    If Not ProcessId = 0 Then
                        cWhereSQL &= " and nCartStatus = " & ProcessId
                    End If
                End If

                ' Quick call to get the total number of records
                cSQL = "SELECT COUNT(*) As Count FROM tblCartOrder " & cWhereSQL
                Dim nTotal As Long = moDBHelper.GetDataValue(cSQL, , , 0)

                If nTotal > 0 Then

                    ' Initial paging option is limit the the rows returned
                    cSQL = "SELECT TOP " & (nStart + nRows) & " * FROM tblCartOrder "
                    cSQL &= cWhereSQL & " ORDER BY nCartOrderKey Desc"

                    oDs = moDBHelper.GetDataSet(cSQL, mcOrderType, mcOrderType & "List")

                    Dim oDR As DataRow

                    If oDs.Tables.Count > 0 Then


                        Dim oContentDetails As XmlElement
                        'Get the content Detail element
                        If oPageDetail Is Nothing Then
                            oContentDetails = moPageXml.SelectSingleNode("Page/ContentDetail")
                            If oContentDetails Is Nothing Then
                                oContentDetails = moPageXml.CreateElement("ContentDetail")
                                If Not moPageXml.InnerXml = "" Then
                                    moPageXml.FirstChild.AppendChild(oContentDetails)
                                Else
                                    oPageDetail.AppendChild(oContentDetails)
                                End If

                            End If
                        Else
                            oContentDetails = oPageDetail
                        End If

                        oContentDetails.SetAttribute("start", nStart)
                        oContentDetails.SetAttribute("total", nTotal)
                        Dim bSingleRecord As Boolean = False
                        If oDs.Tables(mcOrderType).Rows.Count = 1 Then bSingleRecord = True

                        'go through each cart
                        For Each oDR In oDs.Tables(mcOrderType).Rows
                            ' Only add the relevant rows (page selected)
                            nCurrentRow += 1
                            If nCurrentRow > nStart Then
                                Dim oContent As XmlElement = moPageXml.CreateElement("Content")
                                oContent.SetAttribute("type", mcOrderType)
                                oContent.SetAttribute("id", oDR("nCartOrderKey"))
                                oContent.SetAttribute("statusId", oDR("nCartStatus"))

                                'Get Date
                                cSQL = "Select dInsertDate from tblAudit where nAuditKey =" & oDR("nAuditId")
                                Dim oDRe As SqlDataReader = moDBHelper.getDataReader(cSQL)
                                Do While oDRe.Read
                                    oContent.SetAttribute("created", xmlDateTime(oDRe.GetValue(0)))
                                Loop
                                oDRe.Close()

                                'Get stored CartXML
                                If (Not oDR("cCartXML") = "") And bForceRefresh = False Then
                                    oContent.InnerXml = oDR("cCartXML")
                                    Dim oCartElmt As XmlElement = oContent.FirstChild

                                    'check for invoice date etc.
                                    If CLng(oCartElmt.GetAttribute("statusId")) >= 6 And oCartElmt.GetAttribute("InvoiceDate") = "" Then
                                        'fix for any items that have lost the invoice date and ref.
                                        Dim cartId As Long = oDR("nCartOrderKey")
                                        Dim insertDate As String = moDBHelper.ExeProcessSqlScalar("SELECT a.dInsertDate FROM tblCartOrder inner join tblAudit a on nAuditId = nAuditKey where nCartOrderKey = " & cartId)
                                        addDateAndRef(oCartElmt, insertDate, cartId)
                                        SaveCartXML(oCartElmt, cartId)
                                    End If

                                End If

                                If bForceRefresh Then
                                    Dim oCartListElmt As XmlElement = moPageXml.CreateElement("Order")
                                    Me.GetCart(oCartListElmt, oDR("nCartOrderKey"))
                                    oContent.InnerXml = oCartListElmt.OuterXml
                                End If

                                Dim orderNode As XmlElement = oContent.FirstChild
                                'Add values not stored in cartXml
                                If Not orderNode Is Nothing Then
                                    orderNode.SetAttribute("statusId", oDR("nCartStatus"))
                                End If
                                If oDR("cCurrency") Is Nothing Or oDR("cCurrency") = "" Then
                                    oContent.SetAttribute("currency", mcCurrency)
                                    oContent.SetAttribute("currencySymbol", mcCurrencySymbol)
                                Else
                                    oContent.SetAttribute("currency", oDR("cCurrency"))
                                    Dim thisCurrencyNode As XmlElement = moPaymentCfg.SelectSingleNode("currencies/Currency[@ref='" & oDR("cCurrency") & "']")
                                    If Not thisCurrencyNode Is Nothing Then
                                        oContent.SetAttribute("currencySymbol", thisCurrencyNode.GetAttribute("symbol"))
                                    Else
                                        oContent.SetAttribute("currencySymbol", mcCurrencySymbol)
                                    End If
                                End If
                                oContent.SetAttribute("type", LCase(mcOrderType))

                                'oContent.SetAttribute("currency", mcCurrency)
                                'oContent.SetAttribute("currencySymbol", mcCurrencySymbol)

                                If oDR("nCartUserDirId") <> 0 Then
                                    oContent.SetAttribute("userId", oDR("nCartUserDirId"))
                                End If

                                'TS: Removed because it gives a massive overhead when Listing loads of orders.
                                If bSingleRecord Then
                                    If myWeb.mbAdminMode And CInt("0" & oDR("nCartUserDirId")) > 0 Then
                                        oContent.AppendChild(moDBHelper.GetUserXML(CInt(oDR("nCartUserDirId")), False))
                                    End If

                                    Dim aSellerNotes As String() = Split(oDR("cSellerNotes"), "/n")
                                    Dim cSellerNotesHtml As String = "<ul>"
                                    For snCount As Integer = 0 To UBound(aSellerNotes)
                                        cSellerNotesHtml = cSellerNotesHtml + "<li>" + Protean.Tools.Xml.convertEntitiesToCodes(aSellerNotes(snCount)) + "</li>"
                                    Next
                                    Dim sellerNode As XmlElement = addNewTextNode("SellerNotes", oContent.FirstChild, "")
                                    sellerNode.InnerXml = cSellerNotesHtml + "</ul>"

                                    'Add the Delivery Details
                                    'Add Delivery Details
                                    If oDR("nCartStatus") = 9 Then
                                        Dim sSql As String = "Select * from tblCartOrderDelivery where nOrderId=" & oDR("nCartOrderKey")
                                        Dim oDs2 As DataSet = moDBHelper.GetDataSet(sSql, "Delivery", "Details")
                                        For Each oRow2 As DataRow In oDs2.Tables("Delivery").Rows
                                            Dim oElmt As XmlElement = moPageXml.CreateElement("DeliveryDetails")
                                            oElmt.SetAttribute("carrierName", oRow2("cCarrierName"))
                                            oElmt.SetAttribute("ref", oRow2("cCarrierRef"))
                                            oElmt.SetAttribute("notes", oRow2("cCarrierNotes"))
                                            oElmt.SetAttribute("deliveryDate", xmlDate(oRow2("dExpectedDeliveryDate")))
                                            oElmt.SetAttribute("collectionDate", xmlDate(oRow2("dCollectionDate")))
                                            oContent.AppendChild(oElmt)
                                        Next
                                    End If

                                End If

                                Dim oTestNode As XmlElement = oContentDetails.SelectSingleNode("Content[@id=" & oContent.GetAttribute("id") & " and @type='" & LCase(mcOrderType) & "']")
                                If mcOrderType = "Cart" Or mcOrderType = "Order" Then
                                    'If (Not oContent.FirstChild.Attributes("itemCount").Value = 0) And oTestNode Is Nothing Then

                                    oContentDetails.AppendChild(oContent)

                                    'End If
                                Else
                                    If oTestNode Is Nothing Then
                                        If bListAllQuotes Then
                                            oContentDetails.AppendChild(oContent)
                                        Else
                                            '   If (Not oContent.FirstChild.Attributes("itemCount").Value = 0) Then
                                            oContentDetails.AppendChild(oContent)
                                            '    End If
                                        End If
                                    End If
                                End If



                                'If (Not oContent.FirstChild.Attributes("itemCount").Value = 0) And oTestNode Is Nothing Then
                                '    oContentDetails.AppendChild(oContent)
                                'End If
                            End If
                        Next
                    End If
                End If
            Catch ex As Exception
                returnException(mcModuleName, "ListOrders", ex, "", "", gbDebug)
            End Try
        End Sub


        Public Overridable Sub MakeCurrent(ByVal nOrderID As Integer)
            PerfMon.Log("Cart", "MakeCurrent")
            'procedure to make a selected historical
            'order or quote into the currently active one

            Dim oDS As New DataSet
            Dim otmpcart As Cart = Nothing
            Try


                If myWeb.mnUserId = 0 Then Exit Sub
                If Not moDBHelper.ExeProcessSqlScalar("Select nCartUserDirId FROM tblCartOrder WHERE nCartOrderKey = " & nOrderID) = mnEwUserId Then
                    Exit Sub 'else we carry on
                End If
                If mnCartId = 0 Then
                    'create a new cart
                    otmpcart = New Cart(myWeb)

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
                If Not otmpcart Is Nothing Then
                    otmpcart.mnProcessId = 1
                    otmpcart.mcCartCmd = "Cart"
                    otmpcart.apply()
                End If

                'now we need to redirect somewhere?
                'bRedirect = True
                myWeb.moResponse.Redirect("?cartCmd=Cart", False)
                myWeb.moCtx.ApplicationInstance.CompleteRequest()
            Catch ex As Exception
                returnException(mcModuleName, "MakeCurrent", ex, "", "", gbDebug)
            End Try
        End Sub

        Public Function DeleteCart(ByVal nOrderID As Integer) As Boolean
            PerfMon.Log("Cart", "DeleteCart")
            If myWeb.mnUserId = 0 Then Exit Function
            If nOrderID <= 0 Then Exit Function
            Try
                clearSessionCookie()

                Dim cSQL As String = "Select nCartStatus, nCartUserDirId from tblCartOrder WHERE nCartOrderKey=" & nOrderID
                Dim oDR As SqlDataReader = moDBHelper.getDataReader(cSQL)
                Dim nStat As Integer
                Dim nOwner As Integer
                Do While oDR.Read
                    nStat = oDR.GetValue(0)
                    nOwner = oDR.GetValue(1)
                Loop
                oDR.Close()
                'If (nOwner = myWeb.mnUserId And (nStat = 7 Or nStat < 4)) Then moDBHelper.DeleteObject(dbHelper.objectTypes.CartOrder, nOrderID)
                If nOwner = myWeb.mnUserId Then
                    moDBHelper.DeleteObject(dbHelper.objectTypes.CartOrder, nOrderID)
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                returnException(mcModuleName, "Delete Cart", ex, "", "", gbDebug)
            End Try
        End Function

        Public Sub SaveCartXML(ByVal cartXML As XmlElement, Optional nCartId As Long = 0)
            PerfMon.Log("Cart", "SaveCartXML")
            If nCartId = 0 Then nCartId = mnCartId
            Try
                If nCartId > 0 Then
                    Dim sSQL As String = "Update tblCartOrder SET cCartXML ='" & SqlFmt(cartXML.OuterXml.ToString) & "' WHERE nCartOrderKey = " & nCartId
                    moDBHelper.ExeProcessSql(sSQL)
                End If

            Catch ex As Exception
                returnException(mcModuleName, "SaveCartXML", ex, "", "", gbDebug)
            End Try
        End Sub




        ''' <summary>
        ''' Select currency deals with the workflow around choosing a currency when an item is added to the cart.
        ''' </summary>
        ''' <returns></returns>
        Public Function SelectCurrency() As Boolean
            If Not PerfMon Is Nothing Then PerfMon.Log("Cart", "SelectCurrency")
            Dim cProcessInfo As String = ""
            Try
                cProcessInfo = "checking of Override"
                'if we are not looking to switch the currency then
                'we just check if there are any available
                Dim cOverrideCur As String
                cOverrideCur = myWeb.moRequest("Currency")
                If Not cOverrideCur Is Nothing And Not cOverrideCur = "" Then
                    cProcessInfo = "Using Override"
                    mcCurrencyRef = cOverrideCur
                    GetCurrencyDefinition()
                    myWeb.moSession("bCurrencySelected") = True
                    If mnProcessId >= 1 Then
                        If mnShippingRootId > 0 Then
                            mnProcessId = 2
                            mcCartCmd = "Delivery"
                        Else
                            mcCartCmd = "Cart"
                        End If
                    Else
                        mcCartCmd = ""
                    End If
                    Return True
                Else
                    cProcessInfo = "Check to see if already used"
                    If myWeb.moSession("bCurrencySelected") = True Then
                        If mcCartCmd = "Currency" Then
                            mcCartCmd = "Cart"
                        Else
                            mcCartCmd = mcCartCmd
                        End If

                        Return True
                    End If
                End If

                cProcessInfo = "Getting Currencies"
                Dim moPaymentCfg As XmlNode
                moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")
                If moPaymentCfg Is Nothing Then
                    cProcessInfo = "protean/payment Config node is missing"
                    mcCartCmd = "Cart"
                    Return True
                End If

                'check we have differenct currencies
                If moPaymentCfg.SelectSingleNode("currencies/Currency") Is Nothing Then
                    '  mcCartCmd = "Cart"
                    myWeb.moSession("bCurrencySelected") = True
                    Return True
                Else
                    Dim oCurrencies As XmlNodeList = moPaymentCfg.SelectNodes("currencies/Currency")

                    If oCurrencies.Count = 1 Then
                        Dim oCurrency As XmlElement = oCurrencies(0)
                        myWeb.moSession("cCurrency") = oCurrency.GetAttribute("ref")

                        'here we need to re-get the cart stuff in the new currency
                        GetCurrencyDefinition()
                        ' mcCartCmd = "Cart"
                        myWeb.moSession("bCurrencySelected") = True
                        Return True
                    End If
                    oCurrencies = Nothing
                    'If multiple currencies then we need to pick
                End If

                'If Not bOverride And Not mcCurrencyRef = "" Then Return True

                If mcCartCmd = "Currency" Then
                    'And mcCurrencyRef = "" Then Return True
                    'get and load the currency selector xform
                    cProcessInfo = "Supplying and Checking Form"
                    Dim oCForm As New xForm(myWeb)
                    oCForm.NewFrm()
                    oCForm.load("/ewcommon/xforms/cart/CurrencySelector.xml")
                    Dim oInputElmt As XmlElement = oCForm.moXformElmt.SelectSingleNode("group/group/group/select1[@bind='cRef']")
                    Dim oCur As XmlElement = oCForm.Instance.SelectSingleNode("Currency/ref")
                    oCur.InnerText = mcCurrency
                    Dim oCurrencyElmt As XmlElement
                    For Each oCurrencyElmt In moPaymentCfg.SelectNodes("currencies/Currency")
                        'going to need to do something about languages
                        Dim oOptionElmt As XmlElement
                        oOptionElmt = oCForm.addOption(oInputElmt, oCurrencyElmt.SelectSingleNode("name").InnerText, oCurrencyElmt.GetAttribute("ref"))
                        oCForm.addNote(oOptionElmt, xForm.noteTypes.Hint, oCurrencyElmt.SelectSingleNode("description").InnerText)
                    Next
                    If oCForm.isSubmitted Then
                        oCForm.updateInstanceFromRequest()
                        oCForm.addValues()
                        oCForm.validate()
                        If oCForm.valid Then
                            mcCurrencyRef = oCForm.Instance.SelectSingleNode("Currency/ref").InnerText
                            myWeb.moSession("cCurrency") = mcCurrencyRef
                            'here we need to re-get the cart stuff in the new currency
                            GetCurrencyDefinition()
                            myWeb.moSession("bCurrencySelected") = True
                            mcCartCmd = "Cart"
                            Return True
                        End If
                    End If
                    oCForm.addValues()
                    If Not moPageXml.SelectSingleNode("/Page/Contents") Is Nothing Then
                        moPageXml.SelectSingleNode("/Page/Contents").AppendChild(moPageXml.ImportNode(oCForm.moXformElmt.CloneNode(True), True))
                    End If
                    mcCartCmd = "Currency"
                    Return False
                End If


            Catch ex As Exception
                returnException(mcModuleName, "SelectCurrency", ex, "", cProcessInfo, gbDebug)
            End Try

        End Function

        ''' <summary>
        ''' takes the mcCurrencyRef and sets the currency for the current order 
        ''' </summary>
        Public Sub GetCurrencyDefinition()
            If Not PerfMon Is Nothing Then PerfMon.Log("Cart", "GetCurrencyDefinition")
            Dim cProcessInfo As String = ""
            Try
                If Not mcCurrencyRef = "" And Not mcCurrencyRef Is Nothing Then

                    Dim moPaymentCfg As XmlNode

                    moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("protean/payment")

                    Dim oCurrency As XmlElement = moPaymentCfg.SelectSingleNode("currencies/Currency[@ref='" & mcCurrencyRef & "']")
                    If oCurrency Is Nothing Then Exit Sub

                    mcCurrencySymbol = oCurrency.GetAttribute("symbol")

                    mcCurrencyCode = oCurrency.GetAttribute("code")

                    mnShippingRootId = -1

                    If IsNumeric(oCurrency.GetAttribute("ShippingRootId")) Then
                        mnShippingRootId = CInt(oCurrency.GetAttribute("ShippingRootId"))
                    End If

                    mcCurrency = mcCurrencyCode

                    myWeb.moSession("cCurrency") = mcCurrencyRef
                    myWeb.moSession("mcCurrency") = mcCurrency
                    'now update the cart database row
                    Dim sSQL As String = "UPDATE tblCartOrder  SET cCurrency = '" & mcCurrency & "' WHERE nCartOrderKey = " & mnCartId

                    myWeb.moDbHelper.ExeProcessSqlScalar(sSQL)

                End If
            Catch ex As Exception
                returnException(mcModuleName, "GetCurrencyDefinition", ex, , cProcessInfo, gbDebug)
            End Try
        End Sub


        Public Function CartOverview() As XmlElement

            Try
                Dim oRptElmt As XmlElement = myWeb.moPageXml.CreateElement("CartOverview")



                Return oRptElmt

            Catch ex As Exception
                returnException(mcModuleName, "CartOverview", ex, , "", gbDebug)
                Return Nothing
            End Try

        End Function

        Public Function CartReportsDownload(ByVal dBegin As Date, ByVal dEnd As Date, ByVal cCurrencySymbol As String, ByVal cOrderType As String, ByVal nOrderStage As Integer, Optional ByVal updateDatesWithStartAndEndTimes As Boolean = True) As XmlElement
            Try
                Dim cSQL As String = "exec "
                Dim cCustomParam As String = ""
                Dim cReportType As String = "CartDownload"
                Dim oElmt As XmlElement

                ' Set the times for each date
                If updateDatesWithStartAndEndTimes Then
                    dBegin = dBegin.Date
                    dEnd = dEnd.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
                End If


                cSQL &= moDBHelper.getDBObjectNameWithBespokeCheck("spOrderDownload") & " "
                cSQL &= Protean.Tools.Database.SqlDate(dBegin, True) & ","
                cSQL &= Protean.Tools.Database.SqlDate(dEnd, True) & ","
                cSQL &= "'" & cOrderType & "',"
                cSQL &= nOrderStage

                Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report")

                If oDS.Tables("Item").Columns.Contains("cCartXML") Then
                    oDS.Tables("Item").Columns("cCartXML").ColumnMapping = MappingType.Element
                End If
                Dim oRptElmt As XmlElement = myWeb.moPageXml.CreateElement("Content")
                oRptElmt.SetAttribute("type", "Report")
                oRptElmt.SetAttribute("name", "CartDownloads")
                'NB editing this line to add in &'s
                oRptElmt.InnerXml = oDS.GetXml
                For Each oElmt In oRptElmt.SelectNodes("Report/Item/cCartXml")
                    oElmt.InnerXml = oElmt.InnerText
                Next

                'oRptElmt.InnerXml = Replace(Replace(Replace(Replace(oDS.GetXml, "&amp;", "&"), "&gt;", ">"), "&lt;", "<"), " xmlns=""""", "")
                'oRptElmt.InnerXml = Replace(Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<"), " xmlns=""""", "")
                Dim oReturnElmt As XmlElement = oRptElmt.FirstChild
                oReturnElmt.SetAttribute("cReportType", cReportType)
                oReturnElmt.SetAttribute("dBegin", dBegin)
                oReturnElmt.SetAttribute("dEnd", dEnd)
                oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol)
                oReturnElmt.SetAttribute("cOrderType", cOrderType)
                oReturnElmt.SetAttribute("nOrderStage", nOrderStage)

                Return oRptElmt
            Catch ex As Exception
                returnException(mcModuleName, "CartReports", ex, , "", gbDebug)
                Return Nothing
            End Try
        End Function

        Public Function CartReports(ByVal dBegin As Date, ByVal dEnd As Date, Optional ByVal bSplit As Integer = 0, Optional ByVal cProductType As String = "", Optional ByVal nProductId As Integer = 0, Optional ByVal cCurrencySymbol As String = "", Optional ByVal nOrderStatus1 As Integer = 6, Optional ByVal nOrderStatus2 As Integer = 9, Optional ByVal cOrderType As String = "ORDER") As XmlElement
            Try
                Dim cSQL As String = "exec "
                Dim cCustomParam As String = ""
                Dim cReportType As String = ""
                If nProductId > 0 Then
                    'Low Level
                    cSQL &= "spCartActivityLowLevel"
                    cCustomParam = nProductId
                    cReportType = "Item Totals"
                ElseIf Not cProductType = "" Then
                    'Med Level
                    cSQL &= "spCartActivityMedLevel"
                    cCustomParam = "'" & cProductType & "'"
                    cReportType = "Type Totals"
                Else
                    'HighLevel
                    cSQL &= "spCartActivityTopLevel"
                    cCustomParam = bSplit
                    cReportType = "All Totals"
                End If
                cSQL &= Protean.Tools.Database.SqlDate(dBegin) & ","
                cSQL &= Protean.Tools.Database.SqlDate(dEnd) & ","
                cSQL &= cCustomParam & ","
                cSQL &= "'" & cCurrencySymbol & "',"
                cSQL &= nOrderStatus1 & ","
                cSQL &= nOrderStatus2 & ","
                cSQL &= "'" & cOrderType & "'"

                Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report")

                If oDS.Tables("Item").Columns.Contains("cCartXML") Then
                    oDS.Tables("Item").Columns("cCartXML").ColumnMapping = MappingType.Element
                End If
                Dim oRptElmt As XmlElement = myWeb.moPageXml.CreateElement("Content")
                oRptElmt.SetAttribute("type", "Report")
                oRptElmt.SetAttribute("name", "Cart Activity")
                oRptElmt.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                Dim oReturnElmt As XmlElement = oRptElmt.FirstChild
                oReturnElmt.SetAttribute("cReportType", cReportType)

                oReturnElmt.SetAttribute("dBegin", dBegin)
                oReturnElmt.SetAttribute("dEnd", dEnd)
                oReturnElmt.SetAttribute("bSplit", IIf(bSplit, 1, 0))
                oReturnElmt.SetAttribute("cProductType", cProductType)
                oReturnElmt.SetAttribute("nProductId", nProductId)
                oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol)
                oReturnElmt.SetAttribute("nOrderStatus1", nOrderStatus1)
                oReturnElmt.SetAttribute("nOrderStatus2", nOrderStatus2)
                oReturnElmt.SetAttribute("cOrderType", cOrderType)
                Return oRptElmt
            Catch ex As Exception
                returnException(mcModuleName, "CartReports", ex, , "", gbDebug)
                Return Nothing
            End Try
        End Function

        Public Function CartReportsDrilldown(Optional ByVal cGrouping As String = "Page", Optional ByVal nYear As Integer = 0, Optional ByVal nMonth As Integer = 0, Optional ByVal nDay As Integer = 0, Optional ByVal cCurrencySymbol As String = "", Optional ByVal nOrderStatus1 As Integer = 6, Optional ByVal nOrderStatus2 As Integer = 9, Optional ByVal cOrderType As String = "ORDER") As XmlElement
            Try
                Dim cSQL As String = "exec spCartActivityGroupsPages "
                Dim bPage As Boolean = False
                Dim cReportType As String = ""

                cSQL &= "'" & cGrouping & "',"
                cSQL &= nYear & ","
                cSQL &= nMonth & ","
                cSQL &= nDay & ","
                cSQL &= "'" & cCurrencySymbol & "',"
                cSQL &= nOrderStatus1 & ","
                cSQL &= nOrderStatus2 & ","
                cSQL &= "'" & cOrderType & "'"

                Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report")
                'For Grouped by page is going to be bloody hard
                If oDS.Tables("Item").Columns.Contains("nStructId") Then
                    bPage = True
                    cSQL = "EXEC getContentStructure @userId=" & myWeb.mnUserId & ", @bAdminMode=1, @dateNow=" & Protean.Tools.Database.SqlDate(Now) & ", @authUsersGrp = " & gnAuthUsers
                    myWeb.moDbHelper.addTableToDataSet(oDS, cSQL, "MenuItem")
                    'oDS.Tables("MenuItem").Columns.Add(New DataColumn("PageQuantity", GetType(Double)))
                    'oDS.Tables("MenuItem").Columns.Add(New DataColumn("PageCost", GetType(Double)))
                    'oDS.Tables("MenuItem").Columns.Add(New DataColumn("DecendantQuantity", GetType(Double)))
                    'oDS.Tables("MenuItem").Columns.Add(New DataColumn("DecendantCost", GetType(Double)))
                    For Each oDC As DataColumn In oDS.Tables("MenuItem").Columns
                        Dim cValid As String = "id,parid,name" ',PageQuantity,PageCost,DecendantQuantity,DecendantCost"
                        If Not cValid.Contains(oDC.ColumnName) Then
                            oDC.ColumnMapping = MappingType.Hidden
                        Else
                            oDC.ColumnMapping = MappingType.Attribute
                        End If
                    Next
                    For Each oDC As DataColumn In oDS.Tables("Item").Columns
                        oDC.ColumnMapping = MappingType.Attribute
                    Next
                    oDS.Relations.Add("Rel01", oDS.Tables("MenuItem").Columns("id"), oDS.Tables("MenuItem").Columns("parId"), False)
                    oDS.Relations("Rel01").Nested = True
                    oDS.Relations.Add(New DataRelation("Rel02", oDS.Tables("MenuItem").Columns("id"), oDS.Tables("Item").Columns("nStructId"), False))
                    oDS.Relations("Rel02").Nested = True
                End If



                Dim oRptElmt As XmlElement = myWeb.moPageXml.CreateElement("Content")
                oRptElmt.SetAttribute("type", "Report")
                oRptElmt.SetAttribute("name", "Cart Activity")
                oRptElmt.InnerXml = oDS.GetXml
                Dim oReturnElmt As XmlElement = oRptElmt.FirstChild
                oReturnElmt.SetAttribute("cReportType", cReportType)

                oReturnElmt.SetAttribute("nYear", nYear)
                oReturnElmt.SetAttribute("nMonth", nMonth)
                oReturnElmt.SetAttribute("nDay", nDay)
                oReturnElmt.SetAttribute("cGrouping", cGrouping)
                oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol)
                oReturnElmt.SetAttribute("nOrderStatus1", nOrderStatus1)
                oReturnElmt.SetAttribute("nOrderStatus2", nOrderStatus2)
                oReturnElmt.SetAttribute("cOrderType", cOrderType)

                If bPage Then
                    'Page Totals
                    For Each oElmt As XmlElement In oReturnElmt.SelectNodes("descendant-or-self::MenuItem")
                        Dim nQ As Integer = 0
                        Dim nC As Double = 0
                        For Each oItemElmt As XmlElement In oElmt.SelectNodes("Item")
                            nQ += oItemElmt.GetAttribute("nQuantity")
                            nC += oItemElmt.GetAttribute("nLinePrice")
                        Next
                        oElmt.SetAttribute("PageQuantity", nQ)
                        oElmt.SetAttribute("PageCost", nC)
                    Next
                    'loop through each node and then each item and see how many of each
                    'item it has and its decendants
                    For Each oElmt As XmlElement In oReturnElmt.SelectNodes("descendant-or-self::MenuItem")
                        Dim oHN As New Hashtable 'Number found
                        Dim oHQ As New Hashtable 'Quantity
                        Dim oHC As New Hashtable 'Cost
                        For Each oItem As XmlElement In oElmt.SelectNodes("descendant-or-self::Item")
                            Dim cKey As String = "I" & oItem.GetAttribute("nCartItemKey")
                            If Not oHN.ContainsKey(cKey) Then
                                oHN.Add(cKey, 1)
                                oHQ.Add(cKey, oItem.GetAttribute("nQuantity"))
                                oHC.Add(cKey, oItem.GetAttribute("nLinePrice"))
                            Else
                                oHN(cKey) += 1
                            End If
                        Next
                        Dim nQ As Integer = 0
                        Dim nC As Double = 0
                        For Each ci As String In oHN.Keys
                            nQ += oHQ.Item(ci)
                            nC += oHC.Item(ci)
                        Next
                        oElmt.SetAttribute("PageAndDescendantQuantity", nQ)
                        oElmt.SetAttribute("PageAndDescendantCost", nC)
                    Next



                End If


                If bPage And gnTopLevel > 0 Then
                    Dim oElmt As XmlElement = oRptElmt.SelectSingleNode("descendant-or-self::MenuItem[@id=" & gnTopLevel & "]")
                    If Not oElmt Is Nothing Then
                        oRptElmt.FirstChild.InnerXml = oElmt.OuterXml
                    Else
                        oRptElmt.FirstChild.InnerXml = ""
                    End If
                End If

                Return oRptElmt
            Catch ex As Exception
                returnException(mcModuleName, "CartReports", ex, , "", gbDebug)
                Return Nothing
            End Try
        End Function

        Public Function CartReportsPeriod(Optional ByVal cGroup As String = "Month", Optional ByVal nYear As Integer = 0, Optional ByVal nMonth As Integer = 0, Optional ByVal nWeek As Integer = 0, Optional ByVal cCurrencySymbol As String = "", Optional ByVal nOrderStatus1 As Integer = 6, Optional ByVal nOrderStatus2 As Integer = 9, Optional ByVal cOrderType As String = "ORDER") As XmlElement
            Try
                Dim cSQL As String = "exec spCartActivityPagesPeriod "
                Dim bPage As Boolean = False
                Dim cReportType As String = ""
                If nYear = 0 Then nYear = Now.Year
                cSQL &= "@Group='" & cGroup & "'"
                cSQL &= ",@nYear=" & nYear
                cSQL &= ",@nMonth=" & nMonth
                cSQL &= ",@nWeek=" & nWeek
                cSQL &= ",@cCurrencySymbol='" & cCurrencySymbol & "'"
                cSQL &= ",@nOrderStatus1=" & nOrderStatus1
                cSQL &= ",@nOrderStatus2=" & nOrderStatus2
                cSQL &= ",@cOrderType='" & cOrderType & "'"

                Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "Report")
                'For Grouped by page is going to be bloody hard




                Dim oRptElmt As XmlElement = myWeb.moPageXml.CreateElement("Content")
                oRptElmt.SetAttribute("type", "Report")
                oRptElmt.SetAttribute("name", "Cart Activity")
                oRptElmt.InnerXml = oDS.GetXml
                Dim oReturnElmt As XmlElement = oRptElmt.FirstChild
                oReturnElmt.SetAttribute("cReportType", cReportType)

                oReturnElmt.SetAttribute("nYear", nYear)
                oReturnElmt.SetAttribute("nMonth", nMonth)
                oReturnElmt.SetAttribute("nWeek", nWeek)
                oReturnElmt.SetAttribute("cGroup", cGroup)
                oReturnElmt.SetAttribute("cCurrencySymbol", cCurrencySymbol)
                oReturnElmt.SetAttribute("nOrderStatus1", nOrderStatus1)
                oReturnElmt.SetAttribute("nOrderStatus2", nOrderStatus2)
                oReturnElmt.SetAttribute("cOrderType", cOrderType)



                Return oRptElmt
            Catch ex As Exception
                returnException(mcModuleName, "CartReports", ex, , "", gbDebug)
                Return Nothing
            End Try
        End Function


        Public Sub AddShippingCosts(ByRef xmlProduct As XmlElement, ByVal nPrice As String, ByVal nWeight As String)
            Try

                Dim nQuantity As Long = 1

                xmlProduct.SetAttribute("test1", "1")

                Dim xmlShippingOptions As XmlElement = makeShippingOptionsXML()

                ''step through oShipping Options and add to oElmt those options 
                ''that are valid for our price and weight.
                Dim xmlShippingOptionsValid As XmlElement = moPageXml.CreateElement("ShippingOptions")


                Dim strXpath As New Text.StringBuilder
                strXpath.Append("Method[ ")
                strXpath.Append("(WeightMin=0 or WeightMin<=" & nWeight.ToString & ") ")
                strXpath.Append(" and ")
                strXpath.Append("(WeightMax=0 or WeightMax>=" & nWeight.ToString & ") ")
                strXpath.Append(" and ")
                strXpath.Append("(PriceMin=0 or PriceMin<=" & nPrice.ToString & ") ")
                strXpath.Append(" and ")
                strXpath.Append("(PriceMax=0 or PriceMax>=" & nPrice.ToString & ") ")
                strXpath.Append(" ]")


                'add filtered list to xmlShippingOptionsValid
                Dim xmlMethod As XmlElement
                For Each xmlMethod In xmlShippingOptions.SelectNodes(strXpath.ToString)
                    'add to 
                    xmlShippingOptionsValid.AppendChild(xmlMethod.CloneNode(True))
                Next

                'itterate though xmlShippingOptionsValid and get cheapest for each Location
                Dim cShippingLocation As String = ""
                Dim cShippingLocationPrev As String = ""

                For Each xmlMethod In xmlShippingOptionsValid.SelectNodes("Method")
                    Try
                        cShippingLocation = xmlMethod.SelectSingleNode("Location").InnerText
                        If cShippingLocationPrev <> "" And cShippingLocationPrev = cShippingLocation Then
                            xmlShippingOptionsValid.RemoveChild(xmlMethod)
                        End If

                        cShippingLocationPrev = cShippingLocation 'set cShippingLocationPrev for next loop
                    Catch ex As Exception
                        xmlMethod = xmlMethod
                        xmlProduct = xmlProduct

                        returnException(mcModuleName, "AddShippingCosts", ex, , "", gbDebug)
                    End Try

                Next

                'add to product XML
                xmlProduct.AppendChild(xmlShippingOptionsValid.CloneNode(True))

            Catch ex As Exception
                returnException(mcModuleName, "AddShippingCosts", ex, , "", gbDebug)

            End Try



        End Sub


        Public Function getValidShippingOptionsDS(cDestinationCountry As String, nAmount As Long, nQuantity As Long, nWeight As Long) As DataSet

            Try
                Dim sSql As String
                Dim sCountryList As String = getParentCountries(cDestinationCountry, 1)

                sSql = "select opt.*, dbo.fxn_shippingTotal(opt.nShipOptKey," & nAmount & "," & nQuantity & "," & nWeight & ") as nShippingTotal  from tblCartShippingLocations Loc "
                sSql = sSql & "Inner Join tblCartShippingRelations rel ON Loc.nLocationKey = rel.nShpLocId "
                sSql = sSql & "Inner Join tblCartShippingMethods opt ON rel.nShpOptId = opt.nShipOptKey "
                sSql &= "INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey"

                sSql = sSql & " WHERE (nShipOptQuantMin <= 0 or nShipOptQuantMin <= " & nQuantity & ") and (nShipOptQuantMax <= 0 or nShipOptQuantMax >= " & nQuantity & ") and "
                sSql = sSql & "(nShipOptPriceMin <= 0 or nShipOptPriceMin <= " & nAmount & ") and (nShipOptPriceMax <= 0 or nShipOptPriceMax >= " & nAmount & ") and "
                sSql = sSql & "(nShipOptWeightMin <= 0 or nShipOptWeightMin <= " & nWeight & ") and (nShipOptWeightMax <= 0 or nShipOptWeightMax >= " & nWeight & ") "

                sSql &= " and ((opt.cCurrency Is Null) or (opt.cCurrency = '') or (opt.cCurrency = '" & mcCurrency & "'))"
                If myWeb.mnUserId > 0 Then
                    ' if user in group then return it
                    sSql &= " and ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" &
                            " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" &
                            "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " & myWeb.mnUserId & " and perm.nPermLevel = 1) > 0"
                    sSql &= " and not((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" &
                            " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" &
                            "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " & myWeb.mnUserId & " and perm.nPermLevel = 0) > 0)"

                    'method allowed for authenticated or imporsonating CS users.
                    Dim shippingGroupCondition As String
                    Dim customerSuccessGroup = "Customer Services"
                    If myWeb.moSession("PreviewUser") > 0 And myWeb.moDbHelper.checkUserRole(customerSuccessGroup, "Group") Then
                        Dim gnCustomerServiceUsers = myWeb.GetUserXML.SelectSingleNode(String.Format("Group[@name='{0}']", customerSuccessGroup)).Attributes("id").Value
                        shippingGroupCondition = String.Format("perm.nDirId IN ({0},{1})", gnAuthUsers, gnCustomerServiceUsers)
                    Else
                        shippingGroupCondition = "perm.nDirId = " & gnAuthUsers
                    End If
                    sSql &= " Or (SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" &
                           "  where perm.nShippingMethodId = opt.nShipOptKey And " & shippingGroupCondition & " And perm.nPermLevel = 1) > 0"

                    ' if no group exists return it.
                    sSql &= " or (SELECT COUNT(*) from tblCartShippingPermission perm where opt.nShipOptKey = perm.nShippingMethodId and perm.nPermLevel = 1) = 0)"

                Else
                    Dim nonAuthID As Long = gnNonAuthUsers
                    Dim AuthID As Long = gnAuthUsers
                    'method allowed for non-authenticated
                    sSql &= " and ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" &
                           "  where perm.nShippingMethodId = opt.nShipOptKey and perm.nDirId = " & gnNonAuthUsers & "  and perm.nPermLevel = 1) > 0"
                    ' method has no group 
                    sSql &= " or (SELECT COUNT(*) from tblCartShippingPermission perm where opt.nShipOptKey = perm.nShippingMethodId and perm.nPermLevel = 1) = 0)"

                End If
                ' Restrict the shipping options by looking at the delivery country currently selected.  
                ' Of course, if we are hiding the delivery address then this can be ignored.

                If sCountryList <> "" Then
                    sSql = sSql & " and ((loc.cLocationNameShort IN " & sCountryList & ") or (loc.cLocationNameFull IN " & sCountryList & ")) "
                End If

                'Active methods

                sSql &= " AND (tblAudit.nStatus >0)"
                sSql &= " AND ((tblAudit.dPublishDate = 0) or (tblAudit.dPublishDate Is Null) or (tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & "))"
                sSql &= " AND ((tblAudit.dExpireDate = 0) or (tblAudit.dExpireDate Is Null) or (tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & "))"
                'Build Form

                'Go and collect the valid shipping options available for this order
                Return moDBHelper.GetDataSet(sSql & " order by opt.nDisplayPriority, nShippingTotal", "Option", "Shipping")
            Catch ex As Exception

                returnException(mcModuleName, "getValidShippingOptionsDS", ex, , "", gbDebug)

            End Try

        End Function

        Private Function makeShippingOptionsXML() As XmlElement

            Try


                If oShippingOptions Is Nothing Then

                    Dim xmlTemp As XmlElement

                    'create XML of all possible shipping methods and add it to the page XML
                    oShippingOptions = moPageXml.CreateElement("oShippingOptions")


                    'get all the shipping options for a given shipping weight and price
                    Dim strSql As New Text.StringBuilder
                    strSql.Append("SELECT opt.cShipOptCarrier as Carrier, opt.cShipOptTime AS ShippingTime, ")
                    strSql.Append("opt.nShipOptCost AS Cost, ")
                    strSql.Append("tblCartShippingLocations.cLocationNameShort as Location, tblCartShippingLocations.cLocationISOa2 as LocationISOa2, ")
                    strSql.Append("opt.nShipOptWeightMin AS WeightMin, opt.nShipOptWeightMax AS WeightMax,  ")
                    strSql.Append("opt.nShipOptPriceMin AS PriceMin, opt.nShipOptPriceMax AS PriceMax,  ")
                    strSql.Append("opt.nShipOptQuantMin AS QuantMin, opt.nShipOptQuantMax AS QuantMax, ")
                    strSql.Append("tblCartShippingLocations.nLocationType, tblCartShippingLocations.cLocationNameFull ")
                    strSql.Append("FROM tblCartShippingLocations ")
                    strSql.Append("INNER JOIN tblCartShippingRelations ON tblCartShippingLocations.nLocationKey = tblCartShippingRelations.nShpLocId ")
                    strSql.Append("RIGHT OUTER JOIN tblCartShippingMethods AS opt ")
                    strSql.Append("INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey ON tblCartShippingRelations.nShpOptId = opt.nShipOptKey ")

                    strSql.Append("WHERE (tblAudit.nStatus > 0) ")
                    strSql.Append("AND (tblAudit.dPublishDate = 0 OR tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & ") ")
                    strSql.Append("AND (tblAudit.dExpireDate = 0 OR tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & ") ")
                    strSql.Append("AND (tblCartShippingLocations.cLocationNameShort IS NOT NULL) ")
                    strSql.Append("ORDER BY tblCartShippingLocations.cLocationNameShort, opt.nShipOptCost ")



                    Dim oDs As DataSet = moDBHelper.GetDataSet(strSql.ToString, "Method", "ShippingMethods", )
                    oShippingOptions.InnerXml = oDs.GetXml()

                    'move all the shipping methods up a level
                    For Each xmlTemp In oShippingOptions.SelectNodes("ShippingMethods/Method")
                        oShippingOptions.AppendChild(xmlTemp)
                    Next

                    For Each xmlTemp In oShippingOptions.SelectNodes("ShippingMethods")
                        oShippingOptions.RemoveChild(xmlTemp)
                    Next

                End If

                Return oShippingOptions

            Catch ex As Exception
                returnException(mcModuleName, "makeShippingOptionsXML", ex, , "", gbDebug)

            End Try

        End Function

        Private Sub UpdatePackagingANdDeliveryType(ByVal mnCartId As Int32, ByVal ShippingKey As Int32)
            Dim strSql As String
            strSql = "SELECT count(*) as PackagingCount  from tblCartItem where cItemName='Evoucher' AND isNull(nParentId,0)<>0 and nCartOrderId=" & mnCartId
            Dim evoucherPackagingCount As Integer = Convert.ToInt32(moDBHelper.GetDataValue(strSql.ToString, CommandType.Text))

            strSql = "SELECT count(*) as ProductCount  from tblCartItem WHERE isNull(nParentId,0)=0 and nCartOrderId=" & mnCartId
            Dim productCount As Integer = Convert.ToInt32(moDBHelper.GetDataValue(strSql.ToString, CommandType.Text))


            'if all are evoucher set delivery option to evoucher
            If (evoucherPackagingCount = productCount) Then
                Dim update As String = updateGCgetValidShippingOptionsDS("64")
            ElseIf (ShippingKey = 64 And evoucherPackagingCount <> productCount) Then
                Dim update As String = updateGCgetValidShippingOptionsDS("66")
            ElseIf (ShippingKey <> 64 And evoucherPackagingCount = productCount) Then
                Dim update As String = updateGCgetValidShippingOptionsDS("64")
            End If
        End Sub


        Private Function updateGCgetValidShippingOptionsDS(ByVal nShipOptKey As String) As String
            Try

                Dim sSql2 As String
                Dim ods2 As DataSet
                Dim ods As DataSet
                Dim oRow As DataRow
                Dim sSql As String
                Dim cShippingDesc As String
                Dim nShippingCost As String
                Dim cSqlUpdate As String
                Dim ShippingName As String

                sSql = "select * from tblCartShippingMethods "
                sSql = sSql & " where nShipOptKey = " & nShipOptKey
                ods = moDBHelper.GetDataSet(sSql, "Order", "Cart")

                For Each oRow In ods.Tables("Order").Rows
                    cShippingDesc = oRow("cShipOptName") & "-" & oRow("cShipOptCarrier")
                    nShippingCost = oRow("nShipOptCost")
                    cSqlUpdate = "UPDATE tblCartOrder SET cShippingDesc='" & SqlFmt(cShippingDesc) & "', nShippingCost=" & SqlFmt(nShippingCost) & ", nShippingMethodId = " & nShipOptKey & " WHERE nCartOrderKey=" & mnCartId
                    moDBHelper.ExeProcessSql(cSqlUpdate)
                Next


                If (cShippingDesc = "Evoucher-UK Parcel") Then
                    Dim cSqlpkgopUpdate As String = "Update tblCartItem set cItemName='Evoucher', nPrice=0 WHERE isNull(nParentId,0)<>0 and nCartOrderId=" & mnCartId
                    moDBHelper.ExeProcessSql(cSqlpkgopUpdate)

                End If
                ' UpdatePackagingANdDeliveryType(mnCartId, nShipOptKey)

            Catch ex As Exception

                returnException(mcModuleName, "updateGCgetValidShippingOptionsDS", ex, , "", gbDebug)

            End Try
        End Function


        Private Sub AddProductOption(ByRef jObj As Newtonsoft.Json.Linq.JObject)

            Try
                Dim oelmt As XmlElement
                Dim cSqlUpdate As String
                Dim oItemInstance As XmlDataDocument = New XmlDataDocument
                oItemInstance.AppendChild(oItemInstance.CreateElement("instance"))
                oelmt = addNewTextNode("tblCartItem", oItemInstance.DocumentElement)

                Dim json As Newtonsoft.Json.Linq.JObject = jObj

                Dim CartItemId As Long = json.SelectToken("CartItemId")
                Dim ReplaceId As Long = json.SelectToken("ReplaceId")
                Dim OptionName As String = json.SelectToken("ItemName")
                Dim ShippingKey As Int32 = Convert.ToInt32(json.SelectToken("ShippingKey"))

                If (ReplaceId <> 0) Then
                    addNewTextNode("nCartItemKey", oelmt, CStr(ReplaceId))
                End If
                addNewTextNode("nCartOrderId", oelmt, CStr(mnCartId))
                addNewTextNode("nItemId", oelmt, json.SelectToken("ItemId"))
                addNewTextNode("cItemURL", oelmt, json.SelectToken("ItemURL")) 'Erm?
                addNewTextNode("cItemName", oelmt, OptionName)
                addNewTextNode("nItemOptGrpIdx", oelmt, json.SelectToken("ItemOptGrpIdx")) 'Dont Need
                addNewTextNode("nItemOptIdx", oelmt, json.SelectToken("ItemOptIdx")) 'Dont Need
                addNewTextNode("cItemRef", oelmt, json.SelectToken("ItemRef"))
                addNewTextNode("nPrice", oelmt, json.SelectToken("Price"))
                addNewTextNode("nShpCat", oelmt, json.SelectToken("ShpCat"))
                addNewTextNode("nDiscountCat", oelmt, json.SelectToken("DiscountCat"))
                addNewTextNode("nDiscountValue", oelmt, json.SelectToken("DiscountValue"))
                addNewTextNode("nTaxRate", oelmt, json.SelectToken("TaxRate"))
                addNewTextNode("nParentId", oelmt, CartItemId)
                addNewTextNode("cItemUnit", oelmt, json.SelectToken("TaxRate"))
                addNewTextNode("nQuantity", oelmt, json.SelectToken("Qunatity"))
                addNewTextNode("nweight", oelmt, json.SelectToken("Weight"))
                addNewTextNode("xItemXml", oelmt, json.SelectToken("ItemXml"))

                moDBHelper.setObjectInstance(Cms.dbHelper.objectTypes.CartItem, oItemInstance.DocumentElement)

                UpdatePackagingANdDeliveryType(mnCartId, ShippingKey)





            Catch ex As Exception

            End Try


        End Sub

        Public Function AddClientNotes(ByVal sNotesText As String) As String
            Dim cProcessInfo As String
            Dim sSql As String
            Dim oDs As DataSet
            Dim oRow As DataRow
            Dim sXmlContent As String
            Try
                'myCart.moCartXml
                If mnCartId > 0 Then
                    sSql = "select * from tblCartOrder where nCartOrderKey=" & mnCartId
                    oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart")
                    For Each oRow In oDs.Tables("Order").Rows
                        'load existing notes from Cart
                        sXmlContent = oRow("cClientNotes") & ""
                        If sXmlContent = "" Then
                            sXmlContent = "<Notes><Notes/><PromotionalCode/></Notes>"
                        End If
                        Dim NotesXml As New XmlDocument
                        NotesXml.LoadXml(sXmlContent)

                        If NotesXml.SelectSingleNode("Notes/Notes") Is Nothing Then
                            NotesXml.DocumentElement.AppendChild(NotesXml.CreateElement("Notes"))
                        End If

                        NotesXml.SelectSingleNode("Notes/Notes").InnerText = sNotesText

                        oRow("cClientNotes") = NotesXml.OuterXml
                    Next
                    myWeb.moDbHelper.updateDataset(oDs, "Order", True)
                    oDs.Clear()
                    oDs = Nothing

                    Return sNotesText
                Else

                    Return ""
                End If
            Catch ex As Exception
                returnException(mcModuleName, "AddDiscountCode", ex, "", cProcessInfo, gbDebug)
            End Try
        End Function



    End Class



End Class

